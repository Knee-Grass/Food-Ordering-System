using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FoodOrderingSystem.Controls;
using FoodOrderingSystem.Data;
using FoodOrderingSystem.Models;
using FoodOrderingSystem.Backend.Config;
using System.IO;
using System.Text;

namespace FoodOrderingSystem.Forms
{
    public class MainForm : Form
    {
        private readonly DatabaseService _dbService;
        private List<CartItem> _cartItems;
        private List<FoodItem> _allFoodItems; 
        private List<Category> _allCategories; 
        
        private string _userRole;
        private string _username;

        // UI Controls 
        private FlowLayoutPanel _menuContainer = null!;
        private FlowLayoutPanel _cartContainer = null!;
        private FlowLayoutPanel _historyContainer = null!;
        private FlowLayoutPanel _usersContainer = null!;
        private FlowLayoutPanel _productsContainer = null!;
        private FlowLayoutPanel _adminsContainer = null!;
        private FlowLayoutPanel _categoriesContainer = null!; 
        private Label _lblTotal = null!;
        private Panel _sidebarCategories = null!;
        private Panel _rightCartPanel = null!; 
        private Button? _currentCategoryBtn;
        private Label _lblPageTitle = null!; 
        private ComboBox? _cbMonthFilter; 
        
        private Button? _btnOrders; 
        private Button? _btnUsers;
        private Button? _btnProducts;
        private Button? _btnCategories; 
        private Button? _btnAdmins; 

        public MainForm() : this("User", "Guest")
        {
        }

        public MainForm(string role, string username)
        {
            _userRole = role;
            _username = username;

            _dbService = new DatabaseService();
            _cartItems = new List<CartItem>();
            _allFoodItems = new List<FoodItem>();
            _allCategories = new List<Category>();

            this.Text = $"Food Ordering System - {role} Mode ({username})";
            this.Size = new Size(1300, 850);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = DbConfig.LightColor;
            this.Font = DbConfig.MainFont;
            
            this.FormClosed += (s, e) => Application.Exit(); 

            InitializeLayout();
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await LoadDataAsync();

            if (_userRole == "Admin" || _userRole == "SuperAdmin")
            {
                ShowHistoryView();
            }
        }

        private async Task LoadDataAsync()
        {
            if (_menuContainer != null && _menuContainer.Controls.Count == 0)
            {
                Label loading = new Label { Text = "Loading Menu...", AutoSize = true, Font = new Font("Segoe UI", 16F), Padding = new Padding(20) };
                _menuContainer.Controls.Add(loading);
            }

            _allFoodItems = await _dbService.GetFoodItemsAsync();
            _allCategories = await _dbService.GetCategoriesAsync();

            PopulateSidebar();
            
            if (_userRole == "User")
            {
                FilterMenu("All");
            }
        }

        private void PopulateSidebar()
        {
            if (_sidebarCategories == null) return;
            _sidebarCategories.Controls.Clear();
            if (_userRole != "User") return;

            var catNames = _allCategories.Select(x => x.Name).OrderBy(x => x).ToList();
            catNames.Insert(0, "All"); 

            foreach (var cat in catNames)
            {
                Button btn = CreateCategoryButton(cat);
                if (cat == "All") SetActiveCategory(btn);
                _sidebarCategories.Controls.Add(btn);
            }
        }

        private void FilterMenu(string category)
        {
            HideAllViews();

            if (_userRole != "User") return;

            _menuContainer.Visible = true;
            _sidebarCategories.Visible = true;
            _rightCartPanel.Visible = true;
            _lblPageTitle.Text = $"Welcome, {_username}";

            _menuContainer.Controls.Clear();

            var itemsToShow = category == "All" ? _allFoodItems : _allFoodItems.Where(i => i.Category == category).ToList();

            if (itemsToShow.Count == 0)
            {
                Label empty = new Label { Text = "No items found in this category.", AutoSize = true, Font = new Font("Segoe UI", 14F), Padding = new Padding(20) };
                _menuContainer.Controls.Add(empty);
                return;
            }

            foreach (var item in itemsToShow) _menuContainer.Controls.Add(CreateFoodCard(item));
        }

        private void SetActiveCategory(Button btn)
        {
            if (_currentCategoryBtn != null)
            {
                _currentCategoryBtn.BackColor = DbConfig.DarkColor;
                _currentCategoryBtn.ForeColor = Color.White;
            }
            _currentCategoryBtn = btn;
            _currentCategoryBtn.BackColor = DbConfig.PrimaryColor; 
            _currentCategoryBtn.ForeColor = Color.White;
        }

        private void InitializeLayout()
        {
            Panel topPanel = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 80, 
                BackColor = DbConfig.DarkColor,
                Padding = new Padding(30, 0, 30, 0)
            };

            Label lblBrand = new Label 
            { 
                Text = _userRole == "SuperAdmin" ? "SUPER ADMIN" : (_userRole == "Admin" ? "ADMIN PORTAL" : "GOURMET"), 
                Font = new Font("Segoe UI", 22F, FontStyle.Bold), 
                ForeColor = Color.White, 
                AutoSize = true, 
                Dock = DockStyle.Left, 
                TextAlign = ContentAlignment.MiddleLeft
            };

            Panel pnlLogoutContainer = new Panel { Dock = DockStyle.Right, Width = 140, BackColor = Color.Transparent };
            ModernButton btnLogout = new ModernButton 
            { 
                Text = "Log Out", BackColor = Color.Crimson, ForeColor = Color.White,
                Size = new Size(120, 40), Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnLogout.Location = new Point(10, 20); 
            
            btnLogout.Click += (s, e) => {
                if (MessageBox.Show("Are you sure you want to log out?", "Logout Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Application.Restart(); 
                    Environment.Exit(0);
                }
            };
            pnlLogoutContainer.Controls.Add(btnLogout);

            Panel pnlNavCenter = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            FlowLayoutPanel navFlow = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.Transparent, WrapContents = false };

            if (_userRole == "Admin" || _userRole == "SuperAdmin")
            {
                _btnOrders = CreateNavButton("Orders");
                _btnOrders.Click += (s, e) => ShowHistoryView();
                
                _btnUsers = CreateNavButton("Users");
                _btnUsers.Click += (s, e) => ShowUsersView();

                _btnProducts = CreateNavButton("Inventory");
                _btnProducts.Click += (s, e) => ShowProductsView();

                _btnCategories = CreateNavButton("Categories");
                _btnCategories.Click += (s, e) => ShowCategoriesView();

                navFlow.Controls.Add(_btnOrders);
                navFlow.Controls.Add(_btnUsers);
                navFlow.Controls.Add(_btnProducts);
                navFlow.Controls.Add(_btnCategories);

                if (_userRole == "SuperAdmin")
                {
                    _btnAdmins = CreateNavButton("Admins");
                    _btnAdmins.Click += (s, e) => ShowAdminsView();
                    navFlow.Controls.Add(_btnAdmins);
                }
            }

            pnlNavCenter.Controls.Add(navFlow);
            pnlNavCenter.Resize += (s, e) => { navFlow.Location = new Point((pnlNavCenter.Width - navFlow.Width) / 2, (pnlNavCenter.Height - navFlow.Height) / 2); };
            topPanel.Controls.Add(pnlLogoutContainer); topPanel.Controls.Add(lblBrand); topPanel.Controls.Add(pnlNavCenter); pnlNavCenter.BringToFront();

            _rightCartPanel = new Panel { Dock = DockStyle.Right, Width = 380, BackColor = Color.White, Padding = new Padding(15), Visible = (_userRole == "User") };
            _rightCartPanel.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, _rightCartPanel.ClientRectangle, Color.LightGray, ButtonBorderStyle.Solid);
            Label lblCartTitle = new Label { Text = "Current Order", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = DbConfig.DarkColor, Dock = DockStyle.Top, Height = 50 };
            
            Panel checkoutPanel = new Panel { Dock = DockStyle.Bottom, Height = 160, BackColor = Color.White };
            _lblTotal = new Label { Text = "Total: ₱0.00", ForeColor = DbConfig.PrimaryColor, Font = new Font("Segoe UI", 20F, FontStyle.Bold), Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleRight, Height = 50 };
            ModernButton btnCheckout = new ModernButton { Text = "Checkout", BackColor = DbConfig.AccentColor, Dock = DockStyle.Top, Height = 50 };
            btnCheckout.Click += BtnCheckout_Click;
            ModernButton btnClear = new ModernButton { Text = "Clear All", BackColor = Color.IndianRed, Dock = DockStyle.Top, Height = 40 };
            btnClear.Click += (s, e) => { _cartItems.Clear(); UpdateCartUI(); };

            checkoutPanel.Controls.Add(btnCheckout); checkoutPanel.Controls.Add(new Panel { Height = 10, Dock = DockStyle.Top });
            checkoutPanel.Controls.Add(btnClear); checkoutPanel.Controls.Add(_lblTotal);
            _cartContainer = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.WhiteSmoke, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0) };
            _rightCartPanel.Controls.Add(_cartContainer); _rightCartPanel.Controls.Add(checkoutPanel); _rightCartPanel.Controls.Add(lblCartTitle);

            _sidebarCategories = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = DbConfig.DarkColor, AutoScroll = true, Padding = new Padding(0, 10, 0, 0), Visible = (_userRole == "User") };
            Label lblCatHeader = new Label { Text = "CATEGORIES", Dock = DockStyle.Top, ForeColor = Color.Silver, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Height = 30, Padding = new Padding(15, 0, 0, 0), TextAlign = ContentAlignment.BottomLeft };
            _sidebarCategories.Controls.Add(lblCatHeader);

            Panel centerPanel = new Panel { Dock = DockStyle.Fill, BackColor = DbConfig.LightColor };
            Panel centerHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(30, 0, 0, 0) };
            _lblPageTitle = new Label { Text = "Menu", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = DbConfig.DarkColor, Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
            centerHeader.Controls.Add(_lblPageTitle);
            
            _menuContainer = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = DbConfig.LightColor, Padding = new Padding(20), Visible = (_userRole == "User") };
            _historyContainer = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = DbConfig.LightColor, Padding = new Padding(20), Visible = (_userRole == "Admin") };
            
            _usersContainer = CreateListContainer();
            _productsContainer = CreateListContainer();
            _categoriesContainer = CreateListContainer(); 
            _adminsContainer = CreateListContainer();

            centerPanel.Controls.Add(_menuContainer);
            centerPanel.Controls.Add(_historyContainer);
            centerPanel.Controls.Add(_usersContainer);
            centerPanel.Controls.Add(_productsContainer);
            centerPanel.Controls.Add(_categoriesContainer);
            centerPanel.Controls.Add(_adminsContainer);
            centerPanel.Controls.Add(centerHeader);
            
            this.Controls.Add(centerPanel); 
            this.Controls.Add(_sidebarCategories); 
            this.Controls.Add(_rightCartPanel); 
            this.Controls.Add(topPanel);

            topPanel.BringToFront(); 
            if (_rightCartPanel.Visible) _rightCartPanel.BringToFront(); 
            if (_sidebarCategories.Visible) _sidebarCategories.BringToFront(); 
            centerPanel.BringToFront();
        }

        private FlowLayoutPanel CreateListContainer()
        {
            return new FlowLayoutPanel 
            { 
                Dock = DockStyle.Fill, AutoScroll = true, BackColor = DbConfig.LightColor, 
                Padding = new Padding(20), Visible = false, FlowDirection = FlowDirection.TopDown, WrapContents = false 
            };
        }

        private void HideAllViews()
        {
            _menuContainer.Visible = false;
            _sidebarCategories.Visible = false;
            _rightCartPanel.Visible = false;
            _usersContainer.Visible = false;
            _productsContainer.Visible = false;
            _categoriesContainer.Visible = false;
            _adminsContainer.Visible = false;
            _historyContainer.Visible = false;
        }

        private void ShowHistoryView()
        {
            HideAllViews();
            _historyContainer.Visible = true;
            _lblPageTitle.Text = "Order Management";
            ResetCategoryBtn();
            UpdateNavState(_btnOrders);
            LoadHistoryData();
        }

        private void ShowUsersView()
        {
            HideAllViews();
            _usersContainer.Visible = true;
            _lblPageTitle.Text = "User Management";
            ResetCategoryBtn();
            UpdateNavState(_btnUsers);
            LoadUsersData();
        }

        private void ShowProductsView()
        {
            HideAllViews();
            _productsContainer.Visible = true;
            _lblPageTitle.Text = "Inventory Management";
            ResetCategoryBtn();
            UpdateNavState(_btnProducts);
            LoadProductsData();
        }

        private void ShowCategoriesView()
        {
            HideAllViews();
            _categoriesContainer.Visible = true;
            _lblPageTitle.Text = "Category Management";
            ResetCategoryBtn();
            UpdateNavState(_btnCategories);
            LoadCategoriesData();
        }

        private void ShowAdminsView()
        {
            HideAllViews();
            _adminsContainer.Visible = true;
            _lblPageTitle.Text = "Admin Management";
            ResetCategoryBtn();
            UpdateNavState(_btnAdmins);
            LoadAdminsData();
        }

        private void ResetCategoryBtn()
        {
            if (_currentCategoryBtn != null) { _currentCategoryBtn.BackColor = DbConfig.DarkColor; _currentCategoryBtn.ForeColor = Color.White; _currentCategoryBtn = null; }
        }

        private void UpdateNavState(Button? activeBtn)
        {
            if (_btnOrders != null) _btnOrders.ForeColor = Color.FromArgb(200, 200, 200);
            if (_btnUsers != null) _btnUsers.ForeColor = Color.FromArgb(200, 200, 200);
            if (_btnProducts != null) _btnProducts.ForeColor = Color.FromArgb(200, 200, 200);
            if (_btnCategories != null) _btnCategories.ForeColor = Color.FromArgb(200, 200, 200);
            if (_btnAdmins != null) _btnAdmins.ForeColor = Color.FromArgb(200, 200, 200);
            if (activeBtn != null) activeBtn.ForeColor = Color.White;
        }

        // --- LOAD DATA METHODS ---

        private async void LoadCategoriesData()
        {
            _categoriesContainer.Controls.Clear();
            
            Panel header = new Panel { Width = _categoriesContainer.Width - 60, Height = 50 };
            Button btnAdd = new Button { Text = "+ Add Category", BackColor = Color.Green, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(150, 40), Location = new Point(0, 5) };
            btnAdd.Click += async (s, e) => {
                CategoryDialog dlg = new CategoryDialog();
                if(dlg.ShowDialog() == DialogResult.OK) {
                    _dbService.AddCategory(dlg.CategoryName);
                    
                    // REFRESH ENTIRE APP DATA
                    LoadCategoriesData();
                    await LoadDataAsync(); 
                }
            };
            header.Controls.Add(btnAdd);
            _categoriesContainer.Controls.Add(header);

            // Use cached categories
            var categories = await _dbService.GetCategoriesAsync(); 
            int itemWidth = Math.Max(400, _categoriesContainer.Width - 60);

            foreach (var cat in categories)
            {
                Panel card = new Panel { Width = itemWidth, Height = 60, BackColor = Color.White, Margin = new Padding(0, 0, 0, 10) };
                card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.LightGray, ButtonBorderStyle.Solid);

                Label lblName = new Label { Text = cat.Name, Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };

                Button btnDelete = new Button { Text = "Delete", BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(80, 30), Location = new Point(itemWidth - 100, 15), Anchor = AnchorStyles.Right | AnchorStyles.Top };
                btnDelete.Click += async (s, e) => {
                     if(MessageBox.Show($"Are you sure you want to delete category '{cat.Name}'? Products in this category will become Uncategorized.", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                         _dbService.DeleteCategory(cat.Id); 
                         
                         // REFRESH ENTIRE APP DATA
                         LoadCategoriesData();
                         await LoadDataAsync();
                     }
                };

                card.Controls.Add(btnDelete); card.Controls.Add(lblName);
                _categoriesContainer.Controls.Add(card);
            }
        }

        private async void LoadProductsData()
        {
            _productsContainer.Controls.Clear();
            
            Panel header = new Panel { Width = _productsContainer.Width - 60, Height = 50 };
            Button btnAdd = new Button { Text = "+ Add Product", BackColor = Color.Green, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(150, 40), Location = new Point(0, 5) };
            btnAdd.Click += async (s, e) => {
                // Fetch Categories First
                var cats = await _dbService.GetCategoriesAsync();
                if (cats.Count == 0) { MessageBox.Show("Please create a category first!"); return; }

                ProductDialog dlg = new ProductDialog(cats);
                if(dlg.ShowDialog() == DialogResult.OK) {
                    _dbService.AddProduct(dlg.PName, dlg.PPrice, dlg.PCategoryId, dlg.PQuantity, dlg.PImagePath);
                    
                    // REFRESH ENTIRE APP DATA
                    LoadProductsData();
                    await LoadDataAsync();
                }
            };
            header.Controls.Add(btnAdd);
            _productsContainer.Controls.Add(header);

            var items = await _dbService.GetFoodItemsAsync();
            int itemWidth = Math.Max(400, _productsContainer.Width - 60);

            foreach (var item in items)
            {
                Panel card = new Panel { Width = itemWidth, Height = 80, BackColor = Color.White, Margin = new Padding(0, 0, 0, 10) };
                card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.LightGray, ButtonBorderStyle.Solid);

                // Add Image Thumbnail in List
                PictureBox pb = new PictureBox { Size = new Size(60, 60), Location = new Point(10, 10), SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };
                if(!string.IsNullOrEmpty(item.ImagePath) && File.Exists(item.ImagePath))
                {
                    try { pb.Image = Image.FromFile(item.ImagePath); } catch {}
                }
                else
                {
                    // Placeholder logic - simple colored box or text
                    pb.BackColor = Color.LightGray; 
                }

                Label lblName = new Label { Text = item.Name, Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(80, 25), AutoSize = true };
                Label lblPrice = new Label { Text = $"₱{item.Price:F2}", Font = new Font("Segoe UI", 12F), ForeColor = DbConfig.PrimaryColor, Location = new Point(300, 25), AutoSize = true };
                
                // Show Category Name
                Label lblCat = new Label { Text = item.Category, Font = new Font("Segoe UI", 9F, FontStyle.Italic), ForeColor = Color.Gray, Location = new Point(80, 50), AutoSize = true };

                Label lblQty = new Label { Text = $"Stock: {item.Quantity}", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = (item.Quantity > 0 ? Color.DarkSlateGray : Color.Red), Location = new Point(410, 28), AutoSize = true };
                
                string stockText = item.IsAvailable ? "Enabled" : "Disabled";
                Color stockColor = item.IsAvailable ? Color.Gray : Color.Red;
                Button btnToggle = new Button { Text = stockText, BackColor = stockColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(100, 30), Location = new Point(itemWidth - 320, 25), Anchor = AnchorStyles.Right | AnchorStyles.Top };
                
                // REFRESH ON TOGGLE
                btnToggle.Click += async (s, e) => { 
                    _dbService.ToggleProductAvailability(item.Id); 
                    LoadProductsData();
                    await LoadDataAsync();
                };

                Button btnEdit = new Button { Text = "Edit", BackColor = Color.Orange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(70, 30), Location = new Point(itemWidth - 180, 25), Anchor = AnchorStyles.Right | AnchorStyles.Top };
                btnEdit.Click += async (s, e) => {
                    var cats = await _dbService.GetCategoriesAsync();
                    ProductDialog dlg = new ProductDialog(cats, item);
                    if(dlg.ShowDialog() == DialogResult.OK) {
                        _dbService.UpdateProduct(item.Id, dlg.PName, dlg.PPrice, dlg.PCategoryId, dlg.PQuantity, dlg.PImagePath);
                        
                        // REFRESH ENTIRE APP DATA
                        LoadProductsData();
                        await LoadDataAsync();
                    }
                };

                Button btnDelete = new Button { Text = "Del", BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(70, 30), Location = new Point(itemWidth - 100, 25), Anchor = AnchorStyles.Right | AnchorStyles.Top };
                btnDelete.Click += async (s, e) => {
                     if(MessageBox.Show($"Are you sure you want to delete '{item.Name}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                         _dbService.DeleteProduct(item.Id); 
                         
                         // REFRESH ENTIRE APP DATA
                         LoadProductsData();
                         await LoadDataAsync();
                     }
                };

                card.Controls.Add(pb);
                card.Controls.Add(lblCat); 
                card.Controls.Add(lblQty); 
                card.Controls.Add(btnToggle); card.Controls.Add(btnEdit); card.Controls.Add(btnDelete);
                card.Controls.Add(lblPrice); card.Controls.Add(lblName);
                _productsContainer.Controls.Add(card);
            }
        }

        private void LoadUsersData()
        {
            _usersContainer.Controls.Clear();
            
            Panel header = new Panel { Width = _usersContainer.Width - 60, Height = 50 };
            Button btnAdd = new Button { Text = "+ Add User", BackColor = Color.Green, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(150, 40), Location = new Point(0, 5) };
            btnAdd.Click += (s, e) => {
                RegisterForm dlg = new RegisterForm(); 
                // We show it as a dialog here for the admin to add a user instantly
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.ShowDialog();
                // Refresh list after closing
                LoadUsersData();
            };
            header.Controls.Add(btnAdd);
            _usersContainer.Controls.Add(header);

            var users = _dbService.GetUsers().Where(u => u.Role == "User").ToList(); 
            int itemWidth = Math.Max(400, _usersContainer.Width - 60);

            if (users.Count == 0) 
            {
                Label empty = new Label { Text = "No users found.", AutoSize = true, Padding = new Padding(20) };
                _usersContainer.Controls.Add(empty);
            }

            foreach (var user in users)
            {
                Panel card = new Panel { Width = itemWidth, Height = 70, BackColor = Color.White, Margin = new Padding(0, 0, 0, 10) };
                card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.LightGray, ButtonBorderStyle.Solid);

                Label lblName = new Label { Text = user.Username, Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
                Label lblRole = new Label { Text = "USER", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(200, 25), AutoSize = true };

                Button btnDelete = new Button { Text = "Delete", BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(80, 30), Location = new Point(itemWidth - 100, 20), Anchor = AnchorStyles.Right | AnchorStyles.Top };
                btnDelete.Click += (s, e) => {
                        if(MessageBox.Show($"Are you sure you want to delete user '{user.Username}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                            _dbService.DeleteUser(user.Username); LoadUsersData(); 
                        }
                };
                card.Controls.Add(btnDelete); card.Controls.Add(lblName); card.Controls.Add(lblRole);
                _usersContainer.Controls.Add(card);
            }
        }

        private void LoadAdminsData()
        {
            _adminsContainer.Controls.Clear();
            
            Panel header = new Panel { Width = _adminsContainer.Width - 60, Height = 50 };
            Button btnAdd = new Button { Text = "+ Create Admin", BackColor = Color.Green, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(150, 40), Location = new Point(0, 5) };
            btnAdd.Click += (s, e) => {
                AdminDialog dlg = new AdminDialog();
                if(dlg.ShowDialog() == DialogResult.OK) {
                    if(_dbService.CreateAdmin(dlg.Username, dlg.Password)) LoadAdminsData();
                    else MessageBox.Show("Username already exists.");
                }
            };
            header.Controls.Add(btnAdd);
            _adminsContainer.Controls.Add(header);

            var admins = _dbService.GetUsers().Where(u => u.Role == "Admin" || (u.Role == "SuperAdmin" && u.Username != _username)).ToList();
            int itemWidth = Math.Max(400, _adminsContainer.Width - 60);

            foreach (var user in admins)
            {
                Panel card = new Panel { Width = itemWidth, Height = 70, BackColor = Color.White, Margin = new Padding(0, 0, 0, 10) };
                card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.LightGray, ButtonBorderStyle.Solid);

                Label lblName = new Label { Text = user.Username, Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
                Label lblRole = new Label { Text = user.Role.ToUpper(), Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = DbConfig.PrimaryColor, Location = new Point(200, 25), AutoSize = true };

                if (user.Role != "SuperAdmin") 
                {
                    Button btnDelete = new Button { Text = "Delete", BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(80, 30), Location = new Point(itemWidth - 100, 20), Anchor = AnchorStyles.Right | AnchorStyles.Top };
                    btnDelete.Click += (s, e) => {
                        if(MessageBox.Show($"Are you sure you want to delete Admin '{user.Username}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
                            _dbService.DeleteUser(user.Username); LoadAdminsData(); 
                        }
                    };
                    card.Controls.Add(btnDelete);
                }

                card.Controls.Add(lblName); card.Controls.Add(lblRole);
                _adminsContainer.Controls.Add(card);
            }
        }

        private async void LoadHistoryData()
        {
            _historyContainer.Controls.Clear();
            var allOrders = await _dbService.GetOrdersAsync();
            
            // Header Panel for Filters
            Panel filterPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.WhiteSmoke, Width = _historyContainer.Width - 40 };
            
            // Month Filter (Right Aligned)
            Label lblFilter = new Label { Text = "Filter by Month:", AutoSize = true, Location = new Point(filterPanel.Width - 400, 20), Font = new Font("Segoe UI", 10) };
            
            int selectedIndex = _cbMonthFilter != null ? _cbMonthFilter.SelectedIndex : 0;
            _cbMonthFilter = new ComboBox { Location = new Point(filterPanel.Width - 280, 17), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            _cbMonthFilter.Items.Add("All Time");
            for (int i = 0; i < 12; i++) _cbMonthFilter.Items.Add(DateTime.Now.AddMonths(-i).ToString("MMMM yyyy"));
            _cbMonthFilter.SelectedIndex = selectedIndex;
            _cbMonthFilter.SelectedIndexChanged += (s, e) => LoadHistoryData();

            // Monthly Report Button (Right Aligned)
            Button btnMonthly = new Button
            {
                Text = "📅 Monthly Report",
                Location = new Point(filterPanel.Width - 120, 15),
                Width = 110,
                Height = 30,
                BackColor = Color.Orange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnMonthly.Click += (s, e) => {
                // Logic to download monthly report directly or via dialog
                if (_cbMonthFilter.SelectedIndex > 0)
                {
                    // If a specific month is selected, report that month
                    DateTime selectedDate = DateTime.Now.AddMonths(-(_cbMonthFilter.SelectedIndex - 1));
                    GenerateMonthlyCSV(selectedDate);
                }
                else
                {
                    // Default to current month if "All Time" selected, or show dialog
                    GenerateMonthlyCSV(DateTime.Now); 
                }
            };

            filterPanel.Controls.Add(lblFilter);
            filterPanel.Controls.Add(_cbMonthFilter);
            filterPanel.Controls.Add(btnMonthly);
            _historyContainer.Controls.Add(filterPanel);

            // FILTER LOGIC
            if (_cbMonthFilter != null && _cbMonthFilter.SelectedIndex > 0)
            {
                string selected = _cbMonthFilter.SelectedItem.ToString()!;
                allOrders = allOrders.Where(o => o.Date.ToString("MMMM yyyy") == selected).ToList();
            }

            allOrders = allOrders.OrderByDescending(o => o.Date).ToList();

            if (allOrders.Count == 0) 
            { 
                _historyContainer.Controls.Add(new Label { Text = "No transaction history available.", Font = new Font("Segoe UI", 12F), AutoSize = true, Padding = new Padding(20) }); 
                return; 
            }

            foreach(var order in allOrders)
            {
                Panel receipt = new Panel { Width = 280, Height = 460, BackColor = Color.White, Margin = new Padding(15) };
                receipt.Paint += (s, e) => { ControlPaint.DrawBorder(e.Graphics, receipt.ClientRectangle, Color.LightGray, ButtonBorderStyle.Solid); using (Pen pen = new Pen(Color.Gray, 2)) { pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash; e.Graphics.DrawLine(pen, 15, 80, receipt.Width - 15, 80); e.Graphics.DrawLine(pen, 15, receipt.Height - 140, receipt.Width - 15, receipt.Height - 140); } };

                // NEW: Small Independent Print Icon Button
                Button btnPrint = new Button 
                { 
                    Text = "🖨️", // Emoji Icon
                    Font = new Font("Segoe UI Emoji", 14F), 
                    BackColor = Color.Transparent, 
                    ForeColor = Color.Black, 
                    FlatStyle = FlatStyle.Flat, 
                    Size = new Size(35, 35),
                    Location = new Point(receipt.Width - 45, 5), 
                    Cursor = Cursors.Hand 
                };
                btnPrint.FlatAppearance.BorderSize = 1;
                btnPrint.FlatAppearance.BorderColor = Color.Gray;
                
                btnPrint.Click += async (s, e) => {
                    HistoryForm preview = new HistoryForm();
                    await preview.GenerateReceiptPreviewAsync(order);
                    preview.ShowDialog();
                };
                receipt.Controls.Add(btnPrint); btnPrint.BringToFront();

                if (_userRole == "Admin" || _userRole == "SuperAdmin")
                {
                    Label btnDelete = new Label { Text = "✕", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.IndianRed, Cursor = Cursors.Hand, AutoSize = true, Location = new Point(10, 5) };
                    btnDelete.Click += async (s, e) => { if(MessageBox.Show("Are you sure you want to delete this record permanently?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) { await _dbService.DeleteOrderAsync(order.Id); LoadHistoryData(); } };
                    receipt.Controls.Add(btnDelete); btnDelete.BringToFront();

                    Panel pnlAdminActions = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.WhiteSmoke };
                    Button btnComplete = new Button { Text = "✓ Complete", Dock = DockStyle.Left, Width = 140, FlatStyle = FlatStyle.Flat, BackColor = Color.LightGreen };
                    btnComplete.Click += async (s, e) => { await _dbService.UpdateOrderStatusAsync(order.Id, "Completed"); LoadHistoryData(); };
                    Button btnCancel = new Button { Text = "⚠ Cancel", Dock = DockStyle.Right, Width = 140, FlatStyle = FlatStyle.Flat, BackColor = Color.MistyRose };
                    btnCancel.Click += async (s, e) => { await _dbService.UpdateOrderStatusAsync(order.Id, "Cancelled"); LoadHistoryData(); };
                    pnlAdminActions.Controls.Add(btnComplete); pnlAdminActions.Controls.Add(btnCancel); receipt.Controls.Add(pnlAdminActions);
                }

                Label lblTitle = new Label { Text = "ORDER RECEIPT", Font = new Font("Courier New", 12F, FontStyle.Bold), Dock = DockStyle.Top, Height = 40, TextAlign = ContentAlignment.MiddleCenter };
                Label lblId = new Label { Text = $"#{order.Id} - {order.CustomerName}", Font = new Font("Courier New", 10F), ForeColor = Color.Gray, Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.MiddleCenter };
                Label lblItems = new Label { Text = order.Items.Replace(", ", "\n"), Font = new Font("Courier New", 10F), Location = new Point(15, 90), Size = new Size(250, 180) };

                Panel footer = new Panel { Dock = DockStyle.Bottom, Height = 100, BackColor = Color.Transparent };
                Label lblTotal = new Label { Text = $"TOTAL: ₱{order.Total:N2}", Font = new Font("Courier New", 14F, FontStyle.Bold), ForeColor = DbConfig.PrimaryColor, Dock = DockStyle.Top, Height = 40, TextAlign = ContentAlignment.MiddleCenter };
                Label lblDate = new Label { Text = order.Date.ToString("MMM dd, yyyy\nhh:mm tt").ToUpper(), Font = new Font("Courier New", 9F), ForeColor = Color.Gray, Dock = DockStyle.Top, Height = 40, TextAlign = ContentAlignment.MiddleCenter };
                Color statusColor = order.Status == "Completed" ? Color.Green : (order.Status == "Pending" ? Color.Orange : Color.Red);
                Label lblStatus = new Label { Text = $"[{order.Status.ToUpper()}]", Font = new Font("Courier New", 9F, FontStyle.Bold), ForeColor = statusColor, Dock = DockStyle.Bottom, Height = 20, TextAlign = ContentAlignment.MiddleCenter };

                footer.Controls.Add(lblStatus); footer.Controls.Add(lblDate); footer.Controls.Add(lblTotal);
                receipt.Controls.Add(lblItems); receipt.Controls.Add(footer); receipt.Controls.Add(lblId); receipt.Controls.Add(lblTitle); 
                _historyContainer.Controls.Add(receipt);
            }
        }

        private async void GenerateMonthlyCSV(DateTime month)
        {
            var allOrders = await _dbService.GetOrdersAsync();
            var filteredOrders = allOrders.Where(o => o.Date.Month == month.Month && o.Date.Year == month.Year).ToList();

            if (filteredOrders.Count == 0)
            {
                MessageBox.Show($"No records found for {month:MMMM yyyy}.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV File|*.csv";
                sfd.FileName = $"Monthly_Report_{month:yyyy_MM}.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"Monthly Report for {month:MMMM yyyy}");
                        sb.AppendLine("Order ID,Date,Customer,Total Amount,Status,Items Summary");

                        decimal monthlyTotal = 0;

                        foreach (var order in filteredOrders)
                        {
                            string safeItems = $"\"{order.Items.Replace("\"", "\"\"")}\"";
                            sb.AppendLine($"{order.Id},{order.Date},{order.CustomerName},{order.Total},{order.Status},{safeItems}");
                            monthlyTotal += order.Total;
                        }

                        sb.AppendLine($",,,Total Revenue:,{monthlyTotal},");

                        System.IO.File.WriteAllText(sfd.FileName, sb.ToString());
                        MessageBox.Show($"Report for {month:MMMM yyyy} saved successfully!\nTotal Orders: {filteredOrders.Count}\nTotal Revenue: {monthlyTotal:C2}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        // --- COMMON HELPERS ---
        private Button CreateNavButton(string text)
        {
            Button btn = new Button { Text = text, Height = 40, Width = 140, FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Color.FromArgb(200, 200, 200), Font = new Font("Segoe UI", 12F, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(20) };
            btn.FlatAppearance.BorderSize = 0; btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 60); return btn;
        }

        private Button CreateCategoryButton(string text)
        {
            ModernButton btn = new ModernButton 
            { 
                Text = text, 
                Dock = DockStyle.Top, 
                Height = 50, 
                BackColor = DbConfig.DarkColor, 
                ForeColor = Color.White, 
                Font = new Font("Segoe UI", 11F), 
                TextAlign = ContentAlignment.MiddleLeft, 
                Padding = new Padding(20, 0, 0, 0),
                Margin = new Padding(5)
            };
            
            System.Windows.Forms.Timer transitionTimer = new System.Windows.Forms.Timer { Interval = 16 };
            float currentR = btn.BackColor.R;
            float currentG = btn.BackColor.G;
            float currentB = btn.BackColor.B;
            
            int targetR = DbConfig.DarkColor.R;
            int targetG = DbConfig.DarkColor.G;
            int targetB = DbConfig.DarkColor.B;

            transitionTimer.Tick += (s, e) => 
            {
                float factor = 0.2f; 
                bool done = true;

                if (Math.Abs(targetR - currentR) > 0.5f) { currentR += (targetR - currentR) * factor; done = false; } else currentR = targetR;
                if (Math.Abs(targetG - currentG) > 0.5f) { currentG += (targetG - currentG) * factor; done = false; } else currentG = targetG;
                if (Math.Abs(targetB - currentB) > 0.5f) { currentB += (targetB - currentB) * factor; done = false; } else currentB = targetB;

                btn.BackColor = Color.FromArgb((int)currentR, (int)currentG, (int)currentB);
                if (done) transitionTimer.Stop();
            };

            btn.MouseEnter += (s, e) => {
                if (btn != _currentCategoryBtn) {
                    currentR = btn.BackColor.R; currentG = btn.BackColor.G; currentB = btn.BackColor.B;
                    targetR = 80; targetG = 80; targetB = 80; 
                    transitionTimer.Start();
                }
            };
            btn.MouseLeave += (s, e) => {
                if (btn != _currentCategoryBtn) {
                    currentR = btn.BackColor.R; currentG = btn.BackColor.G; currentB = btn.BackColor.B;
                    targetR = DbConfig.DarkColor.R; targetG = DbConfig.DarkColor.G; targetB = DbConfig.DarkColor.B;
                    transitionTimer.Start();
                }
            };

            btn.Click += (s, e) => { SetActiveCategory(btn); FilterMenu(text); }; 
            return btn;
        }

        private Panel CreateFoodCard(FoodItem item)
        {
            Panel card = new Panel { Width = 200, Height = 250, BackColor = Color.White, Margin = new Padding(10) };
            card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(230,230,230), ButtonBorderStyle.Solid);
            
            PictureBox pb = new PictureBox 
            { 
                Size = new Size(180, 100), 
                Location = new Point(10, 10), 
                SizeMode = PictureBoxSizeMode.Zoom, 
                BorderStyle = BorderStyle.None 
            };

            if(!string.IsNullOrEmpty(item.ImagePath) && File.Exists(item.ImagePath))
            {
                try { pb.Image = Image.FromFile(item.ImagePath); } 
                catch { SetPlaceholder(pb, item.Name); }
            }
            else
            {
                SetPlaceholder(pb, item.Name);
            }

            Label lblName = new Label { Text = item.Name, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = DbConfig.TextColor, Dock = DockStyle.Top, Height = 40, TextAlign = ContentAlignment.MiddleCenter, AutoEllipsis = true, Padding = new Padding(5) };
            
            Panel textPanel = new Panel { Location = new Point(0, 110), Size = new Size(200, 140) };
            
            Label lblPrice = new Label { Text = $"₱{item.Price:F2}", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = DbConfig.PrimaryColor, Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.MiddleCenter };
            
            string subText = item.Category;
            if(item.Quantity < 10 && item.Quantity > 0) subText += $" • Only {item.Quantity} left!";
            Label lblCat = new Label { Text = subText, Font = new Font("Segoe UI", 8F), ForeColor = Color.Gray, Dock = DockStyle.Top, Height = 20, TextAlign = ContentAlignment.MiddleCenter };
            
            ModernButton btnAdd = new ModernButton { Text = "Add to Order", BackColor = DbConfig.PrimaryColor, Dock = DockStyle.Bottom, Height = 40 };

            if (!item.IsAvailable || item.Quantity <= 0) 
            { 
                btnAdd.Text = "Out of Stock"; 
                btnAdd.BackColor = Color.Gray; 
                btnAdd.Enabled = true; 
                btnAdd.Click += (s, e) => { MessageBox.Show("Sorry, this product is out of stock.", "Product Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                card.BackColor = Color.WhiteSmoke; 
                lblName.ForeColor = Color.Gray; 
                lblPrice.ForeColor = Color.Gray; 
            }
            else 
            { 
                btnAdd.Click += (s, e) => { 
                    var existing = _cartItems.FirstOrDefault(c => c.Food.Id == item.Id); 
                    int currentQtyInCart = existing?.Quantity ?? 0;
                    
                    if (currentQtyInCart + 1 > item.Quantity)
                    {
                        MessageBox.Show($"Sorry, we only have {item.Quantity} of these available.", "Stock Limit Reached", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    if (existing != null) existing.Quantity++; 
                    else _cartItems.Add(new CartItem { Food = item, Quantity = 1 }); 
                    
                    UpdateCartUI(); 
                }; 
            }

            textPanel.Controls.Add(btnAdd);
            textPanel.Controls.Add(lblCat);
            textPanel.Controls.Add(lblPrice);
            textPanel.Controls.Add(lblName);

            card.Controls.Add(textPanel);
            card.Controls.Add(pb); 
            return card;
        }

        private void SetPlaceholder(PictureBox pb, string name)
        {
            Bitmap bmp = new Bitmap(pb.Width, pb.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.LightGray);
                string letter = string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpper();
                using (Font font = new Font("Segoe UI", 40, FontStyle.Bold))
                {
                    SizeF textSize = g.MeasureString(letter, font);
                    g.DrawString(letter, font, Brushes.Gray, (pb.Width - textSize.Width) / 2, (pb.Height - textSize.Height) / 2);
                }
            }
            pb.Image = bmp;
        }

        private async void BtnCheckout_Click(object? sender, EventArgs e)
        {
            if (_cartItems.Count == 0) return;
            decimal total = _cartItems.Sum(c => c.TotalPrice);
            var btn = sender as Button; if (btn != null) { btn.Enabled = false; btn.Text = "Processing..."; }
            try { 
                int orderId = await _dbService.PlaceOrderAsync(_cartItems, total, _username); 
                MessageBox.Show($"Order #{orderId} Placed Successfully!", "Success"); 
                _cartItems.Clear(); 
                UpdateCartUI(); 
                await LoadDataAsync();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); } finally { if (btn != null) { btn.Enabled = true; btn.Text = "Checkout"; } }
        }

        private void UpdateCartUI()
        {
            _cartContainer.Controls.Clear(); decimal total = 0; _cartContainer.SuspendLayout();
            foreach (var item in _cartItems) {
                total += item.TotalPrice;
                Panel row = new Panel { Width = _cartContainer.Width - 10, Height = 85, BackColor = Color.White, Margin = new Padding(5, 5, 5, 0) };
                row.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, row.ClientRectangle, Color.Transparent, 0, ButtonBorderStyle.None, Color.Transparent, 0, ButtonBorderStyle.None, Color.Transparent, 0, ButtonBorderStyle.None, Color.LightGray, 1, ButtonBorderStyle.Solid);
                Label lblName = new Label { Text = item.Food.Name, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = DbConfig.TextColor, Location = new Point(10, 8), AutoSize = false, Width = 180, Height = 25, AutoEllipsis = true };
                Label lblUnitPrice = new Label { Text = $"@ ₱{item.Food.Price:F2}", Font = new Font("Segoe UI", 8F), ForeColor = Color.Gray, Location = new Point(10, 32), AutoSize = true };
                Label lblLineTotal = new Label { Text = $"₱{item.TotalPrice:F2}", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = DbConfig.PrimaryColor, Location = new Point(10, 55), AutoSize = true };
                Panel qtyPanel = new Panel { Size = new Size(110, 35), Location = new Point(row.Width - 120, 25), BackColor = Color.WhiteSmoke };
                Button btnMinus = new Button { Text = "-", Width = 35, Dock = DockStyle.Left, FlatStyle = FlatStyle.Flat, BackColor = Color.LightGray }; btnMinus.FlatAppearance.BorderSize = 0;
                Label lblQty = new Label { Text = item.Quantity.ToString(), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
                Button btnPlus = new Button { Text = "+", Width = 35, Dock = DockStyle.Right, FlatStyle = FlatStyle.Flat, BackColor = Color.LightGray }; btnPlus.FlatAppearance.BorderSize = 0;
                
                btnMinus.Click += (s, e) => { item.Quantity--; if (item.Quantity <= 0) _cartItems.Remove(item); UpdateCartUI(); };
                
                btnPlus.Click += (s, e) => { 
                    if (item.Quantity + 1 > item.Food.Quantity) {
                         MessageBox.Show($"Max stock reached for {item.Food.Name}");
                         return;
                    }
                    item.Quantity++; 
                    UpdateCartUI(); 
                };
                
                qtyPanel.Controls.Add(lblQty); qtyPanel.Controls.Add(btnPlus); qtyPanel.Controls.Add(btnMinus);
                row.Controls.Add(lblName); row.Controls.Add(lblUnitPrice); row.Controls.Add(lblLineTotal); row.Controls.Add(qtyPanel);
                _cartContainer.Controls.Add(row);
            }
            _cartContainer.ResumeLayout(); _lblTotal.Text = $"Total: ₱{total:F2}";
        }
    }
}