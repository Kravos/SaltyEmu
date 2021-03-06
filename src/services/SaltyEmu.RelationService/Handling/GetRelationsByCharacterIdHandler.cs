﻿using System.Threading.Tasks;
using ChickenAPI.Core.IPC.Protocol;
using ChickenAPI.Data.Relations;
using SaltyEmu.Communication.Utils;
using SaltyEmu.FriendsPlugin.Protocol;

namespace SaltyEmu.RelationService.Handling
{
    public class GetRelationsByCharacterIdHandler : GenericIpcRequestHandler<GetRelationsByCharacterId, GetRelationsByCharacterId.Response>
    {
        private readonly IRelationDao _repository;

        public GetRelationsByCharacterIdHandler(IRelationDao repository)
        {
            _repository = repository;
        }

        protected override async Task<GetRelationsByCharacterId.Response> Handle(GetRelationsByCharacterId request)
        {
            return new GetRelationsByCharacterId.Response
            {
                Relations = (await _repository.GetRelationsByCharacterId(request.CharacterId))
            };
        }
    }
}