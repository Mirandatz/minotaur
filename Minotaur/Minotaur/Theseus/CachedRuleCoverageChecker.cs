namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class CachedRuleCoverageChecker {
		private readonly Dataset _dataset;
		private readonly LruCache<Rule, RuleCoverage> _cache;

		public CachedRuleCoverageChecker(Dataset dataset, LruCache<Rule, RuleCoverage> cache) {
			_dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
			_cache = cache ?? throw new ArgumentNullException(nameof(cache));
		}

		public RuleCoverage ComputeRuleCoverage(Rule rule) {
			if (rule is null)
				throw new ArgumentNullException(nameof(rule));

			var isCached = _cache.TryGet(key: rule, out var ruleCoverage);
			if (!isCached) {
				ruleCoverage = ActuallyComputeRuleCoverage(rule);
				_cache.Add(key: rule, value: ruleCoverage);
			}

			return ruleCoverage;
		}

		private RuleCoverage ActuallyComputeRuleCoverage(Rule rule) {
			var instanceCount = _dataset.InstanceCount;
			var instaceIsCovered = new bool[instanceCount];

			for (int i = 0; i < instanceCount; i++) {
				var instanceData = _dataset.GetInstanceData(i);
				instaceIsCovered[i] = rule.Covers(instanceData);
			}

			return new RuleCoverage(
				dataset: _dataset,
				instancesCovered: instaceIsCovered);
		}
	}

}
