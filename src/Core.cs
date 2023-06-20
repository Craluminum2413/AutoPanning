using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

[assembly: ModInfo("Auto Panning")]

namespace AutoPanning;

public class Core : ModSystem
{
    public int SearchRange { get; set; } = 5;
    public bool AutoPanning { get; set; }

    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

    public override void StartClientSide(ICoreClientAPI capi)
    {
        base.StartClientSide(capi);
        capi.Input.RegisterHotKey("autopanning", Lang.Get("autopanning:ToggleAutoPanning"), GlKeys.X, HotkeyType.CharacterControls, ctrlPressed: true);
        capi.Input.SetHotKeyHandler("autopanning", ToggleAutoPanning);

        capi.Event.RegisterGameTickListener(x => OnGameTick(x, capi), 1000);

        capi.World.Logger.Event("started 'Auto Panning' mod");
    }

    private void OnGameTick(float dt, ICoreClientAPI capi)
    {
        ClientMain clientMain = capi.World as ClientMain;
        if (!AutoPanning) return;

        EntityPlayer entityPlayer = capi?.World?.Player.Entity;
        ItemSlot activeSlot = capi?.World?.Player?.InventoryManager?.ActiveHotbarSlot;
        ItemStack heldItem = activeSlot?.Itemstack;
        BlockPos playerPos = entityPlayer?.Pos?.AsBlockPos;

        if (heldItem?.Collectible is BlockPan blockPan)
        {
            if (TryPan(capi, activeSlot)) return;

            for (int dx = -SearchRange; dx <= SearchRange; dx++)
            {
                for (int dy = -SearchRange; dy <= SearchRange; dy++)
                {
                    for (int dz = -SearchRange; dz <= SearchRange; dz++)
                    {
                        BlockPos blockPos = playerPos.AddCopy(dx, dy, dz);
                        Block block = entityPlayer.World.BlockAccessor.GetBlock(blockPos);

                        if (!blockPan.IsPannableMaterial(block)) continue;

                        var blockSel = new BlockSelection(blockPos, BlockFacing.DOWN, block);
                        clientMain.SendHandInteraction(2, blockSel, null, EnumHandInteract.HeldItemInteract, Vintagestory.Common.EnumHandInteractNw.StartHeldItemUse, true);
                    }
                }
            }
        }
    }

    private static bool TryPan(ICoreClientAPI capi, ItemSlot slot)
    {
        return slot.Itemstack.Attributes.GetAsString("materialBlockCode") != null
            && (capi.Input.InWorldMouseButton.Right = true);
    }

    private bool ToggleAutoPanning(KeyCombination t1)
    {
        AutoPanning = !AutoPanning;
        return true;
    }
}