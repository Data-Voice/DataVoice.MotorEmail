using DataVoice.MotorEmail.Models;
using DataVoice.MotorEmail.Negocio;
using EAGetMail;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataVoice.MotorEmail
{
    public partial class MainForm : Form
    {
        int _CorreoProcesados;
        int _Temporizador = 60;
        int _NumeroEmail = 1;
        private CancellationTokenSource _ctsObtenerCorreos;
        private bool _procesando;
        private readonly Dictionary<string, int> _erroresPorCategoria = new Dictionary<string, int>();
        private int _erroresTotales;
        private string _ultimoError = "";
        string NameFile = ConfigurationManager.AppSettings["NameFile"].ToString();
        public delegate string AsyncMethodCaller(int callDuration, out int threadId);

        public MainForm()
        {
            this.MaximizeBox = false;
            InitializeComponent();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await CargarCuentasAsync();
        }

        private async Task CargarCuentasAsync()
        {
            try
            {
                string cuentaEmail = "";
                foreach (CuentaEmail cuenta in await NegocioAgente.ObtenerCuentasEmailUsuarioAsync("G18"))
                {
                    if (cuentaEmail != cuenta.Usuario)
                    {
                        TreeNode node = new TreeNode(cuenta.Usuario);
                        Cuentas.Nodes.Add(node);
                    }
                    cuentaEmail = cuenta.Usuario;
                }
            }
            catch (Exception ex)
            {
                await saveLOGAsync("Admin-1", DateTime.Now.ToString("ddMMyyyy"), "Error al cargar las cuentas: " + ex);
            }
        }

        private async void _bRetrieveMessageList_Click(object sender, EventArgs e)
        {
            BarraProgreso.Value = 0;
            BarraProgreso.Refresh();
            LstCorreos.Items.Clear();
            await ObtenerCorreos();
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            if (_ctsObtenerCorreos != null)
            {
                _ctsObtenerCorreos.Cancel();
                LblMensaje.Text = "Cancelando...";
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (_ctsObtenerCorreos != null)
                    _ctsObtenerCorreos.Cancel();
            }
            catch
            {
            }
            base.OnFormClosing(e);
        }

        private void ActualizarUI(Action accion)
        {
            try
            {
                if (IsDisposed || !IsHandleCreated)
                    return;
                Invoke(accion);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
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
            ActualizarUI(delegate
            {
                LstCorreos.Items.Insert(0, sb.ToString());
                while (LstCorreos.Items.Count > 200)
                    LstCorreos.Items.RemoveAt(LstCorreos.Items.Count - 1);
            });
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
                string directorio = @"C:\Temporales\LogEmail\";
                if (!Directory.Exists(directorio))
                    Directory.CreateDirectory(directorio);
                string path = directorio + usuario + NameFile + "_" + fecha + ".txt";
                string Conten = "[" + fecha_actual + "]\n {Usuario: " + usuario + ", accionRealizada: " + accion + " ]";
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    fs.Seek(0, SeekOrigin.End);
                    using (StreamWriter writer = new StreamWriter(fs))
                        writer.WriteLine(Conten);
                }
            }
            catch
            {
            }
        }

        private async Task saveLOGAsync(string usuario, string fecha, string accion)
        {
            try
            {
                string fecha_actual = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                string directorio = @"C:\Temporales\LogEmail\";
                if (!Directory.Exists(directorio))
                    Directory.CreateDirectory(directorio);
                string path = directorio + usuario + NameFile + "_" + fecha + ".txt";
                string Conten = "[" + fecha_actual + "]\n {Usuario: " + usuario + ", accionRealizada: " + accion + " ]";
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    fs.Seek(0, SeekOrigin.End);
                    using (StreamWriter writer = new StreamWriter(fs))
                        await writer.WriteLineAsync(Conten);
                }
            }
            catch
            {
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

        

private static string LimpiarEmojis(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return texto;

        // Elimina caracteres Unicode fuera de BMP (emojis)
        return Regex.Replace(
            texto,
            @"[\uD800-\uDBFF][\uDC00-\uDFFF]",
            ""
        );
    }

    public async Task ObtenerCorreos()
        {
            if (_procesando)
                return;

            List<string> cuentasSeleccionadas = Cuentas.Nodes.Cast<TreeNode>()
                .Where(n => n.Checked)
                .Select(n => n.Text)
                .ToList();

            if (cuentasSeleccionadas.Count == 0)
                return;

            _procesando = true;
            _ctsObtenerCorreos = new CancellationTokenSource();
            CancellationToken token = _ctsObtenerCorreos.Token;
            _erroresTotales = 0;
            _erroresPorCategoria.Clear();
            _ultimoError = "";
            BtnBajarCorreos.Enabled = false;
            LblTemporizador.Text = "Procesando";
            LblMensaje.Text = "";
            LblErrores.Text = "";
            Temporizador.Stop();

            try
            {
            await Task.Run(async () =>
            {
            MailClient oClient = null;
            MailServer oServer = null;
            CuentasEmail cuenta = null;
            DateTime? fechaMasReciente = null;

            DateTime start = DateTime.Now;
            try
            {
                string LicenseCodeEAGetMail = ConfigurationManager.AppSettings["LicenseCodeEAGetMail"];
                List<CuentasEmail> cuentas = await NegocioAgente.ObtenerCuentasEmailAsync("G18");

                foreach (string nodoText in cuentasSeleccionadas)
                {
                    if (token.IsCancellationRequested)
                        break;
                        try
                        {
                        //MailServer oServer;
                        //EmailAccounts cuenta = CurrentMailAccounts.Find(seleccion => seleccion.UserName == nodo.Text);
                        //CuentasEmail cuenta = cuentas.Where(seleccion => seleccion.UserName == nodo.Text).FirstOrDefault();
                        cuenta = cuentas.Where(seleccion => seleccion.UserName == nodoText).FirstOrDefault();
                        if (cuenta == null)
                        {
                            await saveLOGAsync("Admin-1", DateTime.Now.ToString("ddMMyyyy"), "La cuenta del nodo '" + nodoText + "' no existe en la vista de cuentas.");
                            continue;
                        }
                        _NumeroEmail = cuenta.UltimoIndice;
                        if (cuenta.Oauth2)
                        {
                            oServer = await RetrieveEmailOauth(cuenta);
                        }
                        else
                        {
                            oServer = await RetrieveEmail(cuenta);
                        }


                        if (oServer == null)
                        {
                            await saveLOGAsync("Admin-1", DateTime.Now.ToString("ddMMyyyy"), "No se pudo crear el servidor de correo para '" + cuenta.UserName + "'.");
                            continue;
                        }

                        //MailClient oClient = new MailClient(LicenseCodeEAGetMail);
                        oClient = new MailClient(LicenseCodeEAGetMail);
                        // Get new email only, if you want to get all emails, please remove this line
                       // oClient.GetMailInfosParam.GetMailInfosOptions = GetMailInfosOptionType.NewOnly;

                        Console.WriteLine("Connecting {0} ...", oServer.Server);
                        oClient.Connect(oServer);





                        // MailInfo[] emails = oClient.GetMailInfos();
                        // Console.WriteLine("Total {0} email(s)\r\n", emails.Length);
                        // this.Invoke(new MethodInvoker(delegate { TxtNumeroCorreo.Text = emails.Length.ToString(); }));

                        //oClient.Connect(oServer);
                        //MailInfo[] emails;
                        //MailInfo[] emails = oClient.GetMailInfos();

                        bool isImap = (oServer.Protocol == ServerProtocol.Imap4);
                        int diasMaximo = 5;
                        int.TryParse(ConfigurationManager.AppSettings["DiasMaximo"], out diasMaximo);
                        bool sinFechaUltimoRegistro = cuenta.FechaUltimoRegistro == null || cuenta.FechaUltimoRegistro == DateTime.MinValue;
                        DateTime fechaFiltro = sinFechaUltimoRegistro
                            ? DateTime.Now.AddDays(-diasMaximo)
                            : cuenta.FechaUltimoRegistro.Value.AddSeconds(-1);

                        MailInfo[] emails = null;

                        if (isImap)
                        {
                            Imap4Folder[] folders = oClient.GetFolders();
                            Imap4Folder inboxFolder = folders
                                .FirstOrDefault(f => f.Name.Equals("INBOX", StringComparison.OrdinalIgnoreCase));

                            if (inboxFolder != null)
                                oClient.SelectFolder(inboxFolder);

                          
                            oClient.GetMailInfosParam.Reset();
                            oClient.GetMailInfosParam.GetMailInfosOptions = GetMailInfosOptionType.DateRange;
                            oClient.GetMailInfosParam.DateRange.SINCE = fechaFiltro;

                            emails = oClient.GetMailInfos();
                        }
                        else
                        {
                       
                            var todos = oClient.GetMailInfos();
                            List<MailInfo> filtrados = new List<MailInfo>();

                            foreach (var info in todos)
                            {
                                Mail m = new Mail(LicenseCodeEAGetMail);
                                m.Load(oClient.GetMailHeader(info));

                                if (m.ReceivedDate.ToLocalTime() >= fechaFiltro)
                                    filtrados.Add(info);
                            }

                            emails = filtrados.ToArray();
                        }

                        //if (isImap)
                        //{
                        //    Imap4Folder[] folders = oClient.GetFolders();
                        //    Imap4Folder inboxFolder = folders.FirstOrDefault(f => f.Name.Equals("INBOX", StringComparison.OrdinalIgnoreCase));
                        //    oClient.SelectFolder(inboxFolder);
                        //    oClient.GetMailInfosParam.Reset();


                        //    ////int startSeq = Math.Max(1, totalCount - recentCount + 1);
                        //    ////oClient.GetMailInfosParam.GetMailInfosOptions = GetMailInfosOptionType.SeqRange;
                        //    ////oClient.GetMailInfosParam.SeqRange = $"{startSeq}:*";

                        //    // Filtra con IMAP SEARCH
                        //    oClient.GetMailInfosParam.GetMailInfosOptions = GetMailInfosOptionType.DateRange;
                        //    DateTime startDate = DateTime.Now.AddDays(-5);
                        //    int totalCount = oClient.GetMailCount();
                        //    int recentCount = Math.Min(500, totalCount);
                        //    oClient.GetMailInfosParam.DateRange.SINCE = startDate;
                        //    emails = oClient.GetMailInfos();
                        //}
                        //else
                        //{
                        //    DateTime fechaLimite = DateTime.Now.AddDays(-5);
                        //    List<Mail> correosFiltrados = new List<Mail>();

                        //    foreach (var info in emails)
                        //    {
                        //        Mail oMail = oClient.GetMail(info); // descarga completa
                        //        if (oMail.ReceivedDate >= fechaLimite)
                        //        {
                        //            correosFiltrados.Add(oMail);
                        //        }
                        //    }

                        //}





                        int total = emails.Length;
                        string path = ConfigurationManager.AppSettings["UbicacionArchivos"];
                        int procesados = 0;
                        int indiceInterno = cuenta.UltimoIndice;
                        DataTable matriz = await NegocioAgente.ObtenerRegistrosMaiMatAsync(9, cuenta.UserName);
                        bool abortarCuenta = false;
                        Stopwatch sw = Stopwatch.StartNew();
                        long ultimaActualizacion = 0;
                        foreach (var info in emails.OrderBy(e => e.Index))
                        {
                            bool reintentado = false;
                            while (true)
                            {
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                MailInfo email = info;

                                Console.WriteLine("Index: {0}; Size: {1}; UIDL: {2}",
                                    email.Index, email.Size, email.UIDL);

                                // Receive email from email server
                                Mail oMail = oClient.GetMail(email);
                                var diasDiferencia = (DateTime.Now - oMail.ReceivedDate.ToLocalTime()).Days;

                                if (!sinFechaUltimoRegistro || diasDiferencia < diasMaximo)
                                {
                                    Email Newemail = new Email();
                                    string _GrupoEmail = "";

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
                                    //int ultimoGuardado = await NegocioAgente.ObtenerUltimoIndiceBD(cuenta.UserName);
                                    //int indiceInterno = ultimoGuardado + 1;
                                   

                                    //Newemail.Indice = email.Index;
                                    Newemail.Cuenta = cuenta.UserName;
                                    Newemail.ClaveGrupo = _GrupoEmail;
                                    //Newemail.FechaUltimoRegistro = oMail.ReceivedDate;
                                    //TimeSpan diferencia = DateTime.Now - mensaje.ReceivedDate;
                                    //email.FechaCorreo = mensaje.ReceivedDate.Add(diferencia);
                                    Newemail.FechaCorreo = oMail.ReceivedDate.ToLocalTime();
                                    Newemail.FechaUltimoRegistro = Newemail.FechaCorreo;
                                    Newemail.Idmail = oMail.ReceivedDate.ToOADate() + cuenta.UserName;
                                    //email.ClaveGrupo = cuenta.Grupo;
                                    //Newemail.Titulo = oMail.Subject != null ? oMail.Subject : "";
                                    Newemail.Titulo = oMail.Subject != null
                                    ? LimpiarEmojis(oMail.Subject)
                                    : "";

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
                                        if (Newemail.Idmail != null &&
                                            !await NegocioAgente.ExisteEmailAsync(Newemail.Cuenta, Newemail.Idmail))
                                        {
                                            indiceInterno++;
                                            Newemail.Indice = indiceInterno;
                                            correoAgregado = true;
                                            await NegocioAgente.InsertarEmailAsync(Newemail);
                                        }
                                        if (fechaMasReciente == null || Newemail.FechaCorreo > fechaMasReciente)
                                            fechaMasReciente = Newemail.FechaCorreo;
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
                                        // _CorreoProcesados = numeroCorreo;
                                        procesados++;
                                        _CorreoProcesados = procesados;
                                        if (sw.ElapsedMilliseconds - ultimaActualizacion >= 250)
                                        {
                                            ultimaActualizacion = sw.ElapsedMilliseconds;
                                            int progreso = (int)((procesados * 100.0) / total);
                                            if (progreso > 100) progreso = 100;
                                            int indiceMostrado = indiceInterno;
                                            int procesadosMostrados = procesados;
                                            ActualizarUI(delegate
                                            {
                                                BarraProgreso.Value = progreso;
                                                TxtCorreosProcesados.Text = procesadosMostrados.ToString();
                                                TxtNumeroCorreo.Text = indiceMostrado + " / " + total;
                                            });
                                            AddLogEntry(oMail.Subject);
                                        }
                                        //BackgroundWorker.ReportProgress(((numeroCorreo + 1 * 100) / total), oMail);
                                    }
                                }
                                break;
                            }
                            catch (Exception ex)
                            {
                                if (ex is OperationCanceledException)
                                {
                                    abortarCuenta = true;
                                    break;
                                }

                                await RegistrarError(ex, cuenta, oServer);

                                if (EsErrorDeTransporte(ex) && !reintentado && ReconectarCliente(oClient, oServer))
                                {
                                    reintentado = true;
                                    continue;
                                }

                                abortarCuenta = true;
                                break;
                            }
                            }
                            if (abortarCuenta)
                                break;
                        }

                        if (!abortarCuenta && !token.IsCancellationRequested && procesados > 0)
                        {
                            int procesadosFinal = procesados;
                            ActualizarUI(delegate
                            {
                                BarraProgreso.Value = 100;
                                TxtCorreosProcesados.Text = procesadosFinal.ToString();
                            });
                        }

                        if (fechaMasReciente != null)
                        {
                            cuenta.FechaUltimoRegistro = fechaMasReciente.Value;
                        }

                        // Quit and expunge emails marked as deleted from server.
                        CerrarCliente(oClient);
                        Console.WriteLine("Completed!");
                    }
                    catch (Exception ex)
                    {
                        CerrarCliente(oClient);
                        await RegistrarError(ex, cuenta, oServer);
                    }
                }
            }
            catch (Exception ex)
            {
                CerrarCliente(oClient);
                string usuarioError = cuenta?.UserName ?? "Desconocido";
                string servidorError = oServer?.Server ?? "Desconocido";
                await SendErrorToAPI(ex, usuarioError, servidorError);

                await saveLOGAsync("Admin", DateTime.Now.ToString("ddMMyyyy"), ex.Message.ToString()+"_"+ ex.StackTrace + "_" + ex.ToString());
                ActualizarUI(delegate
                {
                    BtnBajarCorreos.Enabled = true;
                    LblMensaje.Text = "Error: " + ex.Message.ToString();
                });
                Int32 segundosSleepErrorEmail = Convert.ToInt32(ConfigurationManager.AppSettings["SegundosSleepErrorEmail"]) * 1000;
                await Task.Delay(segundosSleepErrorEmail);
            }
            finally
            {
                _Temporizador = Convert.ToInt32(ConfigurationManager.AppSettings["TiempoTimerEmail"]);
                ActualizarUI(delegate
                {
                    Temporizador.Start();
                    LblMensaje.Text = "La Tarea fue Completada. Fecha Fin " + DateTime.Now + " ";
                    BtnBajarCorreos.Enabled = true;
                });
            }

            TimeSpan duration = DateTime.Now - start;
            //aquí podríamos devolver información de utilidad, como el resultado de un cálculo,
            //número de elementos afectados, etc.. de manera sencilla y segura
            //al hilo principal
            });
            }
            finally
            {
                _procesando = false;
                if (_ctsObtenerCorreos != null)
                {
                    _ctsObtenerCorreos.Dispose();
                    _ctsObtenerCorreos = null;
                }
            }
        }
        private static bool EsErrorDeTransporte(Exception ex)
        {
            if (ex is SocketException || ex is MailServerException)
                return true;
            return ex is IOException && ex.InnerException is SocketException;
        }

        private bool ReconectarCliente(MailClient oClient, MailServer oServer)
        {
            try
            {
                CerrarCliente(oClient);
                oClient.Connect(oServer);
                saveLOG("ReconectarCliente", DateTime.Now.ToString("ddMMyyyy"), "Reconexion exitosa con " + oServer.Server);
                return true;
            }
            catch (Exception ex)
            {
                saveLOG("ReconectarCliente", DateTime.Now.ToString("ddMMyyyy"), ex.ToString());
                return false;
            }
        }

        private void CerrarCliente(MailClient oClient)
        {
            if (oClient == null)
                return;
            try
            {
                oClient.Close();
            }
            catch
            {
            }
        }

        private static string CategorizarError(Exception ex)
        {
            if (EsErrorDeTransporte(ex))
                return "Transporte";
            string tipo = ex.GetType().FullName ?? "";
            if (tipo.StartsWith("MySql"))
                return "BD";
            if (ex is UnauthorizedAccessException || ex is DirectoryNotFoundException || ex is DriveNotFoundException || tipo.StartsWith("System.IO"))
                return "Archivos/Red";
            if (ex is WebException || tipo.StartsWith("Microsoft.Identity") || tipo.Contains("OAuth"))
                return "OAuth";
            return "Otro";
        }

        private void MostrarResumenErrores()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Errores: ");
            sb.Append(_erroresTotales);
            sb.Append(" (");
            bool primero = true;
            foreach (var item in _erroresPorCategoria.OrderByDescending(k => k.Value))
            {
                if (!primero)
                    sb.Append(" | ");
                sb.Append(item.Key);
                sb.Append(" ");
                sb.Append(item.Value);
                primero = false;
            }
            sb.Append(") - Ultimo: ");
            sb.Append(_ultimoError);
            string texto = sb.ToString();
            ActualizarUI(delegate { LblErrores.Text = texto; });
        }

        private async Task RegistrarError(Exception ex, CuentasEmail cuenta, MailServer oServer)
        {
            string usuario = cuenta?.UserName ?? "Desconocido";
            string servidor = oServer?.Server ?? "Desconocido";
            string categoria = CategorizarError(ex);

            _erroresTotales++;
            if (_erroresPorCategoria.ContainsKey(categoria))
                _erroresPorCategoria[categoria]++;
            else
                _erroresPorCategoria[categoria] = 1;
            _ultimoError = DateTime.Now.ToString("HH:mm:ss") + " " + usuario + ": " + ex.Message;
            MostrarResumenErrores();

            await SendErrorToAPI(ex, usuario, servidor);
            await saveLOGAsync("Admin-1", DateTime.Now.ToString("ddMMyyyy"), categoria + " | " + usuario + "@" + servidor + ": " + ex.ToString());
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
        private async Task SendErrorToAPI(Exception ex, string cuentaCorreo, string servidor, string sitio = "ObtenerCorreos")
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var payload = new
                    {
                        usuario = cuentaCorreo ?? "Desconocido",
                        error = ex.Message + " | " + ex.StackTrace + " | " + (servidor ?? "Desconocido"),
                        accion = "Error en la tarea MOTOR DE CORREOS",
                        sitio = sitio,
                        ip = Environment.MachineName
                    };

                    var response = await client.PostAsJsonAsync(Program.ObtenerUrlApiErrores(), payload);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Error enviando log a la API: {response.StatusCode}");
                    }
                }
            }
            catch (Exception apiEx)
            {
                // Evitar que falle el envío de log
                Console.WriteLine($"Error en SendErrorToAPI: {apiEx.Message}");
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


