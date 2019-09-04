namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;

	public sealed class DimensionIntervalCreator {
		public readonly Dataset Dataset;

		public DimensionIntervalCreator(Dataset dataset) {
			Dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
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
			if (!Dataset.IsFeatureIndexValid(featureIndex))
				throw new ArgumentOutOfRangeException($"{nameof(nullFeatureTest)}.{nameof(nullFeatureTest.FeatureIndex)} is invalid.");

			var featureType = Dataset.GetFeatureType(featureIndex);
			var featureValues = Dataset.GetSortedUniqueFeatureValues(featureIndex);

			switch (featureType) {

			// @Improve performance
			case FeatureType.Categorical:
			return new CategoricalDimensionInterval(
				dimensionIndex: featureIndex,
				values: featureValues.ToArray());

			case FeatureType.Continuous:
			var min = featureValues[0];
			var max = featureValues[featureValues.Length - 1];
			return new ContinuousDimensionInterval(
				dimensionIndex: featureIndex,
				start: new DimensionBound(value: min, isInclusive: true),
				end: new DimensionBound(value: max, isInclusive: true));

			default:
			throw new InvalidOperationException($"Unknown {nameof(FeatureType)} type.");
			}
		}

		private IDimensionInterval FromContinuousFeatureTest(ContinuousFeatureTest continuous) {
			return new ContinuousDimensionInterval(
				dimensionIndex: continuous.FeatureIndex,
				start: new DimensionBound(value: continuous.LowerBound, isInclusive: true),
				end: new DimensionBound(value: continuous.UpperBound, isInclusive: false));
		}

		private IDimensionInterval FromCategoricalFeatureTest(CategoricalFeatureTest categorical) {
			return new CategoricalDimensionInterval(
				dimensionIndex: categorical.FeatureIndex,
				values: new float[] { categorical.Value });
		}
	}
}