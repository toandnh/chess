using System.Collections.Generic;

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
			List<Move> moves = moveGenerator.GenerateMoves(board);
			ChoseMove(moves[0]);
		}
	}
}