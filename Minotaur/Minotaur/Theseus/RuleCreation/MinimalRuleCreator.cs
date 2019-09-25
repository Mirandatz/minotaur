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

	public sealed class MinimalRuleCreator: IRuleCreator {

		public Dataset Dataset { get; }
		private readonly SeedSelector _seedSelector;
		private readonly HyperRectangleCreator _hyperRectangleCreator;
		private readonly MinimalTestCreator _testCreator;
		private readonly int _maximumNumberOfNullFeatureTests;

		public MinimalRuleCreator(
			Dataset dataset,
			SeedSelector seedSelector,
			HyperRectangleCreator hyperRectangleCreator,
			MinimalTestCreator minimalTestCreator,
			float maximumRatioOfNullFeatureTest
			) {
			Dataset = dataset;
			_seedSelector = seedSelector;
			_hyperRectangleCreator = hyperRectangleCreator;
			_testCreator = minimalTestCreator;

			if (maximumRatioOfNullFeatureTest < 0 || maximumRatioOfNullFeatureTest > 1)
				throw new ArgumentOutOfRangeException(nameof(maximumRatioOfNullFeatureTest));

			var featureCount = Dataset.FeatureCount;
			_maximumNumberOfNullFeatureTests = (int) (featureCount * maximumRatioOfNullFeatureTest);
		}

		public bool TryCreateRule(Array<Rule> existingRules, [MaybeNullWhen(false)] out Rule newRule) {
			var seedFound = _seedSelector.TryFindSeed(
				existingRules: existingRules,
				datasetInstanceIndex: out var seedIndex);

			if (!seedFound) {
				newRule = null!;
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
				boundingRectangle: secureRectangle);

			var labels = Random.Bools(count: Dataset.ClassCount);

			newRule = new Rule(
				tests: tests,
				predictedLabels: labels);

			return true;
		}

		private IFeatureTest[] CreateTests(int datasetSeedIndex, HyperRectangle boundingRectangle) {

			var testCreationOrder = NaturalRange.CreateShuffled(
				inclusiveStart: 0,
				exclusiveEnd: Dataset.FeatureCount);

			var tests = new IFeatureTest[Dataset.FeatureCount];
			var nullFeatureTestCount = 0;

			for (int i = 0; i < tests.Length; i++) {
				var featureIndex = testCreationOrder[i];

				var newTest = CreateTest(
					featureIndex: featureIndex,
					nullFeatureTestCount: nullFeatureTestCount,
					datasetSeedInstanceIndex: datasetSeedIndex,
					boundingBox: boundingRectangle);

				if (newTest is NullFeatureTest)
					nullFeatureTestCount += 1;

				tests[featureIndex] = newTest;
			}

			return tests;
		}

		private IFeatureTest CreateTest(
			int featureIndex,
			int nullFeatureTestCount,
			int datasetSeedInstanceIndex,
			HyperRectangle boundingBox
			) {

			if (!Dataset.IsFeatureIndexValid(featureIndex))
				throw new ArgumentOutOfRangeException(nameof(featureIndex));

			if (!Dataset.IsInstanceIndexValid(datasetSeedInstanceIndex))
				throw new ArgumentOutOfRangeException(nameof(datasetSeedInstanceIndex));

			return (Dataset.GetFeatureType(featureIndex)) switch
			{
				FeatureType.Categorical => FromCategorical(
					nullFeatureTestCount: nullFeatureTestCount,
					featureIndex: featureIndex,
					datasetSeedInstanceIndex: datasetSeedInstanceIndex,
					boundingBox: boundingBox),

				FeatureType.CategoricalButTriviallyValued => FromCategorical(
					nullFeatureTestCount: nullFeatureTestCount,
					featureIndex: featureIndex,
					datasetSeedInstanceIndex: datasetSeedInstanceIndex,
					boundingBox: boundingBox),

				FeatureType.Continuous => FromContinuous(
					nullFeatureTestCount: nullFeatureTestCount,
					featureIndex: featureIndex,
					datasetSeedInstanceIndex: datasetSeedInstanceIndex,
					boundingBox: boundingBox),

				_ => throw new InvalidOperationException($"Unknown / unsupported value for {nameof(FeatureType)}."),
			};
		}

		private IFeatureTest FromContinuous(int nullFeatureTestCount, int featureIndex, int datasetSeedInstanceIndex, HyperRectangle boundingBox) {
			if (ContinuousTestCanBeNull(nullFeatureTestCount, featureIndex, boundingBox))
				return new NullFeatureTest(featureIndex);

			throw new NotImplementedException();
		}

		private IFeatureTest FromCategorical(int nullFeatureTestCount, int featureIndex, int datasetSeedInstanceIndex, HyperRectangle boundingBox) {
			throw new NotImplementedException();
		}

		private bool ContinuousTestCanBeNull(int nullFeatureTestCount, int featureIndex, HyperRectangle boundingBox) {
			if (nullFeatureTestCount >= _maximumNumberOfNullFeatureTests)
				return false;

			var featureValues = Dataset.GetSortedUniqueFeatureValues(featureIndex);
			var datasetLowerBound = featureValues[0];
			var datasetUpperBound = featureValues[^1];

			var dimension = (ContinuousDimensionInterval) boundingBox.GetDimensionInterval(featureIndex);
			var lowerBound = dimension.Start;

			throw new NotImplementedException();
		}
	}
}
