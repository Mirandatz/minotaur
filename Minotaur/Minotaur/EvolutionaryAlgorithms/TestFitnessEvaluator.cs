namespace Minotaur.EvolutionaryAlgorithms {
	using Minotaur.Collections;
	using Minotaur.EvolutionaryAlgorithms.Metrics;
	using Minotaur.EvolutionaryAlgorithms.Population;

	/// <summary>
	/// This class exists just to make the separation between the train and test
	/// evaluation more explicit.
	/// </summary>
	public sealed class TestFitnessEvaluator {

		private readonly FitnessEvaluator _evaluator;

		public Array<IMetric> Metrics => _evaluator.Metrics;

		public TestFitnessEvaluator(FitnessEvaluator fitnessEvaluator) {
			_evaluator = fitnessEvaluator;
		}

		public Fitness[] EvaluateAsMaximizationTask(Array<Individual> individuals) => _evaluator.EvaluateAsMaximizationTask(individuals);

		public Fitness EvaluateAsMaximizationTask(Individual individual) => _evaluator.EvaluateAsMaximizationTask(individual);
	}
}
