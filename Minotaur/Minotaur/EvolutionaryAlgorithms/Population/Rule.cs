namespace Minotaur.EvolutionaryAlgorithms.Population {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.Classification;
	using Minotaur.Collections;

	public sealed class Rule: IEquatable<Rule> {

		public readonly int NonNullTestCount;
		public readonly RuleAntecedent Antecedent;
		public readonly ILabel Consequent;
		private readonly int _precomputedHashCode;

		public Rule(RuleAntecedent antecedent, ILabel consequent) {
			Antecedent = antecedent;
			Consequent = consequent;
			_precomputedHashCode = HashCode.Combine(antecedent, consequent);
		}

		public bool Covers(Array<float> instance) => Antecedent.Covers(instance);

		public override string ToString() {
			var antecedent = "IF " + string.Join(" AND ", Antecedent);
			var consequent = " THEN " + Consequent.ToString();
			return antecedent + consequent;
		}

		public override int GetHashCode() => _precomputedHashCode;

		public override bool Equals(object? obj) => Equals((Rule) obj!);

		public bool Equals([AllowNull] Rule other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			if (ReferenceEquals(this, other))
				return true;

			return Antecedent.Equals(other.Antecedent) &&
				Consequent.Equals(other.Consequent);
		}
	}
}
