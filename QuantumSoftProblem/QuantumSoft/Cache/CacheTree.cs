using QuantumSoftProblem.QuantumSoft.DataBase;
using QuantumSoftProblem.Utils;
using System.Collections.Generic;
using System.Linq;

namespace QuantumSoftProblem.QuantumSoft.Cache
{
	public class CacheTree
	{
		public IEnumerable<CacheNode> Roots => GetRootNodes(); // Ноды у которых нет родительских нодов
		public HashSet<CacheNode> Changes { get; private set; } // Отслеживание измененных нодов
		public List<CacheNode> Deletions { get; private set; } // Отслеживание удалений
		public IEnumerable<CacheNode> Records => GetRecords();

		private int currentKey = 0;
		private readonly Dictionary<int, CacheNode> cached; // индексированный сет всех нодов для быстрого поиска и доступа
		private readonly Dictionary<int, CacheNode> records; // индексированный сет нодов которые уже есть в базе

		public CacheTree()
		{
			cached = new Dictionary<int, CacheNode>();
			records = new Dictionary<int, CacheNode>();
			Changes = new HashSet<CacheNode>();
			Deletions = new List<CacheNode>();
		}

		/// <summary>
		/// Создаю новый нод, и добавляю его к родительскому по ключу.
		/// </summary>
		public CacheNode CreateChildNodeTo(int parentkey, string value)
		{
			// Если невозможно найти родительский нод по ключу то проблема с построением дерева
			if (!cached.ContainsKey(parentkey))
			{
				DebugUtils.WriteLine($"The parent with key {parentkey} does not exists");
				return null;
			}

			currentKey++;
			CacheNode parentNode = cached[parentkey];
			CacheNode newNode = CreateNode(value, parent: parentNode);

			// Помечаю родительский нод как измененный, так как у него появился новывй дочерний нод.
			TrackChanges(parentNode);

			return newNode;
		}

		/// <summary>
		/// Добавляю запись из базы в дерево
		/// </summary>
		public CacheNode AddRecord(Record record)
		{
			if (records.ContainsKey(record.Id))
			{
				DebugUtils.WriteLine($"The record {record.Id} already exists");
				return records[record.Id];
			}

			// Пытаюсь найти родительских нод для этой записи
			CacheNode parent = records.ContainsKey(record.ParentId) ? records[record.ParentId] : null;
			var newNode = CreateNode(record.Value, parent, record);

			// Ищу ноды для которых новый нод может быть родительским, и назначаю его как родительский
			AsignChildsToNewParent(newNode);

			return newNode;
		}

		/// <summary>
		/// Присваиваю для существующего нода конкретную запись
		/// </summary>
		public void SetRecord(CacheNode node, Record record)
		{
			node.Record = record;
			records.Add(record.Id, node);
		}

		/// <summary>
		/// Удаляю нод
		/// </summary>
		public void RemoveNode(int key)
		{
			// Если невозможно найти нод по ключу то проблема с построением дерева
			if (!cached.ContainsKey(key))
			{
				DebugUtils.WriteLine($"The Node with key {key} does not exists");
				return;
			}

			CacheNode node = cached[key];
			node.Remove();

			// Если нод новый, просто убираю его, иначе отслеживаю удаление
			if (node.IsNew())
			{
				cached.Remove(node.Key);
			}
			else
			{
				TrackDeletion(node);
			}
		}

		/// <summary>
		/// Изменяю значение нода
		/// </summary>
		public CacheNode AlterNode(int key, string value)
		{
			if (!cached.ContainsKey(key))
			{
				DebugUtils.WriteLine($"Node with key {key} does not exists");
				return null;
			}

			CacheNode node = cached[key];
			node.Value = value ?? string.Empty;
			TrackChanges(node);
			return node;
		}

		/// <summary>
		/// Сбрасываю отслеживание кеша
		/// </summary>
		public void Reset()
		{
			Changes = new HashSet<CacheNode>();
			Deletions = new List<CacheNode>();
		}

		private CacheNode CreateNode(string value, CacheNode parent = null, Record record = null)
		{
			currentKey++;
			CacheNode newNode = new CacheNode(currentKey)
			{
				Record = record,
				Parent = parent,
				Value = record?.Value ?? value ?? string.Empty,
				IsActive = parent?.IsActive ?? true
			};
			parent?.Add(newNode);
			cached.Add(currentKey, newNode);

			if (record != null)
			{
				records.Add(record.Id, newNode);
			}

			return newNode;
		}

		private void TrackChanges(CacheNode node)
		{
			if (!node.IsNew())
			{
				Changes.Add(node);
			}
		}

		/// <summary>
		/// Пытаюсь отследить удаление только самого верхнего удаленного нода в иерархии.
		/// Т.к. все равно придется удалять ноды из базы рекурсивно
		/// </summary>
		private void TrackDeletion(CacheNode node)
		{
			for (int i = Deletions.Count - 1; i >= 0; i--)
			{
				if (Deletions[i].IsChildOf(node))
				{
					Deletions.RemoveAt(i);
				}
			}
			Deletions.Add(node);
		}

		private IEnumerable<CacheNode> GetRootNodes()
		{
			return cached.Where(n => n.Value.Parent == null).Select(n => n.Value);
		}

		private IEnumerable<CacheNode> GetRecords()
		{
			return records.Select(n => n.Value);
		}

		/// <summary>
		/// Ищу ноды для которых новый данный нод может быть родительским, и назначаю его как родительский
		/// </summary>
		/// <param name="parent"></param>
		private void AsignChildsToNewParent(CacheNode parent)
		{
			IEnumerable<CacheNode> childs = records.Where(n => n.Value.Record.ParentId == parent.Record.Id).Select(n => n.Value);
			foreach (CacheNode c in childs)
			{
				parent.Add(c);
				c.Parent = parent;

				if (!parent.IsActive)
				{
					c.Remove();
				}
			}
		}
	}
}