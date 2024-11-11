namespace DataVoice.MotorEmail
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.BtnBajarCorreos = new System.Windows.Forms.Button();
            this.LstCorreos = new System.Windows.Forms.ListBox();
            this.TxtNumeroCorreo = new System.Windows.Forms.TextBox();
            this.LblNumeroCorreo = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.TxtCorreosProcesados = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Cuentas = new System.Windows.Forms.TreeView();
            this.BackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.BarraProgreso = new System.Windows.Forms.ProgressBar();
            this.BtnCancelar = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Temporizador = new System.Windows.Forms.Timer(this.components);
            this.label5 = new System.Windows.Forms.Label();
            this.LblTemporizador = new System.Windows.Forms.Label();
            this.LblMensaje = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // BtnBajarCorreos
            // 
            this.BtnBajarCorreos.Location = new System.Drawing.Point(713, 543);
            this.BtnBajarCorreos.Name = "BtnBajarCorreos";
            this.BtnBajarCorreos.Size = new System.Drawing.Size(208, 71);
            this.BtnBajarCorreos.TabIndex = 20;
            this.BtnBajarCorreos.Text = "Bajar Correo";
            this.BtnBajarCorreos.UseVisualStyleBackColor = true;
            this.BtnBajarCorreos.Click += new System.EventHandler(this._bRetrieveMessageList_Click);
            // 
            // LstCorreos
            // 
            this.LstCorreos.Font = new System.Drawing.Font("Arial Narrow", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LstCorreos.ForeColor = System.Drawing.SystemColors.Highlight;
            this.LstCorreos.FormattingEnabled = true;
            this.LstCorreos.ItemHeight = 15;
            this.LstCorreos.Location = new System.Drawing.Point(260, 123);
            this.LstCorreos.Name = "LstCorreos";
            this.LstCorreos.Size = new System.Drawing.Size(661, 379);
            this.LstCorreos.TabIndex = 21;
            // 
            // TxtNumeroCorreo
            // 
            this.TxtNumeroCorreo.Location = new System.Drawing.Point(228, 9);
            this.TxtNumeroCorreo.Name = "TxtNumeroCorreo";
            this.TxtNumeroCorreo.ReadOnly = true;
            this.TxtNumeroCorreo.Size = new System.Drawing.Size(64, 20);
            this.TxtNumeroCorreo.TabIndex = 22;
            // 
            // LblNumeroCorreo
            // 
            this.LblNumeroCorreo.AutoSize = true;
            this.LblNumeroCorreo.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblNumeroCorreo.Location = new System.Drawing.Point(91, 10);
            this.LblNumeroCorreo.Name = "LblNumeroCorreo";
            this.LblNumeroCorreo.Size = new System.Drawing.Size(131, 16);
            this.LblNumeroCorreo.TabIndex = 23;
            this.LblNumeroCorreo.Text = "Número de Correos";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.TxtCorreosProcesados);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.TxtNumeroCorreo);
            this.panel1.Controls.Add(this.LblNumeroCorreo);
            this.panel1.Location = new System.Drawing.Point(625, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(296, 64);
            this.panel1.TabIndex = 24;
            // 
            // TxtCorreosProcesados
            // 
            this.TxtCorreosProcesados.Location = new System.Drawing.Point(227, 37);
            this.TxtCorreosProcesados.Name = "TxtCorreosProcesados";
            this.TxtCorreosProcesados.ReadOnly = true;
            this.TxtCorreosProcesados.Size = new System.Drawing.Size(64, 20);
            this.TxtCorreosProcesados.TabIndex = 24;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(91, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(134, 16);
            this.label2.TabIndex = 25;
            this.label2.Text = "Correos Procesados";
            // 
            // Cuentas
            // 
            this.Cuentas.CheckBoxes = true;
            this.Cuentas.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Cuentas.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.Cuentas.Location = new System.Drawing.Point(11, 123);
            this.Cuentas.Name = "Cuentas";
            this.Cuentas.Size = new System.Drawing.Size(243, 381);
            this.Cuentas.TabIndex = 25;
            this.Cuentas.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.Cuentas_AfterSelect);
            // 
            // BackgroundWorker
            // 
            this.BackgroundWorker.WorkerReportsProgress = true;
            this.BackgroundWorker.WorkerSupportsCancellation = true;
            this.BackgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.BackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // BarraProgreso
            // 
            this.BarraProgreso.Location = new System.Drawing.Point(260, 510);
            this.BarraProgreso.Name = "BarraProgreso";
            this.BarraProgreso.Size = new System.Drawing.Size(661, 23);
            this.BarraProgreso.TabIndex = 28;
            // 
            // BtnCancelar
            // 
            this.BtnCancelar.Location = new System.Drawing.Point(460, 543);
            this.BtnCancelar.Name = "BtnCancelar";
            this.BtnCancelar.Size = new System.Drawing.Size(208, 71);
            this.BtnCancelar.TabIndex = 29;
            this.BtnCancelar.Text = "Cancelar";
            this.BtnCancelar.UseVisualStyleBackColor = true;
            this.BtnCancelar.Click += new System.EventHandler(this.BtnCancelar_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 50);
            this.pictureBox1.TabIndex = 36;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(256, 101);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 19);
            this.label1.TabIndex = 26;
            this.label1.Text = "Correos:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(7, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(79, 19);
            this.label3.TabIndex = 31;
            this.label3.Text = "Cuentas:";
            // 
            // Temporizador
            // 
            this.Temporizador.Interval = 1000;
            this.Temporizador.Tick += new System.EventHandler(this.Temporizador_Tick);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.label5.Location = new System.Drawing.Point(8, 555);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(155, 16);
            this.label5.TabIndex = 34;
            this.label5.Text = "El Proceso Iniciara en :";
            // 
            // LblTemporizador
            // 
            this.LblTemporizador.AutoSize = true;
            this.LblTemporizador.Font = new System.Drawing.Font("Arial Black", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblTemporizador.ForeColor = System.Drawing.SystemColors.Highlight;
            this.LblTemporizador.Location = new System.Drawing.Point(169, 555);
            this.LblTemporizador.Name = "LblTemporizador";
            this.LblTemporizador.Size = new System.Drawing.Size(0, 22);
            this.LblTemporizador.TabIndex = 33;
            // 
            // LblMensaje
            // 
            this.LblMensaje.AutoSize = true;
            this.LblMensaje.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblMensaje.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.LblMensaje.Location = new System.Drawing.Point(9, 585);
            this.LblMensaje.Name = "LblMensaje";
            this.LblMensaje.Size = new System.Drawing.Size(140, 16);
            this.LblMensaje.TabIndex = 35;
            this.LblMensaje.Text = "Mensaje del Sistema";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 626);
            this.Controls.Add(this.LblMensaje);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.LblTemporizador);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.BtnCancelar);
            this.Controls.Add(this.BarraProgreso);
            this.Controls.Add(this.Cuentas);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.LstCorreos);
            this.Controls.Add(this.BtnBajarCorreos);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Motor Email V2.0";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnBajarCorreos;
        private System.Windows.Forms.ListBox LstCorreos;
        private System.Windows.Forms.TextBox TxtNumeroCorreo;
        private System.Windows.Forms.Label LblNumeroCorreo;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TreeView Cuentas;
        private System.ComponentModel.BackgroundWorker BackgroundWorker;
        private System.Windows.Forms.ProgressBar BarraProgreso;
        private System.Windows.Forms.Button BtnCancelar;
        private System.Windows.Forms.TextBox TxtCorreosProcesados;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Timer Temporizador;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label LblTemporizador;
        private System.Windows.Forms.Label LblMensaje;
    }
}

