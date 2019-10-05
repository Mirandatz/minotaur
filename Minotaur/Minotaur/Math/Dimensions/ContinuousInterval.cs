namespace Minotaur.Math.Dimensions {
	using System;

	// @Assumption: intervals can not  be empty.
	public sealed class ContinuousInterval: IInterval, IEquatable<ContinuousInterval> {

		public int DimensionIndex { get; }
		public double Volume => End - Start;
		public readonly float Start;
		public readonly float End;

		public ContinuousInterval(int dimensionIndex, float start, float end) {
			if (dimensionIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(dimensionIndex) + " must be >= 0.");

			if (start >= end)
				throw new ArgumentException(nameof(start) + " must be < " + nameof(end));

			DimensionIndex = dimensionIndex;
			Start = start;
			End = end;
		}

		public bool Contains(float value) => Start <= value && value < End;

		// Silly overrides
		public override string ToString() => $"[{Start}, {End}[";

		public override int GetHashCode() => HashCode.Combine(DimensionIndex, Start, End);

		public override bool Equals(object? obj) => Equals((ContinuousInterval) obj!);

		// Implementation of IEquatable
		public bool Equals(IInterval other) => Equals((ContinuousInterval) other);

		public bool Equals(ContinuousInterval other) {
			return
				DimensionIndex == other.DimensionIndex &&
				Start == other.Start &&
				End == other.End;
		}
	}
}
