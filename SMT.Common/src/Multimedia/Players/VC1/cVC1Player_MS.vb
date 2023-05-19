Imports SMT.HardwareControl.Serial
Imports SMT.Multimedia.DirectShow
Imports SMT.Multimedia.Filters.SMT.Keystone
Imports SMT.Multimedia.Filters.SMT.AMTC
Imports SMT.Multimedia.Filters.SMT.L21G
Imports SMT.DotNet.Utility
Imports Elecard.ModuleConfigInterface
Imports System.IO
Imports System.Runtime.InteropServices
Imports Microsoft.Win32
Imports System.Drawing
Imports System.Windows.Forms
Imports SMT.Multimedia.Enums
Imports SMT.Multimedia.Formats.StillImage
Imports SMT.Multimedia.GraphConstruction

Namespace Multimedia.Players.VC1

    Public Class cVC1Player_MS
        '        Inherits cBasePlayer

        '        Private HR As Integer

        '        Public Sub New()
        '        End Sub

        '#Region "cBasePlayer Overrides"

        '        Public Overrides Function BuildGraph(ByVal FilePath As String, ByVal NotifyWindowHandle As Integer, ByVal UseVMR9 As Boolean, ByVal nParentForm As Form) As Boolean
        '            Try
        '                Graph = New cSMTGraph(NotifyWindowHandle)
        '                ParentForm = nParentForm

        '                Graph.VideoWin = CType(Graph, IVideoWindow)

        '                'File source filter
        '                If Not Graph.AddFilesourceA(FilePath) Then Return False

        '                ''MCE Demuxer & vc1_to_vc1
        '                'If Not AddMCE_DMX(Graph) Then Return False
        '                'If Not AddMCE_VC12VC1(Graph) Then Return False

        '                'MS VC-1 ES Parser
        '                If Not Graph.AddMS_VC1ESParserA() Then Exit Function

        '                'MS VC-1 DMO
        '                If Not Graph.AddVC1DMOA() Then Return False
        '                _CurrentDecoder = Graph.DMOWrapper

        '                'Connect Pins

        '                'Using MC VC-1 ES Parser
        '                'HR = Graph.GraphBuilder.Connect(Me.FSF_Out, MCE_DMX_In)
        '                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                'HR = MCE_DMX.FindPin("VC1", MCE_DMX_Out)
        '                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                'HR = Graph.GraphBuilder.Connect(MCE_DMX_Out, MCE_VC12VC1_In)
        '                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                'HR = Graph.GraphBuilder.Connect(MCE_VC12VC1_Out, MS_VC1_In)
        '                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                'Using MS VC-1 ES Parsr
        '                HR = Graph.GraphBuilder.Connect(Graph.FSFA_Out, Graph.MS_VC1ESParserA_In)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                HR = Graph.MS_VC1ESParserA.FindPin("Output", Graph.MS_VC1ESParserA_Out)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                HR = Graph.GraphBuilder.Connect(Graph.MS_VC1ESParserA_Out, Graph.MS_VC1A_In)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                If Not UseVMR9 Then

        '                    'Keystone
        '                    If Not Graph.AddKeystoneOMNI() Then Return False

        '                    'DLV
        '                    If Not Graph.AddDeckLinkVideo() Then Return False

        '                    HR = Graph.GraphBuilder.Connect(Graph.MS_VC1A_Out, Graph.KO_In)
        '                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                    HR = Graph.GraphBuilder.Connect(Graph.KO_Out, Graph.DLV_In)
        '                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                Else

        '                    ''Keystone
        '                    'If Not AddKeystoneOMNI(Graph) Then Return False

        '                    Graph.AddVMR9(1)

        '                    'decoder to vmr9
        '                    HR = Graph.GraphBuilder.Connect(Graph.MS_VC1A_Out, Graph.VMR9_In_1)
        '                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                    'decoder to key to vmr9
        '                    'HR = Graph.GraphBuilder.Connect(MS_VC1_Out, KO_In)
        '                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                    'HR = Graph.GraphBuilder.Connect(KO_Out, Me.VMR9_In_1)
        '                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                    'commented on creation of the new "Players" code structure
        '                    'Dim VW As New frmViewer
        '                    'VW.Width = 1600
        '                    'VW.Height = 900
        '                    'VW.Show()
        '                    'VW.Top = 0
        '                    'VW.Left = 1280
        '                    'VideoWin.put_Owner(VW.Handle)
        '                    'VideoWin.put_WindowStyle(WS_CHILD Or WS_CLIPSIBLINGS Or WS_CLIPCHILDREN)
        '                    'VideoWin.put_MessageDrain(VW.Handle)
        '                    'VideoWin.SetWindowPosition(0, 0, VW.ClientRectangle.Right, VW.ClientRectangle.Bottom)

        '                End If

        '                'Seeking Setup
        '                'Dim SC As SeekingCapabilities
        '                'Seek.GetCapabilities(SC)
        '                'Debug.WriteLine("Seeking Capabilities: " & SC)

        '                While _Duration = 0
        '                    HR = Graph.MediaSeek.GetDuration(_Duration) '11844600000
        '                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                End While

        '                'Dim Duration As Long = 0
        '                'While Duration = 0
        '                '    HR = Seek.GetDuration(Duration) '11844600000
        '                '    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                'End While
        '                'Me.tbPlayPosition.Maximum = Duration / 10000000
        '                'Me.tbPlayPosition.TickFrequency = Me.tbPlayPosition.Maximum / 100
        '                'Me.tbPlayPosition.LargeChange = 10
        '                'Me.tbPlayPosition.SmallChange = 1

        '                'Duration /= 10000000 'Get total seconds

        '                'Dim tmp As String
        '                'Dim dh As String
        '                'If Duration > 3600 Then
        '                '    tmp = CStr(Duration / 3600)
        '                '    dh = MakeTwoDig(Microsoft.VisualBasic.Left(tmp, 1))
        '                '    Duration -= (dh * 3600)
        '                'Else
        '                '    dh = "00"
        '                'End If

        '                'Dim dm As String
        '                'If Duration > 60 Then
        '                '    tmp = CStr(Duration / 60)
        '                '    Dim t() As String = Split(tmp, ".", -1, CompareMethod.Text)
        '                '    dm = MakeTwoDig(t(0))
        '                '    Duration -= (dm * 60)
        '                'Else
        '                '    dm = "00"
        '                'End If

        '                'Dim ds As String = MakeTwoDig(Duration)
        '                'lblDuration.Text = dh & ":" & dm & ":" & ds

        '                'DISABLE CONTROLS
        '                'TransportControlStatus(True)

        '                Return True
        '            Catch ex As Exception
        '                Throw New Exception("Problem with BuildVideoGraph VC1. Error: " & ex.Message)
        '                Return False
        '            End Try
        '        End Function

        '        Public Overrides Function QuitFrameStepping() As Boolean
        '            Try
        '                HR = Graph.KO_IKeystone.QuitFrameStepping
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '            Catch ex As Exception
        '                Throw New Exception("Problem with QuitFrameStepping(). Error: " & ex.Message)
        '            End Try
        '        End Function

        '        Public Overrides Function Play() As Boolean
        '            Try
        '                Select Case PlayState
        '                    Case ePlayState.FrameStepping
        '                        QuitFrameStepping()
        '                        PlayState = ePlayState.Playing

        '                    Case ePlayState.Playing
        '                        Me.Pause()

        '                    Case ePlayState.FastForwarding, ePlayState.Rewinding
        '                        Me.StopShuttle()
        '                        Graph.MediaSeek.SetRate(1.0)

        '                    Case ePlayState.Paused
        '                        Graph.MediaCtrl.Run()
        '                        PlayState = ePlayState.Playing

        '                    Case Else
        '                        HR = Graph.MediaCtrl.Run()
        '                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                        PlayState = ePlayState.Playing

        '                End Select

        '                If Not Graph.DeckControl Is Nothing Then Graph.DeckControl.Play()
        '                CurrentSpeed = eShuttleSpeed.OneX

        '            Catch ex As Exception
        '                Throw New Exception("Problem with StartPlayback. Error: " & ex.Message)
        '            End Try
        '        End Function

        '        Public Overrides Function Pause() As Boolean
        '            Try
        '                HR = Graph.MediaCtrl.Pause()
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                PlayState = ePlayState.Paused
        '            Catch ex As Exception
        '                Throw New Exception("Problem with fileplayback-pause. Error: " & ex.Message)
        '            End Try
        '        End Function

        '        Public Overrides Function [Stop]() As Boolean
        '            If Graph.MediaCtrl Is Nothing Then Exit Function
        '            HR = Graph.MediaCtrl.Stop()
        '            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '            Graph.DestroyGraph()
        '            PlayState = ePlayState.Stopped
        '        End Function

        '        Public Overrides Function FastForward() As Boolean
        '            Select Case CurrentSpeed
        '                Case eShuttleSpeed.OneX
        '                    CurrentSpeed = eShuttleSpeed.TwoX
        '                Case eShuttleSpeed.TwoX
        '                    CurrentSpeed = eShuttleSpeed.FourX
        '                Case eShuttleSpeed.FourX
        '                    CurrentSpeed = eShuttleSpeed.EightX
        '                Case eShuttleSpeed.EightX
        '                    CurrentSpeed = eShuttleSpeed.SixteenX
        '                Case eShuttleSpeed.SixteenX
        '                    CurrentSpeed = eShuttleSpeed.ThirtyTwoX
        '                Case eShuttleSpeed.ThirtyTwoX
        '                    Debug.WriteLine("32x is top play speed.")
        '            End Select
        '            StartShuttle(CurrentSpeed, True)
        '            PlayState = ePlayState.FastForwarding
        '        End Function

        '        Public Overrides Function Rewind() As Boolean
        '            Select Case CurrentSpeed
        '                Case eShuttleSpeed.OneX
        '                    CurrentSpeed = eShuttleSpeed.TwoX
        '                Case eShuttleSpeed.TwoX
        '                    CurrentSpeed = eShuttleSpeed.FourX
        '                Case eShuttleSpeed.FourX
        '                    CurrentSpeed = eShuttleSpeed.EightX
        '                Case eShuttleSpeed.EightX
        '                    CurrentSpeed = eShuttleSpeed.SixteenX
        '                Case eShuttleSpeed.SixteenX
        '                    CurrentSpeed = eShuttleSpeed.ThirtyTwoX
        '                Case eShuttleSpeed.ThirtyTwoX
        '                    Debug.WriteLine("32x is top play speed.")
        '            End Select
        '            StartShuttle(CurrentSpeed, False)
        '            PlayState = ePlayState.Rewinding
        '        End Function

        '        Public Overrides Function Timesearch() As Boolean

        '        End Function

        '        Public Overrides Sub ClearOSD()

        '        End Sub

        '        Public Overrides Function FrameStep() As Boolean
        '            Try
        '                HR = Graph.KO_IKeystone.FrameStep(True)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                PlayState = ePlayState.FrameStepping
        '            Catch ex As Exception
        '                Throw New Exception("Problem with FrameStep(). Error: " & ex.Message)
        '            End Try
        '        End Function

        '        Public Overrides Function GetPosition() As Long
        '            Try
        '                Dim pCur, pStop As Long
        '                HR = Graph.MediaSeek.GetPositions(pCur, pStop)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                Return pCur
        '            Catch ex As Exception
        '                Debug.Write("Problem with GetPosition. Error: " & ex.Message)
        '            End Try
        '        End Function

        '        Public Overrides Function GetBitmapFromKeystone() As Byte()
        '            Try
        '                Dim SamplePtr As IntPtr
        '                Dim SampSize, SampW, SampH As Integer

        '                HR = Graph.KO_IKeystone.GrabSample(eSampleFrom.Output, SamplePtr, SampSize, SampW, SampH)
        '                If Math.Abs(HR) = 2147483655 Or Math.Abs(HR) = 2147467260 Then
        '                    Throw New Exception("FRAMEGRAB TIMEOUT: No samples received in three seconds.")
        '                    Return Nothing
        '                End If
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                Dim OutputFileSize As Integer = SampSize + 54

        '                'Read raster data buffer from memory
        '                Dim Buffer(SampSize - 1) As Byte
        '                'Debug.WriteLine("FG Source: " & SamplePtr.ToInt32)
        '                Marshal.Copy(SamplePtr, Buffer, 0, SampSize)

        '                ''DEBUGGING - Convert UYVY bytes to RGB24
        '                'Dim CSC As New ColorSpaceConversions
        '                'Buffer = CSC.ConvertUYVYtoRGB24(Buffer)
        '                'OutputFileSize = Buffer.Length + 54
        '                ''END DEBUGGING

        '                ''DEBUGGING - Save the buffer
        '                'FS = New FileStream("C:\Test\Buffer.bin", FileMode.OpenOrCreate)
        '                'FS.Write(Buffer, 0, Buffer.Length)
        '                'FS.Close()
        '                'FS = Nothing
        '                ''END DEBUGGING

        '                'Flip Image
        '                Buffer = FlipImageBuffer_Vertically(Buffer)
        '                Buffer = FlipRGB24ImageBuffer_Horizontally(Buffer, SampW, SampH)

        '                'Make BMI header
        '                Dim BMI(53) As Byte
        '                BMI(0) = 66 'B
        '                BMI(1) = 77 'M

        '                Dim TmpBuff() As Byte

        '                'FileSize
        '                TmpBuff = ConvertDecimalIntoByteArray(OutputFileSize, 4)
        '                BMI(2) = TmpBuff(0)
        '                BMI(3) = TmpBuff(1)
        '                BMI(4) = TmpBuff(2)
        '                BMI(5) = TmpBuff(3)

        '                'Old way
        '                'Dim HexSize As String = Hex(OutputFileSize)
        '                'BMI(2) = CInt("&H" & Microsoft.VisualBasic.Right(HexSize, 2))
        '                'BMI(3) = CInt("&H" & Microsoft.VisualBasic.Left(Mid(HexSize, 2), 2))
        '                'BMI(4) = CInt("&H" & Microsoft.VisualBasic.Left(HexSize, 1))

        '                BMI(10) = 54 'Header size
        '                BMI(14) = 40 'InfoHeader size

        '                'Width
        '                TmpBuff = ConvertDecimalIntoByteArray(SampW, 4)
        '                BMI(18) = TmpBuff(0)
        '                BMI(19) = TmpBuff(1)
        '                BMI(20) = TmpBuff(2)
        '                BMI(21) = TmpBuff(3)

        '                'Height
        '                TmpBuff = ConvertDecimalIntoByteArray(SampH, 4)
        '                BMI(22) = TmpBuff(0)
        '                BMI(23) = TmpBuff(1)
        '                BMI(24) = TmpBuff(2)
        '                BMI(25) = TmpBuff(3)

        '                BMI(26) = 1 'Planes
        '                BMI(28) = 24 'Bit depth

        '                BMI(38) = 196
        '                BMI(39) = 14
        '                BMI(42) = 196
        '                BMI(43) = 14

        '                Dim SampleBitmapBuffer(OutputFileSize) As Byte
        '                Array.Copy(BMI, 0, SampleBitmapBuffer, 0, 54)
        '                Array.Copy(Buffer, 0, SampleBitmapBuffer, 54, Buffer.Length)

        '                'Debugging - Dump the bitmap
        '                'FS = New FileStream("C:\Temp_" & DateTime.Now.Ticks & ".bmp", FileMode.OpenOrCreate)
        '                'FS.Write(SampleBitmapBuffer, 0, SampleBitmapBuffer.Length)
        '                'FS.Close()
        '                'FS = Nothing
        '                'End Debugging

        '                Buffer = Nothing
        '                BMI = Nothing
        '                Return SampleBitmapBuffer
        '            Catch ex As Exception
        '                Throw New Exception("Problem with GetSampleFromKeystone. Error: " & ex.Message)
        '                Return Nothing
        '            End Try
        '        End Function

        '        Public Overrides Function GetYUVSampleFromKeystone() As Byte()
        '            Try
        '                Dim SamplePtr As IntPtr
        '                Dim SampSize, SampW, SampH As Integer

        '                HR = Graph.KO_IKeystone.GrabSample(eSampleFrom.Output, SamplePtr, SampSize, SampW, SampH)
        '                If Math.Abs(HR) = 2147483655 Or Math.Abs(HR) = 2147467260 Then
        '                    Throw New Exception("FRAMEGRAB TIMEOUT: No samples received in three seconds.")
        '                    Return Nothing
        '                End If
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                Dim Buffer(SampSize - 1) As Byte
        '                Marshal.Copy(SamplePtr, Buffer, 0, SampSize)
        '                Return Buffer
        '            Catch ex As Exception
        '                Throw New Exception("Problem with GetYUVSampleFromKeystone(). Error: " & ex.Message)
        '            End Try
        '        End Function

        '        Public Overrides Function JumpBack(ByVal JumpSeconds As Byte) As Boolean
        '            Try
        '                Dim CP As Long
        '                HR = Graph.MediaSeek.GetCurrentPosition(CP)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                CP -= 50000000
        '                SetPosition(CP)
        '                'seek.SetPositions(CP, SeekingFlags.AbsolutePositioning, 0, SeekingFlags.NoPositioning)
        '            Catch ex As Exception
        '                Throw New Exception("Problem with JumpBack. Error: " & ex.Message)
        '            End Try
        '        End Function

        '        Public Overrides Function RenderBitmap(ByVal BM As System.Drawing.Bitmap, ByVal X As Short, ByVal Y As Short) As Boolean
        '            Try
        '                'Put RGB24 raster data into a byte array
        '                Dim BMData As Imaging.BitmapData = BM.LockBits(New Rectangle(0, 0, BM.Width, BM.Height), Imaging.ImageLockMode.ReadWrite, Imaging.PixelFormat.Format24bppRgb)
        '                Dim Bytes(BM.Height * BM.Width * 3 - 1) As Byte
        '                Dim s As Integer = 0
        '                Dim t As Integer = 0
        '                Dim Offset As Short = Math.Abs((BMData.Width * 3) - BMData.Stride)
        '                For l As Short = 1 To BMData.Height
        '                    For p As Short = 1 To BMData.Width
        '                        Bytes(t) = Marshal.ReadByte(BMData.Scan0, s)
        '                        Bytes(t + 1) = Marshal.ReadByte(BMData.Scan0, s + 1)
        '                        Bytes(t + 2) = Marshal.ReadByte(BMData.Scan0, s + 2)
        '                        t += 3
        '                        s += 3
        '                    Next
        '                    s += Offset
        '                Next
        '                BM.UnlockBits(BMData)
        '                BMData = Nothing

        '                'Get pointer to the RGB24 raster buffer
        '                Dim h1 As GCHandle = GCHandle.Alloc(Bytes, GCHandleType.Pinned)
        '                Dim BufferPtr As IntPtr = h1.AddrOfPinnedObject

        '                'render it
        '                HR = Graph.KO_IKeystone.ShowBitmap(BufferPtr.ToInt32, BM.Width, BM.Height, X, Y, eImageFormat.IF_RGB24)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                BM.Dispose()
        '                BM = Nothing
        '            Catch ex As Exception
        '                Throw New Exception("Problem with RenderBitmap(). Error: " & ex.Message)
        '            End Try
        '        End Function

        '        Public Overrides Function RestartStream() As Boolean
        '            SetPosition(0)
        '        End Function

        '        Public Overrides Sub SetOSD(ByVal BM As System.Drawing.Bitmap, ByVal ColorKey As Integer, ByVal DelayMiliSecs As Short, ByVal UseColorKey As Boolean, ByVal Alpha As Single, ByVal Sender As String, ByVal X As Short, ByVal Y As Short)
        '            Try
        '                'If OSDIsActive Then
        '                '    Me.ClearOSD("SetOSD")
        '                'End If
        '                'OSDIsActive = True

        '                'Put RGB24 raster data into a byte array
        '                Dim BMData As Imaging.BitmapData = BM.LockBits(New Rectangle(0, 0, BM.Width, BM.Height), Imaging.ImageLockMode.ReadWrite, Imaging.PixelFormat.Format24bppRgb)
        '                Dim Bytes(BM.Height * BM.Width * 3 - 1) As Byte
        '                Dim s As Integer = 0
        '                Dim t As Integer = 0
        '                Dim Offset As Short = Math.Abs((BMData.Width * 3) - BMData.Stride)
        '                For l As Short = 1 To BMData.Height
        '                    For p As Short = 1 To BMData.Width
        '                        Bytes(t) = Marshal.ReadByte(BMData.Scan0, s)
        '                        Bytes(t + 1) = Marshal.ReadByte(BMData.Scan0, s + 1)
        '                        Bytes(t + 2) = Marshal.ReadByte(BMData.Scan0, s + 2)
        '                        t += 3
        '                        s += 3
        '                    Next
        '                    s += Offset
        '                Next
        '                BM.UnlockBits(BMData)
        '                BMData = Nothing

        '                'For debugging
        '                'Dim FS As New FileStream("C:\Temp\RasterBytes.bin", FileMode.OpenOrCreate)
        '                'FS.Write(Bytes, 0, Bytes.Length)
        '                'FS.Close()
        '                'FS = Nothing
        '                'BM.Save("C:\Temp\OSDImage_Afterlockbits.bmp", System.Drawing.Imaging.ImageFormat.Bmp)
        '                'End Debugging

        '                'Get pointer to the RGB24 raster buffer
        '                Dim h1 As GCHandle = GCHandle.Alloc(Bytes, GCHandleType.Pinned)
        '                Dim BufferPtr As IntPtr = h1.AddrOfPinnedObject

        '                'Debugging
        '                'Debug.WriteLine("OSD ptr: " & Hex(BufferPtr.ToInt32))
        '                'Me.AddConsoleLine("OSD ptr: " & Hex(BufferPtr.ToInt32))

        '                ''Confirm that the pointer being passed points to an array that contains exactly the
        '                ''same bytes as we got from the locked bitmap above. Test confirms correct behavior.
        '                'Dim TestBytes(BM.Height * BM.Width * 3) As Byte
        '                'For i As Integer = 0 To UBound(TestBytes)
        '                '    TestBytes(i) = Marshal.ReadByte(BufferPtr, i)
        '                'Next
        '                'Dim FS1 As New FileStream("C:\Temp\TestBytes.bin", FileMode.OpenOrCreate)
        '                'FS1.Write(TestBytes, 0, Bytes.Length)
        '                'FS1.Close()
        '                'FS1 = Nothing
        '                'End Debugging

        '                'debugging
        '                'used this to confirm that the data we're sending is good
        '                'Dim tBM As Bitmap = SampleGrabber.GetBitmapFromRGB24Raster(Bytes, BM.Width, BM.Height)
        '                'tBM.Save("C:\Temp\Temp_justbeforesendtokeystone.bmp", System.Drawing.Imaging.ImageFormat.Bmp)
        '                'tBM.Dispose()
        '                'tBM = Nothing
        '                'debugging

        '                HR = Graph.KO_IKeystoneMixer.put_OSD(BufferPtr.ToInt32, BM.Width, BM.Height, X, Y, eImageFormat.IF_RGB24, ColorKey, DelayMiliSecs)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                BM.Dispose()
        '                BM = Nothing

        '                'Dim HDC_PTR As IntPtr = Me.GetHDCForBM(BM)
        '                ''Dim BMP_PTR As IntPtr = Me.GetPointerFromObject(BM)

        '                'Dim HR As Integer = ifKeystone.put_OSD(HDC_PTR, BM.Width, BM.Height, 50, 50, ImageFormat.IF_RGB24, 0)
        '                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

        '                ''Now do delay
        '                'OSDTimer = New System.Windows.Forms.Timer
        '                'OSDTimer.Interval = DelayMiliSecs
        '                'AddHandler OSDTimer.Tick, AddressOf ClearOSD
        '                'OSDTimer.Start()
        '            Catch ex As Exception
        '                Throw New Exception("problem setting bitmap. sender: " & Sender & " error: " & ex.Message)
        '            End Try
        '        End Sub

        '        Public Overrides Function SetPosition(ByVal Pos As Long) As Boolean
        '            Try
        '                HR = Graph.MediaSeek.SetPositions(Pos, SeekingFlags.AbsolutePositioning, 0, SeekingFlags.NoPositioning)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                Return True
        '            Catch ex As Exception
        '                Debug.Write("Problem with SetPosition. Error: " & ex.Message)
        '                Return False
        '            End Try
        '        End Function

        '        Public Overrides Sub StartSampleRecording(ByVal TargetDirectory As String)
        '            Try
        '                If NowRecording Then Exit Sub
        '                If Not Directory.Exists(TargetDirectory) Then Directory.CreateDirectory(TargetDirectory)
        '                If Graph.KO_IKeystone Is Nothing Then Exit Sub
        '                Graph.KO_IKeystone.StartRecording(Replace(TargetDirectory, "\", "\\"))
        '                NowRecording = True
        '            Catch ex As Exception
        '                Throw New Exception("Problem with StartSampleRecording(). Error: " & ex.Message)
        '            End Try
        '        End Sub

        '        Public Overrides Sub StopSampleRecording()
        '            Try
        '                If Not NowRecording Then Exit Sub
        '                Graph.KO_IKeystone.StopRecording()
        '                NowRecording = False
        '            Catch ex As Exception
        '                Throw New Exception("Problem with StopRecording(). Error: " & ex.Message)
        '            End Try
        '        End Sub

        '        Public Overrides Function Dispose() As Boolean
        '            Graph.DestroyGraph()
        '        End Function

        '        Public Overrides Function GrabSingleSample(ByVal TargetDirectory As String, ByVal Format As System.Drawing.Imaging.ImageFormat) As Boolean

        '        End Function

        '        Public Overrides Function FilterCheck() As Boolean

        '        End Function

        '#End Region 'Overrides

        '#Region "EVENT HANDLER"

        '        Public Overrides Sub DerivedPlayerHandleEvent(ByVal code As Integer, ByVal p1 As Integer, ByVal p2 As Integer)
        '            Try
        '                Select Case code

        '                End Select
        '            Catch ex As Exception
        '                Throw New Exception("Problem with DerivedPlayerHandleEvent(). Error: " & ex.Message)
        '            End Try
        '        End Sub

        '#End Region 'EVENT HANDLER

        '#Region "Shuttle"

        '        Private WithEvents ShuttleTimer As Timers.Timer
        '        Private ShuttleForward As Boolean
        '        Private Const ONE_SEC As Integer = 400000
        '        Private ShuttleStartPosition As Long
        '        Private ShuttlePosition As Long
        '        Private ShuttleStepSize As Long

        '        Public Sub StartShuttle(ByVal Speed As eShuttleSpeed, ByVal Forward As Boolean)
        '            Debug.WriteLine("StartShuttle: " & Speed)

        '            CurrentSpeed = Speed 'just to be sure (?)
        '            ShuttleForward = Forward
        '            ShuttleTimer.Start()

        '            'ShuttleStartPosition = GetPosition()
        '            'ShuttlePosition = ShuttleStartPosition
        '            'ShuttleStepSize = (ONE_SEC / 2) * Speed

        '            ShuttleTimer.Interval = 100
        '            Graph.KO_IKeystone.ActivateFFRW()

        '        End Sub

        '        Private Sub StopShuttle()
        '            Debug.WriteLine("StopShuttle")
        '            ShuttleTimer.Stop()
        '            If Not Graph.KO_IKeystone Is Nothing Then
        '                Graph.KO_IKeystone.DeactivateFFRW()
        '            End If
        '            CurrentSpeed = eShuttleSpeed.OneX
        '            PlayState = ePlayState.Playing
        '        End Sub

        '        Private Sub ShuttleTimer_Tick(ByVal sender As System.Object, ByVal e As System.Timers.ElapsedEventArgs) Handles ShuttleTimer.Elapsed
        '            Try
        '                Dim CP As Long
        '                HR = Graph.MediaSeek.GetCurrentPosition(CP)
        '                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '                If ShuttleForward Then
        '                    CP += CurrentSpeed * 1000000 '10000000
        '                Else
        '                    CP -= CurrentSpeed * 1000000
        '                End If
        '                Debug.WriteLine(CP)
        '                SetPosition(CP)
        '            Catch ex As Exception
        '                Throw New Exception("Problem with ShuttleTimer_Ticker. Error: " & ex.Message)
        '            End Try
        '        End Sub

        '#End Region 'Shuttle

    End Class

End Namespace
