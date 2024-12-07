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
    public override void Start(ICoreAPI api)
    {
        api.Logger.Notification("preprocessing!");
        _api = api;
        
        // Sage
        RegisterBlock("BlockSage", typeof(BlockSage));
        RegisterItem("SageSeeds", typeof(SageSeeds));
        RegisterItem("ItemSage", typeof(ItemSage));

        // Thyme
        RegisterBlock("BlockThyme", typeof(BlockThyme));
        RegisterItem("ThymeSeeds", typeof(ThymeSeeds));
        RegisterItem("ItemThyme", typeof(ItemThyme));
        
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        
    }

    // Helper methods
    private void RegisterBlock(string name, System.Type blockType)
    {
        _api.RegisterBlockClass(Mod.Info.ModID + "." + name, blockType);
    }

    private void RegisterItem(string name, System.Type itemType)
    {
        _api.RegisterItemClass(Mod.Info.ModID + "." + name, itemType);
    }
}