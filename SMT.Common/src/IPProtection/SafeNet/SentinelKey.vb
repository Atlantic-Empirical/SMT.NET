'**************************************************************************
'* SentinelKey.VB
'*
'* (C) Copyright 2008 SafeNet, Inc. All rights reserved.
'*
'* Description - SentinelKey file provide defination and wrappers for 
'*              Sentinel Keys Client Library.
'*
'****************************************************************************/
Imports System.Runtime.InteropServices

Public Class SentinelKey
    Implements IDisposable
#Region "Constructor / Destructor"

    Private disposed As Boolean = False
    Private licHandle As Int32
    Private strError As String = "Unable to load the required Sentinel Keys Client Library(SentinelKeyW.dll)." + vbCrLf + "Either the library is missing or corrupted."
    'Sentinel Keys Constructor
    Public Sub New()

    End Sub

    Public Overloads Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
    End Sub

    Private Overloads Sub Dispose(ByVal disposing As Boolean)
        ' Check to see if Dispose has already been called.
        If Not Me.disposed Then
            If disposing Then
                licHandle = 0
            End If
        End If
        disposed = True
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub
#End Region
#Region "SentinelKey API error codes"

    Public Const SP_DRIVER_LIBRARY_ERROR_BASE As Integer = 100
    Public Const SP_DUAL_LIBRARY_ERROR_BASE As Integer = 200
    Public Const SP_SERVER_ERROR_BASE As Integer = 300
    Public Const SP_SHELL_ERROR_BASE As Integer = 400
    Public Const SP_SECURE_UPDATE_ERROR_BASE As Integer = 500
	Public Const SP_SETUP_LIBRARY_ERROR_BASE    As Integer =    700
    Public Const SP_SUCCESS As Integer = 0

'Dual Library Error Codes:
    Public Const SP_ERR_INVALID_PARAMETER As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 1)
    Public Const SP_ERR_SOFTWARE_KEY As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 2)
    Public Const SP_ERR_INVALID_LICENSE As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 3)
    Public Const SP_ERR_INVALID_FEATURE As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 4)
    Public Const SP_ERR_INVALID_TOKEN As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 5)
    Public Const SP_ERR_NO_LICENSE As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 6)
    Public Const SP_ERR_INSUFFICIENT_BUFFER As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 7)
    Public Const SP_ERR_VERIFY_FAILED As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 8)
    Public Const SP_ERR_CANNOT_OPEN_DRIVER As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 9)
    Public Const SP_ERR_ACCESS_DENIED As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 10)
    Public Const SP_ERR_INVALID_DEVICE_RESPONSE As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 11)
    Public Const SP_ERR_COMMUNICATIONS_ERROR As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 12)
    Public Const SP_ERR_COUNTER_LIMIT As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 13)
    Public Const SP_ERR_MEM_CORRUPT As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 14)
    Public Const SP_ERR_INVALID_FEATURE_TYPE As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 15)
    Public Const SP_ERR_DEVICE_IN_USE As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 16)
    Public Const SP_ERR_INVALID_API_VERSION As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 17)
    Public Const SP_ERR_TIME_OUT_ERROR As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 18)
    Public Const SP_ERR_INVALID_PACKET As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 19)
    Public Const SP_ERR_KEY_NOT_ACTIVE As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 20)
    Public Const SP_ERR_FUNCTION_NOT_ENABLED As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 21)
    Public Const SP_ERR_DEVICE_RESET As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 22)
    Public Const SP_ERR_TIME_CHEAT As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 23)
    Public Const SP_ERR_INVALID_COMMAND As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 24)
    Public Const SP_ERR_RESOURCE As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 25)
    Public Const SP_ERR_UNIT_NOT_FOUND As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 26)
    Public Const SP_ERR_DEMO_EXPIRED As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 27)
    Public Const SP_ERR_QUERY_TOO_LONG As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 28)
	Public Const SP_ERR_USER_AUTH_REQUIRED As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 29)
    Public Const SP_ERR_DUPLICATE_LIC_ID As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 30)
    Public Const SP_ERR_DECRYPTION_FAILED As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 31)
    Public Const SP_ERR_BAD_CHKSUM  As Integer =  (SP_DUAL_LIBRARY_ERROR_BASE + 32)
    Public Const SP_ERR_BAD_LICENSE_IMAGE As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 33)
    Public Const SP_ERR_INSUFFICIENT_MEMORY As Integer = (SP_DUAL_LIBRARY_ERROR_BASE + 34) 

'Server Error Codes
    Public Const SP_ERR_SERVER_PROBABLY_NOT_UP As Integer = (SP_SERVER_ERROR_BASE + 1)
    Public Const SP_ERR_UNKNOWN_HOST As Integer = (SP_SERVER_ERROR_BASE + 2)
    Public Const SP_ERR_BAD_SERVER_MESSAGE As Integer = (SP_SERVER_ERROR_BASE + 3)
    Public Const SP_ERR_NO_LICENSE_AVAILABLE As Integer = (SP_SERVER_ERROR_BASE + 4)
    Public Const SP_ERR_INVALID_OPERATION As Integer = (SP_SERVER_ERROR_BASE + 5)
    Public Const SP_ERR_INTERNAL_ERROR As Integer = (SP_SERVER_ERROR_BASE + 6)
    Public Const SP_ERR_PROTOCOL_NOT_INSTALLED As Integer = (SP_SERVER_ERROR_BASE + 7)
    Public Const SP_ERR_BAD_CLIENT_MESSAGE As Integer = (SP_SERVER_ERROR_BASE + 8)
    Public Const SP_ERR_SOCKET_OPERATION As Integer = (SP_SERVER_ERROR_BASE + 9)
    Public Const SP_ERR_NO_SERVER_RESPONSE As Integer = (SP_SERVER_ERROR_BASE + 10)


' Shell Error Codes
    Public Const SP_ERR_BAD_ALGO As Integer = (SP_SHELL_ERROR_BASE + 1)
    Public Const SP_ERR_LONG_MSG As Integer = (SP_SHELL_ERROR_BASE + 2)
    Public Const SP_ERR_READ_ERROR As Integer = (SP_SHELL_ERROR_BASE + 3)
    Public Const SP_ERR_NOT_ENOUGH_MEMORY As Integer = (SP_SHELL_ERROR_BASE + 4)
    Public Const SP_ERR_CANNOT_OPEN As Integer = (SP_SHELL_ERROR_BASE + 5)
    Public Const SP_ERR_WRITE_ERROR As Integer = (SP_SHELL_ERROR_BASE + 6)
    Public Const SP_ERR_CANNOT_OVERWRITE As Integer = (SP_SHELL_ERROR_BASE + 7)
    Public Const SP_ERR_INVALID_HEADER As Integer = (SP_SHELL_ERROR_BASE + 8)
    Public Const SP_ERR_TMP_CREATE_ERROR As Integer = (SP_SHELL_ERROR_BASE + 9)
    Public Const SP_ERR_PATH_NOT_THERE As Integer = (SP_SHELL_ERROR_BASE + 10)
    Public Const SP_ERR_BAD_FILE_INFO As Integer = (SP_SHELL_ERROR_BASE + 11)
    Public Const SP_ERR_NOT_WIN32_FILE As Integer = (SP_SHELL_ERROR_BASE + 12)
    Public Const SP_ERR_INVALID_MACHINE As Integer = (SP_SHELL_ERROR_BASE + 13)
    Public Const SP_ERR_INVALID_SECTION As Integer = (SP_SHELL_ERROR_BASE + 14)
    Public Const SP_ERR_INVALID_RELOC As Integer = (SP_SHELL_ERROR_BASE + 15)
    Public Const SP_ERR_CRYPT_ERROR As Integer = (SP_SHELL_ERROR_BASE + 16)
    Public Const SP_ERR_SMARTHEAP_ERROR As Integer = (SP_SHELL_ERROR_BASE + 17)
    Public Const SP_ERR_IMPORT_OVERWRITE_ERROR As Integer = (SP_SHELL_ERROR_BASE + 18)
	
	Public Const SP_ERR_FRAMEWORK_REQUIRED        As Integer = (SP_SHELL_ERROR_BASE + 21)
    Public Const SP_ERR_CANNOT_HANDLE_FILE        As Integer = (SP_SHELL_ERROR_BASE + 22)

    Public Const SP_ERR_STRONG_NAME    As Integer = (SP_SHELL_ERROR_BASE + 26)
    Public Const SP_ERR_FRAMEWORK_10   As Integer = (SP_SHELL_ERROR_BASE + 27)
    Public Const SP_ERR_FRAMEWORK_SDK_10   As Integer = (SP_SHELL_ERROR_BASE + 28) 
    Public Const SP_ERR_FRAMEWORK_11       As Integer = (SP_SHELL_ERROR_BASE + 29)
    Public Const SP_ERR_FRAMEWORK_SDK_11   As Integer = (SP_SHELL_ERROR_BASE + 30)
    Public Const SP_ERR_FRAMEWORK_20       As Integer = (SP_SHELL_ERROR_BASE + 31)
    Public Const SP_ERR_FRAMEWORK_SDK_20   As Integer = (SP_SHELL_ERROR_BASE + 32)
    Public Const SP_ERR_APP_NOT_SUPPORTED  As Integer = (SP_SHELL_ERROR_BASE + 33)
    Public Const SP_ERR_FILE_COPY		   As Integer = (SP_SHELL_ERROR_BASE + 34)
    Public Const SP_ERR_HEADER_SIZE_EXCEED As Integer = (SP_SHELL_ERROR_BASE + 35)
	
	' CMDShell Error codes
    Public Const SP_ERR_PARAMETER_MISSING	 As Integer = (SP_SHELL_ERROR_BASE + 50)
    Public Const SP_ERR_PARAMETER_IDENTIFIER_MISSING As Integer = (SP_SHELL_ERROR_BASE + 51)
    Public Const SP_ERR_PARAMETER_INVALID		As Integer = (SP_SHELL_ERROR_BASE + 52)
    Public Const SP_ERR_REGISTRY					As Integer = (SP_SHELL_ERROR_BASE + 54)
    Public Const SP_ERR_VERIFY_SIGN				As Integer = (SP_SHELL_ERROR_BASE + 55)
    Public Const SP_ERR_PARAMETER				As Integer = (SP_SHELL_ERROR_BASE + 56)
    Public Const SP_ERR_LICENSE_TEMPLATE_FILE	As Integer = (SP_SHELL_ERROR_BASE + 57)
    Public Const SP_ERR_NO_DEVELOPER_KEY		As Integer = (SP_SHELL_ERROR_BASE + 58)
    Public Const SP_ERR_NO_ENDUSER_KEY			As Integer = (SP_SHELL_ERROR_BASE + 59)
    Public Const SP_ERR_NO_POINT_KEYS			As Integer = (SP_SHELL_ERROR_BASE + 60)
    Public Const SP_ERR_NO_SHELL_FEATURE		As Integer = (SP_SHELL_ERROR_BASE + 61)
    Public Const SP_ERR_SHELL_OPTION_FILE_MISSING  As Integer = (SP_SHELL_ERROR_BASE + 62)
    Public Const SP_ERR_SHELL_OPTION_FILE_FORMAT	As Integer = (SP_SHELL_ERROR_BASE +  63)
    Public Const SP_ERR_SHELL_OPTION_FILE_INVALID   As Integer = (SP_SHELL_ERROR_BASE +  64)
    Public Const SP_ERR_DELETE_LICENSE              As Integer = (SP_SHELL_ERROR_BASE +  65)
    Public Const SP_ERR_FILE_CREATE_FAILED          As Integer = (SP_SHELL_ERROR_BASE +  66)
    Public Const SP_ERR_SHELLFILES_LIMIT            As Integer = (SP_SHELL_ERROR_BASE +  67)
    Public Const SP_ERR_SINGLE_INSTANCE_ERROR       As Integer = (SP_SHELL_ERROR_BASE +  68) 
    Public Const SP_ERR_NO_EXE_FILE                 As Integer = (SP_SHELL_ERROR_BASE +  69)
 
'Secure Update error codes
    Public Const SP_ERR_KEY_NOT_FOUND As Integer = (SP_SECURE_UPDATE_ERROR_BASE + 1)
    Public Const SP_ERR_ILLEGAL_UPDATE As Integer = (SP_SECURE_UPDATE_ERROR_BASE + 2)
    Public Const SP_ERR_DLL_LOAD_ERROR As Integer = (SP_SECURE_UPDATE_ERROR_BASE + 3)
    Public Const SP_ERR_NO_CONFIG_FILE As Integer = (SP_SECURE_UPDATE_ERROR_BASE + 4)
    Public Const SP_ERR_INVALID_CONFIG_FILE As Integer = (SP_SECURE_UPDATE_ERROR_BASE + 5)
    Public Const SP_ERR_UPDATE_WIZARD_NOT_FOUND As Integer = (SP_SECURE_UPDATE_ERROR_BASE + 6)
    Public Const SP_ERR_UPDATE_WIZARD_SPAWN_ERROR As Integer = (SP_SECURE_UPDATE_ERROR_BASE + 7)
    Public Const SP_ERR_EXCEPTION_ERROR As Integer = (SP_SECURE_UPDATE_ERROR_BASE + 8)
    Public Const SP_ERR_INVALID_CLIENT_LIB As Integer = (SP_SECURE_UPDATE_ERROR_BASE + 9)
    Public Const SP_ERR_CABINET_DLL As Integer = (SP_SECURE_UPDATE_ERROR_BASE + 10)
    Public Const SP_ERR_INSUFFICIENT_REQ_CODE_BUFFER As Integer = (SP_SECURE_UPDATE_ERROR_BASE + 11)
    Public Const SP_ERR_UPDATE_WIZARD_USER_CANCELED As Integer = (SP_SECURE_UPDATE_ERROR_BASE + 12)
	
	' New Error codes defined for license addition
    Public Const SP_ERR_INVALID_DLL_VERSION  As Integer =  (SP_SECURE_UPDATE_ERROR_BASE + 13)
    Public Const SP_ERR_INVALID_FILE_TYPE  As Integer = (SP_SECURE_UPDATE_ERROR_BASE + 14) 
	
    Public Const  SP_ERR_BAD_XML     As Integer =  (SP_SETUP_LIBRARY_ERROR_BASE + 1)
    Public Const  SP_ERR_BAD_PACKET      As Integer = (SP_SETUP_LIBRARY_ERROR_BASE + 2)
    Public Const  SP_ERR_BAD_FEATURE     As Integer = (SP_SETUP_LIBRARY_ERROR_BASE + 3)
    Public Const  SP_ERR_BAD_HEADER      As Integer = (SP_SETUP_LIBRARY_ERROR_BASE + 4) 
    Public Const  SP_ERR_ISV_MISSING     As Integer = (SP_SETUP_LIBRARY_ERROR_BASE + 5) 
    Public Const  SP_ERR_DEVID_MISMATCH  As Integer = (SP_SETUP_LIBRARY_ERROR_BASE + 6)
    Public Const  SP_ERR_LM_TOKEN_ERROR  As Integer = (SP_SETUP_LIBRARY_ERROR_BASE + 7)
    Public Const  SP_ERR_LM_MISSING      As Integer = (SP_SETUP_LIBRARY_ERROR_BASE + 8)
    Public Const  SP_ERR_INVALID_SIZE    As Integer = (SP_SETUP_LIBRARY_ERROR_BASE + 9)
    Public Const  SP_ERR_FEATURE_NOT_FOUND  As Integer = (SP_SETUP_LIBRARY_ERROR_BASE + 10) 
    Public Const  SP_ERR_LICENSE_NOT_FOUND  As Integer =  (SP_SETUP_LIBRARY_ERROR_BASE + 11) 
    Public Const  SP_ERR_BEYOND_RANGE     As Integer = (SP_SETUP_LIBRARY_ERROR_BASE + 12) 
 
#End Region
#Region "SentinelKey Constants values used by client application"

    'SFNTGetLicense flags
    Public Const SP_TCP_PROTOCOL As Integer = 1
    Public Const SP_IPX_PROTOCOL As Integer = 2
    Public Const SP_NETBEUI_PROTOCOL As Integer = 4
    Public Const SP_STANDALONE_MODE As Integer = 32
    Public Const SP_SERVER_MODE As Integer = 64
    Public Const SP_SHARE_ON As Integer = 128
    Public Const SP_GET_NEXT_LICENSE As Integer = 1024
    Public Const SP_ENABLE_TERMINAL_CLIENT As Integer = 2048

    'query feature flag
    Public Const SP_SIMPLE_QUERY As Integer = 1
    Public Const SP_CHECK_DEMO As Integer = 0

    'Device Capabilities
    Public Const SP_CAPS_AES_128 As Integer = &H1
    Public Const SP_CAPS_ECC_K163 As Integer = &H2
    Public Const SP_CAPS_ECC_KEYEXCH As Integer = &H4
    Public Const SP_CAPS_ECC_SIGN As Integer = &H8
    Public Const SP_CAPS_TIME_SUPP As Integer = &H10
    Public Const SP_CAPS_TIME_RTC As Integer = &H20

    'Feature Attributies
    Public Const SP_ATTR_WRITE_ONCE As Integer = &H200
    Public Const SP_ATTR_ACTIVE As Integer = &H20
    Public Const SP_ATTR_AUTODEC As Integer = &H10
    Public Const SP_ATTR_SIGN As Integer = &H4
    Public Const SP_ATTR_DECRYPT As Integer = &H2
    Public Const SP_ATTR_ENCRYPT As Integer = &H1
    Public Const SP_ATTR_SECMSG_READ As Integer = &H80

    'Feature Type
    Public Const DATA_FEATURE_TYPE_BOOLEAN As Integer = 1
    Public Const DATA_FEATURE_TYPE_BYTE As Integer = 2
    Public Const DATA_FEATURE_TYPE_WORD As Integer = 3
    Public Const DATA_FEATURE_TYPE_DWORD As Integer = 4
    Public Const DATA_FEATURE_TYPE_RAW As Integer = 5
    Public Const DATA_FEATURE_TYPE_STRING As Integer = 6
    Public Const FEATURE_TYPE_COUNTER As Integer = 7
    Public Const FEATURE_TYPE_AES As Integer = 8
    Public Const FEATURE_TYPE_ECC As Integer = 9

    'Length definition
    Public Const SP_PUBILC_KEY_LEN		 As Integer =	42
    Public Const SP_SOFTWARE_KEY_LEN	 As Integer =	112
    Public Const SP_MIN_ENCRYPT_DATA_LEN As Integer =	16
    Public Const SP_MAX_QUERY_LEN		 As Integer =	112
    Public Const SP_MAX_RAW_LEN			 As Integer =	256
    Public Const SP_MAX_STRING_LEN		 As Integer =	256
    Public Const SP_MAX_SIGN_BUFFER_LEN	 As Integer =	&HFFFFFFFF
    
    'Heartbeat Interval Scope
    Public Const SP_MAX_HEARTBEAT As Integer = 2592000
    Public Const SP_MIN_HEARTBEAT As Integer = 60
    Public Const SP_INFINITE_HEARTBEAT As Integer = &HFFFFFFFF

    'it is for SFNTEnumerateServer function
    Public Const SP_RET_ON_FIRST_AVAILABLE As Integer = 1  'firstfound Sentinel Key Server that has a license to offer
    Public Const SP_GET_ALL_SERVERS As Integer = 4   'all the Sentinel Key Servers in the subnet


#End Region
#Region "Server Info Structure Used by SFNTGetServerInfo"
    Public Class ServerInfo
        Private Const MAX_SERVERINFO_LENGTH As Integer = 69
        Private Const BYTE_OFFSET_NAME As Integer = 0
        Private Const BYTE_OFFSET_PROTOCOLS As Integer = 64
        Private Const BYTE_OFFSET_MAJORVER As Integer = 66
        Private Const BYTE_OFFSET_MINORVER As Integer = 68
        Public bInfoBuffer(MAX_SERVERINFO_LENGTH) As Byte
        Dim encoding As System.Text.Encoding = System.Text.Encoding.UTF8


        Public ReadOnly Property ServerName() As String
            Get

                Return encoding.GetString(bInfoBuffer, BYTE_OFFSET_NAME, 64)
            End Get
        End Property

        Public ReadOnly Property Protocols() As Int16
            Get
                Return BitConverter.ToInt16(bInfoBuffer, BYTE_OFFSET_PROTOCOLS)
            End Get
        End Property

        Public ReadOnly Property MajorVersion() As Int16
            Get
                Return BitConverter.ToInt16(bInfoBuffer, BYTE_OFFSET_MAJORVER)
            End Get
        End Property

        Public ReadOnly Property MinorVersion() As Int16
            Get
                Return BitConverter.ToInt16(bInfoBuffer, BYTE_OFFSET_MINORVER)
            End Get
        End Property
    End Class
#End Region
#Region "License Info Structure Used by SFNTGetLicenseInfo"
    Public Class LicenseInfo

        Private Const MAX_LICENSEINFO_LENGTH As Integer = 15
        Private Const BYTE_OFFSET_LICENSEID As Integer = 0
        Private Const BYTE_OFFSET_USERLIMIT As Integer = 4
        Private Const BYTE_OFFSET_FEATURENUMS As Integer = 8
        Private Const BYTE_OFFSET_LICENSESIZE As Integer = 12
        Public bInfoBuffer(MAX_LICENSEINFO_LENGTH) As Byte

        Public ReadOnly Property LicenseID() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_LICENSEID)
            End Get
        End Property

        Public ReadOnly Property UserLimit() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_USERLIMIT)
            End Get
        End Property

        Public ReadOnly Property FeatureNums() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_FEATURENUMS)
            End Get
        End Property

        Public ReadOnly Property LicenseSize() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_LICENSESIZE)
            End Get
        End Property
    End Class
#End Region
#Region "Device Info Structure Used by SFNTGetDeviceInfo"
    Public Class DeviceInfo
        Private Const MAX_DEVICEINFO_LENGTH As Integer = 49
        Private Const BYTE_OFFSET_FORMFACTORTYPE As Integer = 0
        Private Const BYTE_OFFSET_PRODUCTCODE As Integer = 4
        Private Const BYTE_OFFSET_HARDLIMIT As Integer = 8
        Private Const BYTE_OFFSET_CAPABILITIES As Integer = 12
        Private Const BYTE_OFFSET_DEVID As Integer = 16
        Private Const BYTE_OFFSET_DEVSN As Integer = 20
        Private Const BYTE_OFFSET_DEVYEAR As Integer = 24
        Private Const BYTE_OFFSET_DEVMONTH As Integer = 28
        Private Const BYTE_OFFSET_DEVDAY As Integer = 29
        Private Const BYTE_OFFSET_DEVHOUR As Integer = 30
        Private Const BYTE_OFFSET_DEVMINUTE As Integer = 31
        Private Const BYTE_OFFSET_DEVSECOND As Integer = 32
        Private Const BYTE_OFFSET_MEMSIZE As Integer = 36
        Private Const BYTE_OFFSET_FREESIZE As Integer = 40
        Private Const BYTE_OFFSET_DRVVERSION As Integer = 44
        Public bInfoBuffer(MAX_DEVICEINFO_LENGTH) As Byte
        Public ReadOnly Property FormFactorType() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_FORMFACTORTYPE)
            End Get
        End Property
        Public ReadOnly Property ProductCode() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_PRODUCTCODE)
            End Get
        End Property
        Public ReadOnly Property Hardlimit() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_HARDLIMIT)
            End Get
        End Property
        Public ReadOnly Property Capabilities() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_CAPABILITIES)
            End Get
        End Property
        Public ReadOnly Property DevID() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_DEVID)
            End Get
        End Property
        Public ReadOnly Property DevSN() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_DEVSN)
            End Get
        End Property
        Public ReadOnly Property Year() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_DEVYEAR)
            End Get
        End Property
        Public ReadOnly Property Month() As Byte
            Get
                Return bInfoBuffer(BYTE_OFFSET_DEVMONTH)
            End Get
        End Property
        Public ReadOnly Property Day() As Byte
            Get
                Return bInfoBuffer(BYTE_OFFSET_DEVDAY)
            End Get
        End Property
        Public ReadOnly Property Hour() As Byte
            Get
                Return bInfoBuffer(BYTE_OFFSET_DEVHOUR)
            End Get
        End Property
        Public ReadOnly Property Minute() As Byte
            Get
                Return bInfoBuffer([BYTE_OFFSET_DEVMINUTE])
            End Get
        End Property
        Public ReadOnly Property Second() As Byte
            Get
                Return bInfoBuffer([BYTE_OFFSET_DEVSECOND])
            End Get
        End Property
        Public ReadOnly Property MemorySize() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_MEMSIZE)
            End Get
        End Property
        Public ReadOnly Property FreeSize() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_FREESIZE)
            End Get
        End Property
        Public ReadOnly Property DrvVersion() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_DRVVERSION)
            End Get
        End Property
    End Class
#End Region
#Region "Feature Info Structure Used by SFNTGetFeatureInfo"
    Public Class FeatureInfo
        Private Const MAX_FEATUREINFO_LENGTH As Integer = 35
        Private Const BYTE_OFFSET_FEATURETYPE As Integer = 0
        Private Const BYTE_OFFSET_FEATURESIZE As Integer = 4
        Private Const BYTE_OFFSET_FEATUREATTRIBUTES As Integer = 8
        Private Const BYTE_OFFSET_ENABLECOUNTER As Integer = 12
        Private Const BYTE_OFFSET_ENABLESTOPTIME As Integer = 13
        Private Const BYTE_OFFSET_ENABLEDURATION As Integer = 14
        Private Const BYTE_OFFSET_DURATION As Integer = 16
        Private Const BYTE_OFFSET_FEATUREYEAR As Integer = 20
        Private Const BYTE_OFFSET_FEATUREMONTH As Integer = 24
        Private Const BYTE_OFFSET_FEATUREDAY As Integer = 25
        Private Const BYTE_OFFSET_FEATUREHOUR As Integer = 26
        Private Const BYTE_OFFSET_FEATUREMINUTE As Integer = 27
        Private Const BYTE_OFFSET_FEATURESECOND As Integer = 28
        Private Const BYTE_OFFSET_LEFTEXECUTIONNUMBER As Integer = 32
        Public bInfoBuffer(MAX_FEATUREINFO_LENGTH) As Byte
        Public ReadOnly Property FeatureType() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_FEATURETYPE)
            End Get
        End Property
        Public ReadOnly Property FeatureSize() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_FEATURESIZE)
            End Get
        End Property
        Public ReadOnly Property FeatureAttributes() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_FEATUREATTRIBUTES)
            End Get
        End Property
        Public ReadOnly Property bEnableCounter() As Byte
            Get
                Return bInfoBuffer([BYTE_OFFSET_ENABLECOUNTER])
            End Get
        End Property
        Public ReadOnly Property bEnableStopTime() As Byte
            Get
                Return bInfoBuffer([BYTE_OFFSET_ENABLESTOPTIME])
            End Get
        End Property
        Public ReadOnly Property bEnableDurationTime() As Byte
            Get
                Return bInfoBuffer([BYTE_OFFSET_ENABLEDURATION])
            End Get
        End Property
        Public ReadOnly Property Duration() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_DURATION)
            End Get
        End Property
        Public ReadOnly Property Year() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_FEATUREYEAR)
            End Get
        End Property
        Public ReadOnly Property Month() As Byte
            Get
                Return bInfoBuffer(BYTE_OFFSET_FEATUREMONTH)
            End Get
        End Property
        Public ReadOnly Property Day() As Byte
            Get
                Return bInfoBuffer(BYTE_OFFSET_FEATUREDAY)
            End Get
        End Property
        Public ReadOnly Property Hour() As Byte
            Get
                Return bInfoBuffer(BYTE_OFFSET_FEATUREHOUR)
            End Get
        End Property
        Public ReadOnly Property Minute() As Byte
            Get
                Return bInfoBuffer(BYTE_OFFSET_FEATUREMINUTE)
            End Get
        End Property
        Public ReadOnly Property Second() As Byte
            Get
                Return bInfoBuffer(BYTE_OFFSET_FEATURESECOND)
            End Get
        End Property
        Public ReadOnly Property LeftExecutionNumber() As Int32
            Get
                Return BitConverter.ToInt32(bInfoBuffer, BYTE_OFFSET_LEFTEXECUTIONNUMBER)
            End Get
        End Property
    End Class
#End Region

#Region "Enum Server Info Structure Used by SFNTEnumServer"
    Public Class EnumServerInfo
        Private Const MAX_ENUMSERVERINFO_LENGTH As Integer = 66
        Private Const BYTE_OFFSET_SERVERADDR As Integer = 0
        Private Const BYTE_OFFSET_NUMLICAVAIL As Integer = 64

        Public bInfoBuffer(MAX_ENUMSERVERINFO_LENGTH) As Byte
        Dim encoding As System.Text.Encoding = System.Text.Encoding.UTF8
        Public ReadOnly Property ServerAddr() As String
            Get
                Return encoding.GetString(bInfoBuffer, BYTE_OFFSET_SERVERADDR, 64)
            End Get
        End Property
        Public ReadOnly Property NumLicAvail() As Int16
            Get
                Return BitConverter.ToInt16(bInfoBuffer, BYTE_OFFSET_NUMLICAVAIL)
            End Get
        End Property
    End Class
#End Region
#Region "//////////////////////// BEGIN PUBLIC METHODS	///////////////////////////////////"
    <CLSCompliant(False)> _
    Public Function SFNTGetLicense(ByVal devID As UInt32, _
                     ByVal softwareKey As Byte(), _
                     ByVal licID As Int32, _
                     ByVal flags As Int32) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTGetLicense(devID, softwareKey, licID, flags, licHandle)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTQueryFeature(ByVal featureID As Int32, _
                   ByVal flag As Int32, _
                   ByVal query As Byte(), _
                   ByVal queryLength As Int32, _
                   ByVal response As Byte(), _
                   ByVal responseLength As Int32) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTQueryFeature(licHandle, featureID, flag, query, queryLength, response, responseLength)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTReadString(ByVal featureID As Int32, _
                   ByVal stringBuffer As Byte(), _
                   ByVal stringLength As Int32) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTReadString(licHandle, featureID, stringBuffer, stringLength)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTWriteString(ByVal featureID As Int32, _
                   ByVal stringBuffer As Byte(), _
                   ByVal writePassword As Int32) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTWriteString(licHandle, featureID, stringBuffer, writePassword)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTReadInteger(ByVal featureID As Int32, _
                   ByRef featureValue As Int32) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTReadInteger(licHandle, featureID, featureValue)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTWriteInteger(ByVal featureID As Int32, _
                   ByVal featureValue As Int32, _
                   ByVal writePassword As Int32) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTWriteInteger(licHandle, featureID, featureValue, writePassword)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTReadRawData(ByVal featureID As Int32, _
                   ByVal rawDataBuffer As Byte(), _
                   ByVal offset As Int32, _
                   ByVal length As Int32) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTReadRawData(licHandle, featureID, rawDataBuffer, offset, length)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTWriteRawData(ByVal featureID As Int32, _
                   ByVal rawDataBuffer As Byte(), _
                   ByVal offset As Int32, _
                   ByVal length As Int32, _
                   ByVal writePassword As Int32) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTWriteRawData(licHandle, featureID, rawDataBuffer, offset, length, writePassword)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTCounterDecrement(ByVal featureID As Int32, _
                   ByVal decrementValue As Int32) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTCounterDecrement(licHandle, featureID, decrementValue)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTEncrypt(ByVal featureID As Int32, _
                   ByVal plainBuffer As Byte(), _
                   ByVal cipherBuffer As Byte()) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTEncrypt(licHandle, featureID, plainBuffer, cipherBuffer)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTDecrypt(ByVal featureID As Int32, _
                   ByVal cipherBuffer As Byte(), _
                   ByVal plainBuffer As Byte()) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTDecrypt(licHandle, featureID, cipherBuffer, plainBuffer)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTSign(ByVal featureID As Int32, _
                   ByVal signBuffer As Byte(), _
                   ByVal length As Int32, _
                   ByVal signResult As Byte()) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTSign(licHandle, featureID, signBuffer, length, signResult)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTVerify(ByVal publicKey As Byte(), _
                   ByVal signBuffer As Byte(), _
                   ByVal length As Int32, _
                   ByVal signResult As Byte()) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTVerify(licHandle, publicKey, signBuffer, length, signResult)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTSetHeartbeat(ByVal heartbeatValue As Int32) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTSetHeartbeat(licHandle, heartbeatValue)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTGetLicenseInfo(ByVal licenseInfo As SentinelKey.LicenseInfo) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTGetLicenseInfo(licHandle, licenseInfo.bInfoBuffer)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTGetFeatureInfo(ByVal featureID As Int32, _
                   ByVal featureInfo As SentinelKey.FeatureInfo) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTGetFeatureInfo(licHandle, featureID, featureInfo.bInfoBuffer)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTGetDeviceInfo(ByVal deviceInfo As SentinelKey.DeviceInfo) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTGetDeviceInfo(licHandle, deviceInfo.bInfoBuffer)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTGetServerInfo(ByVal serverInfo As SentinelKey.ServerInfo) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTGetServerInfo(licHandle, serverInfo.bInfoBuffer)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTReleaseLicense() As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTReleaseLicense(licHandle)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    Public Function SFNTSetContactServer(ByVal serverAddr As Byte()) As System.Int32
        Dim status As Int32
        Try
            status = SentinelKeyNativeApi.SFNTSetContactServer(serverAddr)
        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

    <CLSCompliant(False)> _
    Public Function SFNTEnumServer(ByVal devID As UInt32, _
                                   ByVal licID As Int32, _
                                   ByVal enumFlag As Int32, _
                                   ByVal serverInfo As SentinelKey.EnumServerInfo(), _
                                   ByRef numLicAvail As Int32) As System.Int32
        Dim status As Int32
        Dim buffer As Byte()
        Dim i As Integer
        Try
            ReDim buffer(66 * numLicAvail)
            status = SentinelKeyNativeApi.SFNTEnumServer(devID, licID, enumFlag, buffer, numLicAvail)
            For i = 0 To numLicAvail - 1
                Array.Copy(buffer, i * 66, serverInfo(i).bInfoBuffer, 0, 66)
            Next

        Catch e As DllNotFoundException
            Throw New DllNotFoundException(strError, e)
        End Try
        Return status
    End Function

#End Region
End Class