namespace Minotaur.Classification {
	using System;
	using Minotaur.Collections;

	public sealed class MultiLabel: ILabel {

		public readonly Array<bool> Values;
		public readonly int Length;
		public readonly int PrecomputedHashCode;

		public MultiLabel(Array<bool> values) {
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
