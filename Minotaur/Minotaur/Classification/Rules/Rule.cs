namespace Minotaur.Classification.Rules {
	using System;
	using System.Diagnostics.CodeAnalysis;

	public sealed class Rule: IEquatable<Rule> {

		public readonly Antecedent Antecedent;
		public readonly Consequent Consequent;
		private readonly int _precomputedHashCode;

		public Rule(Antecedent antecedent, Consequent consequent) {
			Antecedent = antecedent;
			Consequent = consequent;
			_precomputedHashCode = HashCode.Combine(antecedent, consequent);
		}

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
