namespace Minotaur.Theseus {
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class RuleCoverageComputer {
		private readonly Dataset _dataset;
		private readonly IConcurrentCache<Rule, RuleCoverage> _cache;

		public RuleCoverageComputer(Dataset dataset, IConcurrentCache<Rule, RuleCoverage> cache) {
			_dataset = dataset;
			_cache = cache;
		}

		public RuleCoverage ComputeRuleCoverage(Rule rule) {
			if (_cache.TryGet(rule, out var ruleCoverage)) {
				return ruleCoverage;
			} else {
				ruleCoverage = UncachedComputeRuleCoverage(rule);
				_cache.Add(key: rule, value: ruleCoverage);
				return ruleCoverage;
			}
		}

		private RuleCoverage UncachedComputeRuleCoverage(Rule rule) {
			var instanceCount = _dataset.InstanceCount;
			var instaceIsCovered = new bool[instanceCount];

			for (int i = 0; i < instaceIsCovered.Length; i++) {
				var instanceData = _dataset.GetInstanceData(i);
				instaceIsCovered[i] = rule.Covers(instanceData);
			}

			return new RuleCoverage(
				dataset: _dataset,
				instancesCovered: instaceIsCovered);
		}
	}
}
