using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlexibleGrid : LayoutGroup
{
    public int columns;
    public float spacing;
    public float PreferredHeight { get; private set; }
    public RectTransform parentContainer;
    public float minParentContainerSize;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        float parentWidth = rectTransform.rect.width - padding.left - padding.right - spacing * (columns - 1);
        float widthPerChild = parentWidth / columns;

        for(int i = 0; i < rectChildren.Count; i++)
        {
            int row = i / columns;
            RectTransform child = rectChildren[i];
            SetChildAlongAxis(child, 0, (i % columns) * widthPerChild + padding.left + spacing * (i % columns));
            float y = row * widthPerChild + padding.top + spacing * row;
            SetChildAlongAxis(child, 1, y);
            child.sizeDelta = Vector2.one * widthPerChild;
        }

        int maxRow = rectChildren.Count / columns;
        float maxY = maxRow * widthPerChild + padding.top + spacing * maxRow;
        if(parentContainer)
        {
            parentContainer.sizeDelta = new Vector2(parentContainer.sizeDelta.x, Mathf.Max(maxY, minParentContainerSize));
        }
    }

    public override void CalculateLayoutInputVertical()
    {
        CalculateLayoutInputHorizontal();
    }

    public override void SetLayoutHorizontal()
    {
        
    }

    public override void SetLayoutVertical()
    {
        
    }
}
