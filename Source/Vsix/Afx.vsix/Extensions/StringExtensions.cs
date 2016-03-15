using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Afx.vsix.Extensions
{
  static class StringExtensions
  {
    public static string Or(this string text, string alternative)
    {
      return string.IsNullOrWhiteSpace(text) ? alternative : text;
    }
  }
}
