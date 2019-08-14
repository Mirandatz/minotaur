namespace Minotaur.Theseus {
	using System;
	using System.Threading.Tasks;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;

	public sealed class FeatureSpaceRegionCreator {
		private readonly Dataset _dataset;
		private readonly DimensionIntervalCreator _dimensionIntervalCreator;

		public FeatureSpaceRegionCreator(Dataset dataset, DimensionIntervalCreator dimensionIntervalCreator) {
			_dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
			_dimensionIntervalCreator = dimensionIntervalCreator ?? throw new ArgumentNullException(nameof(dimensionIntervalCreator));
		}

		public FeatureSpaceRegion FromRule(Rule rule) {
			if (rule is null)
				throw new ArgumentNullException(nameof(rule));

			var tests = rule.Tests;
			var dimensions = new IDimensionInterval[tests.Length];
			var dimensionTypes = new FeatureType[dimensions.Length];

			Parallel.For(0, dimensions.Length, i => {
				dimensions[i] = _dimensionIntervalCreator.FromFeatureTest(tests[i]);
				dimensionTypes[i] = _dataset.GetFeatureType(i);
			});

			return new FeatureSpaceRegion(
				dimensions: dimensions,
				dimensionTypes: dimensionTypes);
		}
	}
}
