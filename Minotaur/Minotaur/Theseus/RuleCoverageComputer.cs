namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class RuleCoverageComputer {
		private readonly Dataset _dataset;
		private readonly ICache<Rule, RuleCoverage> _cache;

		public RuleCoverageComputer(Dataset dataset, ICache<Rule, RuleCoverage> cache) {
			_dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
			_cache = cache ?? throw new ArgumentNullException(nameof(cache));
		}

		public RuleCoverage ComputeRuleCoverage(Rule rule) {
			if (rule is null)
				throw new ArgumentNullException(nameof(rule));

			var ruleCoverage = _cache.GetOrCreate(
				key: rule,
				valueCreator: () => UncachedComputeRuleCoverage(rule));

			return ruleCoverage;
		}

		private RuleCoverage UncachedComputeRuleCoverage(Rule rule) {
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
