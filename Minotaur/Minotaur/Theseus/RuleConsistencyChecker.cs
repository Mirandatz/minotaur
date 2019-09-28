namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class RuleConsistencyChecker {

		public readonly HyperRectangleCreator HyperRectangleCreator;

		public RuleConsistencyChecker(HyperRectangleCreator hyperRectangleCreator) {
			HyperRectangleCreator = hyperRectangleCreator;
		}

		public bool IsConsistent(Individual individual) {
			var allRulesSpan = individual.Rules.AsSpan();

			for (int i = 1; i < allRulesSpan.Length; i++) {
				var previousRules = allRulesSpan.Slice(
					start: 0,
					length: i);

				var currentRule = allRulesSpan[i];

				if (!AreConsistent(existingRules: previousRules, newRule: currentRule))
					return false;
			}

			return true;
		}

		public bool AreConsistent(ReadOnlySpan<Rule> existingRules, Rule newRule) {
			for (int i = 0; i < existingRules.Length; i++) {
				if (!AreConsistent(existingRules[i], newRule))
					return false;
			}

			return true;
		}

		public bool AreConsistent(Rule lhs, Rule rhs) {
			if (lhs.PredictedLabels.SequenceEquals(rhs.PredictedLabels))
				return true;

			var lhsBox = HyperRectangleCreator.FromRule(lhs);
			var rhsBox = HyperRectangleCreator.FromRule(rhs);
			var boxesOverlap = HyperRectangleIntersector.IntersectsInAllDimensions(lhsBox, rhsBox);

			return !boxesOverlap;
		}
	}
}
