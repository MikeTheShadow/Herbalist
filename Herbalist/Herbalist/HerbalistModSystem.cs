using Herbalist.blockentities;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Common;

namespace Herbalist;

public class HerbalistModSystem : ModSystem
{
    private ICoreAPI _api;

    // Called on server and client
    public override void Start(ICoreAPI api)
    {
        _api = api;
        
        RegisterBlockEntity("ModifiedBush", typeof(BlockEntityModifiedBush));
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        
    }
    
    private void RegisterBlockEntity(string name, System.Type itemType)
    {
        _api.RegisterBlockEntityClass(Mod.Info.ModID + "." + name, itemType);
    }
}