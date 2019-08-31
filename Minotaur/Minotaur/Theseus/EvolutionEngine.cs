namespace Minotaur.Theseus {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.GeneticAlgorithms.Selection;

	public sealed class EvolutionEngine {

		public readonly Dataset Dataset;
		public readonly IndividualMutator IndividualMutator;
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
			var population = initialPopulation.ToList();

			throw new NotImplementedException();
		}
	}
}
