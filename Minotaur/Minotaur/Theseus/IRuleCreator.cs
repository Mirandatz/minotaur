namespace Minotaur.Theseus {
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;

	public interface IRuleCreator {
		Dataset Dataset { get; }
		bool TryCreateRule(Array<Rule> existingRules, out Rule newRule);
	}
}
