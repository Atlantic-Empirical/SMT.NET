Imports System
Imports System.Runtime.InteropServices

Namespace Multimedia.DirectShow

    <ComVisible(True), ComImport(), Guid("6B652FFF-11FE-4fce-92AD-0266B5D7C78F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface ISampleGrabber

        <PreserveSig()> _
        Function SetOneShot(<[In](), MarshalAs(UnmanagedType.Bool)> _
     ByVal OneShot As Boolean) As Integer

        <PreserveSig()> _
        Function SetMediaType(<[In](), MarshalAs(UnmanagedType.LPStruct)> _
     ByVal pmt As AMMediaType) As Integer

        <PreserveSig()> _
        Function GetConnectedMediaType(<Out(), MarshalAs(UnmanagedType.LPStruct)> _
     ByVal pmt As AMMediaType) As Integer

        <PreserveSig()> _
        Function SetBufferSamples(<[In](), MarshalAs(UnmanagedType.Bool)> _
     ByVal BufferThem As Boolean) As Integer

        <PreserveSig()> _
        Function GetCurrentBuffer(ByRef pBufferSize As Integer, ByVal pBuffer As IntPtr) As Integer

        <PreserveSig()> _
        Function GetCurrentSample(ByVal ppSample As IntPtr) As Integer

        <PreserveSig()> _
        Function SetCallback(ByVal pCallback As ISampleGrabberCB, ByVal WhichMethodToCallback As Integer) As Integer
    End Interface

    <ComVisible(True), ComImport(), Guid("0579154A-2B53-4994-B0D0-E773148EFF85"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface ISampleGrabberCB

        <PreserveSig()> _
        Function SampleCB(ByVal SampleTime As Double, ByVal pSample As IMediaSample) As Integer

        <PreserveSig()> _
        Function BufferCB(ByVal SampleTime As Double, ByVal pBuffer As IntPtr, ByVal BufferLen As Integer) As Integer
    End Interface
End Namespace