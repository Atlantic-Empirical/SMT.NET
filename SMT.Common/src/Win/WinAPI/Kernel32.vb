Imports System.Text

Namespace Win.WinAPI

    Public Module Kernel32

        Public Declare Function GetLocaleInfo Lib "kernel32" Alias "GetLocaleInfoA" (ByVal Locale As Integer, ByVal LCType As Integer, ByVal lpLCData As String, ByVal cchData As Integer) As Integer
        Public Declare Function GetVolumeInformation Lib "kernel32" Alias "GetVolumeInformationA" (ByVal lpRootPathName As String, ByVal lpVolumeNameBuffer As StringBuilder, ByVal nVolumeNameSize As Integer, ByRef lpVolumeSerialNumber As UInt32, ByRef lpMaximumComponentLength As UInt32, ByRef lpFileSystemFlags As UInt32, ByVal lpFileSystemNameBuffer As StringBuilder, ByVal nFileSystemNameSize As Integer) As Boolean
        Public Declare Function QueryPerformanceFrequency Lib "kernel32" (ByRef lpFrequency As Integer) As Integer
        Public Declare Function GetLastError Lib "kernel32" Alias "GetLastError" () As Integer
        Public Declare Function FormatMessage Lib "kernel32" Alias "FormatMessageA" (ByVal dwFlags As Integer, ByVal lpSource As Object, ByVal dwMessageId As Integer, ByVal dwLanguageId As Integer, ByVal lpBuffer As String, ByVal nSize As Integer, ByVal Arguments As Integer) As Integer
        Public Declare Function Beep Lib "kernel32" (ByVal freq As Integer, ByVal duration As Integer) As Boolean
        Public Declare Function GetDiskFreeSpaceEx Lib "kernel32" Alias "GetDiskFreeSpaceExA" (ByVal lpDirectoryName As String, ByRef lpFreeBytesAvailableToCaller As Long, ByRef lpTotalNumberOfBytes As Long, ByRef lpTotalNumberOfFreeBytes As Long) As Long
        Public Declare Function GetDriveType Lib "kernel32" Alias "GetDriveTypeA" (ByVal nDrive As String) As Integer
        Public Declare Function GetProductInfo Lib "Kernel32" (ByVal OSMajorVersion As UInt32, ByVal OSMinorVersion As UInt32, ByVal SPMajorVersion As UInt32, ByVal SPMinorVersion As UInt32, ByRef ReturnedProductType As UInt32) As Boolean
        Public Declare Function GetVersion Lib "Kernel32" Alias "GetVersionExA" (ByVal lpVersionInformation As OSVERSIONINFO) As Integer
        Public Declare Function LoadLibrary Lib "kernel32" (ByVal lpFileName As String) As IntPtr

        'Const FORMAT_MESSAGE_ALLOCATE_BUFFER = &H100
        Public Const FORMAT_MESSAGE_FROM_SYSTEM = &H1000
        Public Const LANG_NEUTRAL = &H0
        'Const SUBLANG_DEFAULT = &H1
        'Const ERROR_BAD_USERNAME = 2202&

    End Module

End Namespace
