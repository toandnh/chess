using System;
using System.Collections.Generic;

using UnityEngine;

namespace Chess {
	public class Search {
		Board board;

		Algorithm algorithm;
		MoveGenerator moveGenerator;

		Move bestMove;
		Move bestMoveThisIteration;

		int bestEval;
		int bestEvalThisIteration;

		bool stopSearch { get; set; }

		public Search(Board board) {
			this.board = board;

			algorithm = new Algorithm();
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
				// int evaluateMove = -algorithm.NegaMax(board, depth);
				int evaluateMove = -algorithm.AlphaBeta(board, depth, int.MinValue, int.MaxValue);
				board.UnmakeMove(move);
				
				if (evaluateMove > bestEval) {
					bestMove = move;
					bestEval = evaluateMove;
				}
			}

			return bestMove;
		}
	}
}