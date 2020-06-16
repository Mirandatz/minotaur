namespace Minotaur.Classification.Rules {
	using System;
	using System.Diagnostics.CodeAnalysis;

	public sealed class FeatureTest: IEquatable<FeatureTest> {

		public readonly int FeatureIndex;
		public readonly float LowerBound;
		public readonly float UpperBound;

		public FeatureTest(int featureIndex, float lowerBound, float upperBound) {
			if (FeatureIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + " must be >= 0.");
			if (float.IsNaN(lowerBound))
				throw new ArgumentOutOfRangeException(nameof(lowerBound) + " can't be NaN.");
			if (float.IsNaN(upperBound))
				throw new ArgumentOutOfRangeException(nameof(upperBound) + " can't be NaN.");

			if (lowerBound >= upperBound)
				throw new ArgumentException(nameof(upperBound) + " must be greater than " + nameof(lowerBound));

			FeatureIndex = featureIndex;
			LowerBound = lowerBound;
			UpperBound = upperBound;
		}

		public static FeatureTest FromUnsortedBounds(int featureIndex, float firstBound, float secondBound) {
			if (firstBound < secondBound) {
				return new FeatureTest(
					featureIndex: featureIndex,
					lowerBound: firstBound,
					upperBound: secondBound);
			} else {
				return new FeatureTest(
					featureIndex: featureIndex,
					lowerBound: secondBound,
					upperBound: firstBound);
			}
		}

		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => HashCode.Combine(FeatureIndex, LowerBound, UpperBound);

		public override bool Equals(object? obj) => Equals((FeatureTest) obj!);

		public bool Equals([AllowNull] FeatureTest other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			return FeatureIndex == other.FeatureIndex &&
				LowerBound == other.LowerBound &&
				UpperBound == other.UpperBound;
		}
	}
}
