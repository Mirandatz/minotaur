namespace Minotaur.Mutation {
	using System;
	using System.Linq;
	using Minotaur.Classification;
	using Minotaur.Collections;

	public sealed class PopulationMutator {

		private readonly int _maximumFailedAttempts;
		private readonly int _targetNumberOfMutants;
		private readonly IIndividualMutator _individualMutator;

		public PopulationMutator(IIndividualMutator individualMutator, int maximumFailedAttempts, int targetNumberOfMutants) {
			if (maximumFailedAttempts < 0)
				throw new ArgumentOutOfRangeException(nameof(maximumFailedAttempts) + " must be >= 0.");
			if (targetNumberOfMutants <= 0)
				throw new ArgumentOutOfRangeException(nameof(targetNumberOfMutants) + " must be > 0.");

			_maximumFailedAttempts = maximumFailedAttempts;
			_targetNumberOfMutants = targetNumberOfMutants;
			_individualMutator = individualMutator;
		}

		public ConsistentModel[]? TryGenerateMutants(Array<ConsistentModel> population) {
			if (population.IsEmpty)
				throw new ArgumentException(nameof(population) + " can't be empty.");
			if (population.ToHashSet().Count != population.Length)
				throw new ArgumentException(nameof(population) + " can't contain duplicated elements.");

			throw new NotImplementedException();
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
