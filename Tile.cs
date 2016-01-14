using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;

namespace ImagePuzzle
{
    internal class Tile : PictureBox
    {
        private PuzzleForm parent;
        private int row;
        private int col;

        public Tile(PuzzleForm p)
        {
            parent = p;
        }

        public int Row
        {
            get { return row; }
            set { row = value; }
        }

        public int Col
        {
            get { return col; }
            set { col = value; }
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            parent.solver.Abort();
            parent.buttonTimer.Stop();
            parent.solveButton.Text = "Solve";
            parent.MovePiece(this);
            int[] positions = parent.GetPositions();

            if (parent.IsSolved(positions))
            {
                MessageBox.Show("Congratulations! Puzzle is solved", "Done");
            }

        }
    }
}