using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Herbalist.effect;

public interface IPlayerEffect
{
    
    /**
     * Gets called the first tick of the effect.
     */
    public void EffectStart(IPlayer player, bool onServer, int strength);
    
    /**
     * Called on the last tick of the effect.
     */
    public void EffectStop(IPlayer player, bool onServer, int strength);
    
    /**
     * Called per "tick" of the effect. Will be called on both the client and server.
     * So if you want to do something to the player only the server make sure to add a guard clause.
     */
    public void EffectTick(IPlayer player,bool onServer, int timeLeft, int strength);

    /**
     *
     */
    public string EffectIconLocation();

    public string EffectNameLocation();
    
    /**
     * Not used at the moment but can be implemented by anyone looking to potentially remove effects from players.
     */
    public bool IsCleansable() => false;
}