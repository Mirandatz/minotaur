namespace Minotaur.Math.Geometry {
	using System;
	using System.Diagnostics.CodeAnalysis;

	public sealed class Interval: IEquatable<Interval> {

		public readonly float InclusiveStart;
		public readonly float ExclusiveEnd;

		public Interval(float start, float end) {
			if (float.IsNaN(start))
				throw new ArgumentOutOfRangeException(nameof(start) + " can't be NaN.");
			if (float.IsNaN(end))
				throw new ArgumentOutOfRangeException(nameof(end) + " can't be NaN.");

			if (start >= end)
				throw new ArgumentException(nameof(start) + " must be < " + nameof(end));

			InclusiveStart = start;
			ExclusiveEnd = end;
		}

		public bool Contains(float value) {
			if (float.IsNaN(value))
				throw new ArgumentOutOfRangeException(nameof(value) + " can't be NaN.");

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
