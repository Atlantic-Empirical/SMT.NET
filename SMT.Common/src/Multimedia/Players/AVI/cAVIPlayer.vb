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

Namespace Multimedia.Players.AVI

    Public Class cAVIPlayer
        Inherits cBasePlayer

#Region "cBasePlayer Overrides"

        Public Overrides Function BuildGraph(ByVal FilePath As String, ByVal NotifyWindowHandle As Integer, ByVal nAVMode As eAVMode, ByVal nParentForm As Form) As Boolean
            Try
                If Not FilterCheck() = "True" Then Return False
                Graph = New cSMTGraph(NotifyWindowHandle)
                ParentForm = New cSMTForm(nParentForm)

                If Not Graph.AddAVISplitter() Then Throw New Exception("Problem adding AVISplitter")
                If Not Graph.AddDeckLinkAudio() Then Throw New Exception("Problem adding DLA")
                If Not Graph.AddDeckLinkVideo() Then Throw New Exception("Problem adding DLV")

                HR = Graph.GraphBuilder.RenderFile(FilePath, "")
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'Dim Duration As Long = 0
                'While Duration = 0
                '    HR = Seek.GetDuration(Duration) '11844600000
                '    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                'End While
                'Me.tbPlayPosition.Maximum = Duration / 10000000
                'Me.tbPlayPosition.TickFrequency = Me.tbPlayPosition.Maximum / 100
                'Me.tbPlayPosition.LargeChange = 10
                'Me.tbPlayPosition.SmallChange = 1

                'Duration /= 10000000 'Get total seconds

                'Dim tmp As String
                'Dim dh As String
                'If Duration > 3600 Then
                '    tmp = CStr(Duration / 3600)
                '    dh = MakeTwoDig(Microsoft.VisualBasic.Left(tmp, 1))
                '    Duration -= (dh * 3600)
                'Else
                '    dh = "00"
                'End If

                'Dim dm As String
                'If Duration > 60 Then
                '    tmp = CStr(Duration / 60)
                '    Dim t() As String = Split(tmp, ".", -1, CompareMethod.Text)
                '    dm = MakeTwoDig(t(0))
                '    Duration -= (dm * 60)
                'Else
                '    dm = "00"
                'End If

                'Dim ds As String = MakeTwoDig(Duration)
                'lblDuration.Text = dh & ":" & dm & ":" & ds

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with AVI Graph. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Overrides Function Play() As Boolean

        End Function

        Public Overrides Function Pause() As Boolean

        End Function

        Public Overrides Function [Stop]() As Boolean

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
            'StartShuttle(CurrentSpeed, True)
            HR = Graph.MediaSeek.SetRate(4.0)
            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            PlayState = ePlayState.FastForwarding
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
            'StartShuttle(CurrentSpeed, False)
            PlayState = ePlayState.Rewinding
        End Function

        Public Overrides Function Timesearch() As Boolean

        End Function

        Public Overrides Function FilterCheck() As String
            Try
                Dim startstring As String = "The following components are not installed correctly:" & vbNewLine & vbNewLine
                Dim msgstr As String = startstring
                'If Not MainConcept_AVCDecoder Then msgstr &= "MainConcept AVC Decoder" & vbNewLine
                'If Not MainConcept_MPEGDemux Then msgstr &= "MainConcept MPEG Demux" & vbNewLine
                'If Not SMT_KeystoneHD Then msgstr &= "SMT Keystone HD" & vbNewLine
                If msgstr = startstring Then Return True
                Return msgstr
            Catch ex As Exception
                Debug.WriteLine("Problem with FilterCheck(). Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Overrides Function FrameStep() As Boolean
            Try
                HR = Graph.KO_IKeystone.FrameStep(True)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                PlayState = ePlayState.FrameStepping
            Catch ex As Exception
                Throw New Exception("Problem with FrameStep. Error: " & ex.Message)
            End Try
        End Function

        Public Overrides Function QuitFrameStepping() As Boolean
            Try
                HR = Graph.KO_IKeystone.QuitFrameStepping
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                Throw New Exception("Problem with QuitFrameStepping(). Error: " & ex.Message)
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

        Public Overrides Function GetPosition() As Long
            Try
                Dim pCur, pStop As Long
                HR = Graph.MediaSeek.GetPositions(pCur, pStop)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return pCur
            Catch ex As Exception
                Throw New Exception("Problem with GetPosition(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Overrides Function SetPosition(ByVal Pos As Long) As Boolean
            Try
                HR = Graph.MediaSeek.SetPositions(Pos, SeekingFlags.AbsolutePositioning, 0, SeekingFlags.NoPositioning)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with SetPosition. Error: " & ex.Message, ex)
            End Try
        End Function

        Public Overrides Function RestartStream() As Boolean
            SetPosition(0)
        End Function

        Public Overrides Sub StartSampleRecording(ByVal TargetDirectory As String)
            Try
                If NowRecording Then Exit Sub
                If Not Directory.Exists(TargetDirectory) Then Directory.CreateDirectory(TargetDirectory)
                If Graph.KO_IKeystone Is Nothing Then Exit Sub
                Graph.KO_IKeystone.StartRecording(Replace(TargetDirectory, "\", "\\"))
                NowRecording = True
            Catch ex As Exception
                Throw New Exception("Problem with StartSampleRecording() in cAVCPlayer. Error: " & ex.Message)
            End Try
        End Sub

        Public Overrides Sub StopSampleRecording()
            Try
                If Not NowRecording Then Exit Sub
                Graph.KO_IKeystone.StopRecording()
                NowRecording = False
            Catch ex As Exception
                Throw New Exception("Problem with StopRecording() in cAVCPlayer. Error: " & ex.Message)
            End Try
        End Sub

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

        Public Overrides Sub ClearOSD()

        End Sub

        Public Overrides Function GrabSingleSample(ByVal TargetDirectory As String, ByVal Format As System.Drawing.Imaging.ImageFormat) As Boolean
            'Public Sub GrabScreen(ByVal FrameGrabFormat As System.Drawing.Imaging.ImageFormat, ByVal FrameGrabSource As eFrameGrabContent, ByVal BurnInTimecode As Boolean)
            Try
                'If Not xAD.aFrameGrab Then
                '    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Frame grabbing is not licensed.")
                '    Exit Sub
                'End If

                'Get the RGB24 bits from Keystone
                Dim Buffer() As Byte = GetSampleFromKeystone()

                If Buffer Is Nothing Then
                    Exit Function
                End If

                'Make the bitmap object
                Dim MS As New MemoryStream(Buffer.Length)
                MS.Write(Buffer, 0, Buffer.Length)
                Dim BM As New Bitmap(MS)

                'BM.Save("c:\temp\bm.bmp")

                'Scale image as needed
                If BM.Size <> CurrentVideoDimensions Then
                    BM = ScaleImage(BM, Me.CurrentVideoDimensions.Height, Me.CurrentVideoDimensions.Width)
                End If

                'DEBUGGING
                'BM.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) & "\test.bmp")

                ''Save the bitmap
                'Dim FS As New FileStream(tOutpath, FileMode.OpenOrCreate)
                'FS.Write(Buffer, 0, Buffer.Length)
                'FS.Close()
                'FS = Nothing
                'END DEBUGGING

                'Get save location
                Dim OutPath As String = TargetDirectory
                If Not Directory.Exists(OutPath) Then Directory.CreateDirectory(OutPath)

                'Get desired format
                OutPath &= DateTime.Now.Ticks & "." & Format.ToString

                ''burn in source time code if available
                'If BurnInTimecode Then 'user wants source tc burn in
                '    'Debug.WriteLine(DateTime.Now.Ticks - Me.LastGOPTC_Ticks)
                '    If DateTime.Now.Ticks - Me.LastGOPTC_Ticks < 1700000 Then 'we got a tc recently
                '        BM = DrawTextOnBMP(BM, Me.LastGOPTC.ToString_NoFrames, New PointF((BM.Width * 0.9), 15), 10)
                '    End If
                'End If

                'Save the image
                BM.Save(OutPath, Format)
                BM.Dispose()

                'Clean up
                BM = Nothing
                MS.Close()
                MS = Nothing

                Return True
            Catch ex As Exception
                Throw New Exception("problem getting screengrab. error: " & ex.Message, ex)
            End Try
        End Function

        Public Overrides Sub DerivedPlayerHandleEvent(ByVal code As Integer, ByVal p1 As Integer, ByVal p2 As Integer)
            Try
                'Debug.WriteLine("EVT: " + code.ToString())
                If Not code = DsEvCode.EC_DVD_CURRENT_HMSF_TIME Then
                    'Debug.WriteLine("EVT: " + code.ToString())
                End If
                Select Case code
                    Case DsEvCode.EC_ERRORABORT
                        Me.Stop()
                        'Throw New Exception("Playback aborted. Possibly due to end of stream.", Throw New ExceptionStyle.OkOnly Or Throw New ExceptionStyle.Exclamation)

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

                    Case 88 'EC_QUALITY_CHANGE_KEYSTONE
                        'Me.AddConsoleLine("Keystone Quality Change:" & vbNewLine & vbTab & "Proportion: " & p1 & vbNewLine & vbTab & "Late: " & p2)
                        Debug.WriteLine("Keystone Quality Change:" & vbNewLine & vbTab & "Proportion: " & p1 & vbNewLine & vbTab & "Late: " & p2)

                    Case 89
                        'Keystone 3:2 change
                        'EC_KEYSTONE_32
                        'If playState = ePlayState.SystemJP Then Exit Select
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

                    Case 97
                        ''EC_KEYSTONE_FIELDORDER
                        'If PlayState = ePlayState.SystemJP Then Exit Select
                        'Dim s As String
                        'If p1 = 1 Then
                        '    s = "Top"
                        'Else
                        '    s = "Bottom"
                        'End If

                        'If s <> VSI.lbl32WhichField.Text Then
                        '    If Me.CurrentUserProfile.AppOptions.BeepOnVideoPropChange Then
                        '        Beep(500, 125)
                        '    End If
                        '    Me.AddConsoleLine(eConsoleItemType.NOTICE, "Video source field order changed", s)
                        '    VSI.lbl32WhichField.Text = s
                        'End If

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

                    Case 1074397284
                        ''EC_L21G  sample available
                        'If p1 > 0 Then
                        '    GetL21BufferFromL21G()
                        'End If

                    Case 1074397285
                        ''EC_AMTCBuffer sample available
                        'If p1 > 0 Then
                        '    GetBufferFromAMTC()
                        'End If

                    Case 96 'Keystone sample times
                        Debug.WriteLine("Key: " & p1 & " - " & p2)

                    Case 97 'AMTC sample times
                        Debug.WriteLine("AMTC: " & p1 & " - " & p2)

                    Case 98
                        ''EC_KEYSTONE_MPEGTC

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

                        'LastGOPTC = New cTimecode(h, m, s, f)

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
                        'If Me.PlayState = ePlayState.FrameStepping Then
                        '    If Not f = 0 Then
                        '        f -= 1 'addresses confusion about difference betwen tc burn-in and that displayed here
                        '    End If
                        'End If
                        'DB.lblSourceTimecode.Text = h & ":" & m & ":" & s & ";" & f

                    Case 99 'EC_KEYSTONE_INTERLACING			0x63
                        'If Not PlayState = ePlayState.SystemJP Then
                        '    Dim newval As String = CStr(ReverseBool(CBool(p1)))
                        '    If newval <> VSI.lblCurrentVideoIsInterlaced.Text Then
                        '        If Me.CurrentUserProfile.AppOptions.BeepOnVideoPropChange Then
                        '            Beep(500, 125)
                        '        End If
                        '        Me.AddConsoleLine(eConsoleItemType.NOTICE, "Video source interlacing changed", newval)
                        '        VSI.lblCurrentVideoIsInterlaced.Text = newval
                        '    End If

                        'Else
                        '    VSI.lblCurrentVideoIsInterlaced.Text = ""
                        'End If

                    Case 100 'EC_KEYSTONE_FORCEFRAMEGRAB		0x64
                        KeystoneForcedFrameGrab(New IntPtr(p1), p2)

                    Case 3
                        'EC_ERRORABORT
                        Try
                            Marshal.ThrowExceptionForHR(p1)
                        Catch ex As Exception
                            Debug.WriteLine("EC_ERRORABORT. Error: " & ex.Message)
                        End Try

                    Case &H110
                        ''EC_DVD_PLAYBACK_RATE_CHANGE
                        'If curPBRate > p1 And p1 = 10000 Then
                        '    'returned to normal speed after fast forward
                        '    Me.UnMute()
                        'End If
                        'curPBRate = p1
                        'Debug.WriteLine("EC_DVD_PLAYBACK_RATE_CHANGE. Rate: " & p1)
                        ''1000 = 1x
                        ''-4000 = 4x reverse
                        ''4000 = 4x forwards

                    Case &H65 'EC_KEYSTONE_PROGRESSIVESEQUENCE
                        'If Not PlayState = ePlayState.SystemJP Then
                        '    VSI.lblProgressive_Sequence.Text = CStr(CBool(p1))
                        'Else
                        '    VSI.lblProgressive_Sequence.Text = ""
                        'End If

                    Case &H8065 'NVVIDDEC_EVENT_PICTURE_FRAGMENT
                        ''Debug.WriteLine("CorruptedVideoData")
                        'If PlayState = ePlayState.FastForwarding Or PlayState = ePlayState.Rewinding Then Exit Sub
                        'If VSETimer Is Nothing Then
                        '    VSETC = LastGOPTC
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
                        ''Debug.WriteLine("Discontinuity")
                        'If Me.Muting Then Me.UnMute()
                        'CurrentSpeed = FullSpeed
                        ''Debug.WriteLine("Ticks elapsed since VSETimer started: " & DateTime.Now.Ticks - VSETicks)
                        'If Not VSETimer Is Nothing Then
                        '    VSETimer.Stop()
                        '    VSETimer.Dispose()
                        '    VSETimer = Nothing
                        '    'Debug.WriteLine("Dis VSET: (should be true) " & (VSETimer Is Nothing))
                        'End If
                        ''Debug.WriteLine("-------------------------")

                    Case &H67 'EC_KEYSTONE_MACROVISION
                        ''Debug.WriteLine("Macrovision Level: " & p1)
                        'If PlayState <> ePlayState.SystemJP And PlayState <> ePlayState.Init And PhoenixMode <> ePhoenixMode.Stream Then
                        '    If p1 = 0 Then
                        '        VSI.lblMacrovision.Text = "Off"
                        '    Else
                        '        VSI.lblMacrovision.Text = p1
                        '    End If
                        'End If

                    Case &H8066 'NVVIDDEC_EVENT_MACROVISION_LEVEL
                        'we're getting it from Keystone

                    Case 284 'EC_DVD_PROGRAM_CELL_CHANGE
                        'If PlayState <> ePlayState.SystemJP And PlayState <> ePlayState.Init Then
                        '    Me.DB.lblCell.Text = p2
                        '    Me.DB.lblProgramCur.Text = p1
                        'Else
                        '    Me.DB.lblCell.Text = ""
                        '    Me.DB.lblProgramCur.Text = ""
                        'End If

                    Case Else
                        'Log unknown events
                        Debug.WriteLine("Unknown event: " & code & " p1: " & p1 & " p2: " & p2)
                End Select
            Catch ex As Exception
                Throw New Exception("Problem with DerivedPlayerHandleEvent(). Error: " & ex.Message)
            End Try
        End Sub

#End Region 'Overrides

        Private Function GetSampleFromKeystone() As Byte()
            Try
                Dim SamplePtr As IntPtr
                Dim SampSize, SampW, SampH As Integer

                HR = Graph.KO_IKeystone.GrabSample(eSampleFrom.Output, SamplePtr, SampSize, SampW, SampH)
                If Math.Abs(HR) = 2147483655 Or Math.Abs(HR) = 2147467260 Then
                    Throw New Exception("FRAMEGRAB TIMEOUT: No samples received in three seconds.")
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
                Throw New Exception("Problem with GetSampleFromKeystone. Error: " & ex.Message, ex)
            End Try
        End Function

    End Class

End Namespace
