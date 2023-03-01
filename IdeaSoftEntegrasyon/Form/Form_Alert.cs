using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IdeaSoftEntegrasyon
{
    public partial class Form_Alert : Form
    {
        public Form_Alert()
        {
            InitializeComponent();
        }

        SoundPlayer simpleSound = new SoundPlayer(Application.StartupPath + "\\BildirimSes.wav");

        //////////Fonksiyonlar//////////

        public enum enmAction
        {
            wait,
            start,
            close
        }

        public enum enmType
        {
            Info
        }

        public Form_Alert.enmAction action;
        private int x, y;

        ////////////////////

        //////////Start,wait,close işlemleri//////////

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.TopMost = true;
            switch (this.action)
            {
                case enmAction.wait:

                    simpleSound.Play();
                    timer1.Interval = 10000;
                    action = enmAction.close;
                    break;

                case Form_Alert.enmAction.start:

                    this.timer1.Interval = 1;
                    this.Opacity += 0.1;
                    if (this.x < this.Location.X)
                    {
                        this.Left--;
                    }
                    else
                    {
                        if (this.Opacity == 1.0)
                        {
                            action = Form_Alert.enmAction.wait;
                        }
                    }
                    break;

                case enmAction.close:

                    timer1.Interval = 1;
                    this.Opacity -= 0.1;

                    this.Left += 3;
                    if (base.Opacity == 0.0)
                    {
                        base.Close();
                    }
                    simpleSound.Stop();
                    break;
            }
        }

        ////////////////////

        //////////Bildirim tıklama//////////

        private void Form_Alert_Click(object sender, EventArgs e)
        {
            foreach (Form itemx in Application.OpenForms)
            {
                if (itemx.Name == "Form1")
                {
                    Form1 frm = (Form1)itemx;
                    frm.WindowState = FormWindowState.Maximized;
                    frm.tabControl1.SelectedIndex = 2;
                    break;
                }
            }
            action = Form_Alert.enmAction.close;
            this.timer1.Interval = 1;
        }

        ////////////////////

        //////////Bildirim konumlandırma//////////

        public void showAlert(string msg, enmType type)
        {
            this.Opacity = 0.0;
            this.StartPosition = FormStartPosition.Manual;
            string fname;

            for (int i = 1; i < 10; i++)
            {
                fname = "alert" + i.ToString();
                Form_Alert frm = (Form_Alert)Application.OpenForms[fname];

                if (frm == null)
                {
                    this.Name = fname;
                    this.x = Screen.PrimaryScreen.WorkingArea.Width - this.Width + 15;
                    this.y = Screen.PrimaryScreen.WorkingArea.Height - this.Height * i - 5 * i;
                    this.Location = new Point(this.x, this.y);
                    break;
                }
            }
            this.x = Screen.PrimaryScreen.WorkingArea.Width - base.Width - 5;

            switch (type)
            {
                case enmType.Info:
                    this.pictureBox1.Image = Properties.Resources.Info;
                    this.BackColor = Color.RoyalBlue;
                    break;
            }

            this.lblMsg.Text = msg;

            this.Show();
            this.action = enmAction.start;
            this.timer1.Interval = 1;
            this.timer1.Start();
        }

        ////////////////////

    }
}