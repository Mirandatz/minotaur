namespace Minotaur.GeneticAlgorithms.Population {
	using System;
	using Minotaur.Collections;

	public interface IFeatureTest: IEquatable<IFeatureTest> {
		int TestSize { get; }
		int FeatureIndex { get; }
		bool Matches(Array<float> instance);
		bool Overlaps(IFeatureTest featureTest);
	}
}
