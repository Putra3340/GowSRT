using Gma.System.MouseKeyHook;
using System;
using System.CodeDom;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace CounterSplit
{

    public partial class MainForm : Form
    {
        public static BindingList<SegmentTable> data = new BindingList<SegmentTable>();
        public const string procname = "pcsx2-qt";
        public static string pcsx2MapName = string.Empty;
        const int MaxSharedSize = 0x8B00000 + 0x300000 + 0x100000; // EE + IOP + VU (143MB)
        MemoryMappedFile? mmf = null;
        MemoryMappedViewAccessor accessor = null;
        public Process proc;
        private IntPtr procHandle;
        private const uint PROCESS_ALL_ACCESS = 0x001F0FFF;
        private const uint FILE_MAP_ALL_ACCESS = 0x000F001F;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;
        ContextMenuStrip contextMenu = new ContextMenuStrip();
        int segmenthit = 0;
        float lastHp = -1;
        float totalDamage = 0;
        int hitCount = 0;
        float lastIGT = -1;
        string lastWad1 = "R_Shell"; // on main menu
        string lastWad2 = "R_Shell"; // on main menu
        TimeSpan trackedIGT = TimeSpan.Zero;
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer timersec = new System.Windows.Forms.Timer();
        int selectedsegmentindex = -1;
        bool IsFirstWad = true;
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        public MainForm()
        {
            InitializeComponent();
            globalHook = Hook.GlobalEvents();
            globalHook.KeyDown += GlobalHook_KeyDown;

        }
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        private IKeyboardMouseEvents globalHook;
        private void GlobalHook_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F7)
            {
                Debug_Click(sender, e);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            globalHook.Dispose();
            base.OnFormClosed(e);
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            this.BackColor = Color.FromArgb(255, 70, 63, 63);

            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.BackgroundColor = Color.FromArgb(255, 70, 63, 63);
            dataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(255, 70, 63, 63);
            dataGridView1.DefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 70, 63, 63);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 70, 63, 63);
            dataGridView1.RowHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dataGridView1.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToResizeRows = false;
            dataGridView1.AllowUserToResizeColumns = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridView1.DataSource = data;
            dataGridView1.Columns["SegmentCode"].Visible = false;
            dataGridView1.Columns["SegmentName"].HeaderText = "Segment Name";
            dataGridView1.Columns["CurrentHit"].HeaderText = "Current Hit";
            dataGridView1.Columns["PBHit"].HeaderText = "PB";
            dataGridView1.DefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);


            ToolStripMenuItem starttimer = new ToolStripMenuItem("Start Timer");
            ToolStripMenuItem dump = new ToolStripMenuItem("Dump Split");
            ToolStripMenuItem debug = new ToolStripMenuItem("Debug");
            ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, args) => Application.Exit();
            starttimer.Click += Starttimer_Click;
            dump.Click += export;
            debug.Click += Debug_Click;
            contextMenu.Items.Add(starttimer);
            contextMenu.Items.Add(exitItem);
            contextMenu.Items.Add(dump);
            contextMenu.Items.Add(debug);

            AttachDragEvents(this);
            FindProcess();




        }
        public static SegmentTable selectedsegment = null;
        private void TimerPersecond_Tick(object sender, EventArgs e)
        {
            lbl_date.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();

            //dataGridView1.Rows[0].Selected = true;

            float rawIGT = accessor.ReadSingle(0x302d0c);
            //float floored = (float)Math.Floor(rawIGT);
            float floored = rawIGT;
            TimeSpan rawTime = TimeSpan.FromSeconds(floored);



            // Reset if raw IGT is 0
            if (floored == 0)
            {
                trackedIGT = TimeSpan.Zero;
                lastIGT = 0;
                totalDamage = 0;
                hitCount = 0;
            }
            else if (lastIGT >= 0)
            {
                float delta = floored - lastIGT;

                // Ignore abnormal jump
                if (delta > 0 && delta < 10) // threshold
                {
                    trackedIGT += TimeSpan.FromSeconds(delta);
                }
            }

            lastIGT = floored;

            // Display
            TimeSpan t = rawTime;
            string igt = (t.Hours > 0 ? t.Hours + ":" : "") +
                         (t.Minutes > 0 || t.Hours > 0 ? t.Minutes.ToString("00") + ":" : "") +
                         t.Seconds.ToString("00") + "." +
                         (t.Milliseconds / 10).ToString("00");
            lbl_igt.Text = igt;

            t = trackedIGT;
            string lrt = (t.Hours > 0 ? t.Hours + ":" : "") +
                         (t.Minutes > 0 || t.Hours > 0 ? t.Minutes.ToString("00") + ":" : "") +
                         t.Seconds.ToString("00") + "." +
                         (t.Milliseconds / 10).ToString("00");
            lbl_lrt.Text = lrt;



            float hp = accessor.ReadSingle(0x795978);
            if (lastHp >= 0 && hp < lastHp)
            {
                totalDamage += (lastHp - hp);
                hitCount++;
                selectedsegment.CurrentHit++;
            }

            lastHp = hp;

            lbl_currhp.Text = "HP : " + hp;
            lbl_totaldmg.Text = "HP Lost : " + totalDamage;
            lbl_hitcount.Text = "Total Hit : " + hitCount;

            // read WADs
            var wad1 = accessor.ReadUInt32(0x335280);
            var wad2 = accessor.ReadUInt32(0x335284);

            byte[] buffer1 = new byte[0x50];
            byte[] buffer2 = new byte[0x50];

            accessor.ReadArray((long)wad1, buffer1, 0, buffer1.Length);
            accessor.ReadArray((long)wad2, buffer2, 0, buffer2.Length);

            string wad1text = Encoding.ASCII.GetString(buffer1).Split('\0')[0];
            string wad2text = Encoding.ASCII.GetString(buffer2).Split('\0')[0];

            // add if new and valid
            if (!string.IsNullOrWhiteSpace(wad1text) && wad1text != lastWad1)
            {
                if (selectedsegment != null && selectedsegment.CurrentHit <= selectedsegment.PBHit) // new PB HIT
                {
                    selectedsegment.PBHit = selectedsegment.CurrentHit;
                }
                segmenthit = 0; //reset segment hit
                if (!data.Any(x => x.SegmentCode == wad1text))
                {
                    data.Add(new SegmentTable { SegmentCode = wad1text, SegmentName = wad1text, CurrentHit = 0, PBHit = 6969 });
                }
                lastWad1 = wad1text;
                var seg = data.FirstOrDefault(x => x.SegmentCode == wad1text);
                if (seg != null)
                {
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 70, 63, 63);
                    }

                    dataGridView1.Rows[data.IndexOf(seg)].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 0, 0);
                    selectedsegmentindex = data.IndexOf(seg);
                    selectedsegment = seg;
                }
                IsFirstWad = true;
            }

            if (!string.IsNullOrWhiteSpace(wad2text) && wad2text != lastWad2)
            {
                if (selectedsegment != null && selectedsegment.CurrentHit <= selectedsegment.PBHit) // new PB HIT
                {
                    selectedsegment.PBHit = selectedsegment.CurrentHit;
                }
                segmenthit = 0;
                if (!data.Any(x => x.SegmentCode == wad2text))
                {
                    data.Add(new SegmentTable { SegmentCode = wad2text, SegmentName = wad2text, CurrentHit = 0, PBHit = 6969 });
                }
                lastWad2 = wad2text;
                var seg = data.FirstOrDefault(x => x.SegmentCode == wad2text);
                if (seg != null)
                {
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 70, 63, 63);
                    }

                    dataGridView1.Rows[data.IndexOf(seg)].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 0, 0);
                    selectedsegmentindex = data.IndexOf(seg);
                    selectedsegment = seg;
                }
                IsFirstWad = false;
            }
            dataGridView1.FirstDisplayedScrollingRowIndex = selectedsegmentindex;



            //lbl_wad1.Text = wad1text;
            dataGridView1.Refresh(); // to reflect in UI if needed
            //lbl_wad2.Text = wad2text;
        }
        private void AttachDragEvents(Control control)
        {
            control.MouseDown += MainForm_MouseDown;
            control.MouseClick += MainForm_MouseDown;
            foreach (Control child in control.Controls)
                AttachDragEvents(child);
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
            if (e.Button == MouseButtons.Right)
            {

                contextMenu.Show(this, e.Location);
            }
        }

        private void Debug_Click(object? sender, EventArgs e)
        {
            //accessor.Write(0x29CAEC, true); // open save menu
            // 2076D650 combo 
            accessor.WriteArray(0x26FF78, new byte[4] { 0x01, 0x00, 0x02, 0x24 }, 0, 4); // skip cutscene
            Task.Delay(1000).ContinueWith(_ =>
            {
                // 8C620000
                accessor.WriteArray(0x26FF78, new byte[4] { 0x00, 0x00, 0x062, 0x8C }, 0, 4); // restore
            });
            return;
        }

        private void export(object? sender, EventArgs e)
        {
            foreach (var item in data)
            {
                File.AppendAllText("splits.txt", $"{item.SegmentName}|{item.CurrentHit}|{item.PBHit}\n");
            }
        }

        private void Starttimer_Click(object? sender, EventArgs e)
        {
            timer.Start();
            timersec.Start();
        }

        private void FindProcess()
        {
            proc = Process.GetProcessesByName(procname).FirstOrDefault();
            if (proc == null)
            {
                lbl_status.Text = "PCSX2 not found";
                timer.Stop();
                return;
            }
            procHandle = OpenProcess(PROCESS_ALL_ACCESS, false, proc.Id);
            if (procHandle == IntPtr.Zero)
            {
                lbl_status.Text = "Failed to open process";
                timer.Stop();
                return;
            }
            pcsx2MapName = $"pcsx2_{proc.Id}";
            // Open the memory-mapped file
            mmf = MemoryMappedFile.OpenExisting(pcsx2MapName);
            accessor = mmf.CreateViewAccessor(0, MaxSharedSize, MemoryMappedFileAccess.ReadWrite);
            timer.Interval = 40;
            timersec.Interval = 1000;
            timer.Tick += Timer_Tick;
            timersec.Tick += TimerPersecond_Tick;
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {
                var cellValue = dataGridView1.Rows[e.RowIndex].Cells[3].Value?.ToString();
                if (cellValue == "6969")
                {
                    e.Value = "-";
                    e.FormattingApplied = true; // optional but ensures custom formatting is used
                }
            }
        }
    }
    public class SegmentTable
    {
        public string SegmentCode { get; set; }
        public string SegmentName { get; set; }
        public int CurrentHit { get; set; }
        public int PBHit { get; set; }
    }
}
