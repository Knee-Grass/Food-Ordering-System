using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using FoodOrderingSystem.Data; 
using FoodOrderingSystem.Models; 
using System.Linq; 
using System.Text; 
using System.IO; 
using System.ComponentModel; 

namespace FoodOrderingSystem.Forms 
{
    public class HistoryForm : Form
    {
        private readonly DatabaseService _dbService;
        private FlowLayoutPanel _previewPanel = null!; 
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CashierName { get; set; } = "Unknown"; 
        
        public HistoryForm()
        {
            _dbService = new DatabaseService();
            InitializeUI(); 
        }

        private void InitializeUI()
        {
            this.Text = "Receipt Preview & Reports";
            this.Size = new Size(550, 850); 
            this.WindowState = FormWindowState.Maximized; 
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

            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10),
                BackColor = Color.WhiteSmoke
            };

            Button btnDone = new Button
            {
                Text = "Done", 
                Font = new Font("Segoe UI", 12, FontStyle.Bold), 
                Dock = DockStyle.Right,
                Width = 120, 
                BackColor = Color.Gray, 
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnDone.Click += (s, e) => this.Close();

            Button btnPrint = new Button
            {
                Text = "🖨 Print", 
                Font = new Font("Segoe UI", 12, FontStyle.Bold), 
                Dock = DockStyle.Left, 
                Width = 120, 
                BackColor = Color.Teal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnPrint.Click += (s, e) => DownloadReceipt();

            bottomPanel.Controls.Add(btnDone);
            bottomPanel.Controls.Add(btnPrint);
            
            this.Controls.Add(bottomPanel);
            this.Controls.Add(_previewPanel);
        }

        private int _currentOrderId;
        private string _currentDate = "";
        private string _currentTotal = "";
        private OrderRecord? _currentOrderDetails;

        public async Task GenerateReceiptPreviewAsync(OrderRecord order) 
        {
            _currentOrderId = order.Id;
            _currentDate = order.Date.ToString("yyyy-MM-dd HH:mm:ss");
            _currentTotal = order.Total.ToString("N2");
            _currentOrderDetails = order;

            string recordCashier = !string.IsNullOrEmpty(order.CashierName) ? order.CashierName : CashierName;

            await GenerateReceiptPreviewAsync(order.Id, _currentDate, _currentTotal, recordCashier, order.OrderCode);
        }

        public async Task GenerateReceiptPreviewAsync(int orderId, string? date, string? total, string cashier, string orderCode) 
        {
            _currentOrderId = orderId;
            _currentDate = date ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _currentTotal = total ?? "0.00";
            
            CashierName = cashier;

            _previewPanel.Controls.Clear();

            Label lblHeader = new Label
            {
                Text = "OFFICIAL RECEIPT",
                Font = new Font("Courier New", 14, FontStyle.Bold),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 10),
                Width = 480
            };
            _previewPanel.Controls.Add(lblHeader);

            string displayCode = !string.IsNullOrEmpty(orderCode) ? orderCode : $"#{orderId}";
            
            // Legacy check
            if (string.IsNullOrEmpty(orderCode) && _currentOrderDetails != null && _currentOrderDetails.CustomerName.Contains("[Code:"))
            {
                 int start = _currentOrderDetails.CustomerName.IndexOf("[Code:") + 6;
                 int end = _currentOrderDetails.CustomerName.IndexOf("]");
                 if (start > 0 && end > start)
                 {
                     displayCode = _currentOrderDetails.CustomerName.Substring(start, end - start).Trim();
                 }
            }

            Label lblBarcode = new Label
            {
                Text = displayCode,
                Font = new Font("Courier New", 36, FontStyle.Bold), 
                AutoSize = true, 
                MaximumSize = new Size(480, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 20),
                ForeColor = Color.Black
            };
            _previewPanel.Controls.Add(lblBarcode);

            Label lblDate = new Label
            {
                Text = $"{_currentDate}",
                Font = new Font("Courier New", 10),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 15)
            };
            _previewPanel.Controls.Add(lblDate);

            // Updated Info Display
            string custName = _currentOrderDetails?.CustomerName ?? "Guest";
            // Clean name for display if it has legacy code
            if(custName.Contains("[Code:")) custName = custName.Split('[')[0].Trim();

            // Cashier - Show ONLY if a real cashier is assigned (not "Self-Service" or empty)
            if (!string.IsNullOrEmpty(cashier) && cashier != "Self-Service")
            {
                Label lblCashierTitle = new Label { Text = "CASHIER:", Font = new Font("Courier New", 10, FontStyle.Bold), AutoSize = true };
                _previewPanel.Controls.Add(lblCashierTitle);
                Label lblCashierName = new Label { Text = cashier, Font = new Font("Courier New", 12), AutoSize = true, MaximumSize = new Size(480, 0), Margin = new Padding(0, 0, 0, 15) };
                _previewPanel.Controls.Add(lblCashierName);
            }

            // Customer
            Label lblCustomerTitle = new Label { Text = "CUSTOMER:", Font = new Font("Courier New", 10, FontStyle.Bold), AutoSize = true };
            _previewPanel.Controls.Add(lblCustomerTitle);
            Label lblCustomerName = new Label { Text = custName, Font = new Font("Courier New", 12), AutoSize = true, MaximumSize = new Size(480, 0), Margin = new Padding(0, 0, 0, 15) };
            _previewPanel.Controls.Add(lblCustomerName);

            // ID
            Label lblOrderId = new Label { Text = $"ORDER #: {orderId}", Font = new Font("Courier New", 10, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 0, 0, 20) };
            _previewPanel.Controls.Add(lblOrderId);

            Label lblSep1 = new Label { Text = "------------------------------------------", Font = new Font("Courier New", 12), AutoSize = true };
            _previewPanel.Controls.Add(lblSep1);

            var order = await _dbService.GetOrderDetailsAsync(orderId);
            _currentOrderDetails = order; 

            if (order != null && order.DetailedItems != null)
            {
                foreach (var item in order.DetailedItems)
                {
                    Panel itemPanel = new Panel { Width = 480, Height = 25, Margin = new Padding(0) };
                    Label name = new Label { Text = $"{item.Quantity}x {item.Name}", AutoSize = true, Location = new Point(0, 0), Font = new Font("Courier New", 10) };
                    Label price = new Label { Text = $"{item.Total:N2}", AutoSize = true, Location = new Point(400, 0), Font = new Font("Courier New", 10) };
                    itemPanel.Controls.Add(name); itemPanel.Controls.Add(price);
                    _previewPanel.Controls.Add(itemPanel);
                }
            }

            Label lblTotal = new Label
            {
                Text = $"------------------------------------------\nTOTAL:     {_currentTotal}",
                Font = new Font("Courier New", 16, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0)
            };
            _previewPanel.Controls.Add(lblTotal);

            // --- INSTRUCTION TEXT ---
            // Show this instruction IF:
            // 1. The CashierName is "Self-Service"
            // 2. OR CashierName is null/empty (meaning it hasn't been picked up by a real cashier yet)
            if (CashierName == "Self-Service" || string.IsNullOrEmpty(CashierName))
            {
                Label lblInstruct = new Label 
                { 
                    Text = "\n\n*** PLEASE HAND THIS RECEIPT ***\n*** TO THE CASHIER FOR PAYMENT ***\n", 
                    Font = new Font("Courier New", 12, FontStyle.Bold), 
                    ForeColor = Color.Red,
                    AutoSize = true, 
                    TextAlign = ContentAlignment.MiddleCenter 
                };
                _previewPanel.Controls.Add(lblInstruct);
            }
        }

        private void DownloadReceipt()
        {
            if (_currentOrderDetails == null) return;
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text File|*.txt";
                sfd.FileName = $"Receipt_{_currentOrderId}.txt";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sb = new StringBuilder();
                        string custName = _currentOrderDetails.CustomerName;
                        if(custName.Contains("[Code:")) custName = custName.Split('[')[0].Trim();

                        string orderCode = !string.IsNullOrEmpty(_currentOrderDetails.OrderCode) ? _currentOrderDetails.OrderCode : "N/A";

                        sb.AppendLine("          OFFICIAL RECEIPT");
                        sb.AppendLine("--------------------------------");
                        sb.AppendLine($"CODE    : {orderCode}"); 
                        sb.AppendLine($"ORDER # : {_currentOrderId}"); 
                        sb.AppendLine("--------------------------------");
                        sb.AppendLine($"Date    : {_currentDate}");
                        sb.AppendLine("");

                        // Logic for printing Cashier name in text file
                        if (!string.IsNullOrEmpty(CashierName) && CashierName != "Self-Service")
                        {
                            sb.AppendLine("CASHIER:");
                            sb.AppendLine($"{CashierName}");
                            sb.AppendLine("");
                        }

                        sb.AppendLine("CUSTOMER:");
                        sb.AppendLine($"{custName}");
                        sb.AppendLine("--------------------------------");
                        foreach(var item in _currentOrderDetails.DetailedItems) sb.AppendLine($"{item.Quantity}x {item.Name,-20} {item.Total,10:N2}");
                        sb.AppendLine("--------------------------------");
                        sb.AppendLine($"TOTAL   :           {_currentTotal,10}");
                        
                        // Add instructions to printed text file as well
                        if (CashierName == "Self-Service" || string.IsNullOrEmpty(CashierName))
                        {
                            sb.AppendLine("");
                            sb.AppendLine("*** PLEASE HAND THIS RECEIPT ***");
                            sb.AppendLine("*** TO THE CASHIER FOR PAYMENT ***");
                        }
                        
                        sb.AppendLine("--------------------------------");
                        File.WriteAllText(sfd.FileName, sb.ToString());
                        MessageBox.Show("Receipt saved successfully!");
                    }
                    catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
                }
            }
        }
    }
}