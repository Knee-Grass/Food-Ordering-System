using System;
using System.Drawing;
using System.Windows.Forms;
using FoodOrderingSystem.Models;

namespace FoodOrderingSystem.Forms
{
    public class CrewDialog : Form
    {
        public string Username { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;

        private TextBox _txtUser;
        private TextBox _txtPass;
        private User? _existingUser;

        public CrewDialog(User? user = null)
        {
            _existingUser = user;
            this.Text = user == null ? "Add New Crew Member" : "Edit Crew Member";
            this.Size = new Size(350, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            _txtUser = new TextBox();
            _txtPass = new TextBox();

            InitializeUI();
        }

        private void InitializeUI()
        {
            Label lblUser = new Label { Text = "Username:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _txtUser.Location = new Point(20, 45);
            _txtUser.Width = 280;
            _txtUser.Font = new Font("Segoe UI", 10);
            if (_existingUser != null) _txtUser.Text = _existingUser.Username;
            
            Label lblPass = new Label { Text = "Password:", Location = new Point(20, 85), AutoSize = true, Font = new Font("Segoe UI", 10) };
            _txtPass.Location = new Point(20, 110);
            _txtPass.Width = 280;
            _txtPass.Font = new Font("Segoe UI", 10);
            _txtPass.UseSystemPasswordChar = true;
            
            if (_existingUser != null)
            {
                Label lblNote = new Label { Text = "(Leave blank to keep existing password)", ForeColor = Color.Gray, Font = new Font("Segoe UI", 8), Location = new Point(20, 140), AutoSize = true };
                this.Controls.Add(lblNote);
            }

            Button btnSave = new Button 
            { 
                Text = _existingUser == null ? "Create Crew" : "Update Crew", 
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
            // If adding new, pass is required. If editing, optional.
            if (_existingUser == null && string.IsNullOrWhiteSpace(_txtPass.Text)) { MessageBox.Show("Password is required."); return false; }

            Username = _txtUser.Text;
            Password = _txtPass.Text;
            return true;
        }
    }
}