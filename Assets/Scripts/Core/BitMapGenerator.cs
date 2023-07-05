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

		public ulong VerticalAndHorizontalCheckMap;
		public ulong DiagonalCheckMap;
		public ulong KnightCheckMap;
		public ulong PawnCheckMap;

		public ulong MoveSquareInCheckMap;

		void Initialize() {
			OpponentThreatMap = 0;

			VerticalAndHorizontalCheckMap = 0;
			DiagonalCheckMap = 0;
			KnightCheckMap = 0;
			PawnCheckMap = 0;

			MoveSquareInCheckMap = 0;

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
			VerticalAndHorizontalCheckMap |= GenerateSlidingMoveThreats(opponentKingSquare, 0, 4, false);
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

		ulong GenerateSlidingMoveThreats(int startSquare, int startDirIndex, int endDirIndex, bool generateMoveSquareInCheckMap = true) {
			ulong threatMap = 0;
			for (int dirIndex = startDirIndex; dirIndex < endDirIndex; dirIndex++) {
				int currentDirOffset = DirectionOffsets[dirIndex];
				ulong possibleMoveSquareInCheckMap = 1ul << startSquare;
				for (int n = 0; n < NumSquaresToEdge[startSquare][dirIndex]; n++) {
					int targetSquare = startSquare + currentDirOffset * (n + 1);
					int targetSquarePiece = board.Square[targetSquare];
					int targetSquarePieceType = Piece.PieceType(targetSquarePiece);

					possibleMoveSquareInCheckMap |= 1ul << targetSquare;

					// Mark this square as under attack by opponent
					threatMap |= 1ul << targetSquare;

					// Hit a piece
					if (targetSquarePiece != Piece.None) {
						// Get the location of the opponent's sliding piece looking at the king
						if (generateMoveSquareInCheckMap) {
							if (targetSquarePieceType == Piece.King && Piece.IsColor(targetSquarePiece, friendlyColor)) {
								// Double check
								if (InCheck) {
									MoveSquareInCheckMap = 0;
									continue;
								}
								MoveSquareInCheckMap = possibleMoveSquareInCheckMap;
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
						MoveSquareInCheckMap = 0;
						continue;
					}
					MoveSquareInCheckMap |= 1ul << knightSquare;
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
						MoveSquareInCheckMap = 0;
						continue;
					}
					MoveSquareInCheckMap |= 1ul << pawnSquare;
					InCheck = true;
				}
			}
		}
	}
}