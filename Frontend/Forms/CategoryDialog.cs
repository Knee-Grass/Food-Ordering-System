using System;
using System.Drawing;
using System.Windows.Forms;

namespace FoodOrderingSystem.Forms
{
    public class CategoryDialog : Form
    {
        public string CategoryName { get; private set; } = string.Empty;
        private TextBox _txtName = new TextBox();

        public CategoryDialog(string existingName = "")
        {
            Text = string.IsNullOrEmpty(existingName) ? "Add Category" : "Edit Category";
            Size = new Size(350, 200);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            InitializeUI(existingName);
        }

        private void InitializeUI(string existingName)
        {
            Label l1 = new Label { Text = "Category Name:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _txtName = new TextBox { Location = new Point(20, 45), Width = 280, Font = new Font("Segoe UI", 10) };
            _txtName.Text = existingName;

            Button btnSave = new Button 
            { 
                Text = "Save", Location = new Point(20, 100), Size = new Size(130, 40), 
                BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => {
                if(string.IsNullOrWhiteSpace(_txtName.Text)) { MessageBox.Show("Name is required."); return; }
                CategoryName = _txtName.Text;
                DialogResult = DialogResult.OK;
                Close();
            };

            Button btnCancel = new Button 
            { 
                Text = "Cancel", Location = new Point(170, 100), Size = new Size(130, 40), 
                BackColor = Color.Gray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => Close();

            Controls.AddRange(new Control[] { l1, _txtName, btnSave, btnCancel });
        }
    }
}