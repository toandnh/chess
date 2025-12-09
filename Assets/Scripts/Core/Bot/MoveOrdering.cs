using System;
using System.Collections.Generic;

namespace Chess {
	public class MoveOrdering {
		static readonly int[, ] MVV_LVA = {
//			K		Q		R		B		N		P		-		Attacker//Victim
			{ 0,  0,  0,  0,  0,  0,  0 },				// -
			{ 10, 11, 12, 13, 14, 15, 0 },				// P
			{ 20, 21, 22, 23, 24, 25, 0 },				// N
			{ 30, 31, 32, 33, 34, 35, 0 },				// B
			{ 40, 41, 42, 43, 44, 45, 0 },				// R
			{ 50, 51, 52, 53, 54, 55, 0 },				// Q
			{ 0,  0,  0,  0,  0,  0,  0 }					// K
		};
		
		const int MVV_LVA_OFFSET = int.MaxValue - 256;

		const int MAX_KILLER_MOVES_PER_PLY = 2;
		const int MAX_PLY = 512;

		const int KILLER_MOVE_VALUE = 10;

		public Move[, ] KillerMoves { get; set; }
		
		public MoveOrdering() {
			KillerMoves = new Move[MAX_PLY, MAX_KILLER_MOVES_PER_PLY];
		}

		public int BestMoveScoreIndex(int[] board, List<Move> moves, int currIndex) {
			int score = int.MinValue;

			for (int moveIndex = currIndex; moveIndex < moves.Count; moveIndex++) {
				if (moves[moveIndex].IsCapture) {
					int movePiece = Piece.PieceType(board[moves[moveIndex].StartSquare]);
					int capturePiece = Piece.PieceType(board[moves[moveIndex].TargetSquare]);

					if (score < MVV_LVA[capturePiece, movePiece]) {
						(score, currIndex) = (MVV_LVA[capturePiece, movePiece], moveIndex);
					}
				}
			}

			return currIndex;
		}
		
		public int[] GetMovesScore(int[] board, int ply, List<Move> moves) {
			int[] movesScore = new int[moves.Count];

			for (int moveIndex = 0; moveIndex < moves.Count; moveIndex++) {
				int score = 0;
				if (moves[moveIndex].IsCapture) {
					int movePiece = Piece.PieceType(board[moves[moveIndex].StartSquare]);
					int capturePiece = Piece.PieceType(board[moves[moveIndex].TargetSquare]);

					score = MVV_LVA_OFFSET + MVV_LVA[capturePiece, movePiece];
				} else {
					for (int killerMoveIndex = 0; killerMoveIndex < MAX_KILLER_MOVES_PER_PLY; killerMoveIndex++) {
						if (moves[moveIndex] == KillerMoves[ply, killerMoveIndex]) {
							score = MVV_LVA_OFFSET - (killerMoveIndex + 1) * KILLER_MOVE_VALUE;
						}
					}
				}
				movesScore[moveIndex] = score;
			}

			return movesScore;
		}
		
		public void PickAndSwapMoves(List<Move> moves, int[] movesScore, int currIndex) {
			int currScore = movesScore[currIndex];
			int chosenIndex = currIndex;

			for (int moveIndex = currIndex; moveIndex < movesScore.Length; moveIndex++) {
				chosenIndex = currScore < movesScore[moveIndex] ? moveIndex : currIndex; 
			}
			
			(moves[currIndex], moves[chosenIndex]) = (moves[chosenIndex], moves[currIndex]);
		}
		
		public void StoreKillerMove(int ply, Move move) {
			if (move != KillerMoves[ply, 0]) {
				(KillerMoves[ply, 0], KillerMoves[ply, 1]) = (move, KillerMoves[ply, 0]);
			}
		}
	}
}