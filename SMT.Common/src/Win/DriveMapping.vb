Imports System.Runtime.InteropServices
Imports System.Text
Imports SMT.Win.WinAPI.MPR

Namespace Win

    Public Module DriveMapping

        Public Function MapNetworkDrive(ByVal UNC As String, ByVal DriveLetter As String) As eDriveMappingResult
            Try
                Dim myNetResource As New NETRESOURCE
                myNetResource.dwScope = 2       'RESOURCE_GLOBALNET        
                myNetResource.dwType = 1        'RESOURCETYPE_DISK        
                myNetResource.dwDisplayType = 3 'RESOURCEDISPLAYTYPE_SHARE        
                myNetResource.dwUsage = 1       'RESOURCEUSAGE_CONNECTABLE        
                myNetResource.LocalName = DriveLetter & ":"
                myNetResource.RemoteName = UNC
                myNetResource.Provider = Nothing
                Return WNetAddConnection2(myNetResource, Nothing, Nothing, 0)
            Catch ex As Exception
                Return eDriveMappingResult.ERROR_UNKNOWN
            End Try
        End Function

        Public Function RemoveDriveMap(ByVal DriveLetter As String) As eDriveMappingResult
            Try
                Return WNetCancelConnection2(DriveLetter & ":", 0, 0)
            Catch ex As Exception
                Return eDriveMappingResult.ERROR_UNKNOWN
            End Try
        End Function

        Public Enum eDriveMappingResult
            SUCCESS = 0
            ERROR_ACCESS_DENIED = 5& 'Access to the network resource was denied. 
            ERROR_ALREADY_ASSIGNED = 85& 'The local device specified by lpLocalName is already connected to a network resource. 
            ERROR_BAD_DEV_TYPE = 66& 'The type of local device and the type of network resource do not match. 
            ERROR_BAD_DEVICE 'The value specified by lpLocalName is invalid. 
            ERROR_BAD_NET_NAME = 67& 'The value specified by lpRemoteName is not acceptable to any network resource provider. The resource name is invalid, or the named resource cannot be located. 
            ERROR_BAD_PROFILE = 1206& 'The user profile is in an incorrect format. 
            ERROR_BAD_PROVIDER = 1204& 'The value specified by lpProvider does not match any provider. 
            ERROR_BUSY = 170& 'The router or provider is busy, possibly initializing. The caller should retry. 
            ERROR_CANCELLED 'The attempt to make the connection was cancelled by the user through a dialog box from one of the network resource providers, or by a called resource. 
            ERROR_CANNOT_OPEN_PROFILE = 1205& 'The system is unable to open the user profile to process persistent connections. 
            ERROR_DEVICE_ALREADY_REMEMBERED = 1202& 'An entry for the device specified in lpLocalName is already in the user profile. 
            ERROR_EXTENDED_ERROR = 1208& 'A network-specific error occured. Call the WNetGetLastError function to get a description of the error. 
            ERROR_INVALID_PASSWORD = 86& 'The specified password is invalid. 
            ERROR_NO_NET_OR_BAD_PATH = 1203& 'A network component has not started, or the specified name could not be handled. 
            ERROR_NO_NETWORK = 1222& 'There is no network present. 
            ERROR_CANCEL_VIOLATION = 173&
            ERROR_NO_CONNECTION = 8
            ERROR_NO_DISCONNECT = 9
            ERROR_DEVICE_IN_USE = 2404&
            ERROR_NOT_CONNECTED = 2250&
            ERROR_OPEN_FILES = 2401&
            ERROR_MORE_DATA = 234
            ERROR_UNKNOWN
        End Enum

        Const CONNECT_UPDATE_PROFILE = &H1
        Const RESOURCETYPE_DISK = &H1

        <StructLayout(LayoutKind.Sequential)> _
        Class NETRESOURCE
            Public dwScope As Integer
            Public dwType As Integer
            Public dwDisplayType As Integer
            Public dwUsage As Integer
            Public LocalName As String
            Public RemoteName As String
            Public Comment As String
            Public Provider As String
        End Class 'NETRESOURCE

        <DllImport("mpr.dll", SetLastError:=False, CharSet:=CharSet.Auto)> _
        Function WNetGetConnection(ByVal localName As String, ByVal remoteName As StringBuilder, ByRef remoteSize As Int32) As Int32
        End Function

    End Module

End Namespace

'Examples:

'If Microsoft.VisualBasic.Left(DVDDirectory, 2) = "\\" Then
'    If Me.MapADrive(DVDDirectory) Then
'        DVDDirectory = Me.MappedDriveLetter & "\"
'    Else
'        Throw New Exception("Unable to use UNC path: " & vbNewLine & DVDDirectory, Throw New ExceptionStyle.Critical Or Throw New ExceptionStyle.OKOnly)
'    End If
'End If



'#Region "DriveMapping"

'Public MappedDriveLetter As String = ""

'Public Function MapADrive(ByVal UNC As String) As Boolean
'    Try
'        'first find an available driveletter
'        Dim sb As New StringBuilder(300)
'        Dim size As Int32 = sb.Capacity
'        Dim LR As eAlphabet
'        For s As Byte = 6 To 25
'            LR = s
'            WNetGetConnection(LR.ToString & ":", sb, size)
'            If sb.ToString.Length > 0 Then
'                'we've got our letter
'                sb = Nothing
'                Exit For
'            Else
'                sb = New StringBuilder(300)
'            End If
'        Next

'        'use LR
'        If SMT.Common.Utilities.DriveMapping.MapNetworkDrive(UNC, LR.ToString) = mDriveMapping.eDriveMappingResult.SUCCESS Then
'            MappedDriveLetter = LR.ToString & ":"
'            Return True
'        Else
'            MappedDriveLetter = ""
'            Return False
'        End If
'    Catch ex As Exception
'        Me.AddConsoleLine(eConsoleItemType.ERROR, "Problem with MapADrive. Error: " & ex.Message, Nothing)
'        Return False
'    End Try
'End Function

'#End Region 'DriveMapping
