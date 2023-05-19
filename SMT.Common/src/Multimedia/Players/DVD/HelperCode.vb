Imports SMT.Multimedia.Players.DVD.Enums
Imports SMT.Multimedia.Utility.Timecode
Imports SMT.Multimedia.DirectShow
Imports System.Drawing
Imports System.Xml.Serialization
Imports SMT.DotNet.Reflection
Imports SMT.Multimedia.Enums

Namespace Multimedia.Players.DVD

    Namespace Structures

        Public Structure sParentalLevel
            Public ParentalLevel As eParentalLevels
            Public ParentalCountry As eCountries
        End Structure

        Public Structure sTransferErrorTime
            Public SourceTime As cTimecode
            Public RunningTime As DvdPlayLocation
        End Structure

        Public Structure sNavigatorSetup
            Public IsInitalized As Boolean
            Public DEFAULT_AUDIO_LANGUAGE As eLanguages
            Public DEFAULT_AUDIO_EXTENSION As eAudioExtensions
            Public DEFAULT_SUBTITLE_LANGUAGE As eLanguages
            Public DEFAULT_SUBTITLE_EXTENSION As eSubExtensions
            Public DEFAULT_MENU_LANGUAGE As eLanguages
            Public PLAYER_REGION As eRegions
            Public PARENTAL_LEVEL As eParentalLevels
            Public PARENTAL_COUNTRY As eCountries
            Public ASPECT_RATIO As ePreferredAspectRatio
        End Structure

        Public Structure sVolumeInfo
            Public VolumeCount As Integer
            Public CurrentVolume As Integer
            Public DiscSide As DvdDiscSide
            Public GlobalTitleCount As Integer
        End Structure

    End Namespace 'Structures

    Namespace Classes

#Region "Audio"

        Public Class seqAudioStreams

            Public AudioStreams(-1) As seqAudio

            Public Sub AddAudio(ByVal S As seqAudio)
                ReDim Preserve AudioStreams(UBound(AudioStreams) + 1)
                S.Format = S.Format.ToUpper
                AudioStreams(UBound(AudioStreams)) = S
            End Sub

            Public Sub ClearAudios()
                ReDim Me.AudioStreams(-1)
            End Sub

        End Class

        Public Class seqAudio

            Public _Language As String
            Public _Extension As String
            Public _Format As String
            Public _AppMode As String
            Public _Frequency As String
            Public _Quantization As String
            Public _NumberOfChannels As String
            Public _StreamNumber As String
            Public _AppModeData As Short
            Public _HasMultiChannelInfo As Boolean
            Public _Available As Boolean

            Public Property HasMultiChannelInfo() As Boolean
                Get
                    Return _HasMultiChannelInfo
                End Get
                Set(ByVal Value As Boolean)
                    _HasMultiChannelInfo = Value
                End Set
            End Property

            Public Property AppModeData() As String
                Get
                    Return _AppModeData
                End Get
                Set(ByVal Value As String)
                    _AppModeData = Value
                End Set
            End Property

            Public Property StreamNumber() As String
                Get
                    Return _StreamNumber
                End Get
                Set(ByVal Value As String)
                    _StreamNumber = Value
                End Set
            End Property

            Public Property NumberOfChannels() As String
                Get
                    Return _NumberOfChannels
                End Get
                Set(ByVal Value As String)
                    _NumberOfChannels = Value
                End Set
            End Property

            Public Property Quantization() As String
                Get
                    Return _Quantization
                End Get
                Set(ByVal Value As String)
                    _Quantization = Value
                End Set
            End Property

            Public Property Frequency() As String
                Get
                    Return _Frequency
                End Get
                Set(ByVal Value As String)
                    _Frequency = Value
                End Set
            End Property

            Public Property AppMode() As String
                Get
                    Return _AppMode
                End Get
                Set(ByVal Value As String)
                    _AppMode = Value
                End Set
            End Property

            Public Property Format() As String
                Get
                    Return _Format
                End Get
                Set(ByVal Value As String)
                    _Format = Value
                End Set
            End Property

            Public Property Extension() As String
                Get
                    Return _Extension
                End Get
                Set(ByVal Value As String)
                    _Extension = Value
                End Set
            End Property

            Public Property Language() As String
                Get
                    Return _Language
                End Get
                Set(ByVal Value As String)
                    _Language = Value
                End Set
            End Property

            Public ReadOnly Property Available() As Boolean
                Get
                    Return _Available
                End Get
            End Property

            Public Overloads Overrides Function ToString() As String
                Return StreamNumber & " - " & Language & " - " & Extension & " - " & Format()
            End Function

        End Class

#End Region 'Audio

#Region "Subtitles"

        Public Class seqSubs
            Public Sub New()
            End Sub

            Public SubtitleStreams(-1) As seqSub

            Public Sub AddSub(ByVal S As seqSub)
                ReDim Preserve SubtitleStreams(UBound(SubtitleStreams) + 1)
                SubtitleStreams(UBound(SubtitleStreams)) = S
            End Sub

            Public Sub ClearSubs()
                ReDim Me.SubtitleStreams(-1)
            End Sub

        End Class

        Public Class seqSub
            Private _Language As String
            Private _Extension As String
            Private _Coding As String
            Private _Type As String
            Private _StreamNumber As String
            Private _Enabled As Boolean

            Public Property StreamNumber() As String
                Get
                    Return _StreamNumber
                End Get
                Set(ByVal Value As String)
                    _StreamNumber = Value
                End Set
            End Property

            Public Property Type() As String
                Get
                    Return _Type
                End Get
                Set(ByVal Value As String)
                    _Type = Value
                End Set
            End Property

            Public Property Coding() As String
                Get
                    Return _Coding
                End Get
                Set(ByVal Value As String)
                    _Coding = Value
                End Set
            End Property

            Public Property Extension() As String
                Get
                    Return _Extension
                End Get
                Set(ByVal Value As String)
                    _Extension = Value
                End Set
            End Property

            Public Property Language() As String
                Get
                    Return _Language
                End Get
                Set(ByVal Value As String)
                    _Language = Value
                End Set
            End Property

            Public Property Enabled() As Boolean
                Get
                    Return _Enabled
                End Get
                Set(ByVal Value As Boolean)
                    _Enabled = Value
                End Set
            End Property

            Public Overloads Overrides Function ToString() As String
                Return StreamNumber & " - " & Language & " - " & Extension & " - " & Type
            End Function

        End Class

#End Region 'Subtitles

        <Serializable()> _
        Public Class cNSCPreRoll

            Public Sub New()
            End Sub

            Public Seconds As Byte
            Public Frames As Byte
            Public Interval As Short
        End Class

        Public Class cTitleDuration
            Public TotalTime As DvdTimeCode
            Public FrameRate As String
            Public DropFrame As Boolean
            Public Interpolated As Boolean
        End Class

        Public Class cTitleAttributes
            Public MA As DvdMenuAttr
            Public TA As DvdTitleAttr
        End Class

        Public Class cJacketPicture
            Public Path As String
            Public Sub New(ByVal nPath As String)
                Path = nPath
            End Sub
            Public Overloads Overrides Function tostring() As String
                If InStr(Path.ToLower, "5l", CompareMethod.Text) Then
                    Return "NTSC Large"
                ElseIf InStr(Path.ToLower, "5m", CompareMethod.Text) Then
                    Return "NTSC Medium"
                ElseIf InStr(Path.ToLower, "5s", CompareMethod.Text) Then
                    Return "NTSC Small"
                ElseIf InStr(Path.ToLower, "6l", CompareMethod.Text) Then
                    Return "PAL Large"
                ElseIf InStr(Path.ToLower, "6m", CompareMethod.Text) Then
                    Return "PAL Medium"
                ElseIf InStr(Path.ToLower, "6s", CompareMethod.Text) Then
                    Return "PAL Small"
                End If
            End Function
        End Class

        Public Class cGuidePlacementInfo
            Public L, T, R, B As Short
            Public X, Y As Short
            Public GuideColor As Color
            Public GuidesEnabled As Boolean = False
            Public PlacementEnabled As Boolean = False

            Public Sub New()
                L = 0
                T = 0
                R = 0
                B = 0
                GuideColor = Color.FromArgb(255, 235, 235, 235)
                GuidesEnabled = False
                PlacementEnabled = False
                X = 0
                Y = 0
            End Sub

        End Class

        Public Class cUserOperations

            Public _0_Time_Title_Play As Boolean
            Public _1_Chapter_Play As Boolean
            Public _2_Title_Play As Boolean
            Public _3_Stop As Boolean
            Public _4_GoUp As Boolean
            Public _5_Time_Chapter_Search As Boolean
            Public _6_Chapter_Back As Boolean
            Public _7_Chapter_Next As Boolean
            Public _8_Fast_Forward As Boolean
            Public _9_Rewind As Boolean
            Public _10_Title_Menu As Boolean
            Public _11_Root_Menu As Boolean
            Public _12_Subtitle_Menu As Boolean
            Public _13_Audio_Menu As Boolean
            Public _14_Angle_Menu As Boolean
            Public _15_Chapter_Menu As Boolean
            Public _16_Resume As Boolean
            Public _17_Button_Select_Activate As Boolean
            Public _18_Start_From_Still As Boolean
            Public _19_Pause_Menu_Lang_Select As Boolean
            Public _20_Change_Audio_Stream As Boolean
            Public _21_Change_Subtitle_Stream As Boolean
            Public _22_Angle_Change_Parental_Management_Level_Change As Boolean
            Public _23_Karaoke_Mode_Change As Boolean
            Public _24_Video_Mode_Change As Boolean

            Public Function GetUOPValue(ByVal Number As Byte) As Boolean
                Dim FieldName As FieldNames = Number
                Return CBool(GetFieldValue(Me, FieldName.ToString))
            End Function

            Public Enum FieldNames
                _0_Time_Title_Play
                _1_Chapter_Play
                _2_Title_Play
                _3_Stop
                _4_GoUp
                _5_Time_Chapter_Search
                _6_Chapter_Back
                _7_Chapter_Next
                _8_Fast_Forward
                _9_Rewind
                _10_Title_Menu
                _11_Root_Menu
                _12_Subtitle_Menu
                _13_Audio_Menu
                _14_Angle_Menu
                _15_Chapter_Menu
                _16_Resume
                _17_Button_Select_Activate
                _18_Start_From_Still
                _19_Pause_Menu_Lang_Select
                _20_Change_Audio_Stream
                _21_Change_Subtitle_Stream
                _22_Angle_Change_Parental_Management_Level_Change
                _23_Karaoke_Mode_Change
                _24_Video_Mode_Change
            End Enum

        End Class

        Public Class cDetailedPlayLocation

            'Where to look. Needed for interpretation of the following.
            'Public InVMGM As Boolean = False
            'Public InVTS_MenuSpace As Boolean = False
            'Public InVTS_TitleSpace As Boolean = False
            Public Domain As DvdDomain

            Public LanguageUnit As String = ""

            Public GTTN As Integer = 0
            Public VTSN As Integer = 0
            Public VTS_TTN As Integer = 0
            Public PGCN As Integer = 0
            Public PGN As Integer = 0
            Public Cell As Integer = 0
            Public PTTN As Integer = 0

            Public RunningTime As cTimecode

            Public Sub New()
            End Sub

            Public Sub New(ByVal nVMGM As Boolean, ByVal nVTSM As Boolean, ByVal nVTSTT As Boolean, ByVal nLangUnit As String, ByVal nGTTN As Integer, ByVal nVTSN As Integer, ByVal nVTS_TTN As Integer, ByVal nPGCN As Integer, ByVal nPGN As Integer, ByVal nCell As Integer, ByVal nPTTN As Integer, ByVal nDomain As DvdDomain)
                'Me.InVMGM = nVMGM
                'Me.InVTS_MenuSpace = nVTSM
                'Me.InVTS_TitleSpace = nVTSTT
                Domain = nDomain
                Me.LanguageUnit = nLangUnit
                GTTN = nGTTN
                VTSN = nVTSN
                VTS_TTN = nVTS_TTN
                PGCN = nPGCN
                PGN = nPGN
                Cell = nCell
                PTTN = nPTTN
            End Sub

            ''' <summary>
            ''' Accepts a string created by this classes' ToString_Encoded
            ''' </summary>
            ''' <param name="meString"></param>
            ''' <remarks></remarks>
            Public Sub New(ByVal meString As String)
                Try
                    Dim s() As String = Split(meString, "-")
                    'Me.InVMGM = s(0)
                    'Me.InVTS_MenuSpace = s(1)
                    'Me.InVTS_TitleSpace = s(2)
                    Me.LanguageUnit = s(3)
                    GTTN = s(4)
                    VTSN = s(5)
                    VTS_TTN = s(6)
                    PGCN = s(7)
                    PGN = s(8)
                    Cell = s(9)
                    PTTN = s(10)
                Catch ex As Exception
                    Throw New Exception("Problem with New() cDetailedPlayLocation. Error: " & ex.Message, ex)
                End Try
            End Sub

            Public Overrides Function ToString() As String
                Dim sb As New System.Text.StringBuilder

                sb.Append(DvdDomainToString(Domain) & " | ")

                If Domain = DvdDomain.VideoTitleSetMenu Or Domain = DvdDomain.Title Then sb.Append("VTS=" & VTSN & " ")
                If Domain = DvdDomain.Title Then
                    sb.Append("GTT=" & GTTN & " ")
                    sb.Append("VTS_TT=" & VTS_TTN & " ")
                    sb.Append("PTT=" & PTTN & " ")
                End If

                If Domain = DvdDomain.VideoManagerMenu Or Domain = DvdDomain.VideoTitleSetMenu Then
                    sb.Append("LS=" & LanguageUnit & " ")
                End If

                If Domain = DvdDomain.Title Or Domain = DvdDomain.VideoManagerMenu Or Domain = DvdDomain.VideoTitleSetMenu Then
                    sb.Append("PGC=" & PGCN & " ")
                    sb.Append("PG=" & PGN & " ")
                    sb.Append("Cell=" & Cell & " ")
                End If

                Return sb.ToString
            End Function

            Public ReadOnly Property ToString_Encoded() As String
                Get
                    Return DvdDomainToString(Domain) & "-" & LanguageUnit & "-" & GTTN & "-" & VTSN & "-" & VTS_TTN & "-" & PGCN & "-" & PGN & "-" & Cell & "-" & PTTN
                    'Return InVMGM.ToString & "-" & InVTS_MenuSpace.ToString & "-" & InVTS_TitleSpace.ToString & "-" & LanguageUnit & "-" & GTTN & "-" & VTSN & "-" & VTS_TTN & "-" & PGCN & "-" & PGN & "-" & Cell & "-" & PTTN
                End Get
            End Property

            Private Function DvdDomainToString(ByVal d As DvdDomain) As String
                Select Case d
                    Case DvdDomain.FirstPlay
                        Return "FIRSTPLAY"
                    Case DvdDomain.Stop
                        Return "STOP"
                    Case DvdDomain.Title
                        Return "VTS_T"
                    Case DvdDomain.VideoManagerMenu
                        Return "VMGM"
                    Case DvdDomain.VideoTitleSetMenu
                        Return "VTS_M"
                End Select
            End Function

        End Class

    End Namespace 'Classes

    Namespace Enums

        Public Enum eChannelFilter
            Y_Only = 1
            YUV_MinusU = 6
            YUV_MinusV = 5
        End Enum

        Public Enum eRegViewType
            Dec
            Hex
            ASCII
            Bin
        End Enum

        Public Enum eRegions
            One
            Two
            Three
            Four
            Five
            Six
            Seven
            Eight
        End Enum

        Public Enum eSubExtensions
            Not_Specified
            Caption_Normal
            Caption_Big
            Caption_Children
            Closed_Normal
            Closed_Big
            Closed_Children
            Forced
            Director_Comments_Normal
            Director_Comments_Big
            Director_Comments_Children
        End Enum

        Public Enum eAudioExtensions
            Not_Specified = 0
            Captions = 1
            Visually_Impaired = 2
            Director_Comments_1 = 3
            Director_Comments_2 = 4
        End Enum

        Public Enum ePreferredAspectRatio
            Anamorphic = 1
            Panscan = 2
            Letterbox = 3
        End Enum

        Public Enum eParentalLevels
            PL_Off
            PL_1_G
            PL_2
            PL_3_PG
            PL_4_PG13
            PL_5
            PL_6_R
            PL_7_NC17
            PL_8
        End Enum

        Public Enum eMenuMode
            No
            Buttons
            Still
        End Enum

        Public Enum eNSCsToSimulate
            All
            None
            LBOnly
        End Enum

        Public Enum eUOPs
            Time_Title_Play
            Chapter_Play
            Title_Play
            [Stop]
            GoUp
            Time_Chapter_Search
            Chapter_Back
            Chapter_Next
            Fast_Forward
            Rewind
            Title_Menu
            Root_Menu
            Subtitle_Menu
            Audio_Menu
            Angle_Menu
            Chapter_Menu
            [Resume]
            Button_Select_Activate
            Start_From_Still
            Pause_Menu_Lang_Select
            Change_Audio_Stream
            Change_Subtitle_Stream
            Angle_Change_Parental_Management_Level_Change
            Karaoke_Mode_Change
            Video_Mode_Change
        End Enum

        Public Enum ePlaybackStoppedCauses
            DVD_PB_STOPPED_Other = 0
            DVD_PB_STOPPED_NoBranch = 1
            DVD_PB_STOPPED_NoFirstPlayDomain = 2
            DVD_PB_STOPPED_StopCommand = 3
            DVD_PB_STOPPED_Reset = 4
            DVD_PB_STOPPED_DiscEjected = 5
            DVD_PB_STOPPED_IllegalNavCommand = 6
            DVD_PB_STOPPED_PlayPeriodAutoStop = 7
            DVD_PB_STOPPED_PlayChapterAutoStop = 8
            DVD_PB_STOPPED_ParentalFailure = 9
            DVD_PB_STOPPED_RegionFailure = 10
            DVD_PB_STOPPED_MacrovisionFailure = 11
            DVD_PB_STOPPED_DiscReadError = 12
            DVD_PB_STOPPED_CopyProtectFailure = 13
        End Enum

    End Namespace 'Enums

    Namespace Modules

        Public Module mSharedMethods

            Public Function GetSubtitleExtensionFromString(ByVal ExtString As String) As String
                Select Case ExtString.ToLower
                    Case "not_specified"
                        Return DvdSubPicLangExt.NotSpecified
                    Case "caption_normal"
                        Return DvdSubPicLangExt.CaptionNormal
                    Case "caption_big"
                        Return DvdSubPicLangExt.CaptionBig
                    Case "caption_children"
                        Return DvdSubPicLangExt.CaptionChildren
                    Case "closed_normal"
                        Return DvdSubPicLangExt.ClosedNormal
                    Case "closed_big"
                        Return DvdSubPicLangExt.ClosedBig
                    Case "closed_children"
                        Return DvdSubPicLangExt.ClosedChildren
                    Case "forced"
                        Return DvdSubPicLangExt.Forced
                    Case "director_comments_normal"
                        Return DvdSubPicLangExt.DirectorCmtNormal
                    Case "director_comments_big"
                        Return DvdSubPicLangExt.DirectorCmtBig
                    Case "director_comments_children"
                        Return DvdSubPicLangExt.DirectorCmtChildren
                End Select
            End Function

            ''returns true if TC1 is equal or greater than TC2
            'Public Function CompareDVDTimeCodes(ByVal TC1 As DvdTimeCode, ByVal TC2 As DvdTimeCode) As Boolean
            '    If TC1.bHours > TC2.bHours Then
            '        Return True
            '    Else
            '        Return False
            '    End If
            '    'hours are equal
            '    If TC1.bMinutes > TC2.bMinutes Then
            '        Return True
            '    Else
            '        Return False
            '    End If
            '    'minutes are equal
            '    If TC1.bSeconds > TC2.bMinutes Then
            '        Return True
            '    Else
            '        Return False
            '    End If
            '    'seconds are equal
            '    If TC1.bFrames > TC2.bFrames Then
            '        Return True
            '    Else
            '        Return False
            '    End If
            '    'the timecodes are equal
            '    Return True
            'End Function

        End Module

    End Namespace 'Modules

End Namespace 'Multimedia.Players.DVD
