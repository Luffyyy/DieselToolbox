import clr

clr.AddReference("DieselToolbox")
clr.AddReference("DieselEngineFormats")

from DieselToolbox import ScriptActions
from DieselEngineFormats import StringsFile

class DieselConverter():
    key = "diesel_strings"
    title = "Diesel"
    extension = "strings"
    type = "strings"
    #options {}
    
    def _import(self, filepath):
        return StringsFile(filepath)
        


def register():
    ScriptActions.RegisterConverter(DieselConverter())