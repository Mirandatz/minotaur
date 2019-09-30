namespace Minotaur.Theseus.TestCreation {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;

	public sealed class CerriTestCreator {

		public readonly Dataset Dataset;

		public CerriTestCreator(Dataset dataset) {
			Dataset = dataset;
		}

		public IFeatureTest Create(
			int featureIndex,
			int datasetSeedInstanceIndex,
			HyperRectangle boundingBox
			) {
			if (!Dataset.IsFeatureIndexValid(featureIndex))
				throw new ArgumentOutOfRangeException(nameof(featureIndex));

			if (!Dataset.IsInstanceIndexValid(datasetSeedInstanceIndex))
				throw new ArgumentOutOfRangeException(nameof(datasetSeedInstanceIndex));

			throw new NotImplementedException();

			//return (Dataset.GetFeatureType(featureIndex)) switch
			//{
			//	FeatureType.Categorical => FromCategorical(
			//	featureIndex: featureIndex,
			//	datasetSeedInstanceIndex: datasetSeedInstanceIndex,
			//	boundingBox: boundingBox),

			//	FeatureType.CategoricalButTriviallyValued => FromCategorical(
			//	featureIndex: featureIndex,
			//	datasetSeedInstanceIndex: datasetSeedInstanceIndex,
			//	boundingBox: boundingBox),

			//	FeatureType.Continuous => FromContinuous(
			//		featureIndex: featureIndex,
			//		datasetSeedInstanceIndex: datasetSeedInstanceIndex,
			//		boundingBox: boundingBox),

			//	_ => throw new InvalidOperationException($"Unknown / unsupported value for {nameof(FeatureType)}."),
			//};
		}

		private CategoricalFeatureTest FromCategorical(
			int featureIndex,
			int datasetSeedInstanceIndex,
			HyperRectangle boundingBox
			) {
			var seed = Dataset.GetInstanceData(datasetSeedInstanceIndex);

			// @Sanity check
			if (!boundingBox.Contains(seed))
				throw new InvalidOperationException();

			return new CategoricalFeatureTest(
				featureIndex: featureIndex,
				value: seed[featureIndex]);
		}

		private IFeatureTest FromContinuous(
		int featureIndex,
		int datasetSeedInstanceIndex,
		HyperRectangle boundingBox
		) {
			var seed = Dataset.GetInstanceData(datasetSeedInstanceIndex);

			// @Sanity check
			if (!boundingBox.Contains(seed))
				throw new InvalidOperationException();

			var possibleValues = Dataset.GetSortedUniqueFeatureValues(featureIndex);

			var cerriPoint = seed[featureIndex];

			var cerriLeftIndex = possibleValues.BinarySearchFirstOccurence(cerriPoint);
			var cerriRightIndex = possibleValues.BinarySearchLastOccurence(cerriPoint);

			var cerriLowerBound = cerriLeftIndex == 0
				? float.NegativeInfinity
				: possibleValues[cerriLeftIndex - 1];

			var cerriUpperBounddd = cerriRightIndex == possibleValues.Length - 1
				? float.PositiveInfinity
				: possibleValues[cerriRightIndex + 1];

			var dimensionInterval = (ContinuousDimensionInterval) boundingBox.GetDimensionInterval(featureIndex);

			var lowerBound = Math.Max(
				cerriLowerBound,
				dimensionInterval.Start.Value);

			var upperBound = Math.Min(
				cerriUpperBounddd,
				dimensionInterval.End.Value);

			return new ContinuousFeatureTest(
				featureIndex: featureIndex,
				lowerBound: lowerBound,
				upperBound: upperBound);
		}
	}
}
