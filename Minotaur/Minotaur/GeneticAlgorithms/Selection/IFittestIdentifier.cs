namespace Minotaur.GeneticAlgorithms.Selection {
	using Minotaur.Collections;

	public interface IFittestIdentifier {

		int[] FindIndicesOfFittestIndividuals(Array<Fitness> fitnesses);
	}
}
