namespace Minotaur.Theseus.TestCreation {
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;

	public interface ITestCreator {
		Dataset Dataset { get; }
		IFeatureTest FromDimensionInterval(IDimensionInterval dimensionInterval);
	}
}
