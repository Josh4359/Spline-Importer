bl_info = {
    "name": "Spline Exporter",
    "author": "Josh",
    "version": (1, 0),
    "blender": (2, 80, 0),
    "location": "File > Import-Export",
    "description": "Export spline control point data as .JSON",
    "category": "Import-Export"
}

import json
import bpy
import math
from mathutils import *
from bpy_extras.io_utils import ExportHelper
from bpy_extras.io_utils import ImportHelper
from bpy.props import StringProperty

def ExportSpline():
    curve = bpy.context.active_object
    
    splines = {"splines": []}

    for thisSpline in curve.data.splines:
        points = {
            "controlPoints": [],
            "closed": thisSpline.use_cyclic_u
        }

        for thisPoint in thisSpline.bezier_points:
            co = thisPoint.co
            
            hl = thisPoint.handle_left
            
            hr = thisPoint.handle_right
            
            points["controlPoints"].append(
            {
                "position": {
                    "x": co.x,
                    "y": co.y,
                    "z": co.z
                },
                "handleL": {
                    "x": hl.x,
                    "y": hl.y,
                    "z": hl.z
                },
                "handleR": {
                    "x": hr.x,
                    "y": hr.y,
                    "z": hr.z
                },
                "tilt": thisPoint.tilt * (180 / math.pi)
            })
        
        splines["splines"].append(points)

    class SaveJSON(bpy.types.Operator, ExportHelper):
        bl_idname = "object.save_json"
        
        bl_label = "Save JSON"
        
        filename_ext = ".json"
        
        filepath: StringProperty(subtype="FILE_PATH")
        
        def execute(self, context):
            with open(self.filepath, "w") as f:
                json.dump(splines, f, indent = 4)
            
            return {"FINISHED"}
        
        def invoke(self, context, event):
            context.window_manager.fileselect_add(self)
            
            return {"RUNNING_MODAL"}
    
    bpy.utils.register_class(SaveJSON)

    bpy.ops.object.save_json("INVOKE_DEFAULT")

def ImportSpline():
    class LoadJSON(bpy.types.Operator, ImportHelper):
        bl_idname = "object.load_json"
        
        bl_label = "Load JSON"
        
        filename_ext = ".json"
        
        filter_glob: bpy.props.StringProperty(
            default = "*.json",
            options = {"HIDDEN"},
            maxlen = 255
        )
        
        def execute(self, context):
            with open(self.filepath, "r") as f:
                splines = json.load(f)
                
                curve = bpy.data.curves.new("BezierCurve", "CURVE")
                
                curve.dimensions = "3D"
                
                curve.twist_mode = "Z_UP"
                
                for thisSpline in splines["splines"]:
                    i = 0
                    
                    spline = curve.splines.new("BEZIER")
                    
                    for thisPoint in thisSpline["controlPoints"]:
                        if i > 0:
                            spline.bezier_points.add(1)
                        
                        bezierPoint = spline.bezier_points[i]
                        
                        position = thisPoint["position"]
                        
                        handleL = thisPoint["handleL"]
                        
                        handleR = thisPoint["handleR"]
                        
                        bezierPoint.co = Vector((position["x"], position["y"], position["z"]))
                        
                        bezierPoint.handle_left = Vector((handleL["x"], handleL["y"], handleL["z"]))
                        
                        bezierPoint.handle_right = Vector((handleR["x"], handleR["y"], handleR["z"]))
                        
                        i += 1
                    
                    spline.use_cyclic_u = thisSpline["closed"]
                
                bpy.context.scene.collection.objects.link(bpy.data.objects.new("BezierCurve", curve))
            
            return {"FINISHED"}
    
    bpy.utils.register_class(LoadJSON)
    
    bpy.ops.object.load_json("INVOKE_DEFAULT")

class SplineExportOperator(bpy.types.Operator):
    bl_idname = "object.spline_export_operator"
    
    bl_label = "Export Spline"
    
    def execute(self, context):
        ExportSpline()
        
        return {"FINISHED"}

class SplineImportOperator(bpy.types.Operator):
    bl_idname = "object.spline_import_operator"
    
    bl_label = "Import Spline"
    
    def execute(self, context):
        ImportSpline()
        
        return {"FINISHED"}

class SplineExporterPanel(bpy.types.Panel):
    bl_label = "Spline Exporter"
    
    bl_idname = "SCENE_PT_SPLINE_EXPORTER"
    
    bl_space_type = "PROPERTIES"
    
    bl_region_type = "WINDOW"
    
    bl_context = "scene"
    
    def draw(self, context):
        layout = self.layout
        
        scene = context.scene
        
        layout.label(text="Export Spline")
        
        row = layout.row()
        
        row.scale_y = 1
        
        row.operator("object.spline_export_operator")
        
        row.operator("object.spline_import_operator")

def register():
    bpy.utils.register_class(SplineImportOperator)
    
    bpy.utils.register_class(SplineExportOperator)
    
    bpy.utils.register_class(SplineExporterPanel)

def unregister():
    bpy.utils.unregister_class(SplineImportOperator)
    
    bpy.utils.unregister_class(SplineExportOperator)
    
    bpy.utils.unregister_class(SplineExporterPanel)

if __name__ == "__main__":
    register()