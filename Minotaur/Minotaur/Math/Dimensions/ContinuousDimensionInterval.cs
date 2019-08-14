namespace Minotaur.Math.Dimensions {
	using System;

	public sealed class ContinuousDimensionInterval: IDimensionInterval {

		public int DimensionIndex { get; }
		public readonly DimensionBound Start;
		public readonly DimensionBound End;

		public ContinuousDimensionInterval(
			int dimensionIndex,
			DimensionBound start, 
			DimensionBound end
			) {
			if (dimensionIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(dimensionIndex) + " must be >= 0.");

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
