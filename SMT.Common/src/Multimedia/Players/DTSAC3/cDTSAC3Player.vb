Imports SMT.HardwareControl.Serial
Imports SMT.Multimedia.DirectShow
Imports SMT.DotNet.Utility
Imports Elecard.ModuleConfigInterface
Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.Win32
Imports System.Drawing
Imports System.Windows.Forms
Imports SMT.Multimedia.GraphConstruction
Imports SMT.Multimedia.Enums

Namespace Multimedia.Players.DTSAC3Player

    Public Class cDTSAC3Player
        Inherits cBasePlayer

        Public Sub New()
        End Sub

#Region "cBasePlayer Overrides"

        Public Overrides Function BuildGraph(ByVal FilePath As String, ByVal NotifyWindowHandle As Integer, ByVal nAVMode As eAVMode, ByVal nParentForm As Form) As Boolean
            Try
                If Not FilterCheck() = "True" Then Return False
                Graph = New cSMTGraph(NotifyWindowHandle)
                ParentForm = New cSMTForm(nParentForm)

                'DTS-AC3 Source
                Graph.AddDTSAC3Source(FilePath)

                If AVMode = eAVMode.DesktopVMR Then
                    Graph.AddNVidiaAudioDecoder(True)
                    Graph.AddDefaultDirectSoundDevice()

                    HR = Graph.GraphBuilder.Connect(Graph.DTSAC3_Out, Graph.AudDec_InPin)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    HR = Graph.GraphBuilder.Render(Graph.AudDec_OutPin)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Else
                    Graph.AddNVidiaAudioDecoder(False)
                    Graph.AddAMTC()
                    Graph.AddDeckLinkAudio()
                    Graph.AddNullVideoSource()
                    Graph.AddKeystoneOmni()
                    Graph.AddDeckLinkVideo()

                    'Hookup
                    HR = Graph.GraphBuilder.Connect(Graph.NVS_Out, Graph.KO_In)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    HR = Graph.GraphBuilder.Connect(Graph.KO_Out, Graph.DLV_In)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    HR = Graph.GraphBuilder.Connect(Graph.DTSAC3_Out, Graph.AudDec_InPin)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    HR = Graph.GraphBuilder.Connect(Graph.AudDec_OutPin, Graph.AMTC_In)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    HR = Graph.GraphBuilder.Connect(Graph.AMTC_Out, Graph.AudRen_In)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                End If

                While _Duration = 0
                    HR = Graph.MediaSeek.GetDuration(_Duration)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End While

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with BuildGraph() - AC3/DTS. Error: " & ex.Message)
            End Try
        End Function

        Public Overrides Function Play() As Boolean
            Try
                Select Case PlayState
                    Case ePlayState.FrameStepping
                        QuitFrameStepping()
                        PlayState = ePlayState.Playing

                    Case ePlayState.Playing
                        Me.Pause()

                    Case ePlayState.FastForwarding, ePlayState.Rewinding
                        Graph.MediaSeek.SetRate(1.0)

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
                RaiseEvent_TransportControl(Enums.eTransportControlTypes.Play)
            Catch ex As Exception
                Throw New Exception("Problem with Play(). Error: " & ex.Message)
            End Try
        End Function

        Public Overrides Function Pause() As Boolean
            Try
                HR = Graph.MediaCtrl.Pause()
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                PlayState = ePlayState.Paused
                RaiseEvent_TransportControl(Enums.eTransportControlTypes.Pause)
            Catch ex As Exception
                Throw New Exception("Problem with fileplayback-pause. Error: " & ex.Message)
            End Try
        End Function

        Public Overrides Function [Stop]() As Boolean
            If Graph.MediaCtrl Is Nothing Then Exit Function
            HR = Graph.MediaCtrl.Stop()
            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            PlayState = ePlayState.Stopped
            RaiseEvent_TransportControl(Enums.eTransportControlTypes.Stop)
        End Function

        Public Overrides Function FastForward(Optional ByVal Speed As eShuttleSpeed = eShuttleSpeed.Not_Specified) As Boolean
            HR = Graph.MediaSeek.SetRate(CurrentSpeed)
            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            RaiseEvent_TransportControl(Enums.eTransportControlTypes.FastForward)
        End Function

        Public Overrides Function Rewind(Optional ByVal Speed As eShuttleSpeed = eShuttleSpeed.Not_Specified) As Boolean
            Dim d As Double = CurrentSpeed - (2 * CurrentSpeed)
            HR = Graph.MediaSeek.SetRate(d)
            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            RaiseEvent_TransportControl(Enums.eTransportControlTypes.Rewind)
        End Function

        Public Overrides Function FrameStep() As Boolean
            Try
                HR = Graph.KO_IKeystone.FrameStep(True)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                PlayState = ePlayState.FrameStepping
            Catch ex As Exception
                Throw New Exception("Problem with FrameStep(). Error: " & ex.Message)
            End Try
        End Function

        Public Overrides Function QuitFrameStepping() As Boolean

        End Function

        Public Overrides Function Timesearch() As Boolean

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

        Public Overrides Function RestartStream() As Boolean
            SetPosition(0)
        End Function

        Public Overrides Sub SetOSD(ByVal BM As Bitmap, ByVal ColorKey As Integer, ByVal DelayMiliSecs As Short, ByVal UseColorKey As Boolean, ByVal Alpha As Single, ByVal Sender As String, ByVal X As Short, ByVal Y As Short)
            'Not implemented
        End Sub

        Public Overrides Sub ClearOSD()
        End Sub

        Public Overrides Function RenderBitmap(ByVal BM As Bitmap, ByVal X As Short, ByVal Y As Short) As Boolean
            'Not implemented
        End Function

        Public Overrides Function SetPosition(ByVal Pos As Long) As Boolean
            Try
                HR = Graph.MediaSeek.SetPositions(Pos, SeekingFlags.AbsolutePositioning, 0, SeekingFlags.NoPositioning)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Debug.Write("Problem with SetPosition. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Overrides Sub StartSampleRecording(ByVal TargetDirectory As String)
            Try

            Catch ex As Exception
                Throw New Exception("Problem with StartSampleRecording(). Error: " & ex.Message)
            End Try
        End Sub

        Public Overrides Sub StopSampleRecording()
            Try

            Catch ex As Exception
                Throw New Exception("Problem with StopRecording(). Error: " & ex.Message)
            End Try
        End Sub

        Public Overrides Function GetBitmapFromKeystone() As Byte()
            Throw New Exception("Not implemented.")
        End Function

        Public Overrides Function GetYUVSampleFromKeystone() As Byte()
            Throw New Exception("Not implemented.")
        End Function

        Public Overrides Function GrabSingleSample(ByVal TargetDirectory As String, ByVal Format As System.Drawing.Imaging.ImageFormat) As Boolean
            Throw New Exception("Not implemented.")
        End Function

        Public Overrides Function FilterCheck() As String
            Throw New Exception("Not implemented.")
        End Function

#End Region 'Overrides

#Region "EVENT HANDLING"

        Public Overrides Sub DerivedPlayerHandleEvent(ByVal code As Integer, ByVal p1 As Integer, ByVal p2 As Integer)
            Try
                Select Case code

                End Select
            Catch ex As Exception
                Throw New Exception("Problem with DerivedPlayerHandleEvent(). Error: " & ex.Message)
            End Try
        End Sub

#End Region 'EVENT HANDLING

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
