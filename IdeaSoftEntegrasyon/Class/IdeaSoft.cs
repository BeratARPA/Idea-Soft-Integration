using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Data;
using System.Data.Entity;
using System.Windows.Forms;
using System.Runtime.ExceptionServices;

namespace IdeaSoftEntegrasyon
{
    public class IdeaSoft
    {
        SepetEntities db = new SepetEntities();
        AdisyonKaydet ak = new AdisyonKaydet();
        public bool IdeaAktifMi;

        public void IdeaLogin()
        {
            try
            {
                var IdeaSoft = db.Kullanici.Where(y => y.AktifMi == true).OrderByDescending(y => y.ID).ToList();
                if (IdeaSoft.Count > 0)
                {
                    EntegrasyonBilgileri.ClientID = IdeaSoft[0].ClientID;
                    EntegrasyonBilgileri.ClientSecret = IdeaSoft[0].ClientSecret;
                    EntegrasyonBilgileri.YonlendirmeAdres = IdeaSoft[0].YonlendirmeAdresi;
                    EntegrasyonBilgileri.FirmaAdı = IdeaSoft[0].FirmaAdı;
                    IdeaAktifMi = true;
                }
                else
                {
                    IdeaAktifMi = false;
                }
            }
            catch (WebException x)
            {
                WebResponse errResp = x.Response;
                using (Stream respStream = errResp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(respStream);
                    string text = reader.ReadToEnd();
                }
            }
        }

        public string SiparisSorgula()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://" + EntegrasyonBilgileri.FirmaAdı + ".myideasoft.com/api/orders?limit=1&sort=-id&status=waiting_for_approval");
            request.Method = "GET";

            request.Headers["Authorization"] = "Bearer " + EntegrasyonBilgileri.AccessToken;
            request.ContentType = "application/json; charset=utf-8";
            string bilgi;
            using (var response = request.GetResponse())
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                bilgi = sr.ReadToEnd();
            }
            if (bilgi.Length > 2)
            {
                return bilgi;
            }

            return null;
        }

        public List<order> SiparisDondur(string jsonveri)
        {
            List<order> SiparisListesi = new List<order>();
            order order = new order();
            List<Menuler> menuList = new List<Menuler>();
            Menuler Menu;

            JArray Siparis = JArray.Parse(jsonveri);
            foreach (var item in Siparis)
            {
                JArray urun = (JArray)item["orderItems"];
                foreach (var item2 in urun)
                {
                    Menu = new Menuler();
                    Menu.ID = item2["id"].ToString();
                    Menu.Name = item2["productName"].ToString();
                    Menu.Price = item2["productPrice"].ToString();
                    Menu.Quantity = item2["productQuantity"].ToString();
                    Menu.SiparisID = item["transactionId"].ToString();
                    menuList.Add(Menu);
                    ak.MenuKontrol(menuList);
                }
                order.ID = item["transactionId"].ToString();
                order.DurumID = item["id"].ToString();
                order.AdSoyad = item["customerFirstname"].ToString() + " " + item["customerSurname"].ToString();
                order.Telefon = item["customerPhone"].ToString();
                order.Adres = item["shippingAddress"]["address"].ToString();
                order.Odeme = item["paymentTypeName"].ToString();
                order.Tarih = item["createdAt"].ToString();
                order.Durum = "Onay Bekliyor";
                order.SiparisToplam = item["generalAmount"].ToString();
                order.AraToplam = item["amount"].ToString();
                order.KDV = item["taxAmount"].ToString();
                order.SiparisNotu = item["giftNote"].ToString();
                break;
            }
            order.product = menuList;
            SiparisListesi.Add(order);

            return SiparisListesi;
        }

        public DataSet Urunler()
        {
            DataSet ds = new DataSet();
            double price;
            string productid;

            DataTable dt = ds.Tables.Add("Urunler");
            dt.Columns.Add("ProductID");
            dt.Columns.Add("Name");
            dt.Columns.Add("Price");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://" + EntegrasyonBilgileri.FirmaAdı + ".myideasoft.com/api/products?limit=100");
            request.Method = "GET";

            request.Headers["Authorization"] = "Bearer " + EntegrasyonBilgileri.AccessToken;
            request.ContentType = "application/json; charset=utf-8";

            using (var response = request.GetResponse())
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                string result = sr.ReadToEnd();
                JArray jsonArray = JArray.Parse(result);
                foreach (JObject Urun in jsonArray)
                {
                    string Name = Urun["name"].ToString();
                    price = (double)Urun["price1"];
                    productid = Urun["id"].ToString();
                    dt.Rows.Add(productid, Name, price);
                }
            }
            return ds;
        }

        public DataSet Kategoriler()
        {
            DataSet ds = new DataSet();
            string KategoriID;

            DataTable dt = ds.Tables.Add("Kategoriler");
            dt.Columns.Add("KategoriID");
            dt.Columns.Add("KategoriAdı");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://" + EntegrasyonBilgileri.FirmaAdı + ".myideasoft.com/api/categories?limit=100");
            request.Method = "GET";

            request.Headers["Authorization"] = "Bearer " + EntegrasyonBilgileri.AccessToken;
            request.ContentType = "application/json; charset=utf-8";

            using (var response = request.GetResponse())
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                string result = sr.ReadToEnd();
                JArray jsonArray = JArray.Parse(result);
                foreach (JObject Kategori in jsonArray)
                {
                    string Name = Kategori["name"].ToString();
                    KategoriID = Kategori["id"].ToString();
                    dt.Rows.Add(KategoriID, Name);
                }
            }
            return ds;
        }

        public string SiparisDurum(string SiparisID, string Durum)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://" + EntegrasyonBilgileri.FirmaAdı + ".myideasoft.com/api/orders/" + SiparisID + "");
            request.Method = "PUT";

            request.Headers["Authorization"] = "Bearer " + EntegrasyonBilgileri.AccessToken;
            request.ContentType = "application/json; charset=utf-8";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = string.Format("{{" + (char)34 + "status" + (char)34 + ":" + (char)34 + "{0}" + (char)34 + "}}", Durum);

                streamWriter.Write(json);
                streamWriter.Flush();
            }

            var response = (HttpWebResponse)request.GetResponse();

            return "";
        }

    }
}