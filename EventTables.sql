-- ===============================================
--  BANCO DE EVENTOS - SCRIPT COMPLETO CORRIGIDO
--  Compatível com MySQL 5.6.23
-- ===============================================

SET FOREIGN_KEY_CHECKS = 0;

-- ===============================================
--  Tabela characters (necessária para FK)
-- ===============================================
CREATE TABLE IF NOT EXISTS `characters` (
  `UID` INT(8) UNSIGNED NOT NULL,
  `Name` VARCHAR(64) NOT NULL,
  PRIMARY KEY (`UID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ===============================================
--  Tabela event_config
-- ===============================================
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

-- ===============================================
--  Tabela event_entry
-- ===============================================
CREATE TABLE IF NOT EXISTS `event_entry` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `event_config_id` INT(10) UNSIGNED NOT NULL,
  `character_id` INT(10) UNSIGNED NOT NULL,
  `entry_type` ENUM('SOLO','TEAM') NOT NULL DEFAULT 'SOLO',
  `state` ENUM('PENDING','APPROVED','REJECTED','COMPLETED') NOT NULL DEFAULT 'PENDING',
  `signed_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `mini_objective_tickets` SMALLINT(5) UNSIGNED NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`),

  INDEX `IDX_event_entry_config` (`event_config_id`),
  INDEX `IDX_event_entry_character` (`character_id`),

  CONSTRAINT `FK_event_entry_config`
    FOREIGN KEY (`event_config_id`)
    REFERENCES `event_config`(`id`)
    ON DELETE CASCADE,

  CONSTRAINT `FK_event_entry_character`
    FOREIGN KEY (`character_id`)
    REFERENCES `characters`(`UID`)
    ON DELETE CASCADE,

  UNIQUE KEY `UQ_event_entry_event_character` (`event_config_id`,`character_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ===============================================
--  Tabela event_reward
-- ===============================================
CREATE TABLE IF NOT EXISTS `event_reward` (
  `id` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `event_entry_id` INT(10) UNSIGNED NOT NULL,
  `reward_type` ENUM('ITEM','CURRENCY','EXPERIENCE') NOT NULL DEFAULT 'ITEM',
  `reward_value` INT(10) UNSIGNED NOT NULL DEFAULT 0,
  `granted_at` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `delivered` TINYINT(1) NOT NULL DEFAULT 0,
  `delivered_at` DATETIME NULL,
  PRIMARY KEY (`id`),

  INDEX `IDX_event_reward_entry` (`event_entry_id`),

  CONSTRAINT `FK_event_reward_entry`
    FOREIGN KEY (`event_entry_id`)
    REFERENCES `event_entry`(`id`)
    ON DELETE CASCADE,

  UNIQUE KEY `UQ_event_reward_entry_type`
    (`event_entry_id`,`reward_type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

SET FOREIGN_KEY_CHECKS = 1;

-- ===============================================
--  FIM DO SCRIPT
-- ===============================================
