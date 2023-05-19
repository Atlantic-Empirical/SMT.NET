Imports System.Runtime.InteropServices
Imports System.Runtime.InteropServices.ComTypes

Namespace Win.COM.Interfaces

    <ComVisible(False), ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000000-0000-0000-C000-000000000046")> _
    Public Interface IUnknown
        Function QueryInterface(ByRef riid As Guid) As IntPtr

        <PreserveSig()> _
        Function AddRef() As UInt32

        <PreserveSig()> _
        Function Release() As UInt32
    End Interface 'IUnknown

    ''Public Enum HRESULT
    ''    S_OK = 0
    ''    S_FALSE = 1
    ''    E_NOTIMPL = &H80004001
    ''    E_INVALIDARG = &H80070057
    ''    E_NOINTERFACE = &H80004002
    ''    E_FAIL = &H80004005
    ''    E_UNEXPECTED = &H8000FFFF
    ''End Enum

    ''<ComVisible(True), ComImport(), Guid("7FD52380-4E07-101B-AE2D-08002B2EC713"), _
    ''InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)> _
    ''Public Interface IPersistStreamInit : Inherits IPersist

    ''    Shadows Sub GetClassID(ByRef pClassID As Guid)
    ''    <PreserveSig()> Function IsDirty() As Integer
    ''    <PreserveSig()> Function Load(ByVal pstm As UCOMIStream) As HRESULT
    ''    <PreserveSig()> Function Save(ByVal pstm As UCOMIStream, <MarshalAs(UnmanagedType.Bool)> ByVal fClearDirty As Boolean) As HRESULT
    ''    <PreserveSig()> Function GetSizeMax(<InAttribute(), Out(), _
    ''    MarshalAs(UnmanagedType.U8)> ByRef pcbSize As Long) As HRESULT
    ''    <PreserveSig()> Function InitNew() As HRESULT

    ''End Interface

    ''<ComVisible(True), ComImport(), Guid("0000010c-0000-0000-C000-000000000046"), _
    ''InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)> _
    ''Public Interface IPersist
    ''    Sub GetClassID(ByRef pClassID As Guid)
    ''End Interface


    ''[InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000010c-0000-0000-C000-000000000046")]
    ''public interface IPersist {
    ''    void GetClassID( /* [out] */ out Guid pClassID);
    ''};
    '<ComVisible(True), ComImport(), Guid("0000010c-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    'Public Interface IPersist

    '    <PreserveSig()> _
    '    Sub GetClassID(ByRef pClassID As Guid) ' [out] 
    '    'Sub GetClassID(<[Out]()> ByRef pClassID As Guid)

    'End Interface

    '<ComVisible(True), ComImport(), Guid("00000109-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsDual)> _
    'Public Interface IPersistStream
    '    Inherits IPersist

    '    Shadows Sub GetClassID(ByRef pClassID As Guid)

    '    <PreserveSig()> _
    '    Function IsDirty() As Integer

    '    <PreserveSig()> _
    '    Function Load(<[In]()> ByVal pStm As IStream) As Integer

    '    <PreserveSig()> _
    '    Function Save(<[In]()> ByVal pStm As IStream, <[In](), MarshalAs(UnmanagedType.Bool)> ByVal fClearDirty As Boolean) As Integer

    '    <PreserveSig()> _
    '    Function GetSizeMax(ByRef pcbSize As UInt32) As Integer

    'End Interface

    '<ComVisible(True), ComImport(), Guid("7FD52380-4E07-101B-AE2D-08002B2EC713"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)> _
    'Public Interface IPersistStreamInit

    '    Sub GetClassID(ByRef pClassID As Guid)
    '    <PreserveSig()> Function IsDirty() As Integer
    '    <PreserveSig()> Function Load(ByVal pstm As UCOMIStream) As Integer
    '    <PreserveSig()> Function Save(ByVal pstm As UCOMIStream, ByVal ByValByValfClearDirty As Boolean) As Integer
    '    <PreserveSig()> Function GetSizeMax(<InAttribute(), Out(), MarshalAs(UnmanagedType.U8)> ByRef pcbSize As Long) As Integer
    '    <PreserveSig()> Function InitNew() As Integer

    'End Interface

    '<ComVisible(True), ComImport(), Guid("BD1AE5E0-A6AE-11CE-BD37-504200C10000"), InterfaceType(ComInterfaceType.InterfaceIsDual)> _
    'Public Interface IPersistMemory
    '    Inherits IPersist

    '    Shadows Sub GetClassID(ByRef pClassID As Guid)

    '    <PreserveSig()> _
    '    Function IsDirty() As Integer

    '    <PreserveSig()> _
    '    Function Load(<[In]()> ByVal ptr As IntPtr, <[In]()> ByVal cbSize As UInt32) As Integer

    '    <PreserveSig()> _
    '    Function Save(ByRef ptr As IntPtr, <[In]()> ByVal fClearDirty As Boolean, <[In]()> ByVal cbSize As UInt32) As Integer

    '    <PreserveSig()> _
    '    Function GetSizeMax(ByRef pCbSize As UInt32) As Integer

    '    <PreserveSig()> _
    '    Function InitNew() As Integer

    'End Interface


    '    MIDL_INTERFACE("BD1AE5E0-A6AE-11CE-BD37-504200C10000")
    'IPersistMemory : public IPersist
    '{
    'public:
    '    virtual HRESULT STDMETHODCALLTYPE IsDirty( void) = 0;

    '    virtual /* [local] */ HRESULT STDMETHODCALLTYPE Load( 
    '        /* [size_is][in] */ LPVOID pMem,
    '        /* [in] */ ULONG cbSize) = 0;

    '    virtual /* [local] */ HRESULT STDMETHODCALLTYPE Save( 
    '        /* [size_is][out] */ LPVOID pMem,
    '        /* [in] */ BOOL fClearDirty,
    '        /* [in] */ ULONG cbSize) = 0;

    '    virtual HRESULT STDMETHODCALLTYPE GetSizeMax( 
    '        /* [out] */ ULONG *pCbSize) = 0;

    '    virtual HRESULT STDMETHODCALLTYPE InitNew( void) = 0;

    '};









    '<ComImport(), Guid("0000000c-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    'Public Interface IStream
    '    Sub Read(<Out(), MarshalAs(UnmanagedType.LPArray, SizeParamIndex:=1)> ByVal pv() As Byte, ByVal cb As Integer, ByVal pcbRead As IntPtr)
    '    Sub Write(<MarshalAs(UnmanagedType.LPArray, SizeParamIndex:=1)> ByVal pv() As Byte, ByVal cb As Integer, ByVal pcbWritten As IntPtr)
    '    Sub Seek(ByVal dlibMove As Long, ByVal dwOrigin As Integer, ByVal plibNewPosition As IntPtr)
    '    Sub SetSize(ByVal libNewSize As Long)
    '    Sub CopyTo(ByVal pstm As IStream, ByVal cb As Long, ByVal pcbRead As IntPtr, ByVal pcbWritten As IntPtr)
    '    Sub Commit(ByVal grfCommitFlags As Integer)
    '    Sub Revert()
    '    Sub LockRegion(ByVal libOffset As Long, ByVal cb As Long, ByVal dwLockType As Integer)
    '    Sub UnlockRegion(ByVal libOffset As Long, ByVal cb As Long, ByVal dwLockType As Integer)
    '    Sub Stat(ByRef pstatstg As STATSTG, ByVal grfStatFlag As Integer)
    '    Sub Clone(ByRef ppstm As IStream)
    'End Interface 'IStream 

    '/// <summary>
    '/// Definition for interface IPersistStream.
    '/// </summary>
    '[InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("00000109-0000-0000-C000-000000000046")]
    'public interface IPersistStream : IPersist {
    '    /// <summary>
    '    /// GetClassID
    '    /// </summary>
    '    /// <param name="pClassID"></param>
    '    new void GetClassID(out Guid pClassID);
    '    /// <summary>
    '    /// isDirty
    '    /// </summary>
    '    /// <returns></returns>
    '    [PreserveSig]
    '    int IsDirty( );
    '    /// <summary>
    '    /// Load
    '    /// </summary>
    '    /// <param name="pStm"></param>
    '    void Load([In] UCOMIStream pStm);
    '    /// <summary>
    '    /// Save
    '    /// </summary>
    '    /// <param name="pStm"></param>
    '    /// <param name="fClearDirty"></param>
    '    void Save([In] UCOMIStream pStm, [In, MarshalAs(UnmanagedType.Bool)] bool fClearDirty);
    '    /// <summary>
    '    /// GetSizeMax
    '    /// </summary>
    '    /// <param name="pcbSize"></param>
    '    void GetSizeMax(out long pcbSize);
    '};

    <ComImport(), System.Security.SuppressUnmanagedCodeSecurity(), Guid("00000109-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IPersistStream
        Inherits IPersist

#Region "IPersist Methods"

        <PreserveSig()> _
        Shadows Function GetClassID(<Out()> ByRef pClassID As Guid) As Integer

#End Region

        <PreserveSig()> _
        Function IsDirty() As Integer

        Function Load(<[In]()> ByVal pStm As IStream) As Integer

        <PreserveSig()> _
        Function Save( _
            <[In]()> ByVal pStm As IStream, _
            <[In](), MarshalAs(UnmanagedType.Bool)> ByVal fClearDirty As Boolean _
        ) As Integer

        <PreserveSig()> _
        Function GetSizeMax(<Out()> ByRef pcbSize As Long) As Integer

    End Interface

    <ComImport(), System.Security.SuppressUnmanagedCodeSecurity(), Guid("0000010c-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IPersist

        <PreserveSig()> _
        Function GetClassID(<Out()> ByRef pClassID As Guid) As Integer

    End Interface

End Namespace
