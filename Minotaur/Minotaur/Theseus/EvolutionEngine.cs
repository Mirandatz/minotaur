namespace Minotaur.Theseus {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Minotaur.ExtensionMethods.SystemArray;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.GeneticAlgorithms.Selection;
	using Minotaur.Theseus.IndividualBreeding;
	using Minotaur.Theseus.IndividualMutation;

	public sealed class EvolutionEngine {

		private readonly PopulationMutator _populationMutator;
		private readonly IFittestSelector _fittestSelector;
		private readonly FitnessReportMaker _fitnessReportMaker;
		private readonly RuleConsistencyChecker _consistencyChecker;

		private readonly int _maximumGenerations;

		public EvolutionEngine(
			PopulationMutator populationMutator,
			FitnessReportMaker fitnessReportMaker,
			IFittestSelector fittestSelector,
			RuleConsistencyChecker consistencyChecker,
			int maximumGenerations
			) {
			_populationMutator = populationMutator;
			_fitnessReportMaker = fitnessReportMaker;
			_fittestSelector = fittestSelector;
			_consistencyChecker = consistencyChecker;

			if (maximumGenerations <= 0)
				throw new ArgumentOutOfRangeException(nameof(maximumGenerations));

			_maximumGenerations = maximumGenerations;
		}

		public EvolutionReport
			Run(IEnumerable<Individual> initialPopulation
			) {
			if (initialPopulation is null)
				throw new ArgumentNullException(nameof(initialPopulation));

			// @Improve performance
			var population = initialPopulation.ToArray();
			var reasonForStoppingEvolution = string.Empty;

			int generationsRan;
			for (generationsRan = 0; generationsRan < _maximumGenerations; generationsRan++) {
				Console.Write($"\rRunning generation {generationsRan}/{_maximumGenerations}");
				
				if (!_populationMutator.TryMutate(population, out var mutants)) {
					reasonForStoppingEvolution = "" +
						"reached maximum number of failed mutation attempts " +
						"for a single generation";
					break;
				}

#if DEBUG
				// Saniy check
				Parallel.For(0, population.Length, i => {
					var individual = population[i];
					var isConsistent = _consistencyChecker.IsConsistent(individual);
					if (!isConsistent) {
						throw new InvalidOperationException();
					}
				});
#endif

				if (generationsRan % 10 == 0) {
					Console.WriteLine();
					Console.WriteLine(_fitnessReportMaker.MakeReport(population));
				}

				var populationWithMutants = population.Concatenate(mutants);
				var fittest = _fittestSelector.SelectFittest(populationWithMutants);
				population = fittest;
			}

			// Since during the loop we were "\r"-ing,
			// after we finish the lone we oughta WriteLine
			Console.WriteLine();

			if (generationsRan == _maximumGenerations)
				reasonForStoppingEvolution = "reached maximum number of generations";

			return new EvolutionReport(
				generationsRan: generationsRan,
				reasonForStoppingEvolution: reasonForStoppingEvolution,
				finalPopulation: population);
		}
	}
}
