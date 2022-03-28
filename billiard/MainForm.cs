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

namespace billiard
{
    public partial class MainForm : Form
    {
        private Graphics Graph;
        private Pen MyPen;
        private SolidBrush MyBrush;
        private SolidBrush WhiteBrush;
        private Timer timer;
        
        private GraphicsPath rectangle;             // Прямоугольник/доска
        private PointF[] points;                    // Буффер для создания прямоугольника/доски
        
        private int xPoint, yPoint;                 // Координаты шара
        private bool toRight = true, toDown = true; // Флаги направления движения шара

        private const int BallRadius = 50;  // Радиус шара
        private const int BallStep = 5;     // Шаг движения шара
        
        private const int Fps = 144;    // Количество кадров в секунду

        public MainForm()
        {
            InitializeComponent();
            Graph = CreateGraphics();
            Graph.SmoothingMode = SmoothingMode.HighQuality;
            MyPen = new Pen(Color.Black, 2);
            MyBrush = new SolidBrush(Color.Black);

            CreateBoard();
            InitTimer(1000 / Fps);

            xPoint = ClientSize.Width / 2;
            yPoint = ClientSize.Height / 2;
        }

        private void InitTimer(int interval)
        {
            timer = new Timer();
            timer.Interval = interval;
            timer.Tick += timer_tick;
            timer.Enabled = true;
        }

        private void CreateBoard()
        {
            points = new PointF[4];
            points[0] = new PointF(100, 100);
            points[1] = new PointF(ClientSize.Width - 100, 100);
            points[2] = new PointF(ClientSize.Width - 100, ClientSize.Height - 100);
            points[3] = new PointF(100, ClientSize.Height - 100);
            rectangle = new GraphicsPath();
            rectangle.AddPolygon(points);
        }

        /// <summary>
        /// Каждый тик перерисовывает окно
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_tick(object sender, EventArgs e)
        {
            Invalidate();
        }
        
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            MoveBall();
            e.Graphics.DrawPolygon(MyPen, points); 
            e.Graphics.FillEllipse(MyBrush, xPoint, yPoint, 2 * BallRadius, 2 * BallRadius);
        }

        /// <summary>
        /// Считает новые координаты шара
        /// </summary>
        private void MoveBall()
        {
            if (rectangle == null) return;

            // Если можем двигаться направо, то движемся направо, иначе - налево
            if (toRight) xPoint += BallStep;
            else xPoint -= BallStep;

            // Если можем двигаться вниз, то движемся вниз, иначе - вверх
            if (toDown) yPoint += BallStep;
            else yPoint -= BallStep;

            // В зависимости от пересечения прямоугольника на следующем шаге меняем направление шара
            if (!rectangle.IsVisible(new Point(xPoint, yPoint + BallStep + 2 * BallRadius)))
            {
                MakeSound();
                toDown = false;
            }

            if (!rectangle.IsVisible(new Point(xPoint, yPoint - BallStep)))
            {
                MakeSound();
                toDown = true;
            }

            if (!rectangle.IsVisible(new Point(xPoint - BallStep, yPoint)))
            {
                MakeSound();
                toRight = true;
            }

            if (!rectangle.IsVisible(new Point(xPoint + BallStep + 2 * BallRadius, yPoint)))
            {
                MakeSound();
                toRight = false;
            }
        }

        private void MakeSound()
        {
            System.Media.SystemSounds.Exclamation.Play();
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
                Graph.DrawPath(MyPen, barrier);
            }
        }
        
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Graph.Dispose();
            MyPen.Dispose();
            MyBrush.Dispose();
            timer.Dispose();
        }
    }
}