using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Web;

namespace IdeaSoftEntegrasyon
{
    public partial class GirisAyarlar : Form
    {
        public GirisAyarlar()
        {
            InitializeComponent();
        }

        TokenIstegi TokenIstek = new TokenIstegi();
        Form1 form = new Form1();

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "" && textBox4.Text != "")
                {
                    Properties.Settings.Default.Firma = textBox1.Text;
                    Properties.Settings.Default.ClientID = textBox2.Text;
                    Properties.Settings.Default.Adres = textBox3.Text;
                    Properties.Settings.Default.ClientSecret = textBox4.Text;
                    Properties.Settings.Default.Save();

                    MessageBox.Show("Entegrasyon Bilgileri Kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Boş alanları doldurunuz!", "UYARI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void GirisAyarlar_Load(object sender, EventArgs e)
        {
            textBox1.Text = Properties.Settings.Default.Firma;
            textBox2.Text = Properties.Settings.Default.ClientID;
            textBox3.Text = Properties.Settings.Default.Adres;
            textBox4.Text = Properties.Settings.Default.ClientSecret;
        }

    }
}