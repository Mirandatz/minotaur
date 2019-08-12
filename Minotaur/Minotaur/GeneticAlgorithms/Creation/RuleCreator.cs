namespace Minotaur.GeneticAlgorithms.Creation {
	using System;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Random = Minotaur.Random.ThreadStaticRandom;

	public sealed class RuleCreator {
		private readonly Dataset _dataset;
		private readonly FeatureTestCreator _testCreator;

		public RuleCreator(Dataset dataset, FeatureTestCreator testCreator) {
			_dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
			_testCreator = testCreator ?? throw new ArgumentNullException(nameof(testCreator));
		}

		public Rule CreateRule() {
			var nonNullTestProbability = (float) Random.Uniform();

			var testArray = new IFeatureTest[_dataset.FeatureCount];

			// Keeping track of how many tests we add, to ensure, in the end,
			// that we add at least one test
			var nonNullTests = 0;

			for (int i = 0; i < testArray.Length; i++) {
				var addTest = Random.Bool(biasForTrue: nonNullTestProbability);

				if (addTest) {
					testArray[i] = _testCreator.CreateTest(featureIndex: i);
					nonNullTests += 1;
				} else {
					testArray[i] = new NullFeatureTest(featureIndex: i);
				}
			}

			// If we have not added a single test, then we manually add 1
			// to ensure that we have never create a empty rule
			if(nonNullTests == 0) {
				var index = Random.Int(exclusiveMax: testArray.Length);
				testArray[index] = _testCreator.CreateTest(featureIndex: index);
			}

			var predictedLabels = Random.RandomLabels(_dataset.ClassCount);

			return new Rule(
				tests: testArray,
				predictedLabels: predictedLabels);
		}
	}
}
