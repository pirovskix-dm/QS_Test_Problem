using QuantumSoftProblem.QuantumSoft.DataBase;
using QuantumSoftProblem.Utils;
using System;
using System.Collections.Generic;

namespace QuantumSoftProblem.QuantumSoft.Cache
{
	public class CacheNode : IEquatable<CacheNode>
	{
		public string Value { get; set; }
		public int Key { get; }
		public Record Record { get; set; }
		public CacheNode Parent { get; set; }
		public bool IsActive { get; set; }
		public List<CacheNode> ChildeNodes { get; }
		public List<CacheNode> NewNodes { get; private set; } // Отслеживание добавления новых нодов

		public CacheNode(int key)
		{
			this.Key = key;
			ChildeNodes = new List<CacheNode>();
			NewNodes = new List<CacheNode>();
		}

		/// <summary>
		/// Помечаю это нод и все дочерние ноды как неактивные.
		/// Eсли это новый нод, и еще не присутствует в базе,
		/// то удаляю его.
		/// </summary>
		public void Remove()
		{
			IsActive = false;

			if (IsNew())
			{
				Parent?.ChildeNodes.Remove(this);
				Parent?.NewNodes.Remove(this);
			}
			else
			{
				// Если нод не новый, то рекурсивно ищу дочерние ноды которые нужно пометить удаленными
				RemoveRelated();
			}
		}

		/// <summary>
		/// Добавляет новый дочерний нод к текущему.
		/// </summary>
		public void Add(CacheNode node)
		{
			if(node == null)
			{
				DebugUtils.WriteLine("node is null");
			}

			ChildeNodes.Add(node);
			if (node.IsNew())
			{
				NewNodes.Add(node);
			}
		}


		/// <summary>
		/// Проверить находится ли нод в иерархии родительского нода
		/// </summary>
		public bool IsChildOf(CacheNode parent)
		{
			if (parent == null)
			{
				DebugUtils.WriteLine("parent is null");
				return false;
			}

			CacheNode curent = this;
			while (curent != null)
			{
				curent = curent.Parent;
				if (curent == parent)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsNew()
		{
			return Record == null;
		}

		public void Reset()
		{
			NewNodes = new List<CacheNode>();
		}

		public override bool Equals(object obj)
		{
			return (obj is CacheNode node) && this == node;
		}

		public bool Equals(CacheNode other)
		{
			return this == other;
		}

		public override int GetHashCode()
		{
			return this.Key.GetHashCode();
		}

		public static bool operator ==(in CacheNode left, in CacheNode right)
		{
			return int.Equals(left?.Key, right?.Key);
		}

		public static bool operator !=(in CacheNode left, in CacheNode right)
		{
			return !(left == right);
		}

		private void RemoveRelated()
		{
			for(int i = ChildeNodes.Count - 1; i >= 0; i--)
			{
				if (ChildeNodes[i].IsActive)
				{
					ChildeNodes[i].Remove();
				}
			}
		}
	}
}