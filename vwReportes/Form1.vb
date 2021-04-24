
Imports MySql.Data.MySqlClient
Imports System.IO.Ports
Imports System.IO
Imports System.Text
Imports System.Net.Mail
Imports System.Net
Imports System.ComponentModel
Imports System.Data
Imports System.Windows.Forms
Imports DevExpress.XtraCharts
Imports DevExpress.XtraGauges.Win
Imports DevExpress.XtraGauges.Win.Base
Imports DevExpress.XtraGauges.Win.Gauges.Circular
Imports DevExpress.XtraGauges.Core.Model
Imports DevExpress.XtraGauges.Core.Base
Imports DevExpress.XtraGauges.Core.Drawing
Imports System.Drawing
Imports System.Drawing.Imaging


Public Class Form1

    Dim Estado As Integer = 0
    Dim procesandoAudios As Boolean = False
    Dim eSegundos = 0
    Dim procesandoEscalamientos As Boolean
    Dim procesandoRepeticiones As Boolean
    Dim estadoPrograma As Boolean
    Dim MensajeLlamada = ""
    Dim errorCorreos As String = ""
    Dim cad_consolidado As String = ""
    Dim bajo_color As String
    Dim medio_color As String
    Dim alto_color As String
    Dim escaladas_color As String
    Dim noatendio_color As String
    Dim alto_etiqueta As String
    Dim escaladas_etiqueta As String
    Dim noatendio_etiqueta As String
    Dim bajo_hasta As Integer
    Dim medio_hasta As Integer

    Dim id01 As Boolean = False
    Dim id02 As Boolean = False
    Dim id03 As Boolean = False
    Dim id04 As Boolean = False
    Dim id05 As Boolean = False
    Dim id06 As Boolean = False
    Dim id07 As Boolean = False
    Dim id08 As Boolean = False
    Dim id09 As Boolean = False
    Dim id10 As Boolean = False
    Dim id11 As Boolean = False
    Dim id12 As Boolean = False
    Dim id13 As Boolean = False
    Public be_log_activar As Boolean = False
    Dim filtroFechas As String = ""
    Dim filtroReportes As String = ""
    Dim filtroFechasDia As String = ""
    Dim filtroMTBF_are As String = ""

    Dim filtroMTBF As String = ""
    Dim filtroOEE As String = ""
    Dim filtroOEEDias As String = ""
    Dim filtroRechazos As String = ""
    Dim filtroCorte As String = ""
    Dim filtroMTBF_tec As String = ""
    Dim filtroMTBF_fal As String = ""
    Dim filtroHXH As String = ""
    Dim filtroWIP As String = ""
    Dim filtroTrazabilidad As String
    Dim filtroCalidad As String = ""
    Dim filtroParos As String = ""
    Dim modulo2 As Boolean
    Dim moduloOEE As Boolean
    Dim fDesdeSF As String
    Dim fHastaSF As String
    Dim tituloPeriodos
    Dim agregados As New DataTable("periodos")
    Dim tablaGRafico As DataTable
    Dim secuencia As Long = 0
    Dim turno_actual As Long = 0
    Dim turno_serie As Long = 0
    Dim sDesde As String = ""
    Dim cadPeriodo As String = ""
    Dim eDesde = Now()
    Dim eHasta = Now()
    Dim miTurno As String = ""
    Dim turnoCambiado As Integer = -1
    Dim nuevoTitulo As String = ""
    Dim myBuilder As System.Text.StringBuilder = New System.Text.StringBuilder()


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim argumentos As String() = Environment.GetCommandLineArgs()
        'estadoPrograma = True
        'modulo2 = True
        'moduloOEE = true
        'Dim parametrosSecuencia2 = "0;0;1;0".Split(New Char() {";"c})
        'turno_serie = Val(parametrosSecuencia2(0))
        'secuencia = Val(parametrosSecuencia2(1))
        'turno_actual = Val(parametrosSecuencia2(2))
        'turnoCambiado = Val(parametrosSecuencia2(3))
        'cadenaConexion = "server=127.0.0.1;user id=root;password=usbw;port=3307;Convert Zero Datetime=True;Allow User Variables=True"
        'enviarReportes()


        If Process.GetProcessesByName _
          (Process.GetCurrentProcess.ProcessName).Length > 1 Then
        ElseIf argumentos.Length <= 1 Then
            MsgBox("String connection missing", MsgBoxStyle.Critical, "SIGMA")
        Else
            cadenaConexion = argumentos(1)
            Dim modulos = argumentos(2).Split(New Char() {";"c})
            modulo2 = Val(modulos(0)) = 1
            moduloOEE = Val(modulos(1)) = 1
            Dim parametrosSecuencia = argumentos(3).Split(New Char() {";"c})
            If parametrosSecuencia.Length = 4 Then
                turno_serie = Val(parametrosSecuencia(0))
                secuencia = Val(parametrosSecuencia(1))
                turno_actual = Val(parametrosSecuencia(2))
                turnoCambiado = Val(parametrosSecuencia(3))
                Dim idProceso = Process.GetCurrentProcess.Id
                idProceso = Process.GetCurrentProcess.Id
                estadoPrograma = True
                enviarReportes()
            End If
        End If
        Application.Exit()
    End Sub

    Private Sub enviarReportes()
        'Se envía correo


        Dim regsAfectados = 0
        'Escalada 4
        Dim miError As String = ""
        Dim correo_cuenta As String
        Dim correo_puerto As String
        Dim correo_ssl As Boolean
        Dim correo_clave As String
        Dim correo_host As String
        Dim rutaFiles As String
        Dim be_envio_reportes As Boolean = False

        Dim cadSQL As String = "SELECT * FROM " & rutaBD & ".configuracion"
        Dim readerDS As DataSet = consultaSEL(cadSQL)
        If readerDS.Tables(0).Rows.Count > 0 Then
            Dim reader As DataRow = readerDS.Tables(0).Rows(0)
            be_idioma = ValNull(reader!idioma_defecto, "N")
            etiquetas()
            correo_cuenta = ValNull(reader!correo_cuenta, "A")
            correo_clave = ValNull(reader!correo_clave, "A")
            correo_puerto = ValNull(reader!correo_puerto, "A")
            correo_ssl = ValNull(reader!correo_ssl, "A") = "S"
            be_envio_reportes = ValNull(reader!be_envio_reportes, "A") = "S"
            correo_host = ValNull(reader!correo_host, "A")
            rutaFiles = ValNull(reader!ruta_archivos_enviar, "A")
            alto_etiqueta = ValNull(reader!alto_etiqueta, "A")
            escaladas_etiqueta = ValNull(reader!escaladas_etiqueta, "A")
            noatendio_etiqueta = ValNull(reader!noatendio_etiqueta, "A")
            cad_consolidado = ValNull(reader!cad_consolidado, "A")
            alto_color = ValNull(reader!alto_color, "A")
            medio_color = ValNull(reader!medio_color, "A")
            bajo_color = ValNull(reader!bajo_color, "A")
            escaladas_color = ValNull(reader!escaladas_color, "A")
            noatendio_color = ValNull(reader!noatendio_color, "A")
            bajo_hasta = ValNull(reader!bajo_hasta, "N")
            medio_hasta = ValNull(reader!medio_hasta, "N")
            be_log_activar = ValNull(reader!be_log_activar, "A") = "S"

        End If
        If be_envio_reportes Then
            If bajo_hasta = 0 Then bajo_hasta = 50
            If medio_hasta = 0 Then medio_hasta = 75
            If alto_etiqueta.Length = 0 Then alto_etiqueta = traduccion(2)
            If escaladas_etiqueta.Length = 0 Then escaladas_etiqueta = traduccion(2)
            If noatendio_etiqueta.Length = 0 Then noatendio_etiqueta = traduccion(3)
            alto_color = "#" & alto_color
            escaladas_color = "#" & escaladas_color
            noatendio_color = "#" & noatendio_color
            If alto_color.Length = 0 Then alto_color = System.Drawing.Color.LimeGreen.ToString
            If escaladas_color.Length = 0 Then escaladas_color = System.Drawing.Color.OrangeRed.ToString
            If noatendio_color.Length = 0 Then noatendio_color = System.Drawing.Color.Tomato.ToString

            If rutaFiles.Length = 0 Then
                rutaFiles = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            Else
                rutaFiles = Strings.Replace(rutaFiles, "/", "\")
                If Not My.Computer.FileSystem.DirectoryExists(rutaFiles) Then
                    Try
                        My.Computer.FileSystem.CreateDirectory(rutaFiles)
                    Catch ex As Exception
                        rutaFiles = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    End Try
                End If
            End If
            If rutaFiles <> Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) Then
                For Each foundFile As String In My.Computer.FileSystem.GetFiles(
  rutaFiles, Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly, "*.png")
                    Try
                        File.Delete(foundFile)
                    Catch ex2 As Exception

                    End Try

                    'Se mueven los archivos a otra carpeta
                Next

                For Each foundFile As String In My.Computer.FileSystem.GetFiles(
  rutaFiles, Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly, "*.csv")
                    Try
                        File.Delete(foundFile)
                    Catch ex2 As Exception

                    End Try

                    'Se mueven los archivos a otra carpeta
                Next

            End If
            If Not estadoPrograma Then
                Exit Sub
            End If
            cadSQL = "SELECT a.id, a.nombre, a.titulo, a.cuerpo, a.consulta, a.para, a.copia, a.oculta, a.extraccion, c.solo FROM " & rutaBD & ".cat_correos a INNER JOIN " & rutaBD & ".det_correo b ON a.id = b.correo INNER JOIN " & rutaBD & ".int_listados c ON b.reporte = c.id  WHERE a.estatus = 'A' GROUP BY a.id, a.titulo, a.cuerpo, a.consulta, a.para, a.copia, a.oculta, a.extraccion, c.solo"

            Dim indice = 0

            Dim mensajesDS As DataSet = consultaSEL(cadSQL)
            Dim mensajeGenerado = False
            Dim tMensajes = 0
            If mensajesDS.Tables(0).Rows.Count > 0 Then

                Dim enlazado = False
                Dim errorCorreo = ""
                Dim smtpServer As New SmtpClient()
                Try
                    smtpServer.Credentials = New Net.NetworkCredential(correo_cuenta, correo_clave)
                    smtpServer.Port = correo_puerto
                    smtpServer.Host = correo_host '"smtp.live.com" '"smtp.gmail.com"
                    smtpServer.EnableSsl = correo_ssl
                    enlazado = True
                Catch ex As Exception
                    errorCorreo = ex.Message
                End Try
                If enlazado Then
                    For Each elmensaje In mensajesDS.Tables(0).Rows
                        nuevoTitulo = ""
                        id01 = False
                        id02 = False
                        id03 = False
                        id04 = False
                        id05 = False
                        id06 = False
                        id07 = False
                        id08 = False
                        id09 = False
                        id10 = False
                        id11 = False
                        id12 = False
                        id13 = False
                        Dim enviarSiempre As Boolean = False
                        Dim envio = elmensaje!extraccion.Split(New Char() {";"c})
                        'Se busca si hay uno del día y hra
                        If envio(2).Length > 0 And envio(3).Length > 0 Then
                            Dim enviarDia As Boolean = False
                            Dim diaSemana = DateAndTime.Weekday(Now)
                            Dim cadFrecuencia As String = traduccion(5)
                            'MsgBox(elmensaje!nombre & " envio(2) " & envio(2))
                            If turnoCambiado = 0 Then
                                If envio(2) = "T" Then
                                    enviarDia = True
                                ElseIf envio(2) = "LV" And diaSemana >= 2 And diaSemana <= 6 Then
                                    enviarDia = True
                                    cadFrecuencia = traduccion(6)
                                ElseIf envio(2) = "L" And diaSemana = 2 Then
                                    enviarDia = True
                                    cadFrecuencia = traduccion(7)
                                ElseIf envio(2) = "M" And diaSemana = 3 Then
                                    enviarDia = True
                                    cadFrecuencia = traduccion(8)
                                ElseIf envio(2) = "MI" And diaSemana = 4 Then
                                    enviarDia = True
                                    cadFrecuencia = traduccion(9)
                                ElseIf envio(2) = "J" And diaSemana = 5 Then
                                    enviarDia = True
                                    cadFrecuencia = traduccion(10)
                                ElseIf envio(2) = "V" And diaSemana = 6 Then
                                    enviarDia = True
                                    cadFrecuencia = traduccion(11)
                                ElseIf envio(2) = "S" And diaSemana = 7 Then
                                    enviarDia = True
                                    cadFrecuencia = traduccion(12)
                                ElseIf envio(2) = "D" And diaSemana = 1 Then
                                    enviarDia = True
                                    cadFrecuencia = traduccion(13)
                                ElseIf envio(2) = "1M" And Val(Today.Day) = 1 Then
                                    enviarDia = True
                                    cadFrecuencia = traduccion(14)
                                ElseIf envio(2) = "UM" And Val(Today.Day) = Date.DaysInMonth(Today.Year, Today.Month) Then
                                    enviarDia = True
                                    cadFrecuencia = traduccion(15)
                                End If
                            ElseIf turnoCambiado = 1 Then
                                If envio(2) = "0" Then
                                    enviarDia = True
                                    enviarSiempre = True
                                    cadFrecuencia = traduccion(152)
                                ElseIf envio(2) = "1" And secuencia = 1 Then
                                    enviarDia = True
                                    enviarSiempre = True
                                    cadFrecuencia = traduccion(153)
                                ElseIf envio(2) = "2" And secuencia = 2 Then
                                    enviarDia = True
                                    enviarSiempre = True
                                    cadFrecuencia = traduccion(154)
                                ElseIf envio(2) = "3" And secuencia = 3 Then
                                    enviarDia = True
                                    enviarSiempre = True
                                    cadFrecuencia = traduccion(155)
                                End If
                            End If

                            If enviarDia Then
                                    'MsgBox("entro en enviarDia " & elmensaje!nombre & " envio(3) " & envio(3))
                                    Dim enviar As Boolean = False
                                    Dim hora = Val(Format(Now, "HH"))
                                    If enviarSiempre Then
                                        enviar = True
                                    ElseIf envio(3) = "SSSSSSSSSSSSSSSSSSSSSSSS" Then
                                        enviar = True
                                        cadFrecuencia = cadFrecuencia & traduccion(16)
                                    ElseIf envio(3).length > 0 Then
                                        If envio(3)(Val(hora)) = "S" Then
                                            cadFrecuencia = cadFrecuencia & IIf(Val(hora) = 1, traduccion(18), traduccion(19) & Val(hora) & traduccion(17))
                                            enviar = True
                                        End If

                                    End If
                                    If enviar Then
                                        'MsgBox("entro en enviar " & elmensaje!nombre)
                                        Dim mail As New MailMessage
                                        Try
                                            Dim cuerpo As String = ValNull(elmensaje!cuerpo, "A")
                                            Dim titulo As String = ValNull(elmensaje!titulo, "A")
                                            If titulo.Length = 0 Then titulo = traduccion(20)
                                            If cuerpo.Length = 0 Then cuerpo = traduccion(21)

                                            If elmensaje!solo >= 400 And elmensaje!solo < 500 Then

                                                If myBuilder.Length > 0 Then
                                                    myBuilder.Clear()
                                                End If
                                                myBuilder.Append("<html xmlns='http://www.w3.org/1999/xhtml'>")
                                                myBuilder.Append("<head>")
                                                myBuilder.Append("</head>")
                                                myBuilder.Append("<body>")
                                                myBuilder.Append("<br>")
                                                myBuilder.Append(cuerpo & "<br>")
                                                myBuilder.Append(cadFrecuencia)
                                                myBuilder.Append("<br>")
                                            End If
                                            mail.From = New MailAddress(correo_cuenta)
                                            Dim mails As String = ValNull(elmensaje!para, "A")
                                            Dim mailsV As String() = mails.Split(New Char() {";"c})
                                            For Each cuenta In mailsV
                                                If cuenta.Length > 0 Then
                                                    cuenta = Strings.Replace(cuenta, vbCrLf, "")
                                                    cuenta = Strings.Replace(cuenta, vbLf, "")
                                                    mail.To.Add(cuenta)
                                                End If
                                            Next
                                            mails = ValNull(elmensaje!copia, "A")
                                            mailsV = mails.Split(New Char() {";"c})
                                            For Each cuenta In mailsV
                                                If cuenta.Length > 0 Then
                                                    cuenta = Strings.Replace(cuenta, vbCrLf, "")
                                                    cuenta = Strings.Replace(cuenta, vbLf, "")
                                                    mail.CC.Add(cuenta)
                                                End If
                                            Next
                                            mails = ValNull(elmensaje!oculta, "A")
                                            mailsV = mails.Split(New Char() {";"c})
                                            For Each cuenta In mailsV
                                                If cuenta.Length > 0 Then
                                                    cuenta = Strings.Replace(cuenta, vbCrLf, "")
                                                    cuenta = Strings.Replace(cuenta, vbLf, "")
                                                    mail.Bcc.Add(cuenta)
                                                End If
                                            Next
                                            mail.Subject = titulo
                                            errorCorreos = ""
                                            cuerpo = cuerpo & vbCrLf & traduccion(22)
                                            calcularPeriodos(envio(0), envio(1))
                                            cadSQL = "SELECT a.reporte, b.modulo, b.nombre, b.grafica, b.grafica_asociada, b.file_name, b.grafica FROM " & rutaBD & ".det_correo a INNER JOIN " & rutaBD & ".int_listados b ON a.reporte = b.id AND idioma = " & be_idioma & " WHERE a.correo = " & elmensaje!id & " AND b.solo = " & elmensaje!solo & " ORDER BY b.orden"
                                            mensajesDS = consultaSEL(cadSQL)
                                            If mensajesDS.Tables(0).Rows.Count > 0 Then
                                                Dim mireporte = 0
                                                For Each reporte In mensajesDS.Tables(0).Rows


                                                    If reporte!modulo < 3 Then
                                                        mireporte = generarReporteTipo1y2(reporte!reporte, reporte!nombre, reporte!file_name, rutaFiles, reporte!grafica, elmensaje!consulta, reporte!modulo, reporte!grafica_asociada)
                                                    ElseIf reporte!modulo = 3 Then
                                                        mireporte = generarReporteTipo3(reporte!reporte, reporte!nombre, reporte!file_name, rutaFiles, reporte!grafica, elmensaje!consulta, reporte!modulo, reporte!grafica_asociada)
                                                    ElseIf reporte!modulo = 4 Then
                                                        mireporte = generarReporteTipo4(reporte!reporte, reporte!nombre, reporte!file_name, rutaFiles, reporte!grafica, elmensaje!consulta, reporte!modulo, reporte!grafica_asociada)
                                                    ElseIf reporte!modulo = 5 Then
                                                        mireporte = generarReporteTipo5(reporte!reporte, reporte!nombre, reporte!file_name, rutaFiles, reporte!grafica, elmensaje!consulta, reporte!modulo, reporte!grafica_asociada)
                                                    ElseIf reporte!modulo = 6 Then
                                                        mireporte = generarReporteTipo6(reporte!reporte, reporte!nombre, reporte!file_name, rutaFiles, reporte!grafica, elmensaje!consulta, reporte!modulo, reporte!grafica_asociada)
                                                    ElseIf reporte!modulo = 7 Then

                                                        mireporte = generarReporteTipo7(reporte!reporte, reporte!nombre, reporte!file_name, rutaFiles, reporte!grafica, elmensaje!consulta, reporte!modulo, reporte!grafica_asociada)
                                                    End If
                                                    If mireporte = -1 Then
                                                        cuerpo = cuerpo & vbCrLf & reporte!nombre & traduccion(23) & errorCorreos
                                                    Else

                                                        If My.Computer.FileSystem.FileExists(rutaFiles & "\" & reporte!file_name & ".csv") Then
                                                            If elmensaje!solo >= 400 And elmensaje!solo < 500 Then
                                                                myBuilder.Append(reporte!nombre)
                                                            Else
                                                                cuerpo = cuerpo & vbCrLf & reporte!nombre
                                                            End If

                                                            Dim archivo As Attachment = New Attachment(rutaFiles & "\" & reporte!file_name & ".csv")
                                                            mail.Attachments.Add(archivo)
                                                        ElseIf mireporte = 2 Then
                                                            If elmensaje!solo >= 400 And elmensaje!solo < 500 Then
                                                                myBuilder.Append(reporte!nombre & ", " & traduccion(120))
                                                            Else
                                                                cuerpo = cuerpo & vbCrLf & reporte!nombre & ", " & traduccion(120)
                                                            End If

                                                        Else
                                                            If elmensaje!solo >= 400 And elmensaje!solo < 500 Then
                                                                myBuilder.Append(reporte!nombre & " " & traduccion(24))
                                                            Else
                                                                cuerpo = cuerpo & vbCrLf & reporte!nombre & " " & traduccion(24)
                                                            End If

                                                        End If
                                                        If My.Computer.FileSystem.FileExists(rutaFiles & "\" & reporte!file_name & ".png") Then

                                                            Dim archivo As Attachment = New Attachment(rutaFiles & "\" & reporte!file_name & ".png")
                                                            mail.Attachments.Add(archivo)
                                                        End If
                                                    End If
                                                Next
                                            End If

                                            If elmensaje!solo >= 400 And elmensaje!solo < 500 Then
                                                myBuilder.Append("<br>")
                                                myBuilder.Append("</body>")
                                                myBuilder.Append("</html>")
                                                mail.Subject = nuevoTitulo
                                                mail.IsBodyHtml = True
                                                mail.Body = myBuilder.ToString()
                                            Else
                                                cuerpo = cadFrecuencia & vbCrLf & vbCrLf & cuerpo
                                                mail.Body = cuerpo
                                                mail.IsBodyHtml = False
                                            End If

                                            smtpServer.Send(mail)
                                            tMensajes = tMensajes + 1
                                            mensajeGenerado = True
                                        Catch ex As Exception
                                            agregarLOG(traduccion(25) & ex.Message, 0, 9)
                                        End Try
                                    Else
                                        mensajeGenerado = True
                                    End If
                                End If
                            End If
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".cat_correos Set ultimo_envio = '" & Format(Now, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & elmensaje!id)
                    Next
                End If
                If enlazado Then
                    If tMensajes > 0 Then
                        agregarLOG(traduccion(26).Replace("campo_0", tMensajes))
                    Else
                        agregarLOG(traduccion(27))
                    End If

                Else
                    agregarLOG(traduccion(25) & errorCorreo, 0, 9)
                End If
                smtpServer.Dispose()
            End If
            If tMensajes > 0 Then regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".control (fecha, mensajes, tipo) VALUES ('" & Format(Now, "yyyyMMddHH") & "', " & tMensajes & ", 5)")
        End If
    End Sub

    Function generarReporteTipo3(idReporte As Integer, reporte As String, fName As String, ruta As String, graficar As String, consulta As Long, modulo As Integer, grafica As Long) As Integer
        generarReporteTipo3 = 0

        Dim archivoSaliente = ruta & "\" & fName & ".csv"
        Dim archivoImagen = ruta & "\" & fName & ".png"

        Try
            My.Computer.FileSystem.DeleteFile(archivoSaliente)


        Catch ex As Exception

        End Try

        Try
            My.Computer.FileSystem.DeleteFile(archivoImagen)


        Catch ex As Exception

        End Try

        archivoSaliente = archivoSaliente.Replace("\", "\\")


        Dim comillas = Microsoft.VisualBasic.Strings.Left(Chr(34), 1)

        Dim inicial = ""
        Dim cabecera = ""
        Dim registros = ""

        Dim Leer As Boolean = False
        Dim cadSQL = ""

        cadSQL = "SELECT * FROM " & rutaBD & ".pu_graficos WHERE (usuario = 1 OR usuario = 0) AND grafico = " & grafica & " AND idioma = " & IIf(be_idioma = 0, 1, be_idioma) & " ORDER BY usuario DESC LIMIT 1"

        Dim config As DataSet = consultaSEL(cadSQL)
        If config.Tables(0).Rows.Count > 0 Then
            Dim ordenPareto = ValNull(config.Tables(0).Rows(0)!orden, "N")

            If consulta > 0 Then generarFIltro(consulta)

            Dim mensajesDS As DataSet
            Dim sentencia As String = ""
            Dim adicional = ""
            Dim regsAfectados = 0
            Dim tHaving = ""
            Dim ordenDatos = " 8 "
            Dim campoSumar = "tiempo"

            inicial = Chr(34) & reporte & " (" & cadPeriodo & ")" & Chr(34) & vbCrLf
            inicial = inicial & Chr(34) & traduccion(110) & ": " & Format(Now(), "ddd, dd-MMM-yyyy HH:mm:ss") & Chr(34) & vbCrLf
            If secuencia <> 0 Then
                inicial = inicial & Chr(34) & traduccion(69) & ": " & miTurno & Chr(34) & vbCrLf
            Else
                inicial = inicial & Chr(34) & traduccion(111) & ": " & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & " " & traduccion(112) & ": " & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & Chr(34) & vbCrLf

            End If



            If ordenPareto = 1 Then

                ordenDatos = "7"
                campoSumar = "docs"

            ElseIf ordenPareto = 2 Then

                ordenDatos = "9"
                campoSumar = "loca"

            ElseIf ordenPareto = 3 Then

                ordenDatos = "10"
                campoSumar = "repa"

            End If
            If config.Tables(0).Rows(0)!orden_grafica = "M" Then

                ordenDatos = ordenDatos & " DESC"

            ElseIf config.Tables(0).Rows(0)!orden_grafica = "A" Then

                ordenDatos = " 6 "
            End If

            Dim miReporte = idReporte - 26
            Dim filtroTiempo = " AND c.fecha_reporte >= '" & fDesdeSF & "' AND c.fecha_reporte <= '" & fHastaSF & "' "
            If sDesde <> "-1" Then
                filtroTiempo = " AND c.secuencia " & sDesde & " "
            End If

            sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, d.nombre, c.linea AS id, COUNT(*) AS docs, SUM(c.tiemporeparacion + c.tiempollegada) / 3600 AS tiempo, SUM(c.tiempollegada) / 3600 AS loca, SUM(c.tiemporeparacion) / 3600 AS repa FROM " & rutaBD & ".reportes c INNER JOIN " & rutaBD & ".cat_lineas d ON c.linea = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroTiempo & filtroReportes & " GROUP BY c.linea, d.nombre " & tHaving & " ORDER BY " & ordenDatos

            cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(80) & Chr(34) & "," & Chr(34) & traduccion(42) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(117) & Chr(34) & "," & Chr(34) & traduccion(118) & Chr(34) & "," & Chr(34) & traduccion(119) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                If miReporte = 2 Then

                    sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, d.nombre, c.maquina AS id, COUNT(*) AS docs, SUM(c.tiemporeparacion + c.tiempollegada) / 3600 AS tiempo, SUM(c.tiempollegada) / 3600 AS loca, SUM(c.tiemporeparacion) / 3600 AS repa FROM " & rutaBD & ".reportes c INNER JOIN " & rutaBD & ".cat_maquinas d ON c.maquina = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroTiempo & filtroReportes & " GROUP BY c.maquina, d.nombre " & tHaving & " ORDER BY " & ordenDatos

                    cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(82) & Chr(34) & "," & Chr(34) & traduccion(43) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(117) & Chr(34) & "," & Chr(34) & traduccion(118) & Chr(34) & "," & Chr(34) & traduccion(119) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                ElseIf miReporte = 3 Then

                    sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, d.nombre, c.area AS id, COUNT(*) AS docs, SUM(c.tiemporeparacion + c.tiempollegada) / 3600 AS tiempo, SUM(c.tiempollegada) / 3600 AS loca, SUM(c.tiemporeparacion) / 3600 AS repa FROM " & rutaBD & ".reportes c INNER JOIN " & rutaBD & ".cat_areas d ON c.area = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroTiempo & filtroReportes & " GROUP BY c.area, d.nombre " & tHaving & " ORDER BY " & ordenDatos

                    cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(84) & Chr(34) & "," & Chr(34) & traduccion(44) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(117) & Chr(34) & "," & Chr(34) & traduccion(118) & Chr(34) & "," & Chr(34) & traduccion(119) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                ElseIf miReporte = 4 Then

                    sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, d.nombre, c.falla_ajustada AS id, COUNT(*) AS docs, SUM(c.tiemporeparacion + c.tiempollegada) / 3600 AS tiempo, SUM(c.tiempollegada) / 3600 AS loca, SUM(c.tiemporeparacion) / 3600 AS repa FROM " & rutaBD & ".reportes c INNER JOIN " & rutaBD & ".cat_fallas d ON c.falla_ajustada = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroTiempo & filtroReportes & " GROUP BY c.falla_ajustada, d.nombre " & tHaving & " ORDER BY " & ordenDatos

                    cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(86) & Chr(34) & "," & Chr(34) & traduccion(85) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(117) & Chr(34) & "," & Chr(34) & traduccion(118) & Chr(34) & "," & Chr(34) & traduccion(119) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                ElseIf miReporte = 5 Then

                    sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, IFNULL(b.nombre, '" & traduccion(100) & "') AS nombre, d.tipo AS id, COUNT(*) AS docs, SUM(c.tiemporeparacion + c.tiempollegada) / 3600 AS tiempo, SUM(c.tiempollegada) / 3600 AS loca, SUM(c.tiemporeparacion) / 3600 AS repa FROM " & rutaBD & ".reportes c INNER JOIN " & rutaBD & ".cat_maquinas d ON c.maquina = d.id LEFT JOIN " & rutaBD & ".cat_generales b ON d.tipo = b.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroTiempo & filtroReportes & " GROUP BY id, nombre " & tHaving & " ORDER BY " & ordenDatos

                    cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(151) & Chr(34) & "," & Chr(34) & traduccion(46) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(117) & Chr(34) & "," & Chr(34) & traduccion(118) & Chr(34) & "," & Chr(34) & traduccion(119) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                ElseIf miReporte = 6 Then

                    sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, IFNULL(b.nombre, '" & traduccion(100) & "') AS nombre, d.agrupador_1 AS id, COUNT(*) AS docs, SUM(c.tiemporeparacion + c.tiempollegada) / 3600 AS tiempo, SUM(c.tiempollegada) / 3600 AS loca, SUM(c.tiemporeparacion) / 3600 AS repa FROM " & rutaBD & ".reportes c INNER JOIN " & rutaBD & ".cat_maquinas d ON c.maquina = d.id LEFT JOIN " & rutaBD & ".cat_generales b ON d.agrupador_1 = b.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroTiempo & filtroReportes & " GROUP BY id, nombre " & tHaving & " ORDER BY " & ordenDatos

                    cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(151) & Chr(34) & "," & Chr(34) & traduccion(47) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(117) & Chr(34) & "," & Chr(34) & traduccion(118) & Chr(34) & "," & Chr(34) & traduccion(119) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                ElseIf miReporte = 7 Then

                    sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, IFNULL(b.nombre, '" & traduccion(100) & "') AS nombre, d.agrupador_2 AS id, COUNT(*) AS docs, SUM(c.tiemporeparacion + c.tiempollegada) / 3600 AS tiempo, SUM(c.tiempollegada) / 3600 AS loca, SUM(c.tiemporeparacion) / 3600 AS repa FROM " & rutaBD & ".reportes c INNER JOIN " & rutaBD & ".cat_maquinas d ON c.maquina = d.id LEFT JOIN " & rutaBD & ".cat_generales b ON d.agrupador_2 = b.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroTiempo & filtroReportes & " GROUP BY id, nombre " & tHaving & " ORDER BY " & ordenDatos
                    cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(151) & Chr(34) & "," & Chr(34) & traduccion(48) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(117) & Chr(34) & "," & Chr(34) & traduccion(118) & Chr(34) & "," & Chr(34) & traduccion(119) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                ElseIf miReporte = 8 Then

                    sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, IFNULL(b.nombre, '" & traduccion(100) & "') AS nombre, d.agrupador_1 AS id, COUNT(*) AS docs, SUM(c.tiemporeparacion + c.tiempollegada) / 3600 AS tiempo, SUM(c.tiempollegada) / 3600 AS loca, SUM(c.tiemporeparacion) / 3600 AS repa FROM " & rutaBD & ".reportes c INNER JOIN " & rutaBD & ".cat_fallas d ON c.falla_ajustada = d.id LEFT JOIN " & rutaBD & ".cat_generales b ON d.agrupador_1 = b.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroTiempo & filtroReportes & " GROUP BY id, nombre " & tHaving & " ORDER BY " & ordenDatos
                    cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(151) & Chr(34) & "," & Chr(34) & traduccion(49) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(117) & Chr(34) & "," & Chr(34) & traduccion(118) & Chr(34) & "," & Chr(34) & traduccion(119) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                ElseIf miReporte = 9 Then

                    sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, IFNULL(b.nombre, '" & traduccion(100) & "') AS nombre, d.agrupador_2 AS id, COUNT(*) AS docs, SUM(c.tiemporeparacion + c.tiempollegada) / 3600 AS tiempo, SUM(c.tiempollegada) / 3600 AS loca, SUM(c.tiemporeparacion) / 3600 AS repa FROM " & rutaBD & ".reportes c INNER JOIN " & rutaBD & ".cat_fallas d ON c.falla_ajustada = d.id LEFT JOIN " & rutaBD & ".cat_generales b ON d.agrupador_2 = b.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroTiempo & filtroReportes & " GROUP BY id, nombre " & tHaving & " ORDER BY " & ordenDatos

                    cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(151) & Chr(34) & "," & Chr(34) & traduccion(50) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(117) & Chr(34) & "," & Chr(34) & traduccion(118) & Chr(34) & "," & Chr(34) & traduccion(119) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)
                ElseIf miReporte = 10 Then

                    sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, DATE_FORMAT(c.fecha_reporte, '%Y/%m/%d') AS nombre, 0 AS id, COUNT(*) AS docs, SUM(c.tiemporeparacion + c.tiempollegada) / 3600 AS tiempo, SUM(c.tiempollegada) / 3600 AS loca, SUM(c.tiemporeparacion) / 3600 AS repa FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroTiempo & filtroReportes & " GROUP BY 5 " & tHaving & " ORDER BY " & ordenDatos

                    cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(51) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(117) & Chr(34) & "," & Chr(34) & traduccion(118) & Chr(34) & "," & Chr(34) & traduccion(119) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                ElseIf miReporte = 11 Then

                    sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, CONCAT(YEAR(c.fecha_reporte), '/', WEEK(c.fecha_reporte)) AS nombre, STR_TO_DATE(CONCAT(DATE_FORMAT(c.fecha_reporte,'%x/%v'), ' Monday'), '%x/%v %W') AS inicio, 0 AS id, COUNT(*) AS docs, SUM(c.tiemporeparacion + c.tiempollegada) / 3600 AS tiempo, SUM(c.tiempollegada) / 3600 AS loca, SUM(c.tiemporeparacion) / 3600 AS repa FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroTiempo & filtroReportes & " GROUP BY 5 " & tHaving & " ORDER BY " & ordenDatos

                    cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(52) & Chr(34) & "," & Chr(34) & traduccion(53) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(117) & Chr(34) & "," & Chr(34) & traduccion(118) & Chr(34) & "," & Chr(34) & traduccion(119) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                ElseIf miReporte = 12 Then

                    sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, CONCAT(YEAR(c.fecha_reporte), '/', MONTH(c.fecha_reporte)) AS nombre, 0 AS id, COUNT(*) AS docs, SUM(c.tiemporeparacion + c.tiempollegada) / 3600 AS tiempo, SUM(c.tiempollegada) / 3600 AS loca, SUM(c.tiemporeparacion) / 3600 AS repa FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroTiempo & filtroReportes & " GROUP BY 5 " & tHaving & " ORDER BY " & ordenDatos

                    cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(54) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(117) & Chr(34) & "," & Chr(34) & traduccion(118) & Chr(34) & "," & Chr(34) & traduccion(119) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                ElseIf miReporte = 13 Then

                    sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, d.nombre, c.tecnicoatend AS id, COUNT(*) AS docs, SUM(c.tiemporeparacion + c.tiempollegada) / 3600 AS tiempo, SUM(c.tiempollegada) / 3600 AS loca, SUM(c.tiemporeparacion) / 3600 AS repa FROM " & rutaBD & ".reportes c INNER JOIN " & rutaBD & ".cat_usuarios d ON c.tecnicoatend = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroTiempo & filtroReportes & " GROUP BY c.tecnicoatend, d.nombre " & tHaving & " ORDER BY " & ordenDatos

                    cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(151) & Chr(34) & "," & Chr(34) & traduccion(55) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(117) & Chr(34) & "," & Chr(34) & traduccion(118) & Chr(34) & "," & Chr(34) & traduccion(119) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                End If
                mensajesDS = consultaSEL(sentencia)
                adicional = ""
                If mensajesDS.Tables(0).Rows.Count > 0 Then
                    Dim view_o As DataView = New DataView(mensajesDS.Tables(0))
                    Dim objWriter As New System.IO.StreamWriter(archivoSaliente, False, System.Text.Encoding.UTF8)
                    objWriter.WriteLine(inicial)
                    objWriter.WriteLine(cabecera)
                    Dim totalPareto As Double = 0


                    If config.Tables(0).Rows(0)!maximo_barras > 0 And config.Tables(0).Rows(0)!maximo_barras < mensajesDS.Tables(0).Rows.Count Or config.Tables(0).Rows(0)!maximo_barraspct > 0 And config.Tables(0).Rows(0)!maximo_barraspct < 100 Then
                        'Se calcula el total del Pareto
                        Dim limitar = 0
                        Dim agrupado = ""
                        Dim pcAcum = 0
                        If config.Tables(0).Rows(0)!maximo_barraspct > 0 And config.Tables(0).Rows(0)!maximo_barraspct < 100 Then

                            For Each elmensaje As DataRow In mensajesDS.Tables(0).Rows
                                totalPareto = totalPareto + elmensaje.Item(campoSumar)
                            Next
                            Dim pct = config.Tables(0).Rows(0)!maximo_barraspct / 100
                            Dim i = 0
                            For Each elmensaje In mensajesDS.Tables(0).Rows
                                i = i + 1
                                pcAcum = pcAcum + elmensaje.Item(campoSumar)
                                If pcAcum / totalPareto >= pct Then
                                    limitar = i
                                    Exit For
                                End If
                            Next
                        End If
                        If config.Tables(0).Rows(0)!maximo_barras > 0 Then
                            If limitar > config.Tables(0).Rows(0)!maximo_barras Or limitar = 0 Then
                                limitar = config.Tables(0).Rows(0)!maximo_barras
                            End If
                        End If

                        If limitar + 1 >= mensajesDS.Tables(0).Rows.Count And config.Tables(0).Rows(0)!agrupar = "S" Then
                            limitar = 0
                        ElseIf limitar >= mensajesDS.Tables(0).Rows.Count Then
                            limitar = 0
                        End If
                        If limitar > 0 Then
                            For j = 0 To limitar - 1
                                mensajesDS.Tables(0).Rows(j)!orden = j + 1
                            Next
                            If config.Tables(0).Rows(0)!agrupar = "S" Then
                                'mttr y mtbf
                                Dim faltante1 = 0
                                Dim faltante2 = 0
                                Dim faltante3 = 0
                                Dim faltante4 = 0
                                Dim faltante5 = 0
                                Dim totalAgr = 0
                                For j = limitar To mensajesDS.Tables(0).Rows.Count - 1
                                    mensajesDS.Tables(0).Rows(j)!filtro = 0
                                    faltante1 = faltante1 + mensajesDS.Tables(0).Rows(j)!repa
                                    faltante2 = faltante2 + mensajesDS.Tables(0).Rows(j)!loca
                                    faltante3 = faltante3 + mensajesDS.Tables(0).Rows(j)!docs
                                    faltante4 = faltante4 + mensajesDS.Tables(0).Rows(j)!tiempo
                                Next
                                totalAgr = mensajesDS.Tables(0).Rows.Count - limitar
                                Dim row As DataRow = mensajesDS.Tables(0).NewRow

                                row("id") = "0"
                                row("nombre") = IIf(ValNull(config.Tables(0).Rows(0)!agrupar_texto, "A") = "", traduccion(61), config.Tables(0).Rows(0)!agrupar_texto) & " (" & totalAgr & ")"
                                row("repa") = faltante1
                                row("loca") = faltante2
                                row("docs") = faltante3
                                row("tiempo") = faltante4
                                row("porcentaje") = 0
                                row("filtro") = 1
                                row("pareto") = 1
                                If config.Tables(0).Rows(0)!agrupar_posicion = "F" Then
                                    row("orden") = limitar + 1
                                ElseIf config.Tables(0).Rows(0)!agrupar_posicion = "P" Then
                                    row("orden") = 0
                                End If
                                mensajesDS.Tables(0).Rows.Add(row)

                            Else
                                adicional = traduccion(147)
                            End If
                        End If
                        If config.Tables(0).Rows(0)!agrupar_posicion = "N" Then
                            view_o.Sort = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " " & campoSumar & " DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " " & campoSumar, "nombre"))
                        Else
                            view_o.Sort = "orden ASC"
                        End If
                    Else
                        view_o.Sort = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " " & campoSumar & " DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " " & campoSumar, "nombre"))
                    End If
                    view_o.RowFilter = "filtro = 1"
                    totalPareto = 0
                    For Each registro In mensajesDS.Tables(0).Rows
                        If registro!filtro = 1 Then
                            totalPareto = totalPareto + registro.Item(campoSumar)
                        End If
                    Next
                    Dim acumPareto As Double = 0
                    Dim linea = 0

                    tablaGRafico = view_o.ToTable()
                    For Each registro In tablaGRafico.Rows
                        If registro!filtro = 1 Then
                            linea = linea + 1
                            acumPareto = acumPareto + registro.Item(campoSumar) / totalPareto * 100
                            registro!porcentaje = acumPareto
                            If linea = mensajesDS.Tables(0).Rows.Count Then
                                acumPareto = 100
                            End If
                            Dim cadID = ""
                            If miReporte <> 10 And miReporte <> 11 And miReporte <> 12 Then
                                cadID = Chr(34) & registro!id & Chr(34) & ","
                            End If
                            Dim cadInicio = ""
                            If miReporte = 11 Then
                                cadInicio = Chr(34) & registro!inicio & Chr(34) & ","
                            End If
                            objWriter.WriteLine(Chr(34) & linea & Chr(34) & "," & cadID & Chr(34) & registro!nombre & Chr(34) & "," & cadInicio & Chr(34) & registro!docs & Chr(34) & "," & Chr(34) & registro!tiempo & Chr(34) & "," & Chr(34) & registro!loca & Chr(34) & "," & Chr(34) & registro!repa & Chr(34) & "," & Chr(34) & IIf(registro!pareto = 1, registro.Item(campoSumar) / totalPareto * 100, "") & Chr(34) & "," & Chr(34) & IIf(registro!pareto = 1, acumPareto, "") & Chr(34))
                        End If
                    Next
                    objWriter.WriteLine(traduccion(121) & ": " & linea)
                    If adicional.Length > 0 Then objWriter.WriteLine(adicional)
                    objWriter.WriteLine(traduccion(122))
                    objWriter.Close()

                    If graficar = "S" Then


                        Dim indicador01 = ""
                        Dim indicador02 = ""

                        Dim titulosSeries = config.Tables(0).Rows(0)!textos_adicionales.Split(New Char() {";"c})
                        If titulosSeries.length = 1 Then
                            indicador01 = titulosSeries(0)
                        End If
                        If titulosSeries.length = 2 Then
                            indicador01 = titulosSeries(0)
                            indicador02 = titulosSeries(1)
                        End If

                        If indicador01.Length = 0 Then indicador01 = IIf(modulo = 1, traduccion(64), traduccion(65))
                        If indicador02.Length = 0 Then indicador02 = traduccion(107)

                        ChartControl1.Series.Clear()
                        ChartControl1.Titles.Clear()
                        Dim Titulo As New ChartTitle()
                        Titulo.Text = config.Tables(0).Rows(0)!titulo & Strings.Space(10)
                        Dim miFuente = New Drawing.Font("Lucida Sans", 10, FontStyle.Regular)
                        Dim miFuenteAlto = New Drawing.Font("Lucida Sans", 16, FontStyle.Bold)
                        Dim miFuenteEjes = New Drawing.Font("Lucida Sans", 11, FontStyle.Regular)

                        Titulo.Font = miFuenteAlto
                        Dim series1 As New Series(indicador01, ViewType.Bar)

                        ChartControl1.Series.Add(series1)
                        series1.DataSource = tablaGRafico

                        series1.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                        series1.View.Color = Color.SkyBlue
                        series1.ArgumentScaleType = ScaleType.Qualitative
                        series1.ArgumentDataMember = "nombre"
                        series1.ValueScaleType = ScaleType.Numerical
                        series1.ValueDataMembers.AddRange(New String() {campoSumar})
                        series1.Label.BackColor = Color.DarkBlue
                        series1.Label.TextColor = Color.White
                        series1.Label.Font = miFuente
                        series1.Label.TextPattern = "{V:F" & config.Tables(0).Rows(0)!etiqueta_formato & "}"

                        'Obtener el titulo en Y
                        Dim tituloY As String = ""
                        Dim titulosY = ValNull(config.Tables(0).Rows(0)!texto_y, "A")
                        Try

                            Dim titulosYArreglo = titulosY.Split(New Char() {";"c})
                            If titulosYArreglo.length = 1 Then
                                tituloY = titulosYArreglo(0)
                            Else
                                tituloY = titulosYArreglo(ordenPareto)
                            End If
                        Catch ex As Exception
                            tituloY = titulosY
                        End Try
                        CType(ChartControl1.Diagram, XYDiagram).AxisY.Visibility = DevExpress.Utils.DefaultBoolean.True
                        CType(ChartControl1.Diagram, XYDiagram).AxisY.Label.Font = miFuenteEjes
                        CType(ChartControl1.Diagram, XYDiagram).AxisY.GridSpacingAuto = False
                        CType(ChartControl1.Diagram, XYDiagram).AxisY.GridSpacing = 1
                        CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Text = tituloY
                        CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Font = miFuenteAlto
                        CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True
                        CType(ChartControl1.Diagram, XYDiagram).AxisY.GridLines.Visible = False
                        CType(ChartControl1.Diagram, XYDiagram).AxisY.Tickmarks.Visible = False
                        CType(ChartControl1.Diagram, XYDiagram).AxisY.Tickmarks.MinorVisible = False

                        If config.Tables(0).Rows(0)!grueso_spiline > 0 Then
                            Dim view_2 As DataView = New DataView(tablaGRafico)
                            view_2.RowFilter = "pareto = 1"
                            Dim tablaPareto As DataTable = view_2.ToTable()
                            Dim series2 As New Series(indicador02, ViewType.Spline)

                            ChartControl1.Series.Add(series2)
                            series2.DataSource = tablaPareto
                            series2.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                            series2.View.Color = Color.Green

                            series2.ArgumentScaleType = ScaleType.Qualitative
                            series2.ArgumentDataMember = "nombre"
                            series2.ValueScaleType = ScaleType.Numerical
                            series2.ValueDataMembers.AddRange(New String() {"porcentaje"})
                            series2.Label.BackColor = Color.DarkBlue
                            series2.Label.TextColor = Color.White
                            series2.Label.Font = miFuente
                            series2.Label.TextPattern = "{V:F0}"

                            CType(series2.View, SplineSeriesView).LineStyle.Thickness = config.Tables(0).Rows(0)!grueso_spiline

                            Dim myAxisY As New SecondaryAxisY(traduccion(63))
                            CType(ChartControl1.Diagram, XYDiagram).SecondaryAxesY.Clear()
                            CType(ChartControl1.Diagram, XYDiagram).SecondaryAxesY.Add(myAxisY)
                            CType(series2.View, LineSeriesView).AxisY = myAxisY
                            myAxisY.Title.Text = config.Tables(0).Rows(0)!texto_z
                            myAxisY.Title.Visible = True
                            myAxisY.Label.Font = miFuenteEjes
                            myAxisY.Title.Font = miFuenteAlto
                            myAxisY.GridLines.Visible = False
                            myAxisY.Tickmarks.Visible = False
                            myAxisY.Tickmarks.MinorVisible = False
                            myAxisY.Title.TextColor = Color.Green
                            myAxisY.Label.TextColor = Color.Green
                            myAxisY.Color = Color.Green
                        End If
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.GridLines.Visible = False
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.Font = miFuenteEjes
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Text = Strings.Space(5) & config.Tables(0).Rows(0)!texto_x & Strings.Space(10)
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.GridLines.Visible = False
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Font = miFuenteAlto
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Visibility = DevExpress.Utils.DefaultBoolean.True
                        If config.Tables(0).Rows(0)!overlap = "R" Then
                            CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowStagger = False
                            CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowRotate = True
                        Else
                            CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowStagger = True
                            CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowRotate = False
                        End If


                        ChartControl1.Titles.Add(Titulo)
                        Dim Titulo2 As New ChartTitle()

                        Titulo2.Font = miFuente
                        Titulo2.Text = traduccion(56) & cadPeriodo
                        ChartControl1.Titles.Add(Titulo2)
                        Dim Titulo3 As New ChartTitle()
                        Titulo3.Font = miFuente
                        Titulo3.Text = traduccion(57) & Format(Now, "ddd dd-MMM-yyyy HH:mm:ss")
                        ChartControl1.Titles.Add(Titulo3)
                        Dim Titulo4 As New ChartTitle()
                        Titulo4.Font = miFuente
                        Titulo4.Text = traduccion(58) & Format(eDesde, "dd-MMM-yyyy HH:mm:ss") & traduccion(59) &
                                                            Format(eHasta, "dd-MMM-yyyy HH:mm:ss")
                        ChartControl1.Titles.Add(Titulo4)
                        ChartControl1.Width = 1200
                        ChartControl1.Height = 800

                        If config.Tables(0).Rows(0)!ver_leyenda = "S" Then
                            ChartControl1.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True
                        Else
                            ChartControl1.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False
                        End If
                        Try
                            Dim rutaImagen = Microsoft.VisualBasic.Strings.Replace(archivoImagen, "\", "\\")
                            SaveChartImageToFile(ChartControl1, ImageFormat.Png, rutaImagen)
                            Dim image As Image = GetChartImage(ChartControl1, ImageFormat.Png)
                            image.Save(rutaImagen)

                        Catch ex As Exception
                            agregarLOG(traduccion(60) & ex.Message, 7, 0)
                        End Try
                    End If
                End If
            End If


    End Function

    Function generarReporteTipo4(idReporte As Integer, reporte As String, fName As String, ruta As String, graficar As String, consulta As Long, modulo As Integer, grafica As Long) As Integer
        generarReporteTipo4 = 0

        Dim archivoSaliente = ruta & "\" & fName & ".csv"
        Dim archivoImagen = ruta & "\" & fName & ".png"

        Try
            My.Computer.FileSystem.DeleteFile(archivoSaliente)


        Catch ex As Exception

        End Try

        Try
            My.Computer.FileSystem.DeleteFile(archivoImagen)


        Catch ex As Exception

        End Try

        archivoSaliente = archivoSaliente.Replace("\", "\\")


        Dim comillas = Microsoft.VisualBasic.Strings.Left(Chr(34), 1)

        Dim inicial = ""
        Dim cabecera = ""
        Dim registros = ""

        Dim Leer As Boolean = False
        Dim cadSQL = ""

        cadSQL = "SELECT * FROM " & rutaBD & ".pu_graficos WHERE (usuario = 1 OR usuario = 0) AND grafico = " & grafica & " AND idioma = " & IIf(be_idioma = 0, 1, be_idioma) & " ORDER BY usuario DESC LIMIT 1"

        Dim config As DataSet = consultaSEL(cadSQL)
        If config.Tables(0).Rows.Count > 0 Then
            Dim ordenPareto = ValNull(config.Tables(0).Rows(0)!orden, "N")
            Dim tipoPeriodos = IIf(ValNull(config.Tables(0).Rows(0)!adicionales, "A") = "", "0;0;0;0;0;0;0", config.Tables(0).Rows(0)!adicionales)
            tipoPeriodos = IIf(config.Tables(0).Rows(0)!adicionales = "NNNNNN", "0;0;0;0;0;0;0", config.Tables(0).Rows(0)!adicionales)
            Dim periodos = tipoPeriodos.Split(New Char() {";"c})

            agregados.Rows.Clear()
            agregados.Columns.Clear()

            Dim titulos = IIf(ValNull(config.Tables(0).Rows(0)!adicionales_titulos, "A") = "", ";;;;;;", config.Tables(0).Rows(0)!adicionales_titulos)
            tituloPeriodos = titulos.Split(New Char() {";"c})
            agregados.Columns.Add("fitro", GetType(Integer))
            agregados.Columns.Add("id", GetType(Long))
            agregados.Columns.Add("pareto", GetType(Long))
            agregados.Columns.Add("nombre", GetType(String))
            agregados.Columns.Add("paros_m", GetType(Long))
            agregados.Columns.Add("produccion_m", GetType(Long))
            agregados.Columns.Add("piezas_m", GetType(Long))
            agregados.Columns.Add("rechazos_m", GetType(Double))
            agregados.Columns.Add("calidad_m", GetType(Double))
            agregados.Columns.Add("disponible_m", GetType(Double))
            agregados.Columns.Add("calidad", GetType(Double))
            agregados.Columns.Add("rendimiento", GetType(Double))
            agregados.Columns.Add("disponibilidad", GetType(Double))
            agregados.Columns.Add("oee", GetType(Double))

            Dim agrupando As Boolean = False
            If tipoPeriodos <> "0;0;0;0;0;0;0" Then
                agrupando = True
                For i = 0 To 6
                    If Val(periodos(i)) > 0 Then
                        buscarPeriodos(periodos(i), modulo, i, idReporte)
                    End If

                Next

            End If

            If consulta > 0 Then generarFIltro(consulta)

            Dim mensajesDS As DataSet
            Dim sentencia As String = ""
            Dim adicional = ""
            Dim regsAfectados = 0
            Dim tHaving = ""
            Dim ordenDatos = " 5 "
            Dim campoSumar = "oee"

            Dim filtroTiempo = " a.fecha >= '" & fDesdeSF & "' AND a.fecha <= '" & fHastaSF & "' "
            Dim filtrotiempo2 = " dia >= '" & fDesdeSF & "' AND dia <= '" & fHastaSF & "' "
            If sDesde <> "-1" Then
                filtroTiempo = " a.secuencia_turno " & sDesde & " "
                filtrotiempo2 = " turno_secuencia " & sDesde & " "
            End If

            inicial = Chr(34) & reporte & " (" & cadPeriodo & ")" & Chr(34) & vbCrLf
            inicial = inicial & Chr(34) & traduccion(110) & ": " & Format(Now(), "ddd, dd-MMM-yyyy HH:mm:ss") & Chr(34) & vbCrLf
            If secuencia <> 0 Then
                inicial = inicial & Chr(34) & traduccion(69) & ": " & miTurno & Chr(34) & vbCrLf
            Else
                inicial = inicial & Chr(34) & traduccion(111) & ": " & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & " " & traduccion(112) & ": " & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & Chr(34) & vbCrLf

            End If

            Dim miReporte = idReporte - 100
            If miReporte = 1 Then
                sentencia = "SELECT 1 AS pareto, 0 AS id, 0 AS orden, 1 AS filtro, 100 AS calidad, 0 AS rendimiento, 100 AS disponibilidad, 0 AS oee, DATE_FORMAT(fecha, '%Y/%m/%d') AS nombre, IFNULL(SUM(i.paros), 0) AS paros_m, IFNULL(SUM(i.produccion), 0) AS produccion_m, IFNULL(SUM(i.piezas), 0) AS piezas_m, IFNULL(SUM(i.rechazos), 0) AS rechazos_m, IFNULL(SUM(i.calidad), 0) AS calidad_m, IFNULL(SUM(i.disponible), 0) AS disponible_m FROM " & rutaBD & ".dias a LEFT JOIN (SELECT i.dia, SUM(i.paro) AS paros, SUM(i.produccion_tc) AS produccion, SUM(i.produccion) AS piezas, SUM(i.calidad) AS rechazos, SUM(i.tiempo_disponible) AS disponible, SUM(i.calidad_tc) AS calidad  FROM (SELECT a.equipo, a.fecha AS dia, 0 AS paro, a.parte, a.turno, a.lote AS orden, 0 AS produccion_tc, 0 AS produccion, a.cantidad AS calidad, a.cantidad_tc AS calidad_tc, 0 AS tiempo_disponible FROM " & rutaBD & ".detallerechazos a INNER JOIN " & rutaBD & ".lecturas_cortes b ON a.corte = b.id WHERE " & filtroTiempo & " UNION ALL SELECT equipo, dia, 0 AS paro, parte, turno, orden, 0 AS produccion_tc, 0 AS produccion, calidad, calidad_tc, 0 AS tiempo_disponible FROM " & rutaBD & ".lecturas_cortes WHERE calidad - calidad_clasificada > 0 AND " & filtrotiempo2 & " UNION ALL SELECT equipo, dia, paro, parte, turno, orden, produccion_tc, produccion, 0 AS calidad, 0 AS calidad_tc, tiempo_disponible FROM " & rutaBD & ".lecturas_cortes WHERE " & filtrotiempo2 & ") AS i INNER JOIN " & rutaBD & ".cat_maquinas j ON i.equipo = j.id AND j.oee = 'S' " & filtroOEEDias & " WHERE i.dia >= '" & fDesdeSF & "' AND i.dia <= '" & fHastaSF & "'  GROUP BY dia) AS i ON i.dia = a.fecha WHERE a.fecha >= '" & fDesdeSF & "' AND a.fecha <= '" & fHastaSF & "' GROUP BY 9  "


                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(51) & Chr(34) & "," & Chr(34) & traduccion(158) & Chr(34) & "," & Chr(34) & traduccion(159) & Chr(34) & "," & Chr(34) & traduccion(160) & Chr(34) & "," & Chr(34) & traduccion(161) & Chr(34) & "," & Chr(34) & traduccion(162) & Chr(34) & "," & Chr(34) & traduccion(163) & Chr(34) & "," & Chr(34) & traduccion(164) & Chr(34) & "," & Chr(34) & traduccion(165) & Chr(34) & "," & Chr(34) & traduccion(166) & Chr(34) & "," & Chr(34) & traduccion(167) & Chr(34)
            End If
            mensajesDS = consultaSEL(sentencia)
            adicional = ""
            If mensajesDS.Tables(0).Rows.Count > 0 Then
                Dim view_o As DataView = New DataView(mensajesDS.Tables(0))

                Dim objWriter As New System.IO.StreamWriter(archivoSaliente, False, System.Text.Encoding.UTF8)
                objWriter.WriteLine(inicial)
                objWriter.WriteLine(cabecera)
                Dim totalPareto As Double = 0

                For Each elmensaje As DataRow In mensajesDS.Tables(0).Rows
                    If elmensaje!disponible_m > 0 Then
                        elmensaje!disponibilidad = (elmensaje!disponible_m - elmensaje!paros_m) / elmensaje!disponible_m * 100
                        elmensaje!disponibilidad = IIf(elmensaje!disponibilidad > 100, 100, elmensaje!disponibilidad)
                    End If
                    If elmensaje!disponible_m - elmensaje!paros_m > 0 Then
                        elmensaje!rendimiento = elmensaje!produccion_m / (elmensaje!disponible_m - elmensaje!paros_m) * 100
                        elmensaje!rendimiento = IIf(elmensaje!rendimiento > 100, 100, elmensaje!rendimiento)
                    End If
                    If elmensaje!produccion_m > 0 Then
                        elmensaje!calidad = (elmensaje!produccion_m - elmensaje!calidad_m) / elmensaje!produccion_m * 100
                        elmensaje!calidad = IIf(elmensaje!calidad > 100, 100, elmensaje!calidad)
                    End If
                    elmensaje!oee = elmensaje!disponibilidad * elmensaje!rendimiento * elmensaje!calidad / 10000
                    If config.Tables(0).Rows(0)!incluir_ceros = "N" And elmensaje!oee = 0 Then
                        elmensaje!filtro = 0
                    End If

                Next
                view_o.RowFilter = "filtro = 1"
                Dim tmpTabla As DataTable = view_o.ToTable()
                Dim view_3 As DataView = New DataView(tmpTabla)


                If config.Tables(0).Rows(0)!maximo_barras > 0 And config.Tables(0).Rows(0)!maximo_barras < mensajesDS.Tables(0).Rows.Count Or config.Tables(0).Rows(0)!maximo_barraspct > 0 And config.Tables(0).Rows(0)!maximo_barraspct < 100 Then
                    'Se calcula el total del Pareto
                    Dim limitar = 0
                    Dim agrupado = ""
                    Dim pcAcum = 0
                    If config.Tables(0).Rows(0)!maximo_barraspct > 0 And config.Tables(0).Rows(0)!maximo_barraspct < 100 Then

                        For Each elmensaje As DataRow In tmpTabla.Rows
                            totalPareto = totalPareto + elmensaje.Item(campoSumar)
                        Next
                        Dim pct = config.Tables(0).Rows(0)!maximo_barraspct / 100
                        Dim i = 0
                        For Each elmensaje In tmpTabla.Rows
                            i = i + 1
                            pcAcum = pcAcum + elmensaje.Item(campoSumar)
                            If pcAcum / totalPareto >= pct Then
                                limitar = i
                                Exit For
                            End If
                        Next
                    End If
                    If config.Tables(0).Rows(0)!maximo_barras > 0 Then
                        If limitar > config.Tables(0).Rows(0)!maximo_barras Or limitar = 0 Then
                            limitar = config.Tables(0).Rows(0)!maximo_barras
                        End If
                    End If

                    If limitar + 1 >= tmpTabla.Rows.Count And config.Tables(0).Rows(0)!agrupar = "S" Then
                        limitar = 0
                    ElseIf limitar >= tmpTabla.Rows.Count Then
                        limitar = 0
                    End If
                    If limitar > 0 Then
                        For j = 0 To limitar - 1
                            tmpTabla.Rows(j)!orden = j + 1
                        Next
                        If config.Tables(0).Rows(0)!agrupar = "S" Then
                            Dim faltante1 = 0
                            Dim faltante2 = 0
                            Dim faltante3 = 0
                            Dim faltante4 = 0
                            Dim faltante5 = 0
                            Dim faltante6 = 0
                            Dim totalAgr = 0
                            For j = limitar To tmpTabla.Rows.Count - 1
                                tmpTabla.Rows(j)!filtro = 0
                                faltante1 = faltante1 + tmpTabla.Rows(j)!paros_m
                                faltante2 = faltante2 + tmpTabla.Rows(j)!produccion_m
                                faltante3 = faltante3 + tmpTabla.Rows(j)!piezas_m
                                faltante4 = faltante4 + tmpTabla.Rows(j)!rechazos_m
                                faltante5 = faltante5 + tmpTabla.Rows(j)!calidad_m
                                faltante6 = faltante6 + tmpTabla.Rows(j)!disponible_m
                            Next
                            totalAgr = tmpTabla.Rows.Count - limitar
                            Dim row As DataRow = tmpTabla.NewRow

                            row("id") = "0"
                            row("nombre") = IIf(ValNull(config.Tables(0).Rows(0)!agrupar_texto, "A") = "", traduccion(61), config.Tables(0).Rows(0)!agrupar_texto) & " (" & totalAgr & ")"
                            row("paros_m") = faltante1
                            row("produccion_m") = faltante2
                            row("piezas_m") = faltante3
                            row("rechazos_m") = faltante4
                            row("calidad_m") = faltante5
                            row("disponible_m") = faltante6
                            If faltante6 > 0 Then
                                row("disponibilidad") = (faltante6 - faltante1) / faltante6 * 100
                                row("disponibilidad") = IIf(row("disponibilidad") > 100, 100, row("disponibilidad"))
                            End If
                            If faltante6 - faltante1 > 0 Then
                                row("rendimiento") = faltante2 / (faltante6 - faltante1) * 100
                                row("rendimiento") = IIf(row("rendimiento") > 100, 100, row("rendimiento"))
                            End If
                            If faltante2 > 0 Then
                                row("calidad") = (faltante2 - faltante5) / faltante2 * 100
                                row("calidad") = IIf(row("calidad") > 100, 100, row("calidad"))
                            End If
                            row("oee") = row("disponibilidad") * row("rendimiento") * row("calidad") / 10000
                            row("filtro") = 1
                            row("pareto") = 1
                            If config.Tables(0).Rows(0)!agrupar_posicion = "F" Then
                                row("orden") = limitar + 1
                            ElseIf config.Tables(0).Rows(0)!agrupar_posicion = "P" Then
                                row("orden") = 0
                            End If
                            tmpTabla.Rows.Add(row)

                        Else
                            adicional = traduccion(147)
                        End If
                    End If
                    If config.Tables(0).Rows(0)!agrupar_posicion = "N" Then
                        view_3.Sort = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " " & campoSumar & " DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " " & campoSumar, "nombre"))
                    Else
                        view_3.Sort = "orden ASC"
                    End If
                Else
                    view_3.Sort = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " " & campoSumar & " DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " " & campoSumar, "nombre"))
                End If
                view_3.RowFilter = "filtro = 1"
                Dim acumPareto As Double = 0
                Dim linea = 0

                tablaGRafico = view_o.ToTable()
                Dim indice = 0
                For Each registro In agregados.Rows
                    Dim row As DataRow = tablaGRafico.NewRow

                    row("id") = "0"
                    row("nombre") = registro!nombre
                    row("paros_m") = registro!paros_m
                    row("produccion_m") = registro!produccion_m
                    row("piezas_m") = registro!piezas_m
                    row("rechazos_m") = registro!rechazos_m
                    row("calidad_m") = registro!calidad_m
                    row("disponible_m") = registro!disponible_m
                    If registro!disponible_m > 0 Then
                        row("disponibilidad") = (registro!disponible_m - registro!paros_m) / registro!disponible_m * 100
                        row("disponibilidad") = IIf(row("disponibilidad") > 100, 100, row("disponibilidad"))
                    End If
                    If registro!disponible_m - registro!paros_m > 0 Then
                        row("rendimiento") = registro!produccion_m / (registro!disponible_m - registro!paros_m) * 100
                        row("rendimiento") = IIf(row("rendimiento") > 100, 100, row("rendimiento"))
                    End If
                    If registro!produccion_m > 0 Then
                        row("calidad") = (registro!produccion_m - registro!calidad_m) / registro!produccion_m * 100
                        row("calidad") = IIf(row("calidad") > 100, 100, row("calidad"))
                    End If
                    row("oee") = row("disponibilidad") * row("rendimiento") * row("calidad") / 10000
                    row("filtro") = 1
                    row("pareto") = 0
                    tablaGRafico.Rows.InsertAt(row, indice)
                    indice = indice + 1
                Next


                tablaGRafico = view_3.ToTable()
                For Each registro In tablaGRafico.Rows
                    If registro!filtro = 1 Then
                        linea = linea + 1
                        Dim cadID = ""
                        'If miReporte <> 1 Then
                        ' cadID = Chr(34) & registro!id & Chr(34) & ","
                        ' End If
                        Dim cadInicio = ""
                        'If miReporte = 11 Then
                        ' cadInicio = Chr(34) & registro!inicio & Chr(34) & ","
                        ' End If
                        objWriter.WriteLine(Chr(34) & linea & Chr(34) & "," & cadID & Chr(34) & registro!nombre & Chr(34) & "," & cadInicio & Chr(34) & registro!paros_m & Chr(34) & "," & Chr(34) & registro!produccion_m & Chr(34) & "," & Chr(34) & registro!piezas_m & Chr(34) & "," & Chr(34) & registro!calidad_m & Chr(34) & "," & Chr(34) & registro!rechazos_m & Chr(34) & "," & Chr(34) & registro!disponible_m & Chr(34) & "," & Chr(34) & registro!calidad & Chr(34) & "," & Chr(34) & registro!rendimiento & Chr(34) & "," & Chr(34) & registro!disponibilidad & Chr(34) & "," & Chr(34) & registro!oee & Chr(34))
                    End If
                Next
                objWriter.WriteLine(traduccion(121) & ": " & linea)
                If adicional.Length > 0 Then objWriter.WriteLine(adicional)
                objWriter.WriteLine(traduccion(122))
                objWriter.Close()

                If graficar = "S" Then


                    Dim indicador01 = ""
                    Dim indicador02 = ""
                    Dim indicador03 = ""
                    Dim indicador04 = ""

                    Dim titulosSeries = config.Tables(0).Rows(0)!oee_nombre.Split(New Char() {";"c})
                    If titulosSeries.length = 1 Then
                        indicador02 = titulosSeries(0)
                    End If
                    If titulosSeries.length = 2 Then
                        indicador02 = titulosSeries(0)
                        indicador03 = titulosSeries(1)
                    End If
                    If titulosSeries.length = 3 Then
                        indicador02 = titulosSeries(0)
                        indicador03 = titulosSeries(1)
                        indicador04 = titulosSeries(2)
                    End If
                    If indicador01.Length = 0 Then indicador01 = traduccion(167)
                    If indicador02.Length = 0 Then indicador02 = traduccion(164)
                    If indicador03.Length = 0 Then indicador03 = traduccion(165)
                    If indicador04.Length = 0 Then indicador04 = traduccion(166)
                    Dim tipoOEE = ValNull(config.Tables(0).Rows(0)!oee_tipo, "A")
                    If tipoOEE = "" Then
                        tipoOEE = "LLL"
                    End If

                    Dim graficaOEE = ValNull(config.Tables(0).Rows(0)!oee, "A")
                    If graficaOEE = "" Then
                        graficaOEE = "SSSS"
                    End If


                    ChartControl1.Series.Clear()
                    ChartControl1.Titles.Clear()
                    Dim Titulo As New ChartTitle()
                    Titulo.Text = config.Tables(0).Rows(0)!titulo & Strings.Space(10)
                    Dim miFuente = New Drawing.Font("Lucida Sans", 10, FontStyle.Regular)
                    Dim miFuenteAlto = New Drawing.Font("Lucida Sans", 16, FontStyle.Bold)
                    Dim miFuenteEjes = New Drawing.Font("Lucida Sans", 11, FontStyle.Regular)

                    Titulo.Font = miFuenteAlto
                    Dim series1 As New Series(indicador01, ViewType.Bar)

                    ChartControl1.Series.Add(series1)
                    series1.DataSource = tablaGRafico

                    series1.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                    series1.View.Color = Color.SkyBlue
                    series1.ArgumentScaleType = ScaleType.Qualitative
                    series1.ArgumentDataMember = "nombre"
                    series1.ValueScaleType = ScaleType.Numerical
                    series1.ValueDataMembers.AddRange(New String() {"oee"})
                    series1.Label.BackColor = Color.DarkBlue
                    series1.Label.TextColor = Color.White
                    series1.Label.Font = miFuente
                    series1.Label.TextPattern = "{V:F" & config.Tables(0).Rows(0)!etiqueta_formato & "}"

                    'Obtener el titulo en Y
                    Dim tituloY As String = ""
                    Dim titulosY = ValNull(config.Tables(0).Rows(0)!texto_y, "A")
                    Try

                        Dim titulosYArreglo = titulosY.Split(New Char() {";"c})
                        If titulosYArreglo.length = 1 Then
                            tituloY = titulosYArreglo(0)
                        Else
                            tituloY = titulosYArreglo(ordenPareto)
                        End If
                    Catch ex As Exception
                        tituloY = titulosY
                    End Try

                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Visibility = DevExpress.Utils.DefaultBoolean.True
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Label.Font = miFuenteEjes
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.GridSpacingAuto = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.GridSpacing = 1
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Text = tituloY
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Font = miFuenteAlto
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.GridLines.Visible = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Tickmarks.Visible = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Tickmarks.MinorVisible = False
                    Dim view_2 As DataView = New DataView(tablaGRafico)
                    Dim tablaPareto As DataTable = view_2.ToTable()

                    AddHandler Me.ChartControl1.CustomDrawSeriesPoint, AddressOf Me.ValidarPuntos
                    If Strings.Left(graficaOEE, 1) = "S" Then
                        Dim series2 As New Series(indicador02, IIf(Strings.Left(tipoOEE, 1) = "L", ViewType.Spline, IIf(Strings.Left(tipoOEE, 1) = "B", ViewType.Bar, ViewType.Area)))

                        ChartControl1.Series.Add(series2)
                        series2.DataSource = tablaPareto
                        series2.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                        series2.View.Color = Color.Red

                        series2.ArgumentScaleType = ScaleType.Qualitative
                        series2.ArgumentDataMember = "nombre"
                        series2.ValueScaleType = ScaleType.Numerical
                        series2.ValueDataMembers.AddRange(New String() {"calidad"})
                        series2.Label.BackColor = Color.DarkBlue
                        series2.Label.TextColor = Color.White
                        series2.Label.Font = miFuente
                        series2.Label.TextPattern = "{V:F" & config.Tables(0).Rows(0)!etiqueta_formato & "}"

                        If Strings.Left(tipoOEE, 1) = "L" Then
                            CType(series2.View, SplineSeriesView).LineStyle.Thickness = config.Tables(0).Rows(0)!grueso_spiline
                        End If

                        Dim myAxisY As New SecondaryAxisY(traduccion(63))
                        CType(ChartControl1.Diagram, XYDiagram).SecondaryAxesY.Clear()
                        CType(ChartControl1.Diagram, XYDiagram).SecondaryAxesY.Add(myAxisY)
                        CType(series2.View, LineSeriesView).AxisY = myAxisY
                        myAxisY.Title.Text = config.Tables(0).Rows(0)!texto_z
                        myAxisY.Title.Visible = True
                        myAxisY.Label.Font = miFuenteEjes
                        myAxisY.Title.Font = miFuenteAlto
                        myAxisY.GridLines.Visible = False
                        myAxisY.Tickmarks.Visible = False
                        myAxisY.Tickmarks.MinorVisible = False
                        myAxisY.Title.TextColor = Color.Red
                        myAxisY.Label.TextColor = Color.Red
                        myAxisY.Color = Color.Red
                    End If

                    If Strings.Mid(graficaOEE, 2, 1) = "S" Then
                        Dim series2 As New Series(indicador03, IIf(Strings.Mid(tipoOEE, 2, 1) = "L", ViewType.Spline, IIf(Strings.Mid(tipoOEE, 2, 1) = "B", ViewType.Bar, ViewType.Area)))

                        ChartControl1.Series.Add(series2)
                        series2.DataSource = tablaPareto
                        series2.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                        series2.View.Color = Color.Green

                        series2.ArgumentScaleType = ScaleType.Qualitative
                        series2.ArgumentDataMember = "nombre"
                        series2.ValueScaleType = ScaleType.Numerical
                        series2.ValueDataMembers.AddRange(New String() {"rendimiento"})
                        series2.Label.BackColor = Color.DarkBlue
                        series2.Label.TextColor = Color.White
                        series2.Label.Font = miFuente
                        series2.Label.TextPattern = "{V:F" & config.Tables(0).Rows(0)!etiqueta_formato & "}"

                        If Strings.Mid(tipoOEE, 2, 1) = "L" Then
                            CType(series2.View, SplineSeriesView).LineStyle.Thickness = config.Tables(0).Rows(0)!grueso_spiline
                        End If

                        Dim myAxisY As New SecondaryAxisY(traduccion(63))
                        CType(ChartControl1.Diagram, XYDiagram).SecondaryAxesY.Clear()
                        CType(ChartControl1.Diagram, XYDiagram).SecondaryAxesY.Add(myAxisY)
                        CType(series2.View, LineSeriesView).AxisY = myAxisY
                        myAxisY.Title.Text = config.Tables(0).Rows(0)!texto_z
                        myAxisY.Title.Visible = True
                        myAxisY.Label.Font = miFuenteEjes
                        myAxisY.Title.Font = miFuenteAlto
                        myAxisY.GridLines.Visible = False
                        myAxisY.Tickmarks.Visible = False
                        myAxisY.Tickmarks.MinorVisible = False
                        myAxisY.Title.TextColor = Color.Green
                        myAxisY.Label.TextColor = Color.Green
                        myAxisY.Color = Color.Green
                    End If
                    If Strings.Mid(graficaOEE, 3, 1) = "S" Then
                        Dim series2 As New Series(indicador04, IIf(Strings.Mid(tipoOEE, 3, 1) = "L", ViewType.Spline, IIf(Strings.Mid(tipoOEE, 3, 1) = "B", ViewType.Bar, ViewType.Area)))

                        ChartControl1.Series.Add(series2)
                        series2.DataSource = tablaPareto
                        series2.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                        series2.View.Color = Color.DodgerBlue

                        series2.ArgumentScaleType = ScaleType.Qualitative
                        series2.ArgumentDataMember = "nombre"
                        series2.ValueScaleType = ScaleType.Numerical
                        series2.ValueDataMembers.AddRange(New String() {"disponibilidad"})
                        series2.Label.BackColor = Color.DarkBlue
                        series2.Label.TextColor = Color.White
                        series2.Label.Font = miFuente
                        series2.Label.TextPattern = "{V:F" & config.Tables(0).Rows(0)!etiqueta_formato & "}"

                        If Strings.Mid(tipoOEE, 3, 1) = "L" Then
                            CType(series2.View, SplineSeriesView).LineStyle.Thickness = config.Tables(0).Rows(0)!grueso_spiline
                        End If

                        Dim myAxisY As New SecondaryAxisY(traduccion(63))
                        CType(ChartControl1.Diagram, XYDiagram).SecondaryAxesY.Clear()
                        CType(ChartControl1.Diagram, XYDiagram).SecondaryAxesY.Add(myAxisY)
                        CType(series2.View, LineSeriesView).AxisY = myAxisY
                        myAxisY.Title.Text = config.Tables(0).Rows(0)!texto_z
                        myAxisY.Title.Visible = True
                        myAxisY.Label.Font = miFuenteEjes
                        myAxisY.Title.Font = miFuenteAlto
                        myAxisY.GridLines.Visible = False
                        myAxisY.Tickmarks.Visible = False
                        myAxisY.Tickmarks.MinorVisible = False
                        myAxisY.Title.TextColor = Color.DodgerBlue
                        myAxisY.Label.TextColor = Color.DodgerBlue
                        myAxisY.Color = Color.DodgerBlue
                    End If
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.GridLines.Visible = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.Font = miFuenteEjes
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Text = Strings.Space(5) & config.Tables(0).Rows(0)!texto_x & Strings.Space(10)
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.GridLines.Visible = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Font = miFuenteAlto
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Visibility = DevExpress.Utils.DefaultBoolean.True
                    If config.Tables(0).Rows(0)!overlap = "R" Then
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowStagger = False
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowRotate = True
                    Else
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowStagger = True
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowRotate = False
                    End If


                    ChartControl1.Titles.Add(Titulo)
                    Dim Titulo2 As New ChartTitle()

                    Titulo2.Font = miFuente
                    Titulo2.Text = traduccion(56) & cadPeriodo
                    ChartControl1.Titles.Add(Titulo2)
                    Dim Titulo3 As New ChartTitle()
                    Titulo3.Font = miFuente
                    Titulo3.Text = traduccion(57) & Format(Now, "ddd dd-MMM-yyyy HH:mm:ss")
                    ChartControl1.Titles.Add(Titulo3)
                    Dim Titulo4 As New ChartTitle()
                    Titulo4.Font = miFuente
                    Titulo4.Text = traduccion(58) & Format(eDesde, "dd-MMM-yyyy HH:mm:ss") & traduccion(59) &
                                                        Format(eHasta, "dd-MMM-yyyy HH:mm:ss")
                    ChartControl1.Titles.Add(Titulo4)
                    ChartControl1.Width = 1200
                    ChartControl1.Height = 800

                    If config.Tables(0).Rows(0)!ver_leyenda = "S" Then
                        ChartControl1.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True
                    Else
                        ChartControl1.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False
                    End If
                    Try
                        Dim rutaImagen = Microsoft.VisualBasic.Strings.Replace(archivoImagen, "\", "\\")
                        SaveChartImageToFile(ChartControl1, ImageFormat.Png, rutaImagen)
                        Dim image As Image = GetChartImage(ChartControl1, ImageFormat.Png)
                        image.Save(rutaImagen)

                    Catch ex As Exception
                        agregarLOG(traduccion(60) & ex.Message, 7, 0)
                    End Try
                End If
            End If
        End If


    End Function
    Function generarReporteTipo1y2(idReporte As Integer, reporte As String, fName As String, ruta As String, graficar As String, consulta As Long, modulo As Integer, grafica As Long) As Integer
        generarReporteTipo1y2 = 0

        Dim archivoSaliente = ruta & "\" & fName & ".csv"
        Dim archivoImagen = ruta & "\" & fName & ".png"

        Try
            My.Computer.FileSystem.DeleteFile(archivoSaliente)


        Catch ex As Exception

        End Try

        Try
            My.Computer.FileSystem.DeleteFile(archivoImagen)


        Catch ex As Exception

        End Try

        archivoSaliente = archivoSaliente.Replace("\", "\\")


        Dim comillas = Microsoft.VisualBasic.Strings.Left(Chr(34), 1)

        Dim inicial = ""
        Dim cabecera = ""
        Dim registros = ""

        Dim Leer As Boolean = False
        Dim cadSQL = ""

        Dim miReporte = idReporte
        If modulo = 2 Then miReporte = miReporte - 13

        cadSQL = "SELECT * FROM " & rutaBD & ".pu_graficos WHERE (usuario = 1 OR usuario = 0) AND grafico = " & grafica & " AND idioma = " & IIf(be_idioma = 0, 1, be_idioma) & " ORDER BY usuario DESC LIMIT 1"

        Dim config As DataSet = consultaSEL(cadSQL)

        If consulta > 0 Then generarFIltro(consulta)

        Dim mensajesDS As DataSet
        Dim sentencia As String = ""
        Dim adicional = ""
        Dim regsAfectados = 0
        Dim tHaving = ""
        Dim ordenDatos = " 11 DESC"
        Dim campoSumar = "mttrc"


        Dim filtroTiempo = " AND c.fecha_reporte >= '" & fDesdeSF & "' AND c.fecha_reporte <= '" & fHastaSF & "' "
        If sDesde <> "-1" Then
            filtroTiempo = " AND c.secuencia " & sDesde & " "
        End If

        If idReporte = 42 Then
            'Reporte de Reportes abiertas
            inicial = Chr(34) & reporte & " (" & cadPeriodo & ")" & Chr(34) & vbCrLf
            inicial = inicial & Chr(34) & traduccion(110) & ": " & Format(Now(), "ddd, dd-MMM-yyyy HH:mm:ss") & Chr(34) & vbCrLf
            inicial = inicial & Chr(34) & traduccion(111) & ": " & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & " " & traduccion(112) & ": " & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & Chr(34) & vbCrLf

            cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(66) & Chr(34) & "," & Chr(34) & traduccion(67) & Chr(34) & "," & Chr(34) & traduccion(68) & Chr(34) & "," & Chr(34) & traduccion(69) & Chr(34) & "," & Chr(34) & traduccion(70) & Chr(34) & "," & Chr(34) & traduccion(71) & Chr(34) & "," & Chr(34) & traduccion(72) & Chr(34) & "," & Chr(34) & traduccion(73) & Chr(34) & "," & Chr(34) & traduccion(74) & Chr(34) & "," & Chr(34) & traduccion(75) & Chr(34) & "," & IIf(moduloOEE, Chr(34) & traduccion(123) & Chr(34) & ",", "") & Chr(34) & traduccion(76) & Chr(34) & "," & Chr(34) & traduccion(77) & Chr(34) & "," & Chr(34) & traduccion(78) & Chr(34) & "," & Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(80) & Chr(34) & "," & Chr(34) & traduccion(81) & Chr(34) & "," & Chr(34) & traduccion(82) & Chr(34) & "," & Chr(34) & traduccion(83) & Chr(34) & "," & Chr(34) & traduccion(84) & Chr(34) & "," & Chr(34) & traduccion(85) & Chr(34) & "," & Chr(34) & traduccion(86) & Chr(34) & "," & Chr(34) & traduccion(87) & Chr(34) & "," & Chr(34) & traduccion(88) & Chr(34) & "," & Chr(34) & traduccion(89) & Chr(34) & "," & Chr(34) & traduccion(90) & Chr(34) & "," & Chr(34) & traduccion(91) & Chr(34) & "," & Chr(34) & traduccion(92) & Chr(34) & "," & Chr(34) & traduccion(93) & Chr(34) & "," & Chr(34) & traduccion(94) & Chr(34) & "," & Chr(34) & traduccion(95) & Chr(34) & "," & Chr(34) & traduccion(96) & Chr(34) & "," & Chr(34) & traduccion(97) & Chr(34) & "," & Chr(34) & traduccion(98) & Chr(34) & "," & Chr(34) & traduccion(99) & Chr(34)

            If modulo2 Then
                cabecera = cabecera & "," & Chr(34) & traduccion(124) & Chr(34) & "," & Chr(34) & traduccion(125) & Chr(34) & "," & Chr(34) & traduccion(126) & Chr(34) & "," & Chr(34) & traduccion(127) & Chr(34) & "," & Chr(34) & traduccion(128) & Chr(34) & "," & Chr(34) & traduccion(129) & Chr(34) & "," & Chr(34) & traduccion(130) & Chr(34) & "," & Chr(34) & traduccion(131) & Chr(34) & "," & Chr(34) & traduccion(132) & Chr(34) & "," & Chr(34) & traduccion(133) & Chr(34) & "," & Chr(34) & traduccion(134) & Chr(34) & "," & Chr(34) & traduccion(135) & Chr(34) & "," & Chr(34) & traduccion(136) & Chr(34) & "," & Chr(34) & traduccion(137) & Chr(34) & "," & Chr(34) & traduccion(138) & Chr(34) & "," & Chr(34) & traduccion(139) & Chr(34) & "," & Chr(34) & traduccion(140) & Chr(34) & "," & Chr(34) & traduccion(141) & Chr(34) & "," & Chr(34) & traduccion(142) & Chr(34) & "," & Chr(34) & traduccion(143) & Chr(34) & "," & Chr(34) & traduccion(144) & Chr(34)
            End If


            sentencia = "SELECT c.id, c.fecha, c.fecha_reporte, IFNULL(l.nombre, '" & traduccion(100) & "'), c.estatus, c.inicio_atencion, c.tiempollegada, c.cierre_atencion, c.tiemporeparacion, c.tiemporeparacion + c.tiempollegada, " & IIf(moduloOEE, "IF(c.ultimo_rate > 0, (c.tiemporeparacion + c.tiempollegada) / c.ultimo_rate, 0), ", "") & "c.inicio_reporte, c.cierre_reporte, c.tiemporeporte, IFNULL(a.nombre, '" & traduccion(100) & "'), c.linea, IFNULL(b.nombre, '" & traduccion(100) & "'), c.maquina, IFNULL(d.nombre, '" & traduccion(100) & "'), c.area, IFNULL(e.nombre, '" & traduccion(100) & "'), c.falla_ajustada, IFNULL(f.nombre, '" & traduccion(100) & "'), IFNULL(j.nombre, '" & traduccion(100) & "'), IFNULL(h.nombre, '" & traduccion(100) & "'), IFNULL(g.nombre, '" & traduccion(100) & "'), IFNULL(m.nombre, '" & traduccion(100) & "'), IFNULL(k.nombre, '" & traduccion(100) & "'), c.detalle, IF(c.contabilizar = 'S', '" & traduccion(101) & "', '" & traduccion(102) & "'), IF(c.alarmado = 'S', '" & traduccion(101) & "', '" & traduccion(102) & "'), IF(c.alarmado_atender = 'S', '" & traduccion(101) & "', '" & traduccion(102) & "'), IF(c.alarmado_atendido = 'S', '" & traduccion(101) & "', '" & traduccion(102) & "'), IFNULL(i.nombre, '" & traduccion(100) & "'), c.falla, w.nombre, w.referencia, z.p1, z.p2, z.p3, z.p4, z.p5, z.plan, z.fecha, z.responsable, z.departamento, z.mano_de_obra, z.material, z.metodo, z.maquina, z.medio_ambiente, z.comentarios, y.nombre, x.nombre, z.creacion, z.modificacion FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_lineas a ON c.linea = a.id LEFT JOIN " & rutaBD & ".cat_maquinas b ON c.maquina = b.id LEFT JOIN " & rutaBD & ".cat_areas d ON c.area = d.id LEFT JOIN " & rutaBD & ".cat_fallas e ON c.falla_ajustada = e.id LEFT JOIN " & rutaBD & ".cat_usuarios f ON c.solicitante = f.id LEFT JOIN " & rutaBD & ".cat_usuarios g ON g.tecnico = g.id LEFT JOIN " & rutaBD & ".cat_usuarios h ON c.tecnicoatend = h.id LEFT JOIN " & rutaBD & ".cat_fallas i ON c.falla = i.id LEFT JOIN " & rutaBD & ".cat_generales j ON f.departamento = j.id LEFT JOIN " & rutaBD & ".cat_generales k ON c.tipo = k.id LEFT JOIN " & rutaBD & ".cat_turnos l ON c.turno = l.id LEFT JOIN " & rutaBD & ".cat_usuarios m ON c.confirmado = m.id LEFT JOIN " & rutaBD & ".causa_raiz z ON c.id = z.reporte LEFT JOIN " & rutaBD & ".cat_usuarios y ON z.creado = y.id LEFT JOIN " & rutaBD & ".cat_usuarios x ON z.modificado = x.id LEFT JOIN " & rutaBD & ".cat_partes w ON c.herramental = w.id LEFT JOIN " & rutaBD & ".cat_usuarios v ON c.contabilizar_usuario = v.id WHERE c.estatus >= 0 " & filtroTiempo & " " & filtroReportes

            mensajesDS = consultaSEL(sentencia)
            If mensajesDS.Tables(0).Rows.Count > 0 Then
                Dim objWriter As New System.IO.StreamWriter(archivoSaliente, False, System.Text.Encoding.UTF8)
                objWriter.WriteLine(inicial)
                objWriter.WriteLine(cabecera)
                Dim linea = 0
                Dim limiteCampo = IIf(modulo2, 0, 36)
                For Each registro As DataRow In mensajesDS.Tables(0).Rows
                    linea = linea + 1
                    Dim cadPrint = Chr(34) & linea & Chr(34) & ","
                    Dim columna = 0
                    For Each campo In registro.ItemArray
                        columna = columna + 1
                        If limiteCampo > 0 And columna = limiteCampo Then
                            Exit For
                        End If
                        cadPrint = cadPrint & Chr(34) & campo & Chr(34) & ","
                    Next
                    objWriter.WriteLine(cadPrint)
                Next
                If adicional.Length > 0 Then objWriter.WriteLine(adicional)
                objWriter.WriteLine(traduccion(122))
                objWriter.Close()

            End If
        Else

            Dim ordenPareto = ValNull(config.Tables(0).Rows(0)!orden, "N")
            Dim tipoPeriodos = IIf(ValNull(config.Tables(0).Rows(0)!adicionales, "A") = "", "0;0;0;0;0;0;0", config.Tables(0).Rows(0)!adicionales)
            tipoPeriodos = IIf(config.Tables(0).Rows(0)!adicionales = "NNNNNN", "0;0;0;0;0;0;0", config.Tables(0).Rows(0)!adicionales)
            Dim periodos = tipoPeriodos.Split(New Char() {";"c})

            agregados.Rows.Clear()
            agregados.Columns.Clear()

            Dim titulos = IIf(ValNull(config.Tables(0).Rows(0)!adicionales_titulos, "A") = "", ";;;;;;", config.Tables(0).Rows(0)!adicionales_titulos)
            tituloPeriodos = titulos.Split(New Char() {";"c})
            agregados.Columns.Add("porcentaje", GetType(Double))
            agregados.Columns.Add("fitro", GetType(Integer))
            agregados.Columns.Add("id", GetType(Long))
            agregados.Columns.Add("nombre", GetType(String))
            agregados.Columns.Add("tdias", GetType(Long))
            agregados.Columns.Add("tdisponible", GetType(Long))
            agregados.Columns.Add("docs", GetType(Long))
            agregados.Columns.Add("tiempo_c", GetType(Double))
            agregados.Columns.Add("mttrc", GetType(Double))
            agregados.Columns.Add("mtbfc", GetType(Double))

            Dim agrupando As Boolean = False
            If tipoPeriodos <> "0;0;0;0;0;0;0" Then
                agrupando = True
                For i = 0 To 6
                    If Val(periodos(i)) > 0 Then
                        buscarPeriodos(periodos(i), modulo, i, idReporte)
                    End If

                Next

            End If

            inicial = Chr(34) & reporte & " (" & cadPeriodo & ")" & Chr(34) & vbCrLf
            inicial = inicial & Chr(34) & traduccion(110) & ": " & Format(Now(), "ddd, dd-MMM-yyyy HH:mm:ss") & Chr(34) & vbCrLf
            inicial = inicial & Chr(34) & traduccion(111) & ": " & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & " " & traduccion(112) & ": " & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & Chr(34) & vbCrLf

            If idReporte > 13 Then
                campoSumar = "mtbfc"
            End If

            ordenDatos = " 12 DESC"
            If config.Tables(0).Rows(0)!incluir_ceros = "N" Then
                tHaving = " HAVING mttrc > 0 "
            End If


            If config.Tables(0).Rows(0)!orden_grafica = "N" Then
                ordenDatos = " 12 "
            ElseIf config.Tables(0).Rows(0)!orden_grafica = "A" Then
                ordenDatos = " 6 "
            End If

            If idReporte > 13 Then
                ordenDatos = " 9 DESC"
                If config.Tables(0).Rows(0)!orden_grafica = "N" Then
                    ordenDatos = " 9 "
                End If
                If config.Tables(0).Rows(0)!incluir_ceros = "N" Then
                    tHaving = " HAVING mtbfc > 0 "
                End If
            End If

            If idReporte = 1 Or idReporte = 14 Then
                'If Not id01 Then
                ' id01 = True
                ' Leer = True
                ' End If
                'If Leer Then
                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(80) & Chr(34) & "," & Chr(34) & traduccion(42) & Chr(34) & "," & Chr(34) & traduccion(145) & Chr(34) & "," & Chr(34) & traduccion(113) & Chr(34) & "," & Chr(34) & traduccion(116) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(146) & Chr(34) & "," & Chr(34) & traduccion(114) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, a.linea AS id, b.nombre, e.tdias, SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, (SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0))) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_maquinas a " & IIf(modulo = 1, "INNER", "LEFT") & " JOIN (SELECT maquina, linea, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroTiempo & filtroReportes & " GROUP BY maquina, linea) AS c ON a.id = c.maquina LEFT JOIN " & rutaBD & ".cat_lineas b ON a.linea = b.id, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesdeSF & "' AND fecha <= '" & fHastaSF & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & filtroMTBF) & " GROUP BY a.linea, b.nombre " & tHaving & " ORDER BY " & ordenDatos & ";"
                'End If
            ElseIf idReporte = 2 Or idReporte = 15 Then
                'If Not id02 Then
                ' id02 = True
                ' Leer = True
                ' End If
                '    If Leer Then
                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(82) & Chr(34) & "," & Chr(34) & traduccion(43) & Chr(34) & "," & Chr(34) & traduccion(145) & Chr(34) & "," & Chr(34) & traduccion(113) & Chr(34) & "," & Chr(34) & traduccion(116) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(146) & Chr(34) & "," & Chr(34) & traduccion(114) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, a.id, a.nombre, e.tdias, SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, (SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0))) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_maquinas a " & IIf(modulo = 1, "INNER", "LEFT") & " JOIN (SELECT linea, maquina, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  " & filtroTiempo & filtroReportes & " GROUP BY linea, maquina) AS c ON a.id = c.maquina, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesdeSF & "' AND fecha <= '" & fHastaSF & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & filtroMTBF) & " GROUP BY a.id, a.nombre " & tHaving & " ORDER BY " & ordenDatos & ";"
                '   End If

            ElseIf idReporte = 3 Or idReporte = 16 Then
                'If Not id03 Then
                ' id03 = True
                ' Leer = True
                'End If
                '     If Leer Then
                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(84) & Chr(34) & "," & Chr(34) & traduccion(44) & Chr(34) & "," & Chr(34) & traduccion(145) & Chr(34) & "," & Chr(34) & traduccion(113) & Chr(34) & "," & Chr(34) & traduccion(116) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(146) & Chr(34) & "," & Chr(34) & traduccion(114) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)


                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, a.id, a.nombre, e.tdias, (e.lunes + e.martes + e.miercoles + e.jueves + e.viernes + e.sabado + e.domingo) AS tdisponible, (e.lunes + e.martes + e.miercoles + e.jueves + e.viernes + e.sabado + e.domingo) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_areas a " & IIf(modulo = 1, "INNER", "LEFT") & " JOIN (SELECT AREA, linea, maquina, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  " & filtroTiempo & filtroReportes & " GROUP BY AREA, linea, maquina) AS c ON a.id = c.AREA, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS lunes, SUM(IF(dia = 3, 1, 0)) * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS martes, SUM(IF(dia = 4, 1, 0)) * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS miercoles, SUM(IF(dia = 5, 1, 0)) * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS jueves, SUM(IF(dia = 6, 1, 0)) * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS viernes, SUM(IF(dia = 7, 1, 0)) * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS sabado, SUM(IF(dia = 1, 1, 0)) * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesdeSF & "' AND fecha <= '" & fHastaSF & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & filtroMTBF_are) & " GROUP BY a.id, a.nombre " & tHaving & " ORDER BY " & ordenDatos & ";"
                'End If
            ElseIf idReporte = 4 Or idReporte = 17 Then
                'If Not id04 Then
                ' id04 = True
                ' Leer = True
                ' End If
                '    If Leer Then
                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(86) & Chr(34) & "," & Chr(34) & traduccion(85) & Chr(34) & "," & Chr(34) & traduccion(145) & Chr(34) & "," & Chr(34) & traduccion(113) & Chr(34) & "," & Chr(34) & traduccion(116) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(146) & Chr(34) & "," & Chr(34) & traduccion(114) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, a.id, a.nombre, e.tdias, (e.lunes + e.martes + e.miercoles + e.jueves + e.viernes + e.sabado + e.domingo) AS tdisponible, (e.lunes + e.martes + e.miercoles + e.jueves + e.viernes + e.sabado + e.domingo) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_fallas a " & IIf(modulo = 1, "INNER", "LEFT") & " JOIN (SELECT falla_ajustada, linea, maquina, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  " & filtroTiempo & filtroReportes & " GROUP BY falla_ajustada, linea, maquina) AS c ON a.id = c.falla_ajustada, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS lunes, SUM(IF(dia = 3, 1, 0)) * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS martes, SUM(IF(dia = 4, 1, 0)) * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS miercoles, SUM(IF(dia = 5, 1, 0)) * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS jueves, SUM(IF(dia = 6, 1, 0)) * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS viernes, SUM(IF(dia = 7, 1, 0)) * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS sabado, SUM(IF(dia = 1, 1, 0)) * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesdeSF & "' AND fecha <= '" & fHastaSF & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & filtroMTBF_fal) & " GROUP BY a.id, a.nombre " & tHaving & " ORDER BY " & ordenDatos & ";"
                'End If
            ElseIf idReporte = 5 Or idReporte = 18 Then
                'If Not id05 Then
                ' id05 = True
                ' Leer = True
                ' End If
                '     If Leer Then
                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(151) & Chr(34) & "," & Chr(34) & traduccion(46) & Chr(34) & "," & Chr(34) & traduccion(145) & Chr(34) & "," & Chr(34) & traduccion(113) & Chr(34) & "," & Chr(34) & traduccion(116) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(146) & Chr(34) & "," & Chr(34) & traduccion(114) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)
                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, a.tipo AS id, IFNULL(b.nombre, '" & traduccion(100) & "') AS nombre, COUNT(*), e.tdias, SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, (SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0))) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_maquinas a " & IIf(modulo = 1, "INNER", "LEFT") & " JOIN (SELECT maquina, linea, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  " & filtroTiempo & filtroReportes & " GROUP BY maquina, linea) AS c ON a.id = c.maquina LEFT JOIN " & rutaBD & ".cat_generales b ON a.tipo = b.id AND b.tabla = 50, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesdeSF & "' AND fecha <= '" & fHastaSF & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & filtroMTBF) & " GROUP BY a.tipo, nombre " & tHaving & " ORDER BY " & ordenDatos & ";"
                'End If
            ElseIf idReporte = 6 Or idReporte = 19 Then
                'If Not id06 Then
                ' id06 = True
                ' Leer = True
                ' End If
                '     If Leer Then
                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(151) & Chr(34) & "," & Chr(34) & traduccion(47) & Chr(34) & "," & Chr(34) & traduccion(145) & Chr(34) & "," & Chr(34) & traduccion(113) & Chr(34) & "," & Chr(34) & traduccion(116) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(146) & Chr(34) & "," & Chr(34) & traduccion(114) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)
                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, a.agrupador_1 AS id, IFNULL(b.nombre, '" & traduccion(100) & "') AS nombre, COUNT(*), e.tdias, SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, (SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0))) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_maquinas a " & IIf(modulo = 1, "INNER", "LEFT") & " JOIN (SELECT maquina, linea, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  " & filtroTiempo & filtroReportes & " GROUP BY maquina, linea) AS c ON a.id = c.maquina LEFT JOIN " & rutaBD & ".cat_generales b ON a.agrupador_1 = b.id AND b.tabla = 20, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesdeSF & "' AND fecha <= '" & fHastaSF & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & filtroMTBF) & " GROUP BY a.agrupador_1, nombre " & tHaving & " ORDER BY " & ordenDatos & ";"
                '    End If
            ElseIf idReporte = 7 Or idReporte = 20 Then
                '   If Not id07 Then
                '   id07 = True
                '   Leer = True
                ' End If
                '     If Leer Then
                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(151) & Chr(34) & "," & Chr(34) & traduccion(48) & Chr(34) & "," & Chr(34) & traduccion(145) & Chr(34) & "," & Chr(34) & traduccion(113) & Chr(34) & "," & Chr(34) & traduccion(116) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(146) & Chr(34) & "," & Chr(34) & traduccion(114) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, a.agrupador_2 AS id, IFNULL(b.nombre, '" & traduccion(100) & "') AS nombre, COUNT(*), e.tdias, SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, (SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0))) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_maquinas a " & IIf(modulo = 1, "INNER", "LEFT") & " JOIN (SELECT maquina, linea, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  " & filtroTiempo & filtroReportes & " GROUP BY maquina, linea) AS c ON a.id = c.maquina LEFT JOIN " & rutaBD & ".cat_generales b ON a.agrupador_2 = b.id AND b.tabla = 25, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesdeSF & "' AND fecha <= '" & fHastaSF & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & filtroMTBF) & " GROUP BY a.agrupador_2, nombre " & tHaving & " ORDER BY " & ordenDatos & ";"

                '    End If
            ElseIf idReporte = 8 Or idReporte = 21 Then
                '   If Not id08 Then
                '   id08 = True
                '   Leer = True
                ' End If
                '     If Leer Then
                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(151) & Chr(34) & "," & Chr(34) & traduccion(49) & Chr(34) & "," & Chr(34) & traduccion(145) & Chr(34) & "," & Chr(34) & traduccion(113) & Chr(34) & "," & Chr(34) & traduccion(116) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(146) & Chr(34) & "," & Chr(34) & traduccion(114) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, a.agrupador_1 AS id, IFNULL(b.nombre, '" & traduccion(100) & "') AS nombre, COUNT(*), e.tdias, SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, (SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0))) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_fallas a " & IIf(modulo = 1, "INNER", "LEFT") & " JOIN (SELECT falla_ajustada, maquina, linea, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  " & filtroTiempo & filtroReportes & " GROUP BY falla_ajustada, maquina, linea) AS c ON a.id = c.falla_ajustada LEFT JOIN " & rutaBD & ".cat_generales b ON a.agrupador_1 = b.id AND b.tabla = 40, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesdeSF & "' AND fecha <= '" & fHastaSF & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & filtroMTBF_fal) & " GROUP BY a.agrupador_1, nombre " & tHaving & " ORDER BY " & ordenDatos & ";"

                'End If
            ElseIf idReporte = 9 Or idReporte = 22 Then
                'If Not id09 Then
                ' id09 = True
                ' Leer = True
                ' End If
                '     If Leer Then
                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(151) & Chr(34) & "," & Chr(34) & traduccion(50) & Chr(34) & "," & Chr(34) & traduccion(145) & Chr(34) & "," & Chr(34) & traduccion(113) & Chr(34) & "," & Chr(34) & traduccion(116) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(146) & Chr(34) & "," & Chr(34) & traduccion(114) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, a.agrupador_2 AS id, IFNULL(b.nombre, '" & traduccion(100) & "') AS nombre, COUNT(*), e.tdias, SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, (SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0))) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_fallas a " & IIf(modulo = 1, "INNER", "LEFT") & " JOIN (SELECT falla_ajustada, maquina, linea, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  " & filtroTiempo & filtroReportes & " GROUP BY falla_ajustada, maquina, linea) AS c ON a.id = c.falla_ajustada LEFT JOIN " & rutaBD & ".cat_generales b ON a.agrupador_2 = b.id AND b.tabla = 45, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesdeSF & "' AND fecha <= '" & fHastaSF & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & filtroMTBF_fal) & " GROUP BY a.agrupador_2, nombre " & tHaving & " ORDER BY " & ordenDatos & ";"
                'End If
            ElseIf idReporte = 10 Or idReporte = 23 Then
                'If Not id10 Then
                ' id10 = True
                ' Leer = True
                ' End If
                '     If Leer Then
                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(51) & Chr(34) & "," & Chr(34) & traduccion(113) & Chr(34) & "," & Chr(34) & traduccion(116) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(146) & Chr(34) & "," & Chr(34) & traduccion(114) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, 0 AS id, DATE_FORMAT(a.fecha, '%Y/%m/%d') AS nombre, 0 AS tdias, IFNULL(IF(a.dia = 1, g.domingo, IF(a.dia = 2, g.lunes, IF(a.dia = 3, g.martes, IF(a.dia = 4, g.miercoles, IF(a.dia = 5, g.jueves, IF(a.dia = 6, g.viernes, g.sabado)))))), 0) AS tdisponible, IFNULL(IF(a.dia = 1, g.domingo, IF(a.dia = 2, g.lunes, IF(a.dia = 3, g.martes, IF(a.dia = 4, g.miercoles, IF(a.dia = 5, g.jueves, IF(a.dia = 6, g.viernes, g.sabado)))))), 0) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".dias a " & IIf(modulo = 1, "INNER", "LEFT") & " JOIN (SELECT fecha_reporte, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  " & filtroTiempo & filtroReportes & " GROUP BY fecha_reporte) AS c ON a.fecha = c.fecha_reporte, (SELECT * FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND maquina = 0 AND linea = 0 LIMIT 1) AS g WHERE  fecha >= '" & fDesdeSF & "' AND fecha <= '" & fHastaSF & "' GROUP BY 6 " & tHaving & " ORDER BY " & ordenDatos & ";"
                'End If

            ElseIf idReporte = 11 Or idReporte = 24 Then
                'If Not id11 Then
                ' id11 = True
                ' Leer = True
                ' End If
                '     If Leer Then
                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(52) & Chr(34) & "," & Chr(34) & traduccion(53) & Chr(34) & "," & Chr(34) & traduccion(145) & Chr(34) & "," & Chr(34) & traduccion(113) & Chr(34) & "," & Chr(34) & traduccion(116) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(146) & Chr(34) & "," & Chr(34) & traduccion(114) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, 0 AS id, CONCAT(YEAR(a.fecha), '/', WEEK(a.fecha)) AS nombre, e.tdias, STR_TO_DATE(CONCAT(DATE_FORMAT(a.fecha,'%x/%v'), ' Monday'), '%x/%v %W') AS inicio, e.tdias, SUM(IFNULL(IF(a.dia = 1, g.domingo, IF(a.dia = 2, g.lunes, IF(a.dia = 3, g.martes, IF(a.dia = 4, g.miercoles, IF(a.dia = 5, g.jueves, IF(a.dia = 6, g.viernes, g.sabado)))))), 0)) AS tdisponible, SUM(IFNULL(IF(a.dia = 1, g.domingo, IF(a.dia = 2, g.lunes, IF(a.dia = 3, g.martes, IF(a.dia = 4, g.miercoles, IF(a.dia = 5, g.jueves, IF(a.dia = 6, g.viernes, g.sabado)))))), 0)) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".dias a " & IIf(modulo = 1, "INNER", "LEFT") & " JOIN (SELECT fecha_reporte, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  " & filtroTiempo & filtroReportes & " GROUP BY fecha_reporte) AS c ON a.fecha = c.fecha_reporte, (SELECT * FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND maquina = 0 AND linea = 0 LIMIT 1) AS g, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesdeSF & "' AND fecha <= '" & fHastaSF & "' ) AS e WHERE fecha >= '" & fDesdeSF & "' AND fecha <= '" & fHastaSF & "' GROUP BY 6 " & tHaving & " ORDER BY " & ordenDatos & ";"
                'End If
            ElseIf idReporte = 12 Or idReporte = 25 Then
                'If Not id12 Then
                ' id12 = True
                ' Leer = True
                ' End If
                '     If Leer Then
                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(54) & Chr(34) & "," & Chr(34) & traduccion(145) & Chr(34) & "," & Chr(34) & traduccion(113) & Chr(34) & "," & Chr(34) & traduccion(116) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(146) & Chr(34) & "," & Chr(34) & traduccion(114) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, 0 AS id, CONCAT(YEAR(a.fecha), '/', MONTH(a.fecha)) AS nombre, e.tdias, SUM(IFNULL(IF(a.dia = 1, g.domingo, IF(a.dia = 2, g.lunes, IF(a.dia = 3, g.martes, IF(a.dia = 4, g.miercoles, IF(a.dia = 5, g.jueves, IF(a.dia = 6, g.viernes, g.sabado)))))), 0)) AS tdisponible, SUM(IFNULL(IF(a.dia = 1, g.domingo, IF(a.dia = 2, g.lunes, IF(a.dia = 3, g.martes, IF(a.dia = 4, g.miercoles, IF(a.dia = 5, g.jueves, IF(a.dia = 6, g.viernes, g.sabado)))))), 0)) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".dias a " & IIf(modulo = 1, "INNER", "LEFT") & " JOIN (SELECT fecha_reporte, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  " & filtroTiempo & filtroReportes & " GROUP BY fecha_reporte) AS c ON a.fecha = c.fecha_reporte, (SELECT * FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND maquina = 0 AND linea = 0 LIMIT 1) AS g, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesdeSF & "' AND fecha <= '" & fHastaSF & "' ) AS e WHERE  fecha >= '" & fDesdeSF & "' AND fecha <= '" & fHastaSF & "' GROUP BY 6 " & tHaving & " ORDER BY " & ordenDatos & ";"

                '    End If
            ElseIf idReporte = 13 Or idReporte = 26 Then
                '   If Not id13 Then
                '   id13 = True
                '   Leer = True
                ' End If
                '     If Leer Then
                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(151) & Chr(34) & "," & Chr(34) & traduccion(55) & Chr(34) & "," & Chr(34) & traduccion(145) & Chr(34) & "," & Chr(34) & traduccion(113) & Chr(34) & "," & Chr(34) & traduccion(116) & Chr(34) & "," & Chr(34) & traduccion(105) & Chr(34) & "," & Chr(34) & traduccion(146) & Chr(34) & "," & Chr(34) & traduccion(114) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

                    sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, a.id, a.nombre, e.tdias, (e.lunes + e.martes + e.miercoles + e.jueves + e.viernes + e.sabado + e.domingo) AS tdisponible, (e.lunes + e.martes + e.miercoles + e.jueves + e.viernes + e.sabado + e.domingo) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_usuarios a " & IIf(modulo = 1, "INNER", "LEFT") & " JOIN (SELECT tecnicoatend, maquina, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  " & filtroTiempo & filtroReportes & " GROUP BY tecnicoatend, maquina) AS c ON a.id = c.tecnicoatend, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS lunes, SUM(IF(dia = 3, 1, 0)) * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS martes, SUM(IF(dia = 4, 1, 0)) * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS miercoles, SUM(IF(dia = 5, 1, 0)) * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS jueves, SUM(IF(dia = 6, 1, 0)) * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS viernes, SUM(IF(dia = 7, 1, 0)) * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS sabado, SUM(IF(dia = 1, 1, 0)) * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesdeSF & "' AND fecha <= '" & fHastaSF & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & filtroMTBF_tec) & " GROUP BY a.id, a.nombre " & tHaving & " ORDER BY " & ordenDatos & ";"
                '    End If

            End If

            mensajesDS = consultaSEL(sentencia)
            adicional = ""
            If mensajesDS.Tables(0).Rows.Count > 0 Then
                Dim view_o As DataView = New DataView(mensajesDS.Tables(0))
                Dim objWriter As New System.IO.StreamWriter(archivoSaliente, False, System.Text.Encoding.UTF8)
                objWriter.WriteLine(inicial)
                objWriter.WriteLine(cabecera)
                Dim totalPareto As Double = 0


                If config.Tables(0).Rows(0)!maximo_barras > 0 And config.Tables(0).Rows(0)!maximo_barras < mensajesDS.Tables(0).Rows.Count Or config.Tables(0).Rows(0)!maximo_barraspct > 0 And config.Tables(0).Rows(0)!maximo_barraspct < 100 Then
                    'Se calcula el total del Pareto
                    Dim limitar = 0
                    Dim agrupado = ""
                    Dim pcAcum = 0
                    If config.Tables(0).Rows(0)!maximo_barraspct > 0 And config.Tables(0).Rows(0)!maximo_barraspct < 100 Then

                        For Each elmensaje As DataRow In mensajesDS.Tables(0).Rows
                            totalPareto = totalPareto + elmensaje.Item(campoSumar)
                        Next
                        Dim pct = config.Tables(0).Rows(0)!maximo_barraspct / 100
                        Dim i = 0
                        For Each elmensaje In mensajesDS.Tables(0).Rows
                            i = i + 1
                            pcAcum = pcAcum + elmensaje.Item(campoSumar)
                            If pcAcum / totalPareto >= pct Then
                                limitar = i
                                Exit For
                            End If
                        Next
                    End If
                    If config.Tables(0).Rows(0)!maximo_barras > 0 Then
                        If limitar > config.Tables(0).Rows(0)!maximo_barras Or limitar = 0 Then
                            limitar = config.Tables(0).Rows(0)!maximo_barras
                        End If
                    End If

                    If limitar + 1 >= mensajesDS.Tables(0).Rows.Count And config.Tables(0).Rows(0)!agrupar = "S" Then
                        limitar = 0
                    ElseIf limitar >= mensajesDS.Tables(0).Rows.Count Then
                        limitar = 0
                    End If
                    If limitar > 0 Then
                        For j = 0 To limitar - 1
                            mensajesDS.Tables(0).Rows(j)!orden = j + 1
                        Next
                        If config.Tables(0).Rows(0)!agrupar = "S" Then
                            'mttr y mtbf
                            Dim faltante1 = 0
                            Dim faltante2 = 0
                            Dim faltante3 = 0
                            Dim faltante4 = mensajesDS.Tables(0).Rows(limitar)!tdias
                            Dim faltante5 = 0
                            Dim totalAgr = 0
                            For j = limitar To mensajesDS.Tables(0).Rows.Count - 1
                                mensajesDS.Tables(0).Rows(j)!filtro = 0
                                faltante1 = faltante1 + mensajesDS.Tables(0).Rows(j)!tdisponible
                                faltante2 = faltante2 + mensajesDS.Tables(0).Rows(j)!docs
                                faltante3 = faltante3 + mensajesDS.Tables(0).Rows(j)!tiempo_c
                            Next
                            totalAgr = mensajesDS.Tables(0).Rows.Count - limitar
                            Dim row As DataRow = mensajesDS.Tables(0).NewRow

                            row("id") = "0"
                            row("nombre") = IIf(ValNull(config.Tables(0).Rows(0)!agrupar_texto, "A") = "", traduccion(61), config.Tables(0).Rows(0)!agrupar_texto) & " (" & totalAgr & ")"
                            row("tdias") = faltante4
                            row("tdisponible") = faltante1
                            row("docs") = faltante2
                            row("tiempo_c") = faltante3
                            row("mttrc") = faltante3 / faltante2 / 3600
                            row("mtbfc") = faltante1 / (faltante2 + 1) / 3600
                            row("porcentaje") = 0
                            row("filtro") = 1
                            row("pareto") = 1
                            If config.Tables(0).Rows(0)!agrupar_posicion = "F" Then
                                row("orden") = limitar + 1
                            ElseIf config.Tables(0).Rows(0)!agrupar_posicion = "P" Then
                                row("orden") = 0
                            End If
                            mensajesDS.Tables(0).Rows.Add(row)

                        Else
                            adicional = traduccion(147)
                        End If
                    End If
                    If config.Tables(0).Rows(0)!agrupar_posicion = "N" Then
                        view_o.Sort = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " " & campoSumar & " DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " " & campoSumar, "nombre"))
                    Else
                        view_o.Sort = "orden ASC"
                    End If
                Else
                    view_o.Sort = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " " & campoSumar & " DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " " & campoSumar, "nombre"))
                End If
                view_o.RowFilter = "filtro = 1"
                totalPareto = 0
                For Each registro In mensajesDS.Tables(0).Rows
                    If registro!filtro = 1 Then
                        totalPareto = totalPareto + registro.Item(campoSumar)
                    End If
                Next
                Dim acumPareto As Double = 0
                Dim linea = 0

                tablaGRafico = view_o.ToTable()
                Dim indice = 0
                For Each registro In agregados.Rows
                    Dim row As DataRow = tablaGRafico.NewRow
                    row("id") = "0"
                    row("nombre") = registro!nombre
                    row("tdias") = registro!tdias
                    row("tdisponible") = registro!tdisponible
                    row("docs") = registro!docs
                    row("tiempo_c") = registro!tiempo_c
                    row("mttrc") = registro!mttrc
                    row("mtbfc") = registro!mtbfc
                    row("porcentaje") = registro!porcentaje
                    row("pareto") = 0
                    row("filtro") = 1
                    tablaGRafico.Rows.InsertAt(row, indice)
                    indice = indice + 1
                Next
                For Each registro In tablaGRafico.Rows
                    If registro!filtro = 1 Then
                        linea = linea + 1
                        If registro!pareto = 1 Then
                            acumPareto = acumPareto + registro.Item(campoSumar) / totalPareto * 100
                        End If
                        registro!porcentaje = acumPareto
                        If linea = mensajesDS.Tables(0).Rows.Count Then
                            acumPareto = 100
                        End If
                        Dim cadInicio = ""
                        If miReporte = 11 Then
                            cadInicio = Chr(34) & registro!inicio & Chr(34) & ","
                        End If
                        Dim cadID = ""
                        If miReporte < 10 Then
                            cadID = Chr(34) & registro!id & Chr(34) & ","
                        End If
                        Dim cadTDias = ""
                        If miReporte <> 10 Then
                            cadTDias = Chr(34) & registro!tdias & Chr(34) & ","
                        End If
                        objWriter.WriteLine(Chr(34) & linea & Chr(34) & "," & cadID & Chr(34) & registro!nombre & Chr(34) & "," & cadInicio & cadTDias & Chr(34) & registro!tdisponible & Chr(34) & "," & Chr(34) & registro!mtbfc & Chr(34) & "," & Chr(34) & registro!docs & Chr(34) & "," & Chr(34) & registro!tiempo_c & Chr(34) & "," & Chr(34) & registro!mttrc & Chr(34) & "," & Chr(34) & IIf(registro!pareto = 1, registro.Item(campoSumar) / totalPareto * 100, "") & Chr(34) & "," & Chr(34) & IIf(registro!pareto = 1, acumPareto, "") & Chr(34))
                    End If
                Next
                objWriter.WriteLine(traduccion(121) & ": " & linea)
                If adicional.Length > 0 Then objWriter.WriteLine(adicional)
                objWriter.WriteLine(traduccion(122))
                objWriter.Close()

                If graficar = "S" Then


                    Dim indicador01 = ""
                    Dim indicador02 = ""

                    Dim titulosSeries = config.Tables(0).Rows(0)!textos_adicionales.Split(New Char() {";"c})
                    If titulosSeries.length = 1 Then
                        indicador01 = titulosSeries(0)
                    End If
                    If titulosSeries.length = 2 Then
                        indicador01 = titulosSeries(0)
                        indicador02 = titulosSeries(1)
                    End If

                    If indicador01.Length = 0 Then indicador01 = IIf(modulo = 1, traduccion(64), traduccion(65))
                    If indicador02.Length = 0 Then indicador02 = traduccion(107)


                    ChartControl1.Series.Clear()
                    ChartControl1.Titles.Clear()
                    Dim Titulo As New ChartTitle()
                    Titulo.Text = config.Tables(0).Rows(0)!titulo & Strings.Space(10)
                    Dim miFuente = New Drawing.Font("Lucida Sans", 10, FontStyle.Regular)
                    Dim miFuenteAlto = New Drawing.Font("Lucida Sans", 16, FontStyle.Bold)
                    Dim miFuenteEjes = New Drawing.Font("Lucida Sans", 11, FontStyle.Regular)

                    Titulo.Font = miFuenteAlto
                    Dim series1 As New Series(indicador01, ViewType.Bar)

                    ChartControl1.Series.Add(series1)
                    series1.DataSource = tablaGRafico

                    series1.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                    series1.View.Color = Color.SkyBlue
                    series1.ArgumentScaleType = ScaleType.Qualitative
                    series1.ArgumentDataMember = "nombre"
                    series1.ValueScaleType = ScaleType.Numerical
                    series1.ValueDataMembers.AddRange(New String() {campoSumar})
                    series1.Label.BackColor = Color.DarkBlue
                    series1.Label.TextColor = Color.White
                    series1.Label.Font = miFuente
                    series1.Label.TextPattern = "{V:F" & config.Tables(0).Rows(0)!etiqueta_formato & "}"

                    'Obtener el titulo en Y
                    Dim tituloY As String = ""
                    Dim titulosY = ValNull(config.Tables(0).Rows(0)!texto_y, "A")
                    Try

                        Dim titulosYArreglo = titulosY.Split(New Char() {";"c})
                        If titulosYArreglo.length = 1 Then
                            tituloY = titulosYArreglo(0)
                        Else
                            tituloY = titulosYArreglo(ordenPareto)
                        End If
                    Catch ex As Exception
                        tituloY = titulosY
                    End Try
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Visibility = DevExpress.Utils.DefaultBoolean.True
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Label.Font = miFuenteEjes
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.GridSpacingAuto = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.GridSpacing = 1
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Text = tituloY
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Font = miFuenteAlto
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.GridLines.Visible = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Tickmarks.Visible = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Tickmarks.MinorVisible = False

                    AddHandler Me.ChartControl1.CustomDrawSeriesPoint, AddressOf Me.ValidarPuntos
                    If config.Tables(0).Rows(0)!grueso_spiline > 0 Then
                        Dim view_2 As DataView = New DataView(tablaGRafico)
                        view_2.RowFilter = "pareto = 1"
                        Dim tablaPareto As DataTable = view_2.ToTable()
                        Dim series2 As New Series(indicador02, ViewType.Spline)

                        ChartControl1.Series.Add(series2)
                        series2.DataSource = tablaPareto
                        series2.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                        series2.View.Color = Color.Green

                        series2.ArgumentScaleType = ScaleType.Qualitative
                        series2.ArgumentDataMember = "nombre"
                        series2.ValueScaleType = ScaleType.Numerical
                        series2.ValueDataMembers.AddRange(New String() {"porcentaje"})
                        series2.Label.BackColor = Color.DarkBlue
                        series2.Label.TextColor = Color.White
                        series2.Label.Font = miFuente
                        series2.Label.TextPattern = "{V:F0}"

                        CType(series2.View, SplineSeriesView).LineStyle.Thickness = config.Tables(0).Rows(0)!grueso_spiline
                        Dim myAxisY As New SecondaryAxisY(traduccion(63))
                        CType(ChartControl1.Diagram, XYDiagram).SecondaryAxesY.Clear()
                        CType(ChartControl1.Diagram, XYDiagram).SecondaryAxesY.Add(myAxisY)
                        CType(series2.View, LineSeriesView).AxisY = myAxisY
                        myAxisY.Title.Text = config.Tables(0).Rows(0)!texto_z
                        myAxisY.Title.Visible = True
                        myAxisY.Label.Font = miFuenteEjes
                        myAxisY.Title.Font = miFuenteAlto
                        myAxisY.GridLines.Visible = False
                        myAxisY.Tickmarks.Visible = False
                        myAxisY.Tickmarks.MinorVisible = False
                        myAxisY.Title.TextColor = Color.Green
                        myAxisY.Label.TextColor = Color.Green
                        myAxisY.Color = Color.Green
                    End If
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.GridLines.Visible = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.Font = miFuenteEjes
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Text = Strings.Space(5) & config.Tables(0).Rows(0)!texto_x & Strings.Space(10)
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.GridLines.Visible = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Font = miFuenteAlto
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Visibility = DevExpress.Utils.DefaultBoolean.True
                    If config.Tables(0).Rows(0)!overlap = "R" Then
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowStagger = False
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowRotate = True
                    Else
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowStagger = True
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowRotate = False
                    End If


                    ChartControl1.Titles.Add(Titulo)
                    Dim Titulo2 As New ChartTitle()

                    Titulo2.Font = miFuente
                    Titulo2.Text = traduccion(56) & cadPeriodo
                    ChartControl1.Titles.Add(Titulo2)
                    Dim Titulo3 As New ChartTitle()
                    Titulo3.Font = miFuente
                    Titulo3.Text = traduccion(57) & Format(Now, "ddd dd-MMM-yyyy HH:mm:ss")
                    ChartControl1.Titles.Add(Titulo3)
                    Dim Titulo4 As New ChartTitle()
                    Titulo4.Font = miFuente
                    Titulo4.Text = traduccion(58) & Format(eDesde, "dd-MMM-yyyy HH:mm:ss") & traduccion(59) &
                                                            Format(eHasta, "dd-MMM-yyyy HH:mm:ss")
                    ChartControl1.Titles.Add(Titulo4)
                    ChartControl1.Width = 1200
                    ChartControl1.Height = 800

                    If config.Tables(0).Rows(0)!ver_leyenda = "S" Then
                        ChartControl1.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True
                    Else
                        ChartControl1.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False
                    End If
                    Try
                        Dim rutaImagen = Microsoft.VisualBasic.Strings.Replace(archivoImagen, "\", "\\")
                        SaveChartImageToFile(ChartControl1, ImageFormat.Png, rutaImagen)
                        Dim image As Image = GetChartImage(ChartControl1, ImageFormat.Png)
                        image.Save(rutaImagen)

                    Catch ex As Exception
                        agregarLOG(traduccion(60) & ex.Message, 7, 0)
                    End Try
                End If
            End If
        End If
    End Function

    Function generarReporteTipo5(idReporte As Integer, reporte As String, fName As String, ruta As String, graficar As String, consulta As Long, modulo As Integer, grafica As Long) As Integer
        generarReporteTipo5 = 0

        Dim archivoSaliente = ruta & "\" & fName & ".csv"
        Dim archivoImagen = ruta & "\" & fName & ".png"

        Try
            My.Computer.FileSystem.DeleteFile(archivoSaliente)


        Catch ex As Exception

        End Try

        Try
            My.Computer.FileSystem.DeleteFile(archivoImagen)


        Catch ex As Exception

        End Try

        archivoSaliente = archivoSaliente.Replace("\", "\\")


        Dim comillas = Microsoft.VisualBasic.Strings.Left(Chr(34), 1)

        Dim inicial = ""
        Dim cabecera = ""
        Dim registros = ""

        Dim Leer As Boolean = False
        Dim cadSQL = ""
        Dim miReporte = idReporte - 200

        cadSQL = "SELECT * FROM " & rutaBD & ".pu_graficos WHERE (usuario = 1 OR usuario = 0) AND grafico = " & grafica & " AND idioma = " & IIf(be_idioma = 0, 1, be_idioma) & " ORDER BY usuario DESC LIMIT 1"

        Dim config As DataSet = consultaSEL(cadSQL)
        If config.Tables(0).Rows.Count > 0 Then
            Dim ordenPareto = ValNull(config.Tables(0).Rows(0)!orden, "N")

            If consulta > 0 Then generarFIltro(consulta)

            Dim mensajesDS As DataSet
            Dim sentencia As String = ""
            Dim adicional = ""
            Dim regsAfectados = 0
            Dim tHaving = ""
            Dim ordenDatos = " 9 "
            Dim campoSumar = "tiempo"

            inicial = Chr(34) & reporte & " (" & cadPeriodo & ")" & Chr(34) & vbCrLf
            inicial = inicial & Chr(34) & traduccion(110) & ": " & Format(Now(), "ddd, dd-MMM-yyyy HH:mm:ss") & Chr(34) & vbCrLf
            inicial = inicial & Chr(34) & traduccion(111) & ": " & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & " " & traduccion(112) & ": " & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & Chr(34) & vbCrLf

            If config.Tables(0).Rows(0)!incluir_ceros = "N" Then

                tHaving = " HAVING docs > 0 "
            End If
            If config.Tables(0).Rows(0)!orden = "1" Then
                campoSumar = "docs"
                ordenDatos = "8"

            End If
            If config.Tables(0).Rows(0)!orden_grafica = "M" Then
                ordenDatos = ordenDatos & " DESC"
            ElseIf config.Tables(0).Rows(0)!orden_grafica = "A" Then
                ordenDatos = " 5 "
            End If

            Dim filtroTiempo = " i.dia >= '" & fDesdeSF & "' AND i.dia <= '" & fHastaSF & "' "
            If sDesde <> "-1" Then
                filtroTiempo = " i.sec " & sDesde & " "
            End If

            sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, IFNULL(b.nombre, '" & traduccion(100) & "') AS nombre, b.referencia, a.linea AS id, IFNULL(SUM(i.rechazos), 0) AS docs, IFNULL(SUM(i.calidad), 0) / 3600 AS tiempo FROM (SELECT i.equipo, SUM(i.calidad) AS rechazos, SUM(i.calidad_tc) AS calidad FROM (SELECT a.equipo, a.area, a.tipo, a.fecha AS dia, a.secuencia_turno AS sec, a.parte, a.turno, a.lote AS orden, a.cantidad AS calidad, a.cantidad_tc AS calidad_tc FROM " & rutaBD & ".detallerechazos a INNER JOIN " & rutaBD & ".lecturas_cortes b ON a.corte = b.id UNION ALL SELECT equipo, 0 AS area, 0 AS tipo, dia, turno_secuencia AS sec, parte, turno, orden, calidad, calidad_tc FROM " & rutaBD & ".lecturas_cortes WHERE calidad - calidad_clasificada > 0 UNION ALL SELECT equipo, area, tipo, fecha AS dia, secuencia_turno AS sec, parte, turno, lote AS orden, cantidad AS calidad, cantidad_tc AS calidad_tc FROM " & rutaBD & ".detallerechazos WHERE corte = 0) AS i LEFT JOIN sigma.cat_maquinas j ON i.equipo = j.id WHERE" & filtroTiempo & filtroRechazos & " GROUP BY i.equipo) AS i LEFT JOIN " & rutaBD & ".cat_maquinas a ON i.equipo = a.id AND a.oee = 'S' LEFT JOIN " & rutaBD & ".cat_lineas b ON a.linea = b.id GROUP BY nombre " & tHaving & " ORDER BY " & ordenDatos

            If miReporte = 2 Then

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, IFNULL(a.nombre, '" & traduccion(100) & "') AS nombre, a.referencia, a.id, IFNULL(SUM(i.rechazos), 0) AS docs, IFNULL(SUM(i.calidad), 0) / 3600 AS tiempo FROM (SELECT i.equipo, SUM(i.calidad) AS rechazos, SUM(i.calidad_tc) AS calidad FROM (SELECT a.equipo, a.area, a.tipo, a.fecha AS dia, a.secuencia_turno AS sec, a.parte, a.turno, a.lote AS orden, a.cantidad AS calidad, a.cantidad_tc AS calidad_tc FROM " & rutaBD & ".detallerechazos a INNER JOIN " & rutaBD & ".lecturas_cortes b ON a.corte = b.id UNION ALL SELECT equipo, 0 AS area, 0 AS tipo, dia, turno_secuencia AS sec, parte, turno, orden, calidad, calidad_tc FROM " & rutaBD & ".lecturas_cortes WHERE calidad - calidad_clasificada > 0 UNION ALL SELECT equipo, area, tipo, fecha AS dia, secuencia_turno AS sec, parte, turno, lote AS orden, cantidad AS calidad, cantidad_tc AS calidad_tc FROM " & rutaBD & ".detallerechazos WHERE corte = 0) AS i LEFT JOIN sigma.cat_maquinas j ON i.equipo = j.id WHERE" & filtroTiempo & filtroRechazos & " GROUP BY i.equipo) AS i LEFT JOIN " & rutaBD & ".cat_maquinas a ON i.equipo = a.id WHERE a.oee = 'S' GROUP BY nombre " & tHaving & " ORDER BY " & ordenDatos


            ElseIf miReporte = 3 Then

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, IFNULL(a.nombre, '" & traduccion(100) & "') AS nombre, '' AS referencia, a.id, IFNULL(SUM(i.rechazos), 0) AS docs, IFNULL(SUM(i.calidad), 0) / 3600 AS tiempo FROM (SELECT i.area, SUM(i.calidad) AS rechazos, SUM(i.calidad_tc) AS calidad FROM (SELECT a.equipo, a.area, a.tipo, a.fecha AS dia, a.secuencia_turno AS sec, a.parte, a.turno, a.lote AS orden, a.cantidad AS calidad, a.cantidad_tc AS calidad_tc FROM " & rutaBD & ".detallerechazos a INNER JOIN " & rutaBD & ".lecturas_cortes b ON a.corte = b.id UNION ALL SELECT equipo, 0 AS area, 0 AS tipo, dia, turno_secuencia AS sec, parte, turno, orden, calidad, calidad_tc FROM " & rutaBD & ".lecturas_cortes WHERE calidad - calidad_clasificada > 0 UNION ALL SELECT equipo, area, tipo, fecha AS dia, secuencia_turno AS sec, parte, turno, lote AS orden, cantidad AS calidad, cantidad_tc AS calidad_tc FROM " & rutaBD & ".detallerechazos WHERE corte = 0) AS i LEFT JOIN sigma.cat_maquinas j ON i.equipo = j.id WHERE" & filtroTiempo & filtroRechazos & " GROUP BY i.area) AS i LEFT JOIN " & rutaBD & ".cat_generales a ON i.area = a.id GROUP BY nombre " & tHaving & " ORDER BY " & ordenDatos

                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(151) & Chr(34) & "," & Chr(34) & traduccion(170) & Chr(34) & "," & Chr(34) & traduccion(162) & Chr(34) & "," & Chr(34) & traduccion(171) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)


            ElseIf miReporte = 4 Then

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, IFNULL(a.nombre, '" & traduccion(168) & "') AS nombre, '' AS referencia, a.id, IFNULL(SUM(i.rechazos), 0) AS docs, IFNULL(SUM(i.calidad), 0) / 3600 AS tiempo FROM (SELECT i.tipo, SUM(i.calidad) AS rechazos, SUM(i.calidad_tc) AS calidad FROM (SELECT a.equipo, a.area, a.tipo, a.fecha AS dia, a.secuencia_turno AS sec, a.parte, a.turno, a.lote AS orden, a.cantidad AS calidad, a.cantidad_tc AS calidad_tc FROM " & rutaBD & ".detallerechazos a INNER JOIN " & rutaBD & ".lecturas_cortes b ON a.corte = b.id UNION ALL SELECT equipo, 0 AS area, 0 AS tipo, dia, turno_secuencia AS sec, parte, turno, orden, calidad, calidad_tc FROM " & rutaBD & ".lecturas_cortes WHERE calidad - calidad_clasificada > 0 UNION ALL SELECT equipo, area, tipo, fecha AS dia, secuencia_turno AS sec, parte, turno, lote AS orden, cantidad AS calidad, cantidad_tc AS calidad_tc FROM " & rutaBD & ".detallerechazos WHERE corte = 0) AS i LEFT JOIN sigma.cat_maquinas j ON i.equipo = j.id WHERE" & filtroTiempo & filtroRechazos & " GROUP BY i.tipo) AS i LEFT JOIN " & rutaBD & ".cat_generales a ON i.tipo = a.id GROUP BY nombre " & tHaving & " ORDER BY " & ordenDatos
                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(151) & Chr(34) & "," & Chr(34) & traduccion(169) & Chr(34) & "," & Chr(34) & traduccion(162) & Chr(34) & "," & Chr(34) & traduccion(171) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

            ElseIf miReporte = 5 Then
                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, DATE_FORMAT(i.dia, '%Y/%m/%d') AS nombre, '' AS referencia, 0 AS id, IFNULL(SUM(i.rechazos), 0) AS docs, IFNULL(SUM(i.calidad), 0) / 3600 AS tiempo FROM (SELECT i.dia, SUM(i.calidad) AS rechazos, SUM(i.calidad_tc) AS calidad FROM (SELECT a.equipo, a.area, a.tipo, a.fecha AS dia, a.secuencia_turno AS sec, a.parte, a.turno, a.lote AS orden, a.cantidad AS calidad, a.cantidad_tc AS calidad_tc FROM " & rutaBD & ".detallerechazos a INNER JOIN " & rutaBD & ".lecturas_cortes b ON a.corte = b.id UNION ALL SELECT equipo, 0 AS area, 0 AS tipo, dia, turno_secuencia AS sec, parte, turno, orden, calidad, calidad_tc FROM " & rutaBD & ".lecturas_cortes WHERE calidad - calidad_clasificada > 0 UNION ALL SELECT equipo, area, tipo, fecha AS dia, secuencia_turno AS sec, parte, turno, lote AS orden, cantidad AS calidad, cantidad_tc AS calidad_tc FROM " & rutaBD & ".detallerechazos WHERE corte = 0) AS i LEFT JOIN sigma.cat_maquinas j ON i.equipo = j.id WHERE" & filtroTiempo & filtroRechazos & " GROUP BY i.dia) AS i GROUP BY 5 ORDER BY " & ordenDatos


                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(51) & Chr(34) & "," & Chr(34) & traduccion(162) & Chr(34) & "," & Chr(34) & traduccion(171) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

            ElseIf miReporte = 6 Then

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, CONCAT(YEAR(i.dia), '/', WEEK(i.dia)) AS nombre, STR_TO_DATE(CONCAT(DATE_FORMAT(i.dia, '%x/%v'), ' Monday'), '%x/%v %W') AS inicio, 0 AS id, IFNULL(SUM(i.rechazos), 0) AS docs, IFNULL(SUM(i.calidad), 0) / 3600 AS tiempo FROM (SELECT i.dia, SUM(i.calidad) AS rechazos, SUM(i.calidad_tc) AS calidad FROM (SELECT a.equipo, a.area, a.tipo, a.fecha AS dia, a.secuencia_turno AS sec, a.parte, a.turno, a.lote AS orden, a.cantidad AS calidad, a.cantidad_tc AS calidad_tc FROM " & rutaBD & ".detallerechazos a INNER JOIN " & rutaBD & ".lecturas_cortes b ON a.corte = b.id UNION ALL SELECT equipo, 0 AS area, 0 AS tipo, dia, turno_secuencia AS sec, parte, turno, orden, calidad, calidad_tc FROM " & rutaBD & ".lecturas_cortes WHERE calidad - calidad_clasificada > 0 UNION ALL SELECT equipo, area, tipo, fecha AS dia, secuencia_turno AS sec, parte, turno, lote AS orden, cantidad AS calidad, cantidad_tc AS calidad_tc FROM " & rutaBD & ".detallerechazos WHERE corte = 0) AS i LEFT JOIN sigma.cat_maquinas j ON i.equipo = j.id WHERE" & filtroTiempo & filtroRechazos & " GROUP BY i.dia) AS i GROUP BY 5 ORDER BY " & ordenDatos


            ElseIf miReporte = 7 Then

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, CONCAT(YEAR(i.dia), '/', MONTH(i.dia)) AS nombre, '' AS referencia, 0 AS id, IFNULL(SUM(i.rechazos), 0) AS docs, IFNULL(SUM(i.calidad), 0) / 3600 AS tiempo FROM (SELECT i.dia, SUM(i.calidad) AS rechazos, SUM(i.calidad_tc) AS calidad FROM (SELECT a.equipo, a.area, a.tipo, a.fecha AS dia, a.secuencia_turno AS sec, a.parte, a.turno, a.lote AS orden, a.cantidad AS calidad, a.cantidad_tc AS calidad_tc FROM " & rutaBD & ".detallerechazos a INNER JOIN " & rutaBD & ".lecturas_cortes b ON a.corte = b.id UNION ALL SELECT equipo, 0 AS area, 0 AS tipo, dia, turno_secuencia AS sec, parte, turno, orden, calidad, calidad_tc FROM " & rutaBD & ".lecturas_cortes WHERE calidad - calidad_clasificada > 0 UNION ALL SELECT equipo, area, tipo, fecha AS dia, secuencia_turno AS sec, parte, turno, lote AS orden, cantidad AS calidad, cantidad_tc AS calidad_tc FROM " & rutaBD & ".detallerechazos WHERE corte = 0) AS i LEFT JOIN sigma.cat_maquinas j ON i.equipo = j.id WHERE" & filtroTiempo & filtroRechazos & " GROUP BY i.dia) AS i GROUP BY 5 ORDER BY " & ordenDatos

            End If
            mensajesDS = consultaSEL(sentencia)
            adicional = ""
            If mensajesDS.Tables(0).Rows.Count > 0 Then
                Dim view_o As DataView = New DataView(mensajesDS.Tables(0))
                Dim objWriter As New System.IO.StreamWriter(archivoSaliente, False, System.Text.Encoding.UTF8)
                objWriter.WriteLine(inicial)
                objWriter.WriteLine(cabecera)
                Dim totalPareto As Double = 0


                If config.Tables(0).Rows(0)!maximo_barras > 0 And config.Tables(0).Rows(0)!maximo_barras < mensajesDS.Tables(0).Rows.Count Or config.Tables(0).Rows(0)!maximo_barraspct > 0 And config.Tables(0).Rows(0)!maximo_barraspct < 100 Then
                    'Se calcula el total del Pareto
                    Dim limitar = 0
                    Dim agrupado = ""
                    Dim pcAcum = 0
                    If config.Tables(0).Rows(0)!maximo_barraspct > 0 And config.Tables(0).Rows(0)!maximo_barraspct < 100 Then

                        For Each elmensaje As DataRow In mensajesDS.Tables(0).Rows
                            totalPareto = totalPareto + elmensaje.Item(campoSumar)
                        Next
                        Dim pct = config.Tables(0).Rows(0)!maximo_barraspct / 100
                        Dim i = 0
                        For Each elmensaje In mensajesDS.Tables(0).Rows
                            i = i + 1
                            pcAcum = pcAcum + elmensaje.Item(campoSumar)
                            If pcAcum / totalPareto >= pct Then
                                limitar = i
                                Exit For
                            End If
                        Next
                    End If
                    If config.Tables(0).Rows(0)!maximo_barras > 0 Then
                        If limitar > config.Tables(0).Rows(0)!maximo_barras Or limitar = 0 Then
                            limitar = config.Tables(0).Rows(0)!maximo_barras
                        End If
                    End If

                    If limitar + 1 >= mensajesDS.Tables(0).Rows.Count And config.Tables(0).Rows(0)!agrupar = "S" Then
                        limitar = 0
                    ElseIf limitar >= mensajesDS.Tables(0).Rows.Count Then
                        limitar = 0
                    End If
                    If limitar > 0 Then
                        For j = 0 To limitar - 1
                            mensajesDS.Tables(0).Rows(j)!orden = j + 1
                        Next
                        If config.Tables(0).Rows(0)!agrupar = "S" Then
                            'mttr y mtbf
                            Dim faltante1 = 0
                            Dim faltante2 = 0
                            Dim faltante3 = 0
                            Dim faltante4 = 0
                            Dim faltante5 = 0
                            Dim totalAgr = 0
                            For j = limitar To mensajesDS.Tables(0).Rows.Count - 1
                                mensajesDS.Tables(0).Rows(j)!filtro = 0
                                faltante1 = faltante1 + mensajesDS.Tables(0).Rows(j)!docs
                                faltante2 = faltante2 + mensajesDS.Tables(0).Rows(j)!tiempo
                            Next
                            totalAgr = mensajesDS.Tables(0).Rows.Count - limitar
                            Dim row As DataRow = mensajesDS.Tables(0).NewRow

                            row("id") = "0"
                            row("nombre") = IIf(ValNull(config.Tables(0).Rows(0)!agrupar_texto, "A") = "", traduccion(61), config.Tables(0).Rows(0)!agrupar_texto) & " (" & totalAgr & ")"
                            row("docs") = faltante1
                            row("tiempo") = faltante2
                            row("porcentaje") = 0
                            row("filtro") = 1
                            row("pareto") = 1
                            If config.Tables(0).Rows(0)!agrupar_posicion = "F" Then
                                row("orden") = limitar + 1
                            ElseIf config.Tables(0).Rows(0)!agrupar_posicion = "P" Then
                                row("orden") = 0
                            End If
                            mensajesDS.Tables(0).Rows.Add(row)

                        Else
                            adicional = traduccion(147)
                        End If
                    End If
                    If config.Tables(0).Rows(0)!agrupar_posicion = "N" Then
                        view_o.Sort = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " " & campoSumar & " DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " " & campoSumar, "nombre"))
                    Else
                        view_o.Sort = "orden ASC"
                    End If
                Else
                    view_o.Sort = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " " & campoSumar & " DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " " & campoSumar, "nombre"))
                End If
                view_o.RowFilter = "filtro = 1"
                totalPareto = 0
                For Each registro In mensajesDS.Tables(0).Rows
                    If registro!filtro = 1 Then
                        totalPareto = totalPareto + registro.Item(campoSumar)
                    End If
                Next
                Dim acumPareto As Double = 0
                Dim linea = 0

                tablaGRafico = view_o.ToTable()
                For Each registro In tablaGRafico.Rows
                    If registro!filtro = 1 Then
                        linea = linea + 1
                        acumPareto = acumPareto + registro.Item(campoSumar) / totalPareto * 100
                        registro!porcentaje = acumPareto
                        If linea = mensajesDS.Tables(0).Rows.Count Then
                            acumPareto = 100
                        End If
                        Dim cadID = ""
                        If miReporte <> 5 And miReporte <> 6 And miReporte <> 7 Then
                            cadID = Chr(34) & registro!id & Chr(34) & ","
                        End If
                        Dim cadInicio = ""
                        If miReporte = 6 Then
                            cadInicio = Chr(34) & registro!inicio & Chr(34) & ","
                        End If
                        objWriter.WriteLine(Chr(34) & linea & Chr(34) & "," & cadID & Chr(34) & registro!nombre & Chr(34) & "," & cadInicio & Chr(34) & registro!docs & Chr(34) & "," & Chr(34) & registro!tiempo & Chr(34) & "," & Chr(34) & IIf(registro!pareto = 1, registro.Item(campoSumar) / totalPareto * 100, "") & Chr(34) & "," & Chr(34) & IIf(registro!pareto = 1, acumPareto, "") & Chr(34))
                    End If
                Next
                objWriter.WriteLine(traduccion(121) & ": " & linea)
                If adicional.Length > 0 Then objWriter.WriteLine(adicional)
                objWriter.WriteLine(traduccion(122))
                objWriter.Close()

                If graficar = "S" Then


                    Dim indicador01 = ""
                    Dim indicador02 = ""

                    Dim titulosSeries = config.Tables(0).Rows(0)!textos_adicionales.Split(New Char() {";"c})
                    If titulosSeries.length = 1 Then
                        indicador01 = titulosSeries(0)
                    End If
                    If titulosSeries.length = 2 Then
                        indicador01 = titulosSeries(0)
                        indicador02 = titulosSeries(1)
                    End If

                    If indicador01.Length = 0 Then indicador01 = traduccion(162)
                    If indicador02.Length = 0 Then indicador02 = traduccion(107)


                    ChartControl1.Series.Clear()
                    ChartControl1.Titles.Clear()
                    Dim Titulo As New ChartTitle()
                    Titulo.Text = config.Tables(0).Rows(0)!titulo & Strings.Space(10)
                    Dim miFuente = New Drawing.Font("Lucida Sans", 10, FontStyle.Regular)
                    Dim miFuenteAlto = New Drawing.Font("Lucida Sans", 16, FontStyle.Bold)
                    Dim miFuenteEjes = New Drawing.Font("Lucida Sans", 11, FontStyle.Regular)

                    Titulo.Font = miFuenteAlto
                    Dim series1 As New Series(indicador01, ViewType.Bar)

                    ChartControl1.Series.Add(series1)
                    series1.DataSource = tablaGRafico

                    series1.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                    series1.View.Color = Color.SkyBlue
                    series1.ArgumentScaleType = ScaleType.Qualitative
                    series1.ArgumentDataMember = "nombre"
                    series1.ValueScaleType = ScaleType.Numerical
                    series1.ValueDataMembers.AddRange(New String() {campoSumar})
                    series1.Label.BackColor = Color.DarkBlue
                    series1.Label.TextColor = Color.White
                    series1.Label.Font = miFuente
                    series1.Label.TextPattern = "{V:F" & config.Tables(0).Rows(0)!etiqueta_formato & "}"

                    'Obtener el titulo en Y
                    Dim tituloY As String = ""
                    Dim titulosY = ValNull(config.Tables(0).Rows(0)!texto_y, "A")
                    Try

                        Dim titulosYArreglo = titulosY.Split(New Char() {";"c})
                        If titulosYArreglo.length = 1 Then
                            tituloY = titulosYArreglo(0)
                        Else
                            tituloY = titulosYArreglo(ordenPareto)
                        End If
                    Catch ex As Exception
                        tituloY = titulosY
                    End Try
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Visibility = DevExpress.Utils.DefaultBoolean.True
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Label.Font = miFuenteEjes
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.GridSpacingAuto = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.GridSpacing = 1
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Text = tituloY
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Font = miFuenteAlto
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.GridLines.Visible = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Tickmarks.Visible = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Tickmarks.MinorVisible = False

                    If config.Tables(0).Rows(0)!grueso_spiline > 0 Then
                        Dim view_2 As DataView = New DataView(tablaGRafico)
                        view_2.RowFilter = "pareto = 1"
                        Dim tablaPareto As DataTable = view_2.ToTable()
                        Dim series2 As New Series(indicador02, ViewType.Spline)

                        ChartControl1.Series.Add(series2)
                        series2.DataSource = tablaPareto
                        series2.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                        series2.View.Color = Color.Green

                        series2.ArgumentScaleType = ScaleType.Qualitative
                        series2.ArgumentDataMember = "nombre"
                        series2.ValueScaleType = ScaleType.Numerical
                        series2.ValueDataMembers.AddRange(New String() {"porcentaje"})
                        series2.Label.BackColor = Color.DarkBlue
                        series2.Label.TextColor = Color.White
                        series2.Label.Font = miFuente
                        series2.Label.TextPattern = "{V:F0}"

                        CType(series2.View, SplineSeriesView).LineStyle.Thickness = config.Tables(0).Rows(0)!grueso_spiline

                        Dim myAxisY As New SecondaryAxisY(traduccion(63))
                        CType(ChartControl1.Diagram, XYDiagram).SecondaryAxesY.Clear()
                        CType(ChartControl1.Diagram, XYDiagram).SecondaryAxesY.Add(myAxisY)
                        CType(series2.View, LineSeriesView).AxisY = myAxisY
                        myAxisY.Title.Text = config.Tables(0).Rows(0)!texto_z
                        myAxisY.Title.Visible = True
                        myAxisY.Label.Font = miFuenteEjes
                        myAxisY.Title.Font = miFuenteAlto
                        myAxisY.GridLines.Visible = False
                        myAxisY.Tickmarks.Visible = False
                        myAxisY.Tickmarks.MinorVisible = False
                        myAxisY.Title.TextColor = Color.Green
                        myAxisY.Label.TextColor = Color.Green
                        myAxisY.Color = Color.Green
                    End If
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.GridLines.Visible = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.Font = miFuenteEjes
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Text = Strings.Space(5) & config.Tables(0).Rows(0)!texto_x & Strings.Space(10)
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.GridLines.Visible = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Font = miFuenteAlto
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Visibility = DevExpress.Utils.DefaultBoolean.True
                    If config.Tables(0).Rows(0)!overlap = "R" Then
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowStagger = False
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowRotate = True
                    Else
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowStagger = True
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowRotate = False
                    End If


                    ChartControl1.Titles.Add(Titulo)
                    Dim Titulo2 As New ChartTitle()

                    Titulo2.Font = miFuente
                    Titulo2.Text = traduccion(56) & cadPeriodo
                    ChartControl1.Titles.Add(Titulo2)
                    Dim Titulo3 As New ChartTitle()
                    Titulo3.Font = miFuente
                    Titulo3.Text = traduccion(57) & Format(Now, "ddd dd-MMM-yyyy HH:mm:ss")
                    ChartControl1.Titles.Add(Titulo3)
                    Dim Titulo4 As New ChartTitle()
                    Titulo4.Font = miFuente
                    Titulo4.Text = traduccion(58) & Format(eDesde, "dd-MMM-yyyy HH:mm:ss") & traduccion(59) & Format(eHasta, "dd-MMM-yyyy HH:mm:ss")
                    ChartControl1.Titles.Add(Titulo4)
                    ChartControl1.Width = 1200
                    ChartControl1.Height = 800

                    If config.Tables(0).Rows(0)!ver_leyenda = "S" Then
                        ChartControl1.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True
                    Else
                        ChartControl1.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False
                    End If
                    Try
                        Dim rutaImagen = Microsoft.VisualBasic.Strings.Replace(archivoImagen, "\", "\\")
                        SaveChartImageToFile(ChartControl1, ImageFormat.Png, rutaImagen)
                        Dim image As Image = GetChartImage(ChartControl1, ImageFormat.Png)
                        image.Save(rutaImagen)

                    Catch ex As Exception
                        agregarLOG(traduccion(60) & ex.Message, 7, 0)
                    End Try
                End If
            End If
        End If


    End Function

    Function generarReporteTipo6(idReporte As Integer, reporte As String, fName As String, ruta As String, graficar As String, consulta As Long, modulo As Integer, grafica As Long) As Integer
        generarReporteTipo6 = 0

        Dim archivoSaliente = ruta & "\" & fName & ".csv"
        Dim archivoImagen = ruta & "\" & fName & ".png"

        Try
            My.Computer.FileSystem.DeleteFile(archivoSaliente)


        Catch ex As Exception

        End Try

        Try
            My.Computer.FileSystem.DeleteFile(archivoImagen)


        Catch ex As Exception

        End Try

        archivoSaliente = archivoSaliente.Replace("\", "\\")


        Dim comillas = Microsoft.VisualBasic.Strings.Left(Chr(34), 1)

        Dim inicial = ""
        Dim cabecera = ""
        Dim registros = ""

        Dim Leer As Boolean = False
        Dim cadSQL = ""
        Dim miReporte = idReporte - 300

        cadSQL = "SELECT * FROM " & rutaBD & ".pu_graficos WHERE (usuario = 1 OR usuario = 0) AND grafico = " & grafica & " AND idioma = " & IIf(be_idioma = 0, 1, be_idioma) & " ORDER BY usuario DESC LIMIT 1"

        Dim config As DataSet = consultaSEL(cadSQL)
        If config.Tables(0).Rows.Count > 0 Then
            Dim ordenPareto = ValNull(config.Tables(0).Rows(0)!orden, "N")

            If consulta > 0 Then generarFIltro(consulta)

            Dim mensajesDS As DataSet
            Dim sentencia As String = ""
            Dim adicional = ""
            Dim regsAfectados = 0
            Dim tHaving = ""
            Dim ordenDatos = " 9 "
            Dim campoSumar = "tiempo"

            inicial = Chr(34) & reporte & " (" & cadPeriodo & ")" & Chr(34) & vbCrLf
            inicial = inicial & Chr(34) & traduccion(110) & ": " & Format(Now(), "ddd, dd-MMM-yyyy HH:mm:ss") & Chr(34) & vbCrLf
            inicial = inicial & Chr(34) & traduccion(111) & ": " & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & " " & traduccion(112) & ": " & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & Chr(34) & vbCrLf


            If config.Tables(0).Rows(0)!incluir_ceros = "N" Then

                tHaving = " HAVING docs > 0 "
            End If
            If config.Tables(0).Rows(0)!orden = "1" Then
                ordenDatos = "8"
                campoSumar = "docs"
            ElseIf config.Tables(0).Rows(0)!orden_grafica = "M" Then
                ordenDatos = ordenDatos & " DESC"
            ElseIf config.Tables(0).Rows(0)!orden_grafica = "A" Then
                ordenDatos = " 5 "
            End If

            Dim filtroTiempo = " a.fecha >= '" & fDesdeSF & "' AND a.fecha <= '" & fHastaSF & "' "
            If sDesde <> "-1" Then
                filtroTiempo = " a.secuencia_turno " & sDesde & " "
            End If

            sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, c.referencia, c.id, COUNT(*) AS docs, SUM(a.tiempo) / 3600 AS tiempo FROM " & rutaBD & ".detalleparos a LEFT JOIN " & rutaBD & ".cat_maquinas b ON a.maquina = b.id LEFT JOIN " & rutaBD & ".cat_lineas c ON b.linea = c.id WHERE a.estado = 'F' AND" & filtroTiempo & filtroParos & " GROUP BY nombre " & tHaving & " ORDER BY " & ordenDatos

            If miReporte = 2 Then

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, IFNULL(b.nombre, '" & traduccion(100) & "') AS nombre, b.referencia, a.maquina, COUNT(*) AS docs, SUM(a.tiempo) / 3600 AS tiempo FROM " & rutaBD & ".detalleparos a LEFT JOIN " & rutaBD & ".cat_maquinas b ON a.maquina = b.id WHERE a.estado = 'F' AND" & filtroTiempo & filtroParos & " GROUP BY nombre " & tHaving & " ORDER BY " & ordenDatos

            ElseIf miReporte = 3 Then

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, '' AS referencia, a.area AS id, COUNT(*) AS docs, SUM(a.tiempo) / 3600 AS tiempo FROM " & rutaBD & ".detalleparos a LEFT JOIN " & rutaBD & ".cat_maquinas b ON a.maquina = b.id LEFT JOIN " & rutaBD & ".cat_generales c ON a.area = c.id WHERE a.estado = 'F' AND" & filtroTiempo & filtroParos & " GROUP BY nombre " & tHaving & " ORDER BY " & ordenDatos

                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(151) & Chr(34) & "," & Chr(34) & traduccion(172) & Chr(34) & "," & Chr(34) & traduccion(174) & Chr(34) & "," & Chr(34) & traduccion(175) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)


            ElseIf miReporte = 4 Then

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, '' AS referencia, a.tipo AS id, COUNT(*) AS docs, SUM(a.tiempo) / 3600 AS tiempo FROM " & rutaBD & ".detalleparos a LEFT JOIN " & rutaBD & ".cat_maquinas b ON a.maquina = b.id LEFT JOIN " & rutaBD & ".cat_generales c ON a.tipo = c.id WHERE a.estado = 'F' AND" & filtroTiempo & filtroParos & " GROUP BY nombre " & tHaving & " ORDER BY " & ordenDatos

                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(151) & Chr(34) & "," & Chr(34) & traduccion(173) & Chr(34) & "," & Chr(34) & traduccion(174) & Chr(34) & "," & Chr(34) & traduccion(175) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)

            ElseIf miReporte = 5 Then


                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, DATE_FORMAT(a.fecha, '%Y/%m/%d') AS nombre, '' AS referencia, 0 AS id, COUNT(*) AS docs, SUM(a.tiempo) / 3600 AS tiempo FROM " & rutaBD & ".detalleparos a LEFT JOIN " & rutaBD & ".cat_maquinas b ON a.maquina = b.id WHERE a.estado = 'F' AND" & filtroTiempo & filtroParos & " GROUP BY 5 " & tHaving & " ORDER BY " & ordenDatos

                cabecera = Chr(34) & traduccion(79) & Chr(34) & "," & Chr(34) & traduccion(51) & Chr(34) & "," & Chr(34) & traduccion(174) & Chr(34) & "," & Chr(34) & traduccion(175) & Chr(34) & "," & Chr(34) & traduccion(109) & Chr(34) & "," & Chr(34) & traduccion(107) & Chr(34)


            ElseIf miReporte = 6 Then

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, CONCAT(YEAR(a.fecha), '/', WEEK(a.fecha)) AS nombre, STR_TO_DATE(CONCAT(DATE_FORMAT(a.fecha, '%x/%v'), ' Monday'), '%x/%v %W') AS referencia, 0 AS id, COUNT(*) AS docs, SUM(a.tiempo) / 3600 AS tiempo FROM " & rutaBD & ".detalleparos a LEFT JOIN " & rutaBD & ".cat_maquinas b ON a.maquina = b.id WHERE a.estado = 'F' AND" & filtroTiempo & filtroParos & " GROUP BY 5 " & tHaving & " ORDER BY " & ordenDatos


            ElseIf miReporte = 7 Then

                sentencia = "SELECT 1 AS pareto, 0 AS porcentaje, 1 AS filtro, 0 AS orden, CONCAT(YEAR(a.fecha), '/', MONTH(a.fecha)) AS nombre, '' AS referencia, 0 AS id, COUNT(*) AS docs, SUM(a.tiempo) / 3600 AS tiempo FROM " & rutaBD & ".detalleparos a LEFT JOIN " & rutaBD & ".cat_maquinas b ON a.maquina = b.id WHERE a.estado = 'F' AND" & filtroTiempo & filtroParos & " GROUP BY 5 " & tHaving & " ORDER BY " & ordenDatos
            End If
            mensajesDS = consultaSEL(sentencia)
            adicional = ""
            If mensajesDS.Tables(0).Rows.Count > 0 Then
                Dim view_o As DataView = New DataView(mensajesDS.Tables(0))
                Dim objWriter As New System.IO.StreamWriter(archivoSaliente, False, System.Text.Encoding.UTF8)
                objWriter.WriteLine(inicial)
                objWriter.WriteLine(cabecera)
                Dim totalPareto As Double = 0


                If config.Tables(0).Rows(0)!maximo_barras > 0 And config.Tables(0).Rows(0)!maximo_barras < mensajesDS.Tables(0).Rows.Count Or config.Tables(0).Rows(0)!maximo_barraspct > 0 And config.Tables(0).Rows(0)!maximo_barraspct < 100 Then
                    'Se calcula el total del Pareto
                    Dim limitar = 0
                    Dim agrupado = ""
                    Dim pcAcum = 0
                    If config.Tables(0).Rows(0)!maximo_barraspct > 0 And config.Tables(0).Rows(0)!maximo_barraspct < 100 Then

                        For Each elmensaje As DataRow In mensajesDS.Tables(0).Rows
                            totalPareto = totalPareto + elmensaje.Item(campoSumar)
                        Next
                        Dim pct = config.Tables(0).Rows(0)!maximo_barraspct / 100
                        Dim i = 0
                        For Each elmensaje In mensajesDS.Tables(0).Rows
                            i = i + 1
                            pcAcum = pcAcum + elmensaje.Item(campoSumar)
                            If pcAcum / totalPareto >= pct Then
                                limitar = i
                                Exit For
                            End If
                        Next
                    End If
                    If config.Tables(0).Rows(0)!maximo_barras > 0 Then
                        If limitar > config.Tables(0).Rows(0)!maximo_barras Or limitar = 0 Then
                            limitar = config.Tables(0).Rows(0)!maximo_barras
                        End If
                    End If

                    If limitar + 1 >= mensajesDS.Tables(0).Rows.Count And config.Tables(0).Rows(0)!agrupar = "S" Then
                        limitar = 0
                    ElseIf limitar >= mensajesDS.Tables(0).Rows.Count Then
                        limitar = 0
                    End If
                    If limitar > 0 Then
                        For j = 0 To limitar - 1
                            mensajesDS.Tables(0).Rows(j)!orden = j + 1
                        Next
                        If config.Tables(0).Rows(0)!agrupar = "S" Then
                            'mttr y mtbf
                            Dim faltante1 = 0
                            Dim faltante2 = 0
                            Dim faltante3 = 0
                            Dim faltante4 = 0
                            Dim faltante5 = 0
                            Dim totalAgr = 0
                            For j = limitar To mensajesDS.Tables(0).Rows.Count - 1
                                mensajesDS.Tables(0).Rows(j)!filtro = 0
                                faltante1 = faltante1 + mensajesDS.Tables(0).Rows(j)!docs
                                faltante2 = faltante2 + mensajesDS.Tables(0).Rows(j)!tiempo
                            Next
                            totalAgr = mensajesDS.Tables(0).Rows.Count - limitar
                            Dim row As DataRow = mensajesDS.Tables(0).NewRow

                            row("id") = "0"
                            row("nombre") = IIf(ValNull(config.Tables(0).Rows(0)!agrupar_texto, "A") = "", traduccion(61), config.Tables(0).Rows(0)!agrupar_texto) & " (" & totalAgr & ")"
                            row("docs") = faltante1
                            row("tiempo") = faltante2
                            row("porcentaje") = 0
                            row("filtro") = 1
                            row("pareto") = 1
                            If config.Tables(0).Rows(0)!agrupar_posicion = "F" Then
                                row("orden") = limitar + 1
                            ElseIf config.Tables(0).Rows(0)!agrupar_posicion = "P" Then
                                row("orden") = 0
                            End If
                            mensajesDS.Tables(0).Rows.Add(row)

                        Else
                            adicional = traduccion(147)
                        End If
                    End If
                    If config.Tables(0).Rows(0)!agrupar_posicion = "N" Then
                        view_o.Sort = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " " & campoSumar & " DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " " & campoSumar, "nombre"))
                    Else
                        view_o.Sort = "orden ASC"
                    End If
                Else
                    view_o.Sort = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " " & campoSumar & " DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " " & campoSumar, "nombre"))
                End If
                view_o.RowFilter = "filtro = 1"
                totalPareto = 0
                For Each registro In mensajesDS.Tables(0).Rows
                    If registro!filtro = 1 Then
                        totalPareto = totalPareto + registro.Item(campoSumar)
                    End If
                Next
                Dim acumPareto As Double = 0
                Dim linea = 0

                tablaGRafico = view_o.ToTable()
                For Each registro In tablaGRafico.Rows
                    If registro!filtro = 1 Then
                        linea = linea + 1
                        acumPareto = acumPareto + registro.Item(campoSumar) / totalPareto * 100
                        registro!porcentaje = acumPareto
                        If linea = mensajesDS.Tables(0).Rows.Count Then
                            acumPareto = 100
                        End If
                        Dim cadID = ""
                        If miReporte <> 5 And miReporte <> 6 And miReporte <> 7 Then
                            cadID = Chr(34) & registro!id & Chr(34) & ","
                        End If
                        Dim cadInicio = ""
                        If miReporte = 6 Then
                            cadInicio = Chr(34) & registro!inicio & Chr(34) & ","
                        End If
                        objWriter.WriteLine(Chr(34) & linea & Chr(34) & "," & cadID & Chr(34) & registro!nombre & Chr(34) & "," & cadInicio & Chr(34) & registro!docs & Chr(34) & "," & Chr(34) & registro!tiempo & Chr(34) & "," & Chr(34) & IIf(registro!pareto = 1, registro.Item(campoSumar) / totalPareto * 100, "") & Chr(34) & "," & Chr(34) & IIf(registro!pareto = 1, acumPareto, "") & Chr(34))
                    End If
                Next
                objWriter.WriteLine(traduccion(121) & ": " & linea)
                If adicional.Length > 0 Then objWriter.WriteLine(adicional)
                objWriter.WriteLine(traduccion(122))
                objWriter.Close()

                If graficar = "S" Then


                    Dim indicador01 = ""
                    Dim indicador02 = ""

                    Dim titulosSeries = config.Tables(0).Rows(0)!textos_adicionales.Split(New Char() {";"c})
                    If titulosSeries.length = 1 Then
                        indicador01 = titulosSeries(0)
                    End If
                    If titulosSeries.length = 2 Then
                        indicador01 = titulosSeries(0)
                        indicador02 = titulosSeries(1)
                    End If

                    If indicador01.Length = 0 Then indicador01 = traduccion(162)
                    If indicador02.Length = 0 Then indicador02 = traduccion(107)


                    ChartControl1.Series.Clear()
                    ChartControl1.Titles.Clear()
                    Dim Titulo As New ChartTitle()
                    Titulo.Text = config.Tables(0).Rows(0)!titulo & Strings.Space(10)
                    Dim miFuente = New Drawing.Font("Lucida Sans", 10, FontStyle.Regular)
                    Dim miFuenteAlto = New Drawing.Font("Lucida Sans", 16, FontStyle.Bold)
                    Dim miFuenteEjes = New Drawing.Font("Lucida Sans", 11, FontStyle.Regular)

                    Titulo.Font = miFuenteAlto
                    Dim series1 As New Series(indicador01, ViewType.Bar)

                    ChartControl1.Series.Add(series1)
                    series1.DataSource = tablaGRafico

                    series1.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                    series1.View.Color = Color.SkyBlue
                    series1.ArgumentScaleType = ScaleType.Qualitative
                    series1.ArgumentDataMember = "nombre"
                    series1.ValueScaleType = ScaleType.Numerical
                    series1.ValueDataMembers.AddRange(New String() {campoSumar})
                    series1.Label.BackColor = Color.DarkBlue
                    series1.Label.TextColor = Color.White
                    series1.Label.Font = miFuente
                    series1.Label.TextPattern = "{V:F" & config.Tables(0).Rows(0)!etiqueta_formato & "}"

                    'Obtener el titulo en Y
                    Dim tituloY As String = ""
                    Dim titulosY = ValNull(config.Tables(0).Rows(0)!texto_y, "A")
                    Try

                        Dim titulosYArreglo = titulosY.Split(New Char() {";"c})
                        If titulosYArreglo.length = 1 Then
                            tituloY = titulosYArreglo(0)
                        Else
                            tituloY = titulosYArreglo(ordenPareto)
                        End If
                    Catch ex As Exception
                        tituloY = titulosY
                    End Try
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Visibility = DevExpress.Utils.DefaultBoolean.True
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Label.Font = miFuenteEjes
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.GridSpacingAuto = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.GridSpacing = 1
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Text = tituloY
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Font = miFuenteAlto
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.GridLines.Visible = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Tickmarks.Visible = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisY.Tickmarks.MinorVisible = False

                    If config.Tables(0).Rows(0)!grueso_spiline > 0 Then
                        Dim view_2 As DataView = New DataView(tablaGRafico)
                        view_2.RowFilter = "pareto = 1"
                        Dim tablaPareto As DataTable = view_2.ToTable()
                        Dim series2 As New Series(indicador02, ViewType.Spline)

                        ChartControl1.Series.Add(series2)
                        series2.DataSource = tablaPareto
                        series2.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                        series2.View.Color = Color.Green

                        series2.ArgumentScaleType = ScaleType.Qualitative
                        series2.ArgumentDataMember = "nombre"
                        series2.ValueScaleType = ScaleType.Numerical
                        series2.ValueDataMembers.AddRange(New String() {"porcentaje"})
                        series2.Label.BackColor = Color.DarkBlue
                        series2.Label.TextColor = Color.White
                        series2.Label.Font = miFuente
                        series2.Label.TextPattern = "{V:F0}"

                        CType(series2.View, SplineSeriesView).LineStyle.Thickness = config.Tables(0).Rows(0)!grueso_spiline

                        Dim myAxisY As New SecondaryAxisY(traduccion(63))
                        CType(ChartControl1.Diagram, XYDiagram).SecondaryAxesY.Clear()
                        CType(ChartControl1.Diagram, XYDiagram).SecondaryAxesY.Add(myAxisY)
                        CType(series2.View, LineSeriesView).AxisY = myAxisY
                        myAxisY.Title.Text = config.Tables(0).Rows(0)!texto_z
                        myAxisY.Title.Visible = True
                        myAxisY.Label.Font = miFuenteEjes
                        myAxisY.Title.Font = miFuenteAlto
                        myAxisY.GridLines.Visible = False
                        myAxisY.Tickmarks.Visible = False
                        myAxisY.Tickmarks.MinorVisible = False
                        myAxisY.Title.TextColor = Color.Green
                        myAxisY.Label.TextColor = Color.Green
                        myAxisY.Color = Color.Green
                    End If
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.GridLines.Visible = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.Font = miFuenteEjes
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Text = Strings.Space(5) & config.Tables(0).Rows(0)!texto_x & Strings.Space(10)
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.GridLines.Visible = False
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Font = miFuenteAlto
                    CType(ChartControl1.Diagram, XYDiagram).AxisX.Title.Visibility = DevExpress.Utils.DefaultBoolean.True
                    If config.Tables(0).Rows(0)!overlap = "R" Then
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowStagger = False
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowRotate = True
                    Else
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowStagger = True
                        CType(ChartControl1.Diagram, XYDiagram).AxisX.Label.ResolveOverlappingOptions.AllowRotate = False
                    End If


                    ChartControl1.Titles.Add(Titulo)
                    Dim Titulo2 As New ChartTitle()

                    Titulo2.Font = miFuente
                    Titulo2.Text = traduccion(56) & cadPeriodo
                    ChartControl1.Titles.Add(Titulo2)
                    Dim Titulo3 As New ChartTitle()
                    Titulo3.Font = miFuente
                    Titulo3.Text = traduccion(57) & Format(Now, "ddd dd-MMM-yyyy HH:mm:ss")
                    ChartControl1.Titles.Add(Titulo3)
                    Dim Titulo4 As New ChartTitle()
                    Titulo4.Font = miFuente
                    Titulo4.Text = traduccion(58) & Format(eDesde, "dd-MMM-yyyy HH:mm:ss") & traduccion(59) &
                                                            Format(eHasta, "dd-MMM-yyyy HH:mm:ss")
                    ChartControl1.Titles.Add(Titulo4)
                    ChartControl1.Width = 1200
                    ChartControl1.Height = 800

                    If config.Tables(0).Rows(0)!ver_leyenda = "S" Then
                        ChartControl1.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True
                    Else
                        ChartControl1.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False
                    End If
                    Try
                        Dim rutaImagen = Microsoft.VisualBasic.Strings.Replace(archivoImagen, "\", "\\")
                        SaveChartImageToFile(ChartControl1, ImageFormat.Png, rutaImagen)
                        Dim image As Image = GetChartImage(ChartControl1, ImageFormat.Png)
                        image.Save(rutaImagen)

                    Catch ex As Exception
                        agregarLOG(traduccion(60) & ex.Message, 7, 0)
                    End Try
                End If
            End If
        End If


    End Function
    Private Sub ValidarPuntos(ByVal sender As Object, ByVal e As CustomDrawSeriesPointEventArgs)
        Dim foundRow() As DataRow
        foundRow = tablaGRafico.Select("pareto = 0 AND nombre = '" & e.SeriesPoint.Argument & "'")
        If foundRow.Length > 0 Then
            e.SeriesPoint.Color = Color.DodgerBlue
        End If
    End Sub

    Function calcularPromedio(tiempo As Integer) As String
        calcularPromedio = ""
        tiempo = Math.Round(tiempo, 0)
        Dim horas = tiempo / 3600
        Dim minutos = (tiempo Mod 3600) / 60
        Dim segundos = tiempo Mod 60
        If segundos > 30 Then
            minutos = minutos + 1
        End If
        If minutos = 0 And horas = 0 Then
            minutos = 1
        End If
        calcularPromedio = Format(Math.Floor(horas), "00") & ":" & Format(Math.Floor(minutos), "00")
    End Function

    Private Sub agregarLOG(cadena As String, Optional reporte As Integer = 0, Optional tipo As Integer = 0, Optional aplicacion As Integer = 80)
        If Not be_log_activar Then Exit Sub
        'tipo 0: Info
        'tipo 2: Advertencia
        'tipo 9: Error
        Dim regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".log (aplicacion, tipo, proceso, texto) VALUES (" & aplicacion & ", " & tipo & ", " & reporte & ", '" & Microsoft.VisualBasic.Strings.Left(cadena, 250) & "')")
    End Sub

    Private Function GetChartImage(ByVal chart As ChartControl, ByVal format As ImageFormat) As Image
        ' Create an image.  
        Dim image As Image = Nothing

        ' Create an image of the chart.  
        Using s As New MemoryStream()
            chart.ExportToImage(s, format)
            image = System.Drawing.Image.FromStream(s)
        End Using

        ' Return the image.  
        Return image
    End Function

    Private Function GetGaugeImage(ByVal chart As GaugeControl, ByVal format As ImageFormat) As Image
        ' Create an image.  
        Dim image As Image = Nothing

        ' Create an image of the chart.  
        Using s As New MemoryStream()
            chart.ExportToImage(s, format)
            image = System.Drawing.Image.FromStream(s)
        End Using

        ' Return the image.  
        Return image
    End Function

    Private Sub SaveChartImageToFile(ByVal chart As ChartControl, ByVal format As ImageFormat, ByVal fileName As String)
        ' Create an image in the specified format from the chart  
        ' and save it to the specified path.  
        chart.ExportToImage(fileName, format)
    End Sub

    Private Sub SaveGaugeImageToFile(ByVal chart As GaugeControl, ByVal format As ImageFormat, ByVal fileName As String)
        ' Create an image in the specified format from the chart  
        ' and save it to the specified path.  
        chart.ExportToImage(fileName, format)
    End Sub

    Sub etiquetas()
        Dim general = consultaSEL("SELECT cadena FROM " & rutaBD & ".det_idiomas_back WHERE idioma = " & IIf(be_idioma = 0, 1, be_idioma) & " AND modulo = 5 ORDER BY linea")
        Dim cadenaTrad = ""
        If general.Tables(0).Rows.Count > 0 Then
            For Each cadena In general.Tables(0).Rows
                cadenaTrad = cadenaTrad & cadena!cadena
            Next
        End If
        traduccion = cadenaTrad.Split(New Char() {";"c})
        Label1.Text = traduccion(0)
        cad_consolidado = traduccion(1)
    End Sub


    Sub generarFIltro(consulta)
        Dim general = consultaSEL("SELECT * FROM " & rutaBD & ".consultas_cab WHERE id = " & consulta)
        Dim cadenaTrad = ""
        If general.Tables(0).Rows.Count > 0 Then
            If general.Tables(0).Rows(0)!filtrolin = "N" Then

                filtroReportes = " AND c.linea IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 10) "
                filtroMTBF = " AND a.linea IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 10) "
                filtroOEE = filtroOEE & " AND a.linea IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 10) "
                filtroOEEDias = filtroOEEDias & " AND j.linea IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 10) "
                filtroParos = " AND b.linea IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 10) "
                filtroRechazos = " AND j.linea IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 10) "

            End If
            If general.Tables(0).Rows(0)!filtroori = "0" Then

                filtroReportes = filtroReportes & " AND c.origen = 0 "

            ElseIf general.Tables(0).Rows(0)!filtroori = "1" Then

                filtroReportes = filtroReportes & " AND c.origen > 0 "
            End If
            If general.Tables(0).Rows(0)!filtromaq = "N" Then

                filtroReportes = filtroReportes & " AND c.maquina IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 20) "
                filtroMTBF = filtroMTBF & " AND a.id IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 20) "

                filtroOEE = filtroOEE & " AND a.id IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 20) "
                filtroOEEDias = filtroOEEDias & " AND i.equipo IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 20) "
                filtroParos = filtroParos & " AND a.maquina IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 20) "
                filtroRechazos = filtroRechazos & " AND i.equipo IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 20) "

            End If
            If general.Tables(0).Rows(0)!filtroare = "N" Then

                filtroReportes = filtroReportes & " AND c.area IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 30) "
                filtroMTBF_are = " AND a.id IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 30) "
                filtroParos = filtroParos & " AND a.area IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 30) "
                filtroRechazos = filtroRechazos & " AND i.area IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 30) "

            End If
            If general.Tables(0).Rows(0)!filtrofal = "N" Then

                filtroReportes = filtroReportes & " AND c.falla_ajustada IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 40) "
                filtroMTBF_fal = " AND a.id IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 40) "
            End If
            If general.Tables(0).Rows(0)!filtrotec = "N" Then

                filtroReportes = filtroReportes & " AND c.tecnicoatend IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 50) "
                filtroMTBF_tec = " AND a.id IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 50) "
            End If
            If general.Tables(0).Rows(0)!filtronpar = "N" Then

                filtroCorte = filtroCorte & " AND i.parte IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 60) "
                filtroParos = filtroParos & " AND a.parte IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 60) "
                filtroRechazos = filtroRechazos & " AND i.parte IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 60) "

                filtroOEEDias = filtroOEEDias & " AND i.parte IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 60) "


                If general.Tables(0).Rows(0)!filtrotur = "N" Then

                    filtroCorte = filtroCorte & " AND i.turno IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 70) "
                    filtroParos = filtroParos & " AND a.turno IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 70) "
                    filtroRechazos = filtroRechazos & " AND i.turno IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 70) "

                    filtroOEEDias = filtroOEEDias & " AND i.turno IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 70) "

                End If
                If general.Tables(0).Rows(0)!filtroord = "N" Then

                    filtroCorte = filtroCorte & " AND i.orden IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 80) "
                    filtroParos = filtroParos & " AND a.lote IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 80) "
                    filtroRechazos = filtroRechazos & " AND i.orden IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 80) "
                    filtroOEEDias = filtroOEEDias & " AND i.orden IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 80) "

                End If
                If general.Tables(0).Rows(0)!filtropar = "N" Then

                    filtroParos = filtroParos & " AND a.tipo IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 100) "
                    filtroRechazos = filtroRechazos & " AND i.tipo IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 120) "

                End If
                If general.Tables(0).Rows(0)!filtrocla(0) = "N" Then

                    Dim filtroClase = " AND ("
                    If general.Tables(0).Rows(0)!filtrocla(1) = "S" Then

                        filtroClase = filtroClase & "a.clase = 0"
                    End If
                    If general.Tables(0).Rows(0)!filtrocla(2) = "S" Then

                        filtroClase = filtroClase & IIf(filtroClase = " AND (", "a.clase = 1", " OR a.clase = 1")
                    End If
                    If general.Tables(0).Rows(0)!filtrocla(3) = "S" Then

                        filtroClase = filtroClase & IIf(filtroClase = " AND (", "a.clase = 2", " OR a.clase = 2")
                    End If
                    If general.Tables(0).Rows(0)!filtrocla(4) = "S" Then

                        filtroClase = filtroClase & IIf(filtroClase = " AND (", "a.clase = 3", " OR a.clase = 3")
                    End If
                    If general.Tables(0).Rows(0)!filtrocla(5) = "S" Then

                        filtroClase = filtroClase & IIf(filtroClase = " AND (", "a.clase = 4", " OR a.clase = 4")
                    End If
                    If filtroClase <> " AND (" Then

                        filtroParos = filtroParos & filtroClase & ") "
                    End If
                End If
            End If
        End If
    End Sub

    Sub buscarPeriodos(periodo As String, modulo As Integer, indice As Integer, idReporte As Long)
        Dim fDesde = fDesdeSF
        Dim fHasta = fHastaSF
        Dim desde = Now.Date
        Dim hasta = Now.Date
        Dim grupo = IIf(tituloPeriodos(indice).length > 0, tituloPeriodos(indice), traduccion(148))
        If periodo = 2 Then

            desde = Format(Now, "yyyy") & "/01/01"
            grupo = IIf(tituloPeriodos(indice).length > 0, tituloPeriodos(indice), traduccion(149) & "(" & Format(desde, "yyyy") & ")")

        ElseIf periodo = 3 Then

            desde = Format(Now, "yyyy/MM") & "/01"
            grupo = IIf(tituloPeriodos(indice).length > 0, tituloPeriodos(indice), traduccion(150) & "(" & Format(desde, "yyyy/MM") & ")")

        ElseIf periodo = "4" Then

            desde = Format(DateAndTime.DateAdd(DateInterval.Year, -1, Now), "yyyy") & "/01/01"
            hasta = Format(DateAndTime.DateAdd(DateInterval.Year, -1, Now), "yyyy") & "/12/31"
            grupo = IIf(tituloPeriodos(indice).length > 0, tituloPeriodos(indice), Format(hasta, "yyyy"))

        ElseIf periodo = "5" Then

            desde = Format(DateAndTime.DateAdd(DateInterval.Month, -1, Now), "yyyy/MM") & "/01"
            hasta = Format(DateAndTime.DateAdd(DateInterval.Day, -1, DateValue(Format(Now, "yyyy/MM" & "/01"))), "yyyy/MM/dd")
            grupo = IIf(tituloPeriodos(indice).length > 0, tituloPeriodos(indice), Format(hasta, "yyyy/MM"))

        ElseIf periodo = "6" Then

            desde = Format(DateAndTime.DateAdd(DateInterval.Year, -1, Now), "yyyy/MM") & "/01"
            Dim tmpHasta = DateAndTime.DateAdd(DateInterval.Month, 1, desde)
            hasta = Format(DateAndTime.DateAdd(DateInterval.Day, -1, DateValue(Format(tmpHasta, "yyyy/MM" & "/01"))), "yyyy/MM/dd")
            grupo = IIf(tituloPeriodos(indice).length > 0, tituloPeriodos(indice), Format(hasta, "yyyy/MM"))

        End If

        If periodo > 1 Then
            fDesde = Format(desde, "yyyy/MM/dd")
            fHasta = Format(hasta, "yyyy/MM/dd")

        End If


        Dim sentencia = ""
        If modulo <= 2 Then
            sentencia = "SELECT 0 AS linea, '" & grupo & "' AS nombre, COUNT(*), e.tdias, SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, (SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0))) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_maquinas a " & IIf(modulo = 1, "INNER", "LEFT") + " JOIN (SELECT maquina, linea, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  AND c.fecha_reporte >= '" & fDesde & "' AND c.fecha_reporte <= '" & fHasta & "' " & IIf(periodo <> 1, filtroReportes, "") + " GROUP BY maquina, linea) AS c ON a.id = c.maquina LEFT JOIN " & rutaBD & ".cat_lineas b ON a.linea = b.id, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesde & "' AND fecha <= '" & fHasta & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & IIf(periodo <> 1, filtroMTBF, ""))

            If idReporte = 2 Then

                sentencia = "SELECT 0 AS id, '" & grupo & "' AS nombre, e.tdias, SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, (SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0))) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_maquinas a " & IIf(modulo = 1, "INNER", "LEFT") + " JOIN (SELECT linea, maquina, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  AND c.fecha_reporte >= '" & fDesde & "' AND c.fecha_reporte <= '" & fHasta & "' " & IIf(periodo <> 1, filtroReportes, "") + " GROUP BY linea, maquina) AS c ON a.id = c.maquina, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesde & "' AND fecha <= '" & fHasta & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & IIf(periodo <> 1, filtroMTBF, ""))

            ElseIf idReporte = 3 Then

                sentencia = "SELECT 0 AS id, '" & grupo & "' AS nombre, e.tdias, (e.lunes + e.martes + e.miercoles + e.jueves + e.viernes + e.sabado + e.domingo) AS tdisponible, (e.lunes + e.martes + e.miercoles + e.jueves + e.viernes + e.sabado + e.domingo) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_areas a " & IIf(modulo = 1, "INNER", "LEFT") + " JOIN (SELECT AREA, linea, maquina, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  AND c.fecha_reporte >= '" & fDesde & "' AND c.fecha_reporte <= '" & fHasta & "' " & IIf(periodo <> 1, filtroReportes, "") + " GROUP BY AREA, linea, maquina) AS c ON a.id = c.AREA, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS lunes, SUM(IF(dia = 3, 1, 0)) * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS martes, SUM(IF(dia = 4, 1, 0)) * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS miercoles, SUM(IF(dia = 5, 1, 0)) * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS jueves, SUM(IF(dia = 6, 1, 0)) * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS viernes, SUM(IF(dia = 7, 1, 0)) * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS sabado, SUM(IF(dia = 1, 1, 0)) * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesde & "' AND fecha <= '" & fHasta & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & IIf(periodo <> 1, filtroMTBF_are, ""))

            ElseIf idReporte = 4 Then

                sentencia = "SELECT 0 AS id, '" & grupo & "' AS nombre, e.tdias, (e.lunes + e.martes + e.miercoles + e.jueves + e.viernes + e.sabado + e.domingo) AS tdisponible, (e.lunes + e.martes + e.miercoles + e.jueves + e.viernes + e.sabado + e.domingo) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_fallas a " & IIf(modulo = 1, "INNER", "LEFT") + " JOIN (SELECT falla_ajustada, linea, maquina, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  AND c.fecha_reporte >= '" & fDesde & "' AND c.fecha_reporte <= '" & fHasta & "' " & IIf(periodo <> 1, filtroReportes, "") + " GROUP BY falla_ajustada, linea, maquina) AS c ON a.id = c.falla_ajustada, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS lunes, SUM(IF(dia = 3, 1, 0)) * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS martes, SUM(IF(dia = 4, 1, 0)) * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS miercoles, SUM(IF(dia = 5, 1, 0)) * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS jueves, SUM(IF(dia = 6, 1, 0)) * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS viernes, SUM(IF(dia = 7, 1, 0)) * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS sabado, SUM(IF(dia = 1, 1, 0)) * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesde & "' AND fecha <= '" & fHasta & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & IIf(periodo <> 1, filtroMTBF_fal, ""))

            ElseIf idReporte = 5 Then

                sentencia = "SELECT 0 AS tipo, '" & grupo & "' AS nombre, COUNT(*), e.tdias, SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, (SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0))) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_maquinas a " & IIf(modulo = 1, "INNER", "LEFT") + " JOIN (SELECT maquina, linea, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  AND c.fecha_reporte >= '" & fDesde & "' AND c.fecha_reporte <= '" & fHasta & "' " & IIf(periodo <> 1, filtroReportes, "") + " GROUP BY maquina, linea) AS c ON a.id = c.maquina LEFT JOIN " & rutaBD & ".cat_generales b ON a.tipo = b.id AND b.tabla = 50, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesde & "' AND fecha <= '" & fHasta & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & IIf(periodo <> 1, filtroMTBF, ""))

            ElseIf idReporte = 6 Then

                sentencia = "SELECT 0 AS agrupador_1, '" & grupo & "' AS nombre, COUNT(*), e.tdias, SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, (SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0))) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_maquinas a " & IIf(modulo = 1, "INNER", "LEFT") + " JOIN (SELECT maquina, linea, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  AND c.fecha_reporte >= '" & fDesde & "' AND c.fecha_reporte <= '" & fHasta & "' " & IIf(periodo <> 1, filtroReportes, "") + " GROUP BY maquina, linea) AS c ON a.id = c.maquina LEFT JOIN " & rutaBD & ".cat_generales b ON a.agrupador_1 = b.id AND b.tabla = 20, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesde & "' AND fecha <= '" & fHasta & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & IIf(periodo <> 1, filtroMTBF, ""))

            ElseIf idReporte = 7 Then

                sentencia = "SELECT 0 AS agrupador_2, '" & grupo & "' AS nombre, COUNT(*), e.tdias, SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, (SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0))) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_maquinas a " & IIf(modulo = 1, "INNER", "LEFT") + " JOIN (SELECT maquina, linea, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  AND c.fecha_reporte >= '" & fDesde & "' AND c.fecha_reporte <= '" & fHasta & "' " & IIf(periodo <> 1, filtroReportes, "") + " GROUP BY maquina, linea) AS c ON a.id = c.maquina LEFT JOIN " & rutaBD & ".cat_generales b ON a.agrupador_2 = b.id AND b.tabla = 25, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesde & "' AND fecha <= '" & fHasta & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & IIf(periodo <> 1, filtroMTBF, ""))

            ElseIf idReporte = 8 Then

                sentencia = "SELECT 0 AS agrupador_1, '" & grupo & "' AS nombre, COUNT(*), e.tdias, SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, (SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0))) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_fallas a " & IIf(modulo = 1, "INNER", "LEFT") + " JOIN (SELECT falla_ajustada, maquina, linea, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  AND c.fecha_reporte >= '" & fDesde & "' AND c.fecha_reporte <= '" & fHasta & "' " & IIf(periodo <> 1, filtroReportes, "") + " GROUP BY falla_ajustada, maquina, linea) AS c ON a.id = c.falla_ajustada LEFT JOIN " & rutaBD & ".cat_generales b ON a.agrupador_1 = b.id AND b.tabla = 40, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesde & "' AND fecha <= '" & fHasta & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & IIf(periodo <> 1, filtroMTBF_fal, ""))

            ElseIf idReporte = 9 Then

                sentencia = "SELECT 0 AS agrupador_2, '" & grupo & "' AS nombre, COUNT(*), e.tdias, SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, (SUM(e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0))) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_fallas a " & IIf(modulo = 1, "INNER", "LEFT") + " JOIN (SELECT falla_ajustada, maquina, linea, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  AND c.fecha_reporte >= '" & fDesde & "' AND c.fecha_reporte <= '" & fHasta & "' " & IIf(periodo <> 1, filtroReportes, "") + " GROUP BY falla_ajustada, maquina, linea) AS c ON a.id = c.falla_ajustada LEFT JOIN " & rutaBD & ".cat_generales b ON a.agrupador_2 = b.id AND b.tabla = 45, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesde & "' AND fecha <= '" & fHasta & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & IIf(periodo <> 1, filtroMTBF_fal, ""))

            ElseIf idReporte = 10 Then

                sentencia = "SELECT '" & grupo & "' AS nombre, IFNULL(IF(a.dia = 1, g.domingo, IF(a.dia = 2, g.lunes, IF(a.dia = 3, g.martes, IF(a.dia = 4, g.miercoles, IF(a.dia = 5, g.jueves, IF(a.dia = 6, g.viernes, g.sabado)))))), 0) AS tdisponible, IFNULL(IF(a.dia = 1, g.domingo, IF(a.dia = 2, g.lunes, IF(a.dia = 3, g.martes, IF(a.dia = 4, g.miercoles, IF(a.dia = 5, g.jueves, IF(a.dia = 6, g.viernes, g.sabado)))))), 0) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".dias a " & IIf(modulo = 1, "INNER", "LEFT") + " JOIN (SELECT fecha_reporte, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  AND c.fecha_reporte >= '" & fDesde & "' AND c.fecha_reporte <= '" & fHasta & "' " & IIf(periodo <> 1, filtroReportes, "") + " GROUP BY fecha_reporte) AS c ON a.fecha = c.fecha_reporte, (SELECT * FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND maquina = 0 AND linea = 0 LIMIT 1) AS g WHERE  fecha >= '" & fDesde & "' AND fecha <= '" & fHasta & "' "

            ElseIf idReporte = 11 Then

                sentencia = "SELECT '" & grupo & "' AS nombre, STR_TO_DATE(CONCAT(DATE_FORMAT(a.fecha,'%x/%v'), ' Monday'), '%x/%v %W') AS inicio, COUNT(*), SUM(IFNULL(IF(a.dia = 1, g.domingo, IF(a.dia = 2, g.lunes, IF(a.dia = 3, g.martes, IF(a.dia = 4, g.miercoles, IF(a.dia = 5, g.jueves, IF(a.dia = 6, g.viernes, g.sabado)))))), 0)) AS tdisponible, SUM(IFNULL(IF(a.dia = 1, g.domingo, IF(a.dia = 2, g.lunes, IF(a.dia = 3, g.martes, IF(a.dia = 4, g.miercoles, IF(a.dia = 5, g.jueves, IF(a.dia = 6, g.viernes, g.sabado)))))), 0)) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".dias a " & IIf(modulo = 1, "INNER", "LEFT") + " JOIN (SELECT fecha_reporte, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  AND c.fecha_reporte >= '" & fDesde & "' AND c.fecha_reporte <= '" & fHasta & "' " & IIf(periodo <> 1, filtroReportes, "") + " GROUP BY fecha_reporte) AS c ON a.fecha = c.fecha_reporte, (SELECT * FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND maquina = 0 AND linea = 0 LIMIT 1) AS g WHERE  fecha >= '" & fDesde & "' AND fecha <= '" & fHasta & "' "

            ElseIf idReporte = 12 Then

                sentencia = "SELECT '" & grupo & "' AS nombre, COUNT(*), SUM(IFNULL(IF(a.dia = 1, g.domingo, IF(a.dia = 2, g.lunes, IF(a.dia = 3, g.martes, IF(a.dia = 4, g.miercoles, IF(a.dia = 5, g.jueves, IF(a.dia = 6, g.viernes, g.sabado)))))), 0)) AS tdisponible, SUM(IFNULL(IF(a.dia = 1, g.domingo, IF(a.dia = 2, g.lunes, IF(a.dia = 3, g.martes, IF(a.dia = 4, g.miercoles, IF(a.dia = 5, g.jueves, IF(a.dia = 6, g.viernes, g.sabado)))))), 0)) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".dias a " & IIf(modulo = 1, "INNER", "LEFT") + " JOIN (SELECT fecha_reporte, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  AND c.fecha_reporte >= '" & fDesde & "' AND c.fecha_reporte <= '" & fHasta & "' " & IIf(periodo <> 1, filtroReportes, "") + " GROUP BY fecha_reporte) AS c ON a.fecha = c.fecha_reporte, (SELECT * FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND maquina = 0 AND linea = 0 LIMIT 1) AS g WHERE  fecha >= '" & fDesde & "' AND fecha <= '" & fHasta & "' "

            ElseIf idReporte = 13 Then

                sentencia = "SELECT 0 AS id, '" & grupo & "' AS nombre, e.tdias, (e.lunes + e.martes + e.miercoles + e.jueves + e.viernes + e.sabado + e.domingo) AS tdisponible, (e.lunes + e.martes + e.miercoles + e.jueves + e.viernes + e.sabado + e.domingo) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".cat_usuarios a " & IIf(modulo = 1, "INNER", "LEFT") + " JOIN (SELECT tecnicoatend, maquina, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100  AND c.fecha_reporte >= '" & fDesde & "' AND c.fecha_reporte <= '" & fHasta & "' " & IIf(periodo <> 1, filtroReportes, "") + " GROUP BY tecnicoatend, maquina) AS c ON a.id = c.tecnicoatend, (SELECT COUNT(*) AS tdias, SUM(IF(dia = 2, 1, 0)) * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS lunes, SUM(IF(dia = 3, 1, 0)) * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS martes, SUM(IF(dia = 4, 1, 0)) * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS miercoles, SUM(IF(dia = 5, 1, 0)) * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS jueves, SUM(IF(dia = 6, 1, 0)) * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS viernes, SUM(IF(dia = 7, 1, 0)) * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS sabado, SUM(IF(dia = 1, 1, 0)) * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND linea = 0 AND maquina = 0 LIMIT 1), 0) AS domingo FROM " & rutaBD & ".dias WHERE  fecha >= '" & fDesde & "' AND fecha <= '" & fHasta & "' ) AS e " & IIf(modulo = 1, "", "WHERE a.id > 0 " & IIf(periodo <> 1, filtroMTBF_tec, ""))
            End If

        ElseIf modulo = 4 Then
            sentencia = "SELECT '" & grupo & "' AS nombre, 0 AS linea, '' AS referencia, COUNT(a.id) AS tmaquinas, IFNULL(SUM(i.paros), 0) AS paros_m, IFNULL(SUM(i.produccion), 0) AS produccion_m, IFNULL(SUM(i.piezas), 0) AS piezas_m, IFNULL(SUM(i.rechazos), 0) AS rechazos_m, IFNULL(SUM(i.calidad), 0) AS calidad_m, IFNULL(SUM(i.disponible), 0) AS disponible_m FROM " & rutaBD & ".cat_maquinas a LEFT JOIN " & rutaBD & ".cat_lineas b ON a.linea = b.id LEFT JOIN (SELECT i.equipo, SUM(i.paro) AS paros, SUM(i.produccion_tc) AS produccion, SUM(i.produccion) AS piezas, SUM(i.calidad) AS rechazos, SUM(i.tiempo_disponible) AS disponible, SUM(i.calidad_tc) AS calidad FROM (SELECT a.equipo, a.fecha AS dia, 0 AS paro, a.parte, a.turno, a.lote AS orden, 0 AS produccion_tc, 0 AS produccion, a.cantidad AS calidad, a.cantidad_tc AS calidad_tc, 0 AS tiempo_disponible FROM " & rutaBD & ".detallerechazos a INNER JOIN " & rutaBD & ".lecturas_cortes b ON a.corte = b.id WHERE a.fecha >= '" & fDesde & "' AND a.fecha <= '" & fHasta & "' UNION ALL SELECT equipo, dia, 0 AS paro, parte, turno, orden, 0 AS produccion_tc, 0 AS produccion, calidad, calidad_tc, 0 AS tiempo_disponible FROM " & rutaBD & ".lecturas_cortes WHERE calidad - calidad_clasificada > 0 AND dia >= '" & fDesde & "' AND dia <= '" & fHasta & "' UNION ALL SELECT equipo, dia, paro, parte, turno, orden, produccion_tc, produccion, 0 AS calidad, 0 AS calidad_tc, tiempo_disponible FROM " & rutaBD & ".lecturas_cortes WHERE dia >= '" & fDesde & "' AND dia <= '" & fHasta & "') AS i WHERE i.dia >= '" & fDesde & "' AND i.dia <= '" & fHasta & "' " & IIf(periodo <> 1, filtroCorte, "") + " GROUP BY i.equipo) AS i ON i.equipo = a.id WHERE a.oee = 'S' " & IIf(periodo <> 1, filtroOEE, "")
            If idReporte = 2 Then
                sentencia = "SELECT 0 AS id, '" & grupo & "' AS nombre, '' AS referencia, 0 AS tmaquinas, IFNULL(b.nombre, '" & traduccion(100) & "') AS nlinea, a.linea, IFNULL(SUM(i.paros), 0) AS paros_m, IFNULL(SUM(i.produccion), 0) AS produccion_m, IFNULL(SUM(i.piezas), 0) AS piezas_m, IFNULL(SUM(i.rechazos), 0) AS rechazos_m, IFNULL(SUM(i.calidad), 0) AS calidad_m, IFNULL(SUM(i.disponible), 0) AS disponible_m FROM " & rutaBD & ".cat_maquinas a LEFT JOIN " & rutaBD & ".cat_lineas b ON a.linea = b.id LEFT JOIN (SELECT i.equipo, SUM(i.paro) AS paros, SUM(i.produccion_tc) AS produccion, SUM(i.produccion) AS piezas, SUM(i.calidad) AS rechazos, SUM(i.tiempo_disponible) AS disponible, SUM(i.calidad_tc) AS calidad  FROM (SELECT a.equipo, a.fecha AS dia, 0 AS paro, a.parte, a.turno, a.lote AS orden, 0 AS produccion_tc, 0 AS produccion, a.cantidad AS calidad, a.cantidad_tc AS calidad_tc, 0 AS tiempo_disponible FROM " & rutaBD & ".detallerechazos a INNER JOIN " & rutaBD & ".lecturas_cortes b ON a.corte = b.id WHERE a.fecha >= '" & fDesde & "' AND a.fecha <= '" & fHasta & "' UNION ALL SELECT equipo, dia, 0 AS paro, parte, turno, orden, 0 AS produccion_tc, 0 AS produccion, calidad, calidad_tc, 0 AS tiempo_disponible FROM " & rutaBD & ".lecturas_cortes WHERE calidad - calidad_clasificada > 0 AND dia >= '" & fDesde & "' AND dia <= '" & fHasta & "' UNION ALL SELECT equipo, dia, paro, parte, turno, orden, produccion_tc, produccion, 0 AS calidad, 0 AS calidad_tc, tiempo_disponible FROM " & rutaBD & ".lecturas_cortes WHERE dia >= '" & fDesde & "' AND dia <= '" & fHasta & "') AS i INNER JOIN " & rutaBD & ".cat_maquinas j ON i.equipo = j.id WHERE i.dia >= '" & fDesde & "' AND i.dia <= '" & fHasta & "' " & IIf(periodo <> 1, filtroCorte, "") + " GROUP BY j.id) AS i ON i.equipo = a.id WHERE a.oee = 'S' " & IIf(periodo <> 1, filtroOEE, "")
            ElseIf idReporte = 3 Then

                sentencia = "SELECT '" & grupo & "' AS nombre, 0 AS linea, '' AS referencia, COUNT(a.id) AS tmaquinas, IFNULL(SUM(i.paros), 0) AS paros_m, IFNULL(SUM(i.produccion), 0) AS produccion_m, IFNULL(SUM(i.piezas), 0) AS piezas_m, IFNULL(SUM(i.rechazos), 0) AS rechazos_m, IFNULL(SUM(i.calidad), 0) AS calidad_m, IFNULL(SUM(i.disponible), 0) AS disponible_m FROM " & rutaBD & ".cat_maquinas a LEFT JOIN " & rutaBD & ".cat_generales b ON a.tipo = b.id AND b.tabla = 50 LEFT JOIN (SELECT i.equipo, SUM(i.paro) AS paros, SUM(i.produccion_tc) AS produccion, SUM(i.produccion) AS piezas, SUM(i.calidad) AS rechazos, SUM(i.tiempo_disponible) AS disponible, SUM(i.calidad_tc) AS calidad  FROM (SELECT a.equipo, a.fecha AS dia, 0 AS paro, a.parte, a.turno, a.lote AS orden, 0 AS produccion_tc, 0 AS produccion, a.cantidad AS calidad, a.cantidad_tc AS calidad_tc, 0 AS tiempo_disponible FROM " & rutaBD & ".detallerechazos a INNER JOIN " & rutaBD & ".lecturas_cortes b ON a.corte = b.id WHERE a.fecha >= '" & fDesde & "' AND a.fecha <= '" & fHasta & "' UNION ALL SELECT equipo, dia, 0 AS paro, parte, turno, orden, 0 AS produccion_tc, 0 AS produccion, calidad, calidad_tc, 0 AS tiempo_disponible FROM " & rutaBD & ".lecturas_cortes WHERE calidad - calidad_clasificada > 0 AND dia >= '" & fDesde & "' AND dia <= '" & fHasta & "' UNION ALL SELECT equipo, dia, paro, parte, turno, orden, produccion_tc, produccion, 0 AS calidad, 0 AS calidad_tc, tiempo_disponible FROM " & rutaBD & ".lecturas_cortes WHERE dia >= '" & fDesde & "' AND dia <= '" & fHasta & "') AS i WHERE i.dia >= '" & fDesde & "' AND i.dia <= '" & fHasta & "' " & IIf(periodo <> 1, filtroCorte, "") + " GROUP BY i.equipo) AS i ON i.equipo = a.id WHERE a.oee = 'S' " & IIf(periodo <> 1, filtroOEE, "")

            ElseIf idReporte = 4 Then

                sentencia = "SELECT '" & grupo & "' AS nombre, 0 AS linea, '' AS referencia, COUNT(a.id) AS tmaquinas, IFNULL(SUM(i.paros), 0) AS paros_m, IFNULL(SUM(i.produccion), 0) AS produccion_m, IFNULL(SUM(i.piezas), 0) AS piezas_m, IFNULL(SUM(i.rechazos), 0) AS rechazos_m, IFNULL(SUM(i.calidad), 0) AS calidad_m, IFNULL(SUM(i.disponible), 0) AS disponible_m FROM " & rutaBD & ".cat_maquinas a LEFT JOIN " & rutaBD & ".cat_generales b ON a.agrupador_1 = b.id AND b.tabla = 20 LEFT JOIN (SELECT i.equipo, SUM(i.paro) AS paros, SUM(i.produccion_tc) AS produccion, SUM(i.produccion) AS piezas, SUM(i.calidad) AS rechazos, SUM(i.tiempo_disponible) AS disponible, SUM(i.calidad_tc) AS calidad FROM (SELECT a.equipo, a.fecha AS dia, 0 AS paro, a.parte, a.turno, a.lote AS orden, 0 AS produccion_tc, 0 AS produccion, a.cantidad AS calidad, a.cantidad_tc AS calidad_tc, 0 AS tiempo_disponible FROM " & rutaBD & ".detallerechazos a INNER JOIN " & rutaBD & ".lecturas_cortes b ON a.corte = b.id WHERE a.fecha >= '" & fDesde & "' AND a.fecha <= '" & fHasta & "' UNION ALL SELECT equipo, dia, 0 AS paro, parte, turno, orden, 0 AS produccion_tc, 0 AS produccion, calidad, calidad_tc, 0 AS tiempo_disponible FROM " & rutaBD & ".lecturas_cortes WHERE calidad - calidad_clasificada > 0 AND dia >= '" & fDesde & "' AND dia <= '" & fHasta & "' UNION ALL SELECT equipo, dia, paro, parte, turno, orden, produccion_tc, produccion, 0 AS calidad, 0 AS calidad_tc, tiempo_disponible FROM " & rutaBD & ".lecturas_cortes WHERE dia >= '" & fDesde & "' AND dia <= '" & fHasta & "') AS i WHERE i.dia >= '" & fDesde & "' AND i.dia <= '" & fHasta & "' " & IIf(periodo <> 1, filtroCorte, "") + " GROUP BY i.equipo) AS i ON i.equipo = a.id WHERE a.oee = 'S' " & IIf(periodo <> 1, filtroOEE, "")

            ElseIf idReporte = 5 Then

                sentencia = "SELECT '" & grupo & "' AS nombre, 0 AS linea, '' AS referencia, COUNT(a.id) AS tmaquinas, IFNULL(SUM(i.paros), 0) AS paros_m, IFNULL(SUM(i.produccion), 0) AS produccion_m, IFNULL(SUM(i.piezas), 0) AS piezas_m, IFNULL(SUM(i.rechazos), 0) AS rechazos_m, IFNULL(SUM(i.calidad), 0) AS calidad_m, IFNULL(SUM(i.disponible), 0) AS disponible_m FROM " & rutaBD & ".cat_maquinas a LEFT JOIN " & rutaBD & ".cat_generales b ON a.agrupador_2 = b.id AND b.tabla = 25 LEFT JOIN (SELECT i.equipo, SUM(i.paro) AS paros, SUM(i.produccion_tc) AS produccion, SUM(i.produccion) AS piezas, SUM(i.calidad) AS rechazos, SUM(i.tiempo_disponible) AS disponible, SUM(i.calidad_tc) AS calidad FROM (SELECT a.equipo, a.fecha AS dia, 0 AS paro, a.parte, a.turno, a.lote AS orden, 0 AS produccion_tc, 0 AS produccion, a.cantidad AS calidad, a.cantidad_tc AS calidad_tc, 0 AS tiempo_disponible FROM " & rutaBD & ".detallerechazos a INNER JOIN " & rutaBD & ".lecturas_cortes b ON a.corte = b.id WHERE a.fecha >= '" & fDesde & "' AND a.fecha <= '" & fHasta & "' UNION ALL SELECT equipo, dia, 0 AS paro, parte, turno, orden, 0 AS produccion_tc, 0 AS produccion, calidad, calidad_tc, 0 AS tiempo_disponible FROM " & rutaBD & ".lecturas_cortes WHERE calidad - calidad_clasificada > 0 AND dia >= '" & fDesde & "' AND dia <= '" & fHasta & "' UNION ALL SELECT equipo, dia, paro, parte, turno, orden, produccion_tc, produccion, 0 AS calidad, 0 AS calidad_tc, tiempo_disponible FROM " & rutaBD & ".lecturas_cortes WHERE dia >= '" & fDesde & "' AND dia <= '" & fHasta & "') AS i WHERE i.dia >= '" & fDesde & "' AND i.dia <= '" & fHasta & "' " & IIf(periodo <> 1, filtroCorte, "") + " GROUP BY i.equipo) AS i ON i.equipo = a.id WHERE a.oee = 'S' " & IIf(periodo <> 1, filtroOEE, "")

            ElseIf idReporte = 6 Then

                sentencia = "SELECT '" & grupo & "' AS nombre, IFNULL(SUM(i.paros), 0) AS paros_m, IFNULL(SUM(i.produccion), 0) AS produccion_m, IFNULL(SUM(i.piezas), 0) AS piezas_m, IFNULL(SUM(i.rechazos), 0) AS rechazos_m, IFNULL(SUM(i.calidad), 0) AS calidad_m, IFNULL(SUM(i.disponible), 0) AS disponible_m FROM " & rutaBD & ".dias a LEFT JOIN (SELECT i.dia, SUM(i.paro) AS paros, SUM(i.produccion_tc) AS produccion, SUM(i.produccion) AS piezas, SUM(i.calidad) AS rechazos, SUM(i.tiempo_disponible) AS disponible, SUM(i.calidad_tc) AS calidad  FROM (SELECT a.equipo, a.fecha AS dia, 0 AS paro, a.parte, a.turno, a.lote AS orden, 0 AS produccion_tc, 0 AS produccion, a.cantidad AS calidad, a.cantidad_tc AS calidad_tc, 0 AS tiempo_disponible FROM " & rutaBD & ".detallerechazos a INNER JOIN " & rutaBD & ".lecturas_cortes b ON a.corte = b.id WHERE a.fecha >= '" & fDesde & "' AND a.fecha <= '" & fHasta & "' UNION ALL SELECT equipo, dia, 0 AS paro, parte, turno, orden, 0 AS produccion_tc, 0 AS produccion, calidad, calidad_tc, 0 AS tiempo_disponible FROM " & rutaBD & ".lecturas_cortes WHERE calidad - calidad_clasificada > 0 AND dia >= '" & fDesde & "' AND dia <= '" & fHasta & "' UNION ALL SELECT equipo, dia, paro, parte, turno, orden, produccion_tc, produccion, 0 AS calidad, 0 AS calidad_tc, tiempo_disponible FROM " & rutaBD & ".lecturas_cortes WHERE dia >= '" & fDesde & "' AND dia <= '" & fHasta & "') AS i INNER JOIN " & rutaBD & ".cat_maquinas j ON i.equipo = j.id AND j.oee = 'S' " & IIf(periodo <> 1, filtroOEEDias, "") + " WHERE i.dia >= '" & fDesde & "' AND i.dia <= '" & fHasta & "'  GROUP BY dia) AS i ON i.dia = a.fecha WHERE a.fecha >= '" & fDesde & "' AND a.fecha <= '" & fHasta & "' "

            ElseIf idReporte = 7 Then

                sentencia = "SELECT '" & grupo & "' AS nombre, '' AS inicio, IFNULL(SUM(i.paros), 0) AS paros_m, IFNULL(SUM(i.produccion), 0) AS produccion_m, IFNULL(SUM(i.piezas), 0) AS piezas_m, IFNULL(SUM(i.rechazos), 0) AS rechazos_m, IFNULL(SUM(i.calidad), 0) AS calidad_m, IFNULL(SUM(i.disponible), 0) AS disponible_m FROM " & rutaBD & ".dias a LEFT JOIN (SELECT i.dia, SUM(i.paro) AS paros, SUM(i.produccion_tc) AS produccion, SUM(i.produccion) AS piezas, SUM(i.calidad) AS rechazos, SUM(i.tiempo_disponible) AS disponible, SUM(i.calidad_tc) AS calidad  FROM (SELECT a.equipo, a.fecha AS dia, 0 AS paro, a.parte, a.turno, a.lote AS orden, 0 AS produccion_tc, 0 AS produccion, a.cantidad AS calidad, a.cantidad_tc AS calidad_tc, 0 AS tiempo_disponible FROM " & rutaBD & ".detallerechazos a INNER JOIN " & rutaBD & ".lecturas_cortes b ON a.corte = b.id WHERE a.fecha >= '" & fDesde & "' AND a.fecha <= '" & fHasta & "' UNION ALL SELECT equipo, dia, 0 AS paro, parte, turno, orden, 0 AS produccion_tc, 0 AS produccion, calidad, calidad_tc, 0 AS tiempo_disponible FROM " & rutaBD & ".lecturas_cortes WHERE calidad - calidad_clasificada > 0 AND dia >= '" & fDesde & "' AND dia <= '" & fHasta & "' UNION ALL SELECT equipo, dia, paro, parte, turno, orden, produccion_tc, produccion, 0 AS calidad, 0 AS calidad_tc, tiempo_disponible FROM " & rutaBD & ".lecturas_cortes WHERE dia >= '" & fDesde & "' AND dia <= '" & fHasta & "') AS i INNER JOIN " & rutaBD & ".cat_maquinas j ON i.equipo = j.id AND j.oee = 'S' " & IIf(periodo <> 1, filtroOEEDias, "") + " WHERE i.dia >= '" & fDesde & "' AND i.dia <= '" & fHasta & "'  GROUP BY dia) AS i ON i.dia = a.fecha WHERE a.fecha >= '" & fDesde & "' AND a.fecha <= '" & fHasta & "' "

            ElseIf idReporte = 8 Then

                sentencia = "SELECT '" & grupo & "' AS nombre, IFNULL(SUM(i.paros), 0) AS paros_m, IFNULL(SUM(i.produccion), 0) AS produccion_m, IFNULL(SUM(i.piezas), 0) AS piezas_m, IFNULL(SUM(i.rechazos), 0) AS rechazos_m, IFNULL(SUM(i.calidad), 0) AS calidad_m, IFNULL(SUM(i.disponible), 0) AS disponible_m FROM " & rutaBD & ".dias a LEFT JOIN (SELECT i.dia, SUM(i.paro) AS paros, SUM(i.produccion_tc) AS produccion, SUM(i.produccion) AS piezas, SUM(i.calidad) AS rechazos, SUM(i.tiempo_disponible) AS disponible, SUM(i.calidad_tc) AS calidad  FROM (SELECT a.equipo, a.fecha AS dia, 0 AS paro, a.parte, a.turno, a.lote AS orden, 0 AS produccion_tc, 0 AS produccion, a.cantidad AS calidad, a.cantidad_tc AS calidad_tc, 0 AS tiempo_disponible FROM " & rutaBD & ".detallerechazos a INNER JOIN " & rutaBD & ".lecturas_cortes b ON a.corte = b.id WHERE a.fecha >= '" & fDesde & "' AND a.fecha <= '" & fHasta & "' UNION ALL SELECT equipo, dia, 0 AS paro, parte, turno, orden, 0 AS produccion_tc, 0 AS produccion, calidad, calidad_tc, 0 AS tiempo_disponible FROM " & rutaBD & ".lecturas_cortes WHERE calidad - calidad_clasificada > 0 AND dia >= '" & fDesde & "' AND dia <= '" & fHasta & "' UNION ALL SELECT equipo, dia, paro, parte, turno, orden, produccion_tc, produccion, 0 AS calidad, 0 AS calidad_tc, tiempo_disponible FROM " & rutaBD & ".lecturas_cortes WHERE dia >= '" & fDesde & "' AND dia <= '" & fHasta & "') AS i INNER JOIN " & rutaBD & ".cat_maquinas j ON i.equipo = j.id AND j.oee = 'S' " & IIf(periodo <> 1, filtroOEEDias, "") + " WHERE i.dia >= '" & fDesde & "' AND i.dia <= '" & fHasta & "'  GROUP BY dia) AS i ON i.dia = a.fecha WHERE a.fecha >= '" & fDesde & "' AND a.fecha <= '" & fHasta & "' "
            End If
        End If

        Dim mensajesDS = consultaSEL(sentencia)
        If mensajesDS.Tables(0).Rows.Count > 0 Then
            For Each elmensaje As DataRow In mensajesDS.Tables(0).Rows
                If modulo <= 2 Then
                    Dim row As DataRow = agregados.NewRow
                    row("id") = "0"
                    row("nombre") = elmensaje!nombre
                    row("tdias") = elmensaje!tdias
                    row("tdisponible") = ValNull(elmensaje!tdisponible, "N")
                    row("docs") = elmensaje!docs
                    row("tiempo_c") = elmensaje!tiempo_c
                    row("mttrc") = elmensaje!mttrc
                    row("mtbfc") = ValNull(elmensaje!mtbfc, "N")
                    row("porcentaje") = 0
                    agregados.Rows.Add(row)
                End If
            Next
        End If



    End Sub
    Sub calcularPeriodos(periodo As String, nperiodos As Integer)
        eDesde = Now()
        eHasta = Now()
        sDesde = "-1"
        Dim ePeriodo = nperiodos
        Dim diaSemana = DateAndTime.Weekday(Now)
        Dim intervalo = DateInterval.Second
        cadPeriodo = nperiodos & traduccion(28)
        If periodo = 1 Then
            intervalo = DateInterval.Minute
            cadPeriodo = nperiodos & traduccion(29)
        ElseIf periodo = 2 Then
            intervalo = DateInterval.Hour
            cadPeriodo = nperiodos & traduccion(30)
        ElseIf periodo = 3 Then
            intervalo = DateInterval.Day
            cadPeriodo = nperiodos & traduccion(31)
        ElseIf periodo = 4 Then
            intervalo = DateInterval.Day
            ePeriodo = 6
            cadPeriodo = nperiodos & traduccion(32)
        ElseIf periodo = 5 Then
            intervalo = DateInterval.Month
            cadPeriodo = nperiodos & traduccion(33)
        ElseIf periodo = 6 Then
            intervalo = DateInterval.Year
            cadPeriodo = nperiodos & traduccion(34)
        ElseIf periodo = 7 Then
            intervalo = -9999
            cadPeriodo = nperiodos & traduccion(157)
        ElseIf periodo = 10 Then
            eDesde = CDate(Format(Now, "yyyy/MM/dd") & " 00:00:00")
            cadPeriodo = traduccion(35)
        ElseIf periodo = 14 Then
            sDesde = " = " & turno_serie
            cadPeriodo = traduccion(156)
        ElseIf periodo = 15 Then
            cadPeriodo = traduccion(176)
            sDesde = " >= " & IIf(secuencia = 1, turno_serie, IIf(secuencia = 2, turno_serie - 1, turno_serie - 2))
        ElseIf periodo = 11 Then
            cadPeriodo = traduccion(36)
            If diaSemana = 0 Then
                eDesde = CDate(Format(DateAdd(DateInterval.Day, -6, Now), "yyyy/MM/dd") & " 00:00:00")
            Else
                eDesde = CDate(Format(DateAdd(DateInterval.Day, (diaSemana - 2) * -1, Now), "yyyy/MM/dd") & " 00:00:00")
            End If
        ElseIf periodo = 12 Then
            cadPeriodo = traduccion(37)
            eDesde = CDate(Format(Now, "yyyy/MM") & "/01 00:00:00")
        ElseIf periodo = 13 Then
            cadPeriodo = traduccion(38)
            eDesde = CDate(Format(Now, "yyyy") & "/01/01 00:00:00")
        ElseIf periodo = 20 Then
            cadPeriodo = traduccion(39)
            eDesde = CDate(Format(DateAdd(DateInterval.Day, -1, Now), "yyyy/MM/dd") & " 00:00:00")
            eHasta = CDate(Format(DateAdd(DateInterval.Day, -1, Now), "yyyy/MM/dd") & " 23:59:59")
        ElseIf periodo = 21 Then
            cadPeriodo = traduccion(40)
            Dim dayDiff As Integer = Date.Today.DayOfWeek - DayOfWeek.Monday
            eDesde = CDate(Format(Today.AddDays(-dayDiff), "yyyy/MM/dd") & " 00:00:00")
            eDesde = DateAdd(DateInterval.Day, -7, CDate(eDesde))
            eHasta = DateAdd(DateInterval.Day, 6, CDate(eDesde))
        ElseIf periodo = 22 Then
            cadPeriodo = traduccion(41)
            eDesde = CDate(Format(DateAdd(DateInterval.Month, -1, Now), "yyyy/MM") & "/01 00:00:00")
            eHasta = CDate(Format(DateAdd(DateInterval.Day, -1, CDate(Format(Now, "yyyy/MM") & "/01")), "yyyy/MM/dd") & " 23:59:59")
        End If
        If periodo < 10 And periodo <> 7 Then eDesde = DateAdd(intervalo, ePeriodo * -1, eDesde)
        If periodo = 7 Then
            sDesde = ">= " & turno_serie - ePeriodo - 1
        End If
        Dim cadSQL As String = ""
        If sDesde <> "-1" Then
            cadSQL = "SELECT dia FROM " & rutaBD & ".historico_turnos WHERE secuencia = " & turno_serie
            Dim readerDS As DataSet = consultaSEL(cadSQL)
            If readerDS.Tables(0).Rows.Count > 0 Then
                eDesde = readerDS.Tables(0).Rows(0)!dia
            End If
            eDesde = CDate(Format(eDesde, "yyyy/MM/dd ") & "00:00:00")
        ElseIf periodo = 3 Then
            eDesde = CDate(Format(eDesde, "yyyy/MM/dd ") & "00:00:00")
            eHasta = CDate(Format(DateAdd(DateInterval.Day, -1, Now), "yyyy/MM/dd") & " 23:59:59")
        End If
        cadSQL = "SELECT nombre FROM " & rutaBD & ".cat_turnos WHERE id = " & turno_actual
        Dim general As DataSet = consultaSEL(cadSQL)
        If general.Tables(0).Rows.Count > 0 Then
            miTurno = ValNull(general.Tables(0).Rows(0)!nombre, "A")
        End If
        fDesdeSF = Format(eDesde, "yyyy/MM/dd")
        fHastaSF = Format(eHasta, "yyyy/MM/dd")

        filtroParos = ""
        filtroReportes = ""
    End Sub


    Function generarReporteTipo7(idReporte As Integer, reporte As String, fName As String, ruta As String, graficar As String, consulta As Long, modulo As Integer, grafica As Long) As Integer
        generarReporteTipo7 = 0

        Dim archivoSaliente = ruta & "\" & fName & ".csv"

        Try
            My.Computer.FileSystem.DeleteFile(archivoSaliente)


        Catch ex As Exception

        End Try

        archivoSaliente = archivoSaliente.Replace("\", "\\")

        Dim comillas = Microsoft.VisualBasic.Strings.Left(Chr(34), 1)

        Dim inicial = ""
        Dim registros = ""

        Dim cadSQL = ""

        Dim mensajesDS As DataSet
        Dim sentencia As String = ""
        Dim regsAfectados = 0

        Dim CadenaMail = ""
        Dim diaAnterior = DateAdd(DateInterval.Day, -1, Now())
        Dim horaAnterior = DateAdd(DateInterval.Hour, -1, Now())
        inicial = Chr(34)
        If idReporte = 401 Then
            inicial = inicial & traduccion(178) & " => " & miTurno
        ElseIf idReporte = 402 Then
            inicial = inicial & traduccion(179) & " => " & Format(horaAnterior, "ddd, dd-MMM-yyyy HH")
        ElseIf idReporte = 403 Then
            inicial = inicial & traduccion(180) & " => " & Format(diaAnterior, "ddd, dd-MMM-yyyy")
        End If
        inicial = inicial & Chr(34) & vbCrLf
        'inicial = Chr(34) & reporte & " (" & cadPeriodo & ")" & Chr(34) & vbCrLf
        inicial = inicial & Chr(34) & traduccion(110) & ": " & Format(Now(), "ddd, dd-MMM-yyyy HH:mm:ss") & Chr(34) & vbCrLf
        If secuencia <> 0 Then
            inicial = inicial & Chr(34) & traduccion(69) & ": " & miTurno & Chr(34) & vbCrLf
        Else
            inicial = inicial & Chr(34) & traduccion(111) & ": " & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & " " & traduccion(112) & ": " & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & Chr(34) & vbCrLf
        End If

        Dim cadTitulos = traduccion(177)

        Dim archivoEnv = archivoSaliente
        Dim filtroTurno = " AND a.secuencia " & sDesde & " "

        cadSQL = "SELECT a.*, b.nombre AS nequipo, i.nombre AS nparte, c.numero, IF(a.disponible = 3599, 3600, a.disponible) AS disponible2, IF(a.mantto = 3599, 3600, a.mantto) AS mantto2, IF(a.tiempo = 3599, 3600, a.tiempo) AS tiempo2, IF(a.paro= 3599, 3600, a.paro) AS paro2, d.nombre AS nturno, e.nombre AS ntripulacion, f.nombre AS ncausa, g.nombre AS nresp1, h.nombre AS nresp2 FROM " & rutaBD & ".horaxhora a LEFT JOIN " & rutaBD & ".cat_maquinas b ON a.equipo = b.id LEFT JOIN " & rutaBD & ".lotes c ON a.lote = c.id LEFT JOIN " & rutaBD & ".cat_turnos d ON a.turno = d.id LEFT JOIN " & rutaBD & ".cat_tripulacion e ON a.tripulacion_inicial = e.id LEFT JOIN " & rutaBD & ".cat_generales f ON a.causa = f.id LEFT JOIN " & rutaBD & ".cat_generales g ON a.responsable = g.id LEFT JOIN " & rutaBD & ".cat_generales h ON a.responsable2 = h.id LEFT JOIN " & rutaBD & ".cat_partes i ON a.parte = i.id WHERE a.estatus = 'Z' " & filtroTurno & " ORDER BY a.dia, a.hora, a.id"

        CadenaMail = traduccion(178) & " " & miTurno & vbCrLf & vbCrLf
        If idReporte = 402 Then
            CadenaMail = traduccion(179) & " " & Format(horaAnterior, "ddd, dd-MMM-yyyy HH") & vbCrLf & vbCrLf
        ElseIf idReporte = 403 Then
            CadenaMail = traduccion(180) & " " & Format(diaAnterior, "ddd, dd-MMM-yyyy") & vbCrLf & vbCrLf
        End If

        Dim totalParos As Double = 0
        Dim totalProduccion As Double = 0
        Dim totalCalidad As Double = 0
        Dim totalManual As Double = 0
        Dim totalDisponibilidad As Double = 0

        Dim ttotalParos As Double = 0
        Dim ttotalProduccion As Double = 0
        Dim ttotalCalidad As Double = 0
        Dim ttotalManual As Double = 0
        Dim ttotalDisponibilidad As Double = 0

        Dim requiereResumen = False

        Dim thDesde
        Dim thHasta

        Dim tplan As Double = 0
        Dim treal As Double = 0
        Dim ttMalas As Double = 0

        Dim linea As Integer = 0

        Dim objWriter As New System.IO.StreamWriter(archivoSaliente, False, System.Text.Encoding.UTF8)
        objWriter.WriteLine(inicial)
        objWriter.WriteLine(traduccion(57) & Format(Now(), "ddd dd-MMM-yyyy HH:mm:ss"))
        objWriter.WriteLine(cadTitulos)
        Dim cabecera As Boolean = False
        mensajesDS = consultaSEL(cadSQL)
        Dim oeeTA = 0

        If mensajesDS.Tables(0).Rows.Count > 0 Then
            For Each elmensaje In mensajesDS.Tables(0).Rows
                linea = linea + 1
                If Not cabecera Then
                    cabecera = True
                    If idReporte = 401 Then
                        CadenaMail = CadenaMail & traduccion(132) & ": " & Format(elmensaje!dia, "ddd, dd-MMM-yyyy") & " " & traduccion(69) & ": " & ValNull(elmensaje!nturno, "A") & vbCrLf
                    ElseIf idReporte = 402 Then
                        CadenaMail = CadenaMail & traduccion(132) & ": " & Format(elmensaje!dia, "ddd, dd-MMM-yyyy") & " " & traduccion(181) & ": " & Format(elmensaje!hora, "00") & vbCrLf
                    ElseIf idReporte = 403 Then
                        CadenaMail = CadenaMail & traduccion(132) & ": " & Format(elmensaje!dia, "ddd, dd-MMM-yyyy") & vbCrLf
                    End If
                End If
                Dim rendimiento As Double = 100
                Dim calidad As Double = 100
                Dim disponibilidad As Double = 100
                Dim oee As Double = 0

                If elmensaje!tiempo2 - elmensaje!paro2 > 0 Then
                    rendimiento = elmensaje!buenas_tc / (elmensaje!tiempo2 - elmensaje!paro2) * 100
                End If
                tplan = tplan + elmensaje!plan
                treal = treal + elmensaje!buenas

                If elmensaje!buenas_tc > 0 And elmensaje!malas_tc > 0 Then
                    calidad = (1 - elmensaje!malas_tc / elmensaje!buenas_tc) * 100
                End If
                Dim tcReal As Double = elmensaje!tc
                If elmensaje!tiempo2 > 0 Then
                    disponibilidad = (1 - elmensaje!paro2 / elmensaje!tiempo2) * 100
                End If
                If elmensaje!buenas > 0 Then
                    tcReal = elmensaje!tiempo2 / elmensaje!buenas
                End If
                totalParos = totalParos + elmensaje!paro2
                totalProduccion = totalProduccion + elmensaje!buenas_tc
                totalCalidad = totalCalidad + elmensaje!malas_tc
                totalDisponibilidad = totalDisponibilidad + elmensaje!tiempo2
                Dim adherencia As Double = 0
                If elmensaje!buenas_tc = 0 Then
                    rendimiento = 0
                    calidad = 0
                End If
                If elmensaje!plan > 0 Then
                    adherencia = elmensaje!buenas / elmensaje!plan * 100
                End If
                oee = IIf(rendimiento > 100, 100, rendimiento) * calidad * disponibilidad / 10000
                Dim cadReporte = ""
                Try
                    cadReporte = linea & "," & Chr(34) & IIf(elmensaje!ruptura = 0, traduccion(182), IIf(elmensaje!ruptura = 1, traduccion(69), IIf(elmensaje!ruptura = 2, traduccion(51), IIf(elmensaje!ruptura = 3, traduccion(124), IIf(elmensaje!ruptura = 4, traduccion(183), traduccion(184)))))) & Chr(34) & "," & Chr(34) & Format(elmensaje!dia, "dd-MMM-yyyy") & Chr(34) & "," & Chr(34) & ValNull(elmensaje!nequipo, "A") & Chr(34) & "," & Chr(34) & ValNull(elmensaje!numero, "A") & Chr(34) & "," & Chr(34) & ValNull(elmensaje!nparte, "A") & Chr(34) & "," & Chr(34) & ValNull(elmensaje!nturno, "A") & Chr(34) & "," & Chr(34) & elmensaje!hora & Chr(34) & "," & Chr(34) & elmensaje!desde & Chr(34) & "," & Chr(34) & elmensaje!hasta & Chr(34) & "," & Chr(34) & elmensaje!tiempo2 & Chr(34) & "," & Chr(34) & elmensaje!paro2 & Chr(34) & "," & Chr(34) & elmensaje!tc & Chr(34) & "," & Chr(34) & elmensaje!plan & Chr(34) & "," & Chr(34) & elmensaje!plan_van & Chr(34) & "," & Chr(34) & elmensaje!buenas - elmensaje!malas & Chr(34) & "," & Chr(34) & elmensaje!malas & Chr(34) & "," & Chr(34) & elmensaje!buenas & Chr(34) & "," & Chr(34) & (elmensaje!plan - elmensaje!buenas) * -1 & Chr(34) & "," & Chr(34) & adherencia & Chr(34) & "," & Chr(34) & (elmensaje!buenas_vienen) & Chr(34) & "," & Chr(34) & rendimiento & Chr(34) & "," & Chr(34) & calidad & Chr(34) & "," & Chr(34) & disponibilidad & Chr(34) & "," & Chr(34) & oee & Chr(34) & "," & Chr(34) & elmensaje!scrap & Chr(34) & "," & Chr(34) & tcReal & Chr(34) & "," & Chr(34) & elmensaje!diferencia_vienen & Chr(34) & "," & Chr(34) & IIf(elmensaje!tipo = 0, traduccion(102), traduccion(101)) & Chr(34) & "," & Chr(34) & ValNull(elmensaje!ntripulacion, "A") & Chr(34) & "," & Chr(34) & ValNull(elmensaje!comentarios, "A") & Chr(34) & "," & Chr(34) & ValNull(elmensaje!ncausa, "A") & Chr(34) & "," & Chr(34) & ValNull(elmensaje!nresp1, "A") & Chr(34) & "," & Chr(34) & ValNull(elmensaje!nresp2, "A") & Chr(34)

                Catch ex As Exception
                    'MsgBox(ex.Message)
                End Try
                objWriter.WriteLine(cadReporte)
            Next

            Dim tRendimiento As Double = 100
            Dim tCalidad As Double = 100
            Dim tDisponibilidad As Double = 100
            If totalDisponibilidad - totalParos > 0 Then
                tRendimiento = totalProduccion / (totalDisponibilidad - totalParos) * 100
            End If
            If totalProduccion > 0 Then
                tCalidad = (1 - totalCalidad / totalProduccion) * 100
            End If
            If totalDisponibilidad > 0 Then
                tDisponibilidad = (1 - totalParos / totalDisponibilidad) * 100
            End If
            oeeTA = IIf(tRendimiento > 100, 100, tRendimiento) * tCalidad * tDisponibilidad / 10000
            If idReporte = 401 Then
                nuevoTitulo = traduccion(178) & " " & miTurno & ": " & oeeTA.ToString("0.###") & "%"

            ElseIf idReporte = 402 Then
                nuevoTitulo = traduccion(179) & " " & Format(diaAnterior, "yyyy/MM/dd") & " " & traduccion(182) & ": " & Format(diaAnterior, "HH") & ": " & oeeTA.ToString("0.###") & "%"

            ElseIf idReporte = 403 Then
                nuevoTitulo = traduccion(180) & " " & Format(diaAnterior, "yyyy/MM/dd") & ": " & oeeTA.ToString("0.###") & "%"

            End If
            agregarLOG(traduccion(189), 9, 0)
        Else
            If idReporte = 401 Then
                nuevoTitulo = traduccion(178) & " " & miTurno & ": " & traduccion(185)

            ElseIf idReporte = 402 Then
                nuevoTitulo = traduccion(179) & " " & Format(diaAnterior, "yyyy/MM/dd") & " " & traduccion(182) & ": " & Format(diaAnterior, "HH") & ": " & traduccion(185)

            ElseIf idReporte = 403 Then
                nuevoTitulo = traduccion(180) & " " & Format(diaAnterior, "yyyy/MM/dd") & ": " & traduccion(185)

            End If
            CadenaMail = CadenaMail & traduccion(186)
            objWriter.WriteLine(traduccion(186))
            agregarLOG(traduccion(187), 9, 0)

        End If
        objWriter.WriteLine(traduccion(188))
        objWriter.Close()

        cadSQL = "SELECT a.equipo, b.nombre AS nequipo, a.dia, a.hora, MIN(a.desde) AS desde, MAX(a.hasta) AS hasta, SUM(a.plan) AS plan, MAX(a.plan_van) AS plan_van, SUM(a.scrap) AS scrap, GROUP_CONCAT(i.nombre SEPARATOR '~~') AS nparte, GROUP_CONCAT(c.numero SEPARATOR '~~') AS numero, SUM(IF(a.disponible = 3599, 3600, a.disponible)) AS disponible2, SUM(IF(a.mantto = 3599, 3600, a.mantto)) AS mantto2, SUM(IF(a.tiempo = 3599, 3600, a.tiempo)) AS tiempo2, SUM(IF(a.paro= 3599, 3600, a.paro)) AS paro2, d.nombre AS nturno, GROUP_CONCAT(f.nombre  SEPARATOR '~~') AS ncausa, GROUP_CONCAT(g.nombre SEPARATOR '~~') AS nresp1, GROUP_CONCAT(h.nombre SEPARATOR '~~') AS nresp2, GROUP_CONCAT(a.comentarios SEPARATOR '~~') AS comentarios, SUM(a.buenas) AS buenas, SUM(a.buenas_tc) AS buenas_tc, SUM(a.malas) AS malas, SUM(a.malas_tc) AS malas_tc FROM " & rutaBD & ".horaxhora a LEFT JOIN " & rutaBD & ".cat_maquinas b ON a.equipo = b.id LEFT JOIN " & rutaBD & ".lotes c ON a.lote = c.id LEFT JOIN " & rutaBD & ".cat_turnos d ON a.turno = d.id LEFT JOIN " & rutaBD & ".cat_tripulacion e ON a.tripulacion_inicial = e.id LEFT JOIN " & rutaBD & ".cat_generales f ON a.causa = f.id LEFT JOIN " & rutaBD & ".cat_generales g ON a.responsable = g.id LEFT JOIN " & rutaBD & ".cat_generales h ON a.responsable2 = h.id LEFT JOIN " & rutaBD & ".cat_partes i ON a.parte = i.id WHERE a.estatus = 'Z' " & filtroTurno & " GROUP BY a.equipo, a.dia, a.hora"

        ''''Construcción del mail
        If idReporte = 401 Then
            CadenaMail = traduccion(178) & vbCrLf & vbCrLf

        ElseIf idreporte = 402 Then
            CadenaMail = traduccion(179) & vbCrLf & vbCrLf
        ElseIf idreporte = 403 Then
            CadenaMail = traduccion(180) & vbCrLf & vbCrLf
        End If


        totalParos = 0
        totalProduccion = 0
        totalCalidad = 0
        totalManual = 0
        totalDisponibilidad = 0
        tplan = 0
        treal = 0
        ttMalas = 0
        Dim ttPlan = 0
        Dim ttReal = 0

        mensajesDS = consultaSEL(cadSQL)
        If mensajesDS.Tables(0).Rows.Count > 0 Then

            'MsgBox("hay datos")

            cabecera = False
            linea = 0
            Dim plan = 0
            Dim real = 0
            Dim tMalas = 0

            Dim hDesde
            Dim hHasta = Convert.ToDateTime(Format(mensajesDS.Tables(0).Rows(0)!dia, "yyyy/MM/dd") & " " & mensajesDS.Tables(0).Rows(0)!desde)
            Dim printhHasta = ""
            Dim equipoActual = mensajesDS.Tables(0).Rows(0)!equipo


            myBuilder.Append("<table border='1px' cellpadding='5' cellspacing='0' ")
            myBuilder.Append("style='border: solid 1px DarkGray; font-size: x-medium;'>")

            hDesde = mensajesDS.Tables(0).Rows(0)!desde
            thDesde = mensajesDS.Tables(0).Rows(0)!desde

            For Each elmensaje In mensajesDS.Tables(0).Rows
                If equipoActual <> elmensaje!equipo Then
                    requiereResumen = True
                    cabecera = False
                    equipoActual = elmensaje!equipo
                    'Cabecera
                    '
                    If linea > 1 Then
                        Dim tRendimiento As Double = 100
                        Dim tEficiencia As Double = 0
                        Dim tCalidad As Double = 100
                        Dim tScrap As Double = 0
                        Dim tScrapManual As Double = 0
                        Dim tParos As Double = 0
                        linea = 0
                        Dim tDisponibilidad = 100
                        If totalDisponibilidad - totalParos <> 0 Then
                            tRendimiento = totalProduccion / (totalDisponibilidad - totalParos) * 100
                        End If
                        If tplan <> 0 Then
                            tEficiencia = treal / tplan * 100
                        End If
                        If totalProduccion > 0 Then
                            tCalidad = (1 - totalCalidad / totalProduccion) * 100
                            tScrap = totalCalidad / totalProduccion * 100
                        End If

                        If (treal - tMalas + totalManual) > 0 Then
                            tScrapManual = totalManual / (treal - tMalas + totalManual) * 100
                        End If

                        If totalDisponibilidad > 0 Then
                            tDisponibilidad = (1 - totalParos / totalDisponibilidad) * 100
                            tParos = totalParos / totalDisponibilidad * 100
                        End If

                        If totalProduccion = 0 Then
                            tRendimiento = 0
                            tCalidad = 0
                        End If

                        oeeTA = IIf(tRendimiento > 100, 100, tRendimiento) * tCalidad * tDisponibilidad / 10000

                        If tDisponibilidad = 0 And totalProduccion > 0 Then
                            oeeTA = IIf(tRendimiento > 100, 100, tRendimiento) * tCalidad / 100
                        End If


                        myBuilder.Append("<tr align='left' valign='top' style='background-color:#DCDCDC;border:solid 1px DarkGray;font-weight: 600'>")

                        myBuilder.Append("<td align='center' valign='top'>")
                        myBuilder.Append(hDesde & " " & hHasta)
                        myBuilder.Append("</td>")

                        myBuilder.Append("<td align='center' valign='top' style='border:solid 1px DarkGray'>")
                        myBuilder.Append(IIf(totalDisponibilidad > 0, calcularTiempoCad(totalDisponibilidad), "0"))
                        myBuilder.Append("</td>")

                        myBuilder.Append("<td align='right' valign='top'>")
                        myBuilder.Append(Math.Round(plan, 0))
                        myBuilder.Append("</td>")

                        myBuilder.Append("<td align='right' valign='top'>")
                        myBuilder.Append(Math.Round(plan))
                        myBuilder.Append("</td>")
                        '
                        myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray'>")
                        myBuilder.Append(Math.Round(real))
                        myBuilder.Append("</td>")

                        myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray'>")
                        myBuilder.Append(Math.Round(real))
                        myBuilder.Append("</td>")
                        '''
                        myBuilder.Append("<td align='center' valign='top'>")
                        myBuilder.Append(Math.Round(tEficiencia) & "%")
                        myBuilder.Append("</td>")

                        myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray'>")
                        myBuilder.Append(Math.Round(tMalas))
                        myBuilder.Append("</td>")

                        myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray'>")
                        myBuilder.Append(Math.Round(tScrap, 0) & "%")
                        myBuilder.Append("</td>")

                        myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray'>")
                        myBuilder.Append(Math.Round(totalManual))
                        myBuilder.Append("</td>")

                        myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray'>")
                        myBuilder.Append(Math.Round(tScrapManual, 0) & "%")
                        myBuilder.Append("</td>")


                        myBuilder.Append("<td align='right' valign='top'>")
                        myBuilder.Append(IIf(totalParos > 0, calcularTiempoCad(totalParos), "0"))
                        myBuilder.Append("</td>")

                        myBuilder.Append("<td align='right' valign='top'>")
                        myBuilder.Append(Math.Round(tParos, 0) & "%")
                        myBuilder.Append("</td>")

                        myBuilder.Append("<td align='center' valign='top' style='border:solid 1px DarkGray;font-weight: 700'>")
                        myBuilder.Append(Math.Round(oeeTA, 0))
                        myBuilder.Append("</td>")

                        myBuilder.Append("<td align='center' valign='top'>")
                        myBuilder.Append("")
                        myBuilder.Append("</td>")

                        myBuilder.Append("</tr>")
                    End If
                    totalParos = 0
                    totalProduccion = 0
                    totalCalidad = 0
                    totalDisponibilidad = 0
                    totalManual = 0
                    linea = 0
                    tplan = 0
                    treal = 0
                    tMalas = 0
                End If
                linea = linea + 1
                If Not cabecera Then
                    'MsgBox("cabecera")
                    cabecera = True
                    If hDesde < elmensaje!desde Then
                        hDesde = elmensaje!desde
                    End If
                    myBuilder.Append("<tr align='left' valign='top' style='background-color:#DCDCDC;border: solid 1px DarkGray;font-weight: 600'>")
                    '''
                    myBuilder.Append("<td align='left' valign='top' colspan='7' style='border:solid 1px DarkGray'>")
                    myBuilder.Append(traduccion(81) & ": " & ValNull(elmensaje!nequipo, "A"))
                    myBuilder.Append("</td>")
                    myBuilder.Append("<td align='left' valign='top' colspan='7'style='border:solid 1px DarkGray'>")
                    myBuilder.Append(traduccion(132) & ": " & Format(Now(), "ddd, dd-MMM-yyyy"))
                    myBuilder.Append("</td>")
                    '''
                    myBuilder.Append("</tr>")

                    myBuilder.Append("<tr align='left' valign='top' style='background-color:#DCDCDC;border: solid 1px DarkGray;font-weight: 600'>")
                    '''
                    myBuilder.Append("<td align='center' valign='top' style='border: solid 1px DarkGray;'>")
                    myBuilder.Append(traduccion(182))
                    myBuilder.Append("</td>")
                    myBuilder.Append("<td align='left' valign='top' style='border: solid 1px DarkGray;'>")
                    myBuilder.Append(traduccion(69))
                    myBuilder.Append("</td>")
                    myBuilder.Append("<td align='center' valign='top' style='border: solid 1px DarkGray;'>")
                    myBuilder.Append(traduccion(191))
                    myBuilder.Append("</td>")
                    myBuilder.Append("<td align='center' valign='top' colspan='2' style='border: solid 1px DarkGray;'>")
                    myBuilder.Append(traduccion(192))
                    myBuilder.Append("</td>")
                    myBuilder.Append("<td align='center' valign='top' colspan='2' style='border: solid 1px DarkGray;'>")
                    myBuilder.Append(traduccion(193))
                    myBuilder.Append("</td>")
                    myBuilder.Append("<td align='center' valign='top' style='border: solid 1px DarkGray;'>")
                    myBuilder.Append(traduccion(194))
                    myBuilder.Append("</td>")
                    myBuilder.Append("<td align='center' valign='top' colspan='2' style='border: solid 1px DarkGray;'>")
                    myBuilder.Append(traduccion(195))
                    myBuilder.Append("</td>")
                    'myBuilder.Append("<td align='center' valign='top' colspan='2' style='border: solid 1px DarkGray;'>")
                    'myBuilder.Append("SCRAP (MANUAL)")
                    'myBuilder.Append("</td>")
                    myBuilder.Append("<td align='center' valign='top' colspan='2' style='border: solid 1px DarkGray;'>")
                    myBuilder.Append(traduccion(196))
                    myBuilder.Append("</td>")
                    myBuilder.Append("<td align='center' valign='top' style='border: solid 1px DarkGray;'>")
                    myBuilder.Append(traduccion(167))
                    myBuilder.Append("</td>")
                    myBuilder.Append("<td align='center' valign='top' style='border: solid 1px DarkGray;'>")
                    myBuilder.Append(traduccion(140))
                    myBuilder.Append("</td>")
                    '''
                    myBuilder.Append("</tr>")
                End If
                Dim rendimiento As Double = 100
                Dim calidad As Double = 100
                Dim scrap As Double = 0
                Dim scrapManual = 0
                Dim paros As Double = 0
                Dim disponibilidad As Double = 100
                Dim oee As Double = 0

                If elmensaje!tiempo2 - elmensaje!paro2 > 0 Then
                    rendimiento = elmensaje!buenas_tc / (elmensaje!tiempo2 - elmensaje!paro2) * 100
                End If
                If elmensaje!buenas_tc > 0 And elmensaje!malas_tc > 0 Then
                    calidad = (1 - elmensaje!malas_tc / elmensaje!buenas_tc) * 100
                    scrap = elmensaje!malas_tc / elmensaje!buenas_tc * 100
                End If
                If (elmensaje!buenas - elmensaje!malas + elmensaje!scrap) > 0 And elmensaje!scrap > 0 Then
                    scrapManual = elmensaje!scrap / (elmensaje!buenas - elmensaje!malas + elmensaje!scrap) * 100
                End If
                If elmensaje!tiempo2 > 0 Then
                    disponibilidad = (1 - elmensaje!paro2 / elmensaje!tiempo2) * 100
                    paros = elmensaje!paro2 / elmensaje!tiempo2 * 100
                End If
                totalParos = totalParos + elmensaje!paro2
                totalManual = totalManual + elmensaje!scrap
                totalProduccion = totalProduccion + elmensaje!buenas_tc
                totalCalidad = totalCalidad + elmensaje!malas_tc
                totalDisponibilidad = totalDisponibilidad + elmensaje!tiempo2
                '''
                ttotalParos = ttotalParos + elmensaje!paro2
                ttotalProduccion = ttotalProduccion + elmensaje!buenas_tc
                ttotalCalidad = ttotalCalidad + elmensaje!malas_tc
                ttotalDisponibilidad = ttotalDisponibilidad + elmensaje!tiempo2
                ttotalManual = ttotalManual + elmensaje!scrap
                '''
                Dim adherencia As Double = 0
                If elmensaje!plan > 0 Then
                    adherencia = elmensaje!buenas / elmensaje!plan * 100
                End If
                If elmensaje!buenas_tc = 0 Then
                    rendimiento = 0
                    calidad = 0
                End If
                oee = IIf(rendimiento > 100, 100, rendimiento) * calidad * disponibilidad / 10000

                If disponibilidad = 0 And elmensaje!buenas_tc > 0 Then
                    oee = IIf(rendimiento > 100, 100, rendimiento) * calidad / 100
                End If

                tMalas = tMalas + elmensaje!malas
                ttMalas = ttMalas + elmensaje!malas

                'Linea
                If linea Mod 2 = 0 Then
                    myBuilder.Append("<tr align='left' valign='top' style='background-color:AliceBlue;'>")
                Else
                    myBuilder.Append("<tr align='left' valign='top' style='background-color:none;'>")

                End If

                myBuilder.Append("<td align='center' valign='top' style='border:solid 1px DarkGray;'>")
                myBuilder.Append(elmensaje!desde & " " & elmensaje!hasta)
                myBuilder.Append("</td>")

                myBuilder.Append("<td align='left' valign='top' style='border:solid 1px DarkGray;'>")
                myBuilder.Append(ValNull(elmensaje!nturno, "A"))
                myBuilder.Append("</td>")

                myBuilder.Append("<td align='center' valign='top' style='border:solid 1px DarkGray;'>")
                myBuilder.Append(IIf(elmensaje!tiempo2 > 0, calcularTiempoCad(elmensaje!tiempo2), "0"))
                myBuilder.Append("</td>")

                plan = plan + elmensaje!plan
                real = real + elmensaje!buenas

                tplan = tplan + elmensaje!plan
                treal = treal + elmensaje!buenas

                ttPlan = ttPlan + elmensaje!plan
                ttReal = ttReal + elmensaje!buenas

                myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray;'>")
                myBuilder.Append(Math.Round(elmensaje!plan, 0))
                myBuilder.Append("</td>")

                myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray;'>")
                myBuilder.Append(Math.Round(plan))
                myBuilder.Append("</td>")
                '
                myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray;'>")
                myBuilder.Append(Math.Round(elmensaje!buenas))
                myBuilder.Append("</td>")

                myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray;'>")
                myBuilder.Append(Math.Round(real))
                myBuilder.Append("</td>")
                '''
                myBuilder.Append("<td align='center' valign='top' style='border:solid 1px DarkGray;'>")
                myBuilder.Append(Math.Round(adherencia, 0) & "%")
                myBuilder.Append("</td>")
                Dim colorMalas = "none"
                If elmensaje!malas > 0 Then
                    colorMalas = "#FADBD8"
                End If
                myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray;background-color:" & colorMalas & "'>")
                myBuilder.Append(Math.Round(elmensaje!malas))
                myBuilder.Append("</td>")

                myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray;'>")
                myBuilder.Append(Math.Round(scrap, 2) & "%")
                myBuilder.Append("</td>")

                'myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray;background-color:" & IIf(elmensaje!scrap > 0, "#FADBD8", "none") & "'>")
                'myBuilder.Append(Math.Round(elmensaje!scrap))
                'myBuilder.Append("</td>")

                'myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray;'>")
                'myBuilder.Append(Math.Round(scrapManual, 0) & "%")
                'myBuilder.Append("</td>")

                'columna compartida
                colorMalas = "none"
                If elmensaje!paro2 > 0 Then
                    colorMalas = "#FADBD8"
                End If

                myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray;background-color:" & colorMalas & "'>")
                myBuilder.Append(IIf(elmensaje!paro2 > 0, calcularTiempoCad(elmensaje!paro2), "0"))
                myBuilder.Append("</td>")

                myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray;'>")
                myBuilder.Append(Math.Round(paros, 0) & "%")
                myBuilder.Append("</td>")

                myBuilder.Append("<td align='center' valign='top' style='border:solid 1px DarkGray;font-weight: 700'>")
                myBuilder.Append(Math.Round(oee, 0))
                myBuilder.Append("</td>")

                Dim cadComentarios As String = Strings.Replace(ValNull(elmensaje!comentarios, "A"), "~~", "<br>")
                Dim causa As String = Strings.Replace(ValNull(elmensaje!ncausa, "A"), "~~", "<br>")
                Dim resp1 As String = Strings.Replace(ValNull(elmensaje!nresp1, "A"), "~~", "<br>")
                Dim resp2 As String = Strings.Replace(ValNull(elmensaje!nresp2, "A"), "~~", "<br>")
                If Not IsNothing(causa) Then
                    If causa.Length > 0 Then
                        cadComentarios = cadComentarios & "<br>" & traduccion(197) & ": " & causa
                    End If
                End If
                If Not IsNothing(resp1) Then
                    If resp1.Length > 0 Then
                        cadComentarios = cadComentarios & "<br>" & traduccion(198) & ": " & resp1
                    End If

                End If
                If Not IsNothing(resp2) Then
                    If resp2.Length > 0 Then
                        cadComentarios = cadComentarios & "<br>" & traduccion(199) & ": " & resp2
                    End If
                End If
                myBuilder.Append("<td align='left' valign='top' style='border:solid 1px DarkGray;'>")
                myBuilder.Append(cadComentarios)
                myBuilder.Append("</td>")

                myBuilder.Append("</tr>")
                If hHasta < Convert.ToDateTime(Format(elmensaje!dia, "yyyy/MM/dd") & " " & elmensaje!hasta) Then
                    hHasta = Convert.ToDateTime(Format(elmensaje!dia, "yyyy/MM/dd") & " " & elmensaje!hasta)
                    printhHasta = elmensaje!hasta
                End If

                thHasta = elmensaje!hasta
            Next
            'MsgBox("termino el barrido")
            myBuilder.Append("</tr>")

            If linea > 1 Then
                'MsgBox("mas de una linea")
                Dim tRendimiento As Double = 100
                Dim tEficiencia As Double = 0
                Dim tCalidad As Double = 100
                Dim tScrap As Double = 0
                Dim tScrapManual As Double = 0
                Dim tParos As Double = 0
                Dim tDisponibilidad As Double = 100
                If totalDisponibilidad - totalParos <> 0 Then
                    tRendimiento = totalProduccion / (totalDisponibilidad - totalParos) * 100
                End If
                If tplan <> 0 Then
                    tEficiencia = treal / tplan * 100
                End If
                If ttotalProduccion > 0 Then
                    tCalidad = (1 - totalCalidad / totalProduccion) * 100
                    tScrap = totalCalidad / totalProduccion * 100
                End If
                If treal - tMalas + totalManual > 0 Then
                    tScrapManual = totalManual / (treal - tMalas + totalManual) * 100
                End If
                If totalDisponibilidad > 0 Then
                    tDisponibilidad = (1 - totalParos / totalDisponibilidad) * 100
                    tParos = totalParos / totalDisponibilidad * 100
                End If

                If totalProduccion = 0 Then
                    tRendimiento = 0
                    tCalidad = 0
                End If

                oeeTA = IIf(tRendimiento > 100, 100, tRendimiento) * tCalidad * tDisponibilidad / 10000

                If tDisponibilidad = 0 And totalProduccion > 0 Then
                    oeeTA = IIf(tRendimiento > 100, 100, tRendimiento) * tCalidad / 100
                End If

                myBuilder.Append("<tr align='left' valign='top' style='background-color:#DCDCDC;border:solid 1px DarkGray;font-weight: 600'>")

                myBuilder.Append("<td align='center' valign='top'>")
                myBuilder.Append(hDesde & " " & printhHasta)
                myBuilder.Append("</td>")

                myBuilder.Append("<td align='center' valign='top' style='border:solid 1px DarkGray'>")
                myBuilder.Append("")
                myBuilder.Append("</td>")

                myBuilder.Append("<td align='center' valign='top' style='border:solid 1px DarkGray'>")
                myBuilder.Append(IIf(totalDisponibilidad > 0, calcularTiempoCad(totalDisponibilidad), "0"))
                myBuilder.Append("</td>")


                myBuilder.Append("<td align='right' valign='top'>")
                myBuilder.Append(Math.Round(plan, 0))
                myBuilder.Append("</td>")

                myBuilder.Append("<td align='right' valign='top'>")
                myBuilder.Append("")
                myBuilder.Append("</td>")
                '
                myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray'>")
                myBuilder.Append(Math.Round(real))
                myBuilder.Append("</td>")

                myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray'>")
                myBuilder.Append("")
                myBuilder.Append("</td>")
                '''
                myBuilder.Append("<td align='center' valign='top'>")
                myBuilder.Append(Math.Round(tEficiencia, 0) & "%")
                myBuilder.Append("</td>")

                myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray'>")
                myBuilder.Append(Math.Round(tMalas))
                myBuilder.Append("</td>")

                myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray'>")
                myBuilder.Append(Math.Round(tScrap, 2) & "%")
                myBuilder.Append("</td>")

                'myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray'>")
                'myBuilder.Append(Math.Round(totalManual))
                'myBuilder.Append("</td>")

                'myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray'>")
                'myBuilder.Append(Math.Round(tScrapManual, 0) & "%")
                'myBuilder.Append("</td>")

                myBuilder.Append("<td align='right' valign='top'>")
                myBuilder.Append(IIf(totalParos > 0, calcularTiempoCad(totalParos), "0"))
                myBuilder.Append("</td>")

                myBuilder.Append("<td align='right' valign='top'>")
                myBuilder.Append(Math.Round(tParos, 0) & "%")
                myBuilder.Append("</td>")

                myBuilder.Append("<td align='center' valign='top' style='border:solid 1px DarkGray;font-weight: 700'>")
                myBuilder.Append(Math.Round(oeeTA, 0))
                myBuilder.Append("</td>")

                myBuilder.Append("<td align='center' valign='top'>")
                myBuilder.Append("")
                myBuilder.Append("</td>")

                myBuilder.Append("</tr>")

            End If

        End If
        If requiereResumen Then
            'MsgBox("resumen")
            Dim tRendimiento As Double = 100
            Dim tEficiencia As Double = 0
            Dim tCalidad As Double = 100
            Dim tScrap As Double = 0
            Dim tScrapManual As Double = 0
            Dim tParos As Double = 0
            Dim tDisponibilidad = 100
            If ttotalDisponibilidad - ttotalParos <> 0 Then
                tRendimiento = ttotalProduccion / (ttotalDisponibilidad - ttotalParos) * 100
            End If
            If ttPlan <> 0 Then
                tEficiencia = ttReal / ttPlan * 100
            End If
            If ttotalProduccion > 0 Then
                tCalidad = (1 - ttotalCalidad / ttotalProduccion) * 100
                tScrap = ttotalCalidad / ttotalProduccion * 100
            End If
            If ttReal - ttMalas + ttotalManual > 0 Then
                tScrapManual = ttotalManual / (ttReal - ttMalas + ttotalManual) * 100
            End If
            If ttotalDisponibilidad > 0 Then
                tDisponibilidad = (1 - ttotalParos / ttotalDisponibilidad) * 100
                tParos = ttotalParos / ttotalDisponibilidad * 100
            End If
            oeeTA = IIf(tRendimiento > 100, 100, tRendimiento) * tCalidad * tDisponibilidad / 10000

            myBuilder.Append("<tr align='left' valign='top' style='background-color:#DCDCDC;border:solid 1px DarkGray;font-weight: 600'>")

            myBuilder.Append("<td align='center' valign='top'>")
            myBuilder.Append(thDesde & " " & thHasta)
            myBuilder.Append("</td>")

            myBuilder.Append("<td align='center' valign='top' style='border:solid 1px DarkGray'>")
            myBuilder.Append("")
            myBuilder.Append("</td>")

            myBuilder.Append("<td align='center' valign='top' style='border:solid 1px DarkGray'>")
            myBuilder.Append(IIf(ttotalDisponibilidad > 0, calcularTiempoCad(ttotalDisponibilidad), "0"))
            myBuilder.Append("</td>")

            myBuilder.Append("<td align='right' valign='top'>")
            myBuilder.Append(Math.Round(tplan, 0))
            myBuilder.Append("</td>")

            myBuilder.Append("<td align='right' valign='top'>")
            myBuilder.Append(Math.Round(tplan))
            myBuilder.Append("</td>")
            '
            myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray'>")
            myBuilder.Append(Math.Round(treal))
            myBuilder.Append("</td>")

            myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray'>")
            myBuilder.Append(Math.Round(treal))
            myBuilder.Append("</td>")
            '''
            myBuilder.Append("<td align='center' valign='top'>")
            myBuilder.Append(Math.Round(tEficiencia, 0) & "%")
            myBuilder.Append("</td>")

            myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray'>")
            myBuilder.Append(Math.Round(ttMalas))
            myBuilder.Append("</td>")

            myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray'>")
            myBuilder.Append(Math.Round(tScrap, 2) & "%")
            myBuilder.Append("</td>")

            'myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray'>")
            'myBuilder.Append(Math.Round(ttotalManual))
            'myBuilder.Append("</td>")

            'myBuilder.Append("<td align='right' valign='top' style='border:solid 1px DarkGray'>")
            'myBuilder.Append(Math.Round(tScrapManual, 0) & "%")
            'myBuilder.Append("</td>")

            myBuilder.Append("<td align='right' valign='top'>")
            myBuilder.Append(IIf(ttotalParos > 0, calcularTiempoCad(ttotalParos), "0"))
            myBuilder.Append("</td>")

            myBuilder.Append("<td align='right' valign='top'>")
            myBuilder.Append(Math.Round(tParos, 0) & "%")
            myBuilder.Append("</td>")

            myBuilder.Append("<td align='center' valign='top' style='border:solid 1px DarkGray'>")
            myBuilder.Append(Math.Round(oeeTA, 0))
            myBuilder.Append("</td>")

            myBuilder.Append("<td align='center' valign='top'>")
            myBuilder.Append("")
            myBuilder.Append("</td>")

            myBuilder.Append("</tr>")
        End If
        myBuilder.Append("</table>")
        If idReporte = 401 Then
            nuevoTitulo = traduccion(178) & " " & miTurno & " => " & oeeTA.ToString("0.###") & "%"

        ElseIf idReporte = 402 Then
            nuevoTitulo = traduccion(179) & " " & Format(horaAnterior, "yyyy/MM/dd") & " hora: " & Format(horaAnterior, "HH") & " => " & oeeTA.ToString("0.###") & "%"

        ElseIf idReporte = 403 Then
            nuevoTitulo = traduccion(180) & " " & Format(diaAnterior, "yyyy/MM/dd") & " => " & oeeTA.ToString("0.###") & "%"
        End If
        agregarLOG(traduccion(189), 9, 0)


    End Function

    Function calcularTiempo(Seg) As String
        calcularTiempo = ""
        If Seg < 60 Then
            calcularTiempo = Seg & traduccion(24)
        ElseIf Seg < 3600 Then
            calcularTiempo = Math.Round(Seg / 60, 1) & traduccion(25)
        Else
            calcularTiempo = Math.Round(Seg / 3600, 1) & traduccion(26)
        End If
    End Function

    Function calcularTiempoCad(Seg) As String
        calcularTiempoCad = "-"
        Dim horas = Math.Floor(Seg / 3600)
        Dim minutos = Math.Floor((Seg Mod 3600) / 60)
        Dim segundos = (Seg Mod 3600) Mod 60
        calcularTiempoCad = horas & ":" & Format(minutos, "00") & ":" & Format(segundos, "00")
    End Function


End Class

