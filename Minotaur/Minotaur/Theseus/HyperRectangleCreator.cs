namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;

	public sealed class FeatureSpaceRegionCreator {
		private readonly Dataset _dataset;
		private readonly DimensionIntervalCreator _dimensionIntervalCreator;
		private readonly ICache<Rule, HyperRectangle> _cache;

		public FeatureSpaceRegionCreator(
			Dataset dataset,
			DimensionIntervalCreator dimensionIntervalCreator,
			ICache<Rule, HyperRectangle> cache
			) {
			_dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
			_dimensionIntervalCreator = dimensionIntervalCreator ?? throw new ArgumentNullException(nameof(dimensionIntervalCreator));
			_cache = cache ?? throw new ArgumentNullException(nameof(cache));
		}

		public HyperRectangle FromRule(Rule rule) {
			if (rule is null)
				throw new ArgumentNullException(nameof(rule));

			var isCached = _cache.TryGet(key: rule, out var featureSpace);
			if (!isCached) {
				featureSpace = UnchachedFromRule(rule);
				_cache.Add(key: rule, value: featureSpace);
			}

			return featureSpace;
		}

		public HyperRectangle FromDatasetInstance(int datasetInstanceIndex) {
			// @Improve exception text 
			if (!_dataset.IsInstanceIndexValid(datasetInstanceIndex))
				throw new ArgumentException(nameof(datasetInstanceIndex));
			
			var instanceData = _dataset.GetInstanceData(datasetInstanceIndex);
			var featureTypes = _dataset.GetFeatureTypes();

			throw new NotImplementedException();

			return new HyperRectangle(
				dimensions: dimensions,
				dimensionTypes: dimensionTypes);
		}

		private HyperRectangle UnchachedFromRule(Rule rule) {
			var tests = rule.Tests;
			var dimensions = new IDimensionInterval[tests.Length];
			var dimensionTypes = new FeatureType[dimensions.Length];

			for (int i = 0; i < dimensions.Length; i++) {
				dimensions[i] = _dimensionIntervalCreator.FromFeatureTest(tests[i]);
				dimensionTypes[i] = _dataset.GetFeatureType(i);
			};

			return new HyperRectangle(
				dimensions: dimensions,
				dimensionTypes: dimensionTypes);
		}
	}
}
