using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Game {
	public class GameManager : MonoBehaviour {
		public enum State { Playing }

		public event System.Action<Move> onMoveMade;

		AudioSource Capture;
		AudioSource Castle;
		AudioSource MoveCheck;
		AudioSource MoveSelf;
		AudioSource Promote;

		public enum PlayerType {Human, AI}

		State gameState;

		public PlayerType whitePlayerType;
		public PlayerType blackPlayerType;

		Player whitePlayer;
		Player blackPlayer;
		Player playerToMove;

		public Board board { get; private set; }
		BoardUI boardUI;

		void Start() {
			board = new Board();
			boardUI = FindObjectOfType<BoardUI>();

			Capture = GetComponent<AudioSource>();
			Castle = GetComponent<AudioSource>();
			MoveCheck = GetComponent<AudioSource>();
			MoveSelf = GetComponent<AudioSource>();
			Promote = GetComponent<AudioSource>();

			NewGame(whitePlayerType, blackPlayerType);
		}

		void Update() {
			if (gameState == State.Playing) {
				playerToMove.Update();
			}
		}

		public void NewGame(bool humanPlayWhite) {
			boardUI.SetPerspective(humanPlayWhite);
			NewGame(PlayerType.Human, PlayerType.Human);
		}

		void NewGame(PlayerType whitePlayerType, PlayerType blackPlayerType) {
			board.LoadStartPosition();

			boardUI.UpdatePosition(board);
			boardUI.ResetSquareColor();

			CreatePlayer(ref whitePlayer, whitePlayerType);
			CreatePlayer(ref blackPlayer, blackPlayerType);

			gameState = State.Playing;

			NotifyPlayerToMove();
		}

		void CreatePlayer(ref Player player, PlayerType playerType) {
			if (player != null) {
				player.onMoveChosen -= OnMoveChosen;
			}

			if (playerType == PlayerType.Human) {
				player = new HumanPlayer(board);
			} else {

			}
			player.onMoveChosen += OnMoveChosen;
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

			onMoveMade?.Invoke(move);
			MoveSelf.Play();
			
			boardUI.OnMoveMade(board, move);

			NotifyPlayerToMove();
		}
	}
}
