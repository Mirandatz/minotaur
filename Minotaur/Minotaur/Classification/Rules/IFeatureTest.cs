namespace Minotaur.Classification.Rules {
	using System;
	using Minotaur.Collections;

	public interface IFeatureTest: IEquatable<IFeatureTest> {

		int TestSize { get; }
		int FeatureIndex { get; }
		bool Matches(Array<float> datasetInstance);
	}
}
