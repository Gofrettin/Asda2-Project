﻿using WCell.Constants;
using WCell.Constants.Items;

namespace WCell.Core.DBC
{
  public class CharStartOutfitConverter : AdvancedDBCRecordConverter<CharStartOutfit>
  {
    public override CharStartOutfit ConvertTo(byte[] rawData, ref int id)
    {
      id = GetInt32(rawData, 0);
      int currIndex = 0;
      CharStartOutfit outfit = default(CharStartOutfit);
      outfit.Id = GetUInt32(rawData, currIndex++);
      uint temp = GetUInt32(rawData, currIndex++);
      outfit.Race = (RaceId) (temp & 255u);
      outfit.Class = (ClassId) ((temp & 65280u) >> 8);
      outfit.Gender = (GenderType) ((temp & 16711680u) >> 16);
      for(int i = 0; i < 12; i++)
      {
        outfit.ItemIds[i] = GetUInt32(rawData, currIndex++);
      }

      currIndex += 12;
      for(int i = 0; i < 12; i++)
      {
        outfit.ItemSlots[i] = (InventorySlotType) GetUInt32(rawData, currIndex++);
      }

      return outfit;
    }
  }
}