using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Chess {
	public static class BitBoardUtilities {
		public static bool HasSquare(ulong bitBoard, int square) {
			return ((bitBoard >> square) & 1) != 0;
		}
	}
}