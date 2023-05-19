Imports System
Imports System.Text 
Imports System.Runtime.InteropServices 

Namespace Multimedia.Filters.SMT.Keystone

    <ComVisible(True), ComImport(), Guid("4F96B59E-11EB-4e02-9A7C-1CA55AB7D7FF"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IKeystoneProcAmp_OMNI

        <PreserveSig()> _
        Function put_Brightness(ByVal dBrightness As Double) As Integer

        <PreserveSig()> _
        Function get_Brightness(ByRef dBrightness As Double) As Integer

        <PreserveSig()> _
        Function put_Contrast(ByVal dContrast As Double) As Integer

        <PreserveSig()> _
        Function get_Contrast(ByRef dContrast As Double) As Integer

        <PreserveSig()> _
        Function put_Hue(ByVal dHue As Double) As Integer

        <PreserveSig()> _
        Function get_Hue(ByRef dHue As Double) As Integer

        <PreserveSig()> _
        Function put_Saturation(ByVal dSaturation As Double) As Integer

        <PreserveSig()> _
        Function get_Saturation(ByRef dSaturation As Double) As Integer

        <PreserveSig()> _
        Function ToggleProcAmp(ByVal bToggleProcAmp As Boolean, ByVal bHalfFrame As Boolean) As Integer

        <PreserveSig()> _
        Function ToggleColorFilter(ByVal bDoColorFilter As Boolean, ByVal iUseWhichFilter As Integer) As Integer

    End Interface

End Namespace