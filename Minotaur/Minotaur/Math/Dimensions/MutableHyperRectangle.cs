namespace Minotaur.Math.Dimensions {
	using System;
	using Minotaur.Collections;

	public sealed class MutableHyperRectangle: IHyperRectangle {

		private readonly IDimensionInterval[] _dimensionIntervals;

		public int DimensionCount { get; }
		public bool IsEmpty { get; }

		private MutableHyperRectangle(IDimensionInterval[] dimensionIntervals) {
			_dimensionIntervals = dimensionIntervals;
			DimensionCount = _dimensionIntervals.Length;
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

		public static MutableHyperRectangle FromDatasetInstance(Array<float> seed, Array<FeatureType> featureTypes) {
			if (seed is null)
				throw new ArgumentNullException(nameof(seed));
			if (featureTypes is null)
				throw new ArgumentNullException(nameof(featureTypes));
			if (seed.Length != featureTypes.Length)
				throw new ArgumentException($"{nameof(seed)} and {nameof(featureTypes)} must have the same length.");

			var dimensions = new IDimensionInterval[seed.Length];

			for (int i = 0; i < dimensions.Length; i++) {

				switch (featureTypes[i]) {

				case FeatureType.Categorical: {
					var dimensionInterval = CategoricalDimensionInterval.FromSingleValue(
						dimensionIndex: i,
						value: seed[i]);

					dimensions[i] = dimensionInterval;
					break;
				}

				case FeatureType.CategoricalButTriviallyValued: {
					var dimensionInterval = CategoricalDimensionInterval.FromSingleValue(
						dimensionIndex: i,
						value: seed[i]);

					dimensions[i] = dimensionInterval;
					break;
				}

				case FeatureType.Continuous: {
					var dimensionInterval = ContinuousDimensionInterval.FromSingleValue(
						dimensionIndex: i,
						value: seed[i]);

					dimensions[i] = dimensionInterval;
					break;
				}

				case FeatureType.ContinuousButTriviallyValued: {
					var dimensionInterval = ContinuousDimensionInterval.FromSingleValue(
						dimensionIndex: i,
						value: seed[i]);

					dimensions[i] = dimensionInterval;
					break;
				}

				default:
				throw new InvalidOperationException($"Unknown / unsupported value of {nameof(FeatureType)}.");
				}
			}

			return new MutableHyperRectangle(dimensions);
		}

		public static MutableHyperRectangle FromHyperRectangle(HyperRectangle rect) {
			if (rect is null)
				throw new ArgumentNullException(nameof(rect));

			var dimensions = rect.Dimensions.ToArray();
			return new MutableHyperRectangle(dimensions);
		}

		public HyperRectangle ToHyperRectangle() => new HyperRectangle(_dimensionIntervals);

		public bool IsCompatibleWith(IHyperRectangle other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			if (this.DimensionCount != other.DimensionCount)
				return false;

			var dimensionCount = _dimensionIntervals.Length;

			for (int i = 0; i < dimensionCount; i++) {
				var lhs = GetDimensionInterval(dimensionIndex: i);
				var rhs = other.GetDimensionInterval(dimensionIndex: i);

				if (lhs.DimensionIndex != rhs.DimensionIndex)
					return false;

				if (lhs.GetType() != rhs.GetType())
					return false;
			}

			return true;
		}
	}
}
