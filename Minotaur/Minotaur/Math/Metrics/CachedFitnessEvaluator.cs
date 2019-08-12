namespace Minotaur.Math.Metrics {
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.ExtensionMethods.SystemArray;
	using Minotaur.ExtensionMethods.Span;
	using Minotaur.GeneticAlgorithms;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class CachedFitnessEvaluator {
		public readonly Array<IMetric> Metrics;

		private LruCache<Individual, Fitness> _cache;

		public CachedFitnessEvaluator(IEnumerable<IMetric> metrics, int cacheSize) {
			if (metrics == null)
				throw new ArgumentNullException(nameof(metrics));
			_cache = new LruCache<Individual, Fitness>(cacheSize);

			var metricsAsArray = metrics.ToArray();

			if (metricsAsArray.Length == 0)
				throw new ArgumentException(nameof(metrics) + " can't be empty.");

			if (metricsAsArray.ContainsNulls())
				throw new ArgumentException(nameof(metrics) + " can't contain nulls.");

			Metrics = metricsAsArray.AsReadOnly();
			_cache = new LruCache<Individual, Fitness>(cacheSize);
		}

		public CachedFitnessEvaluator(int cacheSize, params IMetric[] metrics) {
			if (metrics == null)
				throw new ArgumentNullException(nameof(metrics));
			if (cacheSize < 1)
				throw new ArgumentOutOfRangeException(nameof(cacheSize) + " must be >= 1.");

			if (metrics.Length == 0)
				throw new ArgumentException(nameof(metrics) + " can't be empty.");

			if (metrics.ContainsNulls())
				throw new ArgumentException(nameof(metrics) + " can't contain nulls.");

			Metrics = metrics.AsReadOnly();
			_cache = new LruCache<Individual, Fitness>(cacheSize);
		}

		public Fitness[] Evaluate(ReadOnlyMemory<Individual> population) {
			if (population.Span.ContainsNulls())
				throw new ArgumentException(population + " can't contain nulls");
			if (population.IsEmpty)
				throw new ArgumentException(nameof(population) + " can't be empty");

			throw new NotImplementedException();
			//lock (_cache) {
			//	var fitnesses = new Fitness[population.Length];

			//	Parallel.For(0, fitnesses.Length, i => {
			//		fitnesses[i] = Evaluate(population.Span[i]);
			//	});

			//	PruneCache(population);

			//	return fitnesses;
			//}
		}

		private Fitness Evaluate(Individual individual) {
			if (individual == null)
				throw new ArgumentNullException(nameof(individual));

			throw new NotImplementedException();
			//var resultIsCached = _cache.TryGetValue(individual, out var fitness);
			//if (resultIsCached)
			//	return fitness;

			//var fitnessValues = new float[Metrics.Length];
			//for (int i = 0; i < fitnessValues.Length; i++)
			//	fitnessValues[i] = Metrics[i].Evaluate(individual);

			//fitness = Fitness.Wrap(fitnessValues);
			//_cache[individual] = fitness;

			//return fitness;
		}

		public Fitness[] EvaluateAsMaximizationTask(ReadOnlyMemory<Individual> population) {
			if (population.Span.ContainsNulls())
				throw new ArgumentException(population + " can't contain nulls");
			if (population.IsEmpty)
				throw new ArgumentException(nameof(population) + " can't be empty");

			throw new NotImplementedException();

			//lock (_cache) {
			//	var fitnesses = new Fitness[population.Length];

			//	Parallel.For(0, fitnesses.Length, i => {
			//		fitnesses[i] = EvaluateAsMaximizationTask(population.Span[i]);
			//	});

			//	PruneCache(population);

			//	return fitnesses;
			//}
		}

		private Fitness EvaluateAsMaximizationTask(Individual individual) {
			if (individual == null)
				throw new ArgumentNullException(nameof(individual));

			throw new NotImplementedException();

			//var resultIsCached = _cache.TryGetValue(individual, out var fitness);
			//if (resultIsCached)
			//	return fitness;

			//var fitnessValues = new float[Metrics.Length];
			//for (int i = 0; i < fitnessValues.Length; i++)
			//	fitnessValues[i] = Metrics[i].EvaluateAsMaximizationTask(individual);

			//fitness = Fitness.Wrap(fitnessValues);
			//_cache[individual] = fitness;

			//return fitness;
		}

		private void PruneCache(ReadOnlyMemory<Individual> population) {
			throw new NotImplementedException();

			// Todo: improve performance
			//var populationSpan = population.Span;

			//var newCache = new ConcurrentDictionary<Individual, Fitness>(
			//	concurrencyLevel: Environment.ProcessorCount,
			//	capacity: population.Length);

			//for (int i = 0; i < populationSpan.Length; i++) {
			//	var individual = populationSpan[i];
			//	newCache[individual] = _cache[individual];
			//}

			//_cache = newCache;
		}
	}
}
