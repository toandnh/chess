using NUnit.Framework;
using System.Collections.Generic;

namespace Chess {
	public class ZobristKeyUpdate {
		[Test]
		public void SameInitialZobristKey() {
			Board firstBoard = new Board();
			firstBoard.LoadStartPosition();
			Board secondBoard = new Board();
			secondBoard.LoadStartPosition();

			ulong firstZobristKey = firstBoard.CurrentZobristKey;
			ulong secondZobristKey = secondBoard.CurrentZobristKey;

			Assert.AreEqual(true, firstZobristKey == secondZobristKey);
		}

		[Test]
		public void SameMoveSameZobristKey() {
			Board firstBoard = new Board();
			firstBoard.LoadStartPosition();
			Board secondBoard = new Board();
			secondBoard.LoadStartPosition();

			Move e4 = new Move(12, 28, Move.Flag.PawnTwoForward);

			firstBoard.MakeMove(e4);
			secondBoard.MakeMove(e4);

			ulong firstZobristKey = firstBoard.CurrentZobristKey;
			ulong secondZobristKey = secondBoard.CurrentZobristKey;

			Assert.AreEqual(true, firstZobristKey == secondZobristKey);
		}

		[Test]
		public void UpdateMakeUnmakeMove() {
			Board board = new Board();
			board.LoadStartPosition();

			Move e4 = new Move(12, 28, Move.Flag.PawnTwoForward);

			ulong beforeZobristKey = board.CurrentZobristKey;
			board.MakeMove(e4);
			board.UnmakeMove(e4);
			ulong afterZobristKey = board.CurrentZobristKey;

			Assert.AreEqual(true, beforeZobristKey == afterZobristKey);
		}
	}
}