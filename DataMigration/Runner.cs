using FluentNHibernate.Automapping;
using Newtonsoft.Json;
using NHibernate;
using MultiTenancyFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using MultiTenancyFramework.NHibernate;

namespace DataMigration
{
    /// <summary>
    /// I use this to move data from one DB to another; PARTICULARLY from public shared server where I have the public connectionstring
    /// to my own system so I can hold a backup of the live data
    /// </summary>
    public class Runner
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="migrator"></param>
        /// <param name="customMappingAssemblies">Other Mapping DLLs not included in the NH config section. Make sure these DLLs are in the BIN of the (Console) App</param>
        /// <param name="autoPersistenceModel"></param>
        public static void Run(IMigrator migrator, string[] customMappingAssemblies = null, AutoPersistenceModel autoPersistenceModel = null)
        {
            Console.WriteLine("Commencing Migration!");
            ISession fromSession, toSession;
            try
            {
                Initialiser.Init(customMappingAssemblies, out fromSession, out toSession, autoPersistenceModel);

                migrator.Migrate(fromSession, toSession);
                fromSession.Close();
                fromSession.Dispose();
                toSession.Close();
                toSession.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine();
            }
            Console.WriteLine("Done Migration!");
        }

        public static void MoveData<T>(ISession fromSession, ISession toSession, string tableName = null, string entityName = null) where T : class, IEntity
        {
            var typeNameInPlural = typeof(T).Name.ToPlural();
            IList<T> functions;
            if (!string.IsNullOrWhiteSpace(entityName))
            {
                if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentNullException("tableName", "This should not be null now");
                typeNameInPlural = entityName;
                Console.WriteLine("Migrating {0}...", typeNameInPlural);
                functions = fromSession.QueryOver<T>(entityName).List();
            }
            else
            {
                Console.WriteLine("Migrating {0}...", typeNameInPlural);
                if (string.IsNullOrWhiteSpace(tableName)) tableName = typeNameInPlural;
                functions = fromSession.QueryOver<T>().List();
            }

            if (functions != null && functions.Count > 0)
            {

                Console.WriteLine("{0} retrieved. About to flush them into the new database...", typeNameInPlural);
                SqlManipulations.SqlBulkInsert<T>(functions, toSession.Connection, null, tableName, entityName, true);
                Console.WriteLine("Done flushing {0} into the new database...", typeNameInPlural);
            }
            else
            {
                Console.WriteLine("No data found for {0}", typeNameInPlural);
            }
        }

        /// <summary>
        /// Use this when you cannot access the database remotely. It works by POSTing the data to the cloud directly
        /// where an API is there to receive it.
        /// </summary>
        /// <param name="dataDescriptors"></param>
        /// <param name="baseUrlToPOSTDataTo"></param>
        /// <param name="customAssemblies"></param>
        /// <param name="chunkSize"></param>
        public static void MoveDataToCloud(IList<DataEntityDescriptor> dataDescriptors, string baseUrlToPOSTDataTo, string[] customAssemblies, string nhConfigFileName = null, AutoPersistenceModel autoPersistenceModel = null, int chunkSize = 2000)
        {
            ISession fromSession;
            try
            {
                Initialiser.Init(customAssemblies, out fromSession, nhConfigFileName, autoPersistenceModel);

                MoveDataToCloud(dataDescriptors, fromSession, baseUrlToPOSTDataTo);

                fromSession.Close();
                fromSession.Dispose();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(ex);
                Console.WriteLine();
                Console.WriteLine("Processing stopped");
            }
        }

        /// <summary>
        /// Use this when you cannot access the database remotely. It works by POSTing the data to the cloud directly
        /// where an API is there to receive it.
        /// </summary>
        /// <param name="dataDescriptors"></param>
        /// <param name="fromSession"></param>
        /// <param name="baseUrlToPOSTDataTo"></param>
        /// <param name="chunkSize"></param>
        public static void MoveDataToCloud(IList<DataEntityDescriptor> dataDescriptors, ISession fromSession, string baseUrlToPOSTDataTo, int chunkSize = 2000)
        {
            foreach (var dataDesc in dataDescriptors)
            {
                Console.WriteLine("[{0}]: Begin Processing...", dataDesc.TableName);
                IList dataRecords;
                if (string.IsNullOrWhiteSpace(dataDesc.EntityName))
                {
                    dataRecords = fromSession.CreateCriteria(dataDesc.DataType).List();
                }
                else
                {
                    dataRecords = fromSession.CreateCriteria(dataDesc.EntityName).List();
                }
                Console.WriteLine("[{0}]: Data retrieved. Count = {1}...", dataDesc.TableName, dataRecords.Count);
                var dataChunks = dataRecords.Chunks(chunkSize);
                List<Task> tasks = new List<Task>();
                int count = 0;
                foreach (var chunk in dataChunks)
                {
                    System.Threading.Thread.Sleep(15000);
                    count++;
                    Console.WriteLine("\t[{0}]: Serializing data chunk {1}...", dataDesc.TableName, count);
                    var chunkToList = chunk.Cast<object>().ToList();
                    string json = JsonConvert.SerializeObject(chunkToList, dataDesc.DataType, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });
                    Console.WriteLine("\t[{0}]: Done Serializing.\tEncoding serialized data chunk {1}...", dataDesc.TableName, count);
                    string dataToSend = WebUtility.HtmlEncode(json.CompressString());
                    string dataType = WebUtility.HtmlEncode(JsonConvert.SerializeObject(dataDesc).CompressString());
                    Console.WriteLine("\t[{0}]: Done Encoding.\n", dataDesc.TableName);

                    //ProcessRequest(dataToSend, dataType);
                    tasks.Add(Task.Factory.StartNew((xCount) =>
                    {
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(baseUrlToPOSTDataTo);
                            client.DefaultRequestHeaders.Accept.Clear();
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
                            client.DefaultRequestHeaders.Add("datatype", dataType);

                            client.Timeout = new TimeSpan(1, 0, 0);
                            Console.WriteLine("\t[{0}]: - Chunk {1}] being POSTed to our URL", dataDesc.TableName, xCount);
                            Stopwatch sw = new Stopwatch();
                            sw.Start();
                            HttpResponseMessage response = client.PostAsync("BankIswSlmIntg/KeepData", new StringContent(dataToSend)).Result;
                            sw.Stop();
                            Console.WriteLine("\t[{0}]: - Chunk {1}] It took {2} ms to return from server", dataDesc.TableName, xCount, sw.ElapsedMilliseconds.ToNaira());

                            string responseContent = string.Format("\t[{0}]: - Chunk {1}] Back from server\t", dataDesc.TableName, xCount);
                            if (response.IsSuccessStatusCode)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                responseContent += response.Content.ReadAsStringAsync().Result;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                responseContent += string.Format("Response Status {0} - {1}", response.StatusCode, response.ReasonPhrase);
                            }
                            Console.WriteLine(responseContent);
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }, count));
                }
                Task.WaitAll(tasks.ToArray());

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[{0}]: Process completed\n\n", dataDesc.TableName);
            }
        }


        static string ProcessRequest(string receivedData, string dataType)
        {
            if (string.IsNullOrWhiteSpace(receivedData) || string.IsNullOrWhiteSpace(dataType)) throw new ArgumentException("We did not receive the right set of data.");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var jsonback = WebUtility.HtmlDecode(receivedData).DecompressString();
            var dataTypeBack = WebUtility.HtmlDecode(dataType).DecompressString();
            var dataDesc = JsonConvert.DeserializeObject<DataEntityDescriptor>(dataTypeBack);
            var list = JsonConvert.DeserializeObject(jsonback, dataDesc.DataTypeInListForm) as IList;
            sw.Stop();

            long time = sw.ElapsedMilliseconds;
            sw.Restart();
            using (var fromSession = SOMA.Framework.NHDAO.NHibernateManager.NHibernateSessionManager.GetSession())
            {
                Logger.Log("Server about to begin flushing {0} items into Table {1} in Database {2}...", list.Count, dataDesc.TableName, fromSession.Connection.Database);
                SOMA.Framework.DAO.SqlManipulations.SqlBulkInsert(dataDesc.DataType, list, fromSession.Connection, null, dataDesc.TableName, dataDesc.EntityName, true);
                Logger.Log("Server done flushing {0} items into Table {1} in Database {2}...", list.Count, dataDesc.TableName, fromSession.Connection.Database);
            }
            sw.Stop();
            return string.Format("Flushed! It took {0} ms altogether: {1} for unbundling and {2} for flushing.", (sw.ElapsedMilliseconds + time).ToNaira(), time.ToNaira(), sw.ElapsedMilliseconds.ToNaira());
        }
    }
}
