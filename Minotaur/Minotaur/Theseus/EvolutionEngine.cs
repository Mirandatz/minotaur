namespace Minotaur.Theseus {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Minotaur.ExtensionMethods.SystemArray;
	using Minotaur.GeneticAlgorithms;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.GeneticAlgorithms.Selection;
	using Minotaur.Theseus.IndividualMutation;

	public sealed class EvolutionEngine {

		public readonly PopulationMutator PopulationMutator;
		public readonly IFittestSelector FittestSelector;
		public readonly FitnessReportMaker FitnessReportMaker;

		public readonly int MaximumGenerations;

		public EvolutionEngine(
			PopulationMutator populationMutator,
			FitnessReportMaker fitnessReportMaker,
			IFittestSelector fittestSelector,
			int maximumGenerations
			) {
			PopulationMutator = populationMutator ?? throw new ArgumentNullException(nameof(populationMutator));
			FitnessReportMaker = fitnessReportMaker ?? throw new ArgumentNullException(nameof(fitnessReportMaker));
			FittestSelector = fittestSelector ?? throw new ArgumentNullException(nameof(fittestSelector));

			if (maximumGenerations <= 0)
				throw new ArgumentOutOfRangeException(nameof(maximumGenerations));

			MaximumGenerations = maximumGenerations;
		}

		public FitnessEvaluator FitnessEvaluator { get; }

		public EvolutionReport
			Run(IEnumerable<Individual> initialPopulation
			) {
			if (initialPopulation is null)
				throw new ArgumentNullException(nameof(initialPopulation));

			// @Improve performance
			var population = initialPopulation.ToArray();
			var reasonForStoppingEvolution = string.Empty;

			int generationsRan;
			for (generationsRan = 0; generationsRan < MaximumGenerations; generationsRan++) {
				Console.Write($"\rRunning generation {generationsRan}/{MaximumGenerations}");

				var success = PopulationMutator.TryMutate(
					population: population,
					out var mutants);

				if (!success) {
					reasonForStoppingEvolution = "" +
						"reached maximum number of failed mutation attempts " +
						"for a single generation";
					break;
				}

				if (generationsRan % 10 == 0) {
					Console.WriteLine();
					Console.WriteLine(FitnessReportMaker.MakeReport(population));
				}

				var populationWithMutants = population.Concatenate(mutants);
				var fittest = FittestSelector.SelectFittest(populationWithMutants);
				population = fittest;
			}

			// Since during the loop we were "\r"-ing,
			// after we finish the lone we oughta WriteLine
			Console.WriteLine();

			if (generationsRan == MaximumGenerations)
				reasonForStoppingEvolution = "reached maximum number of generations";

			return new EvolutionReport(
				generationsRan: generationsRan,
				reasonForStoppingEvolution: reasonForStoppingEvolution,
				finalPopulation: population);
		}
	}
}
