namespace Minotaur.Classification {
	using System;
	using Minotaur.Collections;

	public sealed class MultiLabel: ILabel {

		private const int MinimumElementCount = 1;

		public readonly Array<bool> Values;
		public readonly int Length;
		public readonly int PrecomputedHashCode;

		public MultiLabel(Array<bool> values) {
			if (values.Length < MinimumElementCount)
				throw new ArgumentException($"{nameof(values)} must contain at least {MinimumElementCount} elements.");

			Values = values.ShallowCopy();
			Length = Values.Length;

			var hash = new HashCode();
			for (int i = 0; i < Values.Length; i++)
				hash.Add(Values[i]);

			PrecomputedHashCode = hash.ToHashCode();
		}

		public bool this[int index] => Values[index];

		public override int GetHashCode() => throw new NotImplementedException();
		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
