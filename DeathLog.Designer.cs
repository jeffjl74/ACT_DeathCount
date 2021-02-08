namespace DeathCount_Plugin
{
    partial class DeathLog
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.At = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Attacker = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.AttackType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Damage = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.DamageType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.At,
            this.Attacker,
            this.AttackType,
            this.Damage,
            this.DamageType});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(410, 80);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // At
            // 
            this.At.Text = "At";
            // 
            // Attacker
            // 
            this.Attacker.Text = "Attacker";
            // 
            // AttackType
            // 
            this.AttackType.Text = "Attack Type";
            this.AttackType.Width = 83;
            // 
            // Damage
            // 
            this.Damage.Text = "Damage";
            // 
            // DamageType
            // 
            this.DamageType.Text = "Damage Type";
            this.DamageType.Width = 98;
            // 
            // DeathLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(410, 80);
            this.Controls.Add(this.listView1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(16, 69);
            this.Name = "DeathLog";
            this.ShowIcon = false;
            this.Text = "DeathLog";
            this.Load += new System.EventHandler(this.DeathLog_Load);
            this.ResizeEnd += new System.EventHandler(this.DeathLog_ResizeEnd);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader Attacker;
        private System.Windows.Forms.ColumnHeader AttackType;
        private System.Windows.Forms.ColumnHeader Damage;
        private System.Windows.Forms.ColumnHeader DamageType;
        private System.Windows.Forms.ColumnHeader At;
    }
}