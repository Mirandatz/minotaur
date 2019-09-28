namespace Minotaur.GeneticAlgorithms.Population {
	using System;
	using Minotaur.Collections;
	using Newtonsoft.Json;

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NullFeatureTest: IFeatureTest, IEquatable<NullFeatureTest> {

		public int TestSize => 0;

		[JsonConstructor]
		public NullFeatureTest(int featureIndex) {
			if (featureIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + " must be  >= 0");

			FeatureIndex = featureIndex;
		}

		[JsonProperty] public int FeatureIndex { get; }

		public bool Matches(Array<float> instance) => true;

		public override string ToString() => string.Empty;

		public override int GetHashCode() => FeatureIndex;

		public override bool Equals(object? obj) {
			if (obj is NullFeatureTest other)
				return Equals(other);
			else
				return false;
		}

		public bool Equals(IFeatureTest test) {
			if (test is NullFeatureTest other)
				return Equals(other);
			else
				return false;
		}

		public bool Equals(NullFeatureTest other) => FeatureIndex == other.FeatureIndex;
	}
}

