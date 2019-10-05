namespace Minotaur.Theseus.RuleCreation {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;
	using static Minotaur.Math.Dimensions.HyperRectangleBuilder;

	public sealed class InstanceCoveringRuleAntecedentCreator {

		public Dataset Dataset { get; }
		private readonly RuleAntecedentHyperRectangleConverter _converter;

		public InstanceCoveringRuleAntecedentCreator(RuleAntecedentHyperRectangleConverter ruleAntecedentHyperRectangleConverter) {
			_converter = ruleAntecedentHyperRectangleConverter;
			Dataset = _converter.Dataset;
		}

		public bool CreateAntecedent(int seedIndex, ReadOnlySpan<int> nearestInstancesIndices, [MaybeNullWhen(false)] out IFeatureTest[] featureTests) {
			var builder = HyperRectangleBuilder.InitializeWithSeed(
				dataset: Dataset,
				seedIndex: seedIndex);

			for (int i = 0; i < nearestInstancesIndices.Length; i++) {
				var index = nearestInstancesIndices[i];
				EnlargeToCoverInstance(
					builder: builder,
					instanceIndex: index);
			}

			if (!builder.TryBuild(out var box)) {
				featureTests = null!;
				return false;
			} else {
				featureTests = _converter.FromHyperRectangle(box);
				return true;
			}
		}

		private void EnlargeToCoverInstance(HyperRectangleBuilder builder, int instanceIndex) {
			var dimensionCount = Dataset.FeatureCount;
			var instance = Dataset.GetInstanceData(instanceIndex);

			for (int i = 0; i < dimensionCount; i++) {
				switch (Dataset.GetFeatureType(i)) {

				case FeatureType.Continuous:
				EnlargeContinuousInterval(
					builder: builder,
					instance: instance,
					dimensionIndex: i);
				break;

				default:
				throw CommonExceptions.UnknownFeatureType;
				}
			}
		}

		private void EnlargeContinuousInterval(HyperRectangleBuilder builder, Array<float> instance, int dimensionIndex) {
			var (Start, End) = builder.GetContinuousDimensionPreview(dimensionIndex);
			var target = instance[dimensionIndex];

			if (target < Start) {
				builder.UpdateContinuousDimensionIntervalStart(
					dimensionIndex: dimensionIndex,
					value: target);
				return;
			}

			if (target >= End) {
				var possibleValues = Dataset.GetSortedUniqueFeatureValues(featureIndex: dimensionIndex);
				var indexOfStart = possibleValues.BinarySearch(target);
				if (indexOfStart == possibleValues.Length - 1) {
					builder.UpdateContinuousDimensionIntervalEnd(
						dimensionIndex: dimensionIndex,
						value: float.PositiveInfinity);
				} else {
					var nextLarger = possibleValues[indexOfStart + 1];
					builder.UpdateContinuousDimensionIntervalEnd(
						dimensionIndex: dimensionIndex,
						value: nextLarger);
				}
			}
		}
	}
}
