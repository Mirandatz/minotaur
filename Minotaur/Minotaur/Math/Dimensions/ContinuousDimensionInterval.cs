namespace Minotaur.Math.Dimensions {
	using System;

	public sealed class ContinuousDimensionInterval: IDimensionInterval {

		public int DimensionIndex { get; }
		public bool IsEmpty { get; }
		public readonly DimensionBound Start;
		public readonly DimensionBound End;

		public ContinuousDimensionInterval(
			int dimensionIndex,
			DimensionBound start,
			DimensionBound end
			) {
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

				IsEmpty = !start.IsInclusive;
			}

			IsEmpty = false;

			DimensionIndex = dimensionIndex;
			Start = start;
			End = end;
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
	}
}
