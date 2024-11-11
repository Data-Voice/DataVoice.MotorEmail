using DataVoice.MotorEmail.Entidad;
using DataVoice.MotorEmail.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVoice.MotorEmail.Negocio
{
    public class NegocioAgente
    {
        public static List<CuentaEmail> ObtenerCuentasEmailUsuario(string usuario)
        {
            ///////Cambio Puertos
            //return cbQryEmail.ObtenerCuentasEmailUsuario(3,usuario);
            return cbQryEmail.ObtenerCuentasEmailUsuario(6, usuario);
        }

        public async static Task<string> ObtenerGrupoPorSubjectAsync(string cadenaSubject, string cuentaEmail)
        {
            return await cbQryEmail.ObtenerGrupoPorSubjectAsync(cadenaSubject, cuentaEmail);
        }

        public async static Task<List<CuentaEmail>> ObtenerCuentasEmailUsuarioAsync(string usuario)
        {
            ///////Cambio Puertos
            //return cbQryEmail.ObtenerCuentasEmailUsuario(3,usuario);
            return await cbQryEmail.ObtenerCuentasEmailUsuarioAsync(6, usuario);
        }

        public async static Task<DataTable> ObtenerRegistrosMaiMatAsync(int vista, string cuentaEmail)
        {
            return await cbQryEmail.ObtenerMailMatAsync(vista, cuentaEmail);
        }


        public async static Task<string> ObtenerGrupoSubjectDefaultAsync(string cuentaEmail)
        {
            return await cbQryEmail.ObtenerGrupoSubjectDefaultAsync(cuentaEmail);
        }

        public async static Task<bool> ExisteEmailAsync(string cuenta, string idMail)
        {
            return await cbQryEmail.ExisteEmailAsync(cuenta, idMail);
        }

        public async static Task InsertarEmailAsync(Email nuevoEmail)
        {
            await cbQryEmail.InsertarEmailAsync(nuevoEmail);
        }

        public async static Task<bool> ActualizarTokenEmailAsync(CuentasEmail cuentasEmail, int vista)
        {
            return await cbQryEmail.ActualizarTokenEmailAsync(cuentasEmail, vista);
        }

        public async static Task<List<CuentasEmail>> ObtenerCuentasEmailAsync(string usuario)
        {
            ///////Cambio Puertos
            //return cbQryEmail.ObtenerCuentasEmailUsuario(3,usuario);
            return await cbQryEmail.ObtenerCuentasEmailAsync(6, usuario);
        }
    }
}
