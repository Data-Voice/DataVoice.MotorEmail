using DataVoice.MotorEmail.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVoice.MotorEmail.Entidad
{
    public class cbQryEmail
    {
        public static List<CuentaEmail> ObtenerCuentasEmailUsuario(int vista, string usuario = "")
        {
            MySqlConnection conexion = cdMySqlConnection.GetConneccionMysql();
            try
            {
                MySqlDataReader rd;
                MySqlCommand cmdInsert = new MySqlCommand("SpConCuentasEmail", conexion);
                cmdInsert.CommandType = System.Data.CommandType.StoredProcedure;
                cmdInsert.Parameters.AddWithValue("p_pvClaveGrupo", "");
                cmdInsert.Parameters.AddWithValue("p_pvCuenta", "");
                cmdInsert.Parameters.AddWithValue("p_pvClaveUsuario", usuario);
                cmdInsert.Parameters.AddWithValue("p_piVista", vista);
                rd = cmdInsert.ExecuteReader();
                List<CuentaEmail> listaCuentasEmail = new List<CuentaEmail>();
                while (rd.Read())
                {
                    CuentaEmail email = new CuentaEmail();
                    email.Id = int.Parse(rd["id"].ToString());
                    email.Cuenta = rd["cuenta"].ToString();
                    email.Servidor = rd["servidor"].ToString();
                    email.Usuario = rd["usuario"].ToString();
                    email.Password = rd["password"].ToString();
                    email.Departamento = rd["departamento"].ToString();
                    email.ClaveGrupo = rd["ClaveGrupo"].ToString();
                    email.TiempoMaximoVerde = int.Parse(rd["TiempoMaximoVerde"].ToString());
                    email.TiempoMaximoAmarillo = int.Parse(rd["TiempoMaximoAmarillo"].ToString());
                    email.UltimoIndice = rd["UltimoIndice"].ToString().Length > 0 ? int.Parse(rd["UltimoIndice"].ToString()) : 0;
                    email.ActivoSubjet = rd["activoSubject"].ToString();

                    email.PuertoEntrada = rd["puertoEntrada"].ToString();
                    email.CrifadoEntrada = rd["crifadoEntrada"].ToString();
                    email.PuertoSalida = rd["puertoSalida"].ToString();
                    email.CifradoSalida = rd["cifradoSalida"].ToString();

                    if (email.ActivoSubjet == "1")
                        email.Subjet = rd["subject"].ToString();
                    else
                        email.Subjet = "";
                    listaCuentasEmail.Add(email);
                }
                return listaCuentasEmail;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conexion.Close();
            }
        }

        public async static Task<string> ObtenerGrupoPorSubjectAsync(string cadenaSubject, string cuentaEmail)
        {
            MySqlConnection conexion = await cdMySqlConnection.GetConneccionMysqlAsync();
            try
            {
                string valor = "";
                MySqlCommand cmdInsert = new MySqlCommand("SpConEmail", conexion);
                cmdInsert.CommandType = System.Data.CommandType.StoredProcedure;
                cmdInsert.Parameters.AddWithValue("p_pvClaveUsuario", "");
                cmdInsert.Parameters.AddWithValue("p_piVista", 7);
                cmdInsert.Parameters.AddWithValue("p_pvIdMail", "");
                cmdInsert.Parameters.AddWithValue("p_pvDe", "");
                cmdInsert.Parameters.AddWithValue("p_pvWhere", cadenaSubject);
                cmdInsert.Parameters.AddWithValue("p_piEstatus", 0);
                cmdInsert.Parameters.AddWithValue("p_pvCuentaMail", cuentaEmail);
                cmdInsert.Parameters.AddWithValue("p_pvAsunto", "");
                cmdInsert.Parameters.AddWithValue("p_Usuario", "");
                var rd = await cmdInsert.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    valor = rd["claveGrupo"].ToString();
                }

                return valor;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conexion.Close();
            }
        }

        public async static Task<List<CuentaEmail>> ObtenerCuentasEmailUsuarioAsync(int vista, string usuario = "")
        {
            MySqlConnection conexion = await cdMySqlConnection.GetConneccionMysqlAsync();
            try
            {
                MySqlCommand cmdInsert = new MySqlCommand("SpConCuentasEmail", conexion);
                cmdInsert.CommandType = System.Data.CommandType.StoredProcedure;
                cmdInsert.Parameters.AddWithValue("p_pvClaveGrupo", "");
                cmdInsert.Parameters.AddWithValue("p_pvCuenta", "");
                cmdInsert.Parameters.AddWithValue("p_pvClaveUsuario", usuario);
                cmdInsert.Parameters.AddWithValue("p_piVista", vista);
                var rd = await cmdInsert.ExecuteReaderAsync();
                List<CuentaEmail> listaCuentasEmail = new List<CuentaEmail>();
                while (await rd.ReadAsync())
                {
                    CuentaEmail email = new CuentaEmail();
                    email.Id = int.Parse(rd["id"].ToString());
                    email.Cuenta = rd["cuenta"].ToString();
                    email.Servidor = rd["servidor"].ToString();
                    email.Usuario = rd["usuario"].ToString();
                    email.Password = rd["password"].ToString();
                    email.Departamento = rd["departamento"].ToString();
                    email.ClaveGrupo = rd["ClaveGrupo"].ToString();
                    email.TiempoMaximoVerde = int.Parse(rd["TiempoMaximoVerde"].ToString());
                    email.TiempoMaximoAmarillo = int.Parse(rd["TiempoMaximoAmarillo"].ToString());
                    email.UltimoIndice = rd["UltimoIndice"].ToString().Length > 0 ? int.Parse(rd["UltimoIndice"].ToString()) : 0;
                    email.ActivoSubjet = rd["activoSubject"].ToString();

                    email.PuertoEntrada = rd["puertoEntrada"].ToString();
                    email.CrifadoEntrada = rd["crifadoEntrada"].ToString();
                    email.PuertoSalida = rd["puertoSalida"].ToString();
                    email.CifradoSalida = rd["cifradoSalida"].ToString();

                    if (email.ActivoSubjet == "1")
                        email.Subjet = rd["subject"].ToString();
                    else
                        email.Subjet = "";
                    listaCuentasEmail.Add(email);
                }
                return listaCuentasEmail;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conexion.Close();
            }
        }

        public async static Task<DataTable> ObtenerMailMatAsync(int vista, string cuentaEmail)
        {
            MySqlConnection conexion = await cdMySqlConnection.GetConneccionMysqlAsync();
            try
            {
                DataTable table = new DataTable();
                string cadenaSubject = String.Empty;
                string finalSubject = String.Empty;
                string cadenaCuenta = String.Empty;
                string cadenaClaveGrupo = String.Empty;
                string cadenaActiveSubject = String.Empty;
                MySqlCommand cmdCon = new MySqlCommand("SpConEmail", conexion);
                cmdCon.CommandType = System.Data.CommandType.StoredProcedure;
                cmdCon.Parameters.AddWithValue("p_pvClaveUsuario", "");
                cmdCon.Parameters.AddWithValue("p_piVista", 9);
                cmdCon.Parameters.AddWithValue("p_pvIdMail", 0);
                cmdCon.Parameters.AddWithValue("p_pvDe", "");
                cmdCon.Parameters.AddWithValue("p_pvWhere", "");
                cmdCon.Parameters.AddWithValue("p_piEstatus", 0);
                cmdCon.Parameters.AddWithValue("p_pvCuentaMail", cuentaEmail);
                cmdCon.Parameters.AddWithValue("p_pvAsunto", "");
                cmdCon.Parameters.AddWithValue("p_Usuario", "");

                var rd = await cmdCon.ExecuteReaderAsync();
                table.Columns.Add("cuenta");
                table.Columns.Add("ClaveGrupo");
                table.Columns.Add("subject");
                table.Columns.Add("activoSubject");
                while (await rd.ReadAsync())
                {
                    cadenaCuenta = rd["cuenta"].ToString();
                    cadenaActiveSubject = rd["activoSubject"].ToString();
                    cadenaClaveGrupo = rd["ClaveGrupo"].ToString();
                    cadenaSubject = rd["subject"].ToString();
                    string[] resultCadena = cadenaSubject.Split(new string[] { "|" }, StringSplitOptions.None);
                    foreach (string resultCadenaSplit in resultCadena)
                    {
                        table.Rows.Add(cadenaCuenta, cadenaClaveGrupo, resultCadenaSplit, cadenaActiveSubject);
                    }
                }
                table.Load(rd);
                return table;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conexion.Close();
            }
        }

        public async static Task<string> ObtenerGrupoSubjectDefaultAsync(string cuentaEmail)
        {
            MySqlConnection conexion = await cdMySqlConnection.GetConneccionMysqlAsync();
            try
            {
                string valor = "";
                MySqlCommand cmdInsert = new MySqlCommand("SpConEmail", conexion);
                cmdInsert.CommandType = System.Data.CommandType.StoredProcedure;
                cmdInsert.Parameters.AddWithValue("p_pvClaveUsuario", "");
                cmdInsert.Parameters.AddWithValue("p_piVista", 8);
                cmdInsert.Parameters.AddWithValue("p_pvIdMail", "");
                cmdInsert.Parameters.AddWithValue("p_pvDe", "");
                cmdInsert.Parameters.AddWithValue("p_pvWhere", "2");
                cmdInsert.Parameters.AddWithValue("p_piEstatus", 0);
                cmdInsert.Parameters.AddWithValue("p_pvCuentaMail", cuentaEmail);
                cmdInsert.Parameters.AddWithValue("p_pvAsunto", "");
                cmdInsert.Parameters.AddWithValue("p_Usuario", "");

                var rd = await cmdInsert.ExecuteReaderAsync();
                if (await rd.ReadAsync())
                {
                    valor = rd["claveGrupo"].ToString();
                }

                return valor;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conexion.Close();
            }
        }

        public async static Task<bool> ExisteEmailAsync(string cuenta, string idMail)
        {
            MySqlConnection conexion = await cdMySqlConnection.GetConneccionMysqlAsync();
            try
            {
                MySqlCommand cmdInsert = new MySqlCommand("SpConEmailUnico", conexion);
                cmdInsert.CommandType = System.Data.CommandType.StoredProcedure;
                cmdInsert.Parameters.AddWithValue("p_pvCuentaEmail", cuenta);
                cmdInsert.Parameters.AddWithValue("p_pvIdMail", idMail);
                var rd = await cmdInsert.ExecuteReaderAsync();
                return rd.HasRows;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conexion.Close();
            }
        }

        public async static Task InsertarEmailAsync(Email nuevoEmail)
        {
            MySqlConnection conexion = await cdMySqlConnection.GetConneccionMysqlAsync();
            try
            {
                MySqlCommand cmdInsert = new MySqlCommand("SpInsEmail", conexion);
                cmdInsert.CommandType = System.Data.CommandType.StoredProcedure;
                cmdInsert.Parameters.AddWithValue("p_fecha_correo", nuevoEmail.FechaCorreo);
                cmdInsert.Parameters.AddWithValue("p_Idmail", nuevoEmail.Idmail);
                cmdInsert.Parameters.AddWithValue("p_ultimoIndice", nuevoEmail.Indice);
                cmdInsert.Parameters.AddWithValue("p_titulo", nuevoEmail.Titulo);
                cmdInsert.Parameters.AddWithValue("p_nombre_de", nuevoEmail.NombreDe);
                cmdInsert.Parameters.AddWithValue("p_de", nuevoEmail.De);
                cmdInsert.Parameters.AddWithValue("p_mensaje", nuevoEmail.Mensaje);
                cmdInsert.Parameters.AddWithValue("p_estatus", nuevoEmail.Estatus);
                cmdInsert.Parameters.AddWithValue("p_asignado", nuevoEmail.Asignado);
                cmdInsert.Parameters.AddWithValue("p_clave_estatus", nuevoEmail.ClaveEstatus);
                cmdInsert.Parameters.AddWithValue("p_para", nuevoEmail.Para);
                cmdInsert.Parameters.AddWithValue("p_email_para", nuevoEmail.EmailPara);
                cmdInsert.Parameters.AddWithValue("p_archivo_adjunto", nuevoEmail.ArchivoAdjunto);
                cmdInsert.Parameters.AddWithValue("p_clavegrupo", nuevoEmail.ClaveGrupo);
                cmdInsert.Parameters.AddWithValue("p_archivo_emailhtml", nuevoEmail.ArchivoEmailHtml);
                cmdInsert.Parameters.AddWithValue("p_archivo_emailtxt", nuevoEmail.ArchivoEmailtxt);
                cmdInsert.Parameters.AddWithValue("p_cuentaEmail", nuevoEmail.Cuenta);
                cmdInsert.Parameters.AddWithValue("p_email_CC", nuevoEmail.ConCopia);
                await cmdInsert.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conexion.Close();
            }
        }

        public async static Task<bool> ActualizarTokenEmailAsync(CuentasEmail Email, int vista)
        {
            MySqlConnection conexion = await cdMySqlConnection.GetConneccionMysqlAsync();
            bool response = true;

            try
            {
                MySqlCommand cmdInsert = new MySqlCommand("SpUpdEmailToken", conexion);
                cmdInsert.CommandType = System.Data.CommandType.StoredProcedure;
                cmdInsert.Parameters.AddWithValue("pvista", vista);
                cmdInsert.Parameters.AddWithValue("p_cuentaEmail", Email.UserName);
                cmdInsert.Parameters.AddWithValue("p_Grupo", Email.ClaveGrupo);
                cmdInsert.Parameters.AddWithValue("p_AccesToken", Email.AccessToken);
                await cmdInsert.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                response = false;
                throw ex;
            }
            finally
            {
                conexion.Close();
            }
            return response;
        }

        public async static Task<List<CuentasEmail>> ObtenerCuentasEmailAsync(int vista, string usuario = "")
        {
            MySqlConnection conexion = await cdMySqlConnection.GetConneccionMysqlAsync();
            try
            {
                MySqlCommand cmdInsert = new MySqlCommand("SpConCuentasEmail", conexion);
                cmdInsert.CommandType = System.Data.CommandType.StoredProcedure;
                cmdInsert.Parameters.AddWithValue("p_pvClaveGrupo", "");
                cmdInsert.Parameters.AddWithValue("p_pvCuenta", "");
                cmdInsert.Parameters.AddWithValue("p_pvClaveUsuario", usuario);
                cmdInsert.Parameters.AddWithValue("p_piVista", vista);
                var rd = await cmdInsert.ExecuteReaderAsync();
                List<CuentasEmail> listaCuentasEmail = new List<CuentasEmail>();
                while (await rd.ReadAsync())
                {
                    CuentasEmail email = new CuentasEmail();
                    email.Id = int.Parse(rd["id"].ToString());
                    email.UserName = rd["cuenta"].ToString();
                    email.Servidor = rd["servidor"].ToString();
                    email.UserName = rd["usuario"].ToString();
                    email.Password = rd["password"].ToString();
                    email.Departamento = rd["departamento"].ToString();
                    email.ClaveGrupo = rd["ClaveGrupo"].ToString();
                    email.TiempoMaximoVerde = int.Parse(rd["TiempoMaximoVerde"].ToString());
                    email.TiempoMaximoAmarillo = int.Parse(rd["TiempoMaximoAmarillo"].ToString());
                    email.UltimoIndice = rd["UltimoIndice"].ToString().Length > 0 ? int.Parse(rd["UltimoIndice"].ToString()) : 0;
                    email.ActivoSubjet = rd["activoSubject"].ToString();

                    email.PuertoEntrada = rd["puertoEntrada"].ToString();
                    email.CrifadoEntrada = rd["crifadoEntrada"].ToString();
                    email.PuertoSalida = rd["puertoSalida"].ToString();
                    email.CifradoSalida = rd["cifradoSalida"].ToString();

                    if (email.ActivoSubjet == "1")
                        email.Subjet = rd["subject"].ToString();
                    else
                        email.Subjet = "";

                    email.Oauth2 = Convert.ToBoolean(rd["Oauth2"]);
                    email.ClientId = rd["ClientId"].ToString();
                    email.ClientSecret = rd["ClientSecret"].ToString();
                    email.TenantId = rd["TenantId"].ToString();
                    email.Pop3 = Convert.ToBoolean(rd["Pop3"]);
                    email.AccessToken = rd["AccessToken"].ToString();
                    if (!string.IsNullOrEmpty(rd["FechaToken"].ToString()))
                        email.FechaToken = Convert.ToDateTime(rd["FechaToken"]);
                    else
                        email.FechaToken = DateTime.Now;

                    listaCuentasEmail.Add(email);
                }
                return listaCuentasEmail;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conexion.Close();
            }
        }
    }
}
