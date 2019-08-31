namespace Minotaur.GeneticAlgorithms.Selection {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math;
	using Minotaur.Math.MultiObjectiveOptimization;

	// @Refactor this
	public sealed class NSGA2: IFittestSelector {

		private readonly int _fittestCount;
		private readonly FitnessEvaluator _fitnessEvaluator;

		public NSGA2(FitnessEvaluator fitnessEvaluator, int fittestCount) {
			_fitnessEvaluator = fitnessEvaluator ?? throw new ArgumentNullException(nameof(fitnessEvaluator));

			if (fittestCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(fittestCount) + " must be >= 1.");

			_fittestCount = fittestCount;
		}

		public Individual[] SelectFittest(Array<Individual> population) {
			if (_fittestCount < 0)
				throw new ArgumentOutOfRangeException(nameof(_fittestCount) + " must be  >= 0");
			if (population.ContainsNulls())
				throw new ArgumentException(nameof(population) + " can't contain nulls");

			var populationArray = population.ToArray();

			var fitnesses = _fitnessEvaluator.EvaluateAsMaximizationTask(population);
			var fitnessesDict = new Dictionary<Individual, Fitness>(capacity: fitnesses.Length);
			for (int i = 0; i < population.Length; i++)
				fitnessesDict[populationArray[i]] = fitnesses[i];

			var dominatedByCounts = Pareto.ComputeDominatedByCounts(fitnesses);
			var dominationDict = new Dictionary<Individual, int>(capacity: dominatedByCounts.Length);
			for (int i = 0; i < populationArray.Length; i++)
				dominationDict[populationArray[i]] = dominatedByCounts[i];

			var fronts = populationArray
				.GroupBy(individual => dominationDict[individual])
				.OrderBy(frontWithKey => frontWithKey.Key)
				.Select(frontWithKey => frontWithKey.ToList())
				.ToList();

			var fittest = new List<Individual>(capacity: _fittestCount);

			foreach (var front in fronts) {
				if (fittest.Count + front.Count <= _fittestCount) {
					fittest.AddRange(front);
				} else {
					var mostDiverse = OrderFrontByDiversity(front, fitnessesDict)
						.Take(_fittestCount - (fittest.Count));

					fittest.AddRange(mostDiverse);
					break;
				}
			}

			// Sanity check
			if (fittest.Count != _fittestCount)
				throw new InvalidOperationException();

			return fittest.ToArray();
		}

		private IEnumerable<Individual> OrderFrontByDiversity(
			List<Individual> front,
			Dictionary<Individual, Fitness> fitnesses
			) {
			var densityEstimator = NSGA2Density.Create(fitnesses);

			// Settings keys
			var densities = new Dictionary<Individual, float>(front.Count);
			for (int i = 0; i < front.Count; i++)
				densities[front[i]] = float.NaN;

			Parallel.For(fromInclusive: 0, toExclusive: front.Count, i => {
				var individual = front[i];
				var density = densityEstimator.Evaluate(individual);
				densities[individual] = density;
			});

			var mostDiverse = densities
				.OrderBy(kvp => kvp.Value)
				.Select(kvp => kvp.Key)
				.ToList();

			return mostDiverse;
		}

		// @Refactor this
		private sealed class NSGA2Density {
			private readonly Dictionary<Individual, Fitness> _scaledFitnesses;
			private readonly Matrix<float> _scaledSortedObjectives;

			private NSGA2Density(
				Dictionary<Individual, Fitness> scaledFitnesses,
				Matrix<float> scaledSortedObjectives
				) {
				_scaledFitnesses = scaledFitnesses ?? throw new ArgumentNullException(nameof(scaledFitnesses));
				_scaledSortedObjectives = scaledSortedObjectives ?? throw new ArgumentNullException(nameof(scaledSortedObjectives));
			}

			public static NSGA2Density Create(IReadOnlyDictionary<Individual, Fitness> fitnesses) {
				if (fitnesses == null)
					throw new ArgumentNullException(nameof(fitnesses));

				var objectiveCount = fitnesses.First().Value.Count;
				var instanceCount = fitnesses.Count;

				var rescaledSortedObjectives = new MutableMatrix<float>(
					rowCount: objectiveCount,
					columnCount: instanceCount);

				var MinMaxScalers = CreateObjectiveMinMaxScalers(fitnesses, objectiveCount, rescaledSortedObjectives);

				var rescaledFitnesses = new Dictionary<Individual, Fitness>(
					capacity: fitnesses.Count);

				foreach (var (ind, fit) in fitnesses) {
					var rescaledFitness = new float[fit.Count];

					for (int i = 0; i < rescaledFitness.Length; i++) {
						rescaledFitness[i] = MinMaxScalers[i].Rescale(fit[i]);
					}

					rescaledFitnesses[ind] = Fitness.Wrap(rescaledFitness);
				}

				return new NSGA2Density(
					scaledFitnesses: rescaledFitnesses,
					scaledSortedObjectives: rescaledSortedObjectives.ToMatrix());
			}

			private static MinMaxScaler[] CreateObjectiveMinMaxScalers(
				IReadOnlyDictionary<Individual, Fitness> fitnesses,
				int objectiveCount,
				MutableMatrix<float> rescaledSortedObjectives) {
				var MinMaxScalers = new MinMaxScaler[objectiveCount];
				for (int i = 0; i < objectiveCount; i++) {
					var values = fitnesses
						.Values
						.Select(v => v[i])
						.ToArray();

					Array.Sort(values);
					MinMaxScalers[i] = MinMaxScaler.Create(values);
					MinMaxScalers[i].Rescale(values);
					var row = rescaledSortedObjectives.GetRow(i);
					values.CopyTo(row);
				}

				return MinMaxScalers;
			}

			public float Evaluate(Individual individual) {
				var density = 0f;
				var fitnesses = _scaledFitnesses[individual];

				for (int objectiveIndex = 0; objectiveIndex < _scaledSortedObjectives.RowCount; objectiveIndex++) {
					var objectiveValues = _scaledSortedObjectives.GetRow(objectiveIndex);
					var fitness = fitnesses[objectiveIndex];
					var position = objectiveValues.BinarySearch(fitness);

					if (position == 0 || position == objectiveValues.Length - 1) {
						return float.NegativeInfinity;
					}

					density += objectiveValues[position - 1];
					density += objectiveValues[position + 1];
				}

				return density;
			}
		}
	}
}
