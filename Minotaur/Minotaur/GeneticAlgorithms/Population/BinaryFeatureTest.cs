namespace Minotaur.GeneticAlgorithms.Population {
	using System;
	using Minotaur.Collections;

	public sealed class BinaryFeatureTest: IFeatureTest, IEquatable<BinaryFeatureTest> {

		public int FeatureIndex { get; }

		public int TestSize => 1;

		///<remarks>
		/// We're using floats because the .csv parser
		/// only handles floats.
		/// </remarks>
		public readonly float Value;

		private readonly int _precomputedHashCode;

		public BinaryFeatureTest(int featureIndex, float value) {
			if (featureIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + " must be >= 0");
			if (value != 0 && value != 1)
				throw new ArgumentOutOfRangeException(nameof(value));

			FeatureIndex = featureIndex;
			Value = value;

			_precomputedHashCode = HashCode.Combine(featureIndex, value);
		}

		public bool Covers(float featureValue) => Value == featureValue;

		public bool Matches(Array<float> instance) => instance[FeatureIndex] == Value;

		public override string ToString() => $"f[{FeatureIndex}]={Value}";

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object? obj) {
			if (obj is BinaryFeatureTest other)
				return Equals(other);
			else
				return false;
		}

		public bool Equals(IFeatureTest test) {
			if (test is BinaryFeatureTest other)
				return Equals(other);
			else
				return false;
		}

		public bool Equals(BinaryFeatureTest other) {
			return
				other.FeatureIndex == FeatureIndex &&
				other.Value == Value;
		}
	}
}
