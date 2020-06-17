namespace Minotaur.Classification {
	using System;
	using Minotaur.Classification.Rules;
	using Minotaur.Math.Geometry;

	public sealed class ConsistencyChecker: IConsistencyChecker {

		public bool AreConsistent(RuleSet rulesSet) {

			// @TODO: Investigate performance gains of caching Antecedent->Rectangles

			if (rulesSet.Count <= 1)
				return true;

			var rules = rulesSet.AsSpan();

			for (int i = 1; i < rules.Length; i++) {
				var current = rules[i];
				var previous = rules.Slice(start: 0, length: i);

				if (!AreConsistent(previous, current))
					return false;
			}

			return true;
		}

		private bool AreConsistent(ReadOnlySpan<Rule> previous, Rule current) {

			for (int i = 0; i < previous.Length; i++) {
				if (!AreConsistent(previous[i], current))
					return false;
			}

			return true;
		}

		private bool AreConsistent(Rule first, Rule second) {
			if (first.Consequent.Equals(second.Consequent))
				return true;

			var lhs = HyperRectangleCreator.FromRuleAntecedent(first.Antecedent);
			var rhs = HyperRectangleCreator.FromRuleAntecedent(second.Antecedent);

			return !HyperRectangleIntersector.Intersects(lhs, rhs);
		}
	}
}
