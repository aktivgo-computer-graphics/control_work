using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace snake
{
    public partial class MainForm : Form
    {
        private readonly Graphics Graph;
        private readonly Pen MyPen;
        private Timer timer;


        private bool isConnected;       // Флаг соединения с препятствием
        private int countSteps;         // Количество шагов
        private PointF headCoordinates; // Координаты головы
        private PointF curConnectPoint; // Точка текущего соединения

        private enum Orientation    // Перечисление направлений змейки
        {
            UP, RIGHT, DOWN, LEFT
        }

        private Orientation curOrientation = Orientation.DOWN;      // Текущее направление
        private Orientation curBarrier;                             // Текущая фигура
        private readonly Queue<GraphicsPath> snake;                 // Змейка
        private readonly Dictionary<GraphicsPath, bool> barriers;   // Препятствия

        private const float SizeSegment = 10;   // Размер сегмента змейки
        private const int Fps = 30;             // Количество кадров в секунду
        
        public MainForm()
        {
            InitializeComponent();
            Graph = CreateGraphics();
            Graph.SmoothingMode = SmoothingMode.HighQuality;
            MyPen = new Pen(Color.Black, 2);

            snake = new Queue<GraphicsPath>();
            barriers = new Dictionary<GraphicsPath, bool>();

            CreateBarriers();
            CreateSnake(150, 0, 10);

            InitTimer(1000 / Fps);
        }
        
        private void InitTimer(int interval)
        {
            timer = new Timer();
            timer.Interval = interval;
            timer.Tick += timer_tick;
            timer.Enabled = true;
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
            PaintBarriers(e.Graphics);
            MoveSnake(e.Graphics);
            PaintSnake(e.Graphics);
        }

        /// <summary>
        /// Создание препятствий
        /// </summary>
        private void CreateBarriers()
        {
            var barrier = new GraphicsPath();
            
            var points = new PointF[4];
            points[0] = new PointF(100, 100);
            points[1] = new PointF(300, 100);
            points[2] = new PointF(300, 200);
            points[3] = new PointF(100, 200);
            barrier.AddPolygon(points);
            barriers.Add(barrier, false);
            
            points = new PointF[6];
            points[0] = new PointF(500, 200);
            points[1] = new PointF(800, 200);
            points[2] = new PointF(800, 600);
            points[3] = new PointF(600, 600);
            points[4] = new PointF(600, 400);
            points[5] = new PointF(500, 400);
            barrier = new GraphicsPath();
            barrier.AddPolygon(points);
            barriers.Add(barrier, false);
        }
        
        /// <summary>
        /// Создание змейки
        /// </summary>
        /// <param name="x">Начальная координата</param>
        /// <param name="y">Начальная координата</param>
        /// <param name="length">Длина змейки в сегментах</param>
        private void CreateSnake(float x, float y, int length)
        {
            for (var i = 0; i < length; i++)
            {
                var sectionCoordinate = new PointF(x + SizeSegment, y);
                var size = new SizeF(SizeSegment, SizeSegment);
                var section = new RectangleF(sectionCoordinate, size);
                var path = new GraphicsPath();
                
                path.AddRectangle(section);
                snake.Enqueue(path);
                Graph.DrawPath(MyPen, path);
                
                headCoordinates = sectionCoordinate;
            }
        }

        /// <summary>
        /// Отрисовка препятствий
        /// </summary>
        /// <param name="graph"></param>
        private void PaintBarriers(Graphics graph)
        {
            foreach (var barrier in barriers)
            {
                graph.DrawPath(MyPen, barrier.Key);
            }
        }

        /// <summary>
        /// Считает новые координаты змейки
        /// </summary>
        /// <param name="graph"></param>
        private void MoveSnake(Graphics graph)
        {
            // Определяем следующее направление змейки
            CalculateNextOrientation(graph);
            // Делаем шаг в этом направлении
            StepSnake();
        }
        
        /// <summary>
        /// Определяет следующее направление змейки
        /// </summary>
        /// <param name="graph"></param>
        private void CalculateNextOrientation(Graphics graph)
        {
            var endBarrier = true;

            // Если змейка соединена с препятствием
            if (isConnected)
            {
                var nextPoint = GetNextPoint();
                // Если сделан полный обход препятствия
                if (countSteps > 2 && Math.Abs(nextPoint.X - curConnectPoint.X) <= SizeSegment
                   && Math.Abs(nextPoint.Y - curConnectPoint.Y) <= SizeSegment)
                {
                    // Отсоединяемся
                    isConnected = false;
                    return;
                }
                
                countSteps++;
                
                var connectPoint = GetConnectPoint();
                foreach (var barrier in barriers.Keys)
                {
                    // Если впереди фигура
                    if (IsEntersBorder(barrier, nextPoint))
                    {
                        // Поворачиваем
                        curOrientation = (Orientation)((int)(curBarrier + 2) % 4);
                        curBarrier = (Orientation)((int)(curBarrier + 1) % 4);
                        return;
                    }
                    
                    // Если сторона препятствия не закончилась
                    if (IsEntersBorder(barrier, connectPoint))
                    {
                        endBarrier = false;
                    }
                }
                
                // Не поворачиваем
                if (!endBarrier) return;
                
                // Иначе поворачиваем
                curOrientation = curBarrier;
                curBarrier = (Orientation)((int)(curBarrier + 3) % 4);
            } 
            // Если не соединены с препятствием
            else
            {
                // Проверяем, находится ли впереди ещё не проходимое препятствие
                CheckConnect(graph);
            }
        }
        
        /// <summary>
        /// Делает шаг
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void StepSnake()
        {
            switch (curOrientation)
            {
                case Orientation.UP:
                    StepUp();
                    break;
                case Orientation.RIGHT:
                    StepRight();
                    break;
                case Orientation.DOWN:
                    StepDown();
                    break;
                case Orientation.LEFT:
                    StepLeft();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Получение следующей точки по направлению движения
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private PointF GetNextPoint()
        {
            PointF nextPoint;
            switch (curOrientation)
            {
                case Orientation.UP:
                    nextPoint = new PointF(headCoordinates.X, headCoordinates.Y - SizeSegment);
                    break;
                case Orientation.RIGHT:
                    nextPoint = new PointF(headCoordinates.X + SizeSegment, headCoordinates.Y);
                    break;
                case Orientation.LEFT:
                    nextPoint = new PointF(headCoordinates.X - SizeSegment, headCoordinates.Y);
                    break;
                case Orientation.DOWN:
                    nextPoint = new PointF(headCoordinates.X, headCoordinates.Y + SizeSegment);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return nextPoint;
        }
        
        /// <summary>
        /// Получение точки соединения с препятствием
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private PointF GetConnectPoint()
        {
            PointF connectPoint;
            switch (curBarrier)
            {
                case Orientation.UP:
                    connectPoint = new PointF(headCoordinates.X, headCoordinates.Y - SizeSegment);
                    break;
                case Orientation.RIGHT:
                    connectPoint = new PointF(headCoordinates.X + SizeSegment, headCoordinates.Y);
                    break;
                case Orientation.LEFT:
                    connectPoint = new PointF(headCoordinates.X - SizeSegment, headCoordinates.Y);
                    break;
                case Orientation.DOWN:
                    connectPoint = new PointF(headCoordinates.X, headCoordinates.Y + SizeSegment);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return connectPoint;
        }
        
        /// <summary>
        /// Проверяет, пересекла ли голова змеи препятствие
        /// </summary>
        /// <param name="path"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool IsEntersBorder(GraphicsPath path, PointF point)
        {
            var p1 = new PointF(point.X, point.Y);
            var p2 = new PointF(point.X, point.Y + SizeSegment);
            var p3 = new PointF(point.X + SizeSegment, point.Y);
            var p4 = new PointF(point.X + SizeSegment, point.Y + SizeSegment);

            if (path.IsVisible(p1) || path.IsVisible(p2) || path.IsVisible(p3) || path.IsVisible(p4))
            {
                return true;
            }
            
            return path.IsOutlineVisible(p1, MyPen) || path.IsOutlineVisible(p2, MyPen) ||
                   path.IsOutlineVisible(p3, MyPen) || path.IsOutlineVisible(p4, MyPen);
        }
        
        /// <summary>
        /// Проверяет, находится ли впереди ещё не проходимое препятствие
        /// </summary>
        /// <param name="graph"></param>
        private void CheckConnect(Graphics graph)
        {
            var nextPoint = GetNextPoint();
            if (nextPoint.X >= ClientSize.Width )
            {
                StepDown();
                curOrientation = Orientation.LEFT;
            }
            else if (nextPoint.X <= 0) 
            {
                StepDown();
                curOrientation = Orientation.RIGHT;
            }
            else if(nextPoint.Y <= 0)
            {
                StepRight();
                curOrientation = Orientation.DOWN;
            }
            else if (nextPoint.Y >= ClientSize.Height)
            {
                StepRight();
                curOrientation = Orientation.UP;
            }
            
            PaintSnake(graph);

            for (var i = 0; i < barriers.Count; i++)
            {
                if (!IsEntersBorder(barriers.ElementAt(i).Key, nextPoint)) continue;
                
                if (barriers.ElementAt(i).Value)
                {
                    StepBound(graph);
                    return;
                }
                
                isConnected = true;
                countSteps = 0;
                curConnectPoint = headCoordinates;

                // Помечаем препятствие как пройденное
                var path = barriers.ElementAt(i).Key;
                barriers.Remove(barriers.ElementAt(i).Key);
                barriers.Add(path, true);

                curBarrier = curOrientation;
                curOrientation = (Orientation)((int)(curOrientation + 1) % 4);
                CalculateNextOrientation(graph);
                return;
            }
        }
        
        /// <summary>
        /// Меняет направление при сталкивании с препятствием или границей окна
        /// </summary>
        /// <param name="graph"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void StepBound(Graphics graph)
        {
            switch (curOrientation)
            {
                case Orientation.RIGHT:
                    StepDown();
                    curOrientation = Orientation.LEFT;
                    break;
                case Orientation.LEFT:
                    StepDown();
                    curOrientation = Orientation.RIGHT;
                    break;
                case Orientation.DOWN:
                    StepRight();
                    curOrientation = Orientation.UP;
                    break;
                case Orientation.UP:
                    StepRight();
                    curOrientation = Orientation.DOWN;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            PaintSnake(graph);
        }

        private void StepLeft()
        {
            headCoordinates.X -= SizeSegment;
        }

        private void StepRight()
        {
            headCoordinates.X += SizeSegment;
        }

        private void StepUp()
        {
            headCoordinates.Y -= SizeSegment;
        }

        private void StepDown()
        {
            headCoordinates.Y += SizeSegment;
        } 
        
        /// <summary>
        /// Отрисовка змейки
        /// </summary>
        /// <param name="graph"></param>
        private void PaintSnake(Graphics graph)
        {
            snake.Dequeue();
            
            var path = new GraphicsPath();
            var head = new RectangleF(headCoordinates, new SizeF(SizeSegment, SizeSegment));
            path.AddRectangle(head);
            snake.Enqueue(path);
            foreach (var segment in snake)
            {
                graph.DrawPath(MyPen, segment);
            }
        }
    }
}