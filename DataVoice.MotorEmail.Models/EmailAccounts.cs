using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataVoice.MotorEmail.Models
{
    public class EmailAccounts
    {
        private string _server;

        public string Server
        {
            get { return _server; }
            set { _server = value; }
        }
        /// <summary>
        /// The username of the mail account
        /// </summary>
        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }
        /// <summary>
        /// The password of the mail account
        /// </summary>
        private string _password;

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        /// <summary>
        /// Does the mail server require SSL
        /// </summary>
        private bool _useSSL;

        public bool UseSSL
        {
            get { return _useSSL; }
            set { _useSSL = value; }
        }
        /// <summary>
        /// Does the user wants to keep a copy of the emails retrieved on the server
        /// </summary>
        private bool _keepMessagesOnServer;

        public bool KeepMessagesOnServer
        {
            get { return _keepMessagesOnServer; }
            set { _keepMessagesOnServer = value; }
        }



        /// <summary>
        /// The Mail Server Port Number
        /// </summary>
        private string _portNumber;

        public string PortNumber
        {
            get { return _portNumber; }
            set { _portNumber = value; }
        }
        /// <summary>
        /// The Sending Server Domain Reference(i.e: smtp.yourdomain.com)
        /// </summary>
        private string _sendingServer;

        public string SendingServer
        {
            get { return _sendingServer; }
            set { _sendingServer = value; }
        }

        /// <summary>
        /// The Sending Server Domain Reference(i.e: smtp.yourdomain.com)
        /// </summary>
        private string _grupo;

        public string Grupo
        {
            get { return _grupo; }
            set { _grupo = value; }
        }

        /// <summary>
        /// The Sending Server Domain Reference(i.e: smtp.yourdomain.com)
        /// </summary>
        private int _ultimoIndice;

        public int UltimoIndice
        {
            get { return _ultimoIndice; }
            set { _ultimoIndice = value; }
        }


        public static EmailAccounts GetAccount(string username, List<EmailAccounts> list)
        {
            foreach (EmailAccounts account in list)
            {
                if (account.UserName == username)
                    return account;
            }

            return null;
        }

        public string Subjet { get; set; }

        public string ActivoSubjet { get; set; }
        public string PuertoEntrada { get; set; }
        public string CrifadoEntrada { get; set; }
        public string PuertoSalida { get; set; }
        public string CifradoSalida { get; set; }
    }
}
