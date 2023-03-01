using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaSoftEntegrasyon
{
    public static class EntegrasyonBilgileri
    {
        public static string ClientID { get; set; }
        public static string ClientSecret { get; set; }
        public static string YonlendirmeAdres { get; set; }
        public static string FirmaAdı { get; set; }
        public static string Code { get; set; }
        public static string AccessToken { get; set; }
        public static string RefreshToken { get; set; }
    }
}