Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Text
Imports SMT.Multimedia.DirectShow

Namespace Multimedia.Formats.Line21

    Public Class cLine21Processing

        Public WithEvents GOPs As colGOPs
        Public DataToProcess() As Byte

        'CORE FUNCTIONS

        Dim Blocking As Boolean
        Public Function AddRawL21Data(ByVal b() As Byte) As String
            Try
                If Blocking Then Return "Not accepting new data right now."
                If b.Length = 0 Then GoTo Success
                Blocking = True
                'If DataToProcess Is Nothing Then ReDim DataToProcess(-1)
                Dim OldUboundDP As Integer = UBound(DataToProcess)
                ReDim Preserve DataToProcess(OldUboundDP + b.Length)
                Array.Copy(b, 0, DataToProcess, OldUboundDP + 1, b.Length)
                If AttemptDataProcess() Then
Success:
                    Blocking = False
                    Return "Success"
                Else
                    Return "Failure to process data."
                End If
            Catch ex As Exception
                Return ("Problem with AddRawL21Data. Error: " & ex.Message)
            End Try
        End Function

        Public Function AttemptDataProcess() As Boolean
            Try
                Dim NewGOPOSs(-1) As Short
                For i As Integer = 0 To UBound(DataToProcess)
                    If i + 1 > UBound(DataToProcess) Then Exit For
                    If DataToProcess(i) = 67 And DataToProcess(i + 1) = 67 Then
                        ReDim Preserve NewGOPOSs(UBound(NewGOPOSs) + 1)
                        NewGOPOSs(UBound(NewGOPOSs)) = i - 4
                    End If
                Next

                Dim NextStartingPoint As Integer = NewGOPOSs(UBound(NewGOPOSs))
                ReDim Preserve NewGOPOSs(UBound(NewGOPOSs) - 1)

                Dim tB() As Byte
                For i As Integer = 0 To UBound(NewGOPOSs)
                    If i = 0 Then
                        ReDim tB(104)
                    ElseIf i = UBound(NewGOPOSs) Then
                        'todo: fix this
                        ReDim tB(UBound(DataToProcess) - NewGOPOSs(i) - 1)
                    Else
                        ReDim tB(NewGOPOSs(i + 1) - NewGOPOSs(i) - 1)
                    End If
                    System.Array.Copy(DataToProcess, NewGOPOSs(i), tB, 0, tB.Length)
                    Me.GOPs.Add(GetGOP(tB), Me)
                Next

                'Remove processed data from the array
                Dim newArr(UBound(DataToProcess) - NextStartingPoint) As Byte
                Array.Copy(DataToProcess, NextStartingPoint, newArr, 0, newArr.Length)
                DataToProcess = newArr
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AttemptDataProcess. Error: " & ex.Message)
                Me.GOPs = New colGOPs
                ReDim DataToProcess(-1)
                Blocking = False
            End Try
        End Function

        Public Function GetAllL21Lines() As String
            Try
                Dim s() As String
                Dim SB As New StringBuilder
                Dim PrevCmd As String
                For Each G As GOP In GOPs
                    If Not G.CaptionSegments Is Nothing Then
                        For Each CS As CaptionSegment In G.CaptionSegments
                            s = Split(CS.Caption_Fa, ",", -1, CompareMethod.Text)
                            For i As Short = 0 To UBound(s)
                                If Not s(i) = "0" And Not s(i) = "128" And Not s(i) = "80" And Not s(i) = "" Then
                                    If InStr(s(i).ToLower, "cmd", CompareMethod.Text) Then
                                        If PrevCmd = s(i).ToLower Then
                                            PrevCmd = ""
                                        Else
                                            SB.Append(vbNewLine & s(i) & vbNewLine & vbNewLine)
                                            PrevCmd = s(i).ToLower
                                        End If
                                    Else
                                        SB.Append(GetStringFromL21Chr(s(i)))
                                    End If
                                End If
                            Next
                            s = Split(CS.Caption_Fb, ",", -1, CompareMethod.Text)
                            For i As Short = 0 To UBound(s)
                                If Not s(i) = "0" And Not s(i) = "128" And Not s(i) = "80" And Not s(i) = "" Then
                                    If InStr(s(i).ToLower, "cmd", CompareMethod.Text) Then
                                        If PrevCmd = s(i).ToLower Then
                                            PrevCmd = ""
                                        Else
                                            SB.Append(vbNewLine & s(i) & vbNewLine & vbNewLine)
                                            PrevCmd = s(i).ToLower
                                        End If
                                    Else
                                        SB.Append(GetStringFromL21Chr(s(i)))
                                    End If
                                End If
                            Next
                        Next
                    End If
                Next
                Dim out As String = SB.ToString
                out = Replace(out, vbNewLine & vbNewLine, vbNewLine, 1, -1, CompareMethod.Text)
                out = Replace(out, vbNewLine & vbNewLine, vbNewLine, 1, -1, CompareMethod.Text)
                Return out
            Catch ex As Exception
                Throw New Exception("Problem getting string from GOP array. Error: " & ex.Message)
            End Try
        End Function

        Public Function GetGOP(ByVal b() As Byte) As GOP
            Try
                Dim out As New GOP
                Dim tB() As Byte
                Dim ascii As New ASCIIEncoding
                Dim tStr As String

                'UserDataPacketHeader - bytes 0-3
                ReDim out.UserDataPacketHeader(3)
                Array.Copy(b, 0, out.UserDataPacketHeader, 0, 4)

                'DVDCCHeader - bytes 4-7
                ReDim tB(3)
                Array.Copy(b, 4, tB, 0, 4)
                out.DVDCCHeader = ascii.GetString(tB)

                'Attributes - byte 8
                Dim Atts As String = DecimalToBinary(b(8))

                Atts = PadString(Atts, 8, "0", True)

                out.TruncateFlag = CBool(Atts.Chars(7).ToString)

                out.CaptionCount = BinaryToDecimal(Atts.Chars(6).ToString & Atts.Chars(5).ToString & Atts.Chars(4).ToString & Atts.Chars(3).ToString)

                ReDim out.Filler(1) '2, 1
                out.Filler(0) = Atts.Chars(1).ToString
                out.Filler(1) = Atts.Chars(2).ToString

                out.PatternFlag = Atts.Chars(0).ToString

                'Caption Segments bytes 9 - n
                If out.CaptionCount = 0 Then
                    out.CaptionSegments = Nothing
                Else
                    ReDim out.CaptionSegments(out.CaptionCount - 1)

                    Dim cnt As Integer = 0
                    For i As Integer = 9 To 6 * out.CaptionCount + 8 Step 6
                        If cnt = out.CaptionCount Then Exit For
                        ReDim tB(5)
                        If i > UBound(b) Or i < 0 Or i + 5 > UBound(b) Then
                            'Padding the GOP with extra null L21s.  See if this is ok by studying output.
                            Dim ttb() As Byte = {255, 128, 128, 254, 0, 0}
                            out.CaptionSegments(cnt) = GetCaptionSegment(ttb, out.PatternFlag)
                        Else
                            Array.Copy(b, i, tB, 0, 6)
                            out.CaptionSegments(cnt) = GetCaptionSegment(tB, out.PatternFlag)
                        End If
                        cnt += 1
                    Next
                End If

                'Footer
                Dim FooterSize As Short = b.Length - 9 - (6 * out.CaptionCount)
                If FooterSize < 1 Then
                    ReDim out.Footer(-1)
                Else
                    ReDim tB(FooterSize)
                    Array.Copy(b, 9 + (6 * out.CaptionCount) - 1, tB, 0, tB.Length)
                    out.Footer = tB
                End If

                Return out
            Catch ex As Exception
                Throw New Exception("Problem with GetGOP. Error: " & ex.Message)
            End Try
        End Function

        Public Function GetCaptionSegment(ByVal b() As Byte, ByVal Pattern As ePatternFlag) As CaptionSegment
            Try
                'b is six bytes containing the caption segment for Field 1 and field 2

                Dim out As New CaptionSegment

                If (b(0) <> 255 And b(0) <> 254) Or (b(3) <> 255 And b(3) <> 254) Then
                    GoTo UsePattern
                End If

                If b(0) = b(3) Then
UsePattern:
                    'the segment field order flagging is not set, use the pattern flag from the header
                    If Pattern = ePatternFlag.F1thenF2 Then
                        out.Field_A = eCaptionSegmentField.Field1
                        out.Field_B = eCaptionSegmentField.Field2
                    Else
                        out.Field_A = eCaptionSegmentField.Field2
                        out.Field_B = eCaptionSegmentField.Field1
                    End If
                Else
                    out.Field_A = CInt(b(0))
                    out.Field_B = CInt(b(3))
                End If

                ''JUST OUT OF CURIOSITY - Does the pattern flag match up with the field ordering specified in the caption segment?
                'Select Case b(0)
                '    Case &HFF
                '        Debug.WriteLine("GetCaptionSegment - Field 1 first.")
                '        If Pattern = ePatternFlag.F2thenF1 Then
                '            Debug.WriteLine("GetCaptionSegment - PATTERN FLAG DOES NOT MATCH SEGMENT ORDER")
                '        End If
                '    Case &HFE
                '        Debug.WriteLine("GetCaptionSegment - Field 2 first.")
                '        If Pattern = ePatternFlag.F1thenF2 Then
                '            Debug.WriteLine("GetCaptionSegment - PATTERN FLAG DOES NOT MATCH SEGMENT ORDER")
                '        End If
                '    Case Else
                '        Debug.WriteLine("GetCaptionSegment - Unexpected. Invalid value here.")
                'End Select

                ''END CURIOSITY

                Dim tB(1) As Byte

                Array.Copy(b, 1, tB, 0, 2)
                If out.Field_A = eCaptionSegmentField.Field2 Then
                    out.Field2Data = tB
                    out.Caption_Fa = ""
                Else
                    out.Field1Data = tB
                    out.Caption_Fa = GetCaptionValue(tB)
                End If

                ReDim tB(1)

                Array.Copy(b, 4, tB, 0, 2)
                If out.Field_B = eCaptionSegmentField.Field2 Then
                    out.Field2Data = tB
                    out.Caption_Fb = ""
                Else
                    out.Field1Data = tB
                    out.Caption_Fb = GetCaptionValue(tB)
                End If

                ''MORE CURIOSITY
                'If out.Field2Data(0) <> 128 Then
                '    Debug.WriteLine("GetCaptionSegment - Field 2 binary data: " & out.Field2Data(0) & " " & out.Field2Data(1))
                '    'Debug.WriteLine("GetCaptionSegment - Field 2 text data: " & GetCaptionValue(out.Field2Data))
                'End If

                ''Debug.WriteLine("GetCaptionSegment - Field 1 text data: " & GetCaptionValue(out.Field1Data))

                ''END CURIOSITY

                Return out
            Catch ex As Exception
                Throw New Exception("Problem with GetCaptionSegment. Error: " & ex.Message)
            End Try
        End Function

        Public Function GetCaptionValue(ByVal b() As Byte) As String
            Try
                If b(0) = &H80 And b(1) = &H80 Then GoTo ReturnIt
                If b(0) = 0 And b(1) = 0 Then GoTo ReturnIt
                If b(0) = 0 And b(1) = 1 Then Return "0,0"

                Dim i As Integer = CInt("&h" & Hex(b(0)) & Hex(b(1)))
                'Debug.WriteLine("&h" & Hex(b(0)) & Hex(b(1)))

                Dim tS As eLine21Commands = i
                If Not IsNumeric(tS.ToString) Then
                    'Debug.WriteLine(tS.ToString)
                    Return "CMD: " & tS.ToString
                End If

                Dim SC As eSpecialChars = i
                If Not IsNumeric(SC.ToString) Then
                    'Debug.WriteLine(tS.ToString)
                    Return "+SC " & SC.ToString
                End If

                Dim MC As eMysteryCombos = i
                If Not IsNumeric(MC.ToString) Then
                    Debug.WriteLine("Mystery Char: &H" & Hex(b(0)) & Hex(b(1)))
                    Return ""
                End If

ReturnIt:
                Return b(0).ToString & "," & b(1).ToString

                'Dim O1 As eCharacters = b(0)
                'Dim O2 As eCharacters = b(1)

                'Dim out As String
                'out = O1 & "," & O2

                'If InStr(O1.ToString, "128", CompareMethod.Text) Or InStr(O2.ToString, "128", CompareMethod.Text) Or InStr(O1.ToString, "0", CompareMethod.Text) Or InStr(O2.ToString, "0", CompareMethod.Text) Or InStr(O1.ToString, "1", CompareMethod.Text) Or InStr(O2.ToString, "1", CompareMethod.Text) Then
                '    'Return out
                '    Return ""
                'Else
                '    'Me.txtASCII.Text &= vbNewLine & out
                '    If O1.ToString = "248" Then
                '        Throw New Exception("hey")
                '    End If
                '    Return out
                'End If
            Catch ex As Exception
                Throw New Exception("Problem getting caption value. Error: " & ex.Message)
            End Try
        End Function

        Public Function GetStringFromL21Chr(ByVal v As eCharacters) As String
            Try
                Select Case v
                    Case eCharacters.Space
                        Return " "
                    Case eCharacters.Exclamation
                        Return "!"
                    Case eCharacters.Quote
                        Return Chr(34)
                    Case eCharacters.DollarSign
                        Return "$"
                    Case eCharacters.Percent
                        Return "%"
                    Case eCharacters.AndSym
                        Return "&"
                    Case eCharacters.SingleQuot
                        Return "'"
                    Case eCharacters.OpenParen
                        Return "("
                    Case eCharacters.CloseParen
                        Return ")"
                    Case eCharacters.Plus
                        Return "+"
                    Case eCharacters.Comma
                        Return ","
                    Case eCharacters.Dash
                        Return "-"
                    Case eCharacters.Period
                        Return "."
                    Case eCharacters.ForwardSlash
                        Return "/"
                    Case eCharacters.Zero
                        Return "0"
                    Case eCharacters.One
                        Return "1"
                    Case eCharacters.Two
                        Return "2"
                    Case eCharacters.Three
                        Return "3"
                    Case eCharacters.Four
                        Return "4"
                    Case eCharacters.Five
                        Return "5"
                    Case eCharacters.Six
                        Return "6"
                    Case eCharacters.Seven
                        Return "7"
                    Case eCharacters.Eight
                        Return "8"
                    Case eCharacters.Nine
                        Return "9"
                    Case eCharacters.Colon
                        Return ":"
                    Case eCharacters.SemiColon
                        Return ";"
                    Case eCharacters.LessThan
                        Return "<"
                    Case eCharacters.Equal
                        Return "="
                    Case eCharacters.GreaterThan
                        Return ">"
                    Case eCharacters.QuestionMark
                        Return "?"
                    Case eCharacters.AtSym
                        Return "@"
                    Case eCharacters.OpenBracket
                        Return "["
                    Case eCharacters.CloseBracket
                        Return "]"
                    Case eCharacters.sA
                        Return "a"
                    Case eCharacters.sB
                        Return "b"
                    Case eCharacters.sC
                        Return "c"
                    Case eCharacters.sD
                        Return "d"
                    Case eCharacters.sE
                        Return "e"
                    Case eCharacters.sF
                        Return "f"
                    Case eCharacters.sG
                        Return "g"
                    Case eCharacters.sH
                        Return "h"
                    Case eCharacters.sI
                        Return "i"
                    Case eCharacters.sJ
                        Return "j"
                    Case eCharacters.sK
                        Return "k"
                    Case eCharacters.sL
                        Return "l"
                    Case eCharacters.sM
                        Return "m"
                    Case eCharacters.sN
                        Return "n"
                    Case eCharacters.sO
                        Return "o"
                    Case eCharacters.sP
                        Return "p"
                    Case eCharacters.sQ
                        Return "q"
                    Case eCharacters.sR
                        Return "r"
                    Case eCharacters.sS
                        Return "s"
                    Case eCharacters.sT
                        Return "t"
                    Case eCharacters.sU
                        Return "u"
                    Case eCharacters.sV
                        Return "v"
                    Case eCharacters.sW
                        Return "w"
                    Case eCharacters.sX
                        Return "x"
                    Case eCharacters.sY
                        Return "y"
                    Case eCharacters.sZ
                        Return "z"
                    Case eCharacters.A
                        Return "A"
                    Case eCharacters.B
                        Return "B"
                    Case eCharacters.C
                        Return "C"
                    Case eCharacters.D
                        Return "D"
                    Case eCharacters.E
                        Return "E"
                    Case eCharacters.F
                        Return "F"
                    Case eCharacters.G
                        Return "G"
                    Case eCharacters.H
                        Return "H"
                    Case eCharacters.I
                        Return "I"
                    Case eCharacters.J
                        Return "J"
                    Case eCharacters.K
                        Return "K"
                    Case eCharacters.L
                        Return "L"
                    Case eCharacters.M
                        Return "M"
                    Case eCharacters.N
                        Return "N"
                    Case eCharacters.O
                        Return "O"
                    Case eCharacters.P
                        Return "P"
                    Case eCharacters.Q
                        Return "Q"
                    Case eCharacters.R
                        Return "R"
                    Case eCharacters.S
                        Return "S"
                    Case eCharacters.T
                        Return "T"
                    Case eCharacters.U
                        Return "U"
                    Case eCharacters.V
                        Return "V"
                    Case eCharacters.W
                        Return "W"
                    Case eCharacters.X
                        Return "X"
                    Case eCharacters.Y
                        Return "Y"
                    Case eCharacters.Z
                        Return "Z"
                    Case eCharacters.Divide
                        Return "/"
                    Case eCharacters.CapNyey
                        Return Chr(209)
                    Case eCharacters.sNyey
                        Return Chr(241)
                    Case eCharacters.aAgu
                        Return Chr(225)
                    Case eCharacters.eAgu
                        Return Chr(233)
                    Case eCharacters.iAgu
                        Return Chr(237)
                    Case eCharacters.oAgu
                        Return Chr(243)
                    Case eCharacters.uAgu
                        Return Chr(250)
                    Case eCharacters.CSedie
                        Return Chr(231)
                    Case eCharacters.CSedie
                        Return Chr(231)
                        'Case eCharacters.Registered
                        '    Return Chr(174)
                        'Case eCharacters.Circle
                        '    Return Chr(149)
                        'Case eCharacters.HalfFrac
                        '    Return Chr(189)
                        'Case eCharacters.UpsideDownQuest
                        '    Return Chr(191)
                        'Case eCharacters.Trademark
                        '    Return Chr(153)
                        'Case eCharacters.Cents
                        '    Return Chr(162)
                        'Case eCharacters.Lire
                        '    Return Chr(163)
                        'Case eCharacters.aGrave
                        '    Return Chr(224)
                        'Case eCharacters.eGrave
                        '    Return Chr(232)
                        'Case eCharacters.aHat
                        '    Return Chr(226)
                        'Case eCharacters.eHat
                        '    Return Chr(234)
                        'Case eCharacters.iHat
                        '    Return Chr(206)
                        'Case eCharacters.oHat
                        '    Return Chr(244)
                        'Case eCharacters.uHat
                        '    Return Chr(251)
                        'Case eCharacters.Blank
                        '    Return " "
                    Case Else
                        Debug.WriteLine("Uncharted Mystery Char: " & v)
                        Return "MC"

                End Select
                'Box = &H7F
                'Capital letters
                'MusicNote = &H9137
                Return v.ToString
            Catch ex As Exception
                Throw New Exception("Problem with GetL21ASCIIString. Error: " & ex.Message)
            End Try
        End Function


        'BITMAP GRABBING

        Public Function GetL21BMP(ByVal b() As Byte) As Bitmap
            Try
                Dim bm As New Bitmap(640, 480, Imaging.PixelFormat.Format24bppRgb)
                Dim PixIncre As Integer = 0
                For X As Short = 479 To 0 Step -1
                    For Y As Short = 0 To 639
                        bm.SetPixel(Y, X, Me.L218bitCT(CInt("&h" & GetByte(b(PixIncre)))))
                        PixIncre += 1
                    Next
                Next
                bm.Save("C:\Temp\" & "L21_" & DateTime.Now.Ticks & ".bmp", System.Drawing.Imaging.ImageFormat.Bmp)
            Catch ex As Exception
                Throw New Exception("Problem with GetL21BMP. Error: " & ex.Message)
            End Try
        End Function

        Public L218bitCT() As Color

        Public Function SetupL218bitColorTable(ByVal MT As AM_MEDIA_TYPE) As Boolean
            Try
                Dim VIH As New VIDEOINFOHEADER
                Dim b(Convert.ToInt32(MT.cbFormat) - 1) As Byte
                For i As Integer = 0 To UBound(b)
                    b(i) = Marshal.ReadByte(MT.pbFormat, i)
                Next

                VIH.AvgTimePerFrame = CInt("&h" & GetByte(b(23)) & GetByte(b(22)) & GetByte(b(21)) & GetByte(b(20)))
                VIH.BitRate = CInt("&h" & GetByte(b(35)) & GetByte(b(34)) & GetByte(b(33)) & GetByte(b(32)))
                VIH.BitErrorRate = CInt("&h" & GetByte(b(39)) & GetByte(b(38)) & GetByte(b(37)) & GetByte(b(36)))
                VIH.AvgTimePerFrame = CInt("&h" & GetByte(b(47)) & GetByte(b(46)) & GetByte(b(45)) & GetByte(b(44)) & GetByte(b(43)) & GetByte(b(42)) & GetByte(b(41)) & GetByte(b(40)))

                Dim R As New DsRECT
                R.Bottom = CInt("&h" & GetByte(b(13)) & GetByte(b(12)))
                R.Right = CInt("&h" & GetByte(b(9)) & GetByte(b(8)))
                R.Top = 0
                R.Left = 0
                VIH.SrcRect = R
                VIH.TagRect = R

                Dim OS As Short = 48
                VIH.BmiHeader.biSize = CInt("&h" & GetByte(b(OS + 3)) & GetByte(b(OS + 2)) & GetByte(b(OS + 1)) & GetByte(b(OS)))
                VIH.BmiHeader.biWidth = CInt("&h" & GetByte(b(OS + 7)) & GetByte(b(OS + 6)) & GetByte(b(OS + 5)) & GetByte(b(OS + 4)))
                VIH.BmiHeader.biHeight = CInt("&h" & GetByte(b(OS + 11)) & GetByte(b(OS + 10)) & GetByte(b(OS + 9)) & GetByte(b(OS + 8)))
                VIH.BmiHeader.biPlanes = CInt("&h" & GetByte(b(OS + 13)) & GetByte(b(OS + 12)))
                VIH.BmiHeader.biBitCount = CInt("&h" & GetByte(b(OS + 15)) & GetByte(b(OS + 14)))
                VIH.BmiHeader.biCompression = CInt("&h" & GetByte(b(OS + 19)) & GetByte(b(OS + 18)) & GetByte(b(OS + 17)) & GetByte(b(OS + 16)))
                VIH.BmiHeader.biSizeImage = CInt("&h" & GetByte(b(OS + 23)) & GetByte(b(OS + 22)) & GetByte(b(OS + 21)) & GetByte(b(OS + 20)))
                VIH.BmiHeader.biXPelsPerMeter = CInt("&h" & GetByte(b(OS + 27)) & GetByte(b(OS + 26)) & GetByte(b(OS + 25)) & GetByte(b(OS + 24)))
                VIH.BmiHeader.biYPelsPerMeter = CInt("&h" & GetByte(b(OS + 31)) & GetByte(b(OS + 30)) & GetByte(b(OS + 29)) & GetByte(b(OS + 28)))
                VIH.BmiHeader.biClrUsed = CInt("&h" & GetByte(b(OS + 35)) & GetByte(b(OS + 34)) & GetByte(b(OS + 33)) & GetByte(b(OS + 32)))
                VIH.BmiHeader.biClrImportant = CInt("&h" & GetByte(b(OS + 39)) & GetByte(b(OS + 38)) & GetByte(b(OS + 37)) & GetByte(b(OS + 36)))
                Dim CT(UBound(b) - (OS + 40)) As Byte
                Dim cnt As Integer = 0
                For i As Short = OS + 40 To UBound(b)
                    CT(cnt) = b(i)
                    cnt += 1
                Next

                ReDim L218bitCT((CT.Length / 4) - 1)
                cnt = 0
                For i As Short = 0 To UBound(CT) Step 4
                    'L218bitCT(cnt) = Color.FromArgb(CInt("&h" & GetByte(CT(i))), CInt("&h" & GetByte(CT(i + 1))), CInt("&h" & GetByte(CT(i + 2))))
                    L218bitCT(cnt) = Color.FromArgb(CInt("&h" & GetByte(CT(i + 2))), CInt("&h" & GetByte(CT(i + 1))), CInt("&h" & GetByte(CT(i))))
                    cnt += 1
                Next

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with GetL218bitCT. Error: " & ex.Message)
                Return False
            End Try
        End Function


        'STANDARD FUNCTIONS

        Public Sub New()
            Me.GOPs = New colGOPs
            ReDim DataToProcess(-1)
        End Sub


        'NEW L21 LINE HANDLING

        Public Event NewL21Line(ByVal Line() As String)

        Public Sub HandleGOPString(ByVal Line As String) Handles GOPs.gNewL21Line
            Line = Replace(Line, "BlankMeBlank", " ")
            Dim out() As String = Split(Line, "@@@")
            RaiseEvent NewL21Line(out)
        End Sub


        'CLASSES

        Public Class GOP

            'User Data Packet header (never changes).
            Public UserDataPacketHeader() As Byte

            'DVD Closed Caption header (never changes).
            Public DVDCCHeader As String

            'Whether or not to drop last three bytes of last caption segment (used to fit 96-byte limit for a 15-frame GOP). Note that the Pattern Flag in the next CC Packet must flip if this flag is set (otherwise, you'd lose that last field's worth of data).
            Public TruncateFlag As Boolean

            'How many caption segments in the packet (same as number of frames in GOP).
            Public CaptionCount As Short

            'Filler (never changes)
            Public Filler() As Byte

            'Pattern Flag: Determines if each caption segment is Field 1 followed by Field 2 (1) or Field 2 followed by Field 1 (0).
            Public PatternFlag As ePatternFlag

            'CAPTION SEGMENT (6 bytes)--repeat for each frame of GOP
            Public CaptionSegments() As CaptionSegment

            'Padding (repeat 00 byte until packet is 96 bytes long)
            Public Footer() As Byte 'padding to 96 bytes

        End Class

        Public Class CaptionSegment
            Public Field_A As eCaptionSegmentField
            Public Caption_Fa As String
            Public Field_B As eCaptionSegmentField
            Public Caption_Fb As String
            Public Field1Data() As Byte
            Public Field2Data() As Byte
        End Class

        Public Class colGOPs
            Inherits CollectionBase

            Public Event gNewL21Line(ByVal Line As String)

            Public Function Add(ByVal newGOP As GOP, ByRef cL21 As cLine21Processing) As Integer
                Try
                    Dim s() As String
                    Dim SB As New StringBuilder
                    Dim PrevCmd As String

                    If Not newGOP.CaptionSegments Is Nothing Then
                        For Each CS As CaptionSegment In newGOP.CaptionSegments
                            s = Split(CS.Caption_Fa, ",", -1, CompareMethod.Text)
                            For i As Short = 0 To UBound(s)
                                If Not s(i) = "0" And Not s(i) = "128" And Not s(i) = "80" And Not s(i) = "" Then
                                    If InStr(s(i).ToLower, "cmd", CompareMethod.Text) Then
                                        If PrevCmd = s(i).ToLower Then
                                            PrevCmd = ""
                                        Else
                                            SB.Append("@@@" & s(i) & "@@@")
                                            PrevCmd = s(i).ToLower
                                        End If
                                    ElseIf InStr(s(i).ToLower, "+sc") Then
                                        'special characters
                                        If PrevCmd = s(i).ToLower Then
                                            PrevCmd = ""
                                        Else
                                            SB.Append(Replace(s(i), "+SC", ""))
                                            PrevCmd = s(i).ToLower
                                        End If
                                    Else
                                        SB.Append(cL21.GetStringFromL21Chr(s(i)))
                                    End If
                                End If
                            Next
                            s = Split(CS.Caption_Fb, ",", -1, CompareMethod.Text)
                            For i As Short = 0 To UBound(s)
                                If Not s(i) = "0" And Not s(i) = "128" And Not s(i) = "80" And Not s(i) = "" Then
                                    If InStr(s(i).ToLower, "cmd", CompareMethod.Text) Then
                                        If PrevCmd = s(i).ToLower Then
                                            PrevCmd = ""
                                        Else
                                            SB.Append("@@@" & s(i) & "@@@")
                                            PrevCmd = s(i).ToLower
                                        End If
                                    ElseIf InStr(s(i).ToLower, "+sc") Then
                                        'special characters
                                        If PrevCmd = s(i).ToLower Then
                                            PrevCmd = ""
                                        Else
                                            SB.Append(Replace(s(i), "+SC", ""))
                                            PrevCmd = s(i).ToLower
                                        End If
                                    Else
                                        SB.Append(cL21.GetStringFromL21Chr(s(i)))
                                    End If
                                End If
                            Next
                        Next
                    End If
                    Dim out As String = SB.ToString
                    out = Replace(out, vbNewLine & vbNewLine, vbNewLine, 1, -1, CompareMethod.Text)
                    out = Replace(out, vbNewLine & vbNewLine, vbNewLine, 1, -1, CompareMethod.Text)

                    If Not out Is Nothing AndAlso out <> "" Then
                        'Debug.WriteLine(out) 'log everything coming through
                        RaiseEvent gNewL21Line(out)
                    End If
                Catch ex As Exception
                    Throw New Exception("Problem with AddGOP string parsing. Error: " & ex.Message)
                End Try

                Return MyBase.List.Add(newGOP)
            End Function

            Default Public ReadOnly Property Item(ByVal index As Integer) As GOP
                Get
                    Return MyBase.List.Item(index)
                End Get
            End Property

            Public Sub Remove(ByVal Item As GOP)
                MyBase.List.Remove(Item)
            End Sub

        End Class

        'ENUMS

        Public Enum ePatternFlag
            F2thenF1 = 0
            F1thenF2 = 1
        End Enum

        Public Enum eCaptionSegmentField
            Field1 = 255
            Field2 = 254
        End Enum

        Public Enum eCharacters
            Space = &H20
            Exclamation = &HA1
            Quote = &H23
            DollarSign = &HA4
            Percent = &H25
            AndSym = &H26
            SingleQuot = &HA7
            OpenParen = &HA8
            CloseParen = &H29
            aAgu = &H2A
            Plus = &HAB
            Comma = &H2C
            Dash = &HAD
            Period = &HAE
            ForwardSlash = &H2F
            Zero = &HB0
            One = &H31
            Two = &H32
            Three = &HB3
            Four = &H34
            Five = &HB5
            Six = &HB6
            Seven = &H37
            Eight = &H38
            Nine = &HB9
            Colon = &HBA
            SemiColon = &H3B
            LessThan = &HBC
            Equal = &H3D
            GreaterThan = &H3E
            QuestionMark = &HBF
            AtSym = &H40
            A = &HC1
            B = &HC2
            C = &H43
            D = &HC4
            E = &H45
            F = &H46
            G = &HC7
            H = &HC8
            I = &H49
            J = &H4A
            K = &HCB
            L = &H4C
            M = &HCD
            N = &HCE
            O = &H4F
            P = &HD0
            Q = &H51
            R = &H52
            S = &HD3
            T = &H54
            U = &HD5
            V = &HD6
            W = &H57
            X = &H58
            Y = &HD9
            Z = &HDA
            OpenBracket = &H5B
            eAgu = &HDC
            CloseBracket = &H5D
            iAgu = &H5E
            oAgu = &HDF
            uAgu = &HE0
            sA = &H61
            sB = &H62
            sC = &HE3
            sD = &H64
            sE = &HE5
            sF = &HE6
            sG = &H67
            sH = &H68
            sI = &HE9
            sJ = &HEA
            sK = &H6B
            sL = &HEC
            sM = &H6D
            sN = &H6E
            sO = &HEF
            sP = &H70
            sQ = &HF1
            sR = &HF2
            sS = &H73
            sT = &HF4
            sU = &H75
            sV = &H76
            sW = &HF7
            sX = &HF8
            sY = &H79
            sZ = &H7A
            CSedie = &HFB
            Divide = &H7C
            CapNyey = &HFD
            sNyey = &HFE
            Box = &H7F

        End Enum

        Public Enum eSpecialChars
            'Special Characters
            Registered = &H91B0
            Circle = &H9131
            HalfFrac = &H9132
            UpsideDownQuest = &H91B3
            Trademark = &H9134
            Cents = &H91B5
            Lire = &H91B6
            EighthNote = &H9137
            aGrave = &H9138
            BlankMeBlank = &H91B9
            eGrave = &H91BA
            aHat = &H913B
            eHat = &H913B
            iHat = &H913D
            oHat = &H913E
            uHat = &H91BF

            'Extended Characters
            'Not applicable to DVD

            'Mid row codes (rest are in commands)
            Italics = &H91AE

        End Enum

        Public Enum eLine21Commands
            'Global codes
            RCL = &H9420
            RDC = &H9429
            RU2 = &H9425
            RU3 = &H9426
            RU4 = &H94A7
            TR = &H942A
            RTD = &H94AB
            EDM = &H942C
            ENM = &H94AE
            EOC = &H942F

            'Column Moves
            MoveOverOneColumn = &H97A1
            MoveOverTwoColumns = &H97A2
            MoveOverThreeColumna = &H97A3

            'Other positioning codes
            TabOffset1 = &H97A1
            TabOffset2 = &H97A2
            TabOffset3 = &H9723
            BackSpace = &H94A1
            DeleteToEndOfRow = &H94A4
            CarriageReturn = &H94AD

            'Mid row codes  (italics is in special characters above)
            ItalicsUnderline = &H912F
            FlashingText = &H94A8
            WhiteText = &H9120
            WhiteUnderline = &H91A1
            GreenText = &H91A2
            GreenUnderline = &H9123
            BlueText = &H91A4
            BlueUnderline = &H9125
            CyanText = &H9126
            CyanUnderline = &H91A7
            RedText = &H91A8
            RedUnderline = &H9129
            YellowText = &H912A
            YellowUnderline = &H91AB
            MagentaText = &H912C
            MagentaUnderline = &H91AD
            BlackText = &H97AE
            BlackUnderline = &H972F

            'Background Color Codes
            BG_White = &H1020
            BG_White_Transparent = &H10A1
            BG_Green = &H10A2
            BG_Green_Transparent = &H1023
            BG_Blue = &H10A4
            BG_Blue_Transparent = &H1025
            BG_Cyan = &H1026
            BG_Cyan_Transparent = &H10A7
            BG_Red = &H10A8
            BG_Red_Transparent = &H1029
            BG_Yellow = &H102A
            BG_Yellow_Transparent = &H10AB
            BG_Magenta = &H102C
            BG_Magenta_Transparent = &H10AD
            BG_Black = &H10AE
            BG_Black_Transparent = &H102F
            BG_Transparent = &H97AD

            'Positioning Codes
            Col_0__Row_1__White__NoUL = &H91D0
            Col_0__Row_1__White__UL = &H9151

            Col_0__Row_2__White__NoUL = &H9170
            Col_0__Row_2__White__UL = &H91F1

            Col_0__Row_3__White__NoUL = &H92D0
            Col_0__Row_3__White__UL = &H9251

            Col_0__Row_4__White__NoUL = &H9270
            Col_0__Row_4__White__UL = &H92F1

            Col_0__Row_5__White__NoUL = &H15D0
            Col_0__Row_5__White__UL = &H1551

            Col_0__Row_6__White__NoUL = &H1570
            Col_0__Row_6__White__UL = &H15F1

            Col_0__Row_7__White__NoUL = &H16D0
            Col_0__Row_7__White__UL = &H1651

            Col_0__Row_8__White__NoUL = &H1670
            Col_0__Row_8__White__UL = &H16F1

            Col_0__Row_9__White__NoUL = &H97D0
            Col_0__Row_9__White__UL = &H9751

            Col_0__Row_10__White__NoUL = &H9770
            Col_0__Row_10__White__UL = &H97F1

            Col_0__Row_11__White__NoUL = &H10D0
            Col_0__Row_11__White__UL = &H1051

            Col_0__Row_12__White__NoUL = &H13D0
            Col_0__Row_12__White__UL = &H1351

            Col_0__Row_13__White__NoUL = &H1370
            Col_0__Row_13__White__UL = &H13F1

            Col_0__Row_14__White__NoUL = &H94D0
            Col_0__Row_14__White__UL = &H9451

            Col_0__Row_15__White__NoUL = &H9470
            Col_0__Row_15__White__UL = &H94F1


            Col_0__Row_1__Green__NoUL = &H91C2
            Col_0__Row_1__Green__UL = &H9143

            Col_0__Row_2__Green__NoUL = &H9162
            Col_0__Row_2__Green__UL = &H91E3

            Col_0__Row_3__Green__NoUL = &H92C2
            Col_0__Row_3__Green__UL = &H9243

            Col_0__Row_4__Green__NoUL = &H9262
            Col_0__Row_4__Green__UL = &H92E3

            Col_0__Row_5__Green__NoUL = &H15C2
            Col_0__Row_5__Green__UL = &H1543

            Col_0__Row_6__Green__NoUL = &H1562
            Col_0__Row_6__Green__UL = &H15E3

            Col_0__Row_7__Green__NoUL = &H16C2
            Col_0__Row_7__Green__UL = &H1643

            Col_0__Row_8__Green__NoUL = &H1662
            Col_0__Row_8__Green__UL = &H16E3

            Col_0__Row_9__Green__NoUL = &H97C2
            Col_0__Row_9__Green__UL = &H9743

            Col_0__Row_10__Green__NoUL = &H9762
            Col_0__Row_10__Green__UL = &H97E3

            Col_0__Row_11__Green__NoUL = &H10C2
            Col_0__Row_11__Green__UL = &H1043

            Col_0__Row_12__Green__NoUL = &H13C2
            Col_0__Row_12__Green__UL = &H1343

            Col_0__Row_13__Green__NoUL = &H1362
            Col_0__Row_13__Green__UL = &H13E3

            Col_0__Row_14__Green__NoUL = &H94C2
            Col_0__Row_14__Green__UL = &H9443

            Col_0__Row_15__Green__NoUL = &H9462
            Col_0__Row_15__Green__UL = &H94E3


            Col_0__Row_1__Blue__NoUL = &H91C4
            Col_0__Row_1__Blue__UL = &H9145

            Col_0__Row_2__Blue__NoUL = &H9164
            Col_0__Row_2__Blue__UL = &H91E5

            Col_0__Row_3__Blue__NoUL = &H92C4
            Col_0__Row_3__Blue__UL = &H9245

            Col_0__Row_4__Blue__NoUL = &H9264
            Col_0__Row_4__Blue__UL = &H92E5

            Col_0__Row_5__Blue__NoUL = &H15C4
            Col_0__Row_5__Blue__UL = &H1545

            Col_0__Row_6__Blue__NoUL = &H1564
            Col_0__Row_6__Blue__UL = &H15E5

            Col_0__Row_7__Blue__NoUL = &H16C4
            Col_0__Row_7__Blue__UL = &H1645

            Col_0__Row_8__Blue__NoUL = &H1664
            Col_0__Row_8__Blue__UL = &H16E5

            Col_0__Row_9__Blue__NoUL = &H97C4
            Col_0__Row_9__Blue__UL = &H9745

            Col_0__Row_10__Blue__NoUL = &H9764
            Col_0__Row_10__Blue__UL = &H97E5

            Col_0__Row_11__Blue__NoUL = &H10C4
            Col_0__Row_11__Blue__UL = &H1045

            Col_0__Row_12__Blue__NoUL = &H13C4
            Col_0__Row_12__Blue__UL = &H1345

            Col_0__Row_13__Blue__NoUL = &H1364
            Col_0__Row_13__Blue__UL = &H13E5

            Col_0__Row_14__Blue__NoUL = &H94C4
            Col_0__Row_14__Blue__UL = &H9445

            Col_0__Row_15__Blue__NoUL = &H9464
            Col_0__Row_15__Blue__UL = &H94E5


            Col_0__Row_1__Cyan__NoUL = &H9146
            Col_0__Row_1__Cyan__UL = &H91C7

            Col_0__Row_2__Cyan__NoUL = &H91E6
            Col_0__Row_2__Cyan__UL = &H9167

            Col_0__Row_3__Cyan__NoUL = &H9246
            Col_0__Row_3__Cyan__UL = &H92C7

            Col_0__Row_4__Cyan__NoUL = &H92E6
            Col_0__Row_4__Cyan__UL = &H9267

            Col_0__Row_5__Cyan__NoUL = &H1546
            Col_0__Row_5__Cyan__UL = &H15C7

            Col_0__Row_6__Cyan__NoUL = &H15E6
            Col_0__Row_6__Cyan__UL = &H1567

            Col_0__Row_7__Cyan__NoUL = &H1646
            Col_0__Row_7__Cyan__UL = &H16C7

            Col_0__Row_8__Cyan__NoUL = &H16E6
            Col_0__Row_8__Cyan__UL = &H1667

            Col_0__Row_9__Cyan__NoUL = &H9746
            Col_0__Row_9__Cyan__UL = &H97C7

            Col_0__Row_10__Cyan__NoUL = &H97E6
            Col_0__Row_10__Cyan__UL = &H9767

            Col_0__Row_11__Cyan__NoUL = &H1046
            Col_0__Row_11__Cyan__UL = &H10C7

            Col_0__Row_12__Cyan__NoUL = &H1346
            Col_0__Row_12__Cyan__UL = &H13C7

            Col_0__Row_13__Cyan__NoUL = &H13E6
            Col_0__Row_13__Cyan__UL = &H1367

            Col_0__Row_14__Cyan__NoUL = &H9446
            Col_0__Row_14__Cyan__UL = &H94C7

            Col_0__Row_15__Cyan__NoUL = &H94E6
            Col_0__Row_15__Cyan__UL = &H9467


            Col_0__Row_1__Red__NoUL = &H91C8
            Col_0__Row_1__Red__UL = &H9149

            Col_0__Row_2__Red__NoUL = &H9168
            Col_0__Row_2__Red__UL = &H91E9

            Col_0__Row_3__Red__NoUL = &H92C8
            Col_0__Row_3__Red__UL = &H9249

            Col_0__Row_4__Red__NoUL = &H9268
            Col_0__Row_4__Red__UL = &H92E9

            Col_0__Row_5__Red__NoUL = &H15C8
            Col_0__Row_5__Red__UL = &H1549

            Col_0__Row_6__Red__NoUL = &H1568
            Col_0__Row_6__Red__UL = &H15E9

            Col_0__Row_7__Red__NoUL = &H16C8
            Col_0__Row_7__Red__UL = &H1649

            Col_0__Row_8__Red__NoUL = &H1668
            Col_0__Row_8__Red__UL = &H16E9

            Col_0__Row_9__Red__NoUL = &H97C8
            Col_0__Row_9__Red__UL = &H9749

            Col_0__Row_10__Red__NoUL = &H9768
            Col_0__Row_10__Red__UL = &H97E9

            Col_0__Row_11__Red__NoUL = &H10C8
            Col_0__Row_11__Red__UL = &H1049

            Col_0__Row_12__Red__NoUL = &H13C8
            Col_0__Row_12__Red__UL = &H1349

            Col_0__Row_13__Red__NoUL = &H1368
            Col_0__Row_13__Red__UL = &H13E9

            Col_0__Row_14__Red__NoUL = &H94C8
            Col_0__Row_14__Red__UL = &H9449

            Col_0__Row_15__Red__NoUL = &H9468
            Col_0__Row_15__Red__UL = &H94E9


            Col_0__Row_1__Yellow__NoUL = &H914A
            Col_0__Row_1__Yellow__UL = &H91CB

            Col_0__Row_2__Yellow__NoUL = &H91EA
            Col_0__Row_2__Yellow__UL = &H916B

            Col_0__Row_3__Yellow__NoUL = &H924A
            Col_0__Row_3__Yellow__UL = &H92CB

            Col_0__Row_4__Yellow__NoUL = &H92EA
            Col_0__Row_4__Yellow__UL = &H926B

            Col_0__Row_5__Yellow__NoUL = &H154A
            Col_0__Row_5__Yellow__UL = &H15CB

            Col_0__Row_6__Yellow__NoUL = &H15EA
            Col_0__Row_6__Yellow__UL = &H156B

            Col_0__Row_7__Yellow__NoUL = &H164A
            Col_0__Row_7__Yellow__UL = &H16CB

            Col_0__Row_8__Yellow__NoUL = &H16EA
            Col_0__Row_8__Yellow__UL = &H166B

            Col_0__Row_9__Yellow__NoUL = &H974A
            Col_0__Row_9__Yellow__UL = &H97CB

            Col_0__Row_10__Yellow__NoUL = &H97EA
            Col_0__Row_10__Yellow__UL = &H976B

            Col_0__Row_11__Yellow__NoUL = &H104A
            Col_0__Row_11__Yellow__UL = &H10CB

            Col_0__Row_12__Yellow__NoUL = &H134A
            Col_0__Row_12__Yellow__UL = &H13CB

            Col_0__Row_13__Yellow__NoUL = &H13EA
            Col_0__Row_13__Yellow__UL = &H136B

            Col_0__Row_14__Yellow__NoUL = &H944A
            Col_0__Row_14__Yellow__UL = &H94CB

            Col_0__Row_15__Yellow__NoUL = &H94EA
            Col_0__Row_15__Yellow__UL = &H946B


            Col_0__Row_1__Magenta__NoUL = &H914C
            Col_0__Row_1__Magenta__UL = &H91CD

            Col_0__Row_2__Magenta__NoUL = &H91EC
            Col_0__Row_2__Magenta__UL = &H916D

            Col_0__Row_3__Magenta__NoUL = &H924C
            Col_0__Row_3__Magenta__UL = &H92CD

            Col_0__Row_4__Magenta__NoUL = &H92EC
            Col_0__Row_4__Magenta__UL = &H926D

            Col_0__Row_5__Magenta__NoUL = &H154C
            Col_0__Row_5__Magenta__UL = &H15CD

            Col_0__Row_6__Magenta__NoUL = &H15EC
            Col_0__Row_6__Magenta__UL = &H156D

            Col_0__Row_7__Magenta__NoUL = &H164C
            Col_0__Row_7__Magenta__UL = &H16CD

            Col_0__Row_8__Magenta__NoUL = &H16EC
            Col_0__Row_8__Magenta__UL = &H166D

            Col_0__Row_9__Magenta__NoUL = &H974C
            Col_0__Row_9__Magenta__UL = &H97CD

            Col_0__Row_10__Magenta__NoUL = &H97EC
            Col_0__Row_10__Magenta__UL = &H976D

            Col_0__Row_11__Magenta__NoUL = &H104C
            Col_0__Row_11__Magenta__UL = &H10CD

            Col_0__Row_12__Magenta__NoUL = &H134C
            Col_0__Row_12__Magenta__UL = &H13CD

            Col_0__Row_13__Magenta__NoUL = &H13EC
            Col_0__Row_13__Magenta__UL = &H136D

            Col_0__Row_14__Magenta__NoUL = &H944C
            Col_0__Row_14__Magenta__UL = &H94CD

            Col_0__Row_15__Magenta__NoUL = &H94EC
            Col_0__Row_15__Magenta__UL = &H946D


            Col_4__Row_1__NoUL = &H9152
            Col_4__Row_1__UL = &H91D3

            Col_4__Row_2__NoUL = &H91F2
            Col_4__Row_2__UL = &H9173

            Col_4__Row_3__NoUL = &H9252
            Col_4__Row_3__UL = &H92D3

            Col_4__Row_4__NoUL = &H92F2
            Col_4__Row_4__UL = &H9273

            Col_4__Row_5__NoUL = &H1552
            Col_4__Row_5__UL = &H15D3

            Col_4__Row_6__NoUL = &H15F2
            Col_4__Row_6__UL = &H1573

            Col_4__Row_7__NoUL = &H1652
            Col_4__Row_7__UL = &H16D3

            Col_4__Row_8__NoUL = &H16F2
            Col_4__Row_8__UL = &H1673

            Col_4__Row_9__NoUL = &H9752
            Col_4__Row_9__UL = &H97D3

            Col_4__Row_10__NoUL = &H97F2
            Col_4__Row_10__UL = &H9773

            Col_4__Row_11__NoUL = &H1052
            Col_4__Row_11__UL = &H10D3

            Col_4__Row_12__NoUL = &H1352
            Col_4__Row_12__UL = &H13D3

            Col_4__Row_13__NoUL = &H13F2
            Col_4__Row_13__UL = &H1373

            Col_4__Row_14__NoUL = &H9452
            Col_4__Row_14__UL = &H94D3

            Col_4__Row_15__NoUL = &H94F2
            Col_4__Row_15__UL = &H94


            Col_8__Row_1__NoUL = &H9154
            Col_8__Row_1__UL = &H91D5

            Col_8__Row_2__NoUL = &H91F4
            Col_8__Row_2__UL = &H9175

            Col_8__Row_3__NoUL = &H9254
            Col_8__Row_3__UL = &H92D5

            Col_8__Row_4__NoUL = &H92F4
            Col_8__Row_4__UL = &H9275

            Col_8__Row_5__NoUL = &H1554
            Col_8__Row_5__UL = &H15D5

            Col_8__Row_6__NoUL = &H15F4
            Col_8__Row_6__UL = &H1575

            Col_8__Row_7__NoUL = &H1654
            Col_8__Row_7__UL = &H16D5

            Col_8__Row_8__NoUL = &H16F4
            Col_8__Row_8__UL = &H1675

            Col_8__Row_9__NoUL = &H9754
            Col_8__Row_9__UL = &H97D5

            Col_8__Row_10__NoUL = &H97F4
            Col_8__Row_10__UL = &H9775

            Col_8__Row_11__NoUL = &H1054
            Col_8__Row_11__UL = &H10D5

            Col_8__Row_12__NoUL = &H1354
            Col_8__Row_12__UL = &H13D5

            Col_8__Row_13__NoUL = &H13F4
            Col_8__Row_13__UL = &H1375

            Col_8__Row_14__NoUL = &H9454
            Col_8__Row_14__UL = &H94D5

            Col_8__Row_15__NoUL = &H94F4
            Col_8__Row_15__UL = &H9475


            Col_12__Row_1__NoUL = &H91D6
            Col_12__Row_1__UL = &H9157

            Col_12__Row_2__NoUL = &H9176
            Col_12__Row_2__UL = &H91F7

            Col_12__Row_3__NoUL = &H92D6
            Col_12__Row_3__UL = &H9257

            Col_12__Row_4__NoUL = &H9276
            Col_12__Row_4__UL = &H92F7

            Col_12__Row_5__NoUL = &H15D6
            Col_12__Row_5__UL = &H1557

            Col_12__Row_6__NoUL = &H1576
            Col_12__Row_6__UL = &H15F7

            Col_12__Row_7__NoUL = &H16D6
            Col_12__Row_7__UL = &H1657

            Col_12__Row_8__NoUL = &H1676
            Col_12__Row_8__UL = &H16F7

            Col_12__Row_9__NoUL = &H97D6
            Col_12__Row_9__UL = &H9757

            Col_12__Row_10__NoUL = &H9776
            Col_12__Row_10__UL = &H97F7

            Col_12__Row_11__NoUL = &H10D6
            Col_12__Row_11__UL = &H1057

            Col_12__Row_12__NoUL = &H13D6
            Col_12__Row_12__UL = &H1357

            Col_12__Row_13__NoUL = &H1376
            Col_12__Row_13__UL = &H13F7

            Col_12__Row_14__NoUL = &H94D6
            Col_12__Row_14__UL = &H9457

            Col_12__Row_15__NoUL = &H9476
            Col_12__Row_15__UL = &H94F7


            Col_16__Row_1__NoUL = &H9158
            Col_16__Row_1__UL = &H91D9

            Col_16__Row_2__NoUL = &H91F8
            Col_16__Row_2__UL = &H9179

            Col_16__Row_3__NoUL = &H9258
            Col_16__Row_3__UL = &H92D9

            Col_16__Row_4__NoUL = &H92F8
            Col_16__Row_4__UL = &H9279

            Col_16__Row_5__NoUL = &H1558
            Col_16__Row_5__UL = &H15D9

            Col_16__Row_6__NoUL = &H15F8
            Col_16__Row_6__UL = &H1579

            Col_16__Row_7__NoUL = &H1658
            Col_16__Row_7__UL = &H16D9

            Col_16__Row_8__NoUL = &H16F8
            Col_16__Row_8__UL = &H1679

            Col_16__Row_9__NoUL = &H9758
            Col_16__Row_9__UL = &H97D9

            Col_16__Row_10__NoUL = &H97F8
            Col_16__Row_10__UL = &H9779

            Col_16__Row_11__NoUL = &H1058
            Col_16__Row_11__UL = &H10D9

            Col_16__Row_12__NoUL = &H1358
            Col_16__Row_12__UL = &H13D9

            Col_16__Row_13__NoUL = &H13F8
            Col_16__Row_13__UL = &H1379

            Col_16__Row_14__NoUL = &H9458
            Col_16__Row_14__UL = &H94D9

            Col_16__Row_15__NoUL = &H94F8
            Col_16__Row_15__UL = &H9479


            Col_20__Row_1__NoUL = &H91DA
            Col_20__Row_1__UL = &H915B

            Col_20__Row_2__NoUL = &H917A
            Col_20__Row_2__UL = &H91FB

            Col_20__Row_3__NoUL = &H92DA
            Col_20__Row_3__UL = &H925B

            Col_20__Row_4__NoUL = &H927A
            Col_20__Row_4__UL = &H92FB

            Col_20__Row_5__NoUL = &H15DA
            Col_20__Row_5__UL = &H155B

            Col_20__Row_6__NoUL = &H157A
            Col_20__Row_6__UL = &H15FB

            Col_20__Row_7__NoUL = &H16DA
            Col_20__Row_7__UL = &H165B

            Col_20__Row_8__NoUL = &H167A
            Col_20__Row_8__UL = &H16FB

            Col_20__Row_9__NoUL = &H97DA
            Col_20__Row_9__UL = &H975B

            Col_20__Row_10__NoUL = &H977A
            Col_20__Row_10__UL = &H97FB

            Col_20__Row_11__NoUL = &H10DA
            Col_20__Row_11__UL = &H105B

            Col_20__Row_12__NoUL = &H13DA
            Col_20__Row_12__UL = &H135B

            Col_20__Row_13__NoUL = &H137A
            Col_20__Row_13__UL = &H13FB

            Col_20__Row_14__NoUL = &H94DA
            Col_20__Row_14__UL = &H945B

            Col_20__Row_15__NoUL = &H947A
            Col_20__Row_15__UL = &H94FB


            Col_24__Row_1__NoUL = &H91DC
            Col_24__Row_1__UL = &H915D

            Col_24__Row_2__NoUL = &H917C
            Col_24__Row_2__UL = &H91FD

            Col_24__Row_3__NoUL = &H92DC
            Col_24__Row_3__UL = &H925D

            Col_24__Row_4__NoUL = &H927C
            Col_24__Row_4__UL = &H92FD

            Col_24__Row_5__NoUL = &H15DC
            Col_24__Row_5__UL = &H155D

            Col_24__Row_6__NoUL = &H157C
            Col_24__Row_6__UL = &H15FD

            Col_24__Row_7__NoUL = &H16DC
            Col_24__Row_7__UL = &H165D

            Col_24__Row_8__NoUL = &H167C
            Col_24__Row_8__UL = &H16FD

            Col_24__Row_9__NoUL = &H97DC
            Col_24__Row_9__UL = &H975D

            Col_24__Row_10__NoUL = &H977C
            Col_24__Row_10__UL = &H97FD

            Col_24__Row_11__NoUL = &H10DC
            Col_24__Row_11__UL = &H105D

            Col_24__Row_12__NoUL = &H13DC
            Col_24__Row_12__UL = &H135D

            Col_24__Row_13__NoUL = &H137C
            Col_24__Row_13__UL = &H13FD

            Col_24__Row_14__NoUL = &H94DC
            Col_24__Row_14__UL = &H945D

            Col_24__Row_15__NoUL = &H947C
            Col_24__Row_15__UL = &H94FD


            Col_28__Row_1__NoUL = &H915E
            Col_28__Row_1__UL = &H91DF

            Col_28__Row_2__NoUL = &H91FE
            Col_28__Row_2__UL = &H917F

            Col_28__Row_3__NoUL = &H925E
            Col_28__Row_3__UL = &H92DF

            Col_28__Row_4__NoUL = &H92FE
            Col_28__Row_4__UL = &H927F

            Col_28__Row_5__NoUL = &H155E
            Col_28__Row_5__UL = &H15DF

            Col_28__Row_6__NoUL = &H15FE
            Col_28__Row_6__UL = &H157F

            Col_28__Row_7__NoUL = &H165E
            Col_28__Row_7__UL = &H16DF

            Col_28__Row_8__NoUL = &H16FE
            Col_28__Row_8__UL = &H167F

            Col_28__Row_9__NoUL = &H975E
            Col_28__Row_9__UL = &H97DF

            Col_28__Row_10__NoUL = &H97FE
            Col_28__Row_10__UL = &H977F

            Col_28__Row_11__NoUL = &H105E
            Col_28__Row_11__UL = &H10DF

            Col_28__Row_12__NoUL = &H135E
            Col_28__Row_12__UL = &H13DF

            Col_28__Row_13__NoUL = &H13FE
            Col_28__Row_13__UL = &H137F

            Col_28__Row_14__NoUL = &H945E
            Col_28__Row_14__UL = &H94DF

            Col_28__Row_15__NoUL = &H94FE
            Col_28__Row_15__UL = &H947F

        End Enum

        Public Enum eMysteryCombos
            Myst1 = &HF89E
            Myst2 = &HF81D
            Myst3 = &H9191
        End Enum

        'HELPER FUNCTIONS

        Public Function DecimalToBinary(ByVal DecimalNum As Long) As String
            Dim tmp As String
            Dim n As Long
            n = DecimalNum
            tmp = Trim(Str(n Mod 2))
            n = n \ 2
            Do While n <> 0
                tmp = Trim(Str(n Mod 2)) & tmp
                n = n \ 2
            Loop
            Return tmp
        End Function

        Public Function PadString(ByVal sText As String, ByVal iSize As Integer, ByVal sChar As String, ByVal PadLeft As Boolean) As String
            Dim CharactersToAdd As Integer = iSize - sText.Length
            If CharactersToAdd < 0 Then Return sText
            For i As Short = 1 To CharactersToAdd
                If PadLeft Then
                    sText = sChar & sText
                Else
                    sText &= sChar
                End If
            Next
            Return sText
        End Function

        Public Function BinaryToDecimal(ByVal Binary As String) As Long
            Try
                Dim n As Long
                Dim s As Integer
                For s = 1 To Len(Binary)
                    n = n + (Mid(Binary, Len(Binary) - s + 1, 1) * (2 ^ (s - 1)))
                Next s
                Return n
            Catch ex As Exception
                Throw New Exception("Problem with BinaryToDecimal. Error: " & ex.Message)
            End Try
        End Function

        Public Function GetByte(ByVal InByte As Byte) As String
            Dim O As String = Hex(InByte)
            If O.Length = 1 Then O = "0" & O
            Return O
        End Function

    End Class 'Line21

End Namespace 'DVD
