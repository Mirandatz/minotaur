namespace Minotaur.GeneticAlgorithms.Population {
	using System;
	using Minotaur.Collections;
	using Newtonsoft.Json;

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class CategoricalFeatureTest: IFeatureTest, IEquatable<CategoricalFeatureTest> {

		[JsonProperty] public int FeatureIndex { get; }

		public int TestSize => 1;

		///<remarks>
		/// We're using floats because the .csv parser
		/// only handles floats.
		/// </remarks>
		[JsonProperty] public readonly float Value;

		private readonly int _precomputedHashCode;

		[JsonConstructor]
		public CategoricalFeatureTest(int featureIndex, float value) {
			if (featureIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + " must be >= 0");
			if (float.IsNaN(value))
				throw new ArgumentOutOfRangeException(nameof(value) + " can't be NaN");

			FeatureIndex = featureIndex;
			Value = value;

			_precomputedHashCode = HashCode.Combine(featureIndex, value);
		}

		public bool Covers(float featureValue) => Value == featureValue;

		public bool Matches(Array<float> instance) => instance[FeatureIndex] == Value;

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object obj) => Equals(obj as CategoricalFeatureTest);

		public bool Equals(IFeatureTest other) => Equals(other as CategoricalFeatureTest);

		public bool Equals(CategoricalFeatureTest other) {
			return other != null &&
				other.FeatureIndex == FeatureIndex &&
				other.Value == Value;
		}
	}
}
