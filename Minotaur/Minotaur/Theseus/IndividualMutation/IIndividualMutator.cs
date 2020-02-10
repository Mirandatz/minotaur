namespace Minotaur.Theseus.IndividualMutation {
	using Minotaur.EvolutionaryAlgorithms.Population;

	public interface IIndividualMutator {
		Individual? TryMutate(Individual original);
	}
}