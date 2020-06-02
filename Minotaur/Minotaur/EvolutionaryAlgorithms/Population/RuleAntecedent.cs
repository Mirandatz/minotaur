namespace Minotaur.EvolutionaryAlgorithms.Population {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.Collections;

	public sealed class RuleAntecedent: IEquatable<RuleAntecedent>, IReadOnlyList<IFeatureTest> {

		public readonly Array<IFeatureTest> FeatureTests;
		public int Count => FeatureTests.Length;
		private readonly int _precomputedHashCode;

		public RuleAntecedent(Array<IFeatureTest> featureTests) {
			if (featureTests.Length == 0)
				throw new ArgumentException(nameof(featureTests) + " can't be empty.");

			var storage = new IFeatureTest[featureTests.Length];

			for (int i = 0; i < storage.Length; i++) {
				var ft = featureTests[i];

				if (ft is null)
					throw new ArgumentException(nameof(featureTests) + " can't contain nulls.");
				if (ft.FeatureIndex != i)
					throw new ArgumentException(nameof(featureTests) + $" contents must be sorted by {nameof(IFeatureTest.FeatureIndex)}.");

				storage[i] = ft;
			}

			FeatureTests = Array<IFeatureTest>.Wrap(storage);

			var hash = new HashCode();
			for (int i = 0; i < featureTests.Length; i++)
				hash.Add(featureTests[i].GetHashCode());

			_precomputedHashCode = hash.ToHashCode();
		}

		public IFeatureTest this[int index] => FeatureTests[index];

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object? obj) => Equals((RuleAntecedent) obj!);

		public bool Equals([AllowNull] RuleAntecedent other) {
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

		public IEnumerator<IFeatureTest> GetEnumerator() => FeatureTests.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => FeatureTests.GetEnumerator();
	}
}
