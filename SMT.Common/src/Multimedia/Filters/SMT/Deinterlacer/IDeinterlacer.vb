Imports System
Imports System.Text
Imports System.Runtime.InteropServices

Namespace Multimedia.Filters.SMT.Deinterlacer

    <ComVisible(True), ComImport(), Guid("A1891943-E854-4296-8DBC-83957BD4C5D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IDeinterlacer

        <PreserveSig()> _
        Function get_DeinterlaceType(ByRef pVal As Integer) As Integer

        <PreserveSig()> _
        Function put_DeinterlaceType(<[In]()> ByVal newVal As Integer) As Integer

        <PreserveSig()> _
        Function get_IsOddFieldFirst(ByRef pVal As Short) As Integer

        <PreserveSig()> _
        Function put_IsOddFieldFirst(<[In]()> ByVal newVal As Short) As Integer

        <PreserveSig()> _
        Function get_DScalerPluginName(<Out(), MarshalAs(UnmanagedType.BStr)> ByRef pVal As String) As Integer

        <PreserveSig()> _
        Function put_DScalerPluginName(<[In](), MarshalAs(UnmanagedType.BStr)> ByVal newVal As String) As Integer

        'virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_DeinterlaceType( 
        '    /* [retval][out] */ long *pVal) = 0;

        'virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_DeinterlaceType( 
        '    /* [in] */ long newVal) = 0;

        'virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_IsOddFieldFirst( 
        '    /* [retval][out] */ VARIANT_BOOL *pVal) = 0;

        'virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_IsOddFieldFirst( 
        '    /* [in] */ VARIANT_BOOL newVal) = 0;

        'virtual /* [helpstring][id][propget] */ HRESULT STDMETHODCALLTYPE get_DScalerPluginName( 
        '    /* [retval][out] */ BSTR *pVal) = 0;

        'virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_DScalerPluginName( 
        '    /* [in] */ BSTR newVal) = 0;

    End Interface

End Namespace