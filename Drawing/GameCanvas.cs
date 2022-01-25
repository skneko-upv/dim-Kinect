using DIM_Kinect7.Model;
using System;
using System.Globalization;
using System.Timers;
using System.Windows;
using System.Windows.Media;

namespace DIM_Kinect7.Drawing
{
    class GameCanvas
    {
        public ImageSource ImageSource => imageSource;

        readonly GameState state;

        readonly DrawingGroup drawingGroup;
        readonly DrawingImage imageSource;
        readonly int width, height;

        int CenterX => width / 2;
        int CenterY => height / 2;
        Point Center => new Point(CenterX, CenterY);

        readonly Timer resetBrushTimer = new Timer(1500);
        readonly Brush defaultBrush = Brushes.White;
        readonly Brush passBrush = Brushes.Green;
        readonly Brush failBrush = Brushes.Red;
        Brush currentBrush;

        const double arrowLength = 180;
        const double arrowHeadBase = 36;
        const double arrowThickness = 10;

        readonly Typeface normalTypeface = new Typeface("Arial");

        public GameCanvas(GameState state, DrawingGroup drawingGroup, int width, int height)
        {
            this.state = state;
            state.CutPassed += State_CutPassed;
            state.CutFailed += State_CutFailed;

            this.drawingGroup = drawingGroup;
            imageSource = new DrawingImage(drawingGroup);
            this.width = width;
            this.height = height;

            currentBrush = defaultBrush;
            resetBrushTimer.Elapsed += (_sender, _args) => currentBrush = defaultBrush;
            resetBrushTimer.AutoReset = false;
        }

        public void Render()
        {
            using (var context = drawingGroup.Open()) {
                // Draw a transparent background to set the render size
                context.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, width, height));

                DrawCurrentCut(context, state.CurrentCut);
                DrawScore(context);
                DrawTimer(context);

                // Prevent drawing outside bounds
                drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, width, height));
            }
        }

        void DrawCurrentCut(DrawingContext context, CutKind cut)
        {
            Pen arrowPen = new Pen(currentBrush, arrowThickness)
            {
                StartLineCap = PenLineCap.Square,
                EndLineCap = PenLineCap.Flat,
            };

            switch (cut) {
                case CutKind.LineUp: { DrawArrowUp(context, arrowPen); break; }
                case CutKind.LineLeft: { DrawArrowLeft(context, arrowPen); break; }
                case CutKind.LineRight: { DrawArrowRight(context, arrowPen); break; }
                case CutKind.CircleClockwise: { DrawArrowClockwise(context, arrowPen); break; }
                case CutKind.CircleCounterclockwise: { DrawArrowCounterclockwise(context, arrowPen); break; }
            }
        }

        void DrawArrowUp(DrawingContext context, Pen arrowPen)
        {
            var arrowTip = new Point(CenterX, CenterY - arrowLength);
            context.DrawLine(arrowPen, Center, arrowTip);
            context.DrawLine(arrowPen, arrowTip, arrowTip + new Vector(arrowHeadBase, arrowHeadBase));
            context.DrawLine(arrowPen, arrowTip, arrowTip + new Vector(-arrowHeadBase, arrowHeadBase));
        }

        void DrawArrowLeft(DrawingContext context, Pen arrowPen)
        {
            var arrowTip = new Point(CenterX - arrowLength, CenterY);
            context.DrawLine(arrowPen, Center, arrowTip);
            context.DrawLine(arrowPen, arrowTip, arrowTip + new Vector(arrowHeadBase, arrowHeadBase));
            context.DrawLine(arrowPen, arrowTip, arrowTip + new Vector(arrowHeadBase, -arrowHeadBase));
        }

        void DrawArrowRight(DrawingContext context, Pen arrowPen)
        {
            var arrowTip = new Point(CenterX + arrowLength, CenterY);
            context.DrawLine(arrowPen, Center, arrowTip);
            context.DrawLine(arrowPen, arrowTip, arrowTip - new Vector(arrowHeadBase, arrowHeadBase));
            context.DrawLine(arrowPen, arrowTip, arrowTip - new Vector(arrowHeadBase, -arrowHeadBase));
        }

        void DrawArrowClockwise(DrawingContext context, Pen arrowPen)
        {
            var radius = arrowLength * 2 / 3;
            var arrowTip = Center - new Vector(radius, 20);
            context.DrawEllipse(null, arrowPen, Center, radius, radius);
            context.DrawLine(arrowPen, arrowTip, arrowTip + new Vector(arrowHeadBase, arrowHeadBase));
            context.DrawLine(arrowPen, arrowTip, arrowTip + new Vector(-arrowHeadBase, arrowHeadBase));
        }

        void DrawArrowCounterclockwise(DrawingContext context, Pen arrowPen)
        {
            var radius = arrowLength * 2 / 3;
            var arrowTip = Center + new Vector(radius, 20);
            context.DrawEllipse(null, arrowPen, Center, radius, radius);
            context.DrawLine(arrowPen, arrowTip, arrowTip - new Vector(arrowHeadBase, arrowHeadBase));
            context.DrawLine(arrowPen, arrowTip, arrowTip - new Vector(-arrowHeadBase, arrowHeadBase));
        }

        void DrawScore(DrawingContext context)
        {
            context.DrawText(NormalText(state.Score.ToString(), 80, currentBrush), new Point(30, 30));
        }

        void DrawTimer(DrawingContext context)
        {
            var text = state.Timer.Elapsed.ToString(@"m\:ss");
            context.DrawText(NormalText(text, 40, Brushes.White), new Point(width - 110, 30));
        }

        void State_CutPassed()
        {
            currentBrush = passBrush;
            resetBrushTimer.Start();

        }

        void State_CutFailed()
        {
            currentBrush = failBrush;
            resetBrushTimer.Start();
        }

        FormattedText NormalText(string text, int size, Brush color) =>
            new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, normalTypeface, size, color, 1.25);
    }
}
