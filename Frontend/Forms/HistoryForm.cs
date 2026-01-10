using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using FoodOrderingSystem.Data; 
using FoodOrderingSystem.Models; 
using System.Linq; 
using System.Text; 
using System.IO; 
using System.ComponentModel; // Added for Designer attributes

namespace FoodOrderingSystem.Forms 
{
    public class HistoryForm : Form
    {
        private readonly DatabaseService _dbService;
        private FlowLayoutPanel _previewPanel = null!; 
        
        // Fix: Attributes added to prevent Designer serialization errors
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
            this.Size = new Size(450, 700);
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
                Text = "🖨 Print", 
                Font = new Font("Segoe UI", 12, FontStyle.Bold), 
                Dock = DockStyle.Right,
                Width = 100, 
                BackColor = Color.Teal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            ToolTip tip = new ToolTip();
            tip.SetToolTip(btnDownload, "Print/Download Current Receipt");
            btnDownload.Click += (s, e) => DownloadReceipt();

            bottomPanel.Controls.Add(btnDownload);
            
            // Add bottomPanel FIRST so it claims the bottom space before _previewPanel fills the rest
            this.Controls.Add(bottomPanel);
            this.Controls.Add(_previewPanel);
        }

        // Store current order details for downloading
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

            await GenerateReceiptPreviewAsync(order.Id, _currentDate, _currentTotal);
        }

        public async Task GenerateReceiptPreviewAsync(int orderId, string? date, string? total) 
        {
            _currentOrderId = orderId;
            _currentDate = date ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _currentTotal = total ?? "0.00";

            _previewPanel.Controls.Clear();

            // HEADER
            Label lblHeader = new Label
            {
                Text = "OFFICIAL RECEIPT",
                Font = new Font("Courier New", 14, FontStyle.Bold),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 10)
            };
            _previewPanel.Controls.Add(lblHeader);

            // BIG BOLD BARCODE (ORDER ID) - Increased size as requested
            Label lblBarcode = new Label
            {
                Text = $"#{orderId}",
                Font = new Font("Courier New", 36, FontStyle.Bold), 
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 10),
                ForeColor = Color.Black
            };
             // Center the barcode label in panel
            Panel barcodeContainer = new Panel { Width = 380, Height = 60 };
            lblBarcode.Dock = DockStyle.Fill;
            barcodeContainer.Controls.Add(lblBarcode);
            _previewPanel.Controls.Add(barcodeContainer);

            // DATE
            Label lblDate = new Label
            {
                Text = $"{_currentDate}",
                Font = new Font("Courier New", 9),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 15)
            };
            _previewPanel.Controls.Add(lblDate);

            // CUSTOMER & CASHIER INFO
            string custName = _currentOrderDetails?.CustomerName ?? "Guest";
            // Clean up Customer Name if it has code
            if(custName.Contains("[Code:")) custName = custName.Split('[')[0].Trim();

            Label lblPeople = new Label
            {
                Text = $"CASHIER : {CashierName}\nCUSTOMER: {custName}",
                Font = new Font("Courier New", 10, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20),
                ForeColor = Color.DarkSlateGray
            };
            _previewPanel.Controls.Add(lblPeople);

            Label lblSep1 = new Label { Text = "--------------------------------", Font = new Font("Courier New", 12), AutoSize = true };
            _previewPanel.Controls.Add(lblSep1);

            var order = await _dbService.GetOrderDetailsAsync(orderId);
            _currentOrderDetails = order; 

            if (order != null && order.DetailedItems != null)
            {
                foreach (var item in order.DetailedItems)
                {
                    Panel itemPanel = new Panel { Width = 360, Height = 25, Margin = new Padding(0) };
                    
                    Label name = new Label { Text = $"{item.Quantity}x {item.Name}", AutoSize = true, Location = new Point(0, 0), Font = new Font("Courier New", 9) };
                    Label price = new Label { Text = $"{item.Total:N2}", AutoSize = true, Location = new Point(300, 0), Font = new Font("Courier New", 9) };

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
                Text = $"--------------------------------\nTOTAL:     {_currentTotal}",
                Font = new Font("Courier New", 14, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0)
            };
            _previewPanel.Controls.Add(lblTotal);
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
                        sb.AppendLine("          OFFICIAL RECEIPT");
                        sb.AppendLine("--------------------------------");
                        sb.AppendLine($"ORDER #: {_currentOrderId}"); 
                        sb.AppendLine("--------------------------------");
                        sb.AppendLine($"Date    : {_currentDate}");
                        sb.AppendLine($"CASHIER : {CashierName}");
                        
                        string custName = _currentOrderDetails.CustomerName;
                        if(custName.Contains("[Code:")) custName = custName.Split('[')[0].Trim();
                        sb.AppendLine($"CUSTOMER: {custName}");
                        
                        sb.AppendLine("--------------------------------");
                        foreach(var item in _currentOrderDetails.DetailedItems)
                        {
                            string line = $"{item.Quantity}x {item.Name}";
                            if(line.Length > 20) line = line.Substring(0, 20);
                            sb.AppendLine($"{line,-20} {item.Total,10:N2}");
                        }
                        sb.AppendLine("--------------------------------");
                        sb.AppendLine($"TOTAL   :           {_currentTotal,10}");
                        sb.AppendLine("--------------------------------");
                        sb.AppendLine("      Thank you for dining!");

                        File.WriteAllText(sfd.FileName, sb.ToString());
                        MessageBox.Show("Receipt saved successfully!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving receipt: {ex.Message}");
                    }
                }
            }
        }
    }
}