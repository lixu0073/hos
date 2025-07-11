using UnityEngine;

namespace Hospital
{
    public class InsideScreenChecker : MonoBehaviour
    {
        // If viewport is the whole screen, keep it as null
        public static bool IsRectTransformsOverlap(Camera cam, RectTransform itemToFit, RectTransform viewport = null, bool fullInsideCheck = true)
        {
            Vector2 viewportMinCorner;
            Vector2 viewportMaxCorner;

            if (viewport != null)
            {               
                // Get world corners of our rect
                Vector3[] v_wcorners = new Vector3[4];
                viewport.GetWorldCorners(v_wcorners); // Bottom left, top left, top right, bottom right

                // Now the rect is relative to the bottom left corner of screen - CV: check if really needed when applyed
                viewportMinCorner = cam.WorldToScreenPoint(v_wcorners[0]);
                viewportMaxCorner = cam.WorldToScreenPoint(v_wcorners[2]);
            }
            else
            {
                // Just use the screen as the viewport
                viewportMinCorner = Vector2.zero;
                viewportMaxCorner = new Vector2(Screen.width, Screen.height);
            }

            // Give 1 pixel border to avoid numeric issues
            viewportMinCorner += Vector2.one;
            viewportMaxCorner -= Vector2.one;

            // Get the itemToFit corners relative to screen
            Vector3[] e_wcorners = new Vector3[4];
            itemToFit.GetWorldCorners(e_wcorners);

            Vector2 item_minCorner = e_wcorners[0];
            Vector2 item_maxCorner = e_wcorners[2];

            if ((item_minCorner.x > viewportMaxCorner.x) || // Completelly outside (to the right)
                (item_minCorner.y > viewportMaxCorner.y) || // Completelly outside (is above)
                (item_maxCorner.x < viewportMinCorner.x) || // Completelly outside (to the left)
                (item_maxCorner.y < viewportMinCorner.y))   // Completelly outside (is below)
            {
                return false;
            }

            // Check if itemToFit is completely inside
            Vector2 minDif = viewportMinCorner - item_minCorner;
            Vector2 maxDif = viewportMaxCorner - item_maxCorner;
            if (fullInsideCheck)
                return minDif.x < 0 && minDif.y < 0 && maxDif.x > 0 && maxDif.y > 0; // Whether it is completely inside or not
            else
                return true; // It's inside (at least partially)
        }
    }

}
