namespace Minotaur.Theseus.IndividualCreation {
	using System;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Theseus.RuleCreation;

	public sealed class MinimalIndividualCreator: IIndividualCreator {
		public Dataset Dataset { get; }
		private readonly IRuleCreator _ruleCreator;

		public MinimalIndividualCreator(IRuleCreator ruleCreator) {
			_ruleCreator = ruleCreator;
			Dataset = ruleCreator.Dataset;
		}

		public Individual Create() {
			if (!_ruleCreator.TryCreateRule(existingRules: Array.Empty<Rule>(), out var rule))
				throw new InvalidOperationException("This operation should never fail.");

			var defaultLabels = new bool[Dataset.ClassCount];
			return new Individual(
				rules: new Rule[] { rule },
				defaultLabels: defaultLabels);
		}
	}
}
