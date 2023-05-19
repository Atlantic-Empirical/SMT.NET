Imports System.Runtime.InteropServices
Imports SMT.Multimedia.DirectShow

Namespace Multimedia.Filters.BlackMagic

    Public Module mDecklinkInstallationTester

        Public ReadOnly Property DecklinkInstalled() As Boolean
            Get
                Try
                    Dim f As IBaseFilter = CType(DsBugWO.CreateDsInstance(New Guid("CEB13CC8-3591-45A5-BA0F-20E9A1D72F76"), Clsid.IBaseFilter_GUID), IBaseFilter)
                    Dim out As Boolean = False
                    If f IsNot Nothing Then
                        out = True
                        Marshal.FinalReleaseComObject(f)
                    End If
                    Return out
                Catch ex As Exception
                    Throw New Exception("Problem with DecklinkInstalled(). Error: " & ex.Message)
                End Try
            End Get
        End Property

    End Module

End Namespace
