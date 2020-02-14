namespace Minotaur.Theseus.Evolution {
	using System;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.EvolutionaryAlgorithms;
	using Minotaur.EvolutionaryAlgorithms.Population;
	using Minotaur.EvolutionaryAlgorithms.Selection;
	using Minotaur.Output;
	using Minotaur.Theseus.Mutation;

	public sealed class EvolutionEngine {

		private readonly int _maximumNumberOfGenerations;
		private readonly FitnessEvaluator _fitnessEvaluator;
		private readonly PopulationMutator _populationMutator;
		private readonly IFittestIdentifier _fittestIdentifier;

		public EvolutionEngine(int maximumNumberOfGenerations, FitnessEvaluator fitnessEvaluator, PopulationMutator populationMutator, IFittestIdentifier fittestIdentifier) {
			if (maximumNumberOfGenerations <= 0)
				throw new ArgumentOutOfRangeException(nameof(maximumNumberOfGenerations));

			_maximumNumberOfGenerations = maximumNumberOfGenerations;
			_fitnessEvaluator = fitnessEvaluator;
			_populationMutator = populationMutator;
			_fittestIdentifier = fittestIdentifier;
		}

		public GenerationSummary Run(Array<Individual> initialPopulation) {
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
			}

			return new GenerationSummary(
				generationNumber: generationNumber,
				population: oldPopulation,
				fitnesses: oldFitnesses);
		}

		private GenerationSummary? RunSingleGeneration(int generationNumber, Array<Individual> population, Array<Fitness> populationFitnesses) {
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

			return new GenerationSummary(
				generationNumber: generationNumber,
				population: fittestIndividuals,
				fitnesses: fittestIndividualsFitnesses);
		}
	}
}
