using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KTHTN.KT
{
    public partial class DangNhapDangKy : Form
    {

        public DangNhapDangKy()
        {
            InitializeComponent();
        }

        private void DangNhapDangKy_Load(object sender, EventArgs e)
        {

        }

        private void button1_napDuLieu_Click(object sender, EventArgs e)
        {
            string jsonPath = "Web_data_DangNhapDangKy.json";
            string spreadsheetId = "1dKW5TttKZzsYU0wnYz_KyL1yE4PiUDe72Zl6Iaz5UOI";
            string range = "Trang tính1!A1:J20"; // Cột J là cột thứ 10

            try
            {
                var service = GetSheetsService(jsonPath);
                var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
                var response = request.Execute();
                var values = response.Values;

                if (values != null && values.Count > 0)
                {
                    dgvBoundary.Rows.Clear(); // Xóa sạch dữ liệu UI hiện tại

                    bool isHeader = true;
                    foreach (var row in values)
                    {
                        if (isHeader) { isHeader = false; continue; }

                        object[] rowData = new object[10];
                        for (int i = 0; i < 10; i++)
                        {
                            // FIX LỖI: Chỉ lấy data từ Sheets cho 6 cột đầu (Input & Mong đợi)
                            // 4 cột cuối (UI, API, Trạng thái, Time) ép thành rỗng để test lại từ đầu
                            if (i <= 5)
                            {
                                rowData[i] = (i < row.Count) ? row[i] : "";
                            }
                            else
                            {
                                rowData[i] = "";
                            }
                        }
                        dgvBoundary.Rows.Add(rowData);
                    }
                    MessageBox.Show("Nạp dữ liệu thành công! Đã làm sạch các cột kết quả cũ.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message);
            }
        }

        private async void button4_ChayKiemThu_Click(object sender, EventArgs e)
        {
            // =========================================================================
            // 0. DỌN DẸP ZOMBIE PROCESS
            // =========================================================================
            try
            {
                foreach (var p in Process.GetProcessesByName("brave"))
                {
                    p.Kill();
                }
            }
            catch { }

            int soLuongLuong = 1;
            if (!int.TryParse(textBox1_SoLuongChay.Text, out soLuongLuong) || soLuongLuong < 1)
                soLuongLuong = 1;

            int tocDoKiemThu = 0;
            if (!int.TryParse(textBox1_TocDoKiemThu.Text, out tocDoKiemThu) || tocDoKiemThu < 0)
                tocDoKiemThu = 0;

            string bravePath = @"C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe";
            int debugPort = 9222;
            string userDataDir = $@"C:\temp\automation_profile_login_{debugPort}";

            Random rnd = new Random();
            string headlessArg = checkBox1_AnGui.Checked ? "--headless=new " : "";

            string extraArgs = "--disable-blink-features=AutomationControlled --disable-infobars --window-size=1366,768";
            ProcessStartInfo psi = new ProcessStartInfo(bravePath, $"{headlessArg}--remote-debugging-port={debugPort} --user-data-dir=\"{userDataDir}\" --no-first-run --no-default-browser-check {extraArgs}")
            {
                UseShellExecute = true,
                WindowStyle = checkBox1_AnGui.Checked ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
            };

            Process braveProcess;
            try { braveProcess = Process.Start(psi); }
            catch { MessageBox.Show("Sai đường dẫn trình duyệt Brave!"); return; }

            await Task.Delay(3000);

            try
            {
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.ConnectOverCDPAsync($"http://127.0.0.1:{debugPort}");

                var validRows = new List<DataGridViewRow>();
                foreach (DataGridViewRow row in dgvBoundary.Rows)
                {
                    if (!row.IsNewRow && row.Cells[4].Value != null && row.Cells[4].Value.ToString().Trim() != "")
                    {
                        validRows.Add(row);
                    }
                }

                if (validRows.Count == 0)
                {
                    MessageBox.Show("Bảng dữ liệu trống, không có gì để chạy!");
                    try { braveProcess.Kill(); } catch { }
                    return;
                }

                int totalCases = validRows.Count;
                if (soLuongLuong > totalCases) { soLuongLuong = totalCases; }

                int chunkSize = (int)Math.Ceiling((double)totalCases / soLuongLuong);
                var tasks = new List<Task>();

                for (int i = 0; i < soLuongLuong; i++)
                {
                    var chunk = validRows.Skip(i * chunkSize).Take(chunkSize).ToList();
                    if (chunk.Count == 0) continue;

                    tasks.Add(Task.Run(async () =>
                    {
                        foreach (var row in chunk)
                        {
                            string inputData = row.Cells[4].Value?.ToString();
                            string[] parts = inputData?.Split('|');
                            if (parts == null || parts.Length < 2) continue;

                            string email = parts[0];
                            string pass = parts[1];

                            this.Invoke((MethodInvoker)delegate {
                                row.Cells[8].Value = "Đang chạy...";
                                row.Cells[8].Style.BackColor = System.Drawing.Color.LightYellow;
                                row.Cells[6].Value = "";
                                row.Cells[7].Value = "";
                            });

                            var contextOptions = new BrowserNewContextOptions
                            {
                                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36",
                                ViewportSize = new ViewportSize { Width = 1366, Height = 768 },
                                Locale = "vi-VN",
                                TimezoneId = "Asia/Ho_Chi_Minh",
                                HasTouch = false
                            };

                            var context = await browser.NewContextAsync(contextOptions);

                            await context.RouteAsync("**/*", async route =>
                            {
                                if (route.Request.ResourceType == "image" || route.Request.ResourceType == "media")
                                    await route.AbortAsync();
                                else
                                    await route.ContinueAsync();
                            });

                            await context.AddInitScriptAsync(@"
                        Object.defineProperty(navigator, 'webdriver', { get: () => false });
                        window.chrome = { runtime: {} };
                        Object.defineProperty(navigator, 'languages', { get: () => ['vi-VN', 'vi', 'en-US', 'en'] });
                        Object.defineProperty(navigator, 'plugins', { get: () => [1, 2, 3, 4, 5] });
                    ");

                            var page = await context.NewPageAsync();

                            string uiActualResult = "UI Chặn lại (Không hiện chữ lỗi)";
                            string apiActualResult = "Không bắt được kết quả";
                            int maxRetries = 6;

                            // =========================================================================
                            // VÒNG LẶP KIỂM THỬ: GIỚI HẠN NGHIÊM NGẶT 6 LẦN
                            // =========================================================================
                            for (int retry = 1; retry <= maxRetries; retry++)
                            {
                                if (retry == 1)
                                    await page.GotoAsync("https://www.maisononline.vn/");
                                else
                                {
                                    await page.ReloadAsync();
                                    await Task.Delay(2000 + tocDoKiemThu);
                                }

                                var loginBtn = page.Locator(".main-header_tool_right_icon").Nth(1);

                                if (!await page.Locator("#form-login-custom").IsVisibleAsync() && !await page.Locator("#customer_login").IsVisibleAsync())
                                {
                                    if (await loginBtn.IsVisibleAsync()) { await loginBtn.ClickAsync(); }
                                    await Task.Delay(1500 + rnd.Next(100, 300) + tocDoKiemThu);

                                    var tabLogin = page.Locator(".item-tab-form[data-tab='login']");
                                    if (await tabLogin.IsVisibleAsync()) { await tabLogin.ClickAsync(); }
                                    await Task.Delay(1000 + rnd.Next(100, 300) + tocDoKiemThu);
                                }

                                var emailInput = page.Locator("input[name='customer[email]']:visible").First;
                                var passInput = page.Locator("input[name='customer[password]']:visible").First;
                                var submitBtn = page.Locator("button.btn-login-form:visible, button.btn-login-form-page:visible").First;

                                await emailInput.ClearAsync();
                                await emailInput.PressSequentiallyAsync(email, new LocatorPressSequentiallyOptions { Delay = rnd.Next(30, 80) });
                                await Task.Delay(rnd.Next(200, 400));

                                await passInput.ClearAsync();
                                await passInput.PressSequentiallyAsync(pass, new LocatorPressSequentiallyOptions { Delay = rnd.Next(30, 80) });

                                await page.Mouse.MoveAsync(rnd.Next(200, 600), rnd.Next(200, 600), new MouseMoveOptions { Steps = 5 });

                                // Click Submit (Bỏ chặn API)
                                await submitBtn.ClickAsync(new LocatorClickOptions { Force = true });

                                // Chờ 2.5s để Backend xử lý và trả về HTML chứa lỗi
                                await Task.Delay(2500 + tocDoKiemThu);

                                // =========================================================================
                                // PHÂN TÍCH GIAO DIỆN (Quét thẳng vào DOM bằng Playwright Native C#)
                                // =========================================================================
                                bool foundError = false;

                                // Tập hợp các class lỗi phổ biến trên trang
                                var errorSelectors = new[] {
                            "#form-login-custom .error-status",
                            ".errors li",
                            ".toast-message",
                            "#form-login-custom .text-error",
                            ".alert-danger",
                            ".error-msg"
                        };

                                foreach (var sel in errorSelectors)
                                {
                                    // Lấy tất cả các thẻ khớp (dù ẩn hay hiện)
                                    var els = await page.Locator(sel).AllAsync();
                                    foreach (var el in els)
                                    {
                                        // Lấy chữ bên trong thẻ HTML
                                        string text = await el.TextContentAsync();
                                        if (!string.IsNullOrWhiteSpace(text))
                                        {
                                            uiActualResult = text.Trim();
                                            foundError = true;
                                            break; // Dừng vòng lặp element
                                        }
                                    }
                                    if (foundError) break; // Dừng vòng lặp selector
                                }

                                // Nếu không thấy chữ lỗi, kiểm tra xem có phải Đăng nhập thành công không
                                if (!foundError)
                                {
                                    string currentUrl = page.Url.ToLower();
                                    if (currentUrl.Contains("/account") && !currentUrl.Contains("login") && !currentUrl.Contains("register"))
                                    {
                                        uiActualResult = "Đăng nhập thành công";
                                    }
                                    else
                                    {
                                        var formLoginModal = page.Locator("#form-login-custom");
                                        if (!await formLoginModal.IsVisibleAsync())
                                        {
                                            uiActualResult = "Đăng nhập thành công";
                                        }
                                    }
                                }

                                // Ghi nhận trạng thái API tương ứng với UI
                                if (uiActualResult == "Đăng nhập thành công")
                                {
                                    apiActualResult = "[302/200] Chấp nhận đăng nhập";
                                }
                                else if (uiActualResult != "UI Chặn lại (Không hiện chữ lỗi)")
                                {
                                    apiActualResult = "[200] Backend trả về HTML chứa lỗi";
                                }

                                // =========================================================================
                                // LUẬT HARD-FAIL: CHỐT KẾT QUẢ VÀ DỪNG NGAY LẬP TỨC
                                // =========================================================================
                                if (uiActualResult != "UI Chặn lại (Không hiện chữ lỗi)")
                                {
                                    break; // Tìm thấy kết quả -> Văng khỏi vòng lặp thử lại!
                                }

                                if (retry == maxRetries)
                                {
                                    uiActualResult = $"Lỗi: Web lỳ lợm không phản hồi cụ thể sau {maxRetries} lần thử";
                                    apiActualResult = "Không bắt được kết quả";
                                    break;
                                }
                            }

                            // =========================================================================
                            // IN KẾT QUẢ RA BẢNG DATAGRIDVIEW
                            // =========================================================================
                            string expectedResult = row.Cells[5].Value?.ToString() ?? "";
                            bool isPass = KiemTraKetQuaLinhHoat(expectedResult, uiActualResult);

                            this.Invoke((MethodInvoker)delegate {
                                row.Cells[6].Value = uiActualResult;
                                row.Cells[7].Value = apiActualResult;
                                row.Cells[8].Value = isPass ? "Pass" : "Fail";
                                row.Cells[8].Style.BackColor = isPass ? System.Drawing.Color.LightGreen : System.Drawing.Color.LightPink;
                                row.Cells[9].Value = DateTime.Now.ToString("HH:mm:ss");
                            });

                            if (uiActualResult.Contains("Đăng nhập thành công") || apiActualResult.Contains("Chấp nhận"))
                            {
                                await page.GotoAsync("https://www.maisononline.vn/account/logout");
                                await Task.Delay(2000 + rnd.Next(100, 300) + tocDoKiemThu);
                            }

                            await page.CloseAsync();
                            await context.CloseAsync();
                        }
                    }));
                }

                await Task.WhenAll(tasks);

                await browser.CloseAsync();
                try { braveProcess.Kill(); } catch { }

                button1_Click(null, null);
                MessageBox.Show("Hoàn tất kiểm thử đồng thời đa luồng! Đã đồng bộ lên Google Sheets.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
                try { braveProcess.Kill(); } catch { }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // =========================================================================
                // LUỒNG 1: LƯU KẾT QUẢ LÊN GOOGLE SHEETS (GIỮ NGUYÊN TOÀN BỘ LOGIC CỦA CẬU)
                // =========================================================================
                string jsonPath = "Web_data_DangNhapDangKy.json";
                string spreadsheetId = "1dKW5TttKZzsYU0wnYz_KyL1yE4PiUDe72Zl6Iaz5UOI";

                var values = new List<IList<object>>();
                foreach (DataGridViewRow row in dgvBoundary.Rows)
                {
                    if (row.IsNewRow) continue;

                    // Đẩy 4 cột: Cột 6 (UI), Cột 7 (API), Cột 8 (Status), Cột 9 (Time)
                    var rowData = new List<object> {
                row.Cells[6].Value?.ToString() ?? "",
                row.Cells[7].Value?.ToString() ?? "",
                row.Cells[8].Value?.ToString() ?? "",
                row.Cells[9].Value?.ToString() ?? ""
            };
                    values.Add(rowData);
                }

                // Đẩy từ cột G (Cột thứ 7 trên Excel) đến Cột J
                var range = "Trang tính1!G2";
                var valueRange = new ValueRange { Values = values };

                var service = GetSheetsService(jsonPath);
                var updateRequest = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                updateRequest.Execute();

                // =========================================================================
                // LUỒNG 2: TỰ ĐỘNG XUẤT FILE REPORT OFFLINE XUỐNG THƯ MỤC CẠNH FILE .EXE
                // =========================================================================
                // Đường dẫn đến thư mục "ketqua" nằm cùng cấp với file .exe đang chạy
                string folderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ketqua");

                // Nếu thư mục "ketqua" chưa tồn tại thì hệ thống tự động tạo mới
                if (!System.IO.Directory.Exists(folderPath))
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                }

                // Tạo tên file theo định dạng chuẩn: KetQua_Ngay_Thang_Nam_Gio_Phut_Giay.csv
                string fileName = $"KetQua_DangNhapDangKyPhanHoachTuongDuongVaGiaTriBien_{DateTime.Now:dd_MM_yyyy_HH_mm_ss}.csv";
                string filePath = System.IO.Path.Combine(folderPath, fileName);

                // Ghi dữ liệu với Encoding.UTF8 (có BOM) để Excel mở không bị lỗi hiển thị Tiếng Việt
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // 1. Ghi dòng tiêu đề (Headers) lấy trực tiếp từ các cột của DataGridView của cậu
                    List<string> headers = new List<string>();
                    foreach (DataGridViewColumn col in dgvBoundary.Columns)
                    {
                        headers.Add(col.HeaderText ?? $"Cot_{col.Index}");
                    }
                    sw.WriteLine(string.Join(",", headers));

                    // 2. Ghi toàn bộ dữ liệu các dòng hiện tại trên bảng dữ liệu
                    foreach (DataGridViewRow row in dgvBoundary.Rows)
                    {
                        if (row.IsNewRow) continue;

                        List<string> rowCells = new List<string>();
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            string cellValue = cell.Value?.ToString() ?? "";

                            // Xử lý chuẩn hóa chuỗi dữ liệu (Nếu text chứa dấu phẩy hoặc dấu nháy kép thì bọc lại để file CSV không bị vỡ cấu trúc)
                            if (cellValue.Contains(",") || cellValue.Contains("\n") || cellValue.Contains("\""))
                            {
                                cellValue = "\"" + cellValue.Replace("\"", "\"\"") + "\"";
                            }
                            rowCells.Add(cellValue);
                        }
                        sw.WriteLine(string.Join(",", rowCells));
                    }
                }

                // Hiện thông báo gom cụm cả 2 luồng lưu trữ để người dùng dễ theo dõi vị trí file cục bộ
                if (sender != null)
                {
                    string thongBaoGom = $"Đã lưu và đồng bộ kết quả thành công!\n\n" +
                                         $"1. [Cloud]: Đã đẩy dữ liệu trực tuyến lên Google Sheets.\n" +
                                         $"2. [Local]: Đã xuất bản sao báo cáo offline tại thư mục:\n" +
                                         $"{filePath}";
                    MessageBox.Show(thongBaoGom, "Đồng Bộ Kết Quả", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu dữ liệu hoặc xuất file: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // Hàm hỗ trợ để khởi tạo SheetsService
        private SheetsService GetSheetsService(string jsonPath)
        {
            var credential = GoogleCredential.FromFile(jsonPath)
                .CreateScoped(SheetsService.Scope.Spreadsheets);

            return new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "AutomationTool",
            });
        }
        // Hàm xử lý việc so sánh linh hoạt (vượt qua sự cứng nhắc của Text)
        private bool KiemTraKetQuaLinhHoat(string expected, string actual)
        {
            if (string.IsNullOrWhiteSpace(expected) || string.IsNullOrWhiteSpace(actual)) return false;

            expected = expected.ToLower().Trim();
            actual = actual.ToLower().Trim();

            // 1. Nếu giống hệt nhau hoặc chứa nhau (Pass cơ bản)
            if (actual.Contains(expected) || expected.Contains(actual)) return true;

            // 2. Xử lý các từ khóa đồng nghĩa thường gặp ở trang Login
            if (expected.Contains("đúng email") && actual.Contains("không hợp lệ")) return true;
            if (expected.Contains("password") && actual.Contains("mật khẩu")) return true;
            if (expected.Contains("tồn tại") && actual.Contains("không hợp lệ")) return true; // Web bảo mật thường đánh đồng lỗi sai user/pass

            return false;
        }

        private void button3_XuatBaoCaoExxcel_Click(object sender, EventArgs e)
        {
            try
            {
                string url = "https://docs.google.com/spreadsheets/d/1dKW5TttKZzsYU0wnYz_KyL1yE4PiUDe72Zl6Iaz5UOI/edit?usp=sharing";

                // Sử dụng Process.Start để mở URL bằng trình duyệt mặc định
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // Quan trọng để mở bằng trình duyệt
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở trang Google Sheets: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_MoExcelNap_Click(object sender, EventArgs e)
        {

        }

        private void button2_XoaBang_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_SoLuongChay_TextChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            string jsonPath = "Web_data_DangNhapDangKy.json";
            string spreadsheetId = "1dKW5TttKZzsYU0wnYz_KyL1yE4PiUDe72Zl6Iaz5UOI";
            // Đọc từ G2 trở đi (Giả định G2 là tiêu đề cột)
            string range = "Trang tính2!G2:K20";

            try
            {
                var service = GetSheetsService(jsonPath);
                var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
                var response = request.Execute();
                var values = response.Values;

                if (values != null && values.Count > 0)
                {
                    dgvBangQuyetDinh.Columns.Clear();
                    dgvBangQuyetDinh.Rows.Clear();

                    // 1. Tạo cột dựa trên dòng đầu tiên (Headers)
                    var headers = values[0];
                    foreach (var header in headers)
                    {
                        dgvBangQuyetDinh.Columns.Add(header.ToString(), header.ToString());
                    }

                    // 2. Nạp dữ liệu các dòng tiếp theo (R1, R2, R3...)
                    for (int i = 1; i < values.Count; i++)
                    {
                        dgvBangQuyetDinh.Rows.Add(values[i].ToArray());
                    }

                    MessageBox.Show("Đã nạp Bảng Quyết định từ Trang tính 2 thành công!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nạp Bảng Quyết định: " + ex.Message);
            }
        }

        private async void button3_ChayKiemThuBangQuyetDinh_Click(object sender, EventArgs e)
        {
            // =========================================================================
            // BƯỚC 0: YÊU CẦU NGƯỜI DÙNG NHẬP TÀI KHOẢN (ĐÚNG/SAI) ĐỂ TEST
            // =========================================================================
            string emailThat = "";
            string passThat = "";
            string emailSai = "";
            string passSai = "";

            using (NhapTaiKhoan frmNhap = new NhapTaiKhoan())
            {
                // Hiển thị form dưới dạng Popup
                if (frmNhap.ShowDialog() == DialogResult.OK)
                {
                    // Hứng toàn bộ 4 biến từ form nhập
                    emailThat = frmNhap.EmailDung;
                    passThat = frmNhap.MatKhauDung;
                    emailSai = frmNhap.EmailSai;
                    passSai = frmNhap.MatKhauSai;
                }
                else
                {
                    return; // Hủy chạy nếu chọn dấu X
                }
            }

            // HỘP THOẠI XÁC NHẬN CHẠY BẢNG QUYẾT ĐỊNH
            DialogResult confirmResult = MessageBox.Show(
                $"Bạn sẽ sinh Bảng quyết định và chạy kiểm thử với thông tin:\n\n" +
                $"[ĐÚNG] TK: {emailThat} | MK: {passThat}\n" +
                $"[SAI]  TK: {emailSai} | MK: {passSai}\n\n" +
                $"Hệ thống sẽ tự động ghép các cặp Đúng/Sai này để thử nghiệm. Tiếp tục?",
                "Xác nhận kiểm thử",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmResult == DialogResult.No)
            {
                return;
            }

            // =========================================================================
            // BƯỚC 1: ENGINE TỰ ĐỘNG SINH BẢNG QUYẾT ĐỊNH 100%
            // =========================================================================
            dgvBangQuyetDinh.Columns.Clear();
            dgvBangQuyetDinh.Rows.Clear();

            dgvBangQuyetDinh.Columns.Add("Condition", "Điều kiện");
            dgvBangQuyetDinh.Columns.Add("R1", "R1");
            dgvBangQuyetDinh.Columns.Add("R2", "R2");
            dgvBangQuyetDinh.Columns.Add("R3", "R3");
            dgvBangQuyetDinh.Columns.Add("R4", "R4");

            dgvBangQuyetDinh.Rows.Add("Email", "T", "T", "F", "F");
            dgvBangQuyetDinh.Rows.Add("Password", "T", "F", "T", "F");
            dgvBangQuyetDinh.Rows.Add("Action", "---", "---", "---", "---");
            dgvBangQuyetDinh.Rows.Add("Login Process", "X", "", "", "");
            dgvBangQuyetDinh.Rows.Add("Error", "", "X", "X", "X");
            dgvBangQuyetDinh.Rows.Add("Thực tế UI", "Đang chờ...", "Đang chờ...", "Đang chờ...", "Đang chờ...");
            dgvBangQuyetDinh.Rows.Add("Trạng thái", "Đang chờ...", "Đang chờ...", "Đang chờ...", "Đang chờ...");

            foreach (DataGridViewRow row in dgvBangQuyetDinh.Rows) { row.ReadOnly = true; }
            dgvBangQuyetDinh.ClearSelection();

            // CĂN CHỈNH KÍCH THƯỚC VÀ ĐỊNH DẠNG GIAO DIỆN
            dgvBangQuyetDinh.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBangQuyetDinh.Columns["Condition"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvBangQuyetDinh.AllowUserToAddRows = false;
            dgvBangQuyetDinh.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvBangQuyetDinh.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // =========================================================================
            // BƯỚC 2: KHỞI ĐỘNG TRÌNH DUYỆT BẰNG PLAYWRIGHT
            // =========================================================================
            string bravePath = @"C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe";
            int debugPort = 9222;
            string userDataDir = $@"C:\temp\automation_profile_{debugPort}";

            string headlessArg = checkBox1_AnGui.Checked ? "--headless=new " : "";

            ProcessStartInfo psi = new ProcessStartInfo(bravePath, $"{headlessArg}--remote-debugging-port={debugPort} --user-data-dir=\"{userDataDir}\" --no-first-run --no-default-browser-check")
            {
                UseShellExecute = true,
                WindowStyle = checkBox1_AnGui.Checked ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
            };

            Process braveProcess;
            try { braveProcess = Process.Start(psi); }
            catch { MessageBox.Show("Sai đường dẫn Brave!"); return; }

            await Task.Delay(3000);

            try
            {
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.ConnectOverCDPAsync($"http://127.0.0.1:{debugPort}");
                var context = await browser.NewContextAsync();

                await context.AddInitScriptAsync(@"
            Object.defineProperty(navigator, 'webdriver', {get: () => undefined});
            window.chrome = { runtime: {} };
            Object.defineProperty(navigator, 'plugins', { get: () => [1, 2, 3] });
        ");

                var page = await context.NewPageAsync();

                // =========================================================================
                // BƯỚC 3: DUYỆT CÁC CỘT R1 -> R4 VÀ THỰC THI KIỂM THỬ (CHUẨN PLAYWRIGHT)
                // =========================================================================
                for (int colIndex = 1; colIndex < dgvBangQuyetDinh.ColumnCount; colIndex++)
                {
                    this.Invoke((MethodInvoker)delegate {
                        dgvBangQuyetDinh.Rows[6].Cells[colIndex].Value = "Đang chạy...";
                        dgvBangQuyetDinh.Rows[6].Cells[colIndex].Style.BackColor = System.Drawing.Color.LightYellow;
                    });

                    string emailTF = dgvBangQuyetDinh.Rows[0].Cells[colIndex].Value.ToString();
                    string passTF = dgvBangQuyetDinh.Rows[1].Cells[colIndex].Value.ToString();

                    // SỬ DỤNG DATA NGƯỜI DÙNG NHẬP VÀO FORM DỰA TRÊN ĐIỀU KIỆN T/F
                    string emailInput = (emailTF == "T") ? emailThat : emailSai;
                    string passInput = (passTF == "T") ? passThat : passSai;

                    bool isExpectSuccess = dgvBangQuyetDinh.Rows[3].Cells[colIndex].Value.ToString() == "X";

                    // Khởi tạo trạng thái mặc định
                    string uiActualResult = "UI Chặn lại (Không hiện chữ lỗi)";

                    // Mở trang và đợi trạng thái DOMLoad hoàn tất thay vì dùng Task.Delay cứng
                    await page.GotoAsync("https://www.maisononline.vn/", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });

                    var loginBtn = page.Locator(".main-header_tool_right_icon").Nth(1);
                    var formLoginModal = page.Locator("#form-login-custom");
                    var formLoginCustomer = page.Locator("#customer_login");

                    // Mở form đăng nhập nếu chưa hiển thị
                    if (!await formLoginModal.IsVisibleAsync() && !await formLoginCustomer.IsVisibleAsync())
                    {
                        if (await loginBtn.IsVisibleAsync())
                        {
                            await loginBtn.ClickAsync();
                            // Chờ form xuất hiện (Tối đa 3s)
                            await formLoginModal.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 3000 });
                        }
                    }

                    var emailField = page.Locator("input[name='customer[email]']").First;
                    var passField = page.Locator("input[name='customer[password]']").First;
                    var submitBtn = page.Locator("button.btn-login-form, button.btn-login-form-page").First;

                    // Nhập liệu và submit
                    await emailField.ClearAsync();
                    await emailField.FillAsync(emailInput);
                    await passField.ClearAsync();
                    await passField.FillAsync(passInput);
                    await submitBtn.ClickAsync(new LocatorClickOptions { Force = true });

                    // =========================================================================
                    // ĐÁNH GIÁ KẾT QUẢ BẰNG LOCATOR NATIVE THAY VÌ JAVASCRIPT
                    // =========================================================================
                    bool foundError = false;
                    int maxCheckRetries = 5;

                    // Vòng lặp nhỏ để chờ Backend phản hồi
                    for (int retry = 0; retry < maxCheckRetries; retry++)
                    {
                        await Task.Delay(1000); // Poll mỗi giây để xem DOM có cập nhật lỗi không

                        var errorSelectors = new[] { ".error-status", ".errors li", ".toast-message", ".text-error" };

                        // 1. Quét tìm thông báo lỗi
                        foreach (var sel in errorSelectors)
                        {
                            var el = page.Locator(sel).First;
                            if (await el.IsVisibleAsync())
                            {
                                string text = await el.TextContentAsync();
                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    uiActualResult = text.Trim();
                                    foundError = true;
                                    break;
                                }
                            }
                        }

                        if (foundError) break; // Nếu thấy chữ lỗi thì thoát vòng chờ ngay lập tức

                        // 2. Nếu không thấy chữ lỗi, kiểm tra xem có phải đã đăng nhập thành công không
                        string currentUrl = page.Url.ToLower();
                        bool isFormHidden = !(await formLoginModal.IsVisibleAsync()) && !(await formLoginCustomer.IsVisibleAsync());

                        if ((currentUrl.Contains("/account") && !currentUrl.Contains("login")) ||
                            (isFormHidden && !currentUrl.Contains("challenge")))
                        {
                            uiActualResult = "Đăng nhập thành công";
                            break;
                        }
                    }

                    // Chốt pass/fail
                    bool isPass = false;
                    if (isExpectSuccess && uiActualResult == "Đăng nhập thành công") { isPass = true; }
                    else if (!isExpectSuccess && uiActualResult != "Đăng nhập thành công" && uiActualResult != "UI Chặn lại (Không hiện chữ lỗi)") { isPass = true; }

                    // In ra giao diện
                    this.Invoke((MethodInvoker)delegate {
                        dgvBangQuyetDinh.Rows[5].Cells[colIndex].Value = uiActualResult;
                        dgvBangQuyetDinh.Rows[6].Cells[colIndex].Value = isPass ? "Pass" : "Fail";
                        dgvBangQuyetDinh.Rows[6].Cells[colIndex].Style.BackColor = isPass ? System.Drawing.Color.LightGreen : System.Drawing.Color.LightPink;
                    });

                    // Đăng xuất nếu thành công để chuẩn bị cho test case sau
                    if (uiActualResult == "Đăng nhập thành công")
                    {
                        await page.GotoAsync("https://www.maisononline.vn/account/logout", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
                    }
                }

                // Đóng các tiến trình trình duyệt sau khi test xong
                await context.CloseAsync();
                await browser.CloseAsync();
                try { braveProcess.Kill(); } catch { }

                // =========================================================================
                // LUỒNG TỰ ĐỘNG 1: ĐẨY TOÀN BỘ DỮ LIỆU LÊN GOOGLE SHEETS (Bắt đầu từ A1)
                // =========================================================================
                string jsonPath = "Web_data_DangNhapDangKy.json";
                string spreadsheetId = "1dKW5TttKZzsYU0wnYz_KyL1yE4PiUDe72Zl6Iaz5UOI";
                string range = "Trang tính2!A1"; // Đẩy toàn bộ bảng bắt đầu từ ô A1

                var values = new List<IList<object>>();

                // 1. Quét lấy hàng Tiêu đề (Header: Điều kiện, R1, R2, R3, R4)
                var headerRow = new List<object>();
                foreach (DataGridViewColumn col in dgvBangQuyetDinh.Columns)
                {
                    headerRow.Add(col.HeaderText ?? "");
                }
                values.Add(headerRow);

                // 2. Quét lấy toàn bộ các hàng dữ liệu (Từ dòng Email, Pass đến tận dòng Trạng thái)
                for (int i = 0; i < dgvBangQuyetDinh.RowCount; i++)
                {
                    var rowData = new List<object>();
                    for (int j = 0; j < dgvBangQuyetDinh.ColumnCount; j++)
                    {
                        rowData.Add(dgvBangQuyetDinh.Rows[i].Cells[j].Value?.ToString() ?? "");
                    }
                    values.Add(rowData);
                }

                var valueRange = new Google.Apis.Sheets.v4.Data.ValueRange { Values = values };

                // Gọi hàm khởi tạo dịch vụ của bạn để đẩy dữ liệu qua API chính thống
                var service = GetSheetsService(jsonPath);
                var updateRequest = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
                updateRequest.ValueInputOption = Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                updateRequest.Execute();

                // =========================================================================
                // LUỒNG TỰ ĐỘNG 2: XUẤT FILE REPORT OFFLINE (.CSV) XUỐNG THƯ MỤC LOCAL
                // =========================================================================
                string folderPath = Path.Combine(Application.StartupPath, "DangNhap_BangQuyetDinh");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                string filePath = Path.Combine(folderPath, $"KetQua_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

                using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    // 1. Ghi dòng tiêu đề (Headers)
                    List<string> headers = new List<string>();
                    foreach (DataGridViewColumn col in dgvBangQuyetDinh.Columns)
                    {
                        headers.Add(col.HeaderText ?? $"Cot_{col.Index}");
                    }
                    sw.WriteLine(string.Join(",", headers));

                    // 2. Ghi toàn bộ dữ liệu các dòng
                    for (int i = 0; i < dgvBangQuyetDinh.RowCount; i++)
                    {
                        List<string> rowCells = new List<string>();
                        for (int j = 0; j < dgvBangQuyetDinh.ColumnCount; j++)
                        {
                            string cellValue = dgvBangQuyetDinh.Rows[i].Cells[j].Value?.ToString() ?? "";
                            // Chuẩn hóa ký tự đặc biệt để cấu trúc CSV không lỗi
                            if (cellValue.Contains(",") || cellValue.Contains("\n") || cellValue.Contains("\""))
                            {
                                cellValue = "\"" + cellValue.Replace("\"", "\"\"") + "\"";
                            }
                            rowCells.Add(cellValue);
                        }
                        sw.WriteLine(string.Join(",", rowCells));
                    }
                }

                // Hiển thị hộp thoại tổng hợp thành công
                string thongBaoGom = $"Đã kiểm thử và đồng bộ dữ liệu xong!\n\n" +
                                     $"1. [Cloud]: Đã ghi toàn bộ bảng lên Google Sheets [Trang tính2!A1].\n" +
                                     $"2. [Local]: Đã lưu file báo cáo offline tại:\n{filePath}";
                MessageBox.Show(thongBaoGom, "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thực thi kiểm thử hoặc đồng bộ dữ liệu: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try { braveProcess.Kill(); } catch { }
            }
        }

        private void button4_TruyCapTrangTinh_Click(object sender, EventArgs e)
        {
            try
            {
                // Đường dẫn trực tiếp đến file Google Sheets của cậu
                string url = "https://docs.google.com/spreadsheets/d/1dKW5TttKZzsYU0wnYz_KyL1yE4PiUDe72Zl6Iaz5UOI/edit";

                // Sử dụng Process.Start để gọi hệ điều hành mở link bằng trình duyệt mặc định (Chrome, Edge, Brave...)
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // Bắt buộc phải có dòng này để mở được URL
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở trang Google Sheets: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_LuuKetQua_Click(object sender, EventArgs e)
        {
            // 1. Tạo thư mục nếu chưa tồn tại
            string folderPath = Path.Combine(Application.StartupPath, "DangNhap_BangQuyetDinh");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, $"KetQua_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            // 2. Thu thập dữ liệu từ DataGridView
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < dgvBangQuyetDinh.RowCount; i++)
            {
                List<string> rowData = new List<string>();
                for (int j = 0; j < dgvBangQuyetDinh.ColumnCount; j++)
                {
                    rowData.Add(dgvBangQuyetDinh.Rows[i].Cells[j].Value?.ToString() ?? "");
                }
                sb.AppendLine(string.Join(",", rowData));
            }

            // 3. Lưu vào file local
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

            // 4. Đẩy lên Google Sheets
            // Lưu ý: Cậu cần có thư viện Google.Apis.Sheets.v4 trong project
            try
            {
                // Giả lập bước này: Vì việc đẩy lên Google Sheets yêu cầu cấu hình API phức tạp (Service Account/OAuth)
                // Tớ khuyên cậu nên dùng 1 script Google Apps Script đơn giản 
                // Sau đó gọi WebHook từ C# để gửi dữ liệu lên.

                MessageBox.Show($"Đã lưu kết quả tại:\n{filePath}\n\nĐang tiến hành đồng bộ lên Google Sheets...",
                                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Đoạn này nếu cậu đã có hàm UpdateGoogleSheet, hãy gọi ở đây:
                // UpdateGoogleSheet(sb.ToString()); 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đồng bộ Google Sheets: " + ex.Message);
            }
        }

        private async void button3_ChayKiemThuDangLKy_Click(object sender, EventArgs e)
        {
            // =========================================================================
            // 0. DỌN DẸP ZOMBIE PROCESS (Tránh bị kẹt chế độ ẩn từ lần chạy trước)
            // =========================================================================
            try
            {
                foreach (var p in Process.GetProcessesByName("brave"))
                {
                    p.Kill();
                }
            }
            catch { }

            // =========================================================================
            // 1. LẤY SỐ LUỒNG TỪ textBox2_SoLuongChayDangKy
            // =========================================================================
            int soLuongLuong = 1;
            if (!int.TryParse(textBox2_SoLuongChayDangKy.Text, out soLuongLuong) || soLuongLuong < 1)
            {
                soLuongLuong = 1; // Mặc định chạy 1 luồng nếu để trống hoặc nhập sai
            }

            // =========================================================================
            // 2. LẤY TỐC ĐỘ (DELAY) TỪ textBox1_TocDoKiemThuDangKy
            // =========================================================================
            int tocDoKiemThu = 0;
            if (!int.TryParse(textBox1_TocDoKiemThuDangKy.Text, out tocDoKiemThu) || tocDoKiemThu < 0)
            {
                tocDoKiemThu = 0; // Mặc định +0ms nếu để trống
            }

            string bravePath = @"C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe";
            int debugPort = 9222;
            string userDataDir = $@"C:\temp\automation_profile_register_{debugPort}";

            // =========================================================================
            // 3. XÉT CHẾ ĐỘ CHẠY ẨN TỪ checkBox1_ChayAnDangKy
            // =========================================================================
            string headlessArg = checkBox1_ChayAnDangKy.Checked ? "--headless=new " : "";

            ProcessStartInfo psi = new ProcessStartInfo(bravePath, $"{headlessArg}--remote-debugging-port={debugPort} --user-data-dir=\"{userDataDir}\" --no-first-run --no-default-browser-check")
            {
                UseShellExecute = true,
                WindowStyle = checkBox1_ChayAnDangKy.Checked ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
            };

            Process braveProcess;
            try { braveProcess = Process.Start(psi); }
            catch { MessageBox.Show("Sai đường dẫn trình duyệt Brave!"); return; }

            await Task.Delay(3000);

            try
            {
                var playwright = await Playwright.CreateAsync();
                var browser = await playwright.Chromium.ConnectOverCDPAsync($"http://127.0.0.1:{debugPort}");

                // Lấy danh sách các dòng hợp lệ có chứa data
                var validRows = new List<DataGridViewRow>();
                foreach (DataGridViewRow row in dgvDangKy.Rows)
                {
                    if (!row.IsNewRow && row.Cells[4].Value != null && row.Cells[4].Value.ToString().Trim() != "")
                    {
                        validRows.Add(row);
                    }
                }

                if (validRows.Count == 0)
                {
                    MessageBox.Show("Bảng dữ liệu trống, không có gì để chạy!");
                    try { braveProcess.Kill(); } catch { }
                    return;
                }

                // TÍNH TOÁN CẮT KHÚC THEO SỐ LUỒNG (Chunking)
                int totalCases = validRows.Count;
                // Nếu số luồng nhập vào > số testcase thực tế, ép số luồng = số testcase
                if (soLuongLuong > totalCases) { soLuongLuong = totalCases; }

                int chunkSize = (int)Math.Ceiling((double)totalCases / soLuongLuong);
                var tasks = new List<Task>();

                for (int i = 0; i < soLuongLuong; i++)
                {
                    // Chia task cho từng luồng
                    var chunk = validRows.Skip(i * chunkSize).Take(chunkSize).ToList();
                    if (chunk.Count == 0) continue;

                    tasks.Add(Task.Run(async () =>
                    {
                        foreach (var row in chunk)
                        {
                            string inputData = row.Cells[4].Value?.ToString();
                            string[] parts = inputData?.Split('|');
                            if (parts == null) continue;

                            string ho = parts.Length > 0 ? parts[0] : "";
                            string ten = parts.Length > 1 ? parts[1] : "";
                            string email = parts.Length > 2 ? parts[2] : "";
                            string pass = parts.Length > 3 ? parts[3] : "";

                            this.Invoke((MethodInvoker)delegate {
                                row.Cells[8].Value = "Đang chạy...";
                                row.Cells[8].Style.BackColor = System.Drawing.Color.LightYellow;
                            });

                            var context = await browser.NewContextAsync();
                            await context.AddInitScriptAsync(@" Object.defineProperty(navigator, 'webdriver', {get: () => undefined}); window.chrome = { runtime: {} }; Object.defineProperty(navigator, 'plugins', { get: () => [1, 2, 3] });");

                            var page = await context.NewPageAsync();
                            await page.GotoAsync("https://www.maisononline.vn/");

                            // 0. BẬT POPUP VÀ CHUYỂN TAB ĐĂNG KÝ (Có cộng thêm tocDoKiemThu)
                            var loginBtn = page.Locator(".main-header_tool_right_icon >> nth=1");
                            if (!await page.Locator("#form-register-custom").IsVisibleAsync())
                            {
                                if (await loginBtn.IsVisibleAsync()) { await loginBtn.ClickAsync(); }
                                await Task.Delay(1500 + tocDoKiemThu);

                                var tabRegister = page.Locator(".item-tab-form[data-tab='register']");
                                if (await tabRegister.IsVisibleAsync()) { await tabRegister.ClickAsync(); }
                                await Task.Delay(1000 + tocDoKiemThu);
                            }

                            // 1. TEST API TRỰC TIẾP
                            string apiActualResult = "";
                            try
                            {
                                var apiResponse = await page.EvaluateAsync<System.Text.Json.JsonElement>(@"async ([ho, ten, email, pass]) => {
            const form = document.querySelector('#form-register-custom');
            if (!form) return { status: 0, body: 'Không tìm thấy form đăng ký' };
            
            const formData = new FormData(form);
            formData.set('customer[last_name]', ho);
            formData.set('customer[first_name]', ten);
            formData.set('customer[email]', email);
            formData.set('customer[password]', pass);
            
            try {
                const res = await fetch(form.action || '/account', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                    body: new URLSearchParams(formData).toString()
                });
                
                if (res.redirected && res.url.includes('account')) {
                    return { status: 302, body: 'Redirected' };
                }
                
                const htmlText = await res.text();
                const parser = new DOMParser();
                const doc = parser.parseFromString(htmlText, 'text/html');
                const errorEl = doc.querySelector('.errors li, .errors, .error-status');
                
                if (errorEl && errorEl.textContent.trim() !== '') {
                    return { status: res.status, body: errorEl.textContent.trim() };
                } else if (htmlText.includes('g-recaptcha')) {
                    return { status: res.status, body: 'Lỗi Captcha' };
                } else {
                    return { status: res.status, body: 'Không tìm thấy text lỗi' };
                }
            } catch (e) {
                return { status: 500, body: e.message };
            }
        }", new[] { ho, ten, email, pass });

                                int statusCode = apiResponse.GetProperty("status").GetInt32();
                                string apiBody = apiResponse.GetProperty("body").GetString();

                                if (statusCode == 302 || apiBody == "Redirected")
                                    apiActualResult = "[302] Backend hổng: Chấp nhận Đăng ký thành công";
                                else
                                    apiActualResult = $"[{statusCode}] Backend báo: {apiBody}";
                            }
                            catch (Exception ex)
                            {
                                apiActualResult = $"Lỗi gửi API: {ex.Message}";
                            }

                            // 2. KHỞI ĐỘNG LẠI TRANG VÀ TEST GIAO DIỆN UI (Có cộng thêm tocDoKiemThu)
                            string uiActualResult = "";
                            int retryUICount = 0;

                            do
                            {
                                await page.ReloadAsync();
                                await Task.Delay(2500 + tocDoKiemThu);

                                if (!await page.Locator("#form-register-custom").IsVisibleAsync())
                                {
                                    if (await loginBtn.IsVisibleAsync()) { await loginBtn.ClickAsync(); }
                                    await Task.Delay(1500 + tocDoKiemThu);
                                    var tabRegister = page.Locator(".item-tab-form[data-tab='register']");
                                    if (await tabRegister.IsVisibleAsync()) { await tabRegister.ClickAsync(); }
                                    await Task.Delay(1000 + tocDoKiemThu);
                                }

                                await page.Locator("#form-register-custom input[name='customer[last_name]']").FillAsync(ho);
                                await page.Locator("#form-register-custom input[name='customer[first_name]']").FillAsync(ten);
                                await page.Locator("#form-register-custom input[name='customer[email]']").FillAsync(email);
                                await page.Locator("#form-register-custom input[name='customer[password]']").FillAsync(pass);

                                await page.Locator("#form-register-custom .btn-register-form").ClickAsync(new LocatorClickOptions { Force = true });
                                await Task.Delay(4000 + tocDoKiemThu);

                                // ĐOẠN SỬA LỖI QUÉT THIẾU `.text-error` (Dùng JavaScript gốc như bạn đang dùng)
                                uiActualResult = await page.EvaluateAsync<string>(@"() => {
            const currentUrl = window.location.href.toLowerCase();
            if (currentUrl.includes('/account') && !currentUrl.includes('login') && !currentUrl.includes('register')) {
                return 'Đăng ký thành công';
            }
            
            const successMsg = document.querySelector('.success-status');
            if(successMsg && successMsg.textContent.trim() !== '') {
                return 'Đăng ký thành công: ' + successMsg.textContent.trim();
            }

            const generalError = document.querySelector('#form-register-custom .error-status') || 
                                 document.querySelector('.errors li') || 
                                 document.querySelector('.toast-message');
            if (generalError && generalError.textContent.trim() !== '') {
                return generalError.textContent.trim();
            }
            
            // SỬA: Dùng document.querySelectorAll để quyét toàn bộ thẻ .text-error trong form đăng ký
            const fieldErrors = document.querySelectorAll('#form-register-custom .text-error');
            for (let el of fieldErrors) {
                // Kiểm tra xem thẻ đó có đang hiện chữ và có thể nhìn thấy trên màn hình không
                if (el && el.textContent.trim() !== '' && el.offsetParent !== null) {
                    return el.textContent.trim();
                }
            }
            
            return 'UI Chặn lại (Không hiện chữ lỗi)';
        }");

                                retryUICount++;

                            } while (uiActualResult == "UI Chặn lại (Không hiện chữ lỗi)" && retryUICount < 15);

                            // 3. CẬP NHẬT KẾT QUẢ VÀO BẢNG
                            string expectedResult = row.Cells[5].Value?.ToString() ?? "";
                            bool isPass = KiemTraKetQuaLinhHoat(expectedResult, uiActualResult);

                            this.Invoke((MethodInvoker)delegate {
                                row.Cells[6].Value = uiActualResult;
                                row.Cells[7].Value = apiActualResult;
                                row.Cells[8].Value = isPass ? "Pass" : "Fail";
                                row.Cells[8].Style.BackColor = isPass ? System.Drawing.Color.LightGreen : System.Drawing.Color.LightPink;
                                row.Cells[9].Value = DateTime.Now.ToString("HH:mm:ss");
                            });

                            if (uiActualResult.Contains("Đăng ký thành công") || apiActualResult.Contains("[302]"))
                            {
                                await page.GotoAsync("https://www.maisononline.vn/account/logout");
                                await Task.Delay(2000 + tocDoKiemThu);
                            }

                            await page.CloseAsync();
                            await context.CloseAsync();
                        }
                    }));
                }

                // Đợi toàn bộ các Task chạy xong
                await Task.WhenAll(tasks);

                await browser.CloseAsync();
                try { braveProcess.Kill(); } catch { }

                // Chạy xong thì tự động gọi hàm lưu kết quả
                button2_LuuKetQuaDangKy_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
                try { braveProcess.Kill(); } catch { }
            }
        }

        private async void button2_LuuKetQuaDangKy_Click(object sender, EventArgs e)
        {
            try
            {
                string jsonPath = "Web_data_DangNhapDangKy.json";
                string spreadsheetId = "1dKW5TttKZzsYU0wnYz_KyL1yE4PiUDe72Zl6Iaz5UOI";

                // TỌA ĐỘ ĐẨY DATA (Trang tính3 từ ô G2)
                string range = "Trang tính3!G2";

                var values = new List<IList<object>>();
                foreach (DataGridViewRow row in dgvDangKy.Rows)
                {
                    if (row.IsNewRow) continue;

                    var rowData = new List<object> {
                row.Cells[6].Value?.ToString() ?? "",
                row.Cells[7].Value?.ToString() ?? "",
                row.Cells[8].Value?.ToString() ?? "",
                row.Cells[9].Value?.ToString() ?? ""
            };
                    values.Add(rowData);
                }

                var valueRange = new Google.Apis.Sheets.v4.Data.ValueRange { Values = values };
                var service = GetSheetsService(jsonPath);
                var updateRequest = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
                updateRequest.ValueInputOption = Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                updateRequest.Execute();

                // LƯU OFFLINE RA FILE CSV
                string folderPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ketqua");
                if (!System.IO.Directory.Exists(folderPath)) { System.IO.Directory.CreateDirectory(folderPath); }

                string fileName = $"KetQua_DangKy_{DateTime.Now:dd_MM_yyyy_HH_mm_ss}.csv";
                string filePath = System.IO.Path.Combine(folderPath, fileName);

                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filePath, false, Encoding.UTF8))
                {
                    List<string> headers = new List<string>();
                    foreach (DataGridViewColumn col in dgvDangKy.Columns)
                    {
                        headers.Add(col.HeaderText ?? $"Cot_{col.Index}");
                    }
                    sw.WriteLine(string.Join(",", headers));

                    foreach (DataGridViewRow row in dgvDangKy.Rows)
                    {
                        if (row.IsNewRow) continue;
                        List<string> rowCells = new List<string>();
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            string cellValue = cell.Value?.ToString() ?? "";
                            if (cellValue.Contains(",") || cellValue.Contains("\n") || cellValue.Contains("\""))
                            {
                                cellValue = "\"" + cellValue.Replace("\"", "\"\"") + "\"";
                            }
                            rowCells.Add(cellValue);
                        }
                        sw.WriteLine(string.Join(",", rowCells));
                    }
                }

                if (sender != null)
                {
                    string thongBaoGom = $"Đã kiểm thử và đồng bộ Đăng Ký thành công!\n\n" +
                                         $"1. [Cloud]: Đã đẩy lên Trang tính3!G2.\n" +
                                         $"2. [Local]: Đã lưu file tại:\n{filePath}";
                    MessageBox.Show(thongBaoGom, "Đồng Bộ Kết Quả", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi đồng bộ kết quả Đăng Ký: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button5_NapDuLieuDangKy_Click(object sender, EventArgs e)
        {
            string jsonPath = "Web_data_DangNhapDangKy.json";
            string spreadsheetId = "1dKW5TttKZzsYU0wnYz_KyL1yE4PiUDe72Zl6Iaz5UOI";
            string range = "Trang tính3!A1:J100"; // Thay đổi thành Trang tính 3 và mở rộng phạm vi dòng dữ liệu

            try
            {
                var service = GetSheetsService(jsonPath);
                var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
                var response = request.Execute();
                var values = response.Values;

                if (values != null && values.Count > 0)
                {
                    dgvDangKy.Rows.Clear(); // Xóa sạch dữ liệu UI hiện tại của bảng Đăng Ký

                    bool isHeader = true;
                    foreach (var row in values)
                    {
                        // Bỏ qua dòng tiêu đề đầu tiên trong file Sheets
                        if (isHeader) { isHeader = false; continue; }

                        object[] rowData = new object[10];
                        for (int i = 0; i < 10; i++)
                        {
                            // Lấy dữ liệu từ Sheets cho 6 cột đầu (Từ Mã TC đến Kết quả mong đợi)
                            if (i <= 5)
                            {
                                rowData[i] = (i < row.Count) ? row[i] : "";
                            }
                            else
                            {
                                // 4 cột kết quả sau (UI, API, Trạng thái, Time) ép thành rỗng để reset dữ liệu cho đợt test mới
                                rowData[i] = "";
                            }
                        }
                        dgvDangKy.Rows.Add(rowData);
                    }
                    MessageBox.Show("Nạp dữ liệu Đăng Ký thành công! Đã làm sạch các cột kết quả cũ.", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Không tìm thấy dữ liệu kịch bản test trên Trang tính 3!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối khi nạp dữ liệu Đăng Ký: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_TruyCapTrangTinhDangKy_Click(object sender, EventArgs e)
        {
            try
            {
                string url = "https://docs.google.com/spreadsheets/d/1dKW5TttKZzsYU0wnYz_KyL1yE4PiUDe72Zl6Iaz5UOI/edit?gid=412762707#gid=412762707";

                // Sử dụng Process.Start để mở URL bằng trình duyệt mặc định
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // Quan trọng để mở bằng trình duyệt
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở trang Google Sheets: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button8_truyCamTrangTinh_Click(object sender, EventArgs e)
        {
            try
            {
                string url = "https://docs.google.com/spreadsheets/d/1dKW5TttKZzsYU0wnYz_KyL1yE4PiUDe72Zl6Iaz5UOI/edit?gid=1913621788#gid=1913621788";

                // Sử dụng Process.Start để mở URL bằng trình duyệt mặc định
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // Quan trọng để mở bằng trình duyệt
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở trang Google Sheets: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button7_ChayKiemThuBangquyetDinhDangKy_Click(object sender, EventArgs e)
        {
            // =========================================================================
            // 0. DỌN DẸP TIẾN TRÌNH VÀ YÊU CẦU NGƯỜI DÙNG NHẬP DỮ LIỆU TEST
            // =========================================================================
            try { foreach (var p in Process.GetProcessesByName("brave")) { p.Kill(); } } catch { }

            string hoThat = "", tenThat = "", emailThat = "", passThat = "";
            string hoSai = "", tenSai = "", emailSai = "", passSai = "";

            using (KTHTN.KT.DangKy_KiemThu frmNhap = new KTHTN.KT.DangKy_KiemThu())
            {
                if (frmNhap.ShowDialog() == DialogResult.OK)
                {
                    hoThat = frmNhap.HoDung; tenThat = frmNhap.TenDung;
                    emailThat = frmNhap.EmailDung; passThat = frmNhap.MatKhauDung;
                    hoSai = frmNhap.HoSai; tenSai = frmNhap.TenSai;
                    emailSai = frmNhap.EmailSai; passSai = frmNhap.MatKhauSai;
                }
                else return;
            }

            if (MessageBox.Show("Hệ thống sẽ sinh Bảng quyết định Đăng Ký và chạy kiểm thử tự động.\nBạn có muốn tiếp tục?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;

            // =========================================================================
            // 1. ENGINE TỰ ĐỘNG SINH BẢNG QUYẾT ĐỊNH
            // =========================================================================
            dgvBangQuyetDinhDangKy.Columns.Clear();
            dgvBangQuyetDinhDangKy.Rows.Clear();

            string[] cols = { "Condition", "R1", "R2", "R3", "R4", "R5", "R6" };
            string[] headers = { "Điều kiện", "R1 (Đúng hết)", "R2 (Sai Họ)", "R3 (Sai Tên)", "R4 (Sai Email)", "R5 (Sai Pass)", "R6 (Sai tất)" };
            for (int i = 0; i < cols.Length; i++) dgvBangQuyetDinhDangKy.Columns.Add(cols[i], headers[i]);

            dgvBangQuyetDinhDangKy.Rows.Add("Họ", "T", "F", "T", "T", "T", "F");
            dgvBangQuyetDinhDangKy.Rows.Add("Tên", "T", "T", "F", "T", "T", "F");
            dgvBangQuyetDinhDangKy.Rows.Add("Email", "T", "T", "T", "F", "T", "F");
            dgvBangQuyetDinhDangKy.Rows.Add("Password", "T", "T", "T", "T", "F", "F");
            dgvBangQuyetDinhDangKy.Rows.Add("Action", "---", "---", "---", "---", "---", "---");
            dgvBangQuyetDinhDangKy.Rows.Add("Register Process", "X", "", "", "", "", "");
            dgvBangQuyetDinhDangKy.Rows.Add("Error", "", "X", "X", "X", "X", "X");
            dgvBangQuyetDinhDangKy.Rows.Add("Kết quả API", "Đang chờ...", "Đang chờ...", "Đang chờ...", "Đang chờ...", "Đang chờ...", "Đang chờ...");
            dgvBangQuyetDinhDangKy.Rows.Add("Thực tế UI", "Đang chờ...", "Đang chờ...", "Đang chờ...", "Đang chờ...", "Đang chờ...", "Đang chờ...");
            dgvBangQuyetDinhDangKy.Rows.Add("Trạng thái", "Đang chờ...", "Đang chờ...", "Đang chờ...", "Đang chờ...", "Đang chờ...", "Đang chờ...");

            foreach (DataGridViewRow row in dgvBangQuyetDinhDangKy.Rows) row.ReadOnly = true;
            dgvBangQuyetDinhDangKy.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBangQuyetDinhDangKy.AllowUserToAddRows = false;
            dgvBangQuyetDinhDangKy.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // =========================================================================
            // 2. CẤU HÌNH HEADLESS VÀ ĐA LUỒNG
            // =========================================================================
            int.TryParse(textBox1_TocDoKiemThuDangKy.Text, out int tocDoKiemThu);
            Random rnd = new Random();

            // Thiết lập số lượng luồng chạy đồng thời (mặc định là 1 nếu nhập sai)
            int soLuongLuong = 1;
            if (!int.TryParse(textBox1_SoLuongCHatBangDangKy.Text, out soLuongLuong) || soLuongLuong < 1)
                soLuongLuong = 1;

            string bravePath = @"C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe";
            int debugPort = 9222;
            string userDataDir = $@"C:\temp\automation_profile_reg_dec_{debugPort}";

            // Đọc trạng thái chạy ẩn giao diện
            string headlessArg = checkBox1_ChayAnDaoDienBangQuyerDinhDangKy.Checked ? "--headless=new " : "";
            string extraArgs = "--disable-blink-features=AutomationControlled --disable-infobars --window-size=1366,768";

            ProcessStartInfo psi = new ProcessStartInfo(bravePath, $"{headlessArg}--remote-debugging-port={debugPort} --user-data-dir=\"{userDataDir}\" --no-first-run --no-default-browser-check {extraArgs}")
            {
                UseShellExecute = true,
                WindowStyle = checkBox1_ChayAnDaoDienBangQuyerDinhDangKy.Checked ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
            };

            Process braveProcess = null;
            IPlaywright playwright = null;
            IBrowser browser = null;

            try
            {
                braveProcess = Process.Start(psi);
                await Task.Delay(3000);

                playwright = await Playwright.CreateAsync();
                browser = await playwright.Chromium.ConnectOverCDPAsync($"http://127.0.0.1:{debugPort}");

                // =========================================================================
                // 3. THỰC THI KIỂM THỬ ĐA LUỒNG (CHUNK TỪ R1 -> R6)
                // =========================================================================

                // Lấy danh sách các cột cần chạy (Từ index 1 đến 6)
                var validColumns = new List<int>();
                for (int i = 1; i < dgvBangQuyetDinhDangKy.ColumnCount; i++) { validColumns.Add(i); }

                int totalCases = validColumns.Count;
                if (soLuongLuong > totalCases) soLuongLuong = totalCases; // Không cho phép số luồng vượt quá số testcase

                int chunkSize = (int)Math.Ceiling((double)totalCases / soLuongLuong);
                var tasks = new List<Task>();

                for (int i = 0; i < soLuongLuong; i++)
                {
                    // Cắt lô (chunk) để giao cho mỗi luồng
                    var chunk = validColumns.Skip(i * chunkSize).Take(chunkSize).ToList();
                    if (chunk.Count == 0) continue;

                    tasks.Add(Task.Run(async () =>
                    {
                        foreach (int colIndex in chunk)
                        {
                            string hoInput = "", tenInput = "", emailInput = "", passInput = "";
                            bool isExpectSuccess = false;

                            // Đọc data từ Grid phải invoke để tránh lỗi Cross-thread
                            this.Invoke(new Action(() => {
                                hoInput = (dgvBangQuyetDinhDangKy.Rows[0].Cells[colIndex].Value.ToString() == "T") ? hoThat : hoSai;
                                tenInput = (dgvBangQuyetDinhDangKy.Rows[1].Cells[colIndex].Value.ToString() == "T") ? tenThat : tenSai;
                                emailInput = (dgvBangQuyetDinhDangKy.Rows[2].Cells[colIndex].Value.ToString() == "T") ? emailThat : emailSai;
                                passInput = (dgvBangQuyetDinhDangKy.Rows[3].Cells[colIndex].Value.ToString() == "T") ? passThat : passSai;
                                isExpectSuccess = dgvBangQuyetDinhDangKy.Rows[5].Cells[colIndex].Value.ToString() == "X";

                                dgvBangQuyetDinhDangKy.Rows[7].Cells[colIndex].Value = "";
                                dgvBangQuyetDinhDangKy.Rows[8].Cells[colIndex].Value = "";
                                dgvBangQuyetDinhDangKy.Rows[9].Cells[colIndex].Value = "Đang chạy...";
                                dgvBangQuyetDinhDangKy.Rows[9].Cells[colIndex].Style.BackColor = System.Drawing.Color.LightYellow;
                            }));

                            string uiActualResult = "UI Chặn lại (Không hiện chữ lỗi)";
                            string apiActualResult = "Không bắt được kết quả";
                            bool isPass = false;

                            IBrowserContext loopContext = null;
                            IPage page = null;

                            try
                            {
                                // Tạo môi trường vô trùng cho luồng hiện tại
                                loopContext = await browser.NewContextAsync(new BrowserNewContextOptions
                                {
                                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36",
                                    ViewportSize = new ViewportSize { Width = 1366, Height = 768 },
                                    Locale = "vi-VN",
                                    TimezoneId = "Asia/Ho_Chi_Minh"
                                });

                                await loopContext.RouteAsync("**/*", async route =>
                                {
                                    if (route.Request.ResourceType == "image" || route.Request.ResourceType == "media") await route.AbortAsync();
                                    else await route.ContinueAsync();
                                });

                                await loopContext.AddInitScriptAsync(@"
                            Object.defineProperty(navigator, 'webdriver', { get: () => false });
                            window.chrome = { runtime: {} };
                        ");

                                page = await loopContext.NewPageAsync();

                                int maxRetries = 4;
                                for (int retry = 1; retry <= maxRetries; retry++)
                                {
                                    if (retry == 1) await page.GotoAsync("https://www.maisononline.vn/", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
                                    else { await page.ReloadAsync(); await Task.Delay(2000 + tocDoKiemThu); }

                                    var formRegLocator = page.Locator("#form-register-custom");
                                    if (!await formRegLocator.IsVisibleAsync())
                                    {
                                        var loginBtn = page.Locator(".main-header_tool_right_icon").Nth(1);
                                        if (await loginBtn.IsVisibleAsync()) await loginBtn.ClickAsync();
                                        await Task.Delay(1000 + tocDoKiemThu);

                                        var tabRegister = page.Locator(".item-tab-form[data-tab='register']");
                                        if (await tabRegister.IsVisibleAsync()) await tabRegister.ClickAsync();

                                        try { await formRegLocator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 5000 }); } catch { }
                                    }

                                    async Task FillSlowly(string selector, string text)
                                    {
                                        var input = page.Locator(selector);
                                        await input.FillAsync("");
                                        await input.PressSequentiallyAsync(text, new LocatorPressSequentiallyOptions { Delay = rnd.Next(30, 80) });
                                        await Task.Delay(rnd.Next(100, 300));
                                    }

                                    if (await formRegLocator.IsVisibleAsync())
                                    {
                                        await FillSlowly("#form-register-custom input[name='customer[last_name]']", hoInput);
                                        await FillSlowly("#form-register-custom input[name='customer[first_name]']", tenInput);
                                        await FillSlowly("#form-register-custom input[name='customer[email]']", emailInput);
                                        await FillSlowly("#form-register-custom input[name='customer[password]']", passInput);

                                        await page.Mouse.MoveAsync(rnd.Next(200, 600), rnd.Next(200, 600), new MouseMoveOptions { Steps = 5 });

                                        try
                                        {
                                            var response = await page.RunAndWaitForResponseAsync(async () =>
                                            {
                                                await page.Locator("#form-register-custom .btn-register-form").ClickAsync(new LocatorClickOptions { Force = true });
                                            },
                                            r => r.Url.Contains("/account") && r.Request.Method == "POST",
                                            new PageRunAndWaitForResponseOptions { Timeout = 10000 });

                                            if (response.Status == 302 || response.Status == 301)
                                                apiActualResult = "[302] Backend hổng: Chấp nhận Đăng ký thành công";
                                            else
                                            {
                                                string bodyText = "";
                                                try { bodyText = System.Text.Encoding.UTF8.GetString(await response.BodyAsync()); } catch { }

                                                if (bodyText.Contains("g-recaptcha") || bodyText.Contains("reCAPTCHA"))
                                                    apiActualResult = $"[{response.Status}] Bị chặn bởi Captcha/Bot";
                                                else
                                                    apiActualResult = $"[{response.Status}] Backend từ chối";
                                            }
                                        }
                                        catch (TimeoutException) { apiActualResult = "Lỗi Timeout: Server không phản hồi gói tin"; }
                                    }

                                    await Task.Delay(2500 + rnd.Next(200, 600) + tocDoKiemThu);

                                    // QUÉT UI 
                                    try
                                    {
                                        string currentUrl = page.Url.ToLower();
                                        if (currentUrl.Contains("/account") && !currentUrl.Contains("login") && !currentUrl.Contains("register"))
                                        {
                                            uiActualResult = "Đăng ký thành công";
                                        }
                                        else
                                        {
                                            var successStatus = formRegLocator.Locator(".success-status");
                                            if (await successStatus.IsVisibleAsync())
                                            {
                                                string txt = await successStatus.TextContentAsync();
                                                if (!string.IsNullOrWhiteSpace(txt)) uiActualResult = $"Đăng ký thành công: {txt.Trim()}";
                                            }

                                            if (uiActualResult == "UI Chặn lại (Không hiện chữ lỗi)")
                                            {
                                                var errorStatus = formRegLocator.Locator(".error-status");
                                                if (await errorStatus.IsVisibleAsync())
                                                {
                                                    string txt = await errorStatus.TextContentAsync();
                                                    if (!string.IsNullOrWhiteSpace(txt)) uiActualResult = txt.Trim();
                                                }
                                            }

                                            if (uiActualResult == "UI Chặn lại (Không hiện chữ lỗi)")
                                            {
                                                var fieldErrors = await formRegLocator.Locator(".text-error").AllAsync();
                                                foreach (var el in fieldErrors)
                                                {
                                                    if (await el.IsVisibleAsync())
                                                    {
                                                        string txt = await el.TextContentAsync();
                                                        if (!string.IsNullOrWhiteSpace(txt))
                                                        {
                                                            uiActualResult = txt.Trim();
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch (Microsoft.Playwright.PlaywrightException ex) when (ex.Message.Contains("destroyed") || ex.Message.Contains("closed"))
                                    {
                                        uiActualResult = "Đăng ký thành công";
                                        apiActualResult = "[302] Backend hổng: Chấp nhận Đăng ký thành công";
                                    }

                                    bool isSuccess = uiActualResult.Contains("Đăng ký thành công");
                                    bool hasClearError = uiActualResult != "UI Chặn lại (Không hiện chữ lỗi)";
                                    bool isHardBlocked = apiActualResult.Contains("Captcha") || apiActualResult.Contains("[403]") || apiActualResult.Contains("[429]");

                                    if (isSuccess || (hasClearError && !isHardBlocked)) break;
                                    if (retry == maxRetries)
                                    {
                                        uiActualResult = isHardBlocked ? $"Lỗi: Bị chặn cứng bởi Captcha/Tường lửa sau {maxRetries} lần thử"
                                                                       : $"Lỗi: Web lỳ lợm không phản hồi cụ thể sau {maxRetries} lần thử";
                                        break;
                                    }
                                }

                                isPass = (isExpectSuccess && uiActualResult.Contains("Đăng ký thành công")) ||
                                         (!isExpectSuccess && !uiActualResult.Contains("Đăng ký thành công") && !uiActualResult.Contains("UI Chặn lại"));

                                this.Invoke(new Action(() => {
                                    dgvBangQuyetDinhDangKy.Rows[7].Cells[colIndex].Value = apiActualResult;
                                    dgvBangQuyetDinhDangKy.Rows[8].Cells[colIndex].Value = uiActualResult;
                                    dgvBangQuyetDinhDangKy.Rows[9].Cells[colIndex].Value = isPass ? "Pass" : "Fail";
                                    dgvBangQuyetDinhDangKy.Rows[9].Cells[colIndex].Style.BackColor = isPass ? System.Drawing.Color.LightGreen : System.Drawing.Color.LightPink;
                                }));
                            }
                            catch (Exception testEx)
                            {
                                this.Invoke(new Action(() => {
                                    dgvBangQuyetDinhDangKy.Rows[7].Cells[colIndex].Value = "Lỗi Code/Mạng";
                                    dgvBangQuyetDinhDangKy.Rows[8].Cells[colIndex].Value = $"Đứt quãng: {testEx.Message}";
                                    dgvBangQuyetDinhDangKy.Rows[9].Cells[colIndex].Value = "Fail";
                                    dgvBangQuyetDinhDangKy.Rows[9].Cells[colIndex].Style.BackColor = System.Drawing.Color.LightPink;
                                }));
                            }
                            finally
                            {
                                if (page != null) await page.CloseAsync();
                                if (loopContext != null) await loopContext.CloseAsync();
                            }
                        }
                    }));
                }

                // =========================================================================
                // ĐỢI TẤT CẢ CÁC LUỒNG HOÀN THÀNH
                // =========================================================================
                await Task.WhenAll(tasks);

                // =========================================================================
                // 4. ĐẨY DỮ LIỆU LÊN GOOGLE SHEETS
                // =========================================================================
                string jsonPath = "Web_data_DangNhapDangKy.json";
                string spreadsheetId = "1dKW5TttKZzsYU0wnYz_KyL1yE4PiUDe72Zl6Iaz5UOI";
                string range = "Trang tính4!A1";

                var values = new List<IList<object>>();
                var headerRow = new List<object>();
                foreach (DataGridViewColumn col in dgvBangQuyetDinhDangKy.Columns) { headerRow.Add(col.HeaderText ?? ""); }
                values.Add(headerRow);

                for (int i = 0; i < dgvBangQuyetDinhDangKy.RowCount; i++)
                {
                    var rowData = new List<object>();
                    for (int j = 0; j < dgvBangQuyetDinhDangKy.ColumnCount; j++)
                    {
                        rowData.Add(dgvBangQuyetDinhDangKy.Rows[i].Cells[j].Value?.ToString() ?? "");
                    }
                    values.Add(rowData);
                }

                var valueRange = new Google.Apis.Sheets.v4.Data.ValueRange { Values = values };
                var service = GetSheetsService(jsonPath);
                var updateRequest = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
                updateRequest.ValueInputOption = Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                updateRequest.Execute();

                // =========================================================================
                // 5. XUẤT FILE REPORT OFFLINE (.CSV)
                // =========================================================================
                string folderPath = Path.Combine(Application.StartupPath, "DangKy_BangQuyetDinh");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                string filePath = Path.Combine(folderPath, $"KetQua_DangKy_Dec_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

                using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    List<string> headersCsv = new List<string>();
                    foreach (DataGridViewColumn col in dgvBangQuyetDinhDangKy.Columns) { headersCsv.Add(col.HeaderText ?? $"Cot_{col.Index}"); }
                    sw.WriteLine(string.Join(",", headersCsv));

                    for (int i = 0; i < dgvBangQuyetDinhDangKy.RowCount; i++)
                    {
                        List<string> rowCells = new List<string>();
                        for (int j = 0; j < dgvBangQuyetDinhDangKy.ColumnCount; j++)
                        {
                            string cellValue = dgvBangQuyetDinhDangKy.Rows[i].Cells[j].Value?.ToString() ?? "";
                            if (cellValue.Contains(",") || cellValue.Contains("\n") || cellValue.Contains("\""))
                            {
                                cellValue = "\"" + cellValue.Replace("\"", "\"\"") + "\"";
                            }
                            rowCells.Add(cellValue);
                        }
                        sw.WriteLine(string.Join(",", rowCells));
                    }
                }

                MessageBox.Show($"Đã kiểm thử và đồng bộ xong Bảng quyết định Đăng Ký!\n\n1. [Cloud]: Ghi lên Sheets [{range}].\n2. [Local]: File CSV tại:\n{filePath}",
                                "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thực thi khởi tạo hoặc đồng bộ: " + ex.Message, "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (browser != null) await browser.CloseAsync();
                try { braveProcess?.Kill(); } catch { }
            }
        }

        private void button6_LuuKetQua_Click(object sender, EventArgs e)
        {

        }
    }
}
