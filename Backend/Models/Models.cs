using System;
using System.Collections.Generic;

namespace FoodOrderingSystem.Models
{
    // RUBRIC: Custom Exceptions (Section 7)
    public class InsufficientStockException : Exception
    {
        public string ItemName { get; }
        public int Remaining { get; }

        public InsufficientStockException(string name, int remaining)
            : base($"Stock Error: '{name}' only has {remaining} left!")
        {
            ItemName = name;
            Remaining = remaining;
        }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public override string ToString() => Name; 
    }

    public class FoodItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; } 
        public string Category { get; set; } = string.Empty; 
        public bool IsAvailable { get; set; } = true;
        public int Quantity { get; set; }
        public string ImageData { get; set; } = string.Empty; 
    }

    public class CartItem
    {
        public FoodItem Food { get; set; } = new FoodItem(); 
        public int Quantity { get; set; }
        public decimal TotalPrice => Food.Price * Quantity;
    }

    public class User
    {
        public int Id { get; set; } 
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }

    public class OrderItem
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }

    public class OrderRecord
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Items { get; set; } = string.Empty; 
        public List<OrderItem> DetailedItems { get; set; } = new List<OrderItem>();
        public string Status { get; set; } = "Pending";
        public string CashierName { get; set; } = "Unknown";
        public string OrderCode { get; set; } = "";
    }
}