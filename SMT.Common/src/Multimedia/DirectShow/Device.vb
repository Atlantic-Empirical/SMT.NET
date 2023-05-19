Imports System
Imports System.Collections
Imports System.Runtime.InteropServices

Namespace Multimedia.DirectShow

    <ComVisible(False)> _
    Public Class DsDev

        Public Shared Function GetDevicesOfCat(ByVal cat As Guid, ByRef devs As ArrayList) As Boolean
            devs = Nothing
            Dim hr As Integer
            Dim comObj As Object = Nothing
            Dim enumDev As ICreateDevEnum = Nothing
            Dim enumMon As UCOMIEnumMoniker = Nothing
            Dim mon(1) As UCOMIMoniker
            Try
                Dim srvType As Type = Type.GetTypeFromCLSID(Clsid.SystemDeviceEnum)
                If srvType Is Nothing Then
                    Throw New NotImplementedException("System Device Enumerator")
                End If
                comObj = Activator.CreateInstance(srvType)
                enumDev = CType(comObj, ICreateDevEnum)
                hr = enumDev.CreateClassEnumerator(cat, enumMon, 0)
                If Not (hr = 0) Then
                    Throw New NotSupportedException("No devices of the category")
                End If
                Dim f As Integer
                Dim count As Integer = 0
                Do
                    hr = enumMon.Next(1, mon, f)
                    If (Not (hr = 0)) OrElse (mon(0) Is Nothing) Then
                        ' break 
                    End If
                    Dim dev As DsDevice = New DsDevice
                    dev.Name = GetFriendlyName(mon(0))
                    If devs Is Nothing Then
                        devs = New ArrayList
                    End If
                    dev.Mon = mon(0)
                    mon(0) = Nothing
                    devs.Add(dev)
                    dev = Nothing
                    System.Math.Min(System.Threading.Interlocked.Increment(count), count - 1)
                Loop While True
                Return count > 0
            Catch generatedExceptionVariable0 As Exception
                If Not (devs Is Nothing) Then
                    For Each d As DsDevice In devs
                        d.Dispose()
                    Next
                    devs = Nothing
                End If
                Return False
            Finally
                enumDev = Nothing
                If Not (mon(0) Is Nothing) Then
                    Marshal.FinalReleaseComObject(mon(0))
                End If
                mon(0) = Nothing
                If Not (enumMon Is Nothing) Then
                    Marshal.FinalReleaseComObject(enumMon)
                End If
                enumMon = Nothing
                If Not (comObj Is Nothing) Then
                    Marshal.FinalReleaseComObject(comObj)
                End If
                comObj = Nothing
            End Try
        End Function

        Private Shared Function GetFriendlyName(ByVal mon As UCOMIMoniker) As String
            Dim bagObj As Object = Nothing
            Dim bag As IPropertyBag = Nothing
            Try
                Dim bagId As Guid = GetType(IPropertyBag).GUID
                mon.BindToStorage(Nothing, Nothing, bagId, bagObj)
                bag = CType(bagObj, IPropertyBag)
                Dim val As Object = ""
                Dim hr As Integer = bag.Read("FriendlyName", val, IntPtr.Zero)
                If Not (hr = 0) Then
                    Marshal.ThrowExceptionForHR(hr)
                End If
                Dim ret As String = CType(val, String)
                If (ret Is Nothing) OrElse (ret.Length < 1) Then
                    Throw New NotImplementedException("Device FriendlyName")
                End If
                Return ret
            Catch generatedExceptionVariable0 As Exception
                Return Nothing
            Finally
                bag = Nothing
                If Not (bagObj Is Nothing) Then
                    Marshal.FinalReleaseComObject(bagObj)
                End If
                bagObj = Nothing
            End Try
        End Function

    End Class

    <ComVisible(False)> _
    Public Class DsDevice
        Implements IDisposable

        Public Name As String
        Public Mon As UCOMIMoniker

        Public Sub Dispose()
            If Not (Mon Is Nothing) Then
                Marshal.FinalReleaseComObject(Mon)
            End If
            Mon = Nothing
        End Sub

        Public Sub Dispose1() Implements System.IDisposable.Dispose

        End Sub

    End Class

    <ComVisible(True), ComImport(), Guid("29840822-5B84-11D0-BD3B-00A0C911CE86"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface ICreateDevEnum

        <PreserveSig()> _
        Function CreateClassEnumerator( _
            <[In]()> ByRef pType As Guid, _
            <Out()> ByRef ppEnumMoniker As UCOMIEnumMoniker, _
            <[In]()> ByVal dwFlags As Integer _
        ) As Integer

    End Interface

    <ComVisible(True), ComImport(), Guid("55272A00-42CB-11CE-8135-00AA004BB851"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IPropertyBag

        <PreserveSig()> _
        Function Read( _
            <[In](), MarshalAs(UnmanagedType.LPWStr)> ByVal pszPropName As String, _
            <[In](), Out(), MarshalAs(UnmanagedType.Struct)> ByRef pVar As Object, _
            ByVal pErrorLog As IntPtr _
        ) As Integer

        <PreserveSig()> _
        Function Write( _
            <[In](), MarshalAs(UnmanagedType.LPWStr)> ByVal pszPropName As String, _
            <[In](), MarshalAs(UnmanagedType.Struct)> ByRef pVar As Object _
        ) As Integer

    End Interface

End Namespace
