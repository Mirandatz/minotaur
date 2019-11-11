namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.GeneticAlgorithms.Selection;
	using Minotaur.Theseus.IndividualMutation;

	public sealed class EvolutionEngineMk2 {

		private readonly int _maximumGenerations;
		private readonly FitnessEvaluatorMk2 _fitnessEvaluator;
		private readonly PopulationMutatorMk2 _populationMutator;
		private readonly IFittestIdentifier _fittestIdentifier;

		public EvolutionEngineMk2(int maximumGenerations, FitnessEvaluatorMk2 fitnessEvaluator, PopulationMutatorMk2 populationMutator, IFittestIdentifier fittestIdentifier) {
			_maximumGenerations = maximumGenerations;
			_fitnessEvaluator = fitnessEvaluator;
			_populationMutator = populationMutator;
			_fittestIdentifier = fittestIdentifier;

			if (maximumGenerations <= 0)
				throw new ArgumentOutOfRangeException(nameof(maximumGenerations));
		}

		public GenerationResult Run(Array<Individual> initialPopulation) {
			if (initialPopulation.Length == 0)
				throw new ArgumentException(nameof(initialPopulation));

			var oldPopulation = initialPopulation.ShallowCopy();
			Array<Fitness> oldFitnesses = _fitnessEvaluator.EvaluateAsMaximizationTask(oldPopulation);

			for (int i = 0; i < _maximumGenerations; i++) {
				var generationResult = RunSingleGeneration(oldPopulation, oldFitnesses);

				if (generationResult is null)
					break;

				oldPopulation = generationResult.Population;
				oldFitnesses = generationResult.Fitnesses;
			}

			return new GenerationResult(
				population: oldPopulation,
				fitnesses: oldFitnesses);
		}

		private GenerationResult? RunSingleGeneration(Array<Individual> population, Array<Fitness> populationFitnesses) {
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
				population: fittestIndividuals,
				fitnesses: fittestIndividualsFitnesses);
		}

		public sealed class GenerationResult {
			public readonly Array<Individual> Population;
			public readonly Array<Fitness> Fitnesses;

			public GenerationResult(Array<Individual> population, Array<Fitness> fitnesses) {
				if (population.Length != fitnesses.Length)
					throw new ArgumentException();

				Population = population;
				Fitnesses = fitnesses;
			}
		}
	}
}
