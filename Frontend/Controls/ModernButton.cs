using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FoodOrderingSystem.Controls
{
    public class ModernButton : Button
    {
        public ModernButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Cursor = Cursors.Hand;
            this.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.ForeColor = Color.White;
            this.Size = new Size(150, 45);
            this.Paint += ModernButton_Paint;
        }

        // Fix CS8622: Use object? sender
        private void ModernButton_Paint(object? sender, PaintEventArgs e)
        {
            GraphicsPath graphicsPath = new GraphicsPath();
            Rectangle rect = this.ClientRectangle;
            rect.Width--; rect.Height--;
            int radius = 10;
            
            graphicsPath.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            graphicsPath.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
            graphicsPath.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
            graphicsPath.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
            graphicsPath.CloseAllFigures();

            this.Region = new Region(graphicsPath);
        }
    }
}