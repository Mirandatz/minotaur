namespace Minotaur.Theseus {
	using System;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class EvolutionReport {
		public readonly int GenerationsRan;
		public readonly string ReasonForStoppingEvolution;
		public readonly Array<Individual> FinalPopulation;

		public EvolutionReport(
			int generationsRan,
			string reasonForStoppingEvolution,
			Array<Individual> finalPopulation
			) {
			GenerationsRan = generationsRan;
			ReasonForStoppingEvolution = reasonForStoppingEvolution ?? throw new ArgumentNullException(nameof(reasonForStoppingEvolution));
			FinalPopulation = finalPopulation ?? throw new ArgumentNullException(nameof(finalPopulation));
		}
	}
}
