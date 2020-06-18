namespace Minotaur.Classification {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.Classification.Rules;

	public sealed class Model: IEquatable<Model> {

		public readonly RuleSet Rules;
		public readonly Consequent DefaultPrediction;

		private Model(RuleSet rules, Consequent defaultPrediction) {
			Rules = rules;
			DefaultPrediction = defaultPrediction;
		}

		public static Model Create(RuleSet ruleSet, Consequent defaultPrediction, IConsistencyChecker consistencyChecker) {
			if (!consistencyChecker.AreConsistent(ruleSet))
				throw new InvalidOperationException();

			var expectedConsequentCount = ruleSet.AsSpan()[0].Consequent.Count;
			if (defaultPrediction.Count != expectedConsequentCount) {
				throw new ArgumentException($"The {nameof(defaultPrediction)} must have the same " +
					$"{nameof(defaultPrediction.Count)} as each of the {nameof(Rule)} in the " +
					$"{nameof(ruleSet)}.");
			}

			return new Model(
				rules: ruleSet,
				defaultPrediction: defaultPrediction);
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => HashCode.Combine(Rules, DefaultPrediction);

		public override bool Equals(object? obj) => Equals((Model) obj!);

		public bool Equals([AllowNull] Model other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			if (ReferenceEquals(this, other))
				return true;

			if (ReferenceEquals(DefaultPrediction, other.DefaultPrediction) &&
				ReferenceEquals(Rules, other.Rules)) {
				return true;
			}

			return DefaultPrediction.Equals(other.DefaultPrediction) &&
				Rules.AsSet().SetEquals(other.Rules.AsSet());
		}
	}
}
