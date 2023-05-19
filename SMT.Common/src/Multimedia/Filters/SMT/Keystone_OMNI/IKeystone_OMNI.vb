Imports System
Imports System.Text
Imports System.Runtime.InteropServices
Imports SMT.Multimedia.DirectShow

Namespace Multimedia.Filters.SMT.Keystone

    <ComVisible(True), ComImport(), Guid("fd5010a3-8ebe-11ce-8183-00aa00577da1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IKeystone_OMNI

        <PreserveSig()> _
        Function get_InputMediaType(ByRef InputMediaType As AM_MEDIA_TYPE) As Integer

        <PreserveSig()> _
        Function get_OutputMediaType(ByRef OutputMediaType As AM_MEDIA_TYPE) As Integer

        <PreserveSig()> _
        Function get_InputWidth(ByRef InputWidth As Integer) As Integer

        <PreserveSig()> _
        Function get_InputHeight(ByRef InputHeight As Integer) As Integer

        <PreserveSig()> _
        Function get_OutputWidth(ByRef OutputWidth As Integer) As Integer

        <PreserveSig()> _
        Function get_OutputHeight(ByRef OutputHeight As Integer) As Integer

        <PreserveSig()> _
        Function GrabSample(<[In]()> ByVal SampleWhat As eSampleFrom, ByRef Sample As IntPtr, ByRef SampleSize As Integer, ByRef Width As Integer, ByRef Height As Integer) As Integer

        <PreserveSig()> _
        Function UnlockFilter(ByRef FilterKey As Guid) As Integer

        <PreserveSig()> _
        Function put_FeedbackClicks(<[In]()> ByVal DoClicks As Boolean) As Integer

        <PreserveSig()> _
        Function get_FeedbackClicks(ByRef DoClicks As Boolean) As Integer

        <PreserveSig()> _
        Function Set32Status(ByVal Do32 As Short) As Integer

        <PreserveSig()> _
        Function FrameStep(ByRef bForward As Boolean) As Integer

        <PreserveSig()> _
        Function QuitFrameStepping() As Integer

        <PreserveSig()> _
        Function ActivateVarispeed(<[In]()> ByVal Speed As Double) As Integer

        <PreserveSig()> _
        Function DeactivateVarispeed() As Integer

        <PreserveSig()> _
        Function SetL21State(<[In]()> ByVal bL21Active As Boolean) As Integer

        <PreserveSig()> _
        Function ShowJacketPicture( _
            <[In](), MarshalAs(UnmanagedType.LPStr)> ByVal stJPPath As String, _
            <[In]()> ByVal X As Integer, _
            <[In]()> ByVal Y As Integer, _
            <[In]()> ByVal W As Integer, _
            <[In]()> ByVal H As Integer) As Integer

        <PreserveSig()> _
        Function ShowBitmap( _
        <[In]()> ByVal pBMP As Integer, _
        <[In]()> ByVal W As Integer, _
        <[In]()> ByVal H As Integer, _
        <[In]()> ByVal X As Integer, _
        <[In]()> ByVal Y As Integer, _
        <[In]()> ByVal Format As Integer) As Integer

        <PreserveSig()> _
        Function SaveNextXFrames( _
            <[In]()> ByVal Count As Integer, _
            <[In](), MarshalAs(UnmanagedType.LPStr)> ByVal stDumpPath As String) As Integer

        <PreserveSig()> _
        Function ForceOutputConnectSize( _
        <[In]()> ByVal W As Integer, _
        <[In]()> ByVal H As Integer) As Integer

        <PreserveSig()> _
        Function ShowUYVYFile( _
        <[In](), MarshalAs(UnmanagedType.LPStr)> ByVal stYUVPath As String, _
        <[In]()> ByVal X As Integer, _
        <[In]()> ByVal Y As Integer, _
        <[In]()> ByVal W As Integer, _
        <[In]()> ByVal H As Integer) As Integer

        <PreserveSig()> _
        Function ClearUYVYFile() As Integer

        <PreserveSig()> _
        Function ShowUYVYBuffer( _
        <[In]()> ByVal pUYVY As Integer, _
        <[In]()> ByVal W As Integer, _
        <[In]()> ByVal H As Integer, _
        <[In]()> ByVal X As Integer, _
        <[In]()> ByVal Y As Integer) As Integer

        <PreserveSig()> _
        Function ClearUYVYFile_A() As Integer

        <PreserveSig()> _
        Function Pause(<[In]()> ByVal nPause As Integer) As Integer

        <PreserveSig()> _
        Function ActivateFFRW(<[In]()> ByVal nPause As Integer) As Integer

        <PreserveSig()> _
        Function DeactivateFFRW() As Integer

        <PreserveSig()> _
        Function ResendLastSamp(ByVal Unconditional As Integer) As Integer

        <PreserveSig()> _
        Function SetNULLTimestamps() As Integer

        <PreserveSig()> _
        Function SetProperty(<[In]()> ByVal gProp As Guid, ByVal iValue As Integer) As Integer

        <PreserveSig()> _
        Function StartRecording(<[In](), MarshalAs(UnmanagedType.LPWStr)> ByVal pszFileName As String) As Integer

        <PreserveSig()> _
        Function StopRecording() As Integer

        <PreserveSig()> _
        Function SetTrialOverride(<[In]()> ByVal bOverride As Boolean) As Integer

        <PreserveSig()> _
        Function SendMediaTimeEvents(ByVal bOverride As UInt16, ByVal iType As UInt16) As Integer

        <PreserveSig()> _
        Function SetOptimizationLevel(ByVal iOptimizationLevel As UInt16) As Integer

        <PreserveSig()> _
        Function SetRenderer(ByVal iRenderer As UInt16) As Integer

    End Interface

End Namespace