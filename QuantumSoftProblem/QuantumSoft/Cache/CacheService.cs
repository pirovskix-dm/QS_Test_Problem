using QuantumSoftProblem.QuantumSoft.DataBase;
using QuantumSoftProblem.Utils;
using System.Collections.Generic;

namespace QuantumSoftProblem.QuantumSoft.Cache
{
	public class CacheService
	{
		private readonly DbContext dbContext;
		private readonly CacheTree cache;

		public CacheService(DbContext dbContext, CacheTree cache)
		{
			this.dbContext = dbContext;
			this.cache = cache;
		}

		/// <summary>
		/// Добавить запись в кэш
		/// </summary>
		public void CacheRecord(int id)
		{
			Record record = dbContext.GetRecord(id);
			if (record == null)
			{
				DebugUtils.WriteLine($"There is no record with {id} id");
			}
			cache.AddRecord(record);
		}

		/// <summary>
		/// Переношу все изменения из кеша в базу
		/// </summary>
		public void DeployChanges()
		{
			if (cache.Changes.Count < 1)
			{
				DebugUtils.WriteLine($"There is no changes to deploy");
			}

			foreach(CacheNode node in cache.Changes)
			{
				// Обновляю запись если таковая есть
				Record record = dbContext.UpdateRecord(node.Record.Id, node.Value);

				// Если запись была удалена до обновления
				if (node.IsActive && record?.IsActive != true)
				{
					node.Remove();
				}

				if (record != null)
				{
					// Рекурсивно добавляю новые дочерние ноды в базу
					AddNewRecords(node.NewNodes, node.Record.Id);
				}

				// Сбрасываю отслеживание изменений в ноде
				node.Reset();
			}
		}

		/// <summary>
		/// Переношу все удаления из кеша в базу
		/// </summary>
		public void DeployDeletions()
		{
			if (cache.Deletions.Count < 1)
			{
				DebugUtils.WriteLine($"There is nothing to delete");
			}

			foreach (CacheNode node in cache.Deletions)
			{
				// Рекурсивно удаляю нод и все его дочерние ноды
				dbContext.DeleteWithChilds(node.Record.Id);
			}
		}

		public void CheckRelevance()
		{
			foreach(CacheNode node in cache.Records)
			{
				Record record = dbContext.GetRecord(node.Record.Id);
				if (node.IsActive && record?.IsActive != true)
				{
					node.Remove();
				}
			}
		}

		public void Reset()
		{
			// Сбрасываю отслеживание изменений в кеше
			cache.Reset();
		}

		/// <summary>
		/// // Рекурсивно добавляю новые ноды в базу
		/// </summary>
		private void AddNewRecords(List<CacheNode> nodes, int parentId)
		{
			if (nodes.Count < 1)
			{
				DebugUtils.WriteLine($"There is nothing to add");
			}

			foreach (CacheNode node in nodes)
			{
				// Создаю новузапись
				var newRecord = new Record()
				{
					Value = node.Value,
					ParentId = parentId,
					IsActive = true
				};
				dbContext.AddRecord(newRecord);

				// Присваиваю эту запись ноду
				cache.SetRecord(node, newRecord);

				// Повторяю для дочерних нодов
				AddNewRecords(node.NewNodes, newRecord.Id);

				node.Reset();
			}
		}
	}
}