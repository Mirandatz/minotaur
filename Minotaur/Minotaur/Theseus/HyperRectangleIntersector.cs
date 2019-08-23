namespace Minotaur.Theseus {
	using System;
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

			if (!target.IsCompatibleWith(other)) {
				throw new ArgumentException(
					$"{nameof(target)} " +
					$"must be compatible (i.e. same number and types of dimensions " +
					$"with {nameof(other)}");
			}

			var dimensionCount = target.DimensionCount;
			for (int i = 0; i < dimensionCount; i++) {
				if (i == dimensionToSkip)
					continue;

				var lhs = target.GetDimensionInterval(i);
				var rhs = other.GetDimensionInterval(i);
				if (Intersects(lhs, rhs))
					return true;
			}

			return false;
		}

		private static bool Intersects(IDimensionInterval lhs, IDimensionInterval rhs) {
			var lhsCat = lhs as CategoricalDimensionInterval;
			var rhsCat = rhs as CategoricalDimensionInterval;
			if (!(lhsCat is null))
				return IntersectsCategorical(lhsCat, rhsCat);


			var lhsCont = lhs as ContinuousDimensionInterval;
			var rhsCont = rhs as ContinuousDimensionInterval;
			if (!(lhsCont is null))
				return IntersectsContinuous(lhsCont, rhsCont);

			throw new InvalidOperationException("This line should never be reached.");
		}

		private static bool IntersectsCategorical(CategoricalDimensionInterval lhsCat, CategoricalDimensionInterval rhsCat) {
			var lhsValues = lhsCat.SortedValues.Span;
			var rhsValues = rhsCat.SortedValues.Span;

			if (lhsValues.IsEmpty || rhsValues.IsEmpty)
				return false;

			// @Improve performance
			for (int i = 0; i < lhsValues.Length; i++) {
				var featureValue = lhsValues[i];
				var indexOnRhs = rhsValues.BinarySearch(featureValue);
				if (indexOnRhs != -1)
					return true;
			}

			return false;
		}

		public static bool IntersectsContinuous(ContinuousDimensionInterval lhsCont, ContinuousDimensionInterval rhsCont) {
			// @Improve performance?
			// @Verify correctness
			var aStart = lhsCont.Start;
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
