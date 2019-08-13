namespace Minotaur.GeneticAlgorithms.Selection {
	using System;
	using Minotaur.GeneticAlgorithms.Population;

	public interface IFittestSelector {
		Individual[] SelectFittest(ReadOnlyMemory<Individual> population, int fittestCount);
	}
}
