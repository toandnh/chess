using UnityEngine;

namespace Chess.Game {
	[CreateAssetMenu(menuName = "Theme/Board")]
	public class BoardTheme : ScriptableObject {
		public SquareColors lightSquares;
		public SquareColors darkSquares;

		[System.Serializable]
		public struct SquareColors {
			public Color normal;
			public Color selected;
		}
	}
}