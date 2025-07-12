using UnityEngine;

namespace Chess.Game {
	public class CaptureUI : MonoBehaviour {
		public PieceTheme pieceTheme;

		Transform bottomCaptures;
		Transform topCaptures;

		void Awake() {
			bottomCaptures = GameObject.Find("Player Captures").transform;
			topCaptures = GameObject.Find("Opponent Captures").transform;
		}

		public void OnMoveMade(Board board, BoardUI boardUI) {
			// Transform captures;

			// // White bottom;
			// // this function is called at the end of the game loop, 
			// // hence, the turn is reversed
			// if (boardUI.IsWhiteBottom && !board.WhiteToMove || !boardUI.IsWhiteBottom && board.WhiteToMove) {
			// 	captures = bottomCaptures;

			// 	// Black bottom
			// } else {
			// 	captures = topCaptures;
			// }

			// ResetCapture(captures);

			// Function called at the end of game loop, so the turn is reversed
			// int color = board.WhiteToMove ? Piece.White : Piece.Black;
			DrawCapturedPieces(board, boardUI, board.WhiteToMove ? Piece.White : Piece.Black);
		}

		public void DrawCapturedPieces(Board board, BoardUI boardUI, int color) {
			Transform captures = topCaptures;

			if (boardUI.IsWhiteBottom && color != Piece.White || !boardUI.IsWhiteBottom && color == Piece.White) {
				captures = bottomCaptures;
			}

			int colorIndex = color == Piece.White ? Board.BlackIndex : Board.WhiteIndex;

			ResetCapture(captures);

			float xPosition = -550;
			for (int pieceType = 1; pieceType < board.Captures[colorIndex].Length; pieceType++) {
				int numPieces = board.Captures[colorIndex][pieceType];
				if (numPieces == 0) continue;

				float depth = -0.1f;
				xPosition += 50;
				for (int i = 0; i < numPieces; i++) {
					depth -= 0.1f;
					xPosition += 30;

					SpriteRenderer pieceRenderer = new GameObject("Piece").AddComponent<SpriteRenderer>();
					pieceRenderer.transform.parent = captures;
					pieceRenderer.transform.localPosition = new Vector3(xPosition, 0, depth);
					pieceRenderer.transform.localScale = new Vector3(100, 100, 1);

					pieceRenderer.sprite = pieceTheme.GetPieceSprite(pieceType | color);
				}
			}
		}

		public void ResetCapture() {
			ResetCapture(bottomCaptures);
			ResetCapture(topCaptures);
		}

		void ResetCapture(Transform captures) {
			while (captures.transform.childCount > 0) {
				DestroyImmediate(captures.transform.GetChild(0).gameObject);
			}
		}
	}
}