Imports SMT.DotNet.Utility

Namespace Multimedia.Formats.AC3

    Public Module mAC3

        Public Function GetAC3ChannelMappingFromAppModeData(ByVal AMD As Integer) As String
            Try
                Select Case CInt(DecToBin(AMD))
                    Case 0
                        Return "Ch1,Ch2"
                    Case 1
                        Return "C"
                    Case 10
                        Return "L,R"
                    Case 11
                        Return "L,C,R"
                    Case 100
                        Return "L,R,S"
                    Case 101
                        Return "L,C,R,S"
                    Case 110
                        Return "L,R,SL,SR"
                    Case 111
                        Return "L,C,R,SL,SR"
                    Case Else
                        Return "U/A"
                End Select

            Catch ex As Exception
                Throw New Exception("Problem with GetAC3ChannelMappingFromAppModeData. Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function GetAC3MainChannelCount(ByVal ACMOD As Integer) As Integer
            Try
                Select Case ACMOD
                    Case 0
                        Return 2
                    Case 1
                        Return 1
                    Case 2
                        Return 2
                    Case 3
                        Return 3
                    Case 4
                        Return 3
                    Case 5
                        Return 4
                    Case 6
                        Return 4
                    Case 7
                        Return 5
                End Select
            Catch ex As Exception
                Throw New Exception("Problem with GetAC3MainChannelCount(). Error: " & ex.Message)
            End Try
        End Function

        Public Class AC3Header
            Public Bitrate As Short
            Public SampleRate As Double
            Public BitStreamMode As String
            Public AudioCodingMode As String
            Public CenterMixLevel As String
            Public SurroundMixLevel As String
            Public DolbySurroundLevel As String
            Public LFE As Boolean
            Public DialogueNormalization As String
            Public CompressionGain As String
            Public LanguageCode As String
            Public MixingLevel As String
            Public RoomType As String
            Public Copyrighted As Boolean
            Public OriginalBitStream As Boolean

            'utility
            Public FrameSize As Long

            Public Sub New()
            End Sub

            Public Sub New(ByVal Bytes() As Byte)
                Try
                    'if it doesn't start with 0b 77 then there's a prob

                    Dim s As String = DecimalToBinary(Bytes(4))
                    s = PadString(s, 8, "0", True)
                    Dim fscod As String = Microsoft.VisualBasic.Left(s, 2)
                    Dim frmsizecod As String = Microsoft.VisualBasic.Right(s, 6)
                    Select Case fscod
                        Case "00"
                            SampleRate = 48
                        Case "01"
                            SampleRate = 44.1
                        Case "10"
                            SampleRate = 32
                        Case "11"
                            SampleRate = -1
                    End Select

                    FrameSize = GetFrameSize(SampleRate, frmsizecod)
                    Bitrate = GetBitrate(frmsizecod)

                    'Get BitStreamMode and AudioCodingMode (they are tied togther so do them together)
                    s = DecimalToBinary(Bytes(5))
                    s = PadString(s, 8, "0", True)
                    Dim bsmod As String = Microsoft.VisualBasic.Right(s, 3)

                    s = DecimalToBinary(Bytes(6))
                    s = PadString(s, 8, "0", True)
                    Dim acmod As String = Microsoft.VisualBasic.Left(s, 3)

                    Select Case bsmod
                        Case "000"
                            Me.BitStreamMode = "Complete"
                        Case "001"
                            Me.BitStreamMode = "Music & Effects"
                        Case "010"
                            Me.BitStreamMode = "Visually Impaired"
                        Case "011"
                            Me.BitStreamMode = "Hearing Impaired"
                        Case "100"
                            Me.BitStreamMode = "Dialogue"
                        Case "101"
                            Me.BitStreamMode = "Commentary"
                        Case "110"
                            Me.BitStreamMode = "Emergency"
                        Case "111"
                            If acmod = "001" Then
                                Me.BitStreamMode = "Voice Over"
                            Else
                                Me.BitStreamMode = "Karaoke"
                            End If
                    End Select

                    Select Case acmod
                        Case "000"
                            Me.AudioCodingMode = "1+1 - 2ch - Ch1,Ch2"
                        Case "001"
                            Me.AudioCodingMode = "1/0 - 1ch - C"
                        Case "010"
                            Me.AudioCodingMode = "2/0 - 2ch - L,R"
                        Case "011"
                            Me.AudioCodingMode = "3/0 - 3ch - L,C,R"
                        Case "100"
                            Me.AudioCodingMode = "2/1 - 3ch - L,R,S"
                        Case "101"
                            Me.AudioCodingMode = "3/1 - 4ch - L,C,R,S"
                        Case "110"
                            Me.AudioCodingMode = "2/2 - 4ch - L,R,SL,SR"
                        Case "111"
                            Me.AudioCodingMode = "3/2 - 5ch - L,C,R,SL,SR"
                    End Select

                    'Center Mix Level
                    Dim cmixlev As String = Microsoft.VisualBasic.Mid(s, 4, 2)
                    Select Case cmixlev
                        Case "00"
                            Me.CenterMixLevel = "-3.0db"
                        Case "01"
                            Me.CenterMixLevel = "-4.5db"
                        Case "10"
                            Me.CenterMixLevel = "-6.0db"
                        Case "11"
                            Me.CenterMixLevel = "R -4.5db"
                    End Select

                    'Surround Mix Level
                    Dim surmixlev As String = Microsoft.VisualBasic.Mid(s, 6, 2)
                    Select Case surmixlev
                        Case "00"
                            Me.SurroundMixLevel = "-3db"
                        Case "01"
                            Me.SurroundMixLevel = "-6db"
                        Case "10"
                            Me.SurroundMixLevel = "0"
                        Case "11"
                            Me.SurroundMixLevel = "R -6db"
                    End Select

                    'Dolby Surround Mode
                    Dim tBit As String = Microsoft.VisualBasic.Right(s, 1)

                    s = DecimalToBinary(Bytes(7))
                    s = PadString(s, 8, "0", True)
                    Dim dsurmod As String = tBit & Microsoft.VisualBasic.Left(s, 1)
                    Select Case dsurmod
                        Case "00"
                            Me.DolbySurroundLevel = "Not Indicated"
                        Case "01"
                            Me.DolbySurroundLevel = "Not Dolby Sur."
                        Case "10"
                            Me.DolbySurroundLevel = "Dolby Surround"
                        Case "11"
                            Me.DolbySurroundLevel = "R - Not Indicated"
                    End Select

                    'LFE
                    Dim lfeon As String = Microsoft.VisualBasic.Mid(s, 2, 1)
                    If lfeon = "0" Then
                        LFE = False
                    Else
                        LFE = True
                    End If

                    'Dialogue Normalization
                    Dim dialnorm As String = Microsoft.VisualBasic.Mid(s, 3, 5)
                    If CShort(dialnorm) = 0 Then
                        Me.DialogueNormalization = "-31db"
                    Else
                        Me.DialogueNormalization = "-" & CStr(BinToDec(dialnorm)) & "db"
                    End If

                    '=====================
                    'Going into the contingint stuff
                    '====================
                    Dim LangCodeAddress As String
                    Dim AudioProdInfoAddress As String

                    'Compression Gain
                    Dim compre As String = Microsoft.VisualBasic.Right(s, 1)
                    If compre = "1" Then
                        'means that byte 8 is the compression gain info
                        s = DecimalToBinary(Bytes(8))
                        s = PadString(s, 8, "0", True)
                        Dim compr As String = s
                        Me.CompressionGain = "TBD"
                        LangCodeAddress = "Byte 9"
                    Else
                        'no compression gain information
                        Me.CompressionGain = "N/A"
                        LangCodeAddress = "First bit of byte 8"
                    End If

                    'Language code
                    Dim langcode As String
                    If LangCodeAddress = "Byte 9" Then
                        s = DecimalToBinary(Bytes(9))
                        s = PadString(s, 8, "0", True)
                        langcode = Microsoft.VisualBasic.Left(s, 1)
                    Else
                        'langcode is in the first bit of byte 8
                        s = DecimalToBinary(Bytes(8))
                        s = PadString(s, 8, "0", True)
                        langcode = Microsoft.VisualBasic.Left(s, 1)
                    End If

                    If langcode = "1" Then
                        'means that the rest of byte 8 and the first bit of byte nine are langcod
                        Me.LanguageCode = "Reserved"
                        AudioProdInfoAddress = "second bit of byte 10"
                    Else
                        'means that the next bit of byte 9 is audprodie
                        Me.LanguageCode = "N/A"
                        AudioProdInfoAddress = "second bit of 9"
                    End If

                    'AudioProductionInfo
                    Dim audprodie As String
                    If AudioProdInfoAddress = "second bit of byte 10" Then
                        s = DecimalToBinary(Bytes(10))
                        s = PadString(s, 8, "0", True)
                        audprodie = Microsoft.VisualBasic.Mid(s, 2, 1)
                        If audprodie = "1" Then
                            Dim mixlevel As String = Microsoft.VisualBasic.Mid(s, 3, 5)
                            Me.MixingLevel = BinToDec(mixlevel)
                            Me.RoomType = "TBD"
                        Else
                            Me.MixingLevel = "N/A"
                            Me.RoomType = "N/A"
                        End If
                    Else
                        s = DecimalToBinary(Bytes(9))
                        s = PadString(s, 8, "0", True)
                        audprodie = Microsoft.VisualBasic.Mid(s, 2, 1)
                        If audprodie = "1" Then
                            Dim mixlevel As String = Microsoft.VisualBasic.Mid(s, 3, 5)
                            Me.MixingLevel = BinToDec(mixlevel)
                            Me.RoomType = "TBD"
                        Else
                            Me.MixingLevel = "N/A"
                            Me.RoomType = "N/A"
                        End If
                    End If

                    'TODO: these
                    Me.Copyrighted = True
                    Me.OriginalBitStream = True


                    '                    Dim FrameBuff() As Byte
                    '                    Dim sCur As Long = 0
                    'NewAC3SyncFrame:
                    '                    ReDim FrameBuff(FrameSize - 1)
                    '                    Array.Copy(Bytes, sCur, FrameBuff, 0, FrameSize)
                    '                    sCur += FrameSize

                    '                    For i As Integer = sCur To UBound(Bytes)
                    '                        If Bytes(i) = 11 Then
                    '                            If Bytes(i + 1) = 119 Then
                    '                                'we've found a new sync frame
                    '                                sCur = i
                    '                                GoTo Newac3SyncFrame
                    '                            End If
                    '                        End If
                    '                    Next
                Catch ex As Exception
                    Throw New Exception("Problem with New in AC3 file handling. Error: " & ex.Message)
                End Try
            End Sub

        End Class

        Public Function GetFrameSize(ByVal SampleRate As Short, ByVal FrameSizeBits As String) As Long
            Try
                Dim FrameSize_bin As Short = BinToDec(FrameSizeBits)
                Select Case SampleRate
                    Case 48
                        Select Case FrameSize_bin
                            Case 0, 1
                                Return 112
                            Case 2, 3
                                Return 160
                            Case 4, 5
                                Return 192
                            Case 6, 7
                                Return 224
                            Case 8, 9
                                Return 256
                            Case 10, 11
                                Return 320
                            Case 12, 13
                                Return 384
                            Case 14, 15
                                Return 448
                            Case 16, 17
                                Return 512
                            Case 18, 19
                                Return 640
                            Case 20, 21
                                Return 768
                            Case 22, 23
                                Return 896
                            Case 24, 25
                                Return 1024
                            Case 26, 27
                                Return 1280
                            Case 28, 29
                                Return 1536
                            Case 30, 31
                                Return 1792
                            Case 32, 33
                                Return 2048
                            Case 34, 35
                                Return 2304
                            Case 36, 37
                                Return 2650
                        End Select
                    Case 32
                        Select Case FrameSize_bin
                            Case 0, 1
                                Return 192
                            Case 2, 3
                                Return 240
                            Case 4, 5
                                Return 288
                            Case 6, 7
                                Return 336
                            Case 8, 9
                                Return 384
                            Case 10, 11
                                Return 480
                            Case 12, 13
                                Return 576
                            Case 14, 15
                                Return 672
                            Case 16, 17
                                Return 768
                            Case 18, 19
                                Return 960
                            Case 20, 21
                                Return 1152
                            Case 22, 23
                                Return 1344
                            Case 24, 25
                                Return 1536
                            Case 26, 27
                                Return 1920
                            Case 28, 29
                                Return 2304
                            Case 30, 31
                                Return 2688
                            Case 32, 33
                                Return 3072
                            Case 34, 35
                                Return 3456
                            Case 36, 37
                                Return 3840
                        End Select
                    Case 44.1
                        Select Case FrameSize_bin
                            Case 0
                                Return 138
                            Case 1
                                Return 140
                            Case 2
                                Return 174
                            Case 3
                                Return 176
                            Case 4
                                Return 208
                            Case 5
                                Return 210
                            Case 6
                                Return 242
                            Case 7
                                Return 244
                            Case 8
                                Return 278
                            Case 9
                                Return 280
                            Case 10
                                Return 348
                            Case 11
                                Return 350
                            Case 12
                                Return 416
                            Case 13
                                Return 418
                            Case 14
                                Return 486
                            Case 15
                                Return 488
                            Case 16
                                Return 556
                            Case 17
                                Return 558
                            Case 18
                                Return 696
                            Case 19
                                Return 698
                            Case 20
                                Return 834
                            Case 21
                                Return 836
                            Case 22
                                Return 974
                            Case 23
                                Return 976
                            Case 24
                                Return 1114
                            Case 25
                                Return 1116
                            Case 26
                                Return 1392
                            Case 27
                                Return 1394
                            Case 28
                                Return 1670
                            Case 29
                                Return 1672
                            Case 30
                                Return 1950
                            Case 31
                                Return 1952
                            Case 32
                                Return 2228
                            Case 33
                                Return 2230
                            Case 34
                                Return 2506
                            Case 35
                                Return 2508
                            Case 36
                                Return 2786
                            Case 37
                                Return 2788
                        End Select
                End Select
            Catch ex As Exception
                Throw New Exception("Problem with GetAC3FrameSize. Error: " & ex.Message)
            End Try
        End Function

        Public Function GetBitrate(ByVal FrameSizeBits As String) As Long
            Try
                Dim FrameSize_bin As Short = BinToDec(FrameSizeBits)
                Select Case FrameSize_bin
                    Case 0, 1
                        Return 32
                    Case 2, 3
                        Return 40
                    Case 4, 5
                        Return 48
                    Case 6, 7
                        Return 56
                    Case 8, 9
                        Return 64
                    Case 10, 11
                        Return 80
                    Case 12, 13
                        Return 96
                    Case 14, 15
                        Return 112
                    Case 16, 17
                        Return 128
                    Case 18, 19
                        Return 160
                    Case 20, 21
                        Return 192
                    Case 22, 23
                        Return 224
                    Case 24, 25
                        Return 256
                    Case 26, 27
                        Return 320
                    Case 28, 29
                        Return 384
                    Case 30, 31
                        Return 448
                    Case 32, 33
                        Return 512
                    Case 34, 35
                        Return 576
                    Case 36, 37
                        Return 640
                End Select
            Catch ex As Exception
                Throw New Exception("Problem with GetAC3Bitrate. Error: " & ex.Message)
            End Try
        End Function

    End Module

    Public Class cAC3SyncInfo
        Public SampleRate As Double
        Public FrameSize As Long
        Public Bitrate As Short

        Public Sub New()
        End Sub

        Public Sub New(ByVal HeaderBytes() As Byte)
            Try
                Dim s As String = DecimalToBinary(HeaderBytes(4))
                s = PadString(s, 8, "0", True)
                Dim fscod As String = Microsoft.VisualBasic.Left(s, 2)
                Dim frmsizecod As String = Microsoft.VisualBasic.Right(s, 6)
                Select Case fscod
                    Case "00"
                        SampleRate = 48
                    Case "01"
                        SampleRate = 44.1
                    Case "10"
                        SampleRate = 32
                    Case "11"
                        SampleRate = -1
                End Select

                FrameSize = GetFrameSize(SampleRate, frmsizecod)
                Bitrate = GetBitrate(frmsizecod)
            Catch ex As Exception
                Throw New Exception("Problem with GetAC3FrameSize. Error: " & ex.Message)
            End Try
        End Sub

    End Class

End Namespace
