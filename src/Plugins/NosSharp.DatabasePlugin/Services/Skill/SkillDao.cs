﻿using AutoMapper;
using ChickenAPI.Data.Skills;
using ChickenAPI.Game.Data.AccessLayer.Skill;
using Microsoft.EntityFrameworkCore;
using SaltyEmu.DatabasePlugin.Context;
using SaltyEmu.DatabasePlugin.Models.Skill;
using SaltyEmu.DatabasePlugin.Services.Base;

namespace SaltyEmu.DatabasePlugin.Services.Skill
{
    public class SkillDao : MappedRepositoryBase<SkillDto, SkillModel>, ISkillService
    {
        public SkillDao(DbContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}