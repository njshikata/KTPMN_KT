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
    public partial class NhapTaiKhoan : Form
    {
        public string EmailDung { get; private set; }
        public string MatKhauDung { get; private set; }
        public string EmailSai { get; private set; }
        public string MatKhauSai { get; private set; }
        public NhapTaiKhoan()
        {
            InitializeComponent();
        }

        private void textBox1_TaiKhoan_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_MatKhau_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Xong_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem người dùng có nhập rỗng bất kỳ ô nào không
            if (string.IsNullOrWhiteSpace(textBox1_TaiKhoan.Text) ||
                string.IsNullOrWhiteSpace(textBox2_MatKhau.Text) ||
                string.IsNullOrWhiteSpace(textBox2_TaiKhoanSai.Text) ||
                string.IsNullOrWhiteSpace(textBox1_MatKhauSai.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tài khoản/Mật khẩu ĐÚNG và SAI!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Lưu toàn bộ dữ liệu vào biến
            EmailDung = textBox1_TaiKhoan.Text;
            MatKhauDung = textBox2_MatKhau.Text;
            EmailSai = textBox2_TaiKhoanSai.Text;
            MatKhauSai = textBox1_MatKhauSai.Text;

            // Báo cho Form chính biết là đã nhập thành công và Đóng form
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void textBox2_TaiKhoanSai_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_MatKhauSai_TextChanged(object sender, EventArgs e)
        {

        }

        private void NhapTaiKhoan_Load(object sender, EventArgs e)
        {

        }
    }
}
