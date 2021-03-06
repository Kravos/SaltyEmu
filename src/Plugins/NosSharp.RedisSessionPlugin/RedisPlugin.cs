﻿using System;
using Autofac;
using ChickenAPI.Core.i18n;
using ChickenAPI.Core.IoC;
using ChickenAPI.Core.Logging;
using ChickenAPI.Core.Plugins;
using ChickenAPI.Core.Utils;
using ChickenAPI.Data.Server;
using ChickenAPI.Game._i18n;
using SaltyEmu.Redis;
using SaltyEmu.RedisWrappers.Redis;

namespace SaltyEmu.RedisWrappers
{
    public class RedisPlugin : IPlugin
    {
        private static readonly Logger Log = Logger.GetLogger<RedisPlugin>();
        private readonly string _configurationPath = $"plugins/config/{nameof(RedisPlugin)}/conf.json";
        private RedisConfiguration _configuration;
        public PluginEnableTime EnableTime => PluginEnableTime.PreContainerBuild;
        public string Name => "RedisPlugin";

        public void OnDisable()
        {
            throw new NotImplementedException();
        }

        public void OnEnable()
        {
            // hehe
        }

        public void OnLoad()
        {
            Log.Info("Loading...");
            _configuration = ConfigurationHelper.Load<RedisConfiguration>(_configurationPath, true);
            ChickenContainer.Builder.Register(s => new RedisSessionService(_configuration)).As<ISessionService>();
            Log.Info("ISessionService registered !");
            ChickenContainer.Builder.Register(s => new RedisServerApi(_configuration)).As<IServerApiService>();
            Log.Info("IServerApiService registered !");
            ChickenContainer.Builder.Register(s => new RedisGameLanguageService(_configuration)).As<IGameLanguageService>();
            Log.Info("IGameLanguageService registered !");
        }

        public void ReloadConfig()
        {
            throw new NotImplementedException();
        }

        public void SaveConfig()
        {
            throw new NotImplementedException();
        }

        public void SaveDefaultConfig()
        {
            throw new NotImplementedException();
        }
    }
}