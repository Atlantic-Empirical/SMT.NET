Imports System.Runtime.InteropServices
Imports System.Management

Namespace Hardware.COM

    Public Module PortEnumeration

        Public ReadOnly Property SerialPortCount() As Integer
            Get
                Return My.Computer.Ports.SerialPortNames.Count
            End Get
        End Property

        Public ReadOnly Property SerialPortNames() As List(Of String)
            Get
                Try
                    Dim out As New List(Of String)
                    For Each n As String In My.Computer.Ports.SerialPortNames
                        out.Add(n)
                    Next
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with COMPortNames(). Error: " & ex.Message, ex)
                End Try
            End Get
        End Property

        Public ReadOnly Property AvailableCOMPortNumbers() As List(Of Byte)
            Get
                Dim out As New List(Of Byte)
                For Each n As String In My.Computer.Ports.SerialPortNames
                    If InStr(n.ToLower, "com") Then
                        out.Add(Replace(n.ToLower, "com", ""))
                    End If
                Next
                Return out
            End Get
        End Property

        '<DllImport("kernel32.dll")> _
        'Private Function GetDefaultCommConfig(ByVal lpszName As String, <[In](), Out()> ByRef lpCC As COMMCONFIG, ByRef lpdwSize As UInteger) As Boolean
        'End Function

        ''<DllImport("kernel32.dll", SetlastError:=True, CharSet:=CharSet.Auto)> _
        ''Private Function GetDefaultCommConfig(ByVal lpszName As String, ByRef lpCC As COMMCONFIG, ByRef lpdwSize As Integer) As Boolean
        ''End Function

        'Public Function isCOMPortAvailable(ByVal aPortNumber As Byte) As Boolean
        '    Try
        '        Dim lCOMMCONFIG As New COMMCONFIG
        '        Dim lCOMMDONFIGSize As Integer = Marshal.SizeOf(lCOMMCONFIG)
        '        lCOMMCONFIG.dwSize = lCOMMDONFIGSize
        '        Dim lResult As Boolean = GetDefaultCommConfig("COM" + aPortNumber.ToString, lCOMMCONFIG, lCOMMDONFIGSize)
        '        Return lResult
        '    Catch ex As Exception
        '        Throw New Exception("Problem with isCOMPortAvailable(). Error: " & ex.Message, ex)
        '    End Try
        'End Function

        'Public Function ScanCommPorts(ByVal maxComPortNumber As Integer) As List(Of String)
        '    Dim cc As New COMMCONFIG
        '    Dim sz As UInteger = CUInt(Marshal.SizeOf(cc))

        '    Dim out As New List(Of String)
        '    For i As Integer = 1 To maxComPortNumber
        '        Dim comPortName As String = String.Format("COM{0}", i)
        '        Dim bSuccess As Boolean = GetDefaultCommConfig(comPortName, cc, sz)
        '        'Console.WriteLine(String.Format("{0}: {1}", comPortName, If(bSuccess, "Valid", "Invalid")))
        '        If bSuccess Then
        '            out.Add("COM" & i & "= " & comPortName)
        '        End If
        '    Next
        '    Return out
        'End Function

        'Public Function GetCOMInfo() As List(Of String)
        '    Try
        '        'Dim mc As ManagementClass
        '        'Dim mo As ManagementObject
        '        'Dim moc As ManagementObjectCollection
        '        'Dim PortDescriptionArray As New List(Of String)
        '        'Dim I As Integer
        '        'mc = New ManagementClass("Win32_SerialPort")
        '        'moc = mc.GetInstances()
        '        'For Each mo In moc
        '        '    If mo.Item("Status").ToString = "OK" Then
        '        '        PortDescriptionArray.Add(mo.Item("Description").ToString)
        '        '        I += 1
        '        '    End If
        '        'Next
        '        'Return PortDescriptionArray
        '        Dim out As New List(Of String)
        '        For Each portName As String In My.Computer.Ports.SerialPortNames
        '            out.Add(portName)
        '        Next
        '        Return out
        '    Catch ex As Exception
        '        Throw New Exception("Problem with GetCOMInfo(). Error: " & ex.Message, ex)
        '    End Try
        'End Function

        '<StructLayout(LayoutKind.Sequential)> _
        'Friend Structure DCB
        '    Public DCBLength As Int32
        '    Public BaudRate As Int32
        '    Public fBitField As Int32
        '    Public wReserved As Int16
        '    Public XonLim As Int16
        '    Public XoffLim As Int16
        '    Public ByteSize As Byte
        '    Public Parity As Byte
        '    Public StopBits As Byte
        '    Public XonChar As Char
        '    Public XoffChar As Char
        '    Public ErrorChar As Char
        '    Public EofChar As Char
        '    Public EvtChar As Char
        '    Public wReserved1 As Int32
        'End Structure

        ''<StructLayout(LayoutKind.Sequential, Pack:=1)> _
        ''Private Structure DCB
        ''    Public DCBlength As Integer ' = System.Int32
        ''    Public BaudRate As Integer
        ''    Public Bits1 As Integer
        ''    Public wReserved As Short ' = System.Int16
        ''    Public XonLim As Short
        ''    Public XoffLim As Short
        ''    Public ByteSize As Byte
        ''    Public Parity As Byte
        ''    Public StopBits As Byte
        ''    Public XonChar As Char
        ''    Public XoffChar As Char
        ''    Public ErrorChar As Char
        ''    Public EofChar As Char
        ''    Public EvtChar As Char
        ''    Public wReserved2 As Short
        ''End Structure

        '<StructLayout(LayoutKind.Sequential)> _
        'Friend Structure COMMCONFIG
        '    Public dwSize As Int32
        '    Public wVersion As Int16
        '    Public wReserved As Int16
        '    Public dcb As DCB
        '    Public dwProviderSubType As Int32
        '    Public dwProviderOffset As Int32
        '    Public dwProviderSize As Int32
        '    Public wcProviderData As String
        'End Structure

        ''<StructLayout(LayoutKind.Sequential, Pack:=8)> _
        ''Private Structure COMMCONFIG
        ''    Public dwSize As Integer
        ''    Public wVersion As Short
        ''    Public wReserved As Short
        ''    Public dcbx As DCB
        ''    Public dwProviderSubType As Integer
        ''    Public dwProviderOffset As Integer
        ''    Public dwProviderSize As Integer
        ''    Public wcProviderData As Short
        ''End Structure

    End Module

End Namespace
