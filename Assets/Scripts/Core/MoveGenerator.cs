using System.Collections.Generic;
using System.Linq;

using static System.Math;

namespace Chess {
	using static BoardRepresentation;
	using static PrecomputedMoveData;
	using static MoveGeneratorUtility;
	using static BitBoardUtility;

	public class MoveGenerator {
		List<Move> moves;

		Board board;

		bool whiteToMove;

		int friendlyColor;
		int opponentColor;
		
		int friendlyColorIndex;

		int friendlyKingSquare;
		int opponentKingSquare;

		ulong opponentThreatMap;

		ulong verticalAndHorizontalCheckMap;
		ulong diagonalCheckMap;
		ulong knightCheckMap;
		ulong pawnCheckMap;

		ulong squaresInCheckRayMap;

		bool inCheck;
		bool inDoubleCheck;

		bool hasKingSideCastleRight;
		bool hasQueenSideCastleRight;

		void Initialize() {
			moves = new List<Move>(64);

			whiteToMove = board.WhiteToMove;

			friendlyColor = board.ColorToMove;
			opponentColor = board.OpponentColor;

			friendlyColorIndex = whiteToMove ? Board.WhiteIndex : Board.BlackIndex;

			Dictionary<int, HashSet<int>> kings = board.PieceList.GetValue(Piece.King);
			friendlyKingSquare = kings[friendlyColor].ToArray()[0];
			opponentKingSquare = kings[opponentColor].ToArray()[0];

			int offset = friendlyColor == Piece.White ? 0 : 2;
			hasKingSideCastleRight = ((board.CurrentGameState >> offset) & 1) != 0;
			hasQueenSideCastleRight = ((board.CurrentGameState >> (offset + 1)) & 1) != 0;

			// Generate the maps
			BitMapGenerator bitMapGenerator = new BitMapGenerator();
			bitMapGenerator.GenerateBitMaps(board);

			opponentThreatMap = bitMapGenerator.OpponentThreatMap;

			verticalAndHorizontalCheckMap = bitMapGenerator.VerticalAndHorizontalCheckMap;
			diagonalCheckMap = bitMapGenerator.DiagonalCheckMap;
			knightCheckMap = bitMapGenerator.KnightCheckMap;
			pawnCheckMap = bitMapGenerator.PawnCheckMap;

			squaresInCheckRayMap = bitMapGenerator.SquaresInCheckRayMap;

			inCheck = bitMapGenerator.InCheck;
			inDoubleCheck = bitMapGenerator.InCheck && squaresInCheckRayMap == 0;
		}

		public List<Move> GenerateMoves(Board board) {
			this.board = board;

			Initialize();

			GenerateKingMoves();

			// Only king can move in double check
			if (!inDoubleCheck) {
				GenerateSlidingMoves();
				GenerateKnightMoves();
				GeneratePawnMoves();
			}

			return moves;
		}

		void GenerateKingMoves() {
			int kingSquare = friendlyKingSquare;
			for (int i = 0; i < KingMoves[kingSquare].Length; i++) {
				int targetSquare = KingMoves[kingSquare][i];
				int targetSquarePiece = board.Square[targetSquare];

				int flag = Move.Flag.None;

				// Square is under control by opponent, cannot move here
				if (HasSquare(opponentThreatMap, targetSquare)) continue;
				// Skip if square is occupied by friendly piece
				if (Piece.IsColor(targetSquarePiece, friendlyColor)) continue;

				// Flag this move as a capture move
				if (Piece.IsColor(targetSquarePiece, opponentColor)) flag = Move.Flag.Capture;

				moves.Add(new Move(kingSquare, targetSquare, flag));

				// Cannot castle while in check
				if (inCheck) continue;

				// Castle kingside
				if (targetSquare == f1 || targetSquare == f8) {
					int castleKingsideSquare = targetSquare + 1;

					// Castle square is under control by opponent, cannot move here
					if (HasSquare(opponentThreatMap, castleKingsideSquare)) continue;

					if (board.Square[castleKingsideSquare] == Piece.None && hasKingSideCastleRight) {
						moves.Add(new Move(kingSquare, castleKingsideSquare, Move.Flag.Castle));
					}
				// Castle queenside
				} else if (targetSquare == d1 || targetSquare == d8) {
					int castleQueensideSquare = targetSquare - 1;

					// Castle squares is under control by opponent, cannot move here
					if (HasSquare(opponentThreatMap, castleQueensideSquare)) continue;
					if (HasSquare(opponentThreatMap, castleQueensideSquare - 1)) continue;

					if (board.Square[castleQueensideSquare] == Piece.None && hasQueenSideCastleRight) {
						moves.Add(new Move(kingSquare, castleQueensideSquare, Move.Flag.Castle));
					}
				}
			}
		}

		void GenerateSlidingMoves() {
			HashSet<int> rooks = board.PieceList.GetValue(Piece.Rook)[friendlyColor];
			foreach (int rookSquare in rooks) {
				int startDir = 0;
				int endDir = 4;
				// Piece is possibly pinned
				if (HasSquare(opponentThreatMap, rookSquare)) {
					if (IsPinned(rookSquare)) {
						// Rook is pinned diagonally, skip this rook
						if (IsAlignedDiagonally(rookSquare, friendlyKingSquare)) {
							continue;
						}
						// Pinned vertically or horizontally, limit rook movement 
						if (IsAlignedVertically(rookSquare, friendlyKingSquare)) {
							startDir = 0;
							endDir = 2;
						} else {
							startDir = 2;
							endDir = 4;
						}
					}
				}
				GenerateSlidingPieceMoves(rookSquare, startDir, endDir);
			}

			HashSet<int> bishops = board.PieceList.GetValue(Piece.Bishop)[friendlyColor];
			foreach (int bishopSquare in bishops) {
				int startDir = 4;
				int endDir = 8;
				// Piece is possibly pinned
				if (HasSquare(opponentThreatMap, bishopSquare)) {
					if (IsPinned(bishopSquare)) {
						// Bishop is pinned vertically or horizontally, skip this bishop
						if (!IsAlignedDiagonally(bishopSquare, friendlyKingSquare)) {
							continue;
						}
						// Pinned diagonally, limit bishop movement 
						if (Abs(DirectionOffset(friendlyKingSquare, bishopSquare)) == NorthWest) {
							startDir = 4;
							endDir = 6;
						} else {
							startDir = 6;
							endDir = 8;
						}
					}
				}
				GenerateSlidingPieceMoves(bishopSquare, startDir, endDir);
			}

			HashSet<int> queens = board.PieceList.GetValue(Piece.Queen)[friendlyColor];
			foreach (int queenSquare in queens) {
				int startDir = 0;
				int endDir = 8;
				// Piece is possibly pinned
				if (HasSquare(opponentThreatMap, queenSquare)) {
					if (IsPinned(queenSquare)) {
						// Queen is pinned vertically or horizontally, limit movement to vertical or horizontal
						if (!IsAlignedDiagonally(queenSquare, friendlyKingSquare)) {
							if (Abs(DirectionOffset(friendlyKingSquare, queenSquare)) == North) {
								startDir = 0;
								endDir = 2;
							} else {
								startDir = 2;
								endDir = 4;
							}
						// Queen is pinned diagonally, limit movement to diagonal
						} else {
							if (Abs(DirectionOffset(friendlyKingSquare, queenSquare)) == NorthWest) {
								startDir = 4;
								endDir = 6;
							} else {
								startDir = 6;
								endDir = 8;
							}
						}
					}
				}
				GenerateSlidingPieceMoves(queenSquare, startDir, endDir);
			}
		}

		void GenerateSlidingPieceMoves(int startSquare, int startDirIndex, int endDirIndex) {
			ulong slidingCheckMap = 0;
			slidingCheckMap = endDirIndex <= 4 ? verticalAndHorizontalCheckMap : diagonalCheckMap;
			slidingCheckMap = startDirIndex == 0 && endDirIndex == 8 ? verticalAndHorizontalCheckMap | diagonalCheckMap : slidingCheckMap;

			for (int dirIndex = startDirIndex; dirIndex < endDirIndex; dirIndex++) {
				int currentDirOffset = DirectionOffsets[dirIndex];
				for (int n = 0; n < NumSquaresToEdge[startSquare][dirIndex]; n++) {
					int targetSquare = startSquare + currentDirOffset * (n + 1);
					int targetSquarePiece = board.Square[targetSquare];

					// King is in check and this move does not block the check
					if (inCheck && !HasSquare(squaresInCheckRayMap, targetSquare)) continue;

					// Block by friendly piece, stop looking in this direction
					if (Piece.IsColor(targetSquarePiece, friendlyColor)) break;

					int flag = Move.Flag.None;
					bool haveToCapture = false;

					// Flag this move as a capture move
					if (targetSquarePiece != Piece.None && Piece.IsColor(targetSquarePiece, opponentColor)) {
						flag |= Move.Flag.Capture;
						haveToCapture = true;
					}

					// Possible discovered check
					if (HasSquare(slidingCheckMap, targetSquare)) {
						if (IsDiscoveredCheck(targetSquare)) {
							flag |= Move.Flag.Check;
						}
					}

					// Check move
					if (HasSquare(slidingCheckMap, targetSquare)) {
						if (flag == Move.Flag.Check) {
							// Double check
						} else {
							flag |= Move.Flag.Check;
						}	
					}

					moves.Add(new Move(startSquare, targetSquare, flag));

					// Block by opponent piece, have to capture
					if (haveToCapture) break;
				}
			}
		}

		void GenerateKnightMoves() {
			HashSet<int> knights = board.PieceList.GetValue(Piece.Knight)[friendlyColor];
			foreach (int knightSquare in knights) {
				// Knight is pinned, skip this piece
				if (HasSquare(opponentThreatMap, knightSquare)) {
					if (IsPinned(knightSquare)) {
						continue;
					}
				}

				for (int knightMoveIndex = 0; knightMoveIndex < KnightMoves[knightSquare].Length; knightMoveIndex++) {
					int targetSquare = KnightMoves[knightSquare][knightMoveIndex];
					int targetSquarePiece = board.Square[targetSquare];

					// King is in check and this move does not block the check
					if (inCheck && !HasSquare(squaresInCheckRayMap, targetSquare)) continue;

					// Skip if same color piece
					if (Piece.IsColor(targetSquarePiece, friendlyColor)) continue;

					int flag = Move.Flag.None;

					// Flag this move as a capture move
					if (Piece.IsColor(targetSquarePiece, opponentColor)) flag |= Move.Flag.Capture;

					// Possible discovered check
					if (HasSquare(verticalAndHorizontalCheckMap, targetSquare) || HasSquare(diagonalCheckMap, targetSquare)) {
						if (IsDiscoveredCheck(knightSquare)) {
							flag |= Move.Flag.Check;
						}
					}

					// Check move
					if (HasSquare(knightCheckMap, targetSquare)) {
						if (flag == Move.Flag.Check) {
							// Double check
						} else {
							flag |= Move.Flag.Check;
						}	
					}

					moves.Add(new Move(knightSquare, targetSquare, flag));
				}
			}
		}

		void GeneratePawnMoves() {
			HashSet<int> pawns = board.PieceList.GetValue(Piece.Pawn)[friendlyColor];

			int pawnOffset = whiteToMove ? North : -North;
			int startRank = whiteToMove ? 1 : 6;

			int enPassantSquare = -1;
			int enPassantFile = ((int) (board.CurrentGameState >> 4) & 15) - 1;
			if (enPassantFile != -1) {
				enPassantSquare = 8 * (whiteToMove ? 5 : 2) + enPassantFile;
			}

			foreach (int pawnSquare in pawns) {
				// Piece is possibly pinned
				if (HasSquare(opponentThreatMap, pawnSquare)) {
					if (IsPinned(pawnSquare)) {
						// Pinned diagonally or horizontally, skip this piece
						if (IsAlignedDiagonally(friendlyKingSquare, pawnSquare) || IsAlignedHorizontally(friendlyKingSquare, pawnSquare)) {
							continue;
						}
					}
				}

				int rank = RankIndex(pawnSquare);
				int promotionRank = whiteToMove ? 7 : 0;

				int flag;

				int squareOneForward = pawnSquare + pawnOffset;

				// Forward moves
				if (board.Square[squareOneForward] == Piece.None) {
					bool canMove = !(inCheck && !HasSquare(squaresInCheckRayMap, squareOneForward));
					if (canMove) {
						if (RankIndex(squareOneForward) == promotionRank) {
							moves.Add(new Move(pawnSquare, squareOneForward, Move.Flag.Promote));
						} else {
							flag = Move.Flag.None;
							if (HasSquare(pawnCheckMap, squareOneForward)) {
								flag |= Move.Flag.Check;
							}
							moves.Add(new Move(pawnSquare, squareOneForward, flag));
						}
					}

					// Pawn on starting square, can move two forward
					if (rank == startRank) {
						int squareTwoForward = squareOneForward + pawnOffset;
						canMove = !(inCheck && !HasSquare(squaresInCheckRayMap, squareTwoForward));
						if (canMove) {
							if (board.Square[squareTwoForward] == Piece.None) {
								flag = Move.Flag.None;
								flag |= Move.Flag.PawnTwoForward;
								if (HasSquare(pawnCheckMap, squareTwoForward)) {
									flag |= Move.Flag.Check;
								}
								moves.Add(new Move(pawnSquare, squareTwoForward, flag));
							}
						}
					}
				}

				// Capture moves
				for (int j = 0; j < 2; j++) {
					// Check if diagonal square exists
					if (NumSquaresToEdge[pawnSquare][PawnAttackDirections[friendlyColorIndex][j]] > 0) {
						int pawnCaptureDir = DirectionOffsets[PawnAttackDirections[friendlyColorIndex][j]];
						int targetSquare = pawnSquare + pawnCaptureDir;
						int targetSquarePiece = board.Square[targetSquare];

						// King is in check and this move does not block the check
						if (inCheck && !HasSquare(squaresInCheckRayMap, targetSquare)) continue;

						flag = Move.Flag.None;

						if (HasSquare(pawnCheckMap, targetSquarePiece)) {
							flag |= Move.Flag.Check;
						}

						// Regular capture
						if (Piece.IsColor(targetSquarePiece, opponentColor)) {
							if (RankIndex(targetSquare) == promotionRank) {
								moves.Add(new Move(pawnSquare, targetSquare, Move.Flag.Promote));
							} else {
								flag |= Move.Flag.Capture;
								moves.Add(new Move(pawnSquare, targetSquare, flag));
							}
						}

						// En-passant capture
						if (targetSquare == enPassantSquare) {
							flag &= Move.Flag.Capture;
							flag |= Move.Flag.EnPassant;
							moves.Add(new Move(pawnSquare, targetSquare, flag));
						}
					}
				}
			}
		}

		bool IsPinned(int startSquare) {
			if (!IsAligned(friendlyKingSquare, startSquare)) return false;

			int sign = whiteToMove ? 1 : -1;
			int searchDirOffset = sign * DirectionOffset(friendlyKingSquare, startSquare);

			return HasAttackingPiece(board.Square, startSquare, searchDirOffset, opponentColor) && 
							!HasPieceBetween(board.Square, startSquare, -searchDirOffset);
		}

		bool IsDiscoveredCheck(int startSquare) {
			if (!IsAligned(opponentKingSquare, startSquare)) return false;

			int sign = whiteToMove ? 1 : -1;
			int searchDirOffset = sign * DirectionOffset(opponentKingSquare, startSquare);

			return HasAttackingPiece(board.Square, startSquare, searchDirOffset, friendlyColor) && 
							!HasPieceBetween(board.Square, startSquare, -searchDirOffset);;
		}
	}
}