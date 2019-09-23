namespace Minotaur.Theseus.RuleCreation {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math;
	using Minotaur.Math.Dimensions;
	using Minotaur.Theseus.TestCreation;
	using Random = Random.ThreadStaticRandom;

	public sealed class CerriRuleCreator: IRuleCreator {

		public Dataset Dataset { get; }
		private readonly SeedSelector _seedSelector;
		private readonly HyperRectangleCreator _hyperRectangleCreator;
		private readonly CerriTestCreator _testCreator;

		public CerriRuleCreator(
			SeedSelector seedSelector,
			CerriTestCreator testCreator,
			HyperRectangleCreator hyperRectangleCreator
			) {
			_seedSelector = seedSelector;
			_testCreator = testCreator;
			_hyperRectangleCreator = hyperRectangleCreator;
			Dataset = testCreator.Dataset;
		}

		public bool TryCreateRule(Array<Rule> existingRules, [MaybeNullWhen(false)] out Rule rule) {
			var seedFound = _seedSelector.TryFindSeed(
				existingRules: existingRules,
				datasetInstanceIndex: out var seedIndex);

			if (!seedFound) {
				rule = null!;
				return false;
			}

			var seed = Dataset.GetInstanceData(seedIndex);

			var hyperRectangles = _hyperRectangleCreator.FromRules(rules: existingRules);

			var dimensionOrder = NaturalRange.CreateShuffled(
				inclusiveStart: 0,
				exclusiveEnd: Dataset.FeatureCount);

			var secureRectangle = _hyperRectangleCreator.CreateLargestNonIntersectingHyperRectangle(
				seed: seed,
				existingRectangles: hyperRectangles,
				dimensionExpansionOrder: dimensionOrder);

			// @Sanity check
			if (!secureRectangle.Contains(seed))
				throw new InvalidOperationException();

			var tests = CreateTests(
				datasetSeedIndex: seedIndex,
				secureRectangle: secureRectangle);

			var labels = Random.Bools(count: Dataset.ClassCount);

			rule = new Rule(
				tests: tests,
				predictedLabels: labels);

			return true;
		}

		private IFeatureTest[] CreateTests(int datasetSeedIndex, HyperRectangle secureRectangle) {
			var dimension = secureRectangle.Dimensions;
			var tests = new IFeatureTest[dimension.Length];

			for (int i = 0; i < tests.Length; i++) {
				tests[i] = _testCreator.Create(
					featureIndex: i,
					datasetSeedInstanceIndex: datasetSeedIndex,
					boundingBox: secureRectangle);
			}

			return tests;
		}
	}
}
