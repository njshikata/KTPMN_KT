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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KTHTN.ThanhToanPhiShip
{
    public partial class ThanhToanPhiShip : Form
    {
        public ThanhToanPhiShip()
        {
            InitializeComponent();
        }

        private void button1_napDuLieu_ThanhToanPhiShip_Click(object sender, EventArgs e)
        {
            string jsonPath = "Web_data_DangNhapDangKy.json";
            string spreadsheetId = "1dKW5TttKZzsYU0wnYz_KyL1yE4PiUDe72Zl6Iaz5UOI";
            string range = "thanhtoanphiship!A1:J20"; // Cột J là cột thứ 10

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
        private SheetsService GetSheetsService(string jsonPath)
        {
            var credential = GoogleCredential.FromFile(jsonPath).CreateScoped(SheetsService.Scope.Spreadsheets);
            return new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "AutomationTool",
            });
        }

        private void button3_XuatBaoCaoExxcel_ThanhToanPhiShip_Click(object sender, EventArgs e)
        {

        }

        private void button1_LuwuKetQua_ThanhToanPhiShip_Click(object sender, EventArgs e)
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
                var range = "thanhtoanphiship!G2";
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

        private async void button4_ChayKiemThu_ThanhToanPhiShip_Click(object sender, EventArgs e)
        {
            // =========================================================================
            // 0. DỌN DẸP TIẾN TRÌNH VÀ CẤU HÌNH
            // =========================================================================
            try { foreach (var p in Process.GetProcessesByName("brave")) { p.Kill(); } } catch { }

            int soLuongLuong = 1;
            if (!int.TryParse(textBox1_SoLuongChay_ThanhToanPhiShip.Text, out soLuongLuong) || soLuongLuong < 1) soLuongLuong = 1;

            int tocDoKiemThu = 0;
            if (!int.TryParse(textBox1_TocDoKiemThu_ThanhToanPhiShip.Text, out tocDoKiemThu) || tocDoKiemThu < 0) tocDoKiemThu = 0;

            string bravePath = @"C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe";
            int debugPort = 9225;
            string userDataDir = $@"C:\temp\automation_profile_shipping_{debugPort}";

            bool isHidden = checkBox1_AnGui_ThanHToanPhiShip.Checked;
            string headlessArg = isHidden ? "--headless=new " : "";
            string extraArgs = "--disable-blink-features=AutomationControlled --disable-infobars --window-size=1366,768";

            ProcessStartInfo psi = new ProcessStartInfo(bravePath, $"{headlessArg}--remote-debugging-port={debugPort} --user-data-dir=\"{userDataDir}\" --no-first-run --no-default-browser-check {extraArgs}")
            {
                UseShellExecute = true,
                WindowStyle = isHidden ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
            };

            Process braveProcess;
            try { braveProcess = Process.Start(psi); }
            catch { MessageBox.Show("Sai đường dẫn trình duyệt!"); return; }

            await Task.Delay(3000); // Chờ trình duyệt mở port

            try
            {
                var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
                var browser = await playwright.Chromium.ConnectOverCDPAsync($"http://127.0.0.1:{debugPort}");

                var validRows = new List<DataGridViewRow>();
                foreach (DataGridViewRow row in dgvBoundary.Rows)
                {
                    if (!row.IsNewRow && row.Cells[4].Value != null && row.Cells[4].Value.ToString().Trim() != "")
                        validRows.Add(row);
                }

                if (validRows.Count == 0) { MessageBox.Show("Bảng dữ liệu trống!"); try { braveProcess.Kill(); } catch { } return; }

                int totalCases = validRows.Count;
                if (soLuongLuong > totalCases) soLuongLuong = totalCases;

                int chunkSize = (int)Math.Ceiling((double)totalCases / soLuongLuong);
                var tasks = new List<Task>();

                for (int i = 0; i < soLuongLuong; i++)
                {
                    var chunk = validRows.Skip(i * chunkSize).Take(chunkSize).ToList();
                    if (chunk.Count == 0) continue;

                    tasks.Add(Task.Run(async () =>
                    {
                        var contextOptions = new Microsoft.Playwright.BrowserNewContextOptions
                        {
                            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36",
                            ViewportSize = new Microsoft.Playwright.ViewportSize { Width = 1366, Height = 768 }
                        };

                        var context = await browser.NewContextAsync(contextOptions);

                        // Chặn tải hình ảnh/video/font để chạy siêu tốc
                        await context.RouteAsync("**/*", async route =>
                        {
                            var type = route.Request.ResourceType;
                            if (type == "image" || type == "media" || type == "font")
                                await route.AbortAsync();
                            else
                                await route.ContinueAsync();
                        });

                        var page = await context.NewPageAsync();
                        await context.ClearCookiesAsync();

                        // Đánh dấu UI đang nạp giỏ hàng
                        this.Invoke((MethodInvoker)delegate {
                            foreach (var row in chunk)
                            {
                                row.Cells[8].Value = "Đang nạp giỏ hàng...";
                                row.Cells[8].Style.BackColor = System.Drawing.Color.LightYellow;
                            }
                        });

                        bool setupSuccess = false;

                        // =========================================================================
                        // BƯỚC 1: QUY TRÌNH TẠO GIỎ HÀNG VÀ VÀO TRANG THANH TOÁN (Bulletproof)
                        // =========================================================================
                        try
                        {
                            await page.GotoAsync("https://www.maisononline.vn/products/mlb-non-len-unisex-tai-meo-ke-soc-summer-3abnb0363-2", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 30000 });
                            await Task.Delay(3000); // Chờ DOM render xong

                            // Tắt popup quảng cáo nếu có (ẩn danh bằng JS Evaluate để không throw exception nếu không có)
                            try { await page.EvaluateAsync("document.querySelectorAll('.close-popup, #close-popup, .popup-close').forEach(el => el.click());"); } catch { }

                            // 2. Chọn Size (Quan trọng: Chỉ bắt các size không bị class .soldout)
                            var sizeOption = page.Locator(".details__sizes_data label.size-sw:not(.soldout)").First;
                            if (await sizeOption.IsVisibleAsync())
                            {
                                await sizeOption.ClickAsync(new Microsoft.Playwright.LocatorClickOptions { Force = true });
                                await Task.Delay(1000);
                            }

                            // 3. Ưu tiên tìm nút Mua Ngay (chuyển thẳng Checkout) hoặc Thêm Vào Giỏ
                            var btnBuyNow = page.Locator("#buy-now, .btn-buy-now").First;
                            var btnAddToCart = page.Locator("#add-to-cart, .add-to-cart").First;

                            if (await btnBuyNow.IsVisibleAsync())
                            {
                                await btnBuyNow.ClickAsync(new Microsoft.Playwright.LocatorClickOptions { Force = true });
                            }
                            else if (await btnAddToCart.IsVisibleAsync())
                            {
                                await btnAddToCart.ClickAsync(new Microsoft.Playwright.LocatorClickOptions { Force = true });
                                await Task.Delay(3500); // Phải chờ API thêm vào giỏ chạy xong

                                await page.GotoAsync("https://www.maisononline.vn/cart", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
                                await Task.Delay(2000);

                                var btnCheckout = page.Locator("#process-checkout, .checkout").First;
                                if (await btnCheckout.IsVisibleAsync())
                                    await btnCheckout.ClickAsync(new Microsoft.Playwright.LocatorClickOptions { Force = true });
                                else
                                    await page.GotoAsync("https://www.maisononline.vn/checkout"); // Backup
                            }
                            else
                            {
                                throw new Exception("Không tìm thấy nút Mua hàng (sản phẩm có thể đã ẩn/hết sạch hàng).");
                            }

                            // 4. Đợi sang được trang Checkout
                            await page.WaitForURLAsync("**/checkout**", new PageWaitForURLOptions { Timeout = 25000 });
                            await page.WaitForSelectorAsync(".select-province-checkout", new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = 15000 });

                            setupSuccess = true; // Set cờ thành công
                        }
                        catch (Exception ex)
                        {
                            // NẾU LỖI THÌ GHI LỖI VÀO BẢNG ĐỂ KHÔNG BỊ TREO "Đang chạy..."
                            this.Invoke((MethodInvoker)delegate {
                                foreach (var row in chunk)
                                {
                                    row.Cells[6].Value = $"Lỗi Setup Giỏ Hàng: {ex.Message}";
                                    row.Cells[8].Value = "Fail";
                                    row.Cells[8].Style.BackColor = System.Drawing.Color.LightPink;
                                    row.Cells[9].Value = DateTime.Now.ToString("HH:mm:ss");
                                }
                            });
                        }

                        // =========================================================================
                        // BƯỚC 2: CHẠY KIỂM THỬ CÁC ĐỊA CHỈ (HỖ TRỢ ÉP LỖI - NEGATIVE TESTING)
                        // =========================================================================
                        if (setupSuccess)
                        {
                            var selectProvince = page.Locator("select.select-province-checkout").First;
                            var selectDistrict = page.Locator("select.select-district-checkout").First;
                            var selectWard = page.Locator("select.select-ward-checkout").First;

                            foreach (var row in chunk)
                            {
                                string inputData = row.Cells[4].Value?.ToString() ?? "";
                                string[] parts = inputData.Split('|');

                                string tinhThanh = parts[0].Trim();
                                string quanHuyen = parts.Length > 1 ? parts[1].Trim() : "";
                                string phuongXa = parts.Length > 2 ? parts[2].Trim() : "";
                                string expectedFee = parts.Length > 3 ? parts[3].Trim().ToLower() : "";
                                string expectedTotal = parts.Length > 4 ? parts[4].Trim().ToLower() : "";

                                string uiActualResult = "Lỗi UI";
                                bool isPass = false;

                                this.Invoke((MethodInvoker)delegate {
                                    row.Cells[8].Value = "Đang test phí ship...";
                                    row.Cells[8].Style.BackColor = System.Drawing.Color.LightYellow;
                                });

                                try
                                {
                                    // Hàm "Tin Tặc": Tìm tương đối, nếu không có thì ÉP nhét dữ liệu rác vào DOM để test Server
                                    async Task SmartSelectOrHack(Microsoft.Playwright.ILocator selectLocator, string searchText)
                                    {
                                        // Case 1: Test thiếu dữ liệu (Bỏ trống)
                                        if (string.IsNullOrWhiteSpace(searchText) || searchText.ToLower() == "trống")
                                        {
                                            await selectLocator.SelectOptionAsync(new[] { new SelectOptionValue { Index = 0 } });
                                            return;
                                        }

                                        var options = await selectLocator.Locator("option").AllAsync();
                                        foreach (var opt in options)
                                        {
                                            string text = (await opt.TextContentAsync()).Trim();
                                            // Tìm thấy lựa chọn hợp lệ trên web
                                            if (text.ToLower().Contains(searchText.ToLower()))
                                            {
                                                string valueToSelect = await opt.GetAttributeAsync("value");
                                                await selectLocator.SelectOptionAsync(new[] { new SelectOptionValue { Value = valueToSelect } });
                                                return;
                                            }
                                        }

                                        // Case 2: Bypass UI - Không có trên web nhưng cứ dùng JS nhét vào để test API Backend
                                        await selectLocator.EvaluateAsync($@"(select, text) => {{
                                    let newOption = new Option(text, 'FAKE_ID_9999');
                                    select.add(newOption, undefined);
                                    select.value = 'FAKE_ID_9999';
                                    select.dispatchEvent(new Event('change', {{ bubbles: true }}));
                                }}", searchText);
                                    }

                                    // 1. CHỌN TỈNH THÀNH
                                    this.Invoke((MethodInvoker)delegate { row.Cells[6].Value = "Đang nhập Tỉnh..."; });
                                    await selectProvince.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
                                    await SmartSelectOrHack(selectProvince, tinhThanh);
                                    await Task.Delay(1500 + tocDoKiemThu);

                                    // 2. CHỌN QUẬN HUYỆN
                                    if (!string.IsNullOrEmpty(quanHuyen))
                                    {
                                        this.Invoke((MethodInvoker)delegate { row.Cells[6].Value = "Đang nhập Huyện..."; });
                                        await SmartSelectOrHack(selectDistrict, quanHuyen);
                                        await Task.Delay(1500 + tocDoKiemThu);

                                        // 3. CHỌN PHƯỜNG XÃ
                                        if (!string.IsNullOrEmpty(phuongXa))
                                        {
                                            this.Invoke((MethodInvoker)delegate { row.Cells[6].Value = "Đang nhập Xã..."; });
                                            await SmartSelectOrHack(selectWard, phuongXa);
                                            await Task.Delay(2000 + tocDoKiemThu); // Đợi API tính phí
                                        }
                                    }

                                    this.Invoke((MethodInvoker)delegate { row.Cells[6].Value = "Đang đọc kết quả tính tiền..."; });

                                    // Lấy kết quả từ giao diện
                                    var shippingFeeNode = page.Locator(".line-total.shipping-fee .value-line").First;
                                    var totalPriceNode = page.Locator(".line-total.total-price .value-line").First;
                                    var errorNode = page.Locator(".content-empty .text-empty").First;

                                    if (await errorNode.IsVisibleAsync())
                                    {
                                        uiActualResult = await errorNode.TextContentAsync();
                                    }
                                    else if (await shippingFeeNode.IsVisibleAsync())
                                    {
                                        string actualFee = (await shippingFeeNode.TextContentAsync()).Trim();
                                        string actualTotal = await totalPriceNode.IsVisibleAsync() ? (await totalPriceNode.TextContentAsync()).Trim() : "";

                                        uiActualResult = $"Ship: {actualFee} | Tổng: {actualTotal}";

                                        // BẮT LỖI LOGIC WEB: Nếu cố tình nhập sai mà web vẫn cho ra số tiền ship -> Web có BUG!
                                        if (actualFee != "0₫" && actualFee != "" && !actualFee.Contains("-"))
                                        {
                                            // Nếu dữ liệu đầu vào chứa từ khoá cố tình làm sai mà vẫn ra tiền thì đánh dấu luôn
                                            if (tinhThanh.Contains("Fake") || quanHuyen.Contains("Fake") || phuongXa.Contains("Fake"))
                                            {
                                                uiActualResult = $"[LỖI WEB NGHIÊM TRỌNG] Nhập địa chỉ fake nhưng vẫn tính phí: {actualFee}";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        uiActualResult = "Lỗi: Không hiển thị vùng giá tiền";
                                    }

                                    // Đánh giá Pass / Fail
                                    bool passFee = string.IsNullOrEmpty(expectedFee) || uiActualResult.ToLower().Contains(expectedFee);
                                    bool passTotal = string.IsNullOrEmpty(expectedTotal) || uiActualResult.ToLower().Contains(expectedTotal);
                                    isPass = passFee && passTotal;
                                }
                                catch (Exception ex)
                                {
                                    uiActualResult = $"Lỗi script: {ex.Message}";
                                }

                                // Trả kết quả cuối cùng về giao diện UI
                                this.Invoke((MethodInvoker)delegate {
                                    row.Cells[6].Value = uiActualResult.Trim();
                                    row.Cells[8].Value = isPass ? "Pass" : "Fail";
                                    row.Cells[8].Style.BackColor = isPass ? System.Drawing.Color.LightGreen : System.Drawing.Color.LightPink;
                                    row.Cells[9].Value = DateTime.Now.ToString("HH:mm:ss");
                                });
                            }
                        }
                        await page.CloseAsync();
                        await context.CloseAsync();
                    }));
                }

                await Task.WhenAll(tasks);
                await browser.CloseAsync();
                try { braveProcess.Kill(); } catch { }

                // Tự động đẩy kết quả lên Excel / Google Sheet
                this.Invoke((MethodInvoker)delegate { button1_LuwuKetQua_ThanhToanPhiShip_Click(null, null); });
            }
            catch (Exception ex) { MessageBox.Show("Lỗi Hệ Thống: " + ex.Message); }
        }
    }
}
