Imports System
Imports System.Runtime.InteropServices
Imports System.IO

Namespace Multimedia.DirectShow

    <ComVisible(True), ComImport(), Guid("593CDDE1-0759-11d1-9E69-00C04FD7C15B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IMixerPinConfig

        <PreserveSig()> _
        Function SetRelativePosition(<[In]()> _
     ByVal dwLeft As Integer, <[In]()> _
     ByVal dwTop As Integer, <[In]()> _
     ByVal dwRight As Integer, <[In]()> _
     ByVal dwBottom As Integer) As Integer

        <PreserveSig()> _
        Function GetRelativePosition(<Out()> _
     ByRef pLeft As Integer, <Out()> _
     ByRef pTop As Integer, <Out()> _
     ByRef pRight As Integer, <Out()> _
     ByRef pBottom As Integer) As Integer

        <PreserveSig()> _
        Function SetZOrder(<[In]()> _
     ByVal pZOrder As Integer) As Integer

        <PreserveSig()> _
        Function GetZOrder(<Out()> _
     ByRef pZOrder As Integer) As Integer

        <PreserveSig()> _
        Function SetColorKey(ByRef pColorKey As COLORKEY) As Integer

        <PreserveSig()> _
        Function GetColorKey(<Out()> _
     ByRef pColorKey As COLORKEY, <Out()> _
     ByRef pColor As Integer) As Integer

        <PreserveSig()> _
        Function SetBlendingParameter(<[In]()> _
     ByVal dwBlendingParameter As Integer) As Integer

        <PreserveSig()> _
        Function GetBlendingParameter(<Out()> _
     ByRef pdwBlendingParameter As Integer) As Integer

        <PreserveSig()> _
        Function SetAspectRatioMode(ByVal amAspectRatioMode As AM_ASPECT_RATIO_MODE) As Integer

        <PreserveSig()> _
        Function GetAspectRatioMode(ByVal pamAspectRatioMode As IntPtr) As Integer

        <PreserveSig()> _
        Function SetStreamTransparent(<[In](), MarshalAs(UnmanagedType.Bool)> _
     ByVal bStreamTransparent As Boolean) As Integer

        <PreserveSig()> _
        Function GetStreamTransparent(<Out(), MarshalAs(UnmanagedType.Bool)> _
     ByRef pbStreamTransparent As Boolean) As Integer
    End Interface

    Public Enum AM_ASPECT_RATIO_MODE
        AM_ARMODE_STRETCHED
        AM_ARMODE_LETTER_BOX
        AM_ARMODE_CROP
        AM_ARMODE_STRETCHED__PRIMARY
    End Enum

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Class COLORKEY
        Public KeyType As Long
        Public PaletteIndex As Long
        Public HighColorValue As Long
    End Class
End Namespace