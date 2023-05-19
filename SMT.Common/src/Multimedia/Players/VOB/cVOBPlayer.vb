Imports SMT.HardwareControl.Serial
Imports SMT.DotNet.Utility
Imports Elecard.ModuleConfigInterface
Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.Win32

Namespace Multimedia.Players.VOB

    Public Class cVOBPlayer
        ''Inherits cBasePlayer

        'Public Function BuildGraph(ByVal FilePath As String, ByVal NotifyWindowHandle As Integer) As Boolean
        '    Try
        '        Dim HR As Integer
        '        If Not DestroyGraph("Build VOB Graph") Then Throw New Exception("Failed to destroy graph.")
        '        GraphBuilder = DsBugWO.CreateDsInstance(Clsid.FilterGraphManager, IGraphBuilder_GUID)
        '        AddGraphToROT(GraphBuilder)
        '        MediaCtrl = CType(GraphBuilder, IMediaControl)
        '        Seek = CType(GraphBuilder, IMediaSeeking)
        '        MediaEvt = CType(GraphBuilder, IMediaEvent)
        '        MediaEvt.SetNotifyWindow(NotifyWindowHandle, EC_USER, IntPtr.Zero)

        '        'File source filter
        '        AddFilesourceA(GraphBuilder)

        '        HR = iFSFA.Load(FilePath, 0)
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '        HR = FileSourceA.FindPin("Output", FSFA_Out)
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '        'moonlight-elecard Demuxer
        '        AddMCE_DMXA(GraphBuilder)

        '        ''MS MPEG-2 Splitter
        '        'mpeg2splitter = CType(DsBugWO.CreateDsInstance(New Guid("3AE86B20-7BE8-11D1-ABE6-00A0C905F375"), ibasefilter_guid), IBaseFilter)
        '        'HR = graphbuilder.AddFilter(mpeg2splitter, "M2 Splitter")
        '        'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '        'mpeg2splitter.FindPin("Input", M2Sp_In)

        '        'Video Decoder
        '        AddNVidiaVideoDecoder(GraphBuilder)

        '        'Audio Decoder
        '        AddNVidiaAudioDecoder(GraphBuilder, False)

        '        'AMTC
        '        AddSequoyanAMTC(GraphBuilder)

        '        'Keystone
        '        AddKeySD(GraphBuilder)

        '        'DLV Renderer
        '        AddDeckLinkVideo(GraphBuilder)

        '        'Decklink Audio Renderer
        '        AddDeckLinkAudio(GraphBuilder)

        '        'Connect filters
        '        HR = GraphBuilder.Connect(FSFA_Out, MCE_DMXA_In)
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '        'Get MEDemux Output pins
        '        Dim PinEnum As IEnumPins

        '        HR = MCE_DMXA.EnumPins(PinEnum)
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '        Dim cnt As Integer
        '        Dim Pins() As Integer
        '        ReDim Pins(0)
        '        HR = PinEnum.Next(1, Pins, cnt)
        '        If HR <> 0 Then Marshal.ThrowExceptionForHR(HR)
        '        Dim OutPinNo As Short = 0
        '        Dim OutPinID(-1) As String
        '        Dim ptr As IntPtr
        '        Dim tPin As IPin
        '        While HR = 0 And cnt > 0
        '            ptr = New IntPtr(CInt(Pins(0)))
        '            Dim obj As Object = Marshal.GetObjectForIUnknown(ptr)
        '            tPin = CType(obj, IPin)
        '            Dim PinID As String
        '            HR = tPin.QueryId(PinID)
        '            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '            'Debug.WriteLine(PinID)
        '            ReDim Preserve OutPinID(UBound(OutPinID) + 1)
        '            OutPinID(OutPinNo) = PinID
        '            OutPinNo += 1
        '            HR = PinEnum.Next(1, Pins, cnt)
        '        End While

        '        For s As Short = 1 To UBound(OutPinID)
        '            If InStr(OutPinID(s).ToLower, "video", CompareMethod.Text) Then
        '                HR = MCE_DMXA.FindPin(OutPinID(s), MCE_DMXA_Out)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                Exit For
        '            End If
        '        Next
        '        For s As Short = 2 To UBound(OutPinID)
        '            If InStr(OutPinID(s).ToLower, "AC3", CompareMethod.Text) Then
        '                HR = MCE_DMXA.FindPin(OutPinID(s), MCE_DMXA_Out_Aud)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                Exit For
        '            End If
        '        Next

        '        If Not MCE_DMXA_Out_Aud Is Nothing Then
        '            HR = GraphBuilder.Connect(MCE_DMXA_Out_Aud, AudDec_InPin)
        '            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '            HR = GraphBuilder.Connect(AudDec_OutPin, AMTC_In)
        '            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '            HR = GraphBuilder.Connect(AMTC_Out, AudRen_In)
        '            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '        End If

        '        HR = GraphBuilder.Connect(MCE_DMXA_Out, VidDec_Vid_In)
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '        'HR = graphbuilder.Connect(fsf_out, M2Sp_In)
        '        'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '        'mpeg2splitter.FindPin("Video", M2Sp_VidOut)
        '        'mpeg2splitter.FindPin("AC3", M2Sp_AudOut)

        '        'HR = graphbuilder.Connect(M2Sp_AudOut, AudDec_InPin)
        '        'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '        'HR = graphbuilder.Connect(M2Sp_VidOut, VidDec_Vid_In)
        '        'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '        HR = GraphBuilder.Connect(VidDec_Vid_Out, KO_In)
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '        HR = GraphBuilder.Connect(KO_Out, DLV_In)
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

        '        'Dim ds As String = MakeTwoDig(Duration)
        '        'Me.lblDuration.Text = dh & ":" & dm & ":" & ds
        '        Return ParseVOB(FilePath)
        '    Catch ex As Exception
        '        Throw New Exception("Problem with BuildVOBGraph. Error: " & ex.Message)
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
