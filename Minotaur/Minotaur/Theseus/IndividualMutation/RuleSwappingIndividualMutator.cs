namespace Minotaur.Theseus.IndividualMutation {
	using System;
	using System.Diagnostics.CodeAnalysis;
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

		public bool TryMutate(Individual original, [MaybeNullWhen(false)] out Individual mutated) {
			return (_mutationChooser.GetRandomChoice()) switch
			{
				IndividualMutationType.AddRule => TryAddRule(original, out mutated!),
				IndividualMutationType.ModifyRule => TryModifyRule(original, out mutated),
				IndividualMutationType.RemoveRule => TryRemoveRule(original, out mutated!),

				_ => throw new InvalidOperationException(ExceptionMessages.UnknownFeatureType),
			};
		}

		private bool TryAddRule(Individual original, [MaybeNullWhen(false)]  out Individual mutated) {
			var oldRules = original.Rules;

			var createdNewRule = _ruleCreator.TryCreateRule(
				existingRules: oldRules,
				newRule: out var newRule);

			if (!createdNewRule) {
				mutated = null!;
				return false;
			}

			var newRules = new Rule[original.Rules.Length + 1];

			for (int i = 0; i < oldRules.Length; i++)
				newRules[i] = oldRules[i];

			newRules[^1] = newRule;

			mutated = new Individual(
				rules: newRules,
				defaultLabels: original.DefaultLabels);

			return true;
		}

		private bool TryModifyRule(Individual original, out Individual mutated) {
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
				throw new InvalidOperationException("This should, like... Never happen.");

			var newRules = oldRules.Swap(
				index: candidateIndex,
				newItem: newRule);

			mutated = new Individual(
				rules: newRules,
				defaultLabels: original.DefaultLabels);

			return true;
		}

		private bool TryRemoveRule(Individual original, [MaybeNullWhen(false)] out Individual mutated) {
			var oldRules = original.Rules;

			// Can't have individuals with no rules
			if (oldRules.Length == 1) {
				mutated = null!;
				return false;
			}

			var indexToRemove = Random.Int(
				inclusiveMin: 0,
				exclusiveMax: oldRules.Length);

			var newRules = oldRules.Remove(indexToRemove);

			mutated = new Individual(
				rules: newRules,
				defaultLabels: original.DefaultLabels);

			return true;
		}
	}
}