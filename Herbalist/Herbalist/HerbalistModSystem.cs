using Herbalist.blocks;
using Herbalist.items.herbs;
using Herbalist.items.seeds;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;

namespace Herbalist;

public class HerbalistModSystem : ModSystem
{
    private ICoreAPI _api;

    // Called on server and client
    // Useful for registering block/entity classes on both sides
    public override void Start(ICoreAPI api)
    {
        api.Logger.Notification("preprocessing!");
        _api = api;
        RegisterBlock("BlockSage", typeof(BlockSage));
        RegisterItem("SageSeeds",typeof(SageSeeds));
        RegisterItem("ItemSage",typeof(ItemSage));
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        api.Logger.Notification("Hello from template mod server side: " + Lang.Get("herbalist:hello"));
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        api.Logger.Notification("Hello from template mod client side: " + Lang.Get("herbalist:hello"));
    }

    private void RegisterBlock(string name, System.Type blockType)
    {
        _api.RegisterBlockClass(Mod.Info.ModID + "." + name, blockType);
    }

    private void RegisterItem(string name, System.Type itemType)
    {
        _api.RegisterItemClass(Mod.Info.ModID + "." + name, itemType);
    }
}