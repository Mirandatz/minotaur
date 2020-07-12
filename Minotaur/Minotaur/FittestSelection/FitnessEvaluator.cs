namespace Minotaur.FittestSelection {
	using System;
	using System.Threading.Tasks;
	using Minotaur.Classification;
	using Minotaur.Collections;
	using Minotaur.Metrics;

	public sealed class FitnessEvaluator {

		private readonly IMetric[] _metrics;

		// Constructors and alike
		public FitnessEvaluator(ReadOnlySpan<IMetric> metrics) {
			if (metrics.IsEmpty)
				throw new ArgumentException(nameof(metrics) + " can't be empty.");

			var storage = new IMetric[metrics.Length];

			for (int i = 0; i < metrics.Length; i++) {
				var current = metrics[i];
				if (current is null)
					throw new ArgumentException(nameof(metrics) + " can't contain nulls.");

				storage[i] = current;
			}

			_metrics = storage;
		}

		// Actual methods
		public Fitness[] EvaluateAsMaximizationTask(ReadOnlySpan<ConsistentModel> population) {
			var populationAsArray = population.ToArray();
			var fitnesses = new Fitness[population.Length];

			Parallel.For(0, fitnesses.Length, i => {
				fitnesses[i] = EvaluateAsMaximizationTask(populationAsArray[i]);
			});

			return fitnesses;
		}

		public Fitness EvaluateAsMaximizationTask(ConsistentModel model) {
			var metricsValues = new float[_metrics.Length];

			for (int i = 0; i < metricsValues.Length; i++)
				metricsValues[i] = _metrics[i].EvaluateAsMaximizationTask(model);

			return new Fitness(objectivesValues: metricsValues);
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();
	}
}
