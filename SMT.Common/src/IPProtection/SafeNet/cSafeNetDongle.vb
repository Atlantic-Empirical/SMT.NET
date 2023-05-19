Imports System.IO
Imports System.Windows.Forms

Namespace IPProtection.SafeNet

    Public Class cSafeNetDongle

#Region "PROPERTIES"

        'PRIVATE
        'Public Shared oSentinelKeysLicense As SentinelKeysLicense   'for  this License. 

        'PROTECTED
        Protected oSentinelKey As SentinelKey                         'for the Sentinel Keys client library.
        Protected DeveloperID As UInt32 = 2536071674                  ' Developer ID of Sentinel Keys key
        Protected status As eSafeNetStatusCodes                     ' The Sentinel Keys API return codes
        Protected LicenseID As Long
        Protected QueryByteLength As Integer

#End Region 'PRIVATE PROPERTIES

#Region "CONSTRUCTOR/DESTRUCTOR"

        Public Sub New()
            Try
                oSecureUpdate = New SecureUpdate
            Catch excp As DllNotFoundException
                MessageBox.Show(excp.Message())
            End Try

            oSentinelKey = New SentinelKey()
        End Sub

        Public Sub Dispose()
            If oSecureUpdate IsNot Nothing Then
                oSecureUpdate.Dispose()
                oSecureUpdate = Nothing
            End If
            If Not oSentinelKey Is Nothing Then
                oSentinelKey.Dispose()
                oSentinelKey = Nothing
            End If
        End Sub

#End Region 'CONSTRUCTOR/DESTRUCTOR

#Region "DONGLE IMPLEMENTATION"

        Public Overridable Function GetLicense() As eSafeNetStatusCodes
            Try
                Return oSentinelKey.SFNTGetLicense(DeveloperID, Nothing, Nothing, SentinelKey.SP_STANDALONE_MODE)
            Catch DllEx As DllNotFoundException
                Return eSafeNetStatusCodes.SP_ERR_DLL_LOAD_ERROR
            Catch ex As Exception
                Throw New Exception("Problem with GetLicense(). Error: " & ex.Message)
                'Return eSafeNetStatusCodes.SP_ERR_GENERIC
            End Try
        End Function

        Public Function AESFeatureQuery(ByVal Feature As Integer) As eSafeNetStatusCodes
            Try
                Dim plainBuffer() As Byte = New Byte(16) {}                         ' A byte array for simple data   
                Dim cipherBuffer() As Byte = New Byte(16) {}                        ' A byte array for encrypted data   
                Dim flag_AES As Byte                                                ' Used To set the counter flag on/off   
                Dim querylength_AES As Int32 = QueryByteLength                      ' The length of the query byte array
                Dim queryValue_AES() As Byte = New Byte(querylength_AES) {}         ' A byte array for query
                Dim responseLength_AES As Int32 = QueryByteLength                   ' The length of the response byte array
                Dim response_AES() As Byte = New Byte(responseLength_AES) {}        ' A byte array for query response 

                status = oSentinelKey.SFNTQueryFeature(Feature, flag_AES, queryValue_AES, querylength_AES, response_AES, responseLength_AES)

                'Debug.WriteLine(status.ToString)
                Return status
            Catch ex As Exception
                Throw New Exception("Problem with PhoenixFeatureQuery(). Error: " & ex.Message, ex)
            End Try
        End Function

        Public Function BoolFeatureQuery(ByVal Feature As Integer) As eSafeNetStatusCodes
            Try
                Dim readValue_boolean As Int32
                status = oSentinelKey.SFNTReadInteger(Feature, readValue_boolean)

                If status = SentinelKey.SP_SUCCESS Then
                    If readValue_boolean Then
                        Return eSafeNetStatusCodes.SP_SUCCESS_TRUE
                    Else
                        Return eSafeNetStatusCodes.SP_SUCCESS_FALSE
                    End If
                Else
                    Return status
                End If
            Catch ex As Exception
                Throw New Exception("Problem with BoolFeatureQuery(). Error: " & ex.Message)
            End Try
        End Function

        Public Function ReleaseLicense() As eSafeNetStatusCodes
            Try
                Return oSentinelKey.SFNTReleaseLicense()
            Catch ex As Exception
                Throw New Exception("Problem with ReleaseLicense(). Error: " & ex.Message)
            End Try
        End Function

        Public Function GetDeviceInfo(ByRef DI As SentinelKey.DeviceInfo) As eSafeNetStatusCodes
            Try
                Return oSentinelKey.SFNTGetDeviceInfo(DI)
            Catch ex As Exception
                Throw New Exception("Problem with GetDeviceInfo(). Error: " & ex.Message)
            End Try
        End Function

        Protected Function GetFeatureInfo(ByVal Feature As Integer, ByRef nFeatureInfo As SentinelKey.FeatureInfo) As eSafeNetStatusCodes
            Try
                Return oSentinelKey.SFNTGetFeatureInfo(Feature, nFeatureInfo)
            Catch ex As Exception
                Throw New Exception("Problem with GetFeatureInfo(). Error: " & ex.Message)
            End Try
        End Function

#End Region 'DONGLE IMPLEMENTATION

#Region "SECURE UPDATE"

        Public oSecureUpdate As SecureUpdate

        Public Function GenerateRequestCode() As Boolean
            Dim status As eSafeNetStatusCodes
            Dim DevID As UInt32
            Dim LicenseID As UInt32
            Dim bufferSize As UInt32
            Dim I As Integer
            Dim dummyReqCode(2) As Byte

            If oSecureUpdate Is Nothing Then Return False

            Try
                DevID = Convert.ToUInt32(0)
                LicenseID = Convert.ToUInt32(0)
                bufferSize = Convert.ToUInt32(0)

                status = oSecureUpdate.SFNTGenerateRequestCode(DevID, LicenseID, dummyReqCode, bufferSize)

                If status <> SecureUpdate.SP_ERR_INSUFFICIENT_REQ_CODE_BUFFER Then
                    'if API not able to return the size of request code than do it manually
                    bufferSize = Convert.ToUInt32(SecureUpdate.MAX_REQUEST_CODE)
                Else
                    'The required size of request code buffer is returned in requestedBuffSize by API.
                End If

                Dim requestBuffer(BitConverter.ToInt32(BitConverter.GetBytes(bufferSize), 0)) As Byte
                status = oSecureUpdate.SFNTGenerateRequestCode(DevID, LicenseID, requestBuffer, bufferSize)

                If status <> SecureUpdate.SP_SUCCESS Then
                    Throw New Exception(status.ToString)
                Else
                    Dim saveDlg As SaveFileDialog = New SaveFileDialog
                    ' Sets the Dialog Title to Save File
                    saveDlg.Title = "Save Request Code File"

                    ' Sets the File List box to Request Code File
                    saveDlg.Filter = "Request Code File (*.req)|*.req"
                    saveDlg.DefaultExt = "req"

                    If saveDlg.ShowDialog() = DialogResult.OK Then
                        Dim fs As New FileStream(saveDlg.FileName, FileMode.Create, FileAccess.Write)
                        Dim s As New BinaryWriter(fs)
                        For I = 0 To (Convert.ToInt32(bufferSize) - 1)
                            s.Write(requestBuffer(I))
                        Next
                        s.Close()
                        Return True
                    Else
                        Return False
                    End If
                End If
            Catch excp As DllNotFoundException
                Throw New Exception("Problem with GenerateRequestCode(). SecureUpdate DLL not found.")
            Catch ex As Exception
                Throw New Exception("Problem with GenerateRequestCode(). Error: " & ex.Message)
            End Try
        End Function

        Public Function ApplyUpdateCode() As Boolean
            Dim size As UInt32
            Dim status As eSafeNetStatusCodes

            If oSecureUpdate Is Nothing Then Return False
            Try
                Dim openDlg As OpenFileDialog = New OpenFileDialog
                ' Sets the Dialog Title to Save File
                openDlg.Title = "Open Update Code File"

                ' Sets the File List box to Request Code File
                openDlg.Filter = "Update Code File (*.upw)|*.upw"
                openDlg.DefaultExt = "upw"

                If openDlg.ShowDialog() = DialogResult.OK Then
                    openDlg.CheckFileExists = True

                    Dim fileR As New FileStream(openDlg.FileName, FileMode.Open, FileAccess.Read)

                    Dim fi As New FileInfo(openDlg.FileName)

                    size = Convert.ToUInt32(fi.Length())
                    Dim updateCode(fi.Length() - 1) As Byte

                    fileR.Read(updateCode, 0, fi.Length())

                    status = oSecureUpdate.SFNTApplyUpdateCode(updateCode, size)

                    If status <> SecureUpdate.SP_SUCCESS Then
                        Throw New Exception(status.ToString)
                    Else
                        Return True
                    End If
                End If
            Catch excp As DllNotFoundException
                Throw New Exception("Problem with ApplyUpdate(). SecureUpdate DLL not found.")
            Catch ex As Exception
                Throw New Exception("Problem with ApplyUpdate(). Error: " & ex.Message())
            End Try
        End Function

        Public Shared Function ApplyNLF(ByVal NlfPth As String) As Boolean
            Try
                If Not File.Exists(NlfPth) Then Throw New Exception("NLF file does not exist.")
                Dim FS As New FileStream(NlfPth, FileMode.Open, FileAccess.Read)
                Dim Size As UInt32 = Convert.ToUInt32(FS.Length)
                Dim updateCode(Size - 1) As Byte
                FS.Read(updateCode, 0, Size)
                FS.Close()
                Dim oSecureUpdate As New SecureUpdate
                Dim status As cSafeNetDongle.eSafeNetStatusCodes = oSecureUpdate.SFNTApplyUpdateCode(updateCode, Size)
                If status <> SecureUpdate.SP_SUCCESS Then
                    Throw New Exception("Update failed: " & status.ToString)
                Else
                    Return True
                End If
            Catch ex As Exception
                Throw New Exception("Problem with ApplyNLF(). Error: " & ex.Message, ex)
            End Try
        End Function

#End Region 'SECURE UPDATE

#Region "SAFENET STATUS CODES"

        Public Const SP_DRIVER_LIBRARY_ERROR_BASE As Integer = 100
        Public Const SP_DUAL_LIBRARY_ERROR_BASE As Integer = 200
        Public Const SP_SERVER_ERROR_BASE As Integer = 300
        Public Const SP_SHELL_ERROR_BASE As Integer = 400
        Public Const SP_SECURE_UPDATE_ERROR_BASE As Integer = 500

        Public Enum eSafeNetStatusCodes As Integer
            SP_SUCCESS_TRUE = 32778
            SP_SUCCESS_FALSE = 32779
            SP_ERR_GENERIC = 32780

            SP_SUCCESS = 0
            SP_DRIVER_LIBRARY_ERROR_BASE = 100
            SP_DUAL_LIBRARY_ERROR_BASE = 200
            SP_SERVER_ERROR_BASE = 300
            SP_SHELL_ERROR_BASE = 400
            SP_SECURE_UPDATE_ERROR_BASE = 500
            SP_SETUP_LIBRARY_ERROR_BASE = 700

            'Dual Library Error Codes:
            SP_ERR_INVALID_PARAMETER = (SP_DUAL_LIBRARY_ERROR_BASE + 1)
            SP_ERR_SOFTWARE_KEY = (SP_DUAL_LIBRARY_ERROR_BASE + 2)
            SP_ERR_INVALID_LICENSE = (SP_DUAL_LIBRARY_ERROR_BASE + 3)
            SP_ERR_INVALID_FEATURE = (SP_DUAL_LIBRARY_ERROR_BASE + 4)
            SP_ERR_INVALID_TOKEN = (SP_DUAL_LIBRARY_ERROR_BASE + 5)
            SP_ERR_NO_LICENSE = (SP_DUAL_LIBRARY_ERROR_BASE + 6)
            SP_ERR_INSUFFICIENT_BUFFER = (SP_DUAL_LIBRARY_ERROR_BASE + 7)
            SP_ERR_VERIFY_FAILED = (SP_DUAL_LIBRARY_ERROR_BASE + 8)
            SP_ERR_CANNOT_OPEN_DRIVER = (SP_DUAL_LIBRARY_ERROR_BASE + 9)
            SP_ERR_ACCESS_DENIED = (SP_DUAL_LIBRARY_ERROR_BASE + 10)
            SP_ERR_INVALID_DEVICE_RESPONSE = (SP_DUAL_LIBRARY_ERROR_BASE + 11)
            SP_ERR_COMMUNICATIONS_ERROR = (SP_DUAL_LIBRARY_ERROR_BASE + 12)
            SP_ERR_COUNTER_LIMIT = (SP_DUAL_LIBRARY_ERROR_BASE + 13)
            SP_ERR_MEM_CORRUPT = (SP_DUAL_LIBRARY_ERROR_BASE + 14)
            SP_ERR_INVALID_FEATURE_TYPE = (SP_DUAL_LIBRARY_ERROR_BASE + 15)
            SP_ERR_DEVICE_IN_USE = (SP_DUAL_LIBRARY_ERROR_BASE + 16)
            SP_ERR_INVALID_API_VERSION = (SP_DUAL_LIBRARY_ERROR_BASE + 17)
            SP_ERR_TIME_OUT_ERROR = (SP_DUAL_LIBRARY_ERROR_BASE + 18)
            SP_ERR_INVALID_PACKET = (SP_DUAL_LIBRARY_ERROR_BASE + 19)
            SP_ERR_KEY_NOT_ACTIVE = (SP_DUAL_LIBRARY_ERROR_BASE + 20)
            SP_ERR_FUNCTION_NOT_ENABLED = (SP_DUAL_LIBRARY_ERROR_BASE + 21)
            SP_ERR_DEVICE_RESET = (SP_DUAL_LIBRARY_ERROR_BASE + 22)
            SP_ERR_TIME_CHEAT = (SP_DUAL_LIBRARY_ERROR_BASE + 23)
            SP_ERR_INVALID_COMMAND = (SP_DUAL_LIBRARY_ERROR_BASE + 24)
            SP_ERR_RESOURCE = (SP_DUAL_LIBRARY_ERROR_BASE + 25)
            SP_ERR_UNIT_NOT_FOUND = (SP_DUAL_LIBRARY_ERROR_BASE + 26)
            SP_ERR_DEMO_EXPIRED = (SP_DUAL_LIBRARY_ERROR_BASE + 27)
            SP_ERR_QUERY_TOO_LONG = (SP_DUAL_LIBRARY_ERROR_BASE + 28)
            SP_ERR_USER_AUTH_REQUIRED = (SP_DUAL_LIBRARY_ERROR_BASE + 29)
            SP_ERR_DUPLICATE_LIC_ID = (SP_DUAL_LIBRARY_ERROR_BASE + 30)
            SP_ERR_DECRYPTION_FAILED = (SP_DUAL_LIBRARY_ERROR_BASE + 31)
            SP_ERR_BAD_CHKSUM = (SP_DUAL_LIBRARY_ERROR_BASE + 32)
            SP_ERR_BAD_LICENSE_IMAGE = (SP_DUAL_LIBRARY_ERROR_BASE + 33)
            SP_ERR_INSUFFICIENT_MEMORY = (SP_DUAL_LIBRARY_ERROR_BASE + 34)

            'Server Error Codes
            SP_ERR_SERVER_PROBABLY_NOT_UP = (SP_SERVER_ERROR_BASE + 1)
            SP_ERR_UNKNOWN_HOST = (SP_SERVER_ERROR_BASE + 2)
            SP_ERR_BAD_SERVER_MESSAGE = (SP_SERVER_ERROR_BASE + 3)
            SP_ERR_NO_LICENSE_AVAILABLE = (SP_SERVER_ERROR_BASE + 4)
            SP_ERR_INVALID_OPERATION = (SP_SERVER_ERROR_BASE + 5)
            SP_ERR_INTERNAL_ERROR = (SP_SERVER_ERROR_BASE + 6)
            SP_ERR_PROTOCOL_NOT_INSTALLED = (SP_SERVER_ERROR_BASE + 7)
            SP_ERR_BAD_CLIENT_MESSAGE = (SP_SERVER_ERROR_BASE + 8)
            SP_ERR_SOCKET_OPERATION = (SP_SERVER_ERROR_BASE + 9)
            SP_ERR_NO_SERVER_RESPONSE = (SP_SERVER_ERROR_BASE + 10)


            ' Shell Error Codes
            SP_ERR_BAD_ALGO = (SP_SHELL_ERROR_BASE + 1)
            SP_ERR_LONG_MSG = (SP_SHELL_ERROR_BASE + 2)
            SP_ERR_READ_ERROR = (SP_SHELL_ERROR_BASE + 3)
            SP_ERR_NOT_ENOUGH_MEMORY = (SP_SHELL_ERROR_BASE + 4)
            SP_ERR_CANNOT_OPEN = (SP_SHELL_ERROR_BASE + 5)
            SP_ERR_WRITE_ERROR = (SP_SHELL_ERROR_BASE + 6)
            SP_ERR_CANNOT_OVERWRITE = (SP_SHELL_ERROR_BASE + 7)
            SP_ERR_INVALID_HEADER = (SP_SHELL_ERROR_BASE + 8)
            SP_ERR_TMP_CREATE_ERROR = (SP_SHELL_ERROR_BASE + 9)
            SP_ERR_PATH_NOT_THERE = (SP_SHELL_ERROR_BASE + 10)
            SP_ERR_BAD_FILE_INFO = (SP_SHELL_ERROR_BASE + 11)
            SP_ERR_NOT_WIN32_FILE = (SP_SHELL_ERROR_BASE + 12)
            SP_ERR_INVALID_MACHINE = (SP_SHELL_ERROR_BASE + 13)
            SP_ERR_INVALID_SECTION = (SP_SHELL_ERROR_BASE + 14)
            SP_ERR_INVALID_RELOC = (SP_SHELL_ERROR_BASE + 15)
            SP_ERR_CRYPT_ERROR = (SP_SHELL_ERROR_BASE + 16)
            SP_ERR_SMARTHEAP_ERROR = (SP_SHELL_ERROR_BASE + 17)
            SP_ERR_IMPORT_OVERWRITE_ERROR = (SP_SHELL_ERROR_BASE + 18)

            SP_ERR_FRAMEWORK_REQUIRED = (SP_SHELL_ERROR_BASE + 21)
            SP_ERR_CANNOT_HANDLE_FILE = (SP_SHELL_ERROR_BASE + 22)

            SP_ERR_STRONG_NAME = (SP_SHELL_ERROR_BASE + 26)
            SP_ERR_FRAMEWORK_10 = (SP_SHELL_ERROR_BASE + 27)
            SP_ERR_FRAMEWORK_SDK_10 = (SP_SHELL_ERROR_BASE + 28)
            SP_ERR_FRAMEWORK_11 = (SP_SHELL_ERROR_BASE + 29)
            SP_ERR_FRAMEWORK_SDK_11 = (SP_SHELL_ERROR_BASE + 30)
            SP_ERR_FRAMEWORK_20 = (SP_SHELL_ERROR_BASE + 31)
            SP_ERR_FRAMEWORK_SDK_20 = (SP_SHELL_ERROR_BASE + 32)
            SP_ERR_APP_NOT_SUPPORTED = (SP_SHELL_ERROR_BASE + 33)
            SP_ERR_FILE_COPY = (SP_SHELL_ERROR_BASE + 34)
            SP_ERR_HEADER_SIZE_EXCEED = (SP_SHELL_ERROR_BASE + 35)

            ' CMDShell Error codes
            SP_ERR_PARAMETER_MISSING = (SP_SHELL_ERROR_BASE + 50)
            SP_ERR_PARAMETER_IDENTIFIER_MISSING = (SP_SHELL_ERROR_BASE + 51)
            SP_ERR_PARAMETER_INVALID = (SP_SHELL_ERROR_BASE + 52)
            SP_ERR_REGISTRY = (SP_SHELL_ERROR_BASE + 54)
            SP_ERR_VERIFY_SIGN = (SP_SHELL_ERROR_BASE + 55)
            SP_ERR_PARAMETER = (SP_SHELL_ERROR_BASE + 56)
            SP_ERR_LICENSE_TEMPLATE_FILE = (SP_SHELL_ERROR_BASE + 57)
            SP_ERR_NO_DEVELOPER_KEY = (SP_SHELL_ERROR_BASE + 58)
            SP_ERR_NO_ENDUSER_KEY = (SP_SHELL_ERROR_BASE + 59)
            SP_ERR_NO_POINT_KEYS = (SP_SHELL_ERROR_BASE + 60)
            SP_ERR_NO_SHELL_FEATURE = (SP_SHELL_ERROR_BASE + 61)
            SP_ERR_SHELL_OPTION_FILE_MISSING = (SP_SHELL_ERROR_BASE + 62)
            SP_ERR_SHELL_OPTION_FILE_FORMAT = (SP_SHELL_ERROR_BASE + 63)
            SP_ERR_SHELL_OPTION_FILE_INVALID = (SP_SHELL_ERROR_BASE + 64)
            SP_ERR_DELETE_LICENSE = (SP_SHELL_ERROR_BASE + 65)
            SP_ERR_FILE_CREATE_FAILED = (SP_SHELL_ERROR_BASE + 66)
            SP_ERR_SHELLFILES_LIMIT = (SP_SHELL_ERROR_BASE + 67)
            SP_ERR_SINGLE_INSTANCE_ERROR = (SP_SHELL_ERROR_BASE + 68)
            SP_ERR_NO_EXE_FILE = (SP_SHELL_ERROR_BASE + 69)

            'Secure Update error codes
            SP_ERR_KEY_NOT_FOUND = (SP_SECURE_UPDATE_ERROR_BASE + 1)
            SP_ERR_ILLEGAL_UPDATE = (SP_SECURE_UPDATE_ERROR_BASE + 2)
            SP_ERR_DLL_LOAD_ERROR = (SP_SECURE_UPDATE_ERROR_BASE + 3)
            SP_ERR_NO_CONFIG_FILE = (SP_SECURE_UPDATE_ERROR_BASE + 4)
            SP_ERR_INVALID_CONFIG_FILE = (SP_SECURE_UPDATE_ERROR_BASE + 5)
            SP_ERR_UPDATE_WIZARD_NOT_FOUND = (SP_SECURE_UPDATE_ERROR_BASE + 6)
            SP_ERR_UPDATE_WIZARD_SPAWN_ERROR = (SP_SECURE_UPDATE_ERROR_BASE + 7)
            SP_ERR_EXCEPTION_ERROR = (SP_SECURE_UPDATE_ERROR_BASE + 8)
            SP_ERR_INVALID_CLIENT_LIB = (SP_SECURE_UPDATE_ERROR_BASE + 9)
            SP_ERR_CABINET_DLL = (SP_SECURE_UPDATE_ERROR_BASE + 10)
            SP_ERR_INSUFFICIENT_REQ_CODE_BUFFER = (SP_SECURE_UPDATE_ERROR_BASE + 11)
            SP_ERR_UPDATE_WIZARD_USER_CANCELED = (SP_SECURE_UPDATE_ERROR_BASE + 12)

            ' New Error codes defined for license addition
            SP_ERR_INVALID_DLL_VERSION = (SP_SECURE_UPDATE_ERROR_BASE + 13)
            SP_ERR_INVALID_FILE_TYPE = (SP_SECURE_UPDATE_ERROR_BASE + 14)

            SP_ERR_BAD_XML = (SP_SETUP_LIBRARY_ERROR_BASE + 1)
            SP_ERR_BAD_PACKET = (SP_SETUP_LIBRARY_ERROR_BASE + 2)
            SP_ERR_BAD_FEATURE = (SP_SETUP_LIBRARY_ERROR_BASE + 3)
            SP_ERR_BAD_HEADER = (SP_SETUP_LIBRARY_ERROR_BASE + 4)
            SP_ERR_ISV_MISSING = (SP_SETUP_LIBRARY_ERROR_BASE + 5)
            SP_ERR_DEVID_MISMATCH = (SP_SETUP_LIBRARY_ERROR_BASE + 6)
            SP_ERR_LM_TOKEN_ERROR = (SP_SETUP_LIBRARY_ERROR_BASE + 7)
            SP_ERR_LM_MISSING = (SP_SETUP_LIBRARY_ERROR_BASE + 8)
            SP_ERR_INVALID_SIZE = (SP_SETUP_LIBRARY_ERROR_BASE + 9)
            SP_ERR_FEATURE_NOT_FOUND = (SP_SETUP_LIBRARY_ERROR_BASE + 10)
            SP_ERR_LICENSE_NOT_FOUND = (SP_SETUP_LIBRARY_ERROR_BASE + 11)
            SP_ERR_BEYOND_RANGE = (SP_SETUP_LIBRARY_ERROR_BASE + 12)

        End Enum

#End Region 'SAFENET STATUS CODES

    End Class 'cSafeNetDongle

End Namespace 'Security.SafeNet
