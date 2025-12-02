using System;
using System.Collections.Generic;

using static System.Math;

namespace Chess {
	public class Algorithm {
		Evaluation evaluation;
		MoveGenerator moveGenerator;
		MoveOrdering moveOrdering;
		
		public Algorithm() {
			evaluation = new Evaluation();
			moveGenerator = new MoveGenerator();
			moveOrdering = new MoveOrdering();
		}
		
		public int NegaMax(Board board, int depth) {
			if (depth == 0) return evaluation.Evaluate(board);

			List<Move> moves = moveGenerator.GenerateMoves(board);
			if (moves.Count == 0) return evaluation.Evaluate(board);
			
			int value = int.MinValue;
			
			foreach (Move move in moves) {
				board.MakeMove(move);
				value = Max(value, -NegaMax(board, depth - 1));
				board.UnmakeMove(move);
			}

			return value;
		}

		public int AlphaBeta(Board board, int depth, int alpha, int beta) {
			if (depth == 0) return evaluation.Evaluate(board);

			List<Move> moves = moveGenerator.GenerateMoves(board);
			if (moves.Count == 0) return evaluation.Evaluate(board);

			int[] movesScore = moveOrdering.GetMovesScore(board.Square, 0, moves);
			
			int value = int.MinValue;
			
			for (int moveIndex = 0; moveIndex < moves.Count; moveIndex++) {
				// moveOrdering.SwapMoves(moves, moveIndex, moveOrdering.BestMoveScoreIndex(board.Square, moves, moveIndex));
				moveOrdering.PickAndSwapMoves(moves, movesScore, moveIndex);

				board.MakeMove(moves[moveIndex]);

				value = Max(value, -AlphaBeta(board, depth - 1, -beta, -alpha));

				board.UnmakeMove(moves[moveIndex]);

				alpha = Max(alpha, value);
				if (alpha >= beta) break;
			}

			return value;
		}
	}
}