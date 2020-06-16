namespace Minotaur.Classification.Rules {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.Collections;

	public sealed class RuleSet: IEquatable<RuleSet> {

		private readonly Rule[] _rulesArray;
		private readonly Set<Rule> _rulesSet;
		private readonly int _precomputedHashCode;
		public int Count => _rulesSet.Count;

		private RuleSet(Rule[] rulesArray, Set<Rule> rulesSet, int precomputedHashCode) {
			_rulesArray = rulesArray;
			_rulesSet = rulesSet;
			_precomputedHashCode = precomputedHashCode;
		}

		public static RuleSet Create(ReadOnlySpan<Rule> rules) {
			if (rules.Length == 0)
				throw new ArgumentException(nameof(rules) + " can't be empty.");

			var array = new Rule[rules.Length];
			var set = new HashSet<Rule>(capacity: rules.Length);
			var hash = new HashCode();

			var expectedTestCount = rules[0].Antecedent.Count;

			for (int i = 0; i < rules.Length; i++) {
				var r = rules[i];

				if (r is null)
					throw new ArgumentNullException(nameof(rules) + " can't contain nulls.");

				if (r.Antecedent.Count != expectedTestCount)
					throw new ArgumentException("All rules must have the same number of FeatureTests.");

				var unique = set.Add(r);
				if (!unique)
					throw new ArgumentException(nameof(rules) + " can't contain duplicates.");

				array[i] = r;
				hash.Add(r);
			}

			return new RuleSet(
				rulesArray: array,
				rulesSet: Set<Rule>.Wrap(set),
				precomputedHashCode: hash.ToHashCode());
		}

		public bool IsEmpty => _rulesArray.Length == 0;

		public ReadOnlySpan<Rule> AsSpan() => _rulesArray;

		public Set<Rule> AsSet() => _rulesSet;

		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object? obj) => Equals((RuleSet) obj!);

		public bool Equals([AllowNull] RuleSet other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			if (ReferenceEquals(this, other))
				return true;

			return _rulesSet.SetEquals(other._rulesSet);
		}
	}
}
