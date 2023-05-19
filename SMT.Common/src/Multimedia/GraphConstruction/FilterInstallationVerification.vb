Imports System.Runtime.InteropServices
Imports SMT.Multimedia.DirectShow

Namespace Multimedia.GraphConstruction

    Public Module mFilterInstallationVerification

        Public ReadOnly Property nVidia_MPEG2Decoder() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("71E4616A-DB5E-452B-8CA5-71D9CC7805E9"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with nVidiaVideoInstalled. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property nVidia_AudioDecoder() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("6C0BDF86-C36A-4D83-8BDB-312D2EAF409E"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with nVidiaAudioInstalled. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property SMT_AMTC() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("A6512CF0-A47B-45ba-A054-0DB0D4BB87F7"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with AMTCInstalled. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property SMT_AMTC2() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("423C0845-670A-4036-B7D3-F45BA701D118"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with AMTCInstalled. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property SMT_KeystoneSD() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("fd501043-8ebe-11ce-8183-00aa00577da1"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with KeystoneSDInstalled. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property SMT_KeystoneOM() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("fd501027-8ebe-11ce-8183-00aa00577da1"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with SMT_KeystoneOM. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property SMT_KeystoneHD() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("FD5010FF-8EBE-11CE-8183-00AA00577DA1"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with SMT_KeystoneSD. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property SMT_L21G() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("A2957546-A38D-44b9-834E-096AF622EC3D"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with L21GInstalled. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property SMT_DAC() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("B4A7BE85-551D-4594-BDC7-832B09185041"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with DACInstalled. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property SMT_NVS() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("FD501075-8EBE-11CE-8183-00AA00577DA1"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with DACInstalled. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property SMT_YUVSource() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("9A80E199-3BBA-4821-B18B-21BB496F80F8"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with SMT_YUVSource. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property MainConcept_AVCDecoder() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("96B9D0ED-8D13-4171-A983-B84D88D627BE"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with MainConcept_AVCDecoder. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property MainConcept_MPEGDemux() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("136DCBF5-3874-4B70-AE3E-15997D6334F7"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with MainConcept_MPEGDemux. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property MainConcept_MPEGDecoder() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("BC4EB321-771F-4E9F-AF67-37C631ECA106"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with MainConcept_MPEGDecoder. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property MainConcept_VC1Decoder() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("C0046C92-D654-4B34-92D6-C6AC34B7346D"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with MainConcept_VC1Decoder. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property MainConcept_Scaler() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("BEB7FFE8-37BA-4849-AE26-7A10EF20A303"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with MainConcept_Scaler. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property CoreAAC() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("6AC7C19E-8CA0-4E3D-9A9F-2881DE29E0AC"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with CoreAAC. Error: " & ex.Message)
                End Try
            End Get
        End Property

        Public ReadOnly Property HaaliSplitter() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("564FD788-86C9-4444-971E-CC4A243DA150"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with HaaliSplitter. Error: " & ex.Message)
                End Try
            End Get
        End Property

    End Module

End Namespace
