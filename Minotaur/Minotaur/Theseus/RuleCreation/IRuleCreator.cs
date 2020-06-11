namespace Minotaur.Theseus.RuleCreation {
	using System;
	using Minotaur.Classification.Rules;

	public interface IRuleCreator {
		Rule? TryCreateRule(ReadOnlySpan<Rule> existingRules);
	}
}
