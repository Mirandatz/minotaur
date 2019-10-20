namespace Minotaur.GeneticAlgorithms {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.ExtensionMethods.Span;
	using Minotaur.ExtensionMethods.SystemArray;
	using Minotaur.GeneticAlgorithms.Metrics;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class FitnessEvaluator {

		private readonly IConcurrentCache<Individual, Fitness> _cache;
		public readonly Array<IMetric> Metrics;

		public FitnessEvaluator(IMetric[] metrics, IConcurrentCache<Individual, Fitness> cache) {
			if (metrics.Length == 0)
				throw new ArgumentException(nameof(metrics) + " can't be empty.");
			if (metrics.ContainsNulls())
				throw new ArgumentException(nameof(metrics) + " can't contain nulls.");

			Metrics = metrics.ToArray();
			_cache = cache;
		}

		public Fitness[] EvaluateAsMaximizationTask(Array<Individual> population) {
			if (population.IsEmpty)
				throw new ArgumentException(nameof(population) + " can't be empty.");
			if (population.ContainsNulls())
				throw new ArgumentException(population + " can't contain nulls.");

			var fitnesses = new Fitness[population.Length];

			Parallel.For(0, fitnesses.Length, i => {
				var individual = population[i];

				if (_cache.TryGet(key: individual, out var fitness)) {
					fitnesses[i] = fitness;
				} else {
					fitness = EvaluateAsMaximizationTask(individual);
					_cache.Add(key: individual, value: fitness);
					fitnesses[i] = fitness;
				}
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
