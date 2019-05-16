namespace QuantumSoftProblem.QuantumSoft.DataBase
{
	public class Record
	{
		public int Id { get; set; }
		public int ParentId { get; set; }
		public string Value { get; set; }
		public bool IsActive { get; set; }
	}
}