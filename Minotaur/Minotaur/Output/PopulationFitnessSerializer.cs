namespace Minotaur.Output {
	using System.Globalization;
	using System.IO;
	using CsvHelper;
	using Minotaur.Collections;
	using Minotaur.EvolutionaryAlgorithms;
	using Minotaur.EvolutionaryAlgorithms.Population;

	public sealed class PopulationFitnessSerializer {

		private readonly TrainFitnessEvaluator _trainFitnessEvaluator;
		private readonly TestFitnessEvaluator _testFitnessEvaluator;

		public PopulationFitnessSerializer(TrainFitnessEvaluator trainFitnessEvaluator, TestFitnessEvaluator testFitnessEvaluator) {
			_trainFitnessEvaluator = trainFitnessEvaluator;
			_testFitnessEvaluator = testFitnessEvaluator;
		}

		public void SerializeFitnesses(TextWriter textWriter, Array<Individual> population) {
			using var csvWriter = new CsvWriter(writer: textWriter, cultureInfo: CultureInfo.InvariantCulture);

			// csv header
			csvWriter.WriteField("Id");
			foreach (var metric in _trainFitnessEvaluator.Metrics)
				csvWriter.WriteField($"{metric.Name} (train)");
			foreach (var metric in _testFitnessEvaluator.Metrics)
				csvWriter.WriteField($"{metric.Name} (test)");
			csvWriter.NextRecord();

			var trainFitnesses = _trainFitnessEvaluator.EvaluateAsMaximizationTask(population);
			var testFitnesses = _testFitnessEvaluator.EvaluateAsMaximizationTask(population);

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
