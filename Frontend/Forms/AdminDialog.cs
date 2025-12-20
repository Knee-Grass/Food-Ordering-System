using System;
using System.Drawing;
using System.Windows.Forms;

namespace FoodOrderingSystem.Forms
{
    public class AdminDialog : Form
    {
        public string Username { get; private set; } = string.Empty; // Init default
        public string Password { get; private set; } = string.Empty; // Init default

        // Init default to avoid CS8618
        private TextBox _txtUser = new TextBox();
        private TextBox _txtPass = new TextBox();

        public AdminDialog()
        {
            this.Text = "Add New Admin";
            this.Size = new Size(350, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            InitializeUI();
        }

        private void InitializeUI()
        {
            Label lblUser = new Label { Text = "Username:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _txtUser = new TextBox { Location = new Point(20, 45), Width = 280, Font = new Font("Segoe UI", 10) };
            
            Label lblPass = new Label { Text = "Password:", Location = new Point(20, 85), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _txtPass = new TextBox { Location = new Point(20, 110), Width = 280, Font = new Font("Segoe UI", 10), UseSystemPasswordChar = true };

            Button btnSave = new Button 
            { 
                Text = "Create Admin", 
                BackColor = Color.FromArgb(255, 87, 34), 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, 
                Location = new Point(20, 170), 
                Size = new Size(280, 40),
                Cursor = Cursors.Hand
            };
            btnSave.Click += (s, e) => {
                if(ValidateInput()) 
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };

            this.Controls.Add(lblUser);
            this.Controls.Add(_txtUser);
            this.Controls.Add(lblPass);
            this.Controls.Add(_txtPass);
            this.Controls.Add(btnSave);
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(_txtUser.Text)) { MessageBox.Show("Username is required."); return false; }
            if (string.IsNullOrWhiteSpace(_txtPass.Text)) { MessageBox.Show("Password is required."); return false; }

            Username = _txtUser.Text;
            Password = _txtPass.Text;
            return true;
        }
    }
}