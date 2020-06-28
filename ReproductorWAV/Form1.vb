
Imports System.Deployment.Application
Imports System.Net.Mail
Imports System.Globalization
Imports System.ComponentModel
Imports System.Text
Imports System.Data
Imports System.IO
Imports System.Data.Odbc
Imports DevExpress.XtraEditors
Imports System.Media
Imports System.Security.Cryptography



Public Class Form1


    Dim Ruta As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
    Dim Segundos As Integer = 10
    Dim Cargando As Boolean = False
    Dim audios_externos As Boolean = False
    Dim audios_externos_carpeta
    Dim audios_externos_modo As Integer = 0
    Dim audios_externos_pausa As Integer = 0


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim args As New XtraMessageBoxArgs

        PictureBox1.Width = Me.Width - 20
        PictureBox1.Height = 80
        LinkLabel1.Left = Me.Width / 2 - LinkLabel1.Width / 2
        LinkLabel1.Top = 85



        Dim argumentos As String() = Environment.GetCommandLineArgs()
        If Process.GetProcessesByName _
          (Process.GetCurrentProcess.ProcessName).Length > 1 Then
            XtraMessageBox.Show("SIGMA Monitor ya se está ejecutando en este equipo", "Sesión iniciada", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Application.Exit()
            'ElseIf argumentos.Length <= 1 Then
            '    XtraMessageBox.Show("No se puede iniciar el monitor: Se requiere la cadena de conexión", "Sesión iniciada", MessageBoxButtons.OK, MessageBoxIcon.Error)
            '    Application.Exit()
        Else
            idProceso = Process.GetCurrentProcess.Id
            If argumentos.Length > 1 Then
                cadenaConexion = argumentos(1).ToUpper
            End If
            If cadenaConexion = "" Then
                cadenaConexion = "server=localhost;user id=root;password=usbw;port=3307;Convert Zero Datetime=True;Allow User Variables=True"
            Else
                Dim baseCadenaConexion = "server=localhost;user id=root;password=usbw;port=3307;Convert Zero Datetime=True;Allow User Variables=True"
                Dim arreParametros = baseCadenaConexion.Split(New Char() {";"c})
                Dim arreConexion = cadenaConexion.Split(New Char() {";"c})

                If arreConexion.Length > 0 Then
                    cadenaConexion = ""
                    For i = LBound(arreParametros) To UBound(arreParametros)
                        Dim variablesValores1 = arreParametros(i).Split(New Char() {"="c})
                        Dim encontrado As Integer = -1
                        For j = LBound(arreConexion) To UBound(arreConexion)
                            Dim variablesValores2 = arreConexion(j).Split(New Char() {"="c})
                            If variablesValores1(0).ToUpper = variablesValores2(0).ToUpper Then
                                encontrado = j
                                Exit For
                            End If
                        Next
                        If encontrado = -1 Then
                            cadenaConexion = cadenaConexion & arreParametros(i).ToLower & ";"
                        Else
                            cadenaConexion = cadenaConexion & arreConexion(encontrado).ToLower & ";"
                        End If
                    Next
                End If
            End If
            estadoPrograma = True

            Dim regsAfectados = 0
            'Escalada 4


            cadSQL = "SELECT * FROM " & rutaBD & ".configuracion"
            readerDS = consultaSEL(cadSQL)
            If readerDS.Tables(0).Rows.Count > 0 Then
                Dim reader As DataRow = readerDS.Tables(0).Rows(0)
                audios_externos = ValNull(reader!audios_externos, "A") = "S"
                audios_externos_modo = ValNull(reader!audios_externos_modo, "N")
                audios_externos_pausa = ValNull(reader!audios_externos_pausa, "N")
                audios_externos_carpeta = ValNull(reader!audios_externos_carpeta, "A")
            End If
            If audios_externos Then
                Me.Text = "Modo audios externos"
                If audios_externos_carpeta.Length = 0 Then
                    audios_externos_carpeta = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                Else
                    audios_externos_carpeta = Strings.Replace(audios_externos_carpeta, "/", "\")
                End If

                If Not My.Computer.FileSystem.DirectoryExists(audios_externos_carpeta) Then
                    Try
                        My.Computer.FileSystem.CreateDirectory(audios_externos_carpeta)
                    Catch ex As Exception
                        audios_externos_carpeta = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    End Try
                End If
            Else
                Me.Text = "Modo generación de audios"
                Try

                    Dim miReader As StreamReader = My.Computer.FileSystem.OpenTextFileReader(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\rutadeaudo.txt")
                    Dim elMensaje As String = miReader.ReadLine
                    Dim MICadena() As String = Split(elMensaje, ",")
                    Ruta = MICadena(0)
                    Segundos = Val(MICadena(1))
                    If Segundos = 0 Then Segundos = 10

                Catch ex As Exception

                End Try

                If Ruta.Trim.Length = 0 Or Not My.Computer.FileSystem.DirectoryExists(Ruta) Then
                    Ruta = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    Dim file As System.IO.StreamWriter
                    file = My.Computer.FileSystem.OpenTextFileWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\rutadeaudo.txt", True)
                    file.WriteLine(Ruta & "," & Segundos)
                    file.Close()
                End If
            End If

            NotifyIcon1.Text = "Esperando por audios..."
            TextEdit16.Text = Ruta
            TextEdit3.Text = Segundos
            Timer1.Interval = Segundos * 1000
            Timer1.Enabled = True
            LabelControl1.Text = Timer1.Interval
            TextEdit1.Text = "Carpeta: " & audios_externos_carpeta
            TextEdit1.Visible = audios_externos
        End If
    End Sub

    Private Sub args_Showing(sender As Object, e As XtraMessageShowingArgs)
        e.Buttons(DialogResult.OK).Text = "&Aceptar"
        e.Buttons(DialogResult.OK).ImageOptions.Image = My.Resources.Resource1.close__2_

        e.Buttons(DialogResult.OK).ImageOptions.Location = ImageLocation.MiddleLeft
        e.Buttons(DialogResult.OK).Font = MiFuente
        e.Form.Appearance.Font = MiFuente
    End Sub

    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseDoubleClick
        Me.Visible = True
    End Sub

    Private Sub ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem1.Click
        Me.Visible = True
    End Sub

    Private Sub ToolStripMenuItem3_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem3.Click
        Me.Close()
    End Sub

    Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Me.Visible = False
        NotifyIcon1.Visible = True

    End Sub

    Private Sub SimpleButton8_Click(sender As Object, e As EventArgs) Handles SimpleButton8.Click
        Procesando = True
        XtraFolderBrowserDialog1.SelectedPath = TextEdit16.Text
        XtraFolderBrowserDialog1.ShowDialog()
        TextEdit16.Text = XtraFolderBrowserDialog1.SelectedPath
        TextEdit16.Focus()
        Procesando = False
    End Sub

    Private Sub SimpleButton1_Click(sender As Object, e As EventArgs) Handles SimpleButton1.Click
        If TextEdit16.Text.Trim.Length = 0 Then
            TextEdit16.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        End If
        If Not My.Computer.FileSystem.DirectoryExists(TextEdit16.Text) Then
            TextEdit16.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        End If
        If Val(TextEdit3.Text.Trim) = 0 Then
            TextEdit3.Text = 10
        End If
        Ruta = TextEdit16.Text
        Segundos = Val(TextEdit3.Text)

        If Ruta.Trim.Length = 0 Then
            Ruta = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)

        End If

        Try

            Dim objWriter As New System.IO.StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\rutadeaudo.txt", False)
            objWriter.WriteLine(Ruta & "," & Segundos)
            objWriter.Close()

        Catch ex As Exception

        End Try
        Timer1.Interval = Segundos * 1000
        Timer1.Enabled = False
        Timer1.Enabled = True
        LabelControl1.Text = Timer1.Interval
        Me.Visible = False
    End Sub

    Private Sub SimpleButton2_Click(sender As Object, e As EventArgs) Handles SimpleButton2.Click
        Me.Visible = False
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        NotifyIcon1.Text = "Reproduciendo..."
        If Cargando Then Exit Sub
        Cargando = True
        ' Listen for the LoadCompleted event.
        If audios_externos Then

            cadSQL = "SELECT id, area, falla, maquina, reproducir_audio_externo FROM " & rutaBD & ".reportes WHERE reproducir_audio_externo > 0 AND estatus = 0"
            mensajesDS = consultaSEL(cadSQL)
            generarMensaje = False

            If mensajesDS.Tables(0).Rows.Count > 0 Then
                Dim numeroUnico = 0
                Dim audioFile As String
                For Each elmensaje In mensajesDS.Tables(0).Rows
                    'Se busca el prefijo
                    Try
                        Dim vecesAudio = ValNull(elmensaje!reproducir_audio_externo, "N")
                        If vecesAudio = 0 Then vecesAudio = 1
                        For i = 1 To vecesAudio
                            audioFile = audios_externos_carpeta & "\p" & elmensaje!area & ".wav"
                            If Not My.Computer.FileSystem.FileExists(audioFile) Then
                                audioFile = audios_externos_carpeta & "\pgeneral.wav"
                            End If
                            If My.Computer.FileSystem.FileExists(audioFile) Then
                                Dim fs As FileStream = New FileStream(audioFile, FileMode.Open, FileAccess.Read)
                                Dim sp As System.Media.SoundPlayer = New System.Media.SoundPlayer(fs)
                                sp.PlaySync()
                                fs.Close()
                            End If
                            Demora(audios_externos_pausa)
                            If audios_externos_modo = 1 Then
                                'Maquina
                                audioFile = audios_externos_carpeta & "\m" & elmensaje!maquina & ".wav"
                                If Not My.Computer.FileSystem.FileExists(audioFile) Then
                                    audioFile = audios_externos_carpeta & "\mgeneral.wav"
                                End If
                                If My.Computer.FileSystem.FileExists(audioFile) Then
                                    Dim fs As FileStream = New FileStream(audioFile, FileMode.Open, FileAccess.Read)
                                    Dim sp As System.Media.SoundPlayer = New System.Media.SoundPlayer(fs)
                                    sp.PlaySync()
                                    fs.Close()
                                End If
                                'Area
                                Demora(audios_externos_pausa)
                                audioFile = audios_externos_carpeta & "\a" & elmensaje!area & ".wav"
                                If Not My.Computer.FileSystem.FileExists(audioFile) Then
                                    audioFile = audios_externos_carpeta & "\ageneral.wav"
                                End If
                                If My.Computer.FileSystem.FileExists(audioFile) Then
                                    Dim fs As FileStream = New FileStream(audioFile, FileMode.Open, FileAccess.Read)
                                    Dim sp As System.Media.SoundPlayer = New System.Media.SoundPlayer(fs)
                                    sp.PlaySync()
                                    fs.Close()
                                End If
                                'Falla
                                Demora(audios_externos_pausa)
                                audioFile = audios_externos_carpeta & "\f" & elmensaje!falla & ".wav"
                                If Not My.Computer.FileSystem.FileExists(audioFile) Then
                                    audioFile = audios_externos_carpeta & "\fgeneral.wav"
                                End If
                                If My.Computer.FileSystem.FileExists(audioFile) Then
                                    Dim fs As FileStream = New FileStream(audioFile, FileMode.Open, FileAccess.Read)
                                    Dim sp As System.Media.SoundPlayer = New System.Media.SoundPlayer(fs)
                                    sp.PlaySync()
                                    fs.Close()
                                End If

                            ElseIf audios_externos_modo = 2 Then
                                'Area
                                audioFile = audios_externos_carpeta & "\a" & elmensaje!area & ".wav"
                                If Not My.Computer.FileSystem.FileExists(audioFile) Then
                                    audioFile = audios_externos_carpeta & "\ageneral.wav"
                                End If
                                If My.Computer.FileSystem.FileExists(audioFile) Then
                                    Dim fs As FileStream = New FileStream(audioFile, FileMode.Open, FileAccess.Read)
                                    Dim sp As System.Media.SoundPlayer = New System.Media.SoundPlayer(fs)
                                    sp.PlaySync()
                                    fs.Close()
                                End If
                                'Maquina
                                Demora(audios_externos_pausa)
                                audioFile = audios_externos_carpeta & "\m" & elmensaje!maquina & ".wav"
                                If Not My.Computer.FileSystem.FileExists(audioFile) Then
                                    audioFile = audios_externos_carpeta & "\mgeneral.wav"
                                End If
                                If My.Computer.FileSystem.FileExists(audioFile) Then
                                    Dim fs As FileStream = New FileStream(audioFile, FileMode.Open, FileAccess.Read)
                                    Dim sp As System.Media.SoundPlayer = New System.Media.SoundPlayer(fs)
                                    sp.PlaySync()
                                    fs.Close()
                                End If
                                'Falla
                                Demora(audios_externos_pausa)
                                audioFile = audios_externos_carpeta & "\f" & elmensaje!falla & ".wav"
                                If Not My.Computer.FileSystem.FileExists(audioFile) Then
                                    audioFile = audios_externos_carpeta & "\fgeneral.wav"
                                End If
                                If My.Computer.FileSystem.FileExists(audioFile) Then
                                    Dim fs As FileStream = New FileStream(audioFile, FileMode.Open, FileAccess.Read)
                                    Dim sp As System.Media.SoundPlayer = New System.Media.SoundPlayer(fs)
                                    sp.PlaySync()
                                    fs.Close()
                                End If
                            End If
                        Next

                    Catch ex2 As Exception
                        Cargando = False
                    End Try

                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".reportes SET reproducir_audio_externo = 0 WHERE id = " & elmensaje!id)

                Next
            End If
        Else

            Dim LaRuta As String

            Try
                If My.Computer.FileSystem.DirectoryExists(TextEdit16.Text) Then
                    LaRuta = TextEdit16.Text
                Else
                    LaRuta = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                End If
            Catch ex As Exception
                Cargando = False
                Exit Sub
            End Try


            For Each foundFile As String In My.Computer.FileSystem.GetFiles(
  LaRuta, Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly, "*.wav")
                Try
                    Dim miArchivo = New IO.FileInfo(foundFile).Name
                    If Strings.Left(miArchivo, 9) = "audio_def" Then
                        Dim fs As FileStream = New FileStream(foundFile, FileMode.Open, FileAccess.Read)
                        Dim sp As System.Media.SoundPlayer = New System.Media.SoundPlayer(fs)
                        sp.PlaySync()
                        fs.Close()
                    End If
                    File.Delete(foundFile)
                    File.Delete(foundFile)

                Catch ex2 As Exception
                    Cargando = False

                End Try

                'Se mueven los archivos a otra carpeta
            Next

        End If

        NotifyIcon1.Text = "Esperando por audios..."
        Cargando = False
    End Sub

    Private Sub AxWindowsMediaPlayer1_Enter(sender As Object, e As EventArgs)

    End Sub

    Private Sub LinkLabel1_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked
        System.Diagnostics.Process.Start(LinkLabel1.Text)
    End Sub

    Sub Demora(Segundos As Integer)
        If Segundos = 0 Then Exit Sub
        Dim Salir = False
        Dim TInicial = DateTime.Now
        Dim TotalSegundos As TimeSpan
        Do While Not Salir
            Dim TiempoFinal = DateTime.Now
            TotalSegundos = TiempoFinal - TInicial
            If TotalSegundos.Seconds > Segundos Then
                Salir = True
            End If
        Loop
    End Sub

End Class
