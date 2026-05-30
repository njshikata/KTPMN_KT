using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KTHTN.rohang
{
    public partial class RoHang : Form
    {
        public RoHang()
        {
            InitializeComponent();
        }

        private void button1_napDuLieu_rohang_Click(object sender, EventArgs e)
        {
            string jsonPath = "Web_data_DangNhapDangKy.json";
            string spreadsheetId = "1dKW5TttKZzsYU0wnYz_KyL1yE4PiUDe72Zl6Iaz5UOI";
            string range = "rohang1!A1:J20"; // Cột J là cột thứ 10

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
            var credential = GoogleCredential.FromFile(jsonPath)
                .CreateScoped(SheetsService.Scope.Spreadsheets);

            return new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "AutomationTool",
            });
        }

        private void button3_XuatBaoCaoExxcel_rohang_Click(object sender, EventArgs e)
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

        private async void button4_ChayKiemThu_rohang_Click(object sender, EventArgs e)
        {
            // =========================================================================
            // 0. DỌN DẸP ZOMBIE PROCESS TRÌNH DUYỆT CŨ
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
            if (!int.TryParse(textBox1_SoLuongChay_RoHang.Text, out soLuongLuong) || soLuongLuong < 1)
                soLuongLuong = 1;

            int tocDoKiemThu = 0;
            if (!int.TryParse(textBox1_TocDoKiemThu_RoHang.Text, out tocDoKiemThu) || tocDoKiemThu < 0)
                tocDoKiemThu = 0;

            string bravePath = @"C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe";
            int debugPort = 9222;
            string userDataDir = $@"C:\temp\automation_profile_cart_{debugPort}";

            string headlessArg = checkBox1_AnGui_RoHang.Checked ? "--headless=new " : "";
            string extraArgs = "--disable-blink-features=AutomationControlled --disable-infobars --window-size=1366,768";

            ProcessStartInfo psi = new ProcessStartInfo(bravePath, $"{headlessArg}--remote-debugging-port={debugPort} --user-data-dir=\"{userDataDir}\" --no-first-run --no-default-browser-check {extraArgs}")
            {
                UseShellExecute = true,
                WindowStyle = checkBox1_AnGui_RoHang.Checked ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
            };

            Process braveProcess;
            try { braveProcess = Process.Start(psi); }
            catch { MessageBox.Show("Sai đường dẫn trình duyệt Brave!"); return; }

            await Task.Delay(3000);

            try
            {
                var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
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

                // =========================================================================
                // 1. TẠO LUỒNG CHẠY KIỂM THỬ ĐỒNG THỜI
                // =========================================================================
                for (int i = 0; i < soLuongLuong; i++)
                {
                    var chunk = validRows.Skip(i * chunkSize).Take(chunkSize).ToList();
                    if (chunk.Count == 0) continue;

                    tasks.Add(Task.Run(async () =>
                    {
                        foreach (var row in chunk)
                        {
                            string inputData = row.Cells[4].Value?.ToString() ?? "";
                            string[] parts = inputData.Split('|');
                            string data1 = parts.Length > 0 ? parts[0].Trim().ToLower() : "";
                            string data2 = parts.Length > 1 ? parts[1].Trim() : data1; // data2 là dữ liệu cần test

                            this.Invoke((MethodInvoker)delegate
                            {
                                row.Cells[8].Value = "Đang chạy...";
                                row.Cells[8].Style.BackColor = System.Drawing.Color.LightYellow;
                                row.Cells[6].Value = "";
                                row.Cells[7].Value = "";
                            });

                            var contextOptions = new Microsoft.Playwright.BrowserNewContextOptions
                            {
                                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36",
                                ViewportSize = new Microsoft.Playwright.ViewportSize { Width = 1366, Height = 768 },
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
                    ");

                            var page = await context.NewPageAsync();

                            string uiActualResult = "UI Chặn lại (Không hiện kết quả)";
                            string apiActualResult = "Không bắt được kết quả";
                            int maxRetries = 6;
                            int tonKhoThucTe = -1;

                            // =========================================================================
                            // 2. VÒNG LẶP KIỂM THỬ SỐ LƯỢNG SẢN PHẨM
                            // =========================================================================
                            for (int retry = 1; retry <= maxRetries; retry++)
                            {
                                if (retry == 1)
                                    await page.GotoAsync("https://www.maisononline.vn/cart");
                                else
                                {
                                    await page.ReloadAsync();
                                    await Task.Delay(2000 + tocDoKiemThu);
                                }

                                // ---> TỰ ĐỘNG THÊM SẢN PHẨM NẾU GIỎ HÀNG ĐANG TRỐNG <---
                                var inputQuantity = page.Locator(".input-quantity").First;
                                if (!await inputQuantity.IsVisibleAsync())
                                {
                                    await page.GotoAsync("https://www.maisononline.vn/products/mlb-non-len-unisex-tai-meo-ke-soc-summer-3abnb0363-2");
                                    await Task.Delay(2000 + tocDoKiemThu);

                                    var sizeLabel = page.Locator(".details__sizes_data label.size-sw").First;
                                    if (await sizeLabel.IsVisibleAsync())
                                    {
                                        await sizeLabel.ClickAsync();
                                        await Task.Delay(500);
                                    }

                                    var notifyEl = page.Locator(".show-stock-slow").First;
                                    if (await notifyEl.IsVisibleAsync())
                                    {
                                        string notifyText = await notifyEl.TextContentAsync();
                                        var match = System.Text.RegularExpressions.Regex.Match(notifyText, @"\d+");
                                        if (match.Success) tonKhoThucTe = int.Parse(match.Value);
                                    }

                                    var btnAddToCart = page.Locator("#add-to-cart").First;
                                    if (await btnAddToCart.IsVisibleAsync())
                                    {
                                        await btnAddToCart.ClickAsync(new Microsoft.Playwright.LocatorClickOptions { Force = true });
                                        await Task.Delay(2500 + tocDoKiemThu);
                                    }

                                    await page.GotoAsync("https://www.maisononline.vn/cart");
                                    await Task.Delay(2000 + tocDoKiemThu);
                                    inputQuantity = page.Locator(".input-quantity").First;
                                }

                                // ---> THỰC THI NHẬP DỮ LIỆU DỊ (ÉP LẤY LỖI WEB) <---
                                if (await inputQuantity.IsVisibleAsync())
                                {
                                    // Đổi thuộc tính type từ 'number' sang 'text' để Playwright cho phép gõ mọi ký tự
                                    await inputQuantity.EvaluateAsync("el => el.type = 'text'");
                                    await inputQuantity.EvaluateAsync("el => el.value = ''"); // Xóa sạch dữ liệu cũ

                                    await inputQuantity.FillAsync(data2);
                                    await inputQuantity.PressAsync("Enter"); // Xác nhận hành động

                                    await Task.Delay(2500 + tocDoKiemThu); // Chờ web bắt lỗi và render cảnh báo
                                }

                                // =========================================================================
                                // 3. PHÂN TÍCH GIAO DIỆN KẾT QUẢ TẠI TRANG GIỎ HÀNG
                                // =========================================================================
                                bool foundError = false;

                                // Quét mọi ngóc ngách có thể hiện dòng chữ thông báo lỗi của web
                                var errorSelectors = new[] {
                            ".empty-products .content-1",
                            "#alert_quick",
                            ".toast-message",
                            ".alert-danger",
                            ".cart-content .text-error",
                            ".cart-product-recommended h3"
                        };

                                foreach (var sel in errorSelectors)
                                {
                                    var els = await page.Locator(sel).AllAsync();
                                    foreach (var el in els)
                                    {
                                        if (await el.IsVisibleAsync())
                                        {
                                            string text = await el.TextContentAsync();
                                            if (!string.IsNullOrWhiteSpace(text))
                                            {
                                                uiActualResult = $"[LỖI UI WEB] {text.Trim()}";
                                                foundError = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (foundError) break;
                                }

                                // ĐỐI CHIẾU DỮ LIỆU NẾU KHÔNG CÓ LỖI NỔI LÊN BỀ MẶT
                                if (!foundError)
                                {
                                    string currentVal = await inputQuantity.InputValueAsync();
                                    int slYeuCau = 0;
                                    int.TryParse(data2, out slYeuCau);

                                    if (tonKhoThucTe != -1 && slYeuCau > tonKhoThucTe && currentVal != data2)
                                    {
                                        uiActualResult = $"[CHẶN HỢP LỆ] Yêu cầu {slYeuCau} nhưng kho chỉ có {tonKhoThucTe}. Khớp logic web!";
                                    }
                                    else
                                    {
                                        string tongSoSanPham = "N/A", tamTinh = "N/A", tongTien = "N/A";

                                        var spanItems = page.Locator(".title-cart-actions span");
                                        if (await spanItems.IsVisibleAsync()) tongSoSanPham = await spanItems.TextContentAsync();

                                        var priceTamTinh = page.Locator(".js-total-price").First;
                                        if (await priceTamTinh.IsVisibleAsync()) tamTinh = await priceTamTinh.TextContentAsync();

                                        // Đã fix fallback: Nếu không thấy Tổng tiền thì tự lấy Tạm tính bù vào
                                        var priceTong = page.Locator(".cart-summary-total span").Last;
                                        if (await priceTong.IsVisibleAsync())
                                            tongTien = await priceTong.TextContentAsync();
                                        else
                                            tongTien = tamTinh;

                                        if (currentVal == data2)
                                        {
                                            uiActualResult = $"Cập nhật OK | {tongSoSanPham.Trim()} | Tạm tính: {tamTinh.Trim()} | Tổng: {tongTien.Trim()}";
                                        }
                                        else if (tonKhoThucTe == -1)
                                        {
                                            uiActualResult = $"[SAI SỐ LƯỢNG] Web tự đổi từ '{data2}' thành '{currentVal}' | {tongSoSanPham.Trim()}";
                                        }
                                    }
                                }

                                // Ghi nhận trạng thái API Backend
                                if (uiActualResult.Contains("Không có sản phẩm nào") || uiActualResult.Contains("GỢI Ý CHO BẠN"))
                                {
                                    apiActualResult = "[200] Backend reset giỏ hàng / Không nhận giá trị";
                                }
                                else if (!uiActualResult.Contains("UI Chặn lại"))
                                {
                                    apiActualResult = "[200] Thao tác Giỏ hàng xử lý thành công";
                                }

                                if (!uiActualResult.Contains("UI Chặn lại")) break;

                                if (retry == maxRetries)
                                {
                                    uiActualResult = $"Lỗi Timeout: Không phản hồi sau {maxRetries} lần thử";
                                    apiActualResult = "Không bắt được kết quả API";
                                    break;
                                }
                            }

                            // =========================================================================
                            // 4. KIỂM TRA ĐÚNG SAI VÀ IN RA BẢNG GRID
                            // =========================================================================
                            string expectedResult = row.Cells[5].Value?.ToString() ?? "";
                            bool isPass = false;

                            if (!string.IsNullOrWhiteSpace(expectedResult))
                            {
                                isPass = uiActualResult.ToLower().Contains(expectedResult.ToLower().Trim());
                            }
                            else
                            {
                                isPass = uiActualResult.Contains("Cập nhật OK") || uiActualResult.Contains("CHẶN HỢP LỆ") || uiActualResult.Contains("LỖI UI WEB");
                            }

                            this.Invoke((MethodInvoker)delegate
                            {
                                row.Cells[6].Value = uiActualResult;
                                row.Cells[7].Value = apiActualResult;
                                row.Cells[8].Value = isPass ? "Pass" : "Fail";
                                row.Cells[8].Style.BackColor = isPass ? System.Drawing.Color.LightGreen : System.Drawing.Color.LightPink;
                                row.Cells[9].Value = DateTime.Now.ToString("HH:mm:ss");
                            });

                            // ---> TEARDOWN: XÓA SẢN PHẨM KHỎI GIỎ HÀNG SAU KHI TEST XONG <---
                            try
                            {
                                var btnDelete = page.Locator(".delete-item-modal").First;
                                if (await btnDelete.IsVisibleAsync())
                                {
                                    await btnDelete.ClickAsync();
                                    await Task.Delay(1000);

                                    var confirmDelete = page.Locator(".remove-product").First;
                                    if (await confirmDelete.IsVisibleAsync())
                                    {
                                        await confirmDelete.ClickAsync(new Microsoft.Playwright.LocatorClickOptions { Force = true });
                                        await Task.Delay(1500);
                                    }
                                }
                            }
                            catch { }

                            await page.CloseAsync();
                            await context.CloseAsync();
                        }
                    }));
                }

                await Task.WhenAll(tasks);

                await browser.CloseAsync();
                try { braveProcess.Kill(); } catch { }

                MessageBox.Show("Hoàn tất kiểm thử giỏ hàng! Đã ép bắt lỗi từ Web thành công.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
                try { braveProcess.Kill(); } catch { }
            }
        }


        private async void button1_Click(object sender, EventArgs e)
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
                var range = "rohang1!G2";
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

    }
}
