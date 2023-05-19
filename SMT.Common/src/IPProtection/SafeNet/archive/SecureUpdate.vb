'***************************************************************'
' SecureUpdate.vb                                               '
'                                                               '
' Developed by     : SafeNet,Inc                                '
'                                                               '
' (C) Copyright 2007 SafeNet,Inc. All rights reserved.          '
'                                                               '
' Description - VB .Net implementation of Secure Update Api's   '
'                                                               '
'***************************************************************'

Namespace IPProtection.SafeNet


    Public Class SecureUpdate
        Implements IDisposable

#Region "Constructor / Destructor"

        Private disposed As Boolean = False
        Private strError As String = "Unable to load the required SecureUpdate.dll." + vbCrLf + "Either the DLL is missing or corrupted."
        'Sentinel Keys Constructor
        Public Sub New()

        End Sub

        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
        End Sub

        Private Overloads Sub Dispose(ByVal disposing As Boolean)
            ' Check to see if Dispose has already been called.
            disposed = True
        End Sub

        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region

#Region "SecureUpdate API Error Codes"

        Public Const SP_SUCCESS = 0
        Public Const SP_ERR_KEY_NOT_FOUND = 501
        Public Const SP_ERR_ILLEGAL_UPDATE = 502
        Public Const SP_ERR_DLL_LOAD_ERROR = 503
        Public Const SP_ERR_NO_CONFIG_FILE = 504
        Public Const SP_ERR_INVALID_CONFIG_FILE = 505
        Public Const SP_ERR_UPDATE_WIZARD_NOT_FOUND = 506
        Public Const SP_ERR_UPDATE_WIZARD_SPAWN_ERROR = 507
        Public Const SP_ERR_EXCEPTION_ERROR = 508
        Public Const SP_ERR_INVALID_CLIENT_LIB = 509
        Public Const SP_ERR_CABINET_DLL = 510
        Public Const SP_ERR_INSUFFICIENT_REQ_CODE_BUFFER = 511
        Public Const MAX_REQUEST_CODE = 8192    'Change according to your need

#End Region

#Region "//////////////////////// BEGIN PUBLIC METHODS	///////////////////////////////////"

        Public Function SFNTGenerateRequestCode(ByVal DevID As UInt32, _
                                                ByVal LicenseID As UInt32, _
                                                ByVal requestBuffer As Byte(), _
                                                ByRef bufferSize As UInt32) As Int32
            Dim status As Int32

            Try
                status = SecureUpdateNativeAPI.SFNTGenerateRequestCode(DevID, LicenseID, requestBuffer, bufferSize)
            Catch e As DllNotFoundException
                Throw New DllNotFoundException(strError, e)
            End Try
            Return status
        End Function

        Public Function SFNTApplyUpdateCode(ByVal updateCode As Byte(), _
                                            ByVal size As UInt32) As Int32
            Dim status As Int32
            Try
                status = SecureUpdateNativeAPI.SFNTApplyUpdateCode(updateCode, size)
            Catch e As DllNotFoundException
                Throw New DllNotFoundException(strError, e)
            End Try
            Return status
        End Function
#End Region

    End Class

End Namespace 'Secuirty.SafeNet
