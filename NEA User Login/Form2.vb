Imports System.IO
Imports System.Threading
Public Class Form2
    Inherits System.Windows.Forms.Form

    Public myCaller As Form1

    Public selectedChat As String = Nothing

    Dim buttons() As Button

    Dim mainThread As Thread = Thread.CurrentThread

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Dim h As New Form1
        h.Show()
        h.myCaller = Me
        Me.Hide()

    End Sub

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Label1.Text = "logged in as: " & myCaller.TextBox2.Text
        loadButtons()
    End Sub

    Sub loadButtons()
        Dim sr As New StreamReader("HH.txt")
        Dim newButtons(File.ReadAllLines("HH.txt").Length - 1) As Button
        buttons = newButtons

        For n = 0 To File.ReadAllLines("HH.txt").Length - 1
            buttons(n) = New Button
            buttons(n).Location = New Point(12, 93 + 73 * n)
            buttons(n).Size = New Size(400, 67)
            buttons(n).Name = "Button" & n
            buttons(n).Text = sr.ReadLine
            Controls.Add(buttons(n))
            buttons(n).BringToFront()
            AddHandler buttons(n).Click, AddressOf Me.ButtonClick
        Next
    End Sub
    Protected Sub ButtonClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        selectedChat = sender.text
        Dim h As New Form3
        Form3.myCaller = Me
        Form3.Show()
        Me.Hide()
    End Sub

    Private Sub Form2_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Application.Exit()

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim h As New Add_Chat
        h.Show()
    End Sub
End Class