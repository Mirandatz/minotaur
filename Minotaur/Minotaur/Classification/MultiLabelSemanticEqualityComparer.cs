namespace Minotaur.Classification {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;

	public sealed class MultiLabelSemanticEqualityComparer: IEqualityComparer<ILabel> {

		public bool Equals([AllowNull] ILabel lhs, [AllowNull] ILabel rhs) {
			return Equals(lhs: (MultiLabel) lhs!, rhs: (MultiLabel) rhs!);

			static bool Equals(MultiLabel lhs, MultiLabel rhs) {
				if (lhs.Length != rhs.Length)
					throw new InvalidOperationException();

				var lhsValues = lhs.Values.AsSpan();
				var rhsValues = rhs.Values.AsSpan();

				return lhsValues.SequenceEqual(rhsValues);
			}
		}

		public int GetHashCode([DisallowNull] ILabel obj) => ((MultiLabel) obj)!.PrecomputedHashCode;
	}
}
