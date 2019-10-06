namespace Minotaur.Theseus {
	using System;
	using System.Collections.Generic;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class RuleConsistencyChecker {

		private readonly RuleAntecedentHyperRectangleConverter _converter;
		private readonly HyperRectangleIntersector _intersector;

		public RuleConsistencyChecker(RuleAntecedentHyperRectangleConverter ruleAntecedentHyperRectangleConverterconverter, HyperRectangleIntersector hyperRectangleIntersector) {
			_converter = ruleAntecedentHyperRectangleConverterconverter;
			_intersector = hyperRectangleIntersector;
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

		public bool AreConsistent(List<Rule> consistentRules, Rule newRule) {
			var ruleCount = consistentRules.Count;
			for (int i = 0; i < ruleCount; i++) {
				if (!AreConsistent(consistentRules[i], newRule))
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
			if (lhs.Consequent.SequenceEquals(rhs.Consequent))
				return true;

			var lhsBox = _converter.FromRule(lhs);
			var rhsBox = _converter.FromRule(rhs);
			var boxesOverlap = _intersector.IntersectsInAllDimension(lhsBox, rhsBox);

			return !boxesOverlap;
		}
	}
}
