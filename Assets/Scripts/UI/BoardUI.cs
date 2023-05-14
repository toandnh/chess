using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Game {
	public class BoardUI : MonoBehaviour {
		const float pieceDepth = -0.1f;
		
		public BoardTheme boardTheme;
		public PieceTheme pieceTheme;
		public bool isWhiteBottom = true;

		MeshRenderer[, ] squareRenderers;
		SpriteRenderer[, ] squarePieceRenderers;

		void Awake() {
			CreateBoard();
		}

		void CreateBoard() {
			Shader squareShader = Shader.Find("Unlit/Color");
			squareRenderers = new MeshRenderer[8, 8];
			squarePieceRenderers = new SpriteRenderer[8, 8];

			for (int file = 0; file < 8; file++) {
				for (int rank = 0; rank < 8; rank++) {
					// Create square
					Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
					square.parent = transform;
					square.name = BoardRepresentation.SquareNameFromCoord(file, rank);
					square.position = PositionFromCoord(file, rank, 0);
					Material squareMaterial = new Material(squareShader);

					squareRenderers[file, rank] = square.gameObject.GetComponent<MeshRenderer>();
					squareRenderers[file, rank].material = squareMaterial;

					// Create piece sprite renderer for current square
					SpriteRenderer pieceRenderer = new GameObject("Piece").AddComponent<SpriteRenderer>();
					pieceRenderer.transform.parent = square;
					pieceRenderer.transform.position = PositionFromCoord(file, rank, pieceDepth);
					pieceRenderer.transform.localScale = Vector3.one * 100 / (2000 / 6f);
					squarePieceRenderers[file, rank] = pieceRenderer;
				}
			}

			ResetSquareColor();
		}

		public void UpdatePosition(Board board) {
			for (int file = 0; file < 8; file++) {
				for (int rank = 0; rank < 8; rank++) {
					Coord coord = new Coord(file, rank);
					int piece = board.square[BoardRepresentation.IndexFromCoord(coord.fileIndex, coord.rankIndex)];
					squarePieceRenderers[file, rank].sprite = pieceTheme.GetPieceSprite(piece);
					squarePieceRenderers[file, rank].transform.position = PositionFromCoord(file, rank, pieceDepth);
				}
			}
		}

		public Vector3 PositionFromCoord(int file, int rank, float depth = 0) {
			if (isWhiteBottom) {
				return new Vector3(-3.5f + file, -3.5f + rank, depth);
			} else {
				return new Vector3(-3.5f + 7 - file, -3.5f + 7 - rank, depth); 
			}
		}

		public Vector3 PositionFromCoord(Coord coord, float depth = 0) {
			return PositionFromCoord(coord.fileIndex, coord.rankIndex, depth);
		}

		public void ResetSquareColor() {
			for (int file = 0; file < 8; file++) {
				for (int rank = 0; rank < 8; rank++) {
					SetSquareColor(new Coord(file, rank), boardTheme.lightSquares.normal, boardTheme.darkSquares.normal);
				}
			}
		}

		void SetSquareColor(Coord square, Color lighCol, Color darkCol) {
			squareRenderers[square.fileIndex, square.rankIndex].material.color = (square.IsLightSquare()) ? lighCol : darkCol;
		}
	}
}
