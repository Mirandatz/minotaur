namespace Minotaur.Output {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Theseus.Evolution;

	public sealed class SingleGenerationLogger {

		public static readonly string FieldSeparator = ",";
		public static readonly string RecordSeparator = Environment.NewLine;

		private readonly Array<string> _fieldNames;

		private readonly string _outputFilename;
		private readonly FitnessEvaluatorMk2 _testFitnessEvaluator;

		private GenerationResult? _lastGeneration;

		public SingleGenerationLogger(string outputFilename, FitnessEvaluatorMk2 trainFitnessEvaluator, FitnessEvaluatorMk2 testFitnessEvaluator) {
			if (File.Exists(outputFilename))
				throw new ArgumentException(nameof(outputFilename) + " already exists.");

			var fieldNames = new List<string>();
			fieldNames.Add("Generation Number");
			fieldNames.Add("Individual Id");
			fieldNames.Add("Individual Parent Id");

			var trainFitnessObjectiveNames = trainFitnessEvaluator
				.Metrics
				.Select(m => m.Name + " (train)");

			fieldNames.AddRange(trainFitnessObjectiveNames);

			var testFitnessObjectiveNames = testFitnessEvaluator
				.Metrics
				.Select(m => m.Name + " (test)");

			fieldNames.AddRange(testFitnessObjectiveNames);
			fieldNames.Add("Rules");

			_fieldNames = fieldNames.ToArray();
			_outputFilename = outputFilename;
			_testFitnessEvaluator = testFitnessEvaluator;
		}

		public void LogGeneration(GenerationResult generationResult) {
			_lastGeneration = generationResult;
		}

		public void WriteToDisk() {
			if (_lastGeneration == null)
				throw new InvalidOperationException();

			var builder = new CsvBuilder(
				fieldsSeparator: FieldSeparator,
				recordSeparator: RecordSeparator,
				fieldNames: _fieldNames.ToArray());

			var population = _lastGeneration.Population;
			var trainFitnesses = _lastGeneration.Fitnesses;
			var testFitnesses = _testFitnessEvaluator.EvaluateAsMaximizationTask(population);

			for (int individualIndex = 0; individualIndex < population.Length; individualIndex++) {
				var individual = population[individualIndex];
				builder.AddField(_lastGeneration.GenerationNumber.ToString());
				builder.AddField(individual.Id.ToString());
				builder.AddField(individual.ParentId.ToString());

				var trainFitness = trainFitnesses[individualIndex];
				foreach (var trainObjective in trainFitness)
					builder.AddField(trainObjective.ToString());

				var testFitness = testFitnesses[individualIndex];
				foreach (var testObjective in testFitness)
					builder.AddField(testObjective.ToString());

				builder.AddField(SerializationHelper.Serialize(individual.Rules));

				builder.FinishRecord();
			}
			using var file = File.OpenWrite(_outputFilename);
			builder.CopyTo(file);
		}
	}
}
