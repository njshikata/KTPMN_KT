namespace KTHTN.rohang
{
    partial class RoHang
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dgvBoundary = new System.Windows.Forms.DataGridView();
            this.Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TrangThai = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.KetQuaThucTe = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.KetQuaMongDoi = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DuLieuInput = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PhuongPhap = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TruongKiemTra = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MoTaKichBan = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MaTC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBox1_AnGui_RoHang = new System.Windows.Forms.CheckBox();
            this.button1_napDuLieu_rohang = new System.Windows.Forms.Button();
            this.button3_XuatBaoCaoExxcel_rohang = new System.Windows.Forms.Button();
            this.button4_ChayKiemThu_rohang = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox1_SoLuongChay_RoHang = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1_TocDoKiemThu_RoHang = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabControl3 = new System.Windows.Forms.TabControl();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.button4_TruyCapTrangTinh_rohang = new System.Windows.Forms.Button();
            this.button3_ChayKiemThuBangQuyetDinh_rohang = new System.Windows.Forms.Button();
            this.button2_LuuKetQua_rohang = new System.Windows.Forms.Button();
            this.checkBox1_ChayAnDaoDienBangQuyerDinhRoHANG = new System.Windows.Forms.CheckBox();
            this.textBox1_SoLuongCHatBangrohANG = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.dgvBangQuyetDinh = new System.Windows.Forms.DataGridView();
            this.tabPage5.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBoundary)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.tabControl3.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBangQuyetDinh)).BeginInit();
            this.SuspendLayout();
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.groupBox1);
            this.tabPage5.Location = new System.Drawing.Point(4, 25);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(948, 509);
            this.tabPage5.TabIndex = 0;
            this.tabPage5.Text = "Phân hoạch tương đương & Phân tích giá trị biên";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.dgvBoundary);
            this.groupBox1.Location = new System.Drawing.Point(3, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(939, 503);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // dgvBoundary
            // 
            this.dgvBoundary.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBoundary.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.MaTC,
            this.MoTaKichBan,
            this.TruongKiemTra,
            this.PhuongPhap,
            this.DuLieuInput,
            this.KetQuaMongDoi,
            this.KetQuaThucTe,
            this.Column1,
            this.TrangThai,
            this.Time});
            this.dgvBoundary.Location = new System.Drawing.Point(6, 6);
            this.dgvBoundary.Name = "dgvBoundary";
            this.dgvBoundary.RowHeadersWidth = 51;
            this.dgvBoundary.RowTemplate.Height = 24;
            this.dgvBoundary.Size = new System.Drawing.Size(927, 346);
            this.dgvBoundary.TabIndex = 0;
            // 
            // Time
            // 
            this.Time.HeaderText = "Time";
            this.Time.MinimumWidth = 6;
            this.Time.Name = "Time";
            this.Time.Width = 125;
            // 
            // TrangThai
            // 
            this.TrangThai.HeaderText = "Trạng thái";
            this.TrangThai.MinimumWidth = 6;
            this.TrangThai.Name = "TrangThai";
            this.TrangThai.Width = 125;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "Kết quả thực tế ( API trả về)";
            this.Column1.MinimumWidth = 6;
            this.Column1.Name = "Column1";
            this.Column1.Width = 125;
            // 
            // KetQuaThucTe
            // 
            this.KetQuaThucTe.HeaderText = "Kết quả thực tế ( Dao Diện)";
            this.KetQuaThucTe.MinimumWidth = 6;
            this.KetQuaThucTe.Name = "KetQuaThucTe";
            this.KetQuaThucTe.Width = 125;
            // 
            // KetQuaMongDoi
            // 
            this.KetQuaMongDoi.HeaderText = "Kết quả mong đợi";
            this.KetQuaMongDoi.MinimumWidth = 6;
            this.KetQuaMongDoi.Name = "KetQuaMongDoi";
            this.KetQuaMongDoi.Width = 125;
            // 
            // DuLieuInput
            // 
            this.DuLieuInput.HeaderText = "Dữ liệu Input";
            this.DuLieuInput.MinimumWidth = 6;
            this.DuLieuInput.Name = "DuLieuInput";
            this.DuLieuInput.Width = 125;
            // 
            // PhuongPhap
            // 
            this.PhuongPhap.HeaderText = "Phương pháp";
            this.PhuongPhap.MinimumWidth = 6;
            this.PhuongPhap.Name = "PhuongPhap";
            this.PhuongPhap.Width = 125;
            // 
            // TruongKiemTra
            // 
            this.TruongKiemTra.HeaderText = "Trường kiểm tra";
            this.TruongKiemTra.MinimumWidth = 6;
            this.TruongKiemTra.Name = "TruongKiemTra";
            this.TruongKiemTra.Width = 125;
            // 
            // MoTaKichBan
            // 
            this.MoTaKichBan.HeaderText = "Mô tả kịch bản";
            this.MoTaKichBan.MinimumWidth = 6;
            this.MoTaKichBan.Name = "MoTaKichBan";
            this.MoTaKichBan.Width = 125;
            // 
            // MaTC
            // 
            this.MaTC.HeaderText = "Mã TC";
            this.MaTC.MinimumWidth = 6;
            this.MaTC.Name = "MaTC";
            this.MaTC.Width = 125;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.textBox1_TocDoKiemThu_RoHang);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.textBox1_SoLuongChay_RoHang);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.button4_ChayKiemThu_rohang);
            this.groupBox2.Controls.Add(this.button3_XuatBaoCaoExxcel_rohang);
            this.groupBox2.Controls.Add(this.button1_napDuLieu_rohang);
            this.groupBox2.Controls.Add(this.checkBox1_AnGui_RoHang);
            this.groupBox2.Location = new System.Drawing.Point(6, 358);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(927, 139);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Lệnh";
            // 
            // checkBox1_AnGui_RoHang
            // 
            this.checkBox1_AnGui_RoHang.AutoSize = true;
            this.checkBox1_AnGui_RoHang.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.checkBox1_AnGui_RoHang.Location = new System.Drawing.Point(6, 21);
            this.checkBox1_AnGui_RoHang.Name = "checkBox1_AnGui_RoHang";
            this.checkBox1_AnGui_RoHang.Size = new System.Drawing.Size(136, 20);
            this.checkBox1_AnGui_RoHang.TabIndex = 0;
            this.checkBox1_AnGui_RoHang.Text = "Chạy Ẩn Dao Diện";
            this.checkBox1_AnGui_RoHang.UseVisualStyleBackColor = true;
            // 
            // button1_napDuLieu_rohang
            // 
            this.button1_napDuLieu_rohang.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1_napDuLieu_rohang.Location = new System.Drawing.Point(323, 49);
            this.button1_napDuLieu_rohang.Name = "button1_napDuLieu_rohang";
            this.button1_napDuLieu_rohang.Size = new System.Drawing.Size(152, 59);
            this.button1_napDuLieu_rohang.TabIndex = 1;
            this.button1_napDuLieu_rohang.Text = "Nạp Dữ Liệu";
            this.button1_napDuLieu_rohang.UseVisualStyleBackColor = true;
            this.button1_napDuLieu_rohang.Click += new System.EventHandler(this.button1_napDuLieu_rohang_Click);
            // 
            // button3_XuatBaoCaoExxcel_rohang
            // 
            this.button3_XuatBaoCaoExxcel_rohang.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button3_XuatBaoCaoExxcel_rohang.Location = new System.Drawing.Point(491, 49);
            this.button3_XuatBaoCaoExxcel_rohang.Name = "button3_XuatBaoCaoExxcel_rohang";
            this.button3_XuatBaoCaoExxcel_rohang.Size = new System.Drawing.Size(152, 59);
            this.button3_XuatBaoCaoExxcel_rohang.TabIndex = 3;
            this.button3_XuatBaoCaoExxcel_rohang.Text = "Truy Cập Trang tính";
            this.button3_XuatBaoCaoExxcel_rohang.UseVisualStyleBackColor = true;
            this.button3_XuatBaoCaoExxcel_rohang.Click += new System.EventHandler(this.button3_XuatBaoCaoExxcel_rohang_Click);
            // 
            // button4_ChayKiemThu_rohang
            // 
            this.button4_ChayKiemThu_rohang.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button4_ChayKiemThu_rohang.Location = new System.Drawing.Point(649, 49);
            this.button4_ChayKiemThu_rohang.Name = "button4_ChayKiemThu_rohang";
            this.button4_ChayKiemThu_rohang.Size = new System.Drawing.Size(152, 59);
            this.button4_ChayKiemThu_rohang.TabIndex = 4;
            this.button4_ChayKiemThu_rohang.Text = "🚀 CHẠY KIỂM THỬ (Run Test)";
            this.button4_ChayKiemThu_rohang.UseVisualStyleBackColor = true;
            this.button4_ChayKiemThu_rohang.Click += new System.EventHandler(this.button4_ChayKiemThu_rohang_Click);
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Location = new System.Drawing.Point(807, 49);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(110, 59);
            this.button1.TabIndex = 6;
            this.button1.Text = "Lưu Kết Quả";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox1_SoLuongChay_RoHang
            // 
            this.textBox1_SoLuongChay_RoHang.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox1_SoLuongChay_RoHang.Location = new System.Drawing.Point(131, 49);
            this.textBox1_SoLuongChay_RoHang.Name = "textBox1_SoLuongChay_RoHang";
            this.textBox1_SoLuongChay_RoHang.Size = new System.Drawing.Size(100, 22);
            this.textBox1_SoLuongChay_RoHang.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 16);
            this.label1.TabIndex = 8;
            this.label1.Text = "Số Luồng Chạy";
            // 
            // textBox1_TocDoKiemThu_RoHang
            // 
            this.textBox1_TocDoKiemThu_RoHang.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox1_TocDoKiemThu_RoHang.Location = new System.Drawing.Point(131, 86);
            this.textBox1_TocDoKiemThu_RoHang.Name = "textBox1_TocDoKiemThu_RoHang";
            this.textBox1_TocDoKiemThu_RoHang.Size = new System.Drawing.Size(100, 22);
            this.textBox1_TocDoKiemThu_RoHang.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 16);
            this.label2.TabIndex = 10;
            this.label2.Text = "Tốc Độ Kiểm Thử";
            // 
            // tabControl3
            // 
            this.tabControl3.Controls.Add(this.tabPage5);
            this.tabControl3.Controls.Add(this.tabPage6);
            this.tabControl3.Location = new System.Drawing.Point(2, 3);
            this.tabControl3.Name = "tabControl3";
            this.tabControl3.SelectedIndex = 0;
            this.tabControl3.Size = new System.Drawing.Size(956, 538);
            this.tabControl3.TabIndex = 2;
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.groupBox3);
            this.tabPage6.Location = new System.Drawing.Point(4, 25);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage6.Size = new System.Drawing.Size(948, 509);
            this.tabPage6.TabIndex = 1;
            this.tabPage6.Text = "Bảng quyết định";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.textBox1_SoLuongCHatBangrohANG);
            this.groupBox4.Controls.Add(this.checkBox1_ChayAnDaoDienBangQuyerDinhRoHANG);
            this.groupBox4.Controls.Add(this.button2_LuuKetQua_rohang);
            this.groupBox4.Controls.Add(this.button3_ChayKiemThuBangQuyetDinh_rohang);
            this.groupBox4.Controls.Add(this.button4_TruyCapTrangTinh_rohang);
            this.groupBox4.Location = new System.Drawing.Point(3, 338);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(927, 139);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Lệnh";
            // 
            // button4_TruyCapTrangTinh_rohang
            // 
            this.button4_TruyCapTrangTinh_rohang.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button4_TruyCapTrangTinh_rohang.Location = new System.Drawing.Point(400, 57);
            this.button4_TruyCapTrangTinh_rohang.Name = "button4_TruyCapTrangTinh_rohang";
            this.button4_TruyCapTrangTinh_rohang.Size = new System.Drawing.Size(152, 59);
            this.button4_TruyCapTrangTinh_rohang.TabIndex = 3;
            this.button4_TruyCapTrangTinh_rohang.Text = "Truy Cập Trang tính";
            this.button4_TruyCapTrangTinh_rohang.UseVisualStyleBackColor = true;
            // 
            // button3_ChayKiemThuBangQuyetDinh_rohang
            // 
            this.button3_ChayKiemThuBangQuyetDinh_rohang.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button3_ChayKiemThuBangQuyetDinh_rohang.Location = new System.Drawing.Point(568, 57);
            this.button3_ChayKiemThuBangQuyetDinh_rohang.Name = "button3_ChayKiemThuBangQuyetDinh_rohang";
            this.button3_ChayKiemThuBangQuyetDinh_rohang.Size = new System.Drawing.Size(152, 59);
            this.button3_ChayKiemThuBangQuyetDinh_rohang.TabIndex = 4;
            this.button3_ChayKiemThuBangQuyetDinh_rohang.Text = "🚀 CHẠY KIỂM THỬ (Run Test)";
            this.button3_ChayKiemThuBangQuyetDinh_rohang.UseVisualStyleBackColor = true;
            this.button3_ChayKiemThuBangQuyetDinh_rohang.Click += new System.EventHandler(this.button3_ChayKiemThuBangQuyetDinh_rohang_Click);
            // 
            // button2_LuuKetQua_rohang
            // 
            this.button2_LuuKetQua_rohang.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button2_LuuKetQua_rohang.Location = new System.Drawing.Point(726, 57);
            this.button2_LuuKetQua_rohang.Name = "button2_LuuKetQua_rohang";
            this.button2_LuuKetQua_rohang.Size = new System.Drawing.Size(110, 59);
            this.button2_LuuKetQua_rohang.TabIndex = 6;
            this.button2_LuuKetQua_rohang.Text = "Lưu Kết Quả";
            this.button2_LuuKetQua_rohang.UseVisualStyleBackColor = true;
            this.button2_LuuKetQua_rohang.Click += new System.EventHandler(this.button2_LuuKetQua_rohang_Click);
            // 
            // checkBox1_ChayAnDaoDienBangQuyerDinhRoHANG
            // 
            this.checkBox1_ChayAnDaoDienBangQuyerDinhRoHANG.AutoSize = true;
            this.checkBox1_ChayAnDaoDienBangQuyerDinhRoHANG.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.checkBox1_ChayAnDaoDienBangQuyerDinhRoHANG.Location = new System.Drawing.Point(25, 42);
            this.checkBox1_ChayAnDaoDienBangQuyerDinhRoHANG.Name = "checkBox1_ChayAnDaoDienBangQuyerDinhRoHANG";
            this.checkBox1_ChayAnDaoDienBangQuyerDinhRoHANG.Size = new System.Drawing.Size(136, 20);
            this.checkBox1_ChayAnDaoDienBangQuyerDinhRoHANG.TabIndex = 12;
            this.checkBox1_ChayAnDaoDienBangQuyerDinhRoHANG.Text = "Chạy Ẩn Dao Diện";
            this.checkBox1_ChayAnDaoDienBangQuyerDinhRoHANG.UseVisualStyleBackColor = true;
            // 
            // textBox1_SoLuongCHatBangrohANG
            // 
            this.textBox1_SoLuongCHatBangrohANG.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox1_SoLuongCHatBangrohANG.Location = new System.Drawing.Point(147, 82);
            this.textBox1_SoLuongCHatBangrohANG.Name = "textBox1_SoLuongCHatBangrohANG";
            this.textBox1_SoLuongCHatBangrohANG.Size = new System.Drawing.Size(100, 22);
            this.textBox1_SoLuongCHatBangrohANG.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(22, 82);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(98, 16);
            this.label5.TabIndex = 14;
            this.label5.Text = "Số Luồng Chạy";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.groupBox4);
            this.groupBox3.Controls.Add(this.dgvBangQuyetDinh);
            this.groupBox3.Location = new System.Drawing.Point(6, 6);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(936, 497);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            // 
            // dgvBangQuyetDinh
            // 
            this.dgvBangQuyetDinh.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBangQuyetDinh.Location = new System.Drawing.Point(0, 10);
            this.dgvBangQuyetDinh.Name = "dgvBangQuyetDinh";
            this.dgvBangQuyetDinh.RowHeadersWidth = 51;
            this.dgvBangQuyetDinh.RowTemplate.Height = 24;
            this.dgvBangQuyetDinh.Size = new System.Drawing.Size(930, 322);
            this.dgvBangQuyetDinh.TabIndex = 0;
            // 
            // RoHang
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(962, 542);
            this.Controls.Add(this.tabControl3);
            this.Name = "RoHang";
            this.Text = "RoHang";
            this.tabPage5.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvBoundary)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabControl3.ResumeLayout(false);
            this.tabPage6.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvBangQuyetDinh)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1_TocDoKiemThu_RoHang;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1_SoLuongChay_RoHang;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button4_ChayKiemThu_rohang;
        private System.Windows.Forms.Button button3_XuatBaoCaoExxcel_rohang;
        private System.Windows.Forms.Button button1_napDuLieu_rohang;
        private System.Windows.Forms.CheckBox checkBox1_AnGui_RoHang;
        private System.Windows.Forms.DataGridView dgvBoundary;
        private System.Windows.Forms.DataGridViewTextBoxColumn MaTC;
        private System.Windows.Forms.DataGridViewTextBoxColumn MoTaKichBan;
        private System.Windows.Forms.DataGridViewTextBoxColumn TruongKiemTra;
        private System.Windows.Forms.DataGridViewTextBoxColumn PhuongPhap;
        private System.Windows.Forms.DataGridViewTextBoxColumn DuLieuInput;
        private System.Windows.Forms.DataGridViewTextBoxColumn KetQuaMongDoi;
        private System.Windows.Forms.DataGridViewTextBoxColumn KetQuaThucTe;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn TrangThai;
        private System.Windows.Forms.DataGridViewTextBoxColumn Time;
        private System.Windows.Forms.TabControl tabControl3;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox1_SoLuongCHatBangrohANG;
        private System.Windows.Forms.CheckBox checkBox1_ChayAnDaoDienBangQuyerDinhRoHANG;
        private System.Windows.Forms.Button button2_LuuKetQua_rohang;
        private System.Windows.Forms.Button button3_ChayKiemThuBangQuyetDinh_rohang;
        private System.Windows.Forms.Button button4_TruyCapTrangTinh_rohang;
        private System.Windows.Forms.DataGridView dgvBangQuyetDinh;
    }
}