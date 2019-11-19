namespace Minotaur.Output {
	using System;
	using System.Text;
	using Minotaur.GeneticAlgorithms;
	using Minotaur.Theseus.Evolution;

	public sealed class BasicStdoutLogger: IEvolutionLogger {

		private readonly object _syncRoot = new object();

		private readonly FitnessEvaluatorMk2 _testDatasetFitnessEvaluator;

		public BasicStdoutLogger(FitnessEvaluatorMk2 testDatasetFitnessEvaluator) {
			_testDatasetFitnessEvaluator = testDatasetFitnessEvaluator;
		}

		public void LogGeneration(GenerationResult generationResult) {
			lock (_syncRoot) {
				if (generationResult.GenerationNumber % 10 != 0)
					return;

				var population = generationResult.Population;
				var trainFitness = generationResult.Fitnesses.ToArray();

				Array.Sort(
					array: trainFitness,
					comparer: new LexicographicalFitnessComparer());

				var builder = new StringBuilder();

				builder.AppendLine();
				builder.Append("Generation ");
				builder.AppendLine(generationResult.GenerationNumber.ToString());
				builder.AppendLine("Train Dataset Fitness");
				builder.Append("Best individual: ");
				builder.AppendLine(trainFitness[^1].ToString());
				builder.Append("Second best individual: ");
				builder.AppendLine(trainFitness[^2].ToString());
				builder.Append("Median individual: ");
				builder.AppendLine(trainFitness[trainFitness.Length / 2].ToString());
				builder.Append("Second worst individual: ");
				builder.AppendLine(trainFitness[1].ToString());
				builder.Append("Worst individual: ");
				builder.AppendLine(trainFitness[0].ToString());

				var testFitness = _testDatasetFitnessEvaluator.EvaluateAsMaximizationTask(population);
				Array.Sort(
					array: testFitness,
					comparer: new LexicographicalFitnessComparer());

				builder.AppendLine("Test Dataset Fitness");
				builder.Append("Best individual: ");
				builder.AppendLine(testFitness[^1].ToString());
				builder.Append("Second best individual: ");
				builder.AppendLine(testFitness[^2].ToString());
				builder.Append("Median individual: ");
				builder.AppendLine(testFitness[testFitness.Length / 2].ToString());
				builder.Append("Second worst individual: ");
				builder.AppendLine(testFitness[1].ToString());
				builder.Append("Worst individual: ");
				builder.AppendLine(testFitness[0].ToString());

				Console.WriteLine(builder.ToString());
			}
		}
	}
}
