Imports System.IO
Imports System.Diagnostics

Namespace Win.ProcessExecution

    ''' <summary>
    ''' Class representing an executable process.
    ''' </summary>
    ''' <remarks>This is a single-use class in which one instance may launch 
    ''' a single process.</remarks>
    Public Class cProcessManager
        Implements IDisposable

#Region "Private members"

        ''' <summary>
        ''' Process instance tied to this object.
        ''' </summary>
        Public _Proc As System.Diagnostics.Process

        ''' <summary>
        ''' Indicates if this associated process has already been started.
        ''' </summary>
        Private _ProcessStarted As Boolean = False

        ''' <summary>
        ''' Indicates if this object has already been disposed off.
        ''' </summary>
        Private _Disposed As Boolean = False

        ''' <summary>
        ''' Indicates if this object's Shutdown method has been called.
        ''' </summary>
        Private _ShuttingDown As Boolean = False

#End Region 'Private members

#Region "Public members"

        ''' <summary>
        ''' Unique ID reference for tracking status (e.g. database key).
        ''' </summary>
        Public ClientProcessID As Integer

        ''' <summary>
        ''' Name of the object.
        ''' </summary>
        Public Name As String

#End Region 'Public members

#Region "Public properties"

        ''' <summary>
        ''' Unique Process ID of the current running process (<c>-1</c> if none).
        ''' </summary>
        ''' <value>Windows Process ID (<c>-1</c> if none)</value>
        Public ReadOnly Property PID() As Integer
            Get
                If _Proc Is Nothing Then
                    Return -1
                Else
                    Return _Proc.Id
                End If
            End Get
        End Property

        ''' <summary>
        ''' Indicates if the associated process has been started.
        ''' </summary>
        ''' <value><c>True</c> if the process has been started</value>
        Public ReadOnly Property hasProcessStarted() As Boolean
            Get
                Return _ProcessStarted
            End Get
        End Property

        ''' <summary>
        ''' Indicates if the associated process has exited.
        ''' </summary>
        ''' <value><c>True</c> if the process has exited</value>
        Public ReadOnly Property hasProcessExited() As Boolean
            Get
                If _Proc IsNot Nothing Then
                    Return _Proc.HasExited
                Else
                    Return False
                End If
            End Get
        End Property

        Public Property Exe_Path() As String
            Get
                Return _Exe_Path
            End Get
            Set(ByVal value As String)
                _Exe_Path = value
            End Set
        End Property
        Private _Exe_Path As String = ""

#End Region 'Public properties

#Region "Public event definitions"

        ''' <summary>
        ''' Event triggered each time a new line of text is output from the process log.
        ''' </summary>
        ''' <param name="ClientProcessID"></param>
        ''' <param name="Name">Object name</param>
        ''' <param name="PID">Windows Process ID</param>
        ''' <param name="Line">Line of text to add to log</param>
        ''' <remarks>Handle this event to capture the log output from the process.</remarks>
        Public Event evLogLineOutput(ByVal ClientProcessID As Integer, ByVal Name As String, ByVal PID As Integer, ByVal Line As String)

        ''' <summary>
        ''' Event triggered when the process exits.
        ''' </summary>
        ''' <param name="ClientProcessID"></param>
        ''' <param name="Name">Object name</param>
        ''' <param name="PID">Windows Process ID</param>
        ''' <param name="Succeeded">Boolean indicating if the process succeeded or terminated with an exit code</param>
        ''' <remarks>Handle this event to capture the output status of the process.</remarks>
        Public Event evProcessExited(ByVal ClientProcessID As Integer, ByVal Name As String, ByVal PID As Integer, ByVal Succeeded As Boolean)

#End Region 'Public event definitions

#Region "Constructors & Destructors"

        ''' <summary>
        ''' Creates a new instance of the class, assigns its properties, 
        ''' and instantiates the process (but does not start it).
        ''' </summary>
        ''' <param name="aProcessID">Unique ID for tracking status</param>
        ''' <param name="aName">Name of this object</param>
        ''' <remarks>Use to create a new instance of the class for launching 
        ''' a process.</remarks>
        Public Sub New(ByVal aProcessID As Integer, ByVal aName As String)
            ClientProcessID = aProcessID
            Name = aName
            'Create and setup Process
            _Proc = CreateProcess()
        End Sub

        ''' <summary>
        ''' Destructor.  Removes event handlers when object is destroyed.
        ''' </summary>
        ''' <remarks>Removes event handlers when object is destroyed.</remarks>
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            ' Take this object off the finalization queue 
            ' and prevent finalization code for this object
            ' from executing a second time.
            GC.SuppressFinalize(Me)
        End Sub

        ''' <summary>
        ''' Disposing method.
        ''' </summary>
        ''' <param name="disposing">If <c>True</c>, method has been called 
        ''' directly or indirectly by user's code.  Both managed and unmanaged 
        ''' resources can be disposed.  If <c>False</c>, method has been 
        ''' called by the runtime from inside the finalizer, so other 
        ''' objects should not be referenced.  Only unmanaged resources 
        ''' can be disposed.</param>
        ''' <remarks>Used to dispose of the object's resources and end tracing.</remarks>
        Private Overloads Sub Dispose(ByVal disposing As Boolean)
            ' Check to see if Dispose has already been called.
            If Not _Disposed Then
                ' If disposing equals true, dispose all managed 
                ' and unmanaged resources.
                If disposing Then
                    'Handle managed resources.
                    If _Proc IsNot Nothing Then
                        RemoveHandler _Proc.Exited, AddressOf ProcessExited           'Exit Event
                        RemoveHandler _Proc.OutputDataReceived, AddressOf ProcessOutput   'Stdout Output Event
                        RemoveHandler _Proc.ErrorDataReceived, AddressOf ProcessOutput    'Stderr Output Event
                    End If
                End If
                'Handle unmanaged resources here (if any)
            End If
            _Disposed = True
        End Sub

        ''' <summary>
        ''' This finalizer will run only if the Dispose method does not 
        ''' get called.  It gives the base class the opportunity to 
        ''' finalize.
        ''' </summary>
        ''' <remarks>Do not provide finalize methods in types derived 
        ''' from this class.</remarks>
        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub

#End Region 'Constructors & Destructors

#Region "Private methods"

        ''' <summary>
        ''' Creates a new instance of Process and populate it with standard settings.
        ''' </summary>
        ''' <returns>Process created</returns>
        Private Function CreateProcess() As System.Diagnostics.Process
            Dim result As System.Diagnostics.Process

            result = New System.Diagnostics.Process
            If result IsNot Nothing Then
                'Set executable location
                result.StartInfo.FileName = Exe_Path
                'Set UseShellExecute to False for redirection
                result.StartInfo.UseShellExecute = False
                result.StartInfo.CreateNoWindow = True
                result.StartInfo.RedirectStandardOutput = True
                result.StartInfo.RedirectStandardError = True
                'Setup event handling for process
                result.EnableRaisingEvents = True
                AddHandler result.Exited, AddressOf ProcessExited               'Exit Event
                AddHandler result.OutputDataReceived, AddressOf ProcessOutput   'Stdout Output Event
                AddHandler result.ErrorDataReceived, AddressOf ProcessOutput    'Stderr Output Event
            End If
            Return result
        End Function

        ''' <summary>
        ''' Start the current process with the specified command line arguments.
        ''' </summary>
        ''' <param name="CommandLine">String containing the command line arguments 
        ''' (optional, default = <c>""</c>)</param>
        Private Function StartProcess(Optional ByVal CommandLine As String = "") As Boolean
            Dim result As Boolean

            result = False
            If (_Proc IsNot Nothing) And Not _ProcessStarted Then
                _ProcessStarted = True
                SyncLock _Proc
                    'Set the command line parameters
                    _Proc.StartInfo.Arguments = CommandLine
                    'Start the process
                    _Proc.Start()
                    'Start the asynchronous read of the sort output stream
                    _Proc.BeginOutputReadLine()
                    _Proc.BeginErrorReadLine()
                End SyncLock
                result = True
            End If
            Return result
        End Function

#End Region    'Private methods

#Region "Public methods"

#Region "Standard methods"

        ''' <summary>
        ''' Terminate the current process.
        ''' </summary>
        ''' <remarks>Removes event handlers and kills the associated process.</remarks>
        Public Sub Shutdown()
            _ShuttingDown = True
            If _Proc IsNot Nothing Then
                SyncLock _Proc
                    Try
                        RemoveHandler _Proc.Exited, AddressOf ProcessExited           'Exit Event
                        RemoveHandler _Proc.OutputDataReceived, AddressOf ProcessOutput   'Stdout Output Event
                        RemoveHandler _Proc.ErrorDataReceived, AddressOf ProcessOutput    'Stderr Output Event
                        If Not _Proc.HasExited Then
                            _Proc.Kill()
                        Else
                            Debug.Print("- process has already exited")
                        End If
                    Catch ex As Exception
                        Debug.Print("Unable to kill process:  " & ex.Message)
                    Finally
                        _Proc.Close()
                        _Proc = Nothing
                    End Try
                End SyncLock
            End If
        End Sub

#End Region 'Standard methods

#Region "Process Starters"

        ''' <summary>
        ''' Starts the process with the specified arguments.
        ''' </summary>
        ''' <returns><c>True</c> if process starts successfully</returns>
        ''' <remarks>Use to launch the process.</remarks>
        Public Function RunProcess(ByVal Commandline As String) As Boolean
            Dim result As Boolean
            Dim cmdOpt1 As String = ""
            Dim cmdOpt2 As String = ""
            Dim cmdOpt3 As String = ""

            result = False
            If _Proc IsNot Nothing Then
                result = StartProcess(Commandline)
            End If
            Return result
        End Function

#End Region 'Process Starters

#Region "Callbacks"

        ''' <summary>
        ''' Event handler for process exiting.  (Do not call directly.)
        ''' </summary>
        ''' <param name="sender">Process that triggered the event</param>
        ''' <param name="e">Event arguments</param>
        ''' <remarks>Do not call this method.</remarks>
        Public Sub ProcessExited(ByVal sender As Object, ByVal e As System.EventArgs)
            Dim p As System.Diagnostics.Process
            Dim succeeded As Boolean

            If Not _ShuttingDown Then
                p = CType(sender, System.Diagnostics.Process)
                succeeded = (p.ExitCode = 0)
                RaiseEvent evProcessExited(ClientProcessID, Name, p.Id, succeeded)
            End If
        End Sub

        ''' <summary>
        ''' Event handler for log output.  (Do not call directly.)
        ''' </summary>
        ''' <param name="sender">Process that triggered the event</param>
        ''' <param name="outLine">Line of log output text</param>
        ''' <remarks>Do not call this method.</remarks>
        Public Sub ProcessOutput(ByVal sender As Object, ByVal outLine As System.Diagnostics.DataReceivedEventArgs)
            Dim proc As System.Diagnostics.Process

            If Not _ShuttingDown Then
                'Only process the event if outLine contains data
                If Not String.IsNullOrEmpty(outLine.Data) Then
                    proc = CType(sender, System.Diagnostics.Process)
                    RaiseEvent evLogLineOutput(ClientProcessID, Name, proc.Id, outLine.Data)
                End If
            End If
        End Sub

#End Region 'Callbacks

#End Region 'Public methods

    End Class

End Namespace
