using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Configuration;
using System.Threading;
using System.Text.RegularExpressions;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Identity.Client;
using EAGetMail;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Globalization;
using System.Net.Http;
using System.Security;
using DataVoice.MotorEmail.Models;
using DataVoice.MotorEmail.Negocio;

namespace DataVoice.MotorEmail
{
    public partial class MainForm : Form
    {
        public static List<EmailAccounts> CurrentMailAccounts = new List<EmailAccounts>();
        int _CorreoProcesados;
        int _Temporizador = 60;
        int _NumeroEmail = 1;
        string NameFile = ConfigurationManager.AppSettings["NameFile"].ToString();
        public delegate string AsyncMethodCaller(int callDuration, out int threadId);

        public MainForm()
        {
            this.MaximizeBox = false;
            InitializeComponent();
            string cuentaEmail = "";
            foreach (CuentaEmail cuenta in NegocioAgente.ObtenerCuentasEmailUsuario("G18"))
            {
                if (cuentaEmail != cuenta.Usuario)
                {
                    TreeNode node = new TreeNode(cuenta.Usuario);
                    Cuentas.Nodes.Add(node);
                }
                cuentaEmail = cuenta.Usuario;
                //TreeNode node = new TreeNode(cuenta.Usuario + cuenta.ClaveGrupo);

            }
        }

        private async void _bRetrieveMessageList_Click(object sender, EventArgs e)
        {
            BarraProgreso.Refresh();
            LstCorreos.Items.Clear();
            //if (!BackgroundWorker.IsBusy)
            // BackgroundWorker.RunWorkerAsync();

            await ObtenerCorreos();
            BtnBajarCorreos.Enabled = false;
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            BackgroundWorker.CancelAsync();
        }

        public void AddLogEntry(string entry)
        {
            DateTime d = DateTime.Now;
            StringBuilder sb = new StringBuilder();
            sb.Append(d.Hour.ToString().PadLeft(2, '0'));
            sb.Append(":");
            sb.Append(d.Minute.ToString().PadLeft(2, '0'));
            sb.Append(":");
            sb.Append(d.Second.ToString().PadLeft(2, '0'));
            sb.Append(".");
            sb.Append(d.Millisecond.ToString().PadLeft(3, '0'));
            sb.Append(" | ");
            sb.Append(entry);
            this.Invoke(new MethodInvoker(delegate
            {
                this.LstCorreos.Items.Insert(0, sb.ToString());
            }));
        }

        async Task<string> ObtenerGrupoPorSubject(string cadenaSubject, string cuentaEmail)
        {
            string _Grupo = "";
            _Grupo = await NegocioAgente.ObtenerGrupoPorSubjectAsync(cadenaSubject, cuentaEmail);
            return _Grupo;
        }
        private string RemoverCaracteresEspeciales(string cadena)
        {
            return Regex.Replace(cadena, @"[^\s\w\.@-]", "", RegexOptions.None);
        }

        string CrearArchivosEmail(string path, string nombreArchivo, string contenido)
        {
            var archivo = path + nombreArchivo;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            // eliminar el fichero si ya existe
            if (!File.Exists(archivo))
                using (var fileStream = File.Create(archivo))
                {
                    var texto = new UTF8Encoding(true).GetBytes(contenido);
                    fileStream.Write(texto, 0, texto.Length);
                    fileStream.Flush();
                }
            return archivo;
        }

        #region Metodos Asincronos
        //private async void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    DateTime start = DateTime.Now;
        //    e.Result = "";
        //    Pop3Client pop = new Pop3Client();
        //    pop.NoDelay = true;
        //    try
        //    {
        //        this.Invoke(new MethodInvoker(delegate { LblTemporizador.Text = "Procesando"; }));
        //        this.Invoke(new MethodInvoker(delegate { LblMensaje.Text = ""; }));
        //        Temporizador.Stop();
        //        List<CuentasEmail> cuentas = await NegocioAgente.ObtenerCuentasEmailAsync("G18");

        //        //Connect to the pop3 client
        //        foreach (TreeNode nodo in Cuentas.Nodes)
        //        {
        //            if (nodo.Checked)
        //            {
        //                MailServer oServer;
        //                //EmailAccounts cuenta = CurrentMailAccounts.Find(seleccion => seleccion.UserName == nodo.Text);
        //                CuentasEmail cuenta = cuentas.Where(seleccion => seleccion.UserName == nodo.Text).FirstOrDefault();
        //                _NumeroEmail = cuenta.UltimoIndice;
        //                if (cuenta.Oauth2)
        //                {
        //                    oServer = await RetrieveEmailOauth(cuenta);
        //                }
        //                else
        //                {
        //                    oServer = await RetrieveEmail(cuenta);
        //                }
        //                MailClient oClient = new MailClient("TryIt");
        //                // Get new email only, if you want to get all emails, please remove this line
        //                oClient.GetMailInfosParam.GetMailInfosOptions = GetMailInfosOptionType.NewOnly;

        //                Console.WriteLine("Connecting {0} ...", oServer.Server);
        //                oClient.Connect(oServer);


        //                MailInfo[] emails = oClient.GetMailInfos();
        //                Console.WriteLine("Total {0} email(s)\r\n", emails.Length);
        //                this.Invoke(new MethodInvoker(delegate { TxtNumeroCorreo.Text = emails.Length.ToString(); }));

        //                int total = oClient.GetMailCount();
        //                string path = ConfigurationManager.AppSettings["UbicacionArchivos"];
        //                MessageCollection mc = new MessageCollection();
        //                if (total < _NumeroEmail)
        //                {
        //                    _NumeroEmail = _NumeroEmail - total;
        //                }
        //                for (int numeroCorreo = _NumeroEmail; numeroCorreo < emails.Length; numeroCorreo++)
        //                {
        //                    try
        //                    {
        //                        MailInfo email = emails[numeroCorreo];
        //                        Console.WriteLine("Index: {0}; Size: {1}; UIDL: {2}",
        //                            email.Index, email.Size, email.UIDL);

        //                        // Receive email from email server
        //                        Mail oMail = oClient.GetMail(email);

        //                        Email Newemail = new Email();
        //                        string _GrupoEmail = "";

        //                        DataTable matriz = await NegocioAgente.ObtenerRegistrosMaiMatAsync(9, cuenta.UserName);
        //                        string cadenaMensaje = oMail.Subject.ToLower();

        //                        for (int item = 0; item < matriz.Rows.Count; item++)
        //                        {
        //                            if (matriz.Rows[item]["activoSubject"].ToString() == "2")
        //                            {
        //                                _GrupoEmail = matriz.Rows[item]["claveGrupo"].ToString();
        //                                break;
        //                            }
        //                            else if (cadenaMensaje.Contains(matriz.Rows[item]["subject"].ToString().ToLower()))
        //                            {
        //                                _GrupoEmail = matriz.Rows[item]["claveGrupo"].ToString();
        //                                break;
        //                            }
        //                        }

        //                        if (_GrupoEmail == string.Empty)
        //                        {
        //                            _GrupoEmail = await NegocioAgente.ObtenerGrupoSubjectDefaultAsync(cuenta.UserName);
        //                        }

        //                        switch (cuenta.ActivoSubjet)
        //                        {
        //                            case "0":
        //                                _GrupoEmail = cuenta.Grupo;
        //                                break;
        //                            case "1":
        //                                //case "2":
        //                                string _CadenaArreglo = "";
        //                                string cadena = oMail.Subject;
        //                                string finalSubject = string.Empty;
        //                                string asunto = RemoverCaracteresEspeciales(cadena);

        //                                string[] resultCadena = asunto.Split(new string[] { " " }, StringSplitOptions.None);
        //                                foreach (string resultCadenaSplit in resultCadena)
        //                                {
        //                                    if (resultCadenaSplit != "")
        //                                        _CadenaArreglo += " subject LIKE '%" + resultCadenaSplit + "%' OR";
        //                                }
        //                                if (_CadenaArreglo != "")
        //                                {
        //                                    _CadenaArreglo = _CadenaArreglo.Remove(_CadenaArreglo.Length - 2);
        //                                }

        //                                _GrupoEmail = await ObtenerGrupoPorSubject(_CadenaArreglo, cuenta.UserName);

        //                                if (_GrupoEmail == string.Empty)
        //                                {
        //                                    await saveLOGAsync("Error Admin..", DateTime.Now.ToString("ddMMyyyy"), "Entra a ObtenerGrupoSubjectDefault");

        //                                    _GrupoEmail = await NegocioAgente.ObtenerGrupoSubjectDefaultAsync(cuenta.UserName);
        //                                }
        //                                break;
        //                        }
        //                        Newemail.Indice = numeroCorreo;
        //                        Newemail.Cuenta = cuenta.UserName;
        //                        Newemail.ClaveGrupo = _GrupoEmail;

        //                        //TimeSpan diferencia = DateTime.Now - mensaje.ReceivedDate;
        //                        //email.FechaCorreo = mensaje.ReceivedDate.Add(diferencia);
        //                        Newemail.FechaCorreo = oMail.ReceivedDate.ToLocalTime();
        //                        Newemail.Idmail = oMail.ReceivedDate.ToOADate() + cuenta.UserName;
        //                        //email.ClaveGrupo = cuenta.Grupo;
        //                        Newemail.Titulo = oMail.Subject != null ? oMail.Subject : "";

        //                        Newemail.NombreDe = oMail.From.Name;

        //                        Newemail.De = oMail.From.Address;
        //                        if (Newemail.De != cuenta.UserName || oMail.Cc.Contains(cuenta.UserName))
        //                        {
        //                            Newemail.Estatus = "Nuevo";
        //                            Newemail.Asignado = 0;
        //                            Newemail.ClaveEstatus = 0;
        //                            Newemail.Para = cuenta.UserName;
        //                            //Newemail.EmailPara = oMail.To.FirstOrDefault().Address;
        //                            if (oMail.To != null)
        //                                Newemail.EmailPara = String.Join(",", oMail.To.Select(x => x.Address));
        //                            Newemail.Cuenta = cuenta.UserName;
        //                            // Newemail.ConCopia = oMail.Cc.FirstOrDefault().Address;
        //                            if (oMail.Cc != null)
        //                                Newemail.ConCopia = String.Join(",", oMail.Cc.Select(x => x.Address));
        //                            string idEmailB64 = System.Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(Newemail.Idmail));
        //                            string pathEmail = path + idEmailB64 + @"\";
        //                            Newemail.ArchivoEmailHtml = pathEmail + "Contenido.html";
        //                            Newemail.ArchivoEmailtxt = pathEmail + "Contenido.txt";
        //                            Newemail.ArchivoAdjunto = pathEmail;

        //                            bool correoAgregado = false;
        //                            bool correoExistente = false;
        //                            foreach (MailAddress para in oMail.To)
        //                            {
        //                                if (Newemail.Idmail != null)
        //                                    if (!await ChatDataVoice.Negocio.NegocioAgente.ExisteEmailAsync(para.Address.Trim(), Newemail.Idmail.Trim()))
        //                                    {
        //                                        //EmailAccounts cuentaPara = CurrentMailAccounts.Find(seleccion => seleccion.UserName == para.Address);
        //                                        CuentasEmail cuentaPara = cuentas.Where(seleccion => seleccion.UserName == para.Address).FirstOrDefault();
        //                                        if (cuentaPara != null)
        //                                        {
        //                                            correoAgregado = true;
        //                                            Newemail.Cuenta = cuentaPara.UserName;
        //                                            Newemail.Indice = cuentaPara.UltimoIndice + 1;
        //                                            //email.ClaveGrupo = cuentaPara.Grupo;
        //                                            Newemail.ClaveGrupo = _GrupoEmail;
        //                                            if (!await ChatDataVoice.Negocio.NegocioAgente.ExisteEmailAsync(Newemail.Cuenta, Newemail.Idmail))
        //                                                await ChatDataVoice.Negocio.NegocioAgente.InsertarEmailAsync(Newemail);

        //                                            Newemail.Cuenta = cuenta.UserName;
        //                                            Newemail.Indice = numeroCorreo;
        //                                        }
        //                                    }
        //                                    else
        //                                        correoExistente = true;
        //                            }
        //                            if (correoAgregado == false && correoExistente == false)
        //                            {
        //                                if (Newemail.Idmail != null)
        //                                    if (!await ChatDataVoice.Negocio.NegocioAgente.ExisteEmailAsync(Newemail.Cuenta, Newemail.Idmail))
        //                                    {
        //                                        correoAgregado = true;
        //                                        await ChatDataVoice.Negocio.NegocioAgente.InsertarEmailAsync(Newemail);
        //                                    }
        //                            }
        //                            if (correoAgregado)
        //                            //if(email.Indice == 3837)
        //                            {
        //                                if (!Directory.Exists(pathEmail))
        //                                //if (email.Indice == 3837)
        //                                {
        //                                    Directory.CreateDirectory(pathEmail);
        //                                    ActiveUp.Net.Mail.Message newMessage = new ActiveUp.Net.Mail.Message();

        //                                    //newMessage = pop.RetrieveMessageObject(numeroCorreo);

        //                                    //oMail.Attachments
        //                                    String sHtml = "";
        //                                    string pathv2 = ConfigurationManager.AppSettings["DireccionArchivos"];
        //                                    //string pathAttachements = @"..\..\mail\" + idEmailB64 + @"\";
        //                                    string pathAttachements = @"" + pathv2 + "" + idEmailB64 + @"/";

        //                                    //if (newMessage.IsHtml)
        //                                    //    sHtml = newMessage.BodyHtml.Text;

        //                                    sHtml = oMail.HtmlBody;
        //                                    foreach (var attach in oMail.Attachments)
        //                                    {
        //                                        string FileName = attach.Name.Replace("%", "").Trim();
        //                                        attach.SaveAs(pathEmail + FileName, true);
        //                                    }

        //                                    //foreach (MimePart attach in newMessage.Attachments)
        //                                    //{
        //                                    //    string fileName = attach.Filename.Replace("%", "").Trim();
        //                                    //    attach.StoreToFile(pathEmail + fileName);
        //                                    //}
        //                                    //foreach (var attach in oMail.Attachments)
        //                                    //{
        //                                    //    if (attach.Name != "")
        //                                    //    {
        //                                    //        String cid = attach.ContentID != null ? attach.ContentID.Trim('<', '>') : "";
        //                                    //        string FileName = attach.Name.Replace("%", "").Trim();
        //                                    //        sHtml = sHtml.Replace("cid:" + cid, pathAttachements + FileName);
        //                                    //        attach.SaveAs(pathEmail + FileName);
        //                                    //    }
        //                                    //}


        //                                    //foreach (MimePart attach in newMessage.EmbeddedObjects)
        //                                    //{
        //                                    //    if (attach.Filename != "")
        //                                    //    {
        //                                    //        String cid = attach.ContentId != null ? attach.ContentId.Trim('<', '>') : "";
        //                                    //        string fileName = attach.Filename.Replace("%", "").Trim();
        //                                    //        sHtml = sHtml.Replace("cid:" + cid, pathAttachements + fileName);
        //                                    //        attach.StoreToFile(pathEmail + fileName);
        //                                    //    }
        //                                    //}
        //                                    //foreach (MimePart attach in newMessage.UnknownDispositionMimeParts)
        //                                    //{
        //                                    //    if (newMessage.IsHtml && attach.Filename != "")
        //                                    //    {
        //                                    //        String cid = attach.ContentId.Trim('<', '>');
        //                                    //        string fileName = attach.Filename.Replace("%", "").Trim();
        //                                    //        sHtml = sHtml.Replace("cid:" + cid, pathAttachements + fileName);
        //                                    //        attach.StoreToFile(pathEmail + fileName);
        //                                    //    }
        //                                    //}

        //                                    if (newMessage.IsHtml)
        //                                        CrearArchivosEmail(pathEmail, "Contenido.html", sHtml);
        //                                    CrearArchivosEmail(pathEmail, "Contenido.txt", newMessage.BodyText.TextStripped);

        //                                }
        //                            }
        //                            BackgroundWorker.ReportProgress(((numeroCorreo * 100) / total), Newemail);
        //                            _CorreoProcesados = numeroCorreo;
        //                        }

        //                        if (BackgroundWorker.CancellationPending)
        //                        {
        //                            e.Cancel = true;
        //                            return;
        //                        }

        //                    }
        //                    catch
        //                    {
        //                        //if (!pop.IsConnected)
        //                        //{
        //                        //    Thread.Sleep(5000);
        //                        //    pop.Close();
        //                        //    pop = new Pop3Client();
        //                        //    saveLOG("Error Admin..", DateTime.Now.ToString("ddMMyyyy"), "Entra a pop.Connect 379");
        //                        //    if (cuenta.PortNumber != null)
        //                        //        pop.Connect(cuenta.Server, int.Parse(cuenta.PortNumber), cuenta.UserName, cuenta.Password);
        //                        //    else
        //                        //        pop.Connect(cuenta.Server, cuenta.UserName, cuenta.Password);
        //                        //}
        //                    }

        //                }
        //                _NumeroEmail = 1;

        //            }
        //        }

        //    }
        //    catch (Pop3Exception pexp)
        //    {
        //        saveLOG("Error Pop3Exception..", DateTime.Now.ToString("ddMMyyyy"), pexp.Message.ToString());
        //        this.Invoke(new MethodInvoker(delegate { BtnBajarCorreos.Enabled = true; }));
        //        //MessageBox.Show("Error: " + pexp.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        this.Invoke(new MethodInvoker(delegate { LblMensaje.Text = "Error. Detalles:: " + pexp.Message.ToString(); }));
        //        Int32 segundosSleepErrorEmail = Convert.ToInt32(ConfigurationManager.AppSettings["SegundosSleepErrorEmail"]) * 1000;
        //        System.Threading.Thread.Sleep(segundosSleepErrorEmail);
        //    }
        //    catch (Exception ex)
        //    {
        //        saveLOG("Admin", DateTime.Now.ToString("ddMMyyyy"), ex.Message.ToString() + " " + ex.StackTrace);
        //        this.Invoke(new MethodInvoker(delegate { BtnBajarCorreos.Enabled = true; }));
        //        this.Invoke(new MethodInvoker(delegate { LblMensaje.Text = "Error: " + ex.Message.ToString(); }));
        //        Int32 segundosSleepErrorEmail = Convert.ToInt32(ConfigurationManager.AppSettings["SegundosSleepErrorEmail"]) * 1000;
        //        System.Threading.Thread.Sleep(segundosSleepErrorEmail);
        //        //  LblMensaje.Text = "Error: " + ex.Message.ToString();
        //        // MessageBox.Show("Error: " + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //    finally
        //    {
        //        if (pop.IsConnected)
        //        {
        //            pop.Disconnect();
        //        }
        //    }

        //    TimeSpan duration = DateTime.Now - start;

        //    //aquí podríamos devolver información de utilidad, como el resultado de un cálculo,
        //    //número de elementos afectados, etc.. de manera sencilla y segura
        //    //al hilo principal
        //    e.Result = "Fecha Fin " + DateTime.Now.ToUniversalTime();
        //}

        private void saveLOG(string usuario, string fecha, string accion)
        {
            try
            {
                string fecha_actual = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                // string path = Environment.CurrentDirectory + @"\" + usuario + "_" + fecha + ".txt";
                string path = @"C:\Temporales\LogEmail\" + usuario + NameFile + "_" + fecha + ".txt";
                string Conten = "[" + fecha_actual + "]\n {Usuario: " + usuario + ", accionRealizada: " + accion + " ]";
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    fs.Seek(0, SeekOrigin.End);
                    using (StreamWriter writer = new StreamWriter(fs))
                        writer.WriteLine(Conten);
                }
                //File.AppendAllText(path, Conten + Environment.NewLine);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task saveLOGAsync(string usuario, string fecha, string accion)
        {
            try
            {
                string fecha_actual = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                // string path = Environment.CurrentDirectory + @"\" + usuario + "_" + fecha + ".txt";
                string path = @"C:\Temporales\LogEmail\" + usuario + NameFile + "_" + fecha + ".txt";
                string Conten = "[" + fecha_actual + "]\n {Usuario: " + usuario + ", accionRealizada: " + accion + " ]";
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    fs.Seek(0, SeekOrigin.End);
                    using (StreamWriter writer = new StreamWriter(fs))
                        await writer.WriteLineAsync(Conten);
                }
                //File.AppendAllText(path, Conten + Environment.NewLine);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                BarraProgreso.Value = e.ProgressPercentage + 1;
                //Actualizamos la barra de progreso   
                TxtCorreosProcesados.Text = _CorreoProcesados.ToString();
                this.AddLogEntry(string.Format("{1}", "", ((Mail)e.UserState).Subject));
            }
            catch (Exception ex)
            {
                saveLOG("Error en backgroundWorker1_ProgressChanged", DateTime.Now.ToString("ddMMyyyy"), ex.Message);
                throw ex;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                LblMensaje.Text = "La Operación fue Cancelada";
                LblTemporizador.Text = "Cancelado";
                //MessageBox.Show(LblMensaje.Text, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                saveLOG("La Operación fue Cancelada", DateTime.Now.ToString("ddMMyyyy"), e.Cancelled.ToString());
            }
            else if (e.Error != null)
            {
                _Temporizador = 60;
                Temporizador.Start();
                LblMensaje.Text = "Error. Detalles: " + (e.Error as Exception).ToString();
                saveLOG("Error en backgroundWorker1_RunWorkerCompleted", DateTime.Now.ToString("ddMMyyyy"), e.Error.ToString());

            }
            else
            {
                //_Temporizador = 25;
                _Temporizador = Convert.ToInt32(ConfigurationManager.AppSettings["TiempoTimerEmail"]);
                Temporizador.Start();
                LblMensaje.Text = "La Tarea fue Completada. " + e.Result.ToString();

            }
            BtnBajarCorreos.Enabled = true;
        }
        #endregion

        void CargarCuentasEmail()
        {
            CurrentMailAccounts = new List<EmailAccounts>();
            foreach (CuentaEmail cuenta in NegocioAgente.ObtenerCuentasEmailUsuario("G18"))
            {
                EmailAccounts account = new EmailAccounts();
                account.Server = cuenta.Servidor;
                account.UserName = cuenta.Usuario;
                account.Password = cuenta.Password;
                account.Grupo = cuenta.ClaveGrupo;
                account.UltimoIndice = cuenta.UltimoIndice;
                account.ActivoSubjet = cuenta.ActivoSubjet;
                account.Subjet = cuenta.Subjet;
                account.PuertoEntrada = cuenta.PuertoEntrada;
                account.CrifadoEntrada = cuenta.CrifadoEntrada;
                account.PuertoSalida = cuenta.PuertoSalida;
                account.CifradoSalida = cuenta.CifradoSalida;

                CurrentMailAccounts.Add(account);
            }
        }

        async Task CargarCuentasEmailAsync()
        {
            CurrentMailAccounts = new List<EmailAccounts>();
            foreach (CuentaEmail cuenta in await NegocioAgente.ObtenerCuentasEmailUsuarioAsync("G18"))
            {
                EmailAccounts account = new EmailAccounts();
                account.Server = cuenta.Servidor;
                account.UserName = cuenta.Usuario;
                account.Password = cuenta.Password;
                account.Grupo = cuenta.ClaveGrupo;
                account.UltimoIndice = cuenta.UltimoIndice;
                account.ActivoSubjet = cuenta.ActivoSubjet;
                account.Subjet = cuenta.Subjet;
                account.PuertoEntrada = cuenta.PuertoEntrada;
                account.CrifadoEntrada = cuenta.CrifadoEntrada;
                account.PuertoSalida = cuenta.PuertoSalida;
                account.CifradoSalida = cuenta.CifradoSalida;

                CurrentMailAccounts.Add(account);
            }
        }

        private async void Temporizador_Tick(object sender, EventArgs e)
        {
            if (_Temporizador > 0)
            {
                _Temporizador--;
                this.Invoke(new MethodInvoker(delegate
                {
                    LblTemporizador.Text = _Temporizador.ToString() + " segundos";
                }));
            }
            else
            {
                Temporizador.Stop();
                this.Invoke(new MethodInvoker(delegate
                {
                    BarraProgreso.Value = 0;
                    BarraProgreso.Refresh();
                    LstCorreos.Items.Clear();
                }));
                //if (!BackgroundWorker.IsBusy)
                //BackgroundWorker.RunWorkerAsync();
                this.Invoke(new MethodInvoker(delegate
                { BtnBajarCorreos.Enabled = false; }));
                await ObtenerCorreos();

            }
        }

        private void Cuentas_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }


        public async Task ObtenerCorreos()
        {
            DateTime start = DateTime.Now;
            try
            {
                this.Invoke(new MethodInvoker(delegate { LblTemporizador.Text = "Procesando"; }));
                this.Invoke(new MethodInvoker(delegate { LblMensaje.Text = ""; }));
                string LicenseCodeEAGetMail = ConfigurationManager.AppSettings["LicenseCodeEAGetMail"];

                Temporizador.Stop();
                List<CuentasEmail> cuentas = await NegocioAgente.ObtenerCuentasEmailAsync("G18");

                foreach (TreeNode nodo in Cuentas.Nodes)
                {
                    if (nodo.Checked)
                    {
                        MailServer oServer;
                        //EmailAccounts cuenta = CurrentMailAccounts.Find(seleccion => seleccion.UserName == nodo.Text);
                        CuentasEmail cuenta = cuentas.Where(seleccion => seleccion.UserName == nodo.Text).FirstOrDefault();
                        _NumeroEmail = cuenta.UltimoIndice;
                        if (cuenta.Oauth2)
                        {
                            oServer = await RetrieveEmailOauth(cuenta);
                        }
                        else
                        {
                            oServer = await RetrieveEmail(cuenta);
                        }


                        MailClient oClient = new MailClient(LicenseCodeEAGetMail);
                        // Get new email only, if you want to get all emails, please remove this line
                        oClient.GetMailInfosParam.GetMailInfosOptions = GetMailInfosOptionType.NewOnly;

                        Console.WriteLine("Connecting {0} ...", oServer.Server);
                        oClient.Connect(oServer);

                        MailInfo[] emails = oClient.GetMailInfos();
                        Console.WriteLine("Total {0} email(s)\r\n", emails.Length);
                        this.Invoke(new MethodInvoker(delegate { TxtNumeroCorreo.Text = emails.Length.ToString(); }));

                        int total = emails.Length;
                        string path = ConfigurationManager.AppSettings["UbicacionArchivos"];
                        if (total < _NumeroEmail)
                        {
                            _NumeroEmail = _NumeroEmail - total;
                        }

                        for (int numeroCorreo = _NumeroEmail; numeroCorreo < emails.Length; numeroCorreo++)
                        {
                            try
                            {
                                MailInfo email = emails[numeroCorreo];
                                Console.WriteLine("Index: {0}; Size: {1}; UIDL: {2}",
                                    email.Index, email.Size, email.UIDL);

                                // Receive email from email server
                                Mail oMail = oClient.GetMail(email);
                                string DiasMaximo = ConfigurationManager.AppSettings["DiasMaximo"].ToString();
                                int diasMaximo = 0;
                                int.TryParse(DiasMaximo, out diasMaximo);
                                DateTime FechaActual = DateTime.Now;
                                var diferencia = FechaActual - oMail.ReceivedDate;
                                var diasDiferencia = diferencia.Days;

                                if (diasDiferencia < diasMaximo)
                                {
                                    Email Newemail = new Email();
                                    string _GrupoEmail = "";

                                    DataTable matriz = await NegocioAgente.ObtenerRegistrosMaiMatAsync(9, cuenta.UserName);
                                    string cadenaMensaje = oMail.Subject.ToLower();

                                    for (int item = 0; item < matriz.Rows.Count; item++)
                                    {
                                        if (matriz.Rows[item]["activoSubject"].ToString() == "2")
                                        {
                                            _GrupoEmail = matriz.Rows[item]["claveGrupo"].ToString();
                                            break;
                                        }
                                        else if (cadenaMensaje.Contains(matriz.Rows[item]["subject"].ToString().ToLower()))
                                        {
                                            _GrupoEmail = matriz.Rows[item]["claveGrupo"].ToString();
                                            break;
                                        }
                                    }

                                    if (_GrupoEmail == string.Empty)
                                    {
                                        _GrupoEmail = await NegocioAgente.ObtenerGrupoSubjectDefaultAsync(cuenta.UserName);
                                    }

                                    switch (cuenta.ActivoSubjet)
                                    {
                                        case "0":
                                            _GrupoEmail = cuenta.Grupo;
                                            break;
                                        case "1":
                                            //case "2":
                                            string _CadenaArreglo = "";
                                            string cadena = oMail.Subject;
                                            string finalSubject = string.Empty;
                                            string asunto = RemoverCaracteresEspeciales(cadena);

                                            string[] resultCadena = asunto.Split(new string[] { " " }, StringSplitOptions.None);
                                            foreach (string resultCadenaSplit in resultCadena)
                                            {
                                                if (resultCadenaSplit != "")
                                                    _CadenaArreglo += " subject LIKE '%" + resultCadenaSplit + "%' OR";
                                            }
                                            if (_CadenaArreglo != "")
                                            {
                                                _CadenaArreglo = _CadenaArreglo.Remove(_CadenaArreglo.Length - 2);
                                            }

                                            _GrupoEmail = await ObtenerGrupoPorSubject(_CadenaArreglo, cuenta.UserName);

                                            if (_GrupoEmail == string.Empty)
                                            {
                                                _GrupoEmail = await NegocioAgente.ObtenerGrupoSubjectDefaultAsync(cuenta.UserName);
                                            }
                                            break;
                                    }

                                    Newemail.Indice = email.Index;
                                    Newemail.Cuenta = cuenta.UserName;
                                    Newemail.ClaveGrupo = _GrupoEmail;

                                    //TimeSpan diferencia = DateTime.Now - mensaje.ReceivedDate;
                                    //email.FechaCorreo = mensaje.ReceivedDate.Add(diferencia);
                                    Newemail.FechaCorreo = oMail.ReceivedDate.ToLocalTime();
                                    Newemail.Idmail = oMail.ReceivedDate.ToOADate() + cuenta.UserName;
                                    //email.ClaveGrupo = cuenta.Grupo;
                                    Newemail.Titulo = oMail.Subject != null ? oMail.Subject : "";

                                    Newemail.NombreDe = oMail.From.Name;

                                    Newemail.De = oMail.From.Address;
                                    if (Newemail.De != cuenta.UserName || oMail.Cc.Contains(cuenta.UserName))
                                    {
                                        Newemail.Estatus = "Nuevo";
                                        Newemail.Asignado = 0;
                                        Newemail.ClaveEstatus = 0;
                                        Newemail.Para = cuenta.UserName;
                                        //Newemail.EmailPara = oMail.To.FirstOrDefault().Address;
                                        if (oMail.To != null)
                                            Newemail.EmailPara = String.Join(",", oMail.To.Select(x => x.Address));
                                        Newemail.Cuenta = cuenta.UserName;
                                        // Newemail.ConCopia = oMail.Cc.FirstOrDefault().Address;
                                        if (oMail.Cc != null)
                                            Newemail.ConCopia = String.Join(",", oMail.Cc.Select(x => x.Address));
                                        string idEmailB64 = System.Convert.ToBase64String(System.Text.ASCIIEncoding.UTF8.GetBytes(Newemail.Idmail));
                                        string pathEmail = path + idEmailB64 + @"\";
                                        Newemail.ArchivoEmailHtml = pathEmail + "Contenido.html";
                                        Newemail.ArchivoEmailtxt = pathEmail + "Contenido.txt";
                                        Newemail.ArchivoAdjunto = pathEmail;

                                        bool correoAgregado = false;
                                        bool correoExistente = false;
                                        foreach (MailAddress para in oMail.To)
                                        {
                                            if (Newemail.Idmail != null)
                                                if (!await NegocioAgente.ExisteEmailAsync(para.Address.Trim(), Newemail.Idmail.Trim()))
                                                {
                                                    EmailAccounts cuentaPara = CurrentMailAccounts.Find(seleccion => seleccion.UserName == para.Address);
                                                    if (cuentaPara != null)
                                                    {
                                                        correoAgregado = true;
                                                        Newemail.Cuenta = cuentaPara.UserName;
                                                        Newemail.Indice = cuentaPara.UltimoIndice + 1;
                                                        //email.ClaveGrupo = cuentaPara.Grupo;
                                                        Newemail.ClaveGrupo = _GrupoEmail;
                                                        if (!await NegocioAgente.ExisteEmailAsync(Newemail.Cuenta, Newemail.Idmail))
                                                            await NegocioAgente.InsertarEmailAsync(Newemail);

                                                        Newemail.Cuenta = cuenta.UserName;
                                                        Newemail.Indice = numeroCorreo;
                                                    }
                                                }
                                                else
                                                    correoExistente = true;
                                        }
                                        if (correoAgregado == false && correoExistente == false)
                                        {
                                            if (Newemail.Idmail != null)
                                                if (!await NegocioAgente.ExisteEmailAsync(Newemail.Cuenta, Newemail.Idmail))
                                                {
                                                    correoAgregado = true;
                                                    await NegocioAgente.InsertarEmailAsync(Newemail);
                                                }
                                        }
                                        if (correoAgregado)
                                        {
                                            if (!Directory.Exists(pathEmail))
                                            {
                                                Directory.CreateDirectory(pathEmail);
                                                //oMail.Attachments
                                                String sHtml = "";
                                                string pathv2 = ConfigurationManager.AppSettings["DireccionArchivos"];
                                                //string pathAttachements = @"..\..\mail\" + idEmailB64 + @"\";
                                                string pathAttachements = @"" + pathv2 + "" + idEmailB64 + @"/";

                                                //if (newMessage.IsHtml)
                                                //    sHtml = newMessage.BodyHtml.Text;

                                                sHtml = oMail.HtmlBody;
                                                foreach (var attach in oMail.Attachments)
                                                {
                                                    String cid = attach.ContentID.Trim('<', '>');

                                                    string FileName = attach.Name.Replace("%", "").Trim();
                                                    sHtml = sHtml.Replace("cid:" + cid, pathAttachements + FileName);
                                                    attach.SaveAs(pathEmail + FileName, true);
                                                }

                                                //foreach (MimePart attach in newMessage.Attachments)
                                                //{
                                                //    string fileName = attach.Filename.Replace("%", "").Trim();
                                                //    attach.StoreToFile(pathEmail + fileName);
                                                //}
                                                //foreach (var attach in oMail.Attachments)
                                                //{
                                                //    if (attach.Name != "")
                                                //    {
                                                //        String cid = attach.ContentID != null ? attach.ContentID.Trim('<', '>') : "";
                                                //        string FileName = attach.Name.Replace("%", "").Trim();
                                                //        sHtml = sHtml.Replace("cid:" + cid, pathAttachements + FileName);
                                                //        attach.SaveAs(pathEmail + FileName);
                                                //    }
                                                //}


                                                //foreach (MimePart attach in newMessage.EmbeddedObjects)
                                                //{
                                                //    if (attach.Filename != "")
                                                //    {
                                                //        String cid = attach.ContentId != null ? attach.ContentId.Trim('<', '>') : "";
                                                //        string fileName = attach.Filename.Replace("%", "").Trim();
                                                //        sHtml = sHtml.Replace("cid:" + cid, pathAttachements + fileName);
                                                //        attach.StoreToFile(pathEmail + fileName);
                                                //    }
                                                //}
                                                //foreach (MimePart attach in newMessage.UnknownDispositionMimeParts)
                                                //{
                                                //    if (newMessage.IsHtml && attach.Filename != "")
                                                //    {
                                                //        String cid = attach.ContentId.Trim('<', '>');
                                                //        string fileName = attach.Filename.Replace("%", "").Trim();
                                                //        sHtml = sHtml.Replace("cid:" + cid, pathAttachements + fileName);
                                                //        attach.StoreToFile(pathEmail + fileName);
                                                //    }
                                                //}

                                                CrearArchivosEmail(pathEmail, "Contenido.html", sHtml);
                                                CrearArchivosEmail(pathEmail, "Contenido.txt", oMail.HtmlBody);

                                            }
                                        }
                                        _CorreoProcesados = numeroCorreo;
                                        this.Invoke(new MethodInvoker(delegate { BarraProgreso.Value = ((email.Index * 100) / total); }));
                                        //Actualizamos la barra de progreso   
                                        this.Invoke(new MethodInvoker(delegate { TxtCorreosProcesados.Text = _CorreoProcesados.ToString(); }));
                                        this.AddLogEntry(string.Format("{1}", "", ((Mail)oMail).Subject));
                                        //BackgroundWorker.ReportProgress(((numeroCorreo + 1 * 100) / total), oMail);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                oClient.Quit();
                                await saveLOGAsync("Admin-1", DateTime.Now.ToString("ddMMyyyy"), ex.Message.ToString());
                                Thread.Sleep(5000);
                            }
                        }
                        // Quit and expunge emails marked as deleted from server.
                        oClient.Quit();
                        Console.WriteLine("Completed!");
                    }
                }
            }
            catch (Exception ex)
            {
                await saveLOGAsync("Admin", DateTime.Now.ToString("ddMMyyyy"), ex.Message.ToString()+"_"+ ex.StackTrace + "_" + ex.ToString());
                this.Invoke(new MethodInvoker(delegate { BtnBajarCorreos.Enabled = true; }));
                this.Invoke(new MethodInvoker(delegate { LblMensaje.Text = "Error: " + ex.Message.ToString(); }));
                Int32 segundosSleepErrorEmail = Convert.ToInt32(ConfigurationManager.AppSettings["SegundosSleepErrorEmail"]) * 1000;
                System.Threading.Thread.Sleep(segundosSleepErrorEmail);
                //  LblMensaje.Text = "Error: " + ex.Message.ToString();
                // MessageBox.Show("Error: " + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _Temporizador = Convert.ToInt32(ConfigurationManager.AppSettings["TiempoTimerEmail"]);
                Temporizador.Start();
                this.Invoke(new MethodInvoker(delegate { LblMensaje.Text = "La Tarea fue Completada. Fecha Fin " + DateTime.Now + " "; }));
                this.Invoke(new MethodInvoker(delegate { BtnBajarCorreos.Enabled = true; }));
            }

            TimeSpan duration = DateTime.Now - start;
            //aquí podríamos devolver información de utilidad, como el resultado de un cálculo,
            //número de elementos afectados, etc.. de manera sencilla y segura
            //al hilo principal
        }
        static string _generateFileName(int sequence)
        {
            DateTime currentDateTime = DateTime.Now;
            return string.Format("{0}-{1:000}-{2:000}.eml",
                currentDateTime.ToString("yyyyMMddHHmmss", new CultureInfo("en-US")),
                currentDateTime.Millisecond,
                sequence);
        }

        static async Task<string> _postString(string uri, string requestData)
        {
            HttpWebRequest httpRequest = WebRequest.Create(uri) as HttpWebRequest;
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/x-www-form-urlencoded";

            using (Stream requestStream = httpRequest.GetRequestStream())
            {
                byte[] requestBuffer = Encoding.UTF8.GetBytes(requestData);
                requestStream.Write(requestBuffer, 0, requestBuffer.Length);
                requestStream.Close();
            }

            try
            {
                HttpWebResponse httpResponse = await httpRequest.GetResponseAsync() as HttpWebResponse;
                using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    // reads response body
                    string responseText = reader.ReadToEnd();
                    Console.WriteLine(responseText);
                    return responseText;
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null)
                    {
                        Console.WriteLine("HTTP: " + response.StatusCode);
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            // reads response body
                            string responseText = reader.ReadToEnd();
                            Console.WriteLine(responseText);
                        }
                    }
                }

                throw;
            }
        }

        public async Task<MailServer> RetrieveEmail(CuentasEmail cuentaEmail)
        {
            OAuthResponseParser parser = new OAuthResponseParser();
            //string officeUser = "AdanRobirosa@EmpresaDemo607.onmicrosoft.com";
            MailServer oServer = null;
            try
            {
                string officeUser = cuentaEmail.UserName;
                oServer = new MailServer(cuentaEmail.Servidor,
                        officeUser,
                       cuentaEmail.Password, // use access token as password
                        cuentaEmail.Pop3 ? ServerProtocol.Pop3 : ServerProtocol.Imap4); // use IMAP protocol

                // Enable SSL/TLS connection
                oServer.SSLConnection = true;
                // Set IMAP4 SSL Port
                oServer.Port = int.Parse(cuentaEmail.PuertoEntrada);
            }
            catch (Exception ex)
            {
                await saveLOGAsync("RetrieveEmail..", DateTime.Now.ToString("ddMMyyyy"), ex.Message.ToString());

            }
            return oServer;
        }


        public async Task<MailServer> RetrieveEmailOauth(CuentasEmail cuentaEmail)
        {
            MailServer oServer = null;
            try
            {

                string TimeToken = ConfigurationManager.AppSettings["TiempoToken"].ToString();
                int timeToken = 0;
                int.TryParse(TimeToken, out timeToken);
                DateTime FechaActual = DateTime.Now;
                var diferencia = FechaActual - cuentaEmail.FechaToken;
                var minutosDiferencia = diferencia.TotalMinutes;
                string responseText = "";
                OAuthResponseParser parser = new OAuthResponseParser();
                if (minutosDiferencia > timeToken || string.IsNullOrEmpty(cuentaEmail.AccessToken))
                {
                    string scope = ConfigurationManager.AppSettings["scopeOffice365"];
                    string urlToken = ConfigurationManager.AppSettings["urlTokenOffice"];
                    string requestData =
                        string.Format("client_id={0}&client_secret={1}&scope={2}&grant_type=client_credentials",
                            cuentaEmail.ClientId, cuentaEmail.ClientSecret, scope);

                    string tokenUri = string.Format(urlToken, cuentaEmail.TenantId);
                    responseText = await _postString(tokenUri, requestData);
                    parser.Load(responseText);
                    cuentaEmail.AccessToken = parser.AccessToken;
                    await NegocioAgente.ActualizarTokenEmailAsync(cuentaEmail, 1);
                }
                else
                {
                    parser.AccessToken = cuentaEmail.AccessToken;
                }

                //string officeUser = "AdanRobirosa@EmpresaDemo607.onmicrosoft.com";
                string officeUser = cuentaEmail.UserName;

                // Office 365 server address
                oServer = new MailServer(cuentaEmail.Servidor,
                        officeUser,
                        parser.AccessToken, // use access token as password
                        cuentaEmail.Pop3 ? ServerProtocol.Pop3 : ServerProtocol.Imap4); // use IMAP protocol

                // Set OAUTH 2.0
                oServer.AuthType = ServerAuthType.AuthXOAUTH2;
                // Enable SSL/TLS connection
                oServer.SSLConnection = true;
                // Set IMAP4 SSL Port
                oServer.Port = int.Parse(cuentaEmail.PuertoEntrada);

            }
            catch (Exception ep)
            {
                Console.WriteLine(ep.ToString());
            }

            return oServer;
        }

    }


}


