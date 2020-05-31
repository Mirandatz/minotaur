namespace Minotaur.EvolutionaryAlgorithms.Population {
	using System;
	using System.Threading;
	using Minotaur.Classification;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;

	public sealed class Individual {
		private const int MinimumRuleCount = 1;

		private static long _individualsCreated = 0;

		public readonly long ParentId;
		public readonly long Id;
		public readonly Array<Rule> Rules;
		public readonly ILabel DefaultPrediction;
		public readonly int PrecomputedHashCode;

		public Individual(Array<Rule> rules, ILabel defaultPrediction) {
			if (rules.Length < MinimumRuleCount)
				throw new ArgumentException(nameof(rules) + $" must contain at least {MinimumRuleCount} rules.");
			if (rules.ContainsNulls())
				throw new ArgumentException(nameof(rules) + " can't contain nulls.");

			Id = Interlocked.Increment(ref _individualsCreated);
			Rules = rules;
			DefaultPrediction = defaultPrediction;

			// We only use rules and labels to compute hash and equality
			// to allow "clones" in the population
			var hash = new HashCode();
			hash.Add(defaultPrediction);
			for (int i = 0; i < rules.Length; i++)
				hash.Add(rules[i]);

			PrecomputedHashCode = hash.ToHashCode();
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

		public override int GetHashCode() => throw new NotImplementedException();
		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
