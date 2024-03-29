﻿Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Public Class Form1

    Public myCaller As Form2

    Public userID As Integer = Nothing

    Dim client As TcpClient
    Dim receiveInputThread As New Thread(AddressOf receiveFromServer)

    Dim isActive As Boolean = True

    Public privateKey(1) As Integer

    Private Sub CheckBoxChange(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked = True Then
            TextBox1.PasswordChar = Nothing
        Else
            TextBox1.PasswordChar = "*"
        End If
    End Sub

    Private Sub FormLoad(sender As Object, e As EventArgs) Handles MyBase.Load

        Try
            client = New TcpClient("147.147.67.94", 50005)

            CheckForIllegalCrossThreadCalls = False

        Catch ex As Exception
            MsgBox("Could not connect to server, make sure it is online.")
            Me.Close()
        End Try

        receiveInputThread.Start()

    End Sub

    Sub CheckUsernamePassword() Handles Button1.Click
        If TextBox2.Text.Contains(" ") Or TextBox1.Text.Contains(" ") Then
            MessageBox.Show("INVALID USERNAME OR PASSWORD: Please Remove All Spaces In Username.")
        Else
            SendAccountToServer(3)
        End If

    End Sub

    Private Sub CreateNewUser() Handles Button2.Click
        If TextBox2.Text.Contains(" ") Or TextBox1.Text.Contains(" ") Then
            MessageBox.Show("INVALID USERNAME OR PASSWORD: Please Remove All Spaces In Username.")
        Else
            SendAccountToServer(4)
        End If


    End Sub

    Sub ExitServer()
        Dim byteStream As NetworkStream = client.GetStream

        Dim exitRequest(10) As Byte
        exitRequest(3) = 5

        byteStream.Write(exitRequest, 0, exitRequest.Length)

    End Sub

    Sub ReceiveFromServer()
        Dim receiveStream As NetworkStream = client.GetStream

        While True
            Dim input(100000) As Byte

            receiveStream.Read(input, 0, input.Length)

            If input(3) = 3 Then 'if response from server is in get password mode
                If input(4) = 1 Then '4th byte is either true or false (1 or 0)
                    MessageBox.Show("Successfully Logged In!")

                    userID = input(5)
                    privateKey = {input(6), input(7)}
                    ExitServer()
                    Exit While
                Else
                    MessageBox.Show("Incorrect Username or Password.")
                End If
            ElseIf input(3) = 4 Then 'if response is in create user mode
                If input(4) = 1 Then '4th byte is true or false whether it could create the account
                    MessageBox.Show("Account Successfully Created!")
                Else
                    MessageBox.Show("INVALID USERNAME: Username Already Taken, Try A Different One.")
                End If
            End If

        End While

    End Sub

    Sub TextToArray(text As String, ByRef message() As Byte, ByRef startLocation As Integer)
        For Each character In text
            message(startLocation) = Asc(character)
            startLocation += 1
        Next
    End Sub

    Private Sub TimerTicks(sender As Object, e As EventArgs) Handles Timer1.Tick
        If userID <> Nothing And isActive Then
            Dim h As New Form2
            h.myCaller = Me
            h.Show()
            h.Location = Me.Location

            Me.Hide()
            isActive = False
        End If
    End Sub

    Private Sub ClosingForm(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Application.Exit()
        End
    End Sub

    Sub SendAccountToServer(sendmode As Integer)

        Dim byteStream As NetworkStream = client.GetStream

        Dim username As String = TextBox2.Text

        Dim password As String = TextBox1.Text

        Dim currentIndex As Integer = 5
        Dim message(1000000) As Byte

        message(3) = sendmode
        message(4) = username.Length

        TextToArray(username, message, currentIndex)

        message(currentIndex) = password.Length
        currentIndex += 1

        TextToArray(password, message, currentIndex)

        byteStream.Write(message, 0, currentIndex)

    End Sub

End Class
