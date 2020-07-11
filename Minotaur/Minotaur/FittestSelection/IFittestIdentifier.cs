namespace Minotaur.EvolutionaryAlgorithms.Selection {
	using System;

	public interface IFittestIdentifier {

		int[] FindIndicesOfFittestIndividuals(ReadOnlySpan<Fitness> fitnesses);
	}
}
