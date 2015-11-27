using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mysoft.Project.Json.Linq;
using Mysoft.Project.Json;
namespace Mysoft.Project.Excel
{
    internal class ExcelGroupRow
    {
        public string TreeCode { get; set; }
        [JsonIgnore]
        public ExcelGroupRow Parent { get; set; }
        public ExcelGroupRow()
        {

        }
        public int Level { get; set; }
        public int EndIndex { get; set; }
        public int StartIndex { get; set; }

        public static Dictionary<string, ExcelGroupRow> GetGroupRows(JArray array, string treecodeFiled)
        {

            var dict = new Dictionary<string, ExcelGroupRow>();
            if (string.IsNullOrEmpty(treecodeFiled)) return dict;
            for (int i = 0; i < array.Count; i++)
            {
                var item = array[i];
                var treecode = (string)item[treecodeFiled];
                treecode = treecode.Trim();
                ExcelGroupRow row = new ExcelGroupRow();
                row.StartIndex = i;
                row.EndIndex = i;
                row.TreeCode = treecode;
                var parentCode = treecode.Substring(0, Math.Max(0, treecode.LastIndexOf('.')));
                while (!string.IsNullOrEmpty(parentCode))
                {
                    ExcelGroupRow parent;
                    if (dict.TryGetValue(parentCode, out parent))
                    {
                        row.Parent = parent;
                        row.Level = parent.Level + 1;
                        item[treecodeFiled] = new string(' ', 2 * row.Level) + treecode;
                        while (parent != null)
                        {
                            parent.EndIndex = i;
                            parent = parent.Parent;
                        }
                        break;
                    }
                    parentCode = parentCode.Substring(0, Math.Max(0, parentCode.LastIndexOf('.') - 1));
                }
                dict.Add(treecode, row);
            }

            return dict;
        }

    }
}
