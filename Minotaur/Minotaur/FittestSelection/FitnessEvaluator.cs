namespace Minotaur.EvolutionaryAlgorithms {
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.EvolutionaryAlgorithms.Metrics;
	using Minotaur.EvolutionaryAlgorithms.Population;

	public sealed class FitnessEvaluator {

		public readonly Array<IMetric> Metrics;

		public FitnessEvaluator(Array<IMetric> metrics) {
			Metrics = metrics.ShallowCopy();
		}

		public Fitness[] EvaluateAsMaximizationTask(Array<Individual> individuals) {
			var fitnesses = new Fitness[individuals.Length];

			Parallel.For(0, fitnesses.Length, i => {
				fitnesses[i] = EvaluateAsMaximizationTask(individuals[i]);
			});

			return fitnesses;
		}

		public Fitness EvaluateAsMaximizationTask(Individual individual) {
			var fitnesses = new float[Metrics.Length];

			for (int i = 0; i < fitnesses.Length; i++)
				fitnesses[i] = Metrics[i].EvaluateAsMaximizationTask(individual);

			return Fitness.Wrap(fitnesses);
		}
	}
}
