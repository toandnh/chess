using NUnit.Framework;
using System.Collections.Generic;

namespace Chess {	
	public class PieceListUpdate {
		Board boardBefore;
		Board boardAfter;

		void LoadCustomPosition(string fen) {
			boardBefore = new Board();
			boardBefore.LoadCustomPosition(fen);
			boardAfter = new Board();
			boardAfter.LoadCustomPosition(fen);
		}

		bool UpdateMakeQuietMove(string fen, Move move) {
			LoadCustomPosition(fen);

			int movePiece = boardBefore.Square[move.StartSquare];
			int movePieceType = Piece.PieceType(movePiece);
			int movePieceColor = Piece.IsColor(movePiece, Piece.White) ? Piece.White : Piece.Black;

			int opponentColor = Piece.IsColor(movePiece, Piece.White) ? Piece.Black : Piece.White;

			int startSquare = move.StartSquare;
			int targetSquare = move.TargetSquare;

			boardAfter.MakeMove(move);

			// Check piece involved in the move updated correctly
			bool correctlyUpdated = true;
			HashSet<int> beforeSet = boardBefore.PieceList.GetValue(movePieceType)[movePieceColor];
			HashSet<int> afterSet = boardAfter.PieceList.GetValue(movePieceType)[movePieceColor];

			if (beforeSet.Count != afterSet.Count) correctlyUpdated = false;

			correctlyUpdated &= beforeSet.Contains(startSquare) && !beforeSet.Contains(targetSquare) && 
													!afterSet.Contains(startSquare) && afterSet.Contains(targetSquare);
			foreach (int square in beforeSet) {
				if (square != startSquare) {
					correctlyUpdated &= afterSet.Contains(square);
				}
			}

			// Make sure other lists are not affected
			bool otherListsUntouched = true;
			beforeSet = boardBefore.PieceList.GetValue(movePieceType)[opponentColor];
			afterSet = boardAfter.PieceList.GetValue(movePieceType)[opponentColor];

			if (beforeSet.Count != afterSet.Count) otherListsUntouched = false;
			foreach (int square in beforeSet) {
				otherListsUntouched &= afterSet.Contains(square);
			}

			for (int pieceType = 1; pieceType < 6; pieceType++) {
				// Already handled
				if (pieceType == movePieceType) continue;

				for (int color = 8; color <= 16; color += 8) {
					beforeSet = boardBefore.PieceList.GetValue(pieceType)[color];
					afterSet = boardAfter.PieceList.GetValue(pieceType)[color];
					if (beforeSet.Count != afterSet.Count) {
						otherListsUntouched = false;
						break;
					}
					foreach (int square in beforeSet) {
						otherListsUntouched &= afterSet.Contains(square);
					}
				}

				if (!otherListsUntouched) break;
			}

			return correctlyUpdated && otherListsUntouched;
		}
		
		bool UpdateUnmakeQuietMove(string fen, Move move) {
			LoadCustomPosition(fen);

			boardAfter.MakeMove(move);
			boardAfter.UnmakeMove(move);

			bool unchanged = true;
			for (int pieceType = 1; pieceType < 6; pieceType++) {
				for (int color = 8; color <= 16; color += 8) {
					HashSet<int> beforeSet = boardBefore.PieceList.GetValue(pieceType)[color];
					HashSet<int> afterSet = boardAfter.PieceList.GetValue(pieceType)[color];
					if (beforeSet.Count != afterSet.Count) {
						unchanged = false;
						break;
					}
					foreach (int square in beforeSet) {
						unchanged &= afterSet.Contains(square);
					}
				}
				if (!unchanged) break;
			}
			return unchanged;
		}

		[Test]
		public void UpdateMakeQuietPawnMove() {
			string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
			Move e4 = new Move(12, 28, Move.Flag.PawnTwoForward);

			Assert.AreEqual(true, UpdateMakeQuietMove(startFen, e4));
		}

		[Test]
		public void UpdateUnmakeQuietPawnMove() {
			string startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
			Move e4 = new Move(12, 28, Move.Flag.PawnTwoForward);

			Assert.AreEqual(true, UpdateUnmakeQuietMove(startFen, e4));
		}
	}
}
