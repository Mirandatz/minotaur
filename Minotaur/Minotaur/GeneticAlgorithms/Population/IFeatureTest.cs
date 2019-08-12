namespace Minotaur.GeneticAlgorithms.Population {
	using System;

	public interface IFeatureTest: IEquatable<IFeatureTest> {
		int TestSize { get; }
		int FeatureIndex { get; }
		bool Matches(ReadOnlySpan<float> instance);
		bool Overlaps(IFeatureTest featureTest);
	}
}
