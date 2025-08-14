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

		ulong orthogonalCheckMap;
		ulong diagonalCheckMap;
		ulong knightCheckMap;
		ulong pawnCheckMap;

		ulong squaresInCheckRayMap;

		bool inCheck;
		bool inDoubleCheck;

		bool hasKingSideCastleRight;
		bool hasQueenSideCastleRight;

		void Initialize(Board board) {
			this.board = board;

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

			orthogonalCheckMap = bitMapGenerator.OrthogonalCheckMap;
			diagonalCheckMap = bitMapGenerator.DiagonalCheckMap;
			knightCheckMap = bitMapGenerator.KnightCheckMap;
			pawnCheckMap = bitMapGenerator.PawnCheckMap;

			squaresInCheckRayMap = bitMapGenerator.SquaresInCheckRayMap;

			inCheck = bitMapGenerator.InCheck;
			inDoubleCheck = bitMapGenerator.InCheck && squaresInCheckRayMap == 0;
		}

		public List<Move> GenerateMoves(Board board) {
			Initialize(board);

			GenerateKingMoves();

			// Only king can move in double check
			if (!inDoubleCheck) {
				GeneratePawnsMoves();
				GenerateKnightsMoves();
				GenerateSlidingMoves();
			}

			// Double check and no king moves = check mate
			return moves;
		}

		public List<Move> GeneratePieceMoves(Board board, int pieceSquare) {
			Initialize(board);

			int piece = Piece.PieceType(board.Square[pieceSquare]);
			
			switch (piece) {
				case Piece.Pawn:
					GeneratePawnMoves(pieceSquare);
					break;
				case Piece.Knight:
					GenerateKnightMoves(pieceSquare);
					break;
				case Piece.Bishop:
					GenerateBishopMoves(pieceSquare);
					break;
				case Piece.Rook:
					GenerateRookMoves(pieceSquare);
					break;
				case Piece.Queen:
					GenerateQueenMoves(pieceSquare);
					break;
				case Piece.King:
					GenerateKingMoves();
					break;
				default:
					break;
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

				bool hasOpponentPiece = Piece.IsColor(targetSquarePiece, opponentColor);

				// Raise capture flag
				if (hasOpponentPiece) flag |= Move.Flag.Capture;
				moves.Add(new Move(kingSquare, targetSquare, flag));

				// Cannot castle while in check or has an opponent piece in between
				if (inCheck || hasOpponentPiece) continue;
				// Castle kingside
				if (targetSquare == f1 || targetSquare == f8) {
					int castleKingsideSquare = targetSquare + 1;

					// Castle square is under control by opponent, cannot move here
					if (HasSquare(opponentThreatMap, castleKingsideSquare)) continue;

					// Square is occupied by opponent piece, cannot castle
					if (Piece.IsColor(board.Square[castleKingsideSquare], opponentColor)) continue;

					if (board.Square[castleKingsideSquare] == Piece.None && hasKingSideCastleRight) {
						moves.Add(new Move(kingSquare, castleKingsideSquare, Move.Flag.Castle));
					}
				// Castle queenside
				} else if (targetSquare == d1 || targetSquare == d8) {
					int castleQueensideSquare = targetSquare - 1;

					// Castle squares is under control by opponent, cannot move here
					if (HasSquare(opponentThreatMap, castleQueensideSquare)) continue;
					if (HasSquare(opponentThreatMap, castleQueensideSquare - 1)) continue;

					// Square is occupied by opponent piece, cannot castle
					if (Piece.IsColor(board.Square[castleQueensideSquare], opponentColor)) continue;
					if (Piece.IsColor(board.Square[castleQueensideSquare - 1], opponentColor)) continue;

					if (board.Square[castleQueensideSquare] == Piece.None && 
							board.Square[castleQueensideSquare -1] == Piece.None && 
							hasQueenSideCastleRight) {
						moves.Add(new Move(kingSquare, castleQueensideSquare, Move.Flag.Castle));
					}
				}
			}
		}

		void GenerateSlidingMoves() {
			HashSet<int> bishops = board.PieceList.GetValue(Piece.Bishop)[friendlyColor];
			foreach (int bishopSquare in bishops) {
				GenerateBishopMoves(bishopSquare);
			}
			
			HashSet<int> rooks = board.PieceList.GetValue(Piece.Rook)[friendlyColor];
			foreach (int rookSquare in rooks) {
				GenerateRookMoves(rookSquare);
			}

			HashSet<int> queens = board.PieceList.GetValue(Piece.Queen)[friendlyColor];
			foreach (int queenSquare in queens) {
				GenerateQueenMoves(queenSquare);
			}
		}

		void GenerateBishopMoves(int bishopSquare) {
			int startDir = 4;
			int endDir = 8;
			// Piece is possibly pinned
			if (HasSquare(opponentThreatMap, bishopSquare)) {
				if (IsPinned(board.Square, bishopSquare, friendlyKingSquare)) {
					// Bishop is pinned vertically or horizontally, skip this bishop
					if (!IsAlignedDiagonally(bishopSquare, friendlyKingSquare)) {
						return;
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

		void GenerateRookMoves(int rookSquare) {
			int startDir = 0;
			int endDir = 4;
			// Piece is possibly pinned
			if (HasSquare(opponentThreatMap, rookSquare)) {
				if (IsPinned(board.Square, rookSquare, friendlyKingSquare)) {
					// Rook is pinned diagonally, skip this rook
					if (IsAlignedDiagonally(rookSquare, friendlyKingSquare)) {
						return;
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

		void GenerateQueenMoves(int queenSquare) {
			int startDir = 0;
			int endDir = 8;
			// Piece is possibly pinned
			if (HasSquare(opponentThreatMap, queenSquare)) {
				if (IsPinned(board.Square, queenSquare, friendlyKingSquare)) {
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

		void GenerateSlidingPieceMoves(int startSquare, int startDirIndex, int endDirIndex) {
			ulong slidingCheckMap = 0;
			slidingCheckMap = endDirIndex <= 4 ? orthogonalCheckMap : diagonalCheckMap;
			slidingCheckMap = startDirIndex == 0 && endDirIndex == 8 ? orthogonalCheckMap | diagonalCheckMap : slidingCheckMap;

			for (int dirIndex = startDirIndex; dirIndex < endDirIndex; dirIndex++) {
				int currentDirOffset = DirectionOffsets[dirIndex];
				for (int n = 0; n < NumSquaresToEdge[startSquare][dirIndex]; n++) {
					int targetSquare = startSquare + currentDirOffset * (n + 1);
					int targetSquarePiece = board.Square[targetSquare];

					// Block by friendly piece, stop looking in this direction
					if (Piece.IsColor(targetSquarePiece, friendlyColor)) break;

					// King is in check and this move does not block the check
					if (inCheck && !HasSquare(squaresInCheckRayMap, targetSquare)) continue;

					int flag = Move.Flag.None;

					// Possible discovered check
					if (HasSquare(slidingCheckMap, targetSquare)) {
						if (IsDiscoveredCheck(board.Square, targetSquare, opponentKingSquare)) {
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

					// Block by opponent piece, have to capture
					if (Piece.IsColor(targetSquarePiece, opponentColor)) {
						// Prevent king-capture moves
						if (targetSquarePiece != Piece.King) {
							flag |= Move.Flag.Capture;
							moves.Add(new Move(startSquare, targetSquare, flag));
						}
						break;
					}

					// Else, add this move to the list and keep looking
					moves.Add(new Move(startSquare, targetSquare, flag));
				}
			}
		}

		void GenerateKnightsMoves() {
			HashSet<int> knights = board.PieceList.GetValue(Piece.Knight)[friendlyColor];
			foreach (int knightSquare in knights) {
				GenerateKnightMoves(knightSquare);
			}
		}

		void GenerateKnightMoves(int knightSquare) {
			// Knight is pinned, skip this piece
			if (HasSquare(opponentThreatMap, knightSquare)) {
				if (IsPinned(board.Square, knightSquare, friendlyKingSquare)) return;
			}

			for (int knightMoveIndex = 0; knightMoveIndex < KnightMoves[knightSquare].Length; knightMoveIndex++) {
				int targetSquare = KnightMoves[knightSquare][knightMoveIndex];
				int targetSquarePiece = board.Square[targetSquare];

				// King is in check and this move does not block the check
				if (inCheck && !HasSquare(squaresInCheckRayMap, targetSquare)) continue;

				// Skip if same color piece
				if (Piece.IsColor(targetSquarePiece, friendlyColor)) continue;

				int flag = Move.Flag.None;

				// Possible discovered check
				if (HasSquare(orthogonalCheckMap, targetSquare) || HasSquare(diagonalCheckMap, targetSquare)) {
					if (IsDiscoveredCheck(board.Square, knightSquare, opponentKingSquare)) {
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

				// Prevent king-capture moves
				if (Piece.IsColor(targetSquarePiece, opponentColor)) {
					if (targetSquarePiece == Piece.King) continue;
					flag |= Move.Flag.Capture;
				}

				moves.Add(new Move(knightSquare, targetSquare, flag));
			}
		}

		void GeneratePawnsMoves() {
			HashSet<int> pawns = board.PieceList.GetValue(Piece.Pawn)[friendlyColor];
			foreach (int pawnSquare in pawns) {
				GeneratePawnMoves(pawnSquare);
			}
		}

		void GeneratePawnMoves(int pawnSquare) {
			int pawnOffset = whiteToMove ? North : -North;
			int startRank = whiteToMove ? 1 : 6;

			int enPassantSquare = -1;
			int enPassantFile = ((int) (board.CurrentGameState >> 4) & 0b1111) - 1;
			if (enPassantFile != -1) {
				enPassantSquare = 8 * (whiteToMove ? 5 : 2) + enPassantFile;
			}

			bool isPinnedVertically = false;
			bool isPinnedDiagonally = false;
			
			// Piece is possibly pinned
			if (HasSquare(opponentThreatMap, pawnSquare)) {
				if (IsPinned(board.Square, pawnSquare, friendlyKingSquare)) {
					// Cannot move when pinned horizontally
					if (IsAlignedHorizontally(friendlyKingSquare, pawnSquare)) return;

					if (IsAlignedDiagonally(friendlyKingSquare, pawnSquare)) {
						isPinnedDiagonally = true;
					} else {
						isPinnedVertically = true;
					}
				}
			}

			int rank = RankIndex(pawnSquare);
			int promotionRank = whiteToMove ? 7 : 0;

			int flag = Move.Flag.None;

			int squareOneForward = pawnSquare + pawnOffset;

			// Forward moves
			if (board.Square[squareOneForward] == Piece.None && !isPinnedDiagonally) {
				bool canMove = !(inCheck && !HasSquare(squaresInCheckRayMap, squareOneForward));
				if (canMove) {
					if (RankIndex(squareOneForward) == promotionRank) {
						moves.Add(new Move(pawnSquare, squareOneForward, FullPawnPromotionFlag(Move.Flag.PromoteToKnight, squareOneForward, knightCheckMap)));
						moves.Add(new Move(pawnSquare, squareOneForward, FullPawnPromotionFlag(Move.Flag.PromoteToBishop, squareOneForward, diagonalCheckMap)));
						moves.Add(new Move(pawnSquare, squareOneForward, FullPawnPromotionFlag(Move.Flag.PromoteToRook, squareOneForward, orthogonalCheckMap)));
						moves.Add(new Move(pawnSquare, squareOneForward, FullPawnPromotionFlag(Move.Flag.PromoteToQueen, squareOneForward, orthogonalCheckMap | diagonalCheckMap)));
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

			if (isPinnedVertically) return;

			// Capture moves
			for (int j = 0; j < 2; j++) {
				// Check if diagonal square exists
				if (NumSquaresToEdge[pawnSquare][PawnAttackDirections[friendlyColorIndex][j]] > 0) {
					int pawnCaptureDir = DirectionOffsets[PawnAttackDirections[friendlyColorIndex][j]];

					int targetSquare = pawnSquare + pawnCaptureDir;
					int targetSquarePiece = board.Square[targetSquare];
					int targetSquarePieceType = Piece.PieceType(targetSquarePiece);

					// Pinned diagonally;
					if (isPinnedDiagonally) {
						// And this square does not have an attacking piece;
						if (targetSquarePieceType != Piece.Bishop && targetSquarePieceType != Piece.Queen) {
							continue;
						// Or has an attacking piece but not on the same line 
						} else {
							if (DirectionOffset(friendlyKingSquare, pawnSquare) != DirectionOffset(friendlyKingSquare, targetSquare)) {
								continue;
							}
						}
					}

					// King is in check and this move does not block the check
					if (inCheck && !HasSquare(squaresInCheckRayMap, targetSquare)) continue;

					flag = Move.Flag.None;

					if (HasSquare(pawnCheckMap, targetSquarePiece)) {
						flag |= Move.Flag.Check;
					}

					// Regular capture
					if (Piece.IsColor(targetSquarePiece, opponentColor)) {
						// Prevent king-capture moves
						if (targetSquarePieceType == Piece.King) continue;

						flag |= Move.Flag.Capture;
						if (RankIndex(targetSquare) == promotionRank) {
							moves.Add(new Move(pawnSquare, targetSquare, FullPawnPromotionFlag(flag | Move.Flag.PromoteToKnight, targetSquare, knightCheckMap)));
							moves.Add(new Move(pawnSquare, targetSquare, FullPawnPromotionFlag(flag | Move.Flag.PromoteToBishop, targetSquare, diagonalCheckMap)));
							moves.Add(new Move(pawnSquare, targetSquare, FullPawnPromotionFlag(flag | Move.Flag.PromoteToRook, targetSquare, orthogonalCheckMap)));
							moves.Add(new Move(pawnSquare, targetSquare, FullPawnPromotionFlag(flag | Move.Flag.PromoteToQueen, targetSquare, orthogonalCheckMap | diagonalCheckMap)));
						} else {
							moves.Add(new Move(pawnSquare, targetSquare, flag));
						}
						flag ^= Move.Flag.Capture;
					}

					// En-passant capture
					if (targetSquare == enPassantSquare) {
						flag |= Move.Flag.EnPassant;
						moves.Add(new Move(pawnSquare, targetSquare, flag));
					}
				}
			}
		}
	}
}