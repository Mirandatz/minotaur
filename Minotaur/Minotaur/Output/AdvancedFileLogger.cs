namespace Minotaur.Output {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms;
	using System.Threading.Tasks;
	using System.IO;
	using Minotaur.Theseus.Evolution;
	using System.Data;
	using System.Linq;

	public sealed class AdvancedFileLogger: IEvolutionLogger {

		private readonly string _recordSeparator = Environment.NewLine;
		private readonly string _fieldSeparator = ",";
		private readonly int _trainFitnessObjectiveCount;
		private readonly int _testFitnessObjectiveCount;

		private readonly string _outputDirectory;
		private readonly string _individualsLogFilename;
		private readonly string _generationLogFilename;

		private readonly object _syncRoot = new object();
		private readonly Dictionary<long, Individual> _knownIndividuals = new Dictionary<long, Individual>();
		private readonly Dictionary<Individual, Fitness> _trainFitnesses = new Dictionary<Individual, Fitness>();
		private readonly Dictionary<Individual, Fitness> _testFitnesses = new Dictionary<Individual, Fitness>();
		private readonly Dictionary<int, Array<long>> _individualsIdsPerGeneration = new Dictionary<int, Array<long>>();

		private readonly FitnessEvaluatorMk2 _testDatasetFitnessEvaluator;

		public AdvancedFileLogger(string outputDirectory, int trainFitnessObjectiveCount, FitnessEvaluatorMk2 testDatasetFitnessEvaluator) {
			if (!Directory.Exists(outputDirectory))
				throw new ArgumentException(nameof(outputDirectory));
			if (trainFitnessObjectiveCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(trainFitnessObjectiveCount));

			_outputDirectory = outputDirectory;
			_individualsLogFilename = Path.Combine(_outputDirectory, "individuals_log.csv");
			_generationLogFilename = Path.Combine(_outputDirectory, "generations_log.csv");

			_trainFitnessObjectiveCount = trainFitnessObjectiveCount;
			_testFitnessObjectiveCount = testDatasetFitnessEvaluator.Metrics.Length;
			_testDatasetFitnessEvaluator = testDatasetFitnessEvaluator;
		}

		public void LogGeneration(GenerationResult generationResult) {
			lock (_syncRoot) {
				Task.WaitAll(
					Task.Run(() => UpdateKnownIndividuals(generationResult)),
					Task.Run(() => UpdateTrainFitnesses(generationResult)),
					Task.Run(() => UpdateTestFitnesses(generationResult)),
					Task.Run(() => UpdateIdsPerGeneration(generationResult))
					);
			}
		}

		public void WriteToDisk() {
			lock (_syncRoot) {
				Task.WaitAll(
					Task.Run(() => WriteIndividualsCsv()),
					Task.Run(() => WriteGenerationsCsv())
					);
			}
		}

		private void UpdateKnownIndividuals(GenerationResult generationResult) {
			var population = generationResult.Population;
			var individualCount = population.Length;

			for (int i = 0; i < individualCount; i++) {
				var individual = population[i];
				_knownIndividuals[individual.Id] = individual;
			}
		}

		private void UpdateTrainFitnesses(GenerationResult generationResult) {
			var population = generationResult.Population;
			var individualCount = population.Length;
			var fitnesses = generationResult.Fitnesses;

			for (int i = 0; i < individualCount; i++) {
				var individual = population[i];
				var fitness = fitnesses[i];
				_trainFitnesses[individual] = fitness;
			}
		}

		private void UpdateTestFitnesses(GenerationResult generationResult) {
			var population = generationResult.Population;
			var individualCount = population.Length;

			// Compute fitnesses if necessary
			var fitnesses = new Fitness[individualCount];
			Parallel.For(0, individualCount, i => {
				var individual = population[i];
				var fitness = _testFitnesses.GetValueOrDefault(individual);
				if (fitness is null)
					fitness = _testDatasetFitnessEvaluator.EvaluateAsMaximizationTask(individual);

				fitnesses[i] = fitness;
			});

			// Update dict
			for (int i = 0; i < individualCount; i++) {
				var individual = population[i];
				var fitness = fitnesses[i];
				_trainFitnesses[individual] = fitness;
			}
		}

		private void UpdateIdsPerGeneration(GenerationResult generationResult) {
			var population = generationResult.Population;
			var individualCount = population.Length;

			var ids = new long[individualCount];

			for (int i = 0; i < individualCount; i++)
				ids[i] = population[i].Id;

			_individualsIdsPerGeneration[generationResult.GenerationNumber] = ids;
		}

		private void WriteIndividualsCsv() {
			var builder = new StringBuilder();

			// Writing header
			builder.Append("Id");
			builder.Append(_fieldSeparator);

			builder.Append("Parent Id");
			builder.Append(_fieldSeparator);

			builder.Append("Rules");
			builder.Append(_fieldSeparator);

			// Writing records
			var records = GenerateRecordsForAllIndividuals();
			builder.Append(records);

			File.WriteAllText(
				path: _individualsLogFilename,
				contents: builder.ToString());
		}

		private string GenerateRecordsForAllIndividuals() {
			var individualsArray = _knownIndividuals.ToArray();
			var individualCount = individualsArray.Length;
			var records = new string[individualCount];

			Parallel.For(fromInclusive: 0, toExclusive: individualCount, i => {
				records[i] = GenerateRecordForSingleIndividual(individualsArray[i].Value);
			});

			return string.Join(
				separator: _recordSeparator,
				values: records as IEnumerable<string>);
		}

		private string GenerateRecordForSingleIndividual(Individual individual) {
			return
				$"{individual.Id}{_fieldSeparator}" +
				$"{individual.ParentId}{_fieldSeparator}" +
				$"{SerializationHelper.Serialize(individual.Rules)} " +
				$"DEFAULT {SerializationHelper.Serialize(individual.DefaultPrediction)}";
		}

		private void WriteGenerationsCsv() {
			var builder = new StringBuilder();

			// Writing header
			builder.Append("Generation Number");
			builder.Append(_fieldSeparator);

			builder.Append("Individual Parent Id");
			builder.Append(_fieldSeparator);

			builder.Append("Individual Id");
			builder.Append(_fieldSeparator);

			for (int i = 0; i < _trainFitnessObjectiveCount; i++) {
				builder.Append("Train Fitness Objective ");
				builder.Append(i);
				builder.Append(_fieldSeparator);
			}

			for (int i = 0; i < _testFitnessObjectiveCount; i++) {
				builder.Append("Test Fitness Objective ");
				builder.Append(i);
				builder.Append(_fieldSeparator);
			}

			builder.Append(_recordSeparator);

			// Writing records
			var records = GenerateRecordsForAllGenerations();
			builder.Append(records);

			File.WriteAllText(
				path: _generationLogFilename,
				contents: builder.ToString());
		}

		private string GenerateRecordsForAllGenerations() {
			var generationCount = _individualsIdsPerGeneration.Count;
			var records = new string[generationCount];

			Parallel.For(fromInclusive: 0, toExclusive: generationCount, generationNumber => {
				records[generationNumber] = GenerateRecordsForSingleGeneration(generationNumber);
			});

			return string.Join(
				separator: _recordSeparator,
				values: records as IEnumerable<string>);
		}

		private string GenerateRecordsForSingleGeneration(int generationNumber) {
			var builder = new StringBuilder();
			builder.Append(generationNumber);
			builder.Append(_fieldSeparator);

			var generationIds = _individualsIdsPerGeneration[generationNumber];
			foreach (var individualId in generationIds) {
				builder.Append(individualId);
				builder.Append(_fieldSeparator);

				var individual = _knownIndividuals[individualId];

				var trainFitness = _trainFitnesses[individual];
				var serializedTrainFitness = string.Join(
					separator: _fieldSeparator,
					values: trainFitness);
				builder.Append(trainFitness);
				builder.Append(_fieldSeparator);

				var testFitness = _testFitnesses[individual];
				var serializedTestFitness = string.Join(
					separator: _fieldSeparator,
					values: testFitness);

				builder.Append(_recordSeparator);
			}

			return builder.ToString();
		}
	}
}
