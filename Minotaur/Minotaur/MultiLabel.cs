namespace Minotaur {
	using System;
	using Minotaur.Collections;

	public sealed class MultiLabel: ILabel, IEquatable<MultiLabel> {

		public readonly Array<bool> Values;

		private readonly int _hashCode;

		private MultiLabel(Array<bool> values) {
			Values = values;

			var hash = new HashCode();
			for (int i = 0; i < Values.Length; i++)
				hash.Add(Values[i]);

			_hashCode = hash.ToHashCode();
		}

		public static MultiLabel Parse(Span<float> values) {
			var labels = new bool[values.Length];

			for (int i = 0; i < values.Length; i++) {
				labels[i] = (values[i]) switch
				{
					0f => false,
					1f => true,
					_ => throw new InvalidOperationException(nameof(values) + " contains non binary values."),
				};
			}

			return new MultiLabel(labels);
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
