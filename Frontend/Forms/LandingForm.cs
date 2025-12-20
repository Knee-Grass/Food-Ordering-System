using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FoodOrderingSystem.Controls; // For ModernButton
using FoodOrderingSystem.Backend.Config; // Correct namespace for DbConfig

namespace FoodOrderingSystem.Forms // Namespace matches other forms
{
    public class LandingForm : Form
    {
        // Store session info
        private string _userRole;
        private string _username;

        // Default constructor for designer support
        public LandingForm() : this("User", "Guest") { }

        public LandingForm(string role, string username)
        {
            _userRole = role;
            _username = username;

            this.Text = $"Welcome, {username} - Gourmet Food System";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ResizeRedraw = true;
            
            // Handle closing: if user closes landing, exit app completely
            this.FormClosed += (s, e) => Application.Exit();
            
            InitializeUI();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle, 
                Color.FromArgb(255, 255, 255), Color.FromArgb(255, 224, 178), 45F))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        private void InitializeUI()
        {
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                BackColor = Color.Transparent
            };
            
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));

            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            Label lblTitle = new Label
            {
                Text = "GOURMET\nFOOD SYSTEM",
                Font = new Font("Segoe UI", 40F, FontStyle.Bold),
                ForeColor = DbConfig.PrimaryColor,
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 160,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Check if the user is an Admin or SuperAdmin
            bool isAdmin = _userRole == "Admin" || _userRole == "SuperAdmin";

            Label lblSubtitle = new Label
            {
                Text = isAdmin ? "Administrator Dashboard" : "Fresh. Fast. Delicious.",
                Font = new Font("Segoe UI Semilight", 18F),
                ForeColor = DbConfig.DarkColor,
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.TopCenter
            };

            Panel buttonPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(50, 20, 50, 0) };
            
            // CHANGE: Button text adapts to role (Admin or SuperAdmin)
            string mainBtnText = isAdmin ? "Manage Orders" : "Start Order";
            ModernButton btnStart = new ModernButton { Text = mainBtnText, BackColor = DbConfig.PrimaryColor, Dock = DockStyle.Top, Height = 55 };
            
            btnStart.Click += (s, e) => { 
                this.Hide(); 
                // CHANGE: Pass credentials to MainForm
                new MainForm(_userRole, _username).ShowDialog(); 
                this.Show(); 
            };

            ModernButton btnHistory = new ModernButton { Text = "Receipt Viewer", BackColor = DbConfig.DarkColor, Dock = DockStyle.Top, Height = 55 };
            btnHistory.Click += (s, e) => { this.Hide(); new HistoryForm().ShowDialog(); this.Show(); };

            ModernButton btnLogout = new ModernButton { Text = "Logout", BackColor = Color.Gray, Dock = DockStyle.Top, Height = 55 };
            
            // LOGOUT LOGIC: Go back to Login Form
            btnLogout.Click += (s, e) => {
                this.Hide(); // Hide Landing
                new LoginForm().Show(); // Show Login
            };

            buttonPanel.Controls.Add(btnLogout);
            buttonPanel.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 15 });
            
            // Only show receipt viewer button for normal users, Admins have built-in history in MainForm
            if (!isAdmin)
            {
                buttonPanel.Controls.Add(btnHistory);
                buttonPanel.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 15 });
            }

            buttonPanel.Controls.Add(btnStart);

            contentPanel.Controls.Add(buttonPanel);
            contentPanel.Controls.Add(lblSubtitle);
            contentPanel.Controls.Add(lblTitle);

            mainLayout.Controls.Add(contentPanel, 1, 1);
            this.Controls.Add(mainLayout);
        }
    }
}