using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaSoftEntegrasyon
{
    public class AdisyonKaydet
    {
        SepetEntities db = new SepetEntities();
        public int AdisyonK(List<order> SiparisListesi, string mssg)
        {
            int sonuc = 1;

            foreach (var item in SiparisListesi)
            {
                var varmi = db.Adisyonlar.Where(s => s.SiparisID == item.ID).ToList();
                if (varmi.Count == 0)
                {
                    sonuc = 0;
                    Adisyonlar Adisyon = new Adisyonlar();
                    Adisyon.SiparisID = item.ID;
                    Adisyon.DurumID = item.DurumID;
                    Adisyon.MusteriAdSoyad = item.AdSoyad;
                    Adisyon.Telefon = item.Telefon;
                    Adisyon.Adres = item.Adres;
                    Adisyon.Odeme = item.Odeme;
                    Adisyon.Tarih = Convert.ToDateTime(item.Tarih);
                    Adisyon.Durum = item.Durum;
                    Adisyon.SiparisToplam = Convert.ToDouble(item.SiparisToplam).ToString("#,##0.00");
                    Adisyon.AktifMi = true;
                    Adisyon.GelenBilgi = mssg;
                    Adisyon.AraToplam = Convert.ToDouble(item.AraToplam).ToString("#,##0.00");
                    Adisyon.KDV = Convert.ToDouble(item.KDV).ToString("#,##0.00");
                    Adisyon.SiparisNot = item.SiparisNotu;
                    db.Adisyonlar.Add(Adisyon);
                    db.SaveChanges();
                }
            }
            return sonuc;
        }

        public void MenuKontrol(List<Menuler> SipList)
        {
            Urunler Urun = new Urunler();

            if (SipList.Count != 0)
            {
                foreach (var item in SipList)
                {
                    var urunvarmi = db.Urunler.Where(u => u.UrunID == item.ID).ToList();
                    if (urunvarmi.Count == 0)
                    {
                        Urun.UrunID = item.ID;
                        Urun.UrunAdı = item.Name;
                        Urun.UrunFiyat = Convert.ToDouble(item.Price).ToString("#,##0.00");
                        Urun.SiparisID = item.SiparisID;
                        Urun.Adet = item.Quantity;
                        db.Urunler.Add(Urun);
                        db.SaveChanges();
                    }
                }
            }
        }

        public void UrunBilgi(List<UrunBilgiler> SipList)
        {
            UrunBilgi Urun = new UrunBilgi();

            if (SipList.Count != 0)
            {
                foreach (var item in SipList)
                {
                    var urunvarmi = db.UrunBilgi.Where(u => u.UrunID == item.ID).ToList();
                    if (urunvarmi.Count == 0)
                    {
                        Urun.UrunID = item.ID;
                        Urun.UrunAdı = item.Name;
                        Urun.UrunFiyat = Convert.ToDouble(item.Price).ToString("#,##0.00");

                        db.UrunBilgi.Add(Urun);
                        db.SaveChanges();
                    }
                }
            }
        }

        public void KategoriKontrol(List<Kategorıler> SipList)
        {
            Kategoriler Kategoriler = new Kategoriler();

            if (SipList.Count != 0)
            {
                foreach (var item in SipList)
                {
                    var urunvarmi = db.Kategoriler.Where(u => u.KategoriID == item.KategoriID).ToList();
                    if (urunvarmi.Count == 0)
                    {
                        Kategoriler.KategoriID = item.KategoriID;
                        Kategoriler.KategoriAdi = item.KategoriAdı;
                        db.Kategoriler.Add(Kategoriler);
                        db.SaveChanges();
                    }
                }
            }
        }

    }
}