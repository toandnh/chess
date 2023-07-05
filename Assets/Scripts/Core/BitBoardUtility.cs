namespace Chess {
	public static class BitBoardUtility {
		public static bool HasSquare(ulong bitBoard, int square) {
			return ((bitBoard >> square) & 1) != 0;
		}
	}
}