namespace Minotaur.Math.Geometry {
	using System;
	using System.Diagnostics.CodeAnalysis;

	public sealed class Interval: IEquatable<Interval> {

		public readonly float InclusiveStart;
		public readonly float ExclusiveEnd;

		public Interval(float inclusiveStart, float exclusiveEnd) {
			if (float.IsNaN(inclusiveStart) || float.IsInfinity(inclusiveStart))
				throw new ArgumentOutOfRangeException(nameof(inclusiveStart) + " must be finite.");
			if (float.IsNaN(exclusiveEnd) || float.IsInfinity(exclusiveEnd))
				throw new ArgumentOutOfRangeException(nameof(exclusiveEnd) + " must be finite.");

			if (inclusiveStart >= exclusiveEnd)
				throw new ArgumentException(nameof(inclusiveStart) + " must be < " + nameof(exclusiveEnd));

			InclusiveStart = inclusiveStart;
			ExclusiveEnd = exclusiveEnd;
		}

		public bool Contains(float value) {
			if (float.IsNaN(value) || float.IsInfinity(value))
				throw new ArgumentOutOfRangeException(nameof(value) + " must be finite.");

			return InclusiveStart <= value && value < ExclusiveEnd;
		}

		// Silly overrides
		public override string ToString() => $"[{InclusiveStart}, {ExclusiveEnd}[";

		public override int GetHashCode() => HashCode.Combine(InclusiveStart, ExclusiveEnd);

		public override bool Equals(object? obj) => Equals((Interval) obj!);

		public bool Equals([AllowNull] Interval other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			return InclusiveStart == other.InclusiveStart &&
				ExclusiveEnd == other.ExclusiveEnd;
		}
	}
}
