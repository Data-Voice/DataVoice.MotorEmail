using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVoice.MotorEmail.Models
{
    public class Email
    {
        public Email()
        {
            ArchivosAdjuntos = new List<ArchivoAdjunto>();
        }

        public decimal ID { get; set; }

        public int Indice { get; set; }

        public DateTime FechaCorreo { get; set; }

        public string Idmail { get; set; }

        public string Titulo { get; set; }

        public string NombreDe { get; set; }

        public string De { get; set; }

        public DateTime FechaAlta { get; set; }

        public string Mensaje { get; set; }

        public string Estatus { get; set; }

        public int Asignado { get; set; }

        public int ClaveEstatus { get; set; }

        public string Para { get; set; }

        public string EmailPara { get; set; }

        public string ArchivoAdjunto { get; set; }

        public string ClaveGrupo { get; set; }

        public string ArchivoEmailHtml { get; set; }

        public string ArchivoEmailtxt { get; set; }

        public string Cuenta { get; set; }

        public DateTime FechaRespuesta { get; set; }

        public string Login { get; set; }

        public string NombreLogin { get; set; }

        public string Folio { get; set; }

        public string MensajeRespuesta { get; set; }

        public string ConCopia { get; set; }

        public List<ArchivoAdjunto> ArchivosAdjuntos { get; set; }

    }
}

