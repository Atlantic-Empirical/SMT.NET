Imports SMT.Hardware.COM.SerialPort

Namespace Multimedia.Hardware.JLCooper.MCS3

    ''' <summary>
    ''' WARNING: The MCSEvent returns on a separate thread.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class cMCS3

#Region "PROPERTIES"

        Public Event MCSEvent(ByVal evt As cMCS3Event)
        Public WriteOnly Property RecordLight() As Boolean
            Set(ByVal value As Boolean)
                If value Then
                    Port.PurgeBuffer(Rs232.PurgeBuffers.TxClear)
                    Port.Write(Chr(&H93))
                Else
                    Port.PurgeBuffer(Rs232.PurgeBuffers.TxClear)
                    Port.Write(Chr(&H83))
                End If
            End Set
        End Property

        Private WithEvents Port As Rs232

#End Region 'PROPERTIES

#Region "PUBLIC METHODS"

        Public Function Open(ByVal COMPortNumber As Byte) As Boolean
            Try
                Port = New Rs232
                With Port
                    .Port = COMPortNumber
                    .BaudRate = 38400
                    .DataBit = 8
                    .StopBit = Rs232.DataStopBit.StopBit_1
                    .Parity = Rs232.DataParity.Parity_None
                    .Timeout = 1000
                    .RxBufferThreshold = 2
                End With
                Port.Open()
                Port.Dtr = True
                Port.Rts = True
                Port.EnableEvents()
                RecordLight = False
                Return Port.IsOpen
            Catch Ex As Exception
                Debug.WriteLine("Problem with cMCS3 Open() " & Ex.Message)
                Return False
            End Try
        End Function

        Public Sub Close()
            Try
                Port.Close()
            Catch ex As Exception
                Throw New Exception("Problem with cMCS3 Close(). Error: " & ex.Message, ex)
            End Try
        End Sub

#End Region 'PUBLIC METHODS

#Region "PRIVATE METHODS"

        Private Sub CommEvent(ByVal source As Rs232, ByVal Mask As Rs232.EventMasks) Handles Port.CommEvent
            Try
                'Debug.WriteLine("moRS232_CommEvent()")
                Dim Buffer() As Byte, Offset As Byte

                If (Mask And Rs232.EventMasks.RxChar) > 0 Then
                    Buffer = source.InputStream
                    If Buffer.Length < 2 Then Throw New Exception("Buffer is less than two bytes.")

                    'DATA VALIDITY CHECK
                    ' Sometimes the 232 code gets mixed up and sends a loose byte first
                    If Buffer(0) < &H80 Or Buffer(0) > &H82 Then
                        'Debug.WriteLine("Corrupted data.")
                        'RaiseEvent MCSEvent(New cMCS3Event("Corrupted data"))
                        Offset = 1 'skip it
                        source.PurgeBuffer(Rs232.PurgeBuffers.RXClear)
                    Else
                        Offset = 0
                    End If

                    'UNEVEN BUFFER SIZE CHECK
                    ' If the user is pressing the buttons on the controller very fast
                    ' we might receive odd sized buffers.
                    ' Discard the last byte.
                    If Not SMT.DotNet.Utility.IsEven(Buffer.Length - Offset) Then
                        'Debug.WriteLine("Uneven buffer size. (" & Buffer.Length & ")")
                        Dim newbuf(Buffer.Length - 2) As Byte
                        [Array].Copy(Buffer, newbuf, Buffer.Length - 1)
                        Buffer = newbuf
                    End If

                    ''DEBUGGING
                    'Debug.WriteLine("Buffer length = " & Buffer.Length)
                    'Debug.Write("Bytes in moRS232_CommEvent() = ")
                    'For Each b As Byte In Buffer
                    '    Debug.Write(Hex(b) & " ")
                    'Next
                    'Debug.WriteLine("")
                    ''DEBUGGING

                    For i As Short = Offset To Buffer.Length - 1 Step 2
                        InterpretEvent(Buffer(i), Buffer(i + 1))
                    Next
                End If
            Catch ex As Exception
                Throw New Exception("Problem with CommEvent(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Private Sub InterpretEvent(ByVal b1 As Byte, ByVal b2 As Byte)
            Try
                'Debug.WriteLine("Bytes in EventHandler() = " & Hex(b1) & " " & Hex(b2))

                Dim ButtonAction As String = ""
                Dim Message As String
                Dim ctl As eMCSControls
                Dim Val As Short
                Dim Pressed As Boolean

                Select Case b1
                    Case &H80 'buttons
                        If (b2 And &H40) = &H40 Then ButtonAction = "Press"
                        If (b2 And &H40) = &H0 Then ButtonAction = "Release"

                        Pressed = (ButtonAction = "Press")

                        Select Case (b2 And &H3F)
                            Case &H0
                                Message = "Record " & ButtonAction
                                ctl = eMCSControls.Record
                                Val = 0
                            Case &H1
                                Message = "F6 " & ButtonAction
                                ctl = eMCSControls.F6
                                Val = 0
                            Case &H2
                                Message = "F4 " & ButtonAction
                                ctl = eMCSControls.F4
                                Val = 0
                            Case &H3
                                Message = "W7 " & ButtonAction
                                ctl = eMCSControls.W7
                                Val = 0
                            Case &H4
                                Message = "Play " & ButtonAction
                                ctl = eMCSControls.Play
                                Val = 0
                            Case &H5
                                Message = "Stop " & ButtonAction
                                ctl = eMCSControls.Stop
                                Val = 0
                            Case &H6
                                Message = "Fast Forward " & ButtonAction
                                ctl = eMCSControls.FastForward
                                Val = 0
                            Case &H7
                                Message = "Rewind " & ButtonAction
                                ctl = eMCSControls.Rewind
                                Val = 0
                            Case &H8
                                Message = "Down " & ButtonAction
                                ctl = eMCSControls.Down
                                Val = 0
                            Case &H9
                                Message = "Up " & ButtonAction
                                ctl = eMCSControls.Up
                                Val = 0
                            Case &HA
                                Message = "Left " & ButtonAction
                                ctl = eMCSControls.Left
                                Val = 0
                            Case &HB
                                Message = "Right " & ButtonAction
                                ctl = eMCSControls.Right
                                Val = 0
                            Case &HC
                                Message = "F3 " & ButtonAction
                                ctl = eMCSControls.F3
                                Val = 0
                            Case &HD
                                Message = "F2 " & ButtonAction
                                ctl = eMCSControls.F2
                                Val = 0
                            Case &HE
                                Message = "F1 " & ButtonAction
                                ctl = eMCSControls.F1
                                Val = 0
                            Case &HF
                                Message = "F5 " & ButtonAction
                                ctl = eMCSControls.F5
                                Val = 0
                            Case &H10
                                Message = "W1 " & ButtonAction
                                ctl = eMCSControls.W1
                                Val = 0
                            Case &H11
                                Message = "W2 " & ButtonAction
                                ctl = eMCSControls.W2
                                Val = 0
                            Case &H12
                                Message = "W3 " & ButtonAction
                                ctl = eMCSControls.W3
                                Val = 0
                            Case &H13
                                Message = "W4 " & ButtonAction
                                ctl = eMCSControls.W4
                                Val = 0
                            Case &H14
                                Message = "W5 " & ButtonAction
                                ctl = eMCSControls.W5
                                Val = 0
                            Case &H15
                                Message = "W6 " & ButtonAction
                                ctl = eMCSControls.W6
                                Val = 0
                            Case Else
                                Message = "Unknown Button Message"
                                ctl = eMCSControls.Unknown
                                Val = 0
                        End Select

                    Case &H81 'jog
                        Select Case b2
                            Case Is < &H40
                                Message = "Jog Forward " & b2
                                ctl = eMCSControls.Jog_Forward
                                Val = b2
                            Case Is >= &H40
                                Message = "Jog Reverse " & (Math.Abs(b2 - 252) - 124)
                                ctl = eMCSControls.Jog_Backward
                                Val = (Math.Abs(b2 - 252) - 124)
                            Case Else
                                Message = "Unknown Jog Message"
                                ctl = eMCSControls.Unknown
                                Val = 255
                        End Select

                    Case &H82 'shuttle
                        Select Case b2
                            Case Is = 0
                                Message = "Shuttle Center "
                                ctl = eMCSControls.Shuttle_Center
                                Val = 0
                            Case Is < &H40
                                Message = "Shuttle Forward " & b2
                                ctl = eMCSControls.Shuttle_Forward
                                Val = b2
                            Case Is >= &H40
                                Message = "Shuttle Reverse " & (Math.Abs(b2 - 252) - 124)
                                ctl = eMCSControls.Shuttle_Rewind
                                Val = (Math.Abs(b2 - 252) - 124)
                            Case Else
                                Message = "Unknown Shuttle Message"
                                ctl = eMCSControls.Unknown
                                Val = 255
                        End Select
                    Case Else
                        Message = "Unknown MCS3 Message"
                        ctl = eMCSControls.Unknown
                        Val = 255
                End Select

                Dim out As New cMCS3Event
                out.Control = ctl
                out.Message = Message
                out.Pressed = Pressed
                out.Value = Val

                RaiseEvent MCSEvent(out)

            Catch ex As Exception
                Throw New Exception("Problem with cMCS3 InterpretEvent(). Error: " & ex.Message, ex)
            End Try
        End Sub

#End Region 'PRIVATE METHODS

    End Class

End Namespace
