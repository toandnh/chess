using System;
using System.Collections;

using UnityEngine;

namespace Chess {
	// https://www.chessprogramming.org/Zobrist_Hashing
	public static class Zobrist {
		const ulong SEED = 7997;

		// [squareIndex, pieceType (1 - 6), pieceColor (0, 1)]
		static readonly ulong[, , ] piecesArray = new ulong[64, 7, 2];
		static readonly ulong[] castleRights = new ulong[16];
		// From 1 to 8
		static readonly ulong[] enpassantFile = new ulong[9];
		static readonly ulong sideToMove;

		static Zobrist() {
			RKiss rand = new RKiss(SEED);

			for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
				for (int pieceIndex = 1; pieceIndex <= 6; pieceIndex++) {
					piecesArray[squareIndex, pieceIndex, Board.WhiteIndex] = rand.RandValue();
					piecesArray[squareIndex, pieceIndex, Board.BlackIndex] = rand.RandValue();
				}
			}

			for (int i = 0; i < castleRights.Length; i++) {
				castleRights[i] = rand.RandValue();
			}

			for (int i = 0; i < enpassantFile.Length; i++) {
				enpassantFile[i] = rand.RandValue();
			}

			sideToMove = rand.RandValue();
		}

		public static ulong CalculateZobristKey(Board board) {
			ulong zobristKey = 0;

			for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
				int currPiece = board.Square[squareIndex];
				if (currPiece != 0) {
					int pieceType = Piece.PieceType(currPiece);
					int pieceColor = Piece.PieceColor(currPiece) == Piece.White ? Board.WhiteIndex : Board.BlackIndex;

					zobristKey ^= piecesArray[squareIndex, pieceType, pieceColor];
				}
			}

			zobristKey ^= castleRights[board.CurrentGameState & 0b1111];

			zobristKey ^= enpassantFile[board.CurrentGameState & (0b1111 << 4)];

			if (!board.WhiteToMove) {
				zobristKey ^= sideToMove;
			}

			return zobristKey;
		}

		public static ulong UpdatePieces(ulong zobristKey, int squareIndex,  int pieceType, int colorIndex) {
			return zobristKey ^ piecesArray[squareIndex, pieceType, colorIndex];
		}

		public static ulong UpdateCastleRights(ulong zobristKey, uint prevCastleRights, uint currCastleRights) {
			return zobristKey ^ castleRights[prevCastleRights] ^ castleRights[currCastleRights];
		}

		public static ulong UpdateEnpassantFile(ulong zobristKey, uint file) {
			return zobristKey ^ enpassantFile[file];
		}

		public static ulong UpdateSideToMove(ulong zobristKey) {
			return zobristKey ^ sideToMove;
		}
	}
}