Imports System.Runtime.InteropServices

Namespace Multimedia.DirectShow

    <ComVisible(True), ComImport(), Guid("5ACD6AA0-F482-11ce-8B67-00AA00A3F1A6"), InterfaceType(ComInterfaceType.InterfaceIsDual)> _
    Public Interface IConfigAviMux

        <PreserveSig()> _
        Function SetMasterStream(<[In]()> ByVal iStream As Integer) As Integer

        <PreserveSig()> _
        Function GetMasterStream(ByRef pStream As Integer) As Integer

        <PreserveSig()> _
        Function SetOutputCompatibilityIndex(<[In]()> ByVal fOldIndex As Boolean) As Integer

        <PreserveSig()> _
        Function GetOutputCompatibilityIndex(ByRef pfOldIndex As Boolean) As Integer

    End Interface

    <Flags()> _
    Public Enum InterleavingMode
        INTERLEAVE_NONE
        INTERLEAVE_CAPTURE
        INTERLEAVE_FULL
        INTERLEAVE_NONE_BUFFERED
    End Enum

    <ComVisible(True), ComImport(), Guid("BEE3D220-157B-11d0-BD23-00A0C911CE86"), InterfaceType(ComInterfaceType.InterfaceIsDual)> _
    Public Interface IConfigInterleaving

        <PreserveSig()> _
        Function put_Mode(ByVal mode As Integer) As Integer

        <PreserveSig()> _
        Function get_Mode(ByRef pMode As InterleavingMode) As Integer

        <PreserveSig()> _
        Function put_Interleaving( _
            <[In]()> ByVal prtInterleave As Long, _
            <[In]()> ByVal prtPreroll As Long) As Integer

        <PreserveSig()> _
        Function get_Interleaving(ByRef prtInterleave As Long, ByRef prtPreroll As Long) As Integer

    End Interface

End Namespace
