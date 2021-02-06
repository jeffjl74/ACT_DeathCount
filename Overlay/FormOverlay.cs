using Advanced_Combat_Tracker;
using Death_Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Overlay
{
    public partial class FormOverlay : Form
    {
        // event callbacks to the main form
        public event EventHandler RemoveItemEvent;
        public event EventHandler UserHidesFormEvent;
        public class RemoveItemEventArgs : EventArgs
        {
            public int index { get; set; }
        }

        // the main form controls the overlay via a ConcurrentQueue that can be
        // populated from any thread
        ConcurrentQueue<QueueArgs> queueArgs = new ConcurrentQueue<QueueArgs>();
        enum OverlayCommand { DEATH = 0, VISIBILITY, MAX_DEATHS, RESET, LOCATE, REMOVE };
        private class QueueArgs
        {
            public OverlayCommand type;
            public int count;       //type DEATH: new death count
            public string who;      //type DEATH:who died
            public bool visible;    //type VISIBILITY
            public int maxDeaths;   //type MAX_DEATHS
            public Size size;       //type LOCATE
            public Point loc;       //type LOCATE
            public int index;       //type REMOVE:listbox index. type DEATH:encounter data index
        }
        AutoResetEvent dataSignal = new AutoResetEvent(false);
        WindowsFormsSynchronizationContext mUiContext = new WindowsFormsSynchronizationContext();
        bool runConsumer = true;

        Size mSize;
        Point mLoc;

        /// <summary>
        /// Current encounter. Saved when the main form adds a death.
        /// </summary>
        EncounterData encounterData;

        public FormOverlay()
        {
            InitializeComponent();

            Consumer();
        }

        /// <summary>
        /// Set size and TopMost
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormOverlay_Load(object sender, EventArgs e)
        {
            if (!mSize.IsEmpty)
                this.Size = mSize;
            if (!mLoc.IsEmpty)
                this.Location = mLoc;
            this.TopMost = true;
        }

        /// <summary>
        /// Only hide it if the user "closes" the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormOverlay_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                //don't want to actually destroy it, just hide it
                this.Hide();
                EventArgs arg = new EventArgs();
                UserHidesFormEvent?.Invoke(this, arg);
                e.Cancel = true;
            }
            else
            {
                //allow it to close
                //stop the consumer thread
                runConsumer = false;
                dataSignal.Set();
            }
        }

        /// <summary>
        /// main form wants to add a death to the list
        /// </summary>
        /// <param name="dd"></param>
        public void AddDeath(WhoDied dd)
        {
            QueueArgs arg = new QueueArgs { type = OverlayCommand.DEATH, count = dd.deathCount, who = dd.who, index = dd.killedAtIndex };
            encounterData = dd.ed;
            queueArgs.Enqueue(arg);
            dataSignal.Set();
        }

        /// <summary>
        /// main form wants to show or hide the overlay
        /// </summary>
        /// <param name="visible"></param>
        public void SetVisibility(bool visible)
        {
            QueueArgs arg = new QueueArgs { type = OverlayCommand.VISIBILITY, visible = visible };
            queueArgs.Enqueue(arg);
            dataSignal.Set();
        }

        /// <summary>
        /// main form sets the max death field
        /// </summary>
        /// <param name="maxDeaths"></param>
        public void SetMaxDeaths(int maxDeaths)
        {
            QueueArgs arg = new QueueArgs { type = OverlayCommand.MAX_DEATHS, maxDeaths = maxDeaths  };
            queueArgs.Enqueue(arg);
            dataSignal.Set();
        }

        /// <summary>
        /// main form signals start of combat
        /// </summary>
        public void CombatStart()
        {
            QueueArgs arg = new QueueArgs { type = OverlayCommand.RESET };
            queueArgs.Enqueue(arg);
            dataSignal.Set();
        }

        /// <summary>
        /// main form wants to position the overlay
        /// </summary>
        /// <param name="size"></param>
        /// <param name="loc"></param>
        public void SetLocation(Size size, Point loc)
        {
            QueueArgs arg = new QueueArgs { type = OverlayCommand.LOCATE, size = size, loc = loc };
            queueArgs.Enqueue(arg);
            dataSignal.Set();
        }

        /// <summary>
        /// main form wants to remove a listing
        /// </summary>
        /// <param name="idx"></param>
        public void RemoveAt(int idx)
        {
            QueueArgs arg = new QueueArgs { type = OverlayCommand.REMOVE, index = idx };
            queueArgs.Enqueue(arg);
            dataSignal.Set();
        }

        /// <summary>
        /// process tasks from the main form via queue
        /// </summary>
        private void Consumer()
        {
            Task.Run(() =>
            {
                while (runConsumer)
                {
                    dataSignal.WaitOne();

                    try
                    {
                        QueueArgs data = null;
                        while (queueArgs.TryDequeue(out data))
                        {
                            if (data != null)
                            {
                                switch (data.type)
                                {
                                    case OverlayCommand.DEATH:
                                        mUiContext.Post(UpdateForm, data);
                                        break;
                                    case OverlayCommand.VISIBILITY:
                                        mUiContext.Post(UpdateVisibility, data);
                                        break;
                                    case OverlayCommand.MAX_DEATHS:
                                        mUiContext.Post(UpdateMaxDeaths, data);
                                        break;
                                    case OverlayCommand.RESET:
                                        mUiContext.Post(Clear, null);
                                        break;
                                    case OverlayCommand.LOCATE:
                                        mUiContext.Post(LocateForm, data);
                                        break;
                                    case OverlayCommand.REMOVE:
                                        mUiContext.Post(RemoveItem, data);
                                        break;
                                }
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
        /// activated from the Consumer() thread, runs on the UI thread
        /// </summary>
        /// <param name="o"></param>
        private void UpdateForm(object o)
        {
            QueueArgs w = o as QueueArgs;
            if (w != null)
            {
                textBoxDeaths.Text = w.count.ToString();
                WhoDied whoDied = new WhoDied { who = w.who, killedAtIndex = w.index, deathCount = w.count, ed = encounterData };
                listBox1.Items.Insert(0, whoDied);
            }
        }

        /// <summary>
        /// activated from the Consumer() thread, runs on the UI thread
        /// </summary>
        /// <param name="o"></param>
        private void UpdateVisibility(object o)
        {
            QueueArgs arg = o as QueueArgs;
            if (arg != null)
            {
                this.Visible = arg.visible;
            }
        }

        /// <summary>
        /// activated from the Consumer() thread, runs on the UI thread
        /// </summary>
        /// <param name="o"></param>
        private void UpdateMaxDeaths(object o)
        {
            QueueArgs arg = o as QueueArgs;
            if (arg != null)
            {
                textBoxMax.Text = arg.maxDeaths.ToString();
            }
        }

        /// <summary>
        /// activated from the Consumer() thread, runs on the UI thread
        /// </summary>
        /// <param name="o"></param>
        private void Clear(object o)
        {
            textBoxMax.Text = string.Empty;
            textBoxDeaths.Text = "0";
            listBox1.Items.Clear();
        }

        /// <summary>
        /// activated from the Consumer() thread, runs on the UI thread
        /// </summary>
        /// <param name="o"></param>
        private void LocateForm(object o)
        {
            QueueArgs arg = o as QueueArgs;
            if (arg != null)
            {
                //if we try to set the location prior to _Load() it won't take
                // so save a copy for use in _Load() in case it hasn't happened yet
                this.Size = mSize = arg.size;
                this.Location = mLoc = arg.loc;
            }
        }

        /// <summary>
        /// activated from the Consumer() thread, runs on the UI thread
        /// </summary>
        /// <param name="o"></param>
        private void RemoveItem(object o)
        {
            QueueArgs arg = o as QueueArgs;
            if (arg != null)
            {
                listBox1.Items.RemoveAt(arg.index);
                textBoxDeaths.Text = listBox1.Items.Count.ToString();
            }
        }

        /// <summary>
        /// double click opens the cause of death window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = listBox1.SelectedIndex;
            WhoDied whoDied = (WhoDied)listBox1.Items[index];
            ShowDeathLog(whoDied);

        }

        /// <summary>
        /// open the combat history
        /// </summary>
        /// <param name="who"></param>
        private void ShowDeathLog(WhoDied who)
        {
            Rectangle rect = new Rectangle(this.Location, this.Size);
            DeathLog deathLog = new DeathLog(who, encounterData, rect);
            deathLog.Show();
        }

        /// <summary>
        /// right click removes a listing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int index = listBox1.IndexFromPoint(e.Location);
                if (index >= 0)
                {
                    listBox1.Items.RemoveAt(index);
                    textBoxDeaths.Text = listBox1.Items.Count.ToString();

                    RemoveItemEventArgs args = new RemoveItemEventArgs { index = index };
                    RemoveItemEvent?.Invoke(listBox1, args);

                }
            }
        }
    }
}
