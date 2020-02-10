namespace Minotaur.Theseus.Mutation {
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.EvolutionaryAlgorithms.Population;
	using Random = Random.ThreadStaticRandom;

	public sealed class PopulationMutator {
		private readonly IIndividualMutator _individualMutator;
		private readonly int _mutantsPerGeneration;
		private readonly int _maximumFailedAttemptsPerGeneration;

		public PopulationMutator(
			IIndividualMutator individualMutator,
			int mutantsPerGeneration,
			int maximumFailedAttemptsPerGeneration
			) {
			_individualMutator = individualMutator;

			if (mutantsPerGeneration <= 0)
				throw new ArgumentOutOfRangeException(nameof(mutantsPerGeneration) + " must be >= 1.");
			if (maximumFailedAttemptsPerGeneration <= 0)
				throw new ArgumentOutOfRangeException(nameof(maximumFailedAttemptsPerGeneration) + " must be >= 1.");

			_mutantsPerGeneration = mutantsPerGeneration;
			_maximumFailedAttemptsPerGeneration = maximumFailedAttemptsPerGeneration;
		}

		public Individual[]? TryMutate(Array<Individual> population) {
			if (population.IsEmpty)
				throw new ArgumentException(nameof(population) + " can't be empty.");

			Individual[] mutants;

			using var cts = new CancellationTokenSource();
			var options = new ParallelOptions();
			options.CancellationToken = cts.Token;
			mutants = TryMutateWithParallelOptions(population: population, options: options);

			// It is possible that a thread created the final mutant
			// while another thread failed to create one, 
			// pushing the fail count above the maximum.
			// That means that even if we reached / crossed the maximum,
			// we may still have succeded.
			// We must, therefore, check if all elements were successfully created.
			for (int i = 0; i < mutants.Length; i++) {
				if (mutants[i] is null) {
					return null!;
				}
			}

			return mutants;
		}

		private Individual[] TryMutateWithParallelOptions(Array<Individual> population, ParallelOptions options) {
			var failedAttempts = 0L;
			var mutants = new Individual[_mutantsPerGeneration];

			Parallel.For(
				fromInclusive: 0,
				toExclusive: mutants.Length,
				parallelOptions: options,
				(long index, ParallelLoopState loopState) => {

					while (true) {
						var updatedFailedAttempts = Interlocked.Read(ref failedAttempts);
						if (updatedFailedAttempts >= _maximumFailedAttemptsPerGeneration)
							loopState.Stop();

						var mutationCandidate = Random.Choice(population);

						var mutated = _individualMutator.TryMutate(original: mutationCandidate);
						if (mutated is null) {
							updatedFailedAttempts = Interlocked.Increment(ref failedAttempts);
						} else {
							mutants[index] = mutated;
							break;
						}
					}
				});

			return mutants;
		}
	}
}
