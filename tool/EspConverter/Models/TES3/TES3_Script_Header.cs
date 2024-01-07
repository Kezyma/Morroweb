﻿namespace EspConverter.Models.TES3
{
    public class TES3_Script_Header
    {
        public int num_shorts { get; set; }
        public int num_longs { get; set; }
        public int num_floats { get; set; }
        public int bytecode_length { get; set; }
        public int variables_length { get; set; }
    }

}
