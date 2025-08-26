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
		
		public List<Move> ReorderMoves(int[] board, List<Move> moves) {
			int score = int.MinValue;
			int index = -1;
			
			for (int i = 0; i < moves.Count; i++) {
				if (!moves[i].IsCapture) continue;

				int movePiece = Piece.PieceType(board[moves[i].StartSquare]);
				int capturePiece = Piece.PieceType(board[moves[i].TargetSquare]);
				
				if (score < MVV_LVA[capturePiece, movePiece]) {
					score = MVV_LVA[capturePiece, movePiece];
					index = i;
				}
			}
			
			if (index != -1) {
				(moves[0], moves[index]) = (moves[index], moves[0]);
			}
			
			return moves;
		}
	}
}