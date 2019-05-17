using QuantumSoftProblem.Attributes;
using QuantumSoftProblem.Models;
using QuantumSoftProblem.QuantumSoft.Cache;
using QuantumSoftProblem.QuantumSoft.DataBase;
using QuantumSoftProblem.Utils;
using System;
using System.Web.Mvc;

namespace QuantumSoftProblem.Controllers
{
	public class HomeController : Controller
	{
		public CacheTree CacheTree => SessionManager.GetSessionData<CacheTree>("sessionNodeTree", new CacheTree());
		public DbContext DbContext => SessionManager.GetSessionData("dbContext", CreateDefaultRecords());

		[NoCache]
		public ActionResult Index()
		{
			var viewModel = new ViewModel()
			{
				Roots = CacheTree.Roots,
				Records = DbContext.GetAll()
			};
			return View(viewModel);
		}

		[HttpGet]
		public ActionResult DeleteNode(int key)
		{
			try
			{
				CacheTree.RemoveNode(key);
			}
			catch (Exception ex)
			{
				DebugUtils.WriteException(ex);
				SessionManager.ClearSession();
			}
			return RedirectToAction("Index");
		}

		[HttpGet]
		public ActionResult AlterNode(int key, string value)
		{
			try
			{
				CacheTree.AlterNode(key, value);
			}
			catch (Exception ex)
			{
				DebugUtils.WriteException(ex);
				SessionManager.ClearSession();
			}
			return RedirectToAction("Index");
		}

		[HttpGet]
		public ActionResult AddNode(int key, string value)
		{
			try
			{
				CacheTree.CreateChildNodeTo(key, value);
			}
			catch (Exception ex)
			{
				DebugUtils.WriteException(ex);
				SessionManager.ClearSession();
			}
			return RedirectToAction("Index");
		}

		[HttpGet]
		public ActionResult ApplyChanges()
		{
			try
			{
				var cacheService = new CacheService(DbContext, CacheTree);
				cacheService.DeployChanges();
				cacheService.DeployDeletions();
				cacheService.CheckRelevance();
				cacheService.Reset();
			}
			catch (Exception ex)
			{
				DebugUtils.WriteException(ex);
				SessionManager.ClearSession();
			}
			return RedirectToAction("Index");
		}

		[HttpGet]
		public ActionResult MoveNodes(int id)
		{
			try
			{
				var cacheService = new CacheService(DbContext, CacheTree);
				cacheService.CacheRecord(id);
			}
			catch (Exception ex)
			{
				DebugUtils.WriteException(ex);
				SessionManager.ClearSession();
			}
			return RedirectToAction("Index");
		}

		[HttpGet]
		public ActionResult ResetNodes()
		{
			SessionManager.ClearSession();
			return RedirectToAction("Index");
		}

		private DbContext CreateDefaultRecords()
		{
			var dbContext = new DbContext();
			dbContext.AddRecord(new Record() { Id = 1, Value = "Node1", IsActive = true, ParentId = 0 });

			dbContext.AddRecord(new Record() { Id = 2, Value = "Node2", IsActive = true, ParentId = 1 });
			dbContext.AddRecord(new Record() { Id = 3, Value = "Node3", IsActive = true, ParentId = 1 });
			dbContext.AddRecord(new Record() { Id = 4, Value = "Node4", IsActive = true, ParentId = 1 });

			dbContext.AddRecord(new Record() { Id = 5, Value = "Node5", IsActive = true, ParentId = 2 });
			dbContext.AddRecord(new Record() { Id = 6, Value = "Node6", IsActive = true, ParentId = 2 });

			dbContext.AddRecord(new Record() { Id = 7, Value = "Node7", IsActive = true, ParentId = 6 });

			dbContext.AddRecord(new Record() { Id = 8, Value = "Node8", IsActive = true, ParentId = 3 });
			dbContext.AddRecord(new Record() { Id = 9, Value = "Node9", IsActive = true, ParentId = 3 });

			dbContext.AddRecord(new Record() { Id = 12, Value = "Node12", IsActive = true, ParentId = 9 });

			dbContext.AddRecord(new Record() { Id = 10, Value = "Node10", IsActive = true, ParentId = 4 });
			dbContext.AddRecord(new Record() { Id = 11, Value = "Node11", IsActive = true, ParentId = 4 });

			dbContext.AddRecord(new Record() { Id = 13, Value = "Node13", IsActive = true, ParentId = 11 });
			dbContext.AddRecord(new Record() { Id = 14, Value = "Node14", IsActive = true, ParentId = 13 });
			return dbContext;
		}
	}
}
