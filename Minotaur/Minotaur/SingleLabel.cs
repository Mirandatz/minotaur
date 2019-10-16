namespace Minotaur {
	using System;

	public sealed class SingleLabel: ILabel, IEquatable<SingleLabel> {

		public readonly float Value;

		public SingleLabel(float value) {
			if (float.IsNaN(value))
				throw new ArgumentOutOfRangeException(nameof(value) + " can't be NaN.");
			if (float.IsInfinity(value))
				throw new ArgumentOutOfRangeException(nameof(value) + " must be finite.");

			Value = value;
		}

		public override int GetHashCode() => Value.GetHashCode();

		public override bool Equals(object? obj) => Equals((SingleLabel) obj!);

		public bool Equals(ILabel ruleConsequent) => Equals((SingleLabel) ruleConsequent);

		public bool Equals(SingleLabel other) => Value == other.Value;
	}
}
