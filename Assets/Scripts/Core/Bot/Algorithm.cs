using System;
using System.Collections.Generic;

namespace Chess {
	public class Algorithm {
		Evaluation evaluation;
		MoveGenerator moveGenerator;
		
		public Algorithm() {
			evaluation = new Evaluation();
			moveGenerator = new MoveGenerator();
		}
		
		public int NegaMax(Board board, int depth) {
			if (depth == 0) return evaluation.Evaluate(board);

			int value = int.MinValue;

			List<Move> moves = moveGenerator.GenerateMoves(board);
			foreach (Move move in moves) {
				board.MakeMove(move);
				value = Math.Max(value, -NegaMax(board, depth - 1));
				board.UnmakeMove(move);
			}

			return value;
		}

		public int NegaMaxAlphaBeta(Board board, int depth, int alpha, int beta, int color) {
			if (depth == 0) return evaluation.Evaluate(board);

			int value = int.MinValue;

			return value;
		}
	}
}