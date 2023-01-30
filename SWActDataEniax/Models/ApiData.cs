using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWActDataEniax.Models
{
    class ApiData
    {
        public CambioEstCita cambioEstadoCita { get; set; }
    }
    public class CambioEstCita
    {
        public string id_cita { get; set; }
        public string estado { get; set; }
        public string fecha { get; set; }
        public string responsable { get; set; }
        public string canal_estado { get; set; }
        public string motivo { get; set; }
        public string informacion_adicional { get; set; }
    }
    public class RespApi
    {
        public string id_transaccion { get; set; }
        public string message { get; set; }
        public string status { get; set; }
    }
    public class CitaProcesada
    {
        public string id_cita { get; set; }
        public string accion { get; set; }
        public string estado { get; set; }
        public string fecha_envio { get; set; }
        public string fecha_cita { get; set; }
        public string id_transaction { get; set; }
    }
    public class LOGApi
    {
        public int codError { get; set; }
        public string gloError { get; set; }
        public int logEjecProcID { get; set; }
        public string logid_cita { get; set; }
        public string fecha { get; set; }
        public string fechaIni { get; set; }
        public string fechaFin { get; set; }
        public string tipoMetodo { get; set; }
        public string urlMetodo { get; set; }
        public string body { get; set; }
    }
    public class NotificacionCita
    {
        public string id_cita { get; set; }
        public string fecha_cita { get; set; }
        public string hora_cita { get; set; }
        public string tipo_cita { get; set; }
        public string duracion_cita { get; set; }
        public string fecha_agendamiento { get; set; }
        public string hora_agendamiento { get; set; }
        public string canal_agendamiento { get; set; }
        public string responsable_agendamiento { get; set; }
        public string es_sobrecupo { get; set; }
        public string es_reagendamiento { get; set; }
        public string es_paquete { get; set; }
        public string id_paquete_padre { get; set; }
        public string instrucciones_previas { get; set; }
        public string estado { get; set; }
        public string identificacion_paciente { get; set; }
        public string tipo_identificacion { get; set; }
        public string cod_paciente { get; set; }
        public string nombre_paciente { get; set; }
        public string primer_apellido_paciente { get; set; }
        public string segundo_apellido_paciente { get; set; }
        public string sexo_paciente { get; set; }
        public string fecha_nacimiento_paciente { get; set; }
        public string direccion_paciente { get; set; }
        public string prevision_paciente { get; set; }
        public string correo_electronico_paciente { get; set; }
        public string nro_telefono_movil_paciente { get; set; }
        public string nro_telefono_fijo_paciente { get; set; }
        public string cod_centro_atencion { get; set; }
        public string nombre_centro_atencion { get; set; }
        public string lugar_atencion { get; set; }
        public string piso_atencion { get; set; }
        public string box_atencion { get; set; }
        public string cod_especialidad { get; set; }
        public string nombre_especialidad { get; set; }
        public string identificacion_profesional { get; set; }
        public string tipo_identificacion_profesional { get; set; }
        public string cod_profesional { get; set; }
        public string nombre_profesional { get; set; }
        public string primer_apellido_profesional { get; set; }
        public string segundo_apellido_profesional { get; set; }
        public string sexo_profesional { get; set; }
        public string prefijo_doctor { get; set; }
        public string cod_prestacion { get; set; }
        public string nombre_prestacion { get; set; }
        public string url_video_conferencia { get; set; }
        public string url_sala_espera { get; set; }
        public string url_pago_online { get; set; }
        public string categoria_paciente { get; set; }
    }
    public class SalParametros
    {
        public string horaEjecucion { get; set; }
        public string minFrecuencia { get; set; }
        public string horaInicio { get; set; }
        public string horaTermino { get; set; }
        public string conexionOracle { get; set; }
        public string urlNotificacionCitas { get; set; }
        public string urlCambioEstado { get; set; }
        public string xUserEniax { get; set; }
        public string xPasswordEniax { get; set; }
        public string xAuthorizationToken { get; set; }

    }
    public class LOGEjecProceso
    {
        public string logEjecProcNom { get; set; }
        public string logEjecFecIni { get; set; }
        public string logEjecFecFin { get; set; }

    }
    public class ActLOGEjecProceso
    {
        public int logEjecProcID { get; set; }
        public string logEjecProcNom { get; set; }
        public string logEjecFecIni { get; set; }
        public string logEjecFecFin { get; set; }

    }
    public class SWLOG
    {
        public string fecha { get; set; }
        public string Proceso { get; set; }
        public string GloError { get; set; }

    }
}
