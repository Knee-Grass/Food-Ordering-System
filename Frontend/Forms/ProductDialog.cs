using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using FoodOrderingSystem.Models; 

namespace FoodOrderingSystem.Forms 
{
    public class ProductDialog : Form
    {
        public string PName { get; private set; } = string.Empty;
        public decimal PPrice { get; private set; } = 0;
        public int PCategoryId { get; private set; } = 0; 
        public int PQuantity { get; private set; } = 0;
        
        // Define controls
        private TextBox _txtName;
        private TextBox _txtPrice;
        private ComboBox _cbCat;
        private NumericUpDown _numQty;

        // Flag to track if we should close safely after loading
        private bool _initFailed = false;

        public ProductDialog(List<Category> categories, FoodItem? item = null)
        {
            // Initialize components immediately in constructor
            _txtName = new TextBox();
            _txtPrice = new TextBox();
            _cbCat = new ComboBox();
            _numQty = new NumericUpDown();

            try
            {
                SetupFormProperties(item);
                InitializeControls();
                LoadData(categories, item);
            }
            catch (Exception ex)
            {
                // CRITICAL: Do NOT call Close() here. It causes "Cannot access a disposed object".
                // Instead, mark initialization as failed and let OnLoad handle the closing.
                _initFailed = true;
                MessageBox.Show($"Error initializing dialog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Safely close the form AFTER it has been created if initialization failed
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (_initFailed)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void SetupFormProperties(FoodItem? item)
        {
            Text = item == null ? "Add Product" : "Edit Product"; 
            Size = new Size(350, 420); 
            StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void InitializeControls()
        {
            // Name
            Label l1 = new Label { Text = "Name:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _txtName.Location = new Point(20, 45);
            _txtName.Width = 280;
            _txtName.Font = new Font("Segoe UI", 10);

            // Price
            Label l2 = new Label { Text = "Price:", Location = new Point(20, 85), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _txtPrice.Location = new Point(20, 110);
            _txtPrice.Width = 280;
            _txtPrice.Font = new Font("Segoe UI", 10);

            // Category
            Label l3 = new Label { Text = "Category:", Location = new Point(20, 150), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _cbCat.Location = new Point(20, 175);
            _cbCat.Width = 280;
            _cbCat.Font = new Font("Segoe UI", 10);
            _cbCat.DropDownStyle = ComboBoxStyle.DropDownList;
            
            // Quantity
            Label l4 = new Label { Text = "Stock Quantity:", Location = new Point(20, 215), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _numQty.BeginInit(); // Suspend layout logic for safer initialization
            _numQty.Location = new Point(20, 240);
            _numQty.Width = 280;
            _numQty.Font = new Font("Segoe UI", 10);
            _numQty.Minimum = 0;
            _numQty.Maximum = 10000;
            _numQty.EndInit();

            // Buttons
            Button btnSave = new Button 
            { 
                Text = "Save", Location = new Point(20, 300), Size = new Size(130, 40), 
                BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new Button 
            { 
                Text = "Cancel", Location = new Point(170, 300), Size = new Size(130, 40), 
                BackColor = Color.Gray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.AddRange(new Control[] { l1, _txtName, l2, _txtPrice, l3, _cbCat, l4, _numQty, btnSave, btnCancel });
        }

        private void LoadData(List<Category> categories, FoodItem? item)
        {
            _txtName.Text = item?.Name ?? string.Empty;
            _txtPrice.Text = item?.Price.ToString() ?? string.Empty;

            // Categories
            if (categories != null && categories.Count > 0)
            {
                _cbCat.DisplayMember = "Name";
                _cbCat.ValueMember = "Id";
                _cbCat.DataSource = categories;

                if (item != null && item.CategoryId > 0)
                    _cbCat.SelectedValue = item.CategoryId;
                else
                    _cbCat.SelectedIndex = 0;
            }

            // Quantity - Safely calculate and assign
            decimal qty = item?.Quantity ?? 50;
            if (qty < _numQty.Minimum) qty = _numQty.Minimum;
            if (qty > _numQty.Maximum) qty = _numQty.Maximum;

            try 
            { 
                _numQty.Value = qty; 
            }
            catch 
            { 
                // Fallback to minimum if something goes wrong (prevents crash)
                _numQty.Value = _numQty.Minimum; 
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(_txtName.Text)) { MessageBox.Show("Name is required."); return; }
            if(!decimal.TryParse(_txtPrice.Text, out decimal p) || p <= 0) { MessageBox.Show("Invalid Price."); return; }
            if(_cbCat.Items.Count == 0 || _cbCat.SelectedItem == null) { MessageBox.Show("Select a category."); return; }

            PName = _txtName.Text; 
            PPrice = p; 

            // Robust Category ID Extraction
            if (_cbCat.SelectedValue is int id) 
            {
                PCategoryId = id;
            }
            else if (_cbCat.SelectedValue != null && int.TryParse(_cbCat.SelectedValue.ToString(), out int parsedId)) 
            {
                PCategoryId = parsedId;
            }
            else 
            {
                PCategoryId = 0;
            }

            PQuantity = (int)_numQty.Value; 
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}