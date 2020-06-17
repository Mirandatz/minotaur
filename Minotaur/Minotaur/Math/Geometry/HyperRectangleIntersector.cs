namespace Minotaur.Math.Geometry {
	using System;

	public static class HyperRectangleIntersector {

		public static bool Intersects(HyperRectangle first, HyperRectangle second) {

			var lhs = first.AsSpan();
			var rhs = second.AsSpan();

			if (lhs.Length != rhs.Length)
				throw new InvalidOperationException();

			for (int i = 0; i < lhs.Length; i++) {
				if (!Intersects(lhs[i], rhs[i]))
					return false;
			}

			return true;
		}

		private static bool Intersects(Interval lhs, Interval rhs) {
			if (lhs.InclusiveStart >= rhs.ExclusiveEnd)
				return false;
			if (lhs.ExclusiveEnd <= rhs.InclusiveStart)
				return false;

			return true;
		}
	}
}
