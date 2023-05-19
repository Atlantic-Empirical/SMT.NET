Imports System.Runtime.InteropServices

Namespace Multimedia.DirectShow

    <ComVisible(False)> _
    Public Class FilterCategory     ' uuids.h  :  CLSID_*
        ' CLSID_AudioInputDeviceCategory, audio capture category
        Public Shared ReadOnly AudioInputDevice As Guid = New Guid("33D9A762-90C8-11D0-BD43-00A0C911CE86")

        ' CLSID_VideoInputDeviceCategory, video capture category
        Public Shared ReadOnly VideoInputDevice As Guid = New Guid(&H860BB310, &H5D01, &H11D0, &HBD, &H3B, &H0, &HA0, &HC9, &H11, &HCE, &H86)

    End Class

    <ComVisible(False)> _
    Public Class Clsid         ' uuids.h  :  CLSID_*
        Public Shared ReadOnly SystemDeviceEnum As Guid = New Guid(&H62BE5D10, &H60EB, &H11D0, &HBD, &H3B, &H0, &HA0, &HC9, &H11, &HCE, &H86)
        Public Shared ReadOnly FilterGraphManager As Guid = New Guid(&HE436EBB3, &H524F, &H11CE, &H9F, &H53, &H0, &H20, &HAF, &HB, &HA7, &H70)
        Public Shared ReadOnly CaptureGraphBuilder2 As Guid = New Guid("BF87B6E1-8C27-11D0-B3F0-00AA003761C5")
        Public Shared ReadOnly SampleGrabber As Guid = New Guid(&HC1F400A0, &H3F08, &H11D3, &H9F, &HB, &H0, &H60, &H8, &H3, &H9E, &H37)
        Public Shared ReadOnly DvdGraphBuilder As Guid = New Guid("FCC152B7-F372-11D0-8E00-00C04FD7C08B")
        Public Shared ReadOnly DVDNavigator As Guid = New Guid("9B8C4620-2C1A-11D0-8493-00A02438AD48")
        Public Shared ReadOnly IGraphBuilder_GUID As Guid = New Guid("56a868a9-0ad4-11ce-b03a-0020af0ba770")
        Public Shared ReadOnly IBaseFilter_GUID As Guid = New Guid("56a86895-0ad4-11ce-b03a-0020af0ba770")

        Public Shared ReadOnly CLSID_SystemClock As Guid = New Guid("e436ebb1-524f-11ce-9f53-0020af0ba770")
        'Public Shared ReadOnly CLSID_DVDState As Guid = New Guid(&HF963C5CF, &HA659, &H4A93, &H96, &H38, &HCA, &HF3, &HCD, &H27, &H7D, &H13)
        Public Shared ReadOnly CLSID_DVDState As Guid = New Guid("F963C5CF-A659-4A93-9638-CAF3CD277D13")

        Public Shared ReadOnly IID_IReferenceClock As Guid = New Guid("56a86897-0ad4-11ce-b03a-0020af0ba770")
        Public Shared ReadOnly IID_IDvdState As Guid = New Guid("86303d6d-1c4a-4087-ab42-f711167048ef")
        Public Shared ReadOnly IID_IPersistMemory As Guid = New Guid("BD1AE5E0-A6AE-11CE-BD37-504200C10000")

    End Class

    <ComVisible(False)> _
    Public Class MediaType  ' MEDIATYPE_*

        ' MEDIATYPE_Video 'vids'
        Public Shared ReadOnly Video As Guid = New Guid(&H73646976, &H0, &H10, &H80, &H0, &H0, &HAA, &H0, &H38, &H9B, &H71)

        ' MEDIATYPE_Interleaved 'iavs'
        Public Shared ReadOnly Interleaved As Guid = New Guid(&H73766169, &H0, &H10, &H80, &H0, &H0, &HAA, &H0, &H38, &H9B, &H71)

        ' MEDIATYPE_Audio 'auds'
        Public Shared ReadOnly Audio As Guid = New Guid(&H73647561, &H0, &H10, &H80, &H0, &H0, &HAA, &H0, &H38, &H9B, &H71)

        ' MEDIATYPE_Text 'txts'
        Public Shared ReadOnly Text As Guid = New Guid(&H73747874, &H0, &H10, &H80, &H0, &H0, &HAA, &H0, &H38, &H9B, &H71)

        ' MEDIATYPE_Stream
        Public Shared ReadOnly Stream As Guid = New Guid(&HE436EB83, &H524F, &H11CE, &H9F, &H53, &H0, &H20, &HAF, &HB, &HA7, &H70)

    End Class

    <ComVisible(False)> _
    Public Class MediaSubType  ' MEDIASUBTYPE_*

        ' MEDIASUBTYPE_YUYV 'YUYV'
        Public Shared ReadOnly YUYV As Guid = New Guid(&H56595559, &H0, &H10, &H80, &H0, &H0, &HAA, &H0, &H38, &H9B, &H71)

        ' MEDIASUBTYPE_IYUV 'IYUV'
        Public Shared ReadOnly IYUV As Guid = New Guid(&H56555949, &H0, &H10, &H80, &H0, &H0, &HAA, &H0, &H38, &H9B, &H71)

        ' MEDIASUBTYPE_DVSD 'DVSD'
        Public Shared ReadOnly DVSD As Guid = New Guid(&H44535644, &H0, &H10, &H80, &H0, &H0, &HAA, &H0, &H38, &H9B, &H71)

        ' MEDIASUBTYPE_RGB1 'RGB1'
        Public Shared ReadOnly RGB1 As Guid = New Guid(&HE436EB78, &H524F, &H11CE, &H9F, &H53, &H0, &H20, &HAF, &HB, &HA7, &H70)

        ' MEDIASUBTYPE_RGB4 'RGB4'
        Public Shared ReadOnly RGB4 As Guid = New Guid(&HE436EB79, &H524F, &H11CE, &H9F, &H53, &H0, &H20, &HAF, &HB, &HA7, &H70)

        ' MEDIASUBTYPE_RGB8 'RGB8'
        Public Shared ReadOnly RGB8 As Guid = New Guid(&HE436EB7A, &H524F, &H11CE, &H9F, &H53, &H0, &H20, &HAF, &HB, &HA7, &H70)

        ' MEDIASUBTYPE_RGB565 'RGB565'
        Public Shared ReadOnly RGB565 As Guid = New Guid(&HE436EB7B, &H524F, &H11CE, &H9F, &H53, &H0, &H20, &HAF, &HB, &HA7, &H70)

        ' MEDIASUBTYPE_RGB555 'RGB555'
        Public Shared ReadOnly RGB555 As Guid = New Guid(&HE436EB7C, &H524F, &H11CE, &H9F, &H53, &H0, &H20, &HAF, &HB, &HA7, &H70)

        ' MEDIASUBTYPE_RGB24 'RGB24'
        Public Shared ReadOnly RGB24 As Guid = New Guid(&HE436EB7D, &H524F, &H11CE, &H9F, &H53, &H0, &H20, &HAF, &HB, &HA7, &H70)

        ' MEDIASUBTYPE_RGB32 'RGB32'
        Public Shared ReadOnly RGB32 As Guid = New Guid(&HE436EB7E, &H524F, &H11CE, &H9F, &H53, &H0, &H20, &HAF, &HB, &HA7, &H70)

        ' MEDIASUBTYPE_Avi
        Public Shared ReadOnly Avi As Guid = New Guid(&HE436EB88, &H524F, &H11CE, &H9F, &H53, &H0, &H20, &HAF, &HB, &HA7, &H70)

        ' MEDIASUBTYPE_Asf
        Public Shared ReadOnly Asf As Guid = New Guid("3DB80F90-9412-11D1-ADED-0000F8754B99")

    End Class

    <ComVisible(False)> _
    Public Class FormatType  ' FORMAT_*

        ' FORMAT_None
        Public Shared ReadOnly None As Guid = New Guid("F6417D6-C318-11D0-A43F-00A0C9223196")

        ' FORMAT_VideoInfo
        Public Shared ReadOnly VideoInfo As Guid = New Guid("5589F80-C356-11CE-BF01-00AA0055595A")

        ' FORMAT_VideoInfo2
        Public Shared ReadOnly VideoInfo2 As Guid = New Guid("F72A76A0-EB0A-11D0-ACE4-0000C0CC16BA")

        ' FORMAT_WaveFormatEx
        Public Shared ReadOnly WaveEx As Guid = New Guid("5589F81-C356-11CE-BF01-00AA0055595A")

        ' FORMAT_MPEGVideo
        Public Shared ReadOnly MpegVideo As Guid = New Guid("5589F82-C356-11CE-BF01-00AA0055595A")

        ' FORMAT_MPEGStreams
        Public Shared ReadOnly MpegStreams As Guid = New Guid("5589F83-C356-11CE-BF01-00AA0055595A")

        ' FORMAT_DvInfo
        Public Shared ReadOnly DvInfo As Guid = New Guid("5589F84-C356-11CE-BF01-00AA0055595A")
    End Class

    <ComVisible(False)> _
    Public Class PinCategory  ' PIN_CATEGORY_*

        ' PIN_CATEGORY_CAPTURE
        Public Shared ReadOnly Capture As Guid = New Guid(&HFB6C4281, &H353, &H11D1, &H90, &H5F, &H0, &H0, &HC0, &HCC, &H16, &HBA)

        ' PIN_CATEGORY_PREVIEW
        Public Shared ReadOnly Preview As Guid = New Guid(&HFB6C4282, &H353, &H11D1, &H90, &H5F, &H0, &H0, &HC0, &HCC, &H16, &HBA)
    End Class

    <ComVisible(False)> _
    Public Class TimeFormat

        ' 00000000-0000-0000-0000-000000000000
        Public Shared ReadOnly TIME_FORMAT_NONE As Guid = New Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)

        ' 7b785570-8c82-11cf-bc0c-00aa00ac74f6
        Public Shared ReadOnly TIME_FORMAT_FRAME As Guid = New Guid("7B785570-8C82-11CF-BC0C-00AA00AC74F6")

        ' 7b785571-8c82-11cf-bc0c-00aa00ac74f6
        Public Shared ReadOnly TIME_FORMAT_BYTE As Guid = New Guid("7B785571-8C82-11CF-BC0C-00AA00AC74F6")

        ' 7b785572-8c82-11cf-bc0c-00aa00ac74f6
        Public Shared ReadOnly TIME_FORMAT_SAMPLE As Guid = New Guid("7B785572-8C82-11CF-BC0C-00AA00AC74F6")

        ' 7b785573-8c82-11cf-bc0c-00aa00ac74f6
        Public Shared ReadOnly TIME_FORMAT_FIELD As Guid = New Guid("7B785573-8C82-11CF-BC0C-00AA00AC74F6")

        ' 7b785574-8c82-11cf-bc0c-00aa00ac74f6
        Public Shared ReadOnly TIME_FORMAT_MEDIA_TIME As Guid = New Guid("7B785574-8C82-11CF-BC0C-00AA00AC74F6")

    End Class

    '<ComVisible(False)> _
    'Public Class TIME_FORMATs
    '    Public Shared ReadOnly TIME_FORMAT_NONE As Guid = New Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
    '    Public Shared ReadOnly TIME_FORMAT_FRAME As Guid = New Guid(&H7B785570, &H8C82, &H11CF, &HBC, &HC, &H0, &HAA, &H0, &HAC, &H74, &HF6) ' 7b785570-8c82-11cf-bc0c-00aa00ac74f6
    '    Public Shared ReadOnly TIME_FORMAT_BYTE As Guid = New Guid(&H7B785571, &H8C82, &H11CF, &HBC, &HC, &H0, &HAA, &H0, &HAC, &H74, &HF6) ' 7b785571-8c82-11cf-bc0c-00aa00ac74f6
    '    Public Shared ReadOnly TIME_FORMAT_SAMPLE As Guid = New Guid(&H7B785572, &H8C82, &H11CF, &HBC, &HC, &H0, &HAA, &H0, &HAC, &H74, &HF6) ' 7b785572-8c82-11cf-bc0c-00aa00ac74f6
    '    Public Shared ReadOnly TIME_FORMAT_FIELD As Guid = New Guid(&H7B785573, &H8C82, &H11CF, &HBC, &HC, &H0, &HAA, &H0, &HAC, &H74, &HF6) ' 7b785573-8c82-11cf-bc0c-00aa00ac74f6
    '    Public Shared ReadOnly TIME_FORMAT_MEDIA_TIME As Guid = New Guid(&H7B785574, &H8C82, &H11CF, &HBC, &HC, &H0, &HAA, &H0, &HAC, &H74, &HF6) ' 7b785574-8c82-11cf-bc0c-00aa00ac74f6
    'End Class

End Namespace
