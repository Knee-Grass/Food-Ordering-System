using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using Npgsql; 
using FoodOrderingSystem.Models;
using FoodOrderingSystem.Backend.Config; 

namespace FoodOrderingSystem.Data
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = DbConfig.ConnectionString;
        }

        // --- CATEGORY MANAGEMENT ---
        public async Task<List<Category>> GetCategoriesAsync()
        {
            var list = new List<Category>();
            try 
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand("SELECT category_id, name FROM categories ORDER BY name", conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new Category { Id = reader.GetInt32(0), Name = reader.GetString(1) });
                        }
                    }
                }
            }
            catch (Exception ex) { System.Windows.Forms.MessageBox.Show("DB Error: " + ex.Message); }
            return list;
        }

        public void AddCategory(string name)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("INSERT INTO categories (name) VALUES (@n)", conn))
                {
                    cmd.Parameters.AddWithValue("n", name);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateCategory(int id, string name)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("UPDATE categories SET name=@n WHERE category_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("n", name);
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteCategory(int id)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM categories WHERE category_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // --- PRODUCT MANAGEMENT ---
        public async Task<List<FoodItem>> GetFoodItemsAsync()
        {
            var list = new List<FoodItem>();
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    string sql = @"
                        SELECT p.product_id, p.name, p.price, c.category_id, c.name, p.is_available, p.quantity, p.image_data 
                        FROM products p 
                        LEFT JOIN categories c ON p.category_id = c.category_id 
                        ORDER BY c.name, p.name";
                    
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new FoodItem
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Price = reader.GetDecimal(2),
                                CategoryId = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                                Category = reader.IsDBNull(4) ? "Uncategorized" : reader.GetString(4),
                                IsAvailable = reader.GetBoolean(5),
                                Quantity = reader.GetInt32(6),
                                ImageData = reader.IsDBNull(7) ? string.Empty : reader.GetString(7)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Database Error: " + ex.Message);
            }
            return list;
        }

        public void AddProduct(string name, decimal price, int categoryId, int quantity, string imageData)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("INSERT INTO products (name, price, category_id, is_available, quantity, image_data) VALUES (@n, @p, @c, TRUE, @q, @img)", conn))
                {
                    cmd.Parameters.AddWithValue("n", name);
                    cmd.Parameters.AddWithValue("p", price);
                    cmd.Parameters.AddWithValue("c", categoryId);
                    cmd.Parameters.AddWithValue("q", quantity);
                    cmd.Parameters.AddWithValue("img", imageData ?? string.Empty);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateProduct(int id, string name, decimal price, int categoryId, int quantity, string imageData)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "UPDATE products SET name=@n, price=@p, category_id=@c, quantity=@q, image_data=@img, is_available = (CASE WHEN @q > 0 THEN TRUE ELSE FALSE END) WHERE product_id=@id";
                
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("n", name);
                    cmd.Parameters.AddWithValue("p", price);
                    cmd.Parameters.AddWithValue("c", categoryId);
                    cmd.Parameters.AddWithValue("q", quantity);
                    cmd.Parameters.AddWithValue("img", imageData ?? string.Empty);
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ToggleProductAvailability(int id)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("UPDATE products SET is_available = NOT is_available WHERE product_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteProduct(int id)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM products WHERE product_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // --- USER MANAGEMENT ---
        public string? AuthenticateUser(string username, string password)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT role FROM users WHERE username=@u AND password=@p", conn))
                {
                    cmd.Parameters.AddWithValue("u", username);
                    cmd.Parameters.AddWithValue("p", password); 
                    var result = cmd.ExecuteScalar();
                    return result?.ToString();
                }
            }
        }

        public bool CreateAdmin(string username, string password) => RegisterUserInternal(username, password, "Admin");
        public bool CreateCrew(string username, string password) => RegisterUserInternal(username, password, "Crew");
        
        public bool RegisterUser(string username, string password, string role = "User") => RegisterUserInternal(username, password, role);

        private bool RegisterUserInternal(string username, string password, string role)
        {
            try
            {
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("INSERT INTO users (username, password, role) VALUES (@u, @p, @r)", conn))
                    {
                        cmd.Parameters.AddWithValue("u", username);
                        cmd.Parameters.AddWithValue("p", password);
                        cmd.Parameters.AddWithValue("r", role);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (PostgresException ex) { if (ex.SqlState == "23505") return false; throw; }
        }

        public void UpdateUser(int userId, string username, string password = "")
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "UPDATE users SET username=@u" + (string.IsNullOrEmpty(password) ? "" : ", password=@p") + " WHERE user_id=@id";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("u", username);
                    if(!string.IsNullOrEmpty(password)) cmd.Parameters.AddWithValue("p", password);
                    cmd.Parameters.AddWithValue("id", userId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<User> GetUsers()
        {
            var list = new List<User>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT user_id, username, role FROM users ORDER BY username", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new User { Id = reader.GetInt32(0), Username = reader.GetString(1), Role = reader.GetString(2) });
                    }
                }
            }
            return list;
        }

        public void DeleteUser(string username)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM users WHERE username=@u AND role != 'SuperAdmin'", conn))
                {
                    cmd.Parameters.AddWithValue("u", username);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // --- ORDER MANAGEMENT ---
        public async Task<int> PlaceOrderAsync(List<CartItem> cartItems, decimal total, string customerName, string cashierName, bool decreaseStock = true, string? existingOrderCode = null)
        {
            int newOrderId = 0;
            string itemsSummary = string.Join(", ", cartItems.Select(i => $"{i.Food.Name} x{i.Quantity}"));
            
            // If existing code is provided (e.g. from a loaded receipt), reuse it. Otherwise generate new.
            string orderCode = existingOrderCode ?? Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var trans = await conn.BeginTransactionAsync()) 
                {
                    try
                    {
                        string sqlOrder = "INSERT INTO orders (customer_name, cashier_name, total_amount, items_summary, status, order_date, order_code) VALUES (@cust, @cashier, @total, @summ, 'Pending', NOW(), @code) RETURNING order_id";
                        using (var cmd = new NpgsqlCommand(sqlOrder, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("cust", customerName);
                            cmd.Parameters.AddWithValue("cashier", cashierName);
                            cmd.Parameters.AddWithValue("total", total);
                            cmd.Parameters.AddWithValue("summ", itemsSummary);
                            cmd.Parameters.AddWithValue("code", orderCode);
                            newOrderId = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                        }

                        string sqlItem = "INSERT INTO order_items (order_id, food_name, price_at_time, quantity) VALUES (@oid, @fname, @price, @qty)";
                        
                        string sqlUpdateStock = "UPDATE products SET quantity = quantity - @qty, is_available = (CASE WHEN quantity - @qty > 0 THEN TRUE ELSE FALSE END) WHERE product_id = @pid";

                        foreach (var item in cartItems)
                        {
                            using (var cmdItem = new NpgsqlCommand(sqlItem, conn, trans))
                            {
                                cmdItem.Parameters.AddWithValue("oid", newOrderId);
                                cmdItem.Parameters.AddWithValue("fname", item.Food.Name);
                                cmdItem.Parameters.AddWithValue("price", item.Food.Price);
                                cmdItem.Parameters.AddWithValue("qty", item.Quantity);
                                await cmdItem.ExecuteNonQueryAsync();
                            }

                            if (decreaseStock)
                            {
                                using (var cmdStock = new NpgsqlCommand(sqlUpdateStock, conn, trans))
                                {
                                    cmdStock.Parameters.AddWithValue("qty", item.Quantity);
                                    cmdStock.Parameters.AddWithValue("pid", item.Food.Id);
                                    await cmdStock.ExecuteNonQueryAsync();
                                }
                            }
                        }
                        await trans.CommitAsync();
                    }
                    catch { await trans.RollbackAsync(); throw; }
                }
            }
            return newOrderId;
        }

        // Method to deduct stock based on an existing order's items
        public async Task DeductStockForOrderAsync(int orderId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var trans = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // Get items associated with the order
                        var items = new List<(string Name, int Qty)>();
                        using(var cmdGet = new NpgsqlCommand("SELECT food_name, quantity FROM order_items WHERE order_id=@id", conn, trans))
                        {
                            cmdGet.Parameters.AddWithValue("id", orderId);
                            using(var reader = await cmdGet.ExecuteReaderAsync())
                            {
                                while(await reader.ReadAsync())
                                {
                                    items.Add((reader.GetString(0), reader.GetInt32(1)));
                                }
                            }
                        }

                        // Update Product Stock based on name matching
                        string sqlUpdate = "UPDATE products SET quantity = quantity - @qty, is_available = (CASE WHEN quantity - @qty > 0 THEN TRUE ELSE FALSE END) WHERE name = @name";
                        foreach(var i in items)
                        {
                            using(var cmdUp = new NpgsqlCommand(sqlUpdate, conn, trans))
                            {
                                cmdUp.Parameters.AddWithValue("qty", i.Qty);
                                cmdUp.Parameters.AddWithValue("name", i.Name.Trim()); 
                                await cmdUp.ExecuteNonQueryAsync();
                            }
                        }
                        await trans.CommitAsync();
                    }
                    catch { await trans.RollbackAsync(); throw; }
                }
            }
        }

        public async Task<List<OrderRecord>> GetOrdersAsync()
        {
            var orders = new List<OrderRecord>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT order_id, order_date, customer_name, total_amount, items_summary, status, cashier_name, order_code FROM orders ORDER BY order_date DESC", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        orders.Add(new OrderRecord 
                        { 
                            Id = reader.GetInt32(0), 
                            Date = reader.GetDateTime(1), 
                            CustomerName = reader.GetString(2), 
                            Total = reader.GetDecimal(3), 
                            Items = reader.GetString(4), 
                            Status = reader.GetString(5),
                            CashierName = reader.IsDBNull(6) ? "Unknown" : reader.GetString(6),
                            OrderCode = reader.IsDBNull(7) ? "N/A" : reader.GetString(7)
                        });
                    }
                }
            }
            return orders;
        }

        public async Task<OrderRecord?> GetOrderDetailsAsync(int orderId)
        {
            OrderRecord? order = null;
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sqlOrder = "SELECT order_id, order_date, customer_name, total_amount, items_summary, status, cashier_name, order_code FROM orders WHERE order_id = @id";
                using (var cmd = new NpgsqlCommand(sqlOrder, conn))
                {
                    cmd.Parameters.AddWithValue("id", orderId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync()) 
                        {
                            order = new OrderRecord 
                            { 
                                Id = reader.GetInt32(0), 
                                Date = reader.GetDateTime(1), 
                                CustomerName = reader.GetString(2), 
                                Total = reader.GetDecimal(3), 
                                Items = reader.GetString(4), 
                                Status = reader.GetString(5),
                                CashierName = reader.IsDBNull(6) ? "Unknown" : reader.GetString(6),
                                OrderCode = reader.IsDBNull(7) ? "N/A" : reader.GetString(7)
                            };
                        }
                    }
                }
                if (order != null)
                {
                    string sqlItems = "SELECT food_name, price_at_time, quantity FROM order_items WHERE order_id = @id";
                    using (var cmd = new NpgsqlCommand(sqlItems, conn))
                    {
                        cmd.Parameters.AddWithValue("id", orderId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync()) order.DetailedItems.Add(new OrderItem { Name = reader.GetString(0), Price = reader.GetDecimal(1), Quantity = reader.GetInt32(2) });
                        }
                    }
                }
            }
            return order;
        }

        public async Task DeleteOrderAsync(int orderId)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("DELETE FROM orders WHERE order_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("id", orderId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("UPDATE orders SET status=@s WHERE order_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("s", newStatus);
                    cmd.Parameters.AddWithValue("id", orderId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // --- NEW METHOD: Update Cashier Name ---
        public async Task UpdateOrderCashierAsync(int orderId, string cashierName)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("UPDATE orders SET cashier_name=@c WHERE order_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("c", cashierName);
                    cmd.Parameters.AddWithValue("id", orderId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}