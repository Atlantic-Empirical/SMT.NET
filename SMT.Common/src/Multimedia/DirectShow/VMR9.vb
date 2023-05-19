Imports System.Runtime.InteropServices
Imports REFERENCE_TIME = System.Int64
Imports D3DFORMAT = System.UInt32
Imports D3DPOOL = System.UInt32
Imports D3DCOLOR = System.UInt32

Namespace Multimedia.DirectShow

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure VMR9VideoStreamInfo
        Public pddsVideoSurface As IntPtr
        Public dwWidth As System.UInt32
        Public dwHeight As System.UInt32
        Public dwStrmID As System.UInt32
        Public fAlpha As Single
        Public rNormal As VMR9NormalizedRect
        Public rtStart As REFERENCE_TIME
        Public rtEnd As REFERENCE_TIME
        Public SampleFormat As VMR9_SampleFormat
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure VMR9VideoDesc
        Public dwSize As System.UInt32
        Public dwSampleWidth As System.UInt32
        Public dwSampleHeight As System.UInt32
        Public SampleFormat As VMR9_SampleFormat
        Public dwFourCC As System.UInt32
        Public InputSampleFreq As VMR9Frequency
        Public OutputFrameFreq As VMR9Frequency
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure VMR9DeinterlaceCaps
        Public dwSize As System.UInt32
        Public dwNumPreviousOutputFrames As System.UInt32
        Public dwNumForwardRefSamples As System.UInt32
        Public dwNumBackwardRefSamples As System.UInt32
        Public DeinterlaceTechnology As VMR9DeinterlaceTech
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure VMR9Frequency
        Public dwNumerator As System.UInt32
        Public dwDenominator As System.UInt32
    End Structure

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)> _
    Public Structure VMR9MonitorInfo
        Public uDevID As System.UInt32
        Public rcMonitor As RECT
        Public hMon As IntPtr
        Public dwFlags As System.UInt32
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=32)> _
        Public szDevice As String
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=512)> _
        Public szDescription As String
        Public liDriverVersion As Long
        Public dwVendorId As System.UInt32
        Public dwDeviceId As System.UInt32
        Public dwSubSysId As System.UInt32
        Public dwRevision As System.UInt32
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure VMR9AlphaBitmap
        Public dwFlags As VMR9AlphaBitmapFlags
        Public hdc As IntPtr
        Public pDDS As IntPtr
        Public rSrc As RECT
        Public rDest As VMR9NormalizedRect
        Public fAlpha As Single
        Public clrSrcKey As Integer
        Public dwFilterMode As VMR9MixerPrefs
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure VMR9ProcAmpControl
        Public dwSize As System.UInt32
        Public dwFlags As System.UInt32
        Public Brightness As Single
        Public Contrast As Single
        Public Hue As Single
        Public Saturation As Single
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure VMR9ProcAmpControlRange
        Public dwSize As System.UInt32
        Public dwProperty As VMR9ProcAmpControlFlags
        Public MinValue As Single
        Public MaxValue As Single
        Public DefaultValue As Single
        Public StepSize As Single
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure VMR9NormalizedRect
        Public left As Single
        Public top As Single
        Public right As Single
        Public bottom As Single
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure AM_MEDIA_TYPE
        Public majortype As Guid
        Public subtype As Guid
        <MarshalAs(UnmanagedType.Bool)> _
        Public bFixedSizeSamples As Boolean
        <MarshalAs(UnmanagedType.Bool)> _
        Public bTemporalCompression As Boolean
        Public lSampleSize As System.UInt32
        Public formattype As Guid
        <MarshalAs(UnmanagedType.IUnknown)> _
        Public pUnk As Object
        Public cbFormat As System.UInt32
        Public pbFormat As IntPtr
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure SIZE
        Public cx As Integer
        Public cy As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure VMR9PresentationInfo
        Public dwFlags As System.UInt32
        Public lpSurf As IntPtr
        Public rtStart As REFERENCE_TIME
        Public rtEnd As REFERENCE_TIME
        Public szAspectRatio As SIZE
        Public rcSrc As RECT
        Public rcDst As RECT
        Public dwReserved1 As System.UInt32
        Public dwReserved2 As System.UInt32
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure VMR9AllocationInfo
        Public dwFlags As System.UInt32
        Public dwWidth As System.UInt32
        Public dwHeight As System.UInt32
        Public Format As D3DFORMAT
        Public Pool As D3DPOOL
        Public MinBuffers As System.UInt32
        Public szAspectRatio As SIZE
        Public szNativeSize As SIZE
    End Structure

    Public Enum VMR9_SampleFormat
        VMR9_SampleReserved = 1
        VMR9_SampleProgressiveFrame = 2
        VMR9_SampleFieldInterleavedEvenFirst = 3
        VMR9_SampleFieldInterleavedOddFirst = 4
        VMR9_SampleFieldSingleEven = 5
        VMR9_SampleFieldSingleOdd = 6
    End Enum

    Public Enum VMR9DeinterlacePrefs
        DeinterlacePref9_NextBest = 1
        DeinterlacePref9_BOB = 2
        DeinterlacePref9_Weave = 4
        DeinterlacePref9_Mk = 7
    End Enum

    Public Enum VMR9DeinterlaceTech
        DeinterlaceTech9_Unknown = 0
        DeinterlaceTech9_BOBLineReplicate = 1
        DeinterlaceTech9_BOBVerticalStretch = 2
        DeinterlaceTech9_MedianFiltering = 4
        DeinterlaceTech9_EdgeFiltering = 16
        DeinterlaceTech9_FieldAdaptive = 32
        DeinterlaceTech9_PixelAdaptive = 64
        DeinterlaceTech9_MotionVectorSteered = 128
    End Enum

    Public Enum VMR9Mode
        VMR9Mode_Windowed = 1
        VMR9Mode_Windowless = 2
        VMR9Mode_Renderless = 4
        VMR9Mode_Mk = 7
    End Enum

    Public Enum VMR9RenderPrefs
        RenderPrefs9_DoNotRenderBorder = 1
        RenderPrefs9_Mk = 1
    End Enum

    Public Enum VMR9AlphaBitmapFlags
        VMR9AlphaBitmap_Disable = 1
        VMR9AlphaBitmap_hDC = 2
        VMR9AlphaBitmap_EntireDDS = 4
        VMR9AlphaBitmap_SrcColorKey = 8
        VMR9AlphaBitmap_SrcRect = 16
        VMR9AlphaBitmap_FilterMode = 32
    End Enum

    Public Enum VMR9ProcAmpControlFlags
        ProcAmpControl9_Brightness = 1
        ProcAmpControl9_Contrt = 2
        ProcAmpControl9_Hue = 4
        ProcAmpControl9_Saturation = 8
        ProcAmpControl9_Mk = 15
    End Enum

    Public Enum VMR9MixerPrefs
        MixerPref9_NoDecimation = 1
        MixerPref9_DecimateOutput = 2
        MixerPref9_ARAdjustXorY = 4
        MixerPref9_NonSquareMixing = 8
        MixerPref9_DecimateMk = 15
        MixerPref9_BiLinearFiltering = 16
        MixerPref9_PointFiltering = 32
        MixerPref9_AnisotropicFiltering = 64
        MixerPref9_PyramidalQuadFiltering = 128
        MixerPref9_GaussianQuadFiltering = 256
        MixerPref9_FilteringReserved = 3584
        MixerPref9_FilteringMk = 4080
        MixerPref9_RenderTargetRGB = 4096
        MixerPref9_RenderTargetYUV = 8192
        MixerPref9_RenderTargetReserved = 1032192
        MixerPref9_RenderTargetMk = 1044480
        MixerPref9_DynamicSwitchToBOB = 1048576
        MixerPref9_DynamicDecimateBy2 = 2097152
        MixerPref9_DynamicReserved = 12582912
        MixerPref9_DynamicMk = 15728640
    End Enum

    Public Enum VMR9AspectRatioMode
        VMR9ARMode_None = 0
        VMR9ARMode_LetterBox = VMR9ARMode_None + 1
    End Enum

    Public Enum VMR9SurfaceAllocationFlags
        VMR9AllocFlag_3DRenderTarget = 1
        VMR9AllocFlag_DXVATarget = 2
        VMR9AllocFlag_TextureSurface = 4
        VMR9AllocFlag_OffscreenSurface = 8
        VMR9AllocFlag_UsageReserved = 240
        VMR9AllocFlag_UsageMk = 255
    End Enum

    Public Enum VMR9PresentationFlags
        VMR9Sample_SyncPoint = 1
        VMR9Sample_Preroll = 2
        VMR9Sample_Discontinuity = 4
        VMR9Sample_TimeValid = 8
    End Enum

    <ComImport(), Guid("1a777eaa-47c8-4930-b2c9-8fee1c1b0f3b"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IVMRMixerControl9

        <PreserveSig()> _
        Function SetAlpha(<[In]()> _
     ByVal dwStreamID As Integer, <[In]()> _
     ByVal Alpha As Single) As Integer

        <PreserveSig()> _
        Function GetAlpha(<[In]()> _
     ByVal dwStreamID As Integer, ByRef Alpha As Single) As Integer

        <PreserveSig()> _
        Function SetZOrder(<[In]()> _
     ByVal dwStreamID As Integer, <[In]()> _
     ByVal dwZ As Integer) As Integer

        <PreserveSig()> _
        Function GetZOrder(<[In]()> _
     ByVal dwStreamID As Integer, ByRef pZ As Integer) As Integer

        <PreserveSig()> _
        Function SetOutputRect(<[In]()> ByVal dwStreamID As Integer, _
        <[In]()> ByVal pRect As Integer) As Integer

        <PreserveSig()> _
        Function GetOutputRect(<[In]()> _
     ByVal dwStreamID As Integer, ByRef pRect As VMR9NormalizedRect) As Integer

        <PreserveSig()> _
        Function SetBackgroundClr(<[In]()> _
     ByVal ClrBkg As Integer) As Integer

        <PreserveSig()> _
        Function GetBackgroundClr(<[In]()> _
     ByVal lpClrBkg As Integer) As Integer

        <PreserveSig()> _
        Function SetMixingPrefs(<[In]()> _
     ByVal dwMixerPrefs As Integer) As Integer

        <PreserveSig()> _
        Function GetMixingPrefs(ByRef dwMixerPrefs As Integer) As Integer

        <PreserveSig()> _
        Function SetProcAmpControl(<[In]()> _
     ByVal dwStreamID As Integer, <[In]()> _
     ByVal lpClrControl As VMR9ProcAmpControl) As Integer

        <PreserveSig()> _
        Function GetProcAmpControl(<[In]()> _
     ByVal dwStreamID As Integer, ByRef lpClrControl As VMR9ProcAmpControl) As Integer

        <PreserveSig()> _
        Function GetProcAmpControlRange(<[In]()> _
     ByVal dwStreamID As Integer, ByRef lpClrControl As VMR9ProcAmpControlRange) As Integer
    End Interface

    <ComImport(), Guid("ced175e5-1935-4820-81bd-ff6ad00c9108"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IVMRMixerBitmap9

        <PreserveSig()> _
        Function SetAlphaBitmap(<[In]()> _
     ByVal pBmpParms As Integer) As Integer

        <PreserveSig()> _
        Function UpdateAlphaBitmapParameters(<[In]()> _
     ByVal pBmpParms As Integer) As Integer

        <PreserveSig()> _
        Function GetAlphaBitmapParameters(ByRef pBmpParms As Integer) As Integer
    End Interface

    <ComImport(), Guid("46c2e457-8ba0-4eef-b80b-0680f0978749"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IVMRMonitorConfig9

        <PreserveSig()> _
        Function SetMonitor(<[In]()> _
     ByVal uDev As Integer) As Integer

        <PreserveSig()> _
        Function GetMonitor(ByRef puDev As Integer) As Integer

        <PreserveSig()> _
        Function SetDefaultMonitor(<[In]()> _
     ByVal uDev As Integer) As Integer

        <PreserveSig()> _
        Function GetDefaultMonitor(ByRef puDev As Integer) As Integer

        <PreserveSig()> _
        Function GetAvailableMonitors(<Out(), MarshalAs(UnmanagedType.LPArray, SizeParamIndex:=1)> _
     ByVal pInfo As VMR9MonitorInfo(), <[In]()> _
     ByVal dwMaxInfoArraySize As Integer, ByRef pdwNumDevices As Integer) As Integer
    End Interface

    <ComImport(), Guid("5a804648-4f66-4867-9c43-4f5c822cf1b8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IVMRFilterConfig9

        <PreserveSig()> _
        Function SetImageCompositor(<[In]()> _
     ByVal lpVMRImgCompositor As IVMRImageCompositor9) As Integer

        <PreserveSig()> _
        Function SetNumberOfStreams(<[In]()> _
     ByVal dwMaxStreams As System.UInt32) As Integer

        <PreserveSig()> _
        Function GetNumberOfStreams(ByRef pdwMaxStreams As Integer) As Integer

        <PreserveSig()> _
        Function SetRenderingPrefs(<[In]()> _
     ByVal dwRenderFlags As VMR9RenderPrefs) As Integer

        <PreserveSig()> _
        Function GetRenderingPrefs(ByRef pdwRenderFlags As VMR9RenderPrefs) As Integer

        <PreserveSig()> _
        Function SetRenderingMode(<[In]()> _
     ByVal Mode As VMR9Mode) As Integer

        <PreserveSig()> _
        Function GetRenderingMode(ByRef pMode As VMR9Mode) As Integer
    End Interface

    <ComImport(), Guid("00d96c29-bbde-4efc-9901-bb5036392146"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IVMRAspectRatioControl9

        Function GetAspectRatioMode() As VMR9AspectRatioMode

        Function SetAspectRatioMode(ByVal dwARMode As VMR9AspectRatioMode) As Integer
    End Interface

    <ComImport(), Guid("a215fb8d-13c2-4f7f-993c-003d6271a459"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IVMRDeinterlaceControl9

        <PreserveSig()> _
        Function GetNumberOfDeinterlaceModes(ByRef lpVideoDescription As VMR9VideoDesc, ByRef lpdwNumDeinterlaceModes As System.UInt32, <Out(), MarshalAs(UnmanagedType.LPArray)> _
     ByVal lpDeinterlaceModes As Guid()) As Integer

        <PreserveSig()> _
        Function GetDeinterlaceModeCaps(ByRef lpDeinterlaceMode As Guid, ByRef lpVideoDescription As VMR9VideoDesc, ByRef lpDeinterlaceCaps As VMR9DeinterlaceCaps) As Integer

        <PreserveSig()> _
        Function GetDeinterlaceMode(ByVal dwStreamID As System.UInt32, ByRef lpDeinterlaceMode As Guid) As Integer

        <PreserveSig()> _
        Function SetDeinterlaceMode(ByVal dwStreamID As System.UInt32, ByRef lpDeinterlaceMode As Guid) As Integer

        Function GetDeinterlacePrefs() As VMR9DeinterlacePrefs

        <PreserveSig()> _
        Function SetDeinterlacePrefs(ByVal dwDeinterlacePrefs As VMR9DeinterlacePrefs) As Integer

        <PreserveSig()> _
        Function GetActualDeinterlaceMode(ByVal dwStreamID As System.UInt32, ByRef lpDeinterlaceMode As Guid) As Integer
    End Interface

    <ComImport(), Guid("1bd0ecb0-f8e2-11ce-aac6-0020af0b99a3"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IQualProp

        <PreserveSig()> _
        Function get_FramesDroppedInRenderer(ByRef pcFrames As Integer) As Integer

        <PreserveSig()> _
        Function get_FramesDrawn(ByVal pcFramesDrawn As Integer) As Integer

        <PreserveSig()> _
        Function get_AvgFrameRate(ByVal piAvgFrameRate As Integer) As Integer

        <PreserveSig()> _
        Function get_Jitter(ByVal iJitter As Integer) As Integer

        <PreserveSig()> _
        Function get_AvgSyncOffset(ByVal piAvg As Integer) As Integer

        <PreserveSig()> _
        Function get_DevSyncOffset(ByVal piDev As Integer) As Integer
    End Interface

    <ComImport(), Guid("d0cfe38b-93e7-4772-8957-0400c49a4485"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IVMRVideoStreamControl9

        Sub SetStreamActiveState(<MarshalAs(UnmanagedType.Bool)> ByVal fActive As Boolean)

        '<return: MarshalAs(UnmanagedType.Bool)> _ 
        Function GetStreamActiveState() As Boolean

    End Interface

    <ComImport(), Guid("dfc581a1-6e1f-4c3a-8d0a-5e9792ea2afc"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
 Public Interface IVMRSurface9

        <PreserveSig()> _
        Function IsSurfaceLocked() As Integer

        Function LockSurface() As IntPtr

        Sub UnlockSurface()

        '<return: MarshalAs(UnmanagedType.IUnknown)> _ 
        Function GetSurface() As Object
    End Interface

    <ComImport(), Guid("4a5c89eb-df51-4654-ac2a-e48e02bbabf6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IVMRImageCompositor9

        Sub InitCompositionDevice(<MarshalAs(UnmanagedType.IUnknown)> _
     ByVal pD3DDevice As Object)

        Sub TermCompositionDevice(<MarshalAs(UnmanagedType.IUnknown)> _
     ByVal pD3DDevice As Object)

        Sub SetStreamMediaType(ByVal dwStrmID As System.UInt32, ByRef pmt As AM_MEDIA_TYPE, <MarshalAs(UnmanagedType.Bool)> _
     ByVal fTexture As Boolean)

        Sub CompositeImage(<MarshalAs(UnmanagedType.IUnknown)> _
     ByVal pD3DDevice As Object, <MarshalAs(UnmanagedType.IUnknown)> _
     ByVal pddsRenderTarget As Object, ByRef pmtRenderTarget As AM_MEDIA_TYPE, ByVal rtStart As REFERENCE_TIME, ByVal rtEnd As REFERENCE_TIME, ByVal dwClrBkGnd As D3DCOLOR, <[In](), MarshalAs(UnmanagedType.LPArray)> _
     ByVal pVideoStreamInfo As VMR9VideoStreamInfo(), ByVal cStreams As System.UInt32)
    End Interface

    <ComImport(), Guid("69188c61-12a3-40f0-8ffc-342e7b433fd7"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IVMRImagePresenter9

        Sub StartPresenting(ByVal dwUserID As IntPtr)

        Sub StopPresenting(ByVal dwUserID As IntPtr)

        Sub PresentImage(ByVal dwUserID As IntPtr, ByRef lpPresInfo As VMR9PresentationInfo)
    End Interface

    <ComImport(), Guid("8d5148ea-3f5d-46cf-9df1-d1b896eedb1f"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IVMRSurfaceAllocator9

        Sub InitializeDevice(ByVal dwUserID As IntPtr, ByRef lpAllocInfo As VMR9AllocationInfo, ByRef lpNumBuffers As System.UInt32)

        Sub TerminateDevice(ByVal dwID As IntPtr)

        '<return: MarshalAs(UnmanagedType.IUnknown)> _ 
        Function GetSurface(ByVal dwUserID As IntPtr, ByVal SurfaceIndex As System.UInt32, ByVal SurfaceFlags As System.UInt32) As Object

        Sub AdviseNotify(ByVal lpIVMRSurfAllocNotify As IVMRSurfaceAllocatorNotify9)
    End Interface

    <ComImport(), Guid("dca3f5df-bb3a-4d03-bd81-84614bfbfa0c"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IVMRSurfaceAllocatorNotify9

        Sub AdviseSurfaceAllocator(ByVal dwUserID As IntPtr, ByVal lpIVRMSurfaceAllocator As IVMRSurfaceAllocator9)

        Sub SetD3DDevice(<MarshalAs(UnmanagedType.IUnknown)> _
     ByVal lpD3DDevice As Object, ByVal hMonitor As IntPtr)

        Sub ChangeD3DDevice(<MarshalAs(UnmanagedType.IUnknown)> _
     ByVal lpD3DDevice As Object, ByVal hMonitor As IntPtr)

        '<return: MarshalAs(UnmanagedType.IUnknown)> _ 
        Function AllocateSurfaceHelper(ByRef lpAllocInfo As VMR9AllocationInfo, ByRef lpNumBuffers As System.UInt32) As Object

        Sub NotifyEvent(ByVal EventCode As Integer, ByVal Param1 As IntPtr, ByVal Param2 As IntPtr)
    End Interface

    <ComImport(), Guid("8f537d09-f85e-4414-b23b-502e54c79927"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IVMRWindowlessControl9

        <PreserveSig()> _
        Function GetNativeVideoSize(ByRef lpWidth As Integer, ByRef lpHeight As Integer, ByRef lpARWidth As Integer, ByRef lpARHeight As Integer) As Integer

        <PreserveSig()> _
        Function GetMinIdealVideoSize(ByRef lpWidth As Integer, ByRef lpHeight As Integer) As Integer

        <PreserveSig()> _
        Function GetMaxIdealVideoSize(ByRef lpWidth As Integer, ByRef lpHeight As Integer) As Integer

        <PreserveSig()> _
        Function SetVideoPosition(ByRef lpSRCRect As RECT, ByRef lpDSTRect As RECT) As Integer

        <PreserveSig()> _
        Function GetVideoPosition(ByRef lpSRCRect As RECT, ByRef lpDSTRect As RECT) As Integer

        <PreserveSig()> _
        Function GetAspectRatioMode() As VMR9AspectRatioMode

        <PreserveSig()> _
        Function SetAspectRatioMode(ByVal AspectRatioMode As VMR9AspectRatioMode) As Integer

        <PreserveSig()> _
        Function SetVideoClippingWindow(ByVal hwnd As IntPtr) As Integer

        <PreserveSig()> _
        Function RepaintVideo(ByVal hwnd As IntPtr, ByVal hdc As IntPtr) As Integer

        <PreserveSig()> _
        Function DisplayModeChanged() As Integer

        <PreserveSig()> _
        Function GetCurrentImage(ByRef lpDib As IntPtr) As Integer

        <PreserveSig()> _
        Function SetBorderColor(ByVal Clr As Integer) As Integer

        <PreserveSig()> _
        Function GetBorderColor() As Integer
    End Interface

    <ComImport(), Guid("45c15cab-6e22-420a-8043-ae1f0ac02c7d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IVMRImagePresenterConfig9

        Sub SetRenderingPrefs(ByVal dwRenderFlags As VMR9RenderPrefs)

        Function GetRenderingPrefs() As VMR9RenderPrefs
    End Interface

End Namespace