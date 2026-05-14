# Developer Notes
## ---170326---
Version 1.0.0

### Create this program

The system is organized into the following modules:

* CodeManager  
Components that enable or disable Grasshopper components by Group or GroupName.  
This module is used to control computation and improve performance.

* Control.Timeline  
Components responsible for controlling the animation timeline, including frame and step management.

* Control.Camera  
Components and utilities for controlling Rhino viewport cameras during animation.

* Geometry.Processing  
Time-dependent geometry operations. These components transform or generate geometry based on animation time segments.

* Geometry.Display  
Visualization utilities for Rhino viewport previews, including fading, clipping, gradients, and other display effects.

* GHComponents  
Definitions and configurations of all Grasshopper components exposed by this plugin.

* data  
Stores configuration or preset data such as colour palettes, camera presets, or reusable geometry settings.  
Data is stored in JSON format and loaded when the plugin initializes.

* Util.IO  
General utilities for reading and writing configuration data.  
These utilities convert JSON files in the `.data` folder into strongly typed C# objects used by the system.

Example responsibilities:
- `Read`: Load JSON files from `.data` and deserialize them into objects (e.g. color palettes, presets).
- `Write`: Save configuration or cached data back to `.data` in JSON format.
- Provide a centralized way for all modules to access configuration data.

Typical structure example:
```
.data/
    colors.json
    camera_presets.json
    geometry_presets.json
```
![System Architecture](/Images/SystemArchitecture.png)


## ---180326---
Testing The colour code and make the GHComponents
Finished the Timeline setting class in C#

## ---190326---
Making Colour Code Component and attributes
Finished
![alt text](/Images/ColourCode.png)
Finished GHComponents
GH_ColourCode with a button
GH_CreateColourCode
GH_ColourCodePanel
GH_SaveColourCode

GH_SaveGeometry
GH_DelGeometry

unfinished
GH_GetColour : Translate the colour code into a system.drawing.color 
Geometry Del, and select -> need a selecting list


## ---200326 ----

### New system architecture
_IO_ 
Util.IO
* JsonRead, JsonWrite
    Parsing data from json into this system
* RhinoJson
    Parsing data from json into rhino information, implementing the JsonRead and JsonWrite
* DataProcessing with Json
    - Colour Code -> Actively initialised
    - Geometry    -> Update based on the users

    - Composed of different functions to parse and display the data in Util.IO namespace
    
    - This data parts at least have three components:
        - Load: load data from a json file with the JsonRead or RhinoJson
        - Save: Save data into a json file with the JsonWrite or RhinoJson
        - Delete: Delete existing data from a json file wuth the JsonWrite or RhinoJson
    
    - UI : On going process
        - GH Component Attribute: Button, List Display, List selection
        - GH Component: Implement the GH Component Attribute

Process
Making the ValueListUIAttributes
![Debug](/Images/Debug1.png)
1. Make the selection list box rounded  (OK and change the colour)
2. Make the colour style same (yeah)
3. Selection list interaction
4. the input and output text need to be attached to the bounds. 


# --- 230326---
1. ValueListUIAttributes : fixlayout() is needed to be finished. 
2. the input and output text need to be attached to the bounds. 
3. Selection list interaction

# --- 240326---
Making the value list attribute (ongoing) -> need to finished the data selection

# --- 250326---
Finished the colour code related components
GH_ColourCode
GH_ColourCodePanel : unstable
GH_CreateColourCode
GH_GetColour
GH_SaveColourCode
GH_SelectColour

Finish Geometry database setting
GH_DelGeometry
GH_LoadGeometry
GH_SaveGeometry
GH_Easing

CameraSetting on the progress: need to do some research with the optimal structure
CameraJson cease due to unnessesary 

Maybe can start from the timeline Global t Slider? 

# ---260326---
Making Timeline slot panel , adjusting the UI setting
Finished 
GH_ShiftTimeline
GH_CreateTimeline
GH_TimelineAbstract

# ---270326---
TimelineUIAttrbute -> MouseResponse Issues and input issue. The text editing part is weird.

GH_ActiveTimeline : making in progress. Pause -> Develop later, can refer to GH_Mapper

# --- 3003---
Finished many components
Util
GH_VisiableGeometry

Timeline Components except for the GH_TimelineMapper

# ---3103---
Idea for camera -> class Camera with a function to change the camera motion setting and view setting

Make a clipping animation : need to see the clipping setting in Rhino Or make a customised (computational cost)

# ---0104----
Debugging the load geometry and colour problems -> File direction error
Memorised the data from load geometry and colour -> Selected Item and Path reference

Start to make the Vectordisplay class
GH_Vectordisplay ClippingBox -> need debug

Finish example GH file Cutting_Example.gh -> Milling part

# ---0204 ---
Finished the Display Geometry and vector display

* GH_VectorDashCurveDisplay : Not set up yet 

Start the geometry processing -> the transformation isn't inherit

# ---0804 ---
Create GH_DisplayGeometryWire and 
Modify the flat shader and outline in GH_DisplayGeometry and GH_DisplayGeometryWire
The function is used to create a Non-photorealistic rendering + outline render effect. 
Add silhouette to compute the outline. 

DrawPaths: On progress.
DrawPath_ChangeEnd : need to be finished

CameraSetting and debug
Making Example file

# ---0904 ---
Finished and debug the Path setting
PathUtil for GH_AddEndPath, GH_LinkPath and GH_DrawPath
Need to make Example file


# ---1004 ---
Debug notes from Walid_Animation_Test_01.gh
![Note1](/Images/Debug_Notes.png)
![Note2](/Images/Debug_Notes_2.png)

GH_DisplayGeometry.cs : need to debug first


# ---1504 ---
Finished this in DisplayUtil
public static List<T1> AlignList<T1, T2>(List<T1> Target, List<T2> Goal)
        {
            if (Target == null) throw new Exception("Target is null");
            if (Goal == null || Goal.Count == 0) throw new Exception("Goal is null");
            var NewList = new List<T1>();
            for(int i = 0; i < Target.Count; i++)
            {
                var Ei = Goal.Count > i ? i : Goal.Count - 1;

            }

            return NewList;
        }

        finished new colourcode setting and GHComponent
# --- 1604 ---
Start on the GeometryCode setting : change the system structure


# --- 1704 ---
Finished the Geometrycode and testing:
Component includes -> in GeometryCode folder
* GH_CreateGeometryCode.cs
* GH_CreateNewGeometryCode
* GH_DeleteGeometryCode
* GH_GeometryCodeAbstract
* GH_LoadGeometry
* GH_SaveGeometryCode
* GH_SelectGeometry


# --- 2004 ---
Finished test
new debug list in Grasshopper

ColourCode update weird and load the colour options

ColourCodePanel Need to be rewrite into GH_Param<Color> refer to the Grasshopper.dll : delay

# --- 2204 ---
Finished CodeManagerUtil and started to make GH_Component for it. 
haven't debugged yet. 

# --- 2304 ---
Fix TimelineDescription bug
Fix CodeManagerUtil.EnableToggle and DisplayToggle bugs

start camera setting, 
Finish 
* cameraParameter: based class to store viewportInfo from rhino
* GH_CameraGoo: a GH_Goo object that implement in GH_PersistentParam
* GH_CameraParam: a GH_PersistentParam parameters for camera


# ---2404 ---
Camera Param debug and editing CameraParamteters with the duplicate function
CameraTranform is create as a static class for the basic transformation
ICameraMotion is an interface to defind the initial structure of the CameraMotion, which control camera with t.
CameraMotionAbstract is the basic motion class.

Setting CameraTranform
DashTypeCode change by system : delay

GH_Component status
GH_CameraGoo
GH_CameraParam
GH_CreateCamera
GH_GetCameraInfo
GH_CodeManagerAbstract
GH_DisplayGroup
GH_DisplayName
GH_EnableGroup
GH_EnableName
GH_ColourCodeAbstract
GH_ColourCodePanel <- pending
GH_CreateColourCode
GH_CreateNewColourCodeFile
GH_GetColour
GH_LoadColourCode
GH_SaveColourCode
GH_SelectColour
GH_DisplayGeometry
GH_DisplayGeometryCols <- pending
GH_DisplayGeometryWire
GH_VectorCurveDisplay <- pending
GH_DashCruveDisplay <- pending
GH_VectorDisplay <- pending
GH_VectorDisplaySetting <- pending
GH_VectorsDisplay <- pending
GH_CreateGeometryCode
GH_CreateNewGeometryCode
GH_DeleteGeometryCode
GH_GeometryCodeAbstract
GH_LoadGeometry
GH_SaveGeometryCode
GH_SelectGeometry
GH_AddEndPath
GH_DrawPath
GH_GeometryActionAbstract <- pending
GH_GeometryAnimation <- pending
GH_LinkPath
GH_PathEditAbstract
GH_TranslationAction <- Pending
GH_ActiveTimeline
GH_ActiveTimelineZone
GH_CreateTimeline
GH_CreateTimelineByAccumulatedTime
GH_IntervalRange
GH_IsActiveByTimeline
GH_RedefineTimeline
GH_SegmentiseTimeslotLinear
GH_SegmentiseTimeslotNonLinear
GH_ShitTimeline
GH_TimelineAbstract
GH_TimelineMapper <- pending
GH_DashCurve
GH_DelGeometry <- obsolete
GH_Easing
GH_Silhouette
GH_VisiableGeometry


# --- 2704 ---
Finish Camera gh setting, only focus on one cameraMotion

GH_CameraDolly -> CM_Dolly
GH_CameraFromAtoB -> CM_CamAtToCamB
GH_CameraLookAt -> CM_LookAt
GH_CameraMotionAbstract 
GH_CameraOrbit -> CM_Orbit
GH_CameraPan -> CM_Pan
GH_CameraRotate -> CM_Rotate
GH_CameraZoom -> CM_Zoom


# --- 2804 ---
Geometry processing Rotation and GH_Components
Setting the Remote Data Channel example, RemoteTime, GH_Timespot setting -> need to create an abstract class and a new system architecture
Debug GH_DisplyGeometryWire

Start making GH animation for testing

# --- 2904 ---
Finished animation

# --- 3004 ---
Camera debugs
* The camA to camB need to debug when the camera spinning
* Parallel Camera Skeleton error, windowrectangle isn't aligned with the frustum

## pending list
GH_FindComponent By name or by group -> will label the found component by a group boundary in yellow 

(multi-transformation)
GH_GeometryActionAbstract <- pending
GH_GeometryAnimation <- pending

GH_ColourCodePanel <- pending, need to change into GH_Param
GH_DisplayGeometryCols <- pending debugs

(VectorDisplay Debugs)
GH_VectorCurveDisplay <- pending
GH_VectorDisplay <- pending
GH_VectorDisplaySetting <- pending
GH_VectorsDisplay <- pending

GH_TimelineMapper <- pending better UI

(DashCurve)
GH_DashCruveDisplay <- pending with DashType data base and a new valuelist which is a sub_class of TagChannel

Tag Channel : General, like data dam

DashType, 
TagChannel_ValueList -> called by DashCurve json mechanism

Timeline list item -> matching output

----
New util framework to output the animation
NewSlider : control t with steps, run, pause, stop
Render: get t, and window size setting, framePrefix, outputlocation
Compile: folder, output location and name, framePrefix, 

# --- 0505 ---
GH_VectorDisplay _ Obsolete
GH_VectorsDisplay : input debugs

Bugs: Animation setting and the transformation action are fixed but needed to optimised by introducing a new class that store all the conveyable data, Geometrybase, Transformation and a dictionary for customised. 

# --- 0605 ---
Create components and class related to Tag Channel
finish the matchlist: match the data in a list

Animation Compile system framework is created
need to solve python compile issue

# --- 0705 ---
python compile remove. using C# to compile the animation


# --- 1205 ---
Set up the new DashType setting structure.

Created:
GH_GlobalDashSetting.cs
ValueListUIAttributesEditable.cs
ValueListItem
DashTypeListItem
Updated:

GH_DisplayGeometryWire
GH_DashCurve
GH_CameraParam
Added animation compile utility work:

Created MirrorFrames in AnimationCompileUtil
Created GH_MirrorFrames
Pending:
Debug ValueListUIAttributesEditable.cs UI behavior.

# --- 1305 ---
Finalised ValueListUIAttributesEditable and GH_GlobalDashSetting

ValueListUIAttributesEditable:
* Adjust the UI setting
* Move ShowListEditor to GH_GlobalDashSetting

Create new interface:
IEditable : regulate ShowEditor()

GH_GlobalDashSetting:
* Debug DashType setting
* Debug DisplayGeometry.cs

Animation Compile:
* Create AnimationCompileWindow with Eto
* Add ShowEditor() in GH_AnimationCompile
* Create ButtonUIAttributesEditable for GH_AnimationCompile
* Update AnimationCompileUtil ffmpeg search for Mac and Windows
* If ffmpeg cannot be found, ask user to open installation page

Pending:
* DisplayGeometry.cs data structure need to be cleaned later
* Colour in DisplayGeometry can be simplified from DataTree<Color> to item setting

GeometryClipping in Geometry.Process
* Meshise input Brep or remesh input Mesh with refined mesh structure
* Test mesh faces against clipping plane
* Display only mesh faces that satisfy the clipping-plane condition


# --- 1405 ---
Update : Double click the component to select a file.
GH_LoadGeometryCode
GH_LoadColourCode
ISelectExistFile
SelectExistFileExtensions

Window ->
GH_DisplayGeometryWire.cs : Silhouette function and null value testing. 

create a Param_Directory which is used for the 
GH_CreateNewCodeFile
GH_CreateNewGeometryCodeFile
Adding add default code in the both