using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVoice.MotorEmail.Models
{
    public  class CuentaEmail
    {
        public int Id { get; set; }

        public string Cuenta { get; set; }

        public string Servidor { get; set; }

        public string Usuario { get; set; }

        public string Password { get; set; }

        public string Departamento { get; set; }

        public string ClaveGrupo { get; set; }

        public int TiempoMaximoVerde { get; set; }

        public int TiempoMaximoAmarillo { get; set; }

        public int UltimoIndice { get; set; }

        public string ServidorSalida { get; set; }

        public string Subjet { get; set; }

        public string ActivoSubjet { get; set; }

        public string PuertoEntrada { get; set; }
        public string CrifadoEntrada { get; set; }
        public string PuertoSalida { get; set; }
        public string CifradoSalida { get; set; }
    }
}
