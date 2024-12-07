using System.Text;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Herbalist.blockentities;

public class BlockEntityModifiedBush : BlockEntity, IAnimalFoodSource
{
    private static Random _rand = new();
    private double _lastCheckAtTotalDays;
    private double _transitionHoursLeft = -1.0;
    private double? _totalDaysForNextStageOld;
    private RoomRegistry _roomreg;
    private int _roomness;
    private bool _pruned;
    private double _lastPrunedTotalDays;
    private float _resetBelowTemperature;
    private float _resetAboveTemperature;
    private float _stopBelowTemperature;
    private float _stopAboveTemperature;
    private float _revertBlockBelowTemperature;
    private float _revertBlockAboveTemperature;
    private float _growthRateMul = 1f;
    private string[] _creatureDietFoodTags;

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        this._growthRateMul = (float)Api.World.Config.GetDecimal("cropGrowthRateMul", _growthRateMul);
        if (!(api is ICoreServerAPI))
            return;
        _creatureDietFoodTags = Block.Attributes["foodTags"].AsArray<string>();
        if (_transitionHoursLeft <= 0.0)
        {
            _transitionHoursLeft = GetHoursForNextStage();
            _lastCheckAtTotalDays = api.World.Calendar.TotalDays;
        }

        if (Api.World.Config.GetBool("processCrops", true))
            RegisterGameTickListener(CheckGrow, 8000);
        api.ModLoader.GetModSystem<POIRegistry>().AddPOI(this);
        _roomreg = Api.ModLoader.GetModSystem<RoomRegistry>();
        if (!_totalDaysForNextStageOld.HasValue)
            return;
        _transitionHoursLeft = (_totalDaysForNextStageOld.Value - Api.World.Calendar.TotalDays) *
                                   Api.World.Calendar.HoursPerDay;
    }

    internal void Prune()
    {
        _pruned = true;
        _lastPrunedTotalDays = Api.World.Calendar.TotalDays;
        MarkDirty(true);
    }

    private void CheckGrow(float dt)
    {
        if (!((ICoreServerAPI)Api).World.IsFullyLoadedChunk(Pos) || Block.Attributes == null)
            return;
        _lastCheckAtTotalDays = Math.Min(_lastCheckAtTotalDays, Api.World.Calendar.TotalDays);
        double num1 = GameMath.Mod(Api.World.Calendar.TotalDays - _lastCheckAtTotalDays,
            Api.World.Calendar.DaysPerYear);
        float num2 = 2f / Api.World.Calendar.HoursPerDay;
        if (num1 <= num2)
            return;
        if (Api.World.BlockAccessor.GetRainMapHeightAt(Pos) > Pos.Y)
        {
            Room roomForPosition = _roomreg?.GetRoomForPosition(Pos);
            _roomness = roomForPosition == null ||
                            roomForPosition.SkylightCount <= roomForPosition.NonSkylightCount ||
                            roomForPosition.ExitCount != 0
                ? 0
                : 1;
        }
        else
            _roomness = 0;

        ClimateCondition baseClimate = null;
        float num3 = 0.0f;
        while (num1 > num2)
        {
            num1 -= num2;
            _lastCheckAtTotalDays += num2;
            _transitionHoursLeft -= 2.0;
            if (baseClimate == null)
            {
                baseClimate = Api.World.BlockAccessor.GetClimateAt(Pos,
                    EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, _lastCheckAtTotalDays);
                if (baseClimate == null)
                    return;
                num3 = baseClimate.WorldGenTemperature;
            }
            else
            {
                baseClimate.Temperature = num3;
                Api.World.BlockAccessor.GetClimateAt(Pos, baseClimate,
                    EnumGetClimateMode.ForSuppliedDate_TemperatureOnly, _lastCheckAtTotalDays);
            }

            float temperature = baseClimate.Temperature;
            if (_roomness > 0)
                temperature += 5f;
            bool flag = temperature < _resetBelowTemperature ||
                        temperature > _resetAboveTemperature;
            if (((temperature < _stopBelowTemperature
                    ? 1
                    : (temperature > _stopAboveTemperature ? 1 : 0)) | (flag ? 1 : 0)) != 0)
            {
                _transitionHoursLeft += 2.0;
                if (flag)
                {
                    int num4 = temperature < _revertBlockBelowTemperature
                        ? 1
                        : (temperature > _revertBlockAboveTemperature ? 1 : 0);
                    _transitionHoursLeft = GetHoursForNextStage();
                    if (num4 != 0 && Block.Variant["state"] != "empty")
                        Api.World.BlockAccessor.ExchangeBlock(
                            Api.World.GetBlock(Block.CodeWithVariant("state", "flowering")).BlockId, Pos);
                }
            }
            else if (_transitionHoursLeft <= 0.0)
            {
                if (!DoGrow())
                    return;
                _transitionHoursLeft = GetHoursForNextStage();
            }
        }

        MarkDirty();
    }

    public override void OnExchanged(Block block)
    {
        base.OnExchanged(block);
        ICoreAPI api = Api;
        if ((api != null ? (api.Side == EnumAppSide.Server ? 1 : 0) : 0) == 0)
            return;
        UpdateTransitionsFromBlock();
    }

    public override void CreateBehaviors(Block block, IWorldAccessor worldForResolve)
    {
        base.CreateBehaviors(block, worldForResolve);
        if (worldForResolve.Side != EnumAppSide.Server)
            return;
        UpdateTransitionsFromBlock();
    }

    private void UpdateTransitionsFromBlock()
    {
        if (Block?.Attributes == null)
        {
            _resetBelowTemperature = _stopBelowTemperature = _revertBlockBelowTemperature = -999f;
            _resetAboveTemperature = _stopAboveTemperature = _revertBlockAboveTemperature = 999f;
        }
        else
        {
            _resetBelowTemperature = Block.Attributes["resetBelowTemperature"].AsFloat(-999f);
            _resetAboveTemperature = Block.Attributes["resetAboveTemperature"].AsFloat(999f);
            _stopBelowTemperature = Block.Attributes["stopBelowTemperature"].AsFloat(-999f);
            _stopAboveTemperature = Block.Attributes["stopAboveTemperature"].AsFloat(999f);
            _revertBlockBelowTemperature = Block.Attributes["revertBlockBelowTemperature"].AsFloat(-999f);
            _revertBlockAboveTemperature = Block.Attributes["revertBlockAboveTemperature"].AsFloat(999f);
        }
    }

    private double GetHoursForNextStage()
    {
        return IsRipe()
            ? 4.0 * (5.0 + _rand.NextDouble()) * 1.6 * Api.World.Calendar.HoursPerDay
            : (5.0 + _rand.NextDouble()) * 1.6 * Api.World.Calendar.HoursPerDay /
              _growthRateMul;
    }

    private bool IsRipe()
    {
        return Api.World.BlockAccessor.GetBlock(Pos).LastCodePart() == "ripe";
    }

    private bool DoGrow()
    {
        if (Api.World.Calendar.TotalDays - _lastPrunedTotalDays > Api.World.Calendar.DaysPerYear)
            _pruned = false;
        Block block1 = Api.World.BlockAccessor.GetBlock(Pos);
        string component;
        switch (block1.LastCodePart())
        {
            case "empty":
                component = "flowering";
                break;
            case "flowering":
                component = "ripe";
                break;
            default:
                component = "empty";
                break;
        }

        AssetLocation blockCode = block1.CodeWithParts(component);
        if (!blockCode.Valid)
        {
            Api.World.BlockAccessor.RemoveBlockEntity(Pos);
            return false;
        }

        Block block2 = Api.World.GetBlock(blockCode);
        if (block2?.Code == null)
            return false;
        Api.World.BlockAccessor.ExchangeBlock(block2.BlockId, Pos);
        MarkDirty(true);
        return true;
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
    {
        base.FromTreeAttributes(tree, worldForResolving);
        _transitionHoursLeft = tree.GetDouble("transitionHoursLeft");
        if (tree.HasAttribute("totalDaysForNextStage"))
            _totalDaysForNextStageOld = tree.GetDouble("totalDaysForNextStage");
        _lastCheckAtTotalDays = tree.GetDouble("lastCheckAtTotalDays");
        _roomness = tree.GetInt("roomness");
        _pruned = tree.GetBool("pruned");
        _lastPrunedTotalDays = tree.GetDecimal("lastPrunedTotalDays");
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        tree.SetDouble("transitionHoursLeft", _transitionHoursLeft);
        tree.SetDouble("lastCheckAtTotalDays", _lastCheckAtTotalDays);
        tree.SetBool("pruned", _pruned);
        tree.SetInt("roomness", _roomness);
        tree.SetDouble("lastPrunedTotalDays", _lastPrunedTotalDays);
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
    {
        Block block = Api.World.BlockAccessor.GetBlock(Pos);

        double num = _transitionHoursLeft / Api.World.Calendar.HoursPerDay;
        if (block.LastCodePart() == "ripe")
            return;
        sb.AppendLine(num < 1.0
            ? Lang.Get("herbalist:modifiedbush-growing-1day")
            : Lang.Get("herbalist:modifiedbush-growing-xdays", (int)num));
        if (_roomness <= 0)
            return;
        sb.AppendLine(Lang.Get("greenhousetempbonus"));
    }

    public bool IsSuitableFor(Entity entity, CreatureDiet diet)
    {
        return diet != null && IsRipe() && diet.Matches(EnumFoodCategory.NoNutrition, _creatureDietFoodTags);
    }

    public float ConsumeOnePortion(Entity entity)
    {
        AssetLocation blockCode = Block.CodeWithParts("empty");
        if (!blockCode.Valid)
        {
            Api.World.BlockAccessor.RemoveBlockEntity(Pos);
            return 0.0f;
        }

        Block block = Api.World.GetBlock(blockCode);
        if (block?.Code == null)
            return 0.0f;
        BlockBehaviorHarvestable behavior = Block.GetBehavior<BlockBehaviorHarvestable>();
        if (behavior?.harvestedStack != null)
        {
            ItemStack nextItemStack = behavior.harvestedStack.GetNextItemStack();
            Api.World.PlaySoundAt(behavior.harvestingSound, Pos.X + 0.5, Pos.Y + 0.5,
                Pos.Z + 0.5);
            Api.World.SpawnItemEntity(nextItemStack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
        }

        Api.World.BlockAccessor.ExchangeBlock(block.BlockId, Pos);
        MarkDirty(true);
        return 0.1f;
    }

    public Vec3d Position => Pos.ToVec3d().Add(0.5, 0.5, 0.5);

    public string Type => "food";

    public override void OnBlockRemoved()
    {
        base.OnBlockRemoved();
        if (Api.Side != EnumAppSide.Server)
            return;
        Api.ModLoader.GetModSystem<POIRegistry>().RemovePOI(this);
    }

    public override void OnBlockUnloaded()
    {
        base.OnBlockUnloaded();
        ICoreAPI api = Api;
        if ((api != null ? (api.Side == EnumAppSide.Server ? 1 : 0) : 0) == 0)
            return;
        Api.ModLoader.GetModSystem<POIRegistry>().RemovePOI(this);
    }

    public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
    {
        if (!_pruned)
            return base.OnTesselation(mesher, tessThreadTesselator);
        mesher.AddMeshData(((BlockBerryBush)Block).GetPrunedMesh(Pos));
        return true;
    }
}