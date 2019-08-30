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
		
		public override int GetHashCode() => FeatureIndex;

		public override bool Equals(object obj) => Equals(obj as NullFeatureTest);

		public bool Equals(IFeatureTest other) => Equals(other as NullFeatureTest);

		public bool Equals(NullFeatureTest other) {
			return other != null &&
				other.FeatureIndex == FeatureIndex;
		}
	}
}
