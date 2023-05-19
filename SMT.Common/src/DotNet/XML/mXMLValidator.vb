Imports System.Xml
Imports System.Xml.Schema
Imports System.IO

Namespace DotNet.XML

    Public Class cXMLValidator

        Public Sub New()
            dPassValidationResult = AddressOf HandleValidationResult
        End Sub

        Public Function ValidateXML(ByVal PathToXML As String) As Boolean
            If Not File.Exists(PathToXML) Then Throw New Exception("XML file does not exist.")
            Dim FS As New FileStream(PathToXML, FileMode.OpenOrCreate)
            Try
                Dim xrs As New XmlReaderSettings
                AddHandler xrs.ValidationEventHandler, AddressOf MyValidationEventHandler
                xrs = New XmlReaderSettings
                xrs.ValidationType = ValidationType.None
                xrs.ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings
                Dim xr As XmlReader = XmlReader.Create(FS, xrs)
                FS.Close()
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with ValidateXML. Error: " & ex.Message)
            Finally
                If Not FS Is Nothing Then FS.Close()
            End Try
        End Function

        Public Sub MyValidationEventHandler(ByVal sender As Object, ByVal args As ValidationEventArgs)
            dPassValidationResult.Invoke(args.Message)
        End Sub

        Public ReadOnly Property XMLIsValid() As Boolean
            Get
                Return _XMLIsValid
            End Get
        End Property
        Private _XMLIsValid As Boolean = True

        Public ReadOnly Property ValidationMessage() As String
            Get
                Return _ValidationMessage
            End Get
        End Property
        Private _ValidationMessage As String

        Private Delegate Sub PassValidationResult(ByVal Message As String)
        Private dPassValidationResult As PassValidationResult

        Private Sub HandleValidationResult(ByVal Message As String)
            Me._XMLIsValid = False
            Me._ValidationMessage = Message
        End Sub

    End Class

End Namespace
