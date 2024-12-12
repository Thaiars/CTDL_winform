using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameMovement
{
    public class NPC
    {
        public Image Image { get; private set; }
        public List<string> MovementFrames { get; private set; }
        public int X { get; set; }
        public int Y{ get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Speed { get; private set; }
        private int currentFrame;
        private int slowDownFrameRate;

        public NPC(int x, int y, int width, int height, int speed, List<string> movementFrames)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Speed = speed;
            MovementFrames = movementFrames;
            Image = Image.FromFile(MovementFrames[0]);
            currentFrame = 0;
            slowDownFrameRate = 0;
        }

        public void Animate(int start, int end)
        {
            slowDownFrameRate++;

            if (slowDownFrameRate == 27 / Speed)
            {
                currentFrame++;
                slowDownFrameRate = 0;
            }

            if (currentFrame > end || currentFrame < start)
            {
                currentFrame = start;
            }

            Image = Image.FromFile(MovementFrames[currentFrame]);
        }

        public void MoveTowards(int cellSize, List<Point> path)
        {
            if (path.Count > 1)
            {
                Point nextStep = path[1];
                int targetPixelX = nextStep.X * cellSize;
                int targetPixelY = nextStep.Y * cellSize;

                if (X < targetPixelX)
                {
                    X += Math.Min(Speed, targetPixelX - X);
                    //X += Speed;
                    Animate(8, 11);
                }
                else if (X > targetPixelX)
                {
                    X -= Math.Min(Speed, X - targetPixelX);
                    //X -= Speed;
                    Animate(12, 15);
                }
                if (Y < targetPixelY)
                {
                    Y += Math.Min(Speed, targetPixelY - Y);
                    //Y += Speed;
                    Animate(0, 3);
                }
                else if (Y > targetPixelY)
                {
                    Y -= Math.Min(Speed, Y - targetPixelY);
                    //Y -= Speed;
                    Animate(4, 7);
                }
            }
        }

    }
}
