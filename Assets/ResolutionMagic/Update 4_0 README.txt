Resolution Magic 2D 4.0 update important note

The Resolution Magic 4.0 update changes fundamentally how the asset works. The camera resizing/matching to resolution/ratio is now both more accurate and instantaneous instead of requiring a short period of zooming.
Due to this change, if you update the asset in a project already setup with a previous version of the asset, you may need to change a few things to ensure your project still works correctly with this asset.
If you are unsure, read the documentation before upgrading.



Version 3.0 Note
Version 3.0 of Resolution Magic 2D removes all UI content and functionality. If your project relies on this functionality  you should revert back to the previous version.

Why was the UI content removed?
When Resolution Magic 2D was first created, Unity's UI implementatin was very poor, and so some basic UI functionality was included in Resolution Magic 2D since UI is dependent on screen resolution.
In the time since this asset was created, Unity's UI implementation has been improved and expanded dramatically to the point where the functionality in this asset is pointless.

Note: Resolution Magic 2D does not interact with Unity's UI, so your UI will work the same regardless of what Resolution Magic does to your camera. In other words, this asset plays nice with Unity's UI.