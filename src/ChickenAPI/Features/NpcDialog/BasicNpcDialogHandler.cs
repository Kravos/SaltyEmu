﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ChickenAPI.Core.Logging;
using ChickenAPI.Game.Entities.Player;
using ChickenAPI.Game.Features.NpcDialog.Events;
using ChickenAPI.Game.Features.NpcDialog.Handlers;

namespace ChickenAPI.Game.Features.NpcDialog
{
    public class BasicNpcDialogHandler : INpcDialogHandler
    {
        private static readonly Logger Log = Logger.GetLogger<BasicNpcDialogHandler>();
        protected readonly Dictionary<long, NpcDialogHandlerAttribute> HandlersByDialogId;

        public BasicNpcDialogHandler()
        {
            HandlersByDialogId = new Dictionary<long, NpcDialogHandlerAttribute>();
            Assembly currentAsm = Assembly.GetAssembly(typeof(BasicNpcDialogHandler));
            foreach (Type handler in currentAsm.GetTypes().Where(s => s.GetMethods().Any(m => m.GetCustomAttribute<NpcDialogHandlerAttribute>() != null)))
            {
                Log.Info("GetTypes()");
                foreach (MethodInfo method in handler.GetMethods().Where(s => s.GetCustomAttribute<NpcDialogHandlerAttribute>() != null))
                {
                    Register(method.GetCustomAttribute<NpcDialogHandlerAttribute>());
                }
            }
        }

        public void Register(NpcDialogHandlerAttribute handlerAttribute)
        {
            if (HandlersByDialogId.ContainsKey(handlerAttribute.NpcDialogId))
            {
                return;
            }

            Log.Info($"[REGISTER_HANDLER] NPC_DIALOG_ID : {handlerAttribute.NpcDialogId} REGISTERED !");
            HandlersByDialogId.Add(handlerAttribute.NpcDialogId, handlerAttribute);
        }

        public void Unregister(long npcDialogId)
        {
            HandlersByDialogId.Remove(npcDialogId);
        }

        public void Unregister(NpcDialogHandlerAttribute handlerAttribute)
        {
            Unregister(handlerAttribute.NpcDialogId);
        }

        public void Execute(IPlayerEntity player, NpcDialogEventArgs eventArgs)
        {
            if (!HandlersByDialogId.TryGetValue(eventArgs.DialogId, out NpcDialogHandlerAttribute handler))
            {
                return;
            }

            handler.Handle(player, eventArgs);
        }
    }
}