namespace Minotaur.Classification.Rules {
	using System;
	using System.Diagnostics.CodeAnalysis;

	public sealed class FeatureTest: IEquatable<FeatureTest> {

		public readonly float LowerBound;
		public readonly float UpperBound;

		public FeatureTest(float lowerBound, float upperBound) {
			if (float.IsNaN(lowerBound))
				throw new ArgumentOutOfRangeException(nameof(lowerBound) + " can't be NaN.");
			if (float.IsNaN(upperBound))
				throw new ArgumentOutOfRangeException(nameof(upperBound) + " can't be NaN.");

			if (lowerBound >= upperBound)
				throw new ArgumentException(nameof(upperBound) + " must be greater than " + nameof(lowerBound));

			LowerBound = lowerBound;
			UpperBound = upperBound;
		}

		public static FeatureTest FromUnsortedBounds(float firstBound, float secondBound) {
			if (firstBound < secondBound) {
				return new FeatureTest(
					lowerBound: firstBound,
					upperBound: secondBound);
			} else {
				return new FeatureTest(
					lowerBound: secondBound,
					upperBound: firstBound);
			}
		}

		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => HashCode.Combine(LowerBound, UpperBound);

		public override bool Equals(object? obj) => Equals((FeatureTest) obj!);

		public bool Equals([AllowNull] FeatureTest other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			return LowerBound == other.LowerBound &&
				UpperBound == other.UpperBound;
		}
	}
}
