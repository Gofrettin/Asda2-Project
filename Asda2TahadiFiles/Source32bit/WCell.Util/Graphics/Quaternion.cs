﻿using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace WCell.Util.Graphics
{
  [StructLayout(LayoutKind.Sequential, Size = 16)]
  public struct Quaternion : IEquatable<Quaternion>
  {
    private static Quaternion _identity = new Quaternion(0.0f, 0.0f, 0.0f, 1f);
    public float X;
    public float Y;
    public float Z;
    public float W;

    public static Quaternion Identity
    {
      get { return _identity; }
    }

    public Quaternion(float x, float y, float z, float w)
    {
      X = x;
      Y = y;
      Z = z;
      W = w;
    }

    public Quaternion(Vector3 vectorPart, float scalarPart)
    {
      X = vectorPart.X;
      Y = vectorPart.Y;
      Z = vectorPart.Z;
      W = scalarPart;
    }

    public override string ToString()
    {
      CultureInfo currentCulture = CultureInfo.CurrentCulture;
      return string.Format(currentCulture, "{{X:{0} Y:{1} Z:{2} W:{3}}}",
        (object) X.ToString(currentCulture),
        (object) Y.ToString(currentCulture),
        (object) Z.ToString(currentCulture),
        (object) W.ToString(currentCulture));
    }

    public bool Equals(Quaternion other)
    {
      return X == (double) other.X && Y == (double) other.Y &&
             Z == (double) other.Z && W == (double) other.W;
    }

    public override bool Equals(object obj)
    {
      bool flag = false;
      if(obj is Quaternion)
        flag = Equals((Quaternion) obj);
      return flag;
    }

    public override int GetHashCode()
    {
      return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode() + W.GetHashCode();
    }

    public float LengthSquared()
    {
      return (float) (X * (double) X + Y * (double) Y +
                      Z * (double) Z + W * (double) W);
    }

    public float Length()
    {
      return (float) Math.Sqrt(X * (double) X + Y * (double) Y +
                               Z * (double) Z + W * (double) W);
    }

    public void Normalize()
    {
      float num = 1f / (float) Math.Sqrt(X * (double) X + Y * (double) Y +
                                         Z * (double) Z + W * (double) W);
      X *= num;
      Y *= num;
      Z *= num;
      W *= num;
    }

    public static Quaternion Normalize(Quaternion quaternion)
    {
      float num = 1f / (float) Math.Sqrt(quaternion.X * (double) quaternion.X +
                                         quaternion.Y * (double) quaternion.Y +
                                         quaternion.Z * (double) quaternion.Z +
                                         quaternion.W * (double) quaternion.W);
      Quaternion quaternion1;
      quaternion1.X = quaternion.X * num;
      quaternion1.Y = quaternion.Y * num;
      quaternion1.Z = quaternion.Z * num;
      quaternion1.W = quaternion.W * num;
      return quaternion1;
    }

    public static void Normalize(ref Quaternion quaternion, out Quaternion result)
    {
      float num = 1f / (float) Math.Sqrt(quaternion.X * (double) quaternion.X +
                                         quaternion.Y * (double) quaternion.Y +
                                         quaternion.Z * (double) quaternion.Z +
                                         quaternion.W * (double) quaternion.W);
      result.X = quaternion.X * num;
      result.Y = quaternion.Y * num;
      result.Z = quaternion.Z * num;
      result.W = quaternion.W * num;
    }

    public void Conjugate()
    {
      X = -X;
      Y = -Y;
      Z = -Z;
    }

    public static Quaternion Conjugate(Quaternion value)
    {
      Quaternion quaternion;
      quaternion.X = -value.X;
      quaternion.Y = -value.Y;
      quaternion.Z = -value.Z;
      quaternion.W = value.W;
      return quaternion;
    }

    public static void Conjugate(ref Quaternion value, out Quaternion result)
    {
      result.X = -value.X;
      result.Y = -value.Y;
      result.Z = -value.Z;
      result.W = value.W;
    }

    public static Quaternion Inverse(Quaternion quaternion)
    {
      float num = 1f / (float) (quaternion.X * (double) quaternion.X +
                                quaternion.Y * (double) quaternion.Y +
                                quaternion.Z * (double) quaternion.Z +
                                quaternion.W * (double) quaternion.W);
      Quaternion quaternion1;
      quaternion1.X = -quaternion.X * num;
      quaternion1.Y = -quaternion.Y * num;
      quaternion1.Z = -quaternion.Z * num;
      quaternion1.W = quaternion.W * num;
      return quaternion1;
    }

    public static void Inverse(ref Quaternion quaternion, out Quaternion result)
    {
      float num = 1f / (float) (quaternion.X * (double) quaternion.X +
                                quaternion.Y * (double) quaternion.Y +
                                quaternion.Z * (double) quaternion.Z +
                                quaternion.W * (double) quaternion.W);
      result.X = -quaternion.X * num;
      result.Y = -quaternion.Y * num;
      result.Z = -quaternion.Z * num;
      result.W = quaternion.W * num;
    }

    public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
    {
      float num1 = angle * 0.5f;
      float num2 = (float) Math.Sin(num1);
      float num3 = (float) Math.Cos(num1);
      Quaternion quaternion;
      quaternion.X = axis.X * num2;
      quaternion.Y = axis.Y * num2;
      quaternion.Z = axis.Z * num2;
      quaternion.W = num3;
      return quaternion;
    }

    public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Quaternion result)
    {
      float num1 = angle * 0.5f;
      float num2 = (float) Math.Sin(num1);
      float num3 = (float) Math.Cos(num1);
      result.X = axis.X * num2;
      result.Y = axis.Y * num2;
      result.Z = axis.Z * num2;
      result.W = num3;
    }

    public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
    {
      float num1 = roll * 0.5f;
      float num2 = (float) Math.Sin(num1);
      float num3 = (float) Math.Cos(num1);
      float num4 = pitch * 0.5f;
      float num5 = (float) Math.Sin(num4);
      float num6 = (float) Math.Cos(num4);
      float num7 = yaw * 0.5f;
      float num8 = (float) Math.Sin(num7);
      float num9 = (float) Math.Cos(num7);
      Quaternion quaternion;
      quaternion.X = (float) (num9 * (double) num5 * num3 +
                              num8 * (double) num6 * num2);
      quaternion.Y = (float) (num8 * (double) num6 * num3 -
                              num9 * (double) num5 * num2);
      quaternion.Z = (float) (num9 * (double) num6 * num2 -
                              num8 * (double) num5 * num3);
      quaternion.W = (float) (num9 * (double) num6 * num3 +
                              num8 * (double) num5 * num2);
      return quaternion;
    }

    public static void CreateFromYawPitchRoll(float yaw, float pitch, float roll, out Quaternion result)
    {
      float num1 = roll * 0.5f;
      float num2 = (float) Math.Sin(num1);
      float num3 = (float) Math.Cos(num1);
      float num4 = pitch * 0.5f;
      float num5 = (float) Math.Sin(num4);
      float num6 = (float) Math.Cos(num4);
      float num7 = yaw * 0.5f;
      float num8 = (float) Math.Sin(num7);
      float num9 = (float) Math.Cos(num7);
      result.X = (float) (num9 * (double) num5 * num3 +
                          num8 * (double) num6 * num2);
      result.Y = (float) (num8 * (double) num6 * num3 -
                          num9 * (double) num5 * num2);
      result.Z = (float) (num9 * (double) num6 * num2 -
                          num8 * (double) num5 * num3);
      result.W = (float) (num9 * (double) num6 * num3 +
                          num8 * (double) num5 * num2);
    }

    public static Quaternion CreateFromRotationMatrix(Matrix matrix)
    {
      float num1 = matrix.M11 + matrix.M22 + matrix.M33;
      Quaternion quaternion = new Quaternion();
      if(num1 > 0.0)
      {
        float num2 = (float) Math.Sqrt(num1 + 1.0);
        quaternion.W = num2 * 0.5f;
        float num3 = 0.5f / num2;
        quaternion.X = (matrix.M23 - matrix.M32) * num3;
        quaternion.Y = (matrix.M31 - matrix.M13) * num3;
        quaternion.Z = (matrix.M12 - matrix.M21) * num3;
        return quaternion;
      }

      if(matrix.M11 >= (double) matrix.M22 && matrix.M11 >= (double) matrix.M33)
      {
        float num2 = (float) Math.Sqrt(1.0 + matrix.M11 - matrix.M22 - matrix.M33);
        float num3 = 0.5f / num2;
        quaternion.X = 0.5f * num2;
        quaternion.Y = (matrix.M12 + matrix.M21) * num3;
        quaternion.Z = (matrix.M13 + matrix.M31) * num3;
        quaternion.W = (matrix.M23 - matrix.M32) * num3;
        return quaternion;
      }

      if(matrix.M22 > (double) matrix.M33)
      {
        float num2 = (float) Math.Sqrt(1.0 + matrix.M22 - matrix.M11 - matrix.M33);
        float num3 = 0.5f / num2;
        quaternion.X = (matrix.M21 + matrix.M12) * num3;
        quaternion.Y = 0.5f * num2;
        quaternion.Z = (matrix.M32 + matrix.M23) * num3;
        quaternion.W = (matrix.M31 - matrix.M13) * num3;
        return quaternion;
      }

      float num4 = (float) Math.Sqrt(1.0 + matrix.M33 - matrix.M11 - matrix.M22);
      float num5 = 0.5f / num4;
      quaternion.X = (matrix.M31 + matrix.M13) * num5;
      quaternion.Y = (matrix.M32 + matrix.M23) * num5;
      quaternion.Z = 0.5f * num4;
      quaternion.W = (matrix.M12 - matrix.M21) * num5;
      return quaternion;
    }

    public static void CreateFromRotationMatrix(ref Matrix matrix, out Quaternion result)
    {
      float num1 = matrix.M11 + matrix.M22 + matrix.M33;
      if(num1 > 0.0)
      {
        float num2 = (float) Math.Sqrt(num1 + 1.0);
        result.W = num2 * 0.5f;
        float num3 = 0.5f / num2;
        result.X = (matrix.M23 - matrix.M32) * num3;
        result.Y = (matrix.M31 - matrix.M13) * num3;
        result.Z = (matrix.M12 - matrix.M21) * num3;
      }
      else if(matrix.M11 >= (double) matrix.M22 && matrix.M11 >= (double) matrix.M33)
      {
        float num2 = (float) Math.Sqrt(1.0 + matrix.M11 - matrix.M22 - matrix.M33);
        float num3 = 0.5f / num2;
        result.X = 0.5f * num2;
        result.Y = (matrix.M12 + matrix.M21) * num3;
        result.Z = (matrix.M13 + matrix.M31) * num3;
        result.W = (matrix.M23 - matrix.M32) * num3;
      }
      else if(matrix.M22 > (double) matrix.M33)
      {
        float num2 = (float) Math.Sqrt(1.0 + matrix.M22 - matrix.M11 - matrix.M33);
        float num3 = 0.5f / num2;
        result.X = (matrix.M21 + matrix.M12) * num3;
        result.Y = 0.5f * num2;
        result.Z = (matrix.M32 + matrix.M23) * num3;
        result.W = (matrix.M31 - matrix.M13) * num3;
      }
      else
      {
        float num2 = (float) Math.Sqrt(1.0 + matrix.M33 - matrix.M11 - matrix.M22);
        float num3 = 0.5f / num2;
        result.X = (matrix.M31 + matrix.M13) * num3;
        result.Y = (matrix.M32 + matrix.M23) * num3;
        result.Z = 0.5f * num2;
        result.W = (matrix.M12 - matrix.M21) * num3;
      }
    }

    public static float Dot(Quaternion quaternion1, Quaternion quaternion2)
    {
      return (float) (quaternion1.X * (double) quaternion2.X +
                      quaternion1.Y * (double) quaternion2.Y +
                      quaternion1.Z * (double) quaternion2.Z +
                      quaternion1.W * (double) quaternion2.W);
    }

    public static void Dot(ref Quaternion quaternion1, ref Quaternion quaternion2, out float result)
    {
      result = (float) (quaternion1.X * (double) quaternion2.X +
                        quaternion1.Y * (double) quaternion2.Y +
                        quaternion1.Z * (double) quaternion2.Z +
                        quaternion1.W * (double) quaternion2.W);
    }

    public static Quaternion Slerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
    {
      float num1 = amount;
      float num2 = (float) (quaternion1.X * (double) quaternion2.X +
                            quaternion1.Y * (double) quaternion2.Y +
                            quaternion1.Z * (double) quaternion2.Z +
                            quaternion1.W * (double) quaternion2.W);
      bool flag = false;
      if(num2 < 0.0)
      {
        flag = true;
        num2 = -num2;
      }

      float num3;
      float num4;
      if(num2 > 0.999998986721039)
      {
        num3 = 1f - num1;
        num4 = flag ? -num1 : num1;
      }
      else
      {
        float num5 = (float) Math.Acos(num2);
        float num6 = (float) (1.0 / Math.Sin(num5));
        num3 = (float) Math.Sin((1.0 - num1) * num5) * num6;
        num4 = flag
          ? (float) -Math.Sin(num1 * (double) num5) * num6
          : (float) Math.Sin(num1 * (double) num5) * num6;
      }

      Quaternion quaternion;
      quaternion.X = (float) (num3 * (double) quaternion1.X + num4 * (double) quaternion2.X);
      quaternion.Y = (float) (num3 * (double) quaternion1.Y + num4 * (double) quaternion2.Y);
      quaternion.Z = (float) (num3 * (double) quaternion1.Z + num4 * (double) quaternion2.Z);
      quaternion.W = (float) (num3 * (double) quaternion1.W + num4 * (double) quaternion2.W);
      return quaternion;
    }

    public static void Slerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount,
      out Quaternion result)
    {
      float num1 = amount;
      float num2 = (float) (quaternion1.X * (double) quaternion2.X +
                            quaternion1.Y * (double) quaternion2.Y +
                            quaternion1.Z * (double) quaternion2.Z +
                            quaternion1.W * (double) quaternion2.W);
      bool flag = false;
      if(num2 < 0.0)
      {
        flag = true;
        num2 = -num2;
      }

      float num3;
      float num4;
      if(num2 > 0.999998986721039)
      {
        num3 = 1f - num1;
        num4 = flag ? -num1 : num1;
      }
      else
      {
        float num5 = (float) Math.Acos(num2);
        float num6 = (float) (1.0 / Math.Sin(num5));
        num3 = (float) Math.Sin((1.0 - num1) * num5) * num6;
        num4 = flag
          ? (float) -Math.Sin(num1 * (double) num5) * num6
          : (float) Math.Sin(num1 * (double) num5) * num6;
      }

      result.X = (float) (num3 * (double) quaternion1.X + num4 * (double) quaternion2.X);
      result.Y = (float) (num3 * (double) quaternion1.Y + num4 * (double) quaternion2.Y);
      result.Z = (float) (num3 * (double) quaternion1.Z + num4 * (double) quaternion2.Z);
      result.W = (float) (num3 * (double) quaternion1.W + num4 * (double) quaternion2.W);
    }

    public static Quaternion Lerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
    {
      float num1 = amount;
      float num2 = 1f - num1;
      Quaternion quaternion = new Quaternion();
      if(quaternion1.X * (double) quaternion2.X + quaternion1.Y * (double) quaternion2.Y +
         quaternion1.Z * (double) quaternion2.Z + quaternion1.W * (double) quaternion2.W >=
         0.0)
      {
        quaternion.X =
          (float) (num2 * (double) quaternion1.X + num1 * (double) quaternion2.X);
        quaternion.Y =
          (float) (num2 * (double) quaternion1.Y + num1 * (double) quaternion2.Y);
        quaternion.Z =
          (float) (num2 * (double) quaternion1.Z + num1 * (double) quaternion2.Z);
        quaternion.W =
          (float) (num2 * (double) quaternion1.W + num1 * (double) quaternion2.W);
      }
      else
      {
        quaternion.X =
          (float) (num2 * (double) quaternion1.X - num1 * (double) quaternion2.X);
        quaternion.Y =
          (float) (num2 * (double) quaternion1.Y - num1 * (double) quaternion2.Y);
        quaternion.Z =
          (float) (num2 * (double) quaternion1.Z - num1 * (double) quaternion2.Z);
        quaternion.W =
          (float) (num2 * (double) quaternion1.W - num1 * (double) quaternion2.W);
      }

      float num3 = 1f / (float) Math.Sqrt(quaternion.X * (double) quaternion.X +
                                          quaternion.Y * (double) quaternion.Y +
                                          quaternion.Z * (double) quaternion.Z +
                                          quaternion.W * (double) quaternion.W);
      quaternion.X *= num3;
      quaternion.Y *= num3;
      quaternion.Z *= num3;
      quaternion.W *= num3;
      return quaternion;
    }

    public static void Lerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount,
      out Quaternion result)
    {
      float num1 = amount;
      float num2 = 1f - num1;
      if(quaternion1.X * (double) quaternion2.X + quaternion1.Y * (double) quaternion2.Y +
         quaternion1.Z * (double) quaternion2.Z + quaternion1.W * (double) quaternion2.W >=
         0.0)
      {
        result.X = (float) (num2 * (double) quaternion1.X + num1 * (double) quaternion2.X);
        result.Y = (float) (num2 * (double) quaternion1.Y + num1 * (double) quaternion2.Y);
        result.Z = (float) (num2 * (double) quaternion1.Z + num1 * (double) quaternion2.Z);
        result.W = (float) (num2 * (double) quaternion1.W + num1 * (double) quaternion2.W);
      }
      else
      {
        result.X = (float) (num2 * (double) quaternion1.X - num1 * (double) quaternion2.X);
        result.Y = (float) (num2 * (double) quaternion1.Y - num1 * (double) quaternion2.Y);
        result.Z = (float) (num2 * (double) quaternion1.Z - num1 * (double) quaternion2.Z);
        result.W = (float) (num2 * (double) quaternion1.W - num1 * (double) quaternion2.W);
      }

      float num3 = 1f / (float) Math.Sqrt(result.X * (double) result.X +
                                          result.Y * (double) result.Y +
                                          result.Z * (double) result.Z +
                                          result.W * (double) result.W);
      result.X *= num3;
      result.Y *= num3;
      result.Z *= num3;
      result.W *= num3;
    }

    public static Quaternion Concatenate(Quaternion value1, Quaternion value2)
    {
      float x1 = value2.X;
      float y1 = value2.Y;
      float z1 = value2.Z;
      float w1 = value2.W;
      float x2 = value1.X;
      float y2 = value1.Y;
      float z2 = value1.Z;
      float w2 = value1.W;
      float num1 = (float) (y1 * (double) z2 - z1 * (double) y2);
      float num2 = (float) (z1 * (double) x2 - x1 * (double) z2);
      float num3 = (float) (x1 * (double) y2 - y1 * (double) x2);
      float num4 = (float) (x1 * (double) x2 + y1 * (double) y2 + z1 * (double) z2);
      Quaternion quaternion;
      quaternion.X = (float) (x1 * (double) w2 + x2 * (double) w1) + num1;
      quaternion.Y = (float) (y1 * (double) w2 + y2 * (double) w1) + num2;
      quaternion.Z = (float) (z1 * (double) w2 + z2 * (double) w1) + num3;
      quaternion.W = w1 * w2 - num4;
      return quaternion;
    }

    public static void Concatenate(ref Quaternion value1, ref Quaternion value2, out Quaternion result)
    {
      float x1 = value2.X;
      float y1 = value2.Y;
      float z1 = value2.Z;
      float w1 = value2.W;
      float x2 = value1.X;
      float y2 = value1.Y;
      float z2 = value1.Z;
      float w2 = value1.W;
      float num1 = (float) (y1 * (double) z2 - z1 * (double) y2);
      float num2 = (float) (z1 * (double) x2 - x1 * (double) z2);
      float num3 = (float) (x1 * (double) y2 - y1 * (double) x2);
      float num4 = (float) (x1 * (double) x2 + y1 * (double) y2 + z1 * (double) z2);
      result.X = (float) (x1 * (double) w2 + x2 * (double) w1) + num1;
      result.Y = (float) (y1 * (double) w2 + y2 * (double) w1) + num2;
      result.Z = (float) (z1 * (double) w2 + z2 * (double) w1) + num3;
      result.W = w1 * w2 - num4;
    }

    public static Quaternion Negate(Quaternion quaternion)
    {
      Quaternion quaternion1;
      quaternion1.X = -quaternion.X;
      quaternion1.Y = -quaternion.Y;
      quaternion1.Z = -quaternion.Z;
      quaternion1.W = -quaternion.W;
      return quaternion1;
    }

    public static void Negate(ref Quaternion quaternion, out Quaternion result)
    {
      result.X = -quaternion.X;
      result.Y = -quaternion.Y;
      result.Z = -quaternion.Z;
      result.W = -quaternion.W;
    }

    public static Quaternion Add(Quaternion quaternion1, Quaternion quaternion2)
    {
      Quaternion quaternion;
      quaternion.X = quaternion1.X + quaternion2.X;
      quaternion.Y = quaternion1.Y + quaternion2.Y;
      quaternion.Z = quaternion1.Z + quaternion2.Z;
      quaternion.W = quaternion1.W + quaternion2.W;
      return quaternion;
    }

    public static void Add(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
    {
      result.X = quaternion1.X + quaternion2.X;
      result.Y = quaternion1.Y + quaternion2.Y;
      result.Z = quaternion1.Z + quaternion2.Z;
      result.W = quaternion1.W + quaternion2.W;
    }

    public static Quaternion Subtract(Quaternion quaternion1, Quaternion quaternion2)
    {
      Quaternion quaternion;
      quaternion.X = quaternion1.X - quaternion2.X;
      quaternion.Y = quaternion1.Y - quaternion2.Y;
      quaternion.Z = quaternion1.Z - quaternion2.Z;
      quaternion.W = quaternion1.W - quaternion2.W;
      return quaternion;
    }

    public static void Subtract(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
    {
      result.X = quaternion1.X - quaternion2.X;
      result.Y = quaternion1.Y - quaternion2.Y;
      result.Z = quaternion1.Z - quaternion2.Z;
      result.W = quaternion1.W - quaternion2.W;
    }

    public static Quaternion Multiply(Quaternion quaternion1, Quaternion quaternion2)
    {
      float x1 = quaternion1.X;
      float y1 = quaternion1.Y;
      float z1 = quaternion1.Z;
      float w1 = quaternion1.W;
      float x2 = quaternion2.X;
      float y2 = quaternion2.Y;
      float z2 = quaternion2.Z;
      float w2 = quaternion2.W;
      float num1 = (float) (y1 * (double) z2 - z1 * (double) y2);
      float num2 = (float) (z1 * (double) x2 - x1 * (double) z2);
      float num3 = (float) (x1 * (double) y2 - y1 * (double) x2);
      float num4 = (float) (x1 * (double) x2 + y1 * (double) y2 + z1 * (double) z2);
      Quaternion quaternion;
      quaternion.X = (float) (x1 * (double) w2 + x2 * (double) w1) + num1;
      quaternion.Y = (float) (y1 * (double) w2 + y2 * (double) w1) + num2;
      quaternion.Z = (float) (z1 * (double) w2 + z2 * (double) w1) + num3;
      quaternion.W = w1 * w2 - num4;
      return quaternion;
    }

    public static void Multiply(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
    {
      float x1 = quaternion1.X;
      float y1 = quaternion1.Y;
      float z1 = quaternion1.Z;
      float w1 = quaternion1.W;
      float x2 = quaternion2.X;
      float y2 = quaternion2.Y;
      float z2 = quaternion2.Z;
      float w2 = quaternion2.W;
      float num1 = (float) (y1 * (double) z2 - z1 * (double) y2);
      float num2 = (float) (z1 * (double) x2 - x1 * (double) z2);
      float num3 = (float) (x1 * (double) y2 - y1 * (double) x2);
      float num4 = (float) (x1 * (double) x2 + y1 * (double) y2 + z1 * (double) z2);
      result.X = (float) (x1 * (double) w2 + x2 * (double) w1) + num1;
      result.Y = (float) (y1 * (double) w2 + y2 * (double) w1) + num2;
      result.Z = (float) (z1 * (double) w2 + z2 * (double) w1) + num3;
      result.W = w1 * w2 - num4;
    }

    public static Quaternion Multiply(Quaternion quaternion1, float scaleFactor)
    {
      Quaternion quaternion;
      quaternion.X = quaternion1.X * scaleFactor;
      quaternion.Y = quaternion1.Y * scaleFactor;
      quaternion.Z = quaternion1.Z * scaleFactor;
      quaternion.W = quaternion1.W * scaleFactor;
      return quaternion;
    }

    public static void Multiply(ref Quaternion quaternion1, float scaleFactor, out Quaternion result)
    {
      result.X = quaternion1.X * scaleFactor;
      result.Y = quaternion1.Y * scaleFactor;
      result.Z = quaternion1.Z * scaleFactor;
      result.W = quaternion1.W * scaleFactor;
    }

    public static Quaternion Divide(Quaternion quaternion1, Quaternion quaternion2)
    {
      float x = quaternion1.X;
      float y = quaternion1.Y;
      float z = quaternion1.Z;
      float w = quaternion1.W;
      float num1 = 1f / (float) (quaternion2.X * (double) quaternion2.X +
                                 quaternion2.Y * (double) quaternion2.Y +
                                 quaternion2.Z * (double) quaternion2.Z +
                                 quaternion2.W * (double) quaternion2.W);
      float num2 = -quaternion2.X * num1;
      float num3 = -quaternion2.Y * num1;
      float num4 = -quaternion2.Z * num1;
      float num5 = quaternion2.W * num1;
      float num6 = (float) (y * (double) num4 - z * (double) num3);
      float num7 = (float) (z * (double) num2 - x * (double) num4);
      float num8 = (float) (x * (double) num3 - y * (double) num2);
      float num9 = (float) (x * (double) num2 + y * (double) num3 + z * (double) num4);
      Quaternion quaternion;
      quaternion.X = (float) (x * (double) num5 + num2 * (double) w) + num6;
      quaternion.Y = (float) (y * (double) num5 + num3 * (double) w) + num7;
      quaternion.Z = (float) (z * (double) num5 + num4 * (double) w) + num8;
      quaternion.W = w * num5 - num9;
      return quaternion;
    }

    public static void Divide(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
    {
      float x = quaternion1.X;
      float y = quaternion1.Y;
      float z = quaternion1.Z;
      float w = quaternion1.W;
      float num1 = 1f / (float) (quaternion2.X * (double) quaternion2.X +
                                 quaternion2.Y * (double) quaternion2.Y +
                                 quaternion2.Z * (double) quaternion2.Z +
                                 quaternion2.W * (double) quaternion2.W);
      float num2 = -quaternion2.X * num1;
      float num3 = -quaternion2.Y * num1;
      float num4 = -quaternion2.Z * num1;
      float num5 = quaternion2.W * num1;
      float num6 = (float) (y * (double) num4 - z * (double) num3);
      float num7 = (float) (z * (double) num2 - x * (double) num4);
      float num8 = (float) (x * (double) num3 - y * (double) num2);
      float num9 = (float) (x * (double) num2 + y * (double) num3 + z * (double) num4);
      result.X = (float) (x * (double) num5 + num2 * (double) w) + num6;
      result.Y = (float) (y * (double) num5 + num3 * (double) w) + num7;
      result.Z = (float) (z * (double) num5 + num4 * (double) w) + num8;
      result.W = w * num5 - num9;
    }

    public static Quaternion operator -(Quaternion quaternion)
    {
      Quaternion quaternion1;
      quaternion1.X = -quaternion.X;
      quaternion1.Y = -quaternion.Y;
      quaternion1.Z = -quaternion.Z;
      quaternion1.W = -quaternion.W;
      return quaternion1;
    }

    public static bool operator ==(Quaternion quaternion1, Quaternion quaternion2)
    {
      return quaternion1.X == (double) quaternion2.X &&
             quaternion1.Y == (double) quaternion2.Y &&
             quaternion1.Z == (double) quaternion2.Z && quaternion1.W == (double) quaternion2.W;
    }

    public static bool operator !=(Quaternion quaternion1, Quaternion quaternion2)
    {
      if(quaternion1.X == (double) quaternion2.X && quaternion1.Y == (double) quaternion2.Y &&
         quaternion1.Z == (double) quaternion2.Z)
        return quaternion1.W != (double) quaternion2.W;
      return true;
    }

    public static Quaternion operator +(Quaternion quaternion1, Quaternion quaternion2)
    {
      Quaternion quaternion;
      quaternion.X = quaternion1.X + quaternion2.X;
      quaternion.Y = quaternion1.Y + quaternion2.Y;
      quaternion.Z = quaternion1.Z + quaternion2.Z;
      quaternion.W = quaternion1.W + quaternion2.W;
      return quaternion;
    }

    public static Quaternion operator -(Quaternion quaternion1, Quaternion quaternion2)
    {
      Quaternion quaternion;
      quaternion.X = quaternion1.X - quaternion2.X;
      quaternion.Y = quaternion1.Y - quaternion2.Y;
      quaternion.Z = quaternion1.Z - quaternion2.Z;
      quaternion.W = quaternion1.W - quaternion2.W;
      return quaternion;
    }

    public static Quaternion operator *(Quaternion quaternion1, Quaternion quaternion2)
    {
      float x1 = quaternion1.X;
      float y1 = quaternion1.Y;
      float z1 = quaternion1.Z;
      float w1 = quaternion1.W;
      float x2 = quaternion2.X;
      float y2 = quaternion2.Y;
      float z2 = quaternion2.Z;
      float w2 = quaternion2.W;
      float num1 = (float) (y1 * (double) z2 - z1 * (double) y2);
      float num2 = (float) (z1 * (double) x2 - x1 * (double) z2);
      float num3 = (float) (x1 * (double) y2 - y1 * (double) x2);
      float num4 = (float) (x1 * (double) x2 + y1 * (double) y2 + z1 * (double) z2);
      Quaternion quaternion;
      quaternion.X = (float) (x1 * (double) w2 + x2 * (double) w1) + num1;
      quaternion.Y = (float) (y1 * (double) w2 + y2 * (double) w1) + num2;
      quaternion.Z = (float) (z1 * (double) w2 + z2 * (double) w1) + num3;
      quaternion.W = w1 * w2 - num4;
      return quaternion;
    }

    public static Quaternion operator *(Quaternion quaternion1, float scaleFactor)
    {
      Quaternion quaternion;
      quaternion.X = quaternion1.X * scaleFactor;
      quaternion.Y = quaternion1.Y * scaleFactor;
      quaternion.Z = quaternion1.Z * scaleFactor;
      quaternion.W = quaternion1.W * scaleFactor;
      return quaternion;
    }

    public static Quaternion operator /(Quaternion quaternion1, Quaternion quaternion2)
    {
      float x = quaternion1.X;
      float y = quaternion1.Y;
      float z = quaternion1.Z;
      float w = quaternion1.W;
      float num1 = 1f / (float) (quaternion2.X * (double) quaternion2.X +
                                 quaternion2.Y * (double) quaternion2.Y +
                                 quaternion2.Z * (double) quaternion2.Z +
                                 quaternion2.W * (double) quaternion2.W);
      float num2 = -quaternion2.X * num1;
      float num3 = -quaternion2.Y * num1;
      float num4 = -quaternion2.Z * num1;
      float num5 = quaternion2.W * num1;
      float num6 = (float) (y * (double) num4 - z * (double) num3);
      float num7 = (float) (z * (double) num2 - x * (double) num4);
      float num8 = (float) (x * (double) num3 - y * (double) num2);
      float num9 = (float) (x * (double) num2 + y * (double) num3 + z * (double) num4);
      Quaternion quaternion;
      quaternion.X = (float) (x * (double) num5 + num2 * (double) w) + num6;
      quaternion.Y = (float) (y * (double) num5 + num3 * (double) w) + num7;
      quaternion.Z = (float) (z * (double) num5 + num4 * (double) w) + num8;
      quaternion.W = w * num5 - num9;
      return quaternion;
    }
  }
}