Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

Public Class Form3

    Inherits System.Windows.Forms.Form

    Public myCaller As Form2

    Dim client As TcpClient

    Dim userID As Integer

    Dim chatID As Integer

    Dim receiveMessageThread As New Thread(AddressOf ReceiveMessages)

    Dim usersInChat As New List(Of Integer)

    Private Sub Form3_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Label1.Text = myCaller.selectedChat

        userID = myCaller.userID
        chatID = myCaller.selectedChat

        Try
            Dim ip As String = "147.147.67.93"
            Dim port As Integer = 50005

            client = New TcpClient(ip, port)

            CheckForIllegalCrossThreadCalls = False

            receiveMessageThread.Start()

            AcceptButton = Button1

            sendToServer("", 8, 0)

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    Private Sub ReceiveMessages(state As Object)
        Try
            While True

                Dim receiveStream As NetworkStream = client.GetStream()

                Dim message(100000) As Byte
                receiveStream.Read(message, 0, message.Length)

                If message IsNot Nothing Then
                    Select Case message(3)
                        Case 0
                            Dim txt As String = arrayToString(message)
                            If RichTextBox1.TextLength > 0 Then
                                RichTextBox1.Text &= vbNewLine & txt
                            Else
                                RichTextBox1.Text = txt
                            End If
                        Case 8
                            For n = 1 To message(5)
                                usersInChat.Add(message(5 + n))
                            Next
                    End Select
                End If
            End While
        Catch ex As Exception
            MessageBox.Show(ex.Message & vbNewLine & "You are likely sending messages too fast, try slowing down.")
        End Try
    End Sub

    Function arrayToString(message() As Byte)

        Dim stringOutput As String = Nothing

        For n = 0 To message.Length - 1
            If message(n) <> Nothing Then
                stringOutput += Chr(message(n))
            End If
        Next

        Return stringOutput

    End Function

    Sub sendToServer(text As String, sendmode As Integer, targetUser As Integer)

        Dim sendToServer As NetworkStream = client.GetStream

        Dim message(100000) As Byte

        message(0) = chatID
        message(1) = userID
        message(3) = sendmode

        Dim n As Integer = 4

        For Each H In text
            message(n) = Asc(H)
            n += 1
        Next

        sendToServer.Write(message, 0, message.Length)

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        myCaller.Location = Me.Location
        myCaller.Show()
        myCaller.startClient()
        exitServer()
        Me.Hide()
    End Sub


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If TextBox1.Text <> "" Then
            Try
                sendToServer(TextBox1.Text, 0, 0)

                TextBox1.Text = ""

            Catch ex As Exception

            End Try
        End If
    End Sub

    Private Sub Form3_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        exitServer()
        Application.Exit()
        End
    End Sub

    Sub exitServer()
        Dim byteStream As NetworkStream = client.GetStream

        Dim exitRequest(10) As Byte
        exitRequest(3) = 5

        byteStream.Write(exitRequest, 0, exitRequest.Length)

    End Sub

End Class