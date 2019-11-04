namespace Minotaur.Math {
	using System;

	public sealed class NaturalRange {
		private readonly int[] _values;
		public readonly int Length;

		private NaturalRange(int[] values) {
			_values = values;
			Length = _values.Length;
		}

		public int this[int index] => _values[index];

		public static NaturalRange CreateSorted(int inclusiveStart, int exclusiveEnd) {
			if (inclusiveStart >= exclusiveEnd)
				throw new ArgumentException(nameof(inclusiveStart) + " must be < " + nameof(exclusiveEnd));

			var count = exclusiveEnd - inclusiveStart;
			var values = new int[count];

			var currentValue = inclusiveStart;
			for (int i = 0; i < values.Length; i++) {
				values[i] = currentValue;
				currentValue += 1;
			}

			return new NaturalRange(values: values);
		}

		public static NaturalRange CreateShuffled(int inclusiveStart, int exclusiveEnd) {
			if (inclusiveStart >= exclusiveEnd)
				throw new ArgumentException(nameof(inclusiveStart) + " must be < " + nameof(exclusiveEnd));

			var count = exclusiveEnd - inclusiveStart;
			var values = new int[count];

			var currentValue = inclusiveStart;
			for (int i = 0; i < values.Length; i++) {
				values[i] = currentValue;
				currentValue += 1;
			}

			Minotaur.Random.ThreadStaticRandom.Shuffle(values);

			return new NaturalRange(values: values);
		}
	}
}
