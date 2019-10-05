namespace Minotaur.Theseus.RuleCreation {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math;
	using Random = Random.ThreadStaticRandom;

	public sealed class CoverageAwareRuleCreator: IRuleCreator {
		public Dataset Dataset { get; }
		private readonly SeedFinder _seedSelector;
		private readonly RuleAntecedentHyperRectangleConverter _boxConverter;
		private readonly NonIntersectingRectangleCreator _boxCreator;
		private readonly HyperRectangleCoverageComputer _coverageComputer;
		private readonly InstanceCoveringRuleAntecedentCreator _antecedentCreator;
		private readonly InstanceLabelsAveragingRuleConsequentCreator _consequentCreator;
		private readonly HyperRectangleIntersector _rectangleIntersector;
		private readonly int _targetNumberOfInstancesToCover;

		public CoverageAwareRuleCreator(SeedFinder seedSelector, RuleAntecedentHyperRectangleConverter boxConverter, NonIntersectingRectangleCreator boxCreator, HyperRectangleCoverageComputer coverageComputer, InstanceCoveringRuleAntecedentCreator antecedentCreator, InstanceLabelsAveragingRuleConsequentCreator consequentCreator, HyperRectangleIntersector hyperRectangleIntersector, int minimumInstancesToCover) {
			_seedSelector = seedSelector;
			_boxConverter = boxConverter;
			_boxCreator = boxCreator;
			_coverageComputer = coverageComputer;
			_antecedentCreator = antecedentCreator;
			_consequentCreator = consequentCreator;
			_targetNumberOfInstancesToCover = minimumInstancesToCover;
			_rectangleIntersector = hyperRectangleIntersector;
			Dataset = _seedSelector.Dataset;
		}

		public bool TryCreateRule(Array<Rule> existingRules, [MaybeNullWhen(false)] out Rule rule) {

			var seedsIndices = _seedSelector.FindSeedsIndices(existingRules);

			if (seedsIndices.IsEmpty) {
				rule = default!;
				return false;
			}

			// This may fail just because we chose a bad seed or even because
			// we chose a bad dimension expansion order
			var seedIndex = Random.Choice(seedsIndices);
			var seed = Dataset.GetInstanceData(seedIndex);

			var boxes = _boxConverter.FromRules(existingRules);

			// @Sanity check
			for (int i = 0; i < boxes.Length; i++) {
				var box = boxes[i];
				if (box.Contains(seed))
					throw new InvalidOperationException();
			}

			var dimensionExpansionOrder = NaturalRange.CreateShuffled(
				inclusiveStart: 0,
				exclusiveEnd: Dataset.FeatureCount);

			var secureRectangle = _boxCreator.CreateLargestNonIntersectingRectangle(
				seedIndex: seedIndex,
				existingHyperRectangles: boxes,
				dimensionExpansionOrder: dimensionExpansionOrder);

			// @Sanity check
			for (int i = 0; i < boxes.Length; i++) {
				var box = boxes[i];
				if (_rectangleIntersector.IntersectsInAllDimension(secureRectangle, box))
					throw new InvalidOperationException();
			}

			var secureRectangleCoverage = _coverageComputer.ComputeCoverage(secureRectangle);
			var coveredInstancesIndices = secureRectangleCoverage.IndicesOfCoveredInstances.ToArray();

			if(coveredInstancesIndices.Length == 0) {
				rule = null!;
				return false;
			}

			var coveredInstancesDistancesToSeed = Dataset.ComputeDistances(
				targetInstanceIndex: seedIndex,
				otherInstancesIndices: coveredInstancesIndices);

			Array.Sort(
				keys: coveredInstancesDistancesToSeed,
				items: coveredInstancesIndices);

			var instancesToCover = Math.Min(_targetNumberOfInstancesToCover, coveredInstancesIndices.Length);
			var relevantInstances = coveredInstancesIndices
				.AsSpan()
				.Slice(start: 0, length: instancesToCover);

			var ruleAntecedent = _antecedentCreator.CreateAntecedent(
				seedIndex: seedIndex,
				nearestInstancesIndices: relevantInstances);

			var ruleConsequent = _consequentCreator.CreateConsequent(relevantInstances);

			rule = new Rule(
				antecedent: ruleAntecedent,
				consequent: ruleConsequent);

			return true;
		}
	}
}
