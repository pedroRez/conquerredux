using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Redux.Game_Server;
using Redux.Structures;

namespace Redux.Events
{
    public class EventNormalizationProfile
    {
        public ushort Strength { get; set; } = 100;
        public ushort Agility { get; set; } = 100;
        public ushort Spirit { get; set; } = 100;
        public ushort Vitality { get; set; } = 100;

        public ushort? FixedMinimumDamage { get; set; }
        public ushort? FixedMaximumDamage { get; set; }
        public ushort? FixedDefense { get; set; }
        public ushort? FixedMagicResistance { get; set; }
        public ushort? FixedMagicDamage { get; set; }
        public ushort? FixedMaxLife { get; set; }
        public ushort? FixedMaxMana { get; set; }
        public byte? FixedAttackRange { get; set; }

        public static EventNormalizationProfile BalancedDefaults => new EventNormalizationProfile
        {
            Strength = 150,
            Agility = 150,
            Spirit = 150,
            Vitality = 150,
            FixedMinimumDamage = 250,
            FixedMaximumDamage = 350,
            FixedDefense = 100,
            FixedMagicResistance = 100,
            FixedMaxLife = 3000,
            FixedMaxMana = 1500,
            FixedAttackRange = 3
        };
    }

    public class EventNormalizationSnapshot
    {
        public ushort Strength { get; set; }
        public ushort Agility { get; set; }
        public ushort Spirit { get; set; }
        public ushort Vitality { get; set; }
        public ushort ExtraStats { get; set; }
        public IList<EventNormalizationSnapshot.EquippedItemSlot> EquippedItems { get; } = new List<EquippedItemSlot>();

        public class EquippedItemSlot
        {
            public byte Slot { get; set; }
            public uint ItemUid { get; set; }
        }
    }

    public class EventNormalizationState
    {
        public EventNormalizationProfile Profile { get; set; }
        public EventNormalizationSnapshot Snapshot { get; set; }
        public bool IsActive => Profile != null && Snapshot != null;

        public void Clear()
        {
            Profile = null;
            Snapshot = null;
        }
    }

    public static class EventNormalizationManager
    {
        private static readonly ConcurrentDictionary<uint, EventNormalizationProfile> _profiles = new ConcurrentDictionary<uint, EventNormalizationProfile>();
        private static readonly uint[] _defaultEventMaps = { 7000u, 7001u };

        static EventNormalizationManager()
        {
            var defaultProfile = EventNormalizationProfile.BalancedDefaults;
            foreach (var mapId in _defaultEventMaps)
                RegisterProfile(mapId, defaultProfile);
        }

        public static void RegisterProfile(uint mapId, EventNormalizationProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            _profiles.AddOrUpdate(mapId, profile, (k, v) => profile);
        }

        public static void OnMapChanged(Player player, uint? previousMapId, uint newMapId)
        {
            if (player == null)
                return;

            if (player.EventNormalization.IsActive && (!IsEventMap(newMapId) || !_profiles.TryGetValue(newMapId, out var newProfile) || player.EventNormalization.Profile != newProfile))
                Restore(player);

            if (_profiles.TryGetValue(newMapId, out var profile))
                Apply(player, profile);
        }

        public static void ApplyCombatOverrides(Player player)
        {
            var profile = player?.EventNormalization?.Profile;
            if (profile == null)
                return;

            if (profile.FixedAttackRange.HasValue)
                player.CombatStats.AttackRange = profile.FixedAttackRange.Value;
            if (profile.FixedMinimumDamage.HasValue)
                player.CombatStats.MinimumDamage = profile.FixedMinimumDamage.Value;
            if (profile.FixedMaximumDamage.HasValue)
                player.CombatStats.MaximumDamage = profile.FixedMaximumDamage.Value;
            if (profile.FixedDefense.HasValue)
                player.CombatStats.Defense = profile.FixedDefense.Value;
            if (profile.FixedMagicResistance.HasValue)
                player.CombatStats.MagicResistance = profile.FixedMagicResistance.Value;
            if (profile.FixedMagicDamage.HasValue)
                player.CombatStats.MagicDamage = profile.FixedMagicDamage.Value;
            if (profile.FixedMaxLife.HasValue)
                player.CombatStats.MaxLife = profile.FixedMaxLife.Value;
            if (profile.FixedMaxMana.HasValue)
                player.CombatStats.MaxMana = profile.FixedMaxMana.Value;
        }

        private static bool IsEventMap(uint mapId)
        {
            return _profiles.ContainsKey(mapId);
        }

        private static void Apply(Player player, EventNormalizationProfile profile)
        {
            if (player.EventNormalization.IsActive)
                return;

            var snapshot = new EventNormalizationSnapshot
            {
                Strength = player.Strength,
                Agility = player.Agility,
                Spirit = player.Spirit,
                Vitality = player.Vitality,
                ExtraStats = player.ExtraStats
            };

            for (byte slot = 1; slot < 10; slot++)
            {
                ConquerItem equipped;
                if (player.Equipment.TryGetItemBySlot(slot, out equipped))
                {
                    snapshot.EquippedItems.Add(new EventNormalizationSnapshot.EquippedItemSlot { Slot = slot, ItemUid = equipped.UniqueID });
                    player.Equipment.UnequipItem(slot);
                }
            }

            player.EventNormalization.Profile = profile;
            player.EventNormalization.Snapshot = snapshot;

            player.Strength = profile.Strength;
            player.Agility = profile.Agility;
            player.Spirit = profile.Spirit;
            player.Vitality = profile.Vitality;
            player.ExtraStats = 0;

            player.Recalculate(true);
            player.SendSysMessage("Você entrou no mapa do evento. Seus atributos e equipamentos foram normalizados.");
        }

        private static void Restore(Player player)
        {
            var snapshot = player.EventNormalization.Snapshot;
            if (snapshot == null)
                return;

            player.Strength = snapshot.Strength;
            player.Agility = snapshot.Agility;
            player.Spirit = snapshot.Spirit;
            player.Vitality = snapshot.Vitality;
            player.ExtraStats = snapshot.ExtraStats;

            foreach (var equipped in snapshot.EquippedItems)
            {
                ConquerItem item;
                if (player.Inventory.TryGetValue(equipped.ItemUid, out item) && player.Equipment.EquipItem(item, equipped.Slot))
                    player.RemoveItem(item, false);
            }

            player.EventNormalization.Clear();
            player.Recalculate(true);
            player.SendSysMessage("Você saiu do mapa do evento. Seus atributos originais foram restaurados.");
        }
    }
}
