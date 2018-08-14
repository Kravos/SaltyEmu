﻿using ChickenAPI.Core.Logging;
using ChickenAPI.Game.Entities.Player;
using ChickenAPI.Game.Features.Movement.Extensions;
using ChickenAPI.Game.Packets;
using ChickenAPI.Game.Packets.Game.Client;
using ChickenAPI.Game.Packets.Game.Server;

namespace NosSharp.PacketHandler.Move
{
    public class WalkPacketHandling
    {
        private static readonly Logger Log = Logger.GetLogger<WalkPacketHandling>();

        public static void OnWalkPacket(WalkPacket packet, IPlayerEntity session)
        {
            if (session.Movable.Actual.X == packet.XCoordinate && session.Movable.Actual.Y == packet.YCoordinate)
            {
                return;
            }

            if (session.Movable.Speed < packet.Speed)
            {
                return;
            }

            session.Movable.Actual.X = packet.XCoordinate;
            session.Movable.Actual.Y = packet.YCoordinate;
            session.Movable.Speed = packet.Speed;

            session.SendPacket(session.GenerateCondPacket());
            if (session.EntityManager is IBroadcastable broadcastable)
            {
                broadcastable.Broadcast(new MvPacket(session));
            }
        }
    }
}