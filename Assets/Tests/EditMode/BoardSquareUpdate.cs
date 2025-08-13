using NUnit.Framework;
using System.Collections.Generic;

namespace Chess {
	public class BoardSquareUpdate {
		Board board;

		void LoadCustomPosition(string fen) {
			board = new Board();
			board.LoadCustomPosition(fen);
		}

		[Test]
		public void UpdateMakeQuietMove() {
			string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
			LoadCustomPosition(startFen);

			Move e4 = new Move(12, 28, Move.Flag.PawnTwoForward);

			int[] boardBefore = (int[]) board.Square.Clone();
			board.MakeMove(e4);

			bool correctlyUpdated = true;
			for (int i = 0; i < boardBefore.Length; i++) {
				if (i == 12 || i == 28) continue;
				correctlyUpdated &= boardBefore[i] == board.Square[i];
			}
			correctlyUpdated &= board.Square[12] == Piece.None;
			correctlyUpdated &= board.Square[28] == (Piece.Pawn | Piece.White);

			Assert.AreEqual(true, correctlyUpdated);
		}

		[Test]
		public void UpdateUnmakeQuietMove() {
			string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
			LoadCustomPosition(startFen);

			Move e4 = new Move(12, 28, Move.Flag.PawnTwoForward);

			int[] boardBefore = (int[]) board.Square.Clone();
			board.MakeMove(e4);
			board.UnmakeMove(e4);

			bool correctlyUpdated = true;
			for (int i = 0; i < boardBefore.Length; i++) {
				correctlyUpdated &= boardBefore[i] == board.Square[i];
			}

			Assert.AreEqual(true, correctlyUpdated);
		}

		[Test]
		public void UpdateMakeCaptureMove() {
			string startFen = "rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 1";
			LoadCustomPosition(startFen);

			Move ed5 = new Move(28, 35, Move.Flag.Capture);

			int[] boardBefore = (int[]) board.Square.Clone();
			board.MakeMove(ed5);

			bool correctlyUpdated = true;
			for (int i = 0; i < boardBefore.Length; i++) {
				if (i == 28 || i == 35) continue;
				correctlyUpdated &= boardBefore[i] == board.Square[i];
			}
			correctlyUpdated &= board.Square[28] == Piece.None;
			correctlyUpdated &= board.Square[35] == (Piece.Pawn | Piece.White);

			Assert.AreEqual(true, correctlyUpdated);
		}

		[Test]
		public void UpdateUnmakeCaptureMove() {
			string startFen = "rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 1";
			LoadCustomPosition(startFen);

			Move ed5 = new Move(28, 35, Move.Flag.Capture);

			int[] boardBefore = (int[]) board.Square.Clone();
			board.MakeMove(ed5);
			board.UnmakeMove(ed5);

			bool correctlyUpdated = true;
			for (int i = 0; i < boardBefore.Length; i++) {
				correctlyUpdated &= boardBefore[i] == board.Square[i];
			}

			Assert.AreEqual(true, correctlyUpdated);
		}

		[Test]
		public void UpdateMakeEnPassantMove() {
			string startFen = "rnbqkbnr/ppp2ppp/8/3Pp3/8/8/PPPP1PPP/RNBQKBNR w KQkq e6 0 1";
			LoadCustomPosition(startFen);

			Move de6 = new Move(35, 44, Move.Flag.EnPassant);

			int[] boardBefore = (int[]) board.Square.Clone();
			board.MakeMove(de6);

			bool correctlyUpdated = true;
			for (int i = 0; i < boardBefore.Length; i++) {
				if (i == 35 || i == 36 || i == 44) continue;
				correctlyUpdated &= boardBefore[i] == board.Square[i];
			}
			
			correctlyUpdated &= board.Square[35] == Piece.None;
			correctlyUpdated &= board.Square[36] == Piece.None;
			correctlyUpdated &= board.Square[44] == (Piece.Pawn | Piece.White);

			Assert.AreEqual(true, correctlyUpdated);
		}

		[Test]
		public void UpdateUnmakeEnPassantMove() {
			string startFen = "rnbqkbnr/ppp2ppp/8/3Pp3/8/8/PPPP1PPP/RNBQKBNR w KQkq e6 0 1";
			LoadCustomPosition(startFen);

			Move de6 = new Move(35, 44, Move.Flag.EnPassant);

			int[] boardBefore = (int[]) board.Square.Clone();
			board.MakeMove(de6);
			board.UnmakeMove(de6);

			bool correctlyUpdated = true;
			for (int i = 0; i < boardBefore.Length; i++) {
				correctlyUpdated &= boardBefore[i] == board.Square[i];
			}

			Assert.AreEqual(true, correctlyUpdated);
		}

		[Test]
		public void UpdateMakeCastleKingSideMove() {
			string startFen = "rnbqk2r/pppbpppp/5n2/3p4/4P3/5N2/PPPPBPPP/RNBQK2R w KQkq - 0 1";
			LoadCustomPosition(startFen);

			Move OO = new Move(4, 6, Move.Flag.Castle);

			int[] boardBefore = (int[]) board.Square.Clone();
			board.MakeMove(OO);

			bool correctlyUpdated = true;
			for (int i = 0; i < boardBefore.Length; i++) {
				if (i >= 4 && i <= 7) continue;
				correctlyUpdated &= boardBefore[i] == board.Square[i];
			}

			correctlyUpdated &= board.Square[4] == Piece.None;
			correctlyUpdated &= board.Square[5] == (Piece.Rook | Piece.White);
			correctlyUpdated &= board.Square[6] == (Piece.King | Piece.White);
			correctlyUpdated &= board.Square[7] == Piece.None;

			Assert.AreEqual(true, correctlyUpdated);
		}

		[Test]
		public void UpdateUnmakeCastleKingSideMove() {
			string startFen = "rnbqk2r/pppbpppp/5n2/3p4/4P3/5N2/PPPPBPPP/RNBQK2R w KQkq - 0 1";
			LoadCustomPosition(startFen);

			Move OO = new Move(4, 6, Move.Flag.Castle);

			int[] boardBefore = (int[]) board.Square.Clone();
			board.MakeMove(OO);
			board.UnmakeMove(OO);

			bool correctlyUpdated = true;
			for (int i = 0; i < boardBefore.Length; i++) {
				correctlyUpdated &= boardBefore[i] == board.Square[i];
			}

			Assert.AreEqual(true, correctlyUpdated);
		}

		[Test]
		public void UpdateMakeCastleQueenSideMove() {
			string startFen = "r3kbnr/pppqpppp/2n1b3/3p4/3P4/2N1B3/PPPQPPPP/R3KBNR w KQkq - 0 1";
			LoadCustomPosition(startFen);

			Move OOO = new Move(4, 2, Move.Flag.Castle);

			int[] boardBefore = (int[]) board.Square.Clone();
			board.MakeMove(OOO);

			bool correctlyUpdated = true;
			for (int i = 0; i < boardBefore.Length; i++) {
				if (i >= 0 && i <= 4) continue;
				correctlyUpdated &= boardBefore[i] == board.Square[i];
			}

			correctlyUpdated &= board.Square[0] == Piece.None;
			correctlyUpdated &= board.Square[1] == Piece.None;
			correctlyUpdated &= board.Square[2] == (Piece.King | Piece.White);
			correctlyUpdated &= board.Square[3] == (Piece.Rook | Piece.White);
			correctlyUpdated &= board.Square[4] == Piece.None;

			Assert.AreEqual(true, correctlyUpdated);
		}

		[Test]
		public void UpdateUnmakeCastleQueenSideMove() {
			string startFen = "r3kbnr/pppqpppp/2n1b3/3p4/3P4/2N1B3/PPPQPPPP/R3KBNR w KQkq - 0 1";
			LoadCustomPosition(startFen);

			Move OOO = new Move(4, 2, Move.Flag.Castle);

			int[] boardBefore = (int[]) board.Square.Clone();
			board.MakeMove(OOO);
			board.UnmakeMove(OOO);

			bool correctlyUpdated = true;
			for (int i = 0; i < boardBefore.Length; i++) {
				correctlyUpdated &= boardBefore[i] == board.Square[i];
			}

			Assert.AreEqual(true, correctlyUpdated);
		}
	}
}
