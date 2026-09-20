using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game;

public class SubsystemEntityBlockBehavior2 : SubsystemBlockBehavior
{
    public SubsystemBlockEntities SubsystemBlockEntities;

    public override int[] HandledBlocks => new int[4] { 45, 64, 27, 216 };

    public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
    {
        Point3 value = new Point3(raycastResult.CellFace.X, raycastResult.CellFace.Y, raycastResult.CellFace.Z);
        ComponentBlockEntity blockEntity = SubsystemBlockEntities.GetBlockEntity(value.X, value.Y, value.Z);
        if (blockEntity == null && componentMiner.ComponentPlayer != null)
        {
            switch (base.Project.FindSubsystem<SubsystemTerrain>().Terrain.GetCellContents(value.X, value.Y, value.Z))
            {
                case 45:
                    {
                        DatabaseObject databaseObject4 = base.Project.GameDatabase.Database.FindDatabaseObject("Chest", base.Project.GameDatabase.EntityTemplateType, throwIfNotFound: true);
                        ValuesDictionary valuesDictionary4 = new ValuesDictionary();
                        valuesDictionary4.PopulateFromDatabaseObject(databaseObject4);
                        valuesDictionary4.GetValue<ValuesDictionary>("BlockEntity").SetValue("Coordinates", value);
                        Entity entity4 = base.Project.CreateEntity(valuesDictionary4);
                        base.Project.AddEntity(entity4);
                        ComponentChest componentChest = entity4.FindComponent<ComponentChest>(throwOnError: true);
                        for (int j = 0; j < 15; j++)
                        {
                            componentChest.m_slots[j].Value = new Random().Int(1, BlocksManager.Blocks.Length - 1);
                            componentChest.m_slots[j].Count = 1;
                        }
                        componentMiner.ComponentPlayer.ComponentGui.ModalPanelWidget = new ChestWidget(componentMiner.Inventory, componentChest);
                        AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                        return true;
                    }
                case 64:
                    {
                        DatabaseObject databaseObject3 = base.Project.GameDatabase.Database.FindDatabaseObject("Furnace", base.Project.GameDatabase.EntityTemplateType, throwIfNotFound: true);
                        ValuesDictionary valuesDictionary3 = new ValuesDictionary();
                        valuesDictionary3.PopulateFromDatabaseObject(databaseObject3);
                        valuesDictionary3.GetValue<ValuesDictionary>("BlockEntity").SetValue("Coordinates", value);
                        Entity entity3 = base.Project.CreateEntity(valuesDictionary3);
                        base.Project.AddEntity(entity3);
                        ComponentFurnace componentFurnace = entity3.FindComponent<ComponentFurnace>(throwOnError: true);
                        int[] array2 = new int[3] { 77, 88, 176 };
                        if (new Random().Float(0f, 1f) < 0.4f)
                        {
                            componentFurnace.m_slots[0].Value = array2[new Random().Int(0, array2.Length - 1)];
                            componentFurnace.m_slots[0].Count = 1;
                        }
                        componentMiner.ComponentPlayer.ComponentGui.ModalPanelWidget = new FurnaceWidget(componentMiner.Inventory, componentFurnace);
                        AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                        return true;
                    }
                case 27:
                    {
                        DatabaseObject databaseObject2 = base.Project.GameDatabase.Database.FindDatabaseObject("CraftingTable", base.Project.GameDatabase.EntityTemplateType, throwIfNotFound: true);
                        ValuesDictionary valuesDictionary2 = new ValuesDictionary();
                        valuesDictionary2.PopulateFromDatabaseObject(databaseObject2);
                        valuesDictionary2.GetValue<ValuesDictionary>("BlockEntity").SetValue("Coordinates", value);
                        Entity entity2 = base.Project.CreateEntity(valuesDictionary2);
                        base.Project.AddEntity(entity2);
                        ComponentCraftingTable componentCraftingTable = entity2.FindComponent<ComponentCraftingTable>(throwOnError: true);
                        int[] array = new int[15]
                        {
                    29, 165, 37, 222, 36, 38, 218, 219, 171, 169,
                    90, 117, 121, 120, 230
                        };
                        for (int i = 0; i < 9; i++)
                        {
                            if (new Random().Float(0f, 1f) < 0.1f)
                            {
                                componentCraftingTable.m_slots[i].Value = array[new Random().Int(0, array.Length - 1)];
                                componentCraftingTable.m_slots[i].Count = 1;
                            }
                        }
                        componentMiner.ComponentPlayer.ComponentGui.ModalPanelWidget = new CraftingTableWidget(componentMiner.Inventory, componentCraftingTable);
                        AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                        return true;
                    }
                case 216:
                    {
                        DatabaseObject databaseObject = base.Project.GameDatabase.Database.FindDatabaseObject("Dispenser", base.Project.GameDatabase.EntityTemplateType, throwIfNotFound: true);
                        ValuesDictionary valuesDictionary = new ValuesDictionary();
                        valuesDictionary.PopulateFromDatabaseObject(databaseObject);
                        valuesDictionary.GetValue<ValuesDictionary>("BlockEntity").SetValue("Coordinates", value);
                        Entity entity = base.Project.CreateEntity(valuesDictionary);
                        base.Project.AddEntity(entity);
                        ComponentDispenser componentDispenser = entity.FindComponent<ComponentDispenser>(throwOnError: true);
                        componentMiner.ComponentPlayer.ComponentGui.ModalPanelWidget = new DispenserWidget(componentMiner.Inventory, componentDispenser);
                        AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                        return true;
                    }
            }
        }
        return false;
    }

    public override void Load(ValuesDictionary valuesDictionary)
    {
        SubsystemBlockEntities = base.Project.FindSubsystem<SubsystemBlockEntities>(throwOnError: true);
        base.SubsystemTerrain = base.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
    }
}
