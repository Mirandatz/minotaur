namespace Minotaur.Theseus {
	using System;
	using Minotaur.Classification.Rules;

	public interface IRuleConsistencyChecker {
		bool AreConsistent(ReadOnlySpan<Rule> rules);
		bool AreConsistent(ReadOnlySpan<Rule> existingRules, Rule newRule);
		bool AreConsistent(Rule lhs, Rule rhs);
	}
}