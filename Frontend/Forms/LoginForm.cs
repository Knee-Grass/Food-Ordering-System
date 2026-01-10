using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO; // Added for File check
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
            btnClose.Click += (s, e) => Application.Exit();
            this.Controls.Add(btnClose);

            // Title Container (Main Wrapper: Logo + TextBlock)
            FlowLayoutPanel pnlLogo = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight, 
                AutoSize = true,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            // 1. Image Logo (Increased Size)
            PictureBox pbLogo = new PictureBox
            {
                Size = new Size(130, 130), // Increased from 100x100
                SizeMode = PictureBoxSizeMode.Zoom,
                Margin = new Padding(0), 
                BackColor = Color.Transparent
            };

            // Try to load image safely handling extensions
            try
            {
                string basePath = "img/boset";
                if (File.Exists(basePath + ".png")) pbLogo.Image = Image.FromFile(basePath + ".png");
                else if (File.Exists(basePath + ".jpg")) pbLogo.Image = Image.FromFile(basePath + ".jpg");
                else if (File.Exists(basePath + ".jpeg")) pbLogo.Image = Image.FromFile(basePath + ".jpeg");
                else if (File.Exists(basePath)) pbLogo.Image = Image.FromFile(basePath);
            }
            catch 
            { 
                // Fallback handled silently
            }

            // 2. Text Block (Vertical Stack: Food \n Hub)
            FlowLayoutPanel pnlText = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 10, 0, 0) // Adjusted top margin to center vertically against bigger logo
            };

            Label lblFood = new Label
            {
                Text = "Food",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 32, FontStyle.Bold), // Increased from 24 to 32
                AutoSize = true,
                Margin = new Padding(0),
                Padding = new Padding(0),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label lblHub = new Label
            {
                Text = "Hub",
                ForeColor = Color.Orange, 
                Font = new Font("Segoe UI", 32, FontStyle.Bold), // Increased from 24 to 32
                AutoSize = true,
                Margin = new Padding(0, -15, 0, 0), // Adjusted negative margin for larger font
                Padding = new Padding(0),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Add labels to vertical text panel
            pnlText.Controls.Add(lblFood);
            pnlText.Controls.Add(lblHub);

            // Add logo and text panel to main horizontal panel
            pnlLogo.Controls.Add(pbLogo);
            pnlLogo.Controls.Add(pnlText);
            
            // Adjust location to center the new larger layout
            // Approx width: 130(img) + ~120(text) = ~250
            // (400 - 250) / 2 = 75
            pnlLogo.Location = new Point(75, 30); 
            this.Controls.Add(pnlLogo);

            // Username 
            _txtUser = CreateInput("Username", 230);
            
            // Password 
            _txtPass = CreateInput("Password", 300, true);

            // Login Button 
            Button btnLogin = new Button
            {
                Text = "LOGIN",
                BackColor = Color.FromArgb(255, 128, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(300, 50),
                Location = new Point(50, 390),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);

            // Register Label 
            Label lblRegister = new Label
            {
                Text = "Don't have an account? Register now",
                ForeColor = Color.DeepSkyBlue,
                Font = new Font("Segoe UI", 10, FontStyle.Underline),
                AutoSize = true,
                Location = new Point(85, 450),
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