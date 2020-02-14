namespace Minotaur.Output {
	using System;
	using System.IO;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.EvolutionaryAlgorithms.Population;

	public sealed class PersistentOutputManager {

		private readonly string _outputDirectory;

		private readonly bool _saveModels;
		private readonly bool _saveTrainPredictions;
		private readonly bool _saveTestPredictions;

		private readonly PopulationFitnessSerializer _populationFitnessSerializer;
		private readonly PredictionsSerializer _trainPredictionsSerializer;
		private readonly PredictionsSerializer _testPredictionsSerializer;

		public PersistentOutputManager(string outputDirectory, bool saveModels, bool saveTrainPredictions, bool saveTestPredictions, PopulationFitnessSerializer populationFitnessSerializer, PredictionsSerializer trainPredictionsSerializer, PredictionsSerializer testPredictionsSerializer) {
			if (!Directory.Exists(outputDirectory))
				throw new ArgumentException();

			_outputDirectory = outputDirectory;
			_saveModels = saveModels;
			_saveTrainPredictions = saveTrainPredictions;
			_saveTestPredictions = saveTestPredictions;
			_populationFitnessSerializer = populationFitnessSerializer;
			_trainPredictionsSerializer = trainPredictionsSerializer;
			_testPredictionsSerializer = testPredictionsSerializer;
		}

		public void SaveWhatMustBeSaved(Array<Individual> population) {
			var tasks = new Task[] {
				Task.Run(()=> SavePopulationFitnesses(population)),
				Task.Run(()=> SaveModelsIfNecessary(population)),
				Task.Run(()=> SaveTrainPredictionsIfNecessary(population)),
				Task.Run(()=> SaveTestPredictionsIfNecessary(population))
			};

			Task.WaitAll(tasks);
		}

		private void SavePopulationFitnesses(Array<Individual> population) {
			var path = Path.Combine(_outputDirectory, "fitnesses.csv");
			using var textWriter = File.CreateText(path);
			_populationFitnessSerializer.SerializeFitnesses(
				textWriter: textWriter,
				population: population);
		}

		private void SaveModelsIfNecessary(Array<Individual> population) {
			if (!_saveModels)
				return;

			Parallel.ForEach(source: population, body: individual => {
				var path = Path.Combine(_outputDirectory, $"model-{individual.Id}.csv");
				using var textWriter = File.CreateText(path);
				ModelSerializer.Serialize(
					textWriter: textWriter,
					individual: individual);
			});
		}

		private void SaveTrainPredictionsIfNecessary(Array<Individual> population) {
			if (!_saveTrainPredictions)
				return;

			Parallel.ForEach(source: population, body: individual => {
				var path = Path.Combine(_outputDirectory, $"train-predictions-{individual.Id}.csv");
				using var textWriter = File.CreateText(path);
				_trainPredictionsSerializer.SerializePredictions(
					textWriter: textWriter,
					individual: individual);
			});
		}

		private void SaveTestPredictionsIfNecessary(Array<Individual> population) {
			if (!_saveTestPredictions)
				return;

			Parallel.ForEach(source: population, body: individual => {
				var path = Path.Combine(_outputDirectory, $"test-predictions-{individual.Id}.csv");
				using var textWriter = File.CreateText(path);
				_testPredictionsSerializer.SerializePredictions(
					textWriter: textWriter,
					individual: individual);
			});
		}
	}
}
