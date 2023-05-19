Imports System.Runtime.InteropServices
'Imports System.Windows.Forms
'Imports SMT.Win.WinAPI.Constants

Namespace DotNet.Utility

    Public Module GeneralWindows

        Public Function GetPointerFromObject(ByRef Obj As Object) As IntPtr
            Try
                Dim Handle As GCHandle = GCHandle.Alloc(Obj, GCHandleType.Pinned)
                Return Handle.AddrOfPinnedObject()
            Catch ex As Exception
                Throw New Exception("problem getting pointer from object. error: " & ex.Message)
            End Try
        End Function

        Public Function GetObjectFromPointer(ByVal Pointer As IntPtr) As Object
            Try
                Return Marshal.GetObjectForIUnknown(Pointer)
                'then do a ctype on the object
            Catch ex As Exception
                Throw New Exception("problem getting object from pointer. error: " & ex.Message)
            End Try
        End Function

    End Module

End Namespace
