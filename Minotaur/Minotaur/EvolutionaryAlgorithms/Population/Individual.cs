namespace Minotaur.EvolutionaryAlgorithms.Population {
	using System;
	using System.Threading;
	using Minotaur.Classification;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;

	/// <summary>
	/// This class represents a "barebones" rule-based classifier.
	/// It provides the minimum amount of safety nets;
	/// it DOES NOT check whether the rules consistent.
	/// </summary>
	public sealed class Individual: IEquatable<Individual> {
		private const int MinimumRuleCount = 1;
		private const long AllFatherId = 0;

		private static long _individualsCreated = 0;

		public readonly long ParentId;
		public readonly long Id;
		public readonly Array<Rule> Rules;
		public readonly ILabel DefaultPrediction;

		private readonly int _hashCode;

		public Individual(long parentId, Array<Rule> rules, ILabel defaultPrediction) {
			if (rules.Length < MinimumRuleCount)
				throw new ArgumentException(nameof(rules) + $" must contain at least {MinimumRuleCount} rules.");
			if (rules.ContainsNulls())
				throw new ArgumentException(nameof(rules) + " can't contain nulls.");

			Id = Interlocked.Increment(ref _individualsCreated);
			ParentId = parentId;
			Rules = rules;
			DefaultPrediction = defaultPrediction;

			// We only use rules and labels to compute hash and equality
			// to allow "clones" in the population
			var hash = new HashCode();
			hash.Add(defaultPrediction);
			for (int i = 0; i < rules.Length; i++)
				hash.Add(rules[i]);

			_hashCode = hash.ToHashCode();
		}

		public static Individual CreateFirstGenerationIndividual(Array<Rule> rules, ILabel defaultPrediction) {
			if (rules.Length < MinimumRuleCount)
				throw new ArgumentException(nameof(rules) + $" must contain at least {MinimumRuleCount} rules.");
			if (rules.ContainsNulls())
				throw new ArgumentException(nameof(rules) + " can't contain nulls.");

			return new Individual(
				parentId: AllFatherId,
				rules: rules,
				defaultPrediction: defaultPrediction);
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
