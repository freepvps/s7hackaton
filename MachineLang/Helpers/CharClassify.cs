using System;
using System.Collections.Generic;
using System.Text;

namespace MachineLang.Helpers
{
    public static class CharClassify
    {
        class CharGroup
        {
            public string Name;
            public int Id;
            public Func<char, bool> Filter;

            public CharGroup(string name, int id, Func<char, bool> filter)
            {
                Name = name;
                Id = id;
                Filter = filter;
            }
        }

        public const string UnknownGroupName = "unknown";
        public const int UnknownGroupId = 0x40000000;

        private readonly static Dictionary<int, CharGroup> GroupsTable = new Dictionary<int, CharGroup>();
        private readonly static CharGroup[] Groups;

        static CharClassify()
        {
            Groups = new CharGroup[]
            {
                new CharGroup("space", 0, char.IsWhiteSpace),
                new CharGroup("letter", 1, char.IsLetterOrDigit)
            };
            foreach (var group in Groups)
            {
                GroupsTable[group.Id] = group;
            }
        }

        public static int ClassifyChar(char ch)
        {
            foreach (var group in Groups)
            {
                if (group.Filter(ch))
                {
                    return group.Id;
                }
            }
            return UnknownGroupId;
        }
        public static string GetClassName(int cls)
        {
            if (GroupsTable.TryGetValue(cls, out CharGroup group))
            {
                return group.Name;
            }
            return UnknownGroupName;
        }
    }
}
