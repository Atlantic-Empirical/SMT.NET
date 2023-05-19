Imports System
Imports System.Text
Imports System.Runtime.InteropServices

Namespace Multimedia.DirectShow

    <ComVisible(False), ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000000-0000-0000-C000-000000000046")> _
    Public Interface IUnknown
        Function QueryInterface(ByRef riid As Guid) As IntPtr

        <PreserveSig()> _
        Function AddRef() As UInt32

        <PreserveSig()> _
        Function Release() As UInt32
    End Interface 'IUnknown

End Namespace
