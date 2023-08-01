using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace Chess.Game {
	public class MoveTextUI : MonoBehaviour {
		public BoardTheme boardTheme;

		Transform content;
		ScrollRect scrollRect;

		void Awake() {
			content = GameObject.Find("Content").transform;
			scrollRect = GameObject.Find("Scroll View").GetComponent<ScrollRect>();
		}

		public void OnMoveMade(Board board) {
			UpdateMoveText(board);
		}

		public void UpdateMoveText(Board board) {
			bool whiteToMove = board.WhiteToMove;
			int currentIndex = board.Text[0].Count - 1;

			GameObject panel;
			RectTransform rectTransform;
			TextMeshProUGUI text;

			// White's turn; 
			// this function is called at the end of the game loop, 
			// hence, it actually is black's turn when the white's move is written down
			if (!whiteToMove) {
				// Create panel
				panel = new GameObject();
				panel.name = "Line " + (currentIndex + 1).ToString();

				panel.AddComponent<RectTransform>();
				panel.AddComponent<CanvasRenderer>();
				panel.AddComponent<Image>();

				panel.transform.SetParent(content);

				rectTransform = panel.GetComponent<RectTransform>();
				// Width, height
				rectTransform.sizeDelta = new Vector2(1284, 100);
				// Anchor top-left
				rectTransform.anchorMin = new Vector2(0, 1);
				rectTransform.anchorMax = new Vector2(0, 1);
				rectTransform.pivot = new Vector2(0, 1);
				// Scale
				rectTransform.localScale = new Vector2(1, 1);

				Image image = panel.GetComponent<Image>();
				image.color = currentIndex % 2 == 0 ? boardTheme.MoveTextLight : boardTheme.MoveTextDark;

				// TextMeshPro
				// Move number
				GameObject moveNumber = new GameObject();
				moveNumber.name = "Move " + (currentIndex + 1).ToString();

				moveNumber.AddComponent<RectTransform>();
				moveNumber.AddComponent<CanvasRenderer>();
				moveNumber.AddComponent<TextMeshProUGUI>();

				moveNumber.transform.SetParent(panel.transform);

				rectTransform = moveNumber.GetComponent<RectTransform>();
				rectTransform.sizeDelta = new Vector2(200, 50);
				// x y offset
				rectTransform.anchoredPosition = new Vector2(-450, 0);
				// Anchor middle-center
				rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
				rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
				rectTransform.pivot = new Vector2(0.5f, 0.5f);

				rectTransform.localScale = new Vector2(1, 1);

				text = moveNumber.GetComponent<TextMeshProUGUI>();
				text.fontStyle = FontStyles.Bold;
				text.fontSize = 40;
				text.text = (currentIndex + 1).ToString() + ".";

				// White's move
				GameObject textWhite = new GameObject();
				textWhite.name = "White " + (currentIndex + 1).ToString();

				textWhite.AddComponent<RectTransform>();
				textWhite.AddComponent<CanvasRenderer>();
				textWhite.AddComponent<TextMeshProUGUI>();

				textWhite.transform.SetParent(panel.transform);

				rectTransform = textWhite.GetComponent<RectTransform>();
				rectTransform.sizeDelta = new Vector2(200, 50);
				// x y offset
				rectTransform.anchoredPosition = new Vector2(-150, 0);
				// Anchor middle-center
				rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
				rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
				rectTransform.pivot = new Vector2(0.5f, 0.5f);

				rectTransform.localScale = new Vector2(1, 1);

				text = textWhite.GetComponent<TextMeshProUGUI>();
				text.fontStyle = FontStyles.Bold;
				text.fontSize = 40;
				text.text = board.Text[Board.WhiteIndex][currentIndex];
			
			// Black's turn
			} else {
				// Get the current panel
				panel = GameObject.Find("Line " + (currentIndex + 1).ToString());

				GameObject textBlack = new GameObject();
				textBlack.name = "Black " + (currentIndex + 1).ToString();

				textBlack.AddComponent<RectTransform>();
				textBlack.AddComponent<CanvasRenderer>();
				textBlack.AddComponent<TextMeshProUGUI>();

				textBlack.transform.SetParent(panel.transform);

				rectTransform = textBlack.GetComponent<RectTransform>();
				rectTransform.sizeDelta = new Vector2(200, 50);
				// x y offset
				rectTransform.anchoredPosition = new Vector2(350, 0);
				// Anchor middle-center
				rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
				rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
				rectTransform.pivot = new Vector2(0.5f, 0.5f);

				rectTransform.localScale = new Vector2(1, 1);

				text = textBlack.GetComponent<TextMeshProUGUI>();
				text.fontStyle = FontStyles.Bold;
				text.fontSize = 40;
				text.text = board.Text[Board.BlackIndex][currentIndex];
			}

			scrollRect.verticalNormalizedPosition = 0;
		}
	}
}