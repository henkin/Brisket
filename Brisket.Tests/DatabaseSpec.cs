using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Brisket.Tests
{
    /*
    public void Create(Entity entity)
    public void Update(Entity entity)
    public void Delete(Entity entity)
    public T GetByID<T>(Guid id) where T : class, IIdentifiable
   
    public IQueryable<T> Repo<T>() where T : class, IIdentifiable
    */
    [TestFixture]
    public class DatabaseSpec
    {
        [Test]
        public void Create_Update_Delete()
        {
            var db = new Database<TestEntity>();
            var testEntity = new TestEntity() { Name = "testEntity" };

            // CREATE
            db.Create(testEntity);
            var actual = db.GetByID(testEntity.ID);
            Assert.AreEqual(testEntity, actual);

            // UPDATE
            actual.Name = "updatedName";
            db.Update(actual);
            actual = db.GetByID(testEntity.ID);
            Assert.AreEqual("updatedName", actual.Name);

            // DELETE
            db.Delete(actual);
            Assert.Throws<FileNotFoundException>(() => db.GetByID(testEntity.ID));
        }

        [Test]
        public void GetAll()
        {
            var db = new Database<TestEntity>();
            var testEntity = AddNewTestEntity(db,"testEntity");
            var testEntity2 = AddNewTestEntity(db, "testEntity2");
            var testEntity3 = AddNewTestEntity(db, "testEntity3");

            var actual = db.GetAll();
            Assert.AreEqual(
                new TestEntity[] {testEntity, testEntity2, testEntity3},
                actual
                );


            // UPDATE
            //actual.Name = "updatedName";
            //db.Update(actual);
            //actual = db.GetByID(testEntity.ID);
            //Assert.AreEqual("updatedName", actual.Name);

            //// DELETE
            //db.Delete(actual);
            //Assert.Throws<FileNotFoundException>(() => db.GetByID(testEntity.ID));

        }

        private static TestEntity AddNewTestEntity(Database<TestEntity> db, string name)
        {
            var testEntity = new TestEntity() { Name = name };
            db.Create(testEntity);
            return testEntity;
        }

        [Test]
        [Ignore]
        public void Queryable()
        {
            var testEntity = new TestEntity() { Name = "testEntity" };
            var db = new Database<TestEntity>();

            db.Create(testEntity);

            //var entities = db.Repo<TestEntity>();

            //var query = from place in entities
            //            where place.Name == "testEntity"
            //            select place.ID;

            //Assert.AreEqual(1, query.ToList().Count());
            
        }




        [Test]
        public void GetEntityPath_NoSubclass()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var entity = new TestEntity();
            var db = new Database<TestEntity>();

            var actual = db.GetEntityPath(entity, dir);
            Assert.AreEqual(Path.Combine(dir, entity.GetType().Name, entity.ID + ".json"), actual);
        }

        [Test]
        public void GetEntityPath_Subclass()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var entity = new TestSubclass();
            var db = new Database<TestEntity>();


            var actual = db.GetEntityPath(entity, dir);
            Assert.AreEqual(Path.Combine(dir, entity.GetType().BaseType.Name, entity.ID + ".json"), actual);
        }
    }

    

    public class TestEntity : IEntity   
    {
        public Guid ID { get; set; }
        public string Name { get; set; }

        public TestEntity()
        {
            ID = Guid.NewGuid();
        }

        public override bool Equals(object obj)
        {
            var testEntity = obj as TestEntity;
            return 
                ID == testEntity.ID &&
                Name == testEntity.Name;
        }

        public override string ToString()
        {
            return string.Format("{0}, Name: '{1}'", ID, Name);
        }
    }

    public class TestSubclass : TestEntity
    {
        
    }
}
