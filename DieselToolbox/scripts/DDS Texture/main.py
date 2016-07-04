import clr

clr.AddReference("DieselToolbox")
#clr.AddReference("DieselEngineFormats")

from DieselToolbox import ScriptActions
#from DieselEngineFormats import StringsFile

class DDSConv():
    key = "texture_dds"
    title = "DDS"
    extension = "dds"
    type = "texture"
    options = { }
    
    def export(self, stream):
        return stream
        


def register():
    ScriptActions.RegisterConverter(DDSConv())