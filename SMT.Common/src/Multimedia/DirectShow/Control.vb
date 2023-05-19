Imports System
Imports System.Runtime.InteropServices

Namespace Multimedia.DirectShow

    <ComVisible(True), ComImport(), Guid("56a868b1-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsDual)> _
    Public Interface IMediaControl

        <PreserveSig()> _
        Function Run() As Integer

        <PreserveSig()> _
        Function Pause() As Integer

        <PreserveSig()> _
        Function [Stop]() As Integer

        <PreserveSig()> _
        Function GetState(ByVal msTimeout As Integer, ByRef pfs As Integer) As Integer

        <PreserveSig()> _
        Function RenderFile(ByVal strFilename As String) As Integer

        <PreserveSig()> _
        Function AddSourceFilter( _
            <[In]()> ByVal strFilename As String, _
            <Out(), MarshalAs(UnmanagedType.IDispatch)> _
            ByRef ppUnk As Object) As Integer

        <PreserveSig()> _
        Function get_FilterCollection(<Out(), MarshalAs(UnmanagedType.IDispatch)> ByRef ppUnk As Object) As Integer

        <PreserveSig()> _
        Function get_RegFilterCollection(<Out(), MarshalAs(UnmanagedType.IDispatch)> ByRef ppUnk As Object) As Integer

        <PreserveSig()> _
        Function StopWhenReady() As Integer

    End Interface

    Public Enum eMediaControlState
        Stopped = 0
        Paused = 1
        Running = 2
    End Enum

    <ComVisible(True), ComImport(), Guid("56a868b6-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsDual)> _
    Public Interface IMediaEvent

        <PreserveSig()> _
        Function GetEventHandle(ByRef hEvent As IntPtr) As Integer

        <PreserveSig()> _
        Function GetEvent(ByRef lEventCode As DsEvCode, ByRef lParam1 As Integer, ByRef lParam2 As Integer, ByVal msTimeout As Integer) As Integer

        <PreserveSig()> _
        Function WaitForCompletion(ByVal msTimeout As Integer, ByRef pEvCode As Integer) As Integer

        <PreserveSig()> _
        Function CancelDefaultHandling(ByVal lEvCode As Integer) As Integer

        <PreserveSig()> _
        Function RestoreDefaultHandling(ByVal lEvCode As Integer) As Integer

        <PreserveSig()> _
        Function FreeEventParams(ByVal lEvCode As DsEvCode, ByVal lParam1 As Integer, ByVal lParam2 As Integer) As Integer
    End Interface

    <ComVisible(True), ComImport(), Guid("56a868c0-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsDual)> _
    Public Interface IMediaEventEx

        <PreserveSig()> _
        Function GetEventHandle(ByRef hEvent As IntPtr) As Integer

        <PreserveSig()> _
        Function GetEvent(ByRef lEventCode As DsEvCode, ByRef lParam1 As Integer, ByRef lParam2 As Integer, ByVal msTimeout As Integer) As Integer

        <PreserveSig()> _
        Function WaitForCompletion(ByVal msTimeout As Integer, <Out()> _
     ByRef pEvCode As Integer) As Integer

        <PreserveSig()> _
        Function CancelDefaultHandling(ByVal lEvCode As Integer) As Integer

        <PreserveSig()> _
        Function RestoreDefaultHandling(ByVal lEvCode As Integer) As Integer

        <PreserveSig()> _
        Function FreeEventParams(ByVal lEvCode As DsEvCode, ByVal lParam1 As Integer, ByVal lParam2 As Integer) As Integer

        <PreserveSig()> _
        Function SetNotifyWindow(ByVal hwnd As IntPtr, ByVal lMsg As Integer, ByVal lInstanceData As IntPtr) As Integer

        <PreserveSig()> _
        Function SetNotifyFlags(ByVal lNoNotifyFlags As Integer) As Integer

        <PreserveSig()> _
        Function GetNotifyFlags(ByRef lplNoNotifyFlags As Integer) As Integer
    End Interface

    <ComVisible(True), ComImport(), Guid("329bb360-f6ea-11d1-9038-00a0c9697298"), InterfaceType(ComInterfaceType.InterfaceIsDual)> _
    Public Interface IBasicVideo2

        <PreserveSig()> _
        Function AvgTimePerFrame(ByRef pAvgTimePerFrame As Double) As Integer

        <PreserveSig()> _
        Function BitRate(ByRef pBitRate As Integer) As Integer

        <PreserveSig()> _
        Function BitErrorRate(ByRef pBitRate As Integer) As Integer

        <PreserveSig()> _
        Function VideoWidth(ByRef pVideoWidth As Integer) As Integer

        <PreserveSig()> _
        Function VideoHeight(ByRef pVideoHeight As Integer) As Integer

        <PreserveSig()> _
        Function put_SourceLeft(ByVal SourceLeft As Integer) As Integer

        <PreserveSig()> _
        Function get_SourceLeft(ByRef pSourceLeft As Integer) As Integer

        <PreserveSig()> _
        Function put_SourceWidth(ByVal SourceWidth As Integer) As Integer

        <PreserveSig()> _
        Function get_SourceWidth(ByRef pSourceWidth As Integer) As Integer

        <PreserveSig()> _
        Function put_SourceTop(ByVal SourceTop As Integer) As Integer

        <PreserveSig()> _
        Function get_SourceTop(ByRef pSourceTop As Integer) As Integer

        <PreserveSig()> _
        Function put_SourceHeight(ByVal SourceHeight As Integer) As Integer

        <PreserveSig()> _
        Function get_SourceHeight(ByRef pSourceHeight As Integer) As Integer

        <PreserveSig()> _
        Function put_DestinationLeft(ByVal DestinationLeft As Integer) As Integer

        <PreserveSig()> _
        Function get_DestinationLeft(ByRef pDestinationLeft As Integer) As Integer

        <PreserveSig()> _
        Function put_DestinationWidth(ByVal DestinationWidth As Integer) As Integer

        <PreserveSig()> _
        Function get_DestinationWidth(ByRef pDestinationWidth As Integer) As Integer

        <PreserveSig()> _
        Function put_DestinationTop(ByVal DestinationTop As Integer) As Integer

        <PreserveSig()> _
        Function get_DestinationTop(ByRef pDestinationTop As Integer) As Integer

        <PreserveSig()> _
        Function put_DestinationHeight(ByVal DestinationHeight As Integer) As Integer

        <PreserveSig()> _
        Function get_DestinationHeight(ByRef pDestinationHeight As Integer) As Integer

        <PreserveSig()> _
        Function SetSourcePosition(ByVal left As Integer, ByVal top As Integer, ByVal width As Integer, ByVal height As Integer) As Integer

        <PreserveSig()> _
        Function GetSourcePosition(ByRef left As Integer, ByRef top As Integer, ByRef width As Integer, ByRef height As Integer) As Integer

        <PreserveSig()> _
        Function SetDefaultSourcePosition() As Integer

        <PreserveSig()> _
        Function SetDestinationPosition(ByVal left As Integer, ByVal top As Integer, ByVal width As Integer, ByVal height As Integer) As Integer

        <PreserveSig()> _
        Function GetDestinationPosition(ByRef left As Integer, ByRef top As Integer, ByRef width As Integer, ByRef height As Integer) As Integer

        <PreserveSig()> _
        Function SetDefaultDestinationPosition() As Integer

        <PreserveSig()> _
        Function GetVideoSize(ByRef pWidth As Integer, ByRef pHeight As Integer) As Integer

        <PreserveSig()> _
        Function GetVideoPaletteEntries(ByVal StartIndex As Integer, ByVal Entries As Integer, ByRef pRetrieved As Integer, ByVal pPalette As IntPtr) As Integer

        <PreserveSig()> _
        Function GetCurrentImage(ByRef pBufferSize As Integer, ByVal pDIBImage As IntPtr) As Integer

        <PreserveSig()> _
        Function IsUsingDefaultSource() As Integer

        <PreserveSig()> _
        Function IsUsingDefaultDestination() As Integer

        <PreserveSig()> _
        Function GetPreferredAspectRatio(ByRef plAspectX As Integer, ByRef plAspectY As Integer) As Integer
    End Interface

    Public Enum OABool
        [False] = 0
        [True] = -1
    End Enum

    <ComVisible(True), ComImport(), Guid("56a868b4-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsDual)> _
    Public Interface IVideoWindow

        <PreserveSig()> _
        Function put_Caption(ByVal caption As String) As Integer

        <PreserveSig()> _
        Function get_Caption(<Out()> ByRef caption As String) As Integer

        <PreserveSig()> _
        Function put_WindowStyle(ByVal windowStyle As Integer) As Integer

        <PreserveSig()> _
        Function get_WindowStyle(ByRef windowStyle As Integer) As Integer

        <PreserveSig()> _
        Function put_WindowStyleEx(ByVal windowStyleEx As Integer) As Integer

        <PreserveSig()> _
        Function get_WindowStyleEx(ByRef windowStyleEx As Integer) As Integer

        <PreserveSig()> _
        Function put_AutoShow(ByVal autoShow As Integer) As Integer

        <PreserveSig()> _
        Function get_AutoShow(ByRef autoShow As Integer) As Integer

        <PreserveSig()> _
        Function put_WindowState(ByVal windowState As Integer) As Integer

        <PreserveSig()> _
        Function get_WindowState(ByRef windowState As Integer) As Integer

        <PreserveSig()> _
        Function put_BackgroundPalette(ByVal backgroundPalette As Integer) As Integer

        <PreserveSig()> _
        Function get_BackgroundPalette(ByRef backgroundPalette As Integer) As Integer

        <PreserveSig()> _
        Function put_Visible(ByVal visible As Integer) As Integer

        <PreserveSig()> _
        Function get_Visible(ByRef visible As Integer) As Integer

        <PreserveSig()> _
        Function put_Left(ByVal left As Integer) As Integer

        <PreserveSig()> _
        Function get_Left(ByRef left As Integer) As Integer

        <PreserveSig()> _
        Function put_Width(ByVal width As Integer) As Integer

        <PreserveSig()> _
        Function get_Width(ByRef width As Integer) As Integer

        <PreserveSig()> _
        Function put_Top(ByVal top As Integer) As Integer

        <PreserveSig()> _
        Function get_Top(ByRef top As Integer) As Integer

        <PreserveSig()> _
        Function put_Height(ByVal height As Integer) As Integer

        <PreserveSig()> _
        Function get_Height(ByRef height As Integer) As Integer

        <PreserveSig()> _
        Function put_Owner(ByVal owner As IntPtr) As Integer

        <PreserveSig()> _
        Function get_Owner(ByRef owner As IntPtr) As Integer

        <PreserveSig()> _
        Function put_MessageDrain(ByVal drain As IntPtr) As Integer

        <PreserveSig()> _
        Function get_MessageDrain(ByRef drain As IntPtr) As Integer

        <PreserveSig()> _
        Function get_BorderColor(ByRef color As Integer) As Integer

        <PreserveSig()> _
        Function put_BorderColor(ByVal color As Integer) As Integer

        <PreserveSig()> _
        Function get_FullScreenMode(<Out()> ByRef fullScreenMode As OABool) As Integer

        <PreserveSig()> _
        Function put_FullScreenMode(<[In]()> ByVal fullScreenMode As OABool) As Integer

        <PreserveSig()> _
        Function SetWindowForeground(ByVal focus As Integer) As Integer

        <PreserveSig()> _
        Function NotifyOwnerMessage(ByVal hwnd As IntPtr, ByVal msg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr) As Integer

        <PreserveSig()> _
        Function SetWindowPosition(ByVal left As Integer, ByVal top As Integer, ByVal width As Integer, ByVal height As Integer) As Integer

        <PreserveSig()> _
        Function GetWindowPosition(ByRef left As Integer, ByRef top As Integer, ByRef width As Integer, ByRef height As Integer) As Integer

        <PreserveSig()> _
        Function GetMinIdealImageSize(ByRef width As Integer, ByRef height As Integer) As Integer

        <PreserveSig()> _
        Function GetMaxIdealImageSize(ByRef width As Integer, ByRef height As Integer) As Integer

        <PreserveSig()> _
        Function GetRestorePosition(ByRef left As Integer, ByRef top As Integer, ByRef width As Integer, ByRef height As Integer) As Integer

        <PreserveSig()> _
        Function HideCursor(ByVal pHideCursor As Integer) As Integer

        <PreserveSig()> _
        Function IsCursorHidden(ByRef hideCursor As Integer) As Integer
    End Interface

    <ComVisible(True), ComImport(), Guid("56a868b2-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsDual)> _
    Public Interface IMediaPosition

        <PreserveSig()> _
        Function get_Duration(ByRef pLength As Double) As Integer

        <PreserveSig()> _
        Function put_CurrentPosition(ByVal llTime As Double) As Integer

        <PreserveSig()> _
        Function get_CurrentPosition(ByRef pllTime As Double) As Integer

        <PreserveSig()> _
        Function get_StopTime(ByRef pllTime As Double) As Integer

        <PreserveSig()> _
        Function put_StopTime(ByVal llTime As Double) As Integer

        <PreserveSig()> _
        Function get_PrerollTime(ByRef pllTime As Double) As Integer

        <PreserveSig()> _
        Function put_PrerollTime(ByVal llTime As Double) As Integer

        <PreserveSig()> _
        Function put_Rate(ByVal dRate As Double) As Integer

        <PreserveSig()> _
        Function get_Rate(ByRef pdRate As Double) As Integer

        <PreserveSig()> _
        Function CanSeekForward(ByRef pCanSeekForward As Integer) As Integer

        <PreserveSig()> _
        Function CanSeekBackward(ByRef pCanSeekBackward As Integer) As Integer
    End Interface

    <ComVisible(True), ComImport(), Guid("56a868b3-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsDual)> _
    Public Interface IBasicAudio

        <PreserveSig()> _
        Function put_Volume(ByVal lVolume As Integer) As Integer

        <PreserveSig()> _
        Function get_Volume(ByRef plVolume As Integer) As Integer

        <PreserveSig()> _
        Function put_Balance(ByVal lBalance As Integer) As Integer

        <PreserveSig()> _
        Function get_Balance(ByRef plBalance As Integer) As Integer
    End Interface

    <ComVisible(True), ComImport(), Guid("56a868b9-0ad4-11ce-b03a-0020af0ba770"), InterfaceType(ComInterfaceType.InterfaceIsDual)> _
    Public Interface IAMCollection

        <PreserveSig()> _
        Function get_Count(ByRef plCount As Integer) As Integer

        <PreserveSig()> _
        Function Item(ByVal lItem As Integer, <Out(), MarshalAs(UnmanagedType.IUnknown)> _
     ByRef ppUnk As Object) As Integer

        <PreserveSig()> _
        Function get_NewEnum(<Out(), MarshalAs(UnmanagedType.IUnknown)> _
     ByRef ppUnk As Object) As Integer
    End Interface

    Public Enum DsEvCode

        ' FROM evcode.h

        EC_COMPLETE = &H1
        ' ( HRESULT, void ) : defaulted (special)
        ' Signals the completed playback of a stream within the graph.  This message
        ' is sent by renderers when they receive end-of-stream.  The default handling
        ' of this message results in a _SINGLE_ EC_COMPLETE being sent to the
        ' application when ALL of the individual renderers have signaled EC_COMPLETE
        ' to the filter graph.  If the default handing is canceled, the application
        ' will see all of the individual EC_COMPLETEs.


        EC_USERABORT = &H2
        ' ( void, void ) : application
        ' In some sense, the user has requested that playback be terminated.
        ' This message is typically sent by renderers that render into a
        ' window if the user closes the window into which it was rendering.
        ' It is up to the application to decide if playback should actually
        ' be stopped.


        EC_ERRORABORT = &H3
        ' ( HRESULT, void ) : application
        ' Operation aborted because of error


        EC_TIME = &H4
        ' ( DWORD, DWORD ) : application
        ' The requested reference time occurred.  (This event is currently not used).
        ' lParam1 is low dword of ref time, lParam2 is high dword of reftime.


        EC_REPAINT = &H5
        ' ( IPin * (could be NULL), void ) : defaulted
        ' A repaint is required - lParam1 contains the (IPin *) that needs the data
        ' to be sent again. Default handling is: if the output pin which the IPin is
        ' attached  to supports the IMediaEventSink interface then it will be called
        ' with the EC_REPAINT first.  If that fails then normal repaint processing is
        ' done by the filter graph.


        ' Stream error notifications
        EC_STREAM_ERROR_STOPPED = &H6
        EC_STREAM_ERROR_STILLPLAYING = &H7
        ' ( HRESULT, DWORD ) : application
        ' lParam 1 is major code, lParam2 is minor code, either may be zero.


        EC_ERROR_STILLPLAYING = &H8
        ' ( HRESULT, void ) : application
        ' The filter graph manager may issue Run's to the graph asynchronously.
        ' If such a Run fails, EC_ERROR_STILLPLAYING is issued to notify the
        ' application of the failure.  The state of the underlying filters
        ' at such a time will be indeterminate - they will all have been asked
        ' to run, but some are almost certainly not.


        EC_PALETTE_CHANGED = &H9
        ' ( void, void ) : application
        ' notify application that the video palette has changed


        EC_VIDEO_SIZE_CHANGED = &HA
        ' ( DWORD, void ) : application
        ' Sent by video renderers.
        ' Notifies the application that the native video size has changed.
        ' LOWORD of the DWORD is the new width, HIWORD is the new height.


        EC_QUALITY_CHANGE = &HB
        ' ( void, void ) : application
        ' Notify application that playback degradation has occurred


        EC_SHUTTING_DOWN = &HC
        ' ( void, void ) : internal
        ' This message is sent by the filter graph manager to any plug-in
        ' distributors which support IMediaEventSink to notify them that
        ' the filter graph is starting to shutdown.


        EC_CLOCK_CHANGED = &HD
        ' ( void, void ) : application
        ' Notify application that the clock has changed.
        ' (i.e. SetSyncSource has been called on the filter graph and has been
        ' distributed successfully to the filters in the graph.)


        EC_PAUSED = &HE
        ' ( HRESULT, void ) : application
        ' Notify application the previous pause request has completed


        EC_OPENING_FILE = &H10
        EC_BUFFERING_DATA = &H11
        ' ( BOOL, void ) : application
        ' lParam1 == 1   --> starting to open file or buffer data
        ' lParam1 == 0   --> not opening or buffering any more
        ' (This event does not appear to be used by ActiveMovie.)


        EC_FULLSCREEN_LOST = &H12
        ' ( void, IBaseFilter * ) : application
        ' Sent by full screen renderers when switched away from full screen.
        ' IBaseFilter may be NULL.


        EC_ACTIVATE = &H13
        ' ( BOOL, IBaseFilter * ) : internal
        ' Sent by video renderers when they lose or gain activation.
        ' lParam1 is set to 1 if gained or 0 if lost
        ' lParam2 is the IBaseFilter* for the filter that is sending the message
        ' Used for sound follows focus and full-screen switching


        EC_NEED_RESTART = &H14
        ' ( void, void ) : defaulted
        ' Sent by renderers when they regain a resource (e.g. audio renderer).
        ' Causes a restart by Pause/put_Current/Run (if running).


        EC_WINDOW_DESTROYED = &H15
        ' ( IBaseFilter *, void ) : internal
        ' Sent by video renderers when the window has been destroyed. Handled
        ' by the filter graph / distributor telling the resource manager.
        ' lParam1 is the IBaseFilter* of the filter whose window is being destroyed


        EC_DISPLAY_CHANGED = &H16
        ' ( IPin *, void ) : internal
        ' Sent by renderers when they detect a display change. the filter graph
        ' will arrange for the graph to be stopped and the pin send in lParam1
        ' to be reconnected. by being reconnected it allows a renderer to reset
        ' and connect with a more appropriate format for the new display mode
        ' lParam1 contains an (IPin *) that should be reconnected by the graph


        EC_STARVATION = &H17
        ' ( void, void ) : defaulted
        ' Sent by a filter when it detects starvation. Default handling (only when
        ' running) is for the graph to be paused until all filters enter the
        ' paused state and then run. Normally this would be sent by a parser or source
        ' filter when too little data is arriving.


        EC_OLE_EVENT = &H18
        ' ( BSTR, BSTR ) : application
        ' Sent by a filter to pass a text string to the application.
        ' Conventionally, the first string is a type, and the second a parameter.


        EC_NOTIFY_WINDOW = &H19
        ' ( HWND, void ) : internal
        ' Pass the window handle around during pin connection.

        EC_STREAM_CONTROL_STOPPED = &H1A
        ' ( IPin * pSender, DWORD dwCookie )
        ' Notification that an earlier call to IAMStreamControl::StopAt
        ' has now take effect.  Calls to the method can be marked
        ' with a cookie which is passed back in the second parameter,
        ' allowing applications to easily tie together request
        ' and completion notifications.
        '
        ' NB: IPin will point to the pin that actioned the Stop.  This
        ' may not be the pin that the StopAt was sent to.

        EC_STREAM_CONTROL_STARTED = &H1B
        ' ( IPin * pSender, DWORD dwCookie )
        ' Notification that an earlier call to IAMStreamControl::StartAt
        ' has now take effect.  Calls to the method can be marked
        ' with a cookie which is passed back in the second parameter,
        ' allowing applications to easily tie together request
        ' and completion notifications.
        '
        ' NB: IPin will point to the pin that actioned the Start.  This
        ' may not be the pin that the StartAt was sent to.

        EC_END_OF_SEGMENT = &H1C
        '
        ' ( const REFERENCE_TIME *pStreamTimeAtEndOfSegment, DWORD dwSegmentNumber )
        '
        ' pStreamTimeAtEndOfSegment
        '     pointer to the accumulated stream clock
        '     time since the start of the segment - this is directly computable
        '     as the sum of the previous and current segment durations (Stop - Start)
        '     and the rate applied to each segment
        '     The source add this time to the time within each segment to get
        '     a total elapsed time
        '
        ' dwSegmentNumber
        '     Segment number - starts at 0
        '
        ' Notifies that a segment end has been reached when the
        ' AM_SEEKING_Segment flags was set for IMediaSeeking::SetPositions
        ' Passes in an IMediaSeeking interface to allow the next segment
        ' to be defined by the application

        EC_SEGMENT_STARTED = &H1D
        '
        ' ( const REFERENCE_TIME *pStreamTimeAtStartOfSegment, DWORD dwSegmentNumber)
        '
        ' pStreamTimeAtStartOfSegment
        '     pointer to the accumulated stream clock
        '     time since the start of the segment - this is directly computable
        '     as the sum of the previous segment durations (Stop - Start)
        '     and the rate applied to each segment
        '
        ' dwSegmentNumber
        '     Segment number - starts at 0
        '
        ' Notifies that a new segment has been started.
        ' This is sent synchronously by any entity that will issue
        ' EC_END_OF_SEGMENT when a new segment is started
        ' (See IMediaSeeking::SetPositions - AM_SEEKING_Segment flag)
        ' It is used to compute how many EC_END_OF_SEGMENT notifications
        ' to expect at the end of a segment and as a consitency check


        EC_LENGTH_CHANGED = &H1E
        ' (void, void)
        ' sent to indicate that the length of the "file" has changed

        EC_DEVICE_LOST = &H1F
        ' (IUnknown, 0)
        '
        ' request window notification when the device is available again
        ' (through WM_DEVICECHANGED messages registered with
        ' RegisterDeviceNotification; see IAMDeviceRemoval interface)

        EC_STEP_COMPLETE = &H24
        ' (BOOL bCacelled, void)
        ' Step request complete
        ' if bCancelled is TRUE the step was cancelled.  This can happen
        ' if the application issued some control request or because there
        ' was a mode change etc etc

        ' Event code 25 is reserved for future use.

        EC_TIMECODE_AVAILABLE = &H30
        ' Sent by filter supporting timecode
        ' Param1 has a pointer to the sending object
        ' Param2 has the device ID of the sending object

        EC_EXTDEVICE_MODE_CHANGE = &H31
        ' Sent by filter supporting IAMExtDevice
        ' Param1 has the new mode
        ' Param2 has the device ID of the sending object

        EC_STATE_CHANGE = &H32
        ' ( FILTER_STATE, BOOL bInternal)
        ' Used to notify the application of any state changes in the filter graph.
        ' lParam1  is of type enum FILTER_STATE (defined in strmif.h) and indicates
        '          the state of the filter graph.
        '
        ' lParam2 == 0 indicates that the previous state change request has completed
        '              & a change in application state.
        ' lParam2 == 1 reserved for future use to indicate internal state changes.


        EC_GRAPH_CHANGED = &H50
        ' Sent by filter to notify interesting graph changes

        EC_CLOCK_UNSET = &H51
        ' ( void, void ) : application
        ' Used to notify the filter graph to unset the current graph clock.
        ' Has the affect of forcing the filter graph to reestablish the graph clock
        ' on the next Pause/Run (note that this is only used by ksproxy, when the pin
        ' of a clock providing filter is disconnected)

        EC_VMR_RENDERDEVICE_SET = &H53
        ' (Render_Device type, void)
        ' Identifies the type of rendering mechanism the VMR
        ' is using to display video.  Types used include:
        'VMR_RENDER_DEVICE_OVERLAY = &H1
        'VMR_RENDER_DEVICE_VIDMEM = &H2
        'VMR_RENDER_DEVICE_SYSMEM = &H4


        EC_VMR_SURFACE_FLIPPED = &H54
        ' (hr - Flip return code, void)
        ' Identifies the VMR's allocator-presenter has called the DDraw flip api on
        ' the surface being presented.   This allows the VMR to keep its DX-VA table
        ' of DDraw surfaces in sync with DDraws flipping chain.

        EC_VMR_RECONNECTION_FAILED = &H55
        ' (hr - ReceiveConnection return code, void)
        ' Identifies that an upstream decoder tried to perform a dynamic format
        ' change and the VMR was unable to accept the new format.


        EC_PREPROCESS_COMPLETE = &H56
        ' Sent by the WM ASF writer filter (WMSDK V9 version) to signal the completion 
        ' of a pre-process run when running in multipass encode mode. 
        ' Param1 = 0, Param2 = IBaseFilter ptr of sending filter

        EC_CODECAPI_EVENT = &H57
        ' Sent by the Codec API when an event is encountered.  Both the Data
        ' must be freed by the recipient using CoTaskMemFree
        ' Param1 = UserDataPointer, Param2 = VOID* Data



        ' FROM dvdevcod.h
        ' DVD-Video event codes
        ' ======================
        ' All DVD-Video event are always passed on to the application, and are 
        ' never processed by the filter graph

        ''' <summary>
        ''' Parameters: ( DWORD, void  
        ''' lParam1 is enum DVD_DOMAIN, and indicates the player'''s new domain
        ''' Raised from following domains: all
        ''' Signaled when ever the DVD player changes domains.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_DOMAIN_CHANGE = 256 + &H1

        ''' <summary>
        ''' Parameters: ( DWORD, void  
        ''' lParam1 is the new title number.
        '''
        ''' Raised from following domains: DVD_DOMAIN_Title
        '''
        ''' Indicates when the current title number changes.  Title numbers
        ''' range 1 to 99.  This indicates the TTN, which is the title number
        ''' with respect to the whole disc, not the VTS_TTN which is the title
        ''' number with respect to just a current VTS.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_TITLE_CHANGE = 256 + &H2

        ''' <summary>
        ''' Parameters: ( DWORD, void  
        ''' lParam1 is the new chapter number (which is the program number for 
        ''' One_Sequential_PGC_Titles.
        '''
        ''' Raised from following domains: DVD_DOMAIN_Title
        '''
        ''' Signales that DVD player started playback of a new program in the Title 
        ''' domain.  This is only signaled for One_Sequential_PGC_Titles.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_CHAPTER_START = 256 + &H3

        ''' <summary>
        ''' Parameters: ( DWORD, void  
        ''' lParam1 is the new user audio stream number.
        '''
        ''' Raised from following domains: all
        '''
        ''' Signaled when ever the current user audio stream number changes for the main 
        ''' title.  This can be changed automatically with a navigation command on disc
        ''' as well as through IDVDAnnexJ.
        ''' Audio stream numbers range from 0 to 7.  Stream &hffffffff
        ''' indicates that no stream is selected.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_AUDIO_STREAM_CHANGE = 256 + &H4

        ''' <summary>
        ''' Parameters: ( DWORD, BOOL  
        ''' lParam1 is the new user subpicture stream number.
        ''' lParam2 is the subpicture'''s on/off state (TRUE if on
        '''
        ''' Raised from following domains: all
        '''
        ''' Signaled when ever the current user subpicture stream number changes for the main 
        ''' title.  This can be changed automatically with a navigation command on disc
        ''' as well as through IDVDAnnexJ.  
        ''' Subpicture stream numbers range from 0 to 31.  Stream &hffffffff
        ''' indicates that no stream is selected.  
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_SUBPICTURE_STREAM_CHANGE = 256 + &H5

        ''' <summary>
        ''' Parameters: ( DWORD, DWORD  
        ''' lParam1 is the number of available angles.
        ''' lParam2 is the current user angle number.
        '''
        ''' Raised from following domains: all
        '''
        ''' Signaled when ever either 
        '''   a the number of available angles changes, or  
        '''   b the current user angle number changes.
        ''' Current angle number can be changed automatically with navigation command 
        ''' on disc as well as through IDVDAnnexJ.
        ''' When the number of available angles is 1, the current video is not multiangle.
        ''' Angle numbers range from 1 to 9.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_ANGLE_CHANGE = 256 + &H6

        ''' <summary>
        ''' Parameters: ( DWORD, DWORD  
        ''' lParam1 is the number of available buttons.
        ''' lParam2 is the current selected button number.
        '''
        ''' Raised from following domains: all
        '''
        ''' Signaled when ever either 
        '''   a the number of available buttons changes, or  
        '''   b the current selected button number changes.
        ''' The current selected button can be changed automatically with navigation 
        ''' commands on disc as well as through IDVDAnnexJ.  
        ''' Button numbers range from 1 to 36.  Selected button number 0 implies that
        ''' no button is selected.  Note that these button numbers enumerate all 
        ''' available button numbers, and do not always correspond to button numbers
        ''' used for IDVDAnnexJ::ButtonSelectAndActivate since only a subset of buttons
        ''' may be activated with ButtonSelectAndActivate.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_BUTTON_CHANGE = 256 + &H7

        ''' <summary>
        ''' Parameters: ( DWORD, void  
        ''' lParam1 is a VALID_UOP_SOMTHING_OR_OTHER bit-field stuct which indicates
        '''   which IDVDAnnexJ commands are explicitly disable by the DVD disc.
        '''
        ''' Raised from following domains: all
        '''
        ''' Signaled when ever the available set of IDVDAnnexJ methods changes.  This
        ''' only indicates which operations are explicited disabled by the content on 
        ''' the DVD disc, and does not guarentee that it is valid to call methods 
        ''' which are not disabled.  For example, if no buttons are currently present,
        ''' IDVDAnnexJ::ButtonActivate( won'''t work, even though the buttons are not
        ''' explicitly disabled. 
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_VALID_UOPS_CHANGE = 256 + &H8

        ''' <summary>
        ''' Parameters: ( BOOL, DWORD  
        ''' lParam1 == 0  -->  buttons are available, so StillOff won'''t work
        ''' lParam1 == 1  -->  no buttons available, so StillOff will work
        ''' lParam2 indicates the number of seconds the still will last, with &hffffffff 
        '''   indicating an infinite still (wait till button or StillOff selected.
        '''
        ''' Raised from following domains: all
        '''
        ''' Signaled at the beginning of any still: PGC still, Cell Still, or VOBU Still.
        ''' Note that all combinations of buttons and still are possible (buttons on with
        ''' still on, buttons on with still off, button off with still on, button off
        ''' with still off.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_STILL_ON = 256 + &H9

        ''' <summary>
        ''' Parameters: ( void, void  
        '''
        '''   Indicating that any still that is currently active
        '''   has been released.
        '''
        ''' Raised from following domains: all
        '''
        ''' Signaled at the end of any still: PGC still, Cell Still, or VOBU Still.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_STILL_OFF = 256 + &HA

        ''' <summary>
        ''' Parameters: ( DWORD, BOOL  
        ''' lParam1 is a DVD_TIMECODE which indicates the current 
        '''   playback time code in a BCD HH:MM:SS:FF format.
        ''' lParam2 == 0  -->  time code is 25 frames/sec
        ''' lParam2 == 1  -->  time code is 30 frames/sec (non-drop.
        ''' lParam2 == 2  -->  time code is invalid (current playback time 
        '''                    cannot be determined for current title
        '''
        ''' Raised from following domains: DVD_DOMAIN_Title
        '''
        ''' Signaled at the beginning of every VOBU, which occurs every .4 to 1.0 sec.
        ''' This is only signaled for One_Sequential_PGC_Titles.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_CURRENT_TIME = 256 + &HB

        ''' <summary>
        ''' Parameters: ( DWORD, void 
        ''' lParam1 is an enum DVD_ERROR which notifies the app of some error condition.
        '''
        ''' Raised from following domains: all
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_ERROR = 256 + &HC

        ''' <summary>
        ''' Parameters: ( DWORD, DWORD 
        ''' lParam1 is an enum DVD_WARNING which notifies the app of some warning condition.
        ''' lParam2 contains more specific information about the warning (warning dependent
        '''
        ''' Raised from following domains: all
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_WARNING = 256 + &HD

        ''' <summary>
        ''' Parameters: (BOOL, void
        ''' lParam1 is a BOOL which indicates the reason for the cancellation of ChapterPlayAutoStop
        ''' lParam1 == 0 indicates successful completion of ChapterPlayAutoStop
        ''' lParam1 == 1 indicates that ChapterPlayAutoStop is being cancelled as a result of another
        '''            IDVDControl call or the end of content has been reached & no more chapters
        '''            can be played.
        '''  Indicating that playback is stopped as a result of a call
        '''  to IDVDControl::ChapterPlayAutoStop(
        '''
        ''' Raised from following domains : DVD_DOMAIN_TITLE
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_CHAPTER_AUTOSTOP = 256 + &HE

        ''' <summary>
        '''  Parameters : (void, void
        '''
        '''  Raised from the following domains : FP_DOM
        '''
        '''  Indicates that the DVD disc does not have a FP_PGC (First Play Program Chain
        '''  and the DVD Navigator will not automatically load any PGC and start playback.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_NO_FP_PGC = 256 + &HF

        ''' <summary>
        '''  Parameters : (LONG, void
        '''  lParam1 is a LONG indicating the new playback rate.
        '''  lParam1 < 0 indicates reverse playback mode.
        '''  lParam1 > 0 indicates forward playback mode
        '''  Value of lParam1 is the actual playback rate multiplied by 10000.
        '''  i.e. lParam1 = rate * 10000
        '''
        '''  Raised from the following domains : TT_DOM
        '''
        '''  Indicates that a rate change in playback has been initiated and the parameter
        '''  lParam1 indicates the new playback rate that is being used.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_PLAYBACK_RATE_CHANGE = 256 + &H10

        ''' <summary>
        '''  Parameters : (LONG, void
        '''  lParam1 is a LONG indicating the new parental level.
        '''
        '''  Raised from the following domains : VMGM_DOM
        '''
        '''  Indicates that an authored Nav command has changed the parental level
        '''  setting in the player.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_PARENTAL_LEVEL_CHANGE = 256 + &H11

        ''' <summary>
        '''  Parameters : (DWORD, void
        '''
        '''  Raised from the following domains : All Domains
        '''
        ''' Indicates that playback has been stopped as the Navigator has completed
        ''' playback of the pgc and did not find any other branching instruction for
        ''' subsequent playback.
        '''
        '''  The DWORD returns the reason for the completion of the playback.  See
        '''  The DVD_PB_STOPPED enumeration for details.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_PLAYBACK_STOPPED = 256 + &H12

        ''' <summary>
        '''  Parameters : (BOOL, void
        '''  lParam1 == 0 indicates that playback is not in an angle block and angles are
        '''             not available
        '''  lParam1 == 1 indicates that an angle block is being played back and angle changes
        '''             can be performed.
        '''
        '''  Indicates whether an angle block is being played and if angle changes can be 
        '''  performed. However, angle changes are not restricted to angle blocks and the
        '''  manifestation of the angle change can be seen only in an angle block.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_ANGLES_AVAILABLE = 256 + &H13

        ''' <summary>
        ''' Parameters: (void, void
        ''' Sent when the PlayPeriodInTitle completes or is cancelled
        '''
        ''' Raised from following domains : DVD_DOMAIN_TITLE
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_PLAYPERIOD_AUTOSTOP = 256 + &H14

        ''' <summary>
        ''' Parameters: (DWORD button, void
        ''' Sent when a button is automatically activated
        '''
        ''' Raised from following domains : DVD_DOMAIN_MENU
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_BUTTON_AUTO_ACTIVATED = 256 + &H15

        ''' <summary>
        ''' Parameters: (CmdID, HRESULT
        ''' Sent when a command begins
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_CMD_START = 256 + &H16

        ''' <summary>
        ''' Parameters: (CmdID, HRESULT
        ''' Sent when a command completes
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_CMD_END = 256 + &H17

        ''' <summary>
        ''' Parameters: none
        ''' Sent when the nav detects that a disc was ejected and stops the playback
        ''' The app does not need to take any action to stop the playback.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_DISC_EJECTED = 256 + &H18

        ''' <summary>
        ''' Parameters: none
        ''' Sent when the nav detects that a disc was inserted and the nav begins playback
        ''' The app does not need to take any action to start the playback.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_DISC_INSERTED = 256 + &H19

        ''' <summary>
        ''' Parameters: ( ULONG, ULONG  
        ''' lParam2 contains a union of the DVD_TIMECODE_FLAGS
        ''' lParam1 contains a DVD_HMSF_TIMECODE.  Assign lParam1 to a ULONG then cast the
        ''' ULONG as a DVD_HMSF_TIMECODE to use its values.
        '''
        ''' Raised from following domains: DVD_DOMAIN_Title
        '''
        ''' Signaled at the beginning of every VOBU, which occurs every .4 to 1.0 sec.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_CURRENT_HMSF_TIME = 256 + &H1A

        ''' <summary>
        ''' Parameters: ( BOOL, reserved  
        ''' lParam1 is either TRUE (a karaoke track is being played or FALSE (no karaoke data is being played.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_KARAOKE_MODE = 256 + &H1B

        ''' <summary>
        ''' Parameters: ( ULONG, ULONG 
        ''' Sent when current program ID and/or cell ID change
        ''' lParam1 contains the new Program ID
        ''' lParam2 contains the new Cell ID
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_PROGRAM_CELL_CHANGE = 256 + &H1C

        ''' <summary>
        ''' Parameters: ( BYTE, void 
        ''' Sent when current VTS (Video Title Set changes
        ''' lParam1 contains the new VTSN (Video Title Set Number
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_TITLE_SET_CHANGE = 256 + &H1D

        ''' <summary>
        ''' Parameters: ( WORD, void 
        ''' Sent when current PGC (Program Chain changes
        ''' lParam1 contains the new PGCN (Program Chain Number
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_PROGRAM_CHAIN_CHANGE = 256 + &H1E

        ''' <summary>
        ''' Parameters: ( BlockOffset, VTSN  
        ''' lParam1 is the block offset of the most recent VOBU.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_VOBU_Offset = 256 + &H1F

        ''' <summary>
        ''' Parameters: ( rtTimestamp.LowPart, rtTimeStamps.HighPart  
        ''' lParam1 is the dshow timestamp of the most recent VOBU.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_VOBU_Timestamp = 256 + &H20

        ''' <summary>
        ''' Parameters: ( GPRM index, GPRM value  
        ''' lParam1 is the GPRM index
        ''' loword(lParam2 is the new GPRM value, hiword(lParam2 is type
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_GPRM_Change = 256 + &H21

        ''' <summary>
        ''' Parameters: ( SPRM index, SPRM value  
        ''' lParam1 is the SPRM index
        ''' loword(lParam2 is the new SPRM value, hiword(lParam2 is type
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_SPRM_Change = 256 + &H22

        ''' <summary>
        ''' Parameters: ( command type, reserved  
        ''' Sent when navigation commands are starting
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_BeginNavigationCommands = 256 + &H23

        ''' <summary>
        ''' Parameters: ( cmd.LowPart, cmd.HighPart  
        ''' 64 bits of DVD navigation command.
        ''' </summary>
        ''' <remarks></remarks>
        EC_DVD_NavigationCommand = 256 + &H24

        'DvdDomChange = 257
        'DvdTitleChange = 258
        'DvdChaptStart = 259
        'DvdAudioStChange = 260
        'DvdSubPicStChange = 261
        'DvdAngleChange = 262
        'DvdButtonChange = 263
        'DvdValidUopsChange = 264
        'DvdStillOn = 265
        'DvdStillOff = 266
        'DvdCurrentTime = 267
        'DvdError = 268
        'DvdWarning = 269
        'DvdChaptAutoStop = 270
        'DvdNoFpPgc = 271
        'DvdPlaybRateChange = 272
        'DvdParentalLChange = 273
        'DvdPlaybStopped = 274
        'DvdAnglesAvail = 275
        'DvdPeriodtop = 276
        'DvdButtonAActivated = 277
        'DvdCmdStart = 278
        'DvdCmdEnd = 279
        'DvdDiscEjected = 280
        'DvdDiscInserted = 281
        'DvdCurrentHmsfTime = 282
        'DvdKaraokeMode = 283
    End Enum

End Namespace


