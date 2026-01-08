using System.Drawing;

namespace FoodOrderingSystem.Backend.Config
{
    public static class DbConfig
    {
        // IMPORTANT: Change 'Password' to your actual PostgreSQL password if it's different.
        public static string ConnectionString = "Host=localhost;Username=postgres;Password=jeypi04.;Database=foodordering";
        
        // Colors
        public static Color PrimaryColor = Color.FromArgb(255, 87, 34);    // Deep Orange
        public static Color DarkColor = Color.FromArgb(38, 50, 56);        // Blue Grey
        public static Color LightColor = Color.FromArgb(245, 245, 245);    // Off-white
        public static Color AccentColor = Color.FromArgb(76, 175, 80);     // Green
        public static Color TextColor = Color.FromArgb(33, 33, 33);        // Almost Black
        
        // Fonts
        public static Font MainFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        public static Font HeaderFont = new Font("Segoe UI", 24F, FontStyle.Bold);
        public static Font SubHeaderFont = new Font("Segoe UI", 14F, FontStyle.Bold);
    }
}