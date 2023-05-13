using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Game {
	public class GameManager : MonoBehaviour {
		BoardUI boardUI;

		void Start() {
			boardUI = FindObjectOfType<BoardUI>();
			Debug.Log(boardUI);
		}
	}
}
