Imports System.Runtime.InteropServices
Imports System.Reflection
Imports SMT.Win.WinAPI.User32
Imports SMT.Win.WinAPI.Constants.HookFlags
Imports SMT.Win.WinAPI.Kernel32

Namespace Win.KeyboardHooking

    Public Class cKeyboardHook

        Private Parent As System.Windows.Forms.Form
        Public ParentContainsFocusOverride As Boolean = False

        Public Sub New(ByRef P As System.Windows.Forms.Form)
            Parent = P
        End Sub

        Public KeyboardHandle As Integer

        '' Implement this function to block as many key combinations as you'd like
        'Public Function IsHooked(ByRef Hookstruct As KBDLLHOOKSTRUCT) As Boolean

        '    Debug.WriteLine("Hookstruct.vkCode: " & Hookstruct.vkCode)
        '    Debug.WriteLine(Hookstruct.vkCode = VK_ESCAPE)
        '    Debug.WriteLine(Hookstruct.vkCode = VK_TAB)

        '    If (Hookstruct.vkCode = VK_ESCAPE) And CBool(GetAsyncKeyState(VK_CONTROL) And &H8000) Then
        '        Call HookedState("Ctrl + Esc blocked")
        '        Return True
        '    End If

        '    If (Hookstruct.vkCode = VK_TAB) And CBool(Hookstruct.flags And LLKHF_ALTDOWN) Then
        '        Call HookedState("Alt + Tab blockd")
        '        Return True
        '    End If

        '    If (Hookstruct.vkCode = VK_ESCAPE) And CBool(Hookstruct.flags And LLKHF_ALTDOWN) Then
        '        Call HookedState("Alt + Escape blocked")
        '        Return True
        '    End If

        '    Return False
        'End Function

        'Private Sub HookedState(ByVal Text As String)
        '    Debug.WriteLine(Text)
        'End Sub

        Public Function KeyboardCallback(ByVal Code As Integer, ByVal wParam As Integer, ByRef lParam As KBDLLHOOKSTRUCT) As Integer
            Try
                'now done below
                ''or when it includes the alt key
                'If wParam = 260 Or wParam = 257 Then
                '    Return CallNextHookEx(KeyboardHandle, Code, wParam, lParam)
                'End If

                'pass the key stroke through when this hook is paused
                'or if it is either of the shift keys, either of the alt keys, or either of the control keys
                If _PauseHook Or wParam = &H104 Or wParam = &H105 Or wParam = &H101 Or lParam.vkCode = 162 Or lParam.vkCode = 163 Then
                    'Debug.WriteLine("Passing keypress through " & wParam & " " & lParam.vkCode)
                    Return CallNextHookEx(KeyboardHandle, Code, wParam, lParam)
                End If

                If (Code = HC_ACTION) And (Parent.ContainsFocus Or ParentContainsFocusOverride) Then
                    'Debug.WriteLine("Processing keypress " & wParam & " " & lParam.vkCode)
                    RaiseEvent KeyPress(Code, wParam, lParam, wParam, New cKBDLLHOOKSTRUCT(lParam))
                    Return 1 'this prevents any lower hooks from processing the keystroke, if we return 1 here then Phoenix will not get the key press on the UI
                Else
                    Return CallNextHookEx(KeyboardHandle, Code, wParam, lParam)
                End If
            Catch ex As Exception
                Throw New Exception("Problem with KeyboardCallback. Error: " & ex.Message, ex)
            End Try
        End Function

        'TPF ADDED THIS:
        'Public Event KeyPress(ByVal Code As Integer, ByVal wParam As Integer, ByVal lParam As KBDLLHOOKSTRUCT)
        Public Event KeyPress(ByVal Code As Integer, ByVal wParam As Integer, ByVal lParam As KBDLLHOOKSTRUCT, ByVal wParam As eWParam, ByVal lParam As cKBDLLHOOKSTRUCT)

        Public Class cKBDLLHOOKSTRUCT
            Public vkCode As Integer
            Public scanCode As Integer
            Public flags As cHOOKSTRUCT_Flags
            Public time As Integer
            Public dwExtraInfo As Integer

            Public Sub New(ByVal KB As KBDLLHOOKSTRUCT)
                Me.vkCode = KB.vkCode
                Me.scanCode = KB.scanCode
                Me.flags = New cHOOKSTRUCT_Flags(KB.flags)
                Me.time = KB.time
                Me.dwExtraInfo = KB.dwExtraInfo
            End Sub

            Public Overloads Overrides Function ToString() As String
                Return "vkCode: " & vkCode & vbNewLine & _
                "ScanCode: " & scanCode & vbNewLine & _
                "Flags" & vbNewLine & _
                vbTab & "Extended Key: " & flags.EXTENDED_KEY.ToString & vbNewLine & _
                vbTab & "Injected Key: " & flags.INJECTED_KEY.ToString & vbNewLine & _
                vbTab & "Alt Pressed: " & flags.ALT_PRESSED.ToString & vbNewLine & _
                vbTab & "Translation State: " & flags.TranlationState.ToString & vbNewLine & _
                "Time: " & time & vbNewLine & _
                "dwExtraInfo: " & dwExtraInfo
            End Function

        End Class

        Public Class cHOOKSTRUCT_Flags
            Public EXTENDED_KEY As Boolean
            Public INJECTED_KEY As Boolean
            Public ALT_PRESSED As Boolean
            Public TranlationState As eKeyTranslationState

            Public Sub New(ByVal nFlags As Integer)
                Me.EXTENDED_KEY = nFlags And 1
                Me.INJECTED_KEY = (nFlags >> 4) And 1
                Me.ALT_PRESSED = (nFlags >> 5) And 1
                Me.TranlationState = (nFlags >> 7) And 1
            End Sub

        End Class

        Public Enum eKeyTranslationState
            Pressed
            Released
        End Enum

        Public Enum eWParam
            WM_KEYDOWN = &H100
            WM_KEYUP = &H101
            WM_SYSKEYDOWN = &H104
            WM_SYSKEYUP = &H105
        End Enum

        Public _PauseHook As Boolean = False

        Public Sub PauseHook()
            _PauseHook = True
        End Sub

        Public Sub UnpauseHook()
            _PauseHook = False
            ParentContainsFocusOverride = False
        End Sub

        <MarshalAs(UnmanagedType.FunctionPtr)> _
        Private callback As KeyboardHookDelegate

        Public Function HookKeyboard() As Boolean
            callback = New KeyboardHookDelegate(AddressOf KeyboardCallback)

            KeyboardHandle = SMT.Win.WinAPI.User32.SetWindowsHookEx( _
              WH_KEYBOARD_LL, callback, _
              Marshal.GetHINSTANCE([Assembly].GetExecutingAssembly.GetModules()(0)).ToInt32, 0)

            Debug.WriteLine("err: " & GetLastError)

            If HasHandle() Then
                Debug.WriteLine("Keyboard hooked")
                Return True
            Else
                Debug.WriteLine("Keyboard hook failed: " & Err.LastDllError)
                Return False
            End If
        End Function

        Public ReadOnly Property HasHandle() As Boolean
            Get
                Return KeyboardHandle <> 0
            End Get
        End Property

        Public ReadOnly Property HookIsActive() As Boolean
            Get
                Return (KeyboardHandle <> 0) And Not _PauseHook
            End Get
        End Property

        Public ReadOnly Property IsPaused() As Boolean
            Get
                Return Me._PauseHook
            End Get
        End Property

        Public Sub UnhookKeyboard()
            Try
                If (HasHandle()) Then
                    Dim i As Integer = UnhookWindowsHookEx(KeyboardHandle)
                    If i = 0 Then
                        Dim e As Integer = GetLastError
                        Throw New Exception("UnhookWindowsHookEx failed.")
                    Else
                        KeyboardHandle = 0
                    End If
                    'Call UnhookWindowsHookEx(KeyboardHandle)
                End If
            Catch ex As Exception
                Throw New Exception("Problem with UnhookKeyboard. Error: " & ex.Message)
            End Try
        End Sub

        Public Function GetStringForErrorCode(ByVal Code As Integer) As String
            Try
                Dim Buffer As String
                Buffer = Space(200)
                Return FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, 0, GetLastError, LANG_NEUTRAL, Buffer, 200, 0)
            Catch ex As Exception
                Throw New Exception("Problem with GetStringForErrorCode. Error: " & ex.Message)
            End Try
        End Function

    End Class

End Namespace
