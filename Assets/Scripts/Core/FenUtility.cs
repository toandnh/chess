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
			['n'] = Piece.Knight,
			['b'] = Piece.Bishop,
			['r'] = Piece.Rook,
			['q'] = Piece.Queen,
			['k'] = Piece.King
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

		public static string GenerateFen(Board board) {
			string fen = "";

			// Board
			for (int rank = 7; rank >= 0; rank--) {
				int numEmptyFiles = 0;
				for (int file = 0; file < 8; file++) {
					int square = rank * 8 + file;
					int piece = board.Square[square];

					if (piece != Piece.None) {
						if (numEmptyFiles != 0) {
							fen += numEmptyFiles;
							numEmptyFiles = 0;
						}

						bool isBlack = Piece.IsColor(piece, Piece.Black);
						int pieceType = Piece.PieceType(piece);
						char pieceChar = ' ';

						switch (pieceType) {
							case Piece.Pawn:
								pieceChar = 'P';
								break;
							case Piece.Knight:
								pieceChar = 'N';
								break;
							case Piece.Bishop:
								pieceChar = 'B';
								break;
							case Piece.Rook:
								pieceChar = 'R';
								break;
							case Piece.Queen:
								pieceChar = 'Q';
								break;
							case Piece.King:
								pieceChar = 'K';
								break;
							default:
								break;
						}

						fen += isBlack ? pieceChar.ToString().ToLower() : pieceChar.ToString();
					} else {
						numEmptyFiles++;
					}
				}

				if (numEmptyFiles != 0) fen += numEmptyFiles;
				if (rank != 0) fen += '/';
 			}

			// Side to move
			fen += ' ';
			fen += board.WhiteToMove ? 'w' : 'b';

			// Castle rights
			fen += ' ';

			bool whiteKingSide = (board.CurrentGameState & 1) == 1;
			bool whiteQueenSide = (board.CurrentGameState >> 1 & 1) == 1;
			bool blackKingSide = (board.CurrentGameState >> 2 & 1) == 1;
			bool blackQueenSide = (board.CurrentGameState >> 3 & 1) == 1;

			fen += whiteKingSide ? 'K' : "";
			fen += whiteQueenSide ? 'Q' : "";
			fen += blackKingSide ? 'k' : "";
			fen += blackQueenSide ? 'q' : "";

			fen += (board.CurrentGameState & 0b1111) == 0 ? '-' : "";

			// En passant
			fen += ' ';

			int epFile = (int) board.CurrentGameState >> 4 & 0b1111;
			if (epFile == 0) {
				fen += '-';
			} else {
				string fileName = BoardRepresentation.FileNames[epFile - 1].ToString();
				int epRank = board.WhiteToMove ? 6 : 3;
				fen += fileName + epRank;	
			}

			// 50 move counter
			fen += ' ';
			fen += ' ';

			// Full move counter
			fen += ' ';
			fen += ' ';

			return fen;
		}
	}
}