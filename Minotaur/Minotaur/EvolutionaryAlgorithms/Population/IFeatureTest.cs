namespace Minotaur.EvolutionaryAlgorithms.Population {
	using Minotaur.Collections;

	public interface IFeatureTest {
		int TestSize { get; }
		int FeatureIndex { get; }
		bool Matches(Array<float> datasetInstance);
	}
}
