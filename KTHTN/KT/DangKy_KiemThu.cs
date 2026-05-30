using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KTHTN.KT
{
    public partial class DangKy_KiemThu : Form
    {
        public DangKy_KiemThu()
        {
            InitializeComponent();
        }
        public string HoDung => textBox4_HoDung.Text;
        public string TenDung => textBox3_Dung.Text;
        public string EmailDung => textBox2_Emaildung.Text;
        public string MatKhauDung => textBox1_MatKhauDung.Text;

        public string HoSai => textBox5_HoSai.Text;
        public string TenSai => textBox6_tenSai.Text;
        public string EmailSai => textBox7_EmailSai.Text;
        public string MatKhauSai => textBox8_MatKhauSai.Text;
        private void textBox4_HoDung_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_Dung_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_Emaildung_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_MatKhauDung_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox5_HoSai_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox6_tenSai_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox7_EmailSai_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox8_MatKhauSai_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Xong_Click(object sender, EventArgs e)
        {
            // Trả về tín hiệu OK cho form chính biết là đã nhập xong
            this.DialogResult = DialogResult.OK;
            this.Close(); // Đóng form nhập liệu
        }
    }
}
