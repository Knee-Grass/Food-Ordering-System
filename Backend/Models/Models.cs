using System;
using System.Collections.Generic;

namespace FoodOrderingSystem.Models
{
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
        public int CategoryId { get; set; } // Link to Category Table
        public string Category { get; set; } = string.Empty; // Category Name
        public bool IsAvailable { get; set; } = true;
        public int Quantity { get; set; }
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
    }
}