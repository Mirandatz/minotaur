namespace Minotaur.Theseus.IndividualCreation {
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;

	public interface IIndividualCreator {
		Dataset Dataset { get; }
		Individual Create();
	}
}
