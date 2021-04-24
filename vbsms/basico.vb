Imports MySql.Data.MySqlClient
Imports System.Security.Cryptography
Imports System.Text
Imports System.IO
Imports System.Data
Imports System.Net.Mail
Imports System.Net
Imports Twilio
Imports Twilio.Rest.Api.V2010.Account
Imports Twilio.Types


Module basico
    Public errorBD As String
    Public horaDesde As DateTime
    Public ultimaFalla
    Public autenticado As Boolean
    Public cadenaConexion As String
    Public be_log_activar As Boolean = False
    Public rutaBD As String = "sigma"
    Public cliente As String = ""
    Public traduccion As String()
    Public be_idioma
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="argumentos"></param>
    Sub Main(argumentos As String())
        If Process.GetProcessesByName _
          (Process.GetCurrentProcess.ProcessName).Length > 1 Then
        ElseIf argumentos.Length = 0 Then
            MsgBox("String connection missing", MsgBoxStyle.Critical, "SIGMA")
        Else
            cadenaConexion = argumentos(0)
            'cadenaConexion = "server=127.0.0.1;user id=root;password=usbw;port=3307;Convert Zero Datetime=True"
            Dim idProceso = Process.GetCurrentProcess.Id

            Dim mensajesDS As DataSet
            Dim eMensaje = ""
            Dim eTitulo = ""
            Dim audiosGen = 0
            Dim audiosNGen = 0
            Dim mTotal = 0
            Dim miError As String = ""
            Dim optimizar As Boolean = False
            Dim mantenerPrioridad As Boolean = False
            Dim rutaSMS As String = ""
            Dim mensajeGenerado As Boolean = False
            Dim be_alarmas_sms As Boolean = False
            Dim regsAfectados = 0

            Dim canales As String = ""
            Dim laLinea As String = ""
            Dim laMaquina As String = ""
            Dim laArea As String = ""
            Dim laFalla As String = ""
            Dim tiempo As String = ""
            Dim nroReporte As Integer = 0
            Dim cadSQL As String = "SELECT * FROM " & rutaBD & ".configuracion"
            Dim readerDS As DataSet = consultaSEL(cadSQL)
            If readerDS.Tables(0).Rows.Count > 0 Then
                Dim reader As DataRow = readerDS.Tables(0).Rows(0)
                be_idioma = ValNull(reader!idioma_defecto, "N")
                etiquetas()
                optimizar = ValNull(reader!optimizar_sms, "A") = "S"
                be_log_activar = ValNull(reader!be_log_activar, "A") = "S"
                mantenerPrioridad = ValNull(reader!mantener_prioridad, "A") = "S"
                be_alarmas_sms = ValNull(reader!be_alarmas_sms, "A") = "S"
                rutaSMS = ValNull(reader!ruta_sms, "A")
            End If
            If be_alarmas_sms Then
                If rutaSMS.Length = 0 Then
                    rutaSMS = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                Else
                    rutaSMS = Strings.Replace(rutaSMS, "/", "\")
                End If
                If Not My.Computer.FileSystem.DirectoryExists(rutaSMS) Then
                    Try
                        My.Computer.FileSystem.CreateDirectory(rutaSMS)
                    Catch ex As Exception
                        rutaSMS = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    End Try
                End If
                regsAfectados = consultaACT("UPDATE " & rutaBD & ".mensajes SET estatus = '" & idProceso & "' WHERE canal = 1 AND estatus = 'E' AND alerta <> -1000")
                Dim agrupado As Boolean = True
                'heb/ se agrega el campo mmcall
                Dim campoMMCALL = ""
                If cliente = "HEB" Then
                    campoMMCALL = "d.mmcall, d.referencia, "
                End If
                If Not optimizar Then
                    cadSQL = "SELECT a.alarma AS nmensaje, a.tipo AS tmensaje, a.id, a.proceso, d.telefonos, " & campoMMCALL & "a.prioridad, z.texto, z.titulo, 1 AS cuenta FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".mensajes_procesados z ON a.id = z.mensaje INNER JOIN " & rutaBD & ".cat_alertas b on a.alerta = b.id INNER JOIN " & rutaBD & ".cat_distribucion d ON a.lista = d.id AND b.estatus = 'A' WHERE a.estatus = '" & idProceso & "' ORDER BY a.prioridad DESC, a.id"
                    agrupado = False
                ElseIf mantenerPrioridad Then
                    cadSQL = "SELECT a.lista, 0 as proceso, 0 AS nmensaje, 0 AS tmensaje, a.prioridad, " & campoMMCALL & "b.telefonos, COUNT(*) AS cuenta, MAX(a.id) AS id FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".mensajes_procesados z ON a.id = z.mensaje INNER JOIN " & rutaBD & ".cat_distribucion b ON a.lista = b.id AND b.estatus = 'A' INNER JOIN " & rutaBD & ".cat_alertas c on a.alerta = c.id WHERE a.estatus = '" & idProceso & "' GROUP BY nmensaje, tmensaje, a.prioridad, a.lista, " & campoMMCALL & "b.telefonos ORDER BY prioridad DESC"
                Else
                    cadSQL = "SELECT a.lista, 0 as proceso, 0 AS nmensaje, 0 AS tmensaje, " & campoMMCALL & "b.telefonos, 0 AS prioridad, COUNT(*) AS cuenta, MAX(a.id) AS id FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".mensajes_procesados z ON a.id = z.mensaje INNER JOIN " & rutaBD & ".cat_distribucion b ON a.lista = b.id AND b.estatus = 'A' INNER JOIN " & rutaBD & ".cat_alertas c on a.alerta = c.id WHERE a.estatus = '" & idProceso & "' GROUP BY nmensaje, tmensaje, a.lista, " & campoMMCALL & "b.telefonos"

                End If
                'Se preselecciona la voz
                mensajesDS = consultaSEL(cadSQL)
                Dim nMensaje = 0
                Dim tMensaje = 0

                If mensajesDS.Tables(0).Rows.Count > 0 Then
                    For Each elmensaje In mensajesDS.Tables(0).Rows
                        canales = ValNull(elmensaje!telefonos, "A")
                        nMensaje = ValNull(elmensaje!nmensaje, "N")
                        tMensaje = ValNull(elmensaje!tmensaje, "N")
                        If tMensaje = 8 Then nMensaje = 0 'No se marca la llamada de resolución
                        eMensaje = ""
                        If elmensaje!cuenta > 1 Then
                            eMensaje = "HAY " & elmensaje!cuenta & " MENSAJES POR ATENDER"
                            If mantenerPrioridad And elmensaje!prioridad > 0 Then
                                eMensaje = "HAY " & elmensaje!cuenta & " MENSAJES PRIORITARIOS"
                            End If
                        ElseIf agrupado Then
                            cadSQL = "SELECT * FROM " & rutaBD & ".mensajes_procesados WHERE mensaje = " & elmensaje!id
                            Dim mensajeAgrupado As DataSet = consultaSEL(cadSQL)
                            If mensajeAgrupado.Tables(0).Rows.Count > 0 Then
                                eMensaje = ValNull(mensajeAgrupado.Tables(0).Rows(0)!texto, "A")
                            End If
                        Else
                            eMensaje = ValNull(elmensaje!texto, "A")
                        End If
                        If eMensaje.Length > 0 And canales.Length > 0 Then
                            Dim antes As String = "ÃÀÁÄÂÈÉËÊÌÍÏÎÒÓÖÔÙÚÜÛãàáäâèéëêìíïîòóöôùúüûÑñÇç"
                            Dim ahora As String = "AAAAAEEEEIIIIOOOOUUUUaaaaaeeeeiiiioooouuuunncc"
                            For i = 0 To antes.Length - 1
                                eMensaje = Replace(eMensaje, antes(i), ahora(i))
                            Next
                            eMensaje = Replace(eMensaje, ";", " ")
                            eMensaje = Replace(eMensaje, "\", "-")
                            eMensaje = Replace(eMensaje, "/", "-")

                            eMensaje = Replace(eMensaje, System.Environment.NewLine, " ")
                            eMensaje = Replace(eMensaje, "[90]", "")

                            Dim telefonos As String()
                            Dim tempArray As String()
                            Dim totalItems = 0

                            If canales.Length > 0 Then
                                Dim arreCanales = canales.Split(New Char() {";"c})
                                For i = LBound(arreCanales) To UBound(arreCanales)
                                    'Redimensionamos el Array temporal y preservamos el valor  


                                    If arreCanales(i).Length > 0 Then
                                        'Redimensionamos el Array temporal y preservamos el valor  
                                        ReDim Preserve telefonos(totalItems + i)
                                        telefonos(totalItems + i) = Strings.Trim(arreCanales(i))
                                    End If


                                Next
                                tempArray = telefonos
                                totalItems = telefonos.Length

                                Dim x As Integer, y As Integer
                                Dim z As Integer
                                For x = 0 To UBound(telefonos)
                                    z = 0
                                    For y = 0 To UBound(telefonos) - 1
                                        'Si el elemento del array es igual al array temporal  
                                        If telefonos(x) = tempArray(z) And y <> x Then
                                            'Entonces Eliminamos el valor duplicado  
                                            telefonos(y) = ""
                                        End If
                                        z = z + 1
                                    Next y
                                Next x
                                canales = ""
                            End If
                            eMensaje = Microsoft.VisualBasic.Strings.Left(eMensaje, 120).Trim
                            mensajeGenerado = False
                            'heb/ se agrega el if del WS
                            Dim validado As Boolean = True
                            Dim url As String = ""
                            Dim ws As String = ""
                            Dim metodo As String = ""
                            Dim ct As String = ""
                            Dim autorizacion As String = ""
                            Dim body As String = ""

                            If cliente = "HEB2" Then
                                Dim webService As String = ValNull(elmensaje!mmcall, "A")
                                webService = Strings.Replace(webService, vbCrLf, "")
                                webService = Strings.Replace(webService, vbCr, "")
                                webService = Strings.Replace(webService, vbLf, "")
                                If webService.Length > 0 Then
                                    Dim campoValor
                                    Dim partesWS = webService.Split(New Char() {";"c})
                                    If partesWS.Length = 5 Then
                                        campoValor = partesWS(0).Split(New Char() {"="c})
                                        If Strings.UCase(campoValor(0)).trim = "URL" Then
                                            url = Strings.Trim(campoValor(1))
                                        End If
                                        campoValor = partesWS(1).Split(New Char() {"="c})
                                        If Strings.UCase(campoValor(0)).trim = "METHOD" Then
                                            metodo = Strings.Trim(campoValor(1))
                                        End If
                                        campoValor = partesWS(2).Split(New Char() {"="c})
                                        If Strings.UCase(campoValor(0)).trim = "CONTENT-TYPE" Then
                                            ct = Strings.Trim(campoValor(1))
                                        End If
                                        campoValor = partesWS(3).Split(New Char() {"="c})
                                        If Strings.UCase(campoValor(0)).trim = "AUTHORIZATION" Then
                                            autorizacion = Strings.Trim(campoValor(1))
                                            If campoValor.length > 2 Then
                                                For i = 2 To campoValor.length - 1
                                                    autorizacion = autorizacion & "=" & Strings.Trim(campoValor(i))
                                                Next
                                            End If

                                        End If
                                        campoValor = partesWS(4).Split(New Char() {"="c})
                                        If Strings.UCase(campoValor(0)) = "BODY" Then
                                            body = Strings.Trim(campoValor(1))
                                            body = Strings.Replace(body, "~MENSAJE~", """" & eMensaje & """")
                                        End If
                                    Else
                                        validado = False
                                    End If
                                Else
                                    validado = False
                                End If
                            ElseIf cliente = "HEB" Then
                                If validarURI("massive.chattigo.com/message/login") Then
                                    Dim response = ""
                                    Try
                                        Dim ntemplate = Strings.Left(elmensaje!texto, Strings.InStr(elmensaje!texto, ";") - 1)
                                        Dim nCaja = Strings.Mid(elmensaje!texto, Strings.InStr(elmensaje!texto, ";") + 1)
                                        Dim request As HttpWebRequest
                                        Dim targetURI As New Uri("https://massive.chattigo.com/message/login")
                                        request = HttpWebRequest.Create(targetURI)
                                        body = "{" & Chr(34) & "username" & Chr(34) & ":" & Chr(34) & "ggalan@hebmex.com" & Chr(34) & ", " & Chr(34) & "password" & Chr(34) & ":" & Chr(34) & "ApiMassive1" & Chr(34) & "}"
                                        'body = Strings.Replace(body, "~NUMERO~", "52" & telefonos(i))
                                        request.ContentLength = Encoding.UTF8.GetBytes(body).Length
                                        request.Method = "POST"
                                        'request.ContentType = ct
                                        'request.Headers.Add(HttpRequestHeader.Authorization, autorizacion)

                                        Using requestStream = request.GetRequestStream
                                            requestStream.Write(Encoding.UTF8.GetBytes(body), 0, body.Length)
                                            requestStream.Close()

                                            Using responseStream = request.GetResponse.GetResponseStream
                                                Using reader As New StreamReader(responseStream)
                                                    response = reader.ReadToEnd()
                                                End Using
                                            End Using
                                        End Using
                                        Dim cadTelefonos = ""
                                        For i = 0 To UBound(telefonos)
                                            If Strings.Trim(telefonos(i)).Length > 0 Then
                                                cadTelefonos = cadTelefonos & "{" & Chr(34) & "destination" & Chr(34) & ": " & Chr(34) & telefonos(i) & Chr(34) & "},"
                                            End If
                                        Next i
                                        cadTelefonos = Strings.Left(cadTelefonos, Len(cadTelefonos) - 1)
                                        If response.Length > 0 Then
                                            Dim request2 As HttpWebRequest
                                            Dim targetURI2 As New Uri("https://massive.chattigo.com/message/inbound")
                                            request = HttpWebRequest.Create(targetURI2)
                                            body = "{" & Chr(34) & "id" & Chr(34) & ":" & Chr(34) & "404" & Chr(34) & ", " & Chr(34) & "did" & Chr(34) & ":" & Chr(34) & "5218182528316" & Chr(34) & ", " & Chr(34) & "type" & Chr(34) & ":" & Chr(34) & "HSM" & Chr(34) & "," & Chr(34) & "channel" & Chr(34) & ":" & Chr(34) & "WHATSAPP" & Chr(34) & "," & Chr(34) & "hsm" & Chr(34) & ": {" & Chr(34) & "destinations" & Chr(34) & ": [" & cadTelefonos & "]," & Chr(34) & "namespace" & Chr(34) & ":" & Chr(34) & "956443a5_36e6_47d5_9aa0_60f6e187ac66" & Chr(34) & "," & Chr(34) & "template" & Chr(34) & ":" & Chr(34) & nTemplate & Chr(34) & "," & Chr(34) & "parameters" & Chr(34) & ": [" & Chr(34) & nCaja & Chr(34) & "]," & Chr(34) & "languageCode" & Chr(34) & ":" & Chr(34) & "es" & Chr(34) & "}}"
                                            request.ContentLength = Encoding.UTF8.GetBytes(body).Length
                                            request.Method = "POST"
                                            request.ContentType = "application/x-www-form-urlencoded"
                                            request.KeepAlive = True
                                            Dim dosP = Strings.InStr(response, ":") + 2
                                            request.Headers("Authorization") = "Bearer " & Mid(response, dosP, response.Length - dosP - 1)

                                            Using requestStream = request.GetRequestStream
                                                requestStream.Write(Encoding.UTF8.GetBytes(body), 0, body.Length)
                                                requestStream.Close()

                                                Using responseStream = request.GetResponse.GetResponseStream
                                                    Using reader As New StreamReader(responseStream)
                                                        response = reader.ReadToEnd()
                                                    End Using
                                                End Using
                                            End Using
                                        End If
                                        mensajeGenerado = response = "" And Not mensajeGenerado
                                        If mensajeGenerado Then
                                            audiosGen = audiosGen + 1
                                            agregarLOG("Mensaje enviado por chattigo. Mensaje: " & eMensaje, nroReporte, 9)
                                        Else

                                            audiosNGen = audiosNGen + 1
                                            agregarLOG("Mensaje NO ENVIADO por chattigo. Respuesta de WS: " & response, nroReporte, 9)
                                        End If
                                    Catch ex As System.Net.WebException
                                        agregarLOG("Servicio de chattigo: " & ex.Message,  , 9)
                                        audiosNGen = audiosNGen + 1
                                        miError = ex.Message
                                    End Try
                                End If
                            Else
                                For i = 0 To UBound(telefonos)

                                    If cliente = "HEB2" Then
                                        If validarURI(url) Then
                                            Dim response = ""

                                            Try

                                                Dim request As HttpWebRequest
                                                Dim targetURI As New Uri(url)
                                                request = HttpWebRequest.Create(targetURI)
                                                body = Strings.Replace(body, "~NUMERO~", "52" & telefonos(i))
                                                request.ContentLength = Encoding.UTF8.GetBytes(body).Length
                                                request.Method = metodo
                                                request.ContentType = ct
                                                request.Headers.Add(HttpRequestHeader.Authorization, autorizacion)

                                                Using requestStream = request.GetRequestStream
                                                    requestStream.Write(Encoding.UTF8.GetBytes(body), 0, body.Length)
                                                    requestStream.Close()

                                                    Using responseStream = request.GetResponse.GetResponseStream
                                                        Using reader As New StreamReader(responseStream)
                                                            response = reader.ReadToEnd()
                                                        End Using
                                                    End Using
                                                End Using
                                                mensajeGenerado = Strings.InStr(response, "Applied") > 0 And Not mensajeGenerado
                                                If mensajeGenerado Then
                                                    audiosGen = audiosGen + 1
                                                    agregarLOG("Mensaje enviado por Twilio. Número: " & telefonos(i) & " Mensaje: " & eMensaje, nroReporte, 9)
                                                Else

                                                    audiosNGen = audiosNGen + 1
                                                    agregarLOG("Mensaje NO ENVIADO por Twilio. Número: " & telefonos(i) & " Respuesta de WS: " & response, nroReporte, 9)
                                                End If
                                            Catch ex As System.Net.WebException
                                                agregarLOG("Servicio de Twilio: " & ex.Message,  , 9)
                                                audiosNGen = audiosNGen + 1
                                                miError = ex.Message
                                            End Try
                                        End If
                                    ElseIf cliente = "HEB3" Then
                                        Dim nReporte = 0
                                        nReporte = ValNull(elmensaje!proceso, "N")

                                        Dim webService2 As String = ValNull(elmensaje!mmcall, "A")
                                        If webService2.Length > 0 Then
                                            Dim partesWS2 = webService2.Split(New Char() {";"c})

                                            Try
                                                Dim accountSid = partesWS2(0) '"AC6c71c75a914fdb725c87ed3fb90d9817" '"AC028dacd5c16ceae219711e2a1fc92311"
                                                Dim authToken = partesWS2(1) '"a59358d6d2887404464a26c09afd98b6" '"20dd1179afed0a9c7b05fb15319b3d75"
                                                TwilioClient.Init(accountSid, authToken)

                                                If partesWS2(2).ToUpper = "WA" Then
                                                    Dim message2 = MessageResource.Create(from:=New Twilio.Types.PhoneNumber("whatsapp:" & partesWS2(3)), body:=eMensaje, to:=New Twilio.Types.PhoneNumber("whatsapp:" & telefonos(i)))
                                                    agregarLOG("Servicio de Twilio: Mensaje de WhatsApp enviado al numero " & telefonos(i) & " Reporte SIGMA: " & nReporte, 0, 9)
                                                    audiosGen = audiosGen + 1
                                                ElseIf partesWS2(2).ToUpper = "SMS" Then
                                                    Dim toNumber = New PhoneNumber(telefonos(i))
                                                    Dim message = MessageResource.Create(
                                                toNumber, from:=New PhoneNumber(partesWS2(4)),
                                                body:=eMensaje)
                                                    agregarLOG("Servicio de Twilio: SMS enviado al numero " & telefonos(i) & " Reporte SIGMA: " & nReporte, 0, 9)
                                                    audiosGen = audiosGen + 1

                                                ElseIf partesWS2(2).ToUpper = "BOTH" Then
                                                    Dim message2 = MessageResource.Create(from:=New Twilio.Types.PhoneNumber("whatsapp:" & partesWS2(3)), body:=eMensaje, to:=New Twilio.Types.PhoneNumber("whatsapp:" & telefonos(i)))
                                                    Dim toNumber = New PhoneNumber(telefonos(i))
                                                    Dim message = MessageResource.Create(toNumber, from:=New PhoneNumber(partesWS2(4)),
                                                body:=eMensaje)
                                                    agregarLOG("Servicio de Twilio: Mensaje de WhatsApp enviado al numero " & telefonos(i) & " Reporte SIGMA: " & nReporte, 0, 9)
                                                    audiosGen = audiosGen + 1
                                                    agregarLOG("Servicio de Twilio: SMS enviado al numero " & telefonos(i) & " Reporte SIGMA: " & nReporte, 0, 9)
                                                    audiosGen = audiosGen + 1
                                                Else
                                                    agregarLOG("Servicio de Twilio: Especifique el tipo de envío para este mensaje (WA, SMS o BOTH)", 0, 9)
                                                    audiosNGen = audiosNGen + 1
                                                End If
                                            Catch ex As Exception
                                                agregarLOG("Servicio de Twilio: Error al enviar al número " & telefonos(i) & " Reporte SIGMA: " & nReporte & ex.Message, 0, 9)
                                                audiosNGen = audiosNGen + 1
                                                miError = ex.Message
                                            End Try
                                        Else
                                            agregarLOG("Servicio de Twilio: No hay datos para enviar por Twilio, configura el webservice de mmcall", 0, 9)
                                            audiosNGen = audiosNGen + 1
                                        End If
                                    Else
                                        Try

                                            Dim objWriter As New System.IO.StreamWriter(rutaSMS & "\" & telefonos(i) & Format(Now, "hhmmss") & IIf(nMensaje > 0, "~A" & nMensaje & "~", "") & i & ".txt", True)
                                            objWriter.WriteLine(eMensaje)
                                            objWriter.Close()
                                            audiosGen = audiosGen + 1
                                            mensajeGenerado = True
                                        Catch ex As Exception
                                            audiosNGen = audiosNGen + 1
                                            miError = ex.Message
                                        End Try

                                    End If


                                Next
                            End If
                        End If
                        cadSQL = "UPDATE " & rutaBD & ".mensajes SET estatus = 'Z', enviada = NOW() WHERE id = " & elmensaje!id
                        If optimizar Then
                            If elmensaje!cuenta > 1 Then
                                cadSQL = "UPDATE " & rutaBD & ".mensajes SET estatus = 'Z', enviada = NOW() WHERE canal = 1 AND lista = " & elmensaje!lista & " AND estatus = '" & idProceso & "'"
                                If mantenerPrioridad Then
                                    cadSQL = cadSQL & " AND prioridad = " & elmensaje!prioridad
                                End If
                            End If
                        End If
                        regsAfectados = consultaACT(cadSQL)
                    Next

                    If audiosNGen > 0 Then
                        agregarLOG(traduccion(8).Replace("campo_0", audiosNGen))
                    End If
                End If

                regsAfectados = consultaACT("UPDATE " & rutaBD & ".mensajes SET estatus = '" & idProceso & "' WHERE canal = 1 AND estatus = 'E' AND alerta = -1000")
                cadSQL = "SELECT a.id, d.telefonos, 0, z.texto, z.titulo, 1 AS cuenta FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".mensajes_procesados z ON a.id = z.mensaje INNER JOIN " & rutaBD & ".cat_distribucion d ON a.lista = d.id AND d.estatus = 'A' WHERE a.estatus = '" & idProceso & "' AND a.alerta = -1000 ORDER BY a.prioridad DESC, a.id"
                mensajesDS = consultaSEL(cadSQL)

                If mensajesDS.Tables(0).Rows.Count > 0 Then
                    For Each elmensaje In mensajesDS.Tables(0).Rows
                        canales = ValNull(elmensaje!telefonos, "A")
                        eMensaje = ValNull(elmensaje!texto, "A")
                        If eMensaje.Length > 0 And canales.Length > 0 Then
                            Dim antes As String = "ÃÀÁÄÂÈÉËÊÌÍÏÎÒÓÖÔÙÚÜÛãàáäâèéëêìíïîòóöôùúüûÑñÇç"
                            Dim ahora As String = "AAAAAEEEEIIIIOOOOUUUUaaaaaeeeeiiiioooouuuunncc"
                            For i = 0 To antes.Length - 1
                                eMensaje = Replace(eMensaje, antes(i), ahora(i))
                            Next
                            eMensaje = Replace(eMensaje, ";", " ")
                            eMensaje = Replace(eMensaje, "\", "-")
                            eMensaje = Replace(eMensaje, "/", "-")

                            eMensaje = Replace(eMensaje, System.Environment.NewLine, " ")
                            eMensaje = Replace(eMensaje, "[90]", "")

                            Dim telefonos As String()
                            Dim tempArray As String()
                            Dim totalItems = 0

                            If canales.Length > 0 Then
                                Dim arreCanales = canales.Split(New Char() {";"c})
                                For i = LBound(arreCanales) To UBound(arreCanales)
                                    If arreCanales(i).Length > 0 Then
                                        'Redimensionamos el Array temporal y preservamos el valor  
                                        ReDim Preserve telefonos(totalItems + i)
                                        telefonos(totalItems + i) = Strings.Trim(arreCanales(i))
                                    End If
                                Next
                                tempArray = telefonos
                                totalItems = telefonos.Length

                                Dim x As Integer, y As Integer
                                Dim z As Integer
                                For x = 0 To UBound(telefonos)
                                    z = 0
                                    For y = 0 To UBound(telefonos) - 1
                                        'Si el elemento del array es igual al array temporal  
                                        If telefonos(x) = tempArray(z) And y <> x Then
                                            'Entonces Eliminamos el valor duplicado  
                                            telefonos(y) = ""
                                        End If
                                        z = z + 1
                                    Next y
                                Next x
                                canales = ""
                            End If
                            eMensaje = Microsoft.VisualBasic.Strings.Left(eMensaje, 120).Trim
                            mensajeGenerado = False
                            'heb/ se agrega el if del WS
                            Dim validado As Boolean = True
                            Dim url As String = ""
                            Dim ws As String = ""
                            Dim metodo As String = ""
                            Dim ct As String = ""
                            Dim autorizacion As String = ""
                            Dim body As String = ""

                            For i = 0 To UBound(telefonos)

                                Try

                                    Dim objWriter As New System.IO.StreamWriter(rutaSMS & "\" & telefonos(i) & Format(Now, "hhmmss") & IIf(nMensaje > 0, "~A" & nMensaje & "~", "") & i & ".txt", True)
                                    objWriter.WriteLine(eMensaje)
                                    objWriter.Close()
                                    audiosGen = audiosGen + 1
                                    mensajeGenerado = True
                                Catch ex As Exception
                                    audiosNGen = audiosNGen + 1
                                    miError = ex.Message
                                End Try
                            Next
                        End If
                        cadSQL = "UPDATE " & rutaBD & ".mensajes SET estatus = 'Z', enviada = NOW() WHERE id = " & elmensaje!id
                        regsAfectados = consultaACT(cadSQL)

                    Next

                    If audiosNGen > 0 Then
                        agregarLOG(traduccion(8).Replace("campo_0", audiosNGen))
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


    Function validarURI(ByVal cadena As String) As Boolean
        Dim validatedUri As System.Uri
        Return Uri.TryCreate(cadena, UriKind.RelativeOrAbsolute, validatedUri)
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

    Function calcularTiempo(Seg) As String
        calcularTiempo = ""
        If Seg < 60 Then
            calcularTiempo = Seg & traduccion(4)
        ElseIf Seg < 3600 Then
            calcularTiempo = Math.Round(Seg / 60, 1) & traduccion(5)
        Else
            calcularTiempo = Math.Round(Seg / 3600, 1) & traduccion(6)
        End If
    End Function

    Function calcularTiempoCad(Seg) As String
        calcularTiempoCad = "-"
        Dim horas = Math.Floor(Seg / 3600)
        Dim minutos = Math.Floor((Seg Mod 3600) / 60)
        Dim segundos = (Seg Mod 3600) Mod 60
        calcularTiempoCad = horas & ":" & Format(minutos, "00") & ":" & Format(segundos, "00")
    End Function

    Private Sub agregarLOG(cadena As String, Optional reporte As Integer = 0, Optional tipo As Integer = 0, Optional aplicacion As Integer = 50)
        If Not be_log_activar Then Exit Sub
        'tipo 0: Info
        'tipo 2: Advertencia
        'tipo 9: Error
        Dim regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".log (aplicacion, tipo, proceso, texto) VALUES (" & aplicacion & ", " & tipo & ", " & reporte & ", '" & Microsoft.VisualBasic.Strings.Left(cadena, 250) & "')")
    End Sub
    Sub etiquetas()
        Dim general = consultaSEL("SELECT cadena FROM " & rutaBD & ".det_idiomas_back WHERE idioma = " & IIf(be_idioma = 0, 1, be_idioma) & " AND modulo = 4 ORDER BY linea")
        Dim cadenaTrad = ""
        If general.Tables(0).Rows.Count > 0 Then
            For Each cadena In general.Tables(0).Rows
                cadenaTrad = cadenaTrad & cadena!cadena
            Next
        End If
        traduccion = cadenaTrad.Split(New Char() {";"c})
    End Sub

End Module
