using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace GameMovement
{
    internal class Items
    {
        public int positionX;
        public int positionY;
        public Image item_image;
        public int height;
        public int width;
        public string name;

        Random range = new Random();
        int lifeTime = 200;
        public bool expired = false;

        public Items(int cellSize, int gridX, int gridY)
        {
            item_image = Image.FromFile("items/item_01.png");

            positionX = range.Next(0, gridX) * cellSize + cellSize / 4;
            positionY = range.Next(0, gridY) * cellSize + cellSize / 4;

            height = cellSize / 2;
            width = cellSize / 2;

            //height = cellSize;
            //width = cellSize ;
        }

        public void CheckLifeTime()
        {
            lifeTime--;

            if (lifeTime < 1)
            {
                expired = true;
            }
        }


    }
}
