using System.Collections.Generic;
using System.Linq;

namespace Chess {
	using static BoardRepresentation;
	using static PrecomputedMoveData;

	public class MoveGenerator {
		List<Move> moves;

		Board board;

		bool isWhiteToMove;

		int friendlyColor;
		int opponentColor;

		int friendlyColorIndex;

		void Initialize() {
			moves = new List<Move>(64);

			isWhiteToMove = board.WhiteToMove;

			friendlyColor = board.ColorToMove;
			opponentColor = board.OpponentColor;

			friendlyColorIndex = board.WhiteToMove ? Board.WhiteIndex : Board.BlackIndex;
		}

		public List<Move> GenerateMoves(Board board) {
			this.board = board;

			Initialize();

			GenerateKingMoves();

			GenerateSlidingMoves();
			GenerateKnightMoves();
			GeneratePawnMoves();

			return moves;
		}

		void GenerateKingMoves() {
			HashSet<int> king = board.PieceList.GetValue(Piece.King)[friendlyColor];
			int kingSquare = king.ToArray()[0]; // There can only be one king of each color
			for (int i = 0; i < KingMoves[kingSquare].Length; i++) {
				int targetSquare = KingMoves[kingSquare][i];
				int targetSquarePiece = board.Square[targetSquare];

				// Skip if square is occupied by friendly piece
				if (Piece.IsColor(targetSquarePiece, friendlyColor)) continue;

				moves.Add(new Move(kingSquare, targetSquare));

				// Castle kingside
				if (targetSquare == f1 || targetSquare == f8) {
					int castleKingsideSquare = targetSquare + 1;
					if (board.Square[castleKingsideSquare] == Piece.None) {
						moves.Add(new Move(kingSquare, castleKingsideSquare, Move.Flag.Castling));
					}
				// Castle queenside
				} else if (targetSquare == d1 || targetSquare == d8) {
					int castleQueensideSquare = targetSquare - 1;
					if (board.Square[castleQueensideSquare] == Piece.None) {
						moves.Add(new Move(kingSquare, castleQueensideSquare, Move.Flag.Castling));
					}
				}
			}
		}

		void GenerateSlidingMoves() {
			HashSet<int> rooks = board.PieceList.GetValue(Piece.Rook)[friendlyColor];
			foreach (int rookSquare in rooks) {
				GenerateSlidingPieceMoves(rookSquare, 0, 4);
			}

			HashSet<int> bishops = board.PieceList.GetValue(Piece.Bishop)[friendlyColor];
			foreach (int bishopSquare in bishops) {
				GenerateSlidingPieceMoves(bishopSquare, 4, 8);
			}

			HashSet<int> queens = board.PieceList.GetValue(Piece.Queen)[friendlyColor];
			foreach (int queenSquare in queens) {
				GenerateSlidingPieceMoves(queenSquare, 0, 8);
			}
		}

		void GenerateSlidingPieceMoves(int startSquare, int startDirIndex, int endDirIndex) {
			for (int dirIndex = startDirIndex; dirIndex < endDirIndex; dirIndex++) {
				int currentDirOffset = DirectionOffsets[dirIndex];
				for (int n = 0; n < NumSquaresToEdge[startSquare][dirIndex]; n++) {
					int targetSquare = startSquare + currentDirOffset * (n + 1);
					int targetSquarePiece = board.Square[targetSquare];

					// Block by friendly piece, stop looking in this direction
					if (Piece.IsColor(targetSquarePiece, friendlyColor)) break;

					moves.Add(new Move(startSquare, targetSquare));

					// Block by opponent piece, have to capture
					if (Piece.IsColor(targetSquarePiece, opponentColor)) break;
				}
			}
		}

		void GenerateKnightMoves() {
			HashSet<int> knights = board.PieceList.GetValue(Piece.Knight)[friendlyColor];
			foreach (int knightSquare in knights) {
				for (int knightMoveIndex = 0; knightMoveIndex < KnightMoves[knightSquare].Length; knightMoveIndex++) {
					int targetSquare = KnightMoves[knightSquare][knightMoveIndex];
					int targetSquarePiece = board.Square[targetSquare];

					// Skip if same color piece
					if (Piece.IsColor(targetSquarePiece, friendlyColor)) continue;

					moves.Add(new Move(knightSquare, targetSquare));
				}
			}
		}

		void GeneratePawnMoves() {
			HashSet<int> pawns = board.PieceList.GetValue(Piece.Pawn)[friendlyColor];

			int pawnOffset = friendlyColor == Piece.White ? 8 : -8;
			int startRank = friendlyColor == Piece.White ? 1 : 6;

			int enPassantSquare = -1;
			int enPassantFile = ((int) (board.CurrentGameState >> 4) & 15) - 1;
			if (enPassantFile != -1) {
				enPassantSquare = 8 * (board.WhiteToMove ? 5 : 2) + enPassantFile;
			}

			foreach (int pawnSquare in pawns) {
				int rank = RankIndex(pawnSquare);

				int squareOneForward = pawnSquare + pawnOffset;
				// Forward moves
				if (board.Square[squareOneForward] == Piece.None) {
					moves.Add(new Move(pawnSquare, squareOneForward));

					// Pawn on starting square, can move two forward
					if (rank == startRank) {
						int squareTwoForward = squareOneForward + pawnOffset;
						if (board.Square[squareTwoForward] == Piece.None) {
							moves.Add(new Move(pawnSquare, squareTwoForward, Move.Flag.PawnTwoForward));
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

						// Regular capture
						if (Piece.IsColor(targetSquarePiece, opponentColor)) {
							moves.Add(new Move(pawnSquare, targetSquare));
						}

						// En-passant capture
						if (targetSquare == enPassantSquare) {
							//int epCaptureSquare = targetSquare + (board.WhiteToMove ? -8 : 8);

							moves.Add(new Move(pawnSquare, targetSquare, Move.Flag.EnPassant));
						}
					}
				}
			}
		}
	}
}