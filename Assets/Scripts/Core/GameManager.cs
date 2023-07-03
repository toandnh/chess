using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Game {
	public class GameManager : MonoBehaviour {
		public enum State { Playing }

		public event System.Action<Move> OnMoveMade;

		public AudioSource capture;
		public AudioSource castle;
		public AudioSource moveCheck;
		public AudioSource moveSelf;
		public AudioSource promote;

		public enum PlayerType {Human, AI}

		State gameState;

		public PlayerType whitePlayerType;
		public PlayerType blackPlayerType;

		Player whitePlayer;
		Player blackPlayer;
		Player playerToMove;

		Board board;
		BoardUI boardUI;

		//string testPromotionFen = "8/6P1/3k1K2/8/8/8/8/8 w - - 0 1";

		void Start() {
			board = new Board();
			boardUI = FindObjectOfType<BoardUI>();

			NewGame(whitePlayerType, blackPlayerType);
		}

		void Update() {
			if (gameState == State.Playing) {
				playerToMove.Update();
			}
		}

		public void NewGame(bool humanPlayWhite) {
			boardUI.SetWhitePerspective(humanPlayWhite);
			NewGame(PlayerType.Human, PlayerType.Human);
		}

		void NewGame(PlayerType whitePlayerType, PlayerType blackPlayerType) {
			board.LoadStartPosition();
			//board.LoadCustomPosition(testPromotionFen);

			boardUI.UpdatePosition(board);
			boardUI.ResetSquareColor();

			CreatePlayer(ref whitePlayer, whitePlayerType);
			CreatePlayer(ref blackPlayer, blackPlayerType);

			gameState = State.Playing;

			NotifyPlayerToMove();
		}

		void CreatePlayer(ref Player player, PlayerType playerType) {
			if (player != null) {
				player.OnMoveChosen -= OnMoveChosen;
			}

			if (playerType == PlayerType.Human) {
				player = new HumanPlayer(board);
			} else {

			}
			player.OnMoveChosen += OnMoveChosen;
		}

		void NotifyPlayerToMove() {
			gameState = GetGameState();

			if (gameState == State.Playing) {
				playerToMove = board.WhiteToMove ? whitePlayer : blackPlayer;
				playerToMove.NotifyTurnToMove();
			}
		}

		State GetGameState() {
			return State.Playing;
		}

		void OnMoveChosen(Move move) {
			board.MakeMove(move);

			OnMoveMade?.Invoke(move);
			
			boardUI.OnMoveMade(board, move);

			switch (move.MoveFlag) {
				case Move.Flag.EnPassant:
				case Move.Flag.Capture:
					capture.Play();
					break;
				case Move.Flag.Castle:
					castle.Play();
					break;
				case Move.Flag.Check:
					moveCheck.Play();
					break;
				case Move.Flag.None:
				case Move.Flag.PawnTwoForward:
					moveSelf.Play();
					break;
				case Move.Flag.Promote:
					promote.Play();
					break;
			}

			NotifyPlayerToMove();
		}
	}
}
