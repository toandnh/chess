using System.Collections.Generic;
using System.Linq;

namespace Chess {
	using static BoardRepresentation;

	public class Board {
		public int[] Square;

		public PieceList PieceList;

		public int[][] Captures;

		public const int WhiteIndex = 0;
		public const int BlackIndex = 1;

		// Bits 0-3 store the castle rights;
		// Bits 4-7 store the en passant file (starting at 1);
		// Bits 8-10 store the promote piece type;
		// TODO: Bits 11-13 store the captured piece type.
		public uint CurrentGameState;

		public bool WhiteToMove;

		public int ColorToMove;
		public int OpponentColor;

		// & masks
		const uint castleRightsMask = 0b00000000001111;
		const uint capturedPieceTypeMask = 0b11100000000000;
		public const uint PromotePieceTypeMask = 0b00011100000000;

		// | masks
		const uint whiteCastleKingSideMask = 0b11111111111110;
		const uint whiteCastleQueenSideMask = 0b11111111111101;
		const uint blackCastleKingSideMask = 0b11111111111011;
		const uint blackCastleQueenSideMask = 0b11111111110111;

		void Initialize() {
			Square = new int[64];

			PieceList = new PieceList();

			Captures = new int[2][];
			Captures[WhiteIndex] = new int[6];
			Captures[BlackIndex] = new int[6];
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

			uint promotePieceType = (CurrentGameState & PromotePieceTypeMask) >> 8; 

			CurrentGameState = 0;

			int moveFrom = move.StartSquare;
			int moveTo = move.TargetSquare;

			int movePiece = Square[moveFrom];
			int movePieceType = Piece.PieceType(movePiece);

			// Clear or unset the MSB - check bit
			int moveFlag = move.MoveFlag & ~(1 << 3);

			// Capture move, outside the switch clause because of capture-into-promote moves
			int targetPieceType = Piece.PieceType(Square[moveTo]);
			if (targetPieceType != Piece.None) {
				// Remove from piece list
				PieceList.Remove(targetPieceType, OpponentColor, moveTo);

				// Update captures list
				Captures[WhiteToMove ? WhiteIndex : BlackIndex][targetPieceType]++;

				CurrentGameState |= (uint) targetPieceType << 11;
			}

			// Update move piece's position in piece lists
			PieceList.Update(movePieceType, ColorToMove, moveFrom, moveTo);

			// Handle special move
			switch (moveFlag) {
				case Move.Flag.EnPassant:
					int epPawnSquare = moveTo + (WhiteToMove ? -8 : 8);

					Square[epPawnSquare] = Piece.None;

					// Remove opponent's pawn from the list
					PieceList.Remove(movePieceType, OpponentColor, epPawnSquare);

					break;
				case Move.Flag.Capture:
					break;
				case Move.Flag.Castle:
					bool kingside = moveTo == g1 || moveTo == g8;

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

					movePiece = (int) promotePieceType | ColorToMove;

					PieceList.Add((int) promotePieceType, ColorToMove, moveTo);

					break;
				default:
					break;
			}

			// Update the board representation
			Square[moveTo] = movePiece;
			Square[moveFrom] = 0;

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
				CurrentGameState |= (uint) (file << 4);
			}

			// Reset the promote piece type
			CurrentGameState &= ~PromotePieceTypeMask;

			UpdateSideToMove(!WhiteToMove);
		}

		public void UnmakeMove(Move move) {
			int friendlyColor = OpponentColor;
			int opponentColor = ColorToMove;

			uint castleRights = CurrentGameState & castleRightsMask;

			uint capturedPieceType = (CurrentGameState & capturedPieceTypeMask) >> 11;
			int capturedPiece = capturedPieceType != 0 ? (int) capturedPieceType | opponentColor : 0;

			CurrentGameState = 0;

			int moveFrom = move.TargetSquare;
			int moveTo = move.StartSquare;

			int movePiece = Square[moveFrom];
			int movePieceType = Piece.PieceType(movePiece);

			// Clear or unset the MSB - check bit
			int moveFlag = move.MoveFlag & ~(1 << 3);

			// Capture move
			if (capturedPieceType != 0) {
				PieceList.Add((int) capturedPieceType, opponentColor, moveFrom);
			}

			PieceList.Update(movePieceType, friendlyColor, moveFrom, moveTo);

			// Special moves
			switch (moveFlag) {
				case Move.Flag.EnPassant:
					int epPawnSquare = moveFrom + (WhiteToMove ? -8 : 8);

					Square[epPawnSquare] = Piece.Pawn | opponentColor;

					// Add opponent's pawn back to the list
					PieceList.Add(movePieceType, opponentColor, epPawnSquare);

					break;
				case Move.Flag.Castle:
					bool kingside = moveFrom == g1 || moveFrom == g8;

					int castlingRookFromIndex = kingside ? moveFrom - 1 : moveFrom + 1;
					int castlingRookToIndex = kingside ? moveFrom + 1 : moveFrom - 2;

					Square[castlingRookFromIndex] = Piece.None;
					Square[castlingRookToIndex] = Piece.Rook | friendlyColor;

					PieceList.Update(Piece.Rook, friendlyColor, castlingRookFromIndex, castlingRookToIndex);

					break;
				case Move.Flag.Promote:
					// Remove promoted piece
					PieceList.Remove(movePiece, friendlyColor, moveTo);

					movePiece = Piece.Pawn | friendlyColor;

					// Add the pawn back
					PieceList.Add(movePiece, friendlyColor, moveTo);

					break;
			}

			// Update the board representation
			Square[moveTo] = movePiece;
			Square[moveFrom] = capturedPiece;

			// Update castle rights
			if (moveFlag == Move.Flag.Castle) {
				bool kingside = moveFrom == g1 || moveFrom == g8;
				if (friendlyColor == Piece.White) {
					castleRights &= kingside ? ~whiteCastleKingSideMask : ~whiteCastleQueenSideMask;
				} else {
					castleRights &= kingside ? ~blackCastleKingSideMask : ~blackCastleQueenSideMask;
				}
			}
			CurrentGameState |= castleRights;

			// Mark en-passant file
			if (moveFlag == Move.Flag.EnPassant) {
				int file = BoardRepresentation.FileIndex(moveFrom) + 1;
				CurrentGameState |= (uint) (file << 4);
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