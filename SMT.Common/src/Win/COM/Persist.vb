Imports System.Runtime.InteropServices
Imports System.Runtime.InteropServices.ComTypes

Namespace Win.COM

    Module Persist

        'Public Sub LoadGraphFile(ByVal graphBuilder As IGraphBuilder, ByVal fileName As String)
        '    Dim hr As Integer = 0
        '    Dim storage As IStorage = Nothing
        '    Dim stream As System.Runtime.InteropServices.ComTypes.IStream = Nothing

        '    Try
        '        If NativeMethods.StgIsStorageFile(fileName) <> 0 Then
        '            Return
        '        End If

        '        hr = NativeMethods.StgOpenStorage(fileName, Nothing, STGM.Transacted Or STGM.Read Or STGM.ShareDenyWrite, IntPtr.Zero, 0, storage)

        '        Marshal.ThrowExceptionForHR(hr)

        '        hr = storage.OpenStream("ActiveMovieGraph", IntPtr.Zero, STGM.Read Or STGM.ShareExclusive, 0, stream)

        '        If hr >= 0 Then
        '            hr = TryCast(graphBuilder, IPersistStream).Load(stream)
        '            Marshal.ThrowExceptionForHR(hr)
        '        End If
        '    Finally
        '        If stream IsNot Nothing Then
        '            Marshal.FinalReleaseComObject(stream)
        '        End If
        '        If storage IsNot Nothing Then
        '            Marshal.FinalReleaseComObject(storage)
        '        End If
        '    End Try
        'End Sub

        'Public Sub SaveGraphFile(ByVal graphBuilder As IGraphBuilder, ByVal fileName As String)
        '    Dim hr As Integer = 0
        '    Dim storage As IStorage = Nothing
        '    Dim stream As IStream = Nothing

        '    If graphBuilder Is Nothing Then
        '        Throw New ArgumentNullException("graphBuilder")
        '    End If

        '    Try
        '        hr = NativeMethods.StgCreateDocfile(fileName, STGM.Create Or STGM.Transacted Or STGM.ReadWrite Or STGM.ShareExclusive, 0, storage)

        '        Marshal.ThrowExceptionForHR(hr)

        '        hr = storage.CreateStream("ActiveMovieGraph", STGM.Write Or STGM.Create Or STGM.ShareExclusive, 0, 0, stream)

        '        Marshal.ThrowExceptionForHR(hr)

        '        hr = TryCast(graphBuilder, IPersistStream).Save(stream, True)
        '        Marshal.ThrowExceptionForHR(hr)

        '        hr = storage.Commit(STGC.[Default])
        '        Marshal.ThrowExceptionForHR(hr)
        '    Finally
        '        If stream IsNot Nothing Then
        '            Marshal.FinalReleaseComObject(stream)
        '        End If
        '        If storage IsNot Nothing Then
        '            Marshal.FinalReleaseComObject(storage)
        '        End If
        '    End Try
        'End Sub

        Friend NotInheritable Class NativeMethods

            Private Sub New()
            End Sub

            <DllImport("ole32.dll", CharSet:=CharSet.Unicode, ExactSpelling:=True)> _
            Public Shared Function StgCreateDocfile( _
                <[In](), MarshalAs(UnmanagedType.LPWStr)> ByVal pwcsName As String, _
                <[In]()> ByVal grfMode As STGM, _
                <[In]()> ByVal reserved As Integer, _
                <Out()> ByRef ppstgOpen As IStorage) As Integer
            End Function

            <DllImport("ole32.dll", CharSet:=CharSet.Unicode, ExactSpelling:=True)> _
            Public Shared Function StgIsStorageFile(<[In](), MarshalAs(UnmanagedType.LPWStr)> ByVal pwcsName As String) As Integer
            End Function

            <DllImport("ole32.dll", CharSet:=CharSet.Unicode, ExactSpelling:=True)> _
            Public Shared Function StgOpenStorage(<[In](), MarshalAs(UnmanagedType.LPWStr)> _
                ByVal pwcsName As String, <[In]()> _
                ByVal pstgPriority As IStorage, <[In]()> _
                ByVal grfMode As STGM, <[In]()> _
                ByVal snbExclude As IntPtr, <[In]()> _
                ByVal reserved As Integer, <Out()> _
                ByRef ppstgOpen As IStorage) As Integer
            End Function

        End Class

        <Flags()> _
        Friend Enum STGM
            Read = 0
            Write = 1
            ReadWrite = 2
            ShareDenyNone = 64
            ShareDenyRead = 48
            ShareDenyWrite = 32
            ShareExclusive = 16
            Priority = 262144
            Create = 4096
            Convert = 131072
            FailIfThere = 0
            Direct = 0
            Transacted = 65536
            NoScratch = 1048576
            NoSnapShot = 2097152
            Simple = 134217728
            DirectSWMR = 4194304
            DeleteOnRelease = 67108864
        End Enum

        <Flags()> _
        Friend Enum STGC
            [Default] = 0
            Overwrite = 1
            OnlyIfCurrent = 2
            DangerouslyCommitMerelyToDiskCache = 4
            Consolidate = 8
        End Enum

        <Guid("0000000b-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
        Friend Interface IStorage

            <PreserveSig()> _
            Function CreateStream( _
                <[In](), MarshalAs(UnmanagedType.LPWStr)> ByVal pwcsName As String, _
                <[In]()> ByVal grfMode As STGM, _
                <[In]()> ByVal reserved1 As Integer, _
                <[In]()> ByVal reserved2 As Integer, _
                <Out()> ByRef ppstm As System.Runtime.InteropServices.ComTypes.IStream _
            ) As Integer

            <PreserveSig()> _
            Function OpenStream(<[In](), MarshalAs(UnmanagedType.LPWStr)> _
                ByVal pwcsName As String, <[In]()> _
                ByVal reserved1 As IntPtr, <[In]()> _
                ByVal grfMode As STGM, <[In]()> _
                ByVal reserved2 As Integer, <Out()> _
                ByRef ppstm As System.Runtime.InteropServices.ComTypes.IStream) As Integer

            <PreserveSig()> _
            Function CreateStorage(<[In](), MarshalAs(UnmanagedType.LPWStr)> _
                ByVal pwcsName As String, <[In]()> _
                ByVal grfMode As STGM, <[In]()> _
                ByVal reserved1 As Integer, <[In]()> _
                ByVal reserved2 As Integer, <Out()> _
                ByRef ppstg As IStorage) As Integer

            <PreserveSig()> _
            Function OpenStorage(<[In](), MarshalAs(UnmanagedType.LPWStr)> _
                ByVal pwcsName As String, <[In]()> _
                ByVal pstgPriority As IStorage, <[In]()> _
                ByVal grfMode As STGM, <[In]()> _
                ByVal snbExclude As Integer, <[In]()> _
                ByVal reserved As Integer, <Out()> _
                ByRef ppstg As IStorage) As Integer

            <PreserveSig()> _
            Function CopyTo(<[In]()> _
                ByVal ciidExclude As Integer, <[In]()> _
                ByVal rgiidExclude As Guid(), <[In]()> _
                ByVal snbExclude As String(), <[In]()> _
                ByVal pstgDest As IStorage) As Integer

            <PreserveSig()> _
            Function MoveElementTo(<[In](), MarshalAs(UnmanagedType.LPWStr)> _
                ByVal pwcsName As String, <[In]()> _
                ByVal pstgDest As IStorage, <[In](), MarshalAs(UnmanagedType.LPWStr)> _
                ByVal pwcsNewName As String, <[In]()> _
                ByVal grfFlags As STGM) As Integer

            <PreserveSig()> _
            Function Commit(<[In]()> _
                ByVal grfCommitFlags As STGC) As Integer

            <PreserveSig()> _
            Function Revert() As Integer

            <PreserveSig()> _
            Function EnumElements(<[In]()> _
                ByVal reserved1 As Integer, <[In]()> _
                ByVal reserved2 As IntPtr, <[In]()> _
                ByVal reserved3 As Integer, <Out(), MarshalAs(UnmanagedType.[Interface])> _
                ByRef ppenum As Object) As Integer

            <PreserveSig()> _
            Function DestroyElement(<[In](), MarshalAs(UnmanagedType.LPWStr)> _
                ByVal pwcsName As String) As Integer

            <PreserveSig()> _
            Function RenameElement(<[In](), MarshalAs(UnmanagedType.LPWStr)> _
                ByVal pwcsOldName As String, <[In](), MarshalAs(UnmanagedType.LPWStr)> _
                ByVal pwcsNewName As String) As Integer

            <PreserveSig()> _
            Function SetElementTimes(<[In](), MarshalAs(UnmanagedType.LPWStr)> _
                ByVal pwcsName As String, <[In]()> _
                ByVal pctime As System.Runtime.InteropServices.ComTypes.FILETIME, <[In]()> _
                ByVal patime As System.Runtime.InteropServices.ComTypes.FILETIME, <[In]()> _
                ByVal pmtime As System.Runtime.InteropServices.ComTypes.FILETIME) As Integer

            <PreserveSig()> _
            Function SetClass(<[In](), MarshalAs(UnmanagedType.LPStruct)> _
                ByVal clsid As Guid) As Integer

            <PreserveSig()> _
            Function SetStateBits(<[In]()> _
                ByVal grfStateBits As Integer, <[In]()> _
                ByVal grfMask As Integer) As Integer

            <PreserveSig()> _
            Function Stat(<Out()> _
                ByRef pStatStg As System.Runtime.InteropServices.ComTypes.STATSTG, <[In]()> _
                ByVal grfStatFlag As Integer) As Integer

        End Interface

    End Module

End Namespace
