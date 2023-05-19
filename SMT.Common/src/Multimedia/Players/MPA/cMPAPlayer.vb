Imports SMT.HardwareControl.Serial
Imports SMT.DotNet.Utility
Imports Elecard.ModuleConfigInterface
Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.Win32

Namespace Multimedia.Players.MPA

    Public Class cMPAPlayer
        ''Inherits cBasePlayer

        'Public Function BuildGraph(ByVal FilePath As String) As Boolean
        '    Try
        '        Dim HR As Integer
        '        'RemoveAllFiltersFromGraph(graphbuilder)
        '        If Not DestroyGraph("Build Audio Graph") Then Throw New Exception("Failed to destroy graph.")
        '        GraphBuilder = DsBugWO.CreateDsInstance(Clsid.FilterGraphManager, IGraphBuilder_GUID)
        '        AddGraphToROT(GraphBuilder)
        '        MediaCtrl = CType(GraphBuilder, IMediaControl)
        '        Seek = CType(GraphBuilder, IMediaSeeking)

        '        'Gabest MPA source filter
        '        FileSourceA = CType(DsBugWO.CreateDsInstance(New Guid("59A0DB73-0287-4C9A-9D3C-8CFF39F8E5DB"), IBaseFilter_GUID), IBaseFilter)
        '        HR = GraphBuilder.AddFilter(FileSourceA, "MPA Source")
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '        iFSFA = CType(FileSourceA, IFileSourceFilter)

        '        HR = iFSFA.Load(FilePath, 0)
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '        'EnumeratePinNames(filesource)
        '        HR = FileSourceA.FindPin("Audio", FSFA_Out)
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '        'MPEG Audio Decoder
        '        MSMPADecoder = CType(DsBugWO.CreateDsInstance(New Guid("4A2286E0-7BEF-11CE-9BD9-0000E202599C"), IBaseFilter_GUID), IBaseFilter)
        '        GraphBuilder.AddFilter(MSMPADecoder, "MPA Decoder")
        '        'EnumeratePinNames(MSMPADecoder)
        '        MSMPADecoder.FindPin("In", MPA_Dec_In)
        '        MSMPADecoder.FindPin("Out", MPA_Dec_Out)

        '        'ACM Wrapper
        '        ACMWrapper = CType(DsBugWO.CreateDsInstance(New Guid("6A08CF80-0E18-11CF-A24D-0020AFD79767"), IBaseFilter_GUID), IBaseFilter)
        '        GraphBuilder.AddFilter(ACMWrapper, "ACM-W")
        '        'EnumeratePinNames(ACMWrapper)
        '        ACMWrapper.FindPin("In", ACMW_In)
        '        ACMWrapper.FindPin("Out", ACMW_Out)

        '        'Decklink Audio Renderer
        '        AddDeckLinkAudio(GraphBuilder)

        '        'Video Setup
        '        'NVS = fd501075-8ebe-11ce-8183-00aa00577da1
        '        NVS = CType(DsBugWO.CreateDsInstance(New Guid("FD501075-8EBE-11CE-8183-00AA00577DA1"), IBaseFilter_GUID), IBaseFilter)
        '        GraphBuilder.AddFilter(NVS, "NVS")
        '        'EnumeratePinNames(NVS)
        '        NVS.FindPin("1", NVS_Out)

        '        AddKeySD(GraphBuilder)

        '        'DLV
        '        AddDeckLinkVideo(GraphBuilder)

        '        'connect pins
        '        HR = GraphBuilder.Connect(NVS_Out, KO_In)
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '        HR = GraphBuilder.Connect(KO_Out, DLV_In)
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '        'End Video Setup

        '        HR = GraphBuilder.Connect(FSFA_Out, MPA_Dec_In)
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '        HR = GraphBuilder.Connect(MPA_Dec_Out, ACMW_In)
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '        HR = GraphBuilder.Connect(ACMW_Out, AudRen_In)
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)


        '        'Dim Duration As Long = 0
        '        'While Duration = 0
        '        '    HR = Seek.GetDuration(Duration) '11844600000
        '        '    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '        'End While
        '        'Me.tbPlayPosition.Maximum = Duration / 10000000
        '        'Me.tbPlayPosition.TickFrequency = Me.tbPlayPosition.Maximum / 100
        '        'Me.tbPlayPosition.LargeChange = 10
        '        'Me.tbPlayPosition.SmallChange = 1

        '        'Duration /= 10000000 'Get total seconds

        '        'Dim tmp As String
        '        'Dim dh As String
        '        'If Duration > 3600 Then
        '        '    tmp = CStr(Duration / 3600)
        '        '    dh = MakeTwoDig(Microsoft.VisualBasic.Left(tmp, 1))
        '        '    Duration -= (dh * 3600)
        '        'Else
        '        '    dh = "00"
        '        'End If

        '        'Dim dm As String
        '        'If Duration > 60 Then
        '        '    tmp = CStr(Duration / 60)
        '        '    Dim t() As String = Split(tmp, ".", -1, CompareMethod.Text)
        '        '    dm = MakeTwoDig(t(0))
        '        '    Duration -= (dm * 60)
        '        'Else
        '        '    dm = "00"
        '        'End If

        '        'RenderBitmap(Me.CreateAudioPBBMP(False), "Build DTSAC3 Graph", 0, 0)
        '        'Me.AudioScreensaver.Start()

        '        'Dim ds As String = MakeTwoDig(Duration)
        '        'lblDuration.Text = dh & ":" & dm & ":" & ds

        '        Return True
        '    Catch ex As Exception
        '        Throw New Exception("Problem with BuildAudioGraph - MPA. Error: " & ex.Message)
        '        Return False
        '    End Try
        'End Function

        ''#Region "cBasePlayer Overrides"

        ''        Public Overrides Function Play() As Boolean

        ''        End Function

        ''        Public Overrides Function Pause() As Boolean

        ''        End Function

        ''        Public Overrides Function [Stop]() As Boolean

        ''        End Function

        ''        Public Overrides Function FastForward(ByVal Rate As Integer) As Boolean

        ''        End Function

        ''        Public Overrides Function Rewind(ByVal Rate As Integer) As Boolean

        ''        End Function

        ''        Public Overrides Function FrameStep(ByVal Rate As Integer) As Boolean

        ''        End Function

        ''        Public Overrides Function Timesearch() As Boolean

        ''        End Function

        ''        Public Overrides Function SetOSD() As Boolean

        ''        End Function

        ''        Public Overrides Function ClearOSD() As Boolean

        ''        End Function

        ''#End Region 'Overrides

        '    Public Sub ShowJPForAudio()
        '        Try
        '            Dim str As Stream = PP.MediaResourcesAssembly.GetManifestResourceStream("SMT.Libraries.MediaResources.Phoenix_SysJP_PAL.bmp")
        '            If str Is Nothing Then
        '                Exit Sub
        '            End If

        '            Dim H As Integer = 0
        '            Dim W As Integer = 720
        '            If PP.CurrentVidStd = SMT.Common.Media.DVD.IFOProcessing.eVideoStandard.NTSC Then
        '                H = 486
        '            Else
        '                H = 576
        '            End If
        '            Dim RasterSize As Integer = H * W * 3
        '            Dim bytes(RasterSize - 1) As Byte
        '            str.Position = 54
        '            str.Read(bytes, 0, RasterSize)

        '            Array.Reverse(bytes)

        '            For s As Short = 0 To H - 1
        '                Array.Reverse(bytes, s * 2160, 2160)
        '            Next

        '            'Get pointer to the RGB24 raster buffer
        '            Dim h1 As GCHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned)
        '            Dim BufferPtr As IntPtr = h1.AddrOfPinnedObject

        '            HR = PP.iKeystoneMixer.put_OSD(BufferPtr.ToInt32, 720, 480, 0, 0, eImageFormat.IF_RGB24, RGB(0, 255, 0), 2147483647)
        '            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '        Catch ex As Exception
        '            PP.AddConsoleLine(eConsoleItemType.ERROR, "Problem with ShowJPForAudio(). Error: " & ex.Message)
        '        End Try
        '    End Sub

    End Class

End Namespace
