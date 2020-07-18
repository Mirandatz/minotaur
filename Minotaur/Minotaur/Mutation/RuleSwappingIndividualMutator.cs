namespace Minotaur.Mutation {
	using System;
	using Minotaur.Classification;
	using Minotaur.Random;

	public sealed class RuleSwappingIndividualMutator: IIndividualMutator {

		private readonly BiasedOptionChooser<MutationType> _mutationChooser;

		public RuleSwappingIndividualMutator(BiasedOptionChooser<MutationType> mutationChooser) {
			_mutationChooser = mutationChooser;
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
			throw new NotImplementedException();
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
