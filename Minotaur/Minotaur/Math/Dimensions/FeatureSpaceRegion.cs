namespace Minotaur.Math.Dimensions {
	using System;
	using Minotaur.Collections;

	public sealed class FeatureSpaceRegion {

		public readonly Array<IDimensionInterval> Dimensions;
		public readonly Array<FeatureType> DimensionTypes;

		private FeatureSpaceRegion(Array<IDimensionInterval> dimensions, Array<FeatureType> dimensionTypes) {
			Dimensions = dimensions ?? throw new ArgumentNullException(nameof(dimensions));
			DimensionTypes = dimensionTypes ?? throw new ArgumentNullException(nameof(dimensionTypes));

			if (DimensionTypes.Length != Dimensions.Length) {
				throw new ArgumentException(
					$"Both {nameof(dimensions)} " +
					$"and {nameof(dimensionTypes)} length must match.");
			}

			if (dimensions.ContainsNulls())
				throw new ArgumentException(nameof(dimensions) + " can't contain nulls.");
		}
	}
}
