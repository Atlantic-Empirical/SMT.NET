'***********************************************************'
' SecureUpdateNativeApi.cs                                  '
'                                                           '
' Developed by     : SafeNet,Inc                            '
'                                                           '
' (C) Copyright 2007 SafeNet,Inc. All rights reserved.      '
'                                                           '
' Description - Secure Update Native Api's                  '
'                                                           '
'***********************************************************'
Imports System.Runtime.InteropServices

Namespace IPProtection.SafeNet

    Public Class SecureUpdateNativeAPI
        Public Declare Ansi Function SFNTGenerateRequestCode Lib "SecureUpdate.dll" Alias _
            "SFNTGenerateRequestCode" (ByVal DevID As UInt32, _
                                            ByVal LicenseID As UInt32, _
                                            <Out()> ByVal requestBuffer As Byte(), _
                                            ByRef bufferSize As UInt32) As Int32

        'Public Declare Ansi Function SFNTApplyUpdateCode Lib "SecureUpdate.dll" Alias _
        '    "SFNTApplyUpdateCode" (<[In](), MarshalAs(UnmanagedType.LPArray)> ByVal updateCode As Byte(), <[In]()> ByVal size As UInt32) As Int32

        Public Declare Ansi Function SFNTApplyUpdateCode Lib "SecureUpdate.dll" Alias _
    "SFNTApplyUpdateCode" (<[In]()> ByVal updateCode As Byte(), <[In]()> ByVal size As UInt32) As Int32

    End Class

End Namespace 'Security.SafeNet
