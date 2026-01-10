using System;
using System.Drawing;
using System.Windows.Forms;
using FoodOrderingSystem.Data;

namespace FoodOrderingSystem.Forms
{
    public class RegisterForm : Form
    {
        private TextBox _txtUser = new TextBox();
        private TextBox _txtPass = new TextBox();
        private TextBox _txtConfirmPass = new TextBox();
        private DatabaseService _dbService;

        public RegisterForm()
        {
            _dbService = new DatabaseService();
            
            this.Text = "Register Customer - Gourmet System";
            this.Size = new Size(400, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(40, 40, 40); // Dark theme
            this.FormBorderStyle = FormBorderStyle.None;

            InitializeUI();
        }

        private void InitializeUI()
        {
            // Close Button
            Label btnClose = new Label
            {
                Text = "X",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(360, 10),
                Cursor = Cursors.Hand,
                AutoSize = true
            };
            btnClose.Click += (s, e) => {
                new LoginForm().Show();
                this.Hide();
            };
            this.Controls.Add(btnClose);

            // Title
            Label lblTitle = new Label
            {
                Text = "CUSTOMER\nREGISTRATION",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(400, 100),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 40)
            };
            this.Controls.Add(lblTitle);

            // Inputs
            _txtUser = CreateInput("Username", 150);
            _txtPass = CreateInput("Password", 220, true);
            _txtConfirmPass = CreateInput("Confirm Password", 290, true);

            // Register Button
            Button btnRegister = new Button
            {
                Text = "REGISTER",
                BackColor = Color.FromArgb(76, 175, 80), // Green
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(300, 50),
                Location = new Point(50, 380),
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;
            this.Controls.Add(btnRegister);

            // Back to Login Link
            Label lblLogin = new Label
            {
                Text = "Already have an account? Login",
                ForeColor = Color.DeepSkyBlue,
                Font = new Font("Segoe UI", 10, FontStyle.Underline),
                AutoSize = true,
                Location = new Point(100, 450),
                Cursor = Cursors.Hand
            };
            lblLogin.Click += (s, e) => {
                new LoginForm().Show();
                this.Hide();
            };
            this.Controls.Add(lblLogin);
        }

        private TextBox CreateInput(string placeholder, int y, bool isPassword = false)
        {
            Label lbl = new Label
            {
                Text = placeholder,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 10),
                Location = new Point(50, y),
                AutoSize = true
            };
            this.Controls.Add(lbl);

            TextBox txt = new TextBox
            {
                Font = new Font("Segoe UI", 12),
                Size = new Size(300, 35),
                Location = new Point(50, y + 25),
                UseSystemPasswordChar = isPassword
            };
            this.Controls.Add(txt);
            return txt;
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtUser.Text) || string.IsNullOrWhiteSpace(_txtPass.Text))
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_txtPass.Text != _txtConfirmPass.Text)
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Calls the Database Service to insert the user
            // FIXED: Explicitly passing "Customer" role
            bool success = _dbService.RegisterUser(_txtUser.Text, _txtPass.Text, "Customer");
            
            if (success)
            {
                MessageBox.Show("Customer Registration Successful! You can now login.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                new LoginForm().Show();
                this.Close(); // Close register form properly
            }
            else
            {
                MessageBox.Show("Username already exists. Please choose another.", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}