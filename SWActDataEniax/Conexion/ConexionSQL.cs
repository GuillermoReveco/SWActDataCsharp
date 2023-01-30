using SWActDataEniax.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWActDataEniax.Conexion
{
    class ConexionSQL
    {
        public string var_cadenaconexion = ConfigurationManager.AppSettings["ConexionSQL"].ToString();

        SqlConnection var_conexion = new SqlConnection();

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

        //public int IngCitaProcesada(CitaProcesada model, EventLog eventLog)
        public int IngCitaProcesada(CitaProcesada model, EventLog log)
        {
            int var_resultado = 0;
            SqlCommand var_comando = new SqlCommand();
            try
            {
                abrirconexion();
                var_comando.CommandText = "sp_Act_IngCitaProcesada";
                var_comando.CommandType = CommandType.StoredProcedure;
                var_comando.Parameters.AddWithValue("@id_cita", model.id_cita);
                var_comando.Parameters.AddWithValue("@accion", model.accion);
                var_comando.Parameters.AddWithValue("@estado", model.estado);
                var_comando.Parameters.AddWithValue("@fecha_envio", Convert.ToDateTime(model.fecha_envio));
                var_comando.Parameters.AddWithValue("@fecha_cita", Convert.ToDateTime(model.fecha_cita));
                var_comando.Parameters.AddWithValue("@id_transaccion", model.id_transaction);

                var_comando.Connection = var_conexion;
                var_resultado = var_comando.ExecuteNonQuery();
                var_comando = null;
                cerrarconexion();
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Hay error al acceder a la base de datos SQLServer(sp_Act_IngCitaProcesada): " + ex.Message);
                //eventLog.WriteEntry("Hay error al acceder a la base de datos SQLServer(sp_Act_IngCitaProcesada): " + ex.Message, EventLogEntryType.Information);
                //Console.ReadLine();
                log.WriteEntry("Hay error al acceder a la base de datos SQLServer(sp_Act_IngCitaProcesada): " + ex.Message, EventLogEntryType.Error);
                throw;
            }
            return var_resultado;
        }
        public int IngLOGApi(LOGApi model, EventLog log)
        {
            int var_resultado = 0;
            SqlCommand var_comando = new SqlCommand();
            try
            {
                abrirconexion();
                var_comando.CommandText = "sp_Act_IngLOGAPI";
                var_comando.CommandType = CommandType.StoredProcedure;
                var_comando.Parameters.AddWithValue("@CodError", model.codError);
                var_comando.Parameters.AddWithValue("@GloError", model.gloError);
                var_comando.Parameters.AddWithValue("@LOGEjecProcID", model.logEjecProcID);//@LOGEjecProcID
                var_comando.Parameters.AddWithValue("@LOGid_cita", model.logid_cita);//@LOGid_cita
                var_comando.Parameters.AddWithValue("@Fecha", Convert.ToDateTime(model.fecha));
                var_comando.Parameters.AddWithValue("@FechaIni", Convert.ToDateTime(model.fechaIni));
                var_comando.Parameters.AddWithValue("@FechaFin", Convert.ToDateTime(model.fechaFin));
                var_comando.Parameters.AddWithValue("@TipoMetodo", model.tipoMetodo);
                var_comando.Parameters.AddWithValue("@UrlMetodo", model.urlMetodo);
                var_comando.Parameters.AddWithValue("@Body", model.body);

                var_comando.Connection = var_conexion;
                var_resultado = var_comando.ExecuteNonQuery();
                var_comando = null;
                cerrarconexion();
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Hay error al acceder a la base de datos SQLServer(sp_Act_IngLOGAPI): " + ex.Message);
                //Console.ReadLine();
                log.WriteEntry("Hay error al acceder a la base de datos SQLServer(sp_Act_IngLOGAPI): " + ex.Message, EventLogEntryType.Error);
                throw;
            }
            return var_resultado;
        }
        public int ActLOGEjecProceso(ActLOGEjecProceso model, EventLog log)
        {
            int var_resultado = 0;
            SqlCommand var_comando = new SqlCommand();
            try
            {
                abrirconexion();
                var_comando.CommandText = "sp_Act_LOGEjecProceso";
                var_comando.CommandType = CommandType.StoredProcedure;
                var_comando.Parameters.AddWithValue("@LOGEjecProcID", model.logEjecProcID);
                var_comando.Parameters.AddWithValue("@LOGEjecProcNom", model.logEjecProcNom);
                var_comando.Parameters.AddWithValue("@LOGEjecFecIni", Convert.ToDateTime(model.logEjecFecIni));
                var_comando.Parameters.AddWithValue("@LOGEjecFecFin", Convert.ToDateTime(model.logEjecFecFin));

                var_comando.Connection = var_conexion;
                var_resultado = var_comando.ExecuteNonQuery();
                var_comando = null;

                cerrarconexion();

            }
            catch (Exception ex)
            {
                //Console.WriteLine("Hay error al acceder a la base de datos SQLServer(sp_Act_IngLOGAPI): " + ex.Message);
                //Console.ReadLine();
                log.WriteEntry("Hay error al acceder a la base de datos SQLServer(sp_Act_LOGEjecProceso): " + ex.Message, EventLogEntryType.Error);
                throw;
            }
            return var_resultado;
        }
        public int ConsultaCita(int Id, string Accion, int Estado, EventLog log)
        {
            int id_cita = 0;
            try
            {
                SqlCommand var_comando = new SqlCommand();
                abrirconexion();
                var_comando.CommandText = "sp_Con_CitaProcesada";
                var_comando.CommandType = CommandType.StoredProcedure;
                var_comando.Parameters.AddWithValue("@id_cita", Id);
                var_comando.Parameters.AddWithValue("@accion", Accion);
                var_comando.Parameters.AddWithValue("@estado", Estado);

                var_comando.Connection = var_conexion;

                SqlDataReader registros = var_comando.ExecuteReader();
                if (registros.Read())
                {
                    id_cita = int.Parse(registros["id_cita"].ToString());
                }

                var_comando = null;
                cerrarconexion();

            }
            catch (Exception ex)
            {
                //Console.WriteLine("Hay error al acceder a la base de datos SQLServer(sp_Con_CitaProcesada): " + ex.Message);
                //Console.ReadLine();
                log.WriteEntry("Hay error al acceder a la base de datos SQLServer(sp_Con_CitaProcesada): " + ex.Message, EventLogEntryType.Error);
                throw;
            }
            return id_cita;
        }
        public SalParametros ConsultaParametros(EventLog log)
        {
            SalParametros regParametros = new SalParametros();
            try
            {
                SqlCommand var_comando = new SqlCommand();
                abrirconexion();
                var_comando.CommandText = "sp_Con_Parametros";
                var_comando.CommandType = CommandType.StoredProcedure;

                var_comando.Connection = var_conexion;

                SqlDataReader registros = var_comando.ExecuteReader();
                if (registros.Read())
                {
                    regParametros.horaEjecucion = registros["HoraEjecucion"].ToString();
                    regParametros.minFrecuencia = registros["MinFrecuencia"].ToString();
                    regParametros.horaInicio = registros["HoraInicio"].ToString();
                    regParametros.horaTermino = registros["HoraTermino"].ToString();
                    regParametros.conexionOracle = registros["ConexionOracle"].ToString();
                    regParametros.urlNotificacionCitas = registros["UrlNotificacionCitas"].ToString();
                    regParametros.urlCambioEstado = registros["UrlCambioEstado"].ToString();
                    regParametros.xUserEniax = registros["XUserEniax"].ToString();
                    regParametros.xPasswordEniax = registros["XPasswordEniax"].ToString();
                    regParametros.xAuthorizationToken = registros["XAuthorizationToken"].ToString();
                }

                var_comando = null;
                cerrarconexion();

            }
            catch (Exception ex)
            {
                //Console.WriteLine("Hay error al acceder a la base de datos SQLServer(sp_Con_Parametros): " + ex.Message);
                //Console.ReadLine();

                log.WriteEntry("Hay error al acceder a la base de datos SQLServer(sp_Con_Parametros): " + ex.Message, EventLogEntryType.Error);
                throw ex;
            }
            return regParametros;
        }
        public int IngLOGEjecProceso(LOGEjecProceso model, EventLog log)
        {
            int var_resultado = 0;
            int id = 0;
            SqlCommand var_comando = new SqlCommand();
            try
            {
                abrirconexion();
                var_comando.CommandText = "sp_Act_IngLOGEjecProceso";
                var_comando.CommandType = CommandType.StoredProcedure;
                var_comando.Parameters.AddWithValue("@LOGEjecProcNom", model.logEjecProcNom);
                var_comando.Parameters.AddWithValue("@LOGEjecFecIni", Convert.ToDateTime(model.logEjecFecIni));
                var_comando.Parameters.AddWithValue("@LOGEjecFecFin", Convert.ToDateTime(model.logEjecFecFin));

                var_comando.Parameters.Add("@LOGEjecProcID", SqlDbType.Int);
                var_comando.Parameters["@LOGEjecProcID"].Direction = ParameterDirection.Output;

                var_comando.Connection = var_conexion;
                var_resultado = var_comando.ExecuteNonQuery();
                id = (int)var_comando.Parameters["@LOGEjecProcID"].Value;
                var_resultado = id;

                var_comando = null;

                cerrarconexion();

            }
            catch (Exception ex)
            {
                //Console.WriteLine("Hay error al acceder a la base de datos SQLServer(sp_Act_IngLOGAPI): " + ex.Message);
                //Console.ReadLine();
                log.WriteEntry("Hay error al acceder a la base de datos SQLServer(sp_Act_IngLOGEjecProceso): " + ex.Message, EventLogEntryType.Error);
                throw;
            }
            return var_resultado;
        }
        //
        public int IngSWLOG(SWLOG model, EventLog log)
        {
            int var_resultado = 0;
            SqlCommand var_comando = new SqlCommand();
            try
            {
                abrirconexion();
                var_comando.CommandText = "sp_Act_IngSWLOG";
                var_comando.CommandType = CommandType.StoredProcedure;
                var_comando.Parameters.AddWithValue("@Fecha", Convert.ToDateTime(model.fecha));
                var_comando.Parameters.AddWithValue("@Proceso", model.Proceso);
                var_comando.Parameters.AddWithValue("@GloError", model.GloError);

                var_comando.Connection = var_conexion;
                var_resultado = var_comando.ExecuteNonQuery();
                var_comando = null;
                cerrarconexion();
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Hay error al acceder a la base de datos SQLServer(sp_Act_IngLOGAPI): " + ex.Message);
                //Console.ReadLine();
                log.WriteEntry("Hay error al acceder a la base de datos SQLServer(IngSWLOG): " + ex.Message, EventLogEntryType.Error);
                throw;
            }
            return var_resultado;
        }
        //
    }
}
