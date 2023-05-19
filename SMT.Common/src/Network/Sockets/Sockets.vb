Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.IO

Namespace Network.Sockets

    Public Class SequoyanSockets
        Inherits System.ComponentModel.Component
        'Inherits SeqSocks_Components

#Region "Globals"
        Public Event StartReceive()
        Public Event ContinueReceive(ByVal Count As Integer)
        Public Event BytesReceived(ByVal b() As Byte)
        Public Event RecieveComplete()
        Public Event Status(ByVal Status As String)
        Public Sockets As New Collection
        Public ServerPort As Short
        'Private Comps As New SeqSocks_Components
        Public Event KPSUpdate(ByVal Speed As String)
#End Region

        Private components As System.ComponentModel.IContainer
        Public Sub New()
            Me.components = New System.ComponentModel.Container
            Me.KPSTimer = New System.Windows.Forms.Timer(Me.components)
        End Sub

#Region "Client Functionality"

        Public Function ConnectToServer(ByVal EP As IPEndPoint) As Socket
            Dim cSkt As Socket
            Try
                cSkt = New Socket(EP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                GC.Collect()
                GC.WaitForPendingFinalizers()

                cSkt.Connect(EP)
                GC.Collect()
                GC.WaitForPendingFinalizers()

                If cSkt.Connected Then
                    Sockets.Add(cSkt)
                    RaiseEvent Status("Client connected to server.")
                    Dim connthread As New Thread(AddressOf Execute)
                    connthread.Start()
                End If
                Return cSkt
            Catch ex As Exception
                If Not cSkt Is Nothing Then
                    Throw New Exception("Failed to connect to server." & vbNewLine & "Error: " & ex.Message)
                End If
                Return Nothing
            End Try
        End Function

#End Region

#Region "Server Functionality"

        Public Sub StartListening(ByVal nPort As String)
            Try
                ServerPort = CShort(nPort)
                Dim ListenThread As New Thread(AddressOf ListenerThread)
                ListenThread.Start()
            Catch ex As Exception
                Throw New Exception("Server port is invalid. Port: " & nPort & " Error: " & ex.Message)
            End Try
        End Sub

        Public Sub ListenerThread()
            Try
                'start listening
                Dim skt As Socket
                Dim listenEndPoint As New IPEndPoint(IPAddress.Any, ServerPort)
                Dim tcpServer As New Socket(listenEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                GC.Collect()
                GC.WaitForPendingFinalizers()

                tcpServer.Bind(listenEndPoint)
                tcpServer.Listen(Integer.MaxValue)

                GC.Collect()
                GC.WaitForPendingFinalizers()

                RaiseEvent Status("Server is listening on port " & CStr(ServerPort))

                'accept clients
                Dim tcpClient As Socket
                While True
                    Try
                        skt = tcpServer.Accept
                        Sockets.Add(skt)
                        RaiseEvent Status("Connection accepted from IP: " & skt.RemoteEndPoint.Serialize.ToString)
                        Dim connthread As New Thread(AddressOf Execute)
                        connthread.Start()
                    Catch ex As Exception
                        tcpServer.Close()
                        Console.ReadLine()
                    End Try
                End While
            Catch ex As Exception
                Throw New Exception("Error starting listener. Error: " & ex.Message)
                RaiseEvent Status("Server is not listening. Error: " & ex.Message)
            End Try
        End Sub

#End Region

#Region "Shared Functionality"

        Public Sub Execute()
            Dim Skt As Socket = Sockets(Sockets.Count)
            Dim MS As New MemoryStream
            Const BUFFER_SIZE As Integer = 1024

            Dim ByteCount As Integer
            Try
                While Skt.Connected
                    Dim a(1024) As Byte
                    Dim ByteLimit As Long

                    ByteCount = Skt.Receive(a, 1024, SocketFlags.None)
                    ByteLimit = BitConverter.ToInt32(a, 0)

                    'Upon receipt of data the socket should compile it into an entact/correct byte array 
                    'identical replica of the byte array that was sent from the other end
                    'NOTE: it seems that some extra bytes are being tacked on to the end of the transmission here.
                    'Example: when 12,078 bytes are transfered the first packet is 8192, and the second is 3,886 (which adds
                    'up to the correct number of bytes but then theres another 3,886 bytes that come across the wire. WTF?
                    'MS.Flush()

                    'answer: large packets have an unknown length and don't fit nicely into 1024 bytes
                    'the solution is to first send a known, fixed length variable of the buffer length to follow
                    'and capture the known length into the buffer only.

                    'read remainder of checksum data to flush buffer
                    Dim BUFFER() As Byte
                    ReDim BUFFER(BUFFER_SIZE)
                    MS.Flush()

                    Do While MS.Length < ByteLimit
                        RaiseEvent StartReceive()
                        If Not Me.KPSTimer.Enabled Then
                            SetupTimer()
                        End If

                        GC.Collect()
                        GC.WaitForPendingFinalizers()
                        Dim t As Integer

                        System.Windows.Forms.Application.DoEvents()

                        Dim nowavail As Integer = Skt.Available

                        If nowavail >= BUFFER.Length Then
                            ByteCount = Skt.Receive(BUFFER, BUFFER.Length, SocketFlags.None)
                        Else
                            ByteCount = Skt.Receive(BUFFER, nowavail, SocketFlags.None)
                        End If

                        GC.Collect()
                        GC.WaitForPendingFinalizers()

                        If ByteCount > 0 Then
                            MS.Write(BUFFER, 0, ByteCount)
                        End If
                        RaiseEvent ContinueReceive(ByteCount)
                        ReceviedByteCountOneSecond += ByteCount
                    Loop
                    KPSTimer.Stop()
                    RaiseEvent BytesReceived(MS.GetBuffer)

                    MS.Flush()
                    MS.Close()

                    'Uncomment following line to mirror data 
                    'SendBytes(MS.GetBuffer, Skt)

                    MS = Nothing
                    MS = New MemoryStream
                End While
            Catch ex As Exception
                Throw New Exception("Problem executing. Error: " & ex.Message)
            End Try
        End Sub

        Public Function SendBytes(ByVal b() As Byte, ByVal Skt As Socket) As Boolean
            Try
                If Not Skt.Connected Then
                    RaiseEvent Status("Not Connected")
                    Return False
                Else
                    Dim buffer(1) As Byte
                    Dim bLength(1024) As Byte
                    bLength = BitConverter.GetBytes(b.Length)
                    Skt.Send(bLength)

                    Dim x As Long
                    Dim buff(1024) As Byte
                    Dim bytecount As Integer

                    Do While True
                        buff.Clear(buff, 0, buff.Length)

                        GC.Collect()
                        GC.WaitForPendingFinalizers()
                        Dim t As Integer

                        System.Windows.Forms.Application.DoEvents()

                        System.Threading.Thread.Sleep(5) '500 = a half second
                        GC.Collect()
                        GC.WaitForPendingFinalizers()

                        Dim Bytes(1024) As Byte

                        If x + 1024 >= b.Length Then
                            buff.Copy(b, x, buff, 0, b.Length - x)
                            Skt.Send(buff, 1024, SocketFlags.None)
                            Exit Do
                        End If
                        buff.Copy(b, x, buff, 0, 1024)
                        bytecount = Skt.Send(buff, 1024, SocketFlags.None)

                        x = x + 1024
                    Loop
                    Return True
                End If
            Catch ex As Exception
                RaiseEvent Status("Error sending bytes. Error: " & ex.Message)
                Return False
            End Try
        End Function

        Public ReceviedByteCountOneSecond As Integer

        Public WithEvents KPSTimer As System.Windows.Forms.Timer
        Private Sub SetupTimer()
            KPSTimer = New System.Windows.Forms.Timer
            With KPSTimer
                .Interval = 1000
                .Start()
            End With
        End Sub

#End Region

        Private Sub SequoyanSockets_KPSTick1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KPSTimer.Tick
            RaiseEvent KPSUpdate(ReceviedByteCountOneSecond / 1024)
            ReceviedByteCountOneSecond = 0
        End Sub

    End Class 'SequoyanSockets

End Namespace 'Networking
