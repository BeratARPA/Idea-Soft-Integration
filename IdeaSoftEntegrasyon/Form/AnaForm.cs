using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Web;
using System.Data.SqlClient;
using System.Net;
using System.Runtime.ExceptionServices;
using RestSharp;
using System.ComponentModel;
using System.Drawing;
using HtmlAgilityPack;
using System.Media;
using System.Drawing.Printing;
using FastReport;

namespace IdeaSoftEntegrasyon
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public byte[] byteArray;
        public int SiparisSayi = 0, DataGridSayı;
        public string HataKaynak;
        DateTime BGWSayac;
        IdeaSoft IdeaSoft = new IdeaSoft();
        Siparis SipForm = new Siparis();
        AdisyonKaydet ak = new AdisyonKaydet();
        order SipClass = new order();
        SepetEntities db = new SepetEntities();
        List<UrunBilgiler> menuList = new List<UrunBilgiler>();
        List<Kategorıler> kategoriList = new List<Kategorıler>();

        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.ShowBalloonTip(1, "Şefim Sepet", "Program çalışıyor!", ToolTipIcon.Info);
            IdeaSoft.IdeaLogin();

            Timer_Token.Start();
            Timer_SiparisKontrol.Start();

            //GridSiparisDoldur();

            tabControl1.SelectedIndex = 2;

            EntegrasyonBilgileri.FirmaAdı = Properties.Settings.Default.Firma;
            EntegrasyonBilgileri.ClientID = Properties.Settings.Default.ClientID;
            EntegrasyonBilgileri.YonlendirmeAdres = Properties.Settings.Default.Adres;
            EntegrasyonBilgileri.ClientSecret = Properties.Settings.Default.ClientSecret;
            EntegrasyonBilgileri.AccessToken = Properties.Settings.Default.AccessToken;
            EntegrasyonBilgileri.RefreshToken = Properties.Settings.Default.RefreshToken;
        }

        //////////Bildirim özellikleri diğer forma gönderme metotu//////////

        public void Alert(string msg, Form_Alert.enmType type)
        {
            Form_Alert frm = new Form_Alert();
            frm.showAlert(msg, type);
        }

        ////////////////////

        //////////Kimlik Doğrulama Kodu İsteği//////////

        private void button2_Click(object sender, EventArgs e)
        {
            TokenIstegi TokenIstek = new TokenIstegi();

            // Token almak için kullanılacak kimlik doğrulama(Code) kodu. Geçerlilik süresi 30 saniyedir.
            System.Diagnostics.Process.Start("https://" + EntegrasyonBilgileri.FirmaAdı + ".myideasoft.com/admin/user/auth?client_id=" + EntegrasyonBilgileri.ClientID + "&response_type=code&state=2b33fdd45jbevd6nam&redirect_uri=" + EntegrasyonBilgileri.YonlendirmeAdres + "");

            this.WindowState = FormWindowState.Minimized;
            TokenIstek.ShowDialog();
        }

        ////////////////////

        //////////Entegrasyon Bilgileri//////////
        private void button3_Click(object sender, EventArgs e)
        {
            GirisAyarlar ga = new GirisAyarlar();
            ga.ShowDialog();
        }

        ////////////////////

        //////////Teslimler//////////

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //////////Ürünler//////////

            if (tabControl1.SelectedIndex == 0)
            {
                dataGridView3.Rows.Clear();
                var urun = db.UrunBilgi.ToList();
                for (int i = 0; i < urun.Count; i++)
                {
                    dataGridView3.Rows.Add();
                    dataGridView3.Rows[i].Cells[0].Value = urun[i].UrunID;
                    dataGridView3.Rows[i].Cells[1].Value = urun[i].UrunAdı;
                    dataGridView3.Rows[i].Cells[2].Value = urun[i].UrunFiyat;
                }
            }

            //////////Kategoriler//////////

            if (tabControl1.SelectedIndex == 1)
            {
                dataGridView4.Rows.Clear();
                var kategori = db.Kategoriler.ToList();
                for (int i = 0; i < kategori.Count; i++)
                {
                    dataGridView4.Rows.Add();
                    dataGridView4.Rows[i].Cells[0].Value = kategori[i].KategoriID;
                    dataGridView4.Rows[i].Cells[1].Value = kategori[i].KategoriAdi;
                }
            }

            //////////Siparişler//////////

            if (tabControl1.SelectedIndex == 2)
            {
                DataGridSayı = 1;
                GridSiparisDoldur();
            }

            //////////Teslimler//////////

            if (tabControl1.SelectedIndex == 3)
            {
                DataGridSayı = 2;
                GridTeslimDoldur();
            }

            //////////İptaller//////////

            if (tabControl1.SelectedIndex == 4)
            {
                DataGridSayı = 5;
                GridİptalDoldur();
            }
        }

        ////////////////////

        //////////Notify Icon//////////

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
        }

        ////////////////////

        //////////Sipariş Kontrol//////////

        private class BGWAktar
        {
            public string siparis;
            public byte tip;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //////////Siparis Kontrol//////////

            HataKaynak = "1";

            if (IdeaSoft.IdeaAktifMi)
            {
                string s = IdeaSoft.SiparisSorgula();
                if (s != null)
                {
                    HataKaynak = "2";
                    BGWAktar nesne = new BGWAktar();
                    nesne.siparis = s;
                    nesne.tip = 0;
                    backgroundWorker_SiparisKontrol.ReportProgress(0, nesne);
                }
            }
        }

        //////////Access,Refresh Token alma//////////

        private void Timer_Token_Tick(object sender, EventArgs e)
        {
            if (Timer_Token.Interval == 600000)
            {
                var url = new Uri("https://" + EntegrasyonBilgileri.FirmaAdı + ".myideasoft.com/oauth/v2/token?grant_type=refresh_token&client_id=" + EntegrasyonBilgileri.ClientID + "&client_secret=" + EntegrasyonBilgileri.ClientSecret + "&refresh_token=" + EntegrasyonBilgileri.RefreshToken + "");
                var client = new WebClient();
                var html = client.DownloadString(url);

                var json = JsonConvert.DeserializeObject(html);
                JObject jObject = JObject.Parse(json.ToString());

                EntegrasyonBilgileri.AccessToken = (string)jObject.SelectToken("access_token");
                EntegrasyonBilgileri.RefreshToken = (string)jObject.SelectToken("refresh_token");

                Properties.Settings.Default.AccessToken = EntegrasyonBilgileri.AccessToken;
                Properties.Settings.Default.RefreshToken = EntegrasyonBilgileri.RefreshToken;
                Properties.Settings.Default.Save();
            }
        }

        private void Timer_SiparisKontrol_Tick(object sender, EventArgs e)
        {
            if (Timer_SiparisKontrol.Interval == 10000)
            {
                if (!backgroundWorker_SiparisKontrol.IsBusy)
                {
                    BGWSayac = DateTime.Now;
                    backgroundWorker_SiparisKontrol.RunWorkerAsync();
                }
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            List<order> orderlist = new List<order>();
            BGWAktar veri = (BGWAktar)e.UserState;
            orderlist = IdeaSoft.SiparisDondur(veri.siparis);
            SiparisKaydet(orderlist, veri.siparis);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            GC.Collect();
        }

        public void SiparisKaydet(List<order> SipList, string mesaj)
        {
            int sonuc = ak.AdisyonK(SipList, mesaj);
            if (sonuc == 0)
            {
                this.Alert("Yeni sipariş var!", Form_Alert.enmType.Info);
                Timer_SiparisKontrol.Start();
                GridSiparisDoldur();
            }
        }

        public void GridİptalDoldur()
        {
            dataGridView5.Rows.Clear();
            int j = 0;
            var AdisyonListesi = db.Adisyonlar.Where(y => y.AktifMi == false && y.Ret == true).OrderByDescending(y => y.ID).ToList();
            for (int i = 0; i < AdisyonListesi.Count; i++)
            {
                dataGridView5.Rows.Add();
                dataGridView5.Rows[i].Cells[0].Value = AdisyonListesi[i].SiparisID;
                dataGridView5.Rows[i].Cells[1].Value = AdisyonListesi[i].MusteriAdSoyad;
                dataGridView5.Rows[i].Cells[2].Value = AdisyonListesi[i].Telefon;
                dataGridView5.Rows[i].Cells[3].Value = AdisyonListesi[i].Adres;
                dataGridView5.Rows[i].Cells[4].Value = AdisyonListesi[i].Odeme;
                dataGridView5.Rows[i].Cells[5].Value = AdisyonListesi[i].Tarih;
                dataGridView5.Rows[i].Cells[6].Value = AdisyonListesi[i].Durum;
                dataGridView5.Rows[i].Cells[7].Value = Convert.ToDouble(AdisyonListesi[i].SiparisToplam).ToString("#,##0.00") + " TL";

                dataGridView5.Rows[j].Cells[0].Style.BackColor = Color.Red;
                dataGridView5.Rows[j].Cells[1].Style.BackColor = Color.Red;
                dataGridView5.Rows[j].Cells[2].Style.BackColor = Color.Red;
                dataGridView5.Rows[j].Cells[3].Style.BackColor = Color.Red;
                dataGridView5.Rows[j].Cells[4].Style.BackColor = Color.Red;
                dataGridView5.Rows[j].Cells[5].Style.BackColor = Color.Red;
                dataGridView5.Rows[j].Cells[6].Style.BackColor = Color.Red;
                dataGridView5.Rows[j].Cells[7].Style.BackColor = Color.Red;

                label1.Text = "Toplam: " + AdisyonListesi.Sum(s => double.Parse(s.SiparisToplam)).ToString("#,##0.00") + " TL";
                label2.Text = "Sipariş Sayısı: " + AdisyonListesi.Count;

                dataGridView5.ForeColor = Color.White;
                j++;
            }
        }

        public void GridTeslimDoldur()
        {
            dataGridView2.Rows.Clear();
            int j = 0;
            var AdisyonListesi = db.Adisyonlar.Where(y => y.AktifMi == false && y.Teslim == true).OrderByDescending(y => y.ID).ToList();
            for (int i = 0; i < AdisyonListesi.Count; i++)
            {
                dataGridView2.Rows.Add();
                dataGridView2.Rows[i].Cells[0].Value = AdisyonListesi[i].SiparisID;
                dataGridView2.Rows[i].Cells[1].Value = AdisyonListesi[i].MusteriAdSoyad;
                dataGridView2.Rows[i].Cells[2].Value = AdisyonListesi[i].Telefon;
                dataGridView2.Rows[i].Cells[3].Value = AdisyonListesi[i].Adres;
                dataGridView2.Rows[i].Cells[4].Value = AdisyonListesi[i].Odeme;
                dataGridView2.Rows[i].Cells[5].Value = AdisyonListesi[i].Tarih;
                dataGridView2.Rows[i].Cells[6].Value = AdisyonListesi[i].Durum;
                dataGridView2.Rows[i].Cells[7].Value = Convert.ToDouble(AdisyonListesi[i].SiparisToplam).ToString("#,##0.00") + " TL";

                dataGridView2.Rows[j].Cells[0].Style.BackColor = Color.Green;
                dataGridView2.Rows[j].Cells[1].Style.BackColor = Color.Green;
                dataGridView2.Rows[j].Cells[2].Style.BackColor = Color.Green;
                dataGridView2.Rows[j].Cells[3].Style.BackColor = Color.Green;
                dataGridView2.Rows[j].Cells[4].Style.BackColor = Color.Green;
                dataGridView2.Rows[j].Cells[5].Style.BackColor = Color.Green;
                dataGridView2.Rows[j].Cells[6].Style.BackColor = Color.Green;
                dataGridView2.Rows[j].Cells[7].Style.BackColor = Color.Green;

                label1.Text = "Toplam: " + AdisyonListesi.Sum(s => double.Parse(s.SiparisToplam)).ToString("#,##0.00") + " TL";
                label2.Text = "Sipariş Sayısı: " + AdisyonListesi.Count;

                dataGridView2.ForeColor = Color.White;
                j++;
            }
        }

        public void GridSiparisDoldur()
        {
            dataGridView1.Rows.Clear();
            int j = 0;
            var AdisyonListesi = db.Adisyonlar.Where(y => y.AktifMi == true).OrderByDescending(y => y.ID).ToList();
            for (int i = 0; i < AdisyonListesi.Count; i++)
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0].Value = AdisyonListesi[i].SiparisID;
                dataGridView1.Rows[i].Cells[1].Value = AdisyonListesi[i].MusteriAdSoyad;
                dataGridView1.Rows[i].Cells[2].Value = AdisyonListesi[i].Telefon;
                dataGridView1.Rows[i].Cells[3].Value = AdisyonListesi[i].Adres;
                dataGridView1.Rows[i].Cells[4].Value = AdisyonListesi[i].Odeme;
                dataGridView1.Rows[i].Cells[5].Value = AdisyonListesi[i].Tarih;
                dataGridView1.Rows[i].Cells[6].Value = AdisyonListesi[i].Durum;
                dataGridView1.Rows[i].Cells[7].Value = Convert.ToDouble(AdisyonListesi[i].SiparisToplam).ToString("#,##0.00") + " TL";

                dataGridView1.Rows[j].Cells[0].Style.BackColor = Color.Orange;
                dataGridView1.Rows[j].Cells[1].Style.BackColor = Color.Orange;
                dataGridView1.Rows[j].Cells[2].Style.BackColor = Color.Orange;
                dataGridView1.Rows[j].Cells[3].Style.BackColor = Color.Orange;
                dataGridView1.Rows[j].Cells[4].Style.BackColor = Color.Orange;
                dataGridView1.Rows[j].Cells[5].Style.BackColor = Color.Orange;
                dataGridView1.Rows[j].Cells[6].Style.BackColor = Color.Orange;
                dataGridView1.Rows[j].Cells[7].Style.BackColor = Color.Orange;
                dataGridView1.Rows[j].Cells[8].Style.BackColor = Color.Orange;

                label1.Text = "Toplam: " + AdisyonListesi.Sum(s => double.Parse(s.SiparisToplam)).ToString("#,##0.00") + " TL";
                label2.Text = "Sipariş Sayısı: " + AdisyonListesi.Count;

                dataGridView1.ForeColor = Color.Black;
                j++;
            }
        }

        public void MenuCevir(DataSet Ds)
        {
            menuList = new List<UrunBilgiler>();
            UrunBilgiler menu;

            for (int i = 0; i < Ds.Tables[0].Rows.Count; i++)
            {
                menu = new UrunBilgiler();
                menu.ID = Ds.Tables[0].Rows[i].ItemArray[0].ToString();
                menu.Name = Ds.Tables[0].Rows[i].ItemArray[1].ToString();
                menu.Price = Ds.Tables[0].Rows[i].ItemArray[2].ToString();
                menuList.Add(menu);
            }
        }

        public void KategoriCevir(DataSet Ds)
        {
            kategoriList = new List<Kategorıler>();
            Kategorıler Kategori;

            for (int i = 0; i < Ds.Tables[0].Rows.Count; i++)
            {
                Kategori = new Kategorıler();
                Kategori.KategoriID = Ds.Tables[0].Rows[i].ItemArray[0].ToString();
                Kategori.KategoriAdı = Ds.Tables[0].Rows[i].ItemArray[1].ToString();
                kategoriList.Add(Kategori);
            }
        }

        private void backgroundWorker_Menu_DoWork(object sender, DoWorkEventArgs e)
        {
            if (IdeaSoft.IdeaAktifMi)
            {
                MenuCevir(IdeaSoft.Urunler());
                KategoriCevir(IdeaSoft.Kategoriler());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("" + EntegrasyonBilgileri.FirmaAdı.ToUpper() + " Ürünleri ve kategorileri eklenecektir!", "Bilgi", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
            {
                if (!backgroundWorker_Menu.IsBusy)
                {
                    backgroundWorker_Menu.RunWorkerAsync();
                }
                ak.UrunBilgi(menuList);
                ak.KategoriKontrol(kategoriList);
            }
        }

        public void AdisyonYazdır()
        {
            string SiparisID = dataGridView1.CurrentRow.Cells[0].Value.ToString();
            var adisyon = db.Adisyonlar.Where(x => x.SiparisID == SiparisID).ToList();
            var urunu = db.Urunler.Where(y => y.SiparisID == SiparisID).ToList();

            PrinterSettings settings = new PrinterSettings();
            string defaultPrinterName = settings.PrinterName;

            Report report = new Report();

            report.Load(Application.StartupPath + "\\IdeaSepet.frx");
            report.Dictionary.Connections[0].ConnectionString = "Data Source=" + Environment.MachineName + "\\SQLEXPRESS;AttachDbFilename=;Initial Catalog=SefimSepetIdea;Integrated Security=True;Persist Security Info=False;User ID=;Password=";

            TextObject j = new TextObject();
            j = (TextObject)report.FindObject("txt_FirmaAd");
            j.Text = EntegrasyonBilgileri.FirmaAdı;

            j = new TextObject();
            j = (TextObject)report.FindObject("txt_Tarih");
            j.Text = adisyon[0].Tarih.ToString();

            j = new TextObject();
            j = (TextObject)report.FindObject("txt_AdSoyad");
            j.Text = adisyon[0].MusteriAdSoyad;

            j = new TextObject();
            j = (TextObject)report.FindObject("txt_tel");
            j.Text = adisyon[0].Telefon;

            j = new TextObject();
            j = (TextObject)report.FindObject("txt_Adres");
            j.Text = adisyon[0].Adres;

            j = new TextObject();
            j = (TextObject)report.FindObject("txt_Toplam");
            j.Text = Convert.ToDecimal(adisyon[0].SiparisToplam).ToString("#,##0.00") + " TL";

            j = new TextObject();
            j = (TextObject)report.FindObject("txt_Aratoplam");
            j.Text = Convert.ToDecimal(adisyon[0].AraToplam).ToString("#,##0.00") + " TL";

            j = new TextObject();
            j = (TextObject)report.FindObject("txt_kdv");
            j.Text = Convert.ToDecimal(adisyon[0].KDV).ToString("#,##0.00") + " TL";

            foreach (var item in urunu)
            {
                TextObject u = new TextObject();
                u = (TextObject)report.FindObject("txt_UrunAd");
                u.Text += Environment.NewLine + "-" + item.UrunAdı + Environment.NewLine;

                u = new TextObject();
                u = (TextObject)report.FindObject("txt_UrunAdet");
                u.Text += Environment.NewLine + item.Adet + Environment.NewLine;

                u = new TextObject();
                u = (TextObject)report.FindObject("txt_UrunFiyat");
                decimal uFiyat = Convert.ToDecimal(item.UrunFiyat) * Convert.ToDecimal(item.Adet);
                u.Text += Environment.NewLine + uFiyat.ToString("#,##0.00") + Environment.NewLine;
            }

            j = new TextObject();
            j = (TextObject)report.FindObject("txt_SipNot");
            j.Text = adisyon[0].SiparisNot;

            report.Prepare();
            report.PrintSettings.ShowDialog = false;
            report.PrintSettings.Printer = defaultPrinterName;
            report.Print();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            GC.Collect();
            if (dataGridView1.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.ColumnIndex == 8)
            {
                try
                {
                    AdisyonYazdır();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Yazıcı hatası oluştu! \n\n" + ex.ToString(), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (dataGridView1.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.ColumnIndex == 6)
            {
                string SiparisID = dataGridView1.CurrentRow.Cells[0].Value.ToString();
                var DurumID = db.Adisyonlar.Where(x => x.SiparisID == SiparisID).Select(s => new { s.DurumID }).ToList();
                var update = db.Adisyonlar.Where(x => x.SiparisID == SiparisID).ToList();

                if (dataGridView1.Rows[e.RowIndex].Cells[6].Value.ToString() == "Onay Bekliyor")
                {
                    dataGridView1.Rows[e.RowIndex].Cells[6].Value = "Hazırlanıyor";
                    IdeaSoft.SiparisDurum(DurumID[0].DurumID, "being_prepared");
                    update[0].Kabul = false;
                    update[0].Ret = false;
                    update[0].Hazirlaniyor = true;
                    update[0].Yolda = false;
                    update[0].Teslim = false;
                    update[0].Durum = "Hazırlanıyor";
                    db.SaveChanges();
                }
                else if (dataGridView1.Rows[e.RowIndex].Cells[6].Value.ToString() == "Hazırlanıyor")
                {
                    dataGridView1.Rows[e.RowIndex].Cells[6].Value = "Yola Çıkarıldı";
                    IdeaSoft.SiparisDurum(DurumID[0].DurumID, "fulfilled");
                    update[0].Kabul = false;
                    update[0].Ret = false;
                    update[0].Hazirlaniyor = false;
                    update[0].Yolda = true;
                    update[0].Teslim = false;
                    update[0].Durum = "Yola Çıkarıldı";
                    db.SaveChanges();
                }
                else if (dataGridView1.Rows[e.RowIndex].Cells[6].Value.ToString() == "Yola Çıkarıldı")
                {
                    dataGridView1.Rows[e.RowIndex].Cells[6].Value = "Teslim Edildi";
                    IdeaSoft.SiparisDurum(DurumID[0].DurumID, "delivered");
                    update[0].Kabul = false;
                    update[0].Ret = false;
                    update[0].Hazirlaniyor = false;
                    update[0].Yolda = false;
                    update[0].Teslim = true;
                    update[0].AktifMi = false;
                    update[0].Durum = "Teslim Edildi";
                    db.SaveChanges();
                    GridSiparisDoldur();
                }
            }
        }

        string SiparisID;

        public void SiparisForm()
        {
            List<order> SipList = new List<order>();
            List<Menuler> productList = new List<Menuler>();
            Menuler Product;

            if (DataGridSayı == 1)
            {
                SiparisID = dataGridView1.CurrentRow.Cells[0].Value.ToString();
            }
            else if (DataGridSayı == 2)
            {
                SiparisID = dataGridView2.CurrentRow.Cells[0].Value.ToString();
            }
            else if (DataGridSayı == 5)
            {
                SiparisID = dataGridView5.CurrentRow.Cells[0].Value.ToString();
            }

            var adisyon = db.Adisyonlar.Where(x => x.SiparisID == SiparisID).ToList();
            var urunu = db.Urunler.Where(y => y.SiparisID == SiparisID).ToList();

            SipClass.ID = adisyon[0].SiparisID;
            SipClass.AdSoyad = adisyon[0].MusteriAdSoyad;
            SipClass.Telefon = adisyon[0].Telefon;
            SipClass.Adres = adisyon[0].Adres;
            SipClass.Tarih = adisyon[0].Tarih.ToString();
            SipClass.Odeme = adisyon[0].Odeme;
            SipClass.SiparisToplam = adisyon[0].SiparisToplam;
            SipClass.SiparisNotu = adisyon[0].SiparisNot;
            SipClass.KDV = adisyon[0].KDV;
            SipClass.AraToplam = adisyon[0].AraToplam;
            SipClass.DurumID = adisyon[0].DurumID;

            foreach (var item2 in urunu)
            {
                Product = new Menuler();
                Product.Name = item2.UrunAdı;
                Product.Quantity = item2.Adet;
                Product.Price = item2.UrunFiyat.ToString();
                productList.Add(Product);
            }

            SipClass.product = productList;
            SipList.Add(SipClass);
            SipForm.gelensiparis(SipList);
            SipForm.ShowDialog();
        }

        private void dataGridView2_DoubleClick(object sender, EventArgs e)
        {
            SiparisForm();
            GridTeslimDoldur();
        }

        private void dataGridView5_DoubleClick(object sender, EventArgs e)
        {
            SiparisForm();
            GridİptalDoldur();
        }

        private void dataGridView1_DoubleClick(object sender, EventArgs e)
        {
            SiparisForm();
            GridSiparisDoldur();
        }

        ////////////////////

    }
}