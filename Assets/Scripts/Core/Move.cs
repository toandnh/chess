namespace Chess {
	public readonly struct Move {

		// In the form 0000; with the MSB reserved for check flag
		// e.g. 1010 indicates the move is a capture into a check
		public readonly struct Flag {
			public const int None = 0;
			public const int EnPassant = 1;
			public const int Capture = 2;
			public const int Castle = 3;
			public const int PawnTwoForward = 4;
			public const int Promote = 5;

			public const int Check = 8;
		}
		readonly ushort moveValue;

		const ushort StartSquareMask = 0b0000000000111111;
		const ushort TargetSquareMask = 0b0000111111000000;

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
				return moveValue & StartSquareMask;
			}
		}

		public int TargetSquare {
			get {
				return (moveValue & TargetSquareMask) >> 6;
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