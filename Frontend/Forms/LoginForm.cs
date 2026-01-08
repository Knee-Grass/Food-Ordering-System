using System;
using System.Drawing;
using System.Windows.Forms;
using FoodOrderingSystem.Data;

namespace FoodOrderingSystem.Forms
{
    public class LoginForm : Form
    {
        private TextBox _txtUser = new TextBox();
        private TextBox _txtPass = new TextBox();
        private DatabaseService _dbService;

        public LoginForm()
        {
            _dbService = new DatabaseService();
            
            this.Text = "Login - Gourmet System";
            this.Size = new Size(400, 500);
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
            btnClose.Click += (s, e) => Application.Exit();
            this.Controls.Add(btnClose);

            // Title
            Label lblTitle = new Label
            {
                Text = "KNEEGRASS\nPOS",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(400, 100),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 50)
            };
            this.Controls.Add(lblTitle);

            // Username
            _txtUser = CreateInput("Username", 180);
            _txtPass = CreateInput("Password", 250, true);

            // Login Button
            Button btnLogin = new Button
            {
                Text = "LOGIN",
                BackColor = Color.FromArgb(255, 128, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(300, 50),
                Location = new Point(50, 340),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);

            // Register Label (Updated Text)
            Label lblRegister = new Label
            {
                Text = "Don't have an account? Register now", // Updated as requested
                ForeColor = Color.DeepSkyBlue,
                Font = new Font("Segoe UI", 10, FontStyle.Underline),
                AutoSize = true,
                Location = new Point(85, 400),
                Cursor = Cursors.Hand
            };
            lblRegister.Click += (s, e) => {
                RegisterForm registerForm = new RegisterForm(); 
                registerForm.Show();
                this.Hide();
            };
            this.Controls.Add(lblRegister);
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

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            string? role = _dbService.AuthenticateUser(_txtUser.Text, _txtPass.Text); 

            if (role != null)
            {
                // Pass role and username to the Main Form
                MainForm mainForm = new MainForm(role, _txtUser.Text);
                mainForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid Username or Password", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}