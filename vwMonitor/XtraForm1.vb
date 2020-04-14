Imports DevExpress.XtraEditors
Imports DevExpress.Skins
Imports MySql.Data.MySqlClient
Imports System.Speech.Synthesis
Imports System.IO.Ports
Imports System.IO
Imports System.Text
Imports System.Net.Mail
Imports System.Net.Http
Imports System.Net
Imports System.ComponentModel


Public Class    XtraForm1
    Dim Estado As Integer = 0
    Dim leyendoLog As Boolean = False
    Dim enMonitor As Boolean = False
    Dim procesandoAudios As Boolean = False
    Dim reenviar As Boolean = False
    Dim procesandoEscalamientos As Boolean
    Dim procesandoRepeticiones As Boolean
    Dim estadoPrograma As Boolean
    Dim eSegundos = 0
    Dim MensajeLlamada = ""
    Dim procesandoMensajes As Boolean = False
    Dim enviandoReportes As Boolean = False
    Dim errorCorreos As String
    Dim telefonos As String()
    Dim mmcalls As String()
    Dim losCorreos As String()
    Dim depurando As Boolean = False, primerSensor As Boolean = True
    Dim revisandoSensores As Boolean = False
    Dim incluyeHoyos = False
    Dim be_log_lineas As Integer
    Dim be_log_activar As Boolean = False
    Dim be_audios_activar As Boolean = False
    Dim entroReportes As Boolean = False
    Dim idProceso
    Private Sub XtraForm1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim argumentos As String() = Environment.GetCommandLineArgs()
        If Process.GetProcessesByName _
          (Process.GetCurrentProcess.ProcessName).Length > 1 Then
            XtraMessageBox.Show("SIGMA Monitor ya se está ejecutando en este equipo", "Sesión iniciada", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Application.Exit()
            Exit Sub
        ElseIf argumentos.Length <= 1 Then
            XtraMessageBox.Show("No se puede iniciar el monitor: Se requiere la cadena de conexión", "Sesión iniciada", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Application.Exit()
            Exit Sub
        Else
            idProceso = Process.GetCurrentProcess.Id
            'cadenaConexion = argumentos(1)
            cadenaConexion = "server=localhost;user id=root;password=usbw;port=3307;Convert Zero Datetime=True;Allow User Variables=True"
            iniciarPantalla()

        End If
        BarStaticItem1.Caption = "Ejecutando desde " & Format(Now(), "dddd, dd-MMM-yyyy HH:mm:ss")
        agregarSolo("Se inicia la aplicación de monitoreo")
        estadoPrograma = True
    End Sub

    Sub iniciarPantalla()
        Dim regsAfectados As Integer = 0
        ListBoxControl1.Items.Clear()

        'Se escribe en la base de datos
        regsAfectados = consultaACT("UPDATE " & rutaBD & ".configuracion SET ejecutando_desde = NOW()")
        If errorBD.Length > 0 Then
            'Error en la base de datos
            agregarLOG("Ocurrió un error al intentar ejecutar una actualización en la base de datos de " & rutaBD & ". Error: " + errorBD, 9, 0)
        ElseIf regsAfectados = 0 Then
            regsAfectados = consultaACT("INSERT INTO configuracion (ejecutando_desde, revisar_cada) VALUES ('" & Format(horaDesde, "yyyy/MM/dd HH:mm:ss") & "', 60)")
        End If
        BarManager1.Items(0).Caption = "Ejecutandose desde: " + Format(horaDesde, "ddd, dd-MMM-yyyy HH:mm:ss")

    End Sub

    Private Sub XtraForm1_SizeChanged(sender As Object, e As EventArgs) Handles Me.SizeChanged
        ListBoxControl1.Width = Me.Width - 30
        GroupControl1.Width = ListBoxControl1.Width
        ListBoxControl1.Height = Me.Height - 250
        SimpleButton3.Left = Me.Width - SimpleButton3.Width - 20
        SimpleButton2.Left = Me.Width - SimpleButton2.Width - 20

    End Sub

    Private Sub SimpleButton1_Click(sender As Object, e As EventArgs) Handles SimpleButton1.Click
        If XtraMessageBox.Show("El log actual se quitará de la pantalla definitivamente. ¿Desea continuar?", "Inicializar LOG en pantalla", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.No Then
            Dim totalRegs As Integer = ListBoxControl1.Items.Count
            ListBoxControl1.Items.Clear()
            ListBoxControl1.Items.Add(Format(Now, "dd-MMM-yyyy HH:mm:ss") & ": " + "Se inicializa el LOG a solicitud del usuario. Se eliminan " & totalRegs & " registro(s) del LOG acumulandose desde " & Format(horaDesde, "dd-MMM-yyyy HH:mm:ss"))
            horaDesde = Now
            ContarLOG()
        End If
    End Sub

    Private Sub SimpleButton3_Click(sender As Object, e As EventArgs) Handles SimpleButton3.Click
        autenticado = False
        Dim Forma As New XtraForm2
        Forma.Text = "Detener aplicación"
        Forma.ShowDialog()
        If autenticado Then
            If XtraMessageBox.Show("Esta acción detendrá el envío de alertas. ¿Desea detener el monitor de alertas?", "Detener la aplicación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.No Then
                Estado = 1
                SimpleButton3.Visible = False
                SimpleButton2.Visible = True
                ContextMenuStrip1.Items(1).Enabled = False
                ContextMenuStrip1.Items(2).Enabled = True
                estadoPrograma = False
                agregarLOG("La interfaz ha sido detenida por el usuario: " & usuarioCerrar, 9, 0)
            End If
        End If
    End Sub

    Private Sub SimpleButton2_Click(sender As Object, e As EventArgs) Handles SimpleButton2.Click
        If XtraMessageBox.Show("Esta acción reanudará el envío de alertas. ¿Desea reanudar el monitor de alertas?", "Reanudar la aplicación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.No Then
            Estado = 1
            SimpleButton3.Visible = True
            SimpleButton2.Visible = False
            ContextMenuStrip1.Items(1).Enabled = True
            ContextMenuStrip1.Items(2).Enabled = False
            'enviarCorreos()
            estadoPrograma = True
            agregarLOG("La interfaz ha sido reanudada por un usuario", 9, 0)
        End If
    End Sub

    Private Sub agregarLOG(cadena As String, tipo As Integer, reporte As Integer, Optional aplicacion As Integer = 1)
        If Not be_log_activar Then Exit Sub
        'Se agrega a la base de datos
        'tipo 1: Info
        'tipo 2: Incongruencia en los datos (usuario)
        'tipo 9: Error crítico de Base de datos sigma
        Dim regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".log (aplicacion, tipo, texto) VALUES (0, " & tipo & ", '" & Microsoft.VisualBasic.Strings.Left(cadena, 250) & "')")

    End Sub

    Private Sub ContarLOG()
        If be_log_lineas = 0 Then be_log_lineas = 1000
        If ListBoxControl1.Items.Count > be_log_lineas Then
            For i = ListBoxControl1.Items.Count - 1 To be_log_lineas Step -1
                ListBoxControl1.Items.RemoveAt(i)
            Next
        End If
        BarStaticItem2.Caption = IIf(ListBoxControl1.Items.Count = 0, "Ningún registro en el visor", IIf(ListBoxControl1.Items.Count = 1, "Un registro en el visor", ListBoxControl1.Items.Count & " registros en el visor"))
    End Sub

    Private Sub HyperlinkLabelControl1_Click(sender As Object, e As EventArgs) Handles HyperlinkLabelControl1.Click
        System.Diagnostics.Process.Start("www.mmcallmexico.mx")
    End Sub

    Private Sub ComboBoxEdit2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBoxEdit2.SelectedIndexChanged
        Dim MiFuente As Font = New System.Drawing.Font("Lucida Sans", 9, FontStyle.Regular)

        If ComboBoxEdit2.SelectedIndex = 0 Then

            ListBoxControl1.Font = MiFuente

        ElseIf ComboBoxEdit2.SelectedIndex = 1 Then
            MiFuente = New System.Drawing.Font("Lucida Sans", 6, FontStyle.Regular)
            ListBoxControl1.Font = MiFuente
        ElseIf ComboBoxEdit2.SelectedIndex = 2 Then
            MiFuente = New System.Drawing.Font("Lucida Sans", 7, FontStyle.Regular)
            ListBoxControl1.Font = MiFuente

        ElseIf ComboBoxEdit2.SelectedIndex = 3 Then
            MiFuente = New System.Drawing.Font("Lucida Sans", 11, FontStyle.Regular)
            ListBoxControl1.Font = MiFuente
        ElseIf ComboBoxEdit2.SelectedIndex = 4 Then
            MiFuente = New System.Drawing.Font("Lucida Sans", 13, FontStyle.Regular)
            ListBoxControl1.Font = MiFuente
        ElseIf ComboBoxEdit2.SelectedIndex = 5 Then
            MiFuente = New System.Drawing.Font("Lucida Sans", 15, FontStyle.Regular)
            ListBoxControl1.Font = MiFuente
        End If
    End Sub

    Private Sub revisaFlag_Tick(sender As Object, e As EventArgs) Handles revisaFlag.Tick
        If enMonitor Or Not estadoPrograma Then Exit Sub

        validarLicencia()

        horaDesde = Now
        Dim cadSQL As String = "SELECT mapa_solicitud, ruta_programa_mapa, be_log_lineas, be_log_activar, audios_activar FROM " & rutaBD & ".configuracion"
        Dim reader As DataSet = consultaSEL(cadSQL)
        Dim regsAfectados = 0
        Dim ruta_programa_mapa As String
        If errorBD.Length > 0 Then
            agregarLOG("No se logró la conexión con MySQL. Error: " + errorBD, 9, 0)

        Else
            be_log_activar = ValNull(reader.Tables(0).Rows(0)!be_log_activar, "A") = "S"
            be_audios_activar = ValNull(reader.Tables(0).Rows(0)!audios_activar, "A") = "S"
            be_log_lineas = ValNull(reader.Tables(0).Rows(0)!be_log_lineas, "N")
            ruta_programa_mapa = ValNull(reader.Tables(0).Rows(0)!ruta_programa_mapa, "A")
            If be_log_lineas = 0 Then be_log_lineas = 1000
            If ValNull(reader.Tables(0).Rows(0)!mapa_solicitud, "A") = "S" Then
                Try
                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".configuracion SET mapa_solicitud = 'P'")
                    ruta_programa_mapa = Strings.Replace(ruta_programa_mapa, "/", "\")
                    Shell(ruta_programa_mapa, AppWinStyle.MinimizedNoFocus)

                Catch ex As Exception
                    agregarSolo("Se generó un error al convertir el mapa, revise el log")
                End Try
            ElseIf ValNull(reader.Tables(0).Rows(0)!mapa_solicitud, "A") = "Z" Then
                regsAfectados = consultaACT("UPDATE " & rutaBD & ".configuracion SET mapa_ultimo = NOW(), mapa_solicitud = 'A'")
                agregarSolo("Se ha procesado una presentación de manera satisfacoria")

            End If

        End If
        'calcularRevision()
        enMonitor = True
        'revisaFlag.Enabled = False
        'revisarEventos()
        cancelarAlertas()
        'paseaStock()
        'asignarCarga()
        'calcularEstimado()
        'depurar()
        enMonitor = False

        'enviar_mensajes()

        revisaFlag.Enabled = True

    End Sub
    Private Sub enviar_mensajes()

        If Not estadoPrograma Then Exit Sub
        Dim cadSQL = "SELECT canal FROM " & rutaBD & ".mensajes WHERE estatus = 'E' GROUP BY canal"
        Dim falla As DataSet = consultaSEL(cadSQL)
        Dim AppFuncion = ""

        If falla.Tables(0).Rows.Count > 0 Then
            For Each lotes In falla.Tables(0).Rows
                If Not estadoPrograma Then Exit Sub
                sinEventos.Enabled = False
                sinEventos.Enabled = True
                Try
                    If lotes!canal = 0 Then
                        AppFuncion = "voz.exe"
                        Shell(Application.StartupPath & "\voz.exe " & Chr(34) & cadenaConexion & Chr(34), AppWinStyle.MinimizedNoFocus)
                        agregarSolo("Se inicia la aplicación de generación de audio de llamada")
                    ElseIf lotes!canal = 1 Then
                        AppFuncion = "sms.exe"
                        Shell(Application.StartupPath & "\sms.exe " & Chr(34) & cadenaConexion & Chr(34), AppWinStyle.MinimizedNoFocus)
                        agregarSolo("Se inicia la aplicación de generación de mensajes de texto (SMS)")
                    ElseIf lotes!canal = 2 Then
                        AppFuncion = "mensajes.exe"
                        Shell(Application.StartupPath & "\mensajes.exe " & Chr(34) & cadenaConexion & Chr(34), AppWinStyle.MinimizedNoFocus)
                        agregarSolo("Se inicia la aplicación de generación de correos electrónicos")
                    ElseIf lotes!canal = 3 Then
                        AppFuncion = "mmcall.exe"
                        Shell(Application.StartupPath & "\mmcall.exe " & Chr(34) & cadenaConexion & Chr(34), AppWinStyle.MinimizedNoFocus)
                        agregarSolo("Se inicia la aplicación de mensajes de texto a MMCall")
                    ElseIf lotes!canal = 4 Then
                        AppFuncion = "log.exe"
                        Shell(Application.StartupPath & "\log.exe " & Chr(34) & cadenaConexion & Chr(34), AppWinStyle.MinimizedNoFocus)
                        agregarSolo("Se inicia la aplicación de generación LOGs")
                    End If
                Catch ex As Exception
                    agregarLOG("Error en la ejecución de la aplicación " & AppFuncion & ". Error: " & ex.Message, 7, 0)
                End Try

            Next
        End If

        If be_audios_activar Then
            cadSQL = "SELECT id FROM " & rutaBD & ".reportes WHERE estatus = 0 LIMIT 1"
            falla = consultaSEL(cadSQL)

            If falla.Tables(0).Rows.Count > 0 Then
                AppFuncion = "voz.exe"
                'Shell(Application.StartupPath & "\voz.exe " & Chr(34) & cadenaConexion & Chr(34), AppWinStyle.MinimizedNoFocus)
                agregarSolo("Se inicia la aplicación de generación de audio de llamada")
            End If

        End If
        If Not estadoPrograma Then Exit Sub


    End Sub

    Private Sub cancelarAlertas()
        Dim regsAfectados = 0
        Dim veces = 0
        Dim pases = 0

        'Alertas ANDON
        Dim cadSQL = "SELECT a.*, c.informar_resolucion, c.evento AS tipoalerta FROM " & rutaBD & ".alarmas a INNER JOIN " & rutaBD & ".cat_alertas c ON a.alerta = c.id INNER JOIN " & rutaBD & ".reportes b ON a.proceso = b.id AND ((b.alarmado_atender = 'S' AND b.estatus > 0) OR (b.alarmado_atendido = 'S' AND b.estatus > 10) OR (b.alarmado = 'S' AND b.estatus > 100)) WHERE a.estatus <> 9"

        Dim falla As DataSet = consultaSEL(cadSQL)
        If falla.Tables(0).Rows.Count > 0 Then
            For Each lotes In falla.Tables(0).Rows
                Dim cadAdic = "UPDATE " & rutaBD & ".reportes SET alarmado_atender = 'Y', generar_audio = 'Z' WHERE id = " & lotes!proceso
                If lotes!tipoalerta = 102 Then
                    cadAdic = "UPDATE " & rutaBD & ".reportes SET alarmado_atendido = 'Y' WHERE id = " & lotes!proceso
                ElseIf lotes!tipoalerta = 103 Then
                    cadAdic = "UPDATE " & rutaBD & ".reportes SET alarmado = 'Y' WHERE id = " & lotes!proceso
                End If
                regsAfectados = consultaACT("UPDATE " & rutaBD & ".alarmas SET estatus = 9, fin = NOW(), tiempo = TIME_TO_SEC(TIMEDIFF(NOW(), inicio))" & IIf(ValNull(lotes!informar_resolucion, "A") = "S", ", informado = 'S'", "") & " WHERE id = " & lotes!id & ";" & cadAdic & ";UPDATE " & rutaBD & ".mensajes SET estatus = 'Z' WHERE alarma = " & lotes!id)
                If ValNull(lotes!informar_resolucion, "A") = "S" Then
                    'Se informa a los involucrados
                    regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, tipo, proceso, alarma, lista) SELECT a.alerta, b.canal, 8, a.proceso, a.id, b.lista FROM " & rutaBD & ".alarmas a INNER JOIN " & rutaBD & ".mensajes b ON a.id = b.alarma WHERE a.id = " & lotes!id & " and a.estatus = 9  GROUP BY a.alerta, b.canal, a.proceso, a.id, b.lista;")
                End If
                agregarLOG("Se ha cerrado la alarma del reporte: " & lotes!proceso, 0, lotes!proceso)
            Next
        End If

        'Alertas OEE
        cadSQL = "SELECT a.*, b.alarmado_bajo, b.alarmado_alto, b.rate_efecto, b.oee_efecto, c.informar_resolucion, c.evento AS tipoalerta FROM " & rutaBD & ".alarmas a INNER JOIN " & rutaBD & ".cat_alertas c ON a.alerta = c.id INNER JOIN " & rutaBD & ".relacion_maquinas_lecturas b ON a.proceso = b.equipo AND ((b.alarmado_bajo = 'S' AND b.rate_efecto <> 'B') OR (b.alarmado_alto = 'S' AND b.rate_efecto <> 'A') OR (b.alarmado_manual = 'N')) WHERE a.estatus <> 9 AND (c.evento = 201 OR c.evento = 202 OR c.evento = 203)"

        falla = consultaSEL(cadSQL)
        If falla.Tables(0).Rows.Count > 0 Then
            For Each lotes In falla.Tables(0).Rows
                Dim cadAdic = ""
                Dim actualizar = False
                If lotes!tipoalerta = 201 And lotes!alarmado_bajo = "S" Then
                    cadAdic = "UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET alarmado_bajo = 'N' WHERE equipo = " & lotes!proceso
                    actualizar = True
                ElseIf lotes!tipoalerta = 202 And lotes!alarmado_alto = "S" Then
                    cadAdic = "UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET alarmado_alto = 'N' WHERE AND equipo = " & lotes!proceso
                    actualizar = True
                ElseIf lotes!tipoalerta = 203 And lotes!alarmado_manual = "N" Then
                    actualizar = True
                End If
                If actualizar Then
                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".alarmas SET estatus = 9, fin = NOW(), tiempo = TIME_TO_SEC(TIMEDIFF(NOW(), inicio))" & IIf(ValNull(lotes!informar_resolucion, "A") = "S", ", informado = 'S'", "") & " WHERE id = " & lotes!id & ";" & cadAdic & ";UPDATE " & rutaBD & ".mensajes SET estatus = 'Z' WHERE alarma = " & lotes!id)
                    If ValNull(lotes!informar_resolucion, "A") = "S" Then
                        'Se informa a los involucrados
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, tipo, proceso, alarma, lista) SELECT a.alerta, b.canal, 8, a.proceso, a.id, b.lista FROM " & rutaBD & ".alarmas a INNER JOIN " & rutaBD & ".mensajes b ON a.id = b.alarma WHERE a.id = " & lotes!id & " and a.estatus = 9  GROUP BY a.alerta, b.canal, a.proceso, a.id, b.lista;")
                    End If
                    agregarLOG("Se ha cerrado la alarma del equipo: " & lotes!proceso, 0, lotes!proceso)
                End If

            Next
        End If

        'Alertas WIP/Lotes
        cadSQL = "SELECT a.*, c.informar_resolucion, c.evento AS tipoalerta FROM " & rutaBD & ".alarmas a INNER JOIN " & rutaBD & ".cat_alertas c ON a.alerta = c.id INNER JOIN " & rutaBD & ".lotes b ON a.proceso = b.id AND (b.alarma_tse_paso = 'S' OR b.alarma_tpe_paso = 'S') WHERE ISNULL(a.fin) AND (c.evento = 302 OR c.evento = 303 OR c.evento = 305 OR c.evento = 306)"

        falla = consultaSEL(cadSQL)
        If falla.Tables(0).Rows.Count > 0 Then
            For Each lotes In falla.Tables(0).Rows
                Dim cadAdic = ""
                cadAdic = "UPDATE " & rutaBD & ".lotes SET " & IIf(lotes!tipoalerta = 302 Or lotes!tipoalerta = 305, "alarma_tse_paso = 'N', alarma_tse = 'N', alarma_tse_p = 'N' ", "alarma_tpe_paso = 'N', alarma_tpe = 'N', alarma_tpe_p = 'N'") & " WHERE id = " & lotes!proceso
                regsAfectados = consultaACT("UPDATE " & rutaBD & ".alarmas SET estatus = 9, fin = NOW(), tiempo = TIME_TO_SEC(TIMEDIFF(NOW(), inicio))" & IIf(ValNull(lotes!informar_resolucion, "A") = "S", ", informado = 'S'", "") & " WHERE id = " & lotes!id & ";" & cadAdic & ";UPDATE " & rutaBD & ".mensajes SET estatus = 'Z' WHERE alarma = " & lotes!id)
                If ValNull(lotes!informar_resolucion, "A") = "S" Then
                    'Se informa a los involucrados
                    regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, tipo, proceso, alarma, lista) SELECT a.alerta, b.canal, 8, a.proceso, a.id, b.lista FROM " & rutaBD & ".alarmas a INNER JOIN " & rutaBD & ".mensajes b ON a.id = b.alarma WHERE a.id = " & lotes!id & " and a.estatus = 9  GROUP BY a.alerta, b.canal, a.proceso, a.id, b.lista;")
                End If
                agregarLOG("Se ha cerrado la alarma del equipo: " & lotes!proceso, 0, lotes!proceso)

            Next
        End If

        'Alertas WIP/Cargas
        cadSQL = "SELECT a.*, c.informar_resolucion, c.transcurrido, c.evento, b.estatus AS estatuscarga, b.carga, b.fecha AS fechacarga, b.completada, b.estatus FROM " & rutaBD & ".alarmas a INNER JOIN " & rutaBD & ".cat_alertas c ON a.alerta = c.id LEFT JOIN " & rutaBD & ".cargas b ON a.proceso = b.id WHERE ISNULL(a.fin) AND (c.evento = 304 OR c.evento = 307)"

        falla = consultaSEL(cadSQL)
        If falla.Tables(0).Rows.Count > 0 Then
            For Each lotes In falla.Tables(0).Rows
                Dim cadAdic = ""
                Dim mensajeTexto = ""
                If Not lotes!completada.Equals(System.DBNull.Value) Then
                    If ValNull(lotes!completada, "A") = "S" Then
                    ElseIf DateAndTime.DateAdd(DateInterval.Second, lotes!transcurrido, lotes!fechacarga) > Now() And lotes!evento = 304 Then
                        mensajeTexto = "POSTERGADA"
                    ElseIf ValNull(lotes!carga, "A") = "" Then
                        mensajeTexto = "ELIMINADA"
                    ElseIf ValNull(lotes!estatuscarga, "A") = "I" Then
                        mensajeTexto = "CANCELADA"
                    End If
                    cadAdic = "UPDATE " & rutaBD & ".cargas SET alarma_rep_p = 'N', alarma_rep_paso = 'N', alarma_rep = 'N' WHERE id = " & lotes!proceso
                ElseIf ValNull(lotes!carga, "A") = "" Then
                    mensajeTexto = "ELIMINADA"
                End If
                If mensajeTexto.Length > 0 Then
                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".alarmas SET estatus = 9, fin = NOW(), tiempo = TIME_TO_SEC(TIMEDIFF(NOW(), inicio))" & IIf(ValNull(lotes!informar_resolucion, "A") = "S", ", informado = 'S'", "") & " WHERE id = " & lotes!id & ";" & cadAdic & ";UPDATE " & rutaBD & ".mensajes SET estatus = 'Z' WHERE alarma = " & lotes!id)

                    If ValNull(lotes!informar_resolucion, "A") = "S" Then

                        'Se informa a los involucrados
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, tipo, proceso, alarma, lista, texto) SELECT a.alerta, b.canal, 8, a.proceso, a.id, b.lista, '" & mensajeTexto & "' FROM " & rutaBD & ".alarmas a INNER JOIN " & rutaBD & ".mensajes b ON a.id = b.alarma WHERE a.id = " & lotes!id & " and a.estatus = 9  GROUP BY a.alerta, b.canal, a.proceso, a.id, b.lista;")
                    End If
                    agregarLOG("Se ha cerrado la alarma del equipo: " & lotes!proceso, 0, lotes!proceso)
                End If
            Next
        End If

        crearMensajes()
    End Sub

    Function tiempoValido(desde, hasta, maquina)
        incluyeHoyos = False
        tiempoValido = DateAndTime.DateDiff(DateInterval.Second, desde, hasta)
        If tiempoValido = 0 Then Exit Function
        Dim diaSemana = 0
        Dim fechaEspecifica As Boolean = False
        Dim maquinaEspecifico As Boolean = False
        Dim completado = False
        Dim fechaEstimada = desde
        Dim horaDesde = Format(fechaEstimada, "HH:mm:ss")
        Dim primerDia = True
        Dim MaximoDias = 14
        Dim diasContados = 0
        tiempoValido = 0
        Do While Not completado
            diasContados = diasContados + 1
            Dim rangosPositivosD(0) As String
            Dim rangosPositivosH(0) As String
            Dim rangosNegativosD(0) As String
            Dim rangosNegativosH(0) As String
            Dim rangoPositivo = 0
            Dim rangoNegativo = 0
            horaDesde = Format(fechaEstimada, "HH:mm:ss")
            'Recorrido por día
            diaSemana = DateAndTime.Weekday(fechaEstimada) - 1
            Dim cadSQL = "SELECT desde, hasta, dia, maquina, tipo FROM " & rutaBD & ".horarios WHERE clase = 0 AND (dia = " & diaSemana & " OR (dia = 9 AND fecha = '" & Format(fechaEstimada, "yyyy/MM/dd") & "')) AND (maquina = 0 OR maquina = " & maquina & ") AND hasta > '" & horaDesde & "' ORDER BY tipo DESC, maquina DESC, dia DESC, desde, hasta"
            Dim horarios As DataSet = consultaSEL(cadSQL)
            maquinaEspecifico = False
            fechaEspecifica = False
            Dim segundos = 0
            Dim primerRegistro = True
            Dim holgura = 0
            Dim combinacion = 0
            Dim sumando As Boolean = True
            Dim continuar = False
            If horarios.Tables(0).Rows.Count > 0 Then
                For Each rango In horarios.Tables(0).Rows

                    If primerRegistro Then
                        primerRegistro = False
                        If rango!tipo = "S" Then
                            rangoPositivo = rangoPositivo + 1
                            If fechaEstimada.date = desde.date Then
                                If rango!desde.ToString > horaDesde Then
                                    rangosPositivosD(rangoPositivo - 1) = rango!desde.ToString
                                Else
                                    rangosPositivosD(rangoPositivo - 1) = horaDesde
                                End If
                            Else
                                rangosPositivosD(rangoPositivo - 1) = rango!desde.ToString
                            End If
                            rangosPositivosH(rangoPositivo - 1) = rango!hasta.ToString
                            maquinaEspecifico = rango!maquina <> 0
                            fechaEspecifica = rango!dia = 9
                            If maquinaEspecifico And fechaEspecifica Then
                                combinacion = 1
                            ElseIf maquinaEspecifico Then
                                combinacion = 2
                            ElseIf fechaEspecifica Then
                                combinacion = 3
                            Else
                                combinacion = 4
                            End If
                        End If
                    Else
                        If sumando And rango!tipo = "N" Then
                            sumando = False
                            rangoNegativo = rangoNegativo + 1
                            If fechaEstimada.date = desde.date Then
                                If rango!desde.ToString > horaDesde Then
                                    rangosNegativosD(rangoNegativo - 1) = rango!desde.ToString
                                Else
                                    rangosNegativosD(rangoNegativo - 1) = rango!desde.ToString
                                    rangosNegativosH(rangoNegativo - 1) = horaDesde
                                End If
                            Else
                                rangosNegativosD(rangoNegativo - 1) = rango!desde.ToString
                            End If
                            rangosNegativosH(rangoNegativo - 1) = rango!hasta.ToString
                            maquinaEspecifico = rango!maquina <> 0
                            fechaEspecifica = rango!dia = 9
                            If maquinaEspecifico And fechaEspecifica Then
                                combinacion = 1
                            ElseIf maquinaEspecifico Then
                                combinacion = 2
                            ElseIf fechaEspecifica Then
                                combinacion = 3
                            Else
                                combinacion = 4
                            End If
                        Else
                            If combinacion = 1 Then
                                continuar = rango!maquina <> 0 And rango!dia <> 9
                            ElseIf combinacion = 2 Then
                                continuar = rango!maquina <> 0
                            ElseIf combinacion = 3 Then
                                continuar = rango!dia = 9
                            Else
                                continuar = True
                            End If
                        End If
                        If continuar Then
                            If sumando Then
                                rangoPositivo = rangoPositivo + 1
                                ReDim Preserve rangosPositivosD(rangoPositivo)
                                ReDim Preserve rangosPositivosH(rangoPositivo)
                                rangosPositivosD(rangoPositivo - 1) = rango!desde.ToString
                                rangosPositivosH(rangoPositivo - 1) = rango!hasta.ToString
                            Else
                                rangoNegativo = rangoNegativo + 1
                                ReDim Preserve rangosNegativosD(rangoNegativo)
                                ReDim Preserve rangosNegativosH(rangoNegativo)
                                rangosNegativosD(rangoNegativo - 1) = rango!desde.ToString
                                rangosNegativosH(rangoNegativo - 1) = rango!hasta.ToString
                            End If
                        End If
                    End If
                Next
            End If
            'Se crear un registro único por día
            If rangoPositivo > 0 Then
                Dim arreDefD(0) As String
                Dim arreDefH(0) As String
                Dim arreDefP(0) As String
                Dim totalItems = 1
                arreDefD(0) = rangosPositivosD(0)
                arreDefH(0) = rangosPositivosH(0)
                arreDefP(0) = "S"
                For i = 0 To rangoPositivo - 1
                    If rangosPositivosD(i) > arreDefH(totalItems - 1) Then
                        totalItems = totalItems + 1
                        ReDim Preserve arreDefD(totalItems)
                        ReDim Preserve arreDefH(totalItems)
                        ReDim Preserve arreDefP(totalItems)
                        arreDefD(totalItems - 1) = rangosPositivosD(i)
                        arreDefH(totalItems - 1) = rangosPositivosH(i)
                        arreDefP(totalItems - 1) = "S"
                    ElseIf rangosPositivosH(i) > arreDefH(totalItems - 1) Then
                        arreDefH(totalItems - 1) = rangosPositivosH(i)
                    End If
                Next
                If rangoNegativo > 0 Then
                    For i = 0 To rangoNegativo - 1
                        For j = 0 To totalItems - 1
                            If rangosNegativosD(i) <= arreDefD(j) And rangosNegativosH(i) >= arreDefH(j) Then
                                arreDefP(j) = "N"
                            End If
                            If rangosNegativosD(i) <= arreDefD(j) And rangosNegativosH(i) >= arreDefD(j) Then
                                arreDefD(j) = rangosNegativosH(i)
                            End If
                            If rangosNegativosD(i) >= arreDefD(j) And rangosNegativosD(i) < arreDefH(j) And rangosNegativosH(i) >= arreDefH(j) Then
                                arreDefH(j) = rangosNegativosD(i)
                            End If
                            If rangosNegativosD(i) > arreDefD(j) And rangosNegativosH(i) < arreDefH(j) Then
                                totalItems = totalItems + 1
                                ReDim Preserve arreDefD(totalItems)
                                ReDim Preserve arreDefH(totalItems)
                                ReDim Preserve arreDefP(totalItems)
                                arreDefD(totalItems - 1) = rangosNegativosH(i)
                                arreDefH(totalItems - 1) = arreDefH(j)
                                arreDefP(totalItems - 1) = "S"
                                arreDefH(j) = rangosNegativosD(i)
                            End If
                        Next
                    Next
                End If
                If totalItems > 0 Then
                    Dim swap1 = 0
                    Dim swap2 = 0
                    Dim swap3 = 0
                    For i = 0 To totalItems - 1
                        For j = 0 To totalItems - 2
                            If arreDefD(j) > arreDefD(j + 1) Then
                                swap1 = arreDefD(j)
                                swap2 = arreDefH(j)
                                swap3 = arreDefP(j)
                                arreDefD(j) = arreDefD(j + 1)
                                arreDefH(j) = arreDefH(j + 1)
                                arreDefP(j) = arreDefP(i + 1)
                                arreDefD(j + 1) = swap1
                                arreDefH(j + 1) = swap2
                                arreDefP(j + 1) = swap3
                            End If
                        Next
                    Next

                    Dim tiempoDisponible = 0
                    For i = 0 To totalItems - 1
                        If Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " " & arreDefH(i)) < hasta Then
                            If arreDefP(i) = "S" Then
                                tiempoValido = tiempoValido + DateDiff(DateInterval.Second, Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " " & arreDefD(i)), Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " " & arreDefH(i)))
                            Else
                                incluyeHoyos = True
                            End If
                        ElseIf Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " " & arreDefH(i)) >= hasta Then
                            If arreDefP(i) = "S" Then
                                If Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " " & arreDefD(i)) < hasta Then
                                    tiempoValido = tiempoValido + DateDiff(DateInterval.Second, Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " " & arreDefD(i)), Convert.ToDateTime(Format(hasta, "yyyy/MM/dd HH:mm:ss")))
                                End If

                            Else
                                incluyeHoyos = True
                            End If
                            completado = True
                        End If
                    Next
                    If Not completado Then
                        fechaEstimada = DateAdd(DateInterval.Day, 1, fechaEstimada)
                        fechaEstimada = Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " 00:00:00")
                    End If
                End If
            Else
                If Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " 23:59:59") >= hasta Then
                    completado = True
                Else
                    fechaEstimada = DateAdd(DateInterval.Day, 1, fechaEstimada)
                    fechaEstimada = Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " 00:00:00")
                End If
            End If
            If diasContados > MaximoDias Then completado = True
        Loop
        'If tiempoValido = 0 Then
        ' tiempoValido = DateAndTime.DateDiff(DateInterval.Second, desde, hasta)
        ' End If
    End Function

    Function tiempoValido2(desde, hasta, maquina)
        tiempoValido2 = DateAndTime.DateDiff(DateInterval.Second, desde, hasta)
        incluyeHoyos = False
        If tiempoValido2 = 0 Then Exit Function
        Dim diaSemana = 0
        Dim fechaEspecifica As Boolean = False
        Dim maquinaEspecifico As Boolean = False
        Dim completado = False
        Dim fechaEstimada = desde
        Dim horaDesde = Format(fechaEstimada, "HH:mm:ss")
        Dim primerDia = True
        Dim MaximoDias = 14
        Dim diasContados = 0
        Do While Not completado
            diasContados = diasContados + 1
            Dim rangosPositivosD(0) As String
            Dim rangosPositivosH(0) As String
            Dim rangosNegativosD(0) As String
            Dim rangosNegativosH(0) As String
            Dim rangoPositivo = 0
            Dim rangoNegativo = 0
            horaDesde = Format(fechaEstimada, "HH:mm:ss")
            'Recorrido por día
            diaSemana = DateAndTime.Weekday(fechaEstimada) - 1
            Dim cadSQL = "SELECT desde, hasta, dia, maquina, tipo FROM " & rutaBD & ".horarios WHERE (dia = " & diaSemana & " OR (dia = 9 AND fecha = '" & Format(fechaEstimada, "yyyy/MM/dd") & "')) AND (maquina = 0 OR maquina = " & maquina & ") AND hasta > '" & horaDesde & "' ORDER BY tipo DESC, maquina DESC, dia DESC, desde, hasta"
            Dim horarios As DataSet = consultaSEL(cadSQL)
            maquinaEspecifico = False
            fechaEspecifica = False
            Dim segundos = 0
            Dim primerRegistro = True
            Dim holgura = 0
            Dim combinacion = 0
            Dim sumando As Boolean = True
            Dim continuar = False
            If horarios.Tables(0).Rows.Count > 0 Then
                For Each rango In horarios.Tables(0).Rows

                    If primerRegistro Then
                        primerRegistro = False
                        If rango!tipo = "S" Then
                            rangoPositivo = rangoPositivo + 1
                            If fechaEstimada.date = desde.date Then
                                If rango!desde.ToString > horaDesde Then
                                    rangosPositivosD(rangoPositivo - 1) = rango!desde.ToString
                                Else
                                    rangosPositivosD(rangoPositivo - 1) = horaDesde
                                End If
                            Else
                                rangosPositivosD(rangoPositivo - 1) = rango!desde.ToString
                            End If
                            rangosPositivosH(rangoPositivo - 1) = rango!hasta.ToString
                            maquinaEspecifico = rango!maquina <> 0
                            fechaEspecifica = rango!dia = 9
                            If maquinaEspecifico And fechaEspecifica Then
                                combinacion = 1
                            ElseIf maquinaEspecifico Then
                                combinacion = 2
                            ElseIf fechaEspecifica Then
                                combinacion = 3
                            Else
                                combinacion = 4
                            End If
                        End If
                    Else
                        If sumando And rango!tipo = "N" Then
                            sumando = False
                            rangoNegativo = rangoNegativo + 1
                            If fechaEstimada.date = desde.date Then
                                If rango!desde.ToString > horaDesde Then
                                    rangosNegativosD(rangoNegativo - 1) = rango!desde.ToString
                                Else
                                    rangosNegativosH(rangoNegativo - 1) = horaDesde
                                End If
                            Else
                                rangosNegativosD(rangoNegativo - 1) = rango!desde.ToString
                            End If
                            rangosNegativosH(rangoNegativo - 1) = rango!hasta.ToString
                            maquinaEspecifico = rango!maquina <> 0
                            fechaEspecifica = rango!dia = 9
                            If maquinaEspecifico And fechaEspecifica Then
                                combinacion = 1
                            ElseIf maquinaEspecifico Then
                                combinacion = 2
                            ElseIf fechaEspecifica Then
                                combinacion = 3
                            Else
                                combinacion = 4
                            End If
                        Else
                            If combinacion = 1 Then
                                continuar = rango!maquina <> 0 And rango!dia <> 9
                            ElseIf combinacion = 2 Then
                                continuar = rango!maquina <> 0
                            ElseIf combinacion = 3 Then
                                continuar = rango!dia = 9
                            Else
                                continuar = True
                            End If
                        End If
                        If continuar Then
                            If sumando Then
                                rangoPositivo = rangoPositivo + 1
                                ReDim Preserve rangosPositivosD(rangoPositivo)
                                ReDim Preserve rangosPositivosH(rangoPositivo)
                                rangosPositivosD(rangoPositivo - 1) = rango!desde.ToString
                                rangosPositivosH(rangoPositivo - 1) = rango!hasta.ToString
                            Else
                                rangoNegativo = rangoNegativo + 1
                                ReDim Preserve rangosNegativosD(rangoNegativo)
                                ReDim Preserve rangosNegativosH(rangoNegativo)
                                rangosNegativosD(rangoNegativo - 1) = rango!desde.ToString
                                rangosNegativosH(rangoNegativo - 1) = rango!hasta.ToString
                            End If
                        End If
                    End If
                Next
            End If
            'Se crear un registro único por día
            If rangoPositivo > 0 Then
                Dim arreDefD(0) As String
                Dim arreDefH(0) As String
                Dim arreDefP(0) As String
                Dim totalItems = 1
                arreDefD(0) = rangosPositivosD(0)
                arreDefH(0) = rangosPositivosH(0)
                arreDefP(0) = "S"
                For i = 0 To rangoPositivo - 1
                    If rangosPositivosD(i) > arreDefD(totalItems - 1) Then
                        totalItems = totalItems + 1
                        ReDim Preserve arreDefD(totalItems)
                        ReDim Preserve arreDefH(totalItems)
                        ReDim Preserve arreDefP(totalItems)
                        arreDefD(totalItems - 1) = rangosPositivosD(i)
                        arreDefH(totalItems - 1) = rangosPositivosH(i)
                        arreDefP(totalItems - 1) = "S"
                    ElseIf rangosPositivosH(i) > arreDefH(totalItems - 1) Then
                        arreDefH(totalItems - 1) = rangosPositivosH(i)
                    End If
                Next
                If rangoNegativo > 0 Then
                    For i = 0 To rangoNegativo - 1
                        For j = 0 To totalItems - 1
                            If rangosNegativosD(i) <= arreDefD(j) And rangosNegativosH(i) >= arreDefH(j) Then
                                arreDefP(j) = "N"
                            End If
                            If rangosNegativosD(i) <= arreDefD(j) And rangosNegativosH(i) >= arreDefD(j) Then
                                arreDefD(j) = rangosNegativosH(i)
                            End If
                            If rangosNegativosD(i) >= arreDefD(j) And rangosNegativosD(i) < arreDefH(j) And rangosNegativosH(i) >= arreDefH(j) Then
                                arreDefH(j) = rangosNegativosD(i)
                            End If
                            If rangosNegativosD(i) > arreDefD(j) And rangosNegativosH(i) < arreDefH(j) Then
                                totalItems = totalItems + 1
                                ReDim Preserve arreDefD(totalItems)
                                ReDim Preserve arreDefH(totalItems)
                                ReDim Preserve arreDefP(totalItems)
                                arreDefD(totalItems - 1) = rangosNegativosH(i)
                                arreDefH(totalItems - 1) = arreDefH(j)
                                arreDefP(totalItems - 1) = "S"
                                arreDefH(j) = rangosNegativosD(i)
                            End If
                        Next
                    Next
                End If
                If totalItems > 0 Then
                    Dim swap1 = 0
                    Dim swap2 = 0
                    Dim swap3 = 0
                    For i = 0 To totalItems - 1
                        For j = 0 To totalItems - 2
                            If arreDefD(j) > arreDefD(j + 1) Then
                                swap1 = arreDefD(j)
                                swap2 = arreDefH(j)
                                swap3 = arreDefP(j)
                                arreDefD(j) = arreDefD(j + 1)
                                arreDefH(j) = arreDefH(j + 1)
                                arreDefP(j) = arreDefP(i + 1)
                                arreDefD(j + 1) = swap1
                                arreDefH(j + 1) = swap2
                                arreDefP(j + 1) = swap3
                            End If
                        Next
                    Next


                    tiempoValido2 = 0
                    Dim tiempoDisponible = 0
                    For i = 0 To totalItems - 1
                        If Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " " & arreDefH(i)) <= hasta Then
                            If arreDefP(i) = "S" Then
                                tiempoValido2 = tiempoValido2 + DateDiff(DateInterval.Second, Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " " & arreDefD(i)), Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " " & arreDefH(i)))
                            End If
                        ElseIf Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " " & arreDefH(i)) > hasta Then
                            If arreDefP(i) = "S" Then
                                tiempoValido2 = tiempoValido2 + DateDiff(DateInterval.Second, Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " " & arreDefD(i)), Convert.ToDateTime(Format(hasta, "yyyy/MM/dd HH:mm:ss")))
                            End If
                            completado = True
                        Else
                            incluyeHoyos = True
                        End If
                    Next
                    If Not completado Then
                        fechaEstimada = DateAdd(DateInterval.Day, 1, fechaEstimada)
                        fechaEstimada = Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " 00:00:00")
                    End If
                End If
            Else
                completado = True
                fechaEstimada = DateAdd(DateInterval.Day, 1, fechaEstimada)
                fechaEstimada = Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " 00:00:00")
            End If
            If diasContados > MaximoDias Then completado = True
        Loop
    End Function


    Function calcularFechaEstimada(fecha, tiempoNecesario, proceso)
        calcularFechaEstimada = fecha
        If tiempoNecesario = 0 Then Exit Function
        Dim diaSemana = 0
        Dim fechaEspecifica As Boolean = False
        Dim procesoEspecifico As Boolean = False
        Dim completado = False
        Dim fechaEstimada = fecha
        Dim horaDesde = Format(fechaEstimada, "HH:mm:ss")
        Dim primerDia = True
        Dim MaximoDias = 14
        Dim diasContados = 0
        Do While Not completado
            diasContados = diasContados + 1
            Dim rangosPositivosD(0) As String
            Dim rangosPositivosH(0) As String
            Dim rangosNegativosD(0) As String
            Dim rangosNegativosH(0) As String
            Dim rangoPositivo = 0
            Dim rangoNegativo = 0
            horaDesde = Format(fechaEstimada, "HH:mm:ss")
            'Recorrido por día
            diaSemana = DateAndTime.Weekday(fechaEstimada) - 1
            Dim cadSQL = "SELECT desde, hasta, dia, proceso, tipo FROM " & rutaBD & ".horarios WHERE (dia = " & diaSemana & " OR (dia = 9 AND fecha = '" & Format(fechaEstimada, "yyyy/MM/dd") & "')) AND (proceso = 0 OR proceso = " & proceso & ") AND hasta > '" & horaDesde & "' ORDER BY tipo DESC, proceso DESC, dia DESC, desde, hasta"
            Dim horarios As DataSet = consultaSEL(cadSQL)
            procesoEspecifico = False
            fechaEspecifica = False
            Dim segundos = 0
            Dim primerRegistro = True
            Dim holgura = 0
            Dim combinacion = 0
            Dim sumando As Boolean = True
            Dim continuar = False
            If horarios.Tables(0).Rows.Count > 0 Then
                For Each rango In horarios.Tables(0).Rows

                    If primerRegistro Then
                        primerRegistro = False
                        If rango!tipo = "S" Then
                            rangoPositivo = rangoPositivo + 1
                            If fechaEstimada.date = fecha.date Then
                                If rango!desde.ToString > horaDesde Then
                                    rangosPositivosD(rangoPositivo - 1) = rango!desde.ToString
                                Else
                                    rangosPositivosD(rangoPositivo - 1) = horaDesde
                                End If
                            Else
                                rangosPositivosD(rangoPositivo - 1) = rango!desde.ToString
                            End If
                            rangosPositivosH(rangoPositivo - 1) = rango!hasta.ToString
                            procesoEspecifico = rango!proceso <> 0
                            fechaEspecifica = rango!dia = 9
                            If procesoEspecifico And fechaEspecifica Then
                                combinacion = 1
                            ElseIf procesoEspecifico Then
                                combinacion = 2
                            ElseIf fechaEspecifica Then
                                combinacion = 3
                            Else
                                combinacion = 4
                            End If
                        End If
                    Else
                        If sumando And rango!tipo = "N" Then
                            sumando = False
                            rangoNegativo = rangoNegativo + 1
                            If fechaEstimada.date = fecha.date Then
                                If rango!desde.ToString > horaDesde Then
                                    rangosNegativosD(rangoNegativo - 1) = rango!desde.ToString
                                Else
                                    rangosNegativosH(rangoNegativo - 1) = horaDesde
                                End If
                            Else
                                rangosNegativosD(rangoNegativo - 1) = rango!desde.ToString
                            End If
                            rangosNegativosH(rangoNegativo - 1) = rango!hasta.ToString
                            procesoEspecifico = rango!proceso <> 0
                            fechaEspecifica = rango!dia = 9
                            If procesoEspecifico And fechaEspecifica Then
                                combinacion = 1
                            ElseIf procesoEspecifico Then
                                combinacion = 2
                            ElseIf fechaEspecifica Then
                                combinacion = 3
                            Else
                                combinacion = 4
                            End If
                        Else
                            If combinacion = 1 Then
                                continuar = rango!proceso <> 0 And rango!dia <> 9
                            ElseIf combinacion = 2 Then
                                continuar = rango!proceso <> 0
                            ElseIf combinacion = 3 Then
                                continuar = rango!dia = 9
                            Else
                                continuar = True
                            End If
                        End If
                        If continuar Then
                            If sumando Then
                                rangoPositivo = rangoPositivo + 1
                                ReDim Preserve rangosPositivosD(rangoPositivo)
                                ReDim Preserve rangosPositivosH(rangoPositivo)
                                rangosPositivosD(rangoPositivo - 1) = rango!desde.ToString
                                rangosPositivosH(rangoPositivo - 1) = rango!hasta.ToString
                            Else
                                rangoNegativo = rangoNegativo + 1
                                ReDim Preserve rangosNegativosD(rangoNegativo)
                                ReDim Preserve rangosNegativosH(rangoNegativo)
                                rangosNegativosD(rangoNegativo - 1) = rango!desde.ToString
                                rangosNegativosH(rangoNegativo - 1) = rango!hasta.ToString
                            End If
                        End If
                    End If
                Next
            End If
            'Se crear un registro único por día
            If rangoPositivo > 0 Then
                Dim arreDefD(0) As String
                Dim arreDefH(0) As String
                Dim arreDefP(0) As String
                Dim totalItems = 1
                arreDefD(0) = rangosPositivosD(0)
                arreDefH(0) = rangosPositivosH(0)
                arreDefP(0) = "S"
                For i = 0 To rangoPositivo - 1
                    If rangosPositivosD(i) > arreDefD(totalItems - 1) Then
                        totalItems = totalItems + 1
                        ReDim Preserve arreDefD(totalItems)
                        ReDim Preserve arreDefH(totalItems)
                        ReDim Preserve arreDefP(totalItems)
                        arreDefD(totalItems - 1) = rangosPositivosD(i)
                        arreDefH(totalItems - 1) = rangosPositivosH(i)
                        arreDefP(totalItems - 1) = "S"
                    ElseIf rangosPositivosH(i) > arreDefH(totalItems - 1) Then
                        arreDefH(totalItems - 1) = rangosPositivosH(i)
                    End If
                Next
                If rangoNegativo > 0 Then
                    For i = 0 To rangoNegativo - 1
                        For j = 0 To totalItems - 1
                            If rangosNegativosD(i) <= arreDefD(j) And rangosNegativosH(i) >= arreDefH(j) Then
                                arreDefP(j) = "N"
                            End If
                            If rangosNegativosD(i) <= arreDefD(j) And rangosNegativosH(i) >= arreDefD(j) Then
                                arreDefD(j) = rangosNegativosH(i)
                            End If
                            If rangosNegativosD(i) >= arreDefD(j) And rangosNegativosD(i) < arreDefH(j) And rangosNegativosH(i) >= arreDefH(j) Then
                                arreDefH(j) = rangosNegativosD(i)
                            End If
                            If rangosNegativosD(i) > arreDefD(j) And rangosNegativosH(i) < arreDefH(j) Then
                                totalItems = totalItems + 1
                                ReDim Preserve arreDefD(totalItems)
                                ReDim Preserve arreDefH(totalItems)
                                ReDim Preserve arreDefP(totalItems)
                                arreDefD(totalItems - 1) = rangosNegativosH(i)
                                arreDefH(totalItems - 1) = arreDefH(j)
                                arreDefP(totalItems - 1) = "S"
                                arreDefH(j) = rangosNegativosD(i)
                            End If
                        Next
                    Next
                End If
                If totalItems > 0 Then
                    Dim swap1 = 0
                    Dim swap2 = 0
                    Dim swap3 = 0
                    For i = 0 To totalItems - 1
                        For j = 0 To totalItems - 2
                            If arreDefD(j) > arreDefD(j + 1) Then
                                swap1 = arreDefD(j)
                                swap2 = arreDefH(j)
                                swap3 = arreDefP(j)
                                arreDefD(j) = arreDefD(j + 1)
                                arreDefH(j) = arreDefH(j + 1)
                                arreDefP(j) = arreDefP(i + 1)
                                arreDefD(j + 1) = swap1
                                arreDefH(j + 1) = swap2
                                arreDefP(j + 1) = swap3
                            End If
                        Next
                    Next


                    Dim tiempoDisponible = 0
                    For i = 0 To totalItems - 1
                        If arreDefP(i) = "S" Then
                            horaDesde = arreDefD(i)
                            tiempoDisponible = DateDiff(DateInterval.Second, Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " " & arreDefD(i)), Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " " & arreDefH(i)))
                            If tiempoDisponible > tiempoNecesario Then
                                'Se cubrió
                                completado = True

                                fechaEstimada = DateAdd(DateInterval.Second, tiempoNecesario, fechaEstimada)
                                tiempoNecesario = 0
                            Else
                                tiempoNecesario = tiempoNecesario - tiempoDisponible
                                fechaEstimada = Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " " & arreDefH(i))
                            End If
                        End If
                    Next
                    If Not completado Then
                        fechaEstimada = DateAdd(DateInterval.Day, 1, fechaEstimada)
                        fechaEstimada = Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " 00:00:00")
                    End If
                End If
            Else
                fechaEstimada = DateAdd(DateInterval.Day, 1, fechaEstimada)
                fechaEstimada = Convert.ToDateTime(Format(fechaEstimada, "yyyy/MM/dd") & " 00:00:00")
            End If
            If diasContados > MaximoDias Then completado = True
        Loop
        If tiempoNecesario > 0 Then
            'Es la misma fecha, se debe sumar obligatoriamente
            fechaEstimada = DateAdd(DateInterval.Second, tiempoNecesario, fechaEstimada)
        End If
        calcularFechaEstimada = fechaEstimada

    End Function

    Private Sub revisarEventos()
        'Se alarman los reportes cuyo de llenado ya pasó
        'Dim regsAfectados = consultaACT("UPDATE " & rutabd & ".reportes a, " & rutabd & ".configuracion b SET a.alarmado = 'S' WHERE TIME_TO_SEC(TIMEDIFF(NOW(), a.inicio_reporte)) >= b.tiempo_reporte AND b.tiempo_reporte > 0 AND a.estatus = 100")
        If Not estadoPrograma Then Exit Sub

        Dim cadSQL = "SELECT * FROM " & rutaBD & ".int_eventos WHERE monitor = 'S' AND (ISNULL(revisado) OR TIME_TO_SEC(TIMEDIFF(NOW(), revisado)) >= revision) ORDER BY prioridad"
        Dim eventos As DataSet = consultaSEL(cadSQL)
        If eventos.Tables(0).Rows.Count > 0 Then
            For Each evento In eventos.Tables(0).Rows
                If Not estadoPrograma Then Exit Sub
                If evento!alerta >= 101 And evento!alerta <= 307 Then
                    'Alertas referenciadas a la tabla reportes
                    alertarReporte(evento!alerta)
                    Dim regsAfectados = consultaACT("UPDATE " & rutaBD & ".int_eventos SET revisado = NOW() WHERE alerta = " & evento!alerta)
                End If
            Next
            crearMensajes()
        End If

    End Sub


    Sub alertarReporte(evento As Integer)
        Dim regsAfectados = 0
        Dim veces = 0
        Dim pases = 0
        Dim cadSQL As String = ""
        If evento = 101 Then
            cadSQL = "SELECT a.id, 0 AS lote, (SELECT id FROM " & rutaBD & ".cat_alertas WHERE (linea = a.linea OR linea = 0) AND (maquina = a.maquina OR maquina = 0) AND (area = a.area OR area = 0) AND (falla = a.falla OR falla = 0) AND evento = 101 AND estatus = 'A' AND TIME_TO_SEC(TIMEDIFF(NOW(), a.fecha)) >= transcurrido ORDER BY prioridad DESC, linea, maquina, area, falla LIMIT 1) AS idalerta FROM " & rutaBD & ".reportes a WHERE a.alarmado_atender = 'N' AND a.estatus = 0 HAVING idalerta > 0"
        ElseIf evento = 102 Then
            cadSQL = "SELECT a.id, 0 AS lote, (SELECT id FROM " & rutaBD & ".cat_alertas WHERE (linea = a.linea OR linea = 0) AND (maquina = a.maquina OR maquina = 0) AND (area = a.area OR area = 0) AND (falla = a.falla OR falla = 0) AND evento = 102 AND estatus = 'A' AND TIME_TO_SEC(TIMEDIFF(NOW(), a.inicio_atencion)) >= transcurrido ORDER BY prioridad DESC, linea, maquina, area, falla LIMIT 1) AS idalerta FROM " & rutaBD & ".reportes a WHERE a.alarmado_atendido = 'N' AND a.estatus = 10 HAVING idalerta > 0"
        ElseIf evento = 103 Then
            cadSQL = "SELECT a.id, 0 AS lote, (SELECT id FROM " & rutaBD & ".cat_alertas WHERE (linea = a.linea OR linea = 0) AND (maquina = a.maquina OR maquina = 0) AND (area = a.area OR area = 0) AND (falla = a.falla OR falla = 0) AND evento = 103 AND estatus = 'A' AND TIME_TO_SEC(TIMEDIFF(NOW(), a.inicio_reporte)) >= transcurrido ORDER BY prioridad DESC, linea, maquina, area, falla LIMIT 1) AS idalerta FROM " & rutaBD & ".reportes a LEFT JOIN " & rutaBD & ".cat_lineas b ON a.linea = b.id LEFT JOIN " & rutaBD & ".cat_maquinas c ON a.maquina = c.id LEFT JOIN " & rutaBD & ".cat_areas d ON a.area = d.id LEFT JOIN " & rutaBD & ".cat_fallas e ON a.falla = e.id WHERE a.alarmado = 'N' AND a.estatus = 100 HAVING idalerta > 0"
        ElseIf evento = 201 Then
            cadSQL = "SELECT a.equipo AS id, 0 AS lote, (SELECT id FROM " & rutaBD & ".cat_alertas WHERE (linea = c.linea OR linea = 0) AND (maquina = a.equipo OR maquina = 0) AND evento = 201 AND estatus = 'A' AND IFNULL(TIME_TO_SEC(TIMEDIFF(NOW(), a.rate_tendencia_baja)), 0) >= transcurrido ORDER BY prioridad DESC, linea, maquina LIMIT 1) AS idalerta FROM " & rutaBD & ".relacion_maquinas_lecturas a LEFT JOIN " & rutaBD & ".cat_maquinas c ON a.equipo = c.id AND c.estatus = 'A' AND c.oee = 'S' LEFT JOIN " & rutaBD & ".cat_lineas b ON c.linea = b.id AND b.estatus = 'A' WHERE a.alarmado_bajo = 'N' HAVING idalerta > 0"
        ElseIf evento = 202 Then
            cadSQL = "SELECT a.equipo AS id, 0 AS lote, (SELECT id FROM " & rutaBD & ".cat_alertas WHERE (linea = c.linea OR linea = 0) AND (maquina = a.equipo OR maquina = 0) AND evento = 202 AND estatus = 'A' AND IFNULL(TIME_TO_SEC(TIMEDIFF(NOW(), a.rate_tendencia_alta)), 0) >= transcurrido ORDER BY prioridad DESC, linea, maquina LIMIT 1) AS idalerta FROM " & rutaBD & ".relacion_maquinas_lecturas a LEFT JOIN " & rutaBD & ".cat_maquinas c ON a.equipo = c.id AND c.estatus = 'A' AND c.oee = 'S' LEFT JOIN " & rutaBD & ".cat_lineas b ON c.linea = b.id AND b.estatus = 'A' WHERE a.alarmado_alto = 'N' HAVING idalerta > 0"
        ElseIf evento = 203 Then
            cadSQL = "SELECT a.equipo AS id, 0 AS lote, (SELECT id FROM " & rutaBD & ".cat_alertas WHERE (linea = c.linea OR linea = 0) AND (maquina = a.equipo OR maquina = 0) AND evento = 203 AND estatus = 'A' AND IFNULL(TIME_TO_SEC(TIMEDIFF(NOW(), a.ultima_produccion)), 0) >= transcurrido ORDER BY prioridad DESC, linea, maquina LIMIT 1) AS idalerta FROM " & rutaBD & ".relacion_maquinas_lecturas a LEFT JOIN " & rutaBD & ".cat_maquinas c ON a.equipo = c.id AND c.estatus = 'A' AND c.oee = 'S' LEFT JOIN " & rutaBD & ".cat_lineas b ON c.linea = b.id AND b.estatus = 'A' WHERE a.alarmado_manual = 'N' HAVING idalerta > 0"
        ElseIf evento = 301 Then
            cadSQL = "SELECT a.id, a.lote, a.proceso, (SELECT id FROM " & rutaBD & ".cat_alertas WHERE (proceso = a.proceso OR proceso = 0) AND evento = 301 AND estatus = 'A' ORDER BY prioridad DESC, proceso LIMIT 1) AS idalerta FROM " & rutaBD & ".lotes_historia a INNER JOIN " & rutaBD & ".lotes b ON a.lote = b.id LEFT JOIN " & rutaBD & ".det_rutas c ON a.ruta_detalle_anterior = c.id LEFT JOIN " & rutaBD & ".det_rutas d ON a.ruta_detalle = d.id LEFT JOIN " & rutaBD & ".cat_partes e ON b.parte = e.id  WHERE a.alarma_so = 'S' HAVING idalerta > 0"

        ElseIf evento = 302 Then
            cadSQL = "SELECT a.id, 0 AS lote, a.proceso, (SELECT id FROM " & rutaBD & ".cat_alertas WHERE (proceso = a.proceso OR proceso = 0) AND evento = 302 AND estatus = 'A' AND TIME_TO_SEC(TIMEDIFF(NOW(), a.hasta)) >= transcurrido ORDER BY prioridad DESC, linea, maquina, area, falla LIMIT 1) AS idalerta FROM " & rutaBD & ".lotes a WHERE estado = 20 AND estatus = 'A' AND alarma_tse <> 'S' HAVING idalerta > 0"
        ElseIf evento = 303 Then
            cadSQL = "SELECT a.id, 0 AS lote, a.proceso, (SELECT id FROM " & rutaBD & ".cat_alertas WHERE (proceso = a.proceso OR proceso = 0) AND evento = 303 AND estatus = 'A' AND TIME_TO_SEC(TIMEDIFF(NOW(), a.hasta)) >= transcurrido ORDER BY prioridad DESC, linea, maquina, area, falla LIMIT 1) AS idalerta FROM " & rutaBD & ".lotes a WHERE estado = 50 AND estatus = 'A' AND alarma_tpe <> 'S' HAVING idalerta > 0"
        ElseIf evento = 304 Then
            cadSQL = "SELECT a.id, a.carga, 0 AS lote, a.alarma, a.alarma_rep, IFNULL((SELECT SUM(cantidad) FROM " & rutaBD & ".programacion WHERE carga = a.id AND estatus = 'A'), 0) AS piezas, (SELECT COUNT(*) FROM " & rutaBD & ".lotes WHERE carga = a.id) AS avance, b.proceso, (SELECT id FROM " & rutaBD & ".cat_alertas WHERE (proceso = b.proceso OR proceso = 0) AND evento = 304 AND estatus = 'A' AND TIME_TO_SEC(TIMEDIFF(NOW(), a.fecha)) >= transcurrido ORDER BY prioridad DESC, linea, maquina, area, falla LIMIT 1) AS idalerta FROM " & rutaBD & ".cargas a LEFT JOIN " & rutaBD & ".cat_maquinas b ON a.equipo = b.id AND b.estatus = 'A' WHERE a.estatus = 'A'  AND a.completada = 'N'"
            'Anticipaciones

        ElseIf evento = 305 Then

            cadSQL = "SELECT a.id, 0 AS lote, a.proceso FROM " & rutaBD & ".lotes a WHERE a.estado = 20 AND estatus = 'A' AND a.alarma_tse_p <> 'S' AND TIME_TO_SEC(TIMEDIFF(a.hasta, NOW())) > 0 AND TIME_TO_SEC(TIMEDIFF(a.hasta, NOW())) <= (SELECT MAX(tiempo0) FROM " & rutaBD & ".cat_alertas WHERE evento = 305 AND estatus = 'A' AND (proceso = 0 OR proceso = a.proceso))"
        ElseIf evento = 306 Then

            cadSQL = "SELECT a.id, 0 AS lote, a.proceso FROM " & rutaBD & ".lotes a WHERE a.estado = 50 AND estatus = 'A' AND a.alarma_tpe_p <> 'S' AND TIME_TO_SEC(TIMEDIFF(a.hasta, NOW())) > 0 AND TIME_TO_SEC(TIMEDIFF(a.hasta, NOW())) <= (SELECT MAX(tiempo0) FROM " & rutaBD & ".cat_alertas WHERE evento = 306 AND estatus = 'A' AND (proceso = 0 OR proceso = a.proceso))"
        ElseIf evento = 307 Then
            cadSQL = "SELECT a.id, a.carga, 0 AS lote, a.alarma, a.alarma_rep_p, IFNULL((SELECT SUM(cantidad) FROM " & rutaBD & ".programacion WHERE carga = a.id AND estatus = 'A'), 0) AS piezas, (SELECT COUNT(*) FROM " & rutaBD & ".lotes WHERE carga = a.id) AS avance, b.proceso FROM " & rutaBD & ".cargas a LEFT JOIN " & rutaBD & ".cat_maquinas b ON a.equipo = b.id AND b.estatus = 'A' WHERE a.estatus = 'A' AND TIME_TO_SEC(TIMEDIFF(a.fecha, NOW())) > 0 AND TIME_TO_SEC(TIMEDIFF(a.fecha, NOW())) <= (SELECT MAX(tiempo0) FROM " & rutaBD & ".cat_alertas WHERE evento = 307 AND estatus = 'A' AND (proceso = 0 OR proceso = b.proceso)) AND a.completada = 'N'"
        End If
        If Not estadoPrograma Then Exit Sub

        Dim falla As DataSet = consultaSEL(cadSQL)
        If falla.Tables(0).Rows.Count > 0 Then
            For Each lotes In falla.Tables(0).Rows
                If evento = 307 Then
                    If lotes!piezas > lotes!avance And lotes!alarma = "S" And lotes!alarma_rep_p <> "S" Then
                    Else
                        Continue For
                    End If
                ElseIf evento = 304 Then
                    If lotes!piezas > lotes!avance And lotes!alarma = "S" And lotes!alarma_rep <> "S" Then
                    Else
                        Continue For
                    End If
                End If
                If Not estadoPrograma Then Exit Sub
                Dim procesoID = lotes!id
                Dim loteID = lotes!lote
                If evento >= 305 And evento <= 307 Then
                    cadSQL = "SELECT * FROM " & rutaBD & ".cat_alertas WHERE evento = " & evento & " AND estatus = 'A' AND (proceso = 0 OR proceso = " & procesoID & ") ORDER BY tiempo0 DESC LIMIT 1"
                Else
                    cadSQL = "SELECT * FROM " & rutaBD & ".cat_alertas WHERE id = " & ValNull(lotes!idalerta, "N")
                End If
                Dim alerta As DataSet = consultaSEL(cadSQL)
                Dim uID = 0

                If alerta.Tables(0).Rows.Count > 0 Then

                    Dim idAlerta = alerta.Tables(0).Rows(0)!id
                    Dim fechaDesde
                    Dim crearReporte As Boolean = True
                    Dim solapable As Boolean = False

                    'Se pregunta si hay un rperte activo y si es solapable
                    If ValNull(alerta.Tables(0).Rows(0)!solapar, "A") = "S" Then
                        solapable = True
                    Else
                        cadSQL = "SELECT * FROM " & rutaBD & ".alarmas WHERE alerta = " & idAlerta & " AND estatus <> 9 "
                        Dim solapar As DataSet = consultaSEL(cadSQL)
                        crearReporte = solapar.Tables(0).Rows.Count = 0
                    End If
                    If crearReporte Then
                        Dim porAcumulacion = False

                        If ValNull(alerta.Tables(0).Rows(0)!acumular, "A") = "S" Then
                            crearReporte = False
                            porAcumulacion = True
                            regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".alarmas (alerta, proceso, prioridad) VALUES (" & idAlerta & ", " & procesoID & ", " & alerta.Tables(0).Rows(0)!prioridad & ")")

                            If ValNull(alerta.Tables(0).Rows(0)!acumular_inicializar, "A") = "S" Then
                                If alerta.Tables(0).Rows(0)!acumular_tiempo > 0 Then
                                    fechaDesde = DateAdd(DateInterval.Second, alerta.Tables(0).Rows(0)!acumular_tiempo * -1, Now)
                                    cadSQL = "SELECT COUNT(*) as cuenta FROM " & rutaBD & ".alarmas WHERE alerta = " & idAlerta & " AND estatus = 0 AND inicio >= '" & Format(fechaDesde, "yyyy/MM/dd HH:mm:ss") & "'"
                                Else
                                    cadSQL = "SELECT COUNT(*) as cuenta FROM " & rutaBD & ".alarmas WHERE alerta = " & idAlerta & " AND estatus = 0"
                                End If
                            Else
                                If alerta.Tables(0).Rows(0)!acumular_tiempo > 0 Then
                                    fechaDesde = DateAdd(DateInterval.Second, alerta.Tables(0).Rows(0)!acumular_tiempo * -1, Now)
                                    cadSQL = "SELECT COUNT(*) as cuenta FROM " & rutaBD & ".alarmas WHERE alerta = " & idAlerta & " AND inicio >= '" & Format(fechaDesde, "yyyy/MM/dd HH:mm:ss") & "'"
                                Else
                                    cadSQL = "SELECT COUNT(*) as cuenta FROM " & rutaBD & ".alarmas WHERE alerta = " & idAlerta
                                End If
                            End If

                            Dim acumulado = 0
                            Dim acum As DataSet = consultaSEL(cadSQL)
                            If acum.Tables(0).Rows.Count > 0 Then
                                acumulado = acum.Tables(0).Rows(0)!cuenta
                            End If
                            If acumulado >= alerta.Tables(0).Rows(0)!acumular_veces Then
                                veces = acumulado + 1
                                If alerta.Tables(0).Rows(0)!acumular_tiempo > 0 Then
                                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".alarmas SET acumulada = 'S', activada = NOW(), estatus = 1 WHERE alerta = " & idAlerta & " AND estatus = 0 AND inicio >= '" & Format(fechaDesde, "yyyy/MM/dd HH:mm:ss") & "'")
                                Else
                                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".alarmas SET acumulada = 'S', activada = NOW(), estatus = 1 WHERE alerta = " & idAlerta & " AND estatus = 0")
                                End If
                                crearReporte = True
                            End If
                        Else
                            regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".alarmas (alerta, proceso, prioridad, estatus, activada) VALUES (" & idAlerta & ", " & procesoID & ", " & alerta.Tables(0).Rows(0)!prioridad & ", 1, NOW())")
                        End If

                        If crearReporte Then
                            If ValNull(alerta.Tables(0).Rows(0)!llamada, "A") = "S" Then
                                regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma) SELECT alerta, 0, proceso, " & alerta.Tables(0).Rows(0)!prioridad & ", " & alerta.Tables(0).Rows(0)!lista & ", id FROM " & rutaBD & ".alarmas WHERE alerta = " & idAlerta & " AND fase = 0")
                            End If
                            If ValNull(alerta.Tables(0).Rows(0)!sms, "A") = "S" Then
                                regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma) Select alerta, 1, proceso, " & alerta.Tables(0).Rows(0)!prioridad & ", " & alerta.Tables(0).Rows(0)!lista & ", id FROM " & rutaBD & ".alarmas WHERE alerta = " & idAlerta & " AND fase = 0")
                            End If
                            If ValNull(alerta.Tables(0).Rows(0)!correo, "A") = "S" Then
                                regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma) Select alerta, 2, proceso, " & alerta.Tables(0).Rows(0)!prioridad & ", " & alerta.Tables(0).Rows(0)!lista & ", id FROM " & rutaBD & ".alarmas WHERE alerta = " & idAlerta & " AND fase = 0")
                            End If
                            If ValNull(alerta.Tables(0).Rows(0)!mmcall, "A") = "S" Then
                                regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma) Select alerta, 3, proceso, " & alerta.Tables(0).Rows(0)!prioridad & ", " & alerta.Tables(0).Rows(0)!lista & ", id FROM " & rutaBD & ".alarmas WHERE alerta = " & idAlerta & " AND fase = 0")
                            End If
                            If ValNull(alerta.Tables(0).Rows(0)!log, "A") = "S" Then
                                regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma) Select alerta, 4, proceso, " & alerta.Tables(0).Rows(0)!prioridad & ", " & alerta.Tables(0).Rows(0)!lista & ", id FROM " & rutaBD & ".alarmas WHERE alerta = " & idAlerta & " AND fase = 0")
                            End If
                            If evento = 301 Then
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".alarmas SET fase = 1, estatus = 9, fin = NOW(), tiempo = TIME_TO_SEC(TIMEDIFF(NOW(), inicio)) WHERE estatus = 1 And fase = 0")
                            Else
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".alarmas SET fase = 1 WHERE alerta = " & idAlerta & " AND estatus = 1 And fase = 0")
                            End If

                            pases = pases + 1
                        End If

                        If evento = 101 Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".reportes Set alarmado_atender = 'S', alarmado_atender_desde = NOW() WHERE id = " & procesoID)
                        ElseIf evento = 102 Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".reportes SET alarmado_atendido = 'S', alarmado_atendido_desde = NOW() WHERE id = " & procesoID)
                        ElseIf evento = 103 Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".reportes SET alarmado = 'S', alarmado_informe_desde = NOW() WHERE id = " & procesoID)
                        ElseIf evento = 201 Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET alarmado_bajo = 'S' WHERE equipo = " & procesoID)
                        ElseIf evento = 202 Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET alarmado_alto = 'S' WHERE equipo = " & procesoID)
                        ElseIf evento = 203 Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET alarmado_manual = 'S' WHERE equipo = " & procesoID)
                        ElseIf evento = 301 Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".lotes_historia SET alarma_so = '0', alarma_so_rep = NOW() WHERE id = " & procesoID & ";UPDATE " & rutaBD & ".lotes SET alarmas = alarmas + 1 WHERE id = " & loteID)
                        ElseIf evento = 302 Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".lotes SET alarma_tse = 'S', alarmas = alarmas + 1 WHERE id = " & procesoID)
                        ElseIf evento = 303 Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".lotes SET alarma_tpe = 'S', alarmas = alarmas + 1 WHERE id = " & procesoID)
                        ElseIf evento = 304 Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".cargas SET alarma_rep = 'S' WHERE id = " & procesoID)
                        ElseIf evento = 305 Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".lotes SET alarma_tse_p = 'S' WHERE id = " & procesoID)
                        ElseIf evento = 306 Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".lotes SET alarma_tpe_p = 'S' WHERE id = " & procesoID)
                        ElseIf evento = 307 Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".cargas SET alarma_repexc_p = 'S' WHERE id = " & procesoID)
                        End If

                    Else
                        agregarLOG("Un evento con el reporte " & procesoID & " no generó alerta por solapamiento en la alerta " & idAlerta, 2, procesoID)
                    End If
                End If
            Next
        End If
        Dim cadAgregar = ""
        If pases > 0 And evento = 101 Then
            cadAgregar = "Se alarmó un reporte por tiempo de espera excecido"
            If pases > 1 Then
                cadAgregar = "Se alarmaron " & pases & " reportes por tiempo de espera excecido"
            End If
        ElseIf pases > 0 And evento = 102 Then
            cadAgregar = "Se alarmó un reporte por tiempo de reparación excecido"
            If pases > 1 Then
                cadAgregar = "Se generaron " & pases & " alarmas por tiempo de reparación excecido"
            End If
        ElseIf pases > 0 And evento = 103 Then
            cadAgregar = "Se alarmó un reporte por tiempo de informe excecido"
            If pases > 1 Then
                cadAgregar = "Se generaron " & pases & " alarmas por tiempo de informe excecido"
            End If
        ElseIf pases > 0 And evento = 201 Then
            cadAgregar = "Se alarmó un equipo por bajo rate"
            If pases > 1 Then
                cadAgregar = "Se generaron " & pases & " alarmas por bajo rate"
            End If
        ElseIf pases > 0 And evento = 202 Then
            cadAgregar = "Se alarmó un equipo por sobre rate"
            If pases > 1 Then
                cadAgregar = "Se generaron " & pases & " alarmas por sobre rate"
            End If
        ElseIf pases > 0 And evento = 203 Then
            cadAgregar = "Se alarmó un equipo por no detección de piezas"
            If pases > 1 Then
                cadAgregar = "Se generaron " & pases & " alarmas por no detección de piezas"
            End If
        ElseIf pases > 0 And evento = 301 Then
            cadAgregar = "Se alarmó un proceso por salto de operación"
            If pases > 1 Then
                cadAgregar = "Se generaron " & pases & " alarmas por salto de operación"
            End If
        ElseIf pases > 0 And evento = 302 Then
            cadAgregar = "Se alarmó un proceso por tiempo excedido de stock"
            If pases > 1 Then
                cadAgregar = "Se generaron " & pases & " alarmas por tiempo excedido de stock"
            End If
        ElseIf pases > 0 And evento = 303 Then
            cadAgregar = "Se alarmó un proceso por tiempo excedido de proceso"
            If pases > 1 Then
                cadAgregar = "Se generaron " & pases & " alarmas por tiempo excedido de proceso"
            End If
        ElseIf pases > 0 And evento = 304 Then
            cadAgregar = "Se alarmó un proceso por tiempo excedido de programación"
            If pases > 1 Then
                cadAgregar = "Se generaron " & pases & " alarmas por tiempo excedido de programación"
            End If
        ElseIf pases > 0 And evento = 305 Then
            cadAgregar = "Se alarmó un proceso por anticipación por tiempo excedido de stock"
            If pases > 1 Then
                cadAgregar = "Se generaron " & pases & " alarmas por tiempo excedido de stock"
            End If
        ElseIf pases > 0 And evento = 306 Then
            cadAgregar = "Se alarmó un proceso por anticipación por tiempo excedido de proceso"
            If pases > 1 Then
                cadAgregar = "Se generaron " & pases & " alarmas por tiempo excedido de proceso"
            End If
        ElseIf pases > 0 And evento = 307 Then
            cadAgregar = "Se alarmó un proceso por anticipación por tiempo excedido de programación"
            If pases > 1 Then
                cadAgregar = "Se generaron " & pases & " alarmas por tiempo excedido de programación"
            End If
        End If

        If pases > 0 Then agregarSolo(cadAgregar)
    End Sub

    Private Sub calcularRevision()
        Dim cadSQL As String = "SELECT revisar_cada FROM " & rutaBD & ".configuracion"
        Dim reader As DataSet = consultaSEL(cadSQL)
        If errorBD.Length > 0 Then
            agregarLOG("Ocurrió un error al intentar leer MySQL. Error: " + errorBD, 9, 0)
        Else
            If reader.Tables(0).Rows.Count > 0 Then
                If ValNull(reader.Tables(0).Rows(0)!revisar_cada, "N") = 0 Then
                    eSegundos = 60
                    Dim regsAfectados = consultaACT("UPDATE " & rutaBD & ".configuracion SET revisar_cada = 60")
                    revisaFlag.Interval = 1000
                    revisaFlag.Enabled = False
                    revisaFlag.Enabled = True
                Else
                    eSegundos = ValNull(reader.Tables(0).Rows(0)!revisar_cada, "N")
                    If revisaFlag.Interval <> eSegundos * 1000 Then
                        revisaFlag.Interval = eSegundos * 1000
                        revisaFlag.Enabled = False
                        revisaFlag.Enabled = True
                    End If
                End If

            End If
        End If
        BarManager1.Items(1).Caption = "Conectado (cada " & eSegundos & " segundos)"
    End Sub

    Function calcularTiempo(Seg) As String
        calcularTiempo = ""
        If Seg < 60 Then
            calcularTiempo = Seg & " seg"
        ElseIf Seg < 3600 Then
            calcularTiempo = Math.Round(Seg / 60, 1) & " min"
        Else
            calcularTiempo = Math.Round(Seg / 3600, 1) & " hr"
        End If
    End Function


    Function calcularTiempoCad(Seg) As String
        calcularTiempoCad = "-"
        Dim horas = Math.Floor(Seg / 3600)
        Dim minutos = Math.Floor((Seg Mod 3600) / 60)
        Dim segundos = (Seg Mod 3600) Mod 60
        calcularTiempoCad = horas & ":" & Format(minutos, "00") & ":" & Format(segundos, "00")
    End Function


    Sub escalamientos()

        BarManager1.Items(1).Caption = "Conectado (revisando escalamientos...)"
        procesandoEscalamientos = True
        Dim regsAfectados = 0
        Dim cadSQL = ""


        'Escalada 5
        cadSQL = "SELECT a.*, b.evento, b.tiempo5, b.prioridad, b.lista5, b.escalar5, b.llamada5, sms5, log5, mmcall5, correo5 FROM " & rutaBD & ".alarmas a INNER JOIN " & rutaBD & ".cat_alertas b ON a.alerta = b.id AND b.estatus = 'A' WHERE b.escalar1 <> 'N' AND b.escalar2 <> 'N' AND b.escalar3 <> 'N' AND b.escalar4 <> 'N' AND b.escalar5 <> 'N' AND ((a.estatus = 5) OR (a.estatus >= 5 AND a.estatus < 9 AND b.repetir5 = 'S')) AND (a.escalamientos5 <= b.veces5 OR b.veces5 = 0)"
        Dim alertaDS As DataSet = consultaSEL(cadSQL)
        If alertaDS.Tables(0).Rows.Count > 0 Then

            For Each alerta In alertaDS.Tables(0).Rows
                If Not estadoPrograma Then
                    procesandoEscalamientos = False
                    Exit Sub
                End If
                Dim idAlerta = alerta!alerta
                Dim procesoID = alerta!proceso
                Dim segundos = 0
                Dim activarEscalada As Boolean = False
                Dim uID = alerta!id
                'Se verifica que no se haya repetido antes
                If alerta!escalada5.Equals(System.DBNull.Value) Then
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!escalada4, "yyyy/MM/dd HH:mm:ss")), Now)
                Else
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!escalada5, "yyyy/MM/dd HH:mm:ss")), Now)
                End If
                Dim tiempoCad = ""
                If segundos >= alerta!tiempo5 Then
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!activada, "yyyy/MM/dd HH:mm:ss")), Now)
                    tiempoCad = calcularTiempoCad(segundos)
                    If ValNull(alerta!escalar5, "A") = "T" And alerta!estatus = 5 Then
                        'Se valida si se repite el mesaje para el nivel anterior
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, alarma, lista, tipo, canal, proceso, prioridad) SELECT alerta, alarma, lista, 15, canal, proceso, prioridad FROM " & rutaBD & ".mensajes WHERE alarma = " & alerta!id & " AND tipo <= 4")
                    End If
                    If alerta!estatus = 5 And alerta!evento < 200 Then
                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".reportes a SET a.escalado = 5, a.generar_audio = 'E' WHERE estatus = 0 AND id IN (SELECT proceso FROM " & rutaBD & ".alarmas WHERE id = " & uID & ")")
                    End If
                    If ValNull(alerta!llamada5, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 0, proceso, prioridad, " & alerta!lista5 & ", id, 5 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!sms5, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 1, proceso, prioridad, " & alerta!lista5 & ", id, 5 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!correo5, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 2, proceso, prioridad, " & alerta!lista5 & ", id, 5 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!mmcall5, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 3, proceso, prioridad, " & alerta!lista5 & ", id, 5 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!log5, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 4, proceso, prioridad, " & alerta!lista5 & ", id, 5 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    agregarLOG("Se han escalado el reporte: " & procesoID & " para el nivel 5", 0, procesoID)
                    Dim cadAdic = "fase = 15, "
                    If alerta!escalamientos5 > 0 Then
                        cadAdic = ""
                    End If
                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".alarmas SET " & cadAdic & "escalada5 = NOW(), escalamientos5 = escalamientos5 + 1, estatus = 6 WHERE id = " & uID)
                End If
            Next
        End If

        cadSQL = "SELECT a.*, b.evento, b.tiempo4, b.prioridad, b.lista4, b.escalar4, b.llamada4, sms4, log4, mmcall4, correo4 FROM " & rutaBD & ".alarmas a INNER JOIN " & rutaBD & ".cat_alertas b ON a.alerta = b.id AND b.estatus = 'A' WHERE b.escalar1 <> 'N' AND b.escalar2 <> 'N' AND b.escalar3 <> 'N' AND b.escalar4 <> 'N' AND ((a.estatus = 4) OR (a.estatus >= 4 AND a.estatus < 9 AND b.repetir4 = 'S')) AND (a.escalamientos4 <= b.veces4 OR b.veces4 = 0)"
        alertaDS = consultaSEL(cadSQL)
        If alertaDS.Tables(0).Rows.Count > 0 Then

            For Each alerta In alertaDS.Tables(0).Rows
                If Not estadoPrograma Then
                    procesandoEscalamientos = False
                    Exit Sub
                End If
                Dim idAlerta = alerta!alerta
                Dim procesoID = alerta!proceso
                Dim segundos = 0
                Dim activarEscalada As Boolean = False
                Dim uID = alerta!id
                'Se verifica que no se haya repetido antes
                If alerta!escalada4.Equals(System.DBNull.Value) Then
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!escalada3, "yyyy/MM/dd HH:mm:ss")), Now)
                Else
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!escalada4, "yyyy/MM/dd HH:mm:ss")), Now)
                End If
                Dim tiempoCad = ""
                If segundos >= alerta!tiempo4 Then
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!activada, "yyyy/MM/dd HH:mm:ss")), Now)
                    tiempoCad = calcularTiempoCad(segundos)
                    If ValNull(alerta!escalar4, "A") = "T" And alerta!estatus = 4 Then
                        'Se valida si se repite el mesaje para el nivel anterior
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, alarma, lista, tipo, canal, proceso, prioridad) SELECT alerta, alarma, lista, 14, canal, proceso, prioridad FROM " & rutaBD & ".mensajes WHERE alarma = " & alerta!id & " AND tipo <= 3")
                    End If
                    If alerta!estatus = 4 And alerta!evento < 200 Then
                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".reportes a SET a.escalado = 4, a.generar_audio = 'E' WHERE estatus = 0 AND id IN (SELECT proceso FROM " & rutaBD & ".alarmas WHERE id = " & uID & ")")
                    End If
                    If ValNull(alerta!llamada4, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 0, proceso, prioridad, " & alerta!lista4 & ", id, 4 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!sms4, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 1, proceso, prioridad, " & alerta!lista4 & ", id, 4 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!correo4, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 2, proceso, prioridad, " & alerta!lista4 & ", id, 4 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!mmcall4, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 3, proceso, prioridad, " & alerta!lista4 & ", id, 4 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!log4, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 4, proceso, prioridad, " & alerta!lista4 & ", id, 4 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    agregarLOG("Se han escalado el reporte: " & procesoID & " para el nivel 4", 0, procesoID)
                    Dim cadAdic = "fase = 14, "
                    If alerta!escalamientos4 > 0 Then
                        cadAdic = ""
                    End If
                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".alarmas SET " & cadAdic & "escalada4 = NOW(), escalamientos4 = escalamientos4 + 1, estatus = 5 WHERE id = " & alerta!id)
                End If
            Next
        End If

        cadSQL = "SELECT a.*, b.evento, b.tiempo3, b.prioridad, b.lista3, b.escalar3, b.llamada3, sms3, log3, mmcall3, correo3 FROM " & rutaBD & ".alarmas a INNER JOIN " & rutaBD & ".cat_alertas b ON a.alerta = b.id AND b.estatus = 'A' WHERE b.escalar1 <> 'N' AND b.escalar2 <> 'N' AND b.escalar3 <> 'N' AND ((a.estatus = 3) OR (a.estatus >= 3 AND a.estatus < 9 AND b.repetir3 = 'S')) AND (a.escalamientos3 <= b.veces3 OR b.veces3 = 0)"
        alertaDS = consultaSEL(cadSQL)
        If alertaDS.Tables(0).Rows.Count > 0 Then

            For Each alerta In alertaDS.Tables(0).Rows
                If Not estadoPrograma Then
                    procesandoEscalamientos = False
                    Exit Sub
                End If
                Dim idAlerta = alerta!alerta
                Dim procesoID = alerta!proceso
                Dim segundos = 0
                Dim activarEscalada As Boolean = False
                Dim uID = alerta!id
                'Se verifica que no se haya repetido antes
                If alerta!escalada3.Equals(System.DBNull.Value) Then
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!escalada2, "yyyy/MM/dd HH:mm:ss")), Now)
                Else
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!escalada3, "yyyy/MM/dd HH:mm:ss")), Now)
                End If
                Dim tiempoCad = ""
                If segundos >= alerta!tiempo3 Then
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!activada, "yyyy/MM/dd HH:mm:ss")), Now)
                    tiempoCad = calcularTiempoCad(segundos)
                    If ValNull(alerta!escalar3, "A") = "T" And alerta!estatus = 3 Then
                        'Se valida si se repite el mesaje para el nivel anterior
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, alarma, lista, tipo, canal, proceso, prioridad) SELECT alerta, alarma, lista, 13, canal, proceso, prioridad FROM " & rutaBD & ".mensajes WHERE alarma = " & alerta!id & " AND tipo <= 2")
                    End If
                    If alerta!estatus = 3 And alerta!evento < 200 Then
                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".reportes a SET a.escalado = 3, a.generar_audio = 'E' WHERE estatus = 0 AND id IN (SELECT proceso FROM " & rutaBD & ".alarmas WHERE id = " & uID & ")")
                    End If
                    If ValNull(alerta!llamada3, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 0, proceso, prioridad, " & alerta!lista3 & ", id, 3 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!sms3, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 1, proceso, prioridad, " & alerta!lista3 & ", id, 3 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!correo3, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 2, proceso, prioridad, " & alerta!lista3 & ", id, 3 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!mmcall3, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 3, proceso, prioridad, " & alerta!lista3 & ", id, 3 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!log3, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 4, proceso, prioridad, " & alerta!lista3 & ", id, 3 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    agregarLOG("Se han escalado el reporte: " & procesoID & " para el nivel 3", 0, procesoID)
                    Dim cadAdic = "fase = 13, "
                    If alerta!escalamientos3 > 0 Then
                        cadAdic = ""
                    End If
                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".alarmas SET " & cadAdic & "escalada3 = NOW(), escalamientos3 = escalamientos3 + 1, estatus = 4 WHERE id = " & alerta!id)
                End If
            Next
        End If

        cadSQL = "SELECT a.*, b.evento, b.tiempo2, b.prioridad, b.lista2, b.escalar2, b.llamada2, sms2, log2, mmcall2, correo2 FROM " & rutaBD & ".alarmas a INNER JOIN " & rutaBD & ".cat_alertas b ON a.alerta = b.id AND b.estatus = 'A' WHERE b.escalar1 <> 'N' AND b.escalar2 <> 'N' AND ((a.estatus = 2) OR (a.estatus >= 2 AND a.estatus < 9 AND b.repetir2 = 'S')) AND (a.escalamientos2 <= b.veces2 OR b.veces2 = 0)"
        alertaDS = consultaSEL(cadSQL)
        If alertaDS.Tables(0).Rows.Count > 0 Then

            For Each alerta In alertaDS.Tables(0).Rows
                If Not estadoPrograma Then
                    procesandoEscalamientos = False
                    Exit Sub
                End If
                Dim idAlerta = alerta!alerta
                Dim procesoID = alerta!proceso
                Dim segundos = 0
                Dim activarEscalada As Boolean = False
                Dim uID = alerta!id
                'Se verifica que no se haya repetido antes
                If alerta!escalada2.Equals(System.DBNull.Value) Then
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!escalada1, "yyyy/MM/dd HH:mm:ss")), Now)
                Else
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!escalada2, "yyyy/MM/dd HH:mm:ss")), Now)
                End If
                Dim tiempoCad = ""
                If segundos >= alerta!tiempo2 Then
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!activada, "yyyy/MM/dd HH:mm:ss")), Now)
                    tiempoCad = calcularTiempoCad(segundos)
                    If ValNull(alerta!escalar2, "A") = "T" And alerta!estatus = 2 Then
                        'Se valida si se repite el mesaje para el nivel anterior
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, alarma, lista, tipo, canal, proceso, prioridad) SELECT alerta, alarma, lista, 12, canal, proceso, prioridad FROM " & rutaBD & ".mensajes WHERE alarma = " & alerta!id & " AND tipo <= 1")
                    End If
                    If alerta!estatus = 2 And alerta!evento < 200 Then
                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".reportes a SET a.escalado = 2, a.generar_audio = 'E' WHERE estatus = 0 AND id IN (SELECT proceso FROM " & rutaBD & ".alarmas WHERE id = " & uID & ")")
                    End If
                    If ValNull(alerta!llamada2, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 0, proceso, prioridad, " & alerta!lista2 & ", id, 2 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!sms2, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 1, proceso, prioridad, " & alerta!lista2 & ", id, 2 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!correo2, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 2, proceso, prioridad, " & alerta!lista2 & ", id, 2 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!mmcall2, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 3, proceso, prioridad, " & alerta!lista2 & ", id, 2 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!log2, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 4, proceso, prioridad, " & alerta!lista2 & ", id, 2 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    agregarLOG("Se han escalado el reporte: " & procesoID & " para el nivel 2", 0, procesoID)
                    Dim cadAdic = "fase = 12, "
                    If alerta!escalamientos2 > 0 Then
                        cadAdic = ""
                    End If
                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".alarmas SET " & cadAdic & "escalada2 = NOW(), escalamientos2 = escalamientos2 + 1, estatus = 3 WHERE id = " & alerta!id)
                End If
            Next
        End If

        cadSQL = "SELECT a.*, b.evento, b.tiempo1, b.prioridad, b.lista1, b.escalar1, b.llamada1, sms1, log1, mmcall1, correo1 FROM " & rutaBD & ".alarmas a INNER JOIN " & rutaBD & ".cat_alertas b ON a.alerta = b.id AND b.estatus = 'A' WHERE b.escalar1 <> 'N' AND ((a.estatus = 1) OR (a.estatus >= 1 AND a.estatus < 9 AND b.repetir1 = 'S')) AND (a.escalamientos1 < b.veces1 OR b.veces1 = 0)"
        alertaDS = consultaSEL(cadSQL)
        If alertaDS.Tables(0).Rows.Count > 0 Then

            For Each alerta In alertaDS.Tables(0).Rows
                If Not estadoPrograma Then
                    procesandoEscalamientos = False
                    Exit Sub
                End If
                Dim idAlerta = alerta!alerta
                Dim procesoID = alerta!proceso
                Dim segundos = 0
                Dim activarEscalada As Boolean = False
                Dim uID = alerta!id
                'Se verifica que no se haya repetido antes
                If alerta!escalada1.Equals(System.DBNull.Value) Then
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!activada, "yyyy/MM/dd HH:mm:ss")), Now)
                Else
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!escalada1, "yyyy/MM/dd HH:mm:ss")), Now)
                End If
                Dim tiempoCad = ""
                If segundos >= alerta!tiempo1 Then
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!activada, "yyyy/MM/dd HH:mm:ss")), Now)
                    tiempoCad = calcularTiempoCad(segundos)
                    If ValNull(alerta!escalar1, "A") = "T" And alerta!estatus = 1 Then
                        'Se valida si se repite el mesaje para el nivel anterior
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, alarma, lista, tipo, canal, proceso, prioridad) SELECT alerta, alarma, lista, 11, canal, proceso, prioridad FROM " & rutaBD & ".mensajes WHERE alarma = " & alerta!id & " AND tipo = 0")
                    End If
                    If alerta!estatus = 1 And alerta!evento < 200 Then
                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".reportes a SET a.escalado = 1, a.generar_audio = 'E' WHERE estatus = 0 AND id IN (SELECT proceso FROM " & rutaBD & ".alarmas WHERE id = " & uID & ")")
                    End If
                    If ValNull(alerta!llamada1, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 0, proceso, prioridad, " & alerta!lista1 & ", id, 1 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!sms1, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 1, proceso, prioridad, " & alerta!lista1 & ", id, 1 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!correo1, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 2, proceso, prioridad, " & alerta!lista1 & ", id, 1 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!mmcall1, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 3, proceso, prioridad, " & alerta!lista1 & ", id, 1 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!log1, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 4, proceso, prioridad, " & alerta!lista1 & ", id, 1 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    agregarLOG("Se han escalado el reporte: " & procesoID & " para el nivel 1", 0, procesoID)
                    Dim cadAdic = "fase = 11, "
                    If alerta!escalamientos1 > 0 Then
                        cadAdic = ""
                    End If
                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".alarmas SET " & cadAdic & "escalada1 = NOW(), escalamientos1 = escalamientos1 + 1, estatus = 2 WHERE id = " & uID)
                End If
            Next
        End If

        cadSQL = "SELECT a.*, b.evento, b.repetir_tiempo, b.repetir_veces, b.prioridad, b.lista, b.llamada, sms, log, mmcall, correo FROM " & rutaBD & ".alarmas a INNER JOIN " & rutaBD & ".cat_alertas b ON a.alerta = b.id AND b.estatus = 'A' WHERE ((a.estatus = 1 AND b.repetir = 'T') OR (a.estatus >= 1 AND a.estatus < 9 AND b.repetir = 'S')) AND b.repetir_tiempo > 0 AND (a.repeticiones < b.repetir_veces OR b.repetir_veces = 0)"
        alertaDS = consultaSEL(cadSQL)
        If alertaDS.Tables(0).Rows.Count > 0 Then

            For Each alerta In alertaDS.Tables(0).Rows
                If Not estadoPrograma Then
                    procesandoEscalamientos = False
                    Exit Sub
                End If
                Dim idAlerta = alerta!alerta
                Dim procesoID = alerta!proceso
                Dim segundos = 0
                Dim activarEscalada As Boolean = False
                Dim uID = alerta!id
                'Se verifica que no se haya repetido antes
                Application.DoEvents()
                Dim repeticiones As Integer = alerta!repeticiones + 1
                'Se verifica que no se haya repetido antes
                If alerta!repetida.Equals(System.DBNull.Value) Then
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!activada, "yyyy/MM/dd HH:mm:ss")), Now)
                Else
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!repetida, "yyyy/MM/dd HH:mm:ss")), Now)
                End If
                Dim tiempoCad = ""
                If segundos >= alerta!repetir_tiempo Then
                    segundos = DateDiff(DateInterval.Second, CDate(Format(alerta!activada, "yyyy/MM/dd HH:mm:ss")), Now)
                    tiempoCad = calcularTiempoCad(segundos)
                    If alerta!evento < 200 Then
                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".reportes a SET a.repeticiones = a.repeticiones + 1, a.generar_audio = 'R' WHERE estatus = 0 AND id IN (SELECT proceso FROM " & rutaBD & ".alarmas WHERE id = " & uID & ")")
                    End If

                    If ValNull(alerta!llamada, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 0, proceso, prioridad, " & alerta!lista & ", " & uID & ", 9 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!sms, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 1, proceso, prioridad, " & alerta!lista & ", " & uID & ", 9 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!correo, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 2, proceso, prioridad, " & alerta!lista & ", " & uID & ", 9 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!mmcall, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 3, proceso, prioridad, " & alerta!lista & ", " & uID & ", 9 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    If ValNull(alerta!log, "A") = "S" Then
                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".mensajes (alerta, canal, proceso, prioridad, lista, alarma, tipo) SELECT alerta, 4, proceso, prioridad, " & alerta!lista & ", " & uID & ", 9 FROM " & rutaBD & ".alarmas WHERE id = " & uID)
                    End If
                    agregarLOG("Se ha enviado una repetición del reporte: " & procesoID, 0, procesoID)
                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".alarmas SET repeticiones = repeticiones + 1, repetida = NOW(), estatus = 1 WHERE id = " & alerta!id)
                End If
            Next
        End If


        procesandoEscalamientos = False
        BarManager1.Items(1).Caption = "Conectado (cada " & eSegundos & " segundos)"
        crearMensajes()
    End Sub

    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseDoubleClick
        Try
            Me.Visible = True
            NotifyIcon1.Visible = False
            Me.WindowState = FormWindowState.Maximized
        Catch ex As Exception

        End Try

    End Sub

    Private Sub XtraForm1_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        Me.Visible = False
        NotifyIcon1.Visible = True
    End Sub


    Private Sub ReanudarElMonitorToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReanudarElMonitorToolStripMenuItem.Click
        If XtraMessageBox.Show("Esta acción reanudará el envío de alertas. ¿Desea reanudar el monitoreo de las fallas?", "Reanudar la aplicación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.No Then
            Estado = 1
            SimpleButton3.Visible = True
            SimpleButton2.Visible = False
            ContextMenuStrip1.Items(1).Enabled = True
            ContextMenuStrip1.Items(2).Enabled = False
            estadoPrograma = True
            agregarLOG("La interfaz ha sido reanudada por un usuario", 9, 0)
        End If
    End Sub

    Private Sub ToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem1.Click
        If XtraMessageBox.Show("Esta acción detendrá el envío de alertas. ¿Desea detener el monitor de las fallas?", "Detener la aplicación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.No Then
            Estado = 1
            SimpleButton3.Visible = False
            SimpleButton2.Visible = True
            ContextMenuStrip1.Items(1).Enabled = False
            ContextMenuStrip1.Items(2).Enabled = True
            estadoPrograma = False
            agregarLOG("La interfaz ha sido detenida por un usuario", 9, 0)
        End If
    End Sub

    Private Sub XtraForm1_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Dim f As Form
        f = sender

        'Check if the form is minimized
        If f.WindowState = FormWindowState.Minimized Then
            Me.Visible = False
            NotifyIcon1.Visible = True
        End If

    End Sub

    Private Sub XtraForm1_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        autenticado = False
        Dim Forma As New XtraForm2
        Forma.Text = "Detener aplicación"
        Forma.ShowDialog()
        If autenticado Then
            If XtraMessageBox.Show("Esta acción CERRARÁ la aplicación de monitoreo. ¿Desea continuar?", "Detener la aplicación", MessageBoxButtons.YesNo, MessageBoxIcon.Stop) <> DialogResult.No Then
                agregarLOG("La aplicación se cerró el usuario: " & usuarioCerrar, 9, 0)
            Else
                e.Cancel = True
            End If
        Else
            e.Cancel = True
        End If
    End Sub

    Sub agregarSolo(cadena As String)
        ListBoxControl1.Items.Insert(0, "MONITOR " & Format(Now, "dd-MMM HH:mm:ss") & ": " & cadena)
        ContarLOG()
    End Sub

    Sub depurar()
        'Se depura la BD
        If Format(DateAndTime.Now(), "HHmm") = "0000" And Not depurando Then
            depurando = True
            'Se produce la depuración a las 12 de la noche
            Dim cadSQL As String = "SELECT gestion_meses, gestion_log FROM " & rutaBD & ".configuracion WHERE gestion_meses > 0 AND (ISNULL(gestion_log) OR gestion_log < '" & Format(Now(), "yyyyMM") & "')"
            Dim reader As DataSet = consultaSEL(cadSQL)
            Dim regsAfectados = 0
            Dim eliminados = 0
            Dim mesesAtras = 1
            If reader.Tables(0).Rows.Count > 0 Then
                mesesAtras = reader.Tables(0).Rows(0)!gestion_meses
                If mesesAtras > 0 Then
                    regsAfectados = consultaACT("DELETE FROM " & rutaBD & ".reportes WHERE cierre_reporte < '" & Format(DateAndTime.DateAdd(DateInterval.Month, mesesAtras * -1, Now()), "yyyy/MM") & "/01 00:00:00' AND estatus = 1000")
                    eliminados = eliminados + regsAfectados

                    regsAfectados = consultaACT("DELETE FROM " & rutaBD & ".lotes WHERE fecha < '" & Format(DateAndTime.DateAdd(DateInterval.Month, mesesAtras * -1, Now()), "yyyy/MM") & "/01 00:00:00'")
                    eliminados = eliminados + regsAfectados

                    regsAfectados = consultaACT("DELETE FROM " & rutaBD & ".lotes_historia WHERE lote Not IN (SELECT id FROM " & rutaBD & ".lotes)")
                    eliminados = eliminados + regsAfectados

                    regsAfectados = consultaACT("DELETE From " & rutaBD & ".ruta_congelada WHERE lote Not IN (SELECT id FROM " & rutaBD & ".lotes)")
                    eliminados = eliminados + regsAfectados

                    regsAfectados = consultaACT("DELETE FROM " & rutaBD & ".calidad_historia WHERE lote Not IN (SELECT id FROM " & rutaBD & ".lotes)")
                    eliminados = eliminados + regsAfectados

                    regsAfectados = consultaACT("DELETE FROM " & rutaBD & ".detalleparos WHERE fecha < '" & Format(DateAndTime.DateAdd(DateInterval.Month, mesesAtras * -1, Now()), "yyyy/MM") & "/01 00:00:00'")
                    eliminados = eliminados + regsAfectados

                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".configuracion SET gestion_log = '" & Format(Now(), "yyyyMM") & "'")
                    eliminados = eliminados + regsAfectados

                    regsAfectados = consultaACT("DELETE FROM " & rutaBD & ".control WHERE fecha < '" & Format(DateAndTime.DateAdd(DateInterval.Month,
    mesesAtras * -1, Now()), "yyyyMM") & "0100'")


                    agregarLOG("Se ejecutó la depuración de la base de datos para el período " & Format(Now(), "MMMM-yyyy") & " (todo lo anterior al día: " & Format(DateAndTime.DateAdd(DateInterval.Month, mesesAtras * -1, Now()), "yyyy/MM") & "/01). Se eliminaron permanentemente " & eliminados & " registro(s)", 7, 0)

                End If
            End If
            mesesAtras = 1
            regsAfectados = consultaACT("DELETE FROM " & rutaBD & ".alarmas WHERE fin < '" & Format(DateAndTime.DateAdd(DateInterval.Day, -10, Now()), "yyyy/MM/dd") & " 00:00:00'")

            'Elvis
            regsAfectados = consultaACT("DELETE FROM " & rutaBD & ".lecturas WHERE fecha < '" & Format(DateAndTime.DateAdd(DateInterval.Day, -2, Now()), "yyyy/MM/dd") & " 00:00:00' AND estatus = 2")
            regsAfectados = consultaACT("DELETE FROM " & rutaBD & ".lecturas_cortes WHERE dia < '" & Format(DateAndTime.DateAdd(DateInterval.Month, -24, Now()), "yyyy/MM/dd") & "'")

            'regsAfectados = consultaACT("DELETE FROM " & rutaBD & ".mensajes WHERE enviada < '" & Format(DateAndTime.DateAdd(DateInterval.Month, mesesAtras * -1, Now()), "yyyy/MM") & "/01 00:00:00'")
            regsAfectados = consultaACT("DELETE FROM " & rutaBD & ".mensajes WHERE enviada < '" & Format(DateAndTime.DateAdd(DateInterval.Day, -2, Now()), "yyyy/MM/dd") & " 00:00:00'")
            eliminados = eliminados + regsAfectados
            regsAfectados = consultaACT("DELETE FROM " & rutaBD & ".log WHERE fecha < '" & Format(DateAndTime.DateAdd(DateInterval.Day, -15, Now()), "yyyy/MM/dd") & " 00:00:00'")
            eliminados = eliminados + regsAfectados
            regsAfectados = consultaACT("DELETE FROM " & rutaBD & ".lecturas_resumen WHERE desde < '" & Format(DateAndTime.DateAdd(DateInterval.Day, -5, Now()), "yyyy/MM/dd") & " 00:00:00'")
            eliminados = eliminados + regsAfectados
            regsAfectados = consultaACT("DELETE FROM " & rutaBD & ".mensajes_procesados WHERE fecha < '" & Format(DateAndTime.DateAdd(DateInterval.Day, -2, Now()), "yyyy/MM/dd") & "'")

            eliminados = eliminados + regsAfectados
            depurando = False
        End If
    End Sub

    Private Sub revisarLog_Tick(sender As Object, e As EventArgs) Handles revisarLog.Tick
        revisarLog.Enabled = False
        If leyendoLog Then Exit Sub
        leyendoLog = True
        Dim regsAfectados = consultaACT("UPDATE " & rutaBD & ".log SET visto = 'P' WHERE visto = 'N'")
        Dim cadSQL = "SELECT id, texto, aplicacion FROM " & rutaBD & ".log WHERE visto = 'P' ORDER BY id"
        Dim reader = consultaSEL(cadSQL)
        If reader.Tables(0).Rows.Count > 0 Then
            sinEventos.Enabled = False
            sinEventos.Enabled = True
            For Each elmensaje In reader.Tables(0).Rows
                Dim appOrigen = "MONITOR"
                If elmensaje!aplicacion = 30 Then
                    appOrigen = "TELEFONIA"
                ElseIf elmensaje!aplicacion = 40 Then
                    appOrigen = "CORREOS"
                ElseIf elmensaje!aplicacion = 20 Then
                    appOrigen = "MMCALL"
                ElseIf elmensaje!aplicacion = 60 Then
                    appOrigen = "LOG"
                ElseIf elmensaje!aplicacion = 50 Then
                    appOrigen = "SMS"
                ElseIf elmensaje!aplicacion = 70 Then
                    appOrigen = "VOZ"
                ElseIf elmensaje!aplicacion = 80 Then
                    appOrigen = "REPORTES"
                End If
                ListBoxControl1.Items.Insert(0, appOrigen & " " & Format(Now, "dd-MMM HH:mm:ss") & ": " & elmensaje!texto)

            Next
            regsAfectados = consultaACT("UPDATE " & rutaBD & ".log SET visto = 'S' WHERE visto = 'P'")
            ContarLOG()
        End If
        leyendoLog = False
        revisarLog.Enabled = True
    End Sub

    Private Sub sinEventos_Tick(sender As Object, e As EventArgs) Handles sinEventos.Tick
        agregarSolo("No se ha generado información durante los ultimos 5 minutos")
    End Sub

    Private Sub escalamiento_Tick(sender As Object, e As EventArgs) Handles escalamiento.Tick
        If procesandoEscalamientos Or Not estadoPrograma Then Exit Sub
        escalamiento.Enabled = False
        escalamientos()
        escalamiento.Enabled = True
    End Sub

    Private Sub reportes_Tick(sender As Object, e As EventArgs) Handles reportes.Tick
        If enviandoReportes Or Not estadoPrograma Then Exit Sub
        If Format(Now, "mm") >= "05" Then
            entroReportes = False
            Exit Sub
        ElseIf Not entroReportes Then
            entroReportes = True
        Else
            Exit Sub

        End If
        reportes.Enabled = False
        enviandoReportes = True
        Try

            Shell(Application.StartupPath & "\reportes.exe " & Chr(34) & cadenaConexion & Chr(34), AppWinStyle.MinimizedNoFocus)
            agregarSolo("Se inicia la aplicación de Envío de reportes por correo")
        Catch ex As Exception
            agregarLOG("Error en la ejecución de la aplicación de envío de reportes por correos. Error: " & ex.Message, 7, 0)
        End Try
        enviandoReportes = False
        reportes.Enabled = True
    End Sub

    Private Sub arduino_Tick(sender As Object, e As EventArgs) Handles arduino.Tick
        If Not estadoPrograma Then Exit Sub

        Dim cadSQL As String = "SELECT ruta_audios, ruta_sms, be_alarmas_llamadas, be_alarmas_sms FROM " & rutaBD & ".configuracion"
        Dim reader As DataSet = consultaSEL(cadSQL)
        Dim regsAfectados = 0
        Dim rutaAudios
        Dim rutaSMS
        Dim be_alarmas_llamadas As Boolean = False
        Dim be_alarmas_sms As Boolean = False

        If errorBD.Length > 0 Then
            agregarLOG("No se logró la conexión con MySQL. Error: " + errorBD, 9, 0)

        Else
            rutaAudios = ValNull(reader.Tables(0).Rows(0)!ruta_audios, "A")
            rutaSMS = ValNull(reader.Tables(0).Rows(0)!ruta_sms, "A")
            be_alarmas_llamadas = ValNull(reader.Tables(0).Rows(0)!be_alarmas_llamadas, "A") = "S"
            be_alarmas_sms = ValNull(reader.Tables(0).Rows(0)!be_alarmas_sms, "A") = "S"
        End If
        Dim llamarPrograma As Boolean = False
        If be_alarmas_llamadas Or be_alarmas_sms Then
            If rutaSMS.Length = 0 Then
                rutaSMS = My.Computer.FileSystem.SpecialDirectories.MyDocuments
            Else
                rutaSMS = Strings.Replace(rutaSMS, "/", "\")
            End If
            If Not My.Computer.FileSystem.DirectoryExists(rutaSMS) Then
                rutaSMS = My.Computer.FileSystem.SpecialDirectories.MyDocuments
            End If

            If rutaAudios.Length = 0 Then
                rutaAudios = My.Computer.FileSystem.SpecialDirectories.MyDocuments
            Else
                rutaAudios = Strings.Replace(rutaAudios, "/", "\")
            End If
            If Not My.Computer.FileSystem.DirectoryExists(rutaAudios) Then
                rutaAudios = My.Computer.FileSystem.SpecialDirectories.MyDocuments
            End If

            If Not My.Computer.FileSystem.DirectoryExists(rutaAudios) Then
                rutaAudios = My.Computer.FileSystem.SpecialDirectories.MyDocuments
            End If
            Dim LlamadasPendientes = 0
            For Each FoundFile As String In My.Computer.FileSystem.GetFiles(
          rutaAudios, Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly, "*.wav")
                Dim Numero = Microsoft.VisualBasic.Strings.Left(Path.GetFileName(FoundFile), 10)
                Dim nombreArchivo = Path.GetFileName(FoundFile)
                If IsNumeric(Numero) Then LlamadasPendientes = LlamadasPendientes + 1
            Next
            If LlamadasPendientes > 0 And be_alarmas_llamadas Then
                llamarPrograma = True
            End If
            If Not llamarPrograma Then
                For Each FoundFile As String In My.Computer.FileSystem.GetFiles(
          rutaSMS, Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly, "*.txt")
                    Dim Numero = Microsoft.VisualBasic.Strings.Left(Path.GetFileName(FoundFile), 10)
                    Dim nombreArchivo = Path.GetFileName(FoundFile)
                    If IsNumeric(Numero) Then LlamadasPendientes = LlamadasPendientes + 1
                Next
                If LlamadasPendientes > 0 And be_alarmas_llamadas Then
                    llamarPrograma = True
                End If
            End If
        End If
        If llamarPrograma Then
            Try
                Shell(Application.StartupPath & "\arduino.exe " & Chr(34) & cadenaConexion & Chr(34), AppWinStyle.MinimizedNoFocus)
                agregarSolo("Se inicia la aplicación de Interfaz telefónica")
            Catch ex As Exception
                agregarLOG("Error en la ejecución de la aplicación de Interfaz telefónica Error: " & ex.Message, 7, 0)
            End Try
        End If
    End Sub

    Sub validarLicencia()
        'Dim cadSQL = "SELECT licencia FROM " & rutabd & ".configuracion"
        ' Dim falla As DataSet = consultaSEL(cadSQL)
        'If falla.Tables(0).Rows.Count > 0 Then
        'If ValNull(falla.Tables(0).Rows(0)!volumen, "A") = "" Then
        'XtraMessageBox.Show("Su licencia no pudo ser validada. Introduzca una licencia en la aplicación web de andon e intente de nuevo o contacte con su proveedor de ANDON-SIGMA", "No es posible continuar", MessageBoxButtons.OK, MessageBoxIcon.Error)
        'Application.Exit()
        'Else
        'Dim miLicencia = ValNull(falla.Tables(0).Rows(0)!volumen, "A")
        'If Strings.Mid(miLicencia, 11, 1) = "V" Or Strings.Mid(miLicencia, 11, 1) <> "S" Then
        'XtraMessageBox.Show("Su licencia ha caducado. Contacte con su proveedor de ANDON-SIGMA", "No es posible continuar", MessageBoxButtons.OK, MessageBoxIcon.Error)
        'Application.Exit()
        'End If
        'End If
        'End If
    End Sub

    Private Sub reenviarMMCALL_Tick(sender As Object, e As EventArgs) Handles reenviarMMCALL.Tick

        If reenviar Then Exit Sub
        reenviar = True
        If Not estadoPrograma Then Exit Sub
        Try

            Shell(Application.StartupPath & "\repeticiones.exe " & Chr(34) & cadenaConexion & Chr(34), AppWinStyle.MinimizedNoFocus)
        Catch ex As Exception
            agregarLOG("Error en la ejecución de la aplicación de envío de repeticiones de MMCall. Error: " & ex.Message, 7, 0)
        End Try

        reenviar = False
    End Sub

    Function validarURI(ByVal cadena As String) As Boolean
        Dim validatedUri As System.Uri
        Return Uri.TryCreate(cadena, UriKind.RelativeOrAbsolute, validatedUri)
    End Function

    Private Sub sensores_Tick(sender As Object, e As EventArgs) Handles sensores.Tick
        If revisandoSensores Then Exit Sub
        If primerSensor Then primerSensor = False
        Dim ultimo = DateAndTime.Now()
        revisandoSensores = True
        Dim regsAfectados As Long = 0
        Dim cadAdic = ""
        Dim cadAdic2 = ""
        Dim produccion As Long = 0, produccion_seg = 0, calidad As Long = 0, buffer As Long = 0
        Dim produccion_tc As Double = 0, calidad_tc As Double = 0
        Dim general As DataSet
        incluyeHoyos = False
        Dim cadSQL = "SELECT modulo_oee, turno_oee, andon_prorrateado FROM " & rutaBD & ".configuracion WHERE modulo_oee = 'S'"
        Dim config As DataSet = consultaSEL(cadSQL)
        Dim aCortar = False
        Dim TTotal As Long = 0
        Dim hayTipo3 As Boolean
        Dim AP As Boolean = False

        If config.Tables(0).Rows.Count > 0 Then
            AP = ValNull(config.Tables(0).Rows(0)!andon_prorrateado, "A") = "S"
            'Se calcula el turno

            regsAfectados = consultaACT("UPDATE " & rutaBD & ".lecturas SET estatus = 1 WHERE estatus = 0")
            cadSQL = "SELECT c.id AS maquinaid, c.oee_estado, c.oee_estado_desde, c.oee_historico_rate, c.oee_historico_rate_reiniciar, c.linea, c.oee_umbral_produccion, c.nombre AS nequipo, e.nombre AS nparte, e.referencia, f.nombre AS ntripulacion, g.numero AS norden, d.nombre AS nturno, c.oee_turno_actual, c.oee_tripulacion_actual, c.oee_lote_actual, c.oee_parte_actual, b.id, b.equipo, b.tipo, b.multiplicador, b.base, DATE(a.fecha) AS fecha, IFNULL(SUM(a.valor), 0) AS totals, d.mover FROM " & rutaBD & ".cat_maquinas c INNER JOIN " & rutaBD & ".relacion_procesos_sensores b ON b.equipo = c.id and b.estatus = 'A' LEFT JOIN " & rutaBD & ".cat_partes e ON c.oee_parte_actual = e.id AND e.estatus = 'A' LEFT JOIN " & rutaBD & ".cat_tripulacion f ON c.oee_tripulacion_actual = f.id AND f.estatus = 'A' LEFT JOIN " & rutaBD & ".cat_turnos d ON c.oee_turno_actual = d.id AND d.estatus = 'A' LEFT JOIN " & rutaBD & ".lecturas a ON b.sensor = a.sensor AND a.estatus = 1 LEFT JOIN " & rutaBD & ".lotes g ON c.oee_lote_actual = g.id AND g.estatus = 'A' WHERE c.oee = 'S' AND c.estatus = 'A' GROUP BY c.linea, c.nombre, e.nombre, e.referencia, f.nombre, b.equipo, b.tipo, b.multiplicador, b.base, DATE(a.fecha), c.oee_turno_actual, c.oee_tripulacion_actual, c.oee_lote_actual, c.oee_parte_actual, d.mover"

            Dim capturas = consultaSEL(cadSQL)
            If capturas.Tables(0).Rows.Count > 0 Then
                Dim equipoActual = 0
                For Each captura In capturas.Tables(0).Rows
                    hayTipo3 = False
                    'MsgBox("Maquina  a maquina: " & captura!maquinaid)
                    If captura!maquinaid = 2 Then
                        Dim uno = 1
                    End If
                    Dim cadPlaneado As String = ", planeado = 'N'"
                    produccion_seg = 0
                    produccion_tc = 0
                    calidad_tc = 0
                    'Buscar rate
                    Dim rateEquipo As Double = 1
                    Dim rateBajo As Double = 75
                    Dim rateAlto As Double = 125
                    Dim medTiempo As Long = 0
                    Dim rateUnidad = "PzaxHr"
                    Dim cadSQLParo = ""
                    Dim funcionando As Boolean

                    cadSQL = "SELECT piezas, bajo, alto, tiempo, unidad FROM " & rutaBD & ".relacion_partes_equipos WHERE (equipo = " & captura!maquinaid & " Or equipo = 0) And (parte = " & captura!oee_parte_actual & " Or parte = 0) ORDER BY parte DESC, equipo DESC LIMIT 1"
                    general = consultaSEL(cadSQL)
                    If general.Tables(0).Rows.Count > 0 Then
                        ''MsgBox("entro 4 " & general.Tables(0).Rows(0)!piezas)
                        rateEquipo = general.Tables(0).Rows(0)!piezas
                        rateBajo = general.Tables(0).Rows(0)!bajo
                        rateAlto = general.Tables(0).Rows(0)!alto
                        medTiempo = general.Tables(0).Rows(0)!tiempo
                        rateUnidad = ValNull(general.Tables(0).Rows(0)!unidad, "A")
                    End If

                    If rateEquipo = 0 Then rateEquipo = 1
                    Dim TC As Double = 0
                    If medTiempo = 2 Then
                        TC = 3600 / rateEquipo
                    ElseIf medTiempo = 1 Then
                        TC = 60 / rateEquipo
                        ' TC = rateEquipo / 60
                    ElseIf medTiempo = 0 Then
                        TC = 1 / rateEquipo
                    ElseIf medTiempo = 3 Then
                        TC = 86400 / rateEquipo
                    End If

                    TC = Math.Round(TC, 10)
                    'MsgBox("Tiempo ciclo " & TC


                    'Buscar Paro

                    Dim conProduccion = False
                    Dim conRateBajo = False
                    cadAdic = ""
                    cadAdic2 = ""
                    Dim paroActual As Long = 0
                    Dim reporteActual As Long = 0
                    Dim paroDesde = ultimo
                    Dim paroCorte = ultimo
                    Dim fechaEstado = ultimo
                    Dim tiempoEstado As Long = 0
                    Dim tiempoParo As Long = 0
                    Dim claseParoActual = 0
                    Dim cadDetener = ""
                    Dim cadIniciarParo = ""

                    'Se pregunta si hay un paro activo
                    Dim cadSQLOri = "SELECT id, clase, corte, reporte, finaliza_sensor, tipo, estado, inicia, finaliza FROM " & rutaBD & ".detalleparos WHERE maquina = " & captura!maquinaid & " AND estatus = 'A' AND estado = 'C' ORDER BY tipo, inicia LIMIT 1"
                    general = consultaSEL(cadSQLOri)
                    TTotal = 0
                    'Del mismo modo, se busca si hay un paro planeado vencido
                    cadSQL = "SELECT id FROM " & rutaBD & ".detalleparos WHERE maquina = " & captura!maquinaid & " AND desde <= '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' AND hasta > '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' AND estatus = 'A' AND clase = 0 AND estado = 'L' ORDER BY tipo, inicia LIMIT 1"
                    Dim general2 As DataSet = consultaSEL(cadSQL)
                    TTotal = 0
                    If general2.Tables(0).Rows.Count > 0 Then
                        'Hay paro vencido
                        If general.Tables(0).Rows.Count > 0 Then
                            'Si hay un paro activo, se termina
                            If Not general.Tables(0).Rows(0)!corte.Equals(System.DBNull.Value) Then
                                paroCorte = general.Tables(0).Rows(0)!corte
                            Else
                                paroCorte = general.Tables(0).Rows(0)!inicia
                            End If
                            If paroCorte <> ultimo Then
                                tiempoParo = tiempoValido(paroCorte, ultimo, captura!maquinaid)
                            End If
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".detalleparos SET estado = 'F', finaliza = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', hasta = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', tiempo = " & tiempoValido(general.Tables(0).Rows(0)!inicia, ultimo, captura!maquinaid) & ", finalizo = 1 WHERE id = " & general.Tables(0).Rows(0)!id & ";UPDATE " & rutaBD & ".detalleparos SET hasta = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & general.Tables(0).Rows(0)!id & " AND clase <> 0")

                        End If
                        'Se activa el paro vencido

                        cadDetener = "UPDATE " & rutaBD & ".detalleparos SET estado = 'C', turno = " & config.Tables(0).Rows(0)!turno_oee & ", inicia = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', inicio = 1, turno = " & captura!oee_turno_actual & ", fecha = '" & Format(ultimo, "yyyy/MM/dd") & "' WHERE id = " & general2.Tables(0).Rows(0)!id & ";UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'D', parada_desde  = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', estado_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE equipo = " & captura!maquinaid
                        regsAfectados = consultaACT(cadDetener)

                        general = consultaSEL(cadSQLOri)
                    End If

                    If general.Tables(0).Rows.Count > 0 Then
                        'Si hay un paro planeado o no, se pregunta si ya se venció
                        cadSQL = "SELECT id, inicia, corte FROM " & rutaBD & ".detalleparos WHERE id = " & general.Tables(0).Rows(0)!id & " AND hasta <= '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                        general2 = consultaSEL(cadSQL)
                        TTotal = 0
                        If general2.Tables(0).Rows.Count > 0 Then
                            'Si ya se venció se calcula su monto y se cierra
                            If Not general2.Tables(0).Rows(0)!corte.Equals(System.DBNull.Value) Then
                                paroCorte = general2.Tables(0).Rows(0)!corte
                            Else
                                paroCorte = general2.Tables(0).Rows(0)!inicia
                            End If
                            If paroCorte <> ultimo Then
                                tiempoParo = tiempoParo + tiempoValido(paroCorte, ultimo, captura!maquinaid)
                            End If
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".detalleparos SET estado = 'F', finaliza = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', tiempo = " & tiempoValido(general2.Tables(0).Rows(0)!inicia, ultimo, captura!maquinaid) & ", finalizo = 1 WHERE id = " & general2.Tables(0).Rows(0)!id & ";UPDATE " & rutaBD & ".detalleparos SET hasta = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & general2.Tables(0).Rows(0)!id & " AND clase <> 0;UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'F', parada_desde = NULL, estado_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', ultima_reparacion = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE equipo = " & captura!maquinaid)
                            If regsAfectados > 0 Then

                                paroActual = 0
                                reporteActual = 0
                                claseParoActual = 0
                            End If
                        Else
                            'Se mantiene el paro actual
                            paroActual = general.Tables(0).Rows(0)!id
                            reporteActual = general.Tables(0).Rows(0)!reporte
                            claseParoActual = general.Tables(0).Rows(0)!clase

                            If claseParoActual = 0 Then
                                cadPlaneado = ", planeado = 'S'"
                            End If

                            paroDesde = general.Tables(0).Rows(0)!inicia

                            If Not general.Tables(0).Rows(0)!corte.Equals(System.DBNull.Value) Then
                                paroCorte = general.Tables(0).Rows(0)!corte
                            Else
                                paroCorte = paroDesde
                            End If

                            Dim pFinalizado = False
                            If reporteActual > 0 Then
                                'Si el paro proviene de un reporte, se pregunta el estatus de éste


                                cadSQL = "SELECT id, fecha, estatus, cierre_atencion FROM " & rutaBD & ".reportes WHERE id = " & reporteActual & " AND afecta_oee = 'S'"

                                Dim miReporte = consultaSEL(cadSQL)

                                If miReporte.Tables(0).Rows.Count > 0 Then
                                    'Se cerró el reporte
                                    If miReporte.Tables(0).Rows(0)!estatus > 10 Then
                                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".detalleparos SET estado = 'F', tiempo = " & tiempoValido(paroDesde, miReporte.Tables(0).Rows(0)!cierre_atencion, captura!maquinaid) & ", finaliza = '" & Format(miReporte.Tables(0).Rows(0)!cierre_atencion, "yyyy/MM/dd HH:mm:ss") & "', hasta = '" & Format(miReporte.Tables(0).Rows(0)!cierre_atencion, "yyyy/MM/dd HH:mm:ss") & "', finalizo_accion = 'R' WHERE reporte = " & reporteActual & ";UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'F', ultima_reparacion = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', estado_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE equipo = " & captura!maquinaid)
                                        reporteActual = 0
                                        paroActual = 0
                                        claseParoActual = 0
                                        pFinalizado = True

                                    End If
                                End If
                            End If
                            If Not pFinalizado Then
                                'Si el paro aún esta vivo
                                If ValNull(captura!oee_estado, "A") = "S" Then
                                    'Se reactiva la máquina
                                    fechaEstado = captura!oee_estado_desde
                                    tiempoEstado = DateAndTime.DateDiff(DateInterval.Second, fechaEstado, ultimo)
                                    TTotal = tiempoEstado
                                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".cat_maquinas SET oee_estado = 'N', oee_estado_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', oee_estado_cambio = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & captura!maquinaid)
                                    funcionando = False
                                End If
                                'Se contabiliza el tiempo del paro
                                If paroCorte <> ultimo Then
                                    tiempoParo = tiempoParo + tiempoValido(paroCorte, ultimo, captura!maquinaid)
                                End If
                            End If
                        End If
                    End If

                    produccion_seg = 0
                    produccion = 0
                    calidad = 0
                    buffer = 0
                    Dim piezas As Long = 0
                    Dim fecha = ultimo
                    If captura!totals > 0 Then
                        fecha = captura!fecha
                        piezas = captura!totals * captura!multiplicador
                        If captura!base > 0 Then
                            piezas = piezas / captura!base
                        End If
                    End If
                    If ValNull(captura!mover, "N") = 1 Then
                        fecha = DateAdd(DateInterval.Day, -1, fecha)
                    ElseIf ValNull(captura!mover, "N") = 2 Then
                        fecha = DateAdd(DateInterval.Day, 1, fecha)
                    End If

                    If captura!tipo = 3 Then
                        hayTipo3 = True
                        cadSQL = "SELECT oee_estado, oee_estado_desde FROM " & rutaBD & ".cat_maquinas WHERE id = " & captura!maquinaid
                        general = consultaSEL(cadSQL)
                        If general.Tables(0).Rows.Count > 0 Then
                            If ValNull(general.Tables(0).Rows(0)!oee_estado, "A") = "S" Then
                                tiempoEstado = DateAndTime.DateDiff(DateInterval.Second, general.Tables(0).Rows(0)!oee_estado_desde, ultimo)
                                TTotal = TTotal + tiempoEstado
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".cat_maquinas SET oee_estado_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & captura!maquinaid & ";UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'F' WHERE equipo = " & captura!maquinaid)
                                funcionando = True
                            End If
                        End If
                    End If

                    'Buscar si ya existe el corte
                    cadSQL = "SELECT id, paro_actual, bloque_inicia, bloque_finaliza FROM " & rutaBD & ".lecturas_cortes WHERE dia = '" & Format(fecha, "yyyy/MM/dd") & "' AND equipo = " & captura!maquinaid & " AND orden = " & captura!oee_lote_actual & " AND parte = " & captura!oee_parte_actual & " AND turno = " & captura!oee_turno_actual & " AND tc = " & TC & " AND tripulacion = " & captura!oee_tripulacion_actual
                    Dim miEquipo = consultaSEL(cadSQL)
                    Dim cadParo = ""
                    If miEquipo.Tables(0).Rows.Count > 0 Then
                        'MsgBox("Piezas del sensor " & captura!totals)
                        'Se busca el turno
                        If captura!totals > 0 Then
                            If captura!tipo = 0 Then
                                cadAdic = ", produccion = produccion + " & piezas
                                cadAdic = cadAdic & ", produccion_tc = produccion_tc + " & piezas * TC
                                produccion = piezas
                                produccion_tc = piezas * TC
                                produccion_seg = piezas
                            ElseIf captura!tipo = 1 Or captura!tipo = 4 Then
                                cadAdic = ", calidad = calidad + " + piezas
                                cadAdic = cadAdic & ", calidad_tc = calidad_tc + " + piezas * TC
                                calidad = piezas
                                calidad_tc = piezas * TC
                            ElseIf captura!tipo = 2 Then
                                cadAdic = ", buffer = buffer + " + piezas
                                buffer = piezas
                            ElseIf captura!tipo = 3 Then

                                cadSQL = "SELECT fecha, valor FROM " & rutaBD & ".lecturas WHERE sensor = " & captura!id & " AND estatus = 1 ORDER BY id"
                                Dim regsSensor = consultaSEL(cadSQL)
                                If regsSensor.Tables(0).Rows.Count > 0 Then
                                    Dim fechaInicial = miEquipo.Tables(0).Rows(0)!bloque_inicia
                                    If Not captura!oee_estado_desde.Equals(System.DBNull.Value) Then
                                        fechaInicial = captura!oee_estado_desde
                                    End If
                                    For Each rSensor In regsSensor.Tables(0).Rows
                                        If rSensor!valor = 2 And ValNull(captura!oee_estado, "A") <> "S" Then
                                            'Encendido
                                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".cat_maquinas SET oee_estado = 'S', oee_estado_desde = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "', oee_estado_cambio = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & captura!maquinaid & ";UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'F', estado_desde = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "' WHERE equipo = " & captura!maquinaid)
                                            funcionando = True
                                            fechaInicial = rSensor!fecha
                                        ElseIf rSensor!valor = 1 And ValNull(captura!oee_estado, "A") = "S" Then
                                            'Apagado
                                            tiempoEstado = DateAndTime.DateDiff(DateInterval.Second, fechaInicial, rSensor!fecha)
                                            TTotal = TTotal + tiempoEstado
                                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".cat_maquinas SET oee_estado = 'N', oee_estado_desde = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "', oee_estado_cambio = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & captura!maquinaid & ";UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'D', estado_desde = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "' WHERE equipo = " & captura!maquinaid)
                                            funcionando = False
                                        End If
                                    Next
                                    cadAdic = ", produccion = produccion + " & TTotal & ", produccion_tc = produccion_tc + " & TTotal
                                    produccion_seg = TTotal
                                End If
                            End If
                        ElseIf captura!tipo = 3 And TTotal > 0 Then
                            cadAdic = ", produccion = produccion + " & TTotal & ", produccion_tc = produccion_tc + " & TTotal
                            produccion_seg = TTotal
                        End If

                        If reporteActual > 0 And AP And tiempoParo > 0 Then
                            tiempoParo = tiempoParo - produccion_tc
                        End If

                        cadAdic = cadAdic & ", paro = paro + " & tiempoParo

                        cadSQL = "UPDATE " & rutaBD & ".lecturas_cortes SET tiempo_disponible = " & tiempoValido(miEquipo.Tables(0).Rows(0)!bloque_inicia, ultimo, captura!maquinaid) & ", bloque_finaliza = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'" & cadAdic & " WHERE id = " & miEquipo.Tables(0).Rows(0)!id & cadParo
                    Else
                        'NUEVO LECTURA CORTE
                        If captura!totals > 0 Then
                            cadAdic2 = cadAdic2 & ", " & piezas & ", " & piezas * TC
                            If captura!tipo = 0 Then
                                cadAdic = ", produccion, produccion_tc"
                                produccion = piezas
                                produccion_seg = piezas
                                produccion_tc = piezas * TC
                            ElseIf captura!tipo = 1 Then
                                cadAdic = ", calidad, calidad_tc"
                                calidad = piezas
                            ElseIf captura!tipo = 2 Then
                                cadAdic = ", buffer"
                                buffer = piezas
                            ElseIf captura!tipo = 3 Then
                                cadSQL = "SELECT fecha, valor FROM " & rutaBD & ".lecturas WHERE sensor = " & captura!id & " AND estatus = 1 ORDER BY id"
                                Dim regsSensor = consultaSEL(cadSQL)
                                If regsSensor.Tables(0).Rows.Count > 0 Then
                                    Dim fechaInicial = ultimo
                                    If Not captura!oee_estado_desde.Equals(System.DBNull.Value) Then
                                        fechaInicial = captura!oee_estado_desde
                                    End If
                                    For Each rSensor In regsSensor.Tables(0).Rows
                                        If rSensor!valor = 2 And ValNull(captura!oee_estado, "A") <> "S" Then
                                            'Encendido
                                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".cat_maquinas SET oee_estado = 'S', oee_estado_desde = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "', oee_estado_cambio = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & captura!maquinaid & ";UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'F', estado_desde = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "' WHERE equipo = " & captura!maquinaid)
                                            funcionando = True
                                            fechaInicial = rSensor!fecha
                                        ElseIf rSensor!valor = 1 And ValNull(captura!oee_estado, "A") = "S" Then
                                            'Apagado
                                            tiempoEstado = DateAndTime.DateDiff(DateInterval.Second, fechaInicial, rSensor!fecha)
                                            TTotal = TTotal + tiempoEstado
                                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".cat_maquinas SET oee_estado = 'N', oee_estado_desde = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "', oee_estado_cambio = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & captura!maquinaid & ";UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'D', estado_desde = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "' WHERE equipo = " & captura!maquinaid)
                                            funcionando = False
                                        End If
                                    Next
                                    cadAdic = ", produccion, produccion_tc"
                                    cadAdic2 = cadAdic2 & ", " & TTotal & ", " & TTotal
                                    produccion_seg = TTotal
                                End If
                            End If
                        ElseIf captura!tipo = 3 And TTotal > 0 Then
                            cadAdic = ", produccion, produccion_tc"
                            cadAdic2 = cadAdic2 & ", " & TTotal & ", " & TTotal
                            produccion_seg = TTotal

                        End If

                        If reporteActual > 0 And AP And tiempoParo > 0 Then
                            tiempoParo = tiempoParo - produccion_tc
                        End If

                        If paroActual > 0 Then
                            cadAdic = cadAdic & ", paro"
                            cadAdic2 = cadAdic2 & ", " & tiempoParo
                        End If

                        cadSQL = "INSERT " & rutaBD & ".lecturas_cortes (dia, orden, parte, turno, equipo, tripulacion" & cadAdic & ", bloque_inicia, bloque_finaliza, tc) VALUES ('" & Format(fecha, "yyyy/MM/dd") & "', " & captura!oee_lote_actual & ", " & captura!oee_parte_actual & ", " & captura!oee_turno_actual & ", " & captura!maquinaid & ", " & captura!oee_tripulacion_actual & cadAdic2 & ", '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', " & TC & ")" & cadParo
                    End If
                    regsAfectados = consultaACT(cadSQL)
                    If equipoActual <> captura!maquinaid Or equipoActual = 0 Then
                        equipoActual = captura!maquinaid
                        cadSQLParo = ""
                        cadAdic = ""
                        cadAdic2 = ""
                        'Buscar objetivo
                        Dim objetivo As Double = 0
                        Dim reinicio As Long = 0
                        cadSQL = "SELECT objetivo, reinicio FROM " & rutaBD & ".equipos_objetivo WHERE (equipo = " & captura!maquinaid & " OR equipo = 0) AND (parte = " & captura!oee_parte_actual & " OR parte = 0) AND (fijo = 'S' OR ('" & Format(fecha, "yyyy/MM/dd") & "' >= desde AND '" & Format(fecha, "yyyy/MM/dd") & "' <= hasta AND (turno = 0 OR turno = " & captura!oee_turno_actual & ") AND (lote = 0 OR lote = " & captura!oee_lote_actual & "))) ORDER BY parte DESC, equipo DESC, fijo, lote DESC, turno DESC LIMIT 1"
                        general = consultaSEL(cadSQL)
                        If general.Tables(0).Rows.Count > 0 Then
                            objetivo = general.Tables(0).Rows(0)!objetivo
                            reinicio = general.Tables(0).Rows(0)!reinicio
                        End If

                        'Buscar estimados
                        Dim EFI As Double = 100, OEE As Double = 85, FTQ As Double = 100, DIS As Double = 100
                        Dim eEFI As Double = 100, eOEE As Double = 85, eFTQ As Double = 100, eDIS As Double = 100
                        cadSQL = "SELECT * FROM " & rutaBD & ".estimados WHERE (linea = " & captura!linea & " OR linea = 0) AND (equipo = " & captura!maquinaid & " OR equipo = 0) AND (fijo = 'S' OR ('" & Format(fecha, "yyyy/MM/dd") & "' >= desde AND '" & Format(fecha, "yyyy/MM/dd") & "' <= hasta )) ORDER BY equipo DESC, linea DESC, fijo, desde LIMIT 1"
                        general = consultaSEL(cadSQL)
                        If general.Tables(0).Rows.Count > 0 Then
                            eOEE = general.Tables(0).Rows(0)!oee
                            eEFI = general.Tables(0).Rows(0)!efi
                            eFTQ = general.Tables(0).Rows(0)!ftq
                            eDIS = general.Tables(0).Rows(0)!dis
                        End If

                        Dim rateActual As Double = 0
                        Dim ultimaProduccion = ultimo
                        Dim ultimaReparacion = ultimo

                        cadSQL = "SELECT a.equipo,  parosmostrar, tiempo_reinicio, tiempo_corte, transcurrido, transcurrido_pasar, estatus, iniciar, iniciar_1, iniciar_2, iniciar_3, iniciar_4, iniciar_5, iniciar_6, iniciar_7, iniciar_8, detener, detener_piezas, detener_notas, detener_resultados, detener_estimado, detener_area, detener_tipo, detener_paro, reanudar, proximo_paro, ultima_produccion, ultima_reparacion, fecha_desde, desde_rate, produccion, produccion_tc, calidad, calidad_tc, buffer, rate_mal_desde, rate, rate_tendencia_baja, rate_tendencia_alta, rate_efecto, paros, ftq, efi, dis, oee, ftq_tendencia_baja, efi_tendencia_baja, dis_tendencia_baja, oee_tendencia_baja, paro_actual, IFNULL(SUM(piezashr), 0) AS piezashr, IFNULL(SUM(t_paros), 0) AS t_paros FROM " & rutaBD & ".relacion_maquinas_lecturas a LEFT JOIN (SELECT equipo, fecha, produccion AS piezashr, paro AS t_paros FROM " & rutaBD & ".piezasxminuto) AS b On a.equipo = b.equipo AND fecha >= a.desde_rate WHERE a.equipo = " & captura!maquinaid

                        Dim miResumen = consultaSEL(cadSQL)
                        If miResumen.Tables(0).Rows.Count = 0 Then
                            regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".relacion_maquinas_lecturas (equipo, ultima_produccion, ultima_buffer, fecha_desde, desde_rate, estado_desde) VALUES(" & captura!maquinaid & ", '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "')")
                            miResumen = consultaSEL(cadSQL)
                        ElseIf miResumen.Tables(0).Rows(0)!equipo.Equals(System.DBNull.Value) Then
                            regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".relacion_maquinas_lecturas (equipo, ultima_produccion, ultima_buffer, fecha_desde, desde_rate, estado_desde) VALUES(" & captura!maquinaid & ", '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "')")
                            miResumen = consultaSEL(cadSQL)
                        Else
                            If Not miResumen.Tables(0).Rows(0)!ultima_produccion.Equals(System.DBNull.Value) Then
                                ultimaProduccion = miResumen.Tables(0).Rows(0)!ultima_produccion
                            End If
                            If Not miResumen.Tables(0).Rows(0)!ultima_reparacion.Equals(System.DBNull.Value) Then
                                ultimaReparacion = miResumen.Tables(0).Rows(0)!ultima_reparacion
                            End If
                        End If

                        Dim tiempoTranscurrido = 1
                        Dim corteAPP = ultimo

                        If Not miResumen.Tables(0).Rows(0)!fecha_desde.Equals(System.DBNull.Value) Then
                            corteAPP = miResumen.Tables(0).Rows(0)!fecha_desde
                            tiempoTranscurrido = tiempoValido(miResumen.Tables(0).Rows(0)!fecha_desde, ultimo, captura!maquinaid)
                        Else
                            cadAdic = cadAdic & ", fecha_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                        End If

                        Dim cortePT = ultimo

                        If Not miResumen.Tables(0).Rows(0)!desde_rate.Equals(System.DBNull.Value) Then
                            cortePT = miResumen.Tables(0).Rows(0)!desde_rate
                        Else
                            cadAdic = cadAdic & ", desde_rate = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                        End If

                        Dim tiempoTranscurridoRate = 1

                        Dim produccionRate = miResumen.Tables(0).Rows(0)!piezashr
                        Dim paroRate = miResumen.Tables(0).Rows(0)!t_paros

                        Dim horasAtras = -24
                        If captura!oee_historico_rate = 0 Then
                            If cortePT > corteAPP Then
                                corteAPP = cortePT
                                'No se busca en la BD
                            End If
                        ElseIf captura!oee_historico_rate = 1 Then
                            horasAtras = -1
                        ElseIf captura!oee_historico_rate = 2 Then
                            horasAtras = -2
                        ElseIf captura!oee_historico_rate = 3 Then
                            horasAtras = -6
                        ElseIf captura!oee_historico_rate = 4 Then
                            horasAtras = -12
                        End If

                        If captura!oee_historico_rate > 0 Then
                            corteAPP = cortePT
                            If corteAPP < DateAdd(DateInterval.Hour, horasAtras, ultimo) Then
                                corteAPP = DateAdd(DateInterval.Hour, horasAtras, ultimo)
                            End If
                        End If
                        If corteAPP <> cortePT Then
                            cadSQL = "SELECT IFNULL(SUM(produccion), 0) AS piezashr, IFNULL(SUM(paro), 0) AS t_paros FROM " & rutaBD & ".piezasxminuto WHERE fecha >= '" & Format(corteAPP, "yyyy/MM/dd HH:mm:ss") & "' AND equipo = " & captura!maquinaid

                            general = consultaSEL(cadSQL)
                            If general.Tables(0).Rows.Count > 0 Then
                                produccionRate = general.Tables(0).Rows(0)!piezashr
                                paroRate = general.Tables(0).Rows(0)!t_paros
                            End If
                        End If
                        tiempoTranscurridoRate = tiempoValido(corteAPP, ultimo, captura!maquinaid)
                        ''MsgBox("entro 12 ")

                        Dim disponibilidad = tiempoTranscurrido - miResumen.Tables(0).Rows(0)!parosmostrar
                        Dim disponibilidadRate = tiempoTranscurridoRate - paroRate

                        produccion = miResumen.Tables(0).Rows(0)!produccion + produccion + TTotal
                        produccion_tc = miResumen.Tables(0).Rows(0)!produccion_tc + produccion_tc + TTotal

                        If disponibilidad <= 0 Then disponibilidad = 1
                        If disponibilidadRate <= 0 Then disponibilidadRate = 1

                        Dim uRate As Double = 0

                        rateActual = produccionRate / disponibilidadRate

                        If medTiempo = 2 Then
                            rateActual = rateActual * 3600
                        ElseIf medTiempo = 1 Then
                            rateActual = rateActual * 60
                            ' TC = rateEquipo / 60
                        ElseIf medTiempo = 0 Then

                        ElseIf medTiempo = 3 Then
                            rateActual = rateActual * 86400
                        End If

                        If miResumen.Tables(0).Rows(0)!produccion = produccion And paroActual > 0 Then
                            rateActual = miResumen.Tables(0).Rows(0)!rate
                        End If

                        uRate = rateActual

                        cadAdic = cadAdic & ", fuera_programa = '" & IIf(miResumen.Tables(0).Rows(0)!transcurrido = tiempoTranscurrido And miResumen.Tables(0).Rows(0)!transcurrido_pasar = 1, "S", "N") & "'"
                        If miResumen.Tables(0).Rows(0)!produccion < produccion Then
                            cadAdic = cadAdic & ", ultima_produccion = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', alarmado_manual = 'N'"
                            ultimaProduccion = ultimo
                            conProduccion = True
                        End If
                        buffer = miResumen.Tables(0).Rows(0)!buffer + buffer
                        If miResumen.Tables(0).Rows(0)!buffer < buffer Then
                            cadAdic = cadAdic & ", ultima_buffer = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                            ultimaProduccion = ultimo
                            conProduccion = True
                        End If
                        Dim paroCreado = paroActual > 0
                        If (paroActual = 0 Or claseParoActual <> 0) And reporteActual = 0 Then
                            cadSQL = "SELECT a.id, a.fecha, a.area, b.nombre, b.agrupador_2 FROM " & rutaBD & ".reportes a LEFT JOIN " & rutaBD & ".cat_fallas b ON a.falla = b.id WHERE a.maquina = " & captura!maquinaid & " AND a.estatus <= 10 AND a.afecta_oee = 'S' ORDER BY a.id LIMIT 1"
                            general = consultaSEL(cadSQL)
                            If general.Tables(0).Rows.Count > 0 Then

                                'Se finaliza algun paro clase 1 o 2
                                If paroActual > 0 Then
                                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".detalleparos SET estado = 'F', finaliza = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', hasta = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', tiempo = " & tiempoValido(paroDesde, ultimo, captura!maquinaid) & ", finalizo = 1 WHERE id = " & paroActual)
                                    If regsAfectados > 0 Then
                                        paroActual = 0
                                        reporteActual = 0
                                        claseParoActual = 0
                                    End If
                                End If



                                reporteActual = general.Tables(0).Rows(0)!id
                                Dim agrupador = ValNull(general.Tables(0).Rows(0)!agrupador_2, "N")

                                regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".detalleparos (parte, lote, turno, paro, clase, reporte, maquina, fecha, tiempo, inicio, inicia, desde, estado, estatus, area, tipo) VALUES (" & captura!oee_parte_actual & ", " & captura!oee_lote_actual & ", " & config.Tables(0).Rows(0)!turno_oee & ", 'ANDON " & Strings.Left(ValNull(general.Tables(0).Rows(0)!nombre, "A"), 44) & "', 3, " & reporteActual & ", " & captura!maquinaid & ", '" & Format(general.Tables(0).Rows(0)!fecha, "yyyy/MM/dd") & "', 0, 1, '" & Format(general.Tables(0).Rows(0)!fecha, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(general.Tables(0).Rows(0)!fecha, "yyyy/MM/dd HH:mm:ss") & "', 'C', 'A', " & general.Tables(0).Rows(0)!area & ", " & agrupador & ");UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'D', parada_desde  = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', estado_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE equipo = " & captura!maquinaid)
                                paroCreado = regsAfectados > 0
                            End If
                        End If

                        If captura!oee_umbral_produccion > 0 And captura!oee_umbral_produccion <= DateAndTime.DateDiff(DateInterval.Second, ultimaReparacion, ultimo) And Not paroCreado Then

                            regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".detalleparos (parte, lote, turno, paro, clase, maquina, fecha, tiempo, inicio, inicia, desde, estado, estatus) VALUES (" & captura!oee_parte_actual & ", " & captura!oee_lote_actual & ", " & config.Tables(0).Rows(0)!turno_oee & ", 'NO SE DETECTAN DE PIEZAS', 1, " & captura!maquinaid & ", '" & Format(fecha, "yyyy/MM/dd") & "', 0, 1, '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', 'C', 'A');UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'D', parada_desde  = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', estado_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE equipo = " & captura!maquinaid)
                        End If

                        If captura!oee_umbral_produccion > 0 And conProduccion And claseParoActual = 1 Then

                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".detalleparos SET estado = 'F', finaliza = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', tiempo = " & tiempoValido(paroDesde, ultimo, captura!maquinaid) & ", finalizo = 1, hasta = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & paroActual & ";UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'F', parada_desde = NULL, estado_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', ultima_reparacion = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE equipo = " & captura!maquinaid)
                        End If

                        cadSQL = "SELECT id FROM " & rutaBD & ".detalleparos WHERE maquina = " & captura!maquinaid & " And desde > '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' AND estatus = 'A' AND estado = 'L' ORDER BY inicia LIMIT 1"
                            general = consultaSEL(cadSQL)
                        If general.Tables(0).Rows.Count > 0 Then
                            cadAdic = cadAdic & ", proximo_paro = " & general.Tables(0).Rows(0)!id
                        Else
                            cadAdic = cadAdic & ", proximo_paro = 0 "
                        End If

                        Dim rateEfecto = "N"

                        If rateAlto > 0 And uRate >= rateEquipo * (rateAlto / 100) Then
                            rateEfecto = "A"
                        ElseIf rateBajo > 0 And uRate <= rateEquipo * (rateBajo / 100) Then
                            rateEfecto = "B"
                        End If
                        Dim cadFecha = ", rate_mal_desde = NULL"
                        If rateEfecto <> "N" Then
                            If rateEfecto = "N" Then
                                conRateBajo = True
                            End If
                            If miResumen.Tables(0).Rows(0)!rate_efecto <> rateEfecto Or miResumen.Tables(0).Rows(0)!rate_mal_desde.Equals(System.DBNull.Value) Then
                                cadFecha = ", rate_mal_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                            Else
                                cadFecha = ""
                            End If
                        End If

                        If hayTipo3 Then conRateBajo = Not funcionando

                        Dim cadFechaBaja = ", rate_tendencia_baja = NULL"
                        If rateEfecto = "B" Then
                            If uRate <= miResumen.Tables(0).Rows(0)!rate Then
                                If miResumen.Tables(0).Rows(0)!rate_tendencia_baja.Equals(System.DBNull.Value) Then
                                    cadFechaBaja = ", rate_tendencia_baja = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                                Else
                                    cadFechaBaja = ""
                                End If
                            End If
                        End If

                        Dim cadFechaAlta = ", rate_tendencia_alta = NULL"
                        If rateEfecto = "A" Then
                            If uRate >= miResumen.Tables(0).Rows(0)!rate Then
                                If miResumen.Tables(0).Rows(0)!rate_tendencia_alta.Equals(System.DBNull.Value) Then
                                    cadFechaAlta = ", rate_tendencia_alta = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                                Else
                                    cadFechaAlta = ""
                                End If
                            End If
                        End If

                        'Calculos de los indicadores

                        If tiempoTranscurrido = 0 Then
                            DIS = 100
                        Else
                            DIS = (disponibilidad / tiempoTranscurrido * 100)
                        End If
                        If disponibilidad = 0 And produccion_tc = 0 Then
                            EFI = 0
                        ElseIf disponibilidad = 0 Then
                            EFI = 100
                        Else
                            EFI = produccion_tc / disponibilidad * 100
                        End If

                        If EFI > 100 Then
                            EFI = 100
                        End If

                        calidad = calidad + miResumen.Tables(0).Rows(0)!calidad
                        If produccion_tc > 0 Then
                            FTQ = (produccion_tc - calidad_tc) / produccion_tc * 100
                        End If

                        If FTQ < 0 Then
                            FTQ = 0
                        End If
                        OEE = EFI * FTQ * DIS / 10000
                        OEE = Math.Round(OEE, 3)
                        EFI = Math.Round(EFI, 3)
                        DIS = Math.Round(DIS, 3)
                        FTQ = Math.Round(FTQ, 3)

                        'MsgBox("OEE " & OEE)

                        Dim cadFechaFTQ = ", ftq_tendencia_baja = NULL"
                        If FTQ <= miResumen.Tables(0).Rows(0)!ftq And miResumen.Tables(0).Rows(0)!ftq < 100 Then
                            If miResumen.Tables(0).Rows(0)!ftq_tendencia_baja.Equals(System.DBNull.Value) Then
                                cadFechaFTQ = ", ftq_tendencia_baja = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                            Else
                                cadFechaFTQ = ""
                            End If
                        End If

                        If eFTQ > 0 Then
                            cadFechaFTQ = cadFechaFTQ & ", ftq_efecto = '" & IIf(FTQ < eFTQ, "S", "N") & "'"
                        End If

                        Dim cadFechaDIS = ", dis_tendencia_baja = NULL"
                        If DIS <= miResumen.Tables(0).Rows(0)!dis And miResumen.Tables(0).Rows(0)!dis > 100 Then
                            If miResumen.Tables(0).Rows(0)!dis_tendencia_baja.Equals(System.DBNull.Value) Then
                                cadFechaDIS = ", dis_tendencia_baja = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                            Else
                                cadFechaDIS = ""
                            End If
                        End If

                        If eDIS > 0 Then
                            cadFechaFTQ = cadFechaFTQ & ", dis_efecto = '" & IIf(DIS < eDIS, "S", "N") & "'"
                        End If

                        Dim cadFechaEFI = ", efi_tendencia_baja = NULL"
                        If EFI <= miResumen.Tables(0).Rows(0)!efi And miResumen.Tables(0).Rows(0)!efi <> 0 Then
                            If miResumen.Tables(0).Rows(0)!efi_tendencia_baja.Equals(System.DBNull.Value) Then
                                cadFechaEFI = ", efi_tendencia_baja = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                            Else
                                cadFechaEFI = ""
                            End If
                        End If

                        If eEFI > 0 Then
                            cadFechaFTQ = cadFechaFTQ & ", efi_efecto = '" & IIf(EFI < eEFI, "S", "N") & "'"
                        End If

                        Dim cadFechaOEE = ", oee_tendencia_baja = NULL"
                        If OEE <= miResumen.Tables(0).Rows(0)!oee And miResumen.Tables(0).Rows(0)!oee <> 0 Then
                            If miResumen.Tables(0).Rows(0)!oee_tendencia_baja.Equals(System.DBNull.Value) Then
                                cadFechaOEE = ", oee_tendencia_baja = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                            Else
                                cadFechaOEE = ""
                            End If
                        End If

                        If eOEE > 0 Then
                            cadFechaFTQ = cadFechaFTQ & ", oee_efecto = '" & IIf(OEE < eOEE, "S", "N") & "'"
                        End If

                        Dim cadOEE = ", oee_imagen = 0"
                        If OEE < miResumen.Tables(0).Rows(0)!oee And miResumen.Tables(0).Rows(0)!oee <> 0 Then
                            cadOEE = ", oee_imagen = 2"
                        ElseIf OEE > miResumen.Tables(0).Rows(0)!oee And miResumen.Tables(0).Rows(0)!oee <> 0 Then
                            cadOEE = ", oee_imagen = 1"
                        End If



                        If EFI < miResumen.Tables(0).Rows(0)!efi And miResumen.Tables(0).Rows(0)!efi <> 0 Then
                            cadOEE = cadOEE & ", efi_imagen = 2"
                        ElseIf EFI > miResumen.Tables(0).Rows(0)!efi And miResumen.Tables(0).Rows(0)!efi <> 0 Then
                            cadOEE = cadOEE & ", efi_imagen = 1"
                        Else
                            cadOEE = cadOEE & ", efi_imagen = 0"
                        End If


                        If DIS < miResumen.Tables(0).Rows(0)!dis And miResumen.Tables(0).Rows(0)!dis <> 0 Then
                            cadOEE = cadOEE & ", dis_imagen = 2"
                        ElseIf DIS > miResumen.Tables(0).Rows(0)!dis And miResumen.Tables(0).Rows(0)!dis <> 0 Then
                            cadOEE = cadOEE & ", dis_imagen = 1"
                        Else
                            cadOEE = cadOEE & ", dis_imagen = 0"
                        End If


                        If FTQ < miResumen.Tables(0).Rows(0)!ftq And miResumen.Tables(0).Rows(0)!ftq <> 0 Then
                            cadOEE = cadOEE & ", ftq_imagen = 2"
                        ElseIf FTQ > miResumen.Tables(0).Rows(0)!ftq And miResumen.Tables(0).Rows(0)!ftq <> 0 Then
                            cadOEE = cadOEE & ", ftq_imagen = 1"
                        Else
                            cadOEE = cadOEE & ", ftq_imagen = 0"
                        End If

                        Dim cadAdicDetener = ""
                        If miResumen.Tables(0).Rows(0)!detener > 0 And paroActual = 0 Then

                            regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".detalleparos (parte, lote, turno, paro, clase, maquina, fecha, tiempo, inicio, inicia, desde, estado, estatus, notas, finaliza_sensor, tipo, area) VALUES (" & captura!oee_parte_actual & ", " & captura!oee_lote_actual & ", " & config.Tables(0).Rows(0)!turno_oee & ", '" & ValNull(miResumen.Tables(0).Rows(0)!detener_paro, "A") & "', 2, " & captura!maquinaid & ", '" & Format(ultimo, "yyyy/MM/dd") & "', " & ValNull(miResumen.Tables(0).Rows(0)!detener_estimado, "N") & ", " & miResumen.Tables(0).Rows(0)!detener & ", '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', 'C', 'A', '" & ValNull(miResumen.Tables(0).Rows(0)!detener_notas, "A") & "', '" & ValNull(miResumen.Tables(0).Rows(0)!detener_piezas, "A") & "', " & ValNull(miResumen.Tables(0).Rows(0)!detener_tipo, "N") & ", " & ValNull(miResumen.Tables(0).Rows(0)!detener_area, "N") & ");UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'D', estado_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE equipo = " & captura!maquinaid)
                            cadAdicDetener = ", reanudar = 0, detener = 0, detener_piezas = 'N', detener_notas = NULL, detener_estimado = 0, detener_area = 0, detener_tipo = 0, detener_paro = NULL "
                        ElseIf miResumen.Tables(0).Rows(0)!reanudar > 0 And paroActual > 0 Then

                            tiempoParo = tiempoParo + tiempoValido(paroCorte, ultimo, captura!maquinaid)

                            Dim cadDatos = "UPDATE " & rutaBD & ".detalleparos SET estado = 'F', tiempo = " & tiempoValido(paroDesde, ultimo, captura!maquinaid) & ", finaliza = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', tipo = " & ValNull(miResumen.Tables(0).Rows(0)!detener_tipo, "N") & ", notas = '" & ValNull(miResumen.Tables(0).Rows(0)!detener_notas, "A") & "', area = " & ValNull(miResumen.Tables(0).Rows(0)!detener_area, "N") & ", paro = '" & ValNull(miResumen.Tables(0).Rows(0)!detener_paro, "A") & "', resultados = '" & ValNull(miResumen.Tables(0).Rows(0)!detener_resultados, "A") & "' WHERE id = " & miResumen.Tables(0).Rows(0)!paro_actual
                            If claseParoActual <> 2 Then
                                cadDatos = "UPDATE " & rutaBD & ".detalleparos SET estado = 'F', tiempo = " & tiempoValido(paroDesde, ultimo, captura!maquinaid) & ", finaliza = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & miResumen.Tables(0).Rows(0)!paro_actual
                            End If

                            regsAfectados = consultaACT(cadDatos & ";UPDATE " & rutaBD & ".detalleparos SET hasta = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & miResumen.Tables(0).Rows(0)!paro_actual & " AND clase <> 0;UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'F', estado_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', alarmado_manual = 'N', ultima_reparacion = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE equipo = " & captura!maquinaid)

                            paroActual = 0
                            reporteActual = 0
                            claseParoActual = 0
                            ultimaReparacion = ultimo
                            cadAdicDetener = ", reanudar = 0, detener = 0, detener_piezas = 'N', detener_notas = NULL, detener_estimado = 0, detener_area = 0, detener_tipo = 0, detener_paro = NULL "
                        End If

                        cadAdic = cadAdic & cadFecha & cadFechaBaja & cadFechaAlta & cadFechaFTQ & cadFechaDIS & cadFechaEFI & cadFechaOEE & cadOEE & cadAdicDetener

                        Dim otraCad As String = ""
                        Dim min = Format(ultimo, "mm")
                        If (min = 0 And miResumen.Tables(0).Rows(0)!tiempo_reinicio <> min) Then
                            otraCad = ", tiempo_reinicio = " & min

                            If Format(ultimo, "HHmm") = "0000" And reinicio = 5 Then
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET iniciar_5 = 'S', factor_iniciar = '" & Format(ultimo, "ddMMyy") & "' WHERE equipo = " & captura!maquinaid & " AND (factor_iniciar <> '" & Format(ultimo, "ddMMyy") & "' OR ISNULL(factor_iniciar))")
                            End If
                            If DateAndTime.Weekday(ultimo) = 2 And reinicio = 6 Then
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET iniciar_6 = 'S', factor_iniciar = '" & Format(ultimo, "ddMMyy") & "' WHERE equipo = " & captura!maquinaid & " AND (factor_iniciar <> '" & Format(ultimo, "ddMMyy") & "' OR ISNULL(factor_iniciar))")
                            End If
                            If Format(ultimo, "dd") = "01" And reinicio = 7 Then
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET iniciar_7 = 'S', factor_iniciar = '" & Format(ultimo, "ddMMyy") & "' WHERE equipo = " & captura!maquinaid & " AND (factor_iniciar <> '" & Format(ultimo, "ddMMyy") & "' OR ISNULL(factor_iniciar))")
                            End If
                            If Format(ultimo, "ddMM") = "0101" And reinicio = 8 Then
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET iniciar_8 = 'S', factor_iniciar = '" & Format(ultimo, "ddMMyy") & "' WHERE equipo = " & captura!maquinaid & " AND (factor_iniciar <> '" & Format(ultimo, "ddMMyy") & "' OR ISNULL(factor_iniciar))")
                            End If
                            If Format(ultimo, "mm") = "00" And reinicio = 3 Then
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET iniciar_3 = 'S', factor_iniciar = '" & Format(ultimo, "HHmm") & "' WHERE equipo = " & captura!maquinaid & " AND (factor_iniciar <> '" & Format(ultimo, "HHmm") & "' OR ISNULL(factor_iniciar))")
                            End If

                        End If

                        If hayTipo3 Then
                            uRate = -100
                            rateActual = -100
                            rateEquipo = -100
                            objetivo = 0
                        End If

                        cadSQL = "UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET ultima_lectura = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', parte = " & captura!oee_parte_actual & ", tripulacion = " & captura!oee_tripulacion_actual & ", turno = " & captura!oee_turno_actual & ", produccion = " & produccion & ", produccion_tc = " & produccion_tc & ", calidad = " & calidad & ", calidad_tc = " & calidad_tc & ", buffer = buffer + " & buffer & ", norden = '" & captura!norden & "', nparte = '" & captura!nparte & "', nequipo = '" & captura!nequipo & "', ntripulacion = '" & captura!ntripulacion & "', referencia = '" & captura!referencia & "', nturno = '" & captura!nturno & "', rate_teorico = " & rateEquipo & ", rate_min = " & rateBajo & ", rate_max = " & rateAlto & ", objetivo = " & objetivo & ", rate = " & rateActual & cadAdic & ", rate_efecto = '" & rateEfecto & "', ultimo_rate = " & uRate & ", ratemed = '" & rateUnidad & "', esperadodis = " & eDIS & ", esperadoftq = " & eFTQ & ", esperadoefi = " & eEFI & ", esperadooee = " & eOEE & ", hoyos = '" & IIf(incluyeHoyos, "S", "N") & "', efi = " & EFI & ", dis = " & DIS & ", ftq = " & FTQ & ", oee = " & OEE & otraCad & ", paro_actual = " & paroActual & ", parosmostrar = parosmostrar + " & tiempoParo & cadPlaneado & " WHERE equipo = " & captura!maquinaid

                        'MsgBox("Guardado todo!")

                        If miResumen.Tables(0).Rows(0)!iniciar = "S" Or (miResumen.Tables(0).Rows(0)!iniciar_1 = "S" And reinicio = 1) _
                        Or (miResumen.Tables(0).Rows(0)!iniciar_2 = "S" And reinicio = 2) _
                        Or (miResumen.Tables(0).Rows(0)!iniciar_3 = "S" And reinicio = 3) _
                        Or (miResumen.Tables(0).Rows(0)!iniciar_4 = "S" And reinicio = 4) _
                        Or (miResumen.Tables(0).Rows(0)!iniciar_5 = "S" And reinicio = 5) _
                        Or (miResumen.Tables(0).Rows(0)!iniciar_6 = "S" And reinicio = 6) _
                        Or (miResumen.Tables(0).Rows(0)!iniciar_7 = "S" And reinicio = 7) _
                        Or (miResumen.Tables(0).Rows(0)!iniciar_8 = "S" And reinicio = 8) Then
                            cadSQL = cadSQL & ";UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET produccion = 0, calidad = 0, produccion_tc = 0, calidad_tc = 0, buffer = 0, parosmostrar = 0, rate_mal_desde = NULL, rate_tendencia_baja = NULL, rate_tendencia_alta = NULL, ftq_tendencia_baja = NULL, oee_tendencia_baja = NULL, dis_tendencia_baja = NULL, efi_tendencia_baja = NULL, parada_desde = NULL, paro_actual = 0, ultima_reparacion = NULL, ultima_produccion = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', alarmado_manual = 'N', ultima_buffer = NULL, iniciar = 'N', iniciar_1 = 'N', iniciar_2 = 'N', iniciar_3 = 'N', iniciar_4 = 'N', iniciar_5 = 'N', iniciar_6 = 'N', iniciar_7 = 'N', iniciar_8 = 'N', fecha_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', planeado = 'N', estado_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'" & IIf(ValNull(captura!oee_historico_rate_reiniciar, "A") = "1", ", desde_rate = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'", "") & " WHERE equipo = " & captura!maquinaid
                        End If

                        regsAfectados = consultaACT(cadSQL & ";" & "UPDATE " & rutaBD & ".relacion_procesos_sensores SET ultima_lectura = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & captura!id & cadSQLParo)
                        Dim corte

                        Dim minutoEste = Format(ultimo, "mm")
                        Dim buscarTipo = False

                        cadSQL = "SELECT id FROM " & rutaBD & ".lecturas_resumen WHERE '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' >= desde AND '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' <= hasta  AND equipo = " & captura!maquinaid
                        corte = consultaSEL(cadSQL)
                        If corte.Tables(0).Rows.Count = 0 Then
                            'Se crean los 4 registros
                            buscarTipo = True
                            Dim minuto = 0
                            For i = 1 To 4
                                regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".lecturas_resumen (equipo, desde, hasta, hora) VALUES (" & captura!maquinaid & ", '" & Format(ultimo, "yyyy/MM/dd HH:") & Format(minuto, "00") & ":00', '" & Format(ultimo, "yyyy/MM/dd HH:") & Format(minuto + 14, "00") & ":59', " & Format(ultimo, "HH") & ")")
                                minuto = minuto + 15
                            Next
                            Dim ultimo2 = ultimo
                            ultimo2 = Convert.ToDateTime(Format(ultimo2, "yyyy/MM/dd HH") & ":00:00")
                            For i = 1 To 88
                                ultimo2 = DateAndTime.DateAdd(DateInterval.Minute, -15, ultimo2)
                                Dim cadSQLINI = "SELECT id FROM " & rutaBD & ".lecturas_resumen WHERE desde = '" & Format(ultimo2, "yyyy/MM/dd HH:mm") & ":00' AND equipo = " & captura!maquinaid
                                corte = consultaSEL(cadSQLINI)
                                If corte.Tables(0).Rows.Count = 0 Then
                                    Dim fechaHasta = DateAndTime.DateAdd(DateInterval.Second, 899, ultimo2)
                                    regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".lecturas_resumen (equipo, desde, hasta, hora) VALUES (" & captura!maquinaid & ", '" & Format(ultimo2, "yyyy/MM/dd HH:mm") & ":00', '" & Format(fechaHasta, "yyyy/MM/dd HH:mm:ss") & "', " & Format(fechaHasta, "HH") & ")")

                                End If
                            Next
                            'Se recalcula el pasado
                            corte = consultaSEL(cadSQL)
                            'MsgBox("REsumenes creados")

                        End If
                        If (minutoEste Mod 5 = 0 And miResumen.Tables(0).Rows(0)!tiempo_corte <> minutoEste) Or buscarTipo Then

                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET tiempo_corte = " & minutoEste & " WHERE equipo = " & captura!maquinaid)
                            Dim cadenaTipo = "SELECT * FROM " & rutaBD & ".lecturas_resumen WHERE equipo = " & captura!maquinaid & " ORDER BY desde DESC LIMIT 8"
                            Dim calcularTipo = consultaSEL(cadenaTipo)
                            If calcularTipo.Tables(0).Rows.Count > 0 Then
                                For Each tipoColor In calcularTipo.Tables(0).Rows
                                    If tipoColor!produccion > 0 And tipoColor!produccion >= tipoColor!bajorate And tipoColor!produccion >= tipoColor!paro Then
                                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".lecturas_resumen SET tipo = 1 WHERE id = " & tipoColor!id)
                                    ElseIf tipoColor!bajorate > 0 And tipoColor!bajorate >= tipoColor!paro And tipoColor!bajorate >= tipoColor!produccion And tipoColor!bajorate >= tipoColor!sinplan Then
                                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".lecturas_resumen SET tipo = 2 WHERE id = " & tipoColor!id)
                                    ElseIf tipoColor!paro > 0 And tipoColor!paro >= tipoColor!bajorate And tipoColor!paro >= tipoColor!produccion And tipoColor!paro >= tipoColor!sinplan Then
                                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".lecturas_resumen SET tipo = 3 WHERE id = " & tipoColor!id)
                                    End If
                                Next
                            End If
                        End If
                        cadAdic = ""
                        If paroActual > 0 Then
                            cadAdic = "paro = paro + 1"
                        ElseIf conRateBajo Then
                            cadAdic = "bajorate = bajorate + 1"
                        ElseIf conProduccion Then
                            cadAdic = "produccion = produccion + 1"
                        ElseIf miResumen.Tables(0).Rows(0)!transcurrido = tiempoTranscurrido And miResumen.Tables(0).Rows(0)!transcurrido_pasar = 1 Then
                            cadAdic = "sinplan = sinplan + 1"
                        Else
                            cadAdic = "produccion = produccion + 1"
                        End If
                        If miResumen.Tables(0).Rows(0)!transcurrido <> tiempoTranscurrido Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET transcurrido = " & tiempoTranscurrido & ", transcurrido_pasar = 0 WHERE equipo = " & captura!maquinaid)
                        Else
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET transcurrido_pasar = 1 WHERE equipo = " & captura!maquinaid)
                        End If
                        Dim cadSeg = ""
                        Dim cadSeg2 = ""
                        cadSeg = "DELETE FROM " & rutaBD & ".piezasxminuto WHERE equipo = " & captura!maquinaid & " And fecha <= DATE_ADD('" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', INTERVAL -1 DAY);"
                        If produccion_seg > 0 Or tiempoParo > 0 Then
                            Dim hhdmm = Format(ultimo, "HHmm")
                            general = consultaSEL("SELECT hhmm FROM " & rutaBD & ".piezasxminuto WHERE equipo = " & captura!maquinaid & " AND hhmm = '" & hhdmm & "'")
                            cadSeg2 = "INSERT INTO " & rutaBD & ".piezasxminuto VALUES(" & captura!maquinaid & ", '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', " & produccion_seg & ", " & tiempoParo & ", '" & hhdmm & "');"

                            If general.Tables(0).Rows.Count > 0 Then
                                cadSeg2 = "UPDATE " & rutaBD & ".piezasxminuto SET produccion = produccion + " & produccion_seg & ", paro = paro + " & tiempoParo & " WHERE equipo = " & captura!maquinaid & " AND hhmm = '" & hhdmm & "';"
                            End If

                        End If

                        If tiempoParo > 0 And paroActual > 0 Then
                            cadSeg = cadSeg & "UPDATE " & rutaBD & ".detalleparos SET corte = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & paroActual & ";"
                        End If
                        regsAfectados = consultaACT(cadSeg & cadSeg2 & "UPDATE " & rutaBD & ".lecturas_resumen SET " & cadAdic & "  WHERE id = " & corte.Tables(0).Rows(0)!id)
                    End If
                    TTotal = 0
                Next
                regsAfectados = consultaACT("UPDATE " & rutaBD & ".lecturas SET estatus = 2 WHERE estatus = 1")
                'MsgBox("FIN")
            End If

            regsAfectados = consultaACT("UPDATE " & rutaBD & ".configuracion Set lectura_pendiente = 0")
        End If
        revisandoSensores = False
    End Sub



    Private Sub cambioTurno_Tick(sender As Object, e As EventArgs) Handles cambioTurno.Tick
        Dim regsAfectados As Long = 0
        Dim cadSQL As String = ""
        Dim ultimoHora As String = Format(Now, "HH:mm:ss")
        Dim nombreTurno As String = ""

        cadSQL = "SELECT turno_oee FROM " & rutaBD & ".configuracion WHERE modulo_oee = 'S'"
        Dim config As DataSet = consultaSEL(cadSQL)
        If config.Tables(0).Rows.Count > 0 Then
            miTurno = -1
            cadSQL = "SELECT id, inicia, termina, nombre FROM " & rutaBD & ".cat_turnos WHERE estatus = 'A' AND (inicia <= '" + ultimoHora + "' OR termina >= '" + ultimoHora + "') AND id <> " & config.Tables(0).Rows(0)!turno_oee & " ORDER BY inicia, termina"
            Dim horarios As DataSet = consultaSEL(cadSQL)
            If horarios.Tables(0).Rows.Count > 0 Then
                For Each horario In horarios.Tables(0).Rows
                    If ultimoHora >= horario!inicia.ToString And ultimoHora <= horario!termina.ToString Then
                        miTurno = horario!id
                        nombreTurno = ValNull(horario!nombre, "A")
                        Exit For
                    End If
                Next
                If miTurno = -1 Then
                    For Each horario In horarios.Tables(0).Rows
                        If (ultimoHora >= horario!inicia.ToString Or ultimoHora <= horario!termina.ToString) And horario!termina.ToString < horario!inicia.ToString Then
                            miTurno = horario!id
                            nombreTurno = ValNull(horario!nombre, "A")
                            Exit For
                        End If
                    Next

                End If

            End If
            If miTurno > 0 Then
                regsAfectados = consultaACT("UPDATE " & rutaBD & ".cat_maquinas SET oee_turno_actual = " & miTurno & " WHERE oee = 'S';UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET iniciar_4 = 'S', turno = " & miTurno & ", nturno = '" & nombreTurno & "';UPDATE " & rutaBD & ".configuracion SET turno_oee = " & miTurno)
                agregarSolo("Cambio de turno: " & nombreTurno)
            End If
        End If
    End Sub

    Sub crearMensajes()
        Dim idProceso = Process.GetCurrentProcess.Id
        Dim mensajesDS As DataSet
        Dim registroDS As DataSet
        Dim eMensaje = ""
        Dim laLinea As String = ""
        Dim laMaquina As String = ""
        Dim laArea As String = ""
        Dim laFalla As String = ""
        Dim fecha
        Dim tiempo As String = ""
        Dim cadSQL As String = ""
        Dim nroReporte As Integer = 0
        Dim eTitulo = ""

        'Escalada 4
        Dim miError As String = ""
        Dim optimizar As Boolean = False
        Dim mantenerPrioridad As Boolean = False
        Dim regsAfectados = 0

        Dim maximo_largo_mmcall As Integer = 40



        regsAfectados = consultaACT("UPDATE " & rutaBD & ".mensajes SET estatus = '" & idProceso & "' WHERE estatus = 'A'")
        cadSQL = "SELECT a.id, a.canal, b.evento, a.prioridad FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".cat_alertas b on a.alerta = b.id WHERE a.estatus = '" & idProceso & "' ORDER BY a.prioridad DESC, a.id"
        'Se preselecciona la voz
        mensajesDS = consultaSEL(cadSQL)
        Dim generarMensaje As Boolean
        If mensajesDS.Tables(0).Rows.Count > 0 Then
            For Each elmensaje In mensajesDS.Tables(0).Rows
                generarMensaje = False
                eMensaje = ""
                laLinea = ""
                laMaquina = ""
                laArea = ""
                laFalla = ""
                tiempo = ""
                generarMensaje = True
                nroReporte = 0

                If elmensaje!evento < 200 Then
                    cadSQL = "SELECT a.*, 0 AS rate, 0 AS oee, e.nombre as nlinea, f.nombre as nmaquina, g.nombre as narea, h.nombre as nfalla, c.id AS idalerta, c.acumular, c.mensaje_mmcall, c.mensaje, c.titulo, c.resolucion_mensaje, c.cancelacion_mensaje, d.fecha, d.inicio_atencion, d.inicio_reporte, d.estatus, i.repeticiones, i.fase, i.escalamientos1, i.escalamientos2, i.escalamientos3, i.escalamientos4, i.escalamientos5 FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".cat_distribucion b ON a.lista = b.id AND b.estatus = 'A' INNER JOIN " & rutaBD & ".cat_alertas c ON a.alerta = c.id INNER JOIN " & rutaBD & ".alarmas i ON a.alarma = i.id LEFT JOIN " & rutaBD & ".reportes d ON a.proceso = d.id LEFT JOIN " & rutaBD & ".cat_lineas e ON d.linea = e.id LEFT JOIN " & rutaBD & ".cat_maquinas f ON d.maquina = f.id LEFT JOIN " & rutaBD & ".cat_areas g ON d.area = g.id LEFT JOIN " & rutaBD & ".cat_fallas h ON d.falla = h.id WHERE a.id = " & elmensaje!id
                ElseIf elmensaje!evento < 300 Then
                    cadSQL = "SELECT a.*, IF(d.rate_teorico > 0, d.rate / d.rate_teorico * 100, 0) AS rate, d.oee, e.nombre as nlinea, f.nombre as nmaquina, '' as narea, '' as nfalla, c.id AS idalerta, c.acumular, c.mensaje, c.titulo, c.mensaje_mmcall, c.resolucion_mensaje, c.cancelacion_mensaje, d.rate_tendencia_baja AS fecha, d.rate_tendencia_alta AS inicio_atencion, d.ultima_produccion AS inicio_reporte, d.estatus, i.repeticiones, i.fase, i.escalamientos1, i.escalamientos2, i.escalamientos3, i.escalamientos4, i.escalamientos5 FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".cat_distribucion b ON a.lista = b.id AND b.estatus = 'A' INNER JOIN " & rutaBD & ".cat_alertas c ON a.alerta = c.id INNER JOIN " & rutaBD & ".alarmas i ON a.alarma = i.id LEFT JOIN " & rutaBD & ".relacion_maquinas_lecturas d ON a.proceso = d.equipo LEFT JOIN " & rutaBD & ".cat_maquinas f ON d.equipo = f.id LEFT JOIN " & rutaBD & ".cat_lineas e ON f.linea = e.id WHERE a.id = " & elmensaje!id
                ElseIf elmensaje!evento = 301 Then
                    cadSQL = "SELECT a.*, e1.referencia, e1.nombre AS producto, b1.numero AS nlote, IFNULL((SELECT MIN(orden) FROM " & rutaBD & ".prioridades WHERE parte = b1.parte AND fecha >= NOW() AND estatus = 'A'), 100) AS prioridad, d.ruta_secuencia, d.ruta_secuencia_antes, IFNULL(c1.nombre, 'N/A') AS ruta_antes, IFNULL(d1.nombre, 'N/A') AS ruta_despues, c.id AS idalerta, c.acumular, c.mensaje_mmcall, c.mensaje, c.titulo, c.resolucion_mensaje, c.cancelacion_mensaje, i.inicio AS fecha, NOW() AS inicio_atencion, NOW() AS inicio_reporte, i.repeticiones, i.fase, i.escalamientos1, i.escalamientos2, i.escalamientos3, i.escalamientos4, i.escalamientos5 FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".cat_distribucion b ON a.lista = b.id AND b.estatus = 'A' INNER JOIN " & rutaBD & ".cat_alertas c ON a.alerta = c.id INNER JOIN " & rutaBD & ".alarmas i ON a.alarma = i.id LEFT JOIN " & rutaBD & ".lotes_historia d ON a.proceso = d.id INNER JOIN " & rutaBD & ".lotes b1 ON d.lote = b1.id LEFT JOIN " & rutaBD & ".det_rutas c1 ON d.ruta_detalle_anterior = c1.id LEFT JOIN " & rutaBD & ".det_rutas d1 ON d.ruta_detalle = d1.id LEFT JOIN " & rutaBD & ".cat_partes e1 ON b1.parte = e1.id WHERE a.id = " & elmensaje!id
                ElseIf elmensaje!evento = 302 Or elmensaje!evento = 303 Or elmensaje!evento = 305 Or elmensaje!evento = 306 Then
                    cadSQL = "SELECT a.*, d.hasta, d.numero AS nlote, d.fecha, TIME_TO_SEC(TIMEDIFF(d.hasta, NOW())) AS previo, d.ruta_secuencia, c1.referencia, c1.nombre AS producto, IFNULL(b1.nombre, 'N/A') AS ruta_actual, IFNULL(e1.nombre, 'N/A') as equipo, IFNULL(d1.nombre, 'N/A') as nproceso, c.id AS idalerta, c.acumular, c.mensaje_mmcall, c.mensaje, c.titulo, c.resolucion_mensaje, c.cancelacion_mensaje, i.inicio AS inicio_atencion, NOW() AS inicio_reporte, d.estatus, i.repeticiones, i.fase, i.escalamientos1, i.escalamientos2, i.escalamientos3, i.escalamientos4, i.escalamientos5 FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".cat_distribucion b ON a.lista = b.id AND b.estatus = 'A' INNER JOIN " & rutaBD & ".cat_alertas c ON a.alerta = c.id INNER JOIN " & rutaBD & ".alarmas i ON a.alarma = i.id LEFT JOIN " & rutaBD & ".lotes d ON a.proceso = d.id LEFT JOIN " & rutaBD & ".det_rutas b1 ON d.ruta_detalle = b1.id LEFT JOIN " & rutaBD & ".cat_partes c1 ON d.parte = c1.id LEFT JOIN " & rutaBD & ".cat_procesos d1 ON d.proceso = d1.id LEFT JOIN " & rutaBD & ".cat_maquinas e1 ON d.equipo= e1.id WHERE a.id = " & elmensaje!id
                ElseIf elmensaje!evento = 304 Or elmensaje!evento = 307 Then
                    cadSQL = "SELECT a.*, 0 AS previo, d.carga, d.alarma, d.alarma_rep, d.fecha, d.permitir_reprogramacion, d.equipo, d.fecha, IFNULL(b1.nombre, 'N/A') as nequipo, IFNULL(c1.nombre, 'N/A') as nproceso, IFNULL((SELECT SUM(cantidad) FROM " & rutaBD & ".programacion WHERE carga = d.id AND estatus = 'A'), 0) AS piezas, (SELECT COUNT(*) FROM " & rutaBD & ".lotes WHERE carga = d.id) AS avance, c.id AS idalerta, c.acumular, c.mensaje_mmcall, c.mensaje, c.titulo, c.resolucion_mensaje, c.cancelacion_mensaje, i.inicio AS inicio_atencion, NOW() AS inicio_reporte, d.estatus, i.repeticiones, i.fase, i.escalamientos1, i.escalamientos2, i.escalamientos3, i.escalamientos4, i.escalamientos5 FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".cat_distribucion b ON a.lista = b.id AND b.estatus = 'A' INNER JOIN " & rutaBD & ".cat_alertas c ON a.alerta = c.id INNER JOIN " & rutaBD & ".alarmas i ON a.alarma = i.id LEFT JOIN " & rutaBD & ".cargas d ON a.proceso = d.id LEFT JOIN " & rutaBD & ".cat_maquinas b1 ON d.equipo = b1.id AND b1.estatus = 'A' LEFT JOIN " & rutaBD & ".cat_procesos c1 ON b1.proceso = c1.id AND c1.estatus = 'A' WHERE a.id = " & elmensaje!id
                End If

                registroDS = consultaSEL(cadSQL)
                If registroDS.Tables(0).Rows.Count > 0 Then
                    nroReporte = registroDS.Tables(0).Rows(0)!proceso
                    If elmensaje!evento < 300 Then
                        laLinea = ValNull(registroDS.Tables(0).Rows(0)!nlinea, "A")
                        laMaquina = ValNull(registroDS.Tables(0).Rows(0)!nmaquina, "A")
                        laArea = ValNull(registroDS.Tables(0).Rows(0)!narea, "A")
                        laFalla = ValNull(registroDS.Tables(0).Rows(0)!nfalla, "A")
                    Else
                        laLinea = ""
                        laMaquina = ""
                        laArea = ""
                        laFalla = ""
                    End If

                    If elmensaje!evento = 101 Or elmensaje!evento = 201 Or elmensaje!evento = 301 Then
                        fecha = registroDS.Tables(0).Rows(0)!fecha
                    ElseIf elmensaje!evento = 102 Or elmensaje!evento = 202 Or elmensaje!evento > 300 Then
                        fecha = registroDS.Tables(0).Rows(0)!inicio_atencion
                    ElseIf elmensaje!evento = 103 Or elmensaje!evento = 203 Then
                        fecha = registroDS.Tables(0).Rows(0)!inicio_reporte
                    End If
                    tiempo = calcularTiempoCad(DateAndTime.DateDiff(DateInterval.Second, fecha, Now))
                    Dim mIFormato = "dd-MMM-yyyy HH:mm:ss"

                    If registroDS.Tables(0).Rows(0)!tipo = 8 Then
                        eMensaje = ValNull(registroDS.Tables(0).Rows(0)!resolucion_mensaje, "A")
                    ElseIf registroDS.Tables(0).Rows(0)!tipo = 7 Then
                        eMensaje = ValNull(registroDS.Tables(0).Rows(0)!cancelacion_mensaje, "A")
                    Else
                        If elmensaje!canal = 3 Then
                            mIFormato = "dd/MM HH:mm"
                            eMensaje = ValNull(registroDS.Tables(0).Rows(0)!mensaje_mmcall, "A")
                        Else
                            eMensaje = ValNull(registroDS.Tables(0).Rows(0)!mensaje, "A")
                            End If

                        End If

                        If eMensaje.Length > 0 Then
                        eMensaje = Replace(eMensaje, "[0]", nroReporte)
                        eMensaje = Replace(eMensaje, "[1]", laLinea)
                        eMensaje = Replace(eMensaje, "[2]", laMaquina)
                        eMensaje = Replace(eMensaje, "[3]", laArea)
                        eMensaje = Replace(eMensaje, "[4]", laFalla)
                        eMensaje = Replace(eMensaje, "[5]", Format(fecha, mIFormato))
                        eMensaje = Replace(eMensaje, "[11]", tiempo)
                        If elmensaje!evento < 300 Then
                            eMensaje = Replace(eMensaje, "[12]", Format(registroDS.Tables(0).Rows(0)!rate, "0.0"))
                            eMensaje = Replace(eMensaje, "[13]", Format(registroDS.Tables(0).Rows(0)!oee, "0.0"))

                        End If
                        If ValNull(registroDS.Tables(0).Rows(0)!repeticiones, "N") > 0 Then
                            eMensaje = Replace(eMensaje, "[20]", "R" & ValNull(registroDS.Tables(0).Rows(0)!repeticiones, "N"))
                        Else
                            eMensaje = Replace(eMensaje, "[20]", "")
                        End If
                        If ValNull(registroDS.Tables(0).Rows(0)!fase - 10, "N") > 0 Then
                            Dim escala = ValNull(registroDS.Tables(0).Rows(0)!fase, "N") - 10
                            eMensaje = Replace(eMensaje, "[30]", "Escalado al Nivel " & If(escala > 0, escala, 0))
                        Else
                            eMensaje = Replace(eMensaje, "[30]", "")
                        End If
                        If ValNull(registroDS.Tables(0).Rows(0)!escalamientos1, "N") > 0 Then
                            eMensaje = Replace(eMensaje, "[31]", "R" & ValNull(registroDS.Tables(0).Rows(0)!escalamientos1, "N"))
                        Else
                            eMensaje = Replace(eMensaje, "[31]", "")
                        End If
                        If ValNull(registroDS.Tables(0).Rows(0)!escalamientos2, "N") > 0 Then
                            eMensaje = Replace(eMensaje, "[32]", "R" & ValNull(registroDS.Tables(0).Rows(0)!escalamientos2, "N"))
                        Else
                            eMensaje = Replace(eMensaje, "[32]", "")
                        End If
                        If ValNull(registroDS.Tables(0).Rows(0)!escalamientos3, "N") > 0 Then
                            eMensaje = Replace(eMensaje, "[33]", "R" & ValNull(registroDS.Tables(0).Rows(0)!escalamientos3, "N"))
                        Else
                            eMensaje = Replace(eMensaje, "[33]", "")
                        End If
                        If ValNull(registroDS.Tables(0).Rows(0)!escalamientos4, "N") > 0 Then
                            eMensaje = Replace(eMensaje, "[34]", "R" & ValNull(registroDS.Tables(0).Rows(0)!escalamientos4, "N"))
                        Else
                            eMensaje = Replace(eMensaje, "[34]", "")
                        End If
                        If ValNull(registroDS.Tables(0).Rows(0)!escalamientos5, "N") > 0 Then
                            eMensaje = Replace(eMensaje, "[35]", "R" & ValNull(registroDS.Tables(0).Rows(0)!escalamientos5, "N"))
                        Else
                            eMensaje = Replace(eMensaje, "[35]", "")
                        End If

                        If elmensaje!evento = 301 Then
                            eMensaje = Replace(eMensaje, "[41]", ValNull(registroDS.Tables(0).Rows(0)!nlote, "A"))
                            eMensaje = Replace(eMensaje, "[42]", ValNull(registroDS.Tables(0).Rows(0)!referencia, "A"))
                            eMensaje = Replace(eMensaje, "[43]", ValNull(registroDS.Tables(0).Rows(0)!producto, "A"))
                            eMensaje = Replace(eMensaje, "[70]", ValNull(registroDS.Tables(0).Rows(0)!ruta_antes, "A"))
                            eMensaje = Replace(eMensaje, "[71]", ValNull(registroDS.Tables(0).Rows(0)!ruta_secuencia_antes, "A"))
                            eMensaje = Replace(eMensaje, "[72]", ValNull(registroDS.Tables(0).Rows(0)!ruta_despues, "A"))
                            eMensaje = Replace(eMensaje, "[73]", ValNull(registroDS.Tables(0).Rows(0)!ruta_secuencia, "A"))

                        ElseIf elmensaje!evento = 302 Or elmensaje!evento = 305 Then
                            eMensaje = Replace(eMensaje, "[41]", ValNull(registroDS.Tables(0).Rows(0)!nlote, "A"))
                            eMensaje = Replace(eMensaje, "[42]", ValNull(registroDS.Tables(0).Rows(0)!referencia, "A"))
                            eMensaje = Replace(eMensaje, "[43]", ValNull(registroDS.Tables(0).Rows(0)!producto, "A"))
                            eMensaje = Replace(eMensaje, "[40]", ValNull(registroDS.Tables(0).Rows(0)!nproceso, "A"))
                            eMensaje = Replace(eMensaje, "[44]", ValNull(registroDS.Tables(0).Rows(0)!ruta_actual, "A"))
                            eMensaje = Replace(eMensaje, "[45]", ValNull(registroDS.Tables(0).Rows(0)!ruta_secuencia, "A"))
                            eMensaje = Replace(eMensaje, "[50]", Format(registroDS.Tables(0).Rows(0)!fecha, mIFormato))
                            eMensaje = Replace(eMensaje, "[51]", Format(registroDS.Tables(0).Rows(0)!hasta, mIFormato))
                            eMensaje = Replace(eMensaje, "[52]", calcularTiempoCad(DateAndTime.DateDiff(DateInterval.Second, registroDS.Tables(0).Rows(0)!hasta, DateAndTime.Now)))
                            eMensaje = Replace(eMensaje, "[83]", calcularTiempoCad(registroDS.Tables(0).Rows(0)!previo))
                        ElseIf elmensaje!evento = 303 Or elmensaje!evento = 306 Then
                            eMensaje = Replace(eMensaje, "[41]", ValNull(registroDS.Tables(0).Rows(0)!nlote, "A"))
                            eMensaje = Replace(eMensaje, "[42]", ValNull(registroDS.Tables(0).Rows(0)!referencia, "A"))
                            eMensaje = Replace(eMensaje, "[43]", ValNull(registroDS.Tables(0).Rows(0)!producto, "A"))
                            eMensaje = Replace(eMensaje, "[40]", ValNull(registroDS.Tables(0).Rows(0)!nproceso, "A"))
                            eMensaje = Replace(eMensaje, "[44]", ValNull(registroDS.Tables(0).Rows(0)!ruta_actual, "A"))
                            eMensaje = Replace(eMensaje, "[45]", ValNull(registroDS.Tables(0).Rows(0)!ruta_secuencia, "A"))
                            eMensaje = Replace(eMensaje, "[61]", ValNull(registroDS.Tables(0).Rows(0)!equipo, "A"))
                            eMensaje = Replace(eMensaje, "[62]", Format(registroDS.Tables(0).Rows(0)!fecha, mIFormato))
                            eMensaje = Replace(eMensaje, "[63]", Format(registroDS.Tables(0).Rows(0)!hasta, mIFormato))
                            eMensaje = Replace(eMensaje, "[64]", calcularTiempoCad(DateAndTime.DateDiff(DateInterval.Second, registroDS.Tables(0).Rows(0)!hasta, DateAndTime.Now)))
                            eMensaje = Replace(eMensaje, "[83]", calcularTiempoCad(registroDS.Tables(0).Rows(0)!previo))
                        ElseIf elmensaje!evento = 304 Or elmensaje!evento = 307 Then
                            eMensaje = Replace(eMensaje, "[80]", ValNull(registroDS.Tables(0).Rows(0)!carga, "A"))
                            eMensaje = Replace(eMensaje, "[40]", ValNull(registroDS.Tables(0).Rows(0)!nproceso, "A"))
                            eMensaje = Replace(eMensaje, "[61]", ValNull(registroDS.Tables(0).Rows(0)!nequipo, "A"))
                            eMensaje = Replace(eMensaje, "[81]", Format(registroDS.Tables(0).Rows(0)!fecha, mIFormato))
                            eMensaje = Replace(eMensaje, "[82]", calcularTiempoCad(DateAndTime.DateDiff(DateInterval.Second, registroDS.Tables(0).Rows(0)!fecha, DateAndTime.Now)))
                            eMensaje = Replace(eMensaje, "[83]", calcularTiempoCad(registroDS.Tables(0).Rows(0)!previo))
                            eMensaje = Replace(eMensaje, "[84]", ValNull(registroDS.Tables(0).Rows(0)!texto, "A"))
                        End If
                    Else
                        If elmensaje!evento = 101 Then
                            eMensaje = "REPORTE " & nroReporte & " TIEMPO ESPERA EXCED"
                        ElseIf elmensaje!evento = 102 Then
                            eMensaje = "REPORTE " & nroReporte & " TIEMPO REPARAC EXCED"
                        ElseIf elmensaje!evento = 103 Then
                            eMensaje = "REPORTE " & nroReporte & " TIEMPO INFORME EXCED"
                        ElseIf elmensaje!evento = 201 Then
                            eMensaje = "BAJO RATE EN " & laMaquina
                        ElseIf elmensaje!evento = 202 Then
                            eMensaje = "SOBRE RATE EN " & laMaquina
                        ElseIf elmensaje!evento = 203 Then
                            eMensaje = laMaquina & " NO SE DETECTAN PIEZAS"
                        ElseIf elmensaje!evento = 301 Then
                            eMensaje = "SALTO DE OPERACION"
                        ElseIf elmensaje!evento = 302 Then
                            eMensaje = "TIEMPO DE STOCK VENCIDO"
                        ElseIf elmensaje!evento = 303 Then
                            eMensaje = "TIEMPO DE PROCESO VENCIDO"
                        ElseIf elmensaje!evento = 304 Then
                            eMensaje = "TIEMPO DE ENTREGA VENCIDO"
                        ElseIf elmensaje!evento = 305 Then
                            eMensaje = "TIEMPO DE STOCK POR VENCER"
                        ElseIf elmensaje!evento = 306 Then
                            eMensaje = "TIEMPO DE PROCESO POR VENCER"
                        ElseIf elmensaje!evento = 307 Then
                            eMensaje = "TIEMPO DE ENTREGA POR VENCER"
                        End If
                    End If

                    If elmensaje!canal = 2 Then
                        If eTitulo.Length = 0 Then
                            eTitulo = ValNull(registroDS.Tables(0).Rows(0)!titulo, "A")
                        End If
                        If eTitulo.Length > 0 Then
                            eTitulo = Replace(eTitulo, "[0]", nroReporte)
                            eTitulo = Replace(eTitulo, "[1]", laLinea)
                            eTitulo = Replace(eTitulo, "[2]", laMaquina)
                            eTitulo = Replace(eTitulo, "[3]", laArea)
                            eTitulo = Replace(eTitulo, "[4]", laFalla)
                            eTitulo = Replace(eTitulo, "[5]", Format(fecha, "ddd, dd-MMM-yyyy HH:mm"))
                            eTitulo = Replace(eTitulo, "[11]", tiempo)
                            If elmensaje!evento <= 300 Then
                                eTitulo = Replace(eTitulo, "[12]", Format(registroDS.Tables(0).Rows(0)!rate, "0") & "%")
                                eTitulo = Replace(eTitulo, "[13]", Format(registroDS.Tables(0).Rows(0)!oee, "0") & "%")
                            End If

                            If ValNull(registroDS.Tables(0).Rows(0)!repeticiones, "N") > 0 Then
                                eTitulo = Replace(eTitulo, "[20]", "Repetición " & ValNull(registroDS.Tables(0).Rows(0)!repeticiones, "N"))
                            Else
                                eTitulo = Replace(eTitulo, "[20]", "")
                            End If
                            If ValNull(registroDS.Tables(0).Rows(0)!fase - 10, "N") > 0 Then
                                Dim escala = ValNull(registroDS.Tables(0).Rows(0)!fase, "N") - 10
                                eTitulo = Replace(eTitulo, "[30]", "Escalado al Nivel " & If(escala > 0, escala, 0))
                            Else
                                eTitulo = Replace(eTitulo, "[30]", "")
                            End If
                            If ValNull(registroDS.Tables(0).Rows(0)!escalamientos1, "N") > 0 Then
                                eTitulo = Replace(eTitulo, "[31]", "Repetición " & ValNull(registroDS.Tables(0).Rows(0)!escalamientos1, "N"))
                            Else
                                eTitulo = Replace(eTitulo, "[31]", "")
                            End If
                            If ValNull(registroDS.Tables(0).Rows(0)!escalamientos2, "N") > 0 Then
                                eTitulo = Replace(eTitulo, "[32]", "Repetición " & ValNull(registroDS.Tables(0).Rows(0)!escalamientos2, "N"))
                            Else
                                eTitulo = Replace(eTitulo, "[32]", "")
                            End If
                            If ValNull(registroDS.Tables(0).Rows(0)!escalamientos3, "N") > 0 Then
                                eTitulo = Replace(eTitulo, "[33]", "Repetición " & ValNull(registroDS.Tables(0).Rows(0)!escalamientos3, "N"))
                            Else
                                eTitulo = Replace(eTitulo, "[33]", "")
                            End If
                            If ValNull(registroDS.Tables(0).Rows(0)!escalamientos4, "N") > 0 Then
                                eTitulo = Replace(eTitulo, "[34]", "Repetición " & ValNull(registroDS.Tables(0).Rows(0)!escalamientos4, "N"))
                            Else
                                eTitulo = Replace(eTitulo, "[34]", "")
                            End If
                            If ValNull(registroDS.Tables(0).Rows(0)!escalamientos5, "N") > 0 Then
                                eTitulo = Replace(eTitulo, "[35]", "Repetición " & ValNull(registroDS.Tables(0).Rows(0)!escalamientos5, "N"))
                            Else
                                eTitulo = Replace(eTitulo, "[35]", "")
                            End If
                            eTitulo = Replace(eTitulo, "[90]", "")
                            eTitulo = Replace(eTitulo, System.Environment.NewLine, " ")

                            If elmensaje!evento = 301 Then
                                eTitulo = Replace(eTitulo, "[41]", ValNull(registroDS.Tables(0).Rows(0)!nlote, "A"))
                                eTitulo = Replace(eTitulo, "[42]", ValNull(registroDS.Tables(0).Rows(0)!referencia, "A"))
                                eTitulo = Replace(eTitulo, "[43]", ValNull(registroDS.Tables(0).Rows(0)!producto, "A"))
                                eTitulo = Replace(eTitulo, "[70]", ValNull(registroDS.Tables(0).Rows(0)!ruta_antes, "A"))
                                eTitulo = Replace(eTitulo, "[71]", ValNull(registroDS.Tables(0).Rows(0)!ruta_secuencia_antes, "A"))
                                eTitulo = Replace(eTitulo, "[72]", ValNull(registroDS.Tables(0).Rows(0)!ruta_despues, "A"))
                                eTitulo = Replace(eTitulo, "[73]", ValNull(registroDS.Tables(0).Rows(0)!ruta_secuencia, "A"))

                            ElseIf elmensaje!evento = 302 Or elmensaje!evento = 305 Then
                                eTitulo = Replace(eTitulo, "[41]", ValNull(registroDS.Tables(0).Rows(0)!nlote, "A"))
                                eTitulo = Replace(eTitulo, "[42]", ValNull(registroDS.Tables(0).Rows(0)!referencia, "A"))
                                eTitulo = Replace(eTitulo, "[43]", ValNull(registroDS.Tables(0).Rows(0)!producto, "A"))
                                eTitulo = Replace(eTitulo, "[40]", ValNull(registroDS.Tables(0).Rows(0)!nproceso, "A"))
                                eTitulo = Replace(eTitulo, "[44]", ValNull(registroDS.Tables(0).Rows(0)!ruta_actual, "A"))
                                eTitulo = Replace(eTitulo, "[45]", ValNull(registroDS.Tables(0).Rows(0)!ruta_secuencia, "A"))
                                eTitulo = Replace(eTitulo, "[50]", Format(registroDS.Tables(0).Rows(0)!fecha, mIFormato))
                                eTitulo = Replace(eTitulo, "[51]", Format(registroDS.Tables(0).Rows(0)!hasta, mIFormato))
                                eTitulo = Replace(eTitulo, "[52]", calcularTiempoCad(DateAndTime.DateDiff(DateInterval.Second, registroDS.Tables(0).Rows(0)!hasta, DateAndTime.Now)))
                                eTitulo = Replace(eTitulo, "[83]", calcularTiempoCad(registroDS.Tables(0).Rows(0)!previo))
                            ElseIf elmensaje!evento = 303 Or elmensaje!evento = 306 Then
                                eTitulo = Replace(eTitulo, "[41]", ValNull(registroDS.Tables(0).Rows(0)!nlote, "A"))
                                eTitulo = Replace(eTitulo, "[42]", ValNull(registroDS.Tables(0).Rows(0)!referencia, "A"))
                                eTitulo = Replace(eTitulo, "[43]", ValNull(registroDS.Tables(0).Rows(0)!producto, "A"))
                                eTitulo = Replace(eTitulo, "[40]", ValNull(registroDS.Tables(0).Rows(0)!nproceso, "A"))
                                eTitulo = Replace(eTitulo, "[44]", ValNull(registroDS.Tables(0).Rows(0)!ruta_actual, "A"))
                                eTitulo = Replace(eTitulo, "[45]", ValNull(registroDS.Tables(0).Rows(0)!ruta_secuencia, "A"))
                                eTitulo = Replace(eTitulo, "[61]", ValNull(registroDS.Tables(0).Rows(0)!equipo, "A"))
                                eTitulo = Replace(eTitulo, "[62]", Format(registroDS.Tables(0).Rows(0)!fecha, mIFormato))
                                eTitulo = Replace(eTitulo, "[63]", Format(registroDS.Tables(0).Rows(0)!hasta, mIFormato))
                                eTitulo = Replace(eTitulo, "[64]", calcularTiempoCad(DateAndTime.DateDiff(DateInterval.Second, registroDS.Tables(0).Rows(0)!hasta, DateAndTime.Now)))
                                eTitulo = Replace(eTitulo, "[83]", calcularTiempoCad(registroDS.Tables(0).Rows(0)!previo))
                            ElseIf elmensaje!evento = 304 Or elmensaje!evento = 307 Then
                                eTitulo = Replace(eTitulo, "[80]", ValNull(registroDS.Tables(0).Rows(0)!carga, "A"))
                                eTitulo = Replace(eTitulo, "[40]", ValNull(registroDS.Tables(0).Rows(0)!nproceso, "A"))
                                eTitulo = Replace(eTitulo, "[61]", ValNull(registroDS.Tables(0).Rows(0)!nequipo, "A"))
                                eTitulo = Replace(eTitulo, "[81]", Format(registroDS.Tables(0).Rows(0)!fecha, mIFormato))
                                eTitulo = Replace(eTitulo, "[82]", calcularTiempoCad(DateAndTime.DateDiff(DateInterval.Second, registroDS.Tables(0).Rows(0)!fecha, DateAndTime.Now)))
                                eTitulo = Replace(eTitulo, "[83]", calcularTiempoCad(registroDS.Tables(0).Rows(0)!previo))
                                eTitulo = Replace(eTitulo, "[84]", ValNull(registroDS.Tables(0).Rows(0)!texto, "A"))

                            End If
                        End If

                    ElseIf elmensaje!canal = 3 Then
                        Dim antes As String = "ÃÀÁÄÂÈÉËÊÌÍÏÎÒÓÖÔÙÚÜÛãàáäâèéëêìíïîòóöôùúüûÑñÇç"
                        Dim ahora As String = "AAAAAEEEEIIIIOOOOUUUUaaaaaeeeeiiiioooouuuunncc"
                        For i = 0 To antes.Length - 1
                            eMensaje = Replace(eMensaje, antes(i), ahora(i))
                        Next
                        eMensaje = Replace(eMensaje, ";", " ")
                        eMensaje = Replace(eMensaje, "\", "-")
                        eMensaje = Replace(eMensaje, "/", "-")
                        eMensaje = Replace(eMensaje, "[90]", "")
                        eMensaje = Replace(eMensaje, System.Environment.NewLine, " ")
                        'Se cambian los caracteres especiales
                    End If
                    eMensaje = Strings.Left(eMensaje, 400)
                    eTitulo = Strings.Left(eTitulo, 100)
                End If
                Dim cadAdic = ""
                If eMensaje.Length > 0 Then
                    cadAdic = "INSERT INTO " & rutaBD & ".mensajes_procesados (texto, canal, titulo, prioridad, fecha, mensaje) VALUES ('" & eMensaje & "', " & elmensaje!canaL & ", '" & eTitulo & "', " & elmensaje!prioridad & ", NOW(), " & elmensaje!id & ");"
                End If
                regsAfectados = consultaACT(cadAdic & "UPDATE " & rutaBD & ".mensajes SET estatus = 'E', enviada = NOW() WHERE estatus = '" & idProceso & "'")
            Next
        End If
    End Sub

    Sub paseaStock()
        Dim cadSQL = "SELECT modulo_wip FROM " & rutaBD & ".configuracion WHERE modulo_wip = 'S'"
        Dim config As DataSet = consultaSEL(cadSQL)
        Dim aCortar = False
        Dim TTotal As Long = 0
        If config.Tables(0).Rows.Count > 0 Then
            cadSQL = "SELECT IFNULL((SELECT MIN(orden) FROM " & rutaBD & ".prioridades WHERE parte = a.parte AND fecha >= NOW() AND estatus = 'A'), 100) AS prioridad, a.id, a.estado, a.proceso, a.ruta_detalle, a.fecha, a.calcular_hasta, b.tiempo_stock, c.ultimo_parte, a.parte, a.equipo FROM " & rutaBD & ".lotes a LEFT JOIN " & rutaBD & ".det_rutas b ON a.ruta_detalle = b.id LEFT JOIN " & rutaBD & ".cat_maquinas c ON a.equipo = c.id WHERE (a.estado = 0 or a.calcular_hasta <> 'N') AND a.estatus = 'A' ORDER BY prioridad, a.fecha ASC"
            Dim reader As DataSet = consultaSEL(cadSQL)
            Dim pases = 0
            Dim realculoStock = 0
            Dim realculoEquipo = 0

            Dim regsAfectados = 0
            If errorBD.Length > 0 Then
                agregarLOG("Ocurrió un error al intentar leer MySQL. Error: " + Microsoft.VisualBasic.Strings.Left(errorBD, 250), 9, 0)
            Else
                If reader.Tables(0).Rows.Count > 0 Then
                    For Each lotes In reader.Tables(0).Rows
                        If lotes!id = 1459 Then
                            Dim uno = 1
                        End If
                        If lotes!calcular_hasta = "N" Or lotes!calcular_hasta = "1" Or lotes!calcular_hasta = "3" Then
                            cadSQL = "SELECT capacidad_stock - (SELECT COUNT(*) FROM " & rutaBD & ".lotes WHERE estado = 20 And proceso = " & lotes!proceso & " AND estatus = 'A') AS capstock FROM " & rutaBD & ".cat_procesos WHERE id = " & lotes!proceso
                            Dim procesos As DataSet = consultaSEL(cadSQL)
                            If procesos.Tables(0).Rows.Count > 0 Then
                                If procesos.Tables(0).Rows(0)!capstock > 0 And (lotes!calcular_hasta <> "3" Or lotes!fecha <= Now()) Then
                                    Dim loteID = lotes!id
                                    Dim procesoID = lotes!proceso
                                    Dim completado = False
                                    Dim fechaEstimada = Now()
                                    Dim FHasta = calcularFechaEstimada(lotes!fecha, lotes!tiempo_stock, lotes!proceso)
                                    If lotes!estado = 0 Then
                                        pases = pases + 1
                                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".lotes SET estado = 20, fecha = NOW(), calcular_hasta = 'N', hasta = '" & Format(FHasta, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & lotes!id & ";UPDATE " & rutaBD & ".lotes_historia SET fecha_stock = NOW(), tiempo_espera = TIME_TO_SEC(TIMEDIFF(NOW(), fecha_entrada)) WHERE lote = " & lotes!id & " AND proceso = " & lotes!proceso)
                                    Else
                                        realculoStock = realculoStock + 1
                                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".lotes SET hasta = '" & Format(FHasta, "yyyy/MM/dd HH:mm:ss") & "', calcular_hasta = 'N' WHERE id = " & lotes!id)
                                    End If

                                End If
                            End If
                        ElseIf lotes!calcular_hasta = "2" Then
                            cadSQL = "SELECT a.tiempo_proceso, a.tiempo_setup, b.reduccion_setup FROM " & rutaBD & ".det_rutas a LEFT JOIN " & rutaBD & ".cat_procesos b ON a.proceso = b.id WHERE a.id = " & lotes!ruta_detalle
                            Dim procesos As DataSet = consultaSEL(cadSQL)
                            If procesos.Tables(0).Rows.Count > 0 Then
                                pases = pases + 1
                                Dim loteID = lotes!id
                                Dim procesoID = lotes!proceso
                                Dim completado = False
                                Dim fechaEstimada = Now()
                                Dim tiempo_sumar = ValNull(procesos.Tables(0).Rows(0)!tiempo_proceso, "N")
                                If ValNull(lotes!ultimo_parte, "N") <> lotes!parte Or procesos.Tables(0).Rows(0)!reduccion_setup = "N" Then
                                    tiempo_sumar = tiempo_sumar + procesos.Tables(0).Rows(0)!tiempo_setup
                                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".cat_maquinas SET ultimo_parte = " & lotes!parte & " WHERE id = " & lotes!equipo & ";UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET desde_rate = NOW() WHERE equipo = " & lotes!equipo)
                                End If
                                realculoEquipo = realculoEquipo + 1
                                Dim FHasta = calcularFechaEstimada(lotes!fecha, tiempo_sumar, lotes!proceso)
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".lotes SET hasta = '" & Format(FHasta, "yyyy/MM/dd HH:mm:ss") & "', calcular_hasta = 'N' WHERE id = " & lotes!id)
                            End If

                        End If

                    Next
                End If
                If pases > 0 Then agregarSolo("Se transfirieron " & pases & " lote(s) desde la situación En Espera a situación En Stock")
                If realculoStock > 0 Then agregarSolo("Se recalculó la fecha de stock de " & realculoStock & " lote(s)")
                If realculoEquipo > 0 Then agregarSolo("Se calculó la fecha de proceso de " & realculoEquipo & " lote(s)")
                If pases > 0 Then agregarLOG("Se ha(n) transferido: " & pases & " lote(s) a la siguiente situación", 1, 1)
            End If
        End If
    End Sub

    Sub asignarCarga()
        Dim cadSQL As String = "Select * FROM " & rutaBD & ".configuracion WHERE modulo_wip = 'S'"
        Dim readerDS As DataSet = consultaSEL(cadSQL)
        Dim dias_programacion = 30
        Dim regsAfectados = 0
        Dim holgura_reprogramar = 0
        If readerDS.Tables(0).Rows.Count > 0 Then
            Dim configuracion As DataRow = readerDS.Tables(0).Rows(0)
            dias_programacion = ValNull(configuracion!dias_programacion, "N")
            holgura_reprogramar = ValNull(configuracion!holgura_reprogramar, "N")
        Else
            Exit Sub
        End If
        Dim pases = 0
        Dim desasignar = 0
        regsAfectados = consultaACT("UPDATE " & rutaBD & ".lotes SET carga = 0 WHERE estado <> 99 and estatus = 'A'")
        cadSQL = "SELECT a.id, a.equipo, b.parte, b.cantidad, c.proceso FROM " & rutaBD & ".cargas a INNER JOIN " & rutaBD & ".programacion b ON a.id = b.carga AND b.estatus = 'A' LEFT JOIN " & rutaBD & ".cat_maquinas c ON a.equipo = c.id AND c.estatus = 'A' WHERE a.estatus = 'A' AND completada <> 'Y' ORDER BY a.fecha ASC"
        Dim reader As DataSet = consultaSEL(cadSQL)
        If errorBD.Length > 0 Then
            agregarLOG("Ocurrió un Error al intentar leer MySQL. Error: " + Microsoft.VisualBasic.Strings.Left(errorBD, 250), 9, 0)
        Else

            If reader.Tables(0).Rows.Count > 0 Then
                For Each cargas In reader.Tables(0).Rows
                    cadSQL = "SELECT a.id, IFNULL((SELECT MIN(orden) FROM " & rutaBD & ".prioridades WHERE parte = a.parte AND fecha >= NOW() AND estatus = 'A'), 100) AS prioridad FROM " & rutaBD & ".lotes a WHERE a.estatus = 'A' AND a.carga = 0 AND a.estado <= 50 AND a.proceso = " & cargas!proceso & " AND a.parte = " & cargas!parte & " ORDER BY inspecciones, prioridad, fecha  ASC"
                    Dim reader2 As DataSet = consultaSEL(cadSQL)
                    If reader2.Tables(0).Rows.Count > 0 Then
                        Dim faltan = ValNull(cargas!cantidad, "N")
                        For Each lotes In reader2.Tables(0).Rows
                            If faltan > 0 Then
                                faltan = faltan - 1
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".lotes SET carga = " & cargas!id & " WHERE id = " & lotes!id)
                                pases = pases + 1
                            Else
                                Exit For
                            End If
                        Next
                    End If
                Next
            End If
            cadSQL = "SELECT a.id, a.equipo, a.estatus, c.proceso, a.fecha, a.completada, IFNULL(z.piezas_2, 0) AS piezas, IFNULL(Y.avance_2, 0) AS avance, IFNULL(X.enequipo_2, 0) AS enequipo FROM " & rutaBD & ".cargas a LEFT JOIN " & rutaBD & ".cat_maquinas c ON a.equipo = c.id AND c.estatus = 'A' LEFT JOIN (SELECT carga, equipo, COUNT(*) AS enequipo_2 FROM " & rutaBD & ".lotes WHERE estado = 50  AND estatus = 'A' GROUP BY carga, equipo) AS X ON X.carga = a.id AND X.equipo = a.equipo LEFT JOIN (SELECT carga, COUNT(*) avance_2 FROM " & rutaBD & ".lotes WHERE estado <= 50 GROUP BY carga) AS Y ON Y.carga = a.id LEFT JOIN (SELECT carga, SUM(cantidad) AS piezas_2 FROM " & rutaBD & ".programacion WHERE estatus = 'A' GROUP BY carga) AS z ON z.carga = a.id WHERE a.estatus = 'A' AND a.completada <> 'Y' ORDER BY a.fecha ASC"
            reader = consultaSEL(cadSQL)
            If reader.Tables(0).Rows.Count > 0 Then
                For Each cargas In reader.Tables(0).Rows
                    If cargas!piezas <= cargas!avance Then
                        If cargas!completada = "N" Then
                            Dim diferencia = DateAndTime.DateDiff(DateInterval.Second, cargas!fecha, DateAndTime.Now)
                            If diferencia < 0 Or diferencia > holgura_reprogramar Then
                                'Se reprograma las programaciones subsiguientes
                                cadSQL = "SELECT a.id, a.fecha, b.proceso FROM " & rutaBD & ".cargas a LEFT JOIN " & rutaBD & ".cat_maquinas b ON a.equipo = b.id WHERE a.permitir_reprogramacion = 'S' AND a.id <> " & cargas!id & " AND a.estatus = 'A' AND a.fecha >= '" & Format(cargas!fecha, "yyyy/MM/dd HH:mm:ss") & "' AND a.equipo = " & cargas!equipo
                                Dim cargasAdic As DataSet = consultaSEL(cadSQL)
                                If cargasAdic.Tables(0).Rows.Count > 0 Then
                                    For Each reprograma In cargasAdic.Tables(0).Rows
                                        Dim FHasta = calcularFechaEstimada(reprograma!fecha, diferencia, reprograma!proceso)
                                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".cargas SET fecha_anterior = fecha, fecha = '" & Format(FHasta, "yyyy/MM/dd HH:mm:ss") & "', reprogramaciones = reprogramaciones + 1 WHERE id = " & reprograma!id)
                                    Next
                                End If
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".cargas SET completada = 'S' WHERE id = " & cargas!id)
                            End If
                        ElseIf cargas!enequipo >= cargas!piezas Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".cargas SET completada = 'Y' WHERE id = " & cargas!id)
                        ElseIf cargas!enequipo > 0 Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".cargas SET completada = 'U' WHERE id = " & cargas!id)
                        ElseIf cargas!completada <> "S" Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".cargas SET completada = 'S' WHERE id = " & cargas!id)
                        End If
                    ElseIf cargas!completada <> "N" Then
                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".cargas SET completada = 'N' WHERE id = " & cargas!id)
                    End If
                Next
            End If
            If pases > 0 Then
                agregarLOG("Se ha(n) asignado: " & pases & " cargas de programación", 1, 1)
            End If
        End If



    End Sub

    Sub calcularEstimado()
        Dim cadSQL = "SELECT modulo_wip FROM " & rutaBD & ".configuracion WHERE modulo_wip = 'S'"
        Dim config As DataSet = consultaSEL(cadSQL)
        Dim aCortar = False
        Dim TTotal As Long = 0
        If config.Tables(0).Rows.Count > 0 Then

            cadSQL = "SELECT id, ruta, inicia FROM " & rutaBD & ".lotes WHERE ISNULL(estimada)"
            Dim reader As DataSet = consultaSEL(cadSQL)
            Dim regsAfectados = 0
            Dim pases1 = 0
            Dim pases2 = 0

            If errorBD.Length > 0 Then
                agregarLOG("Ocurrió un error al intentar leer MySQL. Error: " + Microsoft.VisualBasic.Strings.Left(errorBD, 250), 9, 0)
            Else
                If reader.Tables(0).Rows.Count > 0 Then
                    For Each lotes In reader.Tables(0).Rows
                        cadSQL = "SELECT proceso, tiempo_stock, tiempo_proceso, tiempo_setup FROM " & rutaBD & ".det_rutas WHERE estatus = 'A' AND ruta = " & lotes!ruta & " ORDER BY secuencia"
                        Dim procesos As DataSet = consultaSEL(cadSQL)
                        Dim fechaInicial = lotes!inicia
                        Dim fechaHasta = fechaInicial
                        If procesos.Tables(0).Rows.Count > 0 Then
                            Dim tiempoEstimadoTotal = 0
                            For Each operaciones In procesos.Tables(0).Rows
                                fechaHasta = calcularFechaEstimada(fechaInicial, operaciones!tiempo_stock, operaciones!proceso)
                                fechaHasta = calcularFechaEstimada(fechaHasta, operaciones!tiempo_proceso + operaciones!tiempo_setup, operaciones!proceso)
                                fechaInicial = fechaHasta
                            Next
                            pases1 = pases1 + 1
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".lotes SET tiempo_estimado = " & DateAndTime.DateDiff(DateInterval.Second, lotes!inicia, fechaHasta) & ", estimada = '" & Format(fechaInicial, "yyyy/MM/dd HH:mm:ss") & "', calcular_hasta = 'N' WHERE id = " & lotes!id)
                        End If
                    Next
                End If
            End If

            cadSQL = "SELECT a.id, a.proceso, b.tiempo_stock, b.tiempo_proceso, b.tiempo_setup, a.fecha_entrada FROM " & rutaBD & ".lotes_historia a LEFT JOIN " & rutaBD & ".det_rutas b ON a.ruta_detalle = b.id WHERE ISNULL(a.fecha_estimada)"
            reader = consultaSEL(cadSQL)
            regsAfectados = 0
            If errorBD.Length > 0 Then
                agregarLOG("Ocurrió un error al intentar leer MySQL. Error: " + Microsoft.VisualBasic.Strings.Left(errorBD, 250), 9, 0)
            Else
                Dim pases = 0
                If reader.Tables(0).Rows.Count > 0 Then
                    For Each lotes In reader.Tables(0).Rows
                        Dim fechaHasta = calcularFechaEstimada(lotes!fecha_entrada, lotes!tiempo_stock + lotes!tiempo_proceso + lotes!tiempo_setup, lotes!proceso)
                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".lotes_historia SET tiempo_estimado = " & lotes!tiempo_stock + lotes!tiempo_proceso + lotes!tiempo_setup & ",fecha_estimada = '" & Format(fechaHasta, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & lotes!id)
                        pases2 = pases2 + 1
                    Next
                End If
            End If
            If pases1 > 0 Then agregarSolo("Se calculó el estimado de fecha de entrega para " & pases1 & " lote(s)")
            If pases2 > 0 Then agregarSolo("Se calculó el estimado de fecha de entrega para " & pases2 & "  histórico de lote(s)")
        End If
    End Sub



    Sub sensoresTick()
        If revisandoSensores Then Exit Sub
        If primerSensor Then primerSensor = False
        Dim hayParoNP
        Dim ultimo = DateAndTime.Now()
        revisandoSensores = True
        Dim regsAfectados As Long = 0
        Dim cadAdic = ""
        Dim cadAdic2 = ""
        Dim produccion As Long = 0, produccion_seg = 0, calidad As Long = 0, buffer As Long = 0
        Dim produccion_tc As Double = 0, calidad_tc As Double = 0
        Dim general As DataSet
        incluyeHoyos = False
        Dim cadSQL = "SELECT modulo_oee, turno_oee, andon_prorrateado FROM " & rutaBD & ".configuracion WHERE modulo_oee = 'S'"
        Dim config As DataSet = consultaSEL(cadSQL)
        Dim aCortar = False
        Dim TTotal As Long = 0
        Dim hayTipo3 As Boolean
        Dim AP As Boolean = False
        If config.Tables(0).Rows.Count > 0 Then
            AP = ValNull(config.Tables(0).Rows(0)!andon_prorrateado, "A") = "S"
            'Se calcula el turno

            regsAfectados = consultaACT("UPDATE " & rutaBD & ".lecturas SET estatus = 1 WHERE estatus = 0")
            cadSQL = "SELECT c.id AS maquinaid, c.oee_estado, c.oee_estado_desde, c.oee_historico_rate, c.oee_historico_rate_reiniciar, c.linea, c.oee_umbral_produccion, c.nombre AS nequipo, e.nombre AS nparte, e.referencia, f.nombre AS ntripulacion, g.numero AS norden, d.nombre AS nturno, c.oee_turno_actual, c.oee_tripulacion_actual, c.oee_lote_actual, c.oee_parte_actual, b.id, b.equipo, b.tipo, b.multiplicador, b.base, DATE(a.fecha) AS fecha, IFNULL(SUM(a.valor), 0) AS totals, d.mover FROM " & rutaBD & ".cat_maquinas c INNER JOIN " & rutaBD & ".relacion_procesos_sensores b ON b.equipo = c.id and b.estatus = 'A' LEFT JOIN " & rutaBD & ".cat_partes e ON c.oee_parte_actual = e.id AND e.estatus = 'A' LEFT JOIN " & rutaBD & ".cat_tripulacion f ON c.oee_tripulacion_actual = f.id AND f.estatus = 'A' LEFT JOIN " & rutaBD & ".cat_turnos d ON c.oee_turno_actual = d.id AND d.estatus = 'A' LEFT JOIN " & rutaBD & ".lecturas a ON b.sensor = a.sensor AND a.estatus = 1 LEFT JOIN " & rutaBD & ".lotes g ON c.oee_lote_actual = g.id AND g.estatus = 'A' WHERE c.oee = 'S' AND c.estatus = 'A' GROUP BY c.linea, c.nombre, e.nombre, e.referencia, f.nombre, b.equipo, b.tipo, b.multiplicador, b.base, DATE(a.fecha), c.oee_turno_actual, c.oee_tripulacion_actual, c.oee_lote_actual, c.oee_parte_actual, d.mover"

            Dim capturas = consultaSEL(cadSQL)
            If capturas.Tables(0).Rows.Count > 0 Then
                Dim equipoActual = 0
                For Each captura In capturas.Tables(0).Rows
                    hayTipo3 = False
                    'MsgBox("Maquina  a maquina: " & captura!maquinaid)
                    If captura!maquinaid = 19 Then
                        Dim uno = 1
                    End If

                    produccion_seg = 0
                    produccion_tc = 0
                    calidad_tc = 0
                    'Buscar rate
                    Dim rateEquipo As Double = 1
                    Dim rateBajo As Double = 75
                    Dim rateAlto As Double = 125
                    Dim medTiempo As Long = 0
                    Dim rateUnidad = "PzaxHr"
                    Dim cadSQLParo = ""
                    Dim funcionando As Boolean

                    cadSQL = "SELECT piezas, bajo, alto, tiempo, unidad FROM " & rutaBD & ".relacion_partes_equipos WHERE (equipo = " & captura!maquinaid & " Or equipo = 0) And (parte = " & captura!oee_parte_actual & " Or parte = 0) ORDER BY parte DESC, equipo DESC LIMIT 1"
                    general = consultaSEL(cadSQL)
                    If general.Tables(0).Rows.Count > 0 Then
                        ''MsgBox("entro 4 " & general.Tables(0).Rows(0)!piezas)
                        rateEquipo = general.Tables(0).Rows(0)!piezas
                        rateBajo = general.Tables(0).Rows(0)!bajo
                        rateAlto = general.Tables(0).Rows(0)!alto
                        medTiempo = general.Tables(0).Rows(0)!tiempo
                        rateUnidad = ValNull(general.Tables(0).Rows(0)!unidad, "A")
                    End If

                    If rateEquipo = 0 Then rateEquipo = 1
                    Dim TC As Double = 0
                    If medTiempo = 2 Then
                        TC = 3600 / rateEquipo
                    ElseIf medTiempo = 1 Then
                        TC = 60 / rateEquipo
                        ' TC = rateEquipo / 60
                    ElseIf medTiempo = 0 Then
                        TC = 1 / rateEquipo
                    ElseIf medTiempo = 3 Then
                        TC = 86400 / rateEquipo
                    End If

                    TC = Math.Round(TC, 10)
                    'MsgBox("Tiempo ciclo " & TC


                    'Buscar Paro

                    Dim conProduccion = False
                    Dim conRateBajo = False
                    cadAdic = ""
                    cadAdic2 = ""
                    Dim paroActual As Long = 0
                    Dim reporteActual As Long = 0
                    Dim paroDesde = ultimo
                    Dim paroCorte = ultimo
                    Dim paroFinal = ultimo
                    Dim fechaEstado = ultimo
                    Dim tiempoEstado As Long = 0


                    Dim cadIniciarParo = ""
                    cadSQL = "SELECT id, corte, reporte, finaliza_sensor, tipo, estado, inicia, finaliza FROM " & rutaBD & ".detalleparos WHERE maquina = " & captura!maquinaid & " AND ISNULL(finaliza) AND estatus = 'A' AND estado <> 'F' ORDER BY tipo, inicia LIMIT 1"
                    general = consultaSEL(cadSQL)
                    Dim tiempoParo As Long = 0
                    Dim tiempoReporte As Long = 0
                    TTotal = 0
                    If general.Tables(0).Rows.Count > 0 Then
                        'Si hay paro, se 
                        paroActual = general.Tables(0).Rows(0)!id
                        reporteActual = general.Tables(0).Rows(0)!reporte

                        paroDesde = general.Tables(0).Rows(0)!inicia

                        If Not general.Tables(0).Rows(0)!corte.Equals(System.DBNull.Value) Then
                            paroCorte = general.Tables(0).Rows(0)!corte
                        Else
                            paroCorte = paroDesde
                        End If
                        Dim pFinalizado = False
                        If reporteActual > 0 Then
                            cadSQL = "SELECT id, fecha FROM " & rutaBD & ".reportes WHERE id = " & reporteActual & " AND estatus <= 10 AND afecta_oee = 'S'"
                            Dim miReporte = consultaSEL(cadSQL)
                            If miReporte.Tables(0).Rows.Count = 0 Then
                                'Se cerró el reporte
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".detalleparos SET estado = 'F', tiempo = " & tiempoValido(paroDesde, ultimo, captura!maquinaid) & ", finaliza = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', finalizo_accion = 'R' WHERE reporte = " & reporteActual)
                                reporteActual = 0
                                paroActual = 0
                                pFinalizado = True
                            End If
                        End If
                        If Not pFinalizado Then
                            If ValNull(captura!oee_estado, "A") = "S" Then
                                fechaEstado = captura!oee_estado_desde
                                tiempoEstado = DateAndTime.DateDiff(DateInterval.Second, fechaEstado, ultimo)
                                TTotal = tiempoEstado
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".cat_maquinas SET oee_estado = 'N', oee_estado_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', oee_estado_cambio = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & captura!maquinaid & ";UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'D' WHERE equipo = " & captura!maquinaid)
                                funcionando = False
                            End If
                            'Se detiene el equipo

                            'MsgBox("Hubo un paro " & paroActual)



                            If general.Tables(0).Rows(0)!tipo = 1 Then
                                hayParoNP = True
                            Else
                                hayParoNP = general.Tables(0).Rows(0)!finaliza_sensor = "S"
                                If general.Tables(0).Rows(0)!estado = "L" Then
                                    cadIniciarParo = ", inicia = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                                End If
                            End If
                            tiempoParo = tiempoValido(paroCorte, ultimo, captura!maquinaid)


                            paroFinal = ultimo
                        End If
                    End If
                    produccion_seg = 0
                    produccion = 0
                    calidad = 0
                    buffer = 0
                    Dim piezas As Long = 0
                    Dim fecha = ultimo
                    If captura!totals > 0 Then
                        fecha = captura!fecha
                        piezas = captura!totals * captura!multiplicador
                        If captura!base > 0 Then
                            piezas = piezas / captura!base
                        End If
                    End If
                    If ValNull(captura!mover, "N") = 1 Then
                        fecha = DateAdd(DateInterval.Day, -1, fecha)
                    ElseIf ValNull(captura!mover, "N") = 2 Then
                        fecha = DateAdd(DateInterval.Day, 1, fecha)
                    End If

                    If captura!tipo = 3 Then
                        hayTipo3 = True
                        cadSQL = "SELECT oee_estado, oee_estado_desde FROM " & rutaBD & ".cat_maquinas WHERE id = " & captura!maquinaid
                        general = consultaSEL(cadSQL)
                        If general.Tables(0).Rows.Count > 0 Then
                            If ValNull(general.Tables(0).Rows(0)!oee_estado, "A") = "S" Then
                                tiempoEstado = DateAndTime.DateDiff(DateInterval.Second, general.Tables(0).Rows(0)!oee_estado_desde, ultimo)
                                TTotal = TTotal + tiempoEstado
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".cat_maquinas SET oee_estado_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & captura!maquinaid & ";UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'F' WHERE equipo = " & captura!maquinaid)
                                funcionando = True
                            End If
                        End If
                    End If

                    'Buscar si ya existe el corte
                    cadSQL = "SELECT id, paro_actual, bloque_inicia, bloque_finaliza FROM " & rutaBD & ".lecturas_cortes WHERE dia = '" & Format(fecha, "yyyy/MM/dd") & "' AND equipo = " & captura!maquinaid & " AND orden = " & captura!oee_lote_actual & " AND parte = " & captura!oee_parte_actual & " AND turno = " & captura!oee_turno_actual & " AND tc = " & TC & " AND tripulacion = " & captura!oee_tripulacion_actual
                    Dim miEquipo = consultaSEL(cadSQL)
                    Dim cadParo = ""
                    If miEquipo.Tables(0).Rows.Count > 0 Then
                        'MsgBox("Piezas del sensor " & captura!totals)
                        'Se busca el turno
                        If captura!totals > 0 Then
                            If captura!tipo = 0 Then
                                cadAdic = ", produccion = produccion + " & piezas
                                cadAdic = cadAdic & ", produccion_tc = produccion_tc + " & piezas * TC
                                produccion = piezas
                                produccion_tc = piezas * TC
                                produccion_seg = piezas
                            ElseIf captura!tipo = 1 Or captura!tipo = 4 Then
                                cadAdic = ", calidad = calidad + " + piezas
                                cadAdic = cadAdic & ", calidad_tc = calidad_tc + " + piezas * TC
                                calidad = piezas
                                calidad_tc = piezas * TC
                            ElseIf captura!tipo = 2 Then
                                cadAdic = ", buffer = buffer + " + piezas
                                buffer = piezas
                            ElseIf captura!tipo = 3 Then

                                cadSQL = "SELECT fecha, valor FROM " & rutaBD & ".lecturas WHERE sensor = " & captura!id & " AND estatus = 1 ORDER BY id"
                                Dim regsSensor = consultaSEL(cadSQL)
                                If regsSensor.Tables(0).Rows.Count > 0 Then
                                    Dim fechaInicial = miEquipo.Tables(0).Rows(0)!bloque_inicia
                                    If Not captura!oee_estado_desde.Equals(System.DBNull.Value) Then
                                        fechaInicial = captura!oee_estado_desde
                                    End If
                                    For Each rSensor In regsSensor.Tables(0).Rows
                                        If rSensor!valor = 2 And ValNull(captura!oee_estado, "A") <> "S" Then
                                            'Encendido
                                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".cat_maquinas SET oee_estado = 'S', oee_estado_desde = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "', oee_estado_cambio = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & captura!maquinaid & ";UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'F', fecha_desde = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "' WHERE equipo = " & captura!maquinaid)
                                            funcionando = True
                                            fechaInicial = rSensor!fecha
                                        ElseIf rSensor!valor = 1 And ValNull(captura!oee_estado, "A") = "S" Then
                                            'Apagado
                                            tiempoEstado = DateAndTime.DateDiff(DateInterval.Second, fechaInicial, rSensor!fecha)
                                            TTotal = TTotal + tiempoEstado
                                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".cat_maquinas SET oee_estado = 'N', oee_estado_desde = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "', oee_estado_cambio = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & captura!maquinaid & ";UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'D', fecha_desde = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "' WHERE equipo = " & captura!maquinaid)
                                            funcionando = False
                                        End If
                                    Next
                                    cadAdic = ", produccion = produccion + " & TTotal & ", produccion_tc = produccion_tc + " & TTotal
                                    produccion_seg = TTotal
                                End If
                            End If
                            paroActual = IIf(hayParoNP, 0, paroActual)
                        ElseIf captura!tipo = 3 And TTotal > 0 Then
                            cadAdic = ", produccion = produccion + " & TTotal & ", produccion_tc = produccion_tc + " & TTotal
                            produccion_seg = TTotal
                        End If

                        If reporteActual > 0 And AP Then
                            tiempoParo = tiempoParo - produccion_tc
                        End If

                        If paroActual > 0 And miEquipo.Tables(0).Rows(0)!paro_actual = paroActual Then

                            'cadAdic = cadAdic & ", paro_abierto = paro_abierto + " & tiempoParo
                            cadSQLParo = cadSQLParo & ";UPDATE " & rutaBD & ".detalleparos_info SET tiempo = tiempo + " & tiempoParo & " WHERE paro = " & paroActual & " AND tipo = 'C' AND corte = " & miEquipo.Tables(0).Rows(0)!id


                        ElseIf paroActual > 0 And miEquipo.Tables(0).Rows(0)!paro_actual <> paroActual Then
                            'cadAdic = cadAdic & ", paro_cerrado = paro_cerrado + paro_abierto, paro_abierto = " & tiempoParo & ", paro_actual = " & paroActual
                            cadAdic = cadAdic & ", paro_actual = " & paroActual
                            cadSQLParo = cadSQLParo & ";INSERT INTO " & rutaBD & ".detalleparos_info (paro, corte, tiempo, reporte) VALUES (" & paroActual & ", " & miEquipo.Tables(0).Rows(0)!id & ", " & tiempoParo & ", " & reporteActual & ")"

                        ElseIf paroActual = 0 And miEquipo.Tables(0).Rows(0)!paro_actual > 0 Then
                            'cadAdic = cadAdic & ", paro_cerrado = paro_cerrado + paro_abierto, paro_abierto = 0, paro_actual = 0"
                            cadAdic = cadAdic & ", paro_actual = 0"
                        End If

                        If paroActual > 0 Then
                            cadParo = ";UPDATE " & rutaBD & ".detalleparos SET estado = 'C', corte = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'" & cadIniciarParo & " WHERE id = " & paroActual
                        ElseIf miEquipo.Tables(0).Rows(0)!paro_actual > 0 Then
                            Dim cadTipoParo = ""
                            cadSQL = "SELECT tipo, inicia FROM " & rutaBD & ".detalleparos WHERE id = " & miEquipo.Tables(0).Rows(0)!paro_actual
                            general = consultaSEL(cadSQL)
                            If general.Tables(0).Rows.Count > 0 Then
                                If general.Tables(0).Rows(0)!tipo = 0 Then
                                    cadTipoParo = ", finalizo_accion = 'T'"
                                    paroDesde = general.Tables(0).Rows(0)!inicia
                                End If
                            End If
                            cadParo = ";UPDATE " & rutaBD & ".detalleparos SET estado = 'F', tiempo = " & tiempoValido(paroDesde, ultimo, captura!maquinaid) & ", finaliza = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'" & cadTipoParo & " WHERE id = " & miEquipo.Tables(0).Rows(0)!paro_actual
                        End If

                        cadSQL = "UPDATE " & rutaBD & ".lecturas_cortes SET tiempo_disponible = " & tiempoValido(miEquipo.Tables(0).Rows(0)!bloque_inicia, ultimo, captura!maquinaid) & ", bloque_finaliza = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'" & cadAdic & " WHERE id = " & miEquipo.Tables(0).Rows(0)!id & cadParo
                    Else
                        'MsgBox("Piezas del sensor (nuevo) " & captura!totals)
                        If captura!totals > 0 Then
                            paroActual = IIf(hayParoNP, 0, paroActual)
                            cadAdic2 = cadAdic2 & ", " & piezas & ", " & piezas * TC
                            If captura!tipo = 0 Then
                                cadAdic = ", produccion, produccion_tc"
                                produccion = piezas
                                produccion_seg = piezas
                                produccion_tc = piezas * TC
                            ElseIf captura!tipo = 1 Then
                                cadAdic = ", calidad, calidad_tc"
                                calidad = piezas
                            ElseIf captura!tipo = 2 Then
                                cadAdic = ", buffer"
                                buffer = piezas
                            ElseIf captura!tipo = 3 Then
                                cadSQL = "SELECT fecha, valor FROM " & rutaBD & ".lecturas WHERE sensor = " & captura!id & " AND estatus = 1 ORDER BY id"
                                Dim regsSensor = consultaSEL(cadSQL)
                                If regsSensor.Tables(0).Rows.Count > 0 Then
                                    Dim fechaInicial = ultimo
                                    If Not captura!oee_estado_desde.Equals(System.DBNull.Value) Then
                                        fechaInicial = captura!oee_estado_desde
                                    End If
                                    For Each rSensor In regsSensor.Tables(0).Rows
                                        If rSensor!valor = 2 And ValNull(captura!oee_estado, "A") <> "S" Then
                                            'Encendido
                                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".cat_maquinas SET oee_estado = 'S', oee_estado_desde = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "', oee_estado_cambio = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & captura!maquinaid & ";UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'F', fecha_desde = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "' WHERE equipo = " & captura!maquinaid)
                                            funcionando = True
                                            fechaInicial = rSensor!fecha
                                        ElseIf rSensor!valor = 1 And ValNull(captura!oee_estado, "A") = "S" Then
                                            'Apagado
                                            tiempoEstado = DateAndTime.DateDiff(DateInterval.Second, fechaInicial, rSensor!fecha)
                                            TTotal = TTotal + tiempoEstado
                                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".cat_maquinas SET oee_estado = 'N', oee_estado_desde = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "', oee_estado_cambio = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & captura!maquinaid & ";UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET estatus = 'D', fecha_desde = '" & Format(rSensor!fecha, "yyyy/MM/dd HH:mm:ss") & "' WHERE equipo = " & captura!maquinaid)
                                            funcionando = False
                                        End If
                                    Next
                                    cadAdic = ", produccion, produccion_tc"
                                    cadAdic2 = cadAdic2 & ", " & TTotal & ", " & TTotal
                                    produccion_seg = TTotal
                                End If
                            End If
                        ElseIf captura!tipo = 3 And TTotal > 0 Then
                            cadAdic = ", produccion, produccion_tc"
                            cadAdic2 = cadAdic2 & ", " & TTotal & ", " & TTotal
                            produccion_seg = TTotal

                        End If
                        If paroActual > 0 Then
                            cadAdic = cadAdic & ", paro_abierto, paro_actual"
                            cadAdic2 = cadAdic2 & ", " & tiempoParo & ", " & paroActual
                            cadParo = ";UPDATE " & rutaBD & ".detalleparos SET estado = 'C', corte = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'" & cadIniciarParo & " WHERE id = " & paroActual
                        Else
                            cadParo = ";UPDATE " & rutaBD & ".detalleparos SET estado = 'F', tiempo = " & tiempoValido(paroDesde, ultimo, captura!maquinaid) & ", finaliza = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', finalizo_accion = 'T' WHERE maquina = " & captura!maquinaid & " AND tipo = 0 AND estado = 'C'"
                            'Se busca si hay un paro anterior
                        End If

                        cadSQL = "INSERT " & rutaBD & ".lecturas_cortes (dia, orden, parte, turno, equipo, tripulacion" & cadAdic & ", bloque_inicia, bloque_finaliza, tc) VALUES ('" & Format(fecha, "yyyy/MM/dd") & "', " & captura!oee_lote_actual & ", " & captura!oee_parte_actual & ", " & captura!oee_turno_actual & ", " & captura!maquinaid & ", " & captura!oee_tripulacion_actual & cadAdic2 & ", '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', " & TC & ")" & cadParo
                    End If
                    'MsgBox("Ya se grabó el sensor")
                    regsAfectados = consultaACT(cadSQL & cadSQLParo)
                    If equipoActual <> captura!maquinaid Or equipoActual = 0 Then
                        equipoActual = captura!maquinaid
                        cadSQLParo = ""
                        cadAdic = ""
                        cadAdic2 = ""
                        'Buscar objetivo
                        Dim objetivo As Double = 0
                        Dim reinicio As Long = 0
                        cadSQL = "SELECT objetivo, reinicio FROM " & rutaBD & ".equipos_objetivo WHERE (equipo = " & captura!maquinaid & " OR equipo = 0) AND (parte = " & captura!oee_parte_actual & " OR parte = 0) AND (fijo = 'S' OR ('" & Format(fecha, "yyyy/MM/dd") & "' >= desde AND '" & Format(fecha, "yyyy/MM/dd") & "' <= hasta AND (turno = 0 OR turno = " & captura!oee_turno_actual & ") AND (lote = 0 OR lote = " & captura!oee_lote_actual & "))) ORDER BY parte DESC, equipo DESC, fijo, lote DESC, turno DESC LIMIT 1"
                        general = consultaSEL(cadSQL)
                        If general.Tables(0).Rows.Count > 0 Then
                            objetivo = general.Tables(0).Rows(0)!objetivo
                            reinicio = general.Tables(0).Rows(0)!reinicio
                        End If

                        'MsgBox("Objetivo " & objetivo)

                        'Buscar paros

                        'Buscar estimados
                        Dim EFI As Double = 100, OEE As Double = 85, FTQ As Double = 100, DIS As Double = 100
                        Dim eEFI As Double = 100, eOEE As Double = 85, eFTQ As Double = 100, eDIS As Double = 100
                        cadSQL = "SELECT * FROM " & rutaBD & ".estimados WHERE (linea = " & captura!linea & " OR linea = 0) AND (equipo = " & captura!maquinaid & " OR equipo = 0) AND (fijo = 'S' OR ('" & Format(fecha, "yyyy/MM/dd") & "' >= desde AND '" & Format(fecha, "yyyy/MM/dd") & "' <= hasta )) ORDER BY equipo DESC, linea DESC, fijo, desde LIMIT 1"
                        general = consultaSEL(cadSQL)
                        If general.Tables(0).Rows.Count > 0 Then
                            eOEE = general.Tables(0).Rows(0)!oee
                            eEFI = general.Tables(0).Rows(0)!efi
                            eFTQ = general.Tables(0).Rows(0)!ftq
                            eDIS = general.Tables(0).Rows(0)!dis
                        End If


                        Dim rateActual As Double = 0
                        Dim ultimaProduccion = ultimo

                        'Se crear el registro a mostrar en el visor web
                        'cadSQL = "SELECT IFNULL((SELECT SUM(IF(tiempo > 0, tiempo, 0)) FROM " & rutaBD & ".detalleparos_info WHERE corte = a.equipo AND tipo = 'A' AND estatus = 'A'), 0) AS parosmostrar, tiempo_reinicio, tiempo_corte, transcurrido, transcurrido_pasar, estatus, iniciar, iniciar_1, iniciar_2, iniciar_3, iniciar_4, iniciar_5, iniciar_6, iniciar_7, iniciar_8, detener, reanudar, proximo_paro, ultima_produccion, fecha_desde, desde_rate, produccion, produccion_tc, calidad, calidad_tc, buffer, rate_mal_desde, rate, rate_tendencia_baja, rate_tendencia_alta, rate_efecto, paros, ftq, efi, dis, oee, ftq_tendencia_baja, efi_tendencia_baja, dis_tendencia_baja, oee_tendencia_baja, paro_actual, IFNULL(piezashr, 0) AS piezashr, IFNULL(t_paros, 0) AS t_paros FROM " & rutaBD & ".relacion_maquinas_lecturas a LEFT JOIN (SELECT equipo, SUM(produccion) AS piezashr, SUM(paro) AS t_paros FROM " & rutabd & ".piezasxminuto WHERE fecha >= DATE_ADD('" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', INTERVAL " & horasAtras & " HOUR) GROUP BY equipo) As b On a.equipo = b.equipo WHERE a.equipo = " & captura!maquinaid
                        cadSQL = "SELECT a.equipo, IFNULL((SELECT SUM(IF(tiempo > 0, tiempo, 0)) FROM " & rutaBD & ".detalleparos_info WHERE corte = a.equipo AND tipo = 'A' AND estatus = 'A'), 0) AS parosmostrar, tiempo_reinicio, tiempo_corte, transcurrido, transcurrido_pasar, estatus, iniciar, iniciar_1, iniciar_2, iniciar_3, iniciar_4, iniciar_5, iniciar_6, iniciar_7, iniciar_8, detener, reanudar, proximo_paro, ultima_produccion, fecha_desde, desde_rate, produccion, produccion_tc, calidad, calidad_tc, buffer, rate_mal_desde, rate, rate_tendencia_baja, rate_tendencia_alta, rate_efecto, paros, ftq, efi, dis, oee, ftq_tendencia_baja, efi_tendencia_baja, dis_tendencia_baja, oee_tendencia_baja, paro_actual, IFNULL(SUM(piezashr), 0) AS piezashr, IFNULL(SUM(t_paros), 0) AS t_paros FROM " & rutaBD & ".relacion_maquinas_lecturas a LEFT JOIN (SELECT equipo, fecha, produccion AS piezashr, paro AS t_paros FROM " & rutaBD & ".piezasxminuto) AS b On a.equipo = b.equipo AND fecha >= a.desde_rate WHERE a.equipo = " & captura!maquinaid
                        Dim miResumen = consultaSEL(cadSQL)
                        If miResumen.Tables(0).Rows.Count = 0 Then
                            regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".relacion_maquinas_lecturas (equipo, ultima_produccion, ultima_buffer, fecha_desde, desde_rate) VALUES(" & captura!maquinaid & ", '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "')")
                            miResumen = consultaSEL(cadSQL)
                        ElseIf miResumen.Tables(0).Rows(0)!equipo.Equals(System.DBNull.Value) Then
                            regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".relacion_maquinas_lecturas (equipo, ultima_produccion, ultima_buffer, fecha_desde, desde_rate) VALUES(" & captura!maquinaid & ", '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "')")
                            miResumen = consultaSEL(cadSQL)
                        Else
                            If Not miResumen.Tables(0).Rows(0)!ultima_produccion.Equals(System.DBNull.Value) Then
                                ultimaProduccion = miResumen.Tables(0).Rows(0)!ultima_produccion
                            End If
                        End If
                        Dim tiempoTranscurrido = 1
                        Dim corteAPP = ultimo

                        If Not miResumen.Tables(0).Rows(0)!fecha_desde.Equals(System.DBNull.Value) Then
                            corteAPP = miResumen.Tables(0).Rows(0)!fecha_desde
                            tiempoTranscurrido = tiempoValido(miResumen.Tables(0).Rows(0)!fecha_desde, ultimo, captura!maquinaid)

                        Else
                            cadAdic = cadAdic & ", fecha_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                        End If

                        Dim cortePT = ultimo

                        If Not miResumen.Tables(0).Rows(0)!desde_rate.Equals(System.DBNull.Value) Then
                            cortePT = miResumen.Tables(0).Rows(0)!desde_rate
                        Else
                            cadAdic = cadAdic & ", desde_rate = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                        End If

                        Dim tiempoTranscurridoRate = 1

                        Dim produccionRate = miResumen.Tables(0).Rows(0)!piezashr
                        Dim paroRate = miResumen.Tables(0).Rows(0)!t_paros

                        Dim horasAtras = -24
                        If captura!oee_historico_rate = 0 Then
                            If cortePT > corteAPP Then
                                corteAPP = cortePT
                                'No se busca en la BD
                            End If
                        ElseIf captura!oee_historico_rate = 1 Then
                            horasAtras = -1
                        ElseIf captura!oee_historico_rate = 2 Then
                            horasAtras = -2
                        ElseIf captura!oee_historico_rate = 3 Then
                            horasAtras = -6
                        ElseIf captura!oee_historico_rate = 4 Then
                            horasAtras = -12
                        End If

                        If captura!oee_historico_rate > 0 Then
                            corteAPP = cortePT
                            If corteAPP < DateAdd(DateInterval.Hour, horasAtras, ultimo) Then
                                corteAPP = DateAdd(DateInterval.Hour, horasAtras, ultimo)
                            End If
                        End If
                        If corteAPP <> cortePT Then
                            cadSQL = "SELECT IFNULL(SUM(produccion), 0) AS piezashr, IFNULL(SUM(paro), 0) AS t_paros FROM " & rutaBD & ".piezasxminuto WHERE fecha >= '" & Format(corteAPP, "yyyy/MM/dd HH:mm:ss") & "' AND equipo = " & captura!maquinaid

                            general = consultaSEL(cadSQL)
                            If general.Tables(0).Rows.Count > 0 Then
                                produccionRate = general.Tables(0).Rows(0)!piezashr
                                paroRate = general.Tables(0).Rows(0)!t_paros
                            End If
                        End If
                        tiempoTranscurridoRate = tiempoValido(corteAPP, ultimo, captura!maquinaid)
                        Dim disponibilidad = tiempoTranscurrido - miResumen.Tables(0).Rows(0)!parosmostrar
                        Dim disponibilidadRate = tiempoTranscurridoRate - paroRate

                        produccion = miResumen.Tables(0).Rows(0)!produccion + produccion + TTotal
                        produccion_tc = miResumen.Tables(0).Rows(0)!produccion_tc + produccion_tc + TTotal

                        If disponibilidad <= 0 Then disponibilidad = 1
                        If disponibilidadRate <= 0 Then disponibilidadRate = 1

                        Dim uRate As Double = 0

                        rateActual = produccionRate / disponibilidadRate

                        If medTiempo = 2 Then
                            rateActual = rateActual * 3600
                        ElseIf medTiempo = 1 Then
                            rateActual = rateActual * 60
                            ' TC = rateEquipo / 60
                        ElseIf medTiempo = 0 Then

                        ElseIf medTiempo = 3 Then
                            rateActual = rateActual * 86400
                        End If

                        If miResumen.Tables(0).Rows(0)!produccion = produccion And paroActual > 0 Then
                            rateActual = miResumen.Tables(0).Rows(0)!rate
                        End If


                        uRate = rateActual


                        cadAdic = cadAdic & ", fuera_programa = '" & IIf(miResumen.Tables(0).Rows(0)!transcurrido = tiempoTranscurrido And miResumen.Tables(0).Rows(0)!transcurrido_pasar = 1, "S", "N") & "'"

                        If miResumen.Tables(0).Rows(0)!produccion < produccion Then
                            cadAdic = cadAdic & ", ultima_produccion = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', alarmado_manual = 'N'"
                            ultimaProduccion = ultimo
                            conProduccion = True
                        End If
                        buffer = miResumen.Tables(0).Rows(0)!buffer + buffer
                        If miResumen.Tables(0).Rows(0)!buffer < buffer Then
                            cadAdic = cadAdic & ", ultima_buffer = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                            ultimaProduccion = ultimo
                            conProduccion = True
                        End If
                        hayParoNP = False
                        ''MsgBox("entro 13 ")
                        If captura!oee_umbral_produccion > 0 And captura!oee_umbral_produccion <= DateAndTime.DateDiff(DateInterval.Second, ultimaProduccion, ultimo) Then

                            If miResumen.Tables(0).Rows(0)!paro_actual = 0 Then
                                regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".detalleparos (paro, clase, maquina, fecha, tiempo, inicio, inicia, estado, estatus) VALUES ('NO SE DETECTAN DE PIEZAS', 1, " & captura!maquinaid & ", '" & Format(fecha, "yyyy/MM/dd") & "', 0, 1, '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', 'C', 'A')")

                                hayParoNP = True
                            Else
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".detalleparos SET tiempo = " & tiempoValido(paroDesde, ultimo, captura!maquinaid) & " WHERE id = " & miResumen.Tables(0).Rows(0)!paro_actual & " AND tipo = 1")
                                hayParoNP = regsAfectados > 0

                            End If
                        Else
                            If miResumen.Tables(0).Rows(0)!paro_actual > 0 And captura!oee_umbral_produccion > DateAndTime.DateDiff(DateInterval.Second, ultimaProduccion, ultimo) Then
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".detalleparos SET estado = 'F', tiempo = " & tiempoValido(paroDesde, ultimo, captura!maquinaid) & ", finaliza = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & miResumen.Tables(0).Rows(0)!paro_actual & " AND (clase = 1 OR clase = 2)")

                            End If
                        End If

                        If reporteActual = 0 Then
                            cadSQL = "SELECT a.id, a.fecha, a.area, b.agrupador_2 FROM " & rutaBD & ".reportes a LEFT JOIN " & rutaBD & ".cat_fallas b ON a.falla = b.id WHERE a.maquina = " & captura!maquinaid & " AND a.estatus <= 10 AND a.afecta_oee = 'S' ORDER BY a.id LIMIT 1"
                            general = consultaSEL(cadSQL)
                            If general.Tables(0).Rows.Count > 0 Then
                                reporteActual = general.Tables(0).Rows(0)!id
                                Dim fechaReporte = general.Tables(0).Rows(0)!fecha
                                Dim agrupador = ValNull(general.Tables(0).Rows(0)!agrupador_2, "N")

                                cadSQL = "SELECT id, fecha FROM " & rutaBD & ".detalleparos WHERE reporte = " & reporteActual
                                general = consultaSEL(cadSQL)

                                If general.Tables(0).Rows.Count = 0 Then
                                    regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".detalleparos (paro, clase, reporte, maquina, fecha, tiempo, inicio, inicia, estado, estatus, area, tipo) VALUES ('ANDON', 3, " & reporteActual & ", " & captura!maquinaid & ", '" & Format(fecha, "yyyy/MM/dd") & "', 0, 1, '" & Format(fechaReporte, "yyyy/MM/dd HH:mm:ss") & "', 'C', 'A', " & general.Tables(0).Rows(0)!area & ", " & agrupador & ")")
                                End If
                            End If
                        End If


                        paroActual = 0
                        cadSQL = "SELECT id, tipo, inicia FROM " & rutaBD & ".detalleparos WHERE maquina = " & captura!maquinaid & " AND ISNULL(finaliza) AND estatus = 'A' AND estado <> 'F' ORDER BY tipo, inicia LIMIT 1"
                        general = consultaSEL(cadSQL)
                        If general.Tables(0).Rows.Count > 0 Then
                            paroDesde = general.Tables(0).Rows(0)!inicia
                            If general.Tables(0).Rows(0)!tipo = 0 And hayParoNP Then
                                'Había un paro NP antes
                                Dim tReporte = 0
                                If AP And reporteActual > 0 Then
                                    tReporte = tiempoValido(general.Tables(0).Rows(0)!inicia, ultimo, captura!maquinaid) - produccion_tc
                                Else
                                    tReporte = tiempoValido(general.Tables(0).Rows(0)!inicia, ultimo, captura!maquinaid)
                                End If
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".detalleparos SET estado = 'F', tiempo = " & tReporte & ", finaliza = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & miResumen.Tables(0).Rows(0)!paro_actual & " AND tipo = " & IIf(reporteActual > 0, 3, 1))
                            ElseIf general.Tables(0).Rows(0)!tipo = 1 Then
                                'Había un paro NP antes
                                Dim tReporte = 0
                                If AP And reporteActual > 0 Then
                                    tReporte = tiempoValido(general.Tables(0).Rows(0)!inicia, ultimo, captura!maquinaid) - produccion_tc
                                Else
                                    tReporte = tiempoValido(general.Tables(0).Rows(0)!inicia, ultimo, captura!maquinaid)
                                End If
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".detalleparos SET estado = 'F', tiempo = " & tReporte & ", finaliza = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & miResumen.Tables(0).Rows(0)!paro_actual & " AND tipo = " & IIf(reporteActual > 0, 3, 1))


                            End If
                            paroActual = general.Tables(0).Rows(0)!id
                        End If
                        ''MsgBox("entro 14 ")
                        'Paro siguiente
                        Dim paroSiguiente As Long = 0
                        cadSQL = "SELECT id FROM " & rutaBD & ".detalleparos WHERE maquina = " & captura!maquinaid & " AND inicia > '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' AND estatus = 'A' AND estado = 'L' ORDER BY inicia LIMIT 1"
                        general = consultaSEL(cadSQL)
                        If general.Tables(0).Rows.Count > 0 Then
                            paroSiguiente = general.Tables(0).Rows(0)!id
                        End If

                        cadAdic = cadAdic & ", proximo_paro = " & paroSiguiente

                        If paroActual > 0 Then
                            If miResumen.Tables(0).Rows(0)!fecha_desde > paroCorte Then
                                tiempoParo = tiempoValido(miResumen.Tables(0).Rows(0)!fecha_desde, ultimo, captura!maquinaid)
                                If reporteActual > 0 And AP Then
                                    tiempoParo = tiempoParo - produccion_tc
                                End If
                            End If

                            cadSQL = "SELECT estatus, reporte FROM " & rutaBD & ".detalleparos_info WHERE corte = " & captura!maquinaid & " AND tipo = 'A' AND paro = " & paroActual
                            general = consultaSEL(cadSQL)

                            If general.Tables(0).Rows.Count > 0 Then
                                If general.Tables(0).Rows(0)!estatus = "A" Then
                                    cadSQLParo = ";UPDATE " & rutaBD & ".detalleparos_info SET tiempo = tiempo + " & tiempoParo & " WHERE paro = " & paroActual & " AND tipo = 'A' AND estatus = 'A' AND corte = " & captura!maquinaid
                                End If
                            Else
                                cadSQLParo = ";INSERT INTO " & rutaBD & ".detalleparos_info (paro, corte, tipo, tiempo, reporte) VALUES (" & paroActual & ", " & captura!maquinaid & ", 'A', " & tiempoParo & ", " & reporteActual & ")"
                            End If

                            If miResumen.Tables(0).Rows(0)!paro_actual = paroActual Then
                                'cadAdic = cadAdic & ", parosmostrar = paros + " & tiempoValido(paroDesde, Now(), captura!maquinaid)
                            ElseIf miResumen.Tables(0).Rows(0)!paro_actual > 0 Then
                                cadAdic = cadAdic & ", paros = parosmostrar, paro_actual = " & paroActual & ", parada_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                            Else
                                cadAdic = cadAdic & ", estatus = 'D', parosmostrar = paros, paro_actual = " & paroActual & ", parada_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                            End If

                        ElseIf miResumen.Tables(0).Rows(0)!paro_actual > 0 Then
                            cadAdic = cadAdic & ", " & IIf(hayTipo3, "", "estatus = 'F', ") & "paros = parosmostrar, paro_actual = 0, parada_desde = NULL"

                        ElseIf Not hayTipo3 Then
                            cadAdic = cadAdic & ", estatus = 'F'"
                        End If

                        Dim rateEfecto = "N"

                        If rateAlto > 0 And uRate >= rateEquipo * (rateAlto / 100) Then
                            rateEfecto = "A"
                        ElseIf rateBajo > 0 And uRate <= rateEquipo * (rateBajo / 100) Then
                            rateEfecto = "B"
                        End If
                        Dim cadFecha = ", rate_mal_desde = NULL"
                        If rateEfecto <> "N" Then
                            If rateEfecto = "N" Then
                                conRateBajo = True
                            End If
                            If miResumen.Tables(0).Rows(0)!rate_efecto <> rateEfecto Or miResumen.Tables(0).Rows(0)!rate_mal_desde.Equals(System.DBNull.Value) Then
                                cadFecha = ", rate_mal_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                            Else
                                cadFecha = ""
                            End If
                        End If

                        If hayTipo3 Then conRateBajo = Not funcionando

                        Dim cadFechaBaja = ", rate_tendencia_baja = NULL"
                        If rateEfecto = "B" Then
                            If uRate <= miResumen.Tables(0).Rows(0)!rate Then
                                If miResumen.Tables(0).Rows(0)!rate_tendencia_baja.Equals(System.DBNull.Value) Then
                                    cadFechaBaja = ", rate_tendencia_baja = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                                Else
                                    cadFechaBaja = ""
                                End If
                            End If
                        End If

                        Dim cadFechaAlta = ", rate_tendencia_alta = NULL"
                        If rateEfecto = "A" Then
                            If uRate >= miResumen.Tables(0).Rows(0)!rate Then
                                If miResumen.Tables(0).Rows(0)!rate_tendencia_alta.Equals(System.DBNull.Value) Then
                                    cadFechaAlta = ", rate_tendencia_alta = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                                Else
                                    cadFechaAlta = ""
                                End If
                            End If
                        End If

                        'Calculos de los indicadores

                        If tiempoTranscurrido = 0 Then
                            DIS = 100
                        Else
                            DIS = (disponibilidad / tiempoTranscurrido * 100)
                        End If
                        If disponibilidad = 0 And produccion_tc = 0 Then
                            EFI = 0
                        ElseIf disponibilidad = 0 Then
                            EFI = 100
                        Else
                            EFI = produccion_tc / disponibilidad * 100
                        End If

                        If EFI > 100 Then
                            EFI = 100
                        End If

                        calidad = calidad + miResumen.Tables(0).Rows(0)!calidad
                        If produccion_tc > 0 Then
                            FTQ = (produccion_tc - calidad_tc) / produccion_tc * 100
                        End If

                        If FTQ < 0 Then
                            FTQ = 0
                        End If
                        OEE = EFI * FTQ * DIS / 10000
                        OEE = Math.Round(OEE, 3)
                        EFI = Math.Round(EFI, 3)
                        DIS = Math.Round(DIS, 3)
                        FTQ = Math.Round(FTQ, 3)

                        'MsgBox("OEE " & OEE)

                        Dim cadFechaFTQ = ", ftq_tendencia_baja = NULL"
                        If FTQ <= miResumen.Tables(0).Rows(0)!ftq And miResumen.Tables(0).Rows(0)!ftq < 100 Then
                            If miResumen.Tables(0).Rows(0)!ftq_tendencia_baja.Equals(System.DBNull.Value) Then
                                cadFechaFTQ = ", ftq_tendencia_baja = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                            Else
                                cadFechaFTQ = ""
                            End If
                        End If

                        If eFTQ > 0 Then
                            cadFechaFTQ = cadFechaFTQ & ", ftq_efecto = '" & IIf(FTQ < eFTQ, "S", "N") & "'"
                        End If

                        Dim cadFechaDIS = ", dis_tendencia_baja = NULL"
                        If DIS <= miResumen.Tables(0).Rows(0)!dis And miResumen.Tables(0).Rows(0)!dis > 100 Then
                            If miResumen.Tables(0).Rows(0)!dis_tendencia_baja.Equals(System.DBNull.Value) Then
                                cadFechaDIS = ", dis_tendencia_baja = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                            Else
                                cadFechaDIS = ""
                            End If
                        End If

                        If eDIS > 0 Then
                            cadFechaFTQ = cadFechaFTQ & ", dis_efecto = '" & IIf(DIS < eDIS, "S", "N") & "'"
                        End If

                        Dim cadFechaEFI = ", efi_tendencia_baja = NULL"
                        If EFI <= miResumen.Tables(0).Rows(0)!efi And miResumen.Tables(0).Rows(0)!efi <> 0 Then
                            If miResumen.Tables(0).Rows(0)!efi_tendencia_baja.Equals(System.DBNull.Value) Then
                                cadFechaEFI = ", efi_tendencia_baja = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                            Else
                                cadFechaEFI = ""
                            End If
                        End If

                        If eEFI > 0 Then
                            cadFechaFTQ = cadFechaFTQ & ", efi_efecto = '" & IIf(EFI < eEFI, "S", "N") & "'"
                        End If

                        Dim cadFechaOEE = ", oee_tendencia_baja = NULL"
                        If OEE <= miResumen.Tables(0).Rows(0)!oee And miResumen.Tables(0).Rows(0)!oee <> 0 Then
                            If miResumen.Tables(0).Rows(0)!oee_tendencia_baja.Equals(System.DBNull.Value) Then
                                cadFechaOEE = ", oee_tendencia_baja = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                            Else
                                cadFechaOEE = ""
                            End If
                        End If

                        If eOEE > 0 Then
                            cadFechaFTQ = cadFechaFTQ & ", oee_efecto = '" & IIf(OEE < eOEE, "S", "N") & "'"
                        End If

                        Dim cadOEE = ", oee_imagen = 0"
                        If OEE < miResumen.Tables(0).Rows(0)!oee And miResumen.Tables(0).Rows(0)!oee <> 0 Then
                            cadOEE = ", oee_imagen = 2"
                        ElseIf OEE > miResumen.Tables(0).Rows(0)!oee And miResumen.Tables(0).Rows(0)!oee <> 0 Then
                            cadOEE = ", oee_imagen = 1"
                        End If



                        If EFI < miResumen.Tables(0).Rows(0)!efi And miResumen.Tables(0).Rows(0)!efi <> 0 Then
                            cadOEE = cadOEE & ", efi_imagen = 2"
                        ElseIf EFI > miResumen.Tables(0).Rows(0)!efi And miResumen.Tables(0).Rows(0)!efi <> 0 Then
                            cadOEE = cadOEE & ", efi_imagen = 1"
                        Else
                            cadOEE = cadOEE & ", efi_imagen = 0"
                        End If


                        If DIS < miResumen.Tables(0).Rows(0)!dis And miResumen.Tables(0).Rows(0)!dis <> 0 Then
                            cadOEE = cadOEE & ", dis_imagen = 2"
                        ElseIf DIS > miResumen.Tables(0).Rows(0)!dis And miResumen.Tables(0).Rows(0)!dis <> 0 Then
                            cadOEE = cadOEE & ", dis_imagen = 1"
                        Else
                            cadOEE = cadOEE & ", dis_imagen = 0"
                        End If


                        If FTQ < miResumen.Tables(0).Rows(0)!ftq And miResumen.Tables(0).Rows(0)!ftq <> 0 Then
                            cadOEE = cadOEE & ", ftq_imagen = 2"
                        ElseIf FTQ > miResumen.Tables(0).Rows(0)!ftq And miResumen.Tables(0).Rows(0)!ftq <> 0 Then
                            cadOEE = cadOEE & ", ftq_imagen = 1"
                        Else
                            cadOEE = cadOEE & ", ftq_imagen = 0"
                        End If

                        Dim cadAdicDetener = ""
                        If miResumen.Tables(0).Rows(0)!detener > 0 And paroActual = 0 Then

                            regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".detalleparos (paro, clase, maquina, fecha, tiempo, inicio, inicia, estado, estatus) VALUES ('PARO MANUAL (USUARIO)', 2, " & captura!maquinaid & ", '" & Format(fecha, "yyyy/MM/dd") & "', 0, " & miResumen.Tables(0).Rows(0)!detener & ", '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', 'C', 'A')")
                            cadSQL = "SELECT id FROM " & rutaBD & ".detalleparos WHERE maquina = " & captura!maquinaid & " AND ISNULL(finaliza) AND estatus = 'A' AND estado <> 'F' ORDER BY tipo, inicia LIMIT 1"
                            general = consultaSEL(cadSQL)
                            paroActual = 0
                            If general.Tables(0).Rows.Count > 0 Then
                                paroActual = general.Tables(0).Rows(0)!id
                            End If
                            cadAdicDetener = ", detener = 0"

                            cadSQLParo = ";INSERT INTO " & rutaBD & ".detalleparos_info (paro, corte, tipo, tiempo) VALUES (" & paroActual & ", " & captura!maquinaid & ", 'A', 0)"

                            cadAdic = cadAdic & ", estatus = 'D', parosmostrar = paros, paro_actual = " & paroActual & ", parada_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'"
                        End If


                        If miResumen.Tables(0).Rows(0)!reanudar > 0 And paroActual > 0 Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".detalleparos SET estado = 'F', tiempo = " & tiempoValido(paroDesde, ultimo, captura!maquinaid) & ", finaliza = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & miResumen.Tables(0).Rows(0)!paro_actual & ";UPDATE " & rutaBD & ".detalleparos_info SET tiempo = tiempo + " & tiempoParo & " WHERE paro = " & miResumen.Tables(0).Rows(0)!paro_actual & " AND tipo = 'A' AND corte = " & captura!maquinaid)
                            cadAdicDetener = ", reanudar = 0"

                            cadAdic = cadAdic & ", " & IIf(hayTipo3, "", "estatus = 'F', ") & "paros = parosmostrar, paro_actual = 0, parada_desde = NULL, ultima_produccion = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', alarmado_manual = 'N'"
                        End If

                        cadAdic = cadAdic & cadFecha & cadFechaBaja & cadFechaAlta & cadFechaFTQ & cadFechaDIS & cadFechaEFI & cadFechaOEE & cadOEE & cadAdicDetener

                        Dim otraCad As String = ""
                        Dim min = Format(ultimo, "mm")
                        If (min = 0 And miResumen.Tables(0).Rows(0)!tiempo_reinicio <> min) Then
                            otraCad = ", tiempo_reinicio = " & min

                            If Format(ultimo, "HHmm") = "0000" And reinicio = 5 Then
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET iniciar_5 = 'S', factor_iniciar = '" & Format(ultimo, "ddMMyy") & "' WHERE equipo = " & captura!maquinaid & " AND (factor_iniciar <> '" & Format(ultimo, "ddMMyy") & "' OR ISNULL(factor_iniciar))")
                            End If
                            If DateAndTime.Weekday(ultimo) = 2 And reinicio = 6 Then
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET iniciar_6 = 'S', factor_iniciar = '" & Format(ultimo, "ddMMyy") & "' WHERE equipo = " & captura!maquinaid & " AND (factor_iniciar <> '" & Format(ultimo, "ddMMyy") & "' OR ISNULL(factor_iniciar))")
                            End If
                            If Format(ultimo, "dd") = "01" And reinicio = 7 Then
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET iniciar_7 = 'S', factor_iniciar = '" & Format(ultimo, "ddMMyy") & "' WHERE equipo = " & captura!maquinaid & " AND (factor_iniciar <> '" & Format(ultimo, "ddMMyy") & "' OR ISNULL(factor_iniciar))")
                            End If
                            If Format(ultimo, "ddMM") = "0101" And reinicio = 8 Then
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET iniciar_8 = 'S', factor_iniciar = '" & Format(ultimo, "ddMMyy") & "' WHERE equipo = " & captura!maquinaid & " AND (factor_iniciar <> '" & Format(ultimo, "ddMMyy") & "' OR ISNULL(factor_iniciar))")
                            End If
                            If Format(ultimo, "mm") = "00" And reinicio = 3 Then
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET iniciar_3 = 'S', factor_iniciar = '" & Format(ultimo, "HHmm") & "' WHERE equipo = " & captura!maquinaid & " AND (factor_iniciar <> '" & Format(ultimo, "HHmm") & "' OR ISNULL(factor_iniciar))")
                            End If

                        End If

                        If hayTipo3 Then
                            uRate = -100
                            rateActual = -100
                            rateEquipo = -100
                            objetivo = 0
                        End If

                        cadSQL = "UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET ultima_lectura = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', parte = " & captura!oee_parte_actual & ", tripulacion = " & captura!oee_tripulacion_actual & ", turno = " & captura!oee_turno_actual & ", produccion = " & produccion & ", produccion_tc = " & produccion_tc & ", calidad = " & calidad & ", calidad_tc = " & calidad_tc & ", buffer = buffer + " & buffer & ", norden = '" & captura!norden & "', nparte = '" & captura!nparte & "', nequipo = '" & captura!nequipo & "', ntripulacion = '" & captura!ntripulacion & "', referencia = '" & captura!referencia & "', nturno = '" & captura!nturno & "', rate_teorico = " & rateEquipo & ", rate_min = " & rateBajo & ", rate_max = " & rateAlto & ", objetivo = " & objetivo & ", rate = " & rateActual & cadAdic & ", rate_efecto = '" & rateEfecto & "', ultimo_rate = " & uRate & ", ratemed = '" & rateUnidad & "', esperadodis = " & eDIS & ", esperadoftq = " & eFTQ & ", esperadoefi = " & eEFI & ", esperadooee = " & eOEE & ", hoyos = '" & IIf(incluyeHoyos, "S", "N") & "', efi = " & EFI & ", dis = " & DIS & ", ftq = " & FTQ & ", oee = " & OEE & otraCad & ", parosmostrar = 0 WHERE equipo = " & captura!maquinaid

                        'MsgBox("Guardado todo!")

                        If miResumen.Tables(0).Rows(0)!iniciar = "S" Or (miResumen.Tables(0).Rows(0)!iniciar_1 = "S" And reinicio = 1) _
                        Or (miResumen.Tables(0).Rows(0)!iniciar_2 = "S" And reinicio = 2) _
                        Or (miResumen.Tables(0).Rows(0)!iniciar_3 = "S" And reinicio = 3) _
                        Or (miResumen.Tables(0).Rows(0)!iniciar_4 = "S" And reinicio = 4) _
                        Or (miResumen.Tables(0).Rows(0)!iniciar_5 = "S" And reinicio = 5) _
                        Or (miResumen.Tables(0).Rows(0)!iniciar_6 = "S" And reinicio = 6) _
                        Or (miResumen.Tables(0).Rows(0)!iniciar_7 = "S" And reinicio = 7) _
                        Or (miResumen.Tables(0).Rows(0)!iniciar_8 = "S" And reinicio = 8) Then
                            cadSQL = cadSQL & ";DELETE FROM " & rutaBD & ".detalleparos_info WHERE tipo = 'A' AND corte = " & captura!maquinaid & ";UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET produccion = 0, calidad = 0, produccion_tc = 0, calidad_tc = 0, buffer = 0, paros = 0, parosmostrar = 0, rate_mal_desde = NULL, rate_tendencia_baja = NULL, rate_tendencia_alta = NULL, ftq_tendencia_baja = NULL, oee_tendencia_baja = NULL, dis_tendencia_baja = NULL, efi_tendencia_baja = NULL, parada_desde = NULL, paro_actual = 0, ultima_produccion = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', alarmado_manual = 'N', ultima_buffer = NULL, iniciar = 'N', iniciar_1 = 'N', iniciar_2 = 'N', iniciar_3 = 'N', iniciar_4 = 'N', iniciar_5 = 'N', iniciar_6 = 'N', iniciar_7 = 'N', iniciar_8 = 'N', fecha_desde = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'" & IIf(ValNull(captura!oee_historico_rate_reiniciar, "A") = "1", ", desde_rate = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "'", "") & " WHERE equipo = " & captura!maquinaid
                        End If

                        regsAfectados = consultaACT(cadSQL & ";" & "UPDATE " & rutaBD & ".relacion_procesos_sensores SET ultima_lectura = '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' WHERE id = " & captura!id & cadSQLParo)
                        Dim corte

                        Dim minutoEste = Format(ultimo, "mm")
                        Dim buscarTipo = False

                        cadSQL = "SELECT id FROM " & rutaBD & ".lecturas_resumen WHERE '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' >= desde AND '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "' <= hasta  AND equipo = " & captura!maquinaid
                        corte = consultaSEL(cadSQL)
                        If corte.Tables(0).Rows.Count = 0 Then
                            'Se crean los 4 registros
                            buscarTipo = True
                            Dim minuto = 0
                            For i = 1 To 4
                                regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".lecturas_resumen (equipo, desde, hasta, hora) VALUES (" & captura!maquinaid & ", '" & Format(ultimo, "yyyy/MM/dd HH:") & Format(minuto, "00") & ":00', '" & Format(ultimo, "yyyy/MM/dd HH:") & Format(minuto + 14, "00") & ":59', " & Format(ultimo, "HH") & ")")
                                minuto = minuto + 15
                            Next
                            Dim ultimo2 = ultimo
                            ultimo2 = Convert.ToDateTime(Format(ultimo2, "yyyy/MM/dd HH") & ":00:00")
                            For i = 1 To 88
                                ultimo2 = DateAndTime.DateAdd(DateInterval.Minute, -15, ultimo2)
                                Dim cadSQLINI = "SELECT id FROM " & rutaBD & ".lecturas_resumen WHERE desde = '" & Format(ultimo2, "yyyy/MM/dd HH:mm") & ":00' AND equipo = " & captura!maquinaid
                                corte = consultaSEL(cadSQLINI)
                                If corte.Tables(0).Rows.Count = 0 Then
                                    Dim fechaHasta = DateAndTime.DateAdd(DateInterval.Second, 899, ultimo2)
                                    regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".lecturas_resumen (equipo, desde, hasta, hora) VALUES (" & captura!maquinaid & ", '" & Format(ultimo2, "yyyy/MM/dd HH:mm") & ":00', '" & Format(fechaHasta, "yyyy/MM/dd HH:mm:ss") & "', " & Format(fechaHasta, "HH") & ")")

                                End If
                            Next
                            'Se recalcula el pasado
                            corte = consultaSEL(cadSQL)
                            'MsgBox("REsumenes creados")

                        End If
                        If (minutoEste Mod 5 = 0 And miResumen.Tables(0).Rows(0)!tiempo_corte <> minutoEste) Or buscarTipo Then

                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET tiempo_corte = " & minutoEste & " WHERE equipo = " & captura!maquinaid)
                            Dim cadenaTipo = "SELECT * FROM " & rutaBD & ".lecturas_resumen WHERE equipo = " & captura!maquinaid & " ORDER BY desde DESC LIMIT 8"
                            Dim calcularTipo = consultaSEL(cadenaTipo)
                            If calcularTipo.Tables(0).Rows.Count > 0 Then
                                For Each tipoColor In calcularTipo.Tables(0).Rows
                                    If tipoColor!produccion > 0 And tipoColor!produccion >= tipoColor!bajorate And tipoColor!produccion >= tipoColor!paro Then
                                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".lecturas_resumen SET tipo = 1 WHERE id = " & tipoColor!id)
                                    ElseIf tipoColor!bajorate > 0 And tipoColor!bajorate >= tipoColor!paro And tipoColor!bajorate >= tipoColor!produccion And tipoColor!bajorate >= tipoColor!sinplan Then
                                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".lecturas_resumen SET tipo = 2 WHERE id = " & tipoColor!id)
                                    ElseIf tipoColor!paro > 0 And tipoColor!paro >= tipoColor!bajorate And tipoColor!paro >= tipoColor!produccion And tipoColor!paro >= tipoColor!sinplan Then
                                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".lecturas_resumen SET tipo = 3 WHERE id = " & tipoColor!id)
                                    End If
                                Next
                            End If
                        End If
                        cadAdic = ""
                        If paroActual > 0 Then
                            cadAdic = "paro = paro + 1"
                        ElseIf conRateBajo Then
                            cadAdic = "bajorate = bajorate + 1"
                        ElseIf conProduccion Then
                            cadAdic = "produccion = produccion + 1"
                        ElseIf miResumen.Tables(0).Rows(0)!transcurrido = tiempoTranscurrido And miResumen.Tables(0).Rows(0)!transcurrido_pasar Then
                            cadAdic = "sinplan = sinplan + 1"
                        Else
                            cadAdic = "produccion = produccion + 1"
                        End If
                        If miResumen.Tables(0).Rows(0)!transcurrido <> tiempoTranscurrido Then
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET transcurrido = " & tiempoTranscurrido & ", transcurrido_pasar = 0 WHERE equipo = " & captura!maquinaid)
                        Else
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".relacion_maquinas_lecturas SET transcurrido_pasar = 1 WHERE equipo = " & captura!maquinaid)
                        End If
                        Dim cadSeg = ""
                        Dim cadSeg2 = ""
                        cadSeg = "DELETE FROM " & rutaBD & ".piezasxminuto WHERE equipo = " & captura!maquinaid & " And fecha <= DATE_ADD('" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', INTERVAL -1 DAY);"
                        If produccion_seg > 0 Or tiempoParo + tiempoReporte > 0 Then
                            Dim hhdmm = Format(ultimo, "HHmm")
                            general = consultaSEL("SELECT hhmm FROM " & rutaBD & ".piezasxminuto WHERE equipo = " & captura!maquinaid & " AND hhmm = '" & hhdmm & "'")
                            cadSeg2 = "INSERT INTO " & rutaBD & ".piezasxminuto VALUES(" & captura!maquinaid & ", '" & Format(ultimo, "yyyy/MM/dd HH:mm:ss") & "', " & produccion_seg & ", " & (tiempoParo + tiempoReporte) & ", '" & hhdmm & "');"

                            If general.Tables(0).Rows.Count > 0 Then
                                cadSeg2 = "UPDATE " & rutaBD & ".piezasxminuto SET produccion = produccion + " & produccion_seg & ", paro = paro + " & (tiempoParo + tiempoReporte) & " WHERE equipo = " & captura!maquinaid & " AND hhmm = '" & hhdmm & "';"
                            End If

                        End If
                        regsAfectados = consultaACT(cadSeg & cadSeg2 & "UPDATE " & rutaBD & ".lecturas_resumen SET " & cadAdic & "  WHERE id = " & corte.Tables(0).Rows(0)!id)
                    End If
                    TTotal = 0
                Next
                regsAfectados = consultaACT("UPDATE " & rutaBD & ".lecturas SET estatus = 2 WHERE estatus = 1")
                'MsgBox("FIN")
            End If

            regsAfectados = consultaACT("UPDATE " & rutaBD & ".configuracion Set lectura_pendiente = 0")
        End If
        revisandoSensores = False
    End Sub

End Class

