namespace Minotaur.Theseus.Evolution {
	using System;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.GeneticAlgorithms.Selection;
	using Minotaur.Output;
	using Minotaur.Theseus.IndividualMutation;

	public sealed class EvolutionEngineMk2 {

		private readonly int _maximumNumberOfGenerations;
		private readonly FitnessEvaluatorMk2 _fitnessEvaluator;
		private readonly PopulationMutatorMk2 _populationMutator;
		private readonly IFittestIdentifier _fittestIdentifier;
		private readonly Array<IEvolutionLogger> _loggers;

		public EvolutionEngineMk2(int maximumNumberOfGenerations, FitnessEvaluatorMk2 fitnessEvaluator, PopulationMutatorMk2 populationMutator, IFittestIdentifier fittestIdentifier, Array<IEvolutionLogger> loggers) {
			if (_maximumNumberOfGenerations <= 0)
				throw new ArgumentOutOfRangeException(nameof(maximumNumberOfGenerations));

			_maximumNumberOfGenerations = maximumNumberOfGenerations;
			_fitnessEvaluator = fitnessEvaluator;
			_populationMutator = populationMutator;
			_fittestIdentifier = fittestIdentifier;
			_loggers = loggers;
		}

		public GenerationResult Run(Array<Individual> initialPopulation) {
			if (initialPopulation.Length == 0)
				throw new ArgumentException(nameof(initialPopulation));

			var oldPopulation = initialPopulation.ShallowCopy();
			Array<Fitness> oldFitnesses = _fitnessEvaluator.EvaluateAsMaximizationTask(oldPopulation);

			int generationNumber;
			for (generationNumber = 0; generationNumber <= _maximumNumberOfGenerations; generationNumber++) {
				var generationResult = RunSingleGeneration(generationNumber, oldPopulation, oldFitnesses);

				if (generationResult is null)
					break;

				oldPopulation = generationResult.Population;
				oldFitnesses = generationResult.Fitnesses;

				RunLoggers(generationResult);
			}

			return new GenerationResult(
				generationNumber: generationNumber,
				population: oldPopulation,
				fitnesses: oldFitnesses);
		}

		private GenerationResult? RunSingleGeneration(int generationNumber, Array<Individual> population, Array<Fitness> populationFitnesses) {
			var mutants = _populationMutator.TryMutate(population);
			if (mutants is null)
				return null;

			var mutantsFitnesses = _fitnessEvaluator.EvaluateAsMaximizationTask(mutants);
			var fittestCandidates = population.Concatenate(mutants);
			var fittestCandidatesFitnesses = populationFitnesses.Concatenate(mutantsFitnesses);
			var fittestIndices = _fittestIdentifier.FindIndicesOfFittestIndividuals(fittestCandidatesFitnesses);

			var fittestIndividuals = IndexingHelper.CopyIndexedItems(
				indices: fittestIndices,
				items: fittestCandidates);

			var fittestIndividualsFitnesses = IndexingHelper.CopyIndexedItems(
				indices: fittestIndices,
				items: fittestCandidatesFitnesses);

			return new GenerationResult(
				generationNumber: generationNumber,
				population: fittestIndividuals,
				fitnesses: fittestIndividualsFitnesses);
		}

		private void RunLoggers(GenerationResult generationResult) {
			for (int i = 0; i < _loggers.Length; i++)
				_loggers[i].LogGeneration(generationResult);
		}
	}
}
