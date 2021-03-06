namespace Minotaur.Classification.Rules {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.Collections;

	public sealed class ContinuousFeatureTest: IFeatureTest, IEquatable<ContinuousFeatureTest> {

		public int FeatureIndex { get; }
		public readonly float LowerBound;
		public readonly float UpperBound;
		public int TestSize => 2;

		public ContinuousFeatureTest(int featureIndex, float lowerBound, float upperBound) {
			if (featureIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + " must be >= 0");
			if (float.IsNaN(lowerBound))
				throw new ArgumentOutOfRangeException(nameof(lowerBound) + " can't be NaN.");
			if (float.IsNaN(upperBound))
				throw new ArgumentOutOfRangeException(nameof(upperBound) + " can't be NaN.");

			if (lowerBound >= upperBound)
				throw new ArgumentException(nameof(upperBound) + " must be greater than " + nameof(lowerBound));

			LowerBound = lowerBound;
			UpperBound = upperBound;
			FeatureIndex = featureIndex;
		}

		public static ContinuousFeatureTest FromUnsortedBounds(int featureIndex, float firstBound, float secondBound) {
			if (firstBound < secondBound) {
				return new ContinuousFeatureTest(
					featureIndex: featureIndex,
					lowerBound: firstBound,
					upperBound: secondBound);
			} else {
				return new ContinuousFeatureTest(
					featureIndex: featureIndex,
					lowerBound: secondBound,
					upperBound: firstBound);
			}
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

		public override string ToString() => $"{LowerBound} <= f[{FeatureIndex}] < {UpperBound}";

		public override int GetHashCode() => HashCode.Combine(FeatureIndex, LowerBound, UpperBound);

		public override bool Equals(object? obj) => Equals((ContinuousFeatureTest) obj!);

		public bool Equals([AllowNull] IFeatureTest other) => Equals((ContinuousFeatureTest) other!);

		public bool Equals([AllowNull] ContinuousFeatureTest other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			return FeatureIndex == other.FeatureIndex &&
				LowerBound == other.LowerBound &&
				UpperBound == other.UpperBound;
		}
	}
}
