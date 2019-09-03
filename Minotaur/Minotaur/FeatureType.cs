namespace Minotaur {

	/// <summary>
	/// Since all values in a <see cref="Minotaur.Collections.Dataset.Dataset"/>
	/// are stored as floats, including values of categorical features,
	/// this is enum is used to indicate how a dataset column (representing a feature)
	/// should be treated.
	/// 
	/// The value <see cref="FeatureType.Categorical"/> indicates that
	/// the feature has a categorical nature (e.g. hair color) 
	/// and must be treated as such.
	/// For instance, two values from a column marked with
	/// <see cref="FeatureType.Categorical"/>
	/// should not be compared with the "less than operator".
	/// 
	/// The value <see cref="FeatureType.CategoricalButTriviallyValued"/> indicates
	/// that the featuyre has a categorical nature,
	/// but contains all instances in the dataset have the same value.
	/// Usually this is a (undersided) side-effect of the k-fold technique;
	/// but it may also indicate that the dataset is really well done,
	/// e.g.: a dataset that for predicting whether a woman has breast cancer
	/// with the column "IsHuman"... 
	/// Since all women are human, all instances will have the value "true".
	/// 
	/// The value <see cref="FeatureType.ContinuousButTriviallyValued"/> indicates
	/// that the has a continuous nature,
	/// but contains all instances in the dataset have the same value.
	/// Usually this is a (undersided) side-effect of the k-fold technique.
	/// </summary>
	public enum FeatureType {
		Categorical,
		Continuous,
		CategoricalButTriviallyValued,
		ContinuousButTriviallyValued
	}
}
