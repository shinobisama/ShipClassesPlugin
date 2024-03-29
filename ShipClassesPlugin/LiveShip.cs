﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ShipClassesPlugin.ShipClassDefinition;

namespace ShipClassesPlugin
{
   public class LiveShip
    {
        public String ClassName;
        public long GridEntityId;
        public Boolean HasWorkingBeacon = false;
        public Dictionary<String, int> UsedLimitsPerDefinition = new Dictionary<string, int>();
        public Boolean HasToBeStation = false;
        //block pair name as key, blockdefinitionName as value, easy reference without looping over lists every time
        public Dictionary<String, String> BlockDefinitionNames = new Dictionary<string, string>();
        public Boolean HasPilot = true;
        public Boolean RequiresPilot = false;

        //we want to only do the checks for if they should be enabled on a timer, if the time isnt above that, we just dont allow the block to be enabled if it has the boolean as false 
        public DateTime NextCheck = DateTime.Now;
        public bool KeepDisabled = false;
        public bool MetMinimumOnce = false;
        public bool HasBeenAddedToActive = false;
        public Boolean IsThisBlockAtMaxLimit(string blockPairName, BlocksDefinition definition, BlockId id)
        {
            if (definition.MaximumPoints == 0)
            {
                return true;
            }
            if (UsedLimitsPerDefinition.TryGetValue(definition.BlocksDefinitionName, out int count))
            {
                if (count < definition.MaximumPoints && (count + id.points) <= definition.MaximumPoints)
                {
                 //   ShipClassPlugin.Log.Info("COUNT " + count);
                    UsedLimitsPerDefinition[definition.BlocksDefinitionName] += id.points;
                    return false;
                }
                else
                {
                    return true;
                }

            }
            else
            {
              //  ShipClassPlugin.Log.Info("ADDING NEW GYRO");
                UsedLimitsPerDefinition.Add(definition.BlocksDefinitionName, id.points);
                return false;
            }
        }

        public Boolean CanBlockFunction(String blockPairName)
        {
            if (ShipClassPlugin.DefinedClasses.TryGetValue(ClassName, out ShipClassDefinition def))
            {
                if (def.GetLimitForBlock(blockPairName) > 0)
                {
                    //now we want to get the name for this limit, which is the annoying part
                    if (BlockDefinitionNames.TryGetValue(blockPairName, out string BlockDefinitionName))
                    {
                        if (BlockDefinitionName.Equals("UNLIMITED"))
                        {
                            return true;
                        }

                        //now we want a method we can call to check against the limits instead of writing it twice 
                        //in this method, we add to the number for that limited block

                        //return the opposite of this because weird code
                        var definition = def.GetBlocksDefinition(BlockDefinitionName);
                        if (definition != null)
                        {
                        //    ShipClassPlugin.Log.Info($"LiveShip 1");
                            return !IsThisBlockAtMaxLimit(blockPairName, definition, definition.blocks.FirstOrDefault(x => x.BlockPairName == blockPairName));
                        }
                        else
                        {
                         //   ShipClassPlugin.Log.Info("DEFINITION IS NULL 1");
                            return true;
                        }
                    }
                    //it didnt have it, so now we loop over the list once until we get it, then store that name for later reference
                    else
                    {
                        BlockId id = new BlockId();
                        id.BlockPairName = blockPairName;
                        string temp = "";
                        Boolean Found = false;
                        foreach (BlocksDefinition definition in def.DefinedBlocks)
                        {
                            if (Found)
                                continue;
                            BlockId result = definition.blocks.FirstOrDefault(x => x.BlockPairName.Equals(id.BlockPairName));
                            if (result != null)
                            {
                                BlockDefinitionNames.Add(blockPairName, definition.BlocksDefinitionName);
                                temp = definition.BlocksDefinitionName;
                                Found = true;
                            }
                        }
                        if (!Found)
                        {
                            BlockDefinitionNames.Add(blockPairName, "UNLIMITED");
                        //    ShipClassPlugin.Log.Info("FAILING HERE");
                            return true;
                        }

                        //in this method, we add to the number for that limited block
                        //return the opposite of this because weird code
                        var definition2 = def.GetBlocksDefinition(temp);
                        if (definition2 != null)
                        {
                            return !IsThisBlockAtMaxLimit(blockPairName, definition2, definition2.blocks.FirstOrDefault(x => x.BlockPairName.Equals(id.BlockPairName)));
                        }
                        else
                        {
                        //    ShipClassPlugin.Log.Info("DEFINITION IS NULL 2");
                            return true;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

    }
}
