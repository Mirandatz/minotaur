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
			return dimensionInterval switch
			{
				CategoricalDimensionInterval cat => FromCategorical(cat),
				ContinuousDimensionInterval cont => FromContinuous(cont),

				_ => throw new ArgumentException()
			};
		}

		private IFeatureTest FromCategorical(CategoricalDimensionInterval cat) {
			// @Todo: add safety (sanity?) checks
			var possibleValues = cat.SortedValues;
			var selectedValue = Random.Choice(possibleValues);

			return new CategoricalFeatureTest(
				featureIndex: cat.DimensionIndex,
				value: selectedValue);
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
