using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using FoodOrderingSystem.Data; 
using FoodOrderingSystem.Models; 
using System.Linq; // Added for filtering
using System.Text; // Added for StringBuilder

namespace FoodOrderingSystem.Forms 
{
    public class HistoryForm : Form
    {
        private readonly DatabaseService _dbService;
        private FlowLayoutPanel _previewPanel = null!; 
        
        public HistoryForm()
        {
            _dbService = new DatabaseService();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Receipt Preview & Reports";
            this.Size = new Size(450, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            _previewPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            // Bottom Panel for Actions
            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10),
                BackColor = Color.WhiteSmoke
            };

            // Print/Download Button (Icon Only)
            Button btnDownload = new Button
            {
                Text = "🖨", // Icon only
                Font = new Font("Segoe UI Emoji", 20), // Use Emoji font for better icon rendering
                Dock = DockStyle.Right,
                Width = 60, 
                BackColor = Color.Teal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            ToolTip tip = new ToolTip();
            tip.SetToolTip(btnDownload, "Print/Download Current Receipt");
            btnDownload.Click += (s, e) => DownloadReceipt();

            // Monthly Report Button (With Text)
            Button btnMonthly = new Button
            {
                Text = "📅 Print Monthly Record",
                Dock = DockStyle.Left,
                Width = 180, // Wider to fit text
                BackColor = Color.Orange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnMonthly.Click += (s, e) => DownloadMonthlyReport();

            bottomPanel.Controls.Add(btnDownload);
            bottomPanel.Controls.Add(btnMonthly);
            
            this.Controls.Add(_previewPanel);
            this.Controls.Add(bottomPanel);
        }

        // Store current order details for downloading
        private int _currentOrderId;
        private string _currentDate = "";
        private string _currentTotal = "";
        private OrderRecord? _currentOrderDetails;

        public async Task GenerateReceiptPreviewAsync(OrderRecord order) 
        {
            _currentOrderId = order.Id;
            // Use a specific format that works well with Excel/CSV
            _currentDate = order.Date.ToString("yyyy-MM-dd HH:mm:ss");
            _currentTotal = order.Total.ToString("N2");
            _currentOrderDetails = order;

            await GenerateReceiptPreviewAsync(order.Id, _currentDate, _currentTotal);
        }

        public async Task GenerateReceiptPreviewAsync(int orderId, string? date, string? total) 
        {
            _currentOrderId = orderId;
            // Ensure date is formatted if passed as null
            _currentDate = date ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _currentTotal = total ?? "0.00";

            _previewPanel.Controls.Clear();

            Label lblHeader = new Label
            {
                Text = "RECEIPT",
                Font = new Font("Courier New", 16, FontStyle.Bold),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 20)
            };
            _previewPanel.Controls.Add(lblHeader);

            Label lblDetails = new Label
            {
                Text = $"Order #{orderId}\n{_currentDate}",
                Font = new Font("Courier New", 10),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            _previewPanel.Controls.Add(lblDetails);

            var order = await _dbService.GetOrderDetailsAsync(orderId);
            _currentOrderDetails = order; 

            if (order != null && order.DetailedItems != null)
            {
                foreach (var item in order.DetailedItems)
                {
                    Panel itemPanel = new Panel { Width = 340, Height = 25, Margin = new Padding(0) };
                    
                    Label name = new Label { Text = $"{item.Quantity}x {item.Name}", AutoSize = true, Location = new Point(0, 0), Font = new Font("Courier New", 9) };
                    Label price = new Label { Text = $"{item.Total:N2}", AutoSize = true, Location = new Point(280, 0), Font = new Font("Courier New", 9) };

                    itemPanel.Controls.Add(name);
                    itemPanel.Controls.Add(price);
                    _previewPanel.Controls.Add(itemPanel);
                }
            }
            else
            {
                 Label lblError = new Label { Text = "Order details not found.", ForeColor = Color.Red, AutoSize = true };
                 _previewPanel.Controls.Add(lblError);
            }

            Label lblTotal = new Label
            {
                Text = $"--------------------------------\nTOTAL: {_currentTotal}",
                Font = new Font("Courier New", 12, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 20, 0, 0)
            };
            _previewPanel.Controls.Add(lblTotal);
        }

        private void DownloadReceipt()
        {
            if (_currentOrderDetails == null) return;

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text File|*.txt|CSV File|*.csv";
                sfd.FileName = $"Receipt_{_currentOrderId}.txt";
                
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sb = new StringBuilder();
                        if (sfd.FilterIndex == 2) // CSV
                        {
                            sb.AppendLine("OrderId,Date,Item,Quantity,Price,Total");
                            foreach(var item in _currentOrderDetails.DetailedItems)
                            {
                                // Enclose date in quotes or rely on yyyy-MM-dd format which Excel handles better
                                // Adding a tab or single quote helps prevent Excel auto-formatting issues (like #####)
                                string formattedDate = $"\t{_currentDate}"; 
                                sb.AppendLine($"{_currentOrderId},{formattedDate},{item.Name},{item.Quantity},{item.Price},{item.Total}");
                            }
                        }
                        else // Text
                        {
                            sb.AppendLine("RECEIPT");
                            sb.AppendLine($"Order #{_currentOrderId}");
                            sb.AppendLine($"Date: {_currentDate}");
                            sb.AppendLine("--------------------------------");
                            foreach(var item in _currentOrderDetails.DetailedItems)
                            {
                                sb.AppendLine($"{item.Quantity}x {item.Name}".PadRight(20) + $"{item.Total:N2}".PadLeft(10));
                            }
                            sb.AppendLine("--------------------------------");
                            sb.AppendLine($"TOTAL: {_currentTotal}");
                        }

                        System.IO.File.WriteAllText(sfd.FileName, sb.ToString());
                        MessageBox.Show("Receipt saved successfully!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving receipt: {ex.Message}");
                    }
                }
            }
        }

        // New Module: Monthly Report Generation
        private async void DownloadMonthlyReport()
        {
            // Simple Month Selector using standard InputBox logic (simulated with a small form)
            using (Form prompt = new Form())
            {
                prompt.Width = 300;
                prompt.Height = 180;
                prompt.Text = "Select Month";
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.MaximizeBox = false;
                prompt.MinimizeBox = false;

                Label textLabel = new Label() { Left = 20, Top = 20, Text = "Select month to export:" };
                DateTimePicker dtp = new DateTimePicker() { Left = 20, Top = 50, Width = 240, Format = DateTimePickerFormat.Custom, CustomFormat = "MMMM yyyy" };
                dtp.ShowUpDown = true; // Makes it easier to pick month/year without calendar popup
                
                Button confirmation = new Button() { Text = "Export", Left = 160, Width = 100, Top = 90, DialogResult = DialogResult.OK };
                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(dtp);
                prompt.Controls.Add(confirmation);
                prompt.AcceptButton = confirmation;

                if (prompt.ShowDialog() == DialogResult.OK)
                {
                    DateTime selectedMonth = dtp.Value;
                    await GenerateMonthlyCSV(selectedMonth);
                }
            }
        }

        private async Task GenerateMonthlyCSV(DateTime month)
        {
            var allOrders = await _dbService.GetOrdersAsync();
            var filteredOrders = allOrders.Where(o => o.Date.Month == month.Month && o.Date.Year == month.Year).ToList();

            if (filteredOrders.Count == 0)
            {
                MessageBox.Show($"No records found for {month:MMMM yyyy}.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV File|*.csv";
                sfd.FileName = $"Monthly_Report_{month:yyyy_MM}.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        // Header
                        sb.AppendLine($"Monthly Report for {month:MMMM yyyy}");
                        sb.AppendLine("Order ID,Date,Customer,Total Amount,Status,Items Summary");

                        decimal monthlyTotal = 0;

                        foreach (var order in filteredOrders)
                        {
                            // Escape commas in Items string
                            string safeItems = $"\"{order.Items.Replace("\"", "\"\"")}\"";
                            // Format date explicitly for CSV to avoid ##### in Excel (using yyyy-MM-dd HH:mm)
                            // Prepending a tab (\t) forces Excel to treat it as text, preventing ##### for column width issues
                            string formattedDate = $"\t{order.Date.ToString("yyyy-MM-dd HH:mm")}"; 
                            
                            sb.AppendLine($"{order.Id},{formattedDate},{order.CustomerName},{order.Total},{order.Status},{safeItems}");
                            monthlyTotal += order.Total;
                        }

                        sb.AppendLine($",,,Total Revenue:,{monthlyTotal},");

                        System.IO.File.WriteAllText(sfd.FileName, sb.ToString());
                        MessageBox.Show($"Report for {month:MMMM yyyy} saved successfully!\nTotal Orders: {filteredOrders.Count}\nTotal Revenue: {monthlyTotal:C2}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}