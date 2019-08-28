namespace Minotaur.Random {
	using System;
	using System.Linq;
	using Minotaur.Collections;

	public sealed class BiasedOptionChooser<T> {
		private readonly T[] _options;
		private readonly int[] _weights;
		private readonly int _sumOfWeights;

		private BiasedOptionChooser(T[] options, int[] weights, int sumOfWeights) {
			_options = options;
			_weights = weights;
			_sumOfWeights = sumOfWeights;
		}

		public static BiasedOptionChooser<T> Create(Array<T> options, Array<int> weights) {
			if (options is null)
				throw new ArgumentNullException(nameof(options));
			if (weights is null)
				throw new ArgumentNullException(nameof(weights));
			if (options.Length != weights.Length)
				throw new ArgumentException(nameof(options) + " and " + nameof(weights) + " must have the same length.");
			if (options.Length == 0)
				throw new ArgumentException(nameof(options) + " can't be empty.");

			for (int i = 0; i < weights.Length; i++) {
				if (weights[i] <= 0)
					throw new ArgumentException(nameof(weights) + " can't contain non-positive values.");
			}

			var optionsAndProbabilities = new (T option, int weight)[options.Length];
			for (int i = 0; i < optionsAndProbabilities.Length; i++)
				optionsAndProbabilities[i] = (option: options[i], weight: weights[i]);

			return FromWeightedOptions(optionsAndProbabilities);
		}

		private static BiasedOptionChooser<T> FromWeightedOptions((T option, int weight)[] weightedOptions) {
			var sortedWeightedOptions = weightedOptions
				.OrderBy(op => op.weight)
				.ToList();

			var options = new T[sortedWeightedOptions.Count];
			var weights = new int[sortedWeightedOptions.Count];
			var sumOfWeights = 0;

			/* option A, weight 10
			 * option B, weight 10
			 * option C, weight 30
			 * 
			 * weights are stored in this way
			 * [10, 20, 50]
			 * 
			 * We then roll a dice between 0 and 50
			 * if dice < 10, we choose option a
			 * if dice < 20, we choose option b
			 * else we chose option c
			 */

			for (int i = 0; i < sortedWeightedOptions.Count; i++) {
				var (option, weight) = sortedWeightedOptions[i];

				sumOfWeights += weight;
				options[i] = option;
				weights[i] = sumOfWeights;
			}

			return new BiasedOptionChooser<T>(
				options: options,
				weights: weights,
				sumOfWeights: sumOfWeights);
		}

		public T GetRandomChoice() {
			var probability = ThreadStaticRandom.Int(
				inclusiveMin: 0,
				exclusiveMax: _sumOfWeights);

			// @Improve performance by utilizing BinarySearch
			for (int i = 0; i < _weights.Length - 1; i++) {
				if (probability < _weights[i])
					return _options[i];
			}

			return _options[_options.Length - 1];
		}
	}
}