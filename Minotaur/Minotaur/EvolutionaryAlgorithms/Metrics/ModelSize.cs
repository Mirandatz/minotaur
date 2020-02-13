namespace Minotaur.EvolutionaryAlgorithms.Metrics {
	using Minotaur.EvolutionaryAlgorithms.Population;

	public sealed class ModelSize: IMetric {

		public string Name => nameof(ModelSize);

		public float Evaluate(Individual individual) {
			var modelSize = 0;
			var rules = individual.Rules;
			for (int i = 0; i < rules.Length; i++)
				modelSize += RuleSize(rules[i]);

			return modelSize;
		}

		private int RuleSize(Rule rule) {
			var ruleSize = 0;
			var tests = rule.Antecedent;

			for (int i = 0; i < tests.Length; i++)
				ruleSize += tests[i].TestSize;

			return ruleSize;
		}

		public float EvaluateAsMaximizationTask(Individual individual) => -1 * Evaluate(individual);
	}
}
