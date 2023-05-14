using System.Collections.Generic;

namespace Chess {
	public class Board {
		public int[] square;

		void Initialize() {
			square = new int[64];
		}

		public void LoadStartPosition() {
			LoadPosition(FenUtility.StartFen);
		}

		public void LoadPosition(string fen) {
			Initialize();
			
			var loadedPosition = FenUtility.PositionFromFen(fen);

			for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
				int piece = loadedPosition.squares[squareIndex];
				square[squareIndex] = piece;
			}
		}
	}
}