namespace Minotaur.Classification.Rules {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.ExtensionMethods.SystemArray;

	public sealed class Antecedent: IEquatable<Antecedent>, IReadOnlyList<FeatureTest> {

		private readonly FeatureTest[] _featureTests;
		private readonly int _precomputedHashCode;

		public Antecedent(ReadOnlySpan<FeatureTest> featureTests) {
			if (featureTests.IsEmpty)
				throw new ArgumentException(nameof(featureTests) + " can't be empty.");

			var storage = new FeatureTest[featureTests.Length];
			var hash = new HashCode();

			// Checking nulls and indices are precomputing the hashcode
			for (int i = 0; i < storage.Length; i++) {
				var ft = featureTests[i];

				if (ft is null)
					throw new ArgumentException(nameof(featureTests) + " can't contain nulls.");

				if (ft.FeatureIndex != i)
					throw new ArgumentException("Index mismatch.");

				storage[i] = ft;
				hash.Add(ft);
			}

			_featureTests = storage;
			_precomputedHashCode = hash.ToHashCode();
		}

		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object? obj) => Equals((Antecedent) obj!);

		public bool Equals([AllowNull] Antecedent other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			if (ReferenceEquals(this, other))
				return true;

			if (ReferenceEquals(_featureTests, other._featureTests))
				return true;

			if (Count != other.Count)
				throw new InvalidOperationException();

			var lhs = _featureTests.AsSpan();
			var rhs = other._featureTests.AsSpan();

			for (int i = 0; i < Count; i++) {
				if (!lhs[i].Equals(rhs[i]))
					return false;
			}

			return true;
		}

		public int Count => _featureTests.Length;

		public FeatureTest this[int index] => _featureTests[index];

		public IEnumerator<FeatureTest> GetEnumerator() => _featureTests.GetGenericEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => _featureTests.GetEnumerator();
	}
}
