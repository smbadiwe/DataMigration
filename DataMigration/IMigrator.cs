using NHibernate;

namespace DataMigration
{
    public interface IMigrator
    {
        /// <summary>
        /// Typically, you'll need to write code to load each data (table) you're interested in.
        /// Runner.MoveData has been specially designed to ease that work. So, just use it
        /// </summary>
        /// <param name="fromSession"></param>
        /// <param name="toSession"></param>
        /// <param name="migrateDataMappedInCustomAssembly"></param>
        void Migrate(ISession fromSession, ISession toSession, bool migrateDataMappedInCustomAssembly = false);
    }
}
