using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace ImagePuzzle
{
    internal class PuzzleForm : Form
    {       
        private Image puzzleImage;        
        private int rows = 4;
        private int cols = 4;
        private int timeLimit = 60;
        private int pieces = 0;
        private int missingPiece;
        private Tile[] tiles;
        private Size imgSize;
        OpenFileDialog open;
        ResizeForm settings;
        public Thread solver;
        public System.Timers.Timer buttonTimer;
        int timeLeft;        
        public ToolStripMenuItem solveButton;

        public PuzzleForm()
        {
            solver = new Thread(solvePuzzle);
            
            buttonTimer = new System.Timers.Timer();
            buttonTimer.Interval = 1000;
            buttonTimer.AutoReset = true;
            buttonTimer.Elapsed += UpdateButton;

            puzzleImage = Image.FromFile("background.jpg");
            Text = "Slider Puzzle";

            MenuStrip menu = new MenuStrip();
            ToolStripMenuItem imgButton = new ToolStripMenuItem();
            imgButton.Text = "Image";
            imgButton.Click += ChooseFile;
            menu.Items.Add(imgButton);

            ToolStripMenuItem sizeButton = new ToolStripMenuItem();
            sizeButton.Text = "Settings";
            sizeButton.Click += ShowSettings;
            menu.Items.Add(sizeButton);

            ToolStripMenuItem shuffleButton = new ToolStripMenuItem();
            shuffleButton.Text = "Shuffle";
            shuffleButton.Click += ShuffleClick;
            menu.Items.Add(shuffleButton);

            ToolStripMenuItem aboutButton = new ToolStripMenuItem();
            aboutButton.Text = "About";
            aboutButton.Click += AboutClick;
            menu.Items.Add(aboutButton);
            
            solveButton = new ToolStripMenuItem();
            solveButton.Text = "Solve";
            solveButton.Click += Solve;
            solveButton.Alignment = ToolStripItemAlignment.Right;
            menu.Items.Add(solveButton);

            menu.Dock = DockStyle.Top;
            Controls.Add(menu);

            open = new OpenFileDialog();
            open.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

            settings = new ResizeForm(this);
            SetBackgroundImage();
        }

        private void AboutClick(object sender, EventArgs e)
        {
            MessageBox.Show("Written by Andrew Isaac for COMP 585\nUses IDA* algorithm to optimally solve puzzle.", "About");
        }

        private void ShuffleClick(object sender, EventArgs e)
        {
            SetBackgroundImage();
        }

        private void ShowSettings(object sender, EventArgs e)
        {            
            settings.ShowDialog();
        }

        public int Rows
        {
            get { return rows; }
            set { rows = value; }
        }

        public int Cols
        {
            get { return cols; }
            set { cols = value; }
        }

        public int TimeLimit
        {
            get { return timeLimit; }
            set { timeLimit = value; }
        }

        private void ChooseFile(object sender, EventArgs e)
        {
            if (open.ShowDialog() == DialogResult.OK && open.CheckFileExists)
            {
                puzzleImage = Image.FromStream(open.OpenFile());
                SetBackgroundImage();
            }            
        }

        public void SetBackgroundImage()
        {
            solver.Abort();
            buttonTimer.Stop();
            solveButton.Text = "Solve";

            for (int i = 0; i < pieces; i++)
            {
                tiles[i].Parent = null;
            }

            Bitmap bmpImage = new Bitmap(puzzleImage, ResizeImage());
            Rectangle cropArea = new Rectangle(0, 0, bmpImage.Width / cols * cols, bmpImage.Height / rows * rows);
            bmpImage = bmpImage.Clone(cropArea, bmpImage.PixelFormat);            
            
            imgSize = bmpImage.Size;
            Size = new Size(imgSize.Width + 20, imgSize.Height + 62);
            pieces = rows * cols - 1;
            missingPiece = pieces; // last square is empty initially
            tiles = new Tile[pieces];

            for (int i = 0; i < pieces; i++)
            {
                tiles[i] = new Tile(this);
                tiles[i].Parent = this;
                tiles[i].Row = i / cols;
                tiles[i].Col = i % cols;
                tiles[i].Location = new Point(i % cols * (imgSize.Width / cols) + 2, i / cols * (imgSize.Height / rows) + 22); 
                tiles[i].Size = new Size(imgSize.Width / cols, imgSize.Height / rows);
                cropArea = new Rectangle(i % cols * (imgSize.Width / cols), i / cols * (imgSize.Height / rows), imgSize.Width / cols, imgSize.Height / rows);
                tiles[i].BackgroundImage = bmpImage.Clone(cropArea, bmpImage.PixelFormat);
            }

            Random rand = new Random();

            for (int i = 0; i < 200; i++)
            {
                MovePiece(tiles[rand.Next(pieces)]);
            }
        }

        // Max image size is 800 x 600 
        private Size ResizeImage()
        {
            Size finalSize = puzzleImage.Size;

            if (finalSize.Width > 800)
            {
                float resizePercent = (float)800 / puzzleImage.Width;
                finalSize.Width = 800;
                finalSize.Height = (int)(puzzleImage.Height * resizePercent);
            }

            if (finalSize.Height > 600)
            {
                float resizePercent = (float)600 / finalSize.Height;
                finalSize.Height = 600;
                finalSize.Width = (int)(finalSize.Width * resizePercent);
            }

            return finalSize;
        }

        public void MovePiece(Tile t)
        {
            int row = t.Row;
            int col = t.Col;

            if (t.Row == missingPiece / cols)
            {
                // Move all pieces in same row with column less than missing piece to the right
                if (t.Col < missingPiece % cols)
                {
                    for (int i = 0; i < pieces; i++)
                    {
                        if (t.Row == tiles[i].Row && tiles[i].Col < missingPiece % cols && tiles[i].Col >= t.Col)
                        {
                            tiles[i].Col++;
                            tiles[i].Location = new Point(tiles[i].Location.X + imgSize.Width / cols, tiles[i].Location.Y);
                        }
                    }
                }
                
                // Move all pieces in same row with column greater than missing piece to the left
                else 
                {
                    for (int i = 0; i < pieces; i++)
                    {
                        if (t.Row == tiles[i].Row && tiles[i].Col > missingPiece % cols && tiles[i].Col <= t.Col)
                        {
                            tiles[i].Col--;
                            tiles[i].Location = new Point(tiles[i].Location.X - imgSize.Width / cols, tiles[i].Location.Y);
                        }
                    }
                }

                missingPiece = row * cols + col;
            }

            else if (t.Col == missingPiece % cols)
            {
                // Move all pieces in same col with row less than missing piece down
                if (t.Row < missingPiece / cols)
                {
                    for (int i = 0; i < pieces; i++)
                    {
                        if (t.Col == tiles[i].Col && tiles[i].Row < missingPiece / cols && tiles[i].Row >= t.Row)
                        {
                            tiles[i].Row++;
                            tiles[i].Location = new Point(tiles[i].Location.X, tiles[i].Location.Y + imgSize.Height / rows);
                        }
                    }
                }

                // Move all pieces in same col with row greater than missing piece up
                else
                {
                    for (int i = 0; i < pieces; i++)
                    {
                        if (t.Col == tiles[i].Col && tiles[i].Row > missingPiece / cols && tiles[i].Row <= t.Row)
                        {
                            tiles[i].Row--;
                            tiles[i].Location = new Point(tiles[i].Location.X, tiles[i].Location.Y - imgSize.Height / rows);
                        }
                    }
                }

                missingPiece = row * cols + col;
            }
        }        
        
        public void Solve(Object sender, EventArgs e)
        {
            if (!solver.IsAlive)
            {
                solver = new Thread(solvePuzzle);
                solveButton.Text = (timeLimit - 1).ToString();
                timeLeft = timeLimit - 1;
                buttonTimer.Start();                
                solver.Start();
            }
            else
            {
                solver.Abort();
                buttonTimer.Stop();
                solveButton.Text = "Solve";
            }       
        }

        private void UpdateButton(object sender, ElapsedEventArgs e)
        {
            if (timeLeft <= 0)
            {
                buttonTimer.Stop();
                solveButton.Text = "Solve";
            }
            else
            {
                timeLeft--;
                solveButton.Text = timeLeft.ToString();
            }
        }

        private void solvePuzzle()
        {            
            List<int> moves = new List<int>();
            bool done = false;
            int[] positions = GetPositions();
            int maxThreshold = GetDistance(positions);

            BoardState currentState = new BoardState(moves, positions, -1, GetDistance(positions));

            while (buttonTimer.Enabled)
            {
                BoardState completedState = puzzleDiver(currentState, maxThreshold);

                if (IsSolved(completedState.positions))
                {
                    foreach (int m in completedState.moves)
                    {
                        for (int i = 0; i < pieces; i++)
                        {
                            Tile t = tiles[i];
                            if (t.Row * cols + t.Col == m)
                            {
                                t.Invoke(new MethodInvoker(delegate
                                {
                                    MovePiece(t);
                                }));

                                Thread.Sleep(100);
                                break;
                            }
                        }
                    }

                    string message = "Puzzle optimally solved in " + completedState.moves.Count + " moves.";
                    MessageBox.Show(message, "Done");
                    buttonTimer.Stop();
                    solveButton.Text = "Solve";
                    done = true;
                    break;
                }

                maxThreshold++;
            }

            if (!done)
            {
                string message = "I can not solve this puzzle in less than " + timeLimit + " seconds.";
                MessageBox.Show(message, "Failed");
            }
        }

        public int[] GetPositions()
        {
            int[] positions = new int[rows * cols];
            for (int i = 0; i < pieces; i++)
            {
                positions[tiles[i].Row * cols + tiles[i].Col] = i;
            }

            positions[missingPiece] = pieces;
            return positions;
        }

        private BoardState puzzleDiver(BoardState b, int maxThreshhold)
        {
            // Gone as far as possible in this dive
            if (b.moves.Count + b.distance >= maxThreshhold || IsSolved(b.positions))
            {
                return b;
            }

            List<int> moves = GetMoves(b.positions, b.moveTo);
            
            foreach (int m in moves)
            {
                if (buttonTimer.Enabled)
                {
                    List<int> newMoves = AddMove(b.moves, m);
                    int[] newPositions = ApplyMove(b.positions, m);
                    int moveTo = GetMoveTo(b.positions);

                    BoardState result = puzzleDiver(new BoardState(newMoves, newPositions, moveTo, GetDistance(newPositions)), maxThreshhold);

                    if (IsSolved(result.positions))
                    {
                        return result;
                    }
                }
            }

            return b;
        }
        
        private int GetDistance(int[] currentState)
        {
            int distance = 0;

            for (int i = 0; i < currentState.Length - 1; i++)
            {
                for (int j = 0; j < currentState.Length; j++)
                {
                    if (currentState[j] == i)
                    {
                        // Find the distance from j to i
                        distance += Math.Abs(j % cols - i % cols); // column distance
                        distance += Math.Abs(j / cols - i / cols); // row distance
                        break;
                    }
                }
            }

            return distance;
        }

        public bool IsSolved(int[] positions)
        {
            int i;
            for (i = 0; i < pieces; i++)
            {
                if (positions[i] != i)
                {
                    break;
                }
            }

            return i >= pieces;
        } 
        
        private List<int> GetMoves(int[] positions, int prevMove)
        {
            List<int> moves = new List<int>();
            int blank;

            for (blank = 0; blank < pieces; blank++)
            {
                if (positions[blank] == pieces)
                {
                    break;
                }
            }

            if (blank % cols > 0 && blank - 1 != prevMove)
            {
                moves.Add(blank - 1);
            }

            if (blank % cols < cols - 1 && blank + 1 != prevMove)
            {
                moves.Add(blank + 1);
            }

            if (blank - cols >= 0 && blank - cols != prevMove)
            {
                moves.Add(blank - cols);
            }

            if (blank + cols < rows * cols && blank + cols != prevMove)
            {
                moves.Add(blank + cols);
            }

            return moves;
        }
        
        private int[] ApplyMove(int[] prevGrid, int move)
        {
            int[] newGrid = new int[rows * cols];
            int blank = 0;

            for (int i = 0; i < rows * cols; i++)
            {
                newGrid[i] = prevGrid[i];

                if (prevGrid[i] == pieces)
                {
                    blank = i;
                }
            }

            newGrid[blank] = prevGrid[move];
            newGrid[move] = pieces;

            return newGrid;
        }

        private List<int> AddMove(List<int> prevMoves, int move)
        {
            List<int> newMoves = new List<int>();

            foreach (int m in prevMoves)
            {
                newMoves.Add(m);
            }

            newMoves.Add(move);
            return newMoves;
        } 
        
        private int GetMoveTo(int[] positions)
        {
            for (int i = 0; i < pieces; i++)
            {
                if (i == pieces)
                {
                    return i;
                }
            }

            return -1;
        }      
    }
}