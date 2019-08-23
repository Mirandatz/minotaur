namespace Minotaur.Math.Dimensions {
	using System;
	using Minotaur.Collections;

	public sealed class HyperRectangle: IHyperRectangle {
		public int DimensionCount { get; }
		public Array<IDimensionInterval> Dimensions { get; }

		public HyperRectangle(Array<IDimensionInterval> dimensions) {
			if (dimensions == null)
				throw new ArgumentNullException(nameof(dimensions));

			Dimensions = dimensions.Clone();
			DimensionCount = Dimensions.Length;

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

		public IDimensionInterval GetDimensionInterval(int dimensionIndex) {
			if (dimensionIndex < 0 || dimensionIndex >= Dimensions.Length)
				throw new ArgumentOutOfRangeException(nameof(dimensionIndex));

			return Dimensions[dimensionIndex];
		}

		public bool IsCompatibleWith(IHyperRectangle other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			if (this.DimensionCount != other.DimensionCount)
				return false;

			var dimensionCount = Dimensions.Length;

			for (int i = 0; i < dimensionCount; i++) {
				var lhs = GetDimensionInterval(dimensionIndex: i);
				var rhs = other.GetDimensionInterval(dimensionIndex: i);

				if (lhs.DimensionIndex != rhs.DimensionIndex)
					return false;

				if (lhs.GetType() != rhs.GetType())
					return false;
			}

			return true;
		}
	}
}