
Namespace Win.WinAPI.Constants

    Public Module WindowStyles

        ' Public Constants from WinUser.h
        Public Const GWL_STYLE As Integer = -16
        Public Const GWL_EXSTYLE As Integer = -20

        Public Const SWP_NOSIZE As System.UInt32 = &H1 'ToDo: Unsigned Integers not supported
        Public Const SWP_NOMOVE As System.UInt32 = &H2 'ToDo: Unsigned Integers not supported
        Public Const SWP_NOZORDER As System.UInt32 = &H4 'ToDo: Unsigned Integers not supported
        Public Const SWP_NOREDRAW As System.UInt32 = &H8 'ToDo: Unsigned Integers not supported
        Public Const SWP_NOACTIVATE As System.UInt32 = &H10 'ToDo: Unsigned Integers not supported
        Public Const SWP_FRAMECHANGED As System.UInt32 = &H20 'ToDo: Unsigned Integers not supported
        Public Const SWP_SHOWWINDOW As System.UInt32 = &H40 'ToDo: Unsigned Integers not supported
        Public Const SWP_HIDEWINDOW As System.UInt32 = &H80 'ToDo: Unsigned Integers not supported
        Public Const SWP_NOCOPYBITS As System.UInt32 = &H100 'ToDo: Unsigned Integers not supported
        Public Const SWP_NOOWNERZORDER As System.UInt32 = &H200 'ToDo: Unsigned Integers not supported
        Public Const SWP_NOSENDCHANGING As System.UInt32 = &H400 'ToDo: Unsigned Integers not supported

        Public Const WS_OVERLAPPED = &H0L
        Public Const WS_POPUP = &H80000000L
        Public Const WS_CHILD = &H40000000L
        Public Const WS_MINIMIZE = &H20000000L
        Public Const WS_VISIBLE = &H10000000L
        Public Const WS_DISABLED = &H8000000L
        Public Const WS_CLIPSIBLINGS = &H4000000L
        Public Const WS_CLIPCHILDREN = &H2000000L
        Public Const WS_MAXIMIZE = &H1000000L
        Public Const WS_CAPTION = &HC00000L     '/* WS_BORDER | WS_DLGFRAME  */
        Public Const WS_BORDER = &H800000L
        Public Const WS_DLGFRAME = &H400000L
        Public Const WS_VSCROLL = &H200000L
        Public Const WS_HSCROLL = &H100000L
        Public Const WS_SYSMENU = &H80000L
        Public Const WS_THICKFRAME = &H40000L
        Public Const WS_GROUP = &H20000L
        Public Const WS_TABSTOP = &H10000L
        Public Const WS_MINIMIZEBOX = &H20000L
        Public Const WS_MAXIMIZEBOX = &H10000L

        Public Const WS_EX_DLGMODALFRAME = &H1L
        Public Const WS_EX_NOPARENTNOTIFY = &H4L
        Public Const WS_EX_TOPMOST = &H8L
        Public Const WS_EX_ACCEPTFILES = &H10L
        Public Const WS_EX_TRANSPARENT = &H20L
        Public Const WS_EX_MDICHILD = &H40L
        Public Const WS_EX_TOOLWINDOW = &H80L
        Public Const WS_EX_WINDOWEDGE = &H100L
        Public Const WS_EX_CLIENTEDGE = &H200L
        Public Const WS_EX_CONTEXTHELP = &H400L
        Public Const WS_EX_RIGHT = &H1000L
        Public Const WS_EX_LEFT = &H0L
        Public Const WS_EX_RTLREADING = &H2000L
        Public Const WS_EX_LTRREADING = &H0L
        Public Const WS_EX_LEFTSCROLLBAR = &H4000L
        Public Const WS_EX_RIGHTSCROLLBAR = &H0L
        Public Const WS_EX_CONTROLPARENT = &H10000L
        Public Const WS_EX_STATICEDGE = &H20000L
        Public Const WS_EX_APPWINDOW = &H40000L
        Public Const WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE Or WS_EX_CLIENTEDGE)
        Public Const WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE Or WS_EX_TOOLWINDOW Or WS_EX_TOPMOST)
        Public Const WS_EX_LAYERED = &H80000
        Public Const WS_EX_NOINHERITLAYOUT = &H100000L ' Disable inheritence of mirroring by children
        Public Const WS_EX_LAYOUTRTL = &H400000L ' Right to left mirroring
        Public Const WS_EX_COMPOSITED = &H2000000L
        Public Const WS_EX_NOACTIVATE = &H8000000L

        Public Const SW_HIDE = 0
        Public Const SW_SHOWNORMAL = 1
        Public Const SW_NORMAL = 1
        Public Const SW_SHOWMINIMIZED = 2
        Public Const SW_SHOWMAXIMIZED = 3
        Public Const SW_MAXIMIZE = 3
        Public Const SW_SHOWNOACTIVATE = 4
        Public Const SW_SHOW = 5
        Public Const SW_MINIMIZE = 6
        Public Const SW_SHOWMINNOACTIVE = 7
        Public Const SW_SHOWNA = 8
        Public Const SW_RESTORE = 9
        Public Const SW_SHOWDEFAULT = 10
        Public Const SW_FORCEMINIMIZE = 11
        Public Const SW_MAX = 11

        Public Const HWND_TOPMOST As Short = -1
        Public Const HWND_NOTOPMOST As Short = -2

        Public Const SPI_SETSCREENSAVEACTIVE = 17

    End Module

    Public Module HookFlags

        Public Structure KBDLLHOOKSTRUCT
            Public vkCode As Integer
            Public scanCode As Integer
            Public flags As Integer
            Public time As Integer
            Public dwExtraInfo As Integer
        End Structure

        ' Low-Level Keyboard Public Constants
        Public Const HC_ACTION As Integer = 0
        Public Const LLKHF_EXTENDED As Integer = &H1
        Public Const LLKHF_INJECTED As Integer = &H10
        Public Const LLKHF_ALTDOWN As Integer = &H20
        Public Const LLKHF_UP As Integer = &H80

        ' Virtual Keys
        Public Const VK_TAB = &H9
        Public Const VK_CONTROL = &H11
        Public Const VK_ESCAPE = &H1B
        Public Const VK_DELETE = &H2E

        Public Const WH_KEYBOARD_LL As Integer = 13&

    End Module

    ''' <summary>
    ''' From Winuser.h
    ''' </summary>
    ''' <remarks></remarks>
    Public Module WindowEvents

        Public Const WM_APP As Integer = &H8000
        Public Const WM_GRAPHNOTIFY As Integer = WM_APP + 1
        Public Const WM_DVD_EVENT As Integer = &H8002 ' message from dvd graph
        Public Const WM_USER As Integer = &H400
        Public Const WM_ENTERMENULOOP As Integer = &H211
        Public Const WM_EXITMENULOOP As Integer = &H212
        Public Const WM_COPYDATA As Integer = &H4A
        Public Const WM_DESTROY As Integer = &H2

        Public Const WM_MOUSEFIRST = &H200
        Public Const WM_MOUSEMOVE = &H200
        Public Const WM_LBUTTONDOWN = &H201
        Public Const WM_LBUTTONUP = &H202
        Public Const WM_LBUTTONDBLCLK = &H203
        Public Const WM_RBUTTONDOWN = &H204
        Public Const WM_RBUTTONUP = &H205
        Public Const WM_RBUTTONDBLCLK = &H206
        Public Const WM_MBUTTONDOWN = &H207
        Public Const WM_MBUTTONUP = &H208
        Public Const WM_MBUTTONDBLCLK = &H209
        Public Const WM_MOUSEWHEEL = &H20A
        Public Const WM_XBUTTONDOWN = &H20B
        Public Const WM_XBUTTONUP = &H20C
        Public Const WM_XBUTTONDBLCLK = &H20D
        Public Const WM_MOUSEHWHEEL = &H20E

    End Module

    Public Module EventCodes

        Public EC_USER As Integer = &H8000
        Public EC_IDLE As Integer = EC_USER + 4

    End Module

End Namespace
