namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;
	using Minotaur.Collections;
	using Random = Random.ThreadStaticRandom;
	using Minotaur.Random;

	public sealed class TestCreator {

		public Dataset Dataset;

		public IFeatureTest TryCreate(IDimensionInterval dimensionInterval) {
			if (dimensionInterval is null)
				throw new ArgumentNullException(nameof(dimensionInterval));
			if (!Dataset.IsDimesionIntervalValid(dimensionInterval))
				throw new ArgumentOutOfRangeException(nameof(dimensionInterval));

			switch (dimensionInterval) {

			case CategoricalDimensionInterval cat:
			return FromCategorical(cat);

			case ContinuousDimensionInterval cont:
			return FromContinuous(cont);

			default:
			throw new InvalidOperationException(
				$"Unknown / unsupported implementation of {nameof(IDimensionInterval)}.");
			}
		}

		private CategoricalFeatureTest FromCategorical(CategoricalDimensionInterval cat) {
			var featureIndex = cat.DimensionIndex;

			var possibleValues = cat.SortedValues;
			var weights = new float[possibleValues.Length];

			var scalingFactor = Dataset.InstanceCount;
			for (int i = 0; i < weights.Length; i++) {
				var frequency = Dataset.GetFeatureValueFrequency(
					featureIndex: featureIndex,
					featureValue: possibleValues[i]);

				var weight = ((float) frequency) / scalingFactor;
				weights[i] = weight;
			}

			throw new NotImplementedException();
		}

		// @Remark: ContinuousFeatureTests created by this method
		// use feature values _from the dataset_ as bounds
		// Values that appear more often in the dataset have a higher chance
		// of being used as a bound
		private ContinuousFeatureTest FromContinuous(ContinuousDimensionInterval cont) {
			var possibleValues = Dataset.GetSortedFeatureValues(cont.DimensionIndex);

			var startValue = cont.Start.Value;
			var indexOfStartValue = possibleValues.BinarySearch(startValue);
			if (indexOfStartValue < 0)
				throw new InvalidOperationException();

			var endValue = cont.End.Value;
			var indexOfEndValue = possibleValues.BinarySearch(endValue);
			if (indexOfEndValue < 0)
				throw new InvalidOperationException();

			var indexOfFirstBound = Random.Int(
				inclusiveMin: indexOfStartValue,
				exclusiveMax: indexOfEndValue + 1);
			var firstBound = possibleValues[indexOfFirstBound];

			var indexOfSecondBound = Random.Int(
				inclusiveMin: indexOfStartValue,
				exclusiveMax: indexOfEndValue + 1);
			var secondBound = possibleValues[indexOfSecondBound];

			return new ContinuousFeatureTest(
				featureIndex: cont.DimensionIndex,
				lowerBound: Math.Min(firstBound, secondBound),
				upperBound: Math.Max(firstBound, secondBound)
				);
		}
	}
}