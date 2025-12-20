using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using FoodOrderingSystem.Data; // Ensure this matches DatabaseService namespace
using FoodOrderingSystem.Models; // Ensure this matches Models namespace

namespace FoodOrderingSystem.Forms // Adjusted namespace to match project structure
{
    public class HistoryForm : Form
    {
        private readonly DatabaseService _dbService;
        private FlowLayoutPanel _previewPanel = new FlowLayoutPanel(); // Initialize to avoid CS8618
        
        public HistoryForm()
        {
            _dbService = new DatabaseService();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Receipt Preview";
            this.Size = new Size(400, 600);
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

            this.Controls.Add(_previewPanel);
        }

        public async Task GenerateReceiptPreviewAsync(int orderId, string? date, string? total) // Nullable parameters
        {
            _previewPanel.Controls.Clear();

            if (total == null) total = "0.00";
            if (date == null) date = DateTime.Now.ToString();

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
                Text = $"Order #{orderId}\n{date}",
                Font = new Font("Courier New", 10),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            _previewPanel.Controls.Add(lblDetails);

            var order = await _dbService.GetOrderDetailsAsync(orderId);

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
                Text = $"--------------------------------\nTOTAL: {total}",
                Font = new Font("Courier New", 12, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 20, 0, 0)
            };
            _previewPanel.Controls.Add(lblTotal);
        }
    }
}