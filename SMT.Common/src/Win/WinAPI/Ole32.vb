Imports System.Text
Imports System.Runtime.InteropServices
Imports SMT.Win.WinAPI.Constants.HookFlags

Namespace Win.WinAPI

    Public Module Ole32

        Declare Auto Function CreateStreamOnHGlobal Lib "ole32" (ByVal hGlobal As IntPtr, ByVal fDeleteOnRelease As Boolean, ByRef ppstm As UCOMIStream) As Long

    End Module

End Namespace
