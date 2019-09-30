namespace Minotaur.Math.Dimensions {
	using System;

	public sealed class BinaryDimensionInterval: IDimensionInterval, IEquatable<BinaryDimensionInterval> {

		public double Volume => 1;
		public int DimensionIndex { get; }
		public readonly float Value;

		public BinaryDimensionInterval(int dimensionIndex, float value) {
			if (dimensionIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(dimensionIndex));
			if (value != 0 && value != 1)
				throw new ArgumentOutOfRangeException(nameof(value));

			DimensionIndex = dimensionIndex;
			Value = value;
		}

		public bool Contains(float value) => value == Value;

		// Silly overrides
		public override string ToString() => $"[{Value}]";

		public override int GetHashCode() => HashCode.Combine(DimensionIndex, Value);

		public override bool Equals(object? obj) {
			if (obj is BinaryDimensionInterval bdi)
				return Equals(bdi);
			else
				return false;
		}

		public bool Equals(IDimensionInterval other) {
			if (other is BinaryDimensionInterval bdi)
				return Equals(bdi);
			else
				return false;
		}

		public bool Equals(BinaryDimensionInterval other) {
			return
				DimensionIndex == other.DimensionIndex &&
				Value == other.Value;
		}
	}
}
