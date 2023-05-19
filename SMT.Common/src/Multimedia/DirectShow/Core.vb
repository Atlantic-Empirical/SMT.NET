Imports System
Imports System.Text
Imports System.Runtime.InteropServices

Namespace Multimedia.DirectShow

    <ComVisible(False)> _
    Public Enum PinDirection
        Input
        Output
    End Enum

    <ComVisible(False)> _
    Public Class DsHlp
        Public Const OATRUE As Integer = -1
        Public Const OAFALSE As Integer = 0

        <DllImport("quartz.dll", CharSet:=CharSet.Auto)> _
        Public Shared Function AMGetErrorText(ByVal hr As Integer, ByVal buf As StringBuilder, ByVal max As Integer) As Integer
        End Function
    End Class

    <ComVisible(True), ComImport(), Guid("56a86891-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IPin

        <PreserveSig()> _
        Function Connect(<[In]()> ByVal pReceivePin As IPin, ByRef pmt As AM_MEDIA_TYPE) As Integer

        <PreserveSig()> _
        Function ReceiveConnection( _
             <[In]()> ByVal pReceivePin As IPin, _
             <[In](), MarshalAs(UnmanagedType.LPStruct)> ByVal pmt As AMMediaType) As Integer

        <PreserveSig()> _
        Function Disconnect() As Integer

        <PreserveSig()> _
        Function ConnectedTo(<Out()> ByRef ppPin As IPin) As Integer

        <PreserveSig()> _
        Function ConnectionMediaType(ByRef pmt As AM_MEDIA_TYPE) As Integer

        <PreserveSig()> _
        Function QueryPinInfo(ByVal pInfo As IntPtr) As Integer

        <PreserveSig()> _
        Function QueryDirection(ByRef pPinDir As PinDirection) As Integer

        <PreserveSig()> _
        Function QueryId(<Out(), MarshalAs(UnmanagedType.LPWStr)> _
     ByRef Id As String) As Integer

        <PreserveSig()> _
        Function QueryAccept(<[In](), MarshalAs(UnmanagedType.LPStruct)> ByVal pmt As AMMediaType) As Integer

        <PreserveSig()> _
        Function EnumMediaTypes(<Out()> ByRef ppEnum As IEnumMediaTypes) As Integer

        <PreserveSig()> _
        Function QueryInternalConnections( _
             ByVal apPin As IntPtr, _
             <[In](), Out()> _
            ByRef nPin As Integer) As Integer

        <PreserveSig()> _
        Function EndOfStream() As Integer

        <PreserveSig()> _
        Function BeginFlush() As Integer

        <PreserveSig()> _
        Function EndFlush() As Integer

        <PreserveSig()> _
        Function NewSegment(ByVal tStart As Long, ByVal tStop As Long, ByVal dRate As Double) As Integer

    End Interface

    <ComVisible(True), ComImport(), Guid("56a8689f-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IFilterGraph

        <PreserveSig()> _
        Function AddFilter( _
            <[In]()> ByVal pFilter As IBaseFilter, _
            <[In](), MarshalAs(UnmanagedType.LPWStr)> ByVal pName As String) As Integer

        <PreserveSig()> _
        Function RemoveFilter(<[In]()> ByVal pFilter As IBaseFilter) As Integer

        <PreserveSig()> _
        Function EnumFilters(<Out()> ByRef ppEnum As IEnumFilters) As Integer

        <PreserveSig()> _
        Function FindFilterByName( _
             <[In](), MarshalAs(UnmanagedType.LPWStr)> ByVal pName As String, _
             <Out()> ByRef ppFilter As IBaseFilter) As Integer

        <PreserveSig()> _
        Function ConnectDirect( _
            <[In]()> ByVal ppinOut As IPin, _
            <[In]()> ByVal ppinIn As IPin) As Integer

        <PreserveSig()> _
        Function Reconnect(<[In]()> ByVal ppin As IPin) As Integer

        <PreserveSig()> _
        Function Disconnect(<[In]()> ByVal ppin As IPin) As Integer

        <PreserveSig()> _
        Function SetDefaultSyncSource() As Integer

    End Interface

    <ComVisible(True), ComImport(), Guid("0000010c-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IPersist

        <PreserveSig()> _
        Function GetClassID(<Out()> ByRef pClassID As Guid) As Integer

    End Interface

    <ComVisible(True), ComImport(), Guid("56a86899-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IMediaFilter

        <PreserveSig()> _
        Function GetClassID(<Out()> ByRef pClassID As Guid) As Integer

        <PreserveSig()> _
        Function [Stop]() As Integer

        <PreserveSig()> _
        Function Pause() As Integer

        <PreserveSig()> _
        Function Run(ByVal tStart As Long) As Integer

        <PreserveSig()> _
        Function GetState(ByVal dwMilliSecsTimeout As Integer, ByRef filtState As Integer) As Integer

        <PreserveSig()> _
        Function SetSyncSource(<[In]()> ByVal pClock As IReferenceClock) As Integer 'IReferenceClock

        <PreserveSig()> _
        Function GetSyncSource(<Out()> ByRef pClock As IReferenceClock) As Integer

    End Interface

    <ComVisible(True), ComImport(), Guid("56a86895-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IBaseFilter

        <PreserveSig()> _
        Function GetClassID(<Out()> ByRef pClassID As Guid) As Integer

        <PreserveSig()> _
        Function [Stop]() As Integer

        <PreserveSig()> _
        Function Pause() As Integer

        <PreserveSig()> _
        Function Run(ByVal tStart As Long) As Integer

        <PreserveSig()> _
        Function GetState( _
            ByVal dwMilliSecsTimeout As Integer, _
            <Out()> ByRef filtState As Integer _
        ) As Integer

        <PreserveSig()> _
        Function SetSyncSource(<[In]()> ByVal pClock As IReferenceClock) As Integer

        <PreserveSig()> _
        Function GetSyncSource(<Out()> ByRef pClock As IReferenceClock) As Integer

        <PreserveSig()> _
        Function EnumPins(<Out()> ByRef ppEnum As IEnumPins) As Integer

        <PreserveSig()> _
        Function FindPin( _
             <[In](), MarshalAs(UnmanagedType.LPWStr)> ByVal Id As String, _
             <Out()> ByRef ppPin As IPin _
        ) As Integer

        <PreserveSig()> _
        Function QueryFilterInfo(<Out()> ByRef pInfo As FilterInfo) As Integer

        <PreserveSig()> _
        Function JoinFilterGraph( _
             <[In]()> ByVal pGraph As IFilterGraph, _
             <[In](), MarshalAs(UnmanagedType.LPWStr)> ByVal pName As String _
        ) As Integer

        <PreserveSig()> _
        Function QueryVendorInfo(<Out(), MarshalAs(UnmanagedType.LPWStr)> ByRef pVendorInfo As String) As Integer

    End Interface

    '<StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    'Public Class FilterInfo
    '    Public achName As String
    '    Public pGraph As Integer
    'End Class

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)> _
    Public Structure FilterInfo
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=128)> Public achName As String
        <MarshalAs(UnmanagedType.[Interface])> Public pGraph As IFilterGraph
    End Structure

    <ComVisible(True), ComImport(), Guid("36b73880-c2c8-11cf-8b46-00805f6cef60"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IMediaSeeking

        <PreserveSig()> _
        Function GetCapabilities(ByRef pCapabilities As SeekingCapabilities) As Integer

        <PreserveSig()> _
        Function CheckCapabilities(<[In](), Out()> ByRef pCapabilities As SeekingCapabilities) As Integer

        <PreserveSig()> _
        Function IsFormatSupported(<[In]()> ByRef pFormat As Guid) As Integer

        <PreserveSig()> _
        Function QueryPreferredFormat(<Out()> ByRef pFormat As Guid) As Integer

        <PreserveSig()> _
        Function GetTimeFormat(<Out()> ByRef pFormat As UInt32) As Integer

        <PreserveSig()> _
        Function IsUsingTimeFormat(<[In]()> ByRef pFormat As Guid) As Integer

        <PreserveSig()> _
        Function SetTimeFormat(<[In]()> ByRef pFormat As Guid) As Integer

        <PreserveSig()> _
        Function GetDuration(ByRef pDuration As Long) As Integer

        <PreserveSig()> _
        Function GetStopPosition(ByRef pStop As Long) As Integer

        <PreserveSig()> _
        Function GetCurrentPosition(ByRef pCurrent As Long) As Integer

        <PreserveSig()> _
        Function ConvertTimeFormat(ByRef pTarget As Long, <[In]()> _
     ByRef pTargetFormat As Guid, ByVal Source As Long, <[In]()> _
     ByRef pSourceFormat As Guid) As Integer

        <PreserveSig()> _
        Function SetPositions(ByRef pCurrent As System.Int64, ByVal dwCurrentFlags As SeekingFlags, ByRef pStop As System.Int64, ByVal dwStopFlags As SeekingFlags) As Integer

        <PreserveSig()> _
        Function GetPositions(ByRef pCurrent As Long, ByRef pStop As Long) As Integer

        <PreserveSig()> _
        Function GetAvailable(ByRef pEarliest As Long, ByRef pLatest As Long) As Integer

        <PreserveSig()> _
        Function SetRate(ByVal dRate As Double) As Integer

        <PreserveSig()> _
        Function GetRate(ByRef pdRate As Double) As Integer

        <PreserveSig()> _
        Function GetPreroll(ByRef pllPreroll As Long) As Integer

    End Interface

    <Flags(), ComVisible(False)> _
    Public Enum SeekingCapabilities
        CanSeekAbsolute = 1
        CanSeekForwards = 2
        CanSeekBackwards = 4
        CanGetCurrentPos = 8
        CanGetStopPos = 16
        CanGetDuration = 32
        CanPlayBackwards = 64
        CanDoSegments = 128
        Source = 256
    End Enum

    <Flags(), ComVisible(False)> _
    Public Enum SeekingFlags
        NoPositioning = 0
        AbsolutePositioning = 1
        RelativePositioning = 2
        IncrementalPositioning = 3
        PositioningBitsMk = 3
        SeekToKeyFrame = 4
        ReturnTime = 8
        Segment = 16
        NoFlush = 32
    End Enum

    <ComVisible(True), ComImport(), Guid("56a86897-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IReferenceClock

        <PreserveSig()> _
        Function GetTime(ByRef pTime As Long) As Integer

        <PreserveSig()> _
        Function AdviseTime(ByVal baseTime As Long, ByVal streamTime As Long, ByVal hEvent As IntPtr, ByRef pdwAdviseCookie As Integer) As Integer

        <PreserveSig()> _
        Function AdvisePeriodic(ByVal startTime As Long, ByVal periodTime As Long, ByVal hSemaphore As IntPtr, ByRef pdwAdviseCookie As Integer) As Integer

        <PreserveSig()> _
        Function Unadvise(ByVal dwAdviseCookie As Integer) As Integer

    End Interface

    <ComVisible(True), ComImport(), Guid("56a86893-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IEnumFilters

        <PreserveSig()> _
        Function [Next]( _
            ByVal cFilters As Integer, _
            <MarshalAs(UnmanagedType.LPArray), Out()> ByVal ppFilters As Integer(), _
            ByRef pcFetched As Integer _
        ) As Integer

        <PreserveSig()> _
        Function Skip(<[In]()> ByVal cFilters As Integer) As Integer

        Function Reset() As Integer

        Function Clone(<Out()> ByRef ppEnum As IEnumFilters) As Integer

    End Interface

    <ComVisible(True), ComImport(), Guid("56a86892-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IEnumPins

        <PreserveSig()> _
        Function [Next]( _
             ByVal cPins As Integer, _
             <MarshalAs(UnmanagedType.LPArray), Out()> ByVal ppPins As Integer(), _
             ByRef fetched As Integer _
        ) As Integer

        <PreserveSig()> _
        Function Skip(<[In]()> ByVal cPins As Integer) As Integer

        Sub Reset()

        Sub Clone(<Out()> ByRef ppEnum As IEnumPins)

    End Interface

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Class AMMediaType
        Public majorType As Guid
        Public subType As Guid
        <MarshalAs(UnmanagedType.Bool)> Public fixedSizeSamples As Boolean
        <MarshalAs(UnmanagedType.Bool)> Public temporalCompression As Boolean
        Public sampleSize As Integer
        Public formatType As Guid
        Public unkPtr As IntPtr
        Public formatSize As Integer
        Public formatPtr As IntPtr
    End Class

    <ComVisible(True), ComImport(), Guid("56a8689a-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IMediaSample

        <PreserveSig()> _
        Function GetPointer(ByRef ppBuffer As IntPtr) As Integer

        <PreserveSig()> _
        Function GetSize() As Integer

        <PreserveSig()> _
        Function GetTime(ByRef pTimeStart As Long, ByRef pTimeEnd As Long) As Integer

        <PreserveSig()> _
        Function SetTime( _
             <[In](), MarshalAs(UnmanagedType.LPStruct)> ByVal pTimeStart As DsOptInt64, _
             <[In](), MarshalAs(UnmanagedType.LPStruct)> ByVal pTimeEnd As DsOptInt64 _
        ) As Integer

        <PreserveSig()> _
        Function IsSyncPoint() As Integer

        <PreserveSig()> _
        Function SetSyncPoint(<[In](), MarshalAs(UnmanagedType.Bool)> ByVal bIsSyncPoint As Boolean) As Integer

        <PreserveSig()> _
        Function IsPreroll() As Integer

        <PreserveSig()> _
        Function SetPreroll(<[In](), MarshalAs(UnmanagedType.Bool)> ByVal bIsPreroll As Boolean) As Integer

        <PreserveSig()> _
        Function GetActualDataLength() As Integer

        <PreserveSig()> _
        Function SetActualDataLength(ByVal len As Integer) As Integer

        <PreserveSig()> _
        Function GetMediaType(<Out(), MarshalAs(UnmanagedType.LPStruct)> ByRef ppMediaType As AMMediaType) As Integer

        <PreserveSig()> _
        Function SetMediaType(<[In](), MarshalAs(UnmanagedType.LPStruct)> ByVal pMediaType As AMMediaType) As Integer

        <PreserveSig()> _
        Function IsDiscontinuity() As Integer

        <PreserveSig()> _
        Function SetDiscontinuity(<[In](), MarshalAs(UnmanagedType.Bool)> ByVal bDiscontinuity As Boolean) As Integer

        <PreserveSig()> _
        Function GetMediaTime(ByRef pTimeStart As Long, ByRef pTimeEnd As Long) As Integer

        <PreserveSig()> _
        Function SetMediaTime( _
             <[In](), MarshalAs(UnmanagedType.LPStruct)> ByVal pTimeStart As DsOptInt64, _
             <[In](), MarshalAs(UnmanagedType.LPStruct)> ByVal pTimeEnd As DsOptInt64 _
        ) As Integer

    End Interface

    <ComVisible(True), ComImport(), Guid("36b73884-c2c8-11cf-8b46-00805f6cef60"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IMediaSample2
        Inherits IMediaSample

        Function GetProperties(<[In]()> ByVal cbProperties As Integer, ByRef pbProperties As Integer) As Integer

        Function SetProperties( _
            <[In]()> ByVal cbProperties As Integer, _
            <[In]()> ByVal pbProperties As Integer) As Integer
    End Interface

    <ComVisible(True), ComImport(), Guid("89c31040-846b-11ce-97d3-00aa0055595a"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IEnumMediaTypes

        Function [Next]( _
            <[In]()> ByVal cMediaTypes As Integer, _
            <MarshalAs(UnmanagedType.LPArray), Out()> ByVal ppMediaTypes As Integer(), _
            ByRef pcFetched As Integer _
        ) As Integer

        Function Skip(<[In]()> ByVal cMediaTypes As Integer) As Integer

        Function Reset() As Integer

        Function Clone(<Out()> ByRef ppEnum As IEnumMediaTypes) As Integer

    End Interface

    <ComImport(), System.Security.SuppressUnmanagedCodeSecurity(), Guid("9FD52741-176D-4b36-8F51-CA8F933223BE"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IAMClockSlave

        <PreserveSig()> _
        Function SetErrorTolerance(<[In]()> ByVal dwTolerance As Integer) As Integer

        <PreserveSig()> _
        Function GetErrorTolerance(<Out()> ByRef pdwTolerance As Integer) As Integer

    End Interface

End Namespace