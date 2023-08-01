using System.Collections.Generic;

namespace Chess {
	using static BoardRepresentation;

	public static class MoveText {
		static Dictionary<int, char> pieceSymbolFromType = new Dictionary<int, char>() {
			[Piece.Pawn] = 'p',
			[Piece.Knight] = 'n',
			[Piece.Bishop] = 'b',
			[Piece.Rook] = 'r',
			[Piece.Queen] = 'q',
			[Piece.King] = 'k'
		};

		public static char GetPieceText(int piece) {
			int pieceType = Piece.PieceType(piece);
			bool isWhite = Piece.IsColor(piece, Piece.White);
			return isWhite ? char.ToUpper(pieceSymbolFromType[pieceType]) : pieceSymbolFromType[pieceType];
		}

		public static string GetSquareText(int square) {
			return FileNames[FileIndex(square)] + RankNames[RankIndex(square)].ToString();
		}
	}
}