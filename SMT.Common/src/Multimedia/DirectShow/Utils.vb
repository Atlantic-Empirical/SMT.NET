Imports System.IO
Imports System.Runtime.InteropServices

Namespace Multimedia.DirectShow

    <ComVisible(True)> _
    Public Class DsUtils

        Public Shared Function IsCorrectDirectXVersion() As Boolean
            Return File.Exists(Path.Combine(Environment.SystemDirectory, "dpnhpast.dll"))
        End Function

        Public Shared Function ShowCapPinDialog(ByVal bld As ICaptureGraphBuilder2, ByVal flt As IBaseFilter, ByVal hwnd As IntPtr) As Boolean
            Dim hr As Integer
            Dim comObj As Object = Nothing
            Dim spec As ISpecifyPropertyPages = Nothing
            Dim cauuid As DsCAUUID = New DsCAUUID
            Try
                Dim cat As Guid = PinCategory.Capture
                Dim type As Guid = MediaType.Interleaved
                Dim iid As Guid = GetType(IAMStreamConfig).GUID
                hr = bld.FindInterface(cat, type, flt, iid, comObj)
                If Not (hr = 0) Then
                    type = MediaType.Video
                    hr = bld.FindInterface(cat, type, flt, iid, comObj)
                    If Not (hr = 0) Then
                        Return False
                    End If
                End If
                spec = CType(comObj, ISpecifyPropertyPages)
                If spec Is Nothing Then
                    Return False
                End If
                hr = spec.GetPages(cauuid)
                hr = OleCreatePropertyFrame(hwnd, 30, 30, Nothing, 1, comObj, cauuid.cElems, cauuid.pElems, 0, 0, IntPtr.Zero)
                Return True
            Catch ee As Exception
                Trace.WriteLine("!Ds.NET: ShowCapPinDialog " + ee.Message)
                Return False
            Finally
                If Not (cauuid.pElems.ToInt32 = IntPtr.Zero.ToInt32) Then
                    Marshal.FreeCoTaskMem(cauuid.pElems)
                End If
                spec = Nothing
                If Not (comObj Is Nothing) Then
                    Marshal.FinalReleaseComObject(comObj)
                End If
                comObj = Nothing
            End Try
        End Function

        <DllImport("olepro32.dll", CharSet:=CharSet.Unicode, ExactSpelling:=True)> _
        Public Shared Function OleCreatePropertyFrame(ByVal hwndOwner As IntPtr, ByVal x As Integer, ByVal y As Integer, ByVal lpszCaption As String, ByVal cObjects As Integer, <[In](), MarshalAs(UnmanagedType.Interface)> _
     ByRef ppUnk As Object, ByVal cPages As Integer, ByVal pPageClsID As IntPtr, ByVal lcid As Integer, ByVal dwReserved As Integer, ByVal pvReserved As IntPtr) As Integer
        End Function
    End Class

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure DsPOINT
        Public X As Integer
        Public Y As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure DsRECT
        Public Left As Integer
        Public Top As Integer
        Public Right As Integer
        Public Bottom As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential, Pack:=2), ComVisible(False)> _
    Public Structure DsBITMAPINFOHEADER
        Public Size As Integer
        Public Width As Integer
        Public Height As Integer
        Public Planes As Integer
        Public BitCount As Integer
        Public Compression As Integer
        Public ImageSize As Integer
        Public XPelsPerMeter As Integer
        Public YPelsPerMeter As Integer
        Public ClrUsed As Integer
        Public ClrImportant As Integer
    End Structure

    <ComVisible(False)> _
    Public Class DsROT

        Public Shared Function AddGraphToRot(ByVal graph As Object, ByRef cookie As Integer) As Boolean
            cookie = 0
            Dim hr As Integer = 0
            Dim rot As UCOMIRunningObjectTable = Nothing
            Dim mk As UCOMIMoniker = Nothing
            Try
                hr = GetRunningObjectTable(0, rot)
                If hr < 0 Then
                    Marshal.ThrowExceptionForHR(hr)
                End If
                Dim id As Integer = GetCurrentProcessId()
                Dim iuPtr As IntPtr = Marshal.GetIUnknownForObject(graph)
                Dim iuInt As Integer = iuPtr.ToInt32
                Marshal.Release(iuPtr)
                Dim item As String = String.Format("FilterGraph {0} pid {1}", iuInt.ToString("x8"), id.ToString("x8"))
                hr = CreateItemMoniker("!", item, mk)
                If hr < 0 Then
                    Marshal.ThrowExceptionForHR(hr)
                End If
                rot.Register(ROTFLAGS_REGISTRATIONKEEPSALIVE, graph, mk, cookie)
                Return True
            Catch generatedExceptionVariable0 As Exception
                Return False
            Finally
                If Not (mk Is Nothing) Then
                    Marshal.FinalReleaseComObject(mk)
                End If
                mk = Nothing
                If Not (rot Is Nothing) Then
                    Marshal.FinalReleaseComObject(rot)
                End If
                rot = Nothing
            End Try
        End Function

        Public Shared Function RemoveGraphFromRot(ByRef cookie As Integer) As Boolean
            Dim rot As UCOMIRunningObjectTable = Nothing
            Try
                Dim hr As Integer = GetRunningObjectTable(0, rot)
                If hr < 0 Then
                    Marshal.ThrowExceptionForHR(hr)
                End If
                rot.Revoke(cookie)
                cookie = 0
                Return True
            Catch generatedExceptionVariable0 As Exception
                Return False
            Finally
                If Not (rot Is Nothing) Then
                    Marshal.FinalReleaseComObject(rot)
                End If
                rot = Nothing
            End Try
        End Function
        Private Const ROTFLAGS_REGISTRATIONKEEPSALIVE As Integer = 1

        <DllImport("ole32.dll", ExactSpelling:=True)> _
   Private Shared Function GetRunningObjectTable(ByVal r As Integer, ByRef pprot As UCOMIRunningObjectTable) As Integer
        End Function

        <DllImport("ole32.dll", CharSet:=CharSet.Unicode, ExactSpelling:=True)> _
   Private Shared Function CreateItemMoniker(ByVal delim As String, ByVal item As String, ByRef ppmk As UCOMIMoniker) As Integer
        End Function

        <DllImport("kernel32.dll", ExactSpelling:=True)> _
   Private Shared Function GetCurrentProcessId() As Integer
        End Function
    End Class

    <ComVisible(True), ComImport(), Guid("B196B28B-BAB4-101A-B69C-00AA00341D07"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface ISpecifyPropertyPages

        <PreserveSig()> _
        Function GetPages(ByRef pPages As DsCAUUID) As Integer
    End Interface

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Structure DsCAUUID
        Public cElems As Integer
        Public pElems As IntPtr
    End Structure

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Class DsOptInt64

        Public Sub New(ByVal Value As Long)
            Me.Value = Value
        End Sub
        Public Value As Long
    End Class

    <StructLayout(LayoutKind.Sequential), ComVisible(False)> _
    Public Class DsOptIntPtr
        Public Pointer As IntPtr
    End Class
End Namespace