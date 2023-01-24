Imports System.Net.Sockets
Imports System.Threading

Public Class Add_Chat

    Dim client As TcpClient

    Dim listener As New Thread(AddressOf receiveFromServer)

    Dim users As New Dictionary(Of String, Integer)

    Dim chats As New Dictionary(Of String, Integer)

    Private Sub Add_Chat_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            client = New TcpClient("147.147.67.93", 50005)

            CheckForIllegalCrossThreadCalls = False

            Dim clientRequest(10) As Byte
            clientRequest(3) = 1

            listener.Start()

            client.GetStream.Write(clientRequest, 0, clientRequest.Length)

        Catch ex As Exception
            MsgBox("Could not connect to server, make sure it is online.")
            Me.Close()
        End Try

    End Sub

    Sub receiveFromServer()

        While True
            Dim receive As NetworkStream = client.GetStream

            Dim input(100000) As Byte

            receive.Read(input, 0, input.Length)

            If input(3) = 1 Then
                arrayToUsers(input)
            End If

        End While
    End Sub

    Sub arrayToUsers(message() As Byte)

        Dim currentIndex As Integer = 4

        While message(currentIndex) <> Nothing
            Dim userLength As Integer = message(currentIndex)
            currentIndex += 1
            Dim ID As Integer = message(currentIndex)
            Dim username As String = Nothing
            currentIndex += 1
            For n = 2 To userLength
                username += Chr(message(currentIndex))
                currentIndex += 1
            Next
            users.Add(username, ID)

        End While

        For Each user In users
            CheckedListBox1.Items.Add(user.Key)
        Next
    End Sub

    Private Sub Add_Chat_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        listener.Abort()
        Dim closeMessage(5) As Byte
        closeMessage(3) = 5

        client.GetStream.Write(closeMessage, 0, closeMessage.Length)

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Dim chatName As String = TextBox1.Text
        Dim message(100000) As Byte
        Dim currentIndex As Integer = 5

        message(3) = 6
        message(4) = chatName.Length

        textToArray(chatName, message, currentIndex)
        For Each user In CheckedListBox1.CheckedItems
            message(currentIndex) = users(user)
            currentIndex += 1
        Next

        client.GetStream.Write(message, 0, currentIndex)
    End Sub

    Sub textToArray(text As String, ByRef message() As Byte, ByRef startLocation As Integer)
        For Each character In text
            message(startLocation) = Asc(character)
            startLocation += 1
        Next
    End Sub

End Class