namespace Minotaur.Mutation {
	using System;
	using Minotaur.Classification;
	using Minotaur.Classification.Rules;
	using Minotaur.Random;
	using Minotaur.RuleCreation;
	using Random = Random.ThreadStaticRandom;

	public sealed class RuleSwappingIndividualMutator: IIndividualMutator {

		private readonly BiasedOptionChooser<MutationType> _mutationChooser;
		private readonly ConsistencyChecker _consistencyChecker;
		private readonly RuleCreator _ruleCreator;

		public RuleSwappingIndividualMutator(BiasedOptionChooser<MutationType> mutationChooser, ConsistencyChecker consistencyChecker, RuleCreator ruleCreator) {
			_mutationChooser = mutationChooser;
			_consistencyChecker = consistencyChecker;
			_ruleCreator = ruleCreator;
		}

		public ConsistentModel? TryMutate(ConsistentModel model) {
			return _mutationChooser.GetRandomChoice() switch
			{
				MutationType.AddRule => TryAddRule(model),
				MutationType.SwapRule => TrySwapRule(model),
				MutationType.RemoveRule => TryRemoveRule(model),
				_ => throw new InvalidOperationException($"Unknown {nameof(MutationType)}."),
			};
		}

		private ConsistentModel? TryAddRule(ConsistentModel model) {
			var oldRules = model.Rules.AsSpan();

			var rule = _ruleCreator.TryCreateRule(oldRules);
			if (rule is null)
				return null;

			var newRules = new Rule[oldRules.Length + 1];
			oldRules.CopyTo(newRules);
			newRules[^1] = rule;

			var newRuleSet = RuleSet.Create(newRules);

			return ConsistentModel.Create(
				ruleSet: newRuleSet,
				defaultPrediction: model.DefaultPrediction,
				consistencyChecker: _consistencyChecker);
		}

		private ConsistentModel? TrySwapRule(ConsistentModel model) {
			if (model.Rules.Count < 1)
				throw new InvalidOperationException("This should never happen... Models should contain at least one rule.");

			var newRules = model.Rules.ToArray();

			var swapIndex = Random.Int(exclusiveMax: newRules.Length);
			newRules[swapIndex] = newRules[^1];

			var rulesWithoutLast = newRules.AsSpan().Slice(
				start: 0,
				length: newRules.Length - 1);

			var newRule = _ruleCreator.TryCreateRule(existingRules: rulesWithoutLast);
			if (newRule is null)
				throw new InvalidOperationException("This should never happen... It should always be possible to create a rule after removing a rule.");

			newRules[^1] = newRule;
			var newRuleSet = RuleSet.Create(newRules);

			return ConsistentModel.Create(
				ruleSet: newRuleSet,
				defaultPrediction: model.DefaultPrediction,
				consistencyChecker: _consistencyChecker);
		}

		private ConsistentModel? TryRemoveRule(ConsistentModel model) {
			if (model.Rules.Count < 1)
				throw new InvalidOperationException("This should never happen! Models should contain at least one rule.");

			// Can't remove a rule, the new model would be empty
			// Return null to indicate that the mutation attempt failed gracefully
			if (model.Rules.Count == 1)
				return null;

			var oldRules = model.Rules.AsSpan();
			var newRules = new Rule[oldRules.Length - 1];

			var indexOfRemovedRule = Random.Int(exclusiveMax: oldRules.Length);

			for (int i = 0, j = 0;
				i < oldRules.Length;
				i++) {

				if (i == indexOfRemovedRule)
					continue;

				newRules[j] = oldRules[i];
				j += 1;
			}

			var newRuleSet = RuleSet.Create(newRules);

			return ConsistentModel.Create(
				ruleSet: newRuleSet,
				defaultPrediction: model.DefaultPrediction,
				consistencyChecker: _consistencyChecker);
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
