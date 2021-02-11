using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace arc
{
    /// <summary>
    /// Defines an arc
    /// </summary>
    /// <seealso cref="System.Windows.Shapes.Shape" />
    public class Arc : Shape
    {
        /// <summary>
        /// Initializes the <see cref="Arc"/> class.
        /// </summary>
        // Angle that arc starts at
        public double StartAngle
        {
            get => (double)GetValue(StartAngleProperty);
            set => SetValue(StartAngleProperty, value);
        }

        // DependencyProperty - StartAngle
        private static PropertyMetadata startAngleMetadata =
                new FrameworkPropertyMetadata(
                    0.0,     // Default value
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    null,    // Property changed callback
                    new CoerceValueCallback(CoerceAngle))
                {
                };   // Coerce value callback

        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register("StartAngle", typeof(double), typeof(Arc), startAngleMetadata);

        // Angle that arc ends at
        public double EndAngle
        {
            get => (double)GetValue(EndAngleProperty);
            set => SetValue(EndAngleProperty, value);
        }

        // DependencyProperty - EndAngle
        private static PropertyMetadata endAngleMetadata =
                new FrameworkPropertyMetadata(
                    90.0,     // Default value
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    null,    // Property changed callback
                    new CoerceValueCallback(CoerceAngle));   // Coerce value callback

        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register("EndAngle", typeof(double), typeof(Arc), endAngleMetadata);

        /// <summary>
        /// Coerces the angle.
        /// </summary>
        /// <param name="depObj">The dep object.</param>
        /// <param name="baseVal">The base value.</param>
        /// <returns></returns>
        private static object CoerceAngle(DependencyObject depObj, object baseVal)
        {
            double angle = (double)baseVal;
            angle = Math.Min(angle, 360.0);
            angle = Math.Max(angle, 0.0);
            return angle;
        }


        /// <summary>
        /// Gets or sets the sweep direction.
        /// </summary>
        /// <value>
        /// The sweep direction.
        /// </value>
        public SweepDirection SweepDirection
        {
            get => (SweepDirection)GetValue(SweepDirectionProperty);
            set => SetValue(SweepDirectionProperty, value);
        }

        // Using a DependencyProperty as the backing store for SweepDirection.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SweepDirectionProperty =
            DependencyProperty.Register("SweepDirection", typeof(SweepDirection), typeof(Arc), new FrameworkPropertyMetadata(
                    SweepDirection.Counterclockwise,     // Default value
                    FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    null));



        /// <summary>
        /// Gets a value that represents the <see cref="T:System.Windows.Media.Geometry" /> of the <see cref="T:System.Windows.Shapes.Shape" />.
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                var width = Visibility == Visibility.Visible ? ActualWidth : 0.0;
                var height = Visibility == Visibility.Visible ? ActualHeight : 0.0;

                double maxWidth = Math.Max(0.0, width - StrokeThickness);
                double maxHeight = Math.Max(0.0, height - StrokeThickness);

                if (maxWidth == 0
                  || maxHeight == 0)
                {
                    return new StreamGeometry();
                }

                var arcSize = new Size(maxWidth / 2.0, maxHeight / 2);
                var calculator = new ElipticCurveCalculator(new Size(width, height), new Size(maxWidth / 2, maxHeight / 2));

                var angle = EndAngle - StartAngle;
                var firstAngle = Math.Min(angle, 180);
                var secondAngle = Math.Max(angle - 180, 0);

                // start = 20; end = 200; angle = 180
                var firstStart = SweepDirection == SweepDirection.Clockwise ? -StartAngle : StartAngle;
                var firstEndAngle = SweepDirection == SweepDirection.Clockwise ? firstStart - firstAngle : firstStart + firstAngle;

                var secondEndAngle = 0.0;
                if (secondAngle > 0)
                {
                    secondEndAngle = SweepDirection == SweepDirection.Clockwise ? firstStart - firstAngle - secondAngle : firstStart + firstAngle + secondAngle;
                }

                var p1Start = calculator.GetPoint(firstStart);
                var p1End = calculator.GetPoint(firstEndAngle);

                StreamGeometry geom = new StreamGeometry();
                using (StreamGeometryContext ctx = geom.Open())
                {
                    ctx.BeginFigure(p1Start,
                        false,
                        false);

                    ctx.ArcTo(p1End,
                        arcSize,
                        0.0,     // rotationAngle
                       false,   // greater than 180 deg?
                        SweepDirection,
                        true,    // isStroked
                        true);

                    if (secondEndAngle != 0)
                    {
                        var p2End = calculator.GetPoint(secondEndAngle);

                        ctx.ArcTo(
                          p2End,
                          arcSize,
                          0.0,     // rotationAngle
                         false,   // greater than 180 deg?
                          SweepDirection, // == SweepDirection.Clockwise ? SweepDirection.Counterclockwise : SweepDirection.Clockwise,
                          true,    // isStroked
                          true);
                    }
                }

                geom.Freeze();

                return geom;
            }
        }

        private double GetFistAngle(double startAngle, double angle)
        {
            var endAngle = SweepDirection == SweepDirection.Counterclockwise
              ? Math.Min(startAngle - angle, 180)
              : Math.Min(angle - startAngle, 180);

            return endAngle;
        }
    }

    /// <summary>
    /// Calculates points for an eliptic curve
    /// </summary>
    class ElipticCurveCalculator
    {
        /// <summary>
        /// The point calculator
        /// </summary>
        private readonly EllipsePointCalculator _ellipsePointCalculator;

        /// <summary>
        /// The half of a box size
        /// </summary>
        private readonly Size _boxSizeHalf;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElipticCurveCalculator"/> class.
        /// </summary>
        /// <param name="boxSize">Size of the box.</param>
        /// <param name="arcSize">Size of the arc.</param>
        public ElipticCurveCalculator(Size boxSize, Size arcSize)
        {
            _ellipsePointCalculator = new EllipsePointCalculator(arcSize);
            _boxSizeHalf = new Size(boxSize.Width / 2, boxSize.Height / 2);
        }

        /// <summary>
        /// Gets the point.
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <returns></returns>
        public Point GetPoint(double angle)
        {
            var point = new Point(_boxSizeHalf.Width, _boxSizeHalf.Height);

            var offset = _ellipsePointCalculator.GetPoint(angle);

            point.Offset(offset.X, -offset.Y);

            return point;
        }
    }

    /// <summary>
    /// Calculates ellipse points based on simple math
    /// </summary>
    public class EllipsePointCalculator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EllipsePointCalculator"/> class.
        /// </summary>
        /// <param name="size">The size of the ellipse.</param>
        public EllipsePointCalculator(Size size) => Size = size;

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public Size Size { get; }

        /// <summary>
        /// Gets the point.
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <returns></returns>
        public Point GetPoint(double angle)
        {
            switch (angle)
            {
                case 0:
                case 360:
                    return new Point(Size.Width, 0);

                case 90:
                    return new Point(0, Size.Height);
                case 180:
                    return new Point(-Size.Width, 0);
                case 270:
                    return new Point(0, -Size.Height);

                default:
                    break;
            }

            double x = Size.Width * Math.Cos(angle * Math.PI / 180.0);
            double y = Size.Height * Math.Sin(angle * Math.PI / 180.0);

            return new Point(x, y);
        }
    }
}
