namespace Minotaur.GeneticAlgorithms.Population {
	using System;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Newtonsoft.Json;

	/// <summary>
	/// This class represents a "barebones" rule-based classifier.
	/// It provides the minimum amount of safety nets;
	/// it DOES NOT check whether the rules consistent.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class Individual: IEquatable<Individual> {
		public const int MinimumRuleCount = 1;

		[JsonProperty] public readonly Array<Rule> Rules;
		[JsonProperty] public readonly Array<bool> DefaultLabels;

		/// <summary>
		/// This field is public and serializable to make it easier to identify specific instances during
		/// "manual analysis".
		/// </summary>
		[JsonProperty] public readonly int HashCode;

		[JsonConstructor]
		public Individual(Array<Rule> rules, Array<bool> defaultLabels) {
			if (rules == null)
				throw new ArgumentNullException(nameof(rules));
			if (defaultLabels == null)
				throw new ArgumentNullException(nameof(defaultLabels));
			if (rules.Length < MinimumRuleCount)
				throw new ArgumentException(nameof(rules) + $" must contain at least {MinimumRuleCount} rules.");
			if (rules.ContainsNulls())
				throw new ArgumentException(nameof(rules) + " can't contain nulls.");
			if (defaultLabels.Length == 0)
				throw new ArgumentException(nameof(defaultLabels) + " can't be empty.");


			Rules = rules;
			DefaultLabels = defaultLabels;

			var hash = new HashCode();
			for (int i = 0; i < rules.Length; i++)
				hash.Add(rules[i]);
			for (int i = 0; i < defaultLabels.Length; i++)
				hash.Add(defaultLabels[i]);

			HashCode = hash.ToHashCode();
		}

		public Array<bool> Predict(ReadOnlySpan<float> instance) {
			for (int i = 0; i < Rules.Length; i++) {
				if (Rules[i].Covers(instance)) {
					return Rules[i].PredictedLabels;
				}
			}

			return DefaultLabels;
		}

		public Matrix<bool> Predict(Dataset dataset) {
			if (dataset == null)
				throw new ArgumentNullException(nameof(dataset));

			var instanceCount = dataset.InstanceCount;
			var classCount = dataset.ClassCount;
			var allPredictions = new MutableMatrix<bool>(
				rowCount: instanceCount,
				columnCount: classCount);

			for (int i = 0; i < instanceCount; i++) {
				var prediction = Predict(dataset.GetInstanceData(i));
				var row = allPredictions.GetRow(i);
				prediction.Span.CopyTo(row);
			}

			return allPredictions.ToMatrix();
		}

		public override int GetHashCode() => HashCode;

		public override bool Equals(object obj) => Equals(obj as Individual);

		public bool Equals(Individual other) => ReferenceEquals(this, other);
	}
}
