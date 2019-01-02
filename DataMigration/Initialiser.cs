using FluentNHibernate.Automapping;
using NHibernate;
using System;
using System.Collections.Generic;
using MultiTenancyFramework.NHibernate.NHManager;

namespace DataMigration
{
    public class Initialiser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mappingAssemblies">NB: Make sure these DLLs are in the BIN</param>
        /// <param name="fromSession"></param>
        /// <param name="nhConfigFileName">Default is 'DataFrom'. Don't bother with the '.config' extension</param>
        /// <param name="autoPersistenceModel"></param>
        public static void Init(string[] mappingAssemblies, out ISession fromSession, string nhConfigFileName = null, AutoPersistenceModel autoPersistenceModel = null)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrWhiteSpace(nhConfigFileName)) nhConfigFileName = "DataFrom";
            var fromDir = baseDir + nhConfigFileName + ".config";

            var fromDatasource = "fromdatasource";

            List<string> assemblyFiles = new List<string>();
            if (mappingAssemblies != null && mappingAssemblies.Length > 0)
            {
                foreach (var mappingAssembly in mappingAssemblies)
                {
                    if (!string.IsNullOrWhiteSpace(mappingAssembly))
                    {
                        assemblyFiles.Add(mappingAssembly); //baseDir + 
                    }
                }
            }
            Console.WriteLine("Initializing Service Locator...");
            var ssl = new ServiceLocatorInit();
            Microsoft.Practices.ServiceLocation.ServiceLocator.SetLocatorProvider(() => ssl);

            //Sesson Factory - From
            Console.WriteLine("Initializing 'From' session factory...");
            
            NHibernateSessionManager.Init(new SimpleSessionStorage(), assemblyFiles.ToArray(), autoPersistenceModel, fromDir, NHibernateSessionManager.GetSessionKey(fromDatasource, fromDatasource, false));

            Console.WriteLine("Initializing 'From' session...");
            fromSession = NHibernateSessionManager.GetSession(fromDatasource, fromDatasource, null, false);

            Console.WriteLine("Done Initializing!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mappingAssemblies">NB: Make sure these DLLs are in the BIN</param>
        /// <param name="fromSession"></param>
        /// <param name="toSession"></param>
        /// <param name="autoPersistenceModel"></param>
        public static void Init(string[] mappingAssemblies, out ISession fromSession, out ISession toSession, AutoPersistenceModel autoPersistenceModel = null)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var fromDir = baseDir + "DataFrom.config";
            var toDir = baseDir + "DataTo.config";

            var fromDatasource = "fromdatasource";
            var toDatasource = "todatasource";

            List<string> assemblyFiles = new List<string>();
            if (mappingAssemblies != null && mappingAssemblies.Length > 0)
            {
                foreach (var mappingAssembly in mappingAssemblies)
                {
                    if (!string.IsNullOrWhiteSpace(mappingAssembly))
                    {
                        assemblyFiles.Add(mappingAssembly); //baseDir + 
                    }
                }
            }
            Console.WriteLine("Initializing Service Locator...");
            var ssl = new ServiceLocatorInit();
            Microsoft.Practices.ServiceLocation.ServiceLocator.SetLocatorProvider(() => ssl);

            //Sesson Factory - To
            Console.WriteLine("Initializing 'To' session factory...");
            NHibernateSessionManager.Init(new SimpleSessionStorage(), assemblyFiles.ToArray(), autoPersistenceModel, toDir, NHibernateSessionManager.GetSessionKey(toDatasource, toDatasource, false));

            //Sesson Factory - From
            Console.WriteLine("Initializing 'From' session factory...");
            NHibernateSessionManager.Init(new SimpleSessionStorage(), assemblyFiles.ToArray(), autoPersistenceModel, fromDir, NHibernateSessionManager.GetSessionKey(fromDatasource, fromDatasource, false));

            Console.WriteLine("Initializing 'From' session...");
            fromSession = NHibernateSessionManager.GetSession(fromDatasource, fromDatasource, null, false);
            Console.WriteLine("Initializing 'To' session...");
            toSession = NHibernateSessionManager.GetSession(toDatasource, toDatasource, null, false);

            Console.WriteLine("Done Initializing!");
        }
    }
}
