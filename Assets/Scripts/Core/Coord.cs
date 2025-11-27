using System;

namespace Chess {
	public readonly struct Coord : IComparable<Coord> {
		public readonly int FileIndex;
		public readonly int RankIndex;

		public Coord(int fileIndex, int rankIndex) {
			FileIndex = fileIndex;
			RankIndex = rankIndex;
		}

		public bool IsLightSquare() {
			return (FileIndex + RankIndex) % 2 != 0;
		}

		public int CompareTo(Coord other) {
			return (FileIndex == other.FileIndex && RankIndex == other.RankIndex) ? 0 : 1;
		}
	}
}