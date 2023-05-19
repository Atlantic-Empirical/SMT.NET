Imports System.IO
Imports Microsoft.VisualBasic
Imports SMT.Media.FormatParsing.DVD.Media.Utilities.TimecodeMath
Imports SMT.Media.FormatParsing.DVD.Utilities.ConversionsAndSuch
Imports System.Text
Imports System.Collections.Specialized

Namespace Media.DVD.IFOProcessing

#Region "Classes"

#Region "DVD"

    Public Class cDVD

        Public VMGM As cVMGM
        Public VTSs() As cVTS
        Public DVDPath As String

        Public Sub New(ByVal nPath As String) 'needs video_ts directory
            Try
                DVDPath = nPath
                VMGM = New cVMGM(nPath & "\video_ts.ifo")
                ReDim VTSs(-1)
                For i As Short = 1 To VMGM.NumberOfTitleSets
                    ReDim Preserve VTSs(i - 1)
                    VTSs(i - 1) = New cVTS(nPath & "\VTS_" & PadString(i, 2, "0", True) & "_0.IFO", i)
                Next
            Catch ex As Exception
                Throw New IFOParseException("Problem with New cDVD. Error: " & ex.Message, ex)
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

        Public Function GetNonSeamlessCells() As colNonSeamlessCells
            Try
                Dim Out As New colNonSeamlessCells
                Dim tPGCn As Short
                Dim tPGn As Short
                Dim tNSC As cNonSeamlessCell
                Dim CellNos() As Short
                Dim tNSCID As Short = 0
                'Each IFO (VTS or VMGM)
                For i As Short = 0 To VTSs.Length - 1
                    'Debug.WriteLine("Now parsing " & IFOs(i).Name & " for non-seamless cells.")

                    'Each Title in IFO
                    For TT As Short = 0 To VTSs(i).Titles.Count - 1

                        'Each Chapter in IFO
                        For PTT As Short = 0 To VTSs(i).Titles(TT).PTTs.Count - 1
                            tPGCn = VTSs(i).Titles(TT).PTTs(PTT).PGCN - 1
                            tPGn = VTSs(i).Titles(TT).PTTs(PTT).PGN - 1

                            'Here we get the cell nos used in the current program so we
                            'can check each individually to see if it's N-S
                            CellNos = LookupCellNo(tPGn + 1, VTSs(i).PGCs(tPGCn))
                            For c As Short = LBound(CellNos) To UBound(CellNos)
                                If VTSs(i).PGCs(tPGCn).CellPlaybackInfo(CellNos(c) - 1).StillTime = "0" Then
                                    If Not VTSs(i).PGCs(tPGCn).CellPlaybackInfo(CellNos(c) - 1).Seamless Then
                                        VTSs(i).PGCs(tPGCn).CellPlaybackInfo(CellNos(c) - 1).TitleNumber = TT + 1
                                        tNSC = ConvertCellTocNonSeamless(VTSs(i).PGCs(tPGCn).CellPlaybackInfo(CellNos(c) - 1), VTSs(i))
                                        tNSC.tcLB = GetLBTCFromCellColl(tNSC.Cell, VTSs(i).PGCs(tPGCn).CellPlaybackInfo)
                                        tNSC.LBTC = tNSC.tcLB.ToString
                                        tNSC.ID = tNSCID
                                        tNSCID += 1
                                        Out.Add(tNSC)
                                    End If
                                End If
                            Next
                        Next
                    Next
                Next

                'Get GlobalTT Numbers for the layerbreaks
                For Each L As cNonSeamlessCell In Out
                    For Each GTT As cGlobalTT In VMGM.GlobalTTs
                        If L.VTS = GTT.VTSN And L.VTS_TT = GTT.VTS_TTN Then
                            L.GTTn = GTT.GlobalTT_N
                            GoTo NextLB
                        End If
                    Next
NextLB:
                Next

                MarkCandidateLayerbreaks(Out)
                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with GetNonSeamlessCells(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function MarkCandidateLayerbreaks(ByRef NSCs As colNonSeamlessCells) As Boolean
            Try
                NSCs.CandidateLayerbreaks = 0
                For i As Short = 0 To NSCs.Count - 1
                    NSCs(i).CandidateLayerbreak = False
                    If NSCs(i).PTT = 1 Then GoTo NextNSC
                    If NSCs(i).Cell = 1 Then GoTo NextNSC
                    If NSCs(i).PGn = 1 Then GoTo NextNSC
                    If NSCs(i).Cell = NSCs(i).OutOfnCells Then GoTo NextNSC
                    If NSCs(i).PGn = NSCs(i).OutOfnPrograms Then GoTo NextNSC
                    If (NSCs(i).tcLB.Hours * 60) + NSCs(i).tcLB.Minutes < 5 Then GoTo NextNSC 'the logic here being that a layerbreak is never in the first five minutes of a PGC
                    NSCs(i).CandidateLayerbreak = True
                    NSCs.CandidateLayerbreaks += 1
NextNSC:
                Next

                If NSCs.CandidateLayerbreaks = 1 Then
                    NSCs.LayerbreakConfirmed = True
                    For Each LB As cNonSeamlessCell In NSCs
                        If LB.CandidateLayerbreak Then
                            LB.ConfirmedLayerbreak = True
                            Exit For
                        End If
                    Next
                Else
                    NSCs.LayerbreakConfirmed = False
                End If
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with MarkCandidateLayerbreaks(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function GetLBTCFromCellColl(ByVal LBCell As Short, ByVal Cells As colCellPlaybackInfo) As cTimecode
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

        Public Function SetNSCToSeamless(ByVal NSC As cNonSeamlessCell, ByVal IFODir As String) As Boolean
            Try

                'flip the bit in the IFO and BUP

                ' 1) open the right IFO file

                ' 2) go to right byte position

                ' 3) set the bit(s)

                Dim FS As New FileStream(IFODir & "\VTS_" & PadString(NSC.VTS, 2, "0", True) & "_0.IFO", FileMode.Open, FileAccess.ReadWrite)
                Dim SR As New StreamReader(FS)
                Dim b() As Byte
                Dim SectorSize As Integer = 2048

                '---------------------------------------------------------------------------------
                'VTS
                '---------------------------------------------------------------------------------

                ReDim b(3)
                FS.Seek(204, SeekOrigin.Begin)
                FS.Read(b, 0, 4)
                Array.Reverse(b)

                Dim VTS_PGCIT_SectorPointer As Integer = SectorSize * BitConverter.ToUInt32(b, 0)

                ReDim b(1)
                FS.Seek(VTS_PGCIT_SectorPointer, SeekOrigin.Begin)
                FS.Read(b, 0, 2)
                Array.Reverse(b)

                Dim PGCCount_TitleSpace As Integer = BitConverter.ToUInt16(b, 0)

                Dim VTS_PGC_Offsets(PGCCount_TitleSpace - 1) As Long

                Dim StartOfVTS_PGCI As Short = VTS_PGCIT_SectorPointer

                Dim Plus12 As Short = StartOfVTS_PGCI + 12

                ReDim b(3)
                For i As Short = 0 To PGCCount_TitleSpace - 1
                    FS.Seek(Plus12 + (i * 8), SeekOrigin.Begin)
                    FS.Read(b, 0, 4)
                    Array.Reverse(b)
                    VTS_PGC_Offsets(i) = BitConverter.ToUInt32(b, 0)
                Next
                Debug.WriteLine("hi")

                '---------------------------------------------------------------------------------
                'END VTS
                '---------------------------------------------------------------------------------

                '---------------------------------------------------------------------------------
                'PGCs
                '---------------------------------------------------------------------------------

                Dim offset1 As Integer = VTS_PGC_Offsets(NSC.PGC - 1) + StartOfVTS_PGCI

                ReDim b(0)
                FS.Seek(offset1 + 3, SeekOrigin.Begin)
                FS.Read(b, 0, 1)
                Dim CellCount As Integer = b(0)

                'C_PBI
                If CellCount > 0 Then

                    ReDim b(1)
                    FS.Seek(offset1 + 232, SeekOrigin.Begin)
                    FS.Read(b, 0, 2)
                    Array.Reverse(b)
                    Dim offCellPlaybackInfo As UShort = BitConverter.ToUInt16(b, 0)

                    Dim offset2 As Integer = offset1 + offCellPlaybackInfo
                    offset2 = offset2 + (24 * (NSC.Cell - 1))

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
                    'TESTING

                    FS.Seek(offset2, SeekOrigin.Begin)
                    FS.WriteByte(newbyte)

                    'Dim InterleavedAllocation As Boolean = b(0) >> 2 And 1
                    'Dim STCDiscontinuity As Boolean = b(0) >> 1 And 1

                    'PlaybackMode = bytes(offset + 1) >> 6 And 1
                    'AccessRestriction = bytes(offset + 1) >> 5 And 1
                    'Type = bytes(offset) And 31
                    'StillTime = bytes(offset + 2)
                    'CellPlaybackTime = New cTimecode(bytes, offset + 4)

                End If


                '---------------------------------------------------------------------------------
                'END PGCs
                '---------------------------------------------------------------------------------

                FS.Close()
                FS.Dispose()
                FS = Nothing

                Return True

            Catch ex As Exception
                Throw New Exception("Problem with SetNSCToSeamless(). Error: " & ex.Message, ex)
            End Try
        End Function

    End Class

#End Region 'DVD

#Region "VTS"

    Public Class cVTS

        'Public VTSIFOPath As String
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

        Public TitleSetLastSector As Long
        Public IFOLastSector As Long
        Public Version As String
        Public VTSCategory As String
        Public VTS_MATEndByteAddress As Long
        Public MenuVOBStartSector As Long
        Public TitleVOBStartSector As Long

        Public VTS_PTT_SRPT_SectorPointer As Long '(table of Titles and Chapters)
        Public VTS_PGCIT_SectorPointer As Long '(Title Program Chain table)
        Public VTSM_PGCI_UT_SectorPointer As Long '(Menu Program Chain table)
        Public VTS_TMAPTI_SectorPointer As Long '(time map)
        Public VTSM_C_ADT_SectorPointer As Long '(menu cell address table)
        Public VTSM_VOBU_ADMAP_SectorPointer As Long '(menu VOBU address map)
        Public VTS_C_ADT_SectorPointer As Long '(title set cell address table)
        Public VTS_VOBU_ADMAP_SectorPointer As Long '(title set VOBU address map)

        Public VTSM_VideoAttributes As cVideoAttributes
        Public VTSM_AudioStreamCount As Short
        Public VTSM_AudioAttributes As cMenuAudioAttributes
        Public VTSM_SubpictureStreamCount As Short
        Public VTSM_SubpictureAttributes As cSubpictureAttributes

        Public VTS_VideoAttributes As cVideoAttributes
        Public VTS_AudioStreamCount As Short
        Public VTS_AudioAttributes As cTitleAudioAttributes
        Public VTS_SubpictureStreamCount As Short
        Public VTS_SubpictureAttributes As cSubpictureAttributes

        'Public MultichannelExtension As cMultichannelExtension

        Public TT_EndOfPTTTable As Integer
        Public TT_StartBytes() As Integer

        Public VTSM_PGCI_UT As cVXXM_PGCI_UT
        Public VTS_PGCIT As cVTS_PGCIT

        Public Sub New(ByVal nVTSIFOPath As String, ByVal VTSNumber As Integer)
            Me.New(New FileStream(nVTSIFOPath, FileMode.Open, FileAccess.Read), VTSNumber)
            'Try
            '    If Not File.Exists(nVTSIFOPath) Then Throw New Exception("IFO file does not exist:" & nVTSIFOPath)

            '    VTSIFOPath = nVTSIFOPath
            '    Name = Path.GetFileName(VTSIFOPath)
            '    Titles = New colTitles
            '    PGCs = New colPGCs

            '    Dim IFO() As Byte
            '    Dim B1, B2, B3, B4 As String
            '    Dim FS As New FileStream(nVTSIFOPath, FileMode.Open, FileAccess.Read)
            '    ReDim IFO(FS.Length - 1)
            '    FS.Read(IFO, 0, FS.Length - 1)
            '    FS.Close()
            '    FS = Nothing

            '    Dim tStrA() As String

            '    '---------------------------------------------------------------------------------
            '    'VTS
            '    '---------------------------------------------------------------------------------

            '    tStrA = Split(Path.GetFileNameWithoutExtension(nVTSIFOPath), "_", -1, CompareMethod.Text)
            '    ID = tStrA(1)
            '    TitleSetLastSector = CInt("&h" & GetByte(IFO(12)) & GetByte(IFO(13)) & GetByte(IFO(14)) & GetByte(IFO(15)))
            '    IFOLastSector = CInt("&h" & GetByte(IFO(16)) & GetByte(IFO(17)) & GetByte(IFO(18)) & GetByte(IFO(19)))
            '    Version = "v" & CInt(Microsoft.VisualBasic.Left(GetByte(IFO(33)), 1)) & "." & CInt(Microsoft.VisualBasic.Right(GetByte(IFO(33)), 1))
            '    VTSCategory = CInt("&h" & GetByte(IFO(34)) & GetByte(IFO(35)) & GetByte(IFO(36)) & GetByte(IFO(37)))
            '    If VTSCategory = 0 Then
            '        VTSCategory = "Unspecified"
            '    Else
            '        VTSCategory = "Karaoke"
            '    End If
            '    VTS_MATEndByteAddress = CInt("&h" & GetByte(IFO(128)) & GetByte(IFO(129)) & GetByte(IFO(130)) & GetByte(IFO(131)))
            '    MenuVOBStartSector = CInt("&h" & GetByte(IFO(192)) & GetByte(IFO(193)) & GetByte(IFO(194)) & GetByte(IFO(195)))
            '    TitleVOBStartSector = CInt("&h" & GetByte(IFO(196)) & GetByte(IFO(197)) & GetByte(IFO(198)) & GetByte(IFO(199)))
            '    VTS_PTT_SRPT_SectorPointer = SectorSize * CInt("&h" & GetByte(IFO(200)) & GetByte(IFO(201)) & GetByte(IFO(202)) & GetByte(IFO(203)))
            '    VTS_PGCIT_SectorPointer = SectorSize * CInt("&h" & GetByte(IFO(204)) & GetByte(IFO(205)) & GetByte(IFO(206)) & GetByte(IFO(207)))
            '    VTSM_PGCI_UT_SectorPointer = SectorSize * CInt("&h" & GetByte(IFO(208)) & GetByte(IFO(209)) & GetByte(IFO(210)) & GetByte(IFO(211)))
            '    VTS_TMAPTI_SectorPointer = SectorSize * CInt("&h" & GetByte(IFO(212)) & GetByte(IFO(213)) & GetByte(IFO(214)) & GetByte(IFO(215)))
            '    VTSM_C_ADT_SectorPointer = SectorSize * CInt("&h" & GetByte(IFO(216)) & GetByte(IFO(217)) & GetByte(IFO(218)) & GetByte(IFO(219)))
            '    VTSM_VOBU_ADMAP_SectorPointer = SectorSize * CInt("&h" & GetByte(IFO(220)) & GetByte(IFO(221)) & GetByte(IFO(222)) & GetByte(IFO(223)))
            '    VTS_C_ADT_SectorPointer = SectorSize * CInt("&h" & GetByte(IFO(224)) & GetByte(IFO(225)) & GetByte(IFO(226)) & GetByte(IFO(227)))
            '    VTS_VOBU_ADMAP_SectorPointer = SectorSize * CInt("&h" & GetByte(IFO(228)) & GetByte(IFO(229)) & GetByte(IFO(230)) & GetByte(IFO(231)))

            '    VTSM_PGCI_UT = New cVXXM_PGCI_UT(IFO, VTSM_PGCI_UT_SectorPointer)
            '    VTS_PGCIT = New cVTS_PGCIT(IFO, VTS_PGCIT_SectorPointer)

            '    Dim tByteA() As Byte

            '    ReDim tByteA(1)
            '    System.Array.Copy(IFO, 256, tByteA, 0, 2)
            '    VTSM_VideoAttributes = GetVideoAttsFromBytes(tByteA)
            '    VTSM_AudioStreamCount = CInt("&h" & GetByte(IFO(258)) & GetByte(IFO(259)))

            '    ReDim tByteA(63)
            '    System.Array.Copy(IFO, 260, tByteA, 0, 64)
            '    VTSM_AudioAttributes = GetMenuAudioAttsFromBytes(tByteA)

            '    VTSM_SubpictureStreamCount = CInt("&h" & GetByte(IFO(340)) & GetByte(IFO(341)))

            '    ReDim tByteA(167)
            '    System.Array.Copy(IFO, 342, tByteA, 0, 168)
            '    VTSM_SubpictureAttributes = GetSubpictureAttsFromBytes(tByteA)

            '    ReDim tByteA(1)
            '    System.Array.Copy(IFO, 512, tByteA, 0, 2)
            '    VTS_VideoAttributes = GetVideoAttsFromBytes(tByteA)

            '    VTS_AudioStreamCount = CInt("&h" & GetByte(IFO(514)) & GetByte(IFO(515)))

            '    ReDim tByteA(63)
            '    System.Array.Copy(IFO, 516, tByteA, 0, 64)
            '    VTS_AudioAttributes = GetTitleAudioAttsFromBytes(tByteA)

            '    VTS_SubpictureStreamCount = CInt("&h" & GetByte(IFO(596)) & GetByte(IFO(597)))

            '    ReDim tByteA(191)
            '    System.Array.Copy(IFO, 598, tByteA, 0, 192)
            '    VTS_SubpictureAttributes = GetSubpictureAttsFromBytes(tByteA)

            '    'ReDim tByteA(191)
            '    'System.Array.Copy(IFO, 792, tByteA, 0, 192)
            '    'MultichannelExtension = BytesToMultichannelExtension(tByteA)

            '    TitleCount = CInt("&h" & GetByte(IFO(VTS_PTT_SRPT_SectorPointer)) & GetByte(IFO(VTS_PTT_SRPT_SectorPointer + 1)))

            '    PGCCount_TitleSpace = CInt("&h" & GetByte(IFO(VTS_PGCIT_SectorPointer)) & GetByte(IFO(VTS_PGCIT_SectorPointer + 1)))

            '    Dim VTS_PGC_Offsets(PGCCount_TitleSpace - 1) As Long

            '    Dim StartOfVTS_PGCI As Short = VTS_PGCIT_SectorPointer

            '    Dim Plus12 As Short = StartOfVTS_PGCI + 12

            '    For i As Short = 0 To PGCCount_TitleSpace - 1
            '        B1 = GetByte(IFO(Plus12 + (i * 8)))
            '        B2 = GetByte(IFO(Plus12 + (i * 8) + 1))
            '        B3 = GetByte(IFO(Plus12 + (i * 8) + 2))
            '        B4 = GetByte(IFO(Plus12 + (i * 8) + 3))

            '        If B1.Length = 1 Then B1 = "0" & B1
            '        If B2.Length = 1 Then B2 = "0" & B2
            '        If B3.Length = 1 Then B3 = "0" & B3
            '        If B4.Length = 1 Then B4 = "0" & B4

            '        VTS_PGC_Offsets(i) = CInt("&h" & B1 & B2 & B3 & B4)
            '    Next

            '    '---------------------------------------------------------------------------------
            '    'END VTS
            '    '---------------------------------------------------------------------------------

            '    '---------------------------------------------------------------------------------
            '    'PGCs
            '    '---------------------------------------------------------------------------------

            '    Dim P As cPGC
            '    'Dim PGCStart As Long
            '    For i As Short = 0 To PGCCount_TitleSpace - 1
            '        'After review of the spec it appears that this value just provides the VTS_TTN
            '        Dim PGCC As String = DecimalToBinary(CInt("&h" & GetByte(IFO(StartOfVTS_PGCI + 8 + (i * 8)))))
            '        PGCC = Microsoft.VisualBasic.Right(PGCC, Len(PGCC) - 1)
            '        P = New cPGC(IFO, VTS_PGC_Offsets(i) + StartOfVTS_PGCI, i + 1, ID)
            '        P.VTS_TTN = BinToDec(PGCC)


            '        'P.PGCNumber = i + 1
            '        'PGCStart = VTS_PGC_Offsets(i) + StartOfVTS_PGCI

            '        ''PGC Category Info

            '        ''After review of the spec it appears that this value just provides the VTS_TTN
            '        'Dim PGCC As String = Me.DecimalToBinary(CInt("&h" & GetByte(IFO(StartOfVTS_PGCI + 8 + (i * 8)))))
            '        'PGCC = Microsoft.VisualBasic.Right(PGCC, Len(PGCC) - 1)
            '        'P.VTS_TTN = Me.BinaryToDecimal(PGCC)

            '        ''End PGC Category Info

            '        'P.Offset = PGCStart
            '        'P.ProgramCount = CInt("&h" & GetByte(IFO(PGCStart + 2)))
            '        'P.CellCount = CInt("&h" & GetByte(IFO(PGCStart + 3)))

            '        'ReDim tByteA(3)
            '        'System.Array.Copy(IFO, PGCStart + 4, tByteA, 0, 4)
            '        'P.PlaybackTime = GetTimecodeFromBytes(tByteA).ToString

            '        'P.ProhibitedUOPs = CInt("&h" & GetByte(IFO(PGCStart + 8)) & GetByte(IFO(PGCStart + 9)) & GetByte(IFO(PGCStart + 10)) & GetByte(IFO(PGCStart + 11)))
            '        'P.PGC_AST_CTL = New cPGC_AST_CTL
            '        'P.PGC_SPST_CTL = New cPGC_SPST_CTL

            '        'P.NextPGCN = CInt("&h" & GetByte(IFO(PGCStart + 156)) & GetByte(IFO(PGCStart + 157)))
            '        'P.PreviousPGCN = CInt("&h" & GetByte(IFO(PGCStart + 158)) & GetByte(IFO(PGCStart + 159)))
            '        'P.GoUpPGCN = CInt("&h" & GetByte(IFO(PGCStart + 160)) & GetByte(IFO(PGCStart + 161)))

            '        'Select Case CInt("&h" & GetByte(IFO(PGCStart + 162)))
            '        '    Case 0
            '        '        P.PlaybackMode = "Sequential"
            '        '    Case Else
            '        '        P.PlaybackMode = "Other - not implemented"
            '        'End Select

            '        'P.PGCStillTime = CInt("&h" & CStr(GetByte(IFO(PGCStart + 163))))
            '        'P.CLUT = New cCLUT
            '        'P.offCommands = CInt("&h" & GetByte(IFO(PGCStart + 228)) & GetByte(IFO(PGCStart + 229)))
            '        'P.offProgramMap = CInt("&h" & GetByte(IFO(PGCStart + 230)) & GetByte(IFO(PGCStart + 231)))
            '        'P.offCellPlaybackInfo = CInt("&h" & GetByte(IFO(PGCStart + 232)) & GetByte(IFO(PGCStart + 233)))
            '        'P.offCellPositionInfo = CInt("&h" & GetByte(IFO(PGCStart + 234)) & GetByte(IFO(PGCStart + 235)))

            '        'P.Commands = Nothing

            '        'ReDim tByteA(P.ProgramCount - 1)
            '        'System.Array.Copy(IFO, PGCStart + P.offProgramMap, tByteA, 0, P.ProgramCount)
            '        'P.ProgramMap = GetProgramMap(tByteA)

            '        'ReDim tByteA(P.CellCount * 24 - 1)
            '        'System.Array.Copy(IFO, PGCStart + P.offCellPlaybackInfo, tByteA, 0, P.CellCount * 24)
            '        'P.CellPlaybackInfo = GetCellPlaybackInfo(tByteA, ID, P.PGCNumber)

            '        'P.CellPositionInfo = Nothing

            '        PGCs.Add(P)

            '    Next

            '    '---------------------------------------------------------------------------------
            '    'END PGCs
            '    '---------------------------------------------------------------------------------

            '    '---------------------------------------------------------------------------------
            '    'Titles
            '    '---------------------------------------------------------------------------------

            '    Dim TTStart As Integer = VTS_PTT_SRPT_SectorPointer
            '    Dim TT As cTitle
            '    TitleCount = CInt("&h" & GetByte(IFO(TTStart)) & GetByte(IFO(TTStart + 1)))

            '    TT_EndOfPTTTable = CInt("&h" & GetByte(IFO(TTStart + 4)) & GetByte(IFO(TTStart + 5)) & GetByte(IFO(TTStart + 6)) & GetByte(IFO(TTStart + 7)))
            '    ReDim TT_StartBytes(TitleCount - 1)

            '    For i As Short = 0 To TitleCount - 1
            '        B1 = GetByte(IFO(TTStart + 8 + (4 * i)))
            '        B2 = GetByte(IFO(TTStart + 8 + 1 + (4 * i)))
            '        B3 = GetByte(IFO(TTStart + 8 + 2 + (4 * i)))
            '        B4 = GetByte(IFO(TTStart + 8 + 3 + (4 * i)))

            '        If B1.Length = 1 Then B1 = "0" & B1
            '        If B2.Length = 1 Then B2 = "0" & B2
            '        If B3.Length = 1 Then B3 = "0" & B3
            '        If B4.Length = 1 Then B4 = "0" & B4

            '        TT_StartBytes(i) = CInt("&h" & B1 & B2 & B3 & B4)
            '    Next

            '    Dim PTT As cPTT
            '    Dim PTTStart As Integer = 0
            '    Dim PTTCnt As Short
            '    For i As Short = 0 To TitleCount - 1
            '        TT = New cTitle
            '        TT.VTS_TT = i + 1
            '        TT.VTS = ID
            '        PTTStart = TT_StartBytes(i) + TTStart

            '        PTTCnt = 1
            '        Dim Upper As Integer
            '        If TT_StartBytes.Length - 1 = i Then
            '            Upper = TT_EndOfPTTTable + TTStart
            '        Else
            '            Upper = TTStart + (TT_StartBytes(i + 1) - 1)
            '        End If
            '        For pi As Integer = PTTStart To Upper Step 4
            '            PTT = New cPTT
            '            PTT.PTTNumber = PTTCnt
            '            PTT.PGCN = CInt("&h" & GetByte(IFO(pi)) & GetByte(IFO(pi + 1)))
            '            PTT.PGN = CInt("&h" & GetByte(IFO(pi + 2)) & GetByte(IFO(pi + 3)))
            '            TT.PTTs.Add(PTT)
            '            PTTCnt += 1
            '        Next
            '        PTTCnt = 0
            '        Titles.Add(TT)
            '    Next

            '    '---------------------------------------------------------------------------------
            '    'END Titles
            '    '---------------------------------------------------------------------------------

            '    '---------------------------------------------------------------------------------
            '    'Cell Mapping
            '    '---------------------------------------------------------------------------------

            '    Dim tCM As cCellMapItem
            '    Dim PrevPTT As Short
            '    Dim PrevPGN As Short
            '    For Each oP As cPGC In PGCs
            '        oP.CellMap = New colCells
            '        If oP.CellCount > 0 Then
            '            For c As Short = 1 To oP.CellPlaybackInfo.Count
            '                tCM = New cCellMapItem
            '                tCM.CellNo = c

            '                'get pgn from celln
            '                For Each PMI As cProgramMap In oP.ProgramMap
            '                    If PMI.CellNo = c Then
            '                        tCM.PGNo = PMI.ProgramNo + 1
            '                        PrevPGN = tCM.PGNo
            '                    End If
            '                Next

            '                If tCM.PGNo = 0 Then tCM.PGNo = PrevPGN

            '                'get pttn from pgn
            '                For Each oTT As cTitle In Titles
            '                    For Each oPTT As cPTT In oTT.PTTs
            '                        If oPTT.PGCN = oP.PGCNumber Then
            '                            If oPTT.PGN = tCM.PGNo Then
            '                                tCM.PTTNoChNo = oPTT.PTTNumber
            '                                PrevPTT = tCM.PTTNoChNo
            '                                Exit For
            '                            End If
            '                        End If
            '                    Next
            '                Next

            '                If tCM.PTTNoChNo = 0 Then tCM.PTTNoChNo = PrevPTT
            '                oP.CellMap.Add(tCM)
            '            Next
            '        End If
            '    Next

            '    '---------------------------------------------------------------------------------
            '    'END Cell Mapping
            '    '---------------------------------------------------------------------------------

            'Catch ex As Exception
            '    Throw New Exception("Problem with New cVTS. Error: " & ex.Message, ex)
            'End Try
        End Sub

        Public Sub New(ByRef VTS_Stream As Stream, ByVal VTSNumber As Integer)
            Try
                'If Not File.Exists(nVTSIFOPath) Then Throw New Exception("IFO file does not exist:" & nVTSIFOPath)

                'VTSIFOPath = nVTSIFOPath
                Name = "VTS_" & PadString(VTSNumber, 2, "0", True) & "_0.IFO" ' Path.GetFileName(VTSIFOPath)
                Titles = New colTitles
                PGCs = New colPGCs

                Dim IFO() As Byte
                Dim B1, B2, B3, B4 As String
                'Dim FS As New FileStream(nVTSIFOPath, FileMode.Open, FileAccess.Read)
                ReDim IFO(VTS_Stream.Length - 1)
                VTS_Stream.Read(IFO, 0, VTS_Stream.Length - 1)
                VTS_Stream.Close()
                VTS_Stream = Nothing

                'Dim tStrA() As String

                '---------------------------------------------------------------------------------
                'VTS
                '---------------------------------------------------------------------------------

                'tStrA = Split(Path.GetFileNameWithoutExtension(nVTSIFOPath), "_", -1, CompareMethod.Text) 'should be like "01"
                'ID = tStrA(1)
                ID = PadString(VTSNumber, 2, "0", True)
                TitleSetLastSector = CInt("&h" & GetByte(IFO(12)) & GetByte(IFO(13)) & GetByte(IFO(14)) & GetByte(IFO(15)))
                IFOLastSector = CInt("&h" & GetByte(IFO(16)) & GetByte(IFO(17)) & GetByte(IFO(18)) & GetByte(IFO(19)))
                Version = "v" & CInt(Microsoft.VisualBasic.Left(GetByte(IFO(33)), 1)) & "." & CInt(Microsoft.VisualBasic.Right(GetByte(IFO(33)), 1))
                VTSCategory = CInt("&h" & GetByte(IFO(34)) & GetByte(IFO(35)) & GetByte(IFO(36)) & GetByte(IFO(37)))
                If VTSCategory = 0 Then
                    VTSCategory = "Unspecified"
                Else
                    VTSCategory = "Karaoke"
                End If
                VTS_MATEndByteAddress = CInt("&h" & GetByte(IFO(128)) & GetByte(IFO(129)) & GetByte(IFO(130)) & GetByte(IFO(131)))
                MenuVOBStartSector = CInt("&h" & GetByte(IFO(192)) & GetByte(IFO(193)) & GetByte(IFO(194)) & GetByte(IFO(195)))
                TitleVOBStartSector = CInt("&h" & GetByte(IFO(196)) & GetByte(IFO(197)) & GetByte(IFO(198)) & GetByte(IFO(199)))
                VTS_PTT_SRPT_SectorPointer = SectorSize * CInt("&h" & GetByte(IFO(200)) & GetByte(IFO(201)) & GetByte(IFO(202)) & GetByte(IFO(203)))
                VTS_PGCIT_SectorPointer = SectorSize * CInt("&h" & GetByte(IFO(204)) & GetByte(IFO(205)) & GetByte(IFO(206)) & GetByte(IFO(207)))
                VTSM_PGCI_UT_SectorPointer = SectorSize * CInt("&h" & GetByte(IFO(208)) & GetByte(IFO(209)) & GetByte(IFO(210)) & GetByte(IFO(211)))
                VTS_TMAPTI_SectorPointer = SectorSize * CInt("&h" & GetByte(IFO(212)) & GetByte(IFO(213)) & GetByte(IFO(214)) & GetByte(IFO(215)))
                VTSM_C_ADT_SectorPointer = SectorSize * CInt("&h" & GetByte(IFO(216)) & GetByte(IFO(217)) & GetByte(IFO(218)) & GetByte(IFO(219)))
                VTSM_VOBU_ADMAP_SectorPointer = SectorSize * CInt("&h" & GetByte(IFO(220)) & GetByte(IFO(221)) & GetByte(IFO(222)) & GetByte(IFO(223)))
                VTS_C_ADT_SectorPointer = SectorSize * CInt("&h" & GetByte(IFO(224)) & GetByte(IFO(225)) & GetByte(IFO(226)) & GetByte(IFO(227)))
                VTS_VOBU_ADMAP_SectorPointer = SectorSize * CInt("&h" & GetByte(IFO(228)) & GetByte(IFO(229)) & GetByte(IFO(230)) & GetByte(IFO(231)))

                VTSM_PGCI_UT = New cVXXM_PGCI_UT(IFO, VTSM_PGCI_UT_SectorPointer)
                VTS_PGCIT = New cVTS_PGCIT(IFO, VTS_PGCIT_SectorPointer)

                Dim tByteA() As Byte

                ReDim tByteA(1)
                System.Array.Copy(IFO, 256, tByteA, 0, 2)
                VTSM_VideoAttributes = GetVideoAttsFromBytes(tByteA)
                VTSM_AudioStreamCount = CInt("&h" & GetByte(IFO(258)) & GetByte(IFO(259)))

                ReDim tByteA(63)
                System.Array.Copy(IFO, 260, tByteA, 0, 64)
                VTSM_AudioAttributes = GetMenuAudioAttsFromBytes(tByteA)

                VTSM_SubpictureStreamCount = CInt("&h" & GetByte(IFO(340)) & GetByte(IFO(341)))

                ReDim tByteA(167)
                System.Array.Copy(IFO, 342, tByteA, 0, 168)
                VTSM_SubpictureAttributes = GetSubpictureAttsFromBytes(tByteA)

                ReDim tByteA(1)
                System.Array.Copy(IFO, 512, tByteA, 0, 2)
                VTS_VideoAttributes = GetVideoAttsFromBytes(tByteA)

                VTS_AudioStreamCount = CInt("&h" & GetByte(IFO(514)) & GetByte(IFO(515)))

                ReDim tByteA(63)
                System.Array.Copy(IFO, 516, tByteA, 0, 64)
                VTS_AudioAttributes = GetTitleAudioAttsFromBytes(tByteA)

                VTS_SubpictureStreamCount = CInt("&h" & GetByte(IFO(596)) & GetByte(IFO(597)))

                ReDim tByteA(191)
                System.Array.Copy(IFO, 598, tByteA, 0, 192)
                VTS_SubpictureAttributes = GetSubpictureAttsFromBytes(tByteA)

                'ReDim tByteA(191)
                'System.Array.Copy(IFO, 792, tByteA, 0, 192)
                'MultichannelExtension = BytesToMultichannelExtension(tByteA)

                TitleCount = CInt("&h" & GetByte(IFO(VTS_PTT_SRPT_SectorPointer)) & GetByte(IFO(VTS_PTT_SRPT_SectorPointer + 1)))

                PGCCount_TitleSpace = CInt("&h" & GetByte(IFO(VTS_PGCIT_SectorPointer)) & GetByte(IFO(VTS_PGCIT_SectorPointer + 1)))

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
                'Dim PGCStart As Long
                For i As Short = 0 To PGCCount_TitleSpace - 1
                    'After review of the spec it appears that this value just provides the VTS_TTN
                    Dim PGCC As String = DecimalToBinary(CInt("&h" & GetByte(IFO(StartOfVTS_PGCI + 8 + (i * 8)))))
                    PGCC = Microsoft.VisualBasic.Right(PGCC, Len(PGCC) - 1)
                    P = New cPGC(IFO, VTS_PGC_Offsets(i) + StartOfVTS_PGCI, i + 1, ID)
                    P.VTS_TTN = BinToDec(PGCC)


                    'P.PGCNumber = i + 1
                    'PGCStart = VTS_PGC_Offsets(i) + StartOfVTS_PGCI

                    ''PGC Category Info

                    ''After review of the spec it appears that this value just provides the VTS_TTN
                    'Dim PGCC As String = Me.DecimalToBinary(CInt("&h" & GetByte(IFO(StartOfVTS_PGCI + 8 + (i * 8)))))
                    'PGCC = Microsoft.VisualBasic.Right(PGCC, Len(PGCC) - 1)
                    'P.VTS_TTN = Me.BinaryToDecimal(PGCC)

                    ''End PGC Category Info

                    'P.Offset = PGCStart
                    'P.ProgramCount = CInt("&h" & GetByte(IFO(PGCStart + 2)))
                    'P.CellCount = CInt("&h" & GetByte(IFO(PGCStart + 3)))

                    'ReDim tByteA(3)
                    'System.Array.Copy(IFO, PGCStart + 4, tByteA, 0, 4)
                    'P.PlaybackTime = GetTimecodeFromBytes(tByteA).ToString

                    'P.ProhibitedUOPs = CInt("&h" & GetByte(IFO(PGCStart + 8)) & GetByte(IFO(PGCStart + 9)) & GetByte(IFO(PGCStart + 10)) & GetByte(IFO(PGCStart + 11)))
                    'P.PGC_AST_CTL = New cPGC_AST_CTL
                    'P.PGC_SPST_CTL = New cPGC_SPST_CTL

                    'P.NextPGCN = CInt("&h" & GetByte(IFO(PGCStart + 156)) & GetByte(IFO(PGCStart + 157)))
                    'P.PreviousPGCN = CInt("&h" & GetByte(IFO(PGCStart + 158)) & GetByte(IFO(PGCStart + 159)))
                    'P.GoUpPGCN = CInt("&h" & GetByte(IFO(PGCStart + 160)) & GetByte(IFO(PGCStart + 161)))

                    'Select Case CInt("&h" & GetByte(IFO(PGCStart + 162)))
                    '    Case 0
                    '        P.PlaybackMode = "Sequential"
                    '    Case Else
                    '        P.PlaybackMode = "Other - not implemented"
                    'End Select

                    'P.PGCStillTime = CInt("&h" & CStr(GetByte(IFO(PGCStart + 163))))
                    'P.CLUT = New cCLUT
                    'P.offCommands = CInt("&h" & GetByte(IFO(PGCStart + 228)) & GetByte(IFO(PGCStart + 229)))
                    'P.offProgramMap = CInt("&h" & GetByte(IFO(PGCStart + 230)) & GetByte(IFO(PGCStart + 231)))
                    'P.offCellPlaybackInfo = CInt("&h" & GetByte(IFO(PGCStart + 232)) & GetByte(IFO(PGCStart + 233)))
                    'P.offCellPositionInfo = CInt("&h" & GetByte(IFO(PGCStart + 234)) & GetByte(IFO(PGCStart + 235)))

                    'P.Commands = Nothing

                    'ReDim tByteA(P.ProgramCount - 1)
                    'System.Array.Copy(IFO, PGCStart + P.offProgramMap, tByteA, 0, P.ProgramCount)
                    'P.ProgramMap = GetProgramMap(tByteA)

                    'ReDim tByteA(P.CellCount * 24 - 1)
                    'System.Array.Copy(IFO, PGCStart + P.offCellPlaybackInfo, tByteA, 0, P.CellCount * 24)
                    'P.CellPlaybackInfo = GetCellPlaybackInfo(tByteA, ID, P.PGCNumber)

                    'P.CellPositionInfo = Nothing

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
                TitleCount = CInt("&h" & GetByte(IFO(TTStart)) & GetByte(IFO(TTStart + 1)))

                TT_EndOfPTTTable = CInt("&h" & GetByte(IFO(TTStart + 4)) & GetByte(IFO(TTStart + 5)) & GetByte(IFO(TTStart + 6)) & GetByte(IFO(TTStart + 7)))
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
                        PTT.PGCN = CInt("&h" & GetByte(IFO(pi)) & GetByte(IFO(pi + 1)))
                        PTT.PGN = CInt("&h" & GetByte(IFO(pi + 2)) & GetByte(IFO(pi + 3)))
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
                                    tCM.PGNo = PMI.ProgramNo + 1
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

    Public Class colVTSs
        Inherits CollectionBase

        Public Function Add(ByVal newIFO As cVTS) As Integer
            Return MyBase.List.Add(newIFO)
        End Function

        Default Public ReadOnly Property Item(ByVal index As Integer) As cVTS
            Get
                Return MyBase.List.Item(index)
            End Get
        End Property

        Public Sub Remove(ByVal Item As cVTS)
            MyBase.List.Remove(Item)
        End Sub

    End Class

    Public Class cVTS_PGCIT

        Public VTS_PGCITI As cVTS_PGCITI
        Public VTS_PGCI_SRP() As cVTS_PGCI_SRP
        Public VTS_PGCI() As cVTS_PGCI

        Public Sub New(ByRef IFO() As Byte, ByVal StartAddress As ULong)
            Try

            Catch ex As Exception

            End Try
        End Sub

        Public Class cVTS_PGCITI

            Public SRPCount As Integer

            Public Sub New(ByRef IFO() As Byte, ByVal StartAddress As ULong)

            End Sub

        End Class

        Public Class cVTS_PGCI_SRP

            Public VTS_PGC_Category As cVTS_PGC_Category
            Public VTS_PGCI_StartAddress As ULong

            Public Class cVTS_PGC_Category
                Public EntryType As eEntryType
                Public VTS_TTN As Byte
                Public BlockMode As eBlockMode
                Public BlockType As eBlockType
                Public PTL_ID_FLD As UShort

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

        Public Class cVTS_PGCI

            Public Sub New(ByRef IFO() As Byte, ByVal StartAddress As ULong)

            End Sub

        End Class

    End Class

#End Region 'VTS

#Region "VMGM"

    Public Class cVMGM
        Public Name As String
        'Public VMGMIFOPath As String

        Public NumberOfGlobalTitles As Short
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

        Public LastSectorVMGSet As Int64
        Public LastSectorIFO As Int64
        Public VersionNumber As String
        Public VMGCategory As Integer
        Public NumberOfVolumes As Integer
        Public VolumeNumber As Integer
        Public SideID As Integer
        Public NumberOfTitleSets As Integer
        Public ProviderID As String
        Public VMGPOS() As Byte
        Public EndAddressVMGI_MAT As Int64
        Public StartAddressFP_PGC As Int64
        Public StartSectorMenuVOB As Int64
        Public SectorPointerTT_SRPT As Int64
        Public SectorPointerVMGM_PGCI_UT As Int64
        Public SectorPointerVMG_PTL_MAIT As Int64
        Public SectorPointerVMG_VTS_ATRT As Int64
        Public SectorPointerVMG_TXTDT_MG As Int64
        Public SectorPointerVMGM_C_ADT As Int64
        Public SectorPointerVMGM_VOBU_ADMAP As Int64
        Public VidAttsVMGM_VOBS As cVideoAttributes
        Public NumberOfAudioStreamsInVMGM_VOBS As Byte
        Public AudAttsVMGM_VOBS As cMenuAudioAttributes
        Public NumberOfSubpictureStreamsInVMGM_VOBS As Byte
        Public SubAttsVMGM_VOBS As cSubpictureAttributes

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
            'Try
            '    If Not File.Exists(nVMGMIFOPath) Then Throw New Exception("IFO file does not exist: " & nVMGMIFOPath)

            '    'Dim B1, B2, B3, B4 As String
            '    Dim FS As New FileStream(nVMGMIFOPath, FileMode.Open, FileAccess.Read)
            '    Me.NewFromStream(FS)

            '    Dim VMGM(FS.Length - 1) As Byte
            '    FS.Read(VMGM, 0, FS.Length - 1)
            '    FS.Close()
            '    FS = Nothing

            '    Dim tByteA() As Byte

            '    VMGMIFOPath = nVMGMIFOPath
            '    Name = Path.GetFileName(nVMGMIFOPath)

            '    LastSectorVMGSet = CInt("&h" & GetByte(VMGM(12)) & GetByte(VMGM(13)) & GetByte(VMGM(14)) & GetByte(VMGM(15)))
            '    LastSectorIFO = CInt("&h" & GetByte(VMGM(28)) & GetByte(VMGM(29)) & GetByte(VMGM(30)) & GetByte(VMGM(31)))
            '    VersionNumber = ((VMGM(33) >> 4) And 15) & "." & ((VMGM(33) >> 0) And 15)
            '    'CInt("&h" & GetByte(VMGM(32)) & GetByte(VMGM(33)))
            '    VMGCategory = CInt("&h" & GetByte(VMGM(34)) & GetByte(VMGM(35)) & GetByte(VMGM(36)) & GetByte(VMGM(37)))
            '    RegionMask = CInt("&h" & GetByte(VMGM(35)))

            '    Dim ba As New cSeqBitArray(VMGM(35))
            '    R1 = ba.BitToBool(7, True)
            '    R2 = ba.BitToBool(6, True)
            '    R3 = ba.BitToBool(5, True)
            '    R4 = ba.BitToBool(4, True)
            '    R5 = ba.BitToBool(3, True)
            '    R6 = ba.BitToBool(2, True)
            '    R7 = ba.BitToBool(1, True)
            '    R8 = ba.BitToBool(0, True)
            '    ba = Nothing

            '    NumberOfVolumes = CInt("&h" & GetByte(VMGM(38)) & GetByte(VMGM(39)))
            '    VolumeNumber = CInt("&h" & GetByte(VMGM(40)) & GetByte(VMGM(41)))
            '    SideID = CInt("&h" & GetByte(VMGM(42)))
            '    NumberOfTitleSets = CInt("&h" & GetByte(VMGM(62)) & GetByte(VMGM(63)))

            '    ReDim tByteA(31)
            '    System.Array.Copy(VMGM, 64, tByteA, 0, 32)
            '    ProviderID = GetStringFromBytes(tByteA)

            '    ReDim tByteA(7)
            '    System.Array.Copy(VMGM, 96, tByteA, 0, 8)
            '    VMGPOS = tByteA

            '    EndAddressVMGI_MAT = CInt("&h" & GetByte(VMGM(128)) & GetByte(VMGM(129)) & GetByte(VMGM(130)) & GetByte(VMGM(131)))
            '    StartAddressFP_PGC = CInt("&h" & GetByte(VMGM(132)) & GetByte(VMGM(133)) & GetByte(VMGM(134)) & GetByte(VMGM(135)))
            '    StartSectorMenuVOB = CInt("&h" & GetByte(VMGM(192)) & GetByte(VMGM(193)) & GetByte(VMGM(194)) & GetByte(VMGM(195)))
            '    SectorPointerTT_SRPT = SectorSize * CInt("&h" & GetByte(VMGM(196)) & GetByte(VMGM(197)) & GetByte(VMGM(198)) & GetByte(VMGM(199)))
            '    SectorPointerVMGM_PGCI_UT = SectorSize * CInt("&h" & GetByte(VMGM(200)) & GetByte(VMGM(201)) & GetByte(VMGM(202)) & GetByte(VMGM(203)))
            '    SectorPointerVMG_PTL_MAIT = SectorSize * CInt("&h" & GetByte(VMGM(204)) & GetByte(VMGM(205)) & GetByte(VMGM(206)) & GetByte(VMGM(207)))
            '    SectorPointerVMG_VTS_ATRT = SectorSize * CInt("&h" & GetByte(VMGM(208)) & GetByte(VMGM(209)) & GetByte(VMGM(210)) & GetByte(VMGM(211)))
            '    SectorPointerVMG_TXTDT_MG = SectorSize * CInt("&h" & GetByte(VMGM(212)) & GetByte(VMGM(213)) & GetByte(VMGM(214)) & GetByte(VMGM(215)))
            '    SectorPointerVMGM_C_ADT = SectorSize * CInt("&h" & GetByte(VMGM(216)) & GetByte(VMGM(217)) & GetByte(VMGM(218)) & GetByte(VMGM(219)))
            '    SectorPointerVMGM_VOBU_ADMAP = SectorSize * CInt("&h" & GetByte(VMGM(220)) & GetByte(VMGM(221)) & GetByte(VMGM(222)) & GetByte(VMGM(223)))

            '    'If StartSectorMenuVOB > 0 Then
            '    'Get VMG VOB info
            '    ReDim tByteA(1)
            '    System.Array.Copy(VMGM, 256, tByteA, 0, 2)
            '    VidAttsVMGM_VOBS = GetVideoAttsFromBytes(tByteA)

            '    NumberOfAudioStreamsInVMGM_VOBS = CInt("&h" & GetByte(VMGM(258)) & GetByte(VMGM(259)))

            '    ReDim tByteA(63)
            '    System.Array.Copy(VMGM, 260, tByteA, 0, 64)
            '    AudAttsVMGM_VOBS = GetMenuAudioAttsFromBytes(tByteA)

            '    NumberOfSubpictureStreamsInVMGM_VOBS = CInt("&h" & GetByte(VMGM(340)) & GetByte(VMGM(341)))

            '    ReDim tByteA(167)
            '    System.Array.Copy(VMGM, 342, tByteA, 0, 168)
            '    SubAttsVMGM_VOBS = GetSubpictureAttsFromBytes(tByteA)

            '    NumberOfGlobalTitles = CInt("&h" & GetByte(VMGM(SectorPointerTT_SRPT)) & GetByte(VMGM(SectorPointerTT_SRPT + 1)))
            '    ReDim tByteA(8 + (NumberOfGlobalTitles * 12) - 1)
            '    System.Array.Copy(VMGM, SectorPointerTT_SRPT, tByteA, 0, 8 + (NumberOfGlobalTitles * 12))

            '    If SectorPointerVMG_PTL_MAIT > 0 Then
            '        Me.VMG_PTL_MAIT = New cVMG_PTL_MAIT(VMGM, SectorPointerVMG_PTL_MAIT)
            '        HasParentalManagement = True
            '    Else
            '        HasParentalManagement = False
            '    End If

            '    GlobalTTs = ParseGlobalTTs(tByteA)

            '    'OTHER STRUCTURES
            '    VMGM_PGCI_UT = New cVXXM_PGCI_UT(VMGM, SectorPointerVMGM_PGCI_UT, nVMGMIFOPath)

            '    'Me.VMG_TXTDT_MG = New cVMG_TXTDT_MG(VMGM, SectorPointerVMG_TXTDT_MG)

            'Catch ex As Exception
            '    MsgBox("Problem with New() cVMGM. Error: " & ex.Message)
            'End Try
        End Sub

        Public Sub New(ByRef VMGM_Stream As Stream)
            Try
                'Dim B1, B2, B3, B4 As String
                Dim VMGM(VMGM_Stream.Length - 1) As Byte
                VMGM_Stream.Read(VMGM, 0, VMGM_Stream.Length - 1)
                VMGM_Stream.Close()
                VMGM_Stream = Nothing

                Dim tByteA() As Byte

                'VMGMIFOPath = nVMGMIFOPath
                Name = "VIDEO_TS.IFO" 'Path.GetFileName(nVMGMIFOPath)

                LastSectorVMGSet = CInt("&h" & GetByte(VMGM(12)) & GetByte(VMGM(13)) & GetByte(VMGM(14)) & GetByte(VMGM(15)))
                LastSectorIFO = CInt("&h" & GetByte(VMGM(28)) & GetByte(VMGM(29)) & GetByte(VMGM(30)) & GetByte(VMGM(31)))
                VersionNumber = ((VMGM(33) >> 4) And 15) & "." & ((VMGM(33) >> 0) And 15)
                'CInt("&h" & GetByte(VMGM(32)) & GetByte(VMGM(33)))
                VMGCategory = CInt("&h" & GetByte(VMGM(34)) & GetByte(VMGM(35)) & GetByte(VMGM(36)) & GetByte(VMGM(37)))
                RegionMask = CInt("&h" & GetByte(VMGM(35)))

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

                NumberOfVolumes = CInt("&h" & GetByte(VMGM(38)) & GetByte(VMGM(39)))
                VolumeNumber = CInt("&h" & GetByte(VMGM(40)) & GetByte(VMGM(41)))
                SideID = CInt("&h" & GetByte(VMGM(42)))
                NumberOfTitleSets = CInt("&h" & GetByte(VMGM(62)) & GetByte(VMGM(63)))

                ReDim tByteA(31)
                System.Array.Copy(VMGM, 64, tByteA, 0, 32)
                ProviderID = GetStringFromBytes(tByteA)

                ReDim tByteA(7)
                System.Array.Copy(VMGM, 96, tByteA, 0, 8)
                VMGPOS = tByteA

                EndAddressVMGI_MAT = CInt("&h" & GetByte(VMGM(128)) & GetByte(VMGM(129)) & GetByte(VMGM(130)) & GetByte(VMGM(131)))
                StartAddressFP_PGC = CInt("&h" & GetByte(VMGM(132)) & GetByte(VMGM(133)) & GetByte(VMGM(134)) & GetByte(VMGM(135)))
                StartSectorMenuVOB = CInt("&h" & GetByte(VMGM(192)) & GetByte(VMGM(193)) & GetByte(VMGM(194)) & GetByte(VMGM(195)))
                SectorPointerTT_SRPT = SectorSize * CInt("&h" & GetByte(VMGM(196)) & GetByte(VMGM(197)) & GetByte(VMGM(198)) & GetByte(VMGM(199)))
                SectorPointerVMGM_PGCI_UT = SectorSize * CInt("&h" & GetByte(VMGM(200)) & GetByte(VMGM(201)) & GetByte(VMGM(202)) & GetByte(VMGM(203)))
                SectorPointerVMG_PTL_MAIT = SectorSize * CInt("&h" & GetByte(VMGM(204)) & GetByte(VMGM(205)) & GetByte(VMGM(206)) & GetByte(VMGM(207)))
                SectorPointerVMG_VTS_ATRT = SectorSize * CInt("&h" & GetByte(VMGM(208)) & GetByte(VMGM(209)) & GetByte(VMGM(210)) & GetByte(VMGM(211)))
                SectorPointerVMG_TXTDT_MG = SectorSize * CInt("&h" & GetByte(VMGM(212)) & GetByte(VMGM(213)) & GetByte(VMGM(214)) & GetByte(VMGM(215)))
                SectorPointerVMGM_C_ADT = SectorSize * CInt("&h" & GetByte(VMGM(216)) & GetByte(VMGM(217)) & GetByte(VMGM(218)) & GetByte(VMGM(219)))
                SectorPointerVMGM_VOBU_ADMAP = SectorSize * CInt("&h" & GetByte(VMGM(220)) & GetByte(VMGM(221)) & GetByte(VMGM(222)) & GetByte(VMGM(223)))

                'If StartSectorMenuVOB > 0 Then
                'Get VMG VOB info
                ReDim tByteA(1)
                System.Array.Copy(VMGM, 256, tByteA, 0, 2)
                VidAttsVMGM_VOBS = GetVideoAttsFromBytes(tByteA)

                NumberOfAudioStreamsInVMGM_VOBS = CInt("&h" & GetByte(VMGM(258)) & GetByte(VMGM(259)))

                ReDim tByteA(63)
                System.Array.Copy(VMGM, 260, tByteA, 0, 64)
                AudAttsVMGM_VOBS = GetMenuAudioAttsFromBytes(tByteA)

                NumberOfSubpictureStreamsInVMGM_VOBS = CInt("&h" & GetByte(VMGM(340)) & GetByte(VMGM(341)))

                ReDim tByteA(167)
                System.Array.Copy(VMGM, 342, tByteA, 0, 168)
                SubAttsVMGM_VOBS = GetSubpictureAttsFromBytes(tByteA)

                NumberOfGlobalTitles = CInt("&h" & GetByte(VMGM(SectorPointerTT_SRPT)) & GetByte(VMGM(SectorPointerTT_SRPT + 1)))
                ReDim tByteA(8 + (NumberOfGlobalTitles * 12) - 1)
                System.Array.Copy(VMGM, SectorPointerTT_SRPT, tByteA, 0, 8 + (NumberOfGlobalTitles * 12))

                If SectorPointerVMG_PTL_MAIT > 0 Then
                    Me.VMG_PTL_MAIT = New cVMG_PTL_MAIT(VMGM, SectorPointerVMG_PTL_MAIT)
                    HasParentalManagement = True
                Else
                    HasParentalManagement = False
                End If

                GlobalTTs = ParseGlobalTTs(tByteA)

                'OTHER STRUCTURES
                VMGM_PGCI_UT = New cVXXM_PGCI_UT(VMGM, SectorPointerVMGM_PGCI_UT)

                'Me.VMG_TXTDT_MG = New cVMG_TXTDT_MG(VMGM, SectorPointerVMG_TXTDT_MG)

            Catch ex As Exception
                MsgBox("Problem with New() cVMGM. Error: " & ex.Message)
            End Try
        End Sub

        Public Function ParseGlobalTTs(ByVal Bytes() As Byte) As colGlobalTTS
            Try
                Dim oGTTs As New colGlobalTTS
                Dim GTT As cGlobalTT
                Dim GTTn As Short = 1
                For i As Integer = 8 To Bytes.Length - 1 Step 12
                    GTT = New cGlobalTT
                    GTT.Type = ParseGlobalTTType(Bytes(i))
                    GTT.NumberOfAngles = CInt("&h" & GetByte(Bytes(i + 1)))
                    GTT.NumberOfChapters = CInt("&h" & GetByte(Bytes(i + 2)) & GetByte(Bytes(i + 3)))
                    GTT.ParentalManagementMask = CInt("&h" & GetByte(Bytes(i + 4)) & GetByte(Bytes(i + 5)))
                    GTT.VTSN = CInt("&h" & GetByte(Bytes(i + 6)))
                    GTT.VTS_TTN = CInt("&h" & GetByte(Bytes(i + 7)))
                    GTT.DiscOffsetToStartOfVTS = CInt("&h" & GetByte(Bytes(i + 8)) & GetByte(Bytes(i + 9)) & GetByte(Bytes(i + 10)) & GetByte(Bytes(i + 11)))
                    GTT.GlobalTT_N = GTTn
                    GTT.ParentalManagementValue = New cParentalManagement_US(GTT.ParentalManagementMask, GTTn, GTT.VTSN, Me)
                    GTTn += 1
                    oGTTs.Add(GTT)
                Next
                Return oGTTs
            Catch ex As Exception
                Throw New Exception("Problem with ParseGlobalTTs(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function ParseGlobalTTType(ByVal b As Byte) As cGTTType
            Try
                Dim out As New cGTTType
                If ((b >> 6) And 1) Then
                    out.Sequential = "one_sequential_pgc"
                Else
                    out.Sequential = "not one_sequential_pgc"
                End If

                out.CellCommand = ((b >> 5) And 1)
                out.PrePostCommand = ((b >> 4) And 1)
                out.ButtonCommand = ((b >> 3) And 1)
                out.UOPPTTPlayOrSearch = ((b >> 1) And 1)
                out.UOPTimePlayOrSearch = ((b >> 0) And 1)

                Return out
            Catch ex As Exception
                MsgBox("Problem with ParseGlobalTTType. Error: " & ex.Message)
            End Try
        End Function

        Public ReadOnly Property GlobalTitleParentalManagement() As cGlobalTitleParentalManagement
            Get
                Return New cGlobalTitleParentalManagement(GlobalTTs)
            End Get
        End Property

    End Class

    Public Class colVMGMs
        Inherits CollectionBase

        Public Function Add(ByVal newVMGM As cVMGM) As Integer
            Return MyBase.List.Add(newVMGM)
        End Function

        Default Public ReadOnly Property Item(ByVal index As Integer) As cVMGM
            Get
                Return MyBase.List.Item(index)
            End Get
        End Property

        Public Sub Remove(ByVal Item As cVMGM)
            MyBase.List.Remove(Item)
        End Sub
    End Class

    Public Class cGlobalTT

        Public Type As cGTTType
        Public NumberOfAngles As Short
        Public NumberOfChapters As Short
        Public ParentalManagementMask As UInteger
        Public ParentalManagementValue As cParentalManagement_US
        Public VTSN As Short
        Public VTS_TTN As Short
        Public DiscOffsetToStartOfVTS As Integer
        Public GlobalTT_N As Short

        Public Overloads Overrides Function ToString() As String
            Return "GTTN: " & GlobalTT_N & " Number of chapters: " & NumberOfChapters & " VTS_N: " & VTSN & " VTS_TTN: " & VTS_TTN
        End Function

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
            Return "TT " & GTTN & ": NC17=" & NC17 & " R=" & R & " PG13=" & PG13 & " PG=" & PG & " G=" & G
        End Function

    End Class

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

    End Class

    Public Class cGTTType
        Public Sequential As String
        Public CellCommand As Boolean
        Public PrePostCommand As Boolean
        Public ButtonCommand As Boolean
        Public UOPPTTPlayOrSearch As Boolean
        Public UOPTimePlayOrSearch As Boolean
    End Class

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

                Dim BS As New cSMTBitshifter

                'Me.LangUnitCount = (IFO(Offset) << 8) Or IFO(Offset + 1)
                Me.LangUnitCount = BS.SetVal(2, IFO(Offset), IFO(Offset + 1))
                If LangUnitCount = 0 Then Exit Sub

                'UT_EndAddress = (IFO(Offset + 4) << 24) Or (IFO(Offset + 5) << 16) Or (IFO(Offset + 6) << 8) Or IFO(Offset + 7)
                VXXM_PGCI_UT_EA = BS.SetVal(4, IFO(Offset + 4), IFO(Offset + 5), IFO(Offset + 6), IFO(Offset + 7))

                'Get LU_SRPs
                ReDim LU_SRPs(Me.LangUnitCount - 1)
                Offset += 8
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
                    MsgBox("Problem with FindLUByLang. Param must be two characters.")
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
                MsgBox("Problem with FindLUByLang. Error: " & ex.Message)
                Return Nothing
            End Try
        End Function

    End Class

    Public Class cVXXM_LU_SRP

        Public VXXM_LCD As String
        Public VXXM_EXST As Byte
        Public VXXM_LU_SA As Integer

        Public Sub New(ByRef IFO() As Byte, ByVal Offset As Int64, ByVal IFOPath As String)
            VXXM_LCD = System.Text.Encoding.ASCII.GetString(IFO, Offset, 2)
            VXXM_EXST = IFO(Offset + 3)

            Dim BS As New cSMTBitshifter(4, IFO(Offset + 4), IFO(Offset + 5), IFO(Offset + 6), IFO(Offset + 7))
            VXXM_LU_SA = BS.Val

            'If VXXM_LCD.ToLower = "iw" Then
            '    If MsgBox("Hebrew ""iw"" found, would you like to update to ""he?""", MsgBoxStyle.YesNo, "Hebrew iw") = MsgBoxResult.Yes Then
            '        '&h6865
            '        'Dim FS As New FileStream(IFOPath, FileMode.Open)
            '        'FS.Position = Offset
            '        'FS.WriteByte(&H68)
            '        'FS.WriteByte(&H65)
            '        'FS.Close()
            '    End If
            'ElseIf VXXM_LCD.ToLower = "he" Then
            '    MsgBox("he found")
            'End If

        End Sub

    End Class

    Public Class cVXXM_LU

        'VXXM_LUI
        Public VXXM_PGCI_SRP_Ns As Short
        Public VXXM_LU_EA As Long

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
                VXXM_PGCI_SRP_Ns = New cSMTBitshifter(2, IFO(Offset), IFO(Offset + 1)).Val
                Me.VXXM_LU_EA = New cSMTBitshifter(4, IFO(Offset + 4), IFO(Offset + 5), IFO(Offset + 6), IFO(Offset + 7)).Val

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
        Public VXXM_PGC_CAT As cPGCCategory
        Public VXXM_PGC_SA As Integer

        'CONSTRUCTOR
        Public Sub New(ByVal IFO() As Byte, ByVal Offset As Int64)
            Try
                VXXM_PGC_CAT = New cPGCCategory
                VXXM_PGC_CAT.EntryPGC = (IFO(Offset) >> 7) And 1
                VXXM_PGC_CAT.MenuID = IFO(Offset) And 4
                VXXM_PGC_CAT.BlockMode = (IFO(Offset + 1) >> 6) And 2
                VXXM_PGC_CAT.BlockType = (IFO(Offset + 1) >> 4) And 2
                VXXM_PGC_CAT.ParentalManagement1 = IFO(Offset + 2)
                VXXM_PGC_CAT.ParentalManagement2 = IFO(Offset + 3)
                Me.VXXM_PGC_SA = New cSMTBitshifter(4, IFO(Offset + 4), IFO(Offset + 5), IFO(Offset + 6), IFO(Offset + 7)).Val
            Catch ex As Exception
                MsgBox("Problem with new PGCI_SRP. Error: " & ex.Message)
            End Try
        End Sub

        Public Structure cPGCCategory
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
        Public PGCN As Integer
        Public PGN As Integer
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

#Region "PGC"

    Public Class cPGC

        Public PGCNumber As Short
        Public Offset As Long

        Public ProgramCount As Short
        Public CellCount As Short

        Public PlaybackTime As String
        Public ProhibitedUOPs As String
        Public PGC_AST_CTL As cPGC_AST_CTLT 'Audio Stream Control
        Public PGC_SPST_CTL As cPGC_SPST_CTLT 'Subpicture Stream Control
        Public NextPGCN As Integer
        Public PreviousPGCN As Integer
        Public GoUpPGCN As Integer
        Public PlaybackMode As String
        Public PGCStillTime As Short
        Public CLUT As cCLUT 'Color LookUp Table 

        Public offCommands As Short
        Public PreCommandCount As Short
        Public PreCommands() As cCMD
        Public PostCommandCount As Short
        Public PostCommands() As cCMD
        Public CellCommandCount As Short
        Public CellCommands() As cCMD

        Public offProgramMap As Integer
        Public offCellPlaybackInfo As Integer
        Public offCellPositionInfo As Integer
        Public ProgramMap As colProgramMap
        Public CellPlaybackInfo As colCellPlaybackInfo
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

        Public Sub New(ByVal IFO() As Byte, ByVal nOffset As Int64, ByVal nPGCNumber As Short, Optional ByVal pVTS As Short = -1)
            Try
                Dim tByteA() As Byte

                PGCNumber = nPGCNumber
                'PGCStart = VTS_PGC_Offsets(i) + StartOfVTS_PGCI

                ParentVTS = pVTS
                Offset = nOffset
                ProgramCount = IFO(Offset + 2)
                CellCount = IFO(Offset + 3)

                PlaybackTime = New cTimecode(IFO, Offset + 4).ToString

                'ProhibitedUOPs = CInt("&h" & GetByte(IFO(Offset + 8)) & GetByte(IFO(Offset + 9)) & GetByte(IFO(Offset + 10)) & GetByte(IFO(Offset + 11)))
                ProhibitedUOPs = Hex(IFO(Offset + 8)) & Hex(IFO(Offset + 9)) & Hex(IFO(Offset + 10)) & Hex(IFO(Offset + 11))

                PGC_AST_CTL = New cPGC_AST_CTLT(IFO, Offset + 12)
                PGC_SPST_CTL = New cPGC_SPST_CTLT

                'NextPGCN = CInt("&h" & GetByte(IFO(Offset + 156)) & GetByte(IFO(Offset + 157)))
                NextPGCN = CInt("&H" & Hex(IFO(Offset + 156)) & Hex(IFO(Offset + 157)))

                'PreviousPGCN = CInt("&h" & GetByte(IFO(Offset + 158)) & GetByte(IFO(Offset + 159)))
                'Debug.WriteLine("&H" & Hex(IFO(Offset + 158)) & Hex(IFO(Offset + 159)))
                PreviousPGCN = CInt("&H" & Hex(IFO(Offset + 158)) & Hex(IFO(Offset + 159)))

                'GoUpPGCN = CInt("&h" & GetByte(IFO(Offset + 160)) & GetByte(IFO(Offset + 161)))
                GoUpPGCN = CInt("&H" & Hex(IFO(Offset + 160)) & Hex(IFO(Offset + 161)))

                Dim tPM As Byte = IFO(Offset + 162)
                If tPM = 0 Then
                    PlaybackMode = "Sequential"
                ElseIf tPM > 0 And tPM < 127 Then
                    PlaybackMode = "Random"
                ElseIf tPM = 128 Then
                    PlaybackMode = "Reserved"
                ElseIf tPM > 128 Then
                    PlaybackMode = "Shuffle"
                End If

                'PGCStillTime = CInt("&h" & CStr(GetByte(IFO(Offset + 163))))
                PGCStillTime = IFO(Offset + 163)

                'PGC_SP_PLT
                CLUT = New cCLUT

                'PGC_CMDT
                offCommands = CInt("&h" & GetByte(IFO(Offset + 228)) & GetByte(IFO(Offset + 229)))
                PreCommandCount = CShort("&H" & Hex(IFO(Offset + offCommands)) & Hex(IFO(Offset + offCommands + 1)))
                PostCommandCount = CShort("&H" & Hex(IFO(Offset + offCommands + 2)) & Hex(IFO(Offset + offCommands + 3)))
                CellCommandCount = CShort("&H" & Hex(IFO(Offset + offCommands + 4)) & Hex(IFO(Offset + offCommands + 5)))

                If PostCommandCount > 0 Then
                    Dim PostCommandTableStartByte As Integer = Offset + (offCommands + 8) + (PreCommandCount * 8)
                    ReDim PostCommands(PostCommandCount - 1)
                    For i As Byte = 0 To UBound(PostCommands)
                        PostCommands(i) = New cCMD(IFO, PostCommandTableStartByte + (i * 8))
                    Next
                End If

                'PGC_PGMAP
                'offProgramMap = CInt("&h" & GetByte(IFO(Offset + 230)) & GetByte(IFO(Offset + 231)))
                offProgramMap = CShort("&H" & Hex(IFO(Offset + 230)) & Hex(IFO(Offset + 231)))
                ReDim tByteA(ProgramCount - 1)
                System.Array.Copy(IFO, Offset + offProgramMap, tByteA, 0, ProgramCount)
                ProgramMap = GetProgramMap(tByteA)

                'C_PBI
                If CellCount > 0 Then
                    offCellPlaybackInfo = CInt("&h" & GetByte(IFO(Offset + 232)) & GetByte(IFO(Offset + 233)))
                    'ReDim tByteA(CellCount * 24 - 1)
                    'System.Array.Copy(IFO, Offset + offCellPlaybackInfo, tByteA, 0, CellCount * 24)
                    CellPlaybackInfo = GetCellPlaybackInfo(IFO, Offset + offCellPlaybackInfo, pVTS, PGCNumber, CellCount)
                    'Debug.WriteLine(Offset & " " & offCellPlaybackInfo & " " & pVTS & " " & nPGCNumber)
                End If

                'Cell position info
                offCellPositionInfo = CShort("&H" & Hex(IFO(Offset + 234)) & Hex(IFO(Offset + 235)))
                CellPositionInfo = Nothing
            Catch ex As Exception
                Throw New Exception("Problem with New PGC. Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Shared Function GetCellPlaybackInfo(ByRef Bytes() As Byte, ByVal Offset As ULong, ByVal VTSNum As Short, ByVal PGCNum As Short, ByVal CellCount As Short) As colCellPlaybackInfo
            Try
                'If Offset = 4452 Then
                '    Debug.WriteLine("hi")
                'End If
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

    Public Class cCMD

        'User facing fields

        Public ReadOnly Property FullCommandName() As String
            Get
                Return ""
            End Get
        End Property

        Public CommandBytes() As Byte
        Public CommandName As String = ""
        Public CommandType As Byte = 0
        Public CompareValue As eCompareValues
        Public GoToValue As eGoToValues
        Public LinkValue As eLinkValues
        Public JumpValue As eJumpValues

        'OPERATION CODE
        Public OperationCode As Byte
        Public CmdID1 As Byte
        Public CmdID2 As Boolean
        Public IFlagForCompare As Boolean
        Public IFlagForSetSetSystem As Boolean
        Public CompareField As Byte
        Public BranchField As Byte
        Public SetSetSystemField As Byte

        'OPERAND SET
        Public OperandSet As UInteger
        Public GoToOperand As UShort
        Public JumpOperand As UInteger
        Public LinkOperand As UShort
        Public SetSystemOperand As UInteger

        Public CP1, CP2, C2 As Byte

        Public IsCallSSRSM127 As Boolean = False

        Public Sub New(ByRef IFO() As Byte, ByVal Offset As Long)
            Try
                ReDim CommandBytes(7)
                Array.Copy(IFO, Offset, CommandBytes, 0, 8)


                If CShort(CommandBytes(0)) + CShort(CommandBytes(1)) = 0 Then
                    CommandType = 1
                    CommandName = "Nop"
                    Exit Sub
                End If

                CmdID1 = CommandBytes(0) >> 5
                CmdID2 = CommandBytes(0) >> 4 And 1
                IFlagForSetSetSystem = CommandBytes(0) >> 4 And 1
                SetSetSystemField = CommandBytes(0) And 15

                IFlagForCompare = CommandBytes(1) >> 7 And 1
                CompareField = CommandBytes(1) >> 4 And 7
                BranchField = CommandBytes(1) And 15

                Me.CompareValue = CompareField
                Me.JumpValue = BranchField
                Me.LinkValue = BranchField
                Me.GoToValue = BranchField

                Select Case CmdID1
                    Case 0
                        CommandType = 1
                        OperationCode = CommandBytes(0) << 16 Or CommandBytes(1)
                        GoToOperand = CommandBytes(6) << 8 Or CommandBytes(7)
                        If IFlagForCompare Then
                            CommandName = "Compare GoTo"
                            C2 = CommandBytes(5) << 8 Or CommandBytes(4)
                            CP1 = CommandBytes(3)
                        Else
                            CommandName = "GoTo"
                        End If

                    Case 1
                        CommandType = 1
                        OperationCode = CommandBytes(0) << 16 Or CommandBytes(1)
                        If CmdID2 Then
                            ByteSwap(CommandBytes, 4, 2)
                            JumpOperand = BitConverter.ToUInt32(CommandBytes, 2)
                            If IFlagForCompare Then
                                CommandName = "Compare Jump"

                            Else
                                CommandName = "Jump"
                                Select Case JumpValue
                                    Case eJumpValues.CallSS

                                        Dim VMGM_PGCN_Upper As Byte = CommandBytes(2) And 127
                                        Dim VMGM_PGCN_Lower As Byte = CommandBytes(3)
                                        Dim CN_for_RSM As Byte = CommandBytes(4)
                                        Dim DomainID As Byte = CommandBytes(5) >> 6 And 3
                                        Dim MenuID As Byte = CommandBytes(5) And 15

                                        If CN_for_RSM = 127 Then
                                            IsCallSSRSM127 = True
                                            'MsgBox("Invalid command found")
                                        End If

                                    Case eJumpValues.Exit
                                    Case eJumpValues.JumpSS
                                    Case eJumpValues.JumpTT
                                    Case eJumpValues.JumpVTS_PTT
                                    Case eJumpValues.JumpVTS_TT

                                End Select
                            End If
                        Else
                            If IFlagForCompare Then
                                CommandName = "Compare Link"

                            Else
                                CommandName = "Link"

                            End If
                        End If

                    Case 2
                        CommandType = 2
                        OperationCode = CommandBytes(0) << 16 Or CommandBytes(1)

                    Case 3
                        CommandType = 2
                        OperationCode = CommandBytes(0) << 16 Or CommandBytes(1)

                    Case 4
                        CommandType = 3
                        OperationCode = CommandBytes(0) << 16 Or ((CommandBytes(1) >> 4) And 15)

                    Case 5
                        CommandType = 3
                        OperationCode = CommandBytes(0) << 16 Or ((CommandBytes(1) >> 4) And 15)

                    Case 6
                        CommandType = 3
                        OperationCode = CommandBytes(0) << 16 Or ((CommandBytes(1) >> 4) And 15)

                    Case 7
                        CommandType = 0
                        CommandName = "Reserved"
                        Exit Select
                End Select

            Catch ex As Exception
                Throw New Exception("Problem with New() cCMD. Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Overrides Function ToString() As String
            Return "test"
        End Function

        Public Enum eCompareValues
            Bitwise
            Equal
            NotEqual
            GreaterThanOrEqualTo
            GreaterThan
            LessThanOrEqualTo
            LessThan
        End Enum

        Public Enum eGoToValues
            [GoTo]
            Break
            SetTmpPML
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

        Public Enum eSetSystemValues
            SetSTN
            SetNVTMR
            SetGPRMMD
            SetAMXMD
            SetHL_BTNN = 6
        End Enum

        Public Enum eSetValues
            Mov
            Swp
            Add
            Sun
            Mul
            Div
            [Mod]
            Rnd
            [And]
            [Or]
            [Xor]
        End Enum

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

        Public Sub New(ByRef IFO() As Byte, ByVal StartAddress As ULong)
            ReDim PGC_AST_CTLs(11)
            Dim Pos As Byte = 0
            For i As Byte = 0 To 11
                PGC_AST_CTLs(i) = New cPGC_AST_CTL(IFO, StartAddress + Pos)
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

    Public Class cCellPlaybackInfo 'C_PBI

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
        Public offFirstVOBUStart As Long
        Public offFirstVOBUEnd As Long
        Public offLastVOBUStart As Long
        Public offLastVOBUEnd As Long

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

                'offFirstVOBUStart = CInt("&h" & CStr(Hex(bytes(Offset + 8)) & Hex(bytes(Offset + 9)) & Hex(bytes(Offset + 10)) & Hex(bytes(Offset + 11))))
                'offFirstVOBUEnd = CInt("&h" & CStr(Hex(bytes(Offset + 12)) & Hex(bytes(Offset + 13)) & Hex(bytes(Offset + 14)) & Hex(bytes(Offset + 15))))
                'offLastVOBUStart = CInt("&h" & CStr(Hex(bytes(Offset + 16)) & Hex(bytes(Offset + 17)) & Hex(bytes(Offset + 18)) & Hex(bytes(Offset + 19))))
                'offLastVOBUEnd = CInt("&h" & CStr(Hex(bytes(Offset + 20)) & Hex(bytes(Offset + 21)) & Hex(bytes(Offset + 22)) & Hex(bytes(Offset + 23))))

                ByteSwap(bytes, 4, Offset + 8)
                offFirstVOBUStart = BitConverter.ToUInt32(bytes, Offset + 8)
                ByteSwap(bytes, 4, Offset + 12)
                offFirstVOBUEnd = BitConverter.ToUInt32(bytes, Offset + 12)
                ByteSwap(bytes, 4, Offset + 16)
                offLastVOBUStart = BitConverter.ToUInt32(bytes, Offset + 16)
                ByteSwap(bytes, 4, Offset + 20)
                offLastVOBUEnd = BitConverter.ToUInt32(bytes, Offset + 20)

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

        'OLD WAY
        'Public Sub New(ByVal bytes() As Byte, ByVal Offset As UInt32, ByVal CellNum As UInt32, ByVal VTSNum As UInt32, ByVal PGCNum As UInt32)
        '    Try
        '        CellNumber = CellNum
        '        VTSNumber = VTSNum
        '        PGCNumber = PGCNum

        '        Dim binByte1 As String = DecimalToBinary(bytes(Offset))
        '        If binByte1.Length < 8 Then
        '            Select Case binByte1.Length
        '                Case 7
        '                    binByte1 = "0" & binByte1
        '                Case 6
        '                    binByte1 = "00" & binByte1
        '                Case 5
        '                    binByte1 = "000" & binByte1
        '                Case 4
        '                    binByte1 = "0000" & binByte1
        '                Case 3
        '                    binByte1 = "00000" & binByte1
        '                Case 2
        '                    binByte1 = "000000" & binByte1
        '                Case 1
        '                    binByte1 = "0000000" & binByte1
        '            End Select
        '        End If

        '        Select Case Microsoft.VisualBasic.Left(binByte1, 2)
        '            Case "00"
        '                CellType = "Normal"
        '            Case "01"
        '                CellType = "First of angle block"
        '            Case "10"
        '                CellType = "Middle of angle block"
        '            Case "11"
        '                CellType = "Last of angle block"
        '        End Select

        '        Select Case Mid(binByte1, 3, 2)
        '            Case "00"
        '                BlockType = "Normal"
        '            Case "01"
        '                BlockType = "Angle"
        '        End Select

        '        Select Case Mid(binByte1, 5, 1)
        '            Case "0"
        '                SeamlessPlaybackLinkedInPCI = False
        '            Case "1"
        '                SeamlessPlaybackLinkedInPCI = True
        '        End Select

        '        Select Case Mid(binByte1, 6, 1)
        '            Case "0"
        '                Interleaved = False
        '            Case "1"
        '                Interleaved = True
        '        End Select

        '        Select Case Mid(binByte1, 7, 1)
        '            Case "0"
        '                SCRDiscontinuity = False
        '            Case "1"
        '                SCRDiscontinuity = True
        '        End Select

        '        Select Case Mid(binByte1, 8, 1)
        '            Case "0"
        '                SeamlessAngleLinkedInDSI = False
        '            Case "1"
        '                SCRDiscontinuity = True
        '        End Select

        '        CellStillTime = CInt("&h" & CStr(Hex(bytes(Offset + 2))))
        '        CellCommandNumber = CInt("&h" & CStr(Hex(bytes(Offset + 3))))

        '        Dim tByteA(3) As Byte
        '        System.Array.Copy(bytes, Offset + 4, tByteA, 0, 4)
        '        CellPlaybackTime = GetTimecodeFromBytes(tByteA)
        '        offFirstVOBUStart = CInt("&h" & CStr(Hex(bytes(Offset + 8)) & Hex(bytes(Offset + 9)) & Hex(bytes(Offset + 10)) & Hex(bytes(Offset + 11))))
        '        offFirstVOBUEnd = CInt("&h" & CStr(Hex(bytes(Offset + 12)) & Hex(bytes(Offset + 13)) & Hex(bytes(Offset + 14)) & Hex(bytes(Offset + 15))))
        '        offLastVOBUStart = CInt("&h" & CStr(Hex(bytes(Offset + 16)) & Hex(bytes(Offset + 17)) & Hex(bytes(Offset + 18)) & Hex(bytes(Offset + 19))))
        '        offLastVOBUEnd = CInt("&h" & CStr(Hex(bytes(Offset + 20)) & Hex(bytes(Offset + 21)) & Hex(bytes(Offset + 22)) & Hex(bytes(Offset + 23))))
        '    Catch ex As Exception
        '        Throw New Exception("Problem with New cCellPlaybackInfo. Error: " & ex.Message, ex)
        '    End Try
        'End Sub

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
        Public CodingMode As eSubCodingMode
        Public LangSpecified As Boolean
        Public LangCode() As Byte
        Public LangCodeEx As eSubLangCodeEx
    End Class

    Public Enum eSubCodingMode
        TwoBitRLE = 0
    End Enum

    Public Enum eSubLangCodeEx
        not_specified = 0
        normal = 1
        large = 2
        children = 3
        normal_captions = 5
        large_captions = 6
        childrens_captions = 7
        forced = 9
        director_comments = 13
        large_director_comments = 14
        director_comments_for_children = 15
    End Enum

#End Region 'Subpicture Attributes

#Region "Audio Attributes"

    Public Class cTitleAudioAttributes
        Public StreamCount As Integer
        Public CodingMode As eAudCodingMode
        Public MultiChEx As Short
        Public LangType As Short
        Public Mode As Short
        Public Quantization As Short
        Public SampleRate As Short
        Public LangCode As Short
        Public LangCodeEx As Short
        Public ChAssignments As Short
        Public KaraokeVersion As Short
        Public MCIntro As Short
        Public SoloDuet As Short
        Public DolbySurOk As Boolean
    End Class

    Public Class cMenuAudioAttributes
        Public CodingMode As eAudCodingMode
        Public Quantization As eAudioQuantization
        Public SampleRate As eAudioSampleRate
    End Class

    Public Enum eAudCodingMode
        AC3 = 0
        Unknown = 1
        Mpeg_1 = 2
        Mpeg_2 = 3
        LPCM = 4
        Unknown2 = 5
        DTS = 6
        Unknown3 = 7
    End Enum

    Public Enum eAudioQuantization
        _16bps = 0
        _20bps = 1
        _24bps = 2
        Dynamic_Range_Control = 3
    End Enum

    Public Enum eAudioSampleRate
        _48Ksps = 0
        _96Ksps = 1
    End Enum

#End Region 'Audio Attributes

#Region "Video Attributes"

    Public Class cVideoAttributes
        Public CodingMode As eCodingMode
        Public VideoStandard As eVideoStandard
        Public AspectRatio As eAspectRatio
        Public PanScanAllowed As Boolean
        Public LetterboxAllowed As Boolean
        Public Line21Field1 As Boolean
        Public Line21Field2 As Boolean
        Public EncodingMode As eEncodingMode
        Public Resolution As eResolution
        Public Letterboxed As Short
        Public Film As eFilm
        Public Bitrate As eBitrate
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

    Public Enum eVideoStandard
        NTSC = 0
        PAL = 1
    End Enum

    Public Enum eCodingMode
        MPEG1 = 0
        MPEG2 = 1
    End Enum

#End Region 'Video Attributes

#Region "NonSeamlessCells"

    Public Class cNonSeamlessCell
        Public ID As Short
        Public VTS As Short
        Public VTS_TT As Short
        Public PTT As Short
        Public PGC As Short
        Public PGn As Short
        Public OutOfnPrograms As Short
        Public Cell As Short
        Public OutOfnCells As Short
        Public Timecode As cTimecode
        Public LBTC As String
        Public tcLB As cTimecode
        Public GTTn As Short
        Public Executed As Boolean
        Public ConfirmedLayerbreak As Boolean
        Public CandidateLayerbreak As Boolean
        Public SourceTimeCode As cTimecode

        Public Sub New()
            LBTC = "N/A"
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
                Return "[GTT] " & GTTn & " [VTS] " & VTS & " [VTS_TT] " & VTS_TT & " [PTT] " & PTT & " [PGC] " & PGC & " [PG] " & PGn & " [Cell] " & Cell & " [TC] " & tLBTC 'Replace(LBTC, "/ 30fps", "", 1, -1, CompareMethod.Text)
            Catch ex As Exception
                MsgBox("Problem with NSC to string. Error: " & ex.Message)
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
                MsgBox("Problem with LBTCToString. Error: " & ex.Message)
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

        Public Sub Remove(ByVal Item As cNonSeamlessCell)
            MyBase.List.Remove(Item)
        End Sub

    End Class

#End Region    'Layerbreaks

    Public Class cGlobalTitleParentalManagement

        Public Titles() As cParentalManagement_US

        Public Sub New(ByRef GTTs As colGlobalTTs)
            ReDim Titles(GTTs.Count - 1)
            For i As Short = 0 To GTTs.Count - 1
                Titles(i) = GTTs(i).ParentalManagementValue
            Next
        End Sub

    End Class
#End Region 'Classes

#Region "Shared Helper Functions"

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

        Public Function GetStringFromBytes(ByVal Bytes() As Byte) As String
            Try
                Bytes = RemoveExtraBytesFromArray(Bytes, False)
                Dim Out As String = System.Text.Encoding.ASCII.GetString(Bytes)
                If Out.ToString.Length = 0 Then
                    Out = ""
                End If
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
                MsgBox("Problem with GetByte. Error: " & ex.Message)
                Return ""
            End Try
        End Function

        Public Function GetVideoAttsFromBytes(ByVal Bytes() As Byte) As cVideoAttributes
            Try
                Dim Out As New cVideoAttributes

                Dim Upper As String = DecimalToBinary(CInt(Bytes(0)))
                Upper = PadString(Upper, 8, "0", True)
                Out.CodingMode = Mid(Upper, 1, 2)
                Out.VideoStandard = Mid(Upper, 3, 2)
                Out.AspectRatio = Mid(Upper, 5, 2)
                If Out.AspectRatio = 11 Then
                    Out.AspectRatio = eAspectRatio.ar16x9
                End If
                Out.PanScanAllowed = Mid(Upper, 7, 1)
                Out.LetterboxAllowed = Mid(Upper, 8, 1)

                Dim Lower As String = DecimalToBinary(CInt(Bytes(1)))
                Lower = PadString(Lower, 8, "0", True)
                Out.Line21Field1 = Mid(Lower, 1, 1)
                Out.Line21Field2 = Mid(Lower, 2, 1)
                Out.Bitrate = Mid(Lower, 3, 1)
                Out.Resolution = Mid(Lower, 4, 2)
                Out.LetterboxAllowed = Mid(Lower, 6, 1)
                'Out.Resrved = Mid(Lower, 7, 1)
                Out.Film = Mid(Lower, 8, 1)

                Return Out
            Catch ex As Exception
                MsgBox("problem getting video atts from bytes. Error: " & ex.Message)
            End Try
        End Function

        Public Function GetMenuAudioAttsFromBytes(ByVal Bytes() As Byte) As cMenuAudioAttributes
            Try
                Dim out As New cMenuAudioAttributes
                out.CodingMode = (Bytes(0) >> 5) And 7
                out.Quantization = (Bytes(1) >> 6) And 3
                out.SampleRate = (Bytes(1) >> 4) And 2
                Return out
            Catch ex As Exception
                Throw New Exception("Problem with GetMenuAudioAttsFromBytes(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function GetTitleAudioAttsFromBytes(ByVal Bytes() As Byte) As cTitleAudioAttributes
            Try
                Dim out As New cTitleAudioAttributes
                With out
                    .ChAssignments = 0
                    .CodingMode = eAudCodingMode.Unknown
                    .DolbySurOk = True
                    .KaraokeVersion = 0
                    .LangCode = 0
                    .LangCodeEx = 0
                    .LangType = 0
                    .MCIntro = 0
                    .MultiChEx = 0
                    .Quantization = 0
                    .SampleRate = 0
                    .SoloDuet = 0
                    .StreamCount = 0
                End With
                Return out
            Catch ex As Exception
                Throw New Exception("Problem with GetTitleAudioAttsFromBytes(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function GetSubpictureAttsFromBytes(ByVal Bytes() As Byte) As cSubpictureAttributes
            Try
                Dim out As New cSubpictureAttributes
                out.CodingMode = (Bytes(0) >> 5) And 7
                out.LangSpecified = (Bytes(0) >> 0) And 3
                ReDim out.LangCode(1)
                out.LangCode(0) = Bytes(2)
                out.LangCode(1) = Bytes(3)
                out.LangCodeEx = Bytes(5)
                Return out
            Catch ex As Exception
                MsgBox("problem getting subpicture atts from bytes. Error: " & ex.Message)
            End Try
        End Function

        Public Class cSMTBitshifter
            'designed only for upto four bytes
            Public Val As Integer

            Public Sub New(ByVal ValCnt As Integer, ByVal v1 As Integer, ByVal v2 As Integer, Optional ByVal v3 As Integer = 0, Optional ByVal v4 As Integer = 0)
                SetVal(ValCnt, v1, v2, v3, v4)
            End Sub

            Public Sub New()
            End Sub

            Public Function SetVal(ByVal ValCnt As Short, ByVal v1 As Integer, ByVal v2 As Integer, Optional ByVal v3 As Integer = 0, Optional ByVal v4 As Integer = 0) As Integer
                Try
                    Val = 0
                    ValCnt -= 1
                    Val = v1 << (ValCnt * 8)
                    If ValCnt > 0 Then
                        Val = Val Or (v2 << ((ValCnt - 1) * 8))
                    End If
                    If ValCnt > 1 Then
                        Val = Val Or (v3 << ((ValCnt - 2) * 8))
                    End If
                    If ValCnt > 2 Then
                        Val = Val Or (v4 << ((ValCnt - 3) * 8))
                    End If
                    Return Val
                Catch ex As Exception
                    MsgBox("Problem with SetVal. Error: " & ex.Message)
                End Try
            End Function
        End Class

        'Public Function GetTimecodeFromBytes(ByRef Bytes() As Byte, ByVal Offset As ULong) As cTimecode
        '    Try
        '        'OLD WAY
        '        'Dim out As New cTimecode

        '        ''DO NOT REMOVE HEXs HERE, THEY ARE CORRECT
        '        'out.Hours = Hex(Bytes(Offset))
        '        'out.Minutes = Hex(Bytes(Offset + 1))
        '        'out.Seconds = Hex(Bytes(Offset + 2))

        '        'Dim binFrames As String = DecimalToBinary(Bytes(Offset + 3))

        '        'Select Case (Bytes(Offset + 3) >> 6) And 3 'Microsoft.VisualBasic.Left(binFrames, 2)
        '        '    Case "11"
        '        '        out.Framerate = "30fps"
        '        '    Case "10"
        '        '        out.Framerate = "U/A"
        '        '    Case "01"
        '        '        out.Framerate = "25fps"
        '        '    Case "00"
        '        '        out.Framerate = "U/A"
        '        'End Select
        '        ''Dim F As Integer = BinaryToDecimal(Microsoft.VisualBasic.Right(binFrames, 6))
        '        'Dim F2 As Integer = Bytes(Offset + 3) And 15
        '        'Dim F1 As Integer = ((Bytes(Offset + 3) >> 4) And 3) * 10
        '        'out.Frames = F1 + F2
        '        'Return out
        '    Catch ex As Exception
        '        Throw New Exception("Problem with GetTimecodeFromBytes(). Error: " & ex.Message, ex)
        '    End Try
        'End Function

        Public Function GetProgramMap(ByVal Bytes() As Byte) As colProgramMap
            Try
                Dim PM As New colProgramMap
                For i As Short = 0 To Bytes.Length - 1
                    PM.Add(New cProgramMap(i, CInt("&h" & CStr(Hex(Bytes(i))))))
                Next
                Return PM
            Catch ex As Exception
                MsgBox("Problem with GetProgramMap. Error: " & ex.Message)
                Return Nothing
            End Try
        End Function

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
                MsgBox("Problem with LookupCellNo. Error: " & ex.Message)
            End Try
        End Function

        Public Function ConvertCellTocNonSeamless(ByVal Cell As cCellPlaybackInfo, ByRef VTS As cVTS) As cNonSeamlessCell
            Dim Out As New cNonSeamlessCell
            Out.Cell = Cell.CellNumber
            Out.OutOfnCells = VTS.PGCs(Cell.PGCNumber - 1).CellMap.Count
            Out.PGC = Cell.PGCNumber
            Out.PGn = VTS.PGCs(Cell.PGCNumber - 1).CellMap.Item(Cell.CellNumber - 1).PGNo
            Out.OutOfnPrograms = VTS.PGCs(Cell.PGCNumber - 1).CellMap.Item(VTS.PGCs(Cell.PGCNumber - 1).CellMap.Count - 1).PGNo
            Out.PTT = VTS.PGCs(Cell.PGCNumber - 1).CellMap.Item(Cell.CellNumber - 1).PTTNoChNo
            Out.VTS = Cell.VTSNumber
            Out.VTS_TT = Cell.TitleNumber
            Out.Timecode = Cell.CellPlaybackTime
            Return Out
        End Function

#Region "Converstions"

        'Public Function BinaryToDecimal(ByVal Binary As String) As Long
        '    Dim n As Long
        '    Dim s As Integer

        '    For s = 1 To Len(Binary)
        '        n = n + (Mid(Binary, Len(Binary) - s + 1, 1) * (2 ^ (s - 1)))
        '    Next s

        '    Return n
        'End Function

        'Public Function DecimalToBinary(ByVal DecimalNum As Long) As String
        '    Dim tmp As String
        '    Dim n As Long

        '    n = DecimalNum

        '    tmp = Trim(Str(n Mod 2))
        '    n = n \ 2

        '    Do While n <> 0
        '        tmp = Trim(Str(n Mod 2)) & tmp
        '        n = n \ 2
        '    Loop

        '    Return tmp
        'End Function

        'Function DecToBin(ByVal iDecimalNum As Double, Optional ByVal iLen As Integer = 0) As String
        '    Try
        '        Dim sBinOut As String
        '        If iLen = 0 Then iLen = 8

        '        Do Until Int(iDecimalNum) = 0
        '            If iDecimalNum Mod 2 Then
        '                sBinOut = "1" & sBinOut
        '                iDecimalNum = iDecimalNum - 1
        '            Else
        '                sBinOut = "0" & sBinOut
        '            End If
        '            iDecimalNum = iDecimalNum / 2
        '        Loop

        '        For A As Integer = 1 To (iLen - Len(sBinOut))
        '            sBinOut = "0" & sBinOut
        '        Next A
        '        Return sBinOut
        '    Catch ex As Exception
        '        MsgBox("problem converting dec to bin. error: " & ex.Message)
        '        Return "FAIL"
        '    End Try
        'End Function

        'Public Function PadString(ByVal sText As String, ByVal iSize As Integer, ByVal sChar As String, ByVal PadLeft As Boolean) As String
        '    Dim CharactersToAdd As Integer = iSize - sText.Length
        '    If CharactersToAdd < 0 Then Return sText
        '    For i As Short = 1 To CharactersToAdd
        '        If PadLeft Then
        '            sText = sChar & sText
        '        Else
        '            sText &= sChar
        '        End If
        '    Next
        '    Return sText
        'End Function

#End Region 'Converstions

    End Module

#End Region 'Shared Helper Functions

    Public Class IFOParseException
        Inherits System.ApplicationException

        Public Sub New(ByVal message As String, ByVal inner As Exception)
            MyBase.New(message, inner)
        End Sub

    End Class

End Namespace 'Media.DVD.IFOProcessing
