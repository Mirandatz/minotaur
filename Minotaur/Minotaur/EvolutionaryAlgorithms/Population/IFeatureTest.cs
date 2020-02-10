namespace Minotaur.EvolutionaryAlgorithms.Population {
	using System;
	using Minotaur.Collections;

	public interface IFeatureTest: IEquatable<IFeatureTest> {
		int TestSize { get; }
		int FeatureIndex { get; }
		bool Matches(Array<float> datasetInstance);
	}
}