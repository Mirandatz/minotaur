namespace Minotaur.Math.Geometry {
	using System;
	using System.Diagnostics.CodeAnalysis;

	public sealed class Interval: IEquatable<Interval> {

		public readonly float InclusiveStart;
		public readonly float ExclusiveEnd;

		// Constructors and alike
		public Interval(float inclusiveStart, float exclusiveEnd) {
			if (float.IsNaN(inclusiveStart))
				throw new ArgumentOutOfRangeException(nameof(inclusiveStart) + " can't be NaN.");

			if (float.IsNaN(exclusiveEnd) || float.IsInfinity(exclusiveEnd))
				throw new ArgumentOutOfRangeException(nameof(exclusiveEnd) + " can't be NaN.");

			if (inclusiveStart >= exclusiveEnd)
				throw new ArgumentException(nameof(inclusiveStart) + " must be < " + nameof(exclusiveEnd));

			InclusiveStart = inclusiveStart;
			ExclusiveEnd = exclusiveEnd;
		}

		public static Interval FromUnsortedBounds(float firstBound, float secondBound) {
			if (firstBound < secondBound)
				return new Interval(inclusiveStart: firstBound, exclusiveEnd: secondBound);
			else
				return new Interval(inclusiveStart: secondBound, exclusiveEnd: firstBound);
		}

		// Actual methods
		public bool Contains(float value) {
			if (float.IsNaN(value) || float.IsInfinity(value))
				throw new ArgumentOutOfRangeException(nameof(value) + " must be finite.");

			return InclusiveStart <= value && value < ExclusiveEnd;
		}

		// Silly overrides
		public override string ToString() => $"[{InclusiveStart}, {ExclusiveEnd}[";

		public override int GetHashCode() => HashCode.Combine(InclusiveStart, ExclusiveEnd);

		public override bool Equals(object? obj) => Equals((Interval) obj!);

		// IEquatable
		public bool Equals([AllowNull] Interval other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			return InclusiveStart == other.InclusiveStart &&
				ExclusiveEnd == other.ExclusiveEnd;
		}
	}
}
