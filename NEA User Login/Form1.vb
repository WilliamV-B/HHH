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

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If CheckBox1.Checked = True Then
            TextBox1.PasswordChar = Nothing
        Else
            TextBox1.PasswordChar = "*"
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Try
            client = New TcpClient("195.99.55.53", 50005)

            CheckForIllegalCrossThreadCalls = False

        Catch ex As Exception
            MsgBox("Could not connect to server, make sure it is online.")
            Me.Close()
        End Try

        receiveInputThread.Start()

    End Sub

    Sub checkUsernamePassword() Handles Button1.Click

        sendAccountToServer(3)

    End Sub

    Private Sub createNewUser() Handles Button2.Click
        If TextBox2.Text.Contains(" ") Then
            MessageBox.Show("INVALID USERNAME: Please Remove All Spaces In Username.")
        Else
            sendAccountToServer(4)
        End If


    End Sub

    Sub exitServer()
        Dim byteStream As NetworkStream = client.GetStream

        Dim exitRequest(10) As Byte
        exitRequest(3) = 5

        byteStream.Write(exitRequest, 0, exitRequest.Length)

    End Sub

    Sub receiveFromServer()
        Dim receiveStream As NetworkStream = client.GetStream

        While True
            Dim input(100000) As Byte

            receiveStream.Read(input, 0, input.Length)

            If input(3) = 3 Then 'if response from server is in get password mode
                If input(4) = 1 Then '4th byte is either true or false (1 or 0)
                    MessageBox.Show("Successfully Logged In!")

                    userID = input(5)
                    privateKey = {input(6), input(7)}
                    exitServer()
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

    Sub textToArray(text As String, ByRef message() As Byte, ByRef startLocation As Integer)
        For Each character In text
            message(startLocation) = Asc(character)
            startLocation += 1
        Next
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If userID <> Nothing And isActive Then
            Dim h As New Form2
            h.myCaller = Me
            h.Show()
            h.Location = Me.Location

            Me.Hide()
            isActive = False
        End If
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Application.Exit()
        End
    End Sub

    Sub sendAccountToServer(sendmode As Integer)

        Dim byteStream As NetworkStream = client.GetStream

        Dim username As String = TextBox2.Text
        Dim password As String = TextBox1.Text

        Dim currentIndex As Integer = 5
        Dim message(1000000) As Byte

        message(3) = sendmode
        message(4) = username.Length

        textToArray(username, message, currentIndex)

        message(currentIndex) = password.Length
        currentIndex += 1

        textToArray(password, message, currentIndex)

        byteStream.Write(message, 0, currentIndex)

    End Sub

End Class
