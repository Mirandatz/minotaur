namespace Minotaur.Theseus.IndividualCreation {
	using System;
	using System.Collections.Generic;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Theseus.RuleCreation;

	public sealed class OverfittingIndividualCreator: IIndividualCreator {
		public Dataset Dataset { get; }
		private readonly int _maximumInitialRuleCount;
		private readonly OverfittingRuleCreator _ruleCreator;

		public OverfittingIndividualCreator(OverfittingRuleCreator ruleCreator, int maximumInitialRuleCount) {
			if (maximumInitialRuleCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(maximumInitialRuleCount) + " must be >= 1.");

			_maximumInitialRuleCount = maximumInitialRuleCount;
			_ruleCreator = ruleCreator;
			Dataset = ruleCreator.Dataset;
		}

		public Individual Create() {
			var rules = new List<Rule>(capacity: _maximumInitialRuleCount);

			// @Improve peformance
			while (rules.Count < _maximumInitialRuleCount) {
				if (_ruleCreator.TryCreateRule(existingRules: rules.ToArray(), out var newRule)) {
					rules.Add(newRule);
				} else {
					break;
				}
			}

			var defaultLabels = new bool[Dataset.ClassCount];

			return new Individual(
				rules: rules.ToArray(),
				defaultLabels: defaultLabels);
		}
	}
}
