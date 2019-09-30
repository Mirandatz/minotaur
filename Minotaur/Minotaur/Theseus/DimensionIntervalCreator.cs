namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;

	public sealed class DimensionIntervalCreator {
		public readonly Dataset Dataset;

		public DimensionIntervalCreator(Dataset dataset) {
			Dataset = dataset;
		}

		public IDimensionInterval FromFeatureTest(IFeatureTest test) {
			return test switch
			{
				NullFeatureTest nft => FromNullFeatureTest(nft),
				BinaryFeatureTest bft => FromBinaryFeatureTest(bft),
				ContinuousFeatureTest cft => FromContinuousFeatureTest(cft),

				_ => throw new InvalidOperationException($"Unknown implementation of {nameof(IFeatureTest)}."),
			};
		}

		private IDimensionInterval FromNullFeatureTest(NullFeatureTest nullFeatureTest) {
			var featureIndex = nullFeatureTest.FeatureIndex;
			var featureType = Dataset.GetFeatureType(featureIndex);
			var featureValues = Dataset.GetSortedUniqueFeatureValues(featureIndex);

			switch (featureType) {

			// @Improve performance
			case FeatureType.Binary:
			return BinaryDimensionInterval.FromSortedUniqueFeatureValues(
				dimensionIndex: featureIndex,
				featureValues: featureValues);

			case FeatureType.Continuous:
			var min = featureValues[0];
			var max = featureValues[^1];
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

		private IDimensionInterval FromBinaryFeatureTest(BinaryFeatureTest binary) {
			return BinaryDimensionInterval.FromSingleValue(
				dimensionIndex: binary.FeatureIndex,
				value: binary.Value);
		}

		public IDimensionInterval CreateMaximalDimensionInterval(int dimensionIndex) {
			if (!Dataset.IsFeatureIndexValid(dimensionIndex))
				throw new ArgumentOutOfRangeException(nameof(dimensionIndex));

			throw new NotImplementedException();

			//switch (Dataset.GetFeatureType(dimensionIndex)) {

			//case FeatureType.Categorical: {
			//	var values = Dataset.GetSortedUniqueFeatureValues(dimensionIndex);
			//	return CategoricalDimensionInterval.FromSortedUniqueValues(
			//		dimensionIndex: dimensionIndex,
			//		sortedUniqueValues: values);
			//}

			//case FeatureType.CategoricalButTriviallyValued: {
			//	var values = Dataset.GetSortedUniqueFeatureValues(dimensionIndex);
			//	return CategoricalDimensionInterval.FromSortedUniqueValues(
			//		dimensionIndex: dimensionIndex,
			//		sortedUniqueValues: values);
			//}

			//case FeatureType.Continuous: {
			//	var lowerBound = new DimensionBound(
			//		value: float.NegativeInfinity,
			//		isInclusive: true);

			//	// @Careful, this feels weird...
			//	var upperBound = new DimensionBound(
			//		value: float.PositiveInfinity,
			//		isInclusive: true);

			//	return new ContinuousDimensionInterval(
			//		dimensionIndex: dimensionIndex,
			//		start: lowerBound,
			//		end: upperBound);
			//}

			//case FeatureType.ContinuousButTriviallyValued: {
			//	var lowerBound = new DimensionBound(
			//		value: float.NegativeInfinity,
			//		isInclusive: true);

			//	// @Careful, this feels weird...
			//	var upperBound = new DimensionBound(
			//		value: float.PositiveInfinity,
			//		isInclusive: true);

			//	return new ContinuousDimensionInterval(
			//		dimensionIndex: dimensionIndex,
			//		start: lowerBound,
			//		end: upperBound);
			//}

			//default:
			//throw new InvalidOperationException(ExceptionMessages.UnknownDimensionIntervalType);
			//}
		}
	}
}