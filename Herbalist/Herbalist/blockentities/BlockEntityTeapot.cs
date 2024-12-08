using System;
using System.Text;
using Herbalist.blocks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
namespace Herbalist.blockentities;

public class BlockEntityTeapot : BlockEntity
{
    public int TotalCupsLeft;
    private BlockTeapot _ownBlock;
    private ICoreClientAPI _capi;

    public virtual float MeshAngle { get; set; }

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        _ownBlock = api.World.BlockAccessor.GetBlock(Pos) as BlockTeapot;
        _capi = api as ICoreClientAPI;
    }

    public override void OnBlockPlaced(ItemStack byItemStack = null)
    {
        base.OnBlockPlaced(byItemStack);
        if (byItemStack == null)
            return;
        TotalCupsLeft = ((BlockTeapot)byItemStack.Block).GetRemainingCups(byItemStack);
    }

    public override void FromTreeAttributes(
        ITreeAttribute tree,
        IWorldAccessor worldAccessForResolve)
    {
        base.FromTreeAttributes(tree, worldAccessForResolve);
        TotalCupsLeft = tree.GetInt("remainingCups");
        MeshAngle = tree.GetFloat("meshAngle", MeshAngle);
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        tree.SetInt("remainingCups", TotalCupsLeft);
        tree.SetFloat("meshAngle", MeshAngle);
    }

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tesselator)
    {
        mesher.AddMeshData(_capi.TesselatorManager.GetDefaultBlockMesh(Block).Clone().Rotate(new Vec3f(0.5f, 0.5f, 0.5f), 0.0f, MeshAngle, 0.0f));
        return true;
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
    {
        dsc.AppendLine(TotalCupsLeft == 0
            ? Lang.Get("herbalist:block-teapot-empty")
            : Lang.Get("herbalist:block-teapot-cups-left", TotalCupsLeft));
    }
}