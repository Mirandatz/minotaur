namespace Minotaur.GeneticAlgorithms {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.ExtensionMethods.Span;
	using Minotaur.ExtensionMethods.SystemArray;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Metrics;

	public sealed class CachedFitnessEvaluator {
		public readonly Array<IMetric> Metrics;

		private readonly LruCache<Individual, Fitness> _cache;

		public CachedFitnessEvaluator(IEnumerable<IMetric> metrics, int cacheSize) {
			if (metrics == null)
				throw new ArgumentNullException(nameof(metrics));
			if (cacheSize < 1)
				throw new ArgumentOutOfRangeException(nameof(cacheSize) + " must be >= 1.");

			_cache = new LruCache<Individual, Fitness>(cacheSize);

			var metricsAsArray = metrics.ToArray();

			if (metricsAsArray.Length == 0)
				throw new ArgumentException(nameof(metrics) + " can't be empty.");
			if (metricsAsArray.ContainsNulls())
				throw new ArgumentException(nameof(metrics) + " can't contain nulls.");

			Metrics = metricsAsArray.AsReadOnly();
			_cache = new LruCache<Individual, Fitness>(cacheSize);
		}

		public Fitness[] Evaluate(ReadOnlyMemory<Individual> population) {
			if (population.IsEmpty)
				throw new ArgumentException(nameof(population) + " can't be empty.");
			if (population.Span.ContainsNulls())
				throw new ArgumentException(population + " can't contain nulls.");

			var fitnesses = new Fitness[population.Length];

			Parallel.For(0, fitnesses.Length, i => {
				var individual = population.Span[i];
				var fitnessIsCached = _cache.TryGet(key: individual, out var fitness);

				if (!fitnessIsCached) {
					fitness = Evaluate(individual);
					lock (_cache) {
						_cache.Add(key: individual, val: fitness);
					}
				}

				fitnesses[i] = fitness;
			});

			return fitnesses;
		}

		private Fitness Evaluate(Individual individual) {
			if (individual == null)
				throw new ArgumentNullException(nameof(individual));

			var fitnesses = new float[Metrics.Length];
			Parallel.For(0, fitnesses.Length, i => {
				fitnesses[i] = Metrics[i].Evaluate(individual);
			});

			return Fitness.Wrap(fitnesses);
		}

		public Fitness[] EvaluateAsMaximizationTask(ReadOnlyMemory<Individual> population) {
			if (population.IsEmpty)
				throw new ArgumentException(nameof(population) + " can't be empty.");
			if (population.Span.ContainsNulls())
				throw new ArgumentException(population + " can't contain nulls.");

			var fitnesses = new Fitness[population.Length];

			Parallel.For(0, fitnesses.Length, i => {
				var individual = population.Span[i];
				var fitnessIsCached = _cache.TryGet(key: individual, out var fitness);

				if (!fitnessIsCached) {
					fitness = EvaluateAsMaximizationTask(individual);
					lock (_cache) {
						_cache.Add(key: individual, val: fitness);
					}
				}

				fitnesses[i] = fitness;
			});

			return fitnesses;
		}

		private Fitness EvaluateAsMaximizationTask(Individual individual) {
			if (individual == null)
				throw new ArgumentNullException(nameof(individual));

			var fitnesses = new float[Metrics.Length];
			Parallel.For(0, fitnesses.Length, i => {
				fitnesses[i] = Metrics[i].EvaluateAsMaximizationTask(individual);
			});

			return Fitness.Wrap(fitnesses);
		}
	}
}
