﻿using System;
using Autofac;
using ChickenAPI.Core.i18n;
using ChickenAPI.Core.IoC;
using ChickenAPI.Core.Maths;
using ChickenAPI.Data.Character;
using ChickenAPI.Data.Item;
using ChickenAPI.Data.Skills;
using ChickenAPI.Enums.Game.Character;
using ChickenAPI.Enums.Game.Entity;
using ChickenAPI.Enums.Game.Items;
using ChickenAPI.Enums.Packets;
using ChickenAPI.Game.Effects;
using ChickenAPI.Game.Entities.Player;
using ChickenAPI.Game.Entities.Player.Extensions;
using ChickenAPI.Game.Families.Extensions;
using ChickenAPI.Game.Helpers;
using ChickenAPI.Game.Inventory.Extensions;
using ChickenAPI.Game.PacketHandling.Extensions;
using ChickenAPI.Game.Shops.Extensions;
using ChickenAPI.Game.Skills;
using ChickenAPI.Packets.Game.Client.Player;
using ChickenAPI.Packets.Game.Server.Player;
using ChickenAPI.Packets.Game.Server.UserInterface;

namespace ChickenAPI.Game.Player.Extension
{
    public static class PlayerExtension
    {
        private static readonly IRandomGenerator _randomGenerator = new Lazy<IRandomGenerator>(() => ChickenContainer.Instance.Resolve<IRandomGenerator>()).Value;
        private static readonly IAlgorithmService Algorithm = new Lazy<IAlgorithmService>(() => ChickenContainer.Instance.Resolve<IAlgorithmService>()).Value;

        public static TcInfoPacket GenerateReqInfo(this IPlayerEntity charac)
        {
            ItemInstanceDto fairy = null;
            ItemInstanceDto armor = null;
            ItemInstanceDto weapon2 = null;
            ItemInstanceDto weapon = null;
            if (charac.Inventory != null)
            {
                fairy = charac.Inventory.GetWeared(EquipmentType.Fairy);
                armor = charac.Inventory.GetWeared(EquipmentType.Armor);
                weapon2 = charac.Inventory.GetWeared(EquipmentType.SecondaryWeapon);
                weapon = charac.Inventory.GetWeared(EquipmentType.MainWeapon);
            }

            bool isPvpPrimary = false;
            bool isPvpSecondary = false;
            bool isPvpArmor = false;

            if (weapon?.Item.Name.Contains(": ") == true)
            {
                isPvpPrimary = true;
            }

            isPvpSecondary |= weapon2?.Item.Name.Contains(": ") == true;
            isPvpArmor |= armor?.Item.Name.Contains(": ") == true;

            // tc_info 0 name 0 0 0 0 -1 - 0 0 0 0 0 0 0 0 0 0 0 wins deaths reput 0 0 0 morph
            // talentwin talentlose capitul rankingpoints arenapoints 0 0 ispvpprimary ispvpsecondary
            // ispvparmor herolvl desc
            return new TcInfoPacket
            {
                Level = charac.Level,
                Name = charac.Character.Name,
                Element = fairy?.ElementType ?? ElementType.Neutral,
                ElementRate = fairy?.ElementRate ?? 0,
                Class = charac.Character.Class,
                Gender = charac.Character.Gender,
                Family = charac.HasFamily ? charac.Family.Name : "-1 -",
                ReputationIco = charac.GetReputIcon(),
                DignityIco = charac.GetDignityIcon(),
                HaveWeapon = weapon != null ? 1 : 0,
                WeaponRare = weapon?.Rarity ?? 0,
                WeaponUpgrade = weapon?.Upgrade ?? 0,
                HaveSecondary = weapon2 != null ? 1 : 0,
                SecondaryRare = weapon2?.Rarity ?? 0,
                SecondaryUpgrade = weapon?.Upgrade ?? 0,
                HaveArmor = armor != null ? 1 : 0,
                ArmorRare = armor?.Rarity ?? 0,
                ArmorUpgrade = armor?.Upgrade ?? 0,
                Act4Kill = charac.Character.Act4Kill,
                Act4Dead = charac.Character.Act4Dead,
                Reputation = charac.Character.Reput,
                Unknow = 0,
                Unknow2 = 0,
                Unknow3 = 0,
                Unknow4 = 0,
                Morph = charac.MorphId,
                TalentLose = charac.Character.TalentLose,
                TalentSurrender = charac.Character.TalentSurrender,
                TalentWin = charac.Character.TalentWin,
                MasterPoints = charac.Character.MasterPoints,
                Compliments = charac.Character.Compliment,
                Act4Points = charac.Character.Act4Points,
                isPvpArmor = isPvpArmor,
                isPvpPrimary = isPvpPrimary,
                isPvpSecondary = isPvpSecondary,
                HeroLevel = charac.HeroLevel,
                Biography = string.IsNullOrEmpty(charac.Character.Biography) ? " REKT BOY" : charac.Character.Biography
            };
        }

        public static void ChangeClass(this IPlayerEntity charac, CharacterClassType type)
        {
            charac.JobLevel = 1;
            charac.JobLevelXp = 0;
            charac.SendPacket(new NpInfoPacket { UnKnow = 0 });
            charac.SendPacket(new PClearPacket());

            if (type == CharacterClassType.Adventurer)
            {
                charac.Character.HairStyle = charac.Character.HairStyle > HairStyleType.HairStyleB ? HairStyleType.HairStyleA : charac.Character.HairStyle;
            }

            charac.Character.Class = type;
            charac.HpMax = Algorithm.GetHpMax(type, charac.Level);
            charac.MpMax = Algorithm.GetMpMax(type, charac.Level);
            charac.Hp = charac.HpMax;
            charac.Mp = charac.MpMax;
            charac.SendPacket(new TitPacket
            {
                ClassType = type == CharacterClassType.Adventurer ? "Adventurer" :
                    type == CharacterClassType.Archer ? "Archer" :
                    type == CharacterClassType.Magician ? "Mage" :
                    type == CharacterClassType.Swordman ? "Swordman" : "Martial",
                Name = charac.Character.Name
            });
            charac.ActualizeUiHpBar();
            charac.SendPacket(charac.GenerateEqPacket());
            charac.SendPacket(charac.GenerateEffectPacket(8));
            charac.SendPacket(charac.GenerateEffectPacket(196));
            charac.SendPacket(new ScrPacket { Unknow1 = 0, Unknow2 = 0, Unknow3 = 0, Unknow4 = 0, Unknow5 = 0, Unknow6 = 0 });
            charac.SendChatMessageFormat(ChickenI18NKey.CHARACTER_YOUR_CLASS_CHANGED_TO_X, SayColorType.Blue, type);
            charac.Character.Faction = charac.Family?.FamilyFaction ?? (FactionType)(1 + _randomGenerator.Next(0, 2));
            charac.SendChatMessageFormat(ChickenI18NKey.CHARACTER_YOUR_FACTION_CHANGED_TO_X, SayColorType.Blue, charac.Character.Faction);
            charac.SendPacket(charac.GenerateFsPacket());
            charac.SendPacket(charac.GenerateStatCharPacket());
            charac.SendPacket(charac.GenerateEffectPacket(4799 + (byte)charac.Character.Faction));
            charac.ActualizePlayerCondition();
            charac.ActualizeUiExpBar();
            charac.Broadcast(charac.GenerateCModePacket());
            charac.Broadcast(charac.GenerateInPacket());
            charac.Broadcast(charac.GenerateGidxPacket());
            charac.Broadcast(charac.GenerateEffectPacket(6));
            charac.Broadcast(charac.GenerateEffectPacket(196));

            SkillComponent component = charac.SkillComponent;
            foreach (SkillDto skill in component.Skills.Values)
            {
                if (skill.Id >= 200)
                {
                    component.Skills.Remove(skill.Id);
                }
            }

            // TODO : LATER ADD SKILL
            charac.ActualizeUiSkillList();

            // Later too
            /* foreach (QuicklistEntryDTO quicklists in DAOFactory.QuicklistEntryDAO.LoadByCharacterId(CharacterId).Where(quicklists => QuicklistEntries.Any(qle => qle.Id == quicklists.Id)))
             {
                 DAOFactory.QuicklistEntryDAO.Delete(quicklists.Id);
             }

             QuicklistEntries = new List<QuicklistEntryDTO>
             {
                 new QuicklistEntryDTO
                 {
                     CharacterId = CharacterId,
                     Q1 = 0,
                     Q2 = 9,
                     Type = 1,
                     Slot = 3,
                     Pos = 1
                 }
             };
             if (ServerManager.Instance.Groups.Any(s => s.IsMemberOfGroup(Session) && s.GroupType == GroupType.Group))
             {
                 Session.CurrentMapInstance?.Broadcast(Session, $"pidx 1 1.{CharacterId}", ReceiverType.AllExceptMe);
             }*/
        }

        public static void NotifyRarifyResult(this IPlayerEntity player, sbyte rare)
        {
            player.SendPacket(player.GenerateMsgPacket("RARIFY_SUCCESS", MsgPacketType.Whisper));
            player.SendPacket(player.GenerateSayPacket("RARIFY_SUCCESS", SayColorType.Green));
            player.SendPacket(player.GenerateSayPacket($"{rare}", SayColorType.Green));
            player.Broadcast(player.GenerateEffectPacket(3005));
            player.SendPacket(player.GenerateShopEndPacket(ShopEndPacketType.CloseSubWindow));
        }

        #region Gold

        public static void GoldLess(this IPlayerEntity player, long amount)
        {
            player.Character.Gold -= amount;
            player.ActualizeUiGold();
        }

        public static void GoldUp(this IPlayerEntity player, long amount)
        {
            player.Character.Gold += amount;
            player.ActualizeUiGold();
        }

        #endregion Gold
    }
}