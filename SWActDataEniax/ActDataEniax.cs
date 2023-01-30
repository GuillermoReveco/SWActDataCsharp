using Newtonsoft.Json;
using SWActDataEniax.Conexion;
using SWActDataEniax.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SWActDataEniax
{
    partial class ActDataEniax : ServiceBase
    {
        bool blEnProceso = false;
        public static string urlNotificacionCitas { get; set; }
        public static string urlCambioEstado { get; set; }
        public static string xUserEniax { get; set; }
        public static string xPasswordEniax { get; set; }
        public static string xAuthorizationToken { get; set; }
        public static string conexionOracle { get; set; }
        public static string rangoMin { get; set; }
        public static string Etiqueta { get; set; }
        public static string fechaIni { get; set; }
        public static string fechaFin { get; set; }
        public static string rangoIni { get; set; }
        public static string rangoFin { get; set; }
        public static string horaEjec { get; set; }
        public static int logEjecProcID { get; set; }

        private int TimerConsulta;
        public ActDataEniax()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            int TimerCons;
            ConexionSQL con = new ConexionSQL();
            SalParametros regParametros = new SalParametros();
            regParametros = con.ConsultaParametros(EventLog);
            horaEjec = regParametros.horaEjecucion;
            rangoMin = regParametros.minFrecuencia;
            TimerCons = Int32.Parse(rangoMin);
            //TimerCons = double.Parse(rangoMin);
            //TimerCons = 1;
            TimerCons = TimerCons * 60000;

            rangoIni = regParametros.horaInicio;
            rangoIni = rangoIni.Trim() + ":00";
            rangoFin = regParametros.horaTermino;
            rangoFin = rangoFin.Trim() + ":00";

            conexionOracle = regParametros.conexionOracle;
            urlNotificacionCitas = regParametros.urlNotificacionCitas;
            urlCambioEstado = regParametros.urlCambioEstado;
            xUserEniax = regParametros.xUserEniax;
            xPasswordEniax = regParametros.xPasswordEniax;
            xAuthorizationToken = regParametros.xAuthorizationToken;

            //TimerCons = ConfigurationManager.AppSettings["TimerConsulta"].ToString();
            TimerConsulta = TimerCons;
            stLapso.Interval = TimerConsulta;
            stLapso.Enabled = true;
            //blEnProceso = false;
            //stLapso.Interval = TimerCons;
            stLapso.Start();// TODO: agregar código aquí para iniciar el servicio.
            // TODO: agregar código aquí para iniciar el servicio.
        }

        protected override void OnStop()
        {
            stLapso.Stop();// TODO: agregar código aquí para realizar cualquier anulación necesaria para detener el servicio.
            // TODO: agregar código aquí para realizar cualquier anulación necesaria para detener el servicio.
        }

        private void stLapso_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            stLapso.Stop();
            stLapso.Enabled = false;
            EventLog.WriteEntry("Inicio - Proceso - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), EventLogEntryType.Information);
            //EventLog.WriteEntry("INI - Proceso - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " " + blEnProceso, EventLogEntryType.Information);
            if (blEnProceso) return;
            //INI - Rescate de Parametros
            //int TimerCons;
            //ConexionSQL con = new ConexionSQL();
            //SalParametros regParametros = new SalParametros();
            //regParametros = con.ConsultaParametros();
            ////horaEjec = regParametros.horaEjecucion;
            //rangoMin = regParametros.minFrecuencia;
            ////TimerCons = Int32.Parse(rangoMin);
            //TimerCons = 15;
            //TimerCons = TimerCons * 60000;

            ////rangoIni = regParametros.horaInicio;
            ////rangoIni = rangoIni.Trim() + ":00";
            ////rangoFin = regParametros.horaTermino;
            ////rangoFin = rangoFin.Trim() + ":00";

            //conexionOracle = regParametros.conexionOracle;
            //urlNotificacionCitas = regParametros.urlNotificacionCitas;
            //urlCambioEstado = regParametros.urlCambioEstado;
            //xUserEniax = regParametros.xUserEniax;
            //xPasswordEniax = regParametros.xPasswordEniax;
            //xAuthorizationToken = regParametros.xAuthorizationToken;
            //FIN - Rescate de Parametros
            try
            {
                blEnProceso = true;
                ProcesoPrincipal();

            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Problemas en Servicio ActDataEniax: " + ex.Message, EventLogEntryType.Error);
            }

            EventLog.WriteEntry("Fin - Proceso - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), EventLogEntryType.Information);
            blEnProceso = false;
            stLapso.Interval = TimerConsulta;
            stLapso.Enabled = true;
            stLapso.Start();

        }
        private async Task ProcesoPrincipal()
        {
            //ServiceBase.EventLog.WriteEntry("Inicio - Proceso - ", EventLogEntryType.Information);
            //EventLog.WriteEntry("Inicio - Proceso - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), EventLogEntryType.Information);
            //Console.WriteLine("Inicio - Proceso - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            ConexionOracle cnn = new ConexionOracle();
            cnn.var_cadenaconexion = conexionOracle;
            cnn.var_min = rangoMin.Trim();

            // RescataFechas
            //Console.WriteLine("Rescata Fechas - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            //EventLog.WriteEntry("Rescata Fechas - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), EventLogEntryType.Information);
            cnn.RescataFechas(EventLog);
            fechaIni = cnn.var_FechaRango;
            fechaFin = cnn.var_FechaActual;

            // Notificacion de Citas
            //Console.WriteLine("Notificacion de Citas - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            //EventLog.WriteEntry("Notificacion de Citas - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), EventLogEntryType.Information);
            var dataProc = new LOGEjecProceso();
            dataProc.logEjecProcNom = "Notificacion de Citas";
            dataProc.logEjecFecIni = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            var listNotCitas = new List<NotificacionCita>();
            listNotCitas = cnn.ConsultaNotificacionCitas(EventLog); // Notificacion de Citas

            ConexionSQL conn = new ConexionSQL();   // logEjecProcID
            dataProc.logEjecFecFin = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            logEjecProcID = conn.IngLOGEjecProceso(dataProc, EventLog);

            //conn.var_cadenaconexion = conexionSQL;
            foreach (var dataCon in listNotCitas)
            {
                var dataApi = new NotificacionCita();
                dataApi.id_cita = dataCon.id_cita;
                dataApi.fecha_cita = dataCon.fecha_cita;
                dataApi.hora_cita = dataCon.hora_cita;
                dataApi.tipo_cita = dataCon.tipo_cita;
                dataApi.duracion_cita = dataCon.duracion_cita;
                dataApi.fecha_agendamiento = dataCon.fecha_agendamiento;
                dataApi.hora_agendamiento = dataCon.hora_agendamiento;
                dataApi.canal_agendamiento = dataCon.canal_agendamiento;
                dataApi.responsable_agendamiento = dataCon.responsable_agendamiento;
                dataApi.es_sobrecupo = dataCon.es_sobrecupo;

                dataApi.es_reagendamiento = dataCon.es_reagendamiento;
                dataApi.es_paquete = dataCon.es_paquete;
                dataApi.id_paquete_padre = dataCon.id_paquete_padre;
                dataApi.instrucciones_previas = dataCon.instrucciones_previas;
                dataApi.estado = dataCon.estado;
                dataApi.identificacion_paciente = dataCon.identificacion_paciente;
                dataApi.tipo_identificacion = dataCon.tipo_identificacion;
                dataApi.cod_paciente = dataCon.cod_paciente;
                dataApi.nombre_paciente = dataCon.nombre_paciente;
                dataApi.primer_apellido_paciente = dataCon.primer_apellido_paciente;

                dataApi.segundo_apellido_paciente = dataCon.segundo_apellido_paciente;
                dataApi.sexo_paciente = dataCon.sexo_paciente;
                dataApi.fecha_nacimiento_paciente = dataCon.fecha_nacimiento_paciente;
                dataApi.direccion_paciente = dataCon.direccion_paciente;
                dataApi.prevision_paciente = dataCon.prevision_paciente;
                dataApi.correo_electronico_paciente = dataCon.correo_electronico_paciente;
                dataApi.nro_telefono_movil_paciente = dataCon.nro_telefono_movil_paciente;
                dataApi.nro_telefono_fijo_paciente = dataCon.nro_telefono_fijo_paciente;
                dataApi.cod_centro_atencion = dataCon.cod_centro_atencion;
                dataApi.nombre_centro_atencion = dataCon.nombre_centro_atencion;

                dataApi.lugar_atencion = dataCon.lugar_atencion;
                dataApi.piso_atencion = dataCon.piso_atencion;
                dataApi.box_atencion = dataCon.box_atencion;
                dataApi.cod_especialidad = dataCon.cod_especialidad;
                dataApi.nombre_especialidad = dataCon.nombre_especialidad;
                dataApi.identificacion_profesional = dataCon.identificacion_profesional;
                dataApi.tipo_identificacion_profesional = dataApi.tipo_identificacion_profesional;
                dataApi.cod_profesional = dataCon.cod_profesional;
                dataApi.nombre_profesional = dataCon.nombre_profesional;
                dataApi.primer_apellido_profesional = dataCon.primer_apellido_profesional;

                dataApi.segundo_apellido_profesional = dataCon.segundo_apellido_profesional;
                dataApi.sexo_profesional = dataCon.sexo_profesional;
                dataApi.prefijo_doctor = dataCon.prefijo_doctor;
                dataApi.cod_prestacion = dataCon.cod_prestacion;
                dataApi.nombre_prestacion = dataCon.nombre_prestacion;
                dataApi.url_video_conferencia = dataCon.url_video_conferencia;
                dataApi.url_sala_espera = dataCon.url_sala_espera;
                dataApi.url_pago_online = dataCon.url_pago_online;
                dataApi.categoria_paciente = dataCon.categoria_paciente;

                var Ind = conn.ConsultaCita(Convert.ToInt32(dataApi.id_cita), "I", 0, EventLog);
                if (Ind == 0)
                {
                    // Llamadao a Api
                    var prog = new ActDataEniax();
                    var resultado = await prog.LlamaApiNotificacionCitas(dataApi, EventLog);

                }

            }

            var dataProcAct = new ActLOGEjecProceso();
            ConexionSQL conns = new ConexionSQL();

            dataProcAct.logEjecProcID = logEjecProcID;
            dataProcAct.logEjecProcNom = dataProc.logEjecProcNom;
            dataProcAct.logEjecFecIni = dataProc.logEjecFecIni;
            dataProcAct.logEjecFecFin = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            conns.ActLOGEjecProceso(dataProcAct, EventLog);

            // Confirmada (1)
            //Console.WriteLine("Confirmada de Citas - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            //EventLog.WriteEntry("Confirmada de Citas - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), EventLogEntryType.Information);
            var dataConf = new LOGEjecProceso();
            dataConf.logEjecProcNom = "Confirmada de Citas";
            dataConf.logEjecFecIni = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            var listRegConfirmadas = new List<CambioEstCita>();
            listRegConfirmadas = cnn.ConsultaConfirmadas(EventLog); // Confirmada (1)
            ConexionSQL conc = new ConexionSQL();
            dataConf.logEjecFecFin = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            logEjecProcID = conc.IngLOGEjecProceso(dataConf, EventLog);

            //conc.var_cadenaconexion = conexionSQL;
            foreach (var dataCon in listRegConfirmadas)
            {
                var dataApi = new CambioEstCita();
                dataApi.id_cita = dataCon.id_cita;
                dataApi.estado = dataCon.estado;
                dataApi.fecha = dataCon.fecha;
                dataApi.responsable = dataCon.responsable;
                dataApi.canal_estado = dataCon.canal_estado;
                dataApi.motivo = dataCon.motivo;
                dataApi.informacion_adicional = dataCon.informacion_adicional;
                var Ind = conc.ConsultaCita(Convert.ToInt32(dataApi.id_cita), "M", Convert.ToInt32(dataApi.estado), EventLog);
                if (Ind == 0)
                {
                    // Llamadao a Api
                    var prog = new ActDataEniax();
                    var resultado = await prog.LlamaApiCambioEstado(dataApi, EventLog);

                }

            }


            var dataProcConf = new ActLOGEjecProceso();
            ConexionSQL connc = new ConexionSQL();

            dataProcConf.logEjecProcID = logEjecProcID;
            dataProcConf.logEjecProcNom = dataConf.logEjecProcNom;
            dataProcConf.logEjecFecIni = dataConf.logEjecFecIni;
            dataProcConf.logEjecFecFin = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            connc.ActLOGEjecProceso(dataProcConf, EventLog);

            // Anulacion y Bloqueadas (2/3)
            //Console.WriteLine("Anulacion y Bloqueadas de Citas - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            //EventLog.WriteEntry("Anulacion y Bloqueadas de Citas - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), EventLogEntryType.Information);
            var dataAnuBlo = new LOGEjecProceso();
            dataAnuBlo.logEjecProcNom = "Anulacion y Bloqueadas de Citas";
            dataAnuBlo.logEjecFecIni = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            var listRegAnuBloq = new List<CambioEstCita>();
            listRegAnuBloq = cnn.ConsultaAnuBloq(EventLog); // Anuladas y Bloqueadas (2/3)

            //for (int i = 0; i < listRegAnuBloq.Count; i++)
            //{
            //    var dataApi = new CambioEstCita();
            //    dataApi.id_cita = listRegAnuBloq[i].id_cita;
            //    dataApi.estado = listRegAnuBloq[i].estado;
            //    dataApi.fecha = listRegAnuBloq[i].fecha;
            //    dataApi.responsable = listRegAnuBloq[i].responsable;
            //    dataApi.canal_estado = listRegAnuBloq[i].canal_estado;
            //    dataApi.motivo = listRegAnuBloq[i].motivo;
            //    dataApi.informacion_adicional = listRegAnuBloq[i].informacion_adicional;
            //    // Llamadao a Api
            //    var prog = new Program();
            //    var resultado = await prog.LlamaApiCambioEstado(dataApi);

            //}
            ConexionSQL con = new ConexionSQL();
            dataAnuBlo.logEjecFecFin = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            logEjecProcID = con.IngLOGEjecProceso(dataAnuBlo, EventLog);
            //con.var_cadenaconexion = conexionSQL;
            foreach (var dataCon in listRegAnuBloq)
            {
                var dataApi = new CambioEstCita();
                dataApi.id_cita = dataCon.id_cita;
                dataApi.estado = dataCon.estado;
                dataApi.fecha = dataCon.fecha;
                dataApi.responsable = dataCon.responsable;
                dataApi.canal_estado = dataCon.canal_estado;
                dataApi.motivo = dataCon.motivo;
                dataApi.informacion_adicional = dataCon.informacion_adicional;
                var Ind = con.ConsultaCita(Convert.ToInt32(dataApi.id_cita), "M", Convert.ToInt32(dataApi.estado), EventLog);
                if (Ind == 0)
                {
                    // Llamadao a Api
                    var prog = new ActDataEniax();
                    var resultado = await prog.LlamaApiCambioEstado(dataApi, EventLog);

                }

            }

            var dataProcAnu = new ActLOGEjecProceso();
            ConexionSQL conna = new ConexionSQL();

            dataProcAnu.logEjecProcID = logEjecProcID;
            dataProcAnu.logEjecProcNom = dataAnuBlo.logEjecProcNom;
            dataProcAnu.logEjecFecIni = dataAnuBlo.logEjecFecIni;
            dataProcAnu.logEjecFecFin = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            conna.ActLOGEjecProceso(dataProcAnu, EventLog);

            // Paciente Pago Cita (5/6)
            //Console.WriteLine("Paciente Pago Cita - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            //EventLog.WriteEntry("Paciente Pago Cita - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), EventLogEntryType.Information);
            var dataPaPag = new LOGEjecProceso();
            dataPaPag.logEjecProcNom = "Paciente Pago Cita";
            dataPaPag.logEjecFecIni = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            var listRegPacPag = new List<CambioEstCita>();
            listRegPacPag = cnn.ConsultaPacPag(EventLog); // Paciente Pago Cita (5/6)

            ConexionSQL conp = new ConexionSQL();
            dataPaPag.logEjecFecFin = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            logEjecProcID = conp.IngLOGEjecProceso(dataPaPag, EventLog);
            //conp.var_cadenaconexion = conexionSQL;
            foreach (var dataCon in listRegPacPag)
            {
                var dataApi = new CambioEstCita();
                dataApi.id_cita = dataCon.id_cita;
                dataApi.estado = dataCon.estado;
                dataApi.fecha = dataCon.fecha;
                dataApi.responsable = dataCon.responsable;
                dataApi.canal_estado = dataCon.canal_estado;
                dataApi.motivo = dataCon.motivo;
                dataApi.informacion_adicional = dataCon.informacion_adicional;
                var Ind = conp.ConsultaCita(Convert.ToInt32(dataApi.id_cita), "M", Convert.ToInt32(dataApi.estado), EventLog);
                if (Ind == 0)
                {
                    // Llamadao a Api
                    var prog = new ActDataEniax();
                    var resultado = await prog.LlamaApiCambioEstado(dataApi, EventLog);

                }

            }

            var dataProcPag = new ActLOGEjecProceso();
            ConexionSQL connp = new ConexionSQL();

            dataProcPag.logEjecProcID = logEjecProcID;
            dataProcPag.logEjecProcNom = dataPaPag.logEjecProcNom;
            dataProcPag.logEjecFecIni = dataPaPag.logEjecFecIni;
            dataProcPag.logEjecFecFin = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            connp.ActLOGEjecProceso(dataProcPag, EventLog);

            //}
            //Console.WriteLine("Fin - Proceso - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            //EventLog.WriteEntry("Fin - Proceso - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), EventLogEntryType.Information);

        }
        private async Task<bool> LlamaApiCambioEstado(CambioEstCita dataBody, EventLog log)
        {
            string url;
            string gloMetodo = "";
            switch (dataBody.estado)
            {
                case "0":
                    gloMetodo = "NOTI CITAS";
                    break;
                case "1":
                    gloMetodo = "CONF CITAS";
                    break;
                case "2":
                    gloMetodo = "ANU CITA";
                    break;
                case "3":
                    gloMetodo = "BLOQ CITAS";
                    break;
                case "5":
                    gloMetodo = "PAGO CITA";
                    break;
                case "14":
                    gloMetodo = "NO ASISTE";
                    break;
                default:
                    gloMetodo = "";
                    break;
            }

            url = urlCambioEstado + dataBody.id_cita.Trim();

            var jsonString = JsonConvert.SerializeObject(dataBody);
            HttpContent httpContent = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");

            HttpClient client = new HttpClient();

            //string contentType = "application/json";
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            client.DefaultRequestHeaders.Add("X-User-Eniax", xUserEniax);
            client.DefaultRequestHeaders.Add("X-Password-Eniax", xPasswordEniax);
            client.DefaultRequestHeaders.Add("X-Authorization-Token", xAuthorizationToken);
            Etiqueta = "[PutAsync]";
            //try
            //{

            var httpResponse = await client.PutAsync(url, httpContent);
            //var httpResponse = await client.PutAsJsonAsync() .PutAsync(url, httpContent);

            if (httpResponse.IsSuccessStatusCode)
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                //var dataApi = JsonSerializer.Deserialize<ApiData>(content);
                var respApi = JsonConvert.DeserializeObject<RespApi>(content);

                ConexionSQL con = new ConexionSQL();
                var citaProcesada = new CitaProcesada();
                citaProcesada.id_cita = dataBody.id_cita;
                citaProcesada.accion = "M";
                citaProcesada.estado = dataBody.estado;
                citaProcesada.fecha_envio = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                citaProcesada.fecha_cita = dataBody.fecha;
                citaProcesada.id_transaction = respApi.id_transaccion;
                //con.IngCitaProcesada(citaProcesada, EventLog); // Ingresar Cita Procesada
                con.IngCitaProcesada(citaProcesada, log); // Ingresar Cita Procesada

                var logApi = new LOGApi();
                logApi.codError = (int)httpResponse.StatusCode; ///200;
                logApi.gloError = httpResponse.ReasonPhrase;//respApi.message;
                logApi.logEjecProcID = logEjecProcID;
                logApi.logid_cita = dataBody.id_cita;
                logApi.fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                logApi.fechaIni = fechaIni;
                logApi.fechaFin = fechaFin;
                logApi.tipoMetodo = gloMetodo; // "CAM ESTADO";
                logApi.urlMetodo = url;
                logApi.body = jsonString;
                con.IngLOGApi(logApi, log); // Ingresar LOG

            }
            else
            {
                ConexionSQL con = new ConexionSQL();
                var logApi = new LOGApi();
                logApi.codError = (int)httpResponse.StatusCode;  //500;
                logApi.gloError = httpResponse.ReasonPhrase;//"Error, al Llamar a API";
                logApi.logEjecProcID = logEjecProcID;
                logApi.logid_cita = dataBody.id_cita;
                logApi.fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                logApi.fechaIni = fechaIni;
                logApi.fechaFin = fechaFin;
                logApi.tipoMetodo = gloMetodo;//"CAM ESTADO";
                logApi.urlMetodo = url;
                logApi.body = jsonString;
                con.IngLOGApi(logApi, log); // Ingresar LOG

            }
            //}
            //catch (HttpRequestException httpEx)
            //{
            //    var statusCodeString = httpEx.Message.Substring(ErrorStatusCodeStart.Length, 3);
            //    var statusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), statusCodeString);

            //    ConexionSQL con = new ConexionSQL();
            //    con.var_cadenaconexion = conexionSQL;
            //    var logApi = new LOGApi();
            //    logApi.codError = (int)statusCode;  //500;
            //    logApi.gloError = "Error, al Llamar a API";
            //    logApi.fecha = DateTime.Now.ToString();
            //    logApi.tipoMetodo = "CAM ESTADO";
            //    logApi.urlMetodo = url;
            //    logApi.body = jsonString;
            //    con.IngLOGApi(logApi); // Ingresar LOG

            //}
            return true;
        }
        private async Task<bool> LlamaApiNotificacionCitas(NotificacionCita dataBody, EventLog log)
        {
            string url;

            url = urlNotificacionCitas;

            var jsonString = JsonConvert.SerializeObject(dataBody);
            HttpContent httpContent = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");

            HttpClient client = new HttpClient();

            //string contentType = "application/json";
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            client.DefaultRequestHeaders.Add("X-User-Eniax", xUserEniax);
            client.DefaultRequestHeaders.Add("X-Password-Eniax", xPasswordEniax);
            client.DefaultRequestHeaders.Add("X-Authorization-Token", xAuthorizationToken);
            Etiqueta = "[PostAsync]";
            var httpResponse = await client.PostAsync(url, httpContent);
            //var httpResponse = await client.PutAsJsonAsync() .PutAsync(url, httpContent);

            if (httpResponse.IsSuccessStatusCode)
            {
                var content = await httpResponse.Content.ReadAsStringAsync();
                //var dataApi = JsonSerializer.Deserialize<ApiData>(content);
                var respApi = JsonConvert.DeserializeObject<RespApi>(content);

                ConexionSQL con = new ConexionSQL();
                //con.var_cadenaconexion = conexionSQL;
                var citaProcesada = new CitaProcesada();
                citaProcesada.id_cita = dataBody.id_cita;
                citaProcesada.accion = "I";
                citaProcesada.estado = "0";
                citaProcesada.fecha_envio = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                citaProcesada.fecha_cita = dataBody.fecha_agendamiento;
                citaProcesada.id_transaction = respApi.id_transaccion;
                //con.IngCitaProcesada(citaProcesada, EventLog); // Ingresar Cita Procesada
                con.IngCitaProcesada(citaProcesada, log); // Ingresar Cita Procesada

                var logApi = new LOGApi();
                logApi.codError = (int)httpResponse.StatusCode;//200;
                logApi.gloError = httpResponse.ReasonPhrase; //respApi.message;
                logApi.logEjecProcID = logEjecProcID;
                logApi.logid_cita = dataBody.id_cita;
                logApi.fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                logApi.fechaIni = fechaIni;
                logApi.fechaFin = fechaFin;
                logApi.tipoMetodo = "NOTI CITAS";
                logApi.urlMetodo = url;
                logApi.body = jsonString;
                con.IngLOGApi(logApi, log); // Ingresar LOG

            }
            else
            {
                ConexionSQL con = new ConexionSQL();
                //con.var_cadenaconexion = conexionSQL;
                var logApi = new LOGApi();
                logApi.codError = (int)httpResponse.StatusCode;//500;
                logApi.gloError = httpResponse.ReasonPhrase; //"Error, al Llamar a API";
                logApi.logEjecProcID = logEjecProcID;
                logApi.logid_cita = dataBody.id_cita;
                logApi.fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                logApi.fechaIni = fechaIni;
                logApi.fechaFin = fechaFin;
                logApi.tipoMetodo = "NOTI CITAS";
                logApi.urlMetodo = url;
                logApi.body = jsonString;
                con.IngLOGApi(logApi, log); // Ingresar LOG

            }
            return true;
        }

    }
}
