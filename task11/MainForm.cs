using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace task11
{
    public partial class MainForm : Form
    {
        private readonly Graphics Graph;
        private readonly Pen MyPen;
        private readonly SolidBrush MyBrush;
        private Timer timer;
        
        private GraphicsPath rectangle; // Прямоугольник
        private PointF[] points;        // Буффер для создания прямоугольника
        
        private int xPoint, yPoint;                     // Координаты точки
        private bool pToRight = true, pToDown = true;   // Флаги направления движения точки

        private int countRotates;   // Количество вращений прямоугольника
        private bool toLeft = true; // Направление движения прямоугольника

        private const int PRadius = 10;                 // Радиус точки
        private const int PStep = 8;                    // Шаг движения точки
        private const float RRotateAngle = 1;           // Угол вращения прямоугольника за один тик
        private const int RLength = 400, RHeight = 200; // Размеры прямоугольника
        
        private const int Fps = 144;   // Количество кадров в секунду

        public MainForm()
        {
            InitializeComponent();
            Graph = CreateGraphics();
            Graph.SmoothingMode = SmoothingMode.HighQuality;
            MyPen = new Pen(Color.Black, 2);
            MyBrush = new SolidBrush(Color.Black);

            InitTimer(1000 / Fps);
            CreateRectangle();
            CreatePoint();
        }

        private void InitTimer(int interval)
        {
            timer = new Timer();
            timer.Interval = interval;
            timer.Enabled = true;
            timer.Tick += timer_tick;
        }

        private void CreateRectangle()
        {
            countRotates = 0;
            points = new PointF[4];
            points[0] = new PointF(ClientSize.Width / 2 - RLength / 2, ClientSize.Height / 2);
            points[1] = new PointF(ClientSize.Width / 2 + RLength / 2, ClientSize.Height / 2);
            points[2] = new PointF(ClientSize.Width / 2 + RLength / 2, ClientSize.Height / 2 + RHeight);
            points[3] = new PointF(ClientSize.Width / 2 - RLength / 2, ClientSize.Height / 2 + RHeight);
        }

        private void CreatePoint()
        {
            xPoint = ClientSize.Width / 2;
            yPoint = ClientSize.Height / 2 + RHeight / 2;
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
            MoveRectangle();
            MovePoint();
            e.Graphics.DrawPolygon(MyPen, points);
            e.Graphics.FillEllipse(MyBrush, xPoint, yPoint, 2 * PRadius, 2 * PRadius);
        }

        /// <summary>
        /// Считает новые координаты прямоугольника
        /// </summary>
        private void MoveRectangle()
        {
            // Если прямоугольник сделал полный круг
            if (countRotates == (int)(360 / RRotateAngle))
            {
                // Сдвигаем прямоугольник
                OffsetX();
                MakeSound();
                // Обнуляем счеткик поворотов
                countRotates = 0;
            }
            
            // Считаем новые координаты прямоугольника с учетом поворота относительно правой верхней вершины
            const double angleRadian = RRotateAngle * Math.PI / 180;
            var pointRotate = points[1];
            for (var i = 0; i < points.Length; i++)
            {
                var curX = (float)((points[i].X - pointRotate.X) * Math.Cos(angleRadian) - (points[i].Y - pointRotate.Y) * Math.Sin(angleRadian) + pointRotate.X);
                var curY = (float)((points[i].X - pointRotate.X) * Math.Sin(angleRadian) + (points[i].Y - pointRotate.Y) * Math.Cos(angleRadian) + pointRotate.Y);
                points[i] = new PointF(curX, curY);
            }
            rectangle = new GraphicsPath();
            rectangle.AddPolygon(points);
            
            // Инкрементируем счетчик поворотов
            countRotates++;
        }
        
        /// <summary>
        /// Сдвигает прямоугольник относительно оси X
        /// </summary>
        private void OffsetX()
        {
            // Если прямоугольник движется влево, а при слудующем шаге левая вершина выйдет за пределы экрана,
            // то меняем напрвление
            if (toLeft && points[3].X - 50 < 0)
            {
                toLeft = false;
            }

            // Если прямоугольник движется вправо, а при слудующем шаге правая вершина выйдет за пределы экрана,
            // то меняем напрвление
            if (!toLeft && points[0].X + 50 > ClientSize.Width)
            {
                toLeft = true;
            }
                
            // Считаем новые координаты прямоугольник с учетом шага
            for (var i = 0; i < points.Length; i++)
            {
                var curX = toLeft ? points[i].X - 50 :  points[i].X + 50;
                var curY = points[i].Y;
                points[i] = new PointF(curX, curY);
            }
            rectangle = new GraphicsPath();
            rectangle.AddPolygon(points);
        }
        
        private void MakeSound()
        {
            System.Media.SystemSounds.Exclamation.Play();
        }

        /// <summary>
        /// Считает новые координаты точки
        /// </summary>
        private void MovePoint()
        {
            if (rectangle == null) return;

            // Если можем двигаться направо, то движемся направо, иначе - налево
            if (pToRight) xPoint += PStep;
            else xPoint -= PStep;

            // Если можем двигаться вниз, то движемся вниз, иначе - вверх
            if (pToDown) yPoint += PStep;
            else yPoint -= PStep;

            // В зависимости от пересечения прямоугольника на следующем шаге меняем направление точки
            if (!rectangle.IsVisible(new Point(xPoint, yPoint + PStep + 2 * PRadius))) pToDown = false;
            if (!rectangle.IsVisible(new Point(xPoint, yPoint - PStep))) pToDown = true;
            if (!rectangle.IsVisible(new Point(xPoint - PStep, yPoint))) pToRight = true;
            if (!rectangle.IsVisible(new Point(xPoint + PStep + 2 * PRadius, yPoint))) pToRight = false;
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