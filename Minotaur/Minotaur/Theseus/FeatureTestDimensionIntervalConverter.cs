namespace Minotaur.Theseus {
	using System;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;

	public sealed class FeatureTestDimensionIntervalConverter {

		public BinaryDimensionInterval FromBinaryFeatureTest(BinaryFeatureTest binaryFeatureTest) {
			if (binaryFeatureTest.Value == 0) {
				return new BinaryDimensionInterval(
					dimensionIndex: binaryFeatureTest.FeatureIndex,
					containsFalse: true,
					containsTrue: false);
			}

			if (binaryFeatureTest.Value == 1) {
				return new BinaryDimensionInterval(
					dimensionIndex: binaryFeatureTest.FeatureIndex,
					containsFalse: false,
					containsTrue: true);
			}

			throw new InvalidOperationException();
		}

		public ContinuousDimensionInterval FromContinuousFeatureTest(ContinuousFeatureTest continuousFeatureTest) {
			return new ContinuousDimensionInterval(
				dimensionIndex: continuousFeatureTest.FeatureIndex,
				start: continuousFeatureTest.LowerBound,
				end: continuousFeatureTest.UpperBound);
		}

		public BinaryFeatureTest FromBinaryDimensionInterval(BinaryDimensionInterval binaryDimensionInterval) {
			if (binaryDimensionInterval.ContainsFalse && binaryDimensionInterval.ContainsTrue)
				throw new ArgumentException();

			if (binaryDimensionInterval.ContainsFalse)
				return new BinaryFeatureTest(featureIndex: binaryDimensionInterval.DimensionIndex, value: 0f);
			else
				return new BinaryFeatureTest(featureIndex: binaryDimensionInterval.DimensionIndex, value: 1f);
		}

		public ContinuousFeatureTest FromContinousDimensionInterval(ContinuousDimensionInterval continuousDimensionInterval) {
			return new ContinuousFeatureTest(
				featureIndex: continuousDimensionInterval.DimensionIndex,
				lowerBound: continuousDimensionInterval.Start,
				upperBound: continuousDimensionInterval.End);
		}
	}
}
