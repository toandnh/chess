using UnityEngine;

namespace Chess.Game {
	public class CaptureUI : MonoBehaviour {
		public PieceTheme pieceTheme;

		Transform whiteCaptures;
		Transform blackCaptures;

		void Awake() {
			whiteCaptures = GameObject.Find("Player Captures").transform;
			blackCaptures = GameObject.Find("Opponent Captures").transform;
		}

		public void OnMoveMade(Board board, BoardUI boardUI) {
			Transform captures;

			// White bottom;
			// this function is called at the end of the game loop, 
			// hence, the turn is reversed
			if (boardUI.IsWhiteBottom && !board.WhiteToMove || !boardUI.IsWhiteBottom && board.WhiteToMove) {
				captures = whiteCaptures;

				// Black bottom
			} else {
				captures = blackCaptures;
			}

			while (captures.transform.childCount > 0) {
				DestroyImmediate(captures.transform.GetChild(0).gameObject);
			}

			// Function called at the end of game loop, so the turn is reversed
			int colorIndex = board.WhiteToMove ? Board.BlackIndex : Board.WhiteIndex;
			int color = board.WhiteToMove ? Piece.White : Piece.Black;
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
			while (whiteCaptures.transform.childCount > 0) {
				DestroyImmediate(whiteCaptures.transform.GetChild(0).gameObject);
			}
			while (blackCaptures.transform.childCount > 0) {
				DestroyImmediate(blackCaptures.transform.GetChild(0).gameObject);
			}
		}
	}
}