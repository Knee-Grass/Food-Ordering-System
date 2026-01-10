using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        
        // Track if we are editing an existing order loaded via barcode
        private int _loadedOrderId = 0;

        // UI Controls 
        private FlowLayoutPanel _menuContainer = null!;
        private FlowLayoutPanel _cartContainer = null!;
        private FlowLayoutPanel _historyContainer = null!;
        private FlowLayoutPanel _usersContainer = null!;
        private FlowLayoutPanel _crewContainer = null!; 
        private FlowLayoutPanel _productsContainer = null!;
        private FlowLayoutPanel _adminsContainer = null!;
        private FlowLayoutPanel _categoriesContainer = null!; 
        private FlowLayoutPanel _customersContainer = null!;
        
        private Label _lblTotal = null!;
        private Panel _sidebarCategories = null!;
        private Panel _adminSidebar = null!; 
        private Panel _rightCartPanel = null!; 
        private Button? _currentCategoryBtn;
        private Label _lblPageTitle = null!; 
        
        // Order Management Specific Controls
        private ComboBox? _cbMonthFilter; 
        private Panel? _pnlHistoryFilters;
        private FlowLayoutPanel? _pnlHistoryList;
        private Button? _btnTabPending;
        private Button? _btnTabCompleted; 
        private Button? _btnTabCancelled; 
        private string _currentHistoryTab = "Pending";
        
        // Order Type Selection
        private RadioButton? _rbDineIn;
        private RadioButton? _rbTakeout;
        
        // Navigation Buttons
        private Button? _btnOrders; 
        private Button? _btnUsers; 
        private Button? _btnCrew; 
        private Button? _btnProducts;
        private Button? _btnCategories; 
        private Button? _btnAdmins; 
        private Button? _btnCustomers; 

        // Cashier Specific Controls
        private TextBox? _txtBarcode;
        private Button? _btnSearchOrder;

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

            string displayRole = role;
            if (role == "User") displayRole = "Cashier"; 
            
            this.Text = $"Food Ordering System - {displayRole} Mode ({username})";
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

            if (_userRole == "Crew")
            {
                ShowHistoryView();
            }
            else if (_userRole == "Admin" || _userRole == "SuperAdmin")
            {
                ShowProductsView(); 
            }
            if (_userRole == "User" || _userRole == "Customer")
            {
                FilterMenu("All");
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
        }

        private void PopulateSidebar()
        {
            if (_sidebarCategories == null) return;
            _sidebarCategories.Controls.Clear();
            if (_userRole != "User" && _userRole != "Customer") return;

            Label lblCatHeader = new Label { Text = "CATEGORIES", Dock = DockStyle.Top, ForeColor = Color.Silver, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Height = 30, Padding = new Padding(15, 0, 0, 0), TextAlign = ContentAlignment.BottomLeft };
            _sidebarCategories.Controls.Add(lblCatHeader);

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

            if (_userRole != "User" && _userRole != "Customer") return;

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

            string brandText = "GOURMET";
            if (_userRole == "SuperAdmin") brandText = "SUPER ADMIN";
            else if (_userRole == "Admin") brandText = "ADMIN PORTAL";
            else if (_userRole == "Crew") brandText = "CREW PORTAL";
            else if (_userRole == "User") brandText = "CASHIER"; 
            else if (_userRole == "Customer") brandText = "CUSTOMER";

            Label lblBrand = new Label 
            { 
                Text = brandText, 
                Font = new Font("Segoe UI", 22F, FontStyle.Bold), 
                ForeColor = Color.White, 
                AutoSize = true, 
                Dock = DockStyle.Left, 
                TextAlign = ContentAlignment.MiddleLeft
            };

            Panel pnlLogoutContainer = new Panel { Dock = DockStyle.Right, Width = 140, BackColor = Color.Transparent };
            Button btnLogout = new Button 
            { 
                Text = "Log Out", BackColor = Color.Crimson, ForeColor = Color.White,
                Size = new Size(120, 40), Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnLogout.FlatAppearance.BorderSize = 0;
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

            // --- ADMIN SIDEBAR LOGIC ---
            if (_userRole == "Admin" || _userRole == "SuperAdmin")
            {
                _adminSidebar = new Panel 
                { 
                    Dock = DockStyle.Left, 
                    Width = 260, 
                    BackColor = DbConfig.DarkColor, 
                    Padding = new Padding(0, 10, 0, 0)
                };

                FlowLayoutPanel sideFlow = new FlowLayoutPanel 
                { 
                    Dock = DockStyle.Fill, 
                    FlowDirection = FlowDirection.TopDown, 
                    WrapContents = false, 
                    AutoScroll = true,
                    BackColor = DbConfig.DarkColor
                };
                
                Label lblNavHeader = new Label { Text = "NAVIGATION", ForeColor = Color.Gray, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Margin = new Padding(20, 10, 0, 10), AutoSize = true };
                sideFlow.Controls.Add(lblNavHeader);

                _btnUsers = CreateSidebarButton("Cashiers");
                _btnUsers.Click += (s, e) => ShowUsersView();

                _btnCustomers = CreateSidebarButton("Customers");
                _btnCustomers.Click += (s, e) => ShowCustomersView();

                _btnProducts = CreateSidebarButton("Inventory");
                _btnProducts.Click += (s, e) => ShowProductsView();

                _btnCategories = CreateSidebarButton("Categories");
                _btnCategories.Click += (s, e) => ShowCategoriesView();

                _btnCrew = CreateSidebarButton("Crew");
                _btnCrew.Click += (s, e) => ShowCrewView();

                sideFlow.Controls.Add(_btnUsers);
                sideFlow.Controls.Add(_btnCustomers);
                sideFlow.Controls.Add(_btnProducts);
                sideFlow.Controls.Add(_btnCategories);
                sideFlow.Controls.Add(_btnCrew);

                if (_userRole == "SuperAdmin")
                {
                    _btnAdmins = CreateSidebarButton("Admins");
                    _btnAdmins.Click += (s, e) => ShowAdminsView();
                    sideFlow.Controls.Add(_btnAdmins);
                }
                
                _adminSidebar.Controls.Add(sideFlow);
                this.Controls.Add(_adminSidebar); // Add Sidebar
            }
            else 
            {
                if (_userRole == "Crew")
                {
                    _btnOrders = CreateNavButton("Orders");
                    _btnOrders.Click += (s, e) => ShowHistoryView();
                    navFlow.Controls.Add(_btnOrders);
                }
                
                pnlNavCenter.Controls.Add(navFlow);
                pnlNavCenter.Resize += (s, e) => { navFlow.Location = new Point((pnlNavCenter.Width - navFlow.Width) / 2, (pnlNavCenter.Height - navFlow.Height) / 2); };
                topPanel.Controls.Add(pnlNavCenter); 
                pnlNavCenter.BringToFront();
            }

            topPanel.Controls.Add(pnlLogoutContainer); 
            topPanel.Controls.Add(lblBrand); 

            // --- RIGHT CART PANEL CONFIGURATION ---
            _rightCartPanel = new Panel { Dock = DockStyle.Right, Width = 380, BackColor = Color.White, Padding = new Padding(15), Visible = (_userRole == "User" || _userRole == "Customer") };
            _rightCartPanel.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, _rightCartPanel.ClientRectangle, Color.LightGray, ButtonBorderStyle.Solid);
            
            Label lblCartTitle = new Label { Text = "Current Order", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = DbConfig.DarkColor, Dock = DockStyle.Top, Height = 40 };

            // -- CHECKOUT PANEL (Bottom of Cart) --
            Panel checkoutPanel = new Panel { Dock = DockStyle.Bottom, Height = 220, BackColor = Color.White }; 
            Panel pnlOrderType = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.White };
            _rbDineIn = new RadioButton { Text = "Dine In", AutoSize = true, Location = new Point(50, 10), Font = new Font("Segoe UI", 10), Checked = true };
            _rbTakeout = new RadioButton { Text = "Takeout", AutoSize = true, Location = new Point(200, 10), Font = new Font("Segoe UI", 10) };
            pnlOrderType.Controls.Add(_rbDineIn);
            pnlOrderType.Controls.Add(_rbTakeout);

            _lblTotal = new Label { Text = "Total: ₱0.00", ForeColor = DbConfig.PrimaryColor, Font = new Font("Segoe UI", 20F, FontStyle.Bold), Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleRight, Height = 50 };
            
            string btnText = _userRole == "Customer" ? "Print Receipt" : "Complete Order";
            Button btnCheckout = new Button { Text = btnText, BackColor = DbConfig.AccentColor, Dock = DockStyle.Top, Height = 50, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
            btnCheckout.FlatAppearance.BorderSize = 0;
            btnCheckout.Click += BtnCheckout_Click;
            
            Button btnClear = new Button { Text = "Clear All", BackColor = Color.IndianRed, Dock = DockStyle.Top, Height = 40, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, e) => { 
                _cartItems.Clear(); 
                _loadedOrderId = 0; // Reset any loaded order logic
                UpdateCartUI(); 
            };

            checkoutPanel.Controls.Add(btnCheckout); 
            checkoutPanel.Controls.Add(new Panel { Height = 10, Dock = DockStyle.Top });
            checkoutPanel.Controls.Add(btnClear); 
            checkoutPanel.Controls.Add(_lblTotal);
            checkoutPanel.Controls.Add(pnlOrderType); 

            // -- BARCODE PANEL (Bottom of Cart - Above Checkout) --
            // Moved logic here: Dock=Bottom added AFTER checkoutPanel (which is also Dock=Bottom) puts it ABOVE checkoutPanel
            Panel pnlBarcode = new Panel { Dock = DockStyle.Bottom, Height = 100, BackColor = Color.WhiteSmoke, Padding = new Padding(10) };
            pnlBarcode.Visible = (_userRole == "User"); // Only visible for Cashier

            Label lblCode = new Label { Text = "Load Order Code:", Font = new Font("Segoe UI", 10F, FontStyle.Bold), Dock = DockStyle.Top, Height = 25 };
            
            // Made barcode input bigger and bold
            _txtBarcode = new TextBox { Dock = DockStyle.Top, Font = new Font("Segoe UI", 16F, FontStyle.Bold), Height = 40 };
            
            _btnSearchOrder = new Button { Text = "Load Order", Dock = DockStyle.Bottom, BackColor = Color.Gray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Height = 35 };
            _btnSearchOrder.Click += async (s, e) => await SearchAndLoadOrder();
            _txtBarcode.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await SearchAndLoadOrder(); } };
            
            pnlBarcode.Controls.Add(_btnSearchOrder);
            pnlBarcode.Controls.Add(_txtBarcode);
            pnlBarcode.Controls.Add(lblCode);

            // -- CART ITEMS --
            _cartContainer = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.WhiteSmoke, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(0) };
            
            // Layout Order for Right Panel
            _rightCartPanel.Controls.Add(checkoutPanel); // Bottom-most
            if (_userRole == "User") _rightCartPanel.Controls.Add(pnlBarcode); // Above checkoutPanel
            _rightCartPanel.Controls.Add(lblCartTitle);  // Top
            _rightCartPanel.Controls.Add(_cartContainer); // Fills remaining space
            
            _sidebarCategories = new Panel { Dock = DockStyle.Left, Width = 220, BackColor = DbConfig.DarkColor, AutoScroll = true, Padding = new Padding(0, 10, 0, 0), Visible = (_userRole == "User" || _userRole == "Customer") };
            
            Panel centerPanel = new Panel { Dock = DockStyle.Fill, BackColor = DbConfig.LightColor };
            Panel centerHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(30, 0, 0, 0) };
            _lblPageTitle = new Label { Text = "Menu", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = DbConfig.DarkColor, Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
            centerHeader.Controls.Add(_lblPageTitle);
            
            _menuContainer = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = DbConfig.LightColor, Padding = new Padding(20), Visible = (_userRole == "User" || _userRole == "Customer") };
            _historyContainer = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = DbConfig.LightColor, Padding = new Padding(20), Visible = (_userRole == "Crew"), FlowDirection = FlowDirection.TopDown, WrapContents = false };
            
            _usersContainer = CreateListContainer();
            _productsContainer = CreateListContainer();
            _categoriesContainer = CreateListContainer(); 
            _adminsContainer = CreateListContainer();
            _crewContainer = CreateListContainer();
            _customersContainer = CreateListContainer();

            centerPanel.Controls.Add(_menuContainer);
            centerPanel.Controls.Add(_historyContainer);
            centerPanel.Controls.Add(_usersContainer);
            centerPanel.Controls.Add(_customersContainer);
            centerPanel.Controls.Add(_productsContainer);
            centerPanel.Controls.Add(_categoriesContainer);
            centerPanel.Controls.Add(_adminsContainer);
            centerPanel.Controls.Add(_crewContainer);
            centerPanel.Controls.Add(centerHeader);
            
            this.Controls.Add(centerPanel); 
            this.Controls.Add(_sidebarCategories); 
            this.Controls.Add(_rightCartPanel); 
            this.Controls.Add(topPanel);

            topPanel.BringToFront(); 
            if (_adminSidebar != null) _adminSidebar.BringToFront();
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
            _crewContainer.Visible = false;
            _customersContainer.Visible = false;
        }

        // --- CASHIER SEARCH LOGIC (Updated to Load into Cart) ---
        private async Task SearchAndLoadOrder()
        {
            if (_txtBarcode == null || string.IsNullOrWhiteSpace(_txtBarcode.Text)) return;
            string code = _txtBarcode.Text.Trim().ToUpper();

            _btnSearchOrder!.Text = "...";
            _btnSearchOrder.Enabled = false;

            try
            {
                var orders = await _dbService.GetOrdersAsync();
                // Find order matching code (usually inside CustomerName like "[Code: XXXXXX]") and is Unpaid
                var match = orders.FirstOrDefault(o => o.CustomerName != null && o.CustomerName.Contains(code) && o.Status == "Unpaid");

                if (match != null)
                {
                    // Fetch full details including items
                    var detailedOrder = await _dbService.GetOrderDetailsAsync(match.Id);
                    
                    if (detailedOrder != null && detailedOrder.DetailedItems.Count > 0)
                    {
                        // LOAD INTO CART for review/editing
                        _cartItems.Clear();
                        foreach(var item in detailedOrder.DetailedItems)
                        {
                            // Find original food item to link references (image, category etc)
                            var foodRef = _allFoodItems.FirstOrDefault(f => f.Name == item.Name);
                            if (foodRef != null)
                            {
                                _cartItems.Add(new CartItem 
                                { 
                                    Food = foodRef, 
                                    Quantity = item.Quantity 
                                });
                            }
                        }
                        
                        _loadedOrderId = detailedOrder.Id; // IMPORTANT: Track this ID
                        UpdateCartUI();
                        
                        // Set Order Type based on name if possible
                        if(detailedOrder.CustomerName.Contains("Takeout") && _rbTakeout != null) _rbTakeout.Checked = true;
                        if(detailedOrder.CustomerName.Contains("Dine In") && _rbDineIn != null) _rbDineIn.Checked = true;

                        MessageBox.Show($"Order #{match.Id} Loaded!\n\nOrder loaded into side cart.\nReview items then press 'Complete Order' to finalize.", "Order Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        _txtBarcode.Text = "";
                    }
                }
                else
                {
                    MessageBox.Show("Order not found or already finalized.", "Search Result", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                if (_btnSearchOrder != null) { _btnSearchOrder.Text = "Load Order"; _btnSearchOrder.Enabled = true; }
            }
        }

        // --- ORDER MANAGEMENT (HISTORY VIEW) ---
        private void ShowHistoryView()
        {
            HideAllViews();
            _historyContainer.Visible = true;
            _lblPageTitle.Text = "Order Management";
            ResetCategoryBtn();
            UpdateNavState(_btnOrders);
            
            _historyContainer.Controls.Clear();
            
            Panel pnlTabs = new Panel { Width = _historyContainer.Width - 40, Height = 50, Margin = new Padding(0, 0, 0, 10), BackColor = Color.Transparent };
            
            _btnTabPending = new Button { Text = "Pending", Dock = DockStyle.Left, Width = 150, FlatStyle = FlatStyle.Flat, BackColor = DbConfig.PrimaryColor, ForeColor = Color.White, Cursor = Cursors.Hand };
            _btnTabCompleted = new Button { Text = "Completed", Dock = DockStyle.Left, Width = 150, FlatStyle = FlatStyle.Flat, BackColor = Color.LightGray, ForeColor = Color.Black, Cursor = Cursors.Hand };
            _btnTabCancelled = new Button { Text = "Cancelled", Dock = DockStyle.Left, Width = 150, FlatStyle = FlatStyle.Flat, BackColor = Color.LightGray, ForeColor = Color.Black, Cursor = Cursors.Hand };
            
            _btnTabPending.FlatAppearance.BorderSize = 0;
            _btnTabCompleted.FlatAppearance.BorderSize = 0;
            _btnTabCancelled.FlatAppearance.BorderSize = 0;

            _btnTabPending.Click += (s,e) => SwitchHistoryTab("Pending");
            _btnTabCompleted.Click += (s,e) => SwitchHistoryTab("Completed");
            _btnTabCancelled.Click += (s,e) => SwitchHistoryTab("Cancelled");
            
            pnlTabs.Controls.Add(_btnTabCancelled);
            pnlTabs.Controls.Add(_btnTabCompleted);
            pnlTabs.Controls.Add(_btnTabPending);

            _pnlHistoryFilters = new Panel { Width = _historyContainer.Width - 40, Height = 50, Visible = false, BackColor = Color.WhiteSmoke, Margin = new Padding(0, 0, 0, 10) };
            Label lblFilter = new Label { Text = "Filter by Month:", AutoSize = true, Location = new Point(10, 15), Font = new Font("Segoe UI", 10) };
            
            _cbMonthFilter = new ComboBox { Location = new Point(120, 12), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            _cbMonthFilter.Items.Add("All Time");
            for (int i = 0; i < 12; i++) _cbMonthFilter.Items.Add(DateTime.Now.AddMonths(-i).ToString("MMMM yyyy"));
            _cbMonthFilter.SelectedIndex = 0;
            _cbMonthFilter.SelectedIndexChanged += (s, e) => LoadHistoryList(); 

            Button btnMonthly = new Button
            {
                Text = "📅 Monthly Report",
                Location = new Point(290, 10),
                Width = 160,
                Height = 30,
                BackColor = Color.Orange,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnMonthly.FlatAppearance.BorderSize = 0;
            btnMonthly.Click += (s, e) => {
                if (_cbMonthFilter.SelectedIndex > 0)
                {
                    DateTime selectedDate = DateTime.Now.AddMonths(-(_cbMonthFilter.SelectedIndex - 1));
                    GenerateMonthlyCSV(selectedDate);
                }
                else
                {
                    GenerateMonthlyCSV(DateTime.Now); 
                }
            };
            
            _pnlHistoryFilters.Controls.Add(btnMonthly);
            _pnlHistoryFilters.Controls.Add(_cbMonthFilter);
            _pnlHistoryFilters.Controls.Add(lblFilter);

            _pnlHistoryList = new FlowLayoutPanel { Width = _historyContainer.Width - 40, Height = 600, AutoScroll = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = true, BackColor = DbConfig.LightColor };
            
            _historyContainer.Controls.Add(pnlTabs);
            _historyContainer.Controls.Add(_pnlHistoryFilters);
            _historyContainer.Controls.Add(_pnlHistoryList);

            SwitchHistoryTab("Pending");
        }

        private void SwitchHistoryTab(string tab)
        {
            _currentHistoryTab = tab;
            
            if (_btnTabPending != null && _btnTabCompleted != null && _btnTabCancelled != null && _pnlHistoryFilters != null)
            {
                _btnTabPending.BackColor = Color.LightGray; _btnTabPending.ForeColor = Color.Black;
                _btnTabCompleted.BackColor = Color.LightGray; _btnTabCompleted.ForeColor = Color.Black;
                _btnTabCancelled.BackColor = Color.LightGray; _btnTabCancelled.ForeColor = Color.Black;

                if (tab == "Pending") { _btnTabPending.BackColor = DbConfig.PrimaryColor; _btnTabPending.ForeColor = Color.White; }
                else if (tab == "Completed") { _btnTabCompleted.BackColor = DbConfig.PrimaryColor; _btnTabCompleted.ForeColor = Color.White; }
                else if (tab == "Cancelled") { _btnTabCancelled.BackColor = DbConfig.PrimaryColor; _btnTabCancelled.ForeColor = Color.White; }

                _pnlHistoryFilters.Visible = (tab == "Completed" || tab == "Cancelled");

                foreach(Control c in _pnlHistoryFilters.Controls)
                {
                    if(c is Button btn && btn.Text.Contains("Report"))
                    {
                        btn.Visible = (tab == "Completed");
                    }
                }
            }
            
            LoadHistoryList();
        }

        private async void LoadHistoryList()
        {
            if (_pnlHistoryList == null) return;
            
            _pnlHistoryList.Controls.Clear();
            _pnlHistoryList.Controls.Add(new Label { Text = "Loading...", AutoSize = true });

            var allOrders = await _dbService.GetOrdersAsync();
            List<OrderRecord> filteredOrders = new List<OrderRecord>();

            if (_currentHistoryTab == "Pending")
            {
                filteredOrders = allOrders.Where(o => o.Status == "Pending").ToList();
            }
            else if (_currentHistoryTab == "Completed")
            {
                filteredOrders = allOrders.Where(o => o.Status == "Completed").ToList();
            }
            else if (_currentHistoryTab == "Cancelled")
            {
                filteredOrders = allOrders.Where(o => o.Status == "Cancelled").ToList();
            }

            if (_currentHistoryTab != "Pending" && _cbMonthFilter != null && _cbMonthFilter.SelectedIndex > 0 && _cbMonthFilter.SelectedItem != null)
            {
                string selected = _cbMonthFilter.SelectedItem.ToString()!;
                filteredOrders = filteredOrders.Where(o => o.Date.ToString("MMMM yyyy") == selected).ToList();
            }
            
            filteredOrders = filteredOrders.OrderByDescending(o => o.Date).ToList();
            
            _pnlHistoryList.Controls.Clear();

            if (filteredOrders.Count == 0)
            {
                _pnlHistoryList.Controls.Add(new Label { Text = "No orders found.", Font = new Font("Segoe UI", 12F), AutoSize = true, Padding = new Padding(20) });
                return;
            }

            foreach(var order in filteredOrders)
            {
                Panel receipt = new Panel { Width = 280, Height = 460, BackColor = Color.White, Margin = new Padding(15) };
                receipt.Paint += (s, e) => { ControlPaint.DrawBorder(e.Graphics, receipt.ClientRectangle, Color.LightGray, ButtonBorderStyle.Solid); using (Pen pen = new Pen(Color.Gray, 2)) { pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash; e.Graphics.DrawLine(pen, 15, 80, receipt.Width - 15, 80); e.Graphics.DrawLine(pen, 15, receipt.Height - 140, receipt.Width - 15, receipt.Height - 140); } };

                if (_currentHistoryTab == "Completed")
                {
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
                    
                    // UPDATED: Open HistoryForm with Cashier Name passed
                    btnPrint.Click += async (s, e) => {
                         HistoryForm form = new HistoryForm();
                         form.CashierName = _username; // Pass current user as Cashier
                         await form.GenerateReceiptPreviewAsync(order);
                         form.ShowDialog();
                    };
                    receipt.Controls.Add(btnPrint); btnPrint.BringToFront();
                }

                if (_currentHistoryTab != "Pending")
                {
                    Label btnDelete = new Label { Text = "✕", Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = Color.IndianRed, Cursor = Cursors.Hand, AutoSize = true, Location = new Point(10, 5) };
                    btnDelete.Click += async (s, e) => { 
                        if(MessageBox.Show("Are you sure you want to delete this record permanently?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) { 
                            await _dbService.DeleteOrderAsync(order.Id); 
                            LoadHistoryList(); 
                        } 
                    };
                    receipt.Controls.Add(btnDelete); btnDelete.BringToFront();
                }

                if (_currentHistoryTab == "Pending")
                {
                    Panel pnlAdminActions = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.WhiteSmoke };
                    Button btnComplete = new Button { Text = "✓ Complete", Dock = DockStyle.Left, Width = 140, FlatStyle = FlatStyle.Flat, BackColor = Color.LightGreen };
                    btnComplete.Click += async (s, e) => { 
                        await _dbService.UpdateOrderStatusAsync(order.Id, "Completed"); 
                        LoadHistoryList(); 
                    };
                    
                    Button btnCancel = new Button { Text = "⚠ Cancel", Dock = DockStyle.Right, Width = 140, FlatStyle = FlatStyle.Flat, BackColor = Color.MistyRose };
                    btnCancel.Click += async (s, e) => { 
                        await _dbService.UpdateOrderStatusAsync(order.Id, "Cancelled"); 
                        LoadHistoryList(); 
                    };
                    
                    pnlAdminActions.Controls.Add(btnComplete); pnlAdminActions.Controls.Add(btnCancel); receipt.Controls.Add(pnlAdminActions);
                }

                Label lblTitle = new Label { Text = "ORDER RECEIPT", Font = new Font("Courier New", 12F, FontStyle.Bold), Dock = DockStyle.Top, Height = 40, TextAlign = ContentAlignment.MiddleCenter };
                
                // Simplified display for list view
                string displayTitle = order.CustomerName.Length > 20 ? order.CustomerName.Substring(0, 20) + "..." : order.CustomerName;
                
                Label lblId = new Label { Text = $"#{order.Id} - {displayTitle}", Font = new Font("Courier New", 10F), ForeColor = Color.Gray, Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.MiddleCenter };
                Label lblItems = new Label { Text = order.Items.Replace(", ", "\n"), Font = new Font("Courier New", 10F), Location = new Point(15, 90), Size = new Size(250, 180) };

                Panel footer = new Panel { Dock = DockStyle.Bottom, Height = 100, BackColor = Color.Transparent };
                Label lblTotal = new Label { Text = $"TOTAL: ₱{order.Total:N2}", Font = new Font("Courier New", 14F, FontStyle.Bold), ForeColor = DbConfig.PrimaryColor, Dock = DockStyle.Top, Height = 40, TextAlign = ContentAlignment.MiddleCenter };
                Label lblDate = new Label { Text = order.Date.ToString("MMM dd, yyyy\nhh:mm tt").ToUpper(), Font = new Font("Courier New", 9F), ForeColor = Color.Gray, Dock = DockStyle.Top, Height = 40, TextAlign = ContentAlignment.MiddleCenter };
                Color statusColor = order.Status == "Completed" ? Color.Green : (order.Status == "Pending" ? Color.Orange : Color.Red);
                Label lblStatus = new Label { Text = $"[{order.Status.ToUpper()}]", Font = new Font("Courier New", 9F, FontStyle.Bold), ForeColor = statusColor, Dock = DockStyle.Bottom, Height = 20, TextAlign = ContentAlignment.MiddleCenter };

                footer.Controls.Add(lblStatus); footer.Controls.Add(lblDate); footer.Controls.Add(lblTotal);
                receipt.Controls.Add(lblItems); receipt.Controls.Add(footer); receipt.Controls.Add(lblId); receipt.Controls.Add(lblTitle); 
                _pnlHistoryList.Controls.Add(receipt);
            }
        }

        // --- OTHER VIEWS ---
        private void ShowUsersView()
        {
            HideAllViews();
            _usersContainer.Visible = true;
            _lblPageTitle.Text = "Cashier Management";
            ResetCategoryBtn();
            UpdateNavState(_btnUsers);
            LoadUsersData();
        }

        private void ShowCustomersView()
        {
            HideAllViews();
            _customersContainer.Visible = true;
            _lblPageTitle.Text = "Customer Management";
            ResetCategoryBtn();
            UpdateNavState(_btnCustomers);
            LoadCustomersData();
        }

        private void ShowCrewView()
        {
            HideAllViews();
            _crewContainer.Visible = true;
            _lblPageTitle.Text = "Crew Management";
            ResetCategoryBtn();
            UpdateNavState(_btnCrew);
            LoadCrewData();
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
            if (_btnCrew != null) _btnCrew.ForeColor = Color.FromArgb(200, 200, 200);
            if (_btnAdmins != null) _btnAdmins.ForeColor = Color.FromArgb(200, 200, 200);
            if (_btnCustomers != null) _btnCustomers.ForeColor = Color.FromArgb(200, 200, 200);
            
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
                if(dlg.ShowDialog() == DialogResult.OK) { _dbService.AddCategory(dlg.CategoryName); LoadCategoriesData(); await LoadDataAsync(); }
            };
            header.Controls.Add(btnAdd); _categoriesContainer.Controls.Add(header);
            var categories = await _dbService.GetCategoriesAsync(); 
            int itemWidth = Math.Max(400, _categoriesContainer.Width - 60);
            foreach (var cat in categories) { 
                Panel card = new Panel { Width = itemWidth, Height = 60, BackColor = Color.White, Margin = new Padding(0, 0, 0, 10) };
                card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.LightGray, ButtonBorderStyle.Solid);
                Label lblName = new Label { Text = cat.Name, Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true };
                Button btnEdit = new Button { Text = "Edit", BackColor = Color.Orange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(80, 30), Location = new Point(itemWidth - 190, 15), Anchor = AnchorStyles.Right | AnchorStyles.Top };
                btnEdit.Click += async (s, e) => { CategoryDialog dlg = new CategoryDialog(cat.Name); if (dlg.ShowDialog() == DialogResult.OK) { _dbService.UpdateCategory(cat.Id, dlg.CategoryName); LoadCategoriesData(); await LoadDataAsync(); } };
                Button btnDelete = new Button { Text = "Delete", BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(80, 30), Location = new Point(itemWidth - 100, 15), Anchor = AnchorStyles.Right | AnchorStyles.Top };
                btnDelete.Click += async (s, e) => { if(MessageBox.Show($"Delete category '{cat.Name}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) { _dbService.DeleteCategory(cat.Id); LoadCategoriesData(); await LoadDataAsync(); } };
                card.Controls.Add(btnEdit); card.Controls.Add(btnDelete); card.Controls.Add(lblName); _categoriesContainer.Controls.Add(card);
            }
        }

        private async void LoadProductsData()
        {
            _productsContainer.Controls.Clear();
            Panel header = new Panel { Width = _productsContainer.Width - 60, Height = 50 };
            Button btnAdd = new Button { Text = "+ Add Product", BackColor = Color.Green, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(150, 40), Location = new Point(0, 5) };
            btnAdd.Click += async (s, e) => {
                var cats = await _dbService.GetCategoriesAsync(); if (cats.Count == 0) { MessageBox.Show("Create category first!"); return; }
                ProductDialog dlg = new ProductDialog(cats); if(dlg.ShowDialog() == DialogResult.OK) { _dbService.AddProduct(dlg.PName, dlg.PPrice, dlg.PCategoryId, dlg.PQuantity, dlg.PImageData); LoadProductsData(); await LoadDataAsync(); }
            };
            header.Controls.Add(btnAdd); _productsContainer.Controls.Add(header);
            var items = await _dbService.GetFoodItemsAsync(); int itemWidth = Math.Max(400, _productsContainer.Width - 60);
            foreach (var item in items) {
                Panel card = new Panel { Width = itemWidth, Height = 80, BackColor = Color.White, Margin = new Padding(0, 0, 0, 10) };
                card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.LightGray, ButtonBorderStyle.Solid);
                PictureBox pb = new PictureBox { Size = new Size(60, 60), Location = new Point(10, 10), SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };
                if(!string.IsNullOrEmpty(item.ImageData)) { try { byte[] b = Convert.FromBase64String(item.ImageData); using (MemoryStream ms = new MemoryStream(b)) pb.Image = Image.FromStream(ms); } catch { pb.BackColor = Color.LightGray; } } else pb.BackColor = Color.LightGray;
                Label lblName = new Label { Text = item.Name, Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(80, 25), AutoSize = true };
                Label lblPrice = new Label { Text = $"₱{item.Price:F2}", Font = new Font("Segoe UI", 12F), ForeColor = DbConfig.PrimaryColor, Location = new Point(300, 25), AutoSize = true };
                Label lblQty = new Label { Text = $"Stock: {item.Quantity}", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = (item.Quantity > 0 ? Color.DarkSlateGray : Color.Red), Location = new Point(410, 28), AutoSize = true };
                Button btnToggle = new Button { Text = item.IsAvailable ? "Enabled" : "Disabled", BackColor = item.IsAvailable ? Color.Gray : Color.Red, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(100, 30), Location = new Point(itemWidth - 320, 25), Anchor = AnchorStyles.Right | AnchorStyles.Top };
                btnToggle.Click += async (s, e) => { if (!item.IsAvailable && item.Quantity <= 0) { MessageBox.Show("Cannot enable. Stock is 0."); return; } _dbService.ToggleProductAvailability(item.Id); LoadProductsData(); await LoadDataAsync(); };
                Button btnEdit = new Button { Text = "Edit", BackColor = Color.Orange, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(70, 30), Location = new Point(itemWidth - 180, 25), Anchor = AnchorStyles.Right | AnchorStyles.Top };
                btnEdit.Click += async (s, e) => { var cats = await _dbService.GetCategoriesAsync(); ProductDialog dlg = new ProductDialog(cats, item); if(dlg.ShowDialog() == DialogResult.OK) { _dbService.UpdateProduct(item.Id, dlg.PName, dlg.PPrice, dlg.PCategoryId, dlg.PQuantity, dlg.PImageData); LoadProductsData(); await LoadDataAsync(); } };
                Button btnDelete = new Button { Text = "Del", BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(70, 30), Location = new Point(itemWidth - 100, 25), Anchor = AnchorStyles.Right | AnchorStyles.Top };
                btnDelete.Click += async (s, e) => { if(MessageBox.Show($"Delete '{item.Name}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) { _dbService.DeleteProduct(item.Id); LoadProductsData(); await LoadDataAsync(); } };
                card.Controls.Add(pb); card.Controls.Add(lblQty); card.Controls.Add(btnToggle); card.Controls.Add(btnEdit); card.Controls.Add(btnDelete); card.Controls.Add(lblPrice); card.Controls.Add(lblName); _productsContainer.Controls.Add(card);
            }
        }

        private void LoadUsersData()
        {
            _usersContainer.Controls.Clear();
            Panel header = new Panel { Width = _usersContainer.Width - 60, Height = 50 };
            Button btnAdd = new Button { Text = "+ Add Cashier", BackColor = Color.Green, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(150, 40), Location = new Point(0, 5) };
            btnAdd.Click += (s, e) => { RegisterForm dlg = new RegisterForm(); dlg.StartPosition = FormStartPosition.CenterParent; dlg.ShowDialog(); LoadUsersData(); };
            header.Controls.Add(btnAdd); _usersContainer.Controls.Add(header);
            var users = _dbService.GetUsers().Where(u => u.Role == "User").ToList(); 
            int itemWidth = Math.Max(400, _usersContainer.Width - 60);
            foreach (var user in users) {
                Panel card = new Panel { Width = itemWidth, Height = 70, BackColor = Color.White, Margin = new Padding(0, 0, 0, 10) };
                card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.LightGray, ButtonBorderStyle.Solid);
                Label lblName = new Label { Text = user.Username, Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
                Label lblRole = new Label { Text = "CASHIER", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(200, 25), AutoSize = true };
                Button btnDelete = new Button { Text = "Delete", BackColor = Color.IndianRed, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(80, 30), Location = new Point(itemWidth - 100, 20), Anchor = AnchorStyles.Right | AnchorStyles.Top };
                btnDelete.Click += (s, e) => { if(MessageBox.Show($"Delete cashier '{user.Username}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) { _dbService.DeleteUser(user.Username); LoadUsersData(); } };
                card.Controls.Add(btnDelete); card.Controls.Add(lblName); card.Controls.Add(lblRole); _usersContainer.Controls.Add(card);
            }
        }
        
        private void LoadCustomersData() { /* ... */ _customersContainer.Controls.Clear(); var users = _dbService.GetUsers().Where(u => u.Role == "Customer").ToList(); foreach(var u in users) { Panel card=new Panel{Width=400,Height=70,BackColor=Color.White}; Label l=new Label{Text=u.Username,Location=new Point(20,20)}; Button b=new Button{Text="Delete",Location=new Point(300,20)}; b.Click+=(s,e)=>{_dbService.DeleteUser(u.Username); LoadCustomersData();}; card.Controls.Add(l); card.Controls.Add(b); _customersContainer.Controls.Add(card); } }
        private void LoadCrewData() { /* ... */ _crewContainer.Controls.Clear(); Panel h = new Panel{Height=50}; Button b=new Button{Text="+ Add Crew", BackColor=Color.Green}; b.Click+=(s,e)=>{CrewDialog d=new CrewDialog(); if(d.ShowDialog()==DialogResult.OK) _dbService.CreateCrew(d.Username,d.Password); LoadCrewData();}; h.Controls.Add(b); _crewContainer.Controls.Add(h); var users = _dbService.GetUsers().Where(u => u.Role == "Crew").ToList(); foreach(var u in users) { Panel card=new Panel{Width=400,Height=70,BackColor=Color.White}; Label l=new Label{Text=u.Username,Location=new Point(20,20)}; Button d=new Button{Text="Delete",Location=new Point(300,20)}; d.Click+=(s,e)=>{_dbService.DeleteUser(u.Username); LoadCrewData();}; card.Controls.Add(l); card.Controls.Add(d); _crewContainer.Controls.Add(card); } }
        private void LoadAdminsData() { /* ... */ _adminsContainer.Controls.Clear(); Panel h = new Panel{Height=50}; Button b=new Button{Text="+ Create Admin", BackColor=Color.Green}; b.Click+=(s,e)=>{AdminDialog d=new AdminDialog(); if(d.ShowDialog()==DialogResult.OK) _dbService.CreateAdmin(d.Username,d.Password); LoadAdminsData();}; h.Controls.Add(b); _adminsContainer.Controls.Add(h); var users = _dbService.GetUsers().Where(u => u.Role == "Admin" || u.Role == "SuperAdmin").ToList(); foreach(var u in users) { Panel card=new Panel{Width=400,Height=70,BackColor=Color.White}; Label l=new Label{Text=u.Username,Location=new Point(20,20)}; card.Controls.Add(l); _adminsContainer.Controls.Add(card); } }

        private async void GenerateMonthlyCSV(DateTime month)
        {
            var allOrders = await _dbService.GetOrdersAsync();
            var filteredOrders = allOrders.Where(o => o.Date.Month == month.Month && o.Date.Year == month.Year).ToList();
            if (filteredOrders.Count == 0) { MessageBox.Show("No records found.", "No Data", MessageBoxButtons.OK); return; }
            using (SaveFileDialog sfd = new SaveFileDialog()) {
                sfd.Filter = "CSV File|*.csv"; sfd.FileName = $"Monthly_Report_{month:yyyy_MM}.csv";
                if (sfd.ShowDialog() == DialogResult.OK) {
                    try {
                        StringBuilder sb = new StringBuilder(); sb.AppendLine($"Monthly Report for {month:MMMM yyyy}"); sb.AppendLine("Order ID,Date,Customer,Total Amount,Status,Items Summary");
                        decimal monthlyTotal = 0;
                        foreach (var order in filteredOrders) {
                            string safeItems = $"\"{order.Items.Replace("\"", "\"\"")}\"";
                            sb.AppendLine($"{order.Id},{order.Date},{order.CustomerName},{order.Total},{order.Status},{safeItems}");
                            monthlyTotal += order.Total;
                        }
                        sb.AppendLine($",,,Total Revenue:,{monthlyTotal},"); System.IO.File.WriteAllText(sfd.FileName, sb.ToString()); MessageBox.Show("Report saved!");
                    } catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
                }
            }
        }
        
        private Button CreateNavButton(string text) { Button btn = new Button { Text = text, Height = 40, Width = 140, FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Color.FromArgb(200, 200, 200), Font = new Font("Segoe UI", 12F, FontStyle.Bold), Cursor = Cursors.Hand, Margin = new Padding(20) }; btn.FlatAppearance.BorderSize = 0; return btn; }
        private Button CreateSidebarButton(string text) { Button btn = new Button { Text = text, Height = 60, Width = 260, FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = Color.FromArgb(200, 200, 200), Font = new Font("Segoe UI", 12F, FontStyle.Bold), Cursor = Cursors.Hand, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(30, 0, 0, 0) }; btn.FlatAppearance.BorderSize = 0; return btn; }
        
        private Button CreateCategoryButton(string text)
        {
            Button btn = new Button { Text = text, Dock = DockStyle.Top, Height = 50, BackColor = DbConfig.DarkColor, ForeColor = Color.White, Font = new Font("Segoe UI", 11F), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(20, 0, 0, 0), Margin = new Padding(5), FlatStyle = FlatStyle.Flat }; btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => { SetActiveCategory(btn); FilterMenu(text); }; return btn;
        }

        private Panel CreateFoodCard(FoodItem item)
        {
            Panel card = new Panel { Width = 200, Height = 250, BackColor = Color.White, Margin = new Padding(10) };
            card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(230,230,230), ButtonBorderStyle.Solid);
            PictureBox pb = new PictureBox { Size = new Size(180, 100), Location = new Point(10, 10), SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.None };
            if(!string.IsNullOrEmpty(item.ImageData)) { try { byte[] b = Convert.FromBase64String(item.ImageData); using (MemoryStream ms = new MemoryStream(b)) pb.Image = Image.FromStream(ms); } catch { SetPlaceholder(pb, item.Name); } } else SetPlaceholder(pb, item.Name);
            Label lblName = new Label { Text = item.Name, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = DbConfig.TextColor, Dock = DockStyle.Top, Height = 40, TextAlign = ContentAlignment.MiddleCenter, AutoEllipsis = true, Padding = new Padding(5) };
            Panel textPanel = new Panel { Location = new Point(0, 110), Size = new Size(200, 140) };
            Label lblPrice = new Label { Text = $"₱{item.Price:F2}", Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = DbConfig.PrimaryColor, Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.MiddleCenter };
            string subText = item.Category; if(item.Quantity < 10 && item.Quantity > 0) subText += $" • Only {item.Quantity} left!";
            Label lblCat = new Label { Text = subText, Font = new Font("Segoe UI", 8F), ForeColor = Color.Gray, Dock = DockStyle.Top, Height = 20, TextAlign = ContentAlignment.MiddleCenter };
            Button btnAdd = new Button { Text = "Add to Order", BackColor = DbConfig.PrimaryColor, Dock = DockStyle.Bottom, Height = 40, FlatStyle = FlatStyle.Flat, ForeColor = Color.White, Font = new Font("Segoe UI", 10F, FontStyle.Bold) }; btnAdd.FlatAppearance.BorderSize = 0;

            if (!item.IsAvailable || item.Quantity <= 0) { btnAdd.Text = "Out of Stock"; btnAdd.BackColor = Color.Gray; btnAdd.Enabled = true; btnAdd.Click += (s, e) => { MessageBox.Show("Out of stock."); }; card.BackColor = Color.WhiteSmoke; lblName.ForeColor = Color.Gray; lblPrice.ForeColor = Color.Gray; }
            else { btnAdd.Click += (s, e) => { 
                var existing = _cartItems.FirstOrDefault(c => c.Food.Id == item.Id); 
                int currentQtyInCart = existing?.Quantity ?? 0;
                if (currentQtyInCart + 1 > item.Quantity) { MessageBox.Show($"Only {item.Quantity} available."); return; }
                if (existing != null) existing.Quantity++; else _cartItems.Add(new CartItem { Food = item, Quantity = 1 }); UpdateCartUI(); }; 
            }
            textPanel.Controls.Add(btnAdd); textPanel.Controls.Add(lblCat); textPanel.Controls.Add(lblPrice); textPanel.Controls.Add(lblName); card.Controls.Add(textPanel); card.Controls.Add(pb); return card;
        }

        private void SetPlaceholder(PictureBox pb, string name) { Bitmap bmp = new Bitmap(pb.Width, pb.Height); using (Graphics g = Graphics.FromImage(bmp)) { g.Clear(Color.LightGray); string letter = string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpper(); using (Font font = new Font("Segoe UI", 40, FontStyle.Bold)) { SizeF textSize = g.MeasureString(letter, font); g.DrawString(letter, font, Brushes.Gray, (pb.Width - textSize.Width) / 2, (pb.Height - textSize.Height) / 2); } } pb.Image = bmp; }

        private async void BtnCheckout_Click(object? sender, EventArgs e)
        {
            if (_cartItems.Count == 0) 
            {
                MessageBox.Show("Please add items to the order first.", "Empty Order", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal total = _cartItems.Sum(c => c.TotalPrice);
            var btn = sender as Button; if (btn != null) { btn.Enabled = false; btn.Text = "Processing..."; }
            
            string orderType = (_rbDineIn != null && _rbDineIn.Checked) ? "Dine In" : "Takeout";
            
            try { 
                if (_userRole == "Customer")
                {
                    string code = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                    string displayCustomerName = $"{_username} ({orderType}) [Code: {code}]";
                    
                    await _dbService.PlaceOrderAsync(_cartItems, total, displayCustomerName); 
                    
                    // Auto-mark as Unpaid for Cashier pickup
                    var orders = await _dbService.GetOrdersAsync();
                    var myOrder = orders.OrderByDescending(o => o.Id).FirstOrDefault(o => o.CustomerName == displayCustomerName);
                    if (myOrder != null) await _dbService.UpdateOrderStatusAsync(myOrder.Id, "Unpaid");

                    MessageBox.Show($"RECEIPT PRINTED\n\nYour Order Code: {code}\n\nPlease show this code to the cashier to finalize your order.", "Order Saved");
                }
                else // CASHIER MODE
                {
                    if (_loadedOrderId > 0)
                    {
                        // UPDATING EXISTING LOADED ORDER to "Pending" (Sent to Kitchen)
                        await _dbService.UpdateOrderStatusAsync(_loadedOrderId, "Pending");
                        MessageBox.Show($"Order #{_loadedOrderId} Finalized and Sent to Kitchen!", "Success");
                    }
                    else
                    {
                        // NEW DIRECT ORDER
                        string displayCustomerName = $"{_username} ({orderType})";
                        int orderId = await _dbService.PlaceOrderAsync(_cartItems, total, displayCustomerName); 
                        MessageBox.Show($"Order #{orderId} Placed Successfully! ({orderType})", "Success");
                    }
                }

                _cartItems.Clear(); 
                _loadedOrderId = 0;
                UpdateCartUI(); 
                await LoadDataAsync();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); } finally { if (btn != null) { btn.Enabled = true; btn.Text = _userRole == "Customer" ? "Print Receipt" : "Complete Order"; } }
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
                btnPlus.Click += (s, e) => { if (item.Quantity + 1 > item.Food.Quantity) { MessageBox.Show($"Max stock reached."); return; } item.Quantity++; UpdateCartUI(); };
                
                qtyPanel.Controls.Add(lblQty); qtyPanel.Controls.Add(btnPlus); qtyPanel.Controls.Add(btnMinus);
                row.Controls.Add(lblName); row.Controls.Add(lblUnitPrice); row.Controls.Add(lblLineTotal); row.Controls.Add(qtyPanel);
                _cartContainer.Controls.Add(row);
            }
            _cartContainer.ResumeLayout(); _lblTotal.Text = $"Total: ₱{total:F2}";
        }
    }
}