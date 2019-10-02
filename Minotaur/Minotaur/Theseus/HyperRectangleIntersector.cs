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
				FeatureType.Binary => IntersectsBinary(builder, rect, dimensionIndex),
				FeatureType.Continuous => IntersectsContinuous(builder, rect, dimensionIndex),

				_ => throw CommonExceptions.UnknownFeatureType
			};
		}

		private bool IntersectsBinary(HyperRectangleBuilder builder, HyperRectangle rect, int dimensionIndex) {
			(var containsFalse, var containsTrue) = builder.GetCategoricalDimensionPreview(dimensionIndex);
			var interval = (BinaryDimensionInterval) rect.GetDimensionInterval(dimensionIndex);

			if (interval.ContainsFalse && containsFalse)
				return true;
			if (interval.ContainsTrue && containsTrue)
				return true;

			return false;
		}

		private bool IntersectsContinuous(HyperRectangleBuilder builder, HyperRectangle rect, int dimensionIndex) {
			(var start, var end) = builder.GetContinuousDimensionPreview(dimensionIndex);
			var interval = (ContinuousDimensionInterval) rect.GetDimensionInterval(dimensionIndex);

			throw new NotImplementedException();
		}
	}
}
