using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Record 
{
    private ToolGroupType type;
    private Tool tool;
    private FormInput form;
    public Record(ToolGroupType type, Tool tool) {
        this.type = type;
        this.tool = tool;
        form = null;
    }
    public new ToolGroupType GetType()
    {
        return this.type;
    }
    public Tool GetTool()
    {
        return this.tool;
    }
    public void SetForm(FormInput form) {
        this.form = form;
    }
    public FormInput GetForm()
    {
        return this.form;
    }
}
