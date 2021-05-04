using UnityEngine;

namespace TexDrawLib
{
    /// Interface that implemented for any TEXDraw component
    public interface ITEXDraw
    {
        /// Main string that used for rendering
        string text { get; set; }

        /// Default color that used for rendering
        Color color { get; set; }

        /// Local scaling amount for all characters
        float size { get; set; }

        int fontIndex { get; set; }

        /// Local scaling amount for all characters
        Vector2 alignment { get; set; }

        /// Mode for wrapping
        Wrapping autoWrap { get; set; }

        /// Mode for fitting
        Fitting autoFit { get; set; }

        /// Mode for filling
        Filling autoFill { get; set; }

        /// Access for internal DrawingContext
        DrawingContext drawingContext { get; }

        /// Access for internal DrawingParams
        DrawingParams drawingParams { get; }

        /// Access for internal Cached Preference
        TEXPreference preference { get; }

        /// Helper to push data into DrawingParams
        void GenerateParam();

        /// Will Trigger reparsing
        void SetTextDirty();

        /// Trigger reparsing and rerender on next Update
        void SetTextDirty(bool forceRedraw);

        /// Trigger supplement (and mesh effects) update
        void SetSupplementDirty();
    }
}
