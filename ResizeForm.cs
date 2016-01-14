using System;
using System.Drawing;
using System.Windows.Forms;

namespace ImagePuzzle
{
    internal class ResizeForm : Form
    {
        private PuzzleForm puzzleForm;
        private TextBox rowBox;
        private TextBox colBox;
        private TextBox timeBox;
        private Button okButton;
        private Button cancelButton;

        public ResizeForm(PuzzleForm p) {

            puzzleForm = p;

            Text = "Rows and Columns";
            Size = new Size(250,160);

            TableLayoutPanel table = new TableLayoutPanel();
            table.RowCount = 4;
            table.ColumnCount = 2;
            table.Parent = this;
            table.Dock = DockStyle.Fill;
            table.Padding = new Padding(10);

            Label rowLabel = new Label();
            rowLabel.Text = "Rows (2 - 8): ";
            rowLabel.TextAlign = ContentAlignment.MiddleRight;
            rowLabel.Parent = table;

            rowBox = new TextBox();
            rowBox.Text = p.Rows.ToString();
            rowBox.Parent = table;

            Label colLabel = new Label();
            colLabel.Text = "Columns (2 - 8): ";
            colLabel.TextAlign = ContentAlignment.MiddleRight;
            colLabel.Parent = table;

            colBox = new TextBox();
            colBox.Text = p.Cols.ToString();
            colBox.Parent = table;

            Label timeLabel = new Label();
            timeLabel.Text = "Time (secs): ";
            timeLabel.TextAlign = ContentAlignment.MiddleRight;
            timeLabel.Parent = table;

            timeBox = new TextBox();
            timeBox.Text = p.TimeLimit.ToString();
            timeBox.Parent = table;

            okButton = new Button();
            okButton.Text = "OK";
            okButton.Click += ResizeDone;
            okButton.Anchor = AnchorStyles.None;
            okButton.Parent = table;

            cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Click += ResizeDone;
            cancelButton.Anchor = AnchorStyles.None;
            cancelButton.Parent = table;
        }

        private void ResizeDone(object sender, EventArgs e)
        {
            if (sender == okButton)
            {
                int rows;
                int.TryParse(rowBox.Text, out rows);

                int cols;
                int.TryParse(colBox.Text, out cols);

                int time;
                int.TryParse(timeBox.Text, out time);

                if (rows > 1 && cols > 1 && rows < 9 && cols < 9 && time > 0)
                {
                    puzzleForm.Rows = rows;
                    puzzleForm.Cols = cols;
                    puzzleForm.TimeLimit = time;

                    puzzleForm.SetBackgroundImage();
                    Hide();
                }
                else
                {
                    MessageBox.Show("Rows and columns must be between 2 and 8. Time must be greater than 0.", "Invalid Entry");
                }                
            }
            else
            {
                rowBox.Text = puzzleForm.Rows.ToString();
                colBox.Text = puzzleForm.Cols.ToString();
                timeBox.Text = puzzleForm.TimeLimit.ToString();
                Hide();
            }
        }
    }
}