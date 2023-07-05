using System.Collections.Generic;

namespace Chess {
	public static class FenUtility {
		public class LoadedPositionInfo {
			public int[] Squares;
			public bool WhiteCastleKingSide;
			public bool WhiteCastleQueenSide;
			public bool BlackCastleKingSide;
			public bool BlackCastleQueenSide;
			public int EpFile;
			public bool WhiteToMove;
			public int PlyCount;

			public LoadedPositionInfo() {
				Squares = new int[64];
			}
		}

		static Dictionary<char, int> pieceTypeFromSymbol = new Dictionary<char, int>() {
			['p'] = Piece.Pawn,
			['r'] = Piece.Rook,
			['n'] = Piece.Knight,
			['b'] = Piece.Bishop,
			['k'] = Piece.King,
			['q'] = Piece.Queen
		};
		public const string StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

		public static LoadedPositionInfo PositionFromFen(string fen) {
			LoadedPositionInfo loadedPositionInfo = new LoadedPositionInfo();
			string[] sections = fen.Split(' ');

			int file = 0;
			int rank = 7;

			// Piece placement
			foreach (char symbol in sections[0]) {
				if (symbol == '/') {
					file = 0;
					rank--;
				} else {
					if (char.IsDigit(symbol)) {
						file += (int)char.GetNumericValue(symbol);
					} else {
						int pieceColor = char.IsUpper(symbol) ? Piece.White : Piece.Black;
						int pieceType = pieceTypeFromSymbol[char.ToLower(symbol)];
						loadedPositionInfo.Squares[rank * 8 + file] = pieceColor | pieceType;
						file++;
					}
				}
			}

			// Active color
			loadedPositionInfo.WhiteToMove = sections[1] == "w";

			// Castling rights
			string castlingRights = sections[2];
			loadedPositionInfo.WhiteCastleKingSide = castlingRights.Contains('K');
			loadedPositionInfo.WhiteCastleQueenSide = castlingRights.Contains('Q');
			loadedPositionInfo.BlackCastleKingSide = castlingRights.Contains('k');
			loadedPositionInfo.BlackCastleQueenSide = castlingRights.Contains('q');

			// Possible enPassant target square
			string enPassantFileName = sections[3][0].ToString();
			if (BoardRepresentation.FileNames.Contains(enPassantFileName)) {
				loadedPositionInfo.EpFile = BoardRepresentation.FileNames.IndexOf(enPassantFileName) + 1;
			}

			// Halfmove clock
			int.TryParse(sections[4], out loadedPositionInfo.PlyCount);

			return loadedPositionInfo;
		}
	}
}