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
    Dim cad_consolidado As String = "CONSOLIDADO"
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


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim argumentos As String() = Environment.GetCommandLineArgs()
        If Process.GetProcessesByName _
          (Process.GetCurrentProcess.ProcessName).Length > 1 Then
        ElseIf argumentos.Length <= 1 Then
            MsgBox("No se puede iniciar el envío de correos: Se requiere la cadena de conexión", MsgBoxStyle.Critical, "SIGMA Monitor")
        Else
            cadenaConexion = argumentos(1)
            'cadenaConexion = "server=127.0.0.1;user id=root;password=usbw;port=3307;Convert Zero Datetime=True;Allow User Variables=True"
            Dim idProceso = Process.GetCurrentProcess.Id

            idProceso = Process.GetCurrentProcess.Id



            estadoPrograma = True
            enviarReportes()

        End If
        Application.Exit()
    End Sub

    Private Sub enviarReportes()
        'Se envía correo

        Dim cadSQL As String = "SELECT * FROM " & rutaBD & ".control WHERE fecha = '" & Format(Now, "yyyyMMddHH") & "'"
        Dim readerDS As DataSet = consultaSEL(cadSQL)
        If readerDS.Tables(0).Rows.Count > 0 Then
            Exit Sub
        End If
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

        cadSQL = "SELECT * FROM " & rutaBD & ".configuracion"
        readerDS = consultaSEL(cadSQL)
        If readerDS.Tables(0).Rows.Count > 0 Then
            Dim reader As DataRow = readerDS.Tables(0).Rows(0)
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
            If alto_etiqueta.Length = 0 Then alto_etiqueta = "Buenas"
            If escaladas_etiqueta.Length = 0 Then escaladas_etiqueta = "Escaladas"
            If noatendio_etiqueta.Length = 0 Then noatendio_etiqueta = "No atendidas"
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
            cadSQL = "Select * FROM " & rutaBD & ".cat_correos WHERE estatus = 'A'"
            'Se preselecciona la voz
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
                        Dim envio = elmensaje!extraccion.Split(New Char() {";"c})
                        'Se busca si hay uno del día y hra
                        If envio(2).Length > 0 And envio(3).Length > 0 Then
                            Dim enviarDia As Boolean = False
                            Dim diaSemana = DateAndTime.Weekday(Now)
                            Dim cadFrecuencia As String = "Este reporte se le envía Todos los días"
                            If envio(2) = "T" Then
                                enviarDia = True
                            ElseIf envio(2) = "LV" And diaSemana >= 2 And diaSemana <= 6 Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía de lunes a viernes"
                            ElseIf envio(2) = "L" And diaSemana = 2 Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía los lunes"
                            ElseIf envio(2) = "M" And diaSemana = 3 Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía los martes"
                            ElseIf envio(2) = "MI" And diaSemana = 4 Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía los miércoles"
                            ElseIf envio(2) = "J" And diaSemana = 5 Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía los jueves"
                            ElseIf envio(2) = "V" And diaSemana = 6 Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía los viernes"
                            ElseIf envio(2) = "S" And diaSemana = 7 Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía los sábados"
                            ElseIf envio(2) = "D" And diaSemana = 1 Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía los domingos"
                            ElseIf envio(2) = "1M" And Val(Today.Day) = 1 Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía el primer día del mes"
                            ElseIf envio(2) = "UM" And Val(Today.Day) = Date.DaysInMonth(Today.Year, Today.Month) Then
                                enviarDia = True
                                cadFrecuencia = "Este reporte se le envía el último día del mes"
                            End If

                            'eemv
                            'enviarDia = True


                            If enviarDia Then
                                Dim enviar As Boolean = False
                                Dim hora = Val(Format(Now, "HH"))
                                If envio(3) = "T" Then
                                    enviar = True
                                    cadFrecuencia = cadFrecuencia & " a cada hora"
                                ElseIf Val(envio(3)) = Val(hora) Then
                                    cadFrecuencia = cadFrecuencia & IIf(Val(hora) = 1, " a la 1:00am", "a las " & Val(hora) & " horas")
                                    enviar = True
                                End If


                                'eemv
                                'enviar = True


                                If enviar Then
                                    Dim mail As New MailMessage
                                    Try
                                        Dim cuerpo As String = ValNull(elmensaje!cuerpo, "A")
                                        Dim titulo As String = ValNull(elmensaje!titulo, "A")
                                        If titulo.Length = 0 Then titulo = "ANDON Reportes automáticos"
                                        If cuerpo.Length = 0 Then cuerpo = "Se le ha enviado este correo. No responda ya que esta cuenta no es monitoreada"

                                        mail.From = New MailAddress(correo_cuenta) 'TextBox1.Text & "@gmail.com")
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
                                        cuerpo = cuerpo & vbCrLf & "Reportes a enviar: "

                                        cadSQL = "SELECT a.reporte, b.nombre, b.grafica, b.file_name, b.grafica FROM " & rutaBD & ".det_correo a INNER JOIN " & rutaBD & ".int_listados b ON a.reporte = b.id WHERE a.correo = " & elmensaje!id & " ORDER BY b.orden"
                                        mensajesDS = consultaSEL(cadSQL)
                                        If mensajesDS.Tables(0).Rows.Count > 0 Then

                                            For Each reporte In mensajesDS.Tables(0).Rows
                                                Dim miReporte = generarReporte(reporte!reporte, reporte!nombre, reporte!file_name, envio(0), envio(1), rutaFiles, reporte!grafica)
                                                If miReporte = -1 Then
                                                    cuerpo = cuerpo & vbCrLf & reporte!nombre & " NO SE GENERÓ (por error) " & errorCorreos
                                                Else

                                                    If My.Computer.FileSystem.FileExists(rutaFiles & "\" & reporte!file_name & ".csv") Then
                                                        cuerpo = cuerpo & vbCrLf & reporte!nombre
                                                        Dim archivo As Attachment = New Attachment(rutaFiles & "\" & reporte!file_name & ".csv")
                                                        mail.Attachments.Add(archivo)
                                                    Else
                                                        cuerpo = cuerpo & vbCrLf & reporte!nombre & " NO SE GENERÓ (por datos) "
                                                    End If
                                                    If My.Computer.FileSystem.FileExists(rutaFiles & "\" & reporte!file_name & ".png") Then

                                                        Dim archivo As Attachment = New Attachment(rutaFiles & "\" & reporte!file_name & ".png")
                                                        mail.Attachments.Add(archivo)
                                                    End If
                                                End If
                                            Next
                                        End If
                                        cuerpo = cadFrecuencia & vbCrLf & vbCrLf & cuerpo
                                        mail.Body = cuerpo
                                        smtpServer.Send(mail)
                                        tMensajes = tMensajes + 1
                                        mensajeGenerado = True
                                    Catch ex As Exception
                                        agregarLOG("Hubo un error en la conexión al servidor de correos. El error es: " & ex.Message, 0, 9)
                                    End Try
                                Else
                                    mensajeGenerado = True
                                End If
                            End If
                        End If
                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".cat_correos SET ultimo_envio = '" & Format(Now, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & elmensaje!id)
                    Next
                End If
                If enlazado Then
                    If tMensajes > 0 Then
                        agregarLOG("Se enviaron " & tMensajes & " reporte(s) vía correo electrónico ")
                    Else
                        agregarLOG("No se enviaron reportes vía correo electrónico")
                    End If

                Else
                    agregarLOG("Hubo un error en la conexión al servidor de correos. El error es: " & errorCorreo, 0, 9)
                End If
                smtpServer.Dispose()
            End If
            regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".control (fecha, mensajes) VALUES ('" & Format(Now, "yyyyMMddHH") & "', " & tMensajes & ")")
        End If
    End Sub

    Function generarReporte(idReporte As Integer, reporte As String, fName As String, periodo As String, nperiodos As Integer, ruta As String, graficar As String) As Integer
        generarReporte = 0

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

        Dim eDesde = Now()
        Dim eHasta = Now()
        Dim ePeriodo = nperiodos
        Dim diaSemana = DateAndTime.Weekday(Now)
        Dim intervalo = DateInterval.Second
        Dim cadPeriodo As String = nperiodos & " segundo(s) atras"
        If periodo = 1 Then
            intervalo = DateInterval.Minute
            cadPeriodo = nperiodos & " minuto(s) atras"
        ElseIf periodo = 2 Then
            intervalo = DateInterval.Hour
            cadPeriodo = nperiodos & " hora(s) atras"
        ElseIf periodo = 3 Then
            intervalo = DateInterval.Day
            cadPeriodo = nperiodos & " día(s) atras"
        ElseIf periodo = 4 Then
            intervalo = DateInterval.Day
            ePeriodo = 6
            cadPeriodo = nperiodos & " semana(s) atras"
        ElseIf periodo = 5 Then
            intervalo = DateInterval.Month
            cadPeriodo = nperiodos & " mes(es) atras"
        ElseIf periodo = 6 Then
            intervalo = DateInterval.Year
            cadPeriodo = nperiodos & " año(s) atras"
        ElseIf periodo = 10 Then
            eDesde = CDate(Format(Now, "yyyy/MM/dd") & " 00:00:00")
            cadPeriodo = "Lo que va del día de hoy"
        ElseIf periodo = 11 Then
            cadPeriodo = "Lo que va de la semana"
            If diaSemana = 0 Then
                eDesde = CDate(Format(DateAdd(DateInterval.Day, -6, Now), "yyyy/MM/dd") & " 00:00:00")
            Else
                eDesde = CDate(Format(DateAdd(DateInterval.Day, (diaSemana - 2) * -1, Now), "yyyy/MM/dd") & " 00:00:00")
            End If
        ElseIf periodo = 12 Then
            cadPeriodo = "Lo que va del mes"
            eDesde = CDate(Format(Now, "yyyy/MM") & "/01 00:00:00")
        ElseIf periodo = 13 Then
            cadPeriodo = "Lo que va del anyo"
            eDesde = CDate(Format(Now, "yyyy") & "/01/01 00:00:00")
        ElseIf periodo = 20 Then
            cadPeriodo = "El día de ayer"
            eDesde = CDate(Format(DateAdd(DateInterval.Day, -1, Now), "yyyy/MM/dd") & " 00:00:00")
            eHasta = CDate(Format(DateAdd(DateInterval.Day, -1, Now), "yyyy/MM/dd") & " 23:59:59")
        ElseIf periodo = 21 Then
            cadPeriodo = "La semana pasada"
            Dim dayDiff As Integer = Date.Today.DayOfWeek - DayOfWeek.Monday
            eDesde = CDate(Format(Today.AddDays(-dayDiff), "yyyy/MM/dd") & " 00:00:00")
            eDesde = DateAdd(DateInterval.Day, -7, CDate(eDesde))
            eHasta = DateAdd(DateInterval.Day, 6, CDate(eDesde))
        ElseIf periodo = 22 Then
            cadPeriodo = "El mes pasado"
            eDesde = CDate(Format(DateAdd(DateInterval.Month, -1, Now), "yyyy/MM") & "/01 00:00:00")
            eHasta = CDate(Format(DateAdd(DateInterval.Day, -1, CDate(Format(Now, "yyyy/MM") & "/01")), "yyyy/MM/dd") & " 23:59:59")
        End If
        If periodo < 10 Then eDesde = DateAdd(intervalo, ePeriodo * -1, eDesde)
        Dim fDesde = Format(eDesde, "yyyy/MM/dd HH:mm:ss")
        Dim fHasta = Format(eHasta, "yyyy/MM/dd HH:mm:ss")
        Dim fDesdeSF = Format(eDesde, "yyyy/MM/dd")
        Dim fHastaSF = Format(eHasta, "yyyy/MM/dd")

        Dim filtroParos = " AND f.fecha >= '" & fDesdeSF & "' AND f.fecha <= '" & fHastaSF & "' "
        Dim filtroFechas = " fecha >= '" & fDesdeSF & "' AND fecha <= '" & fHastaSF & "' "
        Dim filtroReportes = " AND c.fecha_reporte >= '" & fDesdeSF & "' AND c.fecha_reporte <= '" & fHastaSF & "' "
        Dim filtroFechasDia = " a.fecha >= '" & fDesdeSF & "' AND a.fecha <= '" & fHastaSF & "' "

        Dim comillas = Microsoft.VisualBasic.Strings.Left(Chr(34), 1)

        Dim inicial = ""
        Dim cabecera = ""
        Dim registros = ""
        Dim cadTitulo = "Nombre de la linea"
        Dim cadTabla = "" & rutaBD & ".cat_lineas"
        Dim cadJoin = "c.linea"
        Dim cadCampo = ""
        Dim numTabla = ""
        Dim grupoFecha = ""
        Dim cadGrafico = ""
        Dim grupoFechaG = ""
        If idReporte = 2 Or idReporte = 15 Or idReporte = 28 Then
            cadTitulo = "Nombre de la maquina"
            cadTabla = "" & rutaBD & ".cat_maquinas"
            cadJoin = "c.maquina"
        ElseIf idReporte = 3 Or idReporte = 16 Or idReporte = 29 Then
            cadTitulo = "Nombre del area"
            cadTabla = "" & rutaBD & ".cat_areas"
            cadJoin = "c.area"
        ElseIf idReporte = 4 Or idReporte = 17 Or idReporte = 30 Then
            cadTitulo = "Nombre de la falla"
            cadTabla = "" & rutaBD & ".cat_fallas"
            cadJoin = "c.falla_ajustada"
        ElseIf idReporte = 5 Or idReporte = 18 Or idReporte = 31 Then
            cadTitulo = "Tipo de máquina"
            cadTabla = "" & rutaBD & ".cat_maquinas"
            cadJoin = "c.maquina"
            cadCampo = "g.tipo"
            numTabla = "50"
        ElseIf idReporte = 6 Or idReporte = 19 Or idReporte = 32 Then
            cadTitulo = "Agrupador (1) de máquina"
            cadTabla = "" & rutaBD & ".cat_maquinas"
            cadJoin = "c.maquina"
            cadCampo = "g.agrupador_1"
            numTabla = "20"
        ElseIf idReporte = 7 Or idReporte = 20 Or idReporte = 33 Then
            cadTitulo = "Agrupador (2) de máquina"
            cadTabla = "" & rutaBD & ".cat_maquinas"
            cadJoin = "c.maquina"
            cadCampo = "g.agrupador_2"
            numTabla = "25"
        ElseIf idReporte = 8 Or idReporte = 21 Or idReporte = 34 Then
            cadTitulo = "Agrupador (1) de falla"
            cadTabla = "" & rutaBD & ".cat_fallas"
            cadJoin = "c.falla_ajustada"
            cadCampo = "g.agrupador_1"
            numTabla = "40"
        ElseIf idReporte = 9 Or idReporte = 22 Or idReporte = 35 Then
            cadTitulo = "Agrupador (2) de falla"
            cadTabla = "" & rutaBD & ".cat_fallas"
            cadJoin = "c.falla_ajustada"
            cadCampo = "g.agrupador_2"
            numTabla = "45"
        ElseIf idReporte = 10 Or idReporte = 23 Or idReporte = 36 Then
            cadTitulo = "Dia"
            cadGrafico = "Dia"
            grupoFecha = "DATE_FORMAT(a.fecha, '%d/%m/%Y') AS nombre"
            grupoFechaG = "DATE_FORMAT(a.fecha, '%d/%m/%Y') AS nombre"
        ElseIf idReporte = 11 Or idReporte = 24 Or idReporte = 37 Then
            cadTitulo = "Semana', 'Dia inicial de la semana"
            cadGrafico = "Semana"
            grupoFechaG = "DATE_FORMAT(a.fecha,'%x/%v') AS nombre"
            grupoFecha = "DATE_FORMAT(a.fecha,'%x/%v') AS nombre, STR_TO_DATE(CONCAT(DATE_FORMAT(a.fecha,'%x/%v'), ' Monday'), '%x/%v %W')"
        ElseIf idReporte = 12 Or idReporte = 25 Or idReporte = 38 Then
            cadTitulo = "Mes"
            cadGrafico = "Mes"
            grupoFecha = "DATE_FORMAT(a.fecha,'%Y/%m') AS nombre"
            grupoFechaG = "DATE_FORMAT(a.fecha,'%Y/%m') AS nombre"
        ElseIf idReporte = 13 Or idReporte = 26 Or idReporte = 39 Then
            cadTitulo = "Nombre del tecnico"
            cadTabla = "" & rutaBD & ".cat_usuarios"
            cadJoin = "c.tecnico"
        End If

        Dim Leer As Boolean = False

        If idReporte = 42 Then
            'Reporte de Reportes abiertas
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07, '' AS c08, '' AS c09, '' AS c10, '' AS c11, '' AS c12, '' AS c13, '' AS c14, '' AS c15, '' AS c16, '' AS c17, '' AS c18, '' AS c19, '' AS c20, '' AS c21, '' AS c22, '' AS c23, '' AS c24, '' AS c25, '' AS c26, '' AS c27, '' AS c28, '' AS c29, '' AS c30, '' AS c31, '' AS c32, '' AS c33, '' AS c34 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '') "
            cabecera = "UNION (SELECT 'Numero', 'Inicio de la falla', 'Fecha para el reporte', 'Turno', 'Estatus', 'Fecha de llegada del tecnico', 'Tiempo esperado (segundos)', 'Fecha de resolucion de la falla', 'Tiempos de reparacion (segundos)', 'Tiempo total de la parada (segundos)', 'Inicio del reporte de mantenimiento', 'Fin del reporte de mantenimiento', 'Tiempo del reporte de mantenimiento (segundos)', 'Linea', 'ID de la linea', 'Maquina', 'ID de la maquina', 'Area que atendio', 'ID del area', 'Falla confirmada', 'ID de la falla confirmada', 'Usuario solicitante', 'Departamento del usuario', 'Tecnico que atendio el reporte', 'Tecnico que cerro el reporte', 'Usuario que confirmo la reparacion', 'Tipo de mantenimiento', 'Detalle de la falla (reporte tecnico)', 'Contabilizar?', 'Alarmado por reporte no cerrado a tiempo', 'Alarmado por tiempo de espera por tecnico excedido', 'Alarmado por tiempo de reparacion excedido', 'Falla reportada por el usuario', 'ID de la falla reportada') "
            registros = "UNION SELECT c.id, c.fecha, c.fecha_reporte, IFNULL(l.nombre, 'N/A'), c.estatus, c.inicio_atencion, c.tiempollegada, c.cierre_atencion, c.tiemporeparacion, c.tiemporeparacion + c.tiempollegada, c.inicio_reporte, c.cierre_reporte, c.tiemporeporte, IFNULL(a.nombre, 'N/A'), c.linea, IFNULL(b.nombre, 'N/A'), c.maquina, IFNULL(d.nombre, 'N/A'), c.area, IFNULL(e.nombre, 'N/A'), c.falla_ajustada, IFNULL(f.nombre, 'N/A'), IFNULL(j.nombre, 'N/A'), IFNULL(h.nombre, 'N/A'), IFNULL(g.nombre, 'N/A'), IFNULL(m.nombre, 'N/A'), IFNULL(k.nombre, 'N/A'), c.detalle, IF(c.contabilizar = 'S', 'Si', 'No'), IF(c.alarmado = 'S', 'Si', 'No'), IF(c.alarmado_atender = 'S', 'Si', 'No'), IF(c.alarmado_atendido = 'S', 'Si', 'No'), IFNULL(i.nombre, 'N/A'), c.falla FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_lineas a ON c.linea = a.id LEFT JOIN " & rutaBD & ".cat_maquinas b ON c.maquina = b.id LEFT JOIN " & rutaBD & ".cat_areas d ON c.area = d.id LEFT JOIN " & rutaBD & ".cat_fallas e ON c.falla_ajustada = e.id LEFT JOIN " & rutaBD & ".cat_usuarios f ON c.solicitante = f.id LEFT JOIN " & rutaBD & ".cat_usuarios g ON g.tecnico = g.id LEFT JOIN " & rutaBD & ".cat_usuarios h ON c.tecnicoatend = h.id LEFT JOIN " & rutaBD & ".cat_fallas i ON c.falla = i.id LEFT JOIN " & rutaBD & ".cat_generales j ON f.departamento = j.id LEFT JOIN " & rutaBD & ".cat_generales k ON c.tipo = k.id LEFT JOIN " & rutaBD & ".cat_turnos l ON c.turno = l.id LEFT JOIN " & rutaBD & ".cat_usuarios m ON c.confirmado = m.id WHERE c.estatus >= 0 AND c.fecha_reporte >= '" & Strings.Left(fDesde, 10) & "' AND c.fecha_reporte <= '" & Strings.Left(fHasta, 10) & "'"
        ElseIf idReporte = 27 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07, '' AS c08 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', 'Referencia', 'ID (numero unico)', 'Reportes cerrados', 'Total tiempo de reparación (seg)', 'Reportes acumulados', 'Total Reportes', 'Porcentaje') "
            registros = "UNION SELECT IFNULL(c.nombre, 'N/A') AS nombre, c.referencia, campo, ttl, tiempottl, @total := @total + c.ttl AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT nombre, referencia, linea AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_lineas d ON linea = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 4 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t  "

        ElseIf idReporte = 28 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07, '' AS c08 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', 'Referencia', 'ID (numero unico)', 'Reportes cerrados', 'Total tiempo de reparación (seg)', 'Reportes acumulados', 'Total Reportes', 'Porcentaje') "
            registros = "UNION SELECT IFNULL(c.nombre, 'N/A') AS nombre, c.referencia, campo, ttl, tiempottl, @total := @total + c.ttl AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT nombre, referencia, maquina AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_maquinas d ON maquina = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 4 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t   "
        ElseIf idReporte = 29 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07, '' AS c08 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', 'Referencia', 'ID (numero unico)', 'Reportes cerrados', 'Total tiempo de reparación (seg)', 'Reportes acumulados', 'Total Reportes', 'Porcentaje') "
            registros = "UNION SELECT IFNULL(c.nombre, 'N/A') AS nombre, c.referencia, campo, ttl, tiempottl, @total := @total + c.ttl AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT nombre, referencia, area AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_areas d ON area = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 4 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t    "
        ElseIf idReporte = 30 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07, '' AS c08 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', 'Referencia', 'ID (numero unico)', 'Reportes cerrados', 'Total tiempo de reparación (seg)', 'Reportes acumulados', 'Total Reportes', 'Porcentaje') "
            registros = "UNION SELECT IFNULL(c.nombre, 'N/A') AS nombre, c.referencia, campo, ttl, tiempottl, @total := @total + c.ttl AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT nombre, referencia, falla_ajustada AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_fallas d ON falla_ajustada = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 4 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t    "
        ElseIf idReporte = 31 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', 'ID (numero unico)', 'Reportes cerrados', 'Total tiempo de reparación (seg)', 'Reportes acumulados', 'Total Reportes', 'Porcentaje') "
            registros = "UNION SELECT IFNULL(c.nombre, 'N/A') AS nombre, campo, ttl, tiempottl, @total := @total + c.ttl AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT e.nombre, d.tipo AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_maquinas d ON c.maquina = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.tipo = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 3 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "
        ElseIf idReporte = 32 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', 'ID (numero unico)', 'Reportes cerrados', 'Total tiempo de reparación (seg)', 'Reportes acumulados', 'Total Reportes', 'Porcentaje') "
            registros = "UNION SELECT IFNULL(c.nombre, 'N/A') AS nombre, campo, ttl, tiempottl, @total := @total + c.ttl AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT e.nombre, d.agrupador_1 AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_maquinas d ON c.maquina = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.agrupador_1 = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 3 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "
        ElseIf idReporte = 33 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', 'ID (numero unico)', 'Reportes cerrados', 'Total tiempo de reparación (seg)', 'Reportes acumulados', 'Total Reportes', 'Porcentaje') "
            registros = "UNION SELECT IFNULL(c.nombre, 'N/A') AS nombre, campo, ttl, tiempottl, @total := @total + c.ttl AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT e.nombre, d.agrupador_2 AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_maquinas d ON c.maquina = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.agrupador_2 = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 3 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "
        ElseIf idReporte = 34 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', 'ID (numero unico)', 'Reportes cerrados', 'Total tiempo de reparación (seg)', 'Reportes acumulados', 'Total Reportes', 'Porcentaje') "
            registros = "UNION SELECT IFNULL(c.nombre, 'N/A') AS nombre, campo, ttl, tiempottl, @total := @total + c.ttl AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT e.nombre, d.agrupador_1 AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_fallas d ON c.falla_ajustada = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.agrupador_1 = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 3 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "
        ElseIf idReporte = 35 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', 'ID (numero unico)', 'Reportes cerrados', 'Total tiempo de reparación (seg)', 'Reportes acumulados', 'Total Reportes', 'Porcentaje') "
            registros = "UNION SELECT IFNULL(c.nombre, 'N/A') AS nombre, campo, ttl, tiempottl, @total := @total + c.ttl AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT e.nombre, d.agrupador_2 AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_fallas d ON c.falla_ajustada = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.agrupador_2 = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 3 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

        ElseIf idReporte = 36 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', 'Reportes cerrados', 'Total tiempo de reparación (seg)', 'Reportes acumulados', 'Total Reportes', 'Porcentaje') "
            registros = "UNION SELECT campo AS nombre, ttl, tiempottl, @total := @total + c.ttl AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT fecha_reporte AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 2 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "
        ElseIf idReporte = 37 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', 'Reportes cerrados', 'Total tiempo de reparación (seg)', 'Reportes acumulados', 'Total Reportes', 'Porcentaje') "
            registros = "UNION SELECT campo AS nombre, campo2, ttl, tiempottl, @total := @total + c.ttl AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT DATE_FORMAT(fecha_reporte,'%x/%v') AS campo, STR_TO_DATE(CONCAT(DATE_FORMAT(fecha_reporte,'%x/%v'), ' Monday'), '%x/%v %W') AS campo2, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY DATE_FORMAT(fecha_reporte,'%x/%v') ORDER BY 2 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

        ElseIf idReporte = 38 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', 'Reportes cerrados', 'Total tiempo de reparación (seg)', 'Reportes acumulados', 'Total Reportes', 'Porcentaje') "
            registros = "UNION SELECT campo AS nombre, ttl, tiempottl, @total := @total + c.ttl AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT DATE_FORMAT(fecha_reporte,'%Y/%m') AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY DATE_FORMAT(fecha_reporte,'%Y/%m') ORDER BY 2 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

        ElseIf idReporte = 39 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07, '' AS c08 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', 'Referencia', 'ID (numero unico)', 'Reportes cerrados', 'Total tiempo de reparación (seg)', 'Reportes acumulados', 'Total Reportes', 'Porcentaje') "
            registros = "UNION SELECT IFNULL(c.nombre, 'N/A') AS nombre, c.referencia, campo, ttl, tiempottl, @total := @total + c.ttl AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT nombre, referencia, c.tecnico AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_usuarios d ON c.tecnico = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 4 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t  "

        ElseIf (idReporte >= 1 And idReporte <= 4 Or idReporte = 13) Or (idReporte >= 14 And idReporte <= 17 Or idReporte = 26) Then
            If Not id01 And (idReporte = 14 Or idReporte = 1) Then
                id01 = True
                Leer = True
            ElseIf Not id02 And (idReporte = 15 Or idReporte = 2) Then
                id02 = True
                Leer = True
            ElseIf Not id03 And (idReporte = 16 Or idReporte = 3) Then
                id03 = True
                Leer = True
            ElseIf Not id04 And (idReporte = 17 Or idReporte = 4) Then
                id04 = True
                Leer = True
            ElseIf Not id13 And (idReporte = 26 Or idReporte = 13) Then
                id13 = True
                Leer = True
            End If
            If Leer Then
                inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07, '' AS c08 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '', '') "
                cabecera = "UNION (SELECT '" & cadTitulo & "', 'Referencia', 'ID (numero unico)', 'Tiempo total de la falla (seg)', 'Reportes cerrados', 'Total tiempo disponible (seg)', 'Tiempo promedio de reparación (MTTR)', 'Tiempo promedio entre fallas (MTBF)') "
                registros = "UNION SELECT a.nombre, a.referencia, a.id, IFNULL(SUM(c.tiemporeparacion + c.tiempollegada), 0) AS tiempo_c, COUNT(c.id) AS docs, (e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, IFNULL(SUM(c.tiemporeparacion + c.tiempollegada) / 3600 / COUNT(c.id), 0) AS mttrc, ((SELECT tdisponible) - IFNULL(SUM(f.tiempo), 0)) / IF(COUNT(c.id) = 0, 1, COUNT(c.id)) / 3600 AS mtbfc FROM " & cadTabla & " a LEFT JOIN (SELECT id, tiemporeparacion, tiempollegada, maquina, tecnico, linea, area, falla_ajustada FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ") AS c ON a.id = " & cadJoin & " LEFT JOIN (SELECT maquina, tiempo FROM " & rutaBD & ".detalleparos f WHERE f.contabilizar = 'S' " & filtroParos & " AND f.tipo = 1) AS f ON c.maquina = f.maquina, (SELECT SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE " & filtroFechas & ") AS e " & IIf(idReporte = 13, " WHERE (a.rol = 'T' OR a.rol = 'A')", "") & "GROUP BY a.nombre "
            End If
        ElseIf (idReporte >= 5 And idReporte <= 9) Or (idReporte >= 18 And idReporte <= 22) Then
            If Not id05 And (idReporte = 18 Or idReporte = 5) Then
                id05 = True
                Leer = True
            ElseIf Not id06 And (idReporte = 6 Or idReporte = 19) Then
                id06 = True
                Leer = True
            ElseIf Not id07 And (idReporte = 7 Or idReporte = 20) Then
                id07 = True
                Leer = True
            ElseIf Not id08 And (idReporte = 8 Or idReporte = 21) Then
                id08 = True
                Leer = True
            ElseIf Not id09 And (idReporte = 9 Or idReporte = 22) Then
                id09 = True
                Leer = True
            End If
            If Leer Then
                inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '') "
                cabecera = "UNION (SELECT '" & cadTitulo & "', 'ID (numero unico)', 'Tiempo total de la falla (seg)', 'Reportes cerrados', 'Total tiempo disponible (seg)', 'Tiempo promedio de reparación (MTTR)', 'Tiempo promedio entre fallas (MTBF)') "
                registros = "UNION SELECT a.nombre, a.id, IFNULL(SUM(c.tiemporeparacion + c.tiempollegada), 0) AS tiempo_c, COUNT(c.id) AS docs, (e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, IFNULL(SUM(c.tiemporeparacion + c.tiempollegada) / 3600 / COUNT(c.id), 0) AS mttrc, ((SELECT tdisponible) - IFNULL(SUM(f.tiempo), 0)) / IF(COUNT(c.id) = 0, 1, COUNT(c.id)) / 3600 AS mtbfc FROM " & rutaBD & ".cat_generales a LEFT JOIN " & cadTabla & " g ON a.id = " & cadCampo & " LEFT JOIN (SELECT id, tiemporeparacion, tiempollegada, maquina, linea, area, falla_ajustada FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ") AS c ON g.id = " & cadJoin & " LEFT JOIN (SELECT maquina, tiempo FROM " & rutaBD & ".detalleparos f WHERE f.contabilizar = 'S' " & filtroParos & " AND f.tipo = 1) AS f ON c.maquina = f.maquina, (SELECT SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE " & filtroFechas & ") AS e WHERE a.tabla = " & numTabla & " GROUP BY a.nombre "
            End If
        ElseIf (idReporte >= 10 And idReporte <= 12) Or (idReporte >= 23 And idReporte <= 25) Then
            If Not id10 And (idReporte = 10 Or idReporte = 23) Then
                id10 = True
                Leer = True
            ElseIf Not id11 And (idReporte = 11 Or idReporte = 24) Then
                id11 = True
                Leer = True
            ElseIf Not id12 And (idReporte = 12 Or idReporte = 25) Then
                id12 = True
                Leer = True
            End If
            If Leer And (idReporte = 10 Or idReporte = 23) Then
                inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '') "
                cabecera = "UNION (SELECT 'Dia', 'Tiempo total de la falla (seg)', 'Reportes cerrados', 'Total tiempo disponible (seg)', 'Tiempo promedio de reparación (MTTR)', 'Tiempo promedio entre fallas (MTBF)') "

                registros = "UNION SELECT a.fecha AS nombre, ROUND(IFNULL(SUM(c.tiemporeparacion + c.tiempollegada), 0)/ 3600, 1) AS tiempo_c, COUNT(c.id) AS docs, (IF(dia = 2, 1, 0) * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + IF(dia = 3, 1, 0) * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + IF(dia = 4, 1, 0) * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + IF(dia = 5, 1, 0) * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + IF(dia = 6, 1, 0) * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + IF(dia = 7, 1, 0) * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + IF(dia = 1, 1, 0) * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, IFNULL(SUM(c.tiemporeparacion + c.tiempollegada) / 3600 / COUNT(c.id), 0) AS mttrc, ((SELECT tdisponible) - IFNULL(SUM(f.tiempo), 0)) / IF(COUNT(c.id) = 0, 1, COUNT(c.id)) / 3600 AS mtbfc FROM " & rutaBD & ".dias a LEFT JOIN (SELECT id, maquina, linea, fecha_reporte, tiemporeparacion, tiempollegada FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ") AS c ON a.fecha = c.fecha_reporte LEFT JOIN (SELECT maquina, tiempo FROM " & rutaBD & ".detalleparos f WHERE f.contabilizar = 'S' " & filtroParos & " AND f.tipo = 1) AS f ON c.maquina = f.maquina WHERE " & filtroFechasDia & " GROUP BY nombre  "
            ElseIf Leer And (idReporte = 11 Or idReporte = 24) Then
                inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '') "
                cabecera = "UNION (SELECT 'Semana', 'Lunes de la semana', 'Tiempo total de la falla (hr)', 'Reportes cerrados', 'Total tiempo disponible (seg)', 'Tiempo promedio de reparación (MTTR)', 'Tiempo promedio entre fallas (MTBF)') "
                registros = "UNION SELECT DATE_FORMAT(a.fecha,'%x/%v') AS nombre, STR_TO_DATE(CONCAT(DATE_FORMAT(a.fecha,'%x/%v'), ' Monday'), '%x/%v %W'), ROUND(IFNULL(SUM(c.tiemporeparacion + c.tiempollegada), 0)/ 3600, 1) AS tiempo_c, COUNT(c.id) AS docs, ((SELECT COUNT(*) FROM dias WHERE dia = 2 AND DATE_FORMAT(a.fecha,'%x/%v') = DATE_FORMAT(fecha,'%x/%v') AND " & filtroFechas & ") * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 3 AND DATE_FORMAT(a.fecha,'%x/%v') = DATE_FORMAT(fecha,'%x/%v') AND " & filtroFechas & ") * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 4 AND DATE_FORMAT(a.fecha,'%x/%v') = DATE_FORMAT(fecha,'%x/%v') AND " & filtroFechas & ") * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 5 AND DATE_FORMAT(a.fecha,'%x/%v') = DATE_FORMAT(fecha,'%x/%v') AND " & filtroFechas & ") * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 6 AND DATE_FORMAT(a.fecha,'%x/%v') = DATE_FORMAT(fecha,'%x/%v') AND " & filtroFechas & ") * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 7 AND DATE_FORMAT(a.fecha,'%x/%v') = DATE_FORMAT(fecha,'%x/%v') AND " & filtroFechas & ") * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 1 AND DATE_FORMAT(a.fecha,'%x/%v') = DATE_FORMAT(fecha,'%x/%v') AND " & filtroFechas & ") * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, IFNULL(SUM(c.tiemporeparacion + c.tiempollegada) / 3600 / COUNT(c.id), 0) AS mttrc, ((SELECT tdisponible) - IFNULL(SUM(f.tiempo), 0)) / IF(COUNT(c.id) = 0, 1, COUNT(c.id)) / 3600 AS mtbfc FROM " & rutaBD & ".dias a LEFT JOIN " & rutaBD & ".reportes c ON a.fecha = c.fecha_reporte AND c.contabilizar = 'S' AND c.estatus >= 100 LEFT JOIN " & rutaBD & ".detalleparos f ON c.maquina = f.maquina AND f.contabilizar = 'S' " & filtroParos & " AND f.tipo = 1 WHERE " & filtroFechasDia & " GROUP BY DATE_FORMAT(a.fecha,'%x/%v') "
            ElseIf Leer And (idReporte = 12 Or idReporte = 25) Then
                inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06 UNION (SELECT CONCAT('Reporte generado en fecha: ', NOW()), '', '', '', '', '') UNION (SELECT CONCAT('Extayendo datos desde: ', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', ' hasta: ', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '') "
                cabecera = "UNION (SELECT 'Mes', 'Tiempo total de la falla (hr)', 'Reportes cerrados', 'Total tiempo disponible (seg)', 'Tiempo promedio de reparación (MTTR)', 'Tiempo promedio entre fallas (MTBF)') "
                registros = "UNION SELECT DATE_FORMAT(a.fecha,'%Y%m') AS nombre, ROUND(IFNULL(SUM(c.tiemporeparacion + c.tiempollegada), 0)/ 3600, 1) AS tiempo_c, COUNT(c.id) AS docs, ((SELECT COUNT(*) FROM dias WHERE dia = 2 AND DATE_FORMAT(a.fecha,'%Y%m') = DATE_FORMAT(fecha,'%Y%m') AND " & filtroFechas & ") * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 3 AND DATE_FORMAT(a.fecha,'%Y%m') = DATE_FORMAT(fecha,'%Y%m') AND " & filtroFechas & ") * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 4 AND DATE_FORMAT(a.fecha,'%Y%m') = DATE_FORMAT(fecha,'%Y%m') AND " & filtroFechas & ") * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 5 AND DATE_FORMAT(a.fecha,'%Y%m') = DATE_FORMAT(fecha,'%Y%m') AND " & filtroFechas & ") * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 6 AND DATE_FORMAT(a.fecha,'%Y%m') = DATE_FORMAT(fecha,'%Y%m') AND " & filtroFechas & ") * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 7 AND DATE_FORMAT(a.fecha,'%Y%m') = DATE_FORMAT(fecha,'%Y%m') AND " & filtroFechas & ") * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 1 AND DATE_FORMAT(a.fecha,'%Y%m') = DATE_FORMAT(fecha,'%Y%m') AND " & filtroFechas & ") * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, IFNULL(SUM(c.tiemporeparacion + c.tiempollegada) / 3600 / COUNT(c.id), 0) AS mttrc, ((SELECT tdisponible) - IFNULL(SUM(f.tiempo), 0)) / IF(COUNT(c.id) = 0, 1, COUNT(c.id)) / 3600 AS mtbfc FROM " & rutaBD & ".dias a LEFT JOIN " & rutaBD & ".reportes c ON a.fecha = c.fecha_reporte AND c.contabilizar = 'S' AND c.estatus >= 100 LEFT JOIN " & rutaBD & ".detalleparos f ON c.maquina = f.maquina AND f.contabilizar = 'S' " & filtroParos & " AND f.tipo = 1 WHERE " & filtroFechasDia & " GROUP BY DATE_FORMAT(a.fecha,'%Y%m') "
            End If
        End If

        Dim regsAfectados = consultaACT(inicial & cabecera & registros & ") AS query01 INTO OUTFILE '" & archivoSaliente & "' FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '" & comillas & "' ENCLOSED BY '" & comillas & "' LINES TERMINATED BY '\n'")
        If errorBD.Length > 0 Then
            errorCorreos = errorBD
            agregarLOG("Error al construir el reporte. " + errorBD, 7, 0)
            generarReporte = -1
        End If
        If graficar = "S" Then
            'Se produce el gráfico
            Dim cadSQL = ""
            If idReporte >= 1 And idReporte <= 13 Then
                cadSQL = "SELECT * FROM " & rutaBD & ".pu_graficos WHERE (usuario = 1 OR usuario = 0) AND grafico = " & 100 + idReporte & " ORDER BY usuario DESC LIMIT 1"
            ElseIf idReporte >= 14 And idReporte <= 26 Then
                cadSQL = "SELECT * FROM " & rutaBD & ".pu_graficos WHERE (usuario = 1 OR usuario = 0) AND grafico = " & 200 + idReporte - 13 & " ORDER BY usuario DESC LIMIT 1"
            ElseIf idReporte >= 27 And idReporte <= 39 Then
                cadSQL = "SELECT * FROM " & rutaBD & ".pu_graficos WHERE (usuario = 1 OR usuario = 0) AND grafico = " & 300 + idReporte - 26 & " ORDER BY usuario DESC LIMIT 1"
            End If

            Dim config As DataSet = consultaSEL(cadSQL)
            regsAfectados = 0
            If errorBD.Length > 0 Then
                agregarLOG("Ocurrió un error al intentar leer MySQL. Error: " + Microsoft.VisualBasic.Strings.Left(errorBD, 250), 9, 0)
                errorCorreos = errorBD
                generarReporte = -1
            Else
                If config.Tables(0).Rows.Count > 0 Then
                    Dim cadOrden As String = ""
                    If idReporte >= 1 And idReporte <= 4 Or idReporte = 13 Then
                        cadOrden = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " ORDER BY 3 DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " ORDER BY 3", "ORDER BY a.nombre"))
                        registros = "SELECT a.nombre, COUNT(c.id) AS docs, IFNULL(SUM(c.tiemporeparacion + c.tiempollegada) / 3600 / COUNT(c.id), 0) AS mttrc FROM " & cadTabla & " a LEFT JOIN " & rutaBD & ".reportes c ON a.id = " & cadJoin & " AND c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & IIf(idReporte = 13, " WHERE (a.rol = 'A' OR a.rol = 'T') ", "") & " GROUP BY a.nombre " & IIf(config.Tables(0).Rows(0)!incluir_ceros = "N", "HAVING docs > 0 ", "") & cadOrden

                    ElseIf idReporte = 27 Then


                        registros = "SELECT IFNULL(c.nombre, 'N/A') AS nombre, ttl AS mttrc, tiempottl, @total := @total + c.ttl AS acumulado, t.total, @total / t.total * 100 AS pct FROM (SELECT nombre, referencia, linea AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_lineas d ON linea = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 4 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t  "
                    ElseIf idReporte = 28 Then


                        registros = "SELECT IFNULL(c.nombre, 'N/A') AS nombre, ttl AS mttrc, tiempottl, @total := @total + c.ttl AS acumulado, t.total, @total / t.total * 100 AS pct FROM (SELECT nombre, referencia, maquina AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_maquinas d ON maquina = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 4 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t  "

                    ElseIf idReporte = 29 Then


                        registros = "SELECT IFNULL(c.nombre, 'N/A') AS nombre, ttl AS mttrc, tiempottl, @total := @total + c.ttl AS acumulado, t.total, @total / t.total * 100 AS pct FROM (SELECT nombre, referencia, area AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_areas d ON area = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 4 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t  "

                    ElseIf idReporte = 30 Then


                        registros = "SELECT IFNULL(c.nombre, 'N/A') AS nombre, ttl AS mttrc, tiempottl, @total := @total + c.ttl AS acumulado, t.total, @total / t.total * 100 AS pct FROM (SELECT nombre, referencia, falla_ajustada AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_fallas d ON falla_ajustada = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 4 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t  "

                    ElseIf idReporte = 31 Then


                        registros = "SELECT IFNULL(c.nombre, 'N/A') AS nombre, campo, ttl AS mttrc, tiempottl, @total := @total + c.ttl AS acumulado, @total / t.total * 100 AS pct FROM (SELECT e.nombre, d.tipo AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_maquinas d ON c.maquina = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.tipo = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 3 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

                    ElseIf idReporte = 32 Then


                        registros = "SELECT IFNULL(c.nombre, 'N/A') AS nombre, campo, ttl AS mttrc, tiempottl, @total := @total + c.ttl AS acumulado, @total / t.total * 100 AS pct FROM (SELECT e.nombre, d.agrupador_1 AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_maquinas d ON c.maquina = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.agrupador_1 = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 3 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

                    ElseIf idReporte = 33 Then


                        registros = "SELECT IFNULL(c.nombre, 'N/A') AS nombre, campo, ttl AS mttrc, tiempottl, @total := @total + c.ttl AS acumulado, @total / t.total * 100 AS pct FROM (SELECT e.nombre, d.agrupador_2 AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_maquinas d ON c.maquina = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.agrupador_2 = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 3 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

                    ElseIf idReporte = 34 Then


                        registros = "SELECT IFNULL(c.nombre, 'N/A') AS nombre, campo, ttl AS mttrc, tiempottl, @total := @total + c.ttl AS acumulado, @total / t.total * 100 AS pct FROM (SELECT e.nombre, d.agrupador_1 AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_fallas d ON c.falla_ajustada = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.agrupador_1 = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 3 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

                    ElseIf idReporte = 35 Then


                        registros = "SELECT IFNULL(c.nombre, 'N/A') AS nombre, campo, ttl AS mttrc, tiempottl, @total := @total + c.ttl AS acumulado, @total / t.total * 100 AS pct FROM (SELECT e.nombre, d.agrupador_2 AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_fallas d ON c.falla_ajustada = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.agrupador_2 = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 3 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

                    ElseIf idReporte = 36 Then


                        registros = "SELECT campo AS nombre, ttl AS mttrc, tiempottl, @total := @total + c.ttl AS acumulado, @total / t.total * 100 AS pct FROM (SELECT DATE_FORMAT(fecha_reporte, '%d/%m/%Y') AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY DATE_FORMAT(fecha_reporte, '%d/%m/%Y') ORDER BY 2 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

                    ElseIf idReporte = 37 Then


                        registros = "SELECT campo AS nombre, ttl AS mttrc, tiempottl, @total := @total + c.ttl AS acumulado, @total / t.total * 100 AS pct FROM (SELECT DATE_FORMAT(fecha_reporte,'%x/%v') AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY DATE_FORMAT(fecha_reporte,'%x/%v') ORDER BY 2 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

                    ElseIf idReporte = 38 Then


                        registros = "SELECT campo AS nombre, ttl AS mttrc, tiempottl, @total := @total + c.ttl AS acumulado, @total / t.total * 100 AS pct FROM (SELECT DATE_FORMAT(fecha_reporte,'%Y/%m') AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY DATE_FORMAT(fecha_reporte,'%Y/%m') ORDER BY 2 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

                    ElseIf idReporte = 39 Then


                        registros = "SELECT IFNULL(c.nombre, 'N/A') AS nombre, ttl AS mttrc, tiempottl, @total := @total + c.ttl AS acumulado, t.total, @total / t.total * 100 AS pct FROM (SELECT nombre, referencia, c.tecnico AS campo, COUNT(*) AS ttl, SUM(tiemporeparacion + tiempollegada) AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_usuarios d ON c.tecnico = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY 4 DESC) c, (SELECT @total := 0) AS total, (SELECT COUNT(*) AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t  "

                    ElseIf idReporte >= 5 And idReporte <= 9 Then
                        cadOrden = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " ORDER BY 3 DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " ORDER BY 3", "ORDER BY a.nombre"))
                        registros = "SELECT a.nombre, COUNT(c.id) AS docs, IFNULL(SUM(c.tiemporeparacion + c.tiempollegada) / 3600 / COUNT(c.id), 0) AS mttrc FROM " & rutaBD & ".cat_generales a LEFT JOIN " & cadTabla & " g ON a.id = " & cadCampo & " LEFT JOIN " & rutaBD & ".reportes c ON g.id = " & cadJoin & " AND c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " WHERE a.tabla = " & numTabla & " GROUP BY a.nombre " & IIf(config.Tables(0).Rows(0)!incluir_ceros = "N", "HAVING docs > 0 ", "") & cadOrden
                    ElseIf idReporte >= 10 And idReporte <= 12 Then
                        cadOrden = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " ORDER BY 3 DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " ORDER BY 3", "ORDER BY a.nombre"))
                        registros = "SELECT " & grupoFecha & ", COUNT(c.id) AS docs, IFNULL(SUM(c.tiemporeparacion + c.tiempollegada) / 3600 / COUNT(c.id), 0) AS mttrc FROM " & rutaBD & ".dias a LEFT JOIN " & rutaBD & ".reportes c ON a.fecha = c.fecha_reporte AND c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY nombre " & IIf(config.Tables(0).Rows(0)!incluir_ceros = "N", "HAVING docs > 0 ", "") & cadOrden
                    ElseIf idReporte >= 14 And idReporte <= 17 Or idReporte = 26 Then
                        cadOrden = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " ORDER BY 4 DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " ORDER BY 4", "ORDER BY a.nombre"))
                        registros = "SELECT a.nombre, COUNT(c.id) AS docs, (e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, ((SELECT tdisponible) - IFNULL(SUM(f.tiempo), 0)) / IF(COUNT(c.id) = 0, 1, COUNT(c.id)) / 3600 AS mttrc FROM " & cadTabla & " a LEFT JOIN (SELECT id, linea, maquina, area, falla_ajustada, tecnico FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ") AS c ON a.id = " & cadJoin & " LEFT JOIN (SELECT maquina, tiempo FROM " & rutaBD & ".detalleparos f WHERE f.contabilizar = 'S' " & filtroParos & " AND f.tipo = 1) AS f ON c.maquina = f.maquina, (SELECT SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE " & filtroFechas & ") AS e " & IIf(idReporte = 26, " WHERE (a.rol = 'T' OR a.rol = 'A')", "") & " GROUP BY a.nombre " & IIf(config.Tables(0).Rows(0)!incluir_ceros = "N", "HAVING docs > 0 ", "") & cadOrden
                    ElseIf idReporte >= 18 And idReporte <= 22 Then
                        cadOrden = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " ORDER BY 4 DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " ORDER BY 4", "ORDER BY a.nombre"))
                        registros = "SELECT a.nombre, COUNT(c.id) AS docs, (e.lunes * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.martes * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.miercoles * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.jueves * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.viernes * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.sabado * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + e.domingo * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, ((SELECT tdisponible) - IFNULL(SUM(f.tiempo), 0)) / IF(COUNT(c.id) = 0, 1, COUNT(c.id)) / 3600 AS mttrc FROM " & rutaBD & ".cat_generales a LEFT JOIN " & cadTabla & " g ON a.id = " & cadCampo & " LEFT JOIN (SELECT id, linea, maquina, falla_ajustada FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ") AS c ON g.id = " & cadJoin & " LEFT JOIN (SELECT maquina, tiempo FROM " & rutaBD & ".detalleparos f WHERE f.contabilizar = 'S' " & filtroParos & " AND f.tipo = 1) AS f ON c.maquina = f.maquina, (SELECT SUM(IF(dia = 2, 1, 0)) AS lunes, SUM(IF(dia = 3, 1, 0)) AS martes, SUM(IF(dia = 4, 1, 0)) AS miercoles, SUM(IF(dia = 5, 1, 0)) AS jueves, SUM(IF(dia = 6, 1, 0)) AS viernes, SUM(IF(dia = 7, 1, 0)) AS sabado, SUM(IF(dia = 1, 1, 0)) AS domingo FROM " & rutaBD & ".dias WHERE " & filtroFechas & ") AS e WHERE a.tabla = " & numTabla & " GROUP BY a.nombre " & IIf(config.Tables(0).Rows(0)!incluir_ceros = "N", "HAVING docs > 0 ", "") & cadOrden
                    End If

                    Dim indicador01 = "MTTR"
                    Dim indicador02 = "PCT"

                    Dim graficos As DataSet = consultaSEL(registros)
                    If graficos.Tables(0).Rows.Count > 0 Then
                        If idReporte >= 1 And idReporte <= 26 Then
                            If idReporte >= 1 And idReporte <= 13 Then
                                indicador01 = "MTTR"
                                indicador02 = "MTTR"
                            ElseIf idReporte >= 14 And idReporte <= 26 Then
                                indicador01 = "MTBF"
                                indicador02 = "MTBF"
                            End If

                            ChartControl1.Series.Clear()
                            ChartControl1.Titles.Clear()
                            Dim Titulo As New ChartTitle()
                            Titulo.Text = config.Tables(0).Rows(0)!titulo & Strings.Space(10)
                            Dim miFuente = New Drawing.Font("Lucida Sans", 10, FontStyle.Regular)
                            Dim miFuenteAlto = New Drawing.Font("Lucida Sans", 16, FontStyle.Bold)
                            Dim miFuenteEjes = New Drawing.Font("Lucida Sans", 11, FontStyle.Regular)

                            Titulo.Font = miFuenteAlto
                            Dim tabla_grafico As New DataTable("grafico")

                            ' Create an empty table.
                            Dim datos As New DataTable("grafico")
                            Dim row As DataRow = Nothing
                            datos.Columns.Add("orden", GetType(String))
                            datos.Columns.Add("estacion", GetType(String))
                            datos.Columns.Add("total", GetType(Decimal))
                            tabla_grafico.Columns.Add("orden", GetType(String))
                            tabla_grafico.Columns.Add("estacion", GetType(String))
                            tabla_grafico.Columns.Add("total", GetType(Decimal))

                            For Each lineas In graficos.Tables(0).Rows
                                row = datos.NewRow()
                                row("orden") = "A"
                                row("estacion") = lineas!nombre
                                row("total") = lineas!mttrc
                                datos.Rows.Add(row)
                            Next

                            If config.Tables(0).Rows(0)!maximo_barras > 0 Or config.Tables(0).Rows(0)!maximo_barraspct > 0 Then
                                Dim TotalVal = 0
                                Dim tBarras = 0
                                Dim valAcum = 0
                                Dim varios = 0
                                Dim valAgrupado = 0
                                For Each filas In datos.Rows
                                    TotalVal = TotalVal + filas!total
                                    tBarras = tBarras + 1
                                Next
                                If tBarras > config.Tables(0).Rows(0)!maximo_barras Or (config.Tables(0).Rows(0)!maximo_barraspct > 0 And config.Tables(0).Rows(0)!maximo_barraspct < 100) Then
                                    Dim view As DataView = New DataView(datos)
                                    Dim tmpTabla As New DataTable("grafico")
                                    ' Create an empty table.
                                    tmpTabla.Columns.Add("orden", GetType(String))
                                    tmpTabla.Columns.Add("estacion", GetType(String))
                                    tmpTabla.Columns.Add("total", GetType(Decimal))


                                    view.Sort = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " total DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " total", "estacion"))
                                    Dim tabla_ordenada As DataTable = view.ToTable()
                                    tBarras = 0
                                    valAcum = 0
                                    For Each filas In tabla_ordenada.Rows
                                        valAcum = valAcum + filas!total
                                        tBarras = tBarras + 1
                                        If tBarras > config.Tables(0).Rows(0)!maximo_barras Or ((valAcum / TotalVal * 100) > config.Tables(0).Rows(0)!maximo_barraspct And config.Tables(0).Rows(0)!maximo_barraspct > 0) Then
                                            valAgrupado = valAgrupado + filas!total
                                            varios = varios + 1
                                        Else
                                            row = tmpTabla.NewRow()
                                            row("orden") = "B"
                                            row("estacion") = filas!estacion
                                            row("total") = filas!total
                                            tmpTabla.Rows.Add(row)
                                        End If
                                    Next
                                    If valAgrupado > 0 And config.Tables(0).Rows(0)!agrupar = "S" Then
                                        Dim cadAgrupado As String = ValNull(config.Tables(0).Rows(0)!agrupar_texto, "A")
                                        If cadAgrupado.Length = 0 Then cadAgrupado = "AGRUPADO"
                                        cadAgrupado = cadAgrupado & " (" & varios & ")"
                                        row = tmpTabla.NewRow()
                                        If config.Tables(0).Rows(0)!agrupar_posicion = "P" Then
                                            row("orden") = "A"
                                        ElseIf config.Tables(0).Rows(0)!agrupar_posicion = "F" Then
                                            row("orden") = "Z"
                                        End If
                                        row("estacion") = cadAgrupado
                                        row("total") = valAgrupado
                                        tmpTabla.Rows.Add(row)
                                    End If
                                    Dim view_o As DataView = New DataView(tmpTabla)
                                    If config.Tables(0).Rows(0)!agrupar_posicion = "O" Then
                                        view_o.Sort = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " total DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " total", "estacion"))

                                    Else
                                        view_o.Sort = "orden ASC," & IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " total DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " total", "estacion"))
                                    End If
                                    tabla_grafico = view_o.ToTable()
                                Else
                                    tabla_grafico = datos
                                End If
                            Else
                                tabla_grafico = datos
                            End If
                            Dim series1 As New Series(indicador01, ViewType.Bar)

                            ChartControl1.Series.Add(series1)
                            series1.DataSource = tabla_grafico
                            series1.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                            series1.View.Color = Color.SkyBlue
                            series1.ArgumentScaleType = ScaleType.Qualitative
                            series1.ArgumentDataMember = "estacion"
                            series1.ValueScaleType = ScaleType.Numerical
                            series1.ValueDataMembers.AddRange(New String() {"total"})
                            series1.Label.BackColor = Color.DarkBlue
                            series1.Label.TextColor = Color.White
                            series1.Label.Font = miFuente
                            series1.Label.TextPattern = "{V:F1}"

                            CType(ChartControl1.Diagram, XYDiagram).AxisY.Visibility = DevExpress.Utils.DefaultBoolean.True
                            CType(ChartControl1.Diagram, XYDiagram).AxisY.Label.Font = miFuenteEjes
                            CType(ChartControl1.Diagram, XYDiagram).AxisY.GridSpacingAuto = False
                            CType(ChartControl1.Diagram, XYDiagram).AxisY.GridSpacing = 1
                            CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Text = config.Tables(0).Rows(0)!texto_y
                            CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Font = miFuenteAlto
                            CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True
                            CType(ChartControl1.Diagram, XYDiagram).AxisY.GridLines.Visible = False
                            CType(ChartControl1.Diagram, XYDiagram).AxisY.Tickmarks.Visible = False
                            CType(ChartControl1.Diagram, XYDiagram).AxisY.Tickmarks.MinorVisible = False

                            CType(ChartControl1.Diagram, XYDiagram).SecondaryAxesY.Clear()

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
                            Titulo2.Text = "Extraccion de datos: " & cadPeriodo
                            ChartControl1.Titles.Add(Titulo2)
                            Dim Titulo3 As New ChartTitle()
                            Titulo3.Font = miFuente
                            Titulo3.Text = "Generado el: " & Format(Now, "ddd dd-MM-yyyy HH:mm:ss")
                            ChartControl1.Titles.Add(Titulo3)
                            Dim Titulo4 As New ChartTitle()
                            Titulo4.Font = miFuente
                            Titulo4.Text = "Extrayendo los datos desde: " & Format(eDesde, "dd-MM-yyyy HH:mm:ss") & " hasta: " &
                                    Format(eHasta, "dd-MM-yyyy HH:mm:ss")
                            ChartControl1.Titles.Add(Titulo4)
                            ChartControl1.Width = 1000
                            ChartControl1.Height = 700

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
                                agregarLOG("Ocurrió un error al intentar construir un archivo de adjunto de reporte (gráfico). Error: " & ex.Message, 7, 0)
                            End Try
                        ElseIf idReporte >= 27 And idReporte <= 39 Then
                            If idReporte >= 27 And idReporte <= 39 Then
                                indicador01 = "REPORTES"
                                indicador02 = "PCT"
                            End If
                            ChartControl1.Series.Clear()
                            ChartControl1.Titles.Clear()
                            Dim Titulo As New ChartTitle()
                            Titulo.Text = config.Tables(0).Rows(0)!titulo & Strings.Space(10)
                            Dim miFuente = New Drawing.Font("Lucida Sans", 10, FontStyle.Regular)
                            Dim miFuenteAlto = New Drawing.Font("Lucida Sans", 16, FontStyle.Bold)
                            Dim miFuenteEjes = New Drawing.Font("Lucida Sans", 11, FontStyle.Regular)

                            Titulo.Font = miFuenteAlto
                            Dim tabla_grafico As New DataTable("grafico")

                            ' Create an empty table.
                            Dim datos As New DataTable("grafico")
                            Dim row As DataRow = Nothing
                            datos.Columns.Add("orden", GetType(String))
                            datos.Columns.Add("estacion", GetType(String))
                            datos.Columns.Add("total", GetType(Decimal))
                            datos.Columns.Add("pct", GetType(Decimal))
                            tabla_grafico.Columns.Add("orden", GetType(String))
                            tabla_grafico.Columns.Add("estacion", GetType(String))
                            tabla_grafico.Columns.Add("total", GetType(Decimal))
                            tabla_grafico.Columns.Add("pct", GetType(Decimal))

                            For Each lineas In graficos.Tables(0).Rows
                                row = datos.NewRow()
                                row("orden") = "A"
                                row("estacion") = lineas!nombre
                                row("total") = lineas!mttrc
                                row("pct") = lineas!pct
                                datos.Rows.Add(row)
                            Next

                            If config.Tables(0).Rows(0)!maximo_barras > 0 Or config.Tables(0).Rows(0)!maximo_barraspct > 0 Then
                                Dim TotalVal = 0
                                Dim tBarras = 0
                                Dim valAcum = 0
                                Dim varios = 0
                                Dim valAgrupado = 0
                                Dim pctAgrupado = 0
                                For Each filas In datos.Rows
                                    TotalVal = TotalVal + filas!total
                                    tBarras = tBarras + 1
                                Next
                                If tBarras > config.Tables(0).Rows(0)!maximo_barras Or (config.Tables(0).Rows(0)!maximo_barraspct > 0 And config.Tables(0).Rows(0)!maximo_barraspct < 100) Then
                                    Dim view As DataView = New DataView(datos)
                                    Dim tmpTabla As New DataTable("grafico")
                                    ' Create an empty table.
                                    tmpTabla.Columns.Add("orden", GetType(String))
                                    tmpTabla.Columns.Add("estacion", GetType(String))
                                    tmpTabla.Columns.Add("total", GetType(Decimal))
                                    tmpTabla.Columns.Add("pct", GetType(Decimal))


                                    view.Sort = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " total DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " total", "estacion"))
                                    Dim tabla_ordenada As DataTable = view.ToTable()
                                    tBarras = 0
                                    valAcum = 0
                                    For Each filas In tabla_ordenada.Rows
                                        valAcum = valAcum + filas!total
                                        tBarras = tBarras + 1
                                        If tBarras > config.Tables(0).Rows(0)!maximo_barras Or ((valAcum / TotalVal * 100) > config.Tables(0).Rows(0)!maximo_barraspct And config.Tables(0).Rows(0)!maximo_barraspct > 0) Then
                                            valAgrupado = valAgrupado + filas!total
                                            pctAgrupado = pctAgrupado + filas!pct
                                            varios = varios + 1
                                        Else
                                            row = tmpTabla.NewRow()
                                            row("orden") = "B"
                                            row("estacion") = filas!estacion
                                            row("total") = filas!total
                                            row("pct") = filas!pct
                                            tmpTabla.Rows.Add(row)
                                        End If
                                    Next
                                    If valAgrupado > 0 And config.Tables(0).Rows(0)!agrupar = "S" Then
                                        Dim cadAgrupado As String = ValNull(config.Tables(0).Rows(0)!agrupar_texto, "A")
                                        If cadAgrupado.Length = 0 Then cadAgrupado = "AGRUPADO"
                                        cadAgrupado = cadAgrupado & " (" & varios & ")"
                                        row = tmpTabla.NewRow()
                                        If config.Tables(0).Rows(0)!agrupar_posicion = "P" Then
                                            row("orden") = "A"
                                        ElseIf config.Tables(0).Rows(0)!agrupar_posicion = "F" Then
                                            row("orden") = "Z"
                                        End If
                                        row("estacion") = cadAgrupado
                                        row("total") = valAgrupado
                                        row("pct") = valAgrupado
                                        tmpTabla.Rows.Add(row)
                                    End If
                                    Dim view_o As DataView = New DataView(tmpTabla)
                                    If config.Tables(0).Rows(0)!agrupar_posicion = "O" Then
                                        view_o.Sort = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " total DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " total", "estacion"))

                                    Else
                                        view_o.Sort = "orden ASC," & IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " total DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " total", "estacion"))
                                    End If
                                    tabla_grafico = view_o.ToTable()
                                Else
                                    tabla_grafico = datos
                                End If
                            Else
                                tabla_grafico = datos
                            End If
                            Dim series1 As New Series(indicador01, ViewType.Bar)

                            ChartControl1.Series.Add(series1)
                            series1.DataSource = tabla_grafico
                            series1.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                            series1.View.Color = Color.SkyBlue
                            series1.ArgumentScaleType = ScaleType.Qualitative
                            series1.ArgumentDataMember = "estacion"
                            series1.ValueScaleType = ScaleType.Numerical
                            series1.ValueDataMembers.AddRange(New String() {"total"})
                            series1.Label.BackColor = Color.DarkBlue
                            series1.Label.TextColor = Color.White
                            series1.Label.Font = miFuente
                            series1.Label.TextPattern = "{V:F1}"

                            Dim series2 As New Series(indicador02, ViewType.Spline)

                            ChartControl1.Series.Add(series2)
                            series2.DataSource = tabla_grafico
                            series2.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True
                            series2.View.Color = Color.Green
                            series2.ArgumentScaleType = ScaleType.Qualitative
                            series2.ArgumentDataMember = "estacion"
                            series2.ValueScaleType = ScaleType.Numerical
                            series2.ValueDataMembers.AddRange(New String() {"pct"})
                            series2.Label.BackColor = Color.DarkBlue
                            series2.Label.TextColor = Color.White
                            series2.Label.Font = miFuente
                            series2.Label.TextPattern = "{V:F1}"

                            CType(ChartControl1.Diagram, XYDiagram).AxisY.Visibility = DevExpress.Utils.DefaultBoolean.True
                            CType(ChartControl1.Diagram, XYDiagram).AxisY.Label.Font = miFuenteEjes
                            CType(ChartControl1.Diagram, XYDiagram).AxisY.GridSpacingAuto = False
                            CType(ChartControl1.Diagram, XYDiagram).AxisY.GridSpacing = 1
                            CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Text = config.Tables(0).Rows(0)!texto_y
                            CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Font = miFuenteAlto
                            CType(ChartControl1.Diagram, XYDiagram).AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True
                            CType(ChartControl1.Diagram, XYDiagram).AxisY.GridLines.Visible = False
                            CType(ChartControl1.Diagram, XYDiagram).AxisY.Tickmarks.Visible = False
                            CType(ChartControl1.Diagram, XYDiagram).AxisY.Tickmarks.MinorVisible = False

                            Dim myAxisY As New SecondaryAxisY("PCT")
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
                            Titulo2.Text = "Extraccion de datos: " & cadPeriodo
                            ChartControl1.Titles.Add(Titulo2)
                            Dim Titulo3 As New ChartTitle()
                            Titulo3.Font = miFuente
                            Titulo3.Text = "Generado el: " & Format(Now, "ddd dd-MM-yyyy HH:mm:ss")
                            ChartControl1.Titles.Add(Titulo3)
                            Dim Titulo4 As New ChartTitle()
                            Titulo4.Font = miFuente
                            Titulo4.Text = "Extrayendo los datos desde: " & Format(eDesde, "dd-MM-yyyy HH:mm:ss") & " hasta: " &
                                    Format(eHasta, "dd-MM-yyyy HH:mm:ss")
                            ChartControl1.Titles.Add(Titulo4)
                            ChartControl1.Width = 1000
                            ChartControl1.Height = 700

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
                                agregarLOG("Ocurrió un error al intentar construir un archivo de adjunto de reporte (gráfico). Error: " & ex.Message, 7, 0)
                            End Try
                        End If
                    End If

                    'No hay datos, notificar
                End If
            End If
        End If

    End Function



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

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click

    End Sub
End Class

