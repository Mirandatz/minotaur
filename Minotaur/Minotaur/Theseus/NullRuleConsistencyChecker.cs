namespace Minotaur.Theseus {
	using System;
	using Minotaur.Classification.Rules;

	public sealed class NullRuleConsistencyChecker: IRuleConsistencyChecker {

		public bool AreConsistent(ReadOnlySpan<Rule> rules) => true;

		public bool AreConsistent(ReadOnlySpan<Rule> existingRules, Rule newRule) => true;

		public bool AreConsistent(Rule lhs, Rule rhs) => true;
	}
}
