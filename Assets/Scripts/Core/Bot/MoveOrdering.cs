using System;
using System.Collections.Generic;

namespace Chess {
	public class MoveOrdering {
		static readonly int[,] MVV_LVA = {
//			K		Q		R		B		N		P		-		Attacker//Victim
			{ 0,  0,  0,  0,  0,  0,  0 },				// -
			{ 10, 11, 12, 13, 14, 15, 0 },				// P
			{ 20, 21, 22, 23, 24, 25, 0 },				// N
			{ 30, 31, 32, 33, 34, 35, 0 },				// B
			{ 40, 41, 42, 43, 44, 45, 0 },				// R
			{ 50, 51, 52, 53, 54, 55, 0 },				// Q
			{ 0,  0,  0,  0,  0,  0,  0 }					// K
		};
		
		public int BestMoveScoreIndex(int[] board, List<Move> moves, int currIndex) {
			int score = int.MinValue;
			
			for (int moveIndex = currIndex; moveIndex < moves.Count; moveIndex++) {
				if (!moves[moveIndex].IsCapture) continue;

				int movePiece = Piece.PieceType(board[moves[moveIndex].StartSquare]);
				int capturePiece = Piece.PieceType(board[moves[moveIndex].TargetSquare]);
				
				if (score < MVV_LVA[capturePiece, movePiece]) {
					(score, currIndex) = (MVV_LVA[capturePiece, movePiece], moveIndex);
				}
			}

			return currIndex;
		}
		
		public void SwapMoves(List<Move> moves, int toIndex, int fromIndex) {
			(moves[toIndex], moves[fromIndex]) = (moves[fromIndex], moves[toIndex]);
		}
	}
}