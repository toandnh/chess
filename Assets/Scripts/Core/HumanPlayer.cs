using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Chess.Game {
	public class HumanPlayer : Player {
		public enum InputState {
			None,
			PieceSelected,
			DraggingPiece,
			PromotePiece
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

		public override void NotifyTurnToMove() {
			return ;
		}

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
				case InputState.PromotePiece:
					HandleMenuPieceSelection(mousePos);
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
				if (boardUI.CanGetSquareUnderMouse(mousePos, out selectedPieceSquare)) {
					int index = BoardRepresentation.IndexFromCoord(selectedPieceSquare);
					// If square contains a piece, select that piece for dragging
					if (Piece.IsColor(board.Square[index], board.ColorToMove)) {
						currentState = InputState.DraggingPiece;
						boardUI.SelectSquare(selectedPieceSquare);
					}
				}
			}
		}

		void HandleMenuPieceSelection(Vector2 mousePos) {
			if (Input.GetMouseButtonDown(0)) {
				if (boardUI.CanGetSquareUnderMouse(mousePos, out selectedPieceSquare)) {
					int index = BoardRepresentation.IndexFromCoord(selectedPieceSquare);

					int startSquareIndex = boardUI.PromoteStartSquareIndex;
					int endSquareIndex = startSquareIndex - 8 * 4;

					// Cancel move
					if (index == endSquareIndex) {
						board.PromotePiece = -1;
						boardUI.DestroyPromoteMenu();
						currentState = InputState.None;
						return ;
					}

					// If index is one of menu squares
					for (int squareIndex = startSquareIndex; squareIndex > endSquareIndex; squareIndex -= 8) {
						if (index == squareIndex) {
							int pieceIndex = 7 - BoardRepresentation.RankIndex(squareIndex);

							// Map the location of the pieces in the menu to its number representation
							// E.g. The rook appears second in the menu, and its number representation is Piece.Rook = 4; 5 - 1 = 4
							// See Piece class for more information on number representation of pieces
							int promotePiece = 5 - pieceIndex;
							board.PromotePiece = promotePiece;

							boardUI.DestroyPromoteMenu();
							currentState = InputState.None;

							int flag = Move.Flag.Promote;
							int opponentKingSquare = board.PieceList.GetValue(Piece.King)[board.OpponentColor].ToArray()[0];
							if (MoveGeneratorUtility.IsCheck(board.Square, startSquareIndex, promotePiece, opponentKingSquare)) {
								flag |= Move.Flag.Check;
							}
							Move chosenMove = new Move(startSquareIndex - 8, startSquareIndex, flag);

							ChoseMove(chosenMove);

							break;
						}
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
			
			if (boardUI.CanGetSquareUnderMouse(mousePos, out targetSquare)) {
				// Return the piece to its original position
				if (targetSquare.Equals(selectedPieceSquare)) {
					boardUI.ResetPiecePosition(selectedPieceSquare);
					if (currentState == InputState.DraggingPiece) {
						currentState = InputState.PieceSelected;
					} else {
						currentState = InputState.None;
						boardUI.DeselectSquare(selectedPieceSquare);
					}
				// Try to move the piece to its target position
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

			for (int i = 0; i < legalMoves.Count; i++) {
				var legalMove = legalMoves[i];
				if (legalMove.StartSquare == startIndex && legalMove.TargetSquare == targetIndex) {
					isLegalMove = true;
					chosenMove = legalMove;
					break;
				}
			}

			if (isLegalMove) {
				// Promotion move
				if (chosenMove.MoveFlag == Move.Flag.Promote) {
					CancelPieceSelection();
					boardUI.CreatePromoteMenu(chosenMove.TargetSquare, board.ColorToMove);
					boardUI.PromoteStartSquareIndex = targetIndex;
					currentState = InputState.PromotePiece;
					return ;
				}

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