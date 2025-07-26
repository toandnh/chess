using System;
using System.Collections.Generic;

using UnityEngine;

namespace Chess {
	public class Search {
		Board board;
		Evaluation evaluation;
		MoveGenerator moveGenerator;

		Move bestMove;
		Move bestMoveThisIteration;
		int bestEval;
		int bestEvalThisIteration;

		bool stopSearch { get; set; }

		public Search(Board board) {
			this.board = board;

			evaluation = new Evaluation();
			moveGenerator = new MoveGenerator();

			bestMove = bestMoveThisIteration = Move.InvalidMove;
			bestEval = bestEvalThisIteration = 0;
			
			stopSearch = false;
		}

		public Move FindMove(int depth) {
			List<Move> moves = moveGenerator.GenerateMoves(board);

			bestMove = Move.InvalidMove;
			bestEval = int.MinValue;

			foreach (Move move in moves) { 
				board.MakeMove(move);
				int evaluateMove = -NegaMax(board, depth);
				board.UnmakeMove(move);
				
				if (evaluateMove > bestEval) {
					bestMove = move;
					bestEval = evaluateMove;
				}
			}

			return bestMove;
		}

		int NegaMax(Board board, int depth) {
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
	}
}