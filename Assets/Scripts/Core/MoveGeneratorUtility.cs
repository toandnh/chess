using static System.Math;

namespace Chess {
	using static BoardRepresentation;
	using static PrecomputedMoveData;
	using static BitBoardUtility;

	public static class MoveGeneratorUtility {
		public static int DirectionOffset(int fromSquare, int toSquare) {
			// Direction from -> to is East or West
			int dirOffSet = fromSquare < toSquare ? East : -East;

			// Other directions
			int rankDiff = Abs(RankIndex(fromSquare) - RankIndex(toSquare));
			dirOffSet = rankDiff != 0 ? (fromSquare - toSquare) / rankDiff : dirOffSet;

			return dirOffSet;
		}

		public static bool HasAttackingPiece(int[] board, int startSquare, int searchDirOffset, int searchForColor) {
			int oppositeColor = searchForColor == Piece.White ? Piece.Black : Piece.White;

			int dirIndex = DirectionIndices[searchDirOffset];
			for (int n = 0; n < NumSquaresToEdge[startSquare][dirIndex]; n++) {
				int targetSquare = startSquare + searchDirOffset * (n + 1);
				int targetSquarePiece = board[targetSquare];
				int targetSquarePieceType = Piece.PieceType(targetSquarePiece);

				// Opposite color piece, stop the search
				if (Piece.IsColor(targetSquarePiece, oppositeColor)) break;

				bool isDiagonal = dirIndex >= 4;

				// Search for color piece, check if piece is attacking the opposite color king
				if (Piece.IsColor(targetSquarePiece, searchForColor)) {
					if (targetSquarePieceType == Piece.Queen ||
							isDiagonal && targetSquarePieceType == Piece.Bishop ||
							!isDiagonal && targetSquarePieceType == Piece.Rook) {
						return true;
					}
				}
			}

			// Could not find any attacking piece
			return false;
		}

		public static bool HasPieceBetween(int[] board, int startSquare, int searchDirOffset) {
			int dirIndex = DirectionIndices[searchDirOffset];
			for (int n = 0; n < NumSquaresToEdge[startSquare][dirIndex]; n++) {
				int targetSquare = startSquare + searchDirOffset * (n + 1);
				int targetSquarePiece = board[targetSquare];
				int targetSquarePieceType = Piece.PieceType(targetSquarePiece);

				if (targetSquarePieceType == Piece.King) return false;
				if (targetSquarePieceType != Piece.None) return true;
			}
			// Non reachable code
			return true;
		}

		public static bool IsAligned(int firstSquare, int secondSquare) {
			return (IsAlignedDiagonally(firstSquare, secondSquare) ||
							IsAlignedVertically(firstSquare, secondSquare) ||
							IsAlignedHorizontally(firstSquare, secondSquare));
		}

		public static bool IsAlignedDiagonally(int firstSquare, int secondSquare) {
			return Abs(RankIndex(firstSquare) - RankIndex(secondSquare)) == Abs(FileIndex(firstSquare) - FileIndex(secondSquare));
		}

		public static bool IsAlignedVertically(int firstSquare, int secondSquare) {
			return FileIndex(firstSquare) == FileIndex(secondSquare);
		}

		public static bool IsAlignedHorizontally(int firstSquare, int secondSquare) {
			return RankIndex(firstSquare) == RankIndex(secondSquare);
		}

		public static bool IsCheck(int[] board, int startSquare, int pieceType, int kingSquare) {
			bool isCheck = false;
			if (pieceType == Piece.Queen || pieceType == Piece.Rook) {
				if (IsAlignedVertically(startSquare, kingSquare) || IsAlignedHorizontally(startSquare, kingSquare)) {
					isCheck |= !HasPieceBetween(board, startSquare, DirectionOffset(startSquare, kingSquare));
				}
			} 
			if (pieceType == Piece.Queen || pieceType == Piece.Bishop) {
				if (IsAlignedDiagonally(startSquare, kingSquare)) {
					isCheck |= !HasPieceBetween(board, startSquare, DirectionOffset(startSquare, kingSquare));
				}
			}
			if (pieceType == Piece.Knight) {
				isCheck |= HasSquare(KnightAttackBitBoard[kingSquare], startSquare);
			}
			return isCheck;
		}
	}
}