namespace Minotaur.GeneticAlgorithms.Population {
	using System;
	using Minotaur.Collections;
	using Newtonsoft.Json;

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class Rule: IEquatable<Rule> {
		public const int MinimumTestCount = 1;

		public readonly int NonNullTestCount;

		/// <summary>
		/// Contains N references to tests, with N == dataset.FeatureCount.
		/// The tests are sorted by the feature index they are testing.
		/// </summary>
		[JsonProperty] public readonly Array<IFeatureTest> Tests;
		[JsonProperty] public readonly Array<bool> PredictedLabels;

		private readonly int _precomputedHashCode;

		[JsonConstructor]
		public Rule(Array<IFeatureTest> tests, Array<bool> predictedLabels) {
			Tests = tests ?? throw new ArgumentNullException(nameof(tests));
			PredictedLabels = predictedLabels ?? throw new ArgumentNullException(nameof(predictedLabels));

			NonNullTestCount = 0;
			for (int i = 0; i < tests.Length; i++) {
				var currentTest = tests[i];

				if (currentTest is null)
					throw new ArgumentException(nameof(tests) + " can't contain nulls.");

				if (currentTest.FeatureIndex != i)
					throw new ArgumentException(nameof(tests) + " must be sorted and can not contain multiple tests for the same feature");

				if (!(currentTest is NullFeatureTest))
					NonNullTestCount += 1;
			}

			if (NonNullTestCount < MinimumTestCount) {
				throw new ArgumentException(
					nameof(tests) + $" must contain at least {MinimumTestCount} " +
					$"tests that are not {nameof(NullFeatureTest)}.");
			}

			_precomputedHashCode = PrecompileHashcode(tests, predictedLabels);
		}

		private int PrecompileHashcode(Array<IFeatureTest> tests, Array<bool> predictedLabels) {
			var hash = new HashCode();

			for (int i = 0; i < tests.Length; i++)
				hash.Add(tests[i]);

			for (int i = 0; i < predictedLabels.Length; i++)
				hash.Add(predictedLabels[i]);

			return hash.ToHashCode();
		}

		public bool Covers(Array<float> instance) {
			for (int i = 0; i < Tests.Length; i++) {
				if (!Tests[i].Matches(instance))
					return false;
			}

			return true;
		}

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object obj) => Equals(obj as Rule);

		public bool Equals(Rule other) {
			// We check ReferenceEquals before checking for nulls because we don't expect to
			// compare to nulls (ever)
			if (ReferenceEquals(this, other))
				return true;
			if (other == null)
				return false;

			// We don't check _predictedLabels's length coz they REALLY REALLY 
			// should ALWAYS have the same size
			return Tests.SequenceEquals(other.Tests) &&
				PredictedLabels.SequenceEquals(other.PredictedLabels);
		}
	}
}
