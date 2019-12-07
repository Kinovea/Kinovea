
Kinovea 0.9.1 - 2019-12.

Kinovea is an open source solution for capturing, studying and annotating videos.
Use it for sport coaching, performance analysis, podiatrics, animation commenting, ergonomics engineering and more...


System Requirements:
--------------------
- CPU : 1 Ghz
- RAM : 256 MB
- Disk Space : 70 MB.
- Screen Resolution : 1024x600 pixels.
- OS : Microsoft Windows 7, 8, 8.1, 10. 
- Framework .NET framework 4.8.


License:
--------
Please see license.txt for details. In a nutshell:
- Source code : GPL v2.
- Graphics : CC-BY-SA.
- Manual : CC-BY-SA.
- Video Tutorials : CC-BY-SA.


Communication channels:
--------
- Web: http://www.kinovea.org
- Mail: asso@kinovea.org
- Videos: http://www.youtube.com/user/kinovea
- Twitter: @Kinovea
- Bugs: http://www.kinovea.org/bugs


Changelog:
----------

0.9.1 - 2019-12.
    Added - Farsi locale.
    Added - Capture: audio trigger.
    Added - Capture: stop recording by time.
    Added - Capture: run command after recording ends.
    Added - Capture: support for recording to uncompressed files.
    Added - Capture: line scan mode for Basler and IDS modules.
    Added - Playback: replay folder observers, auto load and play any new file created in a folder.
    Added - Playback: import numbered image sequences as videos.
    Added - Playback: support for demosaicing (debayering).
    Added - Playback: ability to mark a time as the time origin.
    Added - Annotations: clock tool.
    Added - Annotations: data importer for OpenPose keypoints.
    Added - Annotations: data importer for SRT subtitles.
    Improved - Capture: high performance mode for delayed recording.
    Improved - Capture: the delay value can now be entered manually.
    Improved - Capture: option to ignore the file overwrite warning.
    Improved - Capture: usage of editable textboxes for camera properties values.
    Improved - Capture: IDS module now supports "Sensor Raw 8", "Pixel clock" and "Gain boost" properties.
    Improved - Capture: IDS module configuration dialog can import external parameter files.
    Improved - Capture: IDS module was updated to uEye 4.92.3.
    Improved - Capture: Basler module now shows "Resulting framerate".
    Improved - Capture: Basler module was updated to Pylon 6.0.
    Improved - Capture: option to adjust the threshold and replacement framerate for high speed capture.
    Improved - Capture: display of percentage of "load" for performance feedback.
    Improved - Capture: camera simulator has more options for stream format, image size and framerate.
    Improved - Playback: the default timecode format is easier to read and more standard conformant.
    Improved - Playback: use of 0-based numbering when showing the frame number.
    Improved - Playback: more file-level operations available in the main context menu.
    Improved - Playback: only preload a small number of key images when loading KVA.
    Improved - Annotations: the stopwatch now has copy & paste support and uses the common configuration dialog.
    Improved - Annotations: the stopwatch has more visibility options.
    Improved - Annotations: point markers can now show the distance to the origin of the coordinate system.
    Improved - Annotations: custom tools now supports combinations of options per primitive and hidden options.
    Improved - Annotations: the coordinate system menu is now a proper toggle.
    Improved - Folder selection dialogs were replaced with more usable ones.
    Improved - Kinovea instances can have custom names, defaults to sequential numbers, and can have their own preferences.
    Improved - Loading of the user interface after reboot was improved.
    Fixed - Files with negative start time could not be read.
    Fixed - Support of login/password in IP camera module was broken.
    Fixed - The format converter for KVA 1.5 format (Kinovea 0.8.15) was incomplete.
    Fixed - A missing DLL was causing a crash when using the camera calibration.
    Fixed - When using the Forget custom settings function, profile files from IDS camera weren't deleted.
    Fixed - An error could happen when a zip was clicked in the directory explorer.
    Fixed - In some cases only the top left of the image was visible.
    Fixed - Rafale export dialog could crash.
    Fixed - Resizing screens and using the delay slider caused flashing and flickering issues.
    Fixed - The style and behavior of the record button on the dual capture controls was broken.
    Fixed - The timecode format showed a wrong value when the fractional part rounded up to 100 hundredth of a second.
    Fixed - The heuristic to load videos or cameras into existing screens was not consistent.
    Fixed - In the IDS configuration dialog, auto-gain and auto-exposure were always disabled.
    Fixed - In the Basler module, gain property could be disabled even if the camera supported it.
    Fixed - The aspect ratio of thumbnails for key images was wrong.
    Fixed - The coordinates of points and circle was wrong when the coordinate system itself was tracked.
    Fixed - Dual export was broken when the total image width was not a multiple of 4.
    Removed - Capture: Removed delay compositing framework (e.g: quadrants).
    Removed - Capture: Ability to record live changes in delay value. The delay is fixed at the start of the recording.
    Removed - Replaced capture history sessions by a simple list of the last captured files.


0.8.27 - October 2018.
    Added - Image rotation and detection of image rotation from video metadata.
    Added - Cut, Copy and paste of drawings.
    Added - Paste image from the clipboard into an image object.
    Added - Middle mouse button to pan the image and move objects.
    Added - Set the selected drawing style as the default style for the corresponding tool.
    Added - Debug mode for custom tools.
    Added - Showing the name of the object in the measurement mini label for lines and arrows, markers, circles, trajectories.
    Added - Curve tool.
    Added - Rectangle tool.
    Added - Central sacral vertical line tool.
    Improved - Simplification of the mosaic feature and delay modes in the Capture screen.
    Improved - Capture screen now supports mirroring of the image.
    Improved - Better handling of timestamps in some videos.
    Improved - Look of information bar at the top of the playback screen.
    Improved - Style picking controls now show the currently selected option.
    Improved - Look of arrows and squiggly lines.
    Improved - Use cleaner precision cursor.
    Improved - Use the precision cursor during object modification instead of the closed hand.
    Improved - Use stamp-like cursors for some tools.
    Improved - Circle tool now supports perspective aware rendering and measurement label for center, radius, diameter and circumference.
    Improved - Pencil tool now uses a precision 1px line during drawing.
    Improved - The grid tool can now be toggled between flat or perspective grid from the style picker.
    Improved - Text objects are now initialized with their name.
    Improved - The editing mode of text objects now looks similar to the final mode.
    Improved - Text objects can now have an arrow extending from the object and pointing to something.
    Improved - Look of angle and goniometer tool.
    Improved - Support dash lines for pencil, circle and rectangle objects.
    Improved - Better look and style of mini labels.
    Improved - Custom tools format v1.1, various enhancements.
    Improved - Redesign of the Archery tool.
    Improved - Updated Basler capture module to Pylon 5.1 API.
    Improved - Basler capture module now has an option to bypass color conversion of raw stream formats.
    Fixed - The coordinate system origin was not reloaded correctly.
    Fixed - Text objects could not be edited when reloaded from KVA.
    Fixed - Goniometer tool was showing the wrong angle.
    Fixed - On the image drawing, the image content and bounding box were not saved in the KVA.
    Fixed - Some mini label positions were lost when saving to KVA.
    Fixed - For some drawings certain changes in style were not triggering the save dialog.
    Fixed - The size of the pencil cursor was not updated when zoomed in.
    Fixed - The look of the end points of pencil objects was broken.
    Fixed - The image was not refreshed after the user changed a custom option in a custom drawing.
    Fixed - The capture screen crashed when showing the coordinates of a cross marker object.
    Removed - The option to use the camera frame as a signal to display and push to delay buffer.


0.8.26 - November 2017.
	Added - Angular kinematics analysis dialog.
	Added - Angle-angle diagrams analysis dialog.
	Added - Trackable drawings, including custom tools, can now be used as sources for trajectory analysis.
	Added - Support for IDS cameras in the live capture module.
	Added - Support for recording modes. Restores the possibility to record delayed video.
	Added - Option to globally disable trajectory filtering.
	Improved - Allow multiple instances of Kinovea to run at the same time.
	Improved - Simplified opacity and fading mechanics.
	Improved - Angle tool options for signed/unsigned, rotation direction and supplementary angle.
	Improved - Trajectory analysis now supports multiple sources at the same time.
	Improved - Trajectory analysis now supports relative time and normalized time.
	Improved - Updated Basler capture module to Pylon 5 API.
	Improved - Allow larger memory buffers in the capture module based on bitness and available memory.
	Fixed - Magnifier and mirror were not exported correctly during video export.
	Fixed - Slow motion option was not visible during video export.
	Fixed - The query part of the URL in HTTP capture module was discarded.
	Fixed - Trajectories consisting of exactly 10 samples were improperly processed.
	Fixed - Hotkey for live delay was broken.
	Fixed - Text export for trajectories was broken.


0.8.25 - August 14, 2016 - Intermediate version.
    Added - Arabic locale.
    Added - Delay mosaic framework: multiple views of the delay buffer at different times or refresh rates.
    Added - A default tracking profile, configurable from the preferences.
    Added - x64 build chain.
    Added - The concept of name for drawings.
    Added - The video framerate can be overriden.
    Added - A new file naming framework with context variables, left/right separation and unification of freetext and pattern modes.
    Added - A button to go in and out of full screen from the thumbnail viewer.
    Added - Custom length unit and symbol.
    Improved - Thumbnails can now display more details about the file and the list of details is configurable.
    Improved - The player now defaults to interactive frame tracker. 
    Improved - The current directory is watched for file events and the file explorer updated.
    Improved - The top-level menus were changed for role clarification.
    Improved - Custom tools now spawn under the mouse pointer rather than at fixed position.
    Improved - When tracking the coordinate system the exported coordinates are relative to the system's origin of the current frame.
    Improved - The order of export for drawings has been reversed.
    Improved - All points and lines are now exported to spreadsheet, regardless of whether they have the Display Coordinate UI option.
    Improved - CSV export now uses system-wide list separator.
    Improved - Updated FFMpeg.
    Improved - Updated OpenCV.
    Improved - Updated Basler library.
    Fixed - Angle tool could vanish into oblivion.
    Fixed - restoration of format selection for video capture.
    Fixed - Undo of the deletion of track objects was not working correctly.
    Fixed - Do not show keyframe titles on track if track display is set to none.


0.8.24 - March 31, 2015 - Intermediate version, for testing.
    Added - New capture pipeline.
    Added - Support for selection of stream formats.
    Added - Support for configuration of exposure, gain and focus directly in the camera configuration window.
    Added - Support for Basler cameras.
    Added - Capture history panel and functionality.
    Added - Direct recording of the MJPEG streams without transcoding.
    Added - Display of received framerate, data rate and drop counter.
    Added - Display synchronization strategy and decoupled preview framerate.
    Added - Menu to forget camera configuration.
    Added - Vendor specific camera icons.
    Added - Test grid tool for capture screen.
    Added - Support for Logitech specific exposure property.
    Added - New time code "Total microseconds".
    Improved - Performance of camera simulator.
    Improved - Bikefit tool now has an option to lock segment length.
    Improved - No longer require a camera reconnection if we don't change the stream format.
    Fixed - Several memory issues that were putting pressure on the Garbage Collector.
    Fixed - Time computation in data analysis.
    Fixed - Visibility of HumanModel2 in Capture screen.
    Fixed - Prevent save method to overwrite an open file.
    Fixed - Interference between keyboard shortcuts and renaming captured files or capture target.
    Removed - Adjustment filters.
    Removed - Capture video file format selector (Forced to MP4).


0.8.23 - October 26, 2014 - Intermediate version, for testing.
    Added - Distortion grid tool for manual calibration of lens distortion.
    Added - Camera calibration dialog to load, save or locally compute lens distortion parameters.
    Added - Compensation of lens distortion in measurements and coordinate system visualization.
    Added - Scatter plot - data analysis window for point markers.
    Added - Trajectory plot - data analysis window for trajectories.
    Added - Polyline tool.
    Added - Squiggly-line shape style for line and polyline.
    Added - New arrow tools. Built-in style variations over line and polyline.
    Improved - Better representation of the coordinate system in perspective.
    Improved - Custom tools are now individually configurable in the style presets window.
    Improved - Custom tools are relocated within the toolbar where most appropriate.
    Improved - Presentation of the style presets window.
    Fixed - Angle to horizontal tool.
    Fixed - Clearing of tracks and chronos during video load-over.


0.8.22 - June 19th 2014 - Intermediate version, for testing.
    Added - Japanese locale.
    Added - Serbian (Cyrillic) locale.
    Added - Macedonian locale.
    Added - Auto-save and crash recovery mechanism.
    Added - Coordinate system for plane calibration.
    Added - Velocities, accelerations and coordinates display for trajectories.
    Added - Angular kinematics through best fit circle of trajectory.    
    Added - Ability to drag and drop a KVA file from the Windows Explorer to an open playback or capture screen.
    Added - KSV files (Kinovea Synthetic Video).
    Added - Camera simulator camera type.
    Improved - All kinematics coordinates use subpixel accuracy.
    Improved - Trajectory tracking parameters can be changed manually.
    Improved - Subpixel refinement for trajectory tracking.
    Improved - Data filtering on kinematics data.
    Improved - Calibration is automatically updated when the calibration grid is modified.
    Improved - Capture framerate (high speed video) is saved to KVA.
    Improved - Tracking timelines of trackable drawings are saved to KVA.
    Improved - Bitmap and SVG drawings are saved to KVA.
    Improved - Usage of real-time based synchronization for all synchronization logic.
    Improved - During synchronization, each video has its own marker in the common timeline.
    Improved - F9 hotkey to toggle synchronization merge.
    Improved - During saving of video, usage of constant quantization and lower quantization value to increase quality.
    Improved - In File browser, context menu to launch or delete video file.
    Fixed - Issues with grabbing for large image size or small screen.
    Fixed - Various issues with synchronization of two videos.
    Fixed - Various issues with hot keys for dual player.
    Fixed - Various issues with dual saving and dual snapshot.
    Fixed - Stopwatch was displaying value for 1 frame more than necessary.
    Fixed - Angle values were inverted when using grid calibration.
    Fixed - Various issues related to image size adaptation when importing KVA on different video.
    Fixed - Issues with updating high speed video capture FPS and speed percentage.
    Fixed - Various issues with undo/redo.
    Fixed - Spotlight and autonumbers were not taken into account to detect if KVA had changed.
    Removed - Support for reading embedded KVA inside video.


0.8.21 - October 19th 2013 - Intermediate version, for testing.
    Added - Catalan locale.
    Added - Serbian (Latin) locale.
    Added - Tab and explorer panel for cameras.
    Added - Capture : common controls.
    Added - Playback screen: SVG as first class files.
    Added - Playback screen: copy image to clipboard.
    Added - Playback screen: 10% pagination.
    Improved - Keyboard shortcuts can be customized.
    Improved - File explorer: list of files is vertical.
    Improved - File explorer: current video's directory is added as a temporary shortcut.
    Improved - Explorer panel: locate in Windows explorer menu.
    Improved - Capture screen rewritten from scratch.
    Improved - Custom tools: custom colors.
    Improved - Custom tools: optional constraints.
    Improved - Custom tools: computed points.
    Improved - Custom tools: custom tracking algorithm.

0.8.20 - January 12th 2013 - Intermediate version, for testing.
    Added - Russian locale.
    Added - Plane calibration and grid as coordinate system.
    Added - Trackability for grids.
    Added - Millimeter length unit.
    Improved - Update to Danish and Dutch locales.
    Improved - Hit testing and tool manipulation when image is reduced.
    Improved - Capture file names can contain embedded directory.
    Improved - Natural sorting for custom tool list.
    Improved - Grid divisions option in the configuration dialog.
    Fixed - bug 291 - Custom tools position not preserved when saving and reopening.
    Fixed - Tool color not preserved on some computers.
    Fixed - Crash when using arrow keys on thumbnails explorer.

0.8.19 - September 30th 2012 - Intermediate version, for testing.
    Added - Spanish user manual.
    Added - Trackability for angles, lines, markers, custom drawing tools, magnifier, coordinate system.
    Added - Custom tools: Vertical and horizontal angles, horizontal distance.
    Improved - Custom tools format: distance display, flippability.
    Improved - User coordinate system directly editable on main image + options for tick marks, grid and tracking.
    Improved - File explorer use natural sorting instead of alphabetical sorting.
    Fixed - bug 287 - Misplacement of drawings when recording video with Capture screen.
    Fixed - bug 277 - Drawing persistence settings is not saved.

0.8.18 - August 20th 2012 - Intermediate version, for testing.
    Added - Czech and Korean locale.
    Added - Spotlight tool.
    Added - Auto numbering tool.
    Added - Generic Posture tools. (Goniometer, Legs, Profile, Bikefit, Posture, archery top view)
    Improved - Separation of line and arrow tools.
    Improved - Shift key to constraint motion to 45° steps, for line, angle and generic posture.
    Improved - Shift key to constraint rectangular plane tool to a square.
    Improved - Shift key to constraint pencil tool to draw horizontal or vertical lines.
    Improved - Display 2 digits after decimal point even for pixel measurements.
    Fixed - Crash when image file cannot be read.
    Fixed - Crash when video cannot be read.
    Fixed - Added .ts, .ts1, .ts2, .avr extensions for the explorer.

0.8.17 - March 04th 2012 - (hg:60a79465530e) - Intermediate version, for testing.
    Added - Danish locale.
    Added - Animated GIF reader.
    Improved - (perfs) Asynchronous decoding with prebuffering.
    Improved - (perfs) Unscaled rendering mechanics.
    Improved - Images files are now read by a special reader that turn them into videos.
    Improved - Use absolute times for trajectory points.
    Improved - Capture - buttons to open the capture directories in Windows Explorer.
    Improved - Transparency tweaking during Dual playback merged.
    Fixed - bug 258 - Crash during explorer browse on OSX Parallels.
    Fixed - bug 269 - Magnifier painted at the wrong place during save.
    Fixed - bug that prevented network camera to work properly.
    Fixed - bug that missed to ask for saving KVA when closing from the main menu.
    Removed - Save video with KVA muxed in.

0.8.16 - August 10th 2011 - (r515) - Intermediate version, for testing.
    Added - Full screen feature.
    Improved - Grid and plane are now first-class drawings.
    Improved - KVA loading merges into the existing key images.
    Improved - Option to display time ticks on tracks.
    Improved - More explicit save dialog.
    Improved - Update to Greek and German locales.
    Improved - Support for WebM format (FFMpeg).
    Fixed - bug 209 - Some H.264 files cannot be played (FFMpeg).
    Fixed - bug 245 - Synchronization between a high speed clip and a normal one cannot be done.
    Fixed - bug 247 - Cannot use comma as decimal separator when calibrating line length.
    Fixed - bug 248 - Times are truncated instead of rounded.
    Fixed - bug 250 - Recorded files plays too fast.
    Fixed - bug 255 - Comments not saved if the comment box is not closed or reopened manually.
    Removed - "Save video only" option on save dialog.

0.8.15 - May 15th 2011 - (r464) - Stable version.
    Added - Lithuanian, Swedish locale. Updates to Italian, Dutch, Romanian, Turkish, Greek, Finnish, Portuguese and Spanish.
    Improved - Help manual update to English, French and Italian.
    Improved - Capture - Parse URL for inline username:password.
    Improved - Capture - Improved automatic reconnection mechanism.
    Fixed - Issue with image size not being properly switched when changing device.

0.8.14 - April 1st 2011 - (r428) - Release candidate version.
    Added - Capture - Advanced file naming.
    Added - Capture - Support for network cameras.
    Improved - Capture - File format preferences.
    Improved - Capture - Memory buffer preferences.
    Improved - Capture - Access to device property page.
    Improved - Capture - Captured video operation from thumbnail (launch, hide, delete).
    Improved - Toolbar on main window.
    Improved - Reverse angle function for angle drawings.
    Improved - Added shortcuts for capture operations.
    Improved - Shortcuts for Speed and Delay slider primary function is to jump to next 25%.
    Fixed - bug 241 - Exported length for line appears as 0.
    Fixed - Trajectories, chronometers and magnifier were drawn during play even when the option was disabled.
    Fixed - Change of speed was not always effective.
    Fixed - SVG files that use percents value relative to viewbox for width and height.

0.8.13 - March 1st 2011 - (r376) - Intermediate version, for testing.
    Added - Image tool to import images as observational references.
    Improved - Opacity slider for observational references.
    Improved - Capture - Selected configuration for the device is saved.
    Improved - Thumbnails - Image size and analysis data presence are displayed.
    Fixed - bug 236 - Entry missing in Add/Remove Programs.
    Fixed - bug 234 - Some camera refuses to switch configuration.
    Fixed - bug 233 - Crash when opening camera configuration page.

0.8.12 - February 1st 2011 - (r365) - Intermediate version, for testing.
    Added - Coordinates display and export for cross markers.
    Improved - Whole image can be dragged even without zoom, when synchronizing and blending.
    Improved - Capture - Configuration picker for frame size / frame rate.
    Improved - Updates to Dutch and Italian locales.
    Improved - Refresh the explorer tree when a file is created through capture or save.
    Improved - The menu for Observational references is now updated live.
    Fixed - bug 230 - Speed label too small for some videos.
    Fixed - bug 228 - Coordinates origin not saved in meta data.
    Fixed - bug 227 - Crash when using dual export.
    Fixed - bug 216 - Various UI issues with High Dpi settings in Windows 7.
    Fixed - bug 183 - Export to spreadsheet does not always work.

0.8.11 - November 1st 2010 - (r347) - Intermediate version, for testing.
    Added - Delayed display for capture.
    Improved - Marker for default value on speed slider.
    Fixed - Flickering when recording live video.
    Fixed - bug in preference dialog prevented the correct locale to be preselected.

0.8.10 - October 1st 2010 - (r332) - Intermediate version, for testing.
    Added - Circle tool.
    Added - Total milliseconds time representation.
    Added - Toast messages for non disruptive information. (pause, zoom change, etc.)
    Improved - Image numbers as inserts in Overview.
    Improved - Automatic reconnection in Capture screen.
    Fixed - m221 - Misalignments of key images when saving video at extreme slow motion.
    Fixed - m220 - Crash on dual save when merge activated.
    Fixed - m216 - Missing buttons on Save video dialog on Windows 7 at high DPI. (and some other UI glitches)
    Fixed - m214 - Misalignement of key images when saving.

0.8.9 - July 1st 2010  - (r309) - Intermediate version, for testing.
    Added - Capture Screen - frame grabbing and recording.
    Added - Dual Export for images and videos - to create a composite image or video made from both videos.
    Added - Rich text edit for key images comments.
    Improved - Menus icons.
    Improved - Color picker with more colors and a list of recently choosen ones.
    Improved - Observational References - Support for sub directories, to be reflected as sub menus.
    Fixed - m210 - Time Marker Format won't save "Classic + Frame Numbers".
    Fixed - m207 - Comment box malfunction when attempting to save comments.

0.8.8 - June 1st 2010 - (r282) - Intermediate version, for testing only.
    Added - Observational References. SVG drawings as motion guides.
    Improved - Update to FFMpeg libraries. (in their r23012).
    Improved - Overview feature: implementation of scroll to refine number of images directly.
    Improved - Simplification of the upgrade manager.
    Improved - Global option for drawings persistence to be always visible by default.
    Improved - Possibility to set the default persistence to 1 frame.
    Improved - Speed percentage now uses the action timeframe instead of the video timeframe (high-speed cameras).
    Improved - Spreadsheet export support for Key image time, Lines measures and angles.
    Improved - Added .f4v, .mts and .gif to the list of known file formats for the file explorer.
    Fixed - m195 - Seek issues with ASF file.
    Fixed - m194 - Basler AVI file can't be opened. (new FFMpeg)
    Fixed - m192 - Speed measurement does not reflect high-speed camera settings.
    Fixed - m189 - MOV with watermark can't be read. (new FFMpeg)
    Fixed - m188 - Trajectory label is not exported on ODF files.
    Fixed - m185 - Magnifier does not magnify the right part of the image when mirroring.
    Fixed - m184 - WMV from Windows Movie Maker diaporama can't be read. (new FFMpeg)
    Fixed - m165 - WMV files playing incorrectly. (new FFMpeg)
    Fixed - m164 - Magnifier is not exported on video saving.

0.8.7 - May 7th 2010 - (r257) - Stable version.
    Improved - Updates to Norwegian, Finnish, Dutch, Turkish, Romanian, German, Italian, Greek, Chinese.
    Improved - Video save operations are now cancellable.
    Fixed - m187 - Preferred speed unit is not initially selected when reopening.

0.8.6 - April 3rd 2010 (r235) - Release candidate version.
    Improved - Updates to Turkish and Italian.
    Improved - During synchronization, speed change in one video is automatically reported on the other.
    Improved - During synchronization, actions causing a pause on one video also cause one on the other.
    Improved - Automatic tracking is improved both in speed and robustness.
    Fixed - m179 - Synchronisation is lost when one video is forced to slow down.
    Fixed - m178 - Repeat mode is wrong when loading another video in the same screen.
    Fixed - m177 - Keyboard shortcuts not working when file open through command line.
    Fixed - m176 - Key images not visible when saved within a working zone.
    Fixed - m175 - Crash while opening a video.
    Fixed - locale was forced to French when no user preferences, instead of the system locale or English if not supported.

0.8.5 - February 28th 2010 (r211) - Intermediate version, for testing only.
    Added - Localizations : Finnish, Norwegian, Turkish, Greek and updates to Italian, Spanish, Portuguese, Dutch, German, Romanian.
    Added - Distance and Speed display option on Tracks.
    Added - Export Tracks trajectory data to text.
    Improved - new menu to directly track a point from general right click, out of any Cross marker context.
    Improved - menu to quickly access the time code format options.
    Improved - Markers in the frame tracker gutter to see stopwatches and paths.
    Improved - Measure label on lines can now be moved around.
    Fixed - m170 : Double click using the Pencil tool causes the configuration dialog to appear.
    Fixed - m169 : Saving ends up with an error message, even though the saving was ok.
    Fixed - m167 : Drag and drop of a file on the Kinovea.exe file or on a shortcut does not load the video.
    Fixed - m166 : Mirror filter was not carried over during sync superposition.
    Fixed - Key image markers were not updated in Frame Tracker when removing a key image.
    Fixed - Empty player crashed in 2 screens mode when using keyboard shortcuts with impact on both screens.
    Fixed - Added .TOD extension to known video file formats.

0.8.4 - November 24th 2009 (r153) - Intermediate version, for testing only.
    Added - Option to superpose images from each other video during synchronisation.
    Added - Configuration window to set origin of coordinate system for Paths.
    Added - Image files show up in the Explorer and in the Thumbnails Explorer.
    Fixed - crash on video loading for some systems.
    Fixed - sometimes a screen became unstoppable during synchronisation.
    Fixed - bug that sometimes missed to alert the user when closing screen even if he had added Metadata.
    Fixed - crash when adding a drawing on image file opened as video.
    Fixed - bug on StopWatches where the background did not scale up when image size was increased.

0.8.3 - November 11th 2009 (r143) - Intermediate version, for testing only.
    Added - Options in general Preferences to open files forcing image aspect ratio and deinterlacing.
    Fixed - m145 - Crash at startup for Norvegian based systems.
    Fixed - m147 - Memory leak on deinterlace.

0.8.2 - November 1st 2009 (r139) - Intermediate version, for testing only.
    Added - Trajectories (Paths) - new "focused" mode where the trajectory is only visible around the current frame.
    Added - Command line arguments handler.
    Added - Possibility to force the aspect ratio to 16:9 or 4:3 if autodetect is wrong.
    Improved - Tracing of unhandled exceptions in an external file.
    Improved - Cancel button on the "extraction of frames to memory" progress bar.
    Improved - Usability and performance enhancements on common frame tracker during synchronisation.
    Fixed - m140 - Exported coordinate system is wrong (on Y) for trajectories.
    Fixed - m144 - In Dynamic sync, we were using absolute positions instead of positions relative to the working zone.
    Fixed - When we changed key image title through direct edit, the trajectory KeyframesLabels weren't updated.

0.8.1 - August 9th 2009 - (r117) - Intermediate version, for testing only.
    Added - Export - "Paused Video" saving method. Video with longer pauses on Key Images.
    Improved - Export - Simplified the saving dialog.
    Improved - Export - Export to Spreadsheets (ODF, Excel, XHTML) now uses user units (time and length).
    Fixed - m135 - Uncompressed files from VirtualDub cannot be saved again in Kinovea.
    Fixed - m137 - Error on Vista when opening MPG or MOD files.
    Fixed - Selection wasn't imported to memory when using the in/out buttons.

0.8.0 - July 8th 2009 - (r104) - Intermediate version, for testing only.
    Added - Explorer - Shortcuts tab.
    Added - Motion filters - Mosaic mode.
    Added - Motion filters - Reverse selection (backward playback).
    Added - Export - Export to OpenOffice calc, MS-Excel, XHTML.
    Improved - Explorer - Possibility to rename and delete files directly.
    Improved - Explorer - Thumbnails loop between several images from the video.
    Improved - Explorer - Thumbnails keep image ratio and are centered.
    Improved - Playback - CTRL + Up / Down changes speeds by 25% increments.
    Improved - Playback - New timecode format : classic time + frame number combination.
    Improved - Key Images - Default key image title (timecode) is updated live until the user choose an explicit title.
    Improved - Key Images - Possibility to change key image title directly without going through the comment window.
    Improved - Key images - The key images panel stays docked if the user explicitely asked so.
    Improved - Drawings - Line length display and seal option.
    Improved - Chronos - Countdown mode.
    Improved - Preferences - Thumbnail size, explorer tab and splitters positions are saved to prefs.
    Fixed - m133 - Crash in explorer when "Asus EEE Storage" application is installed.
    Fixed - Explorer - Collapsing a node when a subnode was expanded resulted in automatic re-expanding.
    Fixed - Explorer - Scrolling while thumbnails were loading caused error in the thumbnails positions.
    Fixed - Saving - When saving non analysis mode videos a memory leak could make the RAM peak and computer hang.

0.7.10 - February 4th 2009 - (r55) - Stable version.
    Fixed - Selection and saving issues on files with B-frames.

0.7.9 - January 23th 2009 - (r39) - Intermediate version, for testing only.
    Added - Logging system to improve defect fixing.
    Added - Italian user guide.
    Added - Undo emulation for Image adjustment menus.
    Improved - Splash screen now also covers initial language load.
    Improved - When undoing a 'close screen' command, all its key images and drawings are revived.
    Improved - Non square pixels videos are now handled in display and save.
    Improved - Trajectory now have rounded angles.
    Improved - Manipulation handles zones increased in size.
    Fixed - m0109 - A bug could make the application crash at Player initialization.
    Fixed - m0111 - Measured angles show permanently instead of the assigned number of frames
    Fixed - m0108 - Saving a trajectory muxed in the video file could behave improperly.
    Fixed - Status bar update on file list click.
    Fixed - Configuration dialog box are relocated to center of screen if current mouse location make them go outside screen.

0.7.8 - December 18th 2008 - (r24) - Intermediate version, for testing only.
    Added - Direct Zoom.
    Added - Trajectories suggestions with template matching.
    Added - Label Follows and Arrow Follows mode for trajectories.
    Added - Specification of the capture fps for high speed cameras.
    Improved - Video size is not forced to be multiple of 4.
    Improved - Drawings can not be moved outside screen anymore.
    Improved - Shift + Left arrow on first image moves backwards to the end.
    Improved - Explorer configuration is now saved in settings.
    Improved - Hand and cross custom cursor.
    Improved - Warning dialog box if key images data not saved.
    Improved - F11 toggles stretch mode.
    Improved - Keyboard navigation (TAB) on most dialogs.
    Fixed - Launch issue on some configs.
    Fixed - ColorProfile.xml removed on uninstall.
    Fixed - Closing configuration windows with the red cross does a Cancel.
    Removed - PDF Export.

0.7.7 - November 24th 2008 - Intermediate version, for testing only.
    Added - Persistence of drawings. ( =Fading in/out) + Related configurations dialogs + "Go to key image" menu.
    Added - Drawings color & style preconfiguration window. (=right click before drawing)
    Added - Markers for key frames in the navigation bar.
    Improved - Pencil tool is kept active when changing frames. + Escape returns to the hand tool.
    Improved - Pencil line now has rounded caps.
    Improved - Pencil tool cursor is now a colored circle.
    Improved - Pencil tool style picker has more size options.
    Improved - Pencil width and font size (chronos and texts) now scales with image.
    Improved - Grids, chronos and Trajectories now exported on video and diaporama.
    Improved - Configurations dialogs now opens at mouse location instead of center of screen.
    Fixed - Trajectory color is now taken from its parent cross maker, not from default cross marker color.

0.7.6 - November 11th 2008 - Stable version.
    Added - Deinterlace option to remove comb artifacts.
    Fixed - Crash when adding a drawing while playing.
    Fixed - Crash when closing a screen while synchronizing two videos.
    Fixed - Crash when openning a blank screen after having synchronized two videos.

0.7.5 - November 2nd 2008 - Intermediate version, for testing only.
    Added - Polish Localization.
    Added - Reimplemented the 3D Grid with Homography Matrix. Now called 'Perspective Grid'.
    Added - Grids shows on images exports.
    Added - Double click on drawing goes to their configuration dialog.
    Added - Font size of Text Drawings configurable.
    Added - Undo/Redo on Chronometers Add/Delete/Modify and on Trajectory Delete.
    Added - Magnifier zones blocked at image borders.
    Added - Ergonomics timeformats now have 3 significant digits.
    Fixed - If the video has several streams we take the one with most frames in it.
    Fixed - Crash on zero sized Angles.

0.7.4 - October 20th 2008 - Intermediate version, for testing only.
    Added - German localization.
    Added - Portuguese localization.
    Added - Changing Drawings color / style after setup.
    Added - Chronometers added to color profile.
    Added - Changing color and font size of chronometers after setup.
    Added - Associating of a label with each chronometer.
    Added - Chronometer value takes time format into account.
    Added - Import/Export XML for Chronos and Trajectories.
    Added - Image export is now at current display size.
    Added - Chronometers and Trajectories flushed on Image Export.
    Fixed - Can now manipulate Drawings when a grid or plane is shown.
    Fixed - Memory leak in thumbnail panel.
    Fixed - Exported single image takes time format into account.
    Fixed - default bitrate increased if not conclusive.
    Fixed - Crash hapenned when mouse moved while placing a chronometer.
    Fixed - Diaporama Export.
    Fixed - Some FLV made the whole app crash.

0.7.3 - October 2nd 2008 - Intermediate version, for testing only.
    Added - File thumbnails on right pane.
    Added - Trajectory tool (Manual tracking).
    Added - Chronometer tool.
    Added - Magnifier mode.
    Added - Width of line and pencil tool, arrows endings for lines.
    Added - German and Spanish locale.
    Fixed - .mod files were not recognised in file explorer.
    Fixed - Drawing moved to a different location.
    Fixed - File Explorer now starts at Desktop level.

0.7.2 - July 14th 2008 - Stable version.
    Added - User Preferences.
    Added - Mousewheel to browse video.
    Added - Ability to change time markers.
    Added - Dutch locale.
    Fixed - Dynamic Synchronization.
    Fixed - Windows Vista 64 Bits.

0.7.1 - May - 30th 2008 - Intermediate version, for testing only.
    Fixed - Windows Vista 64 Bits.

0.7.0 - May 3rd 2008 - Intermediate version, for testing only.
    Added - Key Images (Add, browse, comments, etc.)
    Added - Drawings on Key Images (Line, angle, text, etc.)
    Added - Export / Import Key Images data between videos.
    Added - Save Working Zone as new video file.
    Added - 3D Plane. Grids interactivity.
    Added - Export analysis to PDF.
    Improved - Use hundredth instead of frame numbers for timecode.
    Added - Various export options for images.

0.6.3 - March 12th 2008 - Hotfix release.
    Added - Display of ChangeLog within the Update Dialog box.
    Fixed - Bug when running Windows Vista without admin rights.
    Fixed - Kinovea logo is now under LAL (Licence Art Libre) license.

0.6.2 - March 08th 2008.
    First public release.
