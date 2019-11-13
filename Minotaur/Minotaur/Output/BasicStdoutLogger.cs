namespace Minotaur.Output {
	using System;
	using System.Text;
	using Minotaur.GeneticAlgorithms;
	using Minotaur.Theseus.Evolution;

	public sealed class BasicStdoutLogger: IEvolutionLogger {

		private readonly FitnessEvaluatorMk2 _testDatasetFitnessEvaluator;

		public BasicStdoutLogger(FitnessEvaluatorMk2 testDatasetFitnessEvaluator) {
			_testDatasetFitnessEvaluator = testDatasetFitnessEvaluator;
		}

		public void LogGeneration(GenerationResult generationResult) {
			var population = generationResult.Population;
			var trainFitness = generationResult.Fitnesses;
			var sortedFitnesses = trainFitness.ToArray();

			Array.Sort(
				array: sortedFitnesses,
				comparer: new LexicographicalFitnessComparer());

			var builder = new StringBuilder();

			builder.AppendLine();
			builder.AppendLine("Train Dataset Fitness");
			builder.Append("Best individual: ");
			builder.AppendLine(sortedFitnesses[^1].ToString());
			builder.Append("Second best individual: ");
			builder.AppendLine(sortedFitnesses[^2].ToString());
			builder.Append("Median individual: ");
			builder.AppendLine(sortedFitnesses[sortedFitnesses.Length / 2].ToString());
			builder.Append("Second worst individual: ");
			builder.AppendLine(sortedFitnesses[1].ToString());
			builder.Append("Worst individual: ");
			builder.AppendLine(sortedFitnesses[0].ToString());

			var testFitness = _testDatasetFitnessEvaluator.EvaluateAsMaximizationTask(population);
			Array.Sort(
				array: testFitness,
				comparer: new LexicographicalFitnessComparer());

			builder.AppendLine("Test Dataset Fitness");
			builder.Append("Best individual: ");
			builder.AppendLine(sortedFitnesses[^1].ToString());
			builder.Append("Second best individual: ");
			builder.AppendLine(sortedFitnesses[^2].ToString());
			builder.Append("Median individual: ");
			builder.AppendLine(sortedFitnesses[sortedFitnesses.Length / 2].ToString());
			builder.Append("Second worst individual: ");
			builder.AppendLine(sortedFitnesses[1].ToString());
			builder.Append("Worst individual: ");
			builder.AppendLine(sortedFitnesses[0].ToString());

			Console.WriteLine(builder.ToString());
		}
	}
}
