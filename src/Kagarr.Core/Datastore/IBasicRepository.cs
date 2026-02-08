using System.Collections.Generic;

namespace Kagarr.Core.Datastore
{
    public interface IBasicRepository<TModel>
        where TModel : ModelBase, new()
    {
        IEnumerable<TModel> All();
        int Count();
        TModel Get(int id);
        TModel Insert(TModel model);
        TModel Update(TModel model);
        void Delete(int id);
        void Delete(TModel model);
        IEnumerable<TModel> Get(IEnumerable<int> ids);
        void InsertMany(IList<TModel> models);
        void UpdateMany(IList<TModel> models);
        void DeleteMany(List<TModel> models);
        void DeleteMany(IEnumerable<int> ids);
        bool HasItems();
    }
}
