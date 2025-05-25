namespace CounterSplit
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            dataGridView1 = new DataGridView();
            lbl_igt = new Label();
            lbl_hitcount = new Label();
            lbl_lrt = new Label();
            lbl_currhp = new Label();
            lbl_totaldmg = new Label();
            label10 = new Label();
            lbl_status = new Label();
            lbl_date = new Label();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = SystemColors.Control;
            label1.Location = new Point(100, 9);
            label1.Name = "label1";
            label1.Size = new Size(95, 21);
            label1.TabIndex = 0;
            label1.Text = "God of War";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = SystemColors.Control;
            label2.Location = new Point(23, 30);
            label2.Name = "label2";
            label2.Size = new Size(251, 21);
            label2.TabIndex = 1;
            label2.Text = "Normal Glitchless - Damageless";
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.Location = new Point(12, 54);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ScrollBars = ScrollBars.None;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(284, 416);
            dataGridView1.TabIndex = 2;
            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
            // 
            // lbl_igt
            // 
            lbl_igt.AutoSize = true;
            lbl_igt.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbl_igt.ForeColor = Color.White;
            lbl_igt.Location = new Point(16, 510);
            lbl_igt.Name = "lbl_igt";
            lbl_igt.Size = new Size(130, 32);
            lbl_igt.TabIndex = 4;
            lbl_igt.Text = "HH:mm:ss";
            // 
            // lbl_hitcount
            // 
            lbl_hitcount.AutoSize = true;
            lbl_hitcount.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lbl_hitcount.ForeColor = Color.White;
            lbl_hitcount.Location = new Point(192, 560);
            lbl_hitcount.Name = "lbl_hitcount";
            lbl_hitcount.Size = new Size(173, 21);
            lbl_hitcount.TabIndex = 5;
            lbl_hitcount.Text = "Current Total Hits : 10";
            // 
            // lbl_lrt
            // 
            lbl_lrt.AutoSize = true;
            lbl_lrt.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbl_lrt.ForeColor = Color.LimeGreen;
            lbl_lrt.Location = new Point(12, 473);
            lbl_lrt.Name = "lbl_lrt";
            lbl_lrt.Size = new Size(174, 45);
            lbl_lrt.TabIndex = 7;
            lbl_lrt.Text = "HH:mm:ss";
            // 
            // lbl_currhp
            // 
            lbl_currhp.AutoSize = true;
            lbl_currhp.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lbl_currhp.ForeColor = Color.White;
            lbl_currhp.Location = new Point(192, 485);
            lbl_currhp.Name = "lbl_currhp";
            lbl_currhp.Size = new Size(123, 21);
            lbl_currhp.TabIndex = 9;
            lbl_currhp.Text = "Current HP : 10";
            // 
            // lbl_totaldmg
            // 
            lbl_totaldmg.AutoSize = true;
            lbl_totaldmg.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lbl_totaldmg.ForeColor = Color.White;
            lbl_totaldmg.Location = new Point(192, 510);
            lbl_totaldmg.Name = "lbl_totaldmg";
            lbl_totaldmg.Size = new Size(106, 21);
            lbl_totaldmg.TabIndex = 10;
            lbl_totaldmg.Text = "Hp Lost : 124";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label10.ForeColor = Color.White;
            label10.Location = new Point(192, 535);
            label10.Name = "label10";
            label10.Size = new Size(61, 21);
            label10.TabIndex = 11;
            label10.Text = "Best : ?";
            // 
            // lbl_status
            // 
            lbl_status.AutoSize = true;
            lbl_status.ForeColor = Color.White;
            lbl_status.Location = new Point(231, 761);
            lbl_status.Name = "lbl_status";
            lbl_status.Size = new Size(65, 15);
            lbl_status.TabIndex = 12;
            lbl_status.Text = "Connected";
            // 
            // lbl_date
            // 
            lbl_date.AutoSize = true;
            lbl_date.ForeColor = Color.White;
            lbl_date.Location = new Point(23, 566);
            lbl_date.Name = "lbl_date";
            lbl_date.Size = new Size(38, 15);
            lbl_date.TabIndex = 13;
            lbl_date.Text = "label3";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            BackgroundImageLayout = ImageLayout.None;
            ClientSize = new Size(308, 785);
            Controls.Add(lbl_date);
            Controls.Add(lbl_status);
            Controls.Add(label10);
            Controls.Add(lbl_totaldmg);
            Controls.Add(lbl_currhp);
            Controls.Add(lbl_lrt);
            Controls.Add(lbl_hitcount);
            Controls.Add(lbl_igt);
            Controls.Add(dataGridView1);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.None;
            Name = "MainForm";
            Text = "CounterSplit";
            Load += MainForm_Load;
            MouseDown += MainForm_MouseDown;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private DataGridView dataGridView1;
        private Label lbl_igt;
        private Label lbl_hitcount;
        private Label lbl_lrt;
        private Label lbl_currhp;
        private Label lbl_totaldmg;
        private Label label10;
        private Label lbl_status;
        private Label lbl_date;
    }
}
