using System.Collections.Generic;

namespace Chess {
	public static class FenUtility {
		public class LoadedPositionInfo {
			public int[] squares;
			public bool whiteCastleKingSide;
			public bool whiteCastleQueenSide;
			public bool blackCastleKingSide;
			public bool blackCastleQueenSide;
			public int epFile;
			public bool whiteToMove;
			public int plyCount;

			public LoadedPositionInfo() {
				squares = new int[64];
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
						loadedPositionInfo.squares[rank * 8 + file] = pieceColor | pieceType;
						file++;
					}
				}
			}

			// Active color
			loadedPositionInfo.whiteToMove = sections[1] == "w";

			// Castling rights
			string castlingRights = sections[2];
			loadedPositionInfo.whiteCastleKingSide = castlingRights.Contains('K');
			loadedPositionInfo.whiteCastleQueenSide = castlingRights.Contains('Q');
			loadedPositionInfo.blackCastleKingSide = castlingRights.Contains('k');
			loadedPositionInfo.blackCastleQueenSide = castlingRights.Contains('q');

			// Possible enPassant target square
			char enPassantFileName = sections[3][0];
			if (BoardRepresentation.FileNames.Contains(enPassantFileName)) {
				loadedPositionInfo.epFile = BoardRepresentation.FileNames.IndexOf(enPassantFileName) + 1;
			}

			// Halfmove clock
			int.TryParse(sections[4], out loadedPositionInfo.plyCount);

			return loadedPositionInfo;
		}
	}
}