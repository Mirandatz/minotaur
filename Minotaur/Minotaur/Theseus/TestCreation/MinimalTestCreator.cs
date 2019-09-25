namespace Minotaur.Theseus.TestCreation {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;

	public sealed class MinimalTestCreator: ITestCreator {

		public Dataset Dataset { get; }
		private readonly int _maximumNumberOfNullFeatureTests;

		public MinimalTestCreator(Dataset dataset, float maximumRatioOfNullFeatureTest) {
			Dataset = dataset;

			if (maximumRatioOfNullFeatureTest < 0 || maximumRatioOfNullFeatureTest > 1)
				throw new ArgumentOutOfRangeException(nameof(maximumRatioOfNullFeatureTest));

			var featureCount = Dataset.FeatureCount;
			_maximumNumberOfNullFeatureTests = (int) (featureCount * maximumRatioOfNullFeatureTest);
		}

		public IFeatureTest FromDimensionInterval(IDimensionInterval dimensionInterval) {
			throw new NotImplementedException();
		}

		public IFeatureTest Create(int featureIndex, int datasetSeedInstanceIndex, HyperRectangle boundingBox) {
			throw new NotImplementedException();
		}
	}
}
