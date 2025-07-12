using System;
using System.Collections;
using System.Collections.Generic;

// To disambiguate between System.Random and UnityEngine.Random (CS0104)
using Random = System.Random;

using UnityEngine;

namespace Chess.Game {
	public class GameManager : MonoBehaviour {
		public enum State { Playing, GameOver }

		public event Action<Move> OnMoveMade;

		public string FEN = "";

		public AudioSource capture;
		public AudioSource castle;
		public AudioSource moveCheck;
		public AudioSource moveSelf;
		public AudioSource promote;

		public enum PlayerType { Human, Bot }

		State gameState;

		public PlayerType WhitePlayerType;
		public PlayerType BlackPlayerType;

		Player whitePlayer;
		Player blackPlayer;
		Player playerToMove;

		MoveText moveText;
		Board board;

		BoardUI boardUI;
		MoveTextUI moveTextUI;
		CaptureUI captureUI;

		void Start() {
			moveText = new MoveText();
			board = new Board();
			
			boardUI = FindObjectOfType<BoardUI>();
			moveTextUI = FindObjectOfType<MoveTextUI>();
			captureUI = FindObjectOfType<CaptureUI>();

			//NewGame(WhitePlayerType, BlackPlayerType);
			NewRandomGame();
		}

		void Update() {
			if (gameState == State.Playing) {
				playerToMove.Update();
			}
			else if (gameState == State.GameOver) {
				//
			}
		}

		public void NewRandomGame() {
			bool[] options = { true, false };

			Random random = new Random();
			int chosenIndex = random.Next(options.Length);

			NewGame(options[chosenIndex]);
		}
		
		public void NewGameFromFen() {
			boardUI.SetWhitePerspective(true);
			NewGame(FEN, PlayerType.Human, PlayerType.Human);
		}

		public void NewBlackGame()
		{
			NewGame(false);
		}

		public void NewWhiteGame() {
			NewGame(true);
		}

		public void NewGame(bool humanPlayWhite) {
			boardUI.SetWhitePerspective(humanPlayWhite);
			//NewGame(PlayerType.Human, PlayerType.Human);
			PlayerType player1 = humanPlayWhite ? PlayerType.Human : PlayerType.Bot;
			PlayerType player2 = player1 == PlayerType.Human ? PlayerType.Bot : PlayerType.Human;
			NewGame(null, player1, player2);
		}

		void NewGame(string fen, PlayerType whitePlayerType, PlayerType blackPlayerType) {
			board.LoadCustomPosition(fen ?? FenUtility.StartFen);

			moveText.ResetMoveText();

			boardUI.UpdateLabel();
			boardUI.UpdatePosition(board);
			boardUI.ResetSquareColor(false);

			moveTextUI.ResetMoveText();

			// captureUI.ResetCapture();
			// This only re-draw the captures pieces of the moving side
			// captureUI.OnMoveMade(board, boardUI);
			captureUI.DrawCapturedPieces(board, boardUI, Piece.White);
			captureUI.DrawCapturedPieces(board, boardUI, Piece.Black);

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
			} else if (playerType == PlayerType.Bot) {
				player = new BotPlayer(board);
			}
			player.OnMoveChosen += OnMoveChosen;
		}

		void NotifyPlayerToMove() {
			if (gameState == State.Playing) {
				playerToMove = board.WhiteToMove ? whitePlayer : blackPlayer;
				playerToMove.NotifyTurnToMove();
			} else if (gameState == State.GameOver) {
				//
			}
		}

		public void UpdateFen(string fen) {
			FEN = fen;
		}

		void OnMoveChosen(Move move) {
			if (move.IsInvalid) return ;

			bool isCapture = false;
			if (board.Square[move.TargetSquare] != 0) {
				isCapture = true;
			}

			moveText.GenerateMoveText(move, board);
			board.MakeMove(move);

			OnMoveMade?.Invoke(move);
			
			boardUI.OnMoveMade(board, move);
			moveTextUI.OnMoveMade(moveText, board.WhiteToMove);
			
			// Clear or unset the MSB - check bit
			int moveFlag = move.MoveFlag & ~(1 << 3);
			// Get the MSB - check bit
			bool check = ((move.MoveFlag >> 3) & 1) != 0;

			if (isCapture || moveFlag == Move.Flag.EnPassant) {
				captureUI.OnMoveMade(board, boardUI);
				capture.Play();
			}

			switch (moveFlag) {
				case Move.Flag.Castle:
					castle.Play();
					break;
				case Move.Flag.None:
				case Move.Flag.PawnTwoForward:
					moveSelf.Play();
					break;
				case Move.Flag.PromoteToKnight:
				case Move.Flag.PromoteToBishop:
				case Move.Flag.PromoteToRook:
				case Move.Flag.PromoteToQueen:
					promote.Play();
					break;
			}

			if (check) moveCheck.Play();

			NotifyPlayerToMove();
		}
	}
}
