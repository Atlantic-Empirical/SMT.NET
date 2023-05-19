Imports System.Runtime.InteropServices

Namespace Multimedia.Filters.BlackMagic

    <ComVisible(True), ComImport(), Guid("60F58A81-A387-4922-AAAC-998BD9FBE1AA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IDecklinkIOControl

        'virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE GetIOFeatures( 
        '    /* [out] */ unsigned long *features) = 0;
        <PreserveSig()> _
        Function GetIOFeatures( _
            ByRef features As ULong _
        ) As Integer

        'virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetAnalogueOutput( 
        '    /* [in] */ BOOL isComponent,
        '    /* [in] */ BOOL setupIs75) = 0;
        <PreserveSig()> _
        Function SetAnalogueOutput( _
            <[In](), MarshalAs(UnmanagedType.Bool)> ByVal isComponent As Boolean, _
            <[In](), MarshalAs(UnmanagedType.Bool)> ByVal setupIs75 As Boolean _
        ) As Integer

        'virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetVideoInput( 
        '    /* [in] */ BOOL inputIsDigital,
        '    /* [in] */ BOOL isComponent,
        '    /* [in] */ BOOL setupIs75) = 0;
        <PreserveSig()> _
        Function SetVideoInput( _
            <[In](), MarshalAs(UnmanagedType.Bool)> ByVal inputIsDigital As Boolean, _
            <[In](), MarshalAs(UnmanagedType.Bool)> ByVal isComponent As Boolean, _
            <[In](), MarshalAs(UnmanagedType.Bool)> ByVal setupIs75 As Boolean _
        ) As Integer

        'virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetDualLinkOutput( 
        '    /* [in] */ BOOL enableDualLinkOutput) = 0;
        <PreserveSig()> _
        Function SetDualLinkOutput( _
            <[In](), MarshalAs(UnmanagedType.Bool)> ByVal enableDualLinkOutput As Boolean _
        ) As Integer

        'virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetSingleFieldOutputForSynchronousFrames( 
        '    /* [in] */ BOOL singleFieldOutput) = 0;
        <PreserveSig()> _
        Function SetSingleFieldOutputForSynchronousFrames( _
            <[In](), MarshalAs(UnmanagedType.Bool)> ByVal singleFieldOutput As Boolean _
        ) As Integer

        'virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetHDTVPulldownOnOutput( 
        '    /* [in] */ BOOL enableHDTV32PulldownOnOutput) = 0;
        <PreserveSig()> _
        Function SetHDTVPulldownOnOutput( _
            <[In](), MarshalAs(UnmanagedType.Bool)> ByVal enableHDTV32PulldownOnOutput As Boolean _
        ) As Integer

        'virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetBlackToDeckInCapture( 
        '    /* [in] */ unsigned long blackToDeckSetting) = 0;
        <PreserveSig()> _
        Function SetBlackToDeckInCapture( _
            <[In](), MarshalAs(UnmanagedType.U4)> ByVal blackToDeckSetting As Integer _
        ) As Integer

        'virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetAFrameReference( 
        '    /* [in] */ unsigned long aFrameReference) = 0;
        Function SetAFrameReference( _
            <[In](), MarshalAs(UnmanagedType.U4)> ByVal aFrameReference As Integer _
        ) As Integer

        'virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetCaptureVANCLines( 
        '    /* [in] */ unsigned long vancLine1,
        '    /* [in] */ unsigned long vancLine2,
        '    /* [in] */ unsigned long vancLine3) = 0;
        Function SetCaptureVANCLines( _
            <[In](), MarshalAs(UnmanagedType.U4)> ByVal vancLine1 As Integer, _
            <[In](), MarshalAs(UnmanagedType.U4)> ByVal vancLine2 As Integer, _
            <[In](), MarshalAs(UnmanagedType.U4)> ByVal vancLine3 As Integer _
        ) As Integer

        'virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetVideoOutputDownconversionMode( 
        '    /* [in] */ unsigned long downconversionMode) = 0;
        Function SetVideoOutputDownconversionMode( _
            <[In](), MarshalAs(UnmanagedType.U4)> ByVal downconversionMode As Integer _
        ) As Integer

        'virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetAudioInputSource( 
        '    /* [in] */ unsigned long audioInputSource) = 0;
        Function SetAudioInputSource( _
            <[In](), MarshalAs(UnmanagedType.U4)> ByVal audioInputSource As Integer _
        ) As Integer

        'virtual /* [helpstring] */ HRESULT STDMETHODCALLTYPE SetGenlockTiming( 
        '    int timingOffset) = 0;
        Function SetGenlockTiming( _
            <[In](), MarshalAs(UnmanagedType.U2)> ByVal timingOffset As Short _
        ) As Integer

    End Interface

    Public Enum eIOFeatures
        DECKLINK_IOFEATURES_HASCOMPONENTVIDEOOUTPUT = 1 << 0
        DECKLINK_IOFEATURES_HASCOMPOSITEVIDEOOUTPUT = 1 << 1
        DECKLINK_IOFEATURES_HASDIGITALVIDEOOUTPUT = 1 << 2
        DECKLINK_IOFEATURES_HASCOMPONENTVIDEOINPUT = 1 << 3
        DECKLINK_IOFEATURES_HASCOMPOSITEVIDEOINPUT = 1 << 4
        DECKLINK_IOFEATURES_HASDIGITALVIDEOINPUT = 1 << 5
        DECKLINK_IOFEATURES_HASDUALLINKOUTPUT = 1 << 6
        DECKLINK_IOFEATURES_HASDUALLINKINPUT = 1 << 7
        DECKLINK_IOFEATURES_SUPPORTSHD = 1 << 8
        DECKLINK_IOFEATURES_SUPPORTSHDDOWNCONVERSION = 1 << 9
        DECKLINK_IOFEATURES_HASAESAUDIOINPUT = 1 << 10
        DECKLINK_IOFEATURES_MAX = DECKLINK_IOFEATURES_HASAESAUDIOINPUT + 1
    End Enum

    Public Enum eIOFeatures2
        DECKLINK_IOFEATURES_SUPPORTSRGB10BITCAPTURE = 1 << 0
        DECKLINK_IOFEATURES_SUPPORTSRGB10BITPLAYBACK = 1 << 1
        DECKLINK_IOFEATURES_HDAMATEUR = 1 << 2 'NOT DOCUMENTED
        DECKLINK_IOFEATURES_MACONLY = 1 << 3 'NOT DOCUMENTED
        DECKLINK_IOFEATURES_SUPPORTSINTERNALKEY = 1 << 4
        DECKLINK_IOFEATURES_SUPPORTSEXTERNALKEY = 1 << 5
        DECKLINK_IOFEATURES_HASCOMPONENTVIDEOOUTPUT = 1 << 6
        DECKLINK_IOFEATURES_HASCOMPOSITEVIDEOOUTPUT = 1 << 7
        DECKLINK_IOFEATURES_HASDIGITALVIDEOOUTPUT = 1 << 8
        DECKLINK_IOFEATURES_HASDVIVIDEOOUTPUT = 1 << 9
        DECKLINK_IOFEATURES_HASCOMPONENTVIDEOINPUT = 1 << 10
        DECKLINK_IOFEATURES_HASCOMPOSITEVIDEOINPUT = 1 << 11
        DECKLINK_IOFEATURES_HASDIGITALVIDEOINPUT = 1 << 12
        DECKLINK_IOFEATURES_HASDUALLINKOUTPUT = 1 << 13
        DECKLINK_IOFEATURES_HASDUALLINKINPUT = 1 << 14
        DECKLINK_IOFEATURES_SUPPORTSHD = 1 << 15
        DECKLINK_IOFEATURES_SUPPORTS2KOUTPUT = 1 << 16
        DECKLINK_IOFEATURES_SUPPORTSHDDOWNCONVERSION = 1 << 17
        DECKLINK_IOFEATURES_HASAESAUDIOINPUT = 1 << 18
        DECKLINK_IOFEATURES_HASANALOGUEAUDIOINPUT = 1 << 19
        DECKLINK_IOFEATURES_HASSVIDEOINPUT = 1 << 20
        DECKLINK_IOFEATURES_HASSVIDEOOUTPUT = 1 << 21
        DECKLINK_IOFEATURES_SUPPORTSMULTICAMERAINPUT = 1 << 22
        DECKLINK_IOFEATURES_MULTIBRIDGEONAIR_1 = 1 << 23 'NOT DOCUMENTED
        DECKLINK_IOFEATURES_MULTIBRIDGEONAIR_2 = 1 << 24 'NOT DOCUMENTED
        DECKLINK_IOFEATURES_HASRS422SERIALPORT = 1 << 25 'might be 26
        DECKLINK_IOFEATURES_MULTIBRIDGE_ECLIPSE_3GBPS = 1 << 26 'NOT DOCUMENTED
        DECKLINK_IOFEATURES_MAX = DECKLINK_IOFEATURES_SUPPORTSMULTICAMERAINPUT + 1
    End Enum

End Namespace
