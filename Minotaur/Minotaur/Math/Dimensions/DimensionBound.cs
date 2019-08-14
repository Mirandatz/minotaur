namespace Minotaur.Math.Dimensions {
	using System;

	public readonly struct DimensionBound {
		public readonly float Value;
		public readonly bool IsInclusive;

		public DimensionBound(float value, bool isInclusive) {
			if (float.IsNaN(value))
				throw new ArgumentOutOfRangeException(nameof(value) + " can't be NaN.");

			Value = value;
			IsInclusive = isInclusive;
		}
	}
}
