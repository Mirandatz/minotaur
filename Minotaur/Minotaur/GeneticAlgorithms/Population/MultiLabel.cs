namespace Minotaur.GeneticAlgorithms.Population {
	using System;
	using Minotaur.Collections;

	public sealed class MultiLabel: ILabel, IEquatable<MultiLabel> {

		public readonly Array<bool> Values;

		private readonly int _hashCode;

		public MultiLabel(Array<bool> values) {
			Values = values.ShallowCopy();

			var hash = new HashCode();
			for (int i = 0; i < Values.Length; i++)
				hash.Add(Values[i]);

			_hashCode = hash.ToHashCode();
		}

		public override int GetHashCode() => _hashCode;

		public override bool Equals(object? obj) => Equals((MultiLabel) obj!);

		public bool Equals(ILabel ruleConsequent) => Equals((MultiLabel) ruleConsequent);

		public bool Equals(MultiLabel other) {
			return _hashCode == other._hashCode &&
				Values.SequenceEquals(other.Values);
		}
	}
}
