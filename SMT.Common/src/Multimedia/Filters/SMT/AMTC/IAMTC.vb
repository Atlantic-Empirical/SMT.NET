Imports System
Imports System.Text
Imports System.Runtime.InteropServices

Namespace Multimedia.Filters.SMT.AMTC

    <ComVisible(True), ComImport(), Guid("B388D24F-8CD8-4cc2-BCC3-12274BABCD24"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IAMTC

        <PreserveSig()> _
        Function GetBuffer(ByRef Sample As IntPtr, ByRef SampleSize As Integer) As Integer

        <PreserveSig()> _
        Function StoreBuffers(<[In]()> ByVal bGrabBuffers As Boolean) As Integer

        <PreserveSig()> _
        Function FrameStep(<[In]()> ByVal bForward As Boolean) As Integer

        <PreserveSig()> _
        Function QuitFrameStepping() As Integer

        <PreserveSig()> _
        Function SetNULLTimestamps() As Integer

    End Interface

End Namespace