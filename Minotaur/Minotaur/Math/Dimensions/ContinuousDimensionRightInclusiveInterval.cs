namespace Minotaur.Math.Dimensions {
	using System;
	using Minotaur.ExtensionMethods.Float;

	public sealed class ContinuousDimensionRightInclusiveInterval: IDimensionInterval {
		public int DimensionIndex { get; }
		public readonly float InclusiveStart;
		public readonly float InclusiveStop;

		public ContinuousDimensionRightInclusiveInterval(int dimensionIndex, float inclusiveStart, float inclusiveStop) {
			if (dimensionIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(dimensionIndex) + " must be >=0.");
			if (inclusiveStart.IsNanOrInfinity())
				throw new ArgumentOutOfRangeException(nameof(inclusiveStart) + " must be finite.");
			if (inclusiveStop.IsNanOrInfinity())
				throw new ArgumentOutOfRangeException(nameof(inclusiveStop) + " must be finite.");

			if (inclusiveStart > inclusiveStop)
				throw new ArgumentOutOfRangeException(nameof(inclusiveStart) + " must be >= " + nameof(inclusiveStop));

			DimensionIndex = dimensionIndex;
			InclusiveStart = inclusiveStart;
			InclusiveStop = inclusiveStop;
		}

		public bool Contains(float value) {
			if (value.IsNanOrInfinity())
				throw new ArgumentException(nameof(value) + " must be finite.");

			return value >= InclusiveStart && value <= InclusiveStop;
		}
	}
}
