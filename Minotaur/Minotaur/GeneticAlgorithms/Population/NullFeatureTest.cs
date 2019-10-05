namespace Minotaur.GeneticAlgorithms.Population {
	using System;
	using Minotaur.Collections;

	public sealed class NullFeatureTest: IFeatureTest, IEquatable<NullFeatureTest> {

		public int TestSize => 0;

		public NullFeatureTest(int featureIndex) {
			if (featureIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + " must be  >= 0");

			FeatureIndex = featureIndex;
		}

		public int FeatureIndex { get; }

		public bool Matches(Array<float> instance) => true;

		public override string ToString() => string.Empty;

		public override int GetHashCode() => FeatureIndex;

		public override bool Equals(object? obj) => Equals((NullFeatureTest) obj!);

		public bool Equals(IFeatureTest test) => Equals((NullFeatureTest) test);

		public bool Equals(NullFeatureTest other) => FeatureIndex == other.FeatureIndex;
	}
}

