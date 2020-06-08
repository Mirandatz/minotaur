namespace Minotaur.Theseus {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.Classification;
	using Minotaur.Classification.Rules;

	public sealed class ConsistentClassificationModel: IEquatable<ConsistentClassificationModel> {

		public readonly RuleSet Rules;
		public readonly ILabel DefaultPrediction;
		private readonly int _precomputedHashCode;

		public ConsistentClassificationModel(RuleSet rules, ILabel defaultPrediction) {
			Rules = rules;
			DefaultPrediction = defaultPrediction;
			_precomputedHashCode = HashCode.Combine(rules, defaultPrediction);
		}

		public static ConsistentClassificationModel Create(RuleSet rules, ILabel defaultPrediction, RuleConsistencyChecker consistencyChecker) {
			if (!consistencyChecker.AreConsistent(rules.AsSpan()))
				throw new ArgumentException(nameof(rules) + " must be consistent.");

			return new ConsistentClassificationModel(rules, defaultPrediction);
		}

		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object? obj) => Equals((ConsistentClassificationModel) obj!);

		public bool Equals([AllowNull] ConsistentClassificationModel other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			if (ReferenceEquals(this, other))
				return true;

			if (!DefaultPrediction.Equals(other.DefaultPrediction))
				return false;

			return Rules.Equals(other.Rules);
		}
	}
}
