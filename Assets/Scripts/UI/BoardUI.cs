using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Chess.Game {
	public class BoardUI : MonoBehaviour {
		public BoardTheme boardTheme;
		public PieceTheme pieceTheme;

		public Sprite[] textSpriteList;
		public Sprite[] numberSpriteList;

		public bool IsWhiteBottom = true;
		public bool PromoteMenuOnScreen = false;

		public int PromoteStartSquareIndex { get; set; }

		const float pieceDepth = -0.1f;
		const float pieceDragDepth = -0.2f;

		const float menuDepth = -0.3f;
		const float menuChoiceDepth = -0.4f;

		Move lastMove;

		MeshRenderer[, ] squareRenderers;
		SpriteRenderer[, ] squarePieceRenderers;

		MeshRenderer[] fileLabelRenderers;
		MeshRenderer[] rankLabelRenderers;
		SpriteRenderer[] squareTextRenderers;
		SpriteRenderer[] squareNumberRenderers;

		MeshRenderer[] squareMenuRenderers;
		SpriteRenderer[] menuPieceRenderers;


		void Awake() {
			CreateBoard();
		}

		void CreateBoard() {
			Shader squareShader = Shader.Find("Unlit/Color");

			squareRenderers = new MeshRenderer[8, 8];
			squarePieceRenderers = new SpriteRenderer[8, 8];

			fileLabelRenderers = new MeshRenderer[8];
			rankLabelRenderers = new MeshRenderer[8];
			squareTextRenderers = new SpriteRenderer[8];
			squareNumberRenderers = new SpriteRenderer[8];

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

				fileLabelRenderers[file] = square.gameObject.GetComponent<MeshRenderer>();
				fileLabelRenderers[file].material = squareMaterial;
				fileLabelRenderers[file].material.color = boardTheme.labelSquares;

				// Create text sprite renderer for current square
				SpriteRenderer textRenderer = new GameObject("File").AddComponent<SpriteRenderer>();
				textRenderer.sprite = textSpriteList[file];
				textRenderer.transform.parent = square;
				textRenderer.transform.position = PositionFromCoord(file, -1, pieceDepth);
				squareTextRenderers[file] = textRenderer;
			}

			for (int rank = 0; rank < 8; rank++) {
				// Create square
				Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
				square.parent = transform;
				square.name = "rank" + rank.ToString();
				square.position = PositionFromCoord(-1, rank, 0);
				Material squareMaterial = new Material(squareShader);

				rankLabelRenderers[rank] = square.gameObject.GetComponent<MeshRenderer>();
				rankLabelRenderers[rank].material = squareMaterial;
				rankLabelRenderers[rank].material.color = boardTheme.labelSquares;

				// Create number sprite renderer for current square
				SpriteRenderer numberRenderer = new GameObject("Rank").AddComponent<SpriteRenderer>();
				numberRenderer.sprite = numberSpriteList[rank];
				numberRenderer.transform.parent = square;
				numberRenderer.transform.position = PositionFromCoord(-1, rank, pieceDepth);
				squareNumberRenderers[rank] = numberRenderer;
			}

			ResetSquareColor();
		}

		public void CreatePromoteMenu(int startSquare, int colorIndex) {
			PromoteMenuOnScreen = true;

			Shader squareShader = Shader.Find("Unlit/Color");

			squareMenuRenderers = new MeshRenderer[4];
			menuPieceRenderers = new SpriteRenderer[4];

			int mask = IsWhiteBottom ? 0b01000 : 0b10000;

			int file = BoardRepresentation.FileIndex(startSquare);
			int rank = BoardRepresentation.RankIndex(startSquare);

			for (int index = 0; index < 4; index++) {
				// Map index to piece representation
				int pieceType = 5 - index;

				// Create square
				Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
				square.parent = transform;
				square.name = "menu" + index.ToString();
				square.position = PositionFromCoord(file, rank, menuDepth);
				Material squareMaterial = new Material(squareShader);

				squareMenuRenderers[index] = square.gameObject.GetComponent<MeshRenderer>();
				squareMenuRenderers[index].material = squareMaterial;
				squareMenuRenderers[index].material.color = boardTheme.menuSquares;

				// Create options sprite
				SpriteRenderer pieceChoiceRenderer = new GameObject("Menu").AddComponent<SpriteRenderer>();
				pieceChoiceRenderer.sprite = pieceTheme.GetPieceSprite(mask | pieceType);
				pieceChoiceRenderer.transform.parent = square;
				pieceChoiceRenderer.transform.position = PositionFromCoord(file, rank, menuChoiceDepth);
				menuPieceRenderers[index] = pieceChoiceRenderer;

				rank--;
			}
		}

		public void DestroyPromoteMenu() {
			for (int index = 0; index < squareMenuRenderers.Length; index++) {
				Destroy(squareMenuRenderers[index]);
				Destroy(menuPieceRenderers[index]);
			}
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

		public void SetWhitePerspective(bool whitePov) {
			IsWhiteBottom = whitePov;
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

		public bool CanGetSquareUnderMouse(Vector2 mouseWorld, out Coord selectedCoord) {
			int file = (int) (mouseWorld.x + 7);
			int rank = (int) (mouseWorld.y + 4);

			if (!IsWhiteBottom) {
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
			if (IsWhiteBottom) {
				return new Vector3(-6.5f + file, -3.5f + rank, depth);
			} else {
				return new Vector3(-6.5f + 7 - file, -3.5f + 7 - rank, depth); 
			}
		}

		public Vector3 PositionFromCoord(Coord coord, float depth = 0) {
			return PositionFromCoord(coord.fileIndex, coord.rankIndex, depth);
		}
	}
}
