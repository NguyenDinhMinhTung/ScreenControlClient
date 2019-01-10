﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenControlClient
{
    class Convert
    {
        static Dictionary<int, string> dictionary = new Dictionary<int, string>()
        {
            {0x08,"{BS}" },
            {0x09,"{TAB}" },
            {0x0D,"~" },          //ENTER

            {0x20," " },
            {0x30,"0" },
            {0x31,"1" },
            {0x32,"2" },
            {0x33,"3" },
            {0x34,"4" },
            {0x35,"5" },
            {0x36,"6" },
            {0x37,"7" },
            {0x38,"8" },
            {0x39,"9" },

            {0x41, "a"},
            {0x42, "b"},
            {0x43, "c"},
            {0x44, "d"},
            {0x45, "e"},
            {0x46, "f"},
            {0x47, "g"},
            {0x48, "h"},
            {0x49, "i"},
            {0x4A, "j"},
            {0x4B, "k"},
            {0x4C, "l"},
            {0x4D, "m"},
            {0x4E, "n"},
            {0x4F, "o"},
            {0x50, "p"},
            {0x51, "q"},
            {0x52, "r"},
            {0x53, "s"},
            {0x54, "t"},
            {0x55, "u"},
            {0x56, "v"},
            {0x57, "w"},
            {0x58, "x"},
            {0x59, "y"},
            {0x5A, "z"},
            {0x5B, "{LWIN}"},
            {0x5C, "{RWIN}"},
            {0x5D, "{APPS}"},
        };

        public static string ToKey(int key)
        {
            string value = "";
            dictionary.TryGetValue(key, out value);

            return value;
        }
    }
}
