In the Spline Debug scene, there are 6 important objects:

1. Spline
- A curved spline going from (0, 0, 0) to (10, 0, -5)

2. Deform
- A curved spline used to deform Spline

3. Spline Plus
- An object with the SplinePlus component
- This object is used to deform spline Spline along spline Deform
- The resulting spline is rendered in green with Gizmos enabled

4. Evaluate
- Renders a cube gizmo along each of the above splines at a given distance from a given anchor point

5. Nearest Point
- Renders a cube gizmo at the nearest point along the deformed spline from the Spline Plus object

6. Spline Debug
- An instantiated Blender file including a tube warped around the deformed spline using Blender's Curve modifier