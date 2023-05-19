Imports System.Text
Imports Microsoft.Win32.Security
Imports System.IO
Imports SMT.Win.WinAPI.Kernel32
Imports System.Globalization

Namespace Win.Filesystem

    Public Module mFilesystem

        Public Function GetFreeSpace(ByVal Drive As String) As Long
            'returns free space in MB, formatted to two decimal places
            'e.g., Throw New Exception("Free Space on C: "& GetFreeSpace("C:\") & "MB")
            Dim lBytesTotal, lFreeBytes, lFreeBytesAvailable As Long
            Dim iAns As Long
            iAns = GetDiskFreeSpaceEx(Drive, lFreeBytesAvailable, lBytesTotal, lFreeBytes)
            If iAns > 0 Then
                Return BytesToMegabytes(lFreeBytes)
            Else
                Throw New Exception("Invalid or unreadable drive")
            End If
        End Function

        Public Function GetTotalSpace(ByVal Drive As String) As String
            'returns total space in MB, formatted to two decimal places
            'e.g., Throw New Exception("Free Space on C: "& GetTotalSpace("C:\") & "MB")

            Dim lBytesTotal, lFreeBytes, lFreeBytesAvailable As Long
            Dim iAns As Long
            iAns = GetDiskFreeSpaceEx(Drive, lFreeBytesAvailable, lBytesTotal, lFreeBytes)
            If iAns > 0 Then
                Return BytesToMegabytes(lBytesTotal)
            Else
                Throw New Exception("Invalid or unreadable drive")
            End If
        End Function

        Private Function BytesToMegabytes(ByVal Bytes As Long) As Long
            Dim dblAns As Double
            dblAns = (Bytes / 1024) / 1024
            Return Format(dblAns, "###,###,##0.00")
        End Function

        Public Function GetVolumeLabel(ByVal DriveLetter As String) As String
            Try
                Dim serNum As UInt32
                Dim maxCompLen As UInt32
                Dim VolLabel As New StringBuilder(32)
                Dim VolFlags As UInt32
                Dim FSName As New StringBuilder("")
                DriveLetter &= ":\\"
                Dim Ret As Boolean = GetVolumeInformation(DriveLetter, VolLabel, VolLabel.Capacity, serNum, maxCompLen, VolFlags, FSName, 256)
                Return VolLabel.ToString
            Catch ex As Exception
                Return "Problem getting VolumeLabel. Error: " & ex.Message
            End Try
        End Function

        Public Function GetVolumeSerial(ByVal strDriveLetter As String) As UInteger
            Try
                If strDriveLetter = "" OrElse strDriveLetter Is Nothing Then
                    strDriveLetter = "C"
                End If
                Dim disk As New System.Management.ManagementObject("win32_logicaldisk.deviceid=""" + strDriveLetter + ":""")
                disk.Get()
                'Debug.WriteLine(disk("VolumeSerialNumber").ToString())
                Return UInt32.Parse(disk("VolumeSerialNumber").ToString, NumberStyles.HexNumber)
            Catch ex As Exception
                Return "Error: " & ex.Message
            End Try
        End Function

        Public Function CheckDriveType(ByVal nPath As String) As eDriveType
            Dim s As String = Path.GetPathRoot(nPath)
            Return GetDriveType(s)
        End Function

        Public Enum eDriveType
            UNKNOWN
            NO_ROOT_DIR 'The root directory does not exist.
            REMOVABLE 'The disk can be removed from the drive.
            FIXED 'The disk cannot be removed from the drive.
            REMOTE 'The drive is a remote (network) drive.
            CDROM 'The drive is a CD-ROM drive.
            RAMDISK 'The drive is a RAM disk.
        End Enum

        'Public Function SetFileSystemObjectPermission(ByVal strSitePath As String, ByVal strUserName As String) As Boolean
        '    Dim bOk As Boolean
        '    Try
        '        Dim secDesc As SecurityDescriptor = SecurityDescriptor.GetFileSecurity(strSitePath, SECURITY_INFORMATION.DACL_SECURITY_INFORMATION) '
        '        Dim dacl As Dacl = secDesc.Dacl
        '        Dim sidUser As New Sid(strUserName)

        '        ' deny: this folder 
        '        ' write attribs 
        '        ' write extended attribs 
        '        ' delete 
        '        ' change permissions 
        '        ' take ownership 
        '        Dim DAType As DirectoryAccessType = DirectoryAccessType.FILE_WRITE_ATTRIBUTES Or DirectoryAccessType.FILE_WRITE_EA Or DirectoryAccessType.DELETE Or DirectoryAccessType.WRITE_OWNER Or DirectoryAccessType.WRITE_DAC
        '        Dim AType As AccessType = CType(DAType, AccessType)
        '        dacl.AddAce(New AceAccessDenied(New Sid(strUserName), AType))

        '        ' allow: folder, subfolder and files 
        '        ' modify 
        '        dacl.AddAce(New AceAccessAllowed(sidUser, AccessType.GENERIC_WRITE Or AccessType.GENERIC_READ Or AccessType.DELETE Or AccessType.GENERIC_EXECUTE, AceFlags.OBJECT_INHERIT_ACE Or AceFlags.CONTAINER_INHERIT_ACE))

        '        secDesc.SetDacl(dacl)
        '        secDesc.SetFileSecurity(strSitePath, SECURITY_INFORMATION.DACL_SECURITY_INFORMATION)
        '        bOk = True
        '        Console.WriteLine("Permissions set for " & strUserName & " on " & strSitePath)
        '    Catch e As Exception
        '        Console.WriteLine(e.ToString())
        '        bOk = False
        '    End Try
        '    Return bOk
        'End Function 'CreateACLonStore ' CreateACLonStore 

    End Module

    Public Module FileHandlingFunctions

        Public Function SaveStringToFile(ByVal InStr As String, ByVal FilePath As String) As Boolean
            Try
                Dim FS As New FileStream(FilePath, FileMode.OpenOrCreate)
                Dim SW As New StreamWriter(FS)
                SW.Write(InStr)
                SW.Close()
                FS.Close()
                FS = Nothing
                SW = Nothing
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with SaveStringToFile. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public Function GetStringFromFile(ByVal FilePath As String) As String
            Try
                Dim FS As New FileStream(FilePath, FileMode.Open)
                Dim StR As New StreamReader(FS)
                Dim out As String = StR.ReadToEnd
                StR.Close()
                FS.Close()
                Return out
            Catch ex As Exception
                Throw New Exception("Problem with GetStringFromFile. Error: " & ex.Message)
                Return Nothing
            End Try
        End Function

    End Module

    Public Class cGetOldestFileInDir
        Implements IComparer

        Public Function Compare(ByVal X As Object, ByVal Y As Object) As Integer Implements IComparer.Compare
            Dim File1 As FileInfo = DirectCast(X, FileInfo)
            Dim File2 As FileInfo = DirectCast(Y, FileInfo)
            Return Date.Compare(File1.CreationTime, File2.CreationTime)
        End Function

    End Class

    Public Class cFilePath

        Public FullPath As String

        Public Sub New(ByVal nFullPath As String)
            FullPath = nFullPath
        End Sub

        Public ReadOnly Property FileName() As String
            Get
                Return Path.GetFileName(FullPath)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return FileName
        End Function

        Public ReadOnly Property Ext_LowerWithoutPeriod()
            Get
                Return Replace(Path.GetExtension(FullPath).ToLower, ".", "")
            End Get
        End Property

        Public ReadOnly Property IsMultimediaVideoElementaryStream() As Boolean
            Get
                For Each s As String In [Enum].GetNames(GetType(SMT.Multimedia.Enums.eVideoElementaryStreamType))
                    If Ext_LowerWithoutPeriod = s.ToLower Then
                        Return True
                    End If
                Next
            End Get
        End Property

        Public ReadOnly Property IsMultimediaAudioElementaryStream() As Boolean
            Get
                For Each s As String In [Enum].GetNames(GetType(SMT.Multimedia.Enums.eAudioElementaryStreamType))
                    If Ext_LowerWithoutPeriod = s.ToLower Then
                        Return True
                    End If
                Next
            End Get
        End Property

        Public ReadOnly Property IsMultimediaProgramStream() As Boolean
            Get
                For Each s As String In [Enum].GetNames(GetType(SMT.Multimedia.Enums.eProgramStreamType))
                    If Ext_LowerWithoutPeriod = s.ToLower Then
                        Return True
                    End If
                Next
            End Get
        End Property

        Public ReadOnly Property IsMultimediaTransportStream() As Boolean
            Get
                For Each s As String In [Enum].GetNames(GetType(SMT.Multimedia.Enums.eTransportStreamType))
                    If Ext_LowerWithoutPeriod = s.ToLower Then
                        Return True
                    End If
                Next
            End Get
        End Property

    End Class

End Namespace
