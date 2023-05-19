
Namespace UI.WPF

    Partial Public Class COMPortSelector_Dialog

        Private Sub COMPortSelector_Dialog_Loaded(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles MyBase.Loaded
            For Each n As String In My.Computer.Ports.SerialPortNames
                Me.lbCOMPorts.Items.Add(n)
            Next
            If Me.lbCOMPorts.Items.Count > 0 Then Me.lbCOMPorts.SelectedIndex = 0
            Me.btnOK.Focus()
        End Sub

        Public ReadOnly Property SelectedPort() As String
            Get
                If Me.lbCOMPorts.SelectedItem Is Nothing Then Return Nothing
                Return Me.lbCOMPorts.SelectedItem.ToString
            End Get
        End Property

        Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnOK.Click
            Me.DialogResult = True
            Me.Close()
        End Sub

        Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnCancel.Click
            Me.DialogResult = False
            Me.Close()
        End Sub

    End Class

End Namespace
