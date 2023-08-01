using System.Collections.Generic;
using System.Linq;

namespace Chess {
	using static BoardRepresentation;

	public class Board {
		public int[] Square;

		public PieceList PieceList;

		public List<List<string>> Text;

		public int[][] Captures;

		public const int WhiteIndex = 0;
		public const int BlackIndex = 1;

		// Bits 0-3 store the castle rights;
		// Bits 4-7 store the en passant square (starting at 1)
		public uint CurrentGameState;

		public int PromotePiece { get; set; }

		public bool WhiteToMove;

		public int ColorToMove;
		public int OpponentColor;

		const uint castleRightsMask = 0b00001111;

		const uint whiteCastleKingSideMask = 0b11111110;
		const uint whiteCastleQueenSideMask = 0b11111101;
		const uint blackCastleKingSideMask = 0b11111011;
		const uint blackCastleQueenSideMask = 0b11110111;

		void Initialize() {
			Square = new int[64];

			PieceList = new PieceList();

			Text = new List<List<string>>();
			Text.Add(new List<string>());
			Text.Add(new List<string>());

			Captures = new int[2][];
			Captures[WhiteIndex] = new int[6];
			Captures[BlackIndex] = new int[6];

			PromotePiece = -1;
		}

		public void LoadStartPosition() {
			LoadPosition(FenUtility.StartFen);
		}

		public void LoadCustomPosition(string fen) {
			LoadPosition(fen);
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
			uint castleRights = CurrentGameState & castleRightsMask; 

			CurrentGameState = 0;

			int moveFrom = move.StartSquare;
			int moveTo = move.TargetSquare;

			int movePiece = Square[moveFrom];
			int movePieceType = Piece.PieceType(movePiece);

			// Pawns (pieceType == 1) have no symbols
			string moveText = movePieceType == 1 ? "" : MoveText.GetPieceText(movePiece).ToString();

			// Clear or unset the MSB - check bit
			int moveFlag = move.MoveFlag & ~(1 << 3);

			// Capture move, outside the switch clause because of capture-into-promote moves
			int targetPieceType = Piece.PieceType(Square[moveTo]);
			if (targetPieceType != Piece.None) {
				// Remove from piece list
				PieceList.Remove(targetPieceType, OpponentColor, moveTo);

				// Build notations
				if (movePieceType == 1) {
					// Pawn capture pawn
					moveText += FileNames[FileIndex(moveFrom)].ToString();
				}
				moveText = moveText + 'x';

				// Update captures list
				Captures[WhiteToMove ? WhiteIndex : BlackIndex][targetPieceType]++;
			}

			// Record the move
			int colorToMoveIndex = WhiteToMove ? WhiteIndex : BlackIndex;
			moveText += MoveText.GetSquareText(moveTo);

			// Update move piece's position in piece lists
			PieceList.Update(movePieceType, ColorToMove, moveFrom, moveTo);

			// Handle special move
			switch (moveFlag) {
				case Move.Flag.EnPassant:
					int epPawnSquare = moveTo + (WhiteToMove ? -8 : 8);

					CurrentGameState |= (ushort) (Square[epPawnSquare] << 8);
					Square[epPawnSquare] = Piece.None;

					// Remove opponent's pawn from the list
					PieceList.Remove(movePieceType, OpponentColor, epPawnSquare);

					break;
				case Move.Flag.Capture:
					break;
				case Move.Flag.Castle:
					bool kingside = moveTo == g1 || moveTo == g8;

					moveText = kingside ? "O-O" : "O-O-O";

					int castlingRookFromIndex = kingside ? moveTo + 1 : moveTo - 2;
					int castlingRookToIndex = kingside ? moveTo - 1 : moveTo + 1;

					Square[castlingRookFromIndex] = Piece.None;
					Square[castlingRookToIndex] = Piece.Rook | ColorToMove;

					PieceList.Update(Piece.Rook, ColorToMove, castlingRookFromIndex, castlingRookToIndex);

					break;
				case Move.Flag.Check:
					break;
				case Move.Flag.Promote:
					PieceList.Remove(movePieceType, ColorToMove, moveTo);

					movePiece = ColorToMove | PromotePiece;

					PieceList.Add(PromotePiece, ColorToMove, moveTo);

					PromotePiece = -1;

					break;
				default:
					break;
			}

			// Update the board representation
			Square[moveTo] = movePiece;
			Square[moveFrom] = 0;

			// Add to moveText list
			Text[colorToMoveIndex].Add(moveText);

			// Update castle rights
			// King move
			if (movePieceType == Piece.King) {
				if (WhiteToMove) {
					castleRights &= whiteCastleKingSideMask;
					castleRights &= whiteCastleQueenSideMask;
				} else {
					castleRights &= blackCastleKingSideMask;
					castleRights &= blackCastleQueenSideMask;
				}
			}
			// Move into or out of the corner squares
			if (castleRights != 0) {
				if (moveTo == h1 || moveFrom == h1) {
					castleRights &= whiteCastleKingSideMask;
				} else if (moveTo == a1 || moveFrom == a1) {
					castleRights &= whiteCastleQueenSideMask;
				} else if (moveTo == h8 || moveFrom == h8) {
					castleRights &= blackCastleKingSideMask;
				} else if (moveTo == a8 || moveFrom == a8) {
					castleRights &= blackCastleQueenSideMask;
				}
			}
			CurrentGameState |= castleRights;

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