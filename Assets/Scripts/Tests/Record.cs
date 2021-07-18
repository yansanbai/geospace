using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class Record 
{

    public Record(ToolGroupType type, Tool tool) {
        this.type = type;
        this.tool = new Tool();
        this.tool.Name = tool.Name;
        this.tool.Description = tool.Description;
        this.form = null;
    }
    public string GetCommand() {
        Debug.Log(this.type);
        if (this.type == ToolGroupType.Geometry)
        {
            return this.tool.Description;
        }
        else {
            Debug.Log(JsonConvert.SerializeObject(this.form));
            string str1 = "";
            string str2 = "";
            MatchCollection mc1=Regex.Matches(JsonConvert.SerializeObject(this.form), @"""[\u4e00-\u9fa5A-Z=⊥]+""|([0-9]*[.][0-9]*)");
            for (int i = 0; i < mc1.Count; i++) {
                str1 += mc1[i].Value;
            }
            MatchCollection mc2 = Regex.Matches(str1, @"[\u4e00-\u9fa5A-Z=⊥]+|([0-9]*[.][0-9]*)");
            for (int i = 0; i < mc2.Count; i++)
            {
                str2 += mc2[i].Value;
            }
            if (this.type== ToolGroupType.Condition)
            {
                return this.tool.Description + str2;
            }
            else
            {
                return str2;
            }
        }
    }
    public ToolGroupType type { get; set; }
    public Tool tool { get; set; }
    public FormInput form { get; set; }

}
