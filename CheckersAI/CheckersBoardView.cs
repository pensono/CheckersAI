using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CheckersAI {
    public class CheckersBoardView : Control, INotifyPropertyChanged {
        //Properties
        private CheckersBoard board = CheckersBoard.GetInitialBoard();
        public CheckersBoard Board {
            get { return board; }
            set {
                board = value;
                NotifyPropertyChanged("Board");
                InvalidateVisual();
            }
        }

        private Square selectedSquare;
        public Square SelectedSquare {
            get { return selectedSquare; }
            set {
                selectedSquare = value;
                NotifyPropertyChanged("SelectedSquare");
                InvalidateVisual();
            }
        }

        private IEnumerable<Move> displayedMoves = Enumerable.Empty<Move>();
        public IEnumerable<Move> DisplayedMoves {
            get { return displayedMoves; }
            set {
                displayedMoves = value;
                NotifyPropertyChanged("DisplayedMoves");
                InvalidateVisual();
            }
        }

        static CheckersBoardView() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckersBoardView), new FrameworkPropertyMetadata(typeof(CheckersBoardView)));
        }

        public event EventHandler<Square> OnSquareClicked;
        protected virtual void OnRaiseSquareClicked(Square location) {
            if (OnSquareClicked != null) {
                OnSquareClicked(this, location);
            }
        }


        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);

            var position = e.GetPosition(this);
            int x = (int)(position.X / (RenderSize.Width / 8));
            int y = (int)(position.Y / (RenderSize.Height / 8));
            if (CheckersBoard.IsValidLocation(x, y))
                OnRaiseSquareClicked(new Square(x, y));
        }


        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);

            var rectangleWidth = RenderSize.Width / 8;
            var rectangleHeight = RenderSize.Height / 8;

            var rect = new Rect(0, 0, rectangleWidth, rectangleHeight);

            //Draw board
            foreach (Square square in Square.BoardEnumerable()) {
                rect.X = square.X * rectangleWidth;
                rect.Y = square.Y * rectangleHeight;
                drawingContext.DrawRectangle(Brushes.DarkGray, null, rect);
            }

            //Draw selection
            if (SelectedSquare != null) {
                rect.X = SelectedSquare.X * rectangleWidth;
                rect.Y = SelectedSquare.Y * rectangleHeight;
                drawingContext.DrawRectangle(Brushes.Gold, null, rect);
            }

            var linePen = new Pen(Brushes.Black, 1.0);
            //Draw lines
            for (int x = 0; x <= 8; x++) {
                drawingContext.DrawLine(linePen, new Point(x * rectangleWidth, 0), new Point(x * rectangleWidth, RenderSize.Height));
            }
            for (int y = 0; y <= 8; y++) {
                drawingContext.DrawLine(linePen, new Point(0, y * rectangleHeight), new Point(RenderSize.Width, y * rectangleHeight));
            }

            var borderPen = new Pen(Brushes.Gray, 1.0);
            //Draw pieces
            foreach (Square square in Square.BoardEnumerable()) {
                Brush brush;
                switch (Board[square].GetKind()) {
                    case Kind.White:
                        brush = Brushes.White; break;
                    case Kind.Black:
                        brush = Brushes.Black; break;
                    default:
                        continue;
                }
                var center = CenterPoint(square, rectangleWidth, rectangleHeight);
                drawingContext.DrawEllipse(brush, borderPen, center, rectangleWidth * .4, rectangleHeight * .4);

                if (Board[square].IsKing()) {
                    center.Offset(-rectangleWidth * .15, -rectangleHeight * .15);
                    var crown = new Rect(center, new Size(rectangleWidth * .3, rectangleHeight * .3));
                    drawingContext.DrawRectangle(Brushes.Gold, borderPen, crown);
                }
            }

            var movePen = new Pen(Brushes.LightBlue, 3);
            //Draw possible moves
            foreach (var move in DisplayedMoves) {
                var dest = CenterPoint(move.End, rectangleWidth, rectangleHeight);
                drawingContext.DrawLine(movePen, CenterPoint(move.Start, rectangleWidth, rectangleHeight), dest);
                drawingContext.DrawEllipse(Brushes.LightBlue, movePen, dest, rectangleWidth * .1, rectangleHeight * .1);
            }
        }

        private Point CenterPoint(Square location, double rectangleWidth, double rectangleHeight) {
            return new Point((location.X + .5) * rectangleWidth, (location.Y + .5) * rectangleHeight);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
