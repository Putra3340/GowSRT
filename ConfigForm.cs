using CounterSplit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GowDamagelessSTD
{
    public partial class ConfigForm : Form
    {
        public ConfigForm()
        {
            InitializeComponent();
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = Config.SelectedGame - 1;
            comboBox2.SelectedIndex = Config.SelectedPlatform - 1;
            comboBox3.SelectedItem = Config.SelectedDifficulty;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1 || comboBox2.SelectedIndex == -1 || comboBox3.SelectedIndex == -1)
            {
                MessageBox.Show("Please select all");
                return;
            }

            Config.SelectedGame = comboBox1.SelectedIndex + 1;
            Config.SelectedPlatform = comboBox2.SelectedIndex + 1;
            Config.SelectedDifficulty = comboBox3.SelectedItem.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Config.OHKO = checkBox1.Checked;
            Config.OHCRASH = checkBox2.Checked;
        }
    }
}
