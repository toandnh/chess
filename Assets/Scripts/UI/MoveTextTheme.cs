using UnityEngine;

namespace Chess.Game {
	[CreateAssetMenu(menuName = "Theme/MoveText")]
	public class MoveTextTheme : ScriptableObject {
		public Color Text;
		public Color Highlighted;

		public LineColors Light;
		public LineColors Dark;

		[System.Serializable]
		public struct LineColors {
			public Color Normal;
		}
	}
}