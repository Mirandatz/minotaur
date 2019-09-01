namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;
	using Minotaur.Random;
	using Random = Random.ThreadStaticRandom;

	public sealed class TestCreator {

		public readonly Dataset Dataset;

		public TestCreator(Dataset dataset) {
			Dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
		}

		// @Remarks: any IFeatureTests created by this method
		// will use feature values _from the dataset_, instead of random values.
		// That means that values that appear more often in the dataset
		// have a higher chance of being used.
		public IFeatureTest FromDimensionInterval(IDimensionInterval dimensionInterval) {
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

		// @Remarks: Any CategoricalFeatureTest created by this method
		// will use feature values _from the dataset_ as "target values" for the test.
		// That means that values that appear more often in the dataset
		// have a higher chance of being used. 
		private CategoricalFeatureTest FromCategorical(CategoricalDimensionInterval cat) {
			var featureIndex = cat.DimensionIndex;

			var possibleValues = cat.SortedValues;
			var weights = new int[possibleValues.Length];

			for (int i = 0; i < weights.Length; i++) {
				var frequency = Dataset.GetFeatureValueFrequency(
					featureIndex: featureIndex,
					featureValue: possibleValues[i]);

				weights[i] = frequency;
			}

			var chooser = BiasedOptionChooser<float>.Create(
				options: possibleValues,
				weights: weights);

			var value = chooser.GetRandomChoice();

			return new CategoricalFeatureTest(
				featureIndex: cat.DimensionIndex,
				value: value);
		}

		// @Remarks: Any ContinuousFeatureTest created by this method
		// will use feature values _from the dataset_ as bounds.
		// That means that values that appear more often in the dataset
		// have a higher chance	of being used as a bound.
		private ContinuousFeatureTest FromContinuous(ContinuousDimensionInterval cont) {
			var possibleValues = Dataset.GetSortedFeatureValues(cont.DimensionIndex);

			var startValue = cont.Start.Value;
			var startValueIndex = float.IsNegativeInfinity(startValue)
				? 0
				: possibleValues.BinarySearch(startValue);
			if (startValueIndex < 0)
				throw new InvalidOperationException();

			var endValue = cont.End.Value;
			var endValueIndex = float.IsPositiveInfinity(endValue)
				? possibleValues.Length - 1
				: possibleValues.BinarySearch(endValue);
			if (endValueIndex < 0)
				throw new InvalidOperationException();

			var firstBoundIndex = Random.Int(
				inclusiveMin: startValueIndex,
				exclusiveMax: endValueIndex + 1);
			var firstBoundValue = possibleValues[firstBoundIndex];

			var secondBoundIndex = Random.Int(
				inclusiveMin: startValueIndex,
				exclusiveMax: endValueIndex + 1);
			var secondBoundValue = possibleValues[secondBoundIndex];

			return new ContinuousFeatureTest(
				featureIndex: cont.DimensionIndex,
				lowerBound: Math.Min(firstBoundValue, secondBoundValue),
				upperBound: Math.Max(firstBoundValue, secondBoundValue)
				);
		}
	}
}