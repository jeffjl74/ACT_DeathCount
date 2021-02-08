using System.ComponentModel;
using System.Windows.Forms;

namespace DeathCount_Plugin
{
    public partial class DeathCount
    {
        #region IDE CODE
        private TextBox textBoxZone;
        private DataGridView dataGridView1;
        private RichTextBox richTextBox1;
        private ListBox listBoxDeaths;
        private Panel panel1;
        private Panel panel2;
        private Label label2;
        private Label labelWhoDied;
        private Panel panel4;
        private CheckBox checkBoxZoneOnly;
        private Panel panel5;
        private Label label5;
        private TextBox textBoxDelay;
        private CheckBox checkBoxWarnings;
        private IContainer components;
        private TextBox textBoxEncounterZone;
        private Splitter splitter1;
        private Panel panel3;
        private Label label6;
        private Button buttonShowMini;
        private TextBox textBoxMultDelay;
        private Label label7;
        private Label label8;
        private GroupBox groupBox1;
        private ContextMenuStrip contextMenuStripXml;
        private ToolStripMenuItem copyXMLToolStripMenuItem;
        Label lblStatus;	// The status label that appears in ACT's Plugin tab

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.textBoxZone = new System.Windows.Forms.TextBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.contextMenuStripXml = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.listBoxDeaths = new System.Windows.Forms.ListBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.checkBoxZoneOnly = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.labelWhoDied = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.contextMenuStripMob = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToTrackedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.comboBoxMini = new System.Windows.Forms.ComboBox();
            this.buttonShowMini = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBoxAnnounce = new System.Windows.Forms.ComboBox();
            this.textBoxMultDelay = new System.Windows.Forms.TextBox();
            this.checkBoxWarnings = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxDelay = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxEncounterZone = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.contextMenuStripXml.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel5.SuspendLayout();
            this.contextMenuStripMob.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxZone
            // 
            this.textBoxZone.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxZone.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBoxZone.Location = new System.Drawing.Point(102, 248);
            this.textBoxZone.Name = "textBoxZone";
            this.textBoxZone.ReadOnly = true;
            this.textBoxZone.Size = new System.Drawing.Size(427, 20);
            this.textBoxZone.TabIndex = 3;
            this.textBoxZone.MouseHover += new System.EventHandler(this.textBoxZone_MouseHover);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.ContextMenuStrip = this.contextMenuStripXml;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(725, 66);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellContextMenuStripNeeded += new System.Windows.Forms.DataGridViewCellContextMenuStripNeededEventHandler(this.dataGridView1_CellContextMenuStripNeeded);
            this.dataGridView1.UserAddedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.dataGridView1_UserAddedRow);
            this.dataGridView1.UserDeletedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.dataGridView1_UserDeletedRow);
            this.dataGridView1.MouseHover += new System.EventHandler(this.dataGridView1_MouseHover);
            // 
            // contextMenuStripXml
            // 
            this.contextMenuStripXml.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyXMLToolStripMenuItem});
            this.contextMenuStripXml.Name = "contextMenuStrip1";
            this.contextMenuStripXml.Size = new System.Drawing.Size(130, 26);
            // 
            // copyXMLToolStripMenuItem
            // 
            this.copyXMLToolStripMenuItem.Name = "copyXMLToolStripMenuItem";
            this.copyXMLToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.copyXMLToolStripMenuItem.Text = "Copy XML";
            this.copyXMLToolStripMenuItem.Click += new System.EventHandler(this.copyXMLToolStripMenuItem_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(0, 0);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(725, 94);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "Hover the mouse over a control for help.";
            this.richTextBox1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.richTextBox1_MouseClick);
            this.richTextBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.richTextBox1_MouseMove);
            // 
            // listBoxDeaths
            // 
            this.listBoxDeaths.FormattingEnabled = true;
            this.listBoxDeaths.Location = new System.Drawing.Point(230, 20);
            this.listBoxDeaths.Name = "listBoxDeaths";
            this.listBoxDeaths.Size = new System.Drawing.Size(137, 186);
            this.listBoxDeaths.TabIndex = 9;
            this.listBoxDeaths.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBoxDeaths_MouseDoubleClick);
            this.listBoxDeaths.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listBoxDeaths_MouseDown);
            this.listBoxDeaths.MouseHover += new System.EventHandler(this.listBoxDeaths_MouseHover);
            // 
            // panel1
            // 
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.richTextBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 104);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(725, 94);
            this.panel1.TabIndex = 10;
            // 
            // panel2
            // 
            this.panel2.AutoScroll = true;
            this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel2.Controls.Add(this.dataGridView1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 28);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(725, 66);
            this.panel2.TabIndex = 11;
            // 
            // checkBoxZoneOnly
            // 
            this.checkBoxZoneOnly.AutoSize = true;
            this.checkBoxZoneOnly.Location = new System.Drawing.Point(113, 7);
            this.checkBoxZoneOnly.Name = "checkBoxZoneOnly";
            this.checkBoxZoneOnly.Size = new System.Drawing.Size(142, 17);
            this.checkBoxZoneOnly.TabIndex = 1;
            this.checkBoxZoneOnly.Text = "Show Current Zone Only";
            this.checkBoxZoneOnly.UseVisualStyleBackColor = true;
            this.checkBoxZoneOnly.CheckedChanged += new System.EventHandler(this.checkBoxZoneOnly_CheckedChanged);
            this.checkBoxZoneOnly.MouseHover += new System.EventHandler(this.checkBoxZoneOnly_MouseHover);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Tracked Names";
            // 
            // labelWhoDied
            // 
            this.labelWhoDied.AutoSize = true;
            this.labelWhoDied.Location = new System.Drawing.Point(227, 3);
            this.labelWhoDied.Name = "labelWhoDied";
            this.labelWhoDied.Size = new System.Drawing.Size(55, 13);
            this.labelWhoDied.TabIndex = 8;
            this.labelWhoDied.Text = "Who Died";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.panel2);
            this.panel4.Controls.Add(this.splitter1);
            this.panel4.Controls.Add(this.panel1);
            this.panel4.Controls.Add(this.panel3);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 277);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(725, 198);
            this.panel4.TabIndex = 16;
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Location = new System.Drawing.Point(0, 94);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(725, 10);
            this.splitter1.TabIndex = 1;
            this.splitter1.TabStop = false;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label2);
            this.panel3.Controls.Add(this.checkBoxZoneOnly);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(725, 28);
            this.panel3.TabIndex = 0;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(24, 252);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Current Zone:";
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.label1);
            this.panel5.Controls.Add(this.treeView1);
            this.panel5.Controls.Add(this.groupBox2);
            this.panel5.Controls.Add(this.groupBox1);
            this.panel5.Controls.Add(this.label8);
            this.panel5.Controls.Add(this.textBoxZone);
            this.panel5.Controls.Add(this.label6);
            this.panel5.Controls.Add(this.textBoxEncounterZone);
            this.panel5.Controls.Add(this.listBoxDeaths);
            this.panel5.Controls.Add(this.labelWhoDied);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(0, 0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(725, 277);
            this.panel5.TabIndex = 17;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 29;
            this.label1.Text = "Encounters";
            // 
            // treeView1
            // 
            this.treeView1.ContextMenuStrip = this.contextMenuStripMob;
            this.treeView1.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.treeView1.HideSelection = false;
            this.treeView1.Location = new System.Drawing.Point(12, 20);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(212, 183);
            this.treeView1.TabIndex = 28;
            this.treeView1.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView1_BeforeExpand);
            this.treeView1.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.treeView1_DrawNode);
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
            this.treeView1.MouseHover += new System.EventHandler(this.treeView1_MouseHover);
            // 
            // contextMenuStripMob
            // 
            this.contextMenuStripMob.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToTrackedToolStripMenuItem});
            this.contextMenuStripMob.Name = "contextMenuStripMob";
            this.contextMenuStripMob.Size = new System.Drawing.Size(155, 26);
            // 
            // addToTrackedToolStripMenuItem
            // 
            this.addToTrackedToolStripMenuItem.Name = "addToTrackedToolStripMenuItem";
            this.addToTrackedToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.addToTrackedToolStripMenuItem.Text = "Add To Tracked";
            this.addToTrackedToolStripMenuItem.Click += new System.EventHandler(this.addToTrackedToolStripMenuItem_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.comboBoxMini);
            this.groupBox2.Controls.Add(this.buttonShowMini);
            this.groupBox2.Location = new System.Drawing.Point(376, 133);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(154, 78);
            this.groupBox2.TabIndex = 27;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Mini Overlay Form";
            // 
            // comboBoxMini
            // 
            this.comboBoxMini.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMini.FormattingEnabled = true;
            this.comboBoxMini.Items.AddRange(new object[] {
            "Auto show all",
            "Auto show tracked",
            "Disable auto show"});
            this.comboBoxMini.Location = new System.Drawing.Point(6, 19);
            this.comboBoxMini.Name = "comboBoxMini";
            this.comboBoxMini.Size = new System.Drawing.Size(141, 21);
            this.comboBoxMini.TabIndex = 30;
            this.comboBoxMini.SelectedIndexChanged += new System.EventHandler(this.comboBoxMini_SelectedIndexChanged);
            this.comboBoxMini.MouseHover += new System.EventHandler(this.comboBoxMini_MouseHover);
            // 
            // buttonShowMini
            // 
            this.buttonShowMini.Location = new System.Drawing.Point(36, 46);
            this.buttonShowMini.Name = "buttonShowMini";
            this.buttonShowMini.Size = new System.Drawing.Size(75, 23);
            this.buttonShowMini.TabIndex = 12;
            this.buttonShowMini.Text = "Show Mini";
            this.buttonShowMini.UseVisualStyleBackColor = true;
            this.buttonShowMini.Click += new System.EventHandler(this.buttonShowMini_Click);
            this.buttonShowMini.MouseHover += new System.EventHandler(this.buttonShowMini_MouseHover);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.comboBoxAnnounce);
            this.groupBox1.Controls.Add(this.textBoxMultDelay);
            this.groupBox1.Controls.Add(this.checkBoxWarnings);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.textBoxDelay);
            this.groupBox1.Location = new System.Drawing.Point(376, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(153, 124);
            this.groupBox1.TabIndex = 25;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Announcements";
            // 
            // comboBoxAnnounce
            // 
            this.comboBoxAnnounce.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAnnounce.FormattingEnabled = true;
            this.comboBoxAnnounce.Items.AddRange(new object[] {
            "Announce All",
            "Announce Tracked",
            "No Announcements"});
            this.comboBoxAnnounce.Location = new System.Drawing.Point(9, 19);
            this.comboBoxAnnounce.Name = "comboBoxAnnounce";
            this.comboBoxAnnounce.Size = new System.Drawing.Size(138, 21);
            this.comboBoxAnnounce.TabIndex = 31;
            this.comboBoxAnnounce.SelectedIndexChanged += new System.EventHandler(this.comboBoxAnnounce_SelectedIndexChanged);
            this.comboBoxAnnounce.MouseHover += new System.EventHandler(this.comboBoxAnnounce_MouseHover);
            // 
            // textBoxMultDelay
            // 
            this.textBoxMultDelay.Location = new System.Drawing.Point(9, 72);
            this.textBoxMultDelay.Name = "textBoxMultDelay";
            this.textBoxMultDelay.Size = new System.Drawing.Size(38, 20);
            this.textBoxMultDelay.TabIndex = 23;
            this.textBoxMultDelay.Text = "2500";
            this.textBoxMultDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxMultDelay.TextChanged += new System.EventHandler(this.textBoxMultDelay_TextChanged);
            this.textBoxMultDelay.MouseHover += new System.EventHandler(this.textBoxMultDelay_MouseHover);
            // 
            // checkBoxWarnings
            // 
            this.checkBoxWarnings.AutoSize = true;
            this.checkBoxWarnings.Location = new System.Drawing.Point(9, 100);
            this.checkBoxWarnings.Name = "checkBoxWarnings";
            this.checkBoxWarnings.Size = new System.Drawing.Size(123, 17);
            this.checkBoxWarnings.TabIndex = 5;
            this.checkBoxWarnings.Text = "Announce Warnings";
            this.checkBoxWarnings.UseVisualStyleBackColor = true;
            this.checkBoxWarnings.CheckedChanged += new System.EventHandler(this.checkBoxWarnings_CheckedChanged);
            this.checkBoxWarnings.MouseHover += new System.EventHandler(this.checkBoxWarnings_MouseHover);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(51, 76);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(92, 13);
            this.label7.TabIndex = 22;
            this.label7.Text = "msec Mutiple Gap";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(51, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "msec Single Gap";
            // 
            // textBoxDelay
            // 
            this.textBoxDelay.Location = new System.Drawing.Point(9, 46);
            this.textBoxDelay.Name = "textBoxDelay";
            this.textBoxDelay.Size = new System.Drawing.Size(38, 20);
            this.textBoxDelay.TabIndex = 7;
            this.textBoxDelay.Text = "1500";
            this.textBoxDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.textBoxDelay.TextChanged += new System.EventHandler(this.textBoxDelay_TextChanged);
            this.textBoxDelay.MouseHover += new System.EventHandler(this.textBoxDelay_MouseHover);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(9, 227);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(87, 13);
            this.label8.TabIndex = 24;
            this.label8.Text = "Encounter Zone:";
            // 
            // textBoxEncounterZone
            // 
            this.textBoxEncounterZone.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxEncounterZone.Location = new System.Drawing.Point(102, 223);
            this.textBoxEncounterZone.Name = "textBoxEncounterZone";
            this.textBoxEncounterZone.ReadOnly = true;
            this.textBoxEncounterZone.Size = new System.Drawing.Size(428, 20);
            this.textBoxEncounterZone.TabIndex = 19;
            this.textBoxEncounterZone.MouseHover += new System.EventHandler(this.textBoxEncounterZone_MouseHover);
            // 
            // DeathCount
            // 
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel5);
            this.Name = "DeathCount";
            this.Size = new System.Drawing.Size(725, 475);
            this.MouseHover += new System.EventHandler(this.DeathCount_MouseHover);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.contextMenuStripXml.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.contextMenuStripMob.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }


        #endregion IDE CODE

        private GroupBox groupBox2;
        private TreeView treeView1;
        private Label label1;
        private ContextMenuStrip contextMenuStripMob;
        private ToolStripMenuItem addToTrackedToolStripMenuItem;
        private ComboBox comboBoxAnnounce;
        private ComboBox comboBoxMini;
    }
}