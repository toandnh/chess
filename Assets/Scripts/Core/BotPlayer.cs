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
			List<Move> moves = moveGenerator.GenerateMoves(board);

			Random random = new Random();
			int chosenIndex = random.Next(moves.Count);

			ChoseMove(moves[chosenIndex]);
		}
	}
}