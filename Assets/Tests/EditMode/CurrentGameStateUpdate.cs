using NUnit.Framework;
using System.Collections.Generic;

namespace Chess {
	public class CurrentGameStateUpdate {
		[Test]
		public void InitialGameState() {
			Board board = new Board();
			board.LoadStartPosition();

			int castleRights = (int) board.CurrentGameState & 0b00000001111;
			int epFile = (int) (board.CurrentGameState & 0b00011110000) >> 4;
			int capture = (int) (board.CurrentGameState & 0b11100000000) >> 8; 

			bool correctlyLoaded = true;
			correctlyLoaded &= castleRights == 0b1111;
			correctlyLoaded &= epFile == 0;
			correctlyLoaded &= capture == 0;

			Assert.AreEqual(true, correctlyLoaded);
		}

		[Test]
		public void PawnTwoForwardMove() {
			Board board = new Board();
			board.LoadStartPosition();

			Move e4 = new Move(12, 28, Move.Flag.PawnTwoForward);
			board.MakeMove(e4);

			int castleRights = (int) board.CurrentGameState & 0b00000001111;
			int epFile = (int) (board.CurrentGameState & 0b00011110000) >> 4;
			int capture = (int) (board.CurrentGameState & 0b11100000000) >> 8; 

			bool correctlyUpdated = true;
			correctlyUpdated &= castleRights == 0b1111;
			correctlyUpdated &= epFile == 5;
			correctlyUpdated &= capture == 0;

			Assert.AreEqual(true, correctlyUpdated);
		}

		[Test]
		public void PawnCaptureMove() {
			Board board = new Board();
			board.LoadStartPosition();

			Move e4 = new Move(12, 28, Move.Flag.PawnTwoForward);
			Move d5 = new Move(51, 35, Move.Flag.PawnTwoForward);
			Move xd5 = new Move(28, 35, Move.Flag.Capture);

			board.MakeMove(e4);
			board.MakeMove(d5);
			board.MakeMove(xd5);

			int castleRights = (int) board.CurrentGameState & 0b00000001111;
			int epFile = (int) (board.CurrentGameState & 0b00011110000) >> 4;
			int capture = (int) (board.CurrentGameState & 0b11100000000) >> 8; 

			bool correctlyUpdated = true;
			correctlyUpdated &= castleRights == 0b1111;
			correctlyUpdated &= epFile == 0;
			correctlyUpdated &= capture == 1;

			Assert.AreEqual(true, correctlyUpdated);
		}

		[Test]
		public void UnmakePawnCaptureMove() {
			Board board = new Board();
			board.LoadStartPosition();

			Move e4 = new Move(12, 28, Move.Flag.PawnTwoForward);
			Move d5 = new Move(51, 35, Move.Flag.PawnTwoForward);
			Move xd5 = new Move(28, 35, Move.Flag.None);

			board.MakeMove(e4);
			board.MakeMove(d5);
			board.MakeMove(xd5);

			board.UnmakeMove(xd5);

			int castleRights = (int) board.CurrentGameState & 0b00000001111;
			int epFile = (int) (board.CurrentGameState & 0b00011110000) >> 4;
			int capture = (int) (board.CurrentGameState & 0b11100000000) >> 8; 

			bool correctlyUpdated = true;
			correctlyUpdated &= castleRights == 0b1111;
			correctlyUpdated &= epFile == 4;
			correctlyUpdated &= capture == 0;

			Assert.AreEqual(true, correctlyUpdated);
		}
	}
}