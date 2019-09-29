namespace Minotaur.Math.Dimensions {
	using System;
	using Minotaur.Collections;

	public sealed class HyperRectangle: IHyperRectangle, IEquatable<HyperRectangle> {
		public int DimensionCount { get; }
		public Array<IDimensionInterval> Dimensions { get; }

		private readonly int _precomputedHashCode;

		public HyperRectangle(Array<IDimensionInterval> dimensions) {
			Dimensions = dimensions.Clone();
			DimensionCount = Dimensions.Length;

			var hash = new HashCode();

			// Checking whether the dimensions are not null,
			// and that their dimensions indices match with their positions in the provided array
			// and computing the hashcode ._.
			for (int i = 0; i < dimensions.Length; i++) {
				var dimension = dimensions[i];

				if (dimension is null)
					throw new ArgumentException(nameof(dimension) + " can't contain nulls.");

				if (dimension.DimensionIndex != i) {
					throw new ArgumentException($"" +
						$"There is a mismatch between {nameof(IDimensionInterval.DimensionIndex)}" +
						$"at position {i}.");
				}

				hash.Add(dimension);
			}

			_precomputedHashCode = hash.ToHashCode();
		}

		public bool Contains(Array<float> point) {
			if (point.Length != DimensionCount) {
				throw new ArgumentException(
					nameof(point) + " must contain the same number of dimensions as this " +
					nameof(HyperRectangle));
			}

			for (int i = 0; i < DimensionCount; i++) {
				if (!Dimensions[i].Contains(point[i]))
					return false;
			}

			return true;
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

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object? obj) {
			if (obj is HyperRectangle other)
				return Equals(other);
			else
				return false;
		}

		public bool Equals(HyperRectangle other) {
			// @Danger, this might not work
			return ReferenceEquals(this, other);
		}
	}
}