namespace Minotaur.Output {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;
	using System.Linq;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Theseus.Evolution;

	public sealed class AdvancedFileLogger: IEvolutionLogger {

		private readonly string _fieldsSeparator = ", ";
		private readonly string _recordsSeparator = Environment.NewLine;

		private readonly Array<string> _trainFitnessObjectivesNames;
		private readonly Array<string> _testFitnessObjectivesNames;

		private readonly string _outputDirectory;
		private readonly string _individualsLogFilename;
		private readonly string _generationLogFilename;

		private readonly object _syncRoot = new object();
		private readonly Dictionary<long, Individual> _knownIndividuals = new Dictionary<long, Individual>();
		private readonly Dictionary<Individual, Fitness> _trainFitnesses = new Dictionary<Individual, Fitness>();
		private readonly Dictionary<Individual, Fitness> _testFitnesses = new Dictionary<Individual, Fitness>();
		private readonly Dictionary<int, Array<long>> _individualsIdsPerGeneration = new Dictionary<int, Array<long>>();

		private readonly FitnessEvaluatorMk2 _testDatasetFitnessEvaluator;

		public AdvancedFileLogger(string outputDirectory, FitnessEvaluatorMk2 trainDatasetFitnessEvaluator, FitnessEvaluatorMk2 testDatasetFitnessEvaluator) {
			if (!Directory.Exists(outputDirectory))
				throw new ArgumentException(nameof(outputDirectory));

			_outputDirectory = outputDirectory;
			_individualsLogFilename = Path.Combine(_outputDirectory, "individuals_log.csv");
			_generationLogFilename = Path.Combine(_outputDirectory, "generations_log.csv");

			_trainFitnessObjectivesNames = trainDatasetFitnessEvaluator
				.Metrics
				.Select(m => m.Name)
				.ToArray();

			_testFitnessObjectivesNames = testDatasetFitnessEvaluator
				.Metrics
				.Select(m => m.Name)
				.ToArray();

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
				_testFitnesses[individual] = fitness;
			}
		}

		private void UpdateIdsPerGeneration(GenerationResult generationResult) {
			var ids = generationResult
				.Population
				.OrderBy(ind => ind.Id)
				.ThenBy(ind => ind.ParentId)
				.Select(ind => ind.Id)
				.ToArray();

			_individualsIdsPerGeneration.Add(
				key: generationResult.GenerationNumber,
				value: ids);
		}

		public void WriteToDisk() {
			lock (_syncRoot) {
				Task.WaitAll(
					Task.Run(() => WriteIndividualsCsv()),
					Task.Run(() => WriteGenerationsCsv())
					);
			}
		}

		private void WriteIndividualsCsv() {
			var builder = new CsvBuilder(
				fieldsSeparator: _fieldsSeparator,
				recordSeparator: _recordsSeparator,
				fieldNames: new string[] { "Id", "Parent Id", "Rules" });

			var individualsArray = _knownIndividuals
				.Values
				.OrderBy(ind => ind.Id)
				.ThenBy(ind => ind.ParentId)
				.ToArray();

			var ids = new string[individualsArray.Length];
			var parentIds = new string[individualsArray.Length];
			var rules = new string[individualsArray.Length];

			Parallel.For(fromInclusive: 0, toExclusive: individualsArray.Length, i => {
				var individual = individualsArray[i];
				ids[i] = individual.Id.ToString();
				parentIds[i] = individual.ParentId.ToString();
				rules[i] = SerializationHelper.Serialize(individual.Rules);
			});

			for (int i = 0; i < individualsArray.Length; i++) {
				builder.AddField(ids[i]);
				builder.AddField(parentIds[i]);
				builder.AddField(rules[i]);
				builder.FinishRecord();
			}

			File.WriteAllText(
				path: _individualsLogFilename,
				contents: builder.ToString());
		}

		private void WriteGenerationsCsv() {
			var fieldNames = new string[] { "Generation Number", "Individual Id", "Individual Parent Id" }
			.Concat(_trainFitnessObjectivesNames)
			.Concat(_testFitnessObjectivesNames)
			.ToArray();

			var builder = new CsvBuilder(
				fieldsSeparator: _fieldsSeparator,
				recordSeparator: _recordsSeparator,
				fieldNames: fieldNames);

			// @Todo: parallelize the generation of strings,
			// like we did in WriteIndividualsCsv()

			var sortedIndividualsIdsPerGenerations = _individualsIdsPerGeneration
				.OrderBy(kvp => kvp.Key);

			foreach ((var generationNumber, var individualIds) in sortedIndividualsIdsPerGenerations) {
				foreach (var id in individualIds) {
					builder.AddField(generationNumber.ToString());

					var individual = _knownIndividuals[id];
					builder.AddField(individual.Id.ToString());

					builder.AddField(individual.ParentId.ToString());

					var trainFitness = _trainFitnesses[individual];
					foreach (var objective in trainFitness)
						builder.AddField(objective.ToString());

					var testFitness = _testFitnesses[individual];
					foreach (var objective in testFitness)
						builder.AddField(objective.ToString());

					builder.FinishRecord();
				}
			}

			File.WriteAllText(
				path: _generationLogFilename,
				contents: builder.ToString());
		}
	}
}
