Imports System.IO

Public Class Form1

    Dim users As New Dictionary(Of String, String)
    Dim fileReader As New StreamReader("Users.txt")

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked = True Then
            TextBox1.PasswordChar = Nothing
        Else
            TextBox1.PasswordChar = "*"
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If users.ContainsKey(TextBox2.Text) Then
            If users(TextBox2.Text) = TextBox1.Text Then
                MessageBox.Show("Successfully Logged In!")
                Dim h As New Form2
                h.myCaller = Me
                h.Show()
                Me.Hide()
            End If
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For n = 1 To File.ReadAllLines("Users.txt").Length
            seperatePartsOfUser(fileReader.ReadLine)
        Next
    End Sub

    Sub seperatePartsOfUser(data As String)
        Dim username As String
        Dim password As String

        Dim arrayOfData() As String = data.Split

        username = arrayOfData(0)
        password = arrayOfData(1)

        users.Add(username, password)
    End Sub
End Class
