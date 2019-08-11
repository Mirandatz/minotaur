namespace Minotaur.Population {
	using System;
	using Minotaur.Collections;

	public interface IRule: IEquatable<IRule> {
		Array<IFeatureTest> Tests { get; }
		Array<bool> PredictedLabels { get; }
		bool Covers(ReadOnlySpan<float> instance);
	}
}
