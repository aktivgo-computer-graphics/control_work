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
        private SolidBrush BlackBrush;
        private SolidBrush WhiteBrush;

        private Timer timerPoint;
        
        private GraphicsPath rectangle;
        private PointF[] points;
        private bool toRight = true, toDown = true;
        private int xPoint, yPoint;
        private const int BallStep = 5;
        private const int BallRadius = 50;

        private Queue<Point> snake;
        private Point head;
        
        public MainForm()
        {
            InitializeComponent();
            Graph = CreateGraphics();
            Graph.SmoothingMode = SmoothingMode.HighSpeed;
            BlackPen = new Pen(Color.Black, 2);
            WhitePen = new Pen(Color.White, 2);
            BlackBrush = new SolidBrush(Color.Black);
            WhiteBrush = new SolidBrush(Color.White);

            snake = new Queue<Point>();

            timerPoint = new Timer();
            timerPoint.Interval = 50;
            timerPoint.Tick += timerPoint_tick;
            
            xPoint = ClientSize.Width / 2;
            yPoint = ClientSize.Height / 2;
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            Graph.Clear(Color.White);
            timerPoint.Stop();
            
            switch (taskSwitcher.SelectedIndex)
            {
                case 0:
                    StartSnake();
                    break;
                case 1:
                    points = new PointF[4];
                    points[0] = new PointF(100, 100);
                    points[1] = new PointF(ClientSize.Width - 100, 100);
                    points[2] = new PointF(ClientSize.Width - 100, ClientSize.Height - 100);
                    points[3] = new PointF(100, ClientSize.Height - 100);
                    rectangle = new GraphicsPath();
                    rectangle.AddPolygon(points);
                    PaintBoard();

                    PaintBall();
                    timerPoint.Start();
                    break;
            }
        }

        private void PaintBoard()
        {
            Graph.DrawPolygon(BlackPen, points);
        }
        
        private void timerPoint_tick(object sender, EventArgs e)
        {
            if (rectangle == null) return;

            ClearBall();

            if (toRight) xPoint += BallStep;
            else xPoint -= BallStep;

            if (toDown) yPoint += BallStep;
            else yPoint -= BallStep;
            
            PaintBall();

            if (!rectangle.IsVisible(new Point(xPoint, yPoint + BallStep + 2 * BallRadius))) toDown = false;
            if (!rectangle.IsVisible(new Point(xPoint, yPoint - BallStep))) toDown = true;
            if (!rectangle.IsVisible(new Point(xPoint - BallStep, yPoint))) toRight = true;
            if (!rectangle.IsVisible(new Point(xPoint + BallStep + 2 * BallRadius, yPoint))) toRight = false;
        }
        
        private void PaintBall()
        {
            Graph.FillEllipse(BlackBrush, xPoint, yPoint, 2 * BallRadius, 2 * BallRadius);
        }
        
        private void ClearBall()
        {
            Graph.FillEllipse(WhiteBrush, xPoint, yPoint, 2 * BallRadius, 2 * BallRadius);
        }
        
        private void stopBtn_Click(object sender, EventArgs e)
        {
            timerPoint.Stop();
        }

        private void StartSnake()
        {
            var barriers = CreateBarriers();
            PaintBarriers(barriers);
            
            snake = CreateSnake(5);
            PaintSnake(snake);
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
            WhitePen.Dispose();
            BlackBrush.Dispose();
            WhiteBrush.Dispose();
            timerPoint.Dispose();
        }
    }
}