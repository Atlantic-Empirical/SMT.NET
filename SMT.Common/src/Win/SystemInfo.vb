Imports System.Management
Imports System.Security

Namespace Win

    Public Class SystemInfo

        Private Shared _Initialized As Boolean = False
        Private Shared _Mgmt As ManagementObject
        Private Shared _ComputerName As String = ""
        Private Shared _Manufacturer As String = ""
        Private Shared _Model As String = ""
        Private Shared _OSName As String = ""
        Private Shared _OSVersion As String = ""
        Private Shared _SystemType As String = ""
        Private Shared _TPM As String = ""
        Private Shared _Username As String = ""
        Private Shared _WindowsDir As String = ""

        ''' <summary>
        ''' Intializes private shared data by scanning Operating System 
        ''' and Computer System information.
        ''' </summary>
        Private Shared Sub Initialize()
            Dim mgmtSrch As ManagementObjectSearcher
            Dim mgmt As ManagementObject

            Try
                If Not _Initialized Then
                    'Collect Win32 Operating System information
                    mgmtSrch = New ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem")
                    If mgmtSrch IsNot Nothing Then
                        For Each mgmt In mgmtSrch.Get
                            If mgmt IsNot Nothing Then
                                _OSName = mgmt("name").ToString()
                                _OSVersion = mgmt("version").ToString()
                                _ComputerName = mgmt("csname").ToString()
                                _WindowsDir = mgmt("windowsdirectory").ToString()
                            End If
                        Next
                    End If

                    'Collect Win32 Computer System information
                    mgmtSrch = New ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem")
                    If mgmtSrch IsNot Nothing Then
                        For Each mgmt In mgmtSrch.Get
                            If mgmt IsNot Nothing Then
                                _Username = mgmt("username").ToString()
                                _Manufacturer = mgmt("manufacturer").ToString()
                                _Model = mgmt("model").ToString()
                                _SystemType = mgmt("systemtype").ToString
                                _TPM = mgmt("totalphysicalmemory").ToString()
                            End If
                        Next
                    End If
                    _Initialized = True
                End If
            Catch ex As Exception
                'Catch & consume any Exception if run in a partially trusted context.
                'All SystemInfo properties will result in empty strings ("").
                Debug.Print("SystemInfo.Initialize() - Error: " & ex.Message)
                _Initialized = True
            End Try
        End Sub

        Public Shared ReadOnly Property ComputerName() As String
            Get
                If Not _Initialized Then Initialize()
                Return _ComputerName
            End Get
        End Property
        Public Shared ReadOnly Property Manufacturer() As String
            Get
                If Not _Initialized Then Initialize()
                Return _Manufacturer
            End Get
        End Property
        Public Shared ReadOnly Property Model() As String
            Get
                If Not _Initialized Then Initialize()
                Return _Model
            End Get
        End Property
        Public Shared ReadOnly Property OsName() As String
            Get
                If Not _Initialized Then Initialize()
                Return _OSName
            End Get
        End Property
        Public Shared ReadOnly Property OSVersion() As String
            Get
                If Not _Initialized Then Initialize()
                Return _OSVersion
            End Get
        End Property
        Public Shared ReadOnly Property SystemType() As String
            Get
                If Not _Initialized Then Initialize()
                Return _SystemType
            End Get
        End Property
        Public Shared ReadOnly Property TotalPhysicalMemory() As String
            Get
                If Not _Initialized Then Initialize()
                Return _TPM
            End Get
        End Property
        Public Shared ReadOnly Property Username() As String
            Get
                If Not _Initialized Then Initialize()
                Return _Username
            End Get
        End Property
        Public Shared ReadOnly Property WindowsDirectory() As String
            Get
                If Not _Initialized Then Initialize()
                Return _WindowsDir
            End Get
        End Property
        Public Shared ReadOnly Property MACAddress(ByVal GetWhat As String) As String
            Get
                Try
                    Dim mc As System.Management.ManagementClass
                    Dim mo As System.Management.ManagementObject
                    mc = New System.Management.ManagementClass("Win32_NetworkAdapterConfiguration")
                    Dim moc As System.Management.ManagementObjectCollection = mc.GetInstances()
                    For Each mo In moc
                        Select Case GetWhat.ToLower
                            Case "macaddress"
                                If mo.Item("IPEnabled") = True Then
                                    Return Trim(mo.Item("MacAddress").ToString())
                                End If
                        End Select
                    Next
                Catch ex As Exception
                    Return "Error: " & ex.Message
                End Try
            End Get
        End Property
        Public Shared ReadOnly Property CPUInfo(ByVal GetWhat As String) As String
            Get
                Try
                    Dim moReturn As System.Management.ManagementObjectCollection
                    Dim moSearch As System.Management.ManagementObjectSearcher
                    Dim mo As System.Management.ManagementObject
                    moSearch = New System.Management.ManagementObjectSearcher("Select * from Win32_Processor")
                    moReturn = moSearch.Get
                    For Each mo In moReturn
                        Select Case GetWhat.ToLower
                            Case "cpuinfo"
                                Return Trim(String.Format("{0} - {1}", mo("Name"), mo("CurrentClockSpeed")))
                            Case "cpuserial"
                                Return Trim(mo("ProcessorID"))
                        End Select
                    Next
                Catch ex As Exception
                    Return "Error:" & ex.Message
                End Try
            End Get
        End Property

        ''' <summary>
        ''' cSystemInfo class cannot be instantiated.
        ''' </summary>
        Private Sub New()
            'Nothing...
        End Sub

    End Class

End Namespace
