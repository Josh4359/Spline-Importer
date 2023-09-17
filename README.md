# Spline-Importer
 Import and export splines between Blender and Unity

Installing the Blender Add-on:
- In Blender, go to Edit -> Preferences -> Add-ons and click "Install..."
- Select the "Spline Exporter.py" file from this package

Exporting splines from Blender:
- Select a spline
- In the "Properties" window, go to the "Scene Properties" tab and under the "Spline Exporter" dropdown select "Export Spline"
- Select the desired file location for the exported .json file

Exporting splines from Unity:
- Select a GameObject with a SplineContainer component
- Add the SplineImporter script
- (Optional) In the inspector, assign the "Spline Data" variable with the desired output .json file if available
- Click "Export Spline"
- If the "Spline Data" variable is not assigned, you will be prompted to select the desired file location for the exported .json file

Importing splines to Blender:
- Navigate to the "Scene Properties" tab in the "Properties" window and under the "Spline Exporter" dropdown select "Import Spline"
- Select the desired .json file from the file explorer window

Importing splines to Unity:
- Add a GameObject with the SplineImporter script
- Drag your .json file into the "Spline Data" variable in the inspector
- Click "Import Spline"