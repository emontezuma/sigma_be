
Imports System.Security.Cryptography
Imports System.Text
Imports System.Globalization
Imports System.Speech.Synthesis
Imports MySql.Data.MySqlClient
Imports System.IO.Ports
Imports System.IO
Imports System.Data
Imports System.Net.Mail
Imports System.Net
Imports System.ComponentModel
Imports NAudio.Wave
Imports NAudio.Wave.SampleProviders


Module basico
    Public errorBD As String
    Public horaDesde As DateTime
    Public ultimaFalla
    Public autenticado As Boolean
    Public cadenaConexion As String
    Public be_log_activar As Boolean = False
    Public rutaBD As String = "sigma"
    Dim filestoDelete(0) As String
    Public traduccion As String()
    Public be_idioma

    Sub Main(argumentos As String())

        If argumentos.Length Then
            If Strings.UCase(argumentos(0)) = "VOICES" Then
                Dim synthesizer99 As New SpeechSynthesizer()
                Dim CAD = "Total system voice(s): " & synthesizer99.GetInstalledVoices.Count & vbCrLf
                For Each voice In synthesizer99.GetInstalledVoices

                    Dim info As VoiceInfo
                    info = voice.VoiceInfo
                    CAD = CAD & info.Name & vbCrLf
                Next
                MsgBox(CAD)
                Application.Exit()
            End If
        End If

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
            'Escalada 4
            Dim miError As String = ""
            Dim optimizar As Boolean = False
            Dim mantenerPrioridad As Boolean = False
            Dim rutaSMS As String = ""
            Dim mensajeGenerado As Boolean = False
            Dim regsAfectados = 0
            Dim registroDS As DataSet
            Dim voz_audio As String

            Dim canales As String = ""
            Dim laLinea As String = ""
            Dim laMaquina As String = ""
            Dim laArea As String = ""
            Dim mensaje As String = ""
            Dim mensajeOriginal As String = ""
            Dim repeticiones As String = ""
            Dim escalado As String = ""
            Dim audios_ruta As String = ""
            Dim audios_prefijo As String = ""
            Dim audios_escalamiento As Integer

            Dim laFalla As String = ""
            Dim traducir As Boolean = False
            Dim audios_activar As Boolean = False
            Dim be_alarmas_llamadas As Boolean = False
            Dim fecha
            Dim tiempo As String = ""
            Dim nroReporte As Integer = 0
            Dim vecesPR As Integer = 0
            Dim tiempo_audios As Integer = 0
            Dim audio_rate As Integer = 0
            Dim audios_externos As Boolean = False

            Dim cadSQL As String = "SELECT * FROM " & rutaBD & ".configuracion"
            Dim UAudio
            Dim readerDS As DataSet = consultaSEL(cadSQL)
            If readerDS.Tables(0).Rows.Count > 0 Then

                Dim reader As DataRow = readerDS.Tables(0).Rows(0)
                be_idioma = ValNull(reader!idioma_defecto, "N")
                etiquetas()
                optimizar = ValNull(reader!optimizar_llamada, "A") = "S"
                be_log_activar = ValNull(reader!be_log_activar, "A") = "S"
                mantenerPrioridad = ValNull(reader!mantener_prioridad, "A") = "S"
                rutaSMS = ValNull(reader!ruta_audios, "A")
                voz_audio = ValNull(reader!voz_predeterminada, "A")
                traducir = ValNull(reader!traducir, "A") = "S"
                be_alarmas_llamadas = ValNull(reader!be_alarmas_llamadas, "A") = "S"
                audios_activar = ValNull(reader!audios_activar, "A") = "S"
                audios_ruta = ValNull(reader!audios_ruta, "A")
                audios_prefijo = ValNull(reader!audios_prefijo, "A")
                mensajeOriginal = ValNull(reader!mensaje, "A")
                UAudio = reader!ultimo_audio
                vecesPR = ValNull(reader!audios_repeticiones, "N")
                audios_escalamiento = reader!audios_escalamiento
                tiempo_audios = ValNull(reader!tiempo_audios, "N")
                audio_rate = ValNull(reader!audio_rate, "N")

                audios_externos = ValNull(reader!audios_externos, "A") = "S"
            End If
            If vecesPR = 0 Then vecesPR = 1
            If rutaSMS.Length = 0 Then
                rutaSMS = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            Else
                rutaSMS = Strings.Replace(rutaSMS, "/", "\")
            End If
            If audios_ruta.Length = 0 Then
                audios_ruta = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            Else
                audios_ruta = Strings.Replace(audios_ruta, "/", "\")
            End If
            If Not My.Computer.FileSystem.DirectoryExists(rutaSMS) Then
                Try
                    My.Computer.FileSystem.CreateDirectory(rutaSMS)
                Catch ex As Exception
                    rutaSMS = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                End Try
            End If
            If Not My.Computer.FileSystem.DirectoryExists(audios_ruta) Then
                Try
                    My.Computer.FileSystem.CreateDirectory(audios_ruta)
                Catch ex As Exception
                    audios_ruta = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                End Try
            End If

            Dim indiceVoz = 0
            Dim primeraVoz As String
            Dim synthesizer As New SpeechSynthesizer()
            For Each voice In synthesizer.GetInstalledVoices

                indiceVoz = indiceVoz + 1
                Dim info As VoiceInfo
                info = voice.VoiceInfo
                If voz_audio = info.Name Then
                    indiceVoz = -1
                    Exit For
                End If
                If indiceVoz = 1 Then primeraVoz = info.Name
            Next
            If indiceVoz > 0 Then
                agregarLOG(traduccion(10), 0, 9)
                voz_audio = primeraVoz
            ElseIf indiceVoz = 0 Then
                agregarLOG(traduccion(11), 0, 9)
            End If
            Dim generarMensaje As Boolean
            If indiceVoz <> 0 Then
                Dim copiarGeneral As Boolean = True
                If audios_activar Then
                    If UAudio.Equals(System.DBNull.Value) Then
                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".reportes SET generar_audio = 'P' WHERE (generar_audio = 'N' OR generar_audio = 'R' OR generar_audio = 'E') AND estatus = 0")
                    Else
                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".reportes SET generar_audio = 'P' WHERE ((generar_audio = 'N' AND fecha > '" & Format(UAudio, "yyyy/MM/dd HH:mm:ss") & "') OR (generar_audio = 'R') OR (generar_audio = 'E') OR (generar_audio = 'Z')) AND estatus = 0")
                        regsAfectados = consultaACT("UPDATE " & rutaBD & ".configuracion SET ultimo_audio = NOW()")
                    End If
                    cadSQL = "SELECT d.id AS reportenro, d.primer_audio, d.area, d.generar_audio, d.repeticiones, d.escalado, d.fecha, d.estatus AS rep_estatus, b.*, e.nombre as nlinea, f.nombre as nmaquina, b.nombre as narea, h.nombre as nfalla FROM " & rutaBD & ".reportes d INNER JOIN " & rutaBD & ".cat_areas b ON d.area = b.id LEFT JOIN " & rutaBD & ".cat_lineas e ON d.linea = e.id LEFT JOIN " & rutaBD & ".cat_maquinas f ON d.maquina = f.id LEFT JOIN " & rutaBD & ".cat_fallas h ON d.falla = h.id WHERE (d.generar_audio = 'P'" & IIf(tiempo_audios > 0, " OR (d.estatus = 0 AND (TIME_TO_SEC(TIMEDIFF(NOW(), d.audios) >= " & tiempo_audios & ") OR ISNULL(d.audios))))", ")")
                    mensajesDS = consultaSEL(cadSQL)
                    generarMensaje = False

                    If mensajesDS.Tables(0).Rows.Count > 0 Then
                        Dim numeroUnico = 0

                        For Each elmensaje In mensajesDS.Tables(0).Rows
                            mensaje = mensajeOriginal
                            If audios_externos Then
                                If vecesPR > 1 And ValNull(elmensaje!primer_audio, "A") = "N" Then
                                Else
                                    vecesPR = 1
                                End If
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".reportes SET audios = NOW(), generar_audio = 'Z', primer_audio = 'S', reproducir_audio_externo = " & vecesPR & " WHERE id = " & elmensaje!reportenro)
                            Else
                                numeroUnico = numeroUnico + 1
                                Dim audioGenerado = ""
                                laLinea = ValNull(elmensaje!nlinea, "A")
                                nroReporte = ValNull(elmensaje!reportenro, "N")
                                laMaquina = ValNull(elmensaje!nmaquina, "A")
                                laArea = ValNull(elmensaje!narea, "A")
                                laFalla = ValNull(elmensaje!nfalla, "A")
                                repeticiones = ValNull(elmensaje!repeticiones, "N")
                                escalado = ValNull(elmensaje!escalado, "N")
                                fecha = elmensaje!fecha
                                tiempo = calcularTiempoCad(DateAndTime.DateDiff(DateInterval.Second, fecha, Now))

                                If mensaje.Length > 0 Then
                                    mensaje = Replace(mensaje, "[0]", nroReporte)
                                    mensaje = Replace(mensaje, "[1]", laLinea)
                                    mensaje = Replace(mensaje, "[2]", laMaquina)
                                    mensaje = Replace(mensaje, "[3]", laArea)
                                    mensaje = Replace(mensaje, "[4]", laFalla)
                                    mensaje = Replace(mensaje, "[5]", Format(fecha, "dd/MM HH:mm"))
                                    mensaje = Replace(mensaje, "[11]", tiempo)
                                    If (audios_escalamiento = 1 Or audios_escalamiento = 3) And repeticiones > 0 And elmensaje!generar_audio = "R" Then
                                        mensaje = Replace(mensaje, "[20]", traduccion(1) + repeticiones)
                                    Else
                                        mensaje = Replace(mensaje, "[20]", "")
                                    End If
                                    If (audios_escalamiento = 2 Or audios_escalamiento = 3) And escalado > 0 And elmensaje!generar_audio = "E" Then
                                        mensaje = Replace(mensaje, "[30]", traduccion(2) + escalado)
                                    Else
                                        mensaje = Replace(mensaje, "[30]", "")
                                    End If
                                Else
                                    mensaje = traduccion(3) & laMaquina & traduccion(4) & laFalla & " "
                                    If (audios_escalamiento = 1 Or audios_escalamiento = 3) And repeticiones > 0 And elmensaje!generar_audio = "R" Then
                                        mensaje = mensaje & " " & traduccion(1) & repeticiones
                                    End If
                                    If (audios_escalamiento = 2 Or audios_escalamiento = 3) And escalado > 0 And elmensaje!generar_audio = "E" Then
                                        mensaje = mensaje & " " & traduccion(2) & escalado
                                    End If
                                End If
                                If traducir Then mensaje = traducirMensaje(mensaje)
                                Dim audioUnido As String = ""
                                'elvis 01
                                Dim audio_unido = ""
                                audioGenerado = ""
                                numeroUnico = numeroUnico + 1
                                If ValNull(elmensaje!audios_activar, "A") = "S" Then
                                    Dim rutaArea = ValNull(elmensaje!audios_ruta, "A")
                                    If rutaArea.length > 0 Then
                                        rutaArea = Strings.Replace(rutaArea, "/", "\")
                                    End If
                                    Dim prefijo = ValNull(elmensaje!audios_prefijo, "A")
                                    If prefijo.length > 0 Then
                                        prefijo = Strings.Replace(prefijo, "/", "\")
                                    End If
                                    copiarGeneral = elmensaje!audios_general = "S"

                                    Dim nombreFile = ""
                                    Dim audioTMP = ""
                                    Dim audioDEF = ""

                                    'Se graba el audio en la ruta de la carpeta
                                    If My.Computer.FileSystem.DirectoryExists(rutaArea) Then
                                        'Se procesa el audio
                                        Try
                                            Dim synthesizer0 As New SpeechSynthesizer()
                                            nombreFile = Format(DateAndTime.Now(), "yyyyMMddHHmmss" & numeroUnico) & "~R" & nroReporte & "~.wav"
                                            audioTMP = rutaArea & "\audio_tmp" & nombreFile
                                            audioDEF = rutaArea & "\audio_def" & nombreFile
                                            If My.Computer.FileSystem.FileExists(prefijo & "\prefijo.wav") Then
                                                synthesizer0.SetOutputToWaveFile(audioTMP)
                                            Else
                                                synthesizer0.SetOutputToWaveFile(audioDEF)
                                            End If
                                            synthesizer0.SelectVoice(voz_audio)
                                            synthesizer0.Volume = 100 '
                                            synthesizer0.Rate = audio_rate '    
                                            Dim builder2 As New PromptBuilder()
                                            If traducir Then mensaje = traducirMensaje(mensaje)
                                            builder2.AppendText(mensaje)
                                            builder2.Culture = synthesizer0.Voice.Culture
                                            synthesizer0.Speak(builder2)
                                            synthesizer0.SetOutputToDefaultAudioDevice()
                                            synthesizer0.Dispose()


                                            If prefijo.Length > 0 Then


                                                If My.Computer.FileSystem.DirectoryExists(prefijo) Then

                                                    eliminarArchivo(prefijo & "\prefijo_mono.wav")
                                                    eliminarArchivo(prefijo & "\prefijo_rs.wav")
                                                    eliminarArchivo(prefijo & "\tmp_mono.wav")
                                                    eliminarArchivo(prefijo & "\tmp_rs.wav")


                                                    prefijo = prefijo & "\prefijo.wav"
                                                    'Se aplican los procedimientos necesarios para unificar audios...
                                                    'Se pasan de stereo a mono
                                                    If My.Computer.FileSystem.FileExists(prefijo) Then
                                                        Dim audio_mono = New IO.FileInfo(prefijo).DirectoryName & "\tmp_mono.wav"
                                                        Dim audio_rs = New IO.FileInfo(prefijo).DirectoryName & "\tmp_rs.wav"

                                                        Dim prefijo_mono = New IO.FileInfo(prefijo).DirectoryName & "\prefijo_mono.wav"
                                                        Dim prefijo_rs = New IO.FileInfo(prefijo).DirectoryName & "\prefijo_rs.wav"
                                                        Try
                                                            stereo2mono(prefijo, prefijo_mono)
                                                        Catch ex As Exception
                                                            prefijo_mono = prefijo
                                                        End Try
                                                        resample(prefijo_mono, prefijo_rs)

                                                        Try
                                                            stereo2mono(audioTMP, audio_mono)
                                                        Catch ex As Exception
                                                            audio_mono = audioTMP
                                                        End Try


                                                        resample(audio_mono, audio_rs)

                                                        Dim first = New AudioFileReader(prefijo_rs)

                                                        Dim Second = New AudioFileReader(audio_rs)
                                                        Dim playlist = New ConcatenatingSampleProvider({first, Second})
                                                        WaveFileWriter.CreateWaveFile16(audioDEF, playlist)
                                                        Second.Close()
                                                    End If
                                                End If
                                            End If
                                            If vecesPR > 1 And ValNull(elmensaje!primer_audio, "A") = "N" Then
                                                For i = 2 To vecesPR
                                                    My.Computer.FileSystem.CopyFile(audioDEF, rutaArea & "\audio_def_r" & Format(i, "00") & "_" & nombreFile)
                                                Next
                                            End If
                                            audiosGen = audiosGen + 1
                                            If copiarGeneral And audios_activar Then
                                                Try
                                                    File.Copy(audioDEF, audios_ruta & "\audio_def" & elmensaje!nombre & "_" & nombreFile)
                                                    If vecesPR > 1 And ValNull(elmensaje!primer_audio, "A") = "N" Then
                                                        For i = 2 To vecesPR
                                                            My.Computer.FileSystem.CopyFile(audioDEF, audios_ruta & "\audio_def_r" & Format(i, "00") & "_" & elmensaje!nombre & "_" & nombreFile)
                                                        Next
                                                    End If
                                                Catch ex As Exception

                                                End Try
                                            End If
                                        Catch ex As Exception
                                            miError = ex.Message
                                            audiosNGen = audiosNGen + 1

                                        End Try

                                    End If
                                End If
                                If audios_activar Then
                                    If My.Computer.FileSystem.DirectoryExists(audios_ruta) Then
                                        'Se procesa el audio
                                        Try
                                            Dim synthesizer0 As New SpeechSynthesizer()
                                            Dim nombreFile = Format(DateAndTime.Now(), "yyyyMMddHHmmss" & numeroUnico) & "~R" & nroReporte & "~.wav"
                                            Dim audioTMP = audios_ruta & "\audio_tmp" & nombreFile
                                            Dim audioDEF = audios_ruta & "\audio_def" & nombreFile
                                            If My.Computer.FileSystem.FileExists(audios_prefijo & "\prefijo.wav") Then
                                                synthesizer0.SetOutputToWaveFile(audioTMP)
                                            Else
                                                synthesizer0.SetOutputToWaveFile(audioDEF)
                                            End If
                                            synthesizer0.SelectVoice(voz_audio)
                                            synthesizer0.Volume = 100 '
                                            synthesizer0.Rate = audio_rate '    
                                            Dim builder2 As New PromptBuilder()
                                            If traducir Then mensaje = traducirMensaje(mensaje)
                                            builder2.AppendText(mensaje)
                                            builder2.Culture = synthesizer0.Voice.Culture
                                            synthesizer0.Speak(builder2)
                                            synthesizer0.SetOutputToDefaultAudioDevice()
                                            synthesizer0.Dispose()


                                            If audios_prefijo.Length > 0 Then


                                                If My.Computer.FileSystem.DirectoryExists(audios_prefijo) Then

                                                    eliminarArchivo(audios_prefijo & "\prefijo_mono.wav")
                                                    eliminarArchivo(audios_prefijo & "\prefijo_rs.wav")
                                                    eliminarArchivo(audios_prefijo & "\tmp_mono.wav")
                                                    eliminarArchivo(audios_prefijo & "\tmp_rs.wav")

                                                    audios_prefijo = audios_prefijo & "\prefijo.wav"
                                                    If My.Computer.FileSystem.FileExists(audios_prefijo) Then

                                                        'Se aplican los procedimientos necesarios para unificar audios...
                                                        'Se pasan de stereo a mono

                                                        Dim audio_mono = New IO.FileInfo(audios_prefijo).DirectoryName & "\tmp_mono.wav"
                                                        Dim audio_rs = New IO.FileInfo(audios_prefijo).DirectoryName & "\tmp_rs.wav"

                                                        Dim prefijo_mono = New IO.FileInfo(audios_prefijo).DirectoryName & "\prefijo_mono.wav"
                                                        Dim prefijo_rs = New IO.FileInfo(audios_prefijo).DirectoryName & "\prefijo_rs.wav"

                                                        Try
                                                            stereo2mono(audios_prefijo, prefijo_mono)
                                                        Catch ex As Exception
                                                            prefijo_mono = audios_prefijo
                                                        End Try

                                                        resample(prefijo_mono, prefijo_rs)

                                                        Try
                                                            stereo2mono(audioTMP, audio_mono)
                                                        Catch ex As Exception
                                                            audio_mono = audioTMP
                                                        End Try


                                                        resample(audio_mono, audio_rs)

                                                        Dim first = New AudioFileReader(prefijo_rs)

                                                        Dim Second = New AudioFileReader(audio_rs)
                                                        Dim playlist = New ConcatenatingSampleProvider({first, Second})
                                                        WaveFileWriter.CreateWaveFile16(audioDEF, playlist)
                                                        Second.Close()
                                                    End If
                                                End If

                                            End If
                                            If vecesPR > 1 And ValNull(elmensaje!primer_audio, "A") = "N" Then
                                                For i = 2 To vecesPR
                                                    My.Computer.FileSystem.CopyFile(audioDEF, audios_ruta & "\audio_def_r" & Format(i, "00") & "_" & nombreFile)
                                                Next
                                            End If
                                            audiosGen = audiosGen + 1
                                        Catch ex As Exception
                                            miError = ex.Message
                                            audiosNGen = audiosNGen + 1
                                        End Try

                                    End If
                                End If
                                'elvis02
                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".reportes SET audios = NOW(), generar_audio = 'Z', primer_audio = 'S' WHERE id = " & nroReporte)

                            End If

                        Next

                    End If
                End If

                'Generacion de audios para llamadas


                If be_alarmas_llamadas Then
                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".mensajes SET estatus = '" & idProceso & "' WHERE canal = 0 AND estatus = 'E' AND alerta <> -1000")
                    Dim agrupado As Boolean = True
                    If Not optimizar Then
                        cadSQL = "SELECT a.alarma AS nmensaje, a.tipo AS tmensaje, a.id, a.lista, d.telefonos, d.hora_desde, d.hora_hasta, a.prioridad, z.texto, z.titulo, 1 AS cuenta FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".mensajes_procesados z ON a.id = z.mensaje INNER JOIN " & rutaBD & ".cat_alertas b on a.alerta = b.id INNER JOIN " & rutaBD & ".cat_distribucion d ON a.lista = d.id AND b.estatus = 'A' WHERE a.estatus = '" & idProceso & "' ORDER BY a.prioridad DESC, a.id"
                        agrupado = False
                    ElseIf mantenerPrioridad Then
                        cadSQL = "SELECT 0 AS nmensaje, 0 AS tmensaje, a.lista, a.prioridad, b.telefonos, b.hora_desde, b.hora_hasta, COUNT(*) AS cuenta, MAX(a.id) AS id FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".mensajes_procesados z ON a.id = z.mensaje INNER JOIN " & rutaBD & ".cat_distribucion b ON a.lista = b.id AND b.estatus = 'A' INNER JOIN " & rutaBD & ".cat_alertas c on a.alerta = c.id WHERE a.estatus = '" & idProceso & "' GROUP BY nmensaje, tmensaje, a.prioridad, a.lista, b.telefonos, b.hora_desde, b.hora_hasta ORDER BY prioridad DESC"
                    Else
                        cadSQL = "SELECT 0 AS nmensaje, 0 AS tmensaje, a.lista, b.telefonos, 0 AS prioridad, b.hora_desde, b.hora_hasta, COUNT(*) AS cuenta, MAX(a.id) AS id FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".mensajes_procesados z ON a.id = z.mensaje INNER JOIN " & rutaBD & ".cat_distribucion b ON a.lista = b.id AND b.estatus = 'A' INNER JOIN " & rutaBD & ".cat_alertas c on a.alerta = c.id WHERE a.estatus = '" & idProceso & "' GROUP BY nmensaje, tmensaje, a.lista, b.telefonos, b.hora_desde, b.hora_hasta"

                    End If
                    'Se preselecciona la voz
                    mensajesDS = consultaSEL(cadSQL)
                    Dim nMensaje = 0
                    Dim tMensaje = 0
                    Dim unico = 0
                    If mensajesDS.Tables(0).Rows.Count > 0 Then
                        For Each elmensaje In mensajesDS.Tables(0).Rows
                            unico = unico + 1

                            canales = ValNull(elmensaje!telefonos, "A")
                            nMensaje = ValNull(elmensaje!nmensaje, "N")
                            tMensaje = ValNull(elmensaje!tmensaje, "N")
                            If tMensaje = 8 Then nMensaje = 0 'No se marca la llamada de resolución
                            eMensaje = ""
                            If elmensaje!cuenta > 1 Then
                                eMensaje = traduccion(5).Replace("campo_0", elmensaje!cuenta)
                                If mantenerPrioridad And elmensaje!prioridad > 0 Then
                                    eMensaje = traduccion(6).Replace("campo_0", elmensaje!cuenta)
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
                            Dim generarLlamada As Boolean = False
                            If canales.Length > 0 Then
                                If elmensaje!hora_desde.Equals(System.DBNull.Value) And elmensaje!hora_hasta.Equals(System.DBNull.Value) Then
                                    generarLlamada = True
                                ElseIf elmensaje!hora_desde.Equals(System.DBNull.Value) Then
                                    If Format(DateAndTime.Now(), "HH:mm:ss") <= elmensaje!hora_hasta.ToString Then
                                        generarLlamada = True
                                    End If
                                ElseIf elmensaje!hora_hasta.Equals(System.DBNull.Value) Then
                                    If Format(DateAndTime.Now(), "HH:mm:ss") >= elmensaje!hora_desde.ToString Then
                                        generarLlamada = True
                                    End If
                                ElseIf Format(DateAndTime.Now(), "HH:mm:ss") >= elmensaje!hora_desde.ToString And Format(DateAndTime.Now(), "HH:mm:ss") <= elmensaje!hora_hasta.ToString Then
                                    generarLlamada = True
                                End If

                            End If
                            If generarLlamada Then
                                Dim telefonos As String()
                                Dim tempArray As String()
                                Dim totalItems = 0
                                If canales.Length > 0 Then
                                    Dim arreCanales = canales.Split(New Char() {";"c})
                                    For i = LBound(arreCanales) To UBound(arreCanales)
                                        If arreCanales(i).Length > 0 Then
                                            'Redimensionamos el Array temporal y preservamos el valor  
                                            ReDim Preserve telefonos(totalItems + i)
                                            telefonos(totalItems + i) = arreCanales(i)
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
                                mensajeGenerado = False
                                Dim NArchivo = rutaSMS & "\numero_sustituir" & Format(Now, "hhmmss") & IIf(nMensaje > 0, "~A" & nMensaje & "~", "") & unico & "1_1.wav"
                                For i = 0 To UBound(telefonos)
                                    If telefonos(i).Length > 0 Then
                                        Try
                                            Dim synthesizer0 As New SpeechSynthesizer()

                                            synthesizer0.SetOutputToWaveFile(NArchivo)
                                            synthesizer0.SelectVoice(voz_audio)
                                            synthesizer0.Volume = 100 '  // 0...100
                                            synthesizer0.Rate = audio_rate '     // -10...10
                                            Dim builder2 As New PromptBuilder()
                                            If traducir Then eMensaje = traducirMensaje(eMensaje)
                                            builder2.AppendText(eMensaje)
                                            builder2.Culture = synthesizer0.Voice.Culture
                                            synthesizer0.Speak(builder2)
                                            synthesizer0.SetOutputToDefaultAudioDevice()
                                            mensajeGenerado = True

                                            Exit For
                                        Catch ex As Exception
                                            miError = ex.Message
                                            audiosNGen = audiosNGen + 1
                                        End Try
                                    End If
                                Next
                                If mensajeGenerado Then
                                    For i = 0 To UBound(telefonos)
                                        If telefonos(i).Length > 0 Then
                                            Dim NuevoArchivo = Replace(NArchivo, "numero_sustituir", telefonos(i))
                                            My.Computer.FileSystem.CopyFile(NArchivo, NuevoArchivo)
                                            audiosGen = audiosGen + 1
                                        End If
                                    Next

                                End If
                                My.Computer.FileSystem.DeleteFile(NArchivo)
                            End If
                            cadSQL = "UPDATE " & rutaBD & ".mensajes SET estatus = 'Z', enviada = NOW() WHERE id = " & elmensaje!id
                            If optimizar Then
                                If elmensaje!cuenta > 1 Then
                                    cadSQL = "UPDATE " & rutaBD & ".mensajes SET estatus = 'Z', enviada = NOW() WHERE canal = 0 AND lista = " & elmensaje!lista & " AND estatus = '" & idProceso & "'"
                                    If mantenerPrioridad Then
                                        cadSQL = cadSQL & " AND prioridad = " & elmensaje!prioridad
                                    End If
                                End If
                            End If
                            regsAfectados = consultaACT(cadSQL)
                        Next
                        If audiosGen > 0 Or audiosNGen > 0 Then
                            agregarLOG(traduccion(9).Replace("campo_0", audiosGen).Replace("campo_1", audiosNGen))
                        End If
                    End If


                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".mensajes SET estatus = '" & idProceso & "' WHERE canal = 0 AND estatus = 'E' AND alerta = -1000")
                    cadSQL = "SELECT a.id, d.telefonos, d.hora_desde, d.hora_hasta, 0, z.texto, z.titulo, 1 AS cuenta FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".mensajes_procesados z ON a.id = z.mensaje INNER JOIN " & rutaBD & ".cat_distribucion d ON a.lista = d.id AND d.estatus = 'A' WHERE a.estatus = '" & idProceso & "' AND a.alerta = -1000 ORDER BY a.prioridad DESC, a.id"
                    mensajesDS = consultaSEL(cadSQL)
                    nMensaje = 0
                    tMensaje = 0
                    unico = 0

                    If mensajesDS.Tables(0).Rows.Count > 0 Then
                        For Each elmensaje In mensajesDS.Tables(0).Rows
                            unico = unico + 1

                            canales = ValNull(elmensaje!telefonos, "A")
                            eMensaje = ValNull(elmensaje!texto, "A")
                            Dim generarLlamada As Boolean = False
                            If canales.Length > 0 Then
                                If elmensaje!hora_desde.Equals(System.DBNull.Value) And elmensaje!hora_hasta.Equals(System.DBNull.Value) Then
                                    generarLlamada = True
                                ElseIf elmensaje!hora_desde.Equals(System.DBNull.Value) Then
                                    If Format(DateAndTime.Now(), "HH:mm:ss") <= elmensaje!hora_hasta.ToString Then
                                        generarLlamada = True
                                    End If
                                ElseIf elmensaje!hora_hasta.Equals(System.DBNull.Value) Then
                                    If Format(DateAndTime.Now(), "HH:mm:ss") >= elmensaje!hora_desde.ToString Then
                                        generarLlamada = True
                                    End If
                                ElseIf Format(DateAndTime.Now(), "HH:mm:ss") >= elmensaje!hora_desde.ToString And Format(DateAndTime.Now(), "HH:mm:ss") <= elmensaje!hora_hasta.ToString Then
                                    generarLlamada = True
                                End If

                            End If
                            If generarLlamada Then
                                Dim telefonos As String()
                                Dim tempArray As String()
                                Dim totalItems = 0
                                If canales.Length > 0 Then
                                    Dim arreCanales = canales.Split(New Char() {";"c})
                                    For i = LBound(arreCanales) To UBound(arreCanales)
                                        If arreCanales(i).Length > 0 Then
                                            'Redimensionamos el Array temporal y preservamos el valor  
                                            ReDim Preserve telefonos(totalItems + i)
                                            telefonos(totalItems + i) = arreCanales(i)
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
                                mensajeGenerado = False
                                Dim NArchivo = rutaSMS & "\numero_sustituir" & Format(Now, "hhmmss") & IIf(nMensaje > 0, "~A" & nMensaje & "~", "") & unico & "1_1.wav"
                                For i = 0 To UBound(telefonos)
                                    If telefonos(i).Length > 0 Then
                                        Try
                                            Dim synthesizer0 As New SpeechSynthesizer()

                                            synthesizer0.SetOutputToWaveFile(NArchivo)
                                            synthesizer0.SelectVoice(voz_audio)
                                            synthesizer0.Volume = 100 '  // 0...100
                                            synthesizer0.Rate = audio_rate '     // -10...10
                                            Dim builder2 As New PromptBuilder()
                                            If traducir Then eMensaje = traducirMensaje(eMensaje)
                                            builder2.AppendText(eMensaje)
                                            builder2.Culture = synthesizer0.Voice.Culture
                                            synthesizer0.Speak(builder2)
                                            synthesizer0.SetOutputToDefaultAudioDevice()
                                            mensajeGenerado = True

                                            Exit For
                                        Catch ex As Exception
                                            miError = ex.Message
                                            audiosNGen = audiosNGen + 1
                                        End Try
                                    End If
                                Next
                                If mensajeGenerado Then
                                    For i = 0 To UBound(telefonos)
                                        If telefonos(i).Length > 0 Then
                                            Dim NuevoArchivo = Replace(NArchivo, "numero_sustituir", telefonos(i))
                                            My.Computer.FileSystem.CopyFile(NArchivo, NuevoArchivo)
                                            audiosGen = audiosGen + 1
                                        End If
                                    Next

                                End If
                                My.Computer.FileSystem.DeleteFile(NArchivo)
                            End If
                            cadSQL = "UPDATE " & rutaBD & ".mensajes SET estatus = 'Z', enviada = NOW() WHERE id = " & elmensaje!id
                            regsAfectados = consultaACT(cadSQL)
                        Next
                        If audiosGen > 0 Or audiosNGen > 0 Then
                            agregarLOG(traduccion(9).Replace("campo_0", audiosGen).Replace("campo_1", audiosNGen))
                        End If
                    End If




                    cadSQL = "SELECT a.alarma as nmensaje, a.id, a.texto, b.telefonos FROM " & rutaBD & ".mensajes a INNER JOIN " & rutaBD & ".cat_distribucion b ON a.lista = b.id AND b.estatus = 'A' WHERE a.alerta = 0 AND a.canal = 0 AND a.estatus = '" & idProceso & "' ORDER BY a.prioridad DESC, a.id"
                    mensajesDS = consultaSEL(cadSQL)
                    generarMensaje = False
                    unico = 0
                    If mensajesDS.Tables(0).Rows.Count > 0 Then
                        For Each elmensaje In mensajesDS.Tables(0).Rows
                            unico = unico + 1
                            canales = ValNull(elmensaje!telefonos, "A")
                            eMensaje = traduccion(7) & ValNull(elmensaje!texto, "A")
                            nMensaje = traduccion(7) & ValNull(elmensaje!nmensaje, "N")
                            If canales.Length > 0 Then
                                Dim telefonos As String()
                                Dim tempArray As String()
                                Dim totalItems = 0
                                If canales.Length > 0 Then
                                    Dim arreCanales = canales.Split(New Char() {";"c})
                                    For i = LBound(arreCanales) To UBound(arreCanales)
                                        'Redimensionamos el Array temporal y preservamos el valor  
                                        ReDim Preserve telefonos(totalItems + i)
                                        telefonos(totalItems + i) = arreCanales(i)
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
                                mensajeGenerado = False
                                Dim NArchivo = rutaSMS & "\numero_sustituir" & Format(Now, "hhmmss") & IIf(nMensaje > 0, "~A" & nMensaje & "~", "") & unico & "2_1.wav"
                                For i = 0 To UBound(telefonos)
                                    If telefonos(i).Length > 0 Then
                                        Try
                                            Dim synthesizer0 As New SpeechSynthesizer()

                                            synthesizer0.SetOutputToWaveFile(NArchivo)
                                            synthesizer0.SelectVoice(voz_audio)
                                            synthesizer0.Volume = 100 '  // 0...100
                                            synthesizer0.Rate = audio_rate '     // -10...10
                                            Dim builder2 As New PromptBuilder()
                                            If traducir Then eMensaje = traducirMensaje(eMensaje)
                                            builder2.AppendText(eMensaje)
                                            builder2.Culture = synthesizer0.Voice.Culture
                                            synthesizer0.Speak(builder2)
                                            synthesizer0.SetOutputToDefaultAudioDevice()
                                            mensajeGenerado = True

                                            Exit For
                                        Catch ex As Exception
                                            miError = ex.Message
                                            audiosNGen = audiosNGen + 1
                                        End Try
                                    End If
                                Next
                                If mensajeGenerado Then
                                    For i = 0 To UBound(telefonos)
                                        If telefonos(i).Length > 0 Then
                                            Dim NuevoArchivo = Replace(NArchivo, "numero_sustituir", telefonos(i))
                                            My.Computer.FileSystem.CopyFile(NArchivo, NuevoArchivo)
                                            audiosGen = audiosGen + 1
                                        End If
                                    Next
                                    My.Computer.FileSystem.DeleteFile(NArchivo)
                                End If
                            End If
                            If mensajeGenerado Then
                                cadSQL = "UPDATE " & rutaBD & ".mensajes SET estatus = 'Z' WHERE id = " & elmensaje!id
                                regsAfectados = consultaACT(cadSQL)
                            End If
                        Next
                        If audiosNGen > 0 Then
                            agregarLOG(traduccion(8).Replace("campo_0", audiosNGen))
                        End If
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

    Function calcularTiempo(Seg) As String
        calcularTiempo = ""
        If Seg < 60 Then
            calcularTiempo = Seg & traduccion(12)
        ElseIf Seg < 3600 Then
            calcularTiempo = Math.Round(Seg / 60, 1) & traduccion(13)
        Else
            calcularTiempo = Math.Round(Seg / 3600, 1) & traduccion(14)
        End If
    End Function

    Function calcularTiempoCad(Seg) As String
        calcularTiempoCad = "-"
        Dim horas = Math.Floor(Seg / 3600)
        Dim minutos = Math.Floor((Seg Mod 3600) / 60)
        Dim segundos = (Seg Mod 3600) Mod 60
        calcularTiempoCad = horas & ":" & Format(minutos, "00") & ":" & Format(segundos, "00")
    End Function

    Private Sub agregarLOG(cadena As String, Optional reporte As Integer = 0, Optional tipo As Integer = 0, Optional aplicacion As Integer = 70)
        If Not be_log_activar Then Exit Sub
        'tipo 0: Info
        'tipo 2: Advertencia
        'tipo 9: Error
        Dim regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".log (aplicacion, tipo, proceso, texto) VALUES (" & aplicacion & ", " & tipo & ", " & reporte & ", '" & Microsoft.VisualBasic.Strings.Left(cadena, 250) & "')")
    End Sub
    Function traducirMensaje(mensaje As String) As String
        traducirMensaje = mensaje
        Dim cadSQL As String = ""
        Dim cadCanales As String = ValNull(mensaje, "A")
        If cadCanales.Length > 0 Then
            'Se busca toda la frase

            cadSQL = "SELECT traduccion FROM " & rutaBD & ".traduccion WHERE literal = '" & cadCanales & "'"
            Dim reader As DataSet = consultaSEL(cadSQL)
            If reader.Tables(0).Rows.Count > 0 Then
                traducirMensaje = ValNull(reader.Tables(0).Rows(0)!traduccion, "A")
            Else
                traducirMensaje = ""
                Dim arreCanales = cadCanales.Split(New Char() {" "c})
                For i = LBound(arreCanales) To UBound(arreCanales)
                    'Redimensionamos el Array temporal y preservamos el valor  
                    cadSQL = "SELECT traduccion FROM " & rutaBD & ".traduccion WHERE literal = '" & Trim(arreCanales(i)) & "'"
                    Dim reader2 As DataSet = consultaSEL(cadSQL)
                    If reader2.Tables(0).Rows.Count > 0 Then
                        traducirMensaje = traducirMensaje & " " & ValNull(reader2.Tables(0).Rows(0)!traduccion, "A")
                    Else
                        traducirMensaje = traducirMensaje & " " & arreCanales(i)
                    End If
                Next
            End If
        End If


    End Function

    Sub eliminarArchivo(archivo)
        Try

            File.Delete(archivo)
            My.Computer.FileSystem.DeleteFile(archivo)

        Catch ex As Exception

        End Try

    End Sub

    Sub stereo2mono(stereo, mono)
        Dim inputReader = New AudioFileReader(stereo)
        Dim monoFile = New StereoToMonoSampleProvider(inputReader)
        monoFile.LeftVolume = 0.0F
        monoFile.RightVolume = 1.0F
        WaveFileWriter.CreateWaveFile16(mono, monoFile)
    End Sub

    Sub resample(origen, destino)
        Dim reader = New AudioFileReader(origen)
        Dim resampler = New WdlResamplingSampleProvider(reader, 16000)
        WaveFileWriter.CreateWaveFile16(destino, resampler)
        reader.Close()
    End Sub

    Private Sub borrarArchivos()

        For i = 0 To filestoDelete.Length - 1
            If Not IsNothing(filestoDelete(i)) Then eliminarArchivo(filestoDelete(i))
        Next
    End Sub
    Sub etiquetas()
        Dim general = consultaSEL("SELECT cadena FROM " & rutaBD & ".det_idiomas_back WHERE idioma = " & IIf(be_idioma = 0, 1, be_idioma) & " AND modulo = 2 ORDER BY linea")
        Dim cadenaTrad = ""
        If general.Tables(0).Rows.Count > 0 Then
            For Each cadena In general.Tables(0).Rows
                cadenaTrad = cadenaTrad & cadena!cadena
            Next
        End If
        traduccion = cadenaTrad.Split(New Char() {";"c})
    End Sub

End Module

