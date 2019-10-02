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

		public BinaryFeatureTest(int featureIndex, float value) {
			if (featureIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(featureIndex));
			if (value != 0 && value != 1)
				throw new ArgumentOutOfRangeException(nameof(value));

			FeatureIndex = featureIndex;
			Value = value;
		}

		public bool Covers(float featureValue) {
			if (featureValue != 0 && featureValue != 1)
				throw new ArgumentOutOfRangeException(nameof(featureValue));

			return Value == featureValue;
		}

		public bool Matches(Array<float> instance) => Covers(instance[FeatureIndex]);

		public override string ToString() => $"f[{FeatureIndex}]={Value}";

		public override int GetHashCode() => HashCode.Combine(FeatureIndex, Value);

		public override bool Equals(object? obj) => Equals((BinaryFeatureTest) obj!);

		public bool Equals(IFeatureTest test) => Equals((BinaryFeatureTest) test);

		public bool Equals(BinaryFeatureTest other) {
			return
				other.FeatureIndex == FeatureIndex &&
				other.Value == Value;
		}
	}
}
