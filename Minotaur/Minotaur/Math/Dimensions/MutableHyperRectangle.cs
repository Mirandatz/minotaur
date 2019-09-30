namespace Minotaur.Math.Dimensions {
	using System;
	using Minotaur.Collections;

	public sealed class MutableHyperRectangle {

		public readonly int DimensionCount;
		public readonly bool IsEmpty;

		private readonly IDimensionInterval[] _dimensionIntervals;

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
			var index = dimensionInterval.DimensionIndex;
			if (index < 0 || index >= _dimensionIntervals.Length) {
				throw new ArgumentOutOfRangeException(nameof(dimensionInterval) +
					$" contains a {nameof(dimensionInterval.DimensionIndex)} that " +
					$"is not compatible with this {nameof(MutableHyperRectangle)}.");
			}

			_dimensionIntervals[index] = dimensionInterval;
		}

		public static MutableHyperRectangle FromDatasetInstance(Array<float> seed, Array<FeatureType> featureTypes) {
			if (seed.Length != featureTypes.Length)
				throw new ArgumentException($"{nameof(seed)} and {nameof(featureTypes)} must have the same length.");

			var dimensions = new IDimensionInterval[seed.Length];
			throw new NotImplementedException();

			//for (int i = 0; i < dimensions.Length; i++) {

			//	switch (featureTypes[i]) {

			//	case FeatureType.Categorical: {
			//		var dimensionInterval = CategoricalDimensionInterval.FromSingleValue(
			//			dimensionIndex: i,
			//			value: seed[i]);

			//		dimensions[i] = dimensionInterval;
			//		break;
			//	}

			//	case FeatureType.CategoricalButTriviallyValued: {
			//		var dimensionInterval = CategoricalDimensionInterval.FromSingleValue(
			//			dimensionIndex: i,
			//			value: seed[i]);

			//		dimensions[i] = dimensionInterval;
			//		break;
			//	}

			//	case FeatureType.Continuous: {
			//		var dimensionInterval = ContinuousDimensionInterval.FromSingleValue(
			//			dimensionIndex: i,
			//			value: seed[i]);

			//		dimensions[i] = dimensionInterval;
			//		break;
			//	}

			//	case FeatureType.ContinuousButTriviallyValued: {
			//		var dimensionInterval = ContinuousDimensionInterval.FromSingleValue(
			//			dimensionIndex: i,
			//			value: seed[i]);

			//		dimensions[i] = dimensionInterval;
			//		break;
			//	}

			//	default:
			//	throw new InvalidOperationException(ExceptionMessages.UnknownFeatureType);
			//	}
			//}

			//return new MutableHyperRectangle(dimensions);
		}

		public static MutableHyperRectangle FromHyperRectangle(HyperRectangle rect) {
			var dimensions = rect.Dimensions.ToArray();
			return new MutableHyperRectangle(dimensions);
		}

		public HyperRectangle ToHyperRectangle() => new HyperRectangle(_dimensionIntervals);
	}
}
