using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Record 
{

    public Record(ToolGroupType type, Tool tool) {
        this.type = type;
        this.tool = tool;
        form = null;
    }
    public ToolGroupType type { get; set; }
    public Tool tool { get; set; }
    public FormInput form { get; set; }

}
