namespace Minotaur.Theseus {
	using System;
	using System.Collections.Generic;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class IndividualCreator {
		public readonly Dataset Dataset;
		private readonly int _maximumInitialRuleCount;
		private readonly RuleCreator _ruleCreator;

		public IndividualCreator(RuleCreator ruleCreator, int maximumInitialRuleCount) {
			if (maximumInitialRuleCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(maximumInitialRuleCount) + " must be >= 1.");

			_maximumInitialRuleCount = maximumInitialRuleCount;
			_ruleCreator = ruleCreator ?? throw new ArgumentNullException(nameof(ruleCreator));
			Dataset = ruleCreator.Dataset;
		}

		public Individual Create() {
			var rules = new List<Rule>(capacity: _maximumInitialRuleCount);

			// @Improve peformance
			while (rules.Count < _maximumInitialRuleCount) {
				var canCreateNewRule = _ruleCreator.TryCreateRule(
					existingRules: rules.ToArray(),
					out var newRule);

				if (canCreateNewRule)
					rules.Add(newRule);
				else
					break;
			}

			var defaultLabels = new bool[Dataset.ClassCount];

			return new Individual(
				rules: rules.ToArray(),
				defaultLabels: defaultLabels);
		}
	}
}
