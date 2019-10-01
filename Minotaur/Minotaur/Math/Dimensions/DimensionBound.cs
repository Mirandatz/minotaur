namespace Minotaur.Math.Dimensions {
	using System;

	public readonly struct DimensionBound: IEquatable<DimensionBound> {
		public readonly float Value;
		public readonly bool IsInclusive;

		public DimensionBound(float value, bool isInclusive) {
			if (float.IsNaN(value))
				throw new ArgumentOutOfRangeException(nameof(value) + " can't be NaN.");

			Value = value;
			IsInclusive = isInclusive;
		}

		/// <remarks>
		/// All continuous interval "stars" are inclusive.
		/// </remarks>
		public static DimensionBound CreateStart(float value) {
			if (float.IsNaN(value))
				throw new ArgumentOutOfRangeException(nameof(value) + " can't be NaN.");

			return new DimensionBound(
				value: value,
				isInclusive: true);
		}

		/// <remarks>
		/// All continuous interval "ends" are exclusive.
		/// </remarks>
		public static DimensionBound CreateEnd(float value) {
			if (float.IsNaN(value))
				throw new ArgumentOutOfRangeException(nameof(value) + " can't be NaN.");

			return new DimensionBound(
				value: value,
				isInclusive: false);
		}

		// Silly overrides
		public override string ToString() {
			if (IsInclusive)
				return $"{Value}, Inclusive";
			else
				return $"{Value}, Exclusive";
		}

		public override int GetHashCode() => HashCode.Combine(Value, IsInclusive);

		public override bool Equals(object? obj) {
			if (obj is DimensionBound other)
				return Equals(other);
			else
				return false;
		}

		public bool Equals(DimensionBound other) {
			return
				Value == other.Value &&
				IsInclusive == other.IsInclusive;
		}
	}
}
