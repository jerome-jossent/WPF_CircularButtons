using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace arc
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //http://csharphelper.com/blog/2019/05/make-an-intuitive-extension-method-to-draw-an-elliptical-arc-in-wpf-and-c/
        //http://csharphelper.com/blog/2014/12/find-the-area-where-circles-overlap-in-c/

        public MainWindow()
        {
            InitializeComponent();
            DrawArc2();
        }

        void DrawArc()
        {
            Canvas cnv = new Canvas();
            Path pth = new Path();
            pth.Fill = Brushes.BlueViolet;
            pth.Stroke = Brushes.OrangeRed;

            PathGeometry pg = new PathGeometry();
            PathFigureCollection pfc = new PathFigureCollection();

            PathFigure pf = new PathFigure();

            ArcSegment a = new ArcSegment(new Point(200, 100), new Size(300, 300), 45, true, SweepDirection.Clockwise, true);

            cnv.Children.Add(pth);
            pth.Data = pg;
            pfc.Add(pf);
            pg.Figures = pfc;
            pf.Segments.Add(a);
            grd.Children.Insert(0, cnv);
        }

        void DrawArc2()
        {
            Arc _arc = new Arc();
            _arc.StartAngle = 0;
            _arc.EndAngle = 45;

            grd.Children.Insert(0, _arc);
        }

        private void ME(object sender, MouseEventArgs e)
        {
            Path p = (Path)sender;
            Title = p.Name + " ENTER";
        }

        private void MD(object sender, MouseButtonEventArgs e)
        {
            Path p = (Path)sender;
            Title = p.Name + " DOWN";
        }

        private void ML(object sender, MouseEventArgs e)
        {
            Path p = (Path)sender;
            Title = p.Name + " LEAVE";
        }
    }
}
