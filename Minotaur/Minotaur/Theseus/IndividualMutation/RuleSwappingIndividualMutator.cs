namespace Minotaur.Theseus.IndividualMutation {
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Random;
	using Minotaur.Theseus.RuleCreation;
	using Random = Random.ThreadStaticRandom;

	// @Assumption: all individuals have the same default predictions
	public sealed class RuleSwappingIndividualMutator: IIndividualMutator {

		public readonly Dataset Dataset;
		private readonly BiasedOptionChooser<IndividualMutationType> _mutationChooser;
		private readonly IRuleCreator _ruleCreator;

		public RuleSwappingIndividualMutator(
			BiasedOptionChooser<IndividualMutationType> mutationChooser,
			IRuleCreator ruleCreator) {
			_mutationChooser = mutationChooser;
			_ruleCreator = ruleCreator;
			Dataset = _ruleCreator.Dataset;
		}

		public Individual? TryMutate(Individual original) {
			return (_mutationChooser.GetRandomChoice()) switch
			{
				IndividualMutationType.AddRule => TryAddRule(original),
				IndividualMutationType.ModifyRule => TryModifyRule(original),
				IndividualMutationType.RemoveRule => TryRemoveRule(original),

				_ => throw CommonExceptions.UnknownFeatureType,
			};
		}

		private Individual? TryAddRule(Individual original) {
			var oldRules = original.Rules;

			var createdNewRule = _ruleCreator.TryCreateRule(
				existingRules: oldRules,
				newRule: out var newRule);

			if (!createdNewRule)
				return null;

			var newRules = new Rule[original.Rules.Length + 1];

			for (int i = 0; i < oldRules.Length; i++)
				newRules[i] = oldRules[i];

			newRules[^1] = newRule;

			return new Individual(
				rules: newRules,
				defaultLabels: original.DefaultLabels);
		}

		private Individual? TryModifyRule(Individual original) {
			var oldRules = original.Rules;

			// @Todo: investigate the possibility of actually "modifying" a rule,
			// instead of swaping it for a new one

			// @Improve performance
			var candidateIndex = Random.Int(
				inclusiveMin: 0,
				exclusiveMax: oldRules.Length);

			// Copying all but the rule to be mutated
			var withoutCandidate = new Rule[oldRules.Length - 1];
			for (
				int i = 0, j = 0;
				i < oldRules.Length;
				i++
				) {
				if (i == candidateIndex)
					continue;

				withoutCandidate[j] = oldRules[i];
				j += 1;
			}

			// Since we removed a rule, 
			// it should _always_ be possible to create a new rule,
			// even if it is the same rule.
			// So if we get false as return from this method call,
			// we throwing bruh... coz that indicates
			// there's something wrong somewhere
			var canCreate = _ruleCreator.TryCreateRule(
				existingRules: withoutCandidate,
				newRule: out var newRule);

			if (!canCreate)
				return null;

			var newRules = oldRules.Swap(
				index: candidateIndex,
				newItem: newRule);

			return new Individual(
				rules: newRules,
				defaultLabels: original.DefaultLabels);
		}

		private Individual? TryRemoveRule(Individual original) {
			var oldRules = original.Rules;

			// Can't have individuals with no rules
			if (oldRules.Length == 1)
				return null;

			var indexToRemove = Random.Int(
				inclusiveMin: 0,
				exclusiveMax: oldRules.Length);

			var newRules = oldRules.Remove(indexToRemove);

			return new Individual(
				rules: newRules,
				defaultLabels: original.DefaultLabels);
		}
	}
}