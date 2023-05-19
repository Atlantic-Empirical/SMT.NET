Imports System.Net.Mail

Namespace Network.Email

    Public Class cSMTEmail

        Public SMTP_Host As String
        Public SMTP_Port As Short
        Public Credentials_Username As String
        Public Credentials_Password As String
        Public UseSSL As Boolean
        Public From As cEmailAddress
        Public [To]() As Email.cEmailAddress
        Public CC() As Email.cEmailAddress
        Public BCC() As Email.cEmailAddress
        Public BodyIsHTML As Boolean
        Public Body As String
        Public Attachments() As String
        Public Subject As String
        Public Priority As System.Net.Mail.MailPriority

        Public Sub New()
        End Sub

        Public Function SendMail() As Boolean
            Try
                Dim SC As New SmtpClient()

                SC.Host = SMTP_Host
                SC.Port = SMTP_Port
                SC.EnableSsl = UseSSL

                If Credentials_Password <> "" And Credentials_Username <> "" Then
                    SC.Credentials = New System.Net.NetworkCredential(Credentials_Username, Credentials_Password)
                End If

                Dim MM As New MailMessage ' the outgoing message
                MM.From = New MailAddress(From.Address, From.DisplayName)
                For Each JA As cEmailAddress In [To]
                    MM.To.Add(JA.ToMailAddress)
                Next
                If Not CC Is Nothing Then
                    For Each JA As cEmailAddress In CC
                        MM.CC.Add(JA.ToMailAddress)
                    Next
                End If
                If Not BCC Is Nothing Then
                    For Each JA As cEmailAddress In BCC
                        MM.Bcc.Add(JA.ToMailAddress)
                    Next
                End If
                MM.Subject = Subject
                MM.IsBodyHtml = BodyIsHTML
                MM.Body = Body
                MM.Priority = Priority

                SC.Send(MM)
                Return True
            Catch ex As Exception
                Throw New Exception("Problem with SendMail(). Error: " & ex.Message)
            End Try
        End Function

        Public Function StringToAddressArray(ByVal Input As String) As String()
            Try
                'TODO: expand to create an array of sJavEmailAddress
                Input = Microsoft.VisualBasic.Replace(Input, " ", "")
                Return Split(Input, ",")
            Catch ex As Exception
                Throw New Exception("Problem with CSVtoArray. Error: " & ex.Message)
            End Try
        End Function

        Public Function CSVToArray(ByVal Input As String) As String()
            Try
                Input = Microsoft.VisualBasic.Replace(Input, " ", "")
                Dim out() As String = Split(Input, ",")
                out = Split(out(0), ";") 'incase they used semicolons instead of commas
                Return out
            Catch ex As Exception
                Throw New Exception("Problem with CSVtoArray. Error: " & ex.Message)
            End Try
        End Function

        Public Function CovertAddressesToJEMAArray(ByVal Input As String) As cEmailAddress()
            Try
                Dim Addresses() As String = CSVToArray(Input)
                Dim JEMA(Addresses.Length - 1) As cEmailAddress
                For s As Short = 0 To UBound(Addresses)
                    JEMA(s) = New cEmailAddress(Addresses(s), "")
                Next
                Return JEMA
            Catch ex As Exception
                Throw New Exception("Problem with ConvertAddressesToJEMAArray(). Error: " & ex.Message)
                Return Nothing
            End Try
        End Function

    End Class

    Public Class cEmailAddress

        Public Address As String
        Public DisplayName As String

        Public Sub New(ByVal nAddress As String)
            Me.Address = nAddress
            Me.DisplayName = ""
        End Sub

        Public Sub New(ByVal nAddress As String, ByVal nDisplayName As String)
            Me.Address = nAddress
            Me.DisplayName = nDisplayName
        End Sub

        Public Function ToMailAddress() As MailAddress
            Return New MailAddress(Address, DisplayName)
        End Function

        Public Overrides Function ToString() As String
            Return DisplayName & " <" & Address & ">"
        End Function

    End Class

End Namespace
