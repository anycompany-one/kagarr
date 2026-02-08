using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using Dapper;

namespace Kagarr.Core.Datastore
{
    public class BasicRepository<TModel> : IBasicRepository<TModel>
        where TModel : ModelBase, new()
    {
        protected readonly IDatabase _database;
        protected readonly string _table;

        public BasicRepository(IDatabase database)
        {
            _database = database;
            _table = typeof(TModel).Name + "s";
        }

        public IEnumerable<TModel> All()
        {
            using var conn = _database.OpenConnection();
            return conn.Query<TModel>($"SELECT * FROM \"{_table}\"");
        }

        public int Count()
        {
            using var conn = _database.OpenConnection();
            return conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM \"{_table}\"");
        }

        public TModel Get(int id)
        {
            using var conn = _database.OpenConnection();
            var result = conn.QuerySingleOrDefault<TModel>($"SELECT * FROM \"{_table}\" WHERE \"Id\" = @Id", new { Id = id });
            if (result == null)
            {
                throw new ModelNotFoundException(typeof(TModel), id);
            }

            return result;
        }

        public TModel Insert(TModel model)
        {
            using var conn = _database.OpenConnection();
            var properties = typeof(TModel).GetProperties()
                .Where(p => p.Name != "Id" && p.CanRead)
                .Select(p => p.Name)
                .ToList();

            var columns = string.Join(", ", properties.Select(p => $"\"{p}\""));
            var values = string.Join(", ", properties.Select(p => $"@{p}"));

            var sql = $"INSERT INTO \"{_table}\" ({columns}) VALUES ({values}); SELECT last_insert_rowid();";
            model.Id = conn.ExecuteScalar<int>(sql, model);
            return model;
        }

        public TModel Update(TModel model)
        {
            using var conn = _database.OpenConnection();
            var properties = typeof(TModel).GetProperties()
                .Where(p => p.Name != "Id" && p.CanRead)
                .Select(p => p.Name)
                .ToList();

            var setClause = string.Join(", ", properties.Select(p => $"\"{p}\" = @{p}"));
            var sql = $"UPDATE \"{_table}\" SET {setClause} WHERE \"Id\" = @Id";
            conn.Execute(sql, model);
            return model;
        }

        public void Delete(int id)
        {
            using var conn = _database.OpenConnection();
            conn.Execute($"DELETE FROM \"{_table}\" WHERE \"Id\" = @Id", new { Id = id });
        }

        public void Delete(TModel model)
        {
            Delete(model.Id);
        }

        public IEnumerable<TModel> Get(IEnumerable<int> ids)
        {
            using var conn = _database.OpenConnection();
            return conn.Query<TModel>($"SELECT * FROM \"{_table}\" WHERE \"Id\" IN @Ids", new { Ids = ids });
        }

        public void InsertMany(IList<TModel> models)
        {
            foreach (var model in models)
            {
                Insert(model);
            }
        }

        public void UpdateMany(IList<TModel> models)
        {
            foreach (var model in models)
            {
                Update(model);
            }
        }

        public void DeleteMany(List<TModel> models)
        {
            DeleteMany(models.Select(m => m.Id));
        }

        public void DeleteMany(IEnumerable<int> ids)
        {
            using var conn = _database.OpenConnection();
            conn.Execute($"DELETE FROM \"{_table}\" WHERE \"Id\" IN @Ids", new { Ids = ids });
        }

        public bool HasItems()
        {
            return Count() > 0;
        }

        protected List<TModel> Query(Expression<Func<TModel, bool>> where)
        {
            // Simple implementation - for complex queries, override in derived classes
            return All().AsQueryable().Where(where).ToList();
        }
    }

    public class ModelNotFoundException : Exception
    {
        public ModelNotFoundException(Type modelType, int id)
            : base($"{modelType.Name} with ID {id} was not found")
        {
        }
    }
}
