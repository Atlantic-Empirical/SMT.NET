Imports System.Text
Imports System.Runtime.InteropServices
Imports SMT.Win.WinAPI.Constants.HookFlags

Namespace Win.WinAPI


    Public Module Shlwapi

        <DllImport("shlwapi.dll", CharSet:=CharSet.Unicode)> _
        Public Function SHCreateStreamOnFile(ByVal pszFile As String, ByVal grfMode As UInteger, ByRef ppstm As System.Runtime.InteropServices.ComTypes.IStream) As UInteger
        End Function

    End Module

End Namespace
