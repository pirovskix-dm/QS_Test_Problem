using QuantumSoftProblem.QuantumSoft.Cache;
using QuantumSoftProblem.QuantumSoft.DataBase;
using System.Collections.Generic;

namespace QuantumSoftProblem.Models
{
	public class ViewModel
	{
		public IEnumerable<CacheNode> Roots { get; set; }
		public IEnumerable<Record> Records { get; set; }
	}
}