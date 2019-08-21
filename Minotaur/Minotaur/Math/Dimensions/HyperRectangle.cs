namespace Minotaur.Math.Dimensions {
	using System;
	using Minotaur.Collections;

	public sealed class HyperRectangle {
		public readonly Array<IDimensionInterval> Dimensions;

		public HyperRectangle(Array<IDimensionInterval> dimensions) {
			if (dimensions == null)
				throw new ArgumentNullException(nameof(dimensions));

			Dimensions = dimensions.Clone();

			// Checking wether the dimensions are not null and that their
			// dimensions indices match with their positions in the provided array
			for (int i = 0; i < dimensions.Length; i++) {
				var dimension = dimensions[i];

				if (dimension == null)
					throw new ArgumentException(nameof(dimension) + " can't contain nulls.");

				var dimensionIndex = dimension.DimensionIndex;
				if (dimensionIndex != i) {
					throw new ArgumentException($"" +
						$"There is a mismatch between {nameof(IDimensionInterval.DimensionIndex)}" +
						$"at position {i}.");
				}
			}
		}
	}
}