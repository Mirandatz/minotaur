namespace Minotaur.Theseus.Mutation {
	using Minotaur.EvolutionaryAlgorithms.Population;

	public interface IIndividualMutator {
		Individual? TryMutate(Individual original);
	}
}