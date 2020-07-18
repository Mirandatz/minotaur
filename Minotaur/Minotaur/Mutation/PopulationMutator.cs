namespace Minotaur.Mutation {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using Minotaur.Classification;
	using Minotaur.Collections;
	using Random = Random.ThreadStaticRandom;

	public sealed class PopulationMutator {

		private readonly int _maximumFailedAttempts;
		private readonly int _targetNumberOfMutants;
		private readonly IIndividualMutator _individualMutator;

		public PopulationMutator(IIndividualMutator individualMutator, int maximumFailedAttempts, int targetNumberOfMutants) {
			if (maximumFailedAttempts < 0)
				throw new ArgumentOutOfRangeException(nameof(maximumFailedAttempts) + " must be >= 0.");
			if (targetNumberOfMutants <= 0)
				throw new ArgumentOutOfRangeException(nameof(targetNumberOfMutants) + " must be > 0.");

			_maximumFailedAttempts = maximumFailedAttempts;
			_targetNumberOfMutants = targetNumberOfMutants;
			_individualMutator = individualMutator;
		}

		public ConsistentModel[]? TryGenerateMutants(Array<ConsistentModel> population) {
			if (population.IsEmpty)
				throw new ArgumentException(nameof(population) + " can't be empty.");
			if (population.ToHashSet().Count != population.Length)
				throw new ArgumentException(nameof(population) + " can't contain duplicated elements.");

			var uniqueModels = GenerateMutantsInParallel(population);
			var mutantCount = uniqueModels.Count - population.Length;

			// Dang it, we couldn't generate the target number of mutants
			if (mutantCount < _targetNumberOfMutants)
				return null;

			// Removing non-mutants, i.e. individuals from the individual population
			uniqueModels.ExceptWith(population);

			// Our mutant generation stratengy may generate a few more morants than _targetNumberOfMutants
			if (uniqueModels.Count == _targetNumberOfMutants)
				return uniqueModels.ToArray();
			else
				return uniqueModels.Take(_targetNumberOfMutants).ToArray();
		}

		private HashSet<ConsistentModel> GenerateMutantsInParallel(Array<ConsistentModel> population) {
			var loopControl = new LoopControl(
				previousGenerationPopulation: population,
				targetNumberOfMutants: _targetNumberOfMutants,
				maximumFailedMutationAttempts: _maximumFailedAttempts);

			ThreadPool.GetMaxThreads(out var workerThreads, out var _);
			var tasks = new Task[workerThreads];
			for (int i = 0; i < tasks.Length; i++)
				tasks[i] = Task.Run(() => GenerateMutantsTask(loopControl));

			Task.WaitAll(tasks);
			return loopControl.UniqueModels;
		}

		private void GenerateMutantsTask(LoopControl loopControl) {
			while (loopControl.ContinueLooping) {
				var candidate = Random.Choice(loopControl.PreviousGenerationPopulation);
				var mutant = _individualMutator.TryMutate(candidate);
				Task.Run(() => loopControl.UpdateState(mutant));
			}
		}

		// Silly overrides
		public override string ToString() => throw new NotImplementedException();

		public override int GetHashCode() => throw new NotImplementedException();

		public override bool Equals(object? obj) => throw new NotImplementedException();

		private sealed class LoopControl {

			private readonly int _maximumFailedAttempts;
			private readonly int _populationSize;
			private readonly int _targetNumberOfMutants;
			private int _failedAttempts = 0;
			public readonly Array<ConsistentModel> PreviousGenerationPopulation;
			public readonly HashSet<ConsistentModel> UniqueModels;
			public bool ContinueLooping = true;

			public LoopControl(Array<ConsistentModel> previousGenerationPopulation, int targetNumberOfMutants, int maximumFailedMutationAttempts) {
				PreviousGenerationPopulation = previousGenerationPopulation;
				UniqueModels = new HashSet<ConsistentModel>(capacity: previousGenerationPopulation.Length + targetNumberOfMutants);
				for (int i = 0; i < previousGenerationPopulation.Length; i++)
					UniqueModels.Add(previousGenerationPopulation[i]);

				_populationSize = previousGenerationPopulation.Length;
				_maximumFailedAttempts = maximumFailedMutationAttempts;
				_targetNumberOfMutants = targetNumberOfMutants;
			}

			public void UpdateState(ConsistentModel? mutant) {
				if (mutant is null) {
					var updatedFailAttempts = Interlocked.Increment(ref _failedAttempts);
					ContinueLooping = updatedFailAttempts < _maximumFailedAttempts;
					return;
				}

				bool isUnique;
				int uniqueCount;
				lock (UniqueModels) {
					isUnique = UniqueModels.Add(mutant);
					uniqueCount = UniqueModels.Count;
				}

				if (!isUnique) {
					var updatedFailAttempts = Interlocked.Increment(ref _failedAttempts);
					ContinueLooping = updatedFailAttempts < _maximumFailedAttempts;
					return;
				}

				var mutantCount = uniqueCount - _populationSize;
				if (mutantCount >= _targetNumberOfMutants)
					ContinueLooping = false;
			}

			// Silly overrides
			public override string ToString() => throw new NotImplementedException();

			public override int GetHashCode() => throw new NotImplementedException();

			public override bool Equals(object? obj) => throw new NotImplementedException();
		}
	}
}
