namespace Minotaur.Theseus.IndividualCreation {
	using System;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Theseus.RuleCreation;

	public sealed class SingleRuleIndividualCreator: IIndividualCreator {
		public Dataset Dataset { get; }
		private readonly IRuleCreator _ruleCreator;

		public SingleRuleIndividualCreator(IRuleCreator ruleCreator) {
			_ruleCreator = ruleCreator;
			Dataset = ruleCreator.Dataset;
		}

		public Individual Create() {
			var rule = _ruleCreator.TryCreateRule(existingRules: Array.Empty<Rule>());
			if (rule is null)
				throw new InvalidOperationException("This operation should never fail.");

			return new Individual(
				rules: new Rule[] { rule },
				defaultPrediction: Dataset.DefaultLabel);
		}
	}
}
