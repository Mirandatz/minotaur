namespace Minotaur.Theseus.IndividualCreation {
	using Minotaur.Collections.Dataset;
	using Minotaur.EvolutionaryAlgorithms.Population;

	public interface IIndividualCreator {
		Dataset Dataset { get; }
		Individual CreateFirstGenerationIndividual();
	}
}
