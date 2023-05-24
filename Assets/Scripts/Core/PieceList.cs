using System.Collections.Generic;

namespace Chess {
	public class PieceList {
		Dictionary<int, Dictionary<int, HashSet<int>>> allPieceLists;

		public PieceList() {
			allPieceLists = new Dictionary<int, Dictionary<int, HashSet<int>>>() {
				{Piece.Pawn, new Dictionary<int, HashSet<int>>() {
					{Piece.White, new HashSet<int>()},
					{Piece.Black, new HashSet<int>()},
				}},
				{Piece.Rook, new Dictionary<int, HashSet<int>>() {
					{Piece.White, new HashSet<int>()},
					{Piece.Black, new HashSet<int>()},
				}},
				{Piece.Knight, new Dictionary<int, HashSet<int>>() {
					{Piece.White, new HashSet<int>()},
					{Piece.Black, new HashSet<int>()},
				}},
				{Piece.Bishop, new Dictionary<int, HashSet<int>>() {
					{Piece.White, new HashSet<int>()},
					{Piece.Black, new HashSet<int>()},
				}},
				{Piece.Queen, new Dictionary<int, HashSet<int>>() {
					{Piece.White, new HashSet<int>()},
					{Piece.Black, new HashSet<int>()},
				}},
				{Piece.King, new Dictionary<int, HashSet<int>>() {
					{Piece.White, new HashSet<int>()},
					{Piece.Black, new HashSet<int>()},
				}}
			};
		}

		public Dictionary<int, HashSet<int>> GetValue(int piece) {
			return allPieceLists[piece];
		}

		public void Add(int piece, int color, int square) {
			(allPieceLists[piece])[color].Add(square);
		}

		public void Update(int piece, int color, int currentSquare, int updatedSquare) {
			(allPieceLists[piece])[color].Remove(currentSquare);
			(allPieceLists[piece])[color].Add(updatedSquare);
		}

		public void Remove(int piece, int color, int square) {
			(allPieceLists[piece])[color].Remove(square);
		}
	}
}