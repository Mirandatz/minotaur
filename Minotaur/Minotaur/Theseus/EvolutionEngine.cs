namespace Minotaur.Theseus {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Minotaur.ExtensionMethods.SystemArray;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.GeneticAlgorithms.Selection;

	public sealed class EvolutionEngine {

		public readonly PopulationMutator PopulationMutator;
		public readonly IFittestSelector FittestSelector;

		public readonly int MaximumGenerations;

		public EvolutionEngine(
			PopulationMutator populationMutator,
			IFittestSelector fittestSelector,
			int maximumGenerations
			) {
			PopulationMutator = populationMutator ?? throw new ArgumentNullException(nameof(populationMutator));
			FittestSelector = fittestSelector ?? throw new ArgumentNullException(nameof(fittestSelector));

			if (maximumGenerations <= 0)
				throw new ArgumentOutOfRangeException(nameof(maximumGenerations));

			MaximumGenerations = maximumGenerations;
		}

		public EvolutionReport
			Run(IEnumerable<Individual> initialPopulation
			) {
			if (initialPopulation is null)
				throw new ArgumentNullException(nameof(initialPopulation));

			// @Improve performance
			var population = initialPopulation.ToArray();

			var generationsRan = 0;
			for (generationsRan = 0; generationsRan < MaximumGenerations; generationsRan++) {

				Console.WriteLine($"Running generation {generationsRan}/{generationsRan}");

				var success = PopulationMutator.TryMutate(
					population: population,
					out var mutants);

				if (!success) {
					return new EvolutionReport(
						generationsRan: generationsRan,
						reasonForStoppingEvolution: "reached maximum number of failed mutation attempts for a single generation",
						finalPopulation: population);
				}

				var populationWithMutants = population.Concatenate(mutants);
				var fittest = FittestSelector.SelectFittest(populationWithMutants);
				population = fittest;
			}

			return new EvolutionReport(
				generationsRan: generationsRan,
				reasonForStoppingEvolution: "reached maximum number of generations",
				finalPopulation: population);
		}
	}
}
