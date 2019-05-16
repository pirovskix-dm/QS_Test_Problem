using QuantumSoftProblem.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantumSoftProblem.QuantumSoft.DataBase
{
	public class DbContext
	{
		private readonly List<Record> records;

		private int index = 0;

		public DbContext()
		{
			records = new List<Record>();
		}

		public IEnumerable<Record> GetAll()
		{
			return records;
		}

		public Record GetRecord(int id)
		{
			if (id < 1)
			{
				DebugUtils.WriteLine("id is 0");
				return null;
			}

			return records.Find(r => r.Id == id);
		}

		public Record GetRootRecord()
		{
			return records.Find(r => r.ParentId < 1);
		}

		public IEnumerable<Record> GetChilds(int id)
		{
			if (id < 1)
			{
				DebugUtils.WriteLine("id is 0");
				return Array.Empty<Record>();
			}

			return records.Where(rec => rec.ParentId == id);
		}

		public void AddRecord(Record record)
		{
			if (record == null)
			{
				DebugUtils.WriteLine("record is null");
			}

			record.Id = ++index;
			records.Add(record);
		}

		public void UpdateRecord(int id, string value)
		{
			Record r = GetRecord(id);
			if (r != null)
			{
				r.Value = value ?? string.Empty;
			}
			else
			{
				DebugUtils.WriteLine($"There is no record with {id} id");
			}
		}

		public void DeleteRecord(int id)
		{
			Record record = GetRecord(id);
			if (record != null)
			{
				record.IsActive = false;
			}
			else
			{
				DebugUtils.WriteLine($"There is no record with {id} id");
			}
		}

		public void DeleteWithChilds(int id)
		{
			if (id < 1)
			{
				DebugUtils.WriteLine("id is 0");
				return;
			}

			Record record = GetRecord(id);
			if (record != null)
			{
				record.IsActive = false;
				foreach (Record r in GetChilds(id))
				{
					DeleteWithChilds(r.Id);
				}
			}
		}
	}
}