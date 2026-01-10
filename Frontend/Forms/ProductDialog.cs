using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using FoodOrderingSystem.Models; 
using System.IO;

namespace FoodOrderingSystem.Forms 
{
    public class ProductDialog : Form
    {
        public string PName { get; private set; } = string.Empty;
        public decimal PPrice { get; private set; } = 0;
        public int PCategoryId { get; private set; } = 0; 
        public int PQuantity { get; private set; } = 0;
        public string PImageData { get; private set; } = string.Empty; 
        
        private TextBox _txtName;
        private TextBox _txtPrice;
        private ComboBox _cbCat;
        private NumericUpDown _numQty;
        private PictureBox _pbImage; 
        private Button _btnUpload; 
        private Button _btnRemoveImage; 
        private Label _lblPath;

        private bool _initFailed = false;

        // Constructor accepting Category List - fixes Mainform error
        public ProductDialog(List<Category> categories, FoodItem? item = null)
        {
            _txtName = new TextBox();
            _txtPrice = new TextBox();
            _cbCat = new ComboBox();
            _numQty = new NumericUpDown();
            _pbImage = new PictureBox(); 
            _btnUpload = new Button(); 
            _btnRemoveImage = new Button();
            _lblPath = new Label(); 

            try
            {
                SetupFormProperties(item);
                InitializeControls();
                LoadData(categories, item);
            }
            catch (Exception ex)
            {
                _initFailed = true;
                MessageBox.Show($"Error initializing dialog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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
            Size = new Size(550, 480); 
            StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void InitializeControls()
        {
            Label l1 = new Label { Text = "Name:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _txtName.Location = new Point(20, 45);
            _txtName.Width = 250;
            _txtName.Font = new Font("Segoe UI", 10);

            Label l2 = new Label { Text = "Price:", Location = new Point(20, 85), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _txtPrice.Location = new Point(20, 110);
            _txtPrice.Width = 250;
            _txtPrice.Font = new Font("Segoe UI", 10);

            Label l3 = new Label { Text = "Category:", Location = new Point(20, 150), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _cbCat.Location = new Point(20, 175);
            _cbCat.Width = 250;
            _cbCat.Font = new Font("Segoe UI", 10);
            _cbCat.DropDownStyle = ComboBoxStyle.DropDownList;
            
            Label l4 = new Label { Text = "Stock Quantity:", Location = new Point(20, 215), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _numQty.BeginInit(); 
            _numQty.Location = new Point(20, 240);
            _numQty.Width = 250;
            _numQty.Font = new Font("Segoe UI", 10);
            _numQty.Minimum = 0;
            _numQty.Maximum = 10000;
            _numQty.EndInit();

            Label lImage = new Label { Text = "Product Image:", Location = new Point(300, 20), AutoSize = true, Font = new Font("Segoe UI", 10) };
            
            _pbImage.Location = new Point(300, 45);
            _pbImage.Size = new Size(200, 200);
            _pbImage.BorderStyle = BorderStyle.FixedSingle;
            _pbImage.SizeMode = PictureBoxSizeMode.Zoom;
            _pbImage.BackColor = Color.WhiteSmoke;

            _btnUpload.Text = "Choose Image";
            _btnUpload.Location = new Point(300, 255);
            _btnUpload.Size = new Size(95, 35);
            _btnUpload.BackColor = Color.Teal;
            _btnUpload.ForeColor = Color.White;
            _btnUpload.FlatStyle = FlatStyle.Flat;
            _btnUpload.Click += BtnUpload_Click;

            _btnRemoveImage.Text = "Remove";
            _btnRemoveImage.Location = new Point(405, 255);
            _btnRemoveImage.Size = new Size(95, 35);
            _btnRemoveImage.BackColor = Color.IndianRed;
            _btnRemoveImage.ForeColor = Color.White;
            _btnRemoveImage.FlatStyle = FlatStyle.Flat;
            _btnRemoveImage.Click += BtnRemoveImage_Click;

            _lblPath.Location = new Point(300, 295);
            _lblPath.Size = new Size(200, 20);
            _lblPath.Font = new Font("Segoe UI", 8);
            _lblPath.ForeColor = Color.Gray;
            _lblPath.AutoEllipsis = true;
            _lblPath.Text = "No image selected";

            Button btnSave = new Button 
            { 
                Text = "Save", Location = new Point(20, 360), Size = new Size(130, 40), 
                BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new Button 
            { 
                Text = "Cancel", Location = new Point(170, 360), Size = new Size(130, 40), 
                BackColor = Color.Gray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.AddRange(new Control[] { 
                l1, _txtName, l2, _txtPrice, l3, _cbCat, l4, _numQty, 
                lImage, _pbImage, _btnUpload, _btnRemoveImage, _lblPath,
                btnSave, btnCancel 
            });
        }

        private void LoadData(List<Category> categories, FoodItem? item)
        {
            _txtName.Text = item?.Name ?? string.Empty;
            _txtPrice.Text = item?.Price.ToString() ?? string.Empty;

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

            decimal qty = item?.Quantity ?? 50;
            if (qty < _numQty.Minimum) qty = _numQty.Minimum;
            if (qty > _numQty.Maximum) qty = _numQty.Maximum;

            try { _numQty.Value = qty; }
            catch { _numQty.Value = _numQty.Minimum; }

            PImageData = item?.ImageData ?? string.Empty;
            UpdateImagePreview();
        }

        private void UpdateImagePreview()
        {
            if (!string.IsNullOrEmpty(PImageData))
            {
                try 
                { 
                    if (_pbImage.Image != null) _pbImage.Image.Dispose();
                    byte[] imageBytes = Convert.FromBase64String(PImageData);
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        _pbImage.Image = Image.FromStream(ms);
                    }
                    _lblPath.Text = "Image Loaded";
                }
                catch 
                { 
                    _pbImage.Image = null;
                    _lblPath.Text = "Image load error"; 
                }
            }
            else
            {
                _pbImage.Image = null;
                _lblPath.Text = "No image selected";
            }
        }

        private void BtnUpload_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        byte[] imageBytes = File.ReadAllBytes(ofd.FileName);
                        PImageData = Convert.ToBase64String(imageBytes);
                        UpdateImagePreview();
                    }
                    catch
                    {
                        MessageBox.Show("Error reading image file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnRemoveImage_Click(object? sender, EventArgs e)
        {
            PImageData = string.Empty;
            UpdateImagePreview();
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(_txtName.Text)) { MessageBox.Show("Name is required."); return; }
            if(!decimal.TryParse(_txtPrice.Text, out decimal p) || p <= 0) { MessageBox.Show("Invalid Price."); return; }
            if(_cbCat.Items.Count == 0 || _cbCat.SelectedItem == null) { MessageBox.Show("Select a category."); return; }

            PName = _txtName.Text; 
            PPrice = p; 

            if (_cbCat.SelectedValue is int id) PCategoryId = id;
            else if (_cbCat.SelectedValue != null && int.TryParse(_cbCat.SelectedValue.ToString(), out int parsedId)) PCategoryId = parsedId;
            else PCategoryId = 0;

            PQuantity = (int)_numQty.Value; 
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}