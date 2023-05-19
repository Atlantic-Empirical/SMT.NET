Imports System.IO
Imports Microsoft.VisualBasic
Imports SMT.Multimedia.Utility.Timecode
Imports SMT.DotNet.Utility
Imports System.Text
Imports SMT.Multimedia.Players.DVD.Classes
Imports SMT.Multimedia.Formats.DVD.Globalization
Imports SMT.Multimedia.Enums

Namespace Multimedia.Formats.DVD.IFO

#Region "Classes"

#Region "DVD"

    Public Class cDVD

        Public VMGM As cVMGM
        Public VTSs() As cVTS
        Public DVDPath As String

        Public Sub New(ByVal nPath As String) 'needs video_ts directory
            Try
                If Path.HasExtension(nPath) Then nPath = Path.GetDirectoryName(nPath)
                DVDPath = nPath
                VMGM = New cVMGM(nPath & "\video_ts.ifo")
                ReDim VTSs(-1)
                For i As Short = 1 To VMGM.NumberOfTitleSets
                    ReDim Preserve VTSs(i - 1)
                    VTSs(i - 1) = New cVTS(nPath & "\VTS_" & PadString(i, 2, "0", True) & "_0.IFO", i)
                Next

            Catch ex As Exception
                Throw New Exception("Problem with New cDVD. Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Sub New(ByVal IFOStreams() As Stream)
            Try
                VMGM = New cVMGM(IFOStreams(0))
                ReDim VTSs(VMGM.NumberOfTitleSets - 1)
                For i As Short = 1 To VMGM.NumberOfTitleSets
                    VTSs(i - 1) = New cVTS(IFOStreams(i), i)
                Next
            Catch ex As Exception
                Throw New Exception("Problem with New() cDVD. Error: " & ex.Message, ex)
            End Try
        End Sub

#Region "PUBLIC FUNCTIONS"

        Public Function GetGTTDuration(ByVal GTTN As Integer) As cTimecode
            Try
                Dim PGC As cPGC = FindPGCByGTT(GTTN)
                If PGC Is Nothing Then Return Nothing
                Return PGC.PlaybackTime_TC
            Catch ex As Exception
                Throw New Exception("Problem with GetGTTDuration(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function GetTRTFromCellColl(ByVal LBCell As Short, ByVal Cells As colCellPlaybackInfo) As cTimecode
            Dim oTC As cTimecode = New cTimecode
            oTC.Framerate = Cells.Item(0).CellPlaybackTime.Framerate
            Dim SkipForAngleBlock As Boolean = False
            Dim LastTC As New cTimecode
            Dim c As cCellPlaybackInfo
            For i As Short = 0 To LBCell - 1
                c = Cells(i)
                If c.BlockType.ToString.ToLower = "angle" Then
                    If LastTC.ToString <> c.CellPlaybackTime.ToString Then
                        SkipForAngleBlock = False
                    Else
                        SkipForAngleBlock = True
                    End If
                End If

                If Not SkipForAngleBlock Then
                    oTC = AddTimecode(oTC, c.CellPlaybackTime)
                    LastTC = c.CellPlaybackTime
                End If
                SkipForAngleBlock = False
            Next
            'For i As Short = 1 To LBCell - 1
            '    oTC = AddTimecode(oTC, Cells(i).CellPlaybackTime)
            'Next
            Return oTC
        End Function

        Public Function FindPGCByGTT(ByVal GTTN As Byte) As cPGC
            Try
                Dim GTT As cGlobalTT = VMGM.GlobalTTs(GTTN - 1)
                Dim PGCs() As cPGC = VTSs(GTT.VTSN - 1).GetPGCsByVTS_TTN(GTT.VTS_TTN)
                If PGCs.Length = 1 Then
                    PGCs(0).ParentVTS = GTT.VTSN
                    Return PGCs(0)
                Else
                    Return Nothing
                End If
            Catch ex As Exception
                Throw New Exception("Problem with FindPGCByGTT(). Error: " & ex.Message)
            End Try
        End Function

        Public Function GetSPCount(ByVal VTSNo As Byte, ByVal TitleSpace As Boolean) As Byte
            Try
                If TitleSpace Then
                    Return VTSs(VTSNo - 1).VTS_SubpictureStreamCount
                Else
                    Return VTSs(VTSNo - 1).VTSM_SubpictureStreamCount
                End If
            Catch ex As Exception
                Debug.WriteLine("Problem with GetSPCount. Error: " & ex.Message)
            End Try
        End Function

        Public Function GetRegion(ByVal AsMask As Boolean) As String
            Try
                'Dim VTS1Path As String = System.IO.Path.GetDirectoryName(VideoTSIFOPath) & "\VTS_01_0.IFO"
                'Dim IP As New IFOProcessing.cIFOProcessing
                'Dim VTS1 As cIFO = IP.ParseIFO(VTS1Path)

                'Dim VP As New cVMGMProcessing
                'Dim VMGM As cVMGM = VP.ParseVMGM(VideoTSIFOPath)

                If AsMask Then
                    Return VMGM.RegionMask
                Else
                    Dim sb As New StringBuilder
                    If VMGM.R1 Then
                        sb.Append("1")
                    End If
                    If VMGM.R2 Then
                        sb.Append("2")
                    End If
                    If VMGM.R3 Then
                        sb.Append("3")
                    End If
                    If VMGM.R4 Then
                        sb.Append("4")
                    End If
                    If VMGM.R5 Then
                        sb.Append("5")
                    End If
                    If VMGM.R6 Then
                        sb.Append("6")
                    End If
                    If VMGM.R7 Then
                        sb.Append("7")
                    End If
                    If VMGM.R8 Then
                        sb.Append("8")
                    End If
                    Return sb.ToString
                End If
            Catch ex As Exception
                Throw New Exception("Problem with GetRegion(). Error: " & ex.Message)
                Return Nothing
            End Try
        End Function

#End Region 'PUBLIC FUNCTIONS

#Region "PUBLIC PROPERTIES"

        Public ReadOnly Property HasCallSSRSM127() As Boolean
            Get
                'VIDEO TITLE SETS
                For Each tVTS As cVTS In VTSs
                    If tVTS.HasCallSSRSM127 Then Return True
                Next
            End Get
        End Property

        Public ReadOnly Property CallSSRSM127_Location() As cDVDLocation()
            Get
                Dim out(-1) As cDVDLocation

                ''CHECK IN VMGM
                'If VMGM.VMGM_PGCI_UT.LUs IsNot Nothing Then
                '    For i As Byte = 0 To UBound(VMGM.VMGM_PGCI_UT.LUs)
                '        If VMGM.VMGM_PGCI_UT.LUs(i).HasCallSSRSM127 Then
                '            out.VMGM = True
                '            out.LanguageUnit = i
                '            out.LanguageUnitString = VMGM.VMGM_PGCI_UT.LU_SRPs(i).VXXM_LCD
                '            out.PGCN = VMGM.VMGM_PGCI_UT.LUs(i).CallSSRSM127_PGCN
                '            Return out
                '        End If
                '    Next
                'End If

                'CHECK IN VTSs
                For i As Byte = 0 To UBound(VTSs)
                    If VTSs(i).HasCallSSRSM127 Then
                        Dim Locations() As cVTS.cVTSLocation = VTSs(i).CallSSRSM127_Location

                        For Each Location As cVTS.cVTSLocation In Locations
                            ReDim Preserve out(UBound(out) + 1)
                            out(UBound(out)) = New cDVDLocation
                            out(UBound(out)).VMGM = False
                            out(UBound(out)).VTS = i
                            out(UBound(out)).PGCN = Location.PGCN

                            'out.MenuSpace = temp.MenuSpace
                            'If out.MenuSpace Then
                            '    out.LanguageUnit = temp.LanguageUnit
                            '    out.LanguageUnitString = temp.LanguageUnitString
                            'Else
                            For ix As Byte = 0 To VMGM.GlobalTTs.Count - 1
                                If VMGM.GlobalTTs(ix).VTS_TTN = VTSs(i).PGCs(out(UBound(out)).PGCN).VTS_TTN Then
                                    out(UBound(out)).GTTN = ix
                                End If
                            Next
                            'End If

                        Next

                        Return out
                    End If
                Next
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property VideoStandard() As eVideoStandard
            Get
                Try

                    If VMGM.VidAttsVMGM_VOBS.VideoStandard = eVideoStandard.NTSC And VTSs(0).VTS_VideoAttributes.VideoStandard = eVideoStandard.NTSC Then
                        Return eVideoStandard.NTSC
                    ElseIf VMGM.VidAttsVMGM_VOBS.VideoStandard = eVideoStandard.PAL And VTSs(0).VTS_VideoAttributes.VideoStandard = eVideoStandard.PAL Then
                        Return eVideoStandard.PAL
                    ElseIf VMGM.VidAttsVMGM_VOBS.VideoStandard = eVideoStandard.NTSC And VTSs(0).VTS_VideoAttributes.VideoStandard = eVideoStandard.PAL Then
                        'VMG = NTSC
                        'VTS1 = PAL
                        Return eVideoStandard.PAL
                    ElseIf VMGM.VidAttsVMGM_VOBS.VideoStandard = eVideoStandard.PAL And VTSs(0).VTS_VideoAttributes.VideoStandard = eVideoStandard.NTSC Then
                        'VMG = PAL
                        'VTS1 = NTSC
                        Return eVideoStandard.NTSC
                    End If
                Catch ex As Exception
                    Throw New Exception("Problem with VideoStandard(). Error: " & ex.Message)
                    Return Nothing
                End Try
            End Get
        End Property

#End Region 'PUBLIC PROPERTIES

    End Class

#End Region 'DVD

#Region "VTS"

    Public Class cVTS

        Public Name As String
        Const SectorSize As Short = 2048

        Public Overloads Overrides Function ToString() As String
            Return Name
        End Function

        Public ID As Short
        Public TitleCount As Short
        Private _PGCCount_TitleSpace As Short
        Public Property PGCCount_TitleSpace() As Short
            Get
                Return _PGCCount_TitleSpace
            End Get
            Set(ByVal Value As Short)
                _PGCCount_TitleSpace = Value
            End Set
        End Property

        Public ReadOnly Property PGCCount_MenuSpace() As Short
            Get
                'todo: IMPORTANT - this is just using LU 0. It should be using the currently active LU.
                Return Me.VTSM_PGCI_UT.LUs(0).PGCCount
            End Get
        End Property

        Public Titles As colTitles
        Public PGCs As colPGCs

        Public TitleSetLastSector As UInt32
        Public tester32 As UInt32
        Public IFOLastSector As UInt32
        Public Version As String
        Public VTSCategory As String
        Public VTS_MATEndByteAddress As UInt32
        Public MenuVOBStartSector As UInt32
        Public TitleVOBStartSector As UInt32

        Public VTS_PTT_SRPT_SectorPointer As UInt32 '(table of Titles and Chapters)
        Public VTS_PGCIT_SectorPointer As UInt32 '(Title Program Chain table)
        Public VTSM_PGCI_UT_SectorPointer As UInt32 '(Menu Program Chain table)
        Public VTS_TMAPTI_SectorPointer As UInt32 '(time map)
        Public VTSM_C_ADT_SectorPointer As UInt32 '(menu cell address table)
        Public VTSM_VOBU_ADMAP_SectorPointer As UInt32 '(menu VOBU address map)
        Public VTS_C_ADT_SectorPointer As UInt32 '(title set cell address table)
        Public VTS_VOBU_ADMAP_SectorPointer As UInt32 '(title set VOBU address map)

        Public VTSM_VideoAttributes As cVideoAttributes
        Public VTSM_AudioStreamCount As UInt16
        Public VTSM_AudioAttributes As cMenuAudioAttributes
        Public VTSM_SubpictureStreamCount As UInt16
        Public VTSM_SubpictureAttributes As cSubpictureAttributes

        Public VTS_VideoAttributes As cVideoAttributes
        Public VTS_AudioStreamCount As UInt16
        Public VTS_AudioAttributes As cTitleAudioAttributes
        Public VTS_SubpictureStreamCount As UInt16
        Public VTS_SubpictureAttributes As cSubpictureAttributes

        Public TT_EndOfPTTTable As UInt32
        Public TT_StartBytes() As UInt32

        Public VTSM_PGCI_UT As cVXXM_PGCI_UT
        Public VTS_PGCIT As cVTS_PGCIT

        Public Sub New(ByVal nVTSIFOPath As String, ByVal VTSNumber As Integer)
            Me.New(New FileStream(nVTSIFOPath, FileMode.Open, FileAccess.Read), VTSNumber)
        End Sub

        Public Sub New(ByRef VTS_Stream As Stream, ByVal VTSNumber As Integer)
            Try
                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian

                Name = "VTS_" & PadString(VTSNumber, 2, "0", True) & "_0.IFO"
                Titles = New colTitles
                PGCs = New colPGCs

                Dim IFO() As Byte
                Dim B1, B2, B3, B4 As String
                ReDim IFO(VTS_Stream.Length - 1)
                VTS_Stream.Read(IFO, 0, VTS_Stream.Length - 1)
                VTS_Stream.Close()
                VTS_Stream = Nothing

                '---------------------------------------------------------------------------------
                'VTS
                '---------------------------------------------------------------------------------

                ID = PadString(VTSNumber, 2, "0", True)
                TitleSetLastSector = EBC.ToUInt32(IFO, 12)

                IFOLastSector = EBC.ToUInt32(IFO, 16)
                Version = "v" & CInt(Microsoft.VisualBasic.Left(GetByte(IFO(33)), 1)) & "." & CInt(Microsoft.VisualBasic.Right(GetByte(IFO(33)), 1))
                VTSCategory = EBC.ToUInt32(IFO, 34)

                If VTSCategory = 0 Then
                    VTSCategory = "Unspecified"
                Else
                    VTSCategory = "Karaoke"
                End If

                VTS_MATEndByteAddress = EBC.ToUInt32(IFO, 128)
                MenuVOBStartSector = EBC.ToUInt32(IFO, 192)
                TitleVOBStartSector = EBC.ToUInt32(IFO, 196)
                VTS_PTT_SRPT_SectorPointer = SectorSize * EBC.ToUInt32(IFO, 200)
                VTS_PGCIT_SectorPointer = SectorSize * EBC.ToUInt32(IFO, 204)
                VTSM_PGCI_UT_SectorPointer = SectorSize * EBC.ToUInt32(IFO, 208)
                VTS_TMAPTI_SectorPointer = SectorSize * EBC.ToUInt32(IFO, 212)
                VTSM_C_ADT_SectorPointer = SectorSize * EBC.ToUInt32(IFO, 216)
                VTSM_VOBU_ADMAP_SectorPointer = SectorSize * EBC.ToUInt32(IFO, 220)
                VTS_C_ADT_SectorPointer = SectorSize * EBC.ToUInt32(IFO, 224)
                VTS_VOBU_ADMAP_SectorPointer = SectorSize * EBC.ToUInt32(IFO, 228)

                VTSM_PGCI_UT = New cVXXM_PGCI_UT(IFO, VTSM_PGCI_UT_SectorPointer)
                VTS_PGCIT = New cVTS_PGCIT(IFO, VTS_PGCIT_SectorPointer)

                VTSM_VideoAttributes = New cVideoAttributes(IFO, 256)

                VTSM_AudioStreamCount = EBC.ToUInt16(IFO, 258)
                VTSM_AudioAttributes = New cMenuAudioAttributes(IFO, 260)

                VTSM_SubpictureStreamCount = EBC.ToUInt16(IFO, 340)
                VTSM_SubpictureAttributes = New cSubpictureAttributes(IFO, 342, VTSM_SubpictureStreamCount)

                VTS_VideoAttributes = New cVideoAttributes(IFO, 512)

                VTS_AudioStreamCount = EBC.ToUInt16(IFO, 514)
                VTS_AudioAttributes = New cTitleAudioAttributes(IFO, 516)

                VTS_SubpictureStreamCount = EBC.ToUInt16(IFO, 596)
                VTS_SubpictureAttributes = New cSubpictureAttributes(IFO, 598, VTS_SubpictureStreamCount)

                'ReDim tByteA(191)
                'System.Array.Copy(IFO, 792, tByteA, 0, 192)
                'MultichannelExtension = BytesToMultichannelExtension(tByteA)

                TitleCount = EBC.ToUInt16(IFO, VTS_PTT_SRPT_SectorPointer)

                PGCCount_TitleSpace = EBC.ToUInt16(IFO, VTS_PGCIT_SectorPointer)

                Dim VTS_PGC_Offsets(PGCCount_TitleSpace - 1) As Long

                Dim StartOfVTS_PGCI As Short = VTS_PGCIT_SectorPointer

                Dim Plus12 As Short = StartOfVTS_PGCI + 12

                For i As Short = 0 To PGCCount_TitleSpace - 1
                    B1 = GetByte(IFO(Plus12 + (i * 8)))
                    B2 = GetByte(IFO(Plus12 + (i * 8) + 1))
                    B3 = GetByte(IFO(Plus12 + (i * 8) + 2))
                    B4 = GetByte(IFO(Plus12 + (i * 8) + 3))

                    If B1.Length = 1 Then B1 = "0" & B1
                    If B2.Length = 1 Then B2 = "0" & B2
                    If B3.Length = 1 Then B3 = "0" & B3
                    If B4.Length = 1 Then B4 = "0" & B4

                    VTS_PGC_Offsets(i) = CInt("&h" & B1 & B2 & B3 & B4)
                    'Debug.WriteLine(VTS_PGC_Offsets(i))
                Next

                '---------------------------------------------------------------------------------
                'END VTS
                '---------------------------------------------------------------------------------

                '---------------------------------------------------------------------------------
                'PGCs
                '---------------------------------------------------------------------------------

                Dim P As cPGC

                For i As Short = 0 To PGCCount_TitleSpace - 1

                    'After review of the spec it appears that this value just provides the VTS_TTN
                    Dim PGCC As String = DecimalToBinary(CInt("&h" & GetByte(IFO(StartOfVTS_PGCI + 8 + (i * 8)))))
                    PGCC = Microsoft.VisualBasic.Right(PGCC, Len(PGCC) - 1)
                    P = New cPGC(IFO, VTS_PGC_Offsets(i) + StartOfVTS_PGCI, i + 1, ID)
                    P.VTS_TTN = BinToDec(PGCC)
                    PGCs.Add(P)

                Next

                '---------------------------------------------------------------------------------
                'END PGCs
                '---------------------------------------------------------------------------------

                '---------------------------------------------------------------------------------
                'Titles
                '---------------------------------------------------------------------------------

                Dim TTStart As Integer = VTS_PTT_SRPT_SectorPointer
                Dim TT As cTitle
                TitleCount = EBC.ToUInt16(IFO, TTStart)

                TT_EndOfPTTTable = EBC.ToUInt32(IFO, TTStart + 4)
                ReDim TT_StartBytes(TitleCount - 1)

                For i As Short = 0 To TitleCount - 1
                    B1 = GetByte(IFO(TTStart + 8 + (4 * i)))
                    B2 = GetByte(IFO(TTStart + 8 + 1 + (4 * i)))
                    B3 = GetByte(IFO(TTStart + 8 + 2 + (4 * i)))
                    B4 = GetByte(IFO(TTStart + 8 + 3 + (4 * i)))

                    If B1.Length = 1 Then B1 = "0" & B1
                    If B2.Length = 1 Then B2 = "0" & B2
                    If B3.Length = 1 Then B3 = "0" & B3
                    If B4.Length = 1 Then B4 = "0" & B4

                    TT_StartBytes(i) = CInt("&h" & B1 & B2 & B3 & B4)
                Next

                Dim PTT As cPTT
                Dim PTTStart As Integer = 0
                Dim PTTCnt As Short
                For i As Short = 0 To TitleCount - 1
                    TT = New cTitle
                    TT.VTS_TT = i + 1
                    TT.VTS = ID
                    PTTStart = TT_StartBytes(i) + TTStart

                    PTTCnt = 1
                    Dim Upper As Integer
                    If TT_StartBytes.Length - 1 = i Then
                        Upper = TT_EndOfPTTTable + TTStart
                    Else
                        Upper = TTStart + (TT_StartBytes(i + 1) - 1)
                    End If
                    For pi As Integer = PTTStart To Upper Step 4
                        PTT = New cPTT
                        PTT.PTTNumber = PTTCnt
                        PTT.PGCN = EBC.ToUInt16(IFO, pi)
                        PTT.PGN = EBC.ToUInt16(IFO, pi + 2)
                        TT.PTTs.Add(PTT)
                        PTTCnt += 1
                    Next
                    PTTCnt = 0
                    Titles.Add(TT)
                Next

                '---------------------------------------------------------------------------------
                'END Titles
                '---------------------------------------------------------------------------------

                '---------------------------------------------------------------------------------
                'Cell Mapping
                '---------------------------------------------------------------------------------

                Dim tCM As cCellMapItem
                Dim PrevPTT As Short
                Dim PrevPGN As Short
                For Each oP As cPGC In PGCs
                    oP.CellMap = New colCells
                    If oP.CellCount > 0 Then
                        For c As Short = 1 To oP.CellPlaybackInfo.Count
                            tCM = New cCellMapItem
                            tCM.CellNo = c

                            'get pgn from celln
                            For Each PMI As cProgramMap In oP.ProgramMap
                                If PMI.CellNo = c Then
                                    tCM.PGNo = PMI.ProgramNo
                                    PrevPGN = tCM.PGNo
                                End If
                            Next

                            If tCM.PGNo = 0 Then tCM.PGNo = PrevPGN

                            'get pttn from pgn
                            For Each oTT As cTitle In Titles
                                For Each oPTT As cPTT In oTT.PTTs
                                    If oPTT.PGCN = oP.PGCNumber Then
                                        If oPTT.PGN = tCM.PGNo Then
                                            tCM.PTTNoChNo = oPTT.PTTNumber
                                            PrevPTT = tCM.PTTNoChNo
                                            Exit For
                                        End If
                                    End If
                                Next
                            Next

                            If tCM.PTTNoChNo = 0 Then tCM.PTTNoChNo = PrevPTT
                            oP.CellMap.Add(tCM)
                        Next
                    End If
                Next

                '---------------------------------------------------------------------------------
                'END Cell Mapping
                '---------------------------------------------------------------------------------

            Catch ex As Exception
                Throw New Exception("Problem with New cVTS. Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Function GetPGCsByVTS_TTN(ByVal VTS_TTN As Short) As cPGC()
            Try
                Dim out(-1) As cPGC
                For Each p As cPGC In PGCs
                    If p.VTS_TTN = VTS_TTN Then
                        ReDim Preserve out(UBound(out) + 1)
                        out(UBound(out)) = p
                    End If
                Next
                Return out
            Catch ex As Exception
                Debug.WriteLine("Problem with GetPGCsByVTS_TTN. Error: " & ex.Message)
            End Try
        End Function

        Public ReadOnly Property HasCallSSRSM127() As Boolean
            Get
                Try
                    'TITLE SPACE
                    For Each PGC As cPGC In PGCs
                        If PGC.HasCallSSRSM127 Then Return True
                    Next
                    Return False
                Catch ex As Exception
                    Throw New Exception("Problem with HasCallSSRSM127() in cVTS. Error: " & ex.Message, ex)
                End Try
            End Get
        End Property

        Public ReadOnly Property CallSSRSM127_Location() As cVTSLocation()
            Get
                Dim out(-1) As cVTSLocation

                'TITLE SPACE
                For i As Short = 0 To PGCs.Count - 1
                    If PGCs(i).HasCallSSRSM127 Then
                        ReDim Preserve out(UBound(out) + 1)
                        out(UBound(out)) = New cVTSLocation
                        out(UBound(out)).MenuSpace = False
                        out(UBound(out)).PGCN = i
                        out(UBound(out)).VTS_TTN = PGCs(i).VTS_TTN
                    End If
                Next
                Return out

                'MENU SPACE
                'For i As Short = 0 To UBound(VTSM_PGCI_UT.LUs)
                '    If VTSM_PGCI_UT.LUs(i).HasCallSSRSM127 Then
                '        ReDim Preserve out(UBound(out) + 1)
                '        out(UBound(out)).MenuSpace = True
                '        out(UBound(out)).LanguageUnit = i
                '        out(UBound(out)).LanguageUnitString = Me.VTSM_PGCI_UT.LU_SRPs(i).VXXM_LCD
                '        out(UBound(out)).PGCN = VTSM_PGCI_UT.LUs(i).CallSSRSM127_PGCN
                '        Return out
                '    End If
                'Next

            End Get
        End Property

        Public Class cVTSLocation
            Public MenuSpace As Boolean
            Public LanguageUnit As Byte
            Public LanguageUnitString As String
            Public PGCN As Byte
            Public VTS_TTN As Byte
        End Class

    End Class

    'Public Class colVTSs
    '    Inherits CollectionBase

    '    Public Function Add(ByVal newIFO As cVTS) As Integer
    '        Return MyBase.List.Add(newIFO)
    '    End Function

    '    Default Public ReadOnly Property Item(ByVal index As Integer) As cVTS
    '        Get
    '            Return MyBase.List.Item(index)
    '        End Get
    '    End Property

    '    Public Sub Remove(ByVal Item As cVTS)
    '        MyBase.List.Remove(Item)
    '    End Sub

    'End Class

    Public Class cVTS_PGCIT

        Public VTS_PGCI_SRP_Ns As UInt16
        Public VTS_PGCIT_EA As UInt32

        Public SRPs() As cVTS_PGCI_SRP
        Public VTS_PGCs() As cPGC

        'Properties
        Public ReadOnly Property PGCCount() As Byte
            Get
                If Not SRPs Is Nothing Then
                    Return UBound(SRPs) + 1
                End If
            End Get
        End Property

        Public Sub New(ByRef IFO() As Byte, ByVal Offset As ULong)
            'TODO: see spec section 4.2.3, pg VI4-61
            'very similar to spec section 4.1.3.1-1, pg VI4-17
            '4.2.4.1-1, pg VI4-67
            'see code line 
            Try
                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian

                VTS_PGCI_SRP_Ns = EBC.ToUInt16(IFO, Offset)
                VTS_PGCIT_EA = EBC.ToUInt32(IFO, Offset + 4)

                'Get SRPs
                ReDim Me.SRPs(VTS_PGCI_SRP_Ns - 1)
                Dim cnt As Integer = 0

                For i As Integer = 8 To ((8 * VTS_PGCI_SRP_Ns) + 8 - 1) Step 8
                    SRPs(cnt) = New cVTS_PGCI_SRP(IFO, Offset + i)
                    cnt += 1
                Next

                'Get PGCs
                ReDim Me.VTS_PGCs(Me.VTS_PGCI_SRP_Ns - 1)
                cnt = 0
                For Each SRP As cVTS_PGCI_SRP In Me.SRPs
                    Me.VTS_PGCs(cnt) = New cPGC(IFO, Offset + SRP.VTS_PGCI_SA, cnt)
                    cnt += 1
                Next


                '---------------------------------------------------------------------------------
                'Cell Mapping
                '---------------------------------------------------------------------------------

                Dim tCM As cCellMapItem
                Dim PrevPTT As Short
                Dim PrevPGN As Short
                For Each oP As cPGC In Me.VTS_PGCs
                    oP.CellMap = New colCells
                    If oP.CellCount > 0 Then
                        For c As Short = 1 To oP.CellPlaybackInfo.Count
                            tCM = New cCellMapItem
                            tCM.CellNo = c

                            'get pgn from celln
                            For Each PMI As cProgramMap In oP.ProgramMap
                                If PMI.CellNo = c Then
                                    tCM.PGNo = PMI.ProgramNo
                                    PrevPGN = tCM.PGNo
                                End If
                            Next

                            If tCM.PGNo = 0 Then tCM.PGNo = PrevPGN

                            ''get pttn from pgn
                            'For Each oTT As cTitle In Titles
                            '    For Each oPTT As cPTT In oTT.PTTs
                            '        If oPTT.PGCN = oP.PGCNumber Then
                            '            If oPTT.PGN = tCM.PGNo Then
                            '                tCM.PTTNoChNo = oPTT.PTTNumber
                            '                PrevPTT = tCM.PTTNoChNo
                            '                Exit For
                            '            End If
                            '        End If
                            '    Next
                            'Next

                            If tCM.PTTNoChNo = 0 Then tCM.PTTNoChNo = PrevPTT
                            oP.CellMap.Add(tCM)
                        Next
                    End If
                Next

                '---------------------------------------------------------------------------------
                'END Cell Mapping
                '---------------------------------------------------------------------------------

            Catch ex As Exception
                Throw New Exception("Problem with New cVTS_PGCIT. Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Class cVTS_PGCI_SRP

            Public VTS_PGC_CAT As cVTS_PGC_CAT
            Public VTS_PGCI_SA As UInt32

            'CONSTRUCTOR
            Public Sub New(ByRef IFO() As Byte, ByVal Offset As Int64)
                Try
                    VTS_PGC_CAT = New cVTS_PGC_CAT(IFO, Offset)
                    Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                    Me.VTS_PGCI_SA = EBC.ToUInt32(IFO, Offset + 4)
                Catch ex As Exception
                    Throw New Exception("Problem with New() cVTS_PGCI_SRP. Error: " & ex.Message, ex)
                End Try
            End Sub

            Public Class cVTS_PGC_CAT
                Public EntryType As eEntryType
                Public VTS_TTN As Byte
                Public BlockMode As eBlockMode
                Public BlockType As eBlockType
                Public PTL_ID_FLD_U As UShort
                Public PTL_ID_FLD_L As UShort

                Public Sub New(ByRef IFO() As Byte, ByVal Offset As Int64)
                    Try
                        EntryType = (IFO(Offset) >> 7) And 1
                        VTS_TTN = (IFO(Offset)) And 127
                        BlockMode = (IFO(Offset + 1) >> 6) And 2
                        BlockType = (IFO(Offset + 1) >> 4) And 2
                        PTL_ID_FLD_U = IFO(Offset + 2)
                        PTL_ID_FLD_L = IFO(Offset + 3)
                    Catch ex As Exception
                        Throw New Exception("Problem with New() cVTS_PGC_CAT. Error: " & ex.Message, ex)
                    End Try
                End Sub

                Public Enum eEntryType
                    NotEntryPGC
                    EntryPGC
                End Enum

                Public Enum eBlockMode
                    NotAPGCInTheBlock
                    FirstPGCInTheBlock
                    PGCInTheBlockExceptFirstOrLast
                    LastPGCInTheBlock
                End Enum

                Public Enum eBlockType
                    NotAPartOfTheBlock
                    ParentalBlock
                End Enum

            End Class

        End Class

        Public ReadOnly Property HasCallSSRSM127() As Boolean
            Get
                For Each PGC As cPGC In VTS_PGCs
                    If PGC.HasCallSSRSM127 Then Return True
                Next
                Return False
            End Get
        End Property

        Public ReadOnly Property CallSSRSM127_PGCN() As Byte
            Get
                For i As Short = 0 To UBound(VTS_PGCs)
                    If VTS_PGCs(i).HasCallSSRSM127 Then Return i
                Next
            End Get
        End Property

    End Class

#Region "Title"

    Public Class cTitle
        Public VTS As Short
        Public VTS_TT As Short
        Public PTTs As colPTTs

        Public Sub New()
            PTTs = New colPTTs
        End Sub

    End Class

    Public Class colTitles
        Inherits CollectionBase

        Public Function Add(ByVal newTitle As cTitle) As Integer
            Return MyBase.List.Add(newTitle)
        End Function

        Default Public ReadOnly Property Item(ByVal index As Integer) As cTitle
            Get
                Return MyBase.List.Item(index)
            End Get
        End Property

        Public Sub Remove(ByVal Item As cTitle)
            MyBase.List.Remove(Item)
        End Sub
    End Class

    Public Class cPTT
        Public PTTNumber As Short
        Public PGCN As UShort
        Public PGN As UShort
    End Class

    Public Class colPTTs
        Inherits CollectionBase

        Public Function Add(ByVal newPTT As cPTT) As Integer
            Return MyBase.List.Add(newPTT)
        End Function

        Default Public ReadOnly Property Item(ByVal index As Integer) As cPTT
            Get
                Return MyBase.List.Item(index)
            End Get
        End Property

        Public Sub Remove(ByVal Item As cPTT)
            MyBase.List.Remove(Item)
        End Sub
    End Class

#End Region 'Title

#End Region 'VTS

#Region "VMGM"

    Public Class cVMGM

        Public Name As String

        Public NumberOfGlobalTitles As UInt16
        Public GlobalTTs As colGlobalTTs
        Public HasParentalManagement As Boolean

        Public RegionMask As Integer
        Public R1, R2, R3, R4, R5, R6, R7, R8 As Boolean

        Public ReadOnly Property Regions() As Boolean()
            Get
                Dim out(7) As Boolean
                out(0) = R1
                out(1) = R2
                out(2) = R3
                out(3) = R4
                out(4) = R5
                out(5) = R6
                out(6) = R7
                out(7) = R8
                Return out
            End Get
        End Property

        Public LastSectorVMGSet As UInt32
        Public LastSectorIFO As UInt32
        Public VersionNumber As String
        Public VMGCategory As UInt32
        Public NumberOfVolumes As UInt16
        Public VolumeNumber As UInt16
        Public SideID As Byte
        Public NumberOfTitleSets As UInt16
        Public ProviderID As String
        Public VMGPOS() As Byte
        Public EndAddressVMGI_MAT As UInt32
        Public StartAddressFP_PGC As UInt32
        Public StartSectorMenuVOB As UInt32
        Public SectorPointerTT_SRPT As UInt32
        Public SectorPointerVMGM_PGCI_UT As UInt32
        Public SectorPointerVMG_PTL_MAIT As UInt32
        Public SectorPointerVMG_VTS_ATRT As UInt32
        Public SectorPointerVMG_TXTDT_MG As UInt32
        Public SectorPointerVMGM_C_ADT As UInt32
        Public SectorPointerVMGM_VOBU_ADMAP As UInt32
        Public VidAttsVMGM_VOBS As cVideoAttributes
        Public NumberOfAudioStreamsInVMGM_VOBS As UInt16
        Public AudAttsVMGM_VOBS As cMenuAudioAttributes
        Public NumberOfSubpictureStreamsInVMGM_VOBS As UInt16
        Public SubAttsVMGM_VOBS As cSubpictureAttributes
        Public FirstPlayPGC As cPGC

        Public VMGM_PGCI_UT As cVXXM_PGCI_UT
        Public VMG_PTL_MAIT As cVMG_PTL_MAIT
        Public VMG_VTS_ATRT As cVMG_VTS_ATRT
        Public VMG_TXTDT_MG As cVMG_TXTDT_MG

        Public Overloads Overrides Function ToString() As String
            Return Name
        End Function

        Public SectorSize As Short = 2048

        Public Sub New(ByVal nVMGMIFOPath As String)
            Me.New(New FileStream(nVMGMIFOPath, FileMode.Open, FileAccess.Read))
        End Sub

        Public Sub New(ByRef VMGM_Stream As Stream)
            Try
                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian

                Dim VMGM(VMGM_Stream.Length - 1) As Byte
                VMGM_Stream.Read(VMGM, 0, VMGM_Stream.Length - 1)
                VMGM_Stream.Close()
                VMGM_Stream = Nothing

                Name = "VIDEO_TS.IFO"

                LastSectorVMGSet = EBC.ToUInt32(VMGM, 12)
                LastSectorIFO = EBC.ToUInt32(VMGM, 28)
                VersionNumber = ((VMGM(33) >> 4) And 15) & "." & ((VMGM(33) >> 0) And 15)
                VMGCategory = EBC.ToUInt32(VMGM, 34)
                RegionMask = VMGM(35)

                Dim ba As New cSeqBitArray(VMGM(35))
                R1 = ba.BitToBool(7, True)
                R2 = ba.BitToBool(6, True)
                R3 = ba.BitToBool(5, True)
                R4 = ba.BitToBool(4, True)
                R5 = ba.BitToBool(3, True)
                R6 = ba.BitToBool(2, True)
                R7 = ba.BitToBool(1, True)
                R8 = ba.BitToBool(0, True)
                ba = Nothing

                NumberOfVolumes = EBC.ToUInt16(VMGM, 38)
                VolumeNumber = EBC.ToUInt16(VMGM, 40)
                SideID = VMGM(42)
                NumberOfTitleSets = EBC.ToUInt16(VMGM, 62)

                ProviderID = GetStringFromBytes(VMGM, 64, 32)

                ReDim VMGPOS(7)
                Array.Copy(VMGM, 96, VMGPOS, 0, 8)

                EndAddressVMGI_MAT = EBC.ToUInt32(VMGM, 128)
                StartAddressFP_PGC = EBC.ToUInt32(VMGM, 132)
                StartSectorMenuVOB = EBC.ToUInt32(VMGM, 192)
                SectorPointerTT_SRPT = SectorSize * EBC.ToUInt32(VMGM, 196)
                SectorPointerVMGM_PGCI_UT = SectorSize * EBC.ToUInt32(VMGM, 200)
                SectorPointerVMG_PTL_MAIT = SectorSize * EBC.ToUInt32(VMGM, 204)
                SectorPointerVMG_VTS_ATRT = SectorSize * EBC.ToUInt32(VMGM, 208)
                SectorPointerVMG_TXTDT_MG = SectorSize * EBC.ToUInt32(VMGM, 212)
                SectorPointerVMGM_C_ADT = SectorSize * EBC.ToUInt32(VMGM, 216)
                SectorPointerVMGM_VOBU_ADMAP = SectorSize * EBC.ToUInt32(VMGM, 220)

                Me.FirstPlayPGC = New cPGC(VMGM, StartAddressFP_PGC, 1)

                VidAttsVMGM_VOBS = New cVideoAttributes(VMGM, 256)

                NumberOfAudioStreamsInVMGM_VOBS = EBC.ToUInt16(VMGM, 258)

                AudAttsVMGM_VOBS = New cMenuAudioAttributes(VMGM, 260)

                NumberOfSubpictureStreamsInVMGM_VOBS = EBC.ToUInt16(VMGM, 340)

                SubAttsVMGM_VOBS = New cSubpictureAttributes(VMGM, 342, NumberOfSubpictureStreamsInVMGM_VOBS)

                'Parental Management
                If SectorPointerVMG_PTL_MAIT > 0 Then
                    Me.VMG_PTL_MAIT = New cVMG_PTL_MAIT(VMGM, SectorPointerVMG_PTL_MAIT)
                    HasParentalManagement = True
                Else
                    HasParentalManagement = False
                End If

                'Global titles
                NumberOfGlobalTitles = EBC.ToUInt16(VMGM, SectorPointerTT_SRPT)
                GlobalTTs = New colGlobalTTs(VMGM, SectorPointerTT_SRPT + 8, Me, NumberOfGlobalTitles)

                'OTHER STRUCTURES
                VMGM_PGCI_UT = New cVXXM_PGCI_UT(VMGM, SectorPointerVMGM_PGCI_UT)

                'Me.VMG_TXTDT_MG = New cVMG_TXTDT_MG(VMGM, SectorPointerVMG_TXTDT_MG)

            Catch ex As Exception
                Throw New Exception("Problem with New() cVMGM. Error: " & ex.Message, ex)
            End Try
        End Sub

        Public ReadOnly Property GlobalTitleParentalManagement() As cGlobalTitleParentalManagement
            Get
                Return New cGlobalTitleParentalManagement(GlobalTTs)
            End Get
        End Property

        Public Function LookupGTTforVTS_TTN(ByVal VTSN As Integer, ByVal VTS_TTN As Integer) As Integer
            Try
                For Each GTT As cGlobalTT In GlobalTTs
                    If GTT.VTSN = VTSN And GTT.VTS_TTN = VTS_TTN Then Return GTT.GlobalTT_N
                Next
            Catch ex As Exception
                Throw New Exception("Problem in LookupGTTforVTS_TTN(). Error: " & ex.Message, ex)
            End Try
        End Function

    End Class

#Region "GLOBAL TITLES"

    Public Class colGlobalTTs
        Inherits CollectionBase

        Public Function Add(ByVal newGlobalTT As cGlobalTT) As Integer
            Return MyBase.List.Add(newGlobalTT)
        End Function

        Default Public ReadOnly Property Item(ByVal index As Integer) As cGlobalTT
            Get
                Return MyBase.List.Item(index)
            End Get
        End Property

        Public Sub Remove(ByVal Item As cGlobalTT)
            MyBase.List.Remove(Item)
        End Sub

        Public Sub New(ByRef Bytes() As Byte, ByVal Offset As Integer, ByRef VMGM As cVMGM, ByVal GTT_Count As Integer)
            Try
                Dim GTT As cGlobalTT
                Dim GTTn As Short = 1
                For i As Integer = 0 To ((12 * GTT_Count) - 1) Step 12
                    GTT = New cGlobalTT(Bytes, Offset + i)
                    GTT.GlobalTT_N = GTTn
                    GTT.ParentalManagementValue = New cParentalManagement_US(GTT.ParentalManagementMask, GTTn, GTT.VTSN, VMGM)
                    GTTn += 1
                    Add(GTT)
                Next
            Catch ex As Exception
                Throw New Exception("Problem with New() colGlobalTTs. Error: " & ex.Message, ex)
            End Try
        End Sub

    End Class

    Public Class cGlobalTT

        Public Type As cTT_SRP
        Public NumberOfAngles As Byte
        Public NumberOfChapters As UInt16
        Public ParentalManagementMask As UInt16
        Public ParentalManagementValue As cParentalManagement_US
        Public VTSN As Byte
        Public VTS_TTN As Byte
        Public DiscOffsetToStartOfVTS As UInt32
        Public GlobalTT_N As Short

        Public RunningTime As cTimecode

        Public Sub New(ByRef Bytes() As Byte, ByVal Offset As Integer)
            Try
                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian

                Type = New cTT_SRP(Bytes(Offset))
                NumberOfAngles = Bytes(Offset + 1)
                NumberOfChapters = EBC.ToUInt16(Bytes, Offset + 2)
                ParentalManagementMask = EBC.ToUInt16(Bytes, Offset + 4)
                VTSN = Bytes(Offset + 6)
                VTS_TTN = Bytes(Offset + 7)
                DiscOffsetToStartOfVTS = EBC.ToUInt32(Bytes, Offset + 8)
            Catch ex As Exception
                Throw New Exception("Problem with New() cGlobalTT. Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Overloads Overrides Function ToString() As String
            Return "GTTN: " & GlobalTT_N & " Number of chapters: " & NumberOfChapters & " VTS_N: " & VTSN & " VTS_TTN: " & VTS_TTN
        End Function

        ''' <summary>
        ''' VI4-13, 4.1.2-2
        ''' </summary>
        ''' <remarks></remarks>
        Public Class cTT_SRP

            Public Sequential As String
            Public CellCommand As Boolean
            Public PrePostCommand As Boolean
            Public ButtonCommand As Boolean
            Public UOPPTTPlayOrSearch As Boolean
            Public UOPTimePlayOrSearch As Boolean

            Public Sub New(ByVal b As Byte)
                Try
                    If ((b >> 6) And 1) Then
                        Sequential = "Random_or_Multi_PGC"
                    Else
                        Sequential = "One_Sequential_PGC"
                    End If

                    CellCommand = ((b >> 5) And 1)
                    PrePostCommand = ((b >> 4) And 1)
                    ButtonCommand = ((b >> 3) And 1)
                    UOPPTTPlayOrSearch = ((b >> 1) And 1)
                    UOPTimePlayOrSearch = ((b >> 0) And 1)
                Catch ex As Exception
                    Throw New Exception("Problem with New() cGTTType. Error: " & ex.Message, ex)
                End Try
            End Sub

        End Class

    End Class

    Public Class cGlobalTitleParentalManagement

        Public Titles() As cParentalManagement_US

        Public Sub New(ByRef GTTs As colGlobalTTs)
            ReDim Titles(GTTs.Count - 1)
            For i As Short = 0 To GTTs.Count - 1
                Titles(i) = GTTs(i).ParentalManagementValue
            Next
        End Sub

    End Class

    Public Class cParentalManagement_US

        Public NC17 As Boolean
        Public R As Boolean
        Public PG13 As Boolean
        Public PG As Boolean
        Public G As Boolean
        Public GTTN As UShort

        Public bits_NC17 As UShort
        Public bits_R As UShort
        Public bits_PG13 As UShort
        Public bits_PG As UShort
        Public bits_G As UShort

        Public Sub New(ByVal Bitmask As UInteger, ByVal nGTTN As UShort, ByVal VTSNumber As UShort, ByVal VMGM As cVMGM)
            Try
                If Bitmask = 0 Then
                    NC17 = True
                    R = True
                    PG13 = True
                    PG = True
                    G = True
                Else
                    If VTSNumber > 0 Then
                        Me.bits_NC17 = VMGM.VMG_PTL_MAIT.PTL_MAIs(0).PTL_LVLIs(6).VTS_PTL_IDs(VTSNumber - 1)
                        Me.bits_R = VMGM.VMG_PTL_MAIT.PTL_MAIs(0).PTL_LVLIs(5).VTS_PTL_IDs(VTSNumber - 1)
                        Me.bits_PG13 = VMGM.VMG_PTL_MAIT.PTL_MAIs(0).PTL_LVLIs(3).VTS_PTL_IDs(VTSNumber - 1)
                        Me.bits_PG = VMGM.VMG_PTL_MAIT.PTL_MAIs(0).PTL_LVLIs(2).VTS_PTL_IDs(VTSNumber - 1)
                        Me.bits_G = VMGM.VMG_PTL_MAIT.PTL_MAIs(0).PTL_LVLIs(0).VTS_PTL_IDs(VTSNumber - 1)
                    Else
                        Me.bits_NC17 = VMGM.VMG_PTL_MAIT.PTL_MAIs(0).PTL_LVLIs(6).VMG_PTL_ID
                        Me.bits_R = VMGM.VMG_PTL_MAIT.PTL_MAIs(0).PTL_LVLIs(5).VMG_PTL_ID
                        Me.bits_PG13 = VMGM.VMG_PTL_MAIT.PTL_MAIs(0).PTL_LVLIs(3).VMG_PTL_ID
                        Me.bits_PG = VMGM.VMG_PTL_MAIT.PTL_MAIs(0).PTL_LVLIs(2).VMG_PTL_ID
                        Me.bits_G = VMGM.VMG_PTL_MAIT.PTL_MAIs(0).PTL_LVLIs(0).VMG_PTL_ID
                    End If

                    NC17 = Bitmask And Me.bits_NC17
                    R = Bitmask And Me.bits_R
                    PG13 = Bitmask And Me.bits_PG13
                    PG = Bitmask And Me.bits_PG
                    G = Bitmask And Me.bits_G
                End If
                GTTN = nGTTN
            Catch ex As Exception
                Throw New Exception("Problem with New() cParentalManagement_US. Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Overrides Function ToString() As String
            Dim SB As New StringBuilder
            SB.Append("TT " & GTTN & ": ")
            If G Then
                SB.Append("G")
                Return SB.ToString
            End If
            If PG Then
                SB.Append("PG")
                Return SB.ToString
            End If
            If PG13 Then
                SB.Append("PG13")
                Return SB.ToString
            End If
            If R Then
                SB.Append("R")
                Return SB.ToString
            End If
            If NC17 Then
                SB.Append("NC17")
                Return SB.ToString
            End If
        End Function

    End Class

#End Region 'GLOBAL TITLES

#Region "VMG_PTL_MAIT"

    ''' <summary>
    ''' Ok, for US parental management:
    ''' 
    ''' Going to assume that this is the standard table (expected as a constant when country is US):
    ''' 
    ''' 0x4000 = RESERVED
    ''' 0x4000 = NC-17
    ''' 0x2000 = R
    ''' 0x1000 = RESERVED
    ''' 0x0800 = PG-13
    ''' 0x0400 = PG
    ''' 0x0200 = RESERVED
    ''' 0x0100 = G
    ''' 
    ''' So, when one wants to determine the parental level indicated by a parental id field:
    ''' 
    ''' 1) Confirm that the country is US (if it isn't then I don't know what to do)
    ''' 
    ''' 2) Analyize the upper byte only (because the lower doesn't ever seem to have values for US)
    ''' 
    ''' 3) Do a bitmask based on the above values
    ''' 
    ''' </summary>
    ''' <remarks></remarks>

    Public Class cVMG_PTL_MAIT 'Parental Management

        'VMG_PTL_MAITI
        Public CountryCount As UShort
        Public VTSCount As UShort
        Public SRPByteLength As UShort

        Public PTL_MAI_SRPs() As cPTL_MAI_SRP
        Public PTL_MAIs() As cPTL_MAI

        Public Sub New(ByRef VMGM() As Byte, ByVal Offset As ULong)
            Try
                CountryCount = (VMGM(Offset) << 8) Or (VMGM(Offset + 1))
                VTSCount = (VMGM(Offset + 2) << 8) Or (VMGM(Offset + 3))

                ReDim PTL_MAI_SRPs(CountryCount - 1)
                For i As Short = 0 To CountryCount - 1
                    PTL_MAI_SRPs(i) = New cPTL_MAI_SRP(VMGM, Offset + 8 + (i * 8))
                Next

                SRPByteLength = CountryCount * 8

                ReDim PTL_MAIs(UBound(PTL_MAI_SRPs))
                For i As Short = 0 To UBound(PTL_MAI_SRPs)
                    PTL_MAIs(i) = New cPTL_MAI(VMGM, Offset + 8 + SRPByteLength + (i * (7 + ((VTSCount + 1) * 2) * 8)), VTSCount)
                Next
            Catch ex As Exception
                Throw New Exception("Problem with New() cVMG_PTL_MAIT. Error: " & ex.Message, ex)
            End Try
        End Sub

    End Class

    Public Class cPTL_MAI_SRP

        Public Country As String
        Public StartAddress_PTL_MAI As UShort

        Sub New(ByRef VMGM() As Byte, ByVal Offset As ULong)
            Country = System.Text.ASCIIEncoding.ASCII.GetChars(VMGM, Offset, 2)
            ByteSwap(VMGM, 2, Offset + 4)
            StartAddress_PTL_MAI = BitConverter.ToUInt16(VMGM, Offset + 4)
        End Sub

    End Class

    Public Class cPTL_MAI

        Public PTL_LVLIs() As cPTL_LVLI

        Public Sub New(ByRef VMGM() As Byte, ByVal Offset As ULong, ByVal VTSCount As UShort)
            Try
                ReDim PTL_LVLIs(7)
                Dim delta As UShort = 2 + (VTSCount * 2)
                PTL_LVLIs(7) = New cPTL_LVLI(VMGM, Offset, VTSCount)
                PTL_LVLIs(6) = New cPTL_LVLI(VMGM, Offset + delta, VTSCount)
                PTL_LVLIs(5) = New cPTL_LVLI(VMGM, Offset + (delta * 2), VTSCount)
                PTL_LVLIs(4) = New cPTL_LVLI(VMGM, Offset + (delta * 3), VTSCount)
                PTL_LVLIs(3) = New cPTL_LVLI(VMGM, Offset + (delta * 4), VTSCount)
                PTL_LVLIs(2) = New cPTL_LVLI(VMGM, Offset + (delta * 5), VTSCount)
                PTL_LVLIs(1) = New cPTL_LVLI(VMGM, Offset + (delta * 6), VTSCount)
                PTL_LVLIs(0) = New cPTL_LVLI(VMGM, Offset + (delta * 7), VTSCount)
            Catch ex As Exception
                Throw New Exception("Problem with New() cPTL_MAI. Error: " & ex.Message, ex)
            End Try
        End Sub

    End Class

    Public Class cPTL_LVLI

        Public VMG_PTL_ID As UInt16
        Public VTS_PTL_IDs() As UInt16

        Public Sub New(ByRef VMGM() As Byte, ByVal Offset As ULong, ByVal VTSCount As UShort)
            ByteSwap(VMGM, 2, Offset)
            VMG_PTL_ID = BitConverter.ToUInt16(VMGM, Offset)
            ReDim VTS_PTL_IDs(VTSCount - 1)
            For i As Short = 0 To VTSCount - 1
                ByteSwap(VMGM, 2, Offset + 2 + (i * 2))
                VTS_PTL_IDs(i) = BitConverter.ToUInt16(VMGM, Offset + 2 + (i * 2))
            Next
        End Sub

    End Class

#End Region 'VMG_PTL_MAIT

    Public Class cVMG_VTS_ATRT 'copies of VTS audio/sub-picture attributes

        'TODO:
        Public Sub New(ByVal Bytes() As Byte)

        End Sub

    End Class

    Public Class cVMG_TXTDT_MG 'text data
        Public TextID As String
        Public LanguageCount As UInt32
        Public EndByte As UInt32
        Public Languages() As cVMG_TEXTDATA

        Public Sub New(ByVal VMGM() As Byte, ByVal Offset As ULong)
            Try
                TextID = System.Text.Encoding.ASCII.GetString(VMGM, Offset, 12)
                ByteSwap(VMGM, 4, Offset + 12)
                LanguageCount = BitConverter.ToUInt32(VMGM, Offset + 12)
                ByteSwap(VMGM, 4, Offset + 16)
                EndByte = BitConverter.ToUInt32(VMGM, Offset + 16)
                Dim pos As ULong = Offset + 20
                ReDim Languages(LanguageCount - 1)
                For i As UShort = 0 To LanguageCount - 1
                    Languages(i) = New cVMG_TEXTDATA(VMGM, pos)
                Next
            Catch ex As Exception
                Throw New Exception("Problem with New() cVMG_VTS_MG. Error: " & ex.Message, ex)
            End Try
        End Sub

    End Class

    Public Class cVMG_TEXTDATA

        Public LangCode As String
        Public Unknown_1 As UInt16 'textdata count?
        Public SA_TxtTbl As UInt32
        Public TextDatas() As cTxtDt

        Public Sub New(ByRef VMGM() As Byte, ByRef Offset As ULong)
            'need to increment Offset - see if it works properly
            Try
                LangCode = System.Text.Encoding.ASCII.GetString(VMGM, Offset, 2)

            Catch ex As Exception
                Throw New Exception("Problem with New() cVMG_TEXTDATA. Error: " & ex.Message, ex)
            End Try
        End Sub

    End Class

    Public Class cTxtDt
        Public Length As UInt32

        Public Sub New(ByRef VMGM() As Byte, ByRef Offset As ULong)

        End Sub

    End Class

#End Region 'VMGM

#Region "VXXM_PGCI_UT"

    ''' <summary>
    ''' For VMGM_PGCI_UT: VI4-15, 4.1.3
    ''' For VTSM_PGCI_UT: 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class cVXXM_PGCI_UT 'Menu data

        'VXXM_PGCI_UTI
        Public LangUnitCount As Short
        Public VXXM_PGCI_UT_EA As Long

        'Member Arrays
        Public LU_SRPs() As cVXXM_LU_SRP
        Public LUs() As cVXXM_LU

        'SMT Members
        Public IFOPath As String
        Private PGCI_UT_SA As Integer

        'Constructor
        Public Sub New(ByRef IFO() As Byte, ByVal Offset As Integer)
            Try
                If Offset = 0 Then
                    LangUnitCount = 0
                    Exit Sub
                Else
                    PGCI_UT_SA = Offset
                End If

                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian

                'Dim BS As New cSMTBitshifter
                'Me.LangUnitCount = (IFO(Offset) << 8) Or IFO(Offset + 1)
                'Me.LangUnitCount = BS.SetVal(2, IFO(Offset), IFO(Offset + 1))
                Me.LangUnitCount = EBC.ToUInt16(IFO, Offset)
                If LangUnitCount = 0 Then Exit Sub

                'UT_EndAddress = (IFO(Offset + 4) << 24) Or (IFO(Offset + 5) << 16) Or (IFO(Offset + 6) << 8) Or IFO(Offset + 7)
                'VXXM_PGCI_UT_EA = BS.SetVal(4, IFO(Offset + 4), IFO(Offset + 5), IFO(Offset + 6), IFO(Offset + 7))
                VXXM_PGCI_UT_EA = EBC.ToUInt32(IFO, Offset + 4)

                'Get LU_SRPs
                ReDim LU_SRPs(Me.LangUnitCount - 1)
                Offset += 8 'to skip over VXXM_PGCI_UTI
                Dim cnt As Short = 0
                For i As Integer = 0 To 8 * LangUnitCount - 1 Step 8
                    LU_SRPs(cnt) = New cVXXM_LU_SRP(IFO, Offset + i, IFOPath)
                    cnt += 1
                Next

                'Get LUs
                ReDim Me.LUs(Me.LangUnitCount - 1)
                cnt = 0
                For i As Integer = 0 To 8 * LangUnitCount - 1 Step 8
                    LUs(cnt) = New cVXXM_LU(IFO, PGCI_UT_SA + LU_SRPs(cnt).VXXM_LU_SA)
                    cnt += 1
                Next

            Catch ex As Exception
                Throw New Exception("Problem with New cVXXM_PGCI_UT. Error: " & ex.Message, ex)
            End Try
        End Sub

        'Utility
        Public Function FindLUByLang(ByVal Lang As String) As cVXXM_LU
            Try
                If Lang.Length > 2 Or Lang.Length < 2 Then
                    Throw New Exception("Problem with FindLUByLang. Param must be two characters.")
                    Return Nothing
                End If
                If Me.LU_SRPs Is Nothing Then
                    Debug.WriteLine("LU_SRPs is nothing in FindLUByLang.")
                    Return Nothing
                End If
                If Me.LU_SRPs.Length = 1 Then
                    Return Me.LUs(0)
                End If
                For s As Short = 0 To Me.LU_SRPs.Length - 1
                    If LU_SRPs(s).VXXM_LCD.ToLower = Lang.ToLower Then
                        Return Me.LUs(s)
                    End If
                Next
                Return Nothing
            Catch ex As Exception
                Throw New Exception("Problem with FindLUByLang. Error: " & ex.Message)
                Return Nothing
            End Try
        End Function

    End Class

    Public Class cVXXM_LU_SRP

        Public VXXM_LCD As String
        Public VXXM_EXST As Byte
        Public VXXM_LU_SA As Integer

        Public Sub New(ByRef IFO() As Byte, ByVal Offset As Int64, Optional ByVal IFOPath As String = Nothing)

            Try
                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian

                VXXM_LCD = System.Text.Encoding.ASCII.GetString(IFO, Offset, 2)
                If VXXM_LCD = "??" Then VXXM_LCD = "Not Specified"
                VXXM_EXST = IFO(Offset + 3)
                VXXM_LU_SA = EBC.ToUInt32(IFO, Offset + 4)

                'If VXXM_LCD.ToLower = "iw" Then
                '    If Throw New Exception("Hebrew ""iw"" found, would you like to update to ""he?""", Throw New ExceptionStyle.YesNo, "Hebrew iw") = Throw New ExceptionResult.Yes Then
                '        '&h6865
                '        'Dim FS As New FileStream(IFOPath, FileMode.Open)
                '        'FS.Position = Offset
                '        'FS.WriteByte(&H68)
                '        'FS.WriteByte(&H65)
                '        'FS.Close()
                '    End If
                'ElseIf VXXM_LCD.ToLower = "he" Then
                '    Throw New Exception("he found")
                'End If

            Catch ex As Exception
                Throw New Exception("Problem with New() cVXXM_LU_SRP. Error: " & ex.Message, ex)
            End Try
        End Sub

    End Class

    Public Class cVXXM_LU

        'VXXM_LUI
        Public VXXM_PGCI_SRP_Ns As UInt16
        Public VXXM_LU_EA As UInt32

        'Member Arrays
        Public SRPs() As cVXXM_PGCI_SRP
        Public VXXM_PGCs() As cPGC

        'Properties
        Public ReadOnly Property PGCCount() As Byte
            Get
                If Not SRPs Is Nothing Then
                    Return UBound(SRPs) + 1
                End If
            End Get
        End Property

        'Constructor
        Public Sub New(ByRef IFO() As Byte, ByVal Offset As Int64)
            Try
                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian

                VXXM_PGCI_SRP_Ns = EBC.ToUInt16(IFO, Offset)
                VXXM_LU_EA = EBC.ToUInt32(IFO, Offset + 4)

                'Get VXXM_PGCI_SRP
                ReDim Me.SRPs(VXXM_PGCI_SRP_Ns - 1)
                Dim cnt As Integer = 0

                For i As Integer = 8 To ((8 * VXXM_PGCI_SRP_Ns) + 8 - 1) Step 8
                    SRPs(cnt) = New cVXXM_PGCI_SRP(IFO, Offset + i)
                    cnt += 1
                Next

                'GET VXXM_PGCIs
                ReDim Me.VXXM_PGCs(Me.VXXM_PGCI_SRP_Ns - 1)
                cnt = 0
                For Each SRP As cVXXM_PGCI_SRP In Me.SRPs
                    Me.VXXM_PGCs(cnt) = New cPGC(IFO, Offset + SRP.VXXM_PGC_SA, cnt)
                    cnt += 1
                Next



                '---------------------------------------------------------------------------------
                'Cell Mapping
                '---------------------------------------------------------------------------------

                Dim tCM As cCellMapItem
                Dim PrevPTT As Short
                Dim PrevPGN As Short
                For Each oP As cPGC In VXXM_PGCs
                    oP.CellMap = New colCells
                    If oP.CellCount > 0 Then
                        For c As Short = 1 To oP.CellPlaybackInfo.Count
                            tCM = New cCellMapItem
                            tCM.CellNo = c

                            'get pgn from celln
                            For Each PMI As cProgramMap In oP.ProgramMap
                                If PMI.CellNo = c Then
                                    tCM.PGNo = PMI.ProgramNo
                                    PrevPGN = tCM.PGNo
                                End If
                            Next

                            If tCM.PGNo = 0 Then tCM.PGNo = PrevPGN

                            ''get pttn from pgn
                            'For Each oTT As cTitle In Titles
                            '    For Each oPTT As cPTT In oTT.PTTs
                            '        If oPTT.PGCN = oP.PGCNumber Then
                            '            If oPTT.PGN = tCM.PGNo Then
                            '                tCM.PTTNoChNo = oPTT.PTTNumber
                            '                PrevPTT = tCM.PTTNoChNo
                            '                Exit For
                            '            End If
                            '        End If
                            '    Next
                            'Next

                            If tCM.PTTNoChNo = 0 Then tCM.PTTNoChNo = PrevPTT
                            oP.CellMap.Add(tCM)
                        Next
                    End If
                Next

                '---------------------------------------------------------------------------------
                'END Cell Mapping
                '---------------------------------------------------------------------------------

            Catch ex As Exception
                Throw New Exception("Problem with New cVXXM_LU. Error: " & ex.Message, ex)
            End Try
        End Sub

        Public ReadOnly Property HasCallSSRSM127() As Boolean
            Get
                For Each PGC As cPGC In VXXM_PGCs
                    If PGC.HasCallSSRSM127 Then Return True
                Next
                Return False
            End Get
        End Property

        Public ReadOnly Property CallSSRSM127_PGCN() As Byte
            Get
                For i As Short = 0 To UBound(VXXM_PGCs)
                    If VXXM_PGCs(i).HasCallSSRSM127 Then Return i
                Next
            End Get
        End Property

    End Class

    Public Class cVXXM_PGCI_SRP

        'MEMBERS
        Public VXXM_PGC_CAT As cVXXM_PGC_CAT
        Public VXXM_PGC_SA As UInt32

        'CONSTRUCTOR
        Public Sub New(ByRef IFO() As Byte, ByVal Offset As Int64)
            Try
                VXXM_PGC_CAT = New cVXXM_PGC_CAT
                VXXM_PGC_CAT.EntryPGC = (IFO(Offset) >> 7) And 1
                VXXM_PGC_CAT.MenuID = IFO(Offset) And 4
                VXXM_PGC_CAT.BlockMode = (IFO(Offset + 1) >> 6) And 2
                VXXM_PGC_CAT.BlockType = (IFO(Offset + 1) >> 4) And 2
                VXXM_PGC_CAT.ParentalManagement1 = IFO(Offset + 2)
                VXXM_PGC_CAT.ParentalManagement2 = IFO(Offset + 3)
                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
                Me.VXXM_PGC_SA = EBC.ToUInt32(IFO, Offset + 4)
            Catch ex As Exception
                Throw New Exception("Problem with New() cVXXM_PGCI_SRP. Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Structure cVXXM_PGC_CAT
            Public EntryPGC As Boolean
            Public MenuID As Byte
            Public BlockMode As Byte
            Public BlockType As Byte
            Public ParentalManagement1 As Byte
            Public ParentalManagement2 As Byte
        End Structure

    End Class

#End Region 'VXXM_PGCI_UT

#Region "Cell Mapping"

    Public Class cCellMapItem
        Public PTTNoChNo As Short
        Public PGNo As Short
        Public CellNo As Short
    End Class

    Public Class colCells
        Inherits CollectionBase

        Public Function Add(ByVal newCellMapItem As cCellMapItem) As Integer
            Return MyBase.List.Add(newCellMapItem)
        End Function

        Default Public ReadOnly Property Item(ByVal index As Integer) As cCellMapItem
            Get
                Return MyBase.List.Item(index)
            End Get
        End Property

        Public Sub Remove(ByVal Item As cCellMapItem)
            MyBase.List.Remove(Item)
        End Sub

    End Class

#End Region 'Chapter Mapping

#Region "PGC"

    ''' <summary>
    ''' PGCI - VI4-83, 4.3.1
    ''' </summary>
    ''' <remarks></remarks>
    Public Class cPGC

        Public PGCNumber As Short
        Public Offset As Long

        ' PGC_GI
        '   PGC_CNT
        Public ProgramCount As Byte
        Public CellCount As Byte
        '   PGC_PB_TM
        Public PlaybackTime As String
        Public PlaybackTime_TC As cTimecode
        '   PGC_UOP_CTL
        Public ProhibitedUOPs As String
        Public ProhibitedUOPs_UI As UInt32

        Public PGC_AST_CTL As cPGC_AST_CTLT 'Audio Stream Control
        Public PGC_SPST_CTL As cPGC_SPST_CTLT 'Subpicture Stream Control

        '   PGC_NAV_CTL
        Public NextPGCN As UInt16
        Public PreviousPGCN As UInt16
        Public GoUpPGCN As UInt16
        Public PlaybackMode As String
        Public PlaybackMode_b As Byte
        Public PGCStillTime As Byte

        '   PGC_SP_PLT
        Public CLUT As cCLUT 'Color LookUp Table 

        '   PGC_CMDT
        Public offCommands As UInt16
        Public PreCommandCount As Short
        Public PreCommands() As cCMD
        Public PostCommandCount As Short
        Public PostCommands() As cCMD
        Public CellCommandCount As Short
        Public CellCommands() As cCMD

        '   PGC_PGMAP
        Public offProgramMap As UInt16
        Public ProgramMap As colProgramMap

        '   C_PBIT
        Public offCellPlaybackInfo As UInt16
        Public CellPlaybackInfo As colCellPlaybackInfo

        '   C_POSIT
        Public offCellPositionInfo As UInt16
        Public CellPositionInfo As cCellPositionInfo
        Public CellMap As colCells

        'SMT Values
        Public ParentVTS As Short
        Public VTS_TTN As String

        Public ReadOnly Property HasCallSSRSM127() As Boolean
            Get
                Try
                    If Me.PostCommands Is Nothing Then Return False
                    For Each PC As cCMD In PostCommands
                        If PC.IsCallSSRSM127 Then Return True
                    Next
                    Return False
                Catch ex As Exception
                    Throw New Exception("Problem with HasCallSSRSM127() in cPGC. Error: " & ex.Message, ex)
                End Try
            End Get
        End Property

        Public Sub New(ByRef Bytes() As Byte, ByVal nOffset As Int64, ByVal nPGCNumber As Short, Optional ByVal pVTS As Short = -1)
            Try
                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian

                PGCNumber = nPGCNumber

                ParentVTS = pVTS
                Offset = nOffset

                'PGC_CNT
                ProgramCount = Bytes(Offset + 2)
                CellCount = Bytes(Offset + 3)

                'PGC_PB_TM
                 PlaybackTime_TC = New cTimecode(Bytes, Offset + 4)
                PlaybackTime = PlaybackTime_TC.ToString

                'PGC_UOP_CTL
                ProhibitedUOPs = Hex(Bytes(Offset + 8)) & Hex(Bytes(Offset + 9)) & Hex(Bytes(Offset + 10)) & Hex(Bytes(Offset + 11))
                ProhibitedUOPs_UI = EBC.ToUInt32(Bytes, Offset + 8)

                'Aud & SP
                PGC_AST_CTL = New cPGC_AST_CTLT(Bytes, Offset + 12)
                PGC_SPST_CTL = New cPGC_SPST_CTLT

                'PGC_NAV_CTL
                NextPGCN = EBC.ToUInt16(Bytes, Offset + 156)
                PreviousPGCN = EBC.ToUInt16(Bytes, Offset + 158)
                GoUpPGCN = EBC.ToUInt16(Bytes, Offset + 160)
                PlaybackMode_b = Bytes(Offset + 162)
                If PlaybackMode_b = 0 Then
                    PlaybackMode = "Sequential"
                ElseIf PlaybackMode_b > 0 And PlaybackMode_b < 127 Then
                    PlaybackMode = "Random"
                ElseIf PlaybackMode_b = 128 Then
                    PlaybackMode = "Reserved"
                ElseIf PlaybackMode_b > 128 Then
                    PlaybackMode = "Shuffle"
                End If
                PGCStillTime = Bytes(Offset + 163)

                'PGC_SP_PLT
                CLUT = New cCLUT

                'PGC_CMDT
                offCommands = EBC.ToUInt16(Bytes, Offset + 228)

                If offCommands > 0 Then 'we have a command table

                    '   PGC_CMDTI
                    PreCommandCount = EBC.ToUInt16(Bytes, Offset + offCommands)
                    PostCommandCount = EBC.ToUInt16(Bytes, Offset + offCommands + 2)
                    CellCommandCount = EBC.ToUInt16(Bytes, Offset + offCommands + 4)

                    '   PRE_CMD()
                    If PreCommandCount > 0 Then
                        Dim PreCommandTableStartByte As Integer = Offset + (offCommands + 8)
                        ReDim PreCommands(PreCommandCount - 1)
                        For i As Byte = 0 To UBound(PreCommands)
                            PreCommands(i) = New cCMD(Bytes, PreCommandTableStartByte + (i * 8))
                        Next
                    End If

                    '   POST_CMD()
                    If PostCommandCount > 0 Then
                        Dim PostCommandTableStartByte As Integer = Offset + (offCommands + 8) + (PreCommandCount * 8)
                        ReDim PostCommands(PostCommandCount - 1)
                        For i As Byte = 0 To UBound(PostCommands)
                            PostCommands(i) = New cCMD(Bytes, PostCommandTableStartByte + (i * 8))
                        Next
                    End If

                    '   C_CMD()
                    If CellCommandCount > 0 Then

                    End If

                End If

                'PGC_PGMAP
                offProgramMap = EBC.ToUInt16(Bytes, Offset + 230)
                ProgramMap = New colProgramMap(Bytes, Offset + offProgramMap, ProgramCount)

                'C_PBI
                If CellCount > 0 Then
                    offCellPlaybackInfo = EBC.ToUInt16(Bytes, Offset + 232)
                    CellPlaybackInfo = GetCellPlaybackInfo(Bytes, Offset + offCellPlaybackInfo, pVTS, PGCNumber, CellCount)
                End If

                'C_POSIT
                offCellPositionInfo = EBC.ToUInt16(Bytes, Offset + 234)
                CellPositionInfo = Nothing

            Catch ex As Exception
                Throw New Exception("Problem with New PGC. Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Shared Function GetCellPlaybackInfo(ByRef Bytes() As Byte, ByVal Offset As ULong, ByVal VTSNum As Short, ByVal PGCNum As Short, ByVal CellCount As Short) As colCellPlaybackInfo
            Try
                Dim oCPICol As New colCellPlaybackInfo
                Dim CellNum As Short = 1
                For i As Integer = 0 To (CellCount * 24) - 1 Step 24
                    oCPICol.Add(New cCellPlaybackInfo(Bytes, Offset + i, CellNum, VTSNum, PGCNum))
                    CellNum += 1
                Next
                Return oCPICol
            Catch ex As Exception
                Throw New Exception("Problem with GetCellPlaybackInfo(). Error: " & ex.Message, ex)
            End Try
        End Function

    End Class

    ''' <summary>
    ''' VI4-176, 4.6.2
    ''' </summary>
    ''' <remarks></remarks>
    Public Class cCMD

#Region "PUBLIC FIELDS & PROPERTIES"

        Public CommandName As String = ""
        Public IsCallSSRSM127 As Boolean = False
        Public ReadOnly Property CompareEvaluates() As Boolean
            Get
                Return _CompareEvaluates
            End Get
        End Property
        Private _CompareEvaluates As Boolean = False
        Public ReadOnly Property HasCompare() As Boolean
            Get
                Return CompareField <> 0
            End Get
        End Property

#End Region 'PUBLIC FIELDS & PROPERTIES

#Region "PRIVATE FIELDS & PROPERTIES"

        Private CommandType As Byte = 0
        Private Command_Bitmask As UInt64

        'OPERATION CODE
        Private OperationCode As UInt16
        Private ReadOnly Property CmdId1() As Byte
            Get
                If Command_Bitmask = Nothing Then Return 0
                Return (Command_Bitmask >> 61) And 7
            End Get
        End Property
        Private CmdID2 As Byte
        Private IFlagForCompare As Boolean
        Private IFlagForSetSetSystem As Boolean
        Private CompareField As Byte
        Private BranchField As Byte
        Private SetSetSystemField As Byte
        Private SetField As Byte

        Private CompareOption As eCompareValues
        Private GoToOption As eGoToValues
        Private LinkOption As eLinkValues
        Private JumpOption As eJumpValues

        'OPERAND SET
        Private OperandSet As UInt64
        Private GoToOperand As UShort
        Private JumpOperand As UInteger
        Private LinkOperand As UShort
        Private SetSystemOperand As UInteger

        Private CP1 As Byte
        Private C2, CP2 As cCompare2

        Private SPRMS() As UInt16
        Private GPRMS() As UInt16

#End Region 'FIELDS

#Region "CONSTRUCTORS"

        Public Sub New(ByRef IFO() As Byte, ByVal Offset As Long)
            Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian
            ParseCommand(EBC.ToUInt64(IFO, Offset))
        End Sub

        Public Sub New(ByVal nBitmask As UInt64)
            Try
                Dim h As String = Hex(nBitmask)
                h = h.PadLeft(16, "0")
                Dim a(7) As Byte
                Dim cnt As Byte = 0
                For i As Integer = 1 To 15 Step 2
                    a(cnt) = Byte.Parse(Mid(h, i, 2), System.Globalization.NumberStyles.HexNumber)
                    cnt += 1
                Next
                ParseCommand(BitConverter.ToUInt64(a, 0))
            Catch ex As Exception
                Throw New Exception("Problem with New(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Sub New(ByVal hex_cmd As String, ByRef nSPRMS() As UInt16, ByRef nGPRMS() As UInt16)
            Try
                SPRMS = nSPRMS
                GPRMS = nGPRMS
                If hex_cmd.Length < 16 Then hex_cmd = hex_cmd.PadLeft(16, "0")
                Dim a(7) As Byte
                Dim cnt As Byte = 0
                For i As Integer = 1 To 15 Step 2
                    a(cnt) = Byte.Parse(Mid(hex_cmd, i, 2), System.Globalization.NumberStyles.HexNumber)
                    cnt += 1
                Next
                ParseCommand(BitConverter.ToUInt64(a, 0))
            Catch ex As Exception
                Throw New Exception("Problem with New(). Error: " & ex.Message, ex)
            End Try
        End Sub

#End Region 'CONSTRUCTORS

#Region "PARSING"

        Private Sub ParseCommand(ByVal nBitmask As UInt64)
            Try
                'Debug.WriteLine("ParseCommand: " & Hex(nBitmask))

                Command_Bitmask = nBitmask
                Select Case CmdId1
                    Case 0, 1
                        ParseCommand_Type1()
                    Case 2, 3
                        ParseCommand_Type2()
                    Case 4, 5, 6
                        ParseCommand_Type3()
                    Case 7
                        CommandName = "Reserved"
                        Exit Sub
                End Select
            Catch ex As Exception
                Throw New Exception("Problem with ParseCommand(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Private Sub ParseCommand_Type1()
            Try
                CommandType = 1

                'OPERATION CODE & OPERAND SET
                OperationCode = (Command_Bitmask >> 48) And &HFFFF
                OperandSet = Command_Bitmask And &HFFFFFFFFFFFF

                'PARSE OPERATION CODE
                CmdID2 = (Command_Bitmask >> 60) And 1
                IFlagForCompare = (Command_Bitmask >> 55) And 1
                CompareField = (Command_Bitmask >> 52) And 7
                BranchField = (Command_Bitmask >> 48) And 15

                Dim sb As New StringBuilder

                'PARSE OPERAND SET
                Select Case CmdId1

                    Case 0 'Nop, GoTo, Compare GoTo

                        'Nop
                        If CompareField = 0 And BranchField = 0 Then
                            CommandName = "Nop"
                            Exit Sub
                        End If

                        'Compare 
                        If CompareField <> 0 Then
                            C2 = New cCompare2((OperandSet >> 16) And &HFF)
                            CP1 = (OperandSet >> 32) And &HFF
                            sb.Append(GetCompareString(CP1, C2))
                        End If

                        'GoTo
                        Me.GoToOption = BranchField
                        GoToOperand = OperandSet And &HFFFF
                        sb.Append(GoToOption.ToString & " ")
                        Select Case GoToOption
                            Case eGoToValues.GoTo
                                sb.Append((GoToOperand And 255))
                            Case eGoToValues.Break
                                'don't need to add anything
                            Case eGoToValues.SetTmpPML
                                sb.Append(((GoToOperand >> 8) And 15) & " GoTo " & (GoToOperand And 255))
                        End Select

                    Case 1 'Link, Compare Link, Jump, Compare Jump

                        'Link/Compare Link
                        If CmdID2 = 0 Then
                            'Compare
                            If CompareField <> 0 Then
                                C2 = New cCompare2(OperandSet >> 16 And &HFF)
                                CP1 = OperandSet >> 32 And &HFF
                                sb.Append(GetCompareString(CP1, C2))
                            End If

                            'Link
                            sb.Append(GetLinkString(BranchField))
                        End If

                        'Jump/Compare Jump
                        If CmdID2 = 1 Then

                            JumpOption = BranchField
                            JumpOperand = (OperandSet >> 16) And &HFFFFFFFF

                            'Compare
                            If CompareField <> 0 Then
                                CP2 = New cCompare2(JumpOperand And &HFF)
                                CP1 = (JumpOperand >> 8) And &HFF
                                sb.Append(GetCompareString(CP1, CP2))
                            End If

                            'Jump
                            sb.Append(GetJumpString)

                        End If
                End Select

                CommandName = sb.ToString

            Catch ex As Exception
                Throw New Exception("Problem with ParseCommand_Type1(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Private Sub ParseCommand_Type2()
            Try
                CommandType = 2

                'OPERATION CODE & OPERAND SET
                OperationCode = (Command_Bitmask >> 48) And &HFFFF
                OperandSet = Command_Bitmask And &HFFFFFFFFFFFF

                'PARSE OPERATION CODE
                IFlagForSetSetSystem = (Command_Bitmask >> 60) And 1
                SetSetSystemField = (Command_Bitmask >> 56) And &HF
                IFlagForCompare = (Command_Bitmask >> 55) And 1
                CompareField = (Command_Bitmask >> 52) And 7
                BranchField = (Command_Bitmask >> 48) And 15

                Dim sb As New StringBuilder

                Select Case CmdId1
                    Case 2 'SetSystem, Compare SetSystem, SetSystem Link

                        SetSystemOperand = (OperandSet >> 16) And &HFFFFFFFF

                        'Compare
                        If CompareField <> 0 Then
                            CP2 = New cCompare2(OperandSet And &HFF)
                            CP1 = (OperandSet >> 8) And &HFF
                            sb.Append(GetCompareString(CP1, CP2))
                        End If

                        'SetSystem
                        sb.Append(GetSetSystemString)

                        'Link
                        If BranchField <> 0 Then sb.Append(GetLinkString(BranchField))

                    Case 3 'Set, Compare Set, Set Link

                        'Compare
                        If CompareField <> 0 Then
                            C2 = New cCompare2(OperandSet And &HFFFF, IFlagForCompare)
                            CP1 = (OperandSet >> 40) And &HFF
                            sb.Append(GetCompareString(CP1, C2))
                        End If

                        'Set
                        Dim SS As UInt16 = (OperandSet >> 16) And &HFFFF
                        Dim SDG As Byte = (OperandSet >> 32) And &HF
                        sb.Append(GetSetString(SDG, SS))

                        'Link
                        If BranchField <> 0 Then sb.Append(GetLinkString(BranchField))

                End Select

                Me.CommandName = Sb.tostring

            Catch ex As Exception
                Throw New Exception("Problem with ParseCommand_Type2(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Private Sub ParseCommand_Type3()
            Try
                CommandType = 3

                'OPERATION CODE & OPERAND SET
                OperationCode = (Command_Bitmask >> 52) And &HFFF
                OperandSet = Command_Bitmask And &HFFFFFFFFFFFFFUL

                'PARSE OPERATION CODE  
                IFlagForSetSetSystem = (Command_Bitmask >> 60) And 1
                SetSetSystemField = (Command_Bitmask >> 56) And &HF
                IFlagForCompare = (Command_Bitmask >> 55) And 1
                CompareField = (Command_Bitmask >> 52) And 7
                CompareOption = CompareField

                Dim sb As New StringBuilder

                Select Case CmdId1
                    Case 4
                        'Set Compare LinkSIns
                        Dim LinkSInsOperand As UInt16 = OperandSet And &HFFFF
                        Dim C2 As New cCompare2((OperandSet >> 16) And &HFFFF, IFlagForCompare)
                        Dim SS As UInt16 = (OperandSet >> 32) And &HFFFF
                        Dim SCG As Byte = (OperandSet >> 48) And &HF

                        'sb.Append("SetCompareLink | ")

                        'Set
                        sb.Append(GetSetString(SCG, SS))

                        'Compare
                        sb.Append(GetCompareString(SCG, C2))

                        'LinkSIns
                        sb.Append(GetLinkString(LinkSInsOperand))

                    Case 5, 6
                        'Compare & Set-LinkSIns
                        'Compare-Set & LinkSIns

                        'NEEDS TO BE TESTED

                        'Select Case CmdId1
                        '    Case 5
                        '        sb.Append("Compare & Set-LinkSIns | ")
                        '    Case 6
                        '        sb.Append("Compare-Set & LinkSIns | ")
                        'End Select

                        Dim LinkSInsOperand As UInt16 = OperandSet And &HFFFF
                        Dim SDG As Byte = (OperandSet >> 48) And &HF

                        If IFlagForSetSetSystem = 0 Then
                            Dim C2 As New cCompare2((OperandSet >> 16) And &HFFFF, IFlagForCompare)
                            Dim CP1 As Byte = (OperandSet >> 32) And &HFF
                            Dim SSP As Byte = (OperandSet >> 40) And &HFF
                            sb.Append(GetCompareString(CP1, C2))
                            sb.Append(GetSetString(SDG, SSP))
                            sb.Append(GetLinkString(BranchField))
                        Else
                            Dim CP2 As New cCompare2((OperandSet >> 16) And &HFF)
                            Dim CP1 As Byte = (OperandSet >> 24) And &HFF
                            Dim SSV As UInt16 = (OperandSet >> 32) And &HFFFF
                            sb.Append(GetCompareString(CP1, CP2))
                            sb.Append(GetSetString(SDG, SSV))
                            sb.Append(GetLinkString(BranchField))
                        End If


                End Select

                CommandName = sb.ToString

            Catch ex As Exception
                Throw New Exception("Problem with ParseCommand_Type3(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Private Function GetCompareString(ByVal CP1_SCG As Byte, ByVal C2 As cCompare2) As String
            Try
                Me.CompareOption = CompareField
                If IFlagForCompare Then
                    If GPRMS IsNot Nothing Then
                        _CompareEvaluates = EvaluateCompare(GPRMS(CP1_SCG), C2.Val, CompareOption)
                        Return "If GPRM" & CP1_SCG & "(" & GPRMS(CP1_SCG) & ") " & Me.CompareOption.ToString & " " & C2.Val & " Then "
                    Else
                        Return "If GPRM" & CP1_SCG & " " & Me.CompareOption.ToString & " " & C2.Val & " Then "
                    End If
                Else
                    If GPRMS IsNot Nothing Then
                        _CompareEvaluates = EvaluateCompare(GPRMS(CP1_SCG), If(C2.IsSPRM, SPRMS(C2.Val), GPRMS(C2.Val)), CompareOption)
                        Dim stg As String = C2.ToString
                        Dim nbr As Byte = Right(stg, stg.Length - 4)
                        Dim valu As UInt16 = If(InStr(stg, "SPRM"), SPRMS(nbr), GPRMS(nbr))
                        Return "If GPRM" & CP1_SCG & "(" & GPRMS(CP1_SCG) & ") " & Me.CompareOption.ToString & " " & stg & "(" & valu & ") Then "
                    Else
                        Return "If GPRM" & CP1_SCG & " " & Me.CompareOption.ToString & " " & C2.ToString & " Then "
                    End If
                End If
            Catch ex As Exception
                Throw New Exception("Problem with GetCompareString(). Error: " & ex.Message, ex)
            End Try
        End Function

        Private Function GetLinkString(ByVal LinkOption As UInt16) As String
            Try
                'LinkOption = BranchField
                LinkOperand = OperandSet And &HFFFF

                Select Case LinkOption
                    Case eLinkValues.LinkPGCN
                        Return "LinkPGCN " & LinkOperand & " "
                    Case eLinkValues.LinkPTTN
                        Return "LinkPTTN " & (LinkOperand And 1023) & " HL# " & (LinkOperand >> 10 And 63) & " "
                    Case eLinkValues.LinkPGN
                        Return "LinkPGN " & (LinkOperand And 127) & " HL# " & (LinkOperand >> 10 And 63) & " "
                    Case eLinkValues.LinkCN
                        Return "LinkCN " & (LinkOperand And 255) & " HL# " & (LinkOperand >> 10 And 63) & " "
                    Case eLinkValues.LinkSIns
                        Dim LinkSubInstruction As eLinkSubInstruction = LinkOperand And 255
                        Return "LinkSIns: " & LinkSubInstruction.ToString & " HL# " & (LinkOperand >> 10 And 63) & " "
                End Select
            Catch ex As Exception
                Throw New Exception("Problem with GetLinkString(). Error: " & ex.Message, ex)
            End Try
        End Function

        Private Function GetSetString(ByVal SCG_SDG As Byte, ByVal SS As UInt16)
            Try
                Dim PRM_flag As Boolean
                Dim ParameterNumber As Byte
                Dim ParameterNumber2 As Byte
                Dim ImmediateValue As UInt16

                Dim sb As New StringBuilder

                Dim Op As eSetValues = SetSetSystemField
                sb.Append(Op.ToString & " ")

                Select Case IFlagForSetSetSystem
                    Case 0 'Using parameter value
                        ParameterNumber = SCG_SDG And &HF
                        ParameterNumber2 = SS And &H1F
                        PRM_flag = (SS >> 7) And 1
                    Case 1 'Using immediate value
                        ImmediateValue = SS
                End Select

                Select Case Op
                    Case eSetValues.Mov, eSetValues.And, eSetValues.Or, eSetValues.Xor
                        If IFlagForSetSetSystem = 0 Then
                            If GPRMS IsNot Nothing Then
                                sb.Append("GPRM" & SCG_SDG & "(" & GPRMS(SCG_SDG) & ") " & If(PRM_flag, "SPRM", "GPRM") & ParameterNumber2 & "(" & If(PRM_flag, SPRMS(ParameterNumber2), GPRMS(ParameterNumber2)) & ")")
                            Else
                                sb.Append("GPRM" & SCG_SDG & " " & If(PRM_flag, "SPRM", "GPRM") & ParameterNumber2 & " ")
                            End If
                        Else
                            If GPRMS IsNot Nothing Then
                                sb.Append("GPRM" & SCG_SDG & "(" & GPRMS(SCG_SDG) & ") " & ImmediateValue & " ")
                            Else
                                sb.Append("GPRM" & SCG_SDG & " " & ImmediateValue & " ")
                            End If
                        End If

                    Case eSetValues.Swp
                        If GPRMS IsNot Nothing Then
                            sb.Append("GPRM" & SCG_SDG & "(" & GPRMS(SCG_SDG) & ") " & "GPRM" & ParameterNumber2 & "(" & GPRMS(ParameterNumber2) & ") ")
                        Else
                            sb.Append("GPRM" & SCG_SDG & " " & "GPRM" & ParameterNumber2 & " ")
                        End If

                    Case eSetValues.Add, eSetValues.Sub, eSetValues.Mul, eSetValues.Div, eSetValues.Mod, eSetValues.Rnd
                        If IFlagForSetSetSystem = 0 Then
                            If GPRMS IsNot Nothing Then
                                sb.Append("GPRM" & SCG_SDG & "(" & GPRMS(SCG_SDG) & ") " & "GPRM" & ParameterNumber2 & "(" & GPRMS(ParameterNumber2) & ") ")
                            Else
                                sb.Append("GPRM" & SCG_SDG & " " & "GPRM" & ParameterNumber2 & " ")
                            End If
                        Else
                            If GPRMS IsNot Nothing Then
                                sb.Append("GPRM" & SCG_SDG & "(" & GPRMS(SCG_SDG) & ") " & ImmediateValue & " ")
                            Else
                                sb.Append("GPRM" & SCG_SDG & " " & ImmediateValue & " ")
                            End If
                        End If

                End Select

                Return sb.ToString

            Catch ex As Exception
                Throw New Exception("Problem with GetSetString(). Error: " & ex.Message, ex)
            End Try
        End Function

        Private Function GetJumpString() As String
            Try
                Select Case JumpOption
                    Case eJumpValues.Exit
                        Return "Exit"
                    Case eJumpValues.JumpTT
                        Return "JumpTT " & (JumpOperand And 127)
                    Case eJumpValues.JumpVTS_TT
                        Return "JumpVTS_TT " & (JumpOperand And 127)
                    Case eJumpValues.JumpVTS_PTT
                        Return "JumpVTS_PTT VTS_TTN: " & (JumpOperand And 127) & " PTTN: " & (JumpOperand >> 16 And 1023)
                    Case eJumpValues.JumpSS
                        Dim DomainID As eDomainID = JumpOperand >> 6 And 3
                        Dim MenuID As eMenuID = JumpOperand And 15
                        Select Case DomainID
                            Case eDomainID.VTSM_DOM_MenuID
                                Dim VTS_TTN As Byte = (JumpOperand >> 16 And 127)
                                Dim VTSN As Byte = JumpOperand >> 8 And 127
                                Return "JumpSS " & DomainID.ToString & " MenuID: " & MenuID.ToString & " VTSN: " & VTSN & " VTS_TTN: " & VTS_TTN
                            Case Else
                                Dim VMGM_PGCN As UInt16 = (JumpOperand >> 16) And &H7FFF
                                Return "JumpSS " & DomainID.ToString & " MenuID: " & MenuID.ToString & " VMGM_PGCN: " & VMGM_PGCN
                        End Select

                    Case eJumpValues.CallSS
                        Dim DomainID As eDomainID = JumpOperand >> 6 And 3
                        Dim MenuID As eMenuID = JumpOperand And 15
                        Dim VMGM_PGCN As UInt16 = JumpOperand >> 16 And 32767
                        Dim CN_for_RSM As Byte = JumpOperand >> 8 And 255
                        Return "CallSS Domain:" & DomainID.ToString & " MenuID: " & MenuID.ToString & " VMGM_PGCN: " & VMGM_PGCN & " CN for RSM: " & CN_for_RSM

                        'CHECK FOR SCENARIST BUG
                        If CN_for_RSM = 127 Then
                            IsCallSSRSM127 = True
                        End If

                End Select
            Catch ex As Exception
                Throw New Exception("Problem with GetJumpString(). Error: " & ex.Message, ex)
            End Try
        End Function

        Private Function GetSetSystemString() As String
            Try
                Dim sb As New StringBuilder

                Dim Op As eSetSystemValues = SetSetSystemField

                sb.Append(Op.ToString)

                Select Case Op
                    Case eSetSystemValues.SetSTN
                        Dim AGL_flag As Byte = (SetSystemOperand >> 7) And 1
                        Dim SP_flag As Byte = (SetSystemOperand >> 15) And 1
                        Dim A_flag As Byte = (SetSystemOperand >> 23) And 1

                        Dim AGLN As Byte = (SetSystemOperand) And &HF
                        Dim SPSTN As Byte = (SetSystemOperand >> 8) And &H3F
                        Dim SP_disp_flag As Byte = (SetSystemOperand >> 14) And 1
                        Dim ASTN As Byte = (SetSystemOperand >> 16) And &HF

                        Dim GforAGLN As Byte = (SetSystemOperand) And &HF
                        Dim GforSPSTN As Byte = (SetSystemOperand >> 8) And &HF
                        Dim GforASTN As Byte = (SetSystemOperand >> 16) And &HF

                        If A_flag Then
                            sb.Append(" Set audio to ")
                            sb.Append(If(IFlagForSetSetSystem, "Stream " & ASTN, "GPRM" & GforASTN & If(GPRMS IsNot Nothing, "(" & GPRMS(GforASTN) & ")", "")))
                        End If

                        If SP_flag Then
                            sb.Append(" Set subpicture to ")
                            sb.Append(If(IFlagForSetSetSystem, "Stream " & SPSTN, "GPRM" & GforSPSTN & If(GPRMS IsNot Nothing, "(" & GPRMS(GforSPSTN) & ")", "")))
                            sb.Append(If(SP_disp_flag, " ON", " OFF"))
                        End If

                        If AGL_flag Then
                            sb.Append(" Set angle to ")
                            sb.Append(If(IFlagForSetSetSystem, "Stream " & AGLN, "GPRM" & GforAGLN & If(GPRMS IsNot Nothing, "(" & GPRMS(GforAGLN) & ")", "")))
                        End If

                    Case eSetSystemValues.SetNVTMR
                        Dim NV_TMR As UInt16 = (SetSystemOperand >> 16) And &HFFFF
                        Dim TT_PGCN As UInt16 = SetSystemOperand And &H7FFF
                        Dim GPRMN As Byte = (SetSystemOperand >> 16) And &HF

                        If IFlagForSetSetSystem Then
                            sb.Append(" TMR=" & NV_TMR & " TGT=TT_PGCN " & TT_PGCN)
                        Else
                            sb.Append(" TMR=GPRM" & GPRMN & If(GPRMS IsNot Nothing, "(" & GPRMS(GPRMN) & ")", "") & " TGT=TT_PGCN " & TT_PGCN)
                        End If

                    Case eSetSystemValues.SetGPRMMD
                        Dim InitVal As UInt16 = (SetSystemOperand >> 16) And &HFFFF
                        Dim Mode As eGPRMMode = (SetSystemOperand >> 7) And 1
                        Dim GPRMN_ForInitVal As Byte = (SetSystemOperand >> 16) And &HF
                        Dim GPRMN_ModeChange As Byte = SetSystemOperand And &HF

                        If IFlagForSetSetSystem Then
                            sb.Append(" mov GPRM" & GPRMN_ModeChange & If(GPRMS IsNot Nothing, "(" & GPRMS(GPRMN_ModeChange) & ")", "") & " " & InitVal & " Mode=" & Mode.ToString)
                        Else
                            sb.Append(" mov GPRM" & GPRMN_ModeChange & If(GPRMS IsNot Nothing, "(" & GPRMS(GPRMN_ModeChange) & ")", "") & " GPRM" & GPRMN_ForInitVal & If(GPRMS IsNot Nothing, "(" & GPRMS(GPRMN_ForInitVal) & ")", "") & " Mode=" & Mode.ToString)
                        End If

                    Case eSetSystemValues.SetAMXMD
                        Dim ACH4_mixing_ACH0 As Byte = (SetSystemOperand >> 12) And 1
                        Dim ACH3_mixing_ACH0 As Byte = (SetSystemOperand >> 11) And 1
                        Dim ACH2_mixing_ACH0 As Byte = (SetSystemOperand >> 10) And 1
                        Dim ACH4_mixing_ACH1 As Byte = (SetSystemOperand >> 4) And 1
                        Dim ACH3_mixing_ACH1 As Byte = (SetSystemOperand >> 3) And 1
                        Dim ACH2_mixing_ACH1 As Byte = (SetSystemOperand >> 2) And 1
                        Dim GPRMN_AMXMD As Byte = SetSystemOperand And &HF

                        If IFlagForSetSetSystem Then
                            sb.Append(" Values=" & ACH4_mixing_ACH0 & ", " & ACH3_mixing_ACH0 & ", " & ACH2_mixing_ACH0 & ", " & ACH4_mixing_ACH1 & ", " & ACH3_mixing_ACH1 & ", " & ACH2_mixing_ACH1)
                        Else
                            sb.Append(" Use GPRM" & GPRMN_AMXMD & If(GPRMS IsNot Nothing, "(" & GPRMS(GPRMN_AMXMD) & ")", ""))
                        End If

                    Case eSetSystemValues.SetHL_BTNN
                        Dim HL_BTNN As Byte = (SetSystemOperand >> 10) And &H3F
                        Dim GPRMN_HL_BTNN As Byte = SetSystemOperand And &HF

                        If IFlagForSetSetSystem Then
                            sb.Append(" Btn=" & HL_BTNN)
                        Else
                            sb.Append(" GPRM" & GPRMN_HL_BTNN & If(GPRMS IsNot Nothing, "(" & GPRMS(GPRMN_HL_BTNN) & ")", ""))
                        End If

                End Select

                Return sb.ToString

            Catch ex As Exception
                Throw New Exception("Problem with GetSetSystemString(). Error: " & ex.Message, ex)
            End Try
        End Function

#End Region 'PARSING

#Region "CMD EVALUATION"

        Private Shared Function EvaluateCompare(ByVal Val1 As UInt16, ByVal Val2 As UInt16, ByVal CompareOption As eCompareValues) As Boolean
            Try
                Select Case CompareOption
                    Case eCompareValues.Bitwise
                        Return Val1 And Val2
                    Case eCompareValues.IsEqualTo
                        Return Val1 = Val2
                    Case eCompareValues.IsGreaterThan
                        Return Val1 > Val2
                    Case eCompareValues.IsGreaterThanOrEqualTo
                        Return Val1 >= Val2
                    Case eCompareValues.IsLessThan
                        Return Val1 < Val2
                    Case eCompareValues.IsLessThanOrEqualTo
                        Return Val1 <= Val2
                    Case eCompareValues.IsNotEqualTo
                        Return Val1 <> Val2
                End Select
            Catch ex As Exception
                Throw New Exception("Problem with EvaluateCompare(). Error: " & ex.Message, ex)
            End Try
        End Function

#End Region 'CMD EVALUATION

#Region "PUBLIC METHODS"

        Public Overrides Function ToString() As String
            Return CommandName & If(HasCompare, " [Evaluates " & CompareEvaluates & " ]", "")
        End Function

#End Region 'PUBLIC METHODS

#Region "ENUMS"

        Public Enum eGoToValues
            [GoTo] = 1
            Break = 2
            SetTmpPML = 3
        End Enum

        Public Enum eLinkValues
            LinkSIns = 1
            LinkPGCN = 4
            LinkPTTN = 5
            LinkPGN = 6
            LinkCN = 7
        End Enum

        Public Enum eJumpValues
            [Exit] = 1
            JumpTT = 2
            JumpVTS_TT = 3
            JumpVTS_PTT = 5
            JumpSS = 6
            CallSS = 8
        End Enum

        Public Enum eCompareValues
            Bitwise = 1
            IsEqualTo = 2
            IsNotEqualTo = 3
            IsGreaterThanOrEqualTo = 4
            IsGreaterThan = 5
            IsLessThanOrEqualTo = 6
            IsLessThan = 7
        End Enum

        Public Enum eSetSystemValues
            SetSTN = 1
            SetNVTMR = 2
            SetGPRMMD = 3
            SetAMXMD = 4
            SetHL_BTNN = 6
        End Enum

        Public Enum eSetValues
            Mov = 1
            Swp = 2
            Add = 3
            [Sub] = 4
            Mul = 5
            Div = 6
            [Mod] = 7
            Rnd = 8
            [And] = 9
            [Or] = 10
            [Xor] = 11
        End Enum

        Public Enum eLinkSubInstruction
            LinkNoLink = &H0
            LinkTopC = &H1
            LinkNextC = &H2
            LinkPrevC = &H3
            LinkTopPG = &H5
            LinkNextPG = &H6
            LinkPrevPG = &H7
            LinkTopPGC = &H9
            LinkNextPGC = &HA
            LinkPrevPGC = &HB
            LinkGoUpPGC = &HC
            LinkTailPGC = &HD
            RSM = &H10
        End Enum

        Public Enum eDomainID
            FP_DOM
            VMGM_DOM_MenuID
            VTSM_DOM_MenuID
            VMGM_DOM_PGCN
        End Enum

        Public Enum eMenuID
            TitleMenu = 2
            RootMenu = 3
            SubpictureMenu = 4
            AudioMenu = 5
            AngleMenu = 6
            ChapterMenu = 7
        End Enum

        Public Enum eGPRMMode As Byte
            Register = 0
            Counter = 1
        End Enum

#End Region 'ENUMS

#Region "CLASSES"

        Public Class cCompare2

            Public IsSPRM As Boolean
            Public Val As UInt16
            Private IFlagForCompare As Boolean
            Private IsC2 As Boolean

            Public Sub New(ByVal input As Byte)
                Val = input And &H1F
                IsSPRM = (input >> 7) And 1
                IsC2 = False
            End Sub

            Public Sub New(ByVal input As UInt16, ByVal IFCompare As Boolean)
                IsC2 = True
                IFlagForCompare = IFCompare
                If IFlagForCompare Then
                    Val = input
                Else
                    Val = input And &H1F
                    IsSPRM = (input >> 7) And 1
                End If
            End Sub

            Public Overrides Function ToString() As String
                If Not IsC2 Or (IsC2 And Not IFlagForCompare) Then
                    Return If(IsSPRM, "SPRM", "GPRM") & Val
                Else
                    Return Val
                End If
            End Function

        End Class

#End Region 'CLASSES

    End Class

    Public Class colPGCs
        Inherits CollectionBase

        Public Function Add(ByVal newPGC As cPGC) As Integer
            Return MyBase.List.Add(newPGC)
        End Function

        Default Public ReadOnly Property Item(ByVal index As Integer) As cPGC
            Get
                Return MyBase.List.Item(index)
            End Get
        End Property

        Public Sub Remove(ByVal Item As cPGC)
            MyBase.List.Remove(Item)
        End Sub

    End Class

    Public Class cPGC_AST_CTLT

        Public PGC_AST_CTLs() As cPGC_AST_CTL

        Public Sub New(ByRef IFO() As Byte, ByVal Offset As ULong)
            ReDim PGC_AST_CTLs(11)
            Dim Pos As Byte = 0
            For i As Byte = 0 To 11
                PGC_AST_CTLs(i) = New cPGC_AST_CTL(IFO, Offset + Pos)
                Pos += 2
            Next
        End Sub

        Public Class cPGC_AST_CTL

            Public Available As Boolean
            Public DecodingStreamNumber As Byte

            Public Sub New(ByRef IFO() As Byte, ByVal StartAddress As ULong)
                Try
                    Available = IFO(StartAddress) >> 7 And 1
                    DecodingStreamNumber = IFO(StartAddress) And 7
                Catch ex As Exception
                    Throw New Exception("Problem with New() cPGC_AST_CTL. Error: " & ex.Message, ex)
                End Try
            End Sub

        End Class

    End Class

    Public Class cCellPositionInfo

    End Class

    ''' <summary>
    ''' C_PBI - 4.3.5-1
    ''' </summary>
    ''' <remarks></remarks>
    Public Class cCellPlaybackInfo

        Public VTSNumber As Short
        Public TitleNumber As Short
        Public PGCNumber As Short
        Public CellNumber As Short

        'C_CAT
        Public BlockMode As eCellBlockMode
        Public BlockType As eCellBlockType
        Public Seamless As Boolean
        Public InterleavedAllocation As Boolean
        Public STCDiscontinuity As Boolean
        Public SeamlessAngleChange As Boolean
        Public PlaybackMode As eCellPlaybackMode
        Public AccessRestriction As Boolean
        Public Type As eCellType
        Public StillTime As Byte
        Public CommandNumber As Byte

        'C_PBTM
        Public CellPlaybackTime As cTimecode

        'Start Addresses
        Public C_FVOBU_SA As UInt32
        Public C_FILVU_EA As UInt32
        Public C_LVOBU_SA As UInt32 'this is vital for NSC notification if this cell is not seamless
        Public C_LVOBU_EA As UInt32

        Public Sub New(ByRef bytes() As Byte, ByVal Offset As ULong, ByVal CellNum As Short, ByVal VTSNum As Short, ByVal PGCNum As Short)
            Try
                'Debug.WriteLine("Offset: " & Offset)
                CellNumber = CellNum
                VTSNumber = VTSNum
                PGCNumber = PGCNum

                BlockMode = bytes(Offset) >> 6 And 3
                BlockType = bytes(Offset) >> 4 And 3
                Seamless = bytes(Offset) >> 3 And 1
                'If Not Seamless Then
                '    Debug.WriteLine("hi")
                'End If
                'If Not Seamless And VTSNum > -1 Then
                '    Debug.WriteLine("seamless")
                'End If
                'If VTSNum > -1 Then
                '    Debug.WriteLine("hi")
                'End If
                InterleavedAllocation = bytes(Offset) >> 2 And 1
                STCDiscontinuity = bytes(Offset) >> 1 And 1
                SeamlessAngleChange = bytes(Offset) And 1

                PlaybackMode = bytes(Offset + 1) >> 6 And 1
                AccessRestriction = bytes(Offset + 1) >> 5 And 1
                Type = bytes(Offset) And 31

                StillTime = bytes(Offset + 2)
                'Debug.WriteLine("IFOProcessing - Cell Still Time: " & StillTime)

                CommandNumber = bytes(Offset + 3)

                CellPlaybackTime = New cTimecode(bytes, Offset + 4)

                'could use the EBC here instead of byteswap
                ByteSwap(bytes, 4, Offset + 8)
                C_FVOBU_SA = BitConverter.ToUInt32(bytes, Offset + 8)
                ByteSwap(bytes, 4, Offset + 12)
                C_FILVU_EA = BitConverter.ToUInt32(bytes, Offset + 12)
                ByteSwap(bytes, 4, Offset + 16)
                C_LVOBU_SA = BitConverter.ToUInt32(bytes, Offset + 16)
                ByteSwap(bytes, 4, Offset + 20)
                C_LVOBU_EA = BitConverter.ToUInt32(bytes, Offset + 20)

            Catch ex As Exception
                Throw New Exception("Problem with New cCellPlaybackInfo. Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Overrides Function ToString() As String
            Try
                Dim out As New StringBuilder
                out.Append("VTS: " & VTSNumber & " ")
                out.Append("Title: " & TitleNumber & " ")
                out.Append("PGC: " & PGCNumber & " ")
                out.Append("Cell: " & CellNumber & " ")
                Return out.ToString
            Catch ex As Exception
                Throw New Exception("Problem with cCellPlaybackInfo ToString(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Enum eCellBlockMode
            NotAPGCInTheBlock
            FirstPGCInTheBlock
            PGCInTheBlockExceptFirstOrLast
            LastPGCInTheBlock
        End Enum

        Public Enum eCellBlockType
            NotAPartOfTheBlock
            AngleBlock
        End Enum

        Public Enum eCellPlaybackMode
            ContinuiouslyPresentedInTheCell
            StillsInEveryVOBU
        End Enum

        Public Enum eCellType
            NotSpecified
            TitlePicture
            Introduction
            Song_ExceptClimax
            Song_Climax1
            Song_Climax2
            Song_MaleVocal
            Song_FemaleVocal
            Song_MaleAndFemaleVocal
            Interlude_InstrumentalBreak
            InterludeForFadeIn
            IngerludeForFadeOut
            Ending_1
            Ending_2
        End Enum

    End Class

    Public Class colCellPlaybackInfo
        Inherits CollectionBase

        Public Function Add(ByVal newCPI As cCellPlaybackInfo) As Integer
            Return MyBase.List.Add(newCPI)
        End Function

        Default Public ReadOnly Property Item(ByVal index As Integer) As cCellPlaybackInfo
            Get
                Return MyBase.List.Item(index)
            End Get
        End Property

        Public Sub Remove(ByVal Item As cCellPlaybackInfo)
            MyBase.List.Remove(Item)
        End Sub

    End Class

    Public Class colProgramMap
        Inherits CollectionBase

        Public Function Add(ByVal newPM As cProgramMap) As Integer
            Return MyBase.List.Add(newPM)
        End Function

        Default Public ReadOnly Property Item(ByVal index As Integer) As cProgramMap
            Get
                Return MyBase.List.Item(index)
            End Get
        End Property

        Public Sub Remove(ByVal Item As cProgramMap)
            MyBase.List.Remove(Item)
        End Sub

        Public Sub New(ByRef Bytes() As Byte, ByVal Offset As Integer, ByVal ProgramCount As Integer)
            Try
                For i As Short = 0 To ProgramCount - 1
                    Add(New cProgramMap(i + 1, Bytes(Offset + i)))
                Next
            Catch ex As Exception
                Throw New Exception("Problem with GetProgramMap. Error: " & ex.Message, ex)
            End Try
        End Sub

    End Class

    Public Class cProgramMap

        Public ProgramNo As Short
        Public CellNo As Short

        Public Sub New(ByVal PM_NO As Short, ByVal C_NO As Short)
            ProgramNo = PM_NO
            CellNo = C_NO
        End Sub

    End Class

    Public Class cPGC_SPST_CTLT

    End Class

    Public Class cCLUT

    End Class

#End Region 'PGC

#Region "Subpicture Attributes"

    Public Class cSubpictureAttributes

        Public Streams() As cVXXX_SPST_ATR
        Public StreamCount As Integer

        Public Sub New(ByRef Bytes() As Byte, ByVal Offset As Integer, ByVal nStreamCount As Integer)
            Try
                ReDim Streams(nStreamCount - 1)
                Dim cnt As Byte = 0
                For i As Integer = 0 To (nStreamCount * 6) - 1 Step 6
                    If BitConverter.ToInt32(Bytes, Offset + i) > 0 Then StreamCount += 1
                    Streams(cnt) = New cVXXX_SPST_ATR(Bytes, Offset + i)
                    cnt += 1
                Next
            Catch ex As Exception
                Throw New Exception("Problem with New() cSubpictureAttributes. Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Class cVXXX_SPST_ATR

            Public CodingMode As eSubCodingMode
            Public LangSpecified As Boolean
            Public LangCode() As Byte
            Public LangCodeEx As eSubLangCodeEx

            Public Sub New(ByRef Bytes() As Byte, ByVal offset As Integer)
                Try
                    CodingMode = (Bytes(offset + 0) >> 5) And 7
                    LangSpecified = (Bytes(offset + 0) >> 0) And 3
                    ReDim LangCode(1)
                    LangCode(0) = Bytes(offset + 2)
                    LangCode(1) = Bytes(offset + 3)
                    LangCodeEx = Bytes(offset + 5)
                Catch ex As Exception
                    Throw New Exception("Problem with New() cSubpictureAttributes. Error: " & ex.Message)
                End Try
            End Sub

            Public ReadOnly Property LangCodeAsString() As String
                Get
                    If LangCode(0) = 0 And LangCode(1) = 0 Then Return ""
                    Return System.Text.ASCIIEncoding.ASCII.GetChars(LangCode)
                End Get
            End Property

        End Class

    End Class

    Public Enum eSubCodingMode
        TwoBitRLE = 0
    End Enum

    Public Enum eSubLangCodeEx
        Not_Specified = 0
        Normal = 1
        Large = 2
        Children = 3
        Normal_Captions = 5
        Large_Captions = 6
        Childrens_Captions = 7
        Forced = 9
        Director_Comments = 13
        Large_Director_Comments = 14
        Director_Comments_for_Children = 15
    End Enum

#End Region 'Subpicture Attributes

#Region "Audio Attributes"

    Public Class cTitleAudioAttributes

        Public Streams() As cVTS_AST_ATRT
        Public StreamsCount As Integer

        ''' <summary>
        ''' 64 Bytes comprising array of eight audio streams (VTS_AST_ATRT)
        ''' </summary>
        ''' <param name="Bytes"></param>
        ''' <remarks></remarks>
        Public Sub New(ByRef buf() As Byte, ByVal Offset As Integer)
            Try
                ReDim Streams(7)
                Dim cnt As Byte = 0
                StreamsCount = 0
                For b As Integer = 0 To 63 Step 8
                    If BitConverter.ToInt64(buf, Offset + b) > 0 Then StreamsCount += 1
                    Streams(cnt) = New cVTS_AST_ATRT(buf, Offset + b)
                    cnt += 1
                Next
            Catch ex As Exception
                Throw New Exception("Problem with New() cTitleAudioAttributes. Error: " & ex.Message, ex)
            End Try
        End Sub

        ''' <summary>
        ''' VTS_AST_ATRT (4.2.1-2, VI4-49)
        ''' </summary>
        ''' <remarks></remarks>
        Public Class cVTS_AST_ATRT

            Public CodingMode As eAudCodingMode
            Public MultiChEx As Byte
            Public AudioType As eAudioType
            Public ApplicationMode As eAudioApplicationMode
            Public Quantization_DRC As eAudioQuantization
            Public fs As eAudioSampleRate
            Public ChannelCount As eAudioChannelCount
            Public LanguageCode() As Byte
            Public LanguageCode_Extension As eAudLangCodeEx
            Public ApplicationInformation As cAudioApplicationInformation

            ''' <summary>
            ''' Eight bytes comprising VTS_AST_ATRT (4.2.1-2, VI4-49)
            ''' </summary>
            ''' <param name="b"></param>
            ''' <remarks></remarks>
            Public Sub New(ByRef b() As Byte, ByVal Offset As Integer)
                Try
                    Me.CodingMode = b(Offset + 0) >> 5 And 7
                    Me.MultiChEx = b(Offset + 0) >> 4 And 1
                    Me.AudioType = b(Offset + 0) >> 2 And 3
                    Me.ApplicationMode = b(Offset + 0) And 3
                    Me.Quantization_DRC = b(Offset + 1) >> 6 And 3
                    Me.fs = b(Offset + 1) >> 4 And 3
                    Me.ChannelCount = b(Offset + 1) And 7
                    Me.LanguageCode = New Byte() {b(Offset + 2), b(Offset + 3)}
                    Me.LanguageCode_Extension = b(Offset + 5)
                    Me.ApplicationInformation = New cAudioApplicationInformation(b(Offset + 7), ApplicationMode, CodingMode)
                Catch ex As Exception
                    Throw New Exception("Problem with New() cVTS_AST_ATRT. Error: " & ex.Message, ex)
                End Try
            End Sub

            Public ReadOnly Property LanguageCodeAsString() As String
                Get
                    Return System.Text.ASCIIEncoding.ASCII.GetChars(LanguageCode)
                End Get
            End Property

            Public Class cAudioApplicationInformation

                Public ChannelAssignmentMode As Byte
                Public VersionNumber As Byte
                Public MCIntro As Boolean
                Public SoloDuet As eAudioSoloDuet

                Public Sub New(ByVal b As Byte, ByVal ApplicationMode As eAudioApplicationMode, ByVal CodingMode As eAudCodingMode)
                    Me.VersionNumber = b >> 2 And 3
                    Me.MCIntro = b > 1 And 1
                    Me.SoloDuet = b And 1
                    Me.ChannelAssignmentMode = b >> 4 And 7


                    'if the need should arise to take the channel assignment determination further use the code below 
                    'and reference VIX-26, Table C-1

                    '            Select Case ApplicationMode

                    '                Case eAudioApplicationMode.Karaoke
                    '                    Select Case CodingMode
                    '                        Case eAudCodingMode.AC3

                    '                        Case eAudCodingMode.MPEG1_MPEG2WithoutExtension
                    '                        Case eAudCodingMode.MPEG2_WithExtension
                    '                        Case eAudCodingMode.LPCM
                    '                        Case eAudCodingMode.SDDS

                    '                        Case Else
                    '                            GoTo [Default]

                    '                    End Select

                    '                Case eAudioApplicationMode.Surround

                    '                Case Else
                    '[Default]:
                    '                    Me.ChannelAssignmentMode = b >> 4 And 7

                    '            End Select

                End Sub

                Public Enum eAudioSoloDuet
                    Solo
                    Duet
                End Enum

            End Class

        End Class

    End Class

    Public Class cMenuAudioAttributes

        Public CodingMode As eAudCodingMode
        Public Quantization As eAudioQuantization
        Public SampleRate As eAudioSampleRate
        Public ChannelCount As Byte

        Public Sub New(ByRef Bytes() As Byte, ByVal Offset As Integer)
            Try
                CodingMode = (Bytes(Offset) >> 5) And 7
                Quantization = (Bytes(Offset + 1) >> 6) And 3
                SampleRate = (Bytes(Offset + 1) >> 4) And 2
                ChannelCount = Bytes(Offset + 1) And 7
            Catch ex As Exception
                Throw New Exception("Problem with New() in cMenuAudioAttributes. Error: " & ex.Message, ex)
            End Try
        End Sub

    End Class

    Public Enum eAudCodingMode
        AC3 = 0
        Unknown_A = 1
        MPEG1_MPEG2WithoutExtension = 2
        MPEG2_WithExtension = 3
        LPCM = 4
        Unknown_B = 5
        DTS = 6
        SDDS = 7
    End Enum

    Public Enum eAudioQuantization
        _16bps = 0
        _20bps = 1
        _24bps = 2
        Dynamic_Range_Control = 3
    End Enum

    Public Enum eAudioSampleRate
        _48Kbps = 0
        _96Kbps = 1
    End Enum

    Public Enum eAudioType
        Not_Specified
        Language_Included
    End Enum

    Public Enum eAudioApplicationMode
        Not_Specified
        Karaoke
        Surround
    End Enum

    Public Enum eAudioChannelCount
        One_Mono
        Two_Stereo
        Three
        Four
        Five
        Six
        Seven
        Eight
    End Enum

    Public Enum eAudLangCodeEx
        Not_Specified = 0
        Normal_Captions = 1
        Audio_for_Visually_Impaired = 2
        Directors_Comments_1 = 3
        Directors_Comments_2 = 4
        ProviderDefined
    End Enum

#End Region 'Audio Attributes

#Region "Video Attributes"

    ''' <summary>
    ''' Based on VI4-42 VTSM_V_ATR & VI4-46 VTS_V_ATR
    ''' </summary>
    ''' <remarks></remarks>
    Public Class cVideoAttributes

        Public CompressionMode As eCodingMode
        Public VideoStandard As eVideoStandard
        Public AspectRatio As eAspectRatio
        Public DisplayMode As eDisplayMode
        Public SourceResolution As eResolution
        Public SourceLetterboxed As Boolean
        Public Film As eFilm
        Public Line21Field1 As Boolean
        Public Line21Field2 As Boolean

        Public PanScanAllowed As Boolean
        Public LetterboxAllowed As Boolean

        Public Sub New(ByRef b() As Byte, ByVal Offset As Integer)
            Try
                CompressionMode = b(Offset) >> 6 And 3
                VideoStandard = b(Offset) >> 4 And 3
                AspectRatio = b(Offset) >> 2 And 3
                DisplayMode = b(Offset) And 3

                Line21Field1 = b(Offset + 1) >> 7 And 1
                Line21Field2 = b(Offset + 1) >> 6 And 1
                SourceResolution = b(Offset + 1) >> 3 And 7
                SourceLetterboxed = b(Offset + 1) >> 2 And 1
                Film = b(Offset + 1) And 1

                If AspectRatio = eAspectRatio.ar4x3 Then

                    PanScanAllowed = False
                    LetterboxAllowed = False

                ElseIf AspectRatio = eAspectRatio.ar16x9 Then

                    Select Case DisplayMode
                        Case eDisplayMode.BothPanscanANDLetterbox
                            Me.PanScanAllowed = True
                            Me.LetterboxAllowed = True
                        Case eDisplayMode.LetterboxOnly
                            Me.PanScanAllowed = False
                            Me.LetterboxAllowed = True
                        Case eDisplayMode.PanscanOnly
                            Me.PanScanAllowed = False
                            Me.LetterboxAllowed = False
                    End Select

                End If



                'Dim Upper As String = DecimalToBinary(CInt(Bytes(0)))
                'Upper = PadString(Upper, 8, "0", True)
                'CodingMode = Mid(Upper, 1, 2)
                'VideoStandard = Mid(Upper, 3, 2)
                'AspectRatio = Mid(Upper, 5, 2)
                'If AspectRatio = 11 Then
                '    AspectRatio = eAspectRatio.ar16x9
                'End If
                'PanScanAllowed = Mid(Upper, 7, 1)
                'LetterboxAllowed = Mid(Upper, 8, 1)

                'Dim Lower As String = DecimalToBinary(CInt(Bytes(1)))
                'Lower = PadString(Lower, 8, "0", True)
                'Line21Field1 = Mid(Lower, 1, 1)
                'Line21Field2 = Mid(Lower, 2, 1)
                ''Bitrate = Mid(Lower, 3, 1)
                ''Resolution = Mid(Lower, 4, 2)
                'LetterboxAllowed = Mid(Lower, 6, 1)
                ''Resrved = Mid(Lower, 7, 1)
                'Film = Mid(Lower, 8, 1)

            Catch ex As Exception
                Throw New Exception("Problem with New() cVideoAttributes. Error: " & ex.Message)
            End Try
        End Sub

    End Class

    Public Enum eBitrate
        VMR = 0
        CBR = 1
    End Enum

    Public Enum eFilm
        Camera = 0
        Film_PALONLY = 1
    End Enum

    Public Enum eResolution
        'NTSC_PAL
        Res720x480_720x576 = 0
        Res704x480_704x576 = 1
        Res352x480_352x576 = 2
        Res352x240_352x288 = 3
    End Enum

    Public Enum eEncodingMode
        VBR = 0
        CBR = 1
    End Enum

    Public Enum eAspectRatio
        ar4x3 = 0
        arNotSpecified = 1
        Reserved = 2
        ar16x9 = 3
    End Enum

    Public Enum eCodingMode
        MPEG1 = 0
        MPEG2 = 1
    End Enum

    Public Enum eDisplayMode
        BothPanscanANDLetterbox
        PanscanOnly
        LetterboxOnly
        Reserved
    End Enum

#End Region 'Video Attributes

#Region "NonSeamlessCells"

    Public Class cNonSeamlessCell
        Public ID As Short
        Public VTSM As Boolean = False
        Public VMGM As Boolean = False
        Public LanguageUnitNumber As Short 'zero based
        Public LanguageUnitName As String
        Public VTS As Short
        Public VTS_TT As Short
        Public PTT As Short
        Public PGC As Short
        Public PGn As Short
        Public OutOfnPrograms As Short
        Public Cell As Short
        Public CellRunningTime As cTimecode
        Public OutOfnCells As Short
        Public Timecode As cTimecode
        Public LBTC As String
        Public tcLB As cTimecode
        Public GTTn As Short
        Public Executed As Boolean
        Public ConfirmedLayerbreak As Boolean
        Public CandidateLayerbreak As Boolean
        Public NotifyUser As Boolean
        Public SourceTimeCode As cTimecode
        Public VOBU_SA As UInt64

        Public Sub New()
            LBTC = "N/A"
        End Sub

        Public Sub New(ByVal nCell As cCellPlaybackInfo, ByRef nVTS As cVTS)
            Cell = nCell.CellNumber
            CellRunningTime = nCell.CellPlaybackTime
            OutOfnCells = nVTS.PGCs(nCell.PGCNumber - 1).CellMap.Count
            PGC = nCell.PGCNumber
            PGn = nVTS.PGCs(nCell.PGCNumber - 1).CellMap.Item(nCell.CellNumber - 1).PGNo
            OutOfnPrograms = nVTS.PGCs(nCell.PGCNumber - 1).CellMap.Item(nVTS.PGCs(nCell.PGCNumber - 1).CellMap.Count - 1).PGNo
            PTT = nVTS.PGCs(nCell.PGCNumber - 1).CellMap.Item(nCell.CellNumber - 1).PTTNoChNo
            VTS = nCell.VTSNumber
            VTS_TT = nCell.TitleNumber
            Timecode = nCell.CellPlaybackTime
            Me.VOBU_SA = nCell.C_LVOBU_SA
        End Sub

        Public Overloads Overrides Function ToString() As String
            Try
                Dim int As Short = InStr(LBTC, "/")
                int -= 2
                Dim tLBTC As String
                If int > 0 Then
                    tLBTC = Microsoft.VisualBasic.Left(LBTC, LBTC.Length - (LBTC.Length - int))
                Else
                    tLBTC = LBTC
                End If

                If VMGM Then
                    Return "[VMGM] - [LU] " & LanguageUnitName & " [PGC] " & PGC & " [PG] " & PGn & " [Cell] " & Cell & " [TC] " & tLBTC 'Replace(LBTC, "/ 30fps", "", 1, -1, CompareMethod.Text)
                ElseIf VTSM Then
                    Return "[VTS] " & VTS & " [LU] " & LanguageUnitName & " [PGC] " & PGC & " [PG] " & PGn & " [Cell] " & Cell & " [TC] " & tLBTC 'Replace(LBTC, "/ 30fps", "", 1, -1, CompareMethod.Text)
                Else
                    Return "[GTT] " & GTTn & " [VTS] " & VTS & " [VTS_TT] " & VTS_TT & " [PTT] " & PTT & " [PGC] " & PGC & " [PG] " & PGn & " [Cell] " & Cell & " [TC] " & tLBTC 'Replace(LBTC, "/ 30fps", "", 1, -1, CompareMethod.Text)
                End If

            Catch ex As Exception
                Throw New Exception("Problem with NSC to string. Error: " & ex.Message)
                Return "[GTT] " & GTTn & " [VTS] " & VTS & " [VTS_TT] " & VTS_TT & " [PTT] " & PTT & " [PGC] " & PGC & " [PG] " & PGn & " [Cell] " & Cell & " [TC] " & "TC unavailable"
            End Try
        End Function

        Public Function LBTCToString() As String
            Try
                Dim int As Short = InStr(LBTC, "/")
                int -= 2
                Dim tLBTC As String
                If int > 0 Then
                    tLBTC = Microsoft.VisualBasic.Left(LBTC, LBTC.Length - (LBTC.Length - int))
                Else
                    tLBTC = LBTC
                End If
                Return tLBTC
            Catch ex As Exception
                Throw New Exception("Problem with LBTCToString. Error: " & ex.Message)
            End Try
        End Function

        Public Function SetNSCToSeamless(ByVal IFODir As String) As Boolean
            Try

                'flip the bit in the IFO and BUP

                ' 1) open the right IFO file

                ' 2) go to right byte position

                ' 3) set the bit(s)

                Dim EBC As cEndianBitConverter = cEndianBitConverter.CreateForBigEndian

                If Me.VMGM Then

                    'todo

                ElseIf Me.VTSM Then

                    '---------------------------------------------------------------------------------
                    'VTSM
                    '---------------------------------------------------------------------------------

                    Dim FS As New FileStream(IFODir & "\VTS_" & PadString(VTS, 2, "0", True) & "_0.IFO", FileMode.Open, FileAccess.ReadWrite)
                    Dim SR As New StreamReader(FS)
                    Dim b() As Byte
                    Dim OverallOffset As UInt32
                    Dim SectorSize As Integer = 2048

                    ReDim b(3)
                    FS.Seek(208, SeekOrigin.Begin)
                    FS.Read(b, 0, 4)

                    'Get the start address of the VTSM_PGCI_UT
                    Dim VTSM_PGCI_UT_SA As Integer = SectorSize * EBC.ToUInt32(b, 0)
                    If VTSM_PGCI_UT_SA = 0 Then Exit Function 'UNEXPECTED - looking for a NSC in a non existant Language Unit is a bad idea

                    'Get the end address of the VTSM_PGCI_UT
                    ReDim b(3)
                    FS.Seek(VTSM_PGCI_UT_SA + 4, SeekOrigin.Begin)
                    FS.Read(b, 0, 4)
                    Dim VTSM_PGCI_UT_EA As UInt32 = EBC.ToUInt32(b, 0)

                    'Calculate the length of the entire VTSM_PGCI_UT
                    Dim VTSM_PGCI_UT_Length As UInt32 = VTSM_PGCI_UT_EA + 1
                    Dim VTSM_PGCI_UT_EA_Overall As UInt32 = VTSM_PGCI_UT_SA + VTSM_PGCI_UT_EA

                    'COPY THE ENTIRE VTSM_PGCI_UT into a byte array
                    Dim VTSM_PGCI_UT(VTSM_PGCI_UT_Length - 1) As Byte
                    FS.Seek(VTSM_PGCI_UT_SA, SeekOrigin.Begin)
                    FS.Read(VTSM_PGCI_UT, 0, VTSM_PGCI_UT_Length)
                    OverallOffset = VTSM_PGCI_UT_SA

                    'Now, get down to business

                    'Get Language Unit count
                    Dim LangUnitCount As UInt16 = EBC.ToUInt16(VTSM_PGCI_UT, 0)
                    If LangUnitCount = 0 Then Exit Function 'UNEXPECTED - looking for a NSC in a non existant Language Unit is a bad idea

                    'Get LU_SRPs
                    Dim LU_SRPs(LangUnitCount - 1) As cVXXM_LU_SRP
                    Dim cnt As Short = 0
                    For i As Integer = 8 To (8 * LangUnitCount) + 8 - 1 Step 8
                        LU_SRPs(cnt) = New cVXXM_LU_SRP(VTSM_PGCI_UT, i)
                        cnt += 1
                    Next

                    'Get LUs
                    Dim LU As cVXXM_LU = New cVXXM_LU(VTSM_PGCI_UT, LU_SRPs(Me.LanguageUnitNumber).VXXM_LU_SA)

                    OverallOffset += LU_SRPs(Me.LanguageUnitNumber).VXXM_LU_SA

                    OverallOffset += LU.SRPs(Me.PGC).VXXM_PGC_SA 'go to the start of the PGC

                    Dim P As cPGC = LU.VXXM_PGCs(Me.PGC)

                    Dim PG As cProgramMap = P.ProgramMap(Me.PGn - 1)

                    'Dim C As cCellPlaybackInfo = P.CellPlaybackInfo(PG.CellNo - 1) 'not required but was useful during development

                    OverallOffset += P.offCellPlaybackInfo 'go to the start of cell playback info

                    OverallOffset += (24 * (Me.Cell - 1)) 'go to the start of the desired cell, the first byte has the info (see VI4-100)

                    ReDim b(0)
                    FS.Seek(OverallOffset, SeekOrigin.Begin)
                    FS.Read(b, 0, 1)

                    ''TESTING
                    'Dim Seamless As Boolean = b(0) >> 3 And 1
                    'If Not Seamless Then
                    '    'we gocha beech!
                    '    Debug.WriteLine("gotcha")
                    'End If

                    'Dim InterleavedAllocation As Boolean = b(0) >> 2 And 1
                    'Dim STCDiscontinuity As Boolean = b(0) >> 1 And 1

                    'Dim PlaybackMode As Byte = bytes(offset + 1) >> 6 And 1
                    'Dim AccessRestriction As Byte = bytes(offset + 1) >> 5 And 1
                    'Dim Type As Byte = bytes(offset) And 31
                    'Dim StillTime As Byte = bytes(offset + 2)
                    'Dim CellPlaybackTime As Byte = New cTimecode(bytes, offset + 4)
                    'END TESTING

                    Dim newbyte As Byte = b(0) Or 8

                    'TESTING
                    'If newbyte >> 3 And 1 Then
                    '    Debug.WriteLine("hi")
                    '    'it is now seamless!
                    'End If
                    'TESTING

                    FS.Seek(OverallOffset, SeekOrigin.Begin)
                    Dim oldbyte As Byte = FS.ReadByte()
                    FS.Seek(OverallOffset, SeekOrigin.Begin)
                    FS.WriteByte(newbyte)

                    'NOW DO BUP
                    Dim FS2 As New FileStream(IFODir & "\VTS_" & PadString(Me.VTS, 2, "0", True) & "_0.BUP", FileMode.Open, FileAccess.ReadWrite)
                    FS2.Seek(OverallOffset, SeekOrigin.Begin)
                    Dim oldbyteBUP As Byte = FS2.ReadByte
                    If oldbyteBUP <> oldbyte Then
                        Throw New Exception("Byte in BUP does not match byte in IFO. BUP was not be edited.")
                    End If
                    FS2.Seek(OverallOffset, SeekOrigin.Begin)
                    FS2.WriteByte(newbyte)
                    FS2.Close()
                    FS2 = Nothing
                    'DONE WITH BUP

                    FS.Close()
                    FS = Nothing

                Else

                    '---------------------------------------------------------------------------------
                    'VTS
                    '---------------------------------------------------------------------------------

                    Dim FS As New FileStream(IFODir & "\VTS_" & PadString(Me.VTS, 2, "0", True) & "_0.IFO", FileMode.Open, FileAccess.ReadWrite)
                    Dim SR As New StreamReader(FS)
                    Dim b() As Byte
                    Dim SectorSize As Integer = 2048

                    ReDim b(3)
                    FS.Seek(204, SeekOrigin.Begin)
                    FS.Read(b, 0, 4)

                    Dim VTS_PGCIT_SectorPointer As Integer = SectorSize * EBC.ToUInt32(b, 0)

                    ReDim b(1)
                    FS.Seek(VTS_PGCIT_SectorPointer, SeekOrigin.Begin)
                    FS.Read(b, 0, 2)
                    Dim PGCCount_TitleSpace As UInt16 = EBC.ToUInt16(b, 0)

                    Dim VTS_PGC_Offsets(PGCCount_TitleSpace - 1) As Long

                    Dim StartOfVTS_PGCI As Short = VTS_PGCIT_SectorPointer

                    Dim Plus12 As Short = StartOfVTS_PGCI + 12

                    ReDim b(3)
                    For i As Short = 0 To PGCCount_TitleSpace - 1
                        FS.Seek(Plus12 + (i * 8), SeekOrigin.Begin)
                        FS.Read(b, 0, 4)
                        VTS_PGC_Offsets(i) = EBC.ToUInt32(b, 0)
                    Next

                    '---------------------------------------------------------------------------------
                    'END VTS
                    '---------------------------------------------------------------------------------

                    '---------------------------------------------------------------------------------
                    'PGCs
                    '---------------------------------------------------------------------------------

                    Dim offset1 As Integer = VTS_PGC_Offsets(Me.PGC - 1) + StartOfVTS_PGCI

                    ReDim b(0)
                    FS.Seek(offset1 + 3, SeekOrigin.Begin)
                    FS.Read(b, 0, 1)
                    Dim CellCount As Integer = b(0)

                    'C_PBI
                    If CellCount > 0 Then

                        ReDim b(1)
                        FS.Seek(offset1 + 232, SeekOrigin.Begin)
                        FS.Read(b, 0, 2)
                        Dim offCellPlaybackInfo As UShort = EBC.ToUInt16(b, 0)

                        Dim offset2 As Integer = offset1 + offCellPlaybackInfo
                        offset2 = offset2 + (24 * (Me.Cell - 1))

                        ReDim b(0)
                        FS.Seek(offset2, SeekOrigin.Begin)
                        FS.Read(b, 0, 1)

                        'TESTING
                        'Dim Seamless As Boolean = b(0) >> 3 And 1
                        'If Not Seamless Then
                        '    'we gocha beech!
                        'End If
                        'TESTING

                        Dim newbyte As Byte = b(0) Or 8

                        'TESTING
                        'If newbyte >> 3 And 1 Then
                        '    Debug.WriteLine("hi")
                        '    'it is now seamless!
                        'End If

                        'Dim InterleavedAllocation As Boolean = b(0) >> 2 And 1
                        'Dim STCDiscontinuity As Boolean = b(0) >> 1 And 1

                        'PlaybackMode = bytes(offset + 1) >> 6 And 1
                        'AccessRestriction = bytes(offset + 1) >> 5 And 1
                        'Type = bytes(offset) And 31
                        'StillTime = bytes(offset + 2)
                        'CellPlaybackTime = New cTimecode(bytes, offset + 4)

                        'TESTING

                        FS.Seek(offset2, SeekOrigin.Begin)
                        Dim oldbyte As Byte = FS.ReadByte()
                        FS.Seek(offset2, SeekOrigin.Begin)
                        FS.WriteByte(newbyte)

                        'NOW DO BUP
                        Dim FS2 As New FileStream(IFODir & "\VTS_" & PadString(Me.VTS, 2, "0", True) & "_0.BUP", FileMode.Open, FileAccess.ReadWrite)
                        FS2.Seek(offset2, SeekOrigin.Begin)
                        Dim oldbyteBUP As Byte = FS2.ReadByte
                        If oldbyteBUP <> oldbyte Then
                            Throw New Exception("Byte in BUP does not match byte in IFO. BUP was not be edited.")
                        End If
                        FS2.Seek(offset2, SeekOrigin.Begin)
                        FS2.WriteByte(newbyte)
                        FS2.Close()
                        FS2 = Nothing
                        'DONE WITH BUP

                    End If


                    '---------------------------------------------------------------------------------
                    'END PGCs
                    '---------------------------------------------------------------------------------

                    FS.Close()
                    FS.Dispose()
                    FS = Nothing

                End If

                Return True

            Catch ex As Exception
                Throw New Exception("Problem with SetNSCToSeamless(). Error: " & ex.Message, ex)
            End Try
        End Function

    End Class

    Public Class colNonSeamlessCells
        Inherits CollectionBase

        Public LayerbreakConfirmed As Boolean
        Public CandidateLayerbreaks As Short

        Public ReadOnly Property ConfirmedLayerbreak() As cNonSeamlessCell
            Get
                If Not LayerbreakConfirmed Then Return Nothing
                For Each NSC As cNonSeamlessCell In Me
                    If NSC.ConfirmedLayerbreak Then Return NSC
                Next
            End Get
        End Property

        Public Function Add(ByVal newLB As cNonSeamlessCell) As Integer
            Return MyBase.List.Add(newLB)
        End Function

        Default Public ReadOnly Property Item(ByVal index As Integer) As cNonSeamlessCell
            Get
                Return MyBase.List.Item(index)
            End Get
        End Property

        Default Public ReadOnly Property Item(ByVal nItem As cNonSeamlessCell) As Integer
            Get
                Return MyBase.List.IndexOf(nItem)
            End Get
        End Property

        Public Sub Remove(ByVal Item As cNonSeamlessCell)
            MyBase.List.Remove(Item)
        End Sub

        Public Function MarkCandidateLayerbreaks() As Boolean
            Try
                CandidateLayerbreaks = 0
                For i As Short = 0 To Me.Count - 1
                    Me.Item(i).CandidateLayerbreak = False
                    If Me.Item(i).VMGM Then GoTo NextNSC
                    If Me.Item(i).VTSM Then GoTo NextNSC
                    If Me.Item(i).PTT = 1 Then GoTo NextNSC
                    If Me.Item(i).Cell = 1 Then GoTo NextNSC
                    If Me.Item(i).PGn = 1 Then GoTo NextNSC
                    If Me.Item(i).Cell = Me.Item(i).OutOfnCells Then GoTo NextNSC
                    If Me.Item(i).PGn = Me.Item(i).OutOfnPrograms Then GoTo NextNSC

                    'Ivan's idea
                    'Debug.WriteLine("TotalSeconds=" & Item(i).CellRunningTime.TotalSeconds)
                    If (Item(i).Cell = Item(i).OutOfnCells) And (Item(i).CellRunningTime.TotalSeconds = 0) Then
                        'Debug.WriteLine("Ivan's LB catch")
                        GoTo NextNSC
                    End If

                    'If (Me.Item(i).tcLB.Hours * 60) + Me.Item(i).tcLB.Minutes < 5 Then GoTo NextNSC 'the logic here being that a layerbreak is never in the first five minutes of a PGC
                    Me.Item(i).CandidateLayerbreak = True
                    Me.CandidateLayerbreaks += 1
NextNSC:
                Next

                'If Me.CandidateLayerbreaks = 1 Then
                '    Me.LayerbreakConfirmed = True
                '    For Each LB As cNonSeamlessCell In Me
                '        If LB.CandidateLayerbreak Then
                '            LB.ConfirmedLayerbreak = True
                '            Exit For
                '        End If
                '    Next
                'Else
                '    Me.LayerbreakConfirmed = False
                'End If

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with MarkCandidateLayerbreaks(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Sub MarkNSCsForNotification()
            Try
                For i As Integer = 0 To Me.Count - 1
                    If Me.Item(i).Cell = 1 Then GoTo NextNSC
                    If Me.Item(i).PGn = 1 Then GoTo NextNSC
                    If Me.Item(i).Cell = Me.Item(i).OutOfnCells Then GoTo NextNSC
                    If Me.Item(i).PGn = Me.Item(i).OutOfnPrograms Then GoTo NextNSC
                    Me.Item(i).NotifyUser = True
NextNSC:
                Next
            Catch ex As Exception
                Throw New Exception("Problem with MarkNSCsForNotification(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Function CheckNotify(ByVal T As Short, ByVal C As Short, ByVal H As Byte, ByVal M As Byte, ByVal S As Byte, ByVal IsNTSC As Boolean) As Boolean
            Try
                For i As Short = 0 To Me.Count - 1
                    If Not Me.Item(i).NotifyUser Then GoTo NextNSC

                    If Me.Item(i).VMGM Then GoTo NextNSC
                    If Me.Item(i).VTSM Then GoTo NextNSC
                    If Me.Item(i).GTTn <> T Then GoTo NextNSC
                    If (Me.Item(i).PTT > (C - 2)) And (Me.Item(i).PTT < (C)) Then GoTo NextNSC

                    Dim SecDiff As Int64 = Me.Item(i).tcLB.TotalSeconds
                    SecDiff -= New cTimecode(H, M, S, 0, IsNTSC).TotalSeconds
                    If Math.Abs(SecDiff) < 3 Then
                        Return True
                    End If

                    'If Not Me.Item(i).tcLB.Hours = H Then GoTo NextNSC
                    'If Not Me.Item(i).tcLB.Minutes = M Then GoTo NextNSC

                    'If (S > (Me.Item(i).tcLB.Seconds - 3)) And (S < (Me.Item(i).tcLB.Seconds + 1)) Then
                    '    Return True
                    'End If
NextNSC:
                Next
                Return False
            Catch ex As Exception
                Throw New Exception("Problem with CheckNotify(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Sub New(ByRef DVD As cDVD)
            Try
                Dim tPGCn As Short
                Dim tPGn As Short
                Dim tNSC As cNonSeamlessCell
                Dim CellNos() As Short
                Dim tNSCID As Short = 0
                Dim LUCnt As Short


                'VMGM
                'MENU SPACE - added 071107
                If DVD.VMGM.VMGM_PGCI_UT.LUs Is Nothing Then GoTo VTSs
                LUCnt = 0
                For Each LU As cVXXM_LU In DVD.VMGM.VMGM_PGCI_UT.LUs

                    For Each p As cPGC In LU.VXXM_PGCs
                        tPGCn = p.PGCNumber - 1


                        For ci As Integer = 0 To p.CellCount - 1
                            If p.CellPlaybackInfo(ci).StillTime = "0" Then
                                If Not p.CellPlaybackInfo(ci).Seamless Then

                                    tNSC = New cNonSeamlessCell
                                    tNSC.VMGM = True
                                    tNSC.Cell = ci + 1
                                    tNSC.OutOfnCells = p.CellMap.Count
                                    tNSC.PGC = p.PGCNumber
                                    tNSC.PGn = p.CellMap.Item(ci).PGNo
                                    tNSC.OutOfnPrograms = p.ProgramCount
                                    tNSC.Timecode = p.CellPlaybackInfo(ci).CellPlaybackTime
                                    tNSC.LanguageUnitNumber = LUCnt
                                    tNSC.LanguageUnitName = DVD.VMGM.VMGM_PGCI_UT.LU_SRPs(LUCnt).VXXM_LCD.ToString()

                                    tNSC.tcLB = DVD.GetTRTFromCellColl(tNSC.Cell, p.CellPlaybackInfo)
                                    tNSC.LBTC = tNSC.tcLB.ToString
                                    tNSC.ID = tNSCID
                                    tNSCID += 1
                                    Me.Add(tNSC)

                                End If
                            End If
                        Next

                    Next
                    LUCnt += 1
                Next

VTSs:

                'EACH VTS
                For i As Short = 0 To DVD.VTSs.Length - 1
                    'Debug.WriteLine("Now parsing " & IFOs(i).Name & " for non-seamless cells.")

                    'MENU SPACE - added 071107
                    If DVD.VTSs(i).VTSM_PGCI_UT.LUs Is Nothing Then GoTo TitleSpace
                    LUCnt = 0
                    For Each LU As cVXXM_LU In DVD.VTSs(i).VTSM_PGCI_UT.LUs

                        For Each p As cPGC In LU.VXXM_PGCs
                            tPGCn = p.PGCNumber - 1

                            For ci As Integer = 0 To p.CellCount - 1
                                If p.CellPlaybackInfo(ci).StillTime = "0" Then
                                    If Not p.CellPlaybackInfo(ci).Seamless Then

                                        tNSC = New cNonSeamlessCell
                                        tNSC.VTSM = True
                                        tNSC.Cell = ci + 1
                                        tNSC.OutOfnCells = p.CellMap.Count
                                        tNSC.PGC = p.PGCNumber
                                        tNSC.PGn = p.CellMap.Item(ci).PGNo
                                        tNSC.OutOfnPrograms = p.ProgramCount
                                        tNSC.VTS = i + 1
                                        tNSC.Timecode = p.CellPlaybackInfo(ci).CellPlaybackTime
                                        tNSC.LanguageUnitNumber = LUCnt
                                        tNSC.LanguageUnitName = DVD.VTSs(i).VTSM_PGCI_UT.LU_SRPs(LUCnt).VXXM_LCD.ToString()

                                        tNSC.tcLB = DVD.GetTRTFromCellColl(tNSC.Cell, p.CellPlaybackInfo)
                                        tNSC.LBTC = tNSC.tcLB.ToString
                                        tNSC.ID = tNSCID
                                        tNSCID += 1
                                        Me.Add(tNSC)

                                    End If
                                End If
                            Next
                        Next
                        LUCnt += 1
                    Next

TitleSpace:
                    'EACH VTS_TT
                    'Each Title in IFO
                    For TT As Short = 0 To DVD.VTSs(i).Titles.Count - 1

                        'Each Chapter in IFO
                        For PTT As Short = 0 To DVD.VTSs(i).Titles(TT).PTTs.Count - 1
                            tPGCn = DVD.VTSs(i).Titles(TT).PTTs(PTT).PGCN - 1
                            tPGn = DVD.VTSs(i).Titles(TT).PTTs(PTT).PGN - 1

                            'Here we get the cell nos used in the current program so we
                            'can check each individually to see if it's nonseamless
                            CellNos = LookupCellNo(tPGn + 1, DVD.VTSs(i).PGCs(tPGCn))
                            For c As Short = LBound(CellNos) To UBound(CellNos)
                                If DVD.VTSs(i).PGCs(tPGCn).CellPlaybackInfo(CellNos(c) - 1).StillTime = "0" Then
                                    If Not DVD.VTSs(i).PGCs(tPGCn).CellPlaybackInfo(CellNos(c) - 1).Seamless Then
                                        DVD.VTSs(i).PGCs(tPGCn).CellPlaybackInfo(CellNos(c) - 1).TitleNumber = TT + 1
                                        tNSC = New cNonSeamlessCell(DVD.VTSs(i).PGCs(tPGCn).CellPlaybackInfo(CellNos(c) - 1), DVD.VTSs(i))
                                        tNSC.tcLB = DVD.GetTRTFromCellColl(tNSC.Cell, DVD.VTSs(i).PGCs(tPGCn).CellPlaybackInfo)
                                        tNSC.LBTC = tNSC.tcLB.ToString
                                        tNSC.ID = tNSCID
                                        tNSCID += 1
                                        Me.Add(tNSC)
                                    End If
                                End If
                            Next
                        Next
                    Next
                Next

                'Get GlobalTT Numbers for the layerbreaks
                For Each L As cNonSeamlessCell In Me
                    For Each GTT As cGlobalTT In DVD.VMGM.GlobalTTs
                        If L.VTS = GTT.VTSN And L.VTS_TT = GTT.VTS_TTN Then
                            L.GTTn = GTT.GlobalTT_N
                            GoTo NextLB
                        End If
                    Next
NextLB:
                Next

                MarkCandidateLayerbreaks()
                MarkNSCsForNotification()

            Catch ex As Exception
                Throw New Exception("Problem with New() colNonSeamlessCells. Error: " & ex.Message, ex)
            End Try
        End Sub

    End Class

#End Region    'Layerbreaks

#End Region 'Classes

#Region "Support Code"

    Public Module mSharedHelperFunctions

        Public Function AddTimecode(ByVal TC1 As cTimecode, ByVal TC2 As cTimecode) As cTimecode
            Dim Out As New cTimecode
            Out.Framerate = TC1.Framerate

            Out.Frames = TC1.Frames + TC2.Frames
            If Out.Frames > 29 Then
                Out.Seconds = 1
                Out.Frames -= 30
            End If

            Out.Seconds += TC1.Seconds + TC2.Seconds
            If Out.Seconds > 59 Then
                Out.Minutes = 1
                Out.Seconds -= 60
            End If

            Out.Minutes += TC1.Minutes + TC2.Minutes
            If Out.Minutes > 59 Then
                Out.Hours = 1
                Out.Minutes -= 60
            End If

            Out.Hours += TC1.Hours + TC2.Hours

            Return Out
        End Function

        Public Function IsProjectDualLayer(ByVal DVDDirectoryPath As String) As Boolean
            Try
                Dim TotalSize As Long = 0
                Dim FSE() As String = Directory.GetFiles(DVDDirectoryPath)

                For Each F As String In FSE
                    TotalSize += FileLen(F)
                Next

                If TotalSize > 5051158528 Then
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                Throw New Exception("Problem with IsProjectDualLayer. Error: " & ex.Message, ex)
                'RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with IsProjectDualLayer. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

        Public Function GetStringFromBytes(ByRef Bytes() As Byte, ByVal Offset As Integer, ByVal Length As Integer) As String
            Try
                Dim tBuf(Length - 1) As Byte
                Array.Copy(Bytes, Offset, tBuf, 0, Length)
                tBuf = RemoveExtraBytesFromArray(tBuf, False)
                Dim Out As String = System.Text.Encoding.ASCII.GetString(tBuf)
                If Out.ToString.Length = 0 Then Return ""
                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with GetStringFromBytes(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function GetByte(ByVal InByte As Byte) As String
            Try
                Dim O As String = Hex(InByte)
                If O.Length = 1 Then O = "0" & O
                Return O
            Catch ex As Exception
                Throw New Exception("Problem with GetByte(). Error: " & ex.Message, ex)
            End Try
        End Function

        'Public Class cSMTBitshifter
        '    'designed only for upto four bytes
        '    Public Val As Integer

        '    Public Sub New(ByVal ValCnt As Integer, ByVal v1 As Integer, ByVal v2 As Integer, Optional ByVal v3 As Integer = 0, Optional ByVal v4 As Integer = 0)
        '        SetVal(ValCnt, v1, v2, v3, v4)
        '    End Sub

        '    Public Sub New()
        '    End Sub

        '    Public Function SetVal(ByVal ValCnt As Short, ByVal v1 As Integer, ByVal v2 As Integer, Optional ByVal v3 As Integer = 0, Optional ByVal v4 As Integer = 0) As Integer
        '        Try
        '            Val = 0
        '            ValCnt -= 1
        '            Val = v1 << (ValCnt * 8)
        '            If ValCnt > 0 Then
        '                Val = Val Or (v2 << ((ValCnt - 1) * 8))
        '            End If
        '            If ValCnt > 1 Then
        '                Val = Val Or (v3 << ((ValCnt - 2) * 8))
        '            End If
        '            If ValCnt > 2 Then
        '                Val = Val Or (v4 << ((ValCnt - 3) * 8))
        '            End If
        '            Return Val
        '        Catch ex As Exception
        '            Throw New Exception("Problem with SetVal. Error: " & ex.Message)
        '        End Try
        '    End Function

        'End Class

        Public Function LookupCellNo(ByVal PGn As Short, ByVal PGC As cPGC) As Short()
            Try
                Dim out(-1) As Short
                For Each CMI As cCellMapItem In PGC.CellMap
                    If CMI.PGNo = PGn Then
                        ReDim Preserve out(UBound(out) + 1)
                        out(UBound(out)) = CMI.CellNo
                    End If
                Next
                Return out
            Catch ex As Exception
                Throw New Exception("Problem with LookupCellNo. Error: " & ex.Message, ex)
            End Try
        End Function

    End Module

    Public Class cDVDLocation

        Public VMGM As Boolean
        Public VTS As Byte
        Public MenuSpace As Boolean
        Public LanguageUnit As Byte
        Public LanguageUnitString As String
        Public PGCN As Byte
        Public GTTN As Byte

        Public Overrides Function ToString() As String
            If VMGM Then
                Return "VMGM Language Unit " & LanguageUnitString & " PGC " & PGCN + 1
            Else
                If Me.MenuSpace Then
                    Return " VTS " & VTS + 1 & " Menu Language Unit " & LanguageUnitString & " PGCN " & PGCN + 1
                Else
                    Return "Global Title " & GTTN + 1 & " VTS " & VTS + 1 & " PGCN " & PGCN + 1
                End If
            End If
        End Function

    End Class

#End Region 'Support Code

End Namespace 'Media.DVD.IFOProcessing
