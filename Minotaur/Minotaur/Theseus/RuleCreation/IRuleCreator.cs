namespace Minotaur.Theseus.RuleCreation {
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.EvolutionaryAlgorithms.Population;

	public interface IRuleCreator {
		Dataset Dataset { get; }
		Rule? TryCreateRule(Array<Rule> existingRules);
	}
}
