namespace Minotaur.EvolutionaryAlgorithms.Population {
	using System;
	using Minotaur.Classification;
	using Minotaur.Collections;

	public sealed class Rule {
		public const int MinimumTestCount = 1;

		public readonly int NonNullTestCount;
		public readonly Array<IFeatureTest> Antecedent;
		public readonly ILabel Consequent;
		public readonly int PrecomputedHashCode;

		public Rule(Array<IFeatureTest> antecedent, ILabel consequent) {
			if (antecedent.ContainsNulls())
				throw new ArgumentException(nameof(antecedent) + " can't contain nulls.");

			Antecedent = antecedent;
			Consequent = consequent;

			for (int i = 0; i < antecedent.Length; i++) {
				var currentTest = antecedent[i];

				if (currentTest is null)
					throw new ArgumentException(nameof(antecedent) + " can't contain nulls.");

				if (currentTest.FeatureIndex != i)
					throw new ArgumentException(nameof(antecedent) + " must be sorted and can not contain multiple tests for the same feature.");
			}

			PrecomputedHashCode = PrecompileHashcode(antecedent, consequent);

			static int PrecompileHashcode(Array<IFeatureTest> antecedent, ILabel consequent) {
				var hash = new HashCode();

				for (int i = 0; i < antecedent.Length; i++)
					hash.Add(antecedent[i]);

				hash.Add(consequent);

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
			var antecedent = "IF " + string.Join(" AND ", Antecedent);
			var consequent = " THEN " + Consequent.ToString();
			return antecedent + consequent;
		}

		public override int GetHashCode() => throw new NotImplementedException();
		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
