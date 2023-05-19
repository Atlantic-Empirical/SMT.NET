Imports System.Runtime.InteropServices

Namespace Win.COM

    Public Module mCOM

        Public gIUnknown As Guid = New System.Guid("00000000-0000-0000-C000-000000000046")

        <DllImport("ole32.dll")> _
        Private Function CoCreateInstance(ByRef clsid As Guid, ByVal pUnkOuter As IntPtr, ByVal dwClsContext As CLSCTX, ByRef iid As Guid, ByRef ptrIf As IntPtr) As Integer
        End Function

        Public Function CreateCOMInstance(ByRef clsid As Guid, ByRef riid As Guid) As Object
            Try
                Dim ptrIf As IntPtr
                Dim hr As Integer = CoCreateInstance(clsid, IntPtr.Zero, CLSCTX.Inproc, riid, ptrIf)
                If hr < 0 Then Marshal.ThrowExceptionForHR(hr)
                If (Not (hr = 0)) OrElse (ptrIf.ToInt32 = IntPtr.Zero.ToInt32) Then
                    'Throw New Exception("Error with CreateDsInstance. Error: " & MarshalGetExceptionForHR(hr).Message.ToString)
                    Return Nothing
                End If
                Dim iu As Guid = New Guid("00000000-0000-0000-C000-000000000046")
                Dim ptrXX As IntPtr
                hr = Marshal.QueryInterface(ptrIf, iu, ptrXX)
                Dim ooo As Object = System.Runtime.Remoting.Services.EnterpriseServicesHelper.WrapIUnknownWithComObject(ptrIf)
                Dim ct As Integer = Marshal.Release(ptrIf)
                Return ooo
            Catch ex As Exception
                Throw New Exception("Problem with CreateCOMInstance. Error: " & ex.Message)
            End Try
        End Function

        'Public Function CreateCOMInstance(ByRef clsid As Guid, ByRef riid As Guid) As Object
        '    Dim ptrIf As IntPtr
        '    Dim hr As Integer = CoCreateInstance(clsid, IntPtr.Zero, CLSCTX.Inproc, riid, ptrIf)
        '    If (Not (hr = 0)) OrElse (ptrIf.ToInt32 = IntPtr.Zero.ToInt32) Then
        '        'Throw New Exception("Error with CreateDsInstance. Error: " & MarshalGetExceptionForHR(hr).Message.ToString)
        '        Return Nothing
        '    End If
        '    Dim iu As Guid = New Guid("00000000-0000-0000-C000-000000000046")
        '    Dim ptrXX As IntPtr
        '    hr = Marshal.QueryInterface(ptrIf, iu, ptrXX)
        '    Dim ooo As Object = System.Runtime.Remoting.Services.EnterpriseServicesHelper.WrapIUnknownWithComObject(ptrIf)
        '    Dim ct As Integer = Marshal.Release(ptrIf)
        '    Return ooo
        'End Function

#Region "ENUMS"

        <Flags()> _
        Friend Enum CLSCTX
            Inproc = 3
            Server = 21
            All = 23
        End Enum

#End Region 'ENUMS

    End Module

End Namespace
