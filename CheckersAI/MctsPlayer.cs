using System;
using System.Collections.Generic;
using System.Linq;


namespace CheckersAI {
    class MctsPlayer {
        public Move GetMove(CheckersBoard board) {
            BoardNode node = new BoardNode(board, null);
            while (node.Simulations < 1000) {
                node.Simulate();
            }
            return node.GetBestMove();
        }
    }

    class BoardNode {
        private static Random random = new Random();

        private int wins;
        public int Wins {
            get { return wins; }
        }
        private int simulations;
        public int Simulations {
            get { return simulations; }
        }

        public double WlRatio {
            get { return (double) Wins / Simulations; }
        }

        public Move Move { get; }

        public BoardNode(CheckersBoard board, Move move) {
            Current = board;
            Move = move;
        }

        public CheckersBoard Current { get; }

        private List<BoardNode> children;

        internal bool Simulate() {
            if (children == null)
                children = Current.GetMoves().Select(m => new BoardNode(Current.ApplyMove(m), m)).ToList();
            if (Current.Winner == Kind.None) {
                if (children.Count == 0) return false;
                int index = random.Next(children.Count);
                var child = children.ElementAt(index);
                bool win = child.Simulate();
                if (win) {
                    wins++;
                }
                simulations++;
                return win;
            } else {
                return Current.Winner == Kind.Black;
            }
        }

        public Move GetBestMove() {
            return children.Aggregate((a, b) => a.WlRatio > b.WlRatio ? a : b).Move;
        }

        public override string ToString() {
            return Wins + "/" + Simulations + " (" + WlRatio + ") " + Move;
        }
    }
}
