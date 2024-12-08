using Herbalist.blockentities;

namespace Herbalist.blocks;

using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

public class BlockTeapot : Block
{
    public int CapacitySeconds = 5;

    public override void OnLoaded(ICoreAPI coreApi)
    {
        base.OnLoaded(coreApi);
        JsonObject attributes = Attributes;
        CapacitySeconds = attributes != null ? attributes["capacitySeconds"].AsInt(5) : 5;
    }

    public override void OnHeldInteractStart(
        ItemSlot slot,
        EntityAgent byEntity,
        BlockSelection blockSel,
        EntitySelection entitySel,
        bool firstEvent,
        ref EnumHandHandling handHandling)
    {
        if (blockSel == null || byEntity.Controls.ShiftKey)
        {
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
        }
        else
        {
            slot.Itemstack.TempAttributes.SetFloat("secondsUsed", 0.0f);
            IPlayer dualCallByPlayer = null;
            if (byEntity is EntityPlayer player)
                dualCallByPlayer = player.World.PlayerByUid(player.PlayerUID);
            Block block1 = byEntity.World.BlockAccessor.GetBlock(blockSel.Position, 2);
            if (block1.LiquidCode != "water") return;
            BlockPos position = blockSel.Position;
            SetRemainingCups(slot.Itemstack, CapacitySeconds);
            slot.Itemstack.TempAttributes.SetInt("refilled", 1);
            slot.MarkDirty();
            byEntity.World.PlaySoundAt(new AssetLocation("sounds/block/water"), position.X,
                position.Y, position.Z, dualCallByPlayer);
            handHandling = EnumHandHandling.PreventDefault;
        }
    }

    public override void OnGroundIdle(EntityItem entityItem)
    {
        base.OnGroundIdle(entityItem);
        if (!entityItem.FeetInLiquid)
            return;
        SetRemainingCups(entityItem.Itemstack, CapacitySeconds);
    }

    public override BlockDropItemStack[] GetDropsForHandbook(
        ItemStack handbookStack,
        IPlayer forPlayer)
    {
        return GetHandbookDropsFromBreakDrops(handbookStack, forPlayer);
    }

    public override ItemStack[] GetDrops(
        IWorldAccessor world,
        BlockPos pos,
        IPlayer byPlayer,
        float dropQuantityMultiplier = 1f)
    {
        return new[]
        {
            OnPickBlock(world, pos)
        };
    }

    public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
    {
        if (!(world.BlockAccessor.GetBlockEntity(pos) is BlockEntityTeapot blockEntity))
            return base.OnPickBlock(world, pos);
        ItemStack stack = new(this);
        SetRemainingCups(stack, blockEntity.TotalCupsLeft);
        return stack;
    }

    public override bool OnHeldInteractStep(
        float secondsUsed,
        ItemSlot slot,
        EntityAgent byEntity,
        BlockSelection blockSel,
        EntitySelection entitySel)
    {
        if (blockSel == null)
            return base.OnHeldInteractStep(secondsUsed, slot, byEntity, null, entitySel);
        if (slot.Itemstack.TempAttributes.GetInt("refilled") > 0)
            return base.OnHeldInteractStep(secondsUsed, slot, byEntity, blockSel, entitySel);
        return base.OnHeldInteractStep(secondsUsed, slot, byEntity, blockSel, entitySel);
    }

    public override void OnHeldInteractStop(
        float secondsUsed,
        ItemSlot slot,
        EntityAgent byEntity,
        BlockSelection blockSel,
        EntitySelection entitySel)
    {
        slot.MarkDirty();
    }

    public override bool DoPlaceBlock(
        IWorldAccessor world,
        IPlayer byPlayer,
        BlockSelection blockSel,
        ItemStack byItemStack)
    {
        int num1 = base.DoPlaceBlock(world, byPlayer, blockSel, byItemStack) ? 1 : 0;
        if (num1 == 0)
            return false;
        if (!(world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityTeapot blockEntity))
            return true;
        BlockPos blockPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
        double num2 = Math.Atan2(byPlayer.Entity.Pos.X - (blockPos.X + blockSel.HitPosition.X),
            byPlayer.Entity.Pos.Z - (blockPos.Z + blockSel.HitPosition.Z));
        float num3 = 0.3926991f;
        double num4 = num3;
        float num5 = (int)Math.Round(num2 / num4) * num3;
        blockEntity.MeshAngle = num5;
        return true;
    }

    public int GetRemainingCups(ItemStack stack)
    {
        return stack.Attributes.GetInt("remainingCups");
    }

    public void SetRemainingCups(ItemStack stack, int amount)
    {
        stack.Attributes.SetInt("remainingCups", amount);
    }

    public override void GetHeldItemInfo(
        ItemSlot inSlot,
        StringBuilder dsc,
        IWorldAccessor world,
        bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        dsc.AppendLine();
        int remainingCups = GetRemainingCups(inSlot.Itemstack);
        
        dsc.AppendLine(remainingCups == 0
            ? Lang.Get("herbalist:block-teapot-empty")
            : Lang.Get("herbalist:block-teapot-cups-left", remainingCups));
    }
}