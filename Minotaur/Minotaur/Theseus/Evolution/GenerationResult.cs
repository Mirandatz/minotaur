namespace Minotaur.Theseus.Evolution {
	using System;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class GenerationResult {
		public readonly int GenerationNumber;
		public readonly Array<Individual> Population;
		public readonly Array<Fitness> Fitnesses;

		public GenerationResult(int generationNumber, Array<Individual> population, Array<Fitness> fitnesses) {
			if (generationNumber < 0)
				throw new ArgumentOutOfRangeException(nameof(generationNumber));
			if (population.Length != fitnesses.Length)
				throw new ArgumentException();

			Population = population;
			Fitnesses = fitnesses;
		}
	}
}
