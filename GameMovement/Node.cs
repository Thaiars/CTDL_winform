using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameMovement
{
    public class Node
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int G { get; set; } // Chi phí từ điểm bắt đầu
        public int H { get; set; } // Chi phí ước tính đến đích
        public int F => G + H; // Tổng chi phí
        public Node Parent { get; set; }

        public Node(int x, int y)
        {
            X = x;
            Y = y;
            G = 0;
            H = 0;
            Parent = null;
        }
    }
}
