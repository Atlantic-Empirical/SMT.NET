Imports SMT.DotNet.Utility
Imports System.IO

Namespace Multimedia.Formats.MPEG2

    Public Class cParseMPEG2

        Public StopProcessing As Boolean = False

        Public Event evGOPUserData(ByVal user_data() As Byte)

        Private WithEvents FB As cFileBuffer

        Public Function ParseMPEG2(ByVal b() As Byte) As cMPEG2Header
            Try
                Dim Out As New cMPEG2Header
                Dim ba As New cSeqBitArray(b)
                Dim i As Integer = 0

                'GO TO NEXT START CODE
GoToNextStartCode:
                While ba.GetBits(i, 24) <> "000000000000000000000001"
                    i += 1
                    If i + 24 > ba.SBA.Count Then GoTo WereDone
                    If i > 8096 Then GoTo WereDone
                End While

                'Ok, what kind of start code is it?
                Select Case ba.GetBits(i, 32)
                    Case "00000000000000000000000110110010" 'user_data_start_code = 000001B2 = 110110010
                        'we're not going to do anything with user data
                        i += 1
                        GoTo GoToNextStartCode

                    Case "00000000000000000000000110111000" 'group_start_code = 000001B8 = 110111000
                        Out.GOPHeader = Me.GetGOPHeader(i, ba)
                        GoTo GoToNextStartCode

                    Case "00000000000000000000000110110011" 'sequence_header_code = 000001B3 = 110110011
                        Out.SequenceHeader = Me.GetSequenceHeader(i, ba)
                        GoTo GoToNextStartCode

                    Case "00000000000000000000000100000000" 'picture header = 00000100 = 100000000
                        Out.PictureHeader = Me.GetPictureHeader(i, ba)
                        GoTo GoToNextStartCode

                    Case "00000000000000000000000110110101" 'extension_start_code = 000001B5 = 110110101
                        Select Case ba.GetBits(i + 32, 4)
                            Case "0010"
                                Out.SequenceDisplayExtension = Me.GetDisplayExtension(i, ba)
                                GoTo GoToNextStartCode
                            Case "0001"
                                Out.SequenceExtension = Me.GetSequenceExtension(i, ba)
                                GoTo GoToNextStartCode
                            Case "1000"
                                Out.PictureCodingExtension = Me.GetPictureCodingExtension(i, ba)
                                GoTo GoToNextStartCode
                            Case Else
                                i += 1
                                GoTo GoToNextStartCode
                        End Select

                    Case Else
                        i += 1
                        GoTo GoToNextStartCode
                End Select
WereDone:
                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with ParseMPEG2. Error: " & ex.Message)
            End Try
        End Function

        Public Function ParseForGOPUserData(ByVal FilePath As String) As Boolean
            Try
                If Not File.Exists(FilePath) Then Return False

                Dim user_data() As Byte
                Dim ElementaryStream As Boolean = Path.GetExtension(FilePath) = ".m2v" Or Path.GetExtension(FilePath) = ".mpv"

                FB = New cFileBuffer(FilePath, 50000000)

                Dim GOP_user_data_start_code() As Byte = {0, 0, 1, &HB2}

                Do
                    If Me.StopProcessing Then
                        Me.StopProcessing = False
                        GoTo ExitSuccess
                    End If
                    If Me.SeekToStartCode(GOP_user_data_start_code, FB, ElementaryStream) Then
                        ReDim user_data(119)
                        user_data = FB.ReadBytes(120)
                        RaiseEvent evGOPUserData(user_data)
                    Else
                        GoTo ExitSuccess 'end of stream or start code not found
                    End If
                Loop
ExitSuccess:
                FB.Close()
                Return True 'End of stream
            Catch ex As Exception
                Throw New Exception("Problem with ParseForGOPUserData(). Error: " & ex.Message)
            End Try
        End Function

        Private Function SeekToStartCode(ByVal start_code() As Byte, ByRef FB As cFileBuffer, ByVal ElementaryStream As Boolean) As Boolean
            Try
                Dim SeekCode As UInt32 = CUInt("&h" & Hex(start_code(3)).PadLeft(2, "0") & PadString(Hex(start_code(2)), 2, "0", True) & PadString(Hex(start_code(1)), 2, "0", True) & PadString(Hex(start_code(0)), 2, "0", True))
                Dim currentInt As UInt32

                If ElementaryStream Then

                    'four-byte aligned

StartOver_Elementary:
                    If StopProcessing Then Return False
                    Try
                        currentInt = FB.ReadUInt32
                        'Debug.WriteLine("currentInt=" & currentInt)
                        'Debug.WriteLine("Position=" & BR.BaseStream.Position)
                        If currentInt = SeekCode Then
                            Return True
                        Else
                            GoTo StartOver_Elementary
                        End If
                    Catch ex As EndOfStreamException
                        Return False
                    End Try

                Else

StartOver_Program:
                    If StopProcessing Then Return False
                    Try
                        currentInt = FB.ReadUInt32
                        'Debug.WriteLine("currentInt=" & currentInt)
                        'Debug.WriteLine("Position=" & BR.BaseStream.Position)
                        If currentInt = SeekCode Then
                            FB.BS.Position -= 4
                            Return True
                        Else
                            FB.BS.Position -= 3
                            GoTo StartOver_Program
                        End If
                    Catch ex As EndOfStreamException
                        Return False
                    End Try

                End If

            Catch ex As Exception
                Throw New Exception("Problem with SeekToStartCode(). Error: " & ex.Message)
            End Try
        End Function

        Public Function ParseForSequenceExtension(ByVal b() As Byte) As cSequenceExtension()
            Dim ba As New cSeqBitArray(b)
            Dim i As Integer = 0

            Dim out(-1) As cSequenceExtension

            'GO TO NEXT START CODE
GoToNextStartCode:
            While ba.GetBits(i, 24) <> "000000000000000000000001"
                i += 1
                If i + 24 > ba.SBA.Count Then GoTo WereDone
                If i > 8096 Then GoTo WereDone
            End While

            'Ok, what kind of start code is it?
            If ba.GetBits(i, 32) = "00000000000000000000000110110101" Then 'extension_start_code = 000001B5 = 110110101
                If ba.GetBits(i + 32, 4) = "0001" Then
                    ReDim Preserve out(UBound(out) + 1)
                    out(UBound(out)) = Me.GetSequenceExtension(i, ba)
                    i += 1
                    GoTo GoToNextStartCode
                Else
                    i += 1
                    GoTo GoToNextStartCode
                End If
            Else
                i += 1
                GoTo GoToNextStartCode
            End If

WereDone:
            Return out
        End Function

        Private Sub HandleEndOfFile() Handles FB.EndOfFile
            StopProcessing = True
        End Sub

        Public Class cFileBuffer

            Private FS As FileStream
            Public BS As BufferedStream
            'Private Q As Queue
            Public ChunkSize As Integer

            'Private C As colBytes

            Private Pos As Long
            Private Length As Long

            Public Event EndOfFile()

            Public Sub New(ByVal FilePath As String, ByVal nChunkSize As Integer)
                Try
                    FS = New FileStream(FilePath, FileMode.Open)
                    BS = New BufferedStream(FS, nChunkSize)
                    ChunkSize = nChunkSize
                    Pos = 0
                    Length = BS.Length

                    'Dim Buffer(nChunkSize - 1) As Byte
                    'FS.Read(Buffer, 0, nChunkSize)

                    ''QueueSize = nQueueSize
                    ''Q = New Queue
                    ''For Each bt As Byte In b
                    ''    Q.Enqueue(bt)
                    ''Next

                    'ChunkSize = nChunkSize
                    'C = New colBytes
                    'For Each B As Byte In Buffer
                    '    C.Add(B)
                    'Next
                Catch ex As Exception
                    Throw New Exception("Problem with New(). Error: " & ex.Message, ex)
                End Try
            End Sub

            Public Sub Close()
                If FS IsNot Nothing Then
                    BS.Close()
                    FS.Close()
                    BS.Dispose()
                    FS.Dispose()
                End If
            End Sub

            'Private Function RepopulateQ() As Boolean
            '    Try
            '        'C should be empty now
            '        Dim Count As Integer = ChunkSize
            '        If FS.Length - FS.Position < ChunkSize Then
            '            Count = FS.Length - FS.Position
            '            RaiseEvent EndOfFile()
            '            Return False
            '        End If
            '        Dim Buffer(Count - 1) As Byte
            '        FS.Read(Buffer, 0, Count)
            '        For Each B As Byte In Buffer
            '            C.Add(B)
            '        Next
            '        Pos = 0
            '        Return True
            '    Catch ex As Exception
            '        Throw New Exception("Problem with RepopulateQ(). Error: " & ex.Message, ex)
            '    End Try
            'End Function

            Private Function EOFCheck() As Boolean
                If Pos >= Length Then
                    RaiseEvent EndOfFile()
                    Return True
                Else
                    Return False
                End If
            End Function

            Public Function ReadByte() As Byte
                Try
                    'If Pos <= 1 Then RepopulateQ()
                    'Return CByte(Q.Dequeue)
                    If EOFCheck() Then Return Nothing
                    Pos += 1
                    Return BS.ReadByte
                Catch ex As Exception
                    Throw New Exception("Problem with ReadByte(). Error: " & ex.Message, ex)
                End Try
            End Function

            Public Function ReadByteBinary() As String
                Try
                    'If Q.Count < 1 Then RepopulateQ()
                    'Dim b As Byte = CByte(Q.Dequeue)
                    'Return DecToBin(b)
                    If EOFCheck() Then Return Nothing
                    Pos += 1
                    Return DecToBin(BS.ReadByte)
                Catch ex As Exception
                    Throw New Exception("Problem with ReadByteBinary(). Error: " & ex.Message, ex)
                End Try
            End Function

            Public Function ReadByteHalf(ByVal Upper As Boolean) As Byte
                Try
                    'If Q.Count < 1 Then RepopulateQ()
                    'Dim b As Byte = CByte(Q.Peek)
                    'If Upper Then
                    '    Return b >> 4
                    'Else
                    '    Return b & 4
                    'End If
                    If EOFCheck() Then Return Nothing
                    Pos += 1
                    Dim b As Byte = BS.ReadByte
                    If Upper Then
                        Return b >> 4
                    Else
                        Return b & 4
                    End If
                Catch ex As Exception
                    Throw New Exception("Problem with ReadByteHalf(). Error: " & ex.Message)
                End Try
            End Function

            Public Function ReadBytes(ByVal Count As Integer) As Byte()
                Try
                    'If Q.Count < Count Then RepopulateQ()
                    'Dim b(Count - 1) As Byte
                    'For i As Integer = 0 To Count - 1
                    '    b(i) = CByte(Q.Dequeue)
                    'Next
                    'Return b
                    If EOFCheck() Then Return Nothing
                    Pos += Count
                    Dim b(Count - 1) As Byte
                    BS.Read(b, 0, Count)
                    Return b
                Catch ex As Exception
                    Throw New Exception("Problem with ReadBytes(). Error: " & ex.Message)
                End Try
            End Function

            Public Function ReadUInt32() As UInt32
                Try
                    Dim bytes() As Byte = Me.ReadBytes(4)
                    If bytes Is Nothing Then Return Nothing
                    Dim tInt As ULong = bytes(3)
                    Dim out As ULong = (tInt << 24)
                    tInt = bytes(2)
                    out = out Or (tInt << 16)
                    tInt = bytes(1)
                    out = out Or (tInt << 8)
                    tInt = bytes(0)
                    out = out Or tInt
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with ReadUInt(). Problem: " & ex.Message, ex)
                End Try
            End Function

            'Public Property Position() As Long
            '    Get
            '        Return _Position
            '    End Get
            '    Set(ByVal Value As Long)
            '        _Position = Value
            '    End Set
            'End Property

        End Class

        Public Sub newParseForPictureExtension(ByVal FilePath As String)
            Try
                Dim FP As New cFileBuffer(FilePath, 40000)
                Dim PCT As cPictureCodingExtension
                Dim Cnt As Short = 0
[Continue]:
                If FP.ReadByte = 1 Then
                    If FP.ReadByte = &HB5 Then
                        If FP.ReadByteHalf(True) = 8 Then
                            'Debug.WriteLine("We've got a picture extension header")
                            PCT = Me.GetPictureCodingExtension2(FP)
                            Debug.WriteLine("Frame: " & Cnt & " RFF: " & PCT.repeat_first_field & " TFF: " & PCT.top_field_first & " PF: " & PCT.progressive_frame & " FS: " & PCT.field_sequence1 & PCT.field_sequence2 & " PS: " & PCT.picture_structureI & " FC: " & PCT.f_code_1 & ":" & PCT.f_code_2 & ":" & PCT.f_code_3 & ":" & PCT.f_code_4)
                            Cnt += 1
                        End If
                    End If
                End If
                GoTo [Continue] 'always leave this at the byte after the area we've just been messing with
            Catch ex As Exception
                Throw New Exception("Prob: " & ex.Message)
            End Try
        End Sub

        Public Function ParseForPictureExtension(ByVal b() As Byte) As cPictureCodingExtension()
            Dim ba As New cSeqBitArray(b)
            Dim i As Integer = 0

            Dim out(-1) As cPictureCodingExtension

            Dim PCT As cPictureCodingExtension

            'GO TO NEXT START CODE
GoToNextStartCode:
            While ba.GetBits(i, 24) <> "000000000000000000000001"
                i += 1
                If i + 24 > ba.SBA.Count Then GoTo WereDone
                If i > 8096 Then GoTo WereDone
            End While

            'Ok, what kind of start code is it?
            If ba.GetBits(i, 32) = "00000000000000000000000110110101" Then 'extension_start_code = 000001B5 = 110110101
                If ba.GetBits(i + 32, 4) = "1000" Then
                    ReDim Preserve out(UBound(out) + 1)
                    PCT = Me.GetPictureCodingExtension(i, ba)
                    out(UBound(out)) = PCT
                    Debug.WriteLine("RFF: " & PCT.repeat_first_field & " TFF: " & PCT.top_field_first & " PF: " & PCT.progressive_frame)
                    i += 1
                    GoTo GoToNextStartCode
                Else
                    i += 1
                    GoTo GoToNextStartCode
                End If
            Else
                i += 1
                GoTo GoToNextStartCode
            End If

WereDone:
            Return out
        End Function

        Public Function GetSequenceHeader(ByRef i As Integer, ByRef ba As cSeqBitArray) As cSequenceHeader
            Try
                i += 32

                Dim Out As New cSequenceHeader
                Out.horizontal_size_value = BinToDec(ba.GetBits(i, 12))
                i += 12
                Out.vertical_size_value = BinToDec(ba.GetBits(i, 12))
                i += 12

                'aspect_ratio_information
                Dim ti As Short = BinToDec(ba.GetBits(i, 4))
                Select Case ti
                    Case 0
                        Out.aspect_ratio_information = "Forbidden"
                    Case 1
                        Out.aspect_ratio_information = "1,0 Square Sample"
                    Case 2
                        Out.aspect_ratio_information = "4x3"
                    Case 3
                        Out.aspect_ratio_information = "16x9"
                    Case 4
                        Out.aspect_ratio_information = "2.21x1"
                    Case Else
                        Out.aspect_ratio_information = "Reserved"
                End Select
                i += 4

                'frame_rate_code
                ti = BinToDec(ba.GetBits(i, 4))
                Select Case ti
                    Case 0
                        Out.frame_rate_code = "Forbidden"
                    Case 1
                        Out.frame_rate_code = "23.976"
                    Case 2
                        Out.frame_rate_code = "24"
                    Case 3
                        Out.frame_rate_code = "25"
                    Case 4
                        Out.frame_rate_code = "29.97"
                    Case 5
                        Out.frame_rate_code = "30"
                    Case 6
                        Out.frame_rate_code = "50"
                    Case 7
                        Out.frame_rate_code = "59.94"
                    Case 8
                        Out.frame_rate_code = "60"
                    Case Else
                        Out.frame_rate_code = "Reserved"
                End Select
                i += 4

                'bit_rate_value
                Out.bit_rate = BinToDec(ba.GetBits(i, 18))
                i += 18

                'marker_bit
                i += 1

                'vbv_buffer_size_value
                Out.vbv_buffer_size_value = BinToDec(ba.GetBits(i, 10))
                i += 10

                'constrained_parameters_flag
                Out.constrainded_parameters_flag = BinToDec(ba.GetBits(i, 1))
                i += 1

                'load_intra_quantiser_matrix
                Out.load_intra_quantiser_matrix = BinToDec(ba.GetBits(i, 1))
                i += 1

                If Out.load_intra_quantiser_matrix Then
                    'intra_quantiser_matrix
                    Out.intra_quantiser_matrix = ba.GetBytes(i, 512)
                    i += 512
                End If

                'load_non_intra_quantiser_matrix
                Out.load_non_intra_quantiser_matrix = BinToDec(ba.GetBits(i, 1))
                i += 1

                If Out.load_non_intra_quantiser_matrix Then
                    'non_intra_quantiser_matrix
                    Out.non_intra_quantiser_matrix = ba.GetBytes(i, 512)
                    i += 512
                End If

                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with GetSequenceHeader. Error: " & ex.Message)
            End Try
        End Function

        Public Function GetSequenceExtension(ByRef i As Integer, ByRef ba As cSeqBitArray) As cSequenceExtension
            Try
                Dim Out As New cSequenceExtension

                'extension_start_code
                i += 32

                'extension_start_code_identifier
                Out.extension_start_code_identifier = BinToDec(ba.GetBits(i, 4))
                i += 4

                'profile_and_level_indication
                Out.profile_and_level_indication = BinToDec(ba.GetBits(i, 4))
                i += 1 'skip escape bit
                Dim s As String = ba.GetBits(i, 3)
                Select Case s
                    Case "101"
                        Out.Profile = "Simple"
                    Case "100"
                        Out.Profile = "Main"
                    Case "011"
                        Out.Profile = "SNR Scalable"
                    Case "010"
                        Out.Profile = "Spatially Scalable"
                    Case "001"
                        Out.Profile = "High"
                    Case Else
                        Out.Profile = "Reserved"
                End Select

                i += 3
                s = ba.GetBits(i, 4)
                Select Case s
                    Case "1010"
                        Out.Level = "Low"
                    Case "1000"
                        Out.Level = "Main"
                    Case "0110"
                        Out.Level = "High 1440"
                    Case "0100"
                        Out.Level = "High"
                    Case Else
                        Out.Level = "Reserved"
                End Select
                i += 4

                'progressive_sequence
                Out.progressive_sequence = BinToDec(ba.GetBits(i, 1))
                i += 1

                'chroma_format
                Select Case ba.GetBits(i, 2)
                    Case "00"
                        Out.chroma_format = "Reserved"
                    Case "01"
                        Out.chroma_format = "4:2:0"
                    Case "10"
                        Out.chroma_format = "4:2:2"
                    Case "11"
                        Out.chroma_format = "4:4:4"
                End Select
                i += 2

                'horizontal_size_extension
                Out.horizontal_size_extension = BinToDec(ba.GetBits(i, 2))
                i += 2

                'vertical_size_extension
                Out.vertical_size_extension = BinToDec(ba.GetBits(i, 2))
                i += 2

                'bit_rate_extension
                Out.bit_rate_extension = BinToDec(ba.GetBits(i, 12))
                i += 12

                'marker_bit
                i += 1

                'vbv_buffer_size_extension
                Out.vbv_buffer_size_extension = BinToDec(ba.GetBits(i, 8))
                i += 8

                'low_delay
                Out.low_delay = BinToDec(ba.GetBits(i, 1))
                i += 1

                'frame_rate_extension_n
                Out.frame_rate_extension_n = BinToDec(ba.GetBits(i, 2))
                i += 2

                'frame_rate_extension_d
                Out.frame_rate_extension_d = BinToDec(ba.GetBits(i, 2))
                i += 2

                'bump up to round byte
                i += 3

                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with GetSequenceExtension. Error: " & ex.Message)
            End Try
        End Function

        Public Function GetDisplayExtension(ByRef i As Integer, ByRef ba As cSeqBitArray) As cSequenceDisplayExtension
            Try
                Dim Out As New cSequenceDisplayExtension

                'extension_start_code
                i += 32

                'extension_start_code_identifier
                Out.extension_start_code_identifier = BinToDec(ba.GetBits(i, 4))
                i += 4

                'video_format
                Select Case ba.GetBits(i, 3)
                    Case "000"
                        Out.video_format = "Component"
                    Case "001"
                        Out.video_format = "PAL"
                    Case "010"
                        Out.video_format = "NTSC"
                    Case "011"
                        Out.video_format = "SECAM"
                    Case "100"
                        Out.video_format = "MAC"
                    Case "101"
                        Out.video_format = "Unspecified"
                    Case Else
                        Out.video_format = "Reserved"
                End Select
                i += 3

                'color_description
                Out.color_description = BinToDec(ba.GetBits(i, 1))
                i += 1

                If Out.color_description Then
                    'color_primaries
                    Select Case BinToDec(ba.GetBits(i, 8))
                        Case 0
                            Out.color_primaries = "Forbidden"
                        Case 1
                            Out.color_primaries = "ITU-R BT.709"
                        Case 2
                            Out.color_primaries = "Unspecified Video"
                        Case 4
                            Out.color_primaries = "ITU-R BT.470-2 System M"
                        Case 5
                            Out.color_primaries = "ITU-R BT.470-2 System B,G"
                        Case 6
                            Out.color_primaries = "SMPTE 170M"
                        Case 7
                            Out.color_primaries = "SMPTE 240M (1987)"
                        Case Else
                            Out.color_primaries = "Reserved"
                    End Select
                    i += 8

                    'transfer_characteristics
                    Select Case BinToDec(ba.GetBits(i, 8))
                        Case 0
                            Out.transfer_characteristics = "Forbidden"
                        Case 1
                            Out.transfer_characteristics = "ITU-R BT.709"
                        Case 2
                            Out.transfer_characteristics = "Unspecified Video"
                        Case 4
                            Out.transfer_characteristics = "ITU-R BT.470-2 System M"
                        Case 5
                            Out.transfer_characteristics = "ITU-R BT.470-2 System B,G"
                        Case 6
                            Out.transfer_characteristics = "SMPTE 170M"
                        Case 7
                            Out.transfer_characteristics = "SMPTE 240M (1987)"
                        Case 8
                            Out.transfer_characteristics = "Linear Transfer Characteristics"
                        Case Else
                            Out.transfer_characteristics = "Reserved"
                    End Select
                    i += 8

                    'matrix_coeficients
                    Select Case BinToDec(ba.GetBits(i, 8))
                        Case 0
                            Out.matrix_coefficients = "Forbidden"
                        Case 1
                            Out.matrix_coefficients = "ITU-R BT.709"
                        Case 2
                            Out.matrix_coefficients = "Unspecified Video"
                        Case 4
                            Out.matrix_coefficients = "ITU-R BT.470-2 System M"
                        Case 5
                            Out.matrix_coefficients = "ITU-R BT.470-2 System B,G"
                        Case 6
                            Out.matrix_coefficients = "SMPTE 170M"
                        Case 7
                            Out.matrix_coefficients = "SMPTE 240M (1987)"
                        Case Else
                            Out.matrix_coefficients = "Reserved"
                    End Select
                    i += 8
                End If

                'display_horizontal_size
                Out.display_horizontal_size = BinToDec(ba.GetBits(i, 14))
                i += 14

                'marker_bit
                i += 1

                'display_vertical_size
                Out.display_vertical_size = BinToDec(ba.GetBits(i, 14))
                i += 14

                'bump to round byte
                i += 3

                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with GetDisplayExtension. Error: " & ex.Message)
            End Try
        End Function

        Public Function GetGOPHeader(ByRef i As Integer, ByRef ba As cSeqBitArray) As cGOPHeader
            Try
                Dim Out As New cGOPHeader

                'extension_start_code
                i += 32

                'time_code
                Out.time_code.drop_frame_flag = BinToDec(ba.GetBits(i, 1))
                i += 1
                Out.time_code.time_code_hours = BinToDec(ba.GetBits(i, 5))
                i += 5
                Out.time_code.time_code_minutes = BinToDec(ba.GetBits(i, 6))
                i += 6
                i += 1 'marker_bit
                Out.time_code.time_code_seconds = BinToDec(ba.GetBits(i, 6))
                i += 6
                Out.time_code.time_code_pictures = BinToDec(ba.GetBits(i, 6))
                i += 6

                'closed_gop
                Out.closed_gop = BinToDec(ba.GetBits(i, 1))
                i += 1

                'broken_link
                Out.broken_link = BinToDec(ba.GetBits(i, 1))

                'bump up to round byte
                i += 6

                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with GetGOPHeader. Error: " & ex.Message)
            End Try
        End Function

        Public Function GetPictureHeader(ByRef i As Integer, ByRef ba As cSeqBitArray) As cPictureHeader
            Try
                Dim Out As New cPictureHeader

                'start code
                i += 32

                'temporal_reference
                Out.temporal_reference = BinToDec(ba.GetBits(i, 10))
                i += 10

                'picture_coding_type
                Select Case ba.GetBits(i, 3)
                    Case "000"
                        Out.picture_coding_type = "Forbidden"
                    Case "001"
                        Out.picture_coding_type = "Intra-coded(I)"
                    Case "010"
                        Out.picture_coding_type = "Predictive-coded(P)"
                    Case "011"
                        Out.picture_coding_type = "Bidirectionally-predictive-coded(B)"
                    Case "100"
                        Out.picture_coding_type = "Dc intra-coded (D)"
                    Case Else
                        Out.picture_coding_type = "Reserved"
                End Select
                i += 3

                'vbv_delay
                Out.vbv_delay = BinToDec(ba.GetBits(i, 16))
                i += 16

                ''we're going to ignore the rest of the picture header for now
                ''I'm getting tired and the docs don't make too much sense

                ''Go to picture coding extension

                'While ba.GetBits(i, 32) <> "00000000000000000000000110110101"
                '    i += 1
                'End While

                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with GetPictureHeader. Error: " & ex.Message)
            End Try
        End Function

        Public Function GetPictureCodingExtension2(ByVal FP As cFileBuffer) As cPictureCodingExtension
            Try
                'THIS SEEMS TO BE WORKING WELL
                'THERE is still something wrong with this. Occassionally the f_code values are way off (not 5 or 15).

                Dim Out As New cPictureCodingExtension

                'read five bytes. 38 bits of which will be used
                Dim b() As Byte = FP.ReadBytes(5)

                'BYTE 0 'just use lower half because upper half was the picture coding extension header code
                If ((b(0) >> 4) And 15) <> 8 Then
                    Throw New Exception("somthing's wrong")
                End If
                Out.f_code_1 = b(0) And 15

                'BYTE 1
                Out.f_code_2 = (b(1) >> 4) And 15
                Out.f_code_3 = b(1) And 15

                'BYTE 2
                Out.f_code_4 = (b(2) >> 4) And 15
                Out.intra_dc_precision = (b(2) >> 2) And 3
                Out.picture_structureI = b(2) And 3

                'BYTE 3
                Out.top_field_first = (b(3) >> 7) And 1
                Out.frame_pred_frame_dct = (b(3) >> 6) And 1
                Out.concealment_motion_vectors = (b(3) >> 5) And 1
                Out.q_scale_type = (b(3) >> 4) And 1
                Out.intra_vlc_format = (b(3) >> 3) And 1
                Out.alternate_scan = (b(3) >> 2) And 1
                Out.repeat_first_field = (b(3) >> 1) And 1
                Out.chroma_420_type = b(3) And 1

                'BYTE 4
                Out.progressive_frame = (b(4) >> 7) And 1
                'Out.composite_display_flag = (b(2) >> 3) And 1
                'Out.v_axis = (b(2) >> 1) And 1
                'Out.field_sequence1 = b(2) And 1

                ''BYTE 3
                'Out.field_sequence2 = (b(3) >> 4) And 3
                'Out.sub_carrier = (b(3) >> 3) And 1

                ''forget the rest for now
                'Out.burst_amplitude = 0
                'Out.sub_carrier_phase = 0

                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with GetPictureCodingExtension. Error: " & ex.Message)
            End Try
        End Function

        Public Function GetPictureCodingExtension(ByRef i As Integer, ByRef ba As cSeqBitArray) As cPictureCodingExtension
            Try
                Dim Out As New cPictureCodingExtension

                'start code
                i += 32

                'extension_start_code_identifier
                Out.extension_start_code_identifier = BinToDec(ba.GetBits(i, 4))
                i += 4

                'f_code
                i += 16

                'intra_dc_precision
                Out.intra_dc_precision = BinToDec(ba.GetBits(i, 2))
                i += 2

                'picture_structure
                Select Case ba.GetBits(i, 2)
                    Case "00"
                        Out.picture_structure = "Reserved"
                    Case "01"
                        Out.picture_structure = "Top Field"
                    Case "10"
                        Out.picture_structure = "Bottom Field"
                    Case "11"
                        Out.picture_structure = "Frame"
                End Select
                i += 2

                'top_field_first
                Out.top_field_first = BinToDec(ba.GetBits(i, 1))
                i += 1

                'frame_pred_frame_dct
                Out.frame_pred_frame_dct = BinToDec(ba.GetBits(i, 1))
                i += 1

                'concealment_motion_vectors
                Out.concealment_motion_vectors = ba.GetBits(i, 1)
                i += 1

                'q_scale_type
                Out.q_scale_type = ba.GetBits(i, 1)
                i += 1

                'intra_vlc_format
                Out.intra_vlc_format = ba.GetBits(i, 1)
                i += 1

                'alternate_scan
                Out.alternate_scan = ba.GetBits(i, 1)
                i += 1

                'repeat_first_field
                Out.repeat_first_field = ba.GetBits(i, 1)
                i += 1

                'chroma_420_type
                Out.chroma_420_type = ba.GetBits(i, 1)
                i += 1

                'progressive_frame
                Out.progressive_frame = ba.GetBits(i, 1)
                i += 1

                'composite_display_flag
                Out.composite_display_flag = ba.GetBits(i, 1)
                i += 1

                If Out.composite_display_flag Then
                    Throw New Exception("Implement composite_display_flag conditional flag checks.")
                    'v_axis
                    'field_sequence
                    'sub_carrier
                    'burst_amplitude
                    'sub_carrier_phase
                End If

                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with GetPictureCodingExtension. Error: " & ex.Message)
            End Try
        End Function

        'Attempt to use a queue
        '        '000001B8
        '        Public Function GetGOPTimecodes(ByVal M2VPath As String, ByVal Count As Short) As cGOPTimecode()
        '            Try
        '                Dim Out(-1) As cGOPTimecode
        '                Dim FS As New FileStream(M2VPath, FileMode.Open)
        '                Dim BR As New BinaryReader(FS)
        '                Dim ba As cSeqBitArray
        '                Dim b(7) As Byte
        '                Dim q As New Queue(1000000)

        'AddBytesToQueue:
        '                Dim AtEnd As Boolean = False
        '                If FS.Position + 1000000 > FS.Length Then
        '                    'we're at the end of the file
        '                    For i As Long = FS.Position To FS.Length - 1
        '                        q.Enqueue(FS.ReadByte)
        '                    Next
        '                    AtEnd = True
        '                Else
        '                    'add another million bytes to the queue
        '                    For i As Long = FS.Position To FS.Position + 1000000
        '                        q.Enqueue(FS.ReadByte)
        '                    Next
        '                End If

        'SeekNextGOP:
        '                If q.Count < 4 Then
        '                    If AtEnd Then
        '                        GoTo WereDone
        '                    Else
        '                        GoTo AddBytesToQueue
        '                    End If
        '                End If

        '                Dim b0 As Byte = q.Dequeue
        '                If b0 = 0 Then
        '                    Dim b1 As Byte = q.Dequeue
        '                    If b1 = 0 Then
        '                        Dim b2 As Byte = q.Dequeue
        '                        If b2 = 1 Then
        '                            Dim b3 As Byte = q.Dequeue
        '                            If b3 = 184 Then
        '                                ReDim b(7)
        '                                b(0) = b0
        '                                b(1) = b1
        '                                b(2) = b2
        '                                b(3) = b3
        '                                b(4) = q.Dequeue
        '                                b(5) = q.Dequeue
        '                                b(6) = q.Dequeue
        '                                b(7) = q.Dequeue
        '                                ba = New cSeqBitArray(b)
        '                                ReDim Preserve Out(UBound(Out) + 1)
        '                                Out(UBound(Out)) = Me.GetGOPHeader(0, ba).time_code
        '                                If Out.Length >= Count Then GoTo WereDone
        '                            End If
        '                        End If
        '                    End If
        '                End If

        '                GoTo SeekNextGOP

        'WereDone:
        '                FS.Close()
        '                Return Out
        '            Catch ex As Exception
        '                Throw New Exception("Problem with GetGOPTimecodes. Error: " & ex.Message)
        '            End Try
        '        End Function

        '000001B8

        Public Function GetGOPTimecodes(ByVal M2VPath As String, ByVal Count As Short) As cGOPTimecode()
            Try
                Dim Out(-1) As cGOPTimecode
                Dim FS As New FileStream(M2VPath, FileMode.Open, FileAccess.Read)
                Dim BR As New BinaryReader(FS)
                Dim b(7) As Byte
                Dim ba As cSeqBitArray

                For i As Long = 0 To FS.Length - 1
                    ReDim b(3)
                    BR.BaseStream.Seek(i, SeekOrigin.Begin)
                    If BR.BaseStream.Position + 8 >= BR.BaseStream.Length Then
                        GoTo WereDone
                    End If
                    b = BR.ReadBytes(8)
                    If b(0) = 0 And b(1) = 0 And b(2) = 1 And b(3) = 184 Then
                        'we've got a gop header
                        ba = New cSeqBitArray(b)
                        ReDim Preserve Out(UBound(Out) + 1)
                        Out(UBound(Out)) = Me.GetGOPHeader(0, ba).time_code
                        If Out.Length >= Count Then GoTo WereDone
                    End If
                Next
WereDone:
                FS.Close()
                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with GetGOPTimecodes. Error: " & ex.Message)
            End Try
        End Function

        'another old way
        '        Public Function GetGOPTimecodes(ByVal M2VPath As String, ByVal Count As Short) As cGOPTimecode()
        '            Try
        '                Dim Out(-1) As cGOPTimecode
        '                Dim FS As New FileStream(M2VPath, FileMode.Open)
        '                Dim BR As New BinaryReader(FS)
        '                Dim ba As New cSeqBitArray

        '                ba.AddData(BR, 100000)

        '                Dim i As Integer = 0
        'NextStartCode:
        '                While ba.GetBits(i, 24) <> "000000000000000000000001"
        '                    i += 1
        '                    If i + 24 > ba.SBA.Count Then
        '                        'we're at the end of the bit array
        '                        If ba.AddData(BR, 100000) Then
        '                            GoTo WereDone
        '                        End If
        '                    End If
        '                End While

        '                'Ok, what kind of start code is it?
        '                If ba.GetBits(i, 32) = "00000000000000000000000110111000" Then 'group_start_code = 000001B8 = 110111000
        '                    ReDim Preserve Out(UBound(Out) + 1)
        '                    Out(UBound(Out)) = Me.GetGOPHeader(i, ba).time_code

        '                    If Out.Length = Count Then
        '                        GoTo WereDone
        '                    End If

        '                    i += 1
        '                    GoTo NextStartCode
        '                Else
        '                    i += 1
        '                    GoTo NextStartCode
        '                End If
        'WereDone:
        '                FS.Close()
        '                Return Out
        '            Catch ex As Exception
        '                Throw New Exception("Problem with GetGOPTimecodes. Error: " & ex.Message)
        '            End Try
        '        End Function

        'old way
        '        Public Function GetGOPTimecodes(ByVal M2VPath As String) As cGOPTimecode()
        '            Try
        '                Dim FS As New FileStream(M2VPath, FileMode.Open)
        '                Dim Out(-1) As cGOPTimecode
        '                Dim b(9999) As Byte
        '                Dim ba As cSeqBitArray
        '                Dim cur As Long = 0

        'MoveAheadInFile:
        '                If cur = FS.Length Then
        '                    GoTo WereDone
        '                End If
        '                If cur + 10000 > FS.Length Then
        '                    ReDim b(FS.Length - cur - 1)
        '                    FS.Read(b, 0, FS.Length - cur)
        '                Else
        '                    ReDim b(9999)
        '                    FS.Read(b, 0, 10000)
        '                End If
        '                cur += b.Length
        '                ba = New cSeqBitArray(b)
        '                Dim i As Integer = 0

        'NextStartCode:
        '                While ba.GetBits(i, 24) <> "000000000000000000000001"
        '                    i += 1
        '                    If i + 24 > ba.SBA.Count Then
        '                        'we're at the end of the bit array
        '                        GoTo MoveAheadInFile
        '                    End If
        '                End While

        '                'Ok, what kind of start code is it?
        '                Select Case ba.GetBits(i, 32)
        '                    Case "00000000000000000000000110111000" 'group_start_code = 000001B8 = 110111000
        '                        ReDim Preserve Out(UBound(Out) + 1)
        '                        Out(UBound(Out)) = Me.GetGOPHeader(i, ba).time_code

        '                        If Out.Length > 99 Then
        '                            GoTo WereDone
        '                        End If

        '                        i += 1
        '                        GoTo NextStartCode
        '                    Case Else
        '                        i += 1
        '                        GoTo NextStartCode
        '                End Select
        'WereDone:
        '                FS.Close()
        '                Return Out
        '            Catch ex As Exception
        '                Throw New Exception("Problem with GetGOPTimecodes. Error: " & ex.Message)
        '            End Try
        '        End Function

    End Class

    Public Class cMPEG2Header

        Public SequenceHeader As cSequenceHeader
        Public SequenceExtension As cSequenceExtension
        Public UserData As cUserData
        Public SequenceDisplayExtension As cSequenceDisplayExtension
        Public GOPHeader As cGOPHeader
        Public PictureHeader As cPictureHeader
        Public PictureCodingExtension As cPictureCodingExtension

        Public Sub New()
            SequenceHeader = New cSequenceHeader
            SequenceExtension = New cSequenceExtension
            UserData = New cUserData
            SequenceDisplayExtension = New cSequenceDisplayExtension
            GOPHeader = New cGOPHeader
            PictureHeader = New cPictureHeader
            PictureCodingExtension = New cPictureCodingExtension
        End Sub

    End Class

    Public Class cSequenceHeader
        'start code = 000001B3
        Public horizontal_size_value As Short
        Public vertical_size_value As Short
        Public horizontal_size As Short
        Public vertical_size As Short
        Public aspect_ratio_information As String
        Public frame_rate_code As String
        Public bit_rate_value As Short
        Public bit_rate As Short
        Public vbv_buffer_size_value As Short
        Public vbv_buffer_size As Short
        Public constrainded_parameters_flag As Short
        Public load_intra_quantiser_matrix As Short
        Public intra_quantiser_matrix() As Byte
        Public load_non_intra_quantiser_matrix As Short
        Public non_intra_quantiser_matrix() As Byte
    End Class

    Public Class cSequenceExtension
        'start code = 000001B5
        Public extension_start_code_identifier As Short
        Public profile_and_level_indication As Short
        Public Profile As String
        Public Level As String
        Public progressive_sequence As Short
        Public chroma_format As String
        Public horizontal_size_extension As Short
        Public vertical_size_extension As Short
        Public bit_rate_extension As Short
        Public vbv_buffer_size_extension As Short
        Public low_delay As Short
        Public frame_rate_extension_n As Short
        Public frame_rate_extension_d As Short
    End Class

    Public Class cUserData
        'start code = 000001B2
        Public user_data() As Byte
    End Class

    Public Class cSequenceDisplayExtension
        Public extension_start_code_identifier As Short
        Public video_format As String
        Public color_description As Short
        Public color_primaries As String
        Public transfer_characteristics As String
        Public matrix_coefficients As String
        Public display_horizontal_size As Short
        Public display_vertical_size As Short
    End Class

    Public Class cGOPHeader
        'start code = 000001B8
        Public time_code As cGOPTimecode
        Public closed_gop As Short
        Public broken_link As Short

        Public Sub New()
            time_code = New cGOPTimecode
        End Sub
    End Class

    Public Class cGOPTimecode
        Public drop_frame_flag As Short
        Public time_code_hours As Short
        Public time_code_minutes As Short
        Public time_code_seconds As Short
        Public time_code_pictures As Short

        Public Overloads Overrides Function ToString() As String
            Return Me.time_code_hours & ":" & PadString(Me.time_code_minutes, 2, "0", True) & ":" & PadString(Me.time_code_seconds, 2, "0", True) & ";" & PadString(Me.time_code_pictures, 2, "0", True)
        End Function

    End Class

    Public Class cPictureHeader
        Public temporal_reference As Short
        Public picture_coding_type As String
        Public vbv_delay As Integer
        Public full_pel_forward_vector As Short 'x
        Public forward_f_code As Short 'x
        Public full_pel_backward_vector As Short 'x
        Public backward_f_code As Short 'x
        Public extra_bit_picture As Short
        Public extra_information_picture As Short
    End Class

    Public Class cPictureCodingExtension
        Public extension_start_code_identifier As Short
        Public f_code_1 As Short
        Public f_code_2 As Short
        Public f_code_3 As Short
        Public f_code_4 As Short
        Public intra_dc_precision As Short
        Public picture_structure As String
        Public picture_structureI As Byte
        Public top_field_first As Short
        Public frame_pred_frame_dct As Short
        Public concealment_motion_vectors As Short
        Public q_scale_type As Short
        Public intra_vlc_format As Short
        Public alternate_scan As Short
        Public repeat_first_field As Short
        Public chroma_420_type As Short
        Public progressive_frame As Short
        Public composite_display_flag As Short
        Public v_axis As Short
        Public field_sequence1 As Short
        Public field_sequence2 As Short
        Public sub_carrier As Short
        Public burst_amplitude As Short
        Public sub_carrier_phase As Short
    End Class

    'Old way
    'Public Class cSeqBitArray
    '    Public SBA() As Short

    '    Public Function GetBytes(ByVal BitIndex As Integer, ByVal BitCount As Integer) As Byte()
    '        'bitcount must be evenly divisible by 8
    '        Dim out((BitCount / 8) - 1) As Byte
    '        Dim s As String
    '        For i As Short = BitIndex To BitIndex + BitCount - 1
    '            s &= SBA(i)
    '        Next
    '        For i As Integer = 1 To s.Length / 8 Step 8
    '            out(i) = BinaryToDecimal(Microsoft.VisualBasic.Mid(s, i, 8))
    '        Next
    '        Return out
    '    End Function

    '    Public Function GetBits(ByVal Index As Integer, ByVal Count As Integer) As String
    '        Dim OutStr As String
    '        For i As Short = Index To Index + Count - 1
    '            OutStr &= SBA(i)
    '        Next
    '        Return OutStr
    '    End Function

    '    Public Sub New(ByVal b() As Byte)
    '        ReDim SBA(-1)
    '        Dim s As String
    '        For Each bt As Byte In b
    '            s = DecimalToBinary(HEXtoDEC(ByteToString(bt)))
    '            s = PadString(s, 8, "0", True)
    '            For i As Short = 1 To 8
    '                ReDim Preserve SBA(UBound(SBA) + 1)
    '                SBA(UBound(SBA)) = Microsoft.VisualBasic.Mid(s, i, 1)
    '            Next
    '        Next
    '    End Sub

    '    Public Sub New()
    '    End Sub

    '    Public Overloads Overrides Function ToString() As String
    '        Dim s As String
    '        For Each c As Short In SBA
    '            s &= c
    '        Next
    '        Return s
    '    End Function

    '    Public Sub AddData(ByVal b() As Byte)
    '        Dim s As String
    '        For Each bt As Byte In b
    '            s = DecimalToBinary(HEXtoDEC(ByteToString(bt)))
    '            s = PadString(s, 8, "0", True)
    '            For i As Short = 1 To 8
    '                ReDim Preserve SBA(UBound(SBA) + 1)
    '                SBA(UBound(SBA)) = Microsoft.VisualBasic.Mid(s, i, 1)
    '            Next
    '        Next
    '    End Sub

    'End Class

    Public Class cSeqBitArray
        Public SBA As colBits

        Public Function GetBytes(ByVal BitIndex As Integer, ByVal BitCount As Integer) As Byte()
            'bitcount must be evenly divisible by 8
            Dim out((BitCount / 8) - 1) As Byte
            Dim s As String
            For i As Short = BitIndex To BitIndex + BitCount - 1
                s &= SBA(i)
            Next
            For i As Integer = 1 To s.Length / 8 Step 8
                out(i) = BinToDec(Microsoft.VisualBasic.Mid(s, i, 8))
            Next
            Return out
        End Function

        Public Function GetBits(ByVal Index As Integer, ByVal Count As Integer) As String
            Try
                If Index + Count > SBA.Count - 1 Then
                    Count = (SBA.Count - 1) - Index
                End If

                Dim OutStr As String
                For i As Integer = Index To Index + Count - 1
                    If i > SBA.Count - 1 Then
                        Throw New Exception("oh shit " & i & "  " & SBA.Count)
                    End If
                    OutStr &= SBA(i)
                Next
                Return OutStr
            Catch ex As Exception
                Throw New Exception("Problem with GetBits. Error: " & ex.Message)
            End Try
        End Function

        Public Sub New(ByVal b() As Byte)
            SBA = New colBits
            Dim s As String
            For Each bt As Byte In b
                s = DecimalToBinary(HEXtoDEC(ByteToString(bt)))
                s = PadString(s, 8, "0", True)
                For i As Short = 1 To 8
                    SBA.Add(Microsoft.VisualBasic.Mid(s, i, 1))
                Next
            Next
        End Sub

        Public Sub New()
            SBA = New colBits
        End Sub

        Public Overloads Overrides Function ToString() As String
            Dim s As String
            For Each c As Short In SBA
                s &= c
            Next
            Return s
        End Function

        Public Sub AddData(ByVal b() As Byte)
            Dim s As String
            For Each bt As Byte In b
                s = DecimalToBinary(HEXtoDEC(ByteToString(bt)))
                s = PadString(s, 8, "0", True)
                For i As Short = 1 To 8
                    SBA.Add(Microsoft.VisualBasic.Mid(s, i, 1))
                Next
            Next
        End Sub

        Public Function AddData(ByVal BR As BinaryReader, ByVal Count As Integer) As Boolean
            Try
                Dim s As String
                Dim AtEnd As Boolean = False
                If Count + BR.BaseStream.Position > BR.BaseStream.Length - 1 Then
                    Count = (BR.BaseStream.Length - 1) - BR.BaseStream.Position
                    AtEnd = True
                End If

                For i As Integer = 0 To Count
                    s = DecimalToBinary(BR.ReadByte)
                    s = PadString(s, 8, "0", True)
                    For ix As Short = 1 To 8
                        SBA.Add(Microsoft.VisualBasic.Mid(s, ix, 1))
                    Next
                Next
                Return AtEnd
            Catch ex As Exception
                Throw New Exception("Problem with AddData. Error: " & ex.Message)
            End Try
        End Function

    End Class

    Public Class colBits
        Inherits CollectionBase

        Public Function Add(ByVal bit As Byte) As Integer
            Return MyBase.List.Add(bit)
        End Function

        Default Public ReadOnly Property Item(ByVal index As Integer) As Byte
            Get
                Return MyBase.List.Item(index)
            End Get
        End Property

        Public Sub Remove(ByVal Item As Long)
            MyBase.List.RemoveAt(Item)
        End Sub

    End Class

    Public Class colBytes
        Inherits CollectionBase

        Public Function Add(ByVal Item As Byte) As Integer
            Return MyBase.List.Add(Item)
        End Function

        Default Public ReadOnly Property Item(ByVal Index As Integer) As Byte
            Get
                Return MyBase.List.Item(index)
            End Get
        End Property

        Public Sub Remove(ByVal Item As Long)
            MyBase.List.RemoveAt(Item)
        End Sub

    End Class


End Namespace

'Old Version
'Imports SMT.Common.Utilities.ConversionsAndSuch

'Namespace Multimedia.Formats.MPEG2

'    Public Class cParseMPEG2

'        Public Function ParseMPEG2(ByVal b() As Byte) As cMPEG2Header
'            Try
'                Dim Out As New cMPEG2Header
'                Dim ba As New cSeqBitArray(b)
'                Dim i As Integer = 0

'                'GO TO NEXT START CODE
'GoToNextStartCode:
'                While ba.GetBits(i, 24) <> "000000000000000000000001"
'                    i += 1
'                    If i + 24 > UBound(ba.SBA) Then GoTo WereDone
'                End While

'                'Ok, what kind of start code is it?
'                Select Case ba.GetBits(i, 32)
'                    Case "00000000000000000000000110110010" 'user_data_start_code = 000001B2 = 110110010
'                        'we're not going to do anything with user data
'                        i += 1
'                        GoTo GoToNextStartCode

'                    Case "00000000000000000000000110111000" 'group_start_code = 000001B8 = 110111000
'                        Out.GOPHeader = Me.GetGOPHeader(i, ba)
'                        GoTo GoToNextStartCode

'                    Case "00000000000000000000000110110011" 'sequence_header_code = 000001B3 = 110110011
'                        Out.SequenceHeader = Me.GetSequenceHeader(i, ba)
'                        GoTo GoToNextStartCode

'                    Case "00000000000000000000000110110101" 'extension_start_code = 000001B5 = 110110101
'                        Select Case ba.GetBits(i + 32, 4)
'                            Case "0010"
'                                Out.SequenceDisplayExtension = Me.GetDisplayExtension(i, ba)
'                                GoTo GoToNextStartCode
'                            Case "0001"
'                                Out.SequenceExtension = Me.GetSequenceExtension(i, ba)
'                                GoTo GoToNextStartCode
'                            Case "1000"
'                                Out.PictureCodingExtension = Me.GetPictureCodingExtension(i, ba)
'                                GoTo GoToNextStartCode
'                            Case Else
'                                i += 1
'                                GoTo GoToNextStartCode
'                        End Select

'                    Case Else
'                        i += 1
'                        GoTo GoToNextStartCode
'                End Select
'WereDone:
'                Return Out
'            Catch ex As Exception
'                Throw New Exception("Problem with ParseMPEG2. Error: " & ex.Message)
'            End Try
'        End Function

'        Public Function GetSequenceHeader(ByRef i As Integer, ByRef ba As cSeqBitArray) As cSequenceHeader
'            Try
'                i += 32

'                Dim Out As New cSequenceHeader
'                Out.horizontal_size_value = BinaryToDecimal(ba.GetBits(i, 12))
'                i += 12
'                Out.vertical_size_value = BinaryToDecimal(ba.GetBits(i, 12))
'                i += 12

'                'aspect_ratio_information
'                Dim ti As Short = mConversionsAndSuch.BinaryToDecimal(ba.GetBits(i, 4))
'                Select Case ti
'                    Case 0
'                        Out.aspect_ratio_information = "Forbidden"
'                    Case 1
'                        Out.aspect_ratio_information = "1,0 Square Sample"
'                    Case 2
'                        Out.aspect_ratio_information = "4x3"
'                    Case 3
'                        Out.aspect_ratio_information = "16x9"
'                    Case 4
'                        Out.aspect_ratio_information = "2.21x1"
'                    Case Else
'                        Out.aspect_ratio_information = "Reserved"
'                End Select
'                i += 4

'                'frame_rate_code
'                ti = mConversionsAndSuch.BinaryToDecimal(ba.GetBits(i, 4))
'                Select Case ti
'                    Case 0
'                        Out.frame_rate_code = "Forbidden"
'                    Case 1
'                        Out.frame_rate_code = "23.976"
'                    Case 2
'                        Out.frame_rate_code = "24"
'                    Case 3
'                        Out.frame_rate_code = "25"
'                    Case 4
'                        Out.frame_rate_code = "29.97"
'                    Case 5
'                        Out.frame_rate_code = "30"
'                    Case 6
'                        Out.frame_rate_code = "50"
'                    Case 7
'                        Out.frame_rate_code = "59.94"
'                    Case 8
'                        Out.frame_rate_code = "60"
'                    Case Else
'                        Out.frame_rate_code = "Reserved"
'                End Select
'                i += 4

'                'bit_rate_value
'                Out.bit_rate = BinaryToDecimal(ba.GetBits(i, 18))
'                i += 18

'                'marker_bit
'                i += 1

'                'vbv_buffer_size_value
'                Out.vbv_buffer_size_value = BinaryToDecimal(ba.GetBits(i, 10))
'                i += 10

'                'constrained_parameters_flag
'                Out.constrainded_parameters_flag = BinaryToDecimal(ba.GetBits(i, 1))
'                i += 1

'                'load_intra_quantiser_matrix
'                Out.load_intra_quantiser_matrix = BinaryToDecimal(ba.GetBits(i, 1))
'                i += 1

'                If Out.load_intra_quantiser_matrix Then
'                    'intra_quantiser_matrix
'                    Out.intra_quantiser_matrix = ba.GetBytes(i, 512)
'                    i += 512
'                End If

'                'load_non_intra_quantiser_matrix
'                Out.load_non_intra_quantiser_matrix = BinaryToDecimal(ba.GetBits(i, 1))
'                i += 1

'                If Out.load_non_intra_quantiser_matrix Then
'                    'non_intra_quantiser_matrix
'                    Out.non_intra_quantiser_matrix = ba.GetBytes(i, 512)
'                    i += 512
'                End If

'                Return Out
'            Catch ex As Exception
'                Throw New Exception("Problem with GetSequenceHeader. Error: " & ex.Message)
'            End Try
'        End Function

'        Public Function GetSequenceExtension(ByRef i As Integer, ByRef ba As cSeqBitArray) As cSequenceExtension
'            Try
'                Dim Out As New cSequenceExtension

'                'extension_start_code
'                i += 32

'                'extension_start_code_identifier
'                Out.extension_start_code_identifier = BinaryToDecimal(ba.GetBits(i, 4))
'                i += 4

'                'profile_and_level_indication
'                Out.profile_and_level_indication = BinaryToDecimal(ba.GetBits(i, 4))
'                i += 1 'skip escape bit
'                Dim s As String = ba.GetBits(i, 3)
'                Select Case s
'                    Case "101"
'                        Out.Profile = "Simple"
'                    Case "100"
'                        Out.Profile = "Main"
'                    Case "011"
'                        Out.Profile = "SNR Scalable"
'                    Case "010"
'                        Out.Profile = "Spatially Scalable"
'                    Case "001"
'                        Out.Profile = "High"
'                    Case Else
'                        Out.Profile = "Reserved"
'                End Select

'                i += 3
'                s = ba.GetBits(i, 4)
'                Select Case s
'                    Case "1010"
'                        Out.Level = "Low"
'                    Case "1000"
'                        Out.Level = "Main"
'                    Case "0110"
'                        Out.Level = "High 1440"
'                    Case "0100"
'                        Out.Level = "High"
'                    Case Else
'                        Out.Level = "Reserved"
'                End Select
'                i += 4

'                'progressive_sequence
'                Out.progressive_sequence = BinaryToDecimal(ba.GetBits(i, 1))
'                i += 1

'                'chroma_format
'                Select Case ba.GetBits(i, 2)
'                    Case "00"
'                        Out.chroma_format = "Reserved"
'                    Case "01"
'                        Out.chroma_format = "4:2:0"
'                    Case "10"
'                        Out.chroma_format = "4:2:2"
'                    Case "11"
'                        Out.chroma_format = "4:4:4"
'                End Select
'                i += 2

'                'horizontal_size_extension
'                Out.horizontal_size_extension = BinaryToDecimal(ba.GetBits(i, 2))
'                i += 2

'                'vertical_size_extension
'                Out.vertical_size_extension = BinaryToDecimal(ba.GetBits(i, 2))
'                i += 2

'                'bit_rate_extension
'                Out.bit_rate_extension = BinaryToDecimal(ba.GetBits(i, 12))
'                i += 12

'                'marker_bit
'                i += 1

'                'vbv_buffer_size_extension
'                Out.vbv_buffer_size_extension = BinaryToDecimal(ba.GetBits(i, 8))
'                i += 8

'                'low_delay
'                Out.low_delay = BinaryToDecimal(ba.GetBits(i, 1))
'                i += 1

'                'frame_rate_extension_n
'                Out.frame_rate_extension_n = BinaryToDecimal(ba.GetBits(i, 2))
'                i += 2

'                'frame_rate_extension_d
'                Out.frame_rate_extension_d = BinaryToDecimal(ba.GetBits(i, 2))
'                i += 2

'                'bump up to round byte
'                i += 3

'                Return Out
'            Catch ex As Exception
'                Throw New Exception("Problem with GetSequenceExtension. Error: " & ex.Message)
'            End Try
'        End Function

'        Public Function GetDisplayExtension(ByRef i As Integer, ByRef ba As cSeqBitArray) As cSequenceDisplayExtension
'            Try
'                Dim Out As New cSequenceDisplayExtension

'                'extension_start_code
'                i += 32

'                'extension_start_code_identifier
'                Out.extension_start_code_identifier = BinaryToDecimal(ba.GetBits(i, 4))
'                i += 4

'                'video_format
'                Select Case ba.GetBits(i, 3)
'                    Case "000"
'                        Out.video_format = "Component"
'                    Case "001"
'                        Out.video_format = "PAL"
'                    Case "010"
'                        Out.video_format = "NTSC"
'                    Case "011"
'                        Out.video_format = "SECAM"
'                    Case "100"
'                        Out.video_format = "MAC"
'                    Case "101"
'                        Out.video_format = "Unspecified"
'                    Case Else
'                        Out.video_format = "Reserved"
'                End Select
'                i += 3

'                'color_description
'                Out.color_description = BinaryToDecimal(ba.GetBits(i, 1))
'                i += 1

'                If Out.color_description Then
'                    'color_primaries
'                    Select Case BinaryToDecimal(ba.GetBits(i, 8))
'                        Case 0
'                            Out.color_primaries = "Forbidden"
'                        Case 1
'                            Out.color_primaries = "ITU-R BT.709"
'                        Case 2
'                            Out.color_primaries = "Unspecified Video"
'                        Case 4
'                            Out.color_primaries = "ITU-R BT.470-2 System M"
'                        Case 5
'                            Out.color_primaries = "ITU-R BT.470-2 System B,G"
'                        Case 6
'                            Out.color_primaries = "SMPTE 170M"
'                        Case 7
'                            Out.color_primaries = "SMPTE 240M (1987)"
'                        Case Else
'                            Out.color_primaries = "Reserved"
'                    End Select
'                    i += 8

'                    'transfer_characteristics
'                    Select Case BinaryToDecimal(ba.GetBits(i, 8))
'                        Case 0
'                            Out.transfer_characteristics = "Forbidden"
'                        Case 1
'                            Out.transfer_characteristics = "ITU-R BT.709"
'                        Case 2
'                            Out.transfer_characteristics = "Unspecified Video"
'                        Case 4
'                            Out.transfer_characteristics = "ITU-R BT.470-2 System M"
'                        Case 5
'                            Out.transfer_characteristics = "ITU-R BT.470-2 System B,G"
'                        Case 6
'                            Out.transfer_characteristics = "SMPTE 170M"
'                        Case 7
'                            Out.transfer_characteristics = "SMPTE 240M (1987)"
'                        Case 8
'                            Out.transfer_characteristics = "Linear Transfer Characteristics"
'                        Case Else
'                            Out.transfer_characteristics = "Reserved"
'                    End Select
'                    i += 8

'                    'matrix_coeficients
'                    Select Case BinaryToDecimal(ba.GetBits(i, 8))
'                        Case 0
'                            Out.matrix_coefficients = "Forbidden"
'                        Case 1
'                            Out.matrix_coefficients = "ITU-R BT.709"
'                        Case 2
'                            Out.matrix_coefficients = "Unspecified Video"
'                        Case 4
'                            Out.matrix_coefficients = "ITU-R BT.470-2 System M"
'                        Case 5
'                            Out.matrix_coefficients = "ITU-R BT.470-2 System B,G"
'                        Case 6
'                            Out.matrix_coefficients = "SMPTE 170M"
'                        Case 7
'                            Out.matrix_coefficients = "SMPTE 240M (1987)"
'                        Case Else
'                            Out.matrix_coefficients = "Reserved"
'                    End Select
'                    i += 8
'                End If

'                'display_horizontal_size
'                Out.display_horizontal_size = BinaryToDecimal(ba.GetBits(i, 14))
'                i += 14

'                'marker_bit
'                i += 1

'                'display_vertical_size
'                Out.display_vertical_size = BinaryToDecimal(ba.GetBits(i, 14))
'                i += 14

'                'bump to round byte
'                i += 3

'                Return Out
'            Catch ex As Exception
'                Throw New Exception("Problem with GetDisplayExtension. Error: " & ex.Message)
'            End Try
'        End Function

'        Public Function GetGOPHeader(ByRef i As Integer, ByRef ba As cSeqBitArray) As cGOPHeader
'            Try
'                Dim Out As New cGOPHeader

'                'extension_start_code
'                i += 32

'                'time_code
'                Out.time_code.drop_frame_flag = BinaryToDecimal(ba.GetBits(i, 1))
'                i += 1
'                Out.time_code.time_code_hours = BinaryToDecimal(ba.GetBits(i, 5))
'                i += 5
'                Out.time_code.time_code_minutes = BinaryToDecimal(ba.GetBits(i, 6))
'                i += 6
'                i += 1 'marker_bit
'                Out.time_code.time_code_seconds = BinaryToDecimal(ba.GetBits(i, 6))
'                i += 6
'                Out.time_code.time_code_pictures = BinaryToDecimal(ba.GetBits(i, 6))
'                i += 6

'                'closed_gop
'                Out.closed_gop = BinaryToDecimal(ba.GetBits(i, 1))
'                i += 1

'                'broken_link
'                Out.broken_link = BinaryToDecimal(ba.GetBits(i, 1))

'                'bump up to round byte
'                i += 6

'                Return Out
'            Catch ex As Exception
'                Throw New Exception("Problem with GetGOPHeader. Error: " & ex.Message)
'            End Try
'        End Function

'        Public Function GetPictureHeader(ByRef i As Integer, ByRef ba As cSeqBitArray) As cPictureHeader
'            Try
'                Dim Out As New cPictureHeader

'                'start code
'                i += 32

'                'temporal_reference
'                Out.temporal_reference = BinaryToDecimal(ba.GetBits(i, 10))
'                i += 10

'                'picture_coding_type
'                Select Case ba.GetBits(i, 3)
'                    Case "000"
'                        Out.picture_coding_type = "Forbidden"
'                    Case "001"
'                        Out.picture_coding_type = "Intra-coded(I)"
'                    Case "010"
'                        Out.picture_coding_type = "Predictive-coded(P)"
'                    Case "011"
'                        Out.picture_coding_type = "Bidirectionally-predictive-coded(B)"
'                    Case "100"
'                        Out.picture_coding_type = "Dc intra-coded (D)"
'                    Case Else
'                        Out.picture_coding_type = "Reserved"
'                End Select
'                i += 3

'                'vbv_delay
'                Out.vbv_delay = BinaryToDecimal(ba.GetBits(i, 16))
'                i += 16

'                ''we're going to ignore the rest of the picture header for now
'                ''I'm getting tired and the docs don't make too much sense

'                ''Go to picture coding extension

'                'While ba.GetBits(i, 32) <> "00000000000000000000000110110101"
'                '    i += 1
'                'End While

'                Return Out
'            Catch ex As Exception
'                Throw New Exception("Problem with GetPictureHeader. Error: " & ex.Message)
'            End Try
'        End Function

'        Public Function GetPictureCodingExtension(ByRef i As Integer, ByRef ba As cSeqBitArray) As cPictureCodingExtension
'            Try
'                Dim Out As New cPictureCodingExtension

'                'start code
'                i += 32

'                'extension_start_code_identifier
'                Out.extension_start_code_identifier = BinaryToDecimal(ba.GetBits(i, 4))
'                i += 4

'                'f_code
'                i += 14

'                'intra_dc_precision
'                Out.intra_dc_precision = BinaryToDecimal(ba.GetBits(i, 2))
'                i += 2

'                'picture_structure
'                Select Case ba.GetBits(i, 2)
'                    Case "00"
'                        Out.picture_structure = "Reserved"
'                    Case "01"
'                        Out.picture_structure = "Top Field"
'                    Case "10"
'                        Out.picture_structure = "Bottom Field"
'                    Case "11"
'                        Out.picture_structure = "Frame"
'                End Select
'                i += 2

'                'top_field_first
'                Out.top_field_first = BinaryToDecimal(ba.GetBits(i, 1))
'                i += 1

'                'frame_pred_frame_dct
'                Out.frame_pred_frame_dct = BinaryToDecimal(ba.GetBits(i, 1))
'                i += 1

'                'concealment_motion_vectors
'                Out.concealment_motion_vectors = ba.GetBits(i, 1)
'                i += 1

'                'q_scale_type
'                Out.q_scale_type = ba.GetBits(i, 1)
'                i += 1

'                'intra_vlc_format
'                Out.intra_vlc_format = ba.GetBits(i, 1)
'                i += 1

'                'alternate_scan
'                Out.alternate_scan = ba.GetBits(i, 1)
'                i += 1

'                'repeat_first_field
'                Out.repeat_first_field = ba.GetBits(i, 1)
'                i += 1

'                'chroma_420_type
'                Out.chroma_420_type = ba.GetBits(i, 1)
'                i += 1

'                'progressive_frame
'                Out.progressive_frame = ba.GetBits(i, 1)
'                i += 1

'                'composite_display_flag
'                Out.composite_display_flag = ba.GetBits(i, 1)
'                i += 1

'                If Out.composite_display_flag Then
'                    Throw New Exception("Implement composite_display_flag conditional flag checks.")
'                    'v_axis
'                    'field_sequence
'                    'sub_carrier
'                    'burst_amplitude
'                    'sub_carrier_phase
'                End If

'                Return Out
'            Catch ex As Exception
'                Throw New Exception("Problem with GetPictureCodingExtension. Error: " & ex.Message)
'            End Try
'        End Function

'    End Class

'    Public Class cMPEG2Header
'        Public SequenceHeader As cSequenceHeader
'        Public SequenceExtension As cSequenceExtension
'        Public UserData As cUserData
'        Public SequenceDisplayExtension As cSequenceDisplayExtension
'        Public GOPHeader As cGOPHeader
'        Public PictureHeader As cPictureHeader
'        Public PictureCodingExtension As cPictureCodingExtension

'        Public Sub New()
'            SequenceHeader = New cSequenceHeader
'            SequenceExtension = New cSequenceExtension
'            UserData = New cUserData
'            SequenceDisplayExtension = New cSequenceDisplayExtension
'            GOPHeader = New cGOPHeader
'            PictureHeader = New cPictureHeader
'            PictureCodingExtension = New cPictureCodingExtension
'        End Sub

'    End Class

'    Public Class cSequenceHeader
'        'start code = 000001B3
'        Public horizontal_size_value As Short
'        Public vertical_size_value As Short
'        Public horizontal_size As Short
'        Public vertical_size As Short
'        Public aspect_ratio_information As String
'        Public frame_rate_code As String
'        Public bit_rate_value As Short
'        Public bit_rate As Short
'        Public vbv_buffer_size_value As Short
'        Public vbv_buffer_size As Short
'        Public constrainded_parameters_flag As Short
'        Public load_intra_quantiser_matrix As Short
'        Public intra_quantiser_matrix() As Byte
'        Public load_non_intra_quantiser_matrix As Short
'        Public non_intra_quantiser_matrix() As Byte
'    End Class

'    Public Class cSequenceExtension
'        'start code = 000001B5
'        Public extension_start_code_identifier As Short
'        Public profile_and_level_indication As Short
'        Public Profile As String
'        Public Level As String
'        Public progressive_sequence As Short
'        Public chroma_format As String
'        Public horizontal_size_extension As Short
'        Public vertical_size_extension As Short
'        Public bit_rate_extension As Short
'        Public vbv_buffer_size_extension As Short
'        Public low_delay As Short
'        Public frame_rate_extension_n As Short
'        Public frame_rate_extension_d As Short
'    End Class

'    Public Class cUserData
'        'start code = 000001B2
'        Public user_data As Short
'    End Class

'    Public Class cSequenceDisplayExtension
'        Public extension_start_code_identifier As Short
'        Public video_format As String
'        Public color_description As Short
'        Public color_primaries As String
'        Public transfer_characteristics As String
'        Public matrix_coefficients As String
'        Public display_horizontal_size As Short
'        Public display_vertical_size As Short
'    End Class

'    Public Class cGOPHeader
'        'start code = 000001B8
'        Public time_code As cGOPTimecode
'        Public closed_gop As Short
'        Public broken_link As Short

'        Public Sub New()
'            time_code = New cGOPTimecode
'        End Sub
'    End Class

'    Public Class cGOPTimecode
'        Public drop_frame_flag As Short
'        Public time_code_hours As Short
'        Public time_code_minutes As Short
'        Public time_code_seconds As Short
'        Public time_code_pictures As Short
'    End Class

'    Public Class cPictureHeader
'        Public temporal_reference As Short
'        Public picture_coding_type As String
'        Public vbv_delay As Integer
'        Public full_pel_forward_vector As Short 'x
'        Public forward_f_code As Short 'x
'        Public full_pel_backward_vector As Short 'x
'        Public backward_f_code As Short 'x
'        Public extra_bit_picture As Short
'        Public extra_information_picture As Short
'    End Class

'    Public Class cPictureCodingExtension
'        Public extension_start_code_identifier As Short
'        Public f_code As Short
'        Public intra_dc_precision As Short
'        Public picture_structure As String
'        Public top_field_first As Short
'        Public frame_pred_frame_dct As Short
'        Public concealment_motion_vectors As Short
'        Public q_scale_type As Short
'        Public intra_vlc_format As Short
'        Public alternate_scan As Short
'        Public repeat_first_field As Short
'        Public chroma_420_type As Short
'        Public progressive_frame As Short
'        Public composite_display_flag As Short
'        Public v_axis As Short
'        Public field_sequence As Short
'        Public sub_carrier As Short
'        Public burst_amplitude As Short
'        Public sub_carrier_phase As Short
'    End Class

'End Namespace


'BR.ReadByte()
'Debug.WriteLine("SeekToStartCode Position: " & SR.BaseStream.Position)
'If SR.Peek = start_code_bytes(0) Then

'    Debug.WriteLine("Position=" & SR.BaseStream.Position & " Value=" & SR.Peek)
'    SR.BaseStream.Position += 1
'    If SR.EndOfStream Then Return False

'    If SR.Peek = start_code_bytes(1) Then

'        Debug.WriteLine("Position=" & SR.BaseStream.Position & " Value=" & SR.Peek)
'        SR.BaseStream.Position += 1
'        If SR.EndOfStream Then Return False

'        If SR.Peek = start_code_bytes(2) Then

'            Debug.WriteLine("Position=" & SR.BaseStream.Position & " Value=" & SR.Peek)
'            SR.BaseStream.Position += 1
'            If SR.EndOfStream Then Return False

'            If SR.Peek = start_code_bytes(3) Then

'                Debug.WriteLine("Position=" & SR.BaseStream.Position & " Value=" & SR.Peek)
'                SR.BaseStream.Position -= 3
'                Return True

'            Else

'                SR.BaseStream.Position += 1
'                GoTo StartOver

'            End If

'        Else

'            SR.BaseStream.Position += 2
'            GoTo StartOver

'        End If

'    Else

'        SR.BaseStream.Position += 3
'        GoTo StartOver

'    End If

'Else

'    SR.BaseStream.Position += 4
'    If SR.EndOfStream Then Return False
'    GoTo StartOver

'End If

'Return False 'start code not found


'Should work but does not take into account the fact that start codes are quad byte aligned
'StartOver:
'                If SR.Peek = start_code_bytes(0) Then
'                    SR.BaseStream.Position += 1
'                    If SR.EndOfStream Then Return False
'                    If SR.Peek = start_code_bytes(1) Then
'                        SR.BaseStream.Position += 1
'                        If SR.EndOfStream Then Return False
'                        If SR.Peek = start_code_bytes(2) Then
'                            SR.BaseStream.Position += 1
'                            If SR.EndOfStream Then Return False
'                            If SR.Peek = start_code_bytes(3) Then
'                                SR.BaseStream.Position -= 3
'                                Return True
'                            Else
'                                SR.BaseStream.Position -= 2
'                                GoTo StartOver
'                            End If
'                        Else
'                            SR.BaseStream.Position -= 1
'                            GoTo StartOver
'                        End If
'                    Else
'                        GoTo StartOver
'                    End If
'                Else
'                    SR.BaseStream.Position += 1
'                    If SR.EndOfStream Then Return False
'                    GoTo StartOver
'                End If
'                Return False 'start code not found
