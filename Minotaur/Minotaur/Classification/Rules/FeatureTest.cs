namespace Minotaur.Classification.Rules {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.Math.Geometry;

	public sealed class FeatureTest: IEquatable<FeatureTest> {

		public readonly int FeatureIndex;
		public readonly Interval Interval;

		public FeatureTest(int featureIndex, Interval interval) {
			if (FeatureIndex < 0)
				throw new ArgumentOutOfRangeException(nameof(featureIndex) + " must be >= 0.");

			FeatureIndex = featureIndex;
			Interval = interval;
		}

		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => HashCode.Combine(FeatureIndex, Interval);

		public override bool Equals(object? obj) => Equals((FeatureTest) obj!);

		public bool Equals([AllowNull] FeatureTest other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			return FeatureIndex == other.FeatureIndex &&
				Interval.Equals(other.Interval);
		}
	}
}
