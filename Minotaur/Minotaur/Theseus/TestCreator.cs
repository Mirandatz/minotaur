namespace Minotaur.Theseus {
	using System;
	using System.Collections.Generic;
	using Minotaur.Collections;
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


		// @Improve performance
		// @Remarks: Any ContinuousFeatureTest created by this method
		// will use feature values _from the dataset_ as bounds.
		// That means that values that appear more often in the dataset
		// have a higher chance	of being used as a bound.
		private ContinuousFeatureTest FromContinuous(ContinuousDimensionInterval cont) {
			var dimensionIndex = cont.DimensionIndex;
			var startValue = cont.Start.Value;
			var endValue = cont.End.Value;

			var featureValues = Dataset.GetSortedUniqueFeatureValues(
				featureIndex: dimensionIndex);

			if (float.IsFinite(cont.Start.Value) &&
				featureValues.BinarySearch(startValue) < 0
				) {
				throw new InvalidOperationException(
					$"{nameof(cont)}.{nameof(cont.Start)}.{nameof(cont.Start.Value)} " +
					$"is not contained in the {nameof(Dataset)}.");
			}

			if (float.IsFinite(cont.End.Value) &&
				featureValues.BinarySearch(endValue) < 0
				) {
				throw new InvalidOperationException(
					$"{nameof(cont)}.{nameof(cont.End)}.{nameof(cont.End.Value)} " +
					$"is not contained in the {nameof(Dataset)}.");
			}

			// +2 to account for the case where lower bound is -infinity or
			// the upper bound is +infinity
			var weights = new Dictionary<float, int>(capacity: featureValues.Length + 2);

			if (float.IsNegativeInfinity(startValue))
				weights[float.NegativeInfinity] = 1;
			if (float.IsPositiveInfinity(endValue))
				weights[float.PositiveInfinity] = 1;

			for (int i = 0; i < featureValues.Length; i++) {
				var currentValue = featureValues[i];
				if (currentValue >= startValue && currentValue <= endValue) {
					weights[currentValue] = Dataset.GetFeatureValueFrequency(
						featureIndex: dimensionIndex,
						featureValue: currentValue);
				}
			}

			var firstBoundChooser = BiasedOptionChooser<float>.Create(
				weightedOptions: weights);
			var firstBound = firstBoundChooser.GetRandomChoice();

			weights.Remove(key: firstBound);

			var secondBoundChooser = BiasedOptionChooser<float>.Create(
				weightedOptions: weights);
			var secondBound = secondBoundChooser.GetRandomChoice();

			return ContinuousFeatureTest.FromUnsortedBounds(
				featureIndex: dimensionIndex,
				firstBound: firstBound,
				secondBound: secondBound);
		}

		// @Remarks: Any ContinuousFeatureTest created by this method
		// will use feature values _from the dataset_ as bounds.
		// That means that values that appear more often in the dataset
		// have a higher chance	of being used as a bound.
		private ContinuousFeatureTest FromContinuousFast(ContinuousDimensionInterval cont) {
			var dimensionIndex = cont.DimensionIndex;
			var startValue = cont.Start.Value;
			var endValue = cont.End.Value;
			var possibleValues = Dataset.GetFeatureValues(dimensionIndex);

			var startIndex = float.IsNegativeInfinity(startValue)
				? 0
				: possibleValues.LinearProbeFirstOccurence(startValue);

			var stopIndex = float.IsPositiveInfinity(endValue)
				? possibleValues.Length - 1
				: possibleValues.LinearProbeLastOccurence(endValue);

			var firstBoundIndex = Random.Int(
				inclusiveMin: startIndex,
				exclusiveMax: stopIndex + 1);

			var firstBoundValue = possibleValues[firstBoundIndex];

			throw new NotImplementedException();
		}

	}
}