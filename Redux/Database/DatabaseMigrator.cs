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
        private const string EventScriptDirectory = "Scripts";
        private const string EventScriptName = "EventTables.sql";

        public static void EnsureEventTables()
        {
            EnsureCharactersKey();

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

                EnsureEventColumns(session);

                transaction.Commit();
            }
        }

        private static void EnsureCharactersKey()
        {
            using (var session = NHibernateHelper.OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var columnType = session.CreateSQLQuery(
                        "SELECT LOWER(COLUMN_TYPE) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'characters' AND COLUMN_NAME = 'UID'")
                    .UniqueResult<string>();

                if (!string.Equals(columnType, "int(10) unsigned", StringComparison.Ordinal))
                {
                    session.CreateSQLQuery("ALTER TABLE `characters` MODIFY `UID` INT(10) UNSIGNED NOT NULL").ExecuteUpdate();
                }

                var indexExists = Convert.ToInt64(session.CreateSQLQuery(
                        "SELECT COUNT(1) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'characters' AND INDEX_NAME = 'UQ_characters_UID'")
                    .UniqueResult<object>()) > 0;

                if (!indexExists)
                {
                    session.CreateSQLQuery("ALTER TABLE `characters` ADD UNIQUE INDEX `UQ_characters_UID` (`UID`)").ExecuteUpdate();
                }

                transaction.Commit();
            }
        }

        private static void EnsureEventColumns(NHibernate.ISession session)
        {
            EnsureColumn(session, "event_config", "max_tickets_per_player",
                "ALTER TABLE `event_config` ADD COLUMN `max_tickets_per_player` SMALLINT(5) UNSIGNED NOT NULL DEFAULT 0 AFTER `max_signups`");
            EnsureColumn(session, "event_config", "winners_count",
                "ALTER TABLE `event_config` ADD COLUMN `winners_count` TINYINT(3) UNSIGNED NOT NULL DEFAULT 1 AFTER `max_tickets_per_player`");
            EnsureColumn(session, "event_config", "reward_type",
                "ALTER TABLE `event_config` ADD COLUMN `reward_type` ENUM('ITEM','CURRENCY','EXPERIENCE') NOT NULL DEFAULT 'ITEM' AFTER `winners_count`");
            EnsureColumn(session, "event_config", "reward_value",
                "ALTER TABLE `event_config` ADD COLUMN `reward_value` INT(10) UNSIGNED NOT NULL DEFAULT 0 AFTER `reward_type`");
            EnsureColumn(session, "event_entry", "mini_objective_tickets",
                "ALTER TABLE `event_entry` ADD COLUMN `mini_objective_tickets` SMALLINT(5) UNSIGNED NOT NULL DEFAULT 0 AFTER `signed_at`");
            EnsureColumn(session, "event_reward", "delivered",
                "ALTER TABLE `event_reward` ADD COLUMN `delivered` TINYINT(1) NOT NULL DEFAULT 0 AFTER `granted_at`");
            EnsureColumn(session, "event_reward", "delivered_at",
                "ALTER TABLE `event_reward` ADD COLUMN `delivered_at` DATETIME NULL AFTER `delivered`");
        }

        private static void EnsureColumn(NHibernate.ISession session, string table, string column, string alterSql)
        {
            var exists = Convert.ToInt64(session.CreateSQLQuery(
                    "SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = :tableName AND COLUMN_NAME = :columnName")
                .SetParameter("tableName", table)
                .SetParameter("columnName", column)
                .UniqueResult<object>()) > 0;

            if (!exists)
            {
                session.CreateSQLQuery(alterSql).ExecuteUpdate();
            }
        }

        private static string ResolveScriptPath()
        {
            try
            {
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var candidate = Path.Combine(baseDirectory, ScriptsFolder, EventScriptDirectory, EventScriptName);
                if (File.Exists(candidate))
                    return candidate;

                var fallback = Path.Combine(baseDirectory, EventScriptDirectory, EventScriptName);
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
