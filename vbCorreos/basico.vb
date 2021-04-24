
Imports MySql.Data.MySqlClient
Imports System.Security.Cryptography
Imports System.Text
Imports System.IO
Imports System.Data
Imports System.Net.Mail
Imports System.Net
Imports System.Timers

Module basico
    Public errorBD As String
    Public horaDesde As DateTime
    Public ultimaFallu
    Public autenticado As Boolean
    Public cadenaConexion As String
    Public be_log_activar As Boolean = False
    Public rutaBD As String = "sigma"
    'Public rutaBD As String = "sigma_nueva"
    Public traduccion As String()
    Public be_idioma
    Public par1 As Long, par2 As Long, par3 As Long
    Sub Main(argumentos As String())

        'cadenaConexion = "server=127.0.0.1;user id=root;password=usbw;port=3307;Convert Zero Datetime=True;Allow User Variables=True"
        'reporteHoraxHora(1, 54, 1)

        If Process.GetProcessesByName _
          (Process.GetCurrentProcess.ProcessName).Length > 1    Then
        ElseIf argumentos.Length = 0 Then
            MsgBox("String connection missing", MsgBoxStyle.Critical, "SIGMA")
        Else
            'MsgBox("Entro 1")
            cadenaConexion = argumentos(0)
            If argumentos.Length = 2 Then
                If argumentos(1) = "test" Then
                    pruebaMail()
                Else
                    agregarLOG("Se invoca al programa para el envío de correos", 9, 0)
                    Dim arreParametros = argumentos(1).Split(New Char() {";"c})
                    If arreParametros(0) = "oee_por_turno" Then
                        reporteOEEporTurno(Val(arreParametros(1)))
                        'cadenaConexion = "server=127.0.0.1;user id=root;password=usbw;port=3307;Convert Zero Datetime=True"
                        'reporteOEEporTurno(3)
                    ElseIf arreParametros(0) = "hxh_por_turno" Or arreParametros(0) = "hxh_por_hora" Or arreParametros(0) = "hxh_por_dia" Then
                        agregarLOG("Reporte a ejecutar: " & arreParametros(0), 9, 0)
                        'MsgBox("Entro 2")
                        Dim parametro = 1
                        If arreParametros(0) = "hxh_por_hora" Then
                            parametro = 2
                        ElseIf arreParametros(0) = "hxh_por_dia" Then
                            parametro = 3
                        End If
                        par1 = parametro
                        par2 = Val(arreParametros(1))
                        par3 = Val(arreParametros(2))
                        'Msgbox("Entro al reporte de hora por hora")
                        reporteHoraxHora(par1, par2, par3)
                    End If
                End If
            Else
                'cadenaConexion = "server=127.0.0.1;user id=root;password=usbw;port=3307;Convert Zero Datetime=True"
                Dim idProceso = Process.GetCurrentProcess.Id

                Dim mensajesDS As DataSet
                Dim eMensaje = ""
                Dim eTitulo = ""
                Dim audiosGen = 0
                Dim audiosNGen = 0
                Dim mTotal = 0
                'Escalada 4
                Dim miError As String = ""
                Dim optimizar As Boolean = False
                Dim mantenerPrioridad As Boolean = False
                Dim correo_titulo_falla As Boolean
                Dim correo_titulo As String
                Dim correo_firma As String
                Dim correo_cuenta As String
                Dim correo_puerto As String
                Dim correo_ssl As Boolean
                Dim correo_clave As String
                Dim correo_host As String
                Dim separador_mail As String
                Dim mensajeGenerado As Boolean = False
                Dim be_alarmas_correos As Boolean = False
                Dim regsAfectados = 0
                Dim registroDS As DataSet

                Dim canales As String = ""
                Dim laLinea As String = ""
                Dim laMaquina As String = ""
                Dim laArea As String = ""
                Dim laFalla As String = ""
                Dim elLote As String = ""
                Dim elProceso As String = ""
                Dim laCarga As String = ""
                Dim fecha
                Dim tiempo As String = ""
                Dim nroReporte As Integer = 0

                Dim cadSQL As String = "SELECT * FROM " & rutaBD & ".configuracion"
                Dim readerDS As DataSet = consultaSEL(cadSQL)
                Dim escape_mensaje = ""
                If readerDS.Tables(0).Rows.Count > 0 Then
                    Dim reader As DataRow = readerDS.Tables(0).Rows(0)
                    be_idioma = ValNull(reader!idioma_defecto, "N")
                    etiquetas()
                    optimizar = ValNull(reader!optimizar_correo, "A") = "S"
                    be_log_activar = ValNull(reader!be_log_activar, "A") = "S"
                    mantenerPrioridad = ValNull(reader!optimizar_mmcall, "A") = "S"
                    correo_titulo_falla = ValNull(reader!correo_titulo_falla, "A") = "S"
                    be_alarmas_correos = ValNull(reader!be_alarmas_correos, "A") = "S"
                    correo_cuenta = ValNull(reader!correo_cuenta, "A")
                    correo_clave = ValNull(reader!correo_clave, "A")
                    correo_puerto = ValNull(reader!correo_puerto, "A")
                    correo_ssl = ValNull(reader!correo_ssl, "A") = "S"
                    escape_mensaje = ValNull(reader!escape_mensaje, "A")
                    correo_host = ValNull(reader!correo_host, "A")
                    separador_mail = ValNull(reader!separador_mail, "A")
                End If
                If separador_mail = "" Then separador_mail = ";"
                'If correo_firma.Length = 0 Then
                ' correo_firma = traduccion(0)
                ' End If
                'If correo_titulo.Length = 0 Then
                ' correo_titulo = traduccion(1)
                'End If

                If be_alarmas_correos Then

                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".mensajes SET estatus = '" & idProceso & "' WHERE canal = 2 AND estatus = 'E' AND alerta >= 0")
                    Dim agrupado As Boolean = True
                    If Not optimizar Then
                        cadSQL = "SELECT a.id, d.correos, a.prioridad, z.texto, z.titulo, 1 AS cuenta FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".mensajes_procesados z ON a.id = z.mensaje INNER JOIN " & rutaBD & ".cat_alertas b on a.alerta = b.id AND b.estatus = 'A' INNER JOIN " & rutaBD & ".cat_distribucion d ON a.lista = d.id AND d.estatus = 'A' WHERE a.estatus = '" & idProceso & "' ORDER BY a.prioridad DESC, a.id"
                        agrupado = False

                    ElseIf mantenerPrioridad Then
                        cadSQL = "SELECT a.lista, a.prioridad, b.correos, COUNT(*) AS cuenta, MAX(a.id) AS id FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".mensajes_procesados z ON a.id = z.mensaje INNER JOIN " & rutaBD & ".cat_distribucion b ON a.lista = b.id AND b.estatus = 'A' INNER JOIN " & rutaBD & ".cat_alertas c on a.alerta = c.id WHERE a.estatus = '" & idProceso & "' GROUP BY a.prioridad, a.lista, b.correos ORDER BY prioridad DESC"
                    Else
                        cadSQL = "SELECT a.lista, b.correos, 0 AS prioridad, COUNT(*) AS cuenta, MAX(a.id) AS id FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".mensajes_procesados z ON a.id = z.mensaje INNER JOIN " & rutaBD & ".cat_distribucion b ON a.lista = b.id AND b.estatus = 'A' INNER JOIN " & rutaBD & ".cat_alertas c on a.alerta = c.id WHERE a.estatus = '" & idProceso & "' GROUP BY a.lista, b.correos"

                    End If
                    'Se preselecciona la voz
                    mensajesDS = consultaSEL(cadSQL)
                    If mensajesDS.Tables(0).Rows.Count > 0 Then
                        Dim enlazado As Boolean = False
                        Dim smtpServer As New SmtpClient()

                        Try
                            smtpServer.Credentials = New Net.NetworkCredential(correo_cuenta, correo_clave)
                            smtpServer.Port = correo_puerto
                            smtpServer.Host = correo_host '"smtp.live.com" '"smtp.gmail.com"
                            smtpServer.EnableSsl = correo_ssl
                            enlazado = True
                        Catch ex As Exception
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".mensajes SET estatus = 'A' WHERE canal = 2 AND estatus = '" & idProceso & "'")
                            smtpServer.Dispose()
                        End Try
                        If enlazado Then
                            For Each elmensaje In mensajesDS.Tables(0).Rows
                                canales = ValNull(elmensaje!correos, "A")
                                If canales.Length > 0 Then
                                    eMensaje = ""
                                    eTitulo = ""
                                    Dim cadWhere = "AND a.lista = " & elmensaje!lista
                                    If elmensaje!cuenta > 1 Then
                                        eMensaje = traduccion(2).Replace("campo_0", elmensaje!cuenta)
                                        If mantenerPrioridad And elmensaje!prioridad > 0 Then
                                            eMensaje = traduccion(3).Replace("campo_0", elmensaje!cuenta)
                                        End If

                                        If mantenerPrioridad Then
                                            cadWhere = cadWhere & " AND a.prioridad = " & elmensaje!prioridad
                                        End If
                                        cadSQL = "SELECT a.id, z.texto, z.titulo FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".mensajes_procesados z ON a.id = z.mensaje WHERE a.estatus = '" & idProceso & "' " & cadWhere
                                    Else
                                        cadSQL = "SELECT a.id, z.texto, z.titulo FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".mensajes_procesados z ON a.id = z.mensaje WHERE a.id = " & elmensaje!id
                                    End If
                                    Dim mensajeCorreo As String = ""
                                    registroDS = consultaSEL(cadSQL)
                                    Dim linCorreo = 0
                                    If registroDS.Tables(0).Rows.Count > 0 Then
                                        For Each elCorreo In registroDS.Tables(0).Rows
                                            linCorreo = linCorreo + 1
                                            If elmensaje!cuenta > 1 Then
                                                If linCorreo = 1 Then
                                                    mensajeCorreo = traduccion(19) & linCorreo & traduccion(20) & elmensaje!cuenta & vbCrLf
                                                Else
                                                    mensajeCorreo = mensajeCorreo & vbCrLf & vbCrLf & traduccion(19) & linCorreo & traduccion(20) & elmensaje!cuenta & vbCrLf
                                                End If
                                            End If
                                            eMensaje = ValNull(elCorreo!texto, "A")
                                            mensajeCorreo = mensajeCorreo & eMensaje & vbCrLf
                                            eTitulo = ValNull(elCorreo!titulo, "A")
                                        Next
                                        If eTitulo.Length = 0 Then
                                            eTitulo = correo_titulo
                                        End If
                                        mensajeCorreo = mensajeCorreo & vbCrLf & vbCrLf & correo_firma

                                        Dim correos As String()
                                        Dim tempArray As String()
                                        Dim totalItems = 0
                                        If canales.Length > 0 Then
                                            Dim arreCanales = canales.Split(New Char() {";"c})
                                            For i = LBound(arreCanales) To UBound(arreCanales)
                                                'Redimensionamos el Array temporal y preservamos el valor  
                                                ReDim Preserve correos(totalItems + i)
                                                correos(totalItems + i) = arreCanales(i)
                                            Next
                                            tempArray = correos
                                            totalItems = correos.Length

                                            Dim x As Integer, y As Integer
                                            Dim z As Integer

                                            For x = 0 To UBound(correos)
                                                z = 0
                                                For y = 0 To UBound(correos) - 1
                                                    'Si el elemento del array es igual al array temporal  
                                                    If correos(x) = tempArray(z) And y <> x Then
                                                        'Entonces Eliminamos el valor duplicado  
                                                        correos(y) = ""
                                                    End If
                                                    z = z + 1
                                                Next y
                                            Next x
                                        End If
                                        mensajeGenerado = False
                                        Dim mail As New MailMessage
                                        Try
                                            mail.From = New MailAddress(correo_cuenta)
                                            For i = 0 To UBound(correos)
                                                If correos(i).Length > 0 Then
                                                    mail.To.Add(correos(i))
                                                End If
                                            Next i

                                            mail.Subject = eTitulo
                                            mail.Body = mensajeCorreo
                                            smtpServer.Send(mail)
                                            audiosGen = audiosGen + 1
                                            mensajeGenerado = True
                                        Catch ex As Exception
                                            audiosNGen = audiosNGen + 1
                                            agregarLOG(ex.Message, 9, nroReporte)
                                        Finally
                                            mail.Dispose()
                                        End Try
                                        cadSQL = "UPDATE " & rutaBD & ".mensajes SET estatus = 'Z', enviada = NOW() WHERE id = " & elmensaje!id
                                        If optimizar Then
                                            If elmensaje!cuenta > 1 Then
                                                cadSQL = "UPDATE " & rutaBD & ".mensajes SET estatus = 'Z', enviada = NOW() WHERE canal = 2 AND lista = " & elmensaje!lista & " AND estatus = '" & idProceso & "'"
                                                If mantenerPrioridad Then
                                                    cadSQL = cadSQL & " AND prioridad = " & elmensaje!prioridad
                                                End If
                                            End If
                                        End If
                                        regsAfectados = consultaACT(cadSQL)
                                    End If
                                End If

                            Next
                            cadSQL = "SELECT a.id, a.texto, b.correos FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".cat_distribucion b ON a.lista = b.id AND b.estatus = 'A' WHERE a.alerta = 0 AND a.canal = 2 AND a.estatus = '" & idProceso & "' ORDER BY a.prioridad DESC, a.id"
                            mensajesDS = consultaSEL(cadSQL)
                            Dim generarMensaje = False
                            If mensajesDS.Tables(0).Rows.Count > 0 Then
                                For Each elmensaje In mensajesDS.Tables(0).Rows
                                    canales = ValNull(elmensaje!correos, "A")
                                    eMensaje = traduccion(22) & ValNull(elmensaje!texto, "A")
                                    If canales.Length > 0 Then
                                        Dim arreCanales = canales.Split(New Char() {";"c})
                                        Dim correos As String()
                                        Dim tempArray As String()
                                        Dim totalItems = 0

                                        For i = LBound(arreCanales) To UBound(arreCanales)
                                            'Redimensionamos el Array temporal y preservamos el valor  
                                            ReDim Preserve correos(totalItems + i)
                                            correos(totalItems + i) = arreCanales(i)
                                        Next
                                        tempArray = correos
                                        totalItems = correos.Length

                                        Dim x As Integer, y As Integer
                                        Dim z As Integer

                                        For x = 0 To UBound(correos)
                                            z = 0
                                            For y = 0 To UBound(correos) - 1
                                                'Si el elemento del array es igual al array temporal  
                                                If correos(x) = tempArray(z) And y <> x Then
                                                    'Entonces Eliminamos el valor duplicado  
                                                    correos(y) = ""
                                                End If
                                                z = z + 1
                                            Next y
                                        Next x
                                        mensajeGenerado = False
                                        Dim mail As New MailMessage
                                        Try
                                            mail.From = New MailAddress(correo_cuenta)
                                            For i = 0 To UBound(correos)
                                                If correos(i).Length > 0 Then
                                                    mail.To.Add(correos(i))
                                                End If
                                            Next i

                                            mail.Subject = eMensaje
                                            mail.Body = eMensaje
                                            smtpServer.Send(mail)
                                            audiosGen = audiosGen + 1
                                            mensajeGenerado = True
                                        Catch ex As Exception
                                            audiosNGen = audiosNGen + 1
                                            agregarLOG(ex.Message, 9, nroReporte)
                                        Finally
                                            mail.Dispose()
                                        End Try
                                    End If
                                    If mensajeGenerado Then
                                        cadSQL = "UPDATE " & rutaBD & ".mensajes SET estatus = 'Z', enviada = NOW() WHERE id = " & elmensaje!id
                                        regsAfectados = consultaACT(cadSQL)
                                    End If
                                Next
                                If audiosGen > 0 Or audiosNGen > 0 Then
                                    agregarLOG(traduccion(23))
                                End If
                            End If
                        End If
                        If audiosGen > 0 Or audiosNGen > 0 Then
                            agregarLOG(traduccion(23).Replace("campo_0", audiosGen).Replace("campo_1", audiosNGen))
                        End If
                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".mensajes SET estatus = 'Z' WHERE canal = 2 AND estatus = '" & idProceso & "'")
                        smtpServer.Dispose()
                    End If

                    'Checklist
                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".mensajes SET estatus = '" & idProceso & "' WHERE canal = 2 AND estatus = 'E' AND alerta = -1000")
                    cadSQL = "SELECT a.id, d.correos, 0, z.texto, z.titulo, 1 AS cuenta FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".mensajes_procesados z ON a.id = z.mensaje INNER JOIN " & rutaBD & ".cat_distribucion d ON a.lista = d.id AND d.estatus = 'A' WHERE a.estatus = '" & idProceso & "' AND a.alerta = -1000 ORDER BY a.prioridad DESC, a.id"
                    mensajesDS = consultaSEL(cadSQL)
                    If mensajesDS.Tables(0).Rows.Count > 0 Then
                        Dim enlazado As Boolean = False
                        Dim smtpServer As New SmtpClient()

                        Try
                            smtpServer.Credentials = New Net.NetworkCredential(correo_cuenta, correo_clave)
                            smtpServer.Port = correo_puerto
                            smtpServer.Host = correo_host '"smtp.live.com" '"smtp.gmail.com"
                            smtpServer.EnableSsl = correo_ssl
                            enlazado = True
                        Catch ex As Exception
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".mensajes SET estatus = 'A' WHERE canal = 2 AND estatus = '" & idProceso & "'")
                            smtpServer.Dispose()
                        End Try
                        If enlazado Then
                            For Each elmensaje In mensajesDS.Tables(0).Rows
                                canales = ValNull(elmensaje!correos, "A")
                                Dim correos As String()
                                Dim tempArray As String()
                                Dim totalItems = 0
                                If canales.Length > 0 Then
                                    Dim arreCanales = canales.Split(New Char() {";"c})
                                    For i = LBound(arreCanales) To UBound(arreCanales)
                                        'Redimensionamos el Array temporal y preservamos el valor  
                                        ReDim Preserve correos(totalItems + i)
                                        correos(totalItems + i) = arreCanales(i)
                                    Next
                                    tempArray = correos

                                    Dim x As Integer, y As Integer
                                    Dim z As Integer

                                    For x = 0 To UBound(correos)
                                        z = 0
                                        For y = 0 To UBound(correos) - 1
                                            'Si el elemento del array es igual al array temporal  
                                            If correos(x) = tempArray(z) And y <> x Then
                                                'Entonces Eliminamos el valor duplicado  
                                                correos(y) = ""
                                            End If
                                            z = z + 1
                                        Next y
                                    Next x
                                End If
                                mensajeGenerado = False
                                Dim mail As New MailMessage
                                Try
                                    mail.From = New MailAddress(correo_cuenta)
                                    For i = 0 To UBound(correos)
                                        If correos(i).Length > 0 Then
                                            mail.To.Add(correos(i))
                                        End If
                                    Next i

                                    mail.Subject = ValNull(elmensaje!titulo, "A")
                                    mail.Body = ValNull(elmensaje!texto, "A")
                                    smtpServer.Send(mail)
                                    audiosGen = audiosGen + 1
                                    mensajeGenerado = True
                                    cadSQL = "UPDATE " & rutaBD & ".mensajes SET estatus = 'Z', enviada = NOW() WHERE id = " & elmensaje!id
                                    regsAfectados = consultaACT(cadSQL)
                                Catch ex As Exception
                                    audiosNGen = audiosNGen + 1
                                    agregarLOG(ex.Message, 9, nroReporte)
                                Finally
                                    mail.Dispose()
                                End Try
                            Next
                        End If
                        If audiosGen > 0 Or audiosNGen > 0 Then
                            agregarLOG(traduccion(23).Replace("campo_0", audiosGen).Replace("campo_1", audiosNGen))
                        End If
                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".mensajes SET estatus = 'Z' WHERE canal = 2 AND estatus = '" & idProceso & "'")
                        smtpServer.Dispose()
                    End If

                    'Checklist individuales
                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".mensajes SET estatus = '" & idProceso & "' WHERE canal = 2 AND estatus = 'E' AND alerta = -2000")
                    cadSQL = "SELECT a.id, d.correos, 0, z.texto, z.titulo, 1 AS cuenta FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".mensajes_procesados z ON a.id = z.mensaje INNER JOIN " & rutaBD & ".checklist_correos d ON a.id = d.mensaje WHERE a.estatus = '" & idProceso & "' AND a.alerta = -2000 ORDER BY a.prioridad DESC, a.id"
                    mensajesDS = consultaSEL(cadSQL)
                    If mensajesDS.Tables(0).Rows.Count > 0 Then
                        Dim enlazado As Boolean = False
                        Dim smtpServer As New SmtpClient()

                        Try
                            smtpServer.Credentials = New Net.NetworkCredential(correo_cuenta, correo_clave)
                            smtpServer.Port = correo_puerto
                            smtpServer.Host = correo_host '"smtp.live.com" '"smtp.gmail.com"
                            smtpServer.EnableSsl = correo_ssl
                            enlazado = True
                        Catch ex As Exception
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".mensajes SET estatus = 'A' WHERE canal = 2 AND estatus = '" & idProceso & "'")
                            smtpServer.Dispose()
                        End Try
                        If enlazado Then
                            For Each elmensaje In mensajesDS.Tables(0).Rows
                                canales = ValNull(elmensaje!correos, "A")
                                Dim correos As String()
                                Dim tempArray As String()
                                Dim totalItems = 0
                                If canales.Length > 0 Then
                                    Dim arreCanales = canales.Split(New Char() {";"c})
                                    For i = LBound(arreCanales) To UBound(arreCanales)
                                        'Redimensionamos el Array temporal y preservamos el valor  
                                        ReDim Preserve correos(totalItems + i)
                                        correos(totalItems + i) = arreCanales(i)
                                    Next
                                    tempArray = correos

                                    Dim x As Integer, y As Integer
                                    Dim z As Integer

                                    For x = 0 To UBound(correos)
                                        z = 0
                                        For y = 0 To UBound(correos) - 1
                                            'Si el elemento del array es igual al array temporal  
                                            If correos(x) = tempArray(z) And y <> x Then
                                                'Entonces Eliminamos el valor duplicado  
                                                correos(y) = ""
                                            End If
                                            z = z + 1
                                        Next y
                                    Next x
                                End If
                                mensajeGenerado = False
                                Dim mail As New MailMessage
                                Try
                                    mail.From = New MailAddress(correo_cuenta)
                                    For i = 0 To UBound(correos)
                                        If correos(i).Length > 0 Then
                                            mail.To.Add(correos(i))
                                        End If
                                    Next i

                                    mail.Subject = ValNull(elmensaje!titulo, "A")
                                    mail.Body = ValNull(elmensaje!texto, "A")
                                    smtpServer.Send(mail)
                                    audiosGen = audiosGen + 1
                                    mensajeGenerado = True
                                    cadSQL = "UPDATE " & rutaBD & ".mensajes SET estatus = 'Z', enviada = NOW() WHERE id = " & elmensaje!id & ";DELETE FROM " & rutaBD & ".checklist_correos WHERE mensaje = " & elmensaje!id
                                    regsAfectados = consultaACT(cadSQL)
                                Catch ex As Exception
                                    audiosNGen = audiosNGen + 1
                                    agregarLOG(ex.Message, 9, nroReporte)
                                Finally
                                    mail.Dispose()
                                End Try
                            Next
                        End If
                        If audiosGen > 0 Or audiosNGen > 0 Then
                            agregarLOG(traduccion(23).Replace("campo_0", audiosGen).Replace("campo_1", audiosNGen))
                        End If
                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".mensajes SET estatus = 'Z' WHERE canal = 2 AND estatus = '" & idProceso & "'")
                        smtpServer.Dispose()
                    End If
                End If
            End If
        End If
        Application.Exit()
    End Sub
    Public Function consultaACT(cadena As String) As Integer
        Dim miConexion = New MySqlConnection

        miConexion.ConnectionString = cadenaConexion

        miConexion.Open()
        consultaACT = 0
        errorBD = ""
        If miConexion.State = ConnectionState.Open Then
            Try
                Dim comandoSQL As MySqlCommand = New MySqlCommand(cadena)
                comandoSQL.Connection = miConexion
                consultaACT = comandoSQL.ExecuteNonQuery()

            Catch ex As Exception
                errorBD = ex.Message
            End Try
        End If

        miConexion.Dispose()
        miConexion.Close()
        miConexion = Nothing
    End Function

    Public Function consultaSEL(cadena As String) As Data.DataSet

        Try
            errorBD = ""
            Dim miConexion = New MySqlConnection

            miConexion.ConnectionString = cadenaConexion

            miConexion.Open()

            If miConexion.State = ConnectionState.Open Then
                Try
                    Dim comandoSQL As MySqlCommand = New MySqlCommand(cadena)
                    comandoSQL.Connection = miConexion
                    Dim adapter As New MySqlDataAdapter(comandoSQL)
                    Dim LaData As New DataSet
                    adapter.Fill(LaData, "miData")

                    Return LaData
                Catch ex As Exception
                    errorBD = ex.Message
                End Try
            End If
            miConexion.Dispose()
            miConexion.Close()
            miConexion = Nothing
        Catch ex As Exception
            errorBD = ex.Message
        End Try

    End Function

    Function ValNull(ByVal ArVar As Object, ByVal arTipo As String) As Object
        Try
            'para columnas vacias sin datos
            If ArVar.Equals(System.DBNull.Value) Then
                Select Case arTipo
                    Case "A"
                        ValNull = ""
                    Case "N"
                        ValNull = 0
                    Case "D"
                        ValNull = 0
                    Case "F"
                        ValNull = CDate("00/00/0000")
                    Case "DT"
                        ValNull = New DateTime(1, 1, 1)
                    Case Else
                        ValNull = ""
                End Select
                Exit Function
            End If

            If Len(ArVar) > 0 Then
                Select Case arTipo
                    Case "A"
                        ValNull = ArVar
                    Case "N"
                        ValNull = Val(ArVar)
                    Case "D"
                        ValNull = CDec(ArVar)
                    Case "F"
                        If ArVar = "0" Then
                            ValNull = ""
                        Else
                            If InStr(ArVar, "/") > 0 Then
                                ValNull = ArVar
                            Else
                                ValNull = Format(ArVar, "dd/MM/yyyy")
                            End If
                        End If
                    Case Else
                        ValNull = ArVar
                End Select
            Else
                Select Case arTipo
                    Case "A"
                        ValNull = ""
                    Case "N"
                        ValNull = 0
                    Case "D"
                        ValNull = 0
                    Case "F"
                        ValNull = CDate("dd/MM/yyyy")
                    Case Else
                        ValNull = ""
                End Select
            End If
        Catch ex As Exception
            Select Case arTipo
                Case "A"
                    ValNull = ""
                Case "N"
                    ValNull = 0
                Case "D"
                    ValNull = 0
                Case "F"
                    ValNull = CDate("00000000")
                Case Else
                    ValNull = " "
            End Select
        End Try
    End Function

    'Function cadenaConexion() As String
    '   cadenaConexion = "server=127.0.0.1;user id=root;password=usbw;port=3307;Convert Zero Datetime=True"
    'cadenaConexion = "server=10.241.241.30;user id=root;password=usbw;port=3307;Convert Zero Datetime=True"

    'End Function

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

    Private Sub agregarLOG(cadena As String, Optional reporte As Integer = 0, Optional tipo As Integer = 0, Optional aplicacion As Integer = 40)
        'If Not be_log_activar Then Exit Sub
        'tipo 0: Info
        'tipo 2: Advertencia
        'tipo 9: Error
        'Dim regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".log (aplicacion, tipo, proceso, texto) VALUES (" & aplicacion & ", " & tipo & ", " & reporte & ", '" & Microsoft.VisualBasic.Strings.Left(cadena, 250) & "')")
    End Sub

    Sub etiquetas()
        Dim general = consultaSEL("SELECT cadena FROM " & rutaBD & ".det_idiomas_back WHERE idioma = " & IIf(be_idioma = 0, 1, be_idioma) & " AND modulo = 3 ORDER BY linea")
        Dim cadenaTrad = ""
        If general.Tables(0).Rows.Count > 0 Then
            For Each cadena In general.Tables(0).Rows
                cadenaTrad = cadenaTrad & cadena!cadena
            Next
        End If
        traduccion = cadenaTrad.Split(New Char() {";"c})
    End Sub
    Sub pruebaMail()
        Dim cadSQL As String = "SELECT idioma_defecto, correo_cuenta, correo_clave, correo_puerto, correo_ssl, correo_host FROM " & rutaBD & ".configuracion"
        Dim readerDS As DataSet = consultaSEL(cadSQL)
        Dim escape_mensaje = ""
        If readerDS.Tables(0).Rows.Count > 0 Then
            Dim reader As DataRow = readerDS.Tables(0).Rows(0)
            be_idioma = ValNull(reader!idioma_defecto, "N")
            etiquetas()
            agregarLOG(traduccion(28), 9, 0)
            Dim correo_cuenta As String = ValNull(reader!correo_cuenta, "A")
            Dim correo_clave As String = ValNull(reader!correo_clave, "A")
            Dim correo_puerto = ValNull(reader!correo_puerto, "A")
            Dim correo_ssl = ValNull(reader!correo_ssl, "A") = "S"
            Dim correo_host = ValNull(reader!correo_host, "A")
            Dim smtpServer As New SmtpClient()
            Try
                smtpServer.Credentials = New Net.NetworkCredential(correo_cuenta, correo_clave)
                smtpServer.Port = correo_puerto
                smtpServer.Host = correo_host
                smtpServer.EnableSsl = correo_ssl
                '
                Dim mail As New MailMessage
                mail.From = New MailAddress(correo_cuenta)
                mail.To.Add(correo_cuenta)
                traduccion(31) = Strings.Replace(traduccion(31), vbCrLf, "")
                traduccion(31) = Strings.Replace(traduccion(31), vbCr, "")
                traduccion(31) = Strings.Replace(traduccion(31), vbLf, "")

                mail.Subject = traduccion(31)
                mail.Body = traduccion(31)
                smtpServer.Send(mail)
                mail.Dispose()
                Dim regsAfectados = consultaACT("UPDATE " & rutaBD & ".configuracion SET correo_prueba = 'N', correo_respuesta = '" & Format(DateAndTime.Now, "yyyy-MMM-dd HH:mm:ss") & ": " & traduccion(28) & "'")
                agregarLOG(traduccion(28), 9, 0)
            Catch ex As Exception
                Dim regsAfectados = consultaACT("UPDATE " & rutaBD & ".configuracion SET correo_prueba = 'N', correo_respuesta = '" & Format(DateAndTime.Now, "yyyy-MMM-dd HH:mm:ss") & ": " & traduccion(29) & ex.Message & "'")
                agregarLOG(traduccion(29), 9, 0)
            End Try
        End If
    End Sub

    Sub reporteOEEporTurno(secuencia As Long)
        Dim cadSQL As String = "SELECT correo_cuenta, correo_clave, correo_puerto, correo_ssl, correo_host, oee_por_turno_cuentas FROM " & rutaBD & ".configuracion"
        Dim readerDS As DataSet = consultaSEL(cadSQL)
        Dim escape_mensaje = ""
        If readerDS.Tables(0).Rows.Count > 0 Then
            Dim reader As DataRow = readerDS.Tables(0).Rows(0)
            Dim correo_cuenta As String = ValNull(reader!correo_cuenta, "A")
            Dim correo_clave As String = ValNull(reader!correo_clave, "A")
            Dim correo_puerto = ValNull(reader!correo_puerto, "A")
            Dim correo_ssl = ValNull(reader!correo_ssl, "A") = "S"
            Dim correo_host = ValNull(reader!correo_host, "A")
            Dim listaCorreos = ValNull(reader!oee_por_turno_cuentas, "A")
            Dim smtpServer As New SmtpClient()

            Try
                smtpServer.Credentials = New Net.NetworkCredential(correo_cuenta, correo_clave)
                smtpServer.Port = correo_puerto
                smtpServer.Host = correo_host
                smtpServer.EnableSsl = correo_ssl
                '
            Catch ex As Exception
                Application.Exit()
                Exit Sub
            End Try

            Dim correos As String()
            Dim tempArray As String()
            Dim totalItems = 0
            If listaCorreos.Length > 0 Then
                Dim arreCanales = listaCorreos.Split(New Char() {";"c})
                For i = LBound(arreCanales) To UBound(arreCanales)
                    'Redimensionamos el Array temporal y preservamos el valor  
                    ReDim Preserve correos(totalItems + i)
                    correos(totalItems + i) = arreCanales(i)
                Next
                tempArray = correos
                totalItems = correos.Length

                Dim x As Integer, y As Integer
                Dim z As Integer

                For x = 0 To UBound(correos)
                    z = 0
                    For y = 0 To UBound(correos) - 1
                        'Si el elemento del array es igual al array temporal  
                        If correos(x) = tempArray(z) And y <> x Then
                            'Entonces Eliminamos el valor duplicado  
                            correos(y) = ""
                        End If
                        z = z + 1
                    Next y
                Next x
            End If

            Dim mail As New MailMessage

            Try
                mail.From = New MailAddress(correo_cuenta)
                For i = 0 To UBound(correos)
                    If correos(i).Length > 0 Then
                        mail.To.Add(correos(i))
                    End If
                Next i

                Dim CadenaMail = ""

                CadenaMail = "SIGMA Reportes: OEE Turno anterior, resumen por máquina" & vbCrLf & vbCrLf

                cadSQL = "SELECT c.turno, COUNT(*) AS tregs, COUNT(DISTINCT c.parte) AS tpartes, COUNT(DISTINCT c.orden) AS tordenes, a.nombre, c.dia, e.nombre AS tlinea, d.nombre AS tequipo, SUM(c.produccion) AS tpiezas, SUM(c.produccion_tc) AS tprod, SUM(c.calidad) AS trecha, SUM(c.calidad_tc) AS tcalidad, SUM(c.paro) AS tparo, SUM(c.tiempo_disponible) AS tdisp FROM " & rutaBD & ".lecturas_cortes c LEFT JOIN " & rutaBD & ".cat_turnos a ON c.turno = a.id LEFT JOIN " & rutaBD & ".cat_maquinas d ON c.equipo = d.id LEFT JOIN " & rutaBD & ".cat_lineas e ON d.linea = e.id WHERE c.turno_secuencia = " & secuencia & " GROUP BY c.dia, c.turno ORDER BY c.dia, c.turno, c.equipo"

                'Se preselecciona la voz
                Dim oeeTA = 0
                Dim miTurno = ""
                Dim mensajesDS As DataSet = consultaSEL(cadSQL)
                If mensajesDS.Tables(0).Rows.Count > 0 Then
                    Dim totalParos = 0
                    Dim totalProduccion = 0
                    Dim totalCalidad = 0
                    Dim totalDisponibilidad = 0
                    Dim cabecera = False
                    For Each elmensaje In mensajesDS.Tables(0).Rows
                        If Not cabecera Then
                            cabecera = True
                            miTurno = ValNull(elmensaje!nombre, "A")
                            CadenaMail = CadenaMail & "Fecha: " & Format(elmensaje!dia, "ddd, dd-MMM-yyyy") & " Turno: " & miTurno & vbCrLf & vbCrLf
                        End If
                        Dim rendimiento = 100
                        Dim calidad = 100
                        Dim disponibilidad = 100
                        Dim oee = 0

                        If elmensaje!tdisp - elmensaje!tparo > 0 Then
                            rendimiento = elmensaje!tprod / (elmensaje!tdisp - elmensaje!tparo) * 100
                        End If
                        If elmensaje!tprod > 0 Then
                            calidad = (1 - elmensaje!tcalidad / elmensaje!tprod) * 100
                        End If
                        If elmensaje!tdisp > 0 Then
                            disponibilidad = (1 - elmensaje!tparo / elmensaje!tdisp) * 100
                        End If

                        totalParos = totalParos + elmensaje!tparo
                        totalProduccion = totalProduccion + elmensaje!tprod
                        totalCalidad = totalCalidad + elmensaje!tcalidad
                        totalDisponibilidad = totalDisponibilidad + elmensaje!tdisp

                        oee = IIf(rendimiento > 100, 100, rendimiento) * calidad * disponibilidad / 10000
                        CadenaMail = CadenaMail & "Máquina: " & ValNull(elmensaje!tequipo, "A") & "/" & ValNull(elmensaje!tlinea, "A") & vbCrLf & "Disponibilidad: " & disponibilidad.ToString("0.###") & "%" & vbCrLf & "Rendimiento: " & rendimiento.ToString("0.###") & "%" & vbCrLf & "Calidad: " & calidad.ToString("0.###") & "%" & vbCrLf & "OEE: " & oee.ToString("0.###") & "%" & vbCrLf
                    Next
                    Dim tRendimiento = 100
                    Dim tCalidad = 100
                    Dim tDisponibilidad = 100
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
                    mail.Subject = "SIGMA Reporting OEE turno " & miTurno & ": " & oeeTA.ToString("0.###") & "%"
                Else
                    mail.Subject = "SIGMA Reporting OEE turno anterior: N/A"
                    CadenaMail = CadenaMail & "No se encontró información para el reporte..."
                End If
                CadenaMail = CadenaMail & vbCrLf & vbCrLf & "Por favor no responda a este correo, se envía desde una cuenta no monirorizada..."


                mail.Body = CadenaMail
                smtpServer.Send(mail)

            Catch ex As Exception

            Finally
                mail.Dispose()
            End Try
            agregarLOG("Se envió el correo resumen de OEE", 9, 0)
        End If
    End Sub

    Sub reporteHoraxHora(tipo As Integer, secuencia As Long, turno As Long)

        Dim cadSQL As String = "SELECT correo_cuenta, correo_clave, correo_puerto, correo_ssl, correo_host, oee_por_turno_cuentas_hxh, oee_por_turno_cuentas_hxh_turno, oee_por_turno_cuentas_hxh_dia, ruta_archivos_enviar FROM " & rutaBD & ".configuracion"
        Dim readerDS As DataSet = consultaSEL(cadSQL)
        Dim escape_mensaje = ""

        If readerDS.Tables(0).Rows.Count > 0 Then
            'Msgbox("Hallo datos de configurcion")
            Dim reader As DataRow = readerDS.Tables(0).Rows(0)
            Dim correo_cuenta As String = ValNull(reader!correo_cuenta, "A")
            Dim correo_clave As String = ValNull(reader!correo_clave, "A")
            Dim correo_puerto = ValNull(reader!correo_puerto, "A")
            Dim correo_ssl = ValNull(reader!correo_ssl, "A") = "S"
            Dim correo_host = ValNull(reader!correo_host, "A")
            Dim listaCorreos = ValNull(reader!oee_por_turno_cuentas_hxh, "A")
            Dim rutaFiles = ValNull(reader!ruta_archivos_enviar, "A")
            If tipo = 2 Then
                listaCorreos = ValNull(reader!oee_por_turno_cuentas_hxh_turno, "A")
            ElseIf tipo = 3 Then
                listaCorreos = ValNull(reader!oee_por_turno_cuentas_hxh_dia, "A")
            End If
            'listaCorreos = "elvismontezuma@hotmail.com"
            Dim smtpServer As New SmtpClient()

            If rutaFiles.Length = 0 Then
                rutaFiles = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            Else
                rutaFiles = Strings.Replace(rutaFiles, "/", "\")
            End If
            If Not My.Computer.FileSystem.DirectoryExists(rutaFiles) Then
                Try
                    My.Computer.FileSystem.CreateDirectory(rutaFiles)
                Catch ex As Exception
                    rutaFiles = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                End Try
            End If

            Try
                smtpServer.Credentials = New Net.NetworkCredential(correo_cuenta, correo_clave)
                smtpServer.Port = correo_puerto
                smtpServer.Host = correo_host
                smtpServer.EnableSsl = correo_ssl
                '
            Catch ex As Exception
                Application.Exit()
                Exit Sub
            End Try
            Dim correos As String()
            Dim tempArray As String()
            Dim totalItems = 0
            If listaCorreos.Length > 0 Then
                Dim arreCanales = listaCorreos.Split(New Char() {";"c})
                For i = LBound(arreCanales) To UBound(arreCanales)
                    'Redimensionamos el Array temporal y preservamos el valor  
                    ReDim Preserve correos(totalItems + i)
                    correos(totalItems + i) = arreCanales(i)
                Next
                tempArray = correos
                totalItems = correos.Length

                Dim x As Integer, y As Integer
                Dim z As Integer

                For x = 0 To UBound(correos)
                    z = 0
                    For y = 0 To UBound(correos) - 1
                        'Si el elemento del array es igual al array temporal  
                        If correos(x) = tempArray(z) And y <> x Then
                            'Entonces Eliminamos el valor duplicado  
                            correos(y) = ""
                        End If
                        z = z + 1
                    Next y
                Next x
            End If

            Dim mail As New MailMessage

            Try
                mail.From = New MailAddress(correo_cuenta)
                For i = 0 To UBound(correos)
                    If correos(i).Length > 0 Then
                        mail.To.Add(correos(i))
                    End If
                Next i

                Dim miTurno = ""

                cadSQL = "SELECT nombre FROM " & rutaBD & ".cat_turnos WHERE id = " & turno
                Dim general As DataSet = consultaSEL(cadSQL)
                If general.Tables(0).Rows.Count > 0 Then
                    miTurno = ValNull(general.Tables(0).Rows(0)!nombre, "A")
                End If


                Dim cadTitulos = "Secuencia,Ruptura,Fecha,Equipo/Máquina,Orden/Lote,Número de parte,Turno,Hora,Inicia,Termina,Tiempo disponible efectivo,Tiempo de paro efectivo,Tiempo ciclo estimado (seg),Plan,Acumulado,Piezas OK,Piezas No OK,Producción (OK y no OK),Diferencia (Plan versus real),Adherencia al plan (%),Producción de la hora anterior,Rendimiento (OEE),FTQ (OEE),Disponibilidad (OEE),OEE,Scrap manual,Tiempo ciclo real (seg),Diferencia en la hora anterior,Plan automatico (S/N),Tripulación,Comentarios,Causa principal de la desviación (si aplica),Responsable de la desviación (1),Responsable de la desviación (2)"

                Dim CadenaMail = ""
                Dim diaAnterior = DateAdd(DateInterval.Day, -1, Now())
                Dim horaAnterior = DateAdd(DateInterval.Hour, -1, Now())

                Dim archivoEnv = "reporteOEE_turno"
                Dim filtroTurno = " AND a.secuencia = " & secuencia
                If turno = 2 Then
                    filtroTurno = " AND a.secuencia >= " & secuencia - 1
                ElseIf turno = 3 Then
                    filtroTurno = " AND a.secuencia >= " & secuencia - 2
                End If

                If tipo = 1 Then
                    filtroTurno = " AND a.secuencia = " & secuencia
                End If

                cadSQL = "SELECT a.*, b.nombre AS nequipo, i.nombre AS nparte, c.numero, IF(a.disponible = 3599, 3600, a.disponible) AS disponible2, IF(a.mantto = 3599, 3600, a.mantto) AS mantto2, IF(a.tiempo = 3599, 3600, a.tiempo) AS tiempo2, IF(a.paro= 3599, 3600, a.paro) AS paro2, d.nombre AS nturno, e.nombre AS ntripulacion, f.nombre AS ncausa, g.nombre AS nresp1, h.nombre AS nresp2 FROM " & rutaBD & ".horaxhora a LEFT JOIN " & rutaBD & ".cat_maquinas b ON a.equipo = b.id LEFT JOIN " & rutaBD & ".lotes c ON a.lote = c.id LEFT JOIN " & rutaBD & ".cat_turnos d ON a.turno = d.id LEFT JOIN " & rutaBD & ".cat_tripulacion e ON a.tripulacion_inicial = e.id LEFT JOIN " & rutaBD & ".cat_generales f ON a.causa = f.id LEFT JOIN " & rutaBD & ".cat_generales g ON a.responsable = g.id LEFT JOIN " & rutaBD & ".cat_generales h ON a.responsable2 = h.id LEFT JOIN " & rutaBD & ".cat_partes i ON a.parte = i.id WHERE a.estatus = 'Z' " & filtroTurno & " ORDER BY a.dia, a.hora, a.id"
                CadenaMail = "SIGMA Reportes: Resumen del Hora por hora (Corte por Turno) " & miTurno & vbCrLf & vbCrLf
                If tipo = 2 Then
                    CadenaMail = "SIGMA Reportes: Resumen del Hora por hora (Corte por hora)" & Format(horaAnterior, "ddd, dd-MMM-yyyy HH") & vbCrLf & vbCrLf
                    archivoEnv = "reporteOEE_hora"
                ElseIf tipo = 3 Then
                    CadenaMail = "SIGMA Reportes: Resumen del Hora por hora (Corte por día) " & Format(diaAnterior, "ddd, dd-MMM-yyyy") & vbCrLf & vbCrLf
                    archivoEnv = "reporteOEE_dia"
                End If

                'Se preselecciona la voz
                Try
                    File.Delete(rutaFiles & "\" & archivoEnv & ".csv")
                    File.Delete(rutaFiles & "\" & archivoEnv & ".csv")

                Catch ex As Exception

                End Try

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

                Dim cabecera = False

                Dim linea As Integer = 0

                Dim objWriter As New System.IO.StreamWriter(rutaFiles & "\" & archivoEnv & ".csv", False, System.Text.Encoding.UTF8)
                If tipo = 1 Then
                    objWriter.WriteLine("SIGMA Reportes: Resumen del Hora por hora (Corte por Turno) => " & miTurno)
                ElseIf tipo = 2 Then
                    objWriter.WriteLine("SIGMA Reportes: Resumen del Hora por hora (Corte por hora) => " & Format(horaAnterior, "ddd, dd-MMM-yyyy HH"))
                ElseIf tipo = 3 Then
                    objWriter.WriteLine("SIGMA Reportes: Resumen del Hora por hora (Corte por día) => " & Format(diaAnterior, "ddd, dd-MMM-yyyy"))
                End If
                objWriter.WriteLine("Generado en: " & Format(Now(), "ddd dd-MMM-yyyy HH:mm:ss"))
                objWriter.WriteLine(cadTitulos)
                Dim oeeTA = 0
                Dim mensajesDS As DataSet = consultaSEL(cadSQL)
                If mensajesDS.Tables(0).Rows.Count > 0 Then


                    For Each elmensaje In mensajesDS.Tables(0).Rows
                        linea = linea + 1
                        If Not cabecera Then
                            cabecera = True
                            If tipo = 1 Then
                                CadenaMail = CadenaMail & "Fecha: " & Format(elmensaje!dia, "ddd, dd-MMM-yyyy") & " Turno: " & ValNull(elmensaje!nturno, "A") & vbCrLf
                            ElseIf tipo = 2 Then
                                CadenaMail = CadenaMail & "Fecha: " & Format(elmensaje!dia, "ddd, dd-MMM-yyyy") & " Hora: " & Format(elmensaje!hora, "00") & vbCrLf
                            ElseIf tipo = 3 Then
                                CadenaMail = CadenaMail & "Fecha: " & Format(elmensaje!dia, "ddd, dd-MMM-yyyy") & vbCrLf
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
                        If elmensaje!plan > 0 Then
                            adherencia = elmensaje!buenas / elmensaje!plan * 100
                        End If
                        oee = IIf(rendimiento > 100, 100, rendimiento) * calidad * disponibilidad / 10000
                        Dim cadReporte = ""
                        Try
                            cadReporte = linea & "," & Chr(34) & IIf(elmensaje!ruptura = 0, "Hora", IIf(elmensaje!ruptura = 1, "Turno", IIf(elmensaje!ruptura = 2, "Día", IIf(elmensaje!ruptura = 3, "Número de parte", IIf(elmensaje!ruptura = 4, "Número de lote/Orden", "Tiempo Ciclo"))))) & Chr(34) & "," & Chr(34) & Format(elmensaje!dia, "dd-MMM-yyyy") & Chr(34) & "," & Chr(34) & ValNull(elmensaje!nequipo, "A") & Chr(34) & "," & Chr(34) & ValNull(elmensaje!numero, "A") & Chr(34) & "," & Chr(34) & ValNull(elmensaje!nparte, "A") & Chr(34) & "," & Chr(34) & ValNull(elmensaje!nturno, "A") & Chr(34) & "," & Chr(34) & elmensaje!hora & Chr(34) & "," & Chr(34) & elmensaje!desde & Chr(34) & "," & Chr(34) & elmensaje!hasta & Chr(34) & "," & Chr(34) & elmensaje!tiempo2 & Chr(34) & "," & Chr(34) & elmensaje!paro2 & Chr(34) & "," & Chr(34) & elmensaje!tc & Chr(34) & "," & Chr(34) & elmensaje!plan & Chr(34) & "," & Chr(34) & elmensaje!plan_van & Chr(34) & "," & Chr(34) & elmensaje!buenas - elmensaje!malas & Chr(34) & "," & Chr(34) & elmensaje!malas & Chr(34) & "," & Chr(34) & elmensaje!buenas & Chr(34) & "," & Chr(34) & (elmensaje!plan - elmensaje!buenas) * -1 & Chr(34) & "," & Chr(34) & adherencia & Chr(34) & "," & Chr(34) & (elmensaje!buenas_vienen) & Chr(34) & "," & Chr(34) & rendimiento & Chr(34) & "," & Chr(34) & calidad & Chr(34) & "," & Chr(34) & disponibilidad & Chr(34) & "," & Chr(34) & oee & Chr(34) & "," & Chr(34) & elmensaje!scrap & Chr(34) & "," & Chr(34) & tcReal & Chr(34) & "," & Chr(34) & elmensaje!diferencia_vienen & Chr(34) & "," & Chr(34) & IIf(elmensaje!tipo = 0, "No", "Si") & Chr(34) & "," & Chr(34) & ValNull(elmensaje!ntripulacion, "A") & Chr(34) & "," & Chr(34) & ValNull(elmensaje!comentarios, "A") & Chr(34) & "," & Chr(34) & ValNull(elmensaje!ncausa, "A") & Chr(34) & "," & Chr(34) & ValNull(elmensaje!nresp1, "A") & Chr(34) & "," & Chr(34) & ValNull(elmensaje!nresp2, "A") & Chr(34)

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
                    If tipo = 1 Then
                        mail.Subject = "SIGMA Reportes Hora por hora (corte al turno) " & miTurno & ": " & oeeTA.ToString("0.###") & "%"

                    ElseIf tipo = 2 Then
                        mail.Subject = "SIGMA Reportes Hora por hora (corte por hora) " & Format(diaAnterior, "yyyy/MM/dd") & " hora: " & Format(diaAnterior, "HH") & ": " & oeeTA.ToString("0.###") & "%"

                    ElseIf tipo = 3 Then
                        mail.Subject = "SIGMA Reportes Hora por hora (corte por día) " & Format(diaAnterior, "yyyy/MM/dd") & ": " & oeeTA.ToString("0.###") & "%"

                    End If
                    agregarLOG("Se envió el correo resumen de OEE", 9, 0)
                Else
                    If tipo = 1 Then
                        mail.Subject = "SIGMA Reporting Hora por hora (corte al turno) " & miTurno & ": Sin información"

                    ElseIf tipo = 2 Then
                        mail.Subject = "SIGMA Reporting Hora por hora (corte por hora) " & Format(diaAnterior, "yyyy/MM/dd") & " hora: " & Format(diaAnterior, "HH") & ": Sin información"

                    ElseIf tipo = 3 Then
                        mail.Subject = "SIGMA Reporting Hora por hora (corte por día) " & Format(diaAnterior, "yyyy/MM/dd") & ": Sin información"

                    End If
                    CadenaMail = CadenaMail & "No se encontró información para el reporte..."
                    objWriter.WriteLine("No se encontró informacion para el reporte...")
                    agregarLOG("No habían datos para enviar el correo resumen de OEE", 9, 0)

                End If
                CadenaMail = CadenaMail & vbCrLf & vbCrLf & "Por favor no responda a este correo, se envía desde una cuenta no monitoreada..."
                objWriter.WriteLine("FIn del reporte...")
                objWriter.Close()


                'Msgbox("genero primer archivo")
                Dim archivo As Attachment = New Attachment(rutaFiles & "\" & archivoEnv & ".csv")
                mail.Attachments.Add(archivo)

                cadSQL = "SELECT a.equipo, b.nombre AS nequipo, a.dia, a.hora, MIN(a.desde) AS desde, MAX(a.hasta) AS hasta, SUM(a.plan) AS plan, MAX(a.plan_van) AS plan_van, SUM(a.scrap) AS scrap, GROUP_CONCAT(i.nombre SEPARATOR '~~') AS nparte, GROUP_CONCAT(c.numero SEPARATOR '~~') AS numero, SUM(IF(a.disponible = 3599, 3600, a.disponible)) AS disponible2, SUM(IF(a.mantto = 3599, 3600, a.mantto)) AS mantto2, SUM(IF(a.tiempo = 3599, 3600, a.tiempo)) AS tiempo2, SUM(IF(a.paro= 3599, 3600, a.paro)) AS paro2, d.nombre AS nturno, GROUP_CONCAT(f.nombre  SEPARATOR '~~') AS ncausa, GROUP_CONCAT(g.nombre SEPARATOR '~~') AS nresp1, GROUP_CONCAT(h.nombre SEPARATOR '~~') AS nresp2, GROUP_CONCAT(a.comentarios SEPARATOR '~~') AS comentarios, SUM(a.buenas) AS buenas, SUM(a.buenas_tc) AS buenas_tc, SUM(a.malas) AS malas, SUM(a.malas_tc) AS malas_tc FROM " & rutaBD & ".horaxhora a LEFT JOIN " & rutaBD & ".cat_maquinas b ON a.equipo = b.id LEFT JOIN " & rutaBD & ".lotes c ON a.lote = c.id LEFT JOIN " & rutaBD & ".cat_turnos d ON a.turno = d.id LEFT JOIN " & rutaBD & ".cat_tripulacion e ON a.tripulacion_inicial = e.id LEFT JOIN " & rutaBD & ".cat_generales f ON a.causa = f.id LEFT JOIN " & rutaBD & ".cat_generales g ON a.responsable = g.id LEFT JOIN " & rutaBD & ".cat_generales h ON a.responsable2 = h.id LEFT JOIN " & rutaBD & ".cat_partes i ON a.parte = i.id WHERE a.estatus = 'Z' " & filtroTurno & " GROUP BY a.equipo, a.dia, a.hora"

                ''''Construcción del mail
                If tipo = 1 Then
                    CadenaMail = "SIGMA Reportes: Resumen del Hora por hora (Corte por Turno)" & vbCrLf & vbCrLf

                ElseIf tipo = 2 Then
                    CadenaMail = "SIGMA Reportes: Resumen del Hora por hora (Corte por Hora)" & vbCrLf & vbCrLf
                    archivoEnv = "reporteOEE_hora"
                ElseIf tipo = 3 Then
                    CadenaMail = "SIGMA Reportes: Resumen del Hora por hora (Corte por día)" & vbCrLf & vbCrLf
                    archivoEnv = "reporteOEE_dia"
                End If

                Dim myBuilder As System.Text.StringBuilder = New System.Text.StringBuilder()

                myBuilder.Append("<html xmlns='http://www.w3.org/1999/xhtml'>")
                myBuilder.Append("<head>")
                myBuilder.Append("</head>")
                myBuilder.Append("<body>")
                If tipo = 1 Then
                    myBuilder.Append("SIGMA Reportes: Resumen del Hora por hora (Corte por Turno)")
                ElseIf tipo = 2 Then
                    myBuilder.Append("SIGMA Reportes: Resumen del Hora por hora (Corte por hora)")
                ElseIf tipo = 3 Then
                    myBuilder.Append("SIGMA Reportes: Resumen del Hora por hora (Corte por día)")
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
                                oeeTA = IIf(tRendimiento > 100, 100, tRendimiento) * tCalidad * tDisponibilidad / 10000

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
                            myBuilder.Append("LINEA: " & ValNull(elmensaje!nequipo, "A"))
                            myBuilder.Append("</td>")
                            myBuilder.Append("<td align='left' valign='top' colspan='7'style='border:solid 1px DarkGray'>")
                            myBuilder.Append("DÍA: " & Format(Now(), "ddd, dd-MMM-yyyy"))
                            myBuilder.Append("</td>")
                            '''
                            myBuilder.Append("</tr>")

                            myBuilder.Append("<tr align='left' valign='top' style='background-color:#DCDCDC;border: solid 1px DarkGray;font-weight: 600'>")
                            '''
                            myBuilder.Append("<td align='center' valign='top' style='border: solid 1px DarkGray;'>")
                            myBuilder.Append("HORA")
                            myBuilder.Append("</td>")
                            myBuilder.Append("<td align='left' valign='top' style='border: solid 1px DarkGray;'>")
                            myBuilder.Append("TURNO")
                            myBuilder.Append("</td>")
                            myBuilder.Append("<td align='center' valign='top' style='border: solid 1px DarkGray;'>")
                            myBuilder.Append("DISPONIBLE")
                            myBuilder.Append("</td>")
                            myBuilder.Append("<td align='center' valign='top' colspan='2' style='border: solid 1px DarkGray;'>")
                            myBuilder.Append("(A) PIEZAS/OBJETIVO")
                            myBuilder.Append("</td>")
                            myBuilder.Append("<td align='center' valign='top' colspan='2' style='border: solid 1px DarkGray;'>")
                            myBuilder.Append("(B) PIEZAS REALES")
                            myBuilder.Append("</td>")
                            myBuilder.Append("<td align='center' valign='top' style='border: solid 1px DarkGray;'>")
                            myBuilder.Append("ADHERENCIA (B/A)")
                            myBuilder.Append("</td>")
                            myBuilder.Append("<td align='center' valign='top' colspan='2' style='border: solid 1px DarkGray;'>")
                            myBuilder.Append("(C) SCRAP MANUAL")
                            myBuilder.Append("</td>")
                            'myBuilder.Append("<td align='center' valign='top' colspan='2' style='border: solid 1px DarkGray;'>")
                            'myBuilder.Append("SCRAP (MANUAL)")
                            'myBuilder.Append("</td>")
                            myBuilder.Append("<td align='center' valign='top' colspan='2' style='border: solid 1px DarkGray;'>")
                            myBuilder.Append("TIEMPO PARO (D)")
                            myBuilder.Append("</td>")
                            myBuilder.Append("<td align='center' valign='top' style='border: solid 1px DarkGray;'>")
                            myBuilder.Append("OEE")
                            myBuilder.Append("</td>")
                            myBuilder.Append("<td align='center' valign='top' style='border: solid 1px DarkGray;'>")
                            myBuilder.Append("COMENTARIOS")
                            myBuilder.Append("</td>")
                            '''
                            myBuilder.Append("</tr>")
                        End If
                        Dim rendimiento As Double = 100
                        Dim hayRendimiento = False
                        Dim calidad As Double = 100
                        Dim scrap As Double = 0
                        Dim scrapManual = 0
                        Dim paros As Double = 0
                        Dim disponibilidad As Double = 100
                        Dim haydisponibilidad = False
                        Dim oee As Double = 0

                        If elmensaje!tiempo2 - elmensaje!paro2 > 0 Then
                            rendimiento = elmensaje!buenas_tc / (elmensaje!tiempo2 - elmensaje!paro2) * 100
                            hayRendimiento = True
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
                            haydisponibilidad = True
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
                        oee = IIf(rendimiento > 100, 100, rendimiento) * calidad * disponibilidad / 10000

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
                        myBuilder.Append(IIf(hayRendimiento And haydisponibilidad, Math.Round(oee, 0), "-"))
                        myBuilder.Append("</td>")

                        Dim cadComentarios As String = Strings.Replace(ValNull(elmensaje!comentarios, "A"), "~~", "<br>")
                        Dim causa As String = Strings.Replace(ValNull(elmensaje!ncausa, "A"), "~~", "<br>")
                        Dim resp1 As String = Strings.Replace(ValNull(elmensaje!nresp1, "A"), "~~", "<br>")
                        Dim resp2 As String = Strings.Replace(ValNull(elmensaje!nresp2, "A"), "~~", "<br>")
                        If Not IsNothing(causa) Then
                            If causa.Length > 0 Then
                                cadComentarios = cadComentarios & "<br>Causa principal: " & causa
                            End If
                        End If
                        If Not IsNothing(resp1) Then
                            If resp1.Length > 0 Then
                                cadComentarios = cadComentarios & "<br>Responsable (1): " & resp1
                            End If

                        End If
                        If Not IsNothing(resp2) Then
                            If resp2.Length > 0 Then
                                cadComentarios = cadComentarios & "<br>Responsable (2): " & resp2
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
                        Dim tEficiencia As Double = 100
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
                        oeeTA = IIf(tRendimiento > 100, 100, tRendimiento) * tCalidad * tDisponibilidad / 10000

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

                Else
                    If tipo = 1 Then
                        mail.Subject = "SIGMA Reporting Hora por hora (corte al turno) " & miTurno & ": Sin información"

                    ElseIf tipo = 2 Then
                        mail.Subject = "SIGMA Reporting Hora por hora (corte por hora) " & Format(horaAnterior, "yyyy/MM/dd") & " hora: " & Format(horaAnterior, "HH") & ": Sin información"

                    ElseIf tipo = 3 Then
                        mail.Subject = "SIGMA Reporting Hora por hora (corte por día) " & Format(diaAnterior, "yyyy/MM/dd") & ": Sin información"

                    End If
                    myBuilder.Append("No se encontró información para el reporte...")

                End If
                If requiereResumen Then
                    'MsgBox("resumen")
                    Dim tRendimiento As Double = 100
                    Dim tEficiencia As Double = 100
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
                If tipo = 1 Then
                    mail.Subject = "SIGMA Reportes Hora por hora (corte al turno) " & miTurno & " => " & oeeTA.ToString("0.###") & "%"

                ElseIf tipo = 2 Then
                    mail.Subject = "SIGMA Reportes Hora por hora (corte por hora) " & Format(horaAnterior, "yyyy/MM/dd") & " hora: " & Format(horaAnterior, "HH") & " => " & oeeTA.ToString("0.###") & "%"

                ElseIf tipo = 3 Then
                    mail.Subject = "SIGMA Reportes Hora por hora (corte por día) " & Format(diaAnterior, "yyyy/MM/dd") & " => " & oeeTA.ToString("0.###") & "%"
                End If
                agregarLOG("Se envió el correo resumen de OEE", 9, 0)
                myBuilder.Append("<br>")
                myBuilder.Append("Por favor no responda a este correo, se envía desde una cuenta no monitoreada...")
                myBuilder.Append("</body>")
                myBuilder.Append("</html>")

                mail.IsBodyHtml = True
                mail.Body = myBuilder.ToString()
                smtpServer.Send(mail)
                agregarLOG("Mail enviado tipo: " & tipo, 9, 0)


            Catch ex As Exception
                agregarLOG("Error al enviar el correo tipo: " & tipo & " => " & ex.Message, 9, 0)
                'MsgBox("error: " & ex.Message)
            Finally
                mail.Dispose()
            End Try

        End If
    End Sub



End Module
