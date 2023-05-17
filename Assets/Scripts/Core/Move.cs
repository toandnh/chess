namespace Chess {
	public readonly struct Move {
		readonly ushort moveValue;

		const ushort startSquareMask = 0b000000000111111;
		const ushort targetSquareMask = 0b000111111000000;

		public int StartSquare {
			get {
				return moveValue & startSquareMask;
			}
		}

		public int TargetSquare {
			get {
				return (moveValue & targetSquareMask) >> 6;
			}
		}

		public ushort Value {
			get {
				return moveValue;
			}
		}

		public bool IsInvalid {
			get {
				return moveValue == 0;
			}
		}

		public string Name {
			get {
				return BoardRepresentation.SquareNameFromIndex(StartSquare) + "-" + BoardRepresentation.SquareNameFromIndex(TargetSquare);
			}
		}

		public static Move InvalidMove {
			get {
				return new Move(0);
			}
		}

		public Move(ushort moveValue) {
			this.moveValue = moveValue;
		}

		public Move(int startSquare, int targetSquare) {
			moveValue = (ushort) (startSquare | targetSquare << 6);
		}
	}
}