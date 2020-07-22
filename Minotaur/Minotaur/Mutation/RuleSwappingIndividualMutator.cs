namespace Minotaur.Mutation {
	using System;
	using Minotaur.Classification;
	using Minotaur.Classification.Rules;
	using Minotaur.Random;
	using Random = Random.ThreadStaticRandom;

	public sealed class RuleSwappingIndividualMutator: IIndividualMutator {

		private readonly BiasedOptionChooser<MutationType> _mutationChooser;
		private readonly ConsistencyChecker _consistencyChecker;

		public RuleSwappingIndividualMutator(BiasedOptionChooser<MutationType> mutationChooser, ConsistencyChecker consistencyChecker) {
			_mutationChooser = mutationChooser;
			_consistencyChecker = consistencyChecker;
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
			throw new NotImplementedException();
		}

		private ConsistentModel? TrySwapRule(ConsistentModel model) {
			throw new NotImplementedException();
		}

		private ConsistentModel? TryRemoveRule(ConsistentModel model) {
			if (model.Rules.Count < 1)
				throw new InvalidOperationException("This should never happen... Models should contain at least one rule.");

			if (model.Rules.Count == 1)
				return null;

			var oldRules = model.Rules.AsSpan();
			var indexOfRemovedRule = Random.Int(exclusiveMax: oldRules.Length);
			var newRules = new Rule[oldRules.Length - 1];

			var newRulesIndex = 0;
			for (int oldRulesIndex = 0; oldRulesIndex < oldRules.Length; oldRulesIndex++) {
				if (oldRulesIndex == indexOfRemovedRule)
					continue;

				newRules[newRulesIndex] = oldRules[oldRulesIndex];
				newRulesIndex += 1;
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
