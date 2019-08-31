namespace Minotaur.GeneticAlgorithms.Population {
	using System;
	using Minotaur.Collections;
	using Newtonsoft.Json;

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ContinuousFeatureTest: IFeatureTest, IEquatable<ContinuousFeatureTest> {

		[JsonProperty] public readonly float LowerBound;
		[JsonProperty] public readonly float UpperBound;
		[JsonProperty] public int FeatureIndex { get; }

		public int TestSize => 2;

		private readonly int _precomputedHashCode;

		[JsonConstructor]
		public ContinuousFeatureTest(int featureIndex, float lowerBound, float upperBound) {
			if (featureIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + " must be >= 0");
			if (float.IsNaN(lowerBound))
				throw new ArgumentOutOfRangeException(nameof(lowerBound) + " can't be NaN.");
			if (float.IsNaN(upperBound))
				throw new ArgumentOutOfRangeException(nameof(upperBound) + " can't be NaN.");

			if (lowerBound > upperBound)
				throw new ArgumentException(nameof(upperBound) + " must be equal to or greater than " + nameof(lowerBound));

			LowerBound = lowerBound;
			UpperBound = upperBound;
			FeatureIndex = featureIndex;

			_precomputedHashCode = HashCode.Combine(LowerBound, UpperBound, FeatureIndex);
		}

		public bool Matches(Array<float> instance) {
			// We use 
			// lowerBound <= threshold < upperBound
			// instead of 
			// lowerbound <= threshold <= upperBound
			// to allow the creation of "adjacent" rules

			var featureValue = instance[FeatureIndex];
			return LowerBound <= featureValue && featureValue < UpperBound;
		}

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object obj) => Equals(obj as ContinuousFeatureTest);

		public bool Equals(IFeatureTest other) => Equals(other as ContinuousFeatureTest);

		public bool Equals(ContinuousFeatureTest other) {
			return other != null &&
				LowerBound == other.LowerBound &&
				UpperBound == other.UpperBound &&
				FeatureIndex == other.FeatureIndex;
		}
	}
}
