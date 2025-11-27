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

ALTER TABLE `event_config` ADD COLUMN IF NOT EXISTS `max_tickets_per_player` SMALLINT(5) UNSIGNED NOT NULL DEFAULT 0 AFTER `max_signups`;
ALTER TABLE `event_config` ADD COLUMN IF NOT EXISTS `winners_count` TINYINT(3) UNSIGNED NOT NULL DEFAULT 1 AFTER `max_tickets_per_player`;
ALTER TABLE `event_config` ADD COLUMN IF NOT EXISTS `reward_type` ENUM('ITEM','CURRENCY','EXPERIENCE') NOT NULL DEFAULT 'ITEM' AFTER `winners_count`;
ALTER TABLE `event_config` ADD COLUMN IF NOT EXISTS `reward_value` INT(10) UNSIGNED NOT NULL DEFAULT 0 AFTER `reward_type`;
ALTER TABLE `event_entry` ADD COLUMN IF NOT EXISTS `mini_objective_tickets` SMALLINT(5) UNSIGNED NOT NULL DEFAULT 0 AFTER `signed_at`;
ALTER TABLE `event_reward` ADD COLUMN IF NOT EXISTS `delivered` TINYINT(1) NOT NULL DEFAULT 0 AFTER `granted_at`;
ALTER TABLE `event_reward` ADD COLUMN IF NOT EXISTS `delivered_at` DATETIME NULL AFTER `delivered`;
