using System.Collections.Generic;

using static System.Math;

namespace Chess {
	public static class PrecomputedMoveData {
		//																							{ N, S, E, W, NW, SE, NE, SW }
		public static readonly int[] DirectionOffsets = { 8, -8, 1, -1, 7, -7, 9, -9 };
		public static readonly Dictionary<int, int> DirectionIndices = new Dictionary<int, int>() {
			{ 8, 0 },  // North
			{ -8, 1 }, // South
			{ 1, 2 },	 // East
			{ -1, 3 }, // West
			{ 7, 4 },  // NorthWest
			{ -7, 5 }, // SouthEast
			{ 9, 6 },  // NorthEast
			{ -9, 7 }  // SouthWest
		};

		public static readonly int North = 8;
		public static readonly int East = 1;
		public static readonly int NorthWest = 7;

		// Store number of moves available in each of 8 directions to the edge of the board
		public static readonly int[][] NumSquaresToEdge;

		// Store all the square indices a king/knight can move to
		public static readonly byte[][] KingMoves;
		public static readonly byte[][] KnightMoves;

		// Store the king/knight/pawn attacks on bit board
		public static readonly ulong[] KingAttackBitBoard;
		public static readonly ulong[] KnightAttackBitBoard;
		public static readonly ulong[][] PawnAttackBitBoard;

		// Store the legal pawn captures
		public static readonly int[][][] LegalPawnCaptures;

		// Pawn attack directions for black and white
		public static readonly byte[][] PawnAttackDirections = {
			new byte[] { 4, 6 },
			new byte[] { 5, 7 }
		};

		static PrecomputedMoveData() {
			NumSquaresToEdge = new int[64][];

			KingMoves = new byte[64][];
			KnightMoves = new byte[64][];

			KingAttackBitBoard = new ulong[64];
			KnightAttackBitBoard = new ulong[64];
			PawnAttackBitBoard = new ulong[64][];

			LegalPawnCaptures = new int[64][][];

			for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
				int y = squareIndex / 8;
				int x = squareIndex - 8 * y;

				int north = 7 - y;
				int south = y;
				int east = 7 - x;
				int west = x;

				NumSquaresToEdge[squareIndex] = new int[8];

				NumSquaresToEdge[squareIndex][0] = north;
				NumSquaresToEdge[squareIndex][1] = south;
				NumSquaresToEdge[squareIndex][2] = east;
				NumSquaresToEdge[squareIndex][3] = west;

				NumSquaresToEdge[squareIndex][4] = Min(north, west);
				NumSquaresToEdge[squareIndex][5] = Min(south, east);
				NumSquaresToEdge[squareIndex][6] = Min(north, east);
				NumSquaresToEdge[squareIndex][7] = Min(south, west);

				// Calculate all squares king can move to from current square
				var legalKingMoves = new List<byte>();
				ulong kingBitBoard = 0;
				foreach(int kingMoveDelta in DirectionOffsets) {
					int kingMoveSquare = squareIndex + kingMoveDelta;
					if (kingMoveSquare >= 0 && kingMoveSquare < 64) {
						int kingSquareY = kingMoveSquare / 8;
						int kingSquareX = kingMoveSquare - 8 * kingSquareY;

						// Ensure king has move max 1 square on x/y axis
						int maxCoordMoveDst = Max(Abs(x - kingSquareX), Abs(y - kingSquareY));
						if (maxCoordMoveDst == 1) {
							legalKingMoves.Add((byte) kingMoveSquare);
							kingBitBoard |= 1ul << kingMoveSquare;
						}
					}
				}
				KingMoves[squareIndex] = legalKingMoves.ToArray();
				KingAttackBitBoard[squareIndex] = kingBitBoard;

				// Calculate all squares knight can jump to from current square
				int[] allKnightJumps = { 15, 17, -17, -15, 10, -6, 6, -10 };
				var legalKnightJumps = new List<byte>();
				ulong knightBitBoard = 0;
				foreach (int knightJumpDelta in allKnightJumps) {
					int knightJumpSquare = squareIndex + knightJumpDelta;
					if (knightJumpSquare >= 0 && knightJumpSquare < 64) {
						int knightSquareY = knightJumpSquare / 8;
						int knightSquareX = knightJumpSquare - 8 * knightSquareY;

						// Ensure knight has move max 2 square on x/y axis
						int maxCoordMoveDst = Max(Abs(x - knightSquareX), Abs(y - knightSquareY));
						if (maxCoordMoveDst == 2) {
							legalKnightJumps.Add((byte) knightJumpSquare);
							knightBitBoard |= 1ul << knightJumpSquare;
						}
					}
				}
				KnightMoves[squareIndex] = legalKnightJumps.ToArray();
				KnightAttackBitBoard[squareIndex] = knightBitBoard;

				// Calculate all legal pawn captures
				List<int> legalWhitePawnCaptures = new List<int>();
				List<int> legalBlackPawnCaptures = new List<int>();
				LegalPawnCaptures[squareIndex] = new int[2][];
				PawnAttackBitBoard[squareIndex] = new ulong[2];
				if (x > 0) {
					if (y < 7) {
						legalWhitePawnCaptures.Add(squareIndex + 7);
						PawnAttackBitBoard[squareIndex][Board.WhiteIndex] |= 1ul << (squareIndex + 7);
					} 
					if (y > 0) {
						legalBlackPawnCaptures.Add(squareIndex - 9);
						PawnAttackBitBoard[squareIndex][Board.BlackIndex] |= 1ul << (squareIndex - 9);
					}
				}
				if (x < 7) {
					if (y < 7) {
						legalWhitePawnCaptures.Add(squareIndex + 9);
						PawnAttackBitBoard[squareIndex][Board.WhiteIndex] |= 1ul << (squareIndex + 9);
					}
					if (y > 0) {
						legalBlackPawnCaptures.Add(squareIndex - 7);
						PawnAttackBitBoard[squareIndex][Board.BlackIndex] |= 1ul << (squareIndex - 7);
					}
				}
				LegalPawnCaptures[squareIndex][Board.WhiteIndex] = legalWhitePawnCaptures.ToArray();
				LegalPawnCaptures[squareIndex][Board.BlackIndex] = legalBlackPawnCaptures.ToArray();
			}
		}
	}
}