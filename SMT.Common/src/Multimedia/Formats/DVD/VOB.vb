Imports System.IO
Imports SMT.DotNet.Utility
Imports System.Reflection
Imports System.Text
Imports SMT.Multimedia.Formats.DVD.IFO
Imports System.Drawing

Namespace Multimedia.Formats.DVD.VOB

    ''' <summary>
    ''' Wrapper for an entire VOB set (VTS_T or VTS_M or VMGM)
    ''' Allows for reading of VOB data using a global offset from ABN0 of the first VOB in the VOBS.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class cVOBS

        Private BasePath As String
        Private Indexes As cIndexItemWrapper

        Public Sub New(ByVal PathToFirstVOB As String)

            BasePath = Path.GetDirectoryName(PathToFirstVOB) & "\"
            Indexes = New cIndexItemWrapper(BasePath)

            Dim filename As String = Path.GetFileName(PathToFirstVOB).ToLower

            If filename = "video_ts.vob" Then
                'VMGM
                Indexes.Items.Add(New cIndexItem(filename, 0, GetFileSize(BasePath & filename) - 1))
            Else

                Dim VTSN As Byte = GetVTSN(filename)

                If Right(filename, 2) = "_0" Then
                    'VTS_M
                    Indexes.Items.Add(New cIndexItem(filename, 0, GetFileSize(BasePath & filename) - 1))
                Else
                    'VTS_T
                    Dim di As New DirectoryInfo(BasePath)
                    Dim pos As UInt64 = 0
                    For Each fi As FileInfo In di.GetFiles("*.vob")
                        If InStr(fi.Name.ToLower, "video_ts") Then GoTo Nxt
                        If GetVTSN(fi.Name.ToLower) <> VTSN Then GoTo Nxt
                        If Right(fi.Name.ToLower, 6) = "_0.vob" Then GoTo Nxt
                        Indexes.Items.Add(New cIndexItem(fi.Name, pos, pos + fi.Length - 1))
                        pos += fi.Length
Nxt:
                    Next
                End If

            End If

            'get the sizes of the vobs
            'create a lookup table so it is easy to get a file name for a given offset plus the offset within that file

        End Sub

        Public Class cIndexItem

            Public Filename As String
            Public StartBN As UInt64
            Public EndBN As UInt64

            Public Sub New(ByVal nFilename As String, ByVal nStartBN As UInt64, ByVal nEndBN As UInt64)
                Filename = nFilename
                StartBN = nStartBN
                EndBN = nEndBN
                Debug.WriteLine("New Index Item: " & Filename & " " & StartBN & " - " & EndBN)
            End Sub

        End Class

        Public Class cIndexItemWrapper

            Public Items As List(Of cIndexItem)

            Private BasePath As String

            Public Sub New(ByVal nBasePath As String)
                Items = New List(Of cIndexItem)
                BasePath = nBasePath
            End Sub

            ''' <summary>
            ''' Returns a file stream queued to the correct offset.
            ''' </summary>
            ''' <param name="GlobalOffset">This is the offset from the start of VOBS.</param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function GetFilestream(ByVal GlobalOffset As UInt64) As FileStream
                For b As Byte = 0 To Items.Count - 1
                    If GlobalOffset >= Items(b).StartBN And GlobalOffset <= Items(b).EndBN Then
                        Dim out As New FileStream(BasePath & Items(b).Filename, FileMode.Open, FileAccess.Read)
                        out.Seek(GlobalOffset - Items(b).StartBN, SeekOrigin.Begin)
                        Return out
                    End If
                Next
            End Function

        End Class

        Private Shared Function GetFileSize(ByVal Path As String) As UInt64
            If Not File.Exists(Path) Then Return 0
            Dim fi As New FileInfo(Path)
            Return fi.Length
        End Function

        Private Shared Function GetVTSN(ByVal filename As String) As Byte
            Return Mid(filename, 5, 2)
        End Function

        Public Function GetNextNavPack(ByVal Offset As UInt64) As cVOBPack.cNavPacket
            Try
                If Indexes Is Nothing Then Return Nothing
                Dim FS As FileStream = Indexes.GetFilestream(Offset)
                Dim VP As cVOBPack
                Dim buf(2047) As Byte
                While True
                    'this will keep reading from the offset point until a nav pack is found
                    FS.Read(buf, 0, buf.Length)
                    VP = New cVOBPack(buf, 0, 0)
                    If VP._Type = cVOBPack.ePackType.Navigation Then
                        Return VP.NavPacket
                    Else
                        VP = Nothing
                    End If
                End While
                Return Nothing
            Catch ex As Exception
                Throw New Exception("Problem with GetNextNavPack(). Error: " & ex.Message, ex)
            End Try
        End Function

        'Public Function GetFilestream(ByVal Offset As UInt64) As FileStream
        '    If Indexes Is Nothing Then Return Nothing
        '    Return Indexes.GetFilestream(Offset)
        'End Function

    End Class
    Public Class cVOB

        Public Packs As List(Of cVOBPack)

        Private VOBPath As String

        Public Sub New(ByVal nVOBPath As String)
            Try
                If Not File.Exists(nVOBPath) Then Throw New Exception("File does not exist - " & nVOBPath)
                VOBPath = nVOBPath

                Dim ticks1 As UInt64 = DateTime.Now.Ticks
                Dim ticks2 As UInt64

                Packs = New List(Of cVOBPack)

                'each pack is 2048
                'read 100MB at a time from a file, that's 51200 packs at a time

                Dim buf() As Byte
                Dim pos As UInt64 = 0
                Dim NibbleSize As UInt32 = 104857600

                Dim FS As New FileStream(VOBPath, FileMode.Open)
                Dim BR As New BinaryReader(FS)

                'Debug.WriteLine("T1a=" & (DateTime.Now.Ticks - ticks1) / 10000000)
                While pos < FS.Length
                    ticks2 = DateTime.Now.Ticks
                    If pos + NibbleSize >= FS.Length Then
                        NibbleSize = FS.Length - pos
                    End If
                    buf = Nothing
                    'Debug.WriteLine("T2a=" & (DateTime.Now.Ticks - ticks2) / 10000000)
                    buf = BR.ReadBytes(NibbleSize)
                    'Debug.WriteLine("T2b=" & (DateTime.Now.Ticks - ticks2) / 10000000)
                    ProcessNibble(buf, pos)
                    'Debug.WriteLine("T2c=" & (DateTime.Now.Ticks - ticks2) / 10000000)
                    'Debug.WriteLine("")
                    pos += NibbleSize
                End While
                BR.Close()
                FS.Close()
                BR = Nothing
                buf = Nothing
                GC.Collect()
                'Debug.WriteLine("T1b=" & (DateTime.Now.Ticks - ticks1) / 10000000)
            Catch ex As Exception
                Throw New Exception("Problem with New() cVOB. Error: " & ex.Message, ex)
            End Try
        End Sub

        Private Sub ProcessNibble(ByRef buf() As Byte, ByVal pos As UInt32)
            Try
                Dim tPack As cVOBPack
                For i As Int32 = 0 To buf.Length - 1 Step 2048
                    tPack = New cVOBPack(buf, pos + i, i)
                    Me.Packs.Add(tPack)
                Next
            Catch ex As Exception
                Throw New Exception("Problem in ProcessNibble(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Sub PopulatePackData(ByRef Pack As cVOBPack)
            Try
                If Not File.Exists(VOBPath) Then Throw New Exception("File does not exist - " & VOBPath)

                Dim FS As New FileStream(VOBPath, FileMode.Open)
                FS.Position = Pack.Offset
                Dim BR As New BinaryReader(FS)
                Dim buf() As Byte = BR.ReadBytes(2048)
                BR.Close()
                FS.Close()
                FS = Nothing

                Select Case Pack._Type
                    Case cVOBPack.ePackType.Navigation
                        Pack.NavPacket = New cVOBPack.cNavPacket(buf, 14 + Pack.PacketHeader.Length)
                    Case cVOBPack.ePackType.Video
                        Pack.VideoPacket = New cVOBPack.cVideoPacket(buf, 14 + Pack.PacketHeader.Length)
                    Case cVOBPack.ePackType.Audio
                        Pack.AudioPacket = New cVOBPack.cAudioPacket(buf, 14 + Pack.PacketHeader.Length)
                    Case cVOBPack.ePackType.Subpicture
                        Pack.SubPacket = New cVOBPack.cSubpicturePacket(buf, 14 + Pack.PacketHeader.Length)
                End Select
            Catch ex As Exception
                Throw New Exception("Problem with PopulatePackData(). Error: " & ex.Message, ex)
            End Try
        End Sub

    End Class

    ''' <summary>
    ''' 5.2
    ''' </summary>
    ''' <remarks></remarks>
    Public Class cVOBPack

#Region "CONSTRUCTOR"

        Public Sub New(ByRef buf() As Byte, ByVal nOverallOffset As UInt32, ByVal nNibbleOffset As UInt32)
            Try
                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian

                _Offset = nOverallOffset
                _PackNumber = nOverallOffset / 2048

                Me.PackHeader = New cPackHeader(buf, nNibbleOffset)

                'DETERMINE THE PACKET TYPE
                Select Case EBC.ToUInt32(buf, nNibbleOffset + 14)
                    Case 443
                        Me.PacketHeader = New cPacketHeader_Type1(buf, nNibbleOffset + 14)
                        _Type = ePackType.Navigation

                    Case 480
                        Me.PacketHeader = New cPacketHeader_Type2(buf, nNibbleOffset + 14)
                        _Type = ePackType.Video

                    Case Else '442
                        Me.PacketHeader = New cPacketHeader_Type2(buf, nNibbleOffset + 14)

                        Select Case PacketHeader.Type
                            Case ePacketType.Audio_AC3, ePacketType.Audio_DTS, ePacketType.Audio_MPEG, ePacketType.Audio_PCM, ePacketType.Audio_SDDS
                                Me._Type = ePackType.Audio
                            Case ePacketType.Subpicture
                                Me._Type = ePackType.Subpicture
                        End Select

                End Select

                'Select Case _Type
                '    Case ePackType.Navigation
                '        Me.NavPacket = New cNavPacket(buf, Offset + 14 + Me.PacketHeader.Length)
                '    Case ePackType.Video
                '        Me.VideoPacket = New cVideoPacket(buf, Offset + 14 + Me.PacketHeader.Length)
                '    Case ePackType.Audio
                '        Me.AudioPacket = New cAudioPacket(buf, Offset + 14 + Me.PacketHeader.Length)
                '    Case ePackType.Subpicture
                '        Me.SubPacket = New cSubpicturePacket(buf, Offset + 14 + Me.PacketHeader.Length)
                'End Select

            Catch ex As Exception
                Throw New Exception("Problem with New() cVOBPack. Error: " & ex.Message, ex)
            End Try
        End Sub

#End Region 'CONSTRUCTOR

#Region "PROPERTIES"

        Public ReadOnly Property Type() As String
            Get
                Return _Type.ToString
            End Get
        End Property
        Public _Type As ePackType

        Public ReadOnly Property Offset() As UInt32
            Get
                Return _Offset
            End Get
        End Property
        Public _Offset As UInt32

        Public ReadOnly Property PackNumber() As UInt32
            Get
                Return _PackNumber
            End Get
        End Property
        Public _PackNumber As UInt32

        Public PackHeader As cPackHeader
        Public PacketHeader As cPacketHeaderBase

        Public NavPacket As cNavPacket
        Public VideoPacket As cVideoPacket
        Public AudioPacket As cAudioPacket
        Public SubPacket As cSubpicturePacket

#End Region 'PROPERTIES

#Region "METHODS"

        Public Overrides Function ToString() As String
            Try
                Dim FI() As FieldInfo = Me.GetType.GetFields()
                Dim SB As New StringBuilder
                SB.Append("====PACK INFO====")
                SB.Append(vbNewLine)
                For Each F As FieldInfo In FI
                    If Not F.FieldType.Namespace.StartsWith("SMT.Common.") Then
                        SB.Append(F.Name & " = " & F.GetValue(Me) & vbNewLine)
                    End If
                Next
                SB.Append(vbNewLine)
                SB.Append("====PACK HEADER====" & vbNewLine)
                SB.Append(Me.PackHeader.ToString)
                SB.Append(vbNewLine)
                SB.Append("====PACKET HEADER====" & vbNewLine)
                SB.Append(Me.PacketHeader.ToString & vbNewLine)

                Select Case Me._Type
                    Case ePackType.Navigation
                        SB.Append("====NAV PACKET====" & vbNewLine)
                        SB.Append(Me.NavPacket.ToString & vbNewLine)
                    Case ePackType.Video
                        SB.Append("====VIDEO PACKET====" & vbNewLine)
                        SB.Append(Me.VideoPacket.ToString & vbNewLine)
                    Case ePackType.Audio
                        SB.Append("====AUDIO PACKET====" & vbNewLine)
                        SB.Append(Me.AudioPacket.ToString & vbNewLine)
                    Case ePackType.Subpicture
                        SB.Append("====SUBPICUTRE PACKET====" & vbNewLine)
                        SB.Append(Me.SubPacket.ToString & vbNewLine)
                End Select

                Return SB.ToString
            Catch ex As Exception
                Throw New Exception("Problem with ToString(). Error: " & ex.Message, ex)
            End Try
        End Function

#End Region 'METHODS

#Region "CLASSES"

        ''' <summary>
        ''' 5.2.1-2
        ''' </summary>
        ''' <remarks></remarks>
        Public Class cPackHeader
            Inherits cBase_VobClasses

            Public Length As Byte = 14

            Public Pack_Start_code As UInt32
            Public SCR_Base_29_15 As UInt16
            Public SCR_Base_14_0 As UInt16
            Public SCR_Extension As UInt16
            Public program_mux_rate As UInt32
            Public pack_stuffing_length As Byte

            Public Sub New(ByRef buf() As Byte, ByVal pos As UInt32)
                Try
                    Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                    Me.Pack_Start_code = EBC.ToUInt32(buf, pos)
                    Dim tLng As UInt64 = EBC.ToUInt64(buf, pos + 4)
                    tLng = tLng >> 16
                    Me.SCR_Base_29_15 = (tLng >> 27) And 32767
                    Me.SCR_Base_14_0 = (tLng >> 11) And 32767
                    Me.SCR_Extension = (tLng >> 1) And 511
                    Me.program_mux_rate = 1008
                    Me.pack_stuffing_length = buf(13) And 7
                Catch ex As Exception
                    Throw New Exception("Problem with New() cPackHeader. Error: " & ex.Message, ex)
                End Try
            End Sub

        End Class

        Public MustInherit Class cPacketHeaderBase
            Inherits cBase_VobClasses

            Public Length As Byte
            Public Type As ePacketType

        End Class

        ''' <summary>
        ''' For use with Navigation packets
        ''' </summary>
        ''' <remarks>5.2.2-1</remarks>
        Public Class cPacketHeader_Type1
            Inherits cPacketHeaderBase

            Public system_header_start_code As UInt32
            Public header_length As UInt16
            Public rate_bound As Boolean
            Public audio_bound As Byte
            Public fixed_flag As Boolean
            Public CSPS_flag As Boolean
            Public system_audio_lock_flag As Boolean
            Public system_video_lock_flag As Boolean
            Public video_bound As Byte
            Public packet_rate_restriction_flag As Boolean
            Public stream_id_vid As Byte
            Public P_STD_buf_bound_scale_vid As Boolean
            Public P_STD_buf_size_bound_vid As UInt16
            Public stream_id_aud As Byte
            Public P_STD_buf_bound_scale_aud As Boolean
            Public P_STD_buf_size_bound_aud As UInt16
            Public stream_id_ps1 As Byte
            Public P_STD_buf_bound_scale_ps1 As Boolean
            Public P_STD_buf_size_bound_ps1 As UInt16
            Public stream_id_ps2 As Byte
            Public P_STD_buf_bound_scale_ps2 As Boolean
            Public P_STD_buf_size_bound_ps2 As UInt16

            Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                Try
                    Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                    Dim tInt As UInt32

                    Me.system_header_start_code = EBC.ToUInt32(buf, offset)
                    Me.header_length = EBC.ToUInt16(buf, offset + 4) 'always 24?
                    tInt = EBC.ToUInt32(buf, offset + 5) 'note: we're back one byte so this pull goes up only to the desired point
                    Me.rate_bound = (tInt >> 1) And &HFFFFFF
                    tInt = EBC.ToUInt16(buf, offset + 9)
                    Me.audio_bound = (tInt >> 10) And 63
                    Me.fixed_flag = (tInt >> 9) And 1
                    Me.CSPS_flag = (tInt >> 8) And 1
                    Me.system_audio_lock_flag = (tInt >> 7) And 1
                    Me.system_video_lock_flag = (tInt >> 6) And 1
                    Me.video_bound = tInt And 31
                    tInt = buf(offset + 11)
                    Me.packet_rate_restriction_flag = (tInt >> 7) And 1

                    'vid
                    Me.stream_id_vid = buf(12)
                    tInt = EBC.ToUInt16(buf, offset + 13)
                    Me.P_STD_buf_bound_scale_vid = (tInt >> 13) And 1
                    Me.P_STD_buf_size_bound_vid = tInt And 8191

                    'aud
                    Me.stream_id_aud = buf(15)
                    tInt = EBC.ToUInt16(buf, offset + 16)
                    Me.P_STD_buf_bound_scale_aud = (tInt >> 13) And 1
                    Me.P_STD_buf_size_bound_aud = tInt And 8191

                    'ps1
                    Me.stream_id_ps1 = buf(18)
                    tInt = EBC.ToUInt16(buf, offset + 19)
                    Me.P_STD_buf_bound_scale_ps1 = (tInt >> 13) And 1
                    Me.P_STD_buf_size_bound_ps1 = tInt And 8191

                    'ps2
                    Me.stream_id_ps2 = buf(21)
                    tInt = EBC.ToUInt16(buf, offset + 22)
                    Me.P_STD_buf_bound_scale_ps2 = (tInt >> 13) And 1
                    Me.P_STD_buf_size_bound_ps2 = tInt And 8191

                    Me.Length = 24
                    Me.Type = ePacketType.Navigation

                Catch ex As Exception
                    Throw New Exception("Problem with New() cPacketHeader_Type1. Error: " & ex.Message, ex)
                End Try
            End Sub

        End Class

        ''' <summary>
        ''' For use with Video, Audio, and Subpicture packets
        ''' </summary>
        ''' <remarks>5.2.3-1, 5.2.4-1, 5.2.4-2, 5.2.4-3, 5.2.5-1, Annexes H/I</remarks>
        Public Class cPacketHeader_Type2
            Inherits cPacketHeaderBase

            'GENERALLY SHARED
            Public packet_start_code_prefix As UInt32
            Public stream_id As Byte
            Public PES_packet_length As UInt16
            Public PES_scrambling_control As Byte
            Public PES_priority As Boolean
            Public data_alignment_indicator As Boolean
            Public copyright As Boolean
            Public original_or_copy As Boolean
            Public PTS_DTS_flags As Byte
            Public ESCR_flag As Boolean
            Public ES_rate_flag As Boolean
            Public DSM_trick_mode_flag As Boolean
            Public additional_copy_info_flag As Boolean
            Public PES_CRC_flag As Boolean
            Public PES_extension_flag As Boolean
            Public PES_header_data_length As Byte
            Public PTS_32_30 As Byte
            Public PTS_29_15 As Byte
            Public PTS_14_0 As Byte
            Public PES_private_data_flag As Boolean
            Public pack_header_field_flag As Boolean
            Public program_packet_sequence_counter_flag As Boolean
            Public P_STD_buffer_flag As Boolean
            Public PES_extension_flag_2 As Boolean
            Public P_STD_buffer_scale As Boolean
            Public P_STD_buffer_size As UInt16

            'PARTIALLY SHARED
            Public sub_stream_id As Byte
            Public decoding_stream_number As Byte

            'AUDIO SHARED
            Public audio_number_of_frame_headers As Byte
            Public audio_first_access_unit_pointer As UInt16

            'AUDIO LINEAR PCM
            Public audio_emphasis_flag As Boolean
            Public audio_mute_flag As Boolean
            Public audio_frame_number As Byte
            Public audio_quantization_word_length As Byte
            Public audio_sampling_frequency As Byte
            Public audio_number_of_channels As Byte
            Public audio_dynamic_range_control As Byte

            Public Sub New(ByRef buf() As Byte, ByVal offset As UInt64)
                Try
                    Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                    Dim tInt As UInt32
                    tInt = EBC.ToUInt32(buf, offset)

                    'THE FOLLOWING ARE ALWAYS PRESENT (video, audio, subpicture packets)
                    Me.packet_start_code_prefix = (tInt >> 8) And &HFFFFFF
                    Me.stream_id = tInt And 255
                    Me.PES_packet_length = EBC.ToUInt16(buf, offset + 4)

                    tInt = EBC.ToUInt32(buf, offset + 6)
                    tInt = tInt >> 8

                    Me.PES_scrambling_control = (tInt >> 22) And 3
                    Me.PES_priority = (tInt >> 20) And 1
                    Me.data_alignment_indicator = (tInt >> 19) And 1
                    Me.copyright = (tInt >> 18) And 1
                    Me.original_or_copy = (tInt >> 17) And 1
                    Me.PTS_DTS_flags = (tInt >> 16) And 3
                    Me.ESCR_flag = (tInt >> 14) And 1
                    Me.ES_rate_flag = (tInt >> 13) And 1
                    Me.DSM_trick_mode_flag = (tInt >> 12) And 1
                    Me.additional_copy_info_flag = (tInt >> 11) And 1
                    Me.PES_CRC_flag = (tInt >> 10) And 1
                    Me.PES_extension_flag = (tInt >> 9) And 1
                    Me.PES_header_data_length = tInt And 255

                    Me.Length = 9 + Me.PES_header_data_length

                    'here it gets a bit tricky
                    'we could be dealing with video, sub, or any of five audio types
                    'first goal is to figure out which we're dealing with

                    If stream_id = 224 Then
                        'it's a video packet
                        Me.Type = ePacketType.Video

                    ElseIf stream_id = 189 Then

                        sub_stream_id = buf(offset + Length)
                        decoding_stream_number = sub_stream_id And 31

                        If sub_stream_id >= 32 And sub_stream_id <= 63 Then
                            'SUBPICTURE
                            Me.Type = ePacketType.Subpicture

                        Else
                            'Linear PCM, AC3, SDDS, or DTS audio
                            Select Case (sub_stream_id >> 3) And 31
                                Case 20
                                    'Linear PCM
                                    Me.Type = ePacketType.Audio_PCM

                                Case 16
                                    'AC3
                                    Me.Type = ePacketType.Audio_AC3

                                Case 17
                                    'DTS
                                    Me.Type = ePacketType.Audio_DTS

                                Case 18
                                    'SDDS
                                    Me.Type = ePacketType.Audio_SDDS

                            End Select
                        End If

                    ElseIf (stream_id >= 192 And stream_id <= 199) Or (stream_id >= 208 And stream_id <= 215) Then
                        'it is MPEG audio
                        Me.Type = ePacketType.Audio_MPEG

                    End If


                Catch ex As Exception
                    Throw New Exception("Problem with New() cPacketHeader_Type2. Error: " & ex.Message, ex)
                End Try
            End Sub

        End Class

        ''' <summary>
        ''' 5.2.2
        ''' </summary>
        ''' <remarks></remarks>
        Public Class cNavPacket
            Inherits cBase_VobClasses

            Public PCIPacket As cPCIPacket
            Public DSIPacket As cDSIPacket

            Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                Me.PCIPacket = New cPCIPacket(buf, offset)
                Me.DSIPacket = New cDSIPacket(buf, offset + 986)
            End Sub

            ''' <summary>
            ''' 4.4
            ''' </summary>
            ''' <remarks></remarks>
            Public Class cPCIPacket
                Inherits cBase_VobClasses

                Public Length As UInt32 = 979

                Public packet_start_code As UInt32 'combines packet_start_code_prefix and stream_id
                Public PES_packet_length As UInt16
                Public sub_stream_id As Byte

                Public PCI_GI As cPCI_GI
                Public NSML_AGLI As cNSML_AGLI
                Public HLI As cHLI
                Public RECI As cRECI

                Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                    Try
                        Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                        packet_start_code = EBC.ToUInt32(buf, offset)
                        PES_packet_length = EBC.ToUInt16(buf, offset + 4)
                        sub_stream_id = buf(offset + 6)
                        Me.PCI_GI = New cPCI_GI(buf, offset + 7)
                        Me.NSML_AGLI = New cNSML_AGLI(buf, offset + 7 + Me.PCI_GI.Length)
                        Me.HLI = New cHLI(buf, offset + 7 + Me.PCI_GI.Length + Me.NSML_AGLI.Length)
                        Me.RECI = New cRECI(buf, offset + 7 + Me.PCI_GI.Length + Me.NSML_AGLI.Length + Me.HLI.Length)
                    Catch ex As Exception
                        Throw New Exception("Problem with New() cPCIPacket. Error: " & ex.Message, ex)
                    End Try
                End Sub

                ''' <summary>
                ''' 4.4.1
                ''' </summary>
                ''' <remarks></remarks>
                Public Class cPCI_GI
                    Inherits cBase_VobClasses

                    Public Length As UInt32 = 60

                    Public NV_PCK_LBN As UInt32
                    Public VOBU_CAT_APSTB As Byte
                    Public VOBU_UOP_CTL() As Boolean
                    Public VOBU_S_PTM As cPTM
                    Public VOBU_E_PTM As cPTM
                    Public VOBU_SE_E_PTM As cPTM
                    Public C_ELTM As cC_ELTM

                    Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                        Try
                            Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                            Dim tInt As UInt32
                            Me.NV_PCK_LBN = EBC.ToUInt32(buf, offset)
                            Me.VOBU_CAT_APSTB = (EBC.ToUInt16(buf, offset + 4) >> 6) And 3
                            tInt = EBC.ToUInt32(buf, offset + 8)
                            ReDim Me.VOBU_UOP_CTL(24)
                            For i As Integer = 0 To 24
                                Me.VOBU_UOP_CTL(i) = (tInt >> i) And 1
                            Next

                            Me.VOBU_S_PTM = New cPTM(buf, offset + 12)
                            Me.VOBU_E_PTM = New cPTM(buf, offset + 16)
                            Me.VOBU_SE_E_PTM = New cPTM(buf, offset + 20)

                            Me.C_ELTM = New cC_ELTM(buf, offset + 24)

                        Catch ex As Exception
                            Throw New Exception("Problem with New() cPCI_GI. Error: " & ex.Message, ex)
                        End Try
                    End Sub

                    Public Overrides Function ToString() As String
                        Try
                            Dim T As Type = Me.GetType
                            Dim FI() As FieldInfo = T.GetFields()
                            Dim SB As New StringBuilder
                            For Each F As FieldInfo In FI
                                If F.Name = "VOBU_UOP_CTL" Then
                                    For i As Int16 = 0 To 24
                                        SB.Append("VOBU_UOP_CTL_" & i & "=" & VOBU_UOP_CTL(i) & " ")
                                    Next
                                    SB.Append(vbNewLine)
                                Else
                                    If F.FieldType.Namespace.StartsWith("SMT.Common.") Then
                                        SB.Append("=" & F.Name & "=" & vbNewLine)
                                        SB.Append(F.GetValue(Me).ToString & vbNewLine)
                                    Else
                                        SB.Append(F.Name & " = " & F.GetValue(Me) & vbNewLine)
                                    End If
                                End If
                            Next
                            Return SB.ToString
                        Catch ex As Exception
                            Throw New Exception("Problem with ToString(). Error: " & ex.Message, ex)
                        End Try
                    End Function

                End Class 'cPCI_GI

                ''' <summary>
                ''' 4.4.2
                ''' </summary>
                ''' <remarks></remarks>
                Public Class cNSML_AGLI
                    Inherits cBase_VobClasses

                    Public Length As UInt32 = 36

                    Public NSML_AGL_DSTA As List(Of cNSML_AGL_DSTA)

                    Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                        Try
                            Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                            Me.NSML_AGL_DSTA = New List(Of cNSML_AGL_DSTA)
                            For i As Integer = 0 To 8 Step 4
                                Me.NSML_AGL_DSTA.Add(New cNSML_AGL_DSTA(buf, offset + i))
                            Next
                        Catch ex As Exception
                            Throw New Exception("Problem with New() cNSML_AGLI. Error: " & ex.Message, ex)
                        End Try
                    End Sub

                    Public Overrides Function ToString() As String
                        Try
                            Dim T As Type = Me.GetType
                            Dim FI() As FieldInfo = T.GetFields()
                            Dim SB As New StringBuilder
                            For Each F As FieldInfo In FI
                                If F.Name = "NSML_AGL_DSTA" Then
                                    For i As UInt32 = 0 To NSML_AGL_DSTA.Count - 1
                                        SB.Append("===NSML_AGL_DSTA_" & i & vbNewLine)
                                        SB.Append(Me.NSML_AGL_DSTA(i).ToString & vbNewLine)
                                    Next
                                Else
                                    SB.Append(F.Name & " = " & F.GetValue(Me) & vbNewLine)
                                End If
                            Next
                            Return SB.ToString
                        Catch ex As Exception
                            Throw New Exception("Problem with ToString(). Error: " & ex.Message, ex)
                        End Try
                    End Function

                    Public Class cNSML_AGL_DSTA
                        Inherits cBase_VobClasses

                        Public AGL_C_Location As eRelativeLocation
                        Public Destination_VOBU As UInt32

                        Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                            Try
                                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                                Me.AGL_C_Location = (buf(offset) >> 7) And 1
                                Dim tByte As Byte = buf(offset) And 127
                                Dim bt(3) As Byte
                                bt(0) = tByte
                                bt(1) = buf(offset + 1)
                                bt(2) = buf(offset + 2)
                                bt(3) = buf(offset + 3)
                                Me.Destination_VOBU = EBC.ToUInt32(bt, 0)
                            Catch ex As Exception
                                Throw New Exception("Problem with New() cNSML_AGL_DSTA. Error: " & ex.Message, ex)
                            End Try
                        End Sub

                    End Class 'cNSML_AGL_DSTA

                End Class 'cNSML_AGLI

                ''' <summary>
                ''' 4.4.3
                ''' </summary>
                ''' <remarks></remarks>
                Public Class cHLI
                    Inherits cBase_VobClasses

                    Public Length As UInt32 = 694

                    Public HLI_GI As cHLI_GI
                    Public BTN_COLIT As cBTN_COLIT
                    Public BTNIT As cBTNIT

                    Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                        Try
                            Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                            Me.HLI_GI = New cHLI_GI(buf, offset)
                            Me.BTN_COLIT = New cBTN_COLIT(buf, offset + 22)
                            Me.BTNIT = New cBTNIT(buf, offset + 46)
                        Catch ex As Exception
                            Throw New Exception("Problem with New() cHLI. Error: " & ex.Message, ex)
                        End Try
                    End Sub

                    Public Class cHLI_GI
                        Inherits cBase_VobClasses

                        Public HLI_SS As eHLI_SS
                        Public HLI_S_PTM As cPTM
                        Public HLI_E_PTM As cPTM
                        Public BTN_SL_E_PTM As cPTM
                        Public BTN_MD As cBTN_MD
                        Public BTN_OFN As Byte
                        Public BTN_Ns As Byte
                        Public NSL_BTN_Ns As Byte
                        Public FOSL_BTNN As Byte
                        Public FOAC_BTNN As Byte

                        Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                            Try
                                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                                Me.HLI_SS = buf(offset + 1) And 3
                                Me.HLI_S_PTM = New cPTM(buf, offset + 2)
                                Me.HLI_E_PTM = New cPTM(buf, offset + 6)
                                Me.BTN_SL_E_PTM = New cPTM(buf, offset + 10)
                                Me.BTN_MD = New cBTN_MD(buf, offset + 14)
                                Me.BTN_OFN = buf(offset + 16)
                                Me.BTN_Ns = buf(offset + 17)
                                Me.NSL_BTN_Ns = buf(offset + 18)
                                Me.FOSL_BTNN = buf(offset + 19)
                                Me.FOAC_BTNN = buf(offset + 20)
                            Catch ex As Exception
                                Throw New Exception("Problem with New() cHLI_GI. Error: " & ex.Message, ex)
                            End Try
                        End Sub

                        Public Enum eHLI_SS
                            HLI_Non_Existant
                            HLI_Exists_Different_From_Prev
                            HLI_Exists_Same_As_Prev
                            HLI_Exists_Same_As_Prev_Diff_BTN_CMD
                        End Enum

                        Public Class cBTN_MD
                            Inherits cBase_VobClasses

                            Public BTNGR_Ns As eBTNGR_Ns
                            Public BTNGR1_DSP_TY As eBTNGR_DSP_TY
                            Public BTNGR2_DSP_TY As eBTNGR_DSP_TY
                            Public BTNGR3_DSP_TY As eBTNGR_DSP_TY

                            Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                                Try
                                    Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                                    Me.BTNGR_Ns = (buf(offset) >> 4) And 3
                                    Me.BTNGR1_DSP_TY = buf(offset) And 7
                                    Me.BTNGR2_DSP_TY = (buf(offset + 1) >> 4) And 7
                                    Me.BTNGR3_DSP_TY = buf(offset + 1) And 7
                                Catch ex As Exception
                                    Throw New Exception("Problem with New() cBTN_MD. Error: " & ex.Message, ex)
                                End Try
                            End Sub

                            Public Enum eBTNGR_Ns
                                One_Group = 1
                                Two_Groups = 2
                                Three_Groups = 3
                            End Enum

                            Public Enum eBTNGR_DSP_TY
                                Only_Normal_4x3
                                Only_Wide_16x9
                                Only_Letterbox
                                Both_Letterbox_and_Wide
                                Only_PanScan
                                Both_PanScan_and_Wide
                                Both_PanScan_and_Letterbox
                                All_PanScan_Letterbox_Wide
                            End Enum

                        End Class

                    End Class 'cHLI_GI

                    Public Class cBTN_COLIT
                        Inherits cBase_VobClasses

                        Public BTN_COLI() As cBTN_COLI

                        Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                            Try
                                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                                ReDim BTN_COLI(2)
                                Me.BTN_COLI(0) = New cBTN_COLI(buf, offset)
                                Me.BTN_COLI(1) = New cBTN_COLI(buf, offset + 8)
                                Me.BTN_COLI(2) = New cBTN_COLI(buf, offset + 16)
                            Catch ex As Exception
                                Throw New Exception("Problem with New() cBTN_COLIT. Error: " & ex.Message, ex)
                            End Try
                        End Sub

                        Public Overrides Function ToString() As String
                            Try
                                Dim T As Type = Me.GetType
                                Dim FI() As FieldInfo = T.GetFields()
                                Dim SB As New StringBuilder
                                For Each F As FieldInfo In FI
                                    If F.Name = "BTN_COLI" Then
                                        For i As Byte = 0 To 2
                                            SB.Append("BTN_COLI_" & i & vbNewLine & vbNewLine & "=" & BTN_COLI(i).ToString)
                                        Next
                                        SB.Append(vbNewLine)
                                    Else
                                        If F.FieldType.Namespace.StartsWith("SMT.Common.") Then
                                            SB.Append("=" & F.Name & "=" & vbNewLine)
                                            SB.Append(F.GetValue(Me).ToString & vbNewLine)
                                        Else
                                            SB.Append(F.Name & " = " & F.GetValue(Me) & vbNewLine)
                                        End If
                                    End If
                                Next
                                Return SB.ToString
                            Catch ex As Exception
                                Throw New Exception("Problem with ToString(). Error: " & ex.Message, ex)
                            End Try
                        End Function

                        Public Class cBTN_COLI
                            Inherits cBase_VobClasses

                            Public SL_COLI As cSL_COLI
                            Public AC_COLI As cAC_COLI

                            Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                                Try
                                    Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                                    Me.SL_COLI = New cSL_COLI(buf, offset)
                                    Me.AC_COLI = New cAC_COLI(buf, offset + 4)
                                Catch ex As Exception
                                    Throw New Exception("Problem with New() cBTN_COLIT. Error: " & ex.Message, ex)
                                End Try
                            End Sub

                            Public Class cSL_COLI
                                Inherits cBase_VobClasses

                                Public Emphasis_Px2_Sel_Color As Byte
                                Public Emphasis_Px1_Sel_Color As Byte
                                Public Pattern_Px_Sel_Color As Byte
                                Public Background_Px_Sel_Color As Byte
                                Public Emphasis_Px2_Sel_Contrast As Byte
                                Public Emphasis_Px1_Sel_Contrast As Byte
                                Public Pattern_Px_Sel_Contrast As Byte
                                Public Background_Px_Sel_Contrast As Byte

                                Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                                    Try
                                        Emphasis_Px2_Sel_Color = (buf(offset) >> 4) And 15
                                        Emphasis_Px1_Sel_Color = buf(offset) And 15
                                        Pattern_Px_Sel_Color = (buf(offset + 1) >> 4) And 15
                                        Background_Px_Sel_Color = buf(offset + 1) And 15
                                        Emphasis_Px2_Sel_Contrast = (buf(offset + 2) >> 4) And 15
                                        Emphasis_Px1_Sel_Contrast = buf(offset + 2) And 15
                                        Pattern_Px_Sel_Contrast = (buf(offset + 3) >> 4) And 15
                                        Background_Px_Sel_Contrast = buf(offset + 3) And 15
                                    Catch ex As Exception
                                        Throw New Exception("Problem with New() cSL_COLI. Error: " & ex.Message, ex)
                                    End Try
                                End Sub

                            End Class 'cSL_COLI

                            Public Class cAC_COLI
                                Inherits cBase_VobClasses

                                Public Emphasis_Px2_Act_Color As Byte
                                Public Emphasis_Px1_Act_Color As Byte
                                Public Pattern_Px_Act_Color As Byte
                                Public Background_Px_Act_Color As Byte
                                Public Emphasis_Px2_Act_Contrast As Byte
                                Public Emphasis_Px1_Act_Contrast As Byte
                                Public Pattern_Px_Act_Contrast As Byte
                                Public Background_Px_Act_Contrast As Byte

                                Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                                    Try
                                        Emphasis_Px2_Act_Color = (buf(offset) >> 4) And 15
                                        Emphasis_Px1_Act_Color = buf(offset) And 15
                                        Pattern_Px_Act_Color = (buf(offset + 1) >> 4) And 15
                                        Background_Px_Act_Color = buf(offset + 1) And 15
                                        Emphasis_Px2_Act_Contrast = (buf(offset + 2) >> 4) And 15
                                        Emphasis_Px1_Act_Contrast = buf(offset + 2) And 15
                                        Pattern_Px_Act_Contrast = (buf(offset + 3) >> 4) And 15
                                        Background_Px_Act_Contrast = buf(offset + 3) And 15
                                    Catch ex As Exception
                                        Throw New Exception("Problem with New() cAC_COLI. Error: " & ex.Message, ex)
                                    End Try
                                End Sub

                            End Class 'cAC_COLI

                        End Class 'cBTN_COLI

                    End Class 'cBTN_COLIT

                    Public Class cBTNIT
                        Inherits cBase_VobClasses

                        Public BTNI As List(Of cBTNI)

                        Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                            Try
                                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                                Me.BTNI = New List(Of cBTNI)
                                For i As Integer = 0 To 648 Step 18
                                    Me.BTNI.Add(New cBTNI(buf, offset + i))
                                Next
                            Catch ex As Exception
                                Throw New Exception("Problem with New() cBTNIT. Error: " & ex.Message, ex)
                            End Try
                        End Sub

                        Public Overrides Function ToString() As String
                            Try
                                Dim T As Type = Me.GetType
                                Dim FI() As FieldInfo = T.GetFields()
                                Dim SB As New StringBuilder
                                For Each F As FieldInfo In FI
                                    If F.Name = "BTNI" Then
                                        For i As Byte = 0 To 35
                                            SB.Append("BTNI_" & i & vbNewLine & vbNewLine & "=" & BTNI(i).ToString)
                                        Next
                                        SB.Append(vbNewLine)
                                    Else
                                        If F.FieldType.Namespace.StartsWith("SMT.Common.") Then
                                            SB.Append("=" & F.Name & "=" & vbNewLine)
                                            SB.Append(F.GetValue(Me).ToString & vbNewLine)
                                        Else
                                            SB.Append(F.Name & " = " & F.GetValue(Me) & vbNewLine)
                                        End If
                                    End If
                                Next
                                Return SB.ToString
                            Catch ex As Exception
                                Throw New Exception("Problem with ToString(). Error: " & ex.Message, ex)
                            End Try
                        End Function

                        Public Class cBTNI
                            Inherits cBase_VobClasses

                            Public BTN_POSI As cBTN_POSI
                            Public AJBTN_POSI As cAJBTN_POSI
                            Public BTN_CMD As cCMD

                            Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                                Try
                                    Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                                    Me.BTN_POSI = New cBTN_POSI(buf, offset)
                                    Me.AJBTN_POSI = New cAJBTN_POSI(buf, offset + 6)
                                    Me.BTN_CMD = New cCMD(buf, offset + 10)
                                Catch ex As Exception
                                    Throw New Exception("Problem with New() cBTNIT. Error: " & ex.Message, ex)
                                End Try
                            End Sub

                            Public Class cBTN_POSI
                                Inherits cBase_VobClasses

                                Public BTN_COLN As Byte
                                Public Start_X As UInt16
                                Public End_X As UInt16
                                Public Auto_Action As Boolean
                                Public Start_Y As UInt16
                                Public End_Y As UInt16

                                Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                                    Try
                                        Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                                        Dim t16 As UInt16
                                        BTN_COLN = (buf(offset) >> 6) And 3
                                        t16 = EBC.ToUInt16(buf, offset)
                                        Me.Start_X = (t16 >> 4) And 1023
                                        t16 = EBC.ToUInt16(buf, offset + 1)
                                        Me.End_X = t16 And 1023
                                        Me.Auto_Action = (buf(offset + 3) >> 6) And 1
                                        t16 = EBC.ToUInt16(buf, offset + 4)
                                        Me.Start_Y = (t16 >> 4) And 1023
                                        t16 = EBC.ToUInt16(buf, offset + 5)
                                        Me.End_Y = t16 And 1023
                                    Catch ex As Exception
                                        Throw New Exception("Problem with New() cBTNIT. Error: " & ex.Message, ex)
                                    End Try
                                End Sub

                            End Class 'cBTN_POSI

                            Public Class cAJBTN_POSI
                                Inherits cBase_VobClasses

                                Public Upper As Byte
                                Public Lower As Byte
                                Public Left As Byte
                                Public Right As Byte

                                Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                                    Try
                                        Upper = buf(offset) And 63
                                        Lower = buf(offset + 1) And 63
                                        Left = buf(offset + 2) And 63
                                        Right = buf(offset + 3) And 63
                                    Catch ex As Exception
                                        Throw New Exception("Problem with New() cAJBTN_POSI. Error: " & ex.Message, ex)
                                    End Try
                                End Sub

                            End Class 'cAJBTN_POSI

                        End Class 'cBTNI

                    End Class 'cBTNIT

                End Class 'cHLI

                ''' <summary>
                ''' 4.4.4
                ''' </summary>
                ''' <remarks></remarks>
                Public Class cRECI
                    Inherits cBase_VobClasses

                    Public Length As UInt32 = 189

                    Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                        Try
                            'TODO: implement RECI class
                        Catch ex As Exception
                            Throw New Exception("Problem with New() cRECI. Error: " & ex.Message, ex)
                        End Try
                    End Sub

                End Class 'cRECI

            End Class 'cPCIPacket

            ''' <summary>
            ''' 4.5
            ''' </summary>
            ''' <remarks></remarks>
            Public Class cDSIPacket
                Inherits cBase_VobClasses

                Public Length As UInt16 = 1017

                Public packet_start_code As UInt32 'combines packet_start_code_prefix and stream_id
                Public PES_packet_length As UInt16
                Public sub_stream_id As Byte

                Public DSI_GI As cDSI_GI
                Public SML_PBI As cSML_PBI
                Public SML_AGLI As cSML_AGLI
                Public VOBU_SRI As cVOBU_SRI
                Public SYNCI As cSYNCI

                Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                    Try
                        Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                        packet_start_code = EBC.ToUInt32(buf, offset)
                        PES_packet_length = EBC.ToUInt16(buf, offset + 4)
                        sub_stream_id = buf(offset + 6)

                        Me.DSI_GI = New cDSI_GI(buf, offset + 7)
                        Me.SML_PBI = New cSML_PBI(buf, offset + 7 + Me.DSI_GI.Length)
                        Me.SML_AGLI = New cSML_AGLI(buf, offset + 7 + Me.DSI_GI.Length + Me.SML_PBI.Length)
                        Me.VOBU_SRI = New cVOBU_SRI(buf, offset + 7 + Me.DSI_GI.Length + Me.SML_PBI.Length + Me.SML_AGLI.Length)
                        Me.SYNCI = New cSYNCI(buf, offset + 7 + Me.DSI_GI.Length + Me.SML_PBI.Length + Me.SML_AGLI.Length + Me.VOBU_SRI.Length)

                    Catch ex As Exception
                        Throw New Exception("Problem with New() cDSIPacket. Error: " & ex.Message, ex)
                    End Try
                End Sub

                ''' <summary>
                ''' 4.5.1
                ''' </summary>
                ''' <remarks></remarks>
                Public Class cDSI_GI
                    Inherits cBase_VobClasses

                    Public Length As Byte = 32

                    Public NV_PCK_SCR As UInt32
                    Public NV_PCK_LBN As UInt32
                    Public VOBU_EA As UInt32
                    Public VOBU_1STREF_EA As UInt32
                    Public VOBU_2NDREF_EA As UInt32
                    Public VOBU_3RDREF_EA As UInt32
                    Public VOBU_VOB_IDN As UInt16
                    Public VOBU_ADP_ID As Byte
                    Public VOBU_C_IDN As Byte
                    Public C_ELTM As cC_ELTM

                    Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                        Try
                            Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                            Me.NV_PCK_SCR = EBC.ToUInt32(buf, offset)
                            Me.NV_PCK_LBN = EBC.ToUInt32(buf, offset + 4)
                            Me.VOBU_EA = EBC.ToUInt32(buf, offset + 8)
                            Me.VOBU_1STREF_EA = EBC.ToUInt32(buf, offset + 12)
                            Me.VOBU_2NDREF_EA = EBC.ToUInt32(buf, offset + 16)
                            Me.VOBU_3RDREF_EA = EBC.ToUInt32(buf, offset + 20)
                            Me.VOBU_VOB_IDN = EBC.ToUInt16(buf, offset + 24)
                            Me.VOBU_ADP_ID = buf(offset + 26)
                            Me.VOBU_C_IDN = buf(offset + 27)
                            Me.C_ELTM = New cC_ELTM(buf, offset + 28)
                        Catch ex As Exception
                            Throw New Exception("Problem with New() cDSI_GI. Error: " & ex.Message, ex)
                        End Try
                    End Sub

                End Class 'cDSI_GI

                ''' <summary>
                ''' 4.5.2 - INCOMPLETE IMPLEMENTATION
                ''' </summary>
                ''' <remarks></remarks>
                Public Class cSML_PBI
                    Inherits cBase_VobClasses

                    Public Length As Byte = 148

                    'Public VOBU_SML_CAT As cVOBU_SML_CAT
                    'Public ILVU_EA As UInt32
                    'Public NXT_ILVU_SA As UInt32
                    'Public NXT_ILVU_SZ As UInt16
                    'Public VOB_V_S_PTM As cPTM
                    'Public VOB_V_E_PTM As cPTM
                    'Public VOB_A_STP_PTM1 As cPTM
                    'Public VOB_A_STP_PTM2 As cPTM
                    'Public VOB_A_GAP_LEN() As Byte

                    Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                        Try
                            'Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                            'Me.VOBU_SML_CAT = New cVOBU_SML_CAT(buf, offset)
                            'Me.ILVU_EA = EBC.ToUInt32(buf, offset + 2)
                            'Me.NXT_ILVU_SA = EBC.ToUInt32(buf, offset + 6)
                            'Me.NXT_ILVU_SZ = EBC.ToUInt16(buf, offset + 10)
                            'Me.VOB_V_S_PTM = New cPTM(buf, offset + 12)
                            'Me.VOB_V_E_PTM = New cPTM(buf, offset + 16)
                            ''TO DO: continue here
                        Catch ex As Exception
                            Throw New Exception("Problem with New() cSML_PBI. Error: " & ex.Message, ex)
                        End Try
                    End Sub

                    'Public Class cVOBU_SML_CAT
                    '    Inherits cBase_VobClasses

                    '    Public PREU_flag As Boolean
                    '    Public ILVU_flag As Boolean
                    '    Public UnitStartFlag As Boolean
                    '    Public UnitEndFlag As Boolean

                    '    Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                    '        Try
                    '            Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                    '            Me.PREU_flag = (buf(0) >> 7) And 1
                    '            Me.ILVU_flag = (buf(0) >> 6) And 1
                    '            Me.UnitStartFlag = (buf(0) >> 5) And 1
                    '            Me.UnitEndFlag = (buf(0) >> 4) And 1
                    '        Catch ex As Exception
                    '            Throw New Exception("Problem with New() cVOBU_SML_CAT. Error: " & ex.Message, ex)
                    '        End Try
                    '    End Sub

                    'End Class 'cVOBU_SML_CAT

                End Class 'cSML_PBI

                ''' <summary>
                ''' 4.5.3
                ''' </summary>
                ''' <remarks></remarks>
                Public Class cSML_AGLI
                    Inherits cBase_VobClasses

                    Public Length As Byte = 54

                    Public SML_AGL_C_DSTA As List(Of cSML_AGL_DSTA)

                    Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                        Try
                            Me.SML_AGL_C_DSTA = New List(Of cSML_AGL_DSTA)
                            For i As Integer = 0 To 8
                                Me.SML_AGL_C_DSTA.Add(New cSML_AGL_DSTA(buf, offset + (i * 6)))
                            Next
                        Catch ex As Exception
                            Throw New Exception("Problem with New() cSML_AGLI. Error: " & ex.Message, ex)
                        End Try
                    End Sub

                    Public Overrides Function ToString() As String
                        Try
                            Dim T As Type = Me.GetType
                            Dim FI() As FieldInfo = T.GetFields()
                            Dim SB As New StringBuilder
                            For Each F As FieldInfo In FI
                                If F.Name = "SML_AGL_C_DSTA" Then
                                    For i As UInt32 = 0 To SML_AGL_C_DSTA.Count - 1
                                        SB.Append("===SML_AGL_C_DSTA_" & i & vbNewLine)
                                        SB.Append(Me.SML_AGL_C_DSTA(i).ToString & vbNewLine)
                                    Next
                                Else
                                    SB.Append(F.Name & " = " & F.GetValue(Me) & vbNewLine)
                                End If
                            Next
                            Return SB.ToString
                        Catch ex As Exception
                            Throw New Exception("Problem with ToString(). Error: " & ex.Message, ex)
                        End Try
                    End Function

                    Public Class cSML_AGL_DSTA
                        Inherits cBase_VobClasses

                        Public AGL_C_location As Boolean = 0
                        Public AGL_C_DA As UInt32
                        Public AGL_C_IVLU_size As UInt16

                        Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                            Try
                                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                                Dim b(3) As Byte
                                b(0) = buf(offset) And 127
                                b(1) = buf(offset + 1)
                                b(2) = buf(offset + 2)
                                b(3) = buf(offset + 3)
                                Me.AGL_C_DA = EBC.ToUInt32(b, 0)
                                Me.AGL_C_IVLU_size = EBC.ToUInt16(buf, 5)
                            Catch ex As Exception
                                Throw New Exception("Problem with New() cSML_AGL_DSTA. Error: " & ex.Message, ex)
                            End Try
                        End Sub

                    End Class 'cSML_AGL_DSTA

                End Class 'cSML_AGLI

                ''' <summary>
                ''' 4.5.4 - NOT IMPLEMENTED
                ''' </summary>
                ''' <remarks></remarks>
                Public Class cVOBU_SRI
                    Inherits cBase_VobClasses

                    Public Length As Byte = 168

                    Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                        Try
                            Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian

                        Catch ex As Exception
                            Throw New Exception("Problem with New() cVOBU_SRI. Error: " & ex.Message, ex)
                        End Try
                    End Sub

                End Class 'cVOBU_SRI

                ''' <summary>
                ''' 4.5.5
                ''' </summary>
                ''' <remarks></remarks>
                Public Class cSYNCI
                    Inherits cBase_VobClasses

                    Public Length As Byte = 144

                    Public A_SYNCA As List(Of cA_SYNCA)
                    Public S_SYNCA As List(Of cS_SYNCA)

                    Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                        Try
                            Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                            A_SYNCA = New List(Of cA_SYNCA)
                            For i As Integer = 0 To 7
                                A_SYNCA.Add(New cA_SYNCA(buf, offset + (i * 2)))
                            Next
                            S_SYNCA = New List(Of cS_SYNCA)
                            For i As Integer = 0 To 31
                                S_SYNCA.Add(New cS_SYNCA(buf, offset + 16 + (i * 4)))
                            Next
                        Catch ex As Exception
                            Throw New Exception("Problem with New() cSYNCI. Error: " & ex.Message, ex)
                        End Try
                    End Sub

                    Public Overrides Function ToString() As String
                        Try
                            Dim T As Type = Me.GetType
                            Dim FI() As FieldInfo = T.GetFields()
                            Dim SB As New StringBuilder
                            For Each F As FieldInfo In FI
                                If F.Name = "A_SYNCA" Then
                                    For i As UInt32 = 0 To A_SYNCA.Count - 1
                                        SB.Append("===A_SYNCA_" & i & "=" & vbNewLine)
                                        SB.Append(Me.A_SYNCA(i).ToString & vbNewLine)
                                    Next
                                ElseIf F.Name = "S_SYNCA" Then
                                    For i As UInt32 = 0 To S_SYNCA.Count - 1
                                        SB.Append("===S_SYNCA_" & i & "=" & vbNewLine)
                                        SB.Append(Me.S_SYNCA(i).ToString & vbNewLine)
                                    Next
                                Else
                                    SB.Append(F.Name & " = " & F.GetValue(Me) & vbNewLine)
                                End If
                            Next
                            Return SB.ToString
                        Catch ex As Exception
                            Throw New Exception("Problem with ToString(). Error: " & ex.Message, ex)
                        End Try
                    End Function

                    Public Class cA_SYNCA
                        Inherits cBase_VobClasses

                        Public A_PCK_location As eRelativeLocation
                        Public A_PCKA As UInt16

                        Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                            Try
                                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                                Me.A_PCK_location = (buf(offset) >> 7) And 1
                                Dim b(1) As Byte
                                b(0) = buf(offset) And 127
                                b(1) = buf(offset + 1)
                                Me.A_PCKA = EBC.ToUInt16(b, 0)
                            Catch ex As Exception
                                Throw New Exception("Problem with New() cA_SYNCA. Error: " & ex.Message, ex)
                            End Try
                        End Sub

                    End Class 'cA_SYNCA

                    Public Class cS_SYNCA
                        Inherits cBase_VobClasses

                        Public S_PCK_location As eRelativeLocation
                        Public S_PCKA As UInt32

                        Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                            Try
                                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                                Me.S_PCK_location = (buf(offset) >> 7) And 1
                                Dim b(3) As Byte
                                b(0) = buf(offset) And 127
                                b(1) = buf(offset + 1)
                                b(2) = buf(offset + 2)
                                b(3) = buf(offset + 3)
                                Me.S_PCKA = EBC.ToUInt32(b, 0)
                            Catch ex As Exception
                                Throw New Exception("Problem with New() cS_SYNCA. Error: " & ex.Message, ex)
                            End Try
                        End Sub

                    End Class 'cS_SYNCA

                End Class 'cSYNCI

            End Class 'cDSIPacket

        End Class

        ''' <summary>
        ''' 5.2.3
        ''' </summary>
        ''' <remarks></remarks>
        Public Class cVideoPacket
            Inherits cBase_VobClasses

            Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)

            End Sub

        End Class 'cVideoPacket

        ''' <summary>
        ''' 5.2.4
        ''' </summary>
        ''' <remarks></remarks>
        Public Class cAudioPacket
            Inherits cBase_VobClasses

            Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)

            End Sub

        End Class 'cAudioPacket

        ''' <summary>
        ''' 5.2.5
        ''' </summary>
        ''' <remarks></remarks>
        Public Class cSubpicturePacket
            Inherits cBase_VobClasses

            Public DecodingStreamNumber As Byte

            Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                Try
                    Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                    Dim tInt As UInt32
                    tInt = EBC.ToUInt32(buf, offset)
                    Me.DecodingStreamNumber = tInt And 31
                Catch ex As Exception
                    Throw New Exception("Problem with New() cSubpicturePacket. Error: " & ex.Message, ex)
                End Try
            End Sub

        End Class 'cSubpicturePacket

        ''' <summary>
        ''' 4.4.1 (4)
        ''' </summary>
        ''' <remarks></remarks>
        Public Class cPTM
            Inherits cBase_VobClasses

            Public PresentationTime As String
            Public TimeInteger As UInt32

            Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                Try
                    Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                    Me.TimeInteger = EBC.ToUInt32(buf, offset)
                    Me.PresentationTime = TimeInteger / 90000
                Catch ex As Exception
                    Throw New Exception("Problem with New() cPTM. Error: " & ex.Message, ex)
                End Try
            End Sub

        End Class 'cPTM

        ''' <summary>
        ''' 4.4.1 (7)
        ''' </summary>
        ''' <remarks></remarks>
        Public Class cC_ELTM
            Inherits cBase_VobClasses

            Public Hours As Byte
            Public Minutes As Byte
            Public Seconds As Byte
            Public Frames As Byte
            Public Framerate As eFramerate

            Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                Try
                    Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                    Hours = (((buf(offset) >> 4) And 15) * 10) + (buf(offset) And 15)
                    Minutes = (((buf(offset + 1) >> 4) And 15) * 10) + (buf(offset + 1) And 15)
                    Seconds = (((buf(offset + 2) >> 4) And 15) * 10) + (buf(offset + 2) And 15)
                    Frames = (((buf(offset + 3) >> 4) And 3) * 10) + (buf(offset + 3) And 15)
                    Framerate = (buf(offset + 3) >> 6) And 3
                Catch ex As Exception
                    Throw New Exception("Problem with New() cC_ELTM. Error: " & ex.Message, ex)
                End Try
            End Sub

            Public Enum eFramerate
                _25 = 1
                _30 = 3
            End Enum

        End Class 'cC_ELTM

        Public MustInherit Class cBase_VobClasses

            Public Overridable Shadows Function ToString() As String
                Try
                    Dim T As Type = Me.GetType
                    Dim FI() As FieldInfo = T.GetFields()
                    Dim SB As New StringBuilder
                    For Each F As FieldInfo In FI
                        If F.FieldType.Namespace.StartsWith("SMT.Common.") Then
                            If F.FieldType.Name.StartsWith("e") Then
                                'enum
                                SB.Append("=" & F.Name & "=" & vbNewLine)
                                SB.Append(F.GetValue(Me).ToString & vbNewLine)
                            ElseIf F.FieldType.Name = "cCMD" Then
                                Dim c As cCMD = F.GetValue(Me)
                                SB.Append("=" & F.Name & "=" & vbNewLine)
                                SB.Append(c.CommandName & vbNewLine)
                            Else
                                'class
                                SB.Append("=" & F.Name & "=" & vbNewLine)
                                SB.Append(CType(F.GetValue(Me), cBase_VobClasses).ToString & vbNewLine)
                            End If
                        Else
                            SB.Append(F.Name & " = " & F.GetValue(Me) & vbNewLine)
                        End If
                    Next
                    Return SB.ToString
                Catch ex As Exception
                    Throw New Exception("Problem with ToString(). Error: " & ex.Message, ex)
                End Try
            End Function

        End Class 'cBase_VobClasses

#End Region 'CLASSES

#Region "ENUMs"

        Public Enum ePackType
            Navigation
            Video
            Audio
            Subpicture
            Unknown
        End Enum

        Public Enum ePacketType
            Navigation
            Video
            Audio_PCM
            Audio_MPEG
            Audio_AC3
            Audio_DTS
            Audio_SDDS
            Subpicture
        End Enum

        Public Enum eRelativeLocation
            After_this_NV_PCK
            Before_or_current_NV_PCK
        End Enum

#End Region 'ENUMs

    End Class 'cVOBPack

#Region "SUBPICTURE EXTRACTION"

    Public Class cSubpictureExtractor

        Public SPUs As List(Of cSPU)

        Private IsPopulatingSPU As Boolean = False

        Public Function ExtractSubStream(ByVal VOBPath As String, ByVal StreamNumber As Byte, ByVal ExportPath As String) As Integer
            Try
                SPUs = New List(Of cSPU)
                Dim VOB As New cVOB(VOBPath)
                Dim FS As New FileStream(VOBPath, FileMode.Open)
                Dim BR As New BinaryReader(FS)
                Dim LoadSize As UInt32 = 0
                Dim tSPU As cSPU

                For Each P As cVOBPack In VOB.Packs

                    If P._Type = cVOBPack.ePackType.Subpicture AndAlso CType(P.PacketHeader, cVOBPack.cPacketHeader_Type2).decoding_stream_number = StreamNumber Then

                        FS.Position = P.Offset + P.PackHeader.Length + P.PacketHeader.Length + 1

                        LoadSize = 2048 - (P.PackHeader.Length + P.PacketHeader.Length)

                        If tSPU Is Nothing Then
                            tSPU = New cSPU()
                        ElseIf tSPU.IsPopulated Then
                            SPUs.Add(tSPU)
                            tSPU = New cSPU()
                        End If

                        'add data to the current spu
                        tSPU.AddPacketData(BR, LoadSize)

                    End If

                Next

                Return SPUs.Count

            Catch ex As Exception
                Throw New Exception("Problem with ExtractSubStream(). Error: " & ex.Message, ex)
            End Try
        End Function

    End Class

#Region "CLASSES"

    ''' <summary>
    ''' 5.4.3
    ''' </summary>
    ''' <remarks></remarks>
    Public Class cSPU

        Public SPUH As cSPUH
        Public PXD() As Byte
        Public SP_DCSQT As cSP_DCSQT

        Public IsPopulated As Boolean = False
        Public IsInitialized As Boolean = False

        Public ReadOnly Property PXDBytesNeeded() As UInt32
            Get
                Return Me.SPUH.PXD_length - Me.PXD.Length
            End Get
        End Property

        Public ReadOnly Property SP_Image() As Bitmap
            Get
                Return Nothing
            End Get
        End Property

        'Public Sub New(ByRef BR As BinaryReader, ByVal Count As UInt32)
        '    Try
        '        Dim buf() As Byte = BR.ReadBytes(Count)
        '        Me.SPUH = New cSPUH(buf, 0)
        '        ReDim PXD(-1)
        '    Catch ex As Exception
        '        Throw New Exception("Problem with New() cSPU. Error: " & ex.Message, ex)
        '    End Try
        'End Sub

        Public Sub AddPacketData(ByRef BR As BinaryReader, ByVal Count As UInt32)
            Try
                Dim buf() As Byte = BR.ReadBytes(Count)

                Dim offset As UInt32 = 0
                If Me.SPUH Is Nothing Then
                    Me.SPUH = New cSPUH(buf, offset)
                    offset += 4
                    ReDim PXD(-1)
                    Me.IsInitialized = True
                End If

                'does this nibble include everything?
                If Count >= Me.SPUH.SPU_SZ Then
                    'yes
                    ReDim PXD(Me.SPUH.PXD_length - 1)
                    Array.Copy(buf, offset, PXD, 0, Me.SPUH.PXD_length)
                    Me.SP_DCSQT = New cSP_DCSQT(buf, offset + Me.SPUH.PXD_length)
                    Me.IsPopulated = True
                Else
                    'no

                    'does this complete the SPU?
                    If Count >= Me.PXDBytesNeeded Then
                        'yes
                        'copy in what's remaining of pxd data
                        Dim PXDLength As UInt16 = Count - Me.SPUH.DCSQT_length
                        ReDim Preserve PXD(PXD.Length - 1 + PXDLength)
                        Array.Copy(buf, 0, PXD, PXD.Length - PXDLength, PXDLength)

                        'do nothing for now with the SP_DCSQT data
                        'TO DO: later

                        Me.IsPopulated = True
                    Else
                        'no
                        'just copy in the bytes
                        ReDim Preserve PXD(PXD.Length - 1 + Count)
                        Array.Copy(buf, 0, PXD, PXD.Length - Count, Count)
                    End If

                End If

            Catch ex As Exception
                Throw New Exception("Problem with AddPacketData(). Error: " & ex.Message, ex)
            End Try
        End Sub

        ''' <summary>
        ''' 5.4.3.1
        ''' </summary>
        ''' <remarks></remarks>
        Public Class cSPUH

            Public SPU_SZ As UInt16
            Public SP_DCSQT_SA As UInt16

            Public PXD_length As UInt32
            Public DCSQT_length As UInt32

            Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                Try
                    Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                    Me.SPU_SZ = EBC.ToUInt16(buf, offset)
                    Me.SP_DCSQT_SA = EBC.ToUInt16(buf, offset + 2)

                    If SPU_SZ > 0 Then
                        Me.DCSQT_length = Me.SPU_SZ - Me.SP_DCSQT_SA
                        Me.PXD_length = Me.SPU_SZ - 4 - Me.DCSQT_length
                    End If

                Catch ex As Exception
                    Throw New Exception("Problem with New() cSPUH. Error: " & ex.Message, ex)
                End Try
            End Sub

        End Class

        ''' <summary>
        ''' 5.4.3.3 - NOT FULLY IMPLEMENTED
        ''' </summary>
        ''' <remarks>It's kindofabitch. A lot of the work is done but the offsets need more attention. Just dont feel 
        ''' like doing it right now and there isn't a pressing need.</remarks>
        Public Class cSP_DCSQT

            Public SP_DCSQs As List(Of cSP_DCSQ)

            Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                Try
                    'SP_DCSQs = New List(Of cSP_DCSQ)
                    'Dim tDC As cSP_DCSQT
                    'While 1 = 1
                    '    tDC = New cSP_DCSQ
                    '    SP_DCSQs.Add(
                    'End While

                Catch ex As Exception
                    Throw New Exception("Problem with New() cSP_DCSQT. Error: " & ex.Message, ex)
                End Try
            End Sub

            ''' <summary>
            ''' 5.4.3.3-2
            ''' </summary>
            ''' <remarks></remarks>
            Public Class cSP_DCSQ

                Public ReadOnly Property Length() As UInt16
                    Get
                        Dim out As UInt16 = 4
                        For Each DCC As cSP_DCCMD In SP_DCCMDs
                            out += DCC.Length
                        Next
                        Return out
                    End Get
                End Property

                Public SP_DCSQ_STM As UInt16
                Public SP_NXT_DCSQ_SA As UInt16
                Public SP_DCCMDs As List(Of cSP_DCCMD)

                Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                    Try
                        Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                        Me.SP_DCSQ_STM = EBC.ToUInt16(buf, offset)
                        Me.SP_NXT_DCSQ_SA = EBC.ToUInt16(buf, offset + 2)

                        Me.SP_DCCMDs = New List(Of cSP_DCCMD)
                        Dim tDCC As cSP_DCCMD
                        While offset < SP_NXT_DCSQ_SA - 1
                            tDCC = New cSP_DCCMD(buf, offset)
                            SP_DCCMDs.Add(tDCC)
                            offset += tDCC.Length
                        End While

                    Catch ex As Exception
                        Throw New Exception("Problem with New() cSP_DCSQ. Error: " & ex.Message, ex)
                    End Try
                End Sub

                ''' <summary>
                ''' 5.4.3.4
                ''' </summary>
                ''' <remarks></remarks>
                Public Class cSP_DCCMD

                    Public _Type As eSP_DCCMD_Type
                    Public Length As UInt16

                    Public SET_COLOR_ex As cSET_COLOR_ex
                    Public SET_CONTRA_ex As cSET_CONTR_ex
                    Public SET_DAREA_ex As cSET_DAREA_ex
                    Public SET_DSPXA_ex As cSET_DSPXA_ex
                    Public CHG_COLCON_ex As cCHG_COLCON_ex

                    Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                        Try
                            Select Case buf(0)
                                Case 0
                                    _Type = eSP_DCCMD_Type.FSTA_DSP
                                    Length = 1
                                Case 1
                                    _Type = eSP_DCCMD_Type.STA_DSP
                                    Length = 1
                                Case 2
                                    _Type = eSP_DCCMD_Type.STP_DSP
                                    Length = 1
                                Case 3
                                    _Type = eSP_DCCMD_Type.SET_COLOR
                                    Me.SET_COLOR_ex = New cSET_COLOR_ex(buf, offset + 1)
                                    Length = 3
                                Case 4
                                    _Type = eSP_DCCMD_Type.SET_CONTR
                                    Me.SET_CONTRA_ex = New cSET_CONTR_ex(buf, offset + 1)
                                    Length = 3
                                Case 5
                                    _Type = eSP_DCCMD_Type.SET_DAREA
                                    Me.SET_DAREA_ex = New cSET_DAREA_ex(buf, offset + 1)
                                    Length = 7
                                Case 6
                                    _Type = eSP_DCCMD_Type.SET_DSPXA
                                    Me.SET_DSPXA_ex = New cSET_DSPXA_ex(buf, offset + 1)
                                    Length = 5
                                Case 7
                                    _Type = eSP_DCCMD_Type.CHG_COLCON
                                    Me.CHG_COLCON_ex = New cCHG_COLCON_ex(buf, offset + 1)
                                    Length = 1 + Me.CHG_COLCON_ex.Length
                                Case 255
                                    _Type = eSP_DCCMD_Type.CMD_END
                                    Length = 1
                            End Select
                        Catch ex As Exception
                            Throw New Exception("Problem with New() cSP_DCCMD. Error: " & ex.Message, ex)
                        End Try
                    End Sub

                    Public Class cSET_COLOR_ex

                        Public Emphasis_Px2_Color_Code As Byte
                        Public Emphasis_Px1_Color_Code As Byte
                        Public Pattern_Px_Color_Code As Byte
                        Public Background_Px_Color_Code As Byte

                        Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                            Try
                                Me.Emphasis_Px2_Color_Code = (buf(0) >> 4) And 15
                                Me.Emphasis_Px1_Color_Code = buf(0) And 15
                                Me.Pattern_Px_Color_Code = (buf(1) >> 4) And 15
                                Me.Background_Px_Color_Code = buf(1) And 15
                            Catch ex As Exception
                                Throw New Exception("Problem with New() cSET_COLOR_ex. Error: " & ex.Message, ex)
                            End Try
                        End Sub

                    End Class

                    Public Class cSET_CONTR_ex

                        Public Emphasis_Px2_Contrast_Code As Byte
                        Public Emphasis_Px1_Contrast_Code As Byte
                        Public Pattern_Px_Contrast_Code As Byte
                        Public Background_Px_Contrast_Code As Byte

                        Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                            Try
                                Me.Emphasis_Px2_Contrast_Code = (buf(0) >> 4) And 15
                                Me.Emphasis_Px1_Contrast_Code = buf(0) And 15
                                Me.Pattern_Px_Contrast_Code = (buf(1) >> 4) And 15
                                Me.Background_Px_Contrast_Code = buf(1) And 15
                            Catch ex As Exception
                                Throw New Exception("Problem with New() cSET_CONTR_ex. Error: " & ex.Message, ex)
                            End Try
                        End Sub

                    End Class

                    Public Class cSET_DAREA_ex

                        Public Start_X As UInt16
                        Public End_X As UInt16
                        Public Start_Y As UInt16
                        Public End_Y As UInt16

                        Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                            Try
                                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                                Dim t16 As UInt16
                                t16 = EBC.ToUInt16(buf, offset)
                                Me.Start_X = (t16 >> 4) And 1023
                                t16 = EBC.ToUInt16(buf, offset + 1)
                                Me.End_X = t16 And 1023
                                t16 = EBC.ToUInt16(buf, offset + 4)
                                Me.Start_Y = (t16 >> 4) And 1023
                                t16 = EBC.ToUInt16(buf, offset + 5)
                                Me.End_Y = t16 And 1023
                            Catch ex As Exception
                                Throw New Exception("Problem with New() cSET_DAREA_ex. Error: " & ex.Message, ex)
                            End Try
                        End Sub

                    End Class

                    Public Class cSET_DSPXA_ex

                        Public FirstPixelTopField As UInt16
                        Public FirstPixelBottomField As UInt16

                        Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                            Try
                                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                                Me.FirstPixelTopField = EBC.ToUInt16(buf, 0)
                                Me.FirstPixelBottomField = EBC.ToUInt16(buf, 2)
                            Catch ex As Exception
                                Throw New Exception("Problem with New() cSET_DSPXA_ex. Error: " & ex.Message, ex)
                            End Try
                        End Sub

                    End Class

                    ''' <summary>
                    ''' NOT FULLY IMPLEMENTED
                    ''' </summary>
                    ''' <remarks></remarks>
                    Public Class cCHG_COLCON_ex

                        Public Length As UInt16

                        Public ExtendedFieldSize As UInt16
                        Public PixelControlData() As Byte 'not hammered out

                        Public Sub New(ByRef buf() As Byte, ByVal offset As UInt32)
                            Try
                                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                                Me.ExtendedFieldSize = EBC.ToUInt16(buf, offset)
                                ReDim Me.PixelControlData(Me.ExtendedFieldSize - 1)
                                Array.Copy(buf, offset + 2, Me.PixelControlData, 0, Me.ExtendedFieldSize)
                                Length = 2 + Me.ExtendedFieldSize
                            Catch ex As Exception
                                Throw New Exception("Problem with New() cCHG_COLCON_ex. Error: " & ex.Message, ex)
                            End Try
                        End Sub

                    End Class

                    Public Enum eSP_DCCMD_Type
                        FSTA_DSP
                        STA_DSP
                        STP_DSP
                        SET_COLOR
                        SET_CONTR
                        SET_DAREA
                        SET_DSPXA
                        CHG_COLCON
                        CMD_END
                    End Enum

                End Class

            End Class

        End Class

    End Class

#End Region 'CLASSES

#End Region 'SUBPICTURE EXTRACTION

End Namespace
