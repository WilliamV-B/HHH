Imports System.Net
Imports System.Net.Sockets

Public Class Form3
    Inherits System.Windows.Forms.Form

    Public myCaller As Form2

    Dim client As tcpclient

    Private Sub Form3_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Label1.Text = myCaller.selectedChat

        Try
            Dim ip As String = "192.168.1.87"
            Dim port As Integer = 50005

            client = New TcpClient(ip, port)

            CheckForIllegalCrossThreadCalls = False

            Threading.ThreadPool.QueueUserWorkItem(AddressOf ReceiveMessages)

            AcceptButton = Button2
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    Private Sub ReceiveMessages(state As Object)
        Try
            While True

                Dim ns As NetworkStream = client.GetStream()

                Dim toReceive(100000) As Byte
                ns.Read(toReceive, 0, toReceive.Length)
                Dim txt As String = arrayToString(toReceive)

                If RichTextBox1.TextLength > 0 Then
                    RichTextBox1.Text &= vbNewLine & txt
                Else
                    RichTextBox1.Text = txt
                End If
            End While
        Catch ex As Exception
            MsgBox(ex.Message)
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

    Function stringToArray(message As String)

        Dim arrayOutput(message.Length - 1) As Byte

        Dim n As Integer = 0

        For Each character In message
            arrayOutput(n) = Asc(character)
            n += 1
        Next

        Return arrayOutput

    End Function

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        myCaller.Show()
        Me.Close()
    End Sub


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            Dim ns As NetworkStream = client.GetStream

            ns.Write(stringToArray(TextBox1.Text), 0, TextBox1.TextLength)

            TextBox1.Text = ""

        Catch ex As Exception

        End Try
    End Sub
End Class