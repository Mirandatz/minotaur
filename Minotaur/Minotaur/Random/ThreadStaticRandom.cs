namespace Minotaur.Random {
	using System;
	using System.Buffers;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using Minotaur.Collections;
	using Minotaur.ExtensionMethods.Float;

	public static class ThreadStaticRandom {
		// ThreadStatic to make sure that each thread gets a Random for itself, preventing the corruption
		// of the Random object
		[ThreadStatic]
		private static Random? _instance;

		// This wraps the _random variable to make sure each thread gets a Random for itself
		private static Random Instance {
			get {
				if (_instance == null)
					_instance = new Random();

				return _instance;
			}
		}

		public static bool Bool() {
			return Instance.NextDouble() < 0.5;
		}

		public static bool Bool(float biasForTrue) {
			if (float.IsNaN(biasForTrue))
				throw new ArgumentOutOfRangeException(nameof(biasForTrue) + " can't be NaN");
			if (biasForTrue < 0 || biasForTrue > 1)
				throw new ArgumentOutOfRangeException(nameof(biasForTrue) + " must be in [0, 1] interval");

			return Uniform() < biasForTrue;
		}

		public static bool[] Bools(int count) {
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count) + " must be > 0");

			var labels = new bool[count];
			// Todo: if shit hits the fan (it shouldn't), check this out
			var bytes = MemoryMarshal.AsBytes<bool>(labels);
			Instance.NextBytes(bytes);
			for (int i = 0; i < bytes.Length; i++)
				bytes[i] >>= 7;

			return labels;
		}

		public static double Uniform() {
			return Instance.NextDouble();
		}

		public static double Uniform(float inclusiveMin, float exclusiveMax) {
			if (inclusiveMin.IsNanOrInfinity())
				throw new ArgumentOutOfRangeException(nameof(inclusiveMin) + " can't be NaN nor Infinity");
			if (exclusiveMax.IsNanOrInfinity())
				throw new ArgumentOutOfRangeException(nameof(exclusiveMax) + " can't be NaN nor Infinity");
			if (inclusiveMin > exclusiveMax)
				throw new ArgumentException(nameof(inclusiveMin) + " must be <= " + nameof(exclusiveMax));

			double range = exclusiveMax - inclusiveMin;
			range *= Instance.NextDouble();

			return inclusiveMin + range;
		}

		public static int Int(int exclusiveMax) {
			return Instance.Next(minValue: 0, maxValue: exclusiveMax);
		}

		public static int Int(int inclusiveMin, int exclusiveMax) {
			return Instance.Next(minValue: inclusiveMin, maxValue: exclusiveMax);
		}

		public static T Choice<T>(T first, T second) {
			if (Bool())
				return first;
			else
				return second;
		}

		public static T Choice<T>(IReadOnlyList<T> values) {
			if (values == null)
				throw new ArgumentNullException(nameof(values));
			if (values.Count == 0)
				throw new ArgumentException(nameof(values) + " can't be empty");

			var index = Instance.Next(minValue: 0, maxValue: values.Count);
			return values[index];
		}

		public static T Choice<T>(Array<T> values) {
			if (values == null)
				throw new ArgumentNullException(nameof(values));
			if (values.Length == 0)
				throw new ArgumentException(nameof(values) + " can't be empty");

			var index = Instance.Next(minValue: 0, maxValue: values.Length);
			return values[index];
		}

		public static T Choice<T>(ReadOnlySpan<T> values) {
			if (values.Length == 0)
				throw new ArgumentException(nameof(values) + " can't be empty");

			var index = Instance.Next(minValue: 0, maxValue: values.Length);
			return values[index];
		}

		public static T Choice<T>(T[] values) {
			if (values.Length == 0)
				throw new ArgumentException(nameof(values) + " can't be empty");

			var index = Instance.Next(minValue: 0, maxValue: values.Length);
			return values[index];
		}

		public static void Shuffle<T>(Span<T> values) {
			if (values.Length == 0)
				throw new ArgumentException(nameof(values) + " can't be empty.");
			if (values.Length == 0)
				throw new ArgumentException(nameof(values) + " can't be empty.");

			var rng = Instance;
			int n = values.Length;
			while (n > 1) {
				n--;
				int k = rng.Next(n + 1);
				var temp = values[k];
				values[k] = values[n];
				values[n] = temp;
			}
		}

		public static void Shuffle<T>(T[] values) {
			if (values == null)
				throw new ArgumentNullException(nameof(values));
			if (values.Length == 0)
				throw new ArgumentException(nameof(values) + " can't be empty.");

			var rng = Instance;
			int n = values.Length;
			while (n > 1) {
				n--;
				int k = rng.Next(n + 1);
				var temp = values[k];
				values[k] = values[n];
				values[n] = temp;
			}
		}

		public static void CopyRandomWithoutReplacement<T>(ReadOnlySpan<T> from, Span<T> to, int count) {
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count) + " must be >= 0");
			if (from.Length < count)
				throw new ArgumentException(nameof(from) + $" must have at least {nameof(count)} elements.");
			if (to.Length < count)
				throw new ArgumentException(nameof(to) + $" must have at least {nameof(count)} elements.");

			var rentedBuffer = ArrayPool<int>.Shared.Rent(minimumLength: from.Length);
			var indexes = rentedBuffer.AsSpan().Slice(
				start: 0,
				length: from.Length);

			for (int i = 0; i < indexes.Length; i++)
				indexes[i] = i;

			Shuffle(indexes);

			for (int i = 0; i < count; i++)
				to[i] = from[indexes[i]];

			ArrayPool<int>.Shared.Return(rentedBuffer);
		}

		public static T[] SelectRandomWithoutReplacement<T>(ReadOnlySpan<T> from, int count) {
			if (count < 0)
				throw new ArgumentOutOfRangeException(nameof(count) + " must be >= 0");

			var randomlySelected = new T[count];
			CopyRandomWithoutReplacement(
				from: from,
				to: randomlySelected,
				count: count);

			return randomlySelected;
		}
	}
}
