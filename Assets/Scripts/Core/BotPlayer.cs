using System.Collections.Generic;

using Random = System.Random;

using UnityEngine;

namespace Chess.Game {
	public class BotPlayer : Player {
		Board board;
		MoveGenerator moveGenerator;

		public BotPlayer(Board board) {
			this.board = board;
			moveGenerator = new MoveGenerator();
		}

		public override void Update() {
			ComputeMove();
		}

		public override void NotifyTurnToMove() {
			return ;
		}

		void ComputeMove() {
			int depth = 3;

			Search search = new Search(board);
			Move bestMove = search.FindMove(depth);

			ChoseMove(bestMove);
		}
	}
}