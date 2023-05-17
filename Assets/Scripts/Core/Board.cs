using System.Collections.Generic;

namespace Chess {
	public class Board {
		public int[] Square;

		public bool WhiteToMove;
		public int ColorToMove;

		void Initialize() {
			Square = new int[64];
		}

		public void LoadStartPosition() {
			LoadPosition(FenUtility.StartFen);
		}

		public void LoadPosition(string fen) {
			Initialize();
			
			var loadedPosition = FenUtility.PositionFromFen(fen);

			for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
				int piece = loadedPosition.squares[squareIndex];
				Square[squareIndex] = piece;
			}

			WhiteToMove = loadedPosition.whiteToMove;
			ColorToMove = WhiteToMove ? Piece.White : Piece.Black;
		}

		public void MakeMove(Move move) {
			int moveFrom = move.StartSquare;
			int moveTo = move.TargetSquare;
			int movePiece = Square[moveFrom];

			// Update the board representation
			Square[moveTo] = movePiece;
			Square[moveFrom] = 0;
		}
	}
}