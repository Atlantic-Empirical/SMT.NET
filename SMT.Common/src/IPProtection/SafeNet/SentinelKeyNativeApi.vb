Imports System.Runtime.InteropServices
Public Class SentinelKeyNativeApi
    <CLSCompliant(False)> _
    Public Declare Ansi Function SFNTGetLicense Lib "SentinelKeyW.dll" Alias _
    "SFNTGetLicense" (ByVal devID As UInt32, _
                     ByVal softwareKey As Byte(), _
                     ByVal licID As Int32, _
                     ByVal flags As Int32, _
                     ByRef licHandle As Int32) As System.Int32

    Public Declare Ansi Function SFNTQueryFeature Lib "SentinelKeyW.dll" Alias _
    "SFNTQueryFeature" (ByVal licHandle As Int32, _
                   ByVal featureID As Int32, _
                   ByVal flag As Int32, _
                   ByVal query As Byte(), _
                   ByVal queryLength As Int32, _
                   ByVal response As Byte(), _
                   ByVal responseLength As Int32) As System.Int32

    Public Declare Ansi Function SFNTReadString Lib "SentinelKeyW.dll" Alias _
    "SFNTReadString" (ByVal licHandle As Int32, _
                   ByVal featureID As Int32, _
                   ByVal stringBuffer As Byte(), _
                   ByVal stringLength As Int32) As System.Int32

    Public Declare Ansi Function SFNTWriteString Lib "SentinelKeyW.dll" Alias _
    "SFNTWriteString" (ByVal licHandle As Int32, _
                   ByVal featureID As Int32, _
                   ByVal stringBuffer As Byte(), _
                   ByVal writePassword As Int32) As System.Int32

    Public Declare Ansi Function SFNTReadInteger Lib "SentinelKeyW.dll" Alias _
    "SFNTReadInteger" (ByVal licHandle As Int32, _
                   ByVal featureID As Int32, _
                   ByRef featureValue As Int32) As System.Int32

    Public Declare Ansi Function SFNTWriteInteger Lib "SentinelKeyW.dll" Alias _
    "SFNTWriteInteger" (ByVal licHandle As Int32, _
                   ByVal featureID As Int32, _
                   ByVal featureValue As Int32, _
                   ByVal writePassword As Int32) As System.Int32

    Public Declare Ansi Function SFNTReadRawData Lib "SentinelKeyW.dll" Alias _
    "SFNTReadRawData" (ByVal licHandle As Int32, _
                   ByVal featureID As Int32, _
                   ByVal rawDataBuffer As Byte(), _
                   ByVal offset As Int32, _
                   ByVal length As Int32) As System.Int32

    Public Declare Ansi Function SFNTWriteRawData Lib "SentinelKeyW.dll" Alias _
    "SFNTWriteRawData" (ByVal licHandle As Int32, _
                   ByVal featureID As Int32, _
                   ByVal rawDataBuffer As Byte(), _
                   ByVal offset As Int32, _
                   ByVal length As Int32, _
                   ByVal writePassword As Int32) As System.Int32

    Public Declare Ansi Function SFNTCounterDecrement Lib "SentinelKeyW.dll" Alias _
    "SFNTCounterDecrement" (ByVal licHandle As Int32, _
                   ByVal featureID As Int32, _
                   ByVal decrementValue As Int32) As System.Int32

    Public Declare Ansi Function SFNTEncrypt Lib "SentinelKeyW.dll" Alias _
    "SFNTEncrypt" (ByVal licHandle As Int32, _
                   ByVal featureID As Int32, _
                   ByVal plainBuffer As Byte(), _
                   ByVal cipherBuffer As Byte()) As System.Int32

    Public Declare Ansi Function SFNTDecrypt Lib "SentinelKeyW.dll" Alias _
    "SFNTDecrypt" (ByVal licHandle As Int32, _
                   ByVal featureID As Int32, _
                   ByVal cipherBuffer As Byte(), _
                   ByVal plainBuffer As Byte()) As System.Int32

    Public Declare Ansi Function SFNTSign Lib "SentinelKeyW.dll" Alias _
    "SFNTSign" (ByVal licHandle As Int32, _
                   ByVal featureID As Int32, _
                   ByVal signBuffer As Byte(), _
                   ByVal length As Int32, _
                   ByVal signResult As Byte()) As System.Int32

    Public Declare Ansi Function SFNTVerify Lib "SentinelKeyW.dll" Alias _
    "SFNTVerify" (ByVal licHandle As Int32, _
                   ByVal publicKey As Byte(), _
                   ByVal signBuffer As Byte(), _
                   ByVal length As Int32, _
                   ByVal signResult As Byte()) As System.Int32

    Public Declare Ansi Function SFNTSetHeartbeat Lib "SentinelKeyW.dll" Alias _
    "SFNTSetHeartbeat" (ByVal licHandle As Int32, _
                   ByVal heartbeatValue As Int32) As System.Int32

    Public Declare Ansi Function SFNTGetLicenseInfo Lib "SentinelKeyW.dll" Alias _
    "SFNTGetLicenseInfo" (ByVal licHandle As Int32, _
                        ByVal licenseInfo As Byte()) As System.Int32

    Public Declare Ansi Function SFNTGetFeatureInfo Lib "SentinelKeyW.dll" Alias _
    "SFNTGetFeatureInfo" (ByVal licHandle As Int32, _
                          ByVal featureID As Int32, _
                        ByVal featureInfo As Byte()) As System.Int32

    Public Declare Ansi Function SFNTGetDeviceInfo Lib "SentinelKeyW.dll" Alias _
    "SFNTGetDeviceInfo" (ByVal licHandle As Int32, _
                          ByVal deviceInfo As Byte()) As System.Int32

    Public Declare Ansi Function SFNTGetServerInfo Lib "SentinelKeyW.dll" Alias _
    "SFNTGetServerInfo" (ByVal licHandle As Int32, _
                        ByVal serverInfo As Byte()) As System.Int32

    Public Declare Ansi Function SFNTReleaseLicense Lib "SentinelKeyW.dll" Alias _
    "SFNTReleaseLicense" (ByVal licHandle As Int32) As System.Int32

    Public Declare Ansi Function SFNTSetContactServer Lib "SentinelKeyW.dll" Alias _
    "SFNTSetContactServer" (ByVal serverAddr As Byte()) As System.Int32

    <CLSCompliant(False)> _
    Public Declare Ansi Function SFNTEnumServer Lib "SentinelKeyW.dll" Alias _
    "SFNTEnumServer" (ByVal devID As UInt32, _
                      ByVal licHandle As Int32, _
                      ByVal enumFlag As Int32, _
                      ByVal enumServerInfo As Byte(), _
                      ByRef numLicAvail As Int32) As System.Int32

End Class