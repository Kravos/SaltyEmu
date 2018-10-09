﻿using System.Collections.Generic;
using Autofac;
using ChickenAPI.Core.IoC;
using ChickenAPI.Enums.Game.Families;
using ChickenAPI.Game.Data.AccessLayer.Character;
using ChickenAPI.Game.Entities.Player;
using ChickenAPI.Game.Families;
using ChickenAPI.Game.Families.Events;
using NUnit.Framework;

namespace ChickenAPI.Game.Test.Families
{
    [TestFixture]
    public class Family_Join
    {
        private IPlayerEntity _leader;
        private ICharacterService _characterService;
        private readonly BasicFamilyEventHandler _familyEventHandler = new BasicFamilyEventHandler();

        [SetUp]
        public void Setup()
        {
            TestHelper.Initialize();
            _characterService = ChickenContainer.Instance.Resolve<ICharacterService>();
            const string familyName = "family_join_test";
            _leader = LoadPlayer("test_leader");

            _familyEventHandler.Execute(_leader, new FamilyCreationEvent
            {
                Leader = _leader,
                FamilyName = familyName
            });
        }

        private static IPlayerEntity LoadPlayer(string name) => TestHelper.LoadPlayer(name);

        [Test]
        public void Family_Join_Success_Existing_Family()
        {
            IPlayerEntity newPlayer = LoadPlayer("test_member");

            _familyEventHandler.Execute(newPlayer, new FamilyJoinEvent
            {
                Family = _leader.Family,
            });

            Assert.AreEqual(_leader.Family, newPlayer.Family);
            Assert.AreEqual(_leader.Family.Id, newPlayer.FamilyCharacter.FamilyId);
            Assert.AreEqual(newPlayer.Character.Id, newPlayer.FamilyCharacter.CharacterId);
            Assert.AreEqual(newPlayer.FamilyCharacter.Authority, FamilyAuthority.Member);
        }

        [Test]
        public void Family_Join_Success_Custom_Role()
        {
            IPlayerEntity newPlayer = LoadPlayer("test_member_custom_role");

            _familyEventHandler.Execute(newPlayer, new FamilyJoinEvent
            {
                Family = _leader.Family,
                ExpectedAuthority = FamilyAuthority.Assistant
            });

            Assert.AreEqual(_leader.Family, newPlayer.Family);
            Assert.AreEqual(newPlayer.FamilyCharacter.FamilyId, newPlayer.FamilyCharacter.FamilyId);
            Assert.AreEqual(newPlayer.Character.Id, newPlayer.FamilyCharacter.CharacterId);
            Assert.AreEqual(newPlayer.FamilyCharacter.Authority, FamilyAuthority.Assistant);
        }

        [Test]
        public void Family_Join_Fail_Custom_Role_Two_Leaders()
        {
            IPlayerEntity newPlayer = LoadPlayer("test_member_fail_already_leader");
            _familyEventHandler.Execute(newPlayer, new FamilyJoinEvent
            {
                Family = _leader.Family,
                Force = true,
                ExpectedAuthority = FamilyAuthority.Head
            });

            Assert.IsNotNull(_leader.Family);
            Assert.IsTrue(_leader.IsFamilyLeader);
            Assert.IsNull(newPlayer.Family);
            Assert.IsNull(newPlayer.FamilyCharacter);
        }

        [Test]
        public void Family_Join_Fail_Already_In_Family()
        {
            IPlayerEntity newPlayer = LoadPlayer("test_member_fail_already");
            _familyEventHandler.Execute(newPlayer, new FamilyJoinEvent
            {
                Family = _leader.Family,
                Force = true,
                ExpectedAuthority = FamilyAuthority.Member
            });

            Assert.AreEqual(_leader.Family, newPlayer.Family);
            Assert.AreEqual(_leader.Family.Id, newPlayer.FamilyCharacter.FamilyId);
            Assert.AreEqual(newPlayer.Character.Id, newPlayer.FamilyCharacter.CharacterId);
            Assert.AreEqual(newPlayer.FamilyCharacter.Authority, FamilyAuthority.Member);
        }
    }
}