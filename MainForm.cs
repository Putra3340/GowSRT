using Gma.System.MouseKeyHook;
using GowDamagelessSTD;
using System;
using System.CodeDom;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace CounterSplit
{

    public partial class MainForm : Form
    {
        public static BindingList<SegmentTable> data = new BindingList<SegmentTable>();
        #region PCSX2QT
        public const string procname = "pcsx2-qt";
        const int MaxSharedSize = 0x8B00000 + 0x300000 + 0x100000; // EE + IOP + VU (143MB)
        public static string pcsx2MapName = string.Empty;
        MemoryMappedFile? mmf = null;
        MemoryMappedViewAccessor accessor = null;
        bool IsFirstWad = true;
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer timer2 = new System.Windows.Forms.Timer();
        #endregion
        #region RPCS3
        public const string procname2 = "rpcs3";
        const long REGION_START = 0x300000000;
        const long REGION_END = 0x341FFFFFF;

        #endregion
        #region Process
        public Process proc;
        private IntPtr procHandle;
        private const uint PROCESS_ALL_ACCESS = 0x001F0FFF;
        private const uint FILE_MAP_ALL_ACCESS = 0x000F001F;
        private const uint PAGE_EXECUTE_READWRITE = 0x40;
        const int PROCESS_VM_READ = 0x0010;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        #endregion
        ContextMenuStrip contextMenu = new ContextMenuStrip();
        public System.Windows.Forms.Timer TimerRefresh = new System.Windows.Forms.Timer();
        public static SegmentTable selectedsegment = null;


        #region Split
        int segmenthit = 0;
        float lastHp = -1;
        float totalDamage = 0;
        int hitCount = 0;
        float lastIGT = -1;
        
        TimeSpan trackedIGT = TimeSpan.Zero;
        System.Windows.Forms.Timer timersec = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer timerdata = new System.Windows.Forms.Timer();
        int selectedsegmentindex = -1;
        #endregion
        #region Shared Address
        private long RPCS3_HP;
        private long RPCS3_IGT;
        private long RPCS3_WAD1;
        private long RPCS3_WAD2;
        private long PCSX2_HP;
        private long PCSX2_IGT;
        private long PCSX2_WAD1;
        private long PCSX2_WAD2;
        private string lastWad1 = "R_Shell"; // on main menu
        private string lastWad2 = "R_Shell"; // on main menu
        const int GOW1 = 1; // GoW1
        const int GOW2 = 2; // GoW2
        const int GOWCOO = 3; // GoW2
        const int GOWGOS = 4; // GoW2
        public int GameVersion = 0; // default to GoW1
        #endregion
        #region GoW1 Things
        const long GOW1_RPCS3_HP = 0x330A497C4;
        const long GOW1_RPCS3_IGT = 0x300646018;
        const long GOW1_RPCS3_WAD1 = 0x3308C6058;
        const long GOW1_RPCS3_WAD2 = 0x3308C60D0;
        const long GOW1_PCSX2_HP = 0x795978;
        const long GOW1_PCSX2_IGT = 0x302D0C;
        const long GOW1_PCSX2_WAD1 = 0x335280;
        const long GOW1_PCSX2_WAD2 = 0x335284;

        #endregion
        #region GoW2 Things
        const long GOW2_RPCS3_HP = 0x300672470;
        const long GOW2_RPCS3_IGT = 0x300671168;
        const long GOW2_RPCS3_WAD1 = 0x3307C215C;
        const long GOW2_RPCS3_WAD2 = 0x3307C2174;
        #endregion
        #region Keybind
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
        #endregion
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        const int KEYEVENTF_KEYUP = 0x0002;
        const byte VK_F6 = 0x75;

        void PressF6()
        {
            keybd_event(VK_F6, 0, 0, UIntPtr.Zero); // key down
            keybd_event(VK_F6, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // key up
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);
        #region Form

        public MainForm()
        {
            InitializeComponent();
            typeof(DataGridView).InvokeMember("DoubleBuffered",
    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
    null, dataGridView1, new object[] { true });

            globalHook = Hook.GlobalEvents();
            globalHook.KeyDown += GlobalHook_KeyDown;

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
            dataGridView1.Columns["PBDelta"].Visible = false;
            dataGridView1.Columns["SegmentName"].HeaderText = "Segment Name";
            dataGridView1.Columns["CurrentHit"].HeaderText = "Current Hit";
            dataGridView1.Columns["PBHit"].HeaderText = "PB";
            dataGridView1.DefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);


            ToolStripMenuItem config = new ToolStripMenuItem("Configuration");
            ToolStripMenuItem starttimer = new ToolStripMenuItem("Connect to PCSX2");
            ToolStripMenuItem starttimer2 = new ToolStripMenuItem("Connect to RPCS3");
            ToolStripMenuItem import = new ToolStripMenuItem("Import Split");
            ToolStripMenuItem dump = new ToolStripMenuItem("Export Split");
            ToolStripMenuItem clear = new ToolStripMenuItem("Clear Split");
            ToolStripMenuItem debug = new ToolStripMenuItem("Debug");
            ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
            config.Click += OpenConfig;
            exitItem.Click += (s, args) => Application.Exit();
            starttimer.Click += Starttimer_Click;
            starttimer2.Click += Starttimer2_Click;
            dump.Click += export;
            clear.Click += Clear_Click;
            debug.Click += Debug_Click;
            import.Click += ImportFile;
            contextMenu.Items.Add(config);
            contextMenu.Items.Add(starttimer);
            contextMenu.Items.Add(starttimer2);
            contextMenu.Items.Add(import);
            contextMenu.Items.Add(dump);
            contextMenu.Items.Add(clear);
            contextMenu.Items.Add(debug);
            contextMenu.Items.Add(exitItem);
            timer2.Interval = 20;
            timer2.Tick += Timer_Tick_RPCS3;
            timerdata.Interval = 100;
            timerdata.Tick += TimerRefreshData;
            timersec.Interval = 1000; // 1 second
            timersec.Tick += TimerPersecond_Tick;
            timer.Interval = 20;
            timer.Tick += PCSX2_Tick;
            timerdata.Start();
            AttachDragEvents(this);
        }
        private void OpenConfig(object? sender, EventArgs e)
        {
            ConfigForm configForm = new ConfigForm();
            configForm.ShowDialog();
        }
        private void Clear_Click(object? sender, EventArgs e)
        {
            data.Clear();
        }
        private void ImportFile(object? sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Import Split File"
            };
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string[] lines = File.ReadAllLines(openFileDialog.FileName);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('|');
                    if (parts.Length == 3)
                    {
                        string segmentName = parts[0];
                        int currentHit = 0;
                        int pbHit = int.Parse(parts[2]);
                        if (!data.Any(x => x.SegmentCode == segmentName))
                        {
                            data.Add(new SegmentTable { SegmentCode = segmentName, SegmentName = segmentName, CurrentHit = currentHit, PBHit = pbHit });
                        }
                    }
                }
            }
        }
        private void TimerPersecond_Tick(object sender, EventArgs e)
        {
            lbl_date.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        private void TimerRefreshData(object? sender, EventArgs e)
        {
            if (Config.SelectedGame == GOW1) {
                label1.Text = "God of War (2005)"; 
            }else if (Config.SelectedGame == GOW2)
            {
                label1.Text = "God of War II";
            }
            else if (Config.SelectedGame == GOWCOO)
            {
                label1.Text = "God of War: Chains of Olympus";
            }
            else if (Config.SelectedGame == GOWGOS)
            {
                label1.Text = "God of War: Ghost of Sparta";
            }
            label2.Text = $"{Config.SelectedDifficulty} - Damageless";
            dataGridView1.Invalidate();
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
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {

                var cellValue = dataGridView1.Rows[e.RowIndex].Cells[3].Value?.ToString();
                var cellValuede = dataGridView1.Rows[e.RowIndex].Cells[4].Value?.ToString();
                if (cellValue == "6969")
                {
                    e.Value = "-";
                    e.FormattingApplied = true; // optional but ensures custom formatting is used
                }
            }
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int lines = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
            int newFirstRow = dataGridView1.FirstDisplayedScrollingRowIndex - lines;

            if (newFirstRow < 0) newFirstRow = 0;
            if (newFirstRow >= dataGridView1.RowCount) newFirstRow = dataGridView1.RowCount - 1;

            if(newFirstRow > 0)
                dataGridView1.FirstDisplayedScrollingRowIndex = newFirstRow;
        }
        #endregion
        private void Debug_Click(object? sender, EventArgs e)
        {
            //accessor.Write(0x29CAEC, true); // open save menu
            // 2076D650 combo 
            //accessor.WriteArray(0x26FF78, new byte[4] { 0x01, 0x00, 0x02, 0x24 }, 0, 4); // skip cutscene
            //Task.Delay(1000).ContinueWith(_ =>
            //{
            //    // 8C620000
            //    accessor.WriteArray(0x26FF78, new byte[4] { 0x00, 0x00, 0x062, 0x8C }, 0, 4); // restore
            //});
            //return;

            foreach (var x in data)
            {
                x.CurrentHit = 0;
            }
            //byte[] buffer = new byte[MaxSharedSize];
            //accessor.ReadArray(0, buffer, 0, buffer.Length);
            ////File.WriteAllBytes("dump.bin", buffer);
            //byte[] buffer = File.ReadAllBytes("dump.bin");
            //accessor.WriteArray(0, buffer, 0, buffer.Length);
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
            if (Config.SelectedGame == GOW1)
            {
                PCSX2_HP = GOW1_PCSX2_HP;
                PCSX2_IGT = GOW1_PCSX2_IGT;
                PCSX2_WAD1 = GOW1_PCSX2_WAD1;
                PCSX2_WAD2 = GOW1_PCSX2_WAD2;
            }
                FindPCSX2();
            timer.Start();
            timersec.Start();
        }
        private void Starttimer2_Click(object? sender, EventArgs e)
        {
            #region Setup RPCS3 Addresses
            if(Config.SelectedGame == GOW1)
            {
                RPCS3_HP = GOW1_RPCS3_HP;
                RPCS3_IGT = GOW1_RPCS3_IGT;
                RPCS3_WAD1 = GOW1_RPCS3_WAD1;
                RPCS3_WAD2 = GOW1_RPCS3_WAD2;
            }else if(Config.SelectedGame == GOW2)
            {
                RPCS3_HP = GOW2_RPCS3_HP;
                RPCS3_IGT = GOW2_RPCS3_IGT;
                RPCS3_WAD1 = GOW2_RPCS3_WAD1;
                RPCS3_WAD2 = GOW2_RPCS3_WAD2;
            }
            #endregion
            FindRpcs3();
            timer2.Start();
            timersec.Start();
        }
        private void FindPCSX2()
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
            mmf = MemoryMappedFile.OpenExisting(pcsx2MapName);
            accessor = mmf.CreateViewAccessor(0, MaxSharedSize, MemoryMappedFileAccess.ReadWrite);
        }
        private void FindRpcs3()
        {
            foreach (var process in Process.GetProcessesByName("rpcs3"))
            {
                procHandle = OpenProcess(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION, false, process.Id);
            }
        }
        private async void Timer_Tick_RPCS3(object? sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            float floored = ReadFloatBig(RPCS3_IGT);
            if (floored < 0)
                floored = 0;
            if (floored > 23872363824)
                floored = 0; // reset if IGT is too high, this is a safety measure
            if (Config.SelectedGame == GOW2)
            {
                floored = Read4BytesBig(RPCS3_IGT); // GoW2 uses 4 bytes for IGT 
            }
            TimeSpan rawTime = TimeSpan.FromSeconds(floored);
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
                if (delta > 0 && delta < 10)
                {
                    trackedIGT += TimeSpan.FromSeconds(delta);
                }
            }
            lastIGT = floored;
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
            float hp = ReadFloatBig(RPCS3_HP);
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
            var wad1text = ReadString(RPCS3_WAD1,20);
            var wad2text = ReadString(RPCS3_WAD2, 20);
            if (!string.IsNullOrWhiteSpace(wad1text) && wad1text != lastWad1)
            {
                if (selectedsegment != null && selectedsegment.CurrentHit <= selectedsegment.PBHit)
                {
                    selectedsegment.PBDelta = selectedsegment.PBHit - selectedsegment.CurrentHit;
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

                    dataGridView1.Rows[data.IndexOf(seg)].DefaultCellStyle.BackColor = Color.FromArgb(255, 0, 120, 215);
                    selectedsegmentindex = data.IndexOf(seg);
                    selectedsegment = seg;
                }
                IsFirstWad = true;
                if (data.Count != 0)
                {
                    dataGridView1.FirstDisplayedScrollingRowIndex = selectedsegmentindex;
                }
            }
            if (!string.IsNullOrWhiteSpace(wad2text) && wad2text != lastWad2)
            {
                if (selectedsegment != null && selectedsegment.CurrentHit <= selectedsegment.PBHit) // new PB HIT
                {
                    selectedsegment.PBDelta = selectedsegment.PBHit - selectedsegment.CurrentHit;
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

                    dataGridView1.Rows[data.IndexOf(seg)].DefaultCellStyle.BackColor = Color.FromArgb(255, 0, 120, 215);
                    selectedsegmentindex = data.IndexOf(seg);
                    selectedsegment = seg;
                }
                IsFirstWad = false;
                if (data.Count != 0)
                {
                    dataGridView1.FirstDisplayedScrollingRowIndex = selectedsegmentindex;
                }
            }
            int best = 0;
            foreach(var x in data)
            {
                if(x.PBHit < 6969 && x.PBHit >= 0) // valid PB
                    best = best + x.PBHit;
            }
            label10.Text = $"Best : {best}";
        }
        private async void PCSX2_Tick(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            float floored = accessor.ReadSingle(PCSX2_IGT);
            TimeSpan rawTime = TimeSpan.FromSeconds(floored);
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
                if (delta > 0 && delta < 10)
                {
                    trackedIGT += TimeSpan.FromSeconds(delta);
                }
            }
            lastIGT = floored;
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
            float hp = accessor.ReadSingle(PCSX2_HP);
            if (lastHp >= 0 && hp < lastHp)
            {
                totalDamage += (lastHp - hp);
                hitCount++;
                selectedsegment.CurrentHit++;
                if(Config.OHKO)
                {
                    accessor.WriteArray(PCSX2_HP, new byte[8], 0, 8);
                }
                if(Config.OHCRASH)
                {
                    PressF6();
                }
            }
            lastHp = hp;
            lbl_currhp.Text = "HP : " + hp;
            lbl_totaldmg.Text = "HP Lost : " + totalDamage;
            lbl_hitcount.Text = "Total Hit : " + hitCount;
            var wad1 = accessor.ReadUInt32(PCSX2_WAD1);
            var wad2 = accessor.ReadUInt32(PCSX2_WAD2);

            byte[] buffer1 = new byte[0x50];
            byte[] buffer2 = new byte[0x50];

            accessor.ReadArray((long)wad1, buffer1, 0, buffer1.Length);
            accessor.ReadArray((long)wad2, buffer2, 0, buffer2.Length);

            string wad1text = Encoding.ASCII.GetString(buffer1).Split('\0')[0];
            string wad2text = Encoding.ASCII.GetString(buffer2).Split('\0')[0];

            if (!string.IsNullOrWhiteSpace(wad1text) && wad1text != lastWad1)
            {
                if (selectedsegment != null && selectedsegment.CurrentHit <= selectedsegment.PBHit)
                {
                    selectedsegment.PBHit = selectedsegment.CurrentHit;
                }
                segmenthit = 0;
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
                if (data.Count != 0)
                {
                    dataGridView1.FirstDisplayedScrollingRowIndex = selectedsegmentindex;
                }
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
                if (data.Count != 0)
                {
                    dataGridView1.FirstDisplayedScrollingRowIndex = selectedsegmentindex;
                }
            }
        } // PCSX2 Timer Tick
        #region RPCS3 Read Functions

        public float ReadFloatBig(long offset)
        {
            if (offset < REGION_START || offset > REGION_END) throw new ArgumentOutOfRangeException("Invalid memory region");
            byte[] buffer = new byte[4];
            ReadProcessMemory(procHandle, new IntPtr(offset), buffer, buffer.Length, out _);
            buffer = buffer.Reverse().ToArray(); // Reverse for Big Endian
            return BitConverter.ToSingle(buffer, 0);
        }
        public string ReadString(long offset, int length)
        {
            if (offset < REGION_START || offset + length > REGION_END) throw new ArgumentOutOfRangeException("Invalid memory region");
            byte[] buffer = new byte[length];
            ReadProcessMemory(procHandle, new IntPtr(offset), buffer, buffer.Length, out _);
            return Encoding.UTF8.GetString(buffer).TrimEnd('\0').Split("\0")[0];
        }
        public int Read4BytesBig(long offset)
        {
            if (offset < REGION_START || offset > REGION_END - 3) throw new ArgumentOutOfRangeException("Invalid memory region");
            byte[] buffer = new byte[4];
            ReadProcessMemory(procHandle, new IntPtr(offset), buffer, buffer.Length, out _);
            buffer = buffer.Reverse().ToArray(); // Big-endian
            return BitConverter.ToInt32(buffer,0);
        }

        #endregion
    }
    public class SegmentTable
    {
        public string SegmentCode { get; set; }
        public string SegmentName { get; set; }
        public int CurrentHit { get; set; }
        public int PBHit { get; set; }
        public int PBDelta { get; set; }
    }
}
