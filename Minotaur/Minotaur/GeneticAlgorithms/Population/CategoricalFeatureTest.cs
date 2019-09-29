namespace Minotaur.GeneticAlgorithms.Population {
	using System;
	using Minotaur.Collections;

	public sealed class CategoricalFeatureTest: IFeatureTest, IEquatable<CategoricalFeatureTest> {

		public int FeatureIndex { get; }

		public int TestSize => 1;

		///<remarks>
		/// We're using floats because the .csv parser
		/// only handles floats.
		/// </remarks>
		public readonly float Value;

		private readonly int _precomputedHashCode;

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

		public override string ToString() => $"f[{FeatureIndex}]={Value}";

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object? obj) {
			if (obj is CategoricalFeatureTest other)
				return Equals(other);
			else
				return false;
		}

		public bool Equals(IFeatureTest test) {
			if (test is CategoricalFeatureTest other)
				return Equals(other);
			else
				return false;
		}

		public bool Equals(CategoricalFeatureTest other) {
			return
				other.FeatureIndex == FeatureIndex &&
				other.Value == Value;
		}
	}
}
