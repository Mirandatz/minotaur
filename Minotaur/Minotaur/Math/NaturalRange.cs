namespace Minotaur.Math {
	using System;
	using Minotaur.Collections;

	public sealed class NaturalRange {
		public bool IsSorted;
		public readonly Array<int> Values;

		private NaturalRange(Array<int> values, bool isSorted) {
			IsSorted = isSorted;
			Values = values;
		}

		public int Length => Values.Length;
		public int this[int index] => Values[index];

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

			return new NaturalRange(
				values: values,
				isSorted: true);
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

			return new NaturalRange(
				values: values,
				isSorted: true);
		}
	}
}
