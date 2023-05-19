#Region "IMPORTS"

Imports Microsoft.Win32
Imports SMT.Multimedia.Formats.AC3
Imports SMT.Multimedia.Formats.MPEG2
Imports SMT.Multimedia.DirectShow.BlackMagic
Imports SMT.Multimedia.DirectShow.BlackMagic.mDecklinkInstallationTester
Imports SMT.Multimedia.Filters.SMT.Keystone
Imports SMT.Multimedia.Filters.SMT.AMTC
Imports SMT.Multimedia.Filters.SMT.L21G
Imports SMT.Multimedia.Filters.nVidia
Imports SMT.Multimedia.GraphConstruction
Imports SMT.Multimedia.DirectShow.MainConcept
Imports SMT.Multimedia.DirectShow
Imports SMT.Multimedia.DirectShow.DVD
Imports SMT.Multimedia.DirectShow.nVidia.DecoderControl
Imports SMT.Multimedia.DirectShow.SMT_DShow.mFilterInstallationVerification
Imports SMT.Multimedia.Formats.DVD.Globalization
Imports SMT.Multimedia.Players.DVD.Enums
Imports SMT.Multimedia.Players.DVD.Structures
Imports SMT.Multimedia.Players.DVD.Classes
Imports SMT.Multimedia.Players.DVD.Modules.mShuttling
Imports SMT.Multimedia.Formats.DVD.IFO
Imports SMT.Multimedia.Formats.DVD.VOB
Imports SMT.Multimedia.Formats.Line21
Imports SMT.Multimedia.Formats.StillImage
Imports SMT.Multimedia.Players
Imports SMT.Multimedia.Enums
Imports SMT.Multimedia.Modules
Imports SMT.Multimedia.Utility
Imports SMT.Multimedia.Resources
Imports SMT.Multimedia.Utility.Timecode
Imports SMT.Multimedia.Utility.Timecode.Enums
Imports SMT.Utilities
Imports SMT.DotNet.Utility
Imports SMT.Utilities.DriveMapping
Imports SMT.Common.Utilities.ExtensionMapping
Imports SMT.Win32
Imports System.Drawing
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms
Imports System.Xml.Serialization
Imports SMT.Common.Interop.COM.Interfaces
Imports System.Runtime.InteropServices.ComTypes
Imports SMT.DotNet.Reflection
Imports SMT.Win.Globalization
Imports SMT.Win.COM.Persist
Imports SMT.Win.COM.Interfaces
Imports SMT.Multimedia.Players.DVD
Imports SMT.Multimedia.Filters.MainConcept
Imports SMT.DotNet.AppConsole

#End Region 'IMPORTS

Namespace Multimedia.Players.DVD

    Public Class cDVDPlayer
        Inherits cBasePlayer

#Region "CONSTRUCTOR/DESTRUCTOR"

        Public Sub New(ByRef nParentForm As cSMTForm, ByVal nAVMode As eAVMode, ByVal nIntensityScaling As eScalingMode, ByVal nIntensitySignalResolution As eVideoResolution, ByVal NotifyOnSeamlessCell As Boolean)

            If Not FilterCheck_shr() = "True" Then Throw New Exception("Filter verification failed.")

            AVMode = nAVMode
            IntensityScaling = nIntensityScaling
            IntensitySignalResolution = nIntensitySignalResolution
            NonSeamlessCellNotification = NotifyOnSeamlessCell

            ParentForm = nParentForm 'New cSMTForm(nParentForm)
            Me.BtnQ = New Queue

            ''setup delegate for invoking SetOSD
            'm_PassSetOSD = AddressOf SetCorruptVideoNotificationBitmap
            'm_PassClearOSD = AddressOf ClearOSD
            m_PassVideoIsCorrupted = AddressOf RaiseVideoIsCorruptedEvent

            'Dim Assm As System.Reflection.Assembly = Me.GetType.Assembly
            'Dim CountriesCSV As Stream = Assm.GetManifestResourceStream("SMT.Common.Countries.csv")
            'Dim LanguagesCSV As Stream = Assm.GetManifestResourceStream("SMT.Common.Languages.csv")

            Me.Countries = New cCountries()
            Me.Languages = New cLanguages()

            'PersistentSettings = New cDVDPlayerPersistentSettings
            'LoadPersistentSettings()

            Me.GuideInfo = New cGuidePlacementInfo

            Me.VideoFrameReceived_Timer = New System.Windows.Forms.Timer
            Me.VideoFrameReceived_Timer.Interval = 100
            Me.VideoFrameReceived_Timer.Start()

            'Me.AudioFrameReceived_Timer = NewSystem.Windows.Forms.Timer
            'Me.AudioFrameReceived_Timer.Interval = 64
            'Me.AudioFrameReceived_Timer.Start()

        End Sub

        Public Overrides Sub Dispose()
            Try
                _IsEjecting = True

                If SystemJacketPictureTimer IsNot Nothing Then
                    SystemJacketPictureTimer.Stop()
                    SystemJacketPictureTimer.Dispose()
                End If

                If PlayState = ePlayState.FrameStepping Then Me.QuitFrameStepping()
                If PlayState = ePlayState.VariSpeed Then DeactivateVarispeed()

                If Not Graph Is Nothing AndAlso Not Graph.KO_IKeystone Is Nothing Then
                    Graph.KO_IKeystone.Pause(0)
                End If

                PlayState = ePlayState.Ejecting
                _Disposed = True


                'ONE_SECOND_TIMER.Stop()
                'ONE_SECOND_TIMER.Dispose()
                'ONE_SECOND_TIMER = Nothing

                If Not VSETimer Is Nothing Then
                    VSETimer.Stop()
                    VSETimer = Nothing
                End If

                If Not VideoFrameReceived_Timer Is Nothing Then
                    VideoFrameReceived_Timer.Stop()
                    VideoFrameReceived_Timer.Dispose()
                    VideoFrameReceived_Timer = Nothing
                End If

                'If Not AudioFrameReceived_Timer Is Nothing Then
                '    AudioFrameReceived_Timer.Stop()
                '    AudioFrameReceived_Timer.Dispose()
                '    AudioFrameReceived_Timer = Nothing
                'End If

                If Not Graph.DestroyGraph() Then Throw New Exception("Failed to destroy graph.")
                If Not ClosedCaptionLogFile Is Nothing Then
                    Me.ClosedCaptionLogWriter.Close()
                    ClosedCaptionLogFile.Close()
                    ClosedCaptionLogFile = Nothing
                End If
                Me.CloseAudDumpFile()

                Graph.CleanUpROT()

                If Not Me.Viewer_WF Is Nothing Then
                    Viewer_WF.ForceClose = True
                    Viewer_WF.Close()
                    Viewer_WF.Dispose()
                    Viewer_WF = Nothing
                End If

                If Not Me.Viewer_WPF Is Nothing Then
                    'Viewer_WPF.ForceClose = True
                    Viewer_WPF.Close()
                    'Viewer_WPF.Dispose()
                    Viewer_WPF = Nothing
                End If

                'Thread.Sleep(500)

                RaiseEvent evProjectEjected(DoubleStopDispose)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with EjectProject(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'CONSTRUCTOR/DESTRUCTOR

#Region "API"

#Region "API:EVENTS"

        'ADMIN
        Public Event evPlayerInitialized()
        Public Event evSystemJacketPictureDisplayed()
        Public Event evWrongRegion(ByVal PlayerRegion As Byte, ByVal ProjectRegions() As Boolean)
        Public Event evKeyStrike(ByVal e As KeyEventArgs)
        Public Event evKeyStrikeWPF(ByVal e As Windows.Input.KeyEventArgs)
        Public Event evSetupViewer()
        Public Event evKeystoneTimeout()

        'MACRO
        Public Event evMacroItemExecuted(ByVal Command As eMacroCommand, ByVal ExtendedData As Long)
        Public Event evMacroCompleted()

        'CONSOLE
        Public Event evConsoleLine(ByVal Type As eConsoleItemType, ByVal Msg As String, ByVal ExtendedInfo As String, ByVal GOPTC As cTimecode)
        Public Event evClearConsole()

        'CONTENT META
        Public Event evRegionMask_SET(ByVal Value() As Boolean)
        Public Event evVTSCount_SET(ByVal Value As Short)
        Public Event evGlobalTitleCount_SET(ByVal Value As Short)
        Public Event evLayerbreakSet(ByVal Location As cNonSeamlessCell)
        Public Event evJacketPicturesFound(ByVal JacketPicturePaths() As String)
        Public Event evProjectHasCallSSRSM127()
        Public Event evBarDataChanged()
        Public Event evBarDataTooDark()

        'PLAY LOCATION
        Public Event evDomainChange(ByVal NewDomain As DvdDomain)
        Public Event evVTSChange(ByVal NewVTS As Byte)
        Public Event evTitleChange(ByVal NewTitle As Short)
        Public Event evPGCChange(ByVal NewPGC As Short)
        Public Event evChapterChange(ByVal NewChapterNumber As Byte)
        Public Event evProgramChange(ByVal NewPG As Short)
        Public Event evCellChange(ByVal NewCell As Short)
        Public Event evDoubleStopPlay_UOP2_Prohibited()

        'PLAY LOCATION FOR LOGGING
        Public Event evLOG_DomainChange(ByVal NewDomain As DvdDomain)
        Public Event evLOG_VTSChange(ByVal NewVTS As Byte)
        Public Event evLOG_PGCChange(ByVal NewPGC As UInt16)
        Public Event evLOG_ProgramChange(ByVal NewPG As UInt16)
        Public Event evLOG_CellChange(ByVal NewCell As UInt16)
        Public Event evLOG_TitleChange(ByVal NewTitle As UInt16)
        Public Event evLOG_ChapterChange(ByVal NewChapterNumber As UInt16)

        'PLAY STATE
        Public Event evPlaybackStarted()
        Public Event evPlaybackStopped(ByVal Success As Boolean)
        Public Event evProjectEjected(ByVal FromDoubleStopState As Boolean)
        Public Event evPlaybackPaused(ByVal Paused As Boolean)
        Public Event evFastForward(ByVal Speed As Double)
        Public Event evRewind(ByVal Speed As Double)
        Public Event evRunningTimeTick()
        Public Event evDVDPlaybackRateReturnedToOneX()
        Public Event evCorruptedVideoData(ByVal VETSC As sTransferErrorTime)
        Public Event evUserOperationsChanged(ByVal NewUOPMask As Integer)
        Public Event evCellStillTime(ByVal Time As Integer)
        Public Event evTripleStop()
        Public Event evFrameDropped()
        Public Event evGPRMChanged(ByVal Number As Byte, ByVal Value As UInt16)
        Public Event evSPRMChanged(ByVal Number As Byte, ByVal Value As UInt16)
        Public Event evBeginNavCommands(ByVal Source As eNavCmdType, ByVal DetailedPlayLocation As cDetailedPlayLocation)
        Public Event evNavCommand(ByVal Cmd As cCMD)
        Public Event evVOBUOffset(ByVal Offset As Integer, ByVal VTSN As Integer)
        Public Event evVOBUTimestamp(ByVal Timestamp As UInt64)
        Public Event evABLoopCleared()

        'STREAM SURFING
        Public Event evAudioCycled()
        Public Event evAudioStreamSet(ByVal StreamNumber As Short)
        Public Event evAudioStreamChanged(ByVal StreamNumber As Byte, ByVal NumberOfStreams As Byte)
        'Public Event evSubtitlesCycled()
        'Public Event evSubtitlesToggled(ByVal TurnedOn As Boolean)
        Public Event evSubtitleStreamChanged(ByVal NewStreamNumber As Byte)
        Public Event evSubtitleStreamSet(ByVal StreamNumber As Byte)
        Public Event evAngleCycled(ByVal NewAngle As Byte)
        Public Event evAngleChanged(ByVal NewAngle As Byte)
        Public Event evClosedCaptionToggle()

        'STREAM META
        Public Event evLetterboxAllowed_SET(ByVal Value As Boolean)
        Public Event evPanscanAllowed_SET(ByVal Value As Boolean)
        Public Event evCurrentBitrateNotification(ByVal CurrentBitrate As Integer)
        Public Event evFrameRateChange(ByVal NewRate As Double)
        Public Event evKEYSTONE_Interlacing(ByVal Interlaced As Boolean)
        Public Event evKEYSTONE_FieldOrder(ByVal TopFirst As Boolean)
        Public Event evKEYSTONE_ProgressiveSequence(ByVal ProgressiveSequence As Boolean)
        Public Event evMPEG_Timecode(ByVal TC As cTimecode)
        Public Event evMacroVisionLevel(ByVal Level As Byte)
        Public Event evMacrovisionStatus(ByVal Status As String)

#End Region 'API:EVENTS

#Region "API:PROPERTIES"

#Region "API:PROPERTIES:GENERAL"

        Public CurrentDVD As cDVD
        Public IsInitialized As Boolean = False

#Region "DVDDirectory"

        Public ReadOnly Property DVDDirectory() As String
            Get
                Try
                    If _DVDDirectory = "" Then
                        Dim TryCnt As Short = 1
TryAgain:
                        If Not EXGetDVDDirectory() Then
                            TryCnt += 1
                            If TryCnt = 25 Then
                                Throw New Exception("GetDVDDirectory failure.")
                            Else
                                GoTo TryAgain
                            End If
                        End If
                    End If
                    Return _DVDDirectory
                Catch ex As Exception
                    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with GetDVDDirectory(). Error: " & ex.Message, Nothing, Nothing)
                    Return Nothing
                End Try
            End Get
        End Property
        Private _DVDDirectory As String = ""

        Public Function EXGetDVDDirectory() As Boolean
            Try
                Dim BufferSize As Integer
                Dim b(2047) As Byte
                Dim handle As GCHandle = GCHandle.Alloc(b, GCHandleType.Pinned)
                Dim ptrBuffer As IntPtr = handle.AddrOfPinnedObject()
                Dim hr As Integer = Graph.DVDInfo.GetDVDDirectory(ptrBuffer, 2048, BufferSize)
                If hr < 0 Then Marshal.ThrowExceptionForHR(hr)
                _DVDDirectory = ConvertStringFromUnicodeRemoveExtraBytes(b)
                handle.Free()
                Return True
            Catch ex As Exception
                'RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem getting dvd directory. error: " & ex.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

        Public Function IsPathValidDVDDirectory(ByVal TestPath As String) As Boolean
            Try
                If Microsoft.VisualBasic.Right(TestPath, 1) = "\" Then
                    TestPath &= "Video_TS.ifo"
                Else
                    TestPath &= "\Video_TS.ifo"
                End If
                'TestPath = Replace(TestPath, "\\", "\")
                'GetDVDDirectory()
                If InStr(TestPath.ToLower, "sysjp") Then
                    TestPath = "d:\Video_TS\Video_TS.ifo"
                    If Not File.Exists(TestPath) Then
                        Return False
                    End If
                ElseIf Not File.Exists(TestPath) Then
                    Return False
                End If
                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with IsDVDDirectoryValid(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

#End Region 'DVDDirectory

#End Region 'API:PROPERTIES:GENERAL

#Region "API:PROPERTIES:PLAY STATE"

        Public ReadOnly Property StartingDVD() As Boolean
            Get
                Return _StartingDVD
            End Get
        End Property
        Private _StartingDVD As Boolean = False

        Public ReadOnly Property IsEjecting() As Boolean
            Get
                Return _IsEjecting
            End Get
        End Property
        Private _IsEjecting As Boolean = False

        Public ReadOnly Property Disposed() As Boolean
            Get
                Return _Disposed
            End Get
        End Property
        Private _Disposed As Boolean = False

        Public ReadOnly Property ClosedCaptionsAreOn() As Boolean
            Get
                Return _ClosedCaptionsAreOn
            End Get
        End Property
        Private _ClosedCaptionsAreOn As Boolean = False

        Public ReadOnly Property CurrentSpeed() As Double
            Get
                Return _CurrentSpeed
            End Get
        End Property
        Private _CurrentSpeed As Double = OneX

        Public ReadOnly Property DVDIsInStill() As Boolean
            Get
                Return _DVDIsInStill
            End Get
        End Property
        Private _DVDIsInStill As Boolean = False

        Public ReadOnly Property CurrentAngleStream() As Integer
            Get
                Return _CurrentAngleStream
            End Get
        End Property
        Private _CurrentAngleStream As Integer = 1

        Public ReadOnly Property CurrentTitleAngleCount() As Byte
            Get
                Return _CurrentTitleAngleCount
            End Get
        End Property
        Private _CurrentTitleAngleCount = 1

        Public Property CurrentVTS() As Byte
            Get
                Return _CurrentVTS
            End Get
            Set(ByVal value As Byte)
                If _CurrentVTS <> value Then RaiseEvent evLOG_VTSChange(value)
                _CurrentVTS = value
                RaiseEvent evVTSChange(value)
            End Set
        End Property
        Private _CurrentVTS As Byte

        Public Property CurrentCell() As Short
            Get
                Return _CurrentCell
            End Get
            Set(ByVal value As Short)
                If _CurrentCell <> value Then RaiseEvent evLOG_CellChange(value)
                _CurrentCell = value
                RaiseEvent evCellChange(value)
            End Set
        End Property
        Private _CurrentCell As Short

        Public Property CurrentPG() As Short
            Get
                Return _CurrentPG
            End Get
            Set(ByVal value As Short)
                If _CurrentPG <> value Then RaiseEvent evLOG_ProgramChange(value)
                _CurrentPG = value
                RaiseEvent evProgramChange(value)
            End Set
        End Property
        Private _CurrentPG As Short

        Public Property CurrentPGC() As Short
            Get
                Return _CurrentPGC
            End Get
            Set(ByVal value As Short)
                If _CurrentPGC <> value Then RaiseEvent evLOG_PGCChange(value)
                _CurrentPGC = value
                RaiseEvent evPGCChange(_CurrentPGC)
            End Set
        End Property
        Private _CurrentPGC As Short

        Public ReadOnly Property CurrentDomain() As DvdDomain
            Get
                If Graph Is Nothing Then Return 0
                Graph.DVDInfo.GetCurrentDomain(_CurrentDomain)
                Return _CurrentDomain
            End Get
        End Property
        Public _CurrentDomain As DvdDomain

        ''' <summary>
        ''' This CurrentDomain returns the value of the last received EC_DVD_DOMAIN_CHANGE
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property CurrentDomain_Events() As DvdDomain
            Get
                If Graph Is Nothing Then Return DvdDomain.Nav_Not_Initialized
                Return _CurrentDomain_Events
            End Get
            Set(ByVal value As DvdDomain)
                If _CurrentDomain_Events <> value Then RaiseEvent evLOG_DomainChange(value)
                _CurrentDomain_Events = value
            End Set
        End Property
        Public _CurrentDomain_Events As DvdDomain = DvdDomain.Nav_Not_Initialized

        Public ReadOnly Property CurrentPlayLocation() As DvdPlayLocation
            Get
                Try
                    Dim loc As New DvdPlayLocation
                    If Graph.DVDInfo Is Nothing Then Exit Property
                    HR = Graph.DVDInfo.GetCurrentLocation(loc)
                    If HR = -2147220873 Then
                        'menu domain
                        loc.TitleNum = -1
                        Return loc
                    End If
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    Return loc
                Catch ex As Exception
                    'Debug.WriteLine("Problem with GetDVDPlayLocation. Error: " & ex.Message)
                    Return New DvdPlayLocation
                End Try
            End Get
        End Property

        Public ReadOnly Property MenuMode() As eMenuMode
            Get
                Return _MenuMode
            End Get
        End Property
        Private _MenuMode As eMenuMode

        Public ReadOnly Property FullScreen() As Boolean
            Get
                Return _FullScreen
            End Get
        End Property
        Private _FullScreen As Boolean

        Public ReadOnly Property CurrentRunningTime_DVD() As DvdTimeCode
            Get
                Return _CurrentRunningTime
            End Get
        End Property
        Private _CurrentRunningTime As DvdTimeCode

        Public Overrides ReadOnly Property CurrentRunningTime_InSeconds() As UInt32
            Get
                Return (CurrentRunningTime_DVD.bHours * 3600) + (CurrentRunningTime_DVD.bMinutes * 60) + CurrentRunningTime_DVD.bSeconds
            End Get
        End Property

        Public ReadOnly Property CurrentTitle() As Integer
            Get
                Try
                    Dim loc As DvdPlayLocation
                    HR = Graph.DVDInfo.GetCurrentLocation(loc)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    Return loc.TitleNum
                Catch ex As Exception
                    If InStr(ex.Message, "80040277") Then
                        Return -2 'VFW_E_DVD_INVALIDDOMAIN
                    Else
                        Throw New Exception("Problem with CurrentTitle(). Error: " & ex.Message, ex)
                    End If
                End Try
            End Get
        End Property

        Public ReadOnly Property CurrentChapter() As Integer
            Get
                Return _CurrentChapter
            End Get
        End Property
        Private _CurrentChapter As Integer

        Public ReadOnly Property CurrentPlaybackRate() As Integer
            Get
                Return _CurrentPlaybackRate
            End Get
        End Property
        Private _CurrentPlaybackRate As Integer = 10000

        Public ReadOnly Property ChaptersInCurrentTitle() As Short
            Get
                Return _ChaptersInCurrentTitle
            End Get
        End Property
        Private _ChaptersInCurrentTitle As Short

        Public Property LastGOPTC() As cTimecode
            Get
                Return _LastGOPTC
            End Get
            Set(ByVal value As cTimecode)
                _LastGOPTC = value
            End Set
        End Property
        Private _LastGOPTC As cTimecode

        Public ReadOnly Property LastGOPTC_Ticks() As Long
            Get
                Return _LastGOPTC_Ticks
            End Get
        End Property
        Private _LastGOPTC_Ticks

        Public ReadOnly Property CurrentGlobalTT() As cGlobalTT
            Get
                Return CurrentDVD.VMGM.GlobalTTs(CurrentTitle - 1)
            End Get
        End Property

        Public ReadOnly Property CurrentGPRMs() As Integer()
            Get
                Try
                    If PlayState <> ePlayState.Playing And PlayState <> ePlayState.Stopped Then
                        Dim i() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
                        Return i
                    Else
                        Dim I(15) As Integer
                        HR = Graph.DVDInfo.GetAllGPRMs(I)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        Return I
                    End If
                Catch ex As Exception
                    Throw New Exception("Problem with CurrentGPRMs property. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property CurrentSPRMs() As Integer()
            Get
                Try
                    If PlayState <> ePlayState.Playing Then
                        Dim o() As Integer = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
                        Return o
                    Else
                        Dim I(11) As Integer
                        HR = Graph.DVDInfo.GetAllSPRMs(I)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        Return I
                    End If
                Catch ex As Exception
                    Throw New Exception("Problem with CurrentSPRMs property. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property CurrentSPRMs_evt() As UInt16()
            Get
                Return _CurrentSPRMs_evt
            End Get
        End Property
        Private _CurrentSPRMs_evt(23) As UInt16

        Public ReadOnly Property CurrentGPRMs_evt() As UInt16()
            Get
                Return _CurrentGPRMs_evt
            End Get
        End Property
        Private _CurrentGPRMs_evt(15) As UInt16

        Public ReadOnly Property CurrentlySelectedButton() As Integer
            Get
                Try
                    Return CurrentSPRMs(7) / 1024
                Catch ex As Exception
                    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with GetCurrentlySelectedBtn. Error: " & ex.Message, Nothing, Nothing)
                End Try
            End Get
        End Property

        Public ReadOnly Property CurrentAudioLanguage() As String
            Get
                'TODO: IMPORTANT
            End Get
        End Property

        Public ReadOnly Property CurrentAudioExtension() As String
            Get
                Dim att As DvdAudioAttr
                Graph.DVDInfo.GetAudioAttributes(CurrentAudioStreamNumber, att)
                Return att.audioFormat.ToString
            End Get
        End Property

        Public ReadOnly Property CurrentAudioStreamNumber() As Integer
            Get
                Try
                    Dim StreamCount, CurrentStream As Integer
                    HR = Graph.DVDInfo.GetCurrentAudio(StreamCount, CurrentStream)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    Return CurrentStream
                Catch ex As Exception
                    Throw New Exception("Problem with CurrentAudioStreamNumber(). Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property CurrentAudioStreamCount() As Integer
            Get
                Try
                    If Me.CurrentDomain = DvdDomain.Title Then
                        Dim StreamCount, CurrentStream As Integer
                        HR = Graph.DVDInfo.GetCurrentAudio(StreamCount, CurrentStream)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        Return StreamCount
                    Else
                        'this was added 080324 because 2+ audio streams is only supported in title space
                        Return 1
                    End If
                Catch ex As Exception
                    Throw New Exception("Problem with CurrentAudioStreamCount(). Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property LastTimeSearch() As DvdTimeCode
            Get
                Return _LastTimeSearch
            End Get
        End Property
        Private _LastTimeSearch As DvdTimeCode

        Public ReadOnly Property CurrentSubtitleStreamNumber() As Integer
            Get
                Dim CurrentStream, StreamCount As Integer
                Dim Enabled As Boolean = False
                HR = Graph.DVDInfo.GetCurrentSubpicture(StreamCount, CurrentStream, Enabled)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return CurrentStream
            End Get
        End Property

        'Public ReadOnly Property CurrentSubtitleStreamEnabled() As Boolean
        '    Get
        '        Dim CurrentStream, StreamCount As Integer
        '        Dim Enabled As Boolean = False
        '        HR = Graph.DVDInfo.GetCurrentSubpicture(StreamCount, CurrentStream, Enabled)
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '        Return Enabled
        '    End Get
        'End Property

        Public ReadOnly Property SubtitlesAreOn() As Boolean
            Get
                Dim CurrentStream, StreamCount As Integer
                Dim Disabled As Boolean = False
                HR = Graph.DVDInfo.GetCurrentSubpicture(StreamCount, CurrentStream, Disabled)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return Not Disabled
                'Return _SubtitlesAreOn
            End Get
        End Property
        'Private _SubtitlesAreOn As Boolean = False

        Public ReadOnly Property CurrentUserOperations() As Boolean()
            Get
                Return _UOPs
            End Get
        End Property
        Private _UOPs() As Boolean

        Public ReadOnly Property IsAudioStreamAvailable(ByVal StreamNumber As Short) As Boolean
            Get
                Try
                    If StreamNumber > 7 Then Return True
                    Dim out As Boolean
                    Graph.DVDInfo.IsAudioStreamEnabled(StreamNumber, out)
                    Return out

                    '    'IFO CHECK TO SEE IF ALL STREAMS ARE AVAILABLE
                    '    'ADDED IN RESPONSE TO A REQUEST BY TCS
                    '    'TRPF 070421
                    '    Select Case CurrentDomain
                    '        Case DvdDomain.Stop
                    '            Return False
                    '        Case DvdDomain.Title
                    '            Return CurrentDVD.VTSs(CurrentVTS - 1).PGCs(CurrentPGC - 1).PGC_AST_CTL.PGC_AST_CTLs(StreamNumber).Available
                    '        Case DvdDomain.VideoManagerMenu
                    '            Return True
                    '        Case DvdDomain.VideoTitleSetMenu
                    '            Return True
                    '    End Select
                Catch ex As Exception
                    Throw New Exception("Problem with IsAudioStreamAvailable(). Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property VideoIsRunning() As Boolean
            Get
                'Debug.WriteLine("VIR Delta: " & Me.VideoFrameReceived_Delta)
                Return VideoFrameReceived_Delta < 500
            End Get
        End Property

        Public ReadOnly Property CurrentMenuLanguageSet_TwoChar() As String
            Get
                'this is going to be a guess, should usually be correct, because the nav does not tell us which set is being used.
                'the logic is that if the current preferred menu language is not available in the menu sets, then the 0 set is used

                '1) get the desired menu lang two string
                Dim ML As cLanguage = Languages.GetLanguageByName(Me.Users_NavigatorSetup.DEFAULT_MENU_LANGUAGE.ToString)
                Dim DML_SS As String = GetLanguageTwoCharByLCID(ML.DecimalValue)

                '2) get the available menu language two chars
                If Me.MenuLanguages_TwoChar.IndexOf(DML_SS) > -1 Then
                    'it exists
                    Return DML_SS
                Else
                    'it does not exist, return the default
                    Dim out As String = ""
                    If Me.MenuLanguages_TwoChar.Count > 0 Then out = Me.MenuLanguages_TwoChar(0)
                    Return out
                End If
            End Get
        End Property

        'Public ReadOnly Property ResumeDataExists() As Boolean
        '    Get
        '        Return _ResumeDataExists
        '    End Get
        'End Property
        'Private _ResumeDataExists As Boolean = False

        Public ReadOnly Property ABLoop_PositionA() As DvdPlayLocation
            Get
                Return Me._ABLoop_PositionA
            End Get
        End Property
        Private _ABLoop_PositionA As DvdPlayLocation

        Public ReadOnly Property ABLoop_PositionB() As DvdPlayLocation
            Get
                Return Me._ABLoop_PositionB
            End Get
        End Property
        Private _ABLoop_PositionB As DvdPlayLocation

        Public ReadOnly Property CurrentDetailedPlayLocation() As cDetailedPlayLocation
            Get
                Dim out As New cDetailedPlayLocation
                out.Cell = CurrentCell
                out.GTTN = CurrentTitle
                out.LanguageUnit = "HelpGlenn!" 'currently not supported. waiting for help from glenn.
                out.PGCN = CurrentPGC
                out.PGN = CurrentPG
                out.PTTN = CurrentChapter
                out.VTS_TTN = Nothing 'currently not supported. just need to lookup in CurrentDVD
                out.VTSN = CurrentVTS
                'out.InVMGM = (CurrentDomain_Events = DvdDomain.VideoManagerMenu)
                'out.InVTS_MenuSpace = (CurrentDomain_Events = DvdDomain.VideoTitleSetMenu)
                'out.InVTS_TitleSpace = (CurrentDomain_Events = DvdDomain.Title)
                out.Domain = CurrentDomain_Events
                Return out
            End Get
        End Property

#End Region 'API:PROPERTIES:PLAY STATE

#Region "API:PROPERTIES:CONTENT META DATA"

        Public ReadOnly Property CurrentVideoStandard() As eVideoStandard
            Get
                Dim rKeyV As RegistryKey = Registry.LocalMachine.OpenSubKey("Software\NVIDIA Corporation\Filters\Video", False)
                Dim o As Object = rKeyV.GetValue("ForcePALConnection")
                rKeyV.Close()
                If CInt(o) = 0 Then
                    'ntsc
                    Return eVideoStandard.NTSC
                Else
                    'pal
                    Return eVideoStandard.PAL
                End If
                'Return _CurrentVideoStandard
            End Get
        End Property
        'Private _CurrentVideoStandard As eVideoStandard

        Public Shadows ReadOnly Property CurrentVideoDimensions() As Point
            Get
                Return _CurrentVideoDimensions
            End Get
        End Property
        Private _CurrentVideoDimensions As Point

        Public ReadOnly Property CurrentAspectRatio() As eAspectRatio
            Get
                Return _CurrentAspectRatio
            End Get
        End Property
        Private _CurrentAspectRatio As eAspectRatio

        Public Property VTSCount() As Short
            Get
                Return _VTSCount
            End Get
            Set(ByVal value As Short)
                _VTSCount = value
                RaiseEvent evVTSCount_SET(value)
            End Set
        End Property
        Private _VTSCount As Short

        ''' <summary>
        ''' Total number of global titles in current project.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property GlobalTTCount() As Short
            Get
                Return _GlobalTTCount
            End Get
            Set(ByVal value As Short)
                _GlobalTTCount = value
                RaiseEvent evGlobalTitleCount_SET(value)
            End Set
        End Property
        Private _GlobalTTCount As Short

        Public Property CurrentTitleTRT() As DvdTimeCode
            Get
                Return _CurrentTitleTRT
            End Get
            Set(ByVal value As DvdTimeCode)
                _CurrentTitleTRT = value
            End Set
        End Property
        Private _CurrentTitleTRT As DvdTimeCode

        Public ReadOnly Property CurrentTitleDuration() As cTitleDuration
            Get
                Try
                    Dim out As New cTitleDuration
                    Dim i As Integer
                    HR = Graph.DVDInfo.GetTotalTitleTime(out.TotalTime, i)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    If i And &H1 Then
                        out.FrameRate = "25fps"
                    End If
                    If i And &H2 Then
                        out.FrameRate = "30fps"
                    End If
                    If i And &H4 Then
                        out.DropFrame = True
                        out.FrameRate = "29.97fps"
                    End If
                    If i And &H8 Then
                        out.Interpolated = True
                    End If
                    Return out
                Catch ex As Exception
                    If InStr(ex.Message, "80040277") Then Return Nothing
                    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with GetTitleDuration. Error: " & ex.Message, Nothing, Nothing)
                    Return Nothing
                End Try
            End Get
        End Property

        Public ReadOnly Property CurrentTitleDurationInSeconds() As Short
            Get
                Dim trt As cTitleDuration = CurrentTitleDuration
                Return (trt.TotalTime.bHours * 3600) + (trt.TotalTime.bMinutes * 60) + trt.TotalTime.bSeconds
            End Get
        End Property

        Public Property Interlaced() As Boolean
            Get
                Return _Interlaced
            End Get
            Set(ByVal value As Boolean)
                _Interlaced = value
            End Set
        End Property
        Private _Interlaced As Boolean

        Public Property TopFieldFirst() As Boolean
            Get
                Return _TopFieldFirst
            End Get
            Set(ByVal value As Boolean)
                _TopFieldFirst = value
                RaiseEvent evKEYSTONE_FieldOrder(value)
            End Set
        End Property
        Private _TopFieldFirst As Boolean

        Public Property ProgressiveSequence() As Boolean
            Get
                Return _ProgressiveSequence
            End Get
            Set(ByVal value As Boolean)
                _ProgressiveSequence = value
                RaiseEvent evKEYSTONE_ProgressiveSequence(value)
            End Set
        End Property
        Private _ProgressiveSequence As Boolean

        Public Property MacrovisionLevel() As Byte
            Get
                Return _MacrovisionLevel
            End Get
            Set(ByVal value As Byte)
                _MacrovisionLevel = value
                RaiseEvent evMacroVisionLevel(value)
            End Set
        End Property
        Private _MacrovisionLevel As Byte = 0

        Public Property MacrovisionStatus() As String
            Get
                Return _MacrovisionStatus
            End Get
            Set(ByVal value As String)
                _MacrovisionStatus = value
                RaiseEvent evMacrovisionStatus(value)
            End Set
        End Property
        Private _MacrovisionStatus As String

        Public ReadOnly Property CurrentTargetFrameRate() As Byte
            Get
                If Me.CurrentVideoStandard = eVideoStandard.NTSC Then Return 30
                Return 25
            End Get
        End Property

        Public Property LB_OK() As Boolean
            Get
                Return _LB_OK
            End Get
            Set(ByVal Value As Boolean)
                If Value <> _LB_OK Then
                    _LB_OK = Value
                    Graph.KO_IKeystoneMixer.SetARFlags(BoolToByte(PS_OK), BoolToByte(Value))
                End If
                RaiseEvent evLetterboxAllowed_SET(Value)
            End Set
        End Property
        Private _LB_OK As Boolean = True

        Public Property PS_OK() As Boolean
            Get
                Return _PS_OK
            End Get
            Set(ByVal Value As Boolean)
                If Value <> _PS_OK Then
                    _PS_OK = Value
                    Graph.KO_IKeystoneMixer.SetARFlags(BoolToByte(Value), BoolToByte(LB_OK))
                End If
                RaiseEvent evPanscanAllowed_SET(Value)
            End Set
        End Property
        Private _PS_OK As Boolean = True

        Public ReadOnly Property TotalFrameCount() As Long
            Get
                Dim TC As DvdTimeCode
                Dim i As Integer = 0
                HR = Graph.DVDInfo.GetTotalTitleTime(TC, i)
                If HR < 0 Then
                    If HR = -2147220873 Then                'it's a white hand for trying to get duration of an infinite still, infinity is a big number
                        Return 0
                    End If
                    Marshal.ThrowExceptionForHR(HR)
                End If
                Return DVDTimecodeToFrameCount(TC, CurrentVideoStandard.ToString)
            End Get
        End Property

        Public ReadOnly Property VolumeCount() As Integer
            Get
                Return _VolumeCount
            End Get
        End Property
        Private _VolumeCount As Integer

        Public ReadOnly Property CurrentVolume() As Integer
            Get
                Return _CurrentVolume
            End Get
        End Property
        Private _CurrentVolume As Integer

        Public ReadOnly Property DiscSide() As DvdDiscSide
            Get
                Return _DiscSide
            End Get
        End Property
        Private _DiscSide As DvdDiscSide

        Public ReadOnly Property MenuLanguages_TwoChar() As List(Of String)
            Get
                Try
                    Dim out As New List(Of String)
                    For Each i As Integer In MenuLanguages_Integers
                        If i <> 0 Then
                            out.Add(GetLanguageTwoCharByLCID(i))
                        End If
                    Next
                    Return out
                Catch ex As Exception
                    Debug.WriteLine("Problem in MenuLanguages_Strings(). Error: " & ex.Message)
                    'RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "GetMenuLanguages failed. Error: " & ex.Message, Nothing, Nothing)
                End Try
            End Get
        End Property

        Public ReadOnly Property MenuLanguages_Strings() As List(Of String)
            Get
                Try
                    Dim out As New List(Of String)
                    For Each i As Integer In MenuLanguages_Integers
                        If i <> 0 Then
                            out.Add(GetLanguageByLCID(i))
                        End If
                    Next
                    Return out
                Catch ex As Exception
                    Debug.WriteLine("Problem in MenuLanguages_Strings(). Error: " & ex.Message)
                    'RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "GetMenuLanguages failed. Error: " & ex.Message, Nothing, Nothing)
                End Try
            End Get
        End Property

        Public ReadOnly Property MenuLanguages_Integers() As Integer()
            Get
                ReDim _MenuLanguages_Integers(14)
                HR = Graph.DVDInfo.GetMenuLanguages(_MenuLanguages_Integers, 15, _MenuLanguage_Count)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return Me._MenuLanguages_Integers
            End Get
        End Property
        Private _MenuLanguages_Integers() As Integer = Nothing

        Public ReadOnly Property MenuLanguage_Count() As Integer
            Get
                Return _MenuLanguage_Count
            End Get
        End Property
        Private _MenuLanguage_Count As Integer = 0

        Public ReadOnly Property DefaultMenuLanguage_Integer() As Integer
            Get
                If Me._DefaultMenuLanguage_Integer = 0 Then
                    HR = Graph.DVDInfo.GetDefaultMenuLanguage(Me._DefaultMenuLanguage_Integer)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End If

                Return _DefaultMenuLanguage_Integer
            End Get
        End Property
        Private _DefaultMenuLanguage_Integer As Integer = 0

        Public ReadOnly Property DefaultMenuLanguage_TwoChar() As String
            Get
                Return GetLanguageTwoCharByLCID(DefaultMenuLanguage_Integer)
            End Get
        End Property

        'lcid to two char

        Public ReadOnly Property DVDText() As String
            Get
                Return GetDVDText()
            End Get
        End Property

        Public ReadOnly Property VideoEncrypted() As Boolean
            Get
                Dim i As Integer
                Graph.nvVideoAtts.GetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_STATS, nvcommon.ENvVideoDecoderProps_StatsTypeIndexes.NVVIDDEC_STATS_ENCRYPTED, i)
                If i > 0 Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        Public Property Layerbreak() As cNonSeamlessCell
            Get
                Return _Layerbreak
            End Get
            Set(ByVal Value As cNonSeamlessCell)
                _Layerbreak = Value
                RaiseEvent evLayerbreakSet(Value)
            End Set
        End Property
        Private _Layerbreak As cNonSeamlessCell

        Public ReadOnly Property NonSeamlessCells() As colNonSeamlessCells
            Get
                Return _NonSeamlessCells
            End Get
        End Property
        Private _NonSeamlessCells As colNonSeamlessCells

        Public ReadOnly Property JacketPicturesAvailable() As Boolean
            Get
                Return _JacketPicturesAvailable
            End Get
        End Property
        Private _JacketPicturesAvailable As Boolean = False

        Public ReadOnly Property VolumeInfo() As sVolumeInfo
            Get
                Dim out As sVolumeInfo
                HR = Graph.DVDInfo.GetDVDVolumeInfo(out.VolumeCount, out.CurrentVolume, out.DiscSide, out.GlobalTitleCount)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return out
            End Get
        End Property

        Public ReadOnly Property SubtitleStreamCount() As Integer
            Get
                Dim StreamCount As Integer = 0
                Dim CurrentStream As Integer = 0
                Dim OnOff As Boolean = False
                HR = Graph.DVDInfo.GetCurrentSubpicture(StreamCount, CurrentStream, OnOff)
                If HR = 0 Then Marshal.ThrowExceptionForHR(HR)
                Return StreamCount
            End Get
        End Property

        Public ReadOnly Property IsProjectDualLayer(ByVal DVDDirectoryPath As String) As Boolean
            Get
                Try
                    Dim TotalSize As Long = 0
                    Dim FSE() As String = Directory.GetFiles(DVDDirectoryPath)

                    For Each F As String In FSE
                        TotalSize += FileLen(F)
                    Next

                    If TotalSize > 5051158528 Then
                        Return True
                    Else
                        Return False
                    End If
                Catch ex As Exception
                    Throw New Exception("Problem with IsProjectDualLayer. Error: " & ex.Message, ex)
                    'RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with IsProjectDualLayer. Error: " & ex.Message, Nothing, Nothing)
                End Try
            End Get
        End Property

        Public ReadOnly Property TopBar() As Integer
            Get
                Return _TopBar
            End Get
        End Property
        Private _TopBar As Integer = 0

        Public ReadOnly Property LeftBar() As Integer
            Get
                Return _LeftBar
            End Get
        End Property
        Private _LeftBar As Integer = 0

        Public ReadOnly Property RightBar() As Integer
            Get
                Return _RightBar
            End Get
        End Property
        Private _RightBar As Integer = 0

        Public ReadOnly Property BottomBar() As Integer
            Get
                Return _BottomBar
            End Get
        End Property
        Private _BottomBar As Integer = 0

        Public ReadOnly Property CurrentAudioIsAC3EX() As Boolean
            Get
                Dim ex As nvcommon.eDSUREXMOD
                Graph.nvAudioAtts.GetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_STATS, nvcommon.ENvidiaAudioDecoderProps_Stats.NVAUDDEC_STATS_AC3_DSUREXMOD, ex)
                Return ex = nvcommon.eDSUREXMOD.EX_Encoded
            End Get
        End Property

        Public ReadOnly Property AudioStreamCount() As Integer
            Get
                Dim StreamCount As Integer = 0
                Dim CurrentStream As Integer = 0
                Dim OnOff As Boolean = False
                HR = Graph.DVDInfo.GetCurrentAudio(StreamCount, CurrentStream)
                If HR = 0 Then Marshal.ThrowExceptionForHR(HR)
                Return StreamCount
            End Get
        End Property

        Public ReadOnly Property GPRM(ByVal No As Byte) As UInt16
            Get
                If No > 15 Then Throw New Exception("Illegal value.")
                Select Case No
                    Case 0
                        Return CurrentGPRMs(0) And &HFFFF
                    Case 1
                        Return (CurrentGPRMs(0) >> 16) And &HFFFF
                    Case 2
                        Return CurrentGPRMs(1) And &HFFFF
                    Case 3
                        Return (CurrentGPRMs(1) >> 16) And &HFFFF
                    Case 4
                        Return CurrentGPRMs(2) And &HFFFF
                    Case 5
                        Return (CurrentGPRMs(2) >> 16) And &HFFFF
                    Case 6
                        Return CurrentGPRMs(3) And &HFFFF
                    Case 7
                        Return (CurrentGPRMs(3) >> 16) And &HFFFF
                    Case 8
                        Return CurrentGPRMs(4) And &HFFFF
                    Case 9
                        Return (CurrentGPRMs(4) >> 16) And &HFFFF
                    Case 10
                        Return CurrentGPRMs(5) And &HFFFF
                    Case 11
                        Return (CurrentGPRMs(5) >> 16) And &HFFFF
                    Case 12
                        Return CurrentGPRMs(6) And &HFFFF
                    Case 13
                        Return (CurrentGPRMs(6) >> 16) And &HFFFF
                    Case 14
                        Return CurrentGPRMs(7) And &HFFFF
                    Case 15
                        Return (CurrentGPRMs(7) >> 16) And &HFFFF
                End Select
            End Get
        End Property

#End Region 'API:PROPERTIES:CONTENT META DATA

#Region "API:PROPERTIES:USER CONFIG"

        Public Property DumpAudio() As Boolean
            Get
                Return _DumpAudio
            End Get
            Set(ByVal Value As Boolean)
                _DumpAudio = Value
                If Value Then
                    Graph.AMTC_Interface.StoreBuffers(True)
                Else
                    Graph.AMTC_Interface.StoreBuffers(False)
                    If Not AudioDumpFile Is Nothing Then
                        AudioDumpFile.Close()
                        AudioDumpFile = Nothing
                    End If
                End If
            End Set
        End Property
        Private _DumpAudio As Boolean

        Public Property GuideInfo() As cGuidePlacementInfo
            Get
                Return _GuideInfo
            End Get
            Set(ByVal Value As cGuidePlacementInfo)
                _GuideInfo = Value
            End Set
        End Property
        Private _GuideInfo As cGuidePlacementInfo

        Public Property SPPlacementInfo() As cGuidePlacementInfo
            Get
                Return Me._SPPlacementInfo
            End Get
            Set(ByVal Value As cGuidePlacementInfo)
                Me._SPPlacementInfo = Value
            End Set
        End Property
        Private _SPPlacementInfo As cGuidePlacementInfo

        Public Property ReverseFieldOrder() As Boolean
            Get

            End Get
            Set(ByVal value As Boolean)
                Try
                    HR = Graph.KO_IKeystoneMixer.ReverseFieldOrder(value)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Catch ex As Exception
                    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with ReverseFieldOrder(). Error: " & ex.Message, Nothing, Nothing)
                End Try
            End Set
        End Property
        Private _ReversingFieldOrder As Boolean = False

        Public Property BumpingFieldsDown() As Boolean
            Get
                Return _BumpingFieldsDown
            End Get
            Set(ByVal value As Boolean)
                _BumpingFieldsDown = value
            End Set
        End Property
        Private _BumpingFieldsDown As Boolean = False

        Public Property CurrentMPEGFrameMode() As Short
            Get
                Return _CurrentMPEGFrameMode
            End Get
            Set(ByVal value As Short)
                _CurrentMPEGFrameMode = value
            End Set
        End Property
        Private _CurrentMPEGFrameMode As Short = 1

        Public Property ClosedCaptionLogging() As Boolean
            Get
                Return _ClosedCaptionLogging
            End Get
            Set(ByVal value As Boolean)
                _ClosedCaptionLogging = value
                If Not value Then
                    If Not ClosedCaptionLogFile Is Nothing Then
                        ClosedCaptionLogWriter.Close()
                        ClosedCaptionLogFile.Close()
                        ClosedCaptionLogFile = Nothing
                    End If
                End If
            End Set
        End Property
        Private _ClosedCaptionLogging As Boolean = False

        Public Property ClosedCaptionLogging_IncludeCommands() As Boolean
            Get
                Return _ClosedCaptionLogging_IncludeCommands
            End Get
            Set(ByVal value As Boolean)
                _ClosedCaptionLogging_IncludeCommands = value
            End Set
        End Property
        Private _ClosedCaptionLogging_IncludeCommands As Boolean = False

#Region "API:PROPERTIES:USER CONFIG:PLAYER DEFAULTS"

        Public ReadOnly Property DefaultAudioLanguage() As eLanguages
            Get
                Try
                    Dim L As Integer
                    Dim Ext As DvdAudioLangExt
                    HR = Graph.DVDInfo.GetDefaultAudioLanguage(L, Ext)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    If L = -1 Then
                        Return eLanguages.English
                    Else
                        Return [Enum].Parse(GetType(eLanguages), GetLanguageByLCID(L))
                    End If
                Catch ex As Exception
                    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with DefaultAudioLanguage(). Error: " & ex.Message, Nothing, Nothing)
                    Return Nothing
                End Try
            End Get
        End Property

        Public ReadOnly Property DefaultAudioExtension() As eAudioExtensions
            Get
                Try
                    Dim L As Integer
                    Dim Ext As DvdAudioLangExt
                    HR = Graph.DVDInfo.GetDefaultAudioLanguage(L, Ext)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    Return [Enum].Parse(GetType(eAudioExtensions), Ext.ToString)
                Catch ex As Exception
                    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with DefaultAudioExtension(). Error: " & ex.Message, Nothing, Nothing)
                    Return Nothing
                End Try
            End Get
        End Property

        Public ReadOnly Property DefaultSubtitleLanguage() As eLanguages
            Get
                Try
                    Dim L As Integer
                    Dim Ext As DvdSubPicLangExt
                    HR = Graph.DVDInfo.GetDefaultSubpictureLanguage(L, Ext)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    If L = -1 Then
                        Return eLanguages.English
                    Else
                        Return [Enum].Parse(GetType(eLanguages), GetLanguageByLCID(L))
                    End If
                Catch ex As Exception
                    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with DefaultSubtitleLanguage(). Error: " & ex.Message, Nothing, Nothing)
                    Return Nothing
                End Try
            End Get
        End Property

        Public ReadOnly Property DefaultSubtitleExtension() As eSubExtensions
            Get
                Try
                    Dim L As Integer
                    Dim Ext As DvdSubPicLangExt
                    HR = Graph.DVDInfo.GetDefaultSubpictureLanguage(L, Ext)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    Return [Enum].Parse(GetType(eSubExtensions), Ext.ToString)
                Catch ex As Exception
                    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with DefaultSubtitleExtension(). Error: " & ex.Message, Nothing, Nothing)
                    Return Nothing
                End Try
            End Get
        End Property

        Public ReadOnly Property DefaultMenuLanguage() As eLanguages
            Get
                Try
                    Dim L As Integer
                    HR = Graph.DVDInfo.GetDefaultMenuLanguage(L)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    'RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Default menu language: " & DB.GetLanguageByLCID(L))
                    Return [Enum].Parse(GetType(eLanguages), GetLanguageByLCID(L))
                Catch ex As Exception
                    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with GetDefaultMenuLanguage. Error: " & ex.Message, Nothing, Nothing)
                End Try
            End Get
        End Property

        Public ReadOnly Property DefaultAspectRatio() As ePreferredAspectRatio
            Get
                Return _DefaultAspectRatio
            End Get
        End Property
        Private _DefaultAspectRatio As ePreferredAspectRatio = ePreferredAspectRatio.Anamorphic

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns>Zero based!</returns>
        ''' <remarks></remarks>
        Public Property Region() As eRegions
            Get
                Dim rKeyV As RegistryKey
                rKeyV = Registry.LocalMachine.OpenSubKey("Software\NVIDIA Corporation\Filters\Video", True)
                Dim o As Object = rKeyV.GetValue("DVDRegion")
                rKeyV.Close()

                'setup bitmask
                Dim RegionBitmask As Byte = CByte(o)
                Select Case RegionBitmask
                    Case 1
                        Return eRegions.One
                    Case 2
                        Return eRegions.Two
                    Case 4
                        Return eRegions.Three
                    Case 8
                        Return eRegions.Four
                    Case 16
                        Return eRegions.Five
                    Case 32
                        Return eRegions.Six
                    Case 64
                        Return eRegions.Seven
                    Case 128
                        Return eRegions.Eight
                End Select
            End Get
            Set(ByVal value As eRegions)
                Try
                    'REGION
                    Dim rKeyA, rKeyV As RegistryKey
                    rKeyA = Registry.LocalMachine.OpenSubKey("Software\NVIDIA Corporation\Filters\Audio", True)
                    rKeyV = Registry.LocalMachine.OpenSubKey("Software\NVIDIA Corporation\Filters\Video", True)
                    If rKeyA Is Nothing Then
                        rKeyA = Registry.LocalMachine.CreateSubKey("Software\NVIDIA Corporation\Filters\Audio")
                    End If
                    If rKeyV Is Nothing Then
                        rKeyV = Registry.LocalMachine.CreateSubKey("Software\NVIDIA Corporation\Filters\Video")
                    End If

                    'setup bitmask
                    Dim RegionBitmask As Byte = CByte(value) + 1
                    Select Case RegionBitmask
                        Case 1
                            'do nothing
                        Case 2
                            'do nothing
                        Case 3
                            RegionBitmask = 4 '0b100
                        Case 4
                            RegionBitmask = 8 '0b1000
                        Case 5
                            RegionBitmask = 16 '0b10000
                        Case 6
                            RegionBitmask = 32 '0b100000
                        Case 7
                            RegionBitmask = 64 '0b1000000
                        Case 8
                            RegionBitmask = 128 '0b10000000
                    End Select

                    rKeyA.SetValue("DVDRegion", CInt(RegionBitmask))
                    rKeyV.SetValue("DVDRegion", CInt(RegionBitmask))
                    rKeyA.Close()
                    rKeyV.Close()

                    'If DB Is Nothing Then Exit Sub
                    If Graph.MediaCtrl Is Nothing Then Exit Property

                    'me.DVDRegion = NewRegion

                    'Insert new decoders
                    HR = Graph.MediaCtrl.Stop()
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    HR = Graph.GraphBuilder.RemoveFilter(Graph.VSDecoder)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    HR = Graph.GraphBuilder.RemoveFilter(Graph.AudioDecoder)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    Graph.AddNVidiaAudioDecoder(AVMode <> eAVMode.Decklink)
                    Graph.AddNVidiaVideoDecoder(False, AVMode <> eAVMode.Decklink)

                    If AVMode <> eAVMode.DesktopVMR Then

                        HR = Graph.GraphBuilder.Connect(Graph.DVDNav_VidPin, Graph.VidDec_Vid_In)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        HR = Graph.GraphBuilder.Connect(Graph.VidDec_Vid_Out, Graph.KO_In)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        HR = Graph.GraphBuilder.Connect(Graph.DVDNav_AudPin, Graph.AudDec_InPin)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        HR = Graph.GraphBuilder.Render(Graph.AudDec_OutPin)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        HR = Graph.GraphBuilder.Connect(Graph.DVDNav_SubPin, Graph.VidDec_SP_In)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        HR = Graph.GraphBuilder.Connect(Graph.VidDec_SP_Out, Graph.KO_SP)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        HR = Graph.GraphBuilder.Connect(Graph.VidDec_CC_Out, Graph.L21G_In)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    Else
                        'DesktopVMR
                        HR = Graph.GraphBuilder.Connect(Graph.DVDNav_VidPin, Graph.VidDec_Vid_In)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        HR = Graph.GraphBuilder.Connect(Graph.VidDec_Vid_Out, Graph.KO_In)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        HR = Graph.GraphBuilder.Connect(Graph.DVDNav_SubPin, Graph.VidDec_SP_In)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        HR = Graph.GraphBuilder.Connect(Graph.VidDec_SP_Out, Graph.KO_SP)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        HR = Graph.GraphBuilder.Render(Graph.VidDec_CC_Out)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        HR = Graph.GraphBuilder.Connect(Graph.DVDNav_AudPin, Graph.AudDec_InPin)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        HR = Graph.GraphBuilder.Render(Graph.AudDec_OutPin)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    End If

                Catch ex As Exception
                    Throw New Exception("Problem SetRegion(). Error: " & ex.Message & " StackTrace: " & ex.StackTrace)
                End Try
            End Set
        End Property

        Public ReadOnly Property ParentalLevelAndCountry() As String
            Get
                Try
                    Dim PL As Integer
                    Dim b(1) As Byte
                    Dim handle As GCHandle = GCHandle.Alloc(b, GCHandleType.Pinned)
                    Dim ptrBuffer As IntPtr = handle.AddrOfPinnedObject()
                    HR = Graph.DVDInfo.GetPlayerParentalLevel(PL, ptrBuffer)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    b = RemoveExtraBytesFromArray(b, False)
                    Dim Out As String = System.Text.Encoding.ASCII.GetString(b)
                    If Out.ToString.Length = 0 Then Out = "Null"
                    Out = Countries.GetCountryFromAlpha(Out)
                    If PL = -1 Then
                        Out &= " - Off"
                    Else
                        Out &= " - " & PL
                    End If
                    handle.Free()
                    Return Out
                Catch ex As Exception
                    Throw New Exception("Problem with ParentalLevelAndCountry(). Error: " & ex.Message)
                End Try
            End Get
        End Property

#End Region 'API:PROPERTIES:USER CONFIG:PLAYER DEFAULTS

#Region "API:PROPERTIES:USER CONFIG:PROC AMP"

        Public Property ProcAmp_Active() As Boolean
            Get
                Return _ProcAmp_Active
            End Get
            Set(ByVal value As Boolean)
                _ProcAmp_Active = value
                Try
                    If Graph.KO_IKeystoneProcAmp Is Nothing Then Exit Property
                    HR = Graph.KO_IKeystoneProcAmp.ToggleProcAmp(value, ProcAmp_HalfScreen)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Catch ex As Exception
                    Throw New Exception("Problem with SET ProcAmp_Active(). Error: " & ex.Message)
                End Try
            End Set
        End Property
        Private _ProcAmp_Active As Boolean = False

        Public Property ProcAmp_HalfScreen() As Boolean
            Get
                Return _ProcAmp_HalfScreen
            End Get
            Set(ByVal value As Boolean)
                _ProcAmp_HalfScreen = value
                Try
                    If Graph.KO_IKeystoneProcAmp Is Nothing Then Exit Property
                    HR = Graph.KO_IKeystoneProcAmp.ToggleProcAmp(ProcAmp_Active, value)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Catch ex As Exception
                    Throw New Exception("Problem with SET ProcAmp_HalfScreen(). Error: " & ex.Message)
                End Try
            End Set
        End Property
        Private _ProcAmp_HalfScreen As Boolean = False

        Public Property ProcAmp_Brightness() As Double
            Get
                Return _ProcAmp_Brightness
            End Get
            Set(ByVal value As Double)
                _ProcAmp_Brightness = value
                Try
                    If Graph.KO_IKeystoneProcAmp Is Nothing Then Exit Property
                    Dim HR As Integer = Graph.KO_IKeystoneProcAmp.put_Brightness(value)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Catch ex As Exception
                    Throw New Exception("Problem with SET ProcAmp_Brightness. Error: " & ex.Message)
                End Try
            End Set
        End Property
        Private _ProcAmp_Brightness As Double = 80

        Public Property ProcAmp_Contrast() As Double
            Get
                Return _ProcAmp_Contrast
            End Get
            Set(ByVal value As Double)
                _ProcAmp_Contrast = value
                Try
                    If Graph.KO_IKeystoneProcAmp Is Nothing Then Exit Property
                    Dim HR As Integer = Graph.KO_IKeystoneProcAmp.put_Contrast(value)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Catch ex As Exception
                    Throw New Exception("Problem with SET ProcAmp_Contrast. Error: " & ex.Message)
                End Try
            End Set
        End Property
        Private _ProcAmp_Contrast As Double = 80

        Public Property ProcAmp_Hue() As Double
            Get
                Return _ProcAmp_Hue
            End Get
            Set(ByVal value As Double)
                _ProcAmp_Hue = value
                Try
                    If Graph.KO_IKeystoneProcAmp Is Nothing Then Exit Property
                    Dim HR As Integer = Graph.KO_IKeystoneProcAmp.put_Hue(value)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Catch ex As Exception
                    Throw New Exception("Problem with SET ProcAmp_Hue. Error: " & ex.Message)
                End Try
            End Set
        End Property
        Private _ProcAmp_Hue As Double = 80

        Public Property ProcAmp_Saturation() As Double
            Get
                Return _ProcAmp_Saturation
            End Get
            Set(ByVal value As Double)
                _ProcAmp_Saturation = value
                Try
                    If Graph.KO_IKeystoneProcAmp Is Nothing Then Exit Property
                    Dim HR As Integer = Graph.KO_IKeystoneProcAmp.put_Saturation(value)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Catch ex As Exception
                    Throw New Exception("Problem with SET ProcAmp_Saturation. Error: " & ex.Message)
                End Try
            End Set
        End Property
        Private _ProcAmp_Saturation As Double = 80

        Public Property ProcAmp_ChannelFilter() As Boolean
            Get
                Return _ProcAmp_ChannelFilter
            End Get
            Set(ByVal value As Boolean)
                _ProcAmp_ChannelFilter = value
                Graph.KO_IKeystoneProcAmp.ToggleColorFilter(value, ProcAmp_WhichChannelFilter)
            End Set
        End Property
        Private _ProcAmp_ChannelFilter As Boolean = False

        Public Property ProcAmp_WhichChannelFilter() As eChannelFilter
            Get
                Return _ProcAmp_WhichChannelFilter
            End Get
            Set(ByVal value As eChannelFilter)
                _ProcAmp_WhichChannelFilter = value
            End Set
        End Property
        Private _ProcAmp_WhichChannelFilter As eChannelFilter = eChannelFilter.Y_Only

#End Region 'API:PROPERTIES:USER CONFIG:PROC AMP

        Public Property FieldSplitting() As Boolean
            Get
                Return _FieldSplitting
            End Get
            Set(ByVal value As Boolean)
                _FieldSplitting = value
                Graph.KO_IKeystoneMixer.FieldSplit(BoolToByte(value))
            End Set
        End Property
        Private _FieldSplitting As Boolean = False

        Public Property LetterboxColor() As Color
            Get
                Return _LetterboxColor
            End Get
            Set(ByVal value As Color)
                _LetterboxColor = value
                If Graph.KO_IKeystoneMixer Is Nothing Then Exit Property
                Graph.KO_IKeystoneMixer.SetLBColor(value.R, value.G, value.B)
            End Set
        End Property
        Private _LetterboxColor As Color

        Public Property ActionTitleSafeColor() As Color
            Get
                Return _ActionTitleSafeColor
            End Get
            Set(ByVal value As Color)
                _ActionTitleSafeColor = value
                If Graph.KO_IKeystoneMixer Is Nothing Then Exit Property
                Graph.KO_IKeystoneMixer.SetActionTitleSafeColor(value.R, value.G, value.B)
            End Set
        End Property
        Private _ActionTitleSafeColor As Color

        Public Property HighContrastSubpictures() As Boolean
            Get
                Return _HighContrastSubpictures
            End Get
            Set(ByVal value As Boolean)
                _HighContrastSubpictures = value
                If Graph.KO_IKeystoneMixer Is Nothing Then Exit Property
                Graph.KO_IKeystoneMixer.HighContrastSP(BoolToByte(value))
            End Set
        End Property
        Private _HighContrastSubpictures As Boolean = False

        'Public Property ChBackGoesTwo() As Boolean
        '    Get
        '        Return _ChBackGoesTwo
        '    End Get
        '    Set(ByVal value As Boolean)
        '        _ChBackGoesTwo = value
        '    End Set
        'End Property
        'Private _ChBackGoesTwo As Boolean = True

        Public Property NonSeamlessCellNotification() As Boolean
            Get
                Return _NSCNotification
            End Get
            Set(ByVal value As Boolean)
                _NSCNotification = value
            End Set
        End Property
        Private _NSCNotification As Boolean = False

        Public Property IntensityScaling() As eScalingMode
            Get
                Return _IntensityScaling
            End Get
            Set(ByVal value As eScalingMode)
                _IntensityScaling = value
            End Set
        End Property
        Private _IntensityScaling As eScalingMode = eScalingMode.Native_ScaleToAR

        Public Property IntensitySignalResolution() As eVideoResolution
            Get
                Return _IntensitySignalResolution
            End Get
            Set(ByVal value As eVideoResolution)
                _IntensitySignalResolution = value
            End Set
        End Property
        Private _IntensitySignalResolution As eVideoResolution = eVideoResolution._1920x1080

#End Region 'API:PROPERTIES:USER CONFIG

#Region "API:PROPERTIES:DUMP DIRECTORIES"

        Public Property DumpDirectory() As String
            Get
                Return _DumpDirectory
            End Get
            Set(ByVal value As String)
                _DumpDirectory = value
            End Set
        End Property
        Private _DumpDirectory As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\SMT Phoenix\Dump\"

        Public ReadOnly Property FrameGrabDumpDir() As String
            Get
                Dim out As String = DumpDirectory & "FrameGrabs\"
                If Not Directory.Exists(out) Then Directory.CreateDirectory(out)
                Return out
            End Get
        End Property

        Public ReadOnly Property AudioDumpDir() As String
            Get
                Dim out As String = DumpDirectory & "Audio\"
                If Not Directory.Exists(out) Then Directory.CreateDirectory(out)
                Return out
            End Get
        End Property

        Public ReadOnly Property SubpictureDumpDir() As String
            Get
                Dim out As String = DumpDirectory & "Subpictures\"
                If Not Directory.Exists(out) Then Directory.CreateDirectory(out)
                Return out
            End Get
        End Property

        Private ReadOnly Property ClosedCaptionDumpDir() As String
            Get
                Dim out As String = DumpDirectory & "ClosedCaptions\"
                If Not Directory.Exists(out) Then Directory.CreateDirectory(out)
                Return out
            End Get
        End Property

#End Region 'API:PROPERTIES:DUMP DIRECTORIES

#End Region 'API:PROPERTIES

#Region "API:METHODS"

#Region "API:METHODS:PLAYER INITIALIZATION"

        Public Function InitializePlayer(ByVal VIDEO_TS As String, ByVal PlayerDefaults As sNavigatorSetup, Optional ByRef nViewerHostControl As cSMTForm = Nothing) As Boolean
            Try
                If IsInitialized Then Throw New Exception("Player is already initialized and cannot be re-initialized. Please create a new instance of cDVDPlayer.")

                FilePath = VIDEO_TS

                If Not SystemJacketPictureTimer Is Nothing Then
                    SystemJacketPictureTimer.Stop()
                    SystemJacketPictureTimer = Nothing
                End If

                ViewerHostControl = nViewerHostControl

                'If Not Graph Is Nothing Then
                '    Graph.DestroyGraph()
                '    Graph = Nothing
                'End If

                If Graph Is Nothing Then
                    Graph = New cSMTGraph(Me.ParentForm.Handle)
                    Me.BuildGraph(Nothing, Nothing, Nothing, Nothing)
                End If

                Graph.KO_IKeystone.Pause(0)

                If Path.HasExtension(VIDEO_TS) Then
                    VIDEO_TS = Path.GetDirectoryName(VIDEO_TS)
                End If

                If Not Me.IsPathValidDVDDirectory(VIDEO_TS) Then
                    Me.EjectProject()
                    Return False
                End If

                Me.ConsumePlayerDefaults(PlayerDefaults)

                PlayState = ePlayState.Init
                _JacketPicturesAvailable = False
                _Layerbreak = Nothing
                _NonSeamlessCells = Nothing
                RaiseEvent evClearConsole()

                '            If VIDEO_TS = "" Or InStr(DVDDirectory.ToLower, "\blankntsc\") Or InStr(VIDEO_TS.ToLower, "\blankpal\") Then
                '                'start last project in recent projects
                '                DVDDirectory = MostRecentProject
                '                If DVDDirectory = "" Then
                '                    RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "Starting project: D:\VIDEO_TS\", Nothing, Nothing)
                '                    If Not SetupNonSeamlessCells("D:\VIDEO_TS\") Then
                '                        Return False
                '                    End If
                '                Else
                '                    GoTo LoadingMostRecent
                '                End If
                '            Else
                'LoadingMostRecent:
                '                RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "Starting project: " & DVDDirectory, Nothing, Nothing)
                '                If Not SetupNonSeamlessCells(DVDDirectory) Then
                '                    Return False
                '                End If
                '            End If

                'setupnonseamlesscells(VIDEO_TS)

                PlayState = ePlayState.Playing
                If Not GetJacketPics(VIDEO_TS) Then
                    PlayState = ePlayState.Init
                    Return False
                End If

                Try
                    CurrentDVD = New cDVD(VIDEO_TS)
                Catch ex As Exception
                    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "A problem occurred during processing the IFOs for the selected project.", ex.Message, Nothing)
                    Return False
                End Try

                If CurrentDVD.HasCallSSRSM127 Then
                    RaiseEvent evProjectHasCallSSRSM127()
                    Return False
                End If
                RaiseEvent evRegionMask_SET(CurrentDVD.VMGM.Regions)
                VTSCount = CurrentDVD.VMGM.NumberOfTitleSets
                GlobalTTCount = CurrentDVD.VMGM.NumberOfGlobalTitles
                _NonSeamlessCells = New colNonSeamlessCells(CurrentDVD)

                If CurrentDVD.VideoStandard = eVideoStandard.NTSC And CurrentVideoStandard = eVideoStandard.PAL Then
                    'switch to NTSC
                    If Not Me.SwitchVideoFormats(480) Then Throw New Exception("SwitchVideoFormats failed.")
                    '_CurrentVideoStandard = eVideoStandard.NTSC
                    HR = Graph.DVDCtrl.SetDVDDirectory(Me.BlankDVDPath_NTSC)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    HR = Graph.MediaCtrl.Run
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    'debugging 070703- seems to be needed. going to leave for now (build 25)
                    Me.ConsumePlayerDefaults(PlayerDefaults)

                ElseIf CurrentDVD.VideoStandard = eVideoStandard.PAL And CurrentVideoStandard = eVideoStandard.NTSC Then
                    'switch to PAL
                    If Not Me.SwitchVideoFormats(576) Then Throw New Exception("SwitchVideoFormats failed.")
                    '_CurrentVideoStandard = eVideoStandard.PAL

                    HR = Graph.DVDCtrl.SetDVDDirectory(Me.BlankDVDPath_PAL)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    HR = Graph.MediaCtrl.Run
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    'debugging 070703- seems to be needed. going to leave for now (build 25)
                    Me.ConsumePlayerDefaults(PlayerDefaults)

                End If

                If Not CurrentDomain = DvdDomain.Stop Then
                    HR = Graph.DVDCtrl.Stop
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End If

                If Graph.MediaControlState = eMediaControlState.Running Then
                    HR = Graph.MediaCtrl.Stop()
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End If

                HR = Graph.DVDCtrl.SetDVDDirectory(VIDEO_TS)
                If HR = -2147024809 Then
                    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "DVD path not found and disc not found in drive.", Nothing, Nothing)
                    PlayState = ePlayState.Init
                    Return False
                End If
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                If Not Graph.InitializeDVDNavigator(Languages, Countries, Me.Users_NavigatorSetup) Then Throw New Exception("Failed to initialize DVD Navigator.")
                Me.Region = Me.Users_NavigatorSetup.PLAYER_REGION

                'THIS IS WHERE PLAYBACK REALLY STARTS
                If Not Graph.MediaControlState = eMediaControlState.Running Then
                    HR = Graph.MediaCtrl.Run
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    'If Me.PlayerMode = ePlayerMode.BlackMagicSDI Then Me.SendBlankAudioSample() 'used this for Post Logic content. But decided we're just not going to support video content without audio because there are too many challenges to make it work properly with Decklink audio renderer behaving the way it is.
                End If

                If StopAtLocation.ChapterNum <> 0 And Not PlayState = ePlayState.SystemJP Then
                    HR = Graph.DVDInfo.GetCurrentDomain(CurrentDomain)
                    If CurrentDomain <> DvdDomain.Stop Then
                        HR = Graph.DVDCtrl.Stop()
                        While HR <> 0
                            HR = Graph.DVDCtrl.Stop
                            RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "Trying to resume.", Nothing, Nothing)
                        End While
                        RaiseEvent evClearConsole()
                    End If
                    HR = Graph.DVDCtrl.PlayAtTimeInTitle(StopAtLocation.TitleNum, StopAtLocation.timeCode, DvdCmdFlags.None, Nothing)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR) 'And HR <> -2147220874 
                End If

                Dim VI As sVolumeInfo = VolumeInfo
                Me._VolumeCount = VI.VolumeCount
                Me._DiscSide = VI.DiscSide
                Me._CurrentVolume = VI.CurrentVolume

                If Not RegionCompatibilityCheck() Then
                    Graph.KO_IKeystone.Pause(1) 'we don't want them to see anything
                    RaiseEvent evWrongRegion(CByte(Region) + 1, CurrentDVD.VMGM.Regions)
                    Return True
                End If

                GetStartupSPRMs()

                IsInitialized = True
                RaiseEvent evPlayerInitialized()
                RaiseEvent evPlaybackStarted()
                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with InitializePlayer(). Error: " & ex.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

        Public Function InitializeSystemJacketPicture() As Boolean
            Try
                Graph = New cSMTGraph(ParentForm.Handle)

                PlayState = ePlayState.SystemJP
                Graph.SetVideoStandardViaRegistry(True)

                'DVDDirectory = SystemJPPath

                'new graph
                If Not BuildGraph(Nothing, Nothing, Nothing, Nothing) Then
                    Return False
                End If

                If Me.CurrentVideoStandard = eVideoStandard.PAL Then
                    Me.SwitchVideoFormats(480)
                    HR = Graph.DVDCtrl.SetDVDDirectory(Me.BlankDVDPath_PAL)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    HR = Graph.MediaCtrl.Run
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End If

                If Not CurrentDomain = DvdDomain.Stop Then
                    HR = Graph.DVDCtrl.Stop
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End If

                'If Not Graph.InitializeDVDNavigator(Languages, Countries, Me.Users_NavigatorSetup) Then Throw New Exception("Failed to initialize DVD Navigator.")
                'Me.Region = Me.Users_NavigatorSetup.PLAYER_REGION

                FilePath = BlankDVDPath_NTSC
                HR = Graph.DVDCtrl.SetDVDDirectory(Me.BlankDVDPath_NTSC)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.MediaCtrl.Run()
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                RenderSystemJacketPicture()

                RaiseEvent evSystemJacketPictureDisplayed()
                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with DisplaySystemJP. Error: " & ex.Message, " Stack Trace: " & ex.StackTrace, Nothing)
                Return False
            End Try
        End Function

#End Region 'API:METHODS:PLAYER INITIALIZATION

#Region "API:METHODS:PLAY CONTROL"

#Region "API:METHODS:PLAY CONTROL:CORE"

        Public Overrides Function Play() As Boolean
            Try
                If AVMode <> eAVMode.DesktopVMR Then
                    Graph.KeystoneOmni_Unpause()
                End If

                If Me.PlayState = ePlayState.SystemJP Then Return True
                Me.ClearOSD()

                If PlayState = ePlayState.Init Or Graph.MediaCtrl Is Nothing Then
                    Return False

                ElseIf PlayState = ePlayState.Paused Then
                    HR = Graph.DVDCtrl.Pause(False)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    _CurrentSpeed = OneX
                    PlayState = ePlayState.Playing
                    Me.ReSyncAudio(False)
                    If MenuMode = eMenuMode.Still And Not (Graph.DVDCtrl Is Nothing) Then HR = Graph.DVDCtrl.StillOff()

                ElseIf PlayState = ePlayState.FastForwarding Or PlayState = ePlayState.Rewinding Then 'Or playState = ePlayState.SlowForwarding Or playState = ePlayState.SlowRewinding 
                    HR = Graph.DVDCtrl.PlayForwards("1.0", DvdCmdFlags.Flush, cmdOption)
                    'HR = Graph.DVDCtrl.PlayForwards("1.0", DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    HR = Graph.KO_IKeystone.DeactivateFFRW
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    PlayState = ePlayState.Playing
                    _CurrentSpeed = OneX
                    Me.UnMute()
                    Me.ReSyncAudio(False)

                ElseIf PlayState = ePlayState.Stopped Or PlayState = ePlayState.ProjectJP Then
                    'this should "resume"

                    '080610
                    HR = Graph.DVDCtrl.Pause(False)
                    If HR >= 0 Then
                        'HR = Graph.DVDCtrl.PlayForwards("1.0", DvdCmdFlags.None, cmdOption)
                        'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        _CurrentSpeed = OneX
                        PlayState = ePlayState.Playing
                    End If


                    '080604
                    'If Not Me.RestoreFromState() Then Return False


                    ''080516
                    'HR = Graph.DVDCtrl.SetState(DVDState, DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    ''080516

                    'If StopAtLocation.TitleNum > 0 And StopAtLocation.timeCode.bSeconds <> Nothing Then
                    '    cmdOption = New OptIDvdCmd
                    '    HR = Graph.DVDCtrl.PlayAtTimeInTitle(StopAtLocation.TitleNum, StopAtLocation.timeCode, DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                    '    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    'Else
                    '    cmdOption = New OptIDvdCmd
                    '    HR = Graph.DVDCtrl.ShowMenu(DvdMenuID.Root, DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                    '    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    'End If


                    ''If StopAtLocation.TitleNum = 0 Then
                    ''    If Me.CurrentUserOperations(2) Then
                    ''        If Not Me.PlayTitle(1) Then
                    ''            Throw New Exception("Problem with Stop->Play with no resume info. Could not play title 1.")
                    ''        End If
                    ''    ElseIf Me.CurrentUserOperations(0) Then
                    ''        cmdOption = New OptIDvdCmd
                    ''        HR = Graph.DVDCtrl.PlayAtTimeInTitle(1, New cTimecode(0, 0, 0, 0, True).ToDVDTimeCode, DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                    ''        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    ''    Else
                    ''        Throw New Exception("UOPs prevent a proper restart from stop domain.")
                    ''    End If
                    ''Else
                    ''    cmdOption = New OptIDvdCmd
                    ''    HR = Graph.DVDCtrl.PlayAtTimeInTitle(StopAtLocation.TitleNum, StopAtLocation.timeCode, DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                    ''    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    ''End If

                    PlayState = ePlayState.Playing
                    RaiseEvent evVTSCount_SET(Me.VTSCount)

                ElseIf PlayState = ePlayState.DoubleStopped Then
                    'this should start title 1 from the beginning
                    'If Not Me.PlayTitle(1) Then
                    '    Throw New Exception("Problem with Stop->Stop->Play. Could not play title 1.")
                    'End If

                    If Me.CurrentUserOperations(2) Then
                        If Not Me.PlayTitle(1) Then
                            Throw New Exception("Problem with Stop->Stop->Play. Could not play title 1.")
                        End If
                    ElseIf Me.CurrentUserOperations(0) Then
                        cmdOption = New OptIDvdCmd
                        HR = Graph.DVDCtrl.PlayAtTimeInTitle(1, New cTimecode(0, 0, 0, 0, True).DVDTimeCode, DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    Else
                        RaiseEvent evDoubleStopPlay_UOP2_Prohibited()
                        Me.RestoreFromState()
                    End If

                    PlayState = ePlayState.Playing
                    RaiseEvent evVTSCount_SET(Me.VTSCount)

                ElseIf PlayState = ePlayState.FrameStepping Then
                    QuitFrameStepping()

                ElseIf PlayState = ePlayState.VariSpeed Then
                    DeactivateVarispeed()

                ElseIf PlayState = ePlayState.Playing Then
                    HR = Graph.DVDCtrl.PlayForwards("1.0", DvdCmdFlags.None, cmdOption)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                End If

                RaiseEvent evPlaybackStarted()
                RaiseEvent evVTSCount_SET(Me.VTSCount)
                PlayState = ePlayState.Playing
                'PlaybackStartFinalization()
                _StartingDVD = False
                Return True
            Catch ex As Exception
                If CheckEx(ex, "Start Playback") Then Exit Function
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem starting playback. error: " & ex.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

        Public Overrides Function [Stop]() As Boolean
            Try
                If Me.PlayState = ePlayState.SystemJP Then Return True
                If PlayState = ePlayState.VariSpeed Then DeactivateVarispeed()
                If PlayState = ePlayState.ABLoop Then Me.ABLoop_Clear()

                Select Case PlayState
                    Case ePlayState.Stopped 'ENTER SECOND STOP STATE
                        'Go to "DOUBLE STOPPED" state
                        PlayState = ePlayState.DoubleStopped
                        ' Clear GPRMs
                        For i As Integer = 0 To 15
                            Me.SetGPRM(i, 0)
                        Next

                    Case ePlayState.DoubleStopped 'ENTER THIRD STOP STATE (eject project)
                        'THIRD TIME'S A CHARM:
                        Me.DoubleStopDispose = True
                        RaiseEvent evTripleStop()

                    Case Else 'ENTER FIRST STOP STATE

                        If PlayState = ePlayState.FrameStepping Then Me.QuitFrameStepping()

                        'If Not Me.StoreState() Then Throw New Exception("Failed to store state.")

                        ' ''080516
                        ''HR = Graph.DVDInfo.GetState(DVDState)
                        ''If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        ' ''Dim id As ULong = 0
                        ' ''HR = DVDState.GetDiscID(id)
                        ' ''If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        ' ''Debug.WriteLine("DiscID=" & id)

                        '080516

                        ''If CurrentDomain = DvdDomain.VideoTitleSetMenu Then
                        ''    StopAtLocation = New DvdPlayLocation
                        ''Else
                        ''    StopAtLocation = New DvdPlayLocation
                        ''    HR = Graph.DVDInfo.GetCurrentLocation(StopAtLocation)
                        ''    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        ''End If

                        'HR = Graph.DVDCtrl.SetOption(DvdOptionFlag.ResetOnStop, False)
                        'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        'HR = Graph.DVDCtrl.Stop()
                        'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        'PlayState = ePlayState.Stopped

                        ''080606

                        'HR = Graph.DVDCtrl.Stop()
                        'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        'RaiseEvent evPlaybackStopped(True)

                        'Thread.Sleep(300)

                        'HR = Graph.DVDCtrl.SetOption(DvdOptionFlag.ResetOnStop, False)
                        'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        'HR = Graph.MediaCtrl.Stop()
                        'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        'PlayState = ePlayState.Stopped

                        'HR = Graph.DVDCtrl.SetOption(DvdOptionFlag.ResetOnStop, False)
                        'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        'HR = Graph.DVDCtrl1.StopForResume
                        'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                        'PlayState = ePlayState.Stopped

                        '080610

                        'store state just in case it needs to be used on play from double stop (see Play())
                        If Not Me.StoreState() Then Throw New Exception("Failed to store state.")

                        If Me.CurrentDomain = DvdDomain.Title Then
                            'TITLE SPACE, do single stop if pause (UOP19) and rewind (UOP9) are open.
                            'Check the pause UOP
                            If Me.CurrentUserOperations(19) And Me.CurrentUserOperations(9) Then
                                Me.Pause()
                                PlayState = ePlayState.Stopped

                                'now show the appropriate image
                                StopDomainImageSelection()

                            Else
                                HR = Graph.DVDCtrl.SetOption(DvdOptionFlag_Win7.DVD_ResetOnStop, False)
                                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                                HR = Graph.DVDCtrl.Stop()
                                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                                PlayState = ePlayState.DoubleStopped

                                For i As Integer = 0 To 15
                                    Me.SetGPRM(i, 0)
                                Next
                            End If
                        Else
                            'NOT TITLE SPACE, go directly to double stop.
                            HR = Graph.DVDCtrl.SetOption(DvdOptionFlag_Win7.DVD_ResetOnStop, False)
                            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                            HR = Graph.DVDCtrl.Stop()
                            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                            PlayState = ePlayState.DoubleStopped

                            For i As Integer = 0 To 15
                                Me.SetGPRM(i, 0)
                            Next
                        End If

                End Select

                RaiseEvent evPlaybackStopped(True)
                Return True
            Catch ex As Exception
                If CheckEx(ex, "Stop") Then Exit Function
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem stopping playback. error: " & ex.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

        Public Function EjectProject(Optional ByVal Force As Boolean = False) As Boolean
            If Not Force Then If Me.PlayState = ePlayState.SystemJP Then Return True
            Me.Dispose()
            Return True
        End Function

        Public Function PlayTitle(ByVal TTNo As String) As Boolean
            Try
                If Me.PlayState = ePlayState.SystemJP Then Return True
                If PlayState = ePlayState.FrameStepping Then Me.QuitFrameStepping()
                If PlayState = ePlayState.VariSpeed Then DeactivateVarispeed()
                If PlayState = ePlayState.ABLoop Then Me.ABLoop_Clear()
                Graph.KeystoneOmni_Unpause()
                Me.UnMute()
                'Dim u() As Boolean = Me.CurrentUserOperations
                HR = Graph.DVDCtrl.PlayTitle(TTNo, DvdCmdFlags.Flush, cmdOption)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                SetOSD(OSD_TTChange(TTNo), RGB(255, 255, 255), 90, True, 1, "PlayTitle", Me.TitleSafeLeft, Me.TitleSafeTop + 15)
                PlayState = ePlayState.Playing
                Return True
            Catch ex As Exception
                If CheckEx(ex, "Play Title") Then Exit Function
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem playing title. error: " & ex.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

        Public Function PlayChapter(ByVal PTTNo As String, ByVal PreRoll As Boolean) As Boolean
            Try
                If Me.PlayState = ePlayState.SystemJP Then Return True
                If PlayState = ePlayState.FrameStepping Then Me.QuitFrameStepping()
                If PlayState = ePlayState.VariSpeed Then DeactivateVarispeed()
                If PlayState = ePlayState.ABLoop Then Me.ABLoop_Clear()
                Graph.KeystoneOmni_Unpause()
                HR = Graph.DVDCtrl.PlayChapter(PTTNo, DvdCmdFlags.Flush, cmdOption)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                If CurrentDomain = DvdDomain.Title Then
                    SetOSD(OSD_ChChange(PTTNo), RGB(255, 255, 255), 90, True, 1, "PlayChapter", 55, 45)
                End If
                PlayState = ePlayState.Playing
                Return True
            Catch ex As Exception
                If CheckEx(ex, "Play Chapter") Then Exit Function
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem playing chapter. error: " & ex.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

        Public Sub NextChapter()
            Debug.WriteLine("cDVDPlayer::NextChapter() - Start")
            Try
                If Me.PlayState = ePlayState.SystemJP Then Exit Sub
                If Not GetCommandSyncMutex() Then Exit Sub
                Graph.KeystoneOmni_Unpause()
                If PlayState = ePlayState.FastForwarding Or PlayState = ePlayState.Rewinding Or PlayState = ePlayState.Paused Then
                    If Not Play() Then
                        RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with StartPlayback from NextChapter.", Nothing, Nothing)
                        Exit Sub
                    End If
                End If
                If PlayState = ePlayState.FrameStepping Then Me.QuitFrameStepping()
                If PlayState = ePlayState.VariSpeed Then DeactivateVarispeed()
                If PlayState = ePlayState.ABLoop Then Me.ABLoop_Clear()
                If PlayState <> ePlayState.Playing Or Graph.DVDCtrl Is Nothing Then Exit Sub
                HR = Graph.DVDCtrl.PlayNextChapter(DvdCmdFlags.Flush Or DvdCmdFlags.SendEvt, cmdOption)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                If CurrentDomain = DvdDomain.Title And Not (CurrentChapter = ChaptersInCurrentTitle) Then
                    SetOSD(OSD_ChChange(CurrentChapter + 1), RGB(255, 255, 255), 90, True, 1, "PreviousChapter", Me.TitleSafeLeft, Me.TitleSafeTop + 15)
                End If
            Catch ex As Exception
                If CheckEx(ex, "Next Chapter") Then Exit Sub
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem going to next chapter. error: " & ex.Message, Nothing, Nothing)
            End Try
            Debug.WriteLine("cDVDPlayer::NextChapter() - End")
        End Sub

        Public Sub PreviousChapter()
            Try
                If Me.PlayState = ePlayState.SystemJP Then Exit Sub
                If Not GetCommandSyncMutex() Then Exit Sub
                Graph.KeystoneOmni_Unpause()
                If PlayState = ePlayState.FastForwarding Or PlayState = ePlayState.Rewinding Or PlayState = ePlayState.Paused Then
                    If Not Play() Then
                        RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "Problem with StartPlayback from NextChapter.", Nothing, Nothing)
                        Exit Sub
                    End If
                End If
                If PlayState = ePlayState.FrameStepping Then Me.QuitFrameStepping()
                If PlayState = ePlayState.VariSpeed Then DeactivateVarispeed()
                If PlayState = ePlayState.ABLoop Then Me.ABLoop_Clear()
                If PlayState <> ePlayState.Playing Or Graph.DVDCtrl Is Nothing Then Exit Sub

                'Three second rule for Sony 080417
                Dim ThreeSecondRule As Boolean = False
                If Me.PrevChapter_Timer Is Nothing Then
                    'start the timer
                    PrevChapter_Timer = New System.Windows.Forms.Timer
                    PrevChapter_Timer.Interval = 3000
                    AddHandler PrevChapter_Timer.Tick, AddressOf PrevChapter_Timer_Tick
                    Me.PrevChapter_Timer.Start()
                Else
                    'the timer is already running
                    ThreeSecondRule = True 'go to the beginning of the previous chapter
                    Me.PrevChapter_Timer.Stop()
                    Me.PrevChapter_Timer.Start()
                End If

                If CurrentChapter = 1 Then
                    HR = Graph.DVDCtrl.PlayChapter(1, DvdCmdFlags.Flush, cmdOption)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    If CurrentDomain = DvdDomain.Title Then
                        SetOSD(OSD_ChChange(1), RGB(255, 255, 255), 90, True, 1, "PreviousChapter", Me.TitleSafeLeft, Me.TitleSafeTop + 15)
                    End If
                    RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "Already in 1st chapter. Starting chapter 1 over.", Nothing, Nothing)
                ElseIf ThreeSecondRule Then
                    HR = Graph.DVDCtrl.PlayPrevChapter(DvdCmdFlags.Flush, cmdOption)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    If CurrentDomain = DvdDomain.Title Then
                        SetOSD(OSD_ChChange(CurrentChapter - 1), RGB(255, 255, 255), 90, True, 1, "PreviousChapter", Me.TitleSafeLeft, Me.TitleSafeTop + 15)
                    End If
                Else
                    HR = Graph.DVDCtrl.PlayChapter(CurrentChapter, DvdCmdFlags.Flush, cmdOption)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    If CurrentDomain = DvdDomain.Title Then
                        SetOSD(OSD_ChChange(CurrentChapter), RGB(255, 255, 255), 90, True, 1, "PreviousChapter", Me.TitleSafeLeft, Me.TitleSafeTop + 15)
                    End If
                End If
            Catch ex As Exception
                If CheckEx(ex, "Previous Chapter") Then Exit Sub
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem going to previous chapter. error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Function PlayAtTime(ByVal TC As DvdTimeCode, ByVal ShowError As Boolean) As Boolean
            Try
                If Me.PlayState = ePlayState.SystemJP Then Return True
                Graph.KeystoneOmni_Unpause()
                _LastTimeSearch.bHours = TC.bHours
                _LastTimeSearch.bMinutes = TC.bMinutes
                _LastTimeSearch.bSeconds = TC.bSeconds
                _LastTimeSearch.bFrames = 1

                If Play() Then
                    HR = Graph.DVDCtrl.PlayAtTime(TC, DvdCmdFlags.Flush, cmdOption)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    Return True
                Else
                    Return False
                End If

            Catch ex As Exception
                If ShowError Then
                    If CheckEx(ex, "Play At Time") Then Exit Function
                    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem in PlayAtTime. Error: " & ex.Message, Nothing, Nothing)
                End If
                Return False
            End Try
        End Function

        Public Sub PlayAtTime_Clean(ByVal TC As DvdTimeCode, ByVal ShowError As Boolean)
            Try
                Graph.KeystoneOmni_Unpause()
                'TC.bSeconds = 1
                HR = Graph.DVDCtrl.PlayAtTime(TC, DvdCmdFlags.Flush, cmdOption)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                Debug.WriteLine("Problem with PlayAtTime_Clean(). Error: " & ex.Message)
                If ShowError Then
                    If CheckEx(ex, "Play At Time") Then Exit Sub
                    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem in PlayAtTime. Error: " & ex.Message, Nothing, Nothing)
                End If
            End Try
        End Sub

        Public Function PlayAtTimeInTitle(ByVal TC As DvdTimeCode, ByVal TT As Byte) As Boolean
            Try
                If PlayState = ePlayState.ABLoop Then Me.ABLoop_Clear()
                Graph.KeystoneOmni_Unpause()

                HR = Graph.DVDCtrl.PlayAtTimeInTitle(TT, TC, DvdCmdFlags.Flush, cmdOption)

                If HR = -2147024809 Then
                    Return False
                End If
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with PlayAtTimeInTitle. Error: " & ex.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

        Public Function PlayChapterInTitle(ByVal TitleNumber As Integer, ByVal ChapterNumber As Integer) As Boolean
            Try
                If Me.PlayState = ePlayState.SystemJP Then Return True
                If PlayState = ePlayState.FrameStepping Then Me.QuitFrameStepping()
                If PlayState = ePlayState.VariSpeed Then DeactivateVarispeed()
                If PlayState = ePlayState.ABLoop Then Me.ABLoop_Clear()
                Graph.KeystoneOmni_Unpause()
                HR = Graph.DVDCtrl.PlayChapterInTitle(TitleNumber, ChapterNumber, DvdCmdFlags.Flush, cmdOption)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                If CurrentDomain = DvdDomain.Title Then
                    SetOSD(OSD_ChChange(ChapterNumber), RGB(255, 255, 255), 90, True, 1, "PlayChapterInTitle", 55, 45)
                End If
                Return True
            Catch ex As Exception
                If CheckEx(ex, "PlayChapterInTitle") Then Exit Function
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with PlayChapterInTitle(). error: " & ex.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

        Public Overrides Function Pause() As Boolean
            Try
                If Graph.DVDCtrl Is Nothing Then Exit Function
                If Me.PlayState = ePlayState.SystemJP Then Return True
                Select Case PlayState
                    Case ePlayState.Playing, ePlayState.FastForwarding, ePlayState.Rewinding, ePlayState.Paused, ePlayState.ABLoop
                        GoTo DoIt
                    Case Else
                        Return False
                End Select

DoIt:
                If PlayState = ePlayState.Paused Then
                    Play()
                    If ABLoop_PauseFlag Then Me.PlayState = ePlayState.ABLoop
                    RaiseEvent evPlaybackPaused(False)
                Else
                    If PlayState = ePlayState.ABLoop Then ABLoop_PauseFlag = True
                    If PlayState = ePlayState.FastForwarding Or PlayState = ePlayState.Rewinding Or PlayState = ePlayState.VariSpeed Then
                        If Not Play() Then Throw New Exception("Failed Play().")
                    End If
                    HR = Graph.DVDCtrl.Pause(True)
                    If HR > 0 Then Marshal.ThrowExceptionForHR(HR)

                    If AVMode <> eAVMode.DesktopVMR Then
                        HR = Graph.KO_IKeystone.Pause(1)
                        If HR > 0 Then Marshal.ThrowExceptionForHR(HR)
                    End If

                    PlayState = ePlayState.Paused
                    RaiseEvent evPlaybackPaused(True)
                End If
                Return True
            Catch ex As Exception
                If CheckEx(ex, "Pause") Then Exit Function
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with PausePlayback(). Error: " & ex.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

        Public Overrides Function FastForward(Optional ByVal Speed As eShuttleSpeed = eShuttleSpeed.Not_Specified) As Boolean
            Try
                If Me.PlayState = ePlayState.SystemJP Then Return True

                If PlayState = ePlayState.Rewinding Then
                    _CurrentSpeed = FourX
                    GoTo ExecuteFF
                End If

                If PlayState = ePlayState.VariSpeed Then DeactivateVarispeed()
                If PlayState = ePlayState.ABLoop Then Me.ABLoop_Clear()
                Graph.KeystoneOmni_Unpause()

                If PlayState <> ePlayState.Playing And PlayState <> ePlayState.Paused And PlayState <> ePlayState.FastForwarding Then Exit Function

                Select Case CurrentSpeed
                    Case OneX
                        _CurrentSpeed = TwoX
                    Case TwoX
                        _CurrentSpeed = FourX
                    Case FourX
                        _CurrentSpeed = EightX
                    Case EightX
                        _CurrentSpeed = SixteenX
                    Case SixteenX
                        _CurrentSpeed = ThirtyTwoX
                    Case ThirtyTwoX
                        RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "32x is top play speed.", Nothing, Nothing)
                End Select

                'Select Case CurrentSpeed
                '    Case OneX
                '        _CurrentSpeed = FourX
                '    Case FourX
                '        _CurrentSpeed = EightX
                '    Case EightX
                '        _CurrentSpeed = SixteenX
                '    Case SixteenX
                '        _CurrentSpeed = ThirtyTwoX
                '    Case ThirtyTwoX
                '        RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "32x is top play speed.", Nothing, Nothing)
                'End Select

ExecuteFF:
                Debug.WriteLine("FF Speed: " & CurrentSpeed)
                HR = Graph.DVDCtrl.PlayForwards(CurrentSpeed, DvdCmdFlags.Flush, cmdOption)
                'HR = Graph.DVDCtrl.PlayForwards(CurrentSpeed, DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = Graph.KO_IKeystone.ActivateFFRW(CurrentSpeed)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                PlayState = ePlayState.FastForwarding
                Me.Mute()
                Me.SetOSD(OSDBitmap_FFRWSpeed(Math.Round(CurrentSpeed, 0), True), RGB(255, 255, 255), 90, False, 1, "FF", Me.TitleSafeLeft, Me.TitleSafeTop)
                RaiseEvent evFastForward(CurrentSpeed)
            Catch ex As Exception
                If CheckEx(ex, "Fast Forward") Then Exit Function
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem fast forwarding. error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

        Public Overrides Function Rewind(Optional ByVal Speed As eShuttleSpeed = eShuttleSpeed.Not_Specified) As Boolean
            Try
                If Me.PlayState = ePlayState.SystemJP Then Return True
                If PlayState = ePlayState.FastForwarding Then
                    _CurrentSpeed = FourX
                    GoTo ExecuteRW
                End If

                If PlayState = ePlayState.VariSpeed Then DeactivateVarispeed()
                If PlayState = ePlayState.ABLoop Then Me.ABLoop_Clear()
                Graph.KeystoneOmni_Unpause()

                If PlayState <> ePlayState.Playing And PlayState <> ePlayState.Paused And PlayState <> ePlayState.Rewinding Then Exit Function

                Select Case CurrentSpeed
                    Case OneX
                        _CurrentSpeed = TwoX
                    Case TwoX
                        _CurrentSpeed = FourX
                    Case FourX
                        _CurrentSpeed = EightX
                    Case EightX
                        _CurrentSpeed = SixteenX
                    Case SixteenX
                        _CurrentSpeed = ThirtyTwoX
                    Case ThirtyTwoX
                        RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "32x is top play speed.", Nothing, Nothing)
                End Select
ExecuteRW:
                'HR = Graph.DVDCtrl.PlayBackwards(CurrentSpeed, DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                HR = Graph.DVDCtrl.PlayBackwards(CurrentSpeed, DvdCmdFlags.Flush, cmdOption)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = Graph.KO_IKeystone.ActivateFFRW(CurrentSpeed)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                PlayState = ePlayState.Rewinding
                Me.Mute()
                Me.SetOSD(OSDBitmap_FFRWSpeed(Math.Round(CurrentSpeed, 0), False), RGB(255, 255, 255), 90, False, 1, "RW", Me.TitleSafeLeft, Me.TitleSafeTop)
                RaiseEvent evRewind(CurrentSpeed)
            Catch ex As Exception
                If CheckEx(ex, "Rewind") Then Exit Function
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem rewinding. error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

        Public Sub SlowForward()
            Try
                If PlayState <> ePlayState.Playing And PlayState <> ePlayState.Paused Then Exit Sub
                Graph.KeystoneOmni_Unpause()
                ActivateVarispeed(4)
                PlayState = ePlayState.VariSpeed
            Catch ex As Exception
                If CheckEx(ex, "Slow Forward") Then Exit Sub
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem slow forwarding. error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Sub [Resume]()
            Try
                If Me.PlayState = ePlayState.SystemJP Then Exit Sub
                If PlayState = ePlayState.FrameStepping Then Me.QuitFrameStepping()
                If PlayState = ePlayState.VariSpeed Then DeactivateVarispeed()
                If PlayState = ePlayState.ABLoop Then Me.ABLoop_Clear()
                Graph.KeystoneOmni_Unpause()
                'If Not InStr(playState.ToString.ToLower, "menu", CompareMethod.Text) Then Exit Sub
                HR = Graph.DVDCtrl.Resume(DvdCmdFlags.SendEvt, cmdOption)
                If HR = -2147220846 Then
                    RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "Resume information not available.", Nothing, Nothing)
                    '_ResumeDataExists = False
                    Exit Sub
                End If
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                '_ResumeDataExists = False
            Catch ex As Exception
                If CheckEx(ex, "Resume") Then Exit Sub
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem resuming. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Sub GoUp()
            Try
                If Me.PlayState = ePlayState.SystemJP Then Exit Sub
                If PlayState = ePlayState.VariSpeed Then DeactivateVarispeed()
                If PlayState = ePlayState.ABLoop Then Me.ABLoop_Clear()
                Graph.KeystoneOmni_Unpause()
                HR = Graph.DVDCtrl.ReturnFromSubmenu(DvdCmdFlags.SendEvt, cmdOption)
                If HR = -2147220855 Then
                    RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "No GoUp PGC programmed.", Nothing, Nothing)
                    Exit Sub
                End If
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                If CheckEx(ex, "GoUp") Then Exit Sub
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem going up. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Overrides Function JumpBack(ByVal JumpSeconds As Byte) As Boolean
            Try
                If Me.PlayState = ePlayState.SystemJP Then Return True
                If Not GetCommandSyncMutex() Then Exit Function
                Graph.KeystoneOmni_Unpause()
                If PlayState = ePlayState.FrameStepping Then
                    If Not QuitFrameStepping() Then Throw New Exception("QuitFrameStepping Failed in JumpBack.")
                End If
                If PlayState = ePlayState.VariSpeed Then DeactivateVarispeed()

                Dim NewTC As DvdTimeCode = CurrentRunningTime_DVD
                Dim cH As Integer = NewTC.bHours
                Dim cM As Integer = NewTC.bMinutes
                Dim cS As Integer = NewTC.bSeconds
                Dim cF As Integer = NewTC.bFrames

                Dim SecondsIn As UInt32 = DVDTimecodeToSeconds(NewTC)

                If SecondsIn <= JumpSeconds Then
                    'go to beginning of stream
                    NewTC.bFrames = 0
                    NewTC.bSeconds = 0
                    NewTC.bMinutes = 0
                    NewTC.bHours = 0
                    Graph.DVDCtrl.PlayAtTime(NewTC, DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                Else
                    Dim tTC As cTimecode = SubtractTimecode(New cTimecode(NewTC, (Me.CurrentVideoStandard = eVideoStandard.NTSC)), New cTimecode(JumpSeconds, eFramerate.NTSC), eFramerate.NTSC)
                    Debug.WriteLine("jumping back:" & tTC.ToString())
                    HR = Graph.DVDCtrl.PlayAtTime(tTC.DVDTimeCode, DvdCmdFlags.Flush, cmdOption)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End If
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with JumpBack(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

        Public Sub JumpToEndOfTitle()
            Try
                If Me.PlayState = ePlayState.SystemJP Then Exit Sub
                If Not GetCommandSyncMutex() Then Exit Sub
                If PlayState = ePlayState.VariSpeed Then DeactivateVarispeed()
                If PlayState = ePlayState.FrameStepping Then QuitFrameStepping()
                If PlayState = ePlayState.ABLoop Then Me.ABLoop_Clear()
                Graph.KeystoneOmni_Unpause()
                If Not DVDIsInStill And (ePlayState.Playing Or ePlayState.Paused) Then
                    Dim TempTC As New cTimecode(CurrentTitleTRT, (Me.CurrentVideoStandard = eVideoStandard.NTSC))

                    Dim T() As String = Split(TempTC.ToString_NoFrames, ":", -1, CompareMethod.Text)
                    Dim TC As New DvdTimeCode

                    Dim BackBy As Short = 10
                    'BackBy = JumpSeconds
                    If T(2) >= 10 Then
                        'seconds are more than 10, so just subtract and go
                        T(2) = T(2) - BackBy
                    Else
                        If T(1) >= 1 Then
                            'minutes are more than one
                            T(1) = T(1) - 1
                            T(2) = 60 - T(2) - BackBy
                        Else
                            'minutes are less than one
                            If T(0) >= 1 Then
                                'hours are more than one
                                T(0) = T(0) - 1
                                T(1) = T(1) + 59
                                T(2) = 60 - T(2) - BackBy
                            Else
                                'hours are less than one, just leave, the title is too short for this operation
                                Exit Sub
                            End If
                        End If
                    End If
                    TC.bHours = T(0)
                    TC.bMinutes = T(1)
                    TC.bSeconds = T(2)
                    TC.bFrames = 1
                    PlayAtTime(TC, True)
                End If
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem jumping to end of title. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Sub GoToBeginningOfCurrentChapter()
            Try
                If Me.PlayState = ePlayState.SystemJP Then Exit Sub
                If PlayState = ePlayState.VariSpeed Then DeactivateVarispeed()
                Graph.KeystoneOmni_Unpause()
                If CurrentDomain = DvdDomain.Title Then
                    Me.PlayChapter(CurrentChapter, False)
                    Me.UnMute()
                    Me.ReSyncAudio(False)
                Else
                    RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "Go to chapter start is only valid in the title domain.", Nothing, Nothing)
                End If
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with GoToBeginningOfCurrentChapter.", ex.Message, Nothing)
            End Try
        End Sub

        Public Sub GoToPreChapterStart(ByVal NextChapter As Boolean, ByVal JumpSeconds As Short)
            Try
                If Me.PlayState = ePlayState.SystemJP Then Exit Sub
                If PlayState = ePlayState.VariSpeed Then DeactivateVarispeed()
                Graph.KeystoneOmni_Unpause()
                If CurrentDomain = DvdDomain.Title And _UOPs(0) Then
                    If Not NextChapter And CurrentChapter = 1 Then Exit Sub

                    Dim PGC As cPGC = CurrentDVD.FindPGCByGTT(CurrentTitle)
                    Dim VTS As cVTS = CurrentDVD.VTSs(PGC.ParentVTS - 1)

                    '1) Determine which PGC the next chapter is in
                    Dim off As Short
                    If NextChapter Then
                        off = 1
                    Else
                        off = 2
                    End If
                    Dim PTT As cPTT = VTS.Titles(0).PTTs(CurrentChapter - off)

                    '2) Get the Cell number for the target program
                    Dim Cell As Integer = PGC.ProgramMap(PTT.PGN - 1).CellNo

                    '3) Determine running time for cell
                    Dim TC As cTimecode = CurrentDVD.GetTRTFromCellColl(Cell, PGC.CellPlaybackInfo)

                    TC = SubtractTimecode(TC, New cTimecode(0, 0, JumpSeconds, 0, (CurrentVideoStandard = eVideoStandard.NTSC)), 30)

                    If TC.Frames > 28 Then
                        TC.Frames = 28
                    End If

                    HR = Graph.DVDCtrl.PlayAtTime(TC.DVDTimeCode, DvdCmdFlags.Flush Or DvdCmdFlags.Block, cmdOption)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Else
                    RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "Goto pre-chapter start only applies to title domain.", Nothing, Nothing)
                End If
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with GoToPreChapterStart.", ex.Message, Nothing)
            End Try
        End Sub

        Public Sub GoToLayerbreak()
            If PlayState = ePlayState.ABLoop Then Me.ABLoop_Clear()
            Graph.KeystoneOmni_Unpause()
            'Try
            'If PlayState = ePlayState.VariSpeed Then DeactivateVarispeed()
            '    If Me.PlayState = ePlayState.SystemJP Then Exit Sub
            '    If colNSCs.CandidateLayerbreaks = 0 Then
            '        RM.gbLayerbreak.Enabled = False
            '        Exit Sub
            '    ElseIf colNSCs.CandidateLayerbreaks > 1 Then
            '        'more than one candidate, has user selected one of them to use?
            '        If Not colNSCs.LayerbreakConfirmed Then
            '            'show selector to force a choice
            '            Dim dlg As New frmLayerbreakSelector()
            '            dlg.SetupLayerbreaks(colNSCs)
            '            'Me.colNSCs = colNSCs
            '            If Not dlg.ShowDialog = DialogResult.OK Then Exit Sub
            '        End If
            '    End If

            '    If Layerbreak Is Nothing Then
            '        RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "No layerbreak available.", Nothing, Nothing)
            '        Exit Sub
            '    End If

            '    Dim TC As New DvdTimeCode
            '    TC.bHours = Layerbreak.tcLB.Hours
            '    TC.bMinutes = Layerbreak.tcLB.Minutes
            '    If Layerbreak.tcLB.Seconds < 10 Then
            '        If Layerbreak.tcLB.Minutes < 1 Then
            '            If Layerbreak.tcLB.Hours < 1 Then
            '                Throw New Exception("Not enough timecode available before layerbreak for accurate simulation.")
            '                Exit Sub
            '            Else
            '                TC.bHours = Layerbreak.tcLB.Hours - 1
            '                TC.bMinutes = Layerbreak.tcLB.Minutes + 59
            '                TC.bSeconds = Layerbreak.tcLB.Seconds + 59
            '                TC.bSeconds -= 10
            '            End If
            '        Else
            '            TC.bMinutes = Layerbreak.tcLB.Minutes - 1
            '            TC.bSeconds = Layerbreak.tcLB.Seconds + 59
            '            TC.bSeconds -= 10
            '        End If
            '    Else
            '        TC.bSeconds = Layerbreak.tcLB.Seconds - 10
            '    End If
            '    TC.bFrames = Layerbreak.tcLB.Frames

            '    'Go to LB
            '    Try
            '        hr = Graph.DVDCtrl.PlayAtTimeInTitle(Layerbreak.GTTn, TC, DvdCmdFlags.None, cmdOption)
            '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            '    Catch ex As Exception
            '        If InStr(ex.Message, "80040276", CompareMethod.Text) Then
            '            'uop zero is disabled, just play the chapter
            '            RaiseEvent evConsoleLine(eConsoleItemType.WARNING, "UOP 0 is disabled, unable to time search for layerbreak.  Going to chapter that ends with layerbreak.", Nothing, Nothing)
            '            PlayChapter(Layerbreak.PTT, False)
            '            Exit Try
            '        ElseIf InStr(ex.Message, "80040277", CompareMethod.Text) Then
            '            WhiteHand("PlayAtTime")
            '            Exit Sub
            '        End If
            '        RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem going to layerbreak. error: " & ex.Message, Nothing, Nothing)
            '    End Try

            '    'PlayAtTime(TC)
            '    _DVDIsInStill = False
            '    PingLB()
            'Catch ex As Exception
            '    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem going to layerbreak. Error: " & ex.Message, Nothing, Nothing)
            'End Try
        End Sub

        Public Sub PlayAtDetailedLocation(ByVal Loc As cDetailedPlayLocation)
            Try
                If PlayState = ePlayState.ABLoop Then Me.ABLoop_Clear()
                Graph.KeystoneOmni_Unpause()
                If Loc.Domain = DvdDomain.VideoManagerMenu Or Loc.Domain = DvdDomain.VideoTitleSetMenu Then
                    'search target is in the video manager
                    'search target is in menu space of a vts
                    Me.GoToMenu(DvdMenuID.Root)
                Else
                    'search target is in title space
                    Me.PlayChapterInTitle(Loc.GTTN, Loc.PTTN)
                End If
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with PlayAtDetailedLocation(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Overrides Function FrameStep() As Boolean
            Try
                If Not PlayState = ePlayState.FrameStepping Then
                    _FramesStepped = 0
                    If Not PlayState = ePlayState.Playing Then
                        Play()
                        Thread.Sleep(333)
                    End If
                End If
                Me.Mute()

                PlayState = ePlayState.FrameStepping

                If AVMode = eAVMode.DesktopVMR Then
                    Graph.VideoStep.Step(1, Nothing)
                Else
                    _FramesStepped += 1
                    If _FramesStepped > 50 Then
                        HR = Graph.DVDCtrl.Pause(False)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                        Threading.Thread.Sleep(333)
                        _FramesStepped = 0
                    End If
                    HR = Graph.DVDCtrl.Pause(True)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    HR = Graph.KO_IKeystone.FrameStep(True)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End If

            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with FrameStep. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function
        Private _FramesStepped As Byte

        Public Overrides Function QuitFrameStepping() As Boolean
            Try
                PlayState = ePlayState.Playing

                If AVMode = eAVMode.DesktopVMR Then
                    HR = Graph.VideoStep.CancelStep()
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    HR = Graph.MediaCtrl.Run()
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Else
                    HR = Graph.DVDCtrl.Pause(False)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    HR = Graph.KO_IKeystone.QuitFrameStepping
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    'Graph.DVDCtrl.PlayForwards(1.0, DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                    Threading.Thread.Sleep(200)
                    Me.ReSyncAudio(False)
                    'Dim tloc As New DvdPlayLocation
                    'Graph.DVDInfo.GetCurrentLocation(tloc)
                    'Graph.DVDCtrl.PlayAtTime(tloc.timeCode, DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                End If

                Me.UnMute()
                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with QuitFrameStepping. Error: " & ex.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

#End Region 'API:METHODS:PLAY CONTROL:CORE

#Region "API:METHODS:PLAY CONTROL:MENUS"

        Public Sub GoToMenu(ByVal WhichMenu As DvdMenuID, Optional ByVal RootAsResume As Boolean = False)
            Try
                If Graph.DVDCtrl Is Nothing Then Exit Sub

                If PlayState = ePlayState.FrameStepping Then QuitFrameStepping()
                If PlayState = ePlayState.VariSpeed Then Me.Play()

                Me.UnMute()

                ClearOSD()

                If CurrentDomain = DvdDomain.Stop Then
                    PlayState = ePlayState.Playing
                    HR = Graph.DVDCtrl.ShowMenu(WhichMenu, DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    RaiseEvent evVTSCount_SET(Me.VTSCount)
                    Exit Sub
                End If

                If Not PlayState = ePlayState.Playing Then
                    Play()
                End If

                'resume from a menu
                '080618 - should only resume if ... ?
                If (Me.CurrentDomain = DvdDomain.VideoTitleSetMenu And WhichMenu = DvdMenuID.Root And RootAsResume) Then
                    [Resume]()
                    Exit Sub
                End If

                HR = Graph.DVDCtrl.ShowMenu(WhichMenu, DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)


                'Select Case WhichMenu
                '    Case DvdMenuID.Angle
                '        Me.AddMacroItem(eMacroCommand.AngleMenu)
                '    Case DvdMenuID.Audio
                '        Me.AddMacroItem(eMacroCommand.AudioMenu)
                '    Case DvdMenuID.Chapter
                '        Me.AddMacroItem(eMacroCommand.SceneMenu)
                '    Case DvdMenuID.Root
                '        Me.AddMacroItem(eMacroCommand.RootMenu)
                '    Case DvdMenuID.Subpicture
                '        Me.AddMacroItem(eMacroCommand.SubtitleMenu)
                '    Case DvdMenuID.Title
                '        Me.AddMacroItem(eMacroCommand.TitleMenu)
                'End Select

            Catch ex As Exception
                If CheckEx(ex, "Go To Menu") Then Exit Sub
                If InStr(ex.Message, "80040282") Then
                    RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, WhichMenu.ToString & " does not exist. Menu operation canceled.", Nothing, Nothing)
                    Exit Sub
                End If
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem going to " & WhichMenu.ToString.ToLower & " menu. Error : " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Sub DirectionalBtnHit(ByVal Hit As DvdRelButton)
            Try
                Graph.DVDCtrl.SelectRelativeButton(Hit)

                Select Case Hit
                    Case DvdRelButton.Left
                        Me.AddMacroItem(eMacroCommand.Left)
                    Case DvdRelButton.Lower
                        Me.AddMacroItem(eMacroCommand.Down)
                    Case DvdRelButton.Right
                        Me.AddMacroItem(eMacroCommand.Right)
                    Case DvdRelButton.Upper
                        Me.AddMacroItem(eMacroCommand.Up)
                End Select

                'BtnQ.Enqueue(Hit)
                'HitCount += 1
                'If BtnThread Is Nothing Then
                '    BtnThread = New Thread(AddressOf Me.StartBtnPoller)
                '    BtnThread.Name = "BtnThread"
                '    BtnThread.Start()
                '    Exit Sub
                'End If
                ''If BtnThread.ThreadState <> ThreadState.Running Then
                ''    BtnThread = New Thread(AddressOf Me.StartBtnPoller)
                ''    BtnThread.Name = "BtnThread"
                ''    BtnThread.Start()
                ''    Exit Sub
                ''End If
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "DirectionalBtnHit failed. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Sub EnterBtn()
            Try
                If PlayState = ePlayState.FrameStepping Then Me.QuitFrameStepping()
                If MenuMode = eMenuMode.Buttons And Not (Graph.DVDCtrl Is Nothing) Then
                    HR = Graph.DVDCtrl.ActivateButton()
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    Me.AddMacroItem(eMacroCommand.Enter)
                Else
                    If MenuMode = eMenuMode.Still And Not (Graph.DVDCtrl Is Nothing) Then
                        Graph.DVDCtrl.StillOff()
                    End If
                End If
            Catch ex As Exception
                If CheckEx(ex, "Enter Button") Then Exit Sub
            End Try
        End Sub

#End Region 'API:METHODS:PLAY CONTROL:MENUS

#Region "API:METHODS:PLAY CONTROL:AUDIO"

        Public Sub ReSyncAudio(ByVal BackupFiveSecs As Boolean)
            Try
                UpdateUOPs()
                Thread.Sleep(250)
                If _UOPs(5) Then
                    'DO A TIME SEARCH RE-SYNC
                    Dim tloc As New DvdPlayLocation
                    Graph.DVDInfo.GetCurrentLocation(tloc)

                    Dim tc As cTimecode
                    If BackupFiveSecs Then
                        tc = SubtractTimecode(New cTimecode(tloc.timeCode, (Me.CurrentVideoStandard = eVideoStandard.NTSC)), New cTimecode(0, 0, 5, 0, (CurrentVideoStandard = eVideoStandard.NTSC)), CurrentTargetFrameRate)
                    Else
                        tc = New cTimecode(tloc.timeCode, (Me.CurrentVideoStandard = eVideoStandard.NTSC))
                    End If

                    Me.PlayAtTime(tc.DVDTimeCode, True)

                ElseIf _UOPs(9) Then
                    'DO A SHORT REWIND RESYNC
                    HR = Graph.DVDCtrl.PlayBackwards(4.0, DvdCmdFlags.None, cmdOption)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    Thread.Sleep(250)
                    HR = Graph.DVDCtrl.PlayForwards(1.0, DvdCmdFlags.Flush, cmdOption)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                End If

            Catch ex As Exception
                If InStr(ex.Message, "80040277") Then
                    RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "Cannot resync currently due to domain restrictions.", Nothing, Nothing)
                    Exit Sub
                End If
                If InStr(ex.Message, "80040276") Then
                    RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "Cannot resync currently due to UOP restrictions. Try again when ""Time Play"" is permitted", Nothing, Nothing)
                    Exit Sub
                End If
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with ReSyncAudio. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Muting As Boolean = False
        Public Sub Mute()
            Try
                Graph.nvAudioAtts.SetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_CONFIG, nvcommon.ENvidiaAudioDecoderProps_Config.NVAUDDEC_CONFIG_MUTE, 1)
                'RM.btnMute.BackColor = SystemColors.ControlDarkDark
                'RM.btnMute.FlatStyle = FlatStyle.Flat
                Muting = True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem muting. error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Sub UnMute()
            Try
                Graph.nvAudioAtts.SetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_CONFIG, nvcommon.ENvidiaAudioDecoderProps_Config.NVAUDDEC_CONFIG_MUTE, 0)
                'RM.btnMute.BackColor = SystemColors.Control
                'RM.btnMute.FlatStyle = FlatStyle.Standard
                Muting = False
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem unmuting. error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Sub CycleAudio()
            Try
                If Me.PlayState = ePlayState.SystemJP Then Exit Sub
                If Graph.DVDCtrl Is Nothing Or Graph.DVDInfo Is Nothing Then Exit Sub
                Dim NumberOfStreams, CurrentStream As Integer
                HR = Graph.DVDInfo.GetCurrentAudio(NumberOfStreams, CurrentStream)
                If CurrentStream = NumberOfStreams - 1 Then
                    CurrentStream = 0
                Else
                    CurrentStream += 1
                End If

                UnMute()
                If CheckIfStreamIsDTS(CurrentStream) Then
                    'If Not xAD.aDTS Then
                    '    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "DTS audio is not licensed.")
                    '    Mute()
                    'End If
                End If

                HR = Graph.DVDCtrl.SelectAudioStream(CurrentStream, DvdCmdFlags.None, cmdOption)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                CloseAudDumpFile()
                RaiseEvent evAudioCycled()
            Catch ex As Exception
                If InStr(ex.Message.ToLower, "8004028f", CompareMethod.Text) Then
                    Graph.DVDCtrl.SelectAudioStream(0, DvdCmdFlags.None, cmdOption)
                    Exit Sub
                End If
                If CheckEx(ex, "Toggle Audio") Then Exit Sub
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem cycling audio: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Sub SetAudioStream(ByVal StreamNumber As Short)
            Try
                If Graph.DVDCtrl Is Nothing Or Graph.DVDInfo Is Nothing Then Exit Sub
                UnMute()
                If CheckIfStreamIsDTS(StreamNumber) Then
                    'If Not xAD.aDTS Then
                    '    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "DTS audio is not licensed.")
                    '    Mute()
                    'End If
                End If

                HR = Graph.DVDCtrl.SelectAudioStream(StreamNumber, DvdCmdFlags.None, cmdOption)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                RaiseEvent evAudioStreamSet(StreamNumber)

                CloseAudDumpFile()
            Catch ex As Exception
                If CheckEx(ex, "Set Audio") Then Exit Sub
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem toggling audio: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Function GetAudio(ByVal StreamNumber As Short) As seqAudio
            Try
                Dim att As DvdAudioAttr
                Graph.DVDInfo.GetAudioAttributes(StreamNumber, att)
                Dim txt As New StringBuilder(600)
                Dim A As New seqAudio
                With A
                    .AppMode = att.appMode.ToString
                    .Extension = GetAudioExtensionName(att.languageExtension)
                    .Format = att.audioFormat.ToString
                    .Frequency = att.frequency.ToString
                    .Language = GetLanguageByLCID(CLng(att.language))
                    .NumberOfChannels = att.numberOfChannels.ToString
                    .Quantization = att.quantization.ToString
                    .StreamNumber = StreamNumber
                    ._AppModeData = att.appModeData
                    .HasMultiChannelInfo = att.hasMultichannelInfo
                    ._Available = Me.IsAudioStreamAvailable(StreamNumber)

                    If .Format.ToLower = "dts" And CByte(.NumberOfChannels) = 6 Then
                        .Format = "DTS-ES"
                    End If

                    ''commented 080620 in favor of doing this elsewhere (accurately, this was wrong)
                    ''how to tell when it is Dolby EX? AC3-EX?
                    'If .Format.ToLower = "ac3" And CByte(.NumberOfChannels) = 6 Then
                    '    .Format = "AC3-EX"
                    'End If

                End With
                Return A
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem getting audio stream info. error: " & ex.Message, Nothing, Nothing)
                Return Nothing
            End Try
        End Function

#End Region 'API:METHODS:PLAY CONTROL:AUDIO

#Region "API:METHODS:PLAY CONTROL:SUBTITLES"

        Public Sub CycleSubtitles()
            Try
                If Me.PlayState = ePlayState.SystemJP Then Exit Sub
                If Graph.DVDCtrl Is Nothing Or Graph.DVDInfo Is Nothing Then Exit Sub

                Dim NumberOfStreams, CurrentStream As Integer
                Dim SubsAreOff As Boolean
                Dim hr As Integer = Graph.DVDInfo.GetCurrentSubpicture(NumberOfStreams, CurrentStream, SubsAreOff)

                'new logic per Sony Oct 7 2008
                If SubsAreOff Then
                    ToggleSubtitles(True)
                    Exit Sub
                Else
                    If CurrentStream = NumberOfStreams - 1 Then
                        CurrentStream = 0
                    Else
                        CurrentStream += 1
                    End If
                End If

                'If CurrentStream = 0 And Not SubtitlesAreOn Then
                '    ToggleSubtitles(True)
                'ElseIf CurrentStream = 0 And SubtitlesAreOn Then
                '    CurrentStream += 1
                'ElseIf CurrentStream = NumberOfStreams - 1 Then
                '    ToggleSubtitles(False)
                '    CurrentStream = 0
                'Else
                '    CurrentStream += 1
                'End If

                'RaiseEvent evSubtitlesCycled()

                hr = Graph.DVDCtrl.SelectSubpictureStream(CurrentStream, DvdCmdFlags.SendEvt, cmdOption)
                If hr < 0 Then Marshal.ThrowExceptionForHR(hr)
            Catch ex As Exception
                If InStr(ex.Message.ToLower, "8004028f", CompareMethod.Text) Then
                    'ToggleSubtitles(False)
                    Graph.DVDCtrl.SelectSubpictureStream(0, DvdCmdFlags.SendEvt, cmdOption)
                    Exit Sub
                End If
                If CheckEx(ex, "Toggle Subtitles") Then Exit Sub
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem cycling audio: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Sub ToggleSubtitles(ByVal TurnOn As Boolean)
            Try
                If Me.PlayState = ePlayState.SystemJP Then Exit Sub
                If Graph.DVDCtrl Is Nothing Then Exit Sub
                HR = Graph.DVDCtrl.SetSubpictureState(TurnOn, DvdCmdFlags.SendEvt, cmdOption)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                '_SubtitlesAreOn = TurnOn
                'RaiseEvent evSubtitlesToggled(TurnOn)
            Catch ex As Exception
                If CheckEx(ex, "Turn off subtitles") Then Exit Sub
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem turning subs on/off: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Sub SetSubtitleStream(ByVal StreamNumber As Short, ByVal DoToggle As Boolean, ByVal TurnOn As Boolean)
            Try
                If Graph.DVDCtrl Is Nothing Or Graph.DVDInfo Is Nothing Then Exit Sub
                If CurrentDomain <> DvdDomain.Title Then Exit Sub

                'ToggleSubtitles(True)
                HR = Graph.DVDCtrl.SelectSubpictureStream(StreamNumber, DvdCmdFlags.SendEvt, cmdOption)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                If DoToggle Then
                    ToggleSubtitles(TurnOn)
                End If

                RaiseEvent evSubtitleStreamSet(StreamNumber)

            Catch ex As Exception
                If InStr(ex.Message.ToLower, "8004028f", CompareMethod.Text) Then
                    ToggleSubtitles(False)
                    Graph.DVDCtrl.SelectSubpictureStream(0, DvdCmdFlags.SendEvt, cmdOption)
                    RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "Subtitle stream " & StreamNumber & " is not available in the current title.", Nothing, Nothing)
                    Exit Sub
                End If
                If CheckEx(ex, "Set subtitles") Then Exit Sub
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem toggling subtitle: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'API:METHODS:PLAY CONTROL:SUBTITLES

#Region "API:METHODS:PLAY CONTROL:ANGLE"

        Public Sub CycleAngle()
            Try
                If Me.PlayState = ePlayState.SystemJP Then Exit Sub
                If Graph.DVDCtrl Is Nothing Or Graph.DVDInfo Is Nothing Then Exit Sub
                If CurrentTitleAngleCount < 2 Then Throw New Exception("Only one angle available.")
                Dim NumberOfStreams, CurrentStream As Integer
                HR = Graph.DVDInfo.GetCurrentAngle(NumberOfStreams, CurrentStream)

                If CurrentAngleStream = NumberOfStreams Then
                    _CurrentAngleStream = 1
                Else
                    _CurrentAngleStream += 1
                End If
                CurrentStream = CurrentAngleStream
                HR = Graph.DVDCtrl.SelectAngle(CurrentStream, DvdCmdFlags.SendEvt, cmdOption)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                RaiseEvent evAngleCycled(CurrentStream)
            Catch ex As Exception
                If CheckEx(ex, "Toggle Angle") Then Exit Sub
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem cycling angle: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'API:METHODS:PLAY CONTROL:ANGLE

#Region "API:METHODS:PLAY CONTROL:CLOSED CAPTIONS"

        Public Function ToggleClosedCaptions() As Boolean
            Try
                If Me.PlayState = ePlayState.SystemJP Then Exit Function
                If ClosedCaptionsAreOn Then
                    Me.TurnOffCCs()
                Else
                    Me.TurnOnCCs()
                End If
                RaiseEvent evClosedCaptionToggle()
                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with UserToggleCCs. Error: " & ex.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

#End Region 'API:METHODS:PLAY CONTROL:CLOSED CAPTIONS

#Region "API:METHODS:PLAY CONTROL:AB LOOP"

        Private ABLoop_PauseFlag As Boolean = False

        Public Function ABLoop_SetA() As Boolean
            Try
                If Me.CurrentDomain <> DvdDomain.Title Then Return False
                If Me.PlayState <> ePlayState.Playing Then Return False
                If Not Me.CurrentUserOperations(0) Then
                    RaiseEvent evConsoleLine(eConsoleItemType.WARNING, "Time search UOP is prohibited. AB Loop is impossible.", Nothing, Nothing)
                    Return False
                End If
                Me._ABLoop_PositionA = Me.CurrentPlayLocation
                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "ABLoop_SetA. Error: " & ex.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

        Public Sub ABLoop_SetB(ByVal Grab As Boolean, ByVal Format As System.Drawing.Imaging.ImageFormat)
            Try
                If Me.PlayState <> ePlayState.Playing Then Exit Sub
                Me._ABLoop_PositionB = Me.CurrentPlayLocation
                Me.PlayState = ePlayState.ABLoop
                If Grab Then
                    Dim TC2 As New cTimecode(Me.ABLoop_PositionB.timeCode, Me.CurrentVideoStandard = eVideoStandard.NTSC)
                    TC2.Subtract(Me.ABLoop_PositionA.timeCode.bHours, Me.ABLoop_PositionA.timeCode.bMinutes, Me.ABLoop_PositionA.timeCode.bSeconds, Me.ABLoop_PositionA.timeCode.bFrames)
                    Me.MultiFrameGrab(TC2.TotalSeconds * 25, Format) 'this is a bit rough for the frame count but who cares. they may get a few frames less than they expected. (if this were set to 30 they'd get some of the frames again from the top if the content is PAL and we don't want this. it would confuse them.
                End If
                Me.PlayAtTime_Clean(Me.ABLoop_PositionA.timeCode, False)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "ABLoop_SetA. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Sub ABLoop_Clear()
            Me.PlayState = ePlayState.Playing
            Me._ABLoop_PositionA = Nothing
            Me._ABLoop_PositionB = Nothing
            RaiseEvent evABLoopCleared()
        End Sub

#End Region 'API:METHODS:PLAY CONTROL:AB LOOP

#End Region 'API:METHODS:PLAY CONTROL

#Region "API:METHODS:MENUS"

        Public Function InputMousePosition(ByVal X As Short, ByVal Y As Short) As Boolean
            Try
                If Graph.DVDCtrl Is Nothing Or MenuMode <> eMenuMode.Buttons Then Exit Function
                Dim pt As DsPOINT
                pt.X = X
                pt.Y = Y
                HR = Graph.DVDCtrl.SelectAtPosition(pt)
                If HR = 0 Then
                    Return True
                ElseIf HR = -2147220872 Then
                    Return False
                End If
            Catch ex As Exception
                Throw New Exception("Problem with InputMousePosition(). Error: " & ex.Message)
            End Try
        End Function

        Public Sub ActivateButtonAt(ByVal X As Short, ByVal Y As Short)
            Try
                If Graph.DVDCtrl Is Nothing Or MenuMode <> eMenuMode.Buttons Then Exit Sub
                Dim pt As DsPOINT
                pt.X = X
                pt.Y = Y
                Graph.DVDCtrl.ActivateAtPosition(pt)
            Catch ex As Exception
                Throw New Exception("Problem with ActivateButtonAt(). Error: " & ex.Message)
            End Try
        End Sub

        Public Sub NumberEntry(ByVal Val As Byte)
            Try
                HR = Graph.DVDCtrl.SelectButton(Val)
                If HR = -2147220872 Then
                    RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "Button #" & Val & " is not available.", Nothing, Nothing)
                    Exit Sub
                End If
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                Throw New Exception("Problem with NumberEntry(). Error: " & ex.Message, ex)
            End Try
        End Sub

#End Region 'API:METHODS:MENUS

#Region "API:METHODS:VIDEO"

        Public Function GetVideoAttributes() As DvdVideoAttr
            Dim out As DvdVideoAttr
            HR = Graph.DVDInfo.GetCurrentVideoAttributes(out)
            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

            _CurrentVideoDimensions = New Point(out.sourceResolutionX, out.sourceResolutionY)
            If out.aspectX = 16 Then
                _CurrentAspectRatio = eAspectRatio.ar16x9
            ElseIf _CurrentAspectRatio <> eAspectRatio.ar4x3 Then
                _CurrentAspectRatio = eAspectRatio.ar4x3
            End If

            LB_OK = out.letterboxPermitted
            PS_OK = out.panscanPermitted

            Return out
        End Function

        Private CurrentBitrate As Integer = 0
        Public Sub GetBitrate()
            Try
                If Graph.nvVideoAtts Is Nothing Then Exit Sub
                Graph.nvVideoAtts.GetLong(0, nvcommon.ENvVideoDecoderProps_StatsTypeIndexes.NVVIDDEC_STATS_BITRATE, CurrentBitrate)
                RaiseEvent evCurrentBitrateNotification(CurrentBitrate)
            Catch ex As Exception
                If InStr(ex.Message.ToLower, "overflow") Then Exit Sub
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem getting current bitrate. error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private CurrentDroppedFrames As Integer = 0
        Public Sub GetDroppedFrames()
            Try
                If Graph.nvVideoAtts Is Nothing Then Exit Sub
                Graph.nvVideoAtts.GetLong(0, nvcommon.ENvVideoDecoderProps_StatsTypeIndexes.NVVIDDEC_STATS_FRAME_DROPS, CurrentDroppedFrames)
                If CurrentDroppedFrames = 1 Then CurrentDroppedFrames = 0
                'Debug.WriteLine("dropped frames:" & CurrentDroppedFrames)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem getting dropped frames. error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Function GetKeystoneFramerate(Optional ByVal CurrentFramerate As String = "") As Double
            Try
                Dim TargFR As Single = 0
                HR = Graph.KO_IKeystoneQuality.get_TargetFR_In(TargFR)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim sTargFR As String = CStr(TargFR)

                If sTargFR <> CurrentFramerate Then
                    RaiseEvent evFrameRateChange(sTargFR)
                End If

                Return TargFR
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with GetFramerate. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

        Public Sub BumpFieldsDown(ByVal DoBump As Boolean)
            Try
                HR = Graph.KO_IKeystoneMixer.BumpFieldsDown(DoBump)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                BumpingFieldsDown = DoBump
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with BumpFieldsDown. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Sub SetMPEGFrameMode(ByVal Mode As Short)
            Try
                Select Case Mode
                    Case 1
                        Graph.nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_VIDEO_MODE, nvcommon.ENVVIDDEC_CONFIG_VIDEO_MODETypes.NVVIDDEC_CONFIG_IBP_VIDEO_MODE)
                        Graph.KO_IKeystone.Set32Status(1)
                    Case 2
                        Graph.nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_VIDEO_MODE, nvcommon.ENVVIDDEC_CONFIG_VIDEO_MODETypes.NVVIDDEC_CONFIG_IP_VIDEO_MODE)
                        Graph.KO_IKeystone.Set32Status(0)
                    Case 3
                        Graph.nvVideoAtts.SetLong(nvcommon.ENvVideoDecoderProps.NVVIDDEC_CONFIG, nvcommon.ENvidiaVideoDecoderProps_ConfigTypes.NVVIDDEC_CONFIG_VIDEO_MODE, nvcommon.ENVVIDDEC_CONFIG_VIDEO_MODETypes.NVVIDDEC_CONFIG_I_VIDEO_MODE)
                        Graph.KO_IKeystone.Set32Status(0)
                End Select
                CurrentMPEGFrameMode = Mode
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with SetMPEGFrameMode. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Sub GrabScreen(ByVal FrameGrabFormat As System.Drawing.Imaging.ImageFormat, ByVal FrameGrabSource As eFrameGrabContent, ByVal BurnInTimecode As Boolean)
            Try
                'If Not xAD.aFrameGrab Then
                '    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Frame grabbing is not licensed.")
                '    Exit Sub
                'End If

                'Get the RGB24 bits from Keystone
                Dim Buffer() As Byte = GetSampleFromKeystone(FrameGrabSource)

                If Buffer Is Nothing Then
                    Exit Sub
                End If

                'Make the bitmap object
                Dim MS As New MemoryStream(Buffer.Length)
                MS.Write(Buffer, 0, Buffer.Length)
                Dim BM As New Bitmap(MS)

                'BM.Save("c:\temp\bm.bmp")

                'Scale image as needed
                If Me.CurrentVideoStandard = eVideoStandard.NTSC And CurrentAspectRatio = eAspectRatio.ar16x9 And Not BM.Width = 640 Then
                    If Me.DefaultAspectRatio = ePreferredAspectRatio.Anamorphic Then
                        BM = ScaleImage(BM, 480, 853)
                    Else
                        BM = ScaleImage(BM, 480, 720)
                    End If
                ElseIf Me.CurrentVideoStandard = eVideoStandard.PAL And CurrentAspectRatio = eAspectRatio.ar16x9 And Not BM.Width = 640 Then
                    If Me.DefaultAspectRatio = ePreferredAspectRatio.Anamorphic Then
                        BM = ScaleImage(BM, 576, 853)
                    Else
                        BM = ScaleImage(BM, 576, 720)
                    End If
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
                Dim OutPath As String = Me.FrameGrabDumpDir
                If Not Directory.Exists(OutPath) Then Directory.CreateDirectory(OutPath)
                Dim cp As String = CurrentTitlePosition_AsString()
                If InStr(cp.ToLower, "tt-m", CompareMethod.Text) Then
                    cp = "MenuSpace_" & DateTime.Now.Hour & "." & DateTime.Now.Minute & "." & DateTime.Now.Second
                End If

                'Get desired format
                Dim OutFormat As Imaging.ImageFormat = FrameGrabFormat
                OutPath &= FrameGrabSource.ToString & "_" & cp & "." & OutFormat.ToString

                'burn in source time code if available
                If BurnInTimecode Then 'user wants source tc burn in
                    'Debug.WriteLine(DateTime.Now.Ticks - Me.LastGOPTC_Ticks)
                    If DateTime.Now.Ticks - Me.LastGOPTC_Ticks < 1700000 Then 'we got a tc recently
                        BM = DrawTextOnBMP(BM, Me.LastGOPTC.ToString_NoFrames, New PointF((BM.Width * 0.9), 15), 10)
                    End If
                End If

                'Save the image
                BM.Save(OutPath, OutFormat)
                BM.Dispose()

                'Clean up
                BM = Nothing
                MS.Close()
                MS = Nothing

            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem getting screengrab. error: " & ex.Message, " StackTrace: " & ex.StackTrace, Nothing)
            End Try
        End Sub

        Public Function MultiFrameGrab(ByVal Count As Integer, ByVal Format As System.Drawing.Imaging.ImageFormat) As Boolean
            Try
                MGFPath = Me.FrameGrabDumpDir & "MultiFrame_" & DateTime.Now.Hour & "-" & DateTime.Now.Minute & "-" & DateTime.Now.Second
                If Not Directory.Exists(MGFPath) Then Directory.CreateDirectory(MGFPath)

                Me.TempMultiGrabPath = MGFPath & "\temp_" & DateTime.Now.Ticks
                If Not Directory.Exists(Me.TempMultiGrabPath) Then Directory.CreateDirectory(Me.TempMultiGrabPath)

                Dim FramesSaved As Short = Graph.KO_IKeystone.SaveNextXFrames(Count, TempMultiGrabPath)
                If FramesSaved < Count Then
                    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Multi-frame grab did not get all desired frames. " & FramesSaved & " frames output.", Nothing, Nothing)
                    If FramesSaved = 0 Then Exit Function
                Else
                    RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "Multi-frame grab success: " & FramesSaved & " frames output.", Nothing, Nothing)
                End If

                MGFCount = FramesSaved
                MGFCountTarget = Count

                MultiFrameImageFormat = Format
                Dim T As New Thread(AddressOf ProcessMultiFrames)
                T.Start()
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with MultiFrameGrab(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

        Public Sub SetGOPTimecodeBurnIn(ByVal Enabled As Boolean, ByVal RedIFrames As Short)
            Try
                If Enabled Then
                    HR = Graph.KO_IKeystoneMixer.BurnGOPTCs(1, RedIFrames)
                Else
                    HR = Graph.KO_IKeystoneMixer.BurnGOPTCs(0, RedIFrames)
                End If
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                Throw New Exception("Problem with SetGOPTimecodeBurnIn(). Error: " & ex.Message)
            End Try
        End Sub

#End Region 'API:METHODS:VIDEO

#Region "API:METHODS:AUDIO"

        Public Function GetAudioStreamStatus(ByRef NumberOfStreams As Integer, ByRef CurrentStream As Integer) As Boolean
            If Graph.DVDInfo Is Nothing Then Return False
            Graph.DVDInfo.GetCurrentAudio(NumberOfStreams, CurrentStream)
            Return True
        End Function

        Public Function GetAudioBitrate() As Integer
            Try
                'NVAUDDEC_STATS_BITRATE
                Dim i As Integer
                Graph.nvAudioAtts.GetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_STATS, nvcommon.ENvidiaAudioDecoderProps_Stats.NVAUDDEC_STATS_BITRATE, i)
                'Graph.nvAudioAtts.GetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_STATS, nvcommon.ENvidiaAudioDecoderProps_Stats.NVAUDDEC_STATS_BITRATE, i)
                'Debug.WriteLine("rate: " & i)
                'DB.lblCurrentAud_Bitrate.Text = (i / 1024) & "k"
                Return i
            Catch ex As Exception
                Throw New Exception("Problem with GetAudioBitrate(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function GetAudioAC3_ACMOD() As Integer
            Try
                Dim i As Integer
                'NVAUDDEC_STATS_AC3_ACMOD
                Graph.nvAudioAtts.GetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_STATS, nvcommon.ENvidiaAudioDecoderProps_Stats.NVAUDDEC_STATS_AC3_ACMOD, i)
                'System.Threading.Thread.Sleep(1000) 'this was done because we weren't getting valid data, it seems that waiting one second allows the audio decoder to sort out whatever the issue was
                Graph.nvAudioAtts.GetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_STATS, nvcommon.ENvidiaAudioDecoderProps_Stats.NVAUDDEC_STATS_AC3_ACMOD, i)
                Return i
            Catch ex As Exception
                Throw New Exception("Problem with GetAudioAC3_ACMOD(). Error: " & ex.Message)
            End Try
        End Function

        Public Function GetAudioSurroundEncoded() As Integer
            Try
                Dim i As Integer
                'Are we suround encoded
                Graph.nvAudioAtts.GetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_STATS, nvcommon.ENvidiaAudioDecoderProps_Stats.NVAUDDEC_STATS_AC3_DSURMOD, i)
                Graph.nvAudioAtts.GetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_STATS, nvcommon.ENvidiaAudioDecoderProps_Stats.NVAUDDEC_STATS_AC3_DSURMOD, i)
                Return i
            Catch ex As Exception
                Throw New Exception("Problem with GetAudioSurroundEncoded(). Error: " & ex.Message)
            End Try
        End Function

        Public Sub CloseAudDumpFile()
            Try
                If Not AudioDumpFile Is Nothing Then
                    AudioDumpFile.Close()
                    AudioDumpFile = Nothing
                End If
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with CloseAudDumpFile. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Function GetAudioAC3_LFEActive() As Boolean
            Try
                Dim i As Integer
                Graph.nvAudioAtts.GetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_STATS, nvcommon.ENvidiaAudioDecoderProps_Stats.NVAUDDEC_STATS_AC3_LFEON, i)
                Graph.nvAudioAtts.GetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_STATS, nvcommon.ENvidiaAudioDecoderProps_Stats.NVAUDDEC_STATS_AC3_LFEON, i)
                Return i
            Catch ex As Exception
                Throw New Exception("Problem with GetAudioAC3_LFEActive(). Error: " & ex.Message)
            End Try
        End Function

#End Region 'API:METHODS:AUDIO

#Region "API:METHODS:SUBPICTURES"

        Public Function IsSubStreamEnabled(ByVal StreamNumber As Integer) As Boolean
            Dim b As Boolean = False
            HR = Me.Graph.DVDInfo.IsSubpictureStreamEnabled(StreamNumber, b)
            If HR = -2147024809 Then Return True 'invalid parameter (often is 0)
            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Return b
        End Function

        Public ReadOnly Property AreSubtitlesOn() As Boolean
            Get
                Dim StreamsAvailable, CurrentStream As Integer
                Dim SubsOff As Boolean
                Dim hr As Integer = Graph.DVDInfo.GetCurrentSubpicture(StreamsAvailable, CurrentStream, SubsOff)
                If hr < 0 Then Marshal.ThrowExceptionForHR(hr)
                If SubsOff Then
                    Return False
                Else
                    Return True
                End If
            End Get
        End Property

        Public Function GetSubtitle(ByVal StreamNumber As Short) As seqSub
            Try
                Dim att As DvdSubPicAttr
                Graph.DVDInfo.GetSubpictureAttributes(StreamNumber, att)
                Dim txt As New StringBuilder(600)
                Dim S As New seqSub
                S.Coding = GetSubtitleEncoding(att.coding)
                S.Language = GetLanguageByLCID(CLng(att.language))
                S.Extension = GetSubtitleExtensionName(att.languageExt)
                S.Type = GetSubtitleTypeAsString(att.type)
                S.StreamNumber = StreamNumber
                Return S
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

        Public Sub SetSubpicturePlacement(ByVal X As Short, ByVal Y As Short)
            Try
                HR = Me.Graph.KO_IKeystoneMixer.SetSPPlacement(X, Y)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with SetSPPlacement. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'API:METHODS:SUBPICTURES

#Region "API:METHODS:CLOSED CAPTIONS"

        Public Sub SetClosedCaptionPlacement(ByVal X As Short, ByVal Y As Short)
            Try
                HR = Me.Graph.KO_IKeystoneMixer.SetL21Placement(X, Y)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with SetL21Placement. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'API:METHODS:CLOSED CAPTIONS

#Region "API:METHODS:TEST PATTERNS"

        Public Sub ShowColorBars(ByVal ShowThem As Boolean)
            Try
                If ShowThem Then
                    If CurrentVideoDimensions.Y = 576 Then
                        'HR = PP.Graph.KO_IKeystone.ShowUYVYFile(PP.GetExePath & "Media\ColorBars\PAL_smptebars.yuv", 0, 0, PP.CurrentVideoDimensions.X, PP.CurrentVideoDimensions.Y)
                        RenderUYVYResource("SMT.Libraries.MediaResources", "SD_PAL_SMPTE_Colorbars.YUV", 0, 0)
                    Else
                        'HR = Graph.KO_IKeystone.ShowUYVYFile(GetExePath() & "Media\ColorBars\SD_NTSC_SMPTE_Colorbars.YUV", 0, 0, CurrentVideoDimensions.X, CurrentVideoDimensions.Y)
                        RenderUYVYResource("SMT.Libraries.MediaResources", "SD_NTSC_SMPTE_Colorbars.YUV", 0, 0)
                    End If
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Else
                    HR = Graph.KO_IKeystone.ClearUYVYFile_A()
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End If
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with ShowColorBars. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Sub ShowTestPattern(ByVal Display As Boolean, Optional ByVal Anamorphic As Boolean = False)
            Try
                If Display Then
                    If CurrentVideoDimensions.Y = 576 Then
                        If Anamorphic Then
                            RenderUYVYResource("SMT.Libraries.MediaResources", "SD_PAL_PATTERN_16x9.YUV", 0, 0)
                        Else
                            RenderUYVYResource("SMT.Libraries.MediaResources", "SD_PAL_PATTERN_4x3.YUV", 0, 0)
                        End If
                    Else
                        If Anamorphic Then
                            RenderUYVYResource("SMT.Libraries.MediaResources", "SD_NTSC_PATTERN_16x9.YUV", 0, 0)
                        Else
                            RenderUYVYResource("SMT.Libraries.MediaResources", "SD_NTSC_PATTERN_4x3.YUV", 0, 0)
                        End If
                    End If
                Else
                    HR = Graph.KO_IKeystone.ClearUYVYFile_A()
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End If
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with ShowTestPattern(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'API:METHODS:TEST PATTERNS

#Region "API:METHODS:JACKET PICTURES"

        Public Sub ShowJacketPicture(ByVal JPName As String)
            Try
                'For NTSC:
                'J00___5L.MP2 720 x 480
                'J00___5M.MP2 176 x 112
                'J00___5S.MP2 96 x 64

                'For PAL:
                'J00___6L.MP2 720 x 576
                'J00___6M.MP2 176 x 144
                'J00___6S.MP2 96 x 80

                Dim s() As String = Split(Path.GetFileNameWithoutExtension(JPName), "___")
                Dim x, y, w, h As Short
                '5s are NTSC, 6s are PAL
                Select Case Microsoft.VisualBasic.Left(s(1), 2).ToLower
                    Case "5l"
                        x = 0
                        y = 0
                        w = 720
                        h = 480
                    Case "5m"
                        x = 100
                        y = 100
                        w = 176
                        h = 112
                    Case "5s"
                        x = 100
                        y = 100
                        w = 96
                        h = 64
                    Case "6l"
                        x = 0
                        y = 0
                        w = 720
                        h = 576
                    Case "6m"
                        x = 100
                        y = 100
                        w = 176
                        h = 144
                    Case "6s"
                        x = 100
                        y = 100
                        w = 96
                        h = 80
                End Select


                'Graph.KO_IKeystone.ShowJacketPicture(SpecificJPPath, x, y, Me.CurrentVideoDimensions.X, Me.CurrentVideoDimensions.Y)

                'HR = Graph.KO_IKeystone.ShowJacketPicture(JPName, 0, 0, CurrentVideoDimensions.X, CurrentVideoDimensions.Y)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Me.Graph.KO_IKeystone.ShowJacketPicture(JPName, x, y, w, h)
                'Graph.KO_IKeystone.ShowJacketPicture(SpecificJPPath, 0, 0, Me.CurrentVideoDimensions.X, Me.CurrentVideoDimensions.Y)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with ShowJacketPicture(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'API:METHODS:JACKET PICTURES

#Region "API:METHODS:GUIDES"

#Region "API:METHODS:GUIDES:ACTION/TITLE SAFE"

        Public Sub SetActionTitleGuides(ByVal Enabled As Boolean, ByVal Red As Integer, ByVal Green As Integer, ByVal Blue As Integer)
            Try
                If Graph.KO_IKeystoneMixer Is Nothing Then Exit Sub
                If Enabled Then
                    HR = Graph.KO_IKeystoneMixer.SetActionTitleGuides(1, Red, Green, Blue)
                Else
                    HR = Graph.KO_IKeystoneMixer.SetActionTitleGuides(0, Red, Green, Blue)
                End If
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                Throw New Exception("Problem with SetActionTitleGuides(). Error: " & ex.Message)
            End Try
        End Sub

#End Region 'API:METHODS:GUIDES:ACTION/TITLE SAFE

#Region "API:METHODS:GUIDES:FLEX"

        Public Sub SetFlexGuides(ByVal L As Short, ByVal T As Short, ByVal R As Short, ByVal B As Short, ByVal GuideColor As Color)
            Try
                HR = Me.Graph.KO_IKeystoneMixer.SetGuides(L, T, R, B, GuideColor.R, GuideColor.B, GuideColor.G)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with SetGuides. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Sub ClearFlexGuides()
            Me.Graph.KO_IKeystoneMixer.ClearGuides()
        End Sub

#End Region 'API:METHODS:GUIDES:FLEX

#End Region 'API:METHODS:GUIDES

#Region "API:METHODS:UOPs"

        Public Sub UpdateUOPs()
            Dim UOPBitmask As Integer
            Try
                If PlayState = ePlayState.SystemJP Then GoTo SetAllUOPsFalse
                If Me.Graph.DVDInfo Is Nothing Then GoTo SetAllUOPsFalse

                HR = Me.Graph.DVDInfo.GetCurrentUOPS(UOPBitmask)
                If HR = -2147220873 Then
SetAllUOPsFalse:
                    'ReDim _UOPs(24)
                    UOPBitmask = 0
                End If
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'go through each UOP and if it changed log it.
                If PlayState = ePlayState.SystemJP Or PlayState = ePlayState.ColorBars Then
                    'all false
                    UOPBitmask = 33554431
                End If

                'Debug.WriteLine(P1 & vbNewLine)
                'If Not xAD.aUOPs Then Exit Sub

                '33554431/1111111111111111111111111
                If UOPBitmask > 33554431 Then
                    Throw New Exception("Problem with UpdateUOPs(). Error: UOP bitmask is too large.", Nothing)
                    Exit Sub
                End If

                '1) convert p1 to binary
                Dim Bin As String = DecimalToBinary(UOPBitmask)

                '2) Populate an array of the values in Bin
                If Bin.Length <> 25 Then
                    Bin = PadString(Bin, 25, "0", True)
                End If

                Bin = Microsoft.VisualBasic.StrReverse(Bin)

                ReDim _UOPs(24)
                For i As Short = 0 To 24
                    _UOPs(i) = CBool(Bin.Chars(i).ToString)
                    _UOPs(i) = Not _UOPs(i)
                Next
            Catch ex As Exception
                If InStr(ex.Message, "80040277") Then Exit Sub
                Throw New Exception("Problem with UpdateUOPs(). Error: " & ex.Message)
            End Try
        End Sub

#End Region 'API:METHODS:UOPs

#Region "API:METHODS:GPRMs"

        Public Sub SetGPRM(ByVal GPRM As UInt32, ByVal Value As UShort)
            Try
                If Me.Graph Is Nothing OrElse Me.Graph.DVDCtrl Is Nothing Then Exit Sub
                HR = Me.Graph.DVDCtrl.SetGPRM(GPRM, Value, DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with SetGPRM(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'API:METHODS:GPRMs

#Region "API:METHODS:EVENTS"

        Public Function HandleEvent() As Boolean
            Dim hr, p1, p2 As Integer
            Dim code As DsEvCode
            Do
                If Graph Is Nothing OrElse Graph.MediaEvt Is Nothing Then Return False
                hr = Graph.MediaEvt.GetEvent(code, p1, p2, 0)
                If hr < 0 Then Exit Do
                GraphEventHandler(code, p1, p2)
                If Not Graph.MediaEvt Is Nothing Then
                    hr = Graph.MediaEvt.FreeEventParams(code, p1, p2)
                End If
            Loop While hr = 0
        End Function

#End Region 'API:METHODS:EVENTS

#Region "API:METHODS:ON SCREEN DISPLAY"

        Public Overrides Sub ClearOSD()
            Try
                HR = Me.Graph.KO_IKeystoneMixer.ClearOSD()
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with ClearOSD(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'API:METHODS:ON SCREEN DISPLAY

#Region "API:METHODS:KEY STRIKES"

        Public Sub KeyStrike(ByVal e As KeyEventArgs)
            RaiseEvent evKeyStrike(e)
        End Sub

        Public Sub KeyStrikeWPF(ByVal e As Windows.Input.KeyEventArgs)
            RaiseEvent evKeyStrikeWPF(e)
        End Sub

#End Region 'API:METHODS:KEY STRIKES

#Region "API:METHODS:BARDATA"

        Public Overrides Sub BarDataConfig(ByVal DetectBarData As Integer, ByVal BurnGuides As Integer, ByVal Luma_Tolerance As Integer, ByVal Chroma_Tolerance As Integer)
            Try
                If Me.Graph Is Nothing Then Exit Sub
                If Me.Graph.KO_IKeystoneMixer Is Nothing Then Exit Sub
                Me.Graph.KO_IKeystoneMixer.BarDataConfig(DetectBarData, BurnGuides, Luma_Tolerance, Chroma_Tolerance)
            Catch ex As Exception
                Throw New Exception("Problem in BarDataConfig(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Overrides Sub BarDataReset()
            If Me.Graph Is Nothing Then Exit Sub
            If Me.Graph.KO_IKeystoneMixer Is Nothing Then Exit Sub
            Me.Graph.KO_IKeystoneMixer.BarDataReset()
        End Sub

#End Region 'API:METHODS:BARDATA

#End Region 'API:METHODS

#Region "API:CONSTS"

        Public Const DVD_STREAM_DATA_CURRENT As Integer = &H800
        Public Const DVD_STREAM_DATA_VMGM As Integer = &H400
        Public Const DVD_STREAM_DATA_VTSM As Integer = &H401
        Public Const DVD_DEFAULT_AUDIO_STREAM As Integer = &HF

#End Region 'API:CONSTS

#Region "API:ADMIN"

        Public Sub SetNULLTimestamps()
            Try
                Graph.KO_IKeystone.SetNULLTimestamps()
                Graph.AMTC_Interface.SetNULLTimestamps()
            Catch ex As Exception
                Throw New Exception("Problem with SetNULLTimestamps(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Sub SetKeystoneTrialMode(ByVal bTrial As Boolean)
            Try
                'the argument is meaningless. this will always put keystone into trial mode.
                Me.Graph.KO_IKeystone.SetProperty(New Guid("EE369535-4EFA-4ae4-BBAB-93218A3E3A16"), Convert.ToInt32(bTrial))
            Catch ex As Exception
                Throw New Exception("Problem with SetKeystoneTrialMode(). Error: " & ex.Message, ex)
            End Try
        End Sub

        'Public Sub SetKeystoneTrialOverride(ByVal bTrialOverride As Boolean)
        '    Try
        '        'the argument really makes no difference here. This will always DEACTIVATE trial mode override (meaning that it will open the possibility of using the trial burn-in).
        '        Me.Graph.KO_IKeystone.SetProperty(New Guid("19655F52-2A4D-40d9-BA93-A6B90C2EDD72"), Convert.ToInt32(bTrialOverride))
        '        'Me.Graph.KO_IKeystone.SetProperty(New Guid("19655F52-2A4D-40d9-BA93-A6B90C2EDD72"), Convert.ToInt32(bTrialOverride))
        '    Catch ex As Exception
        '        Throw New Exception("Problem with SetKeystoneTrialMode(). Error: " & ex.Message, ex)
        '    End Try
        'End Sub

#End Region 'API:ADMIN

#End Region 'API

#Region "FILTER GRAPH"

        Public Overrides Function BuildGraph(ByVal nFilePath As String, ByVal NotifyWindowHandle As Integer, ByVal nAVMode As eAVMode, ByVal nParentForm As Form) As Boolean
            Try
                Select Case AVMode
                    Case eAVMode.Decklink
                        Return Me.PhoenixGraph_Decklink
                    Case eAVMode.Intensity
                        Return Me.PhoenixGraph_Intensity
                    Case eAVMode.DesktopVMR
                        Return Me.PhoenixGraph_Desktop
                End Select
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with BuildGraph(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

        Private Function PhoenixGraph_Decklink() As Boolean
            Try
                If Graph Is Nothing Then Graph = New cSMTGraph(Me.ParentForm.Handle)

                If Not Graph.AddDeckLinkAudio() Then Return False
                If Not Graph.AddNVidiaVideoDecoder(False, False) Then Return False
                If Not Graph.AddNVidiaAudioDecoder(False) Then Return False
                If Not Graph.AddKeystoneOmni(AVMode) Then Return False

                ''DEBUGGING
                'HR = Graph.GraphBuilder.RemoveFilter(Graph.KeystoneOMNI)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'HR = Marshal.FinalFinalReleaseComObject(Graph.KeystoneOMNI)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                'Graph.KeystoneOMNI = Nothing
                'Throw New Exception("done")
                'Application.Exit()
                ''DEBUGGING

                If Not Graph.AddDVDNav() Then Return False
                If Not Graph.AddAMTC() Then Return False

                HR = Graph.GraphBuilder.Connect(Graph.DVDNav_VidPin, Graph.VidDec_Vid_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.VidDec_Vid_Out, Graph.KO_In)
                'HR = Graph.GraphBuilder.Connect(Graph.VidDec_Vid_Out, Graph.KO_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.DVDNav_SubPin, Graph.VidDec_SP_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.DVDNav_AudPin, Graph.AudDec_InPin)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.AudDec_OutPin, Graph.AMTC_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.AMTC_Out, Graph.AudRen_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.VidDec_SP_Out, Graph.KO_SP)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                If Not Graph.AddDeckLinkVideo() Then Return False

                'HR = Graph.GraphBuilder.Connect(Graph.KO_Out, Graph.DLV_In)
                HR = Graph.GraphBuilder.Connect(Graph.KO_Out, Graph.DLV_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.RemoveFilter(Graph.DLVideo)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Marshal.FinalReleaseComObject(Graph.DLVideo)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Graph.DLVideo = Nothing

                Try
                    If CurrentVideoStandard = eVideoStandard.NTSC Then
                        HR = Graph.DVDCtrl.SetDVDDirectory(Me.BlankDVDPath_NTSC)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    Else
                        HR = Graph.DVDCtrl.SetDVDDirectory(Me.BlankDVDPath_PAL)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    End If
                    HR = Graph.MediaCtrl.Run()
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    'hr = Graph.DVDCtrl.PlayForwards(1.0, DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    HR = Graph.MediaCtrl.Stop
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Catch ex As Exception
                    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Error: " & ex.Message, Nothing, Nothing)
                End Try

                If Not Graph.AddDeckLinkVideo() Then Return False
                If Not Graph.AddLine21Decoder1() Then Return False
                If Not Graph.AddL21G() Then Return False

                HR = Graph.GraphBuilder.Connect(Graph.KO_Out, Graph.DLV_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.VidDec_CC_Out, Graph.L21G_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Render(Graph.L21G_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Graph.AddGraphToROT()

                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem building full graph. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

        Private Function PhoenixGraph_Intensity() As Boolean
            Try
                If Graph Is Nothing Then Graph = New cSMTGraph(Me.ParentForm.Handle)

                Graph.AddGraphToROT()

                If Not Graph.AddDeckLinkAudio() Then Return False
                If Not Graph.AddNVidiaVideoDecoder(False, False) Then Return False
                If Not Graph.AddNVidiaAudioDecoder(False) Then Return False
                If Not Graph.AddKeystoneOmni(AVMode) Then Return False
                If Not Graph.AddMCE_ImgSiz() Then Return False
                'If Not Graph.AddDeinterlacer() Then Return False
                'If Not Graph.AddLEAD_Deinterlacer Then Return False

                'Graph.SetNvidiaDeinterlacing(nvcommon.ENVVIDDEC_CONFIG_DEINTERLACE_MODE_CTRL.NVVIDDEC_CONFIG_DEINTERLACE_CTRL_VIDEO, nvcommon.ENVVIDDEC_CONFIG_DEINTERLACE_MODE.NVVIDDEC_CONFIG_DEINTERLACE_NORMAL)

                ''DEBUGGING
                'HR = Graph.GraphBuilder.RemoveFilter(Graph.KeystoneOMNI)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'HR = Marshal.FinalFinalReleaseComObject(Graph.KeystoneOMNI)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                'Graph.KeystoneOMNI = Nothing
                'Throw New Exception("done")
                'Application.Exit()
                ''DEBUGGING

                If Not Graph.AddDVDNav() Then Return False
                If Not Graph.AddAMTC() Then Return False

                HR = Graph.GraphBuilder.Connect(Graph.DVDNav_VidPin, Graph.VidDec_Vid_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.VidDec_Vid_Out, Graph.KO_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.DVDNav_SubPin, Graph.VidDec_SP_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.DVDNav_AudPin, Graph.AudDec_InPin)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.AudDec_OutPin, Graph.AMTC_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.AMTC_Out, Graph.AudRen_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.VidDec_SP_Out, Graph.KO_SP)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                If Not Graph.AddDeckLinkVideo() Then Return False

                HR = Graph.GraphBuilder.Connect(Graph.KO_Out, Graph.DLV_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.RemoveFilter(Graph.DLVideo)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Marshal.FinalReleaseComObject(Graph.DLVideo)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Graph.DLVideo = Nothing

                'HR = Graph.GraphBuilder.RemoveFilter(Graph.MCE_ImgSiz)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                'HR = Marshal.FinalReleaseComObject(Graph.MCE_ImgSiz)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                'Graph.MCE_ImgSiz = Nothing

                Try
                    If CurrentVideoStandard = eVideoStandard.NTSC Then
                        HR = Graph.DVDCtrl.SetDVDDirectory(Me.BlankDVDPath_NTSC)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    Else
                        HR = Graph.DVDCtrl.SetDVDDirectory(Me.BlankDVDPath_PAL)
                        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    End If
                    HR = Graph.MediaCtrl.Run()
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    'hr = Graph.DVDCtrl.PlayForwards(1.0, DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                    HR = Graph.MediaCtrl.Stop
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Catch ex As Exception
                    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Error: " & ex.Message, Nothing, Nothing)
                End Try

                If Not Graph.AddDeckLinkVideo() Then Return False
                If Not Graph.AddLine21Decoder1() Then Return False
                If Not Graph.AddL21G() Then Return False

                'HR = Graph.GraphBuilder.Connect(Graph.KO_Out, Graph.LD_DINT_In)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                If IntensityScaling <> eScalingMode.Native Then

                    HR = Graph.GraphBuilder.Connect(Graph.KO_Out, Graph.MCE_ImgSiz_In)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                    SetIntensityScaling(16, 9)

                    HR = Graph.GraphBuilder.Connect(Graph.MCE_ImgSiz_Out, Graph.DLV_In)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Else
                    HR = Graph.GraphBuilder.Connect(Graph.KO_Out, Graph.DLV_In)
                    'HR = Graph.GraphBuilder.Connect(Graph.KO_Out, Graph.Dint_In)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End If

                'HR = Graph.GraphBuilder.Connect(Graph.Dint_out, Graph.DLV_In)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.VidDec_CC_Out, Graph.L21G_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Render(Graph.L21G_Out)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem building full graph. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

        Private Function PhoenixGraph_Desktop() As Boolean
            Try
                If Graph Is Nothing Then Graph = New cSMTGraph(Me.ParentForm.Handle)

                If Not Graph.AddNVidiaAudioDecoder(True) Then Return False
                If Not Graph.AddNVidiaVideoDecoder(False, True) Then Return False
                If Not Graph.AddDVDNav() Then Return False
                If Not Graph.AddVMR9(3) Then Return False
                'If Not Graph.AddKeystoneOMNI() Then Return False
                If Not Graph.AddKeystoneOmni() Then Return False
                If Not Graph.AddLine21Decoder2() Then Return False

                HR = Graph.GraphBuilder.Connect(Graph.DVDNav_VidPin, Graph.VidDec_Vid_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.VidDec_Vid_Out, Graph.KO_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.DVDNav_SubPin, Graph.VidDec_SP_In)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.DVDNav_AudPin, Graph.AudDec_InPin)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Render(Graph.AudDec_OutPin)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.VidDec_SP_Out, Graph.KO_SP)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                HR = Graph.GraphBuilder.Connect(Graph.KO_Out, Graph.VMR9_In_1)
                'HR = Graph.GraphBuilder.Connect(Graph.KO_Out, Graph.VMR9_In_1)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                ''NEW
                ''If Not Graph.AddLine21Decoder1() Then Return False
                'If Not Graph.AddL21G() Then Return False

                'HR = Graph.GraphBuilder.Connect(Graph.VidDec_CC_Out, Graph.L21G_In)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'HR = Graph.GraphBuilder.Connect(Graph.L21G_Out, Graph.KO_CC)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)


                'OLD WORKS
                HR = Graph.GraphBuilder.Connect(Graph.VidDec_CC_Out, Graph.VMR_In_2)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)


                Graph.VideoWin = CType(Graph.VMR9, IVideoWindow)
                RaiseEvent evSetupViewer()
                'SetupViewer_WPF()

                'If ViewerHostControl Is Nothing Then
                '    Me.SetupViewer(360, 243, True)
                'Else
                '    Me.PlaceViewerInControl(ParentForm.Handle, ViewerHostControl)
                'End If

                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with PhoenixGraph_Mobile(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

        Private Function SwitchVideoFormats(ByVal NewHeight As Short) As Boolean
            Try
                If NewHeight = 576 Then
                    Graph.SetVideoStandardViaInterface(False)
                    Graph.SetVideoStandardViaRegistry(False)
                Else
                    Graph.SetVideoStandardViaInterface(True)
                    Graph.SetVideoStandardViaRegistry(True)
                End If

                If Not Graph.DestroyGraph() Then Throw New Exception("Failed to destroy graph in SwitchVideoFormat.")
                Graph = Nothing
                If Me.Viewer_WF IsNot Nothing Then
                    Me.Viewer_WF.Close()
                    Me.Viewer_WF = Nothing
                End If
                If Me.Viewer_WPF IsNot Nothing Then
                    Me.Viewer_WPF.Close()
                    Me.Viewer_WPF = Nothing
                End If

                System.Threading.Thread.Sleep(500)

                If Not BuildGraph(Nothing, Nothing, Nothing, Nothing) Then Throw New Exception("Failed on call to BuildGraph in SwitchVideoFormats.")

                Return True
            Catch ex As Exception
                Throw New Exception("Problem with SwitchVideoFormats(). Application exiting.")
                Application.Exit()
                Return False
            End Try
        End Function

#Region "FILTER VERIFICATION"

        Public Shared Function FilterCheck_shr() As String
            Try
                Dim startstring As String = "The following components are not installed correctly:" & vbNewLine & vbNewLine
                Dim msgstr As String = startstring
                If Not nVidia_MPEG2Decoder Then msgstr &= "nVidia video decoder" & vbNewLine
                If Not nVidia_AudioDecoder Then msgstr &= "nVidia audio decoder" & vbNewLine
                If Not SMT_AMTC Then msgstr &= "SMT AMTC" & vbNewLine
                If Not SMT_KeystoneOM Then msgstr &= "SMT Keystone" & vbNewLine
                'If Not SMT_KeystoneSD Then msgstr &= "SMT Keystone" & vbNewLine
                If Not SMT_L21G Then msgstr &= "SMT L21G" & vbNewLine
                If Not MainConcept_MPEGDemux Then msgstr &= "MPEG Demux" & vbNewLine
                'If Not SMT_DAC Then msgstr &= "SMT DAC" & vbNewLine
                If Not SMT_NVS Then msgstr &= "SMT NVS" & vbNewLine
                If Not MainConcept_Scaler Then msgstr &= "MC ImgScl" & vbNewLine
                If msgstr = startstring Then Return "True"
                Return msgstr
            Catch ex As Exception
                'Throw New Exception("Problem with FilterCheck(). Error: " & ex.Message)
                Return "False"
            End Try
        End Function

        Public Overrides Function FilterCheck() As String
            Throw New Exception("FilterCheck() has custom implementation in this class.")
        End Function

#End Region 'FILTER VERIFICATION

#Region "INTENSITY SCALING"

        Private Sub SetIntensityScaling(ByVal X As Byte, ByVal Y As Byte)
            If AVMode <> eAVMode.Intensity Then Exit Sub
            Graph.MCE_SetScaling(IntensitySignalResolution, IntensityScaling, X, Y)
        End Sub

#End Region 'INTENSITY SCALING

#End Region 'FILTER GRAPH

#Region "PRIVATE FUNCTIONALITY"

#Region "PRIVATE PROPERTIES"

        Private HR As Integer

        Private Languages As cLanguages
        Private Countries As cCountries

        Private TempDVDDirForVidStdChange As String = ""
        Private RestartDueToVidStdChange As Boolean = False

        Private Overloads ReadOnly Property TitleSafeTop() As Short
            Get
                Return Me.CurrentVideoDimensions.Y - (0.9 * Me.CurrentVideoDimensions.Y)
            End Get
        End Property

        Private Overloads ReadOnly Property TitleSafeLeft() As Short
            Get
                Return Me.CurrentVideoDimensions.X - (0.9 * Me.CurrentVideoDimensions.X)
            End Get
        End Property

        Private ForceDecoderPALConnection As eForceDecoderPALConnection = eForceDecoderPALConnection.eFalse

        Private StopAtLocation As DvdPlayLocation
        Private PauseLocation As DvdPlayLocation

        Private BlankDVDPath_NTSC As String = GetExePath() & "Media\BlankNTSC"
        Private BlankDVDPath_PAL As String = GetExePath() & "Media\BlankPAL"

        Private DoubleStopDispose As Boolean = False

        'Private LastFrameDelivered_Ticks As Long
        'Private VideoFrameDelivered_Delta As Long

        Private WithEvents VideoFrameReceived_Timer As System.Windows.Forms.Timer
        Private LastVideoFrameReceived_Ticks As Long
        Private VideoFrameReceived_Delta As Long

        'Private WithEvents AudioFrameReceived_Timer As System.Windows.Forms.Timer
        'Private LastAudioFrameReceived_Ticks As Long
        'Private AudioFrameReceived_Delta As Long

        'MUTEX w/timer for command sync
        'Private pendingCmd As Boolean = False 'used as a mutex on certain transport functions so as to prevent double calls
        Private cmdOption As OptIDvdCmd 'not significant. we're not using object event synchronization.
        Private WithEvents CommandSyncTimer As System.Timers.Timer
        Private CommandSyncTolerance As Integer = 750

        Private WithEvents PrevChapter_Timer As System.Windows.Forms.Timer

        Private DVDState As IDvdState

#End Region 'PRIVATE PROPERTIES

#Region "GRAPH:EVENTS"

#Region "GRAPH:EVENTS:CORE"

        Private Sub GraphEventHandler(ByVal code As DsEvCode, ByVal p1 As Integer, ByVal p2 As Integer)
            Try
                If PlayState = ePlayState.SystemJP Then Exit Sub
                'If Not code = DsEvCode.EC_DVD_CURRENT_HMSF_TIME Then Debug.WriteLine("GRAPH EVENT: " & code.ToString)
                Select Case code



                    '===================================================================================================
                    '===================================================================================================
                    ' FILTER GRAPH MANAGER
                    '  (0x001 - 0x064) (evcode.h)
                    '=================================

                    Case DsEvCode.EC_ERRORABORT
                        HandleGraphEvent_EC_ERRORABORT(p1, p2)
                    Case DsEvCode.EC_PAUSED
                        HandleGraphEvent_EC_PAUSED(p1, p2)
                    Case DsEvCode.EC_CLOCK_CHANGED
                        HandleGraphEvent_EC_CLOCK_CHANGED(p1, p2)
                    Case DsEvCode.EC_GRAPH_CHANGED
                        HandleGraphEvent_EC_GRAPH_CHANGED(p1, p2)
                    Case DsEvCode.EC_VIDEO_SIZE_CHANGED
                        HandleGraphEvent_EC_VIDEO_SIZE_CHANGED(p1, p2)
                    Case DsEvCode.EC_USERABORT
                        HandleGraphEvent_EC_USERABORT(p1, p2)
                    Case DsEvCode.EC_QUALITY_CHANGE
                        HandleGraphEvent_EC_QUALITY_CHANGE(p1, p2)
                    Case DsEvCode.EC_VMR_RENDERDEVICE_SET
                        HandleGraphEvent_EC_VMR_RENDERDEVICE_SET(p1, p2)
                    Case DsEvCode.EC_STREAM_ERROR_STILLPLAYING
                        HandleGraphEvent_EC_STREAM_ERROR_STILLPLAYING(p1, p2)
                    Case DsEvCode.EC_STEP_COMPLETE
                        Debug.WriteLine("EC_STEP_COMPLETE")


                        '===================================================================================================
                        '===================================================================================================
                        ' DVD
                        '  (0x0100 - 0x0150) (dvdevcod.h)
                        '=================================

                    Case DsEvCode.EC_DVD_VOBU_Offset
                        HandleGraphEvent_EC_DVD_VOBU_Offset(p1, p2)
                    Case DsEvCode.EC_DVD_VOBU_Timestamp
                        HandleGraphEvent_EC_DVD_VOBU_Timestamp(p1, p2)

                    Case DsEvCode.EC_DVD_GPRM_Change
                        HandleGraphEvent_EC_DVD_GPRM_Change(p1, p2)
                    Case DsEvCode.EC_DVD_SPRM_Change
                        HandleGraphEvent_EC_DVD_SPRM_Change(p1, p2)

                    Case DsEvCode.EC_DVD_BeginNavigationCommands
                        HandleGraphEvent_EC_DVD_BeginNavigationCommands(p1, p2)
                    Case DsEvCode.EC_DVD_NavigationCommand
                        HandleGraphEvent_EC_DVD_NavigationCommand(p1, p2)

                    Case DsEvCode.EC_DVD_DOMAIN_CHANGE
                        HandleGraphEvent_EC_DVD_DOMAIN_CHANGE(p1, p2)
                    Case DsEvCode.EC_DVD_TITLE_SET_CHANGE
                        HandleGraphEvent_EC_DVD_TITLE_SET_CHANGE(p1, p2)
                    Case DsEvCode.EC_DVD_TITLE_CHANGE
                        HandleGraphEvent_EC_DVD_TITLE_CHANGE(p1, p2)
                    Case DsEvCode.EC_DVD_CHAPTER_START
                        HandleGraphEvent_EC_DVD_CHAPTER_START(p1, p2)
                    Case DsEvCode.EC_DVD_PROGRAM_CHAIN_CHANGE
                        HandleGraphEvent_EC_DVD_PROGRAM_CHAIN_CHANGE(p1, p2)
                    Case DsEvCode.EC_DVD_PROGRAM_CELL_CHANGE
                        HandleGraphEvent_EC_DVD_PROGRAM_CELL_CHANGE(p1, p2)
                    Case DsEvCode.EC_DVD_CURRENT_HMSF_TIME
                        HandleGraphEvent_EC_DVD_CURRENT_HMSF_TIME(p1, p2)

                    Case DsEvCode.EC_DVD_PLAYBACK_RATE_CHANGE
                        HandleGraphEvent_EC_DVD_PLAYBACK_RATE_CHANGE(p1, p2)
                    Case DsEvCode.EC_DVD_CMD_START
                        HandleGraphEvent_EC_DVD_CMD_START(p1, p2)
                    Case DsEvCode.EC_DVD_CMD_END
                        HandleGraphEvent_EC_DVD_CMD_END(p1, p2)
                    Case DsEvCode.EC_DVD_STILL_ON
                        HandleGraphEvent_EC_DVD_STILL_ON(p1, p2)
                    Case DsEvCode.EC_DVD_STILL_OFF
                        HandleGraphEvent_EC_DVD_STILL_OFF(p1, p2)
                    Case DsEvCode.EC_DVD_BUTTON_CHANGE
                        HandleGraphEvent_EC_DVD_BUTTON_CHANGE(p1, p2)
                    Case DsEvCode.EC_DVD_NO_FP_PGC
                        HandleGraphEvent_EC_DVD_NO_FP_PGC(p1, p2)
                    Case DsEvCode.EC_DVD_AUDIO_STREAM_CHANGE
                        HandleGraphEvent_EC_DVD_AUDIO_STREAM_CHANGE(p1, p2)
                    Case DsEvCode.EC_DVD_SUBPICTURE_STREAM_CHANGE
                        HandleGraphEvent_EC_DVD_SUBPICTURE_STREAM_CHANGE(p1, p2)
                    Case DsEvCode.EC_DVD_ANGLE_CHANGE
                        HandleGraphEvent_EC_DVD_ANGLE_CHANGE(p1, p2)
                    Case DsEvCode.EC_DVD_PARENTAL_LEVEL_CHANGE
                        HandleGraphEvent_EC_DVD_PARENTAL_LEVEL_CHANGE(p1, p2)
                    Case DsEvCode.EC_DVD_VALID_UOPS_CHANGE
                        HandleGraphEvent_EC_DVD_VALID_UOPS_CHANGE(p1, p2)
                    Case DsEvCode.EC_DVD_ERROR
                        HandleGraphEvent_EC_DVD_ERROR(p1, p2)
                    Case DsEvCode.EC_DVD_WARNING
                        HandleGraphEvent_EC_DVD_WARNING(p1, p2)
                    Case DsEvCode.EC_DVD_PLAYBACK_STOPPED
                        HandleGraphEvent_EC_DVD_PLAYBACK_STOPPED(p1, p2)
                    Case DsEvCode.EC_DVD_BUTTON_AUTO_ACTIVATED
                        HandleGraphEvent_EC_DVD_BUTTON_AUTO_ACTIVATED(p1, p2)
                    Case DsEvCode.EC_DVD_CHAPTER_AUTOSTOP
                        HandleGraphEvent_EC_DVD_CHAPTER_AUTOSTOP(p1, p2)
                    Case DsEvCode.EC_DVD_KARAOKE_MODE
                        HandleGraphEvent_EC_DVD_KARAOKE_MODE(p1, p2)
                    Case DsEvCode.EC_DVD_PLAYPERIOD_AUTOSTOP
                        HandleGraphEvent_EC_DVD_PLAYPERIOD_AUTOSTOP(p1, p2)



                        '===================================================================================================
                        '===================================================================================================
                        ' KEYSTONE
                        '=================================

                    Case &H58 'EC_KEYSTONE_QUALITY_CHANGE
                        HandleGraphEvent_EC_KEYSTONE_QUALITY_CHANGE(p1, p2)
                    Case &H59 'EC_KEYSTONE_32
                        HandleGraphEvent_EC_KEYSTONE_32(p1, p2)
                    Case &H60 'EC_KEYSTONE_SAMPLETIMES
                        HandleGraphEvent_EC_KEYSTONE_SAMPLETIMES(p1, p2)
                    Case &H61 'EC_KEYSTONE_FIELDORDER
                        HandleGraphEvent_EC_KEYSTONE_FIELDORDER(p1, p2)
                    Case &H62 'EC_KEYSTONE_MPEGTC
                        HandleGraphEvent_EC_KEYSTONE_MPEGTC(p1, p2)
                    Case &H63 'EC_KEYSTONE_INTERLACING
                        HandleGraphEvent_EC_KEYSTONE_INTERLACING(p1, p2)
                    Case &H64 'EC_KEYSTONE_FORCEFRAMEGRAB
                        HandleGraphEvent_EC_KEYSTONE_FORCEFRAMEGRAB(p1, p2)
                    Case &H65 'EC_KEYSTONE_PROGRESSIVESEQUENCE
                        HandleGraphEvent_EC_KEYSTONE_PROGRESSIVESEQUENCE(p1, p2)
                    Case &H66 'EC_KEYSTONE_DISCONTINUITY
                        HandleGraphEvent_EC_KEYSTONE_DISCONTINUITY(p1, p2)
                    Case &H67 'EC_KEYSTONE_MACROVISION
                        HandleGraphEvent_EC_KEYSTONE_MACROVISION(p1, p2)
                    Case &H68 'EC_KEYSTONE_FRAMEDELIVERED
                        HandleGraphEvent_EC_KEYSTONE_FRAMEDELIVERED(p1, p2)
                    Case &H69 'EC_KEYSTONE_FRAMERECEIVED
                        HandleGraphEvent_EC_KEYSTONE_FRAMERECEIVED(p1, p2)
                    Case &H70 'EC_KEYSTONE_FRAMEDROPPED
                        HandleGraphEvent_EC_KEYSTONE_FRAMEDROPPED(p1, p2)
                    Case &H71 'EC_KEYSTONE_WRONGDURATION
                        HandleGraphEvent_EC_KEYSTONE_WRONGDURATION(p1, p2)
                    Case &H72 'EC_KEYSTONE_SAMPLESNOTADJACENT
                        HandleGraphEvent_EC_KEYSTONE_SAMPLESNOTADJACENT(p1, p2)

                    Case &H75 'EC_KEYSTONE_BARDATA_TOP_BOTTOM	
                        HandleGraphEvent_EC_KEYSTONE_BARDATA_TOP_BOTTOM(p1, p2)
                    Case &H76 'EC_KEYSTONE_BARDATA_LEFT_RIGHT	
                        HandleGraphEvent_EC_KEYSTONE_BARDATA_LEFT_RIGHT(p1, p2)
                    Case &H77 'EC_KEYSTONE_BARDATA_FRAME_TOO_DARK
                        HandleGraphEvent_EC_KEYSTONE_BARDATA_FRAME_TOO_DARK(p1, p2)
                    Case &H78 'EC_KEYSTONE_PRESENTATIONTIMES
                        'not handled

                    Case &H81 'EC_KEYSTONE_SETPRESENTATIONTIME
                        'not handled

                    Case &H83 'EC_KEYSTONE_DELIVER
                        'not handled
                    Case &H84 'EC_KEYSTONE_RUN
                        'not handled
                    Case &H85 'EC_KEYSTONE_TRIAL_FRAMECOUNT
                        HandleGraphEvent_EC_KEYSTONE_TRIAL_FRAMECOUNT(p1, p2)




                        '===================================================================================================
                        '===================================================================================================
                        ' AMTC
                        '=================================

                    Case 97 'EC_AMTC_SAMPLE_TIMES
                        HandleGraphEvent_EC_AMTC_SAMPLE_TIMES(p1, p2)
                    Case &H1069 'EC_AMTC2_FRAMERECEIVED
                        HandleGraphEvent_EC_AMTC2_FRAMERECEIVED(p1, p2)
                    Case 1074397285 'EC_AMTC_BUFFER sample available
                        HandleGraphEvent_EC_AMTC_BUFFER(p1, p2)



                        '===================================================================================================
                        '===================================================================================================
                        ' L21G
                        '=================================

                    Case 1074397284 'EC_L21G_BUFFER
                        HandleGraphEvent_EC_L21G_BUFFER(p1, p2)



                        '===================================================================================================
                        '===================================================================================================
                        ' NVIDIA VIDEO DECODER
                        '=================================

                    Case &H8065 'NVVIDDEC_EVENT_PICTURE_FRAGMENT
                        HandleGraphEvent_NVVIDDEC_EVENT_PICTURE_FRAGMENT(p1, p2)
                    Case &H8066 'NVVIDDEC_EVENT_MACROVISION_LEVEL
                        HandleGraphEvent_NVVIDDEC_EVENT_MACROVISION_LEVEL(p1, p2)



                    Case Else
                        RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "Unknown event: " & code & " p1: " & p1 & " p2: " & p2, Nothing, Nothing)

                End Select
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with GraphEventHandler(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_(ByRef p1 As Integer, ByRef p2 As Integer)
            Try

            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandlePlayerEvent_EC_DVD_NavigationCommand(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'GRAPH:EVENTS:CORE

#Region "GRAPH:EVENTS:FGM"

        Private Sub HandleGraphEvent_EC_STREAM_ERROR_STILLPLAYING(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_STREAM_ERROR_STILLPLAYING")
            Marshal.ThrowExceptionForHR(p1)
        End Sub

        Private Sub HandleGraphEvent_EC_QUALITY_CHANGE(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_QUALITY_CHANGE")
        End Sub

        Private Sub HandleGraphEvent_EC_VMR_RENDERDEVICE_SET(ByRef p1 As Integer, ByRef p2 As Integer)
            Select Case p1
                Case 1
                    Debug.WriteLine("VMR RENDERER: Overlay")
                Case 2
                    Debug.WriteLine("VMR RENDERER: Video Memory")
                Case 4
                    Debug.WriteLine("VMR RENDERER: System Memory")
            End Select
        End Sub

        Private Sub HandleGraphEvent_EC_PAUSED(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_PAUSED")
        End Sub

        Private Sub HandleGraphEvent_EC_CLOCK_CHANGED(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_CLOCK_CHANGED")
        End Sub

        Private Sub HandleGraphEvent_EC_GRAPH_CHANGED(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_GRAPH_CHANGED")
        End Sub

        Private Sub HandleGraphEvent_EC_VIDEO_SIZE_CHANGED(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Debug.WriteLine("EC_VIDEO_SIZE_CHANGED")
                'Dim xP1 As String = Hex(p1)
                'Dim H, W As Short
                'Dim sH, sW As Short
                'sH = Microsoft.VisualBasic.Left(xP1, 3)
                'sW = Microsoft.VisualBasic.Right(xP1, 3)
                'H = HEXtoDEC(sH)
                'W = HEXtoDEC(sW)
                'RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Video size changed. H: " & H & " W: " & W)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_VIDEO_SIZE_CHANGED(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_USERABORT(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Debug.WriteLine("EC_USERABORT")
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "EVENT: User Abort", Nothing, Nothing)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_USERABORT(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_ERRORABORT(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Try
                    Marshal.ThrowExceptionForHR(p1)
                Catch ex As Exception
                    Debug.WriteLine("EC_ERRORABORT. Error: " & ex.Message)
                End Try
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_ERRORABORT(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'GRAPH:EVENTS:FGM

#Region "GRAPH:EVENTS:KEYSTONE"

        Private Sub HandleGraphEvent_EC_KEYSTONE_TRIAL_FRAMECOUNT(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                'Me.EjectProject()
                RaiseEvent evKeystoneTimeout()
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandlePlayerEvent_EC_DVD_NavigationCommand(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_KEYSTONE_QUALITY_CHANGE(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Debug.WriteLine("EC_KEYSTONE_QUALITY_CHANGE")
                'EC_QUALITY_CHANGE_KEYSTONE
                'RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Keystone Quality Change:" & vbNewLine & vbTab & "Proportion: " & p1 & vbNewLine & vbTab & "Late: " & p2)
                'Debug.WriteLine("Keystone Quality Change:" & vbNewLine & vbTab & "Proportion: " & p1 & vbNewLine & vbTab & "Late: " & p2)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_KEYSTONE_QUALITY_CHANGE(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_KEYSTONE_FRAMERECEIVED(ByRef p1 As Integer, ByRef p2 As Integer)
            'Debug.WriteLine("EVENT: EC_KEYSTONE_FRAMERECEIVED")
            VideoFrameReceived_Delta = 0
            LastVideoFrameReceived_Ticks = DateTime.Now.Ticks
        End Sub

        Private Sub HandleGraphEvent_EC_KEYSTONE_FRAMEDELIVERED(ByRef p1 As Integer, ByRef p2 As Integer)
            ''Debug.WriteLine("EVENT: EC_KEYSTONE_FRAMEDELIVERED")
            'VideoFrameDelivered_Delta = TicksToMilliseconds(DateTime.Now.Ticks - LastFrameDelivered_Ticks)
            ''If Not VideoIsRunning Then ReSizeMixSendLastFrame()
            'LastFrameDelivered_Ticks = DateTime.Now.Ticks
        End Sub

        Private Sub HandleGraphEvent_EC_KEYSTONE_SAMPLETIMES(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EVENT: Keystone Sample Times: " & p1 & " - " & p2)
        End Sub

        Private Sub HandleGraphEvent_EC_KEYSTONE_32(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Debug.WriteLine("EC_KEYSTONE_32")
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
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_KEYSTONE_32(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_KEYSTONE_FIELDORDER(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                'Debug.WriteLine("EVENT: EC_KEYSTONE_FIELDORDER")
                If p1 = 1 Then
                    TopFieldFirst = True
                Else
                    TopFieldFirst = False
                End If
                RaiseEvent evKEYSTONE_FieldOrder(TopFieldFirst)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_KEYSTONE_FIELDORDER(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_KEYSTONE_BARDATA_TOP_BOTTOM(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Me._TopBar = p1
                Me._BottomBar = p2
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_KEYSTONE_BARDATA_TOP_BOTTOM(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_KEYSTONE_BARDATA_LEFT_RIGHT(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Me._LeftBar = p1
                Me._RightBar = p2
                'Debug.WriteLine(LeftBar & " * " & TopBar & " * " & RightBar & " * " & BottomBar)
                RaiseEvent evBarDataChanged()
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_KEYSTONE_BARDATA_LEFT_RIGHT(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_KEYSTONE_BARDATA_FRAME_TOO_DARK(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                RaiseEvent evBarDataTooDark()
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_KEYSTONE_BARDATA_FRAME_TOO_DARK(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_KEYSTONE_MPEGTC(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                'Debug.WriteLine("EVENT: EC_KEYSTONE_MPEGTC")

                Dim fps As Double
                If p2 = 2997 Then
                    fps = 29.97
                Else
                    fps = 25
                End If

                Dim hex As String = DecToHex(p1)
                If hex.Length < 8 Then
                    hex = PadString(hex, 8, "0", True)
                End If
                Dim f As String = HexToDec(Microsoft.VisualBasic.Right(hex, 2))
                Dim s As String = HexToDec(Microsoft.VisualBasic.Mid(hex, 5, 2))
                Dim m As String = HexToDec(Microsoft.VisualBasic.Mid(hex, 3, 2))
                Dim h As String = HexToDec(Microsoft.VisualBasic.Left(hex, 2))

                _LastGOPTC = New cTimecode(h, m, s, f, (CurrentVideoStandard = eVideoStandard.NTSC))
                _LastGOPTC_Ticks = DateTime.Now.Ticks

                If f.Length = 1 Then
                    f = PadString(f, 2, "0", True)
                End If
                If m.Length = 1 Then
                    m = PadString(m, 2, "0", True)
                End If
                If s.Length = 1 Then
                    s = PadString(s, 2, "0", True)
                End If

                'Debug.WriteLine(h & ":" & m & ":" & s & ";" & f)
                If Me.PlayState = ePlayState.FrameStepping Then
                    If Not f = 0 Then
                        f -= 1 'addresses confusion about difference betwen tc burn-in and that displayed here
                    End If
                End If

                RaiseEvent evMPEG_Timecode(New cTimecode(h, m, s, f, (CurrentVideoStandard = eVideoStandard.NTSC)))
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_KEYSTONE_MPEGTC(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_KEYSTONE_INTERLACING(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Debug.WriteLine("EVENT: EC_KEYSTONE_INTERLACING: " & p1)
                Interlaced = Not CBool(p1)
                RaiseEvent evKEYSTONE_Interlacing(Interlaced)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_KEYSTONE_INTERLACING(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_KEYSTONE_FORCEFRAMEGRAB(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Debug.WriteLine("EVENT: EC_KEYSTONE_FORCEFRAMEGRAB")
                'HandleKeystoneForcedFrameGrab(New IntPtr(p1), p2)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_KEYSTONE_FORCEFRAMEGRAB(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_KEYSTONE_PROGRESSIVESEQUENCE(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                'Debug.WriteLine("EVENT: EC_KEYSTONE_PROGRESSIVESEQUENCE")
                If p1 = 1 Then
                    ProgressiveSequence = True
                Else
                    ProgressiveSequence = False
                End If
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_KEYSTONE_PROGRESSIVESEQUENCE(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_KEYSTONE_DISCONTINUITY(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                If PlayState = ePlayState.FastForwarding Or PlayState = ePlayState.Rewinding Then Exit Sub
                Debug.WriteLine("EVENT: EC_KEYSTONE_DISCONTINUITY")
                'Debug.WriteLine("Discontinuity")
                If Me.Muting Then Me.UnMute()
                'Me.FrameRedeliveryTimer.Stop()

                'CurrentSpeed = FullSpeed

                'Debug.WriteLine("Ticks elapsed since VSETimer started: " & DateTime.Now.Ticks - VSETicks)
                If Not VSETimer Is Nothing Then
                    VSETimer.Stop()
                    VSETimer.Dispose()
                    VSETimer = Nothing
                    'Debug.WriteLine("Dis VSET: (should be true) " & (VSETimer Is Nothing))
                End If
                'Debug.WriteLine("-------------------------")
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_KEYSTONE_DISCONTINUITY(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_KEYSTONE_FRAMEDROPPED(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Debug.WriteLine("EC_KEYSTONE_FRAMEDROPPED")
                RaiseEvent evFrameDropped()
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_KEYSTONE_FRAMEDROPPED(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_KEYSTONE_WRONGDURATION(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Debug.WriteLine("EC_KEYSTONE_WRONGDURATION")
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_KEYSTONE_WRONGDURATION(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_KEYSTONE_SAMPLESNOTADJACENT(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                'Debug.WriteLine("EC_KEYSTONE_SAMPLESNOTADJACENT")
                'this should be resolved when we turn 3:2 back on and 
                'use a deinterlacing filter
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_KEYSTONE_SAMPLESNOTADJACENT(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_KEYSTONE_MACROVISION(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Debug.WriteLine("EVENT: EC_KEYSTONE_MACROVISION")
                Debug.WriteLine("Macrovision Level: " & p1)

                If PlayState <> ePlayState.SystemJP And PlayState <> ePlayState.Init And PlayerType = ePlayerType.DVD Then
                    If p1 = 0 Then
                        MacrovisionStatus = "Off"
                    Else
                        MacrovisionStatus = p1
                    End If
                End If
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_KEYSTONE_MACROVISION(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'GRAPH:EVENTS:KEYSTONE

#Region "GRAPH:EVENTS:DVD"

#Region "GRAPH:EVENTS:DVD::LOCATION"

        Private Sub HandleGraphEvent_EC_DVD_DOMAIN_CHANGE(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_DOMAIN_CHANGE: " & p1 & " " & p2)
            CurrentDomain_Events = p1
            RaiseEvent evDomainChange(p1)

            BarDataReset()
            If CurrentDomain = DvdDomain.Stop Then
                StopDomainImageSelection()
            End If

            If PlayState = ePlayState.FastForwarding Or PlayState = ePlayState.Rewinding Then
                HR = Graph.KO_IKeystone.DeactivateFFRW
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                PlayState = ePlayState.Playing
                _CurrentSpeed = OneX
            End If

            If PlayState = ePlayState.VariSpeed Then DeactivateVarispeed()

            ClearOSD()

            If CurrentDomain <> DvdDomain.Title Then Exit Sub

            'CHAPTERS
            HR = Graph.DVDInfo.GetNumberOfChapters(Me.CurrentTitle, _ChaptersInCurrentTitle)
            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

            'ANGLES
            HR = Graph.DVDInfo.GetCurrentAngle(_CurrentTitleAngleCount, _CurrentAngleStream)
            'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

            'TRT
            Dim TC_o As Integer
            HR = Graph.DVDInfo.GetTotalTitleTime(_CurrentTitleTRT, TC_o)
            If HR < 0 Then
                If HR <> -2147220873 Then  'it's a white hand for trying to get duration of an infinite still, infinity is a big number
                    Marshal.ThrowExceptionForHR(HR)
                End If
            End If

            'VTS
            CurrentVTS = CurrentGlobalTT.VTSN

        End Sub

        Private Sub HandleGraphEvent_EC_DVD_TITLE_SET_CHANGE(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_TITLE_SET_CHANGE: " & p1)
            CurrentVTS = p1
            BarDataReset()
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_TITLE_CHANGE(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_TITLE_CHANGE: " & p1 & " " & p2)
            RaiseEvent evLOG_TitleChange(p1)
TitleChange:
            UpdateUOPs()
            'DEBUGGING
            'Me.SendBlankAudioSample()
            'DEBUGGING

            BarDataReset()
            ClearOSD()

            If CurrentDomain <> DvdDomain.Title Then Exit Sub

            'CHAPTERS
            HR = Graph.DVDInfo.GetNumberOfChapters(Me.CurrentTitle, _ChaptersInCurrentTitle)
            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

            'ANGLES
            HR = Graph.DVDInfo.GetCurrentAngle(_CurrentTitleAngleCount, _CurrentAngleStream)
            'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

            'TRT
            Dim TC_o As Integer
            HR = Graph.DVDInfo.GetTotalTitleTime(_CurrentTitleTRT, TC_o)
            If HR < 0 Then
                If HR <> -2147220873 Then  'it's a white hand for trying to get duration of an infinite still, infinity is a big number
                    Marshal.ThrowExceptionForHR(HR)
                End If
            End If

            'VTS
            CurrentVTS = CurrentGlobalTT.VTSN

            RaiseEvent evTitleChange(Me.CurrentTitle)
            RaiseEvent evConsoleLine(eConsoleItemType.EVENT, "Title Start", p1, Nothing)
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_CHAPTER_START(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_CHAPTER_START: " & p1 & " " & p2)
            RaiseEvent evLOG_ChapterChange(p1)
            PingLB()
            _CurrentChapter = p1
            RaiseEvent evChapterChange(p1)
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_PROGRAM_CHAIN_CHANGE(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_PROGRAM_CHAIN_CHANGE: " & p1)
            CurrentPGC = p1
            UpdateUOPs()

            If CurrentDomain = DvdDomain.Title Then
                HR = Graph.DVDInfo.GetNumberOfChapters(Me.CurrentTitle, _ChaptersInCurrentTitle)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            End If

        End Sub

        Private Sub HandleGraphEvent_EC_DVD_PROGRAM_CELL_CHANGE(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_PROGRAM_CELL_CHANGE")
            If PlayState <> ePlayState.SystemJP And PlayState <> ePlayState.Init Then
                CurrentCell = p2
                CurrentPG = p1
            Else
                CurrentCell = 0
                CurrentPG = 0
            End If
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_CURRENT_HMSF_TIME(ByRef p1 As Integer, ByRef p2 As Integer)
            'If Me.DVDStartPauseSetItNow Then
            '    'DVDStartPauseSetItNow = False
            '    'HR = MediaCtrl.Pause()
            '    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            'End If

            Dim ati As Byte() = BitConverter.GetBytes(p1)
            _CurrentRunningTime.bHours = ati(0)
            _CurrentRunningTime.bMinutes = ati(1)
            _CurrentRunningTime.bSeconds = ati(2)
            _CurrentRunningTime.bFrames = ati(3)

            If Me.PlayState = ePlayState.ABLoop Then
                If CompareDVDTimecodes(Me.CurrentRunningTime_DVD, Me.ABLoop_PositionB.timeCode) = 1 Then
                    'time to loop 
                    Me.PlayAtTime_Clean(Me.ABLoop_PositionA.timeCode, False)
                End If
            End If

            'Debug.WriteLine("frames = " & _CurrentRunningTime.bFrames)

            If NonSeamlessCellNotification AndAlso CurrentDomain = DvdDomain.Title Then
                'could use cDetailedPlayLocation if we want to do this disc-wide.
                'currently only supporting notification in title space
                If NonSeamlessCells.CheckNotify(Me.CurrentTitle, Me.CurrentChapter, Me.CurrentRunningTime_DVD.bHours, Me.CurrentRunningTime_DVD.bMinutes, Me.CurrentRunningTime_DVD.bSeconds, CurrentVideoStandard = eVideoStandard.NTSC) Then
                    'Beep(1000, 100)
                    BurnInNSCDot()
                End If
            End If

            RaiseEvent evRunningTimeTick()
        End Sub

#End Region 'GRAPH:EVENTS:DVD::LOCATION

#Region "GRAPH:EVENTS:DVD::VOBU"

        Private Sub HandleGraphEvent_EC_DVD_VOBU_Offset(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Debug.WriteLine("EC_DVD_VOBU_Offset. Offset = " & p1 & "  VTSN = " & p2)
                CheckForHLI(p1)
                RaiseEvent evVOBUOffset(p1, p2)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_DVD_VOBU_Offset(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_VOBU_Timestamp(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Debug.WriteLine("EC_DVD_VOBU_Timestamp = " & p1 & " " & p2)
                'Dim t As Long = Graph.ReferenceTime
                'Debug.WriteLine("Graph Time = " & t)
                'Debug.WriteLine("Delta = " & p1 - t)
                RaiseEvent evVOBUTimestamp((p1 << 32) Or p2)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_DVD_VOBU_Timestamp(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub CheckForHLI(ByVal Offset As UInt64)
            Try
                'Dim p As String = Path.GetDirectoryName(FilePath) & "\"

                'Select Case CurrentDomain_Events
                '    Case DvdDomain.FirstPlay, DvdDomain.VideoManagerMenu
                '        p &= "video_ts.vob"
                '    Case DvdDomain.Nav_Not_Initialized, DvdDomain.Stop
                '        Exit Sub
                '    Case DvdDomain.Title
                '        p &= "vts_" & CurrentVTS.ToString.PadLeft(2, "0") & "_1.vob"
                '    Case DvdDomain.VideoTitleSetMenu
                '        p &= "vts_" & CurrentVTS.ToString.PadLeft(2, "0") & "_0.vob"
                'End Select

                'Dim V As New cVOBS(p)
                'Dim np As cVOBPack.cNavPacket = V.GetNextNavPack(Offset)

                'If np.PCIPacket.HLI.HLI_GI.HLI_SS <> cVOBPack.cNavPacket.cPCIPacket.cHLI.cHLI_GI.eHLI_SS.HLI_Non_Existant Then
                '    Debug.WriteLine("HLI EXISTS!")
                'End If

            Catch ex As Exception
                Throw New Exception("Problem with CheckForHLI(). Error: " & ex.Message, ex)
            End Try
        End Sub

#End Region 'GRAPH:EVENTS:DVD::VOBU

#Region "GRAPH:EVENTS:DVD::PARAMETERS"

        Private Sub HandleGraphEvent_EC_DVD_GPRM_Change(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Debug.WriteLine("EC_DVD_GPRM_Change = " & p1 & " " & p2)
                _CurrentGPRMs_evt(p1) = p2
                RaiseEvent evGPRMChanged(p1, p2)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_DVD_GPRM_Change(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_SPRM_Change(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Debug.WriteLine("EC_DVD_SPRM_Change = " & p1 & " " & p2)
                _CurrentSPRMs_evt(p1) = p2
                RaiseEvent evSPRMChanged(p1, p2)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_DVD_SPRM_Change(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'GRAPH:EVENTS:DVD::PARAMETERS

#Region "GRAPH:EVENTS:DVD::NAVIGATION COMMANDS"

        Private Sub HandleGraphEvent_EC_DVD_BeginNavigationCommands(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Debug.WriteLine("EC_DVD_BeginNavigationCommands = " & p1 & " " & p2)
                Dim s As eNavCmdType = p1
                Debug.WriteLine("BEGIN | Source = " & s.ToString & " | " & CurrentDetailedPlayLocation.ToString)
                RaiseEvent evBeginNavCommands(p1, CurrentDetailedPlayLocation)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_DVD_BeginNavigationCommands(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_NavigationCommand(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Dim str As String = Hex(p2).PadLeft(8, "0") & Hex(p1).PadLeft(8, "0")
                Dim cmd As New cCMD(str, CurrentSPRMs_evt, CurrentGPRMs_evt)
                Debug.WriteLine("EC_DVD_NavigationCommand = " & cmd.ToString)
                RaiseEvent evNavCommand(cmd)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_DVD_NavigationCommand(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'GRAPH:EVENTS:DVD::NAVIGATION COMMANDS

#Region "GRAPH:EVENTS:DVD::OTHER"

        Private Sub HandleGraphEvent_EC_DVD_CMD_START(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_CMD_START")
            'If pendingCmd Then Trace.WriteLine("DvdCmdStart with pending")
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_CMD_END(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_CMD_END")
            'OnCmdComplete(p1, p2)
            'pendingCmd = False
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_STILL_ON(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_STILL_ON")
            'SetAR("DVDStillOn Event")
            If p1 = 0 Then
                _MenuMode = eMenuMode.Buttons
            Else
                _MenuMode = eMenuMode.Still
            End If
            _DVDIsInStill = True

            'Me.ResendLastSample(1)

            If p2 <> &HFFFFFFFF Then
                CellStillDuration = p2
                CellStillTimer = New System.Windows.Forms.Timer
                CellStillTimer.Interval = 1000
                CellStillTimer.Start()
            Else
                RaiseEvent evCellStillTime(&HFFFFFFFF)
            End If
            RaiseEvent evConsoleLine(eConsoleItemType.EVENT, "Still On", "", Nothing)
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_STILL_OFF(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_STILL_OFF")
            If MenuMode = eMenuMode.Still Then
                _MenuMode = eMenuMode.No
            End If
            If Not CellStillTimer Is Nothing Then
                CellStillTimer.Stop()
                CellStillTimer = Nothing
            End If
            RaiseEvent evCellStillTime(0)
            _DVDIsInStill = False
            RaiseEvent evConsoleLine(eConsoleItemType.EVENT, "Still Off", "", Nothing)
            BarDataReset()
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_BUTTON_CHANGE(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_BUTTON_CHANGE: " & p1)
            If p1 <= 0 Then
                _MenuMode = eMenuMode.No
            Else
                _MenuMode = eMenuMode.Buttons
            End If
            'SetAR("DVDButtonChange Event")
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_NO_FP_PGC(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_NO_FP_PGC")
            If Not (Graph.DVDCtrl Is Nothing) Then
                RaiseEvent evConsoleLine(eConsoleItemType.WARNING, "No First Play PGC. Playing Title 1", Nothing, Nothing)
                HR = Graph.DVDCtrl.PlayTitle(1, DvdCmdFlags.None, Nothing)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            End If
            RaiseEvent evConsoleLine(eConsoleItemType.WARNING, "This title has no first play PGC.", "", Nothing)
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_AUDIO_STREAM_CHANGE(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_AUDIO_STREAM_CHANGE")
            If CurrentDomain <> DvdDomain.Title Then Exit Sub
            Dim NumberOfStreams, CurrentStream As Integer
            HR = Graph.DVDInfo.GetCurrentAudio(NumberOfStreams, CurrentStream)
            If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            RaiseEvent evAudioStreamChanged(CurrentStream, NumberOfStreams)
            RaiseEvent evConsoleLine(eConsoleItemType.EVENT, "Audio Stream Change", p1, Nothing)
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_SUBPICTURE_STREAM_CHANGE(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_SUBPICTURE_STREAM_CHANGE")
            RaiseEvent evSubtitleStreamChanged(p1)
            RaiseEvent evConsoleLine(eConsoleItemType.EVENT, "Subpicture Stream Change", p1, Nothing)
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_ANGLE_CHANGE(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_ANGLE_CHANGE")
            RaiseEvent evAngleChanged(p1)
            RaiseEvent evConsoleLine(eConsoleItemType.EVENT, "Angle Change", p1, Nothing)
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_PARENTAL_LEVEL_CHANGE(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_PARENTAL_LEVEL_CHANGE")
            RaiseEvent evConsoleLine(eConsoleItemType.EVENT, "Parental Level Change", p1, Nothing)
            Try
                'the disc is authored to set a temporary parental level 
                If MsgBox("DVD is attempting to set a temporary parental level." & vbNewLine & "Do you wish to allow this?", MsgBoxStyle.MsgBoxHelp.YesNo, "DVD Player") = MsgBoxResult.Yes Then
                    HR = Graph.DVDCtrl.AcceptParentalLevelChange(True)
                Else
                    'no
                    HR = Graph.DVDCtrl.AcceptParentalLevelChange(False)
                End If
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_DVD_PARENTAL_LEVEL_CHANGE(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_VALID_UOPS_CHANGE(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_VALID_UOPS_CHANGE")
            RaiseEvent evUserOperationsChanged(p1)
            'RaiseEvent evConsoleLine(eConsoleItemType.EVENT, "UOP Change", "")
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_ERROR(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_ERROR")
            'we're now handling this elsewhere
            Dim es As String = GetDVDErrorString(p1)
            If InStr(es.ToLower, "incompatiblediscanddecoderregions", CompareMethod.Text) Or InStr(es.ToLower, "incompatiblesystemanddecoderregions", CompareMethod.Text) Or InStr(es, "DVD_ERROR_InvalidDiscRegion", CompareMethod.Text) Then
                RaiseEvent evWrongRegion(CByte(Region) + 1, CurrentDVD.VMGM.Regions)
            Else
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "DVD Error", p1, Nothing)
            End If
            Debug.WriteLine("Unhandled DVD Error")
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_WARNING(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_WARNING")
            Dim s As String = GetDVDWarningString(p1)
            Dim t As String = ""
            Select Case s.ToLower
                Case "open", "seek", "read"
                    t = "One of the DVD image files cannot be opened."
            End Select
            RaiseEvent evConsoleLine(eConsoleItemType.WARNING, "DVD_WARNING_" & s & " | " & p2 & vbNewLine & t, Nothing, Nothing)
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_PLAYBACK_STOPPED(ByRef p1 As Integer, ByRef p2 As Integer)
            Dim cause As ePlaybackStoppedCauses = p1
            Debug.WriteLine("EC_DVD_PLAYBACK_STOPPED. Cause: " & cause.ToString)
            'RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "dvd playback stopped, Ps: " & GetPlaybackStoppedString(p1) & " | " & p2)
            RaiseEvent evConsoleLine(eConsoleItemType.EVENT, "Playback Stopped", p1, Nothing)
            BarDataReset()
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_BUTTON_AUTO_ACTIVATED(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_BUTTON_AUTO_ACTIVATED")
            RaiseEvent evConsoleLine(eConsoleItemType.EVENT, "Button Auto Activate", p1, Nothing)
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_CHAPTER_AUTOSTOP(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_CHAPTER_AUTOSTOP")
            RaiseEvent evConsoleLine(eConsoleItemType.EVENT, "Chapter Auto Stop", p1, Nothing)
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_KARAOKE_MODE(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_KARAOKE_MODE")
            RaiseEvent evConsoleLine(eConsoleItemType.EVENT, "Karaoke Mode Change", p1, Nothing)
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_PLAYPERIOD_AUTOSTOP(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_DVD_PLAYPERIOD_AUTOSTOP")
            RaiseEvent evConsoleLine(eConsoleItemType.EVENT, "Play Period Auto Stop", "", Nothing)
        End Sub

        Private Sub HandleGraphEvent_EC_DVD_PLAYBACK_RATE_CHANGE(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Debug.WriteLine("EVENT: EC_DVD_PLAYBACK_RATE_CHANGE. Rate: " & p1)
                If _CurrentPlaybackRate > p1 And p1 = 10000 Then
                    'returned to normal speed after fast forward
                    Me.UnMute()
                End If
                _CurrentPlaybackRate = p1

                '[TRPF 070426] Commented to fix ff/rw issues with tcs content. 
                'If p1 = 10000 Then
                '    HR = Me.Graph.KO_IKeystone.DeactivateFFRW
                '    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                '    PlayState = ePlayState.Playing
                '    _CurrentSpeed = OneX
                '    Me.UnMute()
                '    Me.ReSyncAudio(False)
                '    RaiseEvent evDVDPlaybackRateReturnedToOneX()
                '    Me.ClearOSD()
                'End If
                'RaiseEvent evConsoleLine(eConsoleItemType.EVENT, "Playback Rate Change", p1)

                '1000 = 1x
                '-4000 = 4x reverse
                '4000 = 4x forwards
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_EC_DVD_PLAYBACK_RATE_CHANGE(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'GRAPH:EVENTS:DVD::OTHER

#End Region 'GRAPH:EVENTS:DVD

#Region "GRAPH:EVENTS:NVVIDDEC"

        Private Sub HandleGraphEvent_NVVIDDEC_EVENT_PICTURE_FRAGMENT(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Debug.WriteLine("EVENT: NVVIDDEC_EVENT_PICTURE_FRAGMENT")
                If PlayState = ePlayState.FastForwarding Or PlayState = ePlayState.Rewinding Then Exit Sub
                'If PlayState = ePlayState.FastForwarding Or PlayState = ePlayState.Rewinding Then Exit Sub
                If CurrentMPEGFrameMode > 1 Then Exit Sub 'we're in MPEG frame filtering, don't throw tranfer errors
                If VSETimer Is Nothing Then
                    VSETC = New sTransferErrorTime
                    VSETC.SourceTime = LastGOPTC
                    Me.Graph.DVDInfo.GetCurrentLocation(VSETC.RunningTime)
                    'Debug.WriteLine("Starting VSETimer")
                    VSETimer = New System.Timers.Timer(2200) 'adjust this larger until you get no false video error notifications
                    AddHandler VSETimer.Elapsed, AddressOf VSETimerTick
                    VSETimer.AutoReset = False
                    VSETimer.Enabled = True
                    VSETicks = DateTime.Now.Ticks
                Else
                    'Debug.WriteLine("Not starting VSETimer")
                End If
                'Debug.WriteLine("-------------------------")
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_NVVIDDEC_EVENT_PICTURE_FRAGMENT(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleGraphEvent_NVVIDDEC_EVENT_MACROVISION_LEVEL(ByRef p1 As Integer, ByRef p2 As Integer)
            Try
                Debug.WriteLine("EVENT: NVVIDDEC_EVENT_MACROVISION_LEVEL: " & p1 & " " & p2)
                'we're getting it from Keystone
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleGraphEvent_NVVIDDEC_EVENT_MACROVISION_LEVEL(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'GRAPH:EVENTS:NVVIDDEC

#Region "GRAPH:EVENTS:AMTC"

        Private Sub HandleGraphEvent_EC_AMTC_SAMPLE_TIMES(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EC_AMTC_SAMPLE_TIMES " & p1 & " - " & p2)
        End Sub

        Private Sub HandleGraphEvent_EC_AMTC_BUFFER(ByRef p1 As Integer, ByRef p2 As Integer)
            Debug.WriteLine("EVENT: EC_AMTC_BUFFER")
            If p1 > 0 Then
                GetBufferFromAMTC()
            End If
        End Sub

        Private Sub HandleGraphEvent_EC_AMTC2_FRAMERECEIVED(ByRef p1 As Integer, ByRef p2 As Integer)
            'AudioFrameReceived_Delta = 0
            'LastAudioFrameReceived_Ticks = DateTime.Now.Ticks
        End Sub

#End Region 'GRAPH:EVENTS:AMTC

#Region "GRAPH:EVENTS:L21G"

        Private Sub HandleGraphEvent_EC_L21G_BUFFER(ByRef p1 As Integer, ByRef p2 As Integer)
            'Debug.WriteLine("EC_L21G_BUFFER")
            If p1 > 0 Then
                GetL21BufferFromL21G()
            End If
        End Sub

#End Region 'GRAPH:EVENTS:L21G

        'Private Sub OnCmdComplete(ByVal p1 As Integer, ByVal hrg As Integer)
        '    'If CurrentDomain = DvdDomain.VideoManagerMenu Or CurrentDomain = DvdDomain.VideoTitleSetMenu Then
        '    '    SetAR()
        '    'End If

        '    ' Trace.WriteLine( "DVD OnCmdComplete.........." );
        '    If pendingCmd = False Or Graph.DVDInfo Is Nothing Then Return
        '    Dim cmd As IDvdCmd
        '    Dim hr As Integer = Graph.DVDInfo.GetCmdFromEvent(p1, cmd)
        '    If hr <> 0 Or cmd Is Nothing Then
        '        Debug.WriteLine("!!!DVD OnCmdComplete GetCmdFromEvent failed!!!")
        '        Return
        '    End If

        '    If Not cmd Is cmdOption.dvdCmd Then
        '        Debug.WriteLine("DVD OnCmdComplete UNKNOWN CMD!!!")
        '        Marshal.FinalReleaseComObject(cmd)
        '        cmd = Nothing
        '        Return
        '    End If

        '    Marshal.FinalReleaseComObject(cmd)
        '    cmd = Nothing
        '    Marshal.FinalReleaseComObject(cmdOption.dvdCmd)
        '    cmdOption.dvdCmd = Nothing
        '    pendingCmd = False
        '    ' Trace.WriteLine( "DVD OnCmdComplete OK." );
        'End Sub

#End Region 'GRAPH:EVENTS

#Region "PRIVATE:METHODS:PLAY CONTROL:STATE"

        Private ReadOnly Property StateStorePath() As String
            Get
                Dim store_path As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                store_path &= "\phoenix_state.bmk"
                Return store_path
            End Get
        End Property

        Private StateName As String = "PhoenixStopBookmark"

        Private Function StoreState() As Boolean
            Try
                HR = Graph.DVDInfo.GetState(DVDState)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim storage As IStorage = Nothing
                Dim stream As IStream = Nothing

                Try
                    HR = NativeMethods.StgCreateDocfile(StateStorePath, STGM.Create Or STGM.Write Or STGM.ShareExclusive, 0, storage)
                    Marshal.ThrowExceptionForHR(HR)
                    HR = storage.CreateStream(StateName, STGM.Write Or STGM.Create Or STGM.ShareExclusive, 0, 0, stream)
                    Marshal.ThrowExceptionForHR(HR)

                    Dim ipst As IPersistStream = CType(DVDState, IPersistStream)

                    HR = ipst.Save(stream, True)
                    Marshal.ThrowExceptionForHR(HR)

                    HR = storage.Commit(STGC.[Default])
                    Marshal.ThrowExceptionForHR(HR)
                Finally
                    If stream IsNot Nothing Then
                        Marshal.FinalReleaseComObject(stream)
                    End If
                    If storage IsNot Nothing Then
                        Marshal.FinalReleaseComObject(storage)
                    End If
                End Try
                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with StoreState. Error: " & ex.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

        Private Function RestoreFromState() As Boolean
            Try
                Dim storage As IStorage = Nothing
                Dim stream As System.Runtime.InteropServices.ComTypes.IStream = Nothing

                Try
                    If NativeMethods.StgIsStorageFile(StateStorePath) <> 0 Then
                        Return False
                    End If

                    HR = NativeMethods.StgOpenStorage(StateStorePath, Nothing, STGM.Read Or STGM.ShareExclusive, IntPtr.Zero, 0, storage)
                    Marshal.ThrowExceptionForHR(HR)

                    HR = storage.OpenStream(StateName, IntPtr.Zero, STGM.Read Or STGM.ShareExclusive, 0, stream)

                    If HR >= 0 Then
                        HR = TryCast(DVDState, IPersistStream).Load(stream)
                        Marshal.ThrowExceptionForHR(HR)
                    End If
                Finally
                    If stream IsNot Nothing Then
                        Marshal.FinalReleaseComObject(stream)
                    End If
                    If storage IsNot Nothing Then
                        Marshal.FinalReleaseComObject(storage)
                    End If
                End Try

                If Graph.MediaControlState <> eMediaControlState.Running Then
                    Graph.MediaCtrl.Run()
                End If

                HR = Graph.DVDCtrl.SetState(DVDState, DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with RestoreFromState. Error: " & ex.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

#End Region '"PRIVATE:METHODS:PLAY CONTROL:STATE"

#Region "Menus"

        Private BtnQ As Queue

        Private HitCount As Short
        Private ExecuteCount As Short
        Private BtnThread As Thread

        Private BtnPollerRunning As Boolean

        Private Sub StartBtnPoller()
            Try
                If Me.BtnPollerRunning = True Then Exit Sub
                Me.BtnPollerRunning = True
                'Debug.WriteLine("Button Poller Running")
NextPoll:
                'Debug.WriteLine("BtnQ count: " & BtnQ.Count)

                While True
                    Thread.Sleep(100)
                    If BtnQ.Count > 0 Then
                        'ExecuteCount += 1
                        HR = Graph.DVDCtrl.SelectRelativeButton(BtnQ.Dequeue)
                        If HR < 0 Then Debug.WriteLine("BtnError: " & HR)
                    End If
                End While

                'If BtnQ.Count > 0 Then
                '    ExecuteCount += 1
                '    hr = Graph.DVDCtrl.SelectRelativeButton(BtnQ.Dequeue)
                '    'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                '    Threading.Thread.Sleep(125)
                '    GoTo NextPoll
                'End If

                Me.BtnPollerRunning = False
                'Debug.WriteLine("Button Poller Stopped")
                'Debug.WriteLine("HitCount: " & Me.HitCount)
                'Debug.WriteLine("ExecuteCount: " & Me.ExecuteCount)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "StartBtnPoller failed. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'Menus

#Region "Audio"

        'Public Sub SendBlankAudioSample()
        '    Try
        '        HR = Graph.AMTC_Interface.SendInititalizationSample
        '        If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
        '    Catch ex As Exception
        '        RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with SendBlankAudioSample().", ex.Message, Nothing)
        '    End Try
        'End Sub

        Private Function CheckIfStreamIsDTS(ByVal StreamNumber As Short) As Boolean
            Try
                If InStr(GetAudio(StreamNumber).Format.ToLower, "dts", CompareMethod.Text) Then
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem checking if stream is DTS. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

        Private Function GetAudioExtensionName(ByVal ExtCode As String) As String
            Select Case ExtCode
                Case "0"
                    Return "NotSpecified"
                Case "1"
                    Return "Captions"
                Case "2"
                    Return "Visually Impaired"
                Case "3"
                    Return "Director Comments 1"
                Case "4"
                    Return "Director Comments 2"
            End Select
        End Function

        Private Function GetAudioExtensionFromString(ByVal AudExt As String) As String
            Select Case AudExt
                Case "Not Specified"
                    Return "0"
                Case "Captions"
                    Return "1"
                Case "Visually Impaired"
                    Return "2"
                Case "Director Comments 1"
                    Return "3"
                Case "Director Comments 2"
                    Return "4"
            End Select
        End Function

#Region "Audio Dumping"

        Private AudioDumpFile As FileStream
        Private tAudBuff() As Byte
        Private StartNewAudioFile As Boolean

        Private Sub GetBufferFromAMTC()
            Try
                'Debug.WriteLine("GetBufferFromAMTC")
                If Not Me.DumpAudio Then Exit Sub
                If AudioDumpFile Is Nothing Then
                    Dim AudFileName As String = Me.AudioDumpDir
                    If Not Directory.Exists(AudFileName) Then
                        Directory.CreateDirectory(AudFileName)
                    End If
                    AudFileName &= "\AudioDump_" & Date.Now.Year & Date.Now.Month & Date.Now.Day & "_" & DateTime.Now.Ticks
                    AudFileName &= "_" & Trim(Me.CurrentAudioLanguage) & "." & Trim(Me.CurrentAudioExtension)
                    AudioDumpFile = New FileStream(AudFileName, FileMode.OpenOrCreate)
                End If

                Dim SamplePtr As IntPtr
                Dim BufferSize As Integer
                HR = Graph.AMTC_Interface.GetBuffer(SamplePtr, BufferSize)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                ReDim tAudBuff((BufferSize - 1) / 2)
                Marshal.Copy(SamplePtr, tAudBuff, 0, BufferSize / 2)
                'AudioDumpFile.Write(tAudBuff, 0, tAudBuff.Length)

                'first, swab the bytes
                tAudBuff = SwabBytes(tAudBuff)

                'first figure out if we're using DTS or AC3
                'the lower half of the sixth byte tells us if its DTS, AC3, or PCM
                Dim s As String = DecimalToBinary(tAudBuff(5))
                s = PadString(s, 4, "0", True)
                s = Microsoft.VisualBasic.Right(s, 4)
                Dim Format As String
                Select Case s
                    Case "1011"
                        Format = "dts"
                    Case "0001"
                        Format = "ac3"
                End Select

                Dim FrameBuff() As Byte
                Dim FrameSize As Short

                'ok, remember that each sync frame has 8 extra bytes at it's start for the spdif header
                'what we're going to do here:
                'go through the buffer looking for frames.
                'write each frame to the out file

                Select Case Format
                    Case "dts"
                        FrameSize = CInt("&H" & Hex(tAudBuff(6)) & Hex(tAudBuff(7))) / 8

                        Dim sCur As Long = 8
NewDTSSyncFrame:
                        ReDim FrameBuff(FrameSize - 1)
                        'Debug.WriteLine(sCur)
                        Array.Copy(tAudBuff, sCur, FrameBuff, 0, FrameSize)
                        AudioDumpFile.Write(FrameBuff, 0, FrameSize)
                        sCur += FrameSize

                        For i As Integer = sCur To UBound(tAudBuff)
                            If tAudBuff(i) = 127 Then
                                If tAudBuff(i + 1) = 254 Then
                                    'we've found a new sync frame
                                    sCur = i
                                    GoTo NewDTSSyncFrame
                                End If
                            End If
                        Next

                    Case "ac3"
                        Dim sCur As Long = 8

                        'get AC3 header info
                        Dim SI(4) As Byte
                        Array.Copy(tAudBuff, 8, SI, 0, 5)
                        Dim tSI As New cAC3SyncInfo(SI)
                        FrameSize = tSI.FrameSize
                        'Debug.WriteLine("FrameSize= " & FrameSize)

NewAC3SyncFrame:
                        ReDim FrameBuff(FrameSize - 1)
                        Array.Copy(tAudBuff, sCur, FrameBuff, 0, FrameSize)
                        AudioDumpFile.Write(FrameBuff, 0, FrameSize)
                        sCur += FrameSize

                        For i As Integer = sCur To UBound(tAudBuff)
                            If tAudBuff(i) = 11 Then
                                If tAudBuff(i + 1) = 119 Then
                                    'we've found a new sync frame
                                    sCur = i
                                    GoTo Newac3SyncFrame
                                End If
                            End If
                        Next

                    Case "pcm"

                End Select
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with GetBufferFromAMTC. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'Audio Dumping

#End Region 'Audio

#Region "Subtitles"

        Private Function GetSubtitleLanguage(ByVal StreamNumber As Short) As String
            Try
                Dim LCID As Integer
                Dim hr As Integer = Graph.DVDInfo.GetSubpictureLanguage(StreamNumber, LCID)
                If hr < 0 Then
                    Marshal.ThrowExceptionForHR(hr)
                Else
                    Dim Lang As String = GetUserLocaleInfo(LCID, LOCALE_SENGLANGUAGE)
                    Return Lang
                End If
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "error getting language. error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

        Private Function GetSubtitleExtensionName(ByVal ExtCode As String) As String
            Select Case ExtCode
                Case "0"
                    Return "NotSpecified"
                Case "1"
                    Return "Caption Normal"
                Case "2"
                    Return "Caption Big"
                Case "3"
                    Return "Caption Children"
                Case "5"
                    Return "CC Normal"
                Case "6"
                    Return "CC Big"
                Case "7"
                    Return "CC Children"
                Case "9"
                    Return "Forced"
                Case "13"
                    Return "Director Comments Normal"
                Case "14"
                    Return "Director Comments Big"
                Case "15"
                    Return "Director Comments Children"
            End Select
        End Function

        Private Function GetSubtitleTypeAsString(ByVal TypeCode As String) As String
            Select Case TypeCode
                Case "0"
                    Return "NotSpecified"
                Case "1"
                    Return "Language"
                Case "2"
                    Return "Other"
            End Select
        End Function

        Private Function GetSubtitleEncoding(ByVal CodingCode As String) As String
            Try
                Select Case CodingCode
                    Case 0
                        Return "Run Length"
                    Case 1
                        Return "Extended"
                    Case 2
                        Return "Other"
                End Select
            Catch ex As Exception
                Return "N/A - error"
            End Try
        End Function

#End Region 'Subtitles

#Region "Video"

        Private Sub VideoFrameReceivedTimer_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles VideoFrameReceived_Timer.Tick
            Try
                If Me.AVMode = eAVMode.DesktopVMR Then Exit Sub

                If Not PlayState = ePlayState.Playing Or Me.CurrentDomain = DvdDomain.Stop Then Exit Sub
                If Me.VidCorruptionDlgDisplayed Then Exit Sub
                Dim DT As Long = DateTime.Now.Ticks - LastVideoFrameReceived_Ticks
                'Debug.WriteLine("DTNT: " & DTNT)
                'Debug.WriteLine("LFRT: " & LFRT)
                'Debug.WriteLine("DT: " & DT)
                VideoFrameReceived_Delta = TicksToMilliseconds(DT)

                If VideoFrameReceived_Delta > 500 Then '500 = 11 frames of tolerance. If we should have received 11 frames but have received none it's safe to say that video has stopped.
                    Me.ResendLastSample(0)
                End If

            Catch ex As Exception
                Throw New Exception("Problem with VideoFrameDeliveredTimer_Tick(). Error: " & ex.Message, ex)
            End Try
        End Sub

        Public Sub ResendLastSample(ByVal Unconditional As Integer)
            Try
                HR = Graph.KO_IKeystone.ResendLastSamp(Unconditional)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with ResendLastSample().", ex.Message, Nothing)
            End Try
        End Sub

        Private Sub ActivateVarispeed(ByVal Factor As Double) '4 = 1/4
            Try
                If PlayState = ePlayState.Paused Then
                    HR = Graph.DVDCtrl.Pause(False)
                    If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                End If
                If PlayState = ePlayState.FrameStepping Then Me.QuitFrameStepping()
                HR = Graph.KO_IKeystone.ActivateVarispeed(Factor) 'FFRWRate
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                PlayState = ePlayState.VariSpeed
                Me.Mute()
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with ActivateVarispeed. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub DeactivateVarispeed()
            Try
                PlayState = ePlayState.Playing
                HR = Graph.KO_IKeystone.DeactivateVarispeed()
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                Dim tloc As DvdPlayLocation
                HR = Graph.DVDInfo.GetCurrentLocation(tloc)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                PlayAtTime(tloc.timeCode, True)
                tloc = Nothing
                Me.UnMute()
                Me.ReSyncAudio(False)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with DeactivateVarispeed. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#Region "Corrupted Video"

        Private VSETimer As System.Timers.Timer

        Private Sub VSETimerTick(ByVal source As Object, ByVal e As System.Timers.ElapsedEventArgs)
            'this timer system is intended to prevent stream error notification in discontinuity circumstances
            'no discontinuity received so it was a notifiyable error
            'Debug.WriteLine("VSETimer Tick")
            'Debug.WriteLine("Ticks elapsed since VSETimer started: " & DateTime.Now.Ticks - VSETicks)
            'Debug.WriteLine("-------------------------")
            VSETimer.Dispose()
            VSETimer = Nothing
            'HandleCorruptVideoDataEvent()
        End Sub

        'Private Delegate Sub PassSetCorruptVideoNotificationBitmap()
        'Private m_PassSetOSD As PassSetCorruptVideoNotificationBitmap

        'Private Delegate Sub PassClearOSD()
        'Private m_PassClearOSD As PassClearOSD

        Private Delegate Sub PassVideoIsCorrupted()
        Private m_PassVideoIsCorrupted As PassVideoIsCorrupted

        Private VSETicks As Long
        Private Shared VSETC As sTransferErrorTime

        'Private Sub SetCorruptVideoNotificationBitmap()
        '    Try
        '        RaiseEvent evCorruptedVideoData(VSETC)

        '        If Me.VidCorruptionDlgDisplayed Then Exit Sub
        '        Me.VidCorruptionDlgDisplayed = True

        '        Me.ClearOSD()
        '        'Invoke(m_PassClearOSD)
        '        Dim BM As New Bitmap(425, 20, Imaging.PixelFormat.Format24bppRgb)
        '        Dim G As Graphics = Graphics.FromImage(BM)
        '        Dim B As Brush = New SolidBrush(Color.White) 'for keying
        '        G.FillRectangle(B, 0, 0, 425, 20)

        '        Dim text_path As New System.Drawing.Drawing2D.GraphicsPath(Drawing2D.FillMode.Alternate)
        '        Dim str As String = "CORRUPTED VIDEO DATA"
        '        If VSETC.ToString <> "" Then
        '            str &= " @  " & VSETC.ToString
        '        End If

        '        text_path.AddString(str, New FontFamily("Arial"), FontStyle.Regular, 16, New Point(0, 0), StringFormat.GenericDefault)
        '        G.DrawPath(New Pen(NTSCBlack, 2), text_path)
        '        G.FillPath(New SolidBrush(Me.NTSCBlue), text_path)
        '        text_path.Dispose()
        '        G.Dispose()
        '        'BM.Save("c:\temp.bmp")

        '        SetOSD(BM, RGB(255, 255, 255), 32000, True, 1, "CorruptedVideoData", 55, 45)

        '        'Dim fn As New Font(System.Drawing.FontFamily.GenericSansSerif, 18, FontStyle.Bold, GraphicsUnit.Point)
        '        'Dim br As New SolidBrush(Color.FromArgb(255, 235, 16, 16))
        '        'G.DrawString(str, fn, br, 0, 0)
        '        'BM.Save("c:\temp.bmp")
        '        'Return BM

        '        'Dim fn As New Font(System.Drawing.FontFamily.GenericSansSerif, 10, FontStyle.Bold, GraphicsUnit.Point)
        '        'Dim br As New SolidBrush(Color.FromArgb(255, 235, 16, 16))
        '        'Dim B As Brush = New SolidBrush(Color.White) 'for keying
        '        'g.FillRectangle(B, 0, 0, 720, 480)
        '        ''Dim sf As New StringFormat
        '        ''sf.Alignment = StringAlignment.left
        '        ''sf.LineAlignment = StringAlignment.lef

        '    Catch ex As Exception
        '        RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with CreateCorruptVideoNotificationBitmap. Error: " & ex.Message, Nothing, Nothing)
        '    End Try
        'End Sub

        Private Sub RaiseVideoIsCorruptedEvent()
            RaiseEvent evCorruptedVideoData(VSETC)
        End Sub

        Private VidCorruptionDlgDisplayed As Boolean = False
        Private Sub HandleCorruptVideoDataEvent()
            Try
                Me.VidCorruptionDlgDisplayed = True
                'RaiseEvent evConsoleLine(eConsoleItemType.WARNING, "Corrupted video data.", VSETC.RunningTime.ToString, VSETC.SourceTime)
                ParentForm.WindowsForm.Invoke(m_PassVideoIsCorrupted)
                'ParentForm.Invoke(m_PassClearOSD)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with HandleCorruptVideoDataEvent. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Sub CloseCorruptVideoDataDlg()
            Me.VidCorruptionDlgDisplayed = False
        End Sub

#End Region

#End Region 'Video

#Region "Angle"

        Private Sub ToggleAngle(ByVal Up As Boolean)
            Try
                If Graph.DVDCtrl Is Nothing Or Graph.DVDInfo Is Nothing Then Exit Sub
                Dim NumberOfStreams, CurrentStream As Integer
                Dim hr As Integer = Graph.DVDInfo.GetCurrentAngle(NumberOfStreams, CurrentStream)
                If CurrentStream = NumberOfStreams - 1 And Up Then
                    CurrentStream = 0
                ElseIf CurrentStream = 0 And Not Up Then
                    CurrentStream = NumberOfStreams - 1
                Else
                    If Up Then
                        CurrentStream += 1
                    Else
                        CurrentStream -= 1
                    End If
                End If
                hr = Graph.DVDCtrl.SelectAngle(CurrentStream, DvdCmdFlags.SendEvt, cmdOption)
                If hr < 0 Then Marshal.ThrowExceptionForHR(hr)
            Catch ex As Exception
                If CheckEx(ex, "Toggle Angle") Then Exit Sub
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem toggling angle: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'Angle

#Region "Closed Captions"

        Private Sub TurnOnCCs()
            Try
                If PlayState = ePlayState.Stopped Or PlayState = ePlayState.Init Then Exit Sub
                HR = Graph.Line21.SetServiceState(AMLine21_CCState.On)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = Graph.KO_IKeystone.SetL21State(True)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                _ClosedCaptionsAreOn = True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem toggling CCs. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Function TurnOffCCs() As Boolean
            Try
                If PlayState = PlayState.Stopped Or PlayState = PlayState.Init Then Exit Function
                Dim HR As Integer = Graph.Line21.SetServiceState(AMLine21_CCState.Off)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                HR = Graph.KO_IKeystone.SetL21State(False)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                _ClosedCaptionsAreOn = False
                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem toggling CCs. Error: " & ex.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

#Region "Logging"

        Private ClosedCaptionLogFile As FileStream
        Private ClosedCaptionLogWriter As StreamWriter
        Private WithEvents ClosedCaptionProcessing As cLine21Processing
        Private TempClosedCaptionBuffer() As Byte
        Private LogClosedCaptionCurrentRow As Short = 0
        Private ClosedCaptionCurrentLine As String

        Private Sub GetL21BufferFromL21G()
            Try
                If Not Me.ClosedCaptionLogging Then Exit Sub
                If ClosedCaptionProcessing Is Nothing Then ClosedCaptionProcessing = New cLine21Processing
                Dim SamplePtr As IntPtr
                Dim BufferSize As Integer
                HR = Graph.iL21G.GetBuffer(SamplePtr, BufferSize)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                ReDim TempClosedCaptionBuffer(BufferSize - 1)
                Marshal.Copy(SamplePtr, TempClosedCaptionBuffer, 0, BufferSize)
                ClosedCaptionProcessing.AddRawL21Data(TempClosedCaptionBuffer)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with GetL21BufferFromL21G. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub HandleNewL21Line(ByVal Line() As String) Handles ClosedCaptionProcessing.NewL21Line

            'setup log file
            If ClosedCaptionLogFile Is Nothing Then
                Dim LogPath As String = Me.ClosedCaptionDumpDir & "ClosedCaptionLog_" & Date.Now.Year & Date.Now.Month & Date.Now.Day & "_" & DateTime.Now.Ticks & ".txt"
                If Not Directory.Exists(Path.GetDirectoryName(LogPath)) Then Directory.CreateDirectory(Path.GetDirectoryName(LogPath))
                ClosedCaptionLogFile = New FileStream(LogPath, FileMode.OpenOrCreate)
                ClosedCaptionLogWriter = New StreamWriter(ClosedCaptionLogFile)
            End If

            For Each s As String In Line
                If s <> "" Then
                    If InStr(s, "CMD") And Me.ClosedCaptionLogging_IncludeCommands Then
                        Me.AddClosedCaptionLineToConsole(s)
                        ClosedCaptionLogWriter.Write(s)
                    End If

                    If InStr(s, "CMD") Then
                        If InStr(s, "ENM") Then
                            'time for a new line
                            'CN.txtConsole.Text &= vbNewLine
                            ClosedCaptionLogWriter.Write(ClosedCaptionCurrentLine & vbNewLine)
                            Me.AddClosedCaptionLineToConsole(ClosedCaptionCurrentLine)
                            ClosedCaptionCurrentLine = ""
                        End If
                        If InStr(s, "Row_") Then
                            'check rows and see if we need to change
                            Dim r() As String = Split(s, "_")
                            If Me.LogClosedCaptionCurrentRow < CShort(r(4)) Then
                                'we need to return
                                'CN.txtConsole.Text &= vbNewLine
                                ClosedCaptionLogWriter.Write(ClosedCaptionCurrentLine & vbNewLine)
                                Me.AddClosedCaptionLineToConsole(ClosedCaptionCurrentLine)
                                ClosedCaptionCurrentLine = ""
                                Me.LogClosedCaptionCurrentRow += 1
                            Else
                                'no return needed, but make sure we think we're at the right row
                                Me.LogClosedCaptionCurrentRow = r(4)
                            End If
                        End If
                    Else
                        ClosedCaptionCurrentLine &= s
                    End If
                End If
NextL21LogLine:
            Next
        End Sub

        Private Sub AddClosedCaptionLineToConsole(ByVal Line As String)
            Try
                RaiseEvent evConsoleLine(eConsoleItemType.CLOSEDCAPTION, Line, Nothing, Nothing)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with AddL21LineToConsole(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'Logging

#End Region 'Closed Captions

#Region "Player Defaults"

        Private Users_NavigatorSetup As sNavigatorSetup

        Private Sub ConsumePlayerDefaults(ByVal PD As sNavigatorSetup)
            Try
                If Not PD.IsInitalized Then
                    'all of the PD values are nothing, we'll use the current values in the player
                    Exit Sub
                Else
                    Users_NavigatorSetup.DEFAULT_MENU_LANGUAGE = PD.DEFAULT_MENU_LANGUAGE
                    Users_NavigatorSetup.DEFAULT_AUDIO_LANGUAGE = PD.DEFAULT_AUDIO_LANGUAGE
                    Users_NavigatorSetup.DEFAULT_AUDIO_EXTENSION = PD.DEFAULT_AUDIO_EXTENSION
                    Users_NavigatorSetup.DEFAULT_SUBTITLE_LANGUAGE = PD.DEFAULT_SUBTITLE_LANGUAGE
                    Users_NavigatorSetup.DEFAULT_SUBTITLE_EXTENSION = PD.DEFAULT_SUBTITLE_EXTENSION
                    Users_NavigatorSetup.PARENTAL_LEVEL = PD.PARENTAL_LEVEL
                    Users_NavigatorSetup.PARENTAL_COUNTRY = PD.PARENTAL_COUNTRY
                    Users_NavigatorSetup.ASPECT_RATIO = PD.ASPECT_RATIO
                    Users_NavigatorSetup.PLAYER_REGION = PD.PLAYER_REGION
                    _DefaultAspectRatio = PD.ASPECT_RATIO
                End If
            Catch ex As Exception
                Throw New Exception("Problem with ConsumePlayerDefaults(). Error: " & ex.Message)
            End Try
        End Sub

#End Region 'Player Defaults

#Region "Text Data"

        Private Function GetDVDText(Optional ByVal Index As String = "1") As String
            Debug.WriteLine("GetDVDText()")
            Try
                If PlayState <> ePlayState.Playing Or Graph.DVDCtrl Is Nothing Then Return ""
                Dim BufferSize As Integer
                Dim b(2047) As Byte
                Dim handle As GCHandle = GCHandle.Alloc(b, GCHandleType.Pinned)
                Dim ptrBuffer As IntPtr = handle.AddrOfPinnedObject()
                Dim TextType As DVD_TextStringType
                HR = Graph.DVDInfo.GetDVDTextStringAsUnicode(0, Index, ptrBuffer, 2048, BufferSize, TextType)
                b = RemoveExtraBytesFromArray(b, True)
                Dim Out As String = System.Text.Encoding.Unicode.GetString(b)
                If Out.ToString.Length = 0 Then
                    Out = ""
                End If
                handle.Free()
                Return Out
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Error: " & ex.Message, Nothing, Nothing)
                Return ""
            End Try
        End Function

#End Region 'Text Data

#Region "Frame Grabbing"

#Region "MultiGrab"

        Private TempMultiGrabPath As String
        Private MGFCount As Short
        Private MGFCountTarget As Short
        Private MGFPath As String

        Private Delegate Sub PassResync(ByVal BackupFiveSecs As Boolean)
        Private m_PassResync As PassResync

        Private MultiFrameImageFormat As System.Drawing.Imaging.ImageFormat
        Private Sub ProcessMultiFrames()
            Try
                Dim OutFileID As Short = 1
                'CN.lvConsole.BeginUpdate()
                For i As Short = MGFCount To MGFCount - MGFCountTarget + 1 Step -1
                    ProcessMultiGrabImage(TempMultiGrabPath & "\" & i & ".bin", MGFPath & "\" & OutFileID & "." & MultiFrameImageFormat.ToString, MultiFrameImageFormat)
                    OutFileID += 1
NextBitmap:
                Next
                'CN.lvConsole.EndUpdate()
                Directory.Delete(Me.TempMultiGrabPath, True)
                m_PassResync = AddressOf ReSyncAudio
                Dim args() As Object = {False}
                ParentForm.WindowsForm.Invoke(m_PassResync, args)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with ProcessMultiFrames. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Function ProcessMultiGrabImage(ByVal InPath As String, ByVal OutPath As String, ByVal OutFormat As Drawing.Imaging.ImageFormat) As Boolean
            Try
                Dim FS As New FileStream(InPath, FileMode.Open)
                Dim Buffer(FS.Length - 1) As Byte
                FS.Read(Buffer, 0, FS.Length)
                FS.Close()
                File.Delete(InPath)
                If File.Exists(InPath) Then
                    File.Delete(InPath)
                End If

                Dim OutputFileSize As Integer = Buffer.Length + 54

                'Flip Image
                Buffer = FlipImageBuffer_Vertically(Buffer)
                Buffer = FlipRGB24ImageBuffer_Horizontally(Buffer, Me.CurrentVideoDimensions.X, Me.CurrentVideoDimensions.Y)

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

                BMI(10) = 54 'Header size
                BMI(14) = 40 'InfoHeader size

                'Width
                TmpBuff = ConvertDecimalIntoByteArray(Me.CurrentVideoDimensions.X, 4)
                BMI(18) = TmpBuff(0)
                BMI(19) = TmpBuff(1)
                BMI(20) = TmpBuff(2)
                BMI(21) = TmpBuff(3)

                'Height
                TmpBuff = ConvertDecimalIntoByteArray(Me.CurrentVideoDimensions.Y, 4)
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

                'DEBUGGING
                '    FS = New FileStream("C:\Temp\test_1.bmp", FileMode.OpenOrCreate)
                '    FS.Write(SampleBitmapBuffer, 0, SampleBitmapBuffer.Length)
                '    FS.Close()
                '    FS = Nothing
                'END DEBUGGING

                'Make the bitmap object
                Dim MS As New MemoryStream(SampleBitmapBuffer.Length)
                MS.Write(SampleBitmapBuffer, 0, SampleBitmapBuffer.Length)
                Buffer = Nothing
                SampleBitmapBuffer = Nothing

                ''DEBUGGING
                'FS = New FileStream(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) & "test.bmp", FileMode.OpenOrCreate)
                'MS.WriteTo(FS)
                'FS.Close()
                'FS = Nothing
                ''END DEBUGGING

                Dim BM As New Bitmap(MS)
                'BM.Save("C:\temp\test.bmp")

                'Scale image as needed
                If Me.CurrentVideoDimensions.Y = 480 And CurrentAspectRatio = eAspectRatio.ar16x9 And Not BM.Width = 640 Then
                    If Me.DefaultAspectRatio = ePreferredAspectRatio.Anamorphic Then
                        BM = ScaleImage(BM, 480, 853)
                    Else
                        BM = ScaleImage(BM, 480, 720)
                    End If
                ElseIf Me.CurrentVideoDimensions.Y = 576 And CurrentAspectRatio = eAspectRatio.ar16x9 And Not BM.Width = 640 Then
                    If Me.DefaultAspectRatio = ePreferredAspectRatio.Anamorphic Then
                        BM = ScaleImage(BM, 576, 853)
                    Else
                        BM = ScaleImage(BM, 576, 720)
                    End If
                End If

                BM.Save(OutPath, OutFormat)

                'DEBUGGING
                'BM.Save(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) & "\test.bmp")

                ''Save the bitmap
                'Dim FS As New FileStream(tOutpath, FileMode.OpenOrCreate)
                'FS.Write(Buffer, 0, Buffer.Length)
                'FS.Close()
                'FS = Nothing
                'END DEBUGGING

                'Clean up
                'BM = Nothing
                MS.Close()
                MS = Nothing
                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with ProcessMultiGrabImage. Error: " & ex.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

#End Region 'MultiGrab

        Private Sub HandleKeystoneForcedFrameGrab(ByVal ptr As IntPtr, ByVal sz As Long)
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

                Dim w As Integer = 640 'needs to be set manually (640 for l21 720 for everything else)

                h = sz / w / 3

                'Flip Image
                buffer = FlipImageBuffer_Vertically(buffer)
                buffer = FlipRGB24ImageBuffer_Horizontally(buffer, w, h)

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
                TmpBuff = ConvertDecimalIntoByteArray(w, 4)
                BMI(18) = TmpBuff(0)
                BMI(19) = TmpBuff(1)
                BMI(20) = TmpBuff(2)
                BMI(21) = TmpBuff(3)

                'Height
                TmpBuff = ConvertDecimalIntoByteArray(h, 4)
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

                fs = New FileStream(Me.DumpDirectory & "Forced_" & DateTime.Now.Ticks & ".bmp", FileMode.OpenOrCreate)
                fs.Write(SampleBitmapBuffer, 0, SampleBitmapBuffer.Length)
                fs.Close()
                fs = Nothing
                Debug.WriteLine("Forced grab saved.")
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with ForceSampleFromKeystone. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Function GetSampleFromKeystone(ByVal FrameGrabSource As eFrameGrabContent) As Byte()
            Try
                Dim SamplePtr As IntPtr
                Dim SampSize, SampW, SampH As Integer

                HR = Graph.KO_IKeystone.GrabSample(FrameGrabSource, SamplePtr, SampSize, SampW, SampH)
                If Math.Abs(HR) = 2147483655 Or Math.Abs(HR) = 2147467260 Then
                    RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "FRAMEGRAB TIMEOUT: No samples received in three seconds for " & FrameGrabSource.ToString & ".", Nothing, Nothing)
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
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with GetSampleFromKeystone. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

        Private Function CurrentTitlePosition_AsString() As String
            Try
                If Not Me.DVDText.ToLower = "null" Then
                    Return Replace(DVDText, " ", "", 1, -1, CompareMethod.Text) & "_TT-" & CurrentTitle & "_CH-" & CurrentChapter & "_TC-" & GetCurrentTimecode_AsString()
                Else
                    Return "TT-" & CurrentTitle & "_CH-" & CurrentChapter & "_TC-" & GetCurrentTimecode_AsString()
                End If
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem getting current title position. error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

        Private Function GetCurrentTimecode_AsString() As String
            Try
                Dim h, m, s, f As Byte
                h = CurrentRunningTime_DVD.bHours
                m = CurrentRunningTime_DVD.bMinutes
                s = CurrentRunningTime_DVD.bSeconds
                f = CurrentRunningTime_DVD.bFrames
                Return MakeTwoDig(h) & "." & MakeTwoDig(m) & "." & MakeTwoDig(s) & "." & MakeTwoDig(f)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with GetCurrentTimecode_AsString(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

#End Region 'Frame Grabbing

#Region "Jacket Pics"

        Private JPPaths() As String

        Private Function GetJacketPics(ByVal DVDDir As String) As Boolean
            Try
                'If Not xAD.aJacketPics Then Exit Function
                DVDDir = Replace(DVDDir, "/", "\", 1, -1, CompareMethod.Text)
                If Not Microsoft.VisualBasic.Right(DVDDir, 1) = "\" Then DVDDir &= "\"
                Dim t() As String = Split(DVDDir, "\", -1, CompareMethod.Text)
                Dim JPPath As String = Replace(DVDDir, t(UBound(t) - 1), "jacket_p", 1, -1, CompareMethod.Text)
                If Not Directory.Exists(JPPath) Then Return True
                _JacketPicturesAvailable = True
                ReDim JPPaths(-1)
                For Each Item As String In Directory.GetFileSystemEntries(JPPath) 'ex: "D:\JACKET_P"
                    ReDim Preserve JPPaths(UBound(JPPaths) + 1)
                    JPPaths(UBound(JPPaths)) = Item
                    'Debug.WriteLine(Item)
                Next
                RaiseEvent evJacketPicturesFound(JPPaths)
                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem getting jacket pics. Try restarting application. Error: " & ex.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

        Private Function GetPreferredJPPath() As String
            Try
                For Each JP As String In JPPaths
                    If InStr(JP.ToLower, "l", CompareMethod.Text) Then
                        Return JP
                    End If
                Next
                For Each jp As String In JPPaths
                    If InStr(jp.ToLower, "m", CompareMethod.Text) Then
                        Return jp
                    End If
                Next
                For Each jp As String In JPPaths
                    If InStr(jp.ToLower, "s", CompareMethod.Text) Then
                        Return jp
                    End If
                Next
                Return ""
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem getting preferred JPPath. Error: " & ex.Message, Nothing, Nothing)
                Return ""
            End Try
        End Function

        Private Function StopDomainImageSelection() As Boolean
            Try
                Debug.WriteLine("StopDomainImageSelection()")
                If Not Disposed Then
                    System.Threading.Thread.Sleep(1000)
                    If JPPaths Is Nothing Then
                        If CurrentVideoDimensions.Y = 480 Then
                            'RenderBMPResource("SMT.Libraries.MediaResources", "SD_StopDomain_NTSC.bmp", 0, 0, True)
                            RenderImageResource("SMT.Libraries.MediaResources", "StopDomain_NTSC.png", 0, 0)
                        Else
                            'RenderBMPResource("SMT.Libraries.MediaResources", "SD_StopDomain_PAL.bmp", 0, 0, False)
                            RenderImageResource("SMT.Libraries.MediaResources", "StopDomain_PAL.png", 0, 0)
                        End If
                    Else
                        Me.ShowJacketPicture(GetPreferredJPPath)
                    End If
                End If
                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with StopDomainImageLogic(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

#End Region 'Jacket Pics

#Region "System Jacket Picture"

        Private WithEvents SystemJacketPictureTimer As System.Windows.Forms.Timer

        Private Sub RenderSystemJacketPicture()
            Try
                If SystemJacketPictureTimer Is Nothing Then
                    SystemJacketPictureTimer = New System.Windows.Forms.Timer
                    SystemJacketPictureTimer.Interval = 500
                    SystemJacketPictureTimer.Start()
                End If
                If Me.CurrentVideoStandard = eVideoStandard.NTSC Then
                    RenderImageResource("SMT.Multimedia.Resources", "Phx_SysJP_NTSC.png", 0, 3)
                    'RenderBMPResource("SMT.Libraries.MediaResources", "Phoenix_SysJP_NTSC.bmp", 0, 0, True)
                Else
                    RenderImageResource("SMT.Multimedia.Resources", "Phx_SysJP_PAL.png", 0, 0)
                    'RenderBMPResource("SMT.Libraries.MediaResources", "Phoenix_SysJP_PAL.bmp", 0, 0, False)
                End If
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with RenderSystemJacketPicture(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub SystemJacketPictureTimer_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles SystemJacketPictureTimer.Tick
            RenderSystemJacketPicture()
        End Sub

#End Region 'System Jacket Picture

#Region "Layerbreak & Non-Seamless Cells"

        Private PreppedNSC As cNonSeamlessCell
        Private PreppedNSCID As Short
        Private WithEvents LayerBreakTimer As System.Windows.Forms.Timer
        Private LayerbreakIsActive As Boolean = False

        'Private Function SetupNonSeamlessCells(ByVal Dir As String) As Boolean
        '    Try
        '        If IsProjectDualLayer(Dir) Then
        '            RM.gbLayerbreak.Enabled = True
        '            Dim IP As New cIFOProcessing
        '            Dim tStr As String
        '            colNSCs = IP.GetNonSeamlessCells(Dir, tStr)



        '            xDB.lblPublisher.Text = tStr
        '            DB.ToolTip.SetToolTip(xDB.lblPublisher, xDB.lblPublisher.Text)

        '            If colNSCs Is Nothing Then
        '                RM.gbLayerbreak.Enabled = False
        '                RM.btnLayerBreak.ContextMenu = Nothing
        '                Exit Function
        '            End If

        '            If colNSCs.CandidateLayerbreaks = 1 Then
        '                Layerbreak = colNSCs.ConfirmedLayerbreak
        '                RM.gbLayerbreak.Enabled = True
        '                RM.btnLayerBreak.ContextMenu = Nothing
        '                'RM.btnLayerBreak.Enabled = True
        '            ElseIf colNSCs.CandidateLayerbreaks = 0 Then
        '                Layerbreak = Nothing
        '                RM.gbLayerbreak.Enabled = False
        '                RM.btnLayerBreak.ContextMenu = Nothing
        '            Else
        '                Layerbreak = Nothing
        '                RM.lblLBTC.Text = "Multi. Candidates"
        '                RM.gbLayerbreak.Enabled = True
        '                RM.btnLayerBreak.ContextMenu = RM.cmLayerbreak
        '                'RM.btnLayerBreak.Enabled = True
        '            End If
        '        Else
        '            Layerbreak = Nothing
        '            RM.gbLayerbreak.Enabled = False
        '        End If
        '        Return True
        '    Catch ex As Exception
        '        RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with SetupNonSeamlessCells. Error: " & ex.Message, Nothing, Nothing)
        '        Return False
        '    End Try
        'End Function

        Private Sub PingLB() 'happens at the start of every chapter
            Try
                If DVDIsInStill Then Exit Sub
                If NonSeamlessCells Is Nothing Then Exit Sub
                Dim tLoc As DvdPlayLocation
                tLoc = New DvdPlayLocation
                HR = Graph.DVDInfo.GetCurrentLocation(tLoc)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'Look for NSCs that occur in this PTT in this GTT
                'do just layerbreak
                If NonSeamlessCellNotification = True Then '101210 - added per request of Disney
                    If Not NonSeamlessCells.ConfirmedLayerbreak Is Nothing AndAlso tLoc.TitleNum = NonSeamlessCells.ConfirmedLayerbreak.GTTn AndAlso tLoc.ChapterNum = NonSeamlessCells.ConfirmedLayerbreak.PTT Then
                        PreppedNSC = NonSeamlessCells.ConfirmedLayerbreak
                        PreppedNSCID = NonSeamlessCells.ConfirmedLayerbreak.ID
                        LayerBreakTimer = New System.Windows.Forms.Timer
                        LayerBreakTimer.Interval = 250
                        AddHandler LayerBreakTimer.Tick, AddressOf LBTimer_Tick
                        LayerBreakTimer.Start()
                    End If
                End If

                'do all nscs
                'For s As Short = 0 To NSCs.Count - 1
                '    If tLoc.TitleNum = NSCs(s).GTTn And tLoc.ChapterNum = NSCs(s).PTT Then
                '        PreppedNSC = NSCs(s)
                '        PreppedNSCID = s
                '        'todo: uncomment this to allow layerbreak simulation
                '        Me.LBTimer.Start()
                '    End If
                'Next
            Catch ex As Exception
                If InStr(ex.Message, "80040277", CompareMethod.Text) Then Exit Sub
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with pinglb. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub LBTimer_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Try
                If LayerbreakIsActive Then
                    Me.LayerBreakTimer.Stop()
                    Me.LayerBreakTimer.Dispose()
                    Me.LayerBreakTimer = Nothing
                    LayerbreakIsActive = False
                    Me.ClearOSD()
                Else

                    ''If PreppedNonSeamlessCell.tcLB.Hours < 0 Then Exit Sub
                    'Dim PreRollTC As New cTimecode
                    'PreRollTC.Hours = 0
                    'PreRollTC.Minutes = 0
                    'PreRollTC.Seconds = 1
                    'PreRollTC.Frames = 10
                    ''1:10 LOOKS PRETTY GOOD
                    'Dim TC As cTimecode = SubtractTimecode(PreppedNSC.tcLB, PreRollTC, CurrentTargetFrameRate)

                    If PreppedNSC IsNot Nothing Then
                        Dim TC As cTimecode = PreppedNSC.tcLB

                        'Debug.WriteLine("Current Time: " & CurrentRunningTime.bHours & ":" & CurrentRunningTime.bMinutes & ":" & CurrentRunningTime.bSeconds & ":" & CurrentRunningTime.bFrames)
                        If CInt(CurrentRunningTime_DVD.bHours) <> CInt(TC.Hours) Then Exit Sub
                        If CInt(CurrentRunningTime_DVD.bMinutes) <> CInt(TC.Minutes) Then Exit Sub

                        If CInt(CurrentRunningTime_DVD.bSeconds) <> CInt(TC.Seconds) - 2 Then Exit Sub
                        'If Math.Abs(CInt(CurrentRunningTime.bSeconds) - CInt(TC.Seconds - 1)) > 1 Then Exit Sub

                        ''If CInt(CurrentRunningTime.bFrames) <> CInt(TC.Frames) Then Exit Sub
                        'Dim FDelt As Short = Math.Abs(CInt(CurrentRunningTime.bFrames) - CInt(TC.Frames))
                        'If FDelt > 15 Then
                        '    'Debug.WriteLine("FDelt: " & FDelt)
                        '    Exit Sub
                        'End If
                        ''Debug.WriteLine("Layerbreak target: " & TC.Hours & ":" & TC.Minutes & ":" & TC.Seconds & ":" & TC.Frames)
                        ''Debug.WriteLine("Layerbreak executed at: " & CurrentRunningTime.bHours & ":" & CurrentRunningTime.bMinutes & ":" & CurrentRunningTime.bSeconds & ":" & CurrentRunningTime.bFrames)
                    End If

                    'Threading.Thread.Sleep(NSCPreRoll.Interval)
                    ExecuteNonSeamlessCell()
                End If
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with LBTimer tick. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub ExecuteNonSeamlessCell()
            Try
                'Beep(1000, 1000)
                If LayerBreakTimer Is Nothing Then Exit Sub
                LayerBreakTimer.Stop()
                LayerBreakTimer = New System.Windows.Forms.Timer
                LayerBreakTimer.Interval = 2000
                AddHandler LayerBreakTimer.Tick, AddressOf LBTimer_Tick
                LayerbreakIsActive = True
                LayerBreakTimer.Start()

                'commented in favor of the yellow dot
                'SetOSD(OSD_LayerBreak, RGB(255, 255, 255), 90, True, 1, "ExecuteNonSeamlessCell", Me.TitleSafeLeft, Me.TitleSafeTop + 15)

                'Pause()
                'System.Threading.Thread.Sleep(250)
                'Play()
                RaiseEvent evConsoleLine(eConsoleItemType.NOTICE, "Non-Seamless cell executed at " & Replace(PreppedNSC.LBTC, "/ 30fps", "", 1, -1, CompareMethod.Text), Nothing, Nothing)

                NonSeamlessCells(PreppedNSCID).Executed = True
                PreppedNSC = Nothing
                PreppedNSCID = Nothing
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem executing non-seamless cell. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private Sub BurnInNSCDot()
            Try
                Dim BM As New Bitmap(50, 50, Imaging.PixelFormat.Format24bppRgb)
                Dim G As Graphics = Graphics.FromImage(BM)
                Dim B As Brush = New SolidBrush(Color.White) 'for keying
                G.FillRectangle(B, 0, 0, 50, 50)
                B = New SolidBrush(Color.Yellow)
                G.FillEllipse(B, 0, 0, 50, 50)

                'Dim text_path As New System.Drawing.Drawing2D.GraphicsPath(Drawing2D.FillMode.Alternate)
                'text_path.AddString("Title: " & TTNo, New FontFamily("Arial"), FontStyle.Regular, 20, New Point(2, 2), StringFormat.GenericDefault)
                'G.DrawPath(New Pen(NTSCBlack, 2), text_path)
                'G.FillPath(New SolidBrush(Me.NTSCGreen), text_path)
                'text_path.Dispose()
                G.Dispose()

                SetOSD(BM, RGB(255, 255, 255), 90, True, 1, "BurnInNSCDot", Me.TitleSafeLeft, Me.TitleSafeTop + 15)

            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with BurnInNSCDot(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'Layerbreak

#Region "On-Screen Display"

        'Private OSDIsActive As Boolean = False
        'Private OSDTimer As System.Windows.Forms.Timer

        Public Overrides Sub SetOSD(ByVal BM As System.Drawing.Bitmap, ByVal ColorKey As Integer, ByVal DelayMiliSecs As Short, ByVal UseColorKey As Boolean, ByVal Alpha As Single, ByVal Sender As String, ByVal X As Short, ByVal Y As Short)
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
                'RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "OSD ptr: " & Hex(BufferPtr.ToInt32))

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
                h1.Free()

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
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "problem setting bitmap. sender: " & Sender & " error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private _MediaResourcesAssembly As System.Reflection.Assembly
        Private ReadOnly Property MediaResourcesAssembly() As System.Reflection.Assembly
            Get
                If _MediaResourcesAssembly Is Nothing Then
                    Dim MR As New cMediaResources
                    Return System.Reflection.Assembly.GetAssembly(MR.GetType)
                Else
                    Return _MediaResourcesAssembly
                End If
            End Get
        End Property

#Region "OSD Bitmaps"

        Private Function OSD_TTChange(ByVal TTNo As String) As Bitmap
            Try
                Dim BM As New Bitmap(140, 31, Imaging.PixelFormat.Format24bppRgb)
                Dim G As Graphics = Graphics.FromImage(BM)
                Dim B As Brush = New SolidBrush(Color.White) 'for keying
                G.FillRectangle(B, 0, 0, 140, 31)

                Dim text_path As New System.Drawing.Drawing2D.GraphicsPath(Drawing2D.FillMode.Alternate)
                text_path.AddString("Title: " & TTNo, New FontFamily("Arial"), FontStyle.Regular, 20, New Point(2, 2), StringFormat.GenericDefault)
                G.DrawPath(New Pen(NTSCBlack, 2), text_path)
                G.FillPath(New SolidBrush(Me.NTSCGreen), text_path)
                text_path.Dispose()
                G.Dispose()

                'Dim F As New System.Drawing.Font("Arial", 7)
                'Dim R As New RectangleF(2, 2, 58, 13)
                'G.DrawString("Title: " & TTNo, F, Brushes.Lime, R)

                'BM.Save("c:\temp.bmp")

                Return BM
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with osd_whitehand. Error: " & ex.Message, Nothing, Nothing)
                Return Nothing
            End Try
        End Function

        Private Function OSD_ChChange(ByVal NewChNo As String) As Bitmap
            Try
                Dim BM As New Bitmap(140, 31, Imaging.PixelFormat.Format24bppRgb)
                Dim G As Graphics = Graphics.FromImage(BM)
                Dim B As Brush = New SolidBrush(Color.White) 'for keying
                G.FillRectangle(B, 0, 0, 140, 31)

                Dim text_path As New System.Drawing.Drawing2D.GraphicsPath(Drawing2D.FillMode.Alternate)
                text_path.AddString("CHAPTER: " & NewChNo, New FontFamily("Arial"), FontStyle.Regular, 20, New Point(0, 0), StringFormat.GenericDefault)
                G.DrawPath(New Pen(NTSCBlack, 2), text_path)
                G.FillPath(New SolidBrush(Me.NTSCGreen), text_path)
                text_path.Dispose()
                G.Dispose()

                'Dim F As New System.Drawing.Font("Arial", 7)
                'Dim R As New RectangleF(2, 2, 58, 13)
                'G.DrawString("Chapter: " & NewChNo, F, Brushes.Lime, R)

                'BM.Save("c:\temp.bmp")

                Return BM
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with osd_whitehand. Error: " & ex.Message, Nothing, Nothing)
                Return Nothing
            End Try
        End Function

        Private Function OSDBitmap_FFRWSpeed(ByVal Speed As String, ByVal FF As Boolean) As Bitmap
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
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with osd_whitehand. Error: " & ex.Message, Nothing, Nothing)
                Return Nothing
            End Try
        End Function

        Private Function OSD_WhiteHand() As Bitmap
            Try
                Dim BM As New Bitmap(30, 30, Imaging.PixelFormat.Format24bppRgb)
                Dim G As Graphics = Graphics.FromImage(BM)
                Dim B As Brush = New SolidBrush(Color.White) 'for keying
                G.FillRectangle(B, 0, 0, 30, 30)

                'text testing
                Dim F As New System.Drawing.Font("Arial", 7)
                Dim R As New RectangleF(2, 2, 28, 28)
                G.DrawString("White Hand", F, Brushes.DarkRed, R)
                'end text testing

                'RR.Save("c:\temp.bmp")

                Return BM
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with osd_whitehand. Error: " & ex.Message, Nothing, Nothing)
                Return Nothing
            End Try
        End Function

        Private Function OSD_RedHand() As Bitmap
            Try
                Dim BM As New Bitmap(30, 30, Imaging.PixelFormat.Format24bppRgb)
                Dim G As Graphics = Graphics.FromImage(BM)
                Dim B As Brush = New SolidBrush(Color.White) 'for keying
                G.FillRectangle(B, 0, 0, 30, 30)

                'text
                Dim F As New System.Drawing.Font("Arial", 7)
                Dim R As New RectangleF(2, 2, 28, 28)
                G.DrawString("Red Hand", F, Brushes.DarkRed, R)

                'BM.Save("c:\temp\temp.bmp", System.Drawing.Imaging.ImageFormat.Bmp)

                Return BM
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with osd_redhand. Error: " & ex.Message, Nothing, Nothing)
                Return Nothing
            End Try
        End Function

        Private Function OSD_GrabScreen() As Bitmap
            Try
                Dim BM As New Bitmap(140, 31, Imaging.PixelFormat.Format24bppRgb)
                Dim G As Graphics = Graphics.FromImage(BM)
                Dim B As Brush = New SolidBrush(Color.White) 'for keying
                G.FillRectangle(B, 0, 0, 140, 31)

                Dim text_path As New System.Drawing.Drawing2D.GraphicsPath(Drawing2D.FillMode.Alternate)
                text_path.AddString("Framegrab", New FontFamily("Arial"), FontStyle.Regular, 20, New Point(2, 2), StringFormat.GenericDefault)
                G.DrawPath(New Pen(NTSCBlack, 2), text_path)
                G.FillPath(New SolidBrush(NTSCGreen), text_path)
                text_path.Dispose()
                G.Dispose()

                'Dim F As New System.Drawing.Font("Arial", 7)
                'Dim R As New RectangleF(2, 2, 58, 13)
                'G.DrawString("Title: " & TTNo, F, Brushes.Lime, R)

                'BM.Save("c:\temp.bmp")

                Return BM
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with osd_whitehand. Error: " & ex.Message, Nothing, Nothing)
                Return Nothing
            End Try
        End Function

        Private Function OSD_LayerBreak() As Bitmap
            Try
                Dim BM As New Bitmap(240, 31, Imaging.PixelFormat.Format24bppRgb)
                Dim G As Graphics = Graphics.FromImage(BM)
                Dim B As Brush = New SolidBrush(Color.White) 'for keying
                G.FillRectangle(B, 0, 0, 240, 31)

                Dim text_path As New System.Drawing.Drawing2D.GraphicsPath(Drawing2D.FillMode.Alternate)
                text_path.AddString("LAYERBREAK PENDING", New FontFamily("Arial"), FontStyle.Regular, 16, New Point(2, 2), StringFormat.GenericDefault)
                G.DrawPath(New Pen(NTSCBlack, 2), text_path)
                G.FillPath(New SolidBrush(Me.NTSCRed), text_path)
                text_path.Dispose()
                G.Dispose()

                'Dim F As New System.Drawing.Font("Arial", 7)
                'Dim R As New RectangleF(2, 2, 58, 13)
                'G.DrawString("Title: " & TTNo, F, Brushes.Lime, R)

                'BM.Save("c:\temp.bmp")

                Return BM
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with osd_whitehand. Error: " & ex.Message, Nothing, Nothing)
                Return Nothing
            End Try
        End Function

#Region "NTSC Safe Colors"

        Private ReadOnly Property NTSCWhite() As Color
            Get
                Return Color.FromArgb(235, 235, 235)
            End Get
        End Property

        Private ReadOnly Property NTSCBlack() As Color
            Get
                Return Color.FromArgb(16, 16, 16)
            End Get
        End Property

        Private ReadOnly Property NTSCGreen() As Color
            Get
                Return Color.FromArgb(16, 235, 16)
            End Get
        End Property

        Private ReadOnly Property NTSCRed() As Color
            Get
                Return Color.FromArgb(235, 16, 16)
            End Get
        End Property

        Private ReadOnly Property NTSCBlue() As Color
            Get
                Return Color.FromArgb(16, 16, 235)
            End Get
        End Property

        Private ReadOnly Property NTSCYellow() As Color
            Get
                Return Color.FromArgb(235, 235, 16)
            End Get
        End Property

        Private ReadOnly Property NTSCPurple() As Color
            Get
                Return Color.FromArgb(235, 16, 235)
            End Get
        End Property

        Private ReadOnly Property NTSCTurquoise() As Color
            Get
                Return Color.FromArgb(16, 235, 235)
            End Get
        End Property

#End Region 'NTSC Safe Colors

#End Region 'OSD Bitmaps

#End Region 'On-Screen Display

#Region "Image Rendering"

        Public Overrides Function RenderBitmap(ByVal BM As Bitmap, ByVal X As Short, ByVal Y As Short) As Boolean
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
                h1.Free()
            Catch ex As Exception
                Throw New Exception("Problem with RenderBitmap(). Error: " & ex.Message)
            End Try
        End Function

        Private Function RenderUYVYResource(ByVal NamespaceName As String, ByVal FileNameWithExtension As String, ByVal X As Short, ByVal Y As Short) As Boolean
            Try
                Dim str As Stream = MediaResourcesAssembly.GetManifestResourceStream(NamespaceName & "." & FileNameWithExtension)
                If str Is Nothing Then
                    Return False
                End If

                Dim stream_reader As New BinaryReader(str)
                Dim bytes(str.Length - 1) As Byte
                bytes = stream_reader.ReadBytes(str.Length)
                stream_reader.Close()
                str.Close()

                Dim h1 As GCHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned)
                Dim BufferPtr As IntPtr = h1.AddrOfPinnedObject

                HR = Graph.KO_IKeystone.ShowUYVYBuffer(BufferPtr.ToInt32, Me.CurrentVideoDimensions.X, Me.CurrentVideoDimensions.Y, X, Y)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                h1.Free()
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with RenderUYVYResource(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

        Private Function RenderBMPResource(ByVal NamespaceName As String, ByVal FileNameWithExtension As String, ByVal X As Short, ByVal Y As Short, ByVal NTSC As Boolean) As Boolean
            Try
                'get RGB24 data from resource
                Dim str As Stream = MediaResourcesAssembly.GetManifestResourceStream(NamespaceName & "." & FileNameWithExtension)
                If str Is Nothing Then
                    Return False
                End If

                Dim H As Integer = 0
                Dim W As Integer = 720
                If NTSC Then
                    H = 486
                Else
                    H = 576
                End If
                Dim RasterSize As Integer = H * W * 3
                Dim bytes(RasterSize - 1) As Byte
                str.Position = 54
                str.Read(bytes, 0, RasterSize)

                Array.Reverse(bytes)

                For s As Short = 0 To H - 1
                    Array.Reverse(bytes, s * 2160, 2160)
                Next

                ' OLD WAY
                ''Dim stream_reader As New BinaryReader(str)
                ''Dim bytes(str.Length - 1) As Byte
                ''bytes = stream_reader.ReadBytes(str.Length)
                ''stream_reader.Close()

                'Dim BM As New Bitmap(str)

                ''debugging
                ''BM.Save("c:\temp\temp.bmp")
                ''debugging

                ''Put RGB24 raster data into a byte array
                'Dim BMData As Imaging.BitmapData = BM.LockBits(New Rectangle(0, 0, BM.Width, BM.Height), Imaging.ImageLockMode.ReadWrite, Imaging.PixelFormat.Format24bppRgb)
                'Dim Bytes(BM.Height * BM.Width * 3 - 1) As Byte
                'Dim s As Integer = 0
                'Dim t As Integer = 0
                'Dim Offset As Short = Math.Abs((BMData.Width * 3) - BMData.Stride)
                'For l As Short = 1 To BMData.Height
                '    For p As Short = 1 To BMData.Width
                '        Bytes(t) = Marshal.ReadByte(BMData.Scan0, s)
                '        Bytes(t + 1) = Marshal.ReadByte(BMData.Scan0, s + 1)
                '        Bytes(t + 2) = Marshal.ReadByte(BMData.Scan0, s + 2)
                '        t += 3
                '        s += 3
                '    Next
                '    s += Offset
                'Next
                'BM.UnlockBits(BMData)
                'BMData = Nothing
                ' OLD WAY

                'Get pointer to the RGB24 raster buffer
                Dim h1 As GCHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned)
                Dim BufferPtr As IntPtr = h1.AddrOfPinnedObject

                'render it
                HR = Graph.KO_IKeystone.ShowBitmap(BufferPtr.ToInt32, W, H, X, Y, eImageFormat.IF_RGB24)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                h1.Free()

                'BM.Dispose()
                'BM = Nothing
                str.Close()
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem setting bitmap resource rendering. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

        Public Function RenderImageResource(ByVal NamespaceName As String, ByVal FileNameWithExtension As String, ByVal X As Short, ByVal Y As Short) As Boolean
            Try
                Dim W As Integer
                Dim H As Integer
                Dim PNGstr As Stream = MediaResourcesAssembly.GetManifestResourceStream(NamespaceName & "." & FileNameWithExtension)
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
                    Array.Reverse(bytes, s * 2160, 2160)
                Next

                'Get pointer to the RGB24 raster buffer
                Dim h1 As GCHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned)
                Dim BufferPtr As IntPtr = h1.AddrOfPinnedObject

                'render it
                HR = Graph.KO_IKeystone.ShowBitmap(BufferPtr.ToInt32, W, H, X, Y, eImageFormat.IF_RGB24)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                h1.Free()
                MS.Close()

            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with RenderImageResource(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

#End Region 'Image Rendering

#Region "ProcAmp"

        Private Sub SetUpProcAmp()
            Try
                Graph.KO_IKeystoneProcAmp.ToggleProcAmp(Me.ProcAmp_Active, Me.ProcAmp_HalfScreen)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with SetUpProcAmp. Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

#End Region 'ProcAmp

#Region "REGION"

        Private Function RegionCompatibilityCheck() As Boolean
            Try
                'check to see if player is set to a region that this disc is enabled for
                Return CurrentDVD.VMGM.Regions(CInt(Region))
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with RegionCompatibilityCheck. Error: " & ex.Message, Nothing, Nothing)
            End Try

        End Function

#End Region 'REGION

#Region "CELL STILL"

        Private WithEvents CellStillTimer As System.Windows.Forms.Timer
        Private CellStillDuration As Integer = 0

        Private Sub CellStillTimerTick(ByVal sender As Object, ByVal e As EventArgs) Handles CellStillTimer.Tick
            If CellStillDuration = 0 Then CellStillTimer.Stop()
            RaiseEvent evCellStillTime(CellStillDuration)
            CellStillDuration -= 1
        End Sub

#End Region 'CELL STILL

#Region "COMMAND SYNCHRONIZATION"

        Private Function GetCommandSyncMutex() As Boolean
            Try
                If Me.CommandSyncTimer IsNot Nothing Then 'another method already has the mutex
                    'Beep(200, 200)
                    Return False
                End If
                Me.CommandSyncTimer = New System.Timers.Timer(CommandSyncTolerance)
                Me.CommandSyncTimer.Start()
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with GetCommandSyncMutex(). Error: " & ex.Message, ex)
            End Try
        End Function

        Private Sub CommandSyncTimer_Callback(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles CommandSyncTimer.Elapsed
            Try
                Me.CommandSyncTimer.Stop()
                Me.CommandSyncTimer.Dispose()
                Me.CommandSyncTimer = Nothing
            Catch ex As Exception
                Throw New Exception("Problem with CommandSyncTimer_Callback(). Error: " & ex.Message, ex)
            End Try
        End Sub

#End Region 'COMMAND SYNCHRONIZATION

#Region "Chapter Back Timer"

        Private Sub PrevChapter_Timer_Tick(ByVal sender As Object, ByVal e As EventArgs)
            Me.PrevChapter_Timer.Stop()
            Me.PrevChapter_Timer.Dispose()
            Me.PrevChapter_Timer = Nothing
        End Sub

#End Region 'Chapter Back Timer

#Region "GPRMs/SPRMs"

        Private Sub GetStartupSPRMs()
            Try
                Dim I(11) As Integer
                HR = Graph.DVDInfo.GetAllSPRMs(I)
                If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                Dim cnt As Byte = 0
                For b As Byte = 0 To 11
                    _CurrentSPRMs_evt(cnt) = I(b) And &HFFFF
                    _CurrentSPRMs_evt(cnt + 1) = (I(b) >> 16) And &HFFFF
                    cnt += 2
                Next
            Catch ex As Exception
                Throw New Exception("Problem with GetStartupSPRMs(). Error: " & ex.Message, ex)
            End Try
        End Sub

#End Region 'GPRMs/SPRMs

        Public Function CheckEx(ByVal Ex As Exception, ByVal Source As String) As Boolean
            Try
                If InStr(Ex.Message, "80040276", CompareMethod.Text) Then
                    RaiseEvent evConsoleLine(eConsoleItemType.WARNING, "Prohibited UOP: " & Source, Nothing, Nothing)
                    Return True
                ElseIf InStr(Ex.Message, "80040277", CompareMethod.Text) Then
                    RaiseEvent evConsoleLine(eConsoleItemType.WARNING, "Invalid UOP: " & Source, Nothing, Nothing)
                    Return True
                End If
                Return False
            Catch ex1 As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with CheckEx(). Error: " & ex1.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

#End Region 'PRIVATE FUNCTIONALITY

#Region "MACRO"

        Public CurrentMacro As Queue
        Public CurrentMacroStartLocation As DvdPlayLocation
        Public CurrentMacroName As String
        Public CurrentMacroPath As String
        Public CurrentMacroSourceFilePath As String

#Region "MACRO:MANAGEMENT"

        Public Function SaveMacro(ByVal OutDirectory As String, ByVal MacroName As String) As Boolean
            Try
                RecordingMacro = False
                Dim TempMacroStorage As cMacroStorageClass = MacroQueueToMacroStorage()
                TempMacroStorage.Name = MacroName
                TempMacroStorage.ProjectPath = Me.DVDDirectory
                TempMacroStorage.StartLocation = CurrentMacroStartLocation
                Dim XS As New XmlSerializer(GetType(cMacroStorageClass))
                Dim TW As New StringWriter
                XS.Serialize(TW, TempMacroStorage)
                XS = Nothing
                Dim Out As String = TW.ToString

                Dim FS As New FileStream(OutDirectory & MacroName & ".pmc", FileMode.OpenOrCreate)
                Dim SW As New StreamWriter(FS)
                SW.Write(Out)
                SW.Close()
                FS.Close()
                FS = Nothing
                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with SaveMacro(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

        Private LoadingMacro As Boolean = False

        Public Function LoadMacro(ByVal MacroPath As String) As Boolean
            Try
                If MacroIsRunning Then
                    MacroIsRunning = False
                    MacroTimer.Stop()
                End If

                Dim TempMacroStorage As cMacroStorageClass = PMCPathToMacro(MacroPath)

                'If TempMacroStorage.ProjectPath <> DVDDirectory Then
                '    If Throw New Exception("The selected macro is not associated with this project. Load anyway?", Throw New ExceptionStyle.YesNo) = Throw New ExceptionResult.No Then
                '        Return False
                '    End If
                'End If

                LoadingMacro = True
                MacroStorageToCurrentMacroQueue(TempMacroStorage)
                LoadingMacro = False
                CurrentMacroStartLocation = TempMacroStorage.StartLocation
                CurrentMacroName = TempMacroStorage.Name
                CurrentMacroPath = TempMacroStorage.ProjectPath
                CurrentMacroSourceFilePath = MacroPath

                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with LoadMacro(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Function

        Private Function MacroQueueToMacroStorage() As cMacroStorageClass
            Try
                Dim out As New cMacroStorageClass
                For Each MI As cMacroItem In CurrentMacro
                    ReDim Preserve out.MacroItems(UBound(out.MacroItems) + 1)
                    out.MacroItems(UBound(out.MacroItems)) = MI
                Next
                Return out
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with MacroQueueToMacroStorage(). Error: " & ex.Message, Nothing, Nothing)
                Return Nothing
            End Try
        End Function

        Private Function MacroStorageToCurrentMacroQueue(ByVal MS As cMacroStorageClass) As Boolean
            Try
                CurrentMacro = Nothing
                For Each MI As cMacroItem In MS.MacroItems
                    AddMacroItem(MI.Command, MI.ExtendedData)
                Next
                Return True
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with MacroStorageToCurrentMacroQueue(). Error: " & ex.Message, Nothing, Nothing)
                Return False
            End Try
        End Function

        Public Function PMCPathToMacro(ByVal PMCPath As String) As cMacroStorageClass
            Try
                Dim FS As New FileStream(PMCPath, FileMode.Open)
                Dim StR As New StreamReader(FS)
                Dim inString As String = StR.ReadToEnd
                StR.Close()
                FS.Close()

                Dim SR As New StringReader(inString)
                Dim XS As New XmlSerializer(GetType(cMacroStorageClass))
                Dim TempMacroStorage As cMacroStorageClass = CType(XS.Deserialize(SR), cMacroStorageClass)
                XS = Nothing
                SR = Nothing
                Return TempMacroStorage
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with PMCPathToMacro(). Error: " & ex.Message, Nothing, Nothing)
                Return Nothing
            End Try
        End Function

#End Region 'MACRO:MANAGEMENT

#Region "MACRO:RECORDING"

        Public RecordingMacro As Boolean = False
        Private MacroStartRecordingTicks As ULong

        Public Sub NewMacro()
            CurrentMacro = Nothing
            RecordingMacro = True
            CurrentMacroStartLocation = Me.CurrentPlayLocation
            MacroStartRecordingTicks = Now.Ticks
        End Sub

        Private Sub AddMacroItem(ByVal nCommand As eMacroCommand, Optional ByVal nExtendedData As Long = 0)
            If MacroIsRunning Then Exit Sub
            If Not RecordingMacro And Not LoadingMacro Then Exit Sub
            If CurrentMacro Is Nothing Then CurrentMacro = New Queue
            CurrentMacro.Enqueue(New cMacroItem(nCommand, nExtendedData, New TimeSpan(Now.Ticks - MacroStartRecordingTicks).Milliseconds))
        End Sub

#End Region 'MACRO:RECORDING

#Region "MACRO:PLAYBACK"

        Public MacroIsRunning As Boolean = False

        Public Sub RunNextMacroItem()
            ExecuteMacroItem(CurrentMacro.Dequeue)
        End Sub

        Public Sub RunMacro_OneX()
            Try
                MacroIsRunning = True
                '1) Check to see if we're at the approximately correct start location.
                'TODO: 
                '2) prep the next item for execution
                MacroItem_NextUpForExecution = CurrentMacro.Dequeue
                '3) start the timer
                MacroTimer = New System.Windows.Forms.Timer
                MacroTimer.Interval = MacroItem_NextUpForExecution.MillisecondsDelta
                MacroTimer.Start()
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with RunMacro_OneX(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Private MacroItem_NextUpForExecution As cMacroItem
        Private WithEvents MacroTimer As System.Windows.Forms.Timer
        Private _StopAutoMacro As Boolean = False

        Public Sub StopAutoMacro()
            _StopAutoMacro = True
        End Sub

        Private Sub MacroTimer_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles MacroTimer.Tick
            MacroTimer.Stop()
            ExecuteMacroItem(MacroItem_NextUpForExecution)
            If _StopAutoMacro Then
                _StopAutoMacro = False
                MacroIsRunning = False
                RaiseEvent evMacroCompleted()
                Exit Sub
            End If
            If CurrentMacro.Count > 0 Then
                MacroItem_NextUpForExecution = CurrentMacro.Dequeue
                MacroTimer.Interval = MacroItem_NextUpForExecution.MillisecondsDelta
                MacroTimer.Start()
            Else
                MacroIsRunning = False
                RaiseEvent evMacroCompleted()
            End If
        End Sub

        Private Sub ExecuteMacroItem(ByVal MI As cMacroItem)
            Try
                Select Case MI.Command
                    Case eMacroCommand.AngleMenu
                        Me.GoToMenu(DvdMenuID.Angle)
                    Case eMacroCommand.AngleSurf
                        Me.CycleAngle()
                    Case eMacroCommand.AudioMenu
                        Me.GoToMenu(DvdMenuID.Audio)
                    Case eMacroCommand.AudioStreamSurf
                        Me.CycleAudio()
                    Case eMacroCommand.ChapterBack
                        Me.PreviousChapter()
                    Case eMacroCommand.ChapterNext
                        Me.NextChapter()
                    Case eMacroCommand.ChapterSearch
                        'TODO
                    Case eMacroCommand.ClosedCaptionsToggle
                        Me.ToggleClosedCaptions()
                    Case eMacroCommand.Down
                        Me.DirectionalBtnHit(DvdRelButton.Lower)
                    Case eMacroCommand.Enter
                        Me.EnterBtn()
                    Case eMacroCommand.FastForward
                        Me.FastForward()
                    Case eMacroCommand.GoUp
                        Me.GoUp()
                    Case eMacroCommand.Left
                        Me.DirectionalBtnHit(DvdRelButton.Left)
                    Case eMacroCommand.Pause
                        Me.Pause()
                    Case eMacroCommand.Play
                        Me.Play()
                    Case eMacroCommand.Resume
                        Me.Resume()
                    Case eMacroCommand.Rewind
                        Me.Rewind()
                    Case eMacroCommand.Right
                        Me.DirectionalBtnHit(DvdRelButton.Right)
                    Case eMacroCommand.RootMenu
                        Me.GoToMenu(DvdMenuID.Root)
                    Case eMacroCommand.SceneMenu
                        Me.GoToMenu(DvdMenuID.Chapter)
                    Case eMacroCommand.SelectAudioStream
                        'TODO
                    Case eMacroCommand.SelectSubtitleStream
                        'TODO
                    Case eMacroCommand.Stop
                        Me.Stop()
                    Case eMacroCommand.SubStreamSurf
                        Me.CycleSubtitles()
                    Case eMacroCommand.SubtitleMenu
                        Me.GoToMenu(DvdMenuID.Subpicture)
                    Case eMacroCommand.SubtitlesToggle
                        'TODO
                        'Me.ToggleSubtitles()
                    Case eMacroCommand.TimeSearch
                        'TODO
                    Case eMacroCommand.TitleMenu
                        Me.GoToMenu(DvdMenuID.Title)
                    Case eMacroCommand.TitleSearch
                        'TODO
                    Case eMacroCommand.Up
                        Me.DirectionalBtnHit(DvdRelButton.Upper)
                End Select
                RaiseEvent evMacroItemExecuted(MI.Command, MI.ExtendedData)
            Catch ex As Exception
                RaiseEvent evConsoleLine(eConsoleItemType.ERROR, "Problem with ExecuteMacroItem(). Error: " & ex.Message, Nothing, Nothing)
            End Try
        End Sub

        Public Sub RestartMacro()
            If MacroIsRunning Then
                Me.StopAutoMacro()
                If LoadMacro(Me.CurrentMacroSourceFilePath) Then
                    RunMacro_OneX()
                End If
            Else
                RunMacro_OneX()
            End If
        End Sub

#End Region 'MACRO:PLAYBACK

#Region "MACRO:DATA TYPES"

        <Serializable()> _
        Public Class cMacroStorageClass

            Public Name As String
            Public MacroItems() As cMacroItem
            Public ProjectPath As String
            Public StartLocation As DvdPlayLocation

            Public Sub New()
                ReDim MacroItems(-1)
            End Sub

        End Class

        Public Class cMacroItem

            Public Command As eMacroCommand
            Public ExtendedData As Long
            Public MillisecondsDelta As ULong

            Public Sub New()
            End Sub

            Public Sub New(ByVal nCommand As eMacroCommand, ByVal nExtendedData As Long, ByVal nMillisecondsDelta As ULong)
                Command = nCommand
                ExtendedData = nExtendedData
                MillisecondsDelta = nMillisecondsDelta
            End Sub

        End Class

        Public Enum eMacroCommand
            Up
            Down
            Right
            Left
            Enter
            Play
            Pause
            [Stop]
            FastForward
            Rewind
            ChapterNext
            ChapterBack
            AudioStreamSurf
            AngleSurf
            SubStreamSurf
            RootMenu
            TitleMenu
            AudioMenu
            SceneMenu
            SubtitleMenu
            AngleMenu
            [Resume]
            GoUp
            SelectAudioStream
            SelectSubtitleStream
            ClosedCaptionsToggle
            SubtitlesToggle
            TimeSearch
            TitleSearch
            ChapterSearch
        End Enum

#End Region 'MACRO:DATA TYPES

#End Region 'MACRO

#Region "NON IMPLEMENTED OVERRIDES"

        Public Overrides Function GetBitmapFromKeystone() As Byte()
            Return MyBase.GetBitmapFromKeystone()
        End Function

        Public Overrides Function GetPosition() As Long

        End Function

        Public Overrides Function GetYUVSampleFromKeystone() As Byte()
            Return MyBase.GetYUVSampleFromKeystone()
        End Function

        Public Overrides Function GrabSingleSample(ByVal TargetDirectory As String, ByVal Format As System.Drawing.Imaging.ImageFormat) As Boolean

        End Function

        Public Overrides Function RestartStream() As Boolean

        End Function

        Public Overrides Function SetPosition(ByVal Pos As Long) As Boolean

        End Function

        Public Overrides Sub StartSampleRecording(ByVal TargetDirectory As String)

        End Sub

        Public Overrides Sub StopSampleRecording()

        End Sub

        Public Overrides Function Timesearch() As Boolean

        End Function

        Public Overrides Sub DerivedPlayerHandleEvent(ByVal code As Integer, ByVal p1 As Integer, ByVal p2 As Integer)
        End Sub

#End Region 'NON IMPLEMENTED OVERRIDES

#Region "UTILITY"

        Public Function Test() As Boolean
            Try

                Dim i As Integer
                Graph.nvAudioAtts.GetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_STATS, nvcommon.ENvidiaAudioDecoderProps_Stats.NVAUDDEC_STATS_AC3_DSURMOD, i)
                Debug.WriteLine("DSURMOD=" & i)
                Graph.nvAudioAtts.GetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_STATS, nvcommon.ENvidiaAudioDecoderProps_Stats.NVAUDDEC_STATS_AC3_DSUREXMOD, i)
                Debug.WriteLine("DSUREXMOD=" & i)
                Graph.nvAudioAtts.GetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_STATS, nvcommon.ENvidiaAudioDecoderProps_Stats.NVAUDDEC_STATS_AC3_LFEON, i)
                Debug.WriteLine("LFEON=" & i)
                Graph.nvAudioAtts.GetLong(nvcommon.EINvidiaAudioDecoderProps.NVAUDDEC_STATS, nvcommon.ENvidiaAudioDecoderProps_Stats.NVAUDDEC_STATS_OUT_CHANNELS, i)
                Debug.WriteLine("OUT_CHANNELS=" & i)


                'HR = Graph.DVDInfo.GetState(DVDState)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)

                'Dim storage As IStorage = Nothing
                'Dim stream As IStream = Nothing

                'Try
                '    HR = NativeMethods.StgCreateDocfile("c:\tomsbkup.bmk", STGM.Create Or STGM.Write Or STGM.ShareExclusive, 0, storage)
                '    Marshal.ThrowExceptionForHR(HR)
                '    HR = storage.CreateStream("PhoenixStopBookmark", STGM.Write Or STGM.Create Or STGM.ShareExclusive, 0, 0, stream)
                '    Marshal.ThrowExceptionForHR(HR)

                '    Dim ipst As IPersistStream = CType(DVDState, IPersistStream)

                '    HR = ipst.Save(stream, True)
                '    Marshal.ThrowExceptionForHR(HR)

                '    HR = storage.Commit(STGC.[Default])
                '    Marshal.ThrowExceptionForHR(HR)
                'Finally
                '    If stream IsNot Nothing Then
                '        Marshal.FinalReleaseComObject(stream)
                '    End If
                '    If storage IsNot Nothing Then
                '        Marshal.FinalReleaseComObject(storage)
                '    End If
                'End Try







                ''Dim o As Object = CreateCOMInstance(Clsid.CLSID_DVDState, Clsid.IID_IDvdState)
                ''DVDState = CType(o, IDvdState)
                ''Dim StPtr As IntPtr = GetPointerFromObject(o)
                ''Debug.WriteLine("pointer=" & StPtr.ToInt32)
                ''Debug.WriteLine("hi")



                ''Dim id As UInt64 = 0

                ''HR = Graph.DVDInfo.GetDiscID(Me.DVDDirectory, id)
                ''Debug.WriteLine("DiscID_1=" & id)

                'HR = Graph.DVDInfo.GetState(DVDState)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                ' ''Dim o As Object = Marshal.GetObjectForIUnknown(DVDState)
                ' ''Marshal.GetUniqueObjectForIUnknown(DVDState)


                ' ''HR = DVDState.GetDiscID(id)
                ' ''If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                ' ''Debug.WriteLine("DiscID_2=" & id)
                ' ''Dim PL As UInt32
                ' ''HR = DVDState.GetParentalLevel(PL)
                ' ''If HR < 0 Then Marshal.ThrowExceptionForHR(HR)
                ' ''Debug.WriteLine("ParentalLevel=" & PL)

                ' ''Dim ipst As IPersistStream = CType(DVDState, IPersistStream)

                ' ''Dim gu As Guid
                ' ''ipst.GetClassID(gu)
                ' ''Debug.WriteLine("guid=" & gu.ToString)
                ' ''Dim StPtr As IntPtr = GetPointerFromObject(DVDState)
                ' ''Debug.WriteLine("pointer=" & StPtr.ToInt32)
                ' ''Debug.WriteLine("hi")


                'Dim ipstM As IPersistMemory
                'Dim iu_ptr As IntPtr = Marshal.GetIUnknownForObject(DVDState)
                'Dim ipm_ptr As IntPtr = Marshal.GetIUnknownForObject(ipstM)
                'HR = Marshal.QueryInterface(iu_ptr, Clsid.IID_IPersistMemory, ipm_ptr)
                'If HR < 0 Then Marshal.ThrowExceptionForHR(HR)



                ''Dim ipstM As IPersistMemory = CType(DVDState, IPersistMemory)

                ''ipstM.InitNew()
                'Dim gu As Guid
                'ipstM.GetClassID(gu)
                'Debug.WriteLine("guid=" & gu.ToString)
                'Dim size2 As UInteger
                'ipstM.GetSizeMax(size2)
                'Debug.WriteLine("size=" & size2)
                'Debug.WriteLine("hi")



                ''Dim str As System.Runtime.InteropServices.ComTypes.IStream
                ''HR = SHCreateStreamOnFile("c:\tomstest.bkm", &H1001, str)
                ''If HR = 0 Then
                ''    'Dim size As Integer = Marshal.SizeOf(DVDState)
                ''    Dim size2 As Long = 0
                ''    ipst.GetSizeMax(size2)
                ''    'For i As Integer = 0 To size - 1
                ''    'Next
                ''    ipst.Save(str, True)
                ''    Debug.WriteLine("done here")
                ''End If

                ''Dim ptrValue As IntPtr = System.Runtime.InteropServices.Marshal.StringToHGlobalAuto("c:\tomstest.bkm")
                ''Dim str As UCOMIStream = Nothing
                ''Dim str As SMT.Common.Interop.COM.Interfaces.IStream
                ''CreateStreamOnHGlobal(ptrValue, True, str)
                ''Dim str As New FileStream("c:\tomstest.bkm", FileMode.OpenOrCreate)
                ''ipst.Save(str, True)
                ''Debug.WriteLine("done here")

                ' ''Dim GTT As cGlobalTT = Me.CurrentGlobalTT
                ' ''Dim VTS As cVTS = Me.CurrentDVD.VTSs(GTT.VTSN)
                ' ''Dim PGC As cPGC = VTS.PGCs(Me.CurrentPGC)
                ' ''Dim PG As cProgramMap = PGC.pro

            Catch ex As Exception
                Debug.WriteLine("Problem with Test(). Error: " & ex.Message)
            End Try
        End Function

        Public Function Test2() As Boolean
            Try

                Dim hr As Integer = 0
                Dim storage As IStorage = Nothing
                Dim stream As System.Runtime.InteropServices.ComTypes.IStream = Nothing

                Try
                    If NativeMethods.StgIsStorageFile("c:\tomsbkup.bmk") <> 0 Then
                        Return False
                    End If

                    hr = NativeMethods.StgOpenStorage("c:\tomsbkup.bmk", Nothing, STGM.Read Or STGM.ShareExclusive, IntPtr.Zero, 0, storage)

                    Marshal.ThrowExceptionForHR(hr)

                    hr = storage.OpenStream("PhoenixStopBookmark", IntPtr.Zero, STGM.Read Or STGM.ShareExclusive, 0, stream)

                    If hr >= 0 Then
                        hr = TryCast(DVDState, IPersistStream).Load(stream)
                        Marshal.ThrowExceptionForHR(hr)
                    End If
                Finally
                    If stream IsNot Nothing Then
                        Marshal.FinalReleaseComObject(stream)
                    End If
                    If storage IsNot Nothing Then
                        Marshal.FinalReleaseComObject(storage)
                    End If
                End Try

                If Graph.MediaControlState <> eMediaControlState.Running Then
                    Graph.MediaCtrl.Run()
                End If

                hr = Graph.DVDCtrl.SetState(DVDState, DvdCmdFlags.Block Or DvdCmdFlags.Flush, cmdOption)
                If hr < 0 Then Marshal.ThrowExceptionForHR(hr)

            Catch ex As Exception
                Debug.WriteLine("Problem with Test2(). Error: " & ex.Message)
            End Try
        End Function

#End Region 'UTILITY

    End Class 'cDVDPlayer

End Namespace 'Multimedia.Players.DVD
