using Advanced_Combat_Tracker;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DeathCount_Plugin
{
    /// <summary>
    /// Form to show the log lines immediately preceeding a death.
    /// </summary>
    public partial class DeathLog : Form
    {
        WhoDied whoDied;
        EncounterData ed;
        Rectangle parentRect;

        /// <summary>
        /// Form constructor
        /// </summary>
        /// <param name="who">Decriptive class for who died</param>
        /// <param name="encounterData">ACT encounter data that includes the death</param>
        /// <param name="rect">Rectangle of the parent form. This form will be placed to its right</param>
        public DeathLog(WhoDied who, EncounterData encounterData, Rectangle rect)
        {
            InitializeComponent();

            whoDied = who;
            ed = encounterData;
            parentRect = rect;
        }

        /// <summary>
        /// Set the initial size and position
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeathLog_Load(object sender, EventArgs e)
        {
            PopulateListview(4);

            Point pt = parentRect.Location;
            pt.X += parentRect.Width + 5;
            Location = pt;
            EnsureVisible(this);
            this.TopMost = true;
        }

        /// <summary>
        /// Adjusts the form, if necessary, so that it is completely on-screen
        /// </summary>
        /// <param name="ctrl"></param>
        private void EnsureVisible(Control ctrl)
        {
            Rectangle ctrlRect = ctrl.DisplayRectangle; //The dimensions of the ctrl
            ctrlRect.Y = ctrl.Top; //Add in the real Top and Left Vals
            ctrlRect.X = ctrl.Left;
            Rectangle screenRect = Screen.GetWorkingArea(ctrl); //The Working Area for the screen showing most of the Ctrl

            //Now tweak the ctrl's Top and Left until it's fully visible. 
            ctrl.Left += Math.Min(0, screenRect.Left + screenRect.Width - ctrl.Left - ctrl.Width);
            ctrl.Left -= Math.Min(0, ctrl.Left - screenRect.Left);
            ctrl.Top += Math.Min(0, screenRect.Top + screenRect.Height - ctrl.Top - ctrl.Height);
            ctrl.Top -= Math.Min(0, ctrl.Top - screenRect.Top);

        }

        /// <summary>
        /// Add items representing the log line up to and including the death
        /// </summary>
        /// <param name="lines">Displayed number of lines prior to the death</param>
        private void PopulateListview(int lines)
        {
            if (ed.Items.ContainsKey(whoDied.who.ToUpper()))
            {
                DamageTypeData dtd;
                Dictionary<string, DamageTypeData> dmg = ed.Items[whoDied.who.ToUpper()].Items;
                if (dmg != null)
                {
                    dmg.TryGetValue("Incoming Damage", out dtd);
                    int killedAt = whoDied.killedAtIndex;
                    if (dtd != null)
                    {
                        this.Text = whoDied.who + " died";
                        List<MasterSwing> swings = dtd.Items["All"].Items;
                        int start = killedAt - lines;
                        if (start < 0)
                            start = 0;
                        long killTicks = swings[killedAt].Time.Ticks;
                        for (int i = start; i <= killedAt; i++)
                        {
                            long itemTicks = killTicks - swings[i].Time.Ticks;
                            long sec = itemTicks / TimeSpan.TicksPerSecond;
                            ListViewItem item = new ListViewItem(string.Format("T-{0}", sec));
                            item.SubItems.Add(swings[i].Attacker);
                            item.SubItems.Add(swings[i].AttackType);
                            item.SubItems.Add(swings[i].Damage.ToString());
                            item.SubItems.Add(swings[i].DamageType);
                            listView1.Items.Add(item);
                        }

                        for (int i = 0; i < listView1.Columns.Count; i++)
                        {
                            listView1.Columns[i].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                        }

                        this.Width = listView1.GetItemRect(0).Width + SystemInformation.VerticalScrollBarWidth + SystemInformation.FrameBorderSize.Width;
                        int rows = listView1.Items.Count;
                        int height = (listView1.GetItemRect(0).Height * rows) + this.MinimumSize.Height;
                        this.Height = height;
                    }
                }
            }
        }

        /// <summary>
        /// When the form is resized, change the number of viewed lines to match the new size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeathLog_ResizeEnd(object sender, EventArgs e)
        {
            int rowHeight = listView1.GetItemRect(0).Height;
            listView1.Items.Clear();
            int lines = listView1.Height / rowHeight;
            PopulateListview(lines);
        }

    }
}
