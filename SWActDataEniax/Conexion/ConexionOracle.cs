using Oracle.ManagedDataAccess.Client;
using SWActDataEniax.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWActDataEniax.Conexion
{
    class ConexionOracle
    {
        public string var_cadenaconexion = "";
        public string var_min = "";
        public string var_FechaActual = "";
        public string var_FechaRango = "";

        OracleConnection var_conexion = new OracleConnection();

        private void abrirconexion()
        {
            var_conexion.ConnectionString = var_cadenaconexion;
            if (var_conexion.State == ConnectionState.Closed)
                var_conexion.Open();
        }
        private void cerrarconexion()
        {
            if (var_conexion.State == ConnectionState.Open)
                var_conexion.Close();
        }
        public void RescataFechas(EventLog log)
        {
            try
            {
                string queryString = "select ";
                queryString += "  to_char(sysdate, 'dd/mm/yyyy hh24:mi') FecActual ";
                queryString += ", to_char(sysdate - " + var_min + "/1440, 'dd/mm/yyyy hh24:mi') FecRango ";
                queryString += "from dual ";

                OracleCommand cmd = new OracleCommand(queryString, var_conexion);
                abrirconexion();

                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var_FechaActual = dr["FecActual"].ToString();
                    var_FechaRango = dr["FecRango"].ToString();
                }

                dr.Close();
                cerrarconexion();

            }
            catch (Exception ex)
            {
                //Console.WriteLine("Hay error al acceder a la base de datos(RescataFechas). " + ex.Message);
                //Console.ReadLine();
                log.WriteEntry("Hay error al acceder a la base de datos Oracle(RescataFechas): " + ex.Message, EventLogEntryType.Error);
                try
                {
                    ConexionSQL con = new ConexionSQL();
                    var dataSW = new SWLOG();
                    dataSW.fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    dataSW.Proceso = "ActDataEniax";
                    dataSW.GloError = "Hay error al acceder a la base de datos Oracle(RescataFechas): " + ex.Message;
                    con.IngSWLOG(dataSW, log);
                }
                catch (Exception err)
                {
                    log.WriteEntry("Hay error al acceder a la base de datos SQL(IngSWLOG): " + err.Message, EventLogEntryType.Error);
                }
                //throw;
            }

        }
        public List<NotificacionCita> ConsultaNotificacionCitas(EventLog log)
        {
            List<NotificacionCita> listNotificacionCitas = new List<NotificacionCita>();

            try
            {
                string queryString = "select ";
                queryString += "r.correl_reserva  id_cita ";
                //queryString += ", r.fecha_reserva  fecha_cita";
                queryString += ", to_char(r.fecha_reserva,'yyyy-mm-dd') fecha_cita";
                queryString += ", r.Hora_reserva   hora_cita";
                queryString += ", 1 Tipo_cita";
                queryString += ", 10  Duracion_cita";
                queryString += ", to_char(fecing_reserva,'yyyy-mm-dd')  Fecha_agendamiento";
                queryString += ", to_char(fecing_reserva,'HH24:mi')  Hora_agendamiento";
                queryString += ", case  r.resping_reserva";
                queryString += " when 'UINTERNET' then 'INTERNET'";
                queryString += " when 'UAVIRTUAL' then 'ALICIA'";
                queryString += " when 'TELEC_ENX' then 'TELECONSULTA'";
                queryString += " else";
                queryString += " decode ( nvl( r.resping_reserva,'X') ,'X','X',  decode ( (select count(*)  from acc_usuarios_r_grupos where cod_empresa=8 and  cod_grupo=1 and id_usuario=r.resping_reserva),0,   decode (   (select count(*)  from acc_usuarios_r_grupos where cod_empresa=8 and   cod_grupo in (172,163)   and id_usuario=r.resping_reserva)  , 0,   decode (  (select count(*)  from acc_usuarios_r_grupos where cod_empresa=8 and  (cod_grupo<> 172 and cod_grupo<>163  and cod_grupo<>1) and id_usuario=r.resping_reserva ) , 0,'NN','PRESENCIAL'), 'TRUSTCORP' )   ,'CALL_CENTER'))";
                queryString += " end  Canal_agendamiento ";
                queryString += ", resping_reserva responsable_agendamiento";
                queryString += ", ind_sobrecupo es_sobrecupo";
                queryString += ", 'N' Es_reagendamiento";
                queryString += ", 'N' Es_paquete";
                queryString += ", '' id_paquete_padre";
                queryString += ", '' instruciones_previas";
                queryString += ", 'NUEVO' estado";

                queryString += ", decode(nvl(rut_paciente,0),0,null,  tp.rut_paciente||'-'|| tp.dv_paciente ) rut_paciente";
                queryString += ", decode(nvl(rut_paciente,'0'),'0',tp.pasaporte,  null ) pasaporte";
                queryString += ", decode(nvl(rut_paciente,0),0,'PASAPORTE', 'RUT' ) tipo_identificacion";
                //queryString += ", tp.rut_paciente";
                //queryString += ", tp.dv_paciente";
                //queryString += ", tp.pasaporte";
                //queryString += ", nvl(tp.tipo_documento_identidad,1) tipo_identificacion";
                queryString += ", r.ID_AMBULATORIO  codigo_paciente";
                queryString += ", tp.nombre_paciente";
                queryString += ", tp.apepat_paciente primer_apellido";
                queryString += ", tp.apemat_paciente segundo_apellido";
                queryString += ", tp.sexo_paciente";
                queryString += ", to_char( tp.fecha_nac_paciente,'YYYY-MM-DD')  fecha_nac_paciente";
                queryString += ", tp.email_paciente correo_electronico_paciente";
                queryString += ", tp.numero_celular nro_telefono_movil_paciente";
                queryString += ", FONO_PRINC_PACIENTE";
                queryString += ", cod_unidad cod_centro_atencion";
                queryString += ", generales.unidades(r.cod_empresa,r.cod_sucursal,r.cod_unidad, 'DESCRIP') nombre_centro_atencion";
                queryString += ", decode(r.cod_sucursal,1,'ALAMEDA',2,'MAIPU') Lugar_atencion";
                queryString += ", box_reserva box_atencion";
                queryString += ", r.cod_especialidad";
                queryString += ", generales.tabparamet(r.cod_empresa,4,r.cod_especialidad) nombre_especialidad";
                queryString += ", generales.profesional(r.cod_empresa,r.cod_prof,'RUT') identificacion_profesional";
                queryString += ", 'RUT' tipo_identificacion_prof";
                queryString += ", r.cod_prof codigo_profesional";
                queryString += ", generales.profesional(r.cod_empresa,r.cod_prof,'NOMBRE_SOLO') nombre_profesional";
                queryString += ", generales.profesional(r.cod_empresa,r.cod_prof,'AP_PATER') primer_ap_profesional";
                queryString += ", generales.profesional(r.cod_empresa,r.cod_prof,'AP_MATER') segundo_ap_profesional";
                queryString += ", 'M' Sexo_profesional ";
                queryString += "from ";
                queryString += "am_Reserva r ";
                queryString += ", tab_paciente tp ";
                queryString += "where r.cod_empresa=8 ";
                queryString += " and r.cod_sucursal>=1";
                queryString += " and r.cod_unidad>=0";
                queryString += " and tp.cod_empresa=r.cod_empresa";
                queryString += " and tp.id_ambulatorio=r.id_ambulatorio";

                //queryString += " and to_date(fecing_reserva,'dd/mm/yyyy') BETWEEN TO_DATE(sysdate - " + var_min + "/1440, 'dd/mm/yyyy') AND TO_DATE(sysdate, 'dd/mm/yyyy') ";
                //queryString += " and to_char(fecing_reserva,'hh24:mi') >= to_char(sysdate - " + var_min + "/1440, 'hh24:mi') ";
                queryString += " and fecing_reserva>=to_date('" + var_FechaRango + "','dd/mm/yyyy hh24:mi') ";
                queryString += " and fecing_reserva< to_date('" + var_FechaActual + "','dd/mm/yyyy hh24:mi') ";
                //queryString += " and fecing_reserva>=to_date('07/07/2022 15:00','dd/mm/yyyy hh24:mi') ";
                //queryString += " and fecing_reserva<=to_date('07/07/2022 16:50','dd/mm/yyyy hh24:mi') ";
                queryString += " and (r.modulo_reserva<>'KIN2I') ";

                queryString += " and tp.id_ambulatorio not in ";
                queryString += " ( ";
                queryString += " select ";
                queryString += " id_ambulatorio ";
                queryString += " from ";
                queryString += " tab_paciente p ";
                queryString += " where ";
                queryString += " p.cod_empresa=8 ";
                queryString += " and p.vigencia='S' ";
                queryString += " and  ( ";
                queryString += " p.apepat_paciente like '%NODAR%' ";
                queryString += " or p.apepat_paciente like '%NO DAR%' ";
                queryString += " or p.apepat_paciente like '%AGENDAR%' ";
                queryString += " or p.apepat_paciente like '%RESERV%' ";
                queryString += " or p.apepat_paciente like '%HORARIO%' ";
                queryString += " or p.apepat_paciente like '%MEDICO%' ";
                queryString += " or p.apepat_paciente like '%NINGUNA%' ";
                queryString += " or p.apepat_paciente like '%NO VIENE%' ";
                queryString += " or p.apepat_paciente like '%BLOQUE%' ";
                queryString += " or p.apepat_paciente like '%PACIENTE%' ";
                queryString += " or p.apepat_paciente like '%HOSPITA%' ";
                queryString += " or p.apepat_paciente like '%CONTROL%' ";
                queryString += " or p.apepat_paciente like '%HORA%' ";
                queryString += " or p.apepat_paciente like '%INTERCONS%' ";
                queryString += " or p.apepat_paciente like '%ANULA%' ";
                queryString += " or p.apepat_paciente like '%NO BORRAR%' ";
                queryString += " or p.apepat_paciente like '%NO ANOTAR%' ";
                queryString += " or p.apepat_paciente like '%NO ASISTE%' ";
                queryString += " or p.apepat_paciente like '%RES NO%' ";
                queryString += " or p.apepat_paciente like '%NO CAMBIAR%' ";
                queryString += " or p.apepat_paciente like '%REPETI%' ";
                queryString += " or p.apepat_paciente like '%RESER%' ";
                queryString += " or p.apepat_paciente like '% RESPIRATORIO%' ";
                queryString += " or p.apepat_paciente like '%NO CITAR%' ";
                queryString += " or p.apepat_paciente like '%NO CAMBIAR%' ";
                queryString += " or p.apepat_paciente like '%RESER%' ";
                queryString += " or p.apepat_paciente like '%NO ESCRIBIR%' ";
                queryString += " ) ";
                queryString += " ) ";

                OracleCommand cmd = new OracleCommand(queryString, var_conexion);
                abrirconexion();

                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    NotificacionCita regNotCitas = new NotificacionCita();
                    regNotCitas.id_cita = dr["id_cita"].ToString();
                    regNotCitas.fecha_cita = dr["fecha_cita"].ToString();
                    regNotCitas.hora_cita = dr["hora_cita"].ToString();
                    regNotCitas.tipo_cita = dr["tipo_cita"].ToString();
                    regNotCitas.duracion_cita = dr["duracion_cita"].ToString();
                    regNotCitas.fecha_agendamiento = dr["fecha_agendamiento"].ToString();
                    regNotCitas.hora_agendamiento = dr["hora_agendamiento"].ToString();
                    regNotCitas.canal_agendamiento = dr["canal_agendamiento"].ToString();
                    regNotCitas.responsable_agendamiento = dr["responsable_agendamiento"].ToString();
                    regNotCitas.es_sobrecupo = dr["es_sobrecupo"].ToString();

                    regNotCitas.es_reagendamiento = dr["es_reagendamiento"].ToString();
                    regNotCitas.es_paquete = dr["es_paquete"].ToString();
                    regNotCitas.id_paquete_padre = dr["id_paquete_padre"].ToString();
                    regNotCitas.instrucciones_previas = dr["instruciones_previas"].ToString();
                    regNotCitas.estado = dr["estado"].ToString();

                    if(dr["tipo_identificacion"].ToString() == "PASAPORTE")
                    {
                        regNotCitas.identificacion_paciente = dr["pasaporte"].ToString();
                    }
                    else
                    {
                        regNotCitas.identificacion_paciente = dr["rut_paciente"].ToString();
                    }
                    regNotCitas.tipo_identificacion = dr["tipo_identificacion"].ToString();

                    regNotCitas.cod_paciente = dr["codigo_paciente"].ToString();
                    regNotCitas.nombre_paciente = dr["nombre_paciente"].ToString();
                    regNotCitas.primer_apellido_paciente = dr["primer_apellido"].ToString();

                    regNotCitas.segundo_apellido_paciente = dr["segundo_apellido"].ToString();
                    regNotCitas.sexo_paciente = dr["sexo_paciente"].ToString();
                    regNotCitas.fecha_nacimiento_paciente = dr["fecha_nac_paciente"].ToString();
                    regNotCitas.direccion_paciente = "";
                    regNotCitas.prevision_paciente = "";
                    regNotCitas.correo_electronico_paciente = dr["correo_electronico_paciente"].ToString();
                    regNotCitas.nro_telefono_movil_paciente = dr["nro_telefono_movil_paciente"].ToString();
                    regNotCitas.nro_telefono_fijo_paciente = dr["fono_princ_paciente"].ToString();
                    regNotCitas.cod_centro_atencion = dr["cod_centro_atencion"].ToString();
                    regNotCitas.nombre_centro_atencion = dr["nombre_centro_atencion"].ToString();

                    regNotCitas.lugar_atencion = dr["lugar_atencion"].ToString();
                    regNotCitas.piso_atencion = "";
                    regNotCitas.box_atencion = dr["box_atencion"].ToString();
                    regNotCitas.cod_especialidad = dr["cod_especialidad"].ToString();
                    regNotCitas.nombre_especialidad = dr["nombre_especialidad"].ToString();
                    regNotCitas.identificacion_profesional = dr["identificacion_profesional"].ToString();
                    regNotCitas.tipo_identificacion_profesional = dr["tipo_identificacion_prof"].ToString();
                    regNotCitas.cod_profesional = dr["codigo_profesional"].ToString();
                    regNotCitas.nombre_profesional = dr["nombre_profesional"].ToString();
                    regNotCitas.primer_apellido_profesional = dr["primer_ap_profesional"].ToString();

                    regNotCitas.segundo_apellido_profesional = dr["segundo_ap_profesional"].ToString();
                    regNotCitas.sexo_profesional = dr["sexo_profesional"].ToString();
                    regNotCitas.prefijo_doctor = "";
                    regNotCitas.cod_prestacion = "";
                    regNotCitas.nombre_prestacion = "";
                    regNotCitas.url_video_conferencia = "";
                    regNotCitas.url_sala_espera = "";
                    regNotCitas.url_pago_online = "";
                    regNotCitas.categoria_paciente = "";

                    listNotificacionCitas.Add(regNotCitas);
                }

                dr.Close();
                cerrarconexion();

            }
            catch (Exception ex)
            {
                //Console.WriteLine("Hay error al acceder a la base de datos(ConsultaNotificacionCitas). " + ex.Message);
                //Console.ReadLine();
                log.WriteEntry("Hay error al acceder a la base de datos Oracle(ConsultaNotificacionCitas): " + ex.Message, EventLogEntryType.Error);
                ConexionSQL con = new ConexionSQL();
                var dataSW = new SWLOG();
                dataSW.fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                dataSW.Proceso = "ActDataEniax";
                dataSW.GloError = "Hay error al acceder a la base de datos Oracle(ConsultaNotificacionCitas): " + ex.Message;
                con.IngSWLOG(dataSW, log);
                //throw ex;
            }
            return listNotificacionCitas;
        }

        public List<CambioEstCita> ConsultaConfirmadas(EventLog log)
        {
            List<CambioEstCita> listRegConfirmadas = new List<CambioEstCita>();

            try
            {
                string queryString = "select ";
                queryString += "r.correl_reserva   id_cita ";
                queryString += ", '1' Estado";
                queryString += ", to_Char( fecconf_reserva ,'yyyy-mm-dd') Fecha";
                //queryString += ", fecconf_reserva Fecha";
                queryString += ", resconf_reserva responsable";
                queryString += ", case r.resconf_reserva";
                queryString += "  when '77777001-2' then 'WHATSAPP'";
                queryString += "  when '77777002-0' then 'CORREO'";
                queryString += "  when '77777003-9' then 'SMS'";
                queryString += "  when '76942590-K' then 'TRUSTCORP'";
                queryString += "  else";
                queryString += "  decode ( nvl( r.resconf_reserva,'X') ,'X','',  decode ( (select count(*)  from acc_usuarios_r_grupos where cod_empresa=8 and  cod_grupo=1 and id_usuario=r.resconf_reserva),0,   decode (   (select count(*)  from acc_usuarios_r_grupos where cod_empresa=8 and   cod_grupo in (172,163)   and id_usuario=r.resconf_reserva)  , 0,   decode (  (select count(*)  from acc_usuarios_r_grupos where cod_empresa=8 and  (cod_grupo<> 172 and cod_grupo<>163  and cod_grupo<>1) and id_usuario=r.resconf_reserva ) , 0,'NN','PRESENCIAL'), 'TRUSTCORP' )   ,'CALL_CENTER')) ";
                queryString += "  end  Canal_Estado";
                queryString += " from ";
                queryString += "  am_Reserva r ";
                queryString += ", tab_paciente tp ";
                queryString += " where r.cod_empresa=8 ";
                queryString += " and r.cod_sucursal>=1 ";
                queryString += " and r.cod_unidad>=0 ";
                queryString += " and tp.cod_empresa=r.cod_empresa ";
                queryString += " and tp.id_ambulatorio=r.id_ambulatorio ";

                //queryString += " and to_date(fecconf_reserva,'dd/mm/yyyy') BETWEEN TO_DATE(sysdate - " + var_min + "/1440, 'dd/mm/yyyy') AND TO_DATE(sysdate, 'dd/mm/yyyy') ";
                //queryString += " and to_char(fecconf_reserva,'hh24:mi') >= to_char(sysdate - " + var_min + "/1440, 'hh24:mi') ";
                queryString += " and fecconf_reserva >=to_date('" + var_FechaRango + "','dd/mm/yyyy hh24:mi') ";
                queryString += " and fecconf_reserva < to_date('" + var_FechaActual + "','dd/mm/yyyy hh24:mi') ";
                //queryString += " and fecconf_reserva >=to_date('06/07/2022 15:00','dd/mm/yyyy hh24:mi') ";
                //queryString += " and fecconf_reserva < to_date('07/07/2022 16:50','dd/mm/yyyy hh24:mi') ";

                queryString += " and confhora_reserva='S' ";
                queryString += " and (r.modulo_reserva<>'KIN2I') ";

                queryString += " and tp.id_ambulatorio not in ";
                queryString += " ( ";
                queryString += " select ";
                queryString += " id_ambulatorio ";
                queryString += " from ";
                queryString += " tab_paciente p ";
                queryString += " where ";
                queryString += " p.cod_empresa=8 ";
                queryString += " and p.vigencia='S' ";
                queryString += " and  ( ";
                queryString += " p.apepat_paciente like '%NODAR%' ";
                queryString += " or p.apepat_paciente like '%NO DAR%' ";
                queryString += " or p.apepat_paciente like '%AGENDAR%' ";
                queryString += " or p.apepat_paciente like '%RESERV%' ";
                queryString += " or p.apepat_paciente like '%HORARIO%' ";
                queryString += " or p.apepat_paciente like '%MEDICO%' ";
                queryString += " or p.apepat_paciente like '%NINGUNA%' ";
                queryString += " or p.apepat_paciente like '%NO VIENE%' ";
                queryString += " or p.apepat_paciente like '%BLOQUE%' ";
                queryString += " or p.apepat_paciente like '%PACIENTE%' ";
                queryString += " or p.apepat_paciente like '%HOSPITA%' ";
                queryString += " or p.apepat_paciente like '%CONTROL%' ";
                queryString += " or p.apepat_paciente like '%HORA%' ";
                queryString += " or p.apepat_paciente like '%INTERCONS%' ";
                queryString += " or p.apepat_paciente like '%ANULA%' ";
                queryString += " or p.apepat_paciente like '%NO BORRAR%' ";
                queryString += " or p.apepat_paciente like '%NO ANOTAR%' ";
                queryString += " or p.apepat_paciente like '%NO ASISTE%' ";
                queryString += " or p.apepat_paciente like '%RES NO%' ";
                queryString += " or p.apepat_paciente like '%NO CAMBIAR%' ";
                queryString += " or p.apepat_paciente like '%REPETI%' ";
                queryString += " or p.apepat_paciente like '%RESER%' ";
                queryString += " or p.apepat_paciente like '% RESPIRATORIO%' ";
                queryString += " or p.apepat_paciente like '%NO CITAR%' ";
                queryString += " or p.apepat_paciente like '%NO CAMBIAR%' ";
                queryString += " or p.apepat_paciente like '%RESER%' ";
                queryString += " or p.apepat_paciente like '%NO ESCRIBIR%' ";
                queryString += " ) ";
                queryString += " ) ";

                OracleCommand cmd = new OracleCommand(queryString, var_conexion);
                abrirconexion();

                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    CambioEstCita regConfirmadas = new CambioEstCita();
                    regConfirmadas.id_cita = dr["id_cita"].ToString();
                    regConfirmadas.estado = dr["Estado"].ToString();
                    regConfirmadas.fecha = dr["Fecha"].ToString();
                    regConfirmadas.responsable = dr["responsable"].ToString();
                    regConfirmadas.canal_estado = dr["Canal_Estado"].ToString();
                    regConfirmadas.motivo = "";
                    regConfirmadas.informacion_adicional = "";

                    listRegConfirmadas.Add(regConfirmadas);
                }

                dr.Close();
                cerrarconexion();

            }
            catch (Exception ex)
            {
                //Console.WriteLine("Hay error al acceder a la base de datos(ConsultaConfirmadas). " + ex.Message);
                //Console.ReadLine();
                log.WriteEntry("Hay error al acceder a la base de datos Oracle(ConsultaConfirmadas): " + ex.Message, EventLogEntryType.Error);
                ConexionSQL con = new ConexionSQL();
                var dataSW = new SWLOG();
                dataSW.fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                dataSW.Proceso = "ActDataEniax";
                dataSW.GloError = "Hay error al acceder a la base de datos Oracle(ConsultaConfirmadas): " + ex.Message;
                con.IngSWLOG(dataSW, log);
                throw;
            }
            return listRegConfirmadas;
        }

        public List<CambioEstCita> ConsultaAnuBloq(EventLog log)
        {
            List<CambioEstCita> listRegAnuBloq = new List<CambioEstCita>();
            //CambioEstCita regAnuBloq = new CambioEstCita();


            try
            {
                string queryString = "select ";
                queryString += "r.correl_reserva   id_cita ";
                queryString += ", case    antr_reserva";
                queryString += "  when 'A' then 2";
                queryString += "  when 'M' then 3";
                queryString += "  else ";
                queryString += "      3";
                queryString += "  end estado ";
                queryString += ", to_Char( fecantr_reserva ,'yyyy-mm-dd') Fecha";
                //queryString += ", fecantr_reserva Fecha";
                queryString += ", respat_reserva  responsable";
                queryString += ", respat_reserva Canal_Estado";
                queryString += ", 'Prueba de Cambio Estado' motivo";
                queryString += ", '' informacion_adicional ";
                queryString += "  from ";
                queryString += "     am_Reserva r ";
                queryString += ",    tab_paciente tp ";
                queryString += " where r.cod_empresa=8 ";
                queryString += " and r.cod_sucursal>=1 ";
                queryString += " and r.cod_unidad>=0 ";
                queryString += " and tp.cod_empresa=r.cod_empresa ";
                queryString += " and tp.id_ambulatorio=r.id_ambulatorio ";

                //queryString += " and to_date(fecantr_reserva,'dd/mm/yyyy') BETWEEN TO_DATE(sysdate - " + var_min + "/1440, 'dd/mm/yyyy') AND TO_DATE(sysdate, 'dd/mm/yyyy') ";
                //queryString += " and to_char(fecantr_reserva,'hh24:mi') >= to_char(sysdate - " + var_min + "/1440, 'hh24:mi') ";
                queryString += " and fecantr_reserva >= to_date('" + var_FechaRango + "','dd/mm/yyyy hh24:mi') ";
                queryString += " and fecantr_reserva <  to_date('" + var_FechaActual + "','dd/mm/yyyy hh24:mi') ";
                //queryString += " and fecantr_reserva >= to_date('08/07/2022 11:20','dd/mm/yyyy hh24:mi') ";
                //queryString += " and fecantr_reserva <  to_date('08/07/2022 11:30','dd/mm/yyyy hh24:mi') ";

                queryString += " and antr_reserva is not null ";
                queryString += " and (r.modulo_reserva<>'KIN2I') ";

                queryString += " and tp.id_ambulatorio not in ";
                queryString += " ( ";
                queryString += " select ";
                queryString += " id_ambulatorio ";
                queryString += " from ";
                queryString += " tab_paciente p ";
                queryString += " where ";
                queryString += " p.cod_empresa=8 ";
                queryString += " and p.vigencia='S' ";
                queryString += " and  ( ";
                queryString += " p.apepat_paciente like '%NODAR%' ";
                queryString += " or p.apepat_paciente like '%NO DAR%' ";
                queryString += " or p.apepat_paciente like '%AGENDAR%' ";
                queryString += " or p.apepat_paciente like '%RESERV%' ";
                queryString += " or p.apepat_paciente like '%HORARIO%' ";
                queryString += " or p.apepat_paciente like '%MEDICO%' ";
                queryString += " or p.apepat_paciente like '%NINGUNA%' ";
                queryString += " or p.apepat_paciente like '%NO VIENE%' ";
                queryString += " or p.apepat_paciente like '%BLOQUE%' ";
                queryString += " or p.apepat_paciente like '%PACIENTE%' ";
                queryString += " or p.apepat_paciente like '%HOSPITA%' ";
                queryString += " or p.apepat_paciente like '%CONTROL%' ";
                queryString += " or p.apepat_paciente like '%HORA%' ";
                queryString += " or p.apepat_paciente like '%INTERCONS%' ";
                queryString += " or p.apepat_paciente like '%ANULA%' ";
                queryString += " or p.apepat_paciente like '%NO BORRAR%' ";
                queryString += " or p.apepat_paciente like '%NO ANOTAR%' ";
                queryString += " or p.apepat_paciente like '%NO ASISTE%' ";
                queryString += " or p.apepat_paciente like '%RES NO%' ";
                queryString += " or p.apepat_paciente like '%NO CAMBIAR%' ";
                queryString += " or p.apepat_paciente like '%REPETI%' ";
                queryString += " or p.apepat_paciente like '%RESER%' ";
                queryString += " or p.apepat_paciente like '% RESPIRATORIO%' ";
                queryString += " or p.apepat_paciente like '%NO CITAR%' ";
                queryString += " or p.apepat_paciente like '%NO CAMBIAR%' ";
                queryString += " or p.apepat_paciente like '%RESER%' ";
                queryString += " or p.apepat_paciente like '%NO ESCRIBIR%' ";
                queryString += " ) ";
                queryString += " ) ";

                OracleCommand cmd = new OracleCommand(queryString, var_conexion);
                abrirconexion();

                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    CambioEstCita regAnuBloq = new CambioEstCita();
                    regAnuBloq.id_cita = dr["id_cita"].ToString();
                    regAnuBloq.estado = dr["estado"].ToString();
                    regAnuBloq.fecha = dr["Fecha"].ToString();
                    regAnuBloq.responsable = dr["responsable"].ToString();
                    regAnuBloq.canal_estado = dr["Canal_Estado"].ToString();
                    regAnuBloq.motivo = dr["motivo"].ToString();
                    regAnuBloq.informacion_adicional = dr["informacion_adicional"].ToString();

                    listRegAnuBloq.Add(regAnuBloq);
                }

                dr.Close();
                cerrarconexion();

            }
            catch (Exception ex)
            {
                //Console.WriteLine("Hay error al acceder a la base de datos(ConsultaAnuBloq). " + ex.Message);
                //Console.ReadLine();
                log.WriteEntry("Hay error al acceder a la base de datos Oracle(ConsultaAnuBloq): " + ex.Message, EventLogEntryType.Error);
                ConexionSQL con = new ConexionSQL();
                var dataSW = new SWLOG();
                dataSW.fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                dataSW.Proceso = "ActDataEniax";
                dataSW.GloError = "Hay error al acceder a la base de datos Oracle(ConsultaAnuBloq): " + ex.Message;
                con.IngSWLOG(dataSW, log);
                throw;
            }
            return listRegAnuBloq;
        }
        public List<CambioEstCita> ConsultaPacPag(EventLog log)
        {
            List<CambioEstCita> listRegPacPag = new List<CambioEstCita>();

            try
            {
                string queryString = "select ";
                queryString += "r.correl_reserva   id_cita ";
                queryString += ", 5  estado "; //--- Aclarar con Enix si se deja 5 o 6
                queryString += ", to_char(fecrec_reserva,'yyyy-mm-dd') Fecha";
                //queryString += ", fecrec_reserva Fecha";
                queryString += ", resprec_reserva responsable";
                queryString += ", 'CAJA-CHP' Canal_Estado ";
                queryString += "  from ";
                queryString += "  am_Reserva r ";
                queryString += ", tab_paciente tp ";
                queryString += " where r.cod_empresa=8 ";
                queryString += " and r.cod_sucursal>=1 ";
                queryString += " and r.cod_unidad>=0 ";
                queryString += " and tp.cod_empresa=r.cod_empresa ";
                queryString += " and tp.id_ambulatorio=r.id_ambulatorio ";

                //queryString += " and to_date(fecrec_reserva,'dd/mm/yyyy') BETWEEN TO_DATE(sysdate - " + var_min + "/1440, 'dd/mm/yyyy') AND TO_DATE(sysdate, 'dd/mm/yyyy') ";
                //queryString += " and to_char(fecrec_reserva,'hh24:mi') >= to_char(sysdate - " + var_min + "/1440, 'hh24:mi') ";
                queryString += " and fecrec_reserva>=to_date('" + var_FechaRango + "','dd/mm/yyyy hh24:mi') ";
                queryString += " and fecrec_reserva< to_date('" + var_FechaActual + "','dd/mm/yyyy hh24:mi') ";
                //queryString += " and fecrec_reserva>=to_date('06/07/2022 15:00','dd/mm/yyyy hh24:mi') ";
                //queryString += " and fecrec_reserva<to_date('07/07/2022 16:50','dd/mm/yyyy hh24:mi') ";

                queryString += " and rec_reserva='S' ";
                queryString += " and antr_reserva is null ";
                queryString += " and (r.modulo_reserva<>'KIN2I') ";

                queryString += " and tp.id_ambulatorio not in ";
                queryString += " ( ";
                queryString += " select ";
                queryString += " id_ambulatorio ";
                queryString += " from ";
                queryString += " tab_paciente p ";
                queryString += " where ";
                queryString += " p.cod_empresa=8 ";
                queryString += " and p.vigencia='S' ";
                queryString += " and  ( ";
                queryString += " p.apepat_paciente like '%NODAR%' ";
                queryString += " or p.apepat_paciente like '%NO DAR%' ";
                queryString += " or p.apepat_paciente like '%AGENDAR%' ";
                queryString += " or p.apepat_paciente like '%RESERV%' ";
                queryString += " or p.apepat_paciente like '%HORARIO%' ";
                queryString += " or p.apepat_paciente like '%MEDICO%' ";
                queryString += " or p.apepat_paciente like '%NINGUNA%' ";
                queryString += " or p.apepat_paciente like '%NO VIENE%' ";
                queryString += " or p.apepat_paciente like '%BLOQUE%' ";
                queryString += " or p.apepat_paciente like '%PACIENTE%' ";
                queryString += " or p.apepat_paciente like '%HOSPITA%' ";
                queryString += " or p.apepat_paciente like '%CONTROL%' ";
                queryString += " or p.apepat_paciente like '%HORA%' ";
                queryString += " or p.apepat_paciente like '%INTERCONS%' ";
                queryString += " or p.apepat_paciente like '%ANULA%' ";
                queryString += " or p.apepat_paciente like '%NO BORRAR%' ";
                queryString += " or p.apepat_paciente like '%NO ANOTAR%' ";
                queryString += " or p.apepat_paciente like '%NO ASISTE%' ";
                queryString += " or p.apepat_paciente like '%RES NO%' ";
                queryString += " or p.apepat_paciente like '%NO CAMBIAR%' ";
                queryString += " or p.apepat_paciente like '%REPETI%' ";
                queryString += " or p.apepat_paciente like '%RESER%' ";
                queryString += " or p.apepat_paciente like '% RESPIRATORIO%' ";
                queryString += " or p.apepat_paciente like '%NO CITAR%' ";
                queryString += " or p.apepat_paciente like '%NO CAMBIAR%' ";
                queryString += " or p.apepat_paciente like '%RESER%' ";
                queryString += " or p.apepat_paciente like '%NO ESCRIBIR%' ";
                queryString += " ) ";
                queryString += " ) ";

                OracleCommand cmd = new OracleCommand(queryString, var_conexion);
                abrirconexion();

                OracleDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    CambioEstCita regPacPag = new CambioEstCita();
                    regPacPag.id_cita = dr["id_cita"].ToString();
                    regPacPag.estado = dr["estado"].ToString();
                    regPacPag.fecha = dr["Fecha"].ToString();
                    regPacPag.responsable = dr["responsable"].ToString();
                    regPacPag.canal_estado = dr["Canal_Estado"].ToString();
                    regPacPag.motivo = "";
                    regPacPag.informacion_adicional = "";

                    listRegPacPag.Add(regPacPag);
                }

                dr.Close();
                cerrarconexion();

            }
            catch (Exception ex)
            {
                //Console.WriteLine("Hay error al acceder a la base de datos(ConsultaPacPag). " + ex.Message);
                //Console.ReadLine();
                log.WriteEntry("Hay error al acceder a la base de datos Oracle(ConsultaPacPag): " + ex.Message, EventLogEntryType.Error);
                ConexionSQL con = new ConexionSQL();
                var dataSW = new SWLOG();
                dataSW.fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                dataSW.Proceso = "ActDataEniax";
                dataSW.GloError = "Hay error al acceder a la base de datos Oracle(ConsultaPacPag): " + ex.Message;
                con.IngSWLOG(dataSW, log);
                throw;
            }
            return listRegPacPag;
        }

    }
}
