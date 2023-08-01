using UnityEngine;

namespace Chess.Game {
	[CreateAssetMenu(menuName = "Theme/Board")]
	public class BoardTheme : ScriptableObject {
		public Color LabelSquares;
		public Color MenuSquares;
		public Color Selected;
		public Color Highlighted;
		public Color Legal;

		public Color MoveTextLight;
		public Color MoveTextDark;

		public SquareColors LightSquares;
		public SquareColors DarkSquares;

		[System.Serializable]
		public struct SquareColors {
			public Color Normal;
		}
	}
}