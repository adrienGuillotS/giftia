using Corel.Interop.VGCore;

namespace DesignerSuite.Core.Utilities
{
    public static class WrappingTextHelper
    {
        public static Shape ConvertParaToArtistic(Shape paraShape)
        {
            if (paraShape == null ||
                paraShape.Type != cdrShapeType.cdrTextShape ||
                paraShape.Text.Type != cdrTextType.cdrParagraphText)
                throw new ArgumentException("Expect a paragraph text shape.");

            // Converts in-place; keeps current visual line breaks
            paraShape.Text.ConvertToArtistic();
            return paraShape;
        }


        public static Shape CreateAutoFittedParagraphText(Document activeDoc, double x, double y, double maxWidth, double maxHeight, string text, string fontName, float initialFontSize)
        {
            try
            {
                // Create initial paragraph frame
                Shape textShape = activeDoc.ActiveLayer.CreateParagraphText(
                    x,
                    y,
                    x + maxWidth,
                    y - maxHeight,
                    text
                );

                textShape.Text.Story.Font = fontName;
                textShape.Text.Story.Size = initialFontSize;

                float currentFontSize = initialFontSize;
                const float MIN_FONT_SIZE = 6.0f;
                const float FONT_REDUCTION_STEP = 0.5f;

                // Reduce font size while it overflows
                while (textShape.Text.Overflow && currentFontSize > MIN_FONT_SIZE)
                {
                    currentFontSize -= FONT_REDUCTION_STEP;
                    textShape.Text.Story.Size = currentFontSize;
                }

                // If still overflowing, attempt to auto-fit
                if (textShape.Text.Overflow)
                {
                    textShape.Text.FitTextToFrame();
                }

                double actualHeight = CalculateFallbackTextHeight(textShape);
                if (actualHeight < maxHeight)
                {
                    // Recreate text shape with updated frame height
                    textShape.Delete();

                    double newY2 = y - actualHeight;
                    textShape = activeDoc.ActiveLayer.CreateParagraphText(
                        x,
                        y,
                        x + maxWidth,
                        newY2,
                        text
                    );

                    textShape.Text.Story.Font = fontName;
                    textShape.Text.Story.Size = currentFontSize;

                    // Optionally re-fit if needed
                    if (textShape.Text.Overflow)
                        textShape.Text.FitTextToFrame();
                }



                return textShape;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating paragraph text with auto-fit: {ex.Message}", ex);
            }
        }

        private static double CalculateFallbackTextHeight(Shape sh)
        {
            TextRange txtRange = sh.Text.Story;

            // Get font size in points
            double fontSize = txtRange.Size;

            // Calculate line spacing
            double lineSpacing = fontSize * 1.2; // Default 120% spacing

            if (txtRange.LineSpacingType == cdrLineSpacingType.cdrPointLineSpacing)
            {
                lineSpacing = txtRange.LineSpacing;
            }
            else if (txtRange.LineSpacingType == cdrLineSpacingType.cdrPercentOfCharacterHeightLineSpacing)
            {
                lineSpacing = fontSize * (txtRange.LineSpacing / 100.0);
            }

            // Get number of lines (considering wrapped text)
            int lineCount = txtRange.Lines.Count;

            // Convert to millimeters (1 point = 0.352777778 mm)
            return lineCount * lineSpacing * 0.352777778;
        }
    }
}
