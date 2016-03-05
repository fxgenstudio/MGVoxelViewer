// MIT License - Copyright (C) FxGen 
// This file is subject to the terms and conditions defined in 
// file 'LICENSE.txt', which is part of this source code package. 

using Microsoft.Xna.Framework;
using System;

namespace FxVoxelUtils
{
  public class ColorHelpers
  {
    public static void ColorToHSV(Color color, out float hue, out float saturation, out float value)
    {
      //H(0-360) S(0-1) V(0-1)

      float r = color.R / 255f;
      float g = color.G / 255f;
      float b = color.B / 255f;

      float max = Math.Max(r, Math.Max(g, b));
      float min = Math.Min(r, Math.Min(g, b));

      //hue = color.GetHue();

      float delta;
      //min = MIN(r, g, b);
      //max = MAX(r, g, b);
      value = max;				// v
      delta = max - min;
      if (max != 0)
        saturation = delta / max;		// s
      else
      {
        // r = g = b = 0		// s = 0, v is undefined
        saturation = 0;
        hue = -1;
        return;
      }
      if (saturation == 0)
      {
        hue = 0;
        return;
      }

      if (r == max)
        hue = (g - b) / delta;		// between yellow & magenta
      else if (g == max)
        hue = 2 + (b - r) / delta;	// between cyan & yellow
      else
        hue = 4 + (r - g) / delta;	// between magenta & cyan
      hue *= 60;				// degrees
      if (hue < 0)
        hue += 360;



      //saturation = (max == 0) ? 0 : 1f - (1f * min / max);
      //value = max / 255f;
    }
    public static Color HSVToColor(float hue, float saturation, float value)
    {
      //H(0-360) S(0-1) V(0-1)
      int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
      double f = hue / 60 - Math.Floor(hue / 60);

      value = value * 255;
      int v = Convert.ToInt32(value);
      int p = Convert.ToInt32(value * (1 - saturation));
      int q = Convert.ToInt32(value * (1 - f * saturation));
      int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

      if (hi == 0)
        return Color.FromNonPremultiplied(v, t, p, 255);
      else if (hi == 1)
        return Color.FromNonPremultiplied(q, v, p, 255);
      else if (hi == 2)
        return Color.FromNonPremultiplied(p, v, t, 255);
      else if (hi == 3)
        return Color.FromNonPremultiplied(p, q, v, 255);
      else if (hi == 4)
        return Color.FromNonPremultiplied(t, p, v, 255);
      else
        return Color.FromNonPremultiplied(v, p, q, 255);
    }
    public static Color HSLToColor(float h, float s, float l)
    {
      if (s == 0)
      {
        // achromatic color (gray scale)
        return new Color(
            Convert.ToInt32(l * 255.0),
            Convert.ToInt32(l * 255.0),
            Convert.ToInt32(l * 255.0)
            );
      }
      else
      {
        float q = (l < 0.5f) ? (l * (1.0f+ s)) : (l + s - (l * s));
        float p = (2.0f * l) - q;

        float Hk = h / 360.0f;
        float[] T = new float[3];
        T[0] = Hk + (1.0f / 3.0f);    // Tr
        T[1] = Hk;                // Tb
        T[2] = Hk - (1.0f / 3.0f);    // Tg

        for (int i = 0; i < 3; i++)
        {
          if (T[i] < 0) T[i] += 1.0f;
          if (T[i] > 1) T[i] -= 1.0f;

          if ((T[i] * 6) < 1)
          {
            T[i] = p + ((q - p) * 6.0f * T[i]);
          }
          else if ((T[i] * 2.0) < 1) //(1.0/6.0)<=T[i] && T[i]<0.5
          {
            T[i] = q;
          }
          else if ((T[i] * 3.0) < 2) // 0.5<=T[i] && T[i]<(2.0/3.0)
          {
            T[i] = p + (q - p) * ((2.0f / 3.0f) - T[i]) * 6.0f;
          }
          else T[i] = p;
        }

        return new Color(
            Convert.ToInt32(T[0] * 255.0),
            Convert.ToInt32(T[1] * 255.0),
            Convert.ToInt32(T[2] * 255.0)
            );
      }
    }

  }
}
