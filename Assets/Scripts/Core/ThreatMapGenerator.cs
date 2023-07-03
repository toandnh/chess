using System.Collections.Generic;
using System.Linq;

namespace Chess {
	using static PrecomputedMoveData;

	public class ThreatMapGenerator {
		Board board;

		int opponentColor;
		int opponentKingSquare;

		public ulong OpponentThreatMap;
		public ulong VerticalAndHorizontalCheckMap;
		public ulong DiagonalCheckMap;
		public ulong KnightCheckMap;

		void Initialize() {
			OpponentThreatMap = 0;
			VerticalAndHorizontalCheckMap = 0;
			DiagonalCheckMap = 0;
			KnightCheckMap = 0;

			opponentColor = board.OpponentColor;

			HashSet<int> king = board.PieceList.GetValue(Piece.King)[opponentColor];
			opponentKingSquare = king.ToArray()[0]; // There can only be one king of each color
		}

		public void GenerateThreatMaps(Board board) {
			this.board = board;

			Initialize();

			GeneratePosibleCheckMap();
			GenerateThreatMap();
		}

		void GeneratePosibleCheckMap() {
			VerticalAndHorizontalCheckMap |= GenerateSlidingMoveThreats(opponentKingSquare, 0, 4);
			DiagonalCheckMap |= GenerateSlidingMoveThreats(opponentKingSquare, 4, 8);

			KnightCheckMap = KnightAttackBitBoard[opponentKingSquare];
		}

		void GenerateThreatMap() {
			// Generate sliding piece threats
			HashSet<int> rooks = board.PieceList.GetValue(Piece.Rook)[opponentColor];
			foreach (int rookSquare in rooks) {
				OpponentThreatMap |= GenerateSlidingMoveThreats(rookSquare, 0, 4);
			}

			HashSet<int> bishops = board.PieceList.GetValue(Piece.Bishop)[opponentColor];
			foreach (int bishopSquare in bishops) {
				OpponentThreatMap |= GenerateSlidingMoveThreats(bishopSquare, 4, 8);
			}

			HashSet<int> queens = board.PieceList.GetValue(Piece.Queen)[opponentColor];
			foreach (int queenSquare in queens) {
				OpponentThreatMap |= GenerateSlidingMoveThreats(queenSquare, 0, 8);
			}

			// Generate other pieces threats
			GenerateKingThreats();
			GenerateKnightThreats();
			GeneratePawnThreats();
		}

		ulong GenerateSlidingMoveThreats(int startSquare, int startDirIndex, int endDirIndex) {
			ulong threatMap = 0;
			for (int dirIndex = startDirIndex; dirIndex < endDirIndex; dirIndex++) {
				int currentDirOffset = DirectionOffsets[dirIndex];
				for (int n = 0; n < NumSquaresToEdge[startSquare][dirIndex]; n++) {
					int targetSquare = startSquare + currentDirOffset * (n + 1);
					int targetSquarePiece = board.Square[targetSquare];

					// Mark this square as under attack by opponent
					threatMap |= 1ul << targetSquare;

					// Hit a piece, stop
					if (targetSquarePiece != Piece.None) {
						break;
					}
				}
			}
			return threatMap;
		}

		void GenerateKingThreats() {
			OpponentThreatMap |= KingAttackBitBoard[opponentKingSquare];
		}

		void GenerateKnightThreats() {
			HashSet<int> knights = board.PieceList.GetValue(Piece.Knight)[opponentColor];
			foreach (int knightSquare in knights) {
				OpponentThreatMap |= KnightAttackBitBoard[knightSquare];
			}
		}

		void GeneratePawnThreats() {
			HashSet<int> pawns = board.PieceList.GetValue(Piece.Pawn)[opponentColor];
			foreach (int pawnSquare in pawns) {
				int opponentIndex = opponentColor == Piece.White ? Board.WhiteIndex : Board.BlackIndex;
				OpponentThreatMap |= PawnAttackBitBoard[pawnSquare][opponentIndex];
			}
		}
	}
}