namespace Minotaur.Theseus {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.ExtensionMethods.SystemArray;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.GeneticAlgorithms.Selection;

	public sealed class EvolutionEngine {

		public readonly Dataset Dataset;
		public readonly PopulationMutator PopulationMutator;
		public readonly IFittestSelector FittestSelector;

		public readonly int MutantsPerGeneration;

		public readonly int MaximumFailedMutationPerGeneration;
		public readonly int MaximumGenerations;

		public (int GenerationsRan, Array<Individual> FinalPopulation)
			Run(IEnumerable<Individual> initialPopulation
			) {
			if (initialPopulation is null)
				throw new ArgumentNullException(nameof(initialPopulation));

			// @Improve performance
			var population = initialPopulation.ToArray();
			var stopWatch = new Stopwatch();
			stopWatch.Start();

			var generationsRan = 0;
			for (generationsRan = 0; generationsRan < MaximumGenerations; generationsRan++) {

				Console.WriteLine($"Running generation {generationsRan}/{generationsRan}");

				var success = PopulationMutator.TryMutate(
					population: population,
					out var mutants);

				if (!success)
					break;

				var populationWithMutants = population.Concatenate(mutants);
				var fittest = FittestSelector.SelectFittest(populationWithMutants);
				population = fittest;
			}
			stopWatch.Stop();

			Console.WriteLine($"Average generation time: {stopWatch.ElapsedMilliseconds / (double) generationsRan}");

			return (GenerationsRan: generationsRan, FinalPopulation: population);
		}
	}
}
