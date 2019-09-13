namespace Minotaur.Theseus.TestCreation {
	using System;
	using System.Linq;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;
	using Minotaur.Random;
	using Random = Random.ThreadStaticRandom;

	public sealed class TestCreator: ITestCreator {

		public Dataset Dataset { get; }

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

		private ContinuousFeatureTest FromContinuous(ContinuousDimensionInterval cont) {
			var dimensionIndex = cont.DimensionIndex;
			var startValue = cont.Start.Value;
			var endValue = cont.End.Value;

			var featureValues = Dataset.GetSortedUniqueFeatureValues(dimensionIndex);
			var possibleValues = featureValues
				.Where(v => v >= startValue && v <= endValue)
				.ToArray();

			var firstBound = Random.Choice(possibleValues);

			possibleValues = possibleValues.Where(v => v != firstBound).ToArray();

			var secondBound = Random.Choice(possibleValues);

			return ContinuousFeatureTest.FromUnsortedBounds(
				featureIndex: dimensionIndex,
				firstBound: firstBound,
				secondBound: secondBound);
		}

		private static ContinuousFeatureTest FirstBoundIsLargestValue(
			int dimensionIndex,
			Array<float> featureValues,
			int startIndex,
			int firstBoundIndex
			) {
			var firstBoundValue = featureValues[firstBoundIndex];
			var firstOccurence = featureValues.BinarySearchFirstOccurence(firstBoundValue);

			var secondBoundIndex = Random.Int(
				inclusiveMin: startIndex,
				exclusiveMax: firstOccurence);

			var secondBoundValue = featureValues[secondBoundIndex];

			return ContinuousFeatureTest.FromUnsortedBounds(
				featureIndex: dimensionIndex,
				firstBound: firstBoundValue,
				secondBound: secondBoundValue);
		}

		private static ContinuousFeatureTest FirstBoundIsSmallestValue(
			int dimensionIndex,
			Array<float> featureValues,
			int stopIndex,
			int firstBoundIndex
			) {
			var firstBoundValue = featureValues[firstBoundIndex];
			var lastOccurence = featureValues.BinarySearchLastOccurence(firstBoundValue);

			var secondBoundIndex = Random.Int(
				inclusiveMin: lastOccurence + 1,
				exclusiveMax: stopIndex + 1);

			var secondBoundValue = featureValues[secondBoundIndex];

			return ContinuousFeatureTest.FromUnsortedBounds(
				featureIndex: dimensionIndex,
				firstBound: firstBoundValue,
				secondBound: secondBoundValue);
		}
	}
}