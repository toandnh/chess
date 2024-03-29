using System.Collections.Generic;
using System.Linq;

namespace Chess {
	using static PrecomputedMoveData;
	using static BitBoardUtility;

	public class BitMapGenerator {
		Board board;

		int opponentColor;
		int friendlyColor;

		int opponentColorIndex;

		int opponentKingSquare;
		int friendlyKingSquare;

		public bool InCheck;

		public ulong OpponentThreatMap;

		public ulong OrthogonalCheckMap;
		public ulong DiagonalCheckMap;
		public ulong KnightCheckMap;
		public ulong PawnCheckMap;

		public ulong SquaresInCheckRayMap;

		void Initialize() {
			OpponentThreatMap = 0;

			OrthogonalCheckMap = 0;
			DiagonalCheckMap = 0;
			KnightCheckMap = 0;
			PawnCheckMap = 0;

			SquaresInCheckRayMap = 0;

			InCheck = false;

			opponentColor = board.OpponentColor;
			friendlyColor = opponentColor == Piece.White ? Piece.Black : Piece.White;

			opponentColorIndex = opponentColor == Piece.White ? Board.WhiteIndex : Board.BlackIndex;

			Dictionary<int, HashSet<int>> kings = board.PieceList.GetValue(Piece.King);
			friendlyKingSquare = kings[friendlyColor].ToArray()[0];
			opponentKingSquare = kings[opponentColor].ToArray()[0];
		}

		public void GenerateBitMaps(Board board) {
			this.board = board;

			Initialize();

			GeneratePosibleCheckMap();
			GenerateThreatMap();
		}

		void GeneratePosibleCheckMap() {
			OrthogonalCheckMap |= GenerateSlidingMoveThreats(opponentKingSquare, 0, 4, false);
			DiagonalCheckMap |= GenerateSlidingMoveThreats(opponentKingSquare, 4, 8, false);

			KnightCheckMap = KnightAttackBitBoard[opponentKingSquare];
			PawnCheckMap = PawnAttackBitBoard[opponentKingSquare][opponentColorIndex];
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

		ulong GenerateSlidingMoveThreats(int startSquare, int startDirIndex, int endDirIndex, bool generateSquaresInCheckRayMap = true) {
			ulong threatMap = 0;
			for (int dirIndex = startDirIndex; dirIndex < endDirIndex; dirIndex++) {
				int currentDirOffset = DirectionOffsets[dirIndex];
				ulong possibleSquaresInCheckRayMap = 1ul << startSquare;
				for (int n = 0; n < NumSquaresToEdge[startSquare][dirIndex]; n++) {
					int targetSquare = startSquare + currentDirOffset * (n + 1);
					int targetSquarePiece = board.Square[targetSquare];
					int targetSquarePieceType = Piece.PieceType(targetSquarePiece);

					possibleSquaresInCheckRayMap |= 1ul << targetSquare;

					// Mark this square as under attack by opponent
					threatMap |= 1ul << targetSquare;

					// Hit a piece
					if (targetSquarePiece != Piece.None) {
						// Get the location of the opponent's sliding piece looking at the king
						if (generateSquaresInCheckRayMap) {
							if (targetSquarePieceType == Piece.King && Piece.IsColor(targetSquarePiece, friendlyColor)) {
								// Double check
								if (InCheck) {
									SquaresInCheckRayMap = 0;
									continue;
								}

								// King cannot move forward or backward in this direction
								if (++n < NumSquaresToEdge[startSquare][dirIndex]) {
									int squareBehindKing = startSquare + currentDirOffset * (n + 1);
									threatMap |= 1ul << squareBehindKing;
								}

								SquaresInCheckRayMap = possibleSquaresInCheckRayMap;
								InCheck = true;
							}
						}
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

				// Get the location of the opponent's knight looking at the king
				if (HasSquare(KnightAttackBitBoard[knightSquare], friendlyKingSquare)) {
					// Double check
					if (InCheck) {
						SquaresInCheckRayMap = 0;
						continue;
					}
					SquaresInCheckRayMap |= 1ul << knightSquare;
					InCheck = true;
				}
			}
		}

		void GeneratePawnThreats() {
			HashSet<int> pawns = board.PieceList.GetValue(Piece.Pawn)[opponentColor];
			foreach (int pawnSquare in pawns) {
				OpponentThreatMap |= PawnAttackBitBoard[pawnSquare][opponentColorIndex];

				// Get the location of the opponent's pawn looking at the king
				if (HasSquare(PawnAttackBitBoard[pawnSquare][opponentColorIndex], friendlyKingSquare)) {
					// Double check
					if (InCheck) {
						SquaresInCheckRayMap = 0;
						continue;
					}
					SquaresInCheckRayMap |= 1ul << pawnSquare;
					InCheck = true;
				}
			}
		}
	}
}