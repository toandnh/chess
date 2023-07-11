using UnityEngine;

namespace Chess.Game {
	[CreateAssetMenu(menuName = "Theme/Board")]
	public class BoardTheme : ScriptableObject {
		public Color labelSquares;
		public Color menuSquares;
		public Color selected;
		public Color highlighted;
		public Color legal;

		public SquareColors lightSquares;
		public SquareColors darkSquares;

		[System.Serializable]
		public struct SquareColors {
			public Color normal;
		}
	}
}