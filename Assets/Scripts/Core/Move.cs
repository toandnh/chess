namespace Chess {
	public readonly struct Move {

		public readonly struct Flag {
			public const int None = 0;
			public const int EnPassant = 1;
			public const int Castling = 2;
			public const int PawnTwoForward = 7;
		}
		readonly ushort moveValue;

		const ushort startSquareMask = 0b000000000111111;
		const ushort targetSquareMask = 0b000111111000000;

		public Move(ushort moveValue) {
			this.moveValue = moveValue;
		}

		public Move(int startSquare, int targetSquare) {
			moveValue = (ushort) (startSquare | targetSquare << 6);
		}

		public Move(int startSquare, int targetSquare, int flag) {
			moveValue = (ushort) (startSquare | targetSquare << 6 | flag << 12);
		}

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

		public int MoveFlag {
			get {
				return moveValue >> 12;
			}
		}

		public static Move InvalidMove {
			get {
				return new Move(0);
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
	}
}