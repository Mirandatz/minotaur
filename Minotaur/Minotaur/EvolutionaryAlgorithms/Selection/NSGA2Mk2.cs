namespace Minotaur.EvolutionaryAlgorithms.Selection {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Minotaur.Collections;
	using Minotaur.Math;
	using Minotaur.Math.MultiObjectiveOptimization;

	public sealed class NSGA2Mk2: IFittestIdentifier {
		private readonly int _fittestCount;

		public NSGA2Mk2(int fittestCount) {
			if (fittestCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(fittestCount));

			_fittestCount = fittestCount;
		}

		public int[] FindIndicesOfFittestIndividuals(Array<Fitness> fitnesses) {
			if (fitnesses.ContainsNulls())
				throw new ArgumentException(nameof(fitnesses));
			if (fitnesses.Length < _fittestCount)
				throw new ArgumentException(nameof(fitnesses));
			var objectiveCount = fitnesses[0].Count;
			for (int i = 0; i < fitnesses.Length; i++) {
				if (fitnesses[i].Count != objectiveCount)
					throw new ArgumentException(nameof(fitnesses));
			}

			var dominatedByCounts = Pareto.ComputeDominatedByCounts(fitnesses);
			var indices = new int[fitnesses.Length];
			for (int i = 0; i < indices.Length; i++)
				indices[i] = i;

			var fronts = indices
				.GroupBy(i => dominatedByCounts[i])
				.OrderBy(g => g.Key)
				.Select(g => g.ToList())
				.ToList();

			var fittestIndices = new List<int>(_fittestCount);

			foreach (var front in fronts) {
				if (fittestIndices.Count + front.Count <= _fittestCount) {
					fittestIndices.AddRange(front);
				} else {
					var mostDiverse = OrderFrontByDiversity(front, fitnesses).Take(_fittestCount - (fittestIndices.Count));
					fittestIndices.AddRange(mostDiverse);
					break;
				}
			}

			// Sanity check
			if (fittestIndices.Count != _fittestCount)
				throw new InvalidOperationException();

			// Sanity check
			if (fittestIndices.Distinct().Count() != fittestIndices.Count)
				throw new InvalidOperationException();

			// Sanity check
			if (fittestIndices.Any(i => i < 0))
				throw new InvalidOperationException();

			return fittestIndices.ToArray();
		}

		private int[] OrderFrontByDiversity(List<int> indices, Array<Fitness> allFitnesses) {
			var indexedFitnesses = SelectAndIndexFitnesses(indices, allFitnesses);
			var objectives = ExtractObjectivesValues(indexedFitnesses);
			var objectiveScalers = CreateScalers(objectives);
			var scaledObjectives = RescaleObjectives(objectives, objectiveScalers);
			var indexedScaledObjectives = IndexScaledObjectives(indexedFitnesses, scaledObjectives);
			var sortedScaledObjectives = SortScaledObjectives(scaledObjectives);
			var crowdingDistances = ComputeCrowdingDistances(indexedScaledObjectives, sortedScaledObjectives);

			var mostDiverse = indices.ToArray();
			Array.Sort(
				keys: crowdingDistances,
				items: mostDiverse);

			return mostDiverse;

			static (int Index, Fitness Fitness)[] SelectAndIndexFitnesses(List<int> indices, Array<Fitness> fitnesses) {
				var indexedFitnesses = new (int Index, Fitness Fitness)[indices.Count];
				for (int i = 0; i < indexedFitnesses.Length; i++)
					indexedFitnesses[i] = (indices[i], fitnesses[indices[i]]);

				return indexedFitnesses;
			}

			static float[][] ExtractObjectivesValues((int Index, Fitness Fitness)[] indexedAndScaledFitnesses) {
				var fitnessesCount = indexedAndScaledFitnesses.Length;
				var objectivesCount = indexedAndScaledFitnesses[0].Fitness.Count;

				var objectiveValues = new float[objectivesCount][];
				for (int i = 0; i < objectivesCount; i++) {
					objectiveValues[i] = new float[fitnessesCount];
					for (int j = 0; j < fitnessesCount; j++) {
						objectiveValues[i][j] = indexedAndScaledFitnesses[j].Fitness[i];
					}
				}

				return objectiveValues;
			}

			static MinMaxScaler[] CreateScalers(float[][] objectivesValues) {
				var objectiveCount = objectivesValues.Length;
				var objectiveScalers = new MinMaxScaler[objectiveCount];
				for (int i = 0; i < objectiveScalers.Length; i++)
					objectiveScalers[i] = MinMaxScaler.Create(objectivesValues[i]);

				return objectiveScalers;
			}

			static float[][] RescaleObjectives(float[][] objectives, MinMaxScaler[] scalers) {
				var rescaledObjectives = new float[objectives.Length][];
				for (int i = 0; i < rescaledObjectives.Length; i++)
					rescaledObjectives[i] = scalers[i].Rescale(objectives[i]);

				return rescaledObjectives;
			}

			static (int Index, float[] Objectives)[] IndexScaledObjectives((int Index, Fitness Fitness)[] indexedFitnesses, float[][] scaledObjectives) {
				var indexedScaledObjectives = new (int Index, float[] Objectives)[indexedFitnesses.Length];
				var objectiveCount = scaledObjectives.Length;

				for (int i = 0; i < indexedScaledObjectives.Length; i++) {
					var objectives = new float[objectiveCount];
					for (int j = 0; j < objectiveCount; j++)
						objectives[j] = scaledObjectives[j][i];

					indexedScaledObjectives[i] = (
						Index: indexedFitnesses[i].Index,
						Objectives: objectives);
				}

				return indexedScaledObjectives;
			}

			static float[][] SortScaledObjectives(float[][] scaledObjectives) {
				var sortedScaledObjectives = new float[scaledObjectives.Length][];
				for (int i = 0; i < sortedScaledObjectives.Length; i++) {
					sortedScaledObjectives[i] = scaledObjectives[i].ToArray();
					Array.Sort(sortedScaledObjectives[i]);
				}

				return sortedScaledObjectives;
			}

			static float[] ComputeCrowdingDistances((int Index, float[] Objectives)[] indexedScaledObjectives, float[][] sortedScaledObjectives) {
				var crowdingDistances = new float[indexedScaledObjectives.Length];
				for (int i = 0; i < crowdingDistances.Length; i++) {
					crowdingDistances[i] = ComputeCrowdingDistance(indexedScaledObjectives[i], sortedScaledObjectives);
				}

				return crowdingDistances;

				static float ComputeCrowdingDistance((int Index, float[] Objectives) instance, float[][] sortedScaledObjectives) {
					var crowdingDistance = 0f;
					var objectiveCount = sortedScaledObjectives.Length;

					for (int i = 0; i < objectiveCount; i++) {
						var instanceObjectiveValue = instance.Objectives[i];
						var sortedObjectiveValues = sortedScaledObjectives[i];
						var instancePosition = Array.BinarySearch(sortedObjectiveValues, instanceObjectiveValue);

						if (instancePosition < 0)
							throw new InvalidOperationException();

						if (instancePosition == 0 || instancePosition == sortedObjectiveValues.Length - 1)
							return float.NegativeInfinity;

						var leftNeighborObjectiveValue = sortedObjectiveValues[instancePosition - 1];
						var rightNeighborObjectiveValue = sortedObjectiveValues[instancePosition + 1];
						crowdingDistance += Math.Abs(rightNeighborObjectiveValue - leftNeighborObjectiveValue);
					}

					return crowdingDistance;
				}
			}
		}
	}
}
