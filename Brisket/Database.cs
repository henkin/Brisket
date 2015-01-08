using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using NLog;

namespace Brisket.Tests
{
    public interface IDatabase<T> where T : class, IEntity, new()
    {
        void Create(IEntity entity);
        void Update(IEntity entity);
        void Delete(IEntity entity);
        T GetByID(Guid id);
        //IQueryable<T> Repo<T>() where T : class, IEntity;
        IEnumerable<T> GetAll();
        IEnumerable<T> Find(Expression<Func<T, bool>> specification);

    }

    public class Database<T> : IDatabase<T> where T : class, IEntity, new()
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private const string DatabaseSubdirectory = ".brisket";
        public string DatabasePath { get; set; }

        public Database(string location = null)
        {
            var baseDirectory = location ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DatabasePath = Path.Combine(baseDirectory, DatabaseSubdirectory);
            InitDatabaseDirectory(DatabasePath);
        }

        private void InitDatabaseDirectory(string databasePath)
        {
            try
            {
                _logger.Debug("Creating {0} directory", DatabaseSubdirectory);
                Directory.CreateDirectory(DatabaseSubdirectory);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed InitDatabaseDirectory: {0}", ex);
                throw;
            }
        }

        public void Create(IEntity entity)
        {
            try
            {
                SerializeAndSave(entity);
            }
            catch (Exception exception)
            {
                _logger.Error(exception);                
                throw;
            }
        }

        public void Update(IEntity entity)
        {
            try
            {
                SerializeAndSave(entity);
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
                throw;
            }
        }

        public void Delete(IEntity entity)
        {
            try
            {
                File.Delete(GetEntityPath(entity, DatabasePath));
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
                throw;
            } 
        }

        public T GetByID(Guid id)
        {
            string entityDatabasePath = GetEntityPath(new T() { ID = id }, DatabasePath);
            return Deserialize<T>(entityDatabasePath) as T;
        }

        public IEnumerable<T> GetAll()
        {
            var entityDir = GetEntityDirectoryPath();
            var list = new List<T>();
            foreach (var entityFile in Directory.GetFiles(entityDir, "*.json")) 
            {
                list.Add(GetByID(new Guid(Path.GetFileNameWithoutExtension(entityFile))));
            }
            return list;
        }

        public IEnumerable<T> Find(Expression<Func<T, bool>> specification)
        {
            return null;
        }

        private void SerializeAndSave(IEntity entity)
        {
            string serializedEntity = Serialize(entity);
            string entityDatabasePath = GetEntityPath(entity, DatabasePath);
            Directory.CreateDirectory(GetEntityDirectoryPath());
            File.WriteAllText(entityDatabasePath, serializedEntity);
        }
        
        private string Serialize(IEntity entity)
        {
            var serializedEntity = JsonConvert.SerializeObject(entity, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
            return serializedEntity;
        }

        private T Deserialize<T>(string entityDatabasePath)
        {
            var deserializedEntity = JsonConvert.DeserializeObject<T>(File.ReadAllText(entityDatabasePath));
            return deserializedEntity;
        }

        public string GetEntityDirectoryPath()
        {
            var type = typeof(T);
            while (type.BaseType != typeof(object))
                type = type.BaseType;
            return Path.Combine(DatabasePath, type.Name);

        }

        public string GetEntityPath(IEntity entity, string databasePath)
        {
            return Path.Combine(databasePath, GetEntityDirectoryPath(), entity.ID + ".json");
        }

        //public IQueryable<T> Repo<T>() where T : class, IEntity
        //{
        //    //var queryable = new QueryableDatabaseData<T>(this);
        //    //return queryable;

        //    throw new NotImplementedException();
        //}
    }
}
