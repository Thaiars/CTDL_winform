using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameMovement
{
    public partial class frmMenu : Form
    {
        public frmMenu()
        {
            InitializeComponent();
        }

        private void Menu_Load(object sender, EventArgs e)
        {
            
        }

        void Setup()
        {
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Form1 formGame = new Form1();
            formGame.Show();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            DialogResult rs = MessageBox.Show("Xác Nhận Thoát?", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

            if (rs == DialogResult.OK)
            {
                Close();
            } 
                
        }
    }
}
