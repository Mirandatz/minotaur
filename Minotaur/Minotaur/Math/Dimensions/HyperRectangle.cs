namespace Minotaur.Math.Dimensions {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.Collections;

	public sealed class HyperRectangle: IEquatable<HyperRectangle> {

		public readonly Array<IInterval> Dimensions;
		public readonly int DimensionCount;

		private readonly int _precomputedHashCode;

		public HyperRectangle(Array<IInterval> dimensions) {

			var intervalStorage = new IInterval[dimensions.Length];
			var hash = new HashCode();

			// Checking whether the dimensions are not null,
			// and that their dimensions indices match with their positions in the provided array
			// and computing the hashcode ._.
			for (int i = 0; i < dimensions.Length; i++) {
				var d = dimensions[i];

				if (d is null)
					throw new ArgumentException(nameof(dimensions) + " can't contain nulls.");

				if (d.DimensionIndex != i) {
					throw new ArgumentException($"" +
						$"There is a mismatch between {nameof(IInterval.DimensionIndex)}" +
						$"at position {i}.");
				}

				intervalStorage[i] = d;
				hash.Add(d);
			}

			Dimensions = Array<IInterval>.Wrap(intervalStorage);
			DimensionCount = Dimensions.Length;
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

		public IInterval GetDimensionInterval(int dimensionIndex) {
			if (dimensionIndex < 0 || dimensionIndex >= Dimensions.Length)
				throw new ArgumentOutOfRangeException(nameof(dimensionIndex));

			return Dimensions[dimensionIndex];
		}

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object? obj) => Equals((HyperRectangle) obj!);

		public bool Equals([AllowNull] HyperRectangle other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			if (ReferenceEquals(this, other))
				return true;

			if (Dimensions.Length != other.Dimensions.Length)
				throw new InvalidOperationException();

			for (int i = 0; i < DimensionCount; i++) {
				var lhs = Dimensions[i];
				var rhs = other.Dimensions[i];
				if (!lhs.Equals(rhs))
					return false;
			}

			return true;
		}
	}
}