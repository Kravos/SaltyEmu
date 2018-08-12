﻿using ChickenAPI.Core.ECS.Components;
using ChickenAPI.Core.ECS.Entities;
using ChickenAPI.Enums.Game.Portals;
using ChickenAPI.Game.Data.TransferObjects.Map;
using ChickenAPI.Game.Maps;

namespace ChickenAPI.Game.Features.Portals
{
    public class PortalComponent : IComponent
    {
        public PortalComponent(IEntity entity, PortalDto dto)
        {
            Entity = entity;
            PortalId = dto.Id;
            Type = dto.Type;
            DestinationMapLayer = null;
            DestinationMapId = dto.DestinationMapId;
            DestinationX = dto.DestinationX;
            DestinationY = dto.DestinationY;
            SourceX = dto.SourceX;
            SourceY = dto.SourceY;
            IsDisabled = dto.IsDisabled;
        }

        public short DestinationMapId { get; set; }

        public long PortalId { get; set; }

        public PortalType Type { get; set; }

        public IMapLayer DestinationMapLayer { get; set; }

        public short DestinationX { get; set; }
        public short DestinationY { get; set; }

        public short SourceX { get; set; }
        public short SourceY { get; set; }
        public bool IsDisabled { get; set; }

        public IEntity Entity { get; }
    }
}