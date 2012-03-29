using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using NHibernate.Tool.hbm2ddl;

namespace InMemoryDatabaseAcrossSessions
{
    class Program
    {
        static void Main(string[] args)
        {
            var cfg =
                Fluently.Configure().Database(SQLiteConfiguration.Standard.InMemory().ShowSql())
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Person>());

            SchemaExport export = new SchemaExport(cfg.BuildConfiguration());

            var con = new System.Data.SQLite.SQLiteConnection("Data Source=:memory:;Version=3;New=True");
            con.Open();
            export.Execute(true, true, false, con, null);

            var sessionFactory = cfg.BuildSessionFactory();

            var session1 = sessionFactory.OpenSession(con);
            Guid graemeId = Guid.Empty;
            for (int i = 0; i < 1000; i++)
            {
                var graeme = new Person { Name = "Graeme Foster" };
                session1.Save(graeme);
                graemeId = graeme.Id;
            }
            session1.Flush();
            session1.Dispose();

            var session2 = sessionFactory.OpenSession(con);
            var graeme2 = session2.Get<Person>(graemeId);
            Console.WriteLine(graeme2.Name);


            Console.ReadLine();
        }
    }

    public class Person
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
    }

    public class PersonMap : ClassMap<Person>
    {
        public PersonMap()
        {
            Id(o => o.Id).GeneratedBy.GuidComb();
            Map(o => o.Name);
        }
    }
}
