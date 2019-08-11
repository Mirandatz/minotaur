namespace Minotaur.Random {
	using System;
	using System.Linq;

	public sealed class BiasedOptionChooser<T> {
		private readonly BiasedChoice[] _accumulatedProbabilities;

		private BiasedOptionChooser(BiasedChoice[] accumulatedProbabilities) {
			_accumulatedProbabilities = accumulatedProbabilities;
		}

		public static BiasedOptionChooser<T> Create(params (T option, float probability)[] optionsProbabilities) {
			if (optionsProbabilities == null)
				throw new ArgumentNullException(nameof(optionsProbabilities));
			if (optionsProbabilities.Length == 0)
				throw new ArgumentNullException(nameof(optionsProbabilities) + " can't be empty");
			if (optionsProbabilities.Select(op => (Decimal) op.probability).Sum() != 1)
				throw new ArgumentException("Probabilities must sum to 1");
			if (optionsProbabilities.Select(op => op.probability).Any(p => !(0 <= p && p <= 1)))
				throw new ArgumentException("All probabilities must be between [0,1]");

			var sortedProbabilities = optionsProbabilities
				.OrderBy(op => op.probability)
				.Reverse()
				.ToList();

			var accumulatedProbabilities = new BiasedChoice[sortedProbabilities.Count];

			float accumulatedProbability = 0;
			for (int i = 0; i < accumulatedProbabilities.Length; i++) {
				var mutationType = sortedProbabilities[i].option;
				var probability = sortedProbabilities[i].probability;
				accumulatedProbability += probability;

				accumulatedProbabilities[i] = new BiasedChoice(
					choice: mutationType,
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
					return _accumulatedProbabilities[i].Choise;
			}

			return _accumulatedProbabilities[_accumulatedProbabilities.Length - 1].Choise;
		}

		private sealed class BiasedChoice {
			public readonly float AccumulatedProbability;
			public readonly T Choise;

			public BiasedChoice(T choice, float accumulatedProbability) {
				if (!(0 <= accumulatedProbability && accumulatedProbability <= 1))
					throw new ArgumentOutOfRangeException(nameof(accumulatedProbability) + " must be between [0,[");

				AccumulatedProbability = accumulatedProbability;
				Choise = choice;
			}
		}

	}
}
