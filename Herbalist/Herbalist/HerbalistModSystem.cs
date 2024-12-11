using Herbalist.blockentities;
using Herbalist.blocks;
using Herbalist.effect;
using Herbalist.gui;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Common;

namespace Herbalist;

public class HerbalistModSystem : ModSystem
{
    private ICoreAPI _api;
    public EffectRegistry EffectRegistry { get; private set; }
    public EffectManager EffectManager { get; private set; }

    public override void StartPre(ICoreAPI api)
    {
        EffectRegistry = new EffectRegistry(api);
        EffectManager = new EffectManager(EffectRegistry, api);
    }

    // Called on server and client
    public override void Start(ICoreAPI api)
    {
        _api = api;
        RegisterBlockEntity("ModifiedBush", typeof(BlockEntityModifiedBush));
        
        // Teapot
        RegisterBlockEntity("EntityTeapot", typeof(BlockEntityTeapot));
        RegisterBlock("BlockTeapot", typeof(BlockTeapot));
        
        EffectRegistry.RegisterEffect(this, "speed", new EffectSpeed(_api));
        EffectRegistry.FreezeRegistry();
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        api.World.RegisterGameTickListener(_ =>
        {
            foreach (IPlayer player in api.World.AllOnlinePlayers)
            {
                EffectManager.TickPlayerEffectsServer(player);
            }

        }, 1000,1000);

        api.Event.PlayerNowPlaying += player =>
        {
            EffectManager.AddPlayerEffect("herbalist:speed", player, 10, 20);
        };
    }
    
    public override void StartClientSide(ICoreClientAPI api)
    {
        
        api.World.RegisterGameTickListener(_ =>
        {
            EffectManager.TickPlayerEffectsClient(api.World.Player);
        }, 1000,1000);
        
        api.Event.RegisterRenderer(new EffectHudRenderer(api,EffectRegistry,EffectManager),EnumRenderStage.Ortho);
    }

    private void RegisterBlockEntity(string name, System.Type itemType)
    {
        _api.RegisterBlockEntityClass(Mod.Info.ModID + "." + name, itemType);
    }

    private void RegisterBlock(string name, System.Type itemType)
    {
        _api.RegisterBlockClass(Mod.Info.ModID + "." + name, itemType);
    }

    public override void Dispose()
    {
        _api.Logger.Notification("Dispose cleanup!");
        EffectRegistry.ResetRegistry();
    }
}