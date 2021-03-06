namespace Minotaur.Theseus.Evolution {
	using System;
	using Minotaur.Collections;
	using Minotaur.EvolutionaryAlgorithms;
	using Minotaur.EvolutionaryAlgorithms.Population;

	public sealed class GenerationSummary {
		public readonly int GenerationNumber;
		public readonly Array<Individual> Population;
		public readonly Array<Fitness> Fitnesses;

		public GenerationSummary(int generationNumber, Array<Individual> population, Array<Fitness> fitnesses) {
			if (generationNumber < 0)
				throw new ArgumentOutOfRangeException(nameof(generationNumber));
			if (population.Length != fitnesses.Length)
				throw new ArgumentException();

			GenerationNumber = generationNumber;
			Population = population;
			Fitnesses = fitnesses;
		}
	}
}
