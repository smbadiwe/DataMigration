using DataMigration;
using NHibernate;

namespace DataMigrationConsole
{
    class Migrator : IMigrator
    {
        public void Migrate(ISession fromSession, ISession toSession, bool migrateDataMappedInCustomAssembly = false)
        {
            // Now add the DTO classes you want to migrate
            //Runner.MoveData<Function>(fromSession, toSession, "Functions");
            //Runner.MoveData<UserRole>(fromSession, toSession, "UserRoles");
            //Runner.MoveData<UserRoleFunction>(fromSession, toSession, "UserRoleFunctions");
        }
    }
}
