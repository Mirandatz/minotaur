namespace Minotaur.Math.Dimensions {
	using System;
	using Minotaur.Collections;

	public sealed class HyperRectangle {
		public readonly Array<IDimensionInterval> Dimensions;
		public readonly Array<FeatureType> DimensionTypes;

		public HyperRectangle(Array<IDimensionInterval> dimensions, Array<FeatureType> dimensionTypes) {
			Dimensions = dimensions ?? throw new ArgumentNullException(nameof(dimensions));
			DimensionTypes = dimensionTypes ?? throw new ArgumentNullException(nameof(dimensionTypes));

			if (DimensionTypes.Length != Dimensions.Length) {
				throw new ArgumentException(
					$"Both {nameof(dimensions)} " +
					$"and {nameof(dimensionTypes)} length must match.");
			}

			// Checking whether the dimensions contains nulls,
			// if their indices match with their positions in the array and
			// if the dimension types matches the provided dimension types
			for (int i = 1; i < Dimensions.Length; i++) {
				if (dimensions[i] is null)
					throw new ArgumentException(nameof(dimensions) + " can't contain nulls.");

				ThrowIfDimensionIndexMismatch(index: i, dimensions: dimensions);

				ThrowIfDimensionTypeMismatch(
					index: i,
					dimensions: dimensions,
					dimensionTypes: dimensionTypes);
			}
		}

		private void ThrowIfDimensionIndexMismatch(int index, Array<IDimensionInterval> dimensions) {
			if (Dimensions[index].DimensionIndex != index) {
				throw new ArgumentException(nameof(dimensions) + $" contains items whose " +
					$"{nameof(IDimensionInterval.DimensionIndex)} match their position in " +
					$"the {nameof(Array)}.");
			}
		}

		private void ThrowIfDimensionTypeMismatch(
			int index,
			Array<IDimensionInterval> dimensions,
			Array<FeatureType> dimensionTypes
			) {

			switch (dimensionTypes[index]) {

			case FeatureType.Categorical:
			if (dimensions[index] as CategoricalDimensionInterval is null) {
				throw new ArgumentException(
					$"Mismatch between IDimensionInterval at position {index} and " +
					$"{nameof(dimensionTypes)} at position {index}.");
			}
			break;

			case FeatureType.Continuous:
			if (dimensions[index] as ContinuousDimensionInterval is null) {
				throw new ArgumentException(
					$"Mismatch between IDimensionInterval at position {index} and " +
					$"{nameof(dimensionTypes)} at position {index}.");
			}
			break;

			default:
			throw new ArgumentException(
				$"Unknown {nameof(FeatureType)} stored at position {index}" +
				$"of {nameof(dimensionTypes)}");
			}
		}
	}
}
