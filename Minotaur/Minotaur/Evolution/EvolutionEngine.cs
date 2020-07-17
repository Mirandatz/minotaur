namespace Minotaur.Evolution {
	using System;
	using Minotaur.Classification;

	public sealed class EvolutionEngine {

		public ConsistentModel[] Evolve(ReadOnlySpan<ConsistentModel> initialPopulation) {
			if (initialPopulation.IsEmpty)
				throw new ArgumentException(nameof(initialPopulation) + " can't be empty.");

			throw new NotImplementedException();
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
