using NUnit.Framework;
using System.Collections.Generic;

namespace Chess {
	public class CaptureListUpdate {
		[Test]
		public void NonCaptureMove() {
			Board board = new Board();
			board.LoadStartPosition();

			Move e4 = new Move(12, 28, Move.Flag.PawnTwoForward);
			board.MakeMove(e4);

			bool noChange = true;
			for (int colorIndex = 0; colorIndex < 2; colorIndex++) { 
				for (int pieceIndex = 0; pieceIndex < board.Captures[colorIndex].Length; pieceIndex++) { 
					noChange &= board.Captures[colorIndex][pieceIndex] == 0;
				}
			}

			Assert.AreEqual(true, noChange);
		} 

		[Test]
		public void WhitePawnCaptureMove() {
			Board board = new Board();
			board.LoadStartPosition();

			Move e4 = new Move(12, 28, Move.Flag.PawnTwoForward);
			Move d5 = new Move(51, 35, Move.Flag.PawnTwoForward);
			Move xd5 = new Move(28, 35, Move.Flag.None);

			board.MakeMove(e4);
			board.MakeMove(d5);
			board.MakeMove(xd5);

			bool correctlyUpdated = true;
			for (int colorIndex = 0; colorIndex < 2; colorIndex++) {
				for (int pieceIndex = 0; pieceIndex < board.Captures[colorIndex].Length; pieceIndex++) { 
					if (colorIndex == Board.WhiteIndex && pieceIndex == Piece.Pawn) {
						correctlyUpdated &= board.Captures[colorIndex][pieceIndex] == 1;
					} else {
						correctlyUpdated &= board.Captures[colorIndex][pieceIndex] == 0;
					}
				}
			}

			Assert.AreEqual(true, correctlyUpdated);
		}

		[Test]
		public void UnmakeWhitePawnCaptureMove() {
			Board board = new Board();
			board.LoadStartPosition();

			Move e4 = new Move(12, 28, Move.Flag.PawnTwoForward);
			Move d5 = new Move(51, 35, Move.Flag.PawnTwoForward);
			Move xd5 = new Move(28, 35, Move.Flag.None);

			board.MakeMove(e4);
			board.MakeMove(d5);
			board.MakeMove(xd5);

			board.UnmakeMove(xd5);

			bool correctlyUpdated = true;
			for (int colorIndex = 0; colorIndex < 2; colorIndex++) {
				for (int pieceIndex = 0; pieceIndex < board.Captures[colorIndex].Length; pieceIndex++) { 
					correctlyUpdated &= board.Captures[colorIndex][pieceIndex] == 0;
				}
			}

			Assert.AreEqual(true, correctlyUpdated);
		}
	}
}