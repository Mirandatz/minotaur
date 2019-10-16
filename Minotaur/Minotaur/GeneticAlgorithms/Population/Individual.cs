namespace Minotaur.GeneticAlgorithms.Population {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;

	/// <summary>
	/// This class represents a "barebones" rule-based classifier.
	/// It provides the minimum amount of safety nets;
	/// it DOES NOT check whether the rules consistent.
	/// </summary>
	public sealed class Individual: IEquatable<Individual> {
		public const int MinimumRuleCount = 1;

		public readonly Array<Rule> Rules;
		public readonly ILabel DefaultPrediction;

		private readonly int _hashCode;

		public Individual(Array<Rule> rules, ILabel defaultPrediction) {
			if (rules.Length < MinimumRuleCount)
				throw new ArgumentException(nameof(rules) + $" must contain at least {MinimumRuleCount} rules.");
			if (rules.ContainsNulls())
				throw new ArgumentException(nameof(rules) + " can't contain nulls.");

			Rules = rules;
			DefaultPrediction = defaultPrediction;

			var hash = new HashCode();
			for (int i = 0; i < rules.Length; i++)
				hash.Add(rules[i]);

			hash.Add(defaultPrediction);

			_hashCode = hash.ToHashCode();
		}

		public ILabel Predict(Array<float> instance) {
			for (int i = 0; i < Rules.Length; i++) {
				if (Rules[i].Covers(instance)) {
					return Rules[i].Consequent;
				}
			}

			return DefaultPrediction;
		}

		public ILabel[] Predict(Dataset dataset) {
			var instanceCount = dataset.InstanceCount;
			var predictions = new ILabel[instanceCount];

			for (int i = 0; i < instanceCount; i++) {
				var instanceData = dataset.GetInstanceData(i);
				var prediction = Predict(instanceData);
				predictions[i] = prediction;
			}

			return predictions;
		}

		public override string ToString() {
			var rules = string.Join(Environment.NewLine, Rules);
			return rules + Environment.NewLine + $"Default: {DefaultPrediction}";
		}

		public override int GetHashCode() => _hashCode;

		public override bool Equals(object? obj) => Equals((Individual) obj!);

		/// <remarks>
		/// Individuals are compared using reference equality instead
		/// of "semantic equality" in order to allow the existance of
		/// "clones" in the genetic algorithm population.
		/// </remarks>
		public bool Equals(Individual other) => ReferenceEquals(this, other);
	}
}
