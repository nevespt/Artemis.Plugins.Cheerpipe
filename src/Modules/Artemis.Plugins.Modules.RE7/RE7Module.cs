﻿using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Modules.FallGuys.DataModels;
using Serilog;
using SRTPluginProviderRE7;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.Modules.Games
{
    [PluginFeature(Name = "Resident Evil 7 Biohazard", Icon = "Umbrella.svg", AlwaysEnabled = true)]
    public class RE7Module : Module<RE7DataModel>
    {
        private readonly ILogger _logger;
        private ReaderRE7 _readerRE7;
        private IGameMemoryRE7 _gameMemoryRE7;

        public override List<IModuleActivationRequirement> ActivationRequirements { get; } = new()
        {
            new ProcessActivationRequirement("re7")
        };



        public override void Enable()
        {
            UpdateDuringActivationOverride = false;
        }


        public RE7Module(ILogger logger)
        {
            _logger = logger;
        }

        // This is the end of your plugin life cycle.
        public override void Disable()
        {
            // Make sure to clean up resources where needed (dispose IDisposables etc.)
        }

        public override void ModuleActivated(bool isOverride)
        {
            _readerRE7 = new ReaderRE7(_logger);
            _readerRE7.Init();
            AddTimedUpdate(TimeSpan.FromMilliseconds(500), _ => ReadMemory());
        }

        private void ReadMemory()
        {
            _gameMemoryRE7 = (IGameMemoryRE7)_readerRE7.PullData();

            if (_gameMemoryRE7 == null)
                return;
            float previousHp = DataModel.CurrentHP;

            DataModel.CurrentDA = _gameMemoryRE7.CurrentDA;
            DataModel.MapName = _gameMemoryRE7.MapName;
            DataModel.CurrentHP = _gameMemoryRE7.CurrentHP;
            DataModel.MaxHP = _gameMemoryRE7.MaxHP;
            DataModel.MrEverything = _gameMemoryRE7.MrEverything;
            DataModel.PlayerInventorySlots = _gameMemoryRE7.PlayerInventorySlots;
            DataModel.PlayerInventoryCount = _gameMemoryRE7.PlayerInventoryCount;
            DataModel.PlayerCurrentSelectedInventorySlots = _gameMemoryRE7.PlayerCurrentSelectedInventorySlots;

            if (DataModel.CurrentHP < previousHp && DataModel.CurrentHP != 0)
            {
                DataModel.DamageReceived.Trigger();
            }
            else if (DataModel.CurrentHP > previousHp + 24)
            {
                DataModel.HealingReceived.Trigger();
            }
        }

        public override void ModuleDeactivated(bool isOverride)
        {
            _readerRE7?.Dispose();
            _readerRE7 = null;
        }

        public override void Update(double deltaTime)
        {
        }
    }
}