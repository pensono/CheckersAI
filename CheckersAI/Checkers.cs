using System;
using System.Collections.Generic;
using System.Linq;

namespace CheckersAI {
    public class CheckersBoard {
        private SquareState[] board;

        private Color turn;
        public Color Turn { get { return turn; } }

        private Square jumpingPiece;

        public CheckersBoard() {
            board = new SquareState[32];
        }

        public CheckersBoard(CheckersBoard other) {
            board = other.board;
            turn = other.turn;
        }

        public SquareState this[Square location] {
            get {
                return board[location.BoardIndex];
            }
        }

        public static bool IsValidLocation(int x, int y) {
            return (x + y) % 2 == 0;
        }

        public IEnumerable<Move> GetMoves() {
            //TODO sort this out. Maybe refactor potential moves into the moves class?

            int direction = Turn.GetDirection();

            var values = new List<Move>();

            if (jumpingPiece != null) {
                if (IsValidCoordinate(jumpingPiece.Y + direction)) {
                    values.AddRange(GetPotentialJumps(jumpingPiece, direction));
                }

                if (this[jumpingPiece].IsKing()) {
                    if (IsValidCoordinate(jumpingPiece.Y - direction)) {
                        values.AddRange(GetPotentialJumps(jumpingPiece, -direction));
                    }
                }
            }

            foreach (Square square in Square.BoardEnumerable()) {
                if (this[square].GetKind() != Turn.AsKind()) continue;
                if (IsValidCoordinate(square.Y + direction)) {
                    values.AddRange(GetPotentialMoves(square, direction));
                    if (IsValidCoordinate(square.Y + direction * 2)) {
                        values.AddRange(GetPotentialJumps(square, direction));
                    }
                }


                if (this[square].IsKing()) {
                    if (IsValidCoordinate(square.Y - direction)) {
                        values.AddRange(GetPotentialMoves(square, -direction));

                        if (IsValidCoordinate(square.Y - direction * 2)) {
                            values.AddRange(GetPotentialJumps(square, -direction));
                        }
                    }
                }
            }

            if (values.Any(move => move.IsJump))
                values = values.Where(move => move.IsJump).ToList();
            return values;
        }

        private bool IsValidCoordinate(int c) {
            return c < 8 && c >= 0;
        }

        private IEnumerable<Move> GetPotentialMoves(Square start, int direction) {
            int spaces = 1;
            if (start.X - spaces >= 0) {
                var test = new Move(start, -spaces, direction * spaces);
                if (IsValidMove(test)) yield return test;
            }
            if (start.X + spaces < 8) {
                var test = new Move(start, spaces, direction * spaces);
                if (IsValidMove(test)) yield return test;
            }
        }

        private IEnumerable<Move> GetPotentialJumps(Square start, int direction) {
            int spaces = 2;
            if (start.X - spaces >= 0) {
                var test = new Move(start, -spaces, direction * spaces);
                if (IsValidMove(test) && IsValidJump(test)) yield return test;
            }
            if (start.X + spaces < 8) {
                var test = new Move(start, spaces, direction * spaces);
                if (IsValidMove(test) && IsValidJump(test)) yield return test;
            }
        }

        public bool IsValidMove(Move move) {
            //TODO clean this ship uppp
            if (this[move.End] == SquareState.None) {
                return true;
            }
            return false;
        }

        public bool IsValidJump(Move move) {
            //TODO clean this ship uppp
            if (this[move.JumpedSquare].GetKind() == Turn.GetOther().AsKind()) {
                return true;
            }
            return false;
        }


        public CheckersBoard ApplyMove(Move move) {
            CheckersBoard result = new CheckersBoard(this);
            result.board[move.End.BoardIndex] = result[move.Start];
            result.board[move.Start.BoardIndex] = SquareState.None;

            if ((move.End.Y == 0 || move.End.Y == 7) && !result[move.End].IsKing()) {
                result.board[move.End.BoardIndex] = result[move.End].AsKing();
            }


            if (move.IsJump) {
                result.board[move.JumpedSquare.BoardIndex] = SquareState.None;
                if (result.CanJump(move.End)) {
                    result.jumpingPiece = move.End;
                    result.turn = Turn;
                } else {
                    result.turn = Turn.GetOther();
                }
            } else {
                result.turn = Turn.GetOther();
            }

            return result;
        }

        private bool CanJump(Square square) {
            //TODO Clean dis plz
           int direction = Turn.GetDirection();

            if (IsValidCoordinate(square.Y + direction * 2)) {
                if (square.X - 2 >= 0) {
                    var test = new Move(square, -2, direction * 2);
                    if (IsValidMove(test) && IsValidJump(test)) return true;
                }
                if (square.X + 2 < 8) {
                    var test = new Move(square, 2, direction * 2);
                    if (IsValidMove(test) && IsValidJump(test)) return true;
                }
            }
            if (this[square].IsKing()) {
                direction = -direction;
                if (IsValidCoordinate(square.Y - direction * 2)) {
                    if (square.X - 2 >= 0) {
                        var test = new Move(square, -2, -direction * 2);
                        if (IsValidMove(test) && IsValidJump(test)) return true;
                    }
                    if (square.X + 2 < 8) {
                        var test = new Move(square, 2, -direction * 2);
                        if (IsValidMove(test) && IsValidJump(test)) return true;
                    }
                }
            }
            return false;
        }

        public static CheckersBoard GetInitialBoard() {
            var board = new CheckersBoard();
            board.fillRow(0, 0, SquareState.Black);
            board.fillRow(1, 1, SquareState.Black);
            board.fillRow(2, 0, SquareState.Black);

            board.fillRow(5, 1, SquareState.White);
            board.fillRow(6, 0, SquareState.White);
            board.fillRow(7, 1, SquareState.White);

            return board;
        }

        private void fillRow(int row, int start, SquareState state) {
            for (int x = start; x < 8; x += 2) {
                board[x / 2 + row * 4] = state;
            }
        }
    }

    public class Move {
        public Square Start { get; }
        public Square End { get; }
        public bool IsJump {
            get {
                return Math.Abs(Start.X - End.X) > 1;
            }
        }

        public Square JumpedSquare {
            get {
                if (!IsJump) throw new InvalidOperationException("Cannot ask for the jumped square of a move with no jump." + this);
                return new Square((Start.X + End.X) / 2, (Start.Y + End.Y) / 2);
            }
        }

        public Move(Square start, Square end) {
            Start = start;
            End = end;
        }

        public Move(Square start, int dx, int dy) {
            Start = start;
            End = new Square(start.X + dx, start.Y + dy);
        }

        public override string ToString() {
            return Start + " -> " + End;
        }
    }

    public class Square {
        public int X { get; }
        public int Y { get; }

        public int BoardIndex {
            get {
                return X / 2 + Y * 4;
            }
        }

        public Square(int x, int y) {
            if (!CheckersBoard.IsValidLocation(x, y)) throw new ArgumentException("Invalid location: " + x + ", " + y);
            X = x;
            Y = y;
        }

        public override string ToString() {
            return "(" + X + "," + Y + ")";
        }

        public static IEnumerable<Square> BoardEnumerable() {
            for (int y = 0; y < 8; y++) {
                for (int x = 0; x < 8; x++) {
                    if (CheckersBoard.IsValidLocation(x, y)) {
                        yield return new Square(x, y);
                    }
                }
            }
            yield break;
        }
    }

    public enum SquareState : byte {
        None, White, Black, WhiteKing, BlackKing
    }

    public enum Kind {
        White, Black, None
    }

    public enum Color {
        White, Black
    }

    public static class CheckersExtentions {
        public static int GetValue(this SquareState state) {
            switch (state) {
                case SquareState.None: return 0;
                case SquareState.Black: return -1;
                case SquareState.BlackKing: return -3;
                case SquareState.White: return 1;
                case SquareState.WhiteKing: return 3;
            }
            throw new ArgumentException("Invalid square state when trying to find its value: " + state);
        }

        public static Kind GetKind(this SquareState state) {
            switch (state) {
                case SquareState.None: return Kind.None;
                case SquareState.Black: return Kind.Black;
                case SquareState.BlackKing: return Kind.Black;
                case SquareState.White: return Kind.White;
                case SquareState.WhiteKing: return Kind.White;
            }
            throw new ArgumentException("Invalid square state when trying to find its kind: " + state);
        }

        public static SquareState AsKing(this SquareState state) {
            switch (state) {
                case SquareState.Black: return SquareState.BlackKing;
                case SquareState.White: return SquareState.WhiteKing;
            }
            throw new ArgumentException("Invalid square state when trying to crown it: " + state);
        }

        public static bool IsKing(this SquareState state) {
            switch (state) {
                case SquareState.None: return false;
                case SquareState.Black: return false;
                case SquareState.BlackKing: return true;
                case SquareState.White: return false;
                case SquareState.WhiteKing: return true;
            }
            throw new ArgumentException("Invalid square state when trying to find if it's a king: " + state);
        }

        public static Kind AsKind(this Color color) {
            switch (color) {
                case Color.Black:
                    return Kind.Black;
                case Color.White:
                    return Kind.White;
            }
            throw new ArgumentException("Invalid color when trying to find its kind: " + color);
        }

        public static Color GetOther(this Color color) {
            switch (color) {
                case Color.Black:
                    return Color.White;
                case Color.White:
                    return Color.Black;
            }
            throw new ArgumentException("Invalid color when trying to find its other: " + color);
        }

        public static int GetDirection(this Color color) {
            switch (color) {
                case Color.Black:
                    return 1;
                case Color.White:
                    return -1;
            }
            throw new ArgumentException("Invalid color when trying to find its other: " + color);
        }
    }
}