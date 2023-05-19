Imports System.Runtime.InteropServices

Namespace Multimedia.Filters.SMT.Keystone

    <ComVisible(False)> _
    Public Enum eImageFormat
        IF_RGB24 = 0
        IF_RGB32 = 1
        IF_YUY2 = 2
        IF_UYVY = 3
        IF_ARGB4444 = 4
    End Enum

    <ComVisible(False)> _
    Public Enum eSampleFrom
        VideoIn = 0
        SubpictureIn = 1
        Line21In = 2
        Output = 3
    End Enum

End Namespace
