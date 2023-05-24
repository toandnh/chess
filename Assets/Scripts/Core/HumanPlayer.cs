using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Game {
	public class HumanPlayer : Player {
		public enum InputState {
			None,
			PieceSelected,
			DraggingPiece
		}

		InputState currentState;

		Camera cam;

		Board board;
		BoardUI boardUI;
		Coord selectedPieceSquare;

		public HumanPlayer(Board board) {
			this.board = board;
			boardUI = GameObject.FindObjectOfType<BoardUI>();
			cam = Camera.main;
		}

		public override void Update() {
			HandleInput();
		}

		public override void NotifyTurnToMove() {}

		void HandleInput() {
			Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

			switch (currentState) {
				case InputState.None:
					HandlePieceSelection(mousePos);
					break;
				case InputState.PieceSelected:
					HandlePointAndClickMovement(mousePos);
					break;
				case InputState.DraggingPiece:
					HandleDragMovement(mousePos);
					break;
				default:
					break;
			}

			if (Input.GetMouseButtonDown(1)) {
				CancelPieceSelection();
			}
		}

		void HandlePieceSelection(Vector2 mousePos) {
			if (Input.GetMouseButtonDown(0)) {
				if (boardUI.TryGetSquareUnderMouse(mousePos, out selectedPieceSquare)) {
					int index = BoardRepresentation.IndexFromCoord(selectedPieceSquare);
					// If square contains a piece, select that piece for dragging
					if (Piece.IsColor(board.Square[index], board.ColorToMove)) {
						currentState = InputState.DraggingPiece;
						boardUI.SelectSquare(selectedPieceSquare);
					}
				}
			}
		}

		void HandlePointAndClickMovement(Vector2 mousePos) {
			if (Input.GetMouseButton(0)) {
				HandlePiecePlacement(mousePos);
			}
		}

		void HandleDragMovement(Vector2 mousePos) {
			boardUI.DragPiece(selectedPieceSquare, mousePos);
			// Try to place the piece on mouse release
			if (Input.GetMouseButtonUp(0)) {
				HandlePiecePlacement(mousePos);
			}
		}

		void HandlePiecePlacement(Vector2 mousePos) {
			Coord targetSquare;

			if (boardUI.TryGetSquareUnderMouse(mousePos, out targetSquare)) {
				if (targetSquare.Equals(selectedPieceSquare)) {
					boardUI.ResetPiecePosition(selectedPieceSquare);
					if (currentState == InputState.DraggingPiece) {
						currentState = InputState.PieceSelected;
					} else {
						currentState = InputState.None;
						boardUI.DeselectSquare(selectedPieceSquare);
					}
				} else {
					int targetIndex = BoardRepresentation.IndexFromCoord(targetSquare.fileIndex, targetSquare.rankIndex);
					if (Piece.IsColor(board.Square[targetIndex], board.ColorToMove) && board.Square[targetIndex] != 0) {
						CancelPieceSelection();
						HandlePieceSelection(mousePos);
					} else {
						TryMakeMove(selectedPieceSquare, targetSquare);
					}
				}
			} else {
				CancelPieceSelection();
			}
		}

		void TryMakeMove(Coord startSquare, Coord targetSquare) {
			int startIndex = BoardRepresentation.IndexFromCoord(startSquare);
			int targetIndex = BoardRepresentation.IndexFromCoord(targetSquare);

			bool isLegalMove = false;

			Move chosenMove = new Move(startIndex, targetIndex);
			MoveGenerator moveGenerator = new MoveGenerator();

			var legalMoves = moveGenerator.GenerateMoves(board);

			//Debug.Log(BoardRepresentation.FileNames[startSquare.fileIndex] + "" + BoardRepresentation.RankNames[startSquare.rankIndex] + " -> " 
			//					+ BoardRepresentation.FileNames[targetSquare.fileIndex] + "" + BoardRepresentation.RankNames[targetSquare.rankIndex]);
			//Debug.Log(legalMoves.Count);

			for (int i = 0; i < legalMoves.Count; i++) {
				var legalMove = legalMoves[i];
				if (legalMove.StartSquare == startIndex && legalMove.TargetSquare == targetIndex) {
					isLegalMove = true;
					chosenMove = legalMove;
					break;
				}
			}

			if (isLegalMove) {
				currentState = InputState.None;
				ChoseMove(chosenMove);
			} else {
				CancelPieceSelection();
			}
		}

		void CancelPieceSelection() {
			if (currentState != InputState.None) {
				currentState = InputState.None;
				boardUI.DeselectSquare(selectedPieceSquare);
				boardUI.ResetPiecePosition(selectedPieceSquare);
			}
		}
	}
} 