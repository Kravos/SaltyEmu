﻿using ChickenAPI.Data.TransferObjects.Shop;
using ChickenAPI.ECS.Systems;
using ChickenAPI.Packets.Game.Client;

namespace ChickenAPI.Game.Features.Shops.Args
{
    public class GetShopInformationEventArgs : SystemEventArgs
    {
        public Shop Shop { get; set; }

        public ShoppingPacket Packet { get; set; }
    }
}