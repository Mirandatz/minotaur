namespace Minotaur.Theseus {
	using System;
	using Minotaur.Classification;
	using Minotaur.Collections.Dataset;
	using Minotaur.Math.Dimensions;

	public sealed class HyperRectangleIntersector {

		public readonly Dataset Dataset;

		public HyperRectangleIntersector(Dataset dataset) {
			Dataset = dataset;
		}

		public bool IntersectsInAllButOneDimension(HyperRectangleBuilder builder, HyperRectangle rect, int dimensionToSkip) {
			var dimensionCount = Dataset.FeatureCount;

			for (int i = 0; i < dimensionCount; i++) {
				if (i == dimensionToSkip)
					continue;

				var intersects = Intersects(
					builder: builder,
					rect: rect,
					dimensionIndex: i);

				if (!intersects)
					return false;
			}

			return true;
		}

		private bool Intersects(HyperRectangleBuilder builder, HyperRectangle rect, int dimensionIndex) {
			return Dataset.GetFeatureType(dimensionIndex) switch
			{
				FeatureType.Continuous => IntersectsContinuous(builder, rect, dimensionIndex),

				_ => throw CommonExceptions.UnknownFeatureType
			};
		}

		private bool IntersectsContinuous(HyperRectangleBuilder builder, HyperRectangle rect, int dimensionIndex) {
			(var start, var end) = builder.GetContinuousDimensionPreview(dimensionIndex);
			var interval = (ContinuousInterval) rect.GetDimensionInterval(dimensionIndex);

			return Intersects(
				aStart: start,
				aEnd: end,
				bStart: interval.Start,
				bEnd: interval.End);
		}

		public bool IntersectsInAllDimension(HyperRectangle lhsBox, HyperRectangle rhsBox) {
			if (lhsBox.DimensionCount != rhsBox.DimensionCount)
				throw new InvalidOperationException();

			var dimensionCount = lhsBox.DimensionCount;
			for (int i = 0; i < dimensionCount; i++) {
				var lhsInterval = (ContinuousInterval) lhsBox.GetDimensionInterval(i);
				var rhsInterval = (ContinuousInterval) rhsBox.GetDimensionInterval(i);
				if (!ContinuousDimensionIntervalIntersects(lhsInterval, rhsInterval))
					return false;
			}

			return true;
		}

		private bool ContinuousDimensionIntervalIntersects(ContinuousInterval lhsInterval, ContinuousInterval rhsInterval) {
			return Intersects(
				aStart: lhsInterval.Start,
				aEnd: lhsInterval.End,
				bStart: rhsInterval.Start,
				bEnd: rhsInterval.End);
		}

		private static bool Intersects(float aStart, float aEnd, float bStart, float bEnd) {
			if (aStart > aEnd)
				throw new InvalidOperationException();
			if (bStart > bEnd)
				throw new InvalidOperationException();

			// @Danger: this might be wrong...
			if (aStart >= bEnd)
				return false;
			if (aEnd <= bStart)
				return false;

			return true;
		}
	}
}
