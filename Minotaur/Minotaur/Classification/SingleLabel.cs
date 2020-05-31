namespace Minotaur.Classification {
	using System;

	public sealed class SingleLabel: ILabel {

		public readonly int Value;

		public SingleLabel(int value) {
			if (value < 0)
				throw new ArgumentOutOfRangeException(nameof(value) + " must be >= 0.");

			Value = value;
		}

		public override string ToString() => Value.ToString();

		public override int GetHashCode() => throw new NotImplementedException();
		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
