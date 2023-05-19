Imports SMT.Multimedia.DirectShow
Imports SMT.Multimedia.Filters.SMT.Keystone
Imports SMT.DotNet.Utility
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Windows.Forms
Imports SMT.Multimedia.Formats.StillImage
Imports SMT.Multimedia.GraphConstruction
Imports SMT.Multimedia.Classes
Imports SMT.Multimedia.Enums

Namespace Multimedia.Players.FrameSplitter

    Public Class cFrameSplittingPlayer
        Inherits cBaseFrameSplitter

        Public Sub New()
            ' SET DEFAULT FS TYPE
            Me.CurrentFrameSplittingMode = eFrameSplitMode.FiftyFifty_A
        End Sub

#Region "OVERRIDES"

        Public Overrides Function BuildFrameSplittingGraph(ByVal FileA As String, ByVal FileB As String, ByVal NotifyWindowHandle As Integer) As Boolean
            Try
                Dim extA As String = Path.GetExtension(FileA)
                Dim extB As String = Path.GetExtension(FileB)

                Select Case extA.ToLower
                    Case ".m2v_hd"
                        Select Case extB.ToLower
                            Case ".m2v"
                                Return Me.BG_FS_M2VHD_M2V(FileA, FileB, NotifyWindowHandle)
                            Case ".m2v_hd"
                                Return Me.BG_FS_M2VHD_M2VHD(FileA, FileB, NotifyWindowHandle)
                            Case ".avc", ".264", ".h264"
                                Return Me.BG_FS_M2VHD_AVC(FileA, FileB, NotifyWindowHandle)
                            Case ".vc1"
                                Return Me.BG_FS_M2VHD_VC1(FileA, FileB, NotifyWindowHandle)
                            Case ".iyuv", ".i420"
                                Return Me.BG_FS_M2VHD_YUV(FileA, FileB, NotifyWindowHandle)
                        End Select

                    Case ".avc", ".264", ".h264"
                        Select Case extB.ToLower
                            Case ".m2v"
                                Return Me.BG_FS_AVC_M2V(FileA, FileB, NotifyWindowHandle)
                            Case ".m2v_hd"
                                Return Me.BG_FS_AVC_M2VHD(FileA, FileB, NotifyWindowHandle)
                            Case ".avc", ".264", ".h264"
                                Return Me.BG_FS_AVC_AVC(FileA, FileB, NotifyWindowHandle)
                            Case ".vc1"
                                Return Me.BG_FS_AVC_VC1(FileA, FileB, NotifyWindowHandle)
                            Case ".iyuv", ".i420"
                                Return Me.BG_FS_AVC_YUV(FileA, FileB, NotifyWindowHandle)
                        End Select

                    Case ".vc1"
                        Select Case extB.ToLower
                            Case ".m2v"
                                Return Me.BG_FS_VC1_M2V(FileA, FileB, NotifyWindowHandle)
                            Case ".m2v_hd"
                                Return Me.BG_FS_VC1_M2VHD(FileA, FileB, NotifyWindowHandle)
                            Case ".avc", ".264", ".h264"
                                Return Me.BG_FS_VC1_AVC(FileA, FileB, NotifyWindowHandle)
                            Case ".vc1"
                                Return Me.BG_FS_VC1_VC1(FileA, FileB, NotifyWindowHandle)
                            Case ".iyuv", ".i420"
                                Return Me.BG_FS_VC1_YUV(FileA, FileB, NotifyWindowHandle)
                        End Select

                    Case Else
                        'we can't split these formats
                        Return False
                End Select

                Return False

            Catch ex As Exception
                Throw New Exception("Problem with BuildFrameSplittingGraph(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Overrides Function BuildGraph(ByVal FilePath As String, ByVal NotifyWindowHandle As Integer, ByVal nAVMode As eAVMode, ByVal pParentForm As Form) As Boolean
            'DO NOT USE FOR FRAME SPLITTING
            Return False
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
                        Graph.MediaSeek.SetRate(1.0)

                    Case ePlayState.Paused
                        Graph.MediaCtrl.Run()
                        PlayState = ePlayState.Playing

                    Case Else
                        HR = Graph.MediaCtrl.Run()
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        PlayState = ePlayState.Playing

                End Select
                If Not Graph.DeckControl Is Nothing Then Graph.DeckControl.Play()
            Catch ex As Exception
                Throw New Exception("Problem with StartPlayback. Error: " & ex.Message)
            End Try
        End Function

        Public Overrides Function Pause() As Boolean
            Try
                If Not Graph.DeckControl Is Nothing Then Graph.DeckControl.Stop()
                HR = Graph.MediaCtrl.Pause()
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                PlayState = ePlayState.Paused
            Catch ex As Exception
                Throw New Exception("Problem with fileplayback-pause. Error: " & ex.Message)
            End Try
        End Function

        Public Overrides Function [Stop]() As Boolean
            If Graph.MediaCtrl Is Nothing Then Exit Function
            HR = Graph.MediaCtrl.Stop()
            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            If Not Graph.DeckControl Is Nothing Then Graph.DeckControl.Stop()
            Graph.DestroyGraph()
            PlayState = ePlayState.Stopped
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
            StartShuttle(CurrentSpeed, True)
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
            StartShuttle(CurrentSpeed, False)
            PlayState = ePlayState.Rewinding
        End Function

        Public Overrides Sub ClearOSD()

        End Sub

        Public Overrides Function FrameStep() As Boolean
            Try
                HR = Graph.IKeyFS.FrameStep(True)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                PlayState = ePlayState.FrameStepping
            Catch ex As Exception
                Throw New Exception("Problem with FrameStep(). Error: " & ex.Message)
            End Try
        End Function

        Public Overrides Function GetPosition() As Long
            Try
                Dim pCur, pStop As Long
                HR = Graph.MediaSeek.GetPositions(pCur, pStop)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return pCur
            Catch ex As Exception
                Throw New Exception("Problem with GetPosition. Error: " & ex.Message, ex)
            End Try
        End Function

        Public Overrides Function GetBitmapFromKeystone() As Byte()
            Try
                Dim SamplePtr As IntPtr
                Dim SampSize, SampW, SampH As Integer

                HR = Graph.IKeyFS.GrabSample(eSampleFrom.Output, SamplePtr, SampSize, SampW, SampH)
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

        End Function

        Public Overrides Function QuitFrameStepping() As Boolean
            Try
                HR = Graph.IKeyFS.QuitFrameStepping
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                Throw New Exception("Problem with QuitFrameStepping(). Error: " & ex.Message)
            End Try
        End Function

        Public Overrides Function RestartStream() As Boolean

        End Function

        Public Overrides Function SetFrameSplit(ByVal FSM As eFrameSplitMode) As Boolean
            Try
                Me.CurrentFrameSplittingMode = FSM
                Select Case FSM
                    Case eFrameSplitMode.Checker_A
                        Graph.IKeyMixerFS.SetFrameSplit(1, 0, 0, 0, 0, 0)

                    Case eFrameSplitMode.Checker_B
                        Graph.IKeyMixerFS.SetFrameSplit(0, 1, 0, 0, 0, 0)

                    Case eFrameSplitMode.FiftyFifty_A
                        Graph.IKeyMixerFS.SetFrameSplit(0, 0, 50, 50, 0, 0)

                    Case eFrameSplitMode.FiftyFifty_B
                        Graph.IKeyMixerFS.SetFrameSplit(1, 1, 50, 50, 0, 0)

                    Case eFrameSplitMode.LeftSides
                        Graph.IKeyMixerFS.SetFrameSplit(0, 2, 50, 50, 0, 0)

                    Case eFrameSplitMode.RightSides
                        Graph.IKeyMixerFS.SetFrameSplit(2, 0, 50, 50, 0, 0)

                    Case eFrameSplitMode.AllA
                        Graph.IKeyMixerFS.SetFrameSplit(3, 0, 0, 0, 0, 0)

                    Case eFrameSplitMode.AllB
                        Graph.IKeyMixerFS.SetFrameSplit(3, 1, 0, 0, 0, 0)

                End Select
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with SetFrameSplit(). Error: " & ex.Message)
            End Try
        End Function

        Public Overrides Function SetDynamicSplitVals(ByVal A As Integer, ByVal B As Integer, ByVal C As Integer, ByVal D As Integer) As Boolean
            Try
                Select Case Me.CurrentFrameSplittingMode
                    Case eFrameSplitMode.Checker_A
                        Graph.IKeyMixerFS.SetFrameSplit(1, 0, A, B, C, D)

                    Case eFrameSplitMode.Checker_B
                        Graph.IKeyMixerFS.SetFrameSplit(0, 1, A, B, C, D)

                    Case eFrameSplitMode.FiftyFifty_A
                        Graph.IKeyMixerFS.SetFrameSplit(0, 0, A, B, C, D)

                    Case eFrameSplitMode.FiftyFifty_B
                        Graph.IKeyMixerFS.SetFrameSplit(1, 1, A, B, C, D)

                    Case eFrameSplitMode.LeftSides
                        Graph.IKeyMixerFS.SetFrameSplit(0, 2, A, B, C, D)

                    Case eFrameSplitMode.RightSides
                        Graph.IKeyMixerFS.SetFrameSplit(2, 0, A, B, C, D)

                    Case eFrameSplitMode.AllA
                        Graph.IKeyMixerFS.SetFrameSplit(3, 0, A, B, C, D)

                    Case eFrameSplitMode.AllB
                        Graph.IKeyMixerFS.SetFrameSplit(3, 1, A, B, C, D)
                End Select
                Return True
            Catch ex As Exception
                Throw New Exception("problem with SetDynamicSplitVals(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Overrides Sub SetOSD(ByVal BM As System.Drawing.Bitmap, ByVal ColorKey As Integer, ByVal DelayMiliSecs As Short, ByVal UseColorKey As Boolean, ByVal Alpha As Single, ByVal Sender As String, ByVal X As Short, ByVal Y As Short)

        End Sub

        Public Overrides Function SetPosition(ByVal Pos As Long) As Boolean
            Try
                HR = Graph.MediaSeek.SetPositions(Pos, SeekingFlags.AbsolutePositioning, 0, SeekingFlags.NoPositioning)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with SetPosition. Error: " & ex.Message, ex)
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

        Public Overrides Function Timesearch() As Boolean
            Throw New Exception("Not implemented.")
        End Function

        Public Overrides Function GrabSingleSample(ByVal TargetDirectory As String, ByVal Format As System.Drawing.Imaging.ImageFormat) As Boolean
            Throw New Exception("Not implemented.")
        End Function

        Public Overrides Function FilterCheck() As String
            Throw New Exception("Not implemented.")
        End Function

#End Region 'OVERRIDES

#Region "EVENT HANDLER"

        Public Overrides Sub DerivedPlayerHandleEvent(ByVal code As Integer, ByVal p1 As Integer, ByVal p2 As Integer)
            Try
                Select Case code

                End Select
            Catch ex As Exception
                Throw New Exception("Problem with DerivedPlayerHandleEvent(). Error: " & ex.Message)
            End Try
        End Sub

#End Region 'EVENT HANDLER

#Region "Graphs"

#Region "M2VHD"

        Public Function BG_FS_M2VHD_M2V(ByVal FilePath_A As String, ByVal FilePath_B As String, ByVal NotifyWindowHandle As Integer) As Boolean
            Try
                Graph = New cSMTGraph(NotifyWindowHandle)

                'Keystone Frame Splitter
                If Not Graph.AddKeystoneOMNI_FrameSplitter() Then Return False

                '==================================================================================
                'SIDE A
                '==================================================================================

                'File source A 
                If Not Graph.AddFilesourceA(FilePath_A) Then Return False

                'Demux A
                If Not Graph.AddMCE_DMXA() Then Return False

                'Decoder A
                If Not Graph.AddMCE_MP2A() Then Return False

                'Connections A
                HR = Graph.GraphBuilder.Connect(Graph.FSFA_Out, Graph.MCE_DMXA_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_DMXA.FindPin("VES", Graph.MCE_DMXA_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXA_Out, Graph.MCE_MP2A_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_MP2A_Out, Graph.KeyHD_FS_EncodeIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SIDE B
                '==================================================================================

                'File source B 
                If Not Graph.AddFilesourceB(FilePath_B) Then Return False

                'Demux B
                If Not Graph.AddMCE_DMXB() Then Return False

                'Decoder B
                If Not Graph.AddMCE_MP2B() Then Return False

                'Image sizer
                If Not Graph.AddMCE_ImgSiz() Then Return False

                'CSC
                If Not Graph.AddMCE_CSC() Then Return False

                'Connections B
                HR = Graph.GraphBuilder.Connect(Graph.FSFB_Out, Graph.MCE_DMXB_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_DMXB.FindPin("VES", Graph.MCE_DMXB_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXB_Out, Graph.MCE_MP2B_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_MP2B_Out, Graph.MCE_ImgSiz_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_ImgSiz.FindPin("Out", Graph.MCE_ImgSiz_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_ImgSiz_Out, Graph.MCE_CSC_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim b As Boolean = Graph.MCE_CSC_IMC.SetParamValue(New Guid("2B58E1BA-19DA-4baa-9A9F-F9677F18C6D0"), 1498831189) 'set output media type to uyvy

                HR = Graph.MCE_CSC.FindPin("Out", Graph.MCE_CSC_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_CSC_Out, Graph.KeyHD_FS_SourceIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)


                '==================================================================================
                'SPLITTER ->
                '==================================================================================

                'DLV Renderer
                If Not Graph.AddDeckLinkVideo() Then Return False

                HR = Graph.GraphBuilder.Connect(Graph.KeyHD_FS_Out, Graph.DLV_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                While _Duration = 0
                    HR = Graph.MediaSeek.GetDuration(_Duration)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End While

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with BG_FS_M2VHD_M2V. Error: " & ex.Message)
            End Try
        End Function

        Public Function BG_FS_M2VHD_M2VHD(ByVal FilePath_A As String, ByVal FilePath_B As String, ByVal NotifyWindowHandle As Integer) As Boolean
            Try
                Graph = New cSMTGraph(NotifyWindowHandle)

                'Keystone Frame Splitter
                If Not Graph.AddKeystoneOMNI_FrameSplitter() Then Return False

                '==================================================================================
                'SIDE A
                '==================================================================================

                'File source A 
                If Not Graph.AddFilesourceA(FilePath_A) Then Return False

                'Demux A
                If Not Graph.AddMCE_DMXA() Then Return False

                'Decoder A
                If Not Graph.AddMCE_MP2A() Then Return False

                'Connections A
                HR = Graph.GraphBuilder.Connect(Graph.FSFA_Out, Graph.MCE_DMXA_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_DMXA.FindPin("VES", Graph.MCE_DMXA_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXA_Out, Graph.MCE_MP2A_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_MP2A_Out, Graph.KeyHD_FS_EncodeIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SIDE B
                '==================================================================================

                'File source B 
                If Not Graph.AddFilesourceB(FilePath_B) Then Return False

                'Demux B
                If Not Graph.AddMCE_DMXB() Then Return False

                'Decoder B
                If Not Graph.AddMCE_MP2B() Then Return False

                'Connections B
                HR = Graph.GraphBuilder.Connect(Graph.FSFB_Out, Graph.MCE_DMXB_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_DMXB.FindPin("VES", Graph.MCE_DMXB_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXB_Out, Graph.MCE_MP2B_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_MP2B_Out, Graph.KeyHD_FS_SourceIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SPLITTER ->
                '==================================================================================

                'DLV Renderer
                If Not Graph.AddDeckLinkVideo() Then Return False

                HR = Graph.GraphBuilder.Connect(Graph.KeyHD_FS_Out, Graph.DLV_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                While _Duration = 0
                    HR = Graph.MediaSeek.GetDuration(_Duration)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End While

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with BG_FS_M2VHD_M2VHD. Error: " & ex.Message)
            End Try
        End Function

        Public Function BG_FS_M2VHD_AVC(ByVal FilePath_A As String, ByVal FilePath_B As String, ByVal NotifyWindowHandle As Integer) As Boolean
            Try
                Graph = New cSMTGraph(NotifyWindowHandle)

                'Keystone Frame Splitter
                If Not Graph.AddKeystoneOMNI_FrameSplitter() Then Return False

                '==================================================================================
                'SIDE A
                '==================================================================================

                'File source A 
                If Not Graph.AddFilesourceA(FilePath_A) Then Return False

                'Demux A
                If Not Graph.AddMCE_DMXA() Then Return False

                'Decoder A
                If Not Graph.AddMCE_MP2A() Then Return False

                'Connections A
                HR = Graph.GraphBuilder.Connect(Graph.FSFA_Out, Graph.MCE_DMXA_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_DMXA.FindPin("VES", Graph.MCE_DMXA_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXA_Out, Graph.MCE_MP2A_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_MP2A_Out, Graph.KeyHD_FS_EncodeIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SIDE B
                '==================================================================================

                'File source B 
                If Not Graph.AddFilesourceB(FilePath_B) Then Return False

                'Demux B
                If Not Graph.AddMCE_DMXB() Then Return False

                'Decoder B
                If Not Graph.AddMCE_AVC() Then Return False

                'Connections B
                HR = Graph.GraphBuilder.Connect(Graph.FSFB_Out, Graph.MCE_DMXB_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_DMXB.FindPin("AVC", Graph.MCE_DMXB_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXB_Out, Graph.MCE_AVC_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_AVC.FindPin("Out", Graph.MCE_AVC_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_AVC_Out, Graph.KeyHD_FS_SourceIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SPLITTER ->
                '==================================================================================

                'DLV Renderer
                If Not Graph.AddDeckLinkVideo() Then Return False

                HR = Graph.GraphBuilder.Connect(Graph.KeyHD_FS_Out, Graph.DLV_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                While _Duration = 0
                    HR = Graph.MediaSeek.GetDuration(_Duration)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End While

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with BG_FS_M2VHD_AVC. Error: " & ex.Message)
            End Try
        End Function

        Public Function BG_FS_M2VHD_VC1(ByVal FilePath_A As String, ByVal FilePath_B As String, ByVal NotifyWindowHandle As Integer) As Boolean
            Try
                Graph = New cSMTGraph(NotifyWindowHandle)

                'Keystone Frame Splitter
                If Not Graph.AddKeystoneOMNI_FrameSplitter() Then Return False

                '==================================================================================
                'SIDE A
                '==================================================================================

                'File source A 
                If Not Graph.AddFilesourceA(FilePath_A) Then Return False

                'Demux A
                If Not Graph.AddMCE_DMXA() Then Return False

                'Decoder A
                If Not Graph.AddMCE_MP2A() Then Return False

                'Connections A
                HR = Graph.GraphBuilder.Connect(Graph.FSFA_Out, Graph.MCE_DMXA_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_DMXA.FindPin("VES", Graph.MCE_DMXA_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXA_Out, Graph.MCE_MP2A_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_MP2A_Out, Graph.KeyHD_FS_EncodeIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SIDE B
                '==================================================================================

                'File source B 
                If Not Graph.AddFilesourceB(FilePath_B) Then Return False

                'MS VC-1 ES Parsre B
                If Not Graph.AddMS_VC1ESParserB() Then Exit Function

                'MS VC-1 DMO B
                If Not Graph.AddVC1DMOB() Then Return False

                'Connections B
                HR = Graph.GraphBuilder.Connect(Graph.FSFB_Out, Graph.MS_VC1ESParserB_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MS_VC1ESParserB.FindPin("Output", Graph.MS_VC1ESParserB_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MS_VC1ESParserB_Out, Graph.MS_VC1B_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MS_VC1B_Out, Graph.KeyHD_FS_SourceIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SPLITTER ->
                '==================================================================================

                'DLV Renderer
                If Not Graph.AddDeckLinkVideo() Then Return False

                HR = Graph.GraphBuilder.Connect(Graph.KeyHD_FS_Out, Graph.DLV_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                While _Duration = 0
                    HR = Graph.MediaSeek.GetDuration(_Duration)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End While

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with BG_FS_M2VHD_VC1. Error: " & ex.Message)
            End Try
        End Function

        Public Function BG_FS_M2VHD_YUV(ByVal FilePath_A As String, ByVal FilePath_B As String, ByVal NotifyWindowHandle As Integer) As Boolean
            Try
                Graph = New cSMTGraph(NotifyWindowHandle)

                'Keystone Frame Splitter
                If Not Graph.AddKeystoneOMNI_FrameSplitter() Then Return False

                '==================================================================================
                'SIDE A
                '==================================================================================

                'File source A 
                If Not Graph.AddFilesourceA(FilePath_A) Then Return False

                'Demux A
                If Not Graph.AddMCE_DMXA() Then Return False

                'Decoder A
                If Not Graph.AddMCE_MP2A() Then Return False

                'Connections A
                HR = Graph.GraphBuilder.Connect(Graph.FSFA_Out, Graph.MCE_DMXA_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_DMXA.FindPin("VES", Graph.MCE_DMXA_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXA_Out, Graph.MCE_MP2A_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_MP2A_Out, Graph.KeyHD_FS_EncodeIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SIDE B
                '==================================================================================

                'YUV Source
                If Not Graph.AddYUVSource() Then Return False
                HR = Graph.iYUVSource.Load(Replace(FilePath_B, "\", "\\"), 0)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim StreamAProperties As cSourceProperties = GetSourceProperties(True)
                HR = Graph.iYUVSource.SetATPF(Math.Round(10000000 / StreamAProperties.FrameRate, 0))
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'Connections B
                HR = Graph.GraphBuilder.Connect(Graph.YUV_Out, Graph.KeyHD_FS_SourceIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)


                '==================================================================================
                'SPLITTER ->
                '==================================================================================

                'DLV Renderer
                If Not Graph.AddDeckLinkVideo() Then Return False

                HR = Graph.GraphBuilder.Connect(Graph.KeyHD_FS_Out, Graph.DLV_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                While _Duration = 0
                    HR = Graph.MediaSeek.GetDuration(_Duration)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End While

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with BG_FS_M2VHD_YUV. Error: " & ex.Message)
            End Try
        End Function

#End Region 'M2VHD

#Region "AVC"

        Public Function BG_FS_AVC_M2V(ByVal FilePath_A As String, ByVal FilePath_B As String, ByVal NotifyWindowHandle As Integer) As Boolean

        End Function

        Public Function BG_FS_AVC_AVC(ByVal FilePath_A As String, ByVal FilePath_B As String, ByVal NotifyWindowHandle As Integer) As Boolean
            'not currently possible
            Return False
        End Function

        Public Function BG_FS_AVC_M2VHD(ByVal FilePath_A As String, ByVal FilePath_B As String, ByVal NotifyWindowHandle As Integer) As Boolean
            Try
                Graph = New cSMTGraph(NotifyWindowHandle)

                'Keystone Frame Splitter
                If Not Graph.AddKeystoneOMNI_FrameSplitter() Then Return False

                '==================================================================================
                'SIDE A
                '==================================================================================

                'File source A 
                If Not Graph.AddFilesourceA(FilePath_A) Then Return False

                'Demux A
                If Not Graph.AddMCE_DMXA() Then Return False

                'Decoder A
                If Not Graph.AddMCE_AVC() Then Return False

                'Connections A
                HR = Graph.GraphBuilder.Connect(Graph.FSFA_Out, Graph.MCE_DMXA_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_DMXA.FindPin("AVC", Graph.MCE_DMXA_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXA_Out, Graph.MCE_AVC_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_AVC.FindPin("Out", Graph.MCE_AVC_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_AVC_Out, Graph.KeyHD_FS_EncodeIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SIDE B
                '==================================================================================

                'File source B 
                If Not Graph.AddFilesourceB(FilePath_B) Then Return False

                'Demux B
                If Not Graph.AddMCE_DMXB() Then Return False

                'Decoder B
                If Not Graph.AddMCE_MP2B() Then Return False

                'Connections B
                HR = Graph.GraphBuilder.Connect(Graph.FSFB_Out, Graph.MCE_DMXB_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_DMXB.FindPin("VES", Graph.MCE_DMXB_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXB_Out, Graph.MCE_MP2B_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_MP2B_Out, Graph.KeyHD_FS_SourceIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SPLITTER ->
                '==================================================================================

                'DLV Renderer
                If Not Graph.AddDeckLinkVideo() Then Return False

                HR = Graph.GraphBuilder.Connect(Graph.KeyHD_FS_Out, Graph.DLV_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                While _Duration = 0
                    HR = Graph.MediaSeek.GetDuration(_Duration)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End While

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with BG_FS_AVC_M2VHD. Error: " & ex.Message)
            End Try
        End Function

        Public Function BG_FS_AVC_VC1(ByVal FilePath_A As String, ByVal FilePath_B As String, ByVal NotifyWindowHandle As Integer) As Boolean
            Try
                Graph = New cSMTGraph(NotifyWindowHandle)

                'Keystone Frame Splitter
                If Not Graph.AddKeystoneOMNI_FrameSplitter() Then Return False


                '==================================================================================
                'SIDE A
                '==================================================================================

                'File source A 
                If Not Graph.AddFilesourceA(FilePath_A) Then Return False

                'Demux A
                If Not Graph.AddMCE_DMXA() Then Return False

                'Decoder A
                If Not Graph.AddMCE_AVC() Then Return False

                'Connections A
                HR = Graph.GraphBuilder.Connect(Graph.FSFA_Out, Graph.MCE_DMXA_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_DMXA.FindPin("AVC", Graph.MCE_DMXA_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXA_Out, Graph.MCE_AVC_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_AVC.FindPin("Out", Graph.MCE_AVC_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_AVC_Out, Graph.KeyHD_FS_EncodeIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SIDE B
                '==================================================================================

                'File source B 
                If Not Graph.AddFilesourceB(FilePath_B) Then Return False

                'MS VC-1 ES Parsre B
                If Not Graph.AddMS_VC1ESParserB() Then Exit Function

                'MS VC-1 DMO B
                If Not Graph.AddVC1DMOB() Then Return False

                'Connections B
                HR = Graph.GraphBuilder.Connect(Graph.FSFB_Out, Graph.MS_VC1ESParserB_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MS_VC1ESParserB.FindPin("Output", Graph.MS_VC1ESParserB_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MS_VC1ESParserB_Out, Graph.MS_VC1B_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MS_VC1B_Out, Graph.KeyHD_FS_SourceIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SPLITTER ->
                '==================================================================================

                'DLV Renderer
                If Not Graph.AddDeckLinkVideo() Then Return False

                HR = Graph.GraphBuilder.Connect(Graph.KeyHD_FS_Out, Graph.DLV_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)


                While _Duration = 0
                    HR = Graph.MediaSeek.GetDuration(_Duration)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End While

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with BG_FS_AVC_VC1. Error: " & ex.Message)
            End Try
        End Function

        Public Function BG_FS_AVC_YUV(ByVal FilePath_A As String, ByVal FilePath_B As String, ByVal NotifyWindowHandle As Integer) As Boolean
            Try
                Graph = New cSMTGraph(NotifyWindowHandle)

                'Keystone Frame Splitter
                If Not Graph.AddKeystoneOMNI_FrameSplitter() Then Return False

                '==================================================================================
                'SIDE A
                '==================================================================================

                'File source A 
                If Not Graph.AddFilesourceA(FilePath_A) Then Return False

                'Demux A
                If Not Graph.AddMCE_DMXA() Then Return False

                'Decoder A
                If Not Graph.AddMCE_AVC() Then Return False

                'Connections A
                HR = Graph.GraphBuilder.Connect(Graph.FSFA_Out, Graph.MCE_DMXA_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_DMXA.FindPin("AVC", Graph.MCE_DMXA_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXA_Out, Graph.MCE_AVC_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_AVC.FindPin("Out", Graph.MCE_AVC_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_AVC_Out, Graph.KeyHD_FS_EncodeIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SIDE B
                '==================================================================================

                'YUV Source
                If Not Graph.AddYUVSource() Then Return False
                HR = Graph.iYUVSource.Load(Replace(FilePath_B, "\", "\\"), 0)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim StreamAProperties As cSourceProperties = GetSourceProperties(True)
                HR = Graph.iYUVSource.SetATPF(Math.Round(10000000 / StreamAProperties.FrameRate, 0))
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'Connections B
                HR = Graph.GraphBuilder.Connect(Graph.YUV_Out, Graph.KeyHD_FS_SourceIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SPLITTER ->
                '==================================================================================

                'DLV Renderer
                If Not Graph.AddDeckLinkVideo() Then Return False

                HR = Graph.GraphBuilder.Connect(Graph.KeyHD_FS_Out, Graph.DLV_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                While _Duration = 0
                    HR = Graph.MediaSeek.GetDuration(_Duration)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End While

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with BG_FS_AVC_YUV. Error: " & ex.Message)
            End Try
        End Function

#End Region 'AVC

#Region "VC1"

        Public Function BG_FS_VC1_M2V(ByVal FilePath_A As String, ByVal FilePath_B As String, ByVal NotifyWindowHandle As Integer) As Boolean

        End Function

        Public Function BG_FS_VC1_VC1(ByVal FilePath_A As String, ByVal FilePath_B As String, ByVal NotifyWindowHandle As Integer) As Boolean
            Return False
            Try
                Graph = New cSMTGraph(NotifyWindowHandle)

                'Keystone Frame Splitter
                If Not Graph.AddKeystoneOMNI_FrameSplitter() Then Return False

                '==================================================================================
                'SIDE A
                '==================================================================================

                'File source A
                If Not Graph.AddFilesourceA(FilePath_A) Then Return False

                'MS VC-1 ES Parsre A
                If Not Graph.AddMS_VC1ESParserA() Then Exit Function

                'MS VC-1 DMO A
                If Not Graph.AddVC1DMOA() Then Return False

                'Connections A
                HR = Graph.GraphBuilder.Connect(Graph.FSFA_Out, Graph.MS_VC1ESParserA_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MS_VC1ESParserA.FindPin("Output", Graph.MS_VC1ESParserA_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MS_VC1ESParserA_Out, Graph.MS_VC1A_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MS_VC1A_Out, Graph.KeyHD_FS_EncodeIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SIDE B
                '==================================================================================

                'File source B 
                If Not Graph.AddFilesourceB(FilePath_B) Then Return False

                'MS VC-1 ES Parsre B
                If Not Graph.AddMS_VC1ESParserB() Then Exit Function

                'MS VC-1 DMO B
                If Not Graph.AddVC1DMOB() Then Return False

                'Connections B
                HR = Graph.GraphBuilder.Connect(Graph.FSFB_Out, Graph.MS_VC1ESParserB_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MS_VC1ESParserB.FindPin("Output", Graph.MS_VC1ESParserB_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MS_VC1ESParserB_Out, Graph.MS_VC1B_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MS_VC1B_Out, Graph.KeyHD_FS_SourceIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SPLITTER ->
                '==================================================================================

                'DLV Renderer
                If Not Graph.AddDeckLinkVideo() Then Return False

                HR = Graph.GraphBuilder.Connect(Graph.KeyHD_FS_Out, Graph.DLV_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                While _Duration = 0
                    HR = Graph.MediaSeek.GetDuration(_Duration)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End While

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with BG_FS_VC1_VC1. Error: " & ex.Message)
            End Try
        End Function

        Public Function BG_FS_VC1_AVC(ByVal FilePath_A As String, ByVal FilePath_B As String, ByVal NotifyWindowHandle As Integer) As Boolean
            Try
                Graph = New cSMTGraph(NotifyWindowHandle)

                'Keystone Frame Splitter
                If Not Graph.AddKeystoneOMNI_FrameSplitter() Then Return False

                '==================================================================================
                'SIDE A
                '==================================================================================

                'File source A
                If Not Graph.AddFilesourceA(FilePath_A) Then Return False

                'MS VC-1 ES Parsre A
                If Not Graph.AddMS_VC1ESParserA() Then Exit Function

                'MS VC-1 DMO A
                If Not Graph.AddVC1DMOA() Then Return False

                'Connections A
                HR = Graph.GraphBuilder.Connect(Graph.FSFA_Out, Graph.MS_VC1ESParserA_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MS_VC1ESParserA.FindPin("Output", Graph.MS_VC1ESParserA_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MS_VC1ESParserA_Out, Graph.MS_VC1A_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MS_VC1A_Out, Graph.KeyHD_FS_EncodeIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SIDE B
                '==================================================================================

                'File source B 
                If Not Graph.AddFilesourceB(FilePath_B) Then Return False

                'Demux B
                If Not Graph.AddMCE_DMXB() Then Return False

                'Decoder B
                If Not Graph.AddMCE_AVC() Then Return False

                'Connections B
                HR = Graph.GraphBuilder.Connect(Graph.FSFB_Out, Graph.MCE_DMXB_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_DMXB.FindPin("AVC", Graph.MCE_DMXB_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXB_Out, Graph.MCE_AVC_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_AVC.FindPin("Out", Graph.MCE_AVC_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_AVC_Out, Graph.KeyHD_FS_SourceIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SPLITTER ->
                '==================================================================================

                'DLV Renderer
                If Not Graph.AddDeckLinkVideo() Then Return False

                HR = Graph.GraphBuilder.Connect(Graph.KeyHD_FS_Out, Graph.DLV_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                While _Duration = 0
                    HR = Graph.MediaSeek.GetDuration(_Duration)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End While

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with BG_FS_VC1_AVC. Error: " & ex.Message)
            End Try
        End Function

        Public Function BG_FS_VC1_M2VHD(ByVal FilePath_A As String, ByVal FilePath_B As String, ByVal NotifyWindowHandle As Integer) As Boolean
            Try
                Graph = New cSMTGraph(NotifyWindowHandle)

                'Keystone Frame Splitter
                If Not Graph.AddKeystoneOMNI_FrameSplitter() Then Return False

                '==================================================================================
                'SIDE A
                '==================================================================================

                'File source A
                If Not Graph.AddFilesourceA(FilePath_A) Then Return False

                'MS VC-1 ES Parsre A
                If Not Graph.AddMS_VC1ESParserA() Then Exit Function

                'MS VC-1 DMO A
                If Not Graph.AddVC1DMOA() Then Return False

                'Connections A
                HR = Graph.GraphBuilder.Connect(Graph.FSFA_Out, Graph.MS_VC1ESParserA_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MS_VC1ESParserA.FindPin("Output", Graph.MS_VC1ESParserA_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MS_VC1ESParserA_Out, Graph.MS_VC1A_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MS_VC1A_Out, Graph.KeyHD_FS_EncodeIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SIDE B
                '==================================================================================

                'File source B 
                If Not Graph.AddFilesourceB(FilePath_B) Then Return False

                'Demux B
                If Not Graph.AddMCE_DMXB() Then Return False

                'Decoder B
                If Not Graph.AddMCE_MP2B() Then Return False

                'Connections B
                HR = Graph.GraphBuilder.Connect(Graph.FSFB_Out, Graph.MCE_DMXB_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MCE_DMXB.FindPin("VES", Graph.MCE_DMXB_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_DMXB_Out, Graph.MCE_MP2B_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MCE_MP2B_Out, Graph.KeyHD_FS_SourceIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SPLITTER ->
                '==================================================================================

                'DLV Renderer
                If Not Graph.AddDeckLinkVideo() Then Return False

                HR = Graph.GraphBuilder.Connect(Graph.KeyHD_FS_Out, Graph.DLV_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                While _Duration = 0
                    HR = Graph.MediaSeek.GetDuration(_Duration)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End While

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with BG_FS_VC1_M2VHD. Error: " & ex.Message)
            End Try
        End Function

        Public Function BG_FS_VC1_YUV(ByVal FilePath_A As String, ByVal FilePath_B As String, ByVal NotifyWindowHandle As Integer) As Boolean
            Try
                Graph = New cSMTGraph(NotifyWindowHandle)

                'Keystone Frame Splitter
                If Not Graph.AddKeystoneOMNI_FrameSplitter() Then Return False

                '==================================================================================
                'SIDE A
                '==================================================================================

                'File source A
                If Not Graph.AddFilesourceA(FilePath_A) Then Return False

                'MS VC-1 ES Parsre A
                If Not Graph.AddMS_VC1ESParserA() Then Exit Function

                'MS VC-1 DMO A
                If Not Graph.AddVC1DMOA() Then Return False

                'Connections A
                HR = Graph.GraphBuilder.Connect(Graph.FSFA_Out, Graph.MS_VC1ESParserA_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MS_VC1ESParserA.FindPin("Output", Graph.MS_VC1ESParserA_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MS_VC1ESParserA_Out, Graph.MS_VC1A_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.MS_VC1A_Out, Graph.KeyHD_FS_EncodeIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SIDE B
                '==================================================================================

                'YUV Source
                If Not Graph.AddYUVSource() Then Return False
                HR = Graph.iYUVSource.Load(Replace(FilePath_B, "\", "\\"), 0)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim StreamAProperties As cSourceProperties = GetSourceProperties(True)
                HR = Graph.iYUVSource.SetATPF(Math.Round(10000000 / StreamAProperties.FrameRate, 0))
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'Connections B
                HR = Graph.GraphBuilder.Connect(Graph.YUV_Out, Graph.KeyHD_FS_SourceIn)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                '==================================================================================
                'SPLITTER ->
                '==================================================================================

                'DLV Renderer
                If Not Graph.AddDeckLinkVideo() Then Return False

                HR = Graph.GraphBuilder.Connect(Graph.KeyHD_FS_Out, Graph.DLV_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                While _Duration = 0
                    HR = Graph.MediaSeek.GetDuration(_Duration)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End While

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with BG_FS_VC1_YUV. Error: " & ex.Message)
            End Try
        End Function

#End Region 'VC1

#End Region 'Graphs

#Region "Shuttle"

        Private WithEvents ShuttleTimer As Timers.Timer
        Private ShuttleForward As Boolean
        Private Const ONE_SEC As Integer = 400000
        Private ShuttleStartPosition As Long
        Private ShuttlePosition As Long
        Private ShuttleStepSize As Long

        Public Sub StartShuttle(ByVal Speed As eShuttleSpeed, ByVal Forward As Boolean)
            Debug.WriteLine("StartShuttle: " & Speed)

            CurrentSpeed = Speed 'just to be sure (?)
            ShuttleForward = Forward
            If ShuttleTimer Is Nothing Then ShuttleTimer = New Timers.Timer
            ShuttleTimer.Start()

            'ShuttleStartPosition = GetPosition()
            'ShuttlePosition = ShuttleStartPosition
            'ShuttleStepSize = (ONE_SEC / 2) * Speed

            ShuttleTimer.Interval = 100
            Graph.KO_IKeystone.ActivateFFRW(Speed)

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
                Dim CP As Long
                HR = Graph.MediaSeek.GetCurrentPosition(CP)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                If ShuttleForward Then
                    CP += CurrentSpeed * 1000000 '10000000
                Else
                    CP -= CurrentSpeed * 1000000
                End If
                Debug.WriteLine(CP)
                SetPosition(CP)
            Catch ex As Exception
                Throw New Exception("Problem with ShuttleTimer_Ticker. Error: " & ex.Message)
            End Try
        End Sub

#End Region 'Shuttle

    End Class

End Namespace
