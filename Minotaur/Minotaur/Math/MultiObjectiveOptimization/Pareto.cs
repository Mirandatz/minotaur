namespace Minotaur.Math.MultiObjectiveOptimization {
	using System;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.ExtensionMethods.Span;
	using Minotaur.EvolutionaryAlgorithms;

	/// <remarks>
	/// All the methods in this class assumes that fitnesses are to be maximized.
	/// </remarks>
	public static class Pareto {

		public static int[] ComputeDominatedByCounts(Array<Fitness> fitnesses) {
			if (fitnesses.ContainsNulls())
				throw new ArgumentException(nameof(fitnesses) + " can't contain nulls");

			if (fitnesses.IsEmpty)
				return Array.Empty<int>();

			if (fitnesses.Length == 1)
				return new int[] { 0 };

			var dominatedByCounts = new int[fitnesses.Length];
			Parallel.For(0, fitnesses.Length, i => {
				var pivotFitness = fitnesses[i];
				var pivotDominatedByCount = ComputeDominatedByCount(pivotFitness, fitnesses);
				dominatedByCounts[i] = pivotDominatedByCount;
			});

			return dominatedByCounts;
		}

		private static int ComputeDominatedByCount(Fitness pivot, Array<Fitness> fitnesses) {
			if (fitnesses.Length == 0)
				throw new ArgumentException(nameof(fitnesses) + " can't be empty");

			var dominatedBy = 0;
			for (int i = 0; i < fitnesses.Length; i++) {
				var status = Domination(pivot, fitnesses[i]);

				if (status == DominationStatus.RightDominatesLeft)
					dominatedBy += 1;
			}

			return dominatedBy;
		}

		public static (int Dominates, int DominatedBy) CountDominations(Fitness pivot, Array<Fitness> fitnesses) {

			var dominates = 0;
			var dominatedBy = 0;
			for (int i = 0; i < fitnesses.Length; i++) {
				var status = Domination(pivot, fitnesses[i]);

				if (status == DominationStatus.LeftDominatesRight)
					dominates += 1;

				if (status == DominationStatus.RightDominatesLeft)
					dominatedBy += 1;
			}

			return (Dominates: dominates, DominatedBy: dominatedBy);
		}

		public static DominationStatus Domination(Fitness lhs, Fitness rhs) {
			if (lhs.Count != rhs.Count)
				throw new InvalidOperationException();

			var lhsBetter = 0;
			var rhsBetter = 0;
			var elementCount = lhs.Count;

			for (int i = 0; i < elementCount; i++) {
				var left = lhs[i];
				var right = rhs[i];

				if (left > right)
					lhsBetter += 1;
				else if (right > left) {
					rhsBetter += 1;
				}
			}

			if (lhsBetter > 0 && rhsBetter == 0)
				return DominationStatus.LeftDominatesRight;
			if (rhsBetter > 0 && lhsBetter == 0)
				return DominationStatus.RightDominatesLeft;

			return DominationStatus.NoDomination;
		}
	}
}
