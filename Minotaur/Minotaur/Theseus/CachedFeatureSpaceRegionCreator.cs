namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;

	public sealed class CachedFeatureSpaceRegionCreator {
		private readonly Dataset _dataset;
		private readonly DimensionIntervalCreator _dimensionIntervalCreator;
		private readonly LruCache<Rule, FeatureSpaceRegion> _cache;

		public CachedFeatureSpaceRegionCreator(
			Dataset dataset,
			DimensionIntervalCreator dimensionIntervalCreator,
			LruCache<Rule, FeatureSpaceRegion> cache
			) {
			_dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
			_dimensionIntervalCreator = dimensionIntervalCreator ?? throw new ArgumentNullException(nameof(dimensionIntervalCreator));
			_cache = cache ?? throw new ArgumentNullException(nameof(cache));
		}

		public FeatureSpaceRegion FromRule(Rule rule) {
			if (rule is null)
				throw new ArgumentNullException(nameof(rule));

			throw new NotImplementedException();
		}

		private FeatureSpaceRegion CreateFeatureSpaceFromRule(Rule rule) {
			if (rule is null)
				throw new ArgumentNullException(nameof(rule));

			var tests = rule.Tests;
			var dimensions = new IDimensionInterval[tests.Length];
			var dimensionTypes = new FeatureType[dimensions.Length];

			for (int i = 0; i < dimensions.Length; i++) {
				dimensions[i] = _dimensionIntervalCreator.FromFeatureTest(tests[i]);
				dimensionTypes[i] = _dataset.GetFeatureType(i);
			};

			return new FeatureSpaceRegion(
				dimensions: dimensions,
				dimensionTypes: dimensionTypes);
		}
	}
}
