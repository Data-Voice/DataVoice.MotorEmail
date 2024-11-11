using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVoice.MotorEmail.Entidad
{
    public class cdMySqlConnection
    {
        public static MySqlConnection GetConneccionMysql()
        {
            MySqlConnection conexion = new MySqlConnection(ConfigurationManager.AppSettings["DVEmpresas"].ToString());
            conexion.Open();
            return conexion;
        }

        public async static Task<MySqlConnection> GetConneccionMysqlAsync()
        {
            MySqlConnection conexion = new MySqlConnection(ConfigurationManager.AppSettings["DVEmpresas"].ToString());
            await conexion.OpenAsync();
            return conexion;
        }
    }
}
