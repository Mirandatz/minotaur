namespace Minotaur {
	using System;
	using System.Linq;
	using System.Text;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class FitnessReportMaker {

		public readonly FitnessEvaluator TrainDatasetFitnessEvaluator;
		public readonly FitnessEvaluator TestDatasetFitnessEvaluator;

		public FitnessReportMaker(
			FitnessEvaluator trainDatasetFitnessEvaluator,
			FitnessEvaluator testDatasetFitnessEvaluator
			) {
			TrainDatasetFitnessEvaluator = trainDatasetFitnessEvaluator ?? throw new ArgumentNullException(nameof(trainDatasetFitnessEvaluator));
			TestDatasetFitnessEvaluator = testDatasetFitnessEvaluator ?? throw new ArgumentNullException(nameof(testDatasetFitnessEvaluator));
		}

		public string MakeReport(Array<Individual> population) {
			if (population is null)
				throw new ArgumentNullException(nameof(population));
			if (population.Length < 3)
				throw new ArgumentException(nameof(population) + " must contain at least 3 elements.");

			var trainFitness = TrainDatasetFitnessEvaluator.EvaluateAsMaximizationTask(population);
			var sortedFitnesses = trainFitness.ToArray();
			Array.Sort(
				array: sortedFitnesses,
				comparer: new LexicographicalFitnessComparer());

			var builder = new StringBuilder();

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


			var testFitness = TestDatasetFitnessEvaluator.EvaluateAsMaximizationTask(population);
			sortedFitnesses = testFitness.ToArray();
			Array.Sort(
				array: sortedFitnesses,
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

			return builder.ToString();
		}
	}
}
