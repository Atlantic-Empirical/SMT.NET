Imports System.Runtime.InteropServices

Namespace Multimedia.DirectShow

    <Flags()> _
    Public Enum DvdGraphFlags
        [Default] = 0
        HwDecPrefer = 1
        HwDecOnly = 2
        SwDecPrefer = 4
        SwDecOnly = 8
        NoVpe = 256
        VMR9 = 2048
    End Enum

    <Flags()> _
    Public Enum DvdStreamFlags
        None = 0
        Video = 1
        Audio = 2
        SubPic = 4
    End Enum

    <StructLayout(LayoutKind.Sequential, Pack:=1), ComVisible(False)> _
    Public Structure DvdRenderStatus
        Public vpeStatus As Integer
        Public volInvalid As Boolean
        Public volUnknown As Boolean
        Public noLine21In As Boolean
        Public noLine21Out As Boolean
        Public numStreams As Integer
        Public numStreamsFailed As Integer
        Public failedStreams As DvdStreamFlags
    End Structure

    <ComVisible(True), ComImport(), Guid("FCC152B6-F372-11d0-8E00-00C04FD7C08B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IDvdGraphBuilder

        <PreserveSig()> _
        Function GetFiltergraph(<Out()> ByRef ppGB As IGraphBuilder) As Integer

        <PreserveSig()> _
        Function GetDvdInterface( _
            <[In]()> ByRef riid As Guid, _
            <Out(), MarshalAs(UnmanagedType.IUnknown)> ByRef ppvIF As Object _
        ) As Integer

        <PreserveSig()> _
        Function RenderDvdVideoVolume( _
            <[In](), MarshalAs(UnmanagedType.LPWStr)> ByVal lpcwszPathName As String, _
            ByVal dwFlags As DvdGraphFlags, _
            <Out()> ByRef pStatus As DvdRenderStatus _
        ) As Integer

    End Interface

    <Flags()> _
    Public Enum DvdCmdFlags
        None = 0
        Flush = 1
        SendEvt = 2
        Block = 4
        StartWRendered = 8
        EndARendered = 16
    End Enum

    'DVD_HMSF_TIMECODE
    <StructLayout(LayoutKind.Sequential, Pack:=1), ComVisible(False)> _
    Public Structure DvdTimeCode
        Public bHours As Byte
        Public bMinutes As Byte
        Public bSeconds As Byte
        Public bFrames As Byte
    End Structure

    Public Enum DvdMenuID
        Title = 2
        Root = 3
        Subpicture = 4
        Audio = 5
        Angle = 6
        Chapter = 7
    End Enum

    Public Enum DvdRelButton
        Upper = 1
        Lower = 2
        Left = 3
        Right = 4
    End Enum

    Public Enum DvdOptionFlag_XP
        ResetOnStop = 1
        NotifyParentalLevelChange = 2
        HmsfTimeCodeEvt = 3
        DVD_AudioDuringFFwdRew = 4
        EnableNonblockingAPIs = 5
        DVD_NotifyPositionChange = 8
    End Enum

    Public Enum DvdOptionFlag_Vista
        DVD_ResetOnStop = 1
        DVD_NotifyParentalLevelChange = 2
        DVD_HMSF_TimeCodeEvents = 3
        DVD_AudioDuringFFwdRew = 4
        DVD_EnableNonblockingAPIs = 5
        DVD_CacheSizeInMB = 6
        DVD_EnablePortableBookmarks = 7
        DVD_EnableExtendedCopyProtectErrors = 8
    End Enum

    Public Enum DvdOptionFlag_Win7
        DVD_ResetOnStop = 1
        DVD_NotifyParentalLevelChange = 2
        DVD_HMSF_TimeCodeEvents = 3
        DVD_AudioDuringFFwdRew = 4
        DVD_EnableNonblockingAPIs = 5
        DVD_CacheSizeInMB = 6
        DVD_EnablePortableBookmarks = 7
        DVD_EnableExtendedCopyProtectErrors = 8
        DVD_NotifyPositionChange = 9
        DVD_IncreaseOutputControl = 10
        DVD_EnableStreaming = 11
        DVD_EnableESOutput = 12
        DVD_EnableTitleLength = 13
        DVD_DisableStillThrottle = 14
        DVD_EnableLoggingEvents = 15
        DVD_MaxReadBurstInKB = 16
        DVD_ReadBurstPeriodInMS = 17
    End Enum

    Public Enum DvdAudioLangExt
        NotSpecified = 0
        Captions = 1
        VisuallyImpaired = 2
        DirectorComments1 = 3
        DirectorComments2 = 4
    End Enum

    Public Enum DvdSubPicLangExt
        NotSpecified = 0
        CaptionNormal = 1
        CaptionBig = 2
        CaptionChildren = 3
        ClosedNormal = 5
        ClosedBig = 6
        ClosedChildren = 7
        Forced = 9
        DirectorCmtNormal = 13
        DirectorCmtBig = 14
        DirectorCmtChildren = 15
    End Enum

    <ComVisible(True), ComImport(), Guid("33BC7430-EEC0-11D2-8201-00A0C9D74842"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IDvdControl2

        <PreserveSig()> _
        Function PlayTitle(ByVal ulTitle As Integer, ByVal dwFlags As DvdCmdFlags, <Out()> _
     ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function PlayChapterInTitle(ByVal ulTitle As Integer, ByVal ulChapter As Integer, ByVal dwFlags As DvdCmdFlags, <Out()> _
     ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function PlayAtTimeInTitle(ByVal ulTitle As Integer, <[In]()> _
     ByRef pStartTime As DvdTimeCode, ByVal dwFlags As DvdCmdFlags, <Out()> _
     ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function [Stop]() As Integer

        <PreserveSig()> _
        Function ReturnFromSubmenu(ByVal dwFlags As DvdCmdFlags, <Out()> _
     ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function PlayAtTime(<[In]()> _
     ByRef pTime As DvdTimeCode, ByVal dwFlags As DvdCmdFlags, <Out()> _
     ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function PlayChapter(ByVal ulChapter As Integer, ByVal dwFlags As DvdCmdFlags, <Out()> _
     ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function PlayPrevChapter(ByVal dwFlags As DvdCmdFlags, <Out()> ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function ReplayChapter(ByVal dwFlags As DvdCmdFlags, <Out()> ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function PlayNextChapter(ByVal dwFlags As DvdCmdFlags, <Out()> ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function PlayForwards(ByVal dSpeed As Double, ByVal dwFlags As DvdCmdFlags, <Out()> ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function PlayBackwards(ByVal dSpeed As Double, ByVal dwFlags As DvdCmdFlags, <Out()> ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function ShowMenu(ByVal MenuID As DvdMenuID, ByVal dwFlags As DvdCmdFlags, <Out()> ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function [Resume](ByVal dwFlags As DvdCmdFlags, <Out()> ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function SelectRelativeButton(ByVal buttonDir As DvdRelButton) As Integer

        <PreserveSig()> _
        Function ActivateButton() As Integer

        <PreserveSig()> _
        Function SelectButton(ByVal ulButton As Integer) As Integer

        <PreserveSig()> _
        Function SelectAndActivateButton(ByVal ulButton As Integer) As Integer

        <PreserveSig()> _
        Function StillOff() As Integer

        <PreserveSig()> _
        Function Pause(<[In](), MarshalAs(UnmanagedType.Bool)> _
     ByVal bState As Boolean) As Integer

        <PreserveSig()> _
        Function SelectAudioStream(ByVal ulAudio As Integer, ByVal dwFlags As DvdCmdFlags, <Out()> _
     ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function SelectSubpictureStream(ByVal ulSubPicture As Integer, ByVal dwFlags As DvdCmdFlags, <Out()> _
     ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function SetSubpictureState(<[In](), MarshalAs(UnmanagedType.Bool)> _
     ByVal bState As Boolean, ByVal dwFlags As DvdCmdFlags, <Out()> _
     ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function SelectAngle(ByVal ulAngle As Integer, ByVal dwFlags As DvdCmdFlags, <Out()> _
     ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function SelectParentalLevel(ByVal ulParentalLevel As Long) As Integer

        <PreserveSig()> _
        Function SelectParentalCountry(<MarshalAs(UnmanagedType.LPArray)> ByVal bCountry As Byte()) As Integer

        <PreserveSig()> _
        Function SelectKaraokeAudioPresentationMode(ByVal ulMode As Integer) As Integer

        <PreserveSig()> _
        Function SelectVideoModePreference(ByVal ulPreferredDisplayMode As Integer) As Integer

        <PreserveSig()> _
        Function SetDVDDirectory(<[In](), MarshalAs(UnmanagedType.LPWStr)> _
     ByVal pszwPath As String) As Integer

        <PreserveSig()> _
        Function ActivateAtPosition(ByVal point As DsPOINT) As Integer

        <PreserveSig()> _
        Function SelectAtPosition(ByVal point As DsPOINT) As Integer

        <PreserveSig()> _
        Function PlayChaptersAutoStop(ByVal ulTitle As Integer, ByVal ulChapter As Integer, ByVal ulChaptersToPlay As Integer, ByVal dwFlags As DvdCmdFlags, <Out()> _
     ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function AcceptParentalLevelChange(<[In](), MarshalAs(UnmanagedType.Bool)> _
     ByVal bAccept As Boolean) As Integer

        <PreserveSig()> _
        Function SetOption(ByVal flag As DvdOptionFlag_Win7, <[In](), MarshalAs(UnmanagedType.Bool)> _
     ByVal fState As Boolean) As Integer

        <PreserveSig()> _
        Function SetState(ByVal pState As IDvdState, ByVal dwFlags As DvdCmdFlags, <Out()> _
     ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function PlayPeriodInTitleAutoStop(ByVal ulTitle As Integer, <[In]()> _
     ByRef pStartTime As DvdTimeCode, <[In]()> _
     ByRef pEndTime As DvdTimeCode, ByVal dwFlags As DvdCmdFlags, <Out()> _
     ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function SetGPRM(ByVal ulIndex As UInt32, ByVal wValue As UInt16, ByVal dwFlags As DvdCmdFlags, <Out()> ByVal ppCmd As OptIDvdCmd) As Integer

        <PreserveSig()> _
        Function SelectDefaultMenuLanguage(ByVal Language As Integer) As Integer

        <PreserveSig()> _
        Function SelectDefaultAudioLanguage(ByVal Language As Integer, ByVal audioExtension As DvdAudioLangExt) As Integer

        <PreserveSig()> _
        Function SelectDefaultSubpictureLanguage(ByVal Language As Integer, ByVal subpictureExtension As DvdSubPicLangExt) As Integer

    End Interface

    <ComImport(), System.Security.SuppressUnmanagedCodeSecurity(), Guid("A70EFE61-E2A3-11d0-A9BE-00AA0061BE93"), Obsolete("The IDvdControl interface is deprecated. Use IDvdControl2 instead.", False), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IDvdControl

        <PreserveSig()> _
        Function TitlePlay(<[In]()> _
            ByVal ulTitle As Integer) As Integer

        <PreserveSig()> _
        Function ChapterPlay(<[In]()> _
            ByVal ulTitle As Integer, <[In]()> _
            ByVal ulChapter As Integer) As Integer

        <PreserveSig()> _
        Function TimePlay(<[In]()> _
            ByVal ulTitle As Integer, <[In]()> _
            ByVal bcdTime As Integer) As Integer

        <PreserveSig()> _
        Function StopForResume() As Integer

        <PreserveSig()> _
        Function GoUp() As Integer

        <PreserveSig()> _
        Function TimeSearch(<[In]()> _
            ByVal bcdTime As Integer) As Integer

        <PreserveSig()> _
        Function ChapterSearch(<[In]()> _
            ByVal ulChapter As Integer) As Integer

        <PreserveSig()> _
        Function PrevPGSearch() As Integer

        <PreserveSig()> _
        Function TopPGSearch() As Integer

        <PreserveSig()> _
        Function NextPGSearch() As Integer

        <PreserveSig()> _
        Function ForwardScan(<[In]()> _
            ByVal dwSpeed As Double) As Integer

        <PreserveSig()> _
        Function BackwardScan(<[In]()> _
            ByVal dwSpeed As Double) As Integer

        <PreserveSig()> _
        Function MenuCall(<[In]()> _
            ByVal MenuID As DvdMenuID) As Integer

        <PreserveSig()> _
        Function [Resume]() As Integer

        <PreserveSig()> _
        Function UpperButtonSelect() As Integer

        <PreserveSig()> _
        Function LowerButtonSelect() As Integer

        <PreserveSig()> _
        Function LeftButtonSelect() As Integer

        <PreserveSig()> _
        Function RightButtonSelect() As Integer

        <PreserveSig()> _
        Function ButtonActivate() As Integer

        <PreserveSig()> _
        Function ButtonSelectAndActivate(<[In]()> _
            ByVal ulButton As Integer) As Integer

        <PreserveSig()> _
        Function StillOff() As Integer

        <PreserveSig()> _
        Function PauseOn() As Integer

        <PreserveSig()> _
        Function PauseOff() As Integer

        <PreserveSig()> _
        Function MenuLanguageSelect(<[In]()> _
            ByVal Language As Integer) As Integer

        <PreserveSig()> _
        Function AudioStreamChange(<[In]()> _
            ByVal ulAudio As Integer) As Integer

        <PreserveSig()> _
        Function SubpictureStreamChange(<[In]()> _
            ByVal ulSubPicture As Integer, <[In](), MarshalAs(UnmanagedType.Bool)> _
            ByVal bDisplay As Boolean) As Integer

        <PreserveSig()> _
        Function AngleChange(<[In]()> _
            ByVal ulAngle As Integer) As Integer

        <PreserveSig()> _
        Function ParentalLevelSelect(<[In]()> _
            ByVal ulParentalLevel As Integer) As Integer

        <PreserveSig()> _
        Function ParentalCountrySelect(<[In]()> _
            ByVal wCountry As Short) As Integer

        <PreserveSig()> _
        Function KaraokeAudioPresentationModeChange(<[In]()> _
            ByVal ulMode As Integer) As Integer

        <PreserveSig()> _
        Function VideoModePreferrence(<[In]()> _
            ByVal ulPreferredDisplayMode As Integer) As Integer

        <PreserveSig()> _
        Function SetRoot(<[In](), MarshalAs(UnmanagedType.LPWStr)> _
            ByVal pszPath As String) As Integer

        <PreserveSig()> _
        Function MouseActivate(<[In]()> _
            ByVal point As DsPOINT) As Integer

        <PreserveSig()> _
        Function MouseSelect(<[In]()> _
            ByVal point As DsPOINT) As Integer

        <PreserveSig()> _
        Function ChapterPlayAutoStop(<[In]()> _
            ByVal ulTitle As Integer, <[In]()> _
            ByVal ulChapter As Integer, <[In]()> _
            ByVal ulChaptersToPlay As Integer) As Integer

    End Interface

    <ComVisible(True), ComImport(), Guid("5a4a97e4-94ee-4a55-9751-74b5643aa27d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IDvdCmd

        <PreserveSig()> _
        Function WaitForStart() As Integer

        <PreserveSig()> _
        Function WaitForEnd() As Integer

    End Interface

    <ComVisible(True), ComImport(), Guid("86303d6d-1c4a-4087-ab42-f711167048ef"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IDvdState

        <PreserveSig()> _
        Function GetDiscID(<Out()> ByRef pullUniqueID As UInt64) As Integer

        <PreserveSig()> _
        Function GetParentalLevel(<Out()> ByRef pulParentalLevel As UInt32) As Integer

    End Interface

    Public Enum DvdDomain
        FirstPlay = 1
        VideoManagerMenu = 2
        VideoTitleSetMenu = 3
        Title = 4
        [Stop] = 5
        Nav_Not_Initialized = 255
    End Enum

    Public Enum DvdVideoCompress
        Other = 0
        Mpeg1 = 1
        Mpeg2 = 2
    End Enum

    <StructLayout(LayoutKind.Sequential, Pack:=1), ComVisible(False)> _
    Public Structure DvdPlayLocation
        Public TitleNum As Integer
        Public ChapterNum As Integer
        Public timeCode As DvdTimeCode
        Public TimeCodeFlags As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential, Pack:=1), ComVisible(False)> _
    Public Structure DvdTitleAttr
        Public appMode As DvdTitleAppMode
        Public videoAt As DvdVideoAttr
        Public numberOfAudioStreams As Integer
        Public ulNumberOfSubpictureStreams As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential, Pack:=1), ComVisible(False)> _
    Public Structure DVD_MultichannelAudioAttributes
        Private Info As DVD_MUA_MixingInfo()
        Private Coeff As DVD_MUA_Coeff()
    End Structure

    <StructLayout(LayoutKind.Sequential, Pack:=1), ComVisible(False)> _
    Public Structure DVD_MUA_Coeff
        Private log2_alpha As Double
        Private log2_beta As Double
    End Structure

    <StructLayout(LayoutKind.Sequential, Pack:=1), ComVisible(False)> _
    Public Structure DVD_MUA_MixingInfo
        Private fMixTo0 As Boolean
        Private fMixTo1 As Boolean
        Private fMix0InPhase As Boolean
        Private fMix1InPhase As Boolean
        Private dwSpeakerPosition As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential, Pack:=1), ComVisible(False)> _
    Public Structure DvdMenuAttr
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=8)> _
        Public compatibleRegion As Boolean()
        Public videoAt As DvdVideoAttr
        Public audioPresent As Boolean
        Public audioAt As DvdAudioAttr
        Public subPicPresent As Boolean
        Public subPicAt As DvdSubPicAttr
    End Structure

    <StructLayout(LayoutKind.Sequential, Pack:=1), ComVisible(False)> _
    Public Structure DvdVideoAttr
        Public panscanPermitted As Boolean
        Public letterboxPermitted As Boolean
        Public aspectX As Integer
        Public aspectY As Integer
        Public frameRate As Integer
        Public frameHeight As Integer
        Public compression As DvdVideoCompress
        Public line21Field1InGOP As Boolean
        Public line21Field2InGOP As Boolean
        Public sourceResolutionX As Integer
        Public sourceResolutionY As Integer
        Public isSourceLetterboxed As Boolean
        Public isFilmMode As Boolean
    End Structure

    Public Enum DvdAudioAppMode
        None = 0
        Karaoke = 1
        Surround = 2
        Other = 3
    End Enum

    Public Enum DvdAudioFormat
        Ac3 = 0
        Mpeg1 = 1
        Mpeg1Drc = 2
        Mpeg2 = 3
        Mpeg2Drc = 4
        Lpcm = 5
        Dts = 6
        Sdds = 7
        Other = 8
    End Enum

    <StructLayout(LayoutKind.Sequential, Pack:=1), ComVisible(False)> _
    Public Structure DvdAudioAttr
        Public appMode As DvdAudioAppMode
        Public appModeData As Integer
        Public audioFormat As DvdAudioFormat
        Public language As Integer
        Public languageExtension As DvdAudioLangExt
        Public hasMultichannelInfo As Boolean
        Public frequency As Integer
        Public quantization As Byte
        Public numberOfChannels As Byte
        Public dummy As Short
        Public res1 As Integer
        Public res2 As Integer
    End Structure

    Public Enum DvdSubPicType
        NotSpecified = 0
        Language = 1
        Other = 2
    End Enum

    Public Enum DvdSubPicCoding
        RunLength = 0
        Extended = 1
        Other = 2
    End Enum

    <StructLayout(LayoutKind.Sequential, Pack:=1), ComVisible(False)> _
    Public Structure DvdSubPicAttr
        Public type As DvdSubPicType
        Public coding As DvdSubPicCoding
        Public language As Integer
        Public languageExt As DvdSubPicLangExt
    End Structure

    Public Enum DvdTitleAppMode
        NotSpecified = 0
        Karaoke = 1
        Other = 3
    End Enum

    Public Enum DvdDiscSide
        A = 1
        B = 2
    End Enum

    Public Enum DvdCharSet
        [Unicode] = 0
        Iso646 = 1
        Jis = 2
        Iso8859 = 3
        SiftJis = 4
    End Enum

    <Flags()> _
    Public Enum DvdAudioCaps
        Ac3 = 1
        Mpeg2 = 2
        Lpcm = 4
        Dts = 8
        Sdds = 16
    End Enum

    <StructLayout(LayoutKind.Sequential, Pack:=1), ComVisible(False)> _
    Public Structure DvdDecoderCaps
        Public size As Integer
        Public audioCaps As DvdAudioCaps
        Public fwdMaxRateVideo As Double
        Public fwdMaxRateAudio As Double
        Public fwdMaxRateSP As Double
        Public bwdMaxRateVideo As Double
        Public bwdMaxRateAudio As Double
        Public bwdMaxRateSP As Double
        Public res1 As Integer
        Public res2 As Integer
        Public res3 As Integer
        Public res4 As Integer
    End Structure

    <ComVisible(True), ComImport(), Guid("34151510-EEC0-11D2-8201-00A0C9D74842"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IDvdInfo2

        <PreserveSig()> _
        Function GetCurrentDomain(<Out()> ByRef pDomain As DvdDomain) As Integer

        <PreserveSig()> _
        Function GetCurrentLocation(<Out()> ByRef pLocation As DvdPlayLocation) As Integer

        <PreserveSig()> _
        Function GetTotalTitleTime(<Out()> ByRef pTotalTime As DvdTimeCode, ByRef ulTimeCodeFlags As Integer) As Integer

        <PreserveSig()> _
        Function GetCurrentButton(ByRef pulButtonsAvailable As Integer, ByRef pulCurrentButton As Integer) As Integer

        <PreserveSig()> _
        Function GetCurrentAngle(ByRef pulAnglesAvailable As Integer, ByRef pulCurrentAngle As Integer) As Integer

        <PreserveSig()> _
        Function GetCurrentAudio(ByRef pulStreamsAvailable As Integer, ByRef pulCurrentStream As Integer) As Integer

        <PreserveSig()> _
        Function GetCurrentSubpicture(ByRef pulStreamsAvailable As Integer, ByRef pulCurrentStream As Integer, <Out(), MarshalAs(UnmanagedType.Bool)> ByRef pbIsDisabled As Boolean) As Integer

        <PreserveSig()> _
        Function GetCurrentUOPS(ByRef pulUOPs As Integer) As Integer

        <PreserveSig()> _
        Function GetAllSPRMs(<Out(), MarshalAs(UnmanagedType.LPArray)> ByVal pRegisterArray As Integer()) As Integer

        <PreserveSig()> _
        Function GetAllGPRMs(<Out(), MarshalAs(UnmanagedType.LPArray, SizeConst:=16)> ByVal pRegisterArray As Integer()) As Integer

        <PreserveSig()> _
        Function GetAudioLanguage(ByVal ulStream As Integer, ByRef pLanguage As Integer) As Integer

        <PreserveSig()> _
        Function GetSubpictureLanguage(ByVal ulStream As Integer, ByRef pLanguage As Integer) As Integer

        <PreserveSig()> _
        Function GetTitleAttributes(ByVal ulTitle As Integer, <Out()> ByRef pMenu As DvdMenuAttr, <Out()> ByRef pTitle As DvdTitleAttr) As Integer

        <PreserveSig()> _
        Function GetVMGAttributes(<Out()> ByRef pATR As DvdMenuAttr) As Integer

        <PreserveSig()> _
        Function GetCurrentVideoAttributes(<Out()> ByRef pATR As DvdVideoAttr) As Integer

        <PreserveSig()> _
        Function GetAudioAttributes(ByVal ulStream As Integer, <Out()> ByRef pATR As DvdAudioAttr) As Integer

        <PreserveSig()> _
        Function GetKaraokeAttributes(ByVal ulStream As Integer, ByVal pATR As IntPtr) As Integer

        <PreserveSig()> _
        Function GetSubpictureAttributes(ByVal ulStream As Integer, <Out()> ByRef pATR As DvdSubPicAttr) As Integer

        <PreserveSig()> _
        Function GetDVDVolumeInfo(ByRef pulNumOfVolumes As Integer, ByRef pulVolume As Integer, ByRef pSide As DvdDiscSide, ByRef pulNumOfTitles As Integer) As Integer

        <PreserveSig()> _
        Function GetDVDTextNumberOfLanguages(ByRef pulNumOfLangs As Integer) As Integer

        <PreserveSig()> _
        Function GetDVDTextLanguageInfo(ByVal ulLangIndex As Integer, ByRef pulNumOfStrings As Integer, ByRef pLangCode As Integer, ByRef pbCharacterSet As DvdCharSet) As Integer

        <PreserveSig()> _
        Function GetDVDTextStringAsNative(ByVal ulLangIndex As Integer, ByVal ulStringIndex As Integer, ByVal pbBuffer As IntPtr, ByVal ulMaxBufferSize As Integer, ByRef pulActualSize As Integer, ByRef DVD_TextStringType As Integer) As Integer

        <PreserveSig()> _
        Function GetDVDTextStringAsUnicode(ByVal ulLangIndex As Integer, ByVal ulStringIndex As Integer, ByVal pchwBuffer As IntPtr, ByVal ulMaxBufferSize As Integer, ByRef pulActualSize As Integer, ByRef DVD_TextStringType As Integer) As Integer

        <PreserveSig()> _
        Function GetPlayerParentalLevel(ByRef pulParentalLevel As Integer, ByVal pbCountryCode As IntPtr) As Integer

        <PreserveSig()> _
        Function GetNumberOfChapters(ByVal ulTitle As Integer, ByRef pulNumOfChapters As Integer) As Integer

        <PreserveSig()> _
        Function GetTitleParentalLevels(ByVal ulTitle As Integer, ByVal pulParentalLevels As IntPtr) As Integer

        <PreserveSig()> _
        Function GetDVDDirectory(ByVal pszwPath As IntPtr, ByVal ulMaxSize As Integer, ByRef pulActualSize As Integer) As Integer

        <PreserveSig()> _
        Function IsAudioStreamEnabled(ByVal ulStreamNum As Integer, <Out(), MarshalAs(UnmanagedType.Bool)> ByRef pbEnabled As Boolean) As Integer

        <PreserveSig()> _
        Function GetDiscID(<[In](), MarshalAs(UnmanagedType.LPWStr)> ByVal pszwPath As String, ByRef pullDiscID As Long) As Integer

        <PreserveSig()> _
        Function GetState(<Out()> ByRef pStateData As IDvdState) As Integer

        <PreserveSig()> _
        Function GetMenuLanguages(<Out(), MarshalAs(UnmanagedType.LPArray)> ByVal pLanguages As Integer(), ByVal ulMaxLanguages As Integer, ByRef pulActualLanguages As Integer) As Integer

        <PreserveSig()> _
        Function GetButtonAtPosition(ByVal point As DsPOINT, ByRef pulButtonIndex As Integer) As Integer

        <PreserveSig()> _
        Function GetCmdFromEvent(ByVal lParam1 As Integer, <Out()> ByRef pCmdObj As IDvdCmd) As Integer

        <PreserveSig()> _
        Function GetDefaultMenuLanguage(ByRef pLanguage As Integer) As Integer

        <PreserveSig()> _
        Function GetDefaultAudioLanguage(ByRef pLanguage As Integer, ByRef pAudioExtension As DvdAudioLangExt) As Integer

        <PreserveSig()> _
        Function GetDefaultSubpictureLanguage(ByRef pLanguage As Integer, ByRef pSubpictureExtension As DvdSubPicLangExt) As Integer

        <PreserveSig()> _
        Function GetDecoderCaps(ByRef pCaps As DvdDecoderCaps) As Integer

        <PreserveSig()> _
        Function GetButtonRect(ByVal ulButton As Integer, ByRef pRect As DsRECT) As Integer

        <PreserveSig()> _
        Function IsSubpictureStreamEnabled(ByVal ulStreamNum As Integer, <Out(), MarshalAs(UnmanagedType.Bool)> ByRef pbEnabled As Boolean) As Integer

    End Interface

    Public Enum DVD_TextStringType
        DVD_Struct_Volume = 1
        DVD_Struct_Title = 2
        DVD_Struct_ParentalID = 3
        DVD_Struct_PartOfTitle = 4
        DVD_Struct_Cell = 5
        DVD_Stream_Audio = 16
        DVD_Stream_Subpicture = 17
        DVD_Stream_Angle = 18
        DVD_Channel_Audio = 32
        DVD_General_Name = 48
        DVD_General_Comments = 49
        DVD_Title_Series = 56
        DVD_Title_Movie = 57
        DVD_Title_Video = 58
        DVD_Title_Album = 59
        DVD_Title_Song = 60
        DVD_Title_Other = 63
        DVD_Title_Sub_Series = 64
        DVD_Title_Sub_Movie = 65
        DVD_Title_Sub_Video = 66
        DVD_Title_Sub_Album = 67
        DVD_Title_Sub_Song = 68
        DVD_Title_Sub_Other = 71
        DVD_Title_Orig_Series = 72
        DVD_Title_Orig_Movie = 73
        DVD_Title_Orig_Video = 74
        DVD_Title_Orig_Album = 75
        DVD_Title_Orig_Song = 76
        DVD_Title_Orig_Other = 79
        DVD_Other_Scene = 80
        DVD_Other_Cut = 81
        DVD_Other_Take = 82
    End Enum

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Class OptIDvdCmd
        Public dvdCmd As IDvdCmd
    End Class

    Public Enum eNavCmdType
        DVD_NavCmdType_Pre = 1
        DVD_NavCmdType_Post = 2
        DVD_NavCmdType_Cell = 3
        DVD_NavCmdType_Button = 4
    End Enum

End Namespace