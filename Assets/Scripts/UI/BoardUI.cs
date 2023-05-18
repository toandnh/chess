using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Game {
	public class BoardUI : MonoBehaviour {
		public BoardTheme boardTheme;
		public PieceTheme pieceTheme;

		const float pieceDepth = -0.1f;
		const float pieceDragDepth = -0.2f;

		public bool isWhiteBottom = true;

		public Sprite[] textSpriteList;
		public Sprite[] numberSpriteList;

		Move lastMove;

		MeshRenderer[, ] squareRenderers;
		MeshRenderer[, ] fileLabelRenderers;
		MeshRenderer[, ] rankLabelRenderers;
		SpriteRenderer[, ] squarePieceRenderers;
		SpriteRenderer[, ] squareTextRenderers;
		SpriteRenderer[, ] squareNumberRenderers;


		void Awake() {
			CreateBoard();
		}

		void CreateBoard() {
			Shader squareShader = Shader.Find("Unlit/Color");
			squareRenderers = new MeshRenderer[8, 8];
			fileLabelRenderers = new MeshRenderer[8, 1];
			rankLabelRenderers = new MeshRenderer[1, 8];
			squarePieceRenderers = new SpriteRenderer[8, 8];
			squareTextRenderers = new SpriteRenderer[8, 1];
			squareNumberRenderers = new SpriteRenderer[1, 8];

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
					//pieceRenderer.transform.localScale = Vector3.one * 100 / (2000 / 6f);
					squarePieceRenderers[file, rank] = pieceRenderer;
				}
			}

			for (int file = 0; file < 8; file++) {
				// Create square
				Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
				square.parent = transform;
				square.name = "file" + file.ToString();
				square.position = PositionFromCoord(file, -1, 0);
				Material squareMaterial = new Material(squareShader);

				fileLabelRenderers[file, 0] = square.gameObject.GetComponent<MeshRenderer>();
				fileLabelRenderers[file, 0].material = squareMaterial;
				fileLabelRenderers[file, 0].material.color = boardTheme.labelSquares;

				// Create text sprite renderer for current square
				SpriteRenderer textRenderer = new GameObject("File").AddComponent<SpriteRenderer>();
				textRenderer.sprite = textSpriteList[file];
				textRenderer.transform.parent = square;
				textRenderer.transform.position = PositionFromCoord(file, -1, pieceDepth);
				squareTextRenderers[file, 0] = textRenderer;
			}

			for (int rank = 0; rank < 8; rank++) {
				// Create square
				Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
				square.parent = transform;
				square.name = "rank" + rank.ToString();
				square.position = PositionFromCoord(-1, rank, 0);
				Material squareMaterial = new Material(squareShader);

				rankLabelRenderers[0, rank] = square.gameObject.GetComponent<MeshRenderer>();
				rankLabelRenderers[0, rank].material = squareMaterial;
				rankLabelRenderers[0, rank].material.color = boardTheme.labelSquares;

				// Create number sprite renderer for current square
				SpriteRenderer numberRenderer = new GameObject("Rank").AddComponent<SpriteRenderer>();
				numberRenderer.sprite = numberSpriteList[rank];
				numberRenderer.transform.parent = square;
				numberRenderer.transform.position = PositionFromCoord(-1, rank, pieceDepth);
				squareNumberRenderers[0, rank] = numberRenderer;
			}

			ResetSquareColor();
		}

		public void OnMoveMade(Board board, Move move) {
			lastMove = move;

			UpdatePosition(board);
			ResetSquareColor();
		}

		void HighlightMove(Move move) {
			SetSquareColor(BoardRepresentation.CoordFromIndex(move.StartSquare), boardTheme.lightSquares.highlighted, boardTheme.darkSquares.highlighted);
			SetSquareColor(BoardRepresentation.CoordFromIndex(move.TargetSquare), boardTheme.lightSquares.highlighted, boardTheme.darkSquares.highlighted);
		}

		public void SetPerspective(bool whitePov) {
			isWhiteBottom = whitePov;
			ResetSquarePosition();
		}

		public void UpdatePosition(Board board) {
			for (int file = 0; file < 8; file++) {
				for (int rank = 0; rank < 8; rank++) {
					Coord coord = new Coord(file, rank);
					int piece = board.Square[BoardRepresentation.IndexFromCoord(coord.fileIndex, coord.rankIndex)];
					squarePieceRenderers[file, rank].sprite = pieceTheme.GetPieceSprite(piece);
					squarePieceRenderers[file, rank].transform.position = PositionFromCoord(file, rank, pieceDepth);
				}
			}
		}

		public void DragPiece(Coord pieceCoord, Vector2 mousePos) {
			squarePieceRenderers[pieceCoord.fileIndex, pieceCoord.rankIndex].transform.position = new Vector3(mousePos.x, mousePos.y, pieceDragDepth);
		}

		public bool TryGetSquareUnderMouse(Vector2 mouseWorld, out Coord selectedCoord) {
			int file = (int) (mouseWorld.x + 4);
			int rank = (int) (mouseWorld.y + 4);

			if (!isWhiteBottom) {
				file = 7 - file;
				rank = 7 - rank;
			}

			selectedCoord = new Coord(file, rank);

			return file >= 0 && file < 8 && rank >= 0 && rank < 8;
		}

		public void SelectSquare(Coord coord) {
			SetSquareColor(coord, boardTheme.lightSquares.selected, boardTheme.darkSquares.selected);
		}

		public void DeselectSquare(Coord coord) {
			ResetSquareColor();
		}

		public void ResetSquareColor(bool highlight = true) {
			for (int file = 0; file < 8; file++) {
				for (int rank = 0; rank < 8; rank++) {
					SetSquareColor(new Coord(file, rank), boardTheme.lightSquares.normal, boardTheme.darkSquares.normal);
				}
			}

			if (highlight && !lastMove.IsInvalid) {
				HighlightMove(lastMove);
			}
		}

		void SetSquareColor(Coord square, Color lightCol, Color darkCol) {
			squareRenderers[square.fileIndex, square.rankIndex].material.color = (square.IsLightSquare()) ? lightCol : darkCol;
		}

		public void ResetPiecePosition(Coord coord) {
			Vector3 position = PositionFromCoord(coord.fileIndex, coord.rankIndex, pieceDepth);
			squarePieceRenderers[coord.fileIndex, coord.rankIndex].transform.position = position;
		}

		public void ResetSquarePosition() {
			for (int file = 0; file < 8; file++) {
				for (int rank = 0; rank < 8; rank++) {
					squareRenderers[file, rank].transform.position = PositionFromCoord(file, rank, 0);
					squarePieceRenderers[file, rank].transform.position = PositionFromCoord(file, rank, pieceDepth);
				}
			}

			if (!lastMove.IsInvalid) {
				HighlightMove(lastMove);
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
	}
}
