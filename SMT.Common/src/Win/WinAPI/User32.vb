Imports System.Text
Imports System.Runtime.InteropServices
Imports SMT.Win.WinAPI.Constants.HookFlags

Namespace Win.WinAPI

    Public Module User32

        Public Declare Auto Function SetParent Lib "user32" Alias "SetParent" (ByVal hWndChild As Integer, ByVal hWndNewParent As Integer) As Integer
        Public Declare Auto Function GetWindowLong Lib "User32" (ByVal hWnd As IntPtr, ByVal Index As Integer) As Integer
        Public Declare Auto Function SetWindowLong Lib "User32" (ByVal hWnd As IntPtr, ByVal Index As Integer, ByVal Value As Integer) As Integer
        Public Declare Auto Function SetWindowPos Lib "User32" (ByVal hWnd As IntPtr, ByVal hWndInsertAfter As IntPtr, ByVal x As Integer, ByVal y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal uFlags As System.UInt32) As Integer
        Public Declare Auto Function SetCursorPos Lib "user32" (ByVal x As Integer, ByVal y As Integer) As Integer
        Public Declare Auto Function DeleteMenu Lib "user32" (ByVal hMenu As Integer, ByVal nPosition As Integer, ByVal wFlags As Integer) As Integer
        Public Declare Auto Function GetSystemMenu Lib "user32" (ByVal hWnd As Integer, ByVal bRevert As Integer) As Integer
        Public Declare Auto Function SendMessage Lib "user32.dll" (ByVal hWnd As IntPtr, ByVal wMsg As Int32, ByVal wParam As Int32, ByVal lParam As Int32) As Int32
        Public Declare Auto Function SystemParametersInfo Lib "User32" Alias "SystemParametersInfoA" (ByVal uAction As Integer, ByVal uParam As Integer, ByVal lpvParam As Integer, ByVal fuWinIni As Integer) As Integer
        Public Declare Auto Function UnhookWindowsHookEx Lib "user32" (ByVal hHook As Integer) As Integer
        Public Declare Auto Function SetWindowsHookEx Lib "user32" Alias "SetWindowsHookExA" (ByVal idHook As Integer, ByVal lpfn As KeyboardHookDelegate, ByVal hmod As Integer, ByVal dwThreadId As Integer) As Integer
        Public Delegate Function KeyboardHookDelegate(ByVal Code As Integer, ByVal wParam As Integer, ByRef lParam As KBDLLHOOKSTRUCT) As Integer
        Public Declare Auto Function CallNextHookEx Lib "user32" (ByVal hHook As Integer, ByVal nCode As Integer, ByVal wParam As Integer, ByRef lParam As KBDLLHOOKSTRUCT) As Integer
        Public Declare Auto Function GetAsyncKeyState Lib "user32" (ByVal vKey As Integer) As Integer
        Public Declare Auto Function ShowWindow Lib "user32" Alias "ShowWindow" (ByVal hwnd As Integer, ByVal nCmdShow As Integer) As Integer
        Public Declare Auto Function CreateWindowEx Lib "user32" Alias "CreateWindowExA" (ByVal dwExStyle As Integer, ByVal lpClassName As String, ByVal lpWindowName As String, ByVal dwStyle As Integer, ByVal x As Integer, ByVal y As Integer, ByVal nWidth As Integer, ByVal nHeight As Integer, ByVal hWndParent As Integer, ByVal hMenu As Integer, ByVal hInstance As Integer, ByVal lpParam As Object) As Long
        Public Declare Ansi Function FindWindow Lib "user32" Alias "FindWindowA" (ByVal lpClassName As String, ByVal lpWindowName As String) As Integer
        Public Declare Ansi Function GetClassName Lib "user32" Alias "GetClassNameA" (ByVal hwnd As IntPtr, ByVal lpClassName As StringBuilder, ByVal nMaxCount As Integer) As Integer
        Public Declare Auto Function HideCaret Lib "user32" (ByVal hWnd As IntPtr) As Integer
        Public Declare Sub SetWindowPos Lib "User32" (ByVal hWnd As Integer, ByVal hWndInsertAfter As Integer, ByVal X As Integer, ByVal Y As Integer, ByVal cx As Integer, ByVal cy As Integer, ByVal wFlags As Integer)

    End Module

End Namespace
