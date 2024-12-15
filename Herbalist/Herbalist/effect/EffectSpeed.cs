using Vintagestory.API.Common;

namespace Herbalist.effect;

public class EffectSpeed : IPlayerEffect
{
    
    public void EffectStart(IPlayer player, bool onServer, int strength)
    {
        player.Entity.Stats.Set("walkspeed", "herbalist", 0.2f * strength, true);
    }

    public void EffectStop(IPlayer player, bool onServer, int strength)
    {
        player.Entity.Stats.Remove("walkspeed", "herbalist");
    }

    /**
     * Nothing to tick
     */
    public void EffectTick(IPlayer player, bool onServer, int timeLeft, int strength)
    {
        
    }

    public string EffectIconLocation()
    {
        return "herbalist:textures/gui/effecticons/speed.png";
    }

    public string EffectNameLocation()
    {
        return "herbalist:effect-speed";
    }

    public bool IsCleansable()
    {
        return false;
    }
}