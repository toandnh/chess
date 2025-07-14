using System.Collections.Generic;

namespace Chess {
	using static BoardRepresentation;

	public class MoveText {
		static Dictionary<int, char> pieceSymbolFromType = new Dictionary<int, char>() {
			[Piece.Pawn] = 'p',
			[Piece.Knight] = 'n',
			[Piece.Bishop] = 'b',
			[Piece.Rook] = 'r',
			[Piece.Queen] = 'q',
			[Piece.King] = 'k'
		};

		public List<List<string>> Text { get; set; }

		public int HighlightIndex = -1;

		public MoveText() {
			Initialize();
		}

		void Initialize() {
			Text = new List<List<string>>();
			Text.Add(new List<string>());
			Text.Add(new List<string>());
		}

		public void GenerateMoveText(Move move, Board board) {
			int moveFrom = move.StartSquare;
			int moveTo = move.TargetSquare;

			int movePiece = board.Square[moveFrom];
			int movePieceType = Piece.PieceType(movePiece);

			// Pawns (pieceType == 1) have no symbols
			string moveText = movePieceType == 1 ? "" : GetPieceText(movePiece).ToString();

			// Capture
			int targetPieceType = Piece.PieceType(board.Square[moveTo]);
			if (targetPieceType != Piece.None) {
				// Build notations
				if (movePieceType == 1) {
					// Pawn capture pawn
					moveText += FileNames[FileIndex(moveFrom)].ToString();
				}
				moveText += 'x';
			}

			moveText += GetSquareText(moveTo);

			if (move.MoveFlag == Move.Flag.Castle) {
				bool kingside = moveTo == g1 || moveTo == g8;
				moveText = kingside ? "O-O" : "O-O-O";
			}

			switch (move.MoveFlag) {
				case Move.Flag.Castle:
					bool kingside = moveTo == g1 || moveTo == g8;
					moveText = kingside ? "O-O" : "O-O-O";

					break;
				case Move.Flag.PromoteToKnight:
				case Move.Flag.PromoteToBishop:
				case Move.Flag.PromoteToRook:
				case Move.Flag.PromoteToQueen:
					int promotePieceType = move.MoveFlag - 2;
					int pieceColor = board.WhiteToMove ? Piece.White : Piece.Black;
					moveText = GetSquareText(moveTo) + GetPieceText(promotePieceType | pieceColor);

					break;
			}

			// Add to moveText list
			Text[board.WhiteToMove ? Board.WhiteIndex : Board.BlackIndex].Add(moveText);

			// ???
			HighlightIndex = Text.Count - 1;
		}

		public void ResetMoveText() {
			Initialize();
		}

		char GetPieceText(int piece) {
			int pieceType = Piece.PieceType(piece);
			bool isWhite = Piece.IsColor(piece, Piece.White);
			return isWhite ? char.ToUpper(pieceSymbolFromType[pieceType]) : pieceSymbolFromType[pieceType];
		}

		string GetSquareText(int square) {
			return FileNames[FileIndex(square)] + RankNames[RankIndex(square)].ToString();
		}
	}
}