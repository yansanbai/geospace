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
        Debug.Log(JsonConvert.SerializeObject(this.tool));
        if (this.type == 0)
        {
            return this.tool.Description;
        }
        else {
            string str1 = "";
            string str2 = "";
            Debug.Log(JsonConvert.SerializeObject(this.form));
            MatchCollection mc1=Regex.Matches(JsonConvert.SerializeObject(this.form), @"""[\u4e00-\u9fa5A-Z]+""");
            for (int i = 0; i < mc1.Count; i++) {
                str1 += mc1[i].Value;
            }
            MatchCollection mc2 = Regex.Matches(str1, @"[\u4e00-\u9fa5A-Z]+");
            for (int i = 0; i < mc2.Count; i++)
            {
                str2 += mc2[i].Value;
            }
            return str2;
        }
    }
    public ToolGroupType type { get; set; }
    public Tool tool { get; set; }
    public FormInput form { get; set; }

}
