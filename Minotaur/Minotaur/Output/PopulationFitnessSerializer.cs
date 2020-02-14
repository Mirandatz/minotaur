namespace Minotaur.Output {
	using System.Globalization;
	using System.IO;
	using CsvHelper;
	using Minotaur.Collections;
	using Minotaur.EvolutionaryAlgorithms;
	using Minotaur.EvolutionaryAlgorithms.Population;

	public static class PopulationFitnessSerializer {

		public static void SerializeFitnesses(TextWriter textWriter, Array<Individual> population, TrainFitnessEvaluator trainFitnessEvaluator, TestFitnessEvaluator testFitnessEvaluator) {
			using var csvWriter = new CsvWriter(writer: textWriter, cultureInfo: CultureInfo.InvariantCulture);

			// csv header
			csvWriter.WriteField("Id");
			foreach (var metric in trainFitnessEvaluator.Metrics)
				csvWriter.WriteField($"{metric.Name} (train)");
			foreach (var metric in testFitnessEvaluator.Metrics)
				csvWriter.WriteField($"{metric.Name} (test)");
			csvWriter.NextRecord();

			var trainFitnesses = trainFitnessEvaluator.EvaluateAsMaximizationTask(population);
			var testFitnesses = testFitnessEvaluator.EvaluateAsMaximizationTask(population);

			for (int i = 0; i < population.Length; i++) {
				csvWriter.WriteField(population[i].Id);

				foreach (var objective in trainFitnesses[i])
					csvWriter.WriteField(objective);

				foreach (var objective in testFitnesses[i])
					csvWriter.WriteField(objective);

				csvWriter.NextRecord();
			}
		}
	}
}
