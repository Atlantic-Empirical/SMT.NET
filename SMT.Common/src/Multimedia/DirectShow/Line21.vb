Imports System
Imports System.Runtime.InteropServices

Namespace Multimedia.DirectShow

    Public Enum AMLine21_CCLevel
        TC2 = 0
    End Enum

    Public Enum AMLine21_CCService
        None = 0
        Caption1 = 1
        Caption2 = 2
        Text1 = 3
        Text2 = 4
        XDS = 5
        DefChannel = 10
        Invalid = 11
    End Enum

    Public Enum AMLine21_CCState
        Off = 0
        [On] = 1
    End Enum

    Public Enum AMLine21_CCStyle
        None = 0
        PopOn = 1
        PaintOn = 2
        RollUp = 3
    End Enum

    Public Enum AMLine21_DrawBGMode
        Opaque = 0
        Transparent = 1
    End Enum

    <ComVisible(True), ComImport(), Guid("6E8D4A21-310C-11d0-B79A-00AA003767A7"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IAMLine21Decoder

        <PreserveSig()> _
        Function GetDecoderLevel(<Out()> _
     ByRef level As AMLine21_CCLevel) As Integer

        <PreserveSig()> _
        Function GetCurrentService(<Out()> _
     ByRef service As AMLine21_CCService) As Integer

        <PreserveSig()> _
        Function SetCurrentService(ByVal service As AMLine21_CCService) As Integer

        <PreserveSig()> _
        Function GetServiceState(<Out()> _
     ByRef state As AMLine21_CCState) As Integer

        <PreserveSig()> _
        Function SetServiceState(ByVal state As AMLine21_CCState) As Integer

        <PreserveSig()> _
        Function GetOutputFormat(ByVal dummy As IntPtr) As Integer

        <PreserveSig()> _
        Function SetOutputFormat(ByVal dummy As IntPtr) As Integer

        <PreserveSig()> _
        Function GetBackgroundColor(<Out()> _
     ByRef color As Integer) As Integer

        <PreserveSig()> _
        Function SetBackgroundColor(ByVal color As Integer) As Integer

        <PreserveSig()> _
        Function GetRedrawAlways(<Out(), MarshalAs(UnmanagedType.Bool)> ByRef [option] As Boolean) As Integer

        <PreserveSig()> _
        Function SetRedrawAlways(<[In](), MarshalAs(UnmanagedType.Bool)> _
     ByVal [option] As Boolean) As Integer

        <PreserveSig()> _
        Function GetDrawBackgroundMode(<Out()> _
     ByRef mode As AMLine21_DrawBGMode) As Integer

        <PreserveSig()> _
        Function SetDrawBackgroundMode(ByVal mode As AMLine21_DrawBGMode) As Integer

    End Interface

End Namespace
