Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic
Imports SMT.DotNet.Utility
Imports SMT.Multimedia.Utility.Timecode

Namespace Multimedia.Filters.BlackMagic

    <ComVisible(True), ComImport(), Guid("72D62DE6-010F-48e6-A251-78CA285BDFE0"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IDecklinkRawDeviceControl

        'command
        '[in]  The raw command to be sent to the attached device. This buffer must include the calculated checksum as its final byte.

        'lenCommand
        '[in]  The length of command in bytes. This length must include the trailing checksum.

        'response
        '[in, out]  Application supplied buffer that upon return will hold the response from the attached device.

        'lenResponse
        '[in, out]  On entry, this argument specifies the length, in bytes, of the response buffer. Upon successful return, this value is updated with the actual length of the response.

        ''works for sending but not receving
        '<PreserveSig()> _
        'Function SendRawCommandSync( _
        '    ByVal command As IntPtr, _
        '    ByVal lenCommand As UInteger, _
        '    ByRef response As IntPtr, _
        '    ByRef lenResponse As UInteger) As Integer

        <PreserveSig()> _
        Function SendRawCommandSync( _
        ByVal command As IntPtr, _
        ByVal lenCommand As System.UInt32, _
        <Out()> ByVal response As IntPtr, _
        <Out()> ByRef lenResponse As UInt32) As Integer

        <PreserveSig()> _
        Function SendRawCommandAsync(ByVal args As DecklinkRawCommandAsync) As Integer

    End Interface

    Public Structure DecklinkRawCommandAsync
        Public command As Long 'PTR
        Public lenCommand As UInt32
        Public asyncResult As Integer 'HR
        Public response As Long
        Public lenResponse As UInt32
        Public commandComplete As Long 'HANDLE
        'the above data types may not be correct. The .Net sizes may differ from the original native sizes.
    End Structure

    Public Class cDeckControl

        Public Sub New(ByVal IDLVTR As IDecklinkRawDeviceControl)
            DLDC = IDLVTR
        End Sub

        Private DLDC As IDecklinkRawDeviceControl

#Region "Core Methods"

        Private Function SendSyncCmd(ByVal CMD As eCMDs, Optional ByVal Speed As Byte = 0) As Boolean
            Try
                '1) put cmd bytes into a buffer

                Dim cmd_buf() As Byte
                Dim str As String = Hex(CMD)

                If Speed = 0 Then
                    ReDim cmd_buf(2)
                    str = PadString(str, 4, 0, True)
                    Dim c1 As Byte = Byte.Parse(Left(str, 2), Globalization.NumberStyles.HexNumber)
                    Dim c2 As Byte = Byte.Parse(Right(str, 2), Globalization.NumberStyles.HexNumber)
                    cmd_buf(0) = c1
                    cmd_buf(1) = c2
                    cmd_buf(2) = c1 + c2
                Else
                    ReDim cmd_buf(3)
                    str = PadString(str, 4, 0, True)
                    Dim c1 As Byte = Byte.Parse(Left(str, 2), Globalization.NumberStyles.HexNumber)
                    Dim c2 As Byte = Byte.Parse(Right(str, 2), Globalization.NumberStyles.HexNumber)
                    cmd_buf(0) = c1 Or 1
                    cmd_buf(1) = c2
                    cmd_buf(2) = 118
                    cmd_buf(3) = cmd_buf(0) + cmd_buf(1) + cmd_buf(2)

                End If

                '2) get a pointer to that buffer
                Dim cmd_handle As GCHandle = GCHandle.Alloc(cmd_buf, GCHandleType.Pinned)
                Dim cmd_ptr As IntPtr = cmd_handle.AddrOfPinnedObject()

                '3) make call
                Dim rep_buf(2) As Byte
                Dim rep_handle As GCHandle = GCHandle.Alloc(rep_buf, GCHandleType.Pinned)
                Dim rep_ptr As IntPtr = rep_handle.AddrOfPinnedObject

                Dim replen As UInt32 = UInt32.Parse(3)
                Dim hr As Integer = DLDC.SendRawCommandSync(cmd_ptr, UInt32.Parse(cmd_buf.Length), rep_ptr, replen)
                Select Case hr
                    Case -2147023436
                        'ERROR_TIMEOUT
                        Return False
                    Case &H8007007A
                        'data passed to system call is too small
                        Debug.WriteLine("Unknown issue")
                        Marshal.ThrowExceptionForHR(hr)
                        Return True
                    Case Is < 0
                        Marshal.ThrowExceptionForHR(hr)
                End Select

                '4) Analyse response
                Debug.WriteLine(replen)

                'Check Sum
                Dim chksum As Short = 0
                For s As Short = 0 To (UBound(rep_buf) - 1)
                    chksum += rep_buf(s)
                Next
                If Not chksum = rep_buf(UBound(rep_buf)) And Not replen.Equals(0) Then
                    Throw New Exception("Bad response check sum or length.")
                    Return False
                End If

                'NAK?
                If rep_buf(0) = &H11 And rep_buf(1) = &H12 Then
                    'VTR doesn't like the command
                    Return False
                End If

                'ACK?
                If rep_buf(0) = &H10 And rep_buf(1) = &H1 Then
                    'Command ok, no return data
                    Return True
                End If

                Return True
            Catch ex As Exception
                Debug.WriteLine("Problem with SendCmd. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Private Function SendAsyncCmd(ByVal CMD As eCMDs) As Boolean
            Try
                ''1) put cmd bytes into a buffer
                'Dim buf(1) As Byte
                'Dim s As String = Hex(CMD)
                's = PadString(s, 4, 0, True)
                'Dim c1 As Byte = Byte.Parse(Left(s, 2), Globalization.NumberStyles.HexNumber)
                'Dim c2 As Byte = Byte.Parse(Right(s, 2), Globalization.NumberStyles.HexNumber)
                'buf(0) = c1
                'buf(1) = c2

                ''2) get a pointer to that buffer
                'Dim handle As GCHandle = GCHandle.Alloc(buf, GCHandleType.Pinned)
                'Dim bufptr As IntPtr = handle.AddrOfPinnedObject()

                ''3) make call
                'Dim repptr As IntPtr
                'Dim replen As UInteger
                'Dim hr As Integer = DL_VTR.SendRawCommandSync(bufptr, 2, repptr, replen)
                'Select Case hr
                '    Case -2147023436
                '        'ERROR_TIMEOUT
                '        Return False
                '    Case Is < 0
                '        Marshal.ThrowExceptionForHR(hr)
                'End Select

                ''4) analyse response
                'Debug.WriteLine(replen)

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with SendCmd. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Private Sub AsyncCallback()

        End Sub

        Private Enum eCMDs
            Local_Disable = &HC         '10 01 Ack  
            Device_Type_Request = &H11  '12 11 Device Type  
            Local_Enable = &H1D         '10 01 Ack  
            [Stop] = &H2000             '10 01 Ack  
            Play = &H2001               '10 01 Ack  
            Record = &H2002             '10 01 Ack  
            Standby_Off = &H2004        '10 01 Ack  
            Standby_On = &H2005         '10 01 Ack  
            Eject = &H200F              '10 01 Ack  
            Fast_Fwd = &H2010           '10 01 Ack  
            Jog_Fwd = &H2011            '10 01 Ack  
            Var_Fwd = &H2012            '10 01 Ack  
            Shuttle_Fwd = &H2013        '10 01 Ack  
            Rewind = &H2020             '10 01 Ack  
            Jog_Rev = &H2021            '10 01 Ack  
            Var_Rev = &H2022            '10 01 Ack  
            Shuttle_Rev = &H2023        '10 01 Ack  
            Preroll = &H2030            '10 01 Ack  
            CueUpWithData = &H2431      '10 01 Ack  
            Sync_Play = &H2034          '10 01 Ack  
            Prog_Speed_Play_Pos = &H2138 '10 01 Ack  
            Prog_Speed_Play_Neg = &H2139 '10 01 Ack  
            Preview = &H2040            '10 01 Ack  
            Review = &H2041             '10 01 Ack  
            AutoEdit = &H2042           '1001Ack
            OutpointPreview = &H2043    '1001Ack
            AntiClogTimerDisable = &H2054 '1001Ack
            AntiClogTimerEnable = &H2055  '1001Ack
            FullEEOff = &H2060          '1001Ack
            FullEEOn = &H2061           '1001Ack
            SelectEEOn = &H2063         '1001Ack
            EditOff = &H2064            '1001Ack
            EditOn = &H2065             '1001Ack
            FreezeOff = &H206A          '1001Ack
            FreezeOn = &H206B           '1001Ack
            Timer1Preset = &H4400       '1001Ack
            TimeCodePreset = &H4404     '1001Ack
            UserBitPreset = &H4405      '1001Ack
            Timer1Reset = &H4008        '1001Ack
            InEntry = &H4010            '1001Ack
            OutEntry = &H4011           '1001Ack
            AudioInEntry = &H4012       '1001Ack
            AudioOutEntry = &H4013      '1001Ack
            InDataPreset = &H4414       '1001Ack
            OutDataPreset = &H4415      '1001Ack
            AudioInDataPreset = &H4416  '1001Ack
            AudioOutDataPreset = &H4417 '1001Ack
            InPlusShift = &H4018        '1001Ack
            InNegShift = &H4019         '1001Ack
            OutPlusShift = &H401A       '1001Ack
            OutNegShift = &H401B        '1001Ack
            AudioInPlusShift = &H401C   '1001Ack
            AudioInNegShift = &H401D    '1001Ack
            AudioOutPlusShift = &H401E  '1001Ack
            AudioOutNegShift = &H401F   '1001Ack
            InFlagReset = &H4020        '1001Ack
            OutFlagReset = &H4021       '1001Ack
            AudioInFlagReset = &H4022   '1001Ack
            AudioOutFlagReset = &H4023  '1001Ack
            InRecall = &H4024           '1001Ack
            OutRecall = &H4025          '1001Ack
            AudioInRecall = &H4026      '1001Ack
            AudioOutRecall = &H4027     '1001Ack
            LostLockReset = &H402D      '1001Ack
            EditPreset = &H4030         '1001Ack
            Prerolltimepreset = &H4431  '1001Ack
            TapeAutoSelect = &H4132     '1001Ack
            ServoRefSelect = &H4133     '1001Ack
            HeadSelect = &H4134         '1001Ack
            ColorFrameselect = &H4135   '1001Ack
            TimerModeSelect = &H4136    '1001Ack
            InputCheck = &H4137         '1001Ack
            EditFieldSelect = &H413A    '1001Ack
            FreezeModeSelect = &H413B   '1001Ack
            RecordInhibit = &H403E      '1001Ack
            AutoModeOff = &H4040        '1001Ack
            AutoModeOn = &H4041         '1001Ack
            SpotEraseOff = &H4042       '1001Ack
            SpotEraseOn = &H4043        '1001Ack
            AudioSplitOff = &H4044      '1001Ack
            AudioSplitOn = &H4045       '1001Ack
            OutputHPhase = &H4098       '1001Ack
            OutputVideoPhase = &H409B   '1001Ack
            AudioInputLevel = &H40A0    '1001Ack
            AudioOutputLevel = &H40A1   '1001Ack
            AudioAdvLevel = &H40A2      '1001Ack
            AudioOutputPhase = &H40A8   '1001Ack
            AudioAdvOutPhase = &H40A9   '1001Ack
            CrossFadeTimePreset = &H40AA    '1001Ack
            LocalKeyMap = &H40B8        '1001Ack
            StillOfftime = &H42F8       '1001Ack
            StbyOfftime = &H42FA        '1001Ack
            TCGenSense = &H610A         '7408GenTimeData
            '                           79 09 Gen User Bits Data  
            '                           74 00 Timer-1 Data  
            '                           74 01 Timer-2 Data  
            '                           74 04  LTC Time Data  
            '                           74 05  User Bits (LTC) Data  
            CurrentTimeSense = &H610C        '7406VITCTimeData
            '                           74 07 User Bits (VITC) Data  
            '                           74 14  Corrected LTC Time Data  
            '                           74 15 Hold User Bits (LTC) Data  
            '                           74 16  Hold VITC Time Data  
            '                           74 17  Hold User Bits (VITC) Data  
            InDataSense = &H6010        '7410InData
            OutDataSense = &H6011       '7411OutData
            AudioInDataSense = &H6012   '7412AudioInData
            AudioOutDataSense = &H6013  '7413AudioOutData
            StatusSense = &H6120        '7X20StatusData
            ExtendedVTRStatus = &H6121  '7X21ExtendedStatusData
            SignalControlSense = &H6223 '7X23SignalControlData
            LocalKeyMapSense = &H6028   '7X28LocalKeyMap
            HeadMeterSense = &H612A     '7X2AHeadMeterData
            RemainingTimeSense = &H602B '762BRemainingTime
            CmdSpeedSense = &H602E      '7X2ECmdSpeedData
            EditPresetSense = &H6130    '7X30EditPresetStatus
            PrerollTimeSense = &H6031   '7431PrerollTime
            TimerModeSense = &H6036     '7136TimerModeStatus
            RecordInhibitSense = &H603E '723ERecordInhibitStatus
            DAInpEmphSense = &H6052     '7152DAInputEmphasisData
            DAPBEmphSense = &H6053      '7153DAPlaybackEmphasisData
            DASampFreqSense = &H6058    '7158DASamplingFrequencyData
            CrossFadeTimeSense = &H61AA '7XAACrossFadeTimeData
        End Enum

        Public Function EncodeTimecodeFor422(ByVal Time As Byte) As Byte
            Try
                Dim Tens As Byte = 0
                If Time >= 10 Then
                    Tens = Math.Truncate(Time / 10)
                End If
                Dim Ones As Integer
                Math.DivRem(Time, 10, Ones)
                Dim out As Byte = Tens << 4
                out = out Or Ones
                Return out
            Catch ex As Exception
                Throw New Exception("Problem with PrepTimeFor422. Error: " & ex.Message)
            End Try
        End Function

        Public Function DecodeTimecodeFor422(ByVal Time As Byte) As Byte
            Try
                Dim Tens As Byte = (Time >> 4) * 10
                Dim Ones As Byte = Time And 15
                Return Tens + Ones
            Catch ex As Exception
                Throw New Exception("Problem with PrepTimeFor422. Error: " & ex.Message)
            End Try
        End Function

        Public Function CueData(ByVal Hours As Byte, ByVal Minutes As Byte, ByVal Seconds As Byte, ByVal Frames As Byte) As Boolean
            Try
                Dim cmd_buf(6) As Byte
                Dim str As String = Hex(eCMDs.CueUpWithData)
                str = PadString(str, 4, 0, True)
                Dim c1 As Byte = Byte.Parse(Left(str, 2), Globalization.NumberStyles.HexNumber)
                Dim c2 As Byte = Byte.Parse(Right(str, 2), Globalization.NumberStyles.HexNumber)
                cmd_buf(0) = c1
                cmd_buf(1) = c2
                cmd_buf(2) = EncodeTimecodeFor422(Frames)
                cmd_buf(3) = EncodeTimecodeFor422(Seconds)
                cmd_buf(4) = EncodeTimecodeFor422(Minutes)
                cmd_buf(5) = EncodeTimecodeFor422(Hours)
                'cmd_buf(6) = cmd_buf(0) + cmd_buf(1) + cmd_buf(2) + cmd_buf(3) + cmd_buf(4) + cmd_buf(5)
                Dim cs As Integer
                For s As Short = 0 To 5
                    cs += cmd_buf(s)
                Next
                cmd_buf(6) = cs And 15

                '2) get a pointer to that buffer
                Dim cmd_handle As GCHandle = GCHandle.Alloc(cmd_buf, GCHandleType.Pinned)
                Dim cmd_ptr As IntPtr = cmd_handle.AddrOfPinnedObject()

                '3) make call
                Dim rep_buf(30000) As Byte
                Dim rep_handle As GCHandle = GCHandle.Alloc(rep_buf, GCHandleType.Pinned)
                Dim rep_ptr As IntPtr = rep_handle.AddrOfPinnedObject

                Dim replen As UInt32
                Dim hr As Integer = DLDC.SendRawCommandSync(cmd_ptr, UInt32.Parse(cmd_buf.Length), rep_ptr, replen)
                Select Case hr
                    Case -2147023436
                        'ERROR_TIMEOUT
                        Return False
                    Case &H8007007A
                        'data passed to system call is too small
                        Debug.WriteLine("Unknown issue")
                        Marshal.ThrowExceptionForHR(hr)
                        Return True
                    Case Is < 0
                        Marshal.ThrowExceptionForHR(hr)
                End Select

                '4) Analyse response
                Debug.WriteLine(replen)

                'Check Sum
                Dim chksum As Short = 0
                For s As Short = 0 To (UBound(rep_buf) - 1)
                    chksum += rep_buf(s)
                Next
                If Not chksum = rep_buf(UBound(rep_buf)) And Not replen.Equals(0) Then
                    Throw New Exception("Bad response check sum or length.")
                    Return False
                End If

                'NAK?
                If rep_buf(0) = &H11 And rep_buf(1) = &H12 Then
                    'VTR doesn't like the command
                    Return False
                End If

                'ACK?
                If rep_buf(0) = &H10 And rep_buf(1) = &H1 Then
                    'Command ok, no return data
                    Return True
                End If

                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        Public Function GetDeckTimecode(ByRef TC As cTimecode) As Boolean
            Try
                Dim cmd_buf(3) As Byte
                Dim str As String = Hex(eCMDs.CurrentTimeSense)
                str = PadString(str, 4, 0, True)
                Dim c1 As Byte = Byte.Parse(Left(str, 2), Globalization.NumberStyles.HexNumber)
                Dim c2 As Byte = Byte.Parse(Right(str, 2), Globalization.NumberStyles.HexNumber)
                cmd_buf(0) = c1
                cmd_buf(1) = c2
                cmd_buf(2) = 3
                cmd_buf(3) = cmd_buf(0) + cmd_buf(1) + cmd_buf(2)

                '2) get a pointer to that buffer
                Dim cmd_handle As GCHandle = GCHandle.Alloc(cmd_buf, GCHandleType.Pinned)
                Dim cmd_ptr As IntPtr = cmd_handle.AddrOfPinnedObject()

                '3) make call
                Dim rep_buf(6) As Byte
                Dim rep_handle As GCHandle = GCHandle.Alloc(rep_buf, GCHandleType.Pinned)
                Dim rep_ptr As IntPtr = rep_handle.AddrOfPinnedObject

                Dim replen As UInt32 = UInt32.Parse(7)
                Dim hr As Integer = DLDC.SendRawCommandSync(cmd_ptr, UInt32.Parse(cmd_buf.Length), rep_ptr, replen)
                Select Case hr
                    Case -2147023436
                        'ERROR_TIMEOUT
                        Return False
                    Case &H8007007A
                        'data passed to system call is too small
                        Debug.WriteLine("Unknown issue")
                        Marshal.ThrowExceptionForHR(hr)
                        Return True
                    Case Is < 0
                        Marshal.ThrowExceptionForHR(hr)
                End Select

                '4) Analyse response
                'Debug.WriteLine(replen)

                ''Check Sum
                'Dim chksum As Short = 0
                'For s As Short = 0 To (UBound(rep_buf) - 1)
                '    chksum += rep_buf(s)
                'Next
                'If Not chksum = rep_buf(UBound(rep_buf)) And Not replen = 0 Then
                '    Debug.WriteLine("Bad response check sum or length in get tape tc.")
                '    Return False
                'End If

                'NAK?
                If rep_buf(0) = &H11 And rep_buf(1) = &H12 Then
                    'VTR doesn't like the command
                    Return False
                End If

                'ACK?
                If rep_buf(0) = &H10 And rep_buf(1) = &H1 Then
                    'Command ok, no return data
                    Return True
                End If

                TC.Hours = DecodeTimecodeFor422(rep_buf(5))
                TC.Minutes = DecodeTimecodeFor422(rep_buf(4))
                TC.Seconds = DecodeTimecodeFor422(rep_buf(3))
                TC.Frames = DecodeTimecodeFor422(rep_buf(2))

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with GetDeckTimecode. Error: " & ex.Message)
                Return Nothing
            End Try
        End Function

#End Region 'Core Methods

#Region "Implementation Methods"

        Public Function Var(ByVal Forward As Boolean, ByVal Speed As Byte) As Boolean
            Try
                If Forward Then
                    Return Me.SendSyncCmd(eCMDs.Var_Fwd, Speed)
                Else
                    Return Me.SendSyncCmd(eCMDs.Var_Rev, Speed)
                End If
            Catch ex As Exception
                Throw New Exception("Problem with DeviceControl Jog. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Function Eject() As Boolean
            Try
                Return Me.SendSyncCmd(eCMDs.Eject)
            Catch ex As Exception
                Throw New Exception("Problem with DeviceControl Eject. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Function Jog(ByVal Forward As Boolean) As Boolean
            Try
                If Forward Then
                    Return Me.SendSyncCmd(eCMDs.Jog_Fwd)
                Else
                    Return Me.SendSyncCmd(eCMDs.Jog_Rev)
                End If
            Catch ex As Exception
                Throw New Exception("Problem with DeviceControl Jog. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Function Shuttle(ByVal Forward As Boolean, ByVal Speed As Byte) As Boolean
            Try
                If Forward Then
                    Return Me.SendSyncCmd(eCMDs.Shuttle_Fwd, Speed)
                Else
                    Return Me.SendSyncCmd(eCMDs.Shuttle_Rev, Speed)
                End If
            Catch ex As Exception
                Throw New Exception("Problem with DeviceControl Shuttle. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Function [Stop]() As Boolean
            Try
                Return Me.SendSyncCmd(eCMDs.Stop)
            Catch ex As Exception
                Throw New Exception("Problem with DeviceControl Stop. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Function Play() As Boolean
            Try
                Return Me.SendSyncCmd(eCMDs.Play)
            Catch ex As Exception
                Throw New Exception("Problem with DeviceControl Play. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Function FastForward() As Boolean
            Try
                Return Me.SendSyncCmd(eCMDs.Fast_Fwd)
            Catch ex As Exception
                Throw New Exception("Problem with DeviceControl FastForward. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Function Rewind() As Boolean
            Try
                Return Me.SendSyncCmd(eCMDs.Rewind)
            Catch ex As Exception
                Throw New Exception("Problem with DeviceControl Shuttle. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Function Freeze(ByVal [On] As Boolean) As Boolean
            Try
                If [On] Then
                    Return Me.SendSyncCmd(eCMDs.FreezeOn)
                Else
                    Return Me.SendSyncCmd(eCMDs.FullEEOff)
                End If
            Catch ex As Exception
                Throw New Exception("Problem with DeviceControl Shuttle. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Function Record() As Boolean
            Try
                Return Me.SendSyncCmd(eCMDs.Record)
            Catch ex As Exception
                Throw New Exception("Problem with DeviceControl Shuttle. Error: " & ex.Message, ex)
            End Try
        End Function

#End Region 'Implementation Methods

    End Class

End Namespace
