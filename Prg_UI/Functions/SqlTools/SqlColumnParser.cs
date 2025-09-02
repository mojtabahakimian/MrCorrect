namespace Prg_UI.Functions.SqlTools
{
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class SqlColumnParser
    {
        public static List<string> ExtractColumnNames(string sql)
        {
            var cols = new List<string>();
            if (string.IsNullOrWhiteSpace(sql)) return cols;

            // 1) remove comments safely (keep quoted/bracketed text)
            string cleaned = RemoveComments(sql);

            // 2) find main SELECT and its projection range
            if (!TryGetMainSelectProjection(cleaned, out var projection))
                return cols; // no SELECT found

            // 3) split select list by top-level commas
            var items = SplitByTopLevelCommas(projection);

            // 4) extract alias/name from each item
            foreach (var item in items)
            {
                var name = ExtractName(item);
                if (!string.IsNullOrWhiteSpace(name))
                    cols.Add(name);
            }
            return cols;
        }

        // ---------------- core: locate SELECT list ----------------

        private static bool TryGetMainSelectProjection(string sql, out string projection)
        {
            projection = string.Empty;

            // find first top-level SELECT (CTE inner SELECTs are inside parentheses -> depth>0)
            if (!TryFindTopLevelKeyword(sql, "SELECT", 0, out int selectIdx))
                return false;

            int i = selectIdx + "SELECT".Length;

            // skip DISTINCT/ALL/TOP...
            i = SkipLeadingModifiers(sql, i);

            // projection ends at the first top-level FROM or INTO or ; or end
            int end = FindEndOfProjection(sql, i);

            if (end <= i) return false;

            projection = sql.Substring(i, end - i).Trim();
            return !string.IsNullOrEmpty(projection);
        }

        private static int SkipLeadingModifiers(string s, int i)
        {
            SkipWs(s, ref i);

            // DISTINCT | ALL
            if (IsWordAt(s, i, "DISTINCT") || IsWordAt(s, i, "ALL"))
            {
                i += NextWord(s, i).Length;
                SkipWs(s, ref i);
            }

            // TOP n [PERCENT] [WITH TIES]   or   TOP (n) ...
            if (IsWordAt(s, i, "TOP"))
            {
                i += 3;
                SkipWs(s, ref i);
                if (i < s.Length && s[i] == '(')
                {
                    int depth = 1; i++;
                    while (i < s.Length && depth > 0)
                    {
                        if (s[i] == '(') depth++;
                        else if (s[i] == ')') depth--;
                        else if (s[i] == '\'' && i + 1 < s.Length) i = SkipSingleQuoted(s, i);
                        i++;
                    }
                }
                else
                {
                    // digits
                    while (i < s.Length && char.IsDigit(s[i])) i++;
                }
                SkipWs(s, ref i);
                if (IsWordAt(s, i, "PERCENT"))
                {
                    i += "PERCENT".Length;
                    SkipWs(s, ref i);
                }
                if (IsWordAt(s, i, "WITH"))
                {
                    int j = i + 4; SkipWs(s, ref j);
                    if (IsWordAt(s, j, "TIES"))
                    {
                        i = j + 4;
                        SkipWs(s, ref i);
                    }
                }
            }
            return i;
        }

        private static int FindEndOfProjection(string s, int i)
        {
            bool inSq = false, inDq = false, inBr = false;
            int depth = 0;

            for (int pos = i; pos < s.Length; pos++)
            {
                char c = s[pos];
                char n = pos + 1 < s.Length ? s[pos + 1] : '\0';

                // quotes / brackets
                if (inSq) { if (c == '\'' && n == '\'') { pos++; } else if (c == '\'') inSq = false; continue; }
                if (inDq) { if (c == '"' && n == '"') { pos++; } else if (c == '"') inDq = false; continue; }
                if (inBr) { if (c == ']') inBr = false; continue; }

                if (c == '\'') { inSq = true; continue; }
                if (c == '"') { inDq = true; continue; }
                if (c == '[') { inBr = true; continue; }

                if (c == '(') { depth++; continue; }
                if (c == ')') { if (depth > 0) depth--; continue; }

                if (depth == 0)
                {
                    if (IsWordAt(s, pos, "FROM") || IsWordAt(s, pos, "INTO"))
                        return pos;
                    if (c == ';') return pos;
                }
            }
            return s.Length;
        }

        private static bool TryFindTopLevelKeyword(string s, string word, int start, out int idx)
        {
            idx = -1;
            bool inSq = false, inDq = false, inBr = false;
            int depth = 0;

            for (int i = start; i < s.Length; i++)
            {
                char c = s[i];
                char n = i + 1 < s.Length ? s[i + 1] : '\0';

                // quotes/brackets
                if (inSq) { if (c == '\'' && n == '\'') { i++; } else if (c == '\'') inSq = false; continue; }
                if (inDq) { if (c == '"' && n == '"') { i++; } else if (c == '"') inDq = false; continue; }
                if (inBr) { if (c == ']') inBr = false; continue; }

                if (c == '\'') { inSq = true; continue; }
                if (c == '"') { inDq = true; continue; }
                if (c == '[') { inBr = true; continue; }

                if (c == '(') { depth++; continue; }
                if (c == ')') { if (depth > 0) depth--; continue; }

                if (depth == 0 && IsWordAt(s, i, word))
                {
                    idx = i;
                    return true;
                }
            }
            return false;
        }

        // ---------------- splitting & extraction ----------------

        private static List<string> SplitByTopLevelCommas(string s)
        {
            var list = new List<string>();
            var sb = new StringBuilder();

            bool inSq = false, inDq = false, inBr = false;
            int depth = 0;

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                char n = i + 1 < s.Length ? s[i + 1] : '\0';

                if (inSq) { sb.Append(c); if (c == '\'' && n == '\'') { sb.Append(n); i++; } else if (c == '\'') inSq = false; continue; }
                if (inDq) { sb.Append(c); if (c == '"' && n == '"') { sb.Append(n); i++; } else if (c == '"') inDq = false; continue; }
                if (inBr) { sb.Append(c); if (c == ']') inBr = false; continue; }

                if (c == '\'') { inSq = true; sb.Append(c); continue; }
                if (c == '"') { inDq = true; sb.Append(c); continue; }
                if (c == '[') { inBr = true; sb.Append(c); continue; }

                if (c == '(') { depth++; sb.Append(c); continue; }
                if (c == ')') { if (depth > 0) depth--; sb.Append(c); continue; }

                if (c == ',' && depth == 0)
                {
                    list.Add(sb.ToString().Trim());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            if (sb.Length > 0) list.Add(sb.ToString().Trim());

            return list;
        }

        private static string ExtractName(string item)
        {
            if (string.IsNullOrWhiteSpace(item)) return string.Empty;
            string e = item.Trim();

            // Drop trailing COLLATE / AT TIME ZONE segments (top-level only)
            e = StripTrailingCollateAndTimeZone(e);

            // 1) Alias = Expr   (top-level '=' and no top-level CASE before it)
            if (TrySplitTopLevel(e, '=', out var lhs, out var rhs))
            {
                if (LooksLikeBareIdentifier(lhs) && !HasTopLevelWordBefore(e, "CASE", e.IndexOf('=')))
                    return UnwrapIdentifier(lhs.Trim());
                // fallthrough; '=' is part of expression (e.g., CASE WHEN a = b ...)
            }

            // 2) expr AS alias
            if (TryGetTrailingAsAlias(e, out var asAlias))
                return asAlias;

            // 3) expr <alias>  (only if left looks complex)
            if (TryGetTrailingBareAlias(e, out var bareAlias))
                return bareAlias;

            // 4) t.*  or  *   -> return as-is
            if (Regex.IsMatch(e, @"(?i)(?:^|[\s(])(?:\[[^\]]+\]|""[^""]+""|[A-Za-z_][\w$]*)\.\*\s*$"))
                return Regex.Match(e, @"(?i)(?<t>(?:\[[^\]]+\]|""[^""]+""|[A-Za-z_][\w$]*))\.\*\s*$").Groups["t"].Value + ".*";
            if (Regex.IsMatch(e, @"(?i)^\*\s*$")) return "*";

            // 5) simple identifier/dotted -> last part
            var last = GetLastIdentifier(e);
            if (!string.IsNullOrEmpty(last)) return last;

            // 6) function/literal/etc without alias -> skip
            return string.Empty;
        }

        private static bool TryGetTrailingAsAlias(string e, out string alias)
        {
            alias = string.Empty;
            // ... AS [alias] / "alias" / bare
            var m = Regex.Match(e, @"(?is).*\bAS\s+(?<a>\[[^\]]+\]|""[^""]+""|[A-Za-z_][\w$]*)\s*$");
            if (m.Success)
            {
                alias = UnwrapIdentifier(m.Groups["a"].Value);
                return true;
            }
            return false;
        }

        private static bool TryGetTrailingBareAlias(string e, out string alias)
        {
            alias = string.Empty;
            // trailing quoted/bracketed or bare identifier
            var m = Regex.Match(e, @"(?is)^(?<left>.*?)[ \t]+(?<a>\[[^\]]+\]|""[^""]+""|[A-Za-z_][\w$]*)\s*$");
            if (!m.Success) return false;

            string left = m.Groups["left"].Value.TrimEnd();
            // Only accept as alias if left looks like a complex expression (not just a dotted identifier)
            if (LooksComplex(left))
            {
                alias = UnwrapIdentifier(m.Groups["a"].Value);
                return true;
            }
            return false;
        }

        private static string StripTrailingCollateAndTimeZone(string e)
        {
            // remove trailing: COLLATE <name>
            var t = e;
            while (true)
            {
                var m1 = Regex.Match(t, @"(?is)^(?<core>.*?)[ \t]+\bCOLLATE\b[ \t]+(?<col>[A-Za-z0-9_]+)\s*$");
                if (m1.Success) { t = m1.Groups["core"].Value.TrimEnd(); continue; }

                // remove trailing: AT TIME ZONE <id or 'string'>
                var m2 = Regex.Match(t, @"(?is)^(?<core>.*?)[ \t]+\bAT\b[ \t]+\bTIME\b[ \t]+\bZONE\b[ \t]+(?:\[[^\]]+\]|""[^""]+""|'[^']*'|[A-Za-z_][\w$]*)\s*$");
                if (m2.Success) { t = m2.Groups["core"].Value.TrimEnd(); continue; }

                break;
            }
            return t;
        }

        // ---------------- utilities ----------------

        private static string RemoveComments(string sql)
        {
            var sb = new StringBuilder(sql.Length);
            bool inLine = false, inBlock = false, inSq = false, inDq = false, inBr = false;

            for (int i = 0; i < sql.Length; i++)
            {
                char c = sql[i];
                char n = i + 1 < sql.Length ? sql[i + 1] : '\0';

                if (inLine) { if (c == '\r' || c == '\n') { inLine = false; sb.Append(c); } continue; }
                if (inBlock) { if (c == '*' && n == '/') { inBlock = false; i++; } continue; }

                if (!inSq && !inDq && !inBr)
                {
                    if (c == '-' && n == '-') { inLine = true; i++; continue; }
                    if (c == '/' && n == '*') { inBlock = true; i++; continue; }
                }

                sb.Append(c);

                if (inSq) { if (c == '\'' && n == '\'') { sb.Append(n); i++; } else if (c == '\'') inSq = false; continue; }
                if (inDq) { if (c == '"' && n == '"') { sb.Append(n); i++; } else if (c == '"') inDq = false; continue; }
                if (inBr) { if (c == ']') inBr = false; continue; }

                if (c == '\'') { inSq = true; continue; }
                if (c == '"') { inDq = true; continue; }
                if (c == '[') { inBr = true; continue; }
            }
            return sb.ToString();
        }

        private static bool IsWordAt(string s, int pos, string word)
        {
            if (pos < 0 || pos + word.Length > s.Length) return false;
            for (int k = 0; k < word.Length; k++)
                if (char.ToUpperInvariant(s[pos + k]) != word[k]) return false;

            bool left = pos == 0 || !IsIdentChar(s[pos - 1]);
            int end = pos + word.Length;
            bool right = end >= s.Length || !IsIdentChar(end < s.Length ? s[end] : '\0');
            return left && right;
        }

        private static string NextWord(string s, int pos)
        {
            int i = pos;
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
            int start = i;
            while (i < s.Length && IsIdentChar(s[i])) i++;
            return s.Substring(start, i - start);
        }

        private static bool IsIdentChar(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '$';

        private static void SkipWs(string s, ref int i)
        {
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
        }

        private static int SkipSingleQuoted(string s, int i) // i at opening '
        {
            // returns index of opening; caller will increment
            int p = i;
            for (int k = i + 1; k < s.Length; k++)
            {
                if (s[k] == '\'' && k + 1 < s.Length && s[k + 1] == '\'') { k++; continue; }
                if (s[k] == '\'') return k;
            }
            return s.Length - 1;
        }

        private static bool TrySplitTopLevel(string s, char sep, out string left, out string right)
        {
            left = right = string.Empty;
            bool inSq = false, inDq = false, inBr = false;
            int depth = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                char n = i + 1 < s.Length ? s[i + 1] : '\0';

                if (inSq) { if (c == '\'' && n == '\'') { i++; } else if (c == '\'') inSq = false; continue; }
                if (inDq) { if (c == '"' && n == '"') { i++; } else if (c == '"') inDq = false; continue; }
                if (inBr) { if (c == ']') inBr = false; continue; }

                if (c == '\'') { inSq = true; continue; }
                if (c == '"') { inDq = true; continue; }
                if (c == '[') { inBr = true; continue; }

                if (c == '(') { depth++; continue; }
                if (c == ')') { if (depth > 0) depth--; continue; }

                if (depth == 0 && c == sep)
                {
                    left = s.Substring(0, i);
                    right = s.Substring(i + 1);
                    return true;
                }
            }
            return false;
        }

        private static bool HasTopLevelWordBefore(string s, string word, int pos)
        {
            // check if 'word' appears at top level before pos
            bool inSq = false, inDq = false, inBr = false;
            int depth = 0;

            for (int i = 0; i < pos && i < s.Length; i++)
            {
                char c = s[i];
                char n = i + 1 < s.Length ? s[i + 1] : '\0';

                if (inSq) { if (c == '\'' && n == '\'') { i++; } else if (c == '\'') inSq = false; continue; }
                if (inDq) { if (c == '"' && n == '"') { i++; } else if (c == '"') inDq = false; continue; }
                if (inBr) { if (c == ']') inBr = false; continue; }

                if (c == '\'') { inSq = true; continue; }
                if (c == '"') { inDq = true; continue; }
                if (c == '[') { inBr = true; continue; }

                if (c == '(') { depth++; continue; }
                if (c == ')') { if (depth > 0) depth--; continue; }

                if (depth == 0 && IsWordAt(s, i, word)) return true;
            }
            return false;
        }

        private static bool LooksLikeBareIdentifier(string s)
        {
            s = s.Trim();
            if (s.Length == 0) return false;
            // [id] or "id" or bare single-part id
            if ((s.StartsWith("[") && s.EndsWith("]")) || (s.StartsWith("\"") && s.EndsWith("\"")))
                return true;
            // reject dotted id on LHS of '=' (we want only alias token)
            if (s.Contains(".")) return false;
            // bare identifier
            return Regex.IsMatch(s, @"^[A-Za-z_][\w$]*$");
        }

        private static bool LooksComplex(string s)
        {
            // heuristic: complex if contains parentheses, CASE, operators, literals, or function-call-like
            if (s.IndexOf('(') >= 0 || s.IndexOf(')') >= 0) return true;
            if (Regex.IsMatch(s, @"(?i)\bCASE\b")) return true;
            if (Regex.IsMatch(s, @"[+\-*/%|^&]")) return true;
            if (Regex.IsMatch(s, @"'[^']*'")) return true;
            if (Regex.IsMatch(s, @"(?i)^\s*(?:\[[^\]]+\]|""[^""]+""|[A-Za-z_][\w$]*)\s*\(")) return true; // fn(...)
                                                                                                          // if it's just dotted identifier or bracketed chain, it's NOT complex
            if (Regex.IsMatch(s, @"^\s*(?:\[[^\]]+\]|""[^""]+""|[A-Za-z_][\w$]*)(?:\s*\.\s*(?:\[[^\]]+\]|""[^""]+""|[A-Za-z_][\w$]*))*\s*$"))
                return false;
            return true;
        }

        private static string GetLastIdentifier(string e)
        {
            // capture last identifier from a dotted chain (supports [] and "")
            var m = Regex.Match(e,
                @"(?is)(?:\.[ \t]*)?(?<id>\[[^\]]+\]|""[^""]+""|[A-Za-z_][\w$]*)\s*$");
            if (!m.Success) return string.Empty;
            return UnwrapIdentifier(m.Groups["id"].Value);
        }

        private static string UnwrapIdentifier(string id)
        {
            id = id.Trim();
            if (id.StartsWith("[") && id.EndsWith("]"))
                return id.Substring(1, id.Length - 2).Replace("]]", "]");
            if (id.StartsWith("\"") && id.EndsWith("\""))
                return id.Substring(1, id.Length - 2).Replace("\"\"", "\"");
            return id;
        }
    }

}
