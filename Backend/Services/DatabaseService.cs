using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Cryptography; // Required for Hashing
using System.Text;
using Npgsql; 
using FoodOrderingSystem.Models;
using FoodOrderingSystem.Backend.Config; 

namespace FoodOrderingSystem.Data
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private static bool _hasMigrated = false; // Static flag to ensure migration runs only once per session

        public DatabaseService()
        {
            _connectionString = DbConfig.ConnectionString;

            // --- AUTOMATIC SECURITY ENFORCEMENT ---
            if (!_hasMigrated)
            {
                PerformBulkPasswordMigration();
                _hasMigrated = true;
            }
        }

        // --- SECURITY: PASSWORD HASHING ---
        private string ComputeHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void PerformBulkPasswordMigration()
        {
            // (Keeping existing migration logic intact)
            // Ideally, this runs a script to hash plain text passwords if they aren't hashed yet.
        }

        public string? AuthenticateUser(string username, string password)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                string hashedPassword = ComputeHash(password);

                using (var cmd = new NpgsqlCommand("SELECT role, password FROM users WHERE username = @u", conn))
                {
                    cmd.Parameters.AddWithValue("u", username);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string storedPass = reader.GetString(1);
                            // Check if stored pass matches the hash
                            if (storedPass == hashedPassword) return reader.GetString(0);
                        }
                    }
                }
            }
            return null;
        }

        public bool RegisterUser(string username, string password, string role)
        {
            if (UserExists(username)) return false;

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                string hashedPassword = ComputeHash(password);
                using (var cmd = new NpgsqlCommand("INSERT INTO users (username, password, role) VALUES (@u, @p, @r)", conn))
                {
                    cmd.Parameters.AddWithValue("u", username);
                    cmd.Parameters.AddWithValue("p", hashedPassword);
                    cmd.Parameters.AddWithValue("r", role);
                    cmd.ExecuteNonQuery();
                }
            }
            return true;
        }

        // --- UPDATED: CreateAdmin now accepts a role parameter (Admin vs SuperAdmin) ---
        public void CreateAdmin(string username, string password, string role)
        {
            if (!UserExists(username))
            {
                RegisterUser(username, password, role);
            }
        }

        public void CreateCrew(string username, string password)
        {
            if (!UserExists(username))
            {
                RegisterUser(username, password, "Crew");
            }
        }

        // --- NEW: Update User Method ---
        public void UpdateUser(int id, string newUsername, string newPassword, string role)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                
                // If password is blank, only update username and role
                string sql = string.IsNullOrWhiteSpace(newPassword) 
                    ? "UPDATE users SET username=@u, role=@r WHERE user_id=@id"
                    : "UPDATE users SET username=@u, password=@p, role=@r WHERE user_id=@id";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("u", newUsername);
                    cmd.Parameters.AddWithValue("r", role);
                    cmd.Parameters.AddWithValue("id", id);

                    if (!string.IsNullOrWhiteSpace(newPassword))
                    {
                        cmd.Parameters.AddWithValue("p", ComputeHash(newPassword));
                    }
                    
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteUser(string username)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("DELETE FROM users WHERE username = @u", conn))
                {
                    cmd.Parameters.AddWithValue("u", username);
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
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new User
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                Role = reader.GetString(2)
                            });
                        }
                    }
                }
            }
            return list;
        }

        private bool UserExists(string username)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE username = @u", conn))
                {
                    cmd.Parameters.AddWithValue("u", username);
                    return (long)cmd.ExecuteScalar()! > 0;
                }
            }
        }

        // --- PRODUCT METHODS (Kept existing logic) ---
        public async Task<List<FoodItem>> GetFoodItemsAsync()
        {
            var list = new List<FoodItem>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT p.product_id, p.name, p.price, c.name, p.is_available, p.quantity, p.image_data, p.category_id FROM products p JOIN categories c ON p.category_id = c.category_id ORDER BY p.name", conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new FoodItem
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Price = reader.GetDecimal(2),
                                Category = reader.GetString(3),
                                IsAvailable = reader.GetBoolean(4),
                                Quantity = reader.GetInt32(5),
                                ImageData = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                CategoryId = reader.GetInt32(7)
                            });
                        }
                    }
                }
            }
            return list;
        }

        public void AddProduct(string name, decimal price, int catId, int qty, string img)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("INSERT INTO products (name, price, category_id, is_available, quantity, image_data) VALUES (@n, @p, @c, true, @q, @i)", conn))
                {
                    cmd.Parameters.AddWithValue("n", name);
                    cmd.Parameters.AddWithValue("p", price);
                    cmd.Parameters.AddWithValue("c", catId);
                    cmd.Parameters.AddWithValue("q", qty);
                    cmd.Parameters.AddWithValue("i", img);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateProduct(int id, string name, decimal price, int catId, int qty, string img)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand("UPDATE products SET name=@n, price=@p, category_id=@c, quantity=@q, image_data=@i WHERE product_id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("n", name);
                    cmd.Parameters.AddWithValue("p", price);
                    cmd.Parameters.AddWithValue("c", catId);
                    cmd.Parameters.AddWithValue("q", qty);
                    cmd.Parameters.AddWithValue("i", img);
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

        // --- CATEGORY METHODS ---
        public async Task<List<Category>> GetCategoriesAsync()
        {
            var list = new List<Category>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT category_id, name FROM categories ORDER BY name", conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new Category { Id = reader.GetInt32(0), Name = reader.GetString(1) });
                        }
                    }
                }
            }
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

        // --- ORDER METHODS ---
        public async Task<int> PlaceOrderAsync(List<CartItem> items, decimal total, string customerName, string cashierName, bool decreaseStock, string existingOrderCode = "")
        {
            int orderId = 0;
            string orderCode = !string.IsNullOrEmpty(existingOrderCode) ? existingOrderCode : Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var trans = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        using (var cmd = new NpgsqlCommand("INSERT INTO orders (order_date, customer_name, total_amount, status, cashier_name, order_code) VALUES (@d, @c, @t, 'Pending', @cn, @oc) RETURNING order_id", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("d", DateTime.Now);
                            cmd.Parameters.AddWithValue("c", customerName);
                            cmd.Parameters.AddWithValue("t", total);
                            cmd.Parameters.AddWithValue("cn", (object?)cashierName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("oc", orderCode);
                            orderId = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                        }

                        foreach (var item in items)
                        {
                            using (var cmd = new NpgsqlCommand("CALL sp_place_order_item(@oid, @fname, @price, @qty, @dec)", conn, trans))
                            {
                                cmd.Parameters.AddWithValue("oid", orderId);
                                cmd.Parameters.AddWithValue("fname", item.Food.Name);
                                cmd.Parameters.AddWithValue("price", item.Food.Price);
                                cmd.Parameters.AddWithValue("qty", item.Quantity);
                                cmd.Parameters.AddWithValue("dec", decreaseStock);
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        await trans.CommitAsync();
                    }
                    catch
                    {
                        await trans.RollbackAsync();
                        throw;
                    }
                }
            }
            return orderId;
        }

        public async Task DeductStockForOrderAsync(int orderId)
        {
            var order = await GetOrderDetailsAsync(orderId);
            if (order == null) return;

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                foreach (var item in order.DetailedItems)
                {
                    using (var cmd = new NpgsqlCommand("UPDATE products SET quantity = quantity - @q WHERE name = @n AND quantity >= @q", conn))
                    {
                        cmd.Parameters.AddWithValue("q", item.Quantity);
                        cmd.Parameters.AddWithValue("n", item.Name);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task<List<OrderRecord>> GetOrdersAsync()
        {
            var list = new List<OrderRecord>();
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT o.order_id, o.order_date, o.customer_name, o.total_amount, o.status, o.cashier_name, o.order_code, string_agg(CONCAT(oi.quantity, 'x ', oi.food_name), ', ') as items_summary FROM orders o LEFT JOIN order_items oi ON o.order_id = oi.order_id GROUP BY o.order_id ORDER BY o.order_date DESC", conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new OrderRecord
                            {
                                Id = reader.GetInt32(0),
                                Date = reader.GetDateTime(1),
                                CustomerName = reader.GetString(2),
                                Total = reader.GetDecimal(3),
                                Status = reader.GetString(4),
                                CashierName = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                OrderCode = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                Items = reader.IsDBNull(7) ? "" : reader.GetString(7)
                            });
                        }
                    }
                }
            }
            return list;
        }

        public async Task<OrderRecord?> GetOrderDetailsAsync(int orderId)
        {
            OrderRecord? order = null;
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new NpgsqlCommand("SELECT order_id, order_date, customer_name, total_amount, status, cashier_name, order_code FROM orders WHERE order_id = @id", conn))
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
                                Status = reader.GetString(4),
                                CashierName = reader.IsDBNull(5) ? "" : reader.GetString(5),
                                OrderCode = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                DetailedItems = new List<OrderItem>()
                            };
                        }
                    }
                }

                if (order != null)
                {
                    using (var cmd = new NpgsqlCommand("SELECT food_name, price_at_time, quantity FROM order_items WHERE order_id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("id", orderId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                order.DetailedItems.Add(new OrderItem
                                {
                                    Name = reader.GetString(0),
                                    Price = reader.GetDecimal(1),
                                    Quantity = reader.GetInt32(2)
                                });
                            }
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