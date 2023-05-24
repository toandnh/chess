using System.Collections.Generic;

namespace Chess {
	using static BoardRepresentation;

	public class Board {
		public int[] Square;

		public PieceList PieceList;

		public const int WhiteIndex = 0;
		public const int BlackIndex = 1;

		//
		public uint CurrentGameState;

		public bool WhiteToMove;
		public int ColorToMove;
		public int OpponentColor;

		void Initialize() {
			Square = new int[64];

			PieceList = new PieceList();
		}

		public void LoadStartPosition() {
			LoadPosition(FenUtility.StartFen);
		}

		public void LoadPosition(string fen) {
			Initialize();
			
			var loadedPosition = FenUtility.PositionFromFen(fen);

			// Populate Square array and PieceList
			for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
				int piece = loadedPosition.Squares[squareIndex];
				Square[squareIndex] = piece;

				if (piece != Piece.None) {
					int pieceType = Piece.PieceType(piece);
					int pieceColor = Piece.IsColor(piece, Piece.White) ? Piece.White : Piece.Black;

					PieceList.Add(pieceType, pieceColor, squareIndex);
				}
			}

			// Update side to move
			UpdateSideToMove(loadedPosition.WhiteToMove);

			// Create game state
			int whiteCastle = (loadedPosition.WhiteCastleKingSide ? 1 << 0 : 0) | (loadedPosition.WhiteCastleQueenSide ? 1 << 1 : 0);
			int blackCastle = (loadedPosition.BlackCastleKingSide ? 1 << 2 : 0) | (loadedPosition.BlackCastleQueenSide ? 1 << 3 : 0);
			int epState = loadedPosition.EpFile << 4;
			ushort initialGameState = (ushort) (whiteCastle | blackCastle | epState);
			CurrentGameState = initialGameState;
		}

		public void MakeMove(Move move) {
			CurrentGameState = 0;
			
			int moveFrom = move.StartSquare;
			int moveTo = move.TargetSquare;

			int movePiece = Square[moveFrom];
			int movePieceType = Piece.PieceType(movePiece);

			int moveFlag = move.MoveFlag;

			// Update position in piece lists
			PieceList.Update(movePieceType, ColorToMove, moveFrom, moveTo);

			// Handle special move
			switch (moveFlag) {
				case Move.Flag.Castling:
					bool kingside = moveTo == g1 || moveTo == g8;

					int castlingRookFromIndex = kingside ? moveTo + 1 : moveTo - 2;
					int castlingRookToIndex = kingside ? moveTo - 1 : moveTo + 1;

					Square[castlingRookFromIndex] = Piece.None;
					Square[castlingRookToIndex] = Piece.Rook | ColorToMove;

					PieceList.Update(Piece.Rook, ColorToMove, castlingRookFromIndex, castlingRookToIndex);

					break;
				case Move.Flag.EnPassant:
					int epSquare = moveTo + (WhiteToMove ? -8 : 8);

					CurrentGameState |= (ushort) (Square[epSquare] << 8);
					Square[epSquare] = Piece.None;

					PieceList.Remove(movePieceType, OpponentColor, epSquare);

					break;
				default:
					break;
			}

			// Update the board representation
			Square[moveTo] = movePiece;
			Square[moveFrom] = 0;

			// Mark en-passant file
			if (moveFlag == Move.Flag.PawnTwoForward) {
				int file = BoardRepresentation.FileIndex(moveFrom) + 1;
				CurrentGameState |= (ushort) (file << 4);
			}

			UpdateSideToMove(!WhiteToMove);
		}

		void UpdateSideToMove(bool whiteToMove) {
			WhiteToMove = whiteToMove;
			ColorToMove = WhiteToMove ? Piece.White : Piece.Black;
			OpponentColor = WhiteToMove ? Piece.Black : Piece.White;
		}
	}
}