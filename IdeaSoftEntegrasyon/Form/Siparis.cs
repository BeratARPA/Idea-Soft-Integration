using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IdeaSoftEntegrasyon
{
    public partial class Siparis : Form
    {
        public Siparis()
        {
            InitializeComponent();
        }

        IdeaSoft IdeaSoft = new IdeaSoft();
        SepetEntities db = new SepetEntities();
        public string SipNo;
        
        public void gelensiparis(List<order> SipList)
        {
            dataGridView1.Rows.Clear();
            this.TopMost = true;
            lblAdSoyad.Text = SipList[0].AdSoyad;
            lblTelefon.Text = SipList[0].Telefon;
            lblTarih.Text = SipList[0].Tarih;
            richTxtAdres.Text = SipList[0].Adres;
            lblKdv.Text = Convert.ToDouble(SipList[0].KDV).ToString("#,##0.00") + " TL";
            lblAraToplam.Text = Convert.ToDouble(SipList[0].AraToplam).ToString("#,##0.00") + " TL";
            lblSiparisToplam.Text = Convert.ToDouble(SipList[0].SiparisToplam).ToString("#,##0.00") + " TL";
            lblSiparisNo.Text = SipList[0].ID;
            lblOdemeTipi.Text = SipList[0].Odeme;
            richTxtNot.Text = SipList[0].SiparisNotu;

            foreach (var item in SipList)
            {
                int i = 0;
                foreach (var item2 in item.product)
                {
                    dataGridView1.Rows.Add();
                    dataGridView1.Rows[i].Cells[0].Value = item2.ID;
                    dataGridView1.Rows[i].Cells[1].Value = item2.Name.ToUpper().ToString();
                    dataGridView1.Rows[i].Cells[2].Value = item2.Quantity.ToString() + " ADET";
                    double uFiyat = Convert.ToDouble(item2.Price) * Convert.ToDouble(item2.Quantity);
                    dataGridView1.Rows[i].Cells[3].Value = uFiyat.ToString("#,##0.00") + " TL";
                    i++;
                }
            }
            SipNo = SipList[0].DurumID;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Siparişi iptal etmek istiyor musunuz!", "Bilgi", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes) 
            {
                var update = db.Adisyonlar.Where(x => x.DurumID == SipNo).ToList();

                IdeaSoft.SiparisDurum(SipNo, "cancelled");
                update[0].Kabul = false;
                update[0].Ret = true;
                update[0].Hazirlaniyor = false;
                update[0].Yolda = false;
                update[0].Teslim = false;
                update[0].AktifMi = false;
                update[0].Durum = "İptal Edildi";
                db.SaveChanges();
                this.Close();                               
            }           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            button1.ForeColor = Color.Black;
        }

        private void button1_MouseMove(object sender, MouseEventArgs e)
        {
            button1.ForeColor = Color.White;
        }

    }
}