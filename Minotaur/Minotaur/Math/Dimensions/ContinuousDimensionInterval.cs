namespace Minotaur.Math.Dimensions {
	using System;
	using System.Text;

	// @Assumption: intervals can not  be empty.
	public sealed class ContinuousDimensionInterval: IDimensionInterval, IEquatable<ContinuousDimensionInterval> {

		public int DimensionIndex { get; }
		public double Volume { get; }
		public readonly DimensionBound Start;
		public readonly DimensionBound End;

		public ContinuousDimensionInterval(int dimensionIndex, DimensionBound start, DimensionBound end) {
			if (dimensionIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(dimensionIndex) + " must be >= 0.");

			if (start.Value > end.Value)
				throw new ArgumentException(nameof(start) + " must be <= " + nameof(end));

			if (start.Value == end.Value) {
				if (start.IsInclusive ^ end.IsInclusive) {
					throw new ArgumentException("Bruh, check your arguments... " +
						$"It doesn't make sense to have " +
						$"{nameof(start)}.{nameof(start.Value)} == " +
						$"{nameof(end)}.{nameof(end.Value)} AND" +
						$"have one of the values inclusive and the other exclusive.");
				}

				// No need to check if !end.IsInclusive,
				// because we already xor'd it with start.IsInclusive
				if (!start.IsInclusive) {
					throw new ArgumentException("It's not possible to create empty continuous intervals.");
				}
			}

			DimensionIndex = dimensionIndex;
			Start = start;
			End = end;
			Volume = End.Value - Start.Value;
		}

		// Constructor for lazy programmers
		public static ContinuousDimensionInterval FromSingleValue(int dimensionIndex, float value) {
			if (dimensionIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(dimensionIndex) + " must be >= 0.");

			if (float.IsNaN(value))
				throw new ArgumentOutOfRangeException(nameof(value) + " can't be NaN.");

			var bound = new DimensionBound(value: value, isInclusive: true);

			return new ContinuousDimensionInterval(
				dimensionIndex: dimensionIndex,
				start: bound,
				end: bound);
		}

		public bool Contains(ContinuousDimensionInterval other) {
			var otherStart = other.Start;
			if (Start.Value > otherStart.Value)
				return false;
			if (Start.Value == otherStart.Value && !Start.IsInclusive)
				return false;

			var otherEnd = other.End;
			if (End.Value < otherEnd.Value)
				return false;
			if (End.Value == otherEnd.Value && !End.IsInclusive)
				return false;

			return true;
		}

		public bool Contains(float value) {
			if (float.IsNaN(value))
				throw new ArgumentOutOfRangeException(nameof(value));

			if (value > Start.Value && value < End.Value)
				return true;
			if (Start.IsInclusive && value == Start.Value)
				return true;
			if (End.IsInclusive && value == End.Value)
				return true;

			return false;
		}

		// Silly overrides
		public override string ToString() {
			var builder = new StringBuilder();
			if (Start.IsInclusive)
				builder.Append('[');
			else
				builder.Append(']');

			builder.Append(Start.Value);
			builder.Append(", ");
			builder.Append(End.Value);

			if (End.IsInclusive)
				builder.Append(']');
			else
				builder.Append('[');

			return builder.ToString();
		}

		public override int GetHashCode() => HashCode.Combine(DimensionIndex, Start, End);

		public override bool Equals(object? obj) {
			if (obj is ContinuousDimensionInterval other)
				return Equals(other);
			else
				return false;
		}

		// Implementation of IEquatable
		public bool Equals(IDimensionInterval dimensionInterval) {
			if (dimensionInterval is ContinuousDimensionInterval other)
				return Equals(other);
			else
				return false;
		}

		public bool Equals(ContinuousDimensionInterval other) {
			return
				DimensionIndex == other.DimensionIndex &&
				Volume == other.Volume &&
				Start.Equals(other.Start) &&
				End.Equals(other.End);
		}
	}
}
