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
using System.Media; 

namespace GameMovement
{
    public partial class Form1 : Form
    {
        //Player
        Image player;
        List<string> playerMovement = new List<string>();
        int steps = 0;
        int slowDownFrameRate = 0;

        bool goLeft, goRight, goUp, goDown;

        int playerX;
        int playerY;
        int playerHeight = 80;
        int playerWidth= 80;
        int playerSpeed = 20;

        //NPC
        private NPC npc;

        // items variables
        int count = 0;

        List<string> item_location = new List<string>();
        List<Items> item_List = new List<Items>();
        int spawnTimeLimit = 35;
        int timeCounter = 0;
        Random rand = new Random();
        string[] itemNames = { "red sword", "medium amour", "green shoes", "gold lamp", "red potion", "fast sword", "instruction manual", "giant sword", "warm jacket", "wizards hat", "red bow and arrow", "red spear", "green potion", "heavy amour", "cursed axe", "gold ring", "purple ring"};

        List<Obstacle> obstacles = new List<Obstacle>();

        int cellSize; // Kích thước ô vuông
        int gridWidth, gridHeight; // Số lượng ô theo chiều ngang và dọc

        private bool isGameWon = false;
        private bool isGameLost = false;
        private bool Player_Moving = true;

        public Form1()
        {
            InitializeComponent();
            SetUp();
            PlayMusic("mission_impossible.wav");
        }
       
        int count_button_P = 0;
        
        private void KeyIsDown(object sender, KeyEventArgs e)
        {
          
            if (isGameWon ||  isGameLost || Player_Moving == false)
            {             
                return;
            }   
            else
            {
                if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left)
                {
                    goLeft = true;
                }

                else if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right)
                {
                    goRight = true;
                }


                else if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up)
                {
                    goUp = true;
                }

                else if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
                {
                    goDown = true;
                }
                else if (e.KeyCode == Keys.P)
                {
                    StopMusic("mission_impossible.wav");                   
                }
               
            }
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            if (isGameWon || isGameLost || Player_Moving == false  )
            {                
                return;
            }
            else
            {
                if (e.KeyCode == Keys.A || e.KeyCode == Keys.Left)
                {
                    goLeft = false;
                }

                else if (e.KeyCode == Keys.D || e.KeyCode == Keys.Right)
                {
                    goRight = false;
                }


                else if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up)
                {
                    goUp = false;
                }

                else if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down)
                {
                    goDown = false;
                }
               
                    else if (e.KeyCode == Keys.P )
                {
                    count_button_P++;
                    if (count_button_P % 2 == 0)
                    {
                        PlayMusic("mission_impossible.wav");
                        
                    }
                   
                 }
            }
        }
        private void Stop_character()
        {
            goLeft = false;
            goRight = false;
            goUp = false;
            goDown = false;
        }
        private void PlayMusic(string filepath)
        {
            //if (!Playing_music)
            
                SoundPlayer music = new SoundPlayer();
                music.SoundLocation = filepath;
                music.Play();
            
            
        }
        private void StopMusic(string filepath)
        {          
                 SoundPlayer music = new SoundPlayer();
                music.SoundLocation = filepath;
                music.Stop();    
                //Playing_music = false;
            
            
        }

        private void FormPaintEvent(object sender, PaintEventArgs e)
        {
            Graphics Canvas = e.Graphics;

            playerWidth = (cellSize - 20) / 2;
            playerHeight = cellSize - 20;

            //Ve luoi 
         //   DrawGrid(Canvas, cellSize);

            // Vẽ các đồ vật trên màn hình
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
            //DrawPathToPlayer(Canvas, cellSize);

            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DesignBlock();
        }

        // ve luoi
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

        private void TimerEvent(object sender, EventArgs e)
        {
            CheckCollision();

            if (timeCounter > 1)
            {
                timeCounter--;    // moi lan goi TimerEvent thi timeCounter se giam
            }
            else
            {
                MakeItems();   // neu giam = 0 thi se tao item moi
            }
            // 
            if (goLeft && playerX > 0 && !IsCollidingWithObstacle(playerX - playerSpeed, playerY))
            {
                playerX -= playerSpeed; // neu k va cham , se giam playerX  di playerSpeed
                AnimatePlayer(4, 7);   // cap nhat huong di chuyen theo su dieu khien
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
            CheckCollisionWithNPC();  // kiem tra da va cham voi NPC or not ?   
            //  Cập nhật màn hình.
            this.Invalidate();
        }


        private void SetUp()
        {
            this.BackgroundImage = Image.FromFile("bg.jpg");
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.DoubleBuffered = true;
            // giam hiệu ứng nhấp nháy khi vẽ các đối tượng đồ họa trong game
            /*
             Nếu ko dùng DoubleBuffered thì sẽ các object sẽ đc vẽ trực tiếp lên màng hình,
            có phần đc vẽ và hiển thij trước khi toàn bộ hoàn thành -> hiện tượng nhấp nháy do đc vẽ nhiều lần
            DÙNG DoubleBuffered  : sẽ đc vẽ trước vào Buffer (bộ đệm) thay vì vẽ trực tiếp lên màn hình.
             + sau khi vẽ xong sẽ được sao chép tất cả và hiển thị lên màn hình -> hình ảnh  mượt mà, hiệu suất tốt hơn
               nhưng nhược điểm là tốn bộ nhớ và tăng độ trễ 
             */

            // Load the player files to the list
            playerMovement = Directory.GetFiles("player", "*.png").ToList();
            player = Image.FromFile(playerMovement[0]);

            item_location = Directory.GetFiles("items", "*.png").ToList();
            
            gridWidth = this.ClientSize.Width / cellSize; // luoi chieu rong
            gridHeight = this.ClientSize.Height / cellSize; // lưới chiều cao
            
        }
        private void AnimatePlayer(int start, int end)
        {
            slowDownFrameRate += 1;  // Giam toc do di chuyen giua cac khung hinh

            if (slowDownFrameRate == 27 / playerSpeed)
            /* 27 : là hằng số tùy chỉnh để kiểm soát tốc độ hoạt ảnh, chứ không phải tốc độ khung hình của game.
            + Xác định tần suất thay đổi khung hình dựa trên tốc độ của nhân vật 
            +  Nếu đạt ngưỡng thì sẽ tăng STEP và reset slowDownFrameRate cho lần sau */
            {
                steps++;
                slowDownFrameRate = 0;
            }
            
            if (steps > end || steps < start)
            // nếu  câu if chạy thì sẽ đặt lại step để đảm bảo step nằm trong khoảng start => end
            {
                steps = start;
            }

            player = Image.FromFile(playerMovement[steps]);
        }



        //private List<Point> FindPathAStar(int startX, int startY, int targetX, int targetY)
        //{
        //    // Tạo lưới     

        //    // Danh sách đóng và mở
        //    List<Node> openList = new List<Node>();
        //    List<Node> closedList = new List<Node>();

        //    // Điểm bắt đầu và đích
        //    Node startNode = new Node(startX, startY);
        //    Node targetNode = new Node(targetX, targetY);

        //    openList.Add(startNode);

        //    // 4 hướng di chuyển
        //    int[] dx = { -1, 1, 0, 0 };
        //    int[] dy = { 0, 0, -1, 1 };

        //    while (openList.Count > 0)
        //    {
        //        // Lấy node có giá trị F nhỏ nhất
        //        Node currentNode = openList.OrderBy(n => n.F).First();
        //        openList.Remove(currentNode);
        //        closedList.Add(currentNode);

        //        // Nếu đã đến đích
        //        if (currentNode.X == targetNode.X && currentNode.Y == targetNode.Y)
        //        {
        //            List<Point> path = new List<Point>();
        //            Node pathNode = currentNode;
        //            while (pathNode != null)
        //            {
        //                path.Add(new Point(pathNode.X, pathNode.Y));
        //                pathNode = pathNode.Parent;
        //            }
        //            path.Reverse();
        //            return path;
        //        }

        //        // Kiểm tra các node lân cận
        //        for (int i = 0; i < 4; i++)
        //        {
        //            int newX = currentNode.X + dx[i];
        //            int newY = currentNode.Y + dy[i];

        //            // Kiểm tra tính hợp lệ
        //            if (newX < 0 || newY < 0 || newX >= gridWidth || newY >= gridHeight)
        //                continue;

        //            // Kiểm tra va chạm với vật cản
        //            if (IsCollidingWithObstacle(newX * cellSize, newY * cellSize))
        //                continue;

        //            // Tạo node mới
        //            Node neighbor = new Node(newX, newY);
        //            if (closedList.Any(n => n.X == neighbor.X && n.Y == neighbor.Y))
        //                continue;

        //            // Tính toán giá trị G, H, F
        //            neighbor.G = currentNode.G + 1;
        //            neighbor.H = Math.Abs(newX - targetNode.X) + Math.Abs(newY - targetNode.Y);

        //            // Kiểm tra nếu node này đã có trong openList với G thấp hơn
        //            Node existingNode = openList.FirstOrDefault(n => n.X == neighbor.X && n.Y == neighbor.Y);
        //            if (existingNode != null && existingNode.G <= neighbor.G)
        //                continue;

        //            neighbor.Parent = currentNode;
        //            openList.Add(neighbor);
        //        }
        //    }

        //    return new List<Point>(); // Không tìm được đường
        //}

        private List<Point> FindPathAStar(int startX, int startY, int targetX, int targetY)
        {
            // Thuat toan tim duọng di ngan nhat
            // Danh sách đóng và mở
            List<Node> openList = new List<Node>(); // cac node can xu li
            List<Node> closedList = new List<Node>(); // cac node da hoan thanh

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
            if (isGameWon || isGameLost) { return; }
                
            // Tính toán tọa độ lưới cho NPC và người chơi
            // Các điều chỉnh nhằm đưa toạ độ tìm kiếm vào giữa nhân vật
            // Đồng thời giúp việc tìm ô thực tế nhân vật đang ở chính xác hơn
            int npcGridX = (npc.X + npc.Height / 2) / cellSize; 
            int npcGridY = (npc.Y + npc.Width / 2) / cellSize;
            //int playerGridX = (playerX + playerWidth / 2) / cellSize;
            //int playerGridY = (playerY + playerHeight / 3) / cellSize;
            int playerGridX = (playerX + playerWidth / 2) / cellSize;
            int playerGridY = (playerY + playerHeight / 2) / cellSize;

            // Tìm đường đi ngắn nhất
            var path = FindPathAStar(npcGridX, npcGridY, playerGridX, playerGridY);

            // di chuyển NPC cùng với animation 
            npc.MoveTowards(cellSize, path);
            
        }  

        private void MakeItems()
        {
            int i = rand.Next(0, item_location.Count);
            // chọn ngẫu nhiên 1 vị trí or loại vp dựa trên item_location

            cellSize = this.ClientSize.Height / 10; 
            

            Items newItems = new Items(cellSize, gridWidth, gridHeight); 
            newItems.item_image = Image.FromFile(item_location[i]);
            newItems.name = itemNames[i];
            timeCounter = spawnTimeLimit;
            item_List.Add(newItems);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            cellSize = this.ClientSize.Height / 10;
            gridWidth = this.ClientSize.Width / cellSize;
            gridHeight = this.ClientSize.Height / cellSize;

            playerHeight = cellSize;
            playerWidth = cellSize /2;

            npc = new NPC(
                this.ClientSize.Width - cellSize,
                this.ClientSize.Height - cellSize,
                cellSize, cellSize,
                6,
                Directory.GetFiles("NPC", "*.png").ToList()
            );
        }

        private void MakeObstacle(int X, int Y)
        {
            cellSize = this.ClientSize.Height / 10;
            Obstacle newObstacle = new Obstacle(cellSize, X - 1, Y - 1);
            obstacles.Add(newObstacle);

            //UpdateGrid();
        }

        private void CheckCollision()     // Kiểm tra va chạm
        {
            foreach (Items item in item_List.ToList())
            {   // To.list để tránh lỗi khi xóa đi các vp trong Loop
                item.CheckLifeTime();
                if (item.expired)  // nếu vp hết thời gian
                {
                    item.item_image = null;  
                    item_List.Remove(item);   // xóa vp
                }

                bool collision = DetectCollisionForTouch(playerX, playerY, playerWidth, playerHeight, item.positionX, item.positionY, item.width, item.height);
                // phát hiện người chơi khi nhăt đồ
                if (collision)
                {  // cập nhật số lượng vp đã nhặt
                    count++;
                    lblCollected.Text = "Collected: " + count;
                    item.item_image = null;
                    item_List.Remove(item);

                    if (count == 10)
                    {
                        winGame();
                        ResetGame();
                        count = 0;
                    }
                }
            }
        }

        private bool DetectCollision(int object1X, int object1Y, int object1Width, int object1Height, int object2X, int object2Y, int object2Width, int object2Height)
        {   // phat hien va cham giua ng va vat the
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

        private void lblCollected_Click(object sender, EventArgs e)
        {

        }

        private bool DetectCollisionForTouch(int object1X, int object1Y, int object1Width, int object1Height, int object2X, int object2Y, int object2Width, int object2Height)
        {  // va cham giua ng choi va vat pham 
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

        //private void CheckCollisionWithNPC()
        //{
        //    if (DetectCollisionForTouch(npc.X, npc.Y, npc.Width, npc.Height, playerX, playerY, playerWidth, playerHeight))
        //    {
        //        isGameLost = true;
        //        npc.X = 0;
        //        npc.Y = 0;
        //        // Tạo form game over tùy chỉnh
        //        Form gameOverForm = new Form();
        //        gameOverForm.Text = "Game Over";
        //        gameOverForm.Size = new Size(300, 200);
        //        gameOverForm.StartPosition = FormStartPosition.CenterScreen;

        //        Label lblGameOver = new Label();
        //        lblGameOver.Text = "Game Over!";
        //        lblGameOver.Font = new Font("Arial", 16, FontStyle.Bold);
        //        lblGameOver.Dock = DockStyle.Top;
        //        lblGameOver.TextAlign = ContentAlignment.MiddleCenter;

        //        Button btnNextLevel = new Button();
        //        btnNextLevel.Text = "Chơi tiếp (Level 2)";
        //        btnNextLevel.DialogResult = DialogResult.Yes;

        //        Button btnReset = new Button();
        //        btnReset.Text = "Chơi lại";
        //        btnReset.DialogResult = DialogResult.Retry;

        //        Button btnHome = new Button();
        //        btnHome.Text = "Về Trang Chủ";
        //        btnHome.DialogResult = DialogResult.Abort;

        //        // Tạo panel để chứa các nút
        //        FlowLayoutPanel panel = new FlowLayoutPanel();
        //        panel.Dock = DockStyle.Fill;
        //        panel.FlowDirection = FlowDirection.TopDown;
        //        panel.Controls.Add(btnNextLevel);
        //        panel.Controls.Add(btnReset);
        //        panel.Controls.Add(btnHome);

        //        gameOverForm.Controls.Add(lblGameOver);
        //        gameOverForm.Controls.Add(panel);

        //        DialogResult result = gameOverForm.ShowDialog();

        //        //if (result == DialogResult.Yes)
        //        //{
        //        //    // Chuyển sang Level 2
        //        //    Form2 frm2 = new Form2();
        //        //    frm2.Show();
        //        //    this.Hide();
        //        //}
        //        if (result == DialogResult.Retry)
        //        {
        //            ResetGame();
        //        }
        //        else if (result == DialogResult.Abort)
        //        {
        //            frmMenu menu = new frmMenu();
        //            menu.Show();
        //            this.Close();
        //        }
        //    }
        //}
        private void CheckCollisionWithNPC()
        {
            if (DetectCollisionForTouch(npc.X, npc.Y, npc.Width, npc.Height, playerX, playerY, playerWidth, playerHeight))
            {
                isGameLost = true;
                npc.X = 0;
                npc.Y = 0;

                // Tạo form game over tùy chỉnh
                Form gameOverForm = new Form();
                gameOverForm.Text = "Game Over";
                gameOverForm.Size = new Size(300, 200);
                gameOverForm.StartPosition = FormStartPosition.CenterScreen;

                // Tiêu đề Game Over
                Label lblGameOver = new Label();
                lblGameOver.Text = "Game Over!";
                lblGameOver.Font = new Font("Arial", 16, FontStyle.Bold);
                lblGameOver.Dock = DockStyle.Top;
                lblGameOver.TextAlign = ContentAlignment.BottomCenter;
                //lblGameOver.

                // Tạo các nút
                //Button btnNextLevel = new Button();
                //btnNextLevel.Text = "Chơi tiếp (Level 2)";
                //btnNextLevel.Size = new Size(120, 40); // Kích thước cố định
                //btnNextLevel.DialogResult = DialogResult.Yes;

                Button btnReset = new Button();
                btnReset.Text = "Chơi lại";
                btnReset.Size = new Size(120, 40);
                btnReset.DialogResult = DialogResult.Retry;

                Button btnHome = new Button();
                btnHome.Text = "Trang Chủ";
                btnHome.Size = new Size(120, 40);
                btnHome.DialogResult = DialogResult.Abort;

                // Sử dụng TableLayoutPanel để căn giữa các nút và xếp ngang hàng
                TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
                tableLayoutPanel.Dock = DockStyle.Fill;
                tableLayoutPanel.ColumnCount = 3;
                tableLayoutPanel.RowCount = 2;

                // Định nghĩa cột và hàng
             
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50.0f));
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50.0f));

                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
             
          
                tableLayoutPanel.Controls.Add(btnReset, 0, 1);     // Cột 1, Hàng 1
                tableLayoutPanel.Controls.Add(btnHome, 1, 1);     // Cột 2, Hàng 1

               
                gameOverForm.Controls.Add(lblGameOver);
                gameOverForm.Controls.Add(tableLayoutPanel);

                // Hiển thị form và xử lý kết quả
                DialogResult result = gameOverForm.ShowDialog();

                if (result == DialogResult.Retry)
                {
                    ResetGame();
                }
                else if (result == DialogResult.Abort)
                {
                    frmMenu menu = new frmMenu();
                    menu.Show();
                    this.Close();
                    return;
                }
            }
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
            //for (int i = 13; i <= 15; i++)
            //{
            //    MakeObstacle(i, 6);
            //}

            //for (int i = 2; i <= 4; i++)
            //{
            //    MakeObstacle(i, 7);
            //}

            //for (int i = 6; i <= 9; i++)
            //{
            //    MakeObstacle(i, 7);
            //}

            //for (int i = 11; i <= 12; i = i + 2)
            //{
            //    MakeObstacle(i, 7);
            //}

            //MakeObstacle(7, 8);
            //MakeObstacle(11, 8);
            //MakeObstacle(13, 7);

            //for (int i = 2; i <= 5; i++)
            //{
            //    MakeObstacle(i, 9);
            //}

            MakeObstacle(6, 10);

            for (int i = 1; i <= 4; i++)
            {
                MakeObstacle(17, i);
            }

            MakeObstacle(16, 4);

            for (int i = 9; i <= 14; i++)
            {
                MakeObstacle(i, 9);
            }

            MakeObstacle(16, 8);
            MakeObstacle(16, 9);
            MakeObstacle(16, 10);

            MakeObstacle(19, 5);
            MakeObstacle(19, 6);
            MakeObstacle(18, 6);

            MakeObstacle(18, 8);
            MakeObstacle(18, 9);

            for (int i = 1; i <= 10; i++)
            {
                MakeObstacle(20, i);
            }
        }

        private void ResetGame()
        {
            isGameLost = false;
            isGameWon = false;
            Player_Moving = true;
            Stop_character();

            count = 0;
            // Reset vị trí của người chơi
            playerX = 0;
            playerY = 0;
            StopMusic("mission_impossible.wav");
            PlayMusic("mission_impossible.wav");
            // Reset vị trí của NPC
            npc.X = this.ClientSize.Width - npc.Width;
            npc.Y = this.ClientSize.Height - npc.Height;

            // Xóa danh sách vật phẩm
            item_List.Clear();

            // Xóa danh sách vật cản (nếu muốn)
            obstacles.Clear();
            Form1_Load(null, null); // Nạp lại vật cản và các cài đặt ban đầu

            // Reset các biến khác
            steps = 0;
            timeCounter = 0;

            this.Invalidate(); // Vẽ lại giao diện
        }

        void winGame()
        {
            isGameWon = true;
            Player_Moving = false;

            // Tạo form chiến thắng tùy chỉnh
            Form winForm = new Form();
            winForm.Text = "Chiến Thắng";
            winForm.Size = new Size(300, 200);
            winForm.StartPosition = FormStartPosition.CenterScreen;

            Label lblWinGame = new Label();
            lblWinGame.Text = "Chúc Mừng Bạn Đã Chiến Thắng!";
            lblWinGame.Font = new Font("Arial", 16, FontStyle.Bold);
            lblWinGame.Dock = DockStyle.Top;
            lblWinGame.TextAlign = ContentAlignment.MiddleCenter;

            Button btnNextLevel = new Button();
            btnNextLevel.Text = "Chơi tiếp (Level 2)";
            btnNextLevel.DialogResult = DialogResult.Yes;
            btnNextLevel.TextAlign = ContentAlignment.MiddleCenter;

            Button btnReset = new Button();
            btnReset.Text = "Chơi lại";
            btnReset.DialogResult = DialogResult.Retry;
            btnReset.TextAlign = ContentAlignment.MiddleCenter;

            Button btnHome = new Button();
            btnHome.Text = "Trang Chủ";
            btnHome.DialogResult = DialogResult.Abort;
            btnHome.TextAlign = ContentAlignment.MiddleCenter;  

            // Tạo panel để chứa các nút
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.FlowDirection = FlowDirection.LeftToRight;
            panel.Controls.Add(btnNextLevel);
            panel.Controls.Add(btnReset);
            panel.Controls.Add(btnHome);

            winForm.Controls.Add(lblWinGame);
            winForm.Controls.Add(panel);

            DialogResult result = winForm.ShowDialog();

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
                // Về trang chủ
                frmMenu menu = new frmMenu();
                menu.Show();
                this.Close();
            }
        }
        
    }
}

/* tình hình là khi chơi lại thì các nút bị dính vào nhau , nên xem lại phần di chuyển keyUp , keyDown
  nên setup phần reset lại tất cả các nút khi reset game
 */