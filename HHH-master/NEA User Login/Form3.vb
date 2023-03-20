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

    Dim users As New List(Of user)

    Dim privateKey() As Integer

    Private Sub Form3_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Label1.Text = myCaller.selectedChatName

        userID = myCaller.userID
        chatID = myCaller.selectedChat
        privateKey = myCaller.privateKey

        Try
            Dim ip As String = "195.99.55.53"
            Dim port As Integer = 50005

            client = New TcpClient(ip, port)

            CheckForIllegalCrossThreadCalls = False

            receiveMessageThread.Start()

            sendToServer("", 8, 0) 'get all IDs on the current chat

            Thread.Sleep(1000)

            sendToServer("", 2, 0) 'get all previous messages from current chat

            AcceptButton = Button1


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
                        Case 10 'if in receive a message mode
                            Dim txt As String = arrayToString(message)
                            If RichTextBox1.TextLength > 0 Then
                                RichTextBox1.Text &= vbNewLine & txt
                            Else
                                RichTextBox1.Text = txt
                            End If
                        Case 8 'if in the mode to receive the users in chat from the server
                            Dim currentIndex As Integer = 6

                            For count = 1 To message(5)

                                Dim userID As Integer = message(currentIndex)
                                currentIndex += 1
                                Dim j As Integer = message(currentIndex)
                                currentIndex += 1
                                Dim n As Integer = message(currentIndex)
                                currentIndex += 1
                                Dim username As String = Nothing
                                For index = 1 To message(currentIndex)
                                    currentIndex += 1
                                    username += Chr(message(currentIndex))
                                Next
                                usersInChat.Add(userID)
                                users.Add(New user(n, j, userID, username))
                                currentIndex += 1
                            Next
                    End Select
                End If
            End While
        Catch ex As Exception
            MessageBox.Show(ex.Message & vbNewLine & "You are likely sending messages too fast, try slowing down.")
            Me.Close()

        End Try
    End Sub

    Function arrayToString(message() As Byte)

        Dim senderUsername As String = Nothing

        For Each user In users
            If user.GetID = message(1) Then
                senderUsername = user.GetUsername
            End If
        Next

        Dim stringOutput As String = senderUsername & ": "

        For n = 4 To message.Length - 1
            If message(n) <> Nothing Then
                stringOutput += Chr(moduloExponent(message(n), privateKey(0), privateKey(1)))

            End If
        Next

        Return stringOutput

    End Function

    Sub sendToServer(text As String, sendmode As Integer, targetUserID As Integer)

        Dim ServerConnection As NetworkStream = client.GetStream

        Dim message(100000) As Byte

        message(0) = chatID
        message(1) = userID
        message(2) = targetUserID
        message(3) = sendmode

        Dim targetUserKey() As Integer

        For Each user In users
            If user.GetID = targetUserID Then
                targetUserKey = user.GetKey
            End If
        Next

        Dim index As Integer = 4

        For Each H In text
            message(index) = moduloExponent(Asc(H), targetUserKey(0), targetUserKey(1))
            index += 1
        Next

        ServerConnection.Write(message, 0, message.Length)

    End Sub

    Function moduloExponent(number As Integer, exponent As Integer, modulus As Integer)

        Dim newNumber As Double = calcFactors(number, exponent, modulus) 'find the simplified version of H^j/H^k

        newNumber = newNumber Mod modulus 'find modulus of H^j/H^k and n

        Return newNumber

    End Function

    Function calcFactors(number, exponent, modulus)
        Dim num As Integer = exponent
        Dim output As Double = 1
        While num <> 0 'calculate each factor in descending order
            Dim e As Integer = 0
            While 2 ^ e <= num
                e += 1
            End While
            e -= 1
            output *= squareSimplify(number, e, modulus) 'simplify each factor and multiply to output as exponents
            output = output Mod modulus 'mods it to stop it from getting too big 
            num -= 2 ^ e
        End While
        Return output
    End Function

    Function squareSimplify(number, times, modulus)
        For x = 1 To times 'squares it the amount necessary
            number ^= 2

            number = number Mod modulus 'mods it to stop it from getting too big
        Next
        Return number
    End Function

    Sub goBackToMenu()

        myCaller.Location = Me.Location
        myCaller.Show()
        myCaller.startClient()
        Try
            exitServer()
        Catch ex As Exception
        End Try

        Me.Hide()
    End Sub


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If TextBox1.Text <> "" Then
            Button1.Enabled = False
            AcceptButton = Nothing
            Try
                For Each user In users
                    sendToServer(TextBox1.Text, 10, user.GetID)
                    Thread.Sleep(100)
                Next

                TextBox1.Text = ""

            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        End If
        Button1.Enabled = True
        AcceptButton = Button1
    End Sub

    Private Sub Form3_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        exitServer()
    End Sub

    Sub exitServer()
        Try
            Dim byteStream As NetworkStream = client.GetStream

            Dim exitRequest(10) As Byte
            exitRequest(3) = 5

            byteStream.Write(exitRequest, 0, exitRequest.Length)

        Catch ex As Exception

        End Try
        receiveMessageThread.Abort()

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        MessageBox.Show(privateKey(0) & " " & privateKey(1))
    End Sub

    Private Sub RichTextBox1_TextChanged(sender As Object, e As EventArgs) Handles RichTextBox1.TextChanged

    End Sub
End Class

Class user
    Dim n As Integer
    Dim j As Integer

    Dim userID As Integer
    Dim username As String

    Sub New(nn As Integer, jj As Integer, ID As Integer, name As String)
        n = nn
        j = jj
        userID = ID
        username = name
    End Sub

    Function GetKey()
        Dim key(1) As Integer
        key(0) = j
        key(1) = n
        Return key
    End Function

    Function GetID()
        Return userID
    End Function

    Function GetUsername()
        Return username
    End Function
End Class