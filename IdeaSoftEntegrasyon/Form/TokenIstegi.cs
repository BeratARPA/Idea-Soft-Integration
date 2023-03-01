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
using HtmlAgilityPack;
using System.Diagnostics;

namespace IdeaSoftEntegrasyon
{
    public partial class TokenIstegi : Form
    {
        public TokenIstegi()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox1.Text != "")
                {
                    EntegrasyonBilgileri.Code = textBox1.Text;

                    var url = new Uri("https://" + EntegrasyonBilgileri.FirmaAdı + ".myideasoft.com/oauth/v2/token?grant_type=authorization_code&client_id=" + EntegrasyonBilgileri.ClientID + "&client_secret=" + EntegrasyonBilgileri.ClientSecret + "&code=" + EntegrasyonBilgileri.Code + "&redirect_uri=" + EntegrasyonBilgileri.YonlendirmeAdres + "");
                    var client = new WebClient();
                    var html = client.DownloadString(url);

                    var json = JsonConvert.DeserializeObject(html);
                    JObject jObject = JObject.Parse(json.ToString());

                    EntegrasyonBilgileri.AccessToken = (string)jObject.SelectToken("access_token");
                    EntegrasyonBilgileri.RefreshToken = (string)jObject.SelectToken("refresh_token");

                    Properties.Settings.Default.AccessToken = EntegrasyonBilgileri.AccessToken;
                    Properties.Settings.Default.RefreshToken = EntegrasyonBilgileri.RefreshToken;
                    Properties.Settings.Default.Save();                    

                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Boş alanı doldurunuz!", "UYARI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                timer1.Stop();
                if (timer1.Interval == 30000)
                {
                    MessageBox.Show("Code değerinin süresi bitmiş. Lütfen tekrardan yetkilendirme yapınız!", "UYARI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("-Code- değerini doğru girdiğinizden emin olunuz!" + Environment.NewLine + Environment.NewLine + ex.ToString(), "HATA");
                }
            }
        }

        private void TokenIstegi_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }

    }
}