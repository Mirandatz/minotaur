namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math;
	using Minotaur.Math.Dimensions;

	public sealed class HyperRectangleCreator {
		public readonly Dataset Dataset;
		private readonly DimensionIntervalCreator _dimensionIntervalCreator;
		private readonly IConcurrentCache<Rule, HyperRectangle> _cache;

		public HyperRectangleCreator(
			DimensionIntervalCreator dimensionIntervalCreator,
			IConcurrentCache<Rule, HyperRectangle> cache
			) {
			_dimensionIntervalCreator = dimensionIntervalCreator ?? throw new ArgumentNullException(nameof(dimensionIntervalCreator));
			_cache = cache ?? throw new ArgumentNullException(nameof(cache));

			Dataset = dimensionIntervalCreator.Dataset;
		}

		public HyperRectangle CreateLargestNonIntersectingHyperRectangle(
			Array<float> seed,
			Array<HyperRectangle> existingRectangles,
			NaturalRange dimensionExpansionOrder
			) {
			if (seed is null)
				throw new ArgumentNullException(nameof(seed));
			if (existingRectangles is null)
				throw new ArgumentNullException(nameof(existingRectangles));
			if (dimensionExpansionOrder is null)
				throw new ArgumentNullException(nameof(dimensionExpansionOrder));

			var dimensions = new IDimensionInterval[seed.Length];

			if (existingRectangles.IsEmpty) {
				for (int i = 0; i < dimensions.Length; i++)
					dimensions[i] = _dimensionIntervalCreator.CreateMaximalDimension(dimensionIndex: i);

				return new HyperRectangle(dimensions);
			}


			throw new NotImplementedException();

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
