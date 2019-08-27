namespace Minotaur.Random {
	using System;
	using System.Linq;

	public sealed class BiasedOptionChooser<T> {
		private const Decimal Epsilon = ((Decimal) 1) / 100000;

		private readonly BiasedChoice[] _accumulatedProbabilities;

		private BiasedOptionChooser(BiasedChoice[] accumulatedProbabilities) {
			_accumulatedProbabilities = accumulatedProbabilities;
		}

		public static BiasedOptionChooser<T> Create(T[] options, float[] probabilities) {
			if (options is null)
				throw new ArgumentNullException(nameof(options));
			if (probabilities is null)
				throw new ArgumentNullException(nameof(probabilities));
			if (options.Length != probabilities.Length)
				throw new ArgumentException(nameof(options) + " and " + nameof(probabilities) + " must have the same length.");
			if (options.Length == 0)
				throw new ArgumentException(nameof(options) + " can't be empty.");
			if ((probabilities.Select(w => (Decimal) w).Sum() - 1) > Epsilon)
				throw new ArgumentException("Probabilities must sum to 1.");
			if (probabilities.Any(p => !(0 <= p && p <= 1)))
				throw new ArgumentException("All weights must be between [0,1].");

			var optionsAndProbabilities = new (T option, float probability)[options.Length];
			for (int i = 0; i < optionsAndProbabilities.Length; i++)
				optionsAndProbabilities[i] = (option: options[i], probability: probabilities[i]);

			return Create(optionsAndProbabilities);
		}

		private static BiasedOptionChooser<T> Create(params (T option, float probability)[] weightedOptions) {
			// OrderBy -op.probability is used
			// to achiev the result of OrderBy followed by a Reverse

			var sortedProbabilities = weightedOptions
				.OrderBy(op => -op.probability)
				.ToList();

			var accumulatedProbabilities = new BiasedChoice[sortedProbabilities.Count];

			float accumulatedProbability = 0;
			for (int i = 0; i < accumulatedProbabilities.Length; i++) {
				var option = sortedProbabilities[i].option;
				var probability = sortedProbabilities[i].probability;
				accumulatedProbability += probability;

				accumulatedProbabilities[i] = new BiasedChoice(
					choice: option,
					accumulatedProbability: accumulatedProbability);
			}

			return new BiasedOptionChooser<T>(accumulatedProbabilities);
		}

		public T GetRandomChoice() {
			var probability = ThreadStaticRandom.Uniform();

			for (
				int i = 0;
				i < _accumulatedProbabilities.Length - 1; // We don't even need to check against the last probability
				i++) {
				if (probability <= _accumulatedProbabilities[i].AccumulatedProbability)
					return _accumulatedProbabilities[i].Choice;
			}

			return _accumulatedProbabilities[_accumulatedProbabilities.Length - 1].Choice;
		}

		private sealed class BiasedChoice {
			public readonly float AccumulatedProbability;
			public readonly T Choice;

			public BiasedChoice(T choice, float accumulatedProbability) {
				if (!(0 <= accumulatedProbability && accumulatedProbability <= 1))
					throw new ArgumentOutOfRangeException(nameof(accumulatedProbability) + " must be between [0,1].");

				AccumulatedProbability = accumulatedProbability;
				Choice = choice;
			}
		}
	}
}
