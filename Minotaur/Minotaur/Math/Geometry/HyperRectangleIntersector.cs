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
			return Intersects(
				lhsStart: lhs.InclusiveStart,
				lhsEnd: lhs.ExclusiveEnd,
				rhsStart: rhs.InclusiveStart,
				rhsEnd: rhs.ExclusiveEnd);
		}

		private static bool Intersects(float lhsStart, float lhsEnd, float rhsStart, float rhsEnd) {
			// @Danger: this might be wrong...
			if (lhsStart >= rhsEnd)
				return false;
			if (lhsEnd <= rhsStart)
				return false;

			return true;
		}
	}
}
