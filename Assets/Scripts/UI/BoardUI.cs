using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Chess.Game {
	public class BoardUI : MonoBehaviour {
		public BoardTheme boardTheme;
		public PieceTheme pieceTheme;

		public Sprite[] textSpriteList;
		public Sprite[] numberSpriteList;

		public bool IsWhiteBottom { get; set; } = true;
		public int PromoteStartSquareIndex { get; set; }

		bool showLegalMoves = false;

		const float squareDepth = 0f;
		const float highlightSquareDepth = -0.1f;
		const float pieceDepth = -0.2f;
		const float pieceDragDepth = -0.3f;

		const float menuDepth = -0.4f;
		const float menuChoiceDepth = -0.5f;

		Move lastMove;
		MoveGenerator moveGenerator;

		MeshRenderer[, ] squareRenderers;
		SpriteRenderer[, ] squarePieceRenderers;

		MeshRenderer[, ] highlightSquareRenderers;

		MeshRenderer[] fileLabelRenderers;
		MeshRenderer[] rankLabelRenderers;
		SpriteRenderer[] squareTextRenderers;
		SpriteRenderer[] squareNumberRenderers;

		MeshRenderer[] squareMenuRenderers;
		SpriteRenderer[] menuPieceRenderers;

		void Awake() {
			moveGenerator = new MoveGenerator();
			CreateBoardUI();
		}

		void CreateBoardUI() {
			Shader squareShader = Shader.Find("Unlit/Color");

			squareRenderers = new MeshRenderer[8, 8];
			squarePieceRenderers = new SpriteRenderer[8, 8];

			highlightSquareRenderers = new MeshRenderer[8, 8];

			fileLabelRenderers = new MeshRenderer[8];
			rankLabelRenderers = new MeshRenderer[8];
			squareTextRenderers = new SpriteRenderer[8];
			squareNumberRenderers = new SpriteRenderer[8];

			for (int file = 0; file < 8; file++) {
				for (int rank = 0; rank < 8; rank++) {
					Material squareMaterial = new Material(squareShader);

					Coord squareCoord = new Coord(file, rank);

					// Create square
					Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
					square.parent = transform;
					square.name = BoardRepresentation.SquareNameFromCoord(file, rank);
					square.position = PositionFromCoord(file, rank, squareDepth);

					squareRenderers[file, rank] = square.gameObject.GetComponent<MeshRenderer>();
					squareRenderers[file, rank].material = squareMaterial;

					squareRenderers[file, rank].material.color = (squareCoord.IsLightSquare()) ? boardTheme.LightSquares.Normal : boardTheme.DarkSquares.Normal;

					// Create highlight square - for highlighting previous, legal moves
					Transform highlightSquare = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
					highlightSquare.parent = transform;
					highlightSquare.name = BoardRepresentation.SquareNameFromCoord(file, rank);
					highlightSquare.position = PositionFromCoord(file, rank, highlightSquareDepth);
					highlightSquare.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

					highlightSquareRenderers[file, rank] = highlightSquare.gameObject.GetComponent<MeshRenderer>();
					highlightSquareRenderers[file, rank].material = squareMaterial;

					highlightSquareRenderers[file, rank].material.color = (squareCoord.IsLightSquare()) ? boardTheme.LightSquares.Normal : boardTheme.DarkSquares.Normal;

					// Create piece sprite renderer for current square
					SpriteRenderer pieceRenderer = new GameObject("Piece").AddComponent<SpriteRenderer>();
					pieceRenderer.transform.parent = square;
					pieceRenderer.transform.position = PositionFromCoord(file, rank, pieceDepth);
					squarePieceRenderers[file, rank] = pieceRenderer;
				}
			}

			for (int file = 0; file < 8; file++) {
				// Create square
				Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
				square.parent = transform;
				square.name = "file" + file.ToString();
				square.position = new Vector3(-6.5f + file, -4.5f, squareDepth);
				Material squareMaterial = new Material(squareShader);

				fileLabelRenderers[file] = square.gameObject.GetComponent<MeshRenderer>();
				fileLabelRenderers[file].material = squareMaterial;
				fileLabelRenderers[file].material.color = boardTheme.LabelSquares;

				// Create text sprite renderer for current square
				int pos = IsWhiteBottom ? file : 7 - file;
				SpriteRenderer textRenderer = new GameObject("File").AddComponent<SpriteRenderer>();
				textRenderer.sprite = textSpriteList[pos];
				textRenderer.transform.parent = square;
				textRenderer.transform.position = new Vector3(-6.5f + file, -4.5f, pieceDepth);
				squareTextRenderers[file] = textRenderer;
			}

			for (int rank = 0; rank < 8; rank++) {
				// Create square
				Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
				square.parent = transform;
				square.name = "rank" + rank.ToString();
				square.position = new Vector3(-7.5f, -3.5f + rank, squareDepth);
				Material squareMaterial = new Material(squareShader);

				rankLabelRenderers[rank] = square.gameObject.GetComponent<MeshRenderer>();
				rankLabelRenderers[rank].material = squareMaterial;
				rankLabelRenderers[rank].material.color = boardTheme.LabelSquares;

				// Create number sprite renderer for current square
				int pos = IsWhiteBottom ? rank : 7 - rank;
				SpriteRenderer numberRenderer = new GameObject("Rank").AddComponent<SpriteRenderer>();
				numberRenderer.sprite = numberSpriteList[pos];
				numberRenderer.transform.parent = square;
				numberRenderer.transform.position = new Vector3(-7.5f, -3.5f + rank, pieceDepth);
				squareNumberRenderers[rank] = numberRenderer;
			}
		}

		public void CreatePromoteMenu(int startSquare, int colorToMove) {
			Shader squareShader = Shader.Find("Unlit/Color");

			squareMenuRenderers = new MeshRenderer[4];
			menuPieceRenderers = new SpriteRenderer[4];

			bool isWhitePromote = colorToMove == Piece.White;

			int mask = isWhitePromote ? Piece.White : Piece.Black;

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
				square.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
				Material squareMaterial = new Material(squareShader);

				squareMenuRenderers[index] = square.gameObject.GetComponent<MeshRenderer>();
				squareMenuRenderers[index].material = squareMaterial;
				squareMenuRenderers[index].material.color = boardTheme.MenuSquares;

				// Create options sprite
				SpriteRenderer pieceChoiceRenderer = new GameObject("Menu").AddComponent<SpriteRenderer>();
				pieceChoiceRenderer.sprite = pieceTheme.GetPieceSprite(mask | pieceType);
				pieceChoiceRenderer.transform.parent = square;
				pieceChoiceRenderer.transform.position = PositionFromCoord(file, rank, menuChoiceDepth);
				menuPieceRenderers[index] = pieceChoiceRenderer;

				// If the promotion is at the top of the board, render the options downwards and vice versa
				rank = (IsWhiteBottom && isWhitePromote) || (!IsWhiteBottom && isWhitePromote) ? rank - 1 : rank + 1;
			}
		}

		public void DestroyPromoteMenu() {
			for (int index = 0; index < squareMenuRenderers.Length; index++) {
				Destroy(squareMenuRenderers[index].gameObject);
				//Destroy(menuPieceRenderers[index]);
			}
		}

		public void OnMoveMade(Board board, Move move) {
			lastMove = move;

			UpdatePosition(board);
			ResetSquareColor();
		}

		void HighlightMove(Move move) {
			SetSquareColor(BoardRepresentation.CoordFromIndex(move.StartSquare), boardTheme.Highlighted, boardTheme.Highlighted);
			SetSquareColor(BoardRepresentation.CoordFromIndex(move.TargetSquare), boardTheme.Highlighted, boardTheme.Highlighted);
		}

		public void HighLightLegalMove(Board board, Coord fromCoord) {
			if (!showLegalMoves) return ;

			List<Move> moves = moveGenerator.GenerateMoves(board);
			foreach (Move move in moves) {
				if (move.StartSquare == BoardRepresentation.IndexFromCoord(fromCoord)) {
					Coord coord = BoardRepresentation.CoordFromIndex(move.TargetSquare);
					SetSquareColor(coord, boardTheme.Legal, boardTheme.Legal);
				}
			}
		}

		public void UpdatePosition(Board board) {
			for (int file = 0; file < 8; file++) {
				for (int rank = 0; rank < 8; rank++) {
					int piece = board.Square[BoardRepresentation.IndexFromCoord(file, rank)];
					squarePieceRenderers[file, rank].sprite = pieceTheme.GetPieceSprite(piece);
					squarePieceRenderers[file, rank].transform.position = PositionFromCoord(file, rank, pieceDepth);

					highlightSquareRenderers[file, rank].transform.position = PositionFromCoord(file, rank, highlightSquareDepth);
				}
			}
		}

		public void UpdateLabel() {
			for (int index = 0; index < 8; index++) {
				int file = IsWhiteBottom ? index : 7 - index;
				int rank = IsWhiteBottom ? index : 7 - index;

				squareTextRenderers[index].sprite = textSpriteList[file];
				squareNumberRenderers[index].sprite = numberSpriteList[rank];
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
			SetSquareColor(coord, boardTheme.Selected, boardTheme.Selected);
		}

		public void DeselectSquare(Coord coord) {
			ResetSquareColor();
		}

		public void ResetSquareColor(bool highlight = true) {
			for (int file = 0; file < 8; file++) {
				for (int rank = 0; rank < 8; rank++) {
					SetSquareColor(new Coord(file, rank), boardTheme.LightSquares.Normal, boardTheme.DarkSquares.Normal);
				}
			}

			if (highlight && !lastMove.IsInvalid) {
				HighlightMove(lastMove);
			}
		}

		void SetSquareColor(Coord square, Color lightCol, Color darkCol) {
			highlightSquareRenderers[square.fileIndex, square.rankIndex].material.color = (square.IsLightSquare()) ? lightCol : darkCol;
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

		public void SetWhitePerspective(bool whitePov) {
			IsWhiteBottom = whitePov;
			ResetSquarePosition();
		}

		public void ToggleLegalMoves(bool show) {
			showLegalMoves = show;
		}
	}
}
