using System;
using System.Drawing;
using System.Windows.Forms;
using FoodOrderingSystem.Models;

namespace FoodOrderingSystem.Forms
{
    // Generic Dialog for Adding/Editing Staff (Cashiers & Crew)
    public class StaffDialog : Form
    {
        public string Username { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;

        private TextBox _txtUser = new TextBox();
        private TextBox _txtPass = new TextBox();
        private User? _existingUser;

        public StaffDialog(string title, User? user = null)
        {
            _existingUser = user;
            this.Text = title;
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

            if (_existingUser != null)
            {
                _txtUser.Text = _existingUser.Username;
                Label lblNote = new Label { Text = "(Leave blank to keep existing password)", ForeColor = Color.Gray, Font = new Font("Segoe UI", 8), Location = new Point(20, 140), AutoSize = true };
                this.Controls.Add(lblNote);
            }

            Button btnSave = new Button 
            { 
                Text = _existingUser == null ? "Create Account" : "Update Account", 
                BackColor = Color.FromArgb(76, 175, 80), // Green 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, 
                Location = new Point(20, 170), 
                Size = new Size(280, 40),
                Cursor = Cursors.Hand
            };
            btnSave.Click += (s, e) => {
                if(ValidateInput()) 
                {
                    Username = _txtUser.Text;
                    Password = _txtPass.Text;
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
            if (_existingUser == null && string.IsNullOrWhiteSpace(_txtPass.Text)) { MessageBox.Show("Password is required."); return false; }
            return true;
        }
    }
}