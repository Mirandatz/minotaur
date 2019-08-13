namespace Minotaur.Math.Dimensions {
	using System;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class DimensionIntervalCreator {
		private readonly Dataset _dataset;

		public DimensionIntervalCreator(Dataset dataset) {
			_dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
		}

		public IDimensionInterval FromFeatureTest(IFeatureTest test) {
			if (test is null)
				throw new ArgumentNullException(nameof(test));

			switch (test) {
			case NullFeatureTest nft:
			return FromNullFeatureTest(nft);

			case CategoricalFeatureTest categorical:
			return FromCategoricalFeatureTest(categorical);

			case ContinuousFeatureTest continuous:
			return FromContinuousFeatureTest(continuous);

			default:
			throw new InvalidOperationException($"Unknown {nameof(IFeatureTest)} type.");
			}
		}

		private IDimensionInterval FromNullFeatureTest(NullFeatureTest nullFeatureTest) {
			var featureIndex = nullFeatureTest.FeatureIndex;

			if (featureIndex < 0)
				throw new InvalidOperationException(nameof(nullFeatureTest) +
					" should never have been created with a negative "
					+ nameof(nullFeatureTest.FeatureIndex) +
					".");

			if (featureIndex >= _dataset.FeatureCount)
				throw new InvalidOperationException(nameof(nullFeatureTest) +
					" should never have been created with a " + nameof(nullFeatureTest.FeatureIndex) +
					" greater than the dataset's " +
					nameof(_dataset.FeatureCount) +
					".");

			var featureType = _dataset.GetFeatureType(featureIndex);
			var featureValues = _dataset.GetSortedUniqueFeatureValues(featureIndex);

			switch (featureType) {

			// @Improve performance
			case FeatureType.Categorical:
			return new CategoricalDimensionInterval(
				dimensionIndex: featureIndex,
				values: featureValues.ToArray());

			case FeatureType.Continuous:
			var min = featureValues[0];
			var max = featureValues[featureValues.Length - 1];
			return new ContinuousDimensionRightInclusiveInterval(
				dimensionIndex: featureIndex,
				inclusiveStart: min,
				inclusiveStop: max);

			default:
			throw new InvalidOperationException($"Unknown {nameof(FeatureType)} type.");
			}
		}

		private IDimensionInterval FromContinuousFeatureTest(ContinuousFeatureTest continuous) {
			return new ContinuousDimensionRightExclusiveInterval(
				dimensionIndex: continuous.FeatureIndex,
				inclusiveStart: continuous.LowerBound,
				exclusiveStop: continuous.UpperBound);
		}

		private IDimensionInterval FromCategoricalFeatureTest(CategoricalFeatureTest categorical) {
			return new CategoricalDimensionInterval(
				dimensionIndex: categorical.FeatureIndex,
				values: new float[] { categorical.Value });
		}
	}
}