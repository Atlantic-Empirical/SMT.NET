#Region "IMPORTS"

Imports SMT.HardwareControl.Serial
Imports SMT.Multimedia.DirectShow
Imports SMT.Multimedia.DirectShow.nVidia.DecoderControl
Imports SMT.Multimedia.Filters.SMT.Keystone
Imports SMT.Multimedia.Filters.SMT.AMTC
Imports SMT.Multimedia.Filters.SMT.L21G
Imports SMT.DotNet.Utility
Imports SMT.Multimedia.DirectShow.BlackMagic
Imports SMT.Multimedia.Utility.TimecodeMath
Imports Elecard.ModuleConfigInterface
Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.Win32
Imports System.Drawing
Imports System.Windows.Forms
Imports SMT.Multimedia.Enums
Imports SMT.Multimedia.Formats.StillImage
Imports SMT.Multimedia.GraphConstruction
Imports SMT.Multimedia.Players.Classes
Imports SMT.Multimedia.DirectShow.SMT_DShow
Imports SMT.Multimedia.DirectShow.MainConcept
Imports SMT.Win.WinAPI.Constants
Imports System.Xml.Serialization
Imports SMT.Multimedia.Classes
Imports SMT.Multimedia.Utility.Timecode
Imports SMT.Multimedia.Filters.MainConcept
Imports SMT.Multimedia.Filters.nVidia

#End Region 'IMPORTS

Namespace Multimedia.Players.M2TS

    Public Class cM2TSPlayer
        Inherits cBasePlayer

#Region "CBASEPLAYER OVERRIDES"

        Public Overloads Function BuildGraph(ByVal nFilePath As String, ByVal nNotifyWindowHandle As Integer, ByVal nAVMode As eAVMode, ByVal nParentForm As cSMTForm, ByVal Pid_Video As Integer, ByVal Pid_Audio As Integer, ByVal Pid_IG As Integer, ByVal Pid_PG As Integer) As Boolean
            Try
                Debug.WriteLine("M2TS PLAYER | BuildGraph()")
                Dim t1 As Long = DateTime.Now.Ticks

                If Not FilterCheck() = "True" Then Return False

                NotifyWindowHandle = nNotifyWindowHandle
                ParentForm = nParentForm
                FilePath = nFilePath
                AVMode = nAVMode

                Graph = New cSMTGraph(NotifyWindowHandle)

                Graph.AddGraphToROT()

                'DEBUGGING
                'Graph.SetSyncSource_NULL()
                'DEBUGGING

                '==========================================================
                ' ADD THE FSF AND DEMUX. HOOK THEM UP.

                If Not Graph.AddFilesourceA(FilePath) Then Return False
                If Not Graph.AddMCE_DMXA() Then Return False

                ' SETUP DEMUX
                Dim b As Boolean
                b = Graph.MCE_IMC_DMXA.SetParamValue(ModuleConfig_Consts.DMX_INIT_MODE, 2)
                b = Graph.MCE_IMC_DMXA.SetParamValue(ModuleConfig_Consts.EMPGDMX_DIRECT_PTS, 1)
                b = Graph.MCE_IMC_DMXA.SetParamValue(ModuleConfig_Consts.EMPGDMX_INDEX_MODE, 3)
                Graph.MCE_IMC_DMXA.CommitChanges()

                ' BUILD INDEX FILE IF NEEDED
                IndexFileCheck(FilePath)

                ' CONNECT FSF & DEMUX
                HR = Graph.GraphBuilder.Connect(Graph.FSFA_Out, Graph.MCE_DMXA_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'Dim SDI As Object = cSMTGraph.MCE_DMX_GetStreamDurationInfo(Graph.MCE_DMXA)


                '==========================================================
                ' ENUMERATE THE PINS ON THE DEMUX.
                ' ADD AND CONNECT THE NEEDED DECODERS

                Dim DemuxPins As List(Of IPin) = Graph.GetPins(Graph.MCE_DMXA)

                _Streams = New List(Of cStreamInfo)
                Dim tStr As cStreamInfo

                For Each P As IPin In DemuxPins
                    tStr = New cStreamInfo(P, Graph)

                    If InStr(tStr.PinName.ToLower, "h264") Then
                        tStr.Format = cStreamInfo.eStreamFormat.h264
                        tStr.Decoder = Me.AddFilter_AVCDecoder(Graph.GraphBuilder)
                        HR = Graph.GraphBuilder.Connect(P, tStr.Decoder.Pin_In)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        tStr.Decoder.SetupOutPin()
                        tStr.Renderer = AddFilter_VMR9(1)
                        HR = Graph.GraphBuilder.Connect(tStr.Decoder.Pin_Out, tStr.Renderer.Pin_In)
                        tStr.SetupEx()
                    End If

                    If InStr(tStr.PinName.ToLower, "vc1") Then
                        tStr.Format = cStreamInfo.eStreamFormat.vc1
                        tStr.Decoder = AddFilter_VC1Decoder(Graph.GraphBuilder)
                        HR = Graph.GraphBuilder.Connect(P, tStr.Decoder.Pin_In)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        tStr.Decoder.SetupOutPin()
                        tStr.Renderer = AddFilter_VMR9(1)
                        HR = Graph.GraphBuilder.Connect(tStr.Decoder.Pin_Out, tStr.Renderer.Pin_In)
                        tStr.SetupEx()
                    End If

                    If InStr(tStr.PinName.ToLower, "video") Or InStr(tStr.PinName.ToLower, "ves") Then
                        tStr.Format = cStreamInfo.eStreamFormat.m2v
                        tStr.Decoder = AddFilter_M2VDecoder(Graph.GraphBuilder)
                        HR = Graph.GraphBuilder.Connect(P, tStr.Decoder.Pin_In)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        tStr.Decoder.SetupOutPin()
                        tStr.Renderer = AddFilter_VMR9(1)
                        HR = Graph.GraphBuilder.Connect(tStr.Decoder.Pin_Out, tStr.Renderer.Pin_In)
                        tStr.SetupEx()
                    End If

                    If InStr(tStr.PinName.ToLower, "ac3") Then
                        tStr.Format = cStreamInfo.eStreamFormat.dd
                        tStr.Decoder = AddFilter_NVAudio()
                        HR = Graph.GraphBuilder.Connect(P, tStr.Decoder.Pin_In)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        tStr.AMTC = AddFilter_AMTC()
                        HR = Graph.GraphBuilder.Connect(tStr.Decoder.Pin_Out, Graph.AMTC_In)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        tStr.Renderer = AddFilter_DefaultDirectSoundDevice()
                        HR = Graph.GraphBuilder.Connect(Graph.AMTC_Out, tStr.Renderer.Pin_In)
                        tStr.SetupEx()
                    End If

                    If InStr(tStr.PinName.ToLower, "dts") Then
                        tStr.Format = cStreamInfo.eStreamFormat.dts
                        tStr.Decoder = AddFilter_NVAudio()
                        HR = Graph.GraphBuilder.Connect(P, tStr.Decoder.Pin_In)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        tStr.Renderer = AddFilter_DefaultDirectSoundDevice()
                        HR = Graph.GraphBuilder.Connect(tStr.Decoder.Pin_Out, tStr.Renderer.Pin_In)
                        tStr.SetupEx()
                    End If

                    If InStr(tStr.PinName.ToLower, "aes") Then
                        tStr.Format = cStreamInfo.eStreamFormat.mpa
                        tStr.Decoder = AddFilter_MPA(Graph.GraphBuilder)
                        HR = Graph.GraphBuilder.Connect(P, tStr.Decoder.Pin_In)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        tStr.Renderer = AddFilter_DefaultDirectSoundDevice()
                        HR = Graph.GraphBuilder.Connect(tStr.Decoder.Pin_Out, tStr.Renderer.Pin_In)
                        tStr.SetupEx()
                    End If

                    If InStr(tStr.PinName.ToLower, "hdmv ig") Then
                        tStr.Format = cStreamInfo.eStreamFormat.ig
                        'tStr.Decoder = AddFilter_MPA(Graph.GraphBuilder)
                        'HR = Graph.GraphBuilder.Connect(P, tStr.Decoder.Pin_In)
                        'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    End If

                    If InStr(tStr.PinName.ToLower, "hdmv pg") Then
                        tStr.Format = cStreamInfo.eStreamFormat.pg
                        'tStr.Decoder = AddFilter_MPA(Graph.GraphBuilder)
                        'HR = Graph.GraphBuilder.Connect(P, tStr.Decoder.Pin_In)
                        'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    End If

                    If InStr(tStr.PinName.ToLower, "text st") Then
                        tStr.Format = cStreamInfo.eStreamFormat.tst
                        'tStr.Decoder = AddFilter_MPA(Graph.GraphBuilder)
                        'HR = Graph.GraphBuilder.Connect(P, tStr.Decoder.Pin_In)
                        'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    End If

                    _Streams.Add(tStr)

                Next

                '==========================================================
                '==========================================================
                ' CHECK TO SEE IF THIS IS TEXTST

                If IsTextST Then
                    Return True
                End If

                '==========================================================
                '==========================================================
                ' NOW THAT WE HAVE THE STREAM INFORMATION FOR ALL THE OTHER STREAMS WE
                ' CAN DISCONNECT THE DECODERS AND REMOVE THE UNUSED RENDERERS

                For Each sss As cStreamInfo In Streams

                    'do not remove the user-requested streams

                    Select Case sss.PID
                        Case Pid_Video
                            sss.IsActive = True
                            'needed because renderer is added below
                            RemoveFilter(sss.Renderer.Filter)
                            sss.Renderer.Dispose()

                        Case Pid_Audio
                            sss.IsActive = True

                        Case Pid_IG, Pid_PG
                            'do nothing until we have decoders

                        Case Else
                            If Not sss.Decoder Is Nothing Then 'required until we have graphics stream decoders
                                RemoveFilter(sss.Decoder.Filter)
                                sss.Decoder.Dispose()
                                If Not sss.AMTC Is Nothing Then
                                    RemoveFilter(sss.AMTC.Filter)
                                    sss.AMTC.Dispose()
                                End If
                                If Not sss.Renderer Is Nothing Then RemoveFilter(sss.Renderer.Filter)
                                sss.Renderer.Dispose()
                            End If

                    End Select

                    'If sss.Category = cStreamInfo.eCategories.PrimaryAudio And sss.StreamNumber = 0 Then
                    '    sss.IsActive = True

                    'ElseIf sss.Category = cStreamInfo.eCategories.PrimaryVideo Then
                    '    sss.IsActive = True
                    '    RemoveFilter(sss.Renderer.Filter)
                    '    sss.Renderer.Dispose()
                    'Else
                    '    If Not sss.Decoder Is Nothing Then 'required until we have graphics stream decoders
                    '        RemoveFilter(sss.Decoder.Filter)
                    '        sss.Decoder.Dispose()
                    '        If Not sss.AMTC Is Nothing Then
                    '            RemoveFilter(sss.AMTC.Filter)
                    '            sss.AMTC.Dispose()
                    '        End If
                    '        If Not sss.Renderer Is Nothing Then RemoveFilter(sss.Renderer.Filter)
                    '        sss.Renderer.Dispose()
                    '    End If
                    'End If

                Next

                '==========================================================
                '==========================================================
                ' NOW WE NEED TO PUMP THE PRIMARY VIDEO THROUGH KEYSTONE

                '==========================================================
                ' FIND THE PRIMARY VIDEO STREAM
                'Dim PrimaryVid As cStreamInfo = ActiveVideoStream
                'Dim PrimaryAud As cStreamInfo = ActiveAudioStream

                '==========================================================
                ' SET THE PRIMARY VIDEO DECODER AS THE CURRENTDECODER
                _CurrentDecoder = ActiveVideoStream.Decoder.Filter
                '_CurrentDecoder = PrimaryVid.Decoder.Filter

                '==========================================================
                ' ADD KEYSTONE
                If Not Graph.AddKeystoneOmni(AVMode) Then Return False
                Graph.KO_IKeystone.SendMediaTimeEvents(1, 1)
                'HR = Graph.KO_IKeystone.SetOptimizationLevel(4)

                ''==========================================================
                '' DISCONNECT DECODER & RENDERER
                'HR = Graph.GraphBuilder.Disconnect(ActiveVideoStream.Decoder.Pin_Out)
                ''HR = Graph.GraphBuilder.Disconnect(PrimaryVid.Decoder.Pin_Out)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                '==========================================================
                ' HOOKUP THE PRIMARY VIDEO DECODER TO KEYSTONE
                HR = Graph.GraphBuilder.Connect(ActiveVideoStream.Decoder.Pin_Out, Graph.KO_In)
                'HR = Graph.GraphBuilder.Connect(PrimaryVid.Decoder.Pin_Out, Graph.KO_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                '==========================================================
                ' ADD RENDERER
                If AVMode = eAVMode.DesktopVMR Then
                    'OLD WAY
                    'If Not Graph.AddVMR9(Me.StreamCount_Video) Then Return False
                    'HR = Graph.GraphBuilder.Render(Graph.KO_Out)

                    ''To use Win32 viewer:
                    ''Me.SetupViewer(960, 540, False)

                    ''To use WPF window for viewer:
                    'Graph.VideoWin = CType(Graph.GraphBuilder, IVideoWindow)

                    'NEW WAY
                    ActiveVideoStream.Renderer = AddFilter_VMR9(1)
                    'PrimaryVid.Renderer = AddFilter_VMR9(1)
                    HR = Graph.GraphBuilder.Connect(Graph.KO_Out, ActiveVideoStream.Renderer.Pin_In)
                    'HR = Graph.GraphBuilder.Connect(Graph.KO_Out, PrimaryVid.Renderer.Pin_In)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    Graph.VideoWin = CType(ActiveVideoStream.Renderer.Filter, IVideoWindow)
                    'Graph.VideoWin = CType(PrimaryVid.Renderer.Filter, IVideoWindow)

                Else
                    ''USE DECKLINK

                    ''DLV Renderer
                    'If Not Graph.AddDeckLinkVideo() Then Return False
                    'If Not Graph.AddDeckLinkAudio() Then Return False

                    ''Connect pins
                    'HR = Graph.GraphBuilder.Connect(Graph.FSFA_Out, Graph.MCE_DMXA_In)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    ''HR = Graph.MCE_DMXA.FindPin("Video (PID 481 @ Prog# 2)", Graph.MCE_DMXA_Out)
                    ''If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    ''search for video pin
                    'Graph.MCE_DMXA_Out = Graph.FindPinByPartialName(Graph.MCE_DMXA, "Video (PID")
                    'If Graph.MCE_DMXA_Out Is Nothing Then Throw New Exception("Could not find video pin")

                    'HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXA_Out, Graph.MCE_MP2A_In)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    'HR = Graph.MCE_MP2A.FindPin("Out", Graph.MCE_MP2A_Out)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    'HR = Graph.GraphBuilder.Connect(Graph.MCE_MP2A_Out, Graph.KO_In)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    'HR = Graph.GraphBuilder.Connect(Graph.KO_Out, Graph.DLV_In)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    ''NOW DONE BELOW IN THE SETAUDIO FUNCTION
                    ' ''search for audio pin
                    ''Graph.MCE_DMXA_Out_Aud = Graph.FindPinByPartialName(Graph.MCE_DMXA, "AES3 (PID")
                    ''If Graph.MCE_DMXA_Out_Aud Is Nothing Then Throw New Exception("Could not find audio pin")

                    ''HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXA_Out_Aud, Graph.MC_MPADEC_In)
                    ''If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    ''HR = Graph.MC_MPADEC.FindPin("Out", Graph.MC_MPADEC_Out)
                    ''If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    ''HR = Graph.GraphBuilder.Connect(Graph.MC_MPADEC_Out, Graph.AudRen_In)
                    ''If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                End If

                While _Duration = 0
                    HR = Graph.MediaSeek.GetDuration(_Duration)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End While

                Debug.WriteLine("M2TS PLAYER | BuildGraph() Duration = " & New TimeSpan(DateTime.Now.Ticks - t1).Milliseconds & "ms")

                RaiseEvent_Initialized()
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with M2TS BuildGraph(). Error: " & ex.Message & " StackTrace: " & ex.StackTrace)
            End Try
        End Function

        Public Overrides Function BuildGraph(ByVal FilePath As String, ByVal NotifyWindowHandle As Integer, ByVal nAVMode As eAVMode, ByVal nParentForm As Form) As Boolean
            Throw New Exception("This BuildGraph() is not supported in M2TS player.")
        End Function

        Public Overrides Function Play() As Boolean
            Debug.WriteLine("M2TS PLAYER | Play()")
            Try
                'If Not AudioIsInitialized Then Return False

                Select Case PlayState
                    Case ePlayState.FrameStepping
                        QuitFrameStepping()
                        PlayState = ePlayState.Playing

                    Case ePlayState.Playing
                        Me.Pause()

                    Case ePlayState.FastForwarding, ePlayState.Rewinding
                        'HR = Graph.KO_IKeystone.DeactivateFFRW
                        'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        'HR = Graph.MediaSeek.SetRate(1.0)
                        'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        'nVidia_AudioMute(False)
                        'AudioRenderConnectionToggle(False)
                        CancelShuttle()
                        PlayState = ePlayState.Playing

                    Case ePlayState.Paused
                        Graph.MediaCtrl.Run()
                        PlayState = ePlayState.Playing

                    Case Else
                        HR = Graph.MediaCtrl.Run()
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        PlayState = ePlayState.Playing
                End Select

                CurrentSpeed = eShuttleSpeed.OneX
                If Not Graph.DeckControl Is Nothing Then Graph.DeckControl.Play()

            Catch ex As Exception
                Throw New Exception("Problem with StartPlayback. Error: " & ex.Message)
            End Try
        End Function

        Public Overrides Function Pause() As Boolean
            Debug.WriteLine("M2TS PLAYER | Pause()")
            Try
                HR = Graph.MediaCtrl.Pause()
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                PlayState = ePlayState.Paused
            Catch ex As Exception
                Throw New Exception("Problem with fileplayback-pause. Error: " & ex.Message)
            End Try
        End Function

        Public Overrides Function [Stop]() As Boolean
            Debug.WriteLine("M2TS PLAYER | Stop()")
            If Graph.MediaCtrl Is Nothing Then Exit Function
            HR = Graph.MediaCtrl.Stop()
            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Graph.DestroyGraph()
            PlayState = ePlayState.Stopped
        End Function

        Public Overrides Function FastForward(Optional ByVal Speed As eShuttleSpeed = eShuttleSpeed.Not_Specified) As Boolean

            If Speed = eShuttleSpeed.Not_Specified Then
                Select Case CurrentSpeed
                    Case eShuttleSpeed.OneX
                        CurrentSpeed = eShuttleSpeed.TwoX
                    Case eShuttleSpeed.TwoX
                        CurrentSpeed = eShuttleSpeed.FourX
                    Case eShuttleSpeed.FourX
                        CurrentSpeed = eShuttleSpeed.EightX
                    Case eShuttleSpeed.EightX
                        CurrentSpeed = eShuttleSpeed.SixteenX
                    Case eShuttleSpeed.SixteenX
                        CurrentSpeed = eShuttleSpeed.ThirtyTwoX
                    Case eShuttleSpeed.ThirtyTwoX
                        Debug.WriteLine("32x is top play speed.")
                        Return False
                End Select
            Else
                CurrentSpeed = Speed
            End If

            If CurrentSpeed = eShuttleSpeed.TwoX Then
                ToggleAudioRendererConnection(True) 'kick out the audio renderer
                HR = Graph.KO_IKeystone.ActivateFFRW(CurrentSpeed)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            End If
            If CurrentSpeed = eShuttleSpeed.FourX Then
                SetActiveVideoFrameMode(True) 'switch to I frames only
            End If
            HR = Graph.MediaSeek.SetRate(CurrentSpeed)
            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

            SetPosition(GetPosition)

            'Select Case CurrentSpeed
            '    Case eShuttleSpeed.TwoX
            '        'smooth fast forward at 2X
            '        HR = Graph.KO_IKeystone.ActivateFFRW(CurrentSpeed)
            '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            '        nVidia_AudioMute(True)
            '        HR = Graph.MediaSeek.SetRate(CurrentSpeed)
            '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            '    Case eShuttleSpeed.FourX
            '        ToggleAudioRendererConnection(True)
            '        HR = Graph.KO_IKeystone.ActivateFFRW(CurrentSpeed)
            '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            '        nVidia_AudioMute(True)
            '        SetActiveVideoFrameMode(True)
            '        HR = Graph.MediaSeek.SetRate(CurrentSpeed)
            '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            '    Case Else
            '        Dim d As Double
            '        HR = Graph.MediaSeek.GetRate(d)
            '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            '        If d > 1 Then
            '            HR = Graph.MediaSeek.SetRate(1.0)
            '            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            '        End If
            '        StartShuttle(CurrentSpeed, True)
            'End Select
            PlayState = ePlayState.FastForwarding
        End Function

        Public Overrides Function Rewind(Optional ByVal Speed As eShuttleSpeed = eShuttleSpeed.Not_Specified) As Boolean

            If Speed = eShuttleSpeed.Not_Specified Then
                Select Case CurrentSpeed
                    Case eShuttleSpeed.OneX
                        CurrentSpeed = eShuttleSpeed.TwoX
                    Case eShuttleSpeed.TwoX
                        CurrentSpeed = eShuttleSpeed.FourX
                    Case eShuttleSpeed.FourX
                        CurrentSpeed = eShuttleSpeed.EightX
                    Case eShuttleSpeed.EightX
                        CurrentSpeed = eShuttleSpeed.SixteenX
                    Case eShuttleSpeed.SixteenX
                        CurrentSpeed = eShuttleSpeed.ThirtyTwoX
                    Case eShuttleSpeed.ThirtyTwoX
                        Debug.WriteLine("32x is top play speed.")
                        Return False
                End Select
            Else
                CurrentSpeed = Speed
            End If


            'HR = Graph.KO_IKeystone.ActivateFFRW(CurrentSpeed)
            'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            'nVidia_AudioMute(True)
            'AudioRenderConnectionToggle(True)
            'HR = Graph.MediaSeek.SetRate(CurrentSpeed - (2 * CurrentSpeed))
            'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            StartShuttle(CurrentSpeed, False)
            PlayState = ePlayState.Rewinding
        End Function

        Public Overrides Function Timesearch() As Boolean
        End Function

        Public Overrides Sub ClearOSD()

        End Sub

        Public Overrides Function FrameStep() As Boolean
            Try
                If AVMode = eAVMode.DesktopVMR Then
                    Graph.VideoStep.Step(1, Nothing)
                Else
                    HR = Graph.KO_IKeystone.FrameStep(True)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End If
                PlayState = ePlayState.FrameStepping
            Catch ex As Exception
                Throw New Exception("Problem with FrameStep(). Error: " & ex.Message)
            End Try
        End Function

        Public Overrides Function QuitFrameStepping() As Boolean
            Try
                If AVMode = eAVMode.DesktopVMR Then
                    HR = Graph.VideoStep.CancelStep()
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    HR = Graph.MediaCtrl.Run()
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Else
                    HR = Graph.KO_IKeystone.QuitFrameStepping
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End If
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with QuitFrameStepping. Error: " & ex.Message, ex)
            End Try
        End Function

        Public Overrides Function GetPosition() As Long
            Try
                Dim pCur, pStop As Long
                HR = Graph.MediaSeek.GetPositions(pCur, pStop)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return pCur
            Catch ex As Exception
                Debug.Write("Problem with GetPosition. Error: " & ex.Message)
            End Try
        End Function

        Public Overrides Function GetBitmapFromKeystone() As Byte()
            Try
                Dim SamplePtr As IntPtr
                Dim SampSize, SampW, SampH As Integer

                HR = Graph.KO_IKeystone.GrabSample(eSampleFrom.Output, SamplePtr, SampSize, SampW, SampH)
                If Math.Abs(HR) = 2147483655 Or Math.Abs(HR) = 2147467260 Then
                    Throw New Exception("FRAMEGRAB TIMEOUT: No samples received in three seconds.")
                    Return Nothing
                End If
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim OutputFileSize As Integer = SampSize + 54

                'Read raster data buffer from memory
                Dim Buffer(SampSize - 1) As Byte
                'Debug.WriteLine("FG Source: " & SamplePtr.ToInt32)
                Marshal.Copy(SamplePtr, Buffer, 0, SampSize)

                ''DEBUGGING - Convert UYVY bytes to RGB24
                'Dim CSC As New ColorSpaceConversions
                'Buffer = CSC.ConvertUYVYtoRGB24(Buffer)
                'OutputFileSize = Buffer.Length + 54
                ''END DEBUGGING

                ''DEBUGGING - Save the buffer
                'FS = New FileStream("C:\Test\Buffer.bin", FileMode.OpenOrCreate)
                'FS.Write(Buffer, 0, Buffer.Length)
                'FS.Close()
                'FS = Nothing
                ''END DEBUGGING

                'Flip Image
                Buffer = FlipImageBuffer_Vertically(Buffer)
                Buffer = FlipRGB24ImageBuffer_Horizontally(Buffer, SampW, SampH)

                'Make BMI header
                Dim BMI(53) As Byte
                BMI(0) = 66 'B
                BMI(1) = 77 'M

                Dim TmpBuff() As Byte

                'FileSize
                TmpBuff = ConvertDecimalIntoByteArray(OutputFileSize, 4)
                BMI(2) = TmpBuff(0)
                BMI(3) = TmpBuff(1)
                BMI(4) = TmpBuff(2)
                BMI(5) = TmpBuff(3)

                'Old way
                'Dim HexSize As String = Hex(OutputFileSize)
                'BMI(2) = CInt("&H" & Microsoft.VisualBasic.Right(HexSize, 2))
                'BMI(3) = CInt("&H" & Microsoft.VisualBasic.Left(Mid(HexSize, 2), 2))
                'BMI(4) = CInt("&H" & Microsoft.VisualBasic.Left(HexSize, 1))

                BMI(10) = 54 'Header size
                BMI(14) = 40 'InfoHeader size

                'Width
                TmpBuff = ConvertDecimalIntoByteArray(SampW, 4)
                BMI(18) = TmpBuff(0)
                BMI(19) = TmpBuff(1)
                BMI(20) = TmpBuff(2)
                BMI(21) = TmpBuff(3)

                'Height
                TmpBuff = ConvertDecimalIntoByteArray(SampH, 4)
                BMI(22) = TmpBuff(0)
                BMI(23) = TmpBuff(1)
                BMI(24) = TmpBuff(2)
                BMI(25) = TmpBuff(3)

                BMI(26) = 1 'Planes
                BMI(28) = 24 'Bit depth

                BMI(38) = 196
                BMI(39) = 14
                BMI(42) = 196
                BMI(43) = 14

                Dim SampleBitmapBuffer(OutputFileSize) As Byte
                Array.Copy(BMI, 0, SampleBitmapBuffer, 0, 54)
                Array.Copy(Buffer, 0, SampleBitmapBuffer, 54, Buffer.Length)

                'Debugging - Dump the bitmap
                'FS = New FileStream("C:\Temp_" & DateTime.Now.Ticks & ".bmp", FileMode.OpenOrCreate)
                'FS.Write(SampleBitmapBuffer, 0, SampleBitmapBuffer.Length)
                'FS.Close()
                'FS = Nothing
                'End Debugging

                Buffer = Nothing
                BMI = Nothing
                Return SampleBitmapBuffer
            Catch ex As Exception
                Throw New Exception("Problem with GetSampleFromKeystone. Error: " & ex.Message)
                Return Nothing
            End Try
        End Function

        Public Overrides Function GetYUVSampleFromKeystone() As Byte()
            Try
                Dim SamplePtr As IntPtr
                Dim SampSize, SampW, SampH As Integer

                HR = Graph.KO_IKeystone.GrabSample(eSampleFrom.Output, SamplePtr, SampSize, SampW, SampH)
                If Math.Abs(HR) = 2147483655 Or Math.Abs(HR) = 2147467260 Then
                    Throw New Exception("FRAMEGRAB TIMEOUT: No samples received in three seconds.")
                    Return Nothing
                End If
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim Buffer(SampSize - 1) As Byte
                Marshal.Copy(SamplePtr, Buffer, 0, SampSize)
                Return Buffer
            Catch ex As Exception
                Throw New Exception("Problem with GetYUVSampleFromKeystone(). Error: " & ex.Message)
            End Try
        End Function

        Public Overrides Function JumpBack(ByVal JumpSeconds As Byte) As Boolean
            Try
                Dim CP As Long
                HR = Graph.MediaSeek.GetCurrentPosition(CP)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                CP -= 50000000
                SetPosition(CP)
                'seek.SetPositions(CP, SeekingFlags.AbsolutePositioning, 0, SeekingFlags.NoPositioning)
            Catch ex As Exception
                Throw New Exception("Problem with JumpBack. Error: " & ex.Message)
            End Try
        End Function

        Public Overrides Function RenderBitmap(ByVal BM As System.Drawing.Bitmap, ByVal X As Short, ByVal Y As Short) As Boolean
            Try
                'Put RGB24 raster data into a byte array
                Dim BMData As Imaging.BitmapData = BM.LockBits(New Rectangle(0, 0, BM.Width, BM.Height), Imaging.ImageLockMode.ReadWrite, Imaging.PixelFormat.Format24bppRgb)
                Dim Bytes(BM.Height * BM.Width * 3 - 1) As Byte
                Dim s As Integer = 0
                Dim t As Integer = 0
                Dim Offset As Short = Math.Abs((BMData.Width * 3) - BMData.Stride)
                For l As Short = 1 To BMData.Height
                    For p As Short = 1 To BMData.Width
                        Bytes(t) = Marshal.ReadByte(BMData.Scan0, s)
                        Bytes(t + 1) = Marshal.ReadByte(BMData.Scan0, s + 1)
                        Bytes(t + 2) = Marshal.ReadByte(BMData.Scan0, s + 2)
                        t += 3
                        s += 3
                    Next
                    s += Offset
                Next
                BM.UnlockBits(BMData)
                BMData = Nothing

                'Get pointer to the RGB24 raster buffer
                Dim h1 As GCHandle = GCHandle.Alloc(Bytes, GCHandleType.Pinned)
                Dim BufferPtr As IntPtr = h1.AddrOfPinnedObject

                'render it
                HR = Graph.KO_IKeystone.ShowBitmap(BufferPtr.ToInt32, BM.Width, BM.Height, X, Y, eImageFormat.IF_RGB24)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                BM.Dispose()
                BM = Nothing
            Catch ex As Exception
                Throw New Exception("Problem with RenderBitmap(). Error: " & ex.Message)
            End Try
        End Function

        Public Overrides Function RestartStream() As Boolean
            SetPosition(0)
        End Function

        Public Overrides Sub SetOSD(ByVal BM As System.Drawing.Bitmap, ByVal ColorKey As Integer, ByVal DelayMiliSecs As Short, ByVal UseColorKey As Boolean, ByVal Alpha As Single, ByVal Sender As String, ByVal X As Short, ByVal Y As Short)
            Try
                'If OSDIsActive Then
                '    Me.ClearOSD("SetOSD")
                'End If
                'OSDIsActive = True

                'Put RGB24 raster data into a byte array
                Dim BMData As Imaging.BitmapData = BM.LockBits(New Rectangle(0, 0, BM.Width, BM.Height), Imaging.ImageLockMode.ReadWrite, Imaging.PixelFormat.Format24bppRgb)
                Dim Bytes(BM.Height * BM.Width * 3 - 1) As Byte
                Dim s As Integer = 0
                Dim t As Integer = 0
                Dim Offset As Short = Math.Abs((BMData.Width * 3) - BMData.Stride)
                For l As Short = 1 To BMData.Height
                    For p As Short = 1 To BMData.Width
                        Bytes(t) = Marshal.ReadByte(BMData.Scan0, s)
                        Bytes(t + 1) = Marshal.ReadByte(BMData.Scan0, s + 1)
                        Bytes(t + 2) = Marshal.ReadByte(BMData.Scan0, s + 2)
                        t += 3
                        s += 3
                    Next
                    s += Offset
                Next
                BM.UnlockBits(BMData)
                BMData = Nothing

                'For debugging
                'Dim FS As New FileStream("C:\Temp\RasterBytes.bin", FileMode.OpenOrCreate)
                'FS.Write(Bytes, 0, Bytes.Length)
                'FS.Close()
                'FS = Nothing
                'BM.Save("C:\Temp\OSDImage_Afterlockbits.bmp", System.Drawing.Imaging.ImageFormat.Bmp)
                'End Debugging

                'Get pointer to the RGB24 raster buffer
                Dim h1 As GCHandle = GCHandle.Alloc(Bytes, GCHandleType.Pinned)
                Dim BufferPtr As IntPtr = h1.AddrOfPinnedObject

                'Debugging
                'Debug.WriteLine("OSD ptr: " & Hex(BufferPtr.ToInt32))
                'Me.AddConsoleLine("OSD ptr: " & Hex(BufferPtr.ToInt32))

                ''Confirm that the pointer being passed points to an array that contains exactly the
                ''same bytes as we got from the locked bitmap above. Test confirms correct behavior.
                'Dim TestBytes(BM.Height * BM.Width * 3) As Byte
                'For i As Integer = 0 To UBound(TestBytes)
                '    TestBytes(i) = Marshal.ReadByte(BufferPtr, i)
                'Next
                'Dim FS1 As New FileStream("C:\Temp\TestBytes.bin", FileMode.OpenOrCreate)
                'FS1.Write(TestBytes, 0, Bytes.Length)
                'FS1.Close()
                'FS1 = Nothing
                'End Debugging

                'debugging
                'used this to confirm that the data we're sending is good
                'Dim tBM As Bitmap = SampleGrabber.GetBitmapFromRGB24Raster(Bytes, BM.Width, BM.Height)
                'tBM.Save("C:\Temp\Temp_justbeforesendtokeystone.bmp", System.Drawing.Imaging.ImageFormat.Bmp)
                'tBM.Dispose()
                'tBM = Nothing
                'debugging

                HR = Graph.KO_IKeystoneMixer.put_OSD(BufferPtr.ToInt32, BM.Width, BM.Height, X, Y, eImageFormat.IF_RGB24, ColorKey, DelayMiliSecs)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                BM.Dispose()
                BM = Nothing

                'Dim HDC_PTR As IntPtr = Me.GetHDCForBM(BM)
                ''Dim BMP_PTR As IntPtr = Me.GetPointerFromObject(BM)

                'Dim HR As Integer = ifKeystone.put_OSD(HDC_PTR, BM.Width, BM.Height, 50, 50, ImageFormat.IF_RGB24, 0)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                ''Now do delay
                'OSDTimer = New System.Windows.Forms.Timer
                'OSDTimer.Interval = DelayMiliSecs
                'AddHandler OSDTimer.Tick, AddressOf ClearOSD
                'OSDTimer.Start()
            Catch ex As Exception
                Throw New Exception("problem setting bitmap. sender: " & Sender & " error: " & ex.Message)
            End Try
        End Sub

        Public Overrides Sub StartSampleRecording(ByVal TargetDirectory As String)
            Try
                If NowRecording Then Exit Sub
                If Not Directory.Exists(TargetDirectory) Then Directory.CreateDirectory(TargetDirectory)
                If Graph.KO_IKeystone Is Nothing Then Exit Sub
                Graph.KO_IKeystone.StartRecording(Replace(TargetDirectory, "\", "\\"))
                NowRecording = True
            Catch ex As Exception
                Throw New Exception("Problem with StartSampleRecording(). Error: " & ex.Message)
            End Try
        End Sub

        Public Overrides Sub StopSampleRecording()
            Try
                If Not NowRecording Then Exit Sub
                Graph.KO_IKeystone.StopRecording()
                NowRecording = False
            Catch ex As Exception
                Throw New Exception("Problem with StopRecording(). Error: " & ex.Message)
            End Try
        End Sub

        Public Overrides Sub DerivedPlayerHandleEvent(ByVal code As Integer, ByVal p1 As Integer, ByVal p2 As Integer)
            Try
                Select Case code

                End Select
            Catch ex As Exception
                Throw New Exception("Problem with DerivedPlayerHandleEvent(). Error: " & ex.Message)
            End Try
        End Sub

        Public Overrides Function GrabSingleSample(ByVal TargetDirectory As String, ByVal Format As System.Drawing.Imaging.ImageFormat) As Boolean

        End Function

        Public Overrides Function FilterCheck() As String
            Return "True"
        End Function

        Public Overrides Function GetSourceProperties() As cSourceProperties
            Try
                Dim AMT As AM_MEDIA_TYPE
                HR = Graph.KO_In.ConnectionMediaType(AMT)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim VIH As VIDEOINFOHEADER2 = CType(Marshal.PtrToStructure(AMT.pbFormat, GetType(VIDEOINFOHEADER2)), VIDEOINFOHEADER2)
                Dim out As New cSourceProperties
                out.FrameRate = Math.Round(10000000 / VIH.AvgTimePerFrame, 3)
                out.ATPF = VIH.AvgTimePerFrame
                out.Height = VIH.rcSource.Bottom
                out.Width = VIH.rcSource.Right
                out.AspectX = VIH.dwPictAspectRatioX
                out.AspectY = VIH.dwPictAspectRatioY
                out.ControlFlags = VIH.dwControlFlags
                out.CopyProtectFlags = VIH.dwCopyProtectFlags
                out.InterlacedFlags = VIH.dwInterlaceFlags
                Return out
            Catch ex As Exception
                Throw New Exception("Problem with GetSourceProperties(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public ReadOnly Property Duration_StringNoFrames() As String
            Get
                If _Duration_StringWithFrames = "" Then
                    _Duration_StringWithFrames = DurationInTimecode.ToString_NoFrames
                End If
                Return _Duration_StringWithFrames
            End Get
        End Property
        Private _Duration_StringWithFrames As String = ""

        Public ReadOnly Property DurationInTimecode() As cTimecode
            Get
                If _DurationInTimecode Is Nothing Then
                    _DurationInTimecode = New cTimecode(Duration_InReferenceTime, ActiveVideoStreamProperties.ATPF, ActiveVideoStreamProperties.FrameRateEn)
                End If
                Return _DurationInTimecode
            End Get
        End Property
        Private _DurationInTimecode As cTimecode

        Public Overrides Sub Dispose()
            Graph.DestroyGraph()
            For Each s As cStreamInfo In Streams
                If s.Decoder IsNot Nothing Then s.Decoder.Dispose()
                If s.Renderer IsNot Nothing Then s.Renderer.Dispose()
                If s.AMTC IsNot Nothing Then s.AMTC.Dispose()
                If s.Scaler IsNot Nothing Then s.AMTC.Dispose()
                If s.DemuxerPin IsNot Nothing Then
                    HR = Marshal.FinalReleaseComObject(s.DemuxerPin)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    s.DemuxerPin = Nothing
                End If
            Next
            'MyBase.Dispose()
        End Sub

#End Region 'CBASEPLAYER OVERRIDES

#Region "M2TS-SPECIFIC FUNCTIONALITY"

#Region "M2TS:PROPERTIES"

        Public ReadOnly Property ActiveVideoStreamProperties() As cSourceProperties
            Get
                If _ActiveVideoStreamProperties Is Nothing Then
                    _ActiveVideoStreamProperties = GetSourceProperties()
                End If
                Return _ActiveVideoStreamProperties
            End Get
        End Property
        Private _ActiveVideoStreamProperties As cSourceProperties

        Public ReadOnly Property IsTextST() As Boolean
            Get
                If Streams Is Nothing Then Return False
                If Streams.Count = 1 AndAlso Streams(0).Format = cStreamInfo.eStreamFormat.tst Then Return True
                Return False
            End Get
        End Property

#End Region 'M2TS:PROPERTIES

#Region "M2TS:INDEXING"

        Public Event evCreatingIndexFile()

        Private Sub IndexFileCheck(ByVal FilePath As String)
            If Not File.Exists(FilePath & ".inx") Then RaiseEvent evCreatingIndexFile()
        End Sub

#End Region 'M2TS:INDEXING

#Region "M2TS:FILTER ADDERS"

        Private Function AddFilter_AVCDecoder(ByRef GraphBuilder As IGraphBuilder) As cSMTFilter
            Try
                Dim o As Object = DsBugWO.CreateDsInstance(New Guid("96B9D0ED-8D13-4171-A983-B84D88D627BE"), Clsid.IBaseFilter_GUID)
                'Dim o As Object = DsBugWO.CreateDsInstance(New Guid("6A270473-9994-4AEB-801F-BB2C4E56EE38"), ClsId.IBaseFilter_GUID)
                Dim out As New cSMTFilter
                out.Filter = CType(o, IBaseFilter)
                HR = GraphBuilder.AddFilter(out.Filter, "MCE_AVC")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim pIUnk As IntPtr = Marshal.GetIUnknownForObject(out.Filter)
                out.IMC = Elecard.ModuleConfigInterface.ModuleConfigAdapter.GetConfigInterface(pIUnk)
                Dim b As Boolean
                b = out.IMC.SetParamValue(New Guid("85c6cbac-fbed-f244-a07c-6f9abd799e64"), Nothing) 'UNLOCK
                b = out.IMC.SetParamValue(ModuleConfig_Consts.EMC_Quality, ModuleConfig_Consts.eEMC_Quality.ObeyQualityMessages)
                b = out.IMC.SetParamValue(ModuleConfig_Consts.AVC_ErrorResilienceMode, ModuleConfig_Consts.eErrorResilienceMode_AVC.ErrorResilienceMode_Decode_Anyway)
                b = out.IMC.SetParamValue(ModuleConfig_Consts.EMC_ErrorConcealment, 0)
                b = out.IMC.SetParamValue(ModuleConfig_Consts.HardwareAcceleration, 1)
                b = out.IMC.SetParamValue(ModuleConfig_Consts.AVC_Deblock, 0)
                b = out.IMC.SetParamValue(ModuleConfig_Consts.HQUpsample_VC1AVC, 1)
                b = out.IMC.SetParamValue(ModuleConfig_Consts.Deinterlace, ModuleConfig_Consts.eDeinterlaceMode.Deinterlace_Auto)
                b = out.IMC.SetParamValue(ModuleConfig_Consts.DoubleRate, 0)
                b = out.IMC.SetParamValue(ModuleConfig_Consts.FieldsReordering, 0)
                b = out.IMC.SetParamValue(ModuleConfig_Consts.FieldsReorderingCondition, ModuleConfig_Consts.eFieldReorderingConditionMode.FieldReorderingCondition_Always)
                b = out.IMC.SetParamValue(ModuleConfig_Consts.Synchronizing, ModuleConfig_Consts.eEM2VD_Synchronizing.PTS)
                b = out.IMC.SetParamValue(ModuleConfig_Consts.FormatVideoInfo, ModuleConfig_Consts.eFormatVideoInfo.FormatVideoInfo_Both)
                b = out.IMC.SetParamValue(ModuleConfig_Consts.AVC_RateMode, ModuleConfig_Consts.eRateMode.RateMode_Field)
                out.IMC.CommitChanges()

                HR = out.Filter.FindPin("In", out.Pin_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'MCE_AVC_PP = CType(out, ISpecifyPropertyPages)

                Return out
            Catch ex As Exception
                Throw New Exception("Problem with AddMCE_AVC. Error: " & ex.Message, ex)
            End Try
        End Function

        Private Function AddFilter_VC1Decoder(ByRef GraphBuilder As IGraphBuilder) As cSMTFilter
            Try
                Dim Out As New cSMTFilter
                Out.Filter = CType(DsBugWO.CreateDsInstance(New Guid("C0046C92-D654-4B34-92D6-C6AC34B7346D"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(Out.Filter, "MCE_VC1")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = Out.Filter.FindPin("In", Out.Pin_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = Out.Filter.FindPin("Out", Out.Pin_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'MCE_VC1A_PP = CType(Out.Filter, ISpecifyPropertyPages)

                Dim pIUnk As IntPtr = Marshal.GetIUnknownForObject(Out.Filter)
                Out.IMC = Elecard.ModuleConfigInterface.ModuleConfigAdapter.GetConfigInterface(pIUnk)
                Dim b As Boolean
                b = Out.IMC.SetParamValue(New Guid("85c6cbac-fbed-f244-a07c-6f9abd799e64"), Nothing) 'UNLOCK
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.VC1_SkipMode, ModuleConfig_Consts.eMCVC1VD_SkipMode.auto)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.VC1_ErrorResilienceMode, ModuleConfig_Consts.eErrorResilienceMode_VC1.Error_resilience_mode_proceed_anyway)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.HardwareAcceleration, 0)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.HQUpsample_VC1AVC, 1)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.Deinterlace, ModuleConfig_Consts.eDeinterlaceMode.Deinterlace_Weave)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.DoubleRate, 0)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.FieldsReordering, 0)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.FieldsReorderingCondition, ModuleConfig_Consts.eFieldReorderingConditionMode.FieldReorderingCondition_Always)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.Synchronizing, ModuleConfig_Consts.eEM2VD_Synchronizing.PTS)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.FormatVideoInfo, ModuleConfig_Consts.eFormatVideoInfo.FormatVideoInfo_Both)
                Out.IMC.CommitChanges()

                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with AddFilter_VC1Decoder(). Error: " & ex.Message)
            End Try
        End Function

        Private Function AddFilter_M2VDecoder(ByRef GraphBuilder As IGraphBuilder) As cSMTFilter
            Try
                Dim Out As New cSMTFilter
                Out.Filter = CType(DsBugWO.CreateDsInstance(New Guid("BC4EB321-771F-4E9F-AF67-37C631ECA106"), Clsid.IBaseFilter_GUID), IBaseFilter)
                HR = GraphBuilder.AddFilter(Out.Filter, "MCE_MP2A")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = Out.Filter.FindPin("In", Out.Pin_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = Out.Filter.FindPin("Out", Out.Pin_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'MCE_MP2A_PP = CType(Out.Filter, ISpecifyPropertyPages)

                Dim pIUnk As IntPtr = Marshal.GetIUnknownForObject(Out.Filter)
                Out.IMC = Elecard.ModuleConfigInterface.ModuleConfigAdapter.GetConfigInterface(pIUnk)
                Dim b As Boolean
                b = Out.IMC.SetParamValue(New Guid("85c6cbac-fbed-f244-a07c-6f9abd799e64"), Nothing)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.EMC_Quality, ModuleConfig_Consts.eEMC_Quality.ObeyQualityMessages)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.M2VD_Brightness, 128)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.HardwareAcceleration, 0)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.M2VD_Resolution, ModuleConfig_Consts.eResolutionMode.Resolution_Full)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.M2VD_IDCTPrecision, 1)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.M2VD_PostProcess, 0)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.Deinterlace, ModuleConfig_Consts.eDeinterlaceMode.Deinterlace_Weave)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.M2VD_DeinterlaceCondition, ModuleConfig_Consts.eDeinterlaceCondition.DeinterlaceCondition_Always)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.M2VD_HQUpsample, 1)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.DoubleRate, 0)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.FieldsReordering, 0)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.FieldsReorderingCondition, ModuleConfig_Consts.eFieldReorderingConditionMode.FieldReorderingCondition_Always)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.FormatVideoInfo, ModuleConfig_Consts.eFormatVideoInfo.FormatVideoInfo_Both)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.EMC_OSD, 0)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.M2VD_CCubeDecodeOrder, 0)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.EMC_ErrorConcealment, 0)
                'WHAT IS "SMP" ?
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.M2VD_MediaTimeSource, 0)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.M2VD_SetMediaTime, 1)
                b = Out.IMC.SetParamValue(ModuleConfig_Consts.Synchronizing, ModuleConfig_Consts.eEM2VD_Synchronizing.PTS)

                Out.IMC.CommitChanges()
                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with AddFilter_M2VDecoder. Error: " & ex.Message)
            End Try
        End Function

        Private Function AddFilter_NVAudio() As cSMTFilter
            Try
                Dim Out As New cSMTFilter
                If Not Graph.AddNVidiaAudioDecoder(AVMode = eAVMode.DesktopVMR) Then Throw New Exception("Failed to add nVidia Audio Decoder.")
                Out.Filter = Graph.AudioDecoder
                Out.Pin_In = Graph.AudDec_InPin
                Out.Pin_Out = Graph.AudDec_OutPin
                Out.NV_AudAtts = Graph.nvAudioAtts
                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with AddFilter_NVAudio. Error: " & ex.Message)
            End Try
        End Function

        Private Function AddFilter_MPA(ByRef GraphBuilder) As cSMTFilter
            Try
                Dim Out As New cSMTFilter
                'Dim o As Object = DsBugWO.CreateDsInstance(New Guid("2F75E451-A88C-4939-BFE5-D92D48C102F2"), Clsid.IBaseFilter_GUID)
                'Out.Filter = CType(o, IBaseFilter)
                'HR = GraphBuilder.AddFilter(Out.Filter, "MCE_AVC")
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'Dim pIUnk As IntPtr = Marshal.GetIUnknownForObject(Out.Filter)
                'Out.IMC = Elecard.ModuleConfigInterface.ModuleConfigAdapter.GetConfigInterface(pIUnk)
                'Dim b As Boolean = Out.IMC.SetParamValue(New Guid("85c6cbac-fbed-f244-a07c-6f9abd799e64"), Nothing)
                'Out.IMC.CommitChanges()

                'HR = Out.Filter.FindPin("In", Out.Pin_In)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                ''MC_MPADEC_PP = CType(MC_MPADEC, ISpecifyPropertyPages)

                Return Out
            Catch ex As Exception
                Throw New Exception("Problem with AddFilter_MPA. Error: " & ex.Message)
            End Try
        End Function

        Private Function AddFilter_VMR9(ByVal NumberOfPins As Short) As cSMTFilter
            Try
                Dim out As New cSMTFilter
                If Not Graph.AddVMR9(1) Then Throw New Exception("Failed to add VMR9.")
                out.Filter = Graph.VMR9
                out.Pin_In = Graph.VMR9_In_1
                Return out
            Catch ex As Exception
                Throw New Exception("Problem with AddVMR9. Error: " & ex.Message)
            End Try
        End Function

        Private Function AddFilter_AMTC() As cSMTFilter
            Try
                Dim out As New cSMTFilter
                Graph.AddAMTC()
                out.Filter = Graph.AMTC
                out.Pin_In = Graph.AMTC_In
                out.Pin_Out = Graph.AMTC_Out
                Return out
            Catch ex As Exception
                Throw New Exception("Problem with AddDefaultDirectSoundDevice. Error: " & ex.Message)
            End Try
        End Function

        Public Function AddFilter_DefaultDirectSoundDevice() As cSMTFilter
            Try
                Dim out As New cSMTFilter
                If Not Graph.AddDefaultDirectSoundDevice() Then Throw New Exception("Failed to add DSound.")
                out.Filter = Graph.AudioRenderer
                out.Pin_In = Graph.AudRen_In
                Return out
            Catch ex As Exception
                Throw New Exception("Problem with AddDefaultDirectSoundDevice. Error: " & ex.Message)
            End Try
        End Function

        Private Sub RemoveFilter(ByRef F As IBaseFilter)
            Try
                HR = Graph.GraphBuilder.RemoveFilter(F)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Marshal.FinalReleaseComObject(F)
            Catch ex As Exception
                Throw New Exception("Problem with RemoveFilter(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Private Sub nVidia_AudioMute(ByVal Mute As Boolean)
            Try
                If ActiveAudioStream.Decoder.NV_AudAtts Is Nothing Then Exit Sub
                'ActiveAudioStream.Decoder.NV_AudAtts.SetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_CONFIG, nvcommon.ENvidiaAudioDecoderProps_Config.NVAUDDEC_CONFIG_SKIP_AUDIO_DECODE, IIf(Mute, 1, 0))
                ActiveAudioStream.Decoder.NV_AudAtts.SetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_CONFIG, nvcommon.ENvidiaAudioDecoderProps_Config.NVAUDDEC_CONFIG_MUTE, IIf(Mute, 1, 0))
            Catch ex As Exception
                Throw New Exception("Problem with nVidia_AudioMute(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Private Sub SetActiveVideoFrameMode(ByVal I_frame_only As Boolean)
            Try
                Dim b As Boolean
                Select Case ActiveVideoStream.Format
                    Case cStreamInfo.eStreamFormat.m2v, cStreamInfo.eStreamFormat.h264
                        b = ActiveVideoStream.Decoder.IMC.SetParamValue(ModuleConfig_Consts.EMC_Quality, IIf(I_frame_only, ModuleConfig_Consts.eEMC_Quality.I_Frame_Only, ModuleConfig_Consts.eEMC_Quality.ObeyQualityMessages))
                    Case cStreamInfo.eStreamFormat.vc1
                        b = ActiveVideoStream.Decoder.IMC.SetParamValue(ModuleConfig_Consts.VC1_SkipMode, IIf(I_frame_only, ModuleConfig_Consts.eMCVC1VD_SkipMode.decode_intra, ModuleConfig_Consts.eMCVC1VD_SkipMode.auto))
                End Select
            Catch ex As Exception
                Throw New Exception("Problem with SetActiveVideoFrameMode(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Private Sub ToggleAudioRendererConnection(ByVal Disconnected As Boolean)
            Try
                If ActiveAudioStream Is Nothing Then Exit Sub
                Graph.MediaCtrl.Stop()
                If Disconnected Then
                    HR = Graph.GraphBuilder.RemoveFilter(ActiveAudioStream.Renderer.Filter)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    'HR = Graph.GraphBuilder.Disconnect(ActiveAudioStream.AMTC.Pin_Out)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Else
                    ActiveAudioStream.Renderer = AddFilter_DefaultDirectSoundDevice()
                    HR = Graph.GraphBuilder.Render(ActiveAudioStream.AMTC.Pin_Out)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End If
                HR = Graph.MediaCtrl.Run()
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                Throw New Exception("Problem with ToggleAudioRendererConnection(). Error: " & ex.Message, ex)
            End Try
        End Sub

#End Region 'M2TS:FILTER ADDERS

#Region "M2TS:STREAM INFO"

        Public ReadOnly Property Streams() As List(Of cStreamInfo)
            Get
                Return _Streams
            End Get
        End Property
        Private _Streams As List(Of cStreamInfo)

        Public ReadOnly Property PrimaryStreams() As cM2TS_StreamSet
            Get
                If _PrimaryStreams Is Nothing Then
                    _PrimaryStreams = New cM2TS_StreamSet(Streams, cStreamInfo.ePrimarySecondary.Primary)
                End If
                Return _PrimaryStreams
            End Get
        End Property
        Private _PrimaryStreams As cM2TS_StreamSet

        Public ReadOnly Property SecondaryStreams() As cM2TS_StreamSet
            Get
                If _SecondaryStreams Is Nothing Then
                    _SecondaryStreams = New cM2TS_StreamSet(Streams, cStreamInfo.ePrimarySecondary.Secondary)
                End If
                Return _SecondaryStreams
            End Get
        End Property
        Private _SecondaryStreams As cM2TS_StreamSet

        Public ReadOnly Property ActiveVideoStream() As cStreamInfo
            Get
                For Each s As cStreamInfo In Streams
                    If s.GeneralCategory = cStreamInfo.eCategories_General.Video And s.IsActive Then Return s
                Next
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property ActiveAudioStream() As cStreamInfo
            Get
                For Each s As cStreamInfo In Streams
                    If s.GeneralCategory = cStreamInfo.eCategories_General.Audio And s.IsActive Then Return s
                Next
                Return Nothing
            End Get
        End Property

        Public ReadOnly Property StreamCount_Total() As Byte
            Get
                Return Me.Streams.Count
            End Get
        End Property

        Public ReadOnly Property StreamCount_Video() As Byte
            Get
                Dim out As Byte = 0
                For Each s As cStreamInfo In Streams
                    If s.Category = cStreamInfo.eCategories.PrimaryVideo Or s.Category = cStreamInfo.eCategories.SecondaryVideo Then
                        out += 1
                    End If
                Next
                Return out
            End Get
        End Property

        Public ReadOnly Property StreamCount_Audio() As Byte
            Get
                Dim out As Byte = 0
                For Each s As cStreamInfo In Streams
                    If s.Category = cStreamInfo.eCategories.PrimaryAudio Or s.Category = cStreamInfo.eCategories.SecondaryAudio Then
                        out += 1
                    End If
                Next
                Return out
            End Get
        End Property

        Public ReadOnly Property StreamCount_Graphics() As Byte
            Get
                Dim out As Byte = 0
                For Each s As cStreamInfo In Streams
                    If s.Category = cStreamInfo.eCategories.InteractiveGraphics Or s.Category = cStreamInfo.eCategories.PresentationGraphics Or s.Category = cStreamInfo.eCategories.TextSubtitles Then
                        out += 1
                    End If
                Next
                Return out
            End Get
        End Property

        Public ReadOnly Property StreamCount_SecondaryVideo() As Byte
            Get
                Dim out As Byte = 0
                For Each s As cStreamInfo In Streams
                    If s.Category = cStreamInfo.eCategories.SecondaryVideo Then
                        out += 1
                    End If
                Next
                Return out
            End Get
        End Property

        Public ReadOnly Property StreamCount_PrimaryAudio() As Byte
            Get
                Dim out As Byte = 0
                For Each s As cStreamInfo In Streams
                    If s.Category = cStreamInfo.eCategories.PrimaryAudio Then
                        out += 1
                    End If
                Next
                Return out
            End Get
        End Property

        Public ReadOnly Property StreamCount_SecondaryAudio() As Byte
            Get
                Dim out As Byte = 0
                For Each s As cStreamInfo In Streams
                    If s.Category = cStreamInfo.eCategories.SecondaryAudio Then
                        out += 1
                    End If
                Next
                Return out
            End Get
        End Property

        Public ReadOnly Property StreamCount_InteractiveGraphics() As Byte
            Get
                Dim out As Byte = 0
                For Each s As cStreamInfo In Streams
                    If s.Category = cStreamInfo.eCategories.InteractiveGraphics Then
                        out += 1
                    End If
                Next
                Return out
            End Get
        End Property

        Public ReadOnly Property StreamCount_PresentationGraphics() As Byte
            Get
                Dim out As Byte = 0
                For Each s As cStreamInfo In Streams
                    If s.Category = cStreamInfo.eCategories.PresentationGraphics Then
                        out += 1
                    End If
                Next
                Return out
            End Get
        End Property

        Public ReadOnly Property StreamCount_ByGeneralCategory(ByVal GC As cStreamInfo.eCategories_General) As Integer
            Get
                Select Case GC
                    Case cStreamInfo.eCategories_General.Video
                        Return StreamCount_Video
                    Case cStreamInfo.eCategories_General.Audio
                        Return StreamCount_Audio
                    Case cStreamInfo.eCategories_General.Graphics
                        Return StreamCount_Graphics
                End Select
            End Get
        End Property

        Public Function GetStreamInfo(ByVal Cat As cStreamInfo.eCategories, ByVal StreamNumber As Byte) As cStreamInfo
            Try
                If Me.Streams Is Nothing Then Return Nothing
                For Each S As cStreamInfo In Streams
                    If S.Category = Cat And S.StreamNumber = StreamNumber Then
                        Return S
                    End If
                Next
                Return Nothing
            Catch ex As Exception
                Throw New Exception("Problem with LocateStream(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function FindStream(ByRef SI As cStreamInfo) As cStreamInfo
            For Each s As cStreamInfo In Streams
                If s.Category = SI.Category And s.StreamNumber = SI.StreamNumber Then Return s
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' Returns a 90kHz value.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property PTS_Current() As UInt64
            Get
                If _CurrentPTS = 0 Then Return 0 'the first few frames might not be adjusted by PTS_Base
                If PTS_Base > _CurrentPTS Then
                    Return _CurrentPTS
                End If
                Return _CurrentPTS '- PTS_Base
            End Get
        End Property

        ''' <summary>
        ''' Returns a 90kHz value.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property PTS_Base() As UInt64
            Get
                Dim inRefTime As UInt64 = Graph.MCE_IMC_DMXA.GetParamValue(ModuleConfig_Consts.DMX_BASETIME)
                Return RT_to_PTS(inRefTime)
            End Get
        End Property

        ''' <summary>
        ''' Returns a 90kHz value.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property PTS_MAX() As UInt64
            Get
                Return PTS_Base + PTS_Duration
            End Get
        End Property

        ''' <summary>
        ''' Returns a 90kHz value.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property PTS_Duration() As UInt64
            Get
                'Dim t As New cTimecode(Duration_InReferenceTime, 417084, eFramerate.FILM)
                'Debug.WriteLine("duration from RT = " & t.ToString)
                'Debug.WriteLine("duration in RT = " & Duration_InReferenceTime)

                'convert 100ns value to 90kHz
                Return RT_to_PTS(Duration_InReferenceTime)
            End Get
        End Property

        ''' <summary>
        ''' Returns as 45kHz value per the THX spec.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property THX_MediaTime() As UInt32
            Get
                Return (PTS_Current >> 1) And UInt32.MaxValue
            End Get
        End Property

#End Region 'M2TS:STREAM INFO

#Region "M2TS:STREAM SELECTION"

#Region "M2TS:STREAM SELECTION:SHARED"

        Public Function SelectStream(ByRef SI As cStreamInfo) As Boolean
            Debug.WriteLine("M2TS PLAYER | SelectStream()")
            Select Case SI.GeneralCategory
                Case cStreamInfo.eCategories_General.Video
                    Return SelectVideoStream(SI)
                Case cStreamInfo.eCategories_General.Audio
                    Return SelectAudioStream(SI)
                Case cStreamInfo.eCategories_General.Graphics
                    Return SelectGraphicsStream(SI)
            End Select
        End Function

        Private Function GraphRebuildForStreamChange(ByVal Pid_Video As Integer, ByVal Pid_Audio As Integer, ByVal Pid_IG As Integer, ByVal Pid_PG As Integer, ByVal MaintainPosition As Boolean) As Boolean
            Debug.WriteLine("M2TS PLAYER | GraphRebuildForStreamChange()")
            Try
                'n) get position
                Dim cp As Long = GetPosition()

                'n) stop graph
                HR = Graph.MediaCtrl.Stop()
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'n) destroy graph
                Graph.DestroyGraph()

                'n) check for "no-change" pids. if the value is -1 that means do not change it
                If Pid_Video = -1 Then Pid_Video = ActiveVideoStream.PID
                If Pid_Audio = -1 Then Pid_Audio = ActiveAudioStream.PID
                'If Pid_IG = -1 Then Pid_IG = ActiveIG.PID
                'If Pid_PG = -1 Then Pid_PG = ActivePG.PID

                'n) build graph
                BuildGraph(FilePath, NotifyWindowHandle, AVMode, ParentForm, Pid_Video, Pid_Audio, Pid_IG, Pid_PG)

                'n) run graph
                Debug.WriteLine("M2TS PLAYER | IMediaControl::Run()")
                HR = Graph.MediaCtrl.Run
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                ''DEBUGGING
                'Dim state As eMediaControlState
                'While state <> eMediaControlState.Running
                '    HR = Graph.MediaCtrl.GetState(100, state)
                'End While
                ''DEBUGGING

                'n) seek to last position
                If MaintainPosition Then
                    System.Threading.Thread.Sleep(1500)
                    If Not SetPosition(0) Then Throw New Exception("Failed to seek after graph rebuild.")
                End If

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with GraphRebuildForStreamChange(). Error: " & ex.Message, ex)
            End Try
        End Function

#End Region 'M2TS:STREAM SELECTION:SHARED

#Region "M2TS:STREAM SELECTION:VIDEO"

        Private Function SelectVideoStream(ByRef SI As cStreamInfo) As Boolean
            Try
                If Not GraphRebuildForStreamChange(SI.PID, -1, -1, -1, False) Then Throw New Exception("Failed to rebuild graph.")
                ActiveVideoStream.IsActive = False
                FindStream(SI).IsActive = True
                Return True

                ''1) stop graph
                'HR = Graph.MediaCtrl.Stop()
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                ''2) remove the active decoder
                'HR = Graph.GraphBuilder.RemoveFilter(ActiveVideoStream.Decoder.Filter)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                ''3) put in new decoder
                'Select Case SI.Format
                '    Case cStreamInfo.eStreamFormat.h264
                '        SI.Decoder = Me.AddFilter_AVCDecoder(Graph.GraphBuilder)
                '    Case cStreamInfo.eStreamFormat.m2v
                '        SI.Decoder = Me.AddFilter_M2VDecoder(Graph.GraphBuilder)
                '    Case cStreamInfo.eStreamFormat.vc1
                '        SI.Decoder = Me.AddFilter_VC1Decoder(Graph.GraphBuilder)
                'End Select

                ''4) connect the decoder to the demux
                'HR = Graph.GraphBuilder.Connect(SI.DemuxerPin, SI.Decoder.Pin_In)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                ''5) remove keystone
                'HR = Graph.GraphBuilder.RemoveFilter(Graph.KeystoneOMNI)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                ''6) add new keystone
                'If Not Graph.AddKeystoneOmni() Then Return False
                'Graph.KO_IKeystone.SendMediaTimeEvents(1, 1)

                ''7) connect decoder to keystone
                'HR = Graph.GraphBuilder.Connect(SI.Decoder.Pin_Out, Graph.KO_In)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                ''8) remove renderer
                'HR = Graph.GraphBuilder.RemoveFilter(ActiveVideoStream.Renderer.Filter)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                'Marshal.FinalReleaseComObject(ActiveVideoStream.Renderer.Filter)

                ''9) add new renderer
                'SI.Renderer = AddFilter_VMR9(1)

                ''10) connect keystone to renderer
                'HR = Graph.GraphBuilder.Connect(Graph.KO_Out, SI.Renderer.Pin_In)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                ''11) setup viewer
                'Graph.VideoWin = CType(SI.Renderer.Filter, IVideoWindow)

                ''12) play graph
                'HR = Graph.MediaCtrl.Run()
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                ''n) change active over to current stream
                'ActiveVideoStream.IsActive = False
                'SI.IsActive = True

                'Return True
            Catch ex As Exception
                Throw New Exception("Problem SelectVideoStream(). Error: " & ex.Message, ex)
            End Try
        End Function

#End Region 'M2TS:STREAM SELECTION:VIDEO

#Region "M2TS:STREAM SELECTION:AUDIO"

        Private Function SelectAudioStream(ByRef SI As cStreamInfo) As Boolean
            Try
                If Not GraphRebuildForStreamChange(-1, SI.PID, -1, -1, True) Then Throw New Exception("Failed to rebuild graph.")
                ActiveAudioStream.IsActive = False
                FindStream(SI).IsActive = True
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with SelectAudioStream(). Error: " & ex.Message, ex)
            End Try
        End Function

#End Region 'M2TS:STREAM SELECTION:AUDIO

#Region "M2TS:STREAM SELECTION:GRAPHICS"

        Private Function SelectGraphicsStream(ByRef SI As cStreamInfo) As Boolean
            Return True
        End Function

#End Region 'M2TS:STREAM SELECTION:GRAPHICS

#End Region 'M2TS:STREAM SELECTION

#Region "M2TS:SEEKING"

        'Public Sub SetSeekMode(ByVal PTS As Boolean)
        '    Try
        '        If Graph Is Nothing OrElse Graph.MCE_IMC_DMXA Is Nothing Then Exit Sub

        '        Dim i As UInt64
        '        Dim b As Boolean = Graph.MCE_IMC_DMXA.SetParamValue(SMT.Common.Multimedia.DirectShow.MainConcept.ModuleConfig_Consts.DMX_BASETIME, i)
        '        Debug.WriteLine("basetime=" & i)

        '        'Dim b As Boolean
        '        'If PTS Then
        '        '    b = Graph.MCE_IMC_DMXA.SetParamValue(SMT.Common.Multimedia.DirectShow.MainConcept.ModuleConfig_Consts.EMPGDMX_DIRECT_PTS, 1)
        '        'Else
        '        '    b = Graph.MCE_IMC_DMXA.SetParamValue(SMT.Common.Multimedia.DirectShow.MainConcept.ModuleConfig_Consts.EMPGDMX_DIRECT_PTS, 0)
        '        'End If
        '    Catch ex As Exception
        '        Throw New Exception("Problem with SetSeekMode(). Error: " & ex.Message, ex)
        '    End Try
        'End Sub

#End Region 'M2TS:SEEKING

#Region "M2TS:BARDATA"

        Public Shadows Sub BarDataConfig(ByVal DetectBarData As Integer, ByVal BurnGuides As Integer, ByVal Luma_Tolerance As Integer, ByVal Chroma_Tolerance As Integer)
            Try
                If Me.Graph Is Nothing Then Exit Sub
                If Me.Graph.KO_IKeystoneMixer Is Nothing Then Exit Sub
                Me.Graph.KO_IKeystoneMixer.BarDataConfig(DetectBarData, BurnGuides, Luma_Tolerance, Chroma_Tolerance)
            Catch ex As Exception
                Throw New Exception("Problem in BarDataConfig(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Shadows Sub BarDataReset()
            If Me.Graph Is Nothing Then Exit Sub
            If Me.Graph.KO_IKeystoneMixer Is Nothing Then Exit Sub
            Me.Graph.KO_IKeystoneMixer.BarDataReset()
        End Sub

#End Region 'M2TS:BARDATA

#Region "M2TS:SHUTTLE"

        Private WithEvents ShuttleTimer As Timers.Timer
        Private ShuttleForward As Boolean
        Private TimerIntervalSecondFraction As Double

        Private Sub StartShuttle(ByVal Speed As eShuttleSpeed, ByVal Forward As Boolean)
            Debug.WriteLine("StartShuttle() Speed: " & Speed)
            ShuttleForward = Forward
            If ShuttleTimer Is Nothing Then ShuttleTimer = New Timers.Timer
            nVidia_AudioMute(True)
            SetActiveVideoFrameMode(True)
            ShuttleTimer.Interval = 300
            Me.TimerIntervalSecondFraction = ShuttleTimer.Interval / 1000
            HR = Graph.KO_IKeystone.ActivateFFRW(Speed)
            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            ShuttleTimer.Start()
        End Sub

        Private Sub CancelShuttle()
            Debug.WriteLine("CancelShuttle()")

            Select Case PlayState
                Case ePlayState.FastForwarding
                    ToggleAudioRendererConnection(False)
                    HR = Graph.MediaSeek.SetRate(1.0)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Case ePlayState.Rewinding
                    If ShuttleTimer IsNot Nothing Then ShuttleTimer.Stop()
                    nVidia_AudioMute(False)

            End Select

            SetActiveVideoFrameMode(False)
            HR = Graph.KO_IKeystone.DeactivateFFRW
            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            CurrentSpeed = eShuttleSpeed.OneX
            PlayState = ePlayState.Playing
        End Sub

        Private Sub ShuttleTimer_Tick(ByVal sender As System.Object, ByVal e As System.Timers.ElapsedEventArgs) Handles ShuttleTimer.Elapsed
            Try
                If PlayState = ePlayState.Stopped Then CancelShuttle()
                Dim CP As Long
                HR = Graph.MediaSeek.GetCurrentPosition(CP)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                If ShuttleForward Then
                    CP += CurrentSpeed * (10000000 * Me.TimerIntervalSecondFraction)
                Else
                    CP -= CurrentSpeed * (10000000 * Me.TimerIntervalSecondFraction)
                End If
                'Debug.WriteLine(CP)
                SetPosition(CP)
            Catch ex As Exception
                Debug.WriteLine("Problem with ShuttleTimer_Ticker. Error: " & ex.Message)
                CancelShuttle()
            End Try
        End Sub

#End Region 'M2TS:SHUTTLE

#Region "M2TS:CLASSES"

        Public Class cM2TS_StreamSet

            Public SetType As cStreamInfo.ePrimarySecondary

            Public Sub New(ByRef nStreams As List(Of cStreamInfo), ByVal nSetType As cStreamInfo.ePrimarySecondary)
                _Streams = nStreams
                SetType = nSetType
            End Sub

            Public ReadOnly Property Streams(ByVal ESType As cStreamInfo.eCategories_General) As List(Of cStreamInfo)
                Get
                    Dim out As New List(Of cStreamInfo)

                    Select Case ESType

                        Case cStreamInfo.eCategories_General.Not_Specified
                            For Each s As cStreamInfo In _Streams
                                out.Add(s)
                            Next
                            Return out

                        Case cStreamInfo.eCategories_General.Video
                            For Each s As cStreamInfo In _Streams
                                If s.Category = IIf(SetType = cStreamInfo.ePrimarySecondary.Primary, cStreamInfo.eCategories.PrimaryVideo, cStreamInfo.eCategories.SecondaryVideo) Then
                                    out.Add(s)
                                    Return out
                                End If
                            Next

                        Case cStreamInfo.eCategories_General.Audio
                            For Each s As cStreamInfo In _Streams
                                If s.Category = IIf(SetType = cStreamInfo.ePrimarySecondary.Primary, cStreamInfo.eCategories.PrimaryAudio, cStreamInfo.eCategories.SecondaryAudio) Then
                                    out.Add(s)
                                End If
                            Next

                        Case cStreamInfo.eCategories_General.Graphics
                            If SetType = cStreamInfo.ePrimarySecondary.Primary Then 'only give graphics streams for primary
                                For Each s As cStreamInfo In _Streams
                                    If s.Category = cStreamInfo.eCategories.InteractiveGraphics Or s.Category = cStreamInfo.eCategories.PresentationGraphics Or s.Category = cStreamInfo.eCategories.TextSubtitles Then
                                        out.Add(s)
                                    End If
                                Next
                            End If

                    End Select

                    Return out
                End Get
            End Property
            Private _Streams As List(Of cStreamInfo)

        End Class

#End Region 'M2TS:CLASSES

#End Region 'M2TS-SPECIFIC FUNCTIONALITY

    End Class

End Namespace
