﻿namespace WCell.Util.Graphics
{
  public interface IPackedVector
  {
    void PackFromVector4(Vector4 vector);

    Vector4 ToVector4();
  }
}