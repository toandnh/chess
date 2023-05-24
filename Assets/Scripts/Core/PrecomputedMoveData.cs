using System.Collections.Generic;
using static System.Math;

namespace Chess {
	public static class PrecomputedMoveData {
		//																							{ N, S, W, E, NW, SE, NE, SW }
		public static readonly int[] DirectionOffsets = { 8, -8, -1, 1, 7, -7, 9, -9 };
		// Store number of moves available in each of 8 directions to the edge of the board
		public static readonly int[][] NumSquaresToEdge;

		// Store all the square indices a king can move to
		public static readonly byte[][] KingMoves;
		// Store all the square indices a knight can land on
		public static readonly byte[][] KnightMoves;

		// Pawn attack directions for black and white
		public static readonly byte[][] PawnAttackDirections = {
			new byte[] { 4, 6 },
			new byte[] { 7, 5 }
		};

		static PrecomputedMoveData() {
			NumSquaresToEdge = new int[64][];

			KingMoves = new byte[64][];
			KnightMoves = new byte[64][];

			int[] allKnightJumps = { 15, 17, -17, -15, 10, -6, 6, -10 };

			for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
				int y = squareIndex / 8;
				int x = squareIndex - 8 * y;

				int north = 7 - y;
				int south = y;
				int west = x;
				int east = 7 - x;

				NumSquaresToEdge[squareIndex] = new int[8];

				NumSquaresToEdge[squareIndex][0] = north;
				NumSquaresToEdge[squareIndex][1] = south;
				NumSquaresToEdge[squareIndex][2] = west;
				NumSquaresToEdge[squareIndex][3] = east;

				NumSquaresToEdge[squareIndex][4] = Min(north, west);
				NumSquaresToEdge[squareIndex][5] = Min(south, east);
				NumSquaresToEdge[squareIndex][6] = Min(north, east);
				NumSquaresToEdge[squareIndex][7] = Min(south, west);

				// Calculate all squares knight can jump to from current square
				var legalKnightJumps = new List<byte>();
				foreach (int knightJumpDelta in allKnightJumps) {
					int knightJumpSquare = squareIndex + knightJumpDelta;
					if (knightJumpSquare >= 0 && knightJumpSquare < 64) {
						int knightSquareY = knightJumpSquare / 8;
						int knightSquareX = knightJumpSquare - 8 * knightSquareY;

						// Ensure knight has move max 2 square on x/y axis
						int maxCoordMoveDst = Max(Abs(x - knightSquareX), Abs(y - knightSquareY));
						if (maxCoordMoveDst == 2) {
							legalKnightJumps.Add((byte) knightJumpSquare);
						}
					}
				}
				KnightMoves[squareIndex] = legalKnightJumps.ToArray();

				// Calculate all squares king can move to from current square
				var legalKingMoves = new List<byte>();
				foreach(int kingMoveDelta in DirectionOffsets) {
					int kingMoveSquare = squareIndex + kingMoveDelta;
					if (kingMoveSquare >= 0 && kingMoveSquare < 64) {
						int kingSquareY = kingMoveSquare / 8;
						int kingSquareX = kingMoveSquare - 8 * kingSquareY;

						// Ensure king has move max 1 square on x/y axis
						int maxCoordMoveDst = Max(Abs(x - kingSquareX), Abs(y - kingSquareY));
						if (maxCoordMoveDst == 1) {
							legalKingMoves.Add((byte) kingMoveSquare);
						}
					}
				}
				KingMoves[squareIndex] = legalKingMoves.ToArray();

				//
			}
		}
	}
}