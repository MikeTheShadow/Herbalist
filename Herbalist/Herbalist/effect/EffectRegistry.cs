using System;
using System.Collections.Generic;
using Vintagestory.API.Common;

namespace Herbalist.effect;

public class EffectRegistry
{
    private bool _isFrozen;

    internal Dictionary<string, IPlayerEffect> EffectReg { get; } = new();

    private readonly ICoreAPI _api;

    internal EffectRegistry(ICoreAPI api)
    {
        _api = api;
    }

    /**
     * Your Mod (I need your MODID) , the effect name, and the class for the effect
     */
    public void RegisterEffect(ModSystem modSystem, string name, IPlayerEffect effect)
    {
        if (_isFrozen) throw new RegistryFrozenException("Effect Registery has been frozen!");
        string effectName = modSystem.Mod.Info.ModID + ":" + name;
        _api.Logger.Notification($"Registering Effect with name {effectName}");
        EffectReg[effectName] = effect;
    }


    public IPlayerEffect GetEffect(string name)
    {
        if (EffectReg.TryGetValue(name, out IPlayerEffect effect)) return effect;
        _api.Logger.Warning($"No effect registered for {name}");
        return null;
    }

    internal void FreezeRegistry()
    {
        _isFrozen = true;
    }

    internal void ResetRegistry()
    {
        _isFrozen = false;
        EffectReg.Clear();
    }

    public bool IsFrozen() => _isFrozen;
}

class RegistryFrozenException : Exception
{
    public RegistryFrozenException(string message) : base(message)
    {
        
    }
}