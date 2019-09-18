namespace Minotaur.Theseus.IndividualMutation {
	using Minotaur.GeneticAlgorithms.Population;

	public interface IIndividualMutator {
		bool TryMutate(Individual original, out Individual mutated);
	}
}