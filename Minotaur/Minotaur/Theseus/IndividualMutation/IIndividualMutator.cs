namespace Minotaur.Theseus.IndividualMutation {
	using Minotaur.GeneticAlgorithms.Population;

	public interface IIndividualMutator {
		Individual? TryMutate(Individual original);
	}
}