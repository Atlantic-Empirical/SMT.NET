Imports System
Imports System.Text
Imports System.Runtime.InteropServices

Namespace Multimedia.Filters.SMT.L21G

    <ComVisible(True), ComImport(), Guid("A231C86D-F2B5-406b-990A-6A235C5342DA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IL21G

        <PreserveSig()> _
        Function GetBuffer(ByRef Sample As IntPtr, ByRef SampleSize As Integer) As Integer

    End Interface

End Namespace