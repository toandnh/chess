using UnityEngine;

namespace Chess.Game {
	[CreateAssetMenu(menuName = "Theme/Pieces")]
	public class PieceTheme : ScriptableObject {
		[System.Serializable]
		public class PieceSprites {
			public Sprite pawn, knight, bishop, rook, queen, king;
			
			public Sprite this [int i] {
				get {
					return new Sprite[] { pawn, knight, bishop, rook, queen, king }[i];
				}
			}
		}

		public PieceSprites whitePieces;
		public PieceSprites blackPieces;

		public Sprite GetPieceSprite(int piece) {
			PieceSprites pieceSprites = Piece.IsColor(piece, Piece.White) ? whitePieces : blackPieces;
			
			switch(Piece.PieceType(piece)) {
				case Piece.Pawn:
					return pieceSprites.pawn;
				case Piece.Knight:
					return pieceSprites.knight;
				case Piece.Bishop:
					return pieceSprites.bishop;
					case Piece.Rook:
					return pieceSprites.rook;
				case Piece.Queen:
					return pieceSprites.queen;
				case Piece.King:
					return pieceSprites.king;
				default:
					return null;
			}
		}
	}
	
}