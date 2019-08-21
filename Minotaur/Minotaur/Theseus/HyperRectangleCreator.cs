namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;

	public sealed class FeatureSpaceRegionCreator {
		private readonly Dataset _dataset;
		private readonly DimensionIntervalCreator _dimensionIntervalCreator;
		private readonly IConcurrentCache<Rule, HyperRectangle> _cache;

		public FeatureSpaceRegionCreator(
			Dataset dataset,
			DimensionIntervalCreator dimensionIntervalCreator,
			IConcurrentCache<Rule, HyperRectangle> cache
			) {
			_dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
			_dimensionIntervalCreator = dimensionIntervalCreator ?? throw new ArgumentNullException(nameof(dimensionIntervalCreator));
			_cache = cache ?? throw new ArgumentNullException(nameof(cache));
		}

		public HyperRectangle FromDatasetInstance(int datasetInstanceIndex) {
			// @Improve exception message 
			if (!_dataset.IsInstanceIndexValid(datasetInstanceIndex))
				throw new ArgumentException(nameof(datasetInstanceIndex));

			var dimensionCount = _dataset.FeatureCount;
			var dimensions = new IDimensionInterval[dimensionCount];

			for (int i = 0; i < dimensions.Length; i++) {
				dimensions[i] = _dimensionIntervalCreator.FromDatasetInstance(
					datasetInstanceIndex: datasetInstanceIndex,
					dimensionIndex: i);
			}

			return new HyperRectangle(dimensions);
		}

		public HyperRectangle FromRule(Rule rule) {
			if (rule is null)
				throw new ArgumentNullException(nameof(rule));

			var isCached = _cache.TryGet(key: rule, out var hyperRectangle);
			if (!isCached) {
				hyperRectangle = UnchachedFromRule(rule);
				_cache.Add(key: rule, value: hyperRectangle);
			}

			return hyperRectangle;
		}

		private HyperRectangle UnchachedFromRule(Rule rule) {
			var tests = rule.Tests;
			var dimensions = new IDimensionInterval[tests.Length];

			for (int i = 0; i < dimensions.Length; i++)
				dimensions[i] = _dimensionIntervalCreator.FromFeatureTest(tests[i]);

			return new HyperRectangle(dimensions: dimensions);
		}
	}
}
