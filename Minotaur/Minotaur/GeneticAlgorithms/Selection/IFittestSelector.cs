namespace Minotaur.GeneticAlgorithms.Selection {
	using System;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms.Population;

	public interface IFittestSelector {
		Individual[] SelectFittest(Array<Individual> population, int fittestCount);
	}
}
