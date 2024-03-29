﻿Imports System.IO
Imports System.Threading
Imports System.Net.Sockets

Public Class Form2

    Public myCaller As Form1

    Public selectedChat As Integer
    Public selectedChatName As String

    Public privateKey() As Integer

    Dim buttons() As Button = Nothing
    Dim buttonsBuffer() As Button = Nothing

    Public userID As Integer

    Dim serverListener As New Thread(AddressOf receiveFromServer)

    Dim client As New TcpClient

    Dim chats As New Dictionary(Of String, Integer)

    Private Sub LogoutButton() Handles Button5.Click
        ExitServer()
        System.Diagnostics.Process.Start(Application.ExecutablePath)
        Application.Exit()
        End
    End Sub

    Private Sub Form2_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Label1.Text = "logged in as: " & myCaller.TextBox2.Text
        userID = myCaller.userID
        privateKey = myCaller.privateKey

        Try
        startClient()

        Catch ex As Exception
            MsgBox("Could not connect to server, make sure it is online.")
            Application.Exit()
            End
        End Try
    End Sub

    Sub ExitServer()
        Dim byteStream As NetworkStream = client.GetStream

        Dim exitRequest(10) As Byte
        exitRequest(3) = 5

        byteStream.Write(exitRequest, 0, exitRequest.Length)

    End Sub

    Sub ReceiveFromServer()
        Dim receiveStream As NetworkStream = client.GetStream
        Try
            While True
                Dim input(100000) As Byte

                receiveStream.Read(input, 0, input.Length)

                If input(3) = 7 Then 'if response from server is in get chat mode

                    ReceiveChats(input)

                End If

            End While
        Catch ex As Exception
            MessageBox.Show(ex.Message & vbNewLine & "ERROR: Something went wrong")
            Application.Restart()
            End
        End Try

    End Sub

    Protected Sub ButtonClick(ByVal sender As System.Object, ByVal e As System.EventArgs)

        selectedChat = chats(sender.text)
        selectedChatName = sender.text

        Dim h As New Form3

        h.myCaller = Me
        h.Show()
        h.Location = Me.Location

    End Sub

    Private Sub FormClose() Handles MyBase.FormClosing
        Application.Exit()
        End
    End Sub

    Private Sub AddChatButton() Handles Button1.Click
        Dim h As New Add_Chat
        h.Show()
    End Sub

    Sub RequestChats() Handles Button2.Click

        chats = New Dictionary(Of String, Integer)

        Dim sendToServer As NetworkStream = client.GetStream

        Dim message(5) As Byte

        message(1) = userID
        message(3) = 7

        sendToServer.Write(message, 0, message.Length)
    End Sub

    Sub ReceiveChats(message() As Byte)

        chats = New Dictionary(Of String, Integer)

        Dim numberOfChats As Integer = message(5)
        Dim currentIndex As Integer = 6

        For n = 1 To numberOfChats

            Dim chatName As String = Nothing
            Dim chatID As Integer = message(currentIndex)

            currentIndex += 1

            Dim chatNameLength As Integer = message(currentIndex)

            currentIndex += 1

            For h = 1 To chatNameLength
                chatName += Chr(message(currentIndex))
                currentIndex += 1
            Next

            chats.Add(chatName, chatID)

        Next

        Dim newButtons(chats.Count - 1) As Button
        Dim buttonIndex As Integer = 0

        buttons = newButtons

        For Each chat In chats
            buttons(buttonIndex) = New Button
            buttons(buttonIndex).Location = New Point(12, 93 + 73 * buttonIndex)
            buttons(buttonIndex).Size = New Size(413, 67)
            buttons(buttonIndex).Name = chat.Value
            buttons(buttonIndex).Text = chat.Key 'cannot be created here as it is on the wrong thread so done in the timer
            'Controls.Add(buttons(buttonIndex))
            buttons(buttonIndex).BringToFront()
            AddHandler buttons(buttonIndex).Click, AddressOf Me.ButtonClick
            buttonIndex += 1
        Next
    End Sub

    Sub checkForNewButtons() Handles Timer1.Tick

        If buttons IsNot buttonsBuffer Then
            If buttonsBuffer IsNot Nothing Then
                For Each button In buttonsBuffer
                    Controls.Remove(button)
                Next
            End If
            For Each button In buttons
                Controls.Add(button)
            Next
        End If
        buttonsBuffer = buttons
    End Sub

    Sub StartClient()

        client = New TcpClient("195.99.55.53", 50005)

        CheckForIllegalCrossThreadCalls = False

        buttons = Nothing
        requestChats()

        If serverListener.IsAlive Then
            serverListener.Abort()
        End If
        serverListener = New Thread(AddressOf receiveFromServer)
        serverListener.Start()
    End Sub
End Class