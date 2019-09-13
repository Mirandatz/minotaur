namespace Minotaur.Theseus.IndividualMutation {
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms.Population;
	using Random = Random.ThreadStaticRandom;

	public sealed class PopulationMutator {
		public readonly IndividualMutator IndividualMutator;
		public readonly int MutantsPerGeneration;
		public readonly int MaximumFailedAttemptsPerGeneration;

		public PopulationMutator(
			IndividualMutator individualMutator,
			int mutantsPerGeneration,
			int maximumFailedAttemptsPerGeneration
			) {
			IndividualMutator = individualMutator ?? throw new ArgumentNullException(nameof(individualMutator));

			if (mutantsPerGeneration <= 0)
				throw new ArgumentOutOfRangeException(nameof(mutantsPerGeneration) + " must be >= 1.");
			if (maximumFailedAttemptsPerGeneration <= 0)
				throw new ArgumentOutOfRangeException(nameof(maximumFailedAttemptsPerGeneration) + " must be >= 1.");

			MutantsPerGeneration = mutantsPerGeneration;
			MaximumFailedAttemptsPerGeneration = maximumFailedAttemptsPerGeneration;
		}

		public bool TryMutate(Array<Individual> population, out Individual[] mutants) {
			if (population is null)
				throw new ArgumentNullException(nameof(population));
			if (population.IsEmpty)
				throw new ArgumentException(nameof(population) + " can't be empty.");

			Individual[] possibleMutants;

			using (var cts = new CancellationTokenSource()) {
				var options = new ParallelOptions();
				options.CancellationToken = cts.Token;

				possibleMutants = WithParallelOptions(
					population: population,
					options: options);
			}

			// It is possible that a thread create the final mutant
			// while another thread failed to create one,
			// pushing the fail count above the maximum.
			// That means that even if we crossed the maximum,
			// we may still have succeded.
			for (int i = 0; i < possibleMutants.Length; i++) {
				if (possibleMutants[i] is null) {
					mutants = null;
					return false;
				}
			}

			mutants = possibleMutants;
			return true;
		}

		private Individual[] WithParallelOptions(Array<Individual> population, ParallelOptions options) {
			var failedAttempts = 0L;
			var mutants = new Individual[MutantsPerGeneration];

			Parallel.For(
				fromInclusive: 0,
				toExclusive: mutants.Length,
				parallelOptions: options,
				(long index, ParallelLoopState loopState) => {

					while (true) {
						var updatedFailedAttempts = Interlocked.Read(ref failedAttempts);
						if (updatedFailedAttempts >= MaximumFailedAttemptsPerGeneration)
							loopState.Stop();

						var candidateIndex = Random.Int(
							inclusiveMin: 0,
							exclusiveMax: population.Length);

						var mutationCandidate = population[candidateIndex];

						var sucess = IndividualMutator.TryMutate(
							original: mutationCandidate,
							out var mutated);

						if (sucess) {
							mutants[index] = mutated;
							break;
						} else {
							updatedFailedAttempts = Interlocked.Increment(ref failedAttempts);
						}
					}
				});

			return mutants;
		}
	}
}
