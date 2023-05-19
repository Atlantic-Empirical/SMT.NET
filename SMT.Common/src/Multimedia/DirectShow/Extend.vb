Imports System
Imports System.Runtime.InteropServices

Namespace Multimedia.DirectShow

    <ComVisible(True), ComImport(), Guid("93E5A4E0-2D50-11d2-ABFA-00A0C9C6E38D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface ICaptureGraphBuilder2

        <PreserveSig()> _
        Function SetFiltergraph(<[In]()> _
     ByVal pfg As IGraphBuilder) As Integer

        <PreserveSig()> _
        Function GetFiltergraph(<Out()> _
     ByRef ppfg As IGraphBuilder) As Integer

        <PreserveSig()> _
        Function SetOutputFileName(<[In]()> _
     ByRef pType As Guid, <[In](), MarshalAs(UnmanagedType.LPWStr)> _
     ByVal lpstrFile As String, <Out()> _
     ByRef ppbf As IBaseFilter, <Out()> _
     ByRef ppSink As IFileSinkFilter) As Integer

        <PreserveSig()> _
        Function FindInterface(<[In]()> _
     ByRef pCategory As Guid, <[In]()> _
     ByRef pType As Guid, <[In]()> _
     ByVal pbf As IBaseFilter, <[In]()> _
     ByRef riid As Guid, <Out(), MarshalAs(UnmanagedType.IUnknown)> _
     ByRef ppint As Object) As Integer

        <PreserveSig()> _
        Function RenderStream(<[In]()> _
     ByRef pCategory As Guid, <[In]()> _
     ByRef pType As Guid, <[In](), MarshalAs(UnmanagedType.IUnknown)> _
     ByVal pSource As Object, <[In]()> _
     ByVal pfCompressor As IBaseFilter, <[In]()> _
     ByVal pfRenderer As IBaseFilter) As Integer

        <PreserveSig()> _
        Function ControlStream(<[In]()> _
     ByRef pCategory As Guid, <[In]()> _
     ByRef pType As Guid, <[In]()> _
     ByVal pFilter As IBaseFilter, <[In]()> _
     ByVal pstart As IntPtr, <[In]()> _
     ByVal pstop As IntPtr, <[In]()> _
     ByVal wStartCookie As Short, <[In]()> _
     ByVal wStopCookie As Short) As Integer

        <PreserveSig()> _
        Function AllocCapFile(<[In](), MarshalAs(UnmanagedType.LPWStr)> _
     ByVal lpstrFile As String, <[In]()> _
     ByVal dwlSize As Long) As Integer

        <PreserveSig()> _
        Function CopyCaptureFile(<[In](), MarshalAs(UnmanagedType.LPWStr)> _
     ByVal lpwstrOld As String, <[In](), MarshalAs(UnmanagedType.LPWStr)> _
     ByVal lpwstrNew As String, <[In]()> _
     ByVal fAllowEscAbort As Integer, <[In]()> _
     ByVal pFilter As IAMCopyCaptureFileProgress) As Integer

        <PreserveSig()> _
        Function FindPin(<[In]()> _
     ByVal pSource As Object, <[In]()> _
     ByVal pindir As Integer, <[In]()> _
     ByRef pCategory As Guid, <[In]()> _
     ByRef pType As Guid, <[In](), MarshalAs(UnmanagedType.Bool)> _
     ByVal fUnconnected As Boolean, <[In]()> _
     ByVal num As Integer, <Out()> _
     ByRef ppPin As IPin) As Integer
    End Interface

    <ComVisible(True), ComImport(), Guid("56a868a9-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IGraphBuilder

        <PreserveSig()> _
        Function AddFilter(<[In]()> _
     ByVal pFilter As IBaseFilter, <[In](), MarshalAs(UnmanagedType.LPWStr)> _
     ByVal pName As String) As Integer

        <PreserveSig()> _
        Function RemoveFilter(<[In]()> _
     ByVal pFilter As IBaseFilter) As Integer

        <PreserveSig()> _
        Function EnumFilters(<Out()> _
     ByRef ppEnum As IEnumFilters) As Integer

        <PreserveSig()> _
        Function FindFilterByName(<[In](), MarshalAs(UnmanagedType.LPWStr)> _
     ByVal pName As String, <Out()> _
     ByRef ppFilter As IBaseFilter) As Integer

        <PreserveSig()> _
        Function ConnectDirect(<[In]()> _
     ByVal ppinOut As IPin, <[In]()> _
     ByVal ppinIn As IPin) As Integer

        <PreserveSig()> _
        Function Reconnect(<[In]()> _
     ByVal ppin As IPin) As Integer

        <PreserveSig()> _
        Function Disconnect(<[In]()> _
     ByVal ppin As IPin) As Integer

        <PreserveSig()> _
        Function SetDefaultSyncSource() As Integer

        <PreserveSig()> _
        Function Connect(<[In]()> _
     ByVal ppinOut As IPin, <[In]()> _
     ByVal ppinIn As IPin) As Integer

        <PreserveSig()> _
        Function Render(<[In]()> _
     ByVal ppinOut As IPin) As Integer

        <PreserveSig()> _
        Function RenderFile(<[In](), MarshalAs(UnmanagedType.LPWStr)> _
     ByVal lpcwstrFile As String, <[In](), MarshalAs(UnmanagedType.LPWStr)> _
     ByVal lpcwstrPlayList As String) As Integer

        <PreserveSig()> _
        Function AddSourceFilter(<[In](), MarshalAs(UnmanagedType.LPWStr)> _
     ByVal lpcwstrFileName As String, <[In](), MarshalAs(UnmanagedType.LPWStr)> _
     ByVal lpcwstrFilterName As String, <Out()> _
     ByRef ppFilter As IBaseFilter) As Integer

        <PreserveSig()> _
        Function SetLogFile(ByVal hFile As IntPtr) As Integer

        <PreserveSig()> _
        Function Abort() As Integer

        <PreserveSig()> _
        Function ShouldOperationContinue() As Integer
    End Interface

    <ComVisible(True), ComImport(), Guid("a2104830-7c70-11cf-8bce-00aa00a3f1a6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IFileSinkFilter

        <PreserveSig()> _
        Function SetFileName(<[In](), MarshalAs(UnmanagedType.LPWStr)> _
     ByVal pszFileName As String, <[In](), MarshalAs(UnmanagedType.LPStruct)> _
     ByVal pmt As AMMediaType) As Integer

        <PreserveSig()> _
        Function GetCurFile(<Out(), MarshalAs(UnmanagedType.LPWStr)> _
     ByRef pszFileName As String, <Out(), MarshalAs(UnmanagedType.LPStruct)> _
     ByVal pmt As AMMediaType) As Integer
    End Interface

    <ComVisible(True), ComImport(), Guid("670d1d20-a068-11d0-b3f0-00aa003761c5"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IAMCopyCaptureFileProgress

        <PreserveSig()> _
        Function Progress(ByVal iProgress As Integer) As Integer
    End Interface

    <ComVisible(True), ComImport(), Guid("e46a9787-2b71-444d-a4b5-1fab7b708d6a"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IVideoFrameStep

        <PreserveSig()> _
        Function [Step](ByVal dwFrames As Integer, <[In](), MarshalAs(UnmanagedType.IUnknown)> _
     ByVal pStepObject As Object) As Integer

        <PreserveSig()> _
        Function CanStep(ByVal bMultiple As Integer, <[In](), MarshalAs(UnmanagedType.IUnknown)> _
     ByVal pStepObject As Object) As Integer

        <PreserveSig()> _
        Function CancelStep() As Integer
    End Interface

    <ComVisible(True), ComImport(), Guid("C6E13340-30AC-11d0-A18C-00A0C9118956"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IAMStreamConfig

        <PreserveSig()> _
        Function SetFormat(<[In](), MarshalAs(UnmanagedType.LPStruct)> _
     ByVal pmt As AM_MEDIA_TYPE) As Integer

        <PreserveSig()> _
        Function GetFormat(<Out(), MarshalAs(UnmanagedType.LPStruct)> _
     ByVal pmt As AMMediaType) As Integer

        <PreserveSig()> _
        Function GetNumberOfCapabilities(ByRef piCount As Integer, ByRef piSize As Integer) As Integer

        <PreserveSig()> _
        Function GetStreamCaps(ByVal iIndex As Integer, <Out(), MarshalAs(UnmanagedType.LPStruct)> _
     ByRef ppmt As AMMediaType, ByVal pSCC As IntPtr) As Integer
    End Interface

    <Flags()> _
    Public Enum AMPROPERTY_PIN
        AMPROPERTY_PIN_CATEGORY
        AMPROPERTY_PIN_MEDIUM
    End Enum

    <ComVisible(True), ComImport(), Guid("31EFAC30-515C-11d0-A9AA-00AA0061BE93"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IKsPropertySet

        <PreserveSig()> _
        Function [Set](<[In]()> _
     ByVal dwPropID As Integer, <[In]()> _
     ByVal pInstanceData As Byte(), <[In]()> _
     ByVal cbInstanceData As Integer, <[In]()> _
     ByVal pPropData As Byte(), <[In]()> _
     ByVal cbPropData As Integer) As Integer

        <PreserveSig()> _
        Function [Get](<[In]()> _
     ByVal guidPropSet As Guid, <[In]()> _
     ByVal dwPropId As Integer, <[In]()> _
     ByVal pInstanceData As Byte(), <[In]()> _
     ByVal cbInstanceData As Integer, ByRef pPropData As Byte(), <[In]()> _
     ByVal cbPropData As Integer, ByRef pcbReturned As Integer) As Integer

        <PreserveSig()> _
        Function QuerySupported(<[In]()> _
     ByVal guidPropSet As Guid, <[In]()> _
     ByVal dwPropID As Integer, ByRef pTypeSupport As Integer) As Integer
    End Interface

    <ComVisible(True), ComImport(), Guid("56a868a6-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IFileSourceFilter

        <PreserveSig()> _
        Function Load(<[In](), MarshalAs(UnmanagedType.LPWStr)> _
     ByVal pszFileName As String, <[In]()> _
     ByVal pmt As Integer) As Integer

        <PreserveSig()> _
        Function GetCurFile(<Out()> _
     ByVal ppszFileName As Integer, <Out()> _
     ByVal pmt As Integer) As Integer

    End Interface

    <ComVisible(True), ComImport(), Guid("5AA8CFff-9235-4b4d-A7B2-8C2BA21D0104"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IFileSourceFilterSMT

        <PreserveSig()> _
        Function Load(<[In](), MarshalAs(UnmanagedType.LPWStr)> _
         ByVal pszFileName As String, <[In]()> _
         ByVal pmt As Integer) As Integer

        <PreserveSig()> _
        Function GetCurFile(<Out()> _
         ByVal ppszFileName As Integer, <Out()> _
         ByVal pmt As Integer) As Integer

        <PreserveSig()> _
        Function SetATPF(<[In]()> ByVal TargetATPF As UInt64) As Integer

    End Interface

End Namespace
