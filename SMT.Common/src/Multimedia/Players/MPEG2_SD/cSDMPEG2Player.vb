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

Namespace Multimedia.Players.SDMPEG2

    Public Class cSDMPEG2Player
        Inherits cBasePlayer

        Public Sub New()
        End Sub

#Region "cBasePlayer Overrides"

        Public Overrides Function BuildGraph(ByVal FilePath As String, ByVal NotifyWindowHandle As Integer, ByVal nAVMode As eAVMode, ByVal nParentForm As Form) As Boolean
            Try
                If Not FilterCheck() = "True" Then Return False
                Graph = New cSMTGraph(NotifyWindowHandle)
                ParentForm = New cSMTForm(nParentForm)

                Graph.AddGraphToROT()

                'File source filter
                If Not Graph.AddFilesourceA(FilePath) Then Return False

                'Demuxer
                If Not Graph.AddMCE_DMXA() Then Return False


                If nAVMode <> eAVMode.DesktopVMR Then
                    'Decoder
                    'If Not Graph.AddMCE_MP2A() Then Return False
                    If Not Graph.AddNVidiaVideoDecoder(False, False) Then Return False
                    '_CurrentDecoder = Graph.MCE_MP2A
                    _CurrentDecoder = Graph.VSDecoder

                    'Keystone SD
                    If Not Graph.AddKeystoneOmni() Then Return False

                    'DLV Renderer
                    If Not Graph.AddDeckLinkVideo() Then Return False

                    'MainConcept Chegepuga
                    'If Not AddMCE_CHG(Graph) Then Return False

                    'Connect pins
                    HR = Graph.GraphBuilder.Connect(Graph.FSFA_Out, Graph.MCE_DMXA_In)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    HR = Graph.MCE_DMXA.FindPin("VES", Graph.MCE_DMXA_Out)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    ''if using mc m2v dec
                    'HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXA_Out, Graph.MCE_MP2A_In)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    'HR = Graph.MCE_MP2A.FindPin("Out", Graph.MCE_MP2A_Out)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    'HR = Graph.GraphBuilder.Connect(Graph.MCE_MP2A_Out, Graph.KO_In)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    ''HR = Graph.GraphBuilder.Connect(MCE_MP2A_Out, DLV_In)
                    ''If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    ' ''end if using mc m2v dec

                    'if using nv m2v dec
                    HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXA_Out, Graph.VidDec_Vid_In)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    HR = Graph.GraphBuilder.Connect(Graph.VidDec_Vid_Out, Graph.KO_In)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    'end if using nv m2v dec

                    'either way
                    HR = Graph.GraphBuilder.Connect(Graph.KO_Out, Graph.DLV_In)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    'use chegepuga
                    'HR = Graph.GraphBuilder.Connect(KO_Out, MCE_CHG_In)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Else
                    'Decoder
                    'If Not Graph.AddMCE_MP2A() Then Return False
                    If Not Graph.AddNVidiaVideoDecoder(False, True) Then Return False
                    _CurrentDecoder = Graph.MCE_MP2A
                    '_CurrentDecoder = Graph.VSDecoder

                    'Add VMR9
                    If Not Graph.AddVMR9(1) Then Return False

                    'Keystone SD
                    If Not Graph.AddKeystoneOmni() Then Return False

                    'Connect pins
                    HR = Graph.GraphBuilder.Connect(Graph.FSFA_Out, Graph.MCE_DMXA_In)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    'Graph.EnumeratePinNames(Graph.MCE_DMXA)

                    HR = Graph.MCE_DMXA.FindPin("VES", Graph.MCE_DMXA_Out)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    ''if using mc m2v dec
                    'HR = Graph.GraphBuilder.Connect(MED_Out, MCE_MP2_In)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    'HR = MCE_MP2.FindPin("Out", MCE_MP2_Out)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    'HR = Graph.GraphBuilder.Connect(MCE_MP2_Out, KO_In)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    'if using nv m2v dec
                    HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXA_Out, Graph.VidDec_Vid_In)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    'HR = Graph.GraphBuilder.Connect(Graph.VidDec_Vid_Out, Graph.KO_In)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    'HR = Graph.GraphBuilder.Connect(Graph.KO_Out, Graph.VMR9_In_1)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    HR = Graph.GraphBuilder.Connect(Graph.VidDec_Vid_Out, Graph.VMR9_In_1)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    SetupViewer(720, 480, False)

                End If

                'Seeking Setup
                'Dim SC As SeekingCapabilities
                'seek.GetCapabilities(SC)
                'If SC And SeekingCapabilities.CanGetStopPos Then
                '    Throw New Exception("hi")
                'End If

                While _Duration = 0
                    HR = Graph.MediaSeek.GetDuration(_Duration)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End While

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with BuildGraph() - SD M2V. Error: " & ex.Message)
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
                        Me.StopShuttle()
                        If Not Graph.KO_IKeystone Is Nothing Then Me.Graph.KO_IKeystone.DeactivateFFRW()

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
                Throw New Exception("Problem with Pause(). Error: " & ex.Message)
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
            End Select
            'HR = Graph.MediaSeek.SetRate(CurrentSpeed)
            'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Me.StartShuttle(CurrentSpeed, True)
            If Not Graph.KO_IKeystone Is Nothing Then HR = Graph.KO_IKeystone.ActivateFFRW(CurrentSpeed)
            Me.PlayState = ePlayState.FastForwarding
            RaiseEvent_TransportControl(Enums.eTransportControlTypes.FastForward)
        End Function

        Public Overrides Function Rewind(Optional ByVal Speed As eShuttleSpeed = eShuttleSpeed.Not_Specified) As Boolean
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
            End Select
            'HR = Graph.MediaSeek.SetRate(CurrentSpeed)
            'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Me.StartShuttle(CurrentSpeed, False)
            If Not Graph.KO_IKeystone Is Nothing Then HR = Graph.KO_IKeystone.ActivateFFRW(CurrentSpeed)
            Me.PlayState = ePlayState.Rewinding
            RaiseEvent_TransportControl(Enums.eTransportControlTypes.Rewind)
            'Dim d As Double = CurrentSpeed - (2 * CurrentSpeed)
            'Debug.WriteLine("rw speed: " & d)
            'If d < 0 Then
            '    StartShuttle(CurrentSpeed, False)
            'Else
            '    'this should never happen, right?
            '    HR = Graph.MediaSeek.SetRate(d)
            '    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            'End If
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

        Public Overrides Function Timesearch() As Boolean

        End Function

        Public Overrides Sub ClearOSD()

        End Sub

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

        Public Overrides Function QuitFrameStepping() As Boolean
            Try
                PlayState = ePlayState.Playing
                HR = Graph.KO_IKeystone.QuitFrameStepping
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with QuitFrameStepping. Error: " & ex.Message)
                Return False
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

        Public Overrides Function SetPosition(ByVal Pos As Long) As Boolean
            Try
                HR = Graph.MediaSeek.SetPositions(Pos, SeekingFlags.AbsolutePositioning, 0, SeekingFlags.NoPositioning)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with SetPosition(). Error: " & ex.Message)
                Return False
            End Try
        End Function

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

        Public Overrides Function GrabSingleSample(ByVal TargetDirectory As String, ByVal Format As System.Drawing.Imaging.ImageFormat) As Boolean
            Try
                If Format Is System.Drawing.Imaging.ImageFormat.Wmf Then
                    ' USER WANTS YUV
                    Dim Buffer() As Byte = GetYUVSampleFromKeystone()
                    If Not Directory.Exists(TargetDirectory) Then Directory.CreateDirectory(TargetDirectory)
                    TargetDirectory &= "Grab_" & Microsoft.VisualBasic.Right(DateTime.Now.Ticks.ToString, 6) & ".yuv"
                    Dim FS As New FileStream(TargetDirectory, FileMode.OpenOrCreate)
                    FS.Write(Buffer, 0, Buffer.Length)
                    FS.Close()
                    FS.Dispose()
                    Buffer = Nothing
                Else
                    'Get the RGB24 bits from Keystone
                    Dim Buffer() As Byte = GetBitmapFromKeystone()

                    If Buffer Is Nothing Then
                        Exit Function
                    End If

                    '' DETERMINE IMAGE SIZE
                    'Dim W, H As Short
                    'Dim RasterSize As Integer = Buffer.Length - 54

                    'If RasterSize / 1920 > 1080 Then
                    '    W = 1920
                    '    H = 1080
                    'ElseIf RasterSize / 720 = 480 Then
                    '    W = 720
                    '    H = 480
                    'ElseIf RasterSize / 720 = 486 Then
                    '    W = 720
                    '    H = 486
                    'ElseIf RasterSize / 720 = 576 Then
                    '    W = 720
                    '    H = 576
                    'End If

                    'Make the bitmap object
                    Dim MS As New MemoryStream(Buffer.Length)
                    MS.Write(Buffer, 0, Buffer.Length)
                    Dim BM As New Bitmap(MS)

                    'BM = Me.ScaleImage(BM, H, W)

                    'DEBUGGING
                    'BM.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) & "\test.bmp")

                    ''Save the bitmap
                    'Dim FS As New FileStream(tOutpath, FileMode.OpenOrCreate)
                    'FS.Write(Buffer, 0, Buffer.Length)
                    'FS.Close()
                    'FS = Nothing
                    'END DEBUGGING

                    'Get save location
                    If Not Directory.Exists(TargetDirectory) Then Directory.CreateDirectory(TargetDirectory)
                    TargetDirectory &= "Grab_" & Microsoft.VisualBasic.Right(DateTime.Now.Ticks.ToString, 6) & "." & Format.ToString

                    'Save the image
                    BM.Save(TargetDirectory, [Enum].Parse(GetType(System.Drawing.Imaging.ImageFormat), Format.ToString))
                    BM.Dispose()

                    'Clean up
                    BM = Nothing
                    MS.Close()
                    MS = Nothing

                    'Me.pbFrameGrab.Load(OutPath)
                    'Me.Height = 464

                End If
            Catch ex As Exception
                Throw New Exception("problem getting screengrab. error: " & ex.Message & " StackTrace: " & ex.StackTrace)
            End Try
        End Function

        Public Overrides Function FilterCheck() As String
            Return "True"
        End Function

#End Region 'Overrides

#Region "EVENT HANDLING"

        Public Overrides Sub DerivedPlayerHandleEvent(ByVal code As Integer, ByVal p1 As Integer, ByVal p2 As Integer)
            Try
                Select Case code
                    Case DsEvCode.EC_DVD_CURRENT_HMSF_TIME
                        'If Me.DVDStartPauseSetItNow Then
                        '    'DVDStartPauseSetItNow = False
                        '    'HR = MediaCtrl.Pause()
                        '    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        'End If

                        Dim ati As Byte() = BitConverter.GetBytes(p1)
                        '_CurrentRunningTime.bHours = ati(0)
                        '_CurrentRunningTime.bMinutes = ati(1)
                        '_CurrentRunningTime.bSeconds = ati(2)
                        '_CurrentRunningTime.bFrames = ati(3)
                        'RaiseEvent evRunningTimeTick()

                    Case DsEvCode.EC_QUALITY_CHANGE
                        Debug.WriteLine("EVENT: QualityChange")

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
                        Debug.WriteLine("EVENT: Stream Error")
                        Marshal.ThrowExceptionForHR(p1)

                    Case DsEvCode.EC_VIDEO_SIZE_CHANGED
                        Debug.WriteLine("EVENT: VideoSizeChanged")
                        'If p1 = 32778 Then SwitchVideoFormats(p2)

                    Case 88 'EC_QUALITY_CHANGE_KEYSTONE
                        Debug.WriteLine("EVENT: EC_QUALITY_CHANGE_KEYSTONE")
                        'EC_QUALITY_CHANGE_KEYSTONE
                        'RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Keystone Quality Change:" & vbNewLine & vbTab & "Proportion: " & p1 & vbNewLine & vbTab & "Late: " & p2)
                        'Debug.WriteLine("Keystone Quality Change:" & vbNewLine & vbTab & "Proportion: " & p1 & vbNewLine & vbTab & "Late: " & p2)

                    Case 89 'EC_KEYSTONE_32
                        Debug.WriteLine("EVENT: EC_KEYSTONE_32")
                        'Keystone 3:2 change
                        'EC_KEYSTONE_32
                        'If playState = eDVDPlayState.SystemJP Then Exit Select
                        'If p1 = 0 Then
                        '    '3:2 has been deactivated
                        '    VSI.lblCurrentlyRunning32.Text = "False"
                        '    'Me.lbl32WhichField.Text = ""
                        'Else
                        '    '3:2 has been activated
                        '    VSI.lblCurrentlyRunning32.Text = "True"
                        '    'If p2 = 1 Then
                        '    '    Me.lbl32WhichField.Text = "Top"
                        '    'Else
                        '    '    Me.lbl32WhichField.Text = "Bottom"
                        '    'End If
                        'End If

                    Case 97 'EC_KEYSTONE_FIELDORDER
                        Debug.WriteLine("EVENT: EC_KEYSTONE_FIELDORDER")
                        'If p1 = 1 Then
                        '    TopFieldFirst = True
                        'Else
                        '    TopFieldFirst = False
                        'End If
                        'RaiseEvent evKEYSTONE_FieldOrder(TopFieldFirst)

                    Case 14 'EC_PAUSED
                        Debug.WriteLine("EVENT: EC_PAUSED")

                    Case 13 'EC_CLOCK_CHANGED
                        Debug.WriteLine("EVENT: EC_CLOCK_CHANGED")

                    Case 80 'EC_GRAPH_CHANGED
                        Debug.WriteLine("EVENT: EC_GRAPH_CHANGED")

                    Case DsEvCode.EC_VIDEO_SIZE_CHANGED
                        Debug.WriteLine("EVENT: Video Size Change")
                        'Dim xP1 As String = Hex(p1)
                        'Dim H, W As Short
                        'Dim sH, sW As Short
                        'sH = Microsoft.VisualBasic.Left(xP1, 3)
                        'sW = Microsoft.VisualBasic.Right(xP1, 3)
                        'H = HEXtoDEC(sH)
                        'W = HEXtoDEC(sW)
                        'RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Video size changed. H: " & H & " W: " & W)

                    Case DsEvCode.EC_USERABORT
                        Debug.WriteLine("EVENT: User Abort")
                        'RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "EVENT: User Abort", Nothing, Nothing)

                    Case 96 'Keystone sample times
                        Debug.WriteLine("EVENT: Keystone Sample Times: " & p1 & " - " & p2)

                    Case 98 'EC_KEYSTONE_MPEGTC
                        Debug.WriteLine("EVENT: EC_KEYSTONE_MPEGTC")

                        'Dim fps As Double
                        'If p2 = 2997 Then
                        '    fps = 29.97
                        'Else
                        '    fps = 25
                        'End If

                        'Dim hex As String = DECtoHEX(p1)
                        'If hex.Length < 8 Then
                        '    hex = PadString(hex, 8, "0", True)
                        'End If
                        'Dim f As String = HEXtoDEC(Microsoft.VisualBasic.Right(hex, 2))
                        'Dim s As String = HEXtoDEC(Microsoft.VisualBasic.Mid(hex, 5, 2))
                        'Dim m As String = HEXtoDEC(Microsoft.VisualBasic.Mid(hex, 3, 2))
                        'Dim h As String = HEXtoDEC(Microsoft.VisualBasic.Left(hex, 2))

                        '_LastGOPTC = New cTimecode(h, m, s, f)
                        '_LastGOPTC_Ticks = DateTime.Now.Ticks

                        'If f.Length = 1 Then
                        '    f = PadString(f, 2, "0", True)
                        'End If
                        'If m.Length = 1 Then
                        '    m = PadString(m, 2, "0", True)
                        'End If
                        'If s.Length = 1 Then
                        '    s = PadString(s, 2, "0", True)
                        'End If

                        ''Debug.WriteLine(h & ":" & m & ":" & s & ";" & f)
                        'If Me.PlayState = eDVDPlayState.FrameStepping Then
                        '    If Not f = 0 Then
                        '        f -= 1 'addresses confusion about difference betwen tc burn-in and that displayed here
                        '    End If
                        'End If

                        'RaiseEvent evMPEG_Timecode(New cTimecode(h, m, s, f))


                    Case 99 'EC_KEYSTONE_INTERLACING			0x63
                        Debug.WriteLine("EVENT: EC_KEYSTONE_INTERLACING: " & p1)
                        'Interlaced = Not CBool(p1)
                        'RaiseEvent evKEYSTONE_Interlacing(Interlaced)

                    Case 100 'EC_KEYSTONE_FORCEFRAMEGRAB		0x64
                        Debug.WriteLine("EVENT: EC_KEYSTONE_FORCEFRAMEGRAB")
                        'HandleKeystoneForcedFrameGrab(New IntPtr(p1), p2)

                    Case 3 'EC_ERRORABORT
                        Debug.WriteLine("EVENT: EC_ERRORABORT")
                        'Try
                        '    Marshal.ThrowExceptionForHR(p1)
                        'Catch ex As Exception
                        '    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "EC_ERRORABORT. Error: " & ex.Message, Nothing, Nothing)
                        'End Try

                    Case &H110 'EC_DVD_PLAYBACK_RATE_CHANGE
                        Debug.WriteLine("EVENT: EC_DVD_PLAYBACK_RATE_CHANGE. Rate: " & p1)
                        'If _CurrentPlaybackRate > p1 And p1 = 10000 Then
                        '    'returned to normal speed after fast forward
                        '    Me.UnMute()
                        'End If
                        '_CurrentPlaybackRate = p1
                        ''1000 = 1x
                        ''-4000 = 4x reverse
                        ''4000 = 4x forwards

                    Case &H65 'EC_KEYSTONE_PROGRESSIVESEQUENCE
                        Debug.WriteLine("EVENT: EC_KEYSTONE_PROGRESSIVESEQUENCE")
                        'If p1 = 1 Then
                        '    ProgressiveSequence = True
                        'Else
                        '    ProgressiveSequence = False
                        'End If


                    Case &H8065 'NVVIDDEC_EVENT_PICTURE_FRAGMENT
                        Debug.WriteLine("EVENT: NVVIDDEC_EVENT_PICTURE_FRAGMENT")
                        ''Debug.WriteLine("CorruptedVideoData")
                        'If playState = eDVDPlayState.FastForwarding Or playState = eDVDPlayState.Rewinding Then Exit Sub
                        'If CurrentMPEGFrameMode > 1 Then Exit Sub
                        'If VSETimer Is Nothing Then
                        '    VSETC = New sTransferErrorTime
                        '    VSETC.SourceTime = LastGOPTC
                        '    Me.Graph.DVDInfo.GetCurrentLocation(VSETC.RunningTime)
                        '    'Debug.WriteLine("Starting VSETimer")
                        '    VSETimer = New System.Timers.Timer(2200) 'adjust this larger until you get no false video error notifications
                        '    AddHandler VSETimer.Elapsed, AddressOf VSETimerTick
                        '    VSETimer.AutoReset = False
                        '    VSETimer.Enabled = True
                        '    VSETicks = DateTime.Now.Ticks
                        'Else
                        '    'Debug.WriteLine("Not starting VSETimer")
                        'End If
                        ''Debug.WriteLine("-------------------------")

                    Case &H66 'EC_KEYSTONE_DISCONTINUITY
                        Debug.WriteLine("EVENT: EC_KEYSTONE_DISCONTINUITY")
                        ''Debug.WriteLine("Discontinuity")
                        'If Me.Muting Then Me.UnMute()

                        ''CurrentSpeed = FullSpeed

                        ''Debug.WriteLine("Ticks elapsed since VSETimer started: " & DateTime.Now.Ticks - VSETicks)
                        'If Not VSETimer Is Nothing Then
                        '    VSETimer.Stop()
                        '    VSETimer.Dispose()
                        '    VSETimer = Nothing
                        '    'Debug.WriteLine("Dis VSET: (should be true) " & (VSETimer Is Nothing))
                        'End If
                        ''Debug.WriteLine("-------------------------")

                    Case &H67 'EC_KEYSTONE_MACROVISION
                        Debug.WriteLine("EVENT: EC_KEYSTONE_MACROVISION. Level: " & p1)
                        'If PlayState <> eDVDPlayState.SystemJP And PlayState <> eDVDPlayState.Init And PlayerMode <> ePlayerMode.Stream Then
                        '    If p1 = 0 Then
                        '        MacrovisionStatus = "Off"
                        '    Else
                        '        MacrovisionStatus = p1
                        '    End If
                        'End If

                    Case &H8066 'NVVIDDEC_EVENT_MACROVISION_LEVEL
                        Debug.WriteLine("EVENT: NVVIDDEC_EVENT_MACROVISION_LEVEL")
                        'we're getting it from Keystone

                    Case Else
                        Debug.WriteLine("UNKNOWN EVENT: " & code)

                End Select
            Catch ex As Exception
                Throw New Exception("Problem with DerivedPlayerHandleEvent(). Error: " & ex.Message)
            End Try
        End Sub

#End Region 'EVENT HANDLING

#Region "Shuttle"

        Private WithEvents ShuttleTimer As Timers.Timer
        Private ShuttleForward As Boolean
        'Private Const ONE_SEC As Integer = 400000
        'Private ShuttleStartPosition As Long
        Private Shared ShuttlePositionForThread As Long
        'Private ShuttleStepSize As Long

        Public Sub StartShuttle(ByVal Speed As eShuttleSpeed, ByVal Forward As Boolean)
            Debug.WriteLine("StartShuttle: " & Speed)

            CurrentSpeed = Speed 'just to be sure (?)
            ShuttleForward = Forward

            If ShuttleTimer Is Nothing Then
                ShuttleTimer = New Timers.Timer
            End If
            ShuttleTimer.Interval = 100
            ShuttleTimer.Start()

            'ShuttleStartPosition = GetPosition()
            'ShuttlePosition = ShuttleStartPosition
            'ShuttleStepSize = (ONE_SEC / 2) * Speed

            If Not Graph.KO_IKeystone Is Nothing Then
                Graph.KO_IKeystone.DeactivateFFRW()
            End If

        End Sub

        Private Sub StopShuttle()
            Debug.WriteLine("StopShuttle")
            ShuttleTimer.Stop()
            If Not Graph.KO_IKeystone Is Nothing Then
                Graph.KO_IKeystone.DeactivateFFRW()
            End If
            CurrentSpeed = eShuttleSpeed.OneX
            PlayState = ePlayState.Playing
        End Sub

        Private Sub ShuttleTimer_Tick(ByVal sender As System.Object, ByVal e As System.Timers.ElapsedEventArgs) Handles ShuttleTimer.Elapsed
            Try
                If PlayState = ePlayState.Stopped Then
                    ShuttleTimer.Stop()
                    Exit Sub
                End If
                HR = Graph.MediaSeek.GetCurrentPosition(ShuttlePositionForThread)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                If ShuttleForward Then
                    ShuttlePositionForThread += CurrentSpeed * 1000000 '10000000
                Else
                    ShuttlePositionForThread -= CurrentSpeed * 1000000
                End If
                'Debug.WriteLine(CP)
                Dim t As New Threading.Thread(AddressOf ShuttleSetPositionThread)
                t.Start()
                'SetPosition(ShuttlePositionForThread)
            Catch ex As Exception
                Throw New Exception("Problem with ShuttleTimer_Tick(). Error: " & ex.Message)
            End Try
        End Sub

        Private Sub ShuttleSetPositionThread()
            Try
                HR = Graph.MediaSeek.SetPositions(ShuttlePositionForThread, SeekingFlags.AbsolutePositioning, 0, SeekingFlags.NoPositioning)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                Throw New Exception("Problem with ShuttleSetPositionThread(). Error: " & ex.Message)
            End Try
        End Sub

#End Region 'Shuttle

    End Class

End Namespace
