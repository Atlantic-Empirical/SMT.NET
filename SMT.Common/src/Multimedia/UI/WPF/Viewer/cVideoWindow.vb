#Region "Using directives"

Imports System
Imports System.Drawing
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Interop
Imports System.Runtime.InteropServices
Imports SMT.Multimedia.DirectShow
Imports SMT.Win.WinAPI.Constants

#End Region

Namespace Multimedia.UI.WPF.Viewer

    Public Class cVideoWindow
        Inherits HwndHost

        Private _iVideoWindow As IVideoWindow
        Private _width, _height As Integer
        'Private DrainHandle As IntPtr

        Public Event evRightClick()
        Public Event evMouseMove(ByVal lParam As Integer)
        Public Event evLeftButtonUp(ByVal lParam As Integer)
        Public Event evLeftButtonDoubleClick()

        Public Sub New(ByVal iVideoWindow As IVideoWindow, ByVal width As Integer, ByVal height As Integer) ', ByVal nDrainHandle As IntPtr
            _iVideoWindow = iVideoWindow
            _width = width
            _height = height
            'DrainHandle = nDrainHandle
        End Sub

        Protected Overloads Overrides Function BuildWindowCore(ByVal hwndParent As HandleRef) As HandleRef
            Dim hwndHost As IntPtr = IntPtr.Zero
            hwndHost = CreateWindowEx(0, "static", "", WS_CHILD Or WS_VISIBLE, 0, 0, _width, _height, hwndParent.Handle, CType(HOST_ID, IntPtr), IntPtr.Zero, 0)
            _iVideoWindow.put_Owner(hwndHost.ToInt32())
            _iVideoWindow.put_WindowStyle(WS_CHILD Or WS_CLIPSIBLINGS Or WS_CLIPCHILDREN)
            _iVideoWindow.put_MessageDrain(hwndHost.ToInt32())  'hwndHost.ToInt32() '(DrainHandle) '(hwndParent.Handle) 
            _iVideoWindow.SetWindowPosition(0, 0, _width, _height)
            Return New HandleRef(Me, hwndHost)
        End Function

        Protected Overloads Overrides Sub DestroyWindowCore(ByVal hwnd As HandleRef)
            DestroyWindow(hwnd.Handle)
        End Sub

        <DllImport("user32.dll", EntryPoint:="CreateWindowEx", CharSet:=CharSet.Auto)> _
        Friend Shared Function CreateWindowEx(ByVal dwExStyle As Integer, ByVal lpszClassName As String, ByVal lpszWindowName As String, ByVal style As Integer, ByVal x As Integer, ByVal y As Integer, _
        ByVal width As Integer, ByVal height As Integer, ByVal hwndParent As IntPtr, ByVal hMenu As IntPtr, ByVal hInst As IntPtr, <MarshalAs(UnmanagedType.AsAny)> ByVal pvParam As Object) As IntPtr
        End Function

        <DllImport("user32.dll", EntryPoint:="DestroyWindow", CharSet:=CharSet.Auto)> _
        Friend Shared Function DestroyWindow(ByVal hwnd As IntPtr) As Boolean
        End Function

        Protected Overloads Overrides Function WndProc(ByVal hwnd As IntPtr, ByVal msg As Integer, ByVal wParam As IntPtr, ByVal lParam As IntPtr, ByRef handled As Boolean) As IntPtr
            'Debug.WriteLine("cVideoWindow Msg: " & Hex(msg))
            Select Case msg
                Case EC_USER
                    Debug.WriteLine("EC_USER: " & msg)
                    'look for right click event
                Case WM_DVD_EVENT
                    Debug.WriteLine("WM_DVD_EVENT: " & msg)
                    'If Not Player Is Nothing Then Player.BaseHandleEvent()
                Case WM_RBUTTONUP
                    'Debug.WriteLine("left button up")
                    RaiseEvent evRightClick()
                Case WM_MOUSEMOVE
                    RaiseEvent evMouseMove(lParam.ToInt32)
                Case WM_LBUTTONUP
                    RaiseEvent evLeftButtonUp(lParam.ToInt32)
                Case WM_LBUTTONDBLCLK
                    RaiseEvent evLeftButtonDoubleClick()
            End Select
            handled = False
            Return IntPtr.Zero
        End Function

        Friend Const HOST_ID As Integer = 2, WS_CHILD As Integer = 1073741824, WS_VISIBLE As Integer = 268435456, WS_CLIPCHILDREN As Integer = 33554432, CLIPSIBLINGS As Integer = 67108864

    End Class

End Namespace
