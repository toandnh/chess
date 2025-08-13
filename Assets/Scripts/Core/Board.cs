using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Chess {
	using static BoardRepresentation;

	public class Board {
		public int[] Square { get; set; }

		public int[][] Captures { get; set; }

		public PieceList PieceList;

		public const int WhiteIndex = 0;
		public const int BlackIndex = 1;

		// Bits 0-3 store the castle rights;
		// Bits 4-7 store the en passant file (1 - 8);
		// Bits 8-10 store the captured piece type.
		public uint CurrentGameState;
		Stack<uint> gameStateHistory;

		public ulong CurrentZobristKey;
		Stack<ulong> zobristKeyHistory;

		public bool WhiteToMove;

		public int ColorToMove;
		public int OpponentColor;

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
			zobristKeyHistory = new Stack<ulong>();
		}

		public void LoadStartPosition() {
			LoadPosition(FenUtility.StartFen);
		}

		public void LoadCustomPosition(string fen) {
			LoadPosition(fen);
		}

		void LoadPosition(string fen) {
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

			// Calculate initial zobrist key
			CurrentZobristKey = Zobrist.CalculateZobristKey(this);
			zobristKeyHistory.Push(CurrentZobristKey);
		}

		public void MakeMove(Move move) {
			int moveFrom = move.StartSquare;
			int moveTo = move.TargetSquare;

			int epPawnSquare = moveTo + (WhiteToMove ? -8 : 8);
			
			int targetPieceSquare = move.IsEnPassant ? epPawnSquare : moveTo;
			int targetPieceType = Piece.PieceType(Square[targetPieceSquare]);

			uint prevCastleRights = CurrentGameState & castleRightsMask; 
			uint currCastleRights = prevCastleRights; 

			int movePiece = Square[moveFrom];
			int movePieceType = Piece.PieceType(movePiece);

			int colorToMoveIndex = ColorToMove == Piece.White ? WhiteIndex : BlackIndex;
			int opponentColorIndex = ColorToMove == Piece.White ? BlackIndex : WhiteIndex;
			
			CurrentGameState = 0;

			// Capture move, outside the switch clause because of capture-into-promote moves
			if (move.IsCapture || move.IsEnPassant) {
				// Remove capture piece from piece list
				PieceList.Remove(targetPieceType, OpponentColor, targetPieceSquare);

				// Update captures list
				Captures[colorToMoveIndex][targetPieceType]++;

				CurrentGameState |= (uint) targetPieceType << 8;

				// Remove capture piece from zobrist key
				CurrentZobristKey = Zobrist.UpdatePieces(CurrentZobristKey, targetPieceSquare, targetPieceType, opponentColorIndex);
				
				// Fix enpassant square
				if (move.IsEnPassant) {
					Square[epPawnSquare] = Piece.None;
				}
			}

			// Handle special moves
			switch (move.OtherMoveFlags) {
				case Move.Flag.Castle:
					bool kingside = moveTo == g1 || moveTo == g8;

					int castlingRookFromIndex = kingside ? moveTo + 1 : moveTo - 2;
					int castlingRookToIndex = kingside ? moveTo - 1 : moveTo + 1;

					Square[castlingRookFromIndex] = Piece.None;
					Square[castlingRookToIndex] = Piece.Rook | ColorToMove;

					PieceList.Update(Piece.Rook, ColorToMove, castlingRookFromIndex, castlingRookToIndex);

					CurrentZobristKey = Zobrist.UpdatePieces(CurrentZobristKey, castlingRookFromIndex, Piece.Rook, colorToMoveIndex);
					CurrentZobristKey = Zobrist.UpdatePieces(CurrentZobristKey, castlingRookToIndex, Piece.Rook, colorToMoveIndex);

					break;
				case Move.Flag.PawnTwoForward:
					uint file = (uint) BoardRepresentation.FileIndex(moveFrom) + 1;
					CurrentGameState |= (file << 4);
					CurrentZobristKey = Zobrist.UpdateEnpassantFile(CurrentZobristKey, file);

					break;
				case Move.Flag.PromoteToKnight:
				case Move.Flag.PromoteToBishop:
				case Move.Flag.PromoteToRook:
				case Move.Flag.PromoteToQueen:
					int promotePieceType = move.PromotionPiece;

					PieceList.Remove(movePieceType, ColorToMove, moveFrom);
					CurrentZobristKey = Zobrist.UpdatePieces(CurrentZobristKey, moveFrom, movePieceType, colorToMoveIndex);

					PieceList.Add(promotePieceType, ColorToMove, moveFrom);
					CurrentZobristKey = Zobrist.UpdatePieces(CurrentZobristKey, moveFrom, promotePieceType, colorToMoveIndex);

					movePiece = (int) promotePieceType | ColorToMove;
					movePieceType = promotePieceType;

					break;
				default:
					break;
			}

			// Update the board representation
			Square[moveTo] = movePiece;
			Square[moveFrom] = 0;

			// Update move piece's position in piece lists
			PieceList.Update(movePieceType, ColorToMove, moveFrom, moveTo);

			// Update move piece's position in zobrist key
			CurrentZobristKey = Zobrist.UpdatePieces(CurrentZobristKey, moveFrom, movePieceType, colorToMoveIndex);
			CurrentZobristKey = Zobrist.UpdatePieces(CurrentZobristKey, moveTo, movePieceType, colorToMoveIndex);

			// Update castle rights
			// King move
			if (movePieceType == Piece.King) {
				if (WhiteToMove) {
					currCastleRights &= whiteCastleKingSideMask;
					currCastleRights &= whiteCastleQueenSideMask;
				} else {
					currCastleRights &= blackCastleKingSideMask;
					currCastleRights &= blackCastleQueenSideMask;
				}
			}
			// Move into or out of the corner squares
			if (currCastleRights != 0) {
				if (moveTo == h1 || moveFrom == h1) {
					currCastleRights &= whiteCastleKingSideMask;
				} else if (moveTo == a1 || moveFrom == a1) {
					currCastleRights &= whiteCastleQueenSideMask;
				} else if (moveTo == h8 || moveFrom == h8) {
					currCastleRights &= blackCastleKingSideMask;
				} else if (moveTo == a8 || moveFrom == a8) {
					currCastleRights &= blackCastleQueenSideMask;
				}
			}
			CurrentGameState |= currCastleRights;

			if (currCastleRights != prevCastleRights) {
				CurrentZobristKey = Zobrist.UpdateCastleRights(CurrentZobristKey, prevCastleRights, currCastleRights);
			}

			gameStateHistory.Push(CurrentGameState);

			if (!WhiteToMove) {
				CurrentZobristKey = Zobrist.UpdateSideToMove(CurrentZobristKey);
			}

			zobristKeyHistory.Push(CurrentZobristKey);

			UpdateSideToMove(!WhiteToMove);
		}

		public void UnmakeMove(Move move) {
			int friendlyColor = OpponentColor;
			int opponentColor = ColorToMove;

			int capturedPieceType = (int) (CurrentGameState & capturedPieceTypeMask) >> 8;
			int capturedPiece = capturedPieceType != 0 ? capturedPieceType | opponentColor : 0;

			int moveFrom = move.TargetSquare;
			int moveTo = move.StartSquare;

			int movePiece = Square[moveFrom];
			int movePieceType = Piece.PieceType(movePiece);

			if (move.IsCapture) {
				PieceList.Add((int) capturedPieceType, opponentColor, moveFrom);
				Captures[opponentColor == Piece.White ? BlackIndex : WhiteIndex][capturedPieceType]--;
			}
			
			if (move.IsEnPassant) {
				int epPawnSquare = moveFrom + (WhiteToMove ? 8 : -8);

				Square[epPawnSquare] = Piece.Pawn | opponentColor;

				// Add opponent's pawn back to the list
				PieceList.Add(Piece.Pawn, opponentColor, epPawnSquare);
				Captures[opponentColor == Piece.White ? BlackIndex : WhiteIndex][Piece.Pawn]--;

				// Mark as already handled
				capturedPiece = 0;
			}

			// Update PieceList for promotion case will be handled below
			if (!move.IsPromotion) {
				PieceList.Update(movePieceType, friendlyColor, moveFrom, moveTo);
			}

			// Special moves
			switch (move.OtherMoveFlags) {
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
					PieceList.Remove(movePieceType, friendlyColor, moveFrom);

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

			zobristKeyHistory.Pop();
			CurrentZobristKey = zobristKeyHistory.Peek();

			UpdateSideToMove(!WhiteToMove);
		}

		void UpdateSideToMove(bool whiteToMove) {
			WhiteToMove = whiteToMove;
			ColorToMove = WhiteToMove ? Piece.White : Piece.Black;
			OpponentColor = WhiteToMove ? Piece.Black : Piece.White;
		}
	}
}