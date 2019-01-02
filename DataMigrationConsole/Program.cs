using System;

namespace DataMigrationConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //string[] customAssemblies = new[]
            //                            {
            //                                "SOMA.Framework",
            //                                "SOMA.Framework.NHDAO"
            //                            }; //Whatever dll name you put here, remember to drop it in the bin.

            //Console.WriteLine("Commencing....!");
            //ISession fromSession;
            //try
            //{
            //    Initialiser.Init(customAssemblies, out fromSession);

            //    var dd = new List<DataEntityDescriptor>();
            //    dd.Add(new DataEntityDescriptor(typeof(AuditTrail.DTO.AuditTrail)));
            //    Runner.MoveDataToCloud(dd, fromSession, "http://localhost/SLMApi/");

            //    fromSession.Close();
            //    fromSession.Dispose();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //    Console.WriteLine();
            //}

            Logging.Logger.Log("Server about to begin flushing {0} items into Table {1} in Database {2}...", 12, 32, 45);

            //string type = typeof(List<Function>).FullName;
            //Console.WriteLine("List Type: {0}\n", type);
            //Type theType = Type.GetType(type);
            //Console.WriteLine("List Type reconstructed: {0}\n", theType.FullName);

            //type = typeof(Function).AssemblyQualifiedName;
            //Console.WriteLine("Type: {0}\n", type);
            //theType = Type.GetType(type);
            //Console.WriteLine("Type reconstructed: {0}\n", theType.FullName);

            //type = string.Format("System.Collections.Generic.List`1[[{0}]]", type);
            //Console.WriteLine("Type: {0}\n", type);
            //theType = Type.GetType(type);
            //Console.WriteLine("Type reconstructed: {0}\n", theType.FullName);
            //Console.WriteLine("Type reconstructed: {0}\n", theType.AssemblyQualifiedName);

            Console.ReadKey();
        }
    }
}
