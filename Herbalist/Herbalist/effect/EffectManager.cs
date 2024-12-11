using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Herbalist.effect;

public class EffectManager
{
    private readonly EffectRegistry _effectRegistry;
    internal Dictionary<string, int> ClientSideEffectsWithDuration { get; } = new();
    private readonly ICoreAPI _api;

    internal EffectManager(EffectRegistry registry, ICoreAPI api)
    {
        _effectRegistry = registry;
        _api = api;
    }

    public void AddPlayerEffect(string effectId, IPlayer player, int duration, int strength)
    {
        IPlayerEffect effect = _effectRegistry.GetEffect(effectId);
        if (effect == null)
        {
            _api.Logger.Warning($"Effect {effectId} not found! Make sure it's properly registered.");
            return;
        }
        ITreeAttribute treeAttribute = GetTreeAttribute(player, effectId, true);
        treeAttribute.SetInt("duration", duration);
        treeAttribute.SetInt("strength", strength);
        effect.EffectStart(player, true, strength);
    }

    public void RemovePlayerEffect(string effectId, IPlayer player)
    {
    }

    public void TickPlayerEffectsServer(IPlayer player)
    {
        // Access the player's effects attribute tree
        ITreeAttribute playerEffectsAttribute = player.Entity.WatchedAttributes.GetOrAddTreeAttribute("effects");

        List<string> keys = new();
        foreach (var treeAttribute in playerEffectsAttribute)
        {
            keys.Add(treeAttribute.Key);
        }

        foreach (string key in keys)
        {
            var effect = _effectRegistry.GetEffect(key);
            var attribute = playerEffectsAttribute.GetTreeAttribute(key);
            playerEffectsAttribute.RemoveAttribute("duration");
            if (effect == null) continue;

            int duration = attribute.GetInt("duration");
            int strength = attribute.GetInt("strength");

            effect.EffectTick(player, true, duration, strength);
            if (duration-- <= 0)
            {
                playerEffectsAttribute.RemoveAttribute(key);
                effect.EffectStop(player, true, strength);
            }
            else
            {
                attribute.SetInt("duration", duration);
            }
        }
        player.Entity.WatchedAttributes.MarkPathDirty("effects");
    }


    public void TickPlayerEffectsClient(IPlayer player)
    {
        ITreeAttribute playerEffectsAttribute = player.Entity.WatchedAttributes.GetOrAddTreeAttribute("effects");
        
        foreach (KeyValuePair<string, IAttribute> attributes in playerEffectsAttribute)
        {
            var effect = _effectRegistry.GetEffect(attributes.Key);
            var attribute = playerEffectsAttribute.GetOrAddTreeAttribute(attributes.Key);
            if (effect == null) continue;
            int duration = attribute.GetInt("duration");
            int strength = attribute.GetInt("strength");
            effect.EffectTick(player, false, duration, strength);
            _api.Logger.Notification("Duration: " + duration);
            if(duration > 0) ClientSideEffectsWithDuration[attributes.Key] = duration;
            else ClientSideEffectsWithDuration.Remove(attributes.Key);
        }
    }

    private ITreeAttribute GetTreeAttribute(IPlayer player, string effectId, bool markDirty)
    {
        SyncedTreeAttribute entityAttributes = player.Entity.WatchedAttributes;
        ITreeAttribute attribute = entityAttributes.GetOrAddTreeAttribute("effects");
        ITreeAttribute effectAttribute = attribute.GetOrAddTreeAttribute(effectId);
        if (markDirty) entityAttributes.MarkPathDirty("effects");
        
        return effectAttribute;
    }
}