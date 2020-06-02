namespace Minotaur.EvolutionaryAlgorithms.Population {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Threading;
	using Minotaur.Classification;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;

	public sealed class Individual: IEquatable<Individual> {

		private static long _individualsCreated = 0;

		public readonly long ParentId;
		public readonly long Id;
		public readonly Array<Rule> Rules;
		public readonly ILabel DefaultPrediction;
		private readonly int _precomputedHashCode;

		public Individual(Array<Rule> rules, ILabel defaultPrediction) {
			if (rules.IsEmpty)
				throw new ArgumentException(nameof(rules) + " can't be empty.");

			// Checking for nulls and creating shallow copy
			var rulesContainer = new Rule[rules.Length];
			for (int i = 0; i < rulesContainer.Length; i++) {
				var r = rules[i];
				if (r is null)
					throw new ArgumentException(nameof(rules) + " can't contain nulls.");

				rulesContainer[i] = r;
			}

			// Checking for duplicates
			if (new HashSet<Rule>(rulesContainer).Count != rulesContainer.Length)
				throw new ArgumentException(nameof(rules) + " cant contain duplicate rules.");

			// @TODO: add expensive-sanity-check that checks if the set is consistent 

			Rules = Array<Rule>.Wrap(rulesContainer);
			DefaultPrediction = defaultPrediction;
			Id = Interlocked.Increment(ref _individualsCreated);

			// Precomputing hash
			var hash = new HashCode();
			hash.Add(defaultPrediction);
			for (int i = 0; i < Rules.Length; i++)
				hash.Add(Rules[i]);

			_precomputedHashCode = hash.ToHashCode();
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

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object? obj) => Equals((Individual) obj!);

		public bool Equals([AllowNull] Individual other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			if (ReferenceEquals(this, other))
				return true;

			if (ReferenceEquals(Rules, other.Rules) &&
				ReferenceEquals(DefaultPrediction, other.DefaultPrediction)) {
				return true;
			}

			if (!DefaultPrediction.Equals(other.DefaultPrediction))
				return false;

			return new HashSet<Rule>(Rules).SetEquals(other.Rules);
		}
	}
}
