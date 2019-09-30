namespace Minotaur.Theseus.TestCreation {
	using System;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;
	using Random = Random.ThreadStaticRandom;

	public sealed class MaximalTestCreator: ITestCreator {

		public Dataset Dataset { get; }

		public MaximalTestCreator(Dataset dataset) {
			Dataset = dataset;
		}

		public IFeatureTest FromDimensionInterval(IDimensionInterval dimensionInterval) {
			throw new NotImplementedException();
		}

		private IFeatureTest FromContinuous(ContinuousDimensionInterval cont) {
			// @Todo: add safety (sanity?) checks

			return new ContinuousFeatureTest(
				featureIndex: cont.DimensionIndex,
				lowerBound: cont.Start.Value,
				upperBound: cont.End.Value);
		}
	}
}
