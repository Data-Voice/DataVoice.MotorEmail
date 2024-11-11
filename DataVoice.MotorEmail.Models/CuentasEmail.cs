using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVoice.MotorEmail.Models
{
    public class CuentasEmail : CuentaEmail
    {
        public string Server { get; set; }

        public string UserName { get; set; }
        private string _password;
        public bool UseSSL { get; set; }
        public bool KeepMessagesOnServer { get; set; }
        public string PortNumber { get; set; }
        public string SendingServer { get; set; }
        public string Grupo { get; set; }
        public string ClientId { get; set; }
        public string TenantId { get; set; }
        public string SecretCertificate { get; set; }
        public bool Oauth2 { get; set; }
        public string ClientSecret { get; set; }
        public bool Pop3 { get; set; }
        public string AccessToken { get; set; }
        public DateTime FechaToken { get; set; }

    }
}
