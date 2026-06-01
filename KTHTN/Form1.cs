using KTHTN.rohang;
using System;
using System.Windows.Forms;
using KTHTN.BoLocTimKiem;
using KTHTN.ThanhToanPhiShip;

namespace KTHTN
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var exitCode = Microsoft.Playwright.Program.Main(new[] { "install" });
            if (exitCode != 0)
            {
                MessageBox.Show("Có lỗi khi cài trình duyệt, hãy kiểm tra kết nối mạng!");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            KTHTN.KT.DangNhapDangKy frmDangNhap = new KTHTN.KT.DangNhapDangKy();

            // Mở song song nhưng Form DangNhapDangKy luôn trôi nổi trên Form hiện tại
            frmDangNhap.Show(this);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RoHang roHang = new RoHang();

            // Mở song song nhưng Form DangNhapDangKy luôn trôi nổi trên Form hiện tại
            roHang.Show(this);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Cú pháp: [Tên_Namespace].[Tên_Class]
            KTHTN.BoLocTimKiem.BoLocTimKiem frmBoLoc = new KTHTN.BoLocTimKiem.BoLocTimKiem();

            // Lệnh Show(this) sẽ mở form mới song song
            frmBoLoc.Show(this);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Cú pháp: [Tên_Namespace].[Tên_Class]
            KTHTN.ThanhToanPhiShip.ThanhToanPhiShip frmBoLoc = new KTHTN.ThanhToanPhiShip.ThanhToanPhiShip();

            // Lệnh Show(this) sẽ mở form mới song song
            frmBoLoc.Show(this);
        }
    }
}
