namespace Minotaur.Math.Dimensions {
	using System;
	using Minotaur.ExtensionMethods.Float;

	public sealed class ContinuousDimensionRightExclusiveInterval: IDimensionInterval {
		public int DimensionIndex { get; }
		public readonly float InclusiveStart;
		public readonly float ExclusiveStop;

		public ContinuousDimensionRightExclusiveInterval(int dimensionIndex, float inclusiveStart, float exclusiveStop) {
			if (dimensionIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(dimensionIndex) + " must be >=0.");
			if (inclusiveStart.IsNanOrInfinity())
				throw new ArgumentOutOfRangeException(nameof(inclusiveStart) + " must be finite.");
			if (exclusiveStop.IsNanOrInfinity())
				throw new ArgumentOutOfRangeException(nameof(exclusiveStop) + " must be finite.");

			if (inclusiveStart > exclusiveStop)
				throw new ArgumentOutOfRangeException(nameof(inclusiveStart) + " must be >= " + nameof(exclusiveStop));

			DimensionIndex = dimensionIndex;
			InclusiveStart = inclusiveStart;
			ExclusiveStop = exclusiveStop;
		}

		public bool Contains(float value) {
			if (value.IsNanOrInfinity())
				throw new ArgumentException(nameof(value) + " must be finite.");

			return value >= InclusiveStart && value < ExclusiveStop;
		}
	}
}
