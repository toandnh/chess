namespace Chess {
	public readonly struct Move {
		public readonly struct Flag {
			public const int None = 0;

			/////////////////////////////////////////////////////////
			// The "other" move flags
			public const int Castle = 1;
			public const int PawnTwoForward = 2;

			public const int PromoteToKnight = 4;
			public const int PromoteToBishop = 8;
			public const int PromoteToRook = 16;
			public const int PromoteToQueen = 32;
			/////////////////////////////////////////////////////////

			public const int EnPassant = 64;
			public const int Capture = 128;
			
			public const int Check = 256;
		}

		readonly ushort moveFlag;
		readonly ushort moveValue;

		const ushort StartSquareMask = 0b000000111111;
		const ushort TargetSquareMask = 0b111111000000;

		const ushort OtherFlagsMask = 0b111111;
		const ushort PromoteFlagsMask = 0b111100;

		public Move(ushort moveValue, ushort moveFlag) {
			this.moveValue = moveValue;
			this.moveFlag = moveFlag;
		}

		public Move(int startSquare, int targetSquare) {
			moveValue = (ushort) (startSquare | targetSquare << 6);
			moveFlag = 0;
		}

		public Move(int startSquare, int targetSquare, int flag) {
			moveValue = (ushort) (startSquare | targetSquare << 6);
			moveFlag = (ushort) flag;
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
				return moveFlag;
			}
		}

		public ushort MoveValue {
			get {
				return moveValue;
			}
		}
		
		public static Move InvalidMove {
			get {
				return new Move(0, 0);
			}
		}

		public bool IsInvalid {
			get {
				return moveValue == 0;
			}
		}
		
		public bool IsPromotion {
			get {
				return (moveFlag & Flag.PromoteToKnight) != 0 ||
								(moveFlag & Flag.PromoteToBishop) != 0 ||
								(moveFlag & Flag.PromoteToRook) != 0 ||
								(moveFlag & Flag.PromoteToQueen) != 0;
			}
		}
		
		public bool IsEnPassant {
			get {
				return (moveFlag & Flag.EnPassant) != 0;
			}
		}
		
		public bool IsCapture {
			get {
				return (moveFlag & Flag.Capture) != 0;
			}
		}
		
		public bool IsCheck {
			get {
				return (moveFlag & Flag.Check) != 0;
			}
		}
		
		public int OtherMoveFlags {
			get {
				return moveFlag & OtherFlagsMask;
			}
		}

		public int PromotionPiece {
			get {
				int promotionPiece = int.MinValue;
				int promoteFlag = moveFlag & PromoteFlagsMask;
				switch (promoteFlag) {
					case Flag.PromoteToKnight:
						promotionPiece = Piece.Knight;
						break;
					case Flag.PromoteToBishop:
						promotionPiece = Piece.Bishop;
						break;
					case Flag.PromoteToRook:
						promotionPiece = Piece.Rook;
						break;
					case Flag.PromoteToQueen:
						promotionPiece = Piece.Queen;
						break;
					default:
						break;
				}
				return promotionPiece;
			}
		}

		public string Name {
			get {
				return BoardRepresentation.SquareNameFromIndex(StartSquare) + "-" + BoardRepresentation.SquareNameFromIndex(TargetSquare);
			}
		}
		
		public override bool Equals(object obj) {
			if (obj is not Move move) return false;
			return moveValue.Equals(move.moveValue) && moveFlag.Equals(move.moveFlag);
		}
		
		public override int GetHashCode() {
			return moveValue.GetHashCode() ^ moveFlag.GetHashCode();
		}
		
		public static bool operator == (Move move1, Move move2) {
			return move1.Equals(move2);
		}
		
		public static bool operator != (Move move1, Move move2) {
			return !move1.Equals(move2);
		}
	}
}