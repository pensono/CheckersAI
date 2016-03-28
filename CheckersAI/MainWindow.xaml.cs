using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace CheckersAI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            BoardComponent.PropertyChanged += Board_PropertyChanged;
            BoardComponent.OnSquareClicked += BoardComponent_OnSquareClicked;
        }

        private void BoardComponent_OnSquareClicked(object sender, Square square) {
            if (BoardComponent.SelectedSquare == null) {
                if (BoardComponent.Board[square].GetKind() == BoardComponent.Board.Turn.AsKind()) {
                    BoardComponent.SelectedSquare = square;
                }
            } else {
                var move = new Move(BoardComponent.SelectedSquare, square);
                if (BoardComponent.Board.GetMoves().Any(m => move.Equals(m))) {
                    BoardComponent.Board = BoardComponent.Board.ApplyMove(move);
                    BoardComponent.SelectedSquare = null;

                } else {
                    if (BoardComponent.Board[square].GetKind() == BoardComponent.Board.Turn.AsKind())
                        BoardComponent.SelectedSquare = square;
                }
            }
        }

        private void Board_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Board") {
                if (BoardComponent.Board.Turn == Color.Black) {
                    //Simulate
                    var simulationThread = new BackgroundWorker();
                    simulationThread.DoWork += CalculateComputerMove;
                    simulationThread.RunWorkerCompleted += PublishComputerMove;
                    simulationThread.RunWorkerAsync();
                }
            }
        }

        private void CalculateComputerMove(object sender, DoWorkEventArgs e) {
            var player = new MctsPlayer();
            Move m = player.GetMove(BoardComponent.Board);
            e.Result = BoardComponent.Board.ApplyMove(m);
        }

        private void PublishComputerMove(object sender, RunWorkerCompletedEventArgs e) {
            BoardComponent.Board = (CheckersBoard)e.Result;
        }
    }
}
