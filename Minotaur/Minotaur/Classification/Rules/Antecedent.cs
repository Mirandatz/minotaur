namespace Minotaur.Classification.Rules {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.Collections;

	public sealed class Antecedent: IEquatable<Antecedent>, IReadOnlyList<IFeatureTest> {

		public readonly Array<IFeatureTest> FeatureTests;
		private readonly int _precomputedHashCode;

		public Antecedent(Array<IFeatureTest> featureTests) {
			if (featureTests.IsEmpty)
				throw new ArgumentException(nameof(featureTests) + " can't be empty.");

			var storage = new IFeatureTest[featureTests.Length];
			var hash = new HashCode();

			// Checking nulls and indices are precomputing the hashcode
			for (int i = 0; i < storage.Length; i++) {
				var ft = featureTests[i];

				if (ft is null)
					throw new ArgumentException(nameof(featureTests) + " can't contain nulls.");

				if (ft.FeatureIndex != i) {
					throw new ArgumentException($"There is a mis-indexed {nameof(IFeatureTest)}." +
						$" Expected index {i}, actual {ft.FeatureIndex}.");
				}

				storage[i] = ft;
				hash.Add(ft);
			}

			FeatureTests = Array<IFeatureTest>.Wrap(storage);
			_precomputedHashCode = hash.ToHashCode();
		}

		public bool Covers(Array<float> instance) {
			if (instance.Length != Count)
				throw new InvalidOperationException();

			var tests = FeatureTests.AsSpan();
			for (int i = 0; i < tests.Length; i++) {
				if (!tests[i].Matches(instance))
					return false;
			}

			return true;
		}

		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object? obj) => Equals((Antecedent) obj!);

		public bool Equals([AllowNull] Antecedent other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			if (ReferenceEquals(this, other))
				return true;

			if (ReferenceEquals(FeatureTests, other.FeatureTests))
				return true;

			if (Count != other.Count)
				throw new InvalidOperationException();

			var lhs = FeatureTests.AsSpan();
			var rhs = other.FeatureTests.AsSpan();

			for (int i = 0; i < Count; i++) {
				if (!lhs[i].Equals(rhs[i]))
					return false;
			}

			return true;
		}

		public int Count => FeatureTests.Length;

		public IFeatureTest this[int index] => FeatureTests[index];

		public IEnumerator<IFeatureTest> GetEnumerator() => FeatureTests.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => FeatureTests.GetEnumerator();
	}
}
