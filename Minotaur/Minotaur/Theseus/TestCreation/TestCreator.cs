namespace Minotaur.Theseus.TestCreation {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;
	using Random = Random.ThreadStaticRandom;

	public sealed class TestCreator: ITestCreator {

		public Dataset Dataset { get; }

		public TestCreator(Dataset dataset) {
			Dataset = dataset;
		}

		public IFeatureTest FromDimensionInterval(IDimensionInterval dimensionInterval) {
			if (!Dataset.IsFeatureIndexValid(dimensionInterval.DimensionIndex))
				throw new InvalidOperationException(nameof(dimensionInterval) + nameof(dimensionInterval.DimensionIndex) + " is invalid.");

			return dimensionInterval switch
			{
				BinaryDimensionInterval bdi => FromBinary(bdi),
				ContinuousDimensionInterval cdi => FromContinuous(cdi),

				_ => throw CommonExceptions.UnknownDimensionIntervalImplementation
			};
		}

		private BinaryFeatureTest FromBinary(BinaryDimensionInterval bdi) {

			float testValue;
			if (bdi.ContainsFalse && bdi.ContainsTrue) {
				if (Random.Bool())
					testValue = 1f;
				else
					testValue = 0f;
			} else {
				if (bdi.ContainsFalse)
					testValue = 0f;
				else
					testValue = 1f;
			}

			return new BinaryFeatureTest(
				featureIndex: bdi.DimensionIndex,
				value: testValue);
		}

		private ContinuousFeatureTest FromContinuous(ContinuousDimensionInterval cdi) {
			var featureIndex = cdi.DimensionIndex;
			var featureValues = Dataset.GetSortedUniqueFeatureValues(featureIndex);
			
			var lowerBound = GetLowerBound(start: cdi.Start, sortedUniqueFeatureValues: featureValues);
			var upperBound = GetUpperBound(end: cdi.End, sortedUniqueFeatureValues: featureValues);

			return new ContinuousFeatureTest(
				featureIndex: featureIndex,
				lowerBound: lowerBound,
				upperBound: upperBound);
		}

		private static float GetLowerBound(DimensionBound start, Array<float> sortedUniqueFeatureValues) {
			var startValue = start.Value;
			var isInclusive = start.IsInclusive;
			var indexOfStart = sortedUniqueFeatureValues.BinarySearch(startValue);

			if (indexOfStart < 0)
				throw new InvalidOperationException();

			if (!isInclusive) {
				return sortedUniqueFeatureValues[indexOfStart + 1];
			} else {
				if (indexOfStart == 0)
					return float.NegativeInfinity;
				else
					return startValue;
			}
		}

		private static float GetUpperBound(DimensionBound end, Array<float> sortedUniqueFeatureValues) {
			var endValue = end.Value;
			var isInclusive = end.IsInclusive;
			var indexOfEnd = sortedUniqueFeatureValues.BinarySearch(endValue);

			if (indexOfEnd < 0)
				throw new InvalidOperationException();

			if (!isInclusive) {
				return sortedUniqueFeatureValues[indexOfEnd - 1];
			} else {
				if (indexOfEnd == sortedUniqueFeatureValues.Length - 1)
					return float.PositiveInfinity;
				else
					return endValue;
			}
		}
	}
}