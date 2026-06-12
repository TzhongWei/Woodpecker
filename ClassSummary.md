# Class Summary

This file maps the main classes in the project by folder. It is intended as a quick navigation index for the Woodpecker animation plugin.

## Root

- `IBOIS_AnimationComponent` - Default sample/component entry point for the plugin assembly.
- `IBOIS_AnimationInfo` - Grasshopper assembly metadata for the Woodpecker animation plugin.

## CodeManager

- `CodeManagerUtil` - Shared helpers for finding and coordinating Grasshopper document components.
- `IEditableWindow` - Interface for components that expose an editable settings window.
- `IHasMultipleActiveInstanceDocumentComponent` - Interface for components that manage multiple active instances in one document.
- `ISelectExistFile` - Interface for components that can select an existing file.
- `SelectExistFileExtensions` - Extension helpers for file-selection workflows.
- `ISingletonDocumentComponent` - Interface for components that should behave as a tagged singleton in a document.
- `ITagChannel<T>` - Generic interface for channel-style tagged data storage.
- `ITimelineDependency` - Interface marker for timeline-dependent components.
- `IUpdateDependent` - Interface for components that can update dependent components.
- `RefleshGHDocument` - Utility for refreshing or expiring related Grasshopper document objects.
- `TagChannel<T>` - Tagged channel storing named Grasshopper `DataTree<T>` payloads.
- `TimeSlotTagChannel` - Tagged channel specialised for a single timeline `double` value.

## Control.Camera

- `CameraExecution` - Applies camera motion results to Rhino views.
- `CameraMotionAbstract` - Base class for camera motion actions evaluated over time.
- `CameraParameter` - Serializable camera state containing camera vectors, projection data, viewport information, and parallel-window values.
- `CameraTransform` - Static camera transformation helpers such as zooming and motion interpolation.
- `CameraUtil` - Camera construction, display, frustum, and Rhino view helper methods.
- `CM_CamAToCamB` - Camera motion interpolating from one camera parameter to another.
- `CM_Dolly` - Camera motion moving camera and target along the viewing direction.
- `CM_LookAt` - Camera motion rotating the view toward a target.
- `CM_Orbit` - Camera motion orbiting around a target.
- `CM_Pan` - Camera motion translating the camera view sideways/upwards.
- `CM_Rotate` - Camera motion rotating around camera axes.
- `CM_Zoom` - Camera motion changing camera zoom.
- `CM_Zoom_Target` - Camera motion zooming toward a specified target point.
- `ICameraMotion` - Interface for time-evaluated camera motion actions.
- `IMultiCamsTransform` - Interface for actions that need multiple camera parameters.
- `ParallelWindowValues` - Viewport-independent parallel projection height, aspect ratio, and offset values.

## Control.Timeline

- `SequetialTimeline` - Timeline made from sequential time points and flip state.
- `TimelineSetting` - Timeline utilities for easing, activation, interval mapping, and timeslot segmentation.

## GHComponents.Camera

- `GH_CameraDolly` - Grasshopper component that creates a dolly camera motion.
- `GH_CameraFromAtoB` - Grasshopper component that creates camera interpolation between two views.
- `GH_CameraGoo` - Grasshopper goo wrapper for camera data.
- `GH_CameraLookAt` - Grasshopper component that creates a look-at camera motion.
- `GH_CameraMotionAbstract` - Base Grasshopper component for camera motion components.
- `GH_CameraOrbit` - Grasshopper component that creates an orbit camera motion.
- `GH_CameraPan` - Grasshopper component that creates a pan camera motion.
- `GH_CameraParam` - Persistent Grasshopper parameter for camera values.
- `GH_CameraRotate` - Grasshopper component that creates a camera rotation motion.
- `GH_CameraZoom` - Grasshopper component that creates a zoom camera motion.
- `GH_CameraZoombyTarget` - Grasshopper component that creates target-based camera zoom motion.
- `GH_CreateCamera` - Grasshopper component that creates a real Rhino camera view from parameters.
- `GH_GetCameraInfo` - Grasshopper component that extracts camera data from a Rhino view.
- `GH_ToCamera` - Grasshopper component that applies camera data to a view.

## GHComponents.CodeManager

- `GH_CodeManagerAbstract` - Base component for display/enable operations on named document objects.
- `GH_DisplayGroup` - Component for changing display state by group.
- `GH_DisplayName` - Component for changing display state by object name.
- `GH_EnableGroup` - Component for enabling/disabling groups.
- `GH_EnableName` - Component for enabling/disabling named objects.
- `GH_TagChannel_Abstract` - Base component for tag-channel input and output components.
- `GH_TagChannel_IN` - Variable input component that publishes named data to a tag channel.
- `GH_TagChannel_Out` - Variable output component that reads named data from a tag channel.

## GHComponents.ColourCode

- `GH_ColourCodeAbstract` - Base component for colour-code components with dependent update behaviour.
- `GH_ColourCodePanel` - Component for displaying or editing colour-code data.
- `GH_CreateColourCode` - Component that creates a colour-code entry.
- `GH_CreateNewColourCodeFile` - Component that creates a new colour-code file.
- `GH_GetColour` - Component that retrieves colours from a colour-code source.
- `GH_LoadColourCode` - Singleton component that loads colour-code data from file.
- `GH_SaveColourCode` - Component that saves colour-code data.
- `GH_SelectedColour` - Component that selects colours from loaded colour-code data.

## GHComponents.CustomGHComponents

- `AnimationTimerAttributes` - Custom resizable canvas attributes for the animation timer component.
- `AnimationTimerAttributesColourSetting` - Colour settings for animation timer attributes.
- `AttributeColourUtil` - Colour helper methods for custom component attributes.
- `AttributeUtil` - Shared drawing and layout helpers for custom attributes.
- `ButtonColourStateSetting` - Button colour settings for state-aware button attributes.
- `ButtonColours` - Colour palette for button UI attributes.
- `ButtonUIAttributes` - Custom component attributes with an embedded button.
- `ButtonUIAttributesEditable` - Button attributes that open editable component windows.
- `ButtonUIAttributesState` - Button attributes for components with explicit on/off state.
- `ButtonUIAttributesStateEditable` - State button attributes with editable window support.
- `ColourDisplayAttributes` - Custom attributes for displaying colour data.
- `DashTypeListItem` - Value-list item representing a dash pattern.
- `Param_Directory` - Custom string parameter with directory selection support.
- `ResizableAttributes<T>` - Base resizable component attribute class for custom canvas UI.
- `SliderDragUndo` - Undo action used while dragging animation timer slider values.
- `TimelineSlot` - Data model for one interactive timeline slot.
- `TimelineSlotColour` - Colour palette for timeline slots.
- `TimelineUIAttributes` - Custom attributes for timeline editing on the Grasshopper canvas.
- `TimelineUIColour` - Colour settings for timeline UI attributes.
- `ValueListItem` - Base item for custom value-list style entries.
- `ValueListUIAttributes` - Custom value-list canvas attributes.
- `ValueListUIAttributesEditable<T>` - Editable value-list attributes with item editing support.
- `ValueListUIColours` - Colour palette for value-list UI attributes.

## GHComponents.Display

- `GH_AnnotationSetting` - Creates screen-facing or plane-oriented annotation display settings.
- `GH_DisplayAnnotation` - Displays screen-facing or plane-oriented text through the custom render conduit.
- `GH_DisplayCurveVector` - Displays vector information using curve-based arrow geometry.
- `GH_DisplayDashCurveVector` - Displays curve-based vectors using configurable dash patterns.
- `GH_DisplayGeometry` - Displays shaded geometry through a selectable Rhino render stage.
- `GH_DisplayGeometryAbstract` - Shared base component for conduit registration, render-stage selection, and preview lifecycle.
- `GH_DisplayGeometryCols` - Displays geometry with per-object or animated colour content.
- `GH_DisplayGeometryWire` - Displays geometry edges and wires through the custom display pipeline.
- `GH_DisplayInstance` - Displays Rhino instance-reference geometry without converting it into ordinary geometry.
- `GH_DisplayVector` - Displays vector content through the custom render pipeline.
- `GH_GlobalDashSetting` - Singleton editable component for global dash-pattern settings.
- `ListItemState` - Internal state object for dash-setting list UI.
- `GH_VectorDisplaySetting` - Component that creates vector display settings.

## GHComponents.GeometryCode

- `GH_CreateGeometry` - Component that creates geometry-code entries.
- `GH_CreateNewGeometryCodeFile` - Component that creates a new geometry-code file.
- `GH_DeleteGeometryCode` - Component that deletes geometry-code entries.
- `GH_GeometryCodeAbstract` - Base component for geometry-code file workflows.
- `GH_LoadGeometry` - Singleton component that loads geometry-code data from file.
- `GH_SaveGeometryCode` - Component that saves geometry-code data.
- `GH_SelectGeometry` - Component that selects geometry from loaded geometry-code data.

## GHComponents.Process

- `GH_AddEndPath` - Path utility component that extends paths with end segments.
- `GH_DrawPath` - Component that creates or displays path geometry.
- `GH_FixedPivotRotationAction` - Component that creates a fixed-pivot rotation geometry action.
- `GH_GeometryActionAbstract` - Base component for geometry action components.
- `GH_GeometryAnimation` - Component that evaluates geometry animation actions.
- `GH_IterativeOffset` - Component for iterative curve or geometry offset workflows.
- `GH_LinkPath` - Path utility component that links path segments.
- `GH_MovingPivotRotationAction` - Component that creates moving-pivot rotation actions.
- `GH_PathEditAbstract` - Base component for path editing components.
- `GH_PlaneToPlaneAction` - Component that creates plane-to-plane transformation actions.
- `GH_TranslationAction` - Component that creates translation actions.

## GHComponents.Timeline

- `GH_ActiveTimeline` - Component that evaluates active timeline intervals.
- `GH_ActiveCircularTimeline` - Component that evaluates active ranges on a looping or circular timeline.
- `GH_ActiveTimelineZone` - Component that evaluates active zones within a timeline.
- `GH_CreateTimeline` - Component that creates a timeline from time values.
- `GH_CreateTimelineByAccumulatedTime` - Component that creates a timeline by accumulating durations.
- `GH_IntervalRange` - Component that computes interval ranges.
- `GH_RedefineTimeline` - Component that remaps or redefines a timeline.
- `GH_SegmentiseTimeslotLinear` - Component that divides timeslots with linear behaviour.
- `GH_SegmentiseTimeslotNonlinear` - Component that divides timeslots with non-linear behaviour.
- `GH_ShiftTimeline` - Component that shifts a timeline in time.
- `GH_TimeSlotChannel_IN` - Component that publishes a global timeline value to a tag channel.
- `GH_TimeSlotChannel_OUT` - Component that reads a global timeline value from a tag channel.
- `GH_TimelineAbstract` - Base component for timeline components with shared UI text.
- `GH_TimelineMapper` - Internal component for mapping timeline data.
- `GH_IsActiveByTimeline` - Component that returns whether a time value is active within a timeline.

## GHComponents.Util.AnimationCompile

- `GH_AnimationCompile` - Editable component that compiles rendered frames into a movie.
- `GH_AnimationSetting` - Component that creates animation compile settings.
- `GH_AnimationTimer` - Editable timer component that publishes a global animation time value.
- `GH_MirrorFrames` - Component that mirrors generated animation frames.
- `GH_RenderController` - Editable render controller that renders frames based on a time slot value.
- `GH_RenderSetting` - Editable component that creates render settings.
- `GH_TimeSlotChannel_Abstract` - Base tag-channel component for timeline double values.
- `GH_ViewCaptureToFile` - Editable component that captures the active viewport to an image file.

## GHComponents.Util

- `GH_CreateLayers` - Variable-parameter component that creates nested Rhino layers.
- `GH_DashCurve` - Component that creates dashed curve display data.
- `GH_Easing` - Component that applies easing functions to timeline values.
- `GH_GetSequentialGeometryDataFromLayer` - Component that collects geometry from sequentially named layers.
- `GH_MatchList` - Variable-output component that splits a list into matched branches.
- `GH_RemapTtoParameter` - Component that remaps normalised time to a parameter range.
- `GH_Silhouette` - Component for silhouette extraction or display.
- `GH_VisiableGeometry` - Component that filters or returns visible geometry.

## Geometry.Display

- `AnnotationDisplaySetting` - Settings for screen-facing and plane-oriented text display.
- `CurveDisplay` - Display helper for curve preview data.
- `DashCodeParam` - Collection and tag-channel wrapper for dash-type definitions.
- `DashType` - Dash pattern definition.
- `DisplayAnnotationContent` - Text, location, plane, colour, and clipping-box data for annotation rendering.
- `DisplayContentAbstract<T>` - Generic base for valid, visible display content with colour and clipping bounds.
- `DisplayDefaultColour` - Default display colour constants.
- `DisplayGeometryConduit` - Rhino display conduit that dispatches registered pipelines to pre-draw, post-draw, or foreground stages.
- `DisplayGeometryContent` - Geometry, display mesh, colour, transparency, and clipping data.
- `DisplayGeometryContentCols` - Geometry display content supporting multiple or animated colours.
- `DisplayInstanceContent` - Instance-reference display data with transformed definition geometry and clipping bounds.
- `DisplayVectorContent` - Vector display data used by the vector render pipeline.
- `DisplayUtil` - Shared display utility methods.
- `GeometryRenderMode` - Geometry rendering mode selection.
- `IDisplayContent` - Common validity, visibility, and clipping-box contract for display content.
- `IRenderPipeline` - Non-generic contract used by the display conduit.
- `RenderAnnotationPipeline` - Draws screen-facing and plane-oriented text annotations.
- `RenderGeometryPipeline` - Draws shaded geometry content through the selected render stage.
- `RenderInstancePipeline` - Draws Rhino instance-reference geometry.
- `RenderPipelineAbstract<TContent>` - Generic collection and rendering base for display pipelines.
- `RenderStage` - Selects pre-draw, post-draw, or foreground rendering.
- `RenderVectorPipeline` - Draws vector display content.
- `VectorRenderMode` - Vector rendering mode selection.
- `VectorDisplaySetting` - Settings object for vector display appearance.

## Geometry.Processing

- `GeometryActionAbstract` - Base geometry action evaluated through animation time.
- `TransformTSAction` - Base geometry action driven by a transformation over time.
- `GeometryAnimation` - Geometry animation sequence that evaluates actions.
- `GeometryAnimationPipeline` - Pipeline that coordinates geometry animation content and actions.
- `GeometryContent` - Geometry state container tracking source and current geometry.
- `GeometryFixedRotation` - Geometry action for fixed-pivot rotation.
- `GeometryFromPlaneToPlane` - Geometry action transforming from one plane to another.
- `GeometryMovingPivotRotation` - Geometry action for rotation with moving pivot behaviour.
- `GeometryTranslation` - Geometry action for translation over time.
- `GeometryUtil` - Shared geometry helper methods.
- `PathUtil` - Path construction and curve utility methods.

## Util.AnimationCompile

- `AnimationCompileUtil` - Utility that compiles image frames into movie output.
- `AnimationCompileWindow` - Eto settings dialog for animation compile options.
- `AnimationSetting` - Data object for animation compile settings.
- `AnimationTimerScheduleUtil` - Helper methods for animation timer schedule values.
- `AnimationTimerWindows` - WinForms settings window for animation timer bounds, value, step, and schedule.
- `PreviewCanvas` - Preview canvas used by compile/render setting windows.
- `RenderSetting` - Data object for viewport render settings.
- `RenderSettingWindow` - Eto settings dialog for viewport render options.
- `RenderUtil` - Utility for viewport capture, bitmap mirroring, and image output.

## Util.IO

- `CameraJson` - JSON helpers for camera parameter serialization.
- `CodeParameters<T>` - Generic base model for named code collections.
- `ColourCodeIO` - Colour-code file read/write helpers.
- `ColourCodeParameters` - Code-parameter collection specialised for colours.
- `ColourCodeUtil` - Colour-code serialization and conversion utilities.
- `DataUtil` - Internal data conversion helpers for Grasshopper data structures.
- `GeometryCodeIO` - Geometry-code file read/write helpers.
- `GeometryCodeParameters` - Code-parameter collection specialised for geometry data.
- `GeometryCodeUtil` - Geometry JSON serialization and deserialization utilities.
- `GeometryDataPair` - Pairing of geometry and associated metadata for geometry-code storage.
- `JsonRead` - Generic JSON read helper.
- `JsonWrite` - Generic JSON write helper.
- `ProjectAppManager` - Project/application path and data-location helper.
- `VersionControl` - Version metadata helper for saved project data.

# Obsolete Types

The following types remain in the project for compatibility or migration. They should not be used for new development.

## Obsolete Control.Camera

- `CameraSetting` - Superseded camera settings container.
- `CameraTransform_Old` - Rectangle-based camera transformation implementation replaced by `CameraTransform`.
- `CM_CamAToCamB_Old` - Previous camera interpolation implementation replaced by `CM_CamAToCamB`.

## Obsolete Control.Timeline

- `RemoteTime` - Previous remote timeline channel replaced by `TimeSlotTagChannel`.

## Obsolete Geometry.Display

- `DisplayGeometry` - Previous geometry display container replaced by display content and render pipelines.
- `DisplayGeometryCols` - Previous multi-colour geometry display container.
- `VectorDisplay` - Previous vector data model replaced by `DisplayVectorContent` and `RenderVectorPipeline`.

## Obsolete GHComponents.ColourCode

- `GH_ColourCode_Old` - Previous colour-code loading component replaced by `GH_LoadColourCode`.
- `GH_SaveColourCode_old` - Previous colour-code saving component replaced by `GH_SaveColourCode`.
- `GH_SelectedColour_old` - Previous colour selection component replaced by `GH_SelectedColour`.

## Obsolete GHComponents.Display

- `GH_DisplayGeometry_Old` - Previous standard Grasshopper geometry preview component.
- `GH_DisplayGeometryCols_Old` - Previous multi-colour geometry preview component.
- `GH_DisplayGeometryWire_Old` - Previous geometry wire preview component.
- `GH_VectorCurveDisplay` - Previous curve-vector preview component.
- `GH_VectorDisplay` - Previous single-vector preview component.
- `GH_VectorsDisplay` - Previous multi-vector preview component.

## Obsolete GHComponents.GeometryCode

- `GH_LoadGeometry_Old` - Previous geometry-code loading component replaced by `GH_LoadGeometry`.
- `GH_SaveGeometry_Old` - Previous geometry-code saving component replaced by `GH_SaveGeometryCode`.

## Obsolete GHComponents.Process

- `GH_GeometryAnimation_Old` - Previous geometry animation evaluator replaced by `GH_GeometryAnimation` and the `GeometryContent` pipeline.

## Obsolete GHComponents.Timeline

- `GH_IntervalRange_old` - Previous interval-range component replaced by `GH_IntervalRange`.
- `GH_RemoteTimeSpotAbstract` - Previous base class for remote timeline components.
- `GH_RemoteTimeSpot_IN` - Previous remote timeline publisher replaced by `GH_TimeSlotChannel_IN`.
- `GH_RemoteTimeSpot_OUT` - Previous remote timeline reader replaced by `GH_TimeSlotChannel_OUT`.

## Obsolete GHComponents.Util

- `GH_DashCurve_OLD` - Previous dash-curve component replaced by `GH_DashCurve`.
- `GH_DelGeometry` - Previous geometry database deletion component.

## Obsolete Util.IO

- `GeometryData` - Previous serialized geometry record replaced by `GeometryDataPair` and `GeometryCodeParameters`.
