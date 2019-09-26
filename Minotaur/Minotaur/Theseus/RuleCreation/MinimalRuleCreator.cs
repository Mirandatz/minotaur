namespace Minotaur.Theseus.RuleCreation {
	using System;
	using System.Collections.Generic;
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
		private readonly int _maximumNumberOfNullFeatureTests;
		private readonly float _probabilityOfGeneratingNullTest;

		public MinimalRuleCreator(
			Dataset dataset,
			SeedSelector seedSelector,
			HyperRectangleCreator hyperRectangleCreator,
			float maximumRatioOfNullFeatureTest,
			float probabilityOfGeneratingNullTest
			) {
			Dataset = dataset;
			_seedSelector = seedSelector;
			_hyperRectangleCreator = hyperRectangleCreator;
			if (maximumRatioOfNullFeatureTest < 0 || maximumRatioOfNullFeatureTest > 1)
				throw new ArgumentOutOfRangeException(nameof(maximumRatioOfNullFeatureTest));

			var featureCount = Dataset.FeatureCount;
			_maximumNumberOfNullFeatureTests = (int) (featureCount * maximumRatioOfNullFeatureTest);

			if (maximumRatioOfNullFeatureTest < 0 || maximumRatioOfNullFeatureTest > 1)
				_probabilityOfGeneratingNullTest = probabilityOfGeneratingNullTest;
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

			// @Improve logic

			var seed = Dataset.GetInstanceData(datasetSeedInstanceIndex);

			// @Sanity check
			if (!boundingBox.Contains(seed))
				throw new InvalidOperationException();

			var possibleValues = Dataset.GetSortedUniqueFeatureValues(featureIndex);

			var cerriPoint = seed[featureIndex];

			var cerriLeftIndex = possibleValues.BinarySearchFirstOccurence(cerriPoint);
			var cerriRightIndex = possibleValues.BinarySearchLastOccurence(cerriPoint);

			var cerriLowerBound = cerriLeftIndex == 0
				? float.NegativeInfinity
				: possibleValues[cerriLeftIndex - 1];

			var cerriUpperBounddd = cerriRightIndex == possibleValues.Length - 1
				? float.PositiveInfinity
				: possibleValues[cerriRightIndex + 1];

			var dimensionInterval = (ContinuousDimensionInterval) boundingBox.GetDimensionInterval(featureIndex);

			var lowerBound = Math.Max(
				cerriLowerBound,
				dimensionInterval.Start.Value);

			var upperBound = Math.Min(
				cerriUpperBounddd,
				dimensionInterval.End.Value);

			return new ContinuousFeatureTest(
				featureIndex: featureIndex,
				lowerBound: lowerBound,
				upperBound: upperBound);
		}

		private IFeatureTest FromCategorical(int nullFeatureTestCount, int featureIndex, int datasetSeedInstanceIndex, HyperRectangle boundingBox) {
			if (CategoricalTestCanBeNull(nullFeatureTestCount, featureIndex, boundingBox))
				return new NullFeatureTest(featureIndex);

			var seed = Dataset.GetInstanceData(datasetSeedInstanceIndex);

			// @Sanity check
			if (!boundingBox.Contains(seed))
				throw new InvalidOperationException();

			return new CategoricalFeatureTest(
				featureIndex: featureIndex,
				value: seed[featureIndex]);
		}

		private bool CategoricalTestCanBeNull(int nullFeatureTestCount, int featureIndex, HyperRectangle boundingBox) {
			if (nullFeatureTestCount >= _maximumNumberOfNullFeatureTests)
				return false;

			var shouldBeNull = Random.Bool(biasForTrue: _probabilityOfGeneratingNullTest);
			if (!shouldBeNull)
				return false;

			var datasetDimension = (CategoricalDimensionInterval) Dataset.GetDimensionInterval(featureIndex: featureIndex);
			var boxDimension = (CategoricalDimensionInterval) boundingBox.GetDimensionInterval(dimensionIndex: featureIndex);

			return boxDimension.Equals(datasetDimension);
		}

		private bool ContinuousTestCanBeNull(int nullFeatureTestCount, int featureIndex, HyperRectangle boundingBox) {
			if (nullFeatureTestCount >= _maximumNumberOfNullFeatureTests)
				return false;

			var shouldBeNull = Random.Bool(biasForTrue: _probabilityOfGeneratingNullTest);
			if (!shouldBeNull)
				return false;

			var datasetDimension = (ContinuousDimensionInterval) Dataset.GetDimensionInterval(featureIndex: featureIndex);
			var boxDimension = (ContinuousDimensionInterval) boundingBox.GetDimensionInterval(dimensionIndex: featureIndex);

			return boxDimension.Contains(datasetDimension);
		}
	}
}
