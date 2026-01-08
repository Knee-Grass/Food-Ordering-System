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
                    // Fetch image_path
                    string sql = @"
                        SELECT p.product_id, p.name, p.price, c.category_id, c.name, p.is_available, p.quantity, p.image_path 
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
                                ImagePath = reader.IsDBNull(7) ? string.Empty : reader.GetString(7) // Retrieve Image Path
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

        public void AddProduct(string name, decimal price, int categoryId, int quantity, string imagePath)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                // Insert image_path
                using (var cmd = new NpgsqlCommand("INSERT INTO products (name, price, category_id, is_available, quantity, image_path) VALUES (@n, @p, @c, TRUE, @q, @img)", conn))
                {
                    cmd.Parameters.AddWithValue("n", name);
                    cmd.Parameters.AddWithValue("p", price);
                    cmd.Parameters.AddWithValue("c", categoryId);
                    cmd.Parameters.AddWithValue("q", quantity);
                    cmd.Parameters.AddWithValue("img", imagePath ?? string.Empty);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateProduct(int id, string name, decimal price, int categoryId, int quantity, string imagePath)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                // Update image_path
                string sql = "UPDATE products SET name=@n, price=@p, category_id=@c, quantity=@q, image_path=@img, is_available = (CASE WHEN @q > 0 THEN TRUE ELSE FALSE END) WHERE product_id=@id";
                
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("n", name);
                    cmd.Parameters.AddWithValue("p", price);
                    cmd.Parameters.AddWithValue("c", categoryId);
                    cmd.Parameters.AddWithValue("q", quantity);
                    cmd.Parameters.AddWithValue("img", imagePath ?? string.Empty);
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
        public bool RegisterUser(string username, string password) => RegisterUserInternal(username, password, "User");

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
        public async Task<int> PlaceOrderAsync(List<CartItem> cartItems, decimal total, string customerName)
        {
            int newOrderId = 0;
            string itemsSummary = string.Join(", ", cartItems.Select(i => $"{i.Food.Name} x{i.Quantity}"));

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var trans = await conn.BeginTransactionAsync()) 
                {
                    try
                    {
                        string sqlOrder = "INSERT INTO orders (customer_name, total_amount, items_summary, status, order_date) VALUES (@cust, @total, @summ, 'Pending', NOW()) RETURNING order_id";
                        using (var cmd = new NpgsqlCommand(sqlOrder, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("cust", customerName);
                            cmd.Parameters.AddWithValue("total", total);
                            cmd.Parameters.AddWithValue("summ", itemsSummary);
                            newOrderId = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                        }

                        string sqlItem = "INSERT INTO order_items (order_id, food_name, price_at_time, quantity) VALUES (@oid, @fname, @price, @qty)";
                        string sqlDeduct = "UPDATE products SET quantity = quantity - @qty WHERE product_id = @pid AND quantity >= @qty";
                        string sqlCheckZero = "UPDATE products SET is_available = FALSE WHERE product_id = @pid AND quantity <= 0";

                        foreach (var item in cartItems)
                        {
                            using (var cmdDeduct = new NpgsqlCommand(sqlDeduct, conn, trans))
                            {
                                cmdDeduct.Parameters.AddWithValue("qty", item.Quantity);
                                cmdDeduct.Parameters.AddWithValue("pid", item.Food.Id);
                                if (await cmdDeduct.ExecuteNonQueryAsync() == 0) throw new Exception($"Insufficient stock for {item.Food.Name}");
                            }
                            using (var cmdZero = new NpgsqlCommand(sqlCheckZero, conn, trans))
                            {
                                cmdZero.Parameters.AddWithValue("pid", item.Food.Id);
                                await cmdZero.ExecuteNonQueryAsync();
                            }
                            using (var cmdItem = new NpgsqlCommand(sqlItem, conn, trans))
                            {
                                cmdItem.Parameters.AddWithValue("oid", newOrderId);
                                cmdItem.Parameters.AddWithValue("fname", item.Food.Name);
                                cmdItem.Parameters.AddWithValue("price", item.Food.Price);
                                cmdItem.Parameters.AddWithValue("qty", item.Quantity);
                                await cmdItem.ExecuteNonQueryAsync();
                            }
                        }
                        await trans.CommitAsync();
                    }
                    catch { await trans.RollbackAsync(); throw; }
                }
            }
            return newOrderId;
        }

        public async Task<List<OrderRecord>> GetOrdersAsync()
        {
            var orders = new List<OrderRecord>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT order_id, order_date, customer_name, total_amount, items_summary, status FROM orders ORDER BY order_date DESC", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        orders.Add(new OrderRecord { Id = reader.GetInt32(0), Date = reader.GetDateTime(1), CustomerName = reader.GetString(2), Total = reader.GetDecimal(3), Items = reader.GetString(4), Status = reader.GetString(5) });
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
                string sqlOrder = "SELECT order_id, order_date, customer_name, total_amount, items_summary, status FROM orders WHERE order_id = @id";
                using (var cmd = new NpgsqlCommand(sqlOrder, conn))
                {
                    cmd.Parameters.AddWithValue("id", orderId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync()) order = new OrderRecord { Id = reader.GetInt32(0), Date = reader.GetDateTime(1), CustomerName = reader.GetString(2), Total = reader.GetDecimal(3), Items = reader.GetString(4), Status = reader.GetString(5) };
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
    }
}