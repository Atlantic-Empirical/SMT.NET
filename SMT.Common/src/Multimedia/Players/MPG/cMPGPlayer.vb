Imports SMT.HardwareControl.Serial
Imports SMT.DotNet.Utility
Imports Elecard.ModuleConfigInterface
Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.Win32

Namespace Multimedia.Players.MPG

    Public Class cMPGPlayer
        ''Inherits cBasePlayer

        'Public Function BuildGraph(ByVal FilePath As String) As Boolean
        '    Try
        '        Dim HR As Integer
        '        If Not DestroyGraph("Build Video Graph") Then Throw New Exception("Failed to destroy graph.")
        '        GraphBuilder = Nothing
        '        GraphBuilder = DsBugWO.CreateDsInstance(Clsid.FilterGraphManager, IGraphBuilder_GUID)
        '        AddGraphToROT(GraphBuilder)
        '        MediaCtrl = CType(GraphBuilder, IMediaControl)

        '        'File source filter
        '        AddFilesourceA(GraphBuilder)

        '        HR = iFSFA.Load(FilePath, 0)
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '        HR = FileSourceA.FindPin("Output", FSFA_Out)
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '        ''Demuxer
        '        'MEDemux = CType(DsBugWO.CreateDsInstance(New Guid("731B8592-4001-46D4-B1A5-33EC792B4501"), ibasefilter_guid), IBaseFilter)
        '        'HR = graphbuilder.AddFilter(MEDemux, "ME-DX")
        '        'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '        'MS MPEG-2 Splitter
        '        MPEG2Splitter = CType(DsBugWO.CreateDsInstance(New Guid("3AE86B20-7BE8-11D1-ABE6-00A0C905F375"), IBaseFilter_GUID), IBaseFilter)
        '        HR = GraphBuilder.AddFilter(MPEG2Splitter, "M2 Splitter")
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '        'Decoder
        '        AddNVidiaVideoDecoder(GraphBuilder)

        '        'Keystone
        '        AddKeySD(GraphBuilder)

        '        'DLV Renderer
        '        AddDeckLinkVideo(GraphBuilder)

        '        ''Add VMR9
        '        'AddVMR9(graphbuilder, 1)

        '        HR = GraphBuilder.Render(FSFA_Out)
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '        Seek = CType(GraphBuilder, IMediaSeeking)
        '        'Dim SC As SeekingCapabilities
        '        'seek.GetCapabilities(SC)
        '        'If SC And SeekingCapabilities.CanGetStopPos Then
        '        '    Throw New Exception("hi")
        '        'End If

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

        '        'Dim ds As String = MakeTwoDig(Duration)
        '        'lblDuration.Text = dh & ":" & dm & ":" & ds
        '        'Return True
        '    Catch ex As Exception
        '        Throw New Exception("Problem with BuildVideoGraph. Error: " & ex.Message)
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

    End Class

End Namespace
