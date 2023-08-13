using NUnit.Framework;
using System.Collections.Generic;

namespace Chess {
	public class MoveGeneration {
		Board board;
		MoveGenerator moveGenerator;

		void Initialize() {
			board = new Board();
			board.LoadStartPosition();

			moveGenerator = new MoveGenerator();
		}

		int MoveGenerationTest(int depth) {
			if (depth == 0) return 1;

			List<Move> moves = moveGenerator.GenerateMoves(board);
			
			int numPositions = 0;
			foreach (Move move in moves) {
				board.MakeMove(move);
				numPositions += MoveGenerationTest(depth - 1);
				board.UnmakeMove(move);
			}

			return numPositions;
		}

		[Test]
		public void MoveGenerationStartingFENDepth1() {
			Initialize();
			Assert.AreEqual(20, MoveGenerationTest(1));
		}

		[Test]
		public void MoveGenerationStartingFENDepth2() {
			Initialize();
			Assert.AreEqual(400, MoveGenerationTest(2));
		}

		[Test]
		public void MoveGenerationStartingFENDepth3() {
			Initialize();
			Assert.AreEqual(8902, MoveGenerationTest(3));
		}

		[Test]
		public void MoveGenerationStartingFENDepth4() {
			Initialize();
			Assert.AreEqual(197281, MoveGenerationTest(4));
		}

		[Test]
		public void MoveGenerationStartingFENDepth5() {
			Initialize();
			Assert.AreEqual(4865609, MoveGenerationTest(5));
		}

		[Test]
		public void MoveGenerationStartingFENDepth6() {
			Initialize();
			Assert.AreEqual(119060324, MoveGenerationTest(6));
		}
	}
}
