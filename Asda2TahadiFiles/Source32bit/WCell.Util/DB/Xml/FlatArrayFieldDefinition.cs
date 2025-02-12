﻿using System.Collections.Generic;
using WCell.Util.Data;

namespace WCell.Util.DB.Xml
{
  /// <summary>Flat array</summary>
  public class FlatArrayFieldDefinition : BaseFieldArrayDefinition, IFlatField, IDataFieldDefinition, IArray
  {
    public SimpleFlatFieldDefinition[] GetColumns(int length)
    {
      Column[] expliciteColumns = ExpliciteColumns;
      if(Pattern == null && expliciteColumns == null)
        throw new DataHolderException(
          "Array-field \"{0}\" had no Pattern NOR an explicit set of Columns - Make sure either of them are set and valid.",
          this);
      if(Pattern != null && expliciteColumns != null)
        throw new DataHolderException(
          "Array-field \"{0}\" defined Pattern AND an explicit set of Columns - Make sure to only specify one.",
          (object) this);
      if(Pattern != null)
      {
        List<SimpleFlatFieldDefinition> flatFieldDefinitionList = new List<SimpleFlatFieldDefinition>();
        for(int offset = Offset; offset < Offset + length; ++offset)
          flatFieldDefinitionList.Add(new SimpleFlatFieldDefinition(Table,
            Patterns.Compile(Pattern, offset)));
        return flatFieldDefinitionList.ToArray();
      }

      length = expliciteColumns.Length;
      SimpleFlatFieldDefinition[] flatFieldDefinitionArray = new SimpleFlatFieldDefinition[length];
      for(int index = 0; index < length; ++index)
        flatFieldDefinitionArray[index] =
          new SimpleFlatFieldDefinition(expliciteColumns[index].Table, expliciteColumns[index].Name);
      return flatFieldDefinitionArray;
    }

    public override DataFieldType DataFieldType
    {
      get { return DataFieldType.FlatArray; }
    }
  }
}