namespace Chess {
	public static class Piece {
		public const int None = 0;
		public const int Pawn = 1;
		public const int Knight = 2;
		public const int Bishop = 3;
		public const int Rook = 4;
		public const int Queen = 5;
		public const int King = 6;
		
		public const int White = 8;
		public const int Black = 16;

		const int typeMask = 0b00111;
		const int whiteMask = 0b01000;
		const int blackMask = 0b10000;
		const int colorMask = whiteMask | blackMask;

		public static bool IsColor(int piece, int color) {
			return (piece & colorMask) == color;
		}

		public static int PieceColor(int piece) {
			return piece & colorMask;
		}

		public static int PieceType(int piece) {
			return piece & typeMask;
		}
	}
}