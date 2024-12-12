using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace GameMovement
{
    internal class Obstacle
    {
        public int positionX;
        public int positionY;
        public Image obstacle_image;
        public int height;
        public int width;

        public Obstacle(int cellSize, int X, int Y)
        {
            obstacle_image = Image.FromFile("wall.png");

            positionX = cellSize * X;
            positionY = cellSize * Y;

            height = cellSize;
            width = cellSize;
        }
    }
}
