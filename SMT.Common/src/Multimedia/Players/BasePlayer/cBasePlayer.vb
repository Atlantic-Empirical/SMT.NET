#Region "IMPORTS"

Imports SMT.Multimedia.DirectShow
Imports System.Runtime.InteropServices
Imports System.Drawing
Imports System.IO
Imports SMT.DotNet.Utility
Imports SMT.Multimedia.Enums
Imports SMT.Multimedia.Classes
Imports System.Windows.Forms
Imports SMT.Win.WinAPI.Constants
Imports SMT.Multimedia.Formats.StillImage
Imports SMT.Multimedia.GraphConstruction
Imports SMT.Multimedia.UI.WinForms.Viewers
Imports SMT.Multimedia.Utility.Timecode
Imports SMT.Multimedia.UI.WPF.Viewer

#End Region 'IMPORTS

Namespace Multimedia.Players

    ''' <summary>
    ''' Each of the Player classes inherit this class.
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class cBasePlayer
        Implements IDisposable

#Region "MUST OVERRIDE"

        'Construct/Deconstruct
        Public MustOverride Function FilterCheck() As String
        Public MustOverride Function BuildGraph(ByVal FilePath As String, ByVal NotifyWindowHandle As Integer, ByVal AVMode As eAVMode, ByVal nParentForm As Form) As Boolean

        'Transport Control
        Public MustOverride Function Play() As Boolean
        Public MustOverride Function [Stop]() As Boolean
        Public MustOverride Function Pause() As Boolean
        Public MustOverride Function FastForward(Optional ByVal Speed As eShuttleSpeed = eShuttleSpeed.Not_Specified) As Boolean
        Public MustOverride Function Rewind(Optional ByVal Speed As eShuttleSpeed = eShuttleSpeed.Not_Specified) As Boolean

        Public MustOverride Function RestartStream() As Boolean

        Public MustOverride Function FrameStep() As Boolean
        Public MustOverride Function QuitFrameStepping() As Boolean

        Public MustOverride Function Timesearch() As Boolean 'need to determine appropriate target location argument type
        Public MustOverride Function JumpBack(ByVal JumpSeconds As Byte) As Boolean

        Public MustOverride Function GetPosition() As Long

        'Sample Grabbing
        Public MustOverride Function GrabSingleSample(ByVal TargetDirectory As String, ByVal Format As System.Drawing.Imaging.ImageFormat) As Boolean
        Public MustOverride Sub StartSampleRecording(ByVal TargetDirectory As String)
        Public MustOverride Sub StopSampleRecording()

        'On Screen Display
        Public MustOverride Sub SetOSD(ByVal BM As Bitmap, ByVal ColorKey As Integer, ByVal DelayMiliSecs As Short, ByVal UseColorKey As Boolean, ByVal Alpha As Single, ByVal Sender As String, ByVal X As Short, ByVal Y As Short)
        Public MustOverride Sub ClearOSD()

        Public MustOverride Sub DerivedPlayerHandleEvent(ByVal code As Integer, ByVal p1 As Integer, ByVal p2 As Integer)

#End Region 'MUST OVERRIDE

#Region "OVERRIDABLE"

        Public Overridable Function SetPosition(ByVal Pos As Long) As Boolean
            Try
                Debug.WriteLine("BASE PLAYER | IMediaSeeking::SetPositions()")
                HR = Graph.MediaSeek.SetPositions(Pos, SeekingFlags.AbsolutePositioning Or SeekingFlags.NoFlush, 0, SeekingFlags.NoPositioning)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with SetPosition(). Error: " & ex.Message)
            End Try
        End Function

        ''' <summary>
        ''' Allows derived players to support customized properties without forcing the app to ctype the player to the current player type to access specialized property methods
        ''' </summary>
        ''' <param name="PropertyID"></param>
        ''' <param name="PropertyValue"></param>
        ''' <param name="PropetyString"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function SetPlayerProperty(ByVal PropertyID As Integer, ByVal PropertyValue As Long, ByVal PropetyString As String) As Boolean
        End Function

        Public Overridable Sub Dispose() Implements IDisposable.Dispose
            Graph.DestroyGraph()
            If Viewer_WF IsNot Nothing Then
                Viewer_WF.ForceClose = True
                Viewer_WF.Close()
                Me.Viewer_WF.Dispose()
                Viewer_WF = Nothing
            End If
            If Viewer_WPF IsNot Nothing Then
                'Viewer_WPF.ForceClose = True
                Viewer_WPF.Close()
                'Viewer_WPF.Dispose()
                Viewer_WPF = Nothing
            End If
        End Sub

        Public Overridable Function GetSourceProperties() As cSourceProperties
            Try
                Dim AMT As AM_MEDIA_TYPE
                If Not Graph.KO_IKeystone Is Nothing Then
                    HR = Me.Graph.KO_IKeystone.get_InputMediaType(AMT)
                ElseIf Not Graph.KO_IKeystone Is Nothing Then
                    HR = Me.Graph.KO_IKeystone.get_InputMediaType(AMT)
                ElseIf Not Graph.VMR9 Is Nothing Then
                    If Graph.VMR9_In_1 Is Nothing Then
                        Graph.VMR9_In_1 = Graph.FindPinByPartialName(Graph.VMR9, "Input0")
                    End If
                    Dim cmt As cSMTGraph.cFullMediaType = cSMTGraph.GetPinConnectedMediaType(Graph.VMR9_In_1)
                    AMT = cmt.AMMT
                End If
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim VIH As VIDEOINFOHEADER = CType(Marshal.PtrToStructure(AMT.pbFormat, GetType(VIDEOINFOHEADER)), VIDEOINFOHEADER)
                Dim out As New cSourceProperties
                out.FrameRate = Math.Round(10000000 / VIH.AvgTimePerFrame, 3)
                out.ATPF = VIH.AvgTimePerFrame
                out.Height = VIH.SrcRect.Bottom
                out.Width = VIH.SrcRect.Right
                Return out
            Catch ex As Exception
                Throw New Exception("Problem with GetSourceFrameRate(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Overridable Function GetBitmapFromKeystone() As Byte()
        End Function

        Public Overridable Function GetYUVSampleFromKeystone() As Byte()
        End Function

        Public Overridable Function RenderBitmap(ByVal BM As Bitmap, ByVal X As Short, ByVal Y As Short) As Boolean
        End Function

        Public Overridable Function RenderImageResource(ByVal PNGstr As Stream, ByVal X As Short, ByVal Y As Short) As Boolean
        End Function

        Public Overridable Function ReverseFieldOrder(ByVal SetActive As Boolean) As Boolean
        End Function

        Public Overridable Sub PlayPauseToggle()
            If PlayState = ePlayState.Playing Then
                Pause()
            Else
                Play()
            End If
        End Sub

#Region "BARDATA"

        Public Overridable Sub BarDataConfig(ByVal DetectBarData As Integer, ByVal BurnGuides As Integer, ByVal Luma_Tolerance As Integer, ByVal Chroma_Tolerance As Integer)
        End Sub

        Public Overridable Sub BarDataReset()
        End Sub

        Public ReadOnly Property TopBar() As Integer
            Get
                Return _TopBar
            End Get
        End Property
        Protected _TopBar As Integer = 0

        Public ReadOnly Property LeftBar() As Integer
            Get
                Return _LeftBar
            End Get
        End Property
        Protected _LeftBar As Integer = 0

        Public ReadOnly Property RightBar() As Integer
            Get
                Return _RightBar
            End Get
        End Property
        Protected _RightBar As Integer = 0

        Public ReadOnly Property BottomBar() As Integer
            Get
                Return _BottomBar
            End Get
        End Property
        Protected _BottomBar As Integer = 0

#End Region 'BARDATA

#End Region 'OVERRIDABLE

#Region "PRIVATE PROPERTIES"

#End Region 'PRIVATE PROPERTIES

#Region "PUBLIC PROPERTIES"

        Public Graph As cSMTGraph
        Public ParentForm As cSMTForm
        Public FilePath As String
        Public NotifyWindowHandle As Integer

        Public CurrentVideoDimensions As System.Drawing.Size = New Point(1920, 1080)
        Public IsFieldSplitting As Boolean = False

        Public Property PlayState() As ePlayState
            Get
                Return _PlayState
            End Get
            Set(ByVal value As ePlayState)
                _PlayState = value
            End Set
        End Property
        Private _PlayState As ePlayState

        Public Property CurrentSpeed() As eShuttleSpeed
            Get
                Return _CurrentSpeed
            End Get
            Set(ByVal value As eShuttleSpeed)
                _CurrentSpeed = value
            End Set
        End Property
        Private _CurrentSpeed As eShuttleSpeed

        Public ReadOnly Property TitleSafeTop() As Short
            Get
                Return CurrentVideoDimensions.Height - (0.9 * CurrentVideoDimensions.Height)
            End Get
        End Property

        Public ReadOnly Property TitleSafeLeft() As Short
            Get
                Return CurrentVideoDimensions.Width - (0.9 * CurrentVideoDimensions.Width)
            End Get
        End Property

        Public ReadOnly Property Duration_InReferenceTime() As UInt64
            Get
                Return _Duration
            End Get
        End Property
        Protected _Duration As UInt64

        Public ReadOnly Property PlayerType() As ePlayerType
            Get
                If FilePath Is Nothing OrElse FilePath = "" Then Return ePlayerType.NOT_SPECIFIED
                If Path.HasExtension(FilePath) Then
                    Select Case Path.GetExtension(FilePath).ToLower
                        Case ".ifo"
                            Return ePlayerType.DVD
                        Case ".m2ts"
                            Return ePlayerType.M2TS
                        Case ".avi"
                            Return ePlayerType.AVI
                        Case ".vc1"
                            Return ePlayerType.VC1
                        Case ".mpg", ".mpeg"
                            Return ePlayerType.MPG
                        Case ".avc", ".h264"
                            Return ePlayerType.AVC
                        Case ".mov"
                            Return ePlayerType.MOV
                        Case ".m2v"
                            Return ePlayerType.M2V
                        Case ".m1v"
                            Return ePlayerType.M1V
                        Case ".mpa"
                            Return ePlayerType.MPA
                        Case ".dts", ".ac3"
                            Return ePlayerType.AC3DTS
                        Case ".pcm", ".wav"
                            Return ePlayerType.PCM
                        Case ".wmv"
                            Return ePlayerType.WMV
                        Case ".ifo"
                            Return ePlayerType.DVD
                    End Select
                Else
                    If File.Exists(FilePath & "video_ts.ifo") Then Return ePlayerType.DVD
                    If File.Exists(FilePath & "\video_ts.ifo") Then Return ePlayerType.DVD
                    Return ePlayerType.NOT_SPECIFIED
                End If
            End Get
        End Property

        Public Property AVMode() As eAVMode
            Get
                If Me.Viewer_WF IsNot Nothing OrElse Viewer_WPF IsNot Nothing Then
                    Return eAVMode.DesktopVMR
                End If
                Return _AVMode
            End Get
            Set(ByVal value As eAVMode)
                _AVMode = value
                RaiseEvent evAVModeSet(value)
            End Set
        End Property
        Private _AVMode As eAVMode = eAVMode.DesktopVMR

        Public Overridable ReadOnly Property CurrentRunningTime() As cTimecode
            Get
                Try
                    Dim SP As cSourceProperties = GetSourceProperties()
                    Return New cTimecode(CurrentRunningTime_InREFERENCE_TIME, SP.ATPF, DoubleTo_eFrameRate(SP.FrameRate))
                Catch ex As Exception
                    Throw New Exception("Problem with CurrentRunningTime in cBasePlayer. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property CurrentRunningTime_InREFERENCE_TIME() As UInt64
            Get
                Try
                    Return GetPosition()
                Catch ex As Exception
                    Throw New Exception("Problem with CurrentRunningTime in cBasePlayer. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public Overridable ReadOnly Property CurrentRunningTime_InSeconds() As UInt32
            Get
                Return CurrentRunningTime_InREFERENCE_TIME / 10000000
            End Get
        End Property

        ''' <summary>
        ''' If there is an audio renderer in the graph this returns true.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property AudioPresent() As Boolean
            Get
                Return Not Graph.AudioRenderer Is Nothing
            End Get
        End Property

        Protected _CurrentPTS As UInt64

        Public ReadOnly Property Duration_InSeconds() As Integer
            Get
                Return Duration_InReferenceTime / 10000000
            End Get
        End Property

#End Region 'PUBLIC PROPERTIES

#Region "PROTECTED PROPERTIES"

        Protected HR As Integer

#End Region 'PROTECTED PROPERTIES

#Region "PUBLIC METHODS"

        Public Function ToggleFieldSplitting() As Boolean
            Try
                IsFieldSplitting = Not IsFieldSplitting
                If Not Graph.KO_IKeystone Is Nothing Then
                    If IsFieldSplitting Then
                        HR = Graph.KO_IKeystoneMixer.FieldSplit(1)
                    Else
                        HR = Graph.KO_IKeystoneMixer.FieldSplit(0)
                    End If
                ElseIf Not Graph.KO_IKeystone Is Nothing Then
                    If IsFieldSplitting Then
                        HR = Graph.KO_IKeystoneMixer.FieldSplit(1)
                    Else
                        HR = Graph.KO_IKeystoneMixer.FieldSplit(0)
                    End If
                Else
                    Return False
                End If
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with SplitFields(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function SetTimeFormat() As Boolean
            Try
                If Graph Is Nothing Then Return False
                If Graph.MediaSeek Is Nothing Then Return False

                'HR = Graph.MediaSeek.IsFormatSupported(TimeFormat.TIME_FORMAT_MEDIA_TIME)
                'Debug.WriteLine(HR)
                'HR = Graph.MediaSeek.IsFormatSupported(TimeFormat.TIME_FORMAT_FIELD)
                'Debug.WriteLine(HR)

                ''Dim i As UInt32
                ''HR = Graph.MediaSeek.GetTimeFormat(i)
                ' ''If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                ''Dim o As Object = Marshal.PtrToStructure(New IntPtr(i), GetType(Guid))

                ''Marshal.ptr()

                HR = Graph.MediaSeek.SetTimeFormat(TimeFormat.TIME_FORMAT_MEDIA_TIME)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

            Catch ex As Exception
                Throw New Exception("Problem with SetTimeFormat(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function DoubleTo_eFrameRate(ByVal FR As Double) As eFramerate
            Select Case FR
                Case 23.976, 24
                    Return eFramerate.FILM
                Case 25
                    Return eFramerate.PAL
                Case 29.97, 30
                    Return eFramerate.NTSC
                Case Else
                    Return Nothing
            End Select
        End Function

#Region "MAINCONCEPT FILTER CONTROL"

        Public Function ToggleMainConceptOSD(ByVal TurnOn As Boolean) As Boolean
            If Graph Is Nothing Then Return False
            Return Graph.MCE_ToggleDecoderOSD(TurnOn)
        End Function

#End Region 'MAINCONCEPT FILTER CONTROL

#End Region 'PUBLIC METHODS

#Region "PROPERTY PAGES"

        Public Sub ShowRendererPropPage(ByVal AppWinHndl As Integer)
            Try
                Dim PGs As DsCAUUID
                Graph.VideoRenderer_PP.GetPages(PGs)
                Dim FI As New FilterInfo
                If Graph.DLVideo IsNot Nothing Then
                    DsUtils.OleCreatePropertyFrame(AppWinHndl, 0, 0, FI.achName, 1, Graph.DLVideo, PGs.cElems, PGs.pElems, 0, 0, Nothing)
                Else
                    DsUtils.OleCreatePropertyFrame(AppWinHndl, 0, 0, FI.achName, 1, Graph.VMR9, PGs.cElems, PGs.pElems, 0, 0, Nothing)
                End If
            Catch ex As Exception
                Throw New Exception("Problem with ShowRendererPropPage(). Error: " & ex.Message)
            End Try
        End Sub

        Public Sub ShowDecoderPropPage(ByVal AppWinHndl As Integer)
            Try
                If Me._CurrentDecoder Is Nothing Then Exit Sub
                Dim PGs As DsCAUUID
                Dim PP As ISpecifyPropertyPages = CType(_CurrentDecoder, ISpecifyPropertyPages)
                PP.GetPages(PGs)
                Dim FI As New FilterInfo
                DsUtils.OleCreatePropertyFrame(AppWinHndl, 0, 0, FI.achName, 1, _CurrentDecoder, PGs.cElems, PGs.pElems, 0, 0, Nothing)
            Catch ex As Exception
                Throw New Exception("Problem with ShowDecoderPropPage(). Error: " & ex.Message)
            End Try
        End Sub
        Protected _CurrentDecoder As IBaseFilter

        Public Sub ShowKeystonePropertyPage(ByVal AppWinHndl As Integer)
            Try
                Dim PGs As DsCAUUID
                'Dim PP As ISpecifyPropertyPages = CType(Graph.KeyHD, ISpecifyPropertyPages)
                Graph.KO_PP.GetPages(PGs)
                Dim FI As New FilterInfo
                DsUtils.OleCreatePropertyFrame(AppWinHndl, 0, 0, FI.achName, 1, _CurrentDecoder, PGs.cElems, PGs.pElems, 0, 0, Nothing)
            Catch ex As Exception
                Throw New Exception("Problem with ShowKeystonePropertyPage(). Error: " & ex.Message)
            End Try
        End Sub

#End Region 'PROPERTY PAGES

#Region "OSD"

        Public Function OnScreenDisplay_ShowFastForwardRewindSpeed(ByVal Speed As String, ByVal FF As Boolean) As Bitmap
            Try
                Dim BM As New Bitmap(140, 31, Imaging.PixelFormat.Format24bppRgb)
                Dim G As Graphics = Graphics.FromImage(BM)
                Dim B As Brush = New SolidBrush(Color.White) 'for keying
                G.FillRectangle(B, 0, 0, 140, 31)

                Dim text_path As New System.Drawing.Drawing2D.GraphicsPath(Drawing2D.FillMode.Alternate)
                Dim dir As String
                If FF Then
                    dir = "FF: "
                Else
                    dir = "RW: "
                End If
                text_path.AddString(dir & Speed & "X", New FontFamily("Arial"), FontStyle.Regular, 20, New Point(0, 0), StringFormat.GenericDefault)
                G.DrawPath(New Pen(NTSCBlack, 2), text_path)
                G.FillPath(New SolidBrush(Color.LightBlue), text_path)
                text_path.Dispose()
                G.Dispose()

                'Dim F As New System.Drawing.Font("Arial", 7)
                'Dim R As New RectangleF(2, 2, 58, 13)
                'G.DrawString("Chapter: " & NewChNo, F, Brushes.Lime, R)

                'BM.Save("c:\temp.bmp")

                Return BM
            Catch ex As Exception
                Throw New Exception("Problem with OSDBitmap_FFRWSpeed(). Error: " & ex.Message)
                Return Nothing
            End Try
        End Function

        Private ReadOnly Property NTSCBlack() As Color
            Get
                Return Color.FromArgb(16, 16, 16)
            End Get
        End Property

#End Region 'OSD

#Region "AUDIO SPECIFIC FUNCTIONALITY"

        Protected Function CreateAudioPBBMP(ByVal RandomPosition As Boolean, ByVal FileName As String) As Bitmap
            Try
                Dim bm As New Bitmap(720, 480, Imaging.PixelFormat.Format24bppRgb)
                Dim gr As Graphics = System.Drawing.Graphics.FromImage(bm)
                Dim fn As New Font(System.Drawing.FontFamily.GenericSansSerif, 65, FontStyle.Bold, GraphicsUnit.Point)
                Dim br As New SolidBrush(Color.FromArgb(255, 16, 16, 235))
                Dim sf As New StringFormat
                sf.Alignment = StringAlignment.Center
                sf.LineAlignment = StringAlignment.Center

                If RandomPosition Then
                    Dim rnd As New Random
                    Dim d As Double = rnd.NextDouble
                    Dim x As Short = Math.Round(d * 720, 0)
                    d = rnd.NextDouble
                    Dim y As Short = Math.Round(d * 480, 0)
                    gr.DrawString("SMT PHOENIX", fn, br, x, y, sf)
                    gr.DrawString(FileName, fn, br, x, y + 80, sf)
                Else
                    gr.DrawString("SMT PHOENIX", fn, br, 360, 200, sf)
                    gr.DrawString(FileName, fn, br, 360, 280, sf)
                End If

                Return bm
            Catch ex As Exception
                Throw New Exception("Problem with CreateAudioPBBMP. Error: " & ex.Message)
            End Try
        End Function

#End Region 'AUDIO SPECIFIC FUNCTIONALITY

#Region "SAMPLE GRABBING"

        Public NowRecording As Boolean = False

#Region "SAMPLE GRABBING:VIDEO"

        Protected Function OutputImageFormat(ByVal ImageFormat As String) As Imaging.ImageFormat
            Select Case ImageFormat.ToLower
                Case "bmp"
                    Return Imaging.ImageFormat.Bmp
                Case "jpg"
                    Return Imaging.ImageFormat.Jpeg
                Case "gif"
                    Return Imaging.ImageFormat.Gif
                Case "png"
                    Return Imaging.ImageFormat.Png
                Case "tif"
                    Return Imaging.ImageFormat.Tiff
                Case Else
                    Debug.WriteLine("Failed in OutputImageFormat() in cBasePlayer.")
                    Return Nothing
            End Select
        End Function

        Protected Sub KeystoneForcedFrameGrab(ByVal ptr As IntPtr, ByVal sz As Long)
            Try
                Dim buffer() As Byte
                Dim fs As FileStream
                Dim h As Short
                Dim BMI() As Byte
                Dim TmpBuff() As Byte
                Dim OutputFileSize As Integer
                Dim SampleBitmapBuffer() As Byte

                ReDim buffer(sz - 1)
                Marshal.Copy(ptr, buffer, 0, sz)

                h = sz / 1920 / 3

                'Flip Image
                buffer = FlipImageBuffer_Vertically(buffer)
                buffer = FlipRGB24ImageBuffer_Horizontally(buffer, 1920, h)

                'Make BMI header
                ReDim BMI(53)
                BMI(0) = 66 'B
                BMI(1) = 77 'M

                OutputFileSize = sz + 54

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
                TmpBuff = ConvertDecimalIntoByteArray(1920, 4)
                BMI(18) = TmpBuff(0)
                BMI(19) = TmpBuff(1)
                BMI(20) = TmpBuff(2)
                BMI(21) = TmpBuff(3)

                'Height
                TmpBuff = ConvertDecimalIntoByteArray(1080, 4)
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

                ReDim SampleBitmapBuffer(OutputFileSize)
                Array.Copy(BMI, 0, SampleBitmapBuffer, 0, 54)
                Array.Copy(buffer, 0, SampleBitmapBuffer, 54, buffer.Length)

                fs = New FileStream("C:\Temp\tmp" & DateTime.Now.Ticks & ".bmp", IO.FileMode.OpenOrCreate)
                fs.Write(SampleBitmapBuffer, 0, SampleBitmapBuffer.Length)
                fs.Close()
                fs = Nothing
                Debug.WriteLine("Forced grab saved.")
            Catch ex As Exception
                Throw New Exception("Problem with KeystoneForcedFrameGrab(). Error: " & ex.Message)
            End Try
        End Sub

#Region "SAMPLE GRABBING:VIDEO:RECORDING"

        Public Function BatchProcessRecordedImages(ByVal OutputFormat As Imaging.ImageFormat, ByVal ImgPath As String) As Boolean
            Try
                Dim DI As New DirectoryInfo(ImgPath)
                Dim Files() As FileInfo = DI.GetFiles("*.bin")
                Dim bMinute As Byte = DateTime.Now.Minute
                Dim OutPath As String
                For i As Integer = 0 To UBound(Files)
                    'If Not Files(i).Extension = ".bin" Then GoTo NextFile

                    If OutputFormat Is Imaging.ImageFormat.Wmf Then
                        OutPath = Path.GetDirectoryName(Files(i).FullName)
                        OutPath &= "\Output_" & bMinute & "\" & Path.GetFileNameWithoutExtension(Files(i).FullName) & ".yuv"
                        If Not Directory.Exists(Path.GetDirectoryName(OutPath)) Then Directory.CreateDirectory(Path.GetDirectoryName(OutPath))
                        System.IO.File.Move(Files(i).FullName, OutPath)
                    ElseIf Me.ProcessOneRecordedImage(Files(i).FullName, bMinute, OutputFormat) Then
                        Files(i).Delete()
                    Else
                        Return False
                    End If
                    RaiseEvent evMultiFrameGrabImageProcessed(Path.GetFileName(Files(i).FullName), i + 1, Files.Length)
NextFile:
                Next
                RaiseEvent evMultiFrameGrabProcessComplete()
                Return True
            Catch ex As Exception
                Throw New Exception("Prob with BatchProcessRecordedImages. Error: " & ex.Message, ex)
            End Try
        End Function

        Protected Function ProcessOneRecordedImage(ByVal fPath As String, ByVal bMinute As Byte, ByVal OutputFormat As Imaging.ImageFormat) As Boolean
            Try
                Dim fs As New FileStream(fPath, IO.FileMode.Open)
                Dim Buffer(fs.Length - 1) As Byte
                fs.Read(Buffer, 0, fs.Length)
                fs.Close()
                fs = Nothing

                Dim OutputFileSize As Integer = 6220800 + 54

                ''DEBUGGING - Save the buffer
                'FS = New FileStream("C:\Test\Buffer.bin", FileMode.OpenOrCreate)
                'FS.Write(Buffer, 0, Buffer.Length)
                'FS.Close()
                'FS = Nothing
                ''END DEBUGGING

                'Flip Image
                Buffer = FlipImageBuffer_Vertically(Buffer)
                Buffer = FlipRGB24ImageBuffer_Horizontally(Buffer, 1920, 1080)

                'Make BMI header
                Dim BMI(53) As Byte
                BMI(0) = &H42 'B
                BMI(1) = &H4D 'M

                'FileSize
                '36 ec 5e 00 = 6220854
                BMI(2) = &H36
                BMI(3) = &HEC
                BMI(4) = &H5E
                BMI(5) = 0

                BMI(10) = &H36 'Header size
                BMI(14) = &H28 'InfoHeader size

                'Width
                BMI(18) = &H80
                BMI(19) = &H7
                BMI(20) = 0
                BMI(21) = 0

                'Height
                BMI(22) = &H38
                BMI(23) = &H4
                BMI(24) = 0
                BMI(25) = 0

                BMI(26) = 1     'Planes
                BMI(28) = &H18  'Bit depth

                'Raster data size
                BMI(34) = 0
                BMI(35) = &HEC
                BMI(36) = &H5E
                BMI(37) = 0

                '42 4d = BM
                '36 ec 5e 00 = filesize
                '00 00 00 00 
                '36 00 00 00 = header size 
                '28 00 00 00 = infoheader size
                '80 07 00 00 = width
                '38 04 00 00 = height
                '01 00 = planes
                '18 00 = bitdepth
                '00 00 00 00 
                '00 ec 5e 00 = raster data size 
                '00 00 00 00 
                '00 00 00 00 
                '00 00 00 00 
                '00 00 00 00 

                Dim SampleBitmapBuffer(OutputFileSize) As Byte
                Array.Copy(BMI, 0, SampleBitmapBuffer, 0, 54)
                Array.Copy(Buffer, 0, SampleBitmapBuffer, 54, Buffer.Length)

                ''OUTPUT
                'Dim OutPath As String = Path.GetDirectoryName(fPath)
                'OutPath &= "\BMPs\" & Path.GetFileNameWithoutExtension(fPath) & ".bmp"
                'If Not Directory.Exists(Path.GetDirectoryName(OutPath)) Then Directory.CreateDirectory(Path.GetDirectoryName(OutPath))
                'fs = New FileStream(OutPath, IO.FileMode.OpenOrCreate)
                'fs.Write(SampleBitmapBuffer, 0, SampleBitmapBuffer.Length)
                'fs.Close()
                'fs = Nothing

                'Buffer = Nothing
                'SampleBitmapBuffer = Nothing
                'Return True

                'Buffer = Nothing

                'Make the bitmap object
                Dim MS As New MemoryStream(SampleBitmapBuffer.Length)
                MS.Write(SampleBitmapBuffer, 0, SampleBitmapBuffer.Length)
                Dim BM As New Bitmap(MS)

                ''debugging
                'fs = New FileStream("c:\temp.bmp", IO.FileMode.OpenOrCreate)
                'fs.Write(SampleBitmapBuffer, 0, SampleBitmapBuffer.Length)
                'fs.Close()
                'fs = Nothing
                'Dim BM As New Bitmap("C:\temp.bmp")
                'Me.pbFrameGrab.Image = BM
                'Return False
                ''debugging

                'SampleBitmapBuffer = Nothing

                'BM = Me.ScaleImage(BM, 1080, 1920)

                'DEBUGGING
                'BM.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) & "\test.bmp")

                ''Save the bitmap
                'Dim FS As New FileStream(tOutpath, FileMode.OpenOrCreate)
                'FS.Write(Buffer, 0, Buffer.Length)
                'FS.Close()
                'FS = Nothing
                'END DEBUGGING

                'Get save location
                Dim OutPath As String = Path.GetDirectoryName(fPath)
                OutPath &= "\Output_" & bMinute & "\" & Path.GetFileNameWithoutExtension(fPath) & "." & OutputFormat.ToString
                If Not Directory.Exists(Path.GetDirectoryName(OutPath)) Then Directory.CreateDirectory(Path.GetDirectoryName(OutPath))

                'Save the image
                BM.Save(OutPath, OutputFormat)
                BM.Dispose()

                'Clean up
                BM = Nothing
                'MS.Close()
                'MS = Nothing
                Return True
            Catch ex As Exception
                Throw New Exception("Prob with ProcessOneRecordImage. Error: " & ex.Message)
            End Try
        End Function

#End Region 'SAMPLE GRABBING:VIDEO:RECORDING

#End Region 'SAMPLE GRABBING:VIDEO

#End Region 'SAMPLE GRABBING

#Region "EVENTS"

        Public Sub BaseHandleEvent()
            Dim hr, p1, p2 As Integer
            Dim code As DsEvCode
            Do
                If Graph Is Nothing Then Exit Sub
                If Graph.MediaEvt Is Nothing Then Exit Sub
                hr = Graph.MediaEvt.GetEvent(code, p1, p2, 0)
                If hr < 0 Then Exit Sub
                If BasePlayerEventHandler(code, p1, p2) Then Exit Sub
                DerivedPlayerHandleEvent(code, p1, p2)
                If Not Graph.MediaEvt Is Nothing Then
                    hr = Graph.MediaEvt.FreeEventParams(code, p1, p2)
                End If
            Loop While hr = 0
        End Sub

        ''' <summary>
        ''' WARNING: This method uses the OMNI-defined event values. Use with KeySD/KeyHD will result in unprediciable event evaluation.
        ''' </summary>
        ''' <param name="code"></param>
        ''' <param name="p1"></param>
        ''' <param name="p2"></param>
        ''' <returns>True if it has been handled here and should NOT be passed to the derived player.</returns>
        ''' <remarks></remarks>
        Protected Function BasePlayerEventHandler(ByVal code As Integer, ByVal p1 As Integer, ByVal p2 As Integer) As Boolean
            Try
                Select Case code

                    Case &H70 'EC_KEYSTONE_FRAMEDROPPED
                        'Debug.WriteLine("EC_KEYSTONE_FRAMEDROPPED late=" & p1)
                        Return True

                    Case &H71 'EC_KEYSTONE_WRONGDURATION
                        'Debug.WriteLine("EC_KEYSTONE_WRONGDURATION dur=" & p1)
                        Return True

                    Case &H68 'EC_KEYSTONE_FRAMEDELIVERED
                        Debug.WriteLine("EC_KEYSTONE_FRAMEDELIVERED: " & p1 & " - " & p2)
                        Return True

                    Case &H72 'EC_KEYSTONE_SAMPLESNOTADJACENT
                        'Debug.WriteLine("EC_KEYSTONE_SAMPLESNOTADJACENT")
                        Return True

                    Case DsEvCode.EC_COMPLETE
                        Debug.WriteLine("EC_COMPLETE")
                        RaiseEvent evStreamEnd()

                    Case &H73 'EC_KEYSTONE_ENDOFSTREAM
                        Debug.WriteLine("EVENT: Complete")
                        RaiseEvent evStreamEnd()

                    Case &H74 '#define EC_KEYSTONE_MEDIATIME
                        _CurrentPTS = p2
                        Dim p1U As UInt64 = p1 And 3 'p1 only has two valid bits
                        p1U = p1U << 31 'the p1 bits go above the p2 bits
                        _CurrentPTS = _CurrentPTS Or p1U
                        'Debug.WriteLine("_CurrentPTS = " & _CurrentPTS)
                        RaiseEvent evMediaTime(_CurrentPTS)

                    Case &H75 'EC_KEYSTONE_BARDATA_TOP_BOTTOM	
                        Me._TopBar = p1
                        Me._BottomBar = p2
                        Return False 'pass for derived filters

                    Case &H76 'EC_KEYSTONE_BARDATA_LEFT_RIGHT	
                        Me._LeftBar = p1
                        Me._RightBar = p2
                        'Debug.WriteLine(LeftBar & " * " & TopBar & " * " & RightBar & " * " & BottomBar)
                        RaiseEvent evBarDataChanged()
                        Return False 'pass for derived filters

                    Case &H77 'EC_KEYSTONE_BARDATA_FRAME_TOO_DARK
                        RaiseEvent evBarDataTooDark()
                        Return False 'pass for derived filters

                    Case &H78 'EC_KEYSTONE_PRESENTATIONTIMES
                        'Debug.WriteLine("EC_KEYSTONE_PRESENTATIONTIMES = " & p1 & " - " & p2)
                        RaiseEvent evPresentationTimes(p1, p2)
                        Return False

                    Case &H79 'EC_KEYSTONE_RUNNOTCALLED
                        RaiseEvent evRunNotCalled()
                        Return False

                    Case &H80 'EC_KEYSTONE_STREAMTIME
                        'RaiseEvent evStreamTime(p1)
                        Return False

                    Case &H81 'EC_KEYSTONE_SETPRESENTATIONTIME
                        RaiseEvent evPresentationTimeSet(p1, p2)
                        Return False

                    Case &H82 'EC_KEYSTONE_RECEIVE
                        RaiseEvent evReceive()
                        Return False

                    Case &H83 'EC_KEYSTONE_DELIVER
                        RaiseEvent evDeliver()
                        Return False

                    Case &H84 'EC_KEYSTONE_RUN
                        RaiseEvent evRun(p1)
                        Return False

                End Select
                Return False
            Catch ex As Exception
                Throw New Exception("Problem with BasePlayerEventHandler(). Error: " & ex.Message)
            End Try
        End Function

        Public Event evTransportControl(ByVal Type As eTransportControlTypes)
        Public Event evStreamEnd()
        Public Event evMultiFrameGrabImageProcessed(ByVal FileName As String, ByVal FileNumber As Integer, ByVal FileCount As Integer)
        Public Event evMultiFrameGrabProcessComplete()
        'Public Event evPlayerTypeSet(ByVal Mode As ePlayerType)
        Public Event evAVModeSet(ByVal Mode As eAVMode)
        Public Event evMediaTime(ByVal Value1 As UInt64)
        Public Event evBarDataChanged_base()
        Public Event evBarDataTooDark_base()
        Public Event evBarDataChanged()
        Public Event evBarDataTooDark()
        Public Event evPresentationTimes(ByVal p1 As Integer, ByVal p2 As Integer)
        Public Event evRunNotCalled()
        Public Event evStreamTime(ByVal p1 As UInt64)
        Public Event evPresentationTimeSet(ByVal Source As Integer, ByVal StartTime As Integer)
        Public Event evReceive()
        Public Event evDeliver()
        Public Event evRun(ByVal Time As Integer)
        Public Event evInitialized()

        ''' <summary>
        ''' When an app receives this event it must kill this object immediately. This object can do nothing if it cannot build a graph.
        ''' </summary>
        ''' <remarks></remarks>
        Public Event evFilterCheckFailure(ByVal Msg As String)

        Protected Sub RaiseEvent_TransportControl(ByVal Type As eTransportControlTypes)
            RaiseEvent evTransportControl(Type)
        End Sub

        Protected Sub RaiseEvent_Initialized()
            RaiseEvent evInitialized()
        End Sub

#End Region 'EVENTS

#Region "VIEWER"

        Public WithEvents Viewer_WF As Viewer_Form
        Public WithEvents Viewer_WPF As Viewer_WPF

        Public ViewerHostControl As cSMTForm

        Public Sub ShowViewer(Optional ByVal Caption As String = "Viewer")
            If Viewer_WF IsNot Nothing Then
                If Me.Viewer_WF Is Nothing Then Exit Sub
                Me.Viewer_WF.Text = Caption
                Me.Viewer_WF.Show()
                If Viewer_WF.Left > Screen.PrimaryScreen.WorkingArea.Width Or Viewer_WF.Left < Screen.PrimaryScreen.WorkingArea.Left Then
                    Viewer_WF.Left = 0
                End If
                If Viewer_WF.Top > Screen.PrimaryScreen.WorkingArea.Height Or Viewer_WF.Top < Screen.PrimaryScreen.WorkingArea.Top Then
                    Viewer_WF.Top = 0
                End If
                If Me.ParentForm Is Nothing Then Exit Sub
                'Me.ParentForm.BringToFront()
                'Me.ParentForm.Focus()
            Else
                If Me.Viewer_WPF Is Nothing Then Exit Sub
                Me.Viewer_WPF.Content = Caption
                Me.Viewer_WPF.Show()
                If Viewer_WPF.Left > Screen.PrimaryScreen.WorkingArea.Width Or Viewer_WPF.Left < Screen.PrimaryScreen.WorkingArea.Left Then
                    Viewer_WPF.Left = 0
                End If
                If Viewer_WPF.Top > Screen.PrimaryScreen.WorkingArea.Height Or Viewer_WPF.Top < Screen.PrimaryScreen.WorkingArea.Top Then
                    Viewer_WPF.Top = 0
                End If
                If Me.ParentForm Is Nothing Then Exit Sub
                'Me.ParentForm.BringToFront()
                'Me.ParentForm.Focus()

            End If
        End Sub

        Protected Sub SetupViewer(ByVal W As Short, ByVal H As Short, ByVal DVDPlayer As Boolean)
            Try
                Viewer_WF = New Viewer_Form(ParentForm.Left + ParentForm.Width, ParentForm.Top, Me.ParentForm.Icon, Me, DVDPlayer)
                Viewer_WF.ClientSize = New System.Drawing.Size(W, H)

                Graph.VideoWin = CType(Graph.GraphBuilder, IVideoWindow)
                Graph.VideoWin.put_Owner(Viewer_WF.Handle)
                Graph.VideoWin.put_WindowStyle(WS_CHILD Or WS_CLIPSIBLINGS Or WS_CLIPCHILDREN)
                Graph.VideoWin.put_MessageDrain(Viewer_WF.Handle)
                Graph.VideoWin.SetWindowPosition(Viewer_WF.ClientRectangle.X, Viewer_WF.ClientRectangle.Y, Viewer_WF.ClientRectangle.Right, Viewer_WF.ClientRectangle.Bottom)
            Catch ex As Exception
                Throw New Exception("Problem with SetupViewer(). Error: " & ex.Message)
            End Try
        End Sub

        Public Sub SetupViewer_WPF()
            Try
                If Viewer_WPF IsNot Nothing Then Viewer_WPF.Close()
                Viewer_WPF = New Viewer_WPF(Me, "Viewer")
                'Viewer_WPF.Icon = ParentForm.Icon
                Viewer_WPF.Left = ParentForm.Left
                Viewer_WPF.Top = ParentForm.Top + ParentForm.Height
                Viewer_WPF.ShowDialog()
                Viewer_WPF.ViewerSize = eViewerSize.HD_Quarter_480x270
            Catch ex As Exception
                Throw New Exception("Problem with SetupViewer(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Protected Sub PlaceViewerInControl(ByVal FormHandle As Integer, ByRef ViewerControl As Control)
            Try
                Graph.VideoWin = CType(Graph.GraphBuilder, IVideoWindow)
                Graph.VideoWin.put_Owner(ViewerControl.Handle)
                Graph.VideoWin.put_WindowStyle(WS_CHILD Or WS_CLIPSIBLINGS Or WS_CLIPCHILDREN)
                Graph.VideoWin.put_MessageDrain(ViewerControl.Handle)
                Graph.VideoWin.SetWindowPosition(ViewerControl.ClientRectangle.X, ViewerControl.ClientRectangle.Y, ViewerControl.ClientRectangle.Right, ViewerControl.ClientRectangle.Bottom)
            Catch ex As Exception
                Throw New Exception("Problem in PlaceViewerInControl(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Sub SetViewerPositionInParent(ByVal Left As Integer, ByVal Top As Integer, ByVal Width As Integer, ByVal height As Integer)
            Graph.VideoWin.SetWindowPosition(Left, Top, Width, height)
        End Sub

#End Region 'VIEWER

    End Class

End Namespace
