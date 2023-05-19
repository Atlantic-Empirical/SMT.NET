
Imports System
Imports System.Runtime.InteropServices
Imports System.Runtime.InteropServices.ComTypes 'requires .net 2.0

Namespace Multimedia.DirectShow

    Public Module DMO

        Public ReadOnly CLSID_DMOWrapperFilter As Guid = New Guid("94297043-bd82-4dfd-b0de-8177739c6d20")
        Public ReadOnly CLSID_DMOFilterCategory As Guid = New Guid("bcd5796c-bd52-4d30-ab76-70f975b89199")
        Public ReadOnly CLSID_NULL As Guid = New Guid("00000000-0000-0000-0000-000000000000")

        'Categories
        Public DMOCATEGORY_AUDIO_DECODER As Guid = New Guid("57f2db8b-e6bb-4513-9d43-dcd2a6593125")
        Public DMOCATEGORY_AUDIO_ENCODER As Guid = New Guid("33D9A761-90C8-11d0-BD43-00A0C911CE86")
        Public DMOCATEGORY_VIDEO_DECODER As Guid = New Guid("4a69b442-28be-4991-969c-b500adf5d8a8")
        Public DMOCATEGORY_VIDEO_ENCODER As Guid = New Guid("33D9A760-90C8-11d0-BD43-00A0C911CE86")
        Public DMOCATEGORY_AUDIO_EFFECT As Guid = New Guid("f3602b3f-0592-48df-a4cd-674721e7ebeb")
        Public DMOCATEGORY_VIDEO_EFFECT As Guid = New Guid("d990ee14-776c-4723-be46-3da2f56f10b9")
        Public DMOCATEGORY_AUDIO_CAPTURE_EFFECT As Guid = New Guid("f665aaba-3e09-4920-aa5f-219811148f09")

        <ComVisible(True), ComImport(), Guid("52d6f586-9f0f-4824-8fc8-e32ca04930c2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)> _
        Public Interface IDMOWrapperFilter
            <PreserveSig()> _
            Function Init( _
                <[In](), MarshalAs(UnmanagedType.LPArray)> ByVal clsidDMO As Byte(), _
                <[In](), MarshalAs(UnmanagedType.LPArray)> ByVal REFCLSID As Byte() _
            ) As Integer

            '    //  Init is passed in the clsid (so it can call CoCreateInstance)
            '    //  and the catgory under which the DMO lives.
            '    //  Note that catDMO can be CLSID_NULL, in which case no special
            '    //  category-specific processing will be invoked in the wrapper filter.
            '    HRESULT Init(REFCLSID clsidDMO, REFCLSID catDMO);

        End Interface

    End Module

End Namespace
