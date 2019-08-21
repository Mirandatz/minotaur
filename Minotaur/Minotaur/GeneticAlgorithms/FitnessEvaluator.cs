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

	public sealed class FitnessEvaluator {
		public readonly Array<IMetric> Metrics;

		private readonly IConcurrentCache<Individual, Fitness> _cache;

		public FitnessEvaluator(IEnumerable<IMetric> metrics, IConcurrentCache<Individual, Fitness> cache) {
			if (metrics == null)
				throw new ArgumentNullException(nameof(metrics));

			_cache = cache ?? throw new ArgumentNullException(nameof(cache));

			Metrics = metrics.ToArray();
			if (Metrics.Length == 0)
				throw new ArgumentException(nameof(metrics) + " can't be empty.");
			if (Metrics.ContainsNulls())
				throw new ArgumentException(nameof(metrics) + " can't contain nulls.");
		}

		public Fitness[] Evaluate(ReadOnlyMemory<Individual> population) {
			if (population.IsEmpty)
				throw new ArgumentException(nameof(population) + " can't be empty.");
			if (population.Span.ContainsNulls())
				throw new ArgumentException(population + " can't contain nulls.");

			var fitnesses = new Fitness[population.Length];

			Parallel.For(0, fitnesses.Length, i => {
				var individual = population.Span[i];

				var isCached = _cache.TryGet(key: individual, out var fitness);
				if (!isCached) {
					fitness = Evaluate(individual);
					_cache.Add(key: individual, value: fitness);
				}

				fitnesses[i] = fitness;
			});

			return fitnesses;
		}

		private Fitness Evaluate(Individual individual) {
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

				var isCached = _cache.TryGet(key: individual, out var fitness);
				if (!isCached) {
					fitness = EvaluateAsMaximizationTask(individual);
					_cache.Add(key: individual, value: fitness);
				}

				fitnesses[i] = fitness;
			});

			return fitnesses;
		}

		private Fitness EvaluateAsMaximizationTask(Individual individual) {
			var fitnesses = new float[Metrics.Length];

			Parallel.For(0, fitnesses.Length, i => {
				fitnesses[i] = Metrics[i].EvaluateAsMaximizationTask(individual);
			});

			return Fitness.Wrap(fitnesses);
		}
	}
}
