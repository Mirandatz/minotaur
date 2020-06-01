namespace Minotaur.Classification {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.Collections;

	public sealed class MultiLabel: ILabel, IEquatable<MultiLabel> {

		public readonly Array<bool> Values;
		public readonly int Length;
		private readonly int _precomputedHashCode;

		public MultiLabel(Array<bool> values) {
			if (values.Length == 0)
				throw new ArgumentException($"{nameof(values)} can't be empty.");

			Values = values.ShallowCopy();
			Length = Values.Length;

			var hash = new HashCode();
			for (int i = 0; i < Values.Length; i++)
				hash.Add(Values[i]);

			_precomputedHashCode = hash.ToHashCode();
		}

		public bool this[int index] => Values[index];

		public override int GetHashCode() => _precomputedHashCode;
		public override bool Equals(object? obj) => Equals((MultiLabel) obj!);
		public bool Equals([AllowNull] ILabel other) => Equals((MultiLabel) other!);

		public bool Equals([AllowNull] MultiLabel other) {
			if (other is null)
				throw new ArgumentNullException(nameof(other));

			if (Length != other.Length)
				throw new InvalidOperationException();

			if (ReferenceEquals(this, other))
				return true;

			var lhs = Values.AsSpan();
			var rhs = other.Values.AsSpan();

			return lhs.SequenceEqual(rhs);
		}
	}
}
