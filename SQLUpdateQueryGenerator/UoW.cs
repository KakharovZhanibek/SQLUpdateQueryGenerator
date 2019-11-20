using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SQLUpdateQueryGenerator
{
    public interface IWithKey
    {
        Guid Id { get; set; }
    }

    /// <summary>
    /// Unit of Work
    /// </summary>
    public class UnitOfWork<T> where T : IWithKey
    {
        private readonly ICollection<T> _collection;
        public UnitOfWork()
        {
            _collection = new List<T>();
        }
        public void Add(T item)
        {
            _collection.Add(item);
        }
        public T Find(Guid id) => 
            _collection.SingleOrDefault(p => p.Id == id);

        private T GetById(Guid id)
        {
            return _collection.SingleOrDefault(x => x.Id == id);
        }

        private bool IsModified(
            object sourceObject,
            object destinationObject,
            PropertyInfo sourceProperty, 
            PropertyInfo destinationProperty)
        {
            if (sourceObject == null) throw new ArgumentNullException(nameof(sourceObject));
            if (destinationObject == null) throw new ArgumentNullException(nameof(destinationObject));

            if (sourceProperty.GetType() != destinationProperty.GetType())
                return false;

            return sourceProperty.GetValue(sourceObject) != 
                destinationProperty.GetValue(destinationObject);
        } 

        public Dictionary<PropertyInfo, (string from, string to)> GetModifiedValues(
            T sourceObject, 
            T destinationObject)
        {
            var dict = new Dictionary<PropertyInfo, (string from, string to)>();
            var sourceProperties = sourceObject.GetType().GetProperties();
            var destinationProperties = destinationObject.GetType().GetProperties();

            foreach (var sourceProperty in sourceProperties)
            {
                foreach (var destinationProperty in destinationProperties)
                {
                    bool isModified = IsModified(
                        sourceObject, 
                        destinationProperties, 
                        sourceProperty, 
                        destinationProperty);

                    dict[sourceProperty] = (
                        sourceProperty.GetValue(sourceObject).ToString(), 
                        destinationProperty.GetValue(destinationObject).ToString());
                }
            }

            return dict;
        }


        //private string GetQuotesFormatForType(Type type)
        //{
        //    string quotesFormat = string.Empty;

        //    if (type.FullName == "System.String")
        //    {
        //        quotesFormat = "{0} = '{1}'";
        //    }
        //    else
        //    {
        //        quotesFormat = "{0} = {1}";
        //    }

        //    return quotesFormat;
        //}

        //public string GetDiffPartForUpdateSql(Dictionary<PropertyInfo, (string from, string to)> dict)
        //{
        //    var sb = new StringBuilder();
        //    sb.Append("SET ");
        //    foreach (var kvp in dict)
        //    {
        //        string format = GetQuotesFormatForType(kvp.Key.GetType());
        //        string expression = string.Format(format, kvp.Key.Name, kvp.Value.to);
        //        sb.Append(",");
        //        sb.Append(expression);
        //    }

        //    sb.Remove(sb.Length - 1, 1);
        //    return sb.ToString();
        //}

        //public string Update(T entity)
        //{
        //    var source = GetById(entity.Id);
        //    if (source == null) throw new InvalidOperationException("Not found!");

        //    var modifiedValues = GetModifiedValues(source, entity);
        //    string partialSql = GetDiffPartForUpdateSql(modifiedValues);

        //    return partialSql;
        //}


        public string Update(T entity)
        {
            var itemToUpdate = _collection.Where(x => x.Id == entity.Id).FirstOrDefault();
            var type = entity.GetType();
            var props = type.GetProperties();
            var itemToUpdateProps = itemToUpdate.GetType().GetProperties();
            var updateQuery = string.Empty;

            foreach (var prop in props)
            {
                foreach (var updProp in itemToUpdateProps)
                {
                    if (prop.Name == updProp.Name)
                    {
                        if (prop.GetValue(itemToUpdate).ToString() != updProp.GetValue(entity).ToString())
                        {
                            if (updateQuery == string.Empty)
                            {
                                updateQuery += $"update {type.Name} set {updProp.Name} = {updProp.GetValue(entity)}";
                            }
                            else
                            {
                                updateQuery += $",{updProp.Name} = {updProp.GetValue(entity)}\n";
                            }
                        }
                    }
                }
            }
            updateQuery += $"where {props[0].Name} = {itemToUpdate.Id}";

            return updateQuery;
        }
    }
}
