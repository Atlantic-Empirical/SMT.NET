Imports System
Imports System.Runtime.InteropServices
Imports REFERENCE_TIME = System.Int64

Namespace Multimedia.DirectShow

    <Flags()> _
    Public Enum AM_PROPERTY_AC3
        AM_PROPERTY_AC3_ERROR_CONCEALMENT = 1
        AM_PROPERTY_AC3_ALTERNATE_AUDIO = 2
        AM_PROPERTY_AC3_DOWNMIX = 3
        AM_PROPERTY_AC3_BIT_STREAM_MODE = 4
        AM_PROPERTY_AC3_DIALOGUE_LEVEL = 5
        AM_PROPERTY_AC3_LANGUAGE_CODE = 6
        AM_PROPERTY_AC3_ROOM_TYPE = 7
    End Enum

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_AC3_ERROR_CONCEALMENT
        Public fRepeatPreviousBlock As Boolean
        Public fErrorInCurrentBlock As Boolean
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_AC3_ALTERNATE_AUDIO
        Public fStereo As Boolean
        Public DualMode As System.UInt32
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_AC3_DOWNMIX
        Public fDownMix As Boolean
        Public fDolbySurround As Boolean
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_AC3_BIT_STREAM_MODE
        Public BitStreamMode As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_AC3_DIALOGUE_LEVEL
        Public DialogueLevel As System.UInt32
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_AC3_ROOM_TYPE
        Public fLargeRoom As Boolean
    End Structure

    <Flags()> _
    Public Enum AM_PROPERTY_DVDSUBPIC
        AM_PROPERTY_DVDSUBPIC_PALETTE = 0
        AM_PROPERTY_DVDSUBPIC_HLI = 1
        AM_PROPERTY_DVDSUBPIC_COMPOSIT_ON = 2
    End Enum

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_DVD_YUV
        Public Reserved As Byte
        Public Y As Byte
        Public U As Byte
        Public V As Byte
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_PROPERTY_SPPAL
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=16)> _
        Public sppal As System.UInt32()
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_COLCON
        Public emph1col_emph2col As Byte
        Public backcol_patcol As Byte
        Public emph1con_emph2con As Byte
        Public backcon_patcon As Byte
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_PROPERTY_SPHLI
        Public HLISS As System.UInt16
        Public Reserved As System.UInt16
        Public StartPTM As System.UInt32
        Public EndPTM As System.UInt32
        Public StartX As System.UInt16
        Public StartY As System.UInt16
        Public StopX As System.UInt16
        Public StopY As System.UInt16
        Public ColCon As AM_COLCON
    End Structure

    <Flags()> _
    Public Enum AM_PROPERTY_DVDCOPYPROT
        AM_PROPERTY_DVDCOPY_CHLG_KEY = 1
        AM_PROPERTY_DVDCOPY_DVD_KEY1 = 2
        AM_PROPERTY_DVDCOPY_DEC_KEY2 = 3
        AM_PROPERTY_DVDCOPY_TITLE_KEY = 4
        AM_PROPERTY_COPY_MACROVISION = 5
        AM_PROPERTY_DVDCOPY_REGION = 6
        AM_PROPERTY_DVDCOPY_SET_COPY_STATE = 7
        AM_PROPERTY_DVDCOPY_DISC_KEY = 128
    End Enum

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_DVDCOPY_CHLGKEY
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=10)> _
        Public ChlgKey As Byte()
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=2)> _
        Public Reserved As Byte()
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_DVDCOPY_BUSKEY
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=5)> _
        Public BusKey As Byte()
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=1)> _
        Public Reserved As Byte()
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_DVDCOPY_DISCKEY
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=2048)> _
        Public DiscKey As Byte()
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_DVDCOPY_TITLEKEY
        Public KeyFlags As System.UInt32
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=2)> _
        Public Reserved1 As System.UInt32()
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=6)> _
        Public TitleKey As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=2)> _
        Public Reserved2 As String
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_COPY_MACROVISION
        Public MACROVISIONLevel As System.UInt32
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_DVDCOPY_SET_COPY_STATE
        Public DVDCopyState As System.UInt32
    End Structure

    <Flags()> _
    Public Enum AM_DVDCOPYSTATE
        AM_DVDCOPYSTATE_INITIALIZE = 0
        AM_DVDCOPYSTATE_INITIALIZE_TITLE = 1
        AM_DVDCOPYSTATE_AUTHENTICATION_NOT_REQUIRED = 2
        AM_DVDCOPYSTATE_AUTHENTICATION_REQUIRED = 3
        AM_DVDCOPYSTATE_DONE = 4
    End Enum

    <Flags()> _
    Public Enum AM_COPY_MACROVISION_LEVEL
        AM_MACROVISION_DISABLED = 0
        AM_MACROVISION_LEVEL1 = 1
        AM_MACROVISION_LEVEL2 = 2
        AM_MACROVISION_LEVEL3 = 3
    End Enum

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure DVD_REGION
        Public CopySystem As Byte
        Public RegionData As Byte
        Public SystemRegion As Byte
        Public Reserved As Byte
    End Structure

    <Flags()> _
    Public Enum AM_MPEG2Level
        AM_MPEG2Level_Low = 1
        AM_MPEG2Level_Main = 2
        AM_MPEG2Level_High1440 = 3
        AM_MPEG2Level_High = 4
    End Enum

    <Flags()> _
    Public Enum AM_MPEG2Profile
        AM_MPEG2Profile_Simple = 1
        AM_MPEG2Profile_Main = 2
        AM_MPEG2Profile_SNRScalable = 3
        AM_MPEG2Profile_SpatiallyScalable = 4
        AM_MPEG2Profile_High = 5
    End Enum

    <Flags()> _
    Public Enum AM_INTERLACE
        AMINTERLACE_IsInterlaced = 1
        AMINTERLACE_1FieldPerSample = 2
        AMINTERLACE_Field1First = 4
        AMINTERLACE_UNUSED = 8
        AMINTERLACE_FieldPatField1Only = 0
        AMINTERLACE_FieldPatField2Only = 16
        AMINTERLACE_FieldPatBothRegular = 32
        AMINTERLACE_FieldPatternMk = 48
        AMINTERLACE_FieldPatBothIrregular = 48
        AMINTERLACE_DisplayModeMk = 192
        AMINTERLACE_DisplayModeBobOnly = 0
        AMINTERLACE_DisplayModeWeaveOnly = 64
        AMINTERLACE_DisplayModeBobOrWeave = 128
    End Enum

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure RECT
        Public left As Integer
        Public top As Integer
        Public right As Integer
        Public bottom As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure BITMAPINFOHEADER
        Public biSize As Integer
        Public biWidth As Integer
        Public biHeight As Integer
        Public biPlanes As Integer
        Public biBitCount As Integer
        Public biCompression As Integer
        Public biSizeImage As Integer
        Public biXPelsPerMeter As Integer
        Public biYPelsPerMeter As Integer
        Public biClrUsed As Integer
        Public biClrImportant As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Class VIDEOINFOHEADER
        Public SrcRect As DsRECT
        Public TagRect As DsRECT
        Public BitRate As Integer
        Public BitErrorRate As Integer
        Public AvgTimePerFrame As Integer
        Public BmiHeader As BITMAPINFOHEADER
    End Class

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure VIDEOINFOHEADER2
        Public rcSource As DsRECT
        Public rcTarget As DsRECT
        Public dwBitRate As UInt32
        Public dwBitErrorRate As UInt32
        Public AvgTimePerFrame As REFERENCE_TIME
        Public dwInterlaceFlags As AM_INTERLACE
        Public dwCopyProtectFlags As UInt32
        Public dwPictAspectRatioX As UInt32
        Public dwPictAspectRatioY As UInt32
        Public dwControlFlags As UInt32
        Public dwReserved2 As UInt32
        Public bmiHeader As BITMAPINFOHEADER
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure WAVEFORMATEX
        Public wFormatTag As WAVE_FORMAT
        Public nChannels As Integer
        Public nSamplesPerSec As Integer
        Public nAvgBytesPerSec As Integer
        Public nBlockAlign As Integer
        Public wBitsPerSample As Integer
        Public cbSize As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure WAVEFORMATEXTENSIBLE
        Public Format As WAVEFORMATEX
        Public wValidBitsPerSample As Integer
        Public wSamplesPerBlock As Integer
        Public wReserved As Integer
        Public dwChannelMask As KSAUDIO_SPEAKER_FORMAT
        Public SubFormat As Guid
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure MPEG2VIDEOINFO
        Public hdr As VIDEOINFOHEADER2
        Public dwStartTimeCode As System.UInt32
        Public cbSequenceHeader As System.UInt32
        Public dwProfile As System.UInt32
        Public dwLevel As System.UInt32
        Public dwFlags As System.UInt32
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=1)> _
        Public dwSequenceHeader As System.UInt32()
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_DvdKaraokeData
        Public dwDownmix As System.UInt32
        Public dwSpeakerAssignment As System.UInt32
    End Structure

    <Flags()> _
    Public Enum AM_PROPERTY_DVDKARAOKE
        AM_PROPERTY_DVDKARAOKE_ENABLE = 0
        AM_PROPERTY_DVDKARAOKE_DATA = 1
    End Enum

    <Flags()> _
    Public Enum AM_PROPERTY_TS_RATE_CHANGE
        AM_RATE_SimpleRateChange = 1
        AM_RATE_ExactRateChange = 2
        AM_RATE_MaxFullDataRate = 3
        AM_RATE_Step = 4
        AM_RATE_UseRateVersion = 5
        AM_RATE_QueryFullFrameRate = 6
        AM_RATE_QueryLtRateSegPTS = 7
        AM_RATE_CorrectTS = 8
    End Enum

    <Flags()> _
    Public Enum AM_PROPERTY_DVD_RATE_CHANGE
        AM_RATE_ChangeRate = 1
        AM_RATE_FullDataRateMax = 2
        AM_RATE_ReverseDecode = 3
        AM_RATE_DecoderPosition = 4
        AM_RATE_DecoderVersion = 5
    End Enum

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_SimpleRateChange
        Public StartTime As REFERENCE_TIME
        Public Rate As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_QueryRate
        Public lMaxForwardFullFrame As Integer
        Public lMaxReverseFullFrame As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_ExactRateChange
        Public OutputZeroTime As REFERENCE_TIME
        Public Rate As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure AM_DVD_ChangeRate
        Public StartInTime As REFERENCE_TIME
        Public StartOutTime As REFERENCE_TIME
        Public Rate As Integer
    End Structure

    <Flags()> _
    Public Enum DVD_PLAY_DIRECTION
        DVD_DIR_FORWARD = 0
        DVD_DIR_BACKWARD = 1
    End Enum

    <Flags()> _
    Public Enum WAVE_FORMAT
        WAVE_FORMAT_UNKNOWN = 0
        WAVE_FORMAT_PCM = 1
        WAVE_FORMAT_ADPCM = 2
        WAVE_FORMAT_IEEE_FLOAT = 3
        WAVE_FORMAT_VSELP = 4
        WAVE_FORMAT_IBM_CVSD = 5
        WAVE_FORMAT_ALAW = 6
        WAVE_FORMAT_MULAW = 7
        WAVE_FORMAT_DTS = 8
        WAVE_FORMAT_DRM = 9
        WAVE_FORMAT_OKI_ADPCM = 16
        WAVE_FORMAT_DVI_ADPCM = 17
        WAVE_FORMAT_MEDIPACE_ADPCM = 18
        WAVE_FORMAT_SIERRA_ADPCM = 19
        WAVE_FORMAT_G723_ADPCM = 20
        WAVE_FORMAT_DIGISTD = 21
        WAVE_FORMAT_DIGIFIX = 22
        WAVE_FORMAT_DIALOGIC_OKI_ADPCM = 23
        WAVE_FORMAT_MEDIAVISION_ADPCM = 24
        WAVE_FORMAT_CU_CODEC = 25
        WAVE_FORMAT_YAMAHA_ADPCM = 32
        WAVE_FORMAT_SONARC = 33
        WAVE_FORMAT_DSPGROUP_TRUESPEECH = 34
        WAVE_FORMAT_ECHOSC1 = 35
        WAVE_FORMAT_AUDIOFILE_AF36 = 36
        WAVE_FORMAT_APTX = 37
        WAVE_FORMAT_AUDIOFILE_AF10 = 38
        WAVE_FORMAT_PROSODY_1612 = 39
        WAVE_FORMAT_LRC = 40
        WAVE_FORMAT_DOLBY_AC2 = 48
        WAVE_FORMAT_GSM610 = 49
        WAVE_FORMAT_MSNAUDIO = 50
        WAVE_FORMAT_ANTEX_ADPCME = 51
        WAVE_FORMAT_CONTROL_RES_VQLPC = 52
        WAVE_FORMAT_DIGIREAL = 53
        WAVE_FORMAT_DIGIADPCM = 54
        WAVE_FORMAT_CONTROL_RES_CR10 = 55
        WAVE_FORMAT_NMS_VBXADPCM = 56
        WAVE_FORMAT_CS_IMAADPCM = 57
        WAVE_FORMAT_ECHOSC3 = 58
        WAVE_FORMAT_ROCKWELL_ADPCM = 59
        WAVE_FORMAT_ROCKWELL_DIGITALK = 60
        WAVE_FORMAT_XEBEC = 61
        WAVE_FORMAT_G721_ADPCM = 64
        WAVE_FORMAT_G728_CELP = 65
        WAVE_FORMAT_MSG723 = 66
        WAVE_FORMAT_MPEG = 80
        WAVE_FORMAT_RT24 = 82
        WAVE_FORMAT_PAC = 83
        WAVE_FORMAT_MPEGLAYER3 = 85
        WAVE_FORMAT_LUCENT_G723 = 89
        WAVE_FORMAT_CIRRUS = 96
        WAVE_FORMAT_ESPCM = 97
        WAVE_FORMAT_VOXWARE = 98
        WAVE_FORMAT_CANOPUS_ATRAC = 99
        WAVE_FORMAT_G726_ADPCM = 100
        WAVE_FORMAT_G722_ADPCM = 101
        WAVE_FORMAT_DSAT_DISPLAY = 103
        WAVE_FORMAT_VOXWARE_BYTE_ALIGNED = 105
        WAVE_FORMAT_VOXWARE_AC8 = 112
        WAVE_FORMAT_VOXWARE_AC10 = 113
        WAVE_FORMAT_VOXWARE_AC16 = 114
        WAVE_FORMAT_VOXWARE_AC20 = 115
        WAVE_FORMAT_VOXWARE_RT24 = 116
        WAVE_FORMAT_VOXWARE_RT29 = 117
        WAVE_FORMAT_VOXWARE_RT29HW = 118
        WAVE_FORMAT_VOXWARE_VR12 = 119
        WAVE_FORMAT_VOXWARE_TQ40 = 121
        WAVE_FORMAT_SOFTSOUND = 128
        WAVE_FORMAT_VOXWARE_TQ60 = 129
        WAVE_FORMAT_MSRT24 = 130
        WAVE_FORMAT_G729A = 131
        WAVE_FORMAT_MVI_MVI2 = 132
        WAVE_FORMAT_DF_G726 = 133
        WAVE_FORMAT_DF_GSM610 = 134
        WAVE_FORMAT_ISIAUDIO = 136
        WAVE_FORMAT_ONLIVE = 137
        WAVE_FORMAT_SBC24 = 145
        WAVE_FORMAT_DOLBY_AC3_SPDIF = 146
        WAVE_FORMAT_MEDIONIC_G723 = 147
        WAVE_FORMAT_PROSODY_8KBPS = 148
        WAVE_FORMAT_ZYXEL_ADPCM = 151
        WAVE_FORMAT_PHILIPS_LPCBB = 152
        WAVE_FORMAT_PACKED = 153
        WAVE_FORMAT_MALDEN_PHONYTALK = 160
        WAVE_FORMAT_RHETOREX_ADPCM = 256
        WAVE_FORMAT_IRAT = 257
        WAVE_FORMAT_VIVO_G723 = 273
        WAVE_FORMAT_VIVO_SIREN = 274
        WAVE_FORMAT_DIGITAL_G723 = 291
        WAVE_FORMAT_SANYO_LD_ADPCM = 293
        WAVE_FORMAT_SIPROLAB_ACEPLNET = 304
        WAVE_FORMAT_SIPROLAB_ACELP4800 = 305
        WAVE_FORMAT_SIPROLAB_ACELP8V3 = 306
        WAVE_FORMAT_SIPROLAB_G729 = 307
        WAVE_FORMAT_SIPROLAB_G729A = 308
        WAVE_FORMAT_SIPROLAB_KELVIN = 309
        WAVE_FORMAT_G726ADPCM = 320
        WAVE_FORMAT_QUALCOMM_PUREVOICE = 336
        WAVE_FORMAT_QUALCOMM_HALFRATE = 337
        WAVE_FORMAT_TUBGSM = 341
        WAVE_FORMAT_MSAUDIO1 = 352
        WAVE_FORMAT_UNISYS_NAP_ADPCM = 368
        WAVE_FORMAT_UNISYS_NAP_ULAW = 369
        WAVE_FORMAT_UNISYS_NAP_ALAW = 370
        WAVE_FORMAT_UNISYS_NAP_16K = 371
        WAVE_FORMAT_CREATIVE_ADPCM = 512
        WAVE_FORMAT_CREATIVE_FTSPEECH8 = 514
        WAVE_FORMAT_CREATIVE_FTSPEECH10 = 515
        WAVE_FORMAT_UHER_ADPCM = 528
        WAVE_FORMAT_QUARTERDECK = 544
        WAVE_FORMAT_ILINK_VC = 560
        WAVE_FORMAT_RAW_SPORT = 576
        WAVE_FORMAT_ESST_AC3 = 577
        WAVE_FORMAT_IPI_HSX = 592
        WAVE_FORMAT_IPI_RPELP = 593
        WAVE_FORMAT_CS2 = 608
        WAVE_FORMAT_SONY_SCX = 624
        WAVE_FORMAT_FM_TOWNS_SND = 768
        WAVE_FORMAT_BTV_DIGITAL = 1024
        WAVE_FORMAT_QDESIGN_MUSIC = 1104
        WAVE_FORMAT_VME_VMPCM = 1664
        WAVE_FORMAT_TPC = 1665
        WAVE_FORMAT_OLIGSM = 4096
        WAVE_FORMAT_OLIADPCM = 4097
        WAVE_FORMAT_OLICELP = 4098
        WAVE_FORMAT_OLISBC = 4099
        WAVE_FORMAT_OLIOPR = 4100
        WAVE_FORMAT_LH_CODEC = 4352
        WAVE_FORMAT_NORRIS = 5120
        WAVE_FORMAT_SOUNDSPACE_MUSICOMPRESS = 5376
        WAVE_FORMAT_DVM = 8192
        WAVE_FORMAT_EXTENSIBLE = 65534
    End Enum

    <Flags()> _
    Public Enum KSAUDIO_SPEAKER_FORMAT
        KSAUDIO_SPEAKER_DIRECTOUT = 0
        KSAUDIO_SPEAKER_MONO = KSAUDIO_SPEAKER_POSITIONS.SPEAKER_FRONT_CENTER
        KSAUDIO_SPEAKER_STEREO = (KSAUDIO_SPEAKER_POSITIONS.SPEAKER_FRONT_LEFT Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_FRONT_RIGHT)
        KSAUDIO_SPEAKER_QUAD = (KSAUDIO_SPEAKER_POSITIONS.SPEAKER_FRONT_LEFT Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_FRONT_RIGHT Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_BACK_LEFT Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_BACK_RIGHT)
        KSAUDIO_SPEAKER_SURROUND = (KSAUDIO_SPEAKER_POSITIONS.SPEAKER_FRONT_LEFT Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_FRONT_RIGHT Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_FRONT_CENTER Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_BACK_CENTER)
        KSAUDIO_SPEAKER_5POINT1 = (KSAUDIO_SPEAKER_POSITIONS.SPEAKER_FRONT_LEFT Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_FRONT_RIGHT Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_FRONT_CENTER Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_LOW_FREQUENCY Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_BACK_LEFT Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_BACK_RIGHT)
        KSAUDIO_SPEAKER_7POINT1 = (KSAUDIO_SPEAKER_POSITIONS.SPEAKER_FRONT_LEFT Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_FRONT_RIGHT Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_FRONT_CENTER Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_LOW_FREQUENCY Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_BACK_LEFT Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_BACK_RIGHT Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_FRONT_LEFT_OF_CENTER Or KSAUDIO_SPEAKER_POSITIONS.SPEAKER_FRONT_RIGHT_OF_CENTER)
    End Enum

    <Flags()> _
    Public Enum KSAUDIO_SPEAKER_POSITIONS
        SPEAKER_FRONT_LEFT = 1
        SPEAKER_FRONT_RIGHT = 2
        SPEAKER_FRONT_CENTER = 4
        SPEAKER_LOW_FREQUENCY = 8
        SPEAKER_BACK_LEFT = 16
        SPEAKER_BACK_RIGHT = 32
        SPEAKER_FRONT_LEFT_OF_CENTER = 64
        SPEAKER_FRONT_RIGHT_OF_CENTER = 128
        SPEAKER_BACK_CENTER = 256
        SPEAKER_SIDE_LEFT = 512
        SPEAKER_SIDE_RIGHT = 1024
        SPEAKER_TOP_CENTER = 2048
        SPEAKER_TOP_FRONT_LEFT = 4096
        SPEAKER_TOP_FRONT_CENTER = 8192
        SPEAKER_TOP_FRONT_RIGHT = 16384
        SPEAKER_TOP_BACK_LEFT = 32768
        SPEAKER_TOP_BACK_CENTER = 65536
        SPEAKER_TOP_BACK_RIGHT = 131072
    End Enum

    <ComVisible(False)> _
    Public Class KSDATAFORMAT_GUIDs
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_WAVEFORMATEX As Guid = New Guid("00000000-0000-0010-8000-00aa00389b71")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_ANALOG As Guid = New Guid("6dba3190-67bd-11cf-a0f7-0020afd156e4")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_PCM As Guid = New Guid("00000001-0000-0010-8000-00aa00389b71")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_DRM As Guid = New Guid("00000009-0000-0010-8000-00aa00389b71")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_ADPCM As Guid = New Guid("00000002-0000-0010-8000-00aa00389b71")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_MPEG As Guid = New Guid("00000050-0000-0010-8000-00aa00389b71")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_STANDARD_MPEG1_VIDEO As Guid = New Guid("36523B21-8EE5-11d1-8CA3-0060B057664A")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_STANDARD_MPEG1_AUDIO As Guid = New Guid("36523B22-8EE5-11d1-8CA3-0060B057664A")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_STANDARD_MPEG2_VIDEO As Guid = New Guid("36523B23-8EE5-11d1-8CA3-0060B057664A")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_STANDARD_MPEG2_AUDIO As Guid = New Guid("36523B24-8EE5-11d1-8CA3-0060B057664A")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_STANDARD_AC3_AUDIO As Guid = New Guid("36523B25-8EE5-11d1-8CA3-0060B057664A")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_MPEG1Packet As Guid = New Guid("e436eb80-524f-11ce-9F53-0020af0ba770")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_MPEG1Payload As Guid = New Guid("e436eb81-524f-11ce-9F53-0020af0ba770")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_MPEG1Video As Guid = New Guid("e436eb86-524f-11ce-9f53-0020af0ba770")
        Public Shared ReadOnly KSDATAFORMAT_TYPE_MPEG2_PES As Guid = New Guid("e06d8020-db46-11cf-b4d1-00805f6cbbea")
        Public Shared ReadOnly KSDATAFORMAT_TYPE_MPEG2_PROGRAM As Guid = New Guid("e06d8022-db46-11cf-b4d1-00805f6cbbea")
        Public Shared ReadOnly KSDATAFORMAT_TYPE_MPEG2_TRANSPORT As Guid = New Guid("e06d8023-db46-11cf-b4d1-00805f6cbbea")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_MPEG2_VIDEO As Guid = New Guid("e06d8026-db46-11cf-b4d1-00805f6cbbea")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_MPEG2_AUDIO As Guid = New Guid("e06d802b-db46-11cf-b4d1-00805f6cbbea")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_LPCM_AUDIO As Guid = New Guid("e06d8032-db46-11cf-b4d1-00805f6cbbea")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_AC3_AUDIO As Guid = New Guid("e06d802c-db46-11cf-b4d1-00805f6cbbea")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_DTS_AUDIO As Guid = New Guid("e06d8033-db46-11cf-b4d1-00805f6cbbea")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_SDDS_AUDIO As Guid = New Guid("e06d8034-db46-11cf-b4d1-00805f6cbbea")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_SUBPICTURE As Guid = New Guid("e06d802d-db46-11cf-b4d1-00805f6cbbea")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_CC As Guid = New Guid("33214CC1-011F-11D2-B4B1-00A0D102CFBE")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_OVERLAY As Guid = New Guid("e436eb7f-524f-11ce-9f53-0020af0ba770")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_Line21_BytePair As Guid = New Guid("6e8d4a22-310c-11d0-b79a-00aa003767a7")
        Public Shared ReadOnly KSDATAFORMAT_SUBTYPE_Line21_GOPPacket As Guid = New Guid("6e8d4a23-310c-11d0-b79a-00aa003767a7")
    End Class

    <ComVisible(False)> _
    Public Structure AM_SAMPLE2_PROPERTIES
        Public cbData As Integer
        Public dwTypeSpecificFlags As AM_VIDEO_FLAGS
        Public dwSampleFlags As AM_SAMPLE_PROPERTY_FLAGS
        Public lActual As Integer
        Public tStart As Integer
        Public tStop As Integer
        Public dwStreamId As AM_SAMPLE_TYPE
        Public pMediaType As AM_MEDIA_TYPE
        Public pbBuffer As Integer
        Public cbBuffer As Integer
    End Structure

    <Flags()> _
    Public Enum AM_SAMPLE_PROPERTY_FLAGS
        AM_SAMPLE_SPLICEPOINT = 1
        AM_SAMPLE_PREROLL = 2
        AM_SAMPLE_DATADISCONTINUITY = 4
        AM_SAMPLE_TYPECHANGED = 8
        AM_SAMPLE_TIMEVALID = 16
        AM_SAMPLE_TIMEDISCONTINUITY = 64
        AM_SAMPLE_FLUSH_ON_PAUSE = 128
        AM_SAMPLE_STOPVALID = 256
        AM_SAMPLE_ENDOFSTREAM = 512
        AM_STREAM_MEDIA = 0
        AM_STREAM_CONTROL = 1
    End Enum

    <Flags()> _
    Public Enum AM_VIDEO_FLAGS
        AM_VIDEO_FLAG_FIELD_MK = 3
        AM_VIDEO_FLAG_INTERLEAVED_FRAME = 0
        AM_VIDEO_FLAG_FIELD1 = 1
        AM_VIDEO_FLAG_FIELD2 = 2
        AM_VIDEO_FLAG_FIELD1FIRST = 4
        AM_VIDEO_FLAG_WEAVE = 8
        AM_VIDEO_FLAG_IPB_MK = 48
        AM_VIDEO_FLAG_I_SAMPLE = 0
        AM_VIDEO_FLAG_P_SAMPLE = 16
        AM_VIDEO_FLAG_B_SAMPLE = 32
        AM_VIDEO_FLAG_REPEAT_FIELD = 64
    End Enum

    <Flags()> _
    Public Enum AM_SAMPLE_TYPE
        AM_STREAM_MEDIA = 0
        AM_STREAM_CONTROL = 1
    End Enum

End Namespace