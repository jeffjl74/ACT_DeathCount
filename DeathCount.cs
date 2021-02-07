using Advanced_Combat_Tracker;
using DeathCount_Plugin;
using Overlay;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

[assembly: AssemblyDescription("ACT plugin that monitors deaths during an encounter")]
[assembly: AssemblyCompany("Mineeme of Maj'Dul")]
[assembly: AssemblyProduct("DeathCount")]
[assembly: AssemblyVersion("1.0.0.0")]


namespace ACT_Plugin
{
    public partial class DeathCount : UserControl, IActPluginV1
    {
        //log line matching
        const string logTimeStampRegexStr = @"^\(\d{10}\)\[.{24}\] ";
        Regex rxDied = new Regex(@" has killed (?<died>\w+)\.$", RegexOptions.Compiled);
        Regex rxAddMob = new Regex(logTimeStampRegexStr + @"You say, ""track deaths for (?<mob>([^ 0-9]+)( [^ 0-9]+)*)\s*(?<max>\d+)?\s*(?<warn>\d+)?\s*""", RegexOptions.Compiled);
        Match match;

        //deaths
        int iDeathCount = 0;
        string strZone = "";
        string strongestEnemy = string.Empty;
        List<string> combatAllies = new List<string>();
        List<string> zoneMobs = new List<string>();
        Dictionary<string, ZoneMob> ZoneMobs = new Dictionary<string, ZoneMob>();
        EncounterData lastEncounter;
        EncounterRecord encounterData;
        bool bTrackMob = false;
        int iWarnLevel = -1;
        bool bTimerExpired = true;
        bool bManyDeaths = false;
        bool bWaitingMultiTimer = false;
        System.Timers.Timer timerAnnounce;
        System.Timers.Timer timerMultDelay;
        System.Timers.Timer timerCharChange;

        //context menu
        int contextMenuRow = -1;

        //help support
        string hyperLinkText = "";
        bool mouseOnHotText = false;
        private Cursor defaultRichTextBoxCursor = Cursors.Default;

        //debug
        //SwTimer swDict = new SwTimer();
        int totalDeaths = 0;
        int displayedDeaths = 0;

        //cross-thread support
        private Object listLock = new Object();
        ConcurrentQueue<WhoDied> queueDied;
        AutoResetEvent diedSignal = new AutoResetEvent(false);
        WindowsFormsSynchronizationContext mUiContext = new WindowsFormsSynchronizationContext();
        bool runConsumer = true;
        bool bDisplayCurrentZoneOnlyState = false;
        bool bAnnounceWarningState = false;
        int delayAnnounce = 1500;           //needs to match the default in the form designer
        int delayMultiDeath = 2500;         //needs to match the default in the form designer

        bool bImporting = false;

        //mini form support
        FormOverlay overlay;
        bool bMiniIsVisible = false;

        //persistent settings
        string settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\DeathCount.config.xml");
        DataSet ds = new DataSet("DeathCount");
        DataTable dtMobs = new DataTable("Mobs");
        DataTable dtSettings = new DataTable("Settings");
        DataTable dtPlayers = new DataTable("Players");
        string currentPlayer = string.Empty;
        enum ComboIndex { All = 0, Tracked, None}
        ComboIndex announceState = ComboIndex.All;  //needs to match default setting in LoadSettings()
        ComboIndex showState = ComboIndex.All;      //needs to match default setting in LoadSettings()

        public DeathCount()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            InitializeComponent();

            Consumer();
        }

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            //Control.CheckForIllegalCrossThreadCalls = true;  //set true to debug for cross-thread calls

            lblStatus = pluginStatusText;	            // Hand the status label's reference to our local var
            pluginScreenSpace.Controls.Add(this);	    // Add this UserControl to the tab ACT provides
            this.Dock = DockStyle.Fill;                 // Expand the UserControl to fill the tab's client space

            queueDied = new ConcurrentQueue<WhoDied>();

            //keep the cursor so it can be restored after changing to the hyperlink cursor
            defaultRichTextBoxCursor = richTextBox1.Cursor;

            //buttonDebug.Visible = true; //debug

            //build the settings data tables
            dtSettings.Columns.Add("checkBoxZoneOnly", typeof(bool));
            dtSettings.Columns.Add("checkBoxWarnings", typeof(bool));
            dtSettings.Columns.Add("textBoxDelay", typeof(string));
            dtSettings.Columns.Add("textBoxMultDelay", typeof(string));
            dtSettings.Columns.Add("comboAnnounce", typeof(int));
            dtSettings.Columns.Add("comboShow", typeof(int));
            //overlay location per player
            dtPlayers.Columns.Add("player", typeof(string));
            dtPlayers.Columns.Add("formMiniDeathsLocation", typeof(Point));
            dtPlayers.Columns.Add("formMiniDeathsSize", typeof(Size));
            dtPlayers.PrimaryKey = new DataColumn[] { dtPlayers.Columns["player"] };
            
            //build the mobs data table
            dtMobs.Columns.Add("Mob", typeof(string));
            dtMobs.Columns.Add("Zone", typeof(string));
            dtMobs.Columns.Add("MaxDeaths", typeof(int));
            dtMobs.Columns.Add("WarningLevel", typeof(int));
            dtMobs.CaseSensitive = false;
            DataColumn[] pk = new DataColumn[2];
            pk[0] = dtMobs.Columns["Mob"];
            pk[1] = dtMobs.Columns["Zone"];
            dtMobs.PrimaryKey = pk;

            //for serialization
            ds.Tables.Add(dtSettings);
            ds.Tables.Add(dtPlayers);
            ds.Tables.Add(dtMobs);

            //attach mobs table to the view
            dataGridView1.DataSource = ds.Tables["Mobs"];
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.Columns["MaxDeaths"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView1.Columns["WarningLevel"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            //Mini Form
            overlay = new FormOverlay();
            overlay.RemoveItemEvent += Overlay_RemoveItemEvent;
            overlay.UserHidesFormEvent += Overlay_UserHidesFormEvent;

            //annoucement timer
            timerAnnounce = new System.Timers.Timer();
            timerAnnounce.SynchronizingObject = this;
            timerAnnounce.Elapsed += TimerAnnounce_Elapsed;
            timerAnnounce.AutoReset = false;

            //multiple deaths timer
            timerMultDelay = new System.Timers.Timer();
            timerMultDelay.SynchronizingObject = this;
            timerMultDelay.Elapsed += TimerMultDelay_Elapsed;
            timerMultDelay.AutoReset = false;

            LoadSettings();

            if (delayAnnounce > 0)
            {
                timerAnnounce.Interval = delayAnnounce;
                timerAnnounce.Enabled = true;
            }

            if (delayMultiDeath > 0)
            {
                timerMultDelay.Interval = delayMultiDeath;
                timerMultDelay.Enabled = true;
            }

            //watch for zone and player name change
            timerCharChange = new System.Timers.Timer();
            timerCharChange.SynchronizingObject = this;
            timerCharChange.AutoReset = true;
            timerCharChange.Interval = 1000;
            timerCharChange.Elapsed += TimerCharChange_Elapsed;
            timerCharChange.Start();

            ActGlobals.oFormActMain.OnCombatStart += oFormActMain_OnCombatStart;
            ActGlobals.oFormActMain.OnCombatEnd += oFormActMain_OnCombatEnd;
            ActGlobals.oFormActMain.OnLogLineRead += oFormActMain_OnLogLineRead;
            ActGlobals.oFormActMain.LogFileChanged += oFormActMain_LogFileChanged;
            ActGlobals.oFormActMain.AfterCombatAction += oFormActMain_AfterCombatAction;
            ActGlobals.oFormActMain.XmlSnippetAdded += oFormActMain_XmlSnippetAdded;
            // If ACT is set to automatically check for updates, check for updates to the plugin
            if (ActGlobals.oFormActMain.GetAutomaticUpdatesAllowed())
                new Thread(new ThreadStart(oFormActMain_UpdateCheckClicked)).Start();	// If we don't put this on a separate thread, web latency will delay the plugin init phase

            lblStatus.Text = "Plugin started";
        }

        public void DeInitPlugin()
        {
            SaveSettings();

            runConsumer = false;
            diedSignal.Set();

            overlay.Close();

            ActGlobals.oFormActMain.OnCombatStart -= oFormActMain_OnCombatStart;
            ActGlobals.oFormActMain.OnCombatEnd -= oFormActMain_OnCombatEnd;
            ActGlobals.oFormActMain.OnLogLineRead -= oFormActMain_OnLogLineRead;
            ActGlobals.oFormActMain.LogFileChanged -= oFormActMain_LogFileChanged;
            ActGlobals.oFormActMain.XmlSnippetAdded -= oFormActMain_XmlSnippetAdded;
            ActGlobals.oFormActMain.UpdateCheckClicked -= oFormActMain_UpdateCheckClicked;

            lblStatus.Text = "Plugin Exited";
        }

        /// <summary>
        /// Watch for zone and character change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerCharChange_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(ActGlobals.charName != "YOU" && ActGlobals.charName != currentPlayer)
            {
                SaveOverlayLoc(); //save location for the previous player

                //look for the new player's location
                currentPlayer = ActGlobals.charName;
                DataRow found = ds.Tables["Players"].Rows.Find(currentPlayer);
                if(found != null)
                {
                    overlay.SetLocation((Size)found["formMiniDeathsSize"], (Point)found["formMiniDeathsLocation"]);
                }
            }

            if (!string.IsNullOrEmpty(ActGlobals.oFormActMain.CurrentZone) && ActGlobals.oFormActMain.CurrentZone != strZone)
            {
                strZone = ActGlobals.oFormActMain.CurrentZone;
                ThreadInvokes.ControlSetText(ActGlobals.oFormActMain, textBoxZone, strZone);
                if (bDisplayCurrentZoneOnlyState)
                {
                    mUiContext.Post(UpdateMobsForZone, null);
                }
                RebuildZoneMobs();
            }

        }

        /// <summary>
        /// Load the overlay form dll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string name = new AssemblyName(args.Name).Name;
            if (name.ToLower().Contains("overlay"))
            {
                //look for the dll in our resources
                string[] res = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                List<string> list = new List<string>(res);
                String resourceName = name + ".dll";
                string match = list.Find(s => s.Contains(name));
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(match))
                {
                    if (stream != null)
                    {
                        Byte[] assemblyData = new Byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);
                        Assembly assy = Assembly.Load(assemblyData);
                        return assy;
                    }
                }

                //fallback - look for the dll file in the plugin directory
                string path = ActGlobals.oFormActMain.PluginGetSelfData(this).pluginFile.DirectoryName;
                string dll = Path.Combine(path, name + ".dll");
                if (File.Exists(dll))
                {
                    Assembly assy = Assembly.Load(File.ReadAllBytes(dll));
                    return assy;
                }
            }
            return null;
        }

        /// <summary>
        /// Check for whether to update the plugin
        /// </summary>
        void oFormActMain_UpdateCheckClicked()
        {
            
            int pluginId = 82;
            try
            {
                Version localVersion = this.GetType().Assembly.GetName().Version;
                Version remoteVersion = new Version(ActGlobals.oFormActMain.PluginGetRemoteVersion(pluginId).TrimStart(new char[] { 'v' }));    // Strip any leading 'v' from the string before passing to the Version constructor
                if (remoteVersion > localVersion)
                {
                    DialogResult result = MessageBox.Show("There is an updated version of the Death Count Plugin.  Update it now?\n\n(If there is an update to ACT, you should click No and update ACT first.)", 
                        "New Version", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        FileInfo updatedFile = ActGlobals.oFormActMain.PluginDownload(pluginId);
                        ActPluginData pluginData = ActGlobals.oFormActMain.PluginGetSelfData(this);
                        pluginData.pluginFile.Delete();
                        updatedFile.MoveTo(pluginData.pluginFile.FullName);
                        ThreadInvokes.CheckboxSetChecked(ActGlobals.oFormActMain, pluginData.cbEnabled, false);
                        Application.DoEvents();
                        ThreadInvokes.CheckboxSetChecked(ActGlobals.oFormActMain, pluginData.cbEnabled, true);
                    }
                }
            }
            catch (Exception ex)
            {
                ActGlobals.oFormActMain.WriteExceptionLog(ex, "DeathCount Plugin Update Check");
            }
        }

        /// <summary>
        /// Load persistant settings
        /// </summary>
        void LoadSettings()
        {
            if (File.Exists(settingsFile))
            {
                ds.ReadXml(settingsFile);
                if (ds.Tables["players"].Rows.Count == 0)
                {
                    //add the default overlay location
                    ds.Tables["Players"].Rows.Add(new object[] { "Default", new Point(0,0), new Size(125,167) });
                }
                if (ds.Tables["Settings"].Rows.Count > 0)
                {
                    DataRow row = ds.Tables["Settings"].Rows[0];
                    try
                    {
                        checkBoxZoneOnly.Checked = bDisplayCurrentZoneOnlyState = (bool)row["checkBoxZoneOnly"];
                        checkBoxWarnings.Checked = bAnnounceWarningState = (bool)row["checkBoxWarnings"];
                        textBoxDelay.Text = (string)row["textBoxDelay"];
                        textBoxMultDelay.Text = (string)row["textBoxMultDelay"];
                        comboBoxAnnounce.SelectedIndex = (int)row["comboAnnounce"];
                        comboBoxMini.SelectedIndex = (int)row["comboShow"];
                    }
                    catch (Exception pte)
                    {
                        MessageBox.Show(pte.Message);
                    }
                }
            }
            else //no existing config file
            {
                //set defaults
                checkBoxWarnings.Checked = bAnnounceWarningState = true;
                comboBoxAnnounce.SelectedIndex = (int)ComboIndex.All;
                comboBoxMini.SelectedIndex = (int)ComboIndex.All;
                showState = announceState = ComboIndex.All;
                //add an example mob
                ds.Tables["Mobs"].Rows.Add(new object[] { "The Hobgoblin Anguish Lord", "Vaedenmoor, Realm of Despair [Raid]", 24, 20 });
                //add a default overlay window location
                ds.Tables["Players"].Rows.Add(new object[] { "Default", new Point(0, 0), new Size(125, 167) });
            }

        }

        /// <summary>
        /// Save persistant settings
        /// </summary>
        void SaveSettings()
        {
            dtSettings.Clear();
            DataRow srow = dtSettings.NewRow();
            srow["checkBoxZoneOnly"] = checkBoxZoneOnly.Checked;
            srow["checkBoxWarnings"] = checkBoxWarnings.Checked;
            srow["textBoxDelay"] = textBoxDelay.Text;
            srow["textBoxMultDelay"] = textBoxMultDelay.Text;
            srow["comboAnnounce"] = comboBoxAnnounce.SelectedIndex;
            srow["comboShow"] = comboBoxMini.SelectedIndex;
            dtSettings.Rows.Add(srow);

            SaveOverlayLoc();

            ds.WriteXml(settingsFile);
        }

        /// <summary>
        /// Save the location of the overlay for the current player
        /// </summary>
        private void SaveOverlayLoc()
        {
            if (!string.IsNullOrEmpty(currentPlayer))
            {
                DataRow found = ds.Tables["players"].Rows.Find(currentPlayer);
                if (found == null)
                {
                    DataRow row = ds.Tables["Players"].NewRow();
                    row["Player"] = currentPlayer;
                    row["formMiniDeathsLocation"] = overlay.Location;
                    row["formMiniDeathsSize"] = overlay.Size;
                    ds.Tables["Players"].Rows.Add(row);
                }
                if (found != null)
                {
                    found["formMiniDeathsLocation"] = overlay.Location;
                    found["formMiniDeathsSize"] = overlay.Size;
                }
            }
        }

        #region CALLBACKS

        /// <summary>
        /// Watch for deaths and user adding a tracked mob via chat
        /// </summary>
        /// <param name="isImport"></param>
        /// <param name="logInfo"></param>
        void oFormActMain_OnLogLineRead(bool isImport, LogLineEventArgs logInfo)
        {
            bImporting = isImport; //for use in other methods

            //see if we're adding a tracked mob from eq2 chat
            if (logInfo.detectedType == 0)
            {
                if ((match = rxAddMob.Match(logInfo.logLine)).Success)
                {
                    //see if we already have this mob
                    string name = match.Groups["mob"].Value.ToString();
                    if (name != "<no target>")
                    {
                        if (!ZoneMobs.ContainsKey(name.ToUpper()))
                        {
                            //don't have this one, add it
                            DataRow row = ds.Tables["Mobs"].NewRow();
                            row["mob"] = name;
                            row["zone"] = strZone;
                            int iVal = 0;
                            if (Int32.TryParse(match.Groups["max"].Value, out iVal))
                                row["MaxDeaths"] = iVal;
                            if (Int32.TryParse(match.Groups["warn"].Value, out iVal))
                                row["WarningLevel"] = iVal;
                            dataGridView1.Invoke((MethodInvoker)delegate { ds.Tables["Mobs"].Rows.Add(row); });
                            RebuildZoneMobs();
                            SaveSettings();
                        }
                    }
                }
            }

            //are we fighting something we are interested in?
            else if (logInfo.inCombat)
            {
                if (bTrackMob || announceState == ComboIndex.All || showState == ComboIndex.Tracked || showState == ComboIndex.All)
                {
                    if ((match = rxDied.Match(logInfo.logLine)).Success)
                    {
                        int increment = 1;
                        string whoDied = match.Groups["died"].Value.ToString();

                        //don't count mob death
                        if (ZoneMobs.ContainsKey(whoDied.ToUpper()))
                            increment = 0;

                        //try to not count pet deaths
                        //if you die, whoDied will be "You". If your pet with your name dies, whoDied will be your name
                        else if (ActGlobals.charName.ToUpper() == whoDied.ToUpper() && announceState != ComboIndex.All)
                            increment = 0;

                        if (increment > 0)
                        {
                            iDeathCount += increment;
                            totalDeaths++; //debug
                            if (whoDied.ToLower().Equals("you"))
                                whoDied = ActGlobals.charName;
                            //get the spot in the all-incoming-damage list where this person died
                            lastEncounter = ActGlobals.oFormActMain.ActiveZone.ActiveEncounter;
                            DamageTypeData dtd;
                            lastEncounter.Items[whoDied.ToUpper()].Items.TryGetValue("Incoming Damage", out dtd);
                            int killedAt = -1;
                            if(dtd != null)
                                killedAt = dtd.Items["All"].Items.Count - 1;
                            WhoDied died = new WhoDied { who = whoDied, deathCount = iDeathCount, killedAtIndex = killedAt, ed = ActGlobals.oFormActMain.ActiveZone.ActiveEncounter };
                            encounterData.deaths.Add(died);
                            //pass data back to the UI threads via queues
                            queueDied.Enqueue(died);
                            diedSignal.Set();
                            //tell the overlay
                            overlay.AddDeath(died);

                            //if warnings enabled, warn regardless of time between announcements
                            if (bAnnounceWarningState && iDeathCount == iWarnLevel)
                            {
                                string announce;
                                if (iDeathCount == 1)
                                    announce = "one death";
                                else
                                    announce = iDeathCount.ToString() + " deaths";
                                if(!isImport)
                                    ActGlobals.oFormActMain.TTS(announce);

                                if (delayAnnounce > 0)
                                {
                                    bTimerExpired = false;
                                    timerAnnounce.Start();
                                }
                                else
                                    bTimerExpired = true;
                            }
                            else if (announceState == ComboIndex.All || announceState == ComboIndex.Tracked)
                            {
                                if (bTimerExpired && bManyDeaths == false)
                                {
                                    string announce = whoDied + " died";
                                    if(!isImport)
                                        ActGlobals.oFormActMain.TTS(announce);
                                    if (delayAnnounce > 0)
                                    {
                                        bTimerExpired = false;
                                        timerAnnounce.Start();
                                    }
                                }
                                else
                                    bManyDeaths = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Watch for conditions that will show the overlay form
        /// </summary>
        /// <param name="isImport"></param>
        /// <param name="actionInfo"></param>
        void oFormActMain_AfterCombatAction(bool isImport, CombatActionEventArgs actionInfo)
        {
            if (showState == ComboIndex.All && !bMiniIsVisible && iDeathCount > 0)
                SetMiniVisible(true);

            //see whether we are figthing a mob that's in our list
            if (ZoneMobs.Count > 0 && bTrackMob == false)
            {
                //swDict.Start();
                ZoneMob mob;
                bool found = ZoneMobs.TryGetValue(actionInfo.combatAction.Victim.ToUpper(), out mob);
                if(!found)
                    found = ZoneMobs.TryGetValue(actionInfo.combatAction.Attacker.ToUpper(), out mob);
                if(found)
                {
                    bTrackMob = true;
                    //auto-show the mini window?
                    if (showState == ComboIndex.Tracked && bMiniIsVisible == false)
                        SetMiniVisible(true);

                    if(mob.maxDeaths.HasValue)
                        overlay.SetMaxDeaths(mob.maxDeaths.Value);
                    if (mob.warningLevel.HasValue)
                        iWarnLevel = mob.warningLevel.Value;
                    else
                        iWarnLevel = -1;
                }
                //swDict.Stop();
            }
        }

        /// <summary>
        /// Get allies and strongest enemy, stop timers, save as most recent encounter
        /// </summary>
        /// <param name="isImport"></param>
        /// <param name="encounterInfo"></param>
        void oFormActMain_OnCombatEnd(bool isImport, CombatToggleEventArgs encounterInfo)
        {
            if (encounterInfo != null)
            {
                //to distinguish enemies by color in the DrawItem callback
                strongestEnemy = encounterInfo.encounter.GetStrongestEnemy(ActGlobals.charName);
                if (!string.IsNullOrEmpty(strongestEnemy))
                    strongestEnemy = strongestEnemy.ToLower();
                else
                    strongestEnemy = " "; //don't want null
                List<CombatantData> allies = encounterInfo.encounter.GetAllies();
                combatAllies.Clear();
                foreach (CombatantData data in allies)
                {
                    combatAllies.Add(data.Name.ToLower());
                }

                //set the 'Last Encounter' stuff
                ThreadInvokes.ControlSetText(ActGlobals.oFormActMain, textBoxEncounterZone, encounterInfo.encounter.ZoneName);
                if (encounterInfo.encounter.Items != null)
                {
                    lastEncounter = encounterInfo.encounter;
                    //add to the UI
                    mUiContext.Post(SetEncounterItems, null);
                }
            }
            //timer is now irrelevant
            timerMultDelay.Stop();
            bWaitingMultiTimer = false;
        }

        /// <summary>
        /// Watch for conditions to show the overlay form. Initialize new encounter
        /// </summary>
        /// <param name="isImport"></param>
        /// <param name="encounterInfo"></param>
        void oFormActMain_OnCombatStart(bool isImport, CombatToggleEventArgs encounterInfo)
        {
            //hide the mini?
            if(bMiniIsVisible &&
                ((showState == ComboIndex.Tracked && bTrackMob == false)
                || (showState == ComboIndex.All && iDeathCount == 0)
                ))
            {
                SetMiniVisible(false);
            }

            iDeathCount = 0;
            bTrackMob = false;
            bManyDeaths = false;
            bWaitingMultiTimer = false;
            iWarnLevel = -1;
            encounterData = new EncounterRecord { ed = encounterInfo.encounter};
            timerMultDelay.Stop();
            mUiContext.Post(SetInitialTreeNode, encounterInfo.encounter.Title);
            overlay.CombatStart();
        }

        /// <summary>
        /// Watch for zone to assist in showing tracked names for the zone
        /// </summary>
        /// <param name="IsImport"></param>
        /// <param name="NewLogFileName"></param>
        void oFormActMain_LogFileChanged(bool IsImport, string NewLogFileName)
        {
            //there's a good chance the zone changed
            //ActGlobals.oFormActMain.ActiveZone.ZoneName doesn't seem to be current
            //let's just clear the old one. The next zone check should set it.
            strZone = "";
        }

        /// <summary>
        /// Handle a tracked name shared from another player
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void oFormActMain_XmlSnippetAdded(object sender, XmlSnippetEventArgs e)
        {
            if (e.ShareType == "Deaths")
            {
                string mob = "";
                e.XmlAttributes.TryGetValue("M", out mob);

                string zone = "";
                e.XmlAttributes.TryGetValue("Z", out zone);

                string deaths = "";
                e.XmlAttributes.TryGetValue("D", out deaths);

                string warning = "";
                e.XmlAttributes.TryGetValue("W", out warning);

                mUiContext.Post(AddMobTableRow, new object[] { mob, zone, deaths, warning });
                
                SaveSettings();

                e.Handled = true;

            }
        }

        /// <summary>
        /// Callback from the overlay form to remove a death from the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Overlay_RemoveItemEvent(object sender, EventArgs e)
        {
            FormOverlay.RemoveItemEventArgs arg = e as FormOverlay.RemoveItemEventArgs;
            if (arg != null)
            {
                RemoveDeath(arg.index);
            }
        }

        /// <summary>
        /// Callback from the overlay - user hiding the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Overlay_UserHidesFormEvent(object sender, EventArgs e)
        {
            buttonShowMini.Text = "Show Mini";
            bMiniIsVisible = false;
        }

        #endregion CALLBACKS

        /// <summary>
        /// Update the ZoneMobs dictionary for the current zone
        /// </summary>
        void RebuildZoneMobs()
        {
            //make a list of mobs we watch for this zone
            ZoneMobs.Clear();
            string escapedZone = strZone.Replace("'", "''");
            string select = "zone='" + escapedZone + "'";
            DataRow[] foundRows = ds.Tables["Mobs"].Select(select);
            if (foundRows.Length != 0)
            {
                foreach (DataRow row in foundRows)
                {
                    string key = row["mob"].ToString().ToUpper();
                    int? warn = row["WarningLevel"] as int?;
                    int? wipe = row["MaxDeaths"] as int?;
                    ZoneMob mob = new ZoneMob(row["mob"].ToString(), strZone, warn, wipe);
                    ZoneMobs.Add(key, mob);
                }
            }
        }

        /// <summary>
        /// Update the mob dictionary for the zone
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            RebuildZoneMobs();
            SaveSettings();
        }

        /// <summary>
        /// Update the mob dictionary for the zone
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            RebuildZoneMobs();
            SaveSettings();
        }

        /// <summary>
        /// Update the tracked names to match the zone. Called on the UI thread.
        /// </summary>
        private void DisplayZoneList()
        {
            if (bDisplayCurrentZoneOnlyState && !string.IsNullOrEmpty(strZone))
            {
                string escapedText = strZone.Replace("'", "''");
                ds.Tables["Mobs"].DefaultView.RowFilter = "Zone = '" + escapedText + "'";
            }
            else
                ds.Tables["Mobs"].DefaultView.RowFilter = "";
        }

        /// <summary>
        /// Handle the "multiple deaths" announcement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerAnnounce_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (bManyDeaths)
            {
                if (bWaitingMultiTimer == false)
                {
                    //we have more to say
                    if (!bImporting)
                        ActGlobals.oFormActMain.TTS("multiple deaths");
                    timerMultDelay.Stop();
                    if (delayMultiDeath > 0)
                    {
                        timerMultDelay.Interval = delayMultiDeath;
                        timerMultDelay.Start();
                        bWaitingMultiTimer = true;
                    }
                }
            }
            bTimerExpired = true;
        }

        /// <summary>
        /// Re-enables the "multiple deaths" announcement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerMultDelay_Elapsed(object sender, ElapsedEventArgs e)
        {
            bManyDeaths = false;
            bWaitingMultiTimer = false;
        }

        /// <summary>
        /// Color the enemies in the encounter list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxEncounter_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            Graphics g = e.Graphics;
            ListBox lb = (ListBox)sender;
            if (e.Index >= 0)
            {
                //make enemies red, strongest enemy bold red
                string item = lb.Items[e.Index].ToString();
                if (item.ToLower().Equals(strongestEnemy))
                    g.DrawString(item, new Font(e.Font, FontStyle.Bold), Brushes.Red, e.Bounds);
                else if (combatAllies.Contains(item.ToLower()) || combatAllies.Count == 0)
                    g.DrawString(item, e.Font, Brushes.Black, e.Bounds);
                else
                    g.DrawString(item, e.Font, Brushes.Red, e.Bounds);
            }
            e.DrawFocusRectangle();
        }

        #region HELP

        private void listBoxEncounter_MouseHover(object sender, EventArgs e)
        {
           const string text =
              @"{\rtf1\ansi {\colortbl ;\red255\green0\blue0;}\cf0\b0\ul0"
            + @"{\b\ul Last Encounter}\line "
            + @"This list is filled with the encounter participants at the end of an encounter. "
            + @"Encounter enemies are colored {\cf1 red} and the strongest enemy is {\cf1\b bold red}. "
            + @"(Note: The coloring process is  a 'best guess' and is not 100% accurate.) "
            + @"\line\line Double click a name to add it to the {\b Tracked Names}."
            + @"}";
            richTextBox1.Rtf = text;
        }

        private void textBoxDeaths_MouseHover(object sender, EventArgs e)
        {
            const string text =
               @"{\rtf1\ansi {\colortbl ;\red255\green0\blue0;}\cf0\b0\ul0"
             + @"{\b\ul Death Count}\line "
             + @"Count of deaths in the current encounter. \line "
             + @"Note that the death of a pet (unless it is your pet) will typically be counted. \line "
             + @"Double-clicking an entry in the {\b Who Died} list will decrement this count. "
             + @"}";

            richTextBox1.Rtf = text;
        }

        private void dataGridView1_MouseHover(object sender, EventArgs e)
        {
            const string text =
               @"{\rtf1\ansi {\colortbl ;\red0\green77\blue187;}\cf0\b0\ul0"
             + @"{\b\ul Tracked Names}\line "
             + @"Deaths are counted and the count displayed as {\b Death Count} when the current encounter contains a matching name and zone from this list. "
             + @"When fighting a matching name and zone, if {\b Announce Warnings} is enabled and the death count reaches the {\b WarningLevel}, "
             + @"the plugin generates an audio alert. {\b Note:} In general, the plugin cannot distinguish between a player death and a pet death. \line "
             + @"For encounters with multiple named mobs, add each mob name to the list to prevent the plugin from counting the death of a named mob. "
             + @"Double-click an entry in the {\b Last Encounter} list to add the name and zone to the {\b Tracked Names} list. "
             + @"\line Add a name from within EQII by targeting the mob and typing {\cf1\b\i /say track deaths for %t}  (click to copy to the clipboard) "
             + @"optionally followed by the max and warning death counts. "
             + @"If manually typing mobs into the list, make sure everything is spelled exactly as shown in-game. "
             + @"\line Right-click an entry to copy for sharing in EQII."
             + @"}";

            richTextBox1.Rtf = text;

            hyperLinkText = "/say track deaths for %t";
            
        }

        private void richTextBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!string.IsNullOrEmpty(hyperLinkText))
            {
                //hack to simulate a hyperlink

                int mousePointerCharIndex = richTextBox1.GetCharIndexFromPosition(e.Location);
                int mousePointerLine = richTextBox1.GetLineFromCharIndex(mousePointerCharIndex);
                int firstCharIndexInMousePointerLine = richTextBox1.GetFirstCharIndexFromLine(mousePointerLine);
                int firstCharIndexInNextLine = richTextBox1.GetFirstCharIndexFromLine(mousePointerLine + 1);
                if (firstCharIndexInNextLine < 0)
                {
                    firstCharIndexInNextLine = richTextBox1.Text.Length;
                }

                // See where the hyperlink starts, as long as it's on the same line as the mouse
                int hotTextStartIndex = richTextBox1.Find(
                    hyperLinkText, firstCharIndexInMousePointerLine, firstCharIndexInNextLine, RichTextBoxFinds.NoHighlight);

                if (hotTextStartIndex >= 0 &&
                    mousePointerCharIndex >= hotTextStartIndex && mousePointerCharIndex < hotTextStartIndex + hyperLinkText.Length)
                {
                    // Simulate hyperlink behavior
                    if(richTextBox1.Cursor != Cursors.Hand)
                        richTextBox1.Cursor = Cursors.Hand;
                    mouseOnHotText = true;
                }
                else
                {
                    if (richTextBox1.Cursor != defaultRichTextBoxCursor)
                        richTextBox1.Cursor = defaultRichTextBoxCursor;
                    mouseOnHotText = false;
                }
            }
        }

        private void richTextBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (mouseOnHotText)
            {
                try
                {
                    Clipboard.SetText("/say track deaths for %t 24 20");
                }
                catch { }
            }
            
        }

        private void checkBoxZoneOnly_MouseHover(object sender, EventArgs e)
        {
            const string text =
              @"{\rtf1\ansi\cf0\b0\ul0"
            + @"{\b\ul Show Current Zone Only}\line "
            + @"List only the {\b Tracked Names} for the current zone. \line "
            + @"}";

            richTextBox1.Rtf = text;
        }

        private void listBoxDeaths_MouseHover(object sender, EventArgs e)
        {
            const string text =
              @"{\rtf1\ansi\cf0\b0\ul0"
            + @"{\b\ul Who Died}\line "
            + @"Deaths in the current encounter. A new death is added at the top of the list. "
            + @"\line\line Double-click an entry to open its cause of death window."
            + @"\line\line Right-click an entry to remove the death, for example a pet. "
            + @"(Any name may be right-clicked to remove it and decrease the death count, it doesn't have to be a pet.)"
            + @"}";
            richTextBox1.Rtf = text;
        }

        private void DeathCount_MouseHover(object sender, EventArgs e)
        {
            richTextBox1.Text = "Hover the mouse over a control for help.";
        }

        private void textBoxDelay_MouseHover(object sender, EventArgs e)
        {
            const string text =
              @"{\rtf1\ansi\cf0\b0\ul0"
            + @"{\b\ul Single Gap}\line "
            + @"Delay between audio announcements, in milleseconds (e.g. 1500 = 1500 milleseconds = 1.5 seconds). "
            + @"Instead of letting audio announcements talk over themselves (which would happen in a raid wipe, for example), "
            + @"the plugin waits this long after starting one announcement before starting another. "
            + @"If a new announcement is triggered before this delay has elapsed, "
            + @"then when this delay expires the plugin will announce 'multiple deaths' instead of announcing each death. "
            + @"Once 'multiple deaths' has been announced, death announcements are disabled for {\b Multiple Gap} milleseconds "
            + @"(they are still counted). "
            + @"This delay feature can be disabled by setting the {\b msec Single Gap} to zero. "
            + @"\line\line The warning annoucement, if enabled, is always played regardless of this setting, i.e. even if it will talk over a playing death announement. "
            + @"}";
            richTextBox1.Rtf = text;
        }

        private void checkBoxWarnings_MouseHover(object sender, EventArgs e)
        {
            const string text =
              @"{\rtf1\ansi\cf0\b0\ul0"
            + @"{\b\ul Announce Warnings}\line "
            + @"Enables the annoucement when the number of deaths reaches the {\b WarningLevel} when fighting a name in the {\b Tracked Names} list. "
            + @"}";
            richTextBox1.Rtf = text;
        }

        private void buttonShowMini_MouseHover(object sender, EventArgs e)
        {
            const string text =
              @"{\rtf1\ansi\cf0\b0\ul0"
            + @"{\b\ul Show/Hide Mini}\line "
            + @"Shows or hides the mini death list window. If you are running the game in windowed mode, the mini window can float above the game window. "
            + @"Double-clicking a name in the mini window removes the death from the list and decrements the {\b Death Count}. "
            + @"}";
            richTextBox1.Rtf = text;
        }

        private void textBoxEncounterZone_MouseHover(object sender, EventArgs e)
        {
            const string text =
              @"{\rtf1\ansi\cf0\b0\ul0"
            + @"{\b\ul Encounter Zone}\line "
            + @"Zone of the last, or selected, encounter. "
            + @"}";
            richTextBox1.Rtf = text;
        }

        private void textBoxZone_MouseHover(object sender, EventArgs e)
        {
            const string text =
              @"{\rtf1\ansi\cf0\b0\ul0"
            + @"{\b\ul Current Zone}\line "
            + @"Current zone according to the log file. "
            + @"}";
            richTextBox1.Rtf = text;
        }

        private void textBoxMultDelay_MouseHover(object sender, EventArgs e)
        {
            const string text =
              @"{\rtf1\ansi\cf0\b0\ul0"
            + @"{\b\ul Multiple Gap}\line "
            + @"Delay between announcing 'multiple deaths' and the next single death announcement, in milleseconds "
            + @"(e.g. 5000 = 5000 milleseconds = 5 seconds). \line "
            + @"'Multiple deaths' is triggered if there are two or more deaths within the {\b Single Gap} milliseconds. "
            + @"\line\line If set to zero, once the 'multiple deaths' announcement occurs, "
            + @"the plugin will not announce any more deaths for that encounter, "
            + @"but the warning level announcement will still occur (if enabled). "
            + @"}";
            richTextBox1.Rtf = text;
        }

        private void comboBoxAnnounce_MouseHover(object sender, EventArgs e)
        {
            const string text =
             @"{\rtf1\ansi\cf0\b0\ul0"
             + @"{\b\ul Announce Selection}\line "
             + @"{\b Note:} Announcements will include the death of a player pet.\line\line"
             + @"{\b Announce All} Enables an annoucement for each death for every encounter. "
             + @"This setting may be used to alert for the a need for a resurrection.\line "
             + @"{\b Announce Tracked} Enables the annoucement for each death only while fighting a name in the {\b Tracked Names} list.\line "
             + @"{\b Announce None} Disable audio announcement of {\b Who Died}. "
             + @"}";
            richTextBox1.Rtf = text;
        }

        private void comboBoxMini_MouseHover(object sender, EventArgs e)
        {
            const string text =
              @"{\rtf1\ansi\cf0\b0\ul0"
             + @"{\b\ul Auto Show Selection}\line\line "
             + @"{\b Auto Show All} Displays the mini window upon a death for every encounter.\line "
             + @"{\b Auto Show Tracked} Displays the mini window when fighting a mob in the {\b Tracked Names} list "
             + @"and closed after a non-tracked fight.\line "
             + @"{\b Disable Auto Show} The mini window is not displayed. "
             + @"}";
            richTextBox1.Rtf = text;
        }

        private void treeView1_MouseHover(object sender, EventArgs e)
        {
            const string text =
               @"{\rtf1\ansi {\colortbl;\red255\green0\blue0;}\cf0\b0\ul0"
             + @"{\b\ul Encounters}\line "
             + @"The tree shows encounters and, at the end of an encounter, expands to show participants. "
             + @"Encounter enemies are colored {\cf1 red} and the strongest enemy is {\cf1\ul underlined red}. "
             + @"(Note: The coloring process is ACT's 'best guess' and is not 100% accurate.) "
             + @"\line\line Right-click a red name or the top tree name and use the menu to add it to the {\b Tracked Names}."
             + @"}";
            richTextBox1.Rtf = text;
        }

        #endregion HELP


        #region CROSS THREAD SUPPORT

        /// <summary>
        /// Copy the UI state to a local and update the mob list to match
        /// </summary>
        private void checkBoxZoneOnly_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxZoneOnly.Checked)
            {
                bDisplayCurrentZoneOnlyState = true;
                DisplayZoneList();
            }
            else
            {
                bDisplayCurrentZoneOnlyState = false;
                DisplayZoneList();
            }
        }

        /// <summary>
        /// Copy the UI state to a local
        /// </summary>
        private void checkBoxWarnings_CheckedChanged(object sender, EventArgs e)
        {
            //copy to a local so the callbacks don't need to do cross-thread UI access
            bAnnounceWarningState = checkBoxWarnings.Checked;
        }

        /// <summary>
        /// Convert the text to a local integer
        /// </summary>
        private void textBoxDelay_TextChanged(object sender, EventArgs e)
        {
            bool parsed = Int32.TryParse(textBoxDelay.Text, out delayAnnounce);
            if(parsed && delayAnnounce > 0)
            {
                timerAnnounce.Interval = delayAnnounce;
                timerAnnounce.Enabled = true;
            }
            else
            {
                // disable the announce
                delayAnnounce = 0;
                timerAnnounce.Enabled = false;
            }
        }

        /// <summary>
        /// Convert the text to a local integer
        /// </summary>
        private void textBoxMultDelay_TextChanged(object sender, EventArgs e)
        {
            bool parsed = !Int32.TryParse(textBoxMultDelay.Text, out delayMultiDeath);
            if(parsed && delayMultiDeath > 0)
            {
                timerMultDelay.Interval = delayMultiDeath;
                timerMultDelay.Enabled = true;
            }
            else
            {
                delayMultiDeath = 0;
                timerMultDelay.Enabled = false;
            }
        }

        /// <summary>
        /// From any thread, update the overlay form visibility and the plugin button text
        /// </summary>
        /// <param name="visible">True to show the overlay form</param>
        private void SetMiniVisible(bool visible)
        {
            overlay.SetVisibility(visible);
            bMiniIsVisible = visible;
            string txt = visible ? "Hide Mini" : "Show Mini";
            ThreadInvokes.ControlSetText(ActGlobals.oFormActMain, buttonShowMini, txt);
        }

        /// <summary>
        /// Update the treeview with encounter participants.
        /// On the UI thread via mUiContext()
        /// </summary>
        /// <param name="o">unused</param>
        private void SetEncounterItems(object o)
        {
            //remove the "Encounter" placeholder tree node
            int index = treeView1.Nodes.Count - 1;
            string tag = treeView1.Nodes[index].Tag as string;
            if (tag == "placeholder")
                treeView1.Nodes[index].Remove();

            // add the real encounter
            TreeNode fight = new TreeNode(lastEncounter.GetStrongestEnemy(ActGlobals.charName));
            fight.Tag = encounterData;
            foreach (CombatantData combatant in lastEncounter.Items.Values)
            {
                TreeNode node = new TreeNode(combatant.Name);
                fight.Nodes.Add(node);
            }
            treeView1.Nodes.Add(fight);
            treeView1.Nodes[treeView1.Nodes.Count - 1].EnsureVisible();
        }

        /// <summary>
        /// Puts a placeholder encounter in the treeview at combat start.
        /// On the UI thread via mUiContext()
        /// </summary>
        /// <param name="o">unused</param>
        private void SetInitialTreeNode(object o)
        {
            // the placeholder gets replaced with the mob name node at combat end
            string title = o as string;
            TreeNode node = new TreeNode(title);
            node.Tag = "placeholder";
            treeView1.Nodes.Add(node);
            treeView1.Nodes[treeView1.Nodes.Count - 1].EnsureVisible();

            listBoxDeaths.Items.Clear();
            labelWhoDied.Text = "Who Died";
        }

        /// <summary>
        /// Update the tracked mobs for the current zone.
        /// On the UI thread via mUiContext()
        /// </summary>
        /// <param name="o">unused</param>
        private void UpdateMobsForZone(object o)
        {
            if (bDisplayCurrentZoneOnlyState && !string.IsNullOrEmpty(strZone))
            {
                string escapedText = strZone.Replace("'", "''");
                ds.Tables["Mobs"].DefaultView.RowFilter = "Zone = '" + escapedText + "'";
            }
            else
                ds.Tables["Mobs"].DefaultView.RowFilter = "";
        }

        /// <summary>
        /// Add a mob to the tracked names table.
        /// On the UI thread via mUiContext()
        /// </summary>
        /// <param name="o">object[] array of the new tracked mob info</param>
        private void AddMobTableRow(object o)
        {
            DataTable tbl = dataGridView1.DataSource as DataTable;
            object[] array = o as object[];
            if (array != null)
            {
                try
                {
                    //if the mob-zone are already in the table, this will fail
                    // thus the try/catch
                    tbl.Rows.Add(array);
                }
                catch { }
            }
        }

        /// <summary>
        /// Waits for a death from the log line thread
        /// </summary>
        private void Consumer()
        {
            Task.Run(() =>
            {
                while (runConsumer)
                {
                    diedSignal.WaitOne();

                    try
                    {
                        WhoDied arg = null;
                        while (queueDied.TryDequeue(out arg))
                        {
                            if (arg != null)
                            {
                                displayedDeaths++;  //debug

                                //plugin tab
                                mUiContext.Post(UpdateDeaths, arg);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            });
        }

        /// <summary>
        /// Update the death list.
        /// On the UI thread via mUiContext()
        /// </summary>
        /// <param name="o">WhoDied describing the death</param>
        private void UpdateDeaths(object o)
        {
            WhoDied args = o as WhoDied;
            if (args != null)
            {
                listBoxDeaths.Items.Insert(0, args);
                labelWhoDied.Text = listBoxDeaths.Items.Count.ToString() +  " Who Died";
            }
        }

        /// <summary>
        /// Save the show-overlay choice to a local.
        /// </summary>
        private void comboBoxMini_SelectedIndexChanged(object sender, EventArgs e)
        {
            //copy to a local so the callbacks don't need to do cross-thread UI access
            showState = (ComboIndex)comboBoxMini.SelectedIndex;
        }

        /// <summary>
        /// Save the announce choice to a local.
        /// </summary>
        private void comboBoxAnnounce_SelectedIndexChanged(object sender, EventArgs e)
        {
            //copy to a local so the callbacks don't need to do cross-thread UI access
            announceState = (ComboIndex)comboBoxAnnounce.SelectedIndex;
        }

        #endregion CROSS THREAD SUPPORT


        #region MENUS & MOUSE CLICKS

        /// <summary>
        /// Encode a tracked name for XML sharing and copy it to the clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copyXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dataGridView1.Rows[contextMenuRow];
            StringBuilder xml = new StringBuilder();
            if (!string.IsNullOrEmpty(row.Cells["Mob"].Value.ToString()) && !string.IsNullOrEmpty(row.Cells["Zone"].Value.ToString()))
            {
                xml.Append(@"<Deaths ");
                xml.Append(@"M=""" + SecurityElement.Escape(row.Cells["Mob"].Value.ToString()) + @""" ");
                xml.Append(@"Z=""" + SecurityElement.Escape(row.Cells["Zone"].Value.ToString()) + @""" ");
                if (!string.IsNullOrEmpty(row.Cells["MaxDeaths"].Value.ToString()))
                    xml.Append(@"D=""" + SecurityElement.Escape(row.Cells["MaxDeaths"].Value.ToString().Replace(", ", ",")) + @""" ");
                if (!string.IsNullOrEmpty(row.Cells["WarningLevel"].Value.ToString()))
                    xml.Append(@"W=""" + SecurityElement.Escape(row.Cells["WarningLevel"].Value.ToString()) + @""" ");
                xml.Append(@"/>");
                if (xml.Length > 250)
                    MessageBox.Show("Entry is too long to paste into EQII");
                try
                {
                    Clipboard.SetText(xml.ToString());
                }
                catch { }
            }
            else
                MessageBox.Show("Shareable entry must contain a mob and a zone.");
        }

        /// <summary>
        /// Save the grid row for the context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            contextMenuRow = e.RowIndex;
        }

        /// <summary>
        /// Toggle the overlay form visibility
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonShowMini_Click(object sender, EventArgs e)
        {
            SetMiniVisible(!bMiniIsVisible);
        }

        /// <summary>
        /// Opens the cause of death form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxDeaths_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = listBoxDeaths.SelectedIndex;

            WhoDied whoDied = (WhoDied)listBoxDeaths.Items[index];
            if (lastEncounter != null && whoDied != null)
            {
                Point loc = listBoxDeaths.Location;
                Point corner = this.PointToScreen(loc);
                Rectangle rect = new Rectangle(corner, listBoxDeaths.Size);
                DeathLog deathLog = new DeathLog(whoDied, lastEncounter, rect);
                deathLog.Show();
            }

        }

        /// <summary>
        /// Remove a death from the list on right-click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxDeaths_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int index = listBoxDeaths.IndexFromPoint(e.Location);
                if (index >= 0)
                {
                    RemoveDeath(index);
                    overlay.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// Remove a death from the list
        /// </summary>
        /// <param name="idx">List index of the item to remove</param>
        private void RemoveDeath(int idx)
        {
            //assumes the indexes are the same in the mini window and the main window (should be)
            if (idx >= 0)
            {
                try
                {
                    iDeathCount--;
                    if (listBoxDeaths.Items.Count > idx)
                    {
                        listBoxDeaths.Items.RemoveAt(idx);
                        labelWhoDied.Text = listBoxDeaths.Items.Count.ToString() + " Who Died";
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Build the death list for the encounter for the selected treeview item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            EncounterRecord er = e.Node.Tag as EncounterRecord;
            if(er != null)
            {
                lastEncounter = er.ed;
                listBoxDeaths.Items.Clear();
                labelWhoDied.Text = "Who Died";
                overlay.CombatStart();
                strongestEnemy = er.ed.GetStrongestEnemy(ActGlobals.charName);
                textBoxEncounterZone.Text = er.ed.ZoneName;
                foreach (WhoDied who in er.deaths)
                {
                    listBoxDeaths.Items.Insert(0, who);
                    //tell the overlay
                    overlay.AddDeath(who);
                }
                if(listBoxDeaths.Items.Count > 0)
                    labelWhoDied.Text = listBoxDeaths.Items.Count.ToString() +  " Who Died";
            }
        }

        /// <summary>
        /// Provides for coloring the enemies and allies differently in the treeview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            TreeNodeStates state = e.State;
            Font font = e.Node.NodeFont ?? e.Node.TreeView.Font;
            Color foreColor;
            Color backColor;

            if ((state & TreeNodeStates.Selected) == TreeNodeStates.Selected)
            {
                // node is selected
                bool isFocused = (state & TreeNodeStates.Focused) == TreeNodeStates.Focused;
                backColor = SystemColors.Highlight;
                foreColor = SystemColors.HighlightText;
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
                if (isFocused)
                    ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds, foreColor, backColor);
                TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, foreColor, 
                    TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            }
            else
            {
                // node is not selected
                backColor = SystemColors.Window;
                foreColor = SystemColors.WindowText; //default
                if(e.Node.Level == 1)
                {
                    //2nd level = combatants
                    // color enemies red, strongest enemy underlined
                    if (e.Node.Text.ToLower().Equals(strongestEnemy))
                    {
                        font = new Font(font, FontStyle.Underline);
                        foreColor = Color.Red;
                    }
                    else if (!combatAllies.Contains(e.Node.Text.ToLower()) && combatAllies.Count != 0)
                        foreColor = Color.Red;
                }
                using (Brush background = new SolidBrush(backColor))
                {
                    e.Graphics.FillRectangle(background, e.Bounds);
                    TextRenderer.DrawText(e.Graphics, e.Node.Text, font, e.Bounds, foreColor, 
                        TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
                }
            }
        }

        /// <summary>
        /// Find the allies and enemies for the selected encounter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            EncounterRecord er = e.Node.Tag as EncounterRecord;
            if (er != null)
            {
                // get set up for enemy highlighting
                strongestEnemy = er.ed.GetStrongestEnemy(ActGlobals.charName).ToLower();
                List<CombatantData> allies = er.ed.GetAllies();
                combatAllies.Clear();
                foreach (CombatantData data in allies)
                {
                    combatAllies.Add(data.Name.ToLower());
                }
            }
        }

        /// <summary>
        /// Context menu to add the selected treeview encounter participant to the tracked names list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addToTrackedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                string mob = treeView1.SelectedNode.Text;
                EncounterRecord er;
                if (treeView1.SelectedNode.Level == 0)
                    er = treeView1.SelectedNode.Tag as EncounterRecord;
                else
                    er = treeView1.SelectedNode.Parent.Tag as EncounterRecord;
                if (er != null)
                {
                    string zone = er.ed.ZoneName;
                    if (!string.IsNullOrEmpty(zone) && !string.IsNullOrEmpty(mob))
                    {
                        DataRow old = ds.Tables["Mobs"].Rows.Find(new object[] { mob, zone });
                        if (old == null)
                        {
                            DataRow row = ds.Tables["Mobs"].NewRow();
                            row["mob"] = mob;
                            row["zone"] = zone;
                            ds.Tables["Mobs"].Rows.Add(row);
                            RebuildZoneMobs();
                            SaveSettings();
                        }
                        else
                            MessageBox.Show("Already tracking " + mob + " / " + zone);
                    }
                }
            }
        }

        /// <summary>
        /// Set the node of the mouse click for use by the context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                treeView1.SelectedNode = e.Node;
        }

        #endregion MENUS & MOUSE CLICKS

    }

}
