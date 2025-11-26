using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Redux.Database.Repositories;

namespace Redux.Database
{
    public static class DatabaseMigrator
    {
        private const string ScriptsFolder = "Database";
        private const string EventScriptName = "Scripts" + Path.DirectorySeparatorChar + "EventTables.sql";

        public static void EnsureEventTables()
        {
            var scriptPath = ResolveScriptPath();
            if (string.IsNullOrEmpty(scriptPath) || !File.Exists(scriptPath))
                return;

            var statements = ReadStatements(scriptPath);
            if (statements.Count == 0)
                return;

            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                foreach (var statement in statements)
                {
                    session.CreateSQLQuery(statement).ExecuteUpdate();
                }
                transaction.Commit();
            }
        }

        private static string ResolveScriptPath()
        {
            try
            {
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var candidate = Path.Combine(baseDirectory, ScriptsFolder, EventScriptName);
                if (File.Exists(candidate))
                    return candidate;

                var fallback = Path.Combine(baseDirectory, EventScriptName);
                if (File.Exists(fallback))
                    return fallback;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to resolve migration script path: " + ex.Message);
            }

            return string.Empty;
        }

        private static IList<string> ReadStatements(string scriptPath)
        {
            try
            {
                var content = File.ReadAllText(scriptPath);
                return content
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(sql => sql.Trim())
                    .Where(sql => !string.IsNullOrWhiteSpace(sql))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read migration script: " + ex.Message);
                return new List<string>();
            }
        }
    }
}
