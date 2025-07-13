using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace Chess.Game {
	public class MoveTextUI : MonoBehaviour
	{
		public MoveTextTheme moveTextTheme;

		public TMP_FontAsset FontAsset;

		Transform content;
		ScrollRect scrollRect;

		void Awake() {
			content = GameObject.Find("Content").transform;
			scrollRect = GameObject.Find("Scroll View").GetComponent<ScrollRect>();
		}

		public void OnMoveMade(MoveText moveText, bool whiteToMove) {
			UpdateMoveText(moveText, whiteToMove);
		}

		public void ResetMoveText() {
			while (content.childCount > 0) {
				DestroyImmediate(content.GetChild(0).gameObject);
			}
		}

		void UpdateMoveText(MoveText moveText, bool whiteToMove) {
			int colorIndex = whiteToMove ? Board.BlackIndex : Board.WhiteIndex;
			int currentIndex = moveText.Text[colorIndex].Count - 1;

			float horOffset = whiteToMove ? 350 : -150;

			GameObject panel;
			RectTransform rectTransform;
			TextMeshProUGUI text;

			// Get the current panel
			panel = GameObject.Find("Line " + (currentIndex + 1).ToString());
			// Create new one if it's not there
			if (!panel) {
				panel = new GameObject {
					name = "Line " + (currentIndex + 1).ToString()
				};

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
				image.color = currentIndex % 2 == 0 ? moveTextTheme.Light.Normal : moveTextTheme.Dark.Normal;

				// TextMeshPro
				// Move number
				GameObject moveNumber = new GameObject {
					name = "Move " + (currentIndex + 1).ToString()
				};

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
				text.font = FontAsset;
				text.enableAutoSizing = true;
				text.color = moveTextTheme.Text;
				text.text = (currentIndex + 1).ToString() + ".";
			}

			GameObject currText = new GameObject {
				name = whiteToMove ? "Black " + (currentIndex + 1).ToString() : "White " + (currentIndex + 1).ToString()
			};

			currText.AddComponent<RectTransform>();
			currText.AddComponent<CanvasRenderer>();
			currText.AddComponent<TextMeshProUGUI>();

			currText.transform.SetParent(panel.transform);

			rectTransform = currText.GetComponent<RectTransform>();
			rectTransform.sizeDelta = new Vector2(200, 50);
			// x y offset
			rectTransform.anchoredPosition = new Vector2(horOffset, 0);
			// Anchor middle-center
			rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			rectTransform.pivot = new Vector2(0.5f, 0.5f);

			rectTransform.localScale = new Vector2(1, 1);

			text = currText.GetComponent<TextMeshProUGUI>();
			text.fontStyle = FontStyles.Bold;
			text.font = FontAsset;
			text.enableAutoSizing = true;
			text.color = moveTextTheme.Text;
			text.text = moveText.Text[colorIndex][currentIndex];
			
			scrollRect.verticalNormalizedPosition = 0;
		}
	}
}