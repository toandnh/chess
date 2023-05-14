using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Game {
	public class GameManager : MonoBehaviour {
		public Board board { get; private set; }
		BoardUI boardUI;

		void Start() {
			board = new Board();
			boardUI = FindObjectOfType<BoardUI>();

			NewGame();
		}

		void NewGame() {
			board.LoadStartPosition();
			Debug.Log(board);
			boardUI.UpdatePosition(board);
			Debug.Log(boardUI);
		}
	}
}
