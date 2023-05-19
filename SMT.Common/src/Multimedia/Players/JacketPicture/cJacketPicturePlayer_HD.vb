Imports SMT.Multimedia.DirectShow
Imports SMT.Multimedia.Filters.SMT.Keystone
Imports SMT.DotNet.Utility
Imports System.Runtime.InteropServices
Imports System.Drawing
Imports System.Windows.Forms
Imports System.IO
Imports SMT.Multimedia.Enums
Imports SMT.Multimedia.Formats.StillImage
Imports SMT.Multimedia.GraphConstruction

Namespace Multimedia.Players.JacketPictures

    Public Class cJacketPicturePlayer_HD
        Inherits cBasePlayer

        Public Sub New()
        End Sub

#Region "cBasePlayer Overrides"

        Public Overrides Function BuildGraph(ByVal FilePath As String, ByVal NotifyWindowHandle As Integer, ByVal nAVMode As eAVMode, ByVal nParentForm As Form) As Boolean
            Try
                If Not FilterCheck() = "True" Then Return False
                Graph = New cSMTGraph(NotifyWindowHandle)
                ParentForm = New cSMTForm(nParentForm)

                If Not Graph.AddNullVideoSource_HD() Then Return False
                If Not Graph.AddKeystoneOmni() Then Return False

                If AVMode <> eAVMode.DesktopVMR Then
                    'DLV Renderer
                    If Not Graph.AddDeckLinkVideo() Then Return False

                    'Connect pins
                    HR = Graph.GraphBuilder.Connect(Graph.NVS_HD_Out, Graph.KO_In)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    HR = Graph.GraphBuilder.Connect(Graph.KO_Out, Graph.DLV_In)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    'HR = Graph.GBIMediaFilter.SetSyncSource(0)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    HR = Graph.GBIMediaFilter.SetSyncSource(Graph.GetSystemClock)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Else
                    'Add VMR9
                    If Not Graph.AddVMR9(1) Then Return False

                    'Connect pins
                    HR = Graph.GraphBuilder.Connect(Graph.NVS_HD_Out, Graph.KO_In)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    HR = Graph.GraphBuilder.Connect(Graph.KO_Out, Graph.VMR9_In_1)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    Me.SetupViewer(960, 540, False)

                End If

                Return True

            Catch ex As Exception
                Throw New Exception("Problem with BuildVideoGraph JacketPicturePlayer HD. Error: " & ex.Message & " Stack: " & ex.StackTrace)
                Return False
            End Try
        End Function

        Public Overrides Function Play() As Boolean
            Try
                HR = Graph.MediaCtrl.Run()
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                PlayState = ePlayState.SystemJP
            Catch ex As Exception
                Throw New Exception("Problem with StartPlayback. Error: " & ex.Message, ex)
            End Try
        End Function

        Public Overrides Function Pause() As Boolean
            'Try
            '    HR = Graph.MediaCtrl.Pause()
            '    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            '    PlayState = ePlayState.SystemJP
            'Catch ex As Exception
            '    Throw New Exception("Problem with Pause(). Error: " & ex.Message, ex)
            'End Try
        End Function

        Public Overrides Function [Stop]() As Boolean
        End Function

        Public Overrides Function FastForward(Optional ByVal Speed As eShuttleSpeed = eShuttleSpeed.Not_Specified) As Boolean
        End Function

        Public Overrides Function Rewind(Optional ByVal Speed As eShuttleSpeed = eShuttleSpeed.Not_Specified) As Boolean
        End Function

        Public Overrides Function Timesearch() As Boolean
        End Function

        Public Overrides Sub ClearOSD()
        End Sub

        Public Overrides Function FrameStep() As Boolean
        End Function

        Public Overrides Function QuitFrameStepping() As Boolean
        End Function

        Public Overrides Function JumpBack(ByVal JumpSeconds As Byte) As Boolean
        End Function

        Public Overrides Function GetPosition() As Long
        End Function

        Public Overrides Function SetPosition(ByVal Pos As Long) As Boolean
        End Function

        Public Overrides Function GetBitmapFromKeystone() As Byte()
        End Function

        Public Overrides Function GetYUVSampleFromKeystone() As Byte()
        End Function

        Public Overrides Function RestartStream() As Boolean
        End Function

        Public Overrides Sub StartSampleRecording(ByVal TargetDirectory As String)
        End Sub

        Public Overrides Sub StopSampleRecording()
        End Sub

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

        Public Overrides Sub SetOSD(ByVal BM As System.Drawing.Bitmap, ByVal ColorKey As Integer, ByVal DelayMiliSecs As Short, ByVal UseColorKey As Boolean, ByVal Alpha As Single, ByVal Sender As String, ByVal X As Short, ByVal Y As Short)
        End Sub

        Public Overrides Function GrabSingleSample(ByVal TargetDirectory As String, ByVal Format As System.Drawing.Imaging.ImageFormat) As Boolean
        End Function

        Public Overrides Function FilterCheck() As String
            Try
                Dim startstring As String = "The following components are not installed correctly:" & vbNewLine & vbNewLine
                Dim msgstr As String = startstring
                If Not SMT_KeystoneHD Then msgstr &= "SMT Keystone HD" & vbNewLine
                If msgstr = startstring Then Return "True"
                Return msgstr
            Catch ex As Exception
                Debug.WriteLine("Problem with FilterCheck(). Error: " & ex.Message)
                Return "False"
            End Try
        End Function

        Public Overrides Function RenderImageResource(ByVal PNGstr As Stream, ByVal X As Short, ByVal Y As Short) As Boolean
            Try
                Dim W, H As Integer
                If PNGstr Is Nothing Then Return False
                Dim bm_png As New Bitmap(PNGstr)
                W = bm_png.Width
                H = bm_png.Height
                Dim bm As New Bitmap(bm_png.Width, bm_png.Height, Imaging.PixelFormat.Format24bppRgb)
                bm.SetResolution(bm_png.HorizontalResolution, bm_png.VerticalResolution)
                Dim g As Graphics = Graphics.FromImage(bm)
                g.DrawImage(bm_png, 0, 0)
                g.Dispose()
                bm_png.Dispose()
                PNGstr.Close()
                PNGstr.Dispose()

                Dim MS As New MemoryStream(W * H * 3 + 54)
                bm.Save(MS, System.Drawing.Imaging.ImageFormat.Bmp)

                Dim RasterSize As Integer = W * H * 3
                Dim bytes(RasterSize - 1) As Byte
                MS.Position = 54
                MS.Read(bytes, 0, RasterSize)

                Array.Reverse(bytes)

                For s As Short = 0 To H - 1
                    Array.Reverse(bytes, s * 5760, 5760)
                Next

                'Get pointer to the RGB24 raster buffer
                Dim h1 As GCHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned)
                Dim BufferPtr As IntPtr = h1.AddrOfPinnedObject

                'render it
                HR = Graph.KO_IKeystone.ShowBitmap(BufferPtr.ToInt32, W, H, X, Y, eImageFormat.IF_RGB24)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                h1.Free()
                MS.Close()

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with RenderImageResource(). Error: " & ex.Message, ex)
            End Try
        End Function

#End Region 'Overrides

#Region "EVENT HANDLING"

        Public Overrides Sub DerivedPlayerHandleEvent(ByVal code As Integer, ByVal p1 As Integer, ByVal p2 As Integer)
            Try
                'Debug.WriteLine("EVT: " + code.ToString())
                If Not code = DsEvCode.EC_DVD_CURRENT_HMSF_TIME Then
                    'Debug.WriteLine("EVT: " + code.ToString())
                End If
                Select Case code
                    Case DsEvCode.EC_ERRORABORT
                        Me.Stop()
                        Try
                            Marshal.ThrowExceptionForHR(p1)
                        Catch ex As Exception
                            Debug.WriteLine("EC_ERRORABORT. Error: " & ex.Message)
                        End Try
                        'Throw New Exception("Playback aborted. Possibly due to end of stream. Error:" & EXCEPINFO.)

                    Case DsEvCode.EC_VMR_RENDERDEVICE_SET
                        Select Case p1
                            Case 1
                                Debug.WriteLine("VMR RENDERER: Overlay")
                            Case 2
                                Debug.WriteLine("VMR RENDERER: Video Memory")
                            Case 4
                                Debug.WriteLine("VMR RENDERER: System Memory")
                        End Select

                    Case DsEvCode.EC_STREAM_ERROR_STILLPLAYING
                        Marshal.ThrowExceptionForHR(p1)

                    Case 14
                        'EC_PAUSED
                        Debug.WriteLine("EC_PAUSED")

                    Case 13
                        'EC_CLOCK_CHANGED
                        Debug.WriteLine("EC_CLOCK_CHANGED")

                    Case 80
                        'EC_GRAPH_CHANGED
                        Debug.WriteLine("EC_GRAPH_CHANGED")

                    Case DsEvCode.EC_VIDEO_SIZE_CHANGED
                        'Dim xP1 As String = Hex(p1)
                        'Dim H, W As Short
                        'Dim sH, sW As Short
                        'sH = Microsoft.VisualBasic.Left(xP1, 3)
                        'sW = Microsoft.VisualBasic.Right(xP1, 3)
                        'H = HEXtoDEC(sH)
                        'W = HEXtoDEC(sW)
                        'AddConsoleLine("Video size changed. H: " & H & " W: " & W)

                    Case DsEvCode.EC_USERABORT
                        Debug.WriteLine("EVENT: User Abort")

                    Case Else
                        'Log unknown events
                        'Debug.WriteLine("Unknown event: " & code & " p1: " & p1 & " p2: " & p2)

                End Select
            Catch ex As Exception
                Throw New Exception("Problem with DerivedPlayerHandleEvent(). Error: " & ex.Message)
            End Try
        End Sub

#End Region 'EVENT HANDLING

    End Class

End Namespace
