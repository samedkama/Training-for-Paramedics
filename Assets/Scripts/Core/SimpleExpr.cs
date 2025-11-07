using System;
using System.Collections.Generic;
using System.Globalization;

namespace VR.Triage.Core
{
    public static class SimpleExpr
    {
        public static bool EvaluateBool(string expr, IDictionary<string, object> state)
        {
            if (string.IsNullOrWhiteSpace(expr)) return true;
            string[] orParts = expr.Split(new[] { "||" }, StringSplitOptions.None);
            foreach (var orPart in orParts)
            {
                bool andOk = true;
                string[] andParts = orPart.Split(new[] { "&&" }, StringSplitOptions.None);
                foreach (var p in andParts)
                {
                    if (!EvalComparison(p.Trim(), state)) { andOk = false; break; }
                }
                if (andOk) return true;
            }
            return false;
        }

        static bool EvalComparison(string s, IDictionary<string, object> state)
        {
            string[] ops = new[] { "==", "!=", "<=", ">=", "<", ">" };
            foreach (var op in ops)
            {
                int i = s.IndexOf(op, StringComparison.Ordinal);
                if (i > 0)
                {
                    var left = s.Substring(0, i).Trim().Replace("state.", "");
                    var right = s.Substring(i + op.Length).Trim().Trim('"');
                    var lVal = Resolve(left, state);
                    var rVal = ParseValue(right);
                    int cmp = Compare(lVal, rVal);
                    return op switch
                    {
                        "==" => cmp == 0,
                        "!=" => cmp != 0,
                        "<" => cmp < 0,
                        ">" => cmp > 0,
                        "<=" => cmp <= 0,
                        ">=" => cmp >= 0,
                        _ => false
                    };
                }
            }
            var v = Resolve(s.Replace("state.", "").Trim(), state);
            return ToBool(v);
        }

        static object Resolve(string key, IDictionary<string, object> state)
            => state != null && state.TryGetValue(key, out var v) ? v : null;

        static object ParseValue(string raw)
        {
            if (bool.TryParse(raw, out var b)) return b;
            if (double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
            return raw;
        }

        static int Compare(object a, object b)
        {
            if (a is null && b is null) return 0;
            if (a is null) return -1;
            if (b is null) return 1;

            if (a is bool ba && b is bool bb) return ba == bb ? 0 : (ba ? 1 : -1);
            if (a is IConvertible && b is IConvertible)
            {
                try
                {
                    double da = Convert.ToDouble(a, CultureInfo.InvariantCulture);
                    double db = Convert.ToDouble(b, CultureInfo.InvariantCulture);
                    return da.CompareTo(db);
                }
                catch { }
            }
            return string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal);
        }

        static bool ToBool(object v)
        {
            if (v is null) return false;
            if (v is bool b) return b;
            if (v is double d) return Math.Abs(d) > 1e-9;
            return !string.IsNullOrEmpty(v.ToString());
        }
    }
}
