namespace Minotaur.Theseus.IndividualCreation {
	using System;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Theseus.RuleCreation;

	public sealed class MinimalIndividualCreator: IIndividualCreator {
		public Dataset Dataset { get; }
		private readonly OverfittingRuleCreator _ruleCreator;

		public MinimalIndividualCreator(OverfittingRuleCreator ruleCreator) {
			_ruleCreator = ruleCreator ?? throw new ArgumentNullException(nameof(ruleCreator));
			Dataset = ruleCreator.Dataset;
		}

		public Individual Create() {
			var sucess = _ruleCreator.TryCreateRule(
				existingRules: Array.Empty<Rule>(),
				out var rule);

			if (!sucess)
				throw new InvalidOperationException("This operation should never fail.");

			var defaultLabels = new bool[Dataset.ClassCount];

			return new Individual(
				rules: new Rule[] { rule },
				defaultLabels: defaultLabels);
		}
	}
}
