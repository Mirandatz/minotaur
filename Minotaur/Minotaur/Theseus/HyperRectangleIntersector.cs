namespace Minotaur.Theseus {
	using System;
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
			var interval = (ContinuousDimensionInterval) rect.GetDimensionInterval(dimensionIndex);

			// @Danger: this might be wrong...
			if (end <= interval.Start)
				return false;
			if (start >= interval.End)
				return false;

			return true;
		}

		public bool IntersectsInAllDimension(HyperRectangle lhsBox, HyperRectangle rhsBox) {
			if (lhsBox.DimensionCount != rhsBox.DimensionCount)
				throw new InvalidOperationException();

			var dimensionCount = lhsBox.DimensionCount;
			for (int i = 0; i < dimensionCount; i++) {
				var lhsInterval = (ContinuousDimensionInterval) lhsBox.GetDimensionInterval(i);
				var rhsInterval = (ContinuousDimensionInterval) rhsBox.GetDimensionInterval(i);
				if (!ContinuousDimensionIntervalIntersects(lhsInterval, rhsInterval))
					return false;
			}

			return true;
		}

		private bool ContinuousDimensionIntervalIntersects(ContinuousDimensionInterval lhsInterval, ContinuousDimensionInterval rhsInterval) {
			// @Danger: this might be wrong...
			if (lhsInterval.Start >= rhsInterval.End)
				return false;
			if (rhsInterval.End <= lhsInterval.Start)
				return false;

			return true;
		}
	}
}
