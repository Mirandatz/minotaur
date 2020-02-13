namespace Minotaur.EvolutionaryAlgorithms {

	/// <summary>
	/// This class exists just to make the separation between the train and test
	/// evaluation more explicit.
	/// </summary>
	public sealed class TestFitnessEvaluator {

		public readonly FitnessEvaluator Evaluator;

		public TestFitnessEvaluator(FitnessEvaluator fitnessEvaluator) {
			Evaluator = fitnessEvaluator;
		}
	}
}
