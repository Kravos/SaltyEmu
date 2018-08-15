﻿using ChickenAPI.Game.Packets;

namespace ChickenAPI.Game.Features.Specialists.Packets
{
    [PacketHeader("sl")]
    public class SlPacket : PacketBase
    {
        #region Properties

        [PacketIndex(0)]
        public byte Type { get; set; }

        [PacketIndex(3)]
        public int TransportId { get; set; }

        [PacketIndex(4)]
        public short SpecialistDamage { get; set; }

        [PacketIndex(5)]
        public short SpecialistDefense { get; set; }

        [PacketIndex(6)]
        public short SpecialistElement { get; set; }

        [PacketIndex(7)]
        public short SpecialistHp { get; set; }

        #endregion
    }
}