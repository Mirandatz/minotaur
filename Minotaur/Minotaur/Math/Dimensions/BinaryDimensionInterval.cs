namespace Minotaur.Math.Dimensions {
	using System;

	public sealed class BinaryDimensionInterval: IDimensionInterval, IEquatable<BinaryDimensionInterval> {

		public double Volume => 1;
		public int DimensionIndex { get; }
		public readonly bool ContainsFalse;
		public readonly bool ContainsTrue;

		public BinaryDimensionInterval(int dimensionIndex, bool containsFalse, bool containsTrue) {
			if (dimensionIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(dimensionIndex));

			if (!(containsTrue || containsFalse))
				throw new InvalidOperationException("Intervals can't be empty.");

			DimensionIndex = dimensionIndex;
			ContainsTrue = containsTrue;
			ContainsFalse = containsFalse;
		}

		public static BinaryDimensionInterval FromSingleValue(int dimensionIndex, float value) {
			return (value) switch
			{
				0f => new BinaryDimensionInterval(dimensionIndex: dimensionIndex, containsFalse: true, containsTrue: false),
				1f => new BinaryDimensionInterval(dimensionIndex: dimensionIndex, containsFalse: false, containsTrue: true),
				_ => throw new InvalidOperationException($"You can't create a {nameof(BinaryDimensionInterval)} with non-binary values.")
			};
		}

		public bool Contains(float value) {
			if (value != 0 && value != 1)
				throw new InvalidOperationException("Jesus, this should never happen...");

			return (value == 0 && ContainsFalse) || (value == 1 && ContainsTrue);
		}

		// Silly overrides
		public override string ToString() {
			if (ContainsFalse && ContainsTrue)
				return "[0, 1]";

			if (ContainsFalse && !ContainsTrue)
				return "[0]";

			// By now we know that ContainsFalse == false
			// therefore ContainsTrue must be true,
			// because we don't allow empty intervals

			return "[1]";
		}

		public override int GetHashCode() => HashCode.Combine(DimensionIndex, ContainsFalse, ContainsTrue);

		public override bool Equals(object? obj) => Equals((BinaryDimensionInterval) obj!);

		public bool Equals(IDimensionInterval other) => Equals((BinaryDimensionInterval) other);

		public bool Equals(BinaryDimensionInterval other) {
			return
				DimensionIndex == other.DimensionIndex &&
				ContainsFalse == other.ContainsFalse &&
				ContainsTrue == other.ContainsTrue;
		}
	}
}
