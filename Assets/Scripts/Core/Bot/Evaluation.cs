using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Chess {
	// https://www.chessprogramming.org/Simplified_Evaluation_Function
	public class Evaluation {
		public const int PawnValue = 100;
		public const int KnightValue = 320;
		public const int BishopValue = 330;
		public const int RookValue = 500;
		public const int QueenValue = 900;
		public const int KingValue = 20000;

		Board board;

		public int Evaluate(Board board) {
			this.board = board;

			int whiteEval = CalculateMaterialsValue(Piece.White) + EvaluatePieceSquareTables(Piece.White);
			int blackEval = CalculateMaterialsValue(Piece.Black) + EvaluatePieceSquareTables(Piece.Black);

			int eval = whiteEval - blackEval;
			int perspective = board.WhiteToMove ? 1 : -1;

			return perspective * eval;
		}

		int CalculateMaterialsValue(int colorIndex) {
			int materialsValue = 0;
			materialsValue += board.PieceList.GetValue(Piece.Pawn)[colorIndex].Count * PawnValue;
			materialsValue += board.PieceList.GetValue(Piece.Knight)[colorIndex].Count * KnightValue;
			materialsValue += board.PieceList.GetValue(Piece.Bishop)[colorIndex].Count * BishopValue;
			materialsValue += board.PieceList.GetValue(Piece.Rook)[colorIndex].Count * RookValue;
			materialsValue += board.PieceList.GetValue(Piece.Queen)[colorIndex].Count * QueenValue;
			materialsValue += board.PieceList.GetValue(Piece.King)[colorIndex].Count * KingValue;
			return materialsValue;
		}

		int EvaluatePieceSquareTables(int colorIndex) {
			int value = 0;
			bool isWhite = colorIndex == Board.WhiteIndex;
			value += EvaluatePieceSquareTable(PieceSquareTables.Pawns, board.PieceList.GetValue(Piece.Pawn)[colorIndex], isWhite);
			value += EvaluatePieceSquareTable(PieceSquareTables.Knights, board.PieceList.GetValue(Piece.Knight)[colorIndex], isWhite);
			value += EvaluatePieceSquareTable(PieceSquareTables.Bishops, board.PieceList.GetValue(Piece.Bishop)[colorIndex], isWhite);
			value += EvaluatePieceSquareTable(PieceSquareTables.Rooks, board.PieceList.GetValue(Piece.Rook)[colorIndex], isWhite);
			value += EvaluatePieceSquareTable(PieceSquareTables.Queens, board.PieceList.GetValue(Piece.Queen)[colorIndex], isWhite);
			int[] kingTable = IsEndGame() ? PieceSquareTables.KingEndGame : PieceSquareTables.KingMiddleGame;
			value += EvaluatePieceSquareTable(kingTable, board.PieceList.GetValue(Piece.King)[colorIndex], isWhite);
			return value;
		}

		static int EvaluatePieceSquareTable(int[] table, HashSet<int> pieceSquareList, bool isWhite) {
			int value = 0;
			foreach (int pieceSquare in pieceSquareList) {
				value += PieceSquareTables.GetValue(table, pieceSquare, isWhite);
			}
			return value;
		}

		// Both sides have no queens or
    // Every side which has a queen has additionally no other pieces or one minorpiece maximum.
		bool IsEndGame() {
			Dictionary<int, HashSet<int>> knights = board.PieceList.GetValue(Piece.Knight);
			Dictionary<int, HashSet<int>> bishops = board.PieceList.GetValue(Piece.Bishop);
			Dictionary<int, HashSet<int>> rooks = board.PieceList.GetValue(Piece.Rook);
			Dictionary<int, HashSet<int>> queens = board.PieceList.GetValue(Piece.Queen);

			bool boardHasNoQueens = queens[Piece.White].Count == 0 && queens[Piece.Black].Count == 0;

			int numWhiteMinorPieces = knights[Piece.White].Count + bishops[Piece.White].Count;
			bool whiteHasQueenAndMaxOneMinorPiece = queens[Piece.White].Count == 1 && rooks[Piece.White].Count == 0 && numWhiteMinorPieces <= 1;

			int numBlackMinorPieces = knights[Piece.Black].Count + bishops[Piece.Black].Count;
			bool blackHasQueenAndMaxOneMinorPiece = queens[Piece.Black].Count == 1 && rooks[Piece.Black].Count == 0 && numBlackMinorPieces <= 1;

			return boardHasNoQueens || (whiteHasQueenAndMaxOneMinorPiece && blackHasQueenAndMaxOneMinorPiece);
		}
	}
}