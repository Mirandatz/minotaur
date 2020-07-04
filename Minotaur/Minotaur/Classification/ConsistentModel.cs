namespace Minotaur.Classification {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using Minotaur.Classification.Rules;
	using Minotaur.Datasets;

	public sealed class ConsistentModel: IEquatable<ConsistentModel> {

		public readonly RuleSet Rules;
		public readonly Consequent DefaultPrediction;

		// Constructors and alike
		private ConsistentModel(RuleSet rules, Consequent defaultPrediction) {
			Rules = rules;
			DefaultPrediction = defaultPrediction;
		}

		public static ConsistentModel Create(RuleSet ruleSet, Consequent defaultPrediction, IConsistencyChecker consistencyChecker) {
			if (!consistencyChecker.AreConsistent(ruleSet))
				throw new InvalidOperationException();

			var expectedConsequentCount = ruleSet.AsSpan()[0].Consequent.Count;
			if (defaultPrediction.Count != expectedConsequentCount) {
				throw new ArgumentException($"The {nameof(defaultPrediction)} must have the same " +
					$"{nameof(defaultPrediction.Count)} as each of the {nameof(Rule)} in the " +
					$"{nameof(ruleSet)}.");
			}

			return new ConsistentModel(
				rules: ruleSet,
				defaultPrediction: defaultPrediction);
		}

		// Actual methods
		public InstanceLabels Predict(InstanceFeatures instaceFeatures) {

			var rules = Rules.AsSpan();

			// We could break out of loop early,
			// as soon as we found the first matching rule...
			// But we will keep iterating to make sure(er?)
			// there is at most a single rule that matches the instance

			throw new NotImplementedException();
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => HashCode.Combine(Rules, DefaultPrediction);

		public override bool Equals(object? obj) => Equals((ConsistentModel) obj!);

		// IEquatable
		public bool Equals([AllowNull] ConsistentModel other) {
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
