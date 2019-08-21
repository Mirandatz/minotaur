namespace Minotaur.Math.Dimensions {
	using System;

	public sealed class MutableHyperRectangle {

		private readonly IDimensionInterval[] _dimensionIntervals;

		private MutableHyperRectangle(IDimensionInterval[] dimensionIntervals) {
			_dimensionIntervals = dimensionIntervals;
		}

		public IDimensionInterval GetDimensionInterval(int dimensionIndex) {
			if (dimensionIndex < 0 || dimensionIndex >= _dimensionIntervals.Length)
				throw new ArgumentOutOfRangeException(nameof(dimensionIndex));

			return _dimensionIntervals[dimensionIndex];
		}

		public void SetDimensionInterval(IDimensionInterval dimensionInterval) {
			if (dimensionInterval is null)
				throw new ArgumentNullException(nameof(dimensionInterval));

			var index = dimensionInterval.DimensionIndex;
			if (index < 0 || index >= _dimensionIntervals.Length) {
				throw new ArgumentOutOfRangeException(nameof(dimensionInterval) +
					$" contains a {nameof(dimensionInterval.DimensionIndex)} that " +
					$"is not compatible with this {nameof(MutableHyperRectangle)}.");
			}

			_dimensionIntervals[index] = dimensionInterval;
		}

		public static MutableHyperRectangle FromHyperRectangle(HyperRectangle rect) {
			if (rect is null)
				throw new ArgumentNullException(nameof(rect));

			var dimensions = rect.Dimensions.ToArray();
			return new MutableHyperRectangle(dimensions);
		}

		public HyperRectangle ToHyperRectangle() => new HyperRectangle(_dimensionIntervals);
	}
}
