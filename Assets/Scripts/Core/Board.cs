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
		// Bits 8-10 store the captured piece type.
		public uint CurrentGameState;

		public bool WhiteToMove;

		public int ColorToMove;
		public int OpponentColor;

		Stack<uint> gameStateHistory;

		// & masks
		const uint castleRightsMask = 0b00000001111;
		const uint capturedPieceTypeMask = 0b11100000000;

		// | masks
		const uint whiteCastleKingSideMask = 0b11111111110;
		const uint whiteCastleQueenSideMask = 0b11111111101;
		const uint blackCastleKingSideMask = 0b11111111011;
		const uint blackCastleQueenSideMask = 0b11111110111;

		void Initialize() {
			Square = new int[64];

			PieceList = new PieceList();

			Captures = new int[2][];
			Captures[WhiteIndex] = new int[6];
			Captures[BlackIndex] = new int[6];

			gameStateHistory = new Stack<uint>();
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
			gameStateHistory.Push(CurrentGameState);
		}

		public void MakeMove(Move move) {
			uint castleRights = CurrentGameState & castleRightsMask; 

			// Clear or unset the MSB - check bit
			int moveFlag = move.MoveFlag & ~(1 << 3);

			int promotePieceType = moveFlag >= 4 && moveFlag <= 7 ? moveFlag - 2 : 0; 

			CurrentGameState = 0;

			int moveFrom = move.StartSquare;
			int moveTo = move.TargetSquare;

			int movePiece = Square[moveFrom];
			int movePieceType = Piece.PieceType(movePiece);

			int epPawnSquare = moveTo + (WhiteToMove ? -8 : 8);

			// Capture move, outside the switch clause because of capture-into-promote moves
			int targetPieceSquare = moveFlag == Move.Flag.EnPassant ? epPawnSquare : moveTo;
			int targetPieceType = Piece.PieceType(Square[targetPieceSquare]);
			if (targetPieceType != Piece.None) {
				// Remove from piece list
				PieceList.Remove(targetPieceType, OpponentColor, targetPieceSquare);

				// Update captures list
				Captures[WhiteToMove ? WhiteIndex : BlackIndex][targetPieceType]++;

				CurrentGameState |= (uint) targetPieceType << 8;
			}

			// Update move piece's position in piece lists
			PieceList.Update(movePieceType, ColorToMove, moveFrom, moveTo);

			// Handle special move
			switch (moveFlag) {
				case Move.Flag.EnPassant:
					Square[epPawnSquare] = Piece.None;

					break;
				case Move.Flag.Castle:
					bool kingside = moveTo == g1 || moveTo == g8;

					int castlingRookFromIndex = kingside ? moveTo + 1 : moveTo - 2;
					int castlingRookToIndex = kingside ? moveTo - 1 : moveTo + 1;

					Square[castlingRookFromIndex] = Piece.None;
					Square[castlingRookToIndex] = Piece.Rook | ColorToMove;

					PieceList.Update(Piece.Rook, ColorToMove, castlingRookFromIndex, castlingRookToIndex);

					break;
				case Move.Flag.PromoteToKnight:
				case Move.Flag.PromoteToBishop:
				case Move.Flag.PromoteToRook:
				case Move.Flag.PromoteToQueen:
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

			gameStateHistory.Push(CurrentGameState);

			UpdateSideToMove(!WhiteToMove);
		}

		public void UnmakeMove(Move move) {
			int friendlyColor = OpponentColor;
			int opponentColor = ColorToMove;

			uint castleRights = CurrentGameState & castleRightsMask;

			int capturedPieceType = (int) (CurrentGameState & capturedPieceTypeMask) >> 8;
			int capturedPiece = capturedPieceType != 0 ? capturedPieceType | opponentColor : 0;

			int moveFrom = move.TargetSquare;
			int moveTo = move.StartSquare;

			int movePiece = Square[moveFrom];
			int movePieceType = Piece.PieceType(movePiece);

			// Clear or unset the MSB - check bit
			int moveFlag = move.MoveFlag & ~(1 << 3);

			bool isPromote = moveFlag >= 4;
			bool isEnPassant = moveFlag == Move.Flag.EnPassant;

			// Capture move; En passant will be handled below
			if (capturedPieceType != 0 && !isEnPassant) {
				PieceList.Add((int) capturedPieceType, opponentColor, moveFrom);
			}

			// Update PieceList for promotion case will be handled below
			if (!isPromote) {
				PieceList.Update(movePieceType, friendlyColor, moveFrom, moveTo);
			}

			// Special moves
			switch (moveFlag) {
				case Move.Flag.EnPassant:
					int epPawnSquare = moveFrom + (WhiteToMove ? 8 : -8);

					Square[epPawnSquare] = Piece.Pawn | opponentColor;

					// Add opponent's pawn back to the list
					PieceList.Add(Piece.Pawn, opponentColor, epPawnSquare);

					// Already handled
					capturedPiece = 0;

					break;
				case Move.Flag.Castle:
					bool kingside = moveFrom == g1 || moveFrom == g8;

					int castlingRookFromIndex = kingside ? moveFrom - 1 : moveFrom + 1;
					int castlingRookToIndex = kingside ? moveFrom + 1 : moveFrom - 2;

					Square[castlingRookFromIndex] = Piece.None;
					Square[castlingRookToIndex] = Piece.Rook | friendlyColor;

					PieceList.Update(Piece.Rook, friendlyColor, castlingRookFromIndex, castlingRookToIndex);

					break;
				case Move.Flag.PromoteToKnight:
				case Move.Flag.PromoteToBishop:
				case Move.Flag.PromoteToRook:
				case Move.Flag.PromoteToQueen:
					// Remove promoted piece
					PieceList.Remove(movePieceType, friendlyColor, moveTo);

					movePieceType = Piece.Pawn;
					movePiece = movePieceType | friendlyColor;

					// Add the pawn back
					PieceList.Add(movePieceType, friendlyColor, moveTo);

					break;
				default:
					break;
			}

			// Update the board representation
			Square[moveTo] = movePiece;
			Square[moveFrom] = capturedPiece;

			gameStateHistory.Pop();
			CurrentGameState = gameStateHistory.Peek();

			UpdateSideToMove(!WhiteToMove);
		}

		void UpdateSideToMove(bool whiteToMove) {
			WhiteToMove = whiteToMove;
			ColorToMove = WhiteToMove ? Piece.White : Piece.Black;
			OpponentColor = WhiteToMove ? Piece.Black : Piece.White;
		}
	}
}