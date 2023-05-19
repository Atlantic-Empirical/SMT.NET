Imports System
Imports System.Text
Imports System.Runtime.InteropServices

Namespace Multimedia.Filters.SMT.Keystone

    <ComVisible(True), ComImport(), Guid("84D28780-77FA-411e-8918-8CBAB8F3BBAC"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IKeystoneQuality_FS

        <PreserveSig()> _
        Function get_TargetFR_Out(ByRef TargetFR As Single) As Integer

        <PreserveSig()> _
        Function get_TargetFR_Out_ATPF(ByRef TargetFR_ATPF As Long) As Integer

        <PreserveSig()> _
        Function get_TargetFR_In(ByRef TargetFR_In As Single) As Integer

        <PreserveSig()> _
        Function get_TargetFR_In_ATPF(ByRef TargetFR_In As Long) As Integer

        <PreserveSig()> _
        Function get_ActualFR_Out(ByRef ActualFR As Single) As Integer

        <PreserveSig()> _
        Function get_ActualFR_Out_ATPF(ByRef ActualFR_ATPF As Long) As Integer

        <PreserveSig()> _
        Function get_ActualFR_In(ByRef ActualFR As Single) As Integer

        <PreserveSig()> _
        Function get_ActualFR_In_ATPF(ByRef ActualFR_ATPF As Long) As Integer

        <PreserveSig()> _
        Function get_Jitter_In(ByRef InputJitter As Long) As Integer

        <PreserveSig()> _
        Function get_Jitter_Out(ByRef OutputJitter As Long) As Integer

    End Interface

End Namespace
