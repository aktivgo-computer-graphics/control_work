using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace snake
{
    public partial class MainForm : Form
    {
        private Graphics Graph;
        private Pen MyPen;
        private SolidBrush MyBrush;
        private Timer timer;

        private List<GraphicsPath> barriers;
        private PointF[] points;

        private const int Fps = 144;
        
        public MainForm()
        {
            InitializeComponent();
            Graph = CreateGraphics();
            Graph.SmoothingMode = SmoothingMode.HighQuality;
            MyPen = new Pen(Color.Black, 2);
            MyBrush = new SolidBrush(Color.Black);

            barriers = new List<GraphicsPath>();

            CreateBarriers();

            InitTimer(1000 / Fps);
        }
        
        private void InitTimer(int interval)
        {
            timer = new Timer();
            timer.Interval = interval;
            timer.Tick += timer_tick;
            timer.Enabled = true;
        }
        
        private void timer_tick(object sender, EventArgs e)
        {
            Invalidate();
        }
        
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            PaintBarriers(e.Graphics);
        }

        private void CreateBarriers()
        {
            var barrier = new GraphicsPath();
            
            points = new PointF[4];
            points[0] = new PointF(100, 100);
            points[1] = new PointF(ClientSize.Width - 100, 100);
            points[2] = new PointF(ClientSize.Width - 100, ClientSize.Height - 100);
            points[3] = new PointF(100, ClientSize.Height - 100);
            barrier.AddPolygon(points);
            barriers.Add(barrier);
            
            points = new PointF[4];
            points[0] = new PointF(100, 100);
            points[1] = new PointF(ClientSize.Width - 100, 100);
            points[2] = new PointF(ClientSize.Width - 100, ClientSize.Height - 100);
            points[3] = new PointF(100, ClientSize.Height - 100);
            barrier = new GraphicsPath();
            barrier.AddPolygon(points);
            barriers.Add(barrier);
            
            points = new PointF[4];
            points[0] = new PointF(100, 100);
            points[1] = new PointF(ClientSize.Width - 100, 100);
            points[2] = new PointF(ClientSize.Width - 100, ClientSize.Height - 100);
            points[3] = new PointF(100, ClientSize.Height - 100);
            barrier = new GraphicsPath();
            barrier.AddPolygon(points);
            barriers.Add(barrier);
        }

        private void PaintBarriers(Graphics graph)
        {
            foreach (var barrier in barriers)
            {
                graph.DrawPath(MyPen, barrier);
            }
        }
    }
}