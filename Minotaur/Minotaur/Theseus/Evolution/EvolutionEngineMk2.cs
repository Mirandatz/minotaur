namespace Minotaur.Theseus.Evolution {
	using System;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.GeneticAlgorithms.Selection;
	using Minotaur.Theseus.IndividualMutation;

	public sealed class EvolutionEngineMk2 {

		private readonly FitnessEvaluatorMk2 _fitnessEvaluator;
		private readonly PopulationMutatorMk2 _populationMutator;
		private readonly IFittestIdentifier _fittestIdentifier;

		private readonly Array<IEvolutionStopper> _evolutionStoppers;
		private readonly Array<IPostGenerationCallback> _postGenerationCallbacks;

		public EvolutionEngineMk2(FitnessEvaluatorMk2 fitnessEvaluator, PopulationMutatorMk2 populationMutator, IFittestIdentifier fittestIdentifier, Array<IEvolutionStopper> evolutionStoppers, Array<IPostGenerationCallback> postGenerationCallbacks) {
			_fitnessEvaluator = fitnessEvaluator;
			_populationMutator = populationMutator;
			_fittestIdentifier = fittestIdentifier;
			_evolutionStoppers = evolutionStoppers;
			_postGenerationCallbacks = postGenerationCallbacks;
		}

		public GenerationResult Run(Array<Individual> initialPopulation) {
			if (initialPopulation.Length == 0)
				throw new ArgumentException(nameof(initialPopulation));

			var oldPopulation = initialPopulation.ShallowCopy();
			Array<Fitness> oldFitnesses = _fitnessEvaluator.EvaluateAsMaximizationTask(oldPopulation);

			int generationNumber = 0;

			while (true) {
				var generationResult = RunSingleGeneration(generationNumber, oldPopulation, oldFitnesses);

				if (generationResult is null)
					break;

				oldPopulation = generationResult.Population;
				oldFitnesses = generationResult.Fitnesses;

				foreach (var cb in _postGenerationCallbacks)
					cb.Run(generationResult);

				foreach (var es in _evolutionStoppers) {
					if (es.ShouldStopEvolution(generationResult))
						break;
				}

				generationNumber += 1;
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
