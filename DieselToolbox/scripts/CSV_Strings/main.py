import clr

clr.AddReference("DieselToolbox")
clr.AddReference("DieselEngineFormats")

from DieselToolbox import ScriptActions
from DieselEngineFormats import StringsFile

class CSVConv():
    key = "strings_csv"
    title = "CSV"
    type = "strings"
    extension = "csv"
    #options {}
    
    def export(self, stream):
    	str = StringsFile(stream)
        strs = []
        strs.append("ID,String\n")
        for entry in str.LocalizationStrings:
            strs.append('"' + entry.ID.ToString() + '","' + entry.Text + '"\n')
            
        return ''.join(strs)
        


def register():
    ScriptActions.RegisterConverter(CSVConv())