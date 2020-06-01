namespace Minotaur.Classification {
	using System;
	using System.Diagnostics.CodeAnalysis;

	public sealed class SingleLabel: ILabel, IEquatable<SingleLabel> {

		public readonly int Value;

		public SingleLabel(int value) {
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value) + " must be >= 0.");

			Value = value;
		}

		public override string ToString() => Value.ToString();

		public override int GetHashCode() => Value.GetHashCode();
		public override bool Equals(object? obj) => Equals((SingleLabel) obj!);
		public bool Equals([AllowNull] ILabel other) => Equals((SingleLabel) other!);

		public bool Equals([AllowNull] SingleLabel other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			return Value == other.Value;
		}
	}
}
