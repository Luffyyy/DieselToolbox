import clr

clr.AddReference("DieselToolbox")
#clr.AddReference("DieselEngineFormats")

from DieselToolbox import ScriptActions
#from DieselEngineFormats import StringsFile

class JSONConv():
    key = "strings_json"
    title = "JSON"
    extension = "json"
    type = "strings"
    options = { }
    
    def export(self, stream):
    	str = StringsFile(stream)
        strs = []
        strs.append("{\n")
        for entry in str.ModifiedStrings:
            strs.append('\t"' + entry.ID + '" : "' + entry.Text + '"\n')
            
        strs.append("}")
        return ''.join(strs)

def register():
    ScriptActions.RegisterConverter(JSONConv())