namespace Minotaur.Theseus.Evolution {
	using System;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.GeneticAlgorithms.Selection;
	using Minotaur.Theseus.IndividualMutation;

	public sealed class EvolutionEngineMk2 {

		private readonly int _maximumNumberOfGenerations = 0;
		private readonly FitnessEvaluatorMk2 _fitnessEvaluator = null!;
		private readonly PopulationMutatorMk2 _populationMutator = null!;
		private readonly IFittestIdentifier _fittestIdentifier = null!;

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
	}
}
