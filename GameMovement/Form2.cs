using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace GameMovement
{
    public partial class Form2 : Form
    {

        Image player;
        List<string> playerMovement = new List<string>();
        int steps = 0;
        int slowDownFrameRate = 0;

        bool goLeft, goRight, goUp, goDown;

        int playerX;
        int playerY;
        int playerHeight = 80;
        int playerWidth = 80;
        int playerSpeed = 20;

        //NPC
        private NPC npc;

        // items variables

        List<string> item_location = new List<string>();
        List<Items> item_List = new List<Items>();
        int spawnTimeLimit = 35;
        int timeCounter = 0;
        Random rand = new Random();
        string[] itemNames = { "red sword", "medium amour", "green shoes", "gold lamp", "red potion", "fast sword", "instruction manual", "giant sword", "warm jacket", "wizards hat", "red bow and arrow", "red spear", "green potion", "heavy amour", "cursed axe", "gold ring", "purple ring" };

        List<Obstacle> obstacles = new List<Obstacle>();

        int cellSize; // Kích thước ô vuông
        int gridWidth, gridHeight; // Số lượng ô theo chiều ngang và dọc
        int count = 0;
        private void TimerEvent(object sender, EventArgs e)
        {
            CheckCollision();            
            if (timeCounter > 1)
            {
                timeCounter--;
            }
            else
            {
                MakeItems();
            }

            if (goLeft && playerX > 0 && !IsCollidingWithObstacle(playerX - playerSpeed, playerY))
            {
                playerX -= playerSpeed;
                AnimatePlayer(4, 7);
            }
            else if (goRight && playerX + playerWidth < this.ClientSize.Width && !IsCollidingWithObstacle(playerX + playerSpeed, playerY))
            {
                playerX += playerSpeed;
                AnimatePlayer(8, 11);
            }
            else if (goUp && playerY > 0 && !IsCollidingWithObstacle(playerX, playerY - playerSpeed))
            {
                playerY -= playerSpeed;
                AnimatePlayer(12, 15);
            }
            else if (goDown && playerY + playerHeight < this.ClientSize.Height &&
                     !IsCollidingWithObstacle(playerX, playerY + playerSpeed))
            {
                playerY += playerSpeed;
                AnimatePlayer(0, 3);
            }
            else
            {
                AnimatePlayer(0, 0);
            }
            MoveNPC();
            CheckCollisionWithNPC();
            this.Invalidate();
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left)
            {
                goLeft = true;
            }

            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right)
            {
                goRight = true;
            }
           

            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up)
            {
                goUp = true;
            }

            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
            {
                goDown = true;
            }
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left)
            {
                goLeft = false;
            }

            if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right)
            {
                goRight = false;
            }
          

            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up)
            {
                goUp = false;
            }

            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
            {
                goDown = false;
            }
        }

        public Form2()
        {
            InitializeComponent();
            SetUp();
        }
        private void FormPaintEvent(object sender, PaintEventArgs e)
        {
            Graphics Canvas = e.Graphics;


            playerWidth = (cellSize - 20) / 2;
            playerHeight = cellSize - 20;

            //Ve luoi 
          //  DrawGrid(Canvas, cellSize);

            if (item_List != null)
            {
                foreach (Items item in item_List)
                {
                    Canvas.DrawImage(item.item_image, item.positionX, item.positionY, item.width, item.height);
                }
            }

            // Ve nguoi choi
            Canvas.DrawImage(player, playerX, playerY, playerWidth, playerHeight);


            // Ve NPC
            Canvas.DrawImage(npc.Image, npc.X, npc.Y, npc.Width, npc.Height);

            // Ve vat can
            foreach (Obstacle obstacle in obstacles)
            {
                Canvas.DrawImage(obstacle.obstacle_image, obstacle.positionX, obstacle.positionY, obstacle.width, obstacle.height);
            }

            // Vẽ đường đi từ NPC đến player
            DrawPathToPlayer(Canvas, cellSize);

        }

        private void DrawGrid(Graphics g, int cellSize)
        {
            Pen gridPen = new Pen(Color.LightGray, 1);
            // Vẽ các đường dọc
            for (int x = 0; x <= this.ClientSize.Width; x += cellSize)
            {
                g.DrawLine(gridPen, x, 0, x, this.ClientSize.Height);
            }

            // Vẽ các đường ngang
            for (int y = 0; y <= this.ClientSize.Height; y += cellSize)
            {
                g.DrawLine(gridPen, 0, y, this.ClientSize.Width, y);
            }
        }

        private void DrawPathToPlayer(Graphics g, int cellSize)
        {
            // Ve duong di tu NPC toi player
            int npcGridX = (npc.X + npc.Height / 2) / cellSize;
            int npcGridY = (npc.Y + npc.Width / 2) / cellSize;
            int playerGridX = (playerX + playerWidth / 2) / cellSize;
            int playerGridY = (playerY + playerHeight / 2) / cellSize;

            var path = FindPathAStar(npcGridX, npcGridY, playerGridX, playerGridY);

            if (path.Count > 1)
            {
                Pen pathPen = new Pen(Color.Red, 2);
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Point p1 = new Point(path[i].X * cellSize + cellSize / 2, path[i].Y * cellSize + cellSize / 2);
                    Point p2 = new Point(path[i + 1].X * cellSize + cellSize / 2, path[i + 1].Y * cellSize + cellSize / 2);
                    g.DrawLine(pathPen, p1, p2);
                }
            }
        }

        private void SetUp()
        {
            this.BackgroundImage = Image.FromFile("bg.jpg");
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.DoubleBuffered = true;

            // Load the player files to the list
            playerMovement = Directory.GetFiles("player", "*.png").ToList();
            player = Image.FromFile(playerMovement[0]);

            item_location = Directory.GetFiles("items", "*.png").ToList();

            // Đặt vị trí ban đầu cho player
            playerX = 0;
            playerY = 0;

            // Thiết lập kích thước ô
            cellSize = this.ClientSize.Height / 18;
            gridWidth = this.ClientSize.Width / cellSize;
            gridHeight = this.ClientSize.Height / cellSize;

            // Khởi tạo NPC
            npc = new NPC(
                this.ClientSize.Width - cellSize,
                this.ClientSize.Height - cellSize,
                cellSize, cellSize,
                6,
                Directory.GetFiles("NPC", "*.png").ToList()
            );
        }

        private void AnimatePlayer(int start, int end)
        {
            slowDownFrameRate += 1;

            if (slowDownFrameRate == 27 / playerSpeed)
            {
                steps++;
                slowDownFrameRate = 0;
            }

            if (steps > end || steps < start)
            {
                steps = start;
            }

            player = Image.FromFile(playerMovement[steps]);
        }

        private void Form2_Resize(object sender, EventArgs e)
        {
            cellSize = this.ClientSize.Height / 18;
            gridWidth = this.ClientSize.Width / cellSize;
            gridHeight = this.ClientSize.Height / cellSize;

            playerHeight = cellSize;
            playerWidth = cellSize / 2;

            npc = new NPC(
                this.ClientSize.Width - cellSize,
                this.ClientSize.Height - cellSize,
                cellSize, cellSize,
                6,
                Directory.GetFiles("NPC", "*.png").ToList()
            );
        }

        private List<Point> FindPathAStar(int startX, int startY, int targetX, int targetY)
        {
            // Danh sách đóng và mở
            List<Node> openList = new List<Node>();
            List<Node> closedList = new List<Node>();

            // Điểm bắt đầu và đích
            Node startNode = new Node(startX, startY);
            Node targetNode = new Node(targetX, targetY);

            openList.Add(startNode);

            // 4 hướng di chuyển (trái, phải, lên, xuống)
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            while (openList.Count > 0)
            {
                // Lấy node có giá trị F nhỏ nhất
                Node currentNode = openList.OrderBy(n => n.F).First();
                openList.Remove(currentNode);
                closedList.Add(currentNode);

                // Nếu đã đến đích
                if (currentNode.X == targetNode.X && currentNode.Y == targetNode.Y)
                {
                    List<Point> path = new List<Point>();
                    Node pathNode = currentNode;
                    while (pathNode != null)
                    {
                        path.Add(new Point(pathNode.X, pathNode.Y));
                        pathNode = pathNode.Parent;
                    }
                    path.Reverse();
                    return path;
                }

                // Kiểm tra các node lân cận
                for (int i = 0; i < 4; i++)
                {
                    int newX = currentNode.X + dx[i];
                    int newY = currentNode.Y + dy[i];

                    // Kiểm tra tính hợp lệ
                    if (newX < 0 || newY < 0 || newX >= gridWidth || newY >= gridHeight)
                        continue;

                    // Kiểm tra va chạm với vật cản
                    if (IsCollidingWithObstacle(newX * cellSize, newY * cellSize))
                        continue;

                    // Tạo node mới
                    Node neighbor = new Node(newX, newY);
                    if (closedList.Any(n => n.X == neighbor.X && n.Y == neighbor.Y))
                        continue;

                    // Tính toán giá trị G, H, F
                    neighbor.G = currentNode.G + 1;
                    neighbor.H = Math.Abs(newX - targetNode.X) + Math.Abs(newY - targetNode.Y);

                    // Kiểm tra nếu node này đã có trong openList với G thấp hơn
                    Node existingNode = openList.FirstOrDefault(n => n.X == neighbor.X && n.Y == neighbor.Y);
                    if (existingNode != null && existingNode.G <= neighbor.G)
                        continue;

                    neighbor.Parent = currentNode;
                    openList.Add(neighbor);
                }
            }

            return new List<Point>(); // Không tìm được đường
        }

        private void MoveNPC()
        {
            // Tính toán tọa độ lưới cho NPC và người chơi
            // Các điều chỉnh nhằm đưa toạ độ tìm kiếm vào giữa nhân vật
            // Đồng thời giúp việc tìm ô thực tế nhân vật đang ở chính xác hơn
            int npcGridX = (npc.X + npc.Height / 2) / cellSize;
            int npcGridY = (npc.Y + npc.Width / 2) / cellSize;
            //int playerGridX = (playerX + playerWidth / 2) / cellSize;
            //int playerGridY = (playerY + playerHeight / 3) / cellSize;
            int playerGridX = playerX / cellSize;
            int playerGridY = playerY / cellSize;

            // Tìm đường đi ngắn nhất
            var path = FindPathAStar(npcGridX, npcGridY, playerGridX, playerGridY);

            // di chuyển NPC cùng với animation 
            npc.MoveTowards(cellSize, path);

        }
  

     
        private void MakeItems()
        {
            int i = rand.Next(0, item_location.Count);

            cellSize = this.ClientSize.Height / 18;


            Items newItems = new Items(cellSize, gridWidth, gridHeight);
            newItems.item_image = Image.FromFile(item_location[i]);
            newItems.name = itemNames[i];
            timeCounter = spawnTimeLimit;
            item_List.Add(newItems);
        }

        private void MakeObstacle(int X, int Y)
        {
            cellSize = this.ClientSize.Height / 18;
            Obstacle newObstacle = new Obstacle(cellSize, X - 1, Y - 1);
            obstacles.Add(newObstacle);

            //UpdateGrid();
        }
        void DesignBlock()
        {
            MakeObstacle(4, 1);
            MakeObstacle(7, 1);
            MakeObstacle(11, 1);
            for (int i = 2; i <= 4; i = i + 2)
            {
                MakeObstacle(i, 2);
            }
            for (int i = 9; i <= 13; i = i + 2)
            {
                MakeObstacle(i, 2);
            }
            MakeObstacle(14, 2);
            // MakeObstacle(15, 2);
            MakeObstacle(1, 3);

            for (int i = 4; i <= 8; i = i + 2)
            {
                MakeObstacle(i, 3);
            }

            MakeObstacle(3, 4);

            for (int i = 6; i <= 10; i = i + 2)
            {
                MakeObstacle(i, 4);
            }

            for (int i = 11; i <= 14; i++)
            {
                MakeObstacle(i, 4);
            }

            //MakeObstacle(2, 5);
            //MakeObstacle(5, 5);
            //MakeObstacle(8, 5);
            //MakeObstacle(10, 5);

            MakeObstacle(4, 6);
            MakeObstacle(7, 6);
            MakeObstacle(11, 6);
            for (int i = 13; i <= 15; i++)
            {
                MakeObstacle(i, 6);
            }

            for (int i = 2; i <= 4; i++)
            {
                MakeObstacle(i, 7);
            }

            for (int i = 6; i <= 9; i++)
            {
                MakeObstacle(i, 7);
            }

            for (int i = 11; i <= 12; i = i + 2)
            {
                MakeObstacle(i, 7);
            }

            MakeObstacle(7, 8);
            MakeObstacle(11, 8);
            MakeObstacle(13, 7);

            for (int i = 2; i <= 5; i++)
            {
                MakeObstacle(i, 9);
            }

            //MakeObstacle(6, 10);

            //for (int i = 1; i <= 4; i++)
            //{
            //    MakeObstacle(17, i);
            //}

            //MakeObstacle(16, 4);

            //for (int i = 9; i <= 14; i++)
            //{
            //    MakeObstacle(i, 9);
            //}

            //MakeObstacle(16, 8);
            //MakeObstacle(16, 9);
            //MakeObstacle(16, 10);

            //MakeObstacle(19, 5);
            //MakeObstacle(19, 6);
            //MakeObstacle(18, 6);

            //MakeObstacle(18, 8);
            //MakeObstacle(18, 9);

            //for (int i = 1; i <= 10; i++)
            //{
            //    MakeObstacle(20, i);
            //}
        }
        private void CheckCollision()
        {
            foreach (Items item in item_List.ToList())
            {
                item.CheckLifeTime();
                if (item.expired)
                {
                    item.item_image = null;
                    item_List.Remove(item);
                }

                bool collision = DetectCollisionForTouch(playerX, playerY, playerWidth, playerHeight, item.positionX, item.positionY, item.width, item.height);

                if (collision)
                {
                    count++;
                    lblCollected.Text = "Collected: " + count;
                    item.item_image = null;
                    item_List.Remove(item);

                    if (count == 15)
                    {
                        //winGame();
                        ResetGame();
                        count = 0;
                    }
                }
            }
        }

        private bool DetectCollision(int object1X, int object1Y, int object1Width, int object1Height, int object2X, int object2Y, int object2Width, int object2Height)
        {
            if (object1X + object1Width < object2X + 30 || object1X + 20 > object2X + object2Width ||
                object1Y + object1Height < object2Y + 30 || object1Y + 30 > object2Y + object2Height)
            {
                return false; // Khong co va cham
            }
            else
            {
                return true; // Tim thay va cham
            }
        }

        private bool DetectCollisionForTouch(int object1X, int object1Y, int object1Width, int object1Height, int object2X, int object2Y, int object2Width, int object2Height)
        {
            if (object1X + object1Width < object2X || object1X > object2X + object2Width ||
                object1Y + object1Height < object2Y || object1Y > object2Y + object2Height)
            {
                return false; // Khong co va cham
            }
            else
            {
                return true; // Tim thay va cham
            }
        }

        private bool IsCollidingWithObstacle(int newX, int newY)
        {
            foreach (Obstacle obstacle in obstacles)
            {
                bool collision = DetectCollision(newX, newY, playerWidth, playerHeight, obstacle.positionX, obstacle.positionY, obstacle.width, obstacle.height);
                if (collision)
                {
                    return true; // Va chạm với vật thể
                }
            }
            return false; // Không va chạm
        }

        private void CheckCollisionWithNPC()
        {
            if (DetectCollisionForTouch(npc.X, npc.Y, npc.Width, npc.Height, playerX, playerY, playerWidth, playerHeight))
            {
                npc.X = 0;
                npc.Y = 0;
                // Tạo form game over tùy chỉnh
                Form gameOverForm = new Form();
                gameOverForm.Text = "Game Over";
                gameOverForm.Size = new Size(300, 200);
                gameOverForm.StartPosition = FormStartPosition.CenterScreen;

                Label lblGameOver = new Label();
                lblGameOver.Text = "Game Over!";
                lblGameOver.Font = new Font("Arial", 16, FontStyle.Bold);
                lblGameOver.Dock = DockStyle.Top;
                lblGameOver.TextAlign = ContentAlignment.MiddleCenter;

                Button btnNextLevel = new Button();
                btnNextLevel.Text = "Chơi tiếp (Level 2)";
                btnNextLevel.DialogResult = DialogResult.Yes;

                Button btnReset = new Button();
                btnReset.Text = "Chơi lại";
                btnReset.DialogResult = DialogResult.Retry;

                Button btnHome = new Button();
                btnHome.Text = "Về Trang Chủ";
                btnHome.DialogResult = DialogResult.Abort;

                // Tạo panel để chứa các nút
                FlowLayoutPanel panel = new FlowLayoutPanel();
                panel.Dock = DockStyle.Fill;
                panel.FlowDirection = FlowDirection.TopDown;
                panel.Controls.Add(btnNextLevel);
                panel.Controls.Add(btnReset);
                panel.Controls.Add(btnHome);

                gameOverForm.Controls.Add(lblGameOver);
                gameOverForm.Controls.Add(panel);

                DialogResult result = gameOverForm.ShowDialog();

                if (result == DialogResult.Yes)
                {
                    // Chuyển sang Level 2
                    Form2 frm2 = new Form2();
                    frm2.Show();
                    this.Hide();
                }
                else if (result == DialogResult.Retry)
                {
                    ResetGame();
                }
                else if (result == DialogResult.Abort)
                {
                    // Giả sử bạn có Form Menu
                    frmMenu menu = new frmMenu();
                    menu.Show();
                    this.Close();
                }
            }
        }

        private void ResetGame()
        {
            // Reset vị trí của người chơi
            playerX = 0;
            playerY = 0;
            

            // Reset vị trí của NPC
            npc.X = this.ClientSize.Width - npc.Width;
            npc.Y = this.ClientSize.Height - npc.Height;

            // Xóa danh sách vật phẩm
            item_List.Clear();

            // Xóa danh sách vật cản (nếu muốn)
            obstacles.Clear();
            Form2_Load(null, null); // Nạp lại vật cản và các cài đặt ban đầu

            // Reset các biến khác
            steps = 0;
            timeCounter = 0;

            this.Invalidate(); // Vẽ lại giao diện
        }



        private void Form2_Load(object sender, EventArgs e)
        {
            SetUp();
        }
    }
}
