namespace Minotaur.Theseus {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Minotaur.Collections;
	using Minotaur.Math.Dimensions;

	public static class HyperRectangleIntersector {

		/// <summary>
		/// This one really needs documentation
		/// </summary>
		public static bool Intersects(
			MutableHyperRectangle target,
			HyperRectangle other,
			int dimensionToSkip
			) {
			if (target is null)
				throw new ArgumentNullException(nameof(target));
			if (other is null)
				throw new ArgumentNullException(nameof(other));
			if (target.DimensionCount != other.Dimensions.Length)
				throw new InvalidOperationException();
			if (dimensionToSkip < 0 || dimensionToSkip >= target.DimensionCount)
				throw new ArgumentOutOfRangeException(nameof(dimensionToSkip));

			var dimensionCount = target.DimensionCount;
			for (int i = 0; i < dimensionCount; i++) {
				if (i == dimensionToSkip)
					continue;

				var lhs = target.GetDimensionInterval(i);
				var rhs = other.Dimensions[i];

				if (Intersects(lhs, rhs))
					return true;
			}

			return false;
		}

		private static bool Intersects(IDimensionInterval lhs, IDimensionInterval rhs) {
			var lhsCat = lhs as CategoricalDimensionInterval;
			var rhsCat = rhs as CategoricalDimensionInterval;
			if (!(lhsCat is null) && !(rhsCat is null)) {
				return IntersectsCategorical(lhsCat, rhsCat);
			}

			var lhsCont = lhs as ContinuousDimensionInterval;
			var rhsCont = rhs as ContinuousDimensionInterval;
			if (!(lhsCont is null) && !(rhsCont is null)) {
				return IntersectsContinuous(lhsCont, rhsCont);
			}

			throw new InvalidOperationException();
		}

		private static bool IntersectsCategorical(CategoricalDimensionInterval lhsCat, CategoricalDimensionInterval rhsCat) {
			var lhsValues = lhsCat.SortedValues;
			var rhsValues = rhsCat.SortedValues;

			throw new NotImplementedException();
		}

		public static bool IntersectsContinuous(ContinuousDimensionInterval lhsCont, ContinuousDimensionInterval rhsCont) {
			// @Improve performance?
			// @Verify correctness
			var aStart= lhsCont.Start;
			var aEnd = lhsCont.End;

			var bStart = rhsCont.Start;
			var bEnd = rhsCont.End;

			if (aStart.Value > bEnd.Value)
				return false;
			if (aStart.Value == bEnd.Value)
				return aStart.IsInclusive && bEnd.IsInclusive;

			if (bStart.Value > aEnd.Value)
				return false;
			if (bStart.Value == aEnd.Value)
				return bStart.IsInclusive && aEnd.IsInclusive;

			return true;
		}
	}
}
