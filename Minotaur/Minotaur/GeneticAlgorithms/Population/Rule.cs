namespace Minotaur.GeneticAlgorithms.Population {
	using System;
	using System.Linq;
	using System.Text;
	using Minotaur.Collections;

	// @Assumption: all rules contain Antecedents with the same length.
	// @Assumption: all rules contain Consequents with the same length.
	public sealed class Rule: IEquatable<Rule> {
		public const int MinimumTestCount = 1;

		public readonly int NonNullTestCount;

		/// <summary>
		/// Contains N references to tests, with N == dataset.FeatureCount.
		/// The tests are sorted by the feature index they are testing.
		/// </summary>
		public readonly Array<IFeatureTest> Antecedent;
		public readonly Array<bool> Consequent;

		private readonly int _precomputedHashCode;

		public Rule(Array<IFeatureTest> antecedent, Array<bool> consequent) {
			Antecedent = antecedent;
			Consequent = consequent;

			NonNullTestCount = 0;
			for (int i = 0; i < antecedent.Length; i++) {
				var currentTest = antecedent[i];

				if (currentTest is null)
					throw new ArgumentException(nameof(antecedent) + " can't contain nulls.");

				if (currentTest.FeatureIndex != i)
					throw new ArgumentException(nameof(antecedent) + " must be sorted and can not contain multiple tests for the same feature");

				if (!(currentTest is NullFeatureTest))
					NonNullTestCount += 1;
			}

			if (NonNullTestCount < MinimumTestCount) {
				throw new ArgumentException(
					nameof(antecedent) + $" must contain at least {MinimumTestCount} " +
					$"tests that are not {nameof(NullFeatureTest)}.");
			}

			_precomputedHashCode = PrecompileHashcode(antecedent, consequent);

			static int PrecompileHashcode(Array<IFeatureTest> antecedent, Array<bool> consequent) {
				var hash = new HashCode();

				for (int i = 0; i < antecedent.Length; i++)
					hash.Add(antecedent[i]);

				for (int i = 0; i < consequent.Length; i++)
					hash.Add(consequent[i]);

				return hash.ToHashCode();
			}
		}

		public bool Covers(Array<float> instance) {
			if (instance.Length != Antecedent.Length)
				throw new ArgumentException(nameof(instance));

			for (int i = 0; i < Antecedent.Length; i++) {
				if (!Antecedent[i].Matches(instance))
					return false;
			}

			return true;
		}

		public override string ToString() {
			var builder = new StringBuilder();

			var relevantTests = Antecedent.Where(t => !(t is NullFeatureTest));

			var antecedent = "IF " + string.Join(" AND ", relevantTests);
			var consequent = " THEN " + Consequent.ToString();
			return antecedent + consequent;
		}

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object? obj) => Equals((Rule) obj!);

		public bool Equals(Rule other) {
			if (ReferenceEquals(this, other))
				return true;

			if (_precomputedHashCode != other._precomputedHashCode)
				return false;

			return Consequent.SequenceEquals(other.Consequent) &&
				Antecedent.SequenceEquals(other.Antecedent);
		}
	}
}
