using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaSoftEntegrasyon
{
    public class order
    {
        public string ID { get; set; }
        public string DurumID { get; set; }
        public string AdSoyad { get; set; }
        public string Telefon { get; set; }
        public string Adres { get; set; }
        public string Odeme { get; set; }
        public string Tarih { get; set; }
        public string Durum { get; set; }
        public string SiparisToplam { get; set; }
        public string AraToplam { get; set; }
        public string KDV { get; set; }
        public string SiparisNotu { get; set; }
        public List<Menuler> product { get; set; }
    }
}