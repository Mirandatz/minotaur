namespace Minotaur.EvolutionaryAlgorithms.Population {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;

	public sealed class ContinuousFeatureTestSemanticEqualityComparer: IEqualityComparer<IFeatureTest> {

		public bool Equals([AllowNull] IFeatureTest lhs, [AllowNull] IFeatureTest rhs) {
			return Equals(lhs: (ContinuousFeatureTest) lhs!, rhs: (ContinuousFeatureTest) rhs!);

			static bool Equals(ContinuousFeatureTest lhs, ContinuousFeatureTest rhs) {
				return lhs.FeatureIndex == rhs.FeatureIndex &&
					lhs.LowerBound == rhs.LowerBound &&
					lhs.UpperBound == rhs.UpperBound;
			}
		}

		public int GetHashCode([DisallowNull] IFeatureTest obj) {
			var cft = (ContinuousFeatureTest) obj!;
			return HashCode.Combine(cft.FeatureIndex, cft.LowerBound, cft.UpperBound);
		}
	}
}
