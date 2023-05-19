Imports System
Imports System.Runtime.InteropServices
Imports System.Runtime.InteropServices.ComTypes
Imports SMT.Win.COM.Interfaces

Namespace Multimedia.Filters.MainConcept

    <ComVisible(True), ComImport(), Guid("486F726E-4D43-49b9-8A0C-C22A2B0524E8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
    Public Interface IModuleConfig
        Inherits IPersistStream

        <PreserveSig()> _
        Shadows Sub GetClassID(ByRef pClassID As Guid)

        <PreserveSig()> _
        Shadows Function IsDirty() As Integer

        <PreserveSig()> _
        Shadows Sub Load(<[In]()> ByVal pStm As System.Runtime.InteropServices.ComTypes.IStream)

        <PreserveSig()> _
        Shadows Sub Save(<[In]()> ByVal pStm As System.Runtime.InteropServices.ComTypes.IStream, <[In](), MarshalAs(UnmanagedType.Bool)> ByVal fClearDirty As Boolean)

        <PreserveSig()> _
        Shadows Sub GetSizeMax(ByRef pcbSize As Long)

        '[helpstring("method SetValue assigns the new value to the module parameter identified by pParamID unique identifier, it should be verified and applied to the inside state of module by the call to CommitChanges")]
        'HRESULT SetValue([in] const GUID* pParamID, [in]  const VARIANT* pValue);
        <PreserveSig()> _
        Function SetValue( _
        <[In]()> ByVal pParamID As Integer, _
            <[In]()> ByVal pValue As Integer) As Integer

        '[helpstring("method GetValue retrieves the value of the module parameter identified by pParamID unique identifier")]
        'HRESULT GetValue([in] const GUID* pParamID, [out] VARIANT* pValue);
        <PreserveSig()> _
        Function GetValue( _
            <[In]()> ByRef pParamID As Guid, _
            <[Out]()> ByRef pValue As Integer) As Integer

        '[helpstring("method GetParamConfig retrieves the pointer to the interface of the module parameter identified by pParamID unique identifier")]
        'HRESULT GetParamConfig([in] const GUID* pParamID, [out] IParamConfig**  pValue);
        <PreserveSig()> _
        Function GetParamConfig( _
            <[In]()> ByRef pParamID As Guid, _
            <[Out]()> ByRef pValue As Integer) As Integer

        '[helpstring("method IsSupported clarifies whether the parameter identified by pParamID is valuable for this module or no.")]
        'HRESULT IsSupported([in] const GUID* pParamID);
        <PreserveSig()> _
        Function IsSupported( _
            <[In]()> ByRef pParamID As Guid) As Integer

        '[helpstring("method SetDefState resets all parameters of the module to its default values")]
        'HRESULT SetDefState();
        <PreserveSig()> _
        Function SetDefState() As Integer

        '[helpstring("method EnumParams retrieves the list of parameters that are valid for this module")]
        'HRESULT EnumParams([in][out] long* pNumParams, [in][out] GUID* pParamIDs);
        <PreserveSig()> _
        Function EnumParams( _
            ByRef pNumParams As Integer, _
            ByRef pParamIDs As Guid) As Integer

        '[helpstring("method CommitChanges verifies and applies changes of parameters to the internal state of the module")]
        'HRESULT CommitChanges([out] VARIANT* pReason);
        <PreserveSig()> _
        Function CommitChanges( _
            <[Out]()> ByRef pReason As Integer) As Integer

        '[helpstring("method DeclineChanges declines all unverified and don't applied changes of module parameters that have been made since the last call to CommitChanges and sets module to its previous committed state")]
        'HRESULT DeclineChanges();
        Function DeclineChanges() As Integer

        '[helpstring("method SaveToRegistry saves to the registry the internal module state that was successfully applied by the last call to CommitChanges")]
        'HRESULT SaveToRegistry([in] DWORD hKeyRoot, [in] const BSTR pszKeyName, [in] const BOOL bPreferReadable);
        <PreserveSig()> _
        Function SaveToRegistry( _
            <[In]()> ByRef hKeyRoot As Short, _
            <[In]()> ByRef pszKeyName As String, _
            <[In]()> ByRef bPreferReadable As Integer) As Integer

        '[helpstring("method LoadFromRegistry loads from the registry module parameters that should be verified and applied by the call to CommitChanges")]
        'HRESULT LoadFromRegistry([in] DWORD hKeyRoot, [in] const BSTR pszKeyName, [in] const BOOL bPreferReadable);
        <PreserveSig()> _
        Function LoadFromRegistry( _
            <[In]()> ByRef hKeyRoot As Short, _
            <[In]()> ByRef pszKeyName As String, _
            <[In]()> ByRef bPreferReadable As Integer) As Integer

        '[helpstring("method RegisterForNotifies registers the client for getting notifies occurred inside the module")]
        'HRESULT RegisterForNotifies([in] IModuleCallback* pModuleCallback);
        <PreserveSig()> _
        Function RegisterForNotifies( _
            <[In]()> ByRef pModuleCallback As Integer) As Integer

        '[helpstring("method UnregisterFromNotifies unregisters the client from getting notifies occurred inside the module")]
        'HRESULT UnregisterFromNotifies([in] IModuleCallback* pModuleCallback);
        <PreserveSig()> _
        Function UnregisterFromNotifies( _
            <[In]()> ByRef pModuleCallback As Integer) As Integer

    End Interface

End Namespace
