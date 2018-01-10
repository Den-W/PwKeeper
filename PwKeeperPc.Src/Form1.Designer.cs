namespace PwKeeperPC
{
    partial class Form1
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tbPwd = new System.Windows.Forms.TextBox();
            this.pbPwd = new System.Windows.Forms.PictureBox();
            this.btDn = new System.Windows.Forms.Button();
            this.btUp = new System.Windows.Forms.Button();
            this.mLst = new System.Windows.Forms.ListView();
            this.cDisp = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cPass = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cValues = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btRecDn = new System.Windows.Forms.Button();
            this.tbV6 = new System.Windows.Forms.TextBox();
            this.btRecUp = new System.Windows.Forms.Button();
            this.btRecNew = new System.Windows.Forms.Button();
            this.tbV3 = new System.Windows.Forms.TextBox();
            this.btRecDel = new System.Windows.Forms.Button();
            this.tbTxPwd = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbV5 = new System.Windows.Forms.TextBox();
            this.tbDispName = new System.Windows.Forms.TextBox();
            this.tbV1 = new System.Windows.Forms.TextBox();
            this.tbTxName = new System.Windows.Forms.TextBox();
            this.tbV2 = new System.Windows.Forms.TextBox();
            this.tbV4 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btLoad = new System.Windows.Forms.Button();
            this.btSave = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btRead = new System.Windows.Forms.Button();
            this.btClear = new System.Windows.Forms.Button();
            this.btWrite = new System.Windows.Forms.Button();
            this.btStop = new System.Windows.Forms.Button();
            this.rtStatus = new System.Windows.Forms.RichTextBox();
            this.tmr1 = new System.Windows.Forms.Timer(this.components);
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.btAbout = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPwd)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.tbPwd);
            this.groupBox1.Controls.Add(this.pbPwd);
            this.groupBox1.Controls.Add(this.btDn);
            this.groupBox1.Controls.Add(this.btUp);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // tbPwd
            // 
            resources.ApplyResources(this.tbPwd, "tbPwd");
            this.tbPwd.Name = "tbPwd";
            this.tbPwd.TextChanged += new System.EventHandler(this.tbPwd_TextChanged);
            // 
            // pbPwd
            // 
            resources.ApplyResources(this.pbPwd, "pbPwd");
            this.pbPwd.Name = "pbPwd";
            this.pbPwd.TabStop = false;
            // 
            // btDn
            // 
            resources.ApplyResources(this.btDn, "btDn");
            this.btDn.Name = "btDn";
            this.btDn.UseVisualStyleBackColor = true;
            this.btDn.Click += new System.EventHandler(this.btDn_Click);
            // 
            // btUp
            // 
            resources.ApplyResources(this.btUp, "btUp");
            this.btUp.Name = "btUp";
            this.btUp.UseVisualStyleBackColor = true;
            this.btUp.Click += new System.EventHandler(this.btUp_Click);
            // 
            // mLst
            // 
            this.mLst.AllowDrop = true;
            resources.ApplyResources(this.mLst, "mLst");
            this.mLst.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.cDisp,
            this.cName,
            this.cPass,
            this.cValues});
            this.mLst.FullRowSelect = true;
            this.mLst.GridLines = true;
            this.mLst.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.mLst.MultiSelect = false;
            this.mLst.Name = "mLst";
            this.mLst.UseCompatibleStateImageBehavior = false;
            this.mLst.View = System.Windows.Forms.View.Details;
            this.mLst.SelectedIndexChanged += new System.EventHandler(this.mLst_SelectedIndexChanged);
            // 
            // cDisp
            // 
            resources.ApplyResources(this.cDisp, "cDisp");
            // 
            // cName
            // 
            resources.ApplyResources(this.cName, "cName");
            // 
            // cPass
            // 
            resources.ApplyResources(this.cPass, "cPass");
            // 
            // cValues
            // 
            resources.ApplyResources(this.cValues, "cValues");
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.btRecDn);
            this.groupBox2.Controls.Add(this.tbV6);
            this.groupBox2.Controls.Add(this.btRecUp);
            this.groupBox2.Controls.Add(this.btRecNew);
            this.groupBox2.Controls.Add(this.tbV3);
            this.groupBox2.Controls.Add(this.btRecDel);
            this.groupBox2.Controls.Add(this.tbTxPwd);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.tbV5);
            this.groupBox2.Controls.Add(this.tbDispName);
            this.groupBox2.Controls.Add(this.tbV1);
            this.groupBox2.Controls.Add(this.tbTxName);
            this.groupBox2.Controls.Add(this.tbV2);
            this.groupBox2.Controls.Add(this.tbV4);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // btRecDn
            // 
            resources.ApplyResources(this.btRecDn, "btRecDn");
            this.btRecDn.Name = "btRecDn";
            this.btRecDn.UseVisualStyleBackColor = true;
            this.btRecDn.Click += new System.EventHandler(this.btRecDn_Click);
            // 
            // tbV6
            // 
            resources.ApplyResources(this.tbV6, "tbV6");
            this.tbV6.Name = "tbV6";
            this.tbV6.TextChanged += new System.EventHandler(this.Rec_TextChanged);
            // 
            // btRecUp
            // 
            resources.ApplyResources(this.btRecUp, "btRecUp");
            this.btRecUp.Name = "btRecUp";
            this.btRecUp.UseVisualStyleBackColor = true;
            this.btRecUp.Click += new System.EventHandler(this.btRecUp_Click);
            // 
            // btRecNew
            // 
            resources.ApplyResources(this.btRecNew, "btRecNew");
            this.btRecNew.Name = "btRecNew";
            this.btRecNew.UseVisualStyleBackColor = true;
            this.btRecNew.Click += new System.EventHandler(this.btRecNew_Click);
            // 
            // tbV3
            // 
            resources.ApplyResources(this.tbV3, "tbV3");
            this.tbV3.Name = "tbV3";
            this.tbV3.TextChanged += new System.EventHandler(this.Rec_TextChanged);
            // 
            // btRecDel
            // 
            resources.ApplyResources(this.btRecDel, "btRecDel");
            this.btRecDel.Name = "btRecDel";
            this.btRecDel.UseVisualStyleBackColor = true;
            this.btRecDel.Click += new System.EventHandler(this.btRecDel_Click);
            // 
            // tbTxPwd
            // 
            resources.ApplyResources(this.tbTxPwd, "tbTxPwd");
            this.tbTxPwd.Name = "tbTxPwd";
            this.tbTxPwd.TextChanged += new System.EventHandler(this.Rec_TextChanged);
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // tbV5
            // 
            resources.ApplyResources(this.tbV5, "tbV5");
            this.tbV5.Name = "tbV5";
            this.tbV5.TextChanged += new System.EventHandler(this.Rec_TextChanged);
            // 
            // tbDispName
            // 
            resources.ApplyResources(this.tbDispName, "tbDispName");
            this.tbDispName.Name = "tbDispName";
            this.tbDispName.TextChanged += new System.EventHandler(this.Rec_TextChanged);
            // 
            // tbV1
            // 
            resources.ApplyResources(this.tbV1, "tbV1");
            this.tbV1.Name = "tbV1";
            this.tbV1.TextChanged += new System.EventHandler(this.Rec_TextChanged);
            // 
            // tbTxName
            // 
            resources.ApplyResources(this.tbTxName, "tbTxName");
            this.tbTxName.Name = "tbTxName";
            this.tbTxName.TextChanged += new System.EventHandler(this.Rec_TextChanged);
            // 
            // tbV2
            // 
            resources.ApplyResources(this.tbV2, "tbV2");
            this.tbV2.Name = "tbV2";
            this.tbV2.TextChanged += new System.EventHandler(this.Rec_TextChanged);
            // 
            // tbV4
            // 
            resources.ApplyResources(this.tbV4, "tbV4");
            this.tbV4.Name = "tbV4";
            this.tbV4.TextChanged += new System.EventHandler(this.Rec_TextChanged);
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // btLoad
            // 
            resources.ApplyResources(this.btLoad, "btLoad");
            this.btLoad.Name = "btLoad";
            this.btLoad.UseVisualStyleBackColor = true;
            this.btLoad.Click += new System.EventHandler(this.btLoad_Click);
            // 
            // btSave
            // 
            resources.ApplyResources(this.btSave, "btSave");
            this.btSave.Name = "btSave";
            this.btSave.UseVisualStyleBackColor = true;
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btLoad);
            this.groupBox3.Controls.Add(this.btSave);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.btRead);
            this.groupBox4.Controls.Add(this.btClear);
            this.groupBox4.Controls.Add(this.btWrite);
            this.groupBox4.Controls.Add(this.btStop);
            resources.ApplyResources(this.groupBox4, "groupBox4");
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.TabStop = false;
            // 
            // btRead
            // 
            resources.ApplyResources(this.btRead, "btRead");
            this.btRead.Name = "btRead";
            this.btRead.UseVisualStyleBackColor = true;
            this.btRead.Click += new System.EventHandler(this.btRead_Click);
            // 
            // btClear
            // 
            resources.ApplyResources(this.btClear, "btClear");
            this.btClear.BackColor = System.Drawing.Color.OrangeRed;
            this.btClear.ForeColor = System.Drawing.Color.White;
            this.btClear.Name = "btClear";
            this.btClear.UseVisualStyleBackColor = false;
            this.btClear.Click += new System.EventHandler(this.btClear_Click);
            // 
            // btWrite
            // 
            resources.ApplyResources(this.btWrite, "btWrite");
            this.btWrite.Name = "btWrite";
            this.btWrite.UseVisualStyleBackColor = true;
            this.btWrite.Click += new System.EventHandler(this.btWrite_Click);
            // 
            // btStop
            // 
            resources.ApplyResources(this.btStop, "btStop");
            this.btStop.Name = "btStop";
            this.btStop.UseVisualStyleBackColor = true;
            this.btStop.Click += new System.EventHandler(this.btStop_Click);
            // 
            // rtStatus
            // 
            resources.ApplyResources(this.rtStatus, "rtStatus");
            this.rtStatus.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtStatus.Name = "rtStatus";
            this.rtStatus.ReadOnly = true;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "PWK";
            this.openFileDialog1.FileName = "PwKeeper";
            resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
            // 
            // saveFileDialog1
            // 
            resources.ApplyResources(this.saveFileDialog1, "saveFileDialog1");
            // 
            // groupBox5
            // 
            resources.ApplyResources(this.groupBox5, "groupBox5");
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.TabStop = false;
            // 
            // btAbout
            // 
            resources.ApplyResources(this.btAbout, "btAbout");
            this.btAbout.Name = "btAbout";
            this.btAbout.UseVisualStyleBackColor = true;
            this.btAbout.Click += new System.EventHandler(this.btAbout_Click);
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btAbout);
            this.Controls.Add(this.rtStatus);
            this.Controls.Add(this.mLst);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox5);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPwd)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox tbPwd;
        private System.Windows.Forms.PictureBox pbPwd;
        private System.Windows.Forms.Button btDn;
        private System.Windows.Forms.Button btUp;
        private System.Windows.Forms.ListView mLst;
        private System.Windows.Forms.ColumnHeader cDisp;
        private System.Windows.Forms.ColumnHeader cValues;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox tbTxPwd;
        private System.Windows.Forms.TextBox tbTxName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbDispName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbV3;
        private System.Windows.Forms.Button btRecDn;
        private System.Windows.Forms.Button btRecUp;
        private System.Windows.Forms.Button btRecDel;
        private System.Windows.Forms.Button btRecNew;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbV2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbV1;
        private System.Windows.Forms.Button btLoad;
        private System.Windows.Forms.Button btSave;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btClear;
        private System.Windows.Forms.Button btStop;
        private System.Windows.Forms.RichTextBox rtStatus;
        private System.Windows.Forms.Timer tmr1;
        private System.Windows.Forms.Button btRead;
        private System.Windows.Forms.Button btWrite;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ColumnHeader cName;
        private System.Windows.Forms.ColumnHeader cPass;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button btAbout;
        private System.Windows.Forms.TextBox tbV6;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbV5;
        private System.Windows.Forms.TextBox tbV4;
        private System.Windows.Forms.Label label7;
    }
}

