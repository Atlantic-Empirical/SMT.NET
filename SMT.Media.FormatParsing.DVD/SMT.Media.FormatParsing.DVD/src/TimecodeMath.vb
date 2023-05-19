Imports System.Text
Imports SMT.Media.FormatParsing.DVD.Utilities.ConversionsAndSuch

Namespace Media.Utilities.TimecodeMath

    Public Module mTimecodeMath

        Public Function SubtractTimecode(ByVal TC1 As cTimecode, ByVal TC2 As cTimecode, ByVal FrameRate As Byte) As cTimecode
            'Dim Out As New cTimecode
            'With Out
            '    .Framerate = TC1.Framerate
            '    .Frames = TC1.Frames
            '    .Hours = TC1.Hours
            '    .Minutes = TC1.Minutes
            '    .Seconds = TC1.Seconds
            'End With

            TC1.Hours -= TC2.Hours
            TC1.Minutes -= TC2.Minutes
            TC1.Seconds -= TC2.Seconds
            TC1.Frames -= TC2.Frames

            If TC1.Hours < 0 Then Return New cTimecode(0, 0, 0, 0, FrameRate)
            If TC1.Hours = 0 And TC1.Minutes < 0 Then Return New cTimecode(0, 0, 0, 0, FrameRate)
            If TC1.Hours = 0 And TC1.Minutes = 0 And TC1.Seconds < 0 Then Return New cTimecode(0, 0, 0, 0, FrameRate)
            If TC1.Hours = 0 And TC1.Minutes = 0 And TC1.Seconds = 0 And TC1.Frames < 0 Then Return New cTimecode(0, 0, 0, 0, FrameRate)

            'ok, now we know that ther's a positive value time code to be had

            If TC1.Frames < 0 Then
                If TC1.Seconds > 0 Then
                    TC1.Seconds -= 1
                    TC1.Frames += FrameRate
                Else
                    If TC1.Minutes > 0 Then
                        TC1.Minutes -= 1
                        TC1.Seconds += 59
                        TC1.Frames += FrameRate
                    Else
                        TC1.Hours -= 1
                            TC1.Minutes += 59
                            TC1.Seconds += 59
                            TC1.Frames += FrameRate
                    End If
                End If
            End If
            'ok now we should have good frames

            If TC1.Seconds < 0 Then
                If TC1.Minutes > 0 Then
                    TC1.Minutes -= 1
                    TC1.Seconds += 59
                    TC1.Frames += FrameRate
                Else
                    TC1.Hours -= 1
                        TC1.Minutes += 59
                        TC1.Seconds += 59
                        TC1.Frames += FrameRate
                End If
            End If
            'ok, now we have good seconds

            If TC1.Minutes < 0 Then
                TC1.Hours -= 1
                TC1.Minutes += 59
                TC1.Seconds += 59
                TC1.Frames += FrameRate
            End If
            'now we have good minutes

            'hours should be good too

            If TC1.Frames >= TC1.Framerate Then
                TC1.Seconds += 1
                TC1.Frames -= TC1.Framerate
            End If
            If TC1.Seconds >= 60 Then
                TC1.Minutes += 1
                TC1.Seconds -= 60
            End If
            If TC1.Minutes >= 60 Then
                TC1.Hours += 1
                TC1.Minutes -= 60
            End If
            If TC1.Frames >= 30 Then
                Debug.WriteLine("hi")
            End If
            Return TC1

            'Out.Frames -= TC2.Frames
            'If Out.Frames < 0 Then
            '    If Out.Seconds > 0 Then
            '        Out.Seconds -= 1
            '        Out.Frames += FrameRate
            '    Else
            '        If Out.Minutes > 0 Then
            '            Out.Minutes -= 1
            '            Out.Seconds += 59
            '            Out.Frames += FrameRate
            '        Else
            '            If Out.Hours > 0 Then
            '                Out.Hours -= 1
            '                Out.Minutes += 59
            '                Out.Seconds += 59
            '                Out.Frames += FrameRate
            '            Else
            '                Out.Hours = 0
            '                Out.Minutes = 0
            '                Out.Seconds = 0
            '                Out.Frames = 0
            '                Return Out
            '            End If
            '        End If
            '    End If
            'End If

            'Out.Seconds -= TC2.Seconds
            'If Out.Seconds < 0 Then
            '    If Out.Minutes > 0 Then
            '        Out.Minutes -= 1
            '        Out.Seconds += 60
            '    Else
            '        If Out.Hours > 0 Then
            '            Out.Hours -= 1
            '            Out.Minutes += 59
            '            Out.Seconds += 60
            '        End If
            '    End If
            'End If

            'Out.Minutes -= TC2.Minutes
            'If Out.Minutes < 0 Then
            '    Out.Hours -= 1
            '    Out.Minutes += 60
            'End If

            'Out.Hours -= TC2.Hours
            'Return Out
        End Function

        Public Function AddTimecode(ByVal TC1 As cTimecode, ByVal TC2 As cTimecode, ByVal FrameRate As Byte) As cTimecode
            TC1.Hours += TC2.Hours
            TC1.Minutes += TC2.Minutes
            TC1.Seconds += TC2.Seconds
            TC1.Frames += TC2.Frames

            If TC1.Frames > FrameRate Then
                TC1.Frames = TC1.Frames - FrameRate
                TC1.Seconds += 1
            End If

            If TC1.Seconds > 59 Then
                TC1.Seconds = TC1.Seconds - 60
                TC1.Minutes += 1
            End If

            If TC1.Minutes > 59 Then
                TC1.Minutes = TC1.Minutes - 60
                TC1.Hours += 1
            End If

            Return TC1

            'Dim Out As New cTimecode
            'Out.Framerate = TC1.Framerate

            'Out.Frames = TC1.Frames + TC2.Frames
            'If Out.Frames > 29 Then
            '    Out.Seconds = 1
            '    Out.Frames -= 30
            'End If

            'Out.Seconds += TC1.Seconds + TC2.Seconds
            'If Out.Seconds > 59 Then
            '    Out.Minutes = 1
            '    Out.Seconds -= 60
            'End If

            'Out.Minutes += TC1.Minutes + TC2.Minutes
            'If Out.Minutes > 59 Then
            '    Out.Hours = 1
            '    Out.Minutes -= 60
            'End If

            'Out.Hours += TC1.Hours + TC2.Hours

            'Return Out
        End Function

        Public Function CompareDVDTimecodes(ByVal TC1 As DvdTimeCode, ByVal TC2 As DvdTimeCode) As Byte
            Try
                'return which is greater
                If TC1.bHours > TC2.bHours Then
                    Return 1
                Else
                    Return 2
                End If

                If TC1.bMinutes > TC2.bMinutes Then
                    Return 1
                Else
                    Return 2
                End If

                If TC1.bSeconds > TC2.bSeconds Then
                    Return 1
                Else
                    Return 2
                End If

                If TC1.bFrames > TC2.bFrames Then
                    Return 1
                Else
                    Return 2
                End If

                'they're equal
                Return 0
            Catch ex As Exception
                MsgBox("CompareDVDTimecodes failed. Error: " & ex.Message)
            End Try
        End Function

        Public Function DVDPlayLocToString(ByVal PL As DvdPlayLocation) As String
            Try
                If PL.TitleNum = Nothing Then Return "N/A"
                If PL.TitleNum = -1 Then Return "Non-Title Domain"
                Dim sb As New StringBuilder
                sb.Append("TT: " & PL.TitleNum & " ")
                sb.Append("PTT: " & PL.ChapterNum & " ")
                sb.Append("TC: " & DVDTimeCodeToString(PL.timeCode))
                Return sb.ToString
            Catch ex As Exception
                Debug.WriteLine("Problem with DVDPlayLocToString. Error: " & ex.Message)
            End Try
        End Function

        Public Function DVDTimeCodeToString(ByVal TC As DvdTimeCode) As String
            Try
                'If TC Is Nothing Then Return ""
                Dim SB As New StringBuilder
                SB.Append(PadString(TC.bHours, 2, "0", True) & ":")
                SB.Append(PadString(TC.bMinutes, 2, "0", True) & ":")
                SB.Append(PadString(TC.bSeconds, 2, "0", True) & ";")
                SB.Append(PadString(TC.bFrames, 2, "0", True))
                Return SB.ToString
            Catch ex As Exception
                Debug.WriteLine("Problem with DVDPlayLocToString. Error: " & ex.Message)
            End Try
        End Function

        Public Function DVDTimecodeToFrameCount(ByVal TC As DvdTimeCode, ByVal VidStd As String) As Long
            Try
                Dim FrameRate As Short
                If VidStd.ToLower = "ntsc" Then
                    FrameRate = 30
                Else
                    FrameRate = 25
                End If
                Dim Out As Long = TC.bHours * 3600 * FrameRate
                Out += TC.bMinutes * 60 * FrameRate
                Out += TC.bSeconds * FrameRate
                Out += TC.bFrames
                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with DVDTimecodeToFrameCount. Error: " & ex.Message)
            End Try
        End Function

        Public Function DVDTimecodeToSeconds(ByVal DVDTC As DvdTimeCode) As UInt32
            Try
                Dim out As UInt32 = 0
                out += DVDTC.bHours * 3600
                out += DVDTC.bMinutes * 60
                out += DVDTC.bSeconds
                Return out
            Catch ex As Exception
                Throw New Exception("Problem with DVDTimecodeToSeconds(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function FrameCountToDVDTimecode(ByVal FrameCount As Long, ByVal VidStd As String) As DvdTimeCode
            Try
                Dim FrameRate As Byte
                If VidStd.ToLower = "ntsc" Then
                    FrameRate = 30
                Else
                    FrameRate = 25
                End If
                Dim out As DvdTimeCode
                out.bHours = FrameCount \ (3600 * FrameRate)
                out.bMinutes = (FrameCount Mod (3600 * FrameRate)) \ (60 * FrameRate)
                out.bSeconds = (FrameCount Mod (60 * FrameRate)) \ FrameRate
                out.bFrames = FrameCount Mod FrameRate
                Return out
            Catch ex As Exception
                Throw New Exception("Problem with DVDTimecodeToFrameCount. Error: " & ex.Message)
            End Try
        End Function

        Public Function PercentageToTimecode(ByVal Per As Byte, ByVal TotalFrameCount As Long, ByVal VidStd As String) As DvdTimeCode
            Try
                Dim TargLocInFrames As Long = Math.Round(TotalFrameCount * (Per / 100), 0)
                Dim TargDVDLoc As DvdTimeCode = FrameCountToDVDTimecode(TargLocInFrames, VidStd)
                Return TargDVDLoc
            Catch ex As Exception
                Throw New Exception("Problem with PercentageToTimeCode. Error: " & ex.Message)
            End Try
        End Function

        Public Function TimecodeToPercentage(ByVal TC As DvdTimeCode, ByVal TotalFrameCount As Long, ByVal VidStd As String) As Byte
            Try
                Dim TTCC As Long = DVDTimecodeToFrameCount(TC, VidStd)
                'Debug.WriteLine("ttcc= " & TTCC)
                'Debug.WriteLine("totalframecount= " & TotalFrameCount)
                Dim Per As Long = Math.Round((TTCC / TotalFrameCount) * 100, 0)
                'Debug.WriteLine("per= " & Per)
                If Per > 100 Then Return 100
                Return Per
            Catch ex As Exception
                Throw New Exception("Problem with TimecodeToPercentage. Error: " & ex.Message)
            End Try
        End Function

    End Module

    Public Class cTimecode

        Public Hours As Short
        Public Minutes As Short
        Public Seconds As Short
        Public Frames As Short
        Public Framerate As eFramerate

        Public Overloads Overrides Function ToString() As String
            Dim oH, oM, oS, [oF] As String
            oH = Hours
            oM = Minutes
            oS = Seconds
            [oF] = Frames
            If oH.Length = 1 Then oH = "0" & oH
            If oM.Length = 1 Then oM = "0" & oM
            If oS.Length = 1 Then oS = "0" & oS
            If [oF].Length = 1 Then [oF] = "0" & [oF]

            Return oH & ":" & oM & ":" & oS & "." & [oF]

        End Function

        Public Function ToString_NoFrames() As String
            Dim oH, oM, oS, [oF] As String
            oH = Hours
            oM = Minutes
            oS = Seconds
            If oM.Length = 1 Then oM = "0" & oM
            If oS.Length = 1 Then oS = "0" & oS
            Return oH & ":" & oM & ":" & oS
        End Function

        Public Function ToString_WithFrameRate()
            If Not Framerate = Nothing Then
                Return ToString() & " / " & Framerate.ToString
            Else
                Return ToString()
            End If
        End Function

        Public Sub New()
        End Sub

        Public Sub New(ByVal nTC As DvdTimeCode, ByVal NTSC As Boolean)
            Me.Hours = nTC.bHours
            Me.Minutes = nTC.bMinutes
            Me.Seconds = nTC.bSeconds
            Me.Frames = nTC.bFrames
            If NTSC Then
                Me.Framerate = eFramerate.NTSC
            Else
                Me.Framerate = eFramerate.PAL
            End If
        End Sub

        Public Sub New(ByVal nHours As Byte, ByVal nMinutes As Byte, ByVal nSeconds As Byte, ByVal nFrames As Byte, ByVal NTSC As Boolean)
            Me.Hours = nHours
            Me.Minutes = nMinutes
            Me.Seconds = nSeconds
            Me.Frames = nFrames
            If NTSC Then
                Me.Framerate = eFramerate.NTSC
            Else
                Me.Framerate = eFramerate.PAL
            End If
        End Sub

        Public Sub New(ByRef C_PBTM() As Byte, ByVal Offset As Long)
            Try
                Dim h10, h1, m10, m1, s10, s1, f10, f1 As Byte

                h10 = C_PBTM(Offset) >> 4 And 15
                h1 = C_PBTM(Offset) And 15

                m10 = C_PBTM(Offset + 1) >> 4 And 15
                m1 = C_PBTM(Offset + 1) And 15

                s10 = C_PBTM(Offset + 2) >> 4 And 15
                s1 = C_PBTM(Offset + 2) And 15

                f10 = C_PBTM(Offset + 3) >> 4 And 3
                f1 = C_PBTM(Offset + 3) And 15


                Hours = (h10 * 10) + h1
                Minutes = (m10 * 10) + m1
                Seconds = (s10 * 10) + s1
                Frames = (f10 * 10) + f1

                Select Case (C_PBTM(Offset + 3) >> 6) And 3
                    Case 3
                        Framerate = eFramerate.NTSC
                    Case 2
                        Framerate = Nothing
                    Case 1
                        Framerate = eFramerate.PAL
                    Case 0
                        Framerate = Nothing
                End Select
            Catch ex As Exception
                Throw New Exception("Problem with New cTimecode from C_PBTM. Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Sub New(ByVal GrossSeconds As UInt32, ByVal nFramerate As eFramerate)
            Me.New(CLng(GrossSeconds * nFramerate), nFramerate)
        End Sub

        Public Sub New(ByVal nFrames As Long, ByVal nFramerate As eFramerate)
            Dim res As Long = 0
            Me.Hours = Math.DivRem(nFrames, nFramerate * 3600, res)
            nFrames -= (nFramerate * 3600) * Hours
            Me.Minutes = Math.DivRem(nFrames, (nFramerate * 60), res)
            nFrames -= (nFramerate * 60) * Minutes
            Me.Seconds = Math.DivRem(nFrames, nFramerate, res)
            nFrames -= nFramerate * Seconds
            Me.Framerate = nFramerate

            If nFrames = nFramerate Then
                nFrames = 0
                Seconds += 1
            End If

            If Seconds = 60 Then
                Seconds = 0
                Minutes += 1
            End If

            If Minutes = 60 Then
                Minutes = 0
                Hours += 1
            End If

            Me.Frames = nFrames

        End Sub

        Public Sub New(ByVal REFERENCE_TIME As ULong, ByVal ATPF As UInt32, ByVal nFrameRate As eFramerate)
            Me.New(CLng(REFERENCE_TIME / ATPF), nFrameRate)
        End Sub

        Public Function ToDVDTimeCode() As DvdTimeCode
            Try
                Dim out As New DvdTimeCode
                out.bFrames = Me.Frames
                out.bHours = Me.Hours
                out.bMinutes = Me.Minutes
                out.bSeconds = Me.Seconds
                Return out
            Catch ex As Exception
                MsgBox("Problem with ToDVDTimeCode. Error: " & ex.Message)
            End Try
        End Function

        Public Sub Subtract(ByVal Hours As Byte, ByVal Minutes As Byte, ByVal Seconds As Byte, ByVal Frames As Byte)
            Try
                Dim tempTC As cTimecode = SubtractTimecode(Me, New cTimecode(Hours, Minutes, Seconds, Frames, Me.Framerate), Me.Framerate)
                Me.Frames = tempTC.Frames
                Me.Seconds = tempTC.Seconds
                Me.Minutes = tempTC.Minutes
                Me.Hours = tempTC.Hours
            Catch ex As Exception
                Throw New Exception("Problem with cTimecode.Subtract(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Sub Add(ByVal Hours As Byte, ByVal Minutes As Byte, ByVal Seconds As Byte, ByVal Frames As Byte)
            Try
                Dim tempTC As cTimecode = AddTimecode(Me, New cTimecode(Hours, Minutes, Seconds, Frames, Me.Framerate), Me.Framerate)
                Me.Frames = tempTC.Frames
                Me.Seconds = tempTC.Seconds
                Me.Minutes = tempTC.Minutes
                Me.Hours = tempTC.Hours
            Catch ex As Exception
                Throw New Exception("Problem with cTimecode.Add(). Error: " & ex.Message, ex)
            End Try
        End Sub

    End Class

    Public Enum eFramerate
        PAL = 25
        NTSC = 30
        FILM = 24
    End Enum

    Public Structure DvdTimeCode
        Public bHours As Byte
        Public bMinutes As Byte
        Public bSeconds As Byte
        Public bFrames As Byte
    End Structure

    Public Structure DvdPlayLocation
        Public TitleNum As Integer
        Public ChapterNum As Integer
        Public timeCode As DvdTimeCode
        Public TimeCodeFlags As Integer
    End Structure

End Namespace 'TimecodeMath

