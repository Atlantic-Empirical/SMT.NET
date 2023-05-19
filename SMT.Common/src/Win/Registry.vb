Imports Microsoft.Win32

Namespace Win

    Public Module Registry

        Public Function SetHKLMKey(ByVal KeyPath As String, ByVal OptionName As String, ByVal OptionValue As Object) As Boolean
            Try
                'Example KeyPath = "Software\Sequoyan\Phoenix\EmulatorSetup"
                Dim rKey As RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(KeyPath, True)
                If rKey Is Nothing Then
                    rKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(KeyPath)
                End If
                rKey.SetValue(OptionName, OptionValue)
                rKey.Close()
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with SetHKLMKey. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Function GetHKLMKey(ByVal KeyPath As String, ByVal ValName As String) As Object
            Try
TryGetRegUserOptionAgain:
                Dim rKey As RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(KeyPath, True)
                If rKey Is Nothing Then
                    Return Nothing
                End If
                Dim O As Object = rKey.GetValue(ValName)
                rKey.Close()
                If O Is Nothing Then Return Nothing
                Return O
            Catch ex As Exception
                Throw New Exception("Problem with GetHKLMKey. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Function SetHKCUKey(ByVal KeyPath As String, ByVal OptionName As String, ByVal OptionValue As Object) As Boolean
            Try
                'Example KeyPath = "Software\Sequoyan\Phoenix\EmulatorSetup"
                Dim rKey As RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(KeyPath, True)
                If rKey Is Nothing Then
                    rKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(KeyPath)
                End If
                rKey.SetValue(OptionName, OptionValue)
                rKey.Close()
                Return True
            Catch ex As Exception
                Debug.WriteLine("Problem with SetHKCUKey. Error: " & ex.Message)
                Return Nothing
            End Try
        End Function

        Public Function GetHKCUKey(ByVal KeyPath As String, ByVal ValName As String) As Object
            Try
TryGetRegUserOptionAgain:
                Dim rKey As RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(KeyPath, True)
                If rKey Is Nothing Then
                    Return Nothing
                End If
                Dim O As Object = rKey.GetValue(ValName)
                rKey.Close()
                If O Is Nothing Then Return Nothing
                Return O
            Catch ex As Exception
                Debug.WriteLine("Problem with GetHKCUKey. Error: " & ex.Message)
                Return Nothing
            End Try
        End Function

        Public Function SetHKCRKey(ByVal KeyPath As String, ByVal OptionName As String, ByVal OptionValue As Object) As Boolean
            Try
                'Example KeyPath = "Software\Sequoyan\Phoenix\EmulatorSetup"
                Dim rKey As RegistryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(KeyPath, True)
                If rKey Is Nothing Then
                    rKey = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(KeyPath)
                End If
                rKey.SetValue(OptionName, OptionValue)
                rKey.Close()
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with SetHKCRKey(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function GetHKCRKey(ByVal KeyPath As String, ByVal ValName As String) As Object
            Try
TryGetRegUserOptionAgain:
                Dim rKey As RegistryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(KeyPath, True)
                If rKey Is Nothing Then
                    Return Nothing
                End If
                Dim O As Object = rKey.GetValue(ValName)
                rKey.Close()
                If O Is Nothing Then Return Nothing
                Return O
            Catch ex As Exception
                Debug.WriteLine("Problem with GetHKCRKey(). Error: " & ex.Message)
                Return Nothing
            End Try
        End Function

        Public Sub DeleteHKCRKey(ByVal KeyPath As String)
            Try
                Microsoft.Win32.Registry.ClassesRoot.DeleteSubKeyTree(KeyPath)
            Catch ex As Exception
                Debug.WriteLine("Problem with DeleteHKCRKey(). Error: " & ex.Message)
            End Try
        End Sub

    End Module

End Namespace
