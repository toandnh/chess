// https://www.chessprogramming.org/Bob_Jenkins#RKISS
public class RKiss {
	struct RandContext {
		public ulong a;
		public ulong b;
		public ulong c;
		public ulong d;
	}

	static RandContext ranctx;

	public RKiss(ulong seed) {
		ranctx.a = 0xf1ea5eed;
		ranctx.b = ranctx.c = ranctx.d = seed;
		for (int i = 0; i < 20; i++) {
			RandValue();
		}
	}

	public ulong RandValue() {
		ulong e = ranctx.a - rotate(ranctx.b, 7);
		ranctx.a = ranctx.b ^ rotate(ranctx.c, 13);
		ranctx.b = ranctx.c + rotate(ranctx.d, 37);
		ranctx.c = ranctx.d + e;
		ranctx.d = e + ranctx.a;
		return ranctx.d;
	}

	ulong rotate(ulong x, int k) {
		return ((x << k) | (x >> (64 - k)));
	}
}