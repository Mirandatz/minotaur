namespace Minotaur.Classification {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Threading.Tasks;
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

			var expectedLabelCount = ruleSet.AsSpan()[0].Consequent.Count;
			if (defaultPrediction.Count != expectedLabelCount) {
				throw new ArgumentException($"The {nameof(defaultPrediction)} must have the same " +
					$"{nameof(defaultPrediction.Count)} as each of the {nameof(Rule)} in the " +
					$"{nameof(ruleSet)}.");
			}

			return new ConsistentModel(
				rules: ruleSet,
				defaultPrediction: defaultPrediction);
		}

		// Actual methods
		private Rule? GetRuleThatCovers(InstanceFeatures instanceFeatures) {
			// @performance
			// We could break out of loop early...
			// As soon as we found the first matching rule...
			// But we will keep iterating to make sure(er?)
			// there is at most a single rule that matches the instance
			var rules = Rules.AsSpan();

			Rule? ruleThatCovers = null;

			for (int i = 0; i < rules.Length; i++) {
				var r = rules[i];

				if (!r.Antecedent.Covers(instanceFeatures))
					continue;

				if (ruleThatCovers is null)
					ruleThatCovers = r;
				else
					throw new InvalidOperationException("This model is totally not consistent!");
			}

			return ruleThatCovers;
		}

		public bool Covers(InstanceFeatures instanceFeatures) {
			var rule = GetRuleThatCovers(instanceFeatures);
			return !(rule is null);
		}

		public InstanceLabels[] Predict(InstancesFeaturesManager instancesFeaturesManager) {
			var predictions = new InstanceLabels[instancesFeaturesManager.InstanceCount];

			Parallel.For(
				fromInclusive: 0,
				toExclusive: instancesFeaturesManager.InstanceCount,
				body: instanceIndex => {
					var instanceFeatures = instancesFeaturesManager.GetFeatures(instanceIndex);
					var p = Predict(instanceFeatures);
					predictions[instanceIndex] = p;
				});

			return predictions;
		}

		private InstanceLabels Predict(InstanceFeatures instanceFeatures) {
			var rule = GetRuleThatCovers(instanceFeatures);

			Consequent consequent;
			if (rule is null)
				consequent = DefaultPrediction;
			else
				consequent = rule.Consequent;

			return new InstanceLabels(values: consequent.AsSpan());
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
