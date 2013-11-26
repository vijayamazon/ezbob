namespace EZBob.DatabaseLib.Model.Database {
	public enum Gender {
		M,
		F
	} // enum Gender

	public enum MaritalStatus {
		Married,
		Single,
		Divorced,
		Widower,
		Other
	} // enum MaritalStatus

	public enum Medal {
		Silver,
		Gold,
		Platinum,
		Diamond
	} // enum Medal

	public enum TypeOfBusiness {
		Entrepreneur = 0, //consumer
		LLP = 1,          //company
		PShip3P = 2,      //consumer
		PShip = 3,        //company
		SoleTrader = 4,   //consumer
		Limited = 5       //company
	} // enum TypeOfBusiness
} // namespace
