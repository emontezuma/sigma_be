Imports MySql.Data.MySqlClient
Imports System.Security.Cryptography
Imports System.Text
Imports System.IO
Imports System.Data
Imports System.Net.Mail
Imports System.Net


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
    Public d
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="argumentos"></param>
    Sub Main(argumentos As String())
        If Process.GetProcessesByName _
          (Process.GetCurrentProcess.ProcessName).Length > 1 Then
        ElseIf argumentos.Length < 2 Then
            MsgBox("Wrong parameters", MsgBoxStyle.Critical, "SIGMA")
        Else
            cadenaConexion = argumentos(0)
            Dim reporte As String = argumentos(1)
            If reporte.ToUpper <> "programacion" And reporte.ToUpper <> "programacion_cp" Then
                MsgBox("Unknown report name", MsgBoxStyle.Critical, "SIGMA")
            ElseIf reporte.ToUpper = "programacion" Or reporte.ToUpper = "programacion_cp" Then
                Dim lotes As DataSet
                Dim fecha = DateAndTime.Now()
                Dim cadSQL = ""
                Dim formatoFecha = "yyyy/MM/dd HH:mm:ss"

                cadenaConexion = "server=127.0.0.1;user id=root;password=usbw;port=3307;Convert Zero Datetime=True"
                Dim regsAfectados = consultaACT("UPDATE " & rutaBD & ".configuracion SET listado01 = 'T', listado01_f = NOW();DELETE FROM " & rutaBD & ".listado01_detalle;ALTER TABLE " & rutaBD & ".listado01_detalle AUTO_INCREMENT = 1;DELETE FROM " & rutaBD & ".listado01_cabecera;ALTER TABLE " & rutaBD & ".listado01_cabecera AUTO_INCREMENT = 1;INSERT INTO " & rutaBD & ".listado01_cabecera (proceso, equipo, hasta) SELECT proceso, equipo, '" & Format(fecha, formatoFecha) & "' FROM " & rutaBD & ".lotes WHERE estatus = 'A' AND estado <= 50 GROUP BY proceso, equipo ORDER BY proceso;INSERT INTO " & rutaBD & ".listado01_detalle (lote, parte, creacion, prioridad, proceso, equipo, ruta, ruta_detalle, estado, secuencia, proceso_desde, proceso_hasta, tiempo_proceso) SELECT id, parte, creacion, IFNULL((SELECT MIN(orden) FROM " & rutaBD & ".prioridades WHERE parte = a.parte  AND proceso = a.proceso AND fecha >= NOW() AND estatus = 'A'), 10000), proceso, equipo, ruta, ruta_detalle, estado, ruta_secuencia, fecha, hasta, TIMESTAMPDIFF(SECOND, fecha, hasta) FROM " & rutaBD & ".lotes a WHERE estado = 50 AND estatus = 'A';INSERT INTO " & rutaBD & ".listado01_detalle (lote, parte, creacion, prioridad, proceso, equipo, ruta, ruta_detalle, estado, secuencia) SELECT id, parte, creacion, IFNULL((SELECT MIN(orden) FROM " & rutaBD & ".prioridades WHERE parte = a.parte  AND proceso = a.proceso AND fecha >= NOW() AND estatus = 'A'), 10000), proceso, equipo, ruta, ruta_detalle, estado, ruta_secuencia FROM " & rutaBD & ".lotes a WHERE estado < 50 AND estatus = 'A';")
                'Se cierran los lotes que estan en estado 50
                cadSQL = "SELECT * FROM " & rutaBD & ".listado01_detalle WHERE estado = 50 ORDER BY proceso, prioridad, creacion"
                lotes = consultaSEL(cadSQL)
                Dim fechaMayor = fecha
                Dim secuencia = 0
                If lotes.Tables(0).Rows.Count > 0 Then
                    For Each lote In lotes.Tables(0).Rows
                        cadSQL = "SELECT secuencia + 1 AS secuencia FROM " & rutaBD & ".listado01_cabecera WHERE proceso = " & lote!proceso
                        Dim proceso As DataSet = consultaSEL(cadSQL)
                        If proceso.Tables(0).Rows.Count > 0 Then
                            secuencia = proceso.Tables(0).Rows(0)!secuencia
                        End If
                        Dim fechaUpdate = fecha
                        'Se calcula la hora y fecha de terminación cuando el lote esta subido al equipo.
                        If lote!proceso_hasta > fecha Then
                            fechaUpdate = lote!proceso_hasta
                            If fechaUpdate > fechaMayor Then
                                fechaMayor = fechaUpdate
                            End If
                        End If
                        Dim fechaFin = fecha
                        Dim crearNuevo50 = True
                        If lote!lote = 2692 Then
                            Dim elvis = 1
                        End If
                        If Not lote!proceso_hasta.Equals(System.DBNull.Value) Then
                            fechaFin = lote!proceso_hasta
                            crearNuevo50 = False
                        End If
                        cadSQL = "SELECT id, secuencia, proceso FROM " & rutaBD & ".det_rutas WHERE ruta = " & lote!ruta & " AND secuencia = " & lote!secuencia + 1
                        Dim rutaSiguiente = consultaSEL(cadSQL)
                        Dim cadInsert = ""
                        Dim ultimaOperacion As Boolean = False
                        If rutaSiguiente.Tables(0).Rows.Count = 0 Then
                            ultimaOperacion = True
                        Else
                            cadInsert = ";INSERT INTO " & rutaBD & ".listado01_detalle (lote, parte, ciclo, creacion, prioridad, proceso, ruta, ruta_detalle, secuencia, finalizo_anterior) VALUES (" & lote!lote & ", " & lote!parte & ", [esteCliclo], '" & Format(lote!creacion, formatoFecha) & "', " & lote!prioridad & ", " & rutaSiguiente.Tables(0).Rows(0)!proceso & ", " & lote!ruta & ", " & rutaSiguiente.Tables(0).Rows(0)!id & ", " & rutaSiguiente.Tables(0).Rows(0)!secuencia & ", '" & Format(fechaFin, formatoFecha) & "')"
                        End If
                        If crearNuevo50 Then
                            regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".listado01_detalle (lote, parte, ciclo, creacion, prioridad, proceso, equipo, ruta, ruta_detalle, estado, secuencia, proceso_desde, proceso_hasta, tiempo_proceso, orden, ultima) VALUES (" & lote!lote & ", " & lote!parte & ", 1, '" & Format(lote!creacion, formatoFecha) & "', " & lote!prioridad & ", " & lote!proceso & ", " & lote!equipo & ", " & lote!ruta & ", " & lote!ruta_detalle & ", " & IIf(ultimaOperacion, "99", "50") & ", " & lote!secuencia & ", '" & Format(lote!proceso_desde, formatoFecha) & "', '" & Format(fechaUpdate, formatoFecha) & "', " & DateAndTime.DateDiff(DateInterval.Second, lote!proceso_desde, fechaUpdate) & ", " & secuencia & ", '" & IIf(ultimaOperacion, "S", "N") & "')" & cadInsert.Replace("[esteCliclo]", "2"))
                        ElseIf cadInsert.Length > 0 Then
                            regsAfectados = consultaACT(Strings.Mid(cadInsert.Replace("[esteCliclo]", "1"), 2))
                        End If
                        If fechaMayor < fechaFin Then
                            fechaMayor = fechaFin
                        End If
                        'Se busca si hay una secuencia superior para mover el lote en estado 0
                        cadSQL = "SELECT hasta FROM " & rutaBD & ".listado01_cabecera WHERE proceso = " & lote!proceso & " AND equipo = " & lote!equipo
                        proceso = consultaSEL(cadSQL)
                        If proceso.Tables(0).Rows.Count > 0 Then
                            Dim cadAdic = ""
                            If proceso.Tables(0).Rows(0)!hasta < fechaMayor Then
                                cadAdic = ", hasta = '" & Format(fechaMayor, formatoFecha) & "'"
                            End If
                            regsAfectados = consultaACT("UPDATE " & rutaBD & ".listado01_cabecera SET secuencia = secuencia + 1, ultima_parte = " & lote!parte & cadAdic & " WHERE proceso = " & lote!proceso & " AND equipo = " & lote!equipo)
                        End If
                    Next
                End If
                'Corridas continuas hasta avanzar todos los lotes
                'Se cierran los lotes que estan en estado 50
                Dim Salir = False
                Dim vuelta = 0
                Dim PasarUno = False
                Do While Not Salir

                    vuelta = vuelta + 1
                    If vuelta = 30 Then
                        Dim elvis4 = 1
                    End If
                    'cadSQL = "SELECT a.proceso, IFNULL((SELECT COUNT(*) AS lotes FROM " & rutaBD & ".listado01_detalle WHERE proceso = a.proceso), 0) AS lotes FROM " & rutaBD & ".listado01_cabecera a GROUP BY a.proceso HAVING lotes > 0 ORDER BY a.proceso"
                    cadSQL = "SELECT a.proceso, COUNT(*) AS lotes FROM " & rutaBD & ".listado01_detalle a GROUP BY a.proceso ORDER BY a.proceso"
                    Dim procesos As DataSet = consultaSEL(cadSQL)
                    If procesos.Tables(0).Rows.Count > 0 Then
                        Dim unoAlMenos = False
                        For Each proceso In procesos.Tables(0).Rows
                            If proceso!proceso = 13 Then
                                Dim elvis2 = 1
                            End If

                            If proceso!lotes > 0 Then
                                cadSQL = "SELECT a.id, a.proceso, a.capacidad, b.hasta, b.secuencia, b.ultima_parte FROM " & rutaBD & ".cat_maquinas a LEFT JOIN " & rutaBD & ".listado01_cabecera b ON a.proceso = b.proceso AND a.id = b.equipo WHERE a.proceso = " & proceso!proceso & " AND (ISNULL(b.hasta) OR b.hasta <= '" & Format(fechaMayor, formatoFecha) & "') AND a.estatus = 'A' ORDER BY b.hasta"
                                Dim maquinas As DataSet = consultaSEL(cadSQL)
                                If maquinas.Tables(0).Rows.Count > 0 Then
                                    Dim LotesRestantes = proceso!lotes
                                    For Each maquina In maquinas.Tables(0).Rows
                                        Dim fechaMaquina = fecha
                                        secuencia = 0
                                        Dim uParte = 0
                                        If Not maquina!hasta.Equals(System.DBNull.Value) Then
                                            fechaMaquina = maquina!hasta
                                            secuencia = maquina!secuencia
                                            uParte = Val(ValNull(maquina!ultima_parte, "N"))
                                        End If
                                        Dim seguir = 0
                                        If LotesRestantes > (maquina!capacidad * 0.8) Then
                                            seguir = 1
                                        End If
                                        If seguir = 0 And DateAndTime.DateDiff(DateInterval.Hour, fechaMaquina, fechaMayor) > 4 Then
                                            seguir = 2
                                            fechaMaquina = fechaMayor
                                        End If
                                        If seguir = 0 And PasarUno Then
                                            seguir = 3
                                        End If

                                        If seguir > 0 Then
                                            Dim disponible = True
                                            'Se valida la disponibilidad de la máquina
                                            cadSQL = "SELECT hasta FROM " & rutaBD & ".detalleparos WHERE maquina = " & maquina!id & " AND estatus = 'A' AND estado NOT IN ('F', 'P') AND '" & Format(fechaMaquina, formatoFecha) & "' >= desde AND '" & Format(fechaMaquina, formatoFecha) & "' <= hasta ORDER BY hasta DESC"
                                            Dim nuevoHasta
                                            Dim paro As DataSet = consultaSEL(cadSQL)
                                            If paro.Tables(0).Rows.Count > 0 Then
                                                disponible = False
                                                nuevoHasta = paro.Tables(0).Rows(0)!hasta
                                            End If
                                            If disponible Then
                                                'Se simula el procesamiento del (de los) lote(s)
                                                cadSQL = "SELECT a.id, a.lote, a.parte, a.prioridad, a.ciclo, a.creacion, a.finalizo_anterior, a.secuencia, a.proceso, a.ruta, b.tiempo_proceso, b.tiempo_setup, b.tiempo_setup_idem FROM " & rutaBD & ".listado01_detalle a LEFT JOIN " & rutaBD & ".det_rutas b ON a.ruta_detalle = b.id WHERE a.proceso = " & proceso!proceso & " AND estado < 50 ORDER BY a.prioridad, a.creacion"
                                                lotes = consultaSEL(cadSQL)
                                                Dim pLote As Boolean = False
                                                Dim tSetup = 0
                                                Dim tProceso = 0
                                                Dim tLotes = 0
                                                Dim fHasta = fechaMaquina
                                                Dim uLote
                                                Dim procesar As Boolean = False
                                                For Each lote In lotes.Tables(0).Rows
                                                    If lote!lote = 2653 Then
                                                        Dim elvis = 1
                                                    End If
                                                    Dim esperaAdicional = 0
                                                    If Not PasarUno Then
                                                        If Not lote!finalizo_anterior.Equals(System.DBNull.Value) Then
                                                            If lote!finalizo_anterior > fechaMaquina Then
                                                                Continue For
                                                            End If
                                                        End If
                                                    ElseIf Not pLote Then
                                                        esperaAdicional = 0
                                                        If lote!finalizo_anterior > fechaMaquina Then
                                                            esperaAdicional = DateAndTime.DateDiff(DateInterval.Second, fechaMaquina, lote!finalizo_anterior)
                                                            fechaMaquina = lote!finalizo_anterior
                                                        End If


                                                        If fechaMaquina > fechaMayor Then
                                                            fechaMayor = fechaMaquina
                                                        End If
                                                        PasarUno = False
                                                    End If
                                                    tLotes = tLotes + 1
                                                    If tLotes > maquina!capacidad Then
                                                        Exit For
                                                    End If
                                                    procesar = True
                                                    secuencia = secuencia + 1
                                                    If Not pLote Then
                                                        tProceso = lote!tiempo_proceso
                                                        pLote = True
                                                        If uParte = lote!parte Then
                                                            tSetup = lote!tiempo_setup_idem
                                                        Else
                                                            tSetup = lote!tiempo_setup
                                                            uParte = lote!parte
                                                        End If

                                                        If fechaMaquina <= fechaMayor Then
                                                            fHasta = calcularFechaEstimada(fechaMaquina, tSetup + tProceso, proceso!proceso)
                                                            If fHasta > fechaMayor Then
                                                                fechaMayor = fHasta
                                                            End If
                                                            unoAlMenos = True
                                                        Else
                                                            Continue For
                                                        End If
                                                    End If

                                                    uLote = lote
                                                    'Se actualiza el lote
                                                    cadSQL = "SELECT id, secuencia, proceso FROM " & rutaBD & ".det_rutas WHERE ruta = " & lote!ruta & " AND secuencia = " & lote!secuencia + 1
                                                    Dim rutaSiguiente = consultaSEL(cadSQL)
                                                    Dim cadInsert = ""
                                                    Dim ultimaOperacion = False
                                                    If rutaSiguiente.Tables(0).Rows.Count = 0 Then
                                                        ultimaOperacion = True
                                                    Else
                                                        cadInsert = ";INSERT INTO " & rutaBD & ".listado01_detalle (lote, parte, ciclo, creacion, prioridad, proceso, ruta, ruta_detalle, secuencia, finalizo_anterior) VALUES (" & lote!lote & ", " & lote!parte & ", " & lote!ciclo + 1 & ", '" & Format(lote!creacion, formatoFecha) & "', " & lote!prioridad & ", " & rutaSiguiente.Tables(0).Rows(0)!proceso & ", " & lote!ruta & ", " & rutaSiguiente.Tables(0).Rows(0)!id & ", " & rutaSiguiente.Tables(0).Rows(0)!secuencia & ", '" & Format(fHasta, formatoFecha) & "')"
                                                    End If
                                                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".listado01_detalle SET equipo = " & maquina!id & ", proceso_desde = '" & Format(fechaMaquina, formatoFecha) & "', proceso_hasta = '" & Format(fHasta, formatoFecha) & "', tiempo_setup = " & tSetup & ", tiempo_proceso = " & tProceso & ", tiempo_espera = TIMESTAMPDIFF(SECOND, finalizo_anterior, '" & Format(fHasta, formatoFecha) & "'), orden = " & secuencia & IIf(ultimaOperacion, ", ultima = 'S', estado = 99", ", ultima = 'N', estado = 50") & ", tipo_proceso = " & seguir & " WHERE id = " & lote!id & cadInsert & IIf(ultimaOperacion, ";UPDATE " & rutaBD & ".listado01_detalle SET terminado = 'S' WHERE lote = " & lote!lote, ""))
                                                Next

                                                If Not procesar Then
                                                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".listado01_cabecera SET hasta = '" & Format(fechaMaquina, formatoFecha) & "' WHERE proceso = " & proceso!proceso & " AND equipo = " & maquina!id)
                                                    If regsAfectados = 0 Then
                                                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".listado01_cabecera (proceso, equipo, hasta) VALUES (" & proceso!proceso & ", " & maquina!id & ", '" & Format(fechaMaquina, formatoFecha) & "')")
                                                    End If
                                                    Continue For
                                                Else
                                                    LotesRestantes = LotesRestantes - tLotes
                                                    regsAfectados = consultaACT("UPDATE " & rutaBD & ".listado01_cabecera SET secuencia = " & secuencia & ", ultima_parte = " & uLote!parte & ", hasta = '" & Format(fHasta, formatoFecha) & "' WHERE proceso = " & proceso!proceso & " AND equipo = " & maquina!id)
                                                    If regsAfectados = 0 Then
                                                        regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".listado01_cabecera (proceso, equipo, hasta, secuencia, ultima_parte) VALUES (" & proceso!proceso & ", " & maquina!id & ", '" & Format(fHasta, formatoFecha) & "', " & secuencia & ", " & uParte & ")")
                                                    End If
                                                End If
                                            Else
                                                regsAfectados = consultaACT("UPDATE " & rutaBD & ".listado01_cabecera SET hasta = '" & Format(fechaMaquina, formatoFecha) & "' WHERE proceso = " & proceso!proceso & " AND equipo = " & maquina!id)
                                                If regsAfectados = 0 Then
                                                    regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".listado01_cabecera (proceso, equipo, hasta) VALUES (" & proceso!proceso & ", " & maquina!id & ", '" & Format(fechaMaquina, formatoFecha) & "')")
                                                End If
                                            End If
                                        End If
                                    Next
                                End If
                            End If
                        Next
                        If Not unoAlMenos Then
                            PasarUno = True
                            'fechaMayor = fecha
                            Dim elvis3 = 1
                        End If
                    End If
                    cadSQL = "SELECT COUNT(*) AS cuenta FROM " & rutaBD & ".listado01_detalle WHERE terminado = 'N' LIMIT 1"
                    Dim valSalir As DataSet = consultaSEL(cadSQL)
                    If valSalir.Tables(0).Rows.Count > 0 Then
                        If valSalir.Tables(0).Rows(0)!cuenta < 5 Then
                            Exit Do
                        End If

                    End If
                Loop
                regsAfectados = consultaACT("UPDATE " & rutaBD & ".configuracion SET listado01 = 'F', listado01_f = NOW();")
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

    Public Function consultaSEL(cadena As String, Optional miCadenaConexion As String = "") As DataSet
        Dim miConexion = New MySqlConnection

        Try
            errorBD = ""
            If miCadenaConexion = "" Then
                miCadenaConexion = cadenaConexion
            End If
            miConexion.ConnectionString = miCadenaConexion

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
        Catch ex As Exception
            errorBD = ex.Message
        Finally
            miConexion.Dispose()
            miConexion.Close()
            miConexion = Nothing
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
        calcularTiempoCad = horas & ": " & Format(minutos, "00") & ":" & Format(segundos, "00")
    End Function

    Private Sub agregarLOG(cadena As String, Optional reporte As Integer = 0, Optional tipo As Integer = 0, Optional aplicacion As Integer = 50)
        If Not be_log_activar Then Exit Sub
        'tipo 0: Info
        'tipo 2: Advertencia
        'tipo 9: Error
        Dim regsAfectados = consultaACT("INSERT INTO " & rutaBD & ".log (aplicacion, tipo, proceso, texto) VALUES (" & aplicacion & ", " & tipo & ", " & reporte & ", '" & Microsoft.VisualBasic.Strings.Left(cadena, 250) & "')")
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
    End Sub

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

End Module
