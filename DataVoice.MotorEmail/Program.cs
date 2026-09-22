using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http.Json;

namespace DataVoice.MotorEmail
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        //static void Main()
        //{
        //    Application.EnableVisualStyles();
        //    Application.SetCompatibleTextRenderingDefault(false);
        //    Application.Run(new MainForm());
        //}


        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Captura excepciones no manejadas en hilos de UI
            Application.ThreadException += async (sender, e) =>
            {
                await SendErrorGlobalToAPI(e.Exception);
            };

            // Captura excepciones en hilos de background (Task, ThreadPool, etc.)
            AppDomain.CurrentDomain.UnhandledException += async (sender, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    await SendErrorGlobalToAPI(ex);
            };

            Application.Run(new MainForm());
        }

        private static async Task SendErrorGlobalToAPI(Exception ex)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var payload = new
                    {
                        usuario = "Global",
                        error = ex.Message + " | " + ex.StackTrace,
                        accion = "Error global en MOTOR DE CORREOS",
                        sitio = "MainForm",
                        ip = Environment.MachineName
                    };

                    await client.PostAsJsonAsync("http://localhost:29139/api/ApiErrorNotifier", payload);
                }
            }
            catch
            {
                // Ignorar errores al enviar logs globales
            }
        }

    }
}
