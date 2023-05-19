Imports Microsoft.Win32

Namespace Win

    Public Module ExtensionMapping

        '1. Determine an arbitrary file type name. This name is internal to Windows and will never be seen by the user. Common practice is to use something that is similar to the file’s extension. For example, for a “.russ” file, you might use “russfile” as this file type name. This name is completely arbitrary, but you want to avoid using one that is already used. For example, don’t use something like “batfile”, or you could really mess things up (I’m not even going to begin to go there!). 
        '2. Write this file type name to the Registry, associating it with your extension. Use this Sub: 

        Public Sub SetFileType(ByVal extension As String, ByVal FileType As String)
            Dim rk As RegistryKey = Microsoft.Win32.Registry.ClassesRoot
            Dim ext As RegistryKey = rk.CreateSubKey(extension)
            ext.SetValue("", FileType)
        End Sub

        '3. Now, add a description for your file type. Run this Sub, using the FileType you defined in step 2: 

        Public Sub SetFileDescription(ByVal FileType As String, ByVal Description As String)
            Dim rk As RegistryKey = Microsoft.Win32.Registry.ClassesRoot
            Dim ext As RegistryKey = rk.CreateSubKey(FileType)
            ext.SetValue("", Description)
        End Sub

        '4. Are we there yet? All that’s left is to add actions. These actions are what will show up when you right-click on your file. The Sub below adds one action, but it won’t do anything, yet. It simply adds the action to your list with the menu text. The “verb” in the Sub can really be anything, and is never seen by the user. The “ActionDescription” is seen, and if you want to set a hotkey, just add an “&” in the ActionDescription (such as "&Open"): 

        Public Sub AddAction(ByVal FileType As String, ByVal Verb As String, ByVal ActionDescription As String)
            Dim rk As RegistryKey = Microsoft.Win32.Registry.ClassesRoot
            Dim ext As RegistryKey = rk.OpenSubKey(FileType, True).CreateSubKey("Shell").CreateSubKey(Verb)
            ext.SetValue("", ActionDescription)
        End Sub

        'Note how I’ve strung together the “OpenSubKey” and “CreateSubKey” methods. You can use this technique for doing some shortcuts in these instructions. 

        '5. Now, we need to set this Action to do something. We are going to build now the command line that will get passed to your application. To make use of this, you will need to know how to use the “Environment.CommandLine” object in your application. Now, to set the command line: 

        Public Sub SetExtensionCommandLine(ByVal Command As String, ByVal FileType As String, ByVal CommandLine As String, Optional ByVal Name As String = "")
            Dim rk As RegistryKey = Microsoft.Win32.Registry.ClassesRoot
            Dim ext As RegistryKey = rk.OpenSubKey(FileType).OpenSubKey("Shell").OpenSubKey(Command, True).CreateSubKey("Command")
            ext.SetValue(Name, CommandLine)
        End Sub

        'Okay, now for a little bit of explaining—make sure you set “CommandLine” exactly as it should be as if you typed in the line from the MS-Dos or from the Command Prompt. Instead of using a File name, however, use “%1”. So, if you normally were to run your application so that it would open file “MyFile.russ” by typing in “c:\MyAppPath\MyApp.exe MyFile.russ”, then the CommandLine should be: 
        'Code:“c:\MyAppPath\MyApp.exe” “%1”
        'The double quotes are important to add to the CommandLine string. The reason is quite simple: if the path or file name has spaces, the quotes make sure that your application will properly recognize the parameters and that Windows will properly recognize your application’s path. The quotes around %1 are not necessary if you only use “Enviornment.CommandLine”. If you use “Environment.CommandLineArgs”, you had better use the quotes around “%1”, or else you’ll have a mess on your hands. 

        '6. Almost done. If you do not specifically specify a default action, the default will be the verb that is named “open”. The default action is what will occur when you double-click on your file. To set a default action, run this Sub: 

        Public Sub SetDefaultAction(ByVal FileType As String, ByVal Verb As String)
            Dim rk As RegistryKey = Microsoft.Win32.Registry.ClassesRoot
            Dim ext As RegistryKey = rk.OpenSubKey(FileType).OpenSubKey("Shell")
            ext.SetValue("", Verb)
        End Sub

        'The “Verb” in the above Sub must be the same as the “Verb” in the “AddAction” Sub. Setting a default action is completely optional. 

        '7. Now to our last step! This step is optional. If you want to set an icon to appear next to files of your new type, you only need to run this Sub: 

        Public Sub SetDefaultIcon(ByVal FileType As String, ByVal Icon As String)
            Dim rk As RegistryKey = Microsoft.Win32.Registry.ClassesRoot
            Dim ext As RegistryKey = rk.OpenSubKey(FileType)
            ext.SetValue("DefaultIcon", Icon)
        End Sub

        Public Function SetAssociatedProgram(ByVal FileExtension As String, ByVal ProgramPath As String, ByVal FileTypeName As String, ByVal FileTypeDescription As String, ByVal ActionDescription As String) As Boolean
            Try
                SetFileType(FileExtension, FileTypeName)
                SetFileDescription(FileTypeName, FileTypeDescription)
                AddAction(FileTypeName, "open", ActionDescription)
                SetExtensionCommandLine("open", FileTypeName, Chr(34) & ProgramPath & Chr(34) & " " & Chr(34) & "%1" & Chr(34), )
                'SetDefaultAction(FileTypeName, "Open")
                Return True
            Catch ex As Exception
                Throw New Exception("SetAssociatedProgram failed. Error: " & ex.Message)
            End Try
        End Function

        Public Function GetAssociatedProgram(ByVal FileExtension As String) As String
            ' Returns the application associated with the specified
            ' FileExtension
            ' ie, path\denenv.exe for "VB" files
            Dim objExtReg As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.ClassesRoot
            Dim objAppReg As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.ClassesRoot
            Dim strExtValue As String
            Try
                ' Add trailing period if doesn't exist
                If FileExtension.Substring(0, 1) <> "." Then FileExtension = "." & FileExtension

                ' Open registry areas containing launching app details
                objExtReg = objExtReg.OpenSubKey(FileExtension.Trim)
                strExtValue = objExtReg.GetValue("")
                objAppReg = objAppReg.OpenSubKey(strExtValue & "\shell\open\command")

                Return objAppReg.GetValue(Nothing)

                '' Parse out, tidy up and return result
                'Dim SplitArray() As String
                'SplitArray = Split(objAppReg.GetValue(Nothing), """")
                'If SplitArray(0).Trim.Length > 0 Then
                '    Return SplitArray(0).Replace("%1", "")
                'Else
                '    Return SplitArray(1).Replace("%1", "")
                'End If
            Catch ex As Exception
                Throw New Exception("GetAssociatedProgram failed. Error: " & ex.Message)
                Return ""
            End Try
        End Function

    End Module

End Namespace
