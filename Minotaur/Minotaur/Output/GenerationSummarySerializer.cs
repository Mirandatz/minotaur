namespace Minotaur.Output {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Minotaur.EvolutionaryAlgorithms;
	using Minotaur.Theseus.Evolution;

	public sealed class GenerationSummarySerializer {

		//private readonly CsvSeparators _separators;
		//private readonly TrainFitnessEvaluator _trainEvaluator;
		//private readonly TestFitnessEvaluator _testEvaluator;

		//public ThatClass(CsvSeparators separators, TrainFitnessEvaluator trainEvaluator, TestFitnessEvaluator testEvaluator) {
		//	_separators = separators;
		//	_trainEvaluator = trainEvaluator;
		//	_testEvaluator = testEvaluator;
		//}

		//public string ThatMethod(GenerationResult generationResult) {
		//	var builder = GetBuilder();

		//	var population = generationResult.Population;
		//	var trainFitnessTask = Task.Run(() => _trainEvaluator.EvaluateAsMaximizationTask(population));
		//	var testFitnessTask = Task.Run(() => _testEvaluator.EvaluateAsMaximizationTask(population));

		//	Task.WaitAll(trainFitnessTask, testFitnessTask);

		//	for (int i = 0; i < population.Length; i++) {
		//		var individual = population[i];
		//		throw new NotImplementedException();

		//		builder.FinishRecord();
		//	}

		//	throw new NotImplementedException();
		//}

		//private CsvBuilder GetBuilder() {
		//	var fieldNames = new List<string>() {
		//		"Generation Number",
		//		"Individual Id",
		//		"Individual Parent Id"
		//	};

		//	{
		//		var trainMetricNames = _trainEvaluator
		//			.Metrics
		//			.Select(m => m.Name + " (train)");

		//		fieldNames.AddRange(trainMetricNames);
		//	}

		//	{
		//		var testMetricNames = _testEvaluator
		//			.Metrics
		//			.Select(m => m.Name + " (test)");

		//		fieldNames.AddRange(testMetricNames);
		//	}

		//	return new CsvBuilder(
		//		separators: _separators,
		//		fieldNames: fieldNames.ToArray());
		//}
	}
}
