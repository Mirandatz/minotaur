namespace Minotaur.GeneticAlgorithms.Selection {
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms.Population;

	public interface IFittestSelector {
		Individual[] SelectFittest(Array<Individual> population);
	}
}
