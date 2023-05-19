Imports System.Runtime.InteropServices
Imports System.Security.Principal

Namespace Win.UserAccounts

    Public Class cLogonUtility

        'import LSA functions
        <DllImport("advapi32.dll")> _
        Private Shared Function LogonUser(ByVal lpszUsername As [String], ByVal lpszDomain As [String], ByVal lpszPassword As [String], ByVal dwLogonType As Integer, ByVal dwLogonProvider As Integer, ByRef phToken As IntPtr) As Boolean
        End Function

        <DllImport("advapi32.dll")> _
        Private Shared Function DuplicateToken(ByVal ExistingTokenHandle As IntPtr, ByVal ImpersonationLevel As Integer, ByRef DuplicateTokenHandle As IntPtr) As Boolean
        End Function

        <DllImport("kernel32.dll")> _
        Private Shared Function CloseHandle(ByVal hObject As IntPtr) As Boolean
        End Function

        <DllImport("advapi32.dll")> _
        Private Shared Function ImpersonateLoggedOnUser(ByVal hToken As IntPtr) As Boolean
        End Function

        <DllImport("kernel32.dll")> _
        Private Shared Function GetLastError() As Integer
        End Function

        'enum impersonation levels an logon types
        Private Enum SecurityImpersonationLevel
            SecurityAnonymous
            SecurityIdentification
            SecurityImpersonation
            SecurityDelegation
        End Enum 'SecurityImpersonationLevel

        Private Enum LogonTypes
            LOGON32_PROVIDER_DEFAULT = 0
            LOGON32_LOGON_INTERACTIVE = 2
            LOGON32_LOGON_NETWORK = 3
            LOGON32_LOGON_BATCH = 4
            LOGON32_LOGON_SERVICE = 5
            LOGON32_LOGON_UNLOCK = 7
            LOGON32_LOGON_NETWORK_CLEARTEXT = 8
            LOGON32_LOGON_NEW_CREDENTIALS = 9
        End Enum 'LogonTypes

        '/ <summary>impersonates a user</summary>
        '/ <param name="sUsername">domain\name of the user account</param>
        '/ <param name="sPassword">the user's password</param>
        '/ <returns>the new WindowsImpersonationContext</returns>
        Public Shared Function ImpersonateUser(ByVal username As [String], ByVal password As [String]) As WindowsImpersonationContext
            'define the handles
            Dim existingTokenHandle As IntPtr = IntPtr.Zero
            Dim duplicateTokenHandle As IntPtr = IntPtr.Zero

            Dim domain As [String]
            If username.IndexOf("\") > 0 Then
                'split domain and name
                Dim splitUserName As [String]() = username.Split("\"c)
                domain = splitUserName(0)
                username = splitUserName(1)
            Else
                domain = [String].Empty
            End If

            Dim isOkay As Boolean = True

            Try
                'get a security token
                isOkay = LogonUser(username, domain, password, CInt(LogonTypes.LOGON32_LOGON_INTERACTIVE), CInt(LogonTypes.LOGON32_PROVIDER_DEFAULT), existingTokenHandle)

                If Not isOkay Then
                    Dim lastWin32Error As Integer = Marshal.GetLastWin32Error()
                    Dim lastError As Integer = GetLastError()

                    Throw New Exception("LogonUser Failed: " + lastWin32Error + " - " + lastError)
                End If

                ' copy the token
                isOkay = DuplicateToken(existingTokenHandle, CInt(SecurityImpersonationLevel.SecurityImpersonation), duplicateTokenHandle)

                If Not isOkay Then
                    Dim lastWin32Error As Integer = Marshal.GetLastWin32Error()
                    Dim lastError As Integer = GetLastError()
                    CloseHandle(existingTokenHandle)
                    Throw New Exception("DuplicateToken Failed: " + lastWin32Error + " - " + lastError)
                Else

                    ' create an identity from the token
                    Dim newId As New WindowsIdentity(duplicateTokenHandle)
                    Dim impersonatedUser As WindowsImpersonationContext = newId.Impersonate()

                    Return impersonatedUser
                End If
            Catch ex As Exception
                Throw ex
            Finally
                'free all handles
                If existingTokenHandle <> IntPtr.Zero Then
                    CloseHandle(existingTokenHandle)
                End If
                If duplicateTokenHandle <> IntPtr.Zero Then
                    CloseHandle(duplicateTokenHandle)
                End If
            End Try
        End Function 'ImpersonateUser

    End Class 'LogonUtility

End Namespace 'LogonDemo
