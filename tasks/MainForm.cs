using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace snake
{
    public partial class MainForm : Form
    {
        private Graphics Graph;
        private Pen BlackPen;
        private Pen WhitePen;

        private Queue<Point> snake;
        private Point head;
        
        public MainForm()
        {
            InitializeComponent();
            Graph = CreateGraphics();
            Graph.SmoothingMode = SmoothingMode.HighQuality;
            BlackPen = new Pen(Color.Black, 2);
            WhitePen = new Pen(Color.White, 2);

            snake = new Queue<Point>();
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            timer.Stop();
            
            switch (taskSwitcher.SelectedIndex)
            {
                case 0:
                    StartSnake();
                    break;
                case 1:
                    break;
            }
        }
        
        private void stopBtn_Click(object sender, EventArgs e)
        {
            timer.Stop();
        }

        private void StartSnake()
        {
            var barriers = CreateBarriers();
            PaintBarriers(barriers);
            
            snake = CreateSnake(5);
            PaintSnake(snake);
            
            timer.Start();
        }
        
        private List<GraphicsPath> CreateBarriers()
        {
            var barriers = new List<GraphicsPath>();
            
            var barrier = new List<Point>
            {
                new(100, 100),
                new(300, 100),
                new(300, 300),
                new(100, 300)
            };
            var path = new GraphicsPath();
            path.AddLines(barrier.ToArray());
            path.CloseAllFigures();
            barriers.Add(path);
            
            barrier = new List<Point>
            {
                new(500, 300),
                new(700, 300),
                new(700, 700),
                new(600, 700),
                new(600, 500),
                new(500, 500),
            };
            path = new GraphicsPath();
            path.AddLines(barrier.ToArray());
            path.CloseAllFigures();
            barriers.Add(path);

            return barriers;
        }

        private void PaintBarriers(List<GraphicsPath> barriers)
        {
            foreach (var barrier in barriers)
            {
                Graph.DrawPath(BlackPen, barrier);
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            Step();
        }

        private Queue<Point> CreateSnake(int size)
        {
            var s = new Queue<Point>();

            for (var i = 0; i < size; i++)
            {
                var p = new Point(70 - i * 10, 100);
                s.Enqueue(p);

                if (i == size - 1)
                {
                    head = p;
                }
            }

            return s;
        }

        private void PaintSnake(Queue<Point> s)
        {
            var size = s.Count;
            for (var i = 0; i < size; i++)
            {
                var tail = s.Dequeue();
                Graph.DrawRectangle(BlackPen, tail.X - 2, tail.Y - 2, 4, 4);
            }
        }

        private void Step()
        {
            var tail = snake.Dequeue();
            ClearPart(tail);
            tail.X = head.X + 10;
            snake.Enqueue(tail);
            PaintPart(tail);
        }

        private void PaintPart(Point part)
        {
            Graph.DrawRectangle(WhitePen, part.X - 2, part.Y - 2, 4, 4);
        }

        private void ClearPart(Point part)
        {
            Graph.DrawRectangle(WhitePen, part.X - 2, part.Y - 2, 4, 4);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Graph.Dispose();
            BlackPen.Dispose();
            timer.Dispose();
        }
    }
}