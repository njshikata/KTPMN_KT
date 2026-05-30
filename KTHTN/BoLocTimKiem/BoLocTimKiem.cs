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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KTHTN.BoLocTimKiem
{
    public partial class BoLocTimKiem : Form
    {
        public BoLocTimKiem()
        {
            InitializeComponent();
        }
        // Hàm hỗ trợ khởi tạo Google Sheets Service
        private SheetsService GetSheetsService(string jsonPath)
        {
            var credential = GoogleCredential.FromFile(jsonPath).CreateScoped(SheetsService.Scope.Spreadsheets);
            return new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "AutomationTool",
            });
        }
        private async void button4_ChayKiemThu_BoLocTimKiemBanhquyetDinh_Click(object sender, EventArgs e)
        {
            // =========================================================================
            // 0. DỌN DẸP ZOMBIE PROCESS VÀ ĐỌC CẤU HÌNH
            // =========================================================================
            try { foreach (var p in Process.GetProcessesByName("brave")) { p.Kill(); } } catch { }

            int soLuongLuong = 1;
            if (!int.TryParse(textBox1_SoLuongChay_BoLocTuongDuong.Text, out soLuongLuong) || soLuongLuong < 1) soLuongLuong = 1;

            int tocDoKiemThu = 0;
            if (!int.TryParse(textBox1_TocDoKiemThu_BoLoctuongDuong.Text, out tocDoKiemThu) || tocDoKiemThu < 0) tocDoKiemThu = 0;

            string bravePath = @"C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe";
            int debugPort = 9225;
            string userDataDir = $@"C:\temp\automation_profile_search_{debugPort}";

            bool isHidden = checkBox1_AnGui_bolocTuongDuong.Checked;
            string headlessArg = isHidden ? "--headless=new " : "";
            string extraArgs = "--disable-blink-features=AutomationControlled --disable-infobars --window-size=1366,768";

            ProcessStartInfo psi = new ProcessStartInfo(bravePath, $"{headlessArg}--remote-debugging-port={debugPort} --user-data-dir=\"{userDataDir}\" --no-first-run --no-default-browser-check {extraArgs}")
            {
                UseShellExecute = true,
                WindowStyle = isHidden ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal
            };

            Process braveProcess;
            try { braveProcess = Process.Start(psi); }
            catch { MessageBox.Show("Sai đường dẫn Brave!"); return; }

            await Task.Delay(3000);

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
                        var context = await browser.NewContextAsync(new Microsoft.Playwright.BrowserNewContextOptions { UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/124.0.0.0 Safari/537.36" });
                        await context.AddInitScriptAsync("Object.defineProperty(navigator, 'webdriver', { get: () => false });");
                        var page = await context.NewPageAsync();

                        foreach (var row in chunk)
                        {
                            string inputData = row.Cells[4].Value?.ToString() ?? "";
                            string[] parts = inputData.Split('|');
                            string keyword = parts[0].Trim();
                            string expected = parts.Length > 1 ? parts[1].Trim().ToLower() : "";

                            string uiActualResult = "Lỗi UI / Ngắt quãng";
                            int maxRetries = 3;

                            for (int retry = 1; retry <= maxRetries; retry++)
                            {
                                this.Invoke((MethodInvoker)delegate {
                                    row.Cells[8].Value = $"Đang chạy (Lần {retry}/3)...";
                                    row.Cells[8].Style.BackColor = System.Drawing.Color.LightYellow;
                                });

                                try
                                {
                                    await page.GotoAsync("https://www.maisononline.vn/", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
                                    await Task.Delay(1000 + tocDoKiemThu);

                                    var btnSearch = page.Locator(".btn-quick-search").First;
                                    if (await btnSearch.IsVisibleAsync()) { await btnSearch.ClickAsync(); await Task.Delay(500); }

                                    var inputSearch = page.Locator("input[name='q'].main-header_tool_search_input, input[name='q'].input-search-mobile").First;
                                    if (await inputSearch.IsVisibleAsync())
                                    {
                                        await inputSearch.ClearAsync();
                                        if (!string.IsNullOrEmpty(keyword)) await inputSearch.FillAsync(keyword);
                                        await inputSearch.PressAsync("Enter");
                                    }

                                    // CHIẾN THUẬT CHỜ 15 GIÂY (EPIC WAIT)
                                    var loading = page.Locator(".item-prod-search-loading").First;
                                    try { await loading.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = 15000 }); } catch { }

                                    try { await page.WaitForSelectorAsync(".filter-count-total span, .no-product p, .empty-mini-search .head-empty-search", new PageWaitForSelectorOptions { Timeout = 10000 }); } catch { }
                                    await Task.Delay(1000 + tocDoKiemThu);

                                    // BẮT KẾT QUẢ
                                    var textNotFound = page.Locator(".no-product p").First;
                                    var textFound = page.Locator(".filter-count-total span").First;
                                    var textEmptyMini = page.Locator(".empty-mini-search .head-empty-search").First;

                                    if (await textNotFound.IsVisibleAsync()) uiActualResult = await textNotFound.TextContentAsync();
                                    else if (await textEmptyMini.IsVisibleAsync()) uiActualResult = await textEmptyMini.TextContentAsync();
                                    else if (await textFound.IsVisibleAsync()) uiActualResult = (await textFound.TextContentAsync()).Trim();
                                    else if (await loading.IsVisibleAsync()) uiActualResult = "[BUG TIMEOUT] Web treo loading quá lâu!";
                                    else uiActualResult = "Web không trả về kết quả!";

                                    // Nếu bắt được text có ý nghĩa thì thoát vòng Retry
                                    if (!uiActualResult.Contains("Lỗi") && !uiActualResult.Contains("Loading")) break;
                                }
                                catch (Exception ex) { uiActualResult = $"Lỗi Script: {ex.Message}"; }
                            }

                            bool isPass = !string.IsNullOrEmpty(expected) ? uiActualResult.ToLower().Contains(expected) : uiActualResult.Contains("sản phẩm") || uiActualResult.Contains("Không tìm thấy");

                            this.Invoke((MethodInvoker)delegate {
                                row.Cells[6].Value = uiActualResult.Trim();
                                row.Cells[8].Value = isPass ? "Pass" : "Fail";
                                row.Cells[8].Style.BackColor = isPass ? System.Drawing.Color.LightGreen : System.Drawing.Color.LightPink;
                                row.Cells[9].Value = DateTime.Now.ToString("HH:mm:ss");
                            });
                        }
                        await page.CloseAsync();
                        await context.CloseAsync();
                    }));
                }

                await Task.WhenAll(tasks);
                await browser.CloseAsync();
                try { braveProcess.Kill(); } catch { }

                this.Invoke((MethodInvoker)delegate { button1_Click(null, null); });
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private async void button1_napDuLieu_Click(object sender, EventArgs e)
        {
            string jsonPath = "Web_data_DangNhapDangKy.json";
            string spreadsheetId = "1dKW5TttKZzsYU0wnYz_KyL1yE4PiUDe72Zl6Iaz5UOI";
            string range = "BoLocTuongDuong!A1:J20"; // Cột J là cột thứ 10

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

        private void BoLocTimKiem_Load(object sender, EventArgs e)
        {

        }
    }
}
