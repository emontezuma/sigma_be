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
    Dim filtroAdicional As String
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



    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim argumentos As String() = Environment.GetCommandLineArgs()
        estadoPrograma=TRUE
        cadenaConexion = "server=127.0.0.1;user id=root;password=usbw;port=3307;Convert Zero Datetime=True;Allow User Variables=True"
        enviarReportes()
        If Process.GetProcessesByName _
          (Process.GetCurrentProcess.ProcessName).Length > 1 Then
        ElseIf argumentos.Length <= 1 Then
            MsgBox("String connection missing", MsgBoxStyle.Critical, "SIGMA")
        Else
            cadenaConexion = argumentos(1)
            Dim idProceso = Process.GetCurrentProcess.Id

            idProceso = Process.GetCurrentProcess.Id



            estadoPrograma = True
            enviarReportes()

        End If
        Application.Exit()
    End Sub

    Private Sub enviarReportes()
        'Se envía correo

        Dim cadSQL As String = "Select * FROM " & rutaBD & ".control WHERE fecha = '" & Format(Now, "yyyyMMddHH") & "' AND tipo = 5"
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
                            Dim cadFrecuencia As String = traduccion(5)
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

                            'eemv
                            'enviarDia = True


                            If enviarDia Then
                                Dim enviar As Boolean = False
                                Dim hora = Val(Format(Now, "HH"))
                                If envio(3) = "T" Then
                                    enviar = True
                                    cadFrecuencia = cadFrecuencia & traduccion(16)
                                ElseIf Val(envio(3)) = Val(hora) Then
                                    cadFrecuencia = cadFrecuencia & IIf(Val(hora) = 1, traduccion(18), traduccion(19) & Val(hora) & traduccion(17))
                                    enviar = True
                                End If


                                'eemv
                                'enviar = True


                                If enviar Then
                                    Dim mail As New MailMessage
                                    Try
                                        Dim cuerpo As String = ValNull(elmensaje!cuerpo, "A")
                                        Dim titulo As String = ValNull(elmensaje!titulo, "A")
                                        Dim ordenPareto As Integer = ValNull(elmensaje!orden, "N")
                                        If titulo.Length = 0 Then titulo = traduccion(20)
                                        If cuerpo.Length = 0 Then cuerpo = traduccion(21)

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
                                        cuerpo = cuerpo & vbCrLf & traduccion(22)

                                        cadSQL = "SELECT a.reporte, b.nombre, b.grafica, b.file_name, b.grafica FROM " & rutaBD & ".det_correo a INNER JOIN " & rutaBD & ".int_listados b ON a.reporte = b.id AND idioma = " & be_idioma & " WHERE a.correo = " & elmensaje!id & " ORDER BY b.orden"
                                        mensajesDS = consultaSEL(cadSQL)
                                        If mensajesDS.Tables(0).Rows.Count > 0 Then

                                            For Each reporte In mensajesDS.Tables(0).Rows
                                                Dim miReporte = generarReporte(reporte!reporte, reporte!nombre, reporte!file_name, envio(0), envio(1), rutaFiles, reporte!grafica, ordenPareto, elmensaje!consulta)
                                                If miReporte = -1 Then
                                                    cuerpo = cuerpo & vbCrLf & reporte!nombre & traduccion(23) & errorCorreos
                                                Else

                                                    If My.Computer.FileSystem.FileExists(rutaFiles & "\" & reporte!file_name & ".csv") Then
                                                        cuerpo = cuerpo & vbCrLf & reporte!nombre
                                                        Dim archivo As Attachment = New Attachment(rutaFiles & "\" & reporte!file_name & ".csv")
                                                        mail.Attachments.Add(archivo)
                                                    ElseIf miReporte = 2 Then
                                                        cuerpo = cuerpo & vbCrLf & reporte!nombre & traduccion(120)
                                                    Else
                                                        cuerpo = cuerpo & vbCrLf & reporte!nombre & traduccion(24)
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
                                        agregarLOG(traduccion(25) & ex.Message, 0, 9)
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
                        agregarLOG(traduccion(26).Replace("campo_0", tMensajes))
                    Else
                        agregarLOG(traduccion(27))
                    End If

                Else
                    agregarLOG(traduccion(25) & errorCorreo, 0, 9)
                End If
                smtpServer.Dispose()
            End If
            regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".control (fecha, mensajes, tipo) VALUES ('" & Format(Now, "yyyyMMddHH") & "', " & tMensajes & ", 5)")
        End If
    End Sub

    Function generarReporte(idReporte As Integer, reporte As String, fName As String, periodo As String, nperiodos As Integer, ruta As String, graficar As String, ordenPareto As Integer, consulta As Long) As Integer
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
        Dim cadPeriodo As String = nperiodos & traduccion(28)
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
        ElseIf periodo = 10 Then
            eDesde = CDate(Format(Now, "yyyy/MM/dd") & " 00:00:00")
            cadPeriodo = traduccion(35)
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
        If periodo < 10 Then eDesde = DateAdd(intervalo, ePeriodo * -1, eDesde)
        If periodo = 3 Then
            eDesde = Format(eDesde, "yyyy/MM/dd ") & "00:00:00"
            eHasta = CDate(Format(DateAdd(DateInterval.Day, -1, Now), "yyyy/MM/dd") & " 23:59:59")
        End If
        Dim fDesdeSF = Format(eDesde, "yyyy/MM/dd")
        Dim fHastaSF = Format(eHasta, "yyyy/MM/dd")
        Dim filtroAdicional As String

        filtroParos = " AND f.fecha >= '" & fDesdeSF & "' AND f.fecha <= '" & fHastaSF & "' "
        filtroFechas = " fecha >= '" & fDesdeSF & "' AND fecha <= '" & fHastaSF & "' "
        filtroReportes = " AND c.fecha_reporte >= '" & fDesdeSF & "' AND c.fecha_reporte <= '" & fHastaSF & "' "
        filtroFechasDia = " a.fecha >= '" & fDesdeSF & "' AND a.fecha <= '" & fHastaSF & "' "

        Dim comillas = Microsoft.VisualBasic.Strings.Left(Chr(34), 1)

        Dim inicial = ""
        Dim cabecera = ""
        Dim registros = ""
        Dim cadTitulo = traduccion(42)
        Dim cadTabla = "" & rutaBD & ".cat_lineas"
        Dim cadJoin = "c.linea"
        Dim cadCampo = ""
        Dim numTabla = ""
        Dim grupoFecha = ""
        Dim grupoFechaG = ""
        If idReporte = 2 Or idReporte = 15 Or idReporte = 28 Then
            cadTitulo = traduccion(43)
            cadTabla = "" & rutaBD & ".cat_maquinas"
            cadJoin = "c.maquina"
        ElseIf idReporte = 3 Or idReporte = 16 Or idReporte = 29 Then
            cadTitulo = traduccion(44)
            cadTabla = "" & rutaBD & ".cat_areas"
            cadJoin = "c.area"
        ElseIf idReporte = 4 Or idReporte = 17 Or idReporte = 30 Then
            cadTitulo = traduccion(45)
            cadTabla = "" & rutaBD & ".cat_fallas"
            cadJoin = "c.falla_ajustada"
        ElseIf idReporte = 5 Or idReporte = 18 Or idReporte = 31 Then
            cadTitulo = traduccion(46)
            cadTabla = "" & rutaBD & ".cat_maquinas"
            cadJoin = "c.maquina"
            cadCampo = "g.tipo"
            numTabla = "50"
        ElseIf idReporte = 6 Or idReporte = 19 Or idReporte = 32 Then
            cadTitulo = traduccion(47)
            cadTabla = "" & rutaBD & ".cat_maquinas"
            cadJoin = "c.maquina"
            cadCampo = "g.agrupador_1"
            numTabla = "20"
        ElseIf idReporte = 7 Or idReporte = 20 Or idReporte = 33 Then
            cadTitulo = traduccion(48)
            cadTabla = "" & rutaBD & ".cat_maquinas"
            cadJoin = "c.maquina"
            cadCampo = "g.agrupador_2"
            numTabla = "25"
        ElseIf idReporte = 8 Or idReporte = 21 Or idReporte = 34 Then
            cadTitulo = traduccion(49)
            cadTabla = "" & rutaBD & ".cat_fallas"
            cadJoin = "c.falla_ajustada"
            cadCampo = "g.agrupador_1"
            numTabla = "40"
        ElseIf idReporte = 9 Or idReporte = 22 Or idReporte = 35 Then
            cadTitulo = traduccion(50)
            cadTabla = "" & rutaBD & ".cat_fallas"
            cadJoin = "c.falla_ajustada"
            cadCampo = "g.agrupador_2"
            numTabla = "45"
        ElseIf idReporte = 10 Or idReporte = 23 Or idReporte = 36 Then
            cadTitulo = traduccion(51)
            grupoFecha = "DATE_FORMAT(a.fecha, '%d/%m/%Y') AS nombre"
            grupoFechaG = "DATE_FORMAT(a.fecha, '%d/%m/%Y') AS nombre"
        ElseIf idReporte = 11 Or idReporte = 24 Or idReporte = 37 Then
            cadTitulo = traduccion(52) & "', '" & traduccion(53)
            grupoFechaG = "DATE_FORMAT(a.fecha,'%x/%v') AS nombre"
            grupoFecha = "DATE_FORMAT(a.fecha,'%x/%v') AS nombre, STR_TO_DATE(CONCAT(DATE_FORMAT(a.fecha,'%x/%v'), ' Monday'), '%x/%v %W')"
        ElseIf idReporte = 12 Or idReporte = 25 Or idReporte = 38 Then
            cadTitulo = traduccion(54)
            grupoFecha = "DATE_FORMAT(a.fecha,'%Y/%m') AS nombre"
            grupoFechaG = "DATE_FORMAT(a.fecha,'%Y/%m') AS nombre"
        ElseIf idReporte = 13 Or idReporte = 26 Or idReporte = 39 Then
            cadTitulo = traduccion(55)
            cadTabla = "" & rutaBD & ".cat_usuarios"
            cadJoin = "c.tecnico"
        End If

        Dim Leer As Boolean = False
        Dim cadSQL = ""
        If idReporte >= 1 And idReporte <= 13 Then
            cadSQL = "SELECT * FROM " & rutaBD & ".pu_graficos WHERE (usuario = 1 OR usuario = 0) AND grafico = " & 100 + idReporte & " ORDER BY usuario DESC LIMIT 1"
        ElseIf idReporte >= 14 And idReporte <= 26 Then
            cadSQL = "SELECT * FROM " & rutaBD & ".pu_graficos WHERE (usuario = 1 OR usuario = 0) AND grafico = " & 200 + idReporte - 13 & " ORDER BY usuario DESC LIMIT 1"
        ElseIf idReporte >= 27 And idReporte <= 39 Then
            cadSQL = "SELECT * FROM " & rutaBD & ".pu_graficos WHERE (usuario = 1 OR usuario = 0) AND grafico = " & 300 + idReporte - 26 & " ORDER BY usuario DESC LIMIT 1"
        End If
        Dim tituloPareto = traduccion(106)
        Dim campoTotal As String = "c.tiempottl"
        Dim campoSumarizado As String = "SUM(tiemporeparacion + tiempollegada) / 3600"
        Dim campoPareto = "tiemporeparacion + tiempollegada"
        If ordenPareto = 1 Then
            campoTotal = "c.ttl"
            campoSumarizado = "COUNT(*)"
        ElseIf ordenPareto = 2 Then
            campoPareto = "tiempollegada"
            campoSumarizado = "SUM(tiempollegada) / 3600"
            tituloPareto = traduccion(118)
        ElseIf ordenPareto = 3 Then
            campoPareto = "tiemporeparacion"
            campoSumarizado = "SUM(tiemporeparacion) / 3600"
            tituloPareto = traduccion(119)
        End If
        Dim config As DataSet = consultaSEL(cadSQL)

        generarFIltro(consulta)

        If idReporte = 42 Then
            'Reporte de Reportes abiertas
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07, '' AS c08, '' AS c09, '' AS c10, '' AS c11, '' AS c12, '' AS c13, '' AS c14, '' AS c15, '' AS c16, '' AS c17, '' AS c18, '' AS c19, '' AS c20, '' AS c21, '' AS c22, '' AS c23, '' AS c24, '' AS c25, '' AS c26, '' AS c27, '' AS c28, '' AS c29, '' AS c30, '' AS c31, '' AS c32, '' AS c33, '' AS c34 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & traduccion(66) & "', '" & traduccion(67) & "', '" & traduccion(68) & "', '" & traduccion(69) & "', '" & traduccion(70) & "', '" & traduccion(71) & "', '" & traduccion(72) & "', '" & traduccion(73) & "', '" & traduccion(74) & "', '" & traduccion(75) & "', '" & traduccion(76) & "', '" & traduccion(77) & "', '" & traduccion(78) & "', '" & traduccion(79) & "', '" & traduccion(80) & "', '" & traduccion(81) & "', '" & traduccion(82) & "', '" & traduccion(83) & "', '" & traduccion(84) & "', '" & traduccion(85) & "', '" & traduccion(86) & "', '" & traduccion(87) & "', '" & traduccion(88) & "', '" & traduccion(89) & "', '" & traduccion(90) & "', '" & traduccion(91) & "', '" & traduccion(92) & "', '" & traduccion(93) & "', '" & traduccion(94) & "', '" & traduccion(95) & "', '" & traduccion(96) & "', '" & traduccion(97) & "', '" & traduccion(98) & "', '" & traduccion(99) & "') "
            registros = "UNION SELECT c.id, c.fecha, c.fecha_reporte, IFNULL(l.nombre, '" & traduccion(100) & "'), c.estatus, c.inicio_atencion, c.tiempollegada, c.cierre_atencion, c.tiemporeparacion, c.tiemporeparacion + c.tiempollegada, c.inicio_reporte, c.cierre_reporte, c.tiemporeporte, IFNULL(a.nombre, '" & traduccion(100) & "'), c.linea, IFNULL(b.nombre, '" & traduccion(100) & "'), c.maquina, IFNULL(d.nombre, '" & traduccion(100) & "'), c.area, IFNULL(e.nombre, '" & traduccion(100) & "'), c.falla_ajustada, IFNULL(f.nombre, '" & traduccion(100) & "'), IFNULL(j.nombre, '" & traduccion(100) & "'), IFNULL(h.nombre, '" & traduccion(100) & "'), IFNULL(g.nombre, '" & traduccion(100) & "'), IFNULL(m.nombre, '" & traduccion(100) & "'), IFNULL(k.nombre, '" & traduccion(100) & "'), c.detalle, IF(c.contabilizar = 'S', '" & traduccion(101) & "', '" & traduccion(102) & "'), IF(c.alarmado = 'S', '" & traduccion(101) & "', '" & traduccion(102) & "'), IF(c.alarmado_atender = 'S', '" & traduccion(101) & "', '" & traduccion(102) & "'), IF(c.alarmado_atendido = 'S', '" & traduccion(101) & "', '" & traduccion(102) & "'), IFNULL(i.nombre, '" & traduccion(100) & "'), c.falla FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_lineas a ON c.linea = a.id LEFT JOIN " & rutaBD & ".cat_maquinas b ON c.maquina = b.id LEFT JOIN " & rutaBD & ".cat_areas d ON c.area = d.id LEFT JOIN " & rutaBD & ".cat_fallas e ON c.falla_ajustada = e.id LEFT JOIN " & rutaBD & ".cat_usuarios f ON c.solicitante = f.id LEFT JOIN " & rutaBD & ".cat_usuarios g ON g.tecnico = g.id LEFT JOIN " & rutaBD & ".cat_usuarios h ON c.tecnicoatend = h.id LEFT JOIN " & rutaBD & ".cat_fallas i ON c.falla = i.id LEFT JOIN " & rutaBD & ".cat_generales j ON f.departamento = j.id LEFT JOIN " & rutaBD & ".cat_generales k ON c.tipo = k.id LEFT JOIN " & rutaBD & ".cat_turnos l ON c.turno = l.id LEFT JOIN " & rutaBD & ".cat_usuarios m ON c.confirmado = m.id WHERE c.estatus >= 0 " & filtroReportes

        ElseIf idReporte = 27 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07, '' AS c08 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', '" & traduccion(103) & "', '" & traduccion(104) & "', '" & traduccion(105) & "', '" & tituloPareto & "', '" & traduccion(107) & "', '" & traduccion(108) & "', '" & traduccion(109) & "') "
            registros = "UNION SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, c.referencia, campo, ttl, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT nombre, referencia, linea AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_lineas d ON linea = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 4, 5) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t  "

        ElseIf idReporte = 28 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07, '' AS c08 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', '" & traduccion(103) & "', '" & traduccion(104) & "', '" & traduccion(105) & "', '" & tituloPareto & "', '" & traduccion(107) & "', '" & traduccion(108) & "', '" & traduccion(109) & "') "
            registros = "UNION SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, c.referencia, campo, ttl, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT nombre, referencia, maquina AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_maquinas d ON maquina = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 4, 5) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t   "
        ElseIf idReporte = 29 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07, '' AS c08 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', '" & traduccion(103) & "', '" & traduccion(104) & "', '" & traduccion(105) & "', '" & tituloPareto & "', '" & traduccion(107) & "', '" & traduccion(108) & "', '" & traduccion(109) & "') "
            registros = "UNION SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, c.referencia, campo, ttl, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT nombre, referencia, area AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_areas d ON area = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 4, 5) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t    "
        ElseIf idReporte = 30 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07, '' AS c08 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', '" & traduccion(103) & "', '" & traduccion(104) & "', '" & traduccion(105) & "', '" & tituloPareto & "', '" & traduccion(107) & "', '" & traduccion(108) & "', '" & traduccion(109) & "') "
            registros = "UNION SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, c.referencia, campo, ttl, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT nombre, referencia, falla_ajustada AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_fallas d ON falla_ajustada = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 4, 5) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t    "
        ElseIf idReporte = 31 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', '" & traduccion(104) & "', '" & traduccion(105) & "', '" & tituloPareto & "', '" & traduccion(107) & "', '" & traduccion(108) & "', '" & traduccion(109) & "') "
            registros = "UNION SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, campo, ttl, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT e.nombre, d.tipo AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_maquinas d ON c.maquina = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.tipo = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 3, 4) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "
        ElseIf idReporte = 32 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', '" & traduccion(104) & "', '" & traduccion(105) & "', '" & tituloPareto & "', '" & traduccion(107) & "', '" & traduccion(108) & "', '" & traduccion(109) & "') "
            registros = "UNION SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, campo, ttl, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT e.nombre, d.agrupador_1 AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_maquinas d ON c.maquina = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.agrupador_1 = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 3, 4) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "
        ElseIf idReporte = 33 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', '" & traduccion(104) & "', '" & traduccion(105) & "', '" & tituloPareto & "', '" & traduccion(107) & "', '" & traduccion(108) & "', '" & traduccion(109) & "') "
            registros = "UNION SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, campo, ttl, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT e.nombre, d.agrupador_2 AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_maquinas d ON c.maquina = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.agrupador_2 = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 3, 4) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "
        ElseIf idReporte = 34 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', '" & traduccion(104) & "', '" & traduccion(105) & "', '" & tituloPareto & "', '" & traduccion(107) & "', '" & traduccion(108) & "', '" & traduccion(109) & "') "
            registros = "UNION SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, campo, ttl, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT e.nombre, d.agrupador_1 AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_fallas d ON c.falla_ajustada = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.agrupador_1 = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 3, 4) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "
        ElseIf idReporte = 35 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', '" & traduccion(104) & "', '" & traduccion(105) & "', '" & tituloPareto & "', '" & traduccion(107) & "', '" & traduccion(108) & "', '" & traduccion(109) & "') "
            registros = "UNION SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, campo, ttl, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT e.nombre, d.agrupador_2 AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_fallas d ON c.falla_ajustada = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.agrupador_2 = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 3, 4) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

        ElseIf idReporte = 36 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', '" & traduccion(105) & "', '" & tituloPareto & "', '" & traduccion(107) & "', '" & traduccion(108) & "', '" & traduccion(109) & "') "
            registros = "UNION SELECT campo AS nombre, ttl, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT fecha_reporte AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 2, 3) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "
        ElseIf idReporte = 37 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', '" & traduccion(105) & "', '" & tituloPareto & "', '" & traduccion(107) & "', '" & traduccion(108) & "', '" & traduccion(109) & "') "
            registros = "UNION SELECT campo AS nombre, campo2, ttl, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT DATE_FORMAT(fecha_reporte,'%x/%v') AS campo, STR_TO_DATE(CONCAT(DATE_FORMAT(fecha_reporte,'%x/%v'), ' Monday'), '%x/%v %W') AS campo2, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY DATE_FORMAT(fecha_reporte,'%x/%v') ORDER BY " & IIf(ordenPareto = 1, 2, 3) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

        ElseIf idReporte = 38 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', '" & traduccion(105) & "', '" & tituloPareto & "', '" & traduccion(107) & "', '" & traduccion(108) & "', '" & traduccion(109) & "') "
            registros = "UNION SELECT campo AS nombre, ttl, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT DATE_FORMAT(fecha_reporte,'%Y/%m') AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY DATE_FORMAT(fecha_reporte,'%Y/%m') ORDER BY " & IIf(ordenPareto = 1, 2, 3) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

        ElseIf idReporte = 39 Then
            inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07, '' AS c08 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '', '') "
            cabecera = "UNION (SELECT '" & cadTitulo & "', '" & traduccion(103) & "', '" & traduccion(104) & "', '" & traduccion(105) & "', '" & tituloPareto & "', '" & traduccion(107) & "', '" & traduccion(108) & "', '" & traduccion(109) & "') "
            Dim ordenA = " 1"


            If ordenPareto = 1 Then
                If ValNull(config.Tables(0).Rows(0)!orden_grafica, "A") = "M" Then
                    ordenA = " 4 DESC"
                ElseIf ValNull(config.Tables(0).Rows(0)!orden_grafica, "A") = "N" Then
                    ordenA = " 4"
                End If
                registros = "UNION SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, c.referencia, campo, ttl, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS porcentaje FROM (SELECT nombre, referencia, c.tecnico AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_usuarios d ON c.tecnico = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & ordenA & ") c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t  "
            Else
                If ValNull(config.Tables(0).Rows(0)!orden_grafica, "A") = "M" Then
                    ordenA = " 5 DESC"
                ElseIf ValNull(config.Tables(0).Rows(0)!orden_grafica, "A") = "N" Then
                    ordenA = " 5"
                End If
                registros = "UNION SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, c.referencia, campo, ttl, tiempottl, @total := @total + " & campoTotal & " As acumulado, t.total, @total / t.total * 100 As porcentaje FROM (Select nombre, referencia, c.tecnico As campo, COUNT(*) As ttl, SUM(" & campoPareto & ") / 3600 As tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_usuarios d On c.tecnico = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & ordenA & ") c, (SELECT @total := 0) AS total, (SELECT SUM(" & campoPareto & ") / 3600 AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t  "
            End If


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
                inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07, '' AS c08 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '', '') "
                cabecera = "UNION (SELECT '" & cadTitulo & "', '" & traduccion(103) & "', '" & traduccion(104) & "', '" & traduccion(115) & "', '" & traduccion(105) & "', '" & traduccion(113) & "', '" & traduccion(114) & "', '" & traduccion(116) & "') "
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
                inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '') "
                cabecera = "UNION (SELECT '" & cadTitulo & "', '" & traduccion(104) & "', '" & traduccion(115) & "', '" & traduccion(105) & "', '" & traduccion(113) & "', '" & traduccion(114) & "', '" & traduccion(116) & "') "
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
                inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '') "
                cabecera = "UNION (SELECT 'Dia', '" & traduccion(115) & "', '" & traduccion(105) & "', '" & traduccion(113) & "', '" & traduccion(114) & "', '" & traduccion(116) & "') "

                registros = "UNION SELECT a.fecha AS nombre, ROUND(IFNULL(SUM(c.tiemporeparacion + c.tiempollegada), 0)/ 3600, 1) AS tiempo_c, COUNT(c.id) AS docs, (IF(dia = 2, 1, 0) * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + IF(dia = 3, 1, 0) * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + IF(dia = 4, 1, 0) * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + IF(dia = 5, 1, 0) * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + IF(dia = 6, 1, 0) * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + IF(dia = 7, 1, 0) * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + IF(dia = 1, 1, 0) * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, IFNULL(SUM(c.tiemporeparacion + c.tiempollegada) / 3600 / COUNT(c.id), 0) AS mttrc, ((SELECT tdisponible) - IFNULL(SUM(f.tiempo), 0)) / IF(COUNT(c.id) = 0, 1, COUNT(c.id)) / 3600 AS mtbfc FROM " & rutaBD & ".dias a LEFT JOIN (SELECT id, maquina, linea, fecha_reporte, tiemporeparacion, tiempollegada FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ") AS c ON a.fecha = c.fecha_reporte LEFT JOIN (SELECT maquina, tiempo FROM " & rutaBD & ".detalleparos f WHERE f.contabilizar = 'S' " & filtroParos & " AND f.tipo = 1) AS f ON c.maquina = f.maquina WHERE " & filtroFechasDia & " GROUP BY nombre  "
            ElseIf Leer And (idReporte = 11 Or idReporte = 24) Then
                inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06, '' AS c07 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '', '') "
                cabecera = "UNION (SELECT '" & traduccion(52) & "'  , '" & traduccion(53) & "', '" & traduccion(117) & "', '" & traduccion(105) & "', '" & traduccion(113) & "', '" & traduccion(114) & "', '" & traduccion(116) & "') "
                registros = "UNION SELECT DATE_FORMAT(a.fecha,'%x/%v') AS nombre, STR_TO_DATE(CONCAT(DATE_FORMAT(a.fecha,'%x/%v'), ' Monday'), '%x/%v %W'), ROUND(IFNULL(SUM(c.tiemporeparacion + c.tiempollegada), 0)/ 3600, 1) AS tiempo_c, COUNT(c.id) AS docs, ((SELECT COUNT(*) FROM dias WHERE dia = 2 AND DATE_FORMAT(a.fecha,'%x/%v') = DATE_FORMAT(fecha,'%x/%v') AND " & filtroFechas & ") * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 3 AND DATE_FORMAT(a.fecha,'%x/%v') = DATE_FORMAT(fecha,'%x/%v') AND " & filtroFechas & ") * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 4 AND DATE_FORMAT(a.fecha,'%x/%v') = DATE_FORMAT(fecha,'%x/%v') AND " & filtroFechas & ") * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 5 AND DATE_FORMAT(a.fecha,'%x/%v') = DATE_FORMAT(fecha,'%x/%v') AND " & filtroFechas & ") * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 6 AND DATE_FORMAT(a.fecha,'%x/%v') = DATE_FORMAT(fecha,'%x/%v') AND " & filtroFechas & ") * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 7 AND DATE_FORMAT(a.fecha,'%x/%v') = DATE_FORMAT(fecha,'%x/%v') AND " & filtroFechas & ") * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 1 AND DATE_FORMAT(a.fecha,'%x/%v') = DATE_FORMAT(fecha,'%x/%v') AND " & filtroFechas & ") * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, IFNULL(SUM(c.tiemporeparacion + c.tiempollegada) / 3600 / COUNT(c.id), 0) AS mttrc, ((SELECT tdisponible) - IFNULL(SUM(f.tiempo), 0)) / IF(COUNT(c.id) = 0, 1, COUNT(c.id)) / 3600 AS mtbfc FROM " & rutaBD & ".dias a LEFT JOIN " & rutaBD & ".reportes c ON a.fecha = c.fecha_reporte AND c.contabilizar = 'S' AND c.estatus >= 100 LEFT JOIN " & rutaBD & ".detalleparos f ON c.maquina = f.maquina AND f.contabilizar = 'S' " & filtroParos & " AND f.tipo = 1 WHERE " & filtroFechasDia & " GROUP BY DATE_FORMAT(a.fecha,'%x/%v') "
            ElseIf Leer And (idReporte = 12 Or idReporte = 25) Then
                inicial = "USE sigma; SELECT * FROM (SELECT '" & reporte & " (" & cadPeriodo & ")' AS c01, '' AS c02, '' AS c03, '' AS c04, '' AS c05, '' AS c06 UNION (SELECT CONCAT('" & traduccion(110) & "', NOW()), '', '', '', '', '') UNION (SELECT CONCAT('" & traduccion(111) & "', '" & Format(eDesde, "dd/MMM/yyyy HH:mm:ss") & "', '" & traduccion(112) & "', '" & Format(eHasta, "dd/MMM/yyyy HH:mm:ss") & "'), '', '', '', '', '') "
                cabecera = "UNION (SELECT 'Mes', '" & traduccion(117) & "', '" & traduccion(105) & "', '" & traduccion(113) & "', '" & traduccion(114) & "', '" & traduccion(116) & "') "
                registros = "UNION SELECT DATE_FORMAT(a.fecha,'%Y%m') AS nombre, ROUND(IFNULL(SUM(c.tiemporeparacion + c.tiempollegada), 0)/ 3600, 1) AS tiempo_c, COUNT(c.id) AS docs, ((SELECT COUNT(*) FROM dias WHERE dia = 2 AND DATE_FORMAT(a.fecha,'%Y%m') = DATE_FORMAT(fecha,'%Y%m') AND " & filtroFechas & ") * IFNULL((SELECT lunes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 3 AND DATE_FORMAT(a.fecha,'%Y%m') = DATE_FORMAT(fecha,'%Y%m') AND " & filtroFechas & ") * IFNULL((SELECT martes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 4 AND DATE_FORMAT(a.fecha,'%Y%m') = DATE_FORMAT(fecha,'%Y%m') AND " & filtroFechas & ") * IFNULL((SELECT miercoles FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 5 AND DATE_FORMAT(a.fecha,'%Y%m') = DATE_FORMAT(fecha,'%Y%m') AND " & filtroFechas & ") * IFNULL((SELECT jueves FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 6 AND DATE_FORMAT(a.fecha,'%Y%m') = DATE_FORMAT(fecha,'%Y%m') AND " & filtroFechas & ") * IFNULL((SELECT viernes FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 7 AND DATE_FORMAT(a.fecha,'%Y%m') = DATE_FORMAT(fecha,'%Y%m') AND " & filtroFechas & ") * IFNULL((SELECT sabado FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0) + (SELECT COUNT(*) FROM dias WHERE dia = 1 AND DATE_FORMAT(a.fecha,'%Y%m') = DATE_FORMAT(fecha,'%Y%m') AND " & filtroFechas & ") * IFNULL((SELECT domingo FROM " & rutaBD & ".disponibilidad WHERE (estatus = 'A') AND ((c.linea = linea) OR (c.maquina = maquina) OR (maquina = 0 AND linea = 0)) ORDER BY maquina DESC, linea DESC LIMIT 1), 0)) AS tdisponible, IFNULL(SUM(c.tiemporeparacion + c.tiempollegada) / 3600 / COUNT(c.id), 0) AS mttrc, ((SELECT tdisponible) - IFNULL(SUM(f.tiempo), 0)) / IF(COUNT(c.id) = 0, 1, COUNT(c.id)) / 3600 AS mtbfc FROM " & rutaBD & ".dias a LEFT JOIN " & rutaBD & ".reportes c ON a.fecha = c.fecha_reporte AND c.contabilizar = 'S' AND c.estatus >= 100 LEFT JOIN " & rutaBD & ".detalleparos f ON c.maquina = f.maquina AND f.contabilizar = 'S' " & filtroParos & " AND f.tipo = 1 WHERE " & filtroFechasDia & " GROUP BY DATE_FORMAT(a.fecha,'%Y%m') "
            End If
        End If

        Dim regsAfectados = consultaACT(inicial & cabecera & registros & ") AS query01 INTO OUTFILE '" & archivoSaliente & "' FIELDS TERMINATED BY ',' OPTIONALLY ENCLOSED BY '" & comillas & "' ENCLOSED BY '" & comillas & "' LINES TERMINATED BY '\n'")
        If errorBD.Length > 0 Then
            If Not Leer Then
                generarReporte = 2
            Else
                errorCorreos = errorBD
                agregarLOG("Error al construir el reporte. " + errorBD, 7, 0)
                generarReporte = -1
            End If
        End If
        If graficar = "S" Then
            'Se produce el gráfico
            regsAfectados = 0

            If config.Tables(0).Rows.Count > 0 Then
                Dim cadOrden As String = ""
                If idReporte >= 1 And idReporte <= 4 Or idReporte = 13 Then
                    cadOrden = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " ORDER BY 3 DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " ORDER BY 3", "ORDER BY a.nombre"))
                    registros = "SELECT a.nombre, COUNT(c.id) AS docs, IFNULL(SUM(c.tiemporeparacion + c.tiempollegada) / 3600 / COUNT(c.id), 0) AS mttrc FROM " & cadTabla & " a LEFT JOIN " & rutaBD & ".reportes c ON a.id = " & cadJoin & " AND c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & IIf(idReporte = 13, " WHERE (a.rol = 'A' OR a.rol = 'T') ", "") & " GROUP BY a.nombre " & IIf(config.Tables(0).Rows(0)!incluir_ceros = "N", "HAVING docs > 0 ", "") & cadOrden

                ElseIf idReporte = 27 Then


                    registros = "SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, ttl AS mttrc, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS pct FROM (SELECT nombre, referencia, linea AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_lineas d ON linea = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 4, 5) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t  "
                ElseIf idReporte = 28 Then


                    registros = "SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, ttl AS mttrc, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS pct FROM (SELECT nombre, referencia, maquina AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_maquinas d ON maquina = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 4, 5) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t  "

                ElseIf idReporte = 29 Then


                    registros = "SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, ttl AS mttrc, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS pct FROM (SELECT nombre, referencia, area AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_areas d ON area = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 4, 5) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t  "

                ElseIf idReporte = 30 Then


                    registros = "SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, ttl AS mttrc, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS pct FROM (SELECT nombre, referencia, falla_ajustada AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_fallas d ON falla_ajustada = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 4, 5) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t  "

                ElseIf idReporte = 31 Then


                    registros = "SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, campo, ttl AS mttrc, tiempottl, @total := @total + " & campoTotal & " AS acumulado, @total / t.total * 100 AS pct FROM (SELECT e.nombre, d.tipo AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_maquinas d ON c.maquina = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.tipo = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 3, 4) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

                ElseIf idReporte = 32 Then


                    registros = "SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, campo, ttl AS mttrc, tiempottl, @total := @total + " & campoTotal & " AS acumulado, @total / t.total * 100 AS pct FROM (SELECT e.nombre, d.agrupador_1 AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_maquinas d ON c.maquina = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.agrupador_1 = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 3, 4) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

                ElseIf idReporte = 33 Then


                    registros = "SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, campo, ttl AS mttrc, tiempottl, @total := @total + " & campoTotal & " AS acumulado, @total / t.total * 100 AS pct FROM (SELECT e.nombre, d.agrupador_2 AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_maquinas d ON c.maquina = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.agrupador_2 = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 3, 4) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

                ElseIf idReporte = 34 Then


                    registros = "SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, campo, ttl AS mttrc, tiempottl, @total := @total + " & campoTotal & " AS acumulado, @total / t.total * 100 AS pct FROM (SELECT e.nombre, d.agrupador_1 AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_fallas d ON c.falla_ajustada = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.agrupador_1 = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 3, 4) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

                ElseIf idReporte = 35 Then


                    registros = "SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, campo, ttl AS mttrc, tiempottl, @total := @total + " & campoTotal & " AS acumulado, @total / t.total * 100 AS pct FROM (SELECT e.nombre, d.agrupador_2 AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_fallas d ON c.falla_ajustada = d.id LEFT JOIN " & rutaBD & ".cat_generales e ON d.agrupador_2 = e.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & IIf(ordenPareto = 1, 3, 4) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

                ElseIf idReporte = 36 Then


                    registros = "SELECT campo AS nombre, ttl AS mttrc, tiempottl, @total := @total + " & campoTotal & " AS acumulado, @total / t.total * 100 AS pct FROM (SELECT DATE_FORMAT(fecha_reporte, '%d/%m/%Y') AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY DATE_FORMAT(fecha_reporte, '%d/%m/%Y') ORDER BY " & IIf(ordenPareto = 1, 2, 3) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

                ElseIf idReporte = 37 Then


                    registros = "SELECT campo AS nombre, ttl AS mttrc, tiempottl, @total := @total + " & campoTotal & " AS acumulado, @total / t.total * 100 AS pct FROM (SELECT DATE_FORMAT(fecha_reporte,'%x/%v') AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY DATE_FORMAT(fecha_reporte,'%x/%v') ORDER BY " & IIf(ordenPareto = 1, 2, 3) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

                ElseIf idReporte = 38 Then
                    registros = "SELECT campo AS nombre, ttl AS mttrc, tiempottl, @total := @total + " & campoTotal & " AS acumulado, @total / t.total * 100 AS pct FROM (SELECT DATE_FORMAT(fecha_reporte,'%Y/%m') AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY DATE_FORMAT(fecha_reporte,'%Y/%m') ORDER BY " & IIf(ordenPareto = 1, 2, 3) & " DESC) c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t "

                ElseIf idReporte = 39 Then
                    Dim ordenA = " 1"
                    If ordenPareto = 1 Then
                        If ValNull(config.Tables(0).Rows(0)!orden_grafica, "A") = "M" Then
                            ordenA = " 4 DESC"
                        ElseIf ValNull(config.Tables(0).Rows(0)!orden_grafica, "A") = "N" Then
                            ordenA = " 4"
                        End If
                        registros = "SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, ttl AS mttrc, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS pct FROM (SELECT nombre, referencia, c.tecnico AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_usuarios d ON c.tecnico = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & ordenA & ") c, (SELECT @total := 0) AS total, (SELECT " & campoSumarizado & " AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t  "
                    Else
                        If ValNull(config.Tables(0).Rows(0)!orden_grafica, "A") = "M" Then
                            ordenA = " 5 DESC"
                        ElseIf ValNull(config.Tables(0).Rows(0)!orden_grafica, "A") = "N" Then
                            ordenA = " 5"
                        End If
                        registros = "SELECT IFNULL(c.nombre, '" & traduccion(100) & "') AS nombre, ttl AS mttrc, tiempottl, @total := @total + " & campoTotal & " AS acumulado, t.total, @total / t.total * 100 AS pct FROM (SELECT nombre, referencia, c.tecnico AS campo, COUNT(*) AS ttl, SUM(" & campoPareto & ") / 3600 AS tiempottl FROM " & rutaBD & ".reportes c LEFT JOIN " & rutaBD & ".cat_usuarios d ON c.tecnico = d.id WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY campo ORDER BY " & ordenA & ") c, (SELECT @total := 0) AS total, (SELECT SUM(" & campoPareto & ") / 3600 AS total FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & ")  AS t  "
                    End If

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
                ElseIf idReporte >= 23 And idReporte <= 25 Then


                    cadOrden = IIf(config.Tables(0).Rows(0)!orden_grafica = "M", " ORDER BY 4 DESC", IIf(config.Tables(0).Rows(0)!orden_grafica = "N", " ORDER BY 4", "ORDER BY a.nombre"))
                    registros = "SELECT " & grupoFecha & ", IFNULL(IF(a.dia = 1, g.domingo, IF(a.dia = 2, g.lunes, IF(a.dia = 3, g.martes, IF(a.dia = 4, g.miercoles, IF(a.dia = 5, g.jueves, IF(a.dia = 6, g.viernes, g.sabado)))))), 0) AS tdisponible, IFNULL(IF(a.dia = 1, g.domingo, IF(a.dia = 2, g.lunes, IF(a.dia = 3, g.martes, IF(a.dia = 4, g.miercoles, IF(a.dia = 5, g.jueves, IF(a.dia = 6, g.viernes, g.sabado)))))), 0) / (IFNULL(SUM(c.reps), 0) + 1) / 3600 AS mtbfc, IFNULL(SUM(c.reps), 0) AS docs, IFNULL(SUM(c.tiempo), 0) AS tiempo_c, IF(IFNULL(SUM(c.reps), 0) = 0, 0, IFNULL(SUM(c.tiempo), 0) / IFNULL(SUM(c.reps), 0) / 3600) AS mttrc FROM " & rutaBD & ".dias a LEFT JOIN (SELECT fecha_reporte, COUNT(*) AS reps, SUM(tiemporeparacion + tiempollegada) AS tiempo FROM " & rutaBD & ".reportes c WHERE c.contabilizar = 'S' AND c.estatus >= 100 " & filtroReportes & " GROUP BY fecha_reporte) AS c ON a.fecha = c.fecha_reporte LEFT JOIN (SELECT DATE(fecha) AS dia, SUM(tiempo) AS tiempo FROM " & rutaBD & ".detalleparos f WHERE f.contabilizar = 'S' " & filtroParos & " AND f.tipo = 1 GROUP BY 1) AS f ON a.fecha = f.dia, (SELECT * FROM " & rutaBD & ".disponibilidad WHERE estatus = 'A' AND maquina = 0 AND linea = 0 LIMIT 1) AS g WHERE fecha >= '" & fDesdeSF & "' AND fecha <= '" & fHastaSF & "' GROUP BY nombre " & IIf(config.Tables(0).Rows(0)!incluir_ceros = "N", "HAVING docs > 0 ", "") & " " & cadOrden & ";"
                End If

                Dim indicador01 = traduccion(64)
                Dim indicador02 = traduccion(63)

                Dim graficos As DataSet = consultaSEL(registros)
                If graficos.Tables(0).Rows.Count > 0 Then
                    If idReporte >= 1 And idReporte <= 26 Then
                        If idReporte >= 1 And idReporte <= 13 Then
                            indicador01 = traduccion(64)
                            indicador02 = traduccion(64)
                        ElseIf idReporte >= 14 And idReporte <= 26 Then
                            indicador01 = traduccion(65)
                            indicador02 = traduccion(65)
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
                                    If cadAgrupado.Length = 0 Then cadAgrupado = traduccion(61)
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
                        Titulo2.Text = traduccion(56) & cadPeriodo
                        ChartControl1.Titles.Add(Titulo2)
                        Dim Titulo3 As New ChartTitle()
                        Titulo3.Font = miFuente
                        Titulo3.Text = traduccion(57) & Format(Now, "ddd dd-MM-yyyy HH:mm:ss")
                        ChartControl1.Titles.Add(Titulo3)
                        Dim Titulo4 As New ChartTitle()
                        Titulo4.Font = miFuente
                        Titulo4.Text = traduccion(58) & Format(eDesde, "dd-MM-yyyy HH:mm:ss") & traduccion(59) &
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
                            agregarLOG(traduccion(60) & ex.Message, 7, 0)
                        End Try
                    ElseIf idReporte >= 27 And idReporte <= 39 Then
                        If idReporte >= 27 And idReporte <= 39 Then
                            indicador01 = traduccion(62)
                            indicador02 = traduccion(63)
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
                        datos.Columns.Add(traduccion(63), GetType(Decimal))
                        tabla_grafico.Columns.Add("orden", GetType(String))
                        tabla_grafico.Columns.Add("estacion", GetType(String))
                        tabla_grafico.Columns.Add("total", GetType(Decimal))
                        tabla_grafico.Columns.Add(traduccion(63), GetType(Decimal))

                        For Each lineas In graficos.Tables(0).Rows
                            row = datos.NewRow()
                            row("orden") = "A"
                            row("estacion") = lineas!nombre
                            If ordenPareto = 1 Then
                                row("total") = lineas!mttrc
                            Else
                                row("total") = lineas!tiempottl
                            End If
                            row(traduccion(63)) = lineas!pct
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
                                tmpTabla.Columns.Add(traduccion(63), GetType(Decimal))


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
                                        row(traduccion(63)) = filas!pct
                                        tmpTabla.Rows.Add(row)
                                    End If
                                Next
                                If valAgrupado > 0 And config.Tables(0).Rows(0)!agrupar = "S" Then
                                    Dim cadAgrupado As String = ValNull(config.Tables(0).Rows(0)!agrupar_texto, "A")
                                    If cadAgrupado.Length = 0 Then cadAgrupado = traduccion(61)
                                    cadAgrupado = cadAgrupado & " (" & varios & ")"
                                    row = tmpTabla.NewRow()
                                    If config.Tables(0).Rows(0)!agrupar_posicion = "P" Then
                                        row("orden") = "A"
                                    ElseIf config.Tables(0).Rows(0)!agrupar_posicion = "F" Then
                                        row("orden") = "Z"
                                    End If
                                    row("estacion") = cadAgrupado
                                    row("total") = valAgrupado
                                    row(traduccion(63)) = valAgrupado
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
                        series2.ValueDataMembers.AddRange(New String() {traduccion(63)})
                        series2.Label.BackColor = Color.DarkBlue
                        series2.Label.TextColor = Color.White
                        series2.Label.Font = miFuente
                        series2.Label.TextPattern = "{V:F1}"

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
                            agregarLOG(traduccion(60) & ex.Message, 7, 0)
                        End Try
                    End If
                End If

                'No hay datos, notificar
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
            If general.Tables(0).Rows(0)!filtroori = "0" Then

                filtroReportes = filtroReportes & " AND c.origen = 0 "
            End If
        ElseIf general.Tables(0).Rows(0)!filtroori = "1" Then

            filtroReportes = filtroReportes & " AND c.origen > 0 "
        End If
        If general.Tables(0).Rows(0)!filtrolin = "N" Then

            filtroReportes = filtroReportes & " AND c.linea IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 10) "
            filtroOEE = filtroOEE & " AND d.linea IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 10) "
            filtroHXH = filtroHXH & " AND b.linea IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 10) "
            filtroMTBF = " AND a.linea IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 10) "
            filtroOEE = filtroOEE & " AND a.linea IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 10) "
            filtroOEEDias = filtroOEEDias & " AND j.linea IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 10) "
            filtroParos = " AND b.linea IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 10) "
            filtroRechazos = " AND j.linea IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 10) "
        End If
        If general.Tables(0).Rows(0)!filtromaq = "N" Then

            filtroReportes = filtroReportes & " AND c.maquina IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 20) "
            filtroOEE = filtroOEE & " AND c.equipo IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 20) "
            filtroHXH = filtroHXH & " AND a.equipo IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 20) "
            filtroWIP = filtroWIP & " AND a.equipo IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 20) "
            filtroTrazabilidad = filtroTrazabilidad & " AND a.equipo IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 20) "
            filtroCalidad = filtroCalidad & " AND a.equipo IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 20) "
            filtroMTBF = filtroMTBF & " AND a.id IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 20) "

            filtroOEE = filtroOEE & " AND a.id IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 20) "
            filtroOEEDias = filtroOEEDias & " AND i.equipo IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 20) "
            filtroParos = filtroParos & " AND a.maquina IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 20) "
            filtroRechazos = filtroRechazos & " AND i.equipo IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 20) "
        End If
        If general.Tables(0).Rows(0)!filtroare = "N" Then
            filtroMTBF_are = " AND a.id IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 30) "

            filtroReportes = filtroReportes & " AND c.area IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 30) "
            filtroReportes = filtroReportes & " AND c.area IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 30) "
            filtroParos = filtroParos & " AND a.area IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 30) "
            filtroRechazos = filtroRechazos & " AND i.area IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 30) "
        End If
        If general.Tables(0).Rows(0)!filtrofal = "N" Then

            filtroReportes = filtroReportes & " AND c.falla IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 40) "
            filtroMTBF_fal = " AND a.id IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 40) "
        End If
        If general.Tables(0).Rows(0)!filtrotec = "N" Then

            filtroReportes = filtroReportes & " AND c.tecnico IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 50) "
            filtroMTBF_tec = " AND a.id IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 50) "
        End If
        If general.Tables(0).Rows(0)!filtronpar = "N" Then

            filtroOEE = filtroOEE & " AND c.parte IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 60) "
            filtroHXH = filtroHXH & " AND a.parte IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 60) "
            filtroWIP = filtroWIP & " AND a.parte IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 60) "
            filtroTrazabilidad = filtroTrazabilidad & " AND a.parte IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 60) "
            filtroCalidad = filtroCalidad & " AND a.parte IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 60) "
            filtroOEEDias = filtroOEEDias & " AND i.parte IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 60) "
            filtroParos = filtroParos & " AND a.tipo IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 100) "
            filtroRechazos = filtroRechazos & " AND i.tipo IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 120) "
            filtroCorte = filtroCorte & " AND i.parte IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 60) "
            filtroParos = filtroParos & " AND a.parte IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 60) "
            filtroRechazos = filtroRechazos & " AND i.parte IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 60) "

            filtroOEEDias = filtroOEEDias & " AND i.parte IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 60) "
        End If
        If general.Tables(0).Rows(0)!filtrotur = "N" Then

            filtroReportes = filtroReportes & " AND c.turno IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 70) "
            filtroOEE = filtroOEE & " AND c.turno IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 70) "
            filtroHXH = filtroHXH & " AND a.turno IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 70) "
            filtroOEEDias = filtroOEEDias & " AND i.turno IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 70) "
            filtroCorte = filtroCorte & " AND i.turno IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 70) "
            filtroParos = filtroParos & " AND a.turno IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 70) "
            filtroRechazos = filtroRechazos & " AND i.turno IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 70) "

            filtroOEEDias = filtroOEEDias & " AND i.turno IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & rutaBD & " AND tabla = 70) "
        End If
        If general.Tables(0).Rows(0)!filtroord = "N" Then

            filtroOEE = filtroOEE & " AND c.orden IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 80) "
            filtroHXH = filtroHXH & " AND a.lote IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 80) "
            filtroWIP = filtroWIP & " AND a.id IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 80) "
            filtroTrazabilidad = filtroTrazabilidad & " AND a.lote IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 80) "
            filtroCalidad = filtroCalidad & " AND a.lote IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 80) "
            filtroOEEDias = filtroOEEDias & " AND i.orden IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 80) "
            filtroCorte = filtroCorte & " AND i.orden IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 80) "
            filtroParos = filtroParos & " AND a.lote IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 80) "
            filtroRechazos = filtroRechazos & " AND i.orden IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 80) "
            filtroOEEDias = filtroOEEDias & " AND i.orden IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 80) "
        End If
        If general.Tables(0).Rows(0)!filtrooper = "N" Then

            filtroWIP = filtroWIP & " AND a.proceso IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 90) "
            filtroTrazabilidad = filtroTrazabilidad & " AND a.proceso IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 90) "
            filtroCalidad = filtroCalidad & " AND a.proceso IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 90) "
        End If

        If general.Tables(0).Rows(0)!filtrocla = "N" Then

            filtroParos = filtroParos & " AND a.clase IN (SELECT valor FROM " & rutaBD & ".consultas_det WHERE consulta = " & consulta & " AND tabla = 110) "
        End If

    End Sub

End Class

