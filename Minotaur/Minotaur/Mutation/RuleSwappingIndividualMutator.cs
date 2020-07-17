namespace Minotaur.Mutation {
	using System;
	using Minotaur.Classification;

	public sealed class RuleSwappingIndividualMutator: IIndividualMutator {

		public ConsistentModel? TryMutate(ConsistentModel model) {
			throw new NotImplementedException();
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
