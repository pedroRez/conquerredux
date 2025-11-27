-- Normalize characters UID so foreign keys work on MySQL 5.6.x
SET @char_uid_type := (
  SELECT LOWER(COLUMN_TYPE)
  FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'characters' AND COLUMN_NAME = 'UID'
);
SET @char_uid_sql := IF(
  @char_uid_type IS NULL OR @char_uid_type NOT LIKE 'int(10) unsigned',
  'ALTER TABLE `characters` MODIFY `UID` INT(10) UNSIGNED NOT NULL',
  'SELECT 1'
);
PREPARE stmt FROM @char_uid_sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @char_uid_idx := (
  SELECT COUNT(1)
  FROM INFORMATION_SCHEMA.STATISTICS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'characters' AND INDEX_NAME = 'UQ_characters_UID'
);
SET @char_uid_idx_sql := IF(
  @char_uid_idx = 0,
  'ALTER TABLE `characters` ADD UNIQUE INDEX `UQ_characters_UID` (`UID`)',
  'SELECT 1'
);
PREPARE stmt FROM @char_uid_idx_sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

CREATE TABLE IF NOT EXISTS `event_config` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `event_code` ENUM('AUTOMATED','SCHEDULED','MANUAL') NOT NULL DEFAULT 'AUTOMATED',
  `title` VARCHAR(64) NOT NULL,
  `max_signups` TINYINT(3) UNSIGNED NOT NULL DEFAULT 1,
  `max_tickets_per_player` SMALLINT(5) UNSIGNED NOT NULL DEFAULT 0,
  `winners_count` TINYINT(3) UNSIGNED NOT NULL DEFAULT 1,
  `reward_type` ENUM('ITEM','CURRENCY','EXPERIENCE') NOT NULL DEFAULT 'ITEM',
  `reward_value` INT(10) UNSIGNED NOT NULL DEFAULT 0,
  `starts_at` DATETIME NOT NULL,
  `ends_at` DATETIME NOT NULL,
  `created_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `status` ENUM('DRAFT','ACTIVE','INACTIVE') NOT NULL DEFAULT 'ACTIVE',
  PRIMARY KEY (`id`),
  UNIQUE KEY `UQ_event_config_code` (`event_code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `event_entry` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `event_config_id` INT(10) UNSIGNED NOT NULL,
  `character_id` INT(10) UNSIGNED NOT NULL,
  `entry_type` ENUM('SOLO','TEAM') NOT NULL DEFAULT 'SOLO',
  `state` ENUM('PENDING','APPROVED','REJECTED','COMPLETED') NOT NULL DEFAULT 'PENDING',
  `signed_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `mini_objective_tickets` SMALLINT(5) UNSIGNED NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),
  CONSTRAINT `FK_event_entry_config` FOREIGN KEY (`event_config_id`) REFERENCES `event_config`(`id`) ON DELETE CASCADE,
  CONSTRAINT `FK_event_entry_character` FOREIGN KEY (`character_id`) REFERENCES `characters`(`UID`) ON DELETE CASCADE,
  UNIQUE KEY `UQ_event_entry_event_character` (`event_config_id`,`character_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS `event_reward` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `event_entry_id` INT(10) UNSIGNED NOT NULL,
  `reward_type` ENUM('ITEM','CURRENCY','EXPERIENCE') NOT NULL DEFAULT 'ITEM',
  `reward_value` INT(10) UNSIGNED NOT NULL DEFAULT 0,
  `granted_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `delivered` TINYINT(1) NOT NULL DEFAULT 0,
  `delivered_at` DATETIME NULL,
  PRIMARY KEY (`id`),
  CONSTRAINT `FK_event_reward_entry` FOREIGN KEY (`event_entry_id`) REFERENCES `event_entry`(`id`) ON DELETE CASCADE,
  UNIQUE KEY `UQ_event_reward_entry_type` (`event_entry_id`,`reward_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- MySQL 5.6 lacks "ADD COLUMN IF NOT EXISTS", so use dynamic checks
SET @cfg_tickets := (
  SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'event_config' AND COLUMN_NAME = 'max_tickets_per_player'
);
SET @cfg_tickets_sql := IF(
  @cfg_tickets = 0,
  'ALTER TABLE `event_config` ADD COLUMN `max_tickets_per_player` SMALLINT(5) UNSIGNED NOT NULL DEFAULT 0 AFTER `max_signups`',
  'SELECT 1'
);
PREPARE stmt FROM @cfg_tickets_sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @cfg_winners := (
  SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'event_config' AND COLUMN_NAME = 'winners_count'
);
SET @cfg_winners_sql := IF(
  @cfg_winners = 0,
  'ALTER TABLE `event_config` ADD COLUMN `winners_count` TINYINT(3) UNSIGNED NOT NULL DEFAULT 1 AFTER `max_tickets_per_player`',
  'SELECT 1'
);
PREPARE stmt FROM @cfg_winners_sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @cfg_reward_type := (
  SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'event_config' AND COLUMN_NAME = 'reward_type'
);
SET @cfg_reward_type_sql := IF(
  @cfg_reward_type = 0,
  'ALTER TABLE `event_config` ADD COLUMN `reward_type` ENUM(''ITEM'',''CURRENCY'',''EXPERIENCE'') NOT NULL DEFAULT ''ITEM'' AFTER `winners_count`',
  'SELECT 1'
);
PREPARE stmt FROM @cfg_reward_type_sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @cfg_reward_value := (
  SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'event_config' AND COLUMN_NAME = 'reward_value'
);
SET @cfg_reward_value_sql := IF(
  @cfg_reward_value = 0,
  'ALTER TABLE `event_config` ADD COLUMN `reward_value` INT(10) UNSIGNED NOT NULL DEFAULT 0 AFTER `reward_type`',
  'SELECT 1'
);
PREPARE stmt FROM @cfg_reward_value_sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @entry_tickets := (
  SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'event_entry' AND COLUMN_NAME = 'mini_objective_tickets'
);
SET @entry_tickets_sql := IF(
  @entry_tickets = 0,
  'ALTER TABLE `event_entry` ADD COLUMN `mini_objective_tickets` SMALLINT(5) UNSIGNED NOT NULL DEFAULT 0 AFTER `signed_at`',
  'SELECT 1'
);
PREPARE stmt FROM @entry_tickets_sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @reward_delivered := (
  SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'event_reward' AND COLUMN_NAME = 'delivered'
);
SET @reward_delivered_sql := IF(
  @reward_delivered = 0,
  'ALTER TABLE `event_reward` ADD COLUMN `delivered` TINYINT(1) NOT NULL DEFAULT 0 AFTER `granted_at`',
  'SELECT 1'
);
PREPARE stmt FROM @reward_delivered_sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

SET @reward_delivered_at := (
  SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'event_reward' AND COLUMN_NAME = 'delivered_at'
);
SET @reward_delivered_at_sql := IF(
  @reward_delivered_at = 0,
  'ALTER TABLE `event_reward` ADD COLUMN `delivered_at` DATETIME NULL AFTER `delivered`',
  'SELECT 1'
);
PREPARE stmt FROM @reward_delivered_at_sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
